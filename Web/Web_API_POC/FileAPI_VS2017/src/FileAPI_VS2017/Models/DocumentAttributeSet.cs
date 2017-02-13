using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileAPI_VS2017.Models
{
    /// <summary>
    /// Document attribute set - contains a (possibly empty) list of document attributes
    /// </summary>
    public class DocumentAttributeSet
    {
        /// <summary>
        /// Error info - Error.ErrorOccurred == true if there has been an error
        /// </summary>
        public ErrorInfo Error { get; set; }

        /// <summary>
        /// list of attributes - may be empty (on error WILL be empty)
        /// </summary>
        public List<DocumentAttribute> Attributes { get; set; }
    }
}
