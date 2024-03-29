﻿using IntegrationServices.Models;

namespace IntegrationServices;

public interface IIntegrationEventService
{
    Task<ICollection<IntegrationEvent>> GetPendingEvents();

    Task MarkEventAsCompleted(Guid eventId);

    Task Publish(IntegrationEvent @event);
}
