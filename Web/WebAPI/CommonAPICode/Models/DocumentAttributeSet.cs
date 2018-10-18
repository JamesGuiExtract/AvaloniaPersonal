using System.Collections.Generic;

namespace WebAPI.Models
{
    /// <summary>
    /// A result containing the data for a document
    /// </summary>
    public class DocumentDataResult
    {
        /// <summary>
        /// A list of attributes (fields) comprising the document data
        /// </summary>
        public List<DocumentAttribute> Attributes { get; set; } = new List<DocumentAttribute>();
    }

    /// <summary>
    /// Document data representation intended to replace all existing data for a document.
    /// </summary>
    public class DocumentDataInput
    {
        /// <summary>
        /// A list of attributes (fields) comprising the document data
        /// </summary>
        public List<DocumentAttribute> Attributes { get; set; }
    }

    /// <summary>
    /// Represents changes to make to existing document data (add/update/delete of specific attributes).
    /// There is no hierarchy to these attributes; each change is made using attribute a guid; changes
    /// are made in the order they appear in the Attributes list.
    /// </summary>
    public class DocumentDataPatch
    {
        /// <summary>
        /// The attributes to add/change/delete
        /// </summary>
        public List<DocumentAttributePatch> Attributes { get; set; }
    }
}
