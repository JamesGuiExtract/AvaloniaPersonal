using Extract.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;

namespace Extract.AttributeFinder.Forms
{
    /// <summary>
    /// Form used to configure RunMode
    /// </summary>
    [ComVisible(true)]
    [Guid("4E3CADA1-A857-42D0-A326-21B7BEE58D75")]
    [CLSCompliant(false)]
    public partial class ConfigureRunModeForm : Form, IRunModeConfigure
    {
        #region Constructors 
        
        /// <summary>
        /// Constructor for form
        /// </summary>
        public ConfigureRunModeForm()
        {
            try
            {
                InitializeComponent();

                // Add the <PageContent> and <PageNumber> tags for the Parent value
                _parentValuePathTagsButton.PathTags.AddTag("<PageContent>", null);
                _parentValuePathTagsButton.PathTags.AddTag("<PageNumber>", null);

                // Set filter so that only <SourceDocName>, <PageContent> and <PageNumber> tags are displayed.
                _parentValuePathTagsButton.PathTags.BuiltInTagFilter = new[] { SourceDocumentPathTags.SourceDocumentTag, "<PageContent>", "<PageNumber>" };
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39486", ex);
            }
        }

        #endregion

        #region IRunModeConfigure members

        /// <summary>
        /// Method used to configure Run mode settings
        /// </summary>
        /// <param name="pRunMode">The <see cref="IRunMode"/> object that is being configured</param>
        /// <param name="nHandle">The handle to the parent window</param>
        public void ConfigureRunMode(IRunMode pRunMode, int nHandle)
        {
            try
            {
                // initialize the values
                _passVOAtoOutputRadioButton.Checked = pRunMode.RunMode == ERuleSetRunMode.kPassInputVOAToOutput;
                _runByDocumentRadioButton.Checked = pRunMode.RunMode == ERuleSetRunMode.kRunPerDocument;
                _runByPageRadioButton.Checked = pRunMode.RunMode == ERuleSetRunMode.kRunPerPage;
                _runOnPaginationDocumentsRadioButton.Checked = pRunMode.RunMode == ERuleSetRunMode.kRunPerPaginationDocument;
                _insertUnderParentCheckBox.Checked = pRunMode.InsertAttributesUnderParent;
                _deepCopyInputAttributesCheckBox.Checked = pRunMode.DeepCopyInput;
                _parentAttributeTextBox.Text = pRunMode.InsertParentName;
                _parentValueTextBox.Text = pRunMode.InsertParentValue;

                // If the parent attribute value is empty set it to "Page"
                if (String.IsNullOrEmpty(_parentAttributeTextBox.Text))
                {
                    _parentAttributeTextBox.Text = "Page";
                }

                enableControls();

                // Display the dialog centered on the parent
                NativeWindow parentWindow = new NativeWindow();
                parentWindow.AssignHandle((IntPtr)nHandle);
                if (ShowDialog(parentWindow) == DialogResult.OK)
                {
                    if (_passVOAtoOutputRadioButton.Checked)
                    {
                        pRunMode.RunMode = ERuleSetRunMode.kPassInputVOAToOutput;
                    }
                    else if (_runByDocumentRadioButton.Checked)
                    {
                        pRunMode.RunMode = ERuleSetRunMode.kRunPerDocument;
                    }
                    else if (_runByPageRadioButton.Checked)
                    {
                        pRunMode.RunMode = ERuleSetRunMode.kRunPerPage;
                    }
                    else if (_runOnPaginationDocumentsRadioButton.Checked)
                    {
                        pRunMode.RunMode = ERuleSetRunMode.kRunPerPaginationDocument;
                    }
                    pRunMode.InsertAttributesUnderParent = _insertUnderParentCheckBox.Checked;
                    pRunMode.InsertParentName = _parentAttributeTextBox.Text;
                    pRunMode.InsertParentValue = _parentValueTextBox.Text;
                    pRunMode.DeepCopyInput = _deepCopyInputAttributesCheckBox.Checked;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39487", ex);
            }
        }
        
        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the CheckedChanged event on _passVOAtoOutputRadioButton control
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleVOAtoOutput_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                enableControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39488", ex);
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event on the _insertUnderParentCheckBox control
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleInsertUnderParent_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // InserUnderParent is required to be checked if ByPage mode so
                // if ByPage is selected set InsertUnderParent to be checked
                if (_runByPageRadioButton.Checked)
                {
                    _insertUnderParentCheckBox.Checked = true;
                }
                enableControls();

            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39489", ex);
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event on the _runByPageRadioButton control
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleByPage_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // if RunByPage is checked InsertUnderParent is Required to be checked
                if (_runByPageRadioButton.Checked)
                {
                    _insertUnderParentCheckBox.Checked = true;
                    enableControls();
                }

            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39490", ex);
            }
        }
        
        /// <summary>
        /// Handles the Validating event for the _parentAttributeTextBox 
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleParentAttributeTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Make sure that if _insertUnderParentCheckBox is checked the attribute value is valid
                if (_insertUnderParentCheckBox.Enabled &&
                    _insertUnderParentCheckBox.Checked &&
                    !UtilityMethods.IsValidIdentifier(_parentAttributeTextBox.Text))
                {
                    _errorProvider.SetError(_parentAttributeTextBox, "Requires valid attribute name.");
                    e.Cancel = true;
                }
                else
                {
                    _errorProvider.SetError(_parentAttributeTextBox, String.Empty);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39491", ex);
            }
        }

        private void HandleRunOnPaginationDocumentsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            enableControls();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method to enable or disable the controls based on the values.
        /// </summary>
        void enableControls()
        {
            _deepCopyInputAttributesCheckBox.Enabled = _passVOAtoOutputRadioButton.Checked;
            _insertUnderParentCheckBox.Enabled = !_runOnPaginationDocumentsRadioButton.Checked;

            bool enableParentAttributeSettings = _insertUnderParentCheckBox.Enabled && _insertUnderParentCheckBox.Checked;
            _parentAttributeTextBox.Enabled = enableParentAttributeSettings;
            _parentValueTextBox.Enabled = enableParentAttributeSettings;
            _parentValuePathTagsButton.Enabled = enableParentAttributeSettings;
        }

        #endregion

    }
}
