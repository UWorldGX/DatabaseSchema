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
                if (Summary == null)
                {
                    throw new ArgumentNullException(Summary);
                }
                if (Picture == null)
                {
                    throw new ArgumentNullException(Picture);
                }
                ItemName ??= Summary.Length > 15 ? Summary[..15] : Summary;
                //并自动生成一部分信息
                ItemStatus = "ONSALE";
                ListingTimestamp = DateTime.Now;
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
            queryer.ModifyItem(item, userCenter.CurrentUser);
        }
    }

    [RelayCommand]
    private void DelistItem()
    {
        //TODO:下架商品。需要在DataQueryer写逻辑。必须注意外键约束。这个操作不会真正删除一条记录。
    }
}
