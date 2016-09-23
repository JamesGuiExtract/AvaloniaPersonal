using Extract.Utilities.Forms;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Extract.Demo_Pagination
{
    /// <summary>
    /// The doc-type specific panel for the "Lab Results doc type.
    /// </summary>
    internal partial class LabResultsPanel : SectionPanel
    {
        /// <summary>
        /// A regex pattern to check for a valid date format.
        /// </summary>
        static Regex _dateRegex = new Regex(@"^\d{2}/\d{2}/\d{4}$", RegexOptions.Compiled);

        /// <summary>
        /// A regex pattern to check for a valid time format.
        /// </summary>
        static Regex _timeRegex = new Regex(@"^\d{2}:\d{2}$", RegexOptions.Compiled);

        /// <summary>
        /// The <see cref="ErrorProvider" /> to display error glyph for fields with invalid data.
        /// </summary>
        ErrorProvider _errorProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabResultsPanel"/> class.
        /// </summary>
        public LabResultsPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the <see cref="ErrorProvider" /> to display error glyph for fields with invalid data.
        /// </summary>
        public override ErrorProvider ErrorProvider
        {
            get
            {
                return _errorProvider;
            }

            set
            {
                try
                {
                    if (value != _errorProvider)
                    {
                        _errorProvider = value;

                        if (_errorProvider != null)
                        {
                            _collectionDateTextBox.SetErrorGlyphPosition(_errorProvider);
                            _collectionTimeTextBox.SetErrorGlyphPosition(_errorProvider);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41390");
                }
            }
        }

        /// <summary>
        /// Loads the <paramref name="data" /> into the controls.
        /// </summary>
        /// <param name="data">The <see cref="Demo_PaginationDocumentData" /> to load.</param>
        public override void LoadData(Demo_PaginationDocumentData data)
        {
            try
            {
                if (data != null)
                {
                    _collectionDateTextBox.Text = data.LabCollectionDate;
                    _collectionTimeTextBox.Text = data.LabCollectionTime;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41391");
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="validateData"><see langword="true"/> if the <see paramref="data"/> should
        /// be validated for errors when saving; otherwise, <see langwor="false"/>.</param>
        /// <returns></returns>
        public override bool SaveData(Demo_PaginationDocumentData data, bool validateData)
        {
            try
            {
                if (data != null)
                {
                    data.LabCollectionDate = _collectionDateTextBox.Text;
                    data.LabCollectionTime = _collectionTimeTextBox.Text;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41392");
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the _collectionDateTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Handle_CollectionDateTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                DateTime dateTime;
                if (string.IsNullOrWhiteSpace(_collectionDateTextBox.Text) ||
                    (_dateRegex.IsMatch(_collectionDateTextBox.Text) &&
                     DateTime.TryParse(_collectionDateTextBox.Text, out dateTime) && dateTime < DateTime.Now))
                {
                    _collectionDateTextBox.SetError(_errorProvider, "");
                }
                else
                {
                    _collectionDateTextBox.SetError(_errorProvider, "Please enter a valid date.");
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41393");
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the _collectionTimeTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Handle_CollectionTimeTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                DateTime dateTime;
                if (string.IsNullOrWhiteSpace(_collectionTimeTextBox.Text) ||
                    (_timeRegex.IsMatch(_collectionTimeTextBox.Text) &&
                     DateTime.TryParse(_collectionTimeTextBox.Text, out dateTime)))
                {
                    _collectionTimeTextBox.SetError(_errorProvider, "");
                }
                else
                {
                    _collectionTimeTextBox.SetError(_errorProvider, "Please enter a valid time.");
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41394");
            }
        }
    }
}
