﻿using Basket.API.Infrastructure.BackgroundTasks;
using Basket.API.Infrastructure.DataAccess;
using Basket.API.Infrastructure.Mapper.Converters;
using Basket.API.Infrastructure.Services;
using EventBus.RabbitMQ.DependencyInjection;
using Google.Protobuf.Collections;
using StackExchange.Redis;
using TaskScheduling.Core;

namespace Basket.API.Configuration;

static class ServicesConfiguration
{
    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddSingleton<ICurrentTimeService, CurrentTimeService>();
    }

    public static void AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("RedisConnection");

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString));

        services.AddScoped<IBasketRepository, BasketRepository>();
    }

    public static void AddIntegrationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRabbitMQEventBus(settings: new
        (
            HostName: configuration.GetValue<string>("RabbitMQSettings:HostName"),
            Retries: configuration.GetValue<int>("RabbitMQSettings:Retries"),
            ClientName: configuration.GetValue<string>("RabbitMQSettings:ClientName"),
            UserName: configuration.GetValue<string>("RabbitMQSettings:UserName"),
            Password: configuration.GetValue<string>("RabbitMQSettings:Password")
        ));
    }

    public static void AddAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(config => 
        {
            config.AddMaps(typeof(Startup));

            config.CreateMap(typeof(RepeatedField<>), typeof(List<>))
                .ConvertUsing(typeof(RepeatedFieldToListTypeConverter<,>));

            config.CreateMap(typeof(List<>), typeof(RepeatedField<>))
                .ConvertUsing(typeof(ListToRepeatedFieldTypeConverter<,>));
        });

        services.AddSingleton(typeof(RepeatedFieldToListTypeConverter<,>));
        services.AddSingleton(typeof(ListToRepeatedFieldTypeConverter<,>));
    }

    public static void AddTaskScheduling(this IServiceCollection services, IConfiguration configuration)
    {
        TimeSpan basketExpirationDays = TimeSpan.FromDays(configuration.GetValue<int>("BackgroundTasks:BasketExpirationDays"));

        services.AddScheduler(
            settings: new SchedulerSettings
            (
                PollingIntervalSec: configuration.GetValue<int>("BackgroundTasks:PollingIntervalSec")
            ),
            taskSettings: new[]
            {
                new BackgroundTaskSettings<DeleteExpiredBasketsBackgroundTask>
                (
                    Schedule: configuration.GetValue<string>("BackgroundTasks:Schedule:DeleteExpiredBaskets")
                )
                {
                    Factory = (sp) => ActivatorUtilities.CreateInstance<DeleteExpiredBasketsBackgroundTask>(sp, basketExpirationDays)
                }
            },
            exceptionHandler: (exception, task, services) =>
            {
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger(task.GetType());

                logger.LogError(exception, "Error occured while executing task {TaskType}", task.GetType().Name);
            });
    }
}