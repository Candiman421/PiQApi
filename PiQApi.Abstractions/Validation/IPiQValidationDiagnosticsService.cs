// PiQApi.Abstractions/Validation/IPiQValidationDiagnosticsService.cs
namespace PiQApi.Abstractions.Validation;

/// <summary>
/// Interface for validation diagnostics service
/// </summary>
public interface IPiQValidationDiagnosticsService
{
    /// <summary>
    /// Gets detailed diagnostic information about a validation result
    /// </summary>
    /// <param name="result">The validation result to diagnose</param>
    /// <returns>A string containing diagnostic information</returns>
    string GetDiagnosticInfo(IPiQValidationResult result);

    /// <summary>
    /// Gets a detailed error report from a validation result
    /// </summary>
    /// <param name="result">The validation result to analyze</param>
    /// <returns>A formatted error report</returns>
    string GetErrorReport(IPiQValidationResult result);
}