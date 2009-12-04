using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Extract.Imaging
{
    /// <summary>
    /// A collection of helper methods for manipulating and managing Bates numbers.
    /// </summary>
    public static class BatesNumberHelper
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(BatesNumberHelper).ToString();

        #endregion Constants

        /// <summary>
        /// Generates the Bates number as text using the specified Bates number and page number.
        /// </summary>
        /// <param name="format">The <see cref="BatesNumberFormat"/> object to use
        /// for formatting the Bates number.</param>
        /// <param name="batesNumber">The Bates number to use.</param>
        /// <param name="pageNumber">The page number on which the Bates number appears.</param>
        /// <returns>The Bates number as text.</returns>
        public static string GetStringFromNumber(BatesNumberFormat format, long batesNumber,
            int pageNumber)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI27912",
					_OBJECT_NAME);

                // Ensure the format object is not null
                ExtractException.Assert("ELI27913", "Format object must not be null.",
                    format != null);

                // Start the string builder with the prefix
                StringBuilder builder = new StringBuilder(format.Prefix);

                // Append zero padding as necessary
                string nextNumberString = batesNumber.ToString(CultureInfo.CurrentCulture);
                if (format.ZeroPad && nextNumberString.Length < format.Digits)
                {
                    builder.Append('0', format.Digits - nextNumberString.Length);
                }

                // Append the Bates number
                builder.Append(nextNumberString);

                // Append page number if necessary
                if (format.AppendPageNumber)
                {
                    // Append page separator
                    builder.Append(format.PageNumberSeparator);

                    // Get the page number as a string
                    string pageNumberString = pageNumber.ToString(CultureInfo.CurrentCulture);

                    // Append page zero padding as necessary
                    if (format.ZeroPadPage)
                    {
                        builder.Append('0', format.PageDigits - pageNumberString.Length);
                    }

                    // Append the page number
                    builder.Append(pageNumberString);
                }

                // Add the suffix
                builder.Append(format.Suffix);

                // Return the result
                return builder.ToString();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27914", ex);
            }
        }
    }
}
