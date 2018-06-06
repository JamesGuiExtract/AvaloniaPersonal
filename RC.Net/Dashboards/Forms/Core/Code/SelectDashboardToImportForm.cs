using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Extract.Dashboard.Forms
{
    public partial class SelectDashboardToImportForm : Form
    {
        #region Constructors

        public SelectDashboardToImportForm()
        {
            InitializeComponent();

        }

        #endregion

        #region Public properties

        /// <summary>
        /// Database name to import dashboard to
        /// </summary>
        public string DashboardName
        {
            get
            {
                return _dashboardNameTextBox.Text;
            }

            set
            {
                _dashboardNameTextBox.Text = value;
            }
        }

        /// <summary>
        /// Server to import dashboard to
        /// </summary>
        public string DashboardFile
        {
            get
            {
                return _dashboardFileTextBox.Text;
            }

            set
            {
                _dashboardFileTextBox.Text = value;
            }
        }

        #endregion

        #region Events

        private void HandleBrowseButtonClick(object sender, EventArgs e)
        {
            try
            {
                    OpenFileDialog openDialog = new OpenFileDialog();
                    openDialog.FileName = _dashboardFileTextBox.Text;
                    openDialog.Filter = "ESDX|*.esdx|XML|*.xml|All|*.*";
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        _dashboardFileTextBox.Text = openDialog.FileName;
                    }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45769");
            }
        }

        #endregion
    }
}
