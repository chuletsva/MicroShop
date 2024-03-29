﻿using Basket.API.Infrastructure.Services;
using Basket.API.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Basket.API.Infrastructure.DataAccess;

class BasketRepository : IBasketRepository
{
    private readonly ILogger _logger;
    private readonly ICurrentTime _currentTime;
    private readonly IConnectionMultiplexer _connection;
    private readonly IDatabase _database;

    public BasketRepository(
        ILogger<BasketRepository> logger, 
        ICurrentTime currentTime,
        IConnectionMultiplexer connection)
    {
        _logger = logger;
        _currentTime = currentTime;
        _connection = connection;
        _database = connection.GetDatabase();
    }

    public async Task<string[]> GetUsers()
    {
        var endpoint = _connection.GetEndPoints().First();
        var server = _connection.GetServer(endpoint);

        return server.Keys().Select(x => x.ToString()).ToArray();
    }

    public async Task<BasketEntry?> Get(string userId)
    {
        var basket = await _database.StringGetAsync(userId);

        if (basket.IsNull)
            return null;

        return JsonSerializer.Deserialize<BasketEntry>(
            basket,
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = false });
    }

    public Task Update(BasketEntry basket)
    {
        basket.LastUpdate = _currentTime.Now;

        return _database.StringSetAsync(basket.UserId, JsonSerializer.Serialize(basket));
    }

    public async Task Remove(string buyerId)
    {
        await _database.KeyDeleteAsync(buyerId);
    }
}
