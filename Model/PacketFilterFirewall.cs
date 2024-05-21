using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HandyControl.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace FirewallDemo.Model;

public partial class PacketFilterFirewall : ObservableObject
{
    private TcpListener _listener;

    [ObservableProperty]
    private ObservableCollection<string> allowedPorts = ["80", "443", "47989", "8888"]; // 允许的端口列表

    public PacketFilterFirewall(IPAddress localAddr, int port)
    {
        try
        {
            _listener = new TcpListener(localAddr, port);
            _listener.Start(32);
        }
        catch (SocketException sex)
        {
            MessageBox.Error($"SocketException: {sex.Message}", sex.ErrorCode.ToString());
        }
    }

    public void Filter()
    {
        while (true)
        {
            //byte[] buffer = new byte[1024];
            NetworkStream clientStream = null;
            bool isAllowed = false;
            try
            {
                clientStream = _listener.AcceptTcpClient().GetStream(); // 接受新的连接请求
                string message = new StreamReader(clientStream).ReadToEnd(); // 读取客户端发送的数据
                foreach (var port in AllowedPorts)
                {
                    if (message != null && message.Contains(port))
                    {
                        // 如果数据在允许的端口列表中，允许它通过并发送给客户端响应
                        MessageBox.Success($"防火墙消息: 在端口检测到允许的连接, 内容如下:" +
                            $"{message}", "提示");
                        string response = "Port is allowed"; // 发送给客户端的响应消息
                        StreamWriter writer = new StreamWriter(clientStream); // 写入响应消息到客户端流中
                        writer.Write(response); // 发送响应消息给客户端
                        writer.Close(); // 关闭流并丢弃连接
                        isAllowed = true;
                        break; // 重试直到接受一个新的合法连接请求或所有请求都已处理完毕
                    }
                }
                if(!isAllowed)
                {
                    MessageBox.Error($"防火墙消息: 在端口检测到不被允许的连接, 内容如下:" +
    $"{message}", "提示");
                    clientStream.Close(); // 丢弃连接并关闭流
                    continue; // 重试直到接受一个新的合法连接请求
                }

            }
            catch (SocketException e) // 如果发生异常，关闭流并重试连接请求
            {
                MessageBox.Error("SocketException: {0}", e.Message);
                clientStream?.Close(); // 关闭流并重试连接请求
            }
        }
    }
}