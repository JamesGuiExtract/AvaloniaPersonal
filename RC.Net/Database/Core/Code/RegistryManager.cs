using Microsoft.Win32;
using System;

namespace Extract.Database
{
    /// <summary>
    /// Manages registry settings for the Extract.Database project. 
    /// </summary>
    internal static class RegistryManager
    {
        #region Constants

        #region SubKeys

        /// <summary>
        /// The sub key for Extract.Database keys.
        /// </summary>
        const string _EXTRACT_DATABASE_SUBKEY =
            @"Software\Extract Systems\ReusableComponents\Extract.Database";

        #endregion SubKeys

        #region Keys

        /// <summary>
        /// The key for verbose logging of exceptions.
        /// </summary>
        const string _VERBOSE_LOGGING_KEY = "Verbose logging";

        #endregion Keys

        #endregion Constants

        #region Fields

        /// <summary>
        /// The current user registry sub key for Extract.Database keys.
        /// </summary>     
        static readonly RegistryKey _databaseSubKey =
            Registry.CurrentUser.CreateSubKey(_EXTRACT_DATABASE_SUBKEY);

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets whether verbose logging is enabled.
        /// </summary>
        /// <returns><see langword="true"/> if verbose logging is enabled;
        /// <see langword="false"/> if verbose logging is disabled.</returns>
        public static bool VerboseLogging
        {
            get 
            {
                try
                {
                    int? reg = _databaseSubKey.GetValue(_VERBOSE_LOGGING_KEY, 0) as int?;
                    return reg != null && reg.Value == 1;
                }
                catch (Exception ex)
                {
                    throw new ExtractException("ELI23471", 
                        "Unable to get verbose logging setting.", ex);
                }
            }
        }

        #endregion Properties
    }
}
