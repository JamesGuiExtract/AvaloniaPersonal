﻿using Extract.Drawing;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A <see cref="PaginationControl"/> that indicates the divider between the end of one document
    /// and the start of another.
    /// </summary>
    internal partial class PaginationSeparator : PaginationControl
    {
        #region Fields

        /// <summary>
        /// The overall <see cref="Size"/> all <see cref="PaginationSeparator"/>s should be.
        /// </summary>
        static Size? _uniformSize;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationSeparator"/> class.
        /// </summary>
        public PaginationSeparator()
            : base()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35496");
            }
        }

        #endregion Constructors

        #region Static Members

        /// <summary>
        /// Gets the overall <see cref="Size"/> all <see cref="PaginationSeparator"/>s should
        /// be.
        /// </summary>
        /// <value>
        /// The overall <see cref="Size"/> all <see cref="PaginationSeparator"/>s should be.
        /// </value>
        public static Size UniformSize
        {
            get
            {
                try
                {
                    if (_uniformSize == null)
                    {
                        using (var separator = new PaginationSeparator())
                        {
                            _uniformSize = new Size(-1, separator.Height);
                        }
                    }

                    return _uniformSize.Value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35658");
                }
            }
        }

        #endregion Static Members

        #region Overrides

        /// <summary>
        /// Gets or sets whether this control is selected.
        /// </summary>
        /// <value><see langword="true"/> if selected; otherwise, <see langword="false"/>.
        /// </value>
        public override bool Selected
        {
            get
            {
                return base.Selected;
            }

            set
            {
                if (value != base.Selected)
                {
                    base.Selected = value;

                    // Invalidate so that paint occurs and new selection state is indicated.
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Retrieves the size of a rectangular area into which a control can be fitted.
        /// </summary>
        /// <param name="proposedSize">The custom-sized area for a control.</param>
        /// <returns>
        /// An ordered pair of type <see cref="T:System.Drawing.Size"/> representing the width and height of a rectangle.
        /// </returns>
        public override Size GetPreferredSize(Size proposedSize)
        {
            try
            {
                return UniformSize;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35657");
            }

            return base.GetPreferredSize(proposedSize);
        }

        /// <summary>
        /// Paints the background of the control.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the
        /// event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            try
            {
                // Doing all the control painting here prevents flicker when the drop indicator is
                // drawn over this separator.

                // Clears the background of the control.
                var brush = ExtractBrushes.GetSolidBrush(SystemColors.Control);
                Rectangle paintRectangle = ClientRectangle;
                e.Graphics.FillRectangle(brush, paintRectangle);

                // If selected, indicate selection with a darker back color excep except for 1 pixel
                // of border.
                paintRectangle.Inflate(-1, -1);
                if (Selected)
                {
                    brush = ExtractBrushes.GetSolidBrush(SystemColors.ControlDark);
                    e.Graphics.FillRectangle(brush, paintRectangle);
                }

                // Draw the black bar in the middle.
                brush = ExtractBrushes.GetSolidBrush(Color.Black);
                paintRectangle.Inflate(-3, -1);
                e.Graphics.FillRectangle(brush, paintRectangle);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35677");
            }
        }

        #endregion Overrides
    }
}
