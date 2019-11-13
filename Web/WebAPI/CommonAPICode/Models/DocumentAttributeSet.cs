using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WebAPI.Models
{
    /// <summary>
    /// The attributes on a given page of a document
    /// </summary>
    public class PageOfAttributes
    {
        /// <summary>
        /// The page to which the attributes belong
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// The attributes for this page
        /// </summary>
        public List<DocumentAttribute> Attributes { get; set; }
    }

    /// <summary>
    /// Represents pages for which attributes were edited without being posted/committed.
    /// </summary>
    public class UncommittedDocumentDataResult
    {
        /// <summary>
        /// The user that made the edits
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The time of the most recent edit
        /// </summary>
        public string ModifiedDateTime { get; set; }

        /// <summary>
        /// A list of pages of edited attributes
        /// </summary>
        public List<PageOfAttributes> UncommittedPagesOfAttributes { get; set; } = new List<PageOfAttributes>();
    }

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
