// CertApi.Abstractions/Enums/ValidationSeverityType.cs
namespace CertApi.Abstractions.Enums;

/// <summary>
/// Specifies the severity level of a validation error
/// </summary>
public enum ValidationSeverityType
{
    /// <summary>
    /// ResultError that prevents operation from proceeding
    /// </summary>
    Error = 0,

    /// <summary>
    /// Warning that operation can proceed with caution
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Informational message without impact on operation
    /// </summary>
    Info = 2
}