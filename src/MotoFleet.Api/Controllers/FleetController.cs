using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace MotoFleet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FleetController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;

    public FleetController(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    [HttpGet("motos/{motoId}/last-location")]
    public async Task<IActionResult> GetLastLocation(Guid motoId)
    {
        var db = _redis.GetDatabase();
        var key = $"fleet:moto:{motoId}:last-known";

        var cachedData = await db.StringGetAsync(key);

        if (cachedData.IsNullOrEmpty)
        {
            return NotFound(new { message = "Nenhuma telemetria encontrada para esta moto no cache." });
        }

        var jsonString = cachedData.ToString();
        var dataObject = System.Text.Json.JsonSerializer.Deserialize<object>(jsonString);

        return Ok(dataObject);
    }
}