using Extract.Encryption;
using Extract.Licensing;
using Extract.Office.Utilities.OfficeToTif;
using Microsoft.Office.Interop;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using MSWord = Microsoft.Office.Interop.Word;
using MSExcel = Microsoft.Office.Interop.Excel;
using MSPowerPoint = Microsoft.Office.Interop.PowerPoint;
using MSTriState = Microsoft.Office.Core.MsoTriState;

namespace Extract.Utilities.Office.OfficeToTif.Office2007ToTif
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
            try
            {
                // License map code
                string mapCode = LicenseUtilities.GetMapLabelValue(new MapLabel());

                // Get the arguments from the file
                string[] args2 =
                    args.Length == 1 ? File.ReadAllLines(Path.GetFullPath(args[0])) : null;
                
                // Ensure there is the proper number of arguments and that the
                // fifth argument matches the LicenseUtilities.MapLabelValue
                if (args2 == null
                    || !mapCode.Equals(ExtractEncryption.DecryptString(args2[4], new MapLabel()),
                    StringComparison.Ordinal))
                {
                    ExtractException ee = new ExtractException("ELI30263",
                        "Invalid command line.");
                    throw ee;
                }

                // Get the arguments
                // 1. File name
                // 2. The office application value (from the OfficeApplication enum)
                // 3. The name of the printer (formatted properly for the office application)
                // 4. The exception file to log exceptions to if an exception occurs
                string fileName = Path.GetFullPath(args2[0]);
                OfficeApplication application = (OfficeApplication) Enum.Parse(
                    typeof(OfficeApplication), args2[1]);
                string printerName = args2[2];
                exceptionFile = Path.GetFullPath(args2[3]);

                switch (application)
                {
                    case OfficeApplication.Word:
                        {
                            object fileToOpen = fileName;
                            word = new MSWord.Application();
                            word.Options.PrintBackground = false;
                            word.ActivePrinter = printerName;
                            MSWord._Document doc = word.Documents.Open(ref fileToOpen,
                                ref oFalse, ref oTrue, ref oFalse, ref missing, ref missing,
                                ref oFalse, ref missing, ref missing, ref missing, ref missing,
                                ref oFalse, ref missing, ref missing, ref missing, ref missing);
                            doc.Activate();
                            word.PrintOut(ref missing, ref missing, ref missing, ref missing,
                                ref missing, ref missing, ref missing, ref missing, ref missing,
                                ref missing, ref missing, ref missing, ref missing, ref missing,
                                ref missing, ref missing, ref missing, ref missing, ref missing);
                            doc.Close(ref oFalse, ref missing, ref missing);
                        }
                        break;
                    case OfficeApplication.Excel:
                        {
                            excel = new MSExcel.Application();
                            MSExcel._Workbook wb = excel.Workbooks.Open(fileName,
                                missing, missing, missing, missing, missing, missing, missing,
                                missing, missing, missing, missing, missing, missing, missing);
                            wb.Activate();

                            // If we desire to ensure grid lines are printed then uncomment
                            // these lines
                            //for (int i = 1; i <= wb.Worksheets.Count; i++)
                            //{
                            //    // Ensure gridlines are printed
                            //    MSExcel.Worksheet sheet = (MSExcel.Worksheet)wb.Worksheets[i];
                            //    sheet.PageSetup.PrintGridlines = true;
                            //}

                            excel.ActivePrinter = printerName;
                            wb.PrintOut(missing, missing, missing, missing, missing, missing,
                                missing, missing);
                            wb.Close(oFalse, missing, missing);
                        }
                        break;

                    case OfficeApplication.PowerPoint:
                        {
                            pp = new MSPowerPoint.Application();
                            MSPowerPoint._Presentation presentation = pp.Presentations.Open(
                                fileName, MSTriState.msoTrue, MSTriState.msoTrue,
                                MSTriState.msoFalse);
                            presentation.PrintOptions.ActivePrinter = printerName;
                            presentation.PrintOptions.FitToPage = MSTriState.msoTrue;
                            presentation.PrintOptions.OutputType =
                                MSPowerPoint.PpPrintOutputType.ppPrintOutputSlides;
                            presentation.PrintOptions.PrintInBackground = MSTriState.msoFalse;
                            presentation.PrintOut(0, presentation.Slides.Count, "", 1,
                                MSTriState.msoTrue);
                            presentation.Close();
                        }
                        break;

                    default:
                        throw new ExtractException("ELI30264", "Unknown office application.");
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

                // This is recommended by MSDN to ensure that the office applications exit
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}