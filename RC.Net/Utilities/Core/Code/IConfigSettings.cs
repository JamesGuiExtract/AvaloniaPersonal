using System;
using System.Linq;

namespace Extract.Utilities
{
    public interface IConfigSettings
    {
        /// <summary>
        /// Method to display a configuration dialog
        /// </summary>
        /// <returns><see langword="true"/> if configuration was successful, <see langword="false"/> if not</returns>
        bool Configure();

        /// <summary>
        /// Method to check if object is configured
        /// </summary>
        /// <returns><see langword="true"/> if valid configuration, <see langword="false"/> if not a valid configuration</returns>
        bool IsConfigured();
    }
}
