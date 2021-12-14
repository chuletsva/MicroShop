﻿using Ordering.Application.Model.Orders;
using Ordering.Application.Requests.Abstractions;

namespace Ordering.Application.Requests.Orders.ConfirmOrder;

public record ConfirmOrderCommand(Guid OrderId, OrderEditDto Order) : Command;
