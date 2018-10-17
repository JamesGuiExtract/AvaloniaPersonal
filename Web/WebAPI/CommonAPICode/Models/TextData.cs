using System;
using System.Collections.Generic;

namespace WebAPI.Models
{
    /// <summary>
    /// Represents text of an API argument or return value.
    /// </summary>
    public class TextData
    {
        /// <summary>
        /// The Text
        /// </summary>
        public String Text { get; set; }

        /// <summary>
        /// Implicit conversion from string.
        /// </summary>
        public static implicit operator TextData(string text)
        {
            var args = new TextData();
            args.Text = text;
            return args;
        }
    }
}
