using System;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Extract.Licensing.Internal
{
    /// <summary>
    /// Defines utility methods for use in this assembly.
    /// </summary>
    public static class UtilityMethods
    {
        /// <summary>
        /// This is the domain that the computer must beling to
        /// </summary>
        static readonly string _DOMAIN = "extract.local";

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

        public static DateTime Round(this DateTime dateTime,
                                     TimeSpan timeSpan,
                                     DateTime boundaryDateTime = new DateTime())
        {
            // Calculate an offset in ticks such that 0 will lie on a times pan boundary where the
            // time spans are aligned with boundaryDateTime.
            long offsetTicks = boundaryDateTime.Ticks % timeSpan.Ticks;
            Assert(dateTime.Ticks >= offsetTicks, "Invalid datetime rounding boundary.");

            // Normalize dateTime.Ticks against the boundaryDateTime.
            long dateTimeTicks = dateTime.Ticks - offsetTicks;

            // Round to the nearest boundary.
            long ticks = ((dateTimeTicks + (timeSpan.Ticks / 2)) / timeSpan.Ticks) * timeSpan.Ticks;

            // Normalize the result back to the standard DateTime timescale.
            ticks += offsetTicks;

            return new DateTime(ticks);
        }

        public static ByteArrayManipulator GetLicenseBytesFromCode(string hexString)
        {
            var byteArray = GetCodeWithKey(hexString,
                                           NativeMethods.Key5,
                                           NativeMethods.Key6,
                                           NativeMethods.Key7,
                                           NativeMethods.Key8);
            var code = byteArray.ReadString();

            return new ByteArrayManipulator(TranslateBytesWithUserKey(true, code.HexStringToBytes()));
        }

        public static string TranslateBytesToLicenseStringWithKey(Byte[] bytes, UInt32 key1, UInt32 key2, UInt32 key3, UInt32 Key4)
        {
            var code = TranslateBytesWithUserKey(false, bytes);
            ByteArrayManipulator byteArray = new ByteArrayManipulator();
            byteArray.Write(code.ToHexString());
            return NativeMethods.EncryptDecryptBytes(byteArray.GetBytes(8), true, key1, key2, key3, Key4).ToHexString();
        }

        public static string TranslateToUnlockCode(Byte[] unlockCodeAsBytes)
        {
            var encrypted = NativeMethods.EncryptDecryptBytes(unlockCodeAsBytes, GetUnlockPassword(), true);
            
            return SwapChars(encrypted.ToHexString());
        }


        public static Byte[] TranslateFromUnlockCode(string code)
        {
            var bytes = SwapChars(code).HexStringToBytes();

            return NativeMethods.EncryptDecryptBytes(bytes, GetUnlockPassword(), false);
        }

        public static bool IsOnExtractDomain()
        {
            try
            {
                return Domain.GetComputerDomain().Name == _DOMAIN;
            }
            catch
            {
                return false;
            }
        }

        private static string SwapChars(string hexString)
        {
            StringBuilder sb = new StringBuilder(hexString.Length);
            char[] charArray = hexString.ToCharArray();
            for (int i = 0; i < hexString.Length - 1; i += 2)
            {
                sb.Append(charArray[i + 1]);
                sb.Append(charArray[i]);
            }
            return sb.ToString();
        }

        public static Byte[] TranslateBytesWithUserKey(bool extract, Byte[] bytes)
        {
            var userPassword = GetUserLicensePassword();
            return NativeMethods.EncryptDecryptBytes(bytes,
                                              !extract,
                                              BitConverter.ToUInt32(userPassword, 0),
                                              BitConverter.ToUInt32(userPassword, 4),
                                              BitConverter.ToUInt32(userPassword, 8),
                                              BitConverter.ToUInt32(userPassword, 12));

        }

        private static ByteArrayManipulator GetCodeWithKey(string hexString, UInt32 key1, UInt32 key2, UInt32 key3, UInt32 key4)
        {
            var byteCode = NativeMethods.EncryptDecryptBytes(hexString.HexStringToBytes(),
                                                             false,
                                                             key1,
                                                             key2,
                                                             key3,
                                                             key4);

            return new ByteArrayManipulator(byteCode);
        }

        private static Byte[] GetUserLicensePassword()
        {
            Byte[] passwordBytes = new byte[16];

            int i;
            for (i = 0; i < 8; i++)
            {
                passwordBytes[i] = (byte)(i * 4 + 23 - i * 2);
            }
            for (int j = 0; j < 8; j++)
            {
                passwordBytes[i + j] = (byte)((2 * i + j) * 7 + 31 - (i + 2 * j) * 3);
            }

            return passwordBytes;
        }

        private static Byte[] GetUnlockPassword()
        {
            byte[] passwordBytes = new byte[16];
            var bytes = BitConverter.GetBytes(NativeMethods.DateTimeKey9);
            passwordBytes[0] = bytes[0];
            passwordBytes[1] = bytes[1];
            passwordBytes[2] = bytes[2];
            passwordBytes[3] = bytes[3];

            bytes = BitConverter.GetBytes(NativeMethods.DateTimeKeyA);
            passwordBytes[4] = bytes[0];
            passwordBytes[5] = bytes[1];
            passwordBytes[6] = bytes[2];
            passwordBytes[7] = bytes[3];

            bytes = BitConverter.GetBytes(NativeMethods.DateTimeKeyB);
            passwordBytes[8] = bytes[0];
            passwordBytes[9] = bytes[1];
            passwordBytes[10] = bytes[2];
            passwordBytes[11] = bytes[3];

            bytes = BitConverter.GetBytes(NativeMethods.DateTimeKeyC);
            passwordBytes[12] = bytes[0];
            passwordBytes[13] = bytes[1];
            passwordBytes[14] = bytes[2];
            passwordBytes[15] = bytes[3];

            return passwordBytes;
        }

    }
}
