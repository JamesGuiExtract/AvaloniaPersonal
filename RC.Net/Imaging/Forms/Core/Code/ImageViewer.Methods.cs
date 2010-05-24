using Extract.Drawing;
using Extract.Licensing;
using Extract.Utilities.Forms;
using Leadtools;
using Leadtools.Annotations;
using Leadtools.ImageProcessing;
using Leadtools.WinForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    sealed partial class ImageViewer
    {
        #region Methods

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

                // Check whether this is a form control
                Form form = control as Form;
                if (form != null)
                {
                    foreach (Form childForm in form.OwnedForms)
                    {
                        // Recursively establish connections with child forms
                        EstablishConnections(childForm);
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
                Refresh();

                // TODO: what should final state be if exceptions are thrown in different sections.

                // [IDSD:193] Convert to the full path name otherwise OCR will fail
                fileName = Path.GetFullPath(fileName);

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
                if (IsImageAvailable && !CloseImage(false))
                {
                    return;
                }

                // Raise the LoadingNewImage event
                OnLoadingNewImage(new LoadingNewImageEventArgs());

                // Refresh the image viewer before opening the new image
                RefreshImageViewerAndParent();

                using (new TemporaryWaitCursor())
                {
                    // Suspend the paint event until after all changes have been made
                    base.BeginUpdate();
                    try
                    {
                        // Reset the valid annotations flag
                        _validAnnotations = true;

                        // Initialize reader
                        _reader = Codecs.CreateReader(fileName);

                        // Get the page count
                        _pageCount = _reader.PageCount;

                        // Create the page data for this image
                        _imagePages = new List<ImagePageData>(_pageCount);
                        for (int i = 0; i < _pageCount; i++)
                        {
                            _imagePages.Add(new ImagePageData());
                        }

                        // Display the first page
                        _pageNumber = 1;
                        base.Image = GetPage(1);

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
                            CursorTool = CursorTool.ZoomWindow;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_reader != null)
                        {
                            _reader.Dispose();
                            _reader = null;
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
                OnPageChanged(new PageChangedEventArgs(_pageNumber));

                // Update the zoom history and raise the zoom changed event
                UpdateZoom(true, true);

                // Restore the cursor for the active cursor tool.
                Cursor = _toolCursor ?? Cursors.Default;
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
                        "Unable to remove file from MRU list.", ex);
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
                if (IsImageAvailable)
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
        bool CloseImage(bool raiseImageFileChangedEvent)
        {
            try
            {
                ExtractException.Assert("ELI22337", "An image must be open.",
                    IsImageAvailable);

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

                    // Set Image to null
                    base.Image = null;

                    // Dispose of the image reader
                    if (_reader != null)
                    {
                        _reader.Dispose();
                        _reader = null;
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

                    // If an image isn't loaded, the cursor for the active cursor tool shouldn't be.
                    Cursor = Cursors.Default;

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
            int originalPage = _pageNumber;
            Matrix transform = null;
            IntPtr hdc = IntPtr.Zero;
            Graphics graphics = null;
            ImageWriter writer = Codecs.CreateWriter(fileName, format);
            try
            {
                // Ensure not saving to the currently open image
                ExtractException.Assert("ELI23374", "Cannot save to the currently open image.",
                    !fileName.Equals(_imageFile, StringComparison.OrdinalIgnoreCase));

                // Set the wait cursor
                waitCursor = new TemporaryWaitCursor();

                // Check if annotations will be stored as tiff tags
                AnnCodecs annCodecs = null;
                if (_displayAnnotations && ImageMethods.IsTiff(format))
                {
                    // Prepare to store tiff tags
                    annCodecs = new AnnCodecs();
                }

                // Iterate through each page of the image
                ColorResolutionCommand command = GetSixteenBppConverter(); // Used to change bpp of image
                for (int i = 1; i <= _pageCount; i++)
                {
                    // TODO: This could be reworked to go directly to disk without changing page number
                    // Go to the specified page
                    SetPageNumber(i, false, false);

                    RasterImage page;
                    hdc = ImageMethods.GetLeadDCWithRetries(base.Image, _SAVE_RETRY_COUNT,
                        command, i, out page);

                    // Create a graphics object from the handle
                    graphics = Graphics.FromHdc(hdc);

                    // Calculate the rotated transform for this page
                    transform = GetRotatedMatrix(graphics.Transform, false);

                    // Render the annotations
                    RasterTagMetadata tag = null;
                    if (_annotations != null)
                    {
                        // Rotate the annotations
                        _annotations.Transform = graphics.Transform.Clone();
                        RotateAnnotations();

                        // Check whether the output image is a tiff
                        if (annCodecs != null)
                        {
                            // Save annotations in a tiff tag
                            tag = annCodecs.SaveToTag(_annotations, AnnCodecsTagFormat.Tiff);
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
                    writer.AppendImage(page);
                    if (tag != null)
                    {
                        writer.WriteTagOnPage(tag, i);
                    }
                }
                transform = null;
                hdc = IntPtr.Zero;
                graphics = null;

                // Save the file
                writer.Commit(true);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22902", ex);
            }
            finally
            {
                // Restore the original page number
                SetPageNumber(originalPage, false, false);

                // Dispose of the resources
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
                if (writer != null)
                {
                    writer.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets a command to convert an image to 16 bpp.
        /// </summary>
        /// <returns>A command to convert an image to 16 bpp.</returns>
        static ColorResolutionCommand GetSixteenBppConverter()
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
            int orientation = _imagePages[_pageNumber - 1].Orientation;
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
            using (Font font = FontMethods.GetFontThatFits(_watermark, graphics, fontSize))
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
                Rectangle imageArea = PhysicalViewRectangle;
                int deltaX = tileArea.Left - imageArea.Left;

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
                        if (_pageNumber == 1)
                        {
                            throw new ExtractException("ELI21855",
                                "Cannot move before first tile.");
                        }

                        // Go to the previous page, preserving the scale factor
                        double scaleFactor = ScaleFactor;
                        SetPageNumber(_pageNumber - 1, false, true);
                        ScaleFactor = scaleFactor;

                        // Move the tile area to the bottom right of new page
                        imageArea = PhysicalViewRectangle;
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
                Rectangle imageArea = PhysicalViewRectangle;
                int deltaX = imageArea.Right - tileArea.Right;

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
                        if (_pageNumber == _pageCount)
                        {
                            throw new ExtractException("ELI21846",
                                "Cannot advance past last tile.");
                        }

                        // Go to the next page, preserving the scale factor
                        double scaleFactor = ScaleFactor;
                        SetPageNumber(_pageNumber + 1, false, true);
                        ScaleFactor = scaleFactor;

                        // Move the tile area to the top-left of the new page
                        tileArea.Offset(PhysicalViewRectangle.Location);
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
            try
            {
                // Ensure it is possible to go to the next layer object.
                if (IsImageAvailable && CanGoToNextLayerObject)
                {
                    // Go to the next layer object.
                    GoToNextVisibleLayerObject(true);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26539", ex);
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
            try
            {
                // Get the next visible layer object
                LayerObject nextLayerObject = GetNextVisibleLayerObject();

                if (nextLayerObject != null)
                {
                    CenterOnLayerObjects(nextLayerObject);

                    if (selectObject)
                    {
                        // Clear the current selection
                        _layerObjects.Selection.Clear();

                        // Make this object the selection object
                        nextLayerObject.Selected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26540", ex);
            }
        }

        /// <summary>
        /// Activates the previous layer object command (with object selection enabled)
        /// </summary>
        public void GoToPreviousLayerObject()
        {
            try
            {
                // Ensure it is possible to go to the Previous layer object.
                if (IsImageAvailable && CanGoToPreviousLayerObject)
                {
                    // Go to the previous layer object.
                    GoToPreviousVisibleLayerObject(true);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26541", ex);
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
            try
            {
                // Get the next visible layer object
                LayerObject previousLayerObject = GetPreviousVisibleLayerObject();

                if (previousLayerObject != null)
                {
                    CenterOnLayerObjects(previousLayerObject);

                    if (selectObject)
                    {
                        // Clear the current selection
                        _layerObjects.Selection.Clear();

                        // Make this object the selection object
                        previousLayerObject.Selected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26542", ex);
            }
        }

        /// <summary>
        /// Centers the view on the specified layer objects.
        /// </summary>
        /// <param name="layerObjects">The layer objects on which to center. Must all be on the 
        /// same page.</param>
        public void CenterOnLayerObjects(params LayerObject[] layerObjects)
        {
            try
            {
                // Is the image viewer in fit to page mode?
                bool isFitToPage = _fitMode == FitMode.FitToPage;

                // Gets the page number of the first layer object
                int pageNumber = layerObjects[0].PageNumber;

                // Set the image viewer to the proper page for these objects
                if (PageNumber != pageNumber)
                {
                    SetPageNumber(pageNumber, isFitToPage, true);
                }

                // Set the appropriate zoom if necessary
                if (!isFitToPage)
                {
                    // Get the unified bounds in image coordinates
                    Rectangle bounds = LayerObject.GetCombinedBounds(layerObjects);

                    // Get the center point in image coordinates
                    Point center =
                        new Point(bounds.Left + bounds.Width/2, bounds.Top + bounds.Height/2);

                    // Center the view on the layer objects' center point
                    CenterAtPoint(center, false, false);

                    // Get the current view rectangle in image coordinates
                    // Do not pad the viewing rectangle [DNRCAU #282]
                    Rectangle visibleArea = GetTransformedRectangle(GetVisibleImageArea(), true);

                    // Check if the objects are in view
                    if (!visibleArea.Contains(bounds))
                    {
                        // An object is not fully in view, adjust the zoom so all
                        // objects are in view (also change fit mode if necessary)
                        Rectangle zoomRectangle = PadViewingRectangle(bounds);
                        zoomRectangle = GetTransformedRectangle(zoomRectangle, false);
                        ZoomToRectangle(zoomRectangle, false, false, true);
                    }

                    // Update the zoom history and raise the zoom changed event
                    UpdateZoom(true, true);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22431", ex);
            }
        }

        /// <summary>
        /// Centers the view at the specified point in logical (image) coordinates.
        /// </summary>
        /// <param name="pt">The point at which to center the view in logical (image) 
        /// coordinates.</param>
        public override void CenterAtPoint(Point pt)
        {
            CenterAtPoint(pt, true, true);
        }

        /// <summary>
        /// Centers the view at the specified point in logical (image) coordinates.
        /// </summary>
        /// <param name="pt">The point at which to center the view in logical (image) 
        /// coordinates.</param>
        /// <param name="updateZoomHistory"><see langword="true"/> if the new zoom setting should 
        /// be added to the history queue; <see langword="false"/> if it should not.</param>
        /// <param name="raiseZoomChanged"><see langword="true"/> if the <see cref="ZoomChanged"/> 
        /// event should be raised; <see langword="false"/> if it should not be raised.</param>
        void CenterAtPoint(Point pt, bool updateZoomHistory, bool raiseZoomChanged)
        {
            // Normalize the point if it is off the image
            int x = pt.X;
            int y = pt.Y;
            if (x < 0)
            {
                x = 0;
            }
            else if (x > ImageWidth)
            {
                x = ImageWidth - 1;
            }
            if (y < 0)
            {
                y = 0;
            }
            else if (y > ImageHeight)
            {
                y = ImageHeight - 1;
            }

            // Convert the point to client coordinates
            Point[] client = new Point[] { new Point(x,y) };
            _transform.TransformPoints(client);

            // Center at the point
            base.CenterAtPoint(client[0]);

            // Update the zoom as necessary
            UpdateZoom(updateZoomHistory, raiseZoomChanged);
        }

        /// <summary>
        /// Will return a new rectangle that has been enlarged (if possible) to
        /// fit the link arrows in view.
        /// </summary>
        /// <param name="viewRectangle">The <see cref="Rectangle"/> to enlarge.</param>
        /// <returns>A padded version of the specified rectangle that has had space
        /// added to account for link arrows.</returns>
        Rectangle PadViewingRectangle(Rectangle viewRectangle)
        {
            return PadViewingRectangle(viewRectangle, _ZOOM_TO_OBJECT_WIDTH_PADDING,
                _ZOOM_TO_OBJECT_HEIGHT_PADDING, false);
        }

        /// <summary>
        /// Will return a new rectangle that has been inflated in each direction by
        /// the specified amount while keeping the rectangle on-page.
        /// </summary>
        /// <param name="viewRectangle">The <see cref="Rectangle"/> to enlarge.</param>
        /// <param name="horizontalPadding">The amount to expand the left and right sides.</param>
        /// <param name="verticalPadding">The amount to expand the top and bottom sides.</param>
        /// <param name="transferPaddingIfOffPage">If <see langword="true"/>, if applying the
        /// padding to any side would cause the rectangle to extend offpage, the padding that
        /// cannot be applied will be applied to the opposite side instead. If
        /// <see langword="false"/>, the padding will be discarded resulting in a truncated
        /// rectangle.</param>
        /// <returns>A padded version of the specified rectangle.</returns>
        public Rectangle PadViewingRectangle(Rectangle viewRectangle, int horizontalPadding, 
            int verticalPadding, bool transferPaddingIfOffPage)
        {
            try
            {
                // Apply the specified padding to all sides.
                int left = viewRectangle.Left - horizontalPadding;
                int top = viewRectangle.Top - verticalPadding;
                int right = viewRectangle.Right + horizontalPadding;
                int bottom = viewRectangle.Bottom + verticalPadding;

                // If transferring padding, look for offpage coordinates.
                if (transferPaddingIfOffPage)
                {
                    // If offpage left, transfer the excess padding to the right side.
                    if (left < 0)
                    {
                        right -= left;
                        left = 0;
                    }
                    // If offpage right, transfer the excess padding to the left side.
                    else if (right >= ImageWidth)
                    {
                        left -= (right - ImageWidth);
                        right = ImageWidth - 1;
                    }

                    // If offpage top, transfer the excess padding to the bottom side.
                    if (top < 0)
                    {
                        bottom -= top;
                        top = 0;
                    }
                    // If offpage bottom, transfer the excess padding to the top side.
                    else if (bottom >= ImageHeight)
                    {
                        top -= (bottom - ImageHeight);
                        bottom = ImageHeight - 1;
                    }
                }

                // Create the padded rectangle then lop off any portion that extends offpage.
                Rectangle paddedRectangle = Rectangle.FromLTRB(left, top, right, bottom);
                paddedRectangle.Intersect(new Rectangle(0, 0, ImageWidth, ImageHeight));

                return paddedRectangle;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27035", ex);
            }
        }

        /// <summary>
        /// Gets the next visible <see cref="LayerObject"/> that can be navigated to.
        /// </summary>
        /// <returns>The next visible <see cref="LayerObject"/> that can be navigated
        /// to if there is one, otherwise <see langword="null"/>.</returns>
        LayerObject GetNextVisibleLayerObject()
        {
            // Default return value to null
            LayerObject returnObject = null;

            // Get the next object (if there is one)
            int index = GetNextVisibleLayerObjectIndex();
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
        LayerObject GetPreviousVisibleLayerObject()
        {
            // Default return value to null
            LayerObject returnObject = null;

            // Get the previous object (if there is one)
            int index = GetPreviousVisibleLayerObjectIndex();
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
        int GetNextVisibleLayerObjectIndex()
        {
            // Default the return value to -1
            int nextIndex = -1;

            // Get the sorted collection of visible layer objects
            ReadOnlyCollection<LayerObject> sortedCollection =
                _layerObjects.GetSortedVisibleCollection(true);

            // Get the current view rectangle
            Rectangle viewRectangle = GetTransformedRectangle(
                GetVisibleImageArea(), true);

            // Check for a selection first (There must be at least one selected object
            // in the current view, otherwise search as if there is no selection)
            if (_layerObjects.Selection.IsAnyObjectContained(viewRectangle, PageNumber))
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
                    _layerObjects.GetIndexOfSortedLayerObjectsInRectangle(PageNumber,
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
                        ImageWidth - viewRectangle.Right, viewRectangle.Height);
                    Collection<int> index = _layerObjects.GetIndexOfSortedLayerObjectsInRectangle(
                        PageNumber, rightView, false, false);

                    // If haven't found an object yet, search below the view area
                    if (index.Count == 0)
                    {
                        Rectangle bottomView = new Rectangle(0, viewRectangle.Bottom,
                            ImageWidth, ImageHeight - viewRectangle.Bottom);
                        index = _layerObjects.GetIndexOfSortedLayerObjectsInRectangle(
                            PageNumber, bottomView, false, false);
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
                            if (sortedCollection[i].PageNumber > PageNumber)
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
        int GetPreviousVisibleLayerObjectIndex()
        {
            // Default the return value to -1
            int previousIndex = -1;

            // Get the sorted collection of visible layer objects
            ReadOnlyCollection<LayerObject> sortedCollection =
                _layerObjects.GetSortedVisibleCollection(true);

            // Get the current view rectangle
            Rectangle viewRectangle = GetTransformedRectangle(
                GetVisibleImageArea(), true);

            // Check for a selection first (There must be at least one selected object
            // in the current view, otherwise search as if there is no selection)
            if (_layerObjects.Selection.IsAnyObjectContained(viewRectangle, PageNumber))
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
                    _layerObjects.GetIndexOfSortedLayerObjectsInRectangle(PageNumber,
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
                        PageNumber, leftView, true, false);

                    // If haven't found an object yet, search above the view area
                    if (index.Count == 0)
                    {
                        Rectangle topView = new Rectangle(0, 0,
                            ImageWidth, viewRectangle.Top);
                        index = _layerObjects.GetIndexOfSortedLayerObjectsInRectangle(
                            PageNumber, topView, true, false);
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
                            if (sortedCollection[i].PageNumber < PageNumber)
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
        void Zoom(bool zoomIn)
        {
            // Ensure an image is open.
            ExtractException.Assert("ELI21434", "No image is open.", base.Image != null);

            // Ensure the fit mode is set to none
            if (_fitMode != FitMode.None)
            {
                // Set the fit mode without updating the zoom
                SetFitMode(FitMode.None, false, false, true);
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
                    CanZoomPrevious);

                // Get the previous zoom history entry
                ZoomInfo zoomInfo = _imagePages[_pageNumber - 1].ZoomPrevious();

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
                    CanZoomNext);

                // Get the previous zoom history entry
                ZoomInfo zoomInfo = _imagePages[_pageNumber - 1].ZoomNext();

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
                ExtractException.Assert("ELI21108", "No image is open.", base.Image != null);

                // Ensure the angle is valid
                ExtractException.Assert("ELI21107", "Rotation angle must be a multiple of 90.",
                    angle % 90 == 0);

                // Calulate the new orientation
                ImagePageData page = _imagePages[_pageNumber - 1];
                page.RotateOrientation(angle);

                // Get the center of the visible image in logical (image) coordinates
                Point center = GetVisibleImageCenter();

                try
                {
                    // Check if the view perspectives are licensed (ID Annotation feature)
                    RotateImageByDegrees(base.Image, angle);
                }
                catch
                {
                    // Reverse the orientation change
                    page.RotateOrientation(-angle);

                    throw;
                }

                // Center at the same point as prior to rotation
                CenterAtPoint(center, false, false);

                // Raise the OrientationChanged event
                OnOrientationChanged(
                    new OrientationChangedEventArgs(page.Orientation));
            }
            catch (Exception e)
            {
                ExtractException ee = new ExtractException("ELI21123",
                    "Cannot rotate image.", e);
                ee.AddDebugData("Rotation", angle, false);
                ee.AddDebugData("Page number", _pageNumber, false);
                throw ee;
            }
        }

        /// <summary>
        /// Rotates the specified image by the specified number of degrees.
        /// </summary>
        /// <param name="image">The image to rotate.</param>
        /// <param name="angle">The number of degrees to rotate the image.</param>
        static void RotateImageByDegrees(RasterImage image, int angle)
        {
            if (angle != 0)
            {
                bool viewPerspectiveLicensed = !RasterSupport.IsLocked(RasterSupportType.Document);

                // Rotate the image.
                // It is faster to rotate using the view perspective, so rotate that 
                // way if it is licensed. Otherwise, use the slower rotate command.
                if (viewPerspectiveLicensed)
                {
                    // Fast rotation
                    image.RotateViewPerspective(angle % 360);
                }
                else
                {
                    // Not as fast rotation
                    RotateCommand rotate = new RotateCommand((angle % 360) * 100,
                        RotateCommandFlags.Resize, RasterColor.FromGdiPlusColor(Color.White));
                    rotate.Run(image);
                }
            }
        }

        /// <summary>
        /// Prints the area of the image that is currently in view.
        /// </summary>
        public void PrintView()
        {
            Print(true, false);
        }

        /// <summary>
        /// Prints the currently open image.
        /// <para><b>Requirements</b></para>
        /// <para>An image must be open.</para>
        /// </summary>
        /// <exception cref="ExtractException">No image is open.</exception>
        public void Print()
        {
            Print(false, true);
        }

        /// <summary>
        /// Prints the currently open image.
        /// <para><b>Requirements</b></para>
        /// <para>An image must be open.</para>
        /// </summary>
        /// <param name="raiseDisplayingPrint">If <see langword="true"/> then
        /// will raise the <see cref="DisplayingPrintDialog"/> event.</param>
        /// <exception cref="ExtractException">No image is open.</exception>
        void Print(bool raiseDisplayingPrint)
        {
            Print(false, raiseDisplayingPrint);
        }
        
        /// <summary>
        /// Prints the currently open image.
        /// </summary>
        /// <param name="printView">Print only the current view.</param>
        /// <param name="raiseDisplayingPrint">If <see langword="true"/> will raise the 
        /// <see cref="DisplayingPrintDialog"/>; <see langword="false"/> will not raise the event.
        /// </param>
        void Print(bool printView, bool raiseDisplayingPrint)

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
                UpdatePrintDialog(printView);

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
                        RefreshImageViewerAndParent();

                        if (printView)
                        {
                            _printDialog.PrinterSettings.PrintRange = PrintRange.Selection;
                        }

                        // Print the document
                        _printDialog.Document.Print();

                        // Store the last printer
                        LastPrinter = _printDialog.PrinterSettings.PrinterName;

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
                        applicationName = string.IsNullOrEmpty(applicationName) 
                            ? "Application" : applicationName;
                        MessageBox.Show(applicationName + " has disallowed Printing to "
                                        + _printDialog.PrinterSettings.PrinterName.ToUpperInvariant() + "."
                                        + " Please select a different printer." + Environment.NewLine
                                        + Environment.NewLine
                                        + Environment.NewLine + "The following printers have been disallowed:"
                                        + Environment.NewLine + "-------------------------------------"
                                        + Environment.NewLine + sb, " Printer",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);

                        // Refresh the form
                        RefreshImageViewerAndParent();

                        // Update the print dialog
                        UpdatePrintDialog(printView);
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

            RefreshImageViewerAndParent();
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
                    _printPreview.Text = "Preview - " + ImageFile;
                    _printPreview.UseAntiAlias = true;
                    
                    // Display a warning message and quit if no printers are valid
                    if (string.IsNullOrEmpty(_printPreview.Document.PrinterSettings.PrinterName))
                    {
                        ShowNoValidPrintersWarning();
                        return;
                    }

                    // Set the start page to the current page
                    _printPreview.PrintPreviewControl.StartPage = _pageNumber - 1;

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
                    RefreshImageViewerAndParent();

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
                RefreshImageViewerAndParent();

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
        PrintDocument GetPrintDocument()
        {
            // Create the print document if not already created
            if (_printDocument == null)
            {
                _printDocument = new PrintDocument();
                _printDocument.PrintPage += HandlePrintPage;
                _printDocument.BeginPrint += HandleBeginPrint;
                _printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                _printDocument.DefaultPageSettings.Color = true;
                _printDocument.PrinterSettings.DefaultPageSettings.Landscape = false;
            }

            // Setup the printer settings
            PrinterSettings settings = _printDocument.PrinterSettings;
            settings.MinimumPage = 1;
            settings.FromPage = 1;
            settings.MaximumPage = _pageCount;
            settings.ToPage = _pageCount;

            // Ensure selection of a valid printer
            if (_disallowedPrinters.Contains(settings.PrinterName.ToUpperInvariant()))
            {
                // Current is invalid check the default printer
                PrinterSettings temp = new PrinterSettings();
                if (settings.IsDefaultPrinter ||
                    _disallowedPrinters.Contains(temp.PrinterName.ToUpperInvariant()))
                {
                    // Default is invalid, try last printer
                    string lastPrinter = LastPrinter;
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
        /// <param name="printView"><see langword="true"/> if only the current view should be 
        /// printed; <see langword="false"/> otherwise.</param>
        void UpdatePrintDialog(bool printView)
        {
            // Check if the print dialog has been created yet
            if (_printDialog == null)
            {
                // Create the print dialog
                _printDialog = new PrintDialog();
                _printDialog.UseEXDialog = true;
            }

            // Enable page options unless only printing the current view
            _printDialog.AllowCurrentPage = !printView;
            _printDialog.AllowSomePages = !printView;

            // Setup the print document
            _printDialog.Document = GetPrintDocument();
        }

        /// <summary>
        /// Prints the page corresponding to <see cref="_printPage"/>.
        /// </summary>
        /// <param name="e">The event data associated with a <see cref="PrintDocument.PrintPage"/> 
        /// event.</param>
        void PrintPage(PrintPageEventArgs e)
        {
            // Get the margin bounds
            Rectangle marginBounds = e.MarginBounds;

            Rectangle source;
            if (e.PageSettings.PrinterSettings.PrintRange == PrintRange.Selection)
            {
                Rectangle imageArea = GetVisibleImageArea();

                // Use the base transform because it does not take rotation into account.
                // Rotation will be applied later
                source = GetTransformedRectangle(imageArea, true, base.Transform);
            }
            else
            {
                source = SourceRectangle;
            }

            // Calculate the scale so the image fits exactly within the bounds of the page
            float scale = (float)Math.Min(
                                     marginBounds.Width / (double)source.Width,
                                     marginBounds.Height / (double)source.Height);

            // Calculate the horizontal and vertical padding
            PointF padding = new PointF(
                (marginBounds.Width - source.Width * scale) / 2.0F,
                (marginBounds.Height - source.Height * scale) / 2.0F);

            // Calculate the destination rectangle
            Rectangle destination = new Rectangle(
                (int)(marginBounds.Left + padding.X), (int)(marginBounds.Top + padding.Y),
                (int)(marginBounds.Width - padding.X * 2), (int)(marginBounds.Height - padding.Y * 2));

            // Clip the region based on printable area [IDSD #318]
            RectangleF clip = RectangleF.Intersect(e.PageSettings.PrintableArea, destination);
            e.Graphics.Clip = new Region(clip);

            Matrix unrotatedToPrinter = null;
            Matrix imageToPrinter = null;
            try
            {
                // Create matrices to map logical (image) coordinates to printer coordinates
                unrotatedToPrinter = GetPrintMatrix(source.Location, scale, destination.Location);
                imageToPrinter = GetRotatedMatrix(unrotatedToPrinter, false);

                // Draw the image
                using (Image img = base.Image.ConvertToGdiPlusImage())
                {
                    e.Graphics.DrawImage(img, destination, source, GraphicsUnit.Pixel);
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
                    if (layerObject.PageNumber == _pageNumber && layerObject.CanRender)
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
        /// Creates a 3x3 affine matrix that maps the unrotated image using the specified offset,
        /// scale, and padding.
        /// </summary>
        /// <param name="offset">The offset applied to the original image in logical (image) 
        /// coordinates.</param>
        /// <param name="scale">The scale to apply to the original image in logical (image) 
        /// coordinates.</param>
        /// <param name="padding">The padding to apply to the original image in logical (image) 
        /// coordinates.</param>
        /// <returns>A 3x3 affine matrix that maps the unrotated image to a rotated destination 
        /// rectangle with the specified scale factor and margin padding.
        /// </returns>
        static Matrix GetPrintMatrix(PointF offset, float scale, PointF padding)
        {
            // Create the matrix
            Matrix printMatrix = new Matrix();

            // Translate the origin by the specified offset
            printMatrix.Translate(-offset.X, -offset.Y);

            // Scale the matrix
            printMatrix.Scale(scale, scale, MatrixOrder.Append);

            // Translate the matrix so the image is centered with the specified padding
            printMatrix.Translate(padding.X, padding.Y, MatrixOrder.Append);

            // Return the rotated the matrix
            return printMatrix;
        }

        /// <summary>
        /// Loads annotations from the currently open <see cref="_imageFile"/>.
        /// </summary>
        /// <remarks>If the <see cref="_displayAnnotations"/> is <see langword="false"/>. 
        /// Annotations will be cleared and no annotations will be displayed.</remarks>
        void UpdateAnnotations()
        {
            // Dispose of the annotations if they exist
            if (_annotations != null)
            {
                _annotations.Dispose();
                _annotations = null;
            }

            // If annotations shouldn't be displayed or there is no open image file
            // we are done.
            if (!_displayAnnotations || _reader == null || !_validAnnotations)
            {
                return;
            }

            try
            {
                // Load annotations from file
                RasterTagMetadata tag = _reader.ReadTagOnPage(_pageNumber);
                if (tag != null)
                {
                    // Initialize a new annotations container and associate it with the image viewer.
                    _annotations = new AnnContainer();

                    // Use original image bounds for rotated images [DotNetRCAndUtils #54]
                    _annotations.Bounds =
                        new AnnRectangle(0, 0, base.Image.Width, base.Image.Height, AnnUnit.Pixel);
                    _annotations.Visible = true;

                    // Ensure the unit converter is the same DPI as the image viewer itself.
                    using (Graphics g = CreateGraphics())
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
        void UpdateZoom(bool updateZoomHistory, bool raiseZoomChanged)
        {
            // Get the current zoom setting
            ZoomInfo zoomInfo = GetZoomInfo();

            // [DataEntry:311] Impose limits on the amount one can zoom in or out to prevent errors
            // rendering the image.
            if (zoomInfo.FitMode == FitMode.None &&
                zoomInfo.ScaleFactor > _MAX_ZOOM_IN_SCALE_FACTOR)
            {
                zoomInfo.ScaleFactor = _MAX_ZOOM_IN_SCALE_FACTOR;
                SetZoomInfo(zoomInfo, false);
            }
            else if (zoomInfo.FitMode == FitMode.None &&
                     zoomInfo.ScaleFactor < _MAX_ZOOM_OUT_SCALE_FACTOR)
            {
                zoomInfo.ScaleFactor = _MAX_ZOOM_OUT_SCALE_FACTOR;
                SetZoomInfo(zoomInfo, false);
            }

            // If nothing should be updated, we are done
            if (!updateZoomHistory && !raiseZoomChanged)
            {
                return;
            }

            // Add the entry to the zoom history for the current page if necessary
            if (updateZoomHistory)
            {
                _imagePages[_pageNumber - 1].ZoomInfo = zoomInfo;
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
        void ZoomToRectangle(Rectangle rc, bool updateZoomHistory, bool raiseZoomChanged, 
            bool updateFitMode)
        {
            try
            {
                // Ensure an image is open
                ExtractException.Assert("ELI21455", "No image is open.", base.Image != null);

                // Change the fit mode if it is not fit mode none
                if (updateFitMode && _fitMode != FitMode.None)
                {
                    // Set the fit mode to none without updating the zoom
                    SetFitMode(FitMode.None, false, false, true);
                }

                // If in fit to page mode, the whole page is already in view. No need to zoom.
                if (_fitMode != FitMode.FitToPage)
                {
                    // Preserve fit to width mode if necessary
                    bool preserveFitToWidth = _fitMode == FitMode.FitToWidth;

                    // Zoom to the specified rectangle
                    base.ZoomToRectangle(rc);

                    // Restore the fit to width mode
                    if (preserveFitToWidth)
                    {
                        SetFitMode(FitMode.FitToWidth, false, false, false);
                    }
                }

                // Update the zoom if necessary
                UpdateZoom(updateZoomHistory, raiseZoomChanged);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27034", ex);
            }
        }

        /// <summary>
        /// Begins an interactive tracking action (e.g. drag and drop) starting at the specified 
        /// mouse position.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        void StartTracking(int mouseX, int mouseY)
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

                        // Find the highlight that was clicked on (if any)
                        // and present the edit text dialog to the user
                        LayerObject layerObject = GetLayerObjectAtPoint<Highlight>(_layerObjects,
                            mouseX, mouseY, false);
                        if (layerObject != null)
                        {
                            Highlight highlight = layerObject as Highlight;
                            // Prompt the user for the new highlight text
                            using (EditHighlightTextForm dialog =
                                new EditHighlightTextForm(highlight.Text))
                            {
                                if (dialog.ShowDialog() == DialogResult.OK)
                                {
                                    // Store the new highlight text
                                    highlight.Text = dialog.HighlightText;
                                }
                            }
                        }
                    }
                    break;

                case CursorTool.SelectLayerObject:
                {
                    // Get modifier keys immediately
                    Keys modifiers = ModifierKeys;

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
                        if (layerObject.PageNumber != _pageNumber)
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

                    LayerObject clickedObject = GetLayerObjectAtPoint<LayerObject>(_layerObjects,
                        mouseX, mouseY, true);
                    if (clickedObject != null)
                    {
                        // Select this layer object if it is not already
                        if (!_layerObjects.Selection.Contains(clickedObject))
                        {
                            // Deselect previous layer object unless modifier key is pressed
                            if (modifiers != Keys.Control && modifiers != Keys.Shift)
                            {
                                _layerObjects.Selection.Clear();
                            }

                            clickedObject.Selected = true;

                            // Switch the cursor to the move cursor
                            Cursor = Cursors.SizeAll;
                        }
                        else if (modifiers == Keys.Control)
                        {
                            // The control key is pressed and a selected 
                            // layer object was clicked. Remove the layer object.
                            _layerObjects.Selection.Remove(clickedObject);

                            // Invalidate the image viewer to remove any previous grip 
                            // handles and to redraw these grip handles
                            Invalidate();
                            return;
                        }

                        // Check if any layer object was clicked [DotNetRCAndUtils #159]
                        // Start a tracking event for all selected layer objects on the active page
                        foreach (LayerObject layerObject in _layerObjects.Selection)
                        {
                            if (layerObject.PageNumber == _pageNumber)
                            {
                                layerObject.StartTrackingSelection(mouseX, mouseY);
                            }
                        }
                    }

                    // Only start a tracking event if a layer object was clicked or banded
                    // selection is enabled.
                    if (clickedObject != null || _allowBandedSelection)
                    {
                        // Start a click and drag event
                        _trackingData = new TrackingData(this, mouseX, mouseY,
                            GetVisibleImageArea());
                    }

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
        /// Gets a layer object at the specified point.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="LayerObject"/> to perform the
        /// hit test on, to check all types just use <see cref="LayerObject"/> to
        /// limit to just <see cref="Highlight"/> objects pass that as the type parameter.</typeparam>
        /// <param name="layerObjects">A collection of layer objects from which to select the 
        /// result.</param>
        /// <param name="x">The physical (client) x coordinate of the point.</param>
        /// <param name="y">The physical (client) y coordinate of the point.</param>
        /// <param name="onlySelectableObjects">If <see langword="true"/> will only
        /// check for objects that are selectable, if <see langword="false"/> all
        /// objects will be considered candidates for selection.</param>
        /// <returns>A layer object from <paramref name="layerObjects"/>at the specified point. 
        /// If more than one layer object is at the point, the one with the lowest average distance 
        /// from its corners to the point is returned. <see langword="null"/> if no layer object 
        /// is at the point.</returns>
        internal LayerObject GetLayerObjectAtPoint<T>(IEnumerable<LayerObject> layerObjects, int x, int y,
            bool onlySelectableObjects) where T : LayerObject
        {
            // Get the mouse position in image coordinates
            Point[] mouse = new Point[] { new Point(x, y) };
            using (Matrix clientToImage = _transform.Clone())
            {
                clientToImage.Invert();
                clientToImage.TransformPoints(mouse);
            }

            Point mousePoint = mouse[0];

            // Build the list of possible layer objects
            List<LayerObject> possibleSelection = new List<LayerObject>();
            foreach (LayerObject layerObject in layerObjects)
            {
                // Only check if the layer object is visible and selectable (if selectable
                // is being checked)
                if ((!onlySelectableObjects || layerObject.Selectable) && layerObject.Visible)
                {
                    // Check that the object is of the appropriate type

                    if (layerObject is T && layerObject.HitTest(mousePoint))
                    {
                        possibleSelection.Add(layerObject);
                    }
                }
            }

            LayerObject clickedObject = null;
            if (possibleSelection.Count > 0)
            {
                // Find the best layer object [DNRCAU #352]
                // Best layer object is defined as the one with the lowest average distance
                // to the corners of the object
                double shortestDistance = double.MaxValue;
                foreach (LayerObject layerObject in possibleSelection)
                {
                    double distance = layerObject.AverageDistanceToCorners(mousePoint);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        clickedObject = layerObject;
                    }
                }
            }

            return clickedObject;
        }

        /// <summary>
        /// Goes to the previous or next link of the specified layer object.
        /// </summary>
        /// <param name="previous"><see langword="true"/> to go to the previous link; 
        /// <see langword="false"/> to go the next link.</param>
        /// <param name="layerObject">The layer object that holds the link to go to.</param>
        void GoToLink(bool previous, LayerObject layerObject)
        {
            // Get the current zoom info
            ZoomInfo zoomInfo = GetZoomInfo();

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
                    ScaleFactor = zoomInfo.ScaleFactor;
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
                SetZoomInfo(zoomInfo, true);
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
        Point GetRelativeLinkPoint(bool left, LayerObject layerObject)
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
        void UpdateTracking(int mouseX, int mouseY)
        {
            // Update the tracking dependent upon the active cursor tool
            switch (_cursorTool)
            {
                case CursorTool.AngularHighlight:
                case CursorTool.AngularRedaction:

                    UpdateDrawingHighlight(mouseX, mouseY, true);
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
                    ControlPaint.DrawReversibleFrame(RectangleToScreen(rectangle),
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
                    ControlPaint.DrawReversibleFrame(RectangleToScreen(rectangle),
                        Color.Black, FrameStyle.Thick);
                    break;

                case CursorTool.RectangularHighlight:
                case CursorTool.RectangularRedaction:

                    UpdateDrawingHighlight(mouseX, mouseY, false);
                    break;

                case CursorTool.SelectLayerObject:

                    if (Cursor == Cursors.Default)
                    {
                        // This is a click and drag multiple layer objects event

                        // Erase the previous frame if it exists
                        ControlPaint.DrawReversibleFrame(RectangleToScreen(_trackingData.Rectangle),
                            Color.Black, FrameStyle.Thick);

                        // Recalculate and redraw the new frame
                        _trackingData.UpdateRectangle(mouseX, mouseY);
                        ControlPaint.DrawReversibleFrame(RectangleToScreen(_trackingData.Rectangle),
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
        /// Updates the highlight being drawn during an interactive manual highlight event.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        /// <param name="angularHighlight"><see langword="true"/> if the highlight being updated 
        /// is angular; <see langword="false"/> if it is rectangular.</param>
        void UpdateDrawingHighlight(int mouseX, int mouseY, bool angularHighlight)
        {
            // Get a drawing surface for the interactive highlight
            using (Graphics graphics = CreateGraphics())
            {
                // Get the appropriate color for drawing the object
                Color drawColor = GetHighlightDrawColor();

                // Erase the previous highlight if it exists
                GdiGraphics gdiGraphics = new GdiGraphics(graphics, RasterDrawMode.NotXorPen);
                gdiGraphics.FillRegion(_trackingData.Region, drawColor);

                // Recalculate and redraw the new interactive highlight
                if (angularHighlight)
                {
                    _trackingData.UpdateAngularRegion(mouseX, mouseY);
                }
                else
                {
                    _trackingData.UpdateRectangularRegion(mouseX, mouseY);
                }

                gdiGraphics.FillRegion(_trackingData.Region, drawColor);
            }
        }

        /// <summary>
        /// Gets the color used to draw highlight during a manual redaction event.
        /// </summary>
        /// <returns>The color used to draw highlight during a manual redaction event.</returns>
        Color GetHighlightDrawColor()
        {
            if (_cursorTool == CursorTool.AngularHighlight ||
                _cursorTool == CursorTool.RectangularHighlight)
            {
                return _defaultHighlightColor;
            }
            
            return _defaultRedactionPaintColor;
        }

        /// <summary>
        /// Completes the interactive tracking action at the specified mouse position.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        /// <param name="cancel"><see langword="true"/> if the tracking event was canceled,
        /// <see langword="false"/> if the event is to be completed.</param>
        void EndTracking(int mouseX, int mouseY, bool cancel)
        {
            try
            {
                // [DataEntry:632, 624]
                // Prevent calls from multiple threads from EndTracking at the same time which can
                // lead to exceptions. There is no need to mutex since a second call wouldn't be
                // doing anything the first didnt' do.
                if (_trackingEventEnding)
                {
                    return;
                }
                _trackingEventEnding = true;

                // Update the tracking dependent upon the active cursor tool
                switch (_cursorTool)
                {
                    case CursorTool.AngularHighlight:
                    case CursorTool.AngularRedaction:

                        EndAngularHighlightOrRedaction(mouseX, mouseY, cancel);
                        break;

                    case CursorTool.DeleteLayerObjects:

                        EndDeleteLayerObjects(mouseX, mouseY, cancel);
                        break;

                    case CursorTool.RectangularHighlight:
                    case CursorTool.RectangularRedaction:

                        EndRectangularHighlightOrRedaction(mouseX, mouseY, cancel);
                        break;

                    case CursorTool.SelectLayerObject:
                        EndSelectLayerObject(mouseX, mouseY, cancel);
                        break;

                    case CursorTool.SetHighlightHeight:

                        EndSetHighlightHeight(mouseX, mouseY, cancel);
                        break;

                    default:

                        // There is no interactive region associated with this region OR
                        // the interactivity is handled by the Leadtools RasterImageViewer
                        break;
                }

                _trackingData.Dispose();
                _trackingData = null;

                // Restore the original cursor tool
                Cursor = _toolCursor ?? Cursors.Default;

                _trackingEventEnding = false;
            }
            catch (Exception ex)
            {
                _trackingEventEnding = false;

                throw ExtractException.AsExtractException("ELI27276", ex);
            }
        }

        /// <summary>
        /// Ends the tracking of a select layer object event.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        /// <param name="cancel"><see langword="true"/> if the tracking event was canceled,
        /// <see langword="false"/> if the event is to be completed.</param>
        void EndSelectLayerObject(int mouseX, int mouseY, bool cancel)
        {
            // Check if this is a click and drag to select multiple highlights
            if (!cancel && Cursor == Cursors.Default)
            {
                // Convert the rectangle from client to image coordinates
                Rectangle rectangle =
                    GetTransformedRectangle(_trackingData.Rectangle, true);

                // Deselect previous layerObject unless modifier key is pressed
                if (ModifierKeys != Keys.Shift)
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
                Invalidate();
            }
            else
            {
                // This was a layer object event

                // Check if the layer object event succeeded
                bool success = !cancel && CheckIfLayerObjectEventSucceeded(new Point(mouseX, mouseY));

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
            Invalidate();

            // Reset the selection cursor
            Cursor = GetSelectionCursor(mouseX, mouseY);
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
        /// <param name="cancel"><see langword="true"/> if the tracking event was canceled,
        /// <see langword="false"/> if the event is to be completed.</param>
        void EndAngularHighlightOrRedaction(int mouseX, int mouseY, bool cancel)
        {
            // Get a drawing surface for the interactive highlight
            Region tempRegion = null;
            Graphics graphics = null;
            try
            {
                graphics = CreateGraphics();

                // Get the appropriate color for drawing the object
                Color drawColor = GetHighlightDrawColor();

                // Save the current tracking region
                tempRegion = _trackingData.Region.Clone();

                // If the event was not canceled process the end of the tracking
                if (!cancel)
                {
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
                                RasterZone zone = new RasterZone(points[0], points[1],
                                    _defaultHighlightHeight, _pageNumber);
                                RasterZone[] zones = GetFittedZones(zone);
                                if (zones != null)
                                {
                                    Redaction redaction = new Redaction(this, PageNumber,
                                        LayerObject.ManualComment, zones, _defaultRedactionFillColor);
                                    _layerObjects.Add(redaction);
                                }
                            }
                        }
                    }
                }

                // Erase the previous highlight if it exists
                GdiGraphics gdiGraphics = new GdiGraphics(graphics, RasterDrawMode.NotXorPen);
                gdiGraphics.FillRegion(tempRegion, drawColor);
            }
            finally
            {
                if (tempRegion != null)
                {
                    tempRegion.Dispose();
                }
                if (graphics != null)
                {
                    graphics.Dispose();
                }
            }

            // Invalidate the image viewer so the object is updated
            Invalidate();
        }

        /// <summary>
        /// Performs block fitting for all <see cref="Highlight"/> or
        /// <see cref="CompositeHighlightLayerObject"/> objects in the
        /// current selection.
        /// </summary>
        internal void BlockFitSelectedZones()
        {
            try
            {
                using (new TemporaryWaitCursor())
                {
                    // Get the list of selected layer objects
                    List<LayerObject> objects = new List<LayerObject>(_layerObjects.Selection);

                    // Iterate through the list of selected objects
                    foreach (LayerObject layerObject in objects)
                    {
                        // If the object is a single highlight, attempt to block fit it.
                        Highlight highlight = layerObject as Highlight;
                        if (highlight != null)
                        {
                            // Fit the highlight (pass false so that layer object changed
                            // events are raised)
                            BlockFitHighlight(highlight, false);

                            continue;
                        }

                        // If the object is a composite highlight object, attempt
                        // to fit each internal highlight
                        CompositeHighlightLayerObject composite =
                            layerObject as CompositeHighlightLayerObject;
                        if (composite != null)
                        {
                            foreach (Highlight compositeHighlight in composite.Objects)
                            {
                                // Fit the highlight (pass true so that layer object
                                // changed events are not raised)
                                BlockFitHighlight(compositeHighlight, true);
                            }

                            // Set the dirty flag (which will also raise
                            // the layer object changed event).
                            composite.Dirty = true;
                        }
                    }

                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30081", ex);
            }
        }

        /// <summary>
        /// Performs block fitting on the specified <see cref="Highlight"/> object.  If
        /// a block fitted zone can be found the <see cref="Highlight"/> will be modified
        /// to fit that zone.  If no zone can be found the <see cref="Highlight"/> will
        /// be unchanged.
        /// </summary>
        /// <param name="highlight">The <see cref="Highlight"/> to fit.</param>
        /// <param name="quietSetData">Whether or not the
        /// <see cref="Highlight.SetSpatialData(RasterZone, bool)"/> call should raise a
        /// layer object changed event.</param>
        void BlockFitHighlight(Highlight highlight, bool quietSetData)
        {
            // Get the fitted zone (do not delete zones if the returned fitted zone is null)
            RasterZone zone = GetBlockFittedZone(highlight.ToRasterZone());
            if (zone != null)
            {
                // Update the highlights spatial data with the fitted zone
                highlight.SetSpatialData(zone, quietSetData);
            }
        }

        /// <summary>
        /// Determines an array of raster zones that fit within the specified zone using the 
        /// appropriate auto fitting algorithm based on the modifier key pressed.
        /// </summary>
        /// <param name="zone">The zone to auto fit.</param>
        /// <returns>An array of autofitted raster zones that fit within <paramref name="zone"/>.
        /// </returns>
        RasterZone[] GetFittedZones(RasterZone zone)
        {
            RasterZone[] zones;
            if (ModifierKeys == Keys.Shift)
            {
                zones = GetLineFittedZones(zone);
            }
            else if (ModifierKeys == (Keys.Shift | Keys.Control))
            {
                RasterZone blockFittedZone = GetBlockFittedZone(zone);
                zones = blockFittedZone == null ? null : new RasterZone[] {blockFittedZone};
            }
            else
            {
                zones = new RasterZone[] { zone };
            }

            return zones;
        }

        /// <summary>
        /// Gets the smallest raster zone that contains all the black pixels of the specified zone.
        /// </summary>
        /// <param name="zone">The raster zone to block fit.</param>
        /// <returns>The smallest raster zone that contains all the black pixels of 
        /// <paramref name="zone"/>.</returns>
        RasterZone GetBlockFittedZone(RasterZone zone)
        {
            using (PixelProbe probe = _reader.CreatePixelProbe(zone.PageNumber))
            {
                FittingData data = GetFittingData(zone);

                return GetBlockFittedZone(data, probe);
            }
        }


        /// <summary>
        /// Gets the smallest raster zone that contains all the black pixels of the specified data.
        /// </summary>
        /// <param name="leftTop">The left top coordinates of the area to block fit.</param>
        /// <param name="rightBottom">The right bottom coordinates of the area to block fit.</param>
        /// <param name="theta">The angle of the resultant raster zone.</param>
        /// <param name="pageNumber">The page number of the image to search.</param>
        /// <param name="probe">A probe to use to test for black pixels.</param>
        /// <returns>The smallest raster zone that contains all the black pixels of the specified 
        /// data.</returns>
        static RasterZone GetBlockFittedZone(PointF leftTop, PointF rightBottom, PointF theta,
            int pageNumber, PixelProbe probe)
        {
            FittingData data = new FittingData(leftTop, rightBottom, theta, pageNumber);

            return GetBlockFittedZone(data, probe);
        }

        /// <summary>
        /// Gets the smallest raster zone that contains all the black pixels of the specified data.
        /// </summary>
        /// <param name="data">The fitting data to use for block fitting.</param>
        /// <param name="probe">A probe to use to test for black pixels.</param>
        /// <returns>The smallest raster zone that contains all the black pixels of the specified 
        /// data.</returns>
        static RasterZone GetBlockFittedZone(FittingData data, PixelProbe probe)
        {
            // Shrink each side of the highlight
            ShrinkLeftSide(data, probe);
            ShrinkTopSide(data, probe);
            ShrinkRightSide(data, probe);
            ShrinkBottomSide(data, probe);

            // Find the corners of the fitted highlight
            PointF point1 = GeometryMethods.Rotate(data.LeftTop, data.Theta);
            PointF point2 = GeometryMethods.Rotate(data.LeftTop.X, data.RightBottom.Y, data.Theta);
            PointF point3 = GeometryMethods.Rotate(data.RightBottom, data.Theta);
            PointF point4 = GeometryMethods.Rotate(data.RightBottom.X, data.LeftTop.Y, data.Theta);

            // Create the raster zone
            int startX = (int)((point1.X + point2.X) / 2);
            int startY = (int)((point1.Y + point2.Y) / 2);
            int endX = (int)((point3.X + point4.X) / 2);
            int endY = (int)((point3.Y + point4.Y) / 2);
            int height = (int)(data.RightBottom.Y - data.LeftTop.Y);

            if (height < _MIN_SPLIT_HEIGHT)
            {
                return null;
            }

            return new RasterZone(startX, startY, endX, endY, height, data.PageNumber);
        }

        /// <summary>
        /// Gets a series of block fitted zones that fit within the specified zone.
        /// </summary>
        /// <param name="zone">The zone to line fit.</param>
        /// <returns>A series of block fitted zones that fit within <paramref name="zone"/>.
        /// </returns>
        RasterZone[] GetLineFittedZones(RasterZone zone)
        {
            List<RasterZone> zones = new List<RasterZone>();
            using (PixelProbe probe = _reader.CreatePixelProbe(zone.PageNumber))
            {
                FittingData data = GetFittingData(zone);

                // Trim whitespace at the top and bottom
                ShrinkLeftSide(data, probe);
                ShrinkTopSide(data, probe);
                ShrinkRightSide(data, probe);
                ShrinkBottomSide(data, probe);

                // Attempt to split the highlight as many times as possible
                RasterZone blockFittedZone;
                float rowToSplit = FindRowToSplit(data, probe);
                while (rowToSplit > 0)
                {
                    // Create the zone entity
                    PointF rightBottom = new PointF(data.RightBottom.X, rowToSplit);
                    blockFittedZone = GetBlockFittedZone(data.LeftTop, rightBottom, data.Theta,
                        zone.PageNumber, probe);

                    // Check if there was an error
                    if (blockFittedZone == null)
                    {
                        // This is a non-serious logic error. End the loop to 
                        // create the highlight without any further line fitting.
                        break;
                    }

                    // Store the entity ID
                    zones.Add(blockFittedZone);

                    // Store the new highlight's top
                    data.LeftTop.Y = rowToSplit + 1;

                    // Trim whitespace from the top
                    ShrinkTopSide(data, probe);

                    // Look for a place to split the highlight
                    rowToSplit = FindRowToSplit(data, probe);
                }

                // Create the last highlight and store the ID
                blockFittedZone = GetBlockFittedZone(data, probe);
                if (blockFittedZone != null)
                {
                    zones.Add(blockFittedZone);
                }
            }

            // If no highlights were created, throw an exception
            return zones.Count == 0 ? null : zones.ToArray();
        }

        /// <summary>
        /// Shrinks the left side of the fitting data to exclude all-white pixel columns.
        /// </summary>
        /// <param name="data">The fitting data to shrink.</param>
        /// <param name="probe">The probe used to test image pixels.</param>
        static void ShrinkLeftSide(FittingData data, PixelProbe probe)
        {
            // Iterate over each column, checking if the left side can be shrunk by one pixel
            while (data.LeftTop.X < data.RightBottom.X)
            {
                // Iterate over each pixel in this column
                for (float y = data.LeftTop.Y; y <= data.RightBottom.Y; y++)
                {
                    // Check whether this pixel is black
                    if (IsPixelBlack(data.LeftTop.X, y, data.Theta, probe))
                    {
                        // We are done.
                        return;
                    }
                }

                data.LeftTop.X++;
            }
        }

        /// <summary>
        /// Shrinks the right side of the fitting data to exclude all-white pixel columns.
        /// </summary>
        /// <param name="data">The fitting data to shrink.</param>
        /// <param name="probe">The probe used to test image pixels.</param>
        static void ShrinkRightSide(FittingData data, PixelProbe probe)
        {
            // Iterate over each column, checking if the right side can be shrunk by one pixel
            while (data.RightBottom.X > data.LeftTop.X)
            {
                // Iterate over each pixel in this column
                for (float y = data.LeftTop.Y; y <= data.RightBottom.Y; y++)
                {
                    // Check whether this pixel is black
                    if (IsPixelBlack(data.RightBottom.X, y, data.Theta, probe))
                    {
                        // We are done.
                        return;
                    }
                }

                data.RightBottom.X--;
            }
        }

        /// <summary>
        /// Shrinks the top side of the fitting data to exclude all-white pixel rows.
        /// </summary>
        /// <param name="data">The fitting data to shrink.</param>
        /// <param name="probe">The probe used to test image pixels.</param>
        static void ShrinkTopSide(FittingData data, PixelProbe probe)
        {
            // Iterate over each row, checking if the top side can be shrunk by one pixel
            while (data.LeftTop.Y < data.RightBottom.Y)
            {

                // Check whether this row contains a black pixel
                if (RowContainsBlackPixel(data.LeftTop.Y, data.LeftTop.X, data.RightBottom.X, data.Theta, probe))
                {
                    // We are done.
                    return;
                }

                data.LeftTop.Y++;
            }
        }

        /// <summary>
        /// Shrinks the bottom side of the fitting data to exclude all-white pixel rows.
        /// </summary>
        /// <param name="data">The fitting data to shrink.</param>
        /// <param name="probe">The probe used to test image pixels.</param>
        static void ShrinkBottomSide(FittingData data, PixelProbe probe)
        {
            // Iterate over each row, checking if the bottom side can be shrunk by one pixel
            while (data.RightBottom.Y > data.LeftTop.Y)
            {
                // Check whether this row contains a black pixel
                if (RowContainsBlackPixel(data.RightBottom.Y, data.LeftTop.X, data.RightBottom.X, data.Theta, probe))
                {
                    // We are done.
                    return;
                }

                data.RightBottom.Y--;
            }
        }

        /// <summary>
        /// Gets the fitting data that corresponds to the specified raster zone.
        /// </summary>
        /// <param name="zone">The zone from which to retrieve fitting data.</param>
        /// <returns>The fitting data that corresponds to <paramref name="zone"/>.</returns>
        FittingData GetFittingData(RasterZone zone)
        {
            // Calculate the angle of the line dy/dx
            double angle = Math.Atan2(zone.EndY - zone.StartY, zone.EndX - zone.StartX);

            // Express the angle from horizontal as a number between -PI/2 and PI/2 [LegacyRCAndUtils #5205]
            angle = GeometryMethods.GetAngleFromHorizontal(angle);

            // Calculate the horizontal and vertical increments
            PointF theta = new PointF((float)Math.Sin(angle), (float)Math.Cos(angle));

            // Calculate the x and y modifiers
            PointF modifier = new PointF(zone.Height / 2F * theta.X, zone.Height / 2F * theta.Y);

            // Calculate opposing corners of the highlight as Points
            PointF leftTop = new PointF(zone.StartX - modifier.X, zone.StartY + modifier.Y);
            PointF rightBottom = new PointF(zone.EndX + modifier.X, zone.EndY - modifier.Y);

            // Thetas should be relative to the side that is closest to the viewer's horizontal. 
            // [LegacyRCAndUtils #5066]
            angle += Orientation * Math.PI / 180.0;

            // Express the angle from horizontal as a number between -PI/2 and PI/2 [LegacyRCAndUtils #5205]
            angle = GeometryMethods.GetAngleFromHorizontal(angle);

            // Calculate the thetas
            theta.X = (float)Math.Sin(angle);
            theta.Y = (float)Math.Cos(angle);

            // Calculate the min/max x/y of the deskewed highlight
            PointF negativeTheta = new PointF((float)Math.Sin(-angle), (float)Math.Cos(-angle));
            leftTop = GeometryMethods.Rotate(leftTop, negativeTheta);
            rightBottom = GeometryMethods.Rotate(rightBottom, negativeTheta);

            if (leftTop.X > rightBottom.X)
            {
                float tmp = leftTop.X;
                leftTop.X = rightBottom.X;
                rightBottom.X = tmp;
            }
            if (leftTop.Y > rightBottom.Y)
            {
                float tmp = leftTop.Y;
                leftTop.Y = rightBottom.Y;
                rightBottom.Y = tmp;
            }

            // Return the fitting data
            return new FittingData(leftTop, rightBottom, theta, zone.PageNumber);
        }

        /// <summary>
        /// Determines if the specified row of pixels contains at least one black pixel.
        /// </summary>
        /// <param name="left">The left most pixel in the row.</param>
        /// <param name="y">The y coordinate of the row.</param>
        /// <param name="right">The right most pixel in the row.</param>
        /// <param name="theta">The cosine and sine theta of the angle of the row.</param>
        /// <param name="probe">The probe used to test image pixels.</param>
        /// <returns><see langword="true"/> if any pixel in the <paramref name="y"/> from 
        /// <paramref name="left"/> to <paramref name="right"/> is black; <see langword="false"/> 
        /// if all pixels in the <paramref name="y"/> from <paramref name="left"/> to 
        /// <paramref name="right"/> are white.</returns>
        static bool RowContainsBlackPixel(float y, float left, float right, PointF theta, PixelProbe probe)
        {
            for (float x = left; x <= right; x++)
            {
                // Check whether this pixel is black
                if (IsPixelBlack(x, y, theta, probe))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if the specified pixel is black.
        /// </summary>
        /// <param name="x">The x coordinate of the pixel to test.</param>
        /// <param name="y">The y coordinate of the pixel to test.</param>
        /// <param name="theta">The cosine and sine of the angle of rotation of the coordinate 
        /// system.</param>
        /// <param name="probe">The probe used to test image pixels.</param>
        /// <returns><see langword="true"/> if the specified pixel is black; 
        /// <see langword="false"/> if the specified pixel is white.</returns>
        static bool IsPixelBlack(float x, float y, PointF theta, PixelProbe probe)
        {
            PointF pixel = GeometryMethods.Rotate(x, y, theta);
            Point point = Point.Round(pixel);

            return probe.Contains(point) && probe.IsPixelBlack(point);
        }

        /// <summary>
        /// Determines the y coordinate that could split the fitting data into two regions.
        /// </summary>
        /// <param name="data">The data to split.</param>
        /// <param name="probe">The probe to use to test image pixels.</param>
        /// <returns>The y coordinate of a row of white pixels were it would be appropriate to 
        /// split the fitting <paramref name="data"/>.</returns>
        static float FindRowToSplit(FittingData data, PixelProbe probe)
        {
            // Starting at the minimum split height, find the first row containing a black pixel
            float row = data.LeftTop.Y + _MIN_SPLIT_HEIGHT - 2;
            do
            {
                row++;

                // Ensure both highlights meet the minimum splitting height
                if (row >= data.RightBottom.Y - _MIN_SPLIT_HEIGHT)
                {
                    return -1;
                }
            }
            while (!RowContainsBlackPixel(row, data.LeftTop.X, data.RightBottom.X, data.Theta, probe));

            // Look for the next row of white pixels
            do
            {
                row++;

                // Ensure both highlights meet the minimum splitting height
                if (row > data.RightBottom.Y - _MIN_SPLIT_HEIGHT)
                {
                    return -1;
                }
            }
            while (RowContainsBlackPixel(row, data.LeftTop.X, data.RightBottom.X, data.Theta, probe));

            // Store this point. This is the row to split if minimum requirements have been met.
            float rowToSplit = row;

            // Starting at the minimum split height for the second highlight, 
            // find the first row containing a black pixel.
            row = data.RightBottom.Y - _MIN_SPLIT_HEIGHT + 2;
            do
            {
                row--;

                // Ensure both highlights meet the minimum splitting height
                if (row <= rowToSplit)
                {
                    return -1;
                }
            }
            while (!RowContainsBlackPixel(row, data.LeftTop.X, data.RightBottom.X, data.Theta, probe));

            // If we reached this point, we have found a row that can split the highlight
            return rowToSplit;
        }

        /// <summary>
        /// Ends the tracking of the delete layer objects interactive event.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        /// <param name="cancel"><see langword="true"/> if the tracking event was canceled,
        /// <see langword="false"/> if the event is to be completed.</param>
        void EndDeleteLayerObjects(int mouseX, int mouseY, bool cancel)
        {
            // Erase the previous frame if it exists
            Rectangle rectangle = _trackingData.Rectangle;
            ControlPaint.DrawReversibleFrame(RectangleToScreen(rectangle),
                Color.Black, FrameStyle.Thick);

            // If the event was canceled, there is nothing more to do.
            if (cancel)
            {
                return;
            }

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
            Invalidate();

            // Restore the last continuous use cursor tool
            CursorTool = _lastContinuousUseTool;
        }

        /// <summary>
        /// Ends the tracking of the create rectangular highlight interactive event.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        /// <param name="cancel"><see langword="true"/> if the tracking event was canceled,
        /// <see langword="false"/> if the event is to be completed.</param>
        void EndRectangularHighlightOrRedaction(int mouseX, int mouseY, bool cancel)
        {
            Region tempRegion = null;
            Graphics graphics = null;
            try
            {
                graphics = CreateGraphics();

                // Get the appropriate color for drawing the object
                Color drawColor = GetHighlightDrawColor();

                // Store the current tracking region
                tempRegion = _trackingData.Region.Clone();

                GdiGraphics gdiGraphics = new GdiGraphics(graphics, RasterDrawMode.MaskPen);

                // If the event was not canceled process the end of the tracking
                if (!cancel)
                {
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

                        // Draw the new highlight
                        gdiGraphics.FillRegion(_trackingData.Region, drawColor);

                        if (_cursorTool == CursorTool.RectangularHighlight)
                        {
                            // Add a new highlight to _layerObjects
                            _layerObjects.Add(new Highlight(this, LayerObject.ManualComment, points[0],
                                points[1], height));
                        }
                        else
                        {
                            // Add a new redaction to _layerObjects
                            RasterZone zone = new RasterZone(points[0], points[1], height, _pageNumber);
                            RasterZone[] zones = GetFittedZones(zone);
                            if (zones != null)
                            {
                                _layerObjects.Add(new Redaction(this, PageNumber,
                                    LayerObject.ManualComment, zones, _defaultRedactionFillColor));
                            }
                        }
                    }
                }

                // Erase the previous highlight if it exists
                gdiGraphics.DrawMode = RasterDrawMode.NotXorPen;
                gdiGraphics.FillRegion(tempRegion, drawColor);
            }
            finally
            {
                if (tempRegion != null)
                {
                    tempRegion.Dispose();
                }
                if (graphics != null)
                {
                    graphics.Dispose();
                }
            }

            // Invalidate the image viewer so the object is updated
            Invalidate();
        }

        /// <summary>
        /// Ends the tracking of the set highlight height interactive event.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        /// <param name="cancel"><see langword="true"/> if the tracking event was canceled,
        /// <see langword="false"/> if the event is to be completed.</param>
        void EndSetHighlightHeight(int mouseX, int mouseY, bool cancel)
        {
            // Erase the previous line if it exists
            Point[] line = _trackingData.Line;
            if (line[0] != line[1])
            {
                ControlPaint.DrawReversibleLine(line[0], line[1], Color.Black);
            }

            // If the event was canceled, there is nothing more to do.
            if (cancel)
            {
                return;
            }

            // Recalculate the new line
            _trackingData.UpdateLine(mouseX, mouseY);

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
            CursorTool = _lastContinuousUseTool;
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
        Matrix GetRotatedMatrix(Matrix matrix, bool reverseRotation)
        {
            // Return a copy of the original matrix if no image is open.
            if (base.Image == null)
            {
                return matrix.Clone();
            }

            // Calculate the reversal multiplier value
            int reverseValue = reverseRotation ? -1 : 1;

            // Get the current orientation
            int orientation = _imagePages[_pageNumber - 1].Orientation;

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
        ZoomInfo GetZoomInfo()
        {
            // Return the ZoomInfo
            return new ZoomInfo(GetVisibleImageCenter(), ScaleFactor, _fitMode);
        }

        /// <summary>
        /// Get the center of the visible image area in logical (image) coordinates.
        /// </summary>
        /// <returns>The center of the visible image area in logical (image) coordinates.
        /// </returns>
        Point GetVisibleImageCenter()
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
            return Rectangle.Intersect(PhysicalViewRectangle, ClientRectangle);
        }

        /// <overloads>Returns a rectangle transformed by an affine matrix.</overloads>
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
            return GetTransformedRectangle(rectangle, clientToImage, _transform);
        }

        /// <summary>
        /// Returns the specified rectangle transformed by the specified matrix.
        /// </summary>
        /// <param name="rectangle">The rectangle to return in a different coordinate system.
        /// </param>
        /// <param name="invertMatrix"><see langword="true"/> if <paramref name="rectangle"/> is 
        /// in client coordinates and should be returned in image coordinates; 
        /// <see langword="false"/> if <paramref name="rectangle"/> is in image coordinates and 
        /// should be returned in client coordinates.</param>
        /// <param name="transform">The transformation matrix to use.</param>
        /// <returns><paramref name="rectangle"/> in the coordinate system specified.</returns>
        static Rectangle GetTransformedRectangle(Rectangle rectangle, bool invertMatrix, 
            Matrix transform)
        {
            try
            {
                // NOTE: This method assumes the transformation matrix has a rotation that is a 
                // multiple of 90 degrees. The results will be incorrect for other rotations.

                // Get the top-left and bottom-right corners of the rectangle.
                Point[] corners = new Point[] 
                {
                    new Point(rectangle.Left, rectangle.Top),
                    new Point(rectangle.Right, rectangle.Bottom)
                };

                // Check the direction of conversion
                if (invertMatrix)
                {
                    // Convert the corners from client coordinates to image coordinates
                    using (Matrix clientToImageMatrix = transform.Clone())
                    {
                        clientToImageMatrix.Invert();
                        clientToImageMatrix.TransformPoints(corners);
                    }
                }
                else
                {
                    // Convert the corners from image to client coordinates
                    transform.TransformPoints(corners);
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
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26543", ex);
            }
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
        /// updated if an image is open; <see langword="false"/> if it should not be updated.
        /// </param>
        /// <param name="raiseZoomChanged"><see langword="true"/> if the <see cref="ZoomChanged"/> 
        /// event should be raised if an image is open; <see langword="false"/> if it should not 
        /// be raised.</param>
        /// <param name="raiseFitModeChanged"><see langword="true"/> if the 
        /// <see cref="FitModeChanged"/> event should be raised; <see langword="false"/> if it 
        /// should not be raised.</param>
        /// <exception cref="ExtractException"><paramref name="fitMode"/> is invalid.</exception>
        /// <event cref="ZoomChanged"><paramref name="raiseZoomChanged"/> was 
        /// <see langword="true"/>.</event>
        /// <event cref="FitModeChanged">Method was successful.</event>
        void SetFitMode(FitMode fitMode, bool updateZoomHistory, bool raiseZoomChanged, 
            bool raiseFitModeChanged)
        {
            switch (fitMode)
            {
                case FitMode.FitToPage:

                    // Set the fit mode
                    base.SizeMode = RasterPaintSizeMode.FitAlways;
                    ScaleFactor = _DEFAULT_SCALE_FACTOR;
                    break;

                case FitMode.FitToWidth:

                    // Set the fit mode, preserving the scroll position
                    Point scrollPosition = ScrollPosition;
                    base.SizeMode = RasterPaintSizeMode.FitWidth;
                    ScaleFactor = _DEFAULT_SCALE_FACTOR;
                    ScrollPosition = scrollPosition;
                    break;

                case FitMode.None:

                    // Zoom the specified rectangle
                    base.ZoomToRectangle(DisplayRectangle);
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

            // Raise the FitModeChanged event if necessary
            if (raiseFitModeChanged)
            {
                OnFitModeChanged(new FitModeChangedEventArgs(_fitMode));
            }
        }

        /// <summary>
        /// Sets the zoom info and optionally updates the zoom history.
        /// </summary>
        /// <param name="zoomInfo">The new zoom info.</param>
        /// <param name="updateZoomHistory"><see langword="true"/> if the zoom history should be 
        /// updated; <see langword="false"/> if it should not be updated.</param>
        /// <event cref="ZoomChanged">Method was successful.</event>
        void SetZoomInfo(ZoomInfo zoomInfo, bool updateZoomHistory)
        {
            // Check if the fit mode is specified
            if (zoomInfo.FitMode != FitMode.None)
            {
                SetFitMode(zoomInfo.FitMode, updateZoomHistory, true, true);
            }
            else
            {
                // Reset the fit mode if it is set
                if (_fitMode != FitMode.None)
                {
                    SetFitMode(FitMode.None, false, false, true);
                }

                // Set the new scale factor
                ScaleFactor = zoomInfo.ScaleFactor;
            }

            // Center at the specified point
            CenterAtPoint(zoomInfo.Center, updateZoomHistory, true);
        }

        /// <summary>
        /// Loads the default shortcut keys into <see cref="_mainShortcuts"/>.
        /// </summary>
        void LoadDefaultShortcuts()
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
        void AdjustHighlightHeight(int mouseX, int mouseY, bool increaseHeight)
        {
            // Adjust the highlight height only if an interactive angular highlight is being drawn.
            if (_trackingData != null &&
                (_cursorTool == CursorTool.AngularHighlight || _cursorTool == CursorTool.AngularRedaction))
            {
                // Increase/decrease highlight height as specified
                _defaultHighlightHeight += 
                    increaseHeight ? _MOUSEWHEEL_HEIGHT_INCREMENT : -_MOUSEWHEEL_HEIGHT_INCREMENT;

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
        static bool LayeredObjectHasProperTag(LayerObject layerObject,
            IEnumerable<string> requiredTags, ArgumentRequirement tagsArgumentRequirement)
        {
            // Ensure the required tags is not null
            ExtractException.Assert("ELI22339", "Required tags may not be null.",
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
        /// specifying how to treat the <paramref name="requiredTypes"/> collection.</param>
        /// <param name="excludeTypes">The <see cref="IEnumerable{T}"/> of
        /// <see cref="Type"/> to exclude.</param>
        /// <param name="excludeTypesArgumentRequirement">The <see cref="ArgumentRequirement"/>
        /// specifying how to treat the <paramref name="excludeTypes"/> collection.</param>
        /// <returns><see langword="true"/> if the <see cref="LayerObject"/> meets the
        /// requirements and <see langword="false"/> if it does not.</returns>
        static bool LayeredObjectIsProperType(LayerObject layerObject,
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
                foreach (LayerObject layerObject in GetLayeredObjects(tags,
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
        /// Will restore the scroll position to its last saved state.
        /// </summary>
        /// <seealso cref="OnScrollPositionChanged"/>
        public void RestoreScrollPosition()
        {
            // Restore to previously saved scroll position [DNRCAU #262 - JDS]
            ScrollPosition = _scrollPosition;

            // Refresh the image viewer
            Invalidate();
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
            try
            {
                return rectangle.Left >= 0 && rectangle.Top >= 0 &&
                       rectangle.Right <= ImageWidth && rectangle.Bottom <= ImageHeight;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26544", ex);
            }
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
            try
            {
                return rectangle.Left < ImageWidth && rectangle.Top < ImageHeight &&
                       rectangle.Right > 0 && rectangle.Bottom > 0;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26545", ex);
            }
        }

        #region Shortcut Key Methods

        /// <summary>
        /// Activates the pan <see cref="CursorTool"/>.
        /// </summary>
        public void SelectPanTool()
        {
            try
            {
                if (base.Image != null)
                {
                    CursorTool = CursorTool.Pan;
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
                    CursorTool = CursorTool.ZoomWindow;
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
                        CursorTool = CursorTool.AngularHighlight;
                    }
                    else if (_cursorTool == CursorTool.AngularHighlight)
                    {
                        CursorTool = CursorTool.RectangularHighlight;
                    }
                    else
                    {
                        // Select the last used highlight tool
                        CursorTool = RegistryManager.GetLastUsedHighlightTool();
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
                    CursorTool = CursorTool.AngularHighlight;
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
                    CursorTool = CursorTool.RectangularHighlight;
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
                        CursorTool = CursorTool.AngularRedaction;
                    }
                    else if (_cursorTool == CursorTool.AngularRedaction)
                    {
                        CursorTool = CursorTool.RectangularRedaction;
                    }
                    else
                    {
                        // Select the last used redaction tool
                        CursorTool = RegistryManager.GetLastUsedRedactionTool();
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
                    CursorTool = CursorTool.EditHighlightText;
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
                    CursorTool = CursorTool.DeleteLayerObjects;
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
                // Do not switch to the selection tool if it is already the current tool
                if (base.Image != null && CursorTool != CursorTool.SelectLayerObject)
                {
                    CursorTool = CursorTool.SelectLayerObject;
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
                    PageNumber = 1;
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
                    if (_pageNumber > 1)
                    {
                        // Go to the previous page
                        PageNumber = _pageNumber - 1;
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
                    if (_pageNumber < _pageCount)
                    {
                        // Go to the next page
                        PageNumber = _pageNumber + 1;
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
                    PageNumber = _pageCount;
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
                    Rotate(90);
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
                    Rotate(270);
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
                    true, true, true);
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
                    true, true, true);
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
                        _openImageFilterIndex > _openImageFileTypeFilter.Count 
                            ? 1 : _openImageFilterIndex;
                    fileDialog.Title = "Open Image File";

                    // Show the dialog and bail if the user selected cancel
                    if (fileDialog.ShowDialog() == DialogResult.Cancel)
                    {
                        return;
                    }

                    // Open the specified file and update the MRU list
                    OpenImage(fileDialog.FileName, true);
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
                Rectangle clientArea = GetTransformedRectangle(ClientRectangle, true);

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
                    if (layerObject.PageNumber == _pageNumber)
                    {
                        // Check if the layer object is contained in the current view
                        if (!layerObject.IsContained(clientArea, _pageNumber))
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
                Invalidate();

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
        static bool IsLinkedToUnselectedLayerObject(LayerObject layerObject)
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
                Invalidate();
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
                Point mousePosition = PointToClient(
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
                Point mousePosition = PointToClient(
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
                if (base.Image != null && !IsLastTile)
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
                if (base.Image != null && !IsFirstTile)
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
                if (base.Image != null && CanZoomPrevious)
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
                if (base.Image != null && CanZoomNext)
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
        /// Activates the print view command.
        /// </summary>
        public void SelectPrintView()
        {
            try
            {
                if (base.Image != null)
                {
                    PrintView();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29258", ex);
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

        #endregion Shortcut Key Methods

        #endregion Methods

        #region Static Methods

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
                throw new ExtractException("ELI21232", "Unable to initialize cursor objects.",
                    ex);
            }
        }

        /// <summary>
        /// Calculates the vertical scale factor of the <see cref="ImageViewer"/>.
        /// </summary>
        /// <remarks>The vertical scale factor is the magnitude of a vertical unit vector after 
        /// it has been transformed by the matrix.</remarks>
        /// <returns>The vertical scale factor of the <see cref="ImageViewer"/>.</returns>
        public double GetScaleFactorY()
        {
            return GetScaleFactorY(_transform);
        }

        /// <summary>
        /// Calculates the vertical scale factor of a 3x3 affine matrix.
        /// </summary>
        /// <remarks>The vertical scale factor is the magnitude of a vertical unit vector after 
        /// it has been transformed by the matrix.</remarks>
        /// <param name="matrix">3x3 affine matrix from which to calculate the vertical scale 
        /// factor.</param>
        /// <returns>The vertical scale factor of the 3x3 affine matrix.</returns>
        static double GetScaleFactorY(Matrix matrix)
        {
            // Formula derived by transforming a vertical unit 
            // vector using an arbitrary transformation matrix:
            //
            // [0 0 1][a b 0]   [ e   f  1]
            // [0 1 1][c d 0] = [c+e d+f 1]
            //        [e f 1]
            //
            // And then applying the distance formula:
            //
            // Sqrt(((c+e)-e)^2 + ((d+f)-f)^2) = Sqrt(c^2 + d^2)
            return Math.Sqrt(Math.Pow(matrix.Elements[2], 2) + Math.Pow(matrix.Elements[3], 2));
        }

        /// <summary>
        /// Will refresh the image viewer and if a parent form is found will
        /// call refresh on the parent as well.
        /// </summary>
        void RefreshImageViewerAndParent()
        {
            // If _parentForm is null attempt to get it
            if (_parentForm == null)
            {
                _parentForm = TopLevelControl as Form;
            }

            // If there is a parent form just refresh the parent
            if (_parentForm != null)
            {
                _parentForm.Refresh();
            }
            else
            {
                Refresh();
            }
        }

        #endregion
    }
}
