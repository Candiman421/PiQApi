// PiQApi.Abstractions/Validation/PiQValidationContext.cs
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Validation
{
    /// <summary>
    /// Represents a validation context that carries operation state
    /// </summary>
    public class PiQValidationContext
    {
        /// <summary>
        /// Gets the correlation identifier to link the validation chain
        /// </summary>
        public string CorrelationId { get; }

        /// <summary>
        /// Gets or sets the validation mode
        /// </summary>
        public ValidationModeType ValidationMode { get; set; } = ValidationModeType.Strict;

        /// <summary>
        /// Gets the cancellation token for async cancellation support
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets additional context metadata
        /// </summary>
        public IDictionary<string, object> AdditionalContext { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets whether to aggregate all errors regardless of validation mode
        /// </summary>
        public bool AggregateAllErrors { get; set; } = false;

        /// <summary>
        /// Creates a new validation context with the specified correlation ID
        /// </summary>
        public PiQValidationContext(string correlationId, CancellationToken cancellationToken = default)
        {
            CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Creates a new validation context with a generated correlation ID
        /// </summary>
        public PiQValidationContext(CancellationToken cancellationToken = default)
            : this(Guid.NewGuid().ToString(), cancellationToken)
        {
        }
    }
}