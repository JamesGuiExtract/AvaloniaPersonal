using Extract.DataEntry.LabDE;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Extract.DataEntry.DEP.Demo_LabDE
{
    public partial class Demo_LabDEPanel : DataEntryControlHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Demo_LabDEPanel"/> class.
        /// </summary>
        public Demo_LabDEPanel()
            : base()
        {
            try
            {
                InitializeComponent();

                LabDEQueryUtilities.Register();
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI37760");
            }
        }

        /// <summary>
        /// Commands the <see cref="DataEntryControlHost"/> to finalize the attribute vector for 
        /// output. This primarily consists of asking each <see cref="IDataEntryControl"/> to 
        /// validate that the data it contains conforms to any validation rules that have been 
        /// applied to it. If so, the vector of attributes as it currently stands is output.
        /// </summary>
        /// <param name="validateData"><see langword="true"/> if the save should only be performed
        /// if all data in the DEP passes validation, <see langword="false"/> if data should be
        /// saved even if there is invalid data.</param>
        /// <returns><see langword="true"/> if the document's data was successfully saved.
        /// <see langword="false"/> if the data was not saved (such as when data fails validation).
        /// </returns>
        public override bool SaveData(bool validateData)
        {
            try
            {
                // https://extract.atlassian.net/browse/ISSUE-13055
                // So that the all order picker functionality can be avoided in configurations
                // where the database may not be available by hiding the order picker column, skip
                // custom bi-directional behavior on save if the order picker column is not visible.
                if (validateData && _orderPicker.Visible)
                {
                    // In case any related data in the database has changed that would result in any
                    // of the data to now be invalid, clear cached data before validating the data
                    // to be saved.
                    _orderPicker.ClearCachedData(true);
                    ClearCache();

                    IEnumerable<string> previouslySubmittedOrders =
                        _orderPicker.GetPreviouslySubmittedOrders();
                    if (previouslySubmittedOrders.Any())
                    {
                        string warningMessage =
                            "Results for the following orders have already been submitted via LabDE:\r\n\r\n" +
                            string.Join("\r\n\r\n", previouslySubmittedOrders) +
                            "\r\n\r\nAre you sure you want to re-submit these results?";

                        using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
                        {
                            messageBox.Caption = "Warning";
                            messageBox.StandardIcon = MessageBoxIcon.Exclamation;
                            messageBox.Text = warningMessage;
                            messageBox.AddStandardButtons(MessageBoxButtons.YesNo);
                            if (messageBox.Show(this) == "No")
                            {
                                return false;
                            }
                        }
                    }
                }

                if (base.SaveData(validateData))
                {
                    // The validateData parameter indicates when the document is being committed.
                    // In this case, the currently selected orders should to be linked to the
                    // document being committed.
                    if (validateData && _orderPicker.Visible)
                    {
                        _orderPicker.LinkFileWithOrders();
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI37761");
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_refreshButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRefreshButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                _orderPicker.ClearCachedData(true);
                ClearCache();
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI38192");
            }
        }
    }
}
