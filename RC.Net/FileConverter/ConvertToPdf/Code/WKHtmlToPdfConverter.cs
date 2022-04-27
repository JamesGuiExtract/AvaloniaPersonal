using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Extract.FileConverter.ConvertToPdf
{
    /// <summary>
    /// An <see cref="IConvertFileToPdf"/> that uses wkhtmltopdf.exe
    /// </summary>
    public class WKHtmlToPdfConverter : IConvertFileToPdf
    {
        private static readonly string _WKHTMLTOPDF_EXE =
            Path.Combine(FileSystemMethods.CommonComponentsPath, "wkhtmltopdf.exe");

        /// <inheritdoc/>
        public IEnumerable<FileType> ConvertsFromFileTypes { get; } = new HashSet<FileType>
        {
            FileType.Html,
            FileType.Text,
        };

        /// <inheritdoc/>
        public bool Convert(FilePathHolder inputFile, PdfFile outputFile)
        {
            try
            {
                if (outputFile is PdfFile outputPdf)
                {
                    // Try to convert the file. Ignore errors when the input type is Unknown
                    return inputFile switch
                    {
                        TextFile inputFileWrapper => Convert(inputFileWrapper.FilePath, outputPdf.FilePath),
                        HtmlFile inputFileWrapper => Convert(inputFileWrapper.FilePath, outputPdf.FilePath),
                        _ => false
                    };
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53180");
            }
        }

        /// <summary>
        /// Create a <see cref="Dto.WKHtmlToPdfConverterV1"/>
        /// </summary>
        public DataTransferObjectWithType CreateDataTransferObject()
        {
            return new(new Dto.WKHtmlToPdfConverterV1());
        }

        // Attempt to convert the file
        private static bool Convert(string inputFile, string outputFile)
        {
            string[] args = new[] { "--log-level", "none", "--disable-javascript", "--disable-local-file-access", inputFile, outputFile };

            return SystemMethods.RunExecutable(_WKHTMLTOPDF_EXE, args, createNoWindow: true) == 0;
        }
    }
}

namespace Extract.FileConverter.ConvertToPdf.Dto
{
    /// <summary>
    /// DTO for <see cref="WKHtmlToPdfConverter"/>
    /// </summary>
    public class WKHtmlToPdfConverterV1 : IDataTransferObject
    {
        /// <summary>
        /// Create a <see cref="WKHtmlToPdfConverter"/> instance from this DTO
        /// </summary>
        public IDomainObject CreateDomainObject()
        {
            return new WKHtmlToPdfConverter();
        }
    }
}
