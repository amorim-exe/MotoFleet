using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MotoFleet.Domain.Events;
using MotoFleet.Infrastructure.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MotoFleet.TelemetryWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _hostname = "localhost";
    private readonly string _queueName = "telemetry-queue";

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TelemetryWorker iniciado às: {time}", DateTimeOffset.Now);

        var factory = new ConnectionFactory { HostName = _hostname };
        
        IConnection? connection = null;
        while (connection == null && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                connection = await factory.CreateConnectionAsync(stoppingToken);
            }
            catch
            {
                _logger.LogWarning("Tentando conectar ao RabbitMQ...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (connection == null) return;

        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            var telemetry = JsonSerializer.Deserialize<MotoTelemetryEvent>(message);

            if (telemetry != null)
            {
                // Cria um escopo para injetar serviços Scoped (como o AppDbContext) dentro de um Singleton Worker
                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<TelemetryProcessorService>();
                
                await processor.ProcessTelemetryAsync(telemetry);
            }

            await Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}