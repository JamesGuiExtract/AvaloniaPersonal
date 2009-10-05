using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Runtime.Serialization;

namespace Extract.Drawing
{
    /// <summary>
    /// Represents a group of methods for working with <see cref="Font"/>s.
    /// </summary>
    public static class FontMethods
    {
        #region Constants

        /// <summary>
        /// The format string for font description label.
        /// </summary>
        readonly static string _FONT_TEXT_FORMAT = "{0} {1}pt; {2}";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(FontMethods).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _OBJECT_NAME);

        #endregion Fields

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> containg the <see cref="Font"/>
        /// information in a user friendly readable format (i.e. 'Arial 30pt; Bold')
        /// </summary>
        /// <param name="font">The font to build the string from.</param>
        /// <returns>A user friendly representation of the specified <see cref="Font"/>.</returns>
        public static string GetUserFriendlyFontString(Font font)
        {
            try
            {
                // Check the license
                _licenseCache.Validate("ELI27860");

                // Return the user friendly font string
                return string.Format(CultureInfo.CurrentCulture, _FONT_TEXT_FORMAT,
                    font.Name, font.SizeInPoints, font.Style.ToString());
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27861", ex);
            }
        }

        #endregion Methods
    }
}
