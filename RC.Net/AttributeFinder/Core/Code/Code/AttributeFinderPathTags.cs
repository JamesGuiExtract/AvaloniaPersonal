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
        #region AttributeFinderPathTags Fields

        /// <summary>
        /// Attribute Finder utility for expanding tags.
        /// </summary>
        AFUtility _utility;

        /// <summary>
        /// The Attribute Finder document.
        /// </summary>
        AFDocument _document;

        #endregion AttributeFinderPathTags Fields

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
            : base(GetTagsToValues())
        {
            _document = document;
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
                return _document;
            }
            set
            {
                _document = value;
            }
        }

        /// <summary>
        /// Gets the Attribute Finder utility for expanding tags.
        /// </summary>
        /// <returns>The Attribute Finder utility for expanding tags.</returns>
        AFUtility Utility
        {
            get
            {
                if (_utility == null)
                {
                    _utility = new AFUtility();
                }

                return _utility;
            }
        }

        #endregion AttributeFinderPathTags Properties

        #region AttributeFinderPathTags Methods

        /// <summary>
        /// Gets the path tags mapped to their expanded form.
        /// </summary>
        static Dictionary<string, string> GetTagsToValues()
        {
            try
            {
                // Get the Attribute Finder tags
                AFUtility utility = new AFUtility();
                VariantVector tags = utility.GetAllTags();

                // Get the tag count
                int tagCount = tags.Size;
                Dictionary<string, string> tagsToValues = new Dictionary<string, string>(tagCount);
                for (int i = 0; i < tagCount; i++)
                {
                    tagsToValues.Add((string)tags[i], "");
                }

                return tagsToValues;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26528",
                    "Unable to get Attribute Finder path tags.", ex);
            }
        }

        #endregion AttributeFinderPathTags Methods

        #region AttributeFinderPathTags Overrides

        /// <summary>
        /// Expands the tags in the specified path.
        /// </summary>
        /// <param name="path">The path that may contain tags.</param>
        /// <returns><paramref name="path"/> with the tags fully expanded.</returns>
        public override string Expand(string path)
        {
            try
            {
                if (_document == null)
                {
                    return path;
                }

                return Utility.ExpandTagsAndFunctions(path, _document);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26529",
                    "Unable to expand path tags.", ex);
                ee.AddDebugData("Path", path, false);
                throw ee;
            }
        }

        #endregion AttributeFinderPathTags Overrides
    }
}
