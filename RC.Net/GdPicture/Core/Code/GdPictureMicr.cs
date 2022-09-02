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
    public class GdPictureMicr : IDisposable
    {
        const string _MICR_CHARS = "0123456789ABCD";

        readonly Retry<Exception> _retry = new(5, 200);
        readonly GdPictureUtility _gdPictureUtil;
        bool _isDisposed;

        string? _currentDocumentPath;
        int _currentPageNumber;

        /// <summary>
        /// Create an instance, initilize GdPicture API instances
        /// </summary>
        public GdPictureMicr()
        {
            try
            {
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
        /// Load the specified image and search for MICR
        /// </summary>
        /// <param name="documentPath">Path to an image or PDF file</param>
        /// <param name="pagesToSearch">Optional list of page numbers to restrict the search to</param>
        /// <returns>A list of <see cref="Dto.TextAnnotation"/> objects, one per page searched</returns>
        public IList<Dto.TextAnnotation> FindMicrAsGcvCompatibleDto(string documentPath, IList<int>? pagesToSearch)
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
                return FindMicrOnPdfPages(pagesToSearchSet);
            }
            else
            {
                return FindMicrOnImagePages(pagesToSearchSet);
            }
        }

        /// <summary>
        /// Recognize MICR lines and write the result to a USS file
        /// </summary>
        /// <param name="imageFilePath">The path to the image to search for MICR</param>
        /// <param name="outputFilePath">The path to write the recognition results, defaults to imageFilePath.uss</param>
        public void CreateUssFile(string imageFilePath, string? outputFilePath = null)
        {
            CreateUssFileFromSpecifiedPages(imageFilePath, outputFilePath, null);
        }

        /// <summary>
        /// Recognize MICR lines and write the result to a USS file
        /// </summary>
        /// <param name="imageFilePath">The path to the image to search for MICR</param>
        /// <param name="outputFilePath">The path to write the recognition results, defaults to imageFilePath.uss</param>
        /// <param name="pagesToSearch">Optional list of page numbers to restrict the search to</param>
        public void CreateUssFileFromSpecifiedPages(string imageFilePath, string? outputFilePath, IList<int>? pagesToSearch)
        {
            try
            {
                ExtractException.Assert("ELI51542", "Input path cannot be empty", !String.IsNullOrEmpty(imageFilePath));
                outputFilePath ??= imageFilePath + ".uss";

                var pages = FindMicrAsGcvCompatibleDto(imageFilePath, pagesToSearch);

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

        // Search for MICR on a TIF or other image file
        IList<Dto.TextAnnotation> FindMicrOnImagePages(HashSet<int>? pagesToSearchSet)
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
                    pageCount = _gdPictureUtil.ImagingAPI.TiffGetPageCount(imageID);
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

                    if (ProcessPage(imageID) is Dto.TextAnnotation resultPage)
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

        // Search for MICR on a PDF
        IList<Dto.TextAnnotation> FindMicrOnPdfPages(HashSet<int>? pagesToSearchSet)
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

        // Search for MICR on the current page of the currently open PDF document
        Dto.TextAnnotation? ProcessPdfPage()
        {
            Dto.TextAnnotation? result = null;
            int imageID = 0;

            try
            {
                ThrowIfStatusNotOK(_gdPictureUtil.PdfAPI.SelectPage(_currentPageNumber), "ELI53519", "Unable to select page");

                imageID = _gdPictureUtil.PdfAPI.RenderPageToGdPictureImage(300, true);
                ThrowIfStatusNotOK(_gdPictureUtil.PdfAPI.GetStat(), "ELI53520", "Unable to render PDF page");

                result = ProcessPage(imageID);

                ThrowIfStatusNotOK(GdPictureDocumentUtilities.DisposeImage(imageID), "ELI53521", "Could not release image. Possible memory leak");
                imageID = 0;
            }
            finally
            {
                if (imageID > 0)
                {
                    LogIfStatusNotOK(() => GdPictureDocumentUtilities.DisposeImage(imageID), "ELI53522", "Could not release image. Possible memory leak");
                }
            }

            return result;
        }

        // Search for MICR on the currently selected page of the specified image ID
        Dto.TextAnnotation? ProcessPage(int imageID)
        {
            Dto.TextAnnotation? result = null;
            string stringResult;
            int orientation;

            try
            {
                ThrowIfStatusNotOK(_gdPictureUtil.OcrAPI.SetImage(imageID), "ELI51548", "Unable to set image for orientation detection");

                try
                {
                    orientation = _gdPictureUtil.OcrAPI.GetOrientation();
                    ThrowIfStatusNotOK(_gdPictureUtil.OcrAPI.GetStat(), "ELI51549", "Unable to get orientation");
                }
                catch (ExtractException uex)
                {
                    orientation = 0;
                    uex.Log();
                }
                finally
                {
                    // Clear OCR data after each page to improve memory usage
                    _gdPictureUtil.OcrAPI.ReleaseOCRResults();
                }

                if (orientation != 0)
                {
                    ThrowIfStatusNotOK(_gdPictureUtil.ImagingAPI.RotateAngle(imageID, 360 - orientation), "ELI51550", "Failed to rotate page");
                }

                stringResult = _gdPictureUtil.ImagingAPI.MICRDoMICR(imageID, MICRFont.MICRFontE13B, MICRContext.MICRContextDocument, _MICR_CHARS, ExpectedSymbols: 0);
                ThrowIfStatusNotOK(_gdPictureUtil.ImagingAPI.GetStat(), "ELI51541", "Unable to find MICR on page");

                if (!String.IsNullOrWhiteSpace(stringResult))
                {
                    result = MicrToGcvConverter.GetDto(_gdPictureUtil.ImagingAPI, imageID, _currentPageNumber, orientation, stringResult);
                }
            }
            catch (ExtractException uex) when (_gdPictureUtil.ImagingAPI.GetStat() == GdPictureStatus.GenericError)
            {
                // Skip the page so that the rest of the document can be searched
                // https://extract.atlassian.net/browse/ISSUE-18413
                new ExtractException("ELI53528", "Unable to process page with GdPicture. This page has been skipped.", uex).Log();
            }
            finally
            {
                // Clear MICR data after each page to improve memory usage
                _gdPictureUtil.ImagingAPI.MICRClear();
            }

            return result;
        }

        // Throw an exception if the status is not OK
        void ThrowIfStatusNotOK(GdPictureStatus status, string eliCode, string message)
        {
            GdPictureUtility.ThrowIfStatusNotOK(status, eliCode, message, new(_currentDocumentPath, _currentPageNumber));
        }

        // Log an exception if the status-generating function throws or the resulting status is not OK
        void LogIfStatusNotOK(Func<GdPictureStatus> statusFun, string eliCode, string message)
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
