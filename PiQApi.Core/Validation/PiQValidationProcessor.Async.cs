// PiQApi.Core/Validation/PiQValidationProcessor.Async.cs
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;

namespace PiQApi.Core.Validation;

public partial class PiQValidationProcessor
{
    /// <summary>
    /// Validates an entity asynchronously
    /// </summary>
    public async Task<IPiQValidationResult> ValidateAsync<T>(T entity, PiQValidationContext context, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rules = GetRulesForType<T>().ToList();

            if (rules.Count == 0)
            {
                LogWarningNoRules(_logger, typeof(T).Name, null);
                return _factory.Success();
            }

            LogDebugValidatingAsync(_logger, typeof(T).Name, rules.Count, null);

            var tasks = rules.Select(rule => rule.ValidateAsync(entity, context, cancellationToken));
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            return _factory.Combine(results);
        }
        catch (OperationCanceledException)
        {
            LogInfoCancelled(_logger, typeof(T).Name, null);
            return _factory.CreateCancelled(context.CorrelationId);
        }
        catch (Exception ex)
        {
            LogErrorValidatingAsync(_logger, typeof(T).Name, ex);
            return _factory.FromException(ex);
        }
    }

    /// <summary>
    /// Validates an object asynchronously
    /// </summary>
    public async Task<IPiQValidationResult> ValidateAsync(object entity, PiQValidationContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entityType = entity.GetType();
            var methodName = nameof(ValidateAsync);
            var validateAsyncMethod = GetType().GetMethod(
                methodName,
                new[] { entityType, typeof(PiQValidationContext), typeof(CancellationToken) });

            if (validateAsyncMethod == null)
            {
                LogWarningNoValidationMethod(_logger, entityType.Name, null);
                return _factory.Success();
            }

            // Fix for CS8600 and CS8602 - Properly handle possible null
            var task = validateAsyncMethod.Invoke(
                this, new object[] { entity, context, cancellationToken }) as Task<IPiQValidationResult>;

            if (task == null)
            {
                LogErrorValidatingAsync(_logger, entityType.Name,
                    new InvalidOperationException($"Failed to invoke validation method for {entityType.Name}"));
                return _factory.FromException(new InvalidOperationException($"Validation method invocation failed for {entityType.Name}"));
            }

            return await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            LogInfoCancelled(_logger, entity.GetType().Name, null);
            return _factory.CreateCancelled(context.CorrelationId);
        }
        catch (Exception ex)
        {
            LogErrorValidatingAsync(_logger, entity.GetType().Name, ex);
            return _factory.FromException(ex);
        }
    }
}