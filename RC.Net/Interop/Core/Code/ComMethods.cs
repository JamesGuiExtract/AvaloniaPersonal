using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Interop
{
    /// <summary>
    /// Represents a collection of methods for COM interoperability.
    /// </summary>
    public static class ComMethods
    {
        #region ComMethods Methods

        /// <summary>
        /// Registers a type as implementing a COM category.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="guidCategory">The category <paramref name="type"/> implements.</param>
        public static void RegisterTypeInCategory(Type type, string guidCategory)
        {
            using (RegistryKey registryKey = getCategoryKey(type))
            {
                if (registryKey != null)
                {
                    registryKey.CreateSubKey(guidCategory);
                }
            }
        }

        /// <summary>
        /// Unregisters a type registered as implementing a COM category.
        /// </summary>
        /// <param name="type">The type to unregister.</param>
        /// <param name="guidCategory">The category <paramref name="type"/> is registered to 
        /// implement.</param>
        public static void UnregisterTypeInCategory(Type type, string guidCategory)
        {
            using (RegistryKey registryKey = getCategoryKey(type))
            {
                if (registryKey != null)
                {
                    registryKey.DeleteSubKey(guidCategory);
                }
            }
        }

        /// <summary>
        /// Gets the registry key that corresponds to the implemented categories of the 
        /// specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type for which to retrieve the registry key.</param>
        /// <returns>The registry key that corresponds to the implemented categories of 
        /// <paramref name="type"/>.</returns>
        static RegistryKey getCategoryKey(Type type)
        {
            string key = @"\CLSID\{" + type.GUID.ToString() + @"}\Implemented Categories";
            return Registry.ClassesRoot.OpenSubKey(key, true);
        }

        #endregion ComMethods Methods
    }
}
