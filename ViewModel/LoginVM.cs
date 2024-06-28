using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model.UserManage;
using FirewallDemo.View;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.ViewModel
{
    public partial class LoginVM(IServiceProvider provider, ILogger<LoginVM> logger) : ObservableObject
    {
        private readonly ILogger<LoginVM> _logger = logger;
        private readonly IServiceProvider _provider = provider;

        [ObservableProperty]
        private string userName = string.Empty;
        [ObservableProperty]
        private string password = string.Empty;


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
                    _logger.LogError("用户登录失败");
                    return;
                }
                var mainWindow = _provider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                _logger.LogInformation($"用户登录成功.用户名:{UserName}");
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
        private void FillCustomer2()
        {
            UserName = "Yang Lu";
            Password = "lYTb1W18nN";
        }

        [RelayCommand]
        private void FillAdmin()
        {
            UserName = "Yan Jialun";
            Password = "6ibsaR1wEc";
        }

    }
}
