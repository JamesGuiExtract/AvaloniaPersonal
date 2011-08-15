using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Extract.Utilities
{
    /// <summary>
    /// Delegate used to provide the replacement value for custom tags.
    /// </summary>
    /// <param name="path">The name of the path being expanded after standard tags and path tag
    /// functions have already been applied.</param>
    /// <returns></returns>
    public delegate string ExpandTag(string path);

    /// <summary>
    /// Represents a collection of expandable path tags.
    /// </summary>
    [TypeConverter(typeof(IPathTagsConverter))]
    public interface IPathTags
    {
        /// <summary>
        /// Sets a replacement value for a standard tag. Adds the tag if it does not already exists.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <param name="value">The value the tag is to be replaced with.</param>
        void SetTagValue(string tag, string value);

        /// <summary>
        /// Adds a custom tag to be expanded with <see paramref="expandTagMethod"/>.
        /// <para><b>Note</b></para>
        /// Unlike standard tag, custom tags will not be expanded until after the path tag functions
        /// have been applied.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <param name="expandTagMethod">The <see cref="ExpandTag"/> implementation that should be
        /// used to provide the replacement value.</param>
        void AddCustomTag(string tag, ExpandTag expandTagMethod);

        /// <summary>
        /// Expands the tags in the specified path.
        /// </summary>
        /// <param name="path">The path that may contain tags.</param>
        /// <returns><paramref name="path"/> with the tags fully expanded.</returns>
        string Expand(string path);

        /// <summary>
        /// Gets the an iterator over the set of tags.
        /// </summary>
        /// <returns>The an iterator over the set of tags.</returns>
        IEnumerable<string> Tags
        {
            get;
        }
    }
}
