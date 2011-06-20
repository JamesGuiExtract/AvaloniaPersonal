using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Defines a <see cref="Control"/> that consists of an icon which will display a tooltip if the
    /// user hovers over or clicks on it.
    /// </summary>
    public partial class InfoTip : UserControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(InfoTip).ToString();

        /// <summary>
        /// The default number of milliseconds the mouse must hover before the tip is displayed.
        /// </summary>
        static readonly int _DEFAULT_INITIAL_DELAY = 100;

        /// <summary>
        /// The default number of milliseconds before the tooltip automatically hides. This is a
        /// very large number since the tool tip is being used in conjunction with an icon
        /// specifically designed to show a tooltip; as long as the user continues to hover over it
        /// the tooltip should coninue to be displayed.
        /// </summary>
        static readonly int _DEFAULT_AUTOHIDE_DELAY = 999999;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="ToolTip"/> instance used to display the tool tip message.
        /// </summary>
        ToolTip _toolTip;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoTip"/> class.
        /// </summary>
        public InfoTip()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI32732", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32729");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the tip text.
        /// </summary>
        /// <value>
        /// The tip text.
        /// </value>
        [Browsable(true), Category("Appearance")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", 
            typeof(UITypeEditor)), Localizable(true)]
        public string TipText
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="ToolTip"/> used by the <see cref="InfoTip"/>.
        /// </summary>
        public ToolTip ToolTip
        {
            get
            {
                return _toolTip;
            }
        }

        #endregion Properties

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _toolTip = new ToolTip();
                _toolTip.InitialDelay = _DEFAULT_INITIAL_DELAY;
                _toolTip.AutoPopDelay = _DEFAULT_AUTOHIDE_DELAY;
                _toolTip.SetToolTip(this, TipText);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32730");
            }
        }

        /// <summary>
        /// Handles the case that the user clicked on the icon in order to force the tooltip to be
        /// displayed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleClick(object sender, EventArgs e)
        {
            try
            {
                _toolTip.Show(TipText, this, _toolTip.AutoPopDelay);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32731");
            }
        }
    }
}
