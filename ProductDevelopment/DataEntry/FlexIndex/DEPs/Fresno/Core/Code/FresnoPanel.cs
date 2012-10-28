using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Extract.DataEntry.DEP.Fresno
{
    /// <summary>
    /// A LabDE <see cref="DataEntryControlHost"/> customized for indexing hazardous material
    /// inventory forms for Fresno County's dept of environmental health.
    /// </summary>
    public partial class FresnoPanel : DataEntryControlHost
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FresnoPanel).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FresnoPanel"/> class.
        /// </summary>
        public FresnoPanel()
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
                    LicenseIdName.DataEntryCoreComponents, "ELI35084", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI35085", ex);
            }
        }

        #endregion Constructors

        /// <summary>
        /// Handles the TextChanged event of the HandleControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleControl_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var control = sender as Control;
                if (control != null)
                {
                    if (control.Text == "[BLANK]")
                    {
                        control.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35096");
            }
        }

        /// <summary>
        /// Handles the CellValueChanged event of the HandleTable control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellEventArgs"/>
        /// instance containing the event data.</param>
        void HandleTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    var table = (DataGridView)sender;
                    if (e.RowIndex < table.RowCount && e.ColumnIndex < table.ColumnCount)
                    {
                        var cell = table.Rows[e.RowIndex].Cells[e.ColumnIndex];
                        if (cell.Value.ToString() == "[BLANK]")
                        {
                            cell.Value = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35098");
            }
        }

        /// <summary>
        /// Handles the Click event of the HandleToggleTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleToggleTextBox_Click(object sender, EventArgs e)
        {
            try
            {
                var textBox = (TextBox)sender;
                textBox.Text = (string.IsNullOrEmpty(textBox.Text) ? "X" : "");
                NativeMethods.HideCaret(textBox.Handle);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35100");
            }
        }
    }
}
