using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model.Data;
using FirewallDemo.Model.UserManage;
using FirewallDemo.Security;
using FirewallDemo.View;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
#nullable disable

namespace FirewallDemo.ViewModel;

public partial class RegisterVM : ObservableObject
{
    private readonly IServiceProvider _provider;

    [ObservableProperty]
    private string userName;
    [ObservableProperty]
    private string nickName;
    [ObservableProperty]
    private string password;
    [ObservableProperty]
    private string password2;
    [ObservableProperty]
    private bool isMale = true;
    [ObservableProperty]
    private bool isFemale;
    [ObservableProperty]
    private sbyte age;

    public RegisterVM(IServiceProvider provider)
    {
        _provider = provider;
    }
    [RelayCommand]
    private void Register()
    {
        try
        {
            //用户名和密码的检查
            ArgumentNullException.ThrowIfNull(UserName);
            ArgumentNullException.ThrowIfNull(NickName);
            ArgumentNullException.ThrowIfNull(Password);
            ArgumentNullException.ThrowIfNull(Password2);
            if(Password != Password2)
            {
                MessageBox.Error("前后密码不一致.");
                return;
            }
            //先构建UserInfo
            UserInfo newUser = new()
            {
                Name = this.UserName,
                Nickname = this.NickName,
                Age = this.Age,
                Sex = IsMale ? "M" : "F"
            };

            var registerMgr = _provider.GetRequiredService<RegisterManager>();
            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();

            registerMgr.RegisterUser(newUser, Password2, userCenter.InnerUser);
            MessageBox.Success("注册成功，请登录。", "提示");
            var registerWindow = _provider.GetRequiredService<RegisterWindow>();
            registerWindow.Close();
        }
        catch (ArgumentNullException argEx)
        {
            MessageBox.Error($"{argEx.ParamName}为空.", "错误");
            return;
        }


    }

    /// <summary>
    /// 导入已存在的用户以对用户信息进行修改
    /// </summary>
    /// <param name="userInfo"></param>
    public void ModifyUser(UserInfo userInfo)
    {
        ArgumentNullException.ThrowIfNull(userInfo);
    }

}
