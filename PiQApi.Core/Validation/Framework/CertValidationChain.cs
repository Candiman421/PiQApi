// PiQApi.Core/Validation/Framework/CertValidationChain.cs
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Abstractions.Validation;
using PiQApi.Core.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Validation.Framework;

/// <summary>
/// Chain of validation processors that executes them in sequence
/// </summary>
public partial class CertValidationChain
{
    private readonly List<ICertValidationProcessor> _processors = new();
    private readonly ILogger<CertValidationChain> _logger;
    private readonly string _chainName;
    private readonly ICertTimeProvider _timeProvider;
    private readonly ICertValidationResultFactory _resultFactory;

    /// <summary>
    /// Gets the number of processors in the chain
    /// </summary>
    public int ProcessorCount => _processors.Count;

    /// <summary>
    /// Creates a new validation chain
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="resultFactory">Factory to create validation results</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    public CertValidationChain(
        ILogger<CertValidationChain> logger,
        ICertValidationResultFactory resultFactory,
        ICertTimeProvider? timeProvider = null)
        : this(logger, "DefaultChain", resultFactory, timeProvider)
    {
    }

    /// <summary>
    /// Creates a new validation chain with a specified name
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="chainName">Name of the chain for logging</param>
    /// <param name="resultFactory">Factory to create validation results</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    public CertValidationChain(
        ILogger<CertValidationChain> logger,
        string chainName,
        ICertValidationResultFactory resultFactory,
        ICertTimeProvider? timeProvider = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _chainName = chainName ?? "DefaultChain";
        _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
        _timeProvider = timeProvider ?? CertTimeProviderFactory.Current;
    }

    /// <summary>
    /// Adds a processor to the chain
    /// </summary>
    /// <param name="processor">Processor to add</param>
    /// <returns>This chain for method chaining</returns>
    public CertValidationChain AddProcessor(ICertValidationProcessor processor)
    {
        ArgumentNullException.ThrowIfNull(processor);
        _processors.Add(processor);
        return this;
    }

    /// <summary>
    /// Clears all processors from the chain
    /// </summary>
    /// <returns>This chain for method chaining</returns>
    public CertValidationChain ClearProcessors()
    {
        _processors.Clear();
        return this;
    }
}
