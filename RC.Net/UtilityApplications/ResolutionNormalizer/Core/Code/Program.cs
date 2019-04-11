using Extract;
using Extract.Imaging.Utilities;
using Extract.Licensing;
using Leadtools;
using Leadtools.Codecs;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ResolutionNormalizer
{
    /// <summary>
    /// https://extract.atlassian.net/browse/ISSUE-13270
    /// This utility application is used to adjust the resolution of image pages where one axis
    /// (horizontal or vertical) has a higher DPI than the other dimension such that both dimensions
    /// use the same DPI in the output. This can be used as a workaround to Extract image viewer
    /// not taking DPI into account when displaying these pages and can also improve OCR output.
    /// </summary>
    static class Program
    {
        #region Constants

        /// <summary>
        /// Default display depth to use for PDF files.
        /// </summary>
        static readonly int _DEFAULT_PDF_DISPLAY_DEPTH = 24;

        /// <summary>
        /// Default resolution to use for PDF files.
        /// </summary>
        static readonly int _DEFAULT_PDF_RESOLUTION = 300;

        /// <summary>
        /// The registry sub key path for the file access retry values (this key is under HKLM)
        /// </summary>
        static readonly string _FILE_ACCESS_KEY =
            @"Software\Extract Systems\ReusableComponents\BaseUtils";

        /// <summary>
        /// The number of times to retry a file access operation if the operation
        /// fails due to a sharing violation.
        /// </summary>
        static int _fileAccessRetries = -1;

        /// <summary>
        /// The amount of time to sleep between file access retries.
        /// </summary>
        static int _fileAccessRetrySleepTime = -1;

        /// <summary>
        /// Mutex used to prevent multiple threads from trying to update the file access
        /// retry values.
        /// </summary>
        static object _fileAccessLock = new object();

        #endregion Constants

        #region Structs

        /// <summary>
        /// Represents the arguments that can be dictated via the command-line.
        /// </summary>
        struct Arguments
        {
            /// <summary>
            /// The filename of the document to normalize.
            /// </summary>
            public string FileName;

            /// <summary>
            /// The number of times greater the resolution of one axis must be over the other axis
            /// before corrective action is taken on a given page.
            /// </summary>
            public double ResolutionFactor;

            /// <summary>
            /// The name of a uex file where any exceptions encountered should be output. If not
            /// specified, any encountered exceptions will be displayed.
            /// </summary>
            public string ExceptionFileName;
        }

        #endregion Structs

        #region Main

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Load license files from folder
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI38890", "Resolution Normalizer");

                UnlockLeadtools.UnlockLeadToolsSupport();

                Arguments? arguments = ParseArguments(args);
                if (!arguments.HasValue)
                {
                    // If valid command-line arguments were not specified, return without processing.
                    return;
                }

                try
                {
                    NormalizeResolution(arguments.Value);
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI38891",
                        "Failed to normalize document.", ex);
                    ee.AddDebugData("Filename", arguments.Value.FileName, false);

                    if (!string.IsNullOrEmpty(arguments.Value.ExceptionFileName))
                    {
                        ee.Log(arguments.Value.ExceptionFileName);
                    }
                    else
                    {
                        ee.Display();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38892");
            }
        }

        #endregion Main

        #region Private Members

        /// <summary>
        /// Normalizes the resolution of image pages in a document.
        /// </summary>
        /// <param name="arguments">The <see cref="Arguments"/> specifying the resolution
        /// normalization operation to perform.</param>
        static void NormalizeResolution(Arguments arguments)
        {
            try
            {
                ExtractException.Assert("ELI38898", "Filename does not exist.",
                    File.Exists(arguments.FileName), "Filename", arguments.FileName);

                using (RasterCodecs codecs = GetCodecs())
                {
                    int pageCount;
                    using (CodecsImageInfo info = codecs.GetInformation(arguments.FileName, true))
                    {
                        pageCount = info.TotalPages;
                    }

                    // Normalize the resolution of each pages as needed.
                    for (int i = 1; i <= pageCount; i++)
                    {
                        NormalizePageResolution(
                            codecs, arguments.FileName, i, arguments.ResolutionFactor);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38893");
            }
        }

        /// <summary>
        /// Normalizes the resolution of the specified <see paramref="page"/> if the resolution of
        /// the axes differs by more than <see paramref="resolutionFactor"/>.
        /// </summary>
        /// <param name="codecs">The <see cref="RasterCodecs"/> to use for the operation.</param>
        /// <param name="fileName">Name of the file being normalized.</param>
        /// <param name="page">The page number for the operation.</param>
        /// <param name="resolutionFactor">The number of times greater the resolution of one axis
        /// must be over the other axis before corrective action is taken on a given page.</param>
        /// <returns></returns>
        static void NormalizePageResolution(RasterCodecs codecs, string fileName, int page,
            double resolutionFactor)
        {
            try
            {
                using (CodecsImageInfo info = codecs.GetInformation(fileName, false, page))
                {
                    RasterImage image = null;
                    var maxResolution = Math.Max(info.XResolution, info.YResolution);
                    var minResolution = Math.Min(info.XResolution, info.YResolution);

                    if (((double)maxResolution / (double)minResolution) > resolutionFactor)
                    {
                        // Calculate the necessary width/height of the image page if both axis are
                        // to have maxResolution.
                        int width = info.Width * (maxResolution / info.XResolution);
                        int height = info.Height * (maxResolution / info.YResolution);

                        // Load the image page scaled according to the required dimensions calculated.
                        PerformFileOperationWithRetry(() =>
                            image = codecs.Load(fileName, width, height, 0,
                            RasterSizeFlags.Bicubic, CodecsLoadByteOrder.BgrOrGray, page, page),
                                false);

                        // If X/YResolution is not set, though the resulting image will have the
                        // normalized dimensions, some image utilities may still report the original
                        // dimensions.
                        image.XResolution = maxResolution;
                        image.YResolution = maxResolution;

                        // Replace the original image page with the normalized page.
                        PerformFileOperationWithRetry(() =>
                            codecs.Save(image, fileName, info.Format, info.BitsPerPixel, 1, 1, page,
                                CodecsSavePageMode.Replace),
                                false);
                    }
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI38894");
                ee.AddDebugData("Page", page, false);
                throw ee;
            }
        }

        /// <summary>
        /// Parses the program arguments.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>An <see cref="Arguments"/> instance representing the passed in arguments or
        /// <see langword="null"/> if the arguments could not be correctly parsed.</returns>
        static Arguments? ParseArguments(string[] args)
        {
            var arguments = new Arguments();
            arguments.ResolutionFactor = 1.5;

            // Parse the command-line arguments.
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg.Equals("/?", StringComparison.OrdinalIgnoreCase))
                {
                    ShowUsage();
                    return null;
                }
                else if (arg.Equals("/ef", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        ShowUsage("Log filename expected.");
                        return null;
                    }

                    arguments.ExceptionFileName = Path.GetFullPath(args[i]);
                }
                else if (i == 0)
                {
                    // Get the fully qualified path to the file
                    arguments.FileName = Path.GetFullPath(arg);
                }
                else if (i == 1)
                {
                    double resolutionFactor;
                    if (!double.TryParse(arg, out resolutionFactor))
                    {
                        ShowUsage("Unable to parse resolution factor: \"{0}\".", arg);
                        return null;
                    }
                    else if (resolutionFactor < 1)
                    {
                        ShowUsage("Resolution factor must be >= 1: \"{0}\".", arg);
                        return null;
                    }

                    arguments.ResolutionFactor = resolutionFactor;
                }
                else
                {
                    ShowUsage("Unrecognized option: \"" + arg + "\"");
                    return null;
                }
            }

            if (string.IsNullOrWhiteSpace(arguments.FileName))
            {
                ShowUsage("Filename not specified.");
                return null;
            }

            return arguments;
        }

        /// <summary>
        /// Displays a usage message to the user
        /// </summary>
        /// <param name="errorInfo">If specified, describes a problem with the specified
        /// command-line arguments.</param>
        static void ShowUsage(params string[] errorInfo)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(errorInfo.Length > 0
                ? string.Format(CultureInfo.CurrentCulture, errorInfo.First(),
                    errorInfo.Skip(1).ToArray())
                : "Usage:");
            sb.AppendLine("------------");
            sb.AppendLine("ResolutionNormalizer.exe <Filename> [<ResolutionFactor>] [/ef <ExceptionFile>]");
            sb.AppendLine("Filename: Name of image file where disproportional image resolutions");
            sb.AppendLine("     (where one axis has a higher DPI than the other) will be normalized");
            sb.AppendLine("     such that both the horizontal and vertical DPI ends up matching the");
            sb.AppendLine("     the greater of the two on the input image. The image will be");
            sb.AppendLine("     modified in-place.");
            sb.AppendLine("ResolutionFactor: A floating point value indicating how many times");
            sb.AppendLine("     greater one axis's DPI must be than the other before the page DPI is");
            sb.AppendLine("     normalized. If not specified, the default is 1.5.");
            sb.AppendLine("/ef <ExceptionFile>: Log exceptions to the specified file rather than");
            sb.AppendLine("     display them");

            MessageBox.Show(sb.ToString(), "ResolutionNormalizer Usage", MessageBoxButtons.OK,
                (errorInfo.Length == 0) ? MessageBoxIcon.Information : MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, 0);
        }

        /// <summary>
        /// Gets a properly initialized <see cref="RasterCodecs"/> instance to use for resolution
        /// normalization operations.
        /// </summary>
        /// <returns>A properly initialized <see cref="RasterCodecs"/> instance.</returns>
        static RasterCodecs GetCodecs()
        {
            RasterCodecs codecs = new RasterCodecs();

            // The same options used by our other applications.
            codecs.Options.Pdf.Save.UseImageResolution = true;
            codecs.Options.Tiff.Load.IgnoreViewPerspective = true;
            codecs.Options.Pdf.Load.DisplayDepth = _DEFAULT_PDF_DISPLAY_DEPTH;
            codecs.Options.RasterizeDocument.Load.XResolution = _DEFAULT_PDF_RESOLUTION;
            codecs.Options.RasterizeDocument.Load.YResolution = _DEFAULT_PDF_RESOLUTION;

            return codecs;
        }

        /// <summary>
        /// Performs the specified file operation within a looping structure to retry a failed file
        /// operation.
        /// <para><b>NOTE</b></para>
        /// This is a copy of the Extract.Utilities.FileSystemMethods method, but it has been copied
        /// here to keep this utility's dependencies to a minimum.
        /// </summary>
        /// <param name="onlyOnSharingViolation"><see langword="true"/> if the retries should be
        /// performed only in the case of a sharing violation; <see langword="false"/> if retries
        /// should be performed no matter the exception.</param>
        /// <param name="fileOperation">The operation to perform.</param>
        static void PerformFileOperationWithRetry(Action fileOperation,
            bool onlyOnSharingViolation)
        {
            try
            {
                var retryCountAndSleepTime = GetRetryCountAndSleepTime();
                int maxAttempts = retryCountAndSleepTime.Item1;
                int sleepTime = retryCountAndSleepTime.Item2;
                int attempts = 1;
                do
                {
                    try
                    {
                        fileOperation();
                        break;
                    }
                    // Changed from IOException because LeadTools will produce exceptions that are
                    // not IOExceptions despite a problem relating to accessing a file.
                    catch (Exception ex)
                    {
                        // https://extract.atlassian.net/browse/ISSUE-11972
                        // Allow for retries for errors other than a sharing violation if
                        // onlyOnSharingViolation is false and there is a windows error code
                        // associated with the exception.
                        if ((onlyOnSharingViolation &&
                                ex.GetWindowsErrorCode() != Win32ErrorCode.SharingViolation) ||
                                ex.GetWindowsErrorCode() == Win32ErrorCode.Success)
                        {
                            throw ex.AsExtract("ELI38895");
                        }
                        else if (attempts >= maxAttempts)
                        {
                            var ee = new ExtractException("ELI38896",
                                "File operation failed after retries.", ex);
                            ee.AddDebugData("Number Of Attempts", attempts, false);
                            ee.AddDebugData("Max Number Of Attempts", maxAttempts, false);
                            throw ee;
                        }
                    }

                    attempts++;
                    Thread.Sleep(sleepTime);
                }
                while (true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38897");
            }
        }

        /// <summary>
        /// Gets the count of retries and the time to sleep in between attempts for
        /// file access sharing violations.
        /// </summary>
        /// <returns>The retry count and sleep time.</returns>
        static Tuple<int, int> GetRetryCountAndSleepTime()
        {
            if (_fileAccessRetries == -1 || _fileAccessRetrySleepTime == -1)
            {
                lock (_fileAccessLock)
                {
                    if (_fileAccessRetries == -1)
                    {
                        // Get the values from the registry
                        var key = Registry.LocalMachine.OpenSubKey(_FILE_ACCESS_KEY);
                        if (key == null)
                        {
                            // [ISSUE-11999]
                            // If _FILE_ACCESS_KEY is missing, use default values. Otherwise
                            // unprivileged users will get exceptions if administrator users haven't
                            // already run applications that generated the key.
                            _fileAccessRetries = 50;
                            _fileAccessRetrySleepTime = 250;
                        }
                        else
                        {
                            var accessRetries = key.GetValue("FileAccessRetries", "50").ToString();
                            _fileAccessRetries = int.Parse(accessRetries, CultureInfo.InvariantCulture);
                            var sleepTime = key.GetValue("FileAccessTimeout", "250").ToString();
                            _fileAccessRetrySleepTime = int.Parse(sleepTime, CultureInfo.InvariantCulture);
                        }
                    }
                }
            }

            return new Tuple<int, int>(_fileAccessRetries, _fileAccessRetrySleepTime);
        }

        #endregion Private Members
    }
}
