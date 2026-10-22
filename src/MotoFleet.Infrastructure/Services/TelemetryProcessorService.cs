using Microsoft.Extensions.Logging;
using MotoFleet.Domain.Events;
using MotoFleet.Domain.Entities;
using MotoFleet.Infrastructure.Cache;
using MotoFleet.Infrastructure.Context;

namespace MotoFleet.Infrastructure.Services;

public class TelemetryProcessorService
{
    private readonly MotoRedisCache _redisCache;
    private readonly AppDbContext _context;
    private readonly ILogger<TelemetryProcessorService> _logger;

    public TelemetryProcessorService(
        MotoRedisCache redisCache, 
        AppDbContext context, 
        ILogger<TelemetryProcessorService> logger)
    {
        _redisCache = redisCache;
        _context = context;
        _logger = logger;
    }

    public async Task ProcessTelemetryAsync(MotoTelemetryEvent telemetry)
    {
        // 1. Atualiza o Redis (Última localização instantânea)
        await _redisCache.SaveLastKnownLocationAsync(telemetry);

        // 2. Salva o histórico no SQL Server
        var log = new MotoTelemetryLog
        {
            MotoId = telemetry.MotoId,
            Plate = telemetry.Plate,
            Latitude = telemetry.Latitude,
            Longitude = telemetry.Longitude,
            Speed = telemetry.Speed,
            FuelLevelPercentage = telemetry.FuelLevelPercentage,
            IsEngineOn = telemetry.IsEngineOn,
            Timestamp = telemetry.Timestamp
        };

        _context.TelemetryLogs.Add(log);
        await _context.SaveChangesAsync();

        _logger.LogInformation(" [Worker + Redis + SQL] Telemetria salva -> Moto: {plate} | Vel: {speed}km/h",
            telemetry.Plate, telemetry.Speed);
    }
}