using System;
using System.Collections.Generic;

namespace WebAPI.Models
{
    /// <summary>
    /// A result containing text from document pages.
    /// </summary>
    public class PageTextResult
    {
        /// <summary>
        /// Document text separated by page
        /// </summary>
        public List<PageText> Pages { get; set; }
    }

    /// <summary>
    /// Represents the text of a particular document page.
    /// </summary>
    public class PageText
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageText"/> class.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="text">The text.</param>
        public PageText(int page, string text)
        {
            Page = page;
            Text = text;
        }

        /// <summary>
        /// The page number
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// The text from the given page number
        /// </summary>
        public string Text { get; set; }
    }
}
