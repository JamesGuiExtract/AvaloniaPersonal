﻿using Extract.Database;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.ComponentModel;
using System.Data.SqlServerCe;
using System.Windows.Forms;

namespace Extract.Utilities.ContextTags
{
    /// <summary>
    /// A <see cref="Form"/> that allows creation of a new context for a
    /// <see cref="ContextTagDatabase"/>.
    /// </summary>
    public partial class CreateContextForm : Form
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(CreateContextForm).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="ContextTagDatabase"/> to edit.
        /// </summary>
        ContextTagDatabase _database;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateContextForm"/> class.
        /// </summary>
        /// <param name="connection">The <see cref="SqlCeConnection"/> of the database to edit.
        /// </param>
        /// <param name="fpsFileDir">The FPS file directory to associate with the new context.
        /// </param>
        public CreateContextForm(SqlCeConnection connection, string fpsFileDir)
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI38027",
                    _OBJECT_NAME);

                InitializeComponent();

                _database = new ContextTagDatabase(connection);

                // To avoid a user from creating contexts mapped to physical drives without specific
                // intention, only initialize FPSFileDir if it is a UNC path.
                if (fpsFileDir.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
                {
                    _fpsFileDirTextBox.Text = fpsFileDir;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38028");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if managed resources should be disposed;
        /// otherwise, <see langword="false"/>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_database != null)
                {
                    _database.Dispose();
                    _database = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the HandleOkButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_contextNameTextBox.Text))
                {
                    UtilityMethods.ShowMessageBox(
                        @"Context name must be specified.", "Context name missing", true);
                    DialogResult = DialogResult.None;
                    return;
                }

                if (string.IsNullOrWhiteSpace(_fpsFileDirTextBox.Text))
                {
                    UtilityMethods.ShowMessageBox(
                        @"FPSFileDir must be specified.", "FPSFileDir missing", true);
                    DialogResult = DialogResult.None;
                    return;
                }

                if (!_fpsFileDirTextBox.Text.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
                {
                    DialogResult response = MessageBox.Show(null, "It is recommended that " +
                        "FPSFileDir be specified with a UNC path to avoid the risk that a " +
                        "context's FPSFileDir will refer to a different actual location " +
                        "depending on what machine or drive mapping is currently being used.",
                        "UNC path recommended", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button1, 0);
                    if (response == DialogResult.No)
                    {
                        DialogResult = DialogResult.None;
                        return;
                    }
                }

                try
                {
                    var context = new ContextTableV1();
                    context.Name = _contextNameTextBox.Text;
                    context.FPSFileDir = _fpsFileDirTextBox.Text;

                    // Remove any trailing backslash to ensure as best as possible that paths
                    // will match exactly when identifying the current context.
                    if (context.FPSFileDir.EndsWith(@"\", StringComparison.OrdinalIgnoreCase))
                    {
                        context.FPSFileDir =
                            context.FPSFileDir.Substring(0, context.FPSFileDir.Length -1);
                    }

                    _database.Context.InsertOnSubmit(context);
                    _database.SubmitChanges();
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI38029");
                    DialogResult = DialogResult.None;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38030");
            }
        }

        #endregion Event Handlers
    }
}
