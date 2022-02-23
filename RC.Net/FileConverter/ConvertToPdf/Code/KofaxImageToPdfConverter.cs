using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Extract.FileConverter.ConvertToPdf
{
    /// <summary>
    /// An <see cref="IConvertFileToPdf"/> that uses Kofax (Nuance) via ImageFormatConverter.exe
    /// </summary>
    public sealed class KofaxImageToPdfConverter : IConvertFileToPdf
    {
        private static readonly string _IMAGE_FORMAT_CONVERTER_EXE =
            Path.Combine(FileSystemMethods.CommonComponentsPath, "ImageFormatConverter.exe");

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
                if (outputFile is PdfFile outputPdf)
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
                throw ex.AsExtract("ELI53222");
            }
        }

        /// <summary>
        /// Create a <see cref="Dto.KofaxImageToPdfConverterV1"/>
        /// </summary>
        public DataTransferObjectWithType CreateDataTransferObject()
        {
            return new(new Dto.KofaxImageToPdfConverterV1());
        }

        // Try to convert the file
        private static bool Convert(FilePathHolder inputFile, PdfFile outputFile, bool suppressException)
        {
            string[] args = new[] { inputFile.FilePath, outputFile.FilePath, "/pdf", "/am", "/color" };
            try
            {
                return SystemMethods.RunExtractExecutable(_IMAGE_FORMAT_CONVERTER_EXE, args) == 0;
            }
            catch (Exception) when (suppressException)
            {
                return false;
            }
        }
    }
}

namespace Extract.FileConverter.ConvertToPdf.Dto
{
    /// <summary>
    /// DTO for <see cref="KofaxImageToPdfConverter"/>
    /// </summary>
    public class KofaxImageToPdfConverterV1 : IDataTransferObject
    {
        /// <summary>
        /// Create a <see cref="KofaxImageToPdfConverter"/> instance from this DTO
        /// </summary>
        public IDomainObject CreateDomainObject()
        {
            return new KofaxImageToPdfConverter();
        }
    }
}
