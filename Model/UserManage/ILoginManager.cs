using FirewallDemo.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.Model.UserManage
{
    public interface ILoginManager
    {
        public UserInfo Login(string userName, string pwd);
    }
}
