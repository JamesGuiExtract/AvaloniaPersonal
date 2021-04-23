using Extract.Imaging;
using Extract.Utilities;
using System;
using System.IO;

using MSExcel = Microsoft.Office.Interop.Excel;
using MSPowerPoint = Microsoft.Office.Interop.PowerPoint;
using MSTriState = Microsoft.Office.Core.MsoTriState;
using Extract.Office;
using System.Collections.ObjectModel;

namespace Extract.FileConverter.Converters
{
    sealed public class OfficeConverter : IConverter
    {
        /// <summary>
        /// Sets the supported destination file formats for this converter.
        /// </summary>
        public Collection<FileFormat> SupportedDestinationFormats => new Collection<FileFormat>() { FileFormat.Pdf, FileFormat.Tiff };

        /// <summary>
        /// Gets or sets the enabled flag for this converter. If the converter is disabled, it will not be run.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets the name of the converter.
        /// </summary>
        public string ConverterName { get => "Extract Office Converter"; }

        /// <summary>
        /// Executes the code to convert from one file type to another.
        /// </summary>
        /// <param name="inputFile">The fully qualified path to the file you wish to convert</param>
        /// <param name="destinationFileFormat">The destination file format</param>
        public void Convert(string inputFile, FileFormat destinationFileFormat)
        {
            TemporaryFile tempPdf = null;
            try
            {
                tempPdf = new TemporaryFile(".pdf", true);
                switch (GetOfficeApplicationFromFilename(inputFile))
                {
                    case OfficeApplication.Word:
                        ConvertWordDocument(inputFile, tempPdf);
                        break;
                    case OfficeApplication.Excel:
                        ConvertExcelDocument(inputFile, tempPdf);
                        break;

                    case OfficeApplication.PowerPoint:
                        ConvertPowerPoint(inputFile, tempPdf);
                        break;

                    default:
                        throw new ExtractException("ELI30264", "Unsupported office application.");
                }
                
                if(destinationFileFormat == FileFormat.Tiff)
                {
                    ImageMethods.ConvertPdfToTif(tempPdf.FileName, Path.ChangeExtension(inputFile, ".tiff"));
                }
                else
                {
                    File.Copy(tempPdf.FileName, Path.ChangeExtension(inputFile, ".pdf"));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51663");
            }
            finally
            {
                if (tempPdf != null)
                {
                    tempPdf.Dispose();
                }

                // This is recommended by MSDN to ensure that the office applications exit
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static void ConvertPowerPoint(string inputFile, TemporaryFile tempPdf)
        {
            // Open the presentation
            MSPowerPoint._Application pp = new MSPowerPoint.Application();

            MSPowerPoint._Presentation presentation = pp.Presentations.Open(
                inputFile, MSTriState.msoTrue, MSTriState.msoTrue,
                MSTriState.msoFalse);
            
            // Save as PDF
            presentation.SaveAs(tempPdf.FileName,
                MSPowerPoint.PpSaveAsFileType.ppSaveAsPDF, MSTriState.msoTrue);
            
            // Close the presentation
            presentation.Close();
            pp.Quit();
        }

        private static void ConvertWordDocument(string inputFile, TemporaryFile tempPdf)
        {
            // Create a new Microsoft Word application object
            Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application()
            {
                Visible = false,
                ScreenUpdating = false
            };

            Microsoft.Office.Interop.Word.Document doc = word.Documents.Open(FileName: inputFile);

            doc.Activate();

            // Save document into PDF Format
            doc.SaveAs(FileName: tempPdf.FileName, FileFormat: Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatPDF);
          
            doc.Close(SaveChanges: Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges);
            word.Quit();
        }

        private static void ConvertExcelDocument(string inputFile, TemporaryFile tempPdf)
        {
            object missing = System.Reflection.Missing.Value;
            MSExcel._Application excel = new MSExcel.Application()
            {
                Visible = false,
                ScreenUpdating = false
            };

            MSExcel._Workbook wb = excel.Workbooks.Open(inputFile,
                missing, missing, missing, missing, missing, missing, missing,
                missing, missing, missing, missing, false, missing, missing);

            // Save to pdf
            wb.ExportAsFixedFormat(MSExcel.XlFixedFormatType.xlTypePDF,
                tempPdf.FileName, MSExcel.XlFixedFormatQuality.xlQualityStandard,
                true, true, missing, missing, false, missing);

            // Close the workbook
            wb.Close(false, missing, missing);
            excel.Quit();
        }

        /// <summary>
        /// Attempts to determine the office application to use to print the file
        /// based on the file extension.
        /// </summary>
        /// <param name="fileName">The name of the file to be processed.</param>
        /// <returns>The office application to use to open the file.</returns>
        private static OfficeApplication GetOfficeApplicationFromFilename(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToUpperInvariant();

            if (extension.Contains("DOC") || extension.Contains("TXT")
                || extension.Contains("RTF") || extension.Contains("ODT")
                || extension.Contains("WPS"))
            {
                return OfficeApplication.Word;
            }
            else if (extension.Contains("XLS") || extension.Contains("CSV") ||
                extension.Contains("ODS") || extension.Contains("PRN"))
            {
                return OfficeApplication.Excel;
            }
            else if (extension.Contains("PPT") || extension.Contains("PPS")
                || extension.Contains("ODP"))
            {
                return OfficeApplication.PowerPoint;
            }
            else
            {
                return OfficeApplication.Unknown;
            }
        }

        public IConverter Clone()
        {
            return new OfficeConverter() { IsEnabled = this.IsEnabled };
        }
    }
}
