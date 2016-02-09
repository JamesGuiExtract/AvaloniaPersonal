using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Extract.SQLCDBEditor;
using System.Data.SqlServerCe;
using Extract.Licensing;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ComponentsPlugin : SQLCDBEditorPlugin
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(SQLCDBEditorPlugin).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="SqlCeConnection"/> for the database to be edited.
        /// </summary>
        SqlCeConnection _connection;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentsPlugin"/> class.
        /// </summary>
        public ComponentsPlugin()
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
                LicenseUtilities.ValidateLicense(LicenseIdName.LabDECoreObjects, "ELI0",
                    _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI0");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Allows plugin to initialize.
        /// </summary>
        /// <param name="pluginManager">The <see cref="ISQLCDBEditorPluginManager"/> manager for
        /// this plugin.</param>
        /// <param name="connection">The <see cref="SqlCeConnection"/> for use by the plugin.</param>
        public override void LoadPlugin(ISQLCDBEditorPluginManager pluginManager,
            SqlCeConnection connection)
        {
            try
            {
                ExtractException.Assert("ELI0", "Null argument exception", connection != null);

                _connection = connection;

                RefreshData();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI0");
            }
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public override string DisplayName
        {
            get
            {
                return "Components";
            }
        }

        /// <summary>
        /// Indicates whether the plugin's <see cref="Control"/> should be displayed in the
        /// <see cref="QueryAndResultsControl"/>.
        /// </summary>
        /// <value><see langword="true"/> if the plugin's control should be displayed;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public override bool DisplayControl
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this plugin's data is valid.
        /// </summary>
        /// <value><see langword="true"/> if the plugin data is valid; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public override bool DataIsValid
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Performs any custom refresh logic needed by the plugin. Generally a plugin where
        /// <see cref="SQLCDBEditorPlugin.ProvidesBindingSource"/> is <see langword="true"/> will
        /// need to perform the refresh of the data here.
        /// </summary>
        public override void RefreshData()
        {
            try
            {
                base.RefreshData();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI0");
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        #endregion Event Handlers

        #region Private Members


        #endregion Private Members
    }
}
