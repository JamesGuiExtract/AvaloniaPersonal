using Extract.Drawing;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="LayerObject"/> that anchors off a particular point.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class AnchoredObject : LayerObject
    {
        #region Fields

        /// <summary>
        /// The anchor point of the <see cref="AnchoredObject"/> in logical (image) coordinates.
        /// </summary>
        Point _anchorPoint;

        /// <summary>
        /// The alignment of the anchor point relative to the <see cref="AnchoredObject"/>.
        /// </summary>
        AnchorAlignment _anchorAlignment;

        /// <summary>
        /// The anchor point at the start of an interactive tracking event.
        /// </summary>
        Point _originalAnchorPoint;

        /// <summary>
        /// The anchor alignment at the start of an interactive tracking event.
        /// </summary>
        AnchorAlignment _originalAnchorAlignment;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchoredObject"/> class.
        /// </summary>
        protected AnchoredObject()
        {
            // Needed for serialization
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchoredObject"/> class.
        /// </summary>
        /// <param name="imageViewer">The <see cref="IDocumentViewer"/> with which the 
        /// <see cref="AnchoredObject"/> is associated.</param>
        /// <param name="pageNumber">The page on which the <see cref="AnchoredObject"/> appears.
        /// </param>
        /// <param name="comment">The method by which the <see cref="AnchoredObject"/> was 
        /// created.</param>
        /// <param name="anchorPoint">The anchor point of the <see cref="AnchoredObject"/> in 
        /// logical (image) coordinates.</param>
        /// <param name="anchorAlignment">The alignment of <paramref name="anchorPoint"/> relative 
        /// to the <see cref="AnchoredObject"/>.</param>
        [CLSCompliant(false)]
        protected AnchoredObject(IDocumentViewer imageViewer, int pageNumber, string comment, 
            Point anchorPoint, AnchorAlignment anchorAlignment)
            : base(imageViewer, pageNumber, comment)
        {
            _anchorPoint = anchorPoint;
            _anchorAlignment = anchorAlignment;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the anchor point of the <see cref="AnchoredObject"/> in logical (image) 
        /// coordinates.
        /// </summary>
        /// <value>The anchor point of the <see cref="AnchoredObject"/> in logical (image) 
        /// coordinates.</value>
        /// <returns>The anchor point of the <see cref="AnchoredObject"/> in logical (image) 
        /// coordinates.</returns>
        [ReadOnly(true)]
        [Category("Position")]
        [Description("The location of the anchor point of the object.")]
        [RefreshProperties(RefreshProperties.All)]
        public virtual Point AnchorPoint
        {
            get
            {
                return _anchorPoint;
            }
            set
            {
                _anchorPoint = value;

                Dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the <see cref="AnchorPoint"/> relative to the 
        /// <see cref="AnchoredObject"/>.
        /// </summary>
        /// <value>The alignment of the <see cref="AnchorPoint"/> relative to the 
        /// <see cref="AnchoredObject"/>.</value>
        /// <returns>The alignment of the <see cref="AnchorPoint"/> relative to the 
        /// <see cref="AnchoredObject"/>.</returns>
        [ReadOnly(true)]
        [Category("Position")]
        [Description("The alignment of the anchor point relative to the object.")]
        [RefreshProperties(RefreshProperties.All)]
        public virtual AnchorAlignment AnchorAlignment
        {
            get
            {
                return _anchorAlignment;
            }
            set
            {
                _anchorAlignment = value;

                Dirty = true;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Retrieves the center points of grip handles in logical (image) coordinates.
        /// </summary>
        /// <returns>The center points of grip handles in logical (image) coordinates.</returns>
        public override PointF[] GetGripPoints()
        {
            return new PointF[0];
        }

        /// <summary>
        /// Retrieves the cursor when the mouse is over a grip handle.
        /// </summary>
        /// <returns>The cursor when the mouse is over a grip handle.</returns>
        public override Cursor GetGripCursor(int gripHandleId)
        {
            throw new ExtractException("ELI22263", "Anchored objects don't have grip handles.");
        }

        /// <summary>
        /// Translates the <see cref="AnchoredObject"/> by the specified point and optionally 
        /// raises events.
        /// </summary>
        /// <param name="offsetBy">The point by which to translate the 
        /// <see cref="AnchoredObject"/> in logical (image) coordinates.</param>
        /// <param name="raiseEvents"><see langword="true"/> if events should be raised; 
        /// <see langword="false"/> if no events should be raised.</param>
        public override void Offset(Point offsetBy, bool raiseEvents)
        {
            try
            {
                _anchorPoint.Offset(offsetBy);

                if (raiseEvents)
                {
                    Dirty = true;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26530", ex);
            }
        }

        /// <summary>
        /// Begins a grip handle tracking event.
        /// </summary>
        /// <param name="mouseX">The physical (client) x-coordinate of the mouse cursor.</param>
        /// <param name="mouseY">The physical (client) y-coordinate of the mouse cursor.</param>
        /// <param name="gripHandleId">The id of the grip handle to track.</param>
        public override void StartTrackingGripHandle(int mouseX, int mouseY, int gripHandleId)
        {
            throw new ExtractException("ELI22264", "Anchored objects don't have grip handles.");
        }

        /// <summary>
        /// Stores the spatial data associated with the <see cref="AnchoredObject"/> at the start 
        /// of  an interactive tracking event.
        /// </summary>
        public override void Store()
        {
            _originalAnchorPoint = _anchorPoint;
            _originalAnchorAlignment = _anchorAlignment;
        }

        /// <summary>
        /// Restores the spatial data associated with the <see cref="AnchoredObject"/> to its 
        /// state at the start of an interactive tracking event.
        /// </summary>
        public override void Restore()
        {
            _anchorPoint = _originalAnchorPoint;
            _anchorAlignment = _originalAnchorAlignment;
        }

        /// <summary>
        /// Paints the <see cref="AnchoredObject"/> within the specified region using the 
        /// specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The graphics object on which to paint in logical (image) 
        /// coordinates.</param>
        /// <param name="clip">The area within which the <see cref="AnchoredObject"/> should be 
        /// clipped in destination coordinates.</param>
        /// <param name="transform">A 3x3 affine matrix that maps logical (image) coordinates to 
        /// destination coordinates.</param>
        public abstract override void Paint(Graphics graphics, Region clip, Matrix transform);

        /// <summary>
        /// Determines whether the specified point is contained by the 
        /// <see cref="AnchoredObject"/>.
        /// </summary>
        /// <param name="point">The point to test for containment in logical (image) coordinates.
        /// </param>
        /// <returns><see langword="true"/> if the point is contained by the 
        /// <see cref="AnchoredObject"/>; <see langword="false"/> if the point is not contained.
        /// </returns>
        public abstract override bool HitTest(Point point);

        /// <summary>
        /// Gets the smallest rectangle that contains the <see cref="AnchoredObject"/> in logical 
        /// (image) coordinates.
        /// </summary>
        /// <returns>The smallest rectangle that contains the <see cref="AnchoredObject"/> in 
        /// logical (image) coordinates.
        /// </returns>
        public abstract override Rectangle GetBounds();

        /// <summary>
        /// Retrieves the vertices of the <see cref="AnchoredObject"/> in logical (image) 
        /// coordinates.
        /// </summary>
        /// <returns>The vertices of the <see cref="AnchoredObject"/> in logical (image) 
        /// coordinates.</returns>
        public abstract override PointF[] GetVertices();

        #endregion Methods

        #region IXmlSerializable Members

        /// <summary>
        /// Generates a <see cref="AnchoredObject"/> from its XML representation.
        /// </summary>
        /// <param name="reader">The stream from which the <see cref="AnchoredObject"/> is 
        /// deserialized.</param>
        public override void ReadXml(XmlReader reader)
        {
            try
            {
                base.ReadXml(reader);

                // Read the anchor position
                if (reader.Name != "AnchorPosition")
                {
                    throw new ExtractException("ELI22868", "Invalid format.");
                }
                _anchorPoint = new Point(
                    Convert.ToInt32(reader.GetAttribute("X"), CultureInfo.CurrentCulture),
                    Convert.ToInt32(reader.GetAttribute("Y"), CultureInfo.CurrentCulture));
                reader.Read();

                // Read the content alignment
                if (reader.Name != "ContentAlignment")
                {
                    throw new ExtractException("ELI22869", "Invalid format.");
                }
                _anchorAlignment = (AnchorAlignment)
                    Enum.Parse(typeof(AnchorAlignment), reader.ReadElementContentAsString());
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22909", ex);
            }
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The stream to which the <see cref="AnchoredObject"/> is serialized.
        /// </param>
        public override void WriteXml(XmlWriter writer)
        {
            try
            {
                base.WriteXml(writer);

                // Write the anchor position
                writer.WriteStartElement("AnchorPosition");
                writer.WriteAttributeString("X", _anchorPoint.X.ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("Y", _anchorPoint.Y.ToString(CultureInfo.CurrentCulture));
                writer.WriteEndElement();

                // Write the content alignment
                writer.WriteElementString("ContentAlignment", _anchorAlignment.ToString());
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22910", ex);
            }
        }

        #endregion IXmlSerializable Members
    }
}
