// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;
using CommunityToolkit.Aspire.Hosting.InfluxDB;
using Microsoft.Extensions.DependencyInjection;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding InfluxDB resources to an <see cref="IDistributedApplicationBuilder"/>.
/// </summary>
public static class InfluxDBResourceBuilderExtensions
{
    private const int InfluxDBServerPort = 8086;

    /// <summary>
    /// Adds a InfluxDB resource to the application model. A container is used for local development.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/> to which the resource is added.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="port">The host port used when launching the container. If null a random port will be assigned.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    /// <remarks>
    /// <para>
    /// This resource includes built-in health checks. When this resource is referenced as a dependency
    /// using the <see cref="ResourceBuilderExtensions.WaitFor{T}(IResourceBuilder{T}, IResourceBuilder{IResource})"/>
    /// extension method then the dependent resource will wait until the InfluxDB resource is able to service
    /// requests.
    /// </para>
    /// This version of the package defaults to the <inheritdoc cref="InfluxDBContainerImageTags.Tag"/> tag of the <inheritdoc cref="InfluxDBContainerImageTags.Image"/> container image.
    /// </remarks>
    public static IResourceBuilder<InfluxDBServerResource> AddInfluxDB(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        int? port = null)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(name, nameof(name));

        var influxDBServer = new InfluxDBServerResource(name);

        string? connectionString = null;
        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(influxDBServer, async (@event, ct) =>
        {
            connectionString = await influxDBServer.ConnectionStringExpression.GetValueAsync(ct).ConfigureAwait(false);

            if (connectionString is null)
            {
                throw new DistributedApplicationException($"ConnectionStringAvailableEvent was published for the '{influxDBServer.Name}' resource but the connection string was null.");
            }
        });

        //var healthCheckKey = $"{name}_check";
        //builder.Services
        //    .AddHealthChecks()
        //    .AddInfluxDB(
        //        sp => connectionString ?? throw new InvalidOperationException("Connection string is unavailable"),
        //        name: healthCheckKey);

        var state = new CustomResourceSnapshot
        {
            State = new(KnownResourceStates.Running, KnownResourceStateStyles.Success),
            ResourceType = "InfluxDB",
            Properties = []
        };

        return builder
            .AddResource(influxDBServer)
            .WithEndpoint(port: port, targetPort: InfluxDBServerPort, scheme: influxDBServer.PrimaryEndpointName)
            .WithImage(InfluxDBContainerImageTags.Image, InfluxDBContainerImageTags.Tag)
            .WithImageRegistry(InfluxDBContainerImageTags.Registry)
            //.WithHealthCheck(healthCheckKey)
            .WithInitialState(state);
    }
}
