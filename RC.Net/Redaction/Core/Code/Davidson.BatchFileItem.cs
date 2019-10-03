using ProtoBuf;
using System;
using System.IO;
using System.Text;

namespace Extract.Redaction.Davidson
{
    /// <summary>
    /// Base class for two types of items appearing in Davidson County RTF batches
    /// </summary>
    [ProtoContract]
    [ProtoInclude(500, typeof(BetweenFileData))]
    [ProtoInclude(501, typeof(OutputFileData))]
    public abstract class BatchFileItem
    {
        /// <summary>
        /// Encoding used by the RTF files (probably all ASCII anyway but...
        /// </summary>
        internal static readonly Encoding _encoding = Encoding.GetEncoding("windows-1252");

        /// <summary>
        /// Write out the item in the format it was received
        /// </summary>
        /// <param name="stream">An open stream to write to and leave open</param>
        public abstract void ToDavidsonFormat(Stream stream);

        /// <summary>
        /// Write out this instance as Protocol Buffer format with base-128 length prefix
        /// </summary>
        /// <param name="stream">An open stream to write to and leave open</param>
        public void ToProtobuf(Stream stream)
        {
            Serializer.SerializeWithLengthPrefix(stream, this, PrefixStyle.Base128);
        }

        /// <summary>
        /// Read an instance of a subclass from Protocol Buffer format with base-128 length prefix
        /// </summary>
        /// <param name="stream">An open stream to read one instance from</param>
        public static BatchFileItem FromProtobuf(Stream stream)
        {
            return Serializer.DeserializeWithLengthPrefix<BatchFileItem>(stream, PrefixStyle.Base128);
        }
    }

    /// <summary>
    /// Represents content that occurs at the begining of an RTF batch or between actual file contents, e.g., labels and whitespace
    /// </summary>
    [ProtoContract]
    public class BetweenFileData : BatchFileItem
    {
        /// <summary>
        /// Constructor for deserialization
        /// </summary>
        public BetweenFileData()
        { }

        /// <summary>
        /// Create instance with text contents
        /// </summary>
        /// <param name="contents">The text that this instance represents</param>
        public BetweenFileData(string contents)
        {
            Contents = contents;
        }

        /// <summary>
        /// The text that this instance represents
        /// </summary>
        [ProtoMember(1)]
        public string Contents { get; }

        /// <summary>
        /// Write out contents as windows-1252-encoded characters
        /// </summary>
        /// <param name="stream">An open stream to write to and leave open</param>
        public override void ToDavidsonFormat(Stream stream)
        {
            try
            {
                using (var writer = new StreamWriter(stream, _encoding, 1024, leaveOpen: true))
                {
                    writer.Write(Contents);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48416");
            }
        }
    }

    /// <summary>
    /// Whether contents of an <see cref="OutputFileData"/>are rich text or not
    /// (e.g., invalid RTF will be considered to be a TextFile)
    /// </summary>
    public enum OutputFileType
    {
        /// <summary>
        /// Not Rich Text format
        /// </summary>
        TextFile,

        /// <summary>
        /// Rich Text Format
        /// </summary>
        RichTextFile,
    }

    /// <summary>
    /// Represents RTF batch content that will be output to a file plus extra stuff between this and the next file's data (sub-label and whitespace)
    /// </summary>
    [ProtoContract]
    public class OutputFileData : BatchFileItem
    {
        /// <summary>
        /// Constructor for deserialization
        /// </summary>
        public OutputFileData()
        { }

        /// <summary>
        /// Create an instance from data
        /// </summary>
        /// <param name="fileNameBase">The file path and name, not including the type-designating-extension (e.g., '.rtf')</param>
        /// <param name="fileType">The type of file that the contents represent</param>
        /// <param name="contents">The full text to be written out to the file</param>
        /// <param name="suffix">Text occuring between this file and the next in the batch (sub-label and whitespace). Can be an empty string</param>
        public OutputFileData(string fileNameBase, OutputFileType fileType, string contents, string suffix)
        {
            FileNameBase = fileNameBase;
            FileType = fileType;
            Contents = contents;
            Suffix = suffix;
        }

        /// <summary>
        /// The file path and name, not including the type-designating-extension (e.g., '.rtf')
        /// </summary>
        [ProtoMember(1)]
        public string FileNameBase { get; }

        /// <summary>
        /// Type of file that the contents represent
        /// </summary>
        [ProtoMember(2)]
        public OutputFileType FileType { get; }

        /// <summary>
        /// Full text to be written out to the file
        /// </summary>
        [ProtoMember(3)]
        public string Contents { get; set; }

        /// <summary>
        /// Text occuring between this file and the next in the batch (sub-label and whitespace). Can be an empty string.
        /// </summary>
        [ProtoMember(4)]
        public string Suffix { get; }

        /// <summary>
        /// ID assigned when the file is added to the FAM DB. Not serialized
        /// </summary>
        public int FileID { get; set; }

        /// <summary>
        /// Write out contents + suffix as windows-1252-encoded characters
        /// </summary>
        /// <param name="stream">An open stream to write to and leave open</param>
        public override void ToDavidsonFormat(Stream stream)
        {
            try
            {
                using (var writer = new StreamWriter(stream, _encoding, 1024, leaveOpen: true))
                {
                    writer.Write(Contents);
                    writer.Write(Suffix);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48417");
            }
        }
    }
}
