using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirewallDemo.Model;
using FirewallDemo.Security;
using FirewallDemo.View;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace FirewallDemo.ViewModel;

public partial class AdminOperateVM(IServiceProvider provider) : ObservableObject
{
    private readonly IServiceProvider _provider = provider;

    [ObservableProperty]
    private DateTime startTime = DateTime.Now;

    [ObservableProperty]
    private DateTime endTime = DateTime.Now;

    [RelayCommand]
    private void GenerateReport()
    {
        StringBuilder sb = new();
        sb.AppendLine("数据汇总");
        sb.AppendLine($"自{StartTime}至{EndTime}以来:");
        var queryer = _provider.GetRequiredService<DataQueryerForAdmin>();
        UserCenter userCenter = _provider.GetRequiredService<UserCenter>();
        var sales = queryer.GetSales(userCenter.CurrentUser)!
            .Where(s => s.Timestamp > StartTime && s.Timestamp < EndTime);
        if(sales != null)
        {
            var saleCount = sales
                .Count();
            sb.AppendLine($"共有{saleCount}条交易记录.");
            var saleGrouped = sales
                .GroupBy(s => s.Item.Type);
            foreach(var g in saleGrouped)
            {
                sb.AppendLine($"{g.Key} 类: {g.Count()}件");
            }
        }
        else
        {
            sb.AppendLine("无任何成交记录.");
        }
        var strClip = sb.ToString();
        Clipboard.SetText(strClip);
        sb.AppendLine("上述报告已复制到剪贴板.");
        MessageBox.Info(sb.ToString());
    }

    [RelayCommand]
    private void ManageSales()
    {
        var saleWindow = _provider.GetRequiredService<SaleOperateWindow>();
        var saleOperateVM = saleWindow.DataContext as SaleOperateVM;
        var queryer = _provider.GetRequiredService<DataQueryerForAdmin>();
        UserCenter userCenter = _provider.GetRequiredService<UserCenter>();

        var saleList = queryer.GetSales(userCenter.CurrentUser);
        if (saleList != null && saleOperateVM is SaleOperateVM vm)
        {
            
            vm.ImportSales(saleList, true);
            //saleList = null;
            //GC.Collect();
            saleWindow.Show();
        }
    }

    [RelayCommand]
    private void VerifyTime()
    {
        if (StartTime - EndTime > TimeSpan.Zero)
        {
            MessageBox.Error("起始时间不能晚于结束时间!");
            StartTime = EndTime.AddHours(-1);
        }
    }
}

