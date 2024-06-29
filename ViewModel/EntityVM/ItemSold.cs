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
            var pki = _provider.GetRequiredService<PKI>();

            //新建一条销售记录，设置各项信息
            Sale sale = new()
            {
                SaleId = "SL-" + Guid.NewGuid().ToString()[..12],
                ItemId = this.ItemId,
                CustomerId = userCenter.CurrentUser.Id,
                SellerId = this.SellerId,
                RealPrice = this.SellPrice,
                Timestamp = DateTime.Now,
                Status = SaleStatus.WaitingforSeller.ToString("F")
            };
            ChatList chatList = new()
            {
                SellerId = this.SellerId,
                CustomerId = userCenter.CurrentUser.Id,
                ChatId = "SL-" + Guid.NewGuid().ToString()[..12]
            };
            Message msg = new()
            {
                Content = "我要买下您的宝贝，等待您的回复.",
                Unread = 0,
                ChatId = chatList.ChatId,
                Timestamp = sale.Timestamp,
                MsgId = "MSG-" + Guid.NewGuid().ToString()[..12]
            }; 

            //修改原有item状态: 售出下架
            Item item = new();
            Utilities.Copy(this, item);
            item.DelistingTimestamp = DateTime.Now;
            item.ItemStatus = "SOLD";
            queryer.ModifyItem(item, userCenter.CurrentUser);
            queryer.AddSale(sale, userCenter.CurrentUser);
            queryer.AddChat(chatList, userCenter.CurrentUser);
            queryer.AddMessage(msg, userCenter.CurrentUser);

            Growl.Success("已发送请求，等待卖家回复.");
            //TODO: 发出信息，刷新商品列表
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
        }
    }

    [RelayCommand]
    private void ChatToSeller()
    {
        try
        {
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
        }
    }
}