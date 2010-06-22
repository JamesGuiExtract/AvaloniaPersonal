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
            object missing = System.Reflection.Missing.Value;
            object oFalse = false;
            object oTrue = true;

            string exceptionFile = null;
            MSWord._Application word = null;
            MSExcel._Application excel = null;
            MSPowerPoint._Application pp = null;
            try
            {
                if (args.Length != 4)
                {
                    ExtractException ee = new ExtractException("ELI30263",
                        "Invalid number of arguments specified.");
                    ee.AddDebugData("Number Of Arguments", args.Length, false);
                    ee.AddDebugData("Number Of Arguments Expected", 4, false);
                    throw ee;
                }

                // Get the arguments
                string fileName = Path.GetFullPath(args[0]);
                OfficeApplication application = (OfficeApplication) Enum.Parse(
                    typeof(OfficeApplication), args[1]);
                string printerName = args[2];
                exceptionFile = Path.GetFullPath(args[3]);

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