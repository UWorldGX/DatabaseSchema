using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

namespace FirewallDemo.ViewModel.EntityVM;

public partial class SaleVM(IServiceProvider provider) : ObservableObject
{
    private readonly IServiceProvider _provider = provider;
    [ObservableProperty]
    private string saleId;

    [ObservableProperty]
    private string itemId;

    [ObservableProperty]
    private string customerId;

    [ObservableProperty]
    private string sellerId;

    [ObservableProperty]
    private decimal realPrice;

    [ObservableProperty]
    private DateTime timestamp;

    [ObservableProperty]
    private string status;

    [ObservableProperty]
    private Item item;

    [RelayCommand]
    private void AcceptSale()
    {

    }

    [RelayCommand]
    private void RejectSale()
    {

    }
}
