// CertApi.Abstractions/Validation/Constants/CertFileExtensions.cs
namespace CertApi.Abstractions.Validation.Constants;

/// <summary>
/// Defines standard file extensions used across the service
/// </summary>
public static class CertFileExtensions
{
    /// <summary>
    /// Default allowed file extensions
    /// </summary>
    public static readonly string[] DefaultAllowedExtensions = {
        ".txt", ".pdf", ".doc", ".docx",
        ".xls", ".xlsx", ".ppt", ".pptx",
        ".jpg", ".jpeg", ".png", ".gif"
    };

    /// <summary>
    /// Image file extensions
    /// </summary>
    public static readonly string[] ImageExtensions = {
        ".jpg", ".jpeg", ".png", ".gif",
        ".bmp", ".tif", ".tiff", ".svg"
    };

    /// <summary>
    /// Document file extensions
    /// </summary>
    public static readonly string[] DocumentExtensions = {
        ".txt", ".pdf", ".doc", ".docx",
        ".rtf", ".odt", ".md", ".markdown"
    };

    /// <summary>
    /// Spreadsheet file extensions
    /// </summary>
    public static readonly string[] SpreadsheetExtensions = {
        ".xls", ".xlsx", ".csv", ".tsv",
        ".ods", ".numbers"
    };

    /// <summary>
    /// Presentation file extensions
    /// </summary>
    public static readonly string[] PresentationExtensions = {
        ".ppt", ".pptx", ".key", ".odp"
    };

    /// <summary>
    /// Dangerous file extensions that should be blocked
    /// </summary>
    public static readonly string[] DangerousExtensions = {
        ".exe", ".dll", ".bat", ".cmd",
        ".msi", ".ps1", ".vbs", ".js",
        ".reg", ".com"
    };
}
