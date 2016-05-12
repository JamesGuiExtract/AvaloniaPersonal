using Extract.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        /// Allows the background of the specified <see paramref="pageControl"/> to be painted.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> to paint.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
        /// <see paramref="pageControl"/>.</param>
        public virtual void PaintBackground(PageThumbnailControl pageControl, PaintEventArgs e)
        {
        }

        /// <summary>
        /// Allows foreground of the specified <see paramref="pageControl"/> to be painted.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> to paint.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
        /// <see paramref="pageControl"/>.</param>
        public virtual void PaintForeground(PageThumbnailControl pageControl, PaintEventArgs e)
        {
        }
    }

    /// <summary>
    /// A <see cref="PageStylist"/> that allows text to be overlaid on top of a
    /// <see cref="PageThumbnailControl"/>.
    /// </summary>
    internal class OverlayTextStylist : PageStylist
    {
        /// <summary>
        /// The text to overlay.
        /// </summary>
        string _text;

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
        /// <param name="text">The text to overlay.</param>
        /// <param name="textColor">The <see cref="Color"/> the text is to be.</param>
        public OverlayTextStylist(string text, Color textColor)
            : this(text, textColor, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OverlayTextStylist"/> class.
        /// </summary>
        /// <param name="text">The text to overlay.</param>
        /// <param name="textColor">The <see cref="Color"/> the text is to be.</param>
        /// <param name="font">The <see cref="Font"/> to be used.</param>
        /// <param name="format">The <see cref="StringFormat"/> to be used.</param>
        public OverlayTextStylist(string text, Color textColor, Font font, StringFormat format)
        {
            _text = text;
            _textColor = textColor;
            _font = font;
            _format = format;
        }

        /// <summary>
        /// Allows foreground of the specified <see paramref="pageControl"/> to be painted.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> to paint.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
        /// <see paramref="pageControl"/>.</param>
        public override void PaintForeground(PageThumbnailControl pageControl, PaintEventArgs e)
        {
            try
            {
                var font = _font ?? FontMethods.GetFontThatFits(_text, e.Graphics,
                    e.ClipRectangle.Size, pageControl.Font.FontFamily, pageControl.Font.Style);

                var brush = ExtractBrushes.GetSolidBrush(_textColor);

                StringFormat format = _format ?? new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                e.Graphics.DrawString(_text, font, brush, e.ClipRectangle, format);
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
        public CopiedPageStylist()
            : base("COPY", Color.FromArgb(99, Color.DarkGreen))
        {
        }

        /// <summary>
        /// Allows foreground of the specified <see paramref="pageControl"/> to be painted.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> to paint.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
        /// <see paramref="pageControl"/>.</param>
        public override void PaintForeground(PageThumbnailControl pageControl, PaintEventArgs e)
        {
            try
            {
                if (pageControl.Page.MultipleCopiesExist)
                {
                    base.PaintForeground(pageControl, e);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39673");
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
        public ModifiedPageStylist()
            : base("*", Color.Red, _INDICATOR_FONT, new StringFormat()
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Near
            })
        {
        }

        /// <summary>
        /// Allows foreground of the specified <see paramref="pageControl"/> to be painted.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> to paint.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
        /// <see paramref="pageControl"/>.</param>
        public override void PaintForeground(PageThumbnailControl pageControl, PaintEventArgs e)
        {
            try
            {
                if (!pageControl.Document.InOriginalForm)
                {
                    base.PaintForeground(pageControl, e);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39735");
            }
        }
    }

    /// <summary>
    /// A <see cref="PageStylist"/> that uses a red asterisk to indicated pages that have been
    /// "modified" (that are part of a pending <see cref="OutputDocument"/> that does not match
    /// the document as it currently exists on disk).
    /// </summary>
    internal class NewOutputPageStylist : OverlayTextStylist
    {
        /// <summary>
        /// The <see cref="Font"/> to use to draw the asterisk.
        /// </summary>
        static readonly Font _INDICATOR_FONT = new Font("Sans Serif", 21);

        /// <summary>
        /// Initializes a new instance of the <see cref="NewOutputPageStylist"/> class.
        /// </summary>
        public NewOutputPageStylist()
            : base("+", Color.Green, _INDICATOR_FONT, new StringFormat()
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Near
            })
        {
        }

        /// <summary>
        /// Allows foreground of the specified <see paramref="pageControl"/> to be painted.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> to paint.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
        /// <see paramref="pageControl"/>.</param>
        public override void PaintForeground(PageThumbnailControl pageControl, PaintEventArgs e)
        {
            try
            {
                if (!pageControl.Document.InSourceDocForm)
                {
                    var modifiedClipRectangle = e.ClipRectangle;
                    modifiedClipRectangle.Height -= 12;
                    modifiedClipRectangle.Location = new Point(
                        e.ClipRectangle.X, e.ClipRectangle.Y + 12);

                    var newArgs = new PaintEventArgs(e.Graphics, modifiedClipRectangle);
                    base.PaintForeground(pageControl, newArgs);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39674");
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
        /// Allows the background of the specified <see paramref="pageControl"/> to be painted.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> to paint.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
        /// <see paramref="pageControl"/>.</param>
        public override void PaintBackground(PageThumbnailControl pageControl, PaintEventArgs e)
        {
            try
            {
                var selectedDocs = pageControl.PageLayoutControl.PartiallySelectedDocuments;

                if (selectedDocs.Count() == 1 &&
                    selectedDocs.Single() == pageControl.Document)
                {
                    var color = Color.FromArgb(50, SystemColors.Highlight);
                    var brush = ExtractBrushes.GetSolidBrush(color);

                    e.Graphics.FillRectangle(brush, e.ClipRectangle);

                    _paintedPages.Add(pageControl);
                }
                else if (_paintedPages.Contains(pageControl))
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
