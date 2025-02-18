namespace CommunityToolkit.Aspire.Hosting.InfluxDB;

/// <summary>
/// Represents the settings for configuring a InfluxDB server resource.
/// </summary>
public class InfluxDBServerSettings
{
    /// <summary>
    /// The internal URL for the InfluxDB server.
    /// If not specified, the container resource will automatically assign a random URL.
    /// </summary>
    public string? ServerUrl { get; set; }

    /// <summary>
    /// Protected constructor to allow inheritance but prevent direct instantiation.
    /// </summary>
    protected InfluxDBServerSettings() { }

    /// <summary>
    /// Creates an unsecured InfluxDB server settings object with default settings.
    /// </summary>
    public static InfluxDBServerSettings Unsecured() => new();
}