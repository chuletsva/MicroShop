﻿using Catalog.Domian.Abstractions;

namespace Ordering.Domian.Entities.BuyerAggregate;

public class Buyer : BaseEntity
{
    public Buyer()
    {
        PaymentMethods = new HashSet<PaymentMethod>();
    }

    public string Name { get; set; }

    public ICollection<PaymentMethod> PaymentMethods { get; set; }
}
