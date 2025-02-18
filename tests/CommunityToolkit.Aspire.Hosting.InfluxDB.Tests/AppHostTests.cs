using Aspire.Components.Common.Tests;
using CommunityToolkit.Aspire.Testing;
using Microsoft.Extensions.Hosting;

namespace CommunityToolkit.Aspire.Hosting.InfluxDB.Tests;


[RequiresDocker]
public class AppHostTests(AspireIntegrationTestFixture<Projects.CommunityToolkit_Aspire_Hosting_InfluxDB_AppHost> fixture) : IClassFixture<AspireIntegrationTestFixture<Projects.CommunityToolkit_Aspire_Hosting_InfluxDB_AppHost>>
{
    [Fact]
    public async Task TestAppHost()
    {
        using var cancellationToken = new CancellationTokenSource();
        cancellationToken.CancelAfter(TimeSpan.FromMinutes(5));

        var connectionName = "influxdb";

        await fixture.ResourceNotificationService.WaitForResourceAsync(connectionName, KnownResourceStates.Running, cancellationToken.Token).WaitAsync(TimeSpan.FromMinutes(5), cancellationToken.Token);

        var endpoint = fixture.GetEndpoint(connectionName, "http");
        Assert.NotNull(endpoint);
        Assert.False(string.IsNullOrWhiteSpace(endpoint.OriginalString));
        Assert.True(endpoint.Scheme == Uri.UriSchemeHttp);

        var appModel = fixture.App.Services.GetRequiredService<DistributedApplicationModel>();
        var serverResource = Assert.Single(appModel.Resources.OfType<InfluxDBServerResource>());

        var serverConnectionString = await serverResource.ConnectionStringExpression.GetValueAsync(cancellationToken.Token);
        Assert.False(string.IsNullOrWhiteSpace(serverConnectionString));
        Assert.Contains(endpoint.OriginalString, serverConnectionString);
    }
}
