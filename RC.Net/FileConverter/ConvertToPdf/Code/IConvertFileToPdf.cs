using Extract.Utilities;
using System.Collections.Generic;

namespace Extract.FileConverter.ConvertToPdf
{
    /// <summary>
    /// Interface for a file-to-pdf converter
    /// </summary>
    public interface IConvertFileToPdf : IDomainObject
    {
        /// <summary>
        /// Get all the <see cref="FileType"/>s that this instance can convert from
        /// </summary>
        IEnumerable<FileType> ConvertsFromFileTypes { get; }

        /// <summary>
        /// Try to convert, return true if successful
        /// </summary>
        bool Convert(FilePathHolder inputFile, PdfFile outputFile);
    }

    /// <summary>
    /// Interface for a file-to-pdf converter that contains other converters
    /// </summary>
    public interface IAggregateFileToPdfConverter
    {
        /// <summary>
        /// Return this and all child converters, recursively
        /// </summary>
        IEnumerable<IConvertFileToPdf> EnumerateConverters();
    }
}
