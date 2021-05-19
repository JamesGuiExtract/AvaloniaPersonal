using System.Collections.ObjectModel;

namespace Extract.FileConverter
{
    public interface IConverter
    {
        /// <summary>
        /// Gets or sets the supported destination formats for a converter
        /// </summary>
        Collection<DestinationFileFormat> SupportedDestinationFormats { get; }

        /// <summary>
        /// Gets or sets the is enabled boolean to indicate if the program should use this converter
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Checks to see if there is a data error contained in the converter.
        /// </summary>
        bool HasDataError { get; }

        /// <summary>
        /// Gets or sets the converter name.
        /// </summary>
        string ConverterName { get; }

        /// <summary>
        /// Converts the input file to the destination file format.
        /// </summary>
        /// <param name="inputFile">The fully qualified path to the input file</param>
        /// <param name="destinationFileFormat">The file format to convert to.</param>
        void Convert(string inputFile, DestinationFileFormat destinationFileFormat);

        /// <summary>
        /// Deep clones the converter.
        /// </summary>
        /// <returns>Returns a deep clone of the provided converter.</returns>
        IConverter Clone();
    }
}
