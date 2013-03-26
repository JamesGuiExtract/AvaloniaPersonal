using Extract.Imaging;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// 
    /// </summary>
    internal class OutputDocument
    {
        #region Fields

        /// <summary>
        /// 
        /// </summary>
        List<PageThumbnailControl> _pageControls = new List<PageThumbnailControl>();

        /// <summary>
        /// 
        /// </summary>
        string _fileName;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputDocument"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public OutputDocument(string fileName)
        {
            try
            {
                _fileName = fileName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35548");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<PageRemovedEventArgs> PageRemoved;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<EventArgs> DocumentOutput;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets the pages.
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
        ///
        /// </summary>
        public string FileName
        {
            get
            {
                return _fileName;
            }

            set
            {
                if (value != _fileName)
                {
                    _fileName = value;
                }
            }
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
        /// Adds the page.
        /// </summary>
        /// <param name="pageControl"></param>
        public void AddPage(PageThumbnailControl pageControl)
        {
            try
            {
                AddPage(pageControl, _pageControls.Count + 1);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35550");
            }
        }

        /// <summary>
        /// Adds the page.
        /// </summary>
        /// <param name="pageControl">The page.</param>
        /// <param name="pageNumber">The page number.</param>
        public void AddPage(PageThumbnailControl pageControl, int pageNumber)
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
        /// 
        /// </summary>
        /// <param name="pageControl"></param>
        /// <param name="deleted"></param>
        public void RemovePage(PageThumbnailControl pageControl, bool deleted)
        {
            try
            {
                ExtractException.Assert("ELI35553", "Null argument exception.", pageControl != null);

                InOriginalForm = false;

                _pageControls.Remove(pageControl);

                pageControl.Document = null;

                OnPageRemoved(pageControl.Page, deleted);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35554");
            }
        }

        /// <summary>
        /// Outputs this instance.
        /// </summary>
        public void Output()
        {
            try
            {
                string directory = Path.GetDirectoryName(_fileName);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (InOriginalForm)
                {
                    // If the document is copied in its present form, it can simply be copied to
                    // _fileName rather than require it to be re-assembled.
                    File.Copy(_pageControls[0].Page.OriginalDocumentName, _fileName);
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
                                ImageReader reader;
                                if (!readers.TryGetValue(page.OriginalDocumentName, out reader))
                                {
                                    reader = codecs.CreateReader(page.OriginalDocumentName);
                                    readers[page.OriginalDocumentName] = reader;
                                }

                                if (writer == null)
                                {
                                    writer = codecs.CreateWriter(FileName, reader.Format, false);
                                }

                                writer.AppendImage(reader.ReadPage(page.OriginalPageNumber));
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
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35556");
            }
        }

        #endregion Methods

        #region Private Members

        /// <summary>
        /// Called when [last page removed].
        /// </summary>
        /// <param name="removedPage"></param>
        /// <param name="deleted"></param>
        void OnPageRemoved(Page removedPage, bool deleted)
        {
            var eventHandler = PageRemoved;
            if (eventHandler != null)
            {
                eventHandler(this, new PageRemovedEventArgs(removedPage, deleted));
            }
        }

        /// <summary>
        /// Called when [document output].
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
