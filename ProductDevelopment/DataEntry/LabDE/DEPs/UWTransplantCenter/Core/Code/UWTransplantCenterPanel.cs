using Extract.DataEntry;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
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
    }
}
