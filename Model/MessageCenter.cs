using FirewallDemo.Model.Data;
using FirewallDemo.Model.UserManage;
using FirewallDemo.Security;
using FirewallDemo.Utility;
using FirewallDemo.ViewModel.EntityVM;
using HandyControl.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace FirewallDemo.Model;

public class MessageCenter(IServiceProvider serviceProvider) : IPermissionChecker
{
    private readonly IServiceProvider _provider = serviceProvider;

    public Permissions CheckPermission(User user)
    {
        return user.Role switch
        {
            "USER" => Permissions.User,
            "SUPREME" => Permissions.Supreme,
            "ADMIN" => Permissions.Admin,
            "INTERNAL" => Permissions.Internal,
            _ => Permissions.Viewer,
        };
    }

    public bool SetMessageRead(string msgId, User caller)
    {
        try
        {
            var permission = CheckPermission(caller);
            if (permission >= Permissions.Viewer)
            {
                throw new InvalidOperationException("不允许使用仅查看用户进行读取消息操作.");
            }

            using var serviceScope = _provider.CreateScope();
            using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

            var msg = GetMessage(msgId, caller) ?? throw new InvalidOperationException("无效的信息ID");
            msg.Unread = 1;
            dataContext.Messages.Update(msg);
            dataContext.SaveChanges();
            return true;
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Error(ex.Message);
            return false;
        }
    }

    public Message SendMessage(string content, string chatListId, User caller)
    {
        try
        {
            var permission = CheckPermission(caller);
            if (permission >= Permissions.Viewer)
            {
                throw new InvalidOperationException("不允许使用仅查看用户进行发消息操作.");
            }
            //每条信息的ID都独一无二，若ID已存在则重新生成一个
            Message msg = new()
            {
                Unread = 0,
                SenderId = caller.Id,
                ChatId = chatListId,
                Timestamp = DateTime.Now,
                Content = content,
                MsgId = "MSG-" + Guid.NewGuid().ToString()[..12]
            };

            using var serviceScope = _provider.CreateScope();
            using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

            do
            {
                var exists = dataContext.Messages.Where(m => m.MsgId == msg.MsgId).Select(m => m);
                if (exists.Any())
                {
                    msg.MsgId = "MSG-" + Guid.NewGuid().ToString()[..12];
                }
                else
                    break;
            }
            while (true);
            //dataContext.Messages.Add(msg);
            //dataContext.SaveChanges();
            //展示了原生执行SQL增删改查的能力(使用了ADO.NET)
            using (var command = dataContext.Database.GetDbConnection().CreateCommand())
            {
                dataContext.Database.OpenConnection();
                //var transaction = dataContext.Database.BeginTransaction();

                try
                {
                    //command.Transaction = (System.Data.Common.DbTransaction?)transaction;
                    command.CommandText = "INSERT INTO messages(msg_id, chat_id, sender_id, `timestamp`, content, unread) VALUES (@msg_id, @chat_id, @sender_id, @timestamp, @content, @unread);";

                    // 构建ADO.NET SQL指令的各项参数
                    var par1 = command.CreateParameter();
                    par1.ParameterName = "@msg_id"; par1.Value = msg.MsgId;
                    command.Parameters.Add(par1);
                    var par2 = command.CreateParameter();
                    par2.ParameterName = "@sender_id"; par2.Value = msg.SenderId;
                    command.Parameters.Add(par2);
                    var par3 = command.CreateParameter();
                    par3.ParameterName = "@timestamp"; par3.Value = msg.Timestamp;
                    command.Parameters.Add(par3);
                    var par4 = command.CreateParameter();
                    par4.ParameterName = "@chat_id"; par4.Value = msg.ChatId;
                    command.Parameters.Add(par4);
                    var par5 = command.CreateParameter();
                    par5.ParameterName = "@content"; par5.Value = msg.Content;
                    command.Parameters.Add(par5);
                    var par6 = command.CreateParameter();
                    par6.ParameterName = "@unread"; par6.Value = msg.Unread;
                    command.Parameters.Add(par6);
                    command.ExecuteNonQuery();
                    //transaction.Commit();
                }
                catch
                {
                    //transaction.Rollback();
                }
                finally
                {
                    dataContext.Database.CloseConnection();
                }
            }
            return msg;
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Error(ex.Message);
            return new Message();
        }
    }

    public Message? GetMessage(string msgId, User caller)
    {
        try
        {
            var permission = CheckPermission(caller);
            if (permission >= Permissions.Viewer)
            {
                throw new InvalidOperationException("不允许使用仅查看用户进行该操作.");
            }

            using var serviceScope = _provider.CreateScope();
            using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

            var result = dataContext.Messages
                         .Where(c => c.MsgId == msgId)
                         .Select(c => c);
            return result.FirstOrDefault();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Error(ex.Message);
            return null;
        }
    }

    public ChatList? CreateChat(string targetUserId, string role, User caller)
    {
        try
        {
            var permission = CheckPermission(caller);
            if (permission >= Permissions.Viewer)
            {
                throw new InvalidOperationException("不允许使用仅查看用户进行发消息操作.");
            }

            ChatList chatList = new()
            {
                SellerId = role == "seller" ? caller.Id : targetUserId,
                CustomerId = role == "seller" ? targetUserId : caller.Id,
                ChatId = "CT-" + Guid.NewGuid().ToString()[..12]
            };

            using var serviceScope = _provider.CreateScope();
            using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

            do
            {
                var exists = dataContext.ChatLists.Where(m => m.ChatId == chatList.ChatId).Select(m => m);
                if (exists.Any())
                {
                    chatList.ChatId = "CT-" + Guid.NewGuid().ToString()[..12];
                }
                else
                    break;
            }
            while (true);

            dataContext.ChatLists.Add(chatList);
            dataContext.SaveChanges();

            return chatList;
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Error(ex.Message);
            return null;
        }
    }

    public bool AddChat(ChatList chatList, User caller)
    {
        try
        {
            var permission = CheckPermission(caller);
            if (permission >= Permissions.Viewer)
            {
                throw new InvalidOperationException("不允许使用仅查看用户进行发消息操作.");
            }

            using var serviceScope = _provider.CreateScope();
            using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

            if (chatList.ChatId != null)
            {
                //若已存在，直接进行更新操作
                var exists = dataContext.ChatLists.Where(c => c.ChatId == chatList.ChatId).Select(c => c);
                if (exists != null)
                {
                    var ex = exists.Single();
                    Utilities.Copy(chatList, ex);
                    dataContext.ChatLists.Update(ex);
                    dataContext.SaveChanges();
                    return true;
                }
                //对于创建的场景，外键列ItemId无变化，不需要修改从表sale
                dataContext.ChatLists.Add(chatList);
                dataContext.SaveChanges();
            }

            return true;
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Error(ex.Message);
            return false;
        }
    }

    public ChatList? GetChat(string chatId, User caller)
    {
        try
        {
            var permission = CheckPermission(caller);
            if (permission >= Permissions.Viewer)
            {
                throw new InvalidOperationException("不允许使用仅查看用户进行该操作.");
            }

            using var serviceScope = _provider.CreateScope();
            using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

            var result = dataContext.ChatLists
                         .Where(c => c.ChatId == chatId)
                         .Include(c => c.Messages)
                         .Select(c => c);
            return result.FirstOrDefault();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Error(ex.Message);
            return null;
        }
    }

    public ChatList? GetChat(string customerId, string sellerId, User caller)
    {
        try
        {
            var permission = CheckPermission(caller);
            if (permission >= Permissions.Viewer)
            {
                throw new InvalidOperationException("不允许使用仅查看用户进行该操作.");
            }

            using var serviceScope = _provider.CreateScope();
            using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

            var result = dataContext.ChatLists
                         .Where(c => (c.CustomerId == customerId && c.SellerId == sellerId))
                         .Include(c => c.Messages)
                         .Select(c => c);
            return result.FirstOrDefault();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Error(ex.Message);
            return null;
        }
    }

    public IEnumerable<ChatList>? GetChats(User caller)
    {
        try
        {
            var permission = CheckPermission(caller);
            if (permission >= Permissions.Viewer)
            {
                throw new InvalidOperationException("不允许使用仅查看用户进行该操作.");
            }

            using var serviceScope = _provider.CreateScope();
            using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

            var result = dataContext.ChatLists
                         .Where(c => c.CustomerId == caller.Id || c.SellerId == caller.Id)
                         .Include(c => c.Messages)
                         .Select(c => c);
            return [.. result];
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Error(ex.Message);
            return null;
        }
    }
}