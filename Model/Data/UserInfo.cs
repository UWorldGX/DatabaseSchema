#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallDemo.Model.Data;
/// <summary>
/// 用户信息类，该类旨在防止其他用户对他人密码等敏感信息的获取
/// </summary>
public record UserInfo
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Nickname { get; set; }

    public sbyte Age { get; set; }

    public string Sex { get; set; }
}
