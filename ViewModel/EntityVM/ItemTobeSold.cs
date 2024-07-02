using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model.Data;
using FirewallDemo.Model;
using FirewallDemo.Security;
using FirewallDemo.Utility;
using FirewallDemo.View;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
#nullable disable

namespace FirewallDemo.ViewModel.EntityVM;

public partial class ItemTobeSold(IServiceProvider provider) : ObservableObject
{
    private readonly IServiceProvider _provider = provider;

    [ObservableProperty]
    private string itemId;

    [ObservableProperty]
    private string itemName;

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
    private bool isDelisted;

    [ObservableProperty]
    private DateTime listingTimestamp;

    [ObservableProperty]
    private DateTime? delistingTimestamp;

    [ObservableProperty]
    private ObservableCollection<string> typeCollection = [];

    [RelayCommand]
    private void SetPicture(object sender)
    {
        if(sender is ImageSelector imageSelector)
        {
            Picture = imageSelector.Uri.LocalPath;
        }
    }

    [RelayCommand]
    private void ItemComplete(object sender)
    {
        try
        {
            if(sender is System.Windows.Window wnd)
            {
                //对商品各项信息进行检查
                if (Summary == null || Summary.Length >= 200)
                {
                    throw new ArgumentNullException(nameof(Summary));
                }
                if(ItemName == null)
                {
                    throw new ArgumentNullException(nameof(ItemName));
                }
                if(Summary.Length >= 200 || ItemName.Length >= 200)
                {
                    throw new ArgumentException("商品介绍或商品名过长.");
                }
                if (Picture == null)
                {
                    throw new ArgumentNullException(Picture);
                }
                ItemName ??= Summary.Length > 15 ? Summary[..15] : Summary;
                //并自动生成一部分信息
                ItemStatus = "ONSALE";
                //测试触发器自动更新
                ListingTimestamp = DateTime.UtcNow;
                var userCenter = _provider.GetRequiredService<UserCenter>();
                SellerId = userCenter.GetCurrentUserInfo().Id;

                ItemId ??= Guid.NewGuid().ToString()[..12];
                wnd.DialogResult = true;
            }
        }
        catch(ArgumentNullException ex)
        {
            MessageBox.Error($"{ex.ParamName}为空.", "错误");
            return;
        }
        catch(ArgumentException ex)
        {
            MessageBox.Error(ex.Message);
            return;
        }

    }

    public void SetDelisted()
    {
        IsDelisted = true;
    }

    [RelayCommand]
    private void ModifyItem()
    {
        var wnd = new AddItemTobeSoldWindow(this);
        if(wnd.ShowDialog() == true)
        {
            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
            var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
            var item = new Item();
            Utilities.Copy(this, item);
            if(queryer.CheckItemStatus(item.ItemStatus) == Model.Data.ItemStatus.SOLD ||
                queryer.CheckItemStatus(item.ItemStatus) == Model.Data.ItemStatus.DELISTED)
                SetDelisted();
            if(queryer.ModifyItem(item, userCenter.CurrentUser))
                Growl.Success("商品编辑成功!");

        }
    }

    /// <summary>
    /// 下架商品。这个操作不会真正删除一条记录。
    /// </summary>
    [RelayCommand]
    private void DelistItem()
    {
        try
        {
            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
            if (ItemStatus == "DELISTED" || ItemStatus == "SOLD")
            {
                MessageBox.Error("该商品已下架.");
                return;
            }
            var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
            SetDelisted();
            queryer.SetItemStatus(ItemId, Model.Data.ItemStatus.DELISTED, userCenter.CurrentUser);
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

    [RelayCommand]
    private void RelistItem()
    {
        try
        {
            if (ItemStatus == "ONSALE" || ItemStatus == "WARNED")
            {
                MessageBox.Error("该商品已上架.");
                return;
            }
            if (ItemStatus == "SOLD")
            {
                MessageBox.Error("已售出商品不可重新上架.");
                return;
            }
            UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
            var queryer = _provider.GetRequiredService<DataQueryerForCustomer>();
            SetDelisted();
            queryer.SetItemStatus(ItemId, Model.Data.ItemStatus.ONSALE, userCenter.CurrentUser);
            Growl.Info("成功上架该商品.");
            //刷新UI
            var mainVM = _provider.GetRequiredService<MainVM>();
            mainVM.RefreshUserInfo();
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
        }
    }
}
