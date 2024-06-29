using FirewallDemo.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FirewallDemo.View
{
    /// <summary>
    /// SaleOperateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SaleOperateWindow : Window
    {
        private readonly SaleOperateVM _vm;
        public SaleOperateWindow(SaleOperateVM vm)
        {
            _vm = vm;
            this.DataContext = _vm;
            InitializeComponent();
        }
    }
}
