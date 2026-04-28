// PiQApi.Core/Operations/CertOperationBase.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Operations;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Core.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Operations;

/// <summary>
/// Base implementation for operations
/// </summary>
public abstract partial class CertOperationBase : ICertOperationBase, IAsyncDisposable
{
    private readonly ICertAsyncLock _asyncLock;
    private readonly ILogger _logger;
    private readonly ICertTimeProvider _timeProvider;
    private TimeSpan? _operationStartTime;
    private bool _isDisposed;

    /// <summary>
    /// Gets the current correlation identifier
    /// </summary>
    public string CorrelationId => Context?.CorrelationContext?.CorrelationId ?? string.Empty;

    /// <summary>
    /// Gets the operation identifier
    /// </summary>
    public string OperationId => Context?.Identifier?.Id ?? string.Empty;

    /// <summary>
    /// Gets whether the operation is currently active
    /// </summary>
    public bool IsActive => !_isDisposed && Context?.State?.IsInProgress == true;

    /// <summary>
    /// Gets whether the operation is ready for use
    /// </summary>
    public bool IsReady { get; protected set; }

    /// <summary>
    /// Gets the operation context
    /// </summary>
    public ICertOperationContext Context { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertOperationBase"/> class
    /// </summary>
    /// <param name="context">Operation context</param>
    /// <param name="asyncLock">Asynchronous lock for thread-safety</param>
    /// <param name="logger">Logger</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    protected CertOperationBase(
        ICertOperationContext context,
        ICertAsyncLock asyncLock,
        ILogger logger,
        ICertTimeProvider? timeProvider = null)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        _asyncLock = asyncLock ?? throw new ArgumentNullException(nameof(asyncLock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? CertTimeProviderFactory.Current;
    }

    /// <summary>
    /// Throws an exception if the operation is disposed
    /// </summary>
    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, GetType().Name);
    }
}