﻿using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows configuration of a <see cref="ValueConditionSelector"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class ValueConditionSelectorSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(ValueConditionSelectorSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueConditionSelectorSettingsDialog"/>
        /// class.
        /// </summary>
        /// <param name="settings">The <see cref="ValueConditionSelector"/> instance to configure.</param>
        public ValueConditionSelectorSettingsDialog(ValueConditionSelector settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleSetEditorUIObject,
                    "ELI33753", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33754");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="ValueConditionSelector"/> to configure.
        /// </summary>
        /// <value>The <see cref="ValueConditionSelector"/> to configure.</value>
        public ValueConditionSelector Settings
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
                    _configureConditionControl.ConfigurableObject =
                        (ICategorizedComponent)Settings.Condition;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33755");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// In the case that the OK button is clicked, validates the settings, applies them, and
        /// closes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // If there are invalid settings, prompt and return without closing.
                if (WarnIfInvalid())
                {
                    return;
                }

                Settings.Condition = (IAFCondition)_configureConditionControl.ConfigurableObject;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33756");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            if (_configureConditionControl.ConfigurableObject == null)
            {
                _configureConditionControl.Focus();
                UtilityMethods.ShowMessageBox("Please specify a condition to use.",
                    "Specify condition", false);
                return true;
            }

            IMustBeConfiguredObject configurable =
                _configureConditionControl.ConfigurableObject as IMustBeConfiguredObject;
            if (configurable != null && !configurable.IsConfigured())
            {
                _configureConditionControl.Focus();
                UtilityMethods.ShowMessageBox("The selected condition has not been properly configured.",
                    "Condition not configured", false);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
