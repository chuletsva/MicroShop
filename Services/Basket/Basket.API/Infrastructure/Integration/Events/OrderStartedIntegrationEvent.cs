﻿using IntegrationServices.Model;
using System.Text.Json.Serialization;

namespace Basket.API.Infrastructure.Integration.Events;

public record OrderStartedIntegrationEvent : IntegrationEvent
{
    [JsonInclude]
    public Guid OrderId { get; private set; }

    [JsonInclude]
    public Guid BuyerId { get; private set; }
}
