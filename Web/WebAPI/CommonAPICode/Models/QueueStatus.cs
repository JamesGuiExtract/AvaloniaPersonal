using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    /// <summary>
    /// Data model for queue status
    /// </summary>
    public class QueueStatus : IResultData
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

        /// <summary>
        /// Error info - Error == true if there has been an error
        /// </summary>
        public ErrorInfo Error { get; set; }
    }
}