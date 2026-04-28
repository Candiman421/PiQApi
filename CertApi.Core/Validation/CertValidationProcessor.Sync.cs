// CertApi.Core/Validation/CertValidationProcessor.Sync.cs
using CertApi.Abstractions.Validation;
using CertApi.Abstractions.Validation.Models;

namespace CertApi.Core.Validation;

public partial class CertValidationProcessor
{
    /// <summary>
    /// Validates an entity
    /// </summary>
    public ICertValidationResult Validate<T>(T entity, CertValidationContext context) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(context);

        var rules = GetRulesForType<T>().ToList();

        if (rules.Count == 0)
        {
            LogWarningNoRules(_logger, typeof(T).Name, null);
            return _factory.Success();
        }

        LogDebugValidating(_logger, typeof(T).Name, rules.Count, null);

        var results = rules.Select(rule => rule.Validate(entity, context)).ToArray();
        return _factory.Combine(results);
    }

    /// <summary>
    /// Validates an object
    /// </summary>
    public ICertValidationResult Validate(object entity, CertValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(context);

        var entityType = entity.GetType();
        var validationMethod = GetType().GetMethod(nameof(Validate), new[] { entityType, typeof(CertValidationContext) });

        if (validationMethod == null)
        {
            LogWarningNoValidationMethod(_logger, entityType.Name, null);
            return _factory.Success();
        }

        try
        {
            // Fix for CS8600 and CS8603 - Handle possible null result
            var result = validationMethod.Invoke(this, new[] { entity, context }) as ICertValidationResult;

            return result ?? _factory.FromException(
                new InvalidOperationException($"Validation method invocation returned null for {entityType.Name}"));
        }
        catch (Exception ex)
        {
            LogErrorValidating(_logger, entityType.Name, ex);
            return _factory.FromException(ex);
        }
    }
}