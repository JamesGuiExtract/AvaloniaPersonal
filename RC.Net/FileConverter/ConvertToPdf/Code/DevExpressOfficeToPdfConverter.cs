using DevExpress.Spreadsheet;
using DevExpress.XtraPrinting;
using DevExpress.XtraRichEdit;
using Extract.Utilities;
using System;
using System.Collections.Generic;

namespace Extract.FileConverter.ConvertToPdf
{
    /// <summary>
    /// An <see cref="IConvertFileToPdf"/> that uses DevExpress
    /// </summary>
    public sealed class DevExpressOfficeToPdfConverter : IConvertFileToPdf
    {
        static object sync = new();

        private static readonly PdfExportOptions _exportOptions = new()
        {
            // TODO: Copied from example, figure out what this means exactly...
            PdfUACompatibility = PdfUACompatibility.PdfUA1
        };

        /// <inheritdoc/>
        public IEnumerable<FileType> ConvertsFromFileTypes { get; } = new HashSet<FileType>
        {
            FileType.Text,
            FileType.Html,
            FileType.Word,
            FileType.Excel,
        };

        /// <inheritdoc/>
        public bool Convert(FilePathHolder inputFile, PdfFile outputFile)
        {
            if (outputFile is PdfFile outputPdf)
            {
                return inputFile switch
                {
                    TextFile inputFileWrapper => ConvertRichEditDocument(inputFileWrapper, outputPdf),
                    HtmlFile inputFileWrapper => ConvertRichEditDocument(inputFileWrapper, outputPdf),
                    WordFile inputFileWrapper => ConvertRichEditDocument(inputFileWrapper, outputPdf),
                    ExcelFile inputFileWrapper => ConvertExcelDocument(inputFileWrapper, outputPdf),
                    _ => false
                };
            }

            return false;
        }

        /// <summary>
        /// Create a <see cref="Dto.DevExpressOfficeToPdfConverterV1"/>
        /// </summary>
        public DataTransferObjectWithType CreateDataTransferObject()
        {
            return new(new Dto.DevExpressOfficeToPdfConverterV1());
        }

        // Convert a non-spreadsheet office document
        private static bool ConvertRichEditDocument(FilePathHolder inputFile, PdfFile outputFile)
        {
            try
            {
                using RichEditDocumentServer wordProcessor = new();
                lock (sync)
                {
                    wordProcessor.LoadDocument(inputFile.FilePath, DevExpress.XtraRichEdit.DocumentFormat.Undefined);
                }
                wordProcessor.ExportToPdf(outputFile.FilePath, _exportOptions);

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53188");
            }
        }

        // Convert a spreadsheet
        private static bool ConvertExcelDocument(ExcelFile inputFile, PdfFile outputFile)
        {
            try
            {
                using Workbook workbook = new();
                workbook.LoadDocument(inputFile.FilePath);
                AdjustWorksheetsToFitWidth(workbook);
                AdjustWorksheetsRemoveBadRows(workbook);
                workbook.ExportToPdf(outputFile.FilePath, _exportOptions);

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53189");
            }
        }

        // Prevent wide worksheets from being divided into multiple pages
        static void AdjustWorksheetsToFitWidth(Workbook workbook)
        {
            foreach (var worksheet in workbook.Worksheets)
            {
                worksheet.PrintOptions.FitToPage = true;
                worksheet.PrintOptions.FitToWidth = 1;
                worksheet.PrintOptions.FitToHeight = 0;
            }
        }

        // Prevent extremely large PDFs caused by a single row of content
        // https://extract.atlassian.net/browse/ISSUE-18046
        static void AdjustWorksheetsRemoveBadRows(Workbook workbook)
        {
            const int maxRows = 1_000_000; // Not quite the max supported by DevExpress
            const int maxReasonableRows = 500_000; // A lot of rows

            foreach (var worksheet in workbook.Worksheets)
            {
                var usedRange = worksheet.GetUsedRange();
                int rowCount = usedRange.RowCount;
                if (rowCount > maxRows)
                {
                    int lastRowIndex = usedRange.BottomRowIndex;
                    worksheet.Rows.Remove(lastRowIndex);

                    // If the range is now 'reasonable' then assume this delete is OK to do
                    if (worksheet.GetUsedRange().RowCount <= maxReasonableRows)
                    {
                        var ee = new ExtractException("ELI53254", "Application trace: Worksheet data deleted during conversion");
                        ee.AddDebugData("Worksheet name", worksheet.Name);
                        ee.AddDebugData("Index of deleted row", lastRowIndex);
                        ee.Log();
                    }
                    else // Otherwise fail the conversion
                    {
                        var ee = new ExtractException("ELI53255", "Number of rows exceeded the maximum");
                        ee.AddDebugData("Worksheet name", worksheet.Name);
                        ee.AddDebugData("Number of rows", usedRange.RowCount);
                        throw ee;
                    }
                }
            }
        }
    }
}

namespace Extract.FileConverter.ConvertToPdf.Dto
{
    /// <summary>
    /// DTO for <see cref="DevExpressOfficeToPdfConverter"/>
    /// </summary>
    public class DevExpressOfficeToPdfConverterV1 : IDataTransferObject
    {
        /// <summary>
        /// Create a <see cref="DevExpressOfficeToPdfConverter"/> instance from this DTO
        /// </summary>
        public IDomainObject CreateDomainObject()
        {
            return new DevExpressOfficeToPdfConverter();
        }
    }
}