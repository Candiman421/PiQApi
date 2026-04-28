// CertApi.Ews.Operations/Folders/Interfaces/IEwsBatchOperations.cs

using CertApi.Abstractions.Enums;
using CertApi.Ews.Core.Results.Interfaces;
using CertApi.Ews.Operations.Core.Interfaces;
using CertApi.Ews.Operations.Folders.Models;
using Microsoft.Exchange.WebServices.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CertApi.Ews.Operations.Folders.Interfaces
{
    /// <summary>
    /// Interface for batch operations
    /// </summary>
    public interface IEwsBatchOperations : IEwsOperationBase
    {
        /// <summary>
        /// Imports a batch of emails into a folder
        /// </summary>
        /// <param name="emails">Emails to import</param>
        /// <param name="destinationFolderId">Destination folder ID</param>
        /// <param name="operationMode">Operation mode</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the batch import operation</returns>
        Task<IEwsResult<BatchImportResult>> ImportEmailsAsync(
            IEnumerable<EmailImportInfo> emails,
            FolderId destinationFolderId,
            BatchOperationModeType operationMode = BatchOperationModeType.Parallel,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Imports emails with original metadata preserved
        /// </summary>
        /// <param name="emails">Emails to import</param>
        /// <param name="destinationFolderId">Destination folder ID</param>
        /// <param name="preserveOriginalDates">Whether to preserve original dates</param>
        /// <param name="operationMode">Operation mode</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the batch import operation</returns>
        Task<IEwsResult<BatchImportResult>> ImportEmailsWithMetadataAsync(
            IEnumerable<EmailImportInfo> emails,
            FolderId destinationFolderId,
            bool preserveOriginalDates = true,
            BatchOperationModeType operationMode = BatchOperationModeType.Parallel,
            CancellationToken cancellationToken = default);
    }
}