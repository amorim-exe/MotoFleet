using Microsoft.EntityFrameworkCore;
using MotoFleet.Infrastructure.Cache;
using MotoFleet.Infrastructure.Context;
using MotoFleet.Infrastructure.Services;
using MotoFleet.TelemetryWorker;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

var sqlConnectionString = "Server=localhost,1433;Database=MotoFleetDb;User Id=sa;TrustServerCertificate=True;Password=Password123!";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(sqlConnectionString));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("localhost:6379"));

builder.Services.AddSingleton<MotoRedisCache>();
builder.Services.AddScoped<TelemetryProcessorService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();