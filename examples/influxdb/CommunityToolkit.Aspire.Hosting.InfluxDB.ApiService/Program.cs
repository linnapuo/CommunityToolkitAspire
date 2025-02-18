var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var app = builder.Build();

app.MapGet("/get", () => Results.Text("Hello influxdb"));

app.MapDefaultEndpoints();
app.Run();
