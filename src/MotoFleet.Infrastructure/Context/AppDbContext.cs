using Microsoft.EntityFrameworkCore;
using MotoFleet.Domain.Entities;

namespace MotoFleet.Infrastructure.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MotoTelemetryLog> TelemetryLogs => Set<MotoTelemetryLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MotoTelemetryLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Plate).IsRequired().HasMaxLength(10);
            entity.HasIndex(e => e.MotoId);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}