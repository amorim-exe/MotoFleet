var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/{**path}", async (
    string? path,
    HttpRequest request,
    IHttpClientFactory clients,
    IConfiguration configuration,
    CancellationToken cancellationToken) =>
{
    var apiBaseAddress = configuration["ApiBaseUrl"] ?? "http://localhost:5240/";
    var target = new Uri(new Uri(apiBaseAddress), $"api/{path}{request.QueryString}");
    using var response = await clients.CreateClient().GetAsync(target, cancellationToken);
    var content = await response.Content.ReadAsStringAsync(cancellationToken);

    return Results.Content(
        content,
        response.Content.Headers.ContentType?.ToString(),
        statusCode: (int)response.StatusCode);
});

app.MapFallbackToFile("index.html");

app.Run();