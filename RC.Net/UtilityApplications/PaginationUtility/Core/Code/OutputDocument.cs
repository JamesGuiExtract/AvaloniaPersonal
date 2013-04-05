using Extract.Imaging;
using Extract.Utilities;
using Leadtools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Represents a potential output document as represented by a collection of
    /// <see cref="PageThumbnailControl"/>s.
    /// </summary>
    internal class OutputDocument
    {
        #region Fields

        /// <summary>
        /// The <see cref="PageThumbnailControl"/>s that represent the pages that are to comprise
        /// the document.
        /// </summary>
        List<PageThumbnailControl> _pageControls = new List<PageThumbnailControl>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputDocument"/> class.
        /// </summary>
        /// <param name="fileName">The filename that the document is to be saved as.</param>
        public OutputDocument(string fileName)
        {
            try
            {
                FileName = fileName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35548");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when the document is about to be output.
        /// </summary>
        public event EventHandler<CancelEventArgs> DocumentOutputting;

        /// <summary>
        /// Raised when the document is output.
        /// </summary>
        public event EventHandler<EventArgs> DocumentOutput;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets the <see cref="PageThumbnailControl"/>s that comprise the document to output.
        /// </summary>
        public ReadOnlyCollection<PageThumbnailControl> PageControls
        {
            get
            {
                try
                {
                    return _pageControls.AsReadOnly();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35549");
                }
            }
        }

        /// <summary>
        /// Gets or sets the filename that the document is to be saved as.
        /// </summary>
        /// <value>
        /// The filename that the document is to be saved as.
        /// </value>
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the document has changed compared to the input
        /// document for all the pages.
        /// </summary>
        /// <value><see langword="true"/> if in its original form; otherwise, <see langword="false"/>.
        /// </value>
        public bool InOriginalForm
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds the specified <see paramref="pageControl"/> as the last page of the document.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> representing the page
        /// to be added.</param>
        public void AddPage(PageThumbnailControl pageControl)
        {
            try
            {
                InsertPage(pageControl, _pageControls.Count + 1);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35550");
            }
        }

        /// <summary>
        /// Inserts the specified <see paramref="pageControl"/> as <see paramref="pageNumber"/> of
        /// the document.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> representing the page
        /// to be inserted.</param>
        /// <param name="pageNumber">The page number the new page should be inserted at.</param>
        public void InsertPage(PageThumbnailControl pageControl, int pageNumber)
        {
            try
            {
                ExtractException.Assert("ELI35551", "Invalid page number",
                    pageNumber > 0 && pageNumber <= _pageControls.Count + 1,
                    "Document", FileName, "Page", pageNumber);

                InOriginalForm = false;

                if (pageNumber <= _pageControls.Count)
                {
                    _pageControls.Insert(pageNumber - 1, pageControl);
                }
                else
                {
                    _pageControls.Add(pageControl);
                }

                pageControl.Document = this;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35552");
            }
        }

        /// <summary>
        /// Removes the specified <see paramref="pageControl"/> from the document.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> that is to be removed.</param>
        public void RemovePage(PageThumbnailControl pageControl)
        {
            try
            {
                ExtractException.Assert("ELI35553", "Null argument exception.", pageControl != null);

                InOriginalForm = false;

                _pageControls.Remove(pageControl);

                pageControl.Document = null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35554");
            }
        }

        /// <summary>
        /// Outputs the document to the current <see cref="FileName"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the document was output; otherwise
        /// <see langword="false"/>.</returns>
        public bool Output()
        {
            try
            {
                CancelEventArgs eventArgs = new CancelEventArgs();
                OnDocumentOutputting(eventArgs);
                if (eventArgs.Cancel)
                {
                    return false;
                }

                // Ensure the destination directory exists.
                string directory = Path.GetDirectoryName(FileName);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (InOriginalForm)
                {
                    // If the document has not been changed from its original form, it can simply be
                    // copied to _fileName rather than require it to be re-assembled.
                    File.Copy(_pageControls[0].Page.OriginalDocumentName, FileName);
                }
                else
                {
                    // Otherwise, generate a new document using the current PageControls as the
                    // document's pages.
                    using (ImageCodecs codecs = new ImageCodecs())
                    {
                        ImageWriter writer = null;
                        var readers = new Dictionary<string, ImageReader>();

                        try
                        {
                            foreach (Page page in PageControls.Select(pageControl => pageControl.Page))
                            {
                                // Get an image reader for the current page.
                                ImageReader reader;
                                if (!readers.TryGetValue(page.OriginalDocumentName, out reader))
                                {
                                    reader = codecs.CreateReader(page.OriginalDocumentName);
                                    readers[page.OriginalDocumentName] = reader;
                                }

                                // On the first page, generate a writer using the same format as the
                                // first page.
                                if (writer == null)
                                {
                                    writer = codecs.CreateWriter(FileName, reader.Format, false);
                                }

                                RasterImage pageImage = reader.ReadPage(page.OriginalPageNumber);
                                // Image must be rotated with forceTrueRotation to true, otherwise
                                // the output page is not rendered with the correct orientation
                                // (unclear why).
                                ImageMethods.RotateImageByDegrees(
                                    pageImage, page.ImageOrientation, true);
                                writer.AppendImage(pageImage);
                            }

                            writer.Commit(false);
                        }
                        finally
                        {
                            try
                            {
                                if (writer != null)
                                {
                                    writer.Dispose();
                                }

                                CollectionMethods.ClearAndDispose(readers);
                            }
                            catch (Exception ex)
                            {
                                ex.ExtractLog("ELI35555");
                            }
                        }
                    }
                }

                OnDocumentOutput();

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35556");
            }
        }

        #endregion Methods

        #region Private Members

        /// <summary>
        /// Raises the <see cref="DocumentOutputting"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="System.ComponentModel.CancelEventArgs"/> instance
        /// containing the event data.</param>
        void OnDocumentOutputting(CancelEventArgs eventArgs)
        {
            var eventHandler = DocumentOutputting;
            if (eventHandler != null)
            {
                eventHandler(this, eventArgs);
            }
        }

        /// <summary>
        /// Raises the <see cref="DocumentOutput"/> event.
        /// </summary>
        void OnDocumentOutput()
        {
            var eventHandler = DocumentOutput;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
