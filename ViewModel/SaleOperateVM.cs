using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model;
using FirewallDemo.Model.Data;
using FirewallDemo.Security;
using FirewallDemo.Utility;
using FirewallDemo.ViewModel.EntityVM;
using HandyControl.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.ViewModel;

public partial class SaleOperateVM(IServiceProvider provider) : ObservableObject
{
    private readonly IServiceProvider _provider = provider;

    [ObservableProperty]
    private ObservableCollection<SaleVM> sales = [];

    [ObservableProperty]
    private ObservableCollection<SaleVM> currentSales = [];

    [ObservableProperty]
    private int pageIndex = 1;

    [ObservableProperty]
    private bool isAdmin;



    //[RelayCommand]
    //private void PageUpdate(FunctionEventArgs<int> info)
    //{
    //    CurrentSales.Clear();
    //    var list = Sales.Skip((info.Info - 1) * 4).Take(4).ToList();
    //    foreach (var item in list)
    //    {
    //        CurrentSales.Add(item);
    //    }
    //    PageSize = Sales.Count / 4;
    //    if (Sales.Count % 4 != 0)
    //        PageSize++;
    //}

    public void ImportSales(IEnumerable<Sale> sales, bool isAdmin)
    {
        var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
        UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
        IsAdmin = isAdmin;
        foreach (var item in sales)
        {
            var saleVM = new SaleVM(_provider);
            Utilities.Copy(item, saleVM);
            saleVM.IsAdmin = isAdmin;
            saleVM.SellerNickname = queryer.GetUser(saleVM.SellerId, userCenter.CurrentUser)!.Nickname;
            saleVM.SetOperatable();
            Sales.Add(saleVM);
        }

        var list = Sales.ToList();
        foreach (var s in list)
        {
            CurrentSales.Add(s);
        }
    }
    
    public void RefreshSales()
    {
        var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
        UserCenter userCenter = _provider.GetRequiredService<UserCenter>();

        var sales = queryer.GetSales(userCenter.CurrentUser);
        CurrentSales.Clear();
        Sales.Clear();

        foreach (var item in sales)
        {
            var saleVM = new SaleVM(_provider);
            Utilities.Copy(item, saleVM);
            saleVM.IsAdmin = IsAdmin;
            saleVM.SellerNickname = queryer.GetUser(saleVM.SellerId, userCenter.CurrentUser)!.Nickname;
            saleVM.SetOperatable();
            Sales.Add(saleVM);
        }

        var list = Sales.ToList();
        foreach (var s in list)
        {
            CurrentSales.Add(s);
        }
    }
}