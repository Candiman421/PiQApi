// PiQApi.Abstractions/Enums/ClientCertificateOptionsType.cs
namespace PiQApi.Abstractions.Enums;

/// <summary>
/// Options for client certificate authentication
/// </summary>
public class ClientCertificateOptionsType
{
    /// <summary>
    /// Gets or sets the certificate store location
    /// </summary>
    public string? StoreLocation { get; set; }

    /// <summary>
    /// Gets or sets the certificate store name
    /// </summary>
    public string? StoreName { get; set; }

    /// <summary>
    /// Gets or sets the certificate thumbprint
    /// </summary>
    public string? Thumbprint { get; set; }

    /// <summary>
    /// Gets or sets the certificate file path
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the certificate password
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets whether to validate the certificate
    /// </summary>
    public bool ValidateCertificate { get; set; } = true;

    /// <summary>
    /// Validates the certificate options
    /// </summary>
    /// <returns>True if options are valid; otherwise, false</returns>
    public bool Validate()
    {
        // Either store-based or file-based certificate must be specified
        var hasStoreInfo = !string.IsNullOrEmpty(StoreLocation) &&
                            !string.IsNullOrEmpty(StoreName) &&
                            !string.IsNullOrEmpty(Thumbprint);

        var hasFileInfo = !string.IsNullOrEmpty(FilePath);

        return hasStoreInfo || hasFileInfo;
    }
}
