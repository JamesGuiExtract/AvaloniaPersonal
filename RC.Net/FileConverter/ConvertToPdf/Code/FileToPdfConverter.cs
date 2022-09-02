using Extract.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Extract.FileConverter.ConvertToPdf
{
    /// <summary>
    /// An aggregate file to PDF converter that takes IConvertFileToPdf dependencies
    /// </summary>
    public class FileToPdfConverter : IConvertFileToPdf, IAggregateFileToPdfConverter
    {
        // Maintain registration order of each converter for each file type
        private readonly ConcurrentDictionary<FileType, List<IConvertFileToPdf>> _fileToPdfConverters = new();

        /// <summary>
        /// Create an instance with the default converters registered
        /// </summary>
        public static FileToPdfConverter CreateDefault()
        {
            var aggregateConverter = new FileToPdfConverter(
                new KofaxImageToPdfConverter(),
                new LeadToolsImageToPdfConverter(),
                new GdPictureImageToPdfConverter(),
                new WKHtmlToPdfConverter(),
                new DevExpressOfficeToPdfConverter());

            return aggregateConverter;
        }

        /// <summary>
        /// Create an instance and register the supplied converters. Parameter order determines converter precedence
        /// </summary>
        public FileToPdfConverter(params IConvertFileToPdf[] converters)
        {
            _ = converters ?? throw new ArgumentNullException(nameof(converters));

            try
            {
                foreach (IConvertFileToPdf converter in converters)
                {
                    RegisterConverter(converter);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53176");
            }
        }

        /// <summary>
        /// Register the converter for the specific type.
        /// Registration order = converter precedence
        /// </summary>
        public void RegisterConverter(FileType convertsFrom, IConvertFileToPdf converter)
        {
            if (converter is IAggregateFileToPdfConverter aggregateConverter)
            {
                ExtractException.Assert("ELI53223", "Circular reference detected",
                    !aggregateConverter.EnumerateConverters().Contains(this));
            }

            try
            {
                _fileToPdfConverters.GetOrAdd(convertsFrom, _ => new()).Add(converter);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53178");
            }
        }

        /// <summary>
        /// Use ConvertToAttribute values to register the converter for all applicable types.
        /// Registration order = converter precedence
        /// </summary>
        public void RegisterConverter(IConvertFileToPdf converter)
        {
            _ = converter ?? throw new ArgumentNullException(nameof(converter));

            try
            {
                foreach (FileType convertsFrom in converter.ConvertsFromFileTypes)
                {
                    _fileToPdfConverters.GetOrAdd(convertsFrom, _ => new()).Add(converter);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53220");
            }

        }

        /// <summary>
        /// Return this and all child converters, recursively
        /// </summary>
        /// <remarks>
        /// The converters will be be enumerated depth-first, then by FileType, ascending
        /// </remarks>
        public IEnumerable<IConvertFileToPdf> EnumerateConverters()
        {
            yield return this;
            foreach (var childConverter in _fileToPdfConverters.OrderBy(kv => kv.Key).SelectMany(kv => kv.Value))
            {
                if (childConverter is IAggregateFileToPdfConverter aggregate)
                {
                    foreach (var converter in aggregate.EnumerateConverters())
                    {
                        yield return converter;
                    }
                }
                else
                {
                    yield return childConverter;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<FileType> ConvertsFromFileTypes =>
            _fileToPdfConverters.Values
            .SelectMany(cl => cl.SelectMany(c => c.ConvertsFromFileTypes))
            .Distinct();

        /// <inheritdoc/>
        public bool Convert(FilePathHolder inputFile, PdfFile outputFile)
        {
            try
            {
                _ = inputFile ?? throw new ArgumentNullException(nameof(inputFile));
                _ = outputFile ?? throw new ArgumentNullException(nameof(outputFile));

                // Don't run if the file is already a PDF
                if (inputFile is PdfFile)
                {
                    return false;
                }

                if (_fileToPdfConverters.TryGetValue(inputFile.FileType, out var converters))
                {
                    return TryToConvert(inputFile, outputFile, converters);
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53177");
            }
        }

        /// <summary>
        /// Attempt to convert a file to a PDF with registered converters for the input type,
        /// trying the converters each at most once, in the order they were registered
        /// </summary>
        public bool Convert(string inputFile, string outputFile)
        {
            _ = inputFile ?? throw new ArgumentNullException(nameof(inputFile));
            _ = outputFile ?? throw new ArgumentNullException(nameof(outputFile));

            return Convert(FilePathHolder.Create(inputFile), new(outputFile));
        }

        // Attempt to convert the file with all converters registered for the type until one succeeds.
        private static bool TryToConvert(FilePathHolder inputFile, PdfFile outputFile, IEnumerable<IConvertFileToPdf> converters)
        {
            // Return true if there a registered converter that can successfully convert the file
            // Stops after the first success. Logs any errors as app trace exceptions
            return converters
                .Distinct()
                .Any(converter =>
            {
                try
                {
                    return converter.Convert(inputFile, outputFile);
                }
                catch (Exception ex)
                {
                    var uex = new ExtractException("ELI53219", "Application trace: File conversion attempt failed", ex);
                    uex.Log();
                    return false;
                }
            });
        }

        /// <summary>
        /// Create a <see cref="Dto.FileToPdfConverterV1"/>
        /// </summary>
        public DataTransferObjectWithType CreateDataTransferObject()
        {
            // Create a mapping of distinct converter instances to what will be their index in
            // the indexed collection in the DTO
            Dictionary<IConvertFileToPdf, int> converterToIndex = _fileToPdfConverters.Values
                .SelectMany(c => c)
                .Distinct()
                .Select((converter, i) => new KeyValuePair<IConvertFileToPdf, int>(converter, i))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            // Get the converters for a particular file type as indexes into the collection
            List<int> GetConverterIndexes(FileType fileType)
            {
                return (_fileToPdfConverters.TryGetValue(fileType, out var c))
                    ? c.Select(c => converterToIndex[c]).ToList()
                    : new();
            }

            // Create the indexed collection of converters for the DTO
            List<DataTransferObjectWithType> indexToConverterDto = converterToIndex
                .OrderBy(kv => kv.Value)
                .Select(kv => kv.Key.CreateDataTransferObject())
                .ToList();

            // Create the DTO instance
            return new(new Dto.FileToPdfConverterV1(indexToConverterDto,
                unknownConverters: GetConverterIndexes(FileType.Unknown),
                imageConverters: GetConverterIndexes(FileType.Image),
                textConverters: GetConverterIndexes(FileType.Text),
                htmlConverters: GetConverterIndexes(FileType.Html),
                wordConverters: GetConverterIndexes(FileType.Word),
                excelConverters: GetConverterIndexes(FileType.Excel)));
        }
    }
}

namespace Extract.FileConverter.ConvertToPdf.Dto
{
    /// <summary>
    /// DTO for <see cref="FileToPdfConverter"/> that stores references efficiently for serialization
    /// </summary>
    public class FileToPdfConverterV1 : IDataTransferObject
    {
        /// <summary>
        /// Create an instance with the supplied property values
        /// </summary>
        [JsonConstructor]
        public FileToPdfConverterV1(
            ICollection<DataTransferObjectWithType> fileToPdfConverters,
            ICollection<int> unknownConverters,
            ICollection<int> imageConverters,
            ICollection<int> textConverters,
            ICollection<int> htmlConverters,
            ICollection<int> wordConverters,
            ICollection<int> excelConverters)
        {
            FileToPdfConverters = fileToPdfConverters;
            UnknownConverters = unknownConverters;
            ImageConverters = imageConverters;
            TextConverters = textConverters;
            HtmlConverters = htmlConverters;
            WordConverters = wordConverters;
            ExcelConverters = excelConverters;
        }

        /// <summary>
        /// Each distinct converter instance that is registered for this instance
        /// </summary>
        public ICollection<DataTransferObjectWithType> FileToPdfConverters { get; }

        /// <summary>
        /// The converters registered for <see cref="FileType.Unknown"/>, as index into FileToPdfConverters
        /// </summary>
        public ICollection<int> UnknownConverters { get; }

        /// <summary>
        /// The converters registered for <see cref="FileType.Image"/>, as index into FileToPdfConverters
        /// </summary>
        public ICollection<int> ImageConverters { get; }

        /// <summary>
        /// The converters registered for <see cref="FileType.Text"/>, as index into FileToPdfConverters
        /// </summary>
        public ICollection<int> TextConverters { get; }

        /// <summary>
        /// The converters registered for <see cref="FileType.Html"/>, as index into FileToPdfConverters
        /// </summary>
        public ICollection<int> HtmlConverters { get; }

        /// <summary>
        /// The converters registered for <see cref="FileType.Word"/>, as index into FileToPdfConverters
        /// </summary>
        public ICollection<int> WordConverters { get; }

        /// <summary>
        /// The converters registered for <see cref="FileType.Excel"/>, as index into FileToPdfConverters
        /// </summary>
        public ICollection<int> ExcelConverters { get; }

        /// <summary>
        /// Create a <see cref="FileToPdfConverter"/> from this DTO instance
        /// </summary>
        public IDomainObject CreateDomainObject()
        {
            var converters = FileToPdfConverters
                .Select(dto => (IConvertFileToPdf)dto.CreateDomainObject())
                .ToList();

            FileToPdfConverter fileToPdfConverter = new();

            void RegisterConvertersForType(FileType fileType, ICollection<int> converterIndexes)
            {
                foreach (var i in converterIndexes)
                    fileToPdfConverter.RegisterConverter(fileType, converters[i]);
            }

            RegisterConvertersForType(FileType.Unknown, UnknownConverters);
            RegisterConvertersForType(FileType.Image, ImageConverters);
            RegisterConvertersForType(FileType.Text, TextConverters);
            RegisterConvertersForType(FileType.Html, HtmlConverters);
            RegisterConvertersForType(FileType.Word, WordConverters);
            RegisterConvertersForType(FileType.Excel, ExcelConverters);

            return fileToPdfConverter;
        }
    }
}
