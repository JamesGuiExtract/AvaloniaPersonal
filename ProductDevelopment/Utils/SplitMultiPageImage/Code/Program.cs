using Extract;
using Extract.Imaging;
using Extract.Imaging.Utilities;
using Extract.Licensing;
using Extract.Utilities;
using Leadtools;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SplitMultiPageImage
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string logFile = null;
            try
            {
                if (args.Length < 1 || args.Length > 5)
                {
                    ShowUsage("Invalid number of arguments.");
                    return;
                }

                if (args[0] == "/?")
                {
                    ShowUsage();
                    return;
                }

                // Get the path relative to the current working directory
                // [LRCAU #5590]
                string input = Path.GetFullPath(args[0]);
                bool delete = false;
                bool overwrite = false;

                // Get command line arguments
                for (int i = 1; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (arg == "/d")
                    {
                        delete = true;
                    }
                    else if (arg == "/o")
                    {
                        overwrite = true;
                    }
                    else if (arg == "/ef")
                    {
                        if (i+1 >= args.Length)
                        {
                            ShowUsage("Log file argument required.");
                            return;
                        }

                        logFile = FileSystemMethods.GetAbsolutePath(args[i+1]);
                        i++;
                    }
                    else
                    {
                        ShowUsage("Invalid argument: " + arg);
                        return;
                    }
                }

                // Validate license
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI28494", "Split Multi Page Image");

                // Retain annotation if annotations are licensed
                bool retainAnnotations = false;
                ExtractException ex = UnlockLeadtools.UnlockDocumentSupport(true);
                if (ex == null)
                {
                    retainAnnotations = true;
                }

                // Perform the file splitting
                using (ImageCodecs codecs = new ImageCodecs())
                {
                    if (Directory.Exists(input))
                    {
                        SplitFilesInFolder(codecs, input, overwrite, delete, retainAnnotations);
                    }
                    else if (File.Exists(input))
                    {
                        SplitFile(codecs, input, overwrite, delete, retainAnnotations);
                    }
                    else
                    {
                        ExtractException ee = new ExtractException("ELI28436", 
                            "Invalid input file.");
                        ee.AddDebugData("Input file", input, false);
                        throw ee;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28435", ex);

                if (string.IsNullOrEmpty(logFile))
                {
                    ee.Display();
                }
                else
                {
                    ee.Log(logFile);
                }
            }
        }

        /// <summary>
        /// Displays the usage message.
        /// </summary>
        static void ShowUsage()
        {
            ShowUsage(null);
        }

        /// <summary>
        /// Displays the usage message with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message to display.</param>
        static void ShowUsage(string errorMessage)
        {
            bool isError = !string.IsNullOrEmpty(errorMessage);

            StringBuilder usage = new StringBuilder();

            if (isError)
            {
                usage.AppendLine(errorMessage);
                usage.AppendLine();
            }

            usage.AppendLine("Splits each page of a multipage tiff into individual tiff files.");
            usage.AppendLine();

            usage.Append(Environment.GetCommandLineArgs()[0]);
            usage.AppendLine(" input [/d] [/o] [/ef log]");
            usage.AppendLine();
            usage.AppendLine("input     The tiff file to split.");
            usage.AppendLine("/d        Delete the input file after splitting.");
            usage.AppendLine("/o        Overwrite the output file if it exists.");
            usage.AppendLine("/ef       Log exceptions to a file.");
            usage.AppendLine("log       The file to which exceptions should be logged.");

            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK, 
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information, 
                MessageBoxDefaultButton.Button1, 0);
        }

        /// <summary>
        /// Splits the tiff files in the specified folder.
        /// </summary>
        /// <param name="codecs">Used to decode and encode the tiff files.</param>
        /// <param name="directory">The directory to search for tiff files.</param>
        /// <param name="overwrite"><see langword="true"/> if the output files should overwrite 
        /// existing files; <see langword="false"/> if an exception should be thrown if an output 
        /// file already exists.</param>
        /// <param name="delete"><see langword="true"/> if the input files should be deleted after 
        /// splitting; <see langword="false"/> if the input files should remain.</param>
        /// <param name="retainAnnotations"><see langword="true"/> if annotations in the input 
        /// image should be retained; <see langword="false"/> if the should not.</param>
        static void SplitFilesInFolder(ImageCodecs codecs, string directory, bool overwrite, bool delete, bool retainAnnotations)
        {
            foreach (string file in Directory.GetFiles(directory, "*.tif"))
            {
                SplitFile(codecs, file, overwrite, delete, retainAnnotations);
            }
        }

        /// <summary>
        /// Splits the pages of the specified tiff into individual one-page images.
        /// </summary>
        /// <param name="codecs">Used to decode and encode the tiff file.</param>
        /// <param name="input">The tiff file to split.</param>
        /// <param name="overwrite"><see langword="true"/> if the output files should overwrite 
        /// existing files; <see langword="false"/> if an exception should be thrown if an output 
        /// file already exists.</param>
        /// <param name="delete"><see langword="true"/> if the input files should be deleted after 
        /// splitting; <see langword="false"/> if the input files should remain.</param>
        /// <param name="retainAnnotations"><see langword="true"/> if annotations in the input 
        /// image should be retained; <see langword="false"/> if the should not.</param>
        static void SplitFile(ImageCodecs codecs, string input, bool overwrite, bool delete, bool retainAnnotations)
        {
            using (ImageReader reader = codecs.CreateReader(input))
            {
                // Check if the image is a PDF file
                if (ImageMethods.IsPdf(reader.Format))
                {
                    ExtractException ee = new ExtractException("ELI29689",
                        "This utility will not work on a PDF file.");
                    ee.AddDebugData("Input File", input, false);
                    throw ee;
                }

                // Get the full path of the input file without the extension
                string baseFileName = FileSystemMethods.GetFullPathWithoutExtension(input);

                // Get format for the extension of the output file
                string stringFormat = GetStringFormat(reader);

                using (var r = new LeadtoolsGuard())
                {
                    // Iterate over each page
                    for (int i = 1; i <= reader.PageCount; i++)
                    {
                        RasterImage image = reader.ReadPage(i);

                        // Retain annotations if necessary
                        RasterTagMetadata tag = null;
                        if (retainAnnotations)
                        {
                            tag = reader.ReadTagOnPage(i);
                        }

                        // Create the output file
                        string output = GetOutputFileName(baseFileName, i, stringFormat);
                        using (ImageWriter writer = codecs.CreateWriter(output, reader.Format, false))
                        {
                            writer.AppendImage(image);
                            if (tag != null)
                            {
                                writer.WriteTagOnPage(tag, 1);
                            }
                            writer.Commit(overwrite);
                        }
                    }
                }
            }

            // Delete the file if necessary
            if (delete)
            {
                FileSystemMethods.DeleteFile(input);
            }
        }

        /// <summary>
        /// Gets the format for the extension of the output image based on the number of pages in 
        /// the image.
        /// </summary>
        /// <param name="reader">The reader that is loading the image.</param>
        /// <returns>he format for the extension of the image being loaded by 
        /// <paramref name="reader"/>.</returns>
        static string GetStringFormat(ImageReader reader)
        {
            int digitCount = GetDigitCount(reader.PageCount);
            if (digitCount < 3)
            {
                digitCount = 3;
            }

            return "D" + digitCount.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the number of base 10 digits in the specified number.
        /// </summary>
        /// <param name="number">The number whose digits should be counted.</param>
        /// <returns>The number of base 10 digits in <paramref name="number"/>.</returns>
        static int GetDigitCount(int number)
        {
            return 1 + (int)Math.Log(number, 10.0);
        }

        /// <summary>
        /// Calculates the name of the output file for the specified page.
        /// </summary>
        /// <param name="baseFileName">The path of the input file without its extension.</param>
        /// <param name="pageNumber">The page number of this file.</param>
        /// <param name="format">The format to use for the extension.</param>
        /// <returns>The name of output file for <paramref name="pageNumber"/>.</returns>
        static string GetOutputFileName(string baseFileName, int pageNumber, string format)
        {
            return baseFileName + "." + pageNumber.ToString(format, CultureInfo.CurrentCulture);
        }
    }
}
