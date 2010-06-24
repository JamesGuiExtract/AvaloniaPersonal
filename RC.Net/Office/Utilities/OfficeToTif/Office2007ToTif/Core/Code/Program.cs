using Extract.Licensing;
using Extract.Office;
using Extract.Utilities;
using Microsoft.Office.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using MSWord = Microsoft.Office.Interop.Word;
using MSExcel = Microsoft.Office.Interop.Excel;
using MSPowerPoint = Microsoft.Office.Interop.PowerPoint;
using MSTriState = Microsoft.Office.Core.MsoTriState;

namespace Extract.Office.Utilities.OfficeToTif.Office2007ToTif
{
    static class Program
    {
        /// <summary>
        /// The path to the image format converter
        /// </summary>
        static readonly string _IMAGE_CONVERTER = Path.Combine(
            Path.GetDirectoryName(Application.ExecutablePath), "imageformatconverter.exe");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Constants needed to pass empty and missing arguments to some of the
            // office COM methods
            object missing = System.Reflection.Missing.Value;
            object oFalse = false;
            object oTrue = true;

            string exceptionFile = null;
            MSWord._Application word = null;
            MSExcel._Application excel = null;
            MSPowerPoint._Application pp = null;
            TemporaryFile tempPdf = null;
            try
            {
                // Get the arguments from the file
                string[] args2 =
                    args.Length == 1 ? File.ReadAllLines(Path.GetFullPath(args[0])) : null;
                
                // Ensure there is the proper number of arguments and that the
                // fifth argument matches the LicenseUtilities.MapLabelValue
                if (args2 == null || args2.Length != 4)
                {
                    ExtractException ee = new ExtractException("ELI30263",
                        "Invalid command line.");
                    throw ee;
                }

                // Load and validate the license
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30287", Path.GetFileNameWithoutExtension(Application.ExecutablePath));

                // Get the arguments
                // 1. File name
                // 2. The name of the destination file
                // 3. The office application value (from the OfficeApplication enum)
                // 4. The exception file to log exceptions to if an exception occurs
                string fileName = Path.GetFullPath(args2[0]);
                string outputFile = Path.GetFullPath(args2[1]);
                OfficeApplication application = (OfficeApplication) Enum.Parse(
                    typeof(OfficeApplication), args2[2]);
                exceptionFile = Path.GetFullPath(args2[3]);
                tempPdf = new TemporaryFile(".pdf");

                switch (application)
                {
                    case OfficeApplication.Word:
                        {
                            // Open the file
                            object fileToOpen = fileName;
                            word = new MSWord.Application();
                            MSWord._Document doc = word.Documents.Open(ref fileToOpen,
                                ref oFalse, ref oTrue, ref oFalse, ref missing, ref missing,
                                ref oFalse, ref missing, ref missing, ref missing, ref missing,
                                ref oFalse, ref missing, ref missing, ref missing, ref missing);

                            // Save file to pdf
                            object outFile = tempPdf.FileName;
                            object fileFormat = MSWord.WdSaveFormat.wdFormatPDF;
                            doc.SaveAs(ref outFile, ref fileFormat, ref missing, ref missing,
                                ref missing, ref missing, ref missing, ref missing,
                                ref missing, ref missing, ref missing, ref missing,
                                ref missing, ref missing, ref missing, ref missing);

                            // Close the document
                            object saveChanges = MSWord.WdSaveOptions.wdDoNotSaveChanges;
                            doc.Close(ref saveChanges, ref missing, ref missing);
                            doc = null;
                        }
                        break;
                    case OfficeApplication.Excel:
                        {
                            // Open the workbook
                            excel = new MSExcel.Application();
                            MSExcel._Workbook wb = excel.Workbooks.Open(fileName,
                                missing, missing, missing, missing, missing, missing, missing,
                                missing, missing, missing, missing, missing, missing, missing);

                            // Save to pdf
                            wb.ExportAsFixedFormat(MSExcel.XlFixedFormatType.xlTypePDF,
                                tempPdf.FileName, MSExcel.XlFixedFormatQuality.xlQualityStandard,
                                true, true, missing, missing, false, missing);

                            // Close the workbook
                            wb.Close(oFalse, missing, missing);
                            wb = null;
                        }
                        break;

                    case OfficeApplication.PowerPoint:
                        {
                            // Open the presentation
                            pp = new MSPowerPoint.Application();
                            MSPowerPoint._Presentation presentation = pp.Presentations.Open(
                                fileName, MSTriState.msoTrue, MSTriState.msoTrue,
                                MSTriState.msoFalse);

                            // Save as PDF
                            presentation.SaveAs(tempPdf.FileName,
                                MSPowerPoint.PpSaveAsFileType.ppSaveAsPDF, MSTriState.msoTrue);

                            // Close the presentation
                            presentation.Close();
                            presentation = null;
                        }
                        break;

                    default:
                        throw new ExtractException("ELI30264", "Unsupported office application.");
                }

                // Now convert the temporary PDF to a tif and copy to the output file
                using (TemporaryFile tempUex = new TemporaryFile(".uex"))
                using (Process process = new Process())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append('"');
                    sb.Append(tempPdf.FileName);
                    sb.Append("\" \"");
                    sb.Append(outputFile);
                    sb.Append("\" /tif /ef \"");
                    sb.Append(tempUex.FileName);
                    sb.Append('"');
                    process.StartInfo.FileName = _IMAGE_CONVERTER;
                    process.StartInfo.Arguments = sb.ToString();
                    process.Start();
                    process.WaitForExit();

                    FileInfo info = new FileInfo(tempUex.FileName);
                    if (info.Length > 0)
                    {
                        throw ExtractException.LoadFromFile("ELI30286", tempUex.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(exceptionFile))
                {
                    ExtractException.Log(exceptionFile, "ELI30265", ex);
                }
                else
                {
                    ExtractException.Display("ELI30266", ex);
                }
            }
            finally
            {
                // Ensure any application we started is exited
                if (word != null)
                {
                    word.Quit(ref oFalse, ref missing, ref missing);
                    word = null;
                }
                if (excel != null)
                {
                    excel.Quit();
                    excel = null;
                }
                if (pp != null)
                {
                    pp.Quit();
                    pp = null;
                }
                if (tempPdf != null)
                {
                    tempPdf.Dispose();
                    tempPdf = null;
                }

                // This is recommended by MSDN to ensure that the office applications exit
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}