// PiQApi.Abstractions/Enums/ConnectionStateType.cs
namespace PiQApi.Abstractions.Enums;

/// <summary>
/// Defines connection states for service operations.
/// </summary>
public enum ConnectionStateType
{
    /// <summary>
    /// Connection has not been initialized.
    /// </summary>
    NotInitialized = 0,

    /// <summary>
    /// Connection is connecting.
    /// </summary>
    Connecting = 1,

    /// <summary>
    /// Connection is established.
    /// </summary>
    Connected = 2,

    /// <summary>
    /// Connection is disconnected.
    /// </summary>
    Disconnected = 3,

    /// <summary>
    /// Connection has failed.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Connection is in recovery.
    /// </summary>
    Recovering = 5,

    /// <summary>
    /// Circuit is open (circuit breaker pattern).
    /// </summary>
    CircuitOpen = 6
}