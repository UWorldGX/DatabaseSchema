using FirewallDemo.Model.Data;
using FirewallDemo.Security;
using FirewallDemo.Utility;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public Item? GetItem(string itemId, User caller)
    {
        throw new NotImplementedException();
    }

    public Message? GetMessage(string msgId, User caller)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Message>? GetMessages(ChatList chatList, User caller)
    {
        throw new NotImplementedException();
    }

    public Sale? GetSale(string saleId, User caller)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Sale>? GetSales(string customerId, string sellerId, User caller)
    {
        throw new NotImplementedException();
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
}