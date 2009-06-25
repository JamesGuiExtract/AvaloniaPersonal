using Extract.Drawing;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Specifies a set of anchor points.
    /// </summary>
    public enum AnchorAlignment
    {
        /// <summary>
        /// The left-bottom point.
        /// </summary>
        LeftBottom,

        /// <summary>
        /// The center-bottom point.
        /// </summary>
        Bottom,

        /// <summary>
        /// The bottom-right point.
        /// </summary>
        RightBottom,

        /// <summary>
        /// The left-center point.
        /// </summary>
        Left,

        /// <summary>
        /// The center point.
        /// </summary>
        Center,

        /// <summary>
        /// The right-center point.
        /// </summary>
        Right,

        /// <summary>
        /// The left-top point.
        /// </summary>
        LeftTop,

        /// <summary>
        /// The center-top point.
        /// </summary>
        Top,

        /// <summary>
        /// The right top point.
        /// </summary>
        RightTop
    }

    /// <summary>
    /// Provides data for the <see cref="AnchorAlignmentControl.AnchorAlignmentChanged"/> event.
    /// </summary>
    public class AnchorAlignmentChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The anchor alignment that changed.
        /// </summary>
        private readonly AnchorAlignment _anchorAlignment;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorAlignmentChangedEventArgs"/> class.
        /// </summary>
        /// <param name="anchorAlignment">The anchor alignment that changed.</param>
        public AnchorAlignmentChangedEventArgs(AnchorAlignment anchorAlignment)
        {
            _anchorAlignment = anchorAlignment;
        }

        /// <summary>
        /// Gets the anchor alignment that changed.
        /// </summary>
        /// <returns>The anchor alignment that changed.</returns>
        public AnchorAlignment AnchorAlignment
        {
            get
            {
                return _anchorAlignment;
            }
        }
    }

    /// <summary>
    /// Represents a <see cref="Control"/> that allows the user to select one of nine anchor 
    /// points relative to text.
    /// </summary>
    public partial class AnchorAlignmentControl : Control
    {
        #region AnchorAlignmentControl Constants

        /// <summary>
        /// The number of client pixels in the radius of one anchor point.
        /// </summary>
        readonly static int _ANCHOR_POINT_RADIUS = 3;

        /// <summary>
        /// The number of client pixels in the diameter of one anchor point.
        /// </summary>
        readonly static int _ANCHOR_POINT_DIAMETER = _ANCHOR_POINT_RADIUS * 2;

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(AnchorAlignmentControl).ToString();

        #endregion AnchorAlignmentControl Constants

        #region AnchorAlignmentControl Fields

        /// <summary>
        /// The currently selected anchor alignment
        /// </summary>
        AnchorAlignment _anchorAlignment = AnchorAlignment.Center;

        /// <summary>
        /// An array of the smallest rectangles that contain the anchor points.
        /// </summary>
        RectangleF[] _anchorPoints;

        #endregion AnchorAlignmentControl Fields

        #region AnchorAlignmentControl Events

        /// <summary>
        /// Occurs when anchor alignment changes.
        /// </summary>
        public event EventHandler<AnchorAlignmentChangedEventArgs> AnchorAlignmentChanged;

        #endregion AnchorAlignmentControl Events

        #region AnchorAlignmentControl Constructors

        /// <summary>
        /// Initializes a new <see cref="AnchorAlignmentControl"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public AnchorAlignmentControl()
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel()); 
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23135",
                    _OBJECT_NAME); 

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23136", ex);
            }
        }

        #endregion AnchorAlignmentControl Constructors

        #region AnchorAlignmentControl Properties

        /// <summary>
        /// Gets or sets the selected anchor alignment.
        /// </summary>
        /// <value>The selected anchor alignment.</value>
        /// <returns>The selected anchor alignment.</returns>
        [DefaultValue(AnchorAlignment.Center)]
        public AnchorAlignment AnchorAlignment
        {
            get
            {
                return _anchorAlignment;
            }
            set
            {
                try
                {
                    // Erase the previous selection
                    base.Invalidate(Rectangle.Round(this.AnchorPoint));

                    // Store the new selection
                    _anchorAlignment = value;

                    // Redraw the new selection
                    base.Invalidate(Rectangle.Round(this.AnchorPoint));

                    // Raise the AnchorAlignmentChanged event
                    OnAnchorAlignmentChanged(new AnchorAlignmentChangedEventArgs(_anchorAlignment));
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26496", ex);
                }
            }
        }
        
        /// <summary>
        /// Gets the selected anchor point.
        /// </summary>
        /// <value>The selected anchor point.</value>
        public RectangleF AnchorPoint
        {
            get 
            { 
                return _anchorPoints[(int)_anchorAlignment];
            }
        }

        #endregion AnchorAlignmentControl Properties

        #region AnchorAlignmentControl Methods

        /// <summary>
        /// Gets the smallest rectangles that contain the anchor points in client coordinates.
        /// </summary>
        /// <returns>The smallest rectangles that contain the anchor points in client coordinates.
        /// </returns>
        protected RectangleF[] GetAnchorPoints()
        {
            // Get the area upon which the left-top coordinates of the anchor points fall.
            // Note: For some unknown reason one pixel needs to be added for the control to 
            // render properly, otherwise the bottom and right side are cut off by one pixel.
            Size anchorPointArea = base.ClientSize;
            anchorPointArea.Width -= _ANCHOR_POINT_DIAMETER + 1;
            anchorPointArea.Height -= _ANCHOR_POINT_DIAMETER + 1;

            // Calculate the x and y coordinates of the anchor points
            float[] x = new float[] 
            {
                0,
                anchorPointArea.Width / 2.0F,
                anchorPointArea.Width
            };
            float[] y = new float[]
            {
                anchorPointArea.Height,
                anchorPointArea.Height / 2.0F,
                0
            };

            // Create the anchor points
            RectangleF[] anchorPoints = new RectangleF[9];
            for (int j = 0; j < y.Length; j++)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    anchorPoints[i + j * y.Length] =
                        new RectangleF(x[i], y[j], _ANCHOR_POINT_DIAMETER, _ANCHOR_POINT_DIAMETER);
                }
            }

            return anchorPoints;
        }

        /// <summary>
        /// Gets the area in which text is drawn in client coordinates.
        /// </summary>
        /// <returns>The area in which text is drawn in client coordinates.</returns>
        // This method performs a computation, so is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected Rectangle GetTextArea()
        {
            // Compute the text area by applying the padding and 
            // shrinking by the anchor point diameter
            // Note: For an unknown reason the ClientRectangle is off by one pixel.
            Rectangle textArea = base.ClientRectangle;
            textArea.X += _ANCHOR_POINT_RADIUS + base.Padding.Left;
            textArea.Y += _ANCHOR_POINT_RADIUS + base.Padding.Top;
            textArea.Width -= _ANCHOR_POINT_DIAMETER + base.Padding.Horizontal - 1;
            textArea.Height -= _ANCHOR_POINT_DIAMETER + base.Padding.Vertical - 1;

            return textArea;
        }

        /// <summary>
        /// Get the rectangle that is drawn on the control in client coordinates.
        /// </summary>
        /// <returns>The rectangle that is drawn on the control in client coordinates.</returns>
        private Rectangle GetRectangleArea()
        {
            // Get the area of the control
            Rectangle rectangle = GetClientArea();

            // Deflate the rectangle by the radius of the anchor points
            rectangle.Inflate(-_ANCHOR_POINT_RADIUS, -_ANCHOR_POINT_RADIUS);
            return rectangle;
        }

        /// <summary>
        /// Gets the area of the control on which pixels can be drawn in client coordinates.
        /// </summary>
        /// <returns>The area of the control on which pixels can be drawn in client coordinates.
        /// </returns>
        Rectangle GetClientArea()
        {
            Rectangle rectangle = base.ClientRectangle;

            // Shrink the rectangle by one pixel. Every example I could find indicated that 
            // the ClientRectangle should be sufficient to define the drawing area, but drawing a 
            // rectangle with just the ClientRectangle does not draw the bottom or right side.
            rectangle.Height -= 1;
            rectangle.Width -= 1;

            return rectangle;
        }

        /// <summary>
        /// Determines whether the specified key is a regular input key or a special key that 
        /// requires preprocessing. 
        /// </summary>
        /// <param name="keyData">The value of the key.</param>
        /// <returns><see langword="true"/> if the specified key is a regular input key; 
        /// otherwise, <see langword="false"/>.</returns>
        protected override bool IsInputKey(Keys keyData)
        {
            // Arrow keys are input keys.
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:

                    return true;
            }

            // Let the base class determine if this is an input key
            return base.IsInputKey(keyData);
        }

        #endregion AnchorAlignmentControl Methods

        #region AnchorAlignmentControl OnEvents

        /// <summary>
        /// Raises the <see cref="Control.Paint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event 
        /// data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw a white rectangle with a black outline
            Rectangle rectangle = GetRectangleArea();
            e.Graphics.FillRectangle(Brushes.White, rectangle);
            e.Graphics.DrawRectangle(Pens.Black, rectangle);

            // Draw the text
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            e.Graphics.DrawString(base.Text, base.Font, Brushes.Black, GetTextArea(), stringFormat);

            // Draw the anchor points
            AnchorAlignment i = 0;
            foreach (RectangleF anchorPoint in _anchorPoints)
            {
                e.Graphics.FillEllipse(_anchorAlignment == i ? Brushes.Red : Brushes.White, anchorPoint);
                e.Graphics.DrawEllipse(Pens.Black, anchorPoint);

                // Increment the counter
                i++;
            }

            // If the control is in focus draw a dashed border around it.
            if (base.Focused)
            {
                e.Graphics.DrawRectangle(ExtractPens.DottedBlack, GetClientArea());
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.KeyDown"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Control.KeyDown"/> 
        /// event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.KeyData & e.KeyCode)
            {
                case Keys.Up:

                    // Ensure the anchor alignment is not already the top row
                    if (_anchorAlignment < AnchorAlignment.LeftTop)
                    {
                        // Move up one row
                        this.AnchorAlignment = _anchorAlignment + 3;
                    }
                    break;

                case Keys.Down:

                    // Ensure the anchor alignment is not already the bottom row
                    if (_anchorAlignment > AnchorAlignment.RightBottom)
                    {
                        // Move down one row
                        this.AnchorAlignment = _anchorAlignment - 3;
                    }
                    break;

                case Keys.Left:

                    // Ensure the anchor alignment is not the left column
                    if ((int)_anchorAlignment % 3 != 0)
                    {
                        // Move left one column
                        this.AnchorAlignment = _anchorAlignment - 1;
                    }
                    break;

                case Keys.Right:

                    // Ensure the anchor alignment is not already the right column
                    if ((int)_anchorAlignment % 3 != 2)
                    {
                        // Move right one column
                        this.AnchorAlignment = _anchorAlignment + 1;
                    }
                    break;
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.SizeChanged"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Control.SizeChanged"/>.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            _anchorPoints = GetAnchorPoints();
        }

        /// <summary>
        /// Raises the <see cref="AnchorAlignmentChanged"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the 
        /// <see cref="AnchorAlignmentChanged"/>.</param>
        protected virtual void OnAnchorAlignmentChanged(AnchorAlignmentChangedEventArgs e)
        {
            if (AnchorAlignmentChanged != null)
            {
                AnchorAlignmentChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Control.TextChanged"/>.
        /// </param>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            // Invalidate the text area
            base.Invalidate(GetTextArea());
        }

        /// <summary>
        /// Raises the <see cref="Control.Enter"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Control.Enter"/> 
        /// event.</param>
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);

            // Invalidate the control
            base.Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="Control.Leave"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Control.Leave"/> 
        /// event.</param>
        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);

            // Invalidate the control
            base.Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseMove"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Control.MouseMove"/>.
        /// </param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Set the cursor to the hand if the mouse is over an anchor point
            foreach (RectangleF anchorPoint in _anchorPoints)
            {
                if (anchorPoint.Contains(e.Location))
                {
                    base.Cursor = Cursors.Hand;
                    return;
                }
            }

            // Reset to the default cursor
            base.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseClick"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Control.MouseClick"/>.
        /// </param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            // Set the focus if necessary
            if (!base.Focused)
            {
                base.Focus();
            }

            // Check if the mouse clicked on an anchor point
            for (int i = 0; i < _anchorPoints.Length; i++)
			{
                if (_anchorPoints[i].Contains(e.Location))
                {
                    // Select this anchor point
                    this.AnchorAlignment = (AnchorAlignment) i;
                    return;
                }
            }
        }

        #endregion AnchorAlignmentControl OnEvents
    }
}
