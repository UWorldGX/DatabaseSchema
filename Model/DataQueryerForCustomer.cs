using FirewallDemo.Model.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.Model;

public class DataQueryerForCustomer(IServiceProvider provider) : IDataQueryer
{
    private readonly IServiceProvider _provider = provider;
    public ChatList? GetChat(string chatId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ChatList>? GetChats(string customerId, string sellerId)
    {
        throw new NotImplementedException();
    }

    public Item? GetItem(string itemId)
    {
        throw new NotImplementedException();
    }

    public Message? GetMessage(string msgId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Message>? GetMessages(ChatList chatList)
    {
        throw new NotImplementedException();
    }

    public Sale? GetSale(string saleId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Sale>? GetSales(string customerId, string sellerId)
    {
        throw new NotImplementedException();
    }

    public User? GetUser(string userId)
    {
        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

        return (from u in dataContext.Users.AsParallel()
                where u.Id == userId
                select u).SingleOrDefault();

    }
}
