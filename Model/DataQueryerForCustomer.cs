using FirewallDemo.Model.Data;
using FirewallDemo.Security;
using FirewallDemo.Utility;
using FirewallDemo.ViewModel.EntityVM;
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

public class DataQueryerForCustomer(IServiceProvider provider) : IPermissionChecker, IStatusChecker
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

    public Item? GetItem(string itemId, User caller)
    {
        var permission = CheckPermission(caller);

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

    public bool AddSale(Sale sale, User caller)
    {
        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
        try
        {
            ArgumentNullException.ThrowIfNull(sale.ItemId);
            ArgumentNullException.ThrowIfNull(sale.CustomerId);
            ArgumentNullException.ThrowIfNull(sale.SellerId);
            ArgumentNullException.ThrowIfNull(sale.Timestamp);
            var permission = CheckPermission(caller);
            if (permission >= Permissions.Viewer)
            {
                throw new InvalidOperationException("不允许使用仅查看用户进行商品购买操作.");
            }
            if (GetItem(sale.ItemId, caller) == null)
            {
                throw new ArgumentException("要创建的sale没有有效的商品id与其对应。");
            }
            //创建SaleId, 并去重
            sale.SaleId = "SL-" + Guid.NewGuid().ToString()[..12];
            do
            {
                var exists = dataContext.Sales.Where(m => m.SaleId == sale.SaleId).Select(m => m);
                if (exists.Any())
                {
                    sale.SaleId = "SL-" + Guid.NewGuid().ToString()[..12];
                }
                else
                    break;
            }
            while (true);
            //添加sale,保存修改
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

    /// <summary>
    /// 完成交易，交易成功，钱款给卖家
    /// </summary>
    /// <param name="saleId"></param>
    /// <param name="caller"></param>
    public void FinishSale(string saleId, User caller)
    {
        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
        try
        {
            var permission = CheckPermission(caller);
            if (permission >= Permissions.Viewer)
            {
                throw new InvalidOperationException("不允许使用仅查看用户进行商品购买操作.");
            }
            var sale = GetSale(saleId, caller);
            if (sale != null)
            {
                UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
                var queryerAdmin = _provider.GetRequiredService<DataQueryerForAdmin>();
                var customer = queryerAdmin.GetUser(sale.CustomerId, userCenter.InnerUser);
                var seller = queryerAdmin.GetUser(sale.SellerId, userCenter.InnerUser);
                //可能存在警告后强行交易，因而显式设置商品已售出
                SetItemStatus(sale.ItemId, ItemStatus.SOLD, caller);
                //设置交易成功，保存修改
                sale.Status = SaleStatus.Success.ToString("F");
                dataContext.Sales.Update(sale);
                dataContext.SaveChanges();
                if (customer != null && seller != null)
                {
                    //更改交易双方钱款，保存交易结果
                    customer.Money -= (int)sale.RealPrice;
                    seller.Money += (int)sale.RealPrice;
                    dataContext.Users.Update(customer);
                    dataContext.SaveChanges();
                    dataContext.Users.Update(seller);
                    dataContext.SaveChanges();
                }
                //开始事务，开始执行存储过程
                dataContext.Database.BeginTransaction();
                dataContext.Database.ExecuteSqlInterpolated($"CALL prog_autodelist({sale.ItemId})");
                dataContext.Database.CommitTransaction();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return;
        }
    }

    /// <summary>
    /// 根据saleId唯一确定一个sale.
    /// </summary>
    /// <param name="saleId"></param>
    /// <param name="caller"></param>
    /// <returns></returns>
    public Sale? GetSale(string saleId, User caller)
    {
        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
        try
        {
            var result = dataContext.Sales
                .Where(u => u.SaleId == saleId)
                .Include(u => u.Item)
                .Select(u => u).SingleOrDefault() ?? throw new ArgumentNullException(nameof(saleId), "不存在符合条件的销售记录");
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
            var result = dataContext.Sales
                .Where(u => u.CustomerId == customerId && u.SellerId == sellerId)
                .Include(u => u.Item)
                .OrderBy(u => u.Timestamp)
                .AsNoTracking()
                .Select(u => u).ToArray() ?? throw new ArgumentNullException(nameof(customerId), "不存在符合条件的销售记录");
            return result;
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return null;
        }
    }

    public IEnumerable<Sale>? GetSales(User caller)
    {
        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
        try
        {
            var result = dataContext.Sales
                .Where(u => u.CustomerId == caller.Id || u.SellerId == caller.Id)
                .Include(u => u.Item)
                .OrderBy(u => u.Timestamp)
                .AsNoTracking()
                .Select(u => u).ToArray() ?? throw new ArgumentNullException(nameof(caller), "不存在符合条件的销售记录");
            return result;
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return null;
        }
    }

    public Sale UpdateSale(Sale sale, User caller)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(sale);
            var permission = CheckPermission(caller);
            if (permission >= Permissions.Viewer)
            {
                throw new InvalidOperationException("不允许使用仅查看用户进行该操作.");
            }
            using var serviceScope = _provider.CreateScope();
            using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
            //var exists = dataContext.Sales.Where(s => s.SaleId == sale.SaleId).Select(s => s).Single();
            //dataContext.Sales.Remove(exists);
            //dataContext.SaveChanges();
            //Utilities.Copy(sale, exists);

            //dataContext.Sales.Add(sale);
            sale.Item = null;
            dataContext.Sales.Update(sale);
            dataContext.SaveChanges();
            return sale;
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return sale;
        }
    }

    public bool SetSaleStatus(string saleId, SaleStatus status, User caller)
    {
        try
        {
            var permission = CheckPermission(caller);
            if (permission >= Permissions.Viewer)
            {
                throw new InvalidOperationException("不允许使用仅查看用户进行该操作.");
            }

            if (status >= SaleStatus.ClosedByOperator && permission > Permissions.Admin)
            {
                throw new InvalidOperationException("该状态仅管理员可设置.");
            }

            using var serviceScope = _provider.CreateScope();
            using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
            var sale = GetSale(saleId, caller);
            if (sale != null)
            {
                sale.Status = status.ToString("F");
                sale.Item = null;
                dataContext.Sales.Update(sale);
                dataContext.SaveChanges();
                //一并将商品也下架
                if (sale.Status.Contains("Closed"))
                {
                    var item = GetItem(sale.ItemId, caller);
                    if (item != null)
                    {
                        item.DelistingTimestamp = null;
                        item.ListingTimestamp = DateTime.Now;
                        item.ItemStatus = "ONSALE";
                        dataContext.Items.Update(item);
                        dataContext.SaveChanges();
                    }
                }
                //一并将商品也设成警告状态
                if (CheckSaleStatus(sale.Status) == SaleStatus.Warned)
                {
                    var item = GetItem(sale.ItemId, caller);
                    if (item != null)
                    {
                        item.ItemStatus = "WARNED";
                        dataContext.Items.Update(item);
                        dataContext.SaveChanges();
                    }
                }
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return false;
        }
    }

    public UserInfo? GetUser(string userId, User caller)
    {
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

    public IEnumerable<UserInfo>? GetAllUsers(User caller)
    {
        try
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
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return null;
        }
    }

    public bool AddItem(Item item, User caller)
    {
        try
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
                //对于创建的场景，外键列ItemId无变化，不需要修改从表sale
                dataContext.Items.Add(item);
                dataContext.SaveChanges();
            }

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return false;
        }
    }

    public bool ModifyItem(Item item, User caller)
    {
        try
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
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return false;
        }
    }

    public bool SetItemStatus(string itemId, ItemStatus status, User caller)
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
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return false;
        }
    }

    public SaleStatus CheckSaleStatus(string status)
    {
        return status switch
        {
            "WaitingforSeller" => SaleStatus.WaitingforSeller,
            "WaitingforBuyer" => SaleStatus.WaitingforBuyer,
            "Success" => SaleStatus.Success,
            "ClosedBySeller" => SaleStatus.ClosedBySeller,
            "ClosedByCustomer" => SaleStatus.ClosedByCustomer,
            "ClosedByOperator" => SaleStatus.ClosedByOperator,
            _ => SaleStatus.Warned
        };
    }

    public ItemStatus CheckItemStatus(string status)
    {
        return status switch
        {
            "ONSALE" => ItemStatus.ONSALE,
            "SOLD" => ItemStatus.SOLD,
            "DELISTED" => ItemStatus.DELISTED,
            _ => ItemStatus.WARNED
        };
    }
}