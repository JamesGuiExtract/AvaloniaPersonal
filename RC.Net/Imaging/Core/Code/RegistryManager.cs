using Extract;
using Extract.Drawing;
using Extract.Imaging;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace Extract.Imaging
{
    /// <summary>
    /// Manages Extract imaging specific registry settings
    /// </summary>
    internal static class RegistryManager
    {
        #region RegistryManager Constants

        #region RegistryManager SubKeys

        /// <summary>
        /// The sub key for Extract imaging keys.
        /// </summary>
        static readonly string _EXTRACT_IMAGING_SUB_KEY =
            @"Software\Extract Systems\Imaging";

        #endregion RegistryManager SubKeys

        #region RegistryManager Keys

        /// <summary>
        /// The key for logging image-related file locking.
        /// </summary>
        static readonly string _LOG_LOCKING_KEY = "Log image locking";

        /// <summary>
        /// The key for image-related file locking.
        /// </summary>
        static readonly string _LOCK_IMAGE_FILES_KEY = "Lock image files";

        /// <summary>
        /// The key for the alignment of the anchor point relative to a Bates number.
        /// </summary>
        static readonly string _BATES_ANCHOR_ALIGNMENT_KEY = "Bates anchor alignment";

        /// <summary>
        /// The key for the alignment of a Bates number relative to the page.
        /// </summary>
        static readonly string _BATES_PAGE_ANCHOR_ALIGNMENT_KEY = "Bates page anchor alignment";

        /// <summary>
        /// The key for the Bates number font family.
        /// </summary>
        static readonly string _BATES_FONT_FAMILY_KEY = "Bates font family";

        /// <summary>
        /// The key for the Bates number point size.
        /// </summary>
        static readonly string _BATES_FONT_SIZE_KEY = "Bates font size";

        /// <summary>
        /// The key for the Bates number font style such as Bold or Italics.
        /// </summary>
        static readonly string _BATES_FONT_STYLE_KEY = "Bates font style";

        /// <summary>
        /// The key for the horizontal offset of Bates numbers in inches.
        /// </summary>
        static readonly string _BATES_HORIZONTAL_INCHES_KEY = "Bates horizontal inches";

        /// <summary>
        /// The key for the vertical offset of Bates numbers in inches.
        /// </summary>
        static readonly string _BATES_VERTICAL_INCHES_KEY = "Bates vertical inches";

        /// <summary>
        /// The key for whether to use a next number file to get the next Bates number.
        /// </summary>
        static readonly string _USE_NEXT_NUMBER_FILE_KEY = "Use next number file";

        /// <summary>
        /// The key for path to the next number file.
        /// </summary>
        static readonly string _NEXT_NUMBER_FILE_KEY = "Next number file";

        /// <summary>
        /// The key for whether to zero pad Bates numbers.
        /// </summary>
        static readonly string _BATES_ZERO_PAD_KEY = "Bates zero pad";
        
        /// <summary>
        /// The key for the number of digits in a zero padded Bates number.
        /// </summary>
        static readonly string _BATES_DIGITS_KEY = "Bates digits";

        /// <summary>
        /// The key for whether to append the page number to Bates numbers
        /// </summary>
        static readonly string _BATES_APPEND_PAGE_NUMBER_KEY = "Bates append page number";

        /// <summary>
        /// The key for whether to zero pad the page number for Bates numbers.
        /// </summary>
        static readonly string _BATES_ZERO_PAD_PAGE_KEY = "Bates zero pad page";

        /// <summary>
        /// The key for the number of digits to in a zero padded Bates number page number.
        /// </summary>
        static readonly string _BATES_PAGE_DIGITS_KEY = "Bates page digits";

        /// <summary>
        /// The key for the page number separator to use for Bates numbers.
        /// </summary>
        static readonly string _BATES_PAGE_NUMBER_SEPARATOR_KEY = "Bates page number separator";

        /// <summary>
        /// The key for the Bates number's prefix.
        /// </summary>
        static readonly string _BATES_PREFIX_KEY = "Bates prefix";

        /// <summary>
        /// The key for the Bates number's suffix.
        /// </summary>
        static readonly string _BATES_SUFFIX_KEY = "Bates suffix";

        /// <summary>
        /// The key for the use database counter for Bates number
        /// </summary>
        static readonly string _BATES_USE_DATABASE_COUNTER = "Use database counter";

        /// <summary>
        /// The key for the database counter to use for the Bates number
        /// </summary>
        static readonly string _BATES_DATABASE_COUNTER_NAME = "Database counter name";

        #endregion RegistryManager Keys

        #region RegistryManager Values

        /// <summary>
        /// The default anchor alignment setting for Bates numbers.
        /// </summary>
        internal static readonly AnchorAlignment _BATES_ANCHOR_ALIGNMENT_DEFAULT = 
            AnchorAlignment.RightTop;

        /// <summary>
        /// The default page anchor alignment setting for Bates numbers.
        /// </summary>
        internal static readonly AnchorAlignment _BATES_PAGE_ANCHOR_ALIGNMENT_DEFAULT = 
            AnchorAlignment.RightTop;

        /// <summary>
        /// The default font family for Bates numbers.
        /// </summary>
        internal static readonly string _BATES_FONT_FAMILY_DEFAULT = "Arial";

        /// <summary>
        /// The default font size for Bates numbers.
        /// </summary>
        internal static readonly float _BATES_FONT_SIZE_DEFAULT = 30F;

        /// <summary>
        /// The default font style for Bates numbers.
        /// </summary>
        internal static readonly FontStyle _BATES_FONT_STYLE_DEFAULT = FontStyle.Bold;

        /// <summary>
        /// The default horizontal offset for Bates numbers in inches.
        /// </summary>
        internal static readonly float _BATES_HORIZONTAL_INCHES_DEFAULT = 0.5F;

        /// <summary>
        /// The default vertical offset for Bates numbers in inches.
        /// </summary>
        internal static readonly float _BATES_VERTICAL_INCHES_DEFAULT = 0.5F;
            
        /// <summary>
        /// The default next Bates number.
        /// </summary>
        internal static readonly long _NEXT_BATES_NUMBER_DEFAULT = 1L;

        // Disabling warning - error CS0649: Warning as Error: Field
        // 'Extract.Imaging.RegistryManager._USE_NEXT_NUMBER_FILE_DEFAULT' is never assigned to,
        // and will always have its default value false
        // The current desired behavior is for the default value of this setting to be false
        // but the compiler warns that the object will always have the default of false.
        // If the code is modified to assign false then an FxCop warning will be thrown
        // that the default is false and so it should not be initialized
#pragma warning disable 649
        /// <summary>
        /// The default setting for whether to use a next Bates number file.
        /// </summary>
        internal static readonly bool _USE_NEXT_NUMBER_FILE_DEFAULT;
#pragma warning restore 649

        /// <summary>
        /// The default next number file.
        /// </summary>
        internal static readonly string _NEXT_NUMBER_FILE_DEFAULT = "";

        /// <summary>
        /// The default for whether to zero pad Bates numbers.
        /// </summary>
        internal static readonly bool _BATES_ZERO_PAD_DEFAULT = true;

        /// <summary>
        /// The default number of digits in a zero padded Bates number.
        /// </summary>
        internal static readonly int _BATES_DIGITS_DEFAULT = 6;

        /// <summary>
        /// The default setting for whether page numbers should be appended to Bates numbers.
        /// </summary>
        internal static readonly bool _BATES_APPEND_PAGE_NUMBER_DEFAULT = true;

        /// <summary>
        /// The default setting for whether a Bates number's page number should be zero padded.
        /// </summary>
        internal static readonly bool _BATES_ZERO_PAD_PAGE_DEFAULT = true;

        /// <summary>
        /// The default number of digits in a zero padded page number of a Bates number.
        /// </summary>
        internal static readonly int _BATES_PAGE_DIGITS_DEFAULT = 3;

        /// <summary>
        /// The default separator between a Bates number and its page number.
        /// </summary>
        internal static readonly string _BATES_PAGE_NUMBER_SEPARATOR_DEFAULT = " / ";

        /// <summary>
        /// The default Bates number prefix.
        /// </summary>
        internal static readonly string _BATES_PREFIX_DEFAULT = "";

        /// <summary>
        /// The default Bates number suffix.
        /// </summary>
        internal static readonly string _BATES_SUFFIX_DEFAULT = "";

        #endregion RegistryManager Values

        #endregion RegistryManager Constants

        #region RegistryManager Fields

        /// <summary>
        /// The current user registry sub key for Extract imaging keys.
        /// </summary>     
        static RegistryKey _userExtractImaging =
            Registry.CurrentUser.CreateSubKey(_EXTRACT_IMAGING_SUB_KEY);

        /// <summary>
        /// A mutex that provides exclusive access to the next number registry value.
        /// </summary>
        static Mutex _nextNumberMutex =
            ThreadingMethods.GetGlobalNamedMutex(@"Global\{F3FF0B9C-4E0D-427e-B7DD-E257F3013E70}");

        /// <summary>
        /// The path to the next bates number file stored in the
        /// 'all users/application data' folder.
        /// </summary>
        static string _nextBatesNumberAppDataFile = InitializeNextBatesNumberAppDataFileName();

        #endregion RegistryManager Fields

        #region RegistryManager Properties

        /// <summary>
        /// Gets whether to log image-related file locking.
        /// </summary>
        /// <value>Whether to log image-related file locking.</value>
        public static bool LogFileLocking
        {
            get
            {
                int? registryValue = _userExtractImaging.GetValue(_LOG_LOCKING_KEY) as int?;
                if (registryValue == null)
                {
                    _userExtractImaging.SetValue(_LOG_LOCKING_KEY, 0, RegistryValueKind.DWord);
                }

                return registryValue == 1;
            }
        }

        /// <summary>
        /// Gets whether to lock image files when reading.
        /// </summary>
        /// <value>Whether to lock image file when reading.</value>
        public static bool LockFiles
        {
            get
            {
                int? registryValue = _userExtractImaging.GetValue(_LOCK_IMAGE_FILES_KEY) as int?;
                if (registryValue == null)
                {
                    _userExtractImaging.SetValue(_LOCK_IMAGE_FILES_KEY, 1, RegistryValueKind.DWord);
                    registryValue = 1;
                }

                return registryValue == 1;
            }
        }

        /// <summary>
        /// Gets or sets the next Bates number.
        /// </summary>
        /// <value>The next Bates number.</value>
        /// <returns>The next Bates number.</returns>
        public static long NextBatesNumber
        {
            get
            {
                // Protect critical section
                _nextNumberMutex.WaitOne();

                try
                {
                    return GetNextBatesNumberFromApplicationData();
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28002", ex);
                }
                finally
                {
                    _nextNumberMutex.ReleaseMutex();
                }
            }
            set
            {
                // Protect critical section
                _nextNumberMutex.WaitOne();

                try
                {
                    SetNextBatesNumberInApplicationData(value);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28003", ex);
                }
                finally
                {
                    _nextNumberMutex.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the anchor point relative to a Bates number.
        /// </summary>
        /// <value>The alignment of the anchor point relative to a Bates number.</value>
        /// <returns>The alignment of the anchor point relative to a Bates number.</returns>
        public static AnchorAlignment BatesAnchorAlignment
        {
            get
            {
                return (AnchorAlignment)_userExtractImaging.GetValue(
                    _BATES_ANCHOR_ALIGNMENT_KEY, _BATES_ANCHOR_ALIGNMENT_DEFAULT);
            }
            set
            {
                _userExtractImaging.SetValue(_BATES_ANCHOR_ALIGNMENT_KEY, value, 
                    RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Gets or sets the alignment of a Bates number relative to the page.
        /// </summary>
        /// <value>The alignment of a Bates number relative to the page.</value>
        /// <returns>The alignment of a Bates number relative to the page.</returns>
        public static AnchorAlignment BatesPageAnchorAlignment
        {
            get
            {
                return (AnchorAlignment)_userExtractImaging.GetValue(
                    _BATES_PAGE_ANCHOR_ALIGNMENT_KEY, _BATES_PAGE_ANCHOR_ALIGNMENT_DEFAULT);
            }
            set
            {
                _userExtractImaging.SetValue(_BATES_PAGE_ANCHOR_ALIGNMENT_KEY, value, 
                    RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Gets or sets the font to use for Bates numbers.
        /// </summary>
        /// <value>The font to use for Bates numbers.</value>
        /// <returns>The font to use for Bates numbers.</returns>
        public static Font BatesFont
        {
            get
            {
                // Get the font family
                string family = (string)_userExtractImaging.GetValue(_BATES_FONT_FAMILY_KEY, 
                    _BATES_FONT_FAMILY_DEFAULT);

                // Get the font size in points
                float size = GetFloat(_userExtractImaging, _BATES_FONT_SIZE_KEY, 
                    _BATES_FONT_SIZE_DEFAULT);

                // Get the font style
                FontStyle style = (FontStyle)_userExtractImaging.GetValue(
                    _BATES_FONT_STYLE_KEY, _BATES_FONT_STYLE_DEFAULT);

                // Create and return the resultant font
                return new Font(family, size, style, GraphicsUnit.Point);
            }
            set
            {
                // Store the font family
                _userExtractImaging.SetValue(_BATES_FONT_FAMILY_KEY, value.FontFamily.Name, 
                    RegistryValueKind.String);

                // Store the size in points
                _userExtractImaging.SetValue(_BATES_FONT_SIZE_KEY, value.SizeInPoints, 
                    RegistryValueKind.String);

                // Store the font style
                _userExtractImaging.SetValue(_BATES_FONT_STYLE_KEY, value.Style, 
                    RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Gets or sets the horizontal inches offset of a Bates number.
        /// </summary>
        /// <value>The horizontal inches offset of a Bates number.</value>
        /// <returns>The horizontal inches offset of a Bates number.</returns>
        public static float BatesHorizontalInches
        {
            get
            {
                return GetFloat(_userExtractImaging, _BATES_HORIZONTAL_INCHES_KEY, 
                    _BATES_HORIZONTAL_INCHES_DEFAULT);
            }
            set
            {
                _userExtractImaging.SetValue(_BATES_HORIZONTAL_INCHES_KEY, value, 
                    RegistryValueKind.String);
            }
        }

        /// <summary>
        /// Gets or sets the vertical inches offset of a Bates number.
        /// </summary>
        /// <value>The vertical inches offset of a Bates number.</value>
        /// <returns>The vertical inches offset of a Bates number.</returns>
        public static float BatesVerticalInches
        {
            get
            {
                return GetFloat(_userExtractImaging, _BATES_VERTICAL_INCHES_KEY, 
                    _BATES_VERTICAL_INCHES_DEFAULT);
            }
            set
            {
                _userExtractImaging.SetValue(_BATES_VERTICAL_INCHES_KEY, value,
                    RegistryValueKind.String);
            }
        }

        /// <summary>
        /// Gets or sets whether to use the next number file for Bates numbers.
        /// </summary>
        /// <value><see langword="true"/> if a Bates next number file should be used;
        /// <see langword="false"/> if the registry value should be used.</value>
        /// <returns><see langword="true"/> if a Bates next number file should be used;
        /// <see langword="false"/> if the registry value should be used.</returns>
        public static bool UseNextNumberFile
        {
            get 
            {
                int registryValue = (int) 
                    _userExtractImaging.GetValue(_USE_NEXT_NUMBER_FILE_KEY, 0);
                return registryValue == 1;
            }
            set 
            { 
                _userExtractImaging.SetValue(_USE_NEXT_NUMBER_FILE_KEY, value, 
                    RegistryValueKind.DWord);
            }
        }
        
        /// <summary>
        /// Gets or sets the path to next number file.
        /// </summary>
        /// <value>The path to next number file.</value>
        /// <returns>The path to next number file.</returns>
        public static string NextNumberFile
        {
            get 
            { 
                return (string) _userExtractImaging.GetValue(_NEXT_NUMBER_FILE_KEY, 
                    _NEXT_NUMBER_FILE_DEFAULT);
            }
            set 
            { 
                _userExtractImaging.SetValue(_NEXT_NUMBER_FILE_KEY, value, 
                    RegistryValueKind.String);
            }
        }
        
        /// <summary>
        /// Gets or sets whether to zero pad Bates numbers.
        /// </summary>
        /// <value><see langword="true"/> if Bates numbers should be zero padded;
        /// <see langword="false"/> if Bates numbers should not be zero padded.</value>
        /// <returns><see langword="true"/> if Bates numbers should be zero padded;
        /// <see langword="false"/> if Bates numbers should not be zero padded.</returns>
        public static bool BatesZeroPad
        {
            get 
            {
                int registryValue = (int) _userExtractImaging.GetValue(_BATES_ZERO_PAD_KEY, 1);
                return registryValue == 1;
            }
            set 
            { 
                _userExtractImaging.SetValue(_BATES_ZERO_PAD_KEY, value, RegistryValueKind.DWord);
            }
        }
        
        /// <summary>
        /// Gets or sets the number of digits in a zero padded Bates number.
        /// </summary>
        /// <value>The number of digits in a zero padded Bates number.</value>
        /// <returns>The number of digits in a zero padded Bates number.</returns>
        public static int BatesDigits
        {
            get 
            { 
                return (int) _userExtractImaging.GetValue(_BATES_DIGITS_KEY, _BATES_DIGITS_DEFAULT);
            }
            set 
            { 
                _userExtractImaging.SetValue(_BATES_DIGITS_KEY, value, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Gets or sets whether to append the page number on a Bates number.
        /// </summary>
        /// <value><see langword="true"/> if the page number should be appended;
        /// <see langword="false"/> if the page number should not be appended.</value>
        /// <returns><see langword="true"/> if the page number should be appended;
        /// <see langword="false"/> if the page number should not be appended.</returns>
        public static bool BatesAppendPageNumber
        {
            get 
            {
                int registryValue = (int) _userExtractImaging.GetValue(
                    _BATES_APPEND_PAGE_NUMBER_KEY, 1);
                return registryValue == 1;
            }
            set 
            { 
                _userExtractImaging.SetValue(_BATES_APPEND_PAGE_NUMBER_KEY, value, 
                    RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Gets or sets whether to zero pad the page number of a Bates number.
        /// </summary>
        /// <value><see langword="true"/> if the page number should be zero padded;
        /// <see langword="false"/> if the page number should not be zero padded.</value>
        /// <returns><see langword="true"/> if the page number should be zero padded;
        /// <see langword="false"/> if the page number should not be zero padded.</returns>
        public static bool BatesZeroPadPage
        {
            get 
            {
                int registryValue = (int) _userExtractImaging.GetValue(
                    _BATES_ZERO_PAD_PAGE_KEY, 1);
                return registryValue == 1;
            }
            set 
            { 
                _userExtractImaging.SetValue(_BATES_ZERO_PAD_PAGE_KEY, value, 
                    RegistryValueKind.DWord);
            }
        }
        
        /// <summary>
        /// Gets or sets the number digits in a zero padded page number of a Bates number.
        /// </summary>
        /// <value>The number digits in a zero padded page number of a Bates number.</value>
        /// <returns>The number digits in a zero padded page number of a Bates number.</returns>
        public static int BatesPageDigits
        {
            get 
            { 
                return (int) _userExtractImaging.GetValue(_BATES_PAGE_DIGITS_KEY, 
                    _BATES_PAGE_DIGITS_DEFAULT);
            }
            set 
            { 
                _userExtractImaging.SetValue(_BATES_PAGE_DIGITS_KEY, value, 
                    RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Gets or sets the page number separator for Bates numbers.
        /// </summary>
        /// <value>The page number separator for Bates numbers.</value>
        /// <returns>The page number separator for Bates numbers.</returns>
        public static string BatesPageNumberSeparator
        {
            get 
            { 
                return (string) _userExtractImaging.GetValue(
                    _BATES_PAGE_NUMBER_SEPARATOR_KEY, _BATES_PAGE_NUMBER_SEPARATOR_DEFAULT);
            }
            set 
            { 
                _userExtractImaging.SetValue(_BATES_PAGE_NUMBER_SEPARATOR_KEY, value, 
                    RegistryValueKind.String);
            }
        }

        /// <summary>
        /// Gets or sets the prefix for Bates numbers.
        /// </summary>
        /// <value>The prefix for Bates numbers.</value>
        /// <returns>The prefix for Bates numbers.</returns>
        public static string BatesPrefix
        {
            get 
            { 
                return (string) _userExtractImaging.GetValue(_BATES_PREFIX_KEY, 
                    _BATES_PREFIX_DEFAULT);
            }
            set 
            { 
                _userExtractImaging.SetValue(_BATES_PREFIX_KEY, value, 
                    RegistryValueKind.String);
            }
        }
        
        /// <summary>
        /// Gets or sets the suffix for Bates numbers.
        /// </summary>
        /// <value>The suffix for Bates numbers.</value>
        /// <returns>The suffix for Bates numbers.</returns>
        public static string BatesSuffix
        {
            get 
            { 
                return (string) _userExtractImaging.GetValue(_BATES_SUFFIX_KEY, 
                    _BATES_SUFFIX_DEFAULT);
            }
            set 
            { 
                _userExtractImaging.SetValue(_BATES_SUFFIX_KEY, value, 
                    RegistryValueKind.String);
            }
        }

        /// <summary>
        /// Gets or sets the whether to use a datbase counter or not
        /// </summary>
        /// <value>Whether to use a databsae counter.</value>
        /// <returns>Whether to use a datbase counter.</returns>
        public static bool UseDatabaseCounter
        {
            get
            {
                int registryValue = (int)_userExtractImaging.GetValue(
                    _BATES_USE_DATABASE_COUNTER, 1);
                return registryValue == 1;
            }
            set
            {
                _userExtractImaging.SetValue(_BATES_USE_DATABASE_COUNTER, value,
                    RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Gets or sets the database counter name for the Bates number
        /// </summary>
        /// <value>The database counter name for the Bates number.</value>
        /// <returns>The database counter name for the Bates number.</returns>
        public static string DatabaseCounterName
        {
            get 
            { 
                return (string) _userExtractImaging.GetValue(_BATES_DATABASE_COUNTER_NAME, "");
            }
            set 
            { 
                _userExtractImaging.SetValue(_BATES_DATABASE_COUNTER_NAME, value, 
                    RegistryValueKind.String);
            }
        }

        #endregion RegistryManager Properties

        #region RegistryManager Methods

        /// <summary>
        /// Retrieves the string value of the specified key as a float.
        /// </summary>
        /// <param name="subkey">The subkey to which <paramref name="key"/> belongs.</param>
        /// <param name="key">The key corresponding to the <paramref name="value"/> to retrieve.
        /// </param>
        /// <param name="defaultValue">The default value to return if <paramref name="key"/> does 
        /// not exist.</param>
        /// <returns>The value at <paramref name="key"/> as a float, or 
        /// <paramref name="defaultValue"/> if <paramref name="key"/> does not exist.</returns>
        static float GetFloat(RegistryKey subkey, string key, float defaultValue)
        {
            // Get the registry value as a string
            string registryValue = subkey.GetValue(key) as string;
            if (registryValue == null)
            {
                return defaultValue;
            }

            // Return the string as a float or else return the default value
            float result;
            return float.TryParse(registryValue, out result) ? result : defaultValue;
        }

        /// <summary>
        /// Gets <see cref="NextBatesNumber"/> and prevents further access until 
        /// <see cref="SetAndReleaseNextBatesNumber"/> or <see cref="ReleaseNextBatesNumber"/> 
        /// is called.
        /// </summary>
        /// <returns>The next Bates number.</returns>
        public static long GetAndHoldNextBatesNumber()
        {
            _nextNumberMutex.WaitOne();

            try
            {
                return GetNextBatesNumberFromApplicationData();
            }
            catch (Exception ex)
            {
                // If there is an exception then need to release the mutex
                _nextNumberMutex.ReleaseMutex();

                // Wrap the exception as an extract exception and rethrow
                throw ExtractException.AsExtractException("ELI28004", ex);
            }
        }

        /// <summary>
        /// Sets <see cref="NextBatesNumber"/> and allows subsequent access to 
        /// <see cref="NextBatesNumber"/> from a previous call to 
        /// <see cref="GetAndHoldNextBatesNumber"/>.
        /// </summary>
        /// <param name="batesNumber">The value to set for the next Bates number.</param>
        public static void SetAndReleaseNextBatesNumber(long batesNumber)
        {
            try
            {
                SetNextBatesNumberInApplicationData(batesNumber);
            }
            catch (Exception ex)
            {
                // Wrap the exception as an extract exception and rethrow
                throw ExtractException.AsExtractException("ELI28005", ex);
            }
            finally
            {
                _nextNumberMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Allows subsequent access to <see cref="NextBatesNumber"/> from a previous call to 
        /// <see cref="GetAndHoldNextBatesNumber"/>.
        /// </summary>
        public static void ReleaseNextBatesNumber()
        {
            _nextNumberMutex.ReleaseMutex();
        }

        /// <summary>
        /// Gets the next Bates number from the file stored in 'all users/application data'
        /// </summary>
        /// <returns>The next Bates number.</returns>
        private static long GetNextBatesNumberFromApplicationData()
        {
            // Default the return value to 1 (if the file does not exist start at 1)
            long nextBatesNumber = 1L;
            if (File.Exists(_nextBatesNumberAppDataFile))
            {
                byte[] nextBatesBytes = File.ReadAllBytes(_nextBatesNumberAppDataFile);

                // Check that there are only 8 bytes in the file, if there is not
                // exactly 8 bytes then the file is corrupt, reinitialize it to 1 [IDSD #360]
                if (nextBatesBytes.Length != 8)
                {
                    // Reinitialize the file to 1
                    File.WriteAllBytes(_nextBatesNumberAppDataFile,
                        BitConverter.GetBytes(nextBatesNumber));

                    // Log an exception
                    ExtractException ee = new ExtractException("ELI28006",
                        "Next Bates number was corrupted, reinitializing to 1");
                    ee.AddDebugData("Bates number file", _nextBatesNumberAppDataFile, false);
                    ee.AddDebugData("Number of bytes found", nextBatesBytes.Length, false);
                    ee.AddDebugData("Bytes",
                        StringMethods.ConvertBytesToHexString(nextBatesBytes), false);
                    ee.Log();
                }
                else
                {
                    nextBatesNumber = BitConverter.ToInt64(nextBatesBytes, 0);
                }
            }

            return nextBatesNumber;
        }

        /// <summary>
        /// Sets the next bates number in the file located in the 'all users/application data'
        /// folder.
        /// </summary>
        /// <param name="batesNumber">The Bates number to store in the file.</param>
        private static void SetNextBatesNumberInApplicationData(long batesNumber)
        {
            byte[] nextBatesBytes = BitConverter.GetBytes(batesNumber);
            File.WriteAllBytes(_nextBatesNumberAppDataFile, nextBatesBytes);
        }

        /// <summary>
        /// Initializes the path for the next bates number file that is stored in the
        /// 'all users/application data' folder (ensures that the file and directory
        /// exists and creates them as necessary).
        /// Specificially at:
        /// All Users\Application Data\Extract Systems\ID Shield Office\BatesNumber.ini
        /// </summary>
        /// <returns>The file name for the next bates number file in the
        /// 'all users/application data' folder.</returns>
        private static string InitializeNextBatesNumberAppDataFileName()
        {
            // Protect critical section
            _nextNumberMutex.WaitOne();

            try
            {
                string fileName =
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                    + Path.DirectorySeparatorChar + "Extract Systems" + Path.DirectorySeparatorChar
                    + "Imaging" + Path.DirectorySeparatorChar + "BatesNumber.ini";

                // Check if the file exists yet, if not we need to create it
                if (!File.Exists(fileName))
                {
                    // File does not exist, check if the directory exists
                    // If the directory for the next bates number file does not exist
                    // then need to create it
                    if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                    }

                    // Initialize the data in the file to 1
                    File.WriteAllBytes(fileName, BitConverter.GetBytes(1L));
                }

                // File and directory exist, return the file name
                return fileName;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28007", ex);
            }
            finally
            {
                _nextNumberMutex.ReleaseMutex();
            }
        }

        #endregion RegistryManager Methods
    }
}
