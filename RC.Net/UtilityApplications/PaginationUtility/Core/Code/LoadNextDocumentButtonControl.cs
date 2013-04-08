using Extract.Drawing;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A <see cref="NavigablePaginationControl"/> that provides a button that can be pressed to
    /// request the next document be loaded.
    /// </summary>
    internal partial class LoadNextDocumentButtonControl : NavigablePaginationControl
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadNextDocumentButtonControl"/> class.
        /// </summary>
        public LoadNextDocumentButtonControl()
            : base()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35621");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when the load next document button is clicked.
        /// </summary>
        public event EventHandler<EventArgs> ButtonClick;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is highlighted.
        /// </summary>
        /// <value><see langword="true"/> if this instance is hilighted; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public override bool Highlighted
        {
            get
            {
                return base.Highlighted;
            }
            set
            {
                base.Highlighted = value;

                if (value)
                {
                    _loadNextDocumentButton.Focus();
                }
                else
                {
                    // Ensure this button doesn't have focus once it is no longer highlighted.
                    Parent.Focus();
                }

                // Refresh _outerPanel to update the indication of whether it is currently the
                // primary selection.
                _outerPanel.Invalidate();
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if managed resources should be disposed;
        /// otherwise, <see langword="false"/>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (components != null)
                    {
                        components.Dispose();
                    }
                }
                catch (System.Exception ex)
                {
                    ex.ExtractLog("ELI35623");
                }
            }

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Paint"/> event of the <see cref="_outerPanel"/> control
        /// in order to indicate the selection state.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance
        /// containing the event data.</param>
        void HandleOuterPanel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (Highlighted)
                {
                    var highlightBrush = ExtractBrushes.GetSolidBrush(SystemColors.Highlight);
                    e.Graphics.FillRectangle(highlightBrush, _outerPanel.ClientRectangle);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35625");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_loadNextDocumentButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleLoadNextDocumentButton_Click(object sender, EventArgs e)
        {
            try
            {
                OnButtonClick();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35626");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Raised the <see cref="ButtonClick"/> event.
        /// </summary>
        void OnButtonClick()
        {
            var eventHandler = ButtonClick;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
