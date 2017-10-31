using System;
using System.Collections.Generic;
using System.Linq;

namespace WebAPI.Models
{
    /// <summary>
    /// Represents the ID of a document.
    /// </summary>
    public class DocumentId : IResultData
    {
        /// <summary>
        /// The ID of the document.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The <see cref="ErrorInfo"/>.
        /// </summary>
        public ErrorInfo Error { get; set; }
    }
}
