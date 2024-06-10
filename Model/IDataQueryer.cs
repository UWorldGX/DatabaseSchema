using FirewallDemo.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.Model;

/// <summary>
/// 数据查询类接口定义
/// </summary>
public interface IDataQueryer
{
    public Item? GetItem(string itemId);
    public Sale? GetSale(string saleId);
    public IEnumerable<Sale>? GetSales(string customerId, string sellerId);
    public User? GetUser(string userId);
    public Message? GetMessage(string msgId);
    public IEnumerable<Message>? GetMessages(ChatList chatList);
    public ChatList? GetChat(string chatId);
    public IEnumerable<ChatList>? GetChats(string customerId, string sellerId);
}
