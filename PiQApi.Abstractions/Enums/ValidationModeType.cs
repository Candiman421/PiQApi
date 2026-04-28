// PiQApi.Abstractions/Enums/ValidationModeType.cs
namespace PiQApi.Abstractions.Enums;

/// <summary>
/// Defines validation modes
/// </summary>
public enum ValidationModeType
{
    /// <summary>
    /// Standard validation mode
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Only required properties are validated
    /// </summary>
    Required = 1,

    /// <summary>
    /// Full validation including all properties
    /// </summary>
    Full = 2,

    /// <summary>
    /// Light validation for quick checks
    /// </summary>
    Light = 3,

    /// <summary>
    /// Strict validation enforcing all rules
    /// </summary>
    Strict = 4,

    /// <summary>
    /// Lenient validation that continues after encountering errors
    /// </summary>
    Lenient = 5
}