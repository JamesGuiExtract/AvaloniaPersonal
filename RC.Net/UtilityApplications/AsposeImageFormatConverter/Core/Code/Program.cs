using Aspose.Pdf;
using Aspose.Pdf.Devices;
using Aspose.Pdf.Facades;
using Extract;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace AsposeImageFormatConverter
{
    class Program
    {
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            string exceptionFile = null;

            try
            {
                int argumentCount = args.Length;

                if (argumentCount == 1
                    && args[0].Equals("/?", StringComparison.OrdinalIgnoreCase))
                {
                    DisplayUsage(null);
                    return;
                }

                if (argumentCount < 2)
                {
                    DisplayUsage("Incorrect number of arguments.");
                    return;
                }

                // Load the license for the Aspose PDF API
                License license = new License();
                license.SetLicense(Path.Combine(Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location),
                        "Aspose.Pdf.lic"));

                string pdfSource = Path.GetFullPath(args[0]);
                string tifDest = Path.GetFullPath(args[1]);

                Aspose.Pdf.Devices.ColorDepth colorDepth = Aspose.Pdf.Devices.ColorDepth.Default;
                bool overwrite = false;

                for (int i = 2; i < argumentCount; i++)
                {
                    string temp = args[i];

                    if (temp.Equals("/1bpp", StringComparison.OrdinalIgnoreCase))
                    {
                        if (colorDepth != Aspose.Pdf.Devices.ColorDepth.Default)
                        {
                            DisplayUsage("Conflicting color depths specified.");
                            return;
                        }
                        colorDepth = Aspose.Pdf.Devices.ColorDepth.Format1bpp;
                    }
                    else if (temp.Equals("/4bpp", StringComparison.OrdinalIgnoreCase))
                    {
                        if (colorDepth != Aspose.Pdf.Devices.ColorDepth.Default)
                        {
                            DisplayUsage("Conflicting color depths specified.");
                            return;
                        }
                        colorDepth = Aspose.Pdf.Devices.ColorDepth.Format4bpp;
                    }
                    else if (temp.Equals("/8bpp", StringComparison.OrdinalIgnoreCase))
                    {
                        if (colorDepth != Aspose.Pdf.Devices.ColorDepth.Default)
                        {
                            DisplayUsage("Conflicting color depths specified.");
                            return;
                        }
                        colorDepth = Aspose.Pdf.Devices.ColorDepth.Format8bpp;
                    }
                    else if (temp.Equals("/o", StringComparison.OrdinalIgnoreCase))
                    {
                        overwrite = true;
                    }
                    else if (temp.Equals("/ef", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((++i) >= argumentCount)
                        {
                            DisplayUsage("No exception file specified.");
                            return;
                        }

                        exceptionFile = Path.GetFullPath(args[i]);
                    }

                    else
                    {
                        DisplayUsage("Unrecognized argument: " + temp);
                        return;
                    }
                }

                if (!overwrite && File.Exists(tifDest))
                {
                    var ee = new ExtractException("ELI37837", "Destination file already exists.");
                    ee.AddDebugData("Destination File", tifDest, false);
                    throw ee;
                }

                using (Document pdfDocument = new Document(pdfSource))
                using (PdfConverter converter = new PdfConverter(pdfDocument))
                {
                    converter.Resolution = new Resolution(300);

                    TiffSettings tiffSettings = new TiffSettings();
                    tiffSettings.Depth = colorDepth;
                    tiffSettings.Compression = CompressionType.LZW;
                    tiffSettings.SkipBlankPages = false;

                    converter.SaveAsTIFF(tifDest, tiffSettings);
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(exceptionFile))
                {
                    ex.ExtractLog("ELI37838", exceptionFile);
                }
                else
                {
                    ex.ExtractDisplay("ELI37839");
                }
            }
        }

        /// <summary>
        /// Displays the usage.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        static void DisplayUsage(string errorMessage)
        {
            bool error = !string.IsNullOrWhiteSpace(errorMessage);

            var sb = new StringBuilder(error ? errorMessage : "", 1024);
            if (error)
            {
                sb.AppendLine();
                sb.AppendLine();
            }
            sb.Append(Environment.GetCommandLineArgs()[0]);
            sb.AppendLine(" <PDFSource> <TifDestination> [/1bpp | /4bpp | /8bpp] " +
                "[/o] [/ef <ExceptionFile>]");
            sb.AppendLine();
            sb.AppendLine("Usage:");
            sb.AppendLine("-------------------");
            sb.AppendLine("<PDFSource>: The source pdf file.");
            sb.AppendLine("<TifDestination>: The destination tif file.");
            sb.AppendLine("/1bpp, /4bpp, /8bpp: Specifies the color depth of the output " +
                "should be limited to 1, 4 or 8 bits per pixel.");
            sb.AppendLine("/o: Will overwrite the destination file if it exists.");
            sb.AppendLine("/ef <ExceptionFile>: Log any exceptions to the specified");
            sb.AppendLine("    file rather than display them.");

            MessageBox.Show(sb.ToString(), error ? "Error" : "Usage", MessageBoxButtons.OK, 
                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
        }
    }
}
