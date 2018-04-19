using System;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Represents a dialog that allows the user to add a new MLModel to the FAM DB
    /// </summary>
    [CLSCompliant(false)]
    public partial class AddMLModel : Form
    {
        #region Fields

        FileProcessingDB _database;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The newly created model name
        /// </summary>
        public string NewValue { get; private set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AddMLModel"/> class.
        /// </summary>
        /// <param name="database">File processing DB</param>
        /// <param name="prefix">Optional prefix to insert into the text box</param>
        public AddMLModel(FileProcessingDB database, string prefix = null)
        {
            InitializeComponent();

            _database = database;

            if (!string.IsNullOrEmpty(prefix))
            {
                _nameTextBox.Text = prefix;
                _nameTextBox.SelectionStart = _nameTextBox.TextLength;
            }
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                var name = _nameTextBox.Text.Trim();
                _database.DefineNewMLModel(name);

                NewValue = name;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45125");
            }
        }

        #endregion Event Handlers
    }
}