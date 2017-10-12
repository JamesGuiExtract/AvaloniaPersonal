using DevExpress.DashboardCommon;
using DevExpress.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Extract;

namespace DashboardViewer
{
    public partial class DashboardViewerForm : DevExpress.XtraEditors.XtraForm
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public DashboardViewerForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructs DashboardViewerform and opening the given file
        /// </summary>
        /// <param name="fileName">File containing dashboard to open</param>
        public DashboardViewerForm(string fileName)
        {
            try
            {
                InitializeComponent();

                dashboardViewerMain.DashboardSource = fileName;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45310");
            }
        }

        #endregion

        #region Menu Events

        void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "ESDX|*.esdx|XML|*.xml|All|*.*";
                openFileDialog.DefaultExt = "esdx";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    dashboardViewerMain.DashboardSource = "";
                    dashboardViewerMain.DashboardSource = openFileDialog.FileName;
                }

            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45309");
            }
        }

        void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                dashboardViewerMain.DashboardSource = "";
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45308");
            }
        }

         private void dashboardViewerMain_DashboardChanged(object sender, EventArgs e)
        {
            try
            {
                if (dashboardViewerMain.Dashboard is null)
                {
                    Text = "Dashboard Viewer";
                }
                else
                {
                    Text = dashboardViewerMain.Dashboard.Title.Text;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45311");
            }
        }
       
        #endregion
    }
}
