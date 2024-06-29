using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model;
using FirewallDemo.Model.Data;
using FirewallDemo.Security;
using FirewallDemo.Utility;
using FirewallDemo.View;
using FirewallDemo.ViewModel.EntityVM;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FirewallDemo.ViewModel;

public partial class MainVM : ObservableObject
{
    private readonly IServiceProvider _provider;

    public MainVM(IServiceProvider provider, UserCenter userCenter)
    {
        _provider = provider;
        CurrentUserInfo = userCenter.GetCurrentUserInfo();
        RefreshUserInfo();
    }

    [ObservableProperty]
    private UserInfo currentUserInfo;

    [ObservableProperty]
    private byte unreads;

    [ObservableProperty]
    private ObservableCollection<ItemTobeSold> itemsCollection = [];

    [ObservableProperty]
    private ObservableCollection<ItemSold> salesCollection = [];

    [ObservableProperty]
    private ObservableCollection<MsgVM> currentMessages = [];

    [ObservableProperty]
    private ObservableCollection<ChatListVM> chats = [];

    [RelayCommand]
    private void StartListening()
    {
        //var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
        //var user = queryer.GetUser("user00000001");
        //if (user != null)
        //    MessageBox.Success(user.Nickname);
    }

    /// <summary>
    /// 每隔一秒，该方法会线程独立地自动刷新用户信息
    /// </summary>
    public void RefreshUserInfo()
    {
        if (CurrentUserInfo != null)
        {
            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
            CurrentUserInfo = userCenter.GetCurrentUserInfo();

        }
        //Task.Run(() =>
        //{
        //    while (true)
        //    {
                
        //        Thread.Sleep(3000);
        //    }
        //});
    }

    [RelayCommand]
    private void LoadSpecificPage(object sender)
    {
        if (sender is HandyControl.Controls.TabControl tabControl)
        {
            

            switch (tabControl.SelectedIndex)
            {
                case 0:
                    {
                        break;
                    }
                //切换到第1页，需要刷新商品信息
                case 1:
                    {
                        try
                        {
                            UserCenter userCenter2 = _provider.GetRequiredService<UserCenter>();

                            ItemsCollection.Clear();
                            var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
                            var items = queryer.GetItems(userCenter2.CurrentUser);
                            if (items != null)
                                foreach (var i in items)
                                {
                                    var it = new ItemTobeSold(_provider);
                                    Utilities.Copy(i, it);
                                    ItemsCollection.Add(it);
                                }
                            break;
                        }
                        catch (InvalidOperationException ex)
                        {
                            MessageBox.Error(ex.Message);
                            return;
                        }
                    }
                case 2:
                    {
                        try
                        {
                            SalesCollection.Clear();
                            UserCenter userCenter1 = _provider.GetRequiredService<UserCenter>();
                            var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
                            var items = queryer.GetAllItems(userCenter1.CurrentUser);
                            if (items != null)
                                foreach (var i in items)
                                {
                                    //自己出售的商品、状态不正常的商品不会出现在检索中
                                    if(i.SellerId != userCenter1.CurrentUser.Id && i.ItemStatus == "ONSALE")
                                    {
                                        var it = new ItemSold(_provider);
                                        Utilities.Copy(i, it);
                                        it.SellerNickname = queryer.GetUser(it.SellerId, userCenter1.CurrentUser)!.Nickname;
                                        SalesCollection.Add(it);
                                    }

                                }
                            break;
                        }
                        catch (InvalidOperationException ex)
                        {
                            MessageBox.Error(ex.Message);
                            return;
                        }
                    }
                case 3:
                    var _queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
                    UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
                    var msgs = _queryer.GetChats(userCenter.CurrentUser);
                    if (msgs != null)
                    {
                        Chats.Clear();
                        Unreads = 0;
                        foreach (var c in msgs)
                        {
                            var cvm = ChatListVM.Create(c, _provider, userCenter.CurrentUser);
                            Chats.Add(cvm);
                            foreach (var ms in c.Messages)
                            {
                                if (ms.Unread == 0)
                                {
                                    Unreads++;
                                }

                            }
                            //if (c.SellerId == CurrentUserInfo.Id)
                            //{

                            //}
                            //else
                            //{

                            //}
                        }
                        //if(unreadCount > 0 )
                        //{
                        //    Growl.Info($"您有{unreadCount}条未读消息.");
                        //}
                    }
                    break;
                default:
                    break;
            }
            RefreshUserInfo();
        }
    }

    /// <summary>
    ///
    /// </summary>
    [RelayCommand]
    private void AddItemtobeSold()
    {
        using var serviceScope = _provider.CreateScope();

        var it = new ItemTobeSold(_provider);
        var addWindow = new AddItemTobeSoldWindow(it);
        if (addWindow != null)
        {
            if (addWindow.ShowDialog() == true)
            {
                ItemsCollection.Add(it);
                UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
                var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
                var item = new Item();
                Utilities.Copy(it, item);
                queryer.AddItem(item, userCenter.CurrentUser);
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    [RelayCommand]
    private void ListAllItem()
    {
        UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
        var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();

        var users = queryer.GetAllUsers(userCenter.CurrentUser);
        //if (ItemsCollection.Count > 0)
        //{
        //    foreach (var i in ItemsCollection)
        //    {
        //        var item = new Item();
        //        Utilities.Copy(i, item);
        //        UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
        //        var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
        //        queryer.AddItem(item, userCenter.CurrentUser);
        //    }
        //}
    }

    [RelayCommand]
    private void LoadMsgOfChats(object sender)
    {
        if(sender is ListBox listbox)
        {
            CurrentMessages.Clear();
            var currentChat = Chats[listbox.SelectedIndex];
            foreach(var m in currentChat.Messages)
            {
                var msgVm = new MsgVM(_provider);
                Utilities.Copy(m, msgVm);
                CurrentMessages.Add(msgVm);
            }
        }
    }
}