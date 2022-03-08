using Extract.Utilities;
using org.apache.pdfbox.pdmodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Extract.FileConverter.ConvertToPdf
{
    public class MimeKitEmailToPdfConverter : IConvertFileToPdf, IAggregateFileToPdfConverter
    {
        private const string LOGICAL_DOCUMENT_NUMBER_TAG = "ExtractSystems.LogicalDocumentNumber";
        private const string LOGICAL_PAGE_NUMBER_TAG = "ExtractSystems.LogicalPageNumber";

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
        private void ConvertOutputFiles(IEnumerable<EmailPartFileRecord> files)
        {
            foreach (var file in files)
            {
                var inputFile = FilePathHolder.Create(file.FilePath);
                var outputFile = new PdfFile(file.FilePath + ".pdf");

                if (inputFile is PdfFile)
                {
                    // No conversion necessary
                    continue;
                }
                else if (_fileConverter.Convert(inputFile, outputFile))
                {
                    file.FilePath = outputFile.FilePath;
                }
                else if (file.IsAttachment)
                {
                    var ex = new ExtractException("ELI53230", "Could not convert email attachment");
                    ex.AddDebugData("Email file", file.SourceEmailFileRecord.FilePath);
                    ex.AddDebugData("Attachment name", file.OriginalName);
                    ex.AddDebugData("Attachment number", file.AttachmentNumber);
                    throw ex;
                }
                else
                {
                    var ex = new ExtractException("ELI53241", "Could not convert email body");
                    ex.AddDebugData("Email file", file.SourceEmailFileRecord.FilePath);
                    throw ex;
                }
            }
        }

        // Put the PDFs together into one PDF
        private static void ConcatenatePDFs(string outputFile, List<EmailPartFileRecord> files, out int pageCount)
        {
            // Keep the source documents open until the destination document is saved or the data will not be available
            using PdfPacket pdfPacket = new(files);

            try
            {
                using var destinationDoc = new PDDocument();

                foreach (var (document, documentNumber) in pdfPacket.Documents.Select((doc, i) => (doc, i + 1)))
                {
                    AddPagesToPDF(destinationDoc, documentNumber, document);
                }

                pageCount = destinationDoc.getNumberOfPages();

                destinationDoc.save(new java.io.File(outputFile));
                destinationDoc.close();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53242");
            }
        }

        // Add pages to destinationDoc
        // Add custom tags to the page with the logical document and page numbers
        private static void AddPagesToPDF(PDDocument destinationDoc, int childDocumentNumber, PDDocument sourceDocument)
        {
            try
            {
                var pages = sourceDocument.getPages().Cast<PDPage>();

                foreach (var (page, pageNumber) in pages.Select((page, i) => (page, i + 1)))
                {
                    PDPage importedPage = destinationDoc.importPage(page);

                    // Import 'inherited resources'
                    if (!page.getCOSObject().containsKey(org.apache.pdfbox.cos.COSName.RESOURCES) && page.getResources() != null)
                    {
                        importedPage.setResources(page.getResources());
                    }

                    // Add the logical document and page number so that it will be easy to create pagination VOA files later
                    var dict = importedPage.getCOSObject();
                    dict.setInt(LOGICAL_DOCUMENT_NUMBER_TAG, childDocumentNumber);
                    dict.setInt(LOGICAL_PAGE_NUMBER_TAG, pageNumber);
                }

            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53262");
            }
        }


        // Dependency for MimeFileSplitter, used to collect information about the email parts
        private sealed class FileCollectorDatabaseClient : IMimeFileSplitterDatabaseClient
        {
            private List<EmailPartFileRecord> _logicalFiles;
            private readonly List<EmailPartFileRecord> _database = new();

            /// <summary>
            /// Ordered list of output files (message body first) that were split from the email
            /// </summary>
            public List<EmailPartFileRecord> OutputFiles => _logicalFiles;

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
                    .Select(file => new
                    {
                        FileRecord = _database[file.DestinationFileID - 1],
                        ChildDocumentNumber = file.ChildDocumentNumber
                    })
                    .OrderBy(file => file.ChildDocumentNumber)
                    .Select(file => file.FileRecord)
                    .ToList();
            }

            /// <summary>
            /// Always returns true, stores the info for later
            /// </summary>
            public bool TryAddFileToDatabase(EmailPartFileRecord fileRecord, string temporaryFilePath, out int fileID)
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

        /// <summary>
        /// Keeps a list of PDF documents open until disposed
        /// </summary>
        private sealed class PdfPacket : IDisposable
        {
            /// <summary>
            /// Ordered list of PDF documents to be concatenated
            /// </summary>
            public IList<PDDocument> Documents { get; }

            /// <summary>
            /// Create an instance by opening all the source documents
            /// </summary>
            /// <param name="sourceDocuments">Ordered list of PDF records to be concatenated</param>
            public PdfPacket(IList<EmailPartFileRecord> sourceDocuments)
            {
                Documents = new List<PDDocument>();
                try
                {
                    foreach (var document in sourceDocuments)
                    {
                        Documents.Add(OpenPdf(document));
                    }
                }
                catch (Exception)
                {
                    Dispose();
                    throw;
                }
            }

            /// <summary>
            /// Close the documents
            /// </summary>
            public void Dispose()
            {
                foreach (var document in Documents)
                {
                    try
                    {
                        document.close();
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI53260");
                    }
                }
            }

            private static PDDocument OpenPdf(EmailPartFileRecord fileRecord)
            {
                try
                {
                    return PDDocument.load(new java.io.File(fileRecord.FilePath));
                }
                catch (Exception ex)
                {
                    if (fileRecord.IsAttachment)
                    {
                        var ee = new ExtractException("ELI53256", "Could not load email attachment", ex);
                        ee.AddDebugData("Email file", fileRecord.SourceEmailFileRecord.FilePath);
                        ee.AddDebugData("Attachment name", fileRecord.OriginalName);
                        ee.AddDebugData("Attachment number", fileRecord.AttachmentNumber);
                        throw ee;
                    }
                    else
                    {
                        var ee = new ExtractException("ELI53257", "Could not load email body", ex);
                        ee.AddDebugData("Email file", fileRecord.SourceEmailFileRecord.FilePath);
                        throw ee;
                    }
                }
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
