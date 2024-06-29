using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

namespace FirewallDemo.ViewModel.EntityVM;

public partial class MsgVM(IServiceProvider provider) : ObservableObject
{
    private readonly IServiceProvider _provider = provider;
    [ObservableProperty]
    private string msgId;

    [ObservableProperty]
    private string chatId;

    [ObservableProperty]
    private DateTime timestamp;

    [ObservableProperty]
    private string content;
    [ObservableProperty]
    private string senderId;
    [ObservableProperty]
    private sbyte unread;

    [ObservableProperty]
    private string role = "Receiver";
}
