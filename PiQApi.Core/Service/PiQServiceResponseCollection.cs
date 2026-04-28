// PiQApi.Core/Service/PiQServiceResponseCollection.cs
using System.Collections.ObjectModel;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Service;
using PiQApi.Core.Exceptions.Base;

namespace PiQApi.Core.Service;

/// <summary>
/// Collection of service responses
/// </summary>
/// <typeparam name="T">Type of service response</typeparam>
public sealed class PiQServiceResponseCollection<T> : Collection<T> where T : IPiQServiceResponse
{
    /// <summary>
    /// Initializes a new instance of the PiQServiceResponseCollection class
    /// </summary>
    public PiQServiceResponseCollection()
        : base(new List<T>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the PiQServiceResponseCollection class
    /// </summary>
    /// <param name="items">Initial collection items</param>
    public PiQServiceResponseCollection(IList<T> items)
        : base(items ?? new List<T>())
    {
    }

    /// <summary>
    /// Gets whether any responses have errors
    /// </summary>
    public bool HasErrors => this.Any(r => !r.IsSuccessful);

    /// <summary>
    /// Gets whether all responses are successful
    /// </summary>
    public bool AllSuccessful => this.All(r => r.IsSuccessful);

    /// <summary>
    /// Gets whether there is at least one success and one failure
    /// </summary>
    public bool HasPartialSuccess => this.Any(r => r.IsSuccessful) && this.Any(r => !r.IsSuccessful);

    /// <summary>
    /// Gets all error messages from failed responses
    /// </summary>
    public IEnumerable<string> GetAllErrors() =>
        this.Where(r => !r.IsSuccessful && !string.IsNullOrEmpty(r.ErrorMessage))
            .Select(r => r.ErrorMessage)
            .Where(msg => msg != null);

    /// <summary>
    /// Checks if any errors are of the specified type
    /// </summary>
    public bool HasErrorOfType<TError>() where TError : PiQServiceException =>
        this.Any(r => r.Exception is TError);

    /// <summary>
    /// Gets all errors of the specified type
    /// </summary>
    public IEnumerable<TError> GetErrorsOfType<TError>() where TError : PiQServiceException =>
        this.Where(r => r.Exception is TError)
            .Select(r => r.Exception as TError)
            .Where(e => e != null)!;

    /// <summary>
    /// Validates that all responses are successful
    /// </summary>
    public Task<bool> ValidateAllAsync(CancellationToken _ = default)
    {
        return Task.FromResult(this.All(r => r.IsSuccessful));
    }

    /// <summary>
    /// Gets all successful results
    /// </summary>
    public IEnumerable<object> SuccessfulResults =>
        this.Where(r => r.IsSuccessful && r.Result != null)
            .Select(r => r.Result!)
            .Where(r => r != null);

    /// <summary>
    /// Gets all results (successful or not)
    /// </summary>
    public IEnumerable<object> AllResults =>
        this.Where(r => r.Result != null)
            .Select(r => r.Result!)
            .Where(r => r != null);

    /// <summary>
    /// Gets the aggregate status of all responses
    /// </summary>
    public ResultStatusType GetAggregateStatus() =>
        !this.Any() ? ResultStatusType.Success :
        AllSuccessful ? ResultStatusType.Success :
        HasPartialSuccess ? ResultStatusType.Partial :
        ResultStatusType.Failed;

    /// <summary>
    /// Adds a response to the collection
    /// </summary>
    public new void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        base.Add(item);
    }
}