using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Extract.FileConverter.ConvertToPdf
{
    /// <summary>
    /// An <see cref="IConvertFileToPdf"/> that uses Leadtools via ImageFormatConverter.exe
    /// </summary>
    public sealed class LeadToolsImageToPdfConverter : IConvertFileToPdf
    {
        static readonly string _IMAGE_FORMAT_CONVERTER_EXE =
            Path.Combine(FileSystemMethods.CommonComponentsPath, "ImageFormatConverter.exe");

        // Keep track of whether an exception has been caught/logged when PDF support is unlicensed
        // to avoid wasting CPU running ImageFormatConverter when it would only fail
        bool _licenseExceptionLogged;

        /// <inheritdoc/>
        public IEnumerable<FileType> ConvertsFromFileTypes { get; } = new HashSet<FileType>
        {
            FileType.Image,
            FileType.Unknown,
        };

        /// <inheritdoc/>
        public bool Convert(FilePathHolder inputFile, PdfFile outputFile)
        {
            try
            {
                if (!_licenseExceptionLogged && outputFile is PdfFile outputPdf)
                {
                    // Try to convert the file. Ignore errors when the input type is Unknown
                    return inputFile switch
                    {
                        ImageFile inputFileWrapper => Convert(inputFileWrapper, outputPdf, suppressException: false),
                        UnknownFile inputFileWrapper => Convert(inputFileWrapper, outputPdf, suppressException: true),
                        _ => false
                    };
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53225");
            }
        }

        /// <summary>
        /// Create a <see cref="Dto.LeadToolsImageToPdfConverterV1"/>
        /// </summary>
        public DataTransferObjectWithType CreateDataTransferObject()
        {
            return new(new Dto.LeadToolsImageToPdfConverterV1());
        }

        // Try to convert the file
        private bool Convert(FilePathHolder inputFile, PdfFile outputFile, bool suppressException)
        {
            string[] args = new[] { inputFile.FilePath, outputFile.FilePath, "/pdf", "/color" };
            try
            {
                return SystemMethods.RunExtractExecutable(_IMAGE_FORMAT_CONVERTER_EXE, args) == 0;
            }
            catch (Exception ex)
            {
                if (!UtilityMethods.IsLeadtoolsPdfWriteLicensed())
                {
                    new ExtractException("ELI53243", "Application trace: LeadTools PDF write support is not licensed." +
                        " This converter instance will not be tried again.", ex).Log();
                    _licenseExceptionLogged = true;
                }
                else if (!suppressException)
                {
                    throw;
                }

                return false;
            }
        }
    }
}

namespace Extract.FileConverter.ConvertToPdf.Dto
{
    /// <summary>
    /// DTO for <see cref="LeadToolsImageToPdfConverter"/>
    /// </summary>
    public class LeadToolsImageToPdfConverterV1 : IDataTransferObject
    {
        /// <summary>
        /// Create a <see cref="LeadToolsImageToPdfConverter"/> instance from this DTO
        /// </summary>
        public IDomainObject CreateDomainObject()
        {
            return new LeadToolsImageToPdfConverter();
        }
    }
}
