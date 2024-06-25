using FirewallDemo.Model;
using FirewallDemo.Model.Data;
using FirewallDemo.Model.UserManage;
using FirewallDemo.Utility;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.Security;

/// <summary>
/// 程序内用户和当前用户管理器
/// </summary>
public class UserCenter
{
    private readonly IServiceProvider _serviceProvider;
    /// <summary>
    /// 程序内部用户，拥有最高权限
    /// </summary>
    public User InnerUser { get; private set; }
    public User CurrentUser { get; private set; }


    /// <summary>
    /// 内部用户私钥，只读
    /// </summary>
    private readonly string innerPrivateKey;

    public UserCenter(IServiceProvider provider)
    {
        this._serviceProvider = provider;
        //该类构造时生成一个内部用户和该用户的一对公钥-私钥
        var rsa = RSA.Create();
        InnerUser = new User()
        {
            Age = 25,
            Sex = "M",
            Role = "SUPREME",
            Id = "user10000001",
            Nickname = "innerUser",
            PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey())
        };
        CurrentUser = new User();

        innerPrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
    }
    /// <summary>
    /// 导入程序中的当前用户，必须由LoginManager进行导入
    /// </summary>
    /// <param name="user"></param>
    /// <param name="loginMgr"></param>
    /// <returns></returns>
    public bool ImportCurrentUser(User user, LoginManager loginMgr)
    {
        if (loginMgr == null)
            return false;
        if (user == null)
            return false;
        CurrentUser = new User();
        Utilities.Copy(user, CurrentUser);
        return true;
    }

    public UserInfo GetCurrentUserInfo()
    {
        var result = new UserInfo();
        Utilities.Copy(CurrentUser, result);
        return result;
    }
}