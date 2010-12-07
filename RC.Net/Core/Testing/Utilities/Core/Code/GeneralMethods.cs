using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

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

        /// <summary>
        /// Writes the embedded resource to the specified file.
        /// </summary>
        /// <typeparam name="T">The type used to resolve the resource location.</typeparam>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="fileName">Name of the file.</param>
        // User needs to supply the type parameter since it is used to get the calling assembly
        // and find the embedded resource to export.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static void WriteEmbeddedResourceToFile<T>(string resourceName, string fileName)
        {
            WriteEmbeddedResourceToFile<T>(null, resourceName, fileName);
        }

        /// <summary>
        /// Writes the embedded resource to the specified file.
        /// </summary>
        /// <typeparam name="T">The type used to resolve the resource location.</typeparam>
        /// <param name="assembly">The assembly containing the embedded resource.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="fileName">Name of the file.</param>
        // User needs to supply the type parameter since it is used to get the calling assembly
        // and find the embedded resource to export.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static void WriteEmbeddedResourceToFile<T>(Assembly assembly,
            string resourceName, string fileName)
        {
            // Get the type
            Type type = typeof(T);

            try
            {
                // If no assembly was specified, get the assembly
                if (assembly == null)
                {
                    assembly = Assembly.GetAssembly(type);
                }

                // Write the embedded resource to the stream
                using (Stream stream =
                    assembly.GetManifestResourceStream(type, resourceName))
                {
                    File.WriteAllBytes(fileName,
                        StreamMethods.ConvertStreamToByteArray(stream));
                }
            }
            catch (Exception ex)
            {
                var ee = ExtractException.AsExtractException("ELI31133", ex);
                ee.AddDebugData("Type", type.ToString(), false);
                ee.AddDebugData("Resource Name", resourceName, false);
                ee.AddDebugData("File Name", fileName, false);
                throw ee;
            }
        }
    }
}
