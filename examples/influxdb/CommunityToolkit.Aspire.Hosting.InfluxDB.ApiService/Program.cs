using InfluxDB.Client;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.AddInfluxDBClient("influxdb");

var app = builder.Build();

app.MapGet("/", async (IInfluxDBClient client) =>
{
    var success = await client.PingAsync();
    return Results.Text($"InfluxDB healthy: {success}");
});

app.MapDefaultEndpoints();
app.Run();
