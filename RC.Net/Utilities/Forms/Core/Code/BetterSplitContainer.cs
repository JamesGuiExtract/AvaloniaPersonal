using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using Extract.Drawing;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a <see cref="SplitContainer"/> with extended functionality.
    /// </summary>
    public partial class BetterSplitContainer : SplitContainer
    {
        #region Fields

        /// <summary>
        /// The <see cref="Color"/> the splitter bar should be painted.
        /// </summary>
        Color _splitterColor = SystemColors.ControlLight;

        /// <summary>
        /// The <see cref="Color"/> the grip dots should be painted.
        /// </summary>
        Color _dotColor = SystemColors.ControlDark;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="BetterSplitContainer"/> class.
        /// </summary>
        public BetterSplitContainer()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="Color"/> the splitter bar should be painted.
        /// </summary>
        /// <value>The <see cref="Color"/> the splitter bar should be painted.</value>
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "ControlLight")]
        [Description("")]
        public Color SplitterColor
        {
            get
            {
                return _splitterColor;
            }
            set
            {
                _splitterColor = value;
            }
        }

        /// <summary>
        /// Gets or sets The <see cref="Color"/> the grip dots should be painted.
        /// </summary>
        /// <value>The <see cref="Color"/> the grip dots should be painted.</value>
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "ControlDark")]
        [Description("")]
        public Color DotColor
        {
            get
            {
                return _dotColor;
            }
            set
            {
                _dotColor = value;
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.SizeChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            try
            {
                base.OnSizeChanged(e);

                // Invalidate when the size is changed, otherwise the splitter bar does not always
                // end up being repainted.
                Invalidate();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36778");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the
        /// event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                // No need to paint the splitter bar if one or both panes are collapsed.
                if (!Panel1Collapsed && !Panel2Collapsed)
                {
                    e.Graphics.FillRectangle(ExtractBrushes.GetSolidBrush(_splitterColor), SplitterRectangle);

                    // Don't paint grip dots if the SplitterWidth is too narrow.
                    if (SplitterWidth >= 2)
                    {
                        // The dot size and spacing should be based on the SplitterWidth
                        Size dotSize = new Size(SplitterWidth - 1, SplitterWidth - 1);
                        int dotSpacing = (SplitterWidth - 1) * 3;
                        var dotLocation = (Orientation == Orientation.Horizontal)
                            ? new Point((Left + Right) / 2 - dotSpacing, SplitterDistance)
                            : new Point(SplitterDistance, (Top + Bottom) / 2 - dotSpacing);
                        var dotRect = new Rectangle(dotLocation, dotSize);

                        // Loop 3 times, once to draw each dot.
                        for (int i = 0; i < 3; i++)
                        {
                            // Need to both draw and fill because just filling seems to results in
                            // squared off circles.
                            e.Graphics.DrawEllipse(ExtractPens.GetPen(_dotColor), dotRect);
                            e.Graphics.FillEllipse(ExtractBrushes.GetSolidBrush(_dotColor), dotRect);

                            if (Orientation == Orientation.Horizontal)
                            {
                                dotRect.Offset(dotSpacing, 0);
                            }
                            else
                            {
                                dotRect.Offset(0, dotSpacing);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36779");
            }
        }

        #endregion Overrides
    }
}
