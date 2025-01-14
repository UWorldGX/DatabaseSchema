﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace FirewallDemo.Model.Data;
public partial class User
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Nickname { get; set; }

    public sbyte Age { get; set; }

    public string Sex { get; set; }

    public string Password { get; set; }

    public string Role { get; set; }

    public string PublicKey { get; set; }

    public int Money { get; set; }

    public virtual ICollection<ChatList> ChatListCustomers { get; set; } = new List<ChatList>();

    public virtual ICollection<ChatList> ChatListSellers { get; set; } = new List<ChatList>();

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Sale> SaleCustomers { get; set; } = new List<Sale>();

    public virtual ICollection<Sale> SaleSellers { get; set; } = new List<Sale>();

    public virtual UserPrivkey UserPrivkey { get; set; }
}