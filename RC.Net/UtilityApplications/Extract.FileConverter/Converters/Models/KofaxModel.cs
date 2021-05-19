using Extract.Utilities;
using System;
using System.ComponentModel;
using System.Linq;

namespace Extract.FileConverter
{
    public sealed class KofaxModel : NotifyPropertyChangedObject, IDataErrorInfo
    {
        private int _pageNumber = -1;
        private string _removePages;
        private bool _color;
        private int _compression = 3;
        private KofaxFileFormat _specifiedCompressionFormat = KofaxFileFormat.None;

        /// <summary>
        /// Used to convert a single page number with the converter.
        /// </summary>
        public int PageNumber
        {
            get => _pageNumber;
            set => Set(ref _pageNumber, value);
        }

        /// <summary>
        /// Gets or sets the remove pages string. Can be an individual number, a comma-separated list, 
        /// a range of pages denoted with a hyphen, or a dash followed by a number to indicate you should remove last x pages.
        /// </summary>
        public string RemovePages
        {
            get => _removePages;
            set => Set(ref _removePages, value);
        }

        /// <summary>
        /// An argument to preserve the color on images.
        /// </summary>
        public bool Color
        {
            get => _color;
            set => Set(ref _color, value);
        }

        /// <summary>
        /// Used to specify how much to compress the image. 1 is more compression, 5 is the least compression.
        /// </summary>
        public int Compression
        {
            get => _compression;
            set => Set(ref _compression, value);
        }

        /// <summary>
        /// Used to change the format of the output file.
        /// </summary>
        public KofaxFileFormat SpecifiedCompressionFormat
        {
            get => _specifiedCompressionFormat;
            set
            {
                try
                {
                    System.Collections.Generic.List<KofaxFileFormat> enumValues = Enum.GetValues(typeof(KofaxFileFormat)).Cast<KofaxFileFormat>().ToList();
                    if (enumValues.Any(m => m.Equals(value)))
                    {
                        Set(ref _specifiedCompressionFormat, value);
                    }
                    else
                    {
                        throw new ExtractException("ELI51712", $"The specified compression format is not supported: {value.AsString()}.");
                    }
                }
                catch (Exception ee)
                {
                    throw ee.AsExtract("ELI51720");
                }
            }
        }

        public bool HasDataError { get; private set; } = false;

        string IDataErrorInfo.Error => null;

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                HasDataError = true;
                if (columnName == "RemovePages")
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(RemovePages))
                        {
                            UtilityMethods.ValidatePageNumbers(RemovePages);
                        }
                    }
                    catch (ExtractException)
                    {
                        return "Invalid input! The input must be an individual number, a comma-separated list, a range of pages denoted with a hyphen, or a dash followed by a number.";
                    }
                }
                if (columnName == "PageNumber" && PageNumber != -1)
                {
                    if (PageNumber <= 0)
                    {
                        return "Page number must be a non negative number.";
                    }
                }

                HasDataError = false;
                // If there's no error, null gets returned
                return null;
            }
        }

        /// <summary>
        /// Performs a deep clone of the KofaxModel.
        /// </summary>
        /// <returns>Returns a deep clone of the KofaxModel.</returns>
        public KofaxModel Clone()
        {
            return (KofaxModel)MemberwiseClone();
        }
    }
}
