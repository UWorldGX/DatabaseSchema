using FirewallDemo.Model.Data;
using FirewallDemo.Security;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.Model.UserManage;

public class RegisterManager(IServiceProvider serviceProvider) : IRegisterManager, IPermissionChecker
{
    private readonly IServiceProvider _provider = serviceProvider;
    public Permissions CheckPermission(User user)
    {
        return user.Role switch
        {
            "USER" => Permissions.User,
            "SUPREME" => Permissions.Supreme,
            "ADMIN" => Permissions.Admin,
            "INTERNAL" => Permissions.Internal,
            _ => Permissions.Viewer,
        };
    }
    /// <summary>
    /// 注册用户。只有内部用户才有此权限。
    /// </summary>
    /// <param name="userInfo"></param>
    /// <param name="password"></param>
    public void RegisterUser(UserInfo userInfo, string password, User caller)
    {
        ArgumentNullException.ThrowIfNull(userInfo);
        ArgumentNullException.ThrowIfNull(password);
        var permission = CheckPermission(caller);
        if (permission > Permissions.Admin)
        {
            throw new InvalidOperationException("不允许使用低权限用户进行用户注册操作.");
        }
        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

        //var exists = dataContext.Users.Where(u => u.Id == userInfo.Id).Select(u => u);
        //if(exists != null)
        //{
        //    throw new InvalidOperationException("用户已存在.");
        //}

            User user = new()
            {
                Age = userInfo.Age,
            Name = userInfo.Name,
            Sex = userInfo.Sex,
            Nickname = userInfo.Nickname,
            Money = 1500,
            Password = password
            };
            //生成用户公钥
            RSA rsa = RSA.Create();
            user.PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());


            var userCount = (from u in dataContext.Users.AsParallel() select u).Count();
            user.Id = "user" + (userCount + 1).ToString("D8");
            user.Role = "USER";

            dataContext.Users.Add(user);
            dataContext.SaveChanges();

            var privkey = new UserPrivkey()
            { PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey()),
             UserId = user.Id};
            dataContext.UserPrivkeys.Add(privkey);
            dataContext.SaveChanges();
    }
}
