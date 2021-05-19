using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Extract.FileConverter
{
    public sealed class KofaxConverter : IConverter
    {
        public KofaxModel KofaxModel { get; set; } = new KofaxModel();

        public bool IsEnabled { get; set; }

        [JsonIgnore]
        public string ConverterName => "Kofax (Nuance)";

        [JsonIgnore]
        public Collection<DestinationFileFormat> SupportedDestinationFormats => new Collection<DestinationFileFormat>() { DestinationFileFormat.Pdf, DestinationFileFormat.Tif };

        [JsonIgnore]
        public bool HasDataError => KofaxModel.HasDataError;

        /// <summary>
        /// Converts the input file to a PDF, and sets the name to the output file name.
        /// </summary>
        /// <param name="inputFile"></param>
        public void Convert(string inputFile, DestinationFileFormat destinationFileFormat)
        {
            try
            {
                string arguments = "\"" + inputFile + "\"" + " "
                               + "\"" + inputFile + "." + destinationFileFormat.EnumValue() + "\""
                               + " /" + destinationFileFormat.EnumValue() + " /am";
                if (!string.IsNullOrEmpty(KofaxModel.RemovePages))
                {
                    arguments += " /RemovePages " + KofaxModel.RemovePages;
                }
                if (KofaxModel.Color)
                {
                    arguments += " /color";
                }
                if (KofaxModel.SpecifiedCompressionFormat != KofaxFileFormat.None)
                {
                    arguments += " /format " + MapKofaxFileFormats.ToImageFormatConverterFormat(KofaxModel.SpecifiedCompressionFormat);
                }
                if (destinationFileFormat.Equals(DestinationFileFormat.Pdf) && KofaxModel.Compression > 0 && KofaxModel.Compression < 6)
                {
                    arguments += " /compression " + KofaxModel.Compression.ToString(CultureInfo.InvariantCulture);
                }
                if (KofaxModel.PageNumber != -1)
                {
                    arguments += " /page " + KofaxModel.PageNumber.ToString(CultureInfo.InvariantCulture);
                }

                Utilities.SystemMethods.RunExtractExecutable(@$"{Utilities.FileSystemMethods.CommonComponentsPath}\ImageFormatConverter.exe", arguments);
            }
            catch (Exception ee)
            {
                throw ee.AsExtract("ELI51717");
            }
        }

        ///<inheritdoc cref="IConverter"/>
        public IConverter Clone()
        {
            return new KofaxConverter()
            {
                IsEnabled = IsEnabled,
                KofaxModel = KofaxModel.Clone()
            };
        }
    }
}
