using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Extract.Utilities
{
    /// <summary>
    /// Represents a collection of expandable path tags.
    /// </summary>
    [TypeConverter(typeof(IPathTagsConverter))]
    public interface IPathTags
    {
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
