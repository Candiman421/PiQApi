// PiQApi.Core/Exceptions/Infrastructure/PiQResourceQuotaExceededException.cs
using PiQApi.Core.Exceptions.Base;

namespace PiQApi.Core.Exceptions.Infrastructure;

/// <summary>
/// Exception thrown when a resource quota or limit is exceeded
/// </summary>
public sealed class PiQResourceQuotaExceededException : PiQServiceException
{
    /// <summary>
    /// Gets the type of resource with exceeded quota
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// Gets the current size of the resource
    /// </summary>
    public long CurrentSize { get; }

    /// <summary>
    /// Gets the maximum allowed size of the resource
    /// </summary>
    public long MaxSize { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQResourceQuotaExceededException"/> class
    /// </summary>
    public PiQResourceQuotaExceededException()
        : base("Resource quota exceeded", "QuotaExceeded")
    {
        ResourceType = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQResourceQuotaExceededException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public PiQResourceQuotaExceededException(string message)
        : base(message, "QuotaExceeded")
    {
        ResourceType = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQResourceQuotaExceededException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQResourceQuotaExceededException(string message, Exception? inner)
        : base(message, "QuotaExceeded", inner)
    {
        ResourceType = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQResourceQuotaExceededException"/> class with a specified resource type,
    /// current size, and maximum size
    /// </summary>
    /// <param name="resourceType">The type of resource with exceeded quota</param>
    /// <param name="currentSize">The current size of the resource</param>
    /// <param name="maxSize">The maximum allowed size of the resource</param>
    public PiQResourceQuotaExceededException(string resourceType, long currentSize, long maxSize)
        : base($"Quota exceeded for resource type {resourceType}. Current: {currentSize}, Max: {maxSize}", "QuotaExceeded")
    {
        ResourceType = resourceType ?? string.Empty;
        CurrentSize = currentSize;
        MaxSize = maxSize;

        if (!string.IsNullOrEmpty(resourceType))
            AddData(nameof(ResourceType), resourceType);

        AddData(nameof(CurrentSize), currentSize);
        AddData(nameof(MaxSize), maxSize);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQResourceQuotaExceededException"/> class with a specified resource type,
    /// current size, maximum size, and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="resourceType">The type of resource with exceeded quota</param>
    /// <param name="currentSize">The current size of the resource</param>
    /// <param name="maxSize">The maximum allowed size of the resource</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQResourceQuotaExceededException(string resourceType, long currentSize, long maxSize, Exception? inner)
        : base($"Quota exceeded for resource type {resourceType}. Current: {currentSize}, Max: {maxSize}", "QuotaExceeded", inner)
    {
        ResourceType = resourceType ?? string.Empty;
        CurrentSize = currentSize;
        MaxSize = maxSize;

        if (!string.IsNullOrEmpty(resourceType))
            AddData(nameof(ResourceType), resourceType);

        AddData(nameof(CurrentSize), currentSize);
        AddData(nameof(MaxSize), maxSize);
    }
}