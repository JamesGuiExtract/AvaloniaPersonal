using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// Forms for configuring a <see cref="LabDEOrderMapper"/> object.
    /// </summary>
    public partial class LabDEOrderMapperConfigurationForm : Form
    {
        /// <summary>
        /// The database file that was selected by this property page (this file name may
        /// contain document tags that need to be expanded - ex. &lt;SourceDocName&gt;)
        /// </summary>
        private string _databaseFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapperConfigurationForm"/> class.
        /// </summary>
        public LabDEOrderMapperConfigurationForm()
            : this(databaseFile: null, requireMandatoryTests: false,
                useFilledRequirement: true, useOutstandingOrders: false,
                requirementsAreOptional: true, eliminateDuplicateTestSubAttributes: true,
                skipSecondPass: false, addESNamesAttribute: true, addESTestCodesAttribute: false,
                setFuzzyType: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapperConfigurationForm"/> class.
        /// </summary>
        /// <param name="databaseFile">The name of the database file to display in the
        /// text box.</param>
        /// <param name="requireMandatoryTests">Whether mandatory tests are required or not.</param>
        /// <param name="useFilledRequirement">Whether the filled requirement of an order should be
        /// used when deciding whether an order can be used.</param>
        /// <param name="useOutstandingOrders">Whether only orders with codes matching the
        /// provided OutstandingOrderCode attributes should be used. If <see langword="false"/> then
        /// the outstanding order codes will be used if possible but other codes will be considered
        /// if necessary.</param>
        /// <param name="requirementsAreOptional">Whether filled/mandatory requirements can be disregarded if necessary</param>
        /// <param name="eliminateDuplicateTestSubAttributes">Whether to eliminate duplicate Test
        /// subattributes after mapping is finished.</param>
        /// <param name="skipSecondPass">Whether to skip the second pass of the mapping algorithm</param>
        /// <param name="addESNamesAttribute">Whether add an ESName attribute to mapped components</param>
        /// <param name="addESTestCodesAttribute">Whether to add an ESTestCodes attribute to components to show
        /// all mappings that were available</param>
        /// <param name="setFuzzyType">Whether set type of components to Fuzzy if they were mapped using a fuzzy
        /// regex pattern</param>
        public LabDEOrderMapperConfigurationForm(string databaseFile, bool requireMandatoryTests,
            bool useFilledRequirement, bool useOutstandingOrders,
            bool requirementsAreOptional, bool eliminateDuplicateTestSubAttributes,
            bool skipSecondPass,
            bool addESNamesAttribute,
            bool addESTestCodesAttribute,
            bool setFuzzyType)
        {
            try
            {
                InitializeComponent();

                _databaseFile = databaseFile;
                _textDatabaseFile.Text = _databaseFile ?? "";
                _checkRequireMandatoryTests.Checked = requireMandatoryTests;
                _checkUseFilledRequirement.Checked = useFilledRequirement;
                _checkUseOutstandingOrders.Checked = useOutstandingOrders;
                _checkRequirementsAreOptional.Checked = requirementsAreOptional;
                _checkEliminateDuplicateTestSubAttributes.Checked = eliminateDuplicateTestSubAttributes;
                _checkSkipSecondPass.Checked = skipSecondPass;
                _checkAddESNamesAttribute.Checked = addESNamesAttribute;
                _checkAddESTestCodesAttribute.Checked = addESTestCodesAttribute;
                _checkSetFuzzyType.Checked = setFuzzyType;
                SetEnabledStates();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26197", ex);
            }
        }

        #region Properties

        /// <summary>
        /// Gets the database file name.
        /// </summary>
        public string DatabaseFileName
        {
            get
            {
                return _databaseFile;
            }
        }

        /// <summary>
        /// Gets whether mandatory tests are required or not.
        /// </summary>
        public bool RequireMandatoryTests
        {
            get
            {
                return _checkRequireMandatoryTests.Checked;
            }
        }

        /// <summary>
        /// Gets whether to require that orders meet their filled requirement
        /// </summary>
        public bool UseFilledRequirement
        {
            get
            {
                return _checkUseFilledRequirement.Checked;
            }
        }

        /// <summary>
        /// Gets whether to limit orders to be considered based on known, outstanding order codes
        /// </summary>
        public bool UseOutstandingOrders
        {
            get
            {
                return _checkUseOutstandingOrders.Checked;
            }
        }

        /// <summary>
        /// Whether filled/mandatory requirements can be disregarded if doing so would increase the number of mapped components
        /// </summary>
        public bool RequirementsAreOptional
        {
            get
            {
                return _checkRequirementsAreOptional.Checked;
            }
        }

        /// <summary>
        /// Whether to remove any duplicate Test sub-attributes after the mapping is finished.
        /// </summary>
        public bool EliminateDuplicateTestSubAttributes
        {
            get
            {
                return _checkEliminateDuplicateTestSubAttributes.Checked;
            }
        }

        /// <summary>
        /// Whether to skip second pass of the order mapping algorithm
        /// </summary>
        public bool SkipSecondPass
        {
            get
            {
                return _checkSkipSecondPass.Checked;
            }
        }

        /// <summary>
        /// Gets or sets whether add an ESName attribute to mapped components
        /// </summary>
        public bool AddESNamesAttribute
        {
            get
            {
                return _checkAddESNamesAttribute.Checked;
            }
        }

        /// <summary>
        /// Gets or sets whether to add an ESTestCodes attribute to components to show all mappings
        /// that were available
        /// </summary>
        public bool AddESTestCodesAttribute
        {
            get
            {
                return _checkAddESTestCodesAttribute.Checked;
            }
        }

        /// <summary>
        /// Gets or sets whether set type of components to Fuzzy if they were mapped using a fuzzy
        /// regex pattern
        /// </summary>
        public bool SetFuzzyType
        {
            get
            {
                return _checkSetFuzzyType.Checked;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="T:CheckBox.CheckChanged"/> event for
        /// various <see cref="System.Windows.Forms.CheckBox"/>s.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCheckChanged(object sender, System.EventArgs e)
        {
            try
            {
                SetEnabledStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39144");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the OK button.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleOkButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Store the text from the text box
                _databaseFile = _textDatabaseFile.Text;

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26200", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the cancel button.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleCancelClicked(object sender, EventArgs e)
        {
            try
            {
                _databaseFile = "";

                this.DialogResult = DialogResult.Cancel;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26201", ex);
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Sets the enabled states of all controls
        /// </summary>
        void SetEnabledStates()
        {
            _checkRequirementsAreOptional.Enabled = _checkUseFilledRequirement.Checked || _checkRequireMandatoryTests.Checked;
        }

        #endregion Private Members
    }
}
