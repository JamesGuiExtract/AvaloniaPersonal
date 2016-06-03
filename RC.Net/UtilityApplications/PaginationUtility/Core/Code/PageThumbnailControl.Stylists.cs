using Extract.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
	/// <summary>
    /// Enables styles to be applied to <see cref="PageThumbnailControl"/>s.
    /// </summary>
    internal abstract class PageStylist
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageStylist"/> class.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> for which this stylist
        /// is responsible.</param>
        protected PageStylist(PageThumbnailControl pageControl)
        {
            PageControl = pageControl;
        }

        /// <summary>
        /// Gets the <see cref="PageThumbnailControl"/> for which this stylist is responsible.
        /// </summary>
        public PageThumbnailControl PageControl
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the tooltip text to display for this stylist.
        /// </summary>
        protected virtual string ToolTipText
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets whether the stylist is visible.
        /// </summary>
        /// <returns><see langword="true"/> if visible.</returns>
        protected abstract bool IsVisible
        {
            get;
        }

        /// <summary>
        /// Gets the area of <see paramref="hostingControl"/> to which the stylist will do any
        /// foreground drawing.
        /// </summary>
        /// <param name="hostingControl">The <see cref="Control"/> on which the stylist will do any
        /// foreground drawing.</param>
        /// <returns>A <see cref="Rectangle"/> describing the area to which the stylist will draw.
        /// </returns>
        protected virtual Rectangle GetDrawRectangle(Control hostingControl)
        {
            return hostingControl.ClientRectangle;
        }

        /// <summary>
        /// Allows the background of the <see cref="PageControl"/> to be painted.
        /// </summary>
        /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
        /// <see cref="PageControl"/>.</param>
        public virtual void PaintBackground(PaintEventArgs e)
        {
        }

        /// <summary>
        /// Allows foreground of the specified <see cref="PageControl"/> to be painted.
        /// </summary>
        /// <param name="paintingControl">The specific <see cref="Control"/> within
        /// <see cref="PageControl"/>being painted.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
        /// <see cref="PageControl"/>.</param>
        public virtual void PaintForeground(Control paintingControl, PaintEventArgs e)
        {
        }
     
        /// <summary>
        /// Gets tooltip text that should be displayed based on the specified
        /// <see paramref="mouseLocation"/> relative to <see paramref="hostingControl"/>.
        /// </summary>
        /// <param name="hostingControl">The child <see cref="Control"/> of
        /// <see cref="PageControl"/> that corresponds to foreground painting and
        /// <see paramref="mouseLocation"/>.</param>
        /// <param name="mouseLocation"></param>
        /// <returns>The tooltip text to display, or <see langword="null"/> if this stylist has no
        /// special tooltip to display.</returns>
        public virtual string GetToolTipText(Control hostingControl, Point mouseLocation)
        {
            try
            {
                if (IsVisible &&
                    !string.IsNullOrWhiteSpace(ToolTipText) &&
                    GetDrawRectangle(hostingControl).Contains(mouseLocation))
                {
                    return ToolTipText;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39850");
            }
        }
    }

    /// <summary>
    /// A <see cref="PageStylist"/> that allows text to be overlaid on top of a
    /// <see cref="PageThumbnailControl"/>.
    /// </summary>
    internal class OverlayTextStylist : PageStylist
    {
        /// <summary>
        /// The <see cref="Color"/> the text is to be.
        /// </summary>
        Color _textColor;

        /// <summary>
        /// The <see cref="Font"/> to be used.
        /// </summary>
        Font _font;

        /// <summary>
        /// The <see cref="StringFormat"/> to be used.
        /// </summary>
        StringFormat _format;

        /// <summary>
        /// Initializes a new instance of the <see cref="OverlayTextStylist"/> class.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> for which this stylist
        /// is responsible.</param>
        /// <param name="text">The text to overlay.</param>
        /// <param name="textColor">The <see cref="Color"/> the text is to be.</param>
        public OverlayTextStylist(PageThumbnailControl pageControl, string text, Color textColor)
            : this(pageControl, text, textColor, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OverlayTextStylist"/> class.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> for which this stylist
        /// is responsible.</param>
        /// <param name="text">The text to overlay.</param>
        /// <param name="textColor">The <see cref="Color"/> the text is to be.</param>
        /// <param name="font">The <see cref="Font"/> to be used.</param>
        /// <param name="format">The <see cref="StringFormat"/> to be used.</param>
        public OverlayTextStylist(PageThumbnailControl pageControl, string text, 
            Color textColor, Font font, StringFormat format)
            : base(pageControl)
        {
            Text = text;
            _textColor = textColor;
            _font = font;
            _format = format;
        }

        /// <summary>
        /// Gets whether the stylist is visible.
        /// </summary>
        /// <returns><see langword="true"/> if visible.
        /// </returns>
        protected override bool IsVisible
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// The text to overlay.
        /// </summary>
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Allows foreground of the PageControl to be painted.
        /// </summary>
        /// <param name="paintingControl">The specific <see cref="Control"/> within
        /// PageControl being painted.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
        /// PageControl.</param>
        public override void PaintForeground(Control paintingControl, PaintEventArgs e)
        {
            try
            {
                if (IsVisible)
                {
                    Rectangle drawRectangle = GetDrawRectangle(paintingControl);

                    var font = _font ?? FontMethods.GetFontThatFits(Text, e.Graphics,
                       drawRectangle.Size, PageControl.Font.FontFamily, PageControl.Font.Style);

                    var brush = ExtractBrushes.GetSolidBrush(_textColor);

                    StringFormat format = _format ?? new StringFormat()
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    // Hack fix for ModifiedPageStylist. The asterisk seems to insist to draw to
                    // high (50% above the top of drawRectangle).
                    if (Text == "*")
                    {
                        drawRectangle.Offset(0, 6);
                        drawRectangle.Inflate(0, 6);
                    }
                    e.Graphics.DrawString(Text, font, brush, drawRectangle, format);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39672");
            }
        }
    }

    /// <summary>
    /// A <see cref="PageStylist"/> that overlays the word "COPY" on any
    /// <see cref="PageThumbnailControl"/> instances that reference the same page as another.
    /// </summary>
    internal class CopiedPageStylist : OverlayTextStylist
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CopiedPageStylist"/> class.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> for which this stylist
        /// is responsible.</param>
        public CopiedPageStylist(PageThumbnailControl pageControl)
            : base(pageControl, "COPY", Color.FromArgb(99, Color.DarkGreen))
        {
        }

        /// <summary>
        /// Gets whether the stylist is visible.
        /// </summary>
        /// <returns><see langword="true"/> if visible.
        /// </returns>
        protected override bool IsVisible
        {
            get
            {
                return PageControl.Page.MultipleCopiesExist;
            }
        }
    }

    /// <summary>
    /// A <see cref="PageStylist"/> that uses a red asterisk to indicated pages that have been
    /// "modified" (that are part of a pending <see cref="OutputDocument"/> that does not match
    /// the document as it currently exists on disk).
    /// </summary>
    internal class ModifiedPageStylist : OverlayTextStylist
    {
        /// <summary>
        /// The <see cref="Font"/> to use to draw the asterisk.
        /// </summary>
        static readonly Font _INDICATOR_FONT = new Font("Sans Serif", 25);

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiedPageStylist"/> class.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> for which this stylist
        /// is responsible.</param>
        public ModifiedPageStylist(PageThumbnailControl pageControl)
            : base(pageControl, "*", Color.Red, _INDICATOR_FONT, null)
        {
        }

        /// <summary>
        /// Gets the tooltip text to display for this stylist.
        /// </summary>
        protected override string ToolTipText
        {
            get
            {
                return "Manual pagination has been applied; reprocessing required.";
            }
        }

        /// <summary>
        /// Gets whether the stylist is visible
        /// </summary>
        /// <returns><see langword="true"/> if visible.</returns>
        protected override bool IsVisible
        {
            get
            {
                return !PageControl.Document.InOriginalForm;
            }
        }

        /// <summary>
        /// Gets the area of <see paramref="hostingControl"/> to which the stylist will do any
        /// foreground drawing.
        /// </summary>
        /// <param name="hostingControl">The <see cref="Control"/> on which the stylist will do any
        /// foreground drawing.</param>
        /// <returns>A <see cref="Rectangle"/> describing the area to which the stylist will draw.
        /// </returns>
        protected override Rectangle GetDrawRectangle(Control hostingControl)
        {
            // Top icon
            Size size = new Size(16, 16);
            Point location = new Point(hostingControl.ClientRectangle.Right - size.Width - 3, 4);
            return new Rectangle(location, size);
        }
    }

    /// <summary>
    /// A <see cref="PageStylist"/> that uses a green page number to indicate pages that will be
    /// output as part of a new document.
    /// </summary>
    internal class NewOutputPageStylist : OverlayTextStylist
    {
        /// <summary>
        /// The <see cref="Font"/> to use to draw the page number.
        /// </summary>
        static readonly Font _INDICATOR_FONT = new Font("Sans Serif", 15);

        /// <summary>
        /// The width of the currently displayed <see cref="OverlayTextStylist.Text"/>.
        /// </summary>
        int _textWidth;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewOutputPageStylist"/> class.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> for which this stylist
        /// is responsible.</param>
        public NewOutputPageStylist(PageThumbnailControl pageControl)
            : base(pageControl, "+", Color.Green, _INDICATOR_FONT, null)
        {
        }

        /// <summary>
        /// Gets the tooltip text to display for this stylist.
        /// </summary>
        protected override string ToolTipText
        {
            get
            {
                return "This page will be output as part of a new document.";
            }
        }

        /// <summary>
        /// Gets whether the stylist is visible.
        /// </summary>
        /// <returns><see langword="true"/> if visible.</returns>
        protected override bool IsVisible
        {
            get
            {
                return !PageControl.Document.InSourceDocForm;
            }
        }

        /// <summary>
        /// Gets the area of <see paramref="hostingControl"/> to which the stylist will do any
        /// foreground drawing.
        /// </summary>
        /// <param name="hostingControl">The <see cref="Control"/> on which the stylist will do any
        /// foreground drawing.</param>
        /// <returns>A <see cref="Rectangle"/> describing the area to which the stylist will draw.
        /// </returns>
        protected override Rectangle GetDrawRectangle(Control hostingControl)
        {
            // 2nd icon to top
            Size size = new Size(_textWidth, 16);
            Point location = new Point(hostingControl.ClientRectangle.Right - size.Width - 4, size.Height + 4);
            return new Rectangle(location, size);
        }

        /// <summary>
        /// Allows foreground of the PageControl to be painted.
        /// </summary>
        /// <param name="paintingControl">The specific <see cref="Control"/> within
        /// PageControl being painted.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
        /// PageControl.</param>
        public override void PaintForeground(Control paintingControl, PaintEventArgs e)
        {
            try
            {
                Text = PageControl.PageNumber.ToString(CultureInfo.InvariantCulture);

                _textWidth = (int)Math.Max(16,
                    e.Graphics.MeasureString(Text, _INDICATOR_FONT).Width + 1);

                base.PaintForeground(paintingControl, e);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39999");
            }
        }
    }

    /// <summary>
    /// A <see cref="PageStylist"/> that indicates a document page that is part of the only
    /// <see cref="OutputDocument"/> for which pages are currently selected.
    /// </summary>
    internal class SelectedDocumentStylist : PageStylist
    {
        /// <summary>
        /// The <see cref="PageThumbnailControl"/> instances that have been painted with document
        /// selection.
        /// </summary>
        HashSet<PageThumbnailControl> _paintedPages = new HashSet<PageThumbnailControl>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedDocumentStylist"/> class.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> for which this stylist
        /// is responsible.</param>
        public SelectedDocumentStylist(PageThumbnailControl pageControl)
            : base(pageControl)
        {
        }

        /// <summary>
        /// Gets whether the stylist is visible.
        /// </summary>
        /// <returns><see langword="true"/> if visible.</returns>
        protected override bool IsVisible
        {
            get
            {
                var selectedDocs = PageControl.PageLayoutControl.PartiallySelectedDocuments;

                return (selectedDocs.Count() == 1 &&
                    selectedDocs.Single() == PageControl.Document);
            }
        }

        /// <summary>
        /// Allows the background of the specified PageControl to be painted.
        /// </summary>
        /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
        /// PageControl.</param>
        public override void PaintBackground(PaintEventArgs e)
        {
            try
            {
                if (IsVisible)
                {
                    var color = Color.FromArgb(50, SystemColors.Highlight);
                    var brush = ExtractBrushes.GetSolidBrush(color);

                    e.Graphics.FillRectangle(brush, e.ClipRectangle);

                    _paintedPages.Add(PageControl);
                }
                else if (_paintedPages.Contains(PageControl))
                {
                    // Invalidate pages that were previously painted so that their appearance as
                    // part of a selected document is cleared when appropriate.
                    foreach (var paintedPage in _paintedPages)
                    {
                        paintedPage.Invalidate();
                    }

                    _paintedPages.Clear();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39675");
            }
        }
    }
}
