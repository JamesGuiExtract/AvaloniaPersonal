using Extract.Drawing;
using Extract.Licensing;
using Leadtools;
using Leadtools.Codecs;
using Leadtools.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace Extract.Imaging
{
    /// <summary>
    /// Contains image manipulation methods.
    /// </summary>
    public static class ImageMethods
    {
        #region Fields

        /// <summary>
        /// License cache object used to validate the license
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, typeof(ImageMethods).ToString());

        #endregion Fields

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
                _licenseCache.Validate("ELI28012");

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
                    thumbnail = null;
                }

                throw ExtractException.AsExtractException("ELI23773", ex);
            }
        }

        /// <summary>
        /// Extracts into a separate <see cref="RasterImage"/> the specified <see cref="RasterZone"/>
        /// The extracted image will be oriented with the <see cref="RasterZone"/> so that the start
        /// and end point lie along a horizontal line that bisects the resulting image.
        /// </summary>
        /// <param name="image">The <see cref="RasterImage"/> from which the <see cref="RasterZone"/> 
        /// is to be extracted.</param>
        /// <param name="rasterZone">The <see cref="RasterZone"/> to be extracted as a separate 
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
        public static RasterImage ExtractImageRasterZone(RasterImage image,
            RasterZone rasterZone, out int orientation, out double skew, out Size offset)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI28013");

                // Move to the raster zone's page.
                image.Page = rasterZone.PageNumber;

                // Calculate the skew of the raster zone.
                skew = (double)GeometryMethods.GetAngle(
                        new Point(rasterZone.StartX, rasterZone.StartY),
                        new Point(rasterZone.EndX, rasterZone.EndY));

                // Convert to degrees
                skew *= (180.0 / Math.PI);

                // Calculate whether the start and end points of the raster zone fall within the
                // image page.  If either are off the image page, find a start and end point
                // along the raster zone's plane that are within the image to work with.
                Rectangle pageArea = new Rectangle(0, 0, image.Width, image.Height);
                Point startPoint = new Point(rasterZone.StartX, rasterZone.StartY);
                Point endPoint = new Point(rasterZone.EndX, rasterZone.EndY);

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
                    startPoint, endPoint, rasterZone.Height);

                // Create a destination image where the raster zone content will be copied to.  It is
                // important that it is the same size as the bounding rectangle since the raster zone
                // is centered in the bounding rectangle and we will be rotating the destination 
                // raster zone about it's center.
                RasterImage rasterZoneImage = new RasterImage(RasterMemoryFlags.Conventional, 
                    boundingRectangle.Width, boundingRectangle.Height, image.BitsPerPixel, 
                    RasterByteOrder.Rgb, image.ViewPerspective, image.GetPalette(), IntPtr.Zero, 0);

                // Initialize the destination image to all white.
                FillCommand fillCommand = new FillCommand();
                fillCommand.Color = new RasterColor(Color.White);
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
                combineCommand.DestinationImage = rasterZoneImage;
                combineCommand.Run(image);

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
                int finalImageAreaWidth = (int) Math.Sqrt(
                    Math.Pow(endPoint.X - startPoint.X, 2) + 
                    Math.Pow(endPoint.Y - startPoint.Y, 2));
                int xOffset = (rasterZoneImage.Width - finalImageAreaWidth) / 2;
                int yOffset = (rasterZoneImage.Height - rasterZone.Height) / 2;

                Rectangle finalImageArea = new Rectangle(xOffset, yOffset, finalImageAreaWidth,
                    rasterZone.Height);

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
                        case 90: transform.Translate(-image.Width, 0); break;
                        case 180: transform.Translate(-image.Width, -image.Height); break;
                        case 270: transform.Translate(0, -image.Height); break;
                    }

                    // Account for the skew by rotating about the center of the image. At this point
                    // all steps nessary to move a point from the original image's coordinate system
                    // into the coordinate system of the extracted image have been completed.
                    PointF imageCenter = new PointF((float)image.Width / 2, (float)image.Height / 2);
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
                ExtractException ee = new ExtractException("ELI24087",
                    "Failed to extract raster zone image!", ex);
                throw ee;
            }
        }

        /// <overloads>Gets the total count of pages from the specified image file.</overloads>
        /// <summary>
        /// Gets the total count of pages from the specified image file.
        /// <para><b>Require:</b></para>
        /// This method requires that RasterCodecs.Startup() has been called.
        /// </summary>
        /// <param name="fileName">The name of the image file to get the page count for.</param>
        /// <returns>The page count for the specified image.</returns>
        public static int GetImagePageCount(string fileName)
        {
            try
            {
                using (RasterCodecs codecs = new RasterCodecs())
                {
                    return GetImagePageCount(fileName, codecs);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27967", ex);
            }
        }

        /// <summary>
        /// Gets the total count of pages from the specified image file.
        /// </summary>
        /// <param name="fileName">The name of the image file to get the page count for.</param>
        /// <param name="codecs">The raster codecs to use to get the page count.</param>
        /// <returns>The page count for the specified image.</returns>
        public static int GetImagePageCount(string fileName, RasterCodecs codecs)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI28014");

                using (CodecsImageInfo info = codecs.GetInformation(fileName, true))
                {
                    return info.TotalPages;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27968", ex);
            }
        }
    }
}
