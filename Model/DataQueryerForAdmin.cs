using FirewallDemo.Model.Data;
using FirewallDemo.Security;
using FirewallDemo.Utility;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.Model
{
    public class DataQueryerForAdmin(IServiceProvider provider) : IDataQueryer, IPermissionChecker
    {
        private readonly IServiceProvider _provider = provider;

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

        public ChatList? GetChat(string chatId, User caller)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ChatList>? GetChats(string customerId, string sellerId, User caller)
        {
            throw new NotImplementedException();
        }

        public Item? GetItem(string itemId, User caller)
        {
            throw new NotImplementedException();
        }

        public Message? GetMessage(string msgId, User caller)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Message>? GetMessages(ChatList chatList, User caller)
        {
            throw new NotImplementedException();
        }

        public Sale? GetSale(string saleId, User caller)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Sale>? GetSales(string customerId, string sellerId, User caller)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 根据用户Id获取用户,若权限低于Admin则不可获得管理员用户
        /// </summary>
        /// <param name="userId">要获取的用户Id</param>
        /// <param name="caller">调用者</param>
        /// <returns>获取到的用户</returns>
        public UserInfo? GetUser(string userId, User caller)
        {
            var permission = CheckPermission(caller);

            using var serviceScope = _provider.CreateScope();
            using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
            try
            {
                if (permission < Permissions.Internal)
                {
                    var user =  (from u in dataContext.Users.AsParallel()
                            where u.Id == userId
                            select u).SingleOrDefault() ?? throw new ArgumentNullException("用户不存在");
                    var result = new UserInfo();
                    Utilities.Copy(user, result);
                    return result;
                }
                else
                {
                    var user = (from u in dataContext.Users.AsParallel()
                            where u.Id == userId
                            && (u.Role != "SUPREME" && u.Role != "ADMIN")
                            select u).SingleOrDefault() ?? throw new ArgumentNullException("用户不存在");
                    var result = new UserInfo();
                    Utilities.Copy(user, result);
                    return result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Error(ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 根据用户名和密码获取用户,仅在调用者权限为Admin及以上才能成功
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="caller">调用者</param>
        /// <returns>登陆成功后的用户</returns>
        /// <exception cref="InvalidOperationException">权限不足</exception>
        public User? GetUser(string userName, string password, User caller)
        {
            var permission = CheckPermission(caller);
            if (permission > Permissions.Internal)
            {
                throw new InvalidOperationException("不允许使用低权限用户进行登录查询操作.");
            }
            try
            {
                using var serviceScope = _provider.CreateScope();
                using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();

                return (from u in dataContext.Users.AsParallel()
                        where u.Name == userName
                        && u.Password == password
                        select u).SingleOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Error(ex.Message);
                return null;
            }
        }
    }
}