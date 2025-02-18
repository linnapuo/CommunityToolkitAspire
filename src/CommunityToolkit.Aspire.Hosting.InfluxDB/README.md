# CommunityToolkit.Aspire.Hosting.InfluxDB library

Provides extension methods and resource definitions for a .NET Aspire AppHost to configure a [InfluxDB](https://github.com/influxdata/influxdb) resource.

## Getting started

### Install the package

In your AppHost project, install the .NET Aspire InfluxDB Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package CommunityToolkit.Aspire.Hosting.InfluxDB
```

## Usage example

Then, in the _Program.cs_ file of `AppHost`, add a InfluxDB resource and consume the connection using the following methods:

```csharp
var influx = builder.AddInfluxDB("influxdb");

var myService = builder.AddProject<Projects.MyService>()
                       .WithReference(influx);
```

## Additional documentation

<!-- TODO: Update the link once it is created -->
https://learn.microsoft.com/dotnet/aspire/community-toolkit/influxdb

## Feedback & contributing

https://github.com/CommunityToolkit/Aspire
