using FirewallDemo.Model.Data;
using FirewallDemo.Security;
using FirewallDemo.Utility;
using HandyControl.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace FirewallDemo.Model;

public class DataQueryerForCustomer(IServiceProvider provider) : IDataQueryer, IPermissionChecker
{
    private readonly IServiceProvider _provider = provider;

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

    public ChatList? GetChat(string chatId, User caller)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ChatList>? GetChats(string customerId, string sellerId, User caller)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ChatList>? GetChats(User caller)
    {
        var permission = CheckPermission(caller);
        if (permission >= Permissions.Viewer)
        {
            throw new InvalidOperationException("不允许使用仅查看用户进行发消息操作.");
        }

        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

        var result = dataContext.ChatLists
                     .Where(c => c.CustomerId == caller.Id || c.SellerId == caller.Id)
                     .Include(c => c.Messages)
                     .Select(c => c);
        return [.. result];
    }

    public bool AddChat(ChatList chatList, User caller)
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
            //对于创建的场景，外键列ItemId无变化，不需要修改从表sale
            dataContext.ChatLists.Add(chatList);
            dataContext.SaveChanges();
        }

        return true;
    }

    public Item? GetItem(string itemId, User caller)
    {
        var permission = CheckPermission(caller);

        //管理员以上用户没有商品，直接返回null
        if (permission < Permissions.Internal)
            return null;

        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

        var result = from i in dataContext.Items.AsParallel()
                     where i.ItemId == itemId
                     select i;
        return result.SingleOrDefault();
    }

    public IEnumerable<Item>? GetItems(User callerAndOwner)
    {
        var permission = CheckPermission(callerAndOwner);

        //管理员以上用户没有商品，直接返回null
        if (permission < Permissions.Internal)
            return null;

        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

        var result = from i in dataContext.Items.AsParallel()
                     where i.SellerId == callerAndOwner.Id
                     select i;
        return result.ToArray();
    }

    public IEnumerable<Item>? GetAllItems(User callerAndOwner)
    {

        //实际上任何用户(包括仅查看)都可调用该函数
        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

        var result = from i in dataContext.Items.AsParallel()
                     select i;
        return result.ToArray();
    }

    public bool AddMessage(Message msg, User caller)
    {
        var permission = CheckPermission(caller);
        if (permission >= Permissions.Viewer)
        {
            throw new InvalidOperationException("不允许使用仅查看用户进行发消息操作.");
        }

        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

        if (msg.MsgId != null)
        {
            //对于创建的场景，外键列ItemId无变化，不需要修改从表sale
            dataContext.Messages.Add(msg);
            dataContext.SaveChanges();
            Growl.Success("上架商品成功!");
        }

        return true;
    }

    public Message CreateSignedMessage(string msgContent, string chatId, User caller)
    {
        var permission = CheckPermission(caller);
        if (permission >= Permissions.Viewer)
        {
            throw new InvalidOperationException("不允许使用仅查看用户进行发消息操作.");
        }

        var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(Convert.FromBase64String(caller.PublicKey), out int a);
        rsa.ImportRSAPrivateKey(GetUserPrivateKey(caller), out int b);
        UTF8Encoding encoder = new();
        var chars = msgContent.ToCharArray();
        var byteCount = encoder.GetByteCount(chars);
        var bytes = new Byte[byteCount];
        int bytesEncodedCount = encoder.GetBytes(chars, 8, 8, bytes, 0);
        Message result = new()
        {
            MsgId = "MSG" + Guid.NewGuid().ToString()[..8],
            ChatId = chatId,
            Timestamp = DateTime.Now,
            Content = Convert.ToBase64String(rsa.Encrypt(bytes, RSAEncryptionPadding.Pkcs1))
        };
        return result;


    }
    public Message? GetMessage(string msgId, User caller)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Message>? GetMessages(ChatList chatList, User caller)
    {
        throw new NotImplementedException();
    }


    public byte[] GetUserPrivateKey(User caller)
    {
        var permission = CheckPermission(caller);
        if (permission >= Permissions.Viewer)
        {
            throw new InvalidOperationException("不允许使用仅查看用户进行该操作.");
        }
        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
        try
        {
            var key = dataContext.UserPrivkeys.AsParallel()
                .Where(k => k.UserId == caller.Id)
                .Select(k => k).Single();
            return Convert.FromBase64String(key.PrivateKey);
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return [];
        }

    }

    public bool AddSale(Sale sale, User caller)
    {
        var permission = CheckPermission(caller);
        if (permission >= Permissions.Viewer)
        {
            throw new InvalidOperationException("不允许使用仅查看用户进行商品购买操作.");
        }

        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
        try
        {
            if(GetItem(sale.ItemId, caller) == null)
            {
                throw new ArgumentException("要创建的sale没有有效的商品id与其对应。");
            }
            dataContext.Sales.Add(sale);
            dataContext.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return false;
        }

    }

    public Sale? GetSale(string saleId, User caller)
    {
        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
        try
        {
            var result = (from u in dataContext.Sales.AsParallel()
                        where u.SaleId == saleId
                        select u).SingleOrDefault() ?? throw new ArgumentNullException(nameof(saleId), "不存在符合条件的销售记录");
            return result;
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return null;
        }
    }

    public IEnumerable<Sale>? GetSales(string customerId, string sellerId, User caller)
    {
        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
        try
        {
            var result = (from u in dataContext.Sales.AsParallel()
                          where u.CustomerId == customerId
                          && u.SellerId == sellerId
                          select u).ToArray() ?? throw new ArgumentNullException(nameof(customerId), "不存在符合条件的销售记录");
            return result;
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return null;
        }
    }

    public bool SetSaleStatus(string saleId, SaleStatus status, User caller)
    {
        var permission = CheckPermission(caller);
        if (permission >= Permissions.Viewer)
        {
            throw new InvalidOperationException("不允许使用仅查看用户进行该操作.");
        }

        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
        var sale = GetSale(saleId, caller);
        if(sale != null)
        {
            sale.Status = status.ToString("F");
            dataContext.Sales.Update(sale);
            dataContext.SaveChanges();
            return true;
        }
        return false;
    }

    public UserInfo? GetUser(string userId, User caller)
    {
        //var permission = CheckPermission(caller);

        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
        try
        {
            var user = (from u in dataContext.Users.AsParallel()
                        where u.Id == userId
                        && (u.Role != "SUPREME" && u.Role != "ADMIN")
                        select u).SingleOrDefault() ?? throw new ArgumentNullException("用户不存在");
            var result = new UserInfo();
            Utilities.Copy(user, result);
            return result;
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return null;
        }
    }

    public IEnumerable<UserInfo> GetAllUsers(User caller)
    {
        var permission = CheckPermission(caller);
        if (permission >= Permissions.Viewer)
        {
            throw new InvalidOperationException("不允许使用仅查看用户进行商品上架操作.");
        }

        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

        //展示了原生SQL ORM的能力和视图查询的能力
        var result = dataContext.Database.SqlQuery<UserInfo>($"SELECT * FROM user_info");
        return [.. result];
    }

    public bool AddItem(Item item, User caller)
    {
        var permission = CheckPermission(caller);
        if(permission >= Permissions.Viewer)
        {
            throw new InvalidOperationException("不允许使用仅查看用户进行商品上架操作.");
        }

        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

        if(item.ItemId != null)
        {
            //对于创建的场景，外键列ItemId无变化，不需要修改从表sale
            dataContext.Items.Add(item);
            dataContext.SaveChanges();
            Growl.Success("上架商品成功!");
        }

        return true;
    }

    public bool ModifyItem(Item item, User caller)
    {
        var permission = CheckPermission(caller);
        if (permission >= Permissions.Viewer)
        {
            throw new InvalidOperationException("不允许使用仅查看用户进行商品上架操作.");
        }

        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

        if (item.ItemId != null)
        {
            //对于修改的场景，外键列ItemId无变化，不需要修改从表sale
            var it = dataContext.Items.Where(i => i.ItemId == item.ItemId).Select(i => i).FirstOrDefault();
            if (it != null)
            {
                Utilities.Copy(item, it);
                dataContext.Items.Update(it);
                dataContext.SaveChanges();
                Growl.Success("商品编辑成功!");
            }
        }
        return true;
    }

    public bool SetItemStatus(string itemId, ItemStatus status, User caller)
    {
        var permission = CheckPermission(caller);
        if (permission >= Permissions.Viewer)
        {
            throw new InvalidOperationException("不允许使用仅查看用户进行该操作.");
        }

        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
        var item = GetItem(itemId, caller);
        if (item != null)
        {
            item.ItemStatus = status.ToString("F");
            dataContext.Items.Update(item);
            dataContext.SaveChanges();
            return true;
        }
        return false;
    }
}