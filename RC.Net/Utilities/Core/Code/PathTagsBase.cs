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
    public abstract class PathTagsBase : IPathTags, ITagUtility
    {
        #region Fields

        /// <summary>
        /// Maps path tags to their fully expanded form
        /// </summary>
        IDictionary<string, string> _tagsToValues;

        /// <summary>
        /// Maps custom tags to the <see cref="ExpandTag"/> methods to use to expand them.
        /// </summary>
        Dictionary<string, ExpandTag> _customTags = new Dictionary<string, ExpandTag>();

        /// <summary>
        /// Maps custom that should not be expanded until after all other tags and functions have
        /// been expanded to the <see cref="ExpandTag"/> methods to use to expand them.
        /// </summary>
        Dictionary<string, ExpandTag> _delayedExpansionCustomTags = new Dictionary<string, ExpandTag>();

        /// <summary>
        /// The <see cref="MiscUtils"/> instance used to evaluate path functions.
        /// </summary>
        MiscUtils _utility = new MiscUtils();

        ITagUtility _tagUtility;

        #endregion Fields

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

            _tagUtility = (ITagUtility)_utility;
            _tagsToValues = tagsToValues ?? new Dictionary<string, string>();
        }

        #endregion PathTagsBase Constructors

        #region ITagUtility

        /// <summary>
        /// Expands path tags in <see paramref="bstrInput"/> using the supplied data in
        /// <see paramref="bstrSourceDocName"/> and <see paramref="pData"/>.
        /// </summary>
        /// <param name="bstrInput">The text to expand.</param>
        /// <param name="bstrSourceDocName">The current source document name.</param>
        /// <param name="pData">Additional data needed to expand the path tags (if needed).
        /// <para><b>Note:</b></para>
        /// The type of pData is specific to implementing class.</param>
        /// <returns>The expanded text.</returns>
        public string ExpandTags(string bstrInput, string bstrSourceDocName, object pData)
        {
            try
            {
                string result = bstrInput;

                // Expand standard tags.
                foreach (KeyValuePair<string, string> pair in _tagsToValues
                    .Where(pair => pair.Value != null))
                {
                    result = result.Replace(pair.Key, pair.Value);
                }

                // Expand custom tags last so the custom tag expansion method knows what the
                // expanded path is. (excluding the expansion of custom tags)
                foreach (KeyValuePair<string, ExpandTag> customTag in _customTags
                    .Where(customTag => bstrInput.Contains(customTag.Key)))
                {
                    result = result.Replace(customTag.Key, customTag.Value(result));
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35187", ex.Message);
            }
        }

        /// <summary>
        /// Expands path tags and functions in <see paramref="bstrInput"/> using the supplied data
        /// in <see paramref="pData"/>.
        /// </summary>
        /// <param name="bstrInput">The text to expand.</param>
        /// <param name="bstrSourceDocName">The current source document name.</param>
        /// <param name="pData">The data needed to expand the path tags.
        /// <para><b>Note:</b></para>
        /// The type of pData is specific to implementing class.</param>
        /// <returns>The expanded text.</returns>
        public string ExpandTagsAndFunctions(string bstrInput, string bstrSourceDocName, object pData)
        {
            try
            {
                string output = _utility.ExpandTagsAndFunctions(bstrInput, this, bstrSourceDocName, pData);

                // Expand _delayedExpansionCustomTags tags last so the custom tag expansion method
                // knows what the rest of the expanded path is. (excluding the expansion of custom tags)
                foreach (KeyValuePair<string, ExpandTag> customTag in _delayedExpansionCustomTags
                    .Where(customTag => bstrInput.Contains(customTag.Key)))
                {
                    output = output.Replace(customTag.Key, customTag.Value(output));
                }

                return output;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35188", ex.Message);
            }
        }

        /// <summary>
        /// Gets a list of all the path tags .
        /// </summary>
        /// <returns>A <see cref="VariantVector"/> of the all the path tags.</returns>
        [CLSCompliant(false)]
        public VariantVector GetAllTags()
        {
            try
            {
                return Tags.ToVariantVector<string>();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35189", ex.Message);
            }
        }

        /// <summary>
        /// Gets a list of the built-in path tags.
        /// </summary>
        /// <returns>A <see cref="VariantVector"/> of the built-in path tags.</returns>
        [CLSCompliant(false)]
        public VariantVector GetBuiltInTags()
        {
            try
            {
                return Tags.ToVariantVector<string>();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35190", ex.Message);
            }
        }

        /// <summary>
        /// Gets a list of the built-in path functions.
        /// </summary>
        /// <returns>A <see cref="VariantVector"/> of the function names.</returns>
        [CLSCompliant(false)]
        public VariantVector GetFunctionNames()
        {
            try
            {
                return _tagUtility.GetFunctionNames();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35191", ex.Message);
            }
        }

        /// <summary>
        /// Gets the function names formatted with parameters for display in a drop down list.
        /// </summary>
        /// <returns>A <see cref="VariantVector"/> of the formatted function names.</returns>
        [CLSCompliant(false)]
        public VariantVector GetFormattedFunctionNames()
        {
            try
            {
                return _tagUtility.GetFormattedFunctionNames();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35192", "Messages");
            }
        }

        /// <summary>
        /// Gets a list of the INI path tags.
        /// </summary>
        /// <returns>A <see cref="VariantVector"/> of the INI path tags.</returns>
        [CLSCompliant(false)]
        public VariantVector GetINIFileTags()
        {
            try
            {
                // 11/15/2012 SNK
                // For better or worse this class hasn't supported INI file tags up until now and
                // I'm leaving it that way for expediency.
                return new VariantVector();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35193", ex.Message);
            }
        }

        #endregion ITagUtility

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
        /// <param name="delayedExpansion"><see langword="true"/> if the tag should not be expanded
        /// until all other tags and functions have been expanded; otherwise, <see langword="false"/>.
        /// <para><b>NOTE:</b></para>
        /// Delayed expansion tags will not be evaluated at all by the <see cref="ExpandTags"/>
        /// method.</param>
        public virtual void AddCustomTag(string tag, ExpandTag expandTagMethod, bool delayedExpansion)
        {
            try
            {
                if (delayedExpansion)
                {
                    _delayedExpansionCustomTags[tag] = expandTagMethod;
                }
                else
                {
                    _customTags[tag] = expandTagMethod;
                }
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
                // Don't need to pass any data to expand tags since all tags will be expanded by
                // this class.
                string result = _utility.ExpandTagsAndFunctions(path, this, "", null);

                // Expand _delayedExpansionCustomTags tags last so the custom tag expansion method
                // knows what the rest of the expanded path is. (excluding the expansion of custom tags)
                foreach (KeyValuePair<string, ExpandTag> customTag in _delayedExpansionCustomTags
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
