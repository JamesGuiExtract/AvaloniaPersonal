using Extract.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents the color that a redaction should be printed with.
    /// </summary>
    public enum RedactionColor
    {
        /// <summary>
        /// Redaction will be printed as <see cref="Color.Black"/>.
        /// </summary>
        Black,

        /// <summary>
        /// Redaction will be printed as <see cref="Color.White"/> with
        /// a <see cref="Color.Black"/> border.
        /// </summary>
        White
    }

    /// <summary>
    /// Represents a redaction that may span multiple lines.
    /// </summary>
    [CLSCompliant(false)]
    public class Redaction : CompositeHighlightLayerObject, IComparable<Redaction>
    {
        #region Constants

        /// <summary>
        /// The color that black redactions will be painted on the screen.
        /// </summary>
        internal static readonly Color BlackPaint = Color.DimGray;

        /// <summary>
        /// The color that white redactions will be painted on the screen.
        /// </summary>
        internal static readonly Color WhitePaint = Color.LightGray;
        
        /// <summary>
        /// The color that black redactions will be rendered on a hard copy.
        /// </summary>
        static readonly Color _BLACK_RENDER = Color.Black;

        /// <summary>
        /// The color that white redactions will be rendered on a hard copy.
        /// </summary>
        static readonly Color _WHITE_RENDER = Color.White;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The color that will be used to render a redaction when printing
        /// or saving it to an image file.
        /// </summary>
        RedactionColor _fillColor;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Redaction"/> class.
        /// </summary>
        protected Redaction()
        {
            // Needed for serialization
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Redaction"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the <see cref="Redaction"/> 
        /// appears.</param>
        /// <param name="pageNumber">The one-based page number where this redaction object
        /// is found.</param>
        /// <param name="tags">The collection of tags to add to this object.</param>
        /// <param name="rasterZones">The collection of raster zones that the 
        /// <see cref="Redaction"/> covers. All <see cref="RasterZone"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.
        /// <para><b>Note:</b></para>
        /// If this is <see langword="null"/> then will create a redaction with an empty
        /// zone collection.</param>
        /// <exception cref="ExtractException">If any <see cref="RasterZone.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        public Redaction(IDocumentViewer imageViewer, int pageNumber, IEnumerable<string> tags,
            IEnumerable<RasterZone> rasterZones)
            : this(imageViewer, pageNumber, tags, rasterZones,
            imageViewer.DefaultRedactionFillColor)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Redaction"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the <see cref="Redaction"/> 
        /// appears.</param>
        /// <param name="pageNumber">The one-based page number where this redaction object
        /// is found.</param>
        /// <param name="tags">The collection of tags to add to this object.</param>
        /// <param name="comment">The method by which the <see cref="Redaction"/> was created.
        /// </param>
        /// <param name="rasterZones">The collection of raster zones that the 
        /// <see cref="Redaction"/> covers. All <see cref="RasterZone"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.
        /// <para><b>Note:</b></para>
        /// If this is <see langword="null"/> then will create a redaction with an empty
        /// zone collection.</param>
        /// <exception cref="ExtractException">If any <see cref="RasterZone.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        public Redaction(IDocumentViewer imageViewer, int pageNumber,IEnumerable<string> tags,
            string comment, IEnumerable<RasterZone> rasterZones)
            : this(imageViewer, pageNumber, tags, comment, rasterZones,
            imageViewer.DefaultRedactionFillColor)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Redaction"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the <see cref="Redaction"/> 
        /// appears.</param>
        /// <param name="pageNumber">The one-based page number where this redaction object
        /// is found.</param>
        /// <param name="comment">The method by which the <see cref="Redaction"/> was created.
        /// </param>
        /// <param name="rasterZones">The collection of raster zones that the 
        /// <see cref="Redaction"/> covers. All <see cref="RasterZone"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.
        /// <para><b>Note:</b></para>
        /// If this is <see langword="null"/> then will create a redaction with an empty
        /// zone collection.</param>
        /// <param name="fillColor">The <see cref="Color"/> that will be used when
        /// the <see cref="Redaction"/> is printed or saved to an image file. This
        /// may only be <see cref="Color.Black"/> or <see cref="Color.White"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="RasterZone.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        /// <exception cref="ExtractException">If <paramref name="fillColor"/> is
        /// not either <see cref="Color.Black"/> or <see cref="Color.White"/>.</exception>
        public Redaction(IDocumentViewer imageViewer, int pageNumber, string comment,
            IEnumerable<RasterZone> rasterZones, RedactionColor fillColor)
            : base(imageViewer, pageNumber, comment, rasterZones,
            fillColor == RedactionColor.Black ? BlackPaint : WhitePaint)
        {
            try
            {
                SetFillColor(fillColor, false);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22798", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Redaction"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the <see cref="Redaction"/> 
        /// appears.</param>
        /// <param name="pageNumber">The one-based page number where this redaction object
        /// is found.</param>
        /// <param name="tags">The collection of tags to add to this object.</param>
        /// <param name="rasterZones">The collection of raster zones that the 
        /// <see cref="Redaction"/> covers. All <see cref="RasterZone"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.
        /// <para><b>Note:</b></para>
        /// If this is <see langword="null"/> then will create a redaction with an empty
        /// zone collection.</param>
        /// <param name="fillColor">The <see cref="Color"/> that will be used when
        /// the <see cref="Redaction"/> is printed or saved to an image file. This
        /// may only be <see cref="Color.Black"/> or <see cref="Color.White"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="RasterZone.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        /// <exception cref="ExtractException">If <paramref name="fillColor"/> is
        /// not either <see cref="Color.Black"/> or <see cref="Color.White"/>.</exception>
        public Redaction(IDocumentViewer imageViewer, int pageNumber, IEnumerable<string> tags,
            IEnumerable<RasterZone> rasterZones, RedactionColor fillColor)
            : base(imageViewer, pageNumber, tags, rasterZones,
            fillColor == RedactionColor.Black ? BlackPaint : WhitePaint)
        {
            try
            {
                SetFillColor(fillColor, false);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22799", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Redaction"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the <see cref="Redaction"/> 
        /// appears.</param>
        /// <param name="pageNumber">The one-based page number where this redaction object
        /// is found.</param>
        /// <param name="tags">The collection of tags to add to this object.</param>
        /// <param name="comment">The method by which the <see cref="Redaction"/> was created.
        /// </param>
        /// <param name="rasterZones">The collection of raster zones that the 
        /// <see cref="Redaction"/> covers. All <see cref="RasterZone"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.
        /// <para><b>Note:</b></para>
        /// If this is <see langword="null"/> then will create a redaction with an empty
        /// zone collection.</param>
        /// <param name="fillColor">The <see cref="Color"/> that will be used when
        /// the <see cref="Redaction"/> is printed or saved to an image file. This
        /// may only be <see cref="Color.Black"/> or <see cref="Color.White"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="RasterZone.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        /// <exception cref="ExtractException">If <paramref name="fillColor"/> is
        /// not either <see cref="Color.Black"/> or <see cref="Color.White"/>.</exception>
        public Redaction(IDocumentViewer imageViewer, int pageNumber, IEnumerable<string> tags,
            string comment, IEnumerable<RasterZone> rasterZones, RedactionColor fillColor)
            : base(imageViewer, pageNumber, tags, comment, rasterZones,
            fillColor == RedactionColor.Black ? BlackPaint : WhitePaint)
        {
            try
            {
                SetFillColor(fillColor, false);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22800", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the fill color for this redaction object.
        /// </summary>
        /// <value>The <see cref="Color"/> to fill the redaction with.  May only be
        /// <see cref="Color.Black"/> or <see cref="Color.White"/>.</value>
        /// <returns>The <see cref="Color"/> that this redaction object will be filled with.
        /// </returns>
        /// <exception cref="ExtractException">If the specified <see cref="Color"/> is not
        /// <see cref="Color.Black"/> or <see cref="Color.White"/>.</exception>
        [Category("Appearance")]
        [Description("The color to use when printing the redaction object.")]
        public RedactionColor FillColor
        {
            get
            {
                return _fillColor;
            }
            set
            {
                try
                {
                    SetFillColor(value, true);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI22802", ex);
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Paints the layer object using the specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The graphics object on which to paint in physical (client) 
        /// coordinates.</param>
        /// <param name="clip">The area within which the <see cref="Redaction"/> should be clipped 
        /// in physical (client) coordinates.</param>
        /// <param name="transform">A 3x3 affine matrix that maps logical (image) coordinates to 
        /// destination coordinates.</param>
        public override void Paint(Graphics graphics, Region clip, Matrix transform)
        {
            try
            {
                base.Paint(graphics, clip, transform);

                if (_fillColor == RedactionColor.White)
                {
                    foreach (Highlight highlight in Objects)
                    {
                        // Draw a black border around the highlight
                        PointF[] vertices = highlight.GetVertices();
                        transform.TransformPoints(vertices);
                        graphics.DrawPolygon(Pens.Black, vertices);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22995", ex);
            }
        }

        /// <summary>
        /// Paints the <see cref="Redaction"/> the way it should appear on a hard copy.
        /// </summary>
        /// <param name="graphics">The graphics object on which to paint in logical (image) 
        /// coordinates.</param>
        /// <param name="transform">A 3x3 affine matrix that maps logical (image) coordinates to 
        /// destination coordinates.</param>
        public override void Render(Graphics graphics, Matrix transform)
        {
            try
            {
                if (CanRender)
                {
                    // Get the render color
                    Color color = _fillColor == RedactionColor.White ? 
                        _WHITE_RENDER : _BLACK_RENDER;

                    foreach (Highlight highlight in Objects)
                    {
                        // Draw the highlight's region as an opaque color [IDSO #149]
                        highlight.DrawRegion(graphics, graphics.Clip, transform, color,
                            RasterDrawMode.CopyPen);

                        // Draw a black border around the highlight if the fill color is white
                        if (_fillColor == RedactionColor.White)
                        {
                            PointF[] vertices = highlight.GetVertices();
                            transform.TransformPoints(vertices);
                            graphics.DrawPolygon(Pens.Black, vertices);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22996", ex);
            }
        }

        /// <summary>
        /// Draws the selection border on the specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The graphics with which to draw. Cannot be 
        ///   <see langword="null"/>.</param>
        /// <param name="drawGripPoints"><see langword="true"/> if grip points should be drawn; 
        ///   <see langword="false"/> if grip points should not be drawn.</param>
        public override void DrawSelection(Graphics graphics, bool drawGripPoints)
        {
            try
            {
                // [IDSD:266]
                // If the fill color is white, we need to "erase" the black outline before the dashed
                // outline is drawn, otherwise the dashed line can't be seen.
                if (_fillColor == RedactionColor.White)
                {
                    // Do nothing if not on the active page
                    if (PageNumber != base.ImageViewer.PageNumber)
                    {
                        return;
                    }

                    // For each highlight in the redaction...
                    foreach (Highlight highlight in Objects)
                    {
                        // "Erase" the black outline by drawing it white so a black dashed line can be
                        // seen on top of it.
                        PointF[] vertices = highlight.GetVertices();
                        ImageViewer.Transform.TransformPoints(vertices);
                        graphics.DrawPolygon(Pens.White, vertices);
                    }
                }

                // Now the base class to proceed with DrawGripHandles as it normally would.
                base.DrawSelection(graphics, drawGripPoints);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26564", ex);
            }
        }

        /// <summary>
        /// Sets the fill color of the <see cref="Redaction"/>.
        /// </summary>
        /// <param name="fillColor">The fill color of the <see cref="Redaction"/>.</param>
        /// <param name="markAsDirty"><see langword="true"/> if the dirty flag should be set to 
        /// <see langword="true"/>; <see langword="false"/> if the dirty flag should not be 
        /// changed.</param>
        void SetFillColor(RedactionColor fillColor, bool markAsDirty)
        {
            // Update the fill color
            _fillColor = fillColor;

            // Set the appropriate highlight color
            Color color = _fillColor == RedactionColor.Black ? BlackPaint : WhitePaint;
            SetColor(color, markAsDirty);
        }

        #endregion Methods

        #region Operator Overloads

        /// <summary>
        /// Adds two <see cref="Redaction"/> objects together.
        /// </summary>
        /// <param name="redaction1">The first <see cref="Redaction"/> addend.</param>
        /// <param name="redaction2">The second <see cref="Redaction"/> addend.</param>
        /// <returns>A new <see cref="Redaction"/> that is the sum of the specified
        /// <see cref="Redaction"/>.</returns>
        public static Redaction operator +(Redaction redaction1, Redaction redaction2)
        {
            try
            {
                // Ensure objects are from the same image viewer
                ExtractException.Assert("ELI22806",
                    "Cannot add redactions from different image viewers!",
                    redaction1.ImageViewer == redaction2.ImageViewer);

                // Ensure objects are on the same page
                ExtractException.Assert("ELI22807",
                    "Cannot add redactions from different pages!",
                    redaction1.PageNumber == redaction2.PageNumber);

                // Create a new redaction layer object initialized with the objects from the first
                // object and containing a combination of the tags
                string comment = redaction1.Comment == redaction2.Comment ? redaction1.Comment : "Union";
                List<string> tags = new List<string>(redaction1.Tags.Count + redaction2.Tags.Count);
                tags.AddRange(redaction1.Tags);
                tags.AddRange(redaction2.Tags);
                Redaction result = new Redaction(redaction1.ImageViewer,
                    redaction1.PageNumber, tags, comment, null);
                
                // Add the objects from both redactions
                result.Objects.AddRange(redaction1.Objects);
                result.Objects.AddRange(redaction2.Objects);

                // Sort the result
                result.Objects.Sort();

                // Return the new redaction
                return result;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22808", ex);
            }
        }

        /// <summary>
        /// Adds two <see cref="Redaction"/> objects together.
        /// </summary>
        /// <param name="redaction1">The first <see cref="Redaction"/> addend.</param>
        /// <param name="redaction2">The second <see cref="Redaction"/> addend.</param>
        /// <returns>A new <see cref="Redaction"/> that is the sum of the specified
        /// <see cref="Redaction"/>.</returns>
        public static Redaction Add(Redaction redaction1, Redaction redaction2)
        {
            return redaction1 + redaction2;
        }

        #endregion Operator Overloads

        #region IComparable<Redaction> Members

        /// <summary>
        /// Compares this <see cref="Redaction"/> with another
        /// <see cref="Redaction"/>.
        /// </summary>
        /// <param name="other">A <see cref="Redaction"/> to compare with this
        /// <see cref="Redaction"/>.</param>
        /// <returns>An <see cref="int"/> that indicates the relative order of the
        /// <see cref="Redaction"/> objects that are being compared.</returns>
        public int CompareTo(Redaction other)
        {
            try
            {
                // Cast to the base class and call the base compare
                CompositeLayerObject<Highlight> thisCompositeHighlight = this;
                CompositeLayerObject<Highlight> otherCompositeHighlight = other;

                return thisCompositeHighlight.CompareTo(otherCompositeHighlight);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22813", ex);
            }
        }

        /// <summary>
        /// Checks whether the specified <see cref="object"/> is equal to
        /// this <see cref="Redaction"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with.</param>
        /// <returns><see langword="true"/> if the objects are equal and
        /// <see langword="false"/> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // Check if this is a redacton object
            Redaction redaction = obj as Redaction;
            if (redaction == null)
            {
                return false;
            }

            // Check if they are equal
            return this == redaction;
        }

        /// <summary>
        /// Checks whether the specified <see cref="Redaction"/> is equal to
        /// this <see cref="Redaction"/>.
        /// </summary>
        /// <param name="redaction">The <see cref="Redaction"/> to compare with.</param>
        /// <returns><see langword="true"/> if the zones are equal and
        /// <see langword="false"/> otherwise.</returns>
        public bool Equals(Redaction redaction)
        {
            return this == redaction;
        }

        /// <summary>
        /// Returns a hashcode for this <see cref="Redaction"/>.
        /// </summary>
        /// <returns>The hashcode for this <see cref="Redaction"/>.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Checks whether the two specified <see cref="Redaction"/> objects
        /// are equal.
        /// </summary>
        /// <param name="redaction1">A <see cref="Redaction"/> to compare.</param>
        /// <param name="redaction2">A <see cref="Redaction"/> to compare.</param>
        /// <returns><see langword="true"/> if the composite objects are equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator ==(Redaction redaction1, Redaction redaction2)
        {
            // Check if the same object first
            if (ReferenceEquals(redaction1, redaction2))
            {
                return true;
            }

            // Cast to the base class
            CompositeLayerObject<Highlight> redactionComposite1 = redaction1;
            CompositeLayerObject<Highlight> redactionComposite2 = redaction2;

            // Call the base equals
            return redactionComposite1 == redactionComposite2;
        }

        /// <summary>
        /// Checks whether the two specified <see cref="Redaction"/> objects
        /// are not equal.
        /// </summary>
        /// <param name="redaction1">A <see cref="Redaction"/> to compare.</param>
        /// <param name="redaction2">A <see cref="Redaction"/> to compare.</param>
        /// <returns><see langword="true"/> if the redactions are not equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator !=(Redaction redaction1, Redaction redaction2)
        {
            return !(redaction1 == redaction2);
        }

        /// <summary>
        /// Checks whether the first specified <see cref="Redaction"/>
        /// is less than the second specified <see cref="Redaction"/>.
        /// </summary>
        /// <param name="redaction1">A <see cref="Redaction"/> to compare.</param>
        /// <param name="redaction2">A <see cref="Redaction"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="redaction1"/> is less
        /// than <paramref name="redaction2"/> and <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator <(Redaction redaction1, Redaction redaction2)
        {
            return redaction1.CompareTo(redaction2) < 0;
        }

        /// <summary>
        /// Checks whether the first specified <see cref="Redaction"/>
        /// is greater than the second specified <see cref="Redaction"/>.
        /// </summary>
        /// <param name="redaction1">A <see cref="Redaction"/> to compare.</param>
        /// <param name="redaction2">A <see cref="Redaction"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="redaction1"/> is greater
        /// than <paramref name="redaction2"/> and <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator >(Redaction redaction1, Redaction redaction2)
        {
            return redaction1.CompareTo(redaction2) > 0;
        }

        #endregion IComparable<Redaction> Members

        #region IXmlSerializable Members

        /// <summary>
        /// Generates a <see cref="Redaction"/> from its XML representation.
        /// </summary>
        /// <param name="reader">The stream from which the <see cref="Redaction"/> is 
        /// deserialized.</param>
        public override void ReadXml(XmlReader reader)
        {
            try
            {
                // Read the base class data (zones only)
                ReadXml(reader, true);

                // Get the fill color
                if (reader.Name != "Color")
                {
                    throw new ExtractException("ELI23254", "Invalid format.");
                }
                SetFillColor((RedactionColor)
                    Enum.Parse(typeof(RedactionColor), reader.ReadElementContentAsString()), false);
                reader.Read();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22922", ex);
            }
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The stream to which the <see cref="Redaction"/> is serialized.
        /// </param>
        public override void WriteXml(XmlWriter writer)
        {
            try
            {
                // Write the base class xml data (zones only)
                WriteXml(writer, true);

                // Write the color
                writer.WriteElementString("Color", _fillColor.ToString());
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22923", ex);
            }
        }

        #endregion IXmlSerializable Members
    }
}
