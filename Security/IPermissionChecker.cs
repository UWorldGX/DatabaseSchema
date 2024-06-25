using FirewallDemo.Model;
using FirewallDemo.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.Security;

internal interface IPermissionChecker
{
    Permissions CheckPermission(User user);
}

public enum Permissions
{
    Supreme,
    Admin,
    Internal,
    User,
    Viewer
}