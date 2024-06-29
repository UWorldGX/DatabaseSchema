using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

}
