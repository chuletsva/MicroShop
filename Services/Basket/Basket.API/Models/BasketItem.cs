﻿namespace Basket.API.Models;

public class BasketItem
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? OldUnitPrice { get; set; }
    public int Quantity { get; set; }
    public string PictureUrl { get; set; }
}
