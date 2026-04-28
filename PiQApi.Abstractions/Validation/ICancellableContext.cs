// PiQApi.Abstractions/Validation/ICancellableContext.cs
using System.Threading;

namespace PiQApi.Abstractions.Validation
{
    /// <summary>
    /// Interface for contexts that support cancellation
    /// </summary>
    public interface ICancellableContext
    {
        /// <summary>
        /// Gets the cancellation token
        /// </summary>
        CancellationToken CancellationToken { get; }
    }
}