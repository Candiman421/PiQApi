// CertApi.Ews.Operations/Folders/Models/BatchImportResult.cs

using System.Collections.Generic;

namespace CertApi.Ews.Operations.Folders.Models
{
    /// <summary>
    /// Represents the result of a batch import operation
    /// </summary>
    public class BatchImportResult
    {
        /// <summary>
        /// Gets the number of successfully imported items
        /// </summary>
        public int SuccessCount { get; }

        /// <summary>
        /// Gets the number of failed items
        /// </summary>
        public int FailureCount { get; }

        /// <summary>
        /// Gets the errors that occurred during import
        /// </summary>
        public IReadOnlyList<EmailImportError> Errors { get; }

        /// <summary>
        /// Gets the total number of processed items
        /// </summary>
        public int TotalCount => SuccessCount + FailureCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchImportResult"/> class
        /// </summary>
        /// <param name="successCount">Number of successfully imported items</param>
        /// <param name="failureCount">Number of failed items</param>
        /// <param name="errors">Errors that occurred during import</param>
        public BatchImportResult(int successCount = 0, int failureCount = 0, IEnumerable<EmailImportError>? errors = null)
        {
            SuccessCount = successCount;
            FailureCount = failureCount;
            Errors = errors != null ? new List<EmailImportError>(errors).AsReadOnly() : new List<EmailImportError>().AsReadOnly();
        }
    }
}