// PiQApi.Core/Validation/PiQValidationRuleFactory.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PiQApi.Abstractions.Validation;

namespace PiQApi.Core.Validation;

/// <summary>
/// Factory for creating validation rules
/// </summary>
public class PiQValidationRuleFactory : IPiQValidationRuleFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PiQValidationRuleFactory> _logger;

    #region LoggerMessage Delegates
    private static readonly Action<ILogger, int, string, Exception?> _logRetrievedRules =
        LoggerMessage.Define<int, string>(
            LogLevel.Debug,
            new EventId(1, nameof(GetRulesForEntity)),
            "Retrieved {Count} validation rules for type {EntityType}");

    private static readonly Action<ILogger, string, Exception> _logErrorRetrievingRules =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2, nameof(GetRulesForEntity)),
            "Error retrieving validation rules for type {EntityType}");

    private static readonly Action<ILogger, string, string, Exception?> _logFailedToResolveRule =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(3, nameof(CreateRule)),
            "Failed to resolve validation rule of type {RuleType} for entity {EntityType}");

    private static readonly Action<ILogger, string, Exception> _logErrorCreatingRule =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(4, nameof(CreateRule)),
            "Error creating validation rule of type {RuleType}");
    #endregion

    /// <summary>
    /// Initializes a new instance of the PiQValidationRuleFactory class
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve rules</param>
    /// <param name="logger">The logger</param>
    public PiQValidationRuleFactory(
        IServiceProvider serviceProvider,
        ILogger<PiQValidationRuleFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all rules for a specific entity type
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <returns>Collection of validation rules for the entity type</returns>
    public IEnumerable<IPiQValidationRule<TEntity>> GetRulesForEntity<TEntity>() where TEntity : class
    {
        try
        {
            var rules = _serviceProvider.GetServices<IPiQValidationRule<TEntity>>().ToList();
            _logRetrievedRules(_logger, rules.Count, typeof(TEntity).Name, null);
            return rules;
        }
        catch (Exception ex)
        {
            _logErrorRetrievingRules(_logger, typeof(TEntity).Name, ex);
            return Enumerable.Empty<IPiQValidationRule<TEntity>>();
        }
    }

    /// <summary>
    /// Creates a validation rule instance of the specified type
    /// </summary>
    /// <typeparam name="T">Type of the validation rule</typeparam>
    /// <returns>Instance of the validation rule</returns>
    public T CreateRule<T>() where T : IPiQValidationRule
    {
        try
        {
            var rule = _serviceProvider.GetService<T>();
            if (rule == null)
            {
                _logFailedToResolveRule(_logger, typeof(T).Name, "unknown", null);
                throw new InvalidOperationException($"Unable to resolve validation rule {typeof(T).Name}");
            }

            return rule;
        }
        catch (Exception ex)
        {
            _logErrorCreatingRule(_logger, typeof(T).Name, ex);
            throw;
        }
    }
}