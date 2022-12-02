using System.Collections.Generic;

namespace WebAPI
{
    /// <summary>
    /// A result representing the status of all files in a workflow.
    /// </summary>
    public class FileStatusResult
    {
        /// <summary>
        /// The list of all file statuses
        /// </summary>
        public List<FileStatus> FileStatuses { get; set; }
    }

    /// <summary>
    /// The status of a particular file in a workflow
    /// </summary>
    public class FileStatus
    {
        /// <summary>
        /// The document ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The status of the file
        /// </summary>
        public string Status { get; set; }
    }
}
