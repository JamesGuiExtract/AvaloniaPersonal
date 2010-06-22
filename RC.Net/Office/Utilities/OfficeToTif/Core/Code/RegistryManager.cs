using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Extract.Office.Utilities.OfficeToTif
{
    /// <summary>
    /// Manages reading/writing registry keys for the OfficeToTif application.
    /// </summary>
    static class RegistryManager
    {
        /// <summary>
        /// The registry key containing the current version of Word information.
        /// </summary>
        static readonly RegistryKey _wordKey = Registry.ClassesRoot.CreateSubKey(
            @"Word.Application\CurVer", RegistryKeyPermissionCheck.ReadSubTree);

        /// <summary>
        /// Gets the current installed version of Word on the system (this returns the
        /// version number for the newest version installed, so if Word 2000, 2003 and 2007
        /// are all installed this function will return 12 (2007).
        /// </summary>
        /// <returns>The current installed version of Word. Returns -1 if
        /// Word is not installed.</returns>
        static public int GetWordVersion()
        {
            try
            {
                string applicationVersion = (string)_wordKey.GetValue("",
                    "Word.Application.-1");

                // Key is stored as Word.Application.#, splite at '.' and grab
                // last piece to get version number
                string[] pieces = applicationVersion.Split(new char[] { '.' },
                    StringSplitOptions.RemoveEmptyEntries);
                string version = pieces[pieces.Length - 1];

                return int.Parse(version, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30274", ex);
            }
        }
    }
}
