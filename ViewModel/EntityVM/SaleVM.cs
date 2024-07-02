using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model;
using FirewallDemo.Model.Data;
using FirewallDemo.Security;
using FirewallDemo.Utility;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
#nullable disable

namespace FirewallDemo.ViewModel.EntityVM;

public partial class SaleVM : ObservableObject
{
    private readonly IServiceProvider _provider;
    [ObservableProperty]
    private string saleId;

    [ObservableProperty]
    private string itemId;

    [ObservableProperty]
    private string customerId;

    [ObservableProperty]
    private string sellerId;

    [ObservableProperty]
    private string sellerNickname;

    [ObservableProperty]
    private decimal realPrice;

    [ObservableProperty]
    private DateTime timestamp;

    [ObservableProperty]
    private string status;

    [ObservableProperty]
    private Item item;

    [ObservableProperty]
    private bool isOperatable = false;

    [ObservableProperty]
    private bool isAdmin;

    public void SetOperatable()
    {
        if (Status.Contains("Closed") || Status == "Success")
        {
            IsOperatable = false;
        }
        else
            IsOperatable = true;
    }

    [RelayCommand]
    private void AcceptSale()
    {
        var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
        UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
        var msgCenter = _provider.GetRequiredService<MessageCenter>();
        if(Status == "Warned")
        {
            if (MessageBox.Ask("该笔交易可能存在问题，已被管理员警告。\n仍要继续?") != System.Windows.MessageBoxResult.OK)
                return;
        }
        var prevSale = queryer.GetSale(SaleId, userCenter.CurrentUser);
        prevSale.Item = null;
        prevSale.ItemId = null;
        
        if (prevSale.RealPrice != RealPrice)
        {
            if (MessageBox.Ask("提出的实价与标价不相同。是否继续?") == System.Windows.MessageBoxResult.OK)
            {
                Sale sale = new();/*queryer.GetSale(SaleId, userCenter.CurrentUser);*/
                if(sale != null)
                {
                    Status = SellerId == userCenter.CurrentUser.Id ?
                        SaleStatus.WaitingforBuyer.ToString("F")
                        : SaleStatus.WaitingforSeller.ToString("F");
                    Utilities.Copy(this, sale);

                    queryer.UpdateSale(sale, userCenter.CurrentUser);

                    var chat = msgCenter.GetChat(CustomerId, SellerId, userCenter.CurrentUser);
                    msgCenter.SendMessage("我已修改价格，等待你确认", chat.ChatId, userCenter.CurrentUser);

                }
            }
            return;
        }
        if(CustomerId == userCenter.CurrentUser.Id)
        {
            MessageBox.Info("您是买家，不可提前付款哦!");
            return;
        }
        //否则确定交易，交易成功
        if (MessageBox.Ask("交易成功后不可退款.确定吗?") == System.Windows.MessageBoxResult.OK)
        {
            queryer.FinishSale(SaleId, userCenter.CurrentUser);
            MessageBox.Success("交易成功，钱款已到账户!");
            //刷新UI
            var mainVM= _provider.GetRequiredService<MainVM>();
            mainVM.RefreshUserInfo();
        }
    }

    [RelayCommand]
    private void RejectSale()
    {
        try
        {
            if (MessageBox.Ask("确定要拒绝该笔交易吗?") == System.Windows.MessageBoxResult.OK)
            {
                var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
                UserCenter userCenter = _provider.GetRequiredService<UserCenter>();

                //根据用户身份，设置不同的交易关闭状态
                if (SellerId != userCenter.CurrentUser.Id)
                {
                    queryer.SetSaleStatus(SaleId, SaleStatus.ClosedByCustomer, userCenter.CurrentUser);
                }
                else
                {
                    queryer.SetSaleStatus(SaleId, SaleStatus.ClosedBySeller, userCenter.CurrentUser);
                }
                var msgCenter = _provider.GetRequiredService<MessageCenter>();
                var chat = msgCenter.GetChat(CustomerId, SellerId, userCenter.CurrentUser);
                msgCenter.SendMessage("我拒绝了你的交易请求", chat.ChatId, userCenter.CurrentUser);

                MessageBox.Success("拒绝交易成功!");
                //刷新UI
                var mainVM = _provider.GetRequiredService<MainVM>();
                mainVM.RefreshUserInfo();
            }
        }
        catch(Exception ex)
        {
            MessageBox.Error(ex.Message);
        }


    }

    [RelayCommand]
    private void WarnSale()
    {
        try
        {
            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();

            if (!IsAdmin || userCenter.CurrentUser.Role != "ADMIN")
            {
                MessageBox.Error("仅管理员可进行该操作.\n若无意看到该对话框，请联系管理员.");
                return;
            }
            if(Status.Contains("Closed") || Status == "Success")
            {
                MessageBox.Error("该交易已关闭.");
                return;
            }
            var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
            queryer.SetSaleStatus(SaleId, SaleStatus.Warned, userCenter.CurrentUser);
            var msgCenter = _provider.GetRequiredService<MessageCenter>();
            var chat = msgCenter.GetChat(CustomerId, SellerId, userCenter.CurrentUser);
            msgCenter.SendMessage("你的商品可能存在问题，我向你提出警告", chat.ChatId, userCenter.CurrentUser);
            Growl.Info("成功发出警告.");
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
        }
    }

    [RelayCommand]
    private void ForceDelistSale()
    {
        try
        {
            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();

            if (!IsAdmin || userCenter.CurrentUser.Role != "ADMIN")
            {
                MessageBox.Error("仅管理员可进行该操作.\n若无意看到该对话框，请联系管理员.");
                return;
            }
            if (Status.Contains("Closed") || Status == "Success")
            {
                MessageBox.Error("该交易已关闭.");
                return;
            }
            var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
            queryer.SetSaleStatus(SaleId, SaleStatus.ClosedByOperator, userCenter.CurrentUser);
            var msgCenter = _provider.GetRequiredService<MessageCenter>();
            var chat = msgCenter.GetChat(CustomerId, SellerId, userCenter.CurrentUser);
            msgCenter.SendMessage("你的商品可能存在问题，我关闭了你的交易", chat.ChatId, userCenter.CurrentUser);
            Growl.Info("成功下架该商品.");
            //刷新UI
            var mainVM = _provider.GetRequiredService<MainVM>();
            mainVM.RefreshUserInfo();
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
        }
    }

    public SaleVM(IServiceProvider provider)
    {
        _provider = provider;
    }
}
