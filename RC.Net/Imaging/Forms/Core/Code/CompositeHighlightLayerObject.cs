using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Xml;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="LayerObject"/> which is made up of multiple <see cref="Highlight"/>
    /// objects.
    /// </summary>
    public class CompositeHighlightLayerObject : CompositeLayerObject<Highlight>,
        IComparable<CompositeHighlightLayerObject>
    {
        #region Fields

        /// <summary>
        /// The color of the highlights in the <see cref="CompositeHighlightLayerObject"/>.
        /// </summary>
        Color _color;

        /// <summary>
        /// The color of the highlights' outlines. <see langword="null"/> if the highlights
        /// should not have outlines.
        /// </summary>
        Color? _outlineColor;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeHighlightLayerObject"/> class.
        /// </summary>
        protected CompositeHighlightLayerObject()
        {
            // Needed for serialization
        }

        /// <overloads>Initializes a new instance of the <see cref="CompositeHighlightLayerObject"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeHighlightLayerObject"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the
        /// <see cref="CompositeHighlightLayerObject"/> appears.</param>
        /// <param name="pageNumber">The one-based page number where this composite object
        /// is found.</param>
        /// <param name="comment">The method by which the <see cref="CompositeHighlightLayerObject"/>
        /// was created.</param>
        /// <param name="highlights">The collection of objects that make up this
        /// <see cref="CompositeHighlightLayerObject"/>.  All <see cref="LayerObject"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="LayerObject.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        public CompositeHighlightLayerObject(ImageViewer imageViewer, int pageNumber,
            string comment, IEnumerable<Highlight> highlights)
            : base(imageViewer, pageNumber, comment, highlights)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeHighlightLayerObject"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the
        /// <see cref="CompositeHighlightLayerObject"/> appears.</param>
        /// <param name="pageNumber">The one-based page number where this composite object
        /// is found.</param>
        /// <param name="tags">The collection of tags to add to this object.</param>
        /// <param name="highlights">The collection of objects that make up this
        /// <see cref="CompositeHighlightLayerObject"/>.  All <see cref="LayerObject"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="LayerObject.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        public CompositeHighlightLayerObject(ImageViewer imageViewer, int pageNumber,
            IEnumerable<string> tags, IEnumerable<Highlight> highlights)
            : base(imageViewer, pageNumber, tags, highlights)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeHighlightLayerObject"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the
        /// <see cref="CompositeHighlightLayerObject"/> appears.</param>
        /// <param name="pageNumber">The one-based page number where this composite object
        /// is found.</param>
        /// <param name="tags">The collection of tags to add to this object.</param>
        /// <param name="highlights">The collection of objects that make up this
        /// <param name="comment">The method by which the <see cref="CompositeHighlightLayerObject"/>
        /// was created.</param>
        /// <see cref="CompositeHighlightLayerObject"/>.  All <see cref="LayerObject"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="LayerObject.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        public CompositeHighlightLayerObject(ImageViewer imageViewer, int pageNumber,
            IEnumerable<string> tags, string comment, IEnumerable<Highlight> highlights)
            : base(imageViewer, pageNumber, tags, comment, highlights)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeHighlightLayerObject"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the
        /// <see cref="CompositeHighlightLayerObject"/> appears.</param>
        /// <param name="pageNumber">The one-based page number where this composite object
        /// is found.</param>
        /// <param name="comment">The method by which the <see cref="CompositeHighlightLayerObject"/>
        /// was created.</param>
        /// <param name="rasterZones">The collection of raster zones that make up this
        /// <see cref="CompositeHighlightLayerObject"/>.  All <see cref="RasterZone"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.</param>
        /// <param name="highlightColor">The <see cref="Color"/> that will be used to draw the
        /// <see cref="Highlight"/> that will be created from the specified
        /// <see cref="RasterZone"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="RasterZone.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        public CompositeHighlightLayerObject(ImageViewer imageViewer, int pageNumber,
            string comment, IEnumerable<RasterZone> rasterZones, Color highlightColor)
            : base(imageViewer, pageNumber, comment, null)
        {
            _color = highlightColor;

            AddRasterZones(rasterZones);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeHighlightLayerObject"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the
        /// <see cref="CompositeHighlightLayerObject"/> appears.</param>
        /// <param name="pageNumber">The one-based page number where this composite object
        /// is found.</param>
        /// <param name="comment">The method by which the <see cref="CompositeHighlightLayerObject"/>
        /// was created.</param>
        /// <param name="tags">The collection of tags to add to this object.</param>
        /// <param name="rasterZones">The collection of raster zones that make up this
        /// <see cref="CompositeHighlightLayerObject"/>.  All <see cref="RasterZone"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.</param>
        /// <param name="highlightColor">The <see cref="Color"/> that will be used to draw the
        /// <see cref="Highlight"/> that will be created from the specified.
        /// <see cref="RasterZone"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="RasterZone.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        public CompositeHighlightLayerObject(ImageViewer imageViewer, int pageNumber,
            IEnumerable<string> tags, string comment, IEnumerable<RasterZone> rasterZones,
            Color highlightColor)
            : base(imageViewer, pageNumber, tags, comment, null)
        {
            _color = highlightColor;

            AddRasterZones(rasterZones);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeHighlightLayerObject"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the
        /// <see cref="CompositeHighlightLayerObject"/> appears.</param>
        /// <param name="pageNumber">The one-based page number where this composite object
        /// is found.</param>
        /// <param name="tags">The collection of tags to add to this object.</param>
        /// <param name="rasterZones">The collection of raster zones that make up this
        /// <see cref="CompositeHighlightLayerObject"/>.  All <see cref="RasterZone"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.</param>
        /// <param name="highlightColor">The <see cref="Color"/> that will be used to draw the
        /// <see cref="Highlight"/> that will be created from the specified
        /// <see cref="RasterZone"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="RasterZone.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        public CompositeHighlightLayerObject(ImageViewer imageViewer, int pageNumber,
            IEnumerable<string> tags, IEnumerable<RasterZone> rasterZones, Color highlightColor)
            : this(imageViewer, pageNumber, tags, "", rasterZones, highlightColor)
        {
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or sets the color of the <see cref="CompositeHighlightLayerObject"/>.
        /// </summary>
        /// <value>The color of the <see cref="CompositeHighlightLayerObject"/>.</value>
        /// <returns>The color of the <see cref="CompositeHighlightLayerObject"/>.</returns>
        [Browsable(false)]
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                try
                {
                    SetColor(value, false);

                    if (ImageViewer != null)
                    {
                        ImageViewer.Invalidate();
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26531", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the <see cref="CompositeHighlightLayerObject"/> outline.
        /// <see langword="null"/> if the <see cref="CompositeHighlightLayerObject"/> has
        /// no outline.
        /// </summary>
        /// <value>The color of the <see cref="CompositeHighlightLayerObject"/>.</value>
        /// <returns>The color of the <see cref="CompositeHighlightLayerObject"/>.</returns>
        [Browsable(false)]
        public Color? OutlineColor
        {
            get
            {
                return _outlineColor;
            }
            set
            {
                try
                {
                    _outlineColor = value;

                    // Update the highlights
                    foreach (Highlight highlight in Objects)
                    {
                        highlight.OutlineColor = _outlineColor;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26532", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the redaction's border.
        /// </summary>
        /// <value>The color of the redaction's border; <see langword="null"/> if no border should 
        /// be drawn.</value>
        public Color? BorderColor
        {
            get
            {
                return Objects[0].BorderColor;
            }
            set
            {
                try
                {
                    foreach (Highlight highlight in Objects)
                    {
                        highlight.BorderColor = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28798", ex);
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Computes the area of overlap between this <see cref="CompositeHighlightLayerObject"/>
        /// and a specified <see cref="CompositeHighlightLayerObject"/>.
        /// </summary>
        /// <param name="compositeObject">The <see cref="CompositeHighlightLayerObject"/> to compute overlap
        /// with. This may not be <see langword="null"/>.</param>
        /// <returns>The area of overlap between this <see cref="CompositeHighlightLayerObject"/> and a
        /// specified <see cref="CompositeHighlightLayerObject"/>.</returns>
        /// <exception cref="ExtractException">If <paramref name="compositeObject"/>
        /// is <see langword="null"/>.</exception>
        public double GetAreaOverlappingWith(CompositeHighlightLayerObject compositeObject)
        {
            try
            {
                // Ensure the object is not null
                ExtractException.Assert("ELI22790", "Composite highlight object may not be null!",
                    compositeObject != null);

                // Create a list to hold the raster zones for the composite object
                List<RasterZone> rasterZones = new List<RasterZone>(compositeObject.Objects.Count);

                foreach (Highlight highlight in compositeObject.Objects)
                {
                    rasterZones.Add(highlight.ToRasterZone());
                }

                // Get the area of overlap with the collection of raster zones
                return GetAreaOverlappingWith(rasterZones);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22791", ex);
            }
        }

        /// <summary>
        /// Computes the area of overlap between this <see cref="CompositeHighlightLayerObject"/>
        /// and a specified <see cref="IEnumerable{T}"/> of <see cref="RasterZone"/>
        /// objects.
        /// </summary>
        /// <param name="rasterZones">The <see cref="IEnumerable{T}"/> of
        /// <see cref="RasterZone"/> objects to compute overlap with.
        /// This may not be <see langword="null"/>.</param>
        /// <returns>The area of overlap between this <see cref="CompositeHighlightLayerObject"/> and a
        /// specified <see cref="IEnumerable{T}"/> of <see cref="RasterZone"/> objects.</returns>
        /// <exception cref="ExtractException">If <paramref name="rasterZones"/>
        /// is <see langword="null"/>.</exception>
        public double GetAreaOverlappingWith(IEnumerable<RasterZone> rasterZones)
        {
            try
            {
                // Ensure the raster zone collection is not null
                ExtractException.Assert("ELI22792", "Raster zone collection may not be null!",
                    rasterZones != null);


                // Get the raster zones of the composite
                RasterZoneCollection myZones = new RasterZoneCollection(Objects.Count);
                for (int i = 0; i < Objects.Count; i++)
                {
                    myZones.Add(Objects[i].ToRasterZone());
                }

                return myZones.GetAreaOverlappingWith(rasterZones);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22793", ex);
            }
        }

        /// <summary>
        /// Computes the area of the <see cref="CompositeHighlightLayerObject"/>.
        /// </summary>
        /// <returns>The area of the <see cref="CompositeHighlightLayerObject"/>.</returns>
        public double Area()
        {
            try
            {
                // Sum the area of each highlight in the composite object to compute
                // the total area of this composite object 
                double area = 0.0;
                foreach (Highlight highlight in Objects)
                {
                    area += highlight.ToRasterZone().Area();
                }

                return area;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22794", ex);
            }
        }

        /// <summary>
        /// Creates highlights for each of the specified raster zones and adds them to the
        /// <see cref="CompositeHighlightLayerObject"/>.
        /// </summary>
        /// <param name="rasterZones">The collection of <see cref="RasterZone"/> to add
        /// to the <see cref="CompositeHighlightLayerObject"/>. All <see cref="RasterZone"/>
        /// objects must be from the same page as the <see cref="CompositeHighlightLayerObject"/>.
        /// </param>
        /// <exception cref="ExtractException">If one of the specified <see cref="RasterZone"/>
        /// objects is not on the same page as this <see cref="CompositeHighlightLayerObject"/>.
        /// </exception>
        private void AddRasterZones(IEnumerable<RasterZone> rasterZones)
        {
            try
            {
                // Do nothing if the raster zone collection is null
                if (rasterZones != null)
                {
                    // Create a new list of highlights built from the raster zones
                    List<Highlight> highlights = new List<Highlight>();
                    foreach (RasterZone rasterZone in rasterZones)
                    {
                        // Ensure the raster zones are all on the same page as this object
                        if (rasterZone.PageNumber != PageNumber)
                        {
                            throw new ExtractException("ELI22803",
                                "Cannot add raster zones from other pages to this composite object!");
                        }

                        highlights.Add(new Highlight(base.ImageViewer, Comment, rasterZone,
                            "", _color));
                    }

                    // Add the highlights to the collection of highlights and sort the collection
                    Objects.AddRange(highlights);
                    Objects.Sort();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22804", ex);
            }
        }

        /// <summary>
        /// Gets a <see cref="ReadOnlyCollection{T}"/> of <see cref="RasterZone"/> objects
        /// that make up this <see cref="CompositeHighlightLayerObject"/>.
        /// </summary>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of <see cref="RasterZone"/>
        /// objects.</returns>
        // The method may perform a calculation, so it is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public ReadOnlyCollection<RasterZone> GetRasterZones()
        {
            try
            {
                // Create a list to hold the raster zones
                List<RasterZone> rasterZones = new List<RasterZone>(Objects.Count);

                // Get the raster zone for each highlight
                foreach (Highlight highlight in Objects)
                {
                    rasterZones.Add(highlight.ToRasterZone());
                }

                // Return a readonly collection of raster zones
                return rasterZones.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23067", ex);
            }
        }

        /// <summary>
        /// Sets the color of the <see cref="CompositeHighlightLayerObject"/>.
        /// </summary>
        /// <param name="color">The color to set the <see cref="CompositeHighlightLayerObject"/>.
        /// </param>
        /// <param name="markAsDirty"><see langword="true"/> to set the dirty flag to 
        /// <see langword="true"/>; <see langword="false"/> to leave the dirty flag unchanged.
        /// </param>
        protected void SetColor(Color color, bool markAsDirty)
        {
            _color = color;

            // Update the highlights
            foreach (Highlight highlight in Objects)
            {
                highlight.SetColor(_color, markAsDirty);
            }

            if (markAsDirty)
            {
                Dirty = true;
            }
        }

        #endregion Methods

        #region Operator Overloads

        /// <summary>
        /// Adds two <see cref="CompositeHighlightLayerObject"/> objects together.
        /// </summary>
        /// <param name="object1">The first <see cref="CompositeHighlightLayerObject"/> addend.</param>
        /// <param name="object2">The second <see cref="CompositeHighlightLayerObject"/> addend.</param>
        /// <returns>A new <see cref="CompositeHighlightLayerObject"/> that is the sum of the specified
        /// <see cref="CompositeHighlightLayerObject"/>.</returns>
        public static CompositeHighlightLayerObject operator +(
            CompositeHighlightLayerObject object1,
            CompositeHighlightLayerObject object2)
        {
            try
            {
                // Ensure objects are from the same image viewer
                ExtractException.Assert("ELI22937",
                    "Cannot add composite highlights from different image viewers!",
                    object1.ImageViewer == object2.ImageViewer);

                // Ensure objects are on the same page
                ExtractException.Assert("ELI22938",
                    "Cannot add composite highlights from different pages!",
                    object1.PageNumber == object2.PageNumber);

                // Create a new composite highlight object initialized with the objects from the first
                // object and containing a combination of the tags
                string comment = object1.Comment == object2.Comment ? object1.Comment : "Union";
                List<string> tags = new List<string>(object1.Tags.Count + object2.Tags.Count);
                tags.AddRange(object1.Tags);
                tags.AddRange(object2.Tags);
                CompositeHighlightLayerObject result = new CompositeHighlightLayerObject(
                    object1.ImageViewer, object1.PageNumber, tags, comment, null);

                // Add the objects from both composite highlights
                result.Objects.AddRange(object1.Objects);
                result.Objects.AddRange(object2.Objects);

                // Sort the result
                result.Objects.Sort();

                // Return the new composite highlight
                return result;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22939", ex);
            }
        }

        /// <summary>
        /// Adds two <see cref="CompositeHighlightLayerObject"/> objects together.
        /// </summary>
        /// <param name="object1">The first <see cref="CompositeHighlightLayerObject"/> addend.</param>
        /// <param name="object2">The second <see cref="CompositeHighlightLayerObject"/> addend.</param>
        /// <returns>A new <see cref="CompositeHighlightLayerObject"/> that is the sum of the specified
        /// <see cref="CompositeHighlightLayerObject"/>.</returns>
        public static CompositeHighlightLayerObject Add(
            CompositeHighlightLayerObject object1, CompositeHighlightLayerObject object2)
        {
            return object1 + object2;
        }

        #endregion Operator Overloads

        #region IComparable<CompositeHighlightLayerObject> Members

        /// <summary>
        /// Compares this <see cref="CompositeHighlightLayerObject"/> with another
        /// <see cref="CompositeHighlightLayerObject"/>.
        /// </summary>
        /// <param name="other">A <see cref="CompositeHighlightLayerObject"/> to compare with this
        /// <see cref="CompositeHighlightLayerObject"/>.</param>
        /// <returns>An <see cref="int"/> that indicates the relative order of the
        /// <see cref="CompositeHighlightLayerObject"/> objects that are being compared.</returns>
        public int CompareTo(CompositeHighlightLayerObject other)
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
                throw ExtractException.AsExtractException("ELI22812", ex);
            }
        }

        /// <summary>
        /// Checks whether the specified <see cref="object"/> is equal to
        /// this <see cref="CompositeHighlightLayerObject"/>.
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

            // Check if this is a composite object
            CompositeHighlightLayerObject compositeHighlight = obj as CompositeHighlightLayerObject;
            if (compositeHighlight == null)
            {
                return false;
            }

            // Check if they are equal
            return this == compositeHighlight;
        }

        /// <summary>
        /// Checks whether the specified <see cref="CompositeHighlightLayerObject"/> is equal to
        /// this <see cref="CompositeHighlightLayerObject"/>.
        /// </summary>
        /// <param name="compositeHighlight">The <see cref="CompositeHighlightLayerObject"/> to compare with.</param>
        /// <returns><see langword="true"/> if the zones are equal and
        /// <see langword="false"/> otherwise.</returns>
        public bool Equals(CompositeHighlightLayerObject compositeHighlight)
        {
            return this == compositeHighlight;
        }

        /// <summary>
        /// Returns a hashcode for this <see cref="CompositeHighlightLayerObject"/>.
        /// </summary>
        /// <returns>The hashcode for this <see cref="CompositeHighlightLayerObject"/>.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Checks whether the two specified <see cref="CompositeHighlightLayerObject"/> objects
        /// are equal.
        /// </summary>
        /// <param name="compositeHighlight1">A <see cref="CompositeHighlightLayerObject"/> to compare.</param>
        /// <param name="compositeHighlight2">A <see cref="CompositeHighlightLayerObject"/> to compare.</param>
        /// <returns><see langword="true"/> if the composite objects are equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator ==(CompositeHighlightLayerObject compositeHighlight1,
            CompositeHighlightLayerObject compositeHighlight2)
        {
            // Check if the same object first
            if (ReferenceEquals(compositeHighlight1, compositeHighlight2))
            {
                return true;
            }

            // Cast to the base class
            CompositeLayerObject<Highlight> baseComposite1 = compositeHighlight1;
            CompositeLayerObject<Highlight> baseComposite2 = compositeHighlight2;

            // Call the base equals
            return baseComposite1 == baseComposite2;
        }

        /// <summary>
        /// Checks whether the two specified <see cref="CompositeHighlightLayerObject"/> objects
        /// are not equal.
        /// </summary>
        /// <param name="compositeHighlight1">A <see cref="CompositeHighlightLayerObject"/> to compare.</param>
        /// <param name="compositeHighlight2">A <see cref="CompositeHighlightLayerObject"/> to compare.</param>
        /// <returns><see langword="true"/> if the composite objects are not equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator !=(CompositeHighlightLayerObject compositeHighlight1,
            CompositeHighlightLayerObject compositeHighlight2)
        {
            return !(compositeHighlight1 == compositeHighlight2);
        }

        /// <summary>
        /// Checks whether the first specified <see cref="CompositeHighlightLayerObject"/>
        /// is less than the second specified <see cref="CompositeHighlightLayerObject"/>.
        /// </summary>
        /// <param name="compositeHighlight1">A <see cref="CompositeHighlightLayerObject"/> to compare.</param>
        /// <param name="compositeHighlight2">A <see cref="CompositeHighlightLayerObject"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="compositeHighlight1"/> is less
        /// than <paramref name="compositeHighlight2"/> and <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator <(CompositeHighlightLayerObject compositeHighlight1,
            CompositeHighlightLayerObject compositeHighlight2)
        {
            return compositeHighlight1.CompareTo(compositeHighlight2) < 0;
        }

        /// <summary>
        /// Checks whether the first specified <see cref="CompositeHighlightLayerObject"/>
        /// is greater than the second specified <see cref="CompositeHighlightLayerObject"/>.
        /// </summary>
        /// <param name="compositeHighlight1">A <see cref="CompositeHighlightLayerObject"/> to compare.</param>
        /// <param name="compositeHighlight2">A <see cref="CompositeHighlightLayerObject"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="compositeHighlight1"/> is greater
        /// than <paramref name="compositeHighlight2"/> and <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator >(CompositeHighlightLayerObject compositeHighlight1,
            CompositeHighlightLayerObject compositeHighlight2)
        {
            return compositeHighlight1.CompareTo(compositeHighlight2) > 0;
        }

        #endregion IComparable<CompositeHighlightLayerObject> Members

        #region IXmlSerializable Members

        /// <summary>
        /// Generates a <see cref="CompositeHighlightLayerObject"/> from its XML representation.
        /// </summary>
        /// <param name="reader">The stream from which the <see cref="CompositeHighlightLayerObject"/> is 
        /// deserialized.</param>
        public override void ReadXml(XmlReader reader)
        {
            ReadXml(reader, false);
        }

        /// <overloads>Generates a <see cref="CompositeHighlightLayerObject"/> from its XML 
        /// representation.</overloads>
        /// <summary>
        /// Generates a <see cref="CompositeHighlightLayerObject"/> from its XML representation.
        /// </summary>
        /// <param name="reader">The stream from which the <see cref="CompositeHighlightLayerObject"/> is 
        /// deserialized.</param>
        /// <param name="zonesOnly">Whether the stream only contains the zones of the 
        /// <see cref="CompositeHighlightLayerObject"/>.</param>
        protected void ReadXml(XmlReader reader, bool zonesOnly)
        {
            try
            {
                base.ReadXml(reader);

                // Get the zones
                while (reader.Name == "Zone")
                {
                    Highlight highlight = new Highlight(Comment, PageNumber);
                    highlight.ReadXmlZone(reader);
                    Objects.Add(highlight);
                }

                // Stop here if only zones are available
                if (zonesOnly)
                {
                    return;
                }

                // Read the color
                if (reader.Name != "Color")
                {
                    throw new ExtractException("ELI22965", "Invalid format.");
                }
                int red = GetAttributeAsInt32(reader, "Red");
                int green = GetAttributeAsInt32(reader, "Green"); 
                int blue = GetAttributeAsInt32(reader, "Blue");
                Color = Color.FromArgb(red, green, blue);
                reader.Read();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22961", ex);
            }
        }

        /// <summary>
        /// Gets the value of the attribute with the specified name as an <see cref="Int32"/>.
        /// </summary>
        /// <param name="reader">The stream which is on the element that contains the attribute.
        /// </param>
        /// <param name="name">The name of the attribute to retrieve.</param>
        /// <returns>The value of the attribute with the specified name as an <see cref="Int32"/>.
        /// </returns>
        static int GetAttributeAsInt32(XmlReader reader, string name)
        {
            string value = reader.GetAttribute(name);
            return Convert.ToInt32(value, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Converts the <see cref="CompositeHighlightLayerObject"/> into its XML representation.
        /// </summary>
        /// <param name="writer">The stream to which the 
        /// <see cref="CompositeHighlightLayerObject"/> is serialized.</param>
        public override void WriteXml(XmlWriter writer)
        {
            WriteXml(writer, false);
        }

        /// <overloads>Converts the <see cref="CompositeHighlightLayerObject"/> into its XML 
        /// representation.</overloads>
        /// <summary>
        /// Converts the zones of the <see cref="CompositeHighlightLayerObject"/> into its XML 
        /// representation.
        /// </summary>
        /// <param name="writer">The stream to which the 
        /// <see cref="CompositeHighlightLayerObject"/> is serialized.</param>
        /// <param name="zonesOnly"><see langword="true"/> if only the zones of the composite 
        /// highlight layer object should be streamed; <see langword="false"/> if all data should 
        /// be streamed.</param>
        protected void WriteXml(XmlWriter writer, bool zonesOnly)
        {
            try
            {
                base.WriteXml(writer);

                // Write the zones 
                foreach (Highlight highlight in Objects)
                {
                    highlight.WriteXmlZone(writer);
                }

                // Stop here if only zones are needed
                if (zonesOnly)
                {
                    return;
                }

                // Write the color
                writer.WriteStartElement("Color");
                writer.WriteAttributeString("Red", GetInt32String(_color.R));
                writer.WriteAttributeString("Green", GetInt32String(_color.G));
                writer.WriteAttributeString("Blue", GetInt32String(_color.B));
                writer.WriteEndElement();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22962", ex);
            }
        }

        /// <summary>
        /// Converts the specified byte into the string representation of its <see cref="Int32"/> 
        /// value.
        /// </summary>
        /// <param name="int32Byte">The byte to convert.</param>
        /// <returns>The string representation of the <see cref="Int32"/> value of 
        /// <paramref name="int32Byte"/>.</returns>
        static string GetInt32String(byte int32Byte)
        {
            int int32 = int32Byte;
            return int32.ToString(CultureInfo.CurrentCulture);
        }

        #endregion IXmlSerializable Members
    }
}
