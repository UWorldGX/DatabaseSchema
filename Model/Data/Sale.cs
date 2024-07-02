﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace FirewallDemo.Model.Data;

public partial class Sale
{
    public string SaleId { get; set; }

    public string ItemId { get; set; }

    public string CustomerId { get; set; }

    public string SellerId { get; set; }

    public decimal RealPrice { get; set; }

    public DateTime Timestamp { get; set; }

    public string Status { get; set; }

    public virtual User Customer { get; set; }

    public virtual Item Item { get; set; }

    public virtual User Seller { get; set; }
}

public enum SaleStatus
{
    WaitingforSeller,
    WaitingforBuyer,
    Success,
    ClosedByCustomer,
    ClosedBySeller,
    ClosedByOperator,
    Warned
}
