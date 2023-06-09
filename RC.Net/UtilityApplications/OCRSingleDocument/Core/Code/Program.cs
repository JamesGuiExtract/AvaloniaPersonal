using Extract;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_SSOCRLib;

namespace Extract.OcrSingleDocument
{
    class Program
    {
        /// <summary>
        /// The main application for OCRSingleDocument
        /// </summary>
        /// <param name="args">Command line arguments for application.</param>
        [STAThread]
        static void Main(string[] args)
        {
            string fileName = null;
            string exceptionFile = null;
            string paramsFile = null;
            try
            {
                // Check the number of command line arguments
                if (args.Length != 1 && args.Length != 3 && args.Length != 5)
                {
                    ShowUsage();
                    return;
                }

                int i = 0;
                for (; i < args.Length - 1; i++)
                {
                    if (exceptionFile == null
                        &&(args[i].Equals("/ef", StringComparison.OrdinalIgnoreCase)
                        || args[i].Equals("-ef", StringComparison.OrdinalIgnoreCase)))
                    {
                        exceptionFile = Path.GetFullPath(args[++i]);
                    }
                    else if (paramsFile == null
                        &&(args[i].Equals("/params", StringComparison.OrdinalIgnoreCase)
                        || args[i].Equals("-params", StringComparison.OrdinalIgnoreCase)))
                    {
                        paramsFile = Path.GetFullPath(args[++i]);
                    }
                    else if (fileName == null)
                    {
                        fileName = Path.GetFullPath(args[i]);
                    }
                    else
                    {
                        ShowUsage();
                        return;
                    }
                }

                if (i == args.Length - 1)
                {
                    if (fileName == null)
                    {
                        fileName = Path.GetFullPath(args[i]);
                    }
                    else
                    {
                        ShowUsage();
                        return;
                    }
                }

                // Ensure the file exists
                ExtractException.Assert("ELI22004", "File does not exist!",
                    File.Exists(fileName), "File Name", fileName);

                // Load license files from folder
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                // Validate the license
                // NOTE: This is licensed as an RDT object for internal use,
                // if it is deemed useful to ship to customers an alternative license
                // ID would be LicenseIdName.OcrOnServerFeature 
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleDevelopmentToolkitObjects,
                    "ELI22005", "OCR Single Document utility");

                // Create the OCR COM object
                ScansoftOCRClass ssocr = new ScansoftOCRClass();

                // Initlialize the private license
                ssocr.InitPrivateLicense(
                    LicenseUtilities.GetMapLabelValue(new MapLabel()));

                // Load the OCR parameters from the file
                IOCRParameters ocrParameters = null;
                if (!string.IsNullOrEmpty(paramsFile))
                {
                    ILoadOCRParameters loadOCRParameters = new RuleSetClass();
                    loadOCRParameters.LoadOCRParameters(paramsFile);
                    ocrParameters = ((IHasOCRParameters)loadOCRParameters).OCRParameters;
                }

                // OCR the document
                SpatialString ocrText = ssocr.RecognizeTextInImage(fileName, 1, -1,
                    EFilterCharacters.kNoFilter, "", EOcrTradeOff.kAccurate, true, null, ocrParameters);

                // Save the OCR output as filename.uss
                ocrText.SaveTo(fileName + ".uss", true, true);
            }
            catch (Exception ex)
            {
                // Wrap the exception as an extract exception
                ExtractException ee = new ExtractException("ELI22003",
                    "Failed to OCR document!", ex);

                // Add the file name as debug data
                ee.AddDebugData("OCR File Name", fileName, false);

                // If logExceptions is true, just log otherwise display the exception
                if (!string.IsNullOrEmpty(exceptionFile))
                {
                    // Log the exception
                    ee.Log(exceptionFile);
                }
                else
                {
                    // Display the exception to the user
                    ee.Display();
                }
            }
        }

        /// <summary>
        /// Displays a usage message to the user
        /// </summary>
        private static void ShowUsage()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Usage:");
            sb.AppendLine("------------");
            sb.AppendLine("OcrSingleDocument.exe <FileName> [/ef <ExceptionFile>] [/params <RuleSetFile>]");
            sb.AppendLine("FileName: Name of file to OCR");
            sb.Append("/ef <ExceptionFile>: Log exceptions to the specified file rather than");
            sb.AppendLine(" display them");
            sb.Append("/params <RuleSetFile>: Load OCR parameters from the specified file");
            
            MessageBox.Show(sb.ToString(), "OCRSingleDocument Usage", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);
        }
    }
}
