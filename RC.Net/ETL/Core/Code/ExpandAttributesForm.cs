﻿using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace Extract.ETL
{
    public partial class ExpandAttributesForm : Form
    {
        #region Private fields

        /// <summary>
        /// Used internally for default values for a new row
        /// </summary>
        ExpandAttributes.DashboardAttributeField _defaultsForNew = new ExpandAttributes.DashboardAttributeField();

        #endregion

        #region Constructors

        /// <summary>
        /// Form to configure ExpandAttributes database service
        /// </summary>
        /// <param name="service">Service to configure</param>
        public ExpandAttributesForm(ExpandAttributes service)
        {
            InitializeComponent();
            ExpandAttributesService = service;

            _storeSpatialInfoCheckBox.Checked = ExpandAttributesService.StoreSpatialInfo;
            _storeEmptyAttributesCheckBox.Checked = service.StoreEmptyAttributes;
            _descriptionTextBox.Text = service.Description;
            _schedulerControl.Value = service.Schedule;

            SetupDataGrid();

            dataGridView.DataSource = service.DashboardAttributes;

        }

        #endregion

        #region Public properties

        /// <summary>
        /// Service that was configured
        /// </summary>
        public ExpandAttributes ExpandAttributesService { get; }

        #endregion

        #region Event Handlers

        void HandleDataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            try
            {
                e.Row.Cells[0].Value = _defaultsForNew.DashboardAttributeName;
                e.Row.Cells[1].Value = _defaultsForNew.AttributeSetNameID;
                e.Row.Cells[2].Value = _defaultsForNew.PathForAttributeInAttributeSet;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45647");
            }
        }

        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_descriptionTextBox.Text))
                {
                    UtilityMethods.ShowMessageBox("Description cannot be empty.", "Invalid configuration", true);
                    _descriptionTextBox.Focus();
                    DialogResult = DialogResult.None;
                    return;
                }
                ExpandAttributesService.StoreEmptyAttributes = _storeEmptyAttributesCheckBox.Checked;
                ExpandAttributesService.StoreSpatialInfo = _storeSpatialInfoCheckBox.Checked;
                ExpandAttributesService.Description = _descriptionTextBox.Text;
                ExpandAttributesService.Schedule = _schedulerControl.Value;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45647");
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Sets up the data grid view columns
        /// </summary>
        void SetupDataGrid()
        {
            var dashboardAttributeNameColumn = new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "DashboardAttributeName",
                HeaderText = "Name for attribute",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            dataGridView.Columns.Add(dashboardAttributeNameColumn);
            _defaultsForNew.DashboardAttributeName = "";
            
            using (var connection = NewSqlDBConnection())
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "Select ID, Description FROM AttributeSetName";
                    DataTable attributeSets = new DataTable();
                    attributeSets.Load(cmd.ExecuteReader());
                    if (attributeSets.Rows.Count < 1)
                    {
                        ExtractException ex = new ExtractException("ELI46108", "No AttributeSets are defined.");
                        throw ex;
                    }

                    var attributeSetColumn = new DataGridViewComboBoxColumn()
                    {
                        DataPropertyName = "AttributeSetNameID",
                        HeaderText = "Attribute Set Name",
                        DataSource = attributeSets,
                        ValueMember = "ID",
                        DisplayMember = "Description",
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                    };
                    dataGridView.Columns.Add(attributeSetColumn);
                    _defaultsForNew.AttributeSetNameID = (Int64) attributeSets.Rows[0]["ID"];
                }
            }

            var pathForAttributeInAttributeSetColumn = new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "PathForAttributeInAttributeSet",
                HeaderText = "Path for attribute",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ToolTipText = @"Path to attribute using attribute name separated by \"
            };
            dataGridView.Columns.Add(pathForAttributeInAttributeSetColumn);
            _defaultsForNew.PathForAttributeInAttributeSet = "";
        }

        /// <summary>
        /// Returns a connection to the configured database
        /// </summary>
        /// <param name="enlist">Whether to enlist in a transaction scope if there is one</param>
        /// <returns>SqlConnection that connects to the <see cref="DatabaseServer"/> and <see cref="DatabaseName"/></returns>
        protected virtual SqlConnection NewSqlDBConnection(bool enlist = true)
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = ExpandAttributesService.DatabaseServer;
            sqlConnectionBuild.InitialCatalog = ExpandAttributesService.DatabaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            sqlConnectionBuild.Enlist = enlist;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }

        #endregion
    }
}
