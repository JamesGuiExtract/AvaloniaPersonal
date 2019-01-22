using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a layer object which is made up of multiple layer objects.
    /// </summary>
    [CLSCompliant(false)]
    public class CompositeLayerObject<T> : LayerObject, IComparable<CompositeLayerObject<T>>
        where T : LayerObject, IDisposable, IComparable<T>
    {
        #region Constants

        /// <summary>
        /// The mask to <see langword="&amp;"/> with a grip handle id to get the grip handle id of 
        /// the individual object to which it corresponds.
        /// </summary>
        const int _GRIP_HANDLE_MASK = 7;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The collection of layer objects from which the <see cref="CompositeLayerObject{T}"/>
        /// is comprised.
        /// </summary>
        List<T> _objects = new List<T>();

        /// <summary>
        /// Cache for the overall bounds as calculated by the GetBounds call to improve efficiency
        /// for repeated calls to GetBounds (as is typical when populating an image with many
        /// CompositeHighlightLayerObjects)
        /// </summary>
        Rectangle? _bounds;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeLayerObject{T}"/> class.
        /// </summary>
        protected CompositeLayerObject()
        {
        }

        /// <overloads>Initializes a new instance of the <see cref="CompositeLayerObject{T}"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeLayerObject{T}"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the
        /// <see cref="CompositeLayerObject{T}"/> appears.</param>
        /// <param name="pageNumber">The one-based page number where this composite object
        /// is found.</param>
        /// <param name="comment">The method by which the <see cref="CompositeLayerObject{T}"/>
        /// was created.</param>
        /// <param name="objects">The collection of objects that make up this
        /// <see cref="CompositeLayerObject{T}"/>.  All <see cref="LayerObject"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="LayerObject.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>

        public CompositeLayerObject(IDocumentViewer imageViewer, int pageNumber, string comment,
            IEnumerable<T> objects)
            : base(imageViewer, pageNumber, comment)
        {
            try
            {
                AddObjects(objects);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22755", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeLayerObject{T}"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the
        /// <see cref="CompositeLayerObject{T}"/> appears.</param>
        /// <param name="pageNumber">The one-based page number where this composite object
        /// is found.</param>
        /// <param name="tags">The collection of tags to add to this object.</param>
        /// <param name="objects">The collection of objects that make up this
        /// <see cref="CompositeLayerObject{T}"/>.  All <see cref="LayerObject"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="LayerObject.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        public CompositeLayerObject(IDocumentViewer imageViewer, int pageNumber,
            IEnumerable<string> tags, IEnumerable<T> objects) : this(imageViewer,
            pageNumber, tags, "", objects)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeLayerObject{T}"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer on which the
        /// <see cref="CompositeLayerObject{T}"/> appears.</param>
        /// <param name="pageNumber">The one-based page number where this composite object
        /// is found.</param>
        /// <param name="tags">The collection of tags to add to this object.</param>
        /// <param name="objects">The collection of objects that make up this
        /// <param name="comment">The method by which the <see cref="CompositeLayerObject{T}"/>
        /// was created.</param>
        /// <see cref="CompositeLayerObject{T}"/>.  All <see cref="LayerObject"/> in the collection
        /// must be on the same page as <paramref name="pageNumber"/>.</param>
        /// <exception cref="ExtractException">If any <see cref="LayerObject.PageNumber"/>
        /// in the collection does not equal <paramref name="pageNumber"/>.</exception>
        public CompositeLayerObject(IDocumentViewer imageViewer, int pageNumber,
            IEnumerable<string> tags, string comment, IEnumerable<T> objects)
            : base(imageViewer, pageNumber, tags, comment)
        {
            try
            {
                AddObjects(objects);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22756", ex);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the top left <see cref="Point"/> for this <see cref="CompositeLayerObject{T}"/>.
        /// </summary>
        /// <returns>The top left <see cref="Point"/> for this <see cref="CompositeLayerObject{T}"/>.
        /// </returns>
        public override Point Location
        {
            get
            {
                // Return the top left of the bounding rectangle
                return _objects.Count > 0 ? GetBounds().Location : Point.Empty;
            }
        }

        /// <summary>
        /// Gets the <see cref="List{T}"/> of objects that make up this
        /// <see cref="CompositeLayerObject{T}"/>.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of objects.</returns>
        // This is an internal property only intended to be accessed by derived
        // classes or classes within this assembly. The derived classes may need access to the
        // underlying list so that they can add to, or remove from the underlying list.  It also
        // will enhance the performance of this property if it is not wrapping
        // the list as readonly or placing it into a more generic collection. Since
        // this is intended to be used by the derived or internal classes, we can focus
        // on performance and expose the underlying list here.
        // For more information on this FxCop violation being suppressed see:
        // http://msdn.microsoft.com/en-us/library/ms182142(VS.80).aspx
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        internal List<T> Objects
        {
            get
            {
                // Return the list of objects
                return _objects;
            }
        }

        /// <summary>
        /// Gets or sets the image viewer associated with the 
        /// <see cref="CompositeLayerObject{T}"/>.
        /// </summary>
        /// <value>The image viewer associated with the <see cref="CompositeLayerObject{T}"/>.
        /// </value>
        /// <returns>The image viewer associated with the <see cref="CompositeLayerObject{T}"/>.
        /// </returns>
        public override IDocumentViewer ImageViewer
        {
            get
            {
                return base.ImageViewer;
            }
            set
            {
                try
                {
                    base.ImageViewer = value;

                    foreach (T layerObject in _objects)
                    {
                        layerObject.ImageViewer = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26533", ex);
                }
            }
        }

        #endregion

        #region Methods

        /// <overloads>Paints the <see cref="CompositeLayerObject{T}"/> using the specified 
        /// <see cref="Graphics"/> object.
        /// </overloads>
        /// <summary>
        /// Paints the layer object using the specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The graphics object on which to paint in logical (image) 
        /// coordinates.</param>
        /// <param name="clip">The area within which the <see cref="CompositeLayerObject{T}"/> 
        /// should be clipped in destination coordinates.</param>
        /// <param name="transform">A 3x3 affine matrix that maps logical (image) coordinates to 
        /// destination coordinates.</param>
        public override void Paint(Graphics graphics, Region clip, Matrix transform)
        {
            try
            {
                // Draw each of the internal objects
                foreach (T layerObject in _objects)
                {
                    layerObject.Paint(graphics, clip, transform);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22757", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified point is contained by the <see cref="CompositeLayerObject{T}"/>.
        /// </summary>
        /// <param name="point">The point to test for containment in logical (image) coordinates.
        /// </param>
        /// <returns><see langword="true"/> if the point is contained by the 
        /// <see cref="CompositeLayerObject{T}"/>; <see langword="false"/> if the point is not contained.
        /// </returns>
        public override bool HitTest(Point point)
        {
            try
            {
                // Check if any object is hit
                foreach (T layerObject in _objects)
                {
                    if (layerObject.HitTest(point))
                    {
                        return true;
                    }
                }

                // No object was hit
                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22758", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified rectangle intersects with the
        /// <see cref="CompositeLayerObject{T}"/>.
        /// </summary>
        /// <param name="rectangle">The rectangle to test for containment in logical (image)
        /// coordinates.
        /// </param>
        /// <returns><see langword="true"/> if the rectangle intersects the
        /// <see cref="CompositeLayerObject{T}"/>; <see langword="false"/> if it does not.</returns>
        public override bool HitTest(Rectangle rectangle)
        {
            try
            {
                // Check if any object is hit
                foreach (T layerObject in _objects)
                {
                    if (layerObject.HitTest(rectangle))
                    {
                        return true;
                    }
                }

                // No object was hit
                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31281", ex);
            }
        }

        /// <summary>
        /// Retrieves the zero-based grip handle id that contains the specified point.
        /// </summary>
        /// <param name="point">The point to retrieve the grip handle in physical (client) 
        /// coordinates.</param>
        /// <returns>The zero-based grip handle id that contains the specified point or -1 if no 
        /// grip handle contains the specified point.</returns>
        public override int GetGripHandleId(Point point)
        {
            try
            {
                // Iterate through all of the objects
                for (int i = 0; i < _objects.Count; i++)
                {
                    // Get the grip handle id
                    int gripHandleId = _objects[i].GetGripHandleId(point);
                    if (gripHandleId >= 0)
                    {
                        // The grip handle is the concatenation of the objects index
                        // and its individual grip handle id
                        return (i << 3) + gripHandleId;
                    }
                }

                // The grip handle wasn't found, return -1
                return -1;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22759", ex);
            }
        }

        /// <summary>
        /// Retrieves the center points of grip handles in logical (image) coordinates.
        /// </summary>
        /// <returns>The center points of grip handles in logical (image) coordinates.</returns>
        public override PointF[] GetGripPoints()
        {
            try
            {
                // Create a list with the capacity to hold all grip handles
                List<PointF> gripPoints = new List<PointF>(_objects.Count * 8);

                // Add all the grip handles to the collection
                foreach (T layerObject in _objects)
                {
                    gripPoints.AddRange(layerObject.GetGripPoints());
                }

                // Return the result
                return gripPoints.ToArray();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22760", ex);
            }
        }

        /// <summary>
        /// Retrieves the cursor when the mouse is over a grip handle.
        /// </summary>
        /// <returns>The cursor when the mouse is over a grip handle.</returns>
        public override Cursor GetGripCursor(int gripHandleId)
        {
            try
            {
                // Get the index of the selected object
                int i = GetObjectIndexFromGripHandleId(gripHandleId);

                // Return the grip cursor of the specified object
                return _objects[i].GetGripCursor(gripHandleId & _GRIP_HANDLE_MASK);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22761", ex);
            }
        }

        /// <summary>
        /// Translates the <see cref="CompositeLayerObject{T}"/> by the specified point and optionally raises 
        /// events.
        /// </summary>
        /// <param name="offsetBy">The point by which to translate the layer object in logical 
        /// (image) coordinates.</param>
        /// <param name="raiseEvents"><see langword="true"/> if events should be raised; 
        /// <see langword="false"/> if no events should be raised.</param>
        public override void Offset(Point offsetBy, bool raiseEvents)
        {
            try
            {
                // The bounds are about to change; clear the bounds cache
                _bounds = null;

                // Offset all of the layer objects
                foreach (T layerObject in _objects)
                {
                    layerObject.Offset(offsetBy, raiseEvents);
                }

                // [DNRCAU #298] - Composite layer object should set dirty flag if offset
                if (raiseEvents)
                {
                    Dirty = true;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22762", ex);
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
            try
            {
                // The bounds are about to change; clear the bounds cache
                _bounds = null;

                // Get the index of the selected object
                int i = GetObjectIndexFromGripHandleId(gripHandleId);

                // Start the tracking event
                _objects[i].StartTrackingGripHandle(mouseX, mouseY,
                    gripHandleId & _GRIP_HANDLE_MASK);

                // Start the tracking data
                TrackingData = new TrackingData((Control)base.ImageViewer, mouseX, mouseY, Rectangle.Empty);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22763", ex);
            }
        }

        /// <summary>
        /// Begins a <see cref="CompositeLayerObject{T}"/> selection event.
        /// </summary>
        /// <param name="mouseX">The physical (client) x-coordinate of the mouse cursor.</param>
        /// <param name="mouseY">The physical (client) y-coordinate of the mouse cursor.</param>
        public override void StartTrackingSelection(int mouseX, int mouseY)
        {
            try
            {
                // The bounds are about to change; clear the bounds cache
                _bounds = null;

                base.StartTrackingSelection(mouseX, mouseY);

                LayerObject layerObject = ImageViewer.GetLayerObjectAtPoint<LayerObject>(
                    EnumerateAsLayerObjects(_objects), mouseX, mouseY, true);
                if (layerObject != null)
                {
                    layerObject.StartTrackingSelection(mouseX, mouseY);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22764", ex);
            }
        }

        /// <summary>
        /// Enumerates the internal generic-type layer objects as layer objects.
        /// </summary>
        /// <param name="layerObjects">The layer objects to enumerate.</param>
        /// <returns>Enumerates the internal generic-type layer objects as layer objects.</returns>
        static IEnumerable<LayerObject> EnumerateAsLayerObjects(IEnumerable<T> layerObjects)
        {
            foreach (T layerObject in layerObjects)
            {
                yield return layerObject;
            }
        }

        /// <summary>
        /// Updates an interactive select composite object event using the mouse position specified.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        public override void UpdateTracking(int mouseX, int mouseY)
        {
            try
            {
                // The bounds are changing; clear the bounds cache
                _bounds = null;

                foreach (T layerObject in _objects)
                {
                    if (layerObject.TrackingData != null)
                    {
                        layerObject.UpdateTracking(mouseX, mouseY);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22765", ex);
            }
        }

        /// <summary>
        /// Ends an interactive tracking event
        /// </summary>
        /// <param name="mouseX">The physical (client) x-coordinate of the mouse cursor.</param>
        /// <param name="mouseY">The physical (client) y-coordinate of the mouse cursor.</param>
        public override void EndTracking(int mouseX, int mouseY)
        {
            try
            {
                // The bounds changed; clear the bounds cache
                _bounds = null;

                foreach (T layerObject in _objects)
                {
                    if (layerObject.TrackingData != null)
                    {
                        layerObject.EndTracking(mouseX, mouseY);

                        // Reset the tracking data
                        layerObject.TrackingData = null;
                    }
                }

                base.EndTracking(mouseX, mouseY);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22766", ex);
            }
        }

        /// <summary>
        /// Cancels the interactive tracking event in progress.
        /// </summary>
        public override void CancelTracking()
        {
            try
            {
                // The bounds changed; clear the bounds cache
                _bounds = null;

                foreach (T layerObject in _objects)
                {
                    if (layerObject.TrackingData != null)
                    {
                        layerObject.CancelTracking();
                    }
                }

                // End the tracking event
                TrackingData = null;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22767", ex);
            }
        }

        /// <summary>
        /// Determines whether the <see cref="CompositeLayerObject{T}"/> is positioned in a valid place.
        /// </summary>
        /// <returns><see langword="true"/> if the <see cref="CompositeLayerObject{T}"/> is positioned in a 
        /// valid place; <see langword="false"/> if the <see cref="CompositeLayerObject{T}"/> is not positioned 
        /// in a valid place.
        /// </returns>
        public override bool IsValid()
        {
            try
            {
                // Check whether any object is invalid
                foreach (T layerObject in _objects)
                {
                    if (!layerObject.IsValid())
                    {
                        return false;
                    }
                }

                // All objects are valud
                return true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22768", ex);
            }
        }

        /// <summary>
        /// Stores the spatial data associated with the <see cref="LayerObject"/> at the start of 
        /// an interactive tracking event.
        /// </summary>
        public override void Store()
        {
            try
            {
                // Store each of the objects
                foreach (T layerObject in _objects)
                {
                    layerObject.Store();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22769", ex);
            }
        }

        /// <summary>
        /// Restores the spatial data associated with the <see cref="LayerObject"/> to its state at 
        /// the start of an interactive tracking event.
        /// </summary>
        public override void Restore()
        {
            try
            {
                // The bounds changed; clear the bounds cache
                _bounds = null;

                // Restore all of the objects
                foreach (T layerObject in _objects)
                {
                    layerObject.Restore();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22770", ex);
            }
        }

        /// <summary>
        /// Gets the smallest rectangle that contains the <see cref="LayerObject"/>
        /// in logical image coordinates.
        /// </summary>
        /// <returns>The smallest rectangle that contains the <see cref="LayerObject"/>
        /// in logical image coordinates.</returns>
        // The method may perform a calculation, so it is better suited as a method.
        public override Rectangle GetBounds()
        {
            try
            {
                // Calculate the bounds if the bounds are not currently cached.
                if (_bounds == null)
                {
                    foreach (T layerObject in _objects)
                    {
                        if (_bounds != null)
                        {
                            _bounds = Rectangle.Union(_bounds.Value, layerObject.GetBounds());
                        }
                        else
                        {
                            _bounds = layerObject.GetBounds();
                        }
                    }
                }

                return _bounds ?? Rectangle.Empty;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22771", ex);
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
                // Draw the grip handles of each object
                foreach (T layerObject in _objects)
                {
                    layerObject.DrawSelection(graphics, drawGripPoints);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22772", ex);
            }
        }

        /// <summary>
        /// Retrieves the vertices of the <see cref="CompositeLayerObject{T}"/> in logical image coordinates.
        /// </summary>
        /// <returns>The vertices of the <see cref="CompositeLayerObject{T}"/> in logical image coordinates.</returns>
        public override PointF[] GetVertices()
        {
            try
            {
                // Create a list with the capacity to hold all the vertices
                List<PointF> vertices = new List<PointF>(_objects.Count * 4);

                // Add all the vertices to the collection
                foreach (T layerObject in _objects)
                {
                    vertices.AddRange(layerObject.GetVertices());
                }

                // Return the result
                return vertices.ToArray();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22773", ex);
            }
        }

        /// <summary>
        /// Retrieves the vertices of the selection border in logical (image) coordinates.
        /// </summary>
        /// <returns>The vertices of the selection border in logical (image) coordinates.</returns>
        public override PointF[] GetGripVertices()
        {
            try
            {
                // Create a list with the capacity to hold all the vertices
                List<PointF> vertices = new List<PointF>(_objects.Count * 4);

                // Add all the vertices to the collection
                foreach (T layerObject in _objects)
                {
                    vertices.AddRange(layerObject.GetGripVertices());
                }

                // Return the result
                return vertices.ToArray();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28781", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified rectangle intersects the <see cref="CompositeLayerObject{T}"/>.
        /// </summary>
        /// <param name="rectangle">The rectangle in logical (image) coordinates to check for 
        /// intersection.</param>
        /// <returns><see langword="true"/> if the <paramref name="rectangle"/> intersects the 
        /// <see cref="CompositeLayerObject{T}"/>; <see langword="false"/> if the <paramref name="rectangle"/> 
        /// does not intersect the <see cref="LayerObject"/>.</returns>
        public override bool IsVisible(Rectangle rectangle)
        {
            try
            {
                if (base.IsVisible(rectangle))
                {
                    // Check if any object is visible
                    foreach (T layerObject in _objects)
                    {
                        if (layerObject.IsVisible(rectangle))
                        {
                            return true;
                        }
                    }
                }

                // No object is visible
                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22774", ex);
            }
        }

        /// <summary>
        /// Gets the corresponding index of the object in <see cref="_objects"/> for the 
        /// specified grip handle id.
        /// </summary>
        /// <param name="gripHandleId">The id from which to retrieve the index.</param>
        /// <returns>The corresponding index of the object in <see cref="_objects"/>.
        /// </returns>
        int GetObjectIndexFromGripHandleId(int gripHandleId)
        {
            int i = gripHandleId >> 3;
            if (i < 0 || i >= _objects.Count)
            {
                throw new ExtractException("ELI22775", "Invalid grip handle id.");
            }

            return i;
        }

        /// <summary>
        /// Adds the specified collection of objects into this <see cref="CompositeLayerObject{T}"/>.
        /// </summary>
        /// <param name="objects">The collection of <see cref="LayerObject"/> to add into the
        /// <see cref="CompositeLayerObject{T}"/>.  Each <see cref="LayerObject"/> in the collection
        /// must be on the same page as this <see cref="CompositeLayerObject{T}"/>.</param>
        /// <exception cref="ExtractException">If <see cref="LayerObject.PageNumber"/>
        /// does not equal this <see cref="LayerObject.PageNumber"/>.</exception>
        void AddObjects(IEnumerable<T> objects)
        {
            try
            {
                if (objects != null)
                {
                    // The set of layer objects is changing; clear the bounds cache
                    _bounds = null;

                    // Ensure all objects are on the same page
                    foreach (T layerObject in objects)
                    {
                        if (layerObject.PageNumber != PageNumber)
                        {
                            throw new ExtractException("ELI22776",
                                "Composite object cannot span multiple pages!");
                        }

                        _objects.Add(layerObject);
                    }

                    // Sort the collection of objects
                    _objects.Sort();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22777", ex);
            }
        }

        #endregion Methods

        #region Operator Overloads

        /// <summary>
        /// Adds two <see cref="CompositeLayerObject{T}"/> objects together.
        /// </summary>
        /// <param name="object1">The first <see cref="CompositeLayerObject{T}"/> addend.</param>
        /// <param name="object2">The second <see cref="CompositeLayerObject{T}"/> addend.</param>
        /// <returns>A new <see cref="CompositeLayerObject{T}"/> that is the sum of the specified
        /// <see cref="CompositeLayerObject{T}"/>.</returns>
        public static CompositeLayerObject<T> operator +(CompositeLayerObject<T> object1,
            CompositeLayerObject<T> object2)
        {
            try
            {
                // Ensure objects are from the same image viewer
                ExtractException.Assert("ELI22778",
                    "Cannot add composite objects from different image viewers!",
                    object1.ImageViewer == object2.ImageViewer);

                // Ensure objects are on the same page
                ExtractException.Assert("ELI22805",
                    "Cannot add composite objects from different pages!",
                    object1.PageNumber == object2.PageNumber);

                // Create a new Composite layer object initialized with the objects from the first
                // object and containing a combination of the tags
                string comment = object1.Comment == object2.Comment ? object1.Comment : "Union";
                List<string> tags = new List<string>(object1.Tags.Count + object2.Tags.Count);
                tags.AddRange(object1.Tags);
                tags.AddRange(object2.Tags);
                CompositeLayerObject<T> result = new CompositeLayerObject<T>(object1.ImageViewer,
                    object1.PageNumber, tags, comment, object1._objects);

                // Add the objects from the second composite layer object
                result._objects.AddRange(object2._objects);

                // Sort the result
                result._objects.Sort();

                // Return the new Composite layer object
                return result;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22779", ex);
            }
        }

        /// <summary>
        /// Adds two <see cref="CompositeLayerObject{T}"/> objects together.
        /// </summary>
        /// <param name="object1">The first <see cref="CompositeLayerObject{T}"/> addend.</param>
        /// <param name="object2">The second <see cref="CompositeLayerObject{T}"/> addend.</param>
        /// <returns>A new <see cref="CompositeLayerObject{T}"/> that is the sum of the specified
        /// <see cref="CompositeLayerObject{T}"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static CompositeLayerObject<T> Add(CompositeLayerObject<T> object1,
            CompositeLayerObject<T> object2)
        {
            try
            {
                return object1 + object2;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22780", ex);
            }
        }

        #endregion Operator Overloads

        #region IDisposable Members

        /// <overloads>Releases resources used by the <see cref="CompositeLayerObject{T}"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="CompositeLayerObject{T}"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_objects != null)
                {
                    foreach (T layerObject in _objects)
                    {
                        if (layerObject != null)
                        {
                            layerObject.Dispose();
                        }
                    }

                    _objects = null;
                }
            }

            // Dispose of unmanaged resources

            // Dispose of base class
            base.Dispose(disposing);
        }

        #endregion IDisposable

        #region IComparable<CompositeLayerObject<T>> Members

        /// <summary>
        /// Compares this <see cref="CompositeLayerObject{T}"/> with another
        /// <see cref="CompositeLayerObject{T}"/>.
        /// </summary>
        /// <param name="other">A <see cref="CompositeLayerObject{T}"/> to compare with this
        /// <see cref="CompositeLayerObject{T}"/>.</param>
        /// <returns>An <see cref="int"/> that indicates the relative order of the
        /// <see cref="CompositeLayerObject{T}"/> objects that are being compared.</returns>
        public int CompareTo(CompositeLayerObject<T> other)
        {
            try
            {
                // Compare the first object of each composite layer object
                int returnVal = _objects[0].CompareTo(other._objects[0]);

                // If the first objects are equal, compare the size of the
                // objects collection
                if (returnVal == 0)
                {
                    returnVal = _objects.Count.CompareTo(other._objects.Count);
                }

                return returnVal;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22781", ex);
            }
        }

        /// <summary>
        /// Checks whether the specified <see cref="object"/> is equal to
        /// this <see cref="CompositeLayerObject{T}"/>.
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

            // Check if this is a CompositeLayerObject object
            CompositeLayerObject<T> compositeObject = obj as CompositeLayerObject<T>;
            if (compositeObject == null)
            {
                return false;
            }

            // Check if they are equal
            return this == compositeObject;
        }

        /// <summary>
        /// Checks whether the specified <see cref="CompositeLayerObject{T}"/> is equal to
        /// this <see cref="CompositeLayerObject{T}"/>.
        /// </summary>
        /// <param name="compositeObject">The <see cref="CompositeLayerObject{T}"/> to compare with.</param>
        /// <returns><see langword="true"/> if the zones are equal and
        /// <see langword="false"/> otherwise.</returns>
        public bool Equals(CompositeLayerObject<T> compositeObject)
        {
            return this == compositeObject;
        }

        /// <summary>
        /// Returns a hashcode for this <see cref="CompositeLayerObject{T}"/>.
        /// </summary>
        /// <returns>The hashcode for this <see cref="CompositeLayerObject{T}"/>.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Checks whether the two specified <see cref="CompositeLayerObject{T}"/> objects
        /// are equal.
        /// </summary>
        /// <param name="compositeObject1">A <see cref="CompositeLayerObject{T}"/> to compare.</param>
        /// <param name="compositeObject2">A <see cref="CompositeLayerObject{T}"/> to compare.</param>
        /// <returns><see langword="true"/> if the composite objects are equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator ==(CompositeLayerObject<T> compositeObject1,
            CompositeLayerObject<T> compositeObject2)
        {
            // Check if the same object first
            if (ReferenceEquals(compositeObject1, compositeObject2))
            {
                return true;
            }

            // If one of the objects is null, return false
            if (((object)compositeObject1 == null) || ((object)compositeObject2 == null))
            {
                return false;
            }

            // If the objects are on different pages, return false
            if (compositeObject1.PageNumber != compositeObject2.PageNumber)
            {
                return false;
            }

            // Check count of layer objects in composite object
            bool equal = compositeObject1._objects.Count == compositeObject2._objects.Count;
            if (equal)
            {
                // Count is equal, check each highlight
                for (int i = 0; i < compositeObject1._objects.Count; i++)
                {
                    equal = compositeObject1._objects[i] == compositeObject2._objects[i];
                    if (!equal)
                    {
                        // One object did not match, no need to keep checking
                        break;
                    }
                }
            }

            return equal;
        }

        /// <summary>
        /// Checks whether the two specified <see cref="CompositeLayerObject{T}"/> objects
        /// are not equal.
        /// </summary>
        /// <param name="compositeObject1">A <see cref="CompositeLayerObject{T}"/> to compare.</param>
        /// <param name="compositeObject2">A <see cref="CompositeLayerObject{T}"/> to compare.</param>
        /// <returns><see langword="true"/> if the objects are not equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator !=(CompositeLayerObject<T> compositeObject1,
            CompositeLayerObject<T> compositeObject2)
        {
            return !(compositeObject1 == compositeObject2);
        }

        /// <summary>
        /// Checks whether the first specified <see cref="CompositeLayerObject{T}"/>
        /// is less than the second specified <see cref="CompositeLayerObject{T}"/>.
        /// </summary>
        /// <param name="compositeObject1">A <see cref="CompositeLayerObject{T}"/> to compare.</param>
        /// <param name="compositeObject2">A <see cref="CompositeLayerObject{T}"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="compositeObject1"/> is less
        /// than <paramref name="compositeObject2"/> and <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator <(CompositeLayerObject<T> compositeObject1,
            CompositeLayerObject<T> compositeObject2)
        {
            return compositeObject1.CompareTo(compositeObject2) < 0;
        }

        /// <summary>
        /// Checks whether the first specified <see cref="CompositeLayerObject{T}"/>
        /// is greater than the second specified <see cref="CompositeLayerObject{T}"/>.
        /// </summary>
        /// <param name="compositeObject1">A <see cref="CompositeLayerObject{T}"/> to compare.</param>
        /// <param name="compositeObject2">A <see cref="CompositeLayerObject{T}"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="compositeObject1"/> is greater
        /// than <paramref name="compositeObject2"/> and <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator >(CompositeLayerObject<T> compositeObject1,
            CompositeLayerObject<T> compositeObject2)
        {
            return compositeObject1.CompareTo(compositeObject2) > 0;
        }

        #endregion IComparable<CompositeLayerObject<T>>
    }
}
