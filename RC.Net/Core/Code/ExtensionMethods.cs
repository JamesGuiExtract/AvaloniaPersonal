using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Extract
{
    /// <summary>
    /// Collection of helper extension methods
    /// </summary>
    public static class ExtensionMethods
    {
        #region Fields

        /// <summary>
        /// Regular expression used to validate hex strings
        /// </summary>
        static readonly Regex _hexValidation = new Regex(@"^[a-fA-F\d]+$");

        #endregion Fields

        #region String Methods

        /// <summary>
        /// Determines whether the specified <paramref name="hexValue"/> is a stringized byte array.
        /// </summary>
        /// <param name="hexValue">The hex value.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <paramref name="hexValue"/>
        /// is stringized byte array; otherwise, <see langword="false"/>.
        /// </returns>
        // Using the word Stringized since that is what this method checks. Is the passed in hexadecimal string
        // a valid stringized byte stream (i.e. string must contain an even number of characters
        // and only contain characters that are valid hexadecimal characters
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Stringized")]
        public static bool IsStringizedByteArray(this string hexValue)
        {
            try
            {
                return hexValue != null
                    && hexValue.Length % 2 == 0
                    && _hexValidation.IsMatch(hexValue);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31801");
            }
        }

        /// <summary>
        /// Converts the stringized version of a <see cref="T:byte[]"/> back to a <see cref="T:byte[]"/>.
        /// <para><b>Note:</b></para>
        /// The specified <paramref name="hexValue"/> must be a stringized byte array, otherwise the
        /// return value will be <see langword="null"/>
        /// </summary>
        /// <param name="hexValue">The hex value.</param>
        /// <returns>The converted byte array.</returns>
        public static byte[] ToByteArray(this string hexValue)
        {
            try
            {
                // Ensure string is a valid hexadecimal string.
                ExtractException.Assert("ELI31802",
                    "Value is not a valid hexadecimal byte array string.",
                    hexValue.IsStringizedByteArray());

                var length = hexValue.Length;

                // Create an array to hold the converted bytes
                byte[] bytes = new byte[length / 2];
                for (int i = 0; i < length; i += 2)
                {
                    // Convert each HEX value from the string to a byte (two characters per byte)
                    bytes[i / 2] = Convert.ToByte(hexValue.Substring(i, 2), 16);
                }

                // Return the converted bytes
                return bytes;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31803");
            }
        }

        /// <summary>
        /// Performs a string comparison using glob pattern mathing.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="globPattern">The pattern to match.</param>
        /// <param name="caseSensitive">Whether the comparison should be case sensitive or not.</param>
        /// <returns><see langword="true"/> if the value matches the pattern.</returns>
        public static bool Like(this string value, string globPattern, bool caseSensitive)
        {
            try
            {
                return LikeOperator.LikeString(value, globPattern,
                       caseSensitive ? CompareMethod.Binary : CompareMethod.Text);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32299");
            }
        }

        /// <summary>
        /// Returns a quoted version of the supplied string.
        /// <example>If the input value is 'Hello World' then the result
        /// will be '"Hello World"'.</example>
        /// </summary>
        /// <param name="value">The <see cref="String"/> to quote.</param>
        /// <returns>A quoted version of the input string.</returns>
        public static string Quote(this string value)
        {
            try
            {
                return "\"" + (value ?? string.Empty) + "\"";
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32437");
            }
        }

        /// <summary>
        /// Returns a quoted version of the supplied string if quotes are needed, else the original string.
        /// <example>For quote char as single-quote and the input value is "Hello World"
        /// then the result will be "Hello World" but if the input is "Hello World's Best Dad" then
        /// the output will be "'Hello World''s Best Dad'".</example>
        /// </summary>
        /// <param name="value">The <see cref="String"/> to quote.</param>
        /// <param name="quote">The char to use for quoting</param>
        /// <param name="delimiter">A string the presence of which requires the string be quoted (e.g., comma in CSV)</param>
        /// <returns>A quoted version of the input string.</returns>
        public static string QuoteIfNeeded(this string value, string quote, string delimiter)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    return value;
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    return quote + value + quote;
                }
                else if (value.Contains(quote) || value.Contains(delimiter))
                {
                    return quote + value.Replace(quote, quote + quote) + quote;
                }
                else
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45494");
            }
        }

        /// <summary>
        /// Returns a quoted version of the supplied string if quotes are needed, else the original string.
        /// <example>For quote char as single-quote and the input value is "Hello World"
        /// then the result will be "Hello World" but if the input is "Hello World's Best Dad" then
        /// the output will be "'Hello World''s Best Dad'".</example>
        /// </summary>
        /// <param name="value">The <see cref="String"/> to quote.</param>
        /// <param name="quote">The char to use for quoting</param>
        /// <param name="specialCharacters">Any other special chars that require quoting (e.g., comma in CSV)</param>
        /// <returns>A quoted version of the input string.</returns>
        public static string QuoteIfNeeded(this string value, string quote, params char[] specialCharacters)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    return value;
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    return quote + value + quote;
                }
                else if (value.Contains(quote) || value.IndexOfAny(specialCharacters) > 0)
                {
                    return quote + value.Replace(quote, quote + quote) + quote;
                }
                else
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45444");
            }
        }

        /// <summary>
        /// Returns a unquoted version of the supplied string.
        /// <example>If the input value is '"Hello World"' then the result
        /// will be 'Hello World'.</example>
        /// </summary>
        /// <param name="value">The <see cref="String"/> to unquote.</param>
        /// <param name="quote">The char used for quoting</param>
        /// <returns>A version of the input string with one quote removed from the beginning and end.</returns>
        /// <remarks>If there is not a quote character at the beginning and end of the string, the returned string will be the same as the input</remarks>
        public static string Unquote(this string value, string quote = "\"")
        {
            try
            {
                if (value.Length > 1
                    && value.StartsWith(quote, StringComparison.Ordinal)
                    && value.EndsWith(quote, StringComparison.Ordinal))
                {
                    return value.Substring(1, value.Length - 2)
                        .Replace(quote + quote, quote);
                }

                return value;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45008");
            }
        }

        #endregion String Methods

        #region Byte Array Methods

        /// <summary>
        /// Converts the <see cref="T:byte[]"/> to a hexadecimal string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A hexadecimal string representation of the
        /// specified <see cref="T:byte[]"/>.</returns>
        public static string ToHexString(this byte[] value)
        {
            try
            {
                ExtractException.Assert("ELI31804", "Array cannot be null!", value != null);

                // Create a string builder with a capacity of twice the length of the bytes
                // since it takes two characters to represent each byte
                StringBuilder sb = new StringBuilder(value.Length * 2);
                foreach (var item in value)
                {
                    sb.Append(item.ToString("X2", CultureInfo.InvariantCulture));
                }

                // Return the string
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31805");
            }
        }

        #endregion Byte Array Methods
    }
}
