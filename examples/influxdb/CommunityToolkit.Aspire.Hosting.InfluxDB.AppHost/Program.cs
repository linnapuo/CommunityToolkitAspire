var builder = DistributedApplication.CreateBuilder(args);

builder.AddInfluxDB("influxdb");

builder.AddProject<Projects.CommunityToolkit_Aspire_Hosting_InfluxDB_ApiService>("apiservice");

builder.Build().Run();
