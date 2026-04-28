// CertApi.Abstractions/Validation/ICancellableContext.cs
using System.Threading;

namespace CertApi.Abstractions.Validation
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