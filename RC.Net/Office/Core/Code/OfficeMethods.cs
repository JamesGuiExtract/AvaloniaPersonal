using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Office
{
    /// <summary>
    /// Class containing helper methods for working with MS Office documents and objects.
    /// </summary>
    public static class OfficeMethods
    {
        /// <summary>
        /// Returns the version number of the most current version of office that is installed.
        /// If no version of office is installed this returns -1.
        /// </summary>
        /// <returns>The version number for the most current version of office that is installed.
        /// </returns>
        public static int CheckOfficeVersion()
        {
            try
            {
                // Return the current version of Word, this is making the assumption that
                // the current version of word is equivalent to the current version of office.
                return RegistryManager.GetWordVersion();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30281", ex);
            }
        }
    }
}
