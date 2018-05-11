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
                // The dialog is opened in a thread to avoid hang when the dialog is being used in a thread / process that 
                // uses MTA threading 
                // https://extract.atlassian.net/browse/ISSUE-15385
                string fileName = _dashboardFileTextBox.Text; 
                DialogResult dlgResult = DialogResult.Cancel;
                Thread dialogThread = new Thread ((ThreadStart) delegate
                {
                    OpenFileDialog openDialog = new OpenFileDialog();
                    openDialog.FileName = fileName;
                    openDialog.Filter = "ESDX|*.esdx|XML|*.xml|All|*.*";
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        fileName = openDialog.FileName;
                        dlgResult = DialogResult.OK;
                    }
                });

                dialogThread.TrySetApartmentState(ApartmentState.STA);
                dialogThread.Start();
                dialogThread.Join();
                if (dlgResult == DialogResult.OK)
                {
                    _dashboardFileTextBox.Text = fileName;
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
