using MotoFleet.Simulator;

var publisher = new TelemetryPublisher();
var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, _) => cts.Cancel();

try
{
    await publisher.StartAsync(cts.Token);
}
catch (Exception ex)
{
    Console.WriteLine($"[Simulator] Erro: {ex.Message}");
}