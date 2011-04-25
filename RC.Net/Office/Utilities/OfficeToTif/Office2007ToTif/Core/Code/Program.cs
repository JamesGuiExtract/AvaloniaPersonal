using Extract.Imaging;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.IO;
using System.Windows.Forms;

using MSExcel = Microsoft.Office.Interop.Excel;
using MSPowerPoint = Microsoft.Office.Interop.PowerPoint;
using MSWord = Microsoft.Office.Interop.Word;
using MSTriState = Microsoft.Office.Core.MsoTriState;

namespace Extract.Office.Utilities.OfficeToTif.Office2007ToTif
{
    static class Program
    {
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
                var settings = OfficeMethods.ParseOfficeToTifApplicationArguments(args[0]);
                exceptionFile = settings.ExceptionFile;

                // Load and validate the license
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30287", Path.GetFileNameWithoutExtension(Application.ExecutablePath));

                tempPdf = new TemporaryFile(".pdf");
                string fileName = settings.OfficeDocumentName;
                switch (settings.OfficeApplication)
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
                                ref oFalse, ref missing, ref missing, ref missing,
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
                                missing, missing, missing, missing, false, missing, missing);

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
                ImageMethods.ConvertPdfToTif(tempPdf.FileName, settings.DestinationFileName);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI32432");
                if (!string.IsNullOrEmpty(exceptionFile))
                {
                    ee.Log(exceptionFile);
                }
                else
                {
                    ee.Display();
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