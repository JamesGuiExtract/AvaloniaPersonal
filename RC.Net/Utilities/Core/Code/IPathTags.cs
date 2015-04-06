using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Extract.Utilities
{
    /// <summary>
    /// Delegate used to provide the replacement value for added tags.
    /// </summary>
    /// <param name="path">The name of the path being expanded after standard tags and path tag
    /// functions have already been applied.</param>
    /// <returns></returns>
    public delegate string ExpandTag(string path);

    /// <summary>
    /// Represents a collection of expandable path tags.
    /// </summary>
    [TypeConverter(typeof(IPathTagsConverter))]
    [CLSCompliant(false)]
    public interface IPathTags
    {
        /// <summary>
        /// Sets a replacement value for a standard tag. Adds the tag if it does not already exists.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <param name="value">The value the tag is to be replaced with.</param>
        void AddTag(string tag, string value);

        /// <summary>
        /// Adds a tag to be expanded with <see paramref="expandTagMethod"/>.
        /// <para><b>Note</b></para>
        /// Unlike other tags, these tags will not be expanded until after the path tag functions
        /// have been applied.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <param name="expandTagMethod">The <see cref="ExpandTag"/> implementation that
        /// should be used to provide the replacement value. The tag will not be expanded until all
        /// other tags and functions have been expanded.</param>
        void AddDelayedExpansionTag(string tag, ExpandTag expandTagMethod);

        /// <summary>
        /// Expands the tags in the specified path.
        /// </summary>
        /// <param name="path">The path that may contain tags.</param>
        /// <returns><paramref name="path"/> with the tags fully expanded.</returns>
        string Expand(string path);

        /// <summary>
        /// Gets a list of the built-in path tags.
        /// </summary>
        /// <returns>A the built-in path tags.</returns>
        IEnumerable<string> BuiltInTags
        { 
            get; 
        }

        /// <summary>
        /// Gets a list of the custom (user-defined) path tags.
        /// </summary>
        /// <returns>A the custom path tags.</returns>
        IEnumerable<string> CustomTags
        { 
            get; 
        }

        /// <summary>
        /// Gets or sets a list of tags that should be filtered from <see cref="BuiltInTags"/> to
        /// prevent them from appearing in a dropdown.
        /// <para><b>Note</b></para>
        /// These tags, if present in a string to be expanded, would still be expanded.
        /// </summary>
        IEnumerable<string> BuiltInTagFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Displays a UI to edit the custom tags.
        /// </summary>
        /// <param name="parentWindow">If not <see langword="null"/>, the tag editing UI will be
        /// displayed modally this window; otherwise the editor window will be modeless.</param>
        void EditCustomTags(int parentWindow);

        /// <summary>
        /// Gets the built-in path functions.
        /// </summary>
        /// <returns>The function names.</returns>
        IEnumerable<string> FunctionNames
        {
            get;
        }

        /// <summary>
        /// Gets the function names formatted with parameters for display in a drop down list.
        /// </summary>
        /// <returns>The formatted function names.</returns>
        IEnumerable<string> FormattedFunctionNames
        {
            get;
        }
    }
}
