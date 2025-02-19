﻿using System.Security.Cryptography.X509Certificates;

namespace CommunityToolkit.Aspire.InfluxDB.Client;

/// <summary>
/// Provides the client configuration settings for connecting to a InfluxDB database.
/// </summary>
public sealed class InfluxDBClientSettings
{
    /// <summary>
    /// The connection string of the InfluxDB server.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// The path to the certificate file.
    /// </summary>
    public string? CertificatePath { get; set; }

    /// <summary>
    /// The password for the certificate.
    /// </summary>
    public string? CertificatePassword { get; set; }

    /// <summary>
    /// The certificate for InfluxDB server.
    /// </summary>
    public X509Certificate2? Certificate { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether InfluxDB health check is disabled or not.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool DisableHealthChecks { get; set; }

    /// <summary>
    /// Gets or sets the timeout in milliseconds for the InfluxDB health check.
    /// </summary>
    public int? HealthCheckTimeout { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether OpenTelemetry tracing is disabled.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool DisableTracing { get; set; }

    /// <summary>
    /// Retrieves the <see cref="X509Certificate2"/> used for authentication, if a certificate path is specified.
    /// </summary>
    /// <returns>An <see cref="X509Certificate2"/> instance if the <see cref="CertificatePath"/> is specified;
    /// otherwise, <see langword="null"/>.</returns>
    internal X509Certificate2? GetCertificate()
    {
        if (Certificate != null)
            return Certificate;

        if (string.IsNullOrEmpty(CertificatePath))
        {
            return null;
        }

#pragma warning disable SYSLIB0057
        return new X509Certificate2(CertificatePath, CertificatePassword);
#pragma warning restore SYSLIB0057
    }
}
