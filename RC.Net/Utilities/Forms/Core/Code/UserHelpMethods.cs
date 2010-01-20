using System;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a grouping of methods for displaying user help.
    /// </summary>
    public static class UserHelpMethods
    {
        #region Constants

        /// <summary>
        /// The URL to the regular expression help file.
        /// </summary>
        const string _REGEX_HELP_FILE_URL =
            @"http://msdn.microsoft.com/en-us/library/hs600312(VS.80).aspx";

        #endregion Constants

        #region Methods

        /// <summary>
        /// Displays the regular expression help file using the specified parent control.
        /// </summary>
        /// <param name="parent">The parent of the help dialog box.</param>
        public static void ShowRegexHelp(Control parent)
        {
            try
            {
                Help.ShowHelp(parent, _REGEX_HELP_FILE_URL);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23210", ex);
            }
        }

        #endregion Methods
    }
}