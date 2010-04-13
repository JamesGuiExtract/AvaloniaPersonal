using Microsoft.Win32;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.Threading;

namespace Extract.Imaging.Forms
{
    internal static class RegistryManager
    {
        #region RegistryManager Constants

        #region RegistryManager Subkeys

        /// <summary>
        /// The current user registry subkey for the spot recognition input receiver.
        /// </summary>
        const string _SPOT_RECOGNITION_IR_SUBKEY =
            @"Software\Extract Systems\InputFunnel\InputReceivers\SpotRecIR";

        /// <summary>
        /// The current user registry subkey for the most recently used images list.
        /// </summary>
        const string _MRU_LIST_USER_SUBKEY = _SPOT_RECOGNITION_IR_SUBKEY
            + @"\MRUList";

        /// <summary>
        /// The current user registry subkey for the image edit settings settings
        /// </summary>
        const string _IMAGE_EDIT_SUBKEY =
            @"Software\Extract Systems\ReusableComponents\OcxAndDlls\ImageEdit";

        /// <summary>
        /// The name for the MRU mutex.
        /// </summary>
        // DO NOT make this a const string, static readonly means that it will be
        // encrypted by the obfuscator, if it is changed to a const it will not be
        // encrypted.
        static readonly string _MRU_MUTEX_NAME = @"Global\D7AFD341-3D38-4C57-BEBA-52DD56EC7E5A";

        #endregion RegistryManager Subkeys

        #region RegistryManager Keys

        /// <summary>
        /// The registry key that contains whether to display percentages.
        /// </summary>
        const string _DISPLAY_PERCENTAGES_USER_KEY = "DisplayPercentageEnabled";

        /// <summary>
        /// The key that contains the last used selection tool.
        /// </summary>
        const string _SELECTION_TOOL_USER_KEY = "SelectionTool";

        /// <summary>
        /// The key that contains the last fit to mode.
        /// </summary>
        const string _FIT_TO_MODE_KEY = "FitToStatus";

        /// <summary>
        /// The key that contains the last used printer.
        /// </summary>
        const string _LAST_PRINTER_KEY = "LastPrinter";

        /// <summary>
        /// The key that contains the maximum number of times save should retry if it fails
        /// to get a device context [IDSD #331]
        /// </summary>
        const string _SAVE_RETRY = "SaveRetries";

        /// <summary>
        /// The key that contains whether anti-aliasing should be turned on or not.
        /// [DNRCAU #422]
        /// </summary>
        const string _USE_ANTI_ALIASING_KEY = "AntiAliasing";

        #endregion RegistryManager Keys

        #region RegistryManager Values

        /// <summary>
        /// The maximum number of items in the most recently used items list.
        /// </summary>
        const int _MAX_MRU_IMAGES = 8;

        /// <summary>
        /// Value to store in the registry for the Angular selection tool.
        /// </summary>
        const string _ANGULAR_SELECTION_TOOL_VALUE = "8";

        /// <summary>
        /// Value to store in the registry for the Rectangular selection tool.
        /// </summary>
        const string _RECTANGULAR_SELECTION_TOOL_VALUE = "16";

        /// <summary>
        /// Value to store in the registry for fit to width mode.
        /// </summary>
        const string _FIT_TO_WIDTH_VALUE = "0";

        /// <summary>
        /// Value to store in the registry for the fit to page mode.
        /// </summary>
        const string _FIT_TO_PAGE_VALUE = "1";

        /// <summary>
        /// Value to store in the registry for no fit mode.
        /// </summary>
        const string _NO_FIT_VALUE = "2";

        #endregion RegistryManager Values

        #endregion RegistryManager Constants

        #region RegistryManager Fields

        /// <summary>
        /// The current user registry subkey for the Spot Recognition Input Receiver.
        /// </summary>
        static readonly RegistryKey _userSpotRecognitionSubkey = 
            Registry.CurrentUser.CreateSubKey(_SPOT_RECOGNITION_IR_SUBKEY);

        /// <summary>
        /// The current user registry subkey for the image edit settings.
        /// </summary>
        static readonly RegistryKey _userImageEditSubkey =
            Registry.CurrentUser.CreateSubKey(_IMAGE_EDIT_SUBKEY);

        /// <summary>
        /// Mutex used to make access to the MRU image list in the registry thread safe.
        /// </summary>
        static readonly Mutex _mruMutex = ThreadingMethods.GetGlobalNamedMutex(_MRU_MUTEX_NAME);

        #endregion RegistryManager Fields

        #region RegistryManager Properties

        /// <summary>
        /// Gets or sets whether to display mouse cursor percentages.
        /// </summary>
        /// <returns><see langword="true"/> if mouse cursor percentages should be displayed;
        /// <see langword="false"/> if mouse cursor percentages should not be displayed.</returns>
        public static bool DisplayPercentages
        {
            get
            {
                string registryValue = (string)_userSpotRecognitionSubkey.GetValue(
                    _DISPLAY_PERCENTAGES_USER_KEY, "1");

                return registryValue == "1";
            }
        }

        /// <summary>
        /// Gets whether or not to turn anti-aliasing on in the Image viewer
        /// </summary>
        /// <returns><see langword="true"/> if anti-aliasing should be turned on;
        /// <see langword="false"/> if anti-aliasing should be turned off.
        /// </returns>
        public static bool UseAntiAliasing
        {
            get
            {
                string registryValue = (string)_userImageEditSubkey.GetValue(
                    _USE_ANTI_ALIASING_KEY, "1");

                return registryValue == "1";
            }
        }

        /// <summary>
        /// Gets or sets the the last fit mode used.
        /// </summary>
        /// <value>The the last fit mode used.</value>
        /// <returns>The the last fit mode used.</returns>
        public static FitMode FitMode
        {
            get
            {
                string registryValue = (string)_userSpotRecognitionSubkey.GetValue(
                    _FIT_TO_MODE_KEY, _NO_FIT_VALUE);

                if (registryValue == _FIT_TO_PAGE_VALUE)
                {
                    return FitMode.FitToPage;
                }
                else if (registryValue == _FIT_TO_WIDTH_VALUE)
	            {
                    return FitMode.FitToWidth;
	            }
                else
                {
                    return FitMode.None;
                }
            }
            set
            {
                try
                {
                    string registryValue;
                    switch (value)
                    {
                        case FitMode.None:
                            registryValue = _NO_FIT_VALUE;
                            break;
                        case FitMode.FitToPage:
                            registryValue = _FIT_TO_PAGE_VALUE;
                            break;
                        case FitMode.FitToWidth:
                            registryValue = _FIT_TO_WIDTH_VALUE;
                            break;
                        default:
                            throw new ExtractException("ELI23224", "Unexpected fit mode.");
                    }

                    _userSpotRecognitionSubkey.SetValue(_FIT_TO_MODE_KEY, registryValue);
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI23225", ex);
                    ee.AddDebugData("Fit mode", value, false);
                    throw ee;
                }
            }
        }
        
        #endregion RegistryManager Properties

        #region RegistryManager Methods

        /// <summary>
        /// Gets the last used highlight tool.
        /// </summary>
        /// <returns>The last used highlight tool.</returns>
        public static CursorTool GetLastUsedHighlightTool()
        {
            // Get the selection tool's value from the registry
            string registryValue = (string) _userSpotRecognitionSubkey.GetValue(
                _SELECTION_TOOL_USER_KEY, _RECTANGULAR_SELECTION_TOOL_VALUE);

            // Return the appropriate cursor tool
            return registryValue == _RECTANGULAR_SELECTION_TOOL_VALUE ?
                CursorTool.RectangularHighlight : CursorTool.AngularHighlight;
        }

        /// <summary>
        /// Gets the last used redaction tool.
        /// </summary>
        /// <returns>The last used redaction tool.</returns>
        public static CursorTool GetLastUsedRedactionTool()
        {
            // Get the selection tool's value from the registry
            string registryValue = (string)_userSpotRecognitionSubkey.GetValue(
                _SELECTION_TOOL_USER_KEY, _RECTANGULAR_SELECTION_TOOL_VALUE);

            // Return the appropriate cursor tool
            return registryValue == _RECTANGULAR_SELECTION_TOOL_VALUE ?
                CursorTool.RectangularRedaction : CursorTool.AngularRedaction;
        }

        /// <summary>
        /// Sets the last used selection tool.
        /// </summary>
        /// <param name="cursorTool">The last used selection tool.</param>
        public static void SetLastUsedSelectionTool(CursorTool cursorTool)
        {
            try
            {
                string registryValue;
                switch (cursorTool)
                {
                    case CursorTool.AngularHighlight:
                    case CursorTool.AngularRedaction:
                        registryValue = _ANGULAR_SELECTION_TOOL_VALUE;
                        break;

                    case CursorTool.RectangularHighlight:
                    case CursorTool.RectangularRedaction:
                        registryValue = _RECTANGULAR_SELECTION_TOOL_VALUE;
                        break;

                    default:
                        throw new ExtractException("ELI23222", "Unexpected cursor tool.");
                }

                _userSpotRecognitionSubkey.SetValue(_SELECTION_TOOL_USER_KEY, registryValue);
            }
            catch (Exception ex)
	        {
                ExtractException ee = ExtractException.AsExtractException("ELI23223", ex);
	            ee.AddDebugData("Cursor tool", cursorTool, false);
	            throw ee;
	        }
        }

        /// <summary>
        /// Creates an array of the most recently used image file names.
        /// </summary>
        /// <returns>An array of the most recently used image file names.</returns>
        public static List<string> GetMostRecentlyUsedImageFiles()
        {
            List<string> mruList = new List<string>();

            // Ensure thread safety over this section
            _mruMutex.WaitOne();

            try
            {
                // Get the most recently used image list
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(_MRU_LIST_USER_SUBKEY))
                {
                    if (key != null)
                    {
                        // Iterate through each file in the key
                        for (int i = 1; i <= _MAX_MRU_IMAGES; i++)
                        {
                            // Check if this value exists
                            object value = key.GetValue("File_" + 
                                i.ToString(CultureInfo.InvariantCulture));
                            if (value == null)
                            {
                                // The value doesn't exist in the registry, we are done.
                                break;
                            }
                            else
                            {
                                // Add this file to the most recently used image list
                                mruList.Add(value.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI21581",
                    "Unable to read MRU image list from registry.", ex);
            }
            finally
            {
                // Ensure the mutex is released
                _mruMutex.ReleaseMutex();
            }

            return mruList;
        }

        /// <summary>
        /// Writes the provided <see cref="List{T}"/> containing the most recently
        /// used image files to the registry.
        /// </summary>
        /// <param name="mruList">Most recently used image file list.</param>
        static void SetMostRecentlyUsedImageFiles(IList<string> mruList)
        {
            // Ensure thread safety over this section
            _mruMutex.WaitOne();

            try
            {
                // Delete the MRU list subkey
                Registry.CurrentUser.DeleteSubKey(_MRU_LIST_USER_SUBKEY, false);

                // Get write access to the most recently used image list from the registry
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(_MRU_LIST_USER_SUBKEY))
                {
                    int fileCount = Math.Min(mruList.Count, _MAX_MRU_IMAGES);
                    for (int i = 1; i <= fileCount; i++)
                    {
                        // Store this file in the registry
                        key.SetValue("File_" + i.ToString(CultureInfo.InvariantCulture), 
                            mruList[i - 1]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI21582",
                    "Unable to write MRU image list to registry.", ex);
            }
            finally
            {
                // Ensure the mutex is released
                _mruMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Adds the specified file name to most recently used image file list.
        /// </summary>
        /// <param name="fileName">Filename to add to the most recently used
        /// image file list.</param>
        public static void AddMostRecentlyUsedImageFile(string fileName)
        {
            // Get the MRU image list from the registry
            List<string> mruList = GetMostRecentlyUsedImageFiles();

            // Search for the current file name in the MRU image list.
            // (index will be -1 if it is not in the list)
            int index = IndexOfImageFile(fileName, mruList);

            // If the filename is already the most recently opened file, we are done.
            if (index == 0)
            {
                return;
            }

            // Remove the previous filename if it already exists
            if (index > 0)
            {
                mruList.RemoveAt(index);
            }

            // If the maximum number of items in the most recently used item list 
            // has been reached remove the last item before adding this one.
            if (mruList.Count >= _MAX_MRU_IMAGES)
            {
                mruList.RemoveAt(mruList.Count - 1);
            }

            // Add the most recently opened image file to the MRU list
            mruList.Insert(0, fileName);

            // Write the most recently used image list to the registry
            SetMostRecentlyUsedImageFiles(mruList);
        }

        /// <summary>
        /// Removes the specified file from the most recently used image file list..
        /// </summary>
        /// <param name="fileName">File name to remove from the most recently used image file list.
        /// </param>
        public static void RemoveMostRecentlyUsedImageFile(string fileName)
        {
            // Get the MRU image list from the registry
            List<string> mruList = GetMostRecentlyUsedImageFiles();

            // Search for the file name in the MRU list
            // (returns -1 if not found)
            int index = IndexOfImageFile(fileName, mruList);

            // If fileName exists in mruList, remove it
            if (index >= 0)
            {
                mruList.RemoveAt(index);

                // Write the most recently used image list to the registry
                SetMostRecentlyUsedImageFiles(mruList);
            }
        }

        /// <summary>
        /// Finds the specified file in the most recently used image list and returns its index.
        /// </summary>
        /// <param name="fileName">The file name for which to search.</param>
        /// <param name="mruList">The list of most recently used image files in which to search.
        /// </param>
        /// <returns>The index of <paramref name="fileName"/> in <paramref name="mruList"/>; or -1 
        /// if <paramref name="fileName"/> does not exist in <paramref name="mruList"/>.</returns>
        static int IndexOfImageFile(string fileName, IList<string> mruList)
        {
            for (int i = 0; i < mruList.Count; i++)
            {
                // Compare each MRU entry case-insensitively
                if (mruList[i].Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            // If we reached this point, the file was not found
            return -1;
        }

        /// <summary>
        /// Gets the last used printer name.
        /// </summary>
        /// <returns>The last used printer name.</returns>
        public static string GetLastUsedPrinter()
        {
            try
            {
                string lastPrinter = _userSpotRecognitionSubkey.GetValue(_LAST_PRINTER_KEY, "")
                    as string;

                if (!string.IsNullOrEmpty(lastPrinter))
                {
                    // Check that the last printer is valid
                    PrinterSettings printerSettings = new PrinterSettings();
                    printerSettings.PrinterName = lastPrinter;
                    if (!printerSettings.IsValid)
                    {
                        // Printer name is not valid, reset the registry key and set last
                        // printer to empty string
                        SetLastUsedPrinter("");
                        lastPrinter = "";
                    }
                }

                // Return the last printer
                return lastPrinter ?? "";
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23304", ex);
            }
        }

        /// <summary>
        /// Sets the last used printer name.
        /// </summary>
        /// <param name="printerName">The name of the last used printer.</param>
        public static void SetLastUsedPrinter(string printerName)
        {
            try
            {
                _userSpotRecognitionSubkey.SetValue(_LAST_PRINTER_KEY, printerName);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23303", ex);
            }
        }

        #endregion RegistryManager Methods

        #region RegistryManager Properties

        /// <summary>
        /// Gets the number of retries to try when saving a file
        /// (See IDSD #331)
        /// </summary>
        public static int SaveRetries
        {
            get
            {
                // Get the retries key
                int? retries = _userSpotRecognitionSubkey.GetValue(_SAVE_RETRY, 20) as int?;

                // Return the value from the registry key
                return retries ?? 20;
            }
        }

        #endregion RegistryManager Properties
    }
}
