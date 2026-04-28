// CertApi.Ews.Core/Context/EwsOperationContext.cs
namespace CertApi.Ews.Core.Context
{
    public partial class EwsOperationContext
    {
        #endregion

        /// <summary>
        /// Dummy scope for when base context is null
        /// </summary>
        private class DummyScope : IDisposable
        {
            public void Dispose() { }
        }
    }

    
}
