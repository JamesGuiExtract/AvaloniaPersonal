using Extract.Drawing;
using Extract.Licensing;
using Leadtools;
using Leadtools.ImageProcessing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Extract.Imaging
{
    /// <summary>
    /// Contains image manipulation methods.
    /// </summary>
    public static class ImageMethods
    {
        #region Constants

        static readonly string _OBJECT_NAME = typeof(ImageMethods).ToString();

        #endregion Constants

        /// <overloads>Generates a thumbnail of a specified scale size from a specified
        /// original image.</overloads>
        /// <summary>
        /// Generates a high quality thumbnail image that is the specified scaling percentage
        /// of the original image.
        /// <para><b>Note:</b></para>
        /// This method will default the interpolation mode to
        /// <see cref="InterpolationMode.HighQualityBicubic"/>.
        /// </summary>
        /// <param name="imageFileName">The image file to produce the thumbnail from.
        /// Must not be <see langword="null"/> or empty string.</param>
        /// <param name="percentage">The percentage to scale by. Must be greater than 0.
        /// <para><b>Note:</b></para>
        /// You can specify a number greater than 100, but the returned image will be larger
        /// than the original image (technically not a "thumbnail").
        /// </param>
        /// <returns>A thumbnail version of the original image.</returns>
        public static Image GenerateThumbnail(string imageFileName, int percentage)
        {
            return GenerateThumbnail(imageFileName, percentage,
                InterpolationMode.HighQualityBicubic);
        }

        /// <summary>
        /// Generates a thumbnail image with the specified quality and scaling percentage from
        /// the specified original image.
        /// </summary>
        /// <param name="imageFileName">The image file to produce the thumbnail from.
        /// Must not be <see langword="null"/> or empty string.</param>
        /// <param name="percentage">The percentage to scale by. Must be greater than 0.
        /// <para><b>Note:</b></para>
        /// You can specify a number greater than 100, but the returned image will be larger
        /// than the original image (technically not a "thumbnail").
        /// </param>
        /// <param name="interpolationMode">The <see cref="InterpolationMode"/>
        /// to use when scaling.</param>
        public static Image GenerateThumbnail(string imageFileName, int percentage,
            InterpolationMode interpolationMode)
        {
            try
            {
                // Create a Bitmap from the image file
                using (Bitmap bitmap = new Bitmap(imageFileName))
                {
                    return GenerateThumbnail(bitmap, percentage, interpolationMode);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23770", ex);
                ee.AddDebugData("Source For Thumbnail", imageFileName ?? "null", false);
                throw ee;
            }
        }
        
        /// <summary>
        /// Generates a high quality thumbnail image that is the specified scaling percentage
        /// of the original image.
        /// <para><b>Note:</b></para>
        /// This method will default the interpolation mode to
        /// <see cref="InterpolationMode.HighQualityBicubic"/>.
        /// </summary>
        /// <param name="original">The original <see cref="Image"/> to make a thumbnail
        /// for. Must not be <see langword="null"/>.</param>
        /// <param name="percentage">The percentage to scale by. Must be greater than 0.
        /// <para><b>Note:</b></para>
        /// You can specify a number greater than 100, but the returned image will be larger
        /// than the original image (technically not a "thumbnail").
        /// </param>
        /// <returns>A thumbnail version of the original image.</returns>
        public static Image GenerateThumbnail(Image original, int percentage)
        {
            return GenerateThumbnail(original, percentage, InterpolationMode.HighQualityBicubic);
        }

        /// <summary>
        /// Generates a thumbnail image with the specified quality and scaling percentage from
        /// the specified original image.
        /// </summary>
        /// <param name="original">The original <see cref="Image"/> to make a thumbnail
        /// for. Must not be <see langword="null"/>.</param>
        /// <param name="percentage">The percentage to scale by. Must be greater than 0.
        /// <para><b>Note:</b></para>
        /// You can specify a number greater than 100, but the returned image will be larger
        /// than the original image (technically not a "thumbnail").
        /// </param>
        /// <param name="interpolationMode">The <see cref="InterpolationMode"/>
        /// to use when scaling.</param>
        /// <returns>A thumbnail version of the original image.</returns>
        public static Image GenerateThumbnail(Image original, int percentage,
            InterpolationMode interpolationMode)
        {
            Bitmap thumbnail = null;
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28012",
                    _OBJECT_NAME);

                // Ensure that the image object is not null and that the percentage is
                // greater than 0
                ExtractException.Assert("ELI23771", "Image must not be null!", original != null);
                ExtractException.Assert("ELI23772", "Percentage must be greater than 0!",
                    percentage > 0, "Percentage", percentage);

                double scaleFactor = 0.01F * percentage;

                thumbnail = new Bitmap((int)(original.Width * scaleFactor),
                    (int)(original.Height * scaleFactor));

                using (Graphics g = Graphics.FromImage(thumbnail))
                {
                    // Create a rectangle for the thumbnail image
                    Rectangle thumbRect = new Rectangle(0, 0, thumbnail.Width, thumbnail.Height);

                    // Set the interpolation mode
                    g.InterpolationMode = interpolationMode;

                    // Set the background to white (better for gifs with transparent background)
                    g.FillRectangle(Brushes.White, thumbRect);

                    // Draw the thumbnail image
                    g.DrawImage(original, thumbRect,
                        new Rectangle(0, 0, original.Width, original.Height), GraphicsUnit.Pixel);
                }

                return thumbnail;
            }
            catch (Exception ex)
            {
                if (thumbnail != null)
                {
                    thumbnail.Dispose();
                }

                throw ExtractException.AsExtractException("ELI23773", ex);
            }
        }

        /// <summary>
        /// Rotates the specified image by the specified number of degrees.
        /// </summary>
        /// <param name="image">The image to rotate.</param>
        /// <param name="angle">The number of degrees to rotate the image.</param>
        public static void RotateImageByDegrees(RasterImage image, int angle)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI30248",
                    _OBJECT_NAME);

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
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30244", ex);
            }
        }

        /// <summary>
        /// Extracts a sub image from from the specified <paramref name="source"/>.
        /// </summary>
        /// <param name="bounds">The bounds of the sub image to extract.</param>
        /// <param name="source">The source image to extract the sub image from.</param>
        /// <returns>A sub image that has been extracted from the <paramref name="source"/>.</returns>
        public static RasterImage ExtractSubImageFromPage(Rectangle bounds, RasterImage source)
        {
            RasterImage subImage = null;
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI30247",
                    _OBJECT_NAME);

                // Clip the bounds within the bounds of the image
                bounds.Intersect(new Rectangle(new Point(0,0), new Size(source.Width, source.Height)));

                // Source point is the top left of the rotated rectangle
                Point sourcePoint = bounds.Location;
                bounds.Location = new Point(0,0);

                // Create the destination image and set matching resolution
                subImage = new RasterImage(RasterMemoryFlags.Conventional, bounds.Width,
                    bounds.Height, source.BitsPerPixel, source.Order,
                    RasterViewPerspective.TopLeft, source.GetPalette(), IntPtr.Zero, 0);
                subImage.XResolution = source.XResolution;
                subImage.YResolution = source.YResolution;

                // Initialize the destination image to all white.
                FillCommand fillCommand = new FillCommand(new RasterColor(Color.White));
                fillCommand.Run(subImage);

                // Copy the content from the source image into the raster zone image.
                CombineFastCommand combineCommand = new CombineFastCommand(subImage,
                    bounds, sourcePoint, CombineFastCommandFlags.SourceCopy);
                combineCommand.Run(source);

                return subImage;
            }
            catch (Exception ex)
            {
                // Ensure image is disposed if exception is thrown
                if (subImage != null)
                {
                    subImage.Dispose();
                }
                ExtractException ee = new ExtractException("ELI30246",
                    "Unable to extract image.", ex);
                ee.AddDebugData("Bounds", bounds, false);
                throw ee;
            }
        }

        /// <summary>
        /// Extracts into a separate <see cref="RasterImage"/> the specified <see cref="RasterZone"/>
        /// The extracted image will be oriented with the <see cref="RasterZone"/> so that the start
        /// and end point lie along a horizontal line that bisects the resulting image.
        /// </summary>
        /// <param name="page">The <see cref="RasterImage"/> from which the 
        /// <see cref="RasterZone"/> is to be extracted. The zone will be extracted from the 
        /// currently active page.</param>
        /// <param name="zone">The <see cref="RasterZone"/> to be extracted as a separate 
        /// <see cref="RasterImage"/>.</param>
        /// <param name="orientation">A value indicating the image orientation that comes closest
        /// to allowing <see paramref="rasterZone"/> to be vertical.
        /// <list type="bullet">
        /// <item><b>0</b>: Right side up.</item>
        /// <item><b>90</b>: Rotated 90 degrees to the right.</item>
        /// <item><b>180</b>: Upside down.</item>
        /// <item><b>270</b>: Rotated 90 degrees to the left.</item>
        /// </list></param>
        /// <param name="skew">The angle (in degress) the raster zone has been rotated compared to
        /// the source image.</param>
        /// <param name="offset">The distance any image coordinate in the resulting raster zone 
        /// image would need to be offset to be at the corresponding location in the source image.
        /// </param>
        /// <returns>An <see cref="RasterImage"/> containing the image content within the
        /// specified <see cref="RasterZone"/>.  NOTE: The caller is responsible from disposing the
        /// returned <see cref="RasterImage"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "4#")]
        public static RasterImage ExtractZoneFromPage(RasterZone zone, RasterImage page, 
            out int orientation, out double skew, out Size offset)
        {
            RasterImage rasterZoneImage = null;
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28013",
                    _OBJECT_NAME);

                // Calculate the skew of the raster zone in degrees.
                skew = zone.ComputeSkew(true);

                // Calculate whether the start and end points of the raster zone fall within the
                // image page.  If either are off the image page, find a start and end point
                // along the raster zone's plane that are within the image to work with.
                Rectangle pageArea = new Rectangle(0, 0, page.Width, page.Height);
                Point startPoint = zone.Start;
                Point endPoint = zone.End;

                if (!pageArea.Contains(startPoint))
                {
                    startPoint = GeometryMethods.GetClippedEndPoint(endPoint, startPoint, pageArea);
                }
                if (!pageArea.Contains(endPoint))
                {
                    endPoint = GeometryMethods.GetClippedEndPoint(startPoint, endPoint, pageArea);
                }

                // Find the smallest rectangle from the image that completely encompases the raster
                // zone defined by the (possibly clipped) raster zone.  Note this bounding rectangle
                // may extend offpage.
                Rectangle boundingRectangle = GeometryMethods.GetBoundingRectangle(
                    startPoint, endPoint, zone.Height);

                // Create a destination image where the raster zone content will be copied to.  It is
                // important that it is the same size as the bounding rectangle since the raster zone
                // is centered in the bounding rectangle and we will be rotating the destination 
                // raster zone about it's center.
                rasterZoneImage = new RasterImage(RasterMemoryFlags.Conventional, 
                    boundingRectangle.Width, boundingRectangle.Height, page.BitsPerPixel, 
                    page.Order, page.ViewPerspective, page.GetPalette(), IntPtr.Zero, 0);
                rasterZoneImage.OriginalFormat = page.OriginalFormat;

                // Initialize the destination image to all white.
                FillCommand fillCommand = new FillCommand(new RasterColor(Color.White));
                fillCommand.Run(rasterZoneImage);

                // Since the bounding rectangle may extend offpage but we can't copy from offpage
                // coordinates, create an adjusted version of the bounding rectangle the contains
                // only the onpage portion of the bounding rectangle.
                Rectangle adjustedBoundingRectangle = boundingRectangle;
                adjustedBoundingRectangle.Intersect(pageArea);

                // Make note of the image location of the adjusted rectangle in the source image.
                Point sourcePoint = 
                    new Point(adjustedBoundingRectangle.Left, adjustedBoundingRectangle.Top);

                // Move the adjusted rectangle so that it indicates where the contents of the 
                // adjusted bounding rectangle need to be copied in the destination image. (Will be 
                // 0,0 unless the original bounding rectangle extended offpage left of top. In this
                // case, we will leave some empty content at the top or left of the destination
                // image to represent the offpage area of the source image.
                adjustedBoundingRectangle.Offset(-boundingRectangle.Left, -boundingRectangle.Top);

                // Copy the content from the source image into the raster zone image.
                CombineFastCommand combineCommand = new CombineFastCommand(rasterZoneImage, 
                    adjustedBoundingRectangle, sourcePoint, CombineFastCommandFlags.SourceCopy);
                combineCommand.Run(page);

                // Since the center point of the raster zone's on-page content is at the exact 
                // center of the destination image, we can rotate the raster zone image so that
                // it is perfectly upright and still know that it is centered.  (Be sure to allow 
                // re-sizing so content isn't cropped from the raster zone.)
                RotateCommand rotateCommand = new RotateCommand();
                rotateCommand.Angle = (int)(-skew * 100);
                rotateCommand.FillColor = new RasterColor(Color.White);
                rotateCommand.Flags = RotateCommandFlags.Resize | RotateCommandFlags.Bicubic;
                rotateCommand.Run(rasterZoneImage);

                // Since the raster zone image contains extra content to ensure content was not
                // lost from the source image during transfer and rotation, we now need to calculate
                // how much content to clip from the edges of the raster zone image to leave us
                // with only the content in the raster zone itself.
                int finalImageAreaWidth = (int) GeometryMethods.Distance(endPoint, startPoint);
                int xOffset = (rasterZoneImage.Width - finalImageAreaWidth) / 2;
                int yOffset = (rasterZoneImage.Height - zone.Height) / 2;

                Rectangle finalImageArea = new Rectangle(xOffset, yOffset, finalImageAreaWidth,
                    zone.Height);

                // Crop off any excess content leaving only the content from the raster zone itself.
                CropCommand cropCommand = new CropCommand();
                cropCommand.Rectangle = finalImageArea;
                cropCommand.Run(rasterZoneImage);
                
                // To allow coordinates from the resulting image to be translated into the source
                // image, calculate the transform needed to move the center of the resulting raster 
                // image to the top left of the raster zone in the source image.
                using (Matrix transform = new Matrix())
                {
                    // Round the skew to the nearest of 0, 90, 180 or 270, use that value as the
                    // orientation, then adjust the skew so that it doesn't include the orientation.
                    double roundedAngle = GeometryMethods.GetAngleDelta(0, skew, true);
                    roundedAngle = Math.Round(roundedAngle / 90) * 90;
                    orientation = (int)roundedAngle;
                    skew -= orientation;
                    orientation = (orientation + 360) % 360;

                    // Offset needed to get to coordinate 0,0 from the center of the extracted image
                    // area.
                    transform.Translate(-(finalImageArea.Width / 2), -(finalImageArea.Height / 2));

                    // Rotation necessary to rotate the page's orientation into the extract
                    // image's orientation.
                    transform.Rotate(-orientation);

                    // Shift the coordinates as necessary to account for a different corner of the
                    // image being used as the base of the coordinate system in a rotated
                    // orientation.
                    switch (orientation)
                    {
                        case 0: break;
                        case 90: transform.Translate(-page.Width, 0); break;
                        case 180: transform.Translate(-page.Width, -page.Height); break;
                        case 270: transform.Translate(0, -page.Height); break;
                    }

                    // Account for the skew by rotating about the center of the image. At this point
                    // all steps nessary to move a point from the original image's coordinate system
                    // into the coordinate system of the extracted image have been completed.
                    PointF imageCenter = new PointF((float)page.Width / 2, (float)page.Height / 2);
                    transform.RotateAt((float)-skew, imageCenter);

                    // Apply the transform to the point at the center of the on-screen portion of the
                    // raster zone.
                    PointF[] points = { GeometryMethods.GetCenterPoint(startPoint, endPoint) };
                    transform.TransformPoints(points);

                    // The result is the amount any point in the raster zone image would need to be 
                    // offset to be placed correctly in the source image.
                    offset = new Size((int)points[0].X, (int)points[0].Y);
                }

                return rasterZoneImage;
            }
            catch (Exception ex)
            {
                // Ensure image is disposed if an exception is thrown
                if (rasterZoneImage != null)
                {
                    rasterZoneImage.Dispose();
                }

                ExtractException ee = new ExtractException("ELI24087",
                    "Failed to extract raster zone image!", ex);
                ee.AddDebugData("Raster Zone", zone.ToString(), false);
                throw ee;
            }
        }

        /// <summary>
        /// Attempts to create a lead tools device context handle that can be used
        /// to create a graphics object for drawing on/manipulating an image. The
        /// method will retry the specified number of times and either return a
        /// handle or throw an exception if it was unsuccessful after retrying.
        /// </summary>
        /// <param name="imageToClone">The image object that will be cloned.  This should
        /// be a single page <see cref="RasterImage"/>.</param>
        /// <param name="retryCount">The number of times to attempt to create the
        /// device context.</param>
        /// <param name="command">A <see cref="ColorResolutionCommand"/> to use to
        /// convert the cloned page. If <see langword="null"/> the cloned page
        /// will be left as is.  If not <see langword="null"/> the page will
        /// be modified if its bits per pixel are less than the bits per pixel
        /// property of the ColorResolutionCommand.</param>
        /// <param name="pageNumber">The page that is being worked on (used for exception
        /// debug information).</param>
        /// <param name="clonedPage">The image object that the returned device
        /// context relates to.</param>
        /// <returns>A device context (which must be freed via a call to
        /// <see cref="RasterImage.DeleteLeadDC"/>) for the image.</returns>
        // An out parameter is necessary since the Device context that is created is
        // related to the image that it is created from which means we need to return the image
        // for the returned device context.
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId="4#")]
        public static IntPtr GetLeadDCWithRetries(RasterImage imageToClone, int retryCount,
            ColorResolutionCommand command, int pageNumber, out RasterImage clonedPage)
        {
            RasterImage page = null;
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28054",
                    _OBJECT_NAME);

                // Sometimes the call to CreateLeadDC can return an IntPtr.Zero, in this
                // case we should dispose of the page, call garbage collection, reclone
                // the page and retry the CreateLeadDC call [IDSD #331]
                IntPtr hdc = IntPtr.Zero;
                int retries = 0;
                for (; retries < retryCount; retries++)
                {
                    // Clone the page before manipulating it
                    page = imageToClone.Clone();

                    // Run the resolution command (if it exists) on any image that
                    // is less than the commands bits per pixel
                    if (command != null && page.BitsPerPixel < command.BitsPerPixel)
                    {
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
                }

                // If it was successful, but only after a retry, log an exception
                if (hdc != IntPtr.Zero && retries > 0)
                {
                    ExtractException ee = new ExtractException("ELI28182",
                        "Application Trace: Device context created successfully after retry.");
                    ee.AddDebugData("Retries attempted", retries, false);
                    AddImageDebugInfo(ee, imageToClone, pageNumber);
                    ee.Log();
                }
                else if (hdc == IntPtr.Zero)
                {
                    // Dispose of the page if it exists
                    if (page != null)
                    {
                        page.Dispose();
                        page = null;
                    }

                    // Throw an exception
                    ExtractException ee = new ExtractException("ELI28050",
                        "Unable to create device context.");
                    ee.AddDebugData("Memory before reclaim", GC.GetTotalMemory(false), false);
                    ee.AddDebugData("Memory after reclaim", GC.GetTotalMemory(true), false);
                    ee.AddDebugData("Retry count", retryCount, false);
                    AddImageDebugInfo(ee, imageToClone, pageNumber);
                    throw ee;
                }

                clonedPage = page;
                return hdc;
            }
            catch (Exception ex)
            {
                // Dispose of the page if an exception occurred
                if (page != null)
                {
                    page.Dispose();
                }

                throw ExtractException.AsExtractException("ELI28051", ex);
            }
        }

        /// <summary>
        /// Adds debug information about an image to the specified exception.
        /// </summary>
        /// <param name="ee">The exception to add the debug information to.</param>
        /// <param name="image">The image to get the information from.</param>
        /// <param name="pageNumber">The page number of the image that the image
        /// object relates to.</param>
        static void AddImageDebugInfo(ExtractException ee, RasterImage image, int pageNumber)
        {
            try
            {
                ee.AddDebugData("Image Page", pageNumber, false);
                if (image != null)
                {
                    ee.AddDebugData("Image Bits Per Pixel", image.BitsPerPixel, false);
                    ee.AddDebugData("Image Width", image.Width, false);
                    ee.AddDebugData("Image Height", image.Height, false);
                }
                else
                {
                    ee.AddDebugData("Image Object", "NULL", true);
                }
            }
            catch (Exception ex)
            {
                // Since this is used to add debug information in an exception handler, we
                // should just log exceptions rather than rethrowing them.
                ExtractException ee2 =
                    new ExtractException("ELI28049", "Error adding image debug information.", ex);
                ee2.Log();
            }
        }

        /// <summary>
        /// Determines whether the specified format is a tagged image file format (TIFF).
        /// </summary>
        /// <param name="format">The image format to evaluate.</param>
        /// <returns><see langword="true"/> if <paramref name="format"/> is a tagged image file 
        /// format; <see langword="false"/> if the <paramref name="format"/> is not a tagged image 
        /// file format.</returns>
        // Enums don't throw exceptions
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public static bool IsTiff(RasterImageFormat format)
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
        /// Determines whether the specified format is a portable document format (PDF).
        /// </summary>
        /// <param name="format">The image format to evaluate.</param>
        /// <returns><see langword="true"/> if <paramref name="format"/> is a portable document
        /// format; <see langword="false"/> if the <paramref name="format"/> is not a portable  
        /// document format.</returns>
        // Enums don't throw exceptions
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public static bool IsPdf(RasterImageFormat format)
        {
            switch (format)
            {
                case RasterImageFormat.PdfLeadMrc:
                case RasterImageFormat.RasPdf:
                case RasterImageFormat.RasPdfCmyk:
                case RasterImageFormat.RasPdfG31Dim:
                case RasterImageFormat.RasPdfG32Dim:
                case RasterImageFormat.RasPdfG4:
                case RasterImageFormat.RasPdfJbig2:
                case RasterImageFormat.RasPdfJpeg:
                case RasterImageFormat.RasPdfJpeg411:
                case RasterImageFormat.RasPdfJpeg422:
                case RasterImageFormat.RasPdfLzw:
                case RasterImageFormat.RasPdfLzwCmyk:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the specified document is a portable document format (PDF) file.
        /// </summary>
        /// <param name="fileName">The file to check.</param>
        /// <returns><see langword="true"/> if <paramref name="fileName"/> is a portable document
        /// format; <see langword="false"/> if the <paramref name="fileName"/> is not a portable  
        /// document format.</returns>
        public static bool IsPdf(string fileName)
        {
            try
            {
                using (ImageCodecs codecs = new ImageCodecs())
                {
                    return IsPdf(fileName, codecs);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29697", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified document is a portable document format (PDF) file.
        /// </summary>
        /// <param name="fileName">The file to check.</param>
        /// <param name="codecs">The <see cref="ImageCodecs"/> to use to load the image.</param>
        /// <returns><see langword="true"/> if <paramref name="fileName"/> is a portable document
        /// format; <see langword="false"/> if the <paramref name="fileName"/> is not a portable  
        /// document format.</returns>
        public static bool IsPdf(string fileName, ImageCodecs codecs)
        {
            try
            {
                ExtractException.Assert("ELI29698", "File name cannot be null or empty.",
                    !string.IsNullOrEmpty(fileName));
                ExtractException.Assert("ELI29699", "Codecs cannot be null or empty.",
                    codecs != null);

                using (ImageReader reader = codecs.CreateReader(fileName))
                {
                    return reader.IsPdf;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29700", ex);
            }
        }
    }
}
