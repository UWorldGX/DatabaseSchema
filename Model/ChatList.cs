﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace FirewallDemo.Model.Model;

public partial class ChatList
{
    public string ChatId { get; set; }

    public string CustomerId { get; set; }

    public string SellerId { get; set; }

    public virtual User Customer { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User Seller { get; set; }
}