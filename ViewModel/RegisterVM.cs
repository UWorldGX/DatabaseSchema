using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.ViewModel;

public partial class RegisterVM : ObservableObject
{
    private readonly IServiceProvider _provider;

    [ObservableProperty]
    private string userName = string.Empty;
    [ObservableProperty]
    private string nickname = string.Empty;
    [ObservableProperty]
    private string password = string.Empty;
    [ObservableProperty]
    private string password2 = string.Empty;
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
        //用户名和密码的检查
    }

    /// <summary>
    /// 导入已存在的用户以对用户信息进行修改
    /// </summary>
    /// <param name="userInfo"></param>
    public void ModifyUser(UserInfo userInfo)
    {

    }

}
