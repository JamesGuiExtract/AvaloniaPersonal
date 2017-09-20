using System.Collections.Generic;

namespace WebAPI.Models
{
    /// <summary>
    /// Document attribute set - contains a (possibly empty) list of document attributes
    /// </summary>
    public class DocumentAttributeSet
    {
        /// <summary>
        /// list of attributes - may be empty (on error WILL be empty)
        /// </summary>
        public List<DocumentAttribute> Attributes { get; set; }

        /// <summary>
        /// Error info - Error == true if there has been an error
        /// </summary>
        public ErrorInfo Error { get; set; }
    }

    /// <summary>
    /// A document attribute set without <see cref="ErrorInfo"/> (for web API callers to use when
    /// updating document data).
    /// </summary>
    public class BareDocumentAttributeSet
    {
        /// <summary>
        /// A list of <see cref="DocumentAttribute"/> comprising the document data.
        /// </summary>
        public List<DocumentAttribute> Attributes { get; set; }
    }
}
