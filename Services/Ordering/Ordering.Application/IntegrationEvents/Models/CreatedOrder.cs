﻿namespace Ordering.Application.IntegrationEvents.Models;

public record CreatedOrder
{
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public int OrderStatusId { get; set; }
}
