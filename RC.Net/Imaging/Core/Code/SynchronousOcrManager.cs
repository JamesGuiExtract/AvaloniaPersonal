using Extract.Drawing;
using Extract.Licensing;
using Extract.Utilities;
using Leadtools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_SSOCRLib;
using UCLID_COMUTILSLib;

using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;

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
        static readonly string _OBJECT_NAME = typeof(SynchronousOcrManager).ToString();

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
        ScansoftOCRClass _ssocr;

        /// <summary>
        /// Holds an instance of the <see cref="ImageCodecs"/> so that we can use the same 
        /// instance throughout the life of the <see cref="SynchronousOcrManager"/> class.
        /// </summary>
        ImageCodecs _codecs;

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
                LicenseUtilities.ValidateLicense(LicenseIdName.OcrOnClientFeature, "ELI24040",
					_OBJECT_NAME);

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
                        SpatialString temp = GetOcrText(fileName, rasterZone, thresholdAngle);
                        if (temp != null)
                        {
                            ocrText.Append(temp);
                        }
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
            return GetOcrText(fileName, rasterZone, thresholdAngle, null);
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
        /// <param name="bounds">If not <see langword="null"/> and the skew does not
        /// require special handling (less than <paramref name="thresholdAngle"/>
        /// or <paramref name="thresholdAngle"/> is less than 0) then the bounding
        /// rectangle of the <paramref name="rasterZone"/> will be restricted by this
        /// <see cref="Rectangle"/>.</param>
        /// <returns>A <see cref="SpatialString"/> containing the OCR output.</returns>
        [CLSCompliant(false)]
        public SpatialString GetOcrText(string fileName, RasterZone rasterZone,
            double thresholdAngle, Rectangle? bounds)
        {
            // Declare raster images outside the try scope to enable disposal via the finally block.
            RasterImage page = null;
            RasterImage rasterZoneImage = null;
            TemporaryFile tempFile = null;

            try
            {
                // If no angle is specified, always use the smallest bounding rectangle.
                // OR
                // If the raster zone's skew is not sufficient to require special handling, 
                // use the smallest bounding rectangle.
                if (thresholdAngle < 0
                    || Math.Abs(rasterZone.ComputeSkew(true)) <= thresholdAngle)
                {
                    // Get the bounding rectangle for the zone
                    Rectangle zoneBounds = rasterZone.GetRectangularBounds();

                    // Ensure the bounding rectangle is contained within the bounds
                    // specified (if they were specified)
                    if (bounds.HasValue)
                    {
                        zoneBounds.Intersect(bounds.Value);
                    }

                    return GetOcrText(fileName, rasterZone.PageNumber, rasterZone.PageNumber,
                        zoneBounds);
                }

                // To compensate for an angled raster zone, load the image for manipulation.
                using (ImageReader reader = Codecs.CreateReader(fileName))
                {
                    page = reader.ReadPage(rasterZone.PageNumber);
                }

                // Extract the raster zone as a separate image oriented in terms of the raster
                // zone's angle.
                Size offset;
                int orientation;
                double skew;
                rasterZoneImage = ImageMethods.ExtractZoneFromPage(rasterZone, page, 
                    out orientation, out skew, out offset);

                // Save the raster zone to a temporary file.
                tempFile = new TemporaryFile();
                using (ImageWriter writer = 
                    Codecs.CreateWriter(tempFile.FileName, rasterZoneImage.OriginalFormat))
                {
                    writer.AppendImage(rasterZoneImage);
                    writer.Commit(true);
                }

                // Recognize the text from the raster zone image, then delete the temporary file.
                SpatialString ocrText = GetOcrText(tempFile.FileName, 1, 1, null);

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
                        page, orientation, skew, offset))
                    {
                        // Use the original value so that changes to the string in a failed
                        // PositionAsTrueSpatialString attempt don't affect PositionAsHybrid.
                        positionedOcrText = ocrText;

                        PositionAsHybridSpatialString(
                            ref positionedOcrText, page, orientation, skew, offset);
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
                if (tempFile != null)
                {
                    tempFile.Dispose();
                }
                if (page != null)
                {
                    page.Dispose();
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
            try
            {
                // Ensure that a valid file name has been passed in.
                ExtractException.Assert("ELI24044", "Filename may not be null or empty!",
                    !string.IsNullOrEmpty(fileName));
                ExtractException.Assert("ELI24045", "File is not valid!",
                    File.Exists(fileName), "Ocr File Name", fileName);

                SpatialString ocrText;
                if (imageArea == null)
                {
                    // If imageArea has not been specified, OCR the entire area each page.
                    ocrText = SSOCR.RecognizeTextInImage(fileName, startPage, endPage,
                        EFilterCharacters.kNoFilter, "", (EOcrTradeOff)_tradeoff, true, null);
                }
                else
                {
                    // If imageArea has been specified, OCR only the area within this logical area 
                    // of this rectangle on each page. [LegacyRCAndUtils:5033] 
                    // TODO: This currently does not respect this.Tradeoff.
                    LongRectangleClass zonalOcrRectangle = new LongRectangleClass();
                    zonalOcrRectangle.SetBounds(imageArea.Value.Left,
                                                imageArea.Value.Top,
                                                imageArea.Value.Right,
                                                imageArea.Value.Bottom);

                    ocrText = SSOCR.RecognizeTextInImageZone(fileName, startPage, endPage,
                        zonalOcrRectangle, 0, EFilterCharacters.kNoFilter, "", false, false,
                        true, null);
                }

                return ocrText;
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

        }

        /// <summary>
        /// Gets the OCR output data as a <see cref="System.String"/>.
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
        /// <param name="formatAsXml">If <see langword="true"/> then the returned text
        /// will be formatted as an XML string containing each line broken down with zonal
        /// coordinates and the text, otherwise just returns the OCR'd text.</param>
        /// <param name="bounds">If not <see langword="null"/> and the skew does not
        /// require special handling (less than <paramref name="thresholdAngle"/>
        /// or <paramref name="thresholdAngle"/> is less than 0) then the bounding
        /// rectangle of the <paramref name="rasterZone"/> will be restricted by this
        /// <see cref="Rectangle"/>.</param>
        /// <returns>A <see cref="System.String"/> containing the OCR output.</returns>
        public string GetOcrTextAsString(string fileName, RasterZone rasterZone,
            double thresholdAngle, bool formatAsXml, Rectangle? bounds)
        {
            try
            {
                SpatialString temp = GetOcrText(fileName, rasterZone, thresholdAngle,
                    bounds);
                if (temp != null)
                {
                    if (formatAsXml)
                    {
                        return ConvertOcrTextToXmlString(temp);
                    }
                    else
                    {
                        return temp.String;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI30121",
                    "Failed to recognize text.", ex);
                ee.AddDebugData("Filename", fileName, false);
                ee.AddDebugData("Raster Zone", rasterZone.ToString(), false);
                throw ee;
            }
        }

        /// <summary>
        /// Converts the OCR result string into an XML formatted string. 
        /// </summary>
        /// <param name="spatialString">The string to convert to XML format.</param>
        /// <returns>An XML string for the ocr result.</returns>
        static string ConvertOcrTextToXmlString(SpatialString spatialString)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.ASCII;
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;
                using (XmlWriter writer = XmlWriter.Create(sb, settings))
                {
                    writer.WriteStartElement("OcrResults");

                    // Check for spatial info
                    if (spatialString.HasSpatialInfo())
                    {
                        // Get the full text and average confidence for the whole string
                        string text = spatialString.String;
                        int max = 0, min = 0, averageConfidence = 0;
                        spatialString.GetCharConfidence(ref max, ref min, ref averageConfidence);

                        // Write the full text element
                        writer.WriteStartElement("FullText");
                        writer.WriteAttributeString("AverageCharConfidence",
                            averageConfidence.ToString(CultureInfo.CurrentCulture));
                        writer.WriteString(text);
                        writer.WriteEndElement(); // Ends the full text element

                        IIUnknownVector lines = spatialString.GetLines();
                        int count = lines.Size();
                        for (int i = 0; i < count; i++)
                        {
                            SpatialString line = (SpatialString) lines.At(i);

                            // Only write the spatial lines
                            if (line.HasSpatialInfo())
                            {
                                AddSpatialLineXmlInformation(writer, line);
                            }
                        }
                    }
                    else
                    {
                        // Non-spatial, just write the full text element
                        writer.WriteElementString("FullText", spatialString.String);
                    }

                    writer.WriteEndElement();
                    writer.Flush();
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30241", ex);
            }
        }

        /// <summary>
        /// Writes the spatial line text, zonal, and bounds information for the
        /// spatial string line to the specified <see cref="XmlWriter"/>
        /// </summary>
        /// <param name="writer">The writer to write the data to.</param>
        /// <param name="line">The spatial line to write to XML.</param>
        static void AddSpatialLineXmlInformation(XmlWriter writer, SpatialString line)
        {
            try
            {
                // Get the page number, text, character confidence, zones, and bounds from the line
                int pageNumber = line.GetFirstPageNumber();
                string text = line.String;
                int max = 0, min = 0, averageConfidence = 0;
                line.GetCharConfidence(ref max, ref min, ref averageConfidence);
                IIUnknownVector zones = line.GetOriginalImageRasterZones();
                LongRectangle rect = line.GetOriginalImageBounds();

                // Get the bounds from the rectangle
                int top, left, right, bottom;
                rect.GetBounds(out left, out top, out right, out bottom);

                writer.WriteStartElement("SpatialLine");
                writer.WriteAttributeString("PageNumber", pageNumber.ToString(CultureInfo.CurrentCulture));

                // Write the line text
                writer.WriteStartElement("LineText");
                writer.WriteAttributeString("AverageCharConfidence", averageConfidence.ToString(CultureInfo.CurrentCulture));
                writer.WriteString(text);
                writer.WriteEndElement(); // Ends the LineText element

                // Write each zone
                int count = zones.Size();
                for (int i = 0; i < count; i++)
                {
                    ComRasterZone zone = (ComRasterZone) zones.At(i);
                    int startX = 0, startY = 0, endX = 0, endY = 0, height = 0, page = 0;
                    zone.GetData(ref startX, ref startY, ref endX, ref endY, ref height, ref page);
                    writer.WriteStartElement("SpatialLineZone");
                    writer.WriteAttributeString("StartX", startX.ToString(CultureInfo.CurrentCulture));
                    writer.WriteAttributeString("StartY", startY.ToString(CultureInfo.CurrentCulture));
                    writer.WriteAttributeString("EndX", endX.ToString(CultureInfo.CurrentCulture));
                    writer.WriteAttributeString("EndY", endY.ToString(CultureInfo.CurrentCulture));
                    writer.WriteAttributeString("Height", height.ToString(CultureInfo.CurrentCulture));
                    writer.WriteEndElement(); // Ends the zone element
                }

                // Write the bounds
                writer.WriteStartElement("SpatialLineBounds");
                writer.WriteAttributeString("Top", top.ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("Left", left.ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("Bottom", bottom.ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("Right", right.ToString(CultureInfo.CurrentCulture));
                writer.WriteEndElement(); // Ends the bounds element

                writer.WriteEndElement(); // Ends the spatial line element
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30240", ex);
            }
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
                    _codecs = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Private Methods

        /// <summary>
        /// Gets the private license code for licensing the OCR engine.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the license
        /// key for licensing the OCR engine.</returns>
        static string GetSpecialOcrValue()
        {
            return LicenseUtilities.GetMapLabelValue(new MapLabel());
        }

        /// <summary>
        /// Obtains (and creates if necessary) the SSOCR an instance to be used throughout the
        /// the life of the <see cref="SynchronousOcrManager"/> class.
        /// </summary>
        ScansoftOCRClass SSOCR
        {
            get
            {
                try
                {
                    if (_ssocr == null)
                    {
                        // Get a new ScanSoftOCR class object
                        _ssocr = new ScansoftOCRClass();

                        // Init the license
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
        /// Gets an instance of <see cref="ImageCodecs"/> to use throughout the lifetime of this
        /// <see cref="SynchronousOcrManager"/> instance.
        /// </summary>
        /// <returns>An instance of <see cref="ImageCodecs"/> to use throughout the lifetime of 
        /// this <see cref="SynchronousOcrManager"/> instance.</returns>
        ImageCodecs Codecs
        {
            get
            {
                try
                {
                    if (_codecs == null)
                    {
                        _codecs = new ImageCodecs();
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
        static void PositionAsHybridSpatialString(ref SpatialString spatialString,
            RasterImage sourceImage, int orientation, double skew, Size offset)
        {
            SpatialPageInfo pageInfo = spatialString.GetPageInfo(1);

            // Create a matrix to translate the string's raster zone coordinates into the
            // coordinate system of sourceImage.
            using (Matrix transform = new Matrix())
            {
                // Apply the skew by rotating the coordinates about the center of the image.
                PointF imageCenter =
                    new PointF(sourceImage.Width / 2F, sourceImage.Height / 2F);
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
        static bool PositionAsTrueSpatialString(ref SpatialString spatialString,
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
                    PointF imageCenterPoint = new PointF(pageInfo.Width / 2F, pageInfo.Height / 2F);
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
