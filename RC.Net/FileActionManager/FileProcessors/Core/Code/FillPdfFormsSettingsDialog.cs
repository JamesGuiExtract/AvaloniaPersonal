using Extract.GdPicture;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    ///  A <see cref="Form"/> to view and modify settings for an <see cref="FillPdfFormsSettingsDialog"/> instance.
    /// </summary>
    public partial class FillPdfFormsSettingsDialog : Form
    {
        public FillPdfFormsTask FillPdfFormsTask { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FillPdfFormsSettingsDialog"/> class.
        /// </summary>
        public FillPdfFormsSettingsDialog(FillPdfFormsTask task)
        {
            try
            {
                this.FillPdfFormsTask = task;
                InitializeComponent();

                LoadFillPdfDictionaryIntoUI();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI54265");
            }
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            return false;
        }

        private Dictionary<string, string> LoadBlankDictionaryFormValues(GdPictureUtility gdPictureUtility, string pdfFormPath)
        {
            var formFieldValues = FillPdfFormsTask.GetFormFieldValues(gdPictureUtility.PdfAPI, pdfFormPath);
            var formFieldKeyValues = formFieldValues.Select(entry => entry.Key);

            var toReturn = new Dictionary<string, string>();
            foreach (var entry in formFieldKeyValues)
            {
                toReturn.Add(entry, string.Empty);
            }

            return toReturn;
        }

        private void _okButton_Click(object sender, EventArgs e)
        {
            TranslateDataGridToDictionary();
            if (WarnIfInvalid())
            {
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void TranslateDataGridToDictionary()
        {
            this.FillPdfFormsTask.FieldsToAutoFill = new Dictionary<string, string>();
            DataTable dt = (DataTable)FieldsToFillDataGrid.DataSource;
            dt.AsEnumerable()
                .ToList()
                .ForEach(row => this.FillPdfFormsTask.FieldsToAutoFill.Add(row[0] as string, row[1] as string));
        }

        private void LoadFillPdfDictionaryIntoUI()
        {
            DataTable dataTable = new();
            dataTable.Columns.Add("Form Field", typeof(string));
            dataTable.Columns.Add("Value to Fill", typeof(string));

            if (this.FillPdfFormsTask.FieldsToAutoFill.Count > 0)
            {
                var enumerator = this.FillPdfFormsTask.FieldsToAutoFill.Keys.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    dataTable.Rows.Add(new object[] { enumerator.Current, this.FillPdfFormsTask.FieldsToAutoFill[enumerator.Current] });
                }
            }

            // Add unique Key constraint so the dictionary does not have issues with duplicate values.
            dataTable.Constraints.Add("keyconstraint", dataTable.Columns[0], true);

            FieldsToFillDataGrid.DataSource = dataTable;
            FieldsToFillDataGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            FieldsToFillDataGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void BrowseToFillPdf_PathSelected(object sender, Extract.Utilities.Forms.PathSelectedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.Path))
                {
                    UtilityMethods.ShowMessageBox("Cannot load empty file path.",
                    "Invalid configuration", true);
                    return;
                }

                TranslateDataGridToDictionary();

                using GdPictureUtility gdPictureUtility = new();

                GdPictureUtility.ThrowIfStatusNotOK(gdPictureUtility.PdfAPI.LoadFromFile(e.Path, false),
                    "ELI54267", "The PDF document can't be loaded", new(filePath: e.Path));

                // Load all values with a blank key, and filter out anything that is not a text box.
                var blankDictionaryFormValues = LoadBlankDictionaryFormValues(gdPictureUtility, e.Path);

                foreach (var keyValuePair in blankDictionaryFormValues)
                {
                    if (!this.FillPdfFormsTask.FieldsToAutoFill.Contains(keyValuePair.Key))
                    {
                        this.FillPdfFormsTask.FieldsToAutoFill.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }

                LoadFillPdfDictionaryIntoUI();
            }
            catch (Exception exception)
            {
                exception.AsExtract("ELI54268").Display();
            }
            finally
            { 
                _browseToFillPdfKeyValues.FileOrFolderPath = null;
            }
        }
    }
}
