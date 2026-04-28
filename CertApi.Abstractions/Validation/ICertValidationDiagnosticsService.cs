// CertApi.Abstractions/Validation/ICertValidationDiagnosticsService.cs
namespace CertApi.Abstractions.Validation;

/// <summary>
/// Interface for validation diagnostics service
/// </summary>
public interface ICertValidationDiagnosticsService
{
    /// <summary>
    /// Gets detailed diagnostic information about a validation result
    /// </summary>
    /// <param name="result">The validation result to diagnose</param>
    /// <returns>A string containing diagnostic information</returns>
    string GetDiagnosticInfo(ICertValidationResult result);

    /// <summary>
    /// Gets a detailed error report from a validation result
    /// </summary>
    /// <param name="result">The validation result to analyze</param>
    /// <returns>A formatted error report</returns>
    string GetErrorReport(ICertValidationResult result);
}