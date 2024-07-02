using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.ViewModel.EntityVM;
using HandyControl.Controls;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable

namespace FirewallDemo.ViewModel;

public partial class FilterVM(IServiceProvider provider) : ObservableObject
{
    private readonly IServiceProvider _provider = provider;

    [ObservableProperty]
    private string typeFilter;

    [ObservableProperty]
    private decimal sellPriceMin;

    [ObservableProperty]
    private decimal sellPriceMax;

    [ObservableProperty]
    private DateTime startTime = DateTime.Now;

    [ObservableProperty]
    private DateTime endTime = DateTime.Now;

    [RelayCommand]
    private void FilterByType(object sender)
    {
        if(sender is SearchBar searchBar)
        {
            var mainVM = _provider.GetRequiredService<MainVM>();
            var expr = (IEnumerable<ItemSold> items) => 
            { return items.Where(i => i.Type.Contains(searchBar.Text)).AsQueryable(); } ;
            mainVM?.Filter(expr);
        }

    }

    [RelayCommand]
    private void FilterByPrice()
    {
        var mainVM = _provider.GetRequiredService<MainVM>();
        var expr = (IEnumerable<ItemSold> items) =>
        { return items.Where(i => i.SellPrice > SellPriceMin && i.SellPrice < SellPriceMax).AsQueryable(); };
        mainVM?.Filter(expr);
    }

    [RelayCommand]
    private void FilterByTime()
    {
        var mainVM = _provider.GetRequiredService<MainVM>();
        var expr = (IEnumerable<ItemSold> items) =>
        { return items.Where(i => i.ListingTimestamp > StartTime && i.ListingTimestamp < EndTime).AsQueryable(); };
        mainVM?.Filter(expr);
    }

    [RelayCommand]
    private void ClearFilter()
    {
        var mainVM = _provider.GetRequiredService<MainVM>();
        mainVM?.ClearFilter();

    }


    [RelayCommand]
    private void VerifyTime()
    {
        if (StartTime - EndTime > TimeSpan.Zero)
        {
            MessageBox.Error("起始时间不能晚于结束时间!");
            StartTime = EndTime.AddHours(-1);
        }
    }
}
