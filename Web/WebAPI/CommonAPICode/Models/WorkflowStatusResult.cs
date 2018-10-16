using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    /// <summary>
    /// overall status information of a workflow
    /// </summary>
    public class WorkflowStatusResult
    {
        /// <summary>
        /// number of documents processing
        /// </summary>
        public int NumberProcessing { get; set; }

        /// <summary>
        /// number of documents done processing
        /// </summary>
        public int NumberDone { get; set; }

        /// <summary>
        /// number of documents that have failed
        /// </summary>
        public int NumberFailed { get; set; }

        /// <summary>
        /// number of document submitted but that are no longer progressing through the workflow.
        /// </summary>
        public int NumberIncomplete { get; set; }
    }
}
