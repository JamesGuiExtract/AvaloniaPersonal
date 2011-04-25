using System;
using System.IO;

namespace Extract.Office
{
    /// <summary>
    /// Helper class to hold the arguments parsed from the office to tif application
    /// argument list.
    /// </summary>
    public class OfficeToTifArguments
    {
        /// <summary>
        /// Gets or sets The file name of the office document to convert.
        /// </summary>
        public string OfficeDocumentName { get; internal set; }

        /// <summary>
        /// Gets or sets the name of the destination tif file.
        /// <para><b>Note:</b></para>
        /// Regardless of the file name, the output file will be a tif format.
        /// </summary>
        public string DestinationFileName { get; internal set; }

        /// <summary>
        /// Gets or sets the office application type for the specified file.
        /// </summary>
        public OfficeApplication OfficeApplication { get; internal set; }

        /// <summary>
        /// Gets or sets the exception file to log any exceptions to.
        /// </summary>
        public string ExceptionFile { get; internal set; }
    }

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

        /// <summary>
        /// Parses the arguments contained in the specified file.
        /// </summary>
        /// <param name="fileWithArguments">The name of the file containing the arguments.</param>
        /// <returns>The parsed set of office to tif arguments.</returns>
        public static OfficeToTifArguments ParseOfficeToTifApplicationArguments(string fileWithArguments)
        {
            try
            {
                // Check for empty file name
                if (string.IsNullOrWhiteSpace(fileWithArguments))
                {
                    throw new ArgumentNullException("fileWithArguments");
                }
                else if (!File.Exists(fileWithArguments))
                {
                    throw new ArgumentException("Invalid file name.", "fileWithArguments");
                }

                // Read the arguments from the file, file should contain the following items
                // 1. File name
                // 2. The name of the destination file
                // 3. The office application value (from the OfficeApplication enum)
                // 4. The exception file to log exceptions to if an exception occurs
                var arguments = File.ReadAllLines(fileWithArguments);
                if (arguments == null || arguments.Length != 4)
                {
                    throw new ExtractException("ELI32440", "Invalid argument list.");
                }

                // Parse the arguments
                var officeArguments = new OfficeToTifArguments();
                officeArguments.OfficeDocumentName = Path.GetFullPath(arguments[0]);
                officeArguments.DestinationFileName = Path.GetFullPath(arguments[1]);
                officeArguments.OfficeApplication = (OfficeApplication)Enum.Parse(
                    typeof(OfficeApplication), arguments[2]);
                officeArguments.ExceptionFile = Path.GetFullPath(arguments[3]);

                return officeArguments;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32438");
            }
        }
    }
}
