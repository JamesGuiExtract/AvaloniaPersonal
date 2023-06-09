using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Extract
{
    /// <summary>
    /// A static class containing useful string manipulation routines.
    /// </summary>
    public static class StringMethods
    {
        // Since our c++ code mostly cannot handle unicode, use this encoding when we need to convert between bytes and strings
        static readonly Encoding _encoding = Encoding.GetEncoding("windows-1252");

        /// <summary>
        /// Converts a <see cref="string"/> of Hex values to an array of  <see cref="byte"/>.
        /// </summary>
        /// <param name="hexValue">A <see cref="string"/> of hex values.  Must
        /// have an even length (every two characters translate to one byte).</param>
        /// <returns>An array of <see cref="byte"/> containing the converted hex characters.</returns>
        /// <exception cref="ExtractException">If <see cref="String.Length"/> is not
        /// a multiple of 2.</exception>
        public static byte[] ConvertHexStringToBytes(string hexValue)
        {
            try
            {
                return hexValue.ToByteArray();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22624", ex);
            }
        }

        /// <summary>
        /// Converts an array of <see cref="byte"/> into a <see cref="string"/> of hex characters.
        /// </summary>
        /// <param name="value">An array of <see cref="byte"/>.  Must not be null.</param>
        /// <returns>A string containing each of the bytes as a two character hex string.</returns>
        /// <exception cref="ExtractException">If value is <see langword="null"/>.</exception>
        public static string ConvertBytesToHexString(byte[] value)
        {
            try
            {
                ExtractException.Assert("ELI22625", "Array cannot be null!", value != null);

                // Create a string builder with a capacity of twice the length of the bytes
                // since it takes two characters to represent each byte
                StringBuilder sb = new StringBuilder(value.Length * 2);
                foreach (byte bite in value)
                {
                    sb.Append(bite.ToString("X2", CultureInfo.InvariantCulture));
                }

                // Return the string
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22626", ex);
            }
        }

        /// <summary>
        /// Converts the specified string into an array of bytes.
        /// </summary>
        /// <param name="data">The string to convert.  Must not be
        /// <see langword="null"/> or empty string.</param>
        /// <returns>The specified string represented as an array of bytes.</returns>
        /// <exception cref="ExtractException">If <paramref name="data"/>
        /// is <see langword="null"/> or empty.</exception>
        public static byte[] ConvertStringToBytes(string data)
        {
            try
            {
                // Ensure the string is not null or empty
                ExtractException.Assert("ELI22648", "Data must not be null or empty!",
                    !string.IsNullOrEmpty(data));

                // Potentially lossy process here but better than failing...
                // https://extract.atlassian.net/browse/ISSUE-19240
                return _encoding.GetBytes(data);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22649", ex);
            }
        }

        /// <summary>
        /// Converts the specified array of bytes into a string.
        /// </summary>
        /// <param name="data">The array of bytes to convert.  Must not be null or empty.</param>
        /// <returns>A string built from the array of bytes.</returns>
        /// <exception cref="ExtractException">If <paramref name="data"/>
        /// is <see langword="null"/> or empty.</exception>
        public static string ConvertBytesToString(byte[] data)
        {
            try
            {
                // Ensure that data is not null or empty
                ExtractException.Assert("ELI22650", "Data may not be null or empty!",
                    data != null && data.Length > 0);

                return _encoding.GetString(data);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22651", ex);
            }
        }

        /// <summary>
        /// Converts a string that was made user-displayable by <see cref="ConvertLiteralToDisplay"/> 
        /// to its literal string form.
        /// </summary>
        /// <param name="display">A string in user-displayable form.</param>
        /// <returns>The literal string form of <paramref name="display"/>.</returns>
        public static string ConvertDisplayToLiteral(string display)
        {
            try
            {
                string result = display.Replace(@"\a", "\a"); // Bell (alert)
                result = result.Replace(@"\b", "\b");         // Backspace
                result = result.Replace(@"\f", "\f");         // Formfeed
                result = result.Replace(@"\n", "\n");         // New line
                result = result.Replace(@"\r", "\r");         // Carriage return
                result = result.Replace(@"\t", "\t");         // Tab
                result = result.Replace(@"\v", "\v");         // Vertical tab
                return result.Replace(@"\\", "\\");           // Literal backslash
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26787",
                    "Unable to convert display string to literal string.", ex);
                ee.AddDebugData("Display string", display, false);
                throw ee;
            }
        }

        /// <summary>
        /// Converts a literal string to a user-displayable form.
        /// </summary>
        /// <param name="literal">A string to convert to user-displayable form.</param>
        /// <returns><paramref name="literal"/> in user-displayable form.</returns>
        public static string ConvertLiteralToDisplay(string literal)
        {
            try
            {
                string result = literal.Replace("\\", @"\\"); // Literal backslash
                result = result.Replace("\v", @"\v");         // Vertical tab
                result = result.Replace("\t", @"\t");         // Tab
                result = result.Replace("\r", @"\r");         // Carriage return
                result = result.Replace("\n", @"\n");         // New line
                result = result.Replace("\f", @"\f");         // Formfeed
                result = result.Replace("\b", @"\b");         // Backspace
                return result.Replace("\a", @"\a");           // Bell (alert)
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26786",
                    "Unable to convert literal string to display string.", ex);
                ee.AddDebugData("Literal string", literal, false);
                throw ee;
            }
        }

        /// <summary>
        /// Converts the specified value to a string.
        /// This method will return <see cref="String.Empty"/> if <paramref name="value"/>
        /// is <see langword="null"/>.
        /// </summary>
        /// <param name="value">The value to convert to a string.</param>
        /// <returns><paramref name="value"/> converted to a string.</returns>
        public static string AsString(this object value)
        {
            return value != null ? value.ToString() : string.Empty;
        }

        /// <summary>
        /// Finds the first index of any of the specified search strings.
        /// <para><b>Note:</b></para>
        /// This uses a default <see cref="StringComparison"/> of
        /// <see cref="StringComparison.CurrentCulture"/>.
        /// </summary>
        /// <param name="valueToSearch">The string to search for the values.</param>
        /// <param name="valuesToFind">The values to search for.</param>
        /// <returns>The first index of any of the specified values.</returns>
        public static int FindIndexOfAny(string valueToSearch, IList<string> valuesToFind)
        {
            return FindIndexOfAny(valueToSearch, valuesToFind, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Finds the first index of any of the specified search strings.
        /// </summary>
        /// <param name="valueToSearch">The string to search for the values.</param>
        /// <param name="valuesToFind">The values to search for.</param>
        /// <param name="comparisonType">The <see cref="StringComparison"/> type to use.</param>
        /// <returns>The first index of any of the specified values.</returns>
        public static int FindIndexOfAny(string valueToSearch, IList<string> valuesToFind,
            StringComparison comparisonType)
        {
            try
            {
                if (string.IsNullOrEmpty(valueToSearch) || valuesToFind == null
                    || valuesToFind.Count == 0)
                {
                    return -1;
                }

                int count = valueToSearch.Length-1;
                int index = int.MaxValue;
                foreach(string value in valuesToFind)
                {
                    // Find the first index of the string
                    int temp = valueToSearch.IndexOf(value, 0, count, comparisonType);
                    if (temp != -1 && temp < index)
                    {
                        index = temp;
                        count = index;
                    }
                }
                if (index == int.MaxValue)
                {
                    index = -1;
                }

                return index;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30153", ex);
            }
        }
    }
}