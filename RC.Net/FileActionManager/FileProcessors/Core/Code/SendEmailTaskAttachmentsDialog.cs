using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Allows attachments to be selected for a <see cref="SendEmailTask"/> instance.
    /// </summary>
    public partial class SendEmailTaskAttachmentsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(SendEmailTaskAttachmentsDialog).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The selection in the last <see cref="_dataGridView"/> row when edit mode was ended.
        /// </summary>
        Tuple<int, int> _editingControlSelection;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEmailTaskAttachmentsDialog"/> class.
        /// </summary>
        public SendEmailTaskAttachmentsDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEmailTaskAttachmentsDialog"/> class.
        /// </summary>
        /// <param name="settings"><see cref="SendEmailTask"/> for which attachments are to be
        /// selected.</param>
        /// <param name="errorEmailMode"><see langword="true"/> if the <see paramref="settings"/>
        /// instance is being configured as an error handler.</param>
        public SendEmailTaskAttachmentsDialog(SendEmailTask settings, bool errorEmailMode)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI35970",
                    _OBJECT_NAME);

                InitializeComponent();
                
                Settings = settings;

                if (errorEmailMode)
                {
                    _pathTagsButton.PathTags.AddTag(SendEmailTask.ExceptionFileTag, null);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35969");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public SendEmailTask Settings
        {
            get;
            set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    foreach (string attachent in Settings.Attachments)
                    {
                        _dataGridView.Rows.Add(attachent);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35966");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="DataGridView.SelectionChanged"/> event of the
        /// <see cref="_dataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void _dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                _pathTagsButton.Enabled = (_dataGridView.SelectedCells.Count == 1);
                _browseButton.Enabled = (_dataGridView.SelectedCells.Count == 1);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35968");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellValidating"/> event of the
        /// <see cref="_dataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellValidatingEventArgs"/> instance containing the event data.</param>
        void HandleDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            try
            {
                // Use of the path tags button will cause edit mode to end and the editing control
                // to close. So that tags/functions can be inserted using the current selection,
                // store the selection as the editing control closes.
                var editingControl = _dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
                if (editingControl != null)
                {
                    _editingControlSelection = new Tuple<int, int>(
                        editingControl.SelectionStart, editingControl.SelectionLength);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35964");
            }
        }

        /// <summary>
        /// Handles the CurrentCellChanged event of the _dataGridView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleDataGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            try
            {
                // When a different cell is selected, clear the selection coordinates to be used if
                // the path tags button is pressed.
                _editingControlSelection = null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35997");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:PathTagsButton.TagSelecting"/> event of the
        /// <see cref="_pathTagsButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.Utilities.Forms.TagSelectingEventArgs"/> instance
        /// containing the event data.</param>
        void HandlePathTagsButton_TagSelecting(object sender, Extract.Utilities.Forms.TagSelectingEventArgs e)
        {
            try
            {
                // Use of the path tags button will have caused edit mode in the data grid view to
                // have ended. Re-enter edit mode and restore the last known selection before
                // applying the tag selection.
                _dataGridView.BeginEdit(true);

                var editingControl = _dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
                if (editingControl != null)
                {
                    _pathTagsButton.TextControl = editingControl;
                    if (_editingControlSelection != null)
                    {
                        editingControl.Select(
                            _editingControlSelection.Item1, _editingControlSelection.Item2);
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35965");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:BrowseButton.PathSelected"/> event of the
        /// <see cref="_browseButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.Utilities.Forms.PathSelectedEventArgs"/> instance
        /// containing the event data.</param>
        void HandleBrowseButton_PathSelected(object sender, Extract.Utilities.Forms.PathSelectedEventArgs e)
        {
            try
            {
                // Use of the path tags button will have caused edit mode in the data grid view to
                // have ended. Re-enter edit mode to apply the selected path.
                _dataGridView.BeginEdit(true);

                var editingControl = _dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
                if (editingControl != null)
                {
                    editingControl.Text = e.Path;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35971");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_clearButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleClearButton_Click(object sender, EventArgs e)
        {
            try
            {
                _dataGridView.Rows.Clear();

                // [DotNetRCAndUtils:1085]
                // To allow immediate entry of an attachment after clearing, select the "new" row.
                _dataGridView.CurrentCell = _dataGridView.Rows[0].Cells[0];
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36003");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the <see cref="Control.Click"/> event.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                Settings.Attachments = _dataGridView.Rows
                    .OfType<DataGridViewRow>()
                    .Select(row => (string)row.Cells[0].Value)
                    .Where(attachment => !string.IsNullOrWhiteSpace(attachment))
                    .ToArray();

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35967", ex);
            }
        }

        #endregion Event Handlers
    }
}
