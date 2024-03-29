﻿using AutoMapper;
using IntegrationServices;
using MediatR;
using Ordering.Application.IntegrationEvents.Events;
using Ordering.Application.IntegrationEvents.Models;
using Ordering.Application.Services;
using Ordering.Domian.Dictionaries;
using Ordering.Domian.Entities;

namespace Ordering.Application.Requests.Orders.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand>
{
    private readonly IMapper _mapper;
    private readonly ICurrentTime _currentTime;
    private readonly IOrderingDbContext _orderingDb;
    private readonly IIntegrationEventService _integrationService;

    public CreateOrderCommandHandler(
        IMapper mapper,
        ICurrentTime currentTime,
        IOrderingDbContext orderingDb,
        IIntegrationEventService integrationService)
    {
        _mapper = mapper;
        _currentTime = currentTime;
        _orderingDb = orderingDb;
        _integrationService = integrationService;
    }

    public async Task<Unit> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        Buyer buyer = await _orderingDb.Buyers.FindAsync(request.BuyerId) ?? 
            await CreateBuyer(request.BuyerId, request.BuyerName);
        
        Order order = new()
        {
            BuyerId = buyer.Id,
            OrderDate = _currentTime.Now,
            OrderStatusId = OrderStatusDict.Submitted.Id,
        };

        foreach (BasketItem basketItem in request.Basket.Items)
        {
            var orderItem = _mapper.Map<OrderItem>(basketItem);
            orderItem.IsInStock = true;
            order.OrderItems.Add(orderItem);
        }

        await _orderingDb.Orders.AddAsync(order);

        await _orderingDb.SaveChangesAsync();

        await _integrationService.Publish(new OrderCreatedIntegrationEvent(_mapper.Map<CreatedOrder>(order)));

        return Unit.Value;
    }

    private async Task<Buyer> CreateBuyer(Guid buyerId, string buyerName)
    {
        Buyer buyer = new()
        {
            Id = buyerId,
            Name = buyerName
        };

        await _orderingDb.Buyers.AddAsync(buyer);

        return buyer;
    }
}