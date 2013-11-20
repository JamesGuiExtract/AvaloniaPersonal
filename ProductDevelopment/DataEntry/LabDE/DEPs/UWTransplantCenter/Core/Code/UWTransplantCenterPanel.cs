using Extract.DataEntry;
using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Extract.Drawing;

namespace Extract.DataEntry.DEP.UWTransplantCenter
{
    /// <summary>
    /// A sample <see cref="DataEntryControlHost"/> intended to demonstrate functionality.
    /// </summary>
    public partial class UWTransplantCenterPanel : DataEntryControlHost
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(UWTransplantCenterPanel).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UWTransplantCenterPanel"/> class.
        /// </summary>
        public UWTransplantCenterPanel() 
            : base()
        {
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDEVerificationUIObject, "ELI35805", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI35806", ex);
            }
        }

        #endregion Constructors

        #region Overloads

        /// <summary>
        /// Raises the Load event. 
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // To enforce the UW request that all text be selected when text boxes gain focus,
                // register for the GotFocus event from all DataEntryTextBoxes.
                RegisterGotFocusEvents(Controls);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36352");
            }
        }

        #endregion Overloads

        #region Event Handlers

        /// <summary>
        /// Handles the DrawItem event of the Handle_HasCoverPageComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DrawItemEventArgs"/> instance containing the event data.</param>
        void HandleHasCoverPageComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                ComboBox combo = (ComboBox)sender;

                if ((e.State & DrawItemState.Disabled) == DrawItemState.Disabled)
                {
                    e.Graphics.FillRectangle(
                        ExtractBrushes.GetSolidBrush(SystemColors.Control), e.Bounds);
                }
                else if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    e.Graphics.FillRectangle(
                        ExtractBrushes.GetSolidBrush(Color.LightGreen), e.Bounds);
                }
                else
                {
                    e.Graphics.FillRectangle(
                        ExtractBrushes.GetSolidBrush(combo.BackColor), e.Bounds);
                }

                if (e.Index != -1)
                {
                    e.Graphics.DrawString(combo.Items[e.Index].ToString(), e.Font,
                        ExtractBrushes.GetSolidBrush(combo.ForeColor),
                        new Point(e.Bounds.X, e.Bounds.Y));
                }

                e.DrawFocusRectangle();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36349");
            }
        }

        /// <summary>
        /// Handles the GotFocus event of a TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleTextBox_GotFocus(object sender, EventArgs e)
        {
            try
            {
                var textBox = (DataEntryTextBox)sender;

                if (Control.MouseButtons.HasFlag(MouseButtons.Left))
                {
                    // The sequence of events that occur during a mouse click that gives a control
                    // focus as well as any intervening mouse movement events prior to the mouse up
                    // event clears any selection we give it here. Wait until the mouse up to select
                    // all text.
                    textBox.MouseUp += HandleTextBox_MouseUp;
                }
                else
                {
                    textBox.SelectAll();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36351");
            }
        }

        /// <summary>
        /// Handles the MouseUp event of a TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance
        /// containing the event data.</param>
        void HandleTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                var textBox = (DataEntryTextBox)sender;

                textBox.MouseUp -= HandleTextBox_MouseUp;
                textBox.SelectAll();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36353");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Registers the got focus events.
        /// </summary>
        /// <param name="controls">The controls.</param>
        void RegisterGotFocusEvents(ControlCollection controls)
        {
            foreach (var control in controls.OfType<Control>())
            {
                var textBox = control as DataEntryTextBox;
                if (textBox != null)
                {
                    textBox.GotFocus += HandleTextBox_GotFocus;
                }

                RegisterGotFocusEvents(control.Controls);
            }
        }

        #endregion Private Members
    }
}
