using FirewallDemo.Model.Data;
using FirewallDemo.Security;
using FirewallDemo.Utility;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.Model.UserManage
{
    public class LoginManager : User, ILoginManager
    {
        public LoginManager(IServiceProvider provider)
        {
            _provider = provider;
            var rsa = RSA.Create();
            this.Age = 26;
            this.Sex = "M";
            this.Role = "SUPREME";
            this.Id = "user10000002";
            this.Nickname = "innerUser";
            this.PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            this.privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

        }
        private readonly IServiceProvider _provider;
        private readonly string privateKey;

        public UserInfo Login(string userName, string pwd)
        {
            //若用户名或密码为空，直接抛出异常
            ArgumentNullException.ThrowIfNull(userName, "用户名");
            ArgumentNullException.ThrowIfNull(pwd, "密码");

            //尝试获取用户

            var queryer = _provider.GetRequiredService<DataQueryerForAdmin>();
            var userCenter = _provider.GetRequiredService<UserCenter>();
            //登录
            var currentUser = queryer.GetUser(userName, pwd, this);
            if(currentUser != null)
            {
                //获取成功，导入用户
                userCenter.ImportCurrentUser(currentUser, this);
                var result = new UserInfo();
                Utilities.Copy(currentUser, result);
                MessageBox.Success("登录成功.");
                return result;
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

    }
}
