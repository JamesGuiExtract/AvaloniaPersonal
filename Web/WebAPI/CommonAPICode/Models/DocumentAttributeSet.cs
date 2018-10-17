using System.Collections.Generic;

namespace WebAPI.Models
{
    /// <summary>
    /// Document attributes retrieved as an API call result.
    /// </summary>
    public class DocumentDataResult
    {
        /// <summary>
        /// list of attributes - may be empty (on error WILL be empty)
        /// </summary>
        public List<DocumentAttribute> Attributes { get; set; } = new List<DocumentAttribute>();
    }

    /// <summary>
    /// Document attributes to replace all existing data for a document.
    /// </summary>
    public class DocumentDataInput
    {
        /// <summary>
        /// A list of <see cref="DocumentAttribute"/> comprising the document data.
        /// </summary>
        public List<DocumentAttribute> Attributes { get; set; }
    }

    /// <summary>
    /// Represents changes to make to an existing attribute set (add/update/delete). There is no
    /// hierarchy to these attributes; each change is made using attribute a guid; changes are made
    /// in the order they appear in the Attributes list.
    /// </summary>
    public class DocumentDataPatch
    {
        /// <summary>
        /// The attributes to add/change/delete
        /// </summary>
        public List<DocumentAttributePatch> Attributes { get; set; }
    }
}
