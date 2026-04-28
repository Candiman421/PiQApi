// PiQApi.Core/Context/CertOperationContext.Validation.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Results;
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;
using PiQApi.Core.Validation;

namespace PiQApi.Core.Context;

public partial class CertOperationContext
{
    /// <summary>
    /// Validates an entity asynchronously
    /// </summary>
    public Task<ICertValidationResult> ValidateAsync<T>(T entity, ValidationModeType mode, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        return _validator.ValidateAsync(entity, mode, cancellationToken);
    }

    /// <summary>
    /// Validates an entity as required asynchronously
    /// </summary>
    public Task<ICertValidationResult> ValidateRequiredAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        return _validator.ValidateRequiredAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Validates an entity and creates a result asynchronously
    /// </summary>
    public Task<ICertResult<T>> ValidateAndCreateResultAsync<T>(T entity, ValidationModeType mode, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        return _validator.ValidateAndCreateResultAsync(entity, mode, cancellationToken);
    }

    /// <summary>
    /// Creates a validation context
    /// </summary>
    public ICertValidationContext CreateValidationContext(ValidationModeType mode = ValidationModeType.Standard, CancellationToken? cancellationToken = null)
    {
        return _validator.CreateValidationContext(mode, cancellationToken);
    }
}