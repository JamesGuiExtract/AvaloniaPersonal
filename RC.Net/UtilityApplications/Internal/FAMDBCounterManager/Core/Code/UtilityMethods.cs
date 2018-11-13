using System;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Extract.FAMDBCounterManager
{
    /// <summary>
    /// Defines utility methods for use in this assembly.
    /// </summary>
    internal static class UtilityMethods
    {
        /// <summary>
        /// Converts the <see cref="T:byte[]"/> to a hexadecimal string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A hexadecimal string representation of the
        /// specified <see cref="T:byte[]"/>.</returns>
        public static string ToHexString(this byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (var item in bytes)
            {
                sb.Append(item.ToString("X2", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts a <see cref="string"/> of Hex values to an array of  <see cref="byte"/>.
        /// </summary>
        /// <param name="hexValue">A <see cref="string"/> of hex values.  Must
        /// have an even length (every two characters translate to one byte).</param>
        /// <returns>An array of <see cref="byte"/> containing the converted hex characters.</returns>
        /// <exception cref="ExtractException">If <see cref="String.Length"/> is not
        /// a multiple of 2.</exception>
        public static byte[] HexStringToBytes(this string hexValue)
        {
            var length = hexValue.Length;

            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexValue.Substring(i, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// Converts a <see langword="string"/> to a <see langword="bool"/> where "0" and "1" are
        /// recognized as well as "true" and "false".
        /// </summary>
        /// <param name="value">The <see langword="string"/> to be converted.</param>
        /// <returns>The <see langword="bool"/> equivalent.</returns>
        public static bool ToBoolean(this string value)
        {
            if (value == "1")
            {
                return true;
            }
            else if (value == "0")
            {
                return false;
            }

            return bool.Parse(value);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the <see paramref="dateTime"/>.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance or "N/A" if the datetime is
        /// zero/empty.
        /// </returns>
        public static string DateTimeToString(this DateTime dateTime)
        {
            // There is no standard way to get the time zone abbreviation. Working off the
            // assumption that Extract Systems will always operate out of the central time zone,
            // hard-code the time zone abbreviation taking only daylight vs standard into account.
            return (dateTime.Ticks == 0)
                ? "N/A"
                : dateTime.ToLocalTime().ToString("g", CultureInfo.CurrentCulture)
                    + (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now)
                        ? " CDT"
                        : " CST");
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the <see paramref="dateTime"/>
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="bias">The number of minutes ahead of UTC of the timezone to which
        /// <paramref name="dateTime"/> belongs.</param>
        /// A <see cref="System.String"/> that represents this instance or "N/A" if the datetime is
        /// zero/empty.
        public static string DateTimeToString(this DateTime dateTime, int bias)
        {
            var dateOffset = new TimeSpan(0, bias, 0);
            var localOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);

            dateTime += dateOffset;
            dateTime += localOffset;

            // There is no standard way to get the time zone abbreviation. Working off the
            // assumption that Extract Systems will always operate out of the central time zone,
            // hard-code the time zone abbreviation taking only daylight vs standard into account.
            return (dateTime.Ticks == 0)
                ? "N/A"
                : dateTime.ToString("g", CultureInfo.CurrentCulture)
                    + (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now)
                        ? " CDT"
                        : " CST");
        }

        /// <summary>
        /// Parses and validates a positive integer from the specified <see paramref="stringValue"/>.
        /// </summary>
        /// <param name="stringValue">The string value to parse</param>
        /// <param name="requireValue"><see langword="true"/> if it is required that
        /// <see paramref="stringValue"/> is not empty; <see langword="false"/> if
        /// <see paramref="stringValue"/> may be empty.</param>
        /// <returns>An <see langword="int?"/> with the parsed value or <see langword="null"/> if
        /// <paramref="stringValue"> was empty.</returns>
        public static int? ParseInteger(this string stringValue, bool requireValue)
        {
            if (!requireValue && string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            uint value = 0;
            if (!UInt32.TryParse(stringValue,
                NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowThousands,
                CultureInfo.CurrentCulture, out value))
            {
                throw new Exception("Value to apply must be a positive integer");
            }

            return (int)value;
        }

        /// <summary>
        /// Displays an error message box with the text of the specified exception.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> whose message should be displayed.</param>
        public static void ShowMessageBox(this Exception ex)
        {
            ShowMessageBox(ex.Message, "Error", true);
        }

        /// <summary>
        /// Displays a message box with the specified message and caption.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="caption">The caption for the message box.</param>
        /// <param name="error">If <see langword="true"/> displays the error icon, otherwise
        /// displays the information icon.</param>
        public static void ShowMessageBox(string message, string caption, bool error)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK,
                error ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, 0);
        }

        /// <summary>
        /// If <see paramref="condition"/> is not <see langword="true"/>, throws an exception with
        /// <see parmref="errorMessage"/> for the exception message.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        /// <param name="errorMessage">The error message if the assertion fails.</param>
        public static void Assert(bool condition, string errorMessage)
        {
            if (!condition)
            {
                throw new Exception(errorMessage);
            }
        }
    }
}
