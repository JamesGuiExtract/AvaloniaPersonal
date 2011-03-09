using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// A class derived from <see cref="TextBox"/> that auto shows/hides
    /// the vertical and horizontal scroll bars as needed.
    /// </summary>
    public partial class BetterMultilineTextBox : TextBox
    {
        #region Constants

        /// <summary>
        /// Object name used in licensing calls
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(BetterMultilineTextBox).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BetterMultilineTextBox"/> class.
        /// </summary>
        public BetterMultilineTextBox()
            : base()
        {
            try
            {
                // Only validate the license at run time
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI32008", _OBJECT_NAME);

                AllowSelectAllKey = true;

                base.Multiline = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32009");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnTextChanged(EventArgs e)
        {
            try
            {
                UpdateScrollbarVisibility();

                base.OnTextChanged(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32010");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.ClientSizeChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnClientSizeChanged(EventArgs e)
        {
            try
            {
                UpdateScrollbarVisibility();

                base.OnClientSizeChanged(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32011");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                if (AllowSelectAllKey)
                {
                    if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
                    {
                        SelectAll();

                        e.Handled = true;
                    }
                }

                base.OnKeyDown(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32015");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this is a multiline <see cref="T:System.Windows.Forms.TextBox"/> control.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public override bool Multiline
        {
            get
            {
                return base.Multiline;
            }
            set
            {
                base.Multiline = value;
            }
        }

        #endregion Overrides

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the multi-line edit box
        /// handles the select all (Ctrl+A) key event.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> then Ctrl+A will select all text in the box.
        /// </value>
        [Category("Behavior")]
        [Description("Indicates whether or not Ctrl+A will select all text in the text box or not.")]
        [DefaultValue(true)]
        public bool AllowSelectAllKey { get; set; }

        #endregion Properties
        
        #region Methods

        /// <summary>
        /// Updates the visibility of the scroll bars
        /// </summary>
        void UpdateScrollbarVisibility()
        {
            using(new LockControlUpdates(this))
            {
                var selection = SelectionStart;
                var size = TextRenderer.MeasureText(Text, Font);
                var clientSize = ClientSize;
                var sb = ScrollBars.None;
                if (clientSize.Height < size.Height + (int)Font.Size)
                {
                    sb |= ScrollBars.Vertical;
                }
                if (clientSize.Width < size.Width)
                {
                    sb |= ScrollBars.Horizontal;
                }

                ScrollBars = sb;
                SelectionStart = selection;
                ScrollToCaret();
            }
        }

        #endregion Methods
    }
}
