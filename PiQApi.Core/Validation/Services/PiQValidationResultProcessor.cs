// PiQApi.Core/Validation/Services/PiQValidationResultProcessor.cs
using System.Globalization;
using System.Text;
using PiQApi.Abstractions.Validation;

namespace PiQApi.Core.Validation.Services;

/// <summary>
/// Service for processing validation results
/// </summary>
public static class PiQValidationResultProcessor
{
    /// <summary>
    /// Gets the correlation ID from a validation result
    /// </summary>
    /// <param name="result">The validation result</param>
    /// <returns>The correlation ID, or null if not found</returns>
    public static string? GetCorrelationId(IPiQValidationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Context.TryGetValue("CorrelationId", out var correlationId) && correlationId is string id)
        {
            return id;
        }

        return null;
    }

    /// <summary>
    /// Gets a summary of all error messages in the validation result
    /// </summary>
    /// <param name="result">The validation result</param>
    /// <returns>A summary of all error messages</returns>
    public static string GetErrorSummary(IPiQValidationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (!result.IsValid || result.Errors.Count == 0)
        {
            return string.Empty;
        }

        if (result.Errors.Count == 1)
        {
            return result.Errors[0].Message;
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Multiple validation errors:");

        for (int i = 0; i < result.Errors.Count; i++)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $" - {result.Errors[i].Message}");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets the validation context from the result
    /// </summary>
    /// <param name="result">The validation result</param>
    /// <returns>The validation context</returns>
    public static IReadOnlyDictionary<string, object> GetValidationContext(IPiQValidationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Context;
    }
}
