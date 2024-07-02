
using FirewallDemo.Model.Data;
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
    public Item? GetItem(string itemId, User caller);
    public Sale? GetSale(string saleId, User caller);
    public IEnumerable<Sale>? GetSales(string customerId, string sellerId, User caller);
    public UserInfo? GetUser(string userId, User caller);

}
