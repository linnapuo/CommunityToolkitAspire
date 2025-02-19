using Aspire.Components.Common.Tests;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using CommunityToolkit.Aspire.Hosting.InfluxDB;

namespace CommunityToolkit.Aspire.InfluxDB.Client.Tests;

public class InfluxDBContainerFixture : IAsyncLifetime
{
    public IContainer? Container { get; private set; }

    public string GetContainerEndpoint()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }
        var endpoint = new UriBuilder("http", Container.Hostname, Container.GetMappedPublicPort(8086)).ToString();
        return endpoint;
    }

    public async Task InitializeAsync()
    {
        if (RequiresDockerAttribute.IsSupported)
        {
            Container = new ContainerBuilder()
              .WithImage($"{InfluxDBContainerImageTags.Registry}/{InfluxDBContainerImageTags.Image}:{InfluxDBContainerImageTags.Tag}")
              .WithPortBinding(8086, true)
              .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8086)))
              .Build();

            await Container.StartAsync();
        }
    }

    public async Task DisposeAsync()
    {
        if (Container is not null)
        {
            await Container.DisposeAsync();
        }
    }
}
