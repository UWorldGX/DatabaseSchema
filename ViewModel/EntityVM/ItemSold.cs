using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model;
using FirewallDemo.Model.Data;
using FirewallDemo.Security;
using FirewallDemo.Utility;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;

#nullable disable

namespace FirewallDemo.ViewModel.EntityVM;

/// <summary>
/// 在商品检索中出现的商品VM
/// </summary>
/// <param name="provider"></param>
public partial class ItemSold(IServiceProvider provider) : ObservableObject
{
    private readonly IServiceProvider _provider = provider;

    [ObservableProperty]
    private string itemId;

    [ObservableProperty]
    private string itemName;

    [ObservableProperty]
    private string sellerNickname;

    [ObservableProperty]
    private string type;

    [ObservableProperty]
    private string sellerId;

    [ObservableProperty]
    private decimal sellPrice;

    [ObservableProperty]
    private string summary;

    [ObservableProperty]
    private string picture;

    [ObservableProperty]
    private string itemStatus;

    [ObservableProperty]
    private DateTime listingTimestamp;

    [ObservableProperty]
    private DateTime? delistingTimestamp;

    [RelayCommand]
    private void SendPurchase()
    {
        try
        {
            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
            var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
            var msgCenter = _provider.GetRequiredService<MessageCenter>();
            var pki = _provider.GetRequiredService<PKI>();
            //用户余额不足则不允许购买
            if(userCenter.CurrentUser.Money < SellPrice)
            {
                Growl.ErrorGlobal("啊呀! 余额不够......");
                return;
            }

            //新建一条销售记录，设置各项信息
            Sale sale = new()
            {
                ItemId = this.ItemId,
                CustomerId = userCenter.CurrentUser.Id,
                SellerId = this.SellerId,
                RealPrice = this.SellPrice,
                Timestamp = DateTime.Now,
                Status = SaleStatus.WaitingforSeller.ToString("F")
            };
            //{
            //    Content = "我要买下您的宝贝，等待您的回复.",
            //    Unread = 0,
            //    SenderId = userCenter.CurrentUser.Id,
            //    ChatId = chatList.ChatId,
            //    Timestamp = sale.Timestamp,
            //    MsgId = "MSG-" + Guid.NewGuid().ToString()[..12]
            //}; 

            //修改原有item状态: 售出下架
            Item item = new();
            Utilities.Copy(this, item);
            item.DelistingTimestamp = DateTime.Now;
            item.ItemStatus = "SOLD";
            if(queryer.ModifyItem(item, userCenter.CurrentUser))
            {
                if (queryer.AddSale(sale, userCenter.CurrentUser))
                {
                    ChatList chatList = msgCenter.CreateChat(SellerId, "customer", userCenter.CurrentUser);
                    Message msg = msgCenter.SendMessage("我要买下您的宝贝，等待您的回复.", chatList.ChatId, userCenter.CurrentUser);


                    MessageBox.Success("已发送请求，等待卖家回复.");
                    //刷新UI
                    var mainVm = _provider.GetRequiredService<MainVM>();
                    mainVm.RefreshUserInfo();
                }
            }

            
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
        }
    }

    /// <summary>
    /// 未使用的方法，作为快捷与上架聊天的预留
    /// </summary>
    //[RelayCommand]
    //private void ChatToSeller()
    //{
    //    try
    //    {
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Error(ex.Message);
    //    }
    //}
}