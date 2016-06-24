using Extract.Drawing;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
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

        /// <summary>
        /// The color of the separator when not selected.
        /// </summary>
        Color _normalColor;

        /// <summary>
        /// The panel to house any <see cref="IPaginationDocumentDataPanel"/> that is displayed.
        /// </summary>
        Control _documentDataPanelControl;

        /// <summary>
        /// The <see cref="OutputDocument"/> this with which this separator is currently associated.
        /// </summary>
        OutputDocument _outputDocument;

        /// <summary>
        /// Used to prevent recursion while trying to update the current selection state.
        /// </summary>
        bool _changingSelection;

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

                _toolTip.SetToolTip(_editedPaginationGlyph, "Manual pagination has been applied");
                _toolTip.SetToolTip(_newDocumentGlyph, "This is a new document that will be created");
                _toolTip.SetToolTip(_editedDataPictureBox, "The data for this document has been modified");

                _normalColor = _tableLayoutPanel.BackColor;
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

        #region Events

        /// <summary>
        /// Raised when a <see cref="IPaginationDocumentDataPanel"/> is needed in response to the
        /// user selecting to edit data.
        /// </summary>
        public event EventHandler<DocumentDataPanelRequestEventArgs> DocumentDataPanelRequest;

        #endregion Events

        #region Public Members

        /// <summary>
        /// Gets the <see cref="OutputDocument"/> with which this separator is associated.
        /// </summary>
        /// <returns></returns>
        public OutputDocument GetDocument()
        {
            try
            {
                var firstPageControl = NextControl as PageThumbnailControl;
                var document = (firstPageControl != null)
                    ? firstPageControl.Document
                    : null;

                if (document != _outputDocument)
                {
                    if (_outputDocument != null)
                    {
                        _outputDocument.Invalidated -= HandleDocument_Invalidated;
                    }

                    _outputDocument = document;

                    if (_outputDocument != null)
                    {
                        _outputDocument.Invalidated += HandleDocument_Invalidated; 
                    }
                }

                return document;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40179");
            }
        }

        #endregion Public Members

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
                    if (_changingSelection)
                    {
                        return;
                    }

                    try
                    {
                        _changingSelection = true;

                        base.Selected = value;

                        _tableLayoutPanel.BackColor = value
                            ? ControlPaint.Dark(_normalColor)
                            : _normalColor;

                        // Invalidate so that paint occurs and new selection state is indicated.
                        Invalidate();
                    }
                    catch (Exception ex)
                    {
                        throw ex.AsExtract("ELI40182");
                    }
                    finally
                    {
                        _changingSelection = false;
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Invalidated"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.Forms.InvalidateEventArgs"/> that
        /// contains the event data.</param>
        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            try
            {
                UpdateControls();

                base.OnInvalidated(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40183");
            }
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

                // If selected, indicate selection with a darker back color except for 1 pixel
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

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_collapseDocumentButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCollapseDocumentButton_Click(object sender, EventArgs e)
        {
            try
            {
                var document = GetDocument();

                if (document != null)
                {
                    if (!document.Collapsed && _documentDataPanelControl != null &&
                        _tableLayoutPanel.Controls.Contains(_documentDataPanelControl))
                    {
                        _tableLayoutPanel.Controls.Remove(_documentDataPanelControl);
                        _documentDataPanelControl = null;
                        UpdateSize();
                    }

                    document.Collapsed = !document.Collapsed;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40184");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_editDocumentDataButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleEditDocumentDataButton_Click(object sender, EventArgs e)
        {
            try
            {
                _tableLayoutPanel.SuspendLayout();

                if (_documentDataPanelControl != null && _tableLayoutPanel.Controls.Contains(_documentDataPanelControl))
                {
                    _tableLayoutPanel.Controls.Remove(_documentDataPanelControl);
                    _documentDataPanelControl = null;
                    UpdateSize();
                }
                else
                {
                    var document = GetDocument();

                    if (document != null)
                    {
                        var args = new DocumentDataPanelRequestEventArgs(document);
                        OnDocumentDataPanelRequest(args);

                        if (args.DocumentDataPanel != null)
                        {
                            _documentDataPanelControl = (Control)args.DocumentDataPanel;
                            _tableLayoutPanel.Controls.Add(_documentDataPanelControl, 0, 1);
                            _tableLayoutPanel.SetColumnSpan(_documentDataPanelControl, _tableLayoutPanel.ColumnCount);
                            UpdateSize();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40185");
            }
            finally
            {
                try
                {
                    _tableLayoutPanel.ResumeLayout(true);
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI40178");
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.ControlRemoved"/> event of the
        /// <see cref="_tableLayoutPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ControlEventArgs"/> instance
        /// containing the event data.</param>
        void HandleTableLayoutPanel_ControlRemoved(object sender, ControlEventArgs e)
        {
            try
            {
                UpdateSize();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40181");
            }
        }

        /// <summary>
        /// Handles the <see cref="OutputDocument.Invalidated"/> event of
        /// <see cref="_outputDocument"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDocument_Invalidated(object sender, EventArgs e)
        {
            try
            {
                // Whenever the associated document is updated, invalidate to ensure displayed icons
                // reflect the current document state.
                Invalidate();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40180");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Updates UI indications to reflect the current state of the associated document.
        /// </summary>
        void UpdateControls()
        {
            var document = GetDocument();
            if (document != null)
            {
                _collapseDocumentButton.Visible = true;
                _collapseDocumentButton.Image = document.Collapsed
                    ? Properties.Resources.Expand
                    : Properties.Resources.Collapse;
                _editDocumentDataButton.Visible = true;
                _summaryLabel.Text = document.Summary;
                _pagesLabel.Text = string.Format(CultureInfo.CurrentCulture,
                    "{0} pages", document.PageControls.Count(c => !c.Deleted));
                _newDocumentGlyph.Visible = !document.InSourceDocForm;
                _editedPaginationGlyph.Visible = !document.InOriginalForm;
                _editedDataPictureBox.Visible = document.DataModified;

                return;
            }
            else
            {
                _collapseDocumentButton.Visible = false;
                _editDocumentDataButton.Visible = false;
                _summaryLabel.Text = "";
                _pagesLabel.Text = "";
                _newDocumentGlyph.Visible = false;
                _editedPaginationGlyph.Visible = false;
                _editedDataPictureBox.Visible = false;
            }
        }

        /// <summary>
        /// Updates the size of the control to fit the current contents.
        /// </summary>
        void UpdateSize()
        {
            // In order to solve issues with this control not auto-sizing based on the size of
            // _tableLayoutPanel, explicitly ask for and resize according to _tableLayoutPanel's
            // preferred size.
            var size = _tableLayoutPanel.GetPreferredSize(new Size(Width, 1000));
            Height = size.Height;
        }

        /// <summary>
        /// Raises the <see cref="DocumentDataPanelRequest"/> event.
        /// </summary>
        /// <param name="args">The <see cref="DocumentDataPanelRequestEventArgs"/> instance
        /// containing the event data.</param>
        void OnDocumentDataPanelRequest(DocumentDataPanelRequestEventArgs args)
        {
            var eventHandler = DocumentDataPanelRequest;
            if (eventHandler != null)
            {
                eventHandler(this, args);
            }
        }

        #endregion Private Members
    }
}
