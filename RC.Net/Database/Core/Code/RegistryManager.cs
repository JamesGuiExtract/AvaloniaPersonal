using Extract;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpDatabaseUtilities
{
    /// <summary>
    /// Manages registry settings for the CSharpDatabaseUtilities project. 
    /// </summary>
    internal static class RegistryManager
    {
        #region RegistryManager Constants

        #region RegistryManager SubKeys

        /// <summary>
        /// The sub key for CSharpDatabaseUtilities keys.
        /// </summary>
        const string _CSHARP_DATABASE_UTILITIES_SUBKEY =
            @"Software\Extract Systems\ReusableComponents\CSharpDatabaseUtilities";

        #endregion RegistryManager SubKeys

        #region RegistryManager Keys

        /// <summary>
        /// The key for verbose logging of exceptions.
        /// </summary>
        const string _VERBOSE_LOGGING_KEY = "Verbose logging";

        #endregion RegistryManager Keys

        #endregion RegistryManager Constants

        #region RegistryManager Fields

        /// <summary>
        /// The current user registry sub key for CSharpDatabaseUtilities keys.
        /// </summary>     
        static RegistryKey _cSharpDatabaseUtilitiesSubKey =
            Registry.CurrentUser.CreateSubKey(_CSHARP_DATABASE_UTILITIES_SUBKEY);

        #endregion RegistryManager Fields

        #region RegistryManager Properties

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
                    int? reg = _cSharpDatabaseUtilitiesSubKey.GetValue(_VERBOSE_LOGGING_KEY, 0) as int?;
                    return reg != null && reg.Value == 1;
                }
                catch (Exception ex)
                {
                    throw new ExtractException("ELI23471", 
                        "Unable to get verbose logging setting.", ex);
                }
            }
        }

        #endregion RegistryManager Properties
    }
}
