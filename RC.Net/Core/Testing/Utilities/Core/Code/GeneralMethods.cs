using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Testing.Utilities
{
    /// <summary>
    /// A static class containing test helper methods.
    /// </summary>
    public static class GeneralMethods
    {
        /// <summary>
        /// General test setup function.  Should be called in each testing assembly
        /// TestFixtureSetup function.
        /// </summary>
        public static void TestSetup()
        {
            // Load the license files
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
        }

        /// <summary>
        /// Resets the license state for all licensed components and clears all
        /// cached values to force rechecking of license state.
        /// </summary>
        public static void ResetLicenseState()
        {
            // Enable all the license IDS and reset the license validation cache
            LicenseUtilities.EnableAll();
            LicenseUtilities.ResetCache();
        }
    }
}
