using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
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
        /// 
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }
    }
}
