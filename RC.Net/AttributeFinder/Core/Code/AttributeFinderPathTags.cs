using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Represents Attribute Finder document tags.
    /// </summary>
    [CLSCompliant(false)]
    [DisplayName("Attribute Finder")]
    public class AttributeFinderPathTags : PathTagsBase
    {
        #region AttributeFinderPathTags Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeFinderPathTags"/> class.
        /// </summary>
        public AttributeFinderPathTags()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeFinderPathTags"/> class.
        /// </summary>
        public AttributeFinderPathTags(AFDocument document)
            : base()
        {
            try
            {
                TagUtility = (ITagUtility)new AFUtility();
                ObjectData = document;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38090");
            }
        }

        #endregion AttributeFinderPathTags Constructors

        #region AttributeFinderPathTags Properties

        /// <summary>
        /// Gets or sets the Attribute Finder document.
        /// </summary>
        /// <value>The Attribute Finder document.</value>
        /// <returns>The Attribute Finder document.</returns>
        public AFDocument Document
        {
            get
            {
                return ObjectData as AFDocument;
            }
            set
            {
                ObjectData = value;
            }
        }

        #endregion AttributeFinderPathTags Properties
    }
}
