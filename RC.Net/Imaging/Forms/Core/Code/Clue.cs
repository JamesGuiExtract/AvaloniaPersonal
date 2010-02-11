using Extract.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Xml;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a clue that may span multiple lines.
    /// </summary>
    public class Clue : CompositeHighlightLayerObject, IComparable<Clue>
    {
        #region Constants

        /// <summary>
        /// The color for a clue object.
        /// </summary>
        static readonly Color _CLUE_COLOR = Color.Yellow;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The text associated with this <see cref="Clue"/>.
        /// </summary>
        string _text = "";

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Clue"/> class.
        /// </summary>
        protected Clue()
        {
            // Needed for serialization
            Deletable = false;
            Movable = false;
            CanRender = false;
        }

        /// <overloads>
        /// Initializes a new instance of the <see cref="Clue"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="Clue"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the
        /// <see cref="Clue"/> appears.</param>
        /// <param name="pageNumber">The one-based page number where this composite object
        /// is found.</param>
        /// <param name="comment">The method by which the <see cref="Clue"/>
        /// was created.</param>
        /// <param name="rasterZones">The collection of raster zones that make up this
        /// <see cref="Clue"/>.  All <see cref="RasterZone"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="RasterZone.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        public Clue(ImageViewer imageViewer, int pageNumber, string comment,
            IEnumerable<RasterZone> rasterZones)
            : this(imageViewer, pageNumber, null, comment, rasterZones)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Clue"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the
        /// <see cref="Clue"/> appears.</param>
        /// <param name="pageNumber">The one-based page number where this composite object
        /// is found.</param>
        /// <param name="tags">The collection of tags to add to this object.</param>
        /// <param name="rasterZones">The collection of raster zones that make up this
        /// <see cref="Clue"/>.  All <see cref="RasterZone"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="RasterZone.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        public Clue(ImageViewer imageViewer, int pageNumber, IEnumerable<string> tags,
            IEnumerable<RasterZone> rasterZones)
            : this(imageViewer, pageNumber, tags, "", rasterZones)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Clue"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the
        /// <see cref="Clue"/> appears.</param>
        /// <param name="pageNumber">The one-based page number where this composite object
        /// is found.</param>
        /// <param name="tags">The collection of tags to add to this object.</param>
        /// <param name="comment">The method by which the <see cref="Clue"/>
        /// was created.</param>
        /// <param name="rasterZones">The collection of raster zones that make up this
        /// <see cref="Clue"/>.  All <see cref="RasterZone"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="RasterZone.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        public Clue(ImageViewer imageViewer, int pageNumber, IEnumerable<string> tags,
            string comment, IEnumerable<RasterZone> rasterZones)
            : base(imageViewer, pageNumber, tags, comment, rasterZones, _CLUE_COLOR)
        {
            Deletable = false;
            Movable = false;
            CanRender = false;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the text associated with this <see cref="Clue"/>.
        /// </summary>
        /// <value>The text associated with this <see cref="Clue"/>.</value>
        /// <returns>The text associated with this <see cref="Clue"/>.</returns>
        [ReadOnly(true)]
        [Category("Appearance")]
        [Description("The text associated with this object.")]
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Retrieves the center points of grip handles in logical (image) coordinates.
        /// </summary>
        /// <returns>The center points of grip handles in logical (image) coordinates.</returns>
        public override Point[] GetGripPoints()
        {
            // Clues do not have grip handles so return an empty array
            return new Point[0];
        }

        /// <summary>
        /// Clues don't have grip handles, just draw the dashed pen around the highlights.
        /// </summary>
        /// <param name="graphics">The graphics with which to draw. Cannot be 
        /// <see langword="null"/>.</param>
        public override void DrawGripHandles(Graphics graphics)
        {
            try
            {
                // Do nothing if not on the active page
                if (PageNumber != base.ImageViewer.PageNumber)
                {
                    return;
                }

                // Just draw the line around the objects, do not draw the grip handles
                foreach (Highlight highlight in Objects)
                {
                    Point[] vertices = highlight.GetGripVertices();
                    ImageViewer.Transform.TransformPoints(vertices);

                    GdiGraphics gdiGraphics = new GdiGraphics(graphics, RasterDrawMode.MaskPen);
                    gdiGraphics.DrawPolygon(SelectionPen, vertices);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23068", ex);
            }
        }

        /// <summary>
        /// Clues do not have grip handles.
        /// </summary>
        /// <param name="point">The point to retrieve the grip handle in physical (client) 
        /// coordinates.</param>
        /// <returns>The zero-based grip handle id that contains the specified point or -1 if no 
        /// grip handle contains the specified point.</returns>
        public override int GetGripHandleId(Point point)
        {
            // No grip handles, just return -1
            return -1;
        }

        /// <summary>
        /// Retrieves the cursor when the mouse is over a grip handle.
        /// </summary>
        /// <returns>The cursor when the mouse is over a grip handle.</returns>
        public override System.Windows.Forms.Cursor GetGripCursor(int gripHandleId)
        {
            throw new ExtractException("ELI23066", "Clues don't have grip handles.");
        }

        #endregion Methods

        #region Operator Overloads

        /// <summary>
        /// Adds two <see cref="Clue"/> objects together.
        /// </summary>
        /// <param name="clue1">The first <see cref="Clue"/> addend.</param>
        /// <param name="clue2">The second <see cref="Clue"/> addend.</param>
        /// <returns>A new <see cref="Clue"/> object which represents the sum of the
        /// specified clues.</returns>
        /// <exception cref="ExtractException">If objects are connected to different
        /// <see cref="ImageViewer"/>.</exception>
        /// <exception cref="ExtractException">If objects are on different pages.</exception>
        public static Clue operator +(Clue clue1, Clue clue2)
        {
            try
            {
                // Ensure objects are from the same image viewer
                ExtractException.Assert("ELI23069", "Cannot add clues from different image viewers!",
                    clue1.ImageViewer == clue2.ImageViewer);

                // Ensure objects are on the same page
                ExtractException.Assert("ELI23070", "Cannot add clues from different pages!",
                    clue1.PageNumber == clue2.PageNumber);

                // Create a new Clue object initialized with the objects from the first
                // clue and containing a combination of the tags
                string comment = clue1.Comment == clue2.Comment ? clue1.Comment : "Union";
                List<string> tags = new List<string>(clue1.Tags.Count + clue2.Tags.Count);
                tags.AddRange(clue1.Tags);
                tags.AddRange(clue2.Tags);
                Clue result = new Clue(clue1.ImageViewer, clue1.PageNumber, tags, comment, null);

                // Add the objects from both clues
                result.Objects.AddRange(clue1.Objects);
                result.Objects.AddRange(clue2.Objects);

                // Sort the result
                result.Objects.Sort();

                // Return the new Clue
                return result;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23071", ex);
            }
        }

        /// <summary>
        /// Adds two <see cref="Clue"/> objects together.
        /// </summary>
        /// <param name="clue1">The first <see cref="Clue"/> addend.</param>
        /// <param name="clue2">The second <see cref="Clue"/> addend.</param>
        /// <returns>A new <see cref="Clue"/> object which represents the sum of the
        /// specified clues.</returns>
        public static Clue Add(Clue clue1, Clue clue2)
        {
            return clue1 + clue2;
        }

        #endregion Operator Overloads
        
        #region IComparable<Clue> Members

        /// <summary>
        /// Compares this <see cref="Clue"/> with another
        /// <see cref="Clue"/>.
        /// </summary>
        /// <param name="other">A <see cref="Clue"/> to compare with this
        /// <see cref="Clue"/>.</param>
        /// <returns>An <see cref="int"/> that indicates the relative order of the
        /// <see cref="Clue"/> objects that are being compared.</returns>
        public int CompareTo(Clue other)
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
                throw ExtractException.AsExtractException("ELI23248", ex);
            }
        }

        /// <summary>
        /// Checks whether the specified <see cref="object"/> is equal to
        /// this <see cref="Clue"/>.
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
            Clue clue = obj as Clue;
            if (clue == null)
            {
                return false;
            }

            // Check if they are equal
            return this == clue;
        }

        /// <summary>
        /// Checks whether the specified <see cref="Clue"/> is equal to
        /// this <see cref="Clue"/>.
        /// </summary>
        /// <param name="clue">The <see cref="Clue"/> to compare with.</param>
        /// <returns><see langword="true"/> if the zones are equal and
        /// <see langword="false"/> otherwise.</returns>
        public bool Equals(Clue clue)
        {
            return this == clue;
        }

        /// <summary>
        /// Returns a hashcode for this <see cref="Clue"/>.
        /// </summary>
        /// <returns>The hashcode for this <see cref="Clue"/>.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Checks whether the two specified <see cref="Clue"/> objects
        /// are equal.
        /// </summary>
        /// <param name="clue1">A <see cref="Clue"/> to compare.</param>
        /// <param name="clue2">A <see cref="Clue"/> to compare.</param>
        /// <returns><see langword="true"/> if the composite objects are equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator ==(Clue clue1, Clue clue2)
        {
            // Check if the same object first
            if (ReferenceEquals(clue1, clue2))
            {
                return true;
            }

            // Cast to the base class
            CompositeLayerObject<Highlight> clueComposite1 = clue1;
            CompositeLayerObject<Highlight> clueComposite2 = clue2;

            // Call the base equals
            return clueComposite1 == clueComposite2;
        }

        /// <summary>
        /// Checks whether the two specified <see cref="Clue"/> objects
        /// are not equal.
        /// </summary>
        /// <param name="clue1">A <see cref="Clue"/> to compare.</param>
        /// <param name="clue2">A <see cref="Clue"/> to compare.</param>
        /// <returns><see langword="true"/> if the redactions are not equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator !=(Clue clue1, Clue clue2)
        {
            return !(clue1 == clue2);
        }

        /// <summary>
        /// Checks whether the first specified <see cref="Clue"/>
        /// is less than the second specified <see cref="Clue"/>.
        /// </summary>
        /// <param name="clue1">A <see cref="Clue"/> to compare.</param>
        /// <param name="clue2">A <see cref="Clue"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="clue1"/> is less
        /// than <paramref name="clue2"/> and <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator <(Clue clue1, Clue clue2)
        {
            return clue1.CompareTo(clue2) < 0;
        }

        /// <summary>
        /// Checks whether the first specified <see cref="Clue"/>
        /// is greater than the second specified <see cref="Clue"/>.
        /// </summary>
        /// <param name="clue1">A <see cref="Clue"/> to compare.</param>
        /// <param name="clue2">A <see cref="Clue"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="clue1"/> is greater
        /// than <paramref name="clue2"/> and <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator >(Clue clue1, Clue clue2)
        {
            return clue1.CompareTo(clue2) > 0;
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// Generates a <see cref="Clue"/> from its XML representation.
        /// </summary>
        /// <param name="reader">The stream from which the <see cref="Clue"/> is 
        /// deserialized.</param>
        public override void ReadXml(XmlReader reader)
        {
            try
            {
                base.ReadXml(reader);

                // Read the text if it exists
                if (reader.Name == "Text")
                {
                    _text = reader.ReadElementContentAsString();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23455", ex);
            }
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The stream to which the <see cref="Clue"/> is serialized.
        /// </param>
        public override void WriteXml(XmlWriter writer)
        {
            try
            {
                base.WriteXml(writer);

                // Write the text
                writer.WriteElementString("Text", _text);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23456", ex);
            }
        }

        #endregion IXmlSerializable Members
    }
}
