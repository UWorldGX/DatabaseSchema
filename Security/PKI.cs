using FirewallDemo.Model.Data;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.Security;

/// <summary>
/// 仿照PKI概念设置的RSA密钥分发类
/// </summary>
public class PKI : IPermissionChecker
{
    private readonly IServiceProvider _provider;

    public Permissions CheckPermission(User user)
    {
        return user.Role switch
        {
            "USER" => Permissions.User,
            "SUPREME" => Permissions.Supreme,
            "ADMIN" => Permissions.Admin,
            "INTERNAL" => Permissions.Internal,
            _ => Permissions.Viewer,
        };
    }

    /// <summary>
    /// 获取调用者用户的私钥。
    /// </summary>
    /// <param name="caller"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public byte[] GetUserPrivateKey(User caller)
    {
        var permission = CheckPermission(caller);
        if (permission >= Permissions.Viewer)
        {
            throw new InvalidOperationException("不允许使用仅查看用户进行该操作.");
        }
        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
        try
        {
            var key = dataContext.UserPrivkeys.AsParallel()
                .Where(k => k.UserId == caller.Id)
                .Select(k => k).Single();
            return Convert.FromBase64String(key.PrivateKey);
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return [];
        }

    }

    /// <summary>
    /// 根据用户ID获取任意用户的公钥
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="caller"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public byte[] GetUserPublicKey(string userId, User caller)
    {
        var permission = CheckPermission(caller);
        if (permission >= Permissions.Viewer)
        {
            throw new InvalidOperationException("不允许使用仅查看用户进行该操作.");
        }
        using var serviceScope = _provider.CreateScope();
        using var dataContext = serviceScope.ServiceProvider.GetRequiredService<xpertContext>();
        try
        {
            var user = dataContext.Users.AsParallel()
                .Where(k => k.Id == userId)
                .Select(k => k).Single();
            return Convert.FromBase64String(user.PublicKey);
        }
        catch (Exception ex)
        {
            MessageBox.Error(ex.Message);
            return [];
        }
    }

    public Message CreateEncryptedMessage(string msgContent, string chatId, string targetUserId, User caller)
    {
        var permission = CheckPermission(caller);
        if (permission >= Permissions.Viewer)
        {
            throw new InvalidOperationException("不允许使用仅查看用户进行发消息操作.");
        }

        var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(GetUserPublicKey(targetUserId, caller), out int a);
        //UTF8Encoding encoder = new();
        var chars = msgContent.ToCharArray();
        //var byteCount = encoder.GetByteCount(chars);
        //var bytes = new byte[byteCount];
        //int bytesEncodedCount = encoder.GetBytes(chars, 8, 8, bytes, 0);
        
        Message result = new()
        {
            MsgId = "MSG" + Guid.NewGuid().ToString()[..8],
            ChatId = chatId,
            Timestamp = DateTime.Now,
            Content = Convert.ToBase64String(rsa.Encrypt(System.Text.Encoding.UTF8.GetBytes(chars), RSAEncryptionPadding.Pkcs1))
        };
        return result;
    }

    public Message DecryptMessage(Message msg, User caller)
    {
        var permission = CheckPermission(caller);
        if (permission >= Permissions.Viewer)
        {
            throw new InvalidOperationException("不允许使用仅查看用户进行发消息操作.");
        }

        var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(GetUserPrivateKey(caller), out int a);

        var base64str = rsa.Decrypt(Convert.FromBase64String(msg.Content), RSAEncryptionPadding.Pkcs1);
        //var chars = msg.Content.ToCharArray();
        
        //var byteCount = encoder.GetByteCount(chars);
        //var bytes = new byte[byteCount];
        //int bytesEncodedCount = encoder.GetBytes(chars, 8, 8, bytes, 0);
        msg.Content = System.Text.Encoding.UTF8.GetString(base64str);
        return msg;

    }

    public PKI(IServiceProvider serviceProvider)
    {
        _provider = serviceProvider;
    }
}
