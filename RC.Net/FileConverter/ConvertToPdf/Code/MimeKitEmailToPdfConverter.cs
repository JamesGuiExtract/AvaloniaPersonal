using Extract.Utilities;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Extract.FileConverter.ConvertToPdf
{
    public class MimeKitEmailToPdfConverter : IConvertFileToPdf, IAggregateFileToPdfConverter
    {
        private const string LOGICAL_DOCUMENT_NUMBER_TAG = "/ExtractSystems.LogicalDocumentNumber";
        private const string LOGICAL_PAGE_NUMBER_TAG = "/ExtractSystems.LogicalPageNumber";

        private readonly IConvertFileToPdf _fileConverter;

        /// <summary>
        /// Create an <see cref="MimeKitEmailToPdfConverter"/> with the provided dependencies
        /// </summary>
        /// <param name="fileConverter">An <see cref="IConvertFileToPdf"/> implementation
        /// to be used to convert the email body and attachments to PDF</param>
        public MimeKitEmailToPdfConverter(IConvertFileToPdf fileConverter)
        {
            _fileConverter = fileConverter ?? throw new ArgumentNullException(nameof(fileConverter));
        }

        /// <summary>
        /// Create an instance that can convert from emails as well as regular files
        /// </summary>
        public static MimeKitEmailToPdfConverter CreateDefault()
        {
            return new MimeKitEmailToPdfConverter(FileToPdfConverter.CreateDefault());
        }

        /// <inheritdoc/>
        public IEnumerable<FileType> ConvertsFromFileTypes =>
            Enumerable.Repeat(FileType.Email, 1)
            .Concat(_fileConverter.ConvertsFromFileTypes)
            .Distinct();

        /// <summary>
        /// Create a <see cref="Dto.MimeKitEmailToPdfConverterV1"/>
        /// </summary>
        public DataTransferObjectWithType CreateDataTransferObject()
        {
            return new(new Dto.MimeKitEmailToPdfConverterV1());
        }

        /// <inheritdoc/>
        public IEnumerable<IConvertFileToPdf> EnumerateConverters()
        {
            yield return this;

            if (_fileConverter is IAggregateFileToPdfConverter aggregate)
            {
                foreach (var converter in aggregate.EnumerateConverters())
                {
                    yield return converter;
                }
            }
        }

        /// <summary>
        /// Convert an email and it's attachments into a single PDF file that contains pagination information
        /// </summary>
        public bool Convert(FilePathHolder inputFile, PdfFile outputFile)
        {
            try
            {
                _ = outputFile ?? throw new ArgumentNullException(nameof(outputFile));

                return inputFile switch
                {
                    EmailFile inputFileWrapper => ConvertEmail(inputFileWrapper, outputFile, out int _),
                    _ => _fileConverter.Convert(inputFile, outputFile)
                };

            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53228");
            }
        }

        /// <summary>
        /// Convert an email and it's attachments into a single PDF file that contains pagination information
        /// </summary>
        public bool ConvertEmail(EmailFile inputFile, PdfFile outputFile, out int pageCount)
        {
            DirectoryInfo tempDir = null;
            try
            {
                _ = outputFile ?? throw new ArgumentNullException(nameof(outputFile));
                _ = inputFile ?? throw new ArgumentNullException(nameof(inputFile));

                tempDir = FileSystemMethods.GetTemporaryFolder();

                FileCollectorDatabaseClient fileCollector = new();
                MimeFileSplitter mimeFileSplitter = new(fileCollector, tempDir.FullName);

                mimeFileSplitter.SplitFile(new EmailFileRecord(inputFile.FilePath));

                ExtractException.Assert("ELI53231", "Unable to split email!", fileCollector.OutputFiles.Any());

                ConvertOutputFiles(fileCollector.OutputFiles);
                ConcatenatePDFs(outputFile.FilePath, fileCollector.OutputFiles, out pageCount);

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53229");
            }
            finally
            {
                try
                {
                    tempDir?.Delete(true);
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI53263", "Could not delete temporary directory");
                }
            }
        }

        // Attempt to convert each file into a pdf and update the filename if a conversion took place
        private void ConvertOutputFiles(IEnumerable<LogicalFileInfo> files)
        {
            foreach (var file in files)
            {
                var inputFile = FilePathHolder.Create(file.FileRecord.FilePath);
                var outputFile = new PdfFile(file.FileRecord.FilePath + ".pdf");

                if (inputFile is PdfFile)
                {
                    // No conversion necessary
                    continue;
                }
                else if (_fileConverter.Convert(inputFile, outputFile))
                {
                    file.FileRecord.FilePath = outputFile.FilePath;
                }
                else if (file.FileRecord.IsAttachment)
                {
                    var ex = new ExtractException("ELI53230", "Could not convert email attachment");
                    ex.AddDebugData("Email file", file.FileRecord.SourceEmailFileRecord.FilePath);
                    ex.AddDebugData("Attachment name", file.FileRecord.OriginalName);
                    ex.AddDebugData("Attachment number", file.FileRecord.AttachmentNumber);
                    throw ex;
                }
                else
                {
                    var ex = new ExtractException("ELI53241", "Could not convert email body");
                    ex.AddDebugData("Email file", file.FileRecord.SourceEmailFileRecord.FilePath);
                    throw ex;
                }
            }
        }

        // Put the PDFs together into one PDF
        private static void ConcatenatePDFs(string outputFile, List<LogicalFileInfo> files, out int pageCount)
        {
            try
            {
                using FileStream outStream = new(outputFile, FileMode.Create, FileAccess.Write, FileShare.None);
                using PdfDocument destinationDoc = new(outStream);

                foreach (var file in files)
                {
                    AddPagesToPDF(destinationDoc, file);
                }

                pageCount = destinationDoc.PageCount;
                destinationDoc.Close();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53242");
            }
        }

        // Add pages to destinationDoc
        // Add custom tags to the page with the logical document and page numbers
        private static void AddPagesToPDF(PdfDocument destinationDoc, LogicalFileInfo file)
        {
            try
            {
                using PdfDocument sourceDoc = PdfReader.Open(file.FileRecord.FilePath, PdfDocumentOpenMode.Import);
                int logicalPageNum = 1;
                foreach (var page in sourceDoc.Pages)
                {
                    var importedPage = destinationDoc.AddPage(page);

                    // Add the logical document and page number so that it will be easy to create pagination VOA files later
                    var customValues = importedPage.CustomValues.Elements;
                    customValues.Add(LOGICAL_DOCUMENT_NUMBER_TAG, new PdfInteger(file.ChildDocumentNumber));
                    customValues.Add(LOGICAL_PAGE_NUMBER_TAG, new PdfInteger(logicalPageNum++));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53242");
            }
        }

        // Stores the position of an email part to be used in the output PDF
        private sealed class LogicalFileInfo
        {
            /// <summary>
            /// Position of this email part in the output PDF
            /// </summary>
            public int ChildDocumentNumber { get; set; }

            /// <summary>
            /// Information about the email part
            /// </summary>
            public EmailPartFileRecord FileRecord { get; set; }
        }

        // Dependency for MimeFileSplitter, used to collect information about the email parts
        private sealed class FileCollectorDatabaseClient : IMimeFileSplitterDatabaseClient
        {
            private List<LogicalFileInfo> _logicalFiles;
            private readonly List<EmailPartFileRecord> _database = new();

            /// <summary>
            /// Ordered list of output files (message body first) that were split from the email
            /// </summary>
            public List<LogicalFileInfo> OutputFiles => _logicalFiles;

            /// <summary>
            /// Return a disposable that doesn't do anything
            /// </summary>
            public DisposableFileTaskSession CreateFileTaskSession(EmailFileRecord sourceFile)
            {
                return new DisposableFileTaskSession(null, default);
            }

            /// <summary>
            /// Map the output file info to the <see cref="EmailPartFileRecord"/>s that were
            /// added to the 'database'
            /// </summary>
            public void ProcessOutputFiles(
                int fileTaskSessionID,
                EmailFileRecord sourceFile,
                IEnumerable<SourceToOutput> outputFiles)
            {
                _logicalFiles = outputFiles
                    .Select(file => new LogicalFileInfo
                    {
                        FileRecord = _database[file.DestinationFileID - 1],
                        ChildDocumentNumber = file.ChildDocumentNumber
                    })
                    .OrderBy(file => file.ChildDocumentNumber)
                    .ToList();
            }

            /// <summary>
            /// Always returns true, stores the info for later
            /// </summary>
            public bool TryAddFileToDatabase(EmailPartFileRecord fileRecord, out int fileID)
            {
                _database.Add(fileRecord);
                fileID = _database.Count;
                return true;
            }

            /// <summary>
            /// Get a random file name in the specified directory that has the same extension as the OriginalName
            /// </summary>
            public string GetOutputFilePath(EmailPartFileRecord outputFileRecord, string outputDir, int copyNumber)
            {
                using TemporaryFile tempFile = new(outputDir, Path.GetExtension(outputFileRecord.OriginalName), true);
                return tempFile.FileName;
            }
        }
    }
}

namespace Extract.FileConverter.ConvertToPdf.Dto
{
    /// <summary>
    /// DTO for <see cref="MimeKitEmailToPdfConverter"/>
    /// </summary>
    public class MimeKitEmailToPdfConverterV1 : IDataTransferObject
    {
        /// <summary>
        /// Create a <see cref="MimeKitEmailToPdfConverter"/> instance from this DTO
        /// </summary>
        public IDomainObject CreateDomainObject()
        {
            // TODO: This system will eventually need to be user-configurable but for now just create the default object
            return MimeKitEmailToPdfConverter.CreateDefault();
        }
    }
}
