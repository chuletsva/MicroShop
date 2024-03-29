﻿using AutoMapper;
using IntegrationServices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordering.Application.IntegrationEvents.Events;
using Ordering.Application.IntegrationEvents.Models;
using Ordering.Application.Services;
using Ordering.Domian.Dictionaries;
using Ordering.Domian.Entities;

namespace Ordering.Application.Requests.Orders.AcceptOrder;

public class AcceptOrderCommandHandler : IRequestHandler<AcceptOrderCommand>
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly IOrderingDbContext _orderingDb;
    private readonly IIntegrationEventService _integrationEvents;

    public AcceptOrderCommandHandler(
        ILogger<AcceptOrderCommandHandler> logger,
        IMapper mapper,
        IOrderingDbContext orderingDb,
        IIntegrationEventService integrationEvents)
    {
        _logger = logger;
        _mapper = mapper;
        _orderingDb = orderingDb;
        _integrationEvents = integrationEvents;
    }

    public async Task<Unit> Handle(AcceptOrderCommand request, CancellationToken cancellationToken)
    {
        Order order = await _orderingDb.Orders
            .Include(x => x.OrderItems)
            .Include(x => x.OrderStatus)
            .Include(x => x.PaymentMethod)
            .SingleAsync(x => x.Id == request.OrderId);

        if (order.OrderStatusId == OrderStatusDict.ConfirmedByUser.Id)
        {
            order.OrderStatusId = OrderStatusDict.Accepted.Id;

            await _orderingDb.SaveChangesAsync();

            AcceptedOrder acceptedOrder = new
            (
                OrderId: order.Id,
                BuyerId: order.BuyerId,
                OrderStatusId: order.OrderStatusId,
                Total: order.OrderItems.Sum(x => x.Quantity * x.UnitPrice),
                PaymentCard: _mapper.Map<BuyerCardInfo>(order.PaymentMethod)
            );

            await _integrationEvents.Publish(new OrderAcceptedIntegrationEvent(acceptedOrder));
        }
        else
        {
            _logger.LogWarning(
                "Order status must be {ExpectedStatus} but got {ActualStatus}",
                OrderStatusDict.ConfirmedByUser.Name,
                order.OrderStatus.Name);
        }

        return Unit.Value;
    }
}