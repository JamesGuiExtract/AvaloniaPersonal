using System.ServiceModel;

namespace Extract.SharePoint.Redaction.Utilities
{
    /// <summary>
    /// WCF Service contract interface for the ID Shield for SP Client application.
    /// </summary>
    [ServiceContract]
    public interface IIDShieldForSPClient
    {
        /// <summary>
        /// Interface method used to launch the specified file for local processing.
        /// </summary>
        /// <param name="data">The data for the file to process.</param>
        [OperationContract]
        void ProcessFile(IDSForSPClientData data);

        /// <summary>
        /// Interface method used to launch the specified file for local verification
        /// </summary>
        /// <param name="data">The data for the file to process.</param>
        [OperationContract]
        void VerifyFile(IDSForSPClientData data);
    }
}
