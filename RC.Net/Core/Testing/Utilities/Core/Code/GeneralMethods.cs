using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

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

        /// <summary>
        /// Writes the embedded resource to a <see cref="TemporaryFile"/> instance that is named
        /// based on the name of the resource file (but in the temp file directory).
        /// </summary>
        /// <typeparam name="T">The type used to resolve the resource location.</typeparam>
        /// <param name="assembly">The assembly containing the embedded resource.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <returns>A <see cref="TemporaryFile"/> instance to manage the </returns>
        // User needs to supply the type parameter since it is used to get the calling assembly
        // and find the embedded resource to export.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static TemporaryFile WriteEmbeddedResourceToTemporaryFile<T>(Assembly assembly,
            string resourceName)
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
                    // Remove any project-prefix to the resource name that is not related to the
                    // original filename.
                    string outputFileName = Regex.Replace(resourceName,
                        @"^(Properties\.)?Resources\.", "", RegexOptions.IgnoreCase);

                    outputFileName = Path.Combine(Path.GetTempPath(), outputFileName);

                    // Since a static filename is being specified (and TemporaryFile will not be
                    // able to guarantee uniqueness), delete any existing instance of the file so
                    // that this operation acts as an overwrite.
                    File.Delete(outputFileName);

                    FileInfo fileInfo = new FileInfo(outputFileName);
                    var temporaryFile = new TemporaryFile(fileInfo, false);
                    
                    File.WriteAllBytes(temporaryFile.FileName,
                        StreamMethods.ConvertStreamToByteArray(stream));

                    return temporaryFile;
                }
            }
            catch (Exception ex)
            {
                var ee = ExtractException.AsExtractException("ELI38404", ex);
                ee.AddDebugData("Type", type.ToString(), false);
                ee.AddDebugData("Resource Name", resourceName, false);
                throw ee;
            }
        }
    }
}
