using System.Text.Json;
using MotoFleet.Domain.Events;
using StackExchange.Redis;

namespace MotoFleet.Infrastructure.Cache;

public class MotoRedisCache
{
    private readonly IConnectionMultiplexer _redis;

    public MotoRedisCache(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task SaveLastKnownLocationAsync(MotoTelemetryEvent telemetry)
    {
        var db = _redis.GetDatabase();
        var key = $"fleet:moto:{telemetry.MotoId}:last-known";

        var data = JsonSerializer.Serialize(new
        {
            telemetry.Plate,
            telemetry.Latitude,
            telemetry.Longitude,
            telemetry.Speed,
            telemetry.FuelLevelPercentage,
            telemetry.IsEngineOn,
            telemetry.Timestamp
        });

        // Salva com chave única para acesso O(1) instantâneo
        await db.StringSetAsync(key, data);
    }
}