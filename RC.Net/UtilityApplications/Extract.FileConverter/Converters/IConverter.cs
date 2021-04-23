using Extract.FileConverter.Converters;
using System.Collections.ObjectModel;

namespace Extract.FileConverter
{
    public interface IConverter
    {
        /// <summary>
        /// Gets or sets the supported destination formats for a converter
        /// </summary>
        Collection<FileFormat> SupportedDestinationFormats { get; }

        /// <summary>
        /// Gets or sets the is enabled boolean to indicate if the program should use this converter
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the converter name.
        /// </summary>
        string ConverterName { get; }

        /// <summary>
        /// Converts the input file to the destination file format.
        /// </summary>
        /// <param name="inputFile">The fully qualified path to the input file</param>
        /// <param name="destinationFileFormat">The file format to convert to.</param>
        void Convert(string inputFile, FileFormat destinationFileFormat);

        IConverter Clone();
    }
}
