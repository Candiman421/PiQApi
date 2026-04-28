// CertApi.Abstractions/Enums/BuilderStateType.cs
namespace CertApi.Abstractions.Enums;

/// <summary>
/// Represents the current state of a builder pattern implementation
/// </summary>
public enum BuilderStateType
{
    /// <summary>
    /// Builder has been created but no configuration has started
    /// </summary>
    New = 0,

    /// <summary>
    /// Builder is currently accepting configuration changes
    /// </summary>
    Configuring = 1,

    /// <summary>
    /// Builder has all required configuration and is ready to build
    /// </summary>
    Ready = 2,

    /// <summary>
    /// Builder is currently executing the build operation
    /// </summary>
    Building = 3,

    /// <summary>
    /// Build operation completed successfully
    /// </summary>
    Complete = 4,

    /// <summary>
    /// Build operation failed
    /// </summary>
    Failed = 5,

    /// <summary>
    /// Builder is validating its configuration
    /// </summary>
    Validating = 6,

    /// <summary>
    /// Builder has invalid configuration and needs correction
    /// </summary>
    Invalid = 7,

    /// <summary>
    /// Builder has been reset and can be reconfigured
    /// </summary>
    Reset = 8,

    /// <summary>
    /// Builder is locked and cannot accept further changes
    /// </summary>
    Locked = 9,

    /// <summary>
    /// Builder has been disposed and cannot be used
    /// </summary>
    Disposed = 10
}