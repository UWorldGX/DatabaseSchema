using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model;
using FirewallDemo.Model.Data;
using FirewallDemo.Model.UserManage;
using FirewallDemo.Security;
using FirewallDemo.View;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.ViewModel
{
    public partial class LoginVM : ObservableObject
    {
        private readonly ILogger<LoginVM> _logger;
        private readonly IServiceProvider _provider;

        [ObservableProperty]
        private string userName = string.Empty;
        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private ObservableCollection<UserInfo> userList = [];

        public LoginVM(IServiceProvider provider, ILogger<LoginVM> logger)
        {
            _provider = provider;
            _logger = logger;
            var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
            var userInfos = queryer.GetAllUsers(userCenter.InnerUser);
            foreach(var u in userInfos)
            {
                UserList.Add(u);
            }
        }


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
        private void FillUser(object sender)
        {
            if(sender is System.Windows.Controls.ComboBox cbx)
            {
                if(cbx.SelectedItem is UserInfo user)
                {
                    var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
                    UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
                    UserName = user.Name;
                    using var serviceScope = _provider.CreateScope();
                    using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

                    Password = dataContext.Users.Where(u => u.Name == UserName).Select(u => u.Password).Single();
                }
            }

        }

        [RelayCommand]
        private void Register()
        {
            RegisterWindow regiWindow = _provider.GetRequiredService<RegisterWindow>();
            if(regiWindow.ShowDialog() == true)
            {
                MessageBox.Success("注册成功，请登录.");
            }
        }

        //[RelayCommand]
        //private void FillCustomer2()
        //{
        //    UserName = "Yang Lu";
        //    Password = "lYTb1W18nN";
        //}

        //[RelayCommand]
        //private void FillAdmin()
        //{
        //    UserName = "Yan Jialun";
        //    Password = "6ibsaR1wEc";
        //}

    }
}
