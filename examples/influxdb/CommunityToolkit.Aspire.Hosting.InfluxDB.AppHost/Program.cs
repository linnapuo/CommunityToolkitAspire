var builder = DistributedApplication.CreateBuilder(args);

var influx = builder.AddInfluxDB("influxdb");

builder.AddProject<Projects.CommunityToolkit_Aspire_Hosting_InfluxDB_ApiService>("apiservice")
    .WithReference(influx)
    .WaitFor(influx);

builder.Build().Run();
