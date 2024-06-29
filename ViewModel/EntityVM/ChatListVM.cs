using CommunityToolkit.Mvvm.ComponentModel;
using FirewallDemo.Model;
using FirewallDemo.Model.Data;
using FirewallDemo.Utility;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

namespace FirewallDemo.ViewModel.EntityVM;

public partial class ChatListVM(IServiceProvider provider) : ObservableObject
{
    private readonly IServiceProvider _provider = provider;
    [ObservableProperty]
    private string chatId;

    [ObservableProperty]
    private string customerId;

    [ObservableProperty]
    private string sellerId;
    [ObservableProperty]
    private ObservableCollection<MsgVM> messages = [];

    [ObservableProperty]
    private UserInfo targetUser;

    public static ChatListVM Create(ChatList chatList, IServiceProvider serviceProvider, User caller)
    {
        ChatListVM vm = new(serviceProvider);
        Utilities.Copy(chatList, vm);
        foreach(var m in chatList.Messages)
        {
            var ms = new MsgVM(serviceProvider);
            Utilities.Copy(m, ms);
            if (ms.SenderId == caller.Id)
                ms.Role = "Sender";
            else
                ms.Role = "Receiver";
            vm.Messages.Add(ms); 
        }
        var sorted = vm.Messages.OrderBy(m => m.Timestamp).Select(m => m).ToArray();
        vm.Messages.Clear();
        foreach (var m in sorted)
            vm.Messages.Add(m);

        //获取目标用户
        var queryer = serviceProvider.GetRequiredService<DataQueryerForCustomer>();
        if (caller.Id == vm.CustomerId)
        {
            vm.TargetUser = queryer.GetUser(chatList.SellerId, caller);
        }
        else
        {
            vm.TargetUser = queryer.GetUser(chatList.CustomerId, caller);
        }
        return vm;

    }
}
