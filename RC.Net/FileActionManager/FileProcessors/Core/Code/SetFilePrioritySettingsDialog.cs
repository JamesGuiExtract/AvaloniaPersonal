using Extract.Utilities.Forms;
using System;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// A dialog for configuring the Set file priority task.
    /// </summary>
    partial class SetFilePrioritySettingsDialog : Form
    {
        #region Fields

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>The priority.</value>
        public EFilePriority Priority { get; set; }

        #endregion Fields

        /// <summary>
        /// Initializes a new instance of the <see cref="SetFilePrioritySettingsDialog"/> class.
        /// </summary>
        public SetFilePrioritySettingsDialog()
            : this(string.Empty, EFilePriority.kPriorityDefault)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetFilePrioritySettingsDialog"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="priority">The priority.</param>
        public SetFilePrioritySettingsDialog(string fileName, EFilePriority priority)
        {
            InitializeComponent();
            FileName = fileName;
            Priority = priority;
        }

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

                var dbManager = new FileProcessingDB();
                dbManager.ConnectLastUsedDBThisProcess();
                var list = dbManager.GetPriorities();
                int size = list.Size;
                for (int i = 0; i < size; i++)
                {
                    _priorityComboBox.Items.Add(list[i].ToString());
                }

                // Select the current setting
                _priorityComboBox.Text = dbManager.AsPriorityString(Priority);

                _fileNameTextBox.Text = FileName;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31583", ex);
            }
        }

        /// <summary>
        /// Handles the ok button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Validate the file name
                if (string.IsNullOrWhiteSpace(_fileNameTextBox.Text))
                {
                    MessageBox.Show("You must specify a file to change priority for.",
                        "No File Name", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1, 0);
                    _fileNameTextBox.Focus();
                    return;
                }
                else if (new FAMTagManager().StringContainsInvalidTags(_fileNameTextBox.Text))
                {
                    MessageBox.Show("File name contains invalid document tags",
                        "Invalid Tags", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1, 0);
                    _fileNameTextBox.Focus();
                    return;
                }

                // Convert the priority string to priority enum value
                var dbManager = new FileProcessingDB();
                dbManager.ConnectLastUsedDBThisProcess();
                Priority = dbManager.AsEFilePriority(_priorityComboBox.Text);
                FileName = _fileNameTextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31584", ex);
            }
        }

        #endregion Methods
    }
}
