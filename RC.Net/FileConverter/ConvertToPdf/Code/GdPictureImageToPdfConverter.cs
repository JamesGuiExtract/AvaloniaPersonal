using Extract.GdPicture;
using Extract.Utilities;
using System;
using System.Collections.Generic;

namespace Extract.FileConverter.ConvertToPdf
{
    /// <summary>
    /// An <see cref="IConvertFileToPdf"/> that uses GdPicture
    /// </summary>
    public sealed class GdPictureImageToPdfConverter : IConvertFileToPdf
    {
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
                throw ex.AsExtract("ELI53614");
            }
        }

        /// <summary>
        /// Create a <see cref="Dto.GdPictureImageToPdfConverter"/>
        /// </summary>
        public DataTransferObjectWithType CreateDataTransferObject()
        {
            return new(new Dto.GdPictureImageToPdfConverterV1());
        }

        // Try to convert the file
        private static bool Convert(FilePathHolder inputFile, PdfFile outputFile, bool suppressException)
        {
            try
            {
                using GdPictureUtility gdPictureUtility = new();
                var documentConverter = gdPictureUtility.DocumentConverter;

                GdPictureUtility.ThrowIfStatusNotOK(
                    documentConverter.LoadFromFile(inputFile.FilePath),
                    "ELI53615",
                    "Could not load file into the document converter",
                    new(inputFile.FilePath));

                GdPictureUtility.ThrowIfStatusNotOK(
                    documentConverter.SaveAsPDF(outputFile.FilePath),
                    "ELI53616",
                    "Could not save file as PDF",
                    new(inputFile.FilePath));

                return true;
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
    /// DTO for <see cref="GdPictureImageToPdfConverter>
    /// </summary>
    public class GdPictureImageToPdfConverterV1 : IDataTransferObject
    {
        /// <summary>
        /// Create a <see cref="GdPictureImageToPdfConverter"/> instance from this DTO
        /// </summary>
        public IDomainObject CreateDomainObject()
        {
            return new GdPictureImageToPdfConverter();
        }
    }
}
