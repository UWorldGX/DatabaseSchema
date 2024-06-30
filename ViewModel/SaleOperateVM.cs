using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model.Data;
using FirewallDemo.Utility;
using FirewallDemo.ViewModel.EntityVM;
using HandyControl.Data;
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

    [RelayCommand]
    private void PageUpdateCommand(FunctionEventArgs<int> info)
    {
        CurrentSales.Clear();
        var list = Sales.Skip((info.Info - 1) * 10).Take(10).ToList();
        foreach (var item in list)
        {
            CurrentSales.Add(item);
        }
    }

    public void ImportSales(IEnumerable<Sale> sales)
    {

        foreach (var item in sales)
        {
            var saleVM = new SaleVM(_provider);
            Utilities.Copy(item, saleVM);
            Sales.Add(saleVM);
        }
        var list = Sales.Take(10).ToList();
        foreach(var s in list)
        {
            CurrentSales.Add(s);
        }
    }

}
