using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.ViewModel;

public partial class MainVM(IServiceProvider provider) : ObservableObject
{
    private readonly IServiceProvider _provider = provider;

    [RelayCommand]
    private void StartListening()
    {
        var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
        var user = queryer.GetUser("user00000001");
        if(user != null)
            MessageBox.Success(user.Nickname);
    }
}
