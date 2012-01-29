using Extract;
using Extract.Licensing;
using Leadtools;
using Leadtools.Codecs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Extract.UtilityApplications.CustomApplications.EmbedPageNumber
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Command line arguments for the application.</param>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Load the licenses
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                // Validate that this is licensed
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI28542", "Embed Page Number Application");

                if (args.Length != 1)
                {
                    ShowUsage("Invalid number of arguments.");
                    return;
                }

                // Get the file name from the argument list
                string fullFileName = Path.GetFullPath(args[0]);
                string fileName = Path.GetFileName(fullFileName);

                // Ensure the file name meets the criteria
                if (CheckFileName(fileName))
                {
                    // Get the page count
                    int pageCount = GetImagePageCount(fullFileName);
                    if (pageCount != -1)
                    {
                        // Get the index of the last XXXXXX
                        int index = fileName.LastIndexOf("XXXXXX", StringComparison.OrdinalIgnoreCase);

                        // Build the new file name
                        StringBuilder sb = new StringBuilder(fileName.Substring(0, index));
                        sb.Append(pageCount.ToString("D6", CultureInfo.InvariantCulture));
                        sb.Append(fileName.Substring(index+6));

                        // Rename the file (move it from the old name to the new name)
                        File.Move(fullFileName,
                            Path.Combine(Path.GetDirectoryName(fullFileName), sb.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28543", ex);
            }
        }

        /// <summary>
        /// Gets the page count for the specified image file. If the specified file
        /// is not an image file then -1 will be returned.
        /// </summary>
        /// <param name="fileName">The file to get the page count from.</param>
        /// <returns>The page count or -1 if the file is not a supported image format.</returns>
        static int GetImagePageCount(string fileName)
        {
            RasterCodecs codecs = null;
            CodecsImageInfo info = null;
            try
            {
                // Get the file information
                codecs = new RasterCodecs();
                info = codecs.GetInformation(fileName, true);

                // Return the page count
                return info.TotalPages;
            }
            catch (RasterException rex)
            {
                // Check for file format exception
                if (rex.Code == RasterExceptionCode.FileFormat)
                {
                    return -1;
                }
                throw ExtractException.AsExtractException("ELI28544", rex);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28545", ex);
            }
            finally
            {
                if (info != null)
                {
                    info.Dispose();
                }
                if (codecs != null)
                {
                    codecs.Dispose();
                }
            }
        }

        /// <summary>
        /// Checks the file name for the customer specific criteria:
        /// <list type="number">
        /// <item><description>File name is exactly 200 characters long.</description></item>
        /// <item><description>XXXXXX are the last 6 characters before the
        /// last period in the file name.</description></item>
        /// </list>
        /// </summary>
        /// <param name="fileName">The name of the file to check</param>
        /// <returns><see langword="true"/> if the file name meets the criteria
        /// and <see langword="false"/> otherwise.</returns>
        static bool CheckFileName(string fileName)
        {
            bool pass = !string.IsNullOrEmpty(fileName) && fileName.Length == 200;
            if (pass)
            {
                int index = fileName.LastIndexOf(".", StringComparison.Ordinal);
                pass = index > 6 && fileName.Substring(index - 6, 6).Equals("XXXXXX",
                    StringComparison.OrdinalIgnoreCase);
            }

            return pass;
        }

        /// <summary>
        /// Displays a usage message to the user.
        /// </summary>
        /// <param name="message">Error message to display along with the usage.</param>
        static void ShowUsage(string message)
        {
            // Build the usage message
            StringBuilder sb = new StringBuilder(message ?? "");
            sb.AppendLine();
            sb.AppendLine("Usage:");
            sb.Append(Path.GetFileNameWithoutExtension(Application.ExecutablePath));
            sb.AppendLine(" <FileName>");
            sb.AppendLine("--------------");
            sb.AppendLine("<FileName> - The file name to modify based on the page count.");
            sb.AppendLine();

            // Display the message
            MessageBox.Show(sb.ToString(), "Usage", MessageBoxButtons.OK,
                string.IsNullOrEmpty(message) ? MessageBoxIcon.Information : MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, 0);
        }
    }
}