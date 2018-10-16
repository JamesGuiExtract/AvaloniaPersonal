
namespace WebAPI.Models
{
    /// <summary>
    /// Result the represent the number of documents and pages pending in a queue as well as the
    /// number of users active in that queue.
    /// </summary>
    public class QueueStatusResult
    {
        /// <summary>
        /// The number of documents currently pending in the queue.
        /// </summary>
        public int PendingDocuments;

        /// <summary>
        /// The number of pages currently pending in the queue.
        /// </summary>
        public int PendingPages;

        /// <summary>
        /// The number of users active in the queue (including the current user).
        /// </summary>
        public int ActiveUsers;
    }
}