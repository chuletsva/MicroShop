﻿using EventBus.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.IntegrationEvents.Events;
using Ordering.Application.Requests.Abstractions;
using Ordering.Application.Requests.Orders.AcceptOrder;
using Ordering.Application.Requests.Orders.UpdateItemsInStock;

namespace Ordering.Application.IntegrationEvents.EventHandlers;

public class OrderInStockCheckedIntegrationEventHandler : IEventHandler<OrderInStockCheckedIntegrationEvent>
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;

    public OrderInStockCheckedIntegrationEventHandler(
        ILogger<OrderInStockCheckedIntegrationEventHandler> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task Handle(OrderInStockCheckedIntegrationEvent @event)
    {
        _logger.LogInformation("Start processing event {@Event}", @event);

        try
        {
            Command command;
            if (@event.Order.Items.Any(x => !x.IsInStock))
                command = new UpdateItemsInStockCommand(@event.Order.OrderId, @event.Order.Items);
            else
                command = new AcceptOrderCommand(@event.Order.OrderId);

            await _mediator.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured while processing event {@Event}", @event.Id);
            return;
        }

        _logger.LogInformation("Processing event {@Event} succeed", @event.Id);
    }
}