using Extract.Licensing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        /// <summary>
        /// Maps custom tags to the <see cref="ExpandTag"/> methods to use to expand them.
        /// </summary>
        Dictionary<string, ExpandTag> _customTags = new Dictionary<string, ExpandTag>();

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
        /// Sets a replacement value for a standard tag. Adds the tag if it does not already exists.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <param name="value">The value the tag is to be replaced with.</param>
        public virtual void SetTagValue(string tag, string value)
        {
            try
            {
                _tagsToValues[tag] = value;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33215");
            }
        }

        /// <summary>
        /// Adds a custom tag to be expanded with <see paramref="expandTagMethod"/>.
        /// <para><b>Note</b></para>
        /// Unlike standard tag, custom tags will not be expanded until after the path tag functions
        /// have been applied.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <param name="expandTagMethod">The <see cref="ExpandTag"/> implementation that should be
        /// used to provide the replacement value.</param>
        public virtual void AddCustomTag(string tag, ExpandTag expandTagMethod)
        {
            try
            {
                _customTags[tag] = expandTagMethod;
                _tagsToValues[tag] = null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33216");
            }
        }

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

                // Expand standard tags.
                foreach (KeyValuePair<string, string> pair in _tagsToValues
                    .Where(pair => pair.Value != null))
                {
                    result = result.Replace(pair.Key, pair.Value);
                }

                // Apply path tag functions.
                MiscUtils utility = new MiscUtils();
                result = utility.GetExpandedTags(result, "<SourceDocName>");

                // Expand custom tags last so the custom tag expansion method knows what the
                // expanded path is. (excluding the expansion of custom tags)
                foreach (KeyValuePair<string, ExpandTag> customTag in _customTags
                    .Where(customTag => path.Contains(customTag.Key)))
                {
                    result = result.Replace(customTag.Key, customTag.Value(result));
                }

                return result;
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
