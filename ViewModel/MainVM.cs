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

    public MainVM(IServiceProvider provider, UserCenter userCenter, 
        FilterVM vM
        ,AdminOperateVM avM)
    {
        _provider = provider;
        filterVM = vM;
        adminOperateVM = avM;
        CurrentUserInfo = userCenter.GetCurrentUserInfo();
        RefreshUserInfo();
        if(userCenter.CurrentUser.Role == "ADMIN")
        {
            IsAdmin = true;
        }
    }

    [ObservableProperty]
    private UserInfo currentUserInfo;
    [ObservableProperty]
    private bool isAdmin = false;
    [ObservableProperty]
    private int currentMoney;

    [ObservableProperty]
    private byte unreads;
    [ObservableProperty]
    private string msgContent = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ItemTobeSold> itemsCollection = [];

    [ObservableProperty]
    private ObservableCollection<ItemSold> salesCollection = [];

    [ObservableProperty]
    private ObservableCollection<MsgVM> currentMessages = [];

    [ObservableProperty]
    private ObservableCollection<ChatListVM> chats = [];

    [ObservableProperty]
    private FilterVM filterVM;

    [ObservableProperty]
    private AdminOperateVM adminOperateVM;

    private bool isChatting = false;

    //[RelayCommand]
    //private void StartListening()
    //{
    //    //var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
    //    //var user = queryer.GetUser("user00000001");
    //    //if (user != null)
    //    //    MessageBox.Success(user.Nickname);
    //}

    /// <summary>
    /// 刷新信息已读数
    /// </summary>
    public void RefreshUnread()
    {
        Unreads = 0;
        if (Chats != null)
            foreach(var c in Chats)
            {
                foreach(var ms in c.Messages)
                    if (ms.Unread == 0)
                    {
                        Unreads++;
                    }
            }
    }
    public void RefreshMessage()
    {
        UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
        var msgCenter = _provider.GetRequiredService<MessageCenter>();
        var msgs = msgCenter.GetChats(userCenter.CurrentUser);
        if (msgs != null)
        {
            Chats.Clear();
            //注意此处整合了RefreshUnread的功能
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
            }
        }
    }

    /// <summary>
    /// 该方法刷新用户信息和其他UI信息
    /// </summary>
    public void RefreshUserInfo()
    {
        try
        {
            isChatting = false;
            //刷新用户信息
            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
            if (CurrentUserInfo != null)
            {
                CurrentUserInfo = userCenter.GetCurrentUserInfo();
                CurrentMoney = userCenter.GetCurrentMoney();
            }
            RefreshMessage();
            //刷新自上架商品信息
            ItemsCollection.Clear();
            var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
            var items = queryer.GetItems(userCenter.CurrentUser);
            if (items != null)
                foreach (var i in items)
                {
                    var it = new ItemTobeSold(_provider);
                    Utilities.Copy(i, it);
                    if (it.ItemStatus == "DELISTED" || it.ItemStatus == "SOLD")
                        it.SetDelisted();
                    ItemsCollection.Add(it);
                }

            //刷新正在销售商品信息
            SalesCollection.Clear();
            var allItems = queryer.GetAllItems(userCenter.CurrentUser);
            if (allItems != null)
                foreach (var i in allItems)
                {
                    //自己出售的商品、状态不正常的商品不会出现在检索中
                    if (i.SellerId != userCenter.CurrentUser.Id && i.ItemStatus == "ONSALE")
                    {
                        var it = new ItemSold(_provider);
                        Utilities.Copy(i, it);
                        it.SellerNickname = queryer.GetUser(it.SellerId, userCenter.CurrentUser)!.Nickname;
                        SalesCollection.Add(it);
                    }

                }
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Error(ex.Message);
            return;
        }
        }

    public void Filter(Func<IEnumerable<ItemSold>, IQueryable<ItemSold>> filterExpr)
    {
        //调用传入的过滤表达式，执行过滤
        //这操作可实现多重过滤
        var filteredByType = filterExpr.Invoke(SalesCollection).ToArray();
        if (filteredByType.Length > 0)
        {
            var newList = new ObservableCollection<ItemSold>();
            SalesCollection.Clear();
            foreach (var i in filteredByType)
                newList.Add(i);
            Growl.Info($"共查找出{newList.Count}件商品.");
            SalesCollection = newList;
        }
        else
        {
            Growl.Error("没有符合条件的商品.");
        }
    }

    public void ClearFilter()
    {
        //清除所有筛选条件
        SalesCollection.Clear();
        UserCenter userCenter2 = _provider.GetRequiredService<UserCenter>();
        var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
        var items = queryer.GetAllItems(userCenter2.CurrentUser);
        if (items != null)
            foreach (var i in items)
            {
                var it = new ItemSold(_provider);
                Utilities.Copy(i, it);
                SalesCollection.Add(it);
            }
    }


    [RelayCommand]
    private void LoadSpecificPage(object sender)
    {
        if (isChatting)
            return;
        if (sender is HandyControl.Controls.TabControl tabControl)
        {
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
                if(queryer.AddItem(item, userCenter.CurrentUser))
                {
                    Growl.Success("上架商品成功!");
                }
            }
        }
    }

    ///// <summary>
    /////
    ///// </summary>
    //[RelayCommand]
    //private void ListAllItem()
    //{
    //    UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
    //    var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();

    //    var users = queryer.GetAllUsers(userCenter.CurrentUser);
    //    //if (ItemsCollection.Count > 0)
    //    //{
    //    //    foreach (var i in ItemsCollection)
    //    //    {
    //    //        var item = new Item();
    //    //        Utilities.Copy(i, item);
    //    //        UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
    //    //        var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
    //    //        queryer.AddItem(item, userCenter.CurrentUser);
    //    //    }
    //    //}
    //}

    /// <summary>
    /// 加载与特定用户的对话记录，并自动设置信息已读
    /// </summary>
    /// <param name="sender"></param>
    [RelayCommand]
    private void LoadMsgOfChats(object sender)
    {
        if(sender is ListBox listbox)
        {
            if (listbox.SelectedIndex == -1)
                return;
            isChatting = true;
            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
            var msgCenter = _provider.GetRequiredService<MessageCenter>();
            CurrentMessages.Clear();
            var currentChat = Chats[listbox.SelectedIndex];
            foreach(var m in currentChat.Messages)
            {
                m.Unread = 1;
                msgCenter.SetMessageRead(m.MsgId, userCenter.CurrentUser);
                CurrentMessages.Add(m);
            }
            RefreshUnread();
        }
    }

    /// <summary>
    /// 向对方发出信息
    /// </summary>
    [RelayCommand]
    private void Chat()
    {
        try
        {
            MsgVM msgModel = new(_provider)
            {
                Unread = 0,
                SenderId = CurrentUserInfo.Id,
                Role = "Sender",
                ChatId = CurrentMessages.First().ChatId,
                Timestamp = DateTime.Now,
                Content = MsgContent,
                MsgId = "MSG-" + Guid.NewGuid().ToString()[..12]
            };
            var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();

            var msgCenter = _provider.GetRequiredService<MessageCenter>();
            Message msg = msgCenter.SendMessage(MsgContent, CurrentMessages.First().ChatId, userCenter.CurrentUser);
            //Utilities.Copy(msgModel, msg);
            CurrentMessages.Add(msgModel);
            //msgCenter.SendMessage(MsgContent, msg.ChatId, userCenter.CurrentUser);
            MsgContent = string.Empty;


        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
        }
    }
    [RelayCommand]
    private void SearchAllSales()
    {
        var saleWindow = _provider.GetRequiredService<SaleOperateWindow>();
        var saleOperateVM = saleWindow.DataContext as SaleOperateVM;
        var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
        UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
        
        var saleList = queryer.GetSales(userCenter.CurrentUser);
        if(saleList != null && saleOperateVM is SaleOperateVM vm)
        {
            vm.ImportSales(saleList, false);
            saleList = null;
            GC.Collect();
            saleWindow.Show();
        }

    }

    [RelayCommand]
    private void SearchCurrentSales()
    {
        var saleWindow = _provider.GetRequiredService<SaleOperateWindow>();
        var saleOperateVM = saleWindow.DataContext as SaleOperateVM;
        var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
        UserCenter userCenter = _provider.GetRequiredService<UserCenter>();

        var saleList = queryer.GetSales(userCenter.CurrentUser);
        if(saleList != null)
        {
            var list2 = saleList.Where(S => S.Status != SaleStatus.Success.ToString("F") && !(S.Status.Contains("Closed")))
            .Select(s => s).ToArray();
            if (list2 != null && saleOperateVM is SaleOperateVM vm)
            {
                vm.ImportSales(list2,false);

                saleWindow.Show();
            }
        }


    }

    [RelayCommand]
    private void ModifyUserInfo()
    {
        RegisterWindow regiWindow = _provider.GetRequiredService<RegisterWindow>();
        var vm = regiWindow.DataContext as RegisterVM;
        vm?.ModifyUser(CurrentUserInfo);

        if (regiWindow.ShowDialog() == true)
        {
            MessageBox.Success("用户信息修改成功.");
        }
    }
}