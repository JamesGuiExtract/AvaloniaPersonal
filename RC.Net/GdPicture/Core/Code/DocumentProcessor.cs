using Extract.GoogleCloud.Dto;
using Extract.Utilities;
using GdPicture14;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Extract.GdPicture
{
    public class DocumentProcessor : IDisposable, IDocumentContext
    {
        readonly Retry<Exception> _retry = new(5, 200);
        readonly GdPictureUtility _gdPictureUtil;
        readonly IPageProcessor _pageProcessor;

        bool _isDisposed;

        string? _currentDocumentPath;
        int _currentPageNumber;

        public GdPictureUtility GdPictureUtility => _gdPictureUtil;

        public int CurrentPageNumber => _currentPageNumber;

        /// <summary>
        /// Create an instance, initilize GdPicture API instances
        /// </summary>
        public DocumentProcessor(IPageProcessor pageProcessor)
        {
            try
            {
                _pageProcessor = pageProcessor ?? throw new ArgumentNullException(nameof(pageProcessor));

                _gdPictureUtil = new();
                _gdPictureUtil.ImagingAPI.TiffOpenMultiPageForWrite(false);
                _gdPictureUtil.ImagingAPI.GifOpenMultiFrameForWrite(false);
                _gdPictureUtil.OcrAPI.AddLanguage(OCRLanguage.English);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51529");
            }
        }

        /// <summary>
        /// Load the specified image and OCR
        /// </summary>
        /// <param name="documentPath">Path to an image or PDF file</param>
        /// <param name="pagesToSearch">Optional list of page numbers to restrict the processing to</param>
        /// <returns>A list of <see cref="Dto.TextAnnotation"/> objects, one per page processed</returns>
        public IList<Dto.TextAnnotation> RecognizeTextAsGcvCompatibleDto(string documentPath, IList<int>? pagesToSearch)
        {
            ExtractException.Assert("ELI51536", "Image path cannot be empty", !String.IsNullOrWhiteSpace(documentPath));
            _currentDocumentPath = documentPath;
            _currentPageNumber = 0;

            HashSet<int>? pagesToSearchSet = null;
            if (pagesToSearch != null)
            {
                pagesToSearchSet = new HashSet<int>(pagesToSearch);
            }

            if (_currentDocumentPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return ProcessPdfPages(pagesToSearchSet);
            }
            else
            {
                return ProcessImagePages(pagesToSearchSet);
            }
        }

        /// <summary>
        /// Recognize text and write the result to a USS file
        /// </summary>
        /// <param name="imageFilePath">The path to the image to process</param>
        /// <param name="outputFilePath">The path to write the recognition results, defaults to imageFilePath.uss</param>
        public void CreateUssFile(string imageFilePath, string? outputFilePath = null)
        {
            CreateUssFileFromSpecifiedPages(imageFilePath, outputFilePath, null);
        }

        /// <summary>
        /// Recognize text and write the result to a USS file
        /// </summary>
        /// <param name="imageFilePath">The path to the image to process</param>
        /// <param name="outputFilePath">The path to write the recognition results, defaults to imageFilePath.uss</param>
        /// <param name="pagesToSearch">Optional list of page numbers to restrict the search to</param>
        public void CreateUssFileFromSpecifiedPages(string imageFilePath, string? outputFilePath, IList<int>? pagesToSearch)
        {
            try
            {
                ExtractException.Assert("ELI51542", "Input path cannot be empty", !String.IsNullOrEmpty(imageFilePath));
                outputFilePath ??= imageFilePath + ".uss";

                var pages = RecognizeTextAsGcvCompatibleDto(imageFilePath, pagesToSearch);

                using var tempFile = new TemporaryFile(false);
                File.Delete(tempFile.FileName);
                using var zipArchive = ZipFile.Open(tempFile.FileName, ZipArchiveMode.Create);

                var entry = zipArchive.CreateEntry("0000.DocumentInfo.json");
                using (var entryStream = entry.Open())
                using (var sw = new StreamWriter(entryStream))
                {
                    var json = new JObject { { "SourceDocName", imageFilePath } };
                    sw.Write(json);
                }

                var jsonSerializer = GcvCompatibleConverter.GetJsonSerializer();
                foreach (var dto in pages)
                {
                    var page = dto.pages.SingleOrDefault()?.pageNumber;
                    if (page is int pageNumber)
                    {
                        string pageName = pageNumber.ToString("D4", CultureInfo.InvariantCulture);
                        entry = zipArchive.CreateEntry(pageName + ".GdPicture gcv v1.json");
                        using var entryStream = entry.Open();
                        using var streamWriter = new StreamWriter(entryStream);
                        using var jsonWriter = new JsonTextWriter(streamWriter);

                        jsonSerializer.Serialize(jsonWriter, dto);
                    }
                    else if (dto.pages.Length != 0)
                    {
                        throw new ExtractException("ELI51543", "Unexpected number of pages in DTO!");
                    }
                }

                zipArchive.Dispose();

                File.Copy(tempFile.FileName, outputFilePath, true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51544");
            }
        }

        // Recognize text on a TIF or other image file
        IList<Dto.TextAnnotation> ProcessImagePages(HashSet<int>? pagesToSearchSet)
        {
            int imageID = 0;
            try
            {
                _retry.DoRetry(() =>
                {
                    imageID = _gdPictureUtil.ImagingAPI.CreateGdPictureImageFromFile(_currentDocumentPath);
                    ThrowIfStatusNotOK(_gdPictureUtil.ImagingAPI.GetStat(), "ELI51537", "Image could not be loaded");
                });

                // GetPageCount returns 0 for single page TIFs
                bool isMultiPage = _gdPictureUtil.ImagingAPI.TiffIsMultiPage(imageID);
                int pageCount = 1;
                if (isMultiPage)
                {
                    pageCount = _gdPictureUtil.ImagingAPI.GetPageCount(imageID);
                }

                var resultPages = new List<Dto.TextAnnotation>();
                for (_currentPageNumber = 1; _currentPageNumber <= pageCount; _currentPageNumber++)
                {
                    if (pagesToSearchSet is not null && !pagesToSearchSet.Contains(_currentPageNumber))
                    {
                        continue;
                    }

                    if (isMultiPage)
                    {
                        ThrowIfStatusNotOK(_gdPictureUtil.ImagingAPI.TiffSelectPage(imageID, _currentPageNumber), "ELI51540", "Unable to select page");
                    }

                    if (_pageProcessor.ProcessPage(imageID, this) is Dto.TextAnnotation resultPage)
                    {
                        resultPages.Add(resultPage);
                    }
                }

                return resultPages;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51538");
            }
            finally
            {
                if (imageID > 0)
                {
                    LogIfStatusNotOK(() => GdPictureDocumentUtilities.DisposeImage(imageID), "ELI53522", "Could not release image. Possible memory leak");
                }
            }
        }

        // Recognize text on a PDF
        IList<Dto.TextAnnotation> ProcessPdfPages(HashSet<int>? pagesToSearchSet)
        {
            bool isPDFDocumentOpen = false;

            try
            {
                _retry.DoRetry(() =>
                {
                    ThrowIfStatusNotOK(_gdPictureUtil.PdfAPI.LoadFromFile(_currentDocumentPath), "ELI53517", "PDF could not be loaded");
                    isPDFDocumentOpen = true;
                });

                int pageCount = _gdPictureUtil.PdfAPI.GetPageCount();
                var resultPages = new List<Dto.TextAnnotation>();
                for (_currentPageNumber = 1; _currentPageNumber <= pageCount; _currentPageNumber++)
                {
                    if (pagesToSearchSet != null && !pagesToSearchSet.Contains(_currentPageNumber))
                    {
                        continue;
                    }

                    if (ProcessPdfPage() is Dto.TextAnnotation resultPage)
                    {
                        resultPages.Add(resultPage);
                    }
                }

                return resultPages;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51539");
            }
            finally
            {
                if (isPDFDocumentOpen)
                {
                    LogIfStatusNotOK(_gdPictureUtil.PdfAPI.CloseDocument, "ELI53523", "Could not close PDF. Possible memory leak");
                }
            }
        }

        // Recognize text on the current page of the currently open PDF document
        Dto.TextAnnotation? ProcessPdfPage()
        {
            Dto.TextAnnotation? result = null;
            int imageID = 0;

            try
            {
                ThrowIfStatusNotOK(_gdPictureUtil.PdfAPI.SelectPage(_currentPageNumber), "ELI53519", "Unable to select page");

                imageID = _gdPictureUtil.PdfAPI.RenderPageToGdPictureImage(300, true);
                ThrowIfStatusNotOK(_gdPictureUtil.PdfAPI.GetStat(), "ELI53520", "Unable to render PDF page");

                result = _pageProcessor.ProcessPage(imageID, this);

                ThrowIfStatusNotOK(GdPictureDocumentUtilities.DisposeImage(imageID), "ELI53521", "Could not release image. Possible memory leak");
                imageID = 0;
            }
            finally
            {
                if (imageID > 0)
                {
                    LogIfStatusNotOK(() => GdPictureDocumentUtilities.DisposeImage(imageID), "ELI53809", "Could not release image. Possible memory leak");
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public void ThrowIfStatusNotOK(GdPictureStatus status, string eliCode, string message)
        {
            GdPictureUtility.ThrowIfStatusNotOK(status, eliCode, message, new(_currentDocumentPath, _currentPageNumber));
        }

        /// <inheritdoc/>
        public void LogIfStatusNotOK(Func<GdPictureStatus> statusFun, string eliCode, string message)
        {
            GdPictureUtility.LogIfStatusNotOK(statusFun, eliCode, message, new(_currentDocumentPath, _currentPageNumber));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _gdPictureUtil.Dispose();
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
