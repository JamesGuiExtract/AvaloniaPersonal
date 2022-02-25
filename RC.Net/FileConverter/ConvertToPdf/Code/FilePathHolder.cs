using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Extract.FileConverter
{
    /// <summary>
    /// Represent file type determined from extension or other means
    /// </summary>
    public enum FileType
    {
        Unknown = 0,
        Image,
        Pdf,
        Text,
        Html,
        Word,
        Excel,
        Email,
    }

    /// <summary>
    /// Base class for file-type-specific wrapper classes
    /// </summary>
    public abstract class FilePathHolder
    {
        readonly string _filePath;

        /// <summary>
        /// The path to the file that this instance represents
        /// </summary>
        public string FilePath => _filePath;

        /// <summary>
        /// The <see cref="FileConverter.FileType"/> that this instance represents
        /// </summary>
        public abstract FileType FileType { get; }

        /// <summary>
        /// Factory method that determines the type of path and creates the appropriate sub-class instance
        /// </summary>
        public static FilePathHolder Create(string filePath)
        {
            return GetFileHolder(filePath);
        }

        /// <summary>
        /// Called by derived class constructors
        /// </summary>
        protected FilePathHolder(string filePath)
        {
            try
            {
                _ = filePath ?? throw new ArgumentNullException(nameof(filePath));
                _filePath = filePath;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53175");
            }
        }

        // Create a file-extension to file-type mapping
        // TODO: Make this more exhaustive or user-configurable
        private static readonly Dictionary<string, FileType> _extensionToFileType =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { ".txt", FileType.Text },

                { ".html", FileType.Html },
                { ".htm", FileType.Html },

                { ".rtf", FileType.Word },
                { ".doc", FileType.Word },
                { ".docx", FileType.Word },

                { ".xls", FileType.Excel },
                { ".xlsx", FileType.Excel },
                { ".csv", FileType.Excel },
                { ".tsv", FileType.Excel },

                { ".tif", FileType.Image },
                { ".tiff", FileType.Image },
                { ".jpg", FileType.Image },
                { ".jpeg", FileType.Image },
                { ".jpe", FileType.Image },
                { ".jif", FileType.Image },
                { ".jfif", FileType.Image },
                { ".jfi", FileType.Image },
                { ".jp2", FileType.Image },
                { ".j2k", FileType.Image },
                { ".jpf", FileType.Image },
                { ".jpx", FileType.Image },
                { ".jpm", FileType.Image },
                { ".gif", FileType.Image },
                { ".png", FileType.Image },
                { ".bmp", FileType.Image },
                { ".dib", FileType.Image },

                { ".pdf", FileType.Pdf },

                { ".eml", FileType.Email },
            };

        // Determine the type of file from a path
        // TODO: It would be nice to use more than the extension to make this determination
        private static FilePathHolder GetFileHolder(string filePath)
        {
            try
            {
                string ext = Path.GetExtension(filePath);

                if (string.IsNullOrEmpty(ext))
                {
                    return new UnknownFile(filePath);
                }
                else if (_extensionToFileType.TryGetValue(ext, out FileType fileType))
                {
                    return fileType switch
                    {
                        FileType.Unknown => new UnknownFile(filePath),
                        FileType.Image => new ImageFile(filePath),
                        FileType.Pdf => new PdfFile(filePath),
                        FileType.Text => new TextFile(filePath),
                        FileType.Html => new HtmlFile(filePath),
                        FileType.Word => new WordFile(filePath),
                        FileType.Excel => new ExcelFile(filePath),
                        FileType.Email => new EmailFile(filePath),
                        _ => throw new NotImplementedException(),
                    };
                }
                // Consider numeric extensions like .001 to be images because county land record examples are known to use this type of extension
                else if (Regex.IsMatch(ext, @"\A\.\d{3}\z"))
                {
                    return new ImageFile(filePath);
                }
                else
                {
                    return new UnknownFile(filePath);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53179");
            }
        }
    }

    /// <summary>
    /// Path to an unknown type of file
    /// </summary>
    public class UnknownFile : FilePathHolder
    {
        public UnknownFile(string filePath) : base(filePath) { }

        public override FileType FileType => FileType.Unknown;
    }

    /// <summary>
    /// Path to a text file
    /// </summary>
    public class TextFile : FilePathHolder
    {
        public TextFile(string filePath) : base(filePath) { }

        public override FileType FileType => FileType.Text;
    }

    /// <summary>
    /// Path to an html file
    /// </summary>
    public class HtmlFile : FilePathHolder
    {
        public HtmlFile(string filePath) : base(filePath) { }

        public override FileType FileType => FileType.Html;
    }

    /// <summary>
    /// Path to an excel file
    /// </summary>
    public class ExcelFile : FilePathHolder
    {
        public ExcelFile(string filePath) : base(filePath) { }

        public override FileType FileType => FileType.Excel;
    }

    /// <summary>
    /// Path to a word file
    /// </summary>
    public class WordFile : FilePathHolder
    {
        public WordFile(string filePath) : base(filePath) { }

        public override FileType FileType => FileType.Word;
    }

    /// <summary>
    /// Path to an image file
    /// </summary>
    public class ImageFile : FilePathHolder
    {
        public ImageFile(string filePath) : base(filePath) { }
        public override FileType FileType => FileType.Image;
    }

    /// <summary>
    /// Path to a pdf file
    /// </summary>
    public class PdfFile : FilePathHolder
    {
        public PdfFile(string filePath) : base(filePath) { }

        public override FileType FileType => FileType.Pdf;
    }

    /// <summary>
    /// Path to an email file
    /// </summary>
    public class EmailFile : FilePathHolder
    {
        public EmailFile(string filePath) : base(filePath) { }

        public override FileType FileType => FileType.Email;
    }
}
