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
                wordProcessor.LoadDocument(inputFile.FilePath, DevExpress.XtraRichEdit.DocumentFormat.Undefined);
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
                workbook.ExportToPdf(outputFile.FilePath, _exportOptions);

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53189");
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