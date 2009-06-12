using Extract;
using Extract.Drawing;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Leadtools;
using Leadtools.Annotations;
using Leadtools.Codecs;
using Leadtools.ImageProcessing;
using Leadtools.WinForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    partial class ImageViewer
    {
        #region Image Viewer Methods

        /// <summary>
        /// Establishes a connection with the specified control and any sub-controls that 
        /// implement the <see cref="IImageViewerControl"/> interface.
        /// </summary>
        /// <param name="control">The top-level control of all image viewer controls to which the 
        /// <see cref="ImageViewer"/> class should establish a connection.</param>
        /// <remarks>The <see cref="ImageViewer"/> control will pass itself to the specified 
        /// <paramref name="control"/> if it implements the <see cref="IImageViewerControl"/> 
        /// interface. It will also recursively pass itself to each child control of 
        /// <paramref name="control"/> and each <see cref="ToolStripItem"/> of each 
        /// <see cref="ToolStrip"/> of <paramref name="control"/>.</remarks>
        /// <seealso cref="IImageViewerControl"/>
        public void EstablishConnections(Control control)
        {
            try
            {
                // Check whether this control is an image viewer control
                IImageViewerControl imageViewerControl = control as IImageViewerControl;
                if (imageViewerControl != null)
                {
                    // Pass a reference to this image viewer
                    imageViewerControl.ImageViewer = this;
                }

                // Check whether this is a toolstrip control
                ToolStrip toolStrip = control as ToolStrip;
                if (toolStrip != null)
                {
                    // Iterate through each tool strip item
                    foreach (ToolStripItem toolStripItem in toolStrip.Items)
                    {
                        // Check whether this toolstrip item is an image viewer control
                        IImageViewerControl imageViewerItem =
                            toolStripItem as IImageViewerControl;
                        if (imageViewerItem != null)
                        {
                            // Pass a reference to this image viewer
                            imageViewerItem.ImageViewer = this;
                        }
                    }
                }

                // Check whether this is a menustrip control
                MenuStrip menuStrip = control as MenuStrip;
                if (menuStrip != null)
                {
                    // Iterate through each tool strip item
                    foreach (ToolStripItem toolStripItem in menuStrip.Items)
                    {
                        // Recursively establish connections with this item and its children
                        EstablishConnections(toolStripItem);
                    }
                }

                // Check each sub control of this control
                ControlCollection controls = control.Controls;
                foreach (Control subcontrol in controls)
                {
                    // Recursively establish connections with the sub control and its children
                    EstablishConnections(subcontrol);
                }

                // Recursively establish connections with the 
                // context menu strip associated with this control.
                if (control.ContextMenuStrip != null)
                {
                    EstablishConnections(control.ContextMenuStrip);
                }
            }
            catch (Exception e)
            {
                ExtractException ee = new ExtractException("ELI21116",
                    "Unable to establish connections.", e);
                ee.AddDebugData("Control", control, false);
                throw ee;
            }
        }

        /// <overloads>Establishes a connection with the specified component.</overloads>
        /// <summary>
        /// Establishes a connection with the specified tool strip item and any child tool strip 
        /// items that implement the <see cref="IImageViewerControl"/> interface.
        /// </summary>
        /// <param name="toolStripItem">The top-level tool strip item to which the 
        /// <see cref="ImageViewer"/> class should establish a connection.</param>
        /// <remarks>The <see cref="ImageViewer"/> control will pass itself to the specified 
        /// <paramref name="toolStripItem"/> if it implements the <see cref="IImageViewerControl"/> 
        /// interface. It will also recursively pass itself to each child tool strip item of 
        /// <paramref name="toolStripItem"/>.</remarks>
        /// <seealso cref="IImageViewerControl"/>
        public void EstablishConnections(ToolStripItem toolStripItem)
        {
            try
            {
                // Check whether this control is an image viewer control
                IImageViewerControl imageViewerControl = toolStripItem as IImageViewerControl;
                if (imageViewerControl != null)
                {
                    // Pass a reference to this image viewer
                    imageViewerControl.ImageViewer = this;
                }

                // Check whether this is a tool strip menu item
                ToolStripMenuItem menuItem = toolStripItem as ToolStripMenuItem;
                if (menuItem != null)
                {
                    // Iterate through each child tool strip item
                    foreach (ToolStripItem childItem in menuItem.DropDownItems)
                    {
                        // Recursively establish connections with this item and its children
                        EstablishConnections(childItem);
                    }
                }
            }
            catch (Exception e)
            {
                ExtractException ee = new ExtractException("ELI21597",
                    "Unable to establish connections.", e);
                ee.AddDebugData("Tool strip item", toolStripItem == null ? "null" :
                    toolStripItem.Name, false);
                throw ee;
            }
        }

        /// <summary>
        /// Opens the specified image file.
        /// </summary>
        /// <param name="fileName">The name of the image file to open. Cannot be 
        /// <see langword="null"/>.</param>
        /// <param name="updateMruList">Flag telling whether the MRU image file list
        /// should be updated or not.</param>
        /// <event cref="ImageFileChanged">Image file opened successfully.</event>
        /// <event cref="ZoomChanged">Image file opened successfully.</event>
        /// <exception cref="ExtractException"><paramref name="fileName"/> is 
        /// <see langword="null"/>.</exception>
        /// <exception cref="ExtractException">Unable to open image file.</exception>
        public void OpenImage(string fileName, bool updateMruList)
        {
            try
            {
                // Refresh the image viewer before opening the file [IDSD #145 - JDS]
                base.Invalidate();
                Application.DoEvents();

                // TODO: what should final state be if exceptions are thrown in different sections.

                // [IDSD:193] Convert to the full path name otherwise OCR will fail
                fileName = Path.GetFullPath(fileName);

                // Ensure the file name is not null
                ExtractException.Assert("ELI21218", "File name must be specified.",
                    !String.IsNullOrEmpty(fileName));

                // Ensure the file exists
                ExtractException.Assert("ELI21110", "File does not exist.", File.Exists(fileName),
                    "File name", fileName);

                // Raise the opening image event
                OpeningImageEventArgs opening = new OpeningImageEventArgs(fileName, updateMruList);
                OnOpeningImage(opening);

                // Check if the event was cancelled
                if (opening.Cancel)
                {
                    return;
                }

                // Close the currently open image without raising
                // the image file changed event.
                if (base.IsImageAvailable && !CloseImage(false))
                {
                    return;
                }

                // Raise the LoadingNewImage event
                OnLoadingNewImage(new LoadingNewImageEventArgs());

                // Refresh the image viewer before opening the new image
                this.RefreshImageViewerAndParent();

                if (!FileSystemMethods.TryOpenExclusive(fileName,
                    out _currentOpenFile))
                {
                    MessageBox.Show("The file \"" + fileName + "\" is currently open by another program"
                        + " on this machine, or by another user on a different machine."
                        + Environment.NewLine + Environment.NewLine
                        + "To allow efficient operation in a multi-user environment "
                        + Application.ProductName + " has been designed to only open files it can"
                        + " obtain exclusive access to.", "File Is Locked", MessageBoxButtons.OK,
                        MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);

                    // Raise the image file changed event and return
                    OnImageFileChanged(new ImageFileChangedEventArgs(""));
                    return;
                }

                using (new TemporaryWaitCursor())
                {
                    // Suspend the paint event until after all changes have been made
                    base.BeginUpdate();
                    try
                    {
                        // Reset the valid annotations flag
                        _validAnnotations = true;

                        // Initialize a new RasterCodecs object
                        _codecs = new RasterCodecs();
                        _codecs.Options.Tiff.Load.IgnoreViewPerspective = true;
                        _codecs.Options.Pdf.Save.UseImageResolution = true;
                        _codecs.Options.Pdf.InitialPath = GetPdfInitializationDirectory();

                        // Leadtools does not support anti-aliasing for non-bitonal pdfs.
                        // If anti-aliasing is set, load pdfs as a bitonal image with high dpi.
                        if (_useAntiAliasing)
                        {
                            // Load as bitonal for anti-aliasing
                            _codecs.Options.Pdf.Load.DisplayDepth = 1;

                            // Use high dpi to preserve image quality
                            _codecs.Options.Pdf.Load.XResolution = 300;
                            _codecs.Options.Pdf.Load.YResolution = 300;
                        }

                        // Get a stream of bytes from the file
                        // Load the new image file
                        RasterImage newImage = _codecs.Load(_currentOpenFile);

                        // Create the page data for this image
                        _imagePages = new List<ImagePageData>(newImage.PageCount);
                        for (int i = 0; i < newImage.PageCount; i++)
                        {
                            _imagePages.Add(new ImagePageData());
                        }

                        // Display the first page
                        base.Image = newImage;

                        // Store the image file name
                        _imageFile = fileName;

                        // If a fit mode is not specified, display the whole page 
                        // [DotNetRCAndUtils #102]
                        if (_fitMode == FitMode.None)
                        {
                            ShowFitToPage();
                        }

                        // Update annotations if they need to be updated
                        UpdateAnnotations();

                        // Set the cursor tool to zoom window if none is set
                        if (_cursorTool == CursorTool.None)
                        {
                            this.CursorTool = CursorTool.ZoomWindow;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_currentOpenFile != null)
                        {
                            _currentOpenFile.Dispose();
                            _currentOpenFile = null;
                        }

                        throw ExtractException.AsExtractException("ELI23376", ex);
                    }
                    finally
                    {
                        // Post the paint event
                        base.EndUpdate();
                    }
                }

                // Update the MRU list if needed
                if (updateMruList)
                {
                    RegistryManager.AddMostRecentlyUsedImageFile(fileName);
                }

                // Raise the image file changed event
                OnImageFileChanged(new ImageFileChangedEventArgs(fileName));

                // Raise the on page changed event
                OnPageChanged(new PageChangedEventArgs(1));

                // Update the zoom history and raise the zoom changed event
                UpdateZoom(true, true);
            }
            catch (Exception e)
            {
                try
                {
                    // If updating MRU list then on error need to remove the file name from
                    // the list
                    if (updateMruList)
                    {
                        RegistryManager.RemoveMostRecentlyUsedImageFile(fileName);
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI21588",
                        "Unable to remove file from MRU list!", ex);
                    ee.AddDebugData("File to remove", fileName, false);
                    ee.Log();
                }

                ExtractException ee2 = new ExtractException("ELI21117", "Unable to open image.", e);
                ee2.AddDebugData("File name", fileName, false);

                // Raise the FileOpenError event
                FileOpenErrorEventArgs eventArgs = new FileOpenErrorEventArgs(fileName, ee2);
                OnFileOpenError(eventArgs);

                // Raise the image file changed event to update statuses as necessary.
                OnImageFileChanged(new ImageFileChangedEventArgs(""));

                if (!eventArgs.Cancel)
                {
                    // The event was not handled, go ahead and throw the exception.
                    throw ee2;
                }
            }
        }

        /// <summary>
        /// Closes the currently open image and raises the
        /// <see cref="ImageFileChanged"/> event.
        /// <para><b>Requirement:</b></para>
        /// An image must be open (<see cref="RasterImageViewer.IsImageAvailable"/>
        /// must be <see langword="true"/>).
        /// </summary>
        /// <exception cref="ExtractException">If there is no currently open image.</exception>
        public void CloseImage()
        {
            try
            {
                // Ensure an image is open
                if (base.IsImageAvailable)
                {
                    CloseImage(true);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23311", ex);
            }
        }

        /// <summary>
        /// Closes the currently open image.
        /// <para><b>Requirement:</b></para>
        /// An image must be open (<see cref="RasterImageViewer.IsImageAvailable"/>
        /// must be <see langword="true"/>).
        /// </summary>
        /// <param name="raiseImageFileChangedEvent">If <see langword="true"/> will raise
        /// the <see cref="ImageFileChanged"/> event; if <see langword="false"/> will not
        /// raise the <see cref="ImageFileChanged"/> event.</param>
        /// <exception cref="ExtractException">If there is no currently open image.</exception>
        /// <returns><see langword="true"/> If the image was closed; <see langword="false"/>
        /// if the closing event was canceled.</returns>
        private bool CloseImage(bool raiseImageFileChangedEvent)
        {
            try
            {
                ExtractException.Assert("ELI22337", "An image must be open!",
                    base.IsImageAvailable);

                // If there is an open image, raise the image file closing event
                if (!string.IsNullOrEmpty(_imageFile))
                {
                    // Raise the image file closing event
                    ImageFileClosingEventArgs closing = new ImageFileClosingEventArgs(_imageFile);
                    OnImageFileClosing(closing);

                    // Check if the user canceled the event
                    if (closing.Cancel)
                    {
                        return false;
                    }
                }

                // Suspend the paint event until after all changes have been made
                base.BeginUpdate();
                try
                {
                    // Dispose of all layer objects
                    _layerObjects.Clear();

                    // Dispose of any previously held codecs
                    if (_codecs != null)
                    {
                        _codecs.Dispose();
                        _codecs = null;
                    }

                    // Set Image to null
                    base.Image = null;

                    // Dispose the open file stream (if it is not null)
                    if (_currentOpenFile != null)
                    {
                        _currentOpenFile.Dispose();
                        _currentOpenFile = null;
                    }

                    // Set image file name to empty string
                    _imageFile = "";

                    // Update the annotations
                    UpdateAnnotations();

                    if (raiseImageFileChangedEvent)
                    {
                        // Raise the image file changed event
                        OnImageFileChanged(new ImageFileChangedEventArgs(""));
                    }

                    return true;
                }
                finally
                {
                    // Post the paint event
                    base.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22338", ex);
            }
        }

        /// <summary>
        /// Saves the image to the specified file.
        /// </summary>
        /// <param name="fileName">The full path of the output image file.
        /// <para><b>Requirement:</b></para>
        /// <paramref name="fileName"/> must not be equal to <see cref="ImageFile"/>.</param>
        /// <param name="format">The file format for the output image.</param>
        /// <exception cref="ExtractException">If <paramref name="fileName"/> is equal
        /// to <see cref="ImageFile"/>.</exception>
        public void SaveImage(string fileName, RasterImageFormat format)
        {
            TemporaryWaitCursor waitCursor = null;
            RasterImage image = null;
            int originalPage = base.Image.Page;
            Matrix transform = null;
            IntPtr hdc = IntPtr.Zero;
            Graphics graphics = null;
            try
            {
                // Ensure not saving to the currently open image
                ExtractException.Assert("ELI23374", "Cannot save to the currently open image!",
                    !fileName.Equals(_imageFile, StringComparison.OrdinalIgnoreCase));

                // Set the wait cursor
                waitCursor = new TemporaryWaitCursor();

                // Determine whether the output format is a tiff
                bool isTiff = IsTiff(format);

                // Check if annotations will be stored as tiff tags
                AnnCodecs annCodecs = null;
                Dictionary<int, RasterTagMetadata> tags = null;
                if (_displayAnnotations && isTiff)
                {
                    // Prepare to store tiff tags
                    annCodecs = new AnnCodecs();
                    tags = new Dictionary<int, RasterTagMetadata>(base.Image.PageCount);
                }

                // Iterate through each page of the image
                ColorResolutionCommand command = null; // Used to change bpp of image
                for (int i = 1; i <= base.Image.PageCount; i++)
                {
                    // Go to the specified page
                    base.Image.Page = i;
                    UpdateAnnotations();

                    // Clone the image
                    RasterImage page = base.Image.Clone();

                    // Sometimes the call to CreateLeadDC can return an IntPtr.Zero, in this
                    // case we should dispose of the page, call garbage collection, reclone
                    // the page and retry the CreateLeadDC call [IDSD #331]
                    int retries = 0;
                    for (; retries < _SAVE_RETRY_COUNT; retries++)
                    {
                        // Convert any image that is less than 16 bpp to 16 bpp
                        if (page.BitsPerPixel < 16)
                        {
                            if (command == null)
                            {
                                command = GetSixteenBppConverter();
                            }

                            command.Run(page);
                        }

                        // Get a handle to a device context
                        hdc = page.CreateLeadDC();

                        // If successful, just break from loop
                        if (hdc != IntPtr.Zero)
                        {
                            break;
                        }

                        // Dispose of the page and call garbage collector
                        page.Dispose();
                        page = null;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        // Reclone the page and convert as necessary
                        page = base.Image.Clone();
                    }

                    // If it was successful, but only after a retry, log an exception
                    if (hdc != IntPtr.Zero && retries > 0)
                    {
                        ExtractException ee = new ExtractException("ELI23377",
                            "Device context created successfully after retry");
                        ee.AddDebugData("Retries attempted", retries, false);
                        ee.AddDebugData("Image file name", _imageFile, false);
                        ee.AddDebugData("Current page", i, false);
                        ee.AddDebugData("Total pages", base.Image.PageCount, false);
                        ee.AddDebugData("Original bpp", base.Image.BitsPerPixel, false);
                        ee.AddDebugData("Current bpp", page.BitsPerPixel, false);
                        ee.Log();
                    }
                    else if(hdc == IntPtr.Zero)
                    {
                        // Throw an exception
                        ExtractException ee = new ExtractException("ELI23378",
                            "Unabled to create device context");
                        ee.AddDebugData("Image file name", _imageFile, false);
                        ee.AddDebugData("Current page", i, false);
                        ee.AddDebugData("Total pages", base.Image.PageCount, false);
                        ee.AddDebugData("Original bpp", base.Image.BitsPerPixel, false);
                        ee.AddDebugData("Current bpp", page.BitsPerPixel, false);
                        ee.AddDebugData("Current memory false", GC.GetTotalMemory(false), false);
                        ee.AddDebugData("Current memory true", GC.GetTotalMemory(true), false);
                        ee.AddDebugData("Retry Count", _SAVE_RETRY_COUNT, false);
                        throw ee;
                    }

                    // Create a graphics object from the handle
                    graphics = Graphics.FromHdc(hdc);

                    // Calculate the rotated transform for this page
                    transform = GetRotatedMatrix(graphics.Transform, false);

                    // Render the annotations
                    if (_annotations != null)
                    {
                        // Rotate the annotations
                        _annotations.Transform = graphics.Transform.Clone();
                        RotateAnnotations();

                        // Check whether the output image is a tiff
                        if (isTiff)
                        {
                            // Save annotations in a tiff tag
                            RasterTagMetadata tag =
                                annCodecs.SaveToTag(_annotations, AnnCodecsTagFormat.Tiff);
                            tags.Add(i, tag);
                        }
                        else
                        {
                            // Burn the annotations directly onto the output image
                            _annotations.Draw(graphics);
                        }
                    }

                    // Render the layer objects
                    foreach (LayerObject layerObject in _layerObjects)
                    {
                        if (layerObject.PageNumber == i)
                        {
                            layerObject.Render(graphics, transform);
                        }
                    }

                    // Apply the watermark if necessary
                    if (_watermark != null)
                    {
                        DrawWatermark(page, graphics);
                    }

                    // Dispose of the transform
                    transform.Dispose();

                    // Release the device context
                    RasterImage.DeleteLeadDC(hdc);

                    // Dispose of the graphics object
                    graphics.Dispose();

                    // Add the page to the resultant image
                    if (image == null)
                    {
                        image = page;
                    }
                    else
                    {
                        image.AddPage(page);
                    }
                }
                transform = null;
                hdc = IntPtr.Zero;
                graphics = null;

                // Save the file
                _codecs.Save(image, fileName, format, isTiff ? 0 : 1, 1, -1, 1,
                    CodecsSavePageMode.Overwrite);

                // Check if tiff annotation tags should be added
                if (isTiff && _displayAnnotations)
                {
                    // Write each tag to its respective page
                    foreach (KeyValuePair<int, RasterTagMetadata> pageToTag in tags)
                    {
                        _codecs.WriteTag(fileName, pageToTag.Key, pageToTag.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22902", ex);
            }
            finally
            {
                // Restore the original page number
                base.Image.Page = originalPage;
                UpdateAnnotations();

                // Dispose of the resources
                if (image != null)
                {
                    image.Dispose();
                }
                if (waitCursor != null)
                {
                    waitCursor.Dispose();
                }
                if (transform != null)
                {
                    transform.Dispose();
                }
                if (hdc != IntPtr.Zero)
                {
                    RasterImage.DeleteLeadDC(hdc);
                }
                if (graphics != null)
                {
                    graphics.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets a ColorResolutionCommand to convert an image to 16 bpp.
        /// </summary>
        /// <returns></returns>
        private static ColorResolutionCommand GetSixteenBppConverter()
        {
            ColorResolutionCommand command = new ColorResolutionCommand();
            command.BitsPerPixel = 16;
            command.Mode = ColorResolutionCommandMode.InPlace;
            return command;
        }

        /// <summary>
        /// Permanently rotates the <see cref="_annotations"/> based on the current 
        /// <see cref="Orientation"/>.
        /// </summary>
        void RotateAnnotations()
        {
            // Get the amount to translate the origin
            PointF offset = PointF.Empty;
            int orientation = _imagePages[base.Image.Page - 1].Orientation;
            switch (orientation)
            {
                case 0:
                    break;

                case 90:
                    offset = new PointF(base.Image.ImageWidth, 0);
                    break;

                case 180:
                    offset =
                        new PointF(base.Image.ImageWidth, base.Image.ImageHeight);
                    break;

                case 270:
                    offset = new PointF(0, base.Image.ImageHeight);
                    break;

                default:
                    break;
            }

            // Rotate each annotation
            foreach (AnnObject annotation in _annotations.Objects)
            {
                annotation.Rotate(orientation, AnnPoint.Empty);
                annotation.Translate(offset.X, offset.Y);
            }
        }

        /// <summary>
        /// Draws a watermark on the specified page.
        /// </summary>
        /// <param name="page">The page on which the watermark should appear.</param>
        /// <param name="graphics">The graphics object with which the watermark will be drawn.
        /// </param>
        void DrawWatermark(RasterImage page, Graphics graphics)
        {
            // Calculate the font size needed
            float minPixels = Math.Min(page.Width, page.Height);
            SizeF fontSize = new SizeF(minPixels, minPixels);

            // Get the font that will fit exactly within the minimum font size
            using (Font font = GetFontThatFits(_watermark, graphics, fontSize))
            {
                // Calculate the center point of the watermark in image coordinates
                SizeF size = graphics.MeasureString(_watermark, font);
                PointF center = new PointF((page.Width - size.Width) / 2F,
                    (page.Height - size.Height) / 2F); 

                // Center the watermark text
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;

                // Draw the watermark
                DrawingMethods.DrawRotatedString(graphics, _watermark, font, Brushes.DimGray,
                    center, -45);
            }
        }

        /// <summary>
        /// Gets a font the fits exactly within the specified size.
        /// </summary>
        /// <param name="text">The text to fit.</param>
        /// <param name="graphics">The graphics object that will draw the font.</param>
        /// <param name="size">The size into which the font should fit.</param>
        /// <returns>A font that fits exactly within the specified size.</returns>
        static Font GetFontThatFits(string text, Graphics graphics, SizeF size)
        {
            // Guess the size needed
            float guess = Math.Min(size.Width, size.Height) * 1.2F / text.Length;

            // Create a font to use to test the guess
            using (Font font = new Font(FontFamily.GenericSerif, guess, FontStyle.Bold, 
                GraphicsUnit.Pixel))
            {
                // Measure the size of the font
                SizeF guessSize = graphics.MeasureString(text, font);

                // Calculate how far from the desired size the guess was
                SizeF scale = new SizeF(size.Width / guessSize.Width,
                    size.Height / guessSize.Height);

                // Calculate the actual font size
                float actual = (scale.Height < scale.Width) ?
                    scale.Height * guess : scale.Width * guess;

                // Return the actual font
                return new Font(FontFamily.GenericSerif, actual, FontStyle.Bold, 
                    GraphicsUnit.Pixel);
            }
        }

        /// <summary>
        /// Determines whether the specified format is a tagged image file format (TIFF).
        /// </summary>
        /// <param name="format">The image format to evaluate.</param>
        /// <returns><see langword="true"/> if <paramref name="format"/> is a tagged image file 
        /// format; <see langword="false"/> if the <paramref name="format"/> is not a tagged image 
        /// file format.</returns>
        static bool IsTiff(RasterImageFormat format)
        {
            switch (format)
            {
                case RasterImageFormat.Ccitt:
                case RasterImageFormat.CcittGroup31Dim:
                case RasterImageFormat.CcittGroup32Dim:
                case RasterImageFormat.CcittGroup4:
                case RasterImageFormat.GeoTiff:
                case RasterImageFormat.IntergraphCcittG4:
                case RasterImageFormat.RawCcitt:
                case RasterImageFormat.Tif:
                case RasterImageFormat.TifAbc:
                case RasterImageFormat.TifAbic:
                case RasterImageFormat.TifCmp:
                case RasterImageFormat.TifCmw:
                case RasterImageFormat.TifCmyk:
                case RasterImageFormat.TifCustom:
                case RasterImageFormat.TifDxf:
                case RasterImageFormat.TifJ2k:
                case RasterImageFormat.TifJbig:
                case RasterImageFormat.TifJbig2:
                case RasterImageFormat.TifJpeg:
                case RasterImageFormat.TifJpeg411:
                case RasterImageFormat.TifJpeg422:
                case RasterImageFormat.TifLead1Bit:
                case RasterImageFormat.TifLeadMrc:
                case RasterImageFormat.TifLzw:
                case RasterImageFormat.TifLzwCmyk:
                case RasterImageFormat.TifLzwYcc:
                case RasterImageFormat.TifMrc:
                case RasterImageFormat.TifPackBits:
                case RasterImageFormat.TifPackBitsCmyk:
                case RasterImageFormat.TifPackbitsYcc:
                case RasterImageFormat.TifUnknown:
                case RasterImageFormat.TifYcc:
                case RasterImageFormat.TifZip:
                case RasterImageFormat.TifxFaxG31D:
                case RasterImageFormat.TifxFaxG32D:
                case RasterImageFormat.TifxFaxG4:
                case RasterImageFormat.TifxJbig:
                case RasterImageFormat.TifxJbigT43:
                case RasterImageFormat.TifxJbigT43Gs:
                case RasterImageFormat.TifxJbigT43ItuLab:
                case RasterImageFormat.TifxJpeg:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Goes to the previous zoom tile.
        /// </summary>
        /// <event cref="ZoomChanged">The method is successful.</event>
        /// <exception cref="ExtractException">No image is open.</exception>
        /// <exception cref="ExtractException"><see cref="IsFirstTile"/> is <see langword="false"/>.
        /// </exception>
        /// <seealso cref="IsFirstTile"/>
        public void PreviousTile()
        {
            try
            {
                // Ensure an image is open
                if (base.Image == null)
                {
                    throw new ExtractException("ELI21854", "No image is open.");
                }

                // Get the visible image area
                Rectangle tileArea = GetVisibleImageArea();

                // Determine the distance in physical (client) coordinates between 
                // the left side of the viewing tile and the left edge of the image.
                Rectangle imageArea = base.PhysicalViewRectangle;
                int deltaX = tileArea.Left - imageArea.Left;

                // Suspend paint events until the next tile is shown
                base.BeginUpdate();
                try
                {
                    // Check if the edge of the image has been reached
                    if (deltaX < _TILE_EDGE_DISTANCE)
                    {
                        // Determine the distance in physical (client) coordinates between
                        // the top of the viewing tile and the top of the image.
                        int deltaY = tileArea.Top - imageArea.Top;

                        // Check if the top of the page has been reached
                        if (deltaY < _TILE_EDGE_DISTANCE)
                        {
                            // Ensure this is not the first tile
                            int currentPage = base.Image.Page;
                            if (currentPage == 1)
                            {
                                throw new ExtractException("ELI21855",
                                    "Cannot move before first tile.");
                            }

                            // Go to the previous page
                            SetPageNumber(currentPage - 1, false, true);

                            // Move the tile area to the bottom right of new page
                            imageArea = base.PhysicalViewRectangle;
                            tileArea.Offset(imageArea.Right, imageArea.Bottom);
                        }
                        else
                        {
                            // Move the tile to the previous row of tiles
                            tileArea.Offset(imageArea.Right, -Math.Min(tileArea.Height, deltaY));
                        }
                    }
                    else
                    {
                        // Move the tile horizontally to the previous area
                        tileArea.Offset(-Math.Min(tileArea.Width, deltaX), 0);
                    }

                    // Move to the next tile, add to the zoom history, and raise ZoomChanged event
                    ZoomToRectangle(tileArea, true, true, false);
                }
                finally
                {
                    base.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI21856",
                    "Unable to go to previous tile.", ex);
            }
        }

        /// <summary>
        /// Goes to the next zoom tile.
        /// </summary>
        /// <event cref="ZoomChanged">The method is successful.</event>
        /// <exception cref="ExtractException">No image is open.</exception>
        /// <exception cref="ExtractException"><see cref="IsLastTile"/> is <see langword="false"/>.
        /// </exception>
        /// <seealso cref="IsLastTile"/>
        public void NextTile()
        {
            try
            {
                // Ensure an image is open
                if (base.Image == null)
                {
                    throw new ExtractException("ELI21845", "No image is open.");
                }

                // Get the visible image area
                Rectangle tileArea = GetVisibleImageArea();

                // Determine the distance in physical (client) coordinates between 
                // the right side of the viewing tile and the right edge of the image.
                Rectangle imageArea = base.PhysicalViewRectangle;
                int deltaX = imageArea.Right - tileArea.Right;

                // Suspend paint events until the next tile is shown
                base.BeginUpdate();
                try
                {
                    // Check if the edge of the image has been reached
                    if (deltaX < _TILE_EDGE_DISTANCE)
                    {
                        // Determine the distance in physical (client) coordinates between
                        // the bottom of the viewing tile and the bottom of the image.
                        int deltaY = imageArea.Bottom - tileArea.Bottom;

                        // Check if the bottom of the page has been reached
                        if (deltaY < _TILE_EDGE_DISTANCE)
                        {
                            // Ensure this is not the last tile
                            int currentPage = base.Image.Page;
                            if (currentPage == base.Image.PageCount)
                            {
                                throw new ExtractException("ELI21846",
                                    "Cannot advance past last tile.");
                            }

                            // Go to the next page
                            SetPageNumber(currentPage + 1, false, true);

                            // Move the tile area to the top-left of the new page
                            tileArea.Offset(base.PhysicalViewRectangle.Location);
                        }
                        else
                        {
                            // Move the tile to the next row of tiles
                            tileArea.Offset(imageArea.Left, Math.Min(tileArea.Height, deltaY));
                        }
                    }
                    else
                    {
                        // Move the tile horizontally to the next area
                        tileArea.Offset(Math.Min(tileArea.Width, deltaX), 0);
                    }

                    // Move to the next tile, add to the zoom history, and raise ZoomChanged event
                    ZoomToRectangle(tileArea, true, true, false);
                }
                finally
                {
                    base.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI21844",
                    "Unable to go to next tile.", ex);
            }
        }

        /// <summary>
        /// Activates the next layer object command (with object selection enabled)
        /// </summary>
        public void GoToNextLayerObject()
        {
            // Ensure it is possible to go to the next layer object.
            if (base.IsImageAvailable && CanGoToNextLayerObject)
            {
                // Go to the next layer object.
                GoToNextVisibleLayerObject(true);
            }
        }

        /// <summary>
        /// Moves to the next visible <see cref="LayerObject"/> and centers the view
        /// on the object. If <paramref name="selectObject"/> is <see langword="true"/> will
        /// clear the current selection and select the next <see cref="LayerObject"/>.
        /// </summary>
        /// <param name="selectObject">If <see langword="true"/> will
        /// clear the current selection and select the next <see cref="LayerObject"/>.</param>
        public void GoToNextVisibleLayerObject(bool selectObject)
        {
            // Get the next visible layer object
            LayerObject nextLayerObject = this.GetNextVisibleLayerObject();

            if (nextLayerObject != null)
            {
                this.CenterOnLayerObject(nextLayerObject, true);

                if (selectObject)
                {
                    // Clear the current selection
                    _layerObjects.Selection.Clear();

                    // Make this object the selection object
                    nextLayerObject.Selected = true;
                }
            }
        }

        /// <summary>
        /// Activates the previous layer object command (with object selection enabled)
        /// </summary>
        public void GoToPreviousLayerObject()
        {
            // Ensure it is possible to go to the Previous layer object.
            if (base.IsImageAvailable && CanGoToPreviousLayerObject)
            {
                // Go to the previous layer object.
                GoToPreviousVisibleLayerObject(true);
            }
        }

        /// <summary>
        /// Moves to the previous visible <see cref="LayerObject"/> and centers the view
        /// on the object. If <paramref name="selectObject"/> is <see langword="true"/> will
        /// clear the current selection and select the previous <see cref="LayerObject"/>.
        /// </summary>
        /// <param name="selectObject">If <see langword="true"/> will
        /// clear the current selection and select the previous <see cref="LayerObject"/>.</param>
        public void GoToPreviousVisibleLayerObject(bool selectObject)
        {
            // Get the next visible layer object
            LayerObject previousLayerObject = this.GetPreviousVisibleLayerObject();

            if (previousLayerObject != null)
            {
                this.CenterOnLayerObject(previousLayerObject, true);

                if (selectObject)
                {
                    // Clear the current selection
                    _layerObjects.Selection.Clear();

                    // Make this object the selection object
                    previousLayerObject.Selected = true;
                }
            }
        }

        /// <summary>
        /// Centers the view on the specified <see cref="LayerObject"/>.
        /// </summary>
        /// <param name="layerObject">The <see cref="LayerObject"/> to center on.</param>
        /// <param name="adjustZoomToFitObject">If <see langword="true"/> will adjust the
        /// zoom if necessary to fit the whole object in view; if <see langword="false"/>
        /// will not adjust the zoom to fit the whole object in view.</param>
        public void CenterOnLayerObject(LayerObject layerObject, bool adjustZoomToFitObject)
        {
            try
            {
                ExtractException.Assert("ELI22430", "Object cannot be null!",
                    layerObject != null);

                // Get the current zoom info
                ZoomInfo zoomInfo = this.GetZoomInfo();

                // Set the image viewer to the proper page for this object
                this.PageNumber = layerObject.PageNumber;

                // Set the appropriate scale factor if necessary
                if (zoomInfo.FitMode != FitMode.FitToPage)
                {
                    // Get the center point for the object
                    Point[] centerPoint = new Point[] {
                        layerObject.GetCenterPoint() };

                    // Set the center point
                    zoomInfo.Center = centerPoint[0];

                    // Center the view on the layer objects center point
                    this.SetZoomInfo(zoomInfo, zoomInfo.FitMode == FitMode.None);

                    // Check if the zoom should be adjusted to fit the object
                    if (adjustZoomToFitObject)
                    {
                        // Get the current view rectangle
                        // Do not pad the viewing rectangle [DNRCAU #282]
                        Rectangle viewRectangle =
                            this.GetTransformedRectangle(this.GetVisibleImageArea(), true);

                        // Check if the object is in view
                        if (!layerObject.IsContained(viewRectangle, base.Image.Page))
                        {
                            // Object is not in view, adjust the zoom so that the object
                            // is in view (also update zoom history and change fit mode
                            // if necessary and raise zoom changed event)
                            Rectangle zoomRectangle = PadViewingRectangle(layerObject.GetBounds());
                            this.ZoomToRectangle(
                                this.GetTransformedRectangle(zoomRectangle, false),
                                true, true, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22431", ex);
            }
        }

        /// <summary>
        /// Will return a new rectangle that has been enlarged (if possible) to
        /// fit the link arrows in view.
        /// </summary>
        /// <param name="viewRectangle">The <see cref="Rectangle"/> to enlarge.</param>
        /// <returns>A padded version of the specified rectangle that has had space
        /// added to account for link arrows.</returns>
        private Rectangle PadViewingRectangle(Rectangle viewRectangle)
        {
            // Get padding values (want to try to make room for link arrows)
            int xPadding = 
                (viewRectangle.Location.X > _ZOOM_TO_OBJECT_WIDTH_PADDING
                && viewRectangle.Right < (this.ImageWidth - _ZOOM_TO_OBJECT_WIDTH_PADDING))
                ? _ZOOM_TO_OBJECT_WIDTH_PADDING : 0;
            int yPadding =
                (viewRectangle.Location.Y > _ZOOM_TO_OBJECT_HEIGHT_PADDING
                && viewRectangle.Bottom < (this.ImageHeight - _ZOOM_TO_OBJECT_HEIGHT_PADDING))
                ? _ZOOM_TO_OBJECT_HEIGHT_PADDING : 0;

            // Adjust rectangle by the padding values
            viewRectangle.Inflate(xPadding * 2, yPadding * 2);
            viewRectangle.Location.Offset((-1) * xPadding, (-1) * yPadding);

            return viewRectangle;
        }

        /// <summary>
        /// Gets the next visible <see cref="LayerObject"/> that can be navigated to.
        /// </summary>
        /// <returns>The next visible <see cref="LayerObject"/> that can be navigated
        /// to if there is one, otherwise <see langword="null"/>.</returns>
        private LayerObject GetNextVisibleLayerObject()
        {
            // Default return value to null
            LayerObject returnObject = null;

            // Get the next object (if there is one)
            int index = this.GetNextVisibleLayerObjectIndex();
            if (index != -1)
            {
                returnObject = _layerObjects.GetSortedVisibleCollection(true)[index];
            }

            return returnObject;
        }

        /// <summary>
        /// Gets the previous visible <see cref="LayerObject"/> that can be navigated to.
        /// </summary>
        /// <returns>The previous visible <see cref="LayerObject"/> that can be navigated
        /// to if there is one, otherwise <see langword="null"/>.</returns>
        private LayerObject GetPreviousVisibleLayerObject()
        {
            // Default return value to null
            LayerObject returnObject = null;

            // Get the previous object (if there is one)
            int index = this.GetPreviousVisibleLayerObjectIndex();
            if (index != -1)
            {
                returnObject = _layerObjects.GetSortedVisibleCollection(true)[index];
            }

            return returnObject;
        }

        /// <summary>
        /// Gets the index into the sorted collection of <see cref="LayerObject"/> of the
        /// next <see cref="LayerObject"/> to move to.
        /// </summary>
        /// <returns>The index of the next <see cref="LayerObject"/> or -1 if there is no
        /// next <see cref="LayerObject"/></returns>
        private int GetNextVisibleLayerObjectIndex()
        {
            // Default the return value to -1
            int nextIndex = -1;

            // Get the sorted collection of visible layer objects
            ReadOnlyCollection<LayerObject> sortedCollection =
                _layerObjects.GetSortedVisibleCollection(true);

            // Get the current view rectangle
            Rectangle viewRectangle = this.GetTransformedRectangle(
                this.GetVisibleImageArea(), true);

            // Check for a selection first (There must be at least one selected object
            // in the current view, otherwise search as if there is no selection)
            if (_layerObjects.Selection.IsAnyObjectContained(viewRectangle, this.PageNumber))
            {
                ReadOnlyCollection<LayerObject> sortedSelection =
                    _layerObjects.Selection.GetSortedVisibleCollection(true);

                // Get the id of the last selected object
                long id = sortedSelection[sortedSelection.Count - 1].Id;

                // Find the index of the last selected object
                for (int i = 0; i < sortedCollection.Count; i++)
                {
                    if (sortedCollection[i].Id == id)
                    {
                        // Set the next index to one past the last selected object
                        nextIndex = i + 1;
                        break;
                    }
                }
            }
            else
            {
                // No selection, check if there are any objects in the current view

                // Get the list of objects in view
                Collection<int> objectsInViewIndex =
                    _layerObjects.GetIndexOfSortedLayerObjectsInRectangle(this.PageNumber,
                    viewRectangle, true, true);

                if (objectsInViewIndex.Count == 1)
                {
                    // Only 1 object in view, get the index of the object immediately after it.
                    nextIndex = objectsInViewIndex[objectsInViewIndex.Count - 1] + 1;
                }
                else if (objectsInViewIndex.Count > 1)
                {
                    // More than one object in view

                    // First check if there is an object on the same page, but not in view
                    for (int i = 0; i < objectsInViewIndex.Count - 1; i++)
                    {
                        // If the index distance between two consecutive objects in
                        // view is not 1 then there is an object that is spatially
                        // in between these two objects, but not visible, set it as
                        // the next object
                        if (objectsInViewIndex[i + 1] - objectsInViewIndex[i] != 1)
                        {
                            nextIndex = objectsInViewIndex[i] + 1;
                            break;
                        }
                    }

                    // If still haven't found the next object, then get first object after
                    // the last object in view
                    if (nextIndex == -1)
                    {
                        nextIndex = objectsInViewIndex[objectsInViewIndex.Count - 1] + 1;
                    }
                }
                else
                {
                    // No objects in the current view need to search the rest of the page
                    
                    // Get a rectangle encapsulating the right of this view and search for
                    // an object in that area
                    Rectangle rightView = new Rectangle(viewRectangle.Right, viewRectangle.Top,
                        this.ImageWidth - viewRectangle.Right, viewRectangle.Height);
                    Collection<int> index = _layerObjects.GetIndexOfSortedLayerObjectsInRectangle(
                        this.PageNumber, rightView, false, false);

                    // If haven't found an object yet, search below the view area
                    if (index.Count == 0)
                    {
                        Rectangle bottomView = new Rectangle(0, viewRectangle.Bottom,
                            this.ImageWidth, this.ImageHeight - viewRectangle.Bottom);
                        index = _layerObjects.GetIndexOfSortedLayerObjectsInRectangle(
                            this.PageNumber, bottomView, false, false);
                    }

                    // If there was an item found set the index to the first item in
                    // the list of found objects
                    nextIndex = index.Count != 0 ? index[0] : -1;

                    // If haven't found an object yet, get the first visible object on following
                    // pages
                    if (nextIndex == -1)
                    {
                        for (int i = 0; i < sortedCollection.Count; i++)
                        {
                            if (sortedCollection[i].PageNumber > this.PageNumber)
                            {
                                nextIndex = i;
                                break;
                            }
                        }
                    }
                }
            }

            // If the index is outside the collection, set next index to -1
            if (nextIndex >= sortedCollection.Count)
            {
                nextIndex = -1;
            }

            return nextIndex;
        }

        /// <summary>
        /// Gets the index into the sorted collection of <see cref="LayerObject"/> of the
        /// previous <see cref="LayerObject"/> to move to.
        /// </summary>
        /// <returns>The index of the previous <see cref="LayerObject"/> or -1 if there is no
        /// previous <see cref="LayerObject"/></returns>
        private int GetPreviousVisibleLayerObjectIndex()
        {
            // Default the return value to -1
            int previousIndex = -1;

            // Get the sorted collection of visible layer objects
            ReadOnlyCollection<LayerObject> sortedCollection =
                _layerObjects.GetSortedVisibleCollection(true);

            // Get the current view rectangle
            Rectangle viewRectangle = this.GetTransformedRectangle(
                this.GetVisibleImageArea(), true);

            // Check for a selection first (There must be at least one selected object
            // in the current view, otherwise search as if there is no selection)
            if (_layerObjects.Selection.IsAnyObjectContained(viewRectangle, this.PageNumber))
            {
                // Get the sorted collection of selection objects
                ReadOnlyCollection<LayerObject> sortedSelection =
                    _layerObjects.Selection.GetSortedCollection();

                // Get the id of the first selected object
                long id = sortedSelection[0].Id;

                // Find the index of the first selected object
                for (int i = 0; i < sortedCollection.Count; i++)
                {
                    if (sortedCollection[i].Id == id)
                    {
                        // Set the previous index to one before the last selected object
                        previousIndex = i - 1;
                        break;
                    }
                }
            }
            else
            {
                // No selection, check if there are any objects in the current view

                // Get the list of objects in view
                Collection<int> objectsInViewIndex =
                    _layerObjects.GetIndexOfSortedLayerObjectsInRectangle(this.PageNumber,
                    viewRectangle, true, true);

                if (objectsInViewIndex.Count == 1)
                {
                    // Only 1 object in view, get the index of the object immediately preceding it.
                    previousIndex = objectsInViewIndex[0] - 1;
                }
                else if (objectsInViewIndex.Count > 1) 
                {
                    // More than one object in view

                    // First check if there is an object on the same page, but not in view
                    for (int i = objectsInViewIndex.Count-1; i > 0 ; i--)
                    {
                        // If the index distance between two consecutive objects in
                        // view is not 1 then there is an object that is spatially
                        // in between these two objects, but not visible, set it as
                        // the previous object
                        if (objectsInViewIndex[i] - objectsInViewIndex[i - 1] != 1)
                        {
                            previousIndex = objectsInViewIndex[i] - 1;
                            break;
                        }
                    }

                    // If still haven't found the previous object, then get first object before
                    // the first object in view
                    if (previousIndex == -1)
                    {
                        previousIndex = objectsInViewIndex[0] - 1;
                    }
                }
                else
                {
                    // No objects in the current view need to search the rest of the page
                    
                    // Get a rectangle encapsulating the left of this view and search for
                    // an object in that area
                    Rectangle leftView = new Rectangle(0, viewRectangle.Top,
                        viewRectangle.Left, viewRectangle.Height);
                    Collection<int> index = _layerObjects.GetIndexOfSortedLayerObjectsInRectangle(
                        this.PageNumber, leftView, true, false);

                    // If haven't found an object yet, search above the view area
                    if (index.Count == 0)
                    {
                        Rectangle topView = new Rectangle(0, 0,
                            this.ImageWidth, viewRectangle.Top);
                        index = _layerObjects.GetIndexOfSortedLayerObjectsInRectangle(
                            this.PageNumber, topView, true, false);
                    }

                    // If there were items found set the index to the last item in
                    // the list of found objects
                    previousIndex = index.Count != 0 ? index[index.Count-1] : -1;

                    // If haven't found an object yet, get the last visible object from all objects
                    // on preceding pages
                    if (previousIndex == -1)
                    {
                        for (int i = sortedCollection.Count - 1; i >= 0; i--)
                        {
                            if (sortedCollection[i].PageNumber < this.PageNumber)
                            {
                                previousIndex = i;
                                break;
                            }
                        }
                    }
                }
            }

            return previousIndex;
        }

        /// <summary>
        /// <para>Zooms in on the currently open image.</para>
        /// <para><b>Requirements</b></para>
        /// <para>An image must be open.</para>
        /// </summary>
        /// <event cref="ZoomChanged">The method is successful.</event>
        /// <exception cref="ExtractException">No image is open.</exception>
        public void ZoomIn()
        {
            try
            {
                Zoom(true);
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI21118", "Unable to zoom in.", e);
            }
        }

        /// <summary>
        /// <para>Zooms out from the currently open image.</para>
        /// <para><b>Requirements</b></para>
        /// <para>An image must be open.</para>
        /// </summary>
        /// <event cref="ZoomChanged">The method is successful.</event>
        /// <exception cref="ExtractException">No image is open.</exception>
        public void ZoomOut()
        {
            try
            {
                Zoom(false);
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI21119", "Unable to zoom out.", e);
            }
        }

        /// <summary>
        /// <para>Zooms the currently open image.</para>
        /// </summary>
        /// <event cref="ZoomChanged">The method is successful.</event>
        /// <exception cref="ExtractException">No image is open.</exception>
        private void Zoom(bool zoomIn)
        {
            // Ensure an image is open.
            ExtractException.Assert("ELI21434", "No image is open.", base.Image != null);

            // Suppress the paint event until all changes have been made
            base.BeginUpdate();
            try
            {
                // Ensure the fit mode is set to none
                if (_fitMode != FitMode.None)
                {
                    // Set the fit mode without updating the zoom
                    SetFitMode(FitMode.None, false, false);
                }

                // Get the current zoom setting
                ZoomInfo zoomInfo = GetZoomInfo();

                // Calculate the new zoom factor
                if (zoomIn)
                {
                    zoomInfo.ScaleFactor *= _ZOOM_FACTOR;
                }
                else
                {
                    zoomInfo.ScaleFactor /= _ZOOM_FACTOR;
                }

                // Apply the new zoom setting
                SetZoomInfo(zoomInfo, true);
            }
            finally
            {
                base.EndUpdate();
            }
        }

        /// <summary>
        /// <para>Sets the current zoom to the previous entry in the zoom history.</para>
        /// <para><b>Requirements</b></para>
        /// <para>An image must be open.</para>
        /// <para><see cref="CanZoomPrevious"/> must be <see langword="true"/>.</para>
        /// </summary>
        /// <event cref="ZoomChanged">The method is successful.</event>
        /// <exception cref="ExtractException">No image is open.</exception>
        /// <exception cref="ExtractException"><see cref="CanZoomPrevious"/> is 
        /// <see langword="false"/>.</exception>
        /// <seealso cref="CanZoomPrevious"/>
        public void ZoomPrevious()
        {
            try
            {
                // Ensure an image is open.
                ExtractException.Assert("ELI21435", "No image is open.", base.Image != null);

                // Ensure there is a previous zoom history entry
                ExtractException.Assert("ELI21469", "There is no previous zoom history entry.",
                    this.CanZoomPrevious);

                // Get the previous zoom history entry
                ZoomInfo zoomInfo = _imagePages[base.Image.Page - 1].ZoomPrevious();

                // Set the new zoom setting without updating the zoom history
                SetZoomInfo(zoomInfo, false);
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI21120", "Unable to zoom previous.", e);
            }
        }

        /// <summary>
        /// <para>Sets the current zoom to the previous entry in the zoom history.</para>
        /// <para><b>Requirements</b></para>
        /// <para>An image must be open.</para>
        /// <para><see cref="CanZoomNext"/> must be <see langword="true"/>.</para>
        /// </summary>
        /// <event cref="ZoomChanged">The method is successful.</event>
        /// <exception cref="ExtractException">No image is open.</exception>
        /// <exception cref="ExtractException"><see cref="CanZoomNext"/> is 
        /// <see langword="false"/>.</exception>
        /// <seealso cref="CanZoomNext"/>
        public void ZoomNext()
        {
            try
            {
                // Ensure an image is open.
                ExtractException.Assert("ELI21436", "No image is open.", base.Image != null);

                // Ensure there is a subsequent zoom history entry
                ExtractException.Assert("ELI21470", "There is no subsequent zoom history entry.",
                    this.CanZoomNext);

                // Get the previous zoom history entry
                ZoomInfo zoomInfo = _imagePages[base.Image.Page - 1].ZoomNext();

                // Set the new zoom setting without updating the zoom history
                SetZoomInfo(zoomInfo, false);
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI21121", "Unable to zoom next.", e);
            }
        }

        /// <summary>
        /// Rotates the active page the specified number of degrees.
        /// <para><b>Requirements</b></para>
        /// <para>An image must be open.</para>
        /// </summary>
        /// <param name="angle">The angle to rotate the image in degrees. Must be a multiple of 
        /// 90.</param>
        /// <exception cref="ExtractException">No image is open.</exception>
        /// <exception cref="ExtractException"><paramref name="angle"/> is not a multiple of 90.
        /// </exception>
        public void Rotate(int angle)
        {
            try
            {
                // Ensure an image is open
                ExtractException.Assert("ELI21106", "No image is open.", base.Image != null);

                // Rotate the current page
                Rotate(angle, base.Image.Page);
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI21122", "Cannot rotate image.", e);
            }
        }

        /// <summary>
        /// Rotates the specified page number the specified number of degrees.
        /// <para><b>Requirements</b></para>
        /// <para>An image must be open.</para>
        /// </summary>
        /// <param name="angle">The angle to rotate the image in degrees. Must be a multiple of 
        /// 90.</param>
        /// <param name="pageNumber">The one-based page number to rotate.</param>
        /// <exception cref="ExtractException">No image is open.</exception>
        /// <exception cref="ExtractException"><paramref name="angle"/> is not a multiple of 90.
        /// </exception>
        /// <exception cref="ExtractException"><paramref name="pageNumber"/> is not valid.
        /// </exception>
        public void Rotate(int angle, int pageNumber)
        {
            try
            {
                // Ensure an image is open
                ExtractException.Assert("ELI21108", "No image is open.", base.Image != null);

                // Ensure the angle is valid
                ExtractException.Assert("ELI21107", "Rotation angle must be a multiple of 90.",
                    angle % 90 == 0);

                // Ensure the page is valid
                ExtractException.Assert("ELI21109", "Invalid page number.",
                    (pageNumber > 0 && pageNumber <= base.Image.PageCount));

                // Check if the view perspectives are licensed (ID Annotation feature)
                bool viewPerspectiveLicensed = !RasterSupport.IsLocked(RasterSupportType.Document);

                // Calulate the new orientation
                _imagePages[pageNumber - 1].RotateOrientation(angle);

                // Store the current page
                int originalPage = base.Image.Page;

                // Postpone the paint event until all changes have been made
                base.BeginUpdate();
                try
                {
                    // Switch to a different page if necessary
                    if (pageNumber != originalPage)
                    {
                        base.Image.Page = pageNumber;
                    }

                    // Get the center of the visible image in logical (image) coordinates
                    Point[] center = new Point[] { GetVisibleImageCenter() };

                    // Rotate the image
                    // NOTE: It is faster to rotate using the view perspective, so rotate that 
                    // way if it is licensed. Otherwise, use the slower rotate command.
                    if (viewPerspectiveLicensed)
                    {
                        // Fast rotation
                        base.Image.RotateViewPerspective(angle % 360);
                    }
                    else
                    {
                        // Not as fast rotation
                        RotateCommand rotate = new RotateCommand((angle % 360) * 100,
                            RotateCommandFlags.Resize,
                            RasterColor.FromGdiPlusColor(Color.White));
                        rotate.Run(base.Image);
                    }


                    // Check if the zoom needs to be updated
                    if (pageNumber == originalPage)
                    {
                        // Convert the center to client coordinates
                        _transform.TransformPoints(center);

                        // Center at the same point as prior to rotation
                        base.CenterAtPoint(center[0]);
                    }
                }
                catch
                {
                    // Reverse the orientation change
                    _imagePages[pageNumber - 1].RotateOrientation(-angle);

                    throw;
                }
                finally
                {
                    // Switch back to original page if necessary
                    if (pageNumber != originalPage)
                    {
                        base.Image.Page = originalPage;
                    }

                    base.EndUpdate();
                }

                // Raise the OrientationChanged event
                OnOrientationChanged(
                    new OrientationChangedEventArgs(_imagePages[pageNumber - 1].Orientation));
            }
            catch (Exception e)
            {
                ExtractException ee = new ExtractException("ELI21123",
                    "Cannot rotate image.", e);
                ee.AddDebugData("Rotation", angle, false);
                ee.AddDebugData("Page number", pageNumber, false);
                throw ee;
            }
        }

        /// <summary>
        /// Prints the currently open image.
        /// <para><b>Requirements</b></para>
        /// <para>An image must be open.</para>
        /// </summary>
        /// <exception cref="ExtractException">No image is open.</exception>
        public void Print()
        {
            this.Print(true);
        }

        /// <summary>
        /// Prints the currently open image.
        /// <para><b>Requirements</b></para>
        /// <para>An image must be open.</para>
        /// </summary>
        /// <param name="raiseDisplayingPrint">If <see langword="true"/> then
        /// will raise the <see cref="DisplayingPrintDialog"/> event.</param>
        /// <exception cref="ExtractException">No image is open.</exception>
        private void Print(bool raiseDisplayingPrint)
        {
            try
            {
                ExtractException.Assert("ELI21114", "No image is currently open.",
                    base.Image != null);

                if (raiseDisplayingPrint)
                {
                    // Raise the DisplayingPrintDialog event
                    DisplayingPrintDialogEventArgs eventArgs = new DisplayingPrintDialogEventArgs();
                    OnDisplayingPrintDialog(eventArgs);

                    // Check if the event was cancelled
                    if (eventArgs.Cancel)
                    {
                        return;
                    }
                }

                // Update the print dialog
                UpdatePrintDialog();

                // Display a warning if there are no valid printers and quit
                if (string.IsNullOrEmpty(_printDialog.PrinterSettings.PrinterName))
                {
                    ShowNoValidPrintersWarning();
                    return;
                }

                // Prompt the user for print options
                bool showDialog = true;
                while (showDialog && _printDialog.ShowDialog() == DialogResult.OK)
                {
                    if (_printDialog.PrinterSettings.IsValid
                        && !_disallowedPrinters.Contains(
                        _printDialog.PrinterSettings.PrinterName.ToUpperInvariant()))
                    {
                        // Refresh the form
                        this.RefreshImageViewerAndParent();

                        // Print the document
                        _printDialog.Document.Print();

                        // Store the last printer
                        this.LastPrinter = _printDialog.PrinterSettings.PrinterName;

                        showDialog = false;
                    }
                    else
                    {
                        // Build a list of disallowed printers
                        StringBuilder sb = new StringBuilder();
                        foreach (string printerName in _disallowedPrinters)
                        {
                            sb.AppendLine(printerName);
                        }
                        sb.AppendLine();

                        // Prompt the user about the disallowed printer
                        string applicationName = Application.ProductName;
                        applicationName = string.IsNullOrEmpty(applicationName) ?
                            "Application" : applicationName;
                        MessageBox.Show(applicationName + " has disallowed Printing to "
                            + _printDialog.PrinterSettings.PrinterName.ToUpperInvariant() + "."
                            + " Please select a different printer." + Environment.NewLine
                            + Environment.NewLine
                            + Environment.NewLine + "The following printers have been disallowed:"
                            + Environment.NewLine + "-------------------------------------"
                            + Environment.NewLine + sb.ToString(), " Printer",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);

                        // Refresh the form
                        this.RefreshImageViewerAndParent();

                        // Update the print dialog
                        UpdatePrintDialog();
                    }
                }
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI21124", "Cannot print image.", e);
            }
        }

        void ShowNoValidPrintersWarning()
        {
            // Create a string with additional information about this warning.
            string moreInfo = "";
            if (PrinterSettings.InstalledPrinters.Count > 0)
            {
                moreInfo = "(You cannot print to " + GetCommaSeparatedDisallowedPrinters() + 
                    " from " + (Application.ProductName ?? "the current application") + ")." 
                    + Environment.NewLine;
            }

            // Warn user that there are no valid printers installed
            MessageBox.Show("There are no printers available." + Environment.NewLine
                + moreInfo + Environment.NewLine
                + "Please install a printer and try again.", "No printers available", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, 0);

            this.RefreshImageViewerAndParent();
        }

        /// <summary>
        /// Gets the list of disallowed printers suitable for display to an end user.
        /// </summary>
        /// <returns>The list of disallowed printers suitable for display to an end user.</returns>
        string GetCommaSeparatedDisallowedPrinters()
        {
            // Check if there are no disallowed printers
            int count = _disallowedPrinters.Count;
            if (count <= 0)
            {
                return "";
            }

            // Check if there is one disallowed printer
            if (count == 1)
            {
                return _disallowedPrinters[0];
            }

            // Check if there are two disallowed printers
            if (count == 2)
            {
                return string.Join(" and ", _disallowedPrinters.ToArray());
            }

            // There are three or more disallowed printers
            return string.Join(",", _disallowedPrinters.ToArray(), 0, count - 1) + ", and " + 
                _disallowedPrinters[count - 1];
        }

        /// <summary>
        /// Shows the print preview window with the currently open image.
        /// <para><b>Requirements:</b></para>
        /// An image must be open.
        /// </summary>
        /// <exception cref="ExtractException">No image is open.</exception>
        public void PrintPreview()
        {
            try
            {
                ExtractException.Assert("ELI23011", "No image is currently open.",
                    base.Image != null);

                // Raise the DisplayingPrintDialog event
                DisplayingPrintDialogEventArgs eventArgs = new DisplayingPrintDialogEventArgs();
                OnDisplayingPrintDialog(eventArgs);

                // Check if the event was cancelled
                if (eventArgs.Cancel)
                {
                    return;
                }

                using (_printPreview = new PrintPreviewDialog())
                {
                    // Set the appropriate settings for the print preview dialog
                    _printPreview.Document = GetPrintDocument();
                    _printPreview.Text = "Preview - " + this.ImageFile;
                    _printPreview.UseAntiAlias = true;
                    
                    // Display a warning message and quit if no printers are valid
                    if (string.IsNullOrEmpty(_printPreview.Document.PrinterSettings.PrinterName))
                    {
                        ShowNoValidPrintersWarning();
                        return;
                    }

                    // Set the start page to the current page
                    _printPreview.PrintPreviewControl.StartPage = base.Image.Page - 1;

                    // Replace the print button on the toolstrip with our own special print button

                    // Get the toolstrip
                    ToolStrip toolStrip = (ToolStrip)_printPreview.Controls[1];

                    // Get the print button from the toolstrip
                    ToolStripButton printButton = (ToolStripButton)toolStrip.Items[0];

                    // Create our own toolstrip button from the print buttons data
                    // but replace the event handler with a call to our print function
                    ToolStripButton ourPrintButton = new ToolStripButton(printButton.Text,
                        printButton.Image, HandlePrintPreviewPrintButtonClick, printButton.Name);
                    ourPrintButton.DisplayStyle = ToolStripItemDisplayStyle.Image;

                    // Remove the original print button and add our own
                    toolStrip.Items.RemoveAt(0);
                    toolStrip.Items.Insert(0, ourPrintButton);

                    // Add a handler to the load method of the print preview dialog
                    // need to do this so that the form shows up where we want it to
                    _printPreview.Load += HandlePrintPreviewLoad;

                    // Invalidate the image viewer before displaying the form
                    this.RefreshImageViewerAndParent();

                    // Show the dialog
                    _printPreview.ShowDialog();

                    // Remove handler for load event
                    _printPreview.Load -= HandlePrintPreviewLoad;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23012", ex);
            }
            finally
            {
                // Set the print preview back to null
                _printPreview = null;
            }
        }

        /// <summary>
        /// Shows a dialog box that allows the user to modify the print settings.
        /// </summary>
        public void ShowPrintPageSetupDialog()
        {
            try
            {
                // Refresh the image viewer
                this.RefreshImageViewerAndParent();

                // Create the page setup dialog
                using (PageSetupDialog pageSetupDialog = new PageSetupDialog())
                {
                    pageSetupDialog.Document = GetPrintDocument();

                    // Display a warning message and quit if no printers are valid
                    if (string.IsNullOrEmpty(pageSetupDialog.PrinterSettings.PrinterName))
                    {
                        ShowNoValidPrintersWarning();
                        return;
                    }

                    pageSetupDialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23030", ex);
            }
        }

        /// <summary>
        /// Gets a <see cref="PrintDocument"/> that can be used for printing or
        /// print preview.
        /// </summary>
        /// <returns>A <see cref="PrintDocument"/> object for this image.</returns>
        private PrintDocument GetPrintDocument()
        {
            // Create the print document if not already created
            if (_printDocument == null)
            {
                _printDocument = new PrintDocument();
                _printDocument.PrintPage += new PrintPageEventHandler(HandlePrintPage);
                _printDocument.BeginPrint += new PrintEventHandler(HandleBeginPrint);
                _printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                _printDocument.DefaultPageSettings.Color = true;
                _printDocument.PrinterSettings.DefaultPageSettings.Landscape = false;
            }

            // Setup the printer settings
            PrinterSettings settings = _printDocument.PrinterSettings;
            settings.MinimumPage = 1;
            settings.FromPage = 1;
            settings.MaximumPage = base.Image.PageCount;
            settings.ToPage = base.Image.PageCount;

            // Ensure selection of a valid printer
            if (_disallowedPrinters.Contains(settings.PrinterName.ToUpperInvariant()))
            {
                // Current is invalid check the default printer
                PrinterSettings temp = new PrinterSettings();
                if (settings.IsDefaultPrinter ||
                    _disallowedPrinters.Contains(temp.PrinterName.ToUpperInvariant()))
                {
                    // Default is invalid, try last printer
                    string lastPrinter = this.LastPrinter;
                    if (string.IsNullOrEmpty(lastPrinter))
                    {
                        // Last printer was invalid, set to the first valid printer found
                        foreach (string printerName in PrinterSettings.InstalledPrinters)
                        {
                            if (!_disallowedPrinters.Contains(printerName.ToUpperInvariant()))
                            {
                                lastPrinter = printerName;
                                break;
                            }
                        }
                    }

                    // Set the printer name
                    settings.PrinterName = lastPrinter;
                }
                else
                {
                    // Default is valid
                    settings.PrinterName = temp.PrinterName;
                }
            }

            return _printDocument;
        }

        /// <summary>
        /// Instantiates <see cref="_printDialog"/> if it has not yet been created and updates its 
        /// printer settings.
        /// </summary>
        private void UpdatePrintDialog()
        {
            // Check if the print dialog has been created yet
            if (_printDialog == null)
            {
                // Create the print dialog
                _printDialog = new PrintDialog();
                _printDialog.AllowCurrentPage = true;
                _printDialog.AllowSomePages = true;
                _printDialog.UseEXDialog = true;
            }

            // Setup the print document
            _printDialog.Document = GetPrintDocument();
        }

        /// <summary>
        /// Prints the page corresponding to <see cref="_printPage"/>.
        /// </summary>
        /// <param name="e">The event data associated with a <see cref="PrintDocument.PrintPage"/> 
        /// event.</param>
        private void PrintPage(PrintPageEventArgs e)
        {
            // Get the margin bounds
            Rectangle marginBounds = e.MarginBounds;

            // Calculate the scale so the image fits exactly within the bounds of the page
            float scale = (float)Math.Min(
                marginBounds.Width / (double)base.Image.ImageWidth,
                marginBounds.Height / (double)base.Image.ImageHeight);

            // Calculate the horizontal and vertical padding
            PointF padding = new PointF(
                (marginBounds.Width - base.Image.ImageWidth * scale) / 2.0F,
                (marginBounds.Height - base.Image.ImageHeight * scale) / 2.0F);

            // Calculate the destination rectangle
            Rectangle destination = new Rectangle(
                (int)(marginBounds.Left + padding.X), (int)(marginBounds.Top + padding.Y),
                (int)(marginBounds.Width - padding.X * 2), (int)(marginBounds.Height - padding.Y * 2));

            // Clip the region based on printable area [IDSD #318]
            e.Graphics.Clip = new Region(e.PageSettings.PrintableArea);

            Matrix unrotatedToPrinter = null;
            Matrix imageToPrinter = null;
            try
            {
                // Create a matrices to map logical (image) coordinates to printer coordinates
                unrotatedToPrinter = GetPrintMatrix(scale, destination.Location);
                imageToPrinter = GetRotatedMatrix(unrotatedToPrinter, false);

                // Draw the image
                using (Image img = base.Image.ConvertToGdiPlusImage())
                {
                    e.Graphics.DrawImage(img, destination);
                }

                // Check if annotations need to be drawn
                if (_annotations != null)
                {
                    // Ensure the original transformation matrix is restored
                    Matrix originalTransform = _annotations.Transform;
                    try
                    {
                        // Apply the rotation to the annotations
                        _annotations.Transform = imageToPrinter.Clone();

                        // Draw the annotations
                        _annotations.Draw(e.Graphics);
                    }
                    finally
                    {
                        // Restore the original transformation matrix
                        _annotations.Transform = originalTransform;
                    }
                }

                // Iterate through each layer object
                foreach (LayerObject layerObject in _layerObjects)
                {
                    // Check if this layer object should be rendered
                    if (layerObject.PageNumber == base.Image.Page && layerObject.CanRender)
                    {
                        // Render the layer object
                        layerObject.Render(e.Graphics, imageToPrinter);
                    }
                }

                // Draw the watermark if necessary
                if (_watermark != null)
                {
                    // Store the original transform
                    Matrix originalTransform = e.Graphics.Transform;
                    try
                    {
                        // Draw the watermark
                        e.Graphics.Transform = unrotatedToPrinter;
                        DrawWatermark(base.Image, e.Graphics);
                    }
                    finally
                    {
                        // Restore the original transform
                        e.Graphics.Transform = originalTransform;
                    }
                }
            }
            finally
            {
                // Dispose of resources
                if (unrotatedToPrinter != null)
                {
                    unrotatedToPrinter.Dispose();
                }
                if (imageToPrinter != null)
                {
                    imageToPrinter.Dispose();
                }
            }
        }

        /// <summary>
        /// Creates a 3x3 affine matrix that maps the unrotated image to a destination rectangle 
        /// with the specified scale factor and margin padding.
        /// </summary>
        /// <param name="scale">The scale to apply to the original image in logical (image) 
        /// coordinates.</param>
        /// <param name="padding">The padding to apply to the original image in logical (image) 
        /// coordinates.</param>
        /// <returns>A 3x3 affine matrix that maps the unrotated image to a rotated destination 
        /// rectangle with the specified scale factor and margin padding.
        /// </returns>
        static Matrix GetPrintMatrix(float scale, PointF padding)
        {
            // Create the matrix
            Matrix printMatrix = new Matrix();

            // Scale the matrix
            printMatrix.Scale(scale, scale);

            // Translate the matrix so the image is centered with the specified padding
            printMatrix.Translate(padding.X, padding.Y, MatrixOrder.Append);

            // Return the rotated the matrix
            return printMatrix;
        }

        /// <summary>
        /// <para>Loads annotations from the currently open <see cref="_imageFile"/>.</para>
        /// <para><b>Requirements</b></para>
        /// <para>An image must be open.</para>
        /// <para><see cref="_imageFile"/> must exist on disk.</para>
        /// <para><see cref="_codecs"/> cannot be <see langword="null"/>.</para>
        /// <remarks>If the <see cref="_displayAnnotations"/> is <see langword="false"/>. 
        /// Annotations will be cleared and no annotations will be displayed.</remarks>
        /// </summary>
        private void UpdateAnnotations()
        {
            // Dispose of the annotations if they exist
            if (_annotations != null)
            {
                _annotations.Dispose();
                _annotations = null;
            }

            // If annotations shouldn't be displayed or there is no open image file
            // we are done.
            if (!_displayAnnotations || _currentOpenFile == null || !_validAnnotations)
            {
                return;
            }

            try
            {
                // Load annotations from file
                RasterTagMetadata tag = _codecs.ReadTag(_currentOpenFile, base.Image.Page,
                    RasterTagMetadata.AnnotationTiff);
                if (tag != null)
                {
                    // Initialize a new annotations container and associate it with the image viewer.
                    _annotations = new AnnContainer();

                    // Note: Use original image bounds for rotated images [DotNetRCAndUtils #54]
                    _annotations.Bounds =
                        new AnnRectangle(0, 0, base.Image.Width, base.Image.Height, AnnUnit.Pixel);
                    _annotations.Visible = true;

                    // Ensure the unit converter is the same DPI as the image viewer itself.
                    using (Graphics g = base.CreateGraphics())
                    {
                        _annotations.UnitConverter = new AnnUnitConverter(g.DpiX, g.DpiY);
                    }

                    // Store the transformation matrix [DotNetRCAndUtils #53]
                    _annotations.Transform = _transform.Clone();

                    // Load the annotations from the Tiff tag.
                    AnnCodecs annCodecs = new AnnCodecs();
                    annCodecs.LoadFromTag(tag, _annotations);

                    // Make all Leadtools annotations visible
                    foreach (AnnObject annotation in _annotations.Objects)
                    {
                        annotation.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Dispose of the annotations container if it has been initialized
                if (_annotations != null)
                {
                    _annotations.Dispose();
                    _annotations = null;
                }

                ExtractException ee = new ExtractException("ELI23242",
                    "Error reading annotations. You may continue to work with the image "
                    + "but existing annotations will not be saved or printed.", ex);
                ee.Display();

                // Set the valid annotations flag to false
                _validAnnotations = false;
            }
            finally
            {
                // After loading the tags the position will be moved, set it back to 0
                if (_currentOpenFile != null && _currentOpenFile.Position != 0)
                {
                    _currentOpenFile.Position = 0;
                }
            }
        }

        /// <summary>
        /// Use the current zoom to raise the <see cref="ZoomChanged"/> event, update the zoom 
        /// history, or both.
        /// </summary>
        /// <param name="updateZoomHistory"><see langword="true"/> if the zoom history should be
        /// updated; <see langword="false"/> if it should not.</param>
        /// <param name="raiseZoomChanged"><see langword="true"/> if the <see cref="ZoomChanged"/> 
        /// event should be raised; <see langword="false"/> if it should not.</param>
        /// <event cref="ZoomChanged"><paramref name="raiseZoomChanged"/> was 
        /// <see langword="true"/>.</event>
        private void UpdateZoom(bool updateZoomHistory, bool raiseZoomChanged)
        {
            // If nothing should be updated, we are done
            if (!updateZoomHistory && !raiseZoomChanged)
            {
                return;
            }

            // Get the current zoom setting
            ZoomInfo zoomInfo = GetZoomInfo();

            // Add the entry to the zoom history for the current page if necessary
            if (updateZoomHistory)
            {
                _imagePages[base.Image.Page - 1].ZoomInfo = zoomInfo;
            }

            // Raise the ZoomChanged event
            if (raiseZoomChanged)
            {
                OnZoomChanged(new ZoomChangedEventArgs(zoomInfo));
            }
        }

        /// <summary>
        /// Zooms the image to the specified rectangle and optionally updates the zoom.
        /// </summary>
        /// <param name="rc">The rectangle to which the image should be zoomed in client 
        /// coordinates.</param>
        /// <param name="updateZoomHistory"><see langword="true"/> if the zoom history should be 
        /// updated; <see langword="false"/> if it should not be updated.</param>
        /// <param name="raiseZoomChanged"><see langword="true"/> if the <see cref="ZoomChanged"/> 
        /// event should be raised; <see langword="false"/> if it should not.</param>
        /// <param name="updateFitMode"><see langword="true"/> if the fit mode should be reset; 
        /// <see langword="false"/> if the fit mode should not be changed.</param>
        /// <exception cref="ExtractException">No image is open.</exception>
        /// <event cref="ZoomChanged"><paramref name="raiseZoomChanged"/> was 
        /// <see langword="true"/>.</event>
        private void ZoomToRectangle(Rectangle rc, bool updateZoomHistory, bool raiseZoomChanged,
            bool updateFitMode)
        {
            // Ensure an image is open
            ExtractException.Assert("ELI21455", "No image is open.", base.Image != null);

            // Suspend paint event until after zoom has changed
            base.BeginUpdate();
            try
            {
                // Change the fit mode if it is not fit mode none
                if (updateFitMode && _fitMode != FitMode.None)
                {
                    // Set the fit mode to none without updating the zoom
                    SetFitMode(FitMode.None, false, false);
                }

                // Zoom to the specified rectangle
                base.ZoomToRectangle(rc);
            }
            finally
            {
                base.EndUpdate();
            }

            // Update the zoom if necessary
            UpdateZoom(updateZoomHistory, raiseZoomChanged);
        }

        /// <summary>
        /// Begins an interactive tracking action (e.g. drag and drop) starting at the specified 
        /// mouse position.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        private void StartTracking(int mouseX, int mouseY)
        {
            // Tracking data should be null at this point, but if not dispose of it.
            if (_trackingData != null)
            {
                _trackingData.Dispose();
                _trackingData = null;
            }

            // Check if interactive region tracking should begin
            switch (_cursorTool)
            {
                case CursorTool.AngularHighlight:
                case CursorTool.AngularRedaction:
                    {
                        // Calculate the default highlight height in physical (client) coordinates
                        int height = (int)(_defaultHighlightHeight * GetScaleFactorY(_transform) +
                            0.5);

                        // Begin interactive region tracking
                        _trackingData = new TrackingData(this, mouseX, mouseY,
                            GetVisibleImageArea(), height);
                    }
                    break;

                case CursorTool.DeleteLayerObjects:
                case CursorTool.Pan:
                case CursorTool.RectangularHighlight:
                case CursorTool.RectangularRedaction:
                case CursorTool.SetHighlightHeight:
                case CursorTool.ZoomWindow:

                    // Begin interactive region tracking
                    _trackingData = new TrackingData(this, mouseX, mouseY, GetVisibleImageArea());
                    break;

                case CursorTool.EditHighlightText:
                    {
                        // Convert the mouse click point from client to image coordinates
                        Point[] mouseClick = new Point[] { new Point(mouseX, mouseY) };
                        using (Matrix clientToImage = _transform.Clone())
                        {
                            clientToImage.Invert();
                            clientToImage.TransformPoints(mouseClick);
                        }

                        // Iterate through all the layer objects
                        foreach (LayerObject layerObject in _layerObjects)
                        {
                            // Check if this layer object was clicked
                            if (layerObject.HitTest(mouseClick[0]))
                            {
                                // Check if this was a highlight
                                Highlight highlight = layerObject as Highlight;
                                if (highlight == null)
                                {
                                    continue;
                                }

                                // Prompt the user for the new highlight text
                                EditHighlightTextForm dialog =
                                    new EditHighlightTextForm(highlight.Text);
                                if (dialog.ShowDialog() == DialogResult.OK)
                                {
                                    // Store the new highlight text
                                    highlight.Text = dialog.HighlightText;
                                }
                                break;
                            }
                        }
                    }
                    break;

                case CursorTool.SelectLayerObject:
                    {
                        // Get modifier keys immediately
                        Keys modifiers = Control.ModifierKeys;

                        // Check if the mouse clicked on a link arrow
                        if (_activeLinkedLayerObject != null)
                        {
                            int arrowId = 
                                _activeLinkedLayerObject.GetLinkArrowId(new Point(mouseX, mouseY));
                            if (arrowId >= 0)
                            {
                                // Go to the previous or next linked layer object
                                GoToLink(arrowId == 0, _activeLinkedLayerObject);
                                return;
                            }
                        }

                        // Iterate through all selected layerObjects
                        foreach (LayerObject layerObject in _layerObjects.Selection)
                        {
                            // Skip this layerObject if it is on a different page
                            if (layerObject.PageNumber != base.Image.Page)
                            {
                                continue;
                            }

                            // Check if the mouse cursor is over one of the grip handles
                            int gripHandleId = layerObject.GetGripHandleId(new Point(mouseX, mouseY));
                            if (gripHandleId >= 0)
                            {
                                layerObject.StartTrackingGripHandle(mouseX, mouseY, gripHandleId);
                                _trackingData = new TrackingData(this, mouseX, mouseY,
                                    GetVisibleImageArea());
                                return;
                            }
                        }

                        // Get the mouse position in image coordinates
                        Point[] mouse = new Point[] { new Point(mouseX, mouseY) };
                        using (Matrix clientToImage = _transform.Clone())
                        {
                            clientToImage.Invert();
                            clientToImage.TransformPoints(mouse);
                        }

                        // Check if the mouse cursor is over a layerObject
                        bool layerObjectClicked = false;
                        foreach (LayerObject layerObject in _layerObjects)
                        {
                            // Only perform hit test if the layer object is selectable and visible
                            if (layerObject.Selectable && layerObject.Visible &&
                                layerObject.HitTest(mouse[0]))
                            {
                                layerObjectClicked = true;

                                // Select this layer object if it is not already
                                if (!_layerObjects.Selection.Contains(layerObject))
                                {
                                    // Deselect previous layer object unless modifier key is pressed
                                    if (modifiers != Keys.Control && modifiers != Keys.Shift)
                                    {
                                        _layerObjects.Selection.Clear();
                                    }

                                    layerObject.Selected = true;

                                    // Switch the cursor to the move cursor
                                    this.Cursor = Cursors.SizeAll;
                                }
                                else if (modifiers == Keys.Control)
                                {
                                    // The control key is pressed and a selected 
                                    // layer object was clicked. Remove the layer object.
                                    _layerObjects.Selection.Remove(layerObject);

                                    // Invalidate the image viewer to remove any previous grip 
                                    // handles and to redraw these grip handles
                                    Invalidate();
                                    return;
                                }
                            }
                        }

                        // Check if any layer object was clicked [DotNetRCAndUtils #159]
                        if (layerObjectClicked)
                        {
                            // Start a tracking event for all selected layer objects on the active page
                            foreach (LayerObject layerObject in _layerObjects.Selection)
                            {
                                if (layerObject.PageNumber == base.Image.Page)
                                {
                                    layerObject.StartTrackingSelection(mouseX, mouseY);
                                }
                            }
                        }

                        // Start a click and drag event
                        _trackingData = new TrackingData(this, mouseX, mouseY,
                            GetVisibleImageArea());

                        // Invalidate the image viewer to remove any previous grip handles
                        // and to redraw these grip handles
                        Invalidate();
                    }
                    break;

                case CursorTool.None:
                    // Do nothing
                    break;

                default:
                    throw new ExtractException("ELI21507", "Unexpected cursor tool.");
            }
        }

        /// <summary>
        /// Goes to the previous or next link of the specified layer object.
        /// </summary>
        /// <param name="previous"><see langword="true"/> to go to the previous link; 
        /// <see langword="false"/> to go the next link.</param>
        /// <param name="layerObject">The layer object that holds the link to go to.</param>
        private void GoToLink(bool previous, LayerObject layerObject)
        {
            // Get the current zoom info
            ZoomInfo zoomInfo = this.GetZoomInfo();

            // Get the side of the layer object from which the 
            // view is relative in physical (client) coordinates
            Point relative = GetRelativeLinkPoint(previous, layerObject);

            // Get the center point of the current view in physical (client) coordinates
            Point[] center = new Point[] { zoomInfo.Center };
            _transform.TransformPoints(center);

            // Get the vector from the midpoint of the side of the layer object
            // to the center point of the view in physical (client) coordinates.
            Size delta = new Size(center[0].X - relative.X, center[0].Y - relative.Y);

            // Get the link object to which the view should go
            _activeLinkedLayerObject = previous ? layerObject.PreviousLink : layerObject.NextLink;

            // Select this layer object if it is selectable
            _layerObjects.Selection.Clear();
            if (_activeLinkedLayerObject.Selectable)
            {
                _activeLinkedLayerObject.Selected = true;
            }

            // Go to the proper page for this object
            SetPageNumber(_activeLinkedLayerObject.PageNumber, zoomInfo.FitMode == FitMode.FitToPage, true);

            // Set the appropriate scale factor if necessary
            if (zoomInfo.FitMode != FitMode.FitToPage)
            {
                // Check if no fit mode is specified
                if (zoomInfo.FitMode == FitMode.None)
                {
                    // Set the new scale factor for this page
                    base.ScaleFactor = zoomInfo.ScaleFactor;
                }

                // Get the side of the link from which the new view 
                // will be relative in physical (client) coordinates
                Point linkRelative = GetRelativeLinkPoint(previous, _activeLinkedLayerObject);

                // Calculate the new center point in logical (image) coordinates
                Point[] newCenter = new Point[] { linkRelative + delta };
                using(Matrix clientToImage = _transform.Clone())
	            {
                    clientToImage.Invert();
                    clientToImage.TransformPoints(newCenter);
	            }

                // Set the center point
                zoomInfo.Center = newCenter[0];
                this.SetZoomInfo(zoomInfo, true);
            }
        }

        /// <summary>
        /// Gets the midpoint of the left side of the specified layer object if 
        /// <paramref name="left"/> is <see langword="true"/> in physical (client) coordinates; 
        /// gets the midpoint of the right side of the specified layer object if 
        /// <paramref name="left"/> is <see langword="false"/>.
        /// </summary>
        /// <param name="left"><see langword="true"/> to get the left side of 
        /// <paramref name="layerObject"/>; <see langword="false"/> to get the right side.</param>
        /// <param name="layerObject">The layer object from which the returned point is relative.
        /// </param>
        /// <returns>The left or right side of the specified layer object in physical (client) 
        /// coordinates.</returns>
        private Point GetRelativeLinkPoint(bool left, LayerObject layerObject)
        {
            // Get the bounds of the layer object in physical (client) coordinates.
            Rectangle bounds = GetTransformedRectangle(layerObject.GetBounds(), false);

            // Get the side of the layer object from which the 
            // view is relative in physical (client) coordinates
            Point relative = new Point(left ? bounds.Left : bounds.Right,
                bounds.Top + bounds.Height / 2);
            return relative;
        }

        /// <summary>
        /// Updates the <see cref="_trackingData"/> using the specified mouse position.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        // This method has undergone a security review.
        [SuppressMessage("Microsoft.Security",
            "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        private void UpdateTracking(int mouseX, int mouseY)
        {
            // Update the tracking dependent upon the active cursor tool
            switch (_cursorTool)
            {
                case CursorTool.AngularHighlight:
                case CursorTool.AngularRedaction:

                    // Get a drawing surface for the interactive highlight
                    using (Graphics graphics = base.CreateGraphics())
                    {
                        // Get the appropriate color for drawing the object
                        Color drawColor = (_cursorTool == CursorTool.AngularHighlight ?
                            _defaultHighlightColor : _defaultRedactionPaintColor);

                        // Erase the previous highlight if it exists
                        if (!_trackingData.Region.IsEmpty(graphics))
                        {
                            DrawRegion(_trackingData.Region, graphics, drawColor,
                                NativeMethods.BinaryRasterOperations.R2_NOTXORPEN);
                        }

                        // Recalculate and redraw the new interactive highlight
                        _trackingData.UpdateAngularRegion(mouseX, mouseY);
                        if (!_trackingData.Region.IsEmpty(graphics))
                        {
                            DrawRegion(_trackingData.Region, graphics, drawColor,
                                NativeMethods.BinaryRasterOperations.R2_NOTXORPEN);
                        }
                    }
                    break;

                case CursorTool.DeleteLayerObjects:

                    // Erase the previous frame if it exists
                    Rectangle rectangle = _trackingData.Rectangle;
                    if (rectangle.Width == 0)
                    {
                        rectangle.Width = 1;
                    }
                    if (rectangle.Height == 0)
                    {
                        rectangle.Height = 1;
                    }
                    ControlPaint.DrawReversibleFrame(base.RectangleToScreen(rectangle),
                            Color.Black, FrameStyle.Thick);

                    // Recalculate and redraw the new frame
                    _trackingData.UpdateRectangle(mouseX, mouseY);
                    rectangle = _trackingData.Rectangle;
                    if (rectangle.Width == 0)
                    {
                        rectangle.Width = 1;
                    }
                    if (rectangle.Height == 0)
                    {
                        rectangle.Height = 1;
                    }
                    ControlPaint.DrawReversibleFrame(base.RectangleToScreen(rectangle),
                            Color.Black, FrameStyle.Thick);
                    break;

                case CursorTool.RectangularHighlight:
                case CursorTool.RectangularRedaction:

                    // Get a drawing surface for the interactive highlight
                    using (Graphics graphics = base.CreateGraphics())
                    {
                        // Get the appropriate color for drawing the object
                        Color drawColor = (_cursorTool == CursorTool.RectangularHighlight ?
                            _defaultHighlightColor : _defaultRedactionPaintColor);

                        // Erase the previous highlight if it exists
                        if (!_trackingData.Region.IsEmpty(graphics))
                        {
                            DrawRegion(_trackingData.Region, graphics, drawColor,
                                NativeMethods.BinaryRasterOperations.R2_NOTXORPEN);
                        }

                        // Recalculate and redraw the new interactive highlight
                        _trackingData.UpdateRectangularRegion(mouseX, mouseY);
                        if (!_trackingData.Region.IsEmpty(graphics))
                        {
                            DrawRegion(_trackingData.Region, graphics, drawColor,
                                NativeMethods.BinaryRasterOperations.R2_NOTXORPEN);
                        }
                    }
                    break;

                case CursorTool.SelectLayerObject:

                    if (base.Cursor == Cursors.Default)
                    {
                        // This is a click and drag multiple layer objects event

                        // Erase the previous frame if it exists
                        ControlPaint.DrawReversibleFrame(base.RectangleToScreen(_trackingData.Rectangle),
                                Color.Black, FrameStyle.Thick);

                        // Recalculate and redraw the new frame
                        _trackingData.UpdateRectangle(mouseX, mouseY);
                        ControlPaint.DrawReversibleFrame(base.RectangleToScreen(_trackingData.Rectangle),
                                Color.Black, FrameStyle.Thick);
                    }
                    else
                    {
                        // This is a layer object event
                        foreach (LayerObject layerObject in _layerObjects.Selection)
                        {
                            if (layerObject.TrackingData != null)
                            {
                                layerObject.UpdateTracking(mouseX, mouseY);
                            }
                        }
                    }
                    break;

                case CursorTool.SetHighlightHeight:

                    // Erase the previous line if it exists
                    Point[] line = _trackingData.Line;
                    if (line[0] != line[1])
                    {
                        ControlPaint.DrawReversibleLine(line[0], line[1], Color.Black);
                    }

                    // Recalculate and redraw the new line
                    _trackingData.UpdateLine(mouseX, mouseY);
                    line = _trackingData.Line;
                    if (line[0] != line[1])
                    {
                        ControlPaint.DrawReversibleLine(line[0], line[1], Color.Black);
                    }
                    break;

                default:

                    // There is no interactive region associated with this region OR
                    // the interactivity is handled by the Leadtools RasterImageViewer
                    break;
            }
        }

        /// <summary>
        /// Completes the interactive tracking action at the specified mouse position.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        private void EndTracking(int mouseX, int mouseY)
        {
            // Update the tracking dependent upon the active cursor tool
            switch (_cursorTool)
            {
                case CursorTool.AngularHighlight:
                case CursorTool.AngularRedaction:

                    EndAngularHighlightOrRedaction(mouseX, mouseY);
                    break;

                case CursorTool.DeleteLayerObjects:

                    EndDeleteLayerObjects(mouseX, mouseY);
                    break;

                case CursorTool.RectangularHighlight:
                case CursorTool.RectangularRedaction:

                    EndRectangularHighlightOrRedaction(mouseX, mouseY);
                    break;

                case CursorTool.SelectLayerObject:
                    EndSelectLayerObject(mouseX, mouseY);
                    break;

                case CursorTool.SetHighlightHeight:

                    EndSetHighlightHeight(mouseX, mouseY);
                    break;

                default:

                    // There is no interactive region associated with this region OR
                    // the interactivity is handled by the Leadtools RasterImageViewer
                    break;
            }

            _trackingData.Dispose();
            _trackingData = null;
        }

        private void EndSelectLayerObject(int mouseX, int mouseY)
        {
            // Check if this is a click and drag to select multiple highlights
            if (base.Cursor == Cursors.Default)
            {
                // Convert the rectangle from client to image coordinates
                Rectangle rectangle =
                    GetTransformedRectangle(_trackingData.Rectangle, true);

                // Deselect previous layerObject unless modifier key is pressed
                if (Control.ModifierKeys != Keys.Shift)
                {
                    _layerObjects.Selection.Clear();
                }

                // Iterate through all the layerObjects
                foreach (LayerObject layerObject in _layerObjects)
                {
                    // Check if this layerObject is selectable and
                    // intersects with the drag and drop rectangle
                    if (layerObject.Selectable && layerObject.IsVisible(rectangle))
                    {
                        // Select this layerObject if it is not already
                        layerObject.Selected = true;
                    }
                }

                // Refresh the image
                base.Invalidate();
            }
            else
            {
                // This was a layer object event

                // Check if the layer object event succeeded
                bool success = CheckIfLayerObjectEventSucceeded(new Point(mouseX, mouseY));

                // Iterate through all layer objects
                bool containsNonMoveableObjects = false;
                foreach (LayerObject layerObject in _layerObjects.Selection)
                {
                    // Check if any of the selected objects were non-moveable
                    containsNonMoveableObjects |= !layerObject.Movable;

                    if (layerObject.TrackingData != null)
                    {
                        // End tracking if the event succeeded.
                        // Cancel the tracking event if it failed.
                        if (success)
                        {
                            layerObject.EndTracking(mouseX, mouseY);
                        }
                        else
                        {
                            layerObject.CancelTracking();
                        }
                    }
                }

                // If objects were moved successfully, but some of the objects were
                // non-moveable prompt the user
                if (success && containsNonMoveableObjects)
                {
                    using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
                    {
                        messageBox.Text = "The selection contained non-moveable objects. These objects have not been moved.";
                        messageBox.Caption = "Some Objects Were Not Moved";
                        messageBox.StandardIcon = MessageBoxIcon.Information;
                        messageBox.AddStandardButtons(MessageBoxButtons.OK);
                        messageBox.Show();
                    }
                }
            }

            // Refresh the image viewer
            base.Invalidate();

            // Reset the selection cursor
            base.Cursor = GetSelectionCursor(mouseX, mouseY);
        }

        /// <summary>
        /// Checks whether a layer object event was successful at the end of tracking.
        /// </summary>
        /// <param name="mouse">The ending mouse position in physical (client) coordinates.
        /// </param>
        /// <returns><see langword="true"/> if the the mouse moved from its original location and 
        /// all the layer objects are valid; <see langword="false"/> if the mouse has not moved or 
        /// if the any layer object is invalid.</returns>
        bool CheckIfLayerObjectEventSucceeded(Point mouse)
        {
            // The layer object event failed if the mouse hasn't moved
            if (_trackingData.StartPoint == mouse)
            {
                return false;
            }

            // The layer object event failed if any layer object is invalid
            foreach (LayerObject layerObject in _layerObjects.Selection)
            {
                if (layerObject.TrackingData != null && !layerObject.IsValid())
                {
                    return false;
                }
            }

            // If this point was reached, the layer object event succeeded
            return true;
        }

        /// <summary>
        /// Ends the tracking of the create angular highlight interactive event.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        private void EndAngularHighlightOrRedaction(int mouseX, int mouseY)
        {
            // Get a drawing surface for the interactive highlight
            using (Graphics graphics = base.CreateGraphics())
            {
                // Get the appropriate color for drawing the object
                Color drawColor = (_cursorTool == CursorTool.AngularHighlight ?
                    _defaultHighlightColor : _defaultRedactionPaintColor);

                // Erase the previous highlight if it exists
                if (!_trackingData.Region.IsEmpty(graphics))
                {
                    DrawRegion(_trackingData.Region, graphics, drawColor,
                        NativeMethods.BinaryRasterOperations.R2_NOTXORPEN);
                }

                // Calculate the new region and rectangle
                _trackingData.UpdateAngularRegion(mouseX, mouseY);

                // Retain the new highlight if it is not empty                        
                if (!_trackingData.Region.IsEmpty(graphics))
                {
                    // Get the points of the highlight's bisecting line
                    Point[] points = new Point[] 
                    {
                        _trackingData.StartPoint, 
                        new Point(mouseX, mouseY)
                    };

                    // Convert the points from physical to logical coordinates
                    using (Matrix inverseMatrix = _transform.Clone())
                    {
                        // Invert the transformation matrix
                        inverseMatrix.Invert();

                        // Transform the midpoints of the sides of the highlight
                        inverseMatrix.TransformPoints(points);

                        if (_cursorTool == CursorTool.AngularHighlight)
                        {
                            // Instantiate the new highlight and add it to _layerObjects
                            Highlight highlight =
                                new Highlight(this, LayerObject.ManualComment, points[0], 
                                points[1], _defaultHighlightHeight);
                            _layerObjects.Add(highlight);
                        }
                        else
                        {
                            // Instantiate a new redaction and add it to _layerObjects
                            Redaction redaction = new Redaction(this, this.PageNumber, 
                                LayerObject.ManualComment,
                                new RasterZone[] { new RasterZone(points[0], points[1],
                                    _defaultHighlightHeight, base.Image.Page) },
                                    _defaultRedactionFillColor);
                            _layerObjects.Add(redaction);
                        }

                    }

                    // Draw the new highlight
                    DrawRegion(_trackingData.Region, graphics, drawColor,
                        NativeMethods.BinaryRasterOperations.R2_MASKPEN);
                }
            }

            // Invalidate the image viewer so the object is updated
            this.Invalidate();
        }

        /// <summary>
        /// Ends the tracking of the delete layer objects interactive event.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        private void EndDeleteLayerObjects(int mouseX, int mouseY)
        {
            // Erase the previous frame if it exists
            Rectangle rectangle = _trackingData.Rectangle;
            ControlPaint.DrawReversibleFrame(base.RectangleToScreen(rectangle),
                    Color.Black, FrameStyle.Thick);

            // Recalculate the new line
            _trackingData.UpdateRectangle(mouseX, mouseY);
            rectangle = _trackingData.Rectangle;

            // Convert the rectangle from client to image coordinates
            rectangle = GetTransformedRectangle(rectangle, true);

            // If the rectangle is empty, give it a thickness of one pixel 
            // to delete any layer objects that were clicked on.
            if (rectangle.Width == 0)
            {
                rectangle.Width = 1;
            }
            if (rectangle.Height == 0)
            {
                rectangle.Height = 1;
            }

            // Get all the layer objects in this region
            LinkedList<long> layerObjectIds = new LinkedList<long>();

            // Create a graphics object
            using (Graphics graphics = base.CreateGraphics())
            {
                foreach (LayerObject layerObject in _layerObjects)
                {
                    // Check if this layer object intersects the rectangle
                    if (layerObject.IsVisible(rectangle))
                    {
                        // Add this layer object's id to the list to delete
                        layerObjectIds.AddLast(layerObject.Id);
                    }
                }

                // Delete any layer objects that intersected
                foreach (int id in layerObjectIds)
                {
                    _layerObjects.Remove(id);
                }

                // Refresh the image
                base.Invalidate();
            }

            // Restore the last continuous use cursor tool
            this.CursorTool = _lastContinuousUseTool;
        }

        /// <summary>
        /// Ends the tracking of the create rectangular highlight interactive event.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        private void EndRectangularHighlightOrRedaction(int mouseX, int mouseY)
        {
            // Get a drawing surface for the interactive highlight
            using (Graphics graphics = base.CreateGraphics())
            {
                // Get the appropriate color for drawing the object
                Color drawColor = (_cursorTool == CursorTool.RectangularHighlight) ?
                    _defaultHighlightColor : _defaultRedactionPaintColor;

                // Erase the previous highlight if it exists
                if (!_trackingData.Region.IsEmpty(graphics))
                {
                    DrawRegion(_trackingData.Region, graphics, drawColor,
                        NativeMethods.BinaryRasterOperations.R2_NOTXORPEN);
                }

                // Calculate the new region and rectangle
                _trackingData.UpdateRectangularRegion(mouseX, mouseY);

                // Retain the new highlight if it is not empty                        
                if (!_trackingData.Region.IsEmpty(graphics))
                {
                    // Get the spatial data in image coordinates for the rectangle
                    Point[] points;
                    int height;
                    GetSpatialDataFromClientRectangle(_trackingData.Rectangle,
                        out points, out height);

                    if (_cursorTool == CursorTool.RectangularHighlight)
                    {
                        // Add a new highlight to _layerObjects
                        _layerObjects.Add(new Highlight(this, LayerObject.ManualComment, points[0],
                            points[1], height));
                    }
                    else
                    {
                        // Add a new redaction to _layerObjects
                        RasterZone[] rasterZones = new RasterZone[] 
                        {
                            new RasterZone(points[0], points[1], height, base.Image.Page)
                        };
                        _layerObjects.Add(new Redaction(this, this.PageNumber, 
                            LayerObject.ManualComment, rasterZones, _defaultRedactionFillColor));
                    }

                    // Draw the new highlight
                    DrawRegion(_trackingData.Region, graphics, drawColor,
                        NativeMethods.BinaryRasterOperations.R2_MASKPEN);
                }
            }

            // Invalidate the image viewer so the object is updated
            this.Invalidate();
        }

        /// <summary>
        /// Ends the tracking of the set highlight height interactive event.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        private void EndSetHighlightHeight(int mouseX, int mouseY)
        {
            // Erase the previous line if it exists
            Point[] line = _trackingData.Line;
            if (line[0] != line[1])
            {
                ControlPaint.DrawReversibleLine(line[0], line[1], Color.Black);
            }

            // Recalculate the new line
            _trackingData.UpdateLine(mouseX, mouseY);
            line = _trackingData.Line;

            // Convert the line from screen to client coordinates
            line = new Point[] { _trackingData.StartPoint, PointToClient(_trackingData.Line[1]) };

            // Convert the line from client to image coordinates
            using (Matrix clientToImage = _transform.Clone())
            {
                clientToImage.Invert();
                clientToImage.TransformPoints(line);
            }

            // Set the new highlight height (distance formula)
            _defaultHighlightHeight = (int)(Math.Sqrt(Math.Pow(line[0].X - line[1].X, 2) +
                Math.Pow(line[0].Y - line[1].Y, 2)) + 0.5);

            // Restore the last continuous use cursor tool
            this.CursorTool = _lastContinuousUseTool;
        }

        /// <summary>
        /// Gets a copy of the specified transformation matrix rotated as per the orientation of 
        /// and dimensions of the currently open image.
        /// </summary>
        /// <remarks>Uses the dimensions of the currently open image to compute the origin of the 
        /// resultant matrix.</remarks>
        /// <param name="matrix">The matrix from which to obtain a rotated matrix.</param>
        /// <param name="reverseRotation"><see langword="true"/> if the matrix should be rotated 
        /// counterclockwise; <see langword="false"/> if the matrix should be rotated clockwise. 
        /// The orientation is specified in degrees clockwise.</param>
        private Matrix GetRotatedMatrix(Matrix matrix, bool reverseRotation)
        {
            // Return a copy of the original matrix if no image is open.
            if (base.Image == null)
            {
                return matrix.Clone();
            }

            // Calculate the reversal multiplier value
            int reverseValue = reverseRotation ? -1 : 1;

            // Get the current orientation
            int orientation = _imagePages[base.Image.Page - 1].Orientation;

            // Rotate the matrix
            Matrix rotatedMatrix = matrix.Clone();
            rotatedMatrix.Rotate(orientation * reverseValue);

            // Translate the origin
            switch (orientation)
            {
                case 0:
                    break;

                case 90:
                    rotatedMatrix.Translate(0, -base.Image.ImageWidth * reverseValue);
                    break;

                case 180:
                    rotatedMatrix.Translate(-base.Image.ImageWidth * reverseValue,
                        -base.Image.ImageHeight * reverseValue);
                    break;

                case 270:
                    rotatedMatrix.Translate(-base.Image.ImageHeight * reverseValue, 0);
                    break;

                default:
                    break;
            }

            // Return the rotated matrix
            return rotatedMatrix;
        }

        /// <summary>
        /// Constructs the current <see cref="ZoomInfo"/>.
        /// </summary>
        /// <remarks>
        /// <para>If the current <see cref="ZoomInfo"/> has already been constructed, it is much
        /// more efficient to use the <see cref="ZoomInfo"/> property.</para>
        /// <para>When no image is open, the <see cref="ZoomInfo"/> returned is 
        /// undefined.</para>
        /// </remarks>
        /// <returns>The current <see cref="ZoomInfo"/>.</returns>
        private ZoomInfo GetZoomInfo()
        {
            // Return the ZoomInfo
            return new ZoomInfo(GetVisibleImageCenter(), base.ScaleFactor, _fitMode);
        }

        /// <summary>
        /// Get the center of the visible image area in logical (image) coordinates.
        /// </summary>
        /// <returns>The center of the visible image area in logical (image) coordinates.
        /// </returns>
        private Point GetVisibleImageCenter()
        {
            // Get the image area of the client in physical (client) coordinates.
            Rectangle imageArea = GetVisibleImageArea();

            // Get the center point of the zoom window in physical (client) coordinates
            Point[] center = new Point[] { new Point(imageArea.Width / 2, imageArea.Height / 2) };

            // Convert the center point to logical (image) coordinates
            using (Matrix clientToImage = _transform.Clone())
            {
                clientToImage.Invert();
                clientToImage.TransformPoints(center);
            }
            return center[0];
        }

        /// <summary>
        /// Gets the visible image area in physical (client) coordinates.
        /// </summary>
        /// <returns>The visible image area in physical (client) coordinates.</returns>
        // This method performs a computation, so is better suited as a method
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public Rectangle GetVisibleImageArea()
        {
            return Rectangle.Intersect(base.PhysicalViewRectangle, base.ClientRectangle);
        }

        /// <summary>
        /// Returns the specified rectangle in either client or image coordinates.
        /// </summary>
        /// <param name="rectangle">The rectangle to return in a different coordinate system.
        /// </param>
        /// <param name="clientToImage"><see langword="true"/> if <paramref name="rectangle"/> is 
        /// in client coordinates and should be returned in image coordinates; 
        /// <see langword="false"/> if <paramref name="rectangle"/> is in image coordinates and 
        /// should be returned in client coordinates.</param>
        /// <returns><paramref name="rectangle"/> in the coordinate system specified.</returns>
        /// <remarks>The result is undefined if no image is open.</remarks>
        public Rectangle GetTransformedRectangle(Rectangle rectangle, bool clientToImage)
        {
            // Get the top-left and bottom-right corners of the rectangle.
            Point[] corners = new Point[] 
            {
                new Point(rectangle.Left, rectangle.Top),
                new Point(rectangle.Right, rectangle.Bottom)
            };

            // Check the direction of conversion
            if (clientToImage)
            {
                // Convert the corners from client coordinates to image coordinates
                using (Matrix clientToImageMatrix = _transform.Clone())
                {
                    clientToImageMatrix.Invert();
                    clientToImageMatrix.TransformPoints(corners);
                }
            }
            else
            {
                // Convert the corners from image to client coordinates
                _transform.TransformPoints(corners);
            }

            // Calculate the x coordinates of the rectangle
            int left = corners[0].X;
            int right = corners[1].X;
            if (left > right)
            {
                left = right;
                right = corners[0].X;
            }

            // Calculate the y coordinates of the rectangle
            int top = corners[0].Y;
            int bottom = corners[1].Y;
            if (top > bottom)
            {
                top = bottom;
                bottom = corners[0].Y;
            }

            // Construct and return the resultant rectangle
            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        /// <summary>
        /// Sets the fit mode to the specified value and optionally updates the zoom.
        /// </summary>
        /// <remarks>The zoom history is updated if and only if 
        /// <paramref name="updateZoomHistory"/> is <see langword="true"/> AND an image is open. 
        /// The <see cref="ZoomChanged"/> event is raised if and only if 
        /// <paramref name="raiseZoomChanged"/> is <see langword="true"/> AND an image is open. 
        /// </remarks>
        /// <param name="fitMode">The new fit mode.</param>
        /// <param name="updateZoomHistory"><see langword="true"/> if the zoom history should be 
        /// updated if an image is open; <see langword="false"/> if it should not be updated at 
        /// all.</param>
        /// <param name="raiseZoomChanged"><see langword="true"/> if the see cref="ZoomChanged"/> 
        /// event should be raised if an image is open; <see langword="false"/> if it should not 
        /// be raised at all.</param>
        /// <exception cref="ExtractException"><paramref name="fitMode"/> is invalid.</exception>
        /// <event cref="ZoomChanged"><paramref name="raiseZoomChanged"/> was 
        /// <see langword="true"/>.</event>
        /// <event cref="FitModeChanged">Method was successful.</event>
        private void SetFitMode(FitMode fitMode, bool updateZoomHistory, bool raiseZoomChanged)
        {
            switch (fitMode)
            {
                case FitMode.FitToPage:

                    // Suspend the paint event until the fit mode has changed
                    base.BeginUpdate();
                    try
                    {
                        base.SizeMode = RasterPaintSizeMode.FitAlways;
                        base.ScaleFactor = _DEFAULT_SCALE_FACTOR;
                    }
                    finally
                    {
                        base.EndUpdate();
                    }
                    break;

                case FitMode.FitToWidth:

                    // Suspend the paint event until the fit mode has changed
                    base.BeginUpdate();
                    try
                    {
                        base.SizeMode = RasterPaintSizeMode.FitWidth;
                        base.ScaleFactor = _DEFAULT_SCALE_FACTOR;
                    }
                    finally
                    {
                        base.EndUpdate();
                    }
                    break;

                case FitMode.None:
                    base.ZoomToRectangle(base.DisplayRectangle);
                    break;

                default:
                    throw new ExtractException("ELI21234", "Unrecognized FitMode value.");
            }

            _fitMode = fitMode;
            RegistryManager.FitMode = fitMode;

            // Update the zoom history if necessary
            if (base.Image != null)
            {
                UpdateZoom(updateZoomHistory, raiseZoomChanged);
            }

            // Raise the FitModeChanged event.
            OnFitModeChanged(new FitModeChangedEventArgs(_fitMode));
        }

        /// <summary>
        /// Sets the zoom info and optionally updates the zoom history.
        /// </summary>
        /// <param name="zoomInfo">The new zoom info.</param>
        /// <param name="updateZoomHistory"><see langword="true"/> if the zoom history should be 
        /// updated; <see langword="false"/> if it should not be updated.</param>
        /// <event cref="ZoomChanged">Method was successful.</event>
        private void SetZoomInfo(ZoomInfo zoomInfo, bool updateZoomHistory)
        {
            // Suspend the paint event until the zoom setting has changed
            base.BeginUpdate();
            try
            {
                // Check if the fit mode is specified
                if (zoomInfo.FitMode != FitMode.None)
                {
                    SetFitMode(zoomInfo.FitMode, updateZoomHistory, true);
                }
                else
                {
                    // Reset the fit mode if it is set
                    if (_fitMode != FitMode.None)
                    {
                        SetFitMode(FitMode.None, false, false);
                    }

                    // Set the new scale factor
                    base.ScaleFactor = zoomInfo.ScaleFactor;
                }

                // Convert the center point to client coordinates
                Point[] center = new Point[] { zoomInfo.Center };
                _transform.TransformPoints(center);

                // Center at the specified point
                base.CenterAtPoint(center[0]);
            }
            finally
            {
                base.EndUpdate();
            }

            // Update the zoom history if necessary and raise the zoom changed event
            UpdateZoom(updateZoomHistory, true);
        }

        /// <summary>
        /// Loads the default shortcut keys into <see cref="_mainShortcuts"/>.
        /// </summary>
        private void LoadDefaultShortcuts()
        {
            // Open an image
            _mainShortcuts[Keys.O | Keys.Control] = SelectOpenImage;

            // Close an image
            _mainShortcuts[Keys.Control | Keys.F4] = CloseImage;

            // Print an image
            _mainShortcuts[Keys.P | Keys.Control] = SelectPrint;

            // Go to the next page
            _mainShortcuts[Keys.PageDown] = GoToNextPage;

            // Go to the previous page
            _mainShortcuts[Keys.PageUp] = GoToPreviousPage;

            // Fit to page
            _mainShortcuts[Keys.P] = ToggleFitToPageMode;

            // Fit to width
            _mainShortcuts[Keys.W] = ToggleFitToWidthMode;

            // Zoom window tool
            _mainShortcuts[Keys.Z] = SelectZoomWindowTool;

            // Zoom in
            _mainShortcuts[Keys.F7] = SelectZoomIn;
            _mainShortcuts[Keys.Add | Keys.Control] = SelectZoomIn;
            _mainShortcuts[Keys.Oemplus | Keys.Control] = SelectZoomIn;

            // Zoom out
            _mainShortcuts[Keys.F8] = SelectZoomOut;
            _mainShortcuts[Keys.Subtract | Keys.Control] = SelectZoomOut;
            _mainShortcuts[Keys.OemMinus | Keys.Control] = SelectZoomOut;

            // Zoom previous
            _mainShortcuts[Keys.R] = SelectZoomPrevious;
            _mainShortcuts[Keys.Left | Keys.Alt] = SelectZoomPrevious;

            // Zoom next
            _mainShortcuts[Keys.T] = SelectZoomNext;
            _mainShortcuts[Keys.Right | Keys.Alt] = SelectZoomNext;

            // Pan tool
            _mainShortcuts[Keys.A] = SelectPanTool;

            // Select layer object tool
            _mainShortcuts[Keys.Escape] = SelectSelectLayerObjectsTool;

            // Highlight tool
            _mainShortcuts[Keys.H] = ToggleHighlightTool;

            // Go to first page
            _mainShortcuts[Keys.Control | Keys.Home] = GoToFirstPage;

            // Go to last page
            _mainShortcuts[Keys.Control | Keys.End] = GoToLastPage;

            // Rotate clockwise
            _mainShortcuts[Keys.R | Keys.Control] = SelectRotateClockwise;

            // Rotate counterclockwise
            _mainShortcuts[Keys.R | Keys.Control | Keys.Shift] = SelectRotateCounterclockwise;

            // Delete selected highlights
            _mainShortcuts[Keys.Delete] = SelectRemoveSelectedLayerObjects;

            // Select all highlights
            _mainShortcuts[Keys.A | Keys.Control] = SelectSelectAllLayerObjects;

            // Go to previous tile
            _mainShortcuts[Keys.Oemcomma] = SelectPreviousTile;

            // Go to next tile
            _mainShortcuts[Keys.OemPeriod] = SelectNextTile;

            // Go to next layer object
            _mainShortcuts[Keys.F3] = GoToNextLayerObject;
            _mainShortcuts[Keys.Control | Keys.OemPeriod] = GoToNextLayerObject;

            // Go to previous layer object
            _mainShortcuts[Keys.F3 | Keys.Shift] = GoToPreviousLayerObject;
            _mainShortcuts[Keys.Control | Keys.Oemcomma] = GoToPreviousLayerObject;

            // Increase highlight height
            _captureShortcuts[Keys.Oemplus] = IncreaseHighlightHeight;
            _captureShortcuts[Keys.Add] = IncreaseHighlightHeight;

            // Decrease highlight height
            _captureShortcuts[Keys.OemMinus] = DecreaseHighlightHeight;
            _captureShortcuts[Keys.Subtract] = DecreaseHighlightHeight;
        }

        /// <summary>
        /// Increases or decreases the default highlight height during an interactive mouse event.
        /// </summary>
        /// <remarks>Height will be incremented or decremented by the value of 
        /// <see cref="_MOUSEWHEEL_HEIGHT_INCREMENT"/>.</remarks>
        /// <param name="mouseX">The current mouse position's x-coordinate in physical (client) 
        /// pixels. Used to redraw the angular highlight.</param>
        /// <param name="mouseY">The current mouse position's y-coordinate in physical (client) 
        /// pixels. Used to redraw the angular highlight.</param>
        /// <param name="increaseHeight"></param>
        private void AdjustHighlightHeight(int mouseX, int mouseY, bool increaseHeight)
        {
            // Adjust the highlight height only if an interactive angular highlight is being drawn.
            if (_trackingData != null &&
                (_cursorTool == CursorTool.AngularHighlight || _cursorTool == CursorTool.AngularRedaction))
            {
                // Increase/decrease highlight height as specified
                _defaultHighlightHeight += (increaseHeight ?
                    _MOUSEWHEEL_HEIGHT_INCREMENT : -_MOUSEWHEEL_HEIGHT_INCREMENT);

                // Enforce a minimum highlight height
                if (_defaultHighlightHeight < LayerObject.MinSize.Height)
                {
                    _defaultHighlightHeight = LayerObject.MinSize.Height;
                }

                // Convert the highlight height to physical (client) coordinates
                int height = (int)(_defaultHighlightHeight * GetScaleFactorY(_transform) + 0.5);

                // Update the tracking data
                _trackingData.Height = height;
                UpdateTracking(mouseX, mouseY);
            }
        }

        /// <overloads>Gets a restricted collection of layer objects.</overloads>
        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="LayerObject"/> objects that contain
        /// one or more of the specified required tags.
        /// </summary>
        /// <param name="requiredTags">A <see cref="IEnumerable{T}"/> of tags to restrict
        /// the returned <see cref="LayerObject"/> collection by. Pass <see langword="null"/>
        /// to not restrict by tags.</param>
        /// <param name="tagsArgumentRequirement">A <see cref="ArgumentRequirement"/>
        /// indicating whether the found layer object must have any or all of
        /// the specified tags.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="LayerObject"/> that
        /// contain one or more of the specified tags.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<LayerObject> GetLayeredObjects(IEnumerable<string> requiredTags,
            ArgumentRequirement tagsArgumentRequirement)
        {
            try
            {
                // Create the return list
                List<LayerObject> returnList = new List<LayerObject>();

                // If no tag restrictions, just return all layer objects
                if (requiredTags == null)
                {
                    returnList.AddRange(_layerObjects);
                }
                else
                {
                    // Loop through all of the layer objects
                    foreach (LayerObject layerObject in _layerObjects)
                    {
                        // Check for proper tags
                        if (LayeredObjectHasProperTag(layerObject, requiredTags,
                            tagsArgumentRequirement))
                        {
                            // Has proper tags, add to return collection
                            returnList.Add(layerObject);
                        }
                    }
                }

                // Return the restricted collection
                return returnList;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22255", ex);
            }
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="LayerObject"/> objects that contain
        /// one or more of the specified required tags and is of at least one of the
        /// required types and not one of the excluded types.
        /// </summary>
        /// <param name="requiredTags">A <see cref="IEnumerable{T}"/> of tags to restrict
        /// the returned <see cref="LayerObject"/> collection by. Pass <see langword="null"/>
        /// to not restrict by tags.</param>
        /// <param name="tagsArgumentRequirement">A <see cref="ArgumentRequirement"/>
        /// indicating whether the found layer object must have any or all of
        /// the specified tags.</param>
        /// <param name="requiredTypes">A <see cref="IEnumerable{T}"/> of <see cref="Type"/>
        /// used to restrict the returned collection.  If the <see cref="IEnumerable{T}"/>
        /// is empty then the returned collection will be empty, otherwise the
        /// returned collection will contain only objects of <see cref="Type"/>
        /// that are contained in the <see cref="IEnumerable{T}"/>. Pass <see langword="null"/>
        /// to not restrict by types.</param>
        /// <param name="requiredTypesArgumentRequirement">A <see cref="ArgumentRequirement"/>
        /// indicating whether the found layer object must be an object of any or
        /// all of the specified types.</param>
        /// <param name="excludeTypes">A <see cref="IEnumerable{T}"/> of <see cref="Type"/>
        /// used to restrict the returned collection.  If the <see cref="IEnumerable{T}"/>
        /// is empty then it will not restrict the collection, otherwise the
        /// returned collection will not contain any objects that are of <see cref="Type"/>
        /// that are contained in the <see cref="IEnumerable{T}"/>. Pass <see langword="null"/>
        /// to not exclude types.</param>
        /// <param name="excludeTypesArgumentRequirement">A <see cref="ArgumentRequirement"/>
        /// inidicating whether the found layer object must not be an object of
        /// any or all of the specified types.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="LayerObject"/> that
        /// contain (any or all depending on <paramref name="tagsArgumentRequirement"/>)
        /// of the specified tags, that are (any or all depending on
        /// <paramref name="requiredTypesArgumentRequirement"/>) of the types listed
        /// in the <paramref name="requiredTypes"/>, and that are not (any or all depending on
        /// <paramref name="excludeTypesArgumentRequirement"/>) of the types
        /// listed in the <paramref name="excludeTypes"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<LayerObject> GetLayeredObjects(IEnumerable<string> requiredTags,
            ArgumentRequirement tagsArgumentRequirement, IEnumerable<Type> requiredTypes, 
            ArgumentRequirement requiredTypesArgumentRequirement, IEnumerable<Type> excludeTypes,
            ArgumentRequirement excludeTypesArgumentRequirement)
        {
            try
            {
                // Create the return list
                List<LayerObject> returnList = new List<LayerObject>();

                // Loop through each of the layer objects in the collection that has
                // been restricted by the required tags
                foreach (LayerObject layerObject in GetLayeredObjects(requiredTags,
                    tagsArgumentRequirement))
                {
                    if (LayeredObjectIsProperType(layerObject, requiredTypes,
                        requiredTypesArgumentRequirement, excludeTypes,
                        excludeTypesArgumentRequirement))
                    {
                        returnList.Add(layerObject);
                    }
                }

                // Return the list
                return returnList;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22256", ex);
            }
        }

        /// <overloads>Gets a restricted collection of layer objects.</overloads>
        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="LayerObject"/> objects that contain
        /// one or more of the specified required tags and are found on the specified page.
        /// </summary>
        /// <param name="pageNumber">The page to restrict the return collection for.</param>
        /// <param name="requiredTags">A <see cref="IEnumerable{T}"/> of tags to restrict
        /// the returned <see cref="LayerObject"/> collection by. Pass <see langword="null"/>
        /// to not restrict by tags.</param>
        /// <param name="tagsArgumentRequirement">A <see cref="ArgumentRequirement"/>
        /// indicating whether the found layer object must have any or all of
        /// the specified tags.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="LayerObject"/> that
        /// contain one or more of the specified tags and are on the specified page.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<LayerObject> GetLayeredObjectsOnPage(int pageNumber, IEnumerable<string> requiredTags,
            ArgumentRequirement tagsArgumentRequirement)
        {
            try
            {
                // Create the return list
                List<LayerObject> returnList = new List<LayerObject>();

                // If no tag restrictions, just restrict by page
                if (requiredTags == null)
                {
                    foreach (LayerObject layerObject in _layerObjects)
                    {
                        if (layerObject.PageNumber == pageNumber)
                        {
                            returnList.Add(layerObject);
                        }
                    }
                }
                else
                {
                    // Loop through all of the layer objects
                    foreach (LayerObject layerObject in _layerObjects)
                    {
                        // If it is not on the specified page then just move to the next object
                        if (layerObject.PageNumber != pageNumber)
                        {
                            continue;
                        }

                        // Check for proper tags
                        if (LayeredObjectHasProperTag(layerObject, requiredTags,
                            tagsArgumentRequirement))
                        {
                            // Has proper tags, add to return collection
                            returnList.Add(layerObject);
                        }
                    }
                }

                // Return the restricted collection
                return returnList;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22257", ex);
            }
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="LayerObject"/> objects that contain
        /// one or more of the specified required tags and is of at least one of the
        /// required types and not one of the excluded types and are found on the specified
        /// page.
        /// </summary>
        /// <param name="pageNumber">The page to restrict the return collection for.</param>
        /// <param name="requiredTags">A <see cref="IEnumerable{T}"/> of tags to restrict
        /// the returned <see cref="LayerObject"/> collection by. Pass <see langword="null"/>
        /// to not restrict by tags.</param>
        /// <param name="tagsArgumentRequirement">A <see cref="ArgumentRequirement"/>
        /// indicating whether the found layer object must have any or all of
        /// the specified tags.</param>
        /// <param name="requiredTypes">A <see cref="IEnumerable{T}"/> of <see cref="Type"/>
        /// used to restrict the returned collection.  If the <see cref="IEnumerable{T}"/>
        /// is empty then the returned collection will be empty, otherwise the
        /// returned collection will contain only objects of <see cref="Type"/>
        /// that are contained in the <see cref="IEnumerable{T}"/>. Pass <see langword="null"/>
        /// to not restrict by types.</param>
        /// <param name="requiredTypesArgumentRequirement"> A <see cref="ArgumentRequirement"/>
        /// indicating whether the found layer object must be an object of any or
        /// all of the specified types.</param>
        /// <param name="excludeTypes">A <see cref="IEnumerable{T}"/> of <see cref="Type"/>
        /// used to restrict the returned collection.  If the <see cref="IEnumerable{T}"/>
        /// is empty then it will not restrict the collection, otherwise the
        /// returned collection will not contain any objects that are of <see cref="Type"/>
        /// that are contained in the <see cref="IEnumerable{T}"/>. Pass <see langword="null"/>
        /// to not exclude types.</param>
        /// <param name="excludeTypesArgumentRequirement">A <see cref="ArgumentRequirement"/>
        /// inidicating whether the found layer object must not be an object of
        /// any or all of the specified types.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="LayerObject"/> that
        /// contain (any or all depending on <paramref name="tagsArgumentRequirement"/>)
        /// of the specified tags, that are (any or all depending on
        /// <paramref name="requiredTypesArgumentRequirement"/>) of the types listed
        /// in the <paramref name="requiredTypes"/>, and that are not (any or all depending on
        /// <paramref name="excludeTypesArgumentRequirement"/>) of the types
        /// listed in the <paramref name="excludeTypes"/> and found on the specified page.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<LayerObject> GetLayeredObjectsOnPage(int pageNumber, IEnumerable<string> requiredTags,
            ArgumentRequirement tagsArgumentRequirement, IEnumerable<Type> requiredTypes,
            ArgumentRequirement requiredTypesArgumentRequirement, IEnumerable<Type> excludeTypes,
            ArgumentRequirement excludeTypesArgumentRequirement)
        {
            try
            {
                // Create the return list
                List<LayerObject> returnList = new List<LayerObject>();

                // Loop through each of the layer objects in the collection that has
                // been restricted by the required tags and page number
                foreach (LayerObject layerObject in GetLayeredObjectsOnPage(pageNumber,
                    requiredTags, tagsArgumentRequirement))
                {
                    if (LayeredObjectIsProperType(layerObject, requiredTypes,
                        requiredTypesArgumentRequirement, excludeTypes,
                        excludeTypesArgumentRequirement))
                    {
                        returnList.Add(layerObject);
                    }
                }

                // Return the restricted collection
                return returnList;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22258", ex);
            }
        }

        /// <summary>
        /// Gets whether the <see cref="LayerObject"/> meets the specified tag requirements.
        /// </summary>
        /// <param name="layerObject">The <see cref="LayerObject"/> to check.</param>
        /// <param name="requiredTags">The <see cref="IEnumerable{T}"/> of
        /// tags to include. This may not be <see langword="null"/>.</param>
        /// <param name="tagsArgumentRequirement">The <see cref="ArgumentRequirement"/>
        /// specifying how to treat the collection of tags.</param>
        /// <returns><see langword="true"/> if the <see cref="LayerObject"/> meets the
        /// requirements and <see langword="false"/> if it does not.</returns>
        /// <exception cref="ExtractException">If <paramref name="requiredTags"/> is
        /// <see langword="null"/>.</exception>
        private static bool LayeredObjectHasProperTag(LayerObject layerObject,
            IEnumerable<string> requiredTags, ArgumentRequirement tagsArgumentRequirement)
        {
            // Ensure the required tags is not null
            ExtractException.Assert("ELI22339", "Required tags may not be null!",
                requiredTags != null);

            // Get each of the objects tags and check
            // that it contains the required tags (any or all
            // depending on the tagsRequirement)
            bool hasTags = true;
            foreach (string tag in requiredTags)
            {
                hasTags = layerObject.Tags.Contains(tag);
                if (tagsArgumentRequirement == ArgumentRequirement.Any)
                {
                    if (hasTags)
                    {
                        break;
                    }
                }
                else
                {
                    if (!hasTags)
                    {
                        break;
                    }
                }
            }

            // Return whether it contained any/all the specified tags
            return hasTags;
        }

        /// <summary>
        /// Gets whether the <see cref="LayerObject"/> meets the specified type requirements.
        /// </summary>
        /// <param name="layerObject">The <see cref="LayerObject"/> to check.</param>
        /// <param name="requiredTypes">The <see cref="IEnumerable{T}"/> of
        /// <see cref="Type"/> to include.</param>
        /// <param name="requiredTypesArgumentRequirement">The <see cref="ArgumentRequirement"/>
        /// specifying how to treat the <paramref name="requiredType"/> collection.</param>
        /// <param name="excludeTypes">The <see cref="IEnumerable{T}"/> of
        /// <see cref="Type"/> to exclude.</param>
        /// <param name="excludeTypesArgumentRequirement">The <see cref="ArgumentRequirement"/>
        /// specifying how to treat the <paramref name="excludedType"/> collection.</param>
        /// <returns><see langword="true"/> if the <see cref="LayerObject"/> meets the
        /// requirements and <see langword="false"/> if it does not.</returns>
        private static bool LayeredObjectIsProperType(LayerObject layerObject,
            IEnumerable<Type> requiredTypes, ArgumentRequirement requiredTypesArgumentRequirement,
            IEnumerable<Type> excludeTypes, ArgumentRequirement excludeTypesArgumentRequirement)
        {
            // Check if the object has (any or all depending on the
            // ArgumentRequirement) of the required types.
            bool hasRequired = true;
            if (requiredTypes != null)
            {
                foreach (Type type in requiredTypes)
                {
                    hasRequired = type.IsInstanceOfType(layerObject);

                    if (requiredTypesArgumentRequirement == ArgumentRequirement.Any)
                    {
                        if (hasRequired)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (!hasRequired)
                        {
                            break;
                        }
                    }
                }
            }

            // If it does not contain a required type return false
            if (!hasRequired)
            {
                return false;
            }

            // Check if the object has (any or all depending on the
            // ArgumentRequirement) of the excluded types.
            bool containsExcluded = false;
            if (excludeTypes != null)
            {
                foreach (Type type in excludeTypes)
                {
                    containsExcluded = type.IsInstanceOfType(layerObject);

                    if (excludeTypesArgumentRequirement == ArgumentRequirement.Any)
                    {
                        if (containsExcluded)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (!containsExcluded)
                        {
                            break;
                        }
                    }
                }
            }

            // Return whether it contains excluded type/types
            return !containsExcluded;
        }

        /// <summary>
        /// Will set the visible flag to specified state for all objects matching the
        /// collection of tags and types
        /// </summary>
        /// <param name="tags">An <see cref="IEnumerable{T}"/> of <see cref="string"/>
        /// that represent the tags to match.</param>
        /// <param name="includeTypes">An <see cref="IEnumerable{T}"/> of
        /// <see cref="Type"/> that represent the types to match.</param>
        /// <param name="visibleState">The value to set the visible flag to.</param>
        public void SetVisibleStateForSpecifiedLayerObjects(IEnumerable<string> tags,
            IEnumerable<Type> includeTypes, bool visibleState)
        {
            try
            {
                // Set the visible state of the layer objects
                foreach (LayerObject layerObject in this.GetLayeredObjects(tags,
                    ArgumentRequirement.Any, includeTypes, ArgumentRequirement.Any,
                    null, ArgumentRequirement.Any))
                {
                    layerObject.Visible = visibleState;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22507", ex);
            }
        }

        /// <summary>
        /// Creates a new font in the specified units from another font.
        /// </summary>
        /// <param name="font">The font from which to create a new font.</param>
        /// <param name="unit">The unit of measure for the new font.</param>
        /// <returns>A new font measured by <paramref name="unit"/> from <paramref name="font"/>.
        /// </returns>
        /// <remarks>This method does not dispose of the input font or the output font.</remarks>
        public Font ConvertFontToUnits(Font font, GraphicsUnit unit)
        {
            try
            {
                // Get the input font size in pixels
                float fontSize = GetFontSizeInPixels(font);

                // Get the input font size in the specified units
                fontSize = ConvertPixelsToEmUnits(fontSize, unit);

                // Create the new font with the specified units
                return new Font(font.FontFamily, fontSize, font.Style, unit, font.GdiCharSet,
                    font.GdiVerticalFont);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23220", ex);
            }
        }

        /// <summary>
        /// Calculates the font size in logical (image) pixels.
        /// </summary>
        /// <param name="font">The font from which to retrieve the font size.</param>
        /// <returns>The font size in logical (image) pixels.</returns>
        float GetFontSizeInPixels(Font font)
        {
            switch (font.Unit)
            {
                case GraphicsUnit.Document:
                    // (1 inch = 300 document units)
                    return font.Size * base.Image.YResolution / 300F;

                case GraphicsUnit.Inch:
                    return font.Size * base.Image.YResolution;

                case GraphicsUnit.Millimeter:
                    // (1 inch = 25.4 millimeters)
                    return font.Size * base.Image.YResolution / 25.4F;

                case GraphicsUnit.Pixel:
                    return font.Size;

                case GraphicsUnit.Point:
                    // (1 inch = 72 points)
                    return font.Size * base.Image.YResolution / 72F;

                case GraphicsUnit.Display:
                case GraphicsUnit.World:
                    throw new NotImplementedException();

                default:
                    ExtractException ee = 
                        new ExtractException("ELI23218", "Unexpected graphics unit.");
                    ee.AddDebugData("Unit", font.Unit, false);
                    throw ee;
            }
        }

        /// <summary>
        /// Calculates font size in the units specified.
        /// </summary>
        /// <param name="pixels">The font size in logical (image) pixels.</param>
        /// <param name="unit">The </param>
        /// <returns></returns>
        float ConvertPixelsToEmUnits(float pixels, GraphicsUnit unit)
        {
            switch (unit)
            {
                case GraphicsUnit.Document:
                    // (1 inch = 300 document units)
                    return pixels * 300F / base.Image.YResolution;

                case GraphicsUnit.Inch:
                    return pixels / base.Image.YResolution;

                case GraphicsUnit.Millimeter:
                    // (1 inch = 25.4 millimeters)
                    return pixels * 25.4F / base.Image.YResolution;

                case GraphicsUnit.Pixel:
                    return pixels;

                case GraphicsUnit.Point:
                    // (1 inch = 72 points)
                    return pixels * 72F / base.Image.YResolution;

                case GraphicsUnit.Display:
                case GraphicsUnit.World:
                    throw new NotImplementedException();

                default:
                    ExtractException ee = 
                        new ExtractException("ELI23219", "Unexpected graphics unit.");
                    ee.AddDebugData("Unit", unit, false);
                    throw ee;
            }
        }

        /// <summary>
        /// Will restore the scroll position to its last saved state.
        /// </summary>
        /// <seealso cref="OnScrollPositionChanged"/>
        public void RestoreScrollPosition()
        {
            // Restore to previously saved scroll position [DNRCAU #262 - JDS]
            base.ScrollPosition = _scrollPosition;

            // Refresh the image viewer
            base.Invalidate();
            Application.DoEvents();
        }

        /// <overloads>Checks whether the specified object is contained on the current page.
        /// </overloads>
        /// <summary>
        /// Checks whether the specified layer object is completely contained on the current page.
        /// </summary>
        /// <param name="layerObject">The layer object to test for containment.</param>
        /// <returns><see langword="true"/> if the layer object is completely contained on the 
        /// current page; <see langword="false"/> if the layer object is completely or partially 
        /// off the page.</returns>
        public bool Contains(LayerObject layerObject)
        {
            return Contains(layerObject.GetBounds());
        }

        /// <summary>
        /// Checks whether the rectangle in logical (image) coordinates is completely contained on 
        /// the current page.
        /// </summary>
        /// <param name="rectangle">The rectangle to test for containment in logical (image) 
        /// coordinates.</param>
        /// <returns><see langword="true"/> if the rectangle is completely contained on the 
        /// current page; <see langword="false"/> if the rectangle is completely or partially 
        /// off the page.</returns>
        public bool Contains(RectangleF rectangle)
        {
            return rectangle.Left >= 0 && rectangle.Top >= 0 &&
                rectangle.Right <= this.ImageWidth && rectangle.Bottom <= this.ImageHeight;
        }

        /// <overloads>Checks whether any part of the specified object appears on the current page.
        /// </overloads>
        /// <summary>
        /// Checks whether any part of the specified layer object is contained on the current page.
        /// </summary>
        /// <param name="layerObject">The layer object to test for intersection.</param>
        /// <returns><see langword="true"/> if the layer object is completely or partially 
        /// contained by the current page; <see langword="false"/> if the layer object is 
        /// completely off the page.</returns>
        public bool Intersects(LayerObject layerObject)
        {
            return Intersects(layerObject.GetBounds());
        }

        /// <summary>
        /// Checks whether any part of the rectangle in logical (image) coordinates is contained 
        /// on the current page.
        /// </summary>
        /// <param name="rectangle">The rectangle to test for intersection in logical (image) 
        /// coordinates.</param>
        /// <returns><see langword="true"/> if the rectangle is completely or partially 
        /// contained by the current page; <see langword="false"/> if the rectangle is 
        /// completely off the page.</returns>
        public bool Intersects(RectangleF rectangle)
        {
            return rectangle.Left < this.ImageWidth && rectangle.Top < this.ImageHeight &&
                rectangle.Right > 0 && rectangle.Bottom > 0;
        }

        #region Image Viewer Shortcut Key Methods

        /// <summary>
        /// Activates the pan <see cref="CursorTool"/>.
        /// </summary>
        public void SelectPanTool()
        {
            try
            {
                if (base.Image != null)
                {
                    this.CursorTool = CursorTool.Pan;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24064", ex);
            }
        }

        /// <summary>
        /// Activates the zoom window <see cref="CursorTool"/>.
        /// </summary>
        public void SelectZoomWindowTool()
        {
            try
            {
                if (base.Image != null)
                {
                    this.CursorTool = CursorTool.ZoomWindow;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24065", ex);
            }
        }

        /// <summary>
        /// Activates either the rectangular or angular highlight <see cref="CursorTool"/>, 
        /// whichever was used last.
        /// </summary>
        /// <remarks>If the currently active tool is one of the two highlight tools, activates 
        /// the other. If neither of the two highlight tools are activated, activates which ever 
        /// highlight tool was most recently used.</remarks>
        public void ToggleHighlightTool()
        {
            try
            {
                if (base.Image != null)
                {
                    // If the current tool is a highlight tool, toggle it
                    if (_cursorTool == CursorTool.RectangularHighlight)
                    {
                        this.CursorTool = CursorTool.AngularHighlight;
                    }
                    else if (_cursorTool == CursorTool.AngularHighlight)
                    {
                        this.CursorTool = CursorTool.RectangularHighlight;
                    }
                    else
                    {
                        // Select the last used highlight tool
                        this.CursorTool = RegistryManager.GetLastUsedHighlightTool();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22287", ex);
            }
        }

        /// <summary>
        /// Activates the angular highlight <see cref="CursorTool"/>.
        /// </summary>
        public void SelectAngularHighlightTool()
        {
            try
            {
                if (base.Image != null)
                {
                    this.CursorTool = CursorTool.AngularHighlight;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24066", ex);
            }
        }

        /// <summary>
        /// Activates the rectangular highlight <see cref="CursorTool"/>.
        /// </summary>
        public void SelectRectangularHighlightTool()
        {
            try
            {
                if (base.Image != null)
                {
                    this.CursorTool = CursorTool.RectangularHighlight;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24067", ex);
            }
        }

        /// <summary>
        /// Activates either the rectangular or angular redaction <see cref="CursorTool"/>, 
        /// whichever was used last.
        /// </summary>
        /// <remarks>If the currently active tool is one of the two redaction tools, activates 
        /// the other. If neither of the two redaction tools are activated, activates which ever 
        /// redaction tool was most recently used.</remarks>
        public void ToggleRedactionTool()
        {
            try
            {
                if (base.Image != null)
                {
                    // If the current tool is a redaction tool, toggle it
                    if (_cursorTool == CursorTool.RectangularRedaction)
                    {
                        this.CursorTool = CursorTool.AngularRedaction;
                    }
                    else if (_cursorTool == CursorTool.AngularRedaction)
                    {
                        this.CursorTool = CursorTool.RectangularRedaction;
                    }
                    else
                    {
                        // Select the last used redaction tool
                        this.CursorTool = RegistryManager.GetLastUsedRedactionTool();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22288", ex);
            }
        }

        /// <summary>
        /// Activates the edit highlight text <see cref="CursorTool"/>.
        /// </summary>
        public void SelectEditHighlightTextTool()
        {
            try
            {
                if (base.Image != null)
                {
                    this.CursorTool = CursorTool.EditHighlightText;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24068", ex);
            }
        }

        /// <summary>
        /// Activates the delete highlights <see cref="CursorTool"/>.
        /// </summary>
        public void SelectDeleteLayerObjectsTool()
        {
            try
            {
                if (base.Image != null)
                {
                    this.CursorTool = CursorTool.DeleteLayerObjects;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24069", ex);
            }
        }

        /// <summary>
        /// Activates the select layer objects <see cref="CursorTool"/>.
        /// </summary>
        public void SelectSelectLayerObjectsTool()
        {
            try
            {
                if (base.Image != null)
                {
                    this.CursorTool = CursorTool.SelectLayerObject;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24070", ex);
            }
        }

        /// <summary>
        /// Goes to the first page in the current image.
        /// </summary>
        public void GoToFirstPage()
        {
            try
            {
                // Ensure an image is open
                if (base.Image != null)
                {
                    this.PageNumber = 1;
                }
            }
            catch (Exception e)
            {
                ExtractException.Display("ELI21147", e);
            }
        }

        /// <summary>
        /// Goes to the previous page in the current image.
        /// </summary>
        public void GoToPreviousPage()
        {
            try
            {
                // Ensure an image is open
                if (base.Image != null)
                {
                    // Ensure this is not the first page
                    int page = base.Image.Page;
                    if (page > 1)
                    {
                        // Go to the previous page
                        this.PageNumber = page - 1;
                    }
                }
            }
            catch (Exception e)
            {
                ExtractException.Display("ELI21148", e);
            }
        }

        /// <summary>
        /// Goes to the next page in the current image.
        /// </summary>
        public void GoToNextPage()
        {
            try
            {
                // Ensure an image is open
                if (base.Image != null)
                {
                    // Ensure this is not the last page
                    int page = base.Image.Page;
                    if (page < base.Image.PageCount)
                    {
                        // Go to the next page
                        this.PageNumber = page + 1;
                    }
                }
            }
            catch (Exception e)
            {
                ExtractException.Display("ELI21149", e);
            }
        }

        /// <summary>
        /// Goes to the last page in the current image.
        /// </summary>
        public void GoToLastPage()
        {
            try
            {
                // Ensure an image is open
                if (base.Image != null)
                {
                    this.PageNumber = base.Image.PageCount;
                }
            }
            catch (Exception e)
            {
                ExtractException.Display("ELI21150", e);
            }
        }

        /// <summary>
        /// Rotates the image 90 degrees clockwise.
        /// </summary>
        public void SelectRotateClockwise()
        {
            try
            {
                if (base.Image != null)
                {
                    // Rotate 90 degrees
                    this.Rotate(90, base.Image.Page);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24071", ex);
            }
        }

        /// <summary>
        /// Rotates the image 90 degrees counterclockwise.
        /// </summary>
        public void SelectRotateCounterclockwise()
        {
            try
            {
                if (base.Image != null)
                {
                    // Rotate 270 degrees
                    this.Rotate(270, base.Image.Page);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24072", ex);
            }
        }

        /// <summary>
        /// Activates fit to page mode.
        /// </summary>
        public void ToggleFitToPageMode()
        {
            try
            {
                // Toggle the fit mode and update the zoom
                SetFitMode(_fitMode == FitMode.FitToPage ? FitMode.None : FitMode.FitToPage,
                    true, true);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24073", ex);
            }
        }

        /// <summary>
        /// Activates fit to width mode.
        /// </summary>
        public void ToggleFitToWidthMode()
        {
            try
            {
                // Toggle the fit mode and update the zoom
                SetFitMode(_fitMode == FitMode.FitToWidth ? FitMode.None : FitMode.FitToWidth,
                    true, true);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24074", ex);
            }
        }

        /// <summary>
        /// Displays the open image dialog and allows user to open a new image.
        /// </summary>
        public void SelectOpenImage()
        {
            try
            {
                // Build the filter list
                StringBuilder filterList = new StringBuilder();
                foreach (string filter in _openImageFileTypeFilter)
                {
                    filterList.Append(filter);
                }

                // Ensure there is at least one entry in the filter list
                if (filterList.Length == 0)
                {
                    filterList.Append("All files (*.*)|*.*|");
                }

                // Add final pipe character
                filterList.Append("|");

                // Create the open file dialog
                using (OpenFileDialog fileDialog = new OpenFileDialog())
                {
                    // Add the filter list, index (ensure index is valid), and title to dialog
                    fileDialog.Filter = filterList.ToString();
                    fileDialog.FilterIndex = 
                        _openImageFilterIndex > _openImageFileTypeFilter.Count ?
                        1 : _openImageFilterIndex;
                    fileDialog.Title = "Open Image File";

                    // Show the dialog and bail if the user selected cancel
                    if (fileDialog.ShowDialog() == DialogResult.Cancel)
                    {
                        return;
                    }

                    // Open the specified file and update the MRU list
                    this.OpenImage(fileDialog.FileName, true);
                }
            }
            catch (Exception e)
            {
                ExtractException.Display("ELI21132", e);
            }
        }

        /// <summary>
        /// Activates the remove selected layer object command.
        /// </summary>
        public void SelectRemoveSelectedLayerObjects()
        {
            try 
	        {
		        // We are done if there are no selected layer objects
                if (_layerObjects.Selection.Count <= 0)
                {
                    return;
                }

                // Build a delete me collection of all selected objects that are deletable
                LayerObjectsCollection deleteMe = new LayerObjectsCollection();
                foreach (LayerObject layerObject in _layerObjects.Selection)
                {
                    if (layerObject.Deletable)
                    {
                        deleteMe.Add(layerObject);
                    }
                }

                // Check if there are non deleteable objects selected
                bool nonDeletableObjectSelected = deleteMe.Count != _layerObjects.Selection.Count;

                // Get the client area in logical (image) coordinates
                Rectangle clientArea = GetTransformedRectangle(base.ClientRectangle, true);

                // Iterate through the selected layer objects and:
                // 1) set layerObjectOnNonVisiblePage to true if any layer object resides on a 
                // page other than the currently visible page
                // 2) set layerObjectOutOfViewOnVisiblePage to true if there is a layer object on 
                // the currently visible page that is not in the current view window
                // 3) set unselectedLinkedLayerObject if any layer object is linked to a layer 
                // object that is not in the current selection
                bool layerObjectOnNonVisiblePage = false;
                bool layerObjectOutOfViewOnVisiblePage = false;
                bool unselectedLinkedLayerObject = false;
                foreach (LayerObject layerObject in deleteMe)
                {
                    // Check if the selected layer object is linked
                    if (!unselectedLinkedLayerObject && layerObject.IsLinked && 
                        IsLinkedToUnselectedLayerObject(layerObject))
                    {
                        unselectedLinkedLayerObject = true;
                    }

                    // Check if the selected layer object is on the visible page
                    if (layerObject.PageNumber == base.Image.Page)
                    {
                        // Check if the layer object is contained in the current view
                        if (!layerObject.IsContained(clientArea, base.Image.Page))
                        {
                            layerObjectOutOfViewOnVisiblePage = true;
                        }
                    }
                    else
                    {
                        // Layer object is on a non-visible page
                        layerObjectOnNonVisiblePage = true;
                    }
                }

                // Prompt before deleting a selected layer object that is not in full view 
                if (layerObjectOnNonVisiblePage || layerObjectOutOfViewOnVisiblePage)
                {
                    // Build the message text
                    StringBuilder text = 
                        new StringBuilder("The selection contains at least one object");
                    text.AppendLine();

                    // Optionally add text about a layer object that is not in view
                    if (layerObjectOutOfViewOnVisiblePage)
                    {
                        text.Append("on the current page that is not in full view");

                        // Optionally add text about layer objects on other pages
                        if (layerObjectOnNonVisiblePage)
                        {
                            text.AppendLine(" and");
                            text.AppendLine("at least one other object that is not on");
                            text.Append("the visible page");
                        }
                    }
                    else
                    {
                        // Add text about layer object on non-visible pages
                        text.Append("that is not on the visible page");
                    }
                    text.AppendLine(".");
                    text.AppendLine();
                    text.Append("Are you sure you want to delete the selection?");

                    // Prompt the user
                    DialogResult result = MessageBox.Show(text.ToString(), "Delete selection?", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                        MessageBoxDefaultButton.Button1, 0);
                    if (result == DialogResult.No)
                    {
                        // Cancel
                        return;
                    }
                }

                // Prompt before deleting a layer object that is linked to an unselected object
                if (unselectedLinkedLayerObject)
                {
                    using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
                    {
                        // Set the message box text
                        messageBox.Text = "The selection contains a linked object.";
                        messageBox.Caption = "Delete linked objects?";
                        messageBox.StandardIcon = MessageBoxIcon.Question;

                        // Add the buttons to message box
                        messageBox.AddButton("Delete selected objects" + Environment.NewLine + 
                            "and all their linked objects", "All", false);
                        messageBox.AddButton("Delete selected objects only", "Selected", false);
                        messageBox.AddButton("Cancel", "Cancel", true);

                        // Prompt the user
                        string result = messageBox.Show();
                        if (result == "Cancel")
                        {
                            return;
                        }
                        else if (result == "All")
                        {
                            // Find all unselected linked layer objects
                            LayerObjectsCollection deleteMeToo = new LayerObjectsCollection();
                            foreach (LayerObject layerObject in deleteMe)
                            {
                                // Add any preceding linked objects that are unselected
                                LayerObject link = layerObject.PreviousLink;
                                while (link != null)
                                {
                                    if (!deleteMe.Contains(link) && !deleteMeToo.Contains(link))
                                    {
                                        deleteMeToo.Add(link);
                                    }

                                    link = link.PreviousLink;
                                }

                                // Check if any subsequent link is unselected
                                link = layerObject.NextLink;
                                while (link != null)
                                {
                                    if (!deleteMe.Contains(link) && !deleteMeToo.Contains(link))
                                    {
                                        deleteMeToo.Add(link);
                                    }

                                    link = link.NextLink;
                                }
                            }

                            // Add the new layer objects to the collection to delete
                            foreach (LayerObject layerObject in deleteMeToo)
                            {
                                deleteMe.Add(layerObject);
                            }
                        }
                    }
                }

                if (deleteMe.Count > 0)
                {
                    // Delete the selected layer objects
                    _layerObjects.Remove(deleteMe);
                }

                // Refresh the image viewer
                base.Invalidate();

                if (nonDeletableObjectSelected)
                {
                    // Prompt if there were non-deletable objects that were not removed
                    using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
                    {
                        messageBox.Text = "The selection contained non-deletable objects. These objects have not been removed.";
                        messageBox.Caption = "Some Objects Not Deleted";
                        messageBox.StandardIcon = MessageBoxIcon.Information;
                        messageBox.AddStandardButtons(MessageBoxButtons.OK);
                        messageBox.Show();
                    }
                }
	        }
	        catch (Exception ex)
	        {
		        ExtractException.Display("ELI22599", ex);
	        }
        }

        /// <summary>
        /// Determines whether the specified layer object is linked to an unselected layer object.
        /// </summary>
        /// <param name="layerObject">The layer object to check for unselected links.</param>
        /// <returns><see langword="true"/> if <paramref name="layerObject"/> is linked to an 
        /// unselected layer object; <see langword="false"/> if <paramref name="layerObject"/> not 
        /// linked to any unselected layer object.</returns>
        private static bool IsLinkedToUnselectedLayerObject(LayerObject layerObject)
        {
            // Check if any preceding link is unselected
            LayerObject link = layerObject.PreviousLink;
            while (link != null)
            {
                if (!link.Selected)
                {
                    return true;
                }

                link = link.PreviousLink;
            }

            // Check if any subsequent link is unselected
            link = layerObject.NextLink;
            while (link != null)
            {
                if (!link.Selected)
                {
                    return true;
                }

                link = link.NextLink;
            }

            // All previous and subsequent links were selected
            return false;
        }

        /// <summary>
        /// Activates the select all highlights command.
        /// </summary>
        public void SelectSelectAllLayerObjects()
        {
            try
            {
                _layerObjects.SelectAll();
                base.Invalidate();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24075", ex);
            }
        }

        /// <summary>
        /// Increases the default highlight height by two pixels.
        /// </summary>
        public void IncreaseHighlightHeight()
        {
            try
            {
                // Increase the highlight height if an image is open
                Point mousePosition = base.PointToClient(
                    new Point(Cursor.Position.X, Cursor.Position.Y));
                AdjustHighlightHeight(mousePosition.X, mousePosition.Y, true);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24076", ex);
            }
        }

        /// <summary>
        /// Decreases the default highlight height by two pixels.
        /// </summary>
        public void DecreaseHighlightHeight()
        {
            try
            {
                // Decrease the highlight height
                Point mousePosition = base.PointToClient(
                    new Point(Cursor.Position.X, Cursor.Position.Y));
                AdjustHighlightHeight(mousePosition.X, mousePosition.Y, false);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24077", ex);
            }
        }

        /// <summary>
        /// Activates the next tile command.
        /// </summary>
        public void SelectNextTile()
        {
            try
            {
                // Go to the next zone if an image is open and this is not the last tile
                if (base.Image != null && !this.IsLastTile)
                {
                    NextTile();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24078", ex);
            }
        }

        /// <summary>
        /// Activates the previous tile command.
        /// </summary>
        public void SelectPreviousTile()
        {
            try
            {
                // Go to the next zone if an image is open and this is not the first tile
                if (base.Image != null && !this.IsFirstTile)
                {
                    PreviousTile();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24079", ex);
            }
        }

        /// <summary>
        /// Activates the zoom in command.
        /// </summary>
        public void SelectZoomIn()
        {
            try
            {
                // Zoom in if an image is open
                if (base.Image != null)
                {
                    Zoom(true);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24080", ex);
            }
        }

        /// <summary>
        /// Activates the zoom out command.
        /// </summary>
        public void SelectZoomOut()
        {
            try
            {
                // Zoom out if an image is open
                if (base.Image != null)
                {
                    Zoom(false);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24081", ex);
            }
        }

        /// <summary>
        /// Activates the zoom previous command.
        /// </summary>
        public void SelectZoomPrevious()
        {
            try
            {
                // Zoom previous if an image is open
                if (base.Image != null && this.CanZoomPrevious)
                {
                    ZoomPrevious();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24082", ex);
            }
        }

        /// <summary>
        /// Activates the zoom next command.
        /// </summary>
        public void SelectZoomNext()
        {
            try
            {
                // Zoom next if an image is open
                if (base.Image != null && this.CanZoomNext)
                {
                    ZoomNext();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24083", ex);
            }
        }

        /// <summary>
        /// Activates the print command.
        /// </summary>
        public void SelectPrint()
        {
            try
            {
                // Print if an image is open
                if (base.Image != null)
                {
                    Print();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24084", ex);
            }
        }

        /// <summary>
        /// Adds a new printer to the disallowed printers list
        /// </summary>
        /// <param name="printerName">The name of the printer to add.</param>
        public void AddDisallowedPrinter(string printerName)
        {
            try
            {
                _disallowedPrinters.Add(printerName.ToUpperInvariant());
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24085", ex);
            }
        }

        /// <summary>
        /// Removes a printer from the disallowed printer list
        /// </summary>
        /// <param name="printerName">The name of the printer to remove.</param>
        public void RemoveDisallowedPrinter(string printerName)
        {
            try
            {
                _disallowedPrinters.Remove(printerName.ToUpperInvariant());
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24086", ex);
            }
        }

        #endregion Image Viewer Shortcut Key Methods

        #endregion Image Viewer Methods

        #region Image Viewer Static Methods

        /// <summary>
        /// Draws the specified region with the specified graphics using the specified options.
        /// </summary>
        /// <param name="region">The region to draw. Cannot be <see langword="null"/>.</param>
        /// <param name="graphics">The graphics with which to draw. Cannot be 
        /// <see langword="null"/>.</param>
        /// <param name="color">The color of the region to draw.</param>
        /// <param name="drawMode">The mix mode of the region to draw.</param>
        /// <permission cref="SecurityPermission">Demands permission for unmanaged code.
        /// </permission>
        [SecurityPermission(SecurityAction.LinkDemand,
            Flags = SecurityPermissionFlag.UnmanagedCode)]
        internal static void DrawRegion(Region region, Graphics graphics, Color color,
            NativeMethods.BinaryRasterOperations drawMode)
        {
            // Create handles for the the brush, region, and device context
            IntPtr brush = IntPtr.Zero;
            IntPtr regionHandle = IntPtr.Zero;
            IntPtr deviceContext = IntPtr.Zero;

            // In try..finally to ensure the handles are released
            try
            {
                // Create a colored brush to draw highlights
                brush = NativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(color));
                if (brush == IntPtr.Zero)
                {
                    throw new ExtractException("ELI21591", "Unable to create brush for region.",
                       new Win32Exception(Marshal.GetLastWin32Error()));
                }

                // Get the region handle
                regionHandle = region.GetHrgn(graphics);

                // Get the device context
                deviceContext = graphics.GetHdc();

                // Set the draw mode
                if (NativeMethods.SetROP2(deviceContext, drawMode) == 0)
                {
                    throw new ExtractException("ELI21113", "Unable to set draw mode for region.",
                        new Win32Exception(Marshal.GetLastWin32Error()));
                }

                // Draw the region
                if (!NativeMethods.FillRgn(deviceContext, regionHandle, brush))
                {
                    throw new ExtractException("ELI21961", "Unable to set draw mode for region.",
                        new Win32Exception(Marshal.GetLastWin32Error()));
                }
            }
            finally
            {
                // Release the handles
                if (brush != IntPtr.Zero)
                {
                    if (!NativeMethods.DeleteObject(brush))
                    {
                        ExtractException ee = new ExtractException("ELI21593",
                            "Unable to delete brush.",
                            new Win32Exception(Marshal.GetLastWin32Error()));
                        ee.Display();
                    }
                }
                if (regionHandle != IntPtr.Zero)
                {
                    region.ReleaseHrgn(regionHandle);
                }
                if (deviceContext != IntPtr.Zero)
                {
                    graphics.ReleaseHdc(deviceContext);
                }
            }
        }



        /// <summary>
        /// Initializes the static <see cref="Dictionary{T,T}"/> of <see cref="ImageViewerCursors"/>
        /// member of the <see cref="ImageViewer"/> class.
        /// </summary>
        /// <returns>A <see cref="Dictionary{T,T}"/> containing the
        /// <see cref="ImageViewerCursors"/> for all of the
        /// <see cref="CursorTool"/> objects</returns>
        static Dictionary<CursorTool, ImageViewerCursors> LoadCursorsForCursorTools()
        {
            try
            {
                // [DotNetRCAndUtils::297] LoadCursorsForCursorTools is called to initialize
                // the static member _cursorsForCursorTools prior to the constructor being called
                // for any particular instance.  Therefore make sure load the license file here
                // if in design mode.
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                Dictionary<CursorTool, ImageViewerCursors> cursorsForCursorTools =
                    new Dictionary<CursorTool, ImageViewerCursors>();

                // Iterate through each of the CursorTools
                foreach (CursorTool value in Enum.GetValues(typeof(CursorTool)))
                {
                    // Create a new ImageViewerCursors object
                    ImageViewerCursors cursors = new ImageViewerCursors();

                    // If the current tool is the None tool, set the tool
                    // cursor to the Cursors.Default and the active cursor to none.
                    if (value == CursorTool.None)
                    {
                        cursors.Tool = null;
                        cursors.Active = null;
                    }
                    else
                    {
                        // Get the name for this enum value
                        string name;
                        switch (value)
                        {
                            case CursorTool.AngularHighlight:
                                name = "Highlight";
                                break;

                            case CursorTool.AngularRedaction:
                                name = "Redaction";
                                break;

                            case CursorTool.SetHighlightHeight:
                                name = "SetHeight";
                                break;

                            case CursorTool.EditHighlightText:
                                name = "EditText";
                                break;

                            case CursorTool.DeleteLayerObjects:
                                name = "Delete";
                                break;

                            default:
                                name = Enum.GetName(typeof(CursorTool), value);
                                break;
                        }

                        // Load the normal cursor first.
                        cursors.Tool = ExtractCursors.GetCursor(typeof(ExtractCursors),
                            "Resources." + name + ".cur");

                        // Now load the active cursor.
                        cursors.Active = ExtractCursors.GetCursor(typeof(ExtractCursors),
                            "Resources.Active" + name + ".cur");
                    }

                    // Add the cursors for the current cursor tool to the collection
                    cursorsForCursorTools.Add(value, cursors);
                }

                return cursorsForCursorTools;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI21232", "Unable to initialize cursor objects!",
                    ex);
            }
        }

        /// <summary>
        /// Calculates the vertical scale factor of a 3x3 affine matrix.
        /// </summary>
        /// <remarks>The vertical scale factor is the magnitude of a vertical unit vector after 
        /// it has been transformed by the matrix.</remarks>
        /// <param name="matrix">3x3 affine matrix from which to calculate the vertical scale 
        /// factor.</param>
        /// <returns>The vertical scale factor of the 3x3 affine matrix.</returns>
        private static double GetScaleFactorY(Matrix matrix)
        {
            // NOTE: Formula derived by transforming a vertical unit 
            // vector using an arbitrary transformation matrix:
            //
            // [0 0 1][a b 0]   [ e   f  1]
            // [0 1 1][c d 0] = [c+e d+f 1]
            //        [e f 1]
            //
            // And then applying the distance formula:
            //
            // Sqrt(((c+e)-e)^2 + ((d+f)-f)^2) = Sqrt(c^2 + d^2)
            return Math.Sqrt(Math.Pow((double)matrix.Elements[2], 2) +
                Math.Pow((double)matrix.Elements[3], 2));
        }

        /// <summary>
        /// Will refresh the image viewer and if a parent form is found will
        /// call refresh on the parent as well.
        /// </summary>
        private void RefreshImageViewerAndParent()
        {
            // If _parentForm is null attempt to get it
            if (_parentForm == null)
            {
                _parentForm = base.TopLevelControl as Form;
            }

            // If there is a parent form just refresh the parent
            if (_parentForm != null)
            {
                _parentForm.Refresh();
            }
            else
            {
                base.Refresh();
            }
        }

        /// <summary>
        /// Gets the PDF initialization directory (the directory that contains
        /// the Lib, Resource, and Fonts directories).
        /// </summary>
        /// <returns>The PDF initialization directory.</returns>
        private static string GetPdfInitializationDirectory()
        {
#if DEBUG
            string pdfInitDir = Path.GetDirectoryName(Application.ExecutablePath)
                + @"\..\..\ReusableComponents\APIs\LeadTools_16\PDF";
#else
            string pdfInitDir = Path.GetDirectoryName(Application.ExecutablePath)
                + @"\..\..\CommonComponents\pdf";
#endif
            pdfInitDir = Path.GetFullPath(pdfInitDir);

            return pdfInitDir;
        }

        #endregion
    }
}
