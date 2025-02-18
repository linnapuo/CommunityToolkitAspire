using Aspire.Hosting;

namespace CommunityToolkit.Aspire.Hosting.InfluxDB.Tests;

public class AddInfluxDBTests
{
    [Fact]
    public void AddRavenServerResource()
    {
        var appBuilder = DistributedApplication.CreateBuilder();
        appBuilder.AddInfluxDB("influxdb");
        using var app = appBuilder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var serverResource = Assert.Single(appModel.Resources.OfType<InfluxDBServerResource>());
        Assert.Equal("influxdb", serverResource.Name);
    }

    [Fact]
    public void VerifyNonDefaultImageTag()
    {
        const string tag = "windows-latest-lts";

        var builder = DistributedApplication.CreateBuilder();
        builder.AddInfluxDB("influxdb").WithImageTag(tag);

        using var app = builder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var resource = Assert.Single(appModel.Resources.OfType<InfluxDBServerResource>());

        Assert.True(resource.TryGetAnnotationsOfType<ContainerImageAnnotation>(out var annotations));
        var annotation = Assert.Single(annotations);
        Assert.NotNull(annotation.Tag);
        Assert.Equal(tag, annotation.Tag);
    }

    [Fact]
    public void VerifyDefaultPort()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddInfluxDB("influxdb");

        using var app = builder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var resource = Assert.Single(appModel.Resources.OfType<InfluxDBServerResource>());

        var endpoint = Assert.Single(resource.Annotations.OfType<EndpointAnnotation>());

        Assert.Equal(8086, endpoint.TargetPort);
    }

    [Fact]
    public void VerifyCustomPort()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddInfluxDB("influxdb", port: 12345);

        using var app = builder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var resource = Assert.Single(appModel.Resources.OfType<InfluxDBServerResource>());

        var endpoint = Assert.Single(resource.Annotations.OfType<EndpointAnnotation>());

        Assert.Equal(12345, endpoint.Port);
    }
}