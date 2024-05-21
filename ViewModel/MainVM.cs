using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.ViewModel;

public partial class MainVM : ObservableObject
{
    [ObservableProperty]
    private PacketFilterFirewall _firewall = new(IPAddress.Parse("127.0.0.1"), 8888);

    [RelayCommand]
    private void StartListening()
    {
        _firewall.Filter();
    }
}
