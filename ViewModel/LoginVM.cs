using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model.UserManage;
using FirewallDemo.View;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.ViewModel
{
    public partial class LoginVM(IServiceProvider provider) : ObservableObject
    {
        private readonly IServiceProvider _provider = provider;

        [ObservableProperty]
        private string userName;
        [ObservableProperty]
        private string password;


        [RelayCommand]
        private void Login()
        {
            var loginMgr = _provider.GetRequiredService<LoginManager>();
            try
            {
                //尝试登录
                var currentUser = loginMgr.Login(UserName, Password);
                if(currentUser == null)
                {
                    return;
                }
                var mainWindow = _provider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                var loginWindow = _provider.GetRequiredService<LoginWindow>();
                loginWindow.Close();
            }
            catch(ArgumentNullException argEx)
            {
                MessageBox.Error($"{argEx.ParamName}为空.","错误");
                return;
            }

        }
        [RelayCommand]
        private void FillCustomer()
        {
            UserName = "Mok Ting Fung";
            Password = "Ds5e7zJAxf";
        }

        [RelayCommand]
        private void FillAdmin()
        {
            UserName = "Yan Jialun";
            Password = "6ibsaR1wEc";
        }

    }
}
