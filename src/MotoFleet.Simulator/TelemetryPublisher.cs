using System.Text;
using System.Text.Json;
using MotoFleet.Domain.Events;
using RabbitMQ.Client;

namespace MotoFleet.Simulator;

public class TelemetryPublisher
{
    private readonly string _hostname = "localhost";
    private readonly string _queueName = "telemetry-queue";

    // DEFINIMOS UM ID FIXO PARA A MOTO DE TESTE
    private readonly Guid _fixedMotoId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private readonly string _plate = "ABC-1234";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory { HostName = _hostname };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        Console.WriteLine($"[Simulator] Iniciando envio de telemetria para a moto {_plate} (ID: {_fixedMotoId})...");

        while (!cancellationToken.IsCancellationRequested)
        {
            var telemetry = new MotoTelemetryEvent
            {
                MotoId = _fixedMotoId, // Usamos o ID fixo aqui
                Plate = _plate,
                Latitude = -23.550520 + (Random.Shared.NextDouble() * 0.01),
                Longitude = -46.633308 + (Random.Shared.NextDouble() * 0.01),
                Speed = Random.Shared.Next(0, 80),
                FuelLevelPercentage = Random.Shared.Next(10, 100),
                IsEngineOn = true,
                Timestamp = DateTime.UtcNow
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(telemetry));

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _queueName,
                body: body
            );

            Console.WriteLine($"[Simulator] Telemetria enviada: Moto {_plate} | Velocidade: {telemetry.Speed} km/h");

            await Task.Delay(3000, cancellationToken);
        }
    }
}