﻿using EventBus.Abstractions;
using IntegrationServices;
using IntegrationServices.Model;
using TaskScheduling.Abstractions;

namespace Ordering.API.Infrastructure.BackgroundTasks;

class IntegrationEventBackgroundTask : IBackgroundTask
{
    private readonly IEventBus _eventBus;
    private readonly IIntegrationEventService _integrationService;

    public IntegrationEventBackgroundTask(
        IEventBus eventBus,
        IIntegrationEventService integrationService)
    {
        _eventBus = eventBus;
        _integrationService = integrationService;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        var pendingEvents = await _integrationService.GetPendingEvents();

        foreach (IntegrationEvent @event in pendingEvents)
        {
            _eventBus.Publish(@event);

            await _integrationService.MarkEventAsCompleted(@event.Id);
        }
    }
}