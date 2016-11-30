using System;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// A <see cref="Form"/> that will not be visible or grab focus when shown.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public class InvisibleForm : Form
    {
        /// <summary>
        /// Whether the form has been loaded.
        /// </summary>
        bool _loaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvisibleForm"/> class.
        /// </summary>
        public InvisibleForm() : base()
        {
            try
            {
                // Change all properties that otherwise contribute to this form appearing or
                // grabbing focus.
                FormsMethods.MakeFormInvisible(this);
                SetStyle(ControlStyles.Selectable, false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41653");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _loaded = true;

                // Once the form is loaded, make it not visible. If visibility is forced to false
                // before being loaded, it prevents OnLoad for this and child controls from being
                // called at all.
                SetVisibleCore(false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41654");
            }
        }

        /// <summary>
        /// Sets the control to the specified visible state.
        /// </summary>
        /// <param name="value"><c>true</c> to make the control visible; otherwise, <c>false</c>.</param>
        protected override void SetVisibleCore(bool value)
        {
            // If visibility is forced to false before being loaded, it prevents OnLoad for this and
            // child controls from being called at all.
            if (!_loaded || !value)
            {
                base.SetVisibleCore(value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the window will be activated when it is shown.
        /// </summary>
        protected override bool ShowWithoutActivation
        {
            get
            {
                // Prevent this form from stealing focus when shown.
                return true;
            }
        }
    }
}
