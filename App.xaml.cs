using FirewallDemo.Model;
using FirewallDemo.Model.Data;
using FirewallDemo.Model.UserManage;
using FirewallDemo.Security;
using FirewallDemo.View;
using FirewallDemo.ViewModel;
using FirewallDemo.ViewModel.EntityVM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog.Extensions.Logging;
using System;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace FirewallDemo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;
    private IConfigurationRoot root = null!;

    //1 封装构造函数
    public App()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ServiceCollection services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    //2 封装服务注入
    private void ConfigureServices(ServiceCollection services)
    {
        var builder = new ConfigurationBuilder();
        services.AddSingleton<HttpClient>();
        services.AddDbContextFactory<xpertContext>(option =>
        {
            option.UseMySql(System.Configuration.ConfigurationManager.ConnectionStrings["connStr"].ConnectionString, new MySqlServerVersion(new Version(8, 0, 35)))
            .EnableDetailedErrors();
        }, ServiceLifetime.Scoped);
        //程序内用户和当前用户管理器
        services.AddScoped<UserCenter>();
        //公钥基础设施
        services.AddTransient<PKI>();
        //主窗口
        services.AddSingleton<MainVM>();
        services.AddScoped<MainWindow>();
        //登录窗口与登录组件
        services.AddTransient<RegisterManager>();
        services.AddTransient<LoginManager>();
        services.AddScoped<LoginVM>();
        services.AddScoped<LoginWindow>();

        //为用户使用的查询类
        services.AddTransient<DataQueryerForCustomer>();
        //为管理员使用的查询类
        services.AddTransient<DataQueryerForAdmin>();
        services.AddLogging(logBuilder =>
        {
            logBuilder.AddNLog();
        });
    }

    //3 重写OnStartUp函数
    protected override void OnStartup(StartupEventArgs e)
    {
        var window = _serviceProvider.GetRequiredService<LoginWindow>();
        window.ShowDialog();
    }

}
