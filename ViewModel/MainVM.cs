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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FirewallDemo.ViewModel;

public partial class MainVM : ObservableObject
{
    private readonly IServiceProvider _provider;

    public MainVM(IServiceProvider provider, UserCenter userCenter)
    {
        _provider = provider;
        CurrentUserInfo = userCenter.GetCurrentUserInfo();
        RefreshUserInfoAsync();
    }

    [ObservableProperty]
    private UserInfo currentUserInfo;

    [ObservableProperty]
    private ObservableCollection<ItemTobeSold> itemsCollection = [];

    [ObservableProperty]
    private ObservableCollection<ItemSold> salesCollection = [];

    [ObservableProperty]
    private ObservableCollection<MsgVM> unreadMessages = [];

    [ObservableProperty]
    private ObservableCollection<ChatList> chats = [];

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
    private async void RefreshUserInfoAsync()
    {
        await Task.Run(() =>
        {
            while (true)
            {
                if (CurrentUserInfo != null)
                {
                    UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
                    CurrentUserInfo = userCenter.GetCurrentUserInfo();

                    using var serviceScope = _provider.CreateScope();
                    var _queryer = serviceScope.ServiceProvider.GetRequiredService<DataQueryerForCustomer>();
                    var msgs = _queryer.GetChats(userCenter.CurrentUser);
                    if(msgs != null)
                    {
                        foreach(var c in msgs)
                        {
                            Chats.Add(c);
                            foreach(var ms in c.Messages)
                            {
                                if ((ms.Timestamp - DateTime.Now).Seconds <= 3)
                                {
                                    var msgVm = new MsgVM(_provider);
                                    Utilities.Copy(ms, msgVm);
                                    UnreadMessages.Add(msgVm);
                                }
                                    
                            }
                        }
                    }

                }
                Thread.Sleep(3000);
            }
        });
    }

    [RelayCommand]
    private void LoadSpecificPage(object sender)
    {
        if (sender is TabControl tabControl)
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
                            ItemsCollection.Clear();
                            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
                            var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
                            var items = queryer.GetItems(userCenter.CurrentUser);
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
                            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
                            var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
                            var items = queryer.GetAllItems(userCenter.CurrentUser);
                            if (items != null)
                                foreach (var i in items)
                                {
                                    //自己出售的商品、状态不正常的商品不会出现在检索中
                                    if(i.SellerId != userCenter.CurrentUser.Id && i.ItemStatus == "ONSALE")
                                    {
                                        var it = new ItemSold(_provider);
                                        Utilities.Copy(i, it);
                                        it.SellerNickname = queryer.GetUser(it.SellerId, userCenter.CurrentUser)!.Nickname;
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
                default:
                    break;
            }
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
}