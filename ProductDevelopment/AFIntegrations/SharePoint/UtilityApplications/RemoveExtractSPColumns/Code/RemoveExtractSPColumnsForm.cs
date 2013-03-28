using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.SharePoint.Client;


namespace RemoveExtractSPColumns
{
    /// <summary>
    /// Form used to select the site and IDShield related columns to remove from Document Libraries
    /// </summary>
    public partial class RemoveExtractSPColumnsForm : System.Windows.Forms.Form
    {
        #region Constants
        
        // TODO: These constants are used in at least 2 places and should be placed
        //       in a location that can be shared
        // Constants for the ID Shield status field
        internal static readonly string IdShieldStatusColumn = "IDShieldStatus";
        internal static readonly string IdShieldStatusColumnDisplay = "ID Shield Status";

        // Constants for the ID Shield redacted file field
        internal static readonly string IdShieldRedactedColumn = "IDShieldRedactedFile";
        internal static readonly string IdShieldRedactedColumnDisplay = "ID Shield Redacted File";

        // Constants for the ID Shield unredacted file field
        internal static readonly string IdShieldUnredactedColumn = "IDShieldUnredactedFile";
        internal static readonly string IdShieldUnredactedColumnDisplay = "ID Shield Unredacted File";

        internal static readonly string SensitiveItemCountColumn = "SensitiveItemCount";
        internal static readonly string SensitiveItemCountColumnDisplay = "SensitiveItemCount";

        internal static readonly string IdShieldReferenceColumn = "IDSReference";
        internal static readonly string IdShieldReferenceColumnDisplay = "IDS Reference";

        // Constants for the Extract Data Capture status field
        internal static readonly string ExtractDataCaptureStatusColumn = "ExtractDataCaptureStatus";
        internal static readonly string ExtractDataCaptureStatusColumnDisplay = "Extract Data Capture Status";
        
        #endregion

        #region Constructors

        /// <summary>
        /// Creates and initializes the form
        /// </summary>
        public RemoveExtractSPColumnsForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Form Event Handlers

        /// <summary>
        /// Click handler for the remove button
        /// </summary>
        /// <param name="sender">Object that sent the event</param>
        /// <param name="e"></param>
        private void handleRemoveSPColumnsButton_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                if (!string.IsNullOrEmpty(_siteURLTextBox.Text))
                {
                    // Open the site
                    using (var context = new ClientContext(_siteURLTextBox.Text))
                    {
                        // Get the document libraries
                        var query = from list in context.Web.Lists
                                    where list.Hidden == false
                                    && list.BaseTemplate == 101
                                    && list.Title != "Site Assets"
                                    select list;
                        var result = context.LoadQuery(query);
                        context.ExecuteQuery();

                        // Need to go through all the lists looking for our columns
                        foreach (var item in result)
                        {
                            if (_redactedFileColumnCheckBox.Checked)
                            {
                                deleteSPColumnFromList(context, item, IdShieldRedactedColumn);
                            }
                            if (_unredactedColumnCheckBox.Checked)
                            {
                                deleteSPColumnFromList(context, item, IdShieldUnredactedColumn);
                            }
                            if (_idshieldStatusColumn.Checked)
                            {
                                deleteSPColumnFromList(context, item, IdShieldStatusColumn);
                            }
                            if (_idshieldSensitiveItemCount.Checked)
                            {
                                deleteSPColumnFromList(context, item, SensitiveItemCountColumn);
                            }
                            if (_idshieldIDSReference.Checked)
                            {
                                deleteSPColumnFromList(context, item, IdShieldReferenceColumn);
                            }
                            if (_DataCaptureStatus.Checked)
                            {
                                deleteSPColumnFromList(context, item, ExtractDataCaptureStatusColumn);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Todo: Replace this with the DisplayInMessageBox in in Extract.ExtensionMethods
                MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        #endregion

        #region Static Helper Methods

        /// <summary>
        /// Removes the column from list on site
        /// </summary>
        /// <param name="context">The Site context that contains the list</param>
        /// <param name="item">The list to remove the column from</param>
        /// <param name="columnName">The internal name of the column to remove</param>
        private static void deleteSPColumnFromList(ClientContext context, List item, string columnName)
        {
            Boolean columnFound = false;
            try
            {
                // Find the column to delete
                Field fld = item.Fields.GetByInternalNameOrTitle(columnName);
                context.Load(fld);
                // This throws an exception if the column does not exist
                context.ExecuteQuery();

                // If get to this spot the column was found in the list
                columnFound = true;

                // Mark the found column as writeable so it can be deleted
                fld.ReadOnlyField = false;
                fld.Update();

                // Delete the column
                fld.DeleteObject();
                context.ExecuteQuery();
            }
            catch (Exception ex)
            { 
                // Only need to handle the case when the column was found
                // but could not be deleted
                if (columnFound)
                {
                    // Todo: Replace this with the DisplayInMessageBox in in Extract.ExtensionMethods
                    MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                }
            }
        }

        #endregion  
    }
}
