using Extract.Licensing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using UCLID_COMUTILSLib;

namespace Extract.Utilities
{
    /// <summary>
    /// Represents a collection of expandable path tags.
    /// </summary>
    public abstract class PathTagsBase : IPathTags
    {
        #region PathTagsBase Fields

        /// <summary>
        /// Maps path tags to their fully expanded form
        /// </summary>
        IDictionary<string, string> _tagsToValues;

        #endregion PathTagsBase Fields

        #region PathTagsBase Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PathTagsBase"/> class.
        /// </summary>
        /// <param name="tagsToValues">A dictionary that maps path tags to their expanded value.
        /// </param>
        protected PathTagsBase(IDictionary<string, string> tagsToValues)
        {
            // Validate the license
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                "ELI30040", this.GetType().ToString());

            _tagsToValues = tagsToValues ?? new Dictionary<string, string>();
        }

        #endregion PathTagsBase Constructors

        #region IPathTags Members

        /// <summary>
        /// Expands the tags in the specified path.
        /// </summary>
        /// <param name="path">The path that may contain tags.</param>
        /// <returns><paramref name="path"/> with the tags fully expanded.</returns>
        public virtual string Expand(string path)
        {
            try
            {
                string result = path;
                foreach (KeyValuePair<string, string> pair in _tagsToValues)
                {
                    result = result.Replace(pair.Key, pair.Value);
                }

                MiscUtils utility = new MiscUtils();
                return utility.GetExpandedTags(result, "<SourceDocName>");
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26524", 
                    "Unable to expand tags.", ex);
                ee.AddDebugData("Path", path, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the an iterator over the set of tags.
        /// </summary>
        /// <returns>The an iterator over the set of tags.</returns>
        public IEnumerable<string> Tags
        {
            get
            {
                return _tagsToValues.Keys;   
            }
        }

        #endregion IPathTags Members
    }
}
