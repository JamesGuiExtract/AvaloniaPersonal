using System;
using System.ComponentModel;
using System.Windows.Forms;
using UCLID_REDACTIONCUSTOMCOMPONENTSLib;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents a control that allows the user to select an ID Shield data file path.
    /// </summary>
    [DefaultEvent("DataFileChanged")]
    [DefaultProperty("DataFile")]
    public sealed partial class DataFileControl : UserControl
    {
        #region Constants

        /// <summary>
        /// The default data file.
        /// </summary>
        const string _DEFAULT_DATA_FILE = "<SourceDocName>.voa";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The ID Shield data file selected by the user.
        /// </summary>
        string _dataFile = _DEFAULT_DATA_FILE;

        #endregion Fields

        #region Events

        /// <summary>
        /// Occurs when the data file is changed.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when the data file is changed.")]
        public event EventHandler<DataFileChangedEventArgs> DataFileChanged;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataFileControl"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public DataFileControl()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the ID Shield data file selected by the user.
        /// </summary>
        /// <value>The ID Shield data file selected by the user.</value>
        [Category("Behavior")]
        [DefaultValue(_DEFAULT_DATA_FILE)]
        [Description("The ID Shield data file selected by the user.")]
        public string DataFile
        {
            get 
            {
                return _dataFile;
            }
            set
            {
                _dataFile = value;

                UpdateLabel();
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Updates the label that indicates the ID Shield data file location.
        /// </summary>
        void UpdateLabel()
        {
            _label.Text = _dataFile == _DEFAULT_DATA_FILE ?
                "You are using default settings." : _dataFile;
        }

        #endregion Methods

        #region OnEvents

        /// <summary>
        /// Raises the <see cref="DataFileChanged"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="DataFileChanged"/> 
        /// event.</param>
        void OnDataFileChanged(DataFileChangedEventArgs e)
        {
            if (DataFileChanged != null)
            {
                DataFileChanged(this, e);
            }
        }

        #endregion OnEvents

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        void HandleCustomizeButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Create dialog
                SelectTargetFileUI dialog = new SelectTargetFileUI();
                dialog.Title = "Specify ID Shield data file path";
                dialog.Instructions = "ID Shield data file";
                dialog.DefaultFileName = _DEFAULT_DATA_FILE;
                dialog.DefaultExtension = ".voa";
                dialog.FileTypes = "VOA Files (*.voa)|*.voa||";
                dialog.FileName = _dataFile;

                // Prompt for new ID Shield data file
                if (dialog.PromptForFile())
                {
                    _dataFile = dialog.FileName;

                    UpdateLabel();

                    OnDataFileChanged(new DataFileChangedEventArgs(_dataFile));
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28530", ex);
            }
        }

        #endregion Event Handlers
    }
}
