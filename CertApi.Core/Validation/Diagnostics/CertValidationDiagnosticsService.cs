// CertApi.Core/Validation/Diagnostics/CertValidationDiagnosticsService.cs
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Validation;
using CertApi.Abstractions.Validation.Constants;
using System.Globalization;
using System.Text;

namespace CertApi.Core.Validation.Diagnostics;

/// <summary>
/// Service for validation diagnostics
/// </summary>
public class CertValidationDiagnosticsService : ICertValidationDiagnosticsService
{
    // Cache composite formats for frequently used format strings
    private static readonly CompositeFormat SummaryFormatComposite =
        System.Text.CompositeFormat.Parse(CertValidationMessages.SummaryFormat);

    private static readonly CompositeFormat GroupHeaderFormatComposite =
        System.Text.CompositeFormat.Parse(CertValidationMessages.GroupHeaderFormat);

    /// <summary>
    /// Gets detailed diagnostic information about a validation result
    /// </summary>
    /// <param name="result">The validation result to diagnose</param>
    /// <returns>A string containing diagnostic information</returns>
    public string GetDiagnosticInfo(ICertValidationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var sb = new StringBuilder();

        // Basic information
        _ = sb.AppendLine("Validation Result Diagnostics");
        _ = sb.AppendLine("==========================");
        _ = sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Is Valid: {0}", result.IsValid));
        _ = sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Error Count: {0}", result.Errors.Count));
        _ = sb.AppendLine();

        // Error counts by severity
        var errorCount = result.Errors.Count(e => e.Severity == ValidationSeverityType.Error);
        var warningCount = result.Errors.Count(e => e.Severity == ValidationSeverityType.Warning);
        var infoCount = result.Errors.Count(e => e.Severity == ValidationSeverityType.Info);

        _ = sb.AppendLine("Error Summary");
        _ = sb.AppendLine("------------");
        _ = sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
            SummaryFormatComposite,
            result.Errors.Count, errorCount, warningCount, infoCount));
        _ = sb.AppendLine();

        // Exception details if any
        if (result.Exception != null)
        {
            _ = sb.AppendLine("Exception Information");
            _ = sb.AppendLine("--------------------");
            _ = sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Exception Type: {0}", result.Exception.GetType().Name));
            _ = sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Message: {0}", result.Exception.Message));
            _ = sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Stack Trace: {0}", result.Exception.StackTrace));
            _ = sb.AppendLine();
        }

        // Context values if any
        if (result.Context.Count > 0)
        {
            _ = sb.AppendLine("Validation Context");
            _ = sb.AppendLine("-----------------");
            foreach (var kvp in result.Context)
            {
                _ = sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", kvp.Key, kvp.Value));
            }
            _ = sb.AppendLine();
        }

        // Detailed error information
        if (result.Errors.Count > 0)
        {
            _ = sb.AppendLine("Detailed Errors");
            _ = sb.AppendLine("---------------");

            foreach (var error in result.Errors)
            {
                _ = sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Code: {0}", error.Code));
                _ = sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Message: {0}", error.Message));
                _ = sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Severity: {0}", error.Severity));

                if (!string.IsNullOrEmpty(error.PropertyPath))
                {
                    _ = sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Property Path: {0}", error.PropertyPath));
                }

                _ = sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets a detailed error report from a validation result
    /// </summary>
    /// <param name="result">The validation result to analyze</param>
    /// <returns>A formatted error report</returns>
    public string GetErrorReport(ICertValidationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsValid)
        {
            return CertValidationMessages.ValidationSuccessful;
        }

        var sb = new StringBuilder();

        if (result.Errors.Count == 0)
        {
            return CertValidationMessages.ValidationFailed;
        }

        // Group errors by severity
        var errorsByProperty = result.Errors
            .GroupBy(e => e.PropertyName)
            .OrderBy(g => g.Key);

        foreach (var group in errorsByProperty)
        {
            var propertyName = string.IsNullOrEmpty(group.Key) ? "General" : group.Key;
            var errorCount = group.Count();
            var singleOrMultiple = errorCount == 1 ? CertValidationMessages.SingleIssue : CertValidationMessages.MultipleIssues;

            _ = sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                GroupHeaderFormatComposite,
                propertyName, errorCount, singleOrMultiple));

            foreach (var error in group)
            {
                _ = sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "{0}{1}: {2}",
                    CertValidationMessages.ListItemPrefix, error.Code, error.Message));
            }

            _ = sb.AppendLine();
        }

        return sb.ToString();
    }
}