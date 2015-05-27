using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_COMUTILSLib;

namespace Extract.Utilities
{
    /// <summary>
    /// Represents a collection of expandable path tags.
    /// </summary>
    public abstract class PathTagsBase : IPathTags
    {
        #region Fields

        /// <summary>
        /// Maps tags that should not be expanded until after all other tags and functions have
        /// been expanded to the <see cref="ExpandTag"/> methods to use to expand them.
        /// </summary>
        Dictionary<string, ExpandTag> _delayedExpansionTags = new Dictionary<string, ExpandTag>();

        /// <summary>
        /// A list of tags that should be filtered from <see cref="BuiltInTags"/>.
        /// </summary>
        HashSet<string> _builtInTagFilter;
        
        /// <summary>
        /// The <see cref="TagUtility"/> instance used to expand tags..
        /// </summary>
        ITagUtility _tagUtility;

        #endregion Fields

        #region PathTagsBase Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PathTagsBase"/> class.
        /// </summary>
        protected PathTagsBase()
        {
            // Validate the license
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                "ELI30040", this.GetType().ToString());
        }

        #endregion PathTagsBase Constructors

        #region IPathTags Members

        /// <summary>
        /// Gets a list of the built-in path tags.
        /// </summary>
        /// <returns>A <see cref="VariantVector"/> of the built-in path tags.</returns>
        public virtual IEnumerable<string> BuiltInTags
        {
            get
            {
                try
                {
                    var builtInTags = TagUtility.GetBuiltInTags().ToIEnumerable<string>()
                        .Union(_delayedExpansionTags.Keys);

                    if (_builtInTagFilter != null)
                    {
                        builtInTags = builtInTags.Where(tag => _builtInTagFilter.Contains(tag));
                    }

                    return builtInTags;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35190");
                }
            }
        }

        /// <summary>
        /// Gets a list of the custom path tags.
        /// </summary>
        /// <returns>A <see cref="VariantVector"/> of the INI path tags.</returns>
        [CLSCompliant(false)]
        public virtual IEnumerable<string> CustomTags
        {
            get
            {
                try
                {
                    return TagUtility.GetCustomFileTags().ToIEnumerable<string>();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35193");
                }
            }
        }

        /// <summary>
        /// Gets a list of the built-in path functions.
        /// </summary>
        /// <returns>A <see cref="VariantVector"/> of the function names.</returns>
        public virtual IEnumerable<string> FunctionNames
        {
            get
            {
                try
                {
                    return TagUtility.GetFunctionNames().ToIEnumerable<string>();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35191");
                }
            }
        }

        /// <summary>
        /// Gets the function names formatted with parameters for display in a drop down list.
        /// </summary>
        /// <returns>A <see cref="VariantVector"/> of the formatted function names.</returns>
        public virtual IEnumerable<string> FormattedFunctionNames
        {
            get
            {
                try
                {
                    return TagUtility.GetFormattedFunctionNames().ToIEnumerable<string>();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35192");
                }
            }
        }

        /// <summary>
        /// Displays a UI to edit the custom tags.
        /// </summary>
        /// <param name="parentWindow">If not <see langword="null"/>, the tag editing UI will be
        /// displayed modally this window; otherwise the editor window will be modeless.</param>
        public virtual void EditCustomTags(int parentWindow)
        {
            try
            {
                TagUtility.EditCustomTags(parentWindow);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38270");
            }
        }

        /// <summary>
        /// Sets a replacement value for a standard tag. Adds the tag if it does not already exists.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <param name="value">The value the tag is to be replaced with.</param>
        public void AddTag(string tag, string value)
        {
            try
            {
                if (!tag.StartsWith("<", StringComparison.OrdinalIgnoreCase))
                {
                    tag = "<" + tag;
                }
                if (!tag.EndsWith(">", StringComparison.OrdinalIgnoreCase))
                {
                    tag += ">";
                }


                TagUtility.AddTag(tag, value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33215");
            }
        }

        /// <summary>
        /// Adds a tag to be expanded with <see paramref="expandTagMethod"/>.
        /// <para><b>Note</b></para>
        /// Unlike standard tag, tags added via this method will not be expanded until after the
        /// path tag functions have been applied.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <param name="expandTagMethod">The <see cref="ExpandTag"/> implementation that should be
        /// used to provide the replacement value. The tag will not be expanded until all other tags
        /// and functions have been expanded.</param>
        public void AddDelayedExpansionTag(string tag, ExpandTag expandTagMethod)
        {
            try
            {
                if (!tag.StartsWith("<", StringComparison.OrdinalIgnoreCase))
                {
                    tag = "<" + tag;
                }
                if (!tag.EndsWith(">", StringComparison.OrdinalIgnoreCase))
                {
                    tag += ">";
                }

                _delayedExpansionTags[tag] = expandTagMethod;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33216");
            }
        }

        /// <summary>
        /// Gets or sets a list of tags that should be filtered from <see cref="BuiltInTags"/> to
        /// prevent them from appearing in a dropdown.
        /// <para><b>Note</b></para>
        /// These tags, if present in a string to be expanded, would still be expanded.
        /// </summary>
        public IEnumerable<string> BuiltInTagFilter
        {
            get
            {
                return (_builtInTagFilter as IEnumerable<string>) ?? new string[0];
            }

            set
            {
                _builtInTagFilter = (value == null || !value.Any())
                    ? null
                    : new HashSet<string>(value, StringComparer.OrdinalIgnoreCase);
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
                string result = TagUtility.ExpandTagsAndFunctions(path, StringData, ObjectData);

                // Expand _delayedExpansionTags tags last so the tag expansion method knows what the
                // rest of the expanded path is. 
                foreach (KeyValuePair<string, ExpandTag> tag in _delayedExpansionTags
                    .Where(tag => path.Contains(tag.Key)))
                {
                    result = result.Replace(tag.Key, tag.Value(result));
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

        #endregion IPathTags Members

        #region Protected Members

        /// <summary>
        /// The <see langword="string"/> parameter to be used in calls to
        /// <see cref="M:ITagUtility.ExpandTagsAndFunctions()"/> (typically SourceDocName).
        /// </summary>
        protected string StringData
        {
            get;
            set;
        }

        /// <summary>
        /// The <see langword="object"/> parameter to be used in calls to
        /// <see cref="M:ITagUtility.ExpandTagsAndFunctions()"/>.
        /// </summary>
        protected object ObjectData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the underlying <see cref="ITagUtility"/> used to expand tags and functions.
        /// </summary>
        /// <value>The underlying <see cref="ITagUtility"/>.</value>
        [CLSCompliant(false)]
        protected ITagUtility TagUtility
        {
            get
            {
                try
                {
                    if (_tagUtility == null)
                    {
                        _tagUtility = (ITagUtility)new MiscUtils();
                    }

                    return _tagUtility;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38068");
                }
            }

            set
            {
                _tagUtility = value;
            }
        }

        #endregion Protected Members
    }
}
