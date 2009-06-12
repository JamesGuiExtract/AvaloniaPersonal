using Extract.Drawing;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents an object in a particular layer of an <see cref="ImageViewer"/>.
    /// </summary>
    public abstract class LayerObject : IDisposable, IComparable<LayerObject>, IXmlSerializable
    {
        #region LayerObject Constants

        /// <summary>
        /// The comment to use for manually created layer object.
        /// </summary>
        public static readonly string ManualComment = "Manual";

        /// <summary>
        /// The shortest distance between the layer object and the center point of the link arrow 
        /// in physical (client) pixels.
        /// </summary>
        static readonly int _LINK_ARROW_DISTANCE = 13;

        /// <summary>
        /// Half the length of one side of a link arrow in physical (client) pixels.
        /// </summary>
        static readonly int _HALF_LINK_ARROW_SIDE = 8;

        /// <summary>
        /// The distance to expand the bounds of a layer object to encapsulate a link arrow on one 
        /// side in physical (client) coordinates.
        /// </summary>
        static readonly int _LINK_ARROW_EXPAND_DISTANCE = 
            _LINK_ARROW_DISTANCE + _HALF_LINK_ARROW_SIDE;

        /// <summary>
        /// Half the length of one side of a grip handle in physical (client) pixels.
        /// </summary>
        private static readonly int _HALF_GRIP_HANDLE_SIDE = 4;

        /// <summary>
        /// The minimum height and width of a layer object in logical (image) pixels.
        /// </summary>
        private static readonly Size _MIN_SIZE = new Size(1, 1);

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(LayerObject).ToString();

        #endregion LayerObject Constants

        #region LayerObject Fields

        /// <summary>
        /// The next unique id for a layer object.
        /// </summary>
        static long _nextId = 1;

        /// <summary>
        /// The unique id of the layer object.
        /// </summary>
        long _id;

        /// <summary>
        /// The revision number of this layer object.
        /// </summary>
        int _revision = 1;

        /// <summary>
        /// Whether the layer object has changed during the current session.
        /// </summary>
        bool _dirty = true;

        /// <summary>
        /// The page that this layer object is on.
        /// </summary>
        int _pageNumber;

        /// <summary>
        /// The next layer object that this layer object is linked to.
        /// <see langword="null"/> if there is no link.
        /// </summary>
        LayerObject _nextLink;

        /// <summary>
        /// The previous layer object that this layer object is linked to.
        /// <see langword="null"/> if there is no link.
        /// </summary>
        LayerObject _previousLink;

        /// <summary>
        /// The comment associated with <see cref="LayerObject"/>.
        /// </summary>
        string _comment;

        /// <summary>
        /// <see langword="true"/> if this object is currently selected; <see langword="false"/> if
        /// the layer object is not currently selected.
        /// </summary>
        bool _isSelected;

        /// <summary>
        /// <see langword="true"/> if this object is selectable <see langword="false"/> if the
        /// layer object is not selectable.
        /// </summary>
        bool _isSelectable = true;

        /// <summary>
        /// <see langword="true"/> if this object can be moved; <see langword="false"/>
        /// if this layer object cannot be moved.
        /// </summary>
        bool _isMovable = true;

        /// <summary>
        /// <see langword="true"/> if this object can be deleted; <see langword="false"/>
        /// if this layer object cannot be deleted.
        /// </summary>
        bool _isDeletable = true;

        /// <summary>
        /// The tags applied to the layer object.
        /// </summary>
        List<string> _tags = new List<string>();

        /// <summary>
        /// <see langword="true"/> if the layer object is visible; <see langword="false"/> if this
        /// layer object is not visible.
        /// </summary>
        bool _isVisible = true;

        /// <summary>
        /// <see langword="true"/> if the layer object is visible on a hard copy; 
        /// <see langword="false"/> if the layer object is not visible on a hard copy.
        /// </summary>
        bool _render = true;

        /// <summary>
        /// The image viewer on which the layer object appears.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// A <see cref="TrackingData"/> object associated with the interactive event currently in 
        /// progess (e.g. drag and drop).
        /// </summary>
        /// <remarks>Value is <see langword="null"/> if no interactive cursor tool event is being 
        /// tracked.</remarks>
        TrackingData _trackingData;
        
        #endregion LayerObject Fields

        #region LayerObject Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerObject"/> class.
        /// </summary>
        protected LayerObject()
        {
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23110",
                _OBJECT_NAME);

            // Note: This constructor is needed for serialization
            _dirty = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerObject"/> class.
        /// </summary>
        /// <param name="comment">The comment associated with the <see cref="LayerObject"/>.</param>
        /// <param name="pageNumber">The page that this <see cref="LayerObject"/> is on.</param>
        internal LayerObject(int pageNumber, string comment)
        {
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23111",
                _OBJECT_NAME);

            _pageNumber = pageNumber;
            _comment = comment;
        }
        
        /// <overloads>Initializes a new instance of the <see cref="LayerObject"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="LayerObject"/> class.
        /// </summary>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> that this
        /// <see cref="LayerObject"/> is associated with.</param>
        /// <param name="pageNumber">The page that this <see cref="LayerObject"/> is on.</param>
        /// <param name="comment">The comment associated with the <see cref="LayerObject"/>.</param>
        protected LayerObject(ImageViewer imageViewer, int pageNumber, string comment)
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23112",
                    _OBJECT_NAME);

                // Ensure image viewer is not null.
                ExtractException.Assert("ELI21197", "Image viewer must be specified.",
                    imageViewer != null);

                // Ensure an image is open
                ExtractException.Assert("ELI21203", "No image is open.",
                    imageViewer.IsImageAvailable);

                // Store the image viewer
                _imageViewer = imageViewer;

                // Store the page number
                _pageNumber = pageNumber;

                // Store the add method
                _comment = comment;

                // Set the id
                _id = GetNextId();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22729", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerObject"/> class.
        /// </summary>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> that this
        /// <see cref="LayerObject"/> is associated with.</param>
        /// <param name="pageNumber">The one-based page number where this <see cref="LayerObject"/>
        /// is found.</param>
        /// <param name="tags">A <see cref="IEnumerable{T}"/> of tags associated with this
        /// <see cref="LayerObject"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        protected LayerObject(ImageViewer imageViewer, int pageNumber, IEnumerable<string> tags)
            : this(imageViewer, pageNumber, tags, "")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerObject"/> class.
        /// </summary>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> that this
        /// <see cref="LayerObject"/> is associated with.</param>
        /// <param name="pageNumber">The page that this <see cref="LayerObject"/> is on.</param>
        /// <param name="tags">A <see cref="IEnumerable{T}"/> of tags associated with this
        /// <see cref="LayerObject"/>.</param>
        /// <param name="comment">The comment associated with the <see cref="LayerObject"/>.</param>
        protected LayerObject(ImageViewer imageViewer, int pageNumber, IEnumerable<string> tags,
            string comment) : this(imageViewer, pageNumber, comment)
        {
            try
            {
                if (tags != null)
                {
                    _tags.AddRange(tags);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22728", ex);
            }
        }

        #endregion LayerObject Constructors

        #region LayerObject Properties

        /// <summary>
        /// Gets the unique identification number of the layer object.
        /// </summary>
        /// <returns>The unique identification number of the layer object.</returns>
        [Browsable(false)]
        public long Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets or sets the revision number of the layer object.
        /// </summary>
        /// <value>The revision number of the layer object.</value>
        /// <returns>The revision number of the layer object.</returns>
        [Category("Modifications")]
        [Description("The number of times this object has been revised.")]
        public int Revision
        {
            get
            {
                return _revision;
            }
        }

        /// <summary>
        /// Gets or sets whether the layer object has changed during this session.
        /// </summary>
        /// <value><see langword="true"/> if the layer object has changed during this session;
        /// <see langword="false"/> if it has not changed this session.</value>
        /// <returns><see langword="true"/> if the layer object has changed since it was created;
        /// <see langword="false"/> if it has not changed this session.</returns>
        [Browsable(false)]
        public bool Dirty
        {
            get
            {
                return _dirty;
            }
            set
            {
                // Check if the dirty flag is changing value
                if (_dirty ^ value)
                {
                    // Add one if the object is now dirty.
                    // Subtract one if the object is no longer dirty.
                    _revision += value ? 1 : -1;
                }

                _dirty = value;

                // Raise the LayerObjectChanged event if necessary
                if (_dirty == true && _imageViewer != null)
                {
                    _imageViewer.LayerObjects.RaiseLayerObjectChangedEvent(this);
                }
            }
        }

        /// <summary>
        /// Gets the one-based page number that this <see cref="LayerObject"/> is on.
        /// </summary>
        /// <returns>The one-based page number that this <see cref="LayerObject"/> is on.</returns>
        [Category("Position")]
        [Description("The page number on which the object appears.")]
        [ReadOnly(true)]
        public int PageNumber
        {
            get
            {
                return _pageNumber;
            }
        }

        /// <summary>
        /// Gets the next linked <see cref="LayerObject"/> (<see langword="null"/> if
        /// no next linked <see cref="LayerObject"/>).
        /// </summary>
        /// <returns>The next linked <see cref="LayerObject"/>.</returns>
        [Browsable(false)]
        public LayerObject NextLink
        {
            get
            {
                return _nextLink;
            }
        }

        /// <summary>
        /// Gets the previous linked <see cref="LayerObject"/> (<see langword="null"/> if
        /// no previous linked <see cref="LayerObject"/>).
        /// </summary>
        /// <returns>The previous linked <see cref="LayerObject"/>.</returns>
        [Browsable(false)]
        public LayerObject PreviousLink
        {
            get
            {
                return _previousLink;
            }
        }

        /// <summary>
        /// Gets whether this <see cref="LayerObject"/> is linked to another
        /// <see cref="LayerObject"/>.
        /// </summary>
        [Category("Position")]
        [Description("Whether the object is linked to another object.")]
        [RefreshProperties(RefreshProperties.All)]
        public bool IsLinked
        {
            get
            {
                // If there is a link then this object is linked
                return _nextLink != null || _previousLink != null;
            }
        }

        /// <summary>
        /// Gets or sets the method by which the layer object was created.
        /// </summary>
        /// <value>The comment associated with the layer object.</value>
        [Category("Modifications")]
        [Description("The comment associated with the object.")]
        [ReadOnly(true)]
        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                _comment = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the layer object is currently selected.
        /// </summary>
        /// <value><see langword="true"/> if the layer object is currently selected; 
        /// <see langword="false"/> if the layer object is not selected.</value>
        [Browsable(false)]
        public bool Selected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                try
                {
                    // Ensure the layer object is selectable before it can be selected
	                ExtractException.Assert("ELI22600", "Cannot select unselectable object.", 
                        !value || _isSelectable);

                    // Ensure an image viewer exists
                    ExtractException.Assert("ELI22596", "Image viewer must exist.",
                            _imageViewer != null);

                    _isSelected = value;

                    // Get the selection collection
                    LayerObjectsCollection selection = _imageViewer.LayerObjects.Selection;

                    // Check if the layer object is being selected
                    if (value)
                    {
                        // Add the layer object if it was not already
                        if (!selection.Contains(this))
                        {
                            selection.Add(this);
                        }    
                    }
                    else
                    {
                        // Remove the layer object if was not already
                        if (selection.Contains(this))
                        {
                            selection.Remove(this);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI22597",
                        "Cannot select object.", ex);
                    ee.AddDebugData("Selected", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the layer object is selectable.
        /// </summary>
        /// <value><see langword="true"/> if the layer object is selectable; 
        /// <see langword="false"/> if the layer object is not selectable.</value>
        /// <returns><see langword="true"/> if the layer object is selectable; 
        /// <see langword="false"/> if the layer object is not selectable.</returns>
        [Browsable(false)]
        public bool Selectable
        {
            get
            {
                return _isSelectable;
            }
            set
            {
                _isSelectable = value;

                // Unselect the layer object if it is now unselectable
                if (_isSelectable == false && _isSelected)
                {
                    this.Selected = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the layer object is currently visible.
        /// </summary>
        /// <value><see langword="true"/> if the layer object is visible; <see langword="false"/> 
        /// if the layer object is not visible.</value>
        [Browsable(false)]
        public bool Visible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;

                // Raise the visibility changed event
                _imageViewer.LayerObjects.RaiseLayerObjectVisibilityChangedEvent(this);
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="LayerObject"/> is moveable.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="LayerObject"/>
        /// is moveable; <see langword="false"/> if the <see cref="LayerObject"/>
        /// is not moveable.</value>
        /// <returns><see langword="true"/> if the <see cref="LayerObject"/>
        /// is moveable; <see langword="false"/> if the <see cref="LayerObject"/>
        /// is not moveable.</returns>
        [Browsable(false)]
        public bool Movable
        {
            get
            {
                return _isMovable;
            }
            set
            {
                _isMovable = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="LayerObject"/> is deletable.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="LayerObject"/>
        /// is deletable; <see langword="false"/> if the <see cref="LayerObject"/>
        /// is not deletable.</value>
        /// <returns><see langword="true"/> if the <see cref="LayerObject"/>
        /// is deletable; <see langword="false"/> if the <see cref="LayerObject"/>
        /// is not deletable.</returns>
        [Browsable(false)]
        public bool Deletable
        {
            get
            {
                return _isDeletable;
            }
            set
            {
                _isDeletable = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the layer object is visible on a hard copy.
        /// </summary>
        /// <value><see langword="true"/> if the layer object is visible on a hard copy;
        /// <see langword="false"/> if the layer object is not visible on a hard copy.</value>
        /// <returns><see langword="true"/> if the layer object is visible on a hard copy;
        /// <see langword="false"/> if the layer object is not visible on a hard copy.</returns>
        [Browsable(false)]
        public bool CanRender
        {
            get
            {
                return _render;
            }
            set
            {
                _render = value;
            }
        }

        /// <summary>
        /// Gets or sets the image viewer associated with the layer object.
        /// </summary>
        /// <value>The image viewer associated with the layer object.</value>
        /// <returns>The image viewer associated with the layer object.</returns>
        [Browsable(false)]
        public virtual ImageViewer ImageViewer
        {
            get
            {
                return _imageViewer;
            }
            set
            {
                _imageViewer = value;
            }
        }

        /// <summary>
        /// Gets or sets the tracking data associated with the interactive event in progress.
        /// </summary>
        /// <value>The tracking data associated with the interactive event in progress.</value>
        /// <returns>The tracking data associated with the interactive event in progress.</returns>
        internal TrackingData TrackingData
        {
            get
            {
                return _trackingData;
            }
            set
            {
                // Dispose of the previous tracking data
                if (_trackingData != null)
                {
                    _trackingData.Dispose();
                }

                // Set the new tracking data
                _trackingData = value;
            }
        }

        /// <summary>
        /// Gets the minimum height and width of a layer object in logical (image) coordinates.
        /// </summary>
        /// <returns>The minimum height and width of a layer object in logical (image) coordinates.
        /// </returns>
        public static Size MinSize
        {
            get
            {
                return _MIN_SIZE;
            }
        }

        /// <summary>
        /// Gets the collection of tags for this <see cref="LayerObject"/>.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of strings containing the tags
        /// for this <see cref="LayerObject"/>.</returns>
        // FxCop recommends changing this to a generic collection meant for inheritance
        // but in our case we are dealing with an internal List<string> and want to expose
        // the underlying object so that a caller can add additional tags to the collection.
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [Browsable(false)]
        public List<string> Tags
        {
            get
            {
                return _tags;
            }
        }

        /// <summary>
        /// Gets the top left <see cref="Point"/> for this <see cref="LayerObject"/>.
        /// </summary>
        /// <returns>The top left <see cref="Point"/> for this <see cref="LayerObject"/></returns>
        [Category("Position")]
        [Description("The top left point of the bounds of the object.")]
        [RefreshProperties(RefreshProperties.All)]
        public abstract Point Location
        {
            get;
        }

        #endregion LayerObject Properties

        #region LayerObject Methods

        /// <summary>
        /// Gets the next unique id for an object.
        /// </summary>
        /// <returns>The next unique id for an object.</returns>
        static internal long GetNextId()
        {
            return _nextId++;
        }

        /// <summary>
        /// Resets the next unique id. Should only be called when there are no instantiated layer 
        /// objects.
        /// </summary>
        /// <remarks>Unexpected behavior can occur when two layer objects have the same id. If 
        /// this method is called when at least one layer object is instantiated, it is possible 
        /// for subsequent created layer objects to have the same id. For this reason, it is 
        /// strongly recommended that this method only be called when there are no instantiated 
        /// layer objects.</remarks>
        public static void ResetNextId()
        {
            _nextId = 1;
        }

        /// <overloads>Paints the layer object using the specified <see cref="Graphics"/> object.
        /// </overloads>
        /// <summary>
        /// Paints the layer object using the specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The graphics object on which to paint in logical (image) 
        /// coordinates.</param>
        public void Paint(Graphics graphics)
        {
            Paint(graphics, graphics.Clip, _imageViewer.Transform);
        }

        /// <summary>
        /// Paints the layer object within the specified region using the specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The graphics object on which to paint in logical (image) 
        /// coordinates.</param>
        /// <param name="clip">The area within which the layer object should be clipped in 
        /// physical (client) coordinates.</param>
        public void Paint(Graphics graphics, Region clip)
        {
            Paint(graphics, clip, _imageViewer.Transform);
        }

        /// <summary>
        /// Paints the layer object within the specified region using the specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The graphics object on which to paint in logical (image) 
        /// coordinates.</param>
        /// <param name="clip">The area within which the layer object should be clipped in 
        /// destination coordinates.</param>
        /// <param name="transform">A 3x3 affine matrix that maps logical (image) coordinates to 
        /// destination coordinates.</param>
        public abstract void Paint(Graphics graphics, Region clip, Matrix transform);

        /// <summary>
        /// Paints the layer object the way it should appear on a hard copy.
        /// </summary>
        /// <param name="graphics">The graphics object on which to paint in logical (image) 
        /// coordinates.</param>
        /// <param name="transform">A 3x3 affine matrix that maps logical (image) coordinates to 
        /// destination coordinates.</param>
        public virtual void Render(Graphics graphics, Matrix transform)
        {
            if (_render)
            {
                Paint(graphics, graphics.Clip, transform);
            }
        }

        /// <summary>
        /// Determines whether the specified point is contained by the layer object.
        /// </summary>
        /// <param name="point">The point to test for containment in logical (image) coordinates.
        /// </param>
        /// <returns><see langword="true"/> if the point is contained by the layer object; 
        /// <see langword="false"/> if the point is not contained.</returns>
        public abstract bool HitTest(Point point);

        /// <summary>
        /// Determines whether the specified point is in proximity of the layer object or its
        /// link arrows.
        /// </summary>
        /// <param name="point">The point to test for containment in physical (client) coordinates.
        /// </param>
        /// <returns><see langword="true"/> if the point is close to the layer object or one of 
        /// its link arrows; <see langword="false"/> otherwise.</returns>
        public virtual bool HitLinkAreaTest(Point point)
        {
            // Ensure this layer object is on the active page
            if (_imageViewer == null || _imageViewer.PageNumber != _pageNumber)
            {
                return false;
            }

            Rectangle linkArea = GetLinkArea();

            // Check if the point is contained in the link area
            return linkArea.Contains(point);
        }

        

        /// <summary>
        /// Retrieves the zero-based grip handle id that contains the specified point.
        /// </summary>
        /// <param name="point">The point to retrieve the grip handle in physical (client) 
        /// coordinates.</param>
        /// <returns>The zero-based grip handle id that contains the specified point or -1 if no 
        /// grip handle contains the specified point.</returns>
        public virtual int GetGripHandleId(Point point)
        {
            // Get the grip handles in logical (image) coordinates
            Point[] gripPoints = GetGripPoints();

            // If there are no grip points, return -1
            if (gripPoints == null || gripPoints.Length <= 0)
            {
                return -1;
            }

            // Convert the grip points to physical (client) coordinates
            _imageViewer.Transform.TransformPoints(gripPoints);

            // Iterate through the grip points
            for (int i = 0; i < gripPoints.Length; i++)
            {
                // If this point is contained, return the grip handle id
                if (Math.Abs(gripPoints[i].X - point.X) <= _HALF_GRIP_HANDLE_SIDE && 
                    Math.Abs(gripPoints[i].Y - point.Y) <= _HALF_GRIP_HANDLE_SIDE)
                {
                    return i;
                }
            }

            // The grip handle wasn't found, return -1
            return -1;
        }

        /// <summary>
        /// Retrieves the zero-based link arrow id that contains the specified point.
        /// </summary>
        /// <param name="point">The point to retrieve the link arrow in physical (client) 
        /// coordinates.</param>
        /// <returns>The zero-based link arrow id that contains the specified point or -1 if no 
        /// link arrow contains the specified point.</returns>
        public virtual int GetLinkArrowId(Point point)
        {
            // Ensure the layer object is linked
            if (!this.IsLinked)
            {
                return -1;
            }

            // Get the link arrows in physical (client) coordinates
            Point[] linkArrows = GetLinkPoints();

            // If there are no link arrows, return -1
            if (linkArrows == null || linkArrows.Length <= 0)
            {
                return -1;
            }

            // Check if the mouse is over the previous link arrow
            if (this.PreviousLink != null && 
                Math.Abs(linkArrows[0].X - point.X) <= _HALF_LINK_ARROW_SIDE && 
                Math.Abs(linkArrows[0].Y - point.Y) <= _HALF_LINK_ARROW_SIDE)
            {
                return 0;
            }

            // Check if the mouse is over the next link arrow
            if (this.NextLink != null &&
                Math.Abs(linkArrows[1].X - point.X) <= _HALF_LINK_ARROW_SIDE &&
                Math.Abs(linkArrows[1].Y - point.Y) <= _HALF_LINK_ARROW_SIDE)
            {
                return 1;
            }

            // The mouse wasn't over the link arrow, return -1
            return -1;
        }

        /// <summary>
        /// Retrieves the center points of grip handles in logical (image) coordinates.
        /// </summary>
        /// <returns>The center points of grip handles in logical (image) coordinates.</returns>
        public abstract Point[] GetGripPoints();

        /// <summary>
        /// Retrieves the center points of the link arrows in physical (client) coordinates.
        /// </summary>
        /// <returns>The center points of the link arrows in physical (client) coordinates.
        /// </returns>
        public virtual Point[] GetLinkPoints()
        {
            // Get the bounds in physical (client) coordinates
            Rectangle bounds = _imageViewer.GetTransformedRectangle(GetBounds(), false);

            // Calculate the y-coordinate of the center
            int y = bounds.Top + bounds.Height / 2;

            // Calculate the link points
            return new Point[]
            {
                new Point(bounds.Left - _LINK_ARROW_DISTANCE, y),
                new Point(bounds.Right + _LINK_ARROW_DISTANCE, y)
            };
        }

        /// <summary>
        /// Retrieves the cursor when the mouse is over a grip handle.
        /// </summary>
        /// <param name="gripHandleId">The id of the grip handle.</param>
        /// <returns>The cursor when the mouse is over a grip handle.</returns>
        public abstract Cursor GetGripCursor(int gripHandleId);

        /// <overloads>Translates the layer object.</overloads>
        /// <summary>
        /// Translates the layer object by the specified point.
        /// </summary>
        /// <param name="offsetBy">The point by which to translate the layer object in logical 
        /// (image) coordinates.</param>
        public void Offset(Point offsetBy)
        {
            Offset(offsetBy, true);
        }

        /// <summary>
        /// Translates the layer object by the specified point and optionally raises events.
        /// </summary>
        /// <param name="offsetBy">The point by which to translate the layer object in logical 
        /// (image) coordinates.</param>
        /// <param name="raiseEvents"><see langword="true"/> if events should be raised; 
        /// <see langword="false"/> if no events should be raised.</param>
        public abstract void Offset(Point offsetBy, bool raiseEvents);

        /// <summary>
        /// Translates the layer object by the specified horizontal and vertical distance.
        /// </summary>
        /// <param name="offsetX">The horizontal distance to translate the layer object in logical 
        /// (image) coordinates.</param>
        /// <param name="offsetY">The vertical distance to translate the layer object in logical 
        /// (image) coordinates.</param>
        public void Offset(int offsetX, int offsetY)
        {
            Offset(new Point(offsetX, offsetY), true);
        }

        /// <summary>
        /// Translates the layer object by the specified horizontal &amp; vertical distance and 
        /// optionally raises events.
        /// </summary>
        /// <param name="offsetX">The horizontal distance to translate the layer object in logical 
        /// (image) coordinates.</param>
        /// <param name="offsetY">The vertical distance to translate the layer object in logical 
        /// (image) coordinates.</param>
        /// <param name="raiseEvents"><see langword="true"/> if events should be raised; 
        /// <see langword="false"/> if no events should be raised.</param>
        public void Offset(int offsetX, int offsetY, bool raiseEvents)
        {
            Offset(new Point(offsetX, offsetY), raiseEvents);
        }

        /// <summary>
        /// Begins a grip handle tracking event.
        /// </summary>
        /// <param name="mouseX">The physical (client) x-coordinate of the mouse cursor.</param>
        /// <param name="mouseY">The physical (client) y-coordinate of the mouse cursor.</param>
        /// <param name="gripHandleId">The id of the grip handle to track.</param>
        public abstract void StartTrackingGripHandle(int mouseX, int mouseY, int gripHandleId);

        /// <summary>
        /// Begins a layer object selection event.
        /// </summary>
        /// <param name="mouseX">The physical (client) x-coordinate of the mouse cursor.</param>
        /// <param name="mouseY">The physical (client) y-coordinate of the mouse cursor.</param>
        public virtual void StartTrackingSelection(int mouseX, int mouseY)
        {
            if (_isSelectable && _isVisible && _isMovable)
            {
                // Get the mouse position in image coordinates
                Point[] mouse = new Point[] { new Point(mouseX, mouseY) };
                using (Matrix clientToImage = _imageViewer.Transform.Clone())
                {
                    clientToImage.Invert();
                    clientToImage.TransformPoints(mouse);
                }

                // Start the tracking event
                this.TrackingData = new TrackingData(_imageViewer, mouse[0].X, mouse[0].Y,
                    _imageViewer.GetTransformedRectangle(_imageViewer.GetVisibleImageArea(), true));

                // Store the spatial data associated with the layer object
                Store();
            }
        }

        /// <summary>
        /// Updates an interactive tracking event
        /// </summary>
        /// <param name="mouseX">The physical (client) x-coordinate of the mouse cursor.</param>
        /// <param name="mouseY">The physical (client) y-coordinate of the mouse cursor.</param>
        public virtual void UpdateTracking(int mouseX, int mouseY)
        {
            // Check if this is a move event
            if (_imageViewer.Cursor == Cursors.SizeAll)
            {
                // Get the mouse position as a point in image coordinates
                Point[] mouse = new Point[] { new Point(mouseX, mouseY) };
                using (Matrix clientToImage = _imageViewer.Transform.Clone())
                {
                    clientToImage.Invert();
                    clientToImage.TransformPoints(mouse);
                }

                // Compute the distance moved in the x and y direction
                int xDist = mouse[0].X - _trackingData.StartPoint.X;
                int yDist = mouse[0].Y - _trackingData.StartPoint.Y;

                // Update the tracking data
                _trackingData.StartPoint = mouse[0];

                // Now adjust the layer object accordingly
                Offset(xDist, yDist, false);

                _imageViewer.Invalidate();
            }
        }

        /// <summary>
        /// Ends an interactive tracking event
        /// </summary>
        /// <param name="mouseX">The physical (client) x-coordinate of the mouse cursor.</param>
        /// <param name="mouseY">The physical (client) y-coordinate of the mouse cursor.</param>
        public virtual void EndTracking(int mouseX, int mouseY)
        {
            // Update the tracking one last time
            UpdateTracking(mouseX, mouseY);

            // Set the dirty flag
            this.Dirty = true;

            // Reset the tracking data
            this.TrackingData = null;
        }

        /// <summary>
        /// Determines whether the <see cref="LayerObject"/> is positioned in valid place.
        /// </summary>
        /// <returns><see langword="true"/> if the layer object is positioned in a valid place; 
        /// <see langword="false"/> if the layer object is not positioned in a valid place.
        /// </returns>
        public virtual bool IsValid()
        {
            // Ensure the bounds are the minimum size
            Rectangle bounds = GetBounds();
            if (bounds.Size.Height < _MIN_SIZE.Height || bounds.Size.Width < _MIN_SIZE.Width)
            {
                return false;
            }

            // Ensure at least one pixel is contained on the page
            return _imageViewer.Intersects(bounds);
        }

        /// <summary>
        /// Draws a grip handle at the specified point.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object with which to draw.</param>
        /// <param name="gripPoint">The point in physical (client) coordinates where the grip 
        /// handle should be drawn.</param>
        internal static void DrawGripHandle(Graphics graphics, Point gripPoint)
        {
            // Calculate the grip handle dimensions
            Rectangle gripHandle = Rectangle.FromLTRB(
                gripPoint.X - _HALF_GRIP_HANDLE_SIDE, gripPoint.Y - _HALF_GRIP_HANDLE_SIDE,
                gripPoint.X + _HALF_GRIP_HANDLE_SIDE, gripPoint.Y + _HALF_GRIP_HANDLE_SIDE);

            // Draw the grip handle
            graphics.FillRectangle(Brushes.White, gripHandle);
            gripHandle.Inflate(-1, -1);
            graphics.DrawRectangle(Pens.Black, gripHandle);
        }

        /// <summary>
        /// Draws the grip handles on the specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The graphics with which to draw. Cannot be 
        /// <see langword="null"/>.</param>
        public virtual void DrawGripHandles(Graphics graphics)
        {
            // Do nothing if not on the active page
            if (_imageViewer == null || _pageNumber != _imageViewer.PageNumber)
            {
                return;
            }

            // Get the centers of the grip handles in logical (image) coordinates
            Point[] gripPoints = GetGripPoints();

            // Draw dashed lines around the layer object, if there is not a single grip point
            // NOTE: A widthless highlight will only have one grip handle
            if (gripPoints.Length != 1)
            {
                Point[] vertices = GetVertices();
                _imageViewer.Transform.TransformPoints(vertices);
                graphics.DrawPolygon(ExtractPens.DashedBlack, vertices);
            }

            // If there are no grip points, we are done
            if (gripPoints.Length <= 0)
            {
                return;
            }

            // Convert the grip points to to physical (client) coordinates
            _imageViewer.Transform.TransformPoints(gripPoints);

            // Iterate through each grip center
            for (int i = 0; i < gripPoints.Length; i++)
            {
                LayerObject.DrawGripHandle(graphics, gripPoints[i]);
            }
        }

        /// <summary>
        /// Draws the link arrows on the specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The graphics with which to draw. Cannot be 
        /// <see langword="null"/>.</param>
        public virtual void DrawLinkArrows(Graphics graphics)
        {
            // Do nothing if not on the active page
            if (_imageViewer == null || _pageNumber != _imageViewer.PageNumber)
            {
                return;
            }

            // Get the centers of the link arrows in logical (image) coordinates
            Point[] linkPoints = GetLinkPoints();

            // Draw the left link arrow if necessary
            if (_previousLink != null)
            {
                // Get the x-coordinate of rightside of the link arrow
                int x = linkPoints[0].X + _HALF_LINK_ARROW_SIDE;

                // Calculate the vertices of the link arrow
                Point[] vertices = new Point[]
                {
                    new Point(x, linkPoints[0].Y - _HALF_LINK_ARROW_SIDE),
                    new Point(x, linkPoints[0].Y + _HALF_LINK_ARROW_SIDE),
                    new Point(linkPoints[0].X - _HALF_LINK_ARROW_SIDE, linkPoints[0].Y)
                };

                // Draw the arrow
                graphics.FillPolygon(Brushes.Red, vertices);
                graphics.DrawPolygon(Pens.Black, vertices);
            }

            // Draw the right link arrow if necessary
            if (_nextLink != null)
            {
                // Get the x-coordinate of leftside of the link arrow
                int x = linkPoints[1].X - _HALF_LINK_ARROW_SIDE;

                // Calculate the vertices of the link arrow
                Point[] vertices = new Point[]
                {
                    new Point(x, linkPoints[1].Y - _HALF_LINK_ARROW_SIDE),
                    new Point(x, linkPoints[1].Y + _HALF_LINK_ARROW_SIDE),
                    new Point(linkPoints[1].X + _HALF_LINK_ARROW_SIDE, linkPoints[1].Y)
                };

                // Draw the arrow
                graphics.FillPolygon(Brushes.Red, vertices);
                graphics.DrawPolygon(Pens.Black, vertices);
            }
        }

        /// <summary>
        /// Stores the spatial data associated with the <see cref="LayerObject"/> at the start of 
        /// an interactive tracking event.
        /// </summary>
        public abstract void Store();

        /// <summary>
        /// Restores the spatial data associated with the <see cref="LayerObject"/> to its state at 
        /// the start of an interactive tracking event.
        /// </summary>
        public abstract void Restore();

        /// <summary>
        /// Cancels the interactive tracking event in progress.
        /// </summary>
        public virtual void CancelTracking()
        {
            Restore();
            this.TrackingData = null;
        }

        /// <summary>
        /// Gets the smallest rectangle that contains the <see cref="LayerObject"/> in logical 
        /// (image) coordinates.
        /// </summary>
        /// <returns>The smallest rectangle that contains the <see cref="LayerObject"/> in logical 
        /// (image) coordinates.</returns>
        // The method may perform a calculation, so it is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public abstract Rectangle GetBounds();

        /// <summary>
        /// Gets the smallest rectangle that contains the <see cref="LayerObject"/> in physical 
        /// (client) coordinates.
        /// </summary>
        /// <returns>The smallest rectangle that contains the <see cref="LayerObject"/> in physical 
        /// (client) coordinates.</returns>
        // This method performs a calculation, so is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual Rectangle GetLinkArea()
        {
            // Get the bounds in physical (client) coordinates
            Rectangle linkArea = _imageViewer.GetTransformedRectangle(GetBounds(), false);

            // Ensure the link area contains the height of the link arrow
            int deltaY = _HALF_LINK_ARROW_SIDE * 2 - linkArea.Height;
            if (deltaY > 0)
            {
                linkArea.Offset(0, deltaY / -2);
                linkArea.Height += deltaY;
            }

            // If the there is previous link arrow, expand to the left
            if (this.PreviousLink != null)
            {
                linkArea.Offset(-_LINK_ARROW_EXPAND_DISTANCE, 0);
                linkArea.Width += _LINK_ARROW_EXPAND_DISTANCE;
            }

            // If there is a next link arrow, expand to the right
            if (this.NextLink != null)
            {
                linkArea.Width += _LINK_ARROW_EXPAND_DISTANCE;
            }

            // Expand the link area by one pixel
            linkArea.Inflate(1, 1);

            return linkArea;
        }

        /// <summary>
        /// Gets the center point of the <see cref="LayerObject"/>
        /// in logical (image) coordinates.
        /// </summary>
        /// <returns>The center point for the <see cref="LayerObject"/> in
        /// logical (image) coordinates.</returns>
        // This method performs a calculation, so it is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual Point GetCenterPoint()
        {
            try
            {
                // Get the bounding rectangle
                Rectangle bounds = this.GetBounds();

                // Get the top left of the rectangle
                Point centerPoint = bounds.Location;

                // Offset the point by half the width and height
                centerPoint.Offset(bounds.Width / 2, bounds.Height / 2);

                return centerPoint;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22508", ex);
            }
        }

        /// <summary>
        /// Retrieves the vertices of the <see cref="LayerObject"/> in logical (image) coordinates.
        /// </summary>
        /// <returns>The vertices of the <see cref="LayerObject"/> in logical (image) coordinates.
        /// </returns>
        public abstract Point[] GetVertices();

        /// <summary>
        /// Determines whether a horizontal line can be drawn such that it intersects this 
        /// layer object and the specified layer object.
        /// </summary>
        /// <returns><see langword="true"/> if the a horizontal line can be drawn that intersects 
        /// the <see cref="LayerObject"/> and <paramref name="layerObject"/>; 
        /// <see langword="false"/> if no horizontal line can be drawn that intersects the 
        /// <see cref="LayerObject"/> and <paramref name="layerObject"/>.</returns>
        public bool HorizontallyOverlap(LayerObject layerObject)
        {
            // Get the bounds of both layer objects
            Rectangle myBounds = GetBounds();
            Rectangle yourBounds = layerObject.GetBounds();

            // Determine whether they overlap horizontally
            return yourBounds.Bottom > myBounds.Top && myBounds.Bottom > yourBounds.Top;
        }

        /// <summary>
        /// Determines whether the specified rectangle intersects the <see cref="LayerObject"/>.
        /// </summary>
        /// <param name="rectangle">The rectangle in logical (image) coordinates to check for 
        /// intersection.</param>
        /// <returns><see langword="true"/> if the <paramref name="rectangle"/> intersects the 
        /// <see cref="LayerObject"/>; <see langword="false"/> if the <paramref name="rectangle"/> 
        /// does not intersect the <see cref="LayerObject"/>.</returns>
        public virtual bool IsVisible(Rectangle rectangle)
        {
            // Returns true iff the object is visible and on the currently visible page
            return this.Visible && _imageViewer != null
                && _imageViewer.PageNumber == this.PageNumber;
        }

        /// <summary>
        /// Determines whether the <see cref="LayerObject"/> is completely contained
        /// within the specified <see cref="Rectangle"/> on the specified page.
        /// </summary>
        /// <param name="rectangle">The bounding <see cref="Rectangle"/> in logical (image) 
        /// coordinates.</param>
        /// <param name="pageNumber">The page to check on.</param>
        /// <returns><see langword="true"/> if this <see cref="LayerObject"/> is completely
        /// contained within <paramref name="rectangle"/> on <paramref name="pageNumber"/> and
        /// <see langword="false"/> otherwise.</returns>
        public virtual bool IsContained(Rectangle rectangle, int pageNumber)
        {
            try
            {
                // Return true if this object is completely contained by the specified rectangle
                // on the specified page
                return this.PageNumber == pageNumber
                    && rectangle.Contains(this.GetBounds());
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22487", ex);
            }
        }

        /// <summary>
        /// Adds a link to the specified <see cref="LayerObject"/> to
        /// this <see cref="LayerObject"/>.
        /// </summary>
        /// <param name="newLayerObject">The <see cref="LayerObject"/> to add a link to.
        /// Must not be <see langword="null"/> or a self-reference.</param>
        /// <exception cref="ExtractException">If <paramref name="newLayerObject"/> is
        /// <see langword="null"/>.</exception>
        /// <exception cref="ExtractException">If <paramref name="newLayerObject"/>
        /// <see cref="LayerObject.Id"/> equals this <see cref="LayerObject.Id"/>.</exception>
        public virtual void AddLink(LayerObject newLayerObject)
        {
            try
            {
                // Ensure the object is not null
                ExtractException.Assert("ELI22525", "Object must not be null!",
                    newLayerObject != null);

                // Ensure not a self reference
                ExtractException.Assert("ELI22526", "Object cannot link to itself!",
                    this.Id != newLayerObject.Id);

                // Get the first linked object connected to the new layer object
                LayerObject firstObject = newLayerObject;
                while (firstObject.PreviousLink != null)
                {
                    firstObject = firstObject.PreviousLink;
                }

                // Now build a list of layer objects to link to
                // [DotNetRCAndUtils #210]
                List<LayerObject> objectsToLink = new List<LayerObject>();
                while (firstObject != null)
                {
                    // Ensure we don't try to add a self-reference
                    if (firstObject.Id != this.Id)
                    {
                        objectsToLink.Add(firstObject);
                    }

                    firstObject = firstObject.NextLink;
                }

                // Loop through each of the linked objects connected to the new layer object
                // and add links to each one
                foreach(LayerObject layerObject in objectsToLink)
                {
                    // Find where the new layer object belongs
                    // Check if the new layer object comes before or after this object
                    if (layerObject > this)
                    {
                        // Check if there is a next link yet
                        if (_nextLink == null)
                        {
                            SetNextLink(layerObject);
                            layerObject.SetPreviousLink(this);
                            return;
                        }

                        // Search until we find a next link that is greater than the
                        // new layer object or we find a null next link
                        LayerObject next = _nextLink;
                        while (layerObject > next)
                        {
                            if (next.NextLink == null)
                            {
                                // Found the end, add this layer object
                                next.SetNextLink(layerObject);
                                layerObject.SetPreviousLink(next);
                                return;
                            }

                            next = next.NextLink;
                        }

                        // Found where this object belongs, get the previous object
                        LayerObject previous = next.PreviousLink;

                        // Now insert this object in the middle of the chain
                        previous.SetNextLink(layerObject);
                        layerObject.SetPreviousLink(previous);
                        layerObject.SetNextLink(next);
                        next.SetPreviousLink(layerObject);
                    }
                    else
                    {
                        // Check if there is a previous link yet
                        if (_previousLink == null)
                        {
                            SetPreviousLink(layerObject);
                            layerObject.SetNextLink(this);
                            return;
                        }

                        // Search until we find a previous link that is less than the
                        // new layer object or we find a null previous link
                        LayerObject previous = _previousLink;
                        while (layerObject < previous)
                        {
                            if (previous.PreviousLink == null)
                            {
                                // Found the beginning, add this layer object as the first
                                // link in the chain
                                previous.SetPreviousLink(layerObject);
                                layerObject.SetNextLink(previous);
                                return;
                            }

                            previous = previous.PreviousLink;
                        }

                        // Found where this object belongs, get the next object
                        LayerObject next = previous.NextLink;

                        // Now insert this object in the middle of the chain
                        next.SetPreviousLink(layerObject);
                        layerObject.SetNextLink(next);
                        layerObject.SetPreviousLink(previous);
                        previous.SetNextLink(layerObject);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22527", ex);
            }
        }

        /// <summary>
        /// Removes the links from this <see cref="LayerObject"/> and patches the
        /// links (if necessary) of <see cref="LayerObject"/> that this object is
        /// linked to.
        /// <para>Example:</para>
        /// Before remove B: A &lt;-&gt; B &lt;-&gt; C<para/>
        /// After remove B: A &lt;-&gt; C and
        /// <see langword="null"/> &lt;-&gt; B &lt;-&gt; <see langword="null"/>.
        /// </summary>
        public virtual void RemoveLinks()
        {
            try
            {
                // Check if there is a next link
                if (_nextLink != null)
                {
                    // Set the next links previous to previous link
                    // Note: previous link may be null
                    _nextLink.SetPreviousLink(_previousLink);
                }
                // Check if there is a previous link
                if (_previousLink != null)
                {
                    // Set the previous links next to next link
                    // Note: next link may be null
                    _previousLink.SetNextLink(_nextLink);
                }

                // Set both the next and previous links to null
                _nextLink = null;
                _previousLink = null;
                this.Dirty = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22530", ex);
            }
        }

        /// <summary>
        /// Sets the <see cref="NextLink"/> to point to the specified <see cref="LayerObject"/>.
        /// </summary>
        /// <param name="layerObject">The <see cref="LayerObject"/> to set as the
        /// <see cref="NextLink"/>.</param>
        private void SetNextLink(LayerObject layerObject)
        {
            _nextLink = layerObject;
            this.Dirty = true;
        }

        /// <summary>
        /// Sets the <see cref="PreviousLink"/> to point to the specified <see cref="LayerObject"/>.
        /// </summary>
        /// <param name="layerObject">The <see cref="LayerObject"/> to set as the
        /// <see cref="PreviousLink"/>.</param>
        private void SetPreviousLink(LayerObject layerObject)
        {
            _previousLink = layerObject;
            this.Dirty = true;
        }

        #endregion LayerObject Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="LayerObject"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="LayerObject"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="LayerObject"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_trackingData != null)
                {
                    _trackingData.Dispose();
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region IComparable<LayerObject> Members

        /// <summary>
        /// Compares this <see cref="LayerObject"/> with another <see cref="LayerObject"/>.
        /// </summary>
        /// <param name="other">A <see cref="LayerObject"/> to compare with this
        /// <see cref="LayerObject"/>.</param>
        /// <returns>An <see cref="int"/> that indicates the relative order of the
        /// <see cref="LayerObject"/> objects that are being compared.</returns>
        public virtual int CompareTo(LayerObject other)
        {
            // Check the page number of this object
            int returnVal = this.PageNumber.CompareTo(other.PageNumber);

            // 0 indicates they are on the same page
            if (returnVal == 0)
            {
                // If the layer objects horizontally overlap the leftmost one comes first,
                // otherwise the topmost one comes first
                if (HorizontallyOverlap(other))
                {
                    // Check the left point
                    returnVal = this.Location.X.CompareTo(other.Location.X);

                    // 0 indicates the X values are the same
                    if (returnVal == 0)
                    {
                        // Check the top point
                        returnVal = this.Location.Y.CompareTo(other.Location.Y);
                    }
                }
                else
                {
                    // Check the Top point
                    returnVal = this.Location.Y.CompareTo(other.Location.Y);

                    // 0 indicates the Y value is the same
                    if (returnVal == 0)
                    {
                        // Check the Left point
                        returnVal = this.Location.X.CompareTo(other.Location.X);
                    }
                }
            }

            // 0 indicates top-left is the same
            if (returnVal == 0)
            {
                // Compare the center points
                Point thisCenter = this.GetCenterPoint();
                Point otherCenter = other.GetCenterPoint();

                // Compare top point first
                returnVal = thisCenter.Y.CompareTo(otherCenter.Y);

                // 0 indicates the Y of the center point is the same
                if (returnVal == 0)
                {
                    // Check the X value
                    returnVal = thisCenter.X.CompareTo(otherCenter.X);
                }
            }

            // 0 indicates center point is the same
            if (returnVal == 0)
            {
                // Compare the unique ID's
                returnVal = this.Id.CompareTo(other.Id);
            }

            return returnVal;
        }

        /// <summary>
        /// Checks whether the specified <see cref="object"/> is equal to
        /// this <see cref="LayerObject"/>.
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

            // Check if it is a LayerObject
            LayerObject layerObject = obj as LayerObject;
            if (layerObject == null)
            {
                return false;
            }

            // Check if they are equal
            return this == layerObject;
        }

        /// <summary>
        /// Checks whether the specified <see cref="LayerObject"/> is equal to
        /// this <see cref="LayerObject"/>.
        /// </summary>
        /// <param name="layerObject">The <see cref="LayerObject"/> to compare with.</param>
        /// <returns><see langword="true"/> if the zones are equal and
        /// <see langword="false"/> otherwise.</returns>
        public bool Equals(LayerObject layerObject)
        {
            return this == layerObject;
        }

        /// <summary>
        /// Returns a hashcode for this <see cref="LayerObject"/>.
        /// </summary>
        /// <returns>The hashcode for this <see cref="LayerObject"/>.</returns>
        public override int GetHashCode()
        {
            return _id.GetHashCode(); 
        }

        /// <summary>
        /// Checks whether the two specified <see cref="LayerObject"/> objects
        /// are equal.
        /// </summary>
        /// <param name="layerObject1">A <see cref="LayerObject"/> to compare.</param>
        /// <param name="layerObject2">A <see cref="LayerObject"/> to compare.</param>
        /// <returns><see langword="true"/> if the zones are equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator ==(LayerObject layerObject1, LayerObject layerObject2)
        {
            if (object.ReferenceEquals(layerObject1, layerObject2))
            {
                return true;
            }

            if (((object)layerObject1 == null) || ((object)layerObject2 == null))
            {
                return false;
            }

            return (
                layerObject1.PageNumber == layerObject2.PageNumber
                && layerObject1.Location == layerObject2.Location
                && layerObject1.GetCenterPoint() == layerObject2.GetCenterPoint()
            );
        }

        /// <summary>
        /// Checks whether the two specified <see cref="LayerObject"/> objects
        /// are not equal.
        /// </summary>
        /// <param name="layerObject1">A <see cref="LayerObject"/> to compare.</param>
        /// <param name="layerObject2">A <see cref="LayerObject"/> to compare.</param>
        /// <returns><see langword="true"/> if the zones are not equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator !=(LayerObject layerObject1, LayerObject layerObject2)
        {
            return !(layerObject1 == layerObject2);
        }

        /// <summary>
        /// Checks whether the first specified <see cref="LayerObject"/>
        /// is less than the second specified <see cref="LayerObject"/>.
        /// </summary>
        /// <param name="layerObject1">A <see cref="LayerObject"/> to compare.</param>
        /// <param name="layerObject2">A <see cref="LayerObject"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="layerObject1"/> is less
        /// than <paramref name="layerObject2"/> and <see langword="false"/> otherwise.</returns>
        public static bool operator <(LayerObject layerObject1, LayerObject layerObject2)
        {
            return layerObject1.CompareTo(layerObject2) < 0;
        }

        /// <summary>
        /// Checks whether the first specified <see cref="LayerObject"/>
        /// is greater than the second specified <see cref="LayerObject"/>.
        /// </summary>
        /// <param name="layerObject1">A <see cref="LayerObject"/> to compare.</param>
        /// <param name="layerObject2">A <see cref="LayerObject"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="layerObject1"/> is greater
        /// than <paramref name="layerObject2"/> and <see langword="false"/> otherwise.</returns>
        public static bool operator >(LayerObject layerObject1, LayerObject layerObject2)
        {
            return layerObject1.CompareTo(layerObject2) > 0; 
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// This method is not used.
        /// </summary>
        /// <returns><see langword="null"/></returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates a <see cref="LayerObject"/> from its XML representation.
        /// </summary>
        /// <param name="reader">The stream from which the <see cref="LayerObject"/> is 
        /// deserialized.</param>
        public virtual void ReadXml(XmlReader reader)
        {
            try
            {
                // Read the id and the revision number
                _id = Convert.ToInt64(reader.GetAttribute("Id"), CultureInfo.CurrentCulture);
                _revision = 
                    Convert.ToInt32(reader.GetAttribute("Revision"), CultureInfo.CurrentCulture);
                reader.Read();

                // Set the next id if it is smaller than this one
                if (_nextId <= _id)
                {
                    _nextId = _id + 1;
                }

                // Get the page number
                if (reader.Name != "Page")
                {
                    throw new ExtractException("ELI22796", "Invalid format.");
                }
                _pageNumber = reader.ReadElementContentAsInt();

                // Get the comment
                if (reader.Name != "Comment")
                {
                    throw new ExtractException("ELI22858", "Invalid format.");
                }
                _comment = reader.ReadElementContentAsString();

                // Get the tags
                if (reader.Name != "Tags")
                {
                    throw new ExtractException("ELI22859", "Invalid format.");
                }
                if (!reader.IsEmptyElement)
                {
                    reader.Read();
                    while (reader.Name == "Tag")
                    {
                        _tags.Add(reader.ReadElementContentAsString());
                    }
                }
                reader.Read();

                // Read the links if they exist
                if (reader.Name == "PreviousObjectId")
                {
                    reader.Skip();
                }
                if (reader.Name == "NextObjectId")
                {
                    reader.Skip();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22920", ex);
            }
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The stream to which the <see cref="LayerObject"/> is serialized.
        /// </param>
        public virtual void WriteXml(XmlWriter writer)
        {
            try
            {
                // Write the id and revision number
                writer.WriteAttributeString("Id", 
                    _id.ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("Revision", 
                    _revision.ToString(CultureInfo.CurrentCulture));

                // Write the page number
                writer.WriteElementString("Page", 
                    _pageNumber.ToString(CultureInfo.CurrentCulture));

                // Write the comment
                writer.WriteElementString("Comment", _comment);

                // Write the tags
                writer.WriteStartElement("Tags");
                foreach (string tag in _tags)
                {
                    writer.WriteElementString("Tag", tag);
                }
                writer.WriteEndElement();

                // Write the links
                if (_previousLink != null)
                {
                    writer.WriteElementString("PreviousObjectId", 
                        _previousLink.Id.ToString(CultureInfo.CurrentCulture));
                }
                if (_nextLink != null)
                {
                    writer.WriteElementString("NextObjectId", 
                        _nextLink.Id.ToString(CultureInfo.CurrentCulture));
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22921", ex);
            }
        }

        #endregion IXmlSerializable Members
    }
}
