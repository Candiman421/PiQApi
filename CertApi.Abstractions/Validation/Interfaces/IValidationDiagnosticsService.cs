// CertApi.Abstractions/Validation/Interfaces/IValidationDiagnosticsService.cs
namespace CertApi.Abstractions.Validation.Interfaces
{
    /// <summary>
    /// Service interface for validation diagnostics operations
    /// </summary>
    public interface IValidationDiagnosticsService
    {
        /// <summary>
        /// Gets detailed diagnostic information about a validation result
        /// </summary>
        /// <param name="result">The validation result to diagnose</param>
        /// <returns>A string containing diagnostic information</returns>
        string GetDiagnosticInfo(CertValidationResult result);

        /// <summary>
        /// Gets a detailed error report from a validation result
        /// </summary>
        /// <param name="result">The validation result to analyze</param>
        /// <returns>A formatted error report</returns>
        string GetErrorReport(CertValidationResult result);
    }
}