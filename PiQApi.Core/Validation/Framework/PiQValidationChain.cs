// PiQApi.Core/Validation/Framework/PiQValidationChain.cs
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Abstractions.Validation;
using PiQApi.Core.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Validation.Framework;

/// <summary>
/// Chain of validation processors that executes them in sequence
/// </summary>
public partial class PiQValidationChain
{
    private readonly List<IPiQValidationProcessor> _processors = new();
    private readonly ILogger<PiQValidationChain> _logger;
    private readonly string _chainName;
    private readonly IPiQTimeProvider _timeProvider;
    private readonly IPiQValidationResultFactory _resultFactory;

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
    public PiQValidationChain(
        ILogger<PiQValidationChain> logger,
        IPiQValidationResultFactory resultFactory,
        IPiQTimeProvider? timeProvider = null)
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
    public PiQValidationChain(
        ILogger<PiQValidationChain> logger,
        string chainName,
        IPiQValidationResultFactory resultFactory,
        IPiQTimeProvider? timeProvider = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _chainName = chainName ?? "DefaultChain";
        _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
        _timeProvider = timeProvider ?? PiQTimeProviderFactory.Current;
    }

    /// <summary>
    /// Adds a processor to the chain
    /// </summary>
    /// <param name="processor">Processor to add</param>
    /// <returns>This chain for method chaining</returns>
    public PiQValidationChain AddProcessor(IPiQValidationProcessor processor)
    {
        ArgumentNullException.ThrowIfNull(processor);
        _processors.Add(processor);
        return this;
    }

    /// <summary>
    /// Clears all processors from the chain
    /// </summary>
    /// <returns>This chain for method chaining</returns>
    public PiQValidationChain ClearProcessors()
    {
        _processors.Clear();
        return this;
    }
}
