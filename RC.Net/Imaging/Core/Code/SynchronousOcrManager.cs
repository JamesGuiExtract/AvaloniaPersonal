using Extract;
using Extract.Drawing;
using Extract.Licensing;
using Extract.Utilities;
using Leadtools;
using Leadtools.Codecs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_SSOCRLib;
using UCLID_COMUTILSLib;

namespace Extract.Imaging
{
    /// <summary>
    /// A class to perform OCR operations in the calling thread.
    /// </summary>
    public class SynchronousOcrManager : IDisposable
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(SynchronousOcrManager).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The tradeoff setting between accuracy and speed
        /// </summary>
        OcrTradeoff _tradeoff = OcrTradeoff.Balanced;

        /// <summary>
        /// Holds an instance of the ScansoftOcrClass so that we can use the same
        /// COM object throughout the life of the <see cref="SynchronousOcrManager"/> class.
        /// </summary>
        private ScansoftOCRClass _ssocr;

        /// <summary>
        /// Holds an instance of the RasterCodecs so that we can use the same instance throughout 
        /// the life of the <see cref="SynchronousOcrManager"/> class.
        /// </summary>
        RasterCodecs _codecs;

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.OcrOnClientFeature, _OBJECT_NAME);

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new instance of the <see cref="SynchronousOcrManager"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronousOcrManager"/> class.
        /// </summary>
        public SynchronousOcrManager()
            : this(OcrTradeoff.Balanced)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronousOcrManager"/> class with the
        /// specified speed-accuracy tradeoff.
        /// </summary>
        public SynchronousOcrManager(OcrTradeoff tradeoff)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI24040");

                // Set the tradeoff
                _tradeoff = tradeoff;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24041", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the speed-accuracy tradeoff.
        /// </summary>
        /// <value>The speed-accuracy tradeoff.</value>
        /// <returns>The speed-accuracy tradeoff.</returns>
        public OcrTradeoff Tradeoff
        {
            get
            {
                return _tradeoff;
            }
            set
            {
                _tradeoff = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <overloads>Gets the OCR output data as a <see cref="SpatialString"/>.</overloads>
        /// <summary>
        /// Gets the OCR output data as a <see cref="SpatialString"/>.
        /// </summary>
        /// <param name="fileName">The file on which text recognition is needed.</param>
        /// <param name="rasterZones">The raster zones whose text should be recognized.</param>
        /// <param name="thresholdAngle">If less than zero, the smallest
        /// <see cref="Rectangle"/> containing each <see cref="RasterZone"/> is OCR'd (which may 
        /// contain significantly more area than the actual raster zone if the raster zone is at 
        /// a steep angle). Otherwise, the smallest <see cref="Rectangle"/> containing each 
        /// <see cref="RasterZone"/> is OCR'd for <see cref="RasterZone"/>s at whose angle is 
        /// less than the specified angle. For <see cref="RasterZone"/>s at a steeper angle
        /// than specified, the angle of the swipe is taken into account and only the text within
        /// the <see cref="RasterZone"/> itself is OCR'd.</param>
        /// <returns>A <see cref="SpatialString"/> containing the OCR output.</returns>
        [CLSCompliant(false)]
        public SpatialString GetOcrText(string fileName, IEnumerable<RasterZone> rasterZones,
            double thresholdAngle)
        {
            try
            {
                SpatialString ocrText = null;

                foreach (RasterZone rasterZone in rasterZones)
                {
                    if (ocrText == null)
                    {
                        ocrText = GetOcrText(fileName, rasterZone, thresholdAngle);
                    }
                    else
                    {
                        ocrText.Append(GetOcrText(fileName, rasterZone, thresholdAngle));
                    }
                }

                return ocrText;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24042", ex);
            }
        }

        /// <summary>
        /// Gets the OCR output data as a <see cref="SpatialString"/>.
        /// </summary>
        /// <param name="fileName">The file on which text recognition is needed.</param>
        /// <param name="rasterZone">The <see cref="RasterZone"/> defining the image region where
        /// text recognition is needed.</param>
        /// <param name="thresholdAngle">If less than zero, the smallest
        /// <see cref="Rectangle"/> containing the <see cref="RasterZone"/> is OCR'd (which may 
        /// contain significantly more area than the actual raster zone if the raster zone is at 
        /// a steep angle). Otherwise, the smallest <see cref="Rectangle"/> containing the 
        /// <see cref="RasterZone"/> is OCR'd for if the <see cref="RasterZone"/> is at an angle 
        /// less than the specified angle. For <see cref="RasterZone"/>s at a steeper angle
        /// than specified, the angle of the swipe is taken into account and only the text within
        /// the <see cref="RasterZone"/> itself is OCR'd.</param>
        /// <returns>A <see cref="SpatialString"/> containing the OCR output.</returns>
        [CLSCompliant(false)]
        public SpatialString GetOcrText(string fileName, RasterZone rasterZone,
            double thresholdAngle)
        {
            // Declare raster images and temporary filename outside the try scope to enable 
            // disposal via the finally block.
            RasterImage sourceImage = null;
            RasterImage rasterZoneImage = null;
            string rasterZoneImageFileName = "";

            try
            {
                // If no angle is specified, always use the smallest bounding rectangle.
                if (thresholdAngle < 0)
                {
                    return GetOcrText(fileName, rasterZone.PageNumber, rasterZone.PageNumber,
                        rasterZone.GetRectangularBounds());
                }

                // Calculate the skew of the raster zone.
                double skew = (double)GeometryMethods.GetAngle(
                        new Point(rasterZone.StartX, rasterZone.StartY),
                        new Point(rasterZone.EndX, rasterZone.EndY));

                // Convert to degrees
                skew *= (180.0 / Math.PI);

                // If the raster zone's skew is not sufficient to require special handling, 
                // use the smallest bounding rectangle.
                if (Math.Abs(skew) <= thresholdAngle)
                {
                    return  GetOcrText(fileName, rasterZone.PageNumber, rasterZone.PageNumber,
                        rasterZone.GetRectangularBounds());
                }

                // To compensate for an angled raster zone, load the image for manipulation.
                sourceImage = this.Codecs.Load(fileName);

                // Extract the raster zone as a separate image oriented in terms of the raster
                // zone's angle.
                Size offset;
                int orientation;
                rasterZoneImage = ImageMethods.ExtractImageRasterZone(sourceImage, rasterZone,
                    out orientation, out skew, out offset);

                // Save the raster zone to a temporary file.
                rasterZoneImageFileName = FileSystemMethods.GetTemporaryFileName();
                this.Codecs.Save(rasterZoneImage, rasterZoneImageFileName, 
                    RasterImageFormat.CcittGroup4, 0, 1, -1, 1, CodecsSavePageMode.Overwrite);

                // Recognize the text from the raster zone image, then delete the temporary file.
                SpatialString ocrText = GetOcrText(rasterZoneImageFileName, 1, 1, null);

                // If OCR text was found, its spatial information needs to be converted to match
                // the location of the raster zone in the original image using the coordinate system
                // of the extracted raster zone image. 
                if (ocrText != null && ocrText.GetMode() != ESpatialStringMode.kNonSpatialMode)
                {
                    // Create a copy of the ocrText that can be used for positioning so that if
                    // positioning fails, we still have the original unmodified version.
                    ICopyableObject ipCopyThis = (ICopyableObject)ocrText;
                    SpatialString positionedOcrText = (SpatialString)ipCopyThis.Clone();

                    // If the string cannot be positioned as a true spatial string because of
                    // coordinate issues, position it as a hybrid string.
                    if (!PositionAsTrueSpatialString(ref positionedOcrText, 
                        sourceImage, orientation, skew, offset))
                    {
                        // Use the original value so that changes to the string in a failed
                        // PositionAsTrueSpatialString attempt don't affect PositionAsHybrid.
                        positionedOcrText = ocrText;

                        PositionAsHybridSpatialString(
                            ref positionedOcrText, sourceImage, orientation, skew, offset);
                    }

                    // Set the correct page for the OCR'd text.
                    positionedOcrText.UpdatePageNumber(rasterZone.PageNumber);

                    // [DataEntry:334]
                    // Use the original file name as the source doc name.
                    positionedOcrText.SourceDocName = fileName;

                    return positionedOcrText;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI24043", "Failed to recognize text!", 
                    ex);
                ee.AddDebugData("Filename", fileName, false);
                throw ee;
            }
            finally
            {
                if (!string.IsNullOrEmpty(rasterZoneImageFileName))
                {
                    FileSystemMethods.TryDeleteFile(rasterZoneImageFileName);
                }
                if (sourceImage != null)
                {
                    sourceImage.Dispose();
                }
                if (rasterZoneImage != null)
                {
                    rasterZoneImage.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the OCR output data as a <see cref="SpatialString"/> on a specified range of pages.
        /// </summary>
        /// <param name="fileName">The file on which text recognition is needed.</param>
        /// <param name="startPage">The first page on which text recognition is needed.</param>
        /// <param name="endPage">The last page on which text recognition is needed.</param>
        /// <param name="imageArea">If <see langword="null"/> the entirety of each specified page
        /// is OCR'd.  If non-null, only the area described by the <see cref="Rectangle"/> will be
        /// OCR'd on each page.</param>
        /// <returns>A <see cref="SpatialString"/> containing the OCR output.</returns>
        [CLSCompliant(false)]
        public SpatialString GetOcrText(string fileName, int startPage, int endPage, 
            Rectangle? imageArea)
        {
            SpatialString ocrText = null;

            try
            {
                // Ensure that a valid file name has been passed in.
                ExtractException.Assert("ELI24044", "Filename may not be null or empty!",
                    !string.IsNullOrEmpty(fileName));
                ExtractException.Assert("ELI24045", "File is not valid!",
                    File.Exists(fileName), "Ocr File Name", fileName);

                if (imageArea == null)
                {
                    // If imageArea has not been specified, OCR the entire area each page.
                    ocrText = this.SSOCR.RecognizeTextInImage(fileName, startPage, endPage,
                        EFilterCharacters.kNoFilter, "", (EOcrTradeOff)this._tradeoff, true, null);
                }
                else
                {
                    // If imageArea has been specified, OCR only the area within this logical area 
                    // of this rectangle on each page. [LegacyRCAndUtils:5033] TODO: This currently does 
                    // not respect this.Tradeoff.
                    LongRectangleClass zonalOCRRectangle = new LongRectangleClass();
                    zonalOCRRectangle.SetBounds(imageArea.Value.Left,
                                                imageArea.Value.Top,
                                                imageArea.Value.Right,
                                                imageArea.Value.Bottom);

                    ocrText = this.SSOCR.RecognizeTextInImageZone(fileName, startPage, endPage,
                        zonalOCRRectangle, 0, EFilterCharacters.kNoFilter, "", false, false,
                        true, null);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI24046", "Failed to recognize text!", 
                    ex);
                ee.AddDebugData("Filename", fileName, false);
                ee.AddDebugData("Start page", startPage, false);
                ee.AddDebugData("End page", endPage, false);
                throw ee;
            }

            return ocrText;
        }

        #endregion Methods

        #region IDisposable Members

        /// <overloads>Releases resources used by the <see cref="SynchronousOcrManager"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="SynchronousOcrManager"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="SynchronousOcrManager"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_codecs != null)
                {
                    _codecs.Dispose();
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Priviate Methods

        /// <summary>
        /// Gets the private license code for licensing the OCR engine.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the private license
        /// key for licensing the OCR engine.</returns>
        private static string GetSpecialOcrValue()
        {
            return LicenseUtilities.GetMapLabelValue(new MapLabel());
        }

        /// <summary>
        /// Obtains (and creates if necessary) the SSOCR an instance to be used throughout the
        /// the life of the <see cref="SynchronousOcrManager"/> class.
        /// </summary>
        private ScansoftOCRClass SSOCR
        {
            get
            {
                try
                {
                    if (_ssocr == null)
                    {
                        // Get a new ScanSoftOCR class object
                        _ssocr = new ScansoftOCRClass();

                        // Init the private license
                        _ssocr.InitPrivateLicense(GetSpecialOcrValue());
                    }

                    return _ssocr;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24017", ex);
                }
            }
        }

        /// <summary>
        /// Gets an instance of <see cref="RasterCodecs"/> to use throughout the lifetime of this
        /// <see cref="SynchronousOcrManager"/> instance.
        /// </summary>
        /// <returns>An instance of <see cref="RasterCodecs"/> to use throughout the lifetime of 
        /// this <see cref="SynchronousOcrManager"/> instance.</returns>
        private RasterCodecs Codecs
        {
            get
            {
                try
                {
                    if (_codecs == null)
                    {
                        // Initialize a new RasterCodecs object
                        _codecs = new RasterCodecs();
                        _codecs.Options.Tiff.Load.IgnoreViewPerspective = true;
                        _codecs.Options.Pdf.Save.UseImageResolution = true;

                        // Leadtools does not support anti-aliasing for non-bitonal pdfs.
                        // If anti-aliasing is set, load pdfs as a bitonal image with high dpi.
                        if (!RasterSupport.IsLocked(RasterSupportType.Bitonal))
                        {
                            // Load as bitonal for anti-aliasing
                            _codecs.Options.Pdf.Load.DisplayDepth = 1;

                            // Use high dpi to preserve image quality
                            _codecs.Options.Pdf.Load.XResolution = 300;
                            _codecs.Options.Pdf.Load.YResolution = 300;
                        }
                    }

                    return _codecs;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24095", ex);
                }
            }
        }

        /// <summary>
        /// Converts the provided <see cref="SpatialString"/> to hybrid mode and positions it
        /// into <see paramref="sourceImage"/>'s coordinate system using the specified
        /// <see paramref="orientation"/>, <see paramref="skew"/> and <see paramref="offset"/>
        /// parameters.
        /// </summary>
        /// <param name="spatialString">The <see cref="SpatialString"/> to be converted to hybrid
        /// and positioned.</param>
        /// <param name="sourceImage">The <see cref="RasterImage"/> in which the string is being
        /// positioned.</param>
        /// <param name="orientation">A value indicating the image orientation that comes closest
        /// to allowing the spatialString to be vertical.
        /// <list type="bullet">
        /// <item><b>0</b>: Right side up.</item>
        /// <item><b>90</b>: Rotated 90 degrees to the right.</item>
        /// <item><b>180</b>: Upside down.</item>
        /// <item><b>270</b>: Rotated 90 degrees to the left.</item>
        /// </list></param>
        /// <param name="skew">The skew  angle (in degrees) that should be applied to the
        /// spatialString.</param>
        /// <param name="offset">The amount spatialString needs to be offset to be placed into
        /// sourceImage's coordinate system.</param>
        private static void PositionAsHybridSpatialString(ref SpatialString spatialString,
            RasterImage sourceImage, int orientation, double skew, Size offset)
        {
            SpatialPageInfo pageInfo = spatialString.GetPageInfo(1);

            // Create a matrix to translate the string's raster zone coordinates into the
            // coordinate system of sourceImage.
            using (Matrix transform = new Matrix())
            {
                // Apply the skew by rotating the coordinates about the center of the image.
                PointF imageCenter =
                    new PointF(sourceImage.Width / 2, sourceImage.Height / 2);
                transform.RotateAt((float)skew, imageCenter);

                // Shift the coordinates as necessary to account for a different corner of the
                // image being used as the base of the coordinate system in a rotated
                // orientation.
                switch (orientation)
                {
                    case 0: break;
                    case 90: transform.Translate(sourceImage.Width, 0); break;
                    case 180: transform.Translate(sourceImage.Width, sourceImage.Height); break;
                    case 270: transform.Translate(0, sourceImage.Height); break;
                    default:
                        {
                            ExtractException ee = new ExtractException("ELI25974", "Invalid parameter!");
                            ee.AddDebugData("Orientation", orientation, false);
                            throw ee;
                        }
                }

                // Rotation necessary to rotate the string's coordinate system orientation into that
                // of the page.
                transform.Rotate(orientation);

                // Finally apply the specified offset.  At this point any specified spatialString
                // coordinates will be aligned with sourceImage.
                transform.Translate(offset.Width, offset.Height);

                // Loop through all raster zones to update the coordinates using the transform
                // matrix.
                IUnknownVector rasterZones = spatialString.GetOriginalImageRasterZones();
                int count = rasterZones.Size();
                for (int i = 0; i < count; i++)
                {
                    UCLID_RASTERANDOCRMGMTLib.RasterZone rasterZone =
                        (UCLID_RASTERANDOCRMGMTLib.RasterZone)rasterZones.At(i);

                    Point[] rasterZonePoints = {
                                    new Point(rasterZone.StartX, rasterZone.StartY),
                                    new Point(rasterZone.EndX, rasterZone.EndY) };

                    // Move the points into the sourceImage's coordinate system.
                    transform.TransformPoints(rasterZonePoints);

                    // Apply the converted points.[
                    rasterZone.StartX = rasterZonePoints[0].X;
                    rasterZone.StartY = rasterZonePoints[0].Y;
                    rasterZone.EndX = rasterZonePoints[1].X;
                    rasterZone.EndY = rasterZonePoints[1].Y;
                }

                // Adjust the SpatialPageInfo to reflect the size of the source image
                pageInfo.SetPageInfo(sourceImage.Width, sourceImage.Height, EOrientation.kRotNone, 0);

                // Rebuild spatialString as a hybrid string using the converted raster zones.
                spatialString.CreateHybridString(rasterZones, spatialString.String,
                    spatialString.SourceDocName, spatialString.SpatialPageInfos);
            }
        }

        /// <summary>
        /// Positions <see cref="SpatialString"/> into <see paramref="sourceImage"/>'s coordinate
        /// system using the specified <see paramref="orientation"/>, <see paramref="skew"/> and
        /// <see paramref="offset"/> parameters.
        /// </summary>
        /// <param name="spatialString">The <see cref="SpatialString"/> to be converted to hybrid
        /// and positioned.</param>
        /// <param name="sourceImage">The <see cref="RasterImage"/> in which the string is being
        /// positioned.</param>
        /// <param name="orientation">A value indicating the image orientation that comes closest
        /// to allowing the spatialString to be vertical.
        /// <list type="bullet">
        /// <item><b>0</b>: Right side up.</item>
        /// <item><b>90</b>: Rotated 90 degrees to the right.</item>
        /// <item><b>180</b>: Upside down.</item>
        /// <item><b>270</b>: Rotated 90 degrees to the left.</item>
        /// </list></param>
        /// <param name="skew">The skew angle (in degrees) that should be applied to the
        /// spatialString.</param>
        /// <param name="offset">The amount spatialString needs to be offset to be placed into
        /// sourceImage's coordinate system.</param>
        /// <returns><see langword="true"/> if the string was properly positioned as a true spatial
        /// string, <see langword="false"/> if the string was not able to be positioned as a true
        /// spatial string.</returns>
        private static bool PositionAsTrueSpatialString(ref SpatialString spatialString,
            RasterImage sourceImage, int orientation, double skew, Size offset)
        {
            // [DataEntry:144]
            // Test to see if the OCR coordinates of the spatial string will be negative in
            // either dimension after applying the offset. If so, the spatial string cannot
            // be represented in a true spatial mode. In that case, convert it into a hybrid
            // string that will not need a deskew value and, thus, not need negative
            // coordinates to be represented.
            ILongRectangle bounds = spatialString.GetOriginalImageBounds();
            if (bounds.Left + offset.Width < 0 || bounds.Top + offset.Height < 0)
            {
                return false;
            }

            // Adjust the SpatialPageInfo to reflect the size of the source image
            SpatialPageInfo pageInfo = spatialString.GetPageInfo(1);
            pageInfo.Width = sourceImage.Width;
            pageInfo.Height = sourceImage.Height;

            // Get the deskew of the OCR'd text within the extracted raster zone image area
            // before updating the deskew.
            double ocrDeskew = pageInfo.Deskew;
            
            // Apply deskew to the text's page info so that the OCR text's spatial area will
            // be depicted in terms of the skew of the original swipe.
            pageInfo.Deskew = skew;

            // Apply the specified page orientation to the page info.
            switch (orientation)
            {
                case 0:
                    {
                        pageInfo.SetPageInfo(
                            sourceImage.Width, sourceImage.Height, EOrientation.kRotNone, skew);
                    }
                    break;
                case 90:
                    {
                        pageInfo.SetPageInfo(
                            sourceImage.Width, sourceImage.Height, EOrientation.kRotLeft, skew);
                    }
                    break;
                case 180:
                    {
                        pageInfo.SetPageInfo(
                            sourceImage.Width, sourceImage.Height, EOrientation.kRotDown, skew);
                    }
                    break;
                case 270:
                    {
                        pageInfo.SetPageInfo(
                            sourceImage.Width, sourceImage.Height, EOrientation.kRotRight, skew);
                    }
                    break;
                default:
                    {
                        ExtractException ee = new ExtractException("ELI25975", "Invalid parameter!");
                        ee.AddDebugData("Orientation", orientation, false);
                        throw ee;
                    }
            }

            // Convert the coordinates of the OCR'd text so that the text appears at the
            // correct location in the source image.
            spatialString.Offset(offset.Width, offset.Height);

            // Create a transform matrix to adjust for difference between the angle of the 
            // source raster zone and the deskew angle of the text with the raster zone (if there
            // is any such deskew).
            if (ocrDeskew != 0)
            {
                using (Matrix transform = new Matrix())
                {
                    // Obtain the center point of the spatial string.
                    bounds = spatialString.GetOriginalImageBounds();
                    int left = bounds.Left;
                    int top = bounds.Top;
                    Point resultCenterPoint = new Point(left + (bounds.Right - left) / 2,
                                                        top + (bounds.Bottom - top) / 2);

                    // The final spatial string will need to take both the swipe's angle into
                    // account as well as the deskew of the text within that swipe. Obtain the
                    // coordinates of the center point in a coordinate system that take both into
                    // account.
                    Point[] transformedResultCenterPoint = { resultCenterPoint };
                    PointF imageCenterPoint = new PointF(pageInfo.Width / 2, pageInfo.Height / 2);
                    transform.RotateAt((float)-(skew + ocrDeskew + orientation), imageCenterPoint);
                    transform.TransformPoints(transformedResultCenterPoint);
                    resultCenterPoint = transformedResultCenterPoint[0];

                    // Now rotate the starting point by the amount of deskew of the text within the
                    // raster zone to determine the offset adjustment needed once the adjusted
                    // deskew is applied to the page info object.
                    transform.Reset();
                    transform.RotateAt((float)ocrDeskew, imageCenterPoint);
                    transform.TransformPoints(transformedResultCenterPoint);

                    offset.Width = resultCenterPoint.X - transformedResultCenterPoint[0].X;
                    offset.Height = resultCenterPoint.Y - transformedResultCenterPoint[0].Y;

                    bounds = spatialString.GetOCRImageBounds();

                    // If the offset require will result in negative image coordinates, return false
                    // to indicate the result cannot be represented with a true spatial string.
                    if ((bounds.Left + offset.Width < 0) || (bounds.Top + offset.Height < 0))
                    {
                        return false;
                    }

                    // Apply the offset and updated deskew
                    spatialString.Offset(offset.Width, offset.Height);
                    pageInfo.Deskew += ocrDeskew;
                }
            }

            return true;
        }

        #endregion Private Methods
    }
}
