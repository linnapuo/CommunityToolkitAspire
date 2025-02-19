using CommunityToolkit.Aspire.InfluxDB.Client;
using InfluxDB.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for connecting InfluxDB database.
/// </summary>
public static class InfluxDBClientExtension
{
    private const string ActivityNameSource = "InfluxDB.Client.DiagnosticSources";

    private const string DefaultConfigSectionName = "Aspire:InfluxDB:Client";

    /// <summary>
    /// Registers <see cref="IInfluxDBClient"/> instance for connecting to an existing or new InfluxDB database with InfluxDB.Client.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> used to add services.</param>
    /// <param name="connectionName">The name used to retrieve the connection string from the "ConnectionStrings" configuration section.</param>
    /// <param name="configureSettings">An optional delegate that can be used for customizing options. It is invoked after the settings are read from the configuration.</param>
    /// <remarks>Notes:
    /// <list type="bullet">
    /// <item><description>Reads the configuration from "Aspire:InfluxDB:Client" section.</description></item>
    /// <item><description>The <see cref="IInfluxDBClient"/> is registered as a singleton, meaning a single instance is shared throughout the application's lifetime.</description></item>
    /// </list>
    /// </remarks>
    public static void AddInfluxDBClient(
        this IHostApplicationBuilder builder,
        string connectionName,
        Action<InfluxDBClientSettings>? configureSettings = null)
    {
        var settings = GetInfluxDBClientSettings(builder, connectionName, configureSettings);

        builder.AddInfluxDBClientInternal(settings);
    }

    /// <summary>
    /// Registers <see cref="IInfluxDBClient"/> instance for connecting to an existing or new InfluxDB database with InfluxDB.Client,
    /// identified by a unique service key.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> used to add services.</param>
    /// <param name="serviceKey">A unique key that identifies this instance of the InfluxDB client service.</param>
    /// <param name="connectionName">The name used to retrieve the connection string from the "ConnectionStrings" configuration section.</param>
    /// <param name="configureSettings">An optional delegate that can be used for customizing options. It is invoked after the settings are read from the configuration.</param>
    /// <remarks>Notes:
    /// <list type="bullet">
    /// <item><description>Reads the configuration from "Aspire:InfluxDB:Client" section.</description></item>
    /// <item><description>The <see cref="IInfluxDBClient"/> is registered as a singleton, meaning a single instance is shared throughout the application's lifetime.</description></item>
    /// </list>
    /// </remarks>
    public static void AddKeyedInfluxDBClient(
        this IHostApplicationBuilder builder,
        object serviceKey,
        string connectionName,
        Action<InfluxDBClientSettings>? configureSettings = null)
    {
        var settings = GetInfluxDBClientSettings(builder, connectionName, configureSettings);

        builder.AddInfluxDBClientInternal(settings, serviceKey);
    }

    /// <summary>
    /// Registers <see cref="IInfluxDBClient"/> instance for connecting to an existing or new InfluxDB database with InfluxDB.Client.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> used to add services.</param>
    /// <param name="settings">The settings required to configure the <see cref="IInfluxDBClient"/>.</param>
    /// <remarks>Notes:
    /// <list type="bullet">
    /// <item><description>The <see cref="IInfluxDBClient"/> is registered as a singleton, meaning a single instance is shared throughout the application's lifetime.</description></item>
    /// </list>
    /// </remarks>
    public static void AddInfluxDBClient(
        this IHostApplicationBuilder builder,
        InfluxDBClientSettings settings)
    {
        builder.AddInfluxDBClientInternal(settings);
    }

    /// <summary>
    /// Registers <see cref="IInfluxDBClient"/> instance for connecting to an existing or new InfluxDB database with InfluxDB.Client, identified by a unique service key.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> used to add services.</param>
    /// <param name="serviceKey">A unique key that identifies this instance of the InfluxDB client service.</param>
    /// <param name="settings">The settings required to configure the <see cref="IInfluxDBClient"/>.</param>
    /// <remarks>Notes:
    /// <list type="bullet">
    /// <item><description>The <see cref="IInfluxDBClient"/> is registered as a singleton, meaning a single instance is shared throughout the application's lifetime.</description></item>
    /// </list>
    /// </remarks>
    public static void AddKeyedInfluxDBClient(
        this IHostApplicationBuilder builder,
        object serviceKey,
        InfluxDBClientSettings settings)
    {
        builder.AddInfluxDBClientInternal(settings, serviceKey);
    }

    private static void AddInfluxDBClientInternal(
        this IHostApplicationBuilder builder,
        InfluxDBClientSettings settings,
        object? serviceKey = null)
    {
        ValidateSettings(builder, settings);

        if (serviceKey is null)
        {
            builder.Services.AddTransient<IInfluxDBClient, InfluxDBClient>(
                sp => new InfluxDBClient(settings.ConnectionString));
        }
        else
        {
            builder.Services.AddKeyedTransient<IInfluxDBClient, InfluxDBClient>(
                serviceKey,
                (sp, serviceKey) => new InfluxDBClient(settings.ConnectionString));
        }

        if (!settings.DisableTracing)
        {
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    tracing.AddSource(ActivityNameSource);
                });
        }

        builder.AddHealthCheck(
            serviceKey is null ? "InfluxDB.Client" : $"InfluxDB.Client_{serviceKey}",
            settings);
    }

    private static void AddHealthCheck(
        this IHostApplicationBuilder builder,
        string healthCheckName,
        InfluxDBClientSettings settings)
    {
        if (settings.DisableHealthChecks)
        {
            return;
        }

        builder.TryAddHealthCheck(
            healthCheckName,
            healthCheck => healthCheck.AddInfluxDB(
                settings.ConnectionString!,
                healthCheckName,
                null,
                null,
                settings.HealthCheckTimeout > 0 ? TimeSpan.FromMilliseconds(settings.HealthCheckTimeout.Value) : null));
    }

    private static InfluxDBClientSettings GetInfluxDBClientSettings(
        this IHostApplicationBuilder builder,
        string connectionName,
        Action<InfluxDBClientSettings>? configureSettings)
    {
        var configSection = builder.Configuration.GetSection(DefaultConfigSectionName);
        var namedConfigSection = configSection.GetSection(connectionName);

        var settings = new InfluxDBClientSettings();
        configSection.Bind(settings);
        namedConfigSection.Bind(settings);

        var connectionString = builder.Configuration.GetConnectionString(connectionName);

        settings.ConnectionString = connectionString;

        configureSettings?.Invoke(settings);

        return settings;
    }

    private static void ValidateSettings(
        IHostApplicationBuilder builder,
        InfluxDBClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (string.IsNullOrEmpty(settings.ConnectionString))
        {
            throw new ArgumentNullException(nameof(settings.ConnectionString), "Connection string must be provided.");
        }

        if (!IsValidUrl(settings.ConnectionString, out _))
        {
            throw new ArgumentException($"The provided connection string '{settings.ConnectionString}' is invalid. Please provide a valid HTTP or HTTPS URL.");
        }
    }

    private static bool IsValidUrl(string url, out Uri? uriResult)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
