using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Extract.Utilities;
using System.Diagnostics.CodeAnalysis;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Configuration dialog for the <see cref="RasterizePdfTask"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Rasterize")]
    internal partial class RasterizePdfTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// Default string for the PDF file.
        /// </summary>
        static readonly string _DEFAULT_PDF_FILE = FileActionManagerPathTags.SourceDocumentTag;

        /// <summary>
        /// Default string for the converted file.
        /// </summary>
        static readonly string _DEFAULT_CONVERTED = _DEFAULT_PDF_FILE + ".tif";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Gets the PDF file.
        /// </summary>
        public string PdfFile { get; private set; }

        /// <summary>
        /// Gets the output file.
        /// </summary>
        public string ConvertedFile { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the input PDF should be deleted after conversion.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> then the PDF file will be deleted after conversion.
        /// </value>
        public bool DeletePdf { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the task should fail when the delete fails.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> and the delete file fails then the task will fail.
        /// </value>
        public bool FailIfDeleteFails { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the SourceDocName should be modified
        /// when the conversion is complete.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> SourceDocName will be modified to <see cref="ConvertedFile"/>
        /// in the database; otherwise it will not be changed.
        /// </value>
        public bool ChangeSourceDocName { get; private set; }

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterizePdfTaskSettingsDialog"/> class.
        /// </summary>
        public RasterizePdfTaskSettingsDialog()
            : this(null, null, false, true, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterizePdfTaskSettingsDialog"/> class.
        /// </summary>
        /// <param name="pdfFile">The PDF file.</param>
        /// <param name="convertedFile">The destination file for the conversion.</param>
        /// <param name="deletePdf">Whether or not the PDF file should be deleted after
        /// conversion.</param>
        /// <param name="failIfDeleteFails">Whether or not the task should fail if when deleting
        /// the PDF file there is an error.</param>
        /// <param name="changeSourceDocName">Whether or not the SourceDocName should be
        /// updated after conversion.</param>
        public RasterizePdfTaskSettingsDialog(string pdfFile, string convertedFile,
            bool deletePdf, bool failIfDeleteFails, bool changeSourceDocName)
        {
            try
            {
                InitializeComponent();

                _pathTagsButtonPdfFile.PathTags = new FileActionManagerPathTags();
                _pathTagsButtonOutputFile.PathTags = new FileActionManagerPathTags();

                PdfFile = string.IsNullOrWhiteSpace(pdfFile)
                    ? _DEFAULT_PDF_FILE : pdfFile;
                ConvertedFile = string.IsNullOrWhiteSpace(convertedFile)
                    ? _DEFAULT_CONVERTED : convertedFile;
                DeletePdf = deletePdf;
                FailIfDeleteFails = failIfDeleteFails;

                // Changing source doc name is only valid if the input PDF file is SourceDocName
                ChangeSourceDocName = PdfFile.Equals(_DEFAULT_PDF_FILE, StringComparison.Ordinal)
                    && changeSourceDocName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32236");
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _textPdfFile.Text = PdfFile;
                _textConvertedFile.Text = ConvertedFile;
                _checkDeletePdf.Checked = DeletePdf;
                _groupDeleteFailed.Enabled = DeletePdf;
                _radioFailTask.Checked = FailIfDeleteFails;
                _radioIgnoreError.Checked = !FailIfDeleteFails;

                _checkModifySourceDoc.Enabled = _textPdfFile.Text.Equals(
                    _DEFAULT_PDF_FILE, StringComparison.Ordinal);
                _checkModifySourceDoc.Checked = _checkModifySourceDoc.Enabled
                    && ChangeSourceDocName;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32237");
            }
        }

        /// <summary>
        /// Handles the ok clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleOkClicked(object sender, EventArgs e)
        {
            try
            {
                var tagManager = new FAMTagManager();

                // Validate settings
                if (string.IsNullOrWhiteSpace(_textPdfFile.Text))
                {
                    UtilityMethods.ShowMessageBox("PDF file cannot be blank.",
                        "Invalid Entry", true);
                    _textPdfFile.Focus();
                    return;
                }
                else if (tagManager.StringContainsInvalidTags(_textPdfFile.Text))
                {
                    UtilityMethods.ShowMessageBox("PDF file contains invalid tag(s).",
                        "Invalid Tags", true);
                    _textPdfFile.Focus();
                    return;
                }
                if (string.IsNullOrWhiteSpace(_textConvertedFile.Text))
                {
                    UtilityMethods.ShowMessageBox("Converted file cannot be blank.",
                        "Invalid Entry", true);
                    _textConvertedFile.Focus();
                    return;
                }
                else if (tagManager.StringContainsInvalidTags(_textConvertedFile.Text))
                {
                    UtilityMethods.ShowMessageBox("Converted file contains invalid tag(s).",
                        "Invalid Tags", true);
                    _textPdfFile.Focus();
                    return;
                }
                if (_checkModifySourceDoc.Checked
                    && !_textPdfFile.Text.Equals(_DEFAULT_PDF_FILE, StringComparison.Ordinal))
                {
                    UtilityMethods.ShowMessageBox(
                        "Can only change SourceDocName in the database when the PDF file is '<SourceDocName>'.",
                        "Invalid Entry", true);
                    _checkModifySourceDoc.Focus();
                    return;
                }

                PdfFile = _textPdfFile.Text;
                ConvertedFile = _textConvertedFile.Text;
                ChangeSourceDocName = _checkModifySourceDoc.Enabled && _checkModifySourceDoc.Checked;
                DeletePdf = _checkDeletePdf.Checked;
                FailIfDeleteFails = _radioFailTask.Checked;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32238");
            }
        }

        /// <summary>
        /// Handles the PDF file text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandlePdfFileTextChanged(object sender, EventArgs e)
        {
            try
            {
                _checkModifySourceDoc.Enabled =
                    _textPdfFile.Text.Equals(_DEFAULT_PDF_FILE, StringComparison.Ordinal);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32239");
            }
        }

        /// <summary>
        /// Handles the delete PDF check changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleDeletePdfCheckChanged(object sender, EventArgs e)
        {
            try
            {
                _groupDeleteFailed.Enabled = _checkDeletePdf.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32244");
            }
        }

        /// <summary>
        /// Handles the modify source doc check changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleModifySourceDocCheckChanged(object sender, EventArgs e)
        {
            if (_checkModifySourceDoc.Checked)
            {
                // Set the pdf file text back to the default and disable it.
                _textPdfFile.Text = _DEFAULT_PDF_FILE;
                _textPdfFile.Enabled = false;
            }
            else
            {
                _textPdfFile.Enabled = true;
            }
        }

        #endregion Methods
    }
}
