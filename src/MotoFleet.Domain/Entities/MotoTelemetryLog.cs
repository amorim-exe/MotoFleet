namespace MotoFleet.Domain.Entities;

public class MotoTelemetryLog
{
    public long Id { get; set; }
    public Guid MotoId { get; set; }
    public string Plate { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Speed { get; set; }
    public int FuelLevelPercentage { get; set; }
    public bool IsEngineOn { get; set; }
    public DateTime Timestamp { get; set; }
}