using FluentAssertions;
using MotoFleet.Domain.Events;
using Xunit;

namespace MotoFleet.UnitTests;

public class TelemetryEventTests
{
    [Fact]
    public void MotoTelemetryEvent_ShouldInitializeWithCorrectDefaultValues()
    {
        // Arrange & Act
        var motoId = Guid.NewGuid();
        var telemetry = new MotoTelemetryEvent
        {
            MotoId = motoId,
            Plate = "XYZ-9876",
            Latitude = -23.5505,
            Longitude = -46.6333,
            Speed = 45.0,
            FuelLevelPercentage = 80,
            IsEngineOn = true
        };

        // Assert
        telemetry.MotoId.Should().Be(motoId);
        telemetry.Plate.Should().Be("XYZ-9876");
        telemetry.Speed.Should().Be(45.0);
        telemetry.IsEngineOn.Should().BeTrue();
        telemetry.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}