﻿using Ordering.Application.Models.PaymentMethods;
using Ordering.Application.Requests.Abstractions;

namespace Ordering.Application.Requests.Buyers.CreatePaymentMethod;

public record CreatePaymentMethodCommand(Guid BuyerId, PaymentMethodEditDto PaymentMethod) : Command<Guid>;
