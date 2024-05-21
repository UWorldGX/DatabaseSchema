using FirewallDemo.View;
using FirewallDemo.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Data;
using System.Net.Http;
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
        //using HttpClient httpClient = new();
        //HttpRequestMessage request = new(HttpMethod.Get, "http://150.158.40.245");
        //builder.AddJsonStream(httpClient.Send(request).Content.ReadAsStream());
        //builder.AddJsonFile("config.json", optional: false, reloadOnChange: true);
        //root = builder.Build();

        //services.AddOptions()
        //    .Configure<Model.MemoManager>(root.Bind);
        //services.AddDbContextFactory<InnoContext>(option =>
        //{
        //    option.UseMySql(System.Configuration.ConfigurationManager.ConnectionStrings["connStr"].ConnectionString, new MySqlServerVersion(new Version(8, 0, 35)))
        //    .EnableDetailedErrors();
        //}, ServiceLifetime.Scoped);
        services.AddSingleton<MainVM>();

        services.AddScoped<MainWindow>();
    }

    //3 重写OnStartUp函数
    protected override void OnStartup(StartupEventArgs e)
    {
        var window = _serviceProvider.GetRequiredService<MainWindow>();
        window.Show();
    }
}
