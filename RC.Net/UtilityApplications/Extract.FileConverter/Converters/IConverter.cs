using Extract.FileConverter.Converters;
using System.Collections.ObjectModel;

namespace Extract.FileConverter
{
    public interface IConverter
    {
        Collection<FileFormat> SupportedFormats { get; }

        bool IsEnabled { get; set; }

        string ConverterName { get; }

        void Convert(string inputFile, FileFormat destinationFileFormat);
    }
}
