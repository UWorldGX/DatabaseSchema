using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
            mainVM?.Filter("type", searchBar.Text);
        }

    }

    [RelayCommand]
    private void ClearFilter()
    {
        var mainVM = _provider.GetRequiredService<MainVM>();
        mainVM?.Filter("default", TypeFilter);

    }
}
