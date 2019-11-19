using Extract.Drawing;
using Extract.Imaging;
using Extract.Imaging.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    partial class DataEntryControlHost
    {
        /// <summary>
        /// Manages the positioning of tooltip <see cref="TextLayerObject"/>s in an effort to keep
        /// all tooltips on-page and to keep them from overlapping themselves or the highlights with
        /// which they are associated.
        /// </summary>
        sealed class DataEntryToolTip : IDisposable, IComparable<DataEntryToolTip>
        {
            #region Fields

            /// <summary>
            /// The <see cref="DataEntryControlHost"/> to which the <see cref="DataEntryToolTip"/>
            /// belongs.
            /// </summary>
            readonly DataEntryControlHost _host;

            /// <summary>
            /// The <see cref="IAttribute"/> the <see cref="DataEntryToolTip"/> is associated with.
            /// </summary>
            readonly IAttribute _attribute;

            /// <summary>
            /// Specifies whether <see cref="DataEntryToolTip"/>s should be arranged horizontally or
            /// vertically.
            /// </summary>
            readonly bool _horizontal = true;

            /// <summary>
            /// The smallest bounding rectangle containing the spatial information associated with
            /// this <see cref="DataEntryToolTip"/>'s <see cref="IAttribute"/>.
            /// </summary>
            readonly Rectangle _attributeBounds;

            /// <summary>
            /// The actual <see cref="TextLayerObject"/> managed by this
            /// <see cref="DataEntryToolTip"/> instance.
            /// </summary>
            TextLayerObject _textLayerObject;

            /// <summary>
            /// The page this tooltip is on. 
            /// </summary>
            readonly int _page;

            /// <summary>
            /// The starting x-coordinate of the tooltip when _horizontal, the starting y-coordinate
            /// of the tooltip when !_horizontal. (In ImageViewer coordinates)
            /// </summary>
            int _start;

            /// <summary>
            /// The ending x-coordinate of the tooltip when _horizontal, the ending y-coordinate of
            /// the tooltip when !_horizontal. (In ImageViewer coordinates)
            /// </summary>
            int _end;

            /// <summary>
            /// The x-coordinate marking the midpoint the associated attribute when _horizontal,
            /// the y-coordinate of the midpoint when !_horizontal.
            /// </summary>
            int _highlightCenter;

            /// <summary>
            /// The area (in ImageViewer coordinates) the TextLayerObject must remain within to
            /// ensure visibility of all tooltips and highlights.
            /// </summary>
            Rectangle _maximumExtent;

            /// <summary>
            /// <see langword="true"/> if the tooltip is above the attribute (when _horizontal) or
            /// to the right of the attribute (when !_horizontal). <see langword="false"/> if the
            /// tooltip will appear below or to the left of the attribute due to lack of space on
            /// side 1.
            /// </summary>
            bool _usingSide1 = true;

            /// <summary>
            /// The tooltip that immediately preceeds this one (on the same side as this tooltip).
            /// <see langword="null"/> if this is the first tooltip on this side.
            /// </summary>
            DataEntryToolTip _previousTooltip;

            /// <summary>
            /// The point on the highlight to which the tooltip will be anchored.
            /// </summary>
            Point _highlightAnchorPoint;

            /// <summary>
            /// The factor by which shifts in the x or y dimension of the TextLayerObject's
            /// coordinate system must me multiplied to attain the same x or y movement in the
            /// ImageViewer coordinate system. (To account for skew).
            /// </summary>
            double _shiftProjectionFactor;

            /// <summary>
            /// Specifies the rotation (in degrees, as a multiple of 90) that would best position
            /// the tooltips being drawn "up" in relation to the image coordinates.
            /// </summary>
            int _referenceOrientation;

            /// <summary>
            /// The smallest rectangle containing the TextLayerObject in ImageViewer coordinates.
            /// </summary>
            Rectangle _normalizedBounds;

            /// <summary>
            /// The bounds of the image in ImageViewer coordiantes.
            /// </summary>
            Rectangle _normalizedImageBounds;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new <see cref="DataEntryToolTip"/> instance.
            /// <para><b>Note:</b></para>
            /// Do not add the tooltip to the <see cref="ImageViewer"/> if you will call
            /// <see cref="PositionToolTips"/>. <see cref="PositionToolTips"/> will take care of
            /// adding the tooltip to the <see cref="ImageViewer"/>.
            /// </summary>
            /// <param name="host">The <see cref="DataEntryControlHost"/> to which the
            /// <see cref="DataEntryToolTip"/> belongs.</param>
            /// <param name="attribute">The <see cref="IAttribute"/> the
            /// <see cref="DataEntryToolTip"/> is associated with.</param>
            /// <param name="horizontal"><see langword="true"/> if <see cref="DataEntryToolTip"/>s
            /// should be arranged horizontally; <see langword="false"/> if they should be arranged
            /// vertically.</param>
            /// <param name="attributeBounds">The smallest bounding rectangle containing the spatial
            /// information associated with this <see cref="DataEntryToolTip"/>'s
            /// <see cref="IAttribute"/>. Can be <see langword="null"/>, in which case the
            /// <see cref="DataEntryToolTip"/> will not prevent other tooltips from overlapping this
            /// tooltip's <see cref="IAttribute"/>.
            /// </param>
            public DataEntryToolTip(DataEntryControlHost host, IAttribute attribute,
                bool horizontal, Rectangle? attributeBounds)
            {
                try
                {
                    ExtractException.Assert("ELI26932", "Null argument exception!",
                        host != null);
                    ExtractException.Assert("ELI26933", "Null argument exception!",
                        attribute != null);

                    _host = host;
                    _attribute = attribute;
                    _page = _host.ImageViewer.PageNumber;
                    _attributeBounds = attributeBounds ?? new Rectangle();
                    _horizontal = horizontal;

                    // Provides the original positioning of the tooltip. The original position will
                    // be shifted or flipped to side2 as necessary to keep it on-page.
                    CreateToolTip();
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26926", ex);
                }
            }

            #endregion Constructors

            #region Static Members

            /// <summary>
            /// Repositions the specified <see cref="DataEntryToolTip"/>s with the goal to prevent
            /// the tooltips from overlapping with each other or each other's highlights and keep
            /// them on-page. Also adds the <see cref="TextLayerObject"/> to the
            /// <see cref="ImageViewer"/>.
            /// </summary>
            /// <param name="toolTips">The <see cref="DataEntryToolTip"/>s to be positioned.</param>
            public static void PositionToolTips(List<DataEntryToolTip> toolTips)
            {
                try
                {
                    // Sort the tooltips so that they are in order of their highlight anchor points
                    // in the dimension specified by _horizontal.
                    toolTips.Sort();

                    // Loop through each tooltip to position it. Keep track of the previous tooltip
                    // on each side as well as overall for use in following iterations.
                    DataEntryToolTip lastToolTip = null;
                    DataEntryToolTip lastSide1ToolTip = null;
                    DataEntryToolTip lastSide2ToolTip = null;
                    foreach (DataEntryToolTip toolTip in toolTips)
                    {
                        // If the tooltip hasn't been moved to side2, see if it will fit on side1.
                        if (toolTip._usingSide1)
                        {
                            // If there are no other tooltips on side1, it will fit by default.
                            if (lastSide1ToolTip == null)
                            {
                                lastSide1ToolTip = toolTip;
                            }
                            // Otherwise the last tooltip on side1 will need to determine if there
                            // is room for this tooltip as well.
                            else if (lastSide1ToolTip.TryFitToolTip(toolTip))
                            {
                                lastSide1ToolTip = toolTip;
                            }
                            // If the tooltip could not fit on side1, move it to side2.
                            else
                            {
                                toolTip._usingSide1 = false;
                                toolTip.CreateToolTip();
                            }
                        }

                        if (!toolTip._usingSide1)
                        {
                            // If the tooltip has been moved to side2 ask the previous tooltip on
                            // side2 (if there is one) to fit it in as best as possible.
                            if (lastSide2ToolTip != null)
                            {
                                lastSide2ToolTip.TryFitToolTip(toolTip);
                            }

                            lastSide2ToolTip = toolTip;
                        }

                        // Finally, if the tooltips are positioned vertically, check to see if the
                        // current tooltip is overlapping the last tooltip's attribute (or
                        // vice-versa). If so, shift the tooltip off of the highlight.
                        // TODO: This may shift the tooltip offpage.
                        if (!toolTip._horizontal && lastToolTip != null)
                        {
                            // Check for overlap between the last tooltip and this tooltip's attribute.
                            Rectangle lastToolTipBounds = GeometryMethods.RotateRectangle(
                                lastToolTip._textLayerObject.GetBounds(),
                                toolTip._referenceOrientation, new PointF(0, 0));
                            lastToolTipBounds.Offset(-lastToolTip._normalizedImageBounds.X,
                                -lastToolTip._normalizedImageBounds.Y);
                            Rectangle overlap =
                                Rectangle.Intersect(lastToolTipBounds, toolTip._attributeBounds);

                            // If they overlap, shift the last tooltip enough left or right so that
                            // the entire attribute highlight is visible.
                            if (!overlap.IsEmpty)
                            {
                                lastToolTip.Shift(
                                    lastToolTip._usingSide1 ? overlap.Width : -overlap.Width, true);
                            }

                            // Check for overlap between this tooltip and this the last tooltip's
                            // attribute.
                            Rectangle toolTipBounds = GeometryMethods.RotateRectangle(
                                toolTip._textLayerObject.GetBounds(),
                                toolTip._referenceOrientation, new PointF(0, 0));
                            toolTipBounds.Offset(-toolTip._normalizedImageBounds.X,
                                -toolTip._normalizedImageBounds.Y);
                            overlap =
                                Rectangle.Intersect(toolTipBounds, lastToolTip._attributeBounds);

                            // If they overlap, shift the last tooltip enough left or right so that
                            // the entire attribute highlight is visible.
                            if (!overlap.IsEmpty)
                            {
                                toolTip.Shift(
                                    toolTip._usingSide1 ? overlap.Width : -overlap.Width, true);
                            }
                        }

                        lastToolTip = toolTip;

                        // Once the toolTip is positioned, add it to the ImageViewer.
                        toolTip._host.ImageViewer.LayerObjects.Add(toolTip._textLayerObject);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26927", ex);
                }
            }

            #endregion Static Members

            #region Properties

            /// <summary>
            /// The actual <see cref="TextLayerObject"/> managed by this
            /// <see cref="DataEntryToolTip"/> instance.
            /// </summary>
            /// <returns>The <see cref="TextLayerObject"/> managed by this
            /// <see cref="DataEntryToolTip"/> instance.</returns>
            public TextLayerObject TextLayerObject
            {
                get
                {
                    return _textLayerObject;
                }
            }

            /// <summary>
            /// The smallest rectangle containing the TextLayerObject in ImageViewer coordinates.
            /// </summary>
            public Rectangle NormalizedBounds
            {
                get
                {
                    return _normalizedBounds;
                }
            }

            #endregion Properties

            #region IComparable Members

            /// <summary>
            /// Compares this <see cref="DataEntryToolTip"/> instance with another
            /// <see cref="DataEntryToolTip"/> instance via spatial positioning.
            /// </summary>
            /// <param name="otherToolTip">A <see cref="DataEntryToolTip"/> to compare with this
            /// instance.</param>
            /// <returns>Less than zero if this instance's highlight is to the left of (for
            /// horizontally arranged tooltips) or above (for horizontally arranged tooltips)
            /// <see paramref="otherToolTip"/>'s highlight.
            /// Less than zero if this instance's highlight anchor point is even with
            /// <see paramref="otherToolTip"/>'s highlight.
            /// Greater than zero if this instance's highlight is to the right of (for
            /// horizontally arranged tooltips) or below (for horizontally arranged tooltips)
            /// <see paramref="otherToolTip"/>'s highlight.</returns>
            public int CompareTo(DataEntryToolTip otherToolTip)
            {
                if (_horizontal)
                {
                    return _highlightAnchorPoint.X - otherToolTip._highlightAnchorPoint.X;
                }
                else
                {
                    return _highlightAnchorPoint.Y - otherToolTip._highlightAnchorPoint.Y;
                }
            }

            #endregion IComparable Members

            #region IDisposable Members

            /// <summary>
            /// Releases all resources used by the <see cref="DataEntryToolTip"/>.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases all resources used by the <see cref="DataEntryToolTip"/>.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
            /// resources; <see langword="false"/> to release only unmanaged resources.</param> 
            void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_textLayerObject != null)
                    {
                        // Remove the text layer object from the ImageViewer if necessary.
                        if (_host.ImageViewer != null &&
                            _host.ImageViewer.LayerObjects.Contains(_textLayerObject))
                        {
                            _host.ImageViewer.LayerObjects.Remove(_textLayerObject);
                        }

                        _textLayerObject.Dispose();
                        _textLayerObject = null;
                    }
                }
            }

            #endregion  IDisposable Members

            #region Private Members

            /// <summary>
            /// The distance this <see cref="DataEntryToolTip"/> can be shifted forward (right or
            /// down) and still be on-page (in ImageViewer coordinates).
            /// </summary>
            int ForwardMargin
            {
                get
                {
                    // The forward margin is the difference between the end coordinate and the edge
                    // of the _maximumExtent.
                    if (_horizontal)
                    {
                        return _maximumExtent.Right - _end - GetErrorIconPadding();
                    }
                    else
                    {
                        return _maximumExtent.Bottom - _end;
                    }
                }

                set
                {
                    // Obtain the difference between the current margin and the specified margin.
                    int offset = value - ForwardMargin;

                    // Adjust the _maximumExtent using this offset.
                    if (_horizontal)
                    {
                        _maximumExtent.Width += offset;
                    }
                    else
                    {
                        _maximumExtent.Height += offset;
                    }
                }
            }

            /// <summary>
            /// The distance this <see cref="DataEntryToolTip"/> can be shifted backward (left or
            /// up) and still be on-page and leave room for preceeding
            /// <see cref="DataEntryToolTip"/>(s). (In ImageViewer coordinates)
            /// </summary>
            int BackwardMargin
            {
                get
                {
                    // The forward margin is the difference between the start coordinate and the
                    // edge of the _maximumExtent
                    if (_horizontal)
                    {
                        return _start - _maximumExtent.Left;
                    }
                    else
                    {
                        return _start - _maximumExtent.Top;
                    }
                }

                set
                {
                    // Obtain the difference between the current margin and the specified margin.
                    int offset = value - BackwardMargin;

                    // Adjust the _maximumExtent using this offset.
                    if (_horizontal)
                    {
                        _maximumExtent.X -= offset;
                        _maximumExtent.Width += offset;
                    }
                    else
                    {
                        _maximumExtent.Y -= offset;
                        _maximumExtent.Height += offset;
                    }
                }
            }

            /// <summary>
            /// Creates the associated <see cref="TextLayerObject"/> and positions it to keep it
            /// on-page.  May change _usingSide1 from <see langword="true"/> to
            /// <see langword="false"/> to keep it onpage, but will never change usingSide1 from
            /// <see langword="false"/> to <see langword="true"/> even if it cannot it will extend
            /// off-page using side2.
            /// </summary>
            void CreateToolTip()
            {
                try
                {
                    // Dispose of any previous _textLayerObject. (CreateToolTip will not be called
                    // after the tooltip is added to the ImageViewer, so it does not need to be
                    // removed from the ImageViewer).
                    if (_textLayerObject != null)
                    {
                        _textLayerObject.Dispose();
                        _textLayerObject = null;
                    }

                    // Obtain all the attribute's RasterZones grouped by page.
                    Dictionary<int, List<RasterZone>> rasterZonesByPage =
                        _host.GetAttributeRasterZonesByPage(_attribute, false);

                    // Retrieve the RasterZones for this instance's page.
                    List<RasterZone> pageOfRasterZones;
                    if (!rasterZonesByPage.TryGetValue(_page, out pageOfRasterZones))
                    {
                        throw new ExtractException("ELI26828", "Error creating tooltip!");
                    }

                    // Use the full text of the attribute, not just the text on this page.
                    string toolTipText = _attribute.Value.String;

                    // Initialize variables used to calculate anchor points based on the current
                    // horizontal vs vertical and side1 vs side2 arrangement.
                    AnchorAlignment highlightAnchorAlignment;
                    AnchorAlignment highlightCenterAlignment;
                    AnchorAlignment toolTipAnchorAlignment;
                    double tooltipStandoffAngle;

                    if (_usingSide1)
                    {
                        if (_horizontal)
                        {
                            highlightAnchorAlignment = AnchorAlignment.LeftTop;
                            highlightCenterAlignment = AnchorAlignment.Top;
                            toolTipAnchorAlignment = AnchorAlignment.LeftBottom;
                            tooltipStandoffAngle = 0;
                        }
                        else
                        {
                            highlightAnchorAlignment = AnchorAlignment.Right;
                            highlightCenterAlignment = AnchorAlignment.Right;
                            toolTipAnchorAlignment = AnchorAlignment.Left;
                            tooltipStandoffAngle = 90;
                        }
                    }
                    else
                    {
                        if (_horizontal)
                        {
                            highlightAnchorAlignment = AnchorAlignment.LeftBottom;
                            highlightCenterAlignment = AnchorAlignment.Bottom;
                            toolTipAnchorAlignment = AnchorAlignment.LeftTop;
                            tooltipStandoffAngle = 180;
                        }
                        else
                        {
                            highlightAnchorAlignment = AnchorAlignment.Left;
                            highlightCenterAlignment = AnchorAlignment.Left;
                            toolTipAnchorAlignment = AnchorAlignment.Right;
                            tooltipStandoffAngle = 270;
                        }
                    }

                    // Calculate the initial anchorpoint for the tooltip 
                    _highlightAnchorPoint = GetAnchorPoint(pageOfRasterZones,
                        highlightAnchorAlignment, tooltipStandoffAngle,
                        (int)_host.Config.Settings.TooltipFontSize,
                        out double toolTipRotation, out RectangleF bounds);

                    Point highlightCenterPoint = GetAnchorPoint(bounds, toolTipRotation,
                        highlightCenterAlignment, tooltipStandoffAngle,
                        (int)_host.Config.Settings.TooltipFontSize);

                    // Create the TextLayerObject
                    _textLayerObject = new TextLayerObject(_host.ImageViewer, _page,
                        "ToolTip", toolTipText, _host._toolTipFont, _highlightAnchorPoint,
                        toolTipAnchorAlignment, Color.Yellow, Color.Black, (float)toolTipRotation);

                    _textLayerObject.CanRender = false;

                    // Calculate the _referenceOrientation as the closest 90 degree angle to the
                    // negative of the specified tooltip rotation.
                    _referenceOrientation = - (int)Math.Round(toolTipRotation / 90) * 90;

                    // Initialize position data for use in positioning the tooltip
                    InitializePosition(highlightCenterPoint);

                    if (_usingSide1)
                    {
                        // Ensure there is enough room for the tooltip to remain on-page on side1.
                        // If there is not enough room, move it to side2.
                        if ((_horizontal && _normalizedBounds.Top < _maximumExtent.Top) ||
                            (!_horizontal && _normalizedBounds.Right > _maximumExtent.Right))
                        {
                            _textLayerObject.Dispose();
                            _textLayerObject = null;

                            _usingSide1 = false;

                            CreateToolTip();
                            return;
                        }
                    }

                    // If the forward margin is negative, it is extending off-page. Shift it back
                    // on-page.
                    if (ForwardMargin < 0)
                    {
                        Shift(ForwardMargin);
                    }

                    _textLayerObject.Selectable = false;
                    _textLayerObject.Visible = true;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26928", ex);
                }
            }

            /// <summary>
            /// Initializes position data for use in positioning the tooltip.
            /// </summary>
            /// <param name="highlightCenterPoint"></param>
            void InitializePosition(Point highlightCenterPoint)
            {
                try
                {
                    // Normalize the coordinates of the tooltip bounds and image to image viewer
                    // coordinates.
                    PointF rotationPoint = new PointF(0, 0);
                    _normalizedBounds = GeometryMethods.RotateRectangle(
                        _textLayerObject.GetBounds(), _referenceOrientation, rotationPoint);
                    _normalizedImageBounds = GeometryMethods.RotateRectangle(
                        new Rectangle(0, 0, _host.ImageViewer.ImageWidth, _host.ImageViewer.ImageHeight),
                        _referenceOrientation, rotationPoint);

                    // The normalized page bounds are the initial _maximumExtent for this tooltip.
                    _maximumExtent = _normalizedImageBounds;

                    // [DataEntry:885]
                    EnsureTooltipIsNotWiderThanPage();

                    using (Matrix transform = new Matrix())
                    {
                        // Initialize coordinates that need to be mapped into the ImageViewer
                        // coordinate system.
                        PointF[] cornerPoints = _textLayerObject.GetVertices();
                        Point[] highlightAnchorPoints =
                            { _highlightAnchorPoint, highlightCenterPoint };
                        Point[] coordinateProjectionFactor = new Point[1];

                        // Translate the coordinates into the ImageViewer coordinate system.
                        transform.Rotate(_referenceOrientation);
                        transform.TransformPoints(cornerPoints);
                        transform.TransformPoints(highlightAnchorPoints);

                        ExtractException.Assert("ELI26930",
                            "Cannot create tooltip for empty attribute!",
                            cornerPoints[0].X != cornerPoints[1].X &&
                            cornerPoints[0].Y != cornerPoints[3].Y);

                        // Update the translated anchor point positions.
                        _highlightAnchorPoint = highlightAnchorPoints[0];
                        highlightCenterPoint = highlightAnchorPoints[1];

                        // Initialize the start and end positions based on the transformed corner
                        // points.
                        if (_horizontal)
                        {
                            _start = (int)(cornerPoints[0].X + 0.5F);
                            _end = (int)(cornerPoints[1].X + 0.5F);
                            int height = (int)(cornerPoints[0].Y + 0.5F);
                            coordinateProjectionFactor[0] = Point.Round(cornerPoints[1])
                                - new Size(_start, height);
                        }
                        else
                        {
                            _start = (int)(cornerPoints[0].Y + 0.5F);
                            _end = (int)(cornerPoints[3].Y + 0.5F);
                            int width = (int)(cornerPoints[0].X + 0.5F);
                            coordinateProjectionFactor[0] = Point.Round(cornerPoints[3]) 
                                - new Size(width, _start);
                        }

                        // Perform a new translation to map from the ImageViewer coordinate
                        // system into the TextLayerObject coordinate system.
                        transform.Reset();
                        transform.Rotate(
                            -(_textLayerObject.Orientation + _referenceOrientation));
                        transform.TransformVectors(coordinateProjectionFactor);

                        if (_horizontal)
                        {
                            // Calculate the _shiftProjectionFactor based on how much longer the
                            // horizontal distance is in the TextLayerObject coordinate system over
                            // the projected distance recorded into the ImageViewer coordinate
                            // system.
                            _shiftProjectionFactor = coordinateProjectionFactor[0].X /
                                                     (double)(cornerPoints[1].X - cornerPoints[0].X);

                            _highlightCenter = highlightCenterPoint.X;
                        }
                        else
                        {
                            // Calculate the _shiftProjectionFactor based on how much longer the
                            // horizontal distance is in the TextLayerObject coordinate system over
                            // the projected distance recorded into the ImageViewer coordinate
                            // system.
                            _shiftProjectionFactor = coordinateProjectionFactor[0].Y /
                                                     (double)(cornerPoints[3].Y - cornerPoints[0].Y);

                            _highlightCenter = highlightCenterPoint.Y;
                        }
                    }

                    // Adjust the ForwardMargin to ensure the start of the tooltip will never move
                    // past the midpoint of the attribute's highlight.
                    if (ForwardMargin > (_highlightCenter - _start))
                    {
                        ForwardMargin = (_highlightCenter - _start);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26929", ex);
                }
            }

            /// <summary>
            /// Ensures that no individual tooltip is wider than the image page itself by scaling
            /// down the font if necessary.
            /// </summary>
            void EnsureTooltipIsNotWiderThanPage()
            {
                if (_normalizedBounds.Width > _normalizedImageBounds.Width)
                {
                    using (Graphics graphics = _host.ImageViewer.CreateGraphics())
                    {
                        SizeF imageSizeF = new SizeF(_normalizedImageBounds.Size.Width,
                            _normalizedImageBounds.Size.Height);

                        _textLayerObject.Font = FontMethods.GetFontThatFits(
                            _textLayerObject.Text, graphics, imageSizeF,
                            _textLayerObject.Font.FontFamily, _textLayerObject.Font.Style);
                    }
                }
            }

            /// <summary>
            /// Attempts to fit the specified tooltip into the current side.
            /// </summary>
            /// <param name="nextToolTip">The <see cref="DataEntryToolTip"/> that needs to be fit
            /// along the current side of the <see cref="IAttribute"/> highlights.</param>
            /// <returns><see langword="true"/> if there was enough room to fit
            /// <see paramref="nextToolTip"/> in or it was fit as best as possible;
            /// <see langword="false"/> if there was not enough room and <see paramref="nextToolTip"/>
            /// was not positioned. (If _usingSide2, it will be positioned as best as possible even
            /// if there is not enough room.
            /// </returns>
            bool TryFitToolTip(DataEntryToolTip nextToolTip)
            {
                try
                {
                    // Set the BackwardMargin of nextToolTip to ensure it never moves past the
                    // center point of this tooltip's attribute.
                    nextToolTip.BackwardMargin = nextToolTip._start - _highlightCenter;

                    // Obtain the distance between this tooltip and the next.
                    int separation = GetSeparation(nextToolTip);

                    // If separation is negative, the tooltips are overlapping.
                    if (separation < 0)
                    {
                        int overlap = -separation;

                        // Determine how much space is available to scoot this tooltip backward
                        // without affecting any other tooltips.
                        int freeSpace = GetFreeSpace(nextToolTip);

                        // squeezeRequired is the amount of shifting that needs to take place to fit
                        // nextToolTip that will require tooltips other than this one to move.
                        int squeezeRequired = (overlap - freeSpace);
                        squeezeRequired = (squeezeRequired < 0) ? 0 : squeezeRequired;

                        // If no squeezing is required, simply shift this tooltip to avoid overlap.
                        if (squeezeRequired <= 0)
                        {
                            Shift(-overlap);
                        }
                        else
                        {
                            // Determine how much backward squeeze is available by choosing the
                            // smaller of this tooltip's BackwardMargin and nextToolTip's
                            // BackwardMargin. Don't include the overlap when comparing against
                            // nextToolTip.BackwardMargin since nextToolTip will not shift until the
                            // overlap has been compensated for.
                            int backwardSqueezeAvailable =
                                Math.Min(BackwardMargin, nextToolTip.BackwardMargin + overlap);

                            // Any freeSpace available should not be part of the squeeze available.
                            backwardSqueezeAvailable -= freeSpace;

                            // Determine how much forward squeeze is available by checking the
                            // forward margin of nextToolTip.
                            int forwardSqueezeAvailable = nextToolTip.ForwardMargin;

                            // Combine the two to see how much overall squeeze room is available.
                            int squeezeAvailable =
                                backwardSqueezeAvailable + forwardSqueezeAvailable;

                            // If _usingSide1 and there is not enough room available, don't attempt
                            // to fit it in on this side.
                            if (_usingSide1 && squeezeRequired > squeezeAvailable)
                            {
                                return false;
                            }

                            // Squeeze backward a distance proportional to the amount of backward
                            // squeeze available vs the amount of total squeeze available.
                            int backwardSqueezeAmount = (squeezeAvailable == 0) ? 0 :
                                (squeezeRequired * backwardSqueezeAvailable / squeezeAvailable);

                            // The total backward shift of this tooltip is the amount of freeSpace
                            // plus backward squeeze.
                            int backwardShiftAmount = freeSpace + backwardSqueezeAmount;
                            Shift(-backwardShiftAmount);

                            // The total forward shift of nextToolTip is the remaining overlap that
                            // must be compensated for.
                            int forwardShiftAmount = (overlap - backwardShiftAmount);
                            nextToolTip.Shift(forwardShiftAmount);

                            nextToolTip.ForwardMargin -= forwardShiftAmount;
                        }
                    }

                    // After positioning the nextToolTip, if this tooltip's BackwardMargin combined
                    // with any separation that might exist between the tooltips is less than its
                    // own BackwardMargin, it must be reduced so that it doesn't try to force this
                    // tooltip back further than it can go.
                    int alternateBackwardMargin = BackwardMargin + GetSeparation(nextToolTip);
                    nextToolTip.BackwardMargin =
                        Math.Min(nextToolTip.BackwardMargin, alternateBackwardMargin);

                    // Link nextToolTip.
                    nextToolTip._previousTooltip = this;

                    return true;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26931", ex);
                }
            }

            /// <summary>
            /// Determines how much space is available to scoot this <see cref="DataEntryToolTip"/>
            /// backward without affecting any other tooltips.
            /// </summary>
            /// <param name="nextToolTip">The <see cref="DataEntryToolTip"/> that room is being made
            /// for.</param>
            /// <returns>The distance (in ImageViewer coordinates) that this tooltip can be shifted
            /// backward without impacting any other tooltips.</returns>
            int GetFreeSpace(DataEntryToolTip nextToolTip)
            {
                int freeSpace;

                if (_previousTooltip == null)
                {
                    // If there is no previous tooltip, we can go all the way back to the beginning
                    // of the page.
                    freeSpace = BackwardMargin;
                }
                else
                {
                    // If there is a previous tooltip, we can go back until this tooltip runs up
                    // against the previous one.
                    freeSpace = _previousTooltip.GetSeparation(this);
                }

                int overlap = -GetSeparation(nextToolTip);
                overlap = (overlap < 0) ? 0 : overlap;

                // However, if the nextToolTip has less BackwardMargin available than this one, it
                // becomes the limiting factor on freeSpace that can be used.
                // Don't include the overlap when comparing against nextToolTip.BackwardMargin
                // since nextToolTip will not shift until the overlap has been compensated for.
                freeSpace = Math.Min(freeSpace, nextToolTip.BackwardMargin + overlap);

                return freeSpace < 0 ? 0 : freeSpace;
            }

            /// <summary>
            /// Obtains the distance between this <see cref="DataEntryToolTip"/> and
            /// <see paramref="nextToolTip"/>.
            /// </summary>
            /// <param name="nextToolTip">The <see cref="DataEntryToolTip"/> for which separation is
            /// to be measured.</param>
            /// <returns>The distance between this <see cref="DataEntryToolTip"/> and
            /// <see paramref="nextToolTip"/>.</returns>
            int GetSeparation(DataEntryToolTip nextToolTip)
            {
                return nextToolTip._start - _end - (_horizontal ? GetErrorIconPadding() : 0);
            }

            /// <summary>
            /// Gets the amount of additional width needed for an error icon to be displayed
            /// </summary>
            /// <returns>The size of an error icon and padding if this tooltip will have an error
            /// icon, else 0 if this attribute will not have an error icon</returns>
            int GetErrorIconPadding()
            {
                if (AttributeStatusInfo.GetDataValidity(_attribute) != DataValidity.Invalid ||
                    AttributeStatusInfo.GetHintType(_attribute) == HintType.Indirect)
                {
                    return 0;
                }

                return _host.GetPageIconSize(_page).Width + (int)(_host.Config.Settings.TooltipFontSize * 2);
            }

            /// <summary>
            /// Shifts this <see cref="DataEntryToolTip"/> forward (positive
            /// <see paramref="shiftAmount"/>) or backward (negative <see paramref="shiftAmount"/>).
            /// <para><b>Note:</b></para>
            /// Shifting backward may result in previous tooltips being shifted backward as well.
            /// </summary>
            /// <param name="shiftAmount">The distance along the ImageViewer x-axis (_horizontal) or
            /// the ImageViewer y-axis (!_horizontal) the tooltip should be shifted.</param>
            void Shift(int shiftAmount)
            {
                Shift(shiftAmount, _horizontal);
            }

            /// <summary>
            /// Shifts this <see cref="DataEntryToolTip"/> forward (positive
            /// <see paramref="shiftAmount"/>) or backward (negative <see paramref="shiftAmount"/>).
            /// <para><b>Note:</b></para>
            /// Shifting backward may result in previous tooltips being shifted backward as well.
            /// </summary>
            /// <param name="shiftAmount">The distance along the ImageViewer x-axis or (per
            /// <see paramref="horizontal"/>) the tooltip should be shifted.</param>
            /// <param name="horizontal">If <see langword="true"/>, the tooltip will be shifted
            /// along the x-axis; if <see langword="false"/> it will be shifted along the y-axis.
            /// </param>
            void Shift(int shiftAmount, bool horizontal)
            {
                // The offset needs to be rotated back to be in relation to the tooltip rotation.
                using (Matrix transform = new Matrix())
                {
                    // If shifting in the same axis as _horizontal, update the start and end
                    // positions.
                    if (horizontal == _horizontal)
                    {
                        _start += shiftAmount;
                        _end += shiftAmount;
                        
                        // If necessary, shift the previous tooltip back as well to prevent overlap.
                        if (_previousTooltip != null)
                        {
                            int overlap = -_previousTooltip.GetSeparation(this);

                            if (overlap > 0)
                            {
                                _previousTooltip.Shift(-overlap);
                            }
                        }
                    }

                    // Multiply the shift by _shiftProjectionFactor so that the shift in the
                    // TextLayerObject's coordinate system is large enough to cause the specified
                    // shift amount in the ImageViewer coordinate system.
                    shiftAmount = (int)Math.Round(shiftAmount * _shiftProjectionFactor);

                    // Translate the shift into the TextLayerObject's coordinate system and perform
                    // the shift.
                    Point[] offsetPoint = 
                        { horizontal ? new Point(shiftAmount, 0) : new Point(0, shiftAmount) };
                    transform.Rotate(_textLayerObject.Orientation);
                    transform.TransformVectors(offsetPoint);
                    _textLayerObject.Offset(offsetPoint[0], false);

                    // Recreate the normalized bounds
                    PointF rotationPoint = new PointF(0, 0);
                    _normalizedBounds = GeometryMethods.RotateRectangle(
                        _textLayerObject.GetBounds(), _referenceOrientation, rotationPoint);
                }
            }

            #endregion Private Members
        }
    }
}
