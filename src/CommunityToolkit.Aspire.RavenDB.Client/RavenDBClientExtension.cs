﻿using CommunityToolkit.Aspire.RavenDB.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using System.Data.Common;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for connecting RavenDB database.
/// </summary>
public static class RavenDBClientExtension
{
    private const string ActivityNameSource = "RavenDB.Client.DiagnosticSources";

    private const string DefaultConfigSectionName = "Aspire:RavenDB:Client";

    /// <summary>
    /// Registers <see cref="IDocumentStore"/> and the associated <see cref="IDocumentSession"/> and <see cref="IAsyncDocumentSession"/>
    /// instances for connecting to an existing or new RavenDB database with RavenDB.Client.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> used to add services.</param>
    /// <param name="connectionName">The name used to retrieve the connection string from the "ConnectionStrings" configuration section.</param>
    /// <param name="configureSettings">An optional delegate that can be used for customizing options. It is invoked after the settings are read from the configuration.</param>
    /// <remarks>Notes:
    /// <list type="bullet">
    /// <item><description>Reads the configuration from "Aspire:RavenDB:Client" section.</description></item>
    /// <item><description>The <see cref="IDocumentStore"/> is registered as a singleton, meaning a single instance is shared throughout the application's lifetime, 
    /// while <see cref="IDocumentSession"/> and <see cref="IAsyncDocumentSession"/> are registered per request to ensure short-lived session instances for each use.</description></item>
    /// </list>
    /// </remarks>
    public static void AddRavenDBClient(
        this IHostApplicationBuilder builder,
        string connectionName,
        Action<RavenDBClientSettings>? configureSettings = null)
    {
        var settings = GetRavenDBClientSettings(builder, connectionName, configureSettings);

        builder.AddRavenDBClientInternal(settings);
    }

    /// <summary>
    /// Registers <see cref="IDocumentStore"/> and the associated <see cref="IDocumentSession"/> and <see cref="IAsyncDocumentSession"/>
    /// instances for connecting to an existing or new RavenDB database with RavenDB.Client, identified by a unique service key.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> used to add services.</param>
    /// <param name="serviceKey">A unique key that identifies this instance of the RavenDB client service.</param>
    /// <param name="connectionName">The name used to retrieve the connection string from the "ConnectionStrings" configuration section.</param>
    /// <param name="configureSettings">An optional delegate that can be used for customizing options. It is invoked after the settings are read from the configuration.</param>
    /// <remarks>Notes:
    /// <list type="bullet">
    /// <item><description>Reads the configuration from "Aspire:RavenDB:Client" section.</description></item>
    /// <item><description>The <see cref="IDocumentStore"/> is registered as a singleton, meaning a single instance is shared throughout the application's lifetime, 
    /// while <see cref="IDocumentSession"/> and <see cref="IAsyncDocumentSession"/> are registered per request to ensure short-lived session instances for each use.</description></item>
    /// </list>
    /// </remarks>
    public static void AddKeyedRavenDBClient(
        this IHostApplicationBuilder builder,
        object serviceKey,
        string connectionName,
        Action<RavenDBClientSettings>? configureSettings = null)
    {
        var settings = GetRavenDBClientSettings(builder, connectionName, configureSettings);

        builder.AddRavenDBClientInternal(settings, serviceKey);
    }

    /// <summary>
    /// Registers <see cref="IDocumentStore"/> and the associated <see cref="IDocumentSession"/> and <see cref="IAsyncDocumentSession"/>
    /// instances for connecting to an existing or new RavenDB database with RavenDB.Client.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> used to add services.</param>
    /// <param name="settings">The settings required to configure the <see cref="IDocumentStore"/>.</param>
    /// <remarks>Notes:
    /// <list type="bullet">
    /// <item><description>If <see cref="RavenDBClientSettings.DatabaseName"/> is not specified and <see cref="RavenDBClientSettings.CreateDatabase"/> is set to 'false',
    /// <see cref="IDocumentSession"/> and <see cref="IAsyncDocumentSession"/> will not be registered.</description></item>
    /// <item><description>The <see cref="IDocumentStore"/> is registered as a singleton, meaning a single instance is shared throughout the application's lifetime, 
    /// while <see cref="IDocumentSession"/> and <see cref="IAsyncDocumentSession"/> are registered per request to ensure short-lived session instances for each use.</description></item>
    /// </list>
    /// </remarks>
    public static void AddRavenDBClient(
        this IHostApplicationBuilder builder,
        RavenDBClientSettings settings)
    {
        builder.AddRavenDBClientInternal(settings);
    }

    /// <summary>
    /// Registers <see cref="IDocumentStore"/> and the associated <see cref="IDocumentSession"/> and <see cref="IAsyncDocumentSession"/>
    /// instances for connecting to an existing or new RavenDB database with RavenDB.Client, identified by a unique service key.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> used to add services.</param>
    /// <param name="serviceKey">A unique key that identifies this instance of the RavenDB client service.</param>
    /// <param name="settings">The settings required to configure the <see cref="IDocumentStore"/>.</param>
    /// <remarks>Notes:
    /// <list type="bullet">
    /// <item><description>If <see cref="RavenDBClientSettings.DatabaseName"/> is not specified and <see cref="RavenDBClientSettings.CreateDatabase"/> is set to 'false',
    /// <see cref="IDocumentSession"/> and <see cref="IAsyncDocumentSession"/> will not be registered.</description></item>
    /// <item><description>The <see cref="IDocumentStore"/> is registered as a singleton, meaning a single instance is shared throughout the application's lifetime, 
    /// while <see cref="IDocumentSession"/> and <see cref="IAsyncDocumentSession"/> are registered per request to ensure short-lived session instances for each use.</description></item>
    /// </list>
    /// </remarks>
    public static void AddKeyedRavenDBClient(
        this IHostApplicationBuilder builder,
        object serviceKey,
        RavenDBClientSettings settings)
    {
        builder.AddRavenDBClientInternal(settings, serviceKey);
    }

    private static void AddRavenDBClientInternal(
        this IHostApplicationBuilder builder,
        RavenDBClientSettings settings,
        object? serviceKey = null)
    {
        ValidateSettings(builder, settings);

        var documentStore = CreateRavenClient(settings);

        if (serviceKey is null)
        {
            builder
                .Services
                .AddSingleton<IDocumentStore>(documentStore);
        }
        else
        {
            builder
                .Services
                .AddKeyedSingleton<IDocumentStore>(serviceKey, documentStore);
        }

        builder.AddRavenDocumentSession(documentStore, serviceKey);

        if (!settings.DisableTracing)
        {
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    tracing.AddSource(ActivityNameSource);
                });
        }

        builder.AddHealthCheck(
            serviceKey is null ? "RavenDB.Client" : $"RavenDB.Client_{serviceKey}",
            settings);
    }

    private static void AddRavenDocumentSession(
        this IHostApplicationBuilder builder,
        IDocumentStore documentStore,
        object? serviceKey)
    {
        if (string.IsNullOrWhiteSpace(documentStore.Database))
            return;

        // AddTransient creates new instance per request/usage which is ideal for document sessions

        if (serviceKey is null)
        {
            builder.Services.AddTransient<IDocumentSession>(provider =>
                provider.CreateDocumentSession(documentStore));

            builder.Services.AddTransient<IAsyncDocumentSession>(provider =>
                provider.CreateAsyncDocumentSession(documentStore));

            return;
        }

        builder.Services.AddKeyedTransient<IDocumentSession>(serviceKey,
            (sp, _) => sp.CreateDocumentSession(documentStore));

        builder.Services.AddKeyedTransient<IAsyncDocumentSession>(serviceKey,
            (sp, _) => sp.CreateAsyncDocumentSession(documentStore));
    }

    private static void AddHealthCheck(
        this IHostApplicationBuilder builder,
        string healthCheckName,
        RavenDBClientSettings settings)
    {
        if (settings.DisableHealthChecks)
            return;

        builder.TryAddHealthCheck(
            healthCheckName,
            healthCheck => healthCheck.AddRavenDB(options =>
                {
                    options.Database = settings.DatabaseName;
                    options.Urls = settings.Urls!;
                    options.Certificate = settings.GetCertificate();
                },
                healthCheckName,
                null,
                null,
                settings.HealthCheckTimeout > 0 ? TimeSpan.FromMilliseconds(settings.HealthCheckTimeout.Value) : null));
    }

    private static IDocumentStore CreateRavenClient(RavenDBClientSettings ravenDbClientSettings)
    {
        var documentStore = new DocumentStore()
        {
            Urls = ravenDbClientSettings.Urls,
            Database = ravenDbClientSettings.DatabaseName,
            Certificate = ravenDbClientSettings.GetCertificate()
        };

        ravenDbClientSettings.ModifyDocumentStore?.Invoke(documentStore);

        documentStore.Initialize();

        if (ravenDbClientSettings.CreateDatabase)
        {
            var databaseRecord = documentStore.Maintenance.Server.Send(new GetDatabaseRecordOperation(ravenDbClientSettings.DatabaseName));
            if (databaseRecord == null)
            {
                var newDatabaseRecord = new DatabaseRecord(ravenDbClientSettings.DatabaseName);
                documentStore.Maintenance.Server.Send(new CreateDatabaseOperation(newDatabaseRecord));
            }
        }

        return documentStore;
    }

    private static IDocumentSession CreateDocumentSession(this IServiceProvider provider,
        IDocumentStore documentStore) => documentStore.OpenSession();

    private static IAsyncDocumentSession CreateAsyncDocumentSession(this IServiceProvider provider,
        IDocumentStore documentStore) => documentStore.OpenAsyncSession();

    private static RavenDBClientSettings GetRavenDBClientSettings(this IHostApplicationBuilder builder,
        string connectionName,
        Action<RavenDBClientSettings>? configureSettings)
    {
        var configSection = builder.Configuration.GetSection(DefaultConfigSectionName);
        var namedConfigSection = configSection.GetSection(connectionName);

        var settings = new RavenDBClientSettings();
        configSection.Bind(settings);
        namedConfigSection.Bind(settings);

        var connectionString = builder.Configuration.GetConnectionString(connectionName);
        if (string.IsNullOrEmpty(connectionString) == false)
        {
            var connectionBuilder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            if (connectionBuilder.TryGetValue("URL", out var url) && url is string serverUrl)
            {
                settings.Urls = new[] { serverUrl };
            }

            if (connectionBuilder.TryGetValue("Database", out var database) && database is string databaseName)
            {
                settings.DatabaseName = databaseName;
            }
        }

        configureSettings?.Invoke(settings);

        return settings;
    }

    private static void ValidateSettings(
        IHostApplicationBuilder builder,
        RavenDBClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (settings.Urls is null || settings.Urls.Length == 0)
            throw new ArgumentNullException(nameof(settings.Urls), "At least one connection URL must be provided.");

        if (settings.CreateDatabase && string.IsNullOrWhiteSpace(settings.DatabaseName))
            throw new ArgumentNullException(nameof(settings.DatabaseName), "A database name must be specified in 'RavenDBClientSettings.DatabaseName' " +
                                                                           "when 'RavenDBClientSettings.CreateDatabase' is set to true.");

        foreach (var url in settings.Urls)
        {
            if (IsValidUrl(url, out var uri) == false)
                throw new ArgumentException($"The provided URL '{url}' is invalid. Please provide a valid HTTP or HTTPS URL.");

            if (uri!.Scheme == Uri.UriSchemeHttps)
            {
                if (string.IsNullOrEmpty(settings.CertificatePath) && settings.Certificate == null)
                    throw new ArgumentNullException(nameof(settings.Certificate), "A valid certificate must be provided in 'RavenDBClientSettings.Certificate' " +
                        "or a certificate path in 'RavenDBClientSettings.CertificatePath' when using HTTPS.");
            }
        }

        bool IsValidUrl(string url, out Uri? uriResult)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
