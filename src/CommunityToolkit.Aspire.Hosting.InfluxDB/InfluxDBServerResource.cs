namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// Represents a resource for InfluxDB container.
/// </summary>
public class InfluxDBServerResource(string name, bool isSecured = false) : ContainerResource(name), IResourceWithConnectionString
{
    /// <summary>
    /// Gets the protocol used for the primary endpoint, based on the security setting ("http" or "https").
    /// </summary>
    internal string PrimaryEndpointName => isSecured ? "https" : "http";

    private EndpointReference? _primaryEndpoint;

    /// <summary>
    /// Gets the primary endpoint for the InfluxDB server.
    /// </summary>
    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);

    /// <summary>
    /// Gets the connection string expression for the InfluxDB server, 
    /// formatted as "http(s)://{Host}:{Port}" depending on the security setting.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression => ReferenceExpression.Create(
        $"URL={PrimaryEndpointName}://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}");
}
