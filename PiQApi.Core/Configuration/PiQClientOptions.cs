// PiQApi.Core/Configuration/PiQClientOptions.cs
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace PiQApi.Core.Configuration;

/// <summary>
/// Client options for service connections
/// </summary>
public record PiQClientOptions
{
    private readonly Dictionary<string, string> _defaultHeaders = new();

    /// <summary>
    /// Gets or sets the timeout in seconds
    /// </summary>
    [Range(1, 300)]
    public int TimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Gets or sets the maximum connections
    /// </summary>
    [Range(1, 1000)]
    public int MaxConnections { get; init; } = 100;

    /// <summary>
    /// Gets or sets whether to use HTTP/2
    /// </summary>
    public bool UseHttp2 { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to compress request content
    /// </summary>
    public bool CompressRequestContent { get; init; } = true;

    /// <summary>
    /// Gets or sets the user agent
    /// </summary>
    public string? UserAgent { get; init; }

    /// <summary>
    /// Gets or sets the service URI
    /// </summary>
    public Uri? ServiceUri { get; init; }

    /// <summary>
    /// Gets the default request headers as a read-only dictionary
    /// </summary>
    public IReadOnlyDictionary<string, string> DefaultHeaders =>
        new ReadOnlyDictionary<string, string>(_defaultHeaders);

    /// <summary>
    /// Creates a new instance with the provided headers
    /// </summary>
    /// <param name="headers">The headers dictionary</param>
    public PiQClientOptions WithHeaders(IDictionary<string, string> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        var result = this with { };
        foreach (var header in headers)
        {
            result.AddHeader(header.Key, header.Value);
        }
        return result;
    }

    /// <summary>
    /// Adds a header to the default headers collection
    /// </summary>
    public void AddHeader(string name, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _defaultHeaders[name] = value ?? string.Empty;
    }

    /// <summary>
    /// Removes a header from the default headers collection
    /// </summary>
    public bool RemoveHeader(string name)
    {
        return !string.IsNullOrEmpty(name) && _defaultHeaders.Remove(name);
    }

    /// <summary>
    /// Clears all default headers
    /// </summary>
    public void ClearHeaders()
    {
        _defaultHeaders.Clear();
    }
}