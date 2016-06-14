﻿using Extract.Imaging;
using Leadtools;
using Leadtools.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Represents a particular page of a document.
    /// </summary>
    internal class Page : IDisposable
    {
        #region Constants

        /// <summary>
        /// The default thumbnail image to use for an image that is not yet loaded.
        /// </summary>
        static readonly RasterImage _LOADING_IMAGE = RasterImageConverter.ConvertFromImage(
                Properties.Resources.Loading, ConvertFromImageOptions.None);

        #endregion Constants

        #region Fields

        /// <summary>
        /// A set of all objects currently referencing this instance.
        /// </summary>
        HashSet<object> _references = new HashSet<object>();

        /// <summary>
        /// The thumbnail <see cref="RasterImage"/> for this page.
        /// </summary>
        RasterImage _thumbnailImage;

        /// <summary>
        /// The current orientation of the image page relative to its original orientation in
        /// degrees.
        /// </summary>
        int _imageOrientation;

        /// <summary>
        /// <see langword="true"/> if this instance has been disposed; <see langword="false"/>
        /// otherwise.
        /// </summary>
        bool _isDisposed;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Page"/> class.
        /// </summary>
        /// <param name="sourceDocument">The <see cref="SourceDocument"/> this page is from.</param>
        /// <param name="pageNumber">The page number of this page in <see paramref="sourceDocument"/>.
        /// </param>
        public Page(SourceDocument sourceDocument, int pageNumber)
        {
            try
            {
                SourceDocument = sourceDocument;
                OriginalPageNumber = pageNumber;
                _thumbnailImage = _LOADING_IMAGE.Clone();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35423");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when the <see cref="ThumbnailImage"/> has changed.
        /// </summary>
        public event EventHandler<EventArgs> ThumbnailChanged;

        /// <summary>
        /// Raised when the orientation of the <see cref="ThumbnailImage"/> has changed.
        /// </summary>
        public event EventHandler<EventArgs> OrientationChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// The <see cref="SourceDocument"/> this page is from.
        /// </summary>
        public SourceDocument SourceDocument
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the document name this page originally came from.
        /// </summary>
        /// <value>
        /// The document name this page originally came from.
        /// </value>
        public string OriginalDocumentName
        {
            get
            {
                return SourceDocument.FileName;
            }
        }

        /// <summary>
        /// Gets the page number of the document this page originally came from.
        /// </summary>
        /// <value>
        /// The page number of the document this page originally came from.
        /// </value>
        public int OriginalPageNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the thumbnail image for this page.
        /// </summary>
        /// <value>
        /// The thumbnail image for this page.
        /// </value>
        public RasterImage ThumbnailImage
        {
            get
            {
                return _thumbnailImage;
            }

            set
            {
                try
                {
                    if (value != _thumbnailImage)
                    {
                        // To prevent any threading issues due to thumbnails loading on a background\
                        // thread, assign the new thumbnail image before disposing of the old one.
                        RasterImage oldThumbnailImage = _thumbnailImage;
                        _thumbnailImage = value.Clone();
                        if (oldThumbnailImage != null)
                        {
                            oldThumbnailImage.Dispose();
                        }

                        OnThumbnailChanged();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35424");
                }
            }
        }

        /// <summary>
        /// Gets or sets the orientation of the image page relative to its original orientation in
        /// degrees.
        /// </summary>
        /// <value>
        /// The orientation of the image page relative to its original orientation in degrees.
        /// </value>
        public int ImageOrientation
        {
            get
            {
                return _imageOrientation;
            }

            set
            {
                try
                {
                    if (value != _imageOrientation)
                    {
                        if (ThumbnailImage != null)
                        {
                            ImageMethods.RotateImageByDegrees(ThumbnailImage,
                                value - _imageOrientation);

                            OnOrientationChanged();
                        }

                        _imageOrientation = value;
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI35565");
                }
            }
        }

        /// <summary>
        /// Gets all <see cref="PageThumbnailControl"/>s currently referencing this instance.
        /// </summary>
        public IEnumerable<PageThumbnailControl> PageControlReferences
        {
            get
            {
                return _references.OfType<PageThumbnailControl>();
            }
        }

        /// <summary>
        /// Gets a value indicating whether any object has an active reference to this instance.
        /// </summary>
        /// <value><see langword="true"/> if there is an active reference; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool HasActiveReference
        {
            get
            {
                try
                {
                    return _references.Any();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35425");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether multiple (non-deleted) copies of this page exist
        /// in the UI.
        /// </summary>
        /// <value><see langword="true"/> if multiple copies exist; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool MultipleCopiesExist
        {
            get
            {
                try
                {
                    return _references
                        .OfType<PageThumbnailControl>()
                        .Where(c => !c.Deleted)
                        .Count() > 1;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38271");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value><see langword="true"/> if this instance is disposed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsDisposed
        {
            get
            {
                return _isDisposed;
            }
        }

        #endregion Properties

        #region Static Methods

        /// <summary>
        /// Gets whether the two specified <see cref="Page"/> sequences are equal in terms of their
        /// source document names, page numbers an page orientations.
        /// </summary>
        /// <param name="first">The first <see cref="Page"/> sequence to compare.</param>
        /// <param name="second">The second <see cref="Page"/> sequence to compare.</param>
        /// <returns></returns>
        static public bool PagesAreEqual(IEnumerable<Page> first, IEnumerable<Page> second)
        {
            try
            {
                return first
                    .Select(page => new Tuple<string, int, int>(
                        page.OriginalDocumentName, page.OriginalPageNumber, page.ImageOrientation))
                    .SequenceEqual(second.Select(page => new Tuple<string, int, int>(
                        page.OriginalDocumentName, page.OriginalPageNumber, page.ImageOrientation)));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39662");
            }
        }

        #endregion Static Methods

        #region Methods

        /// <summary>
        /// Adds a reference to this instance from <see paramref="o"/>
        /// </summary>
        /// <param name="o">The<see cref="object"/> that now references this instance.</param>
        public void AddReference(object o)
        {
            try
            {
                _references.Add(o);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35426");
            }
        }

        /// <summary>
        /// Removes the reference to this instance from <see paramref="o"/>
        /// </summary>
        /// <param name="o">The <see cref="object"/> that no longer references this instance.</param>
        public void RemoveReference(object o)
        {
            try
            {
                _references.Remove(o);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35427");
            }
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="Page"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="Page"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="Page"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    _isDisposed = true;

                    // Dispose of managed resources
                    if (_thumbnailImage != null)
                    {
                        _thumbnailImage.Dispose();
                        _thumbnailImage = null;
                    }
                }
                catch { }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Private Methods

        /// <summary>
        /// Raises the <see cref="ThumbnailChanged"/> event.
        /// </summary>
        void OnThumbnailChanged()
        {
            var eventHandler = ThumbnailChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="OrientationChanged"/> event.
        /// </summary>
        void OnOrientationChanged()
        {
            var eventHandler = OrientationChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        #endregion Private Methods
    }
}
