using FirewallDemo.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.Security;

public interface IStatusChecker
{
    public SaleStatus CheckSaleStatus(string status);
    public ItemStatus CheckItemStatus(string status);
}
