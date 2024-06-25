using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model;
using FirewallDemo.Model.Data;
using FirewallDemo.Security;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.ViewModel;

public partial class MainVM : ObservableObject
{
    private readonly IServiceProvider _provider;

    public MainVM(IServiceProvider provider, UserCenter userCenter)
    {
        _provider = provider;
        CurrentUserInfo = userCenter.GetCurrentUserInfo();
    }

    [ObservableProperty]
    private UserInfo currentUserInfo;

    [RelayCommand]
    private void StartListening()
    {
        //var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
        //var user = queryer.GetUser("user00000001");
        //if (user != null)
        //    MessageBox.Success(user.Nickname);
    }

    private void RefreshUserInfo()
    {
        if (CurrentUserInfo != null)
        {
            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
            CurrentUserInfo = userCenter.GetCurrentUserInfo();

        }
    }
}
