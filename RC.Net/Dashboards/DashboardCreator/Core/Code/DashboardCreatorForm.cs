using Extract;
using System;
using System.Windows.Forms;

namespace DashboardCreator
{
    public partial class DashboardCreatorForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        #region Fields

        /// <summary>
        /// The currently open dashboard file
        /// </summary>
        string _dashboardFileName;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public DashboardCreatorForm()
        {
            InitializeComponent();
        } 

        #endregion

        #region Event Handlers

        private void dashboardDesigner1_DashboardSaving(object sender, DevExpress.DashboardWin.DashboardSavingEventArgs e)
        {
            try
            {
                if (e.Command == DevExpress.DashboardWin.DashboardSaveCommand.SaveAs ||
            e.Command == DevExpress.DashboardWin.DashboardSaveCommand.Save && String.IsNullOrEmpty(_dashboardFileName))
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "ESDX|*.esdx|All|*.*";
                    saveFileDialog.DefaultExt = "esdx";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        _dashboardFileName = saveFileDialog.FileName;
                        dashboardDesigner1.Dashboard.SaveToXml(_dashboardFileName);
                    }
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45303");
            }
        }

        private void dashboardDesigner1_DashboardCreating(object sender, DevExpress.DashboardWin.DashboardCreatingEventArgs e)
        {
            try
            {
                _dashboardFileName = null;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45304");
            }
        }

        private void dashboardDesigner1_DashboardChanged(object sender, EventArgs e)
        {
            try
            {
                if (_dashboardFileName is null)
                {
                    Text = "Dashboard creator";
                }
                else
                {
                    Text = dashboardDesigner1.Dashboard.Title.Text + ": " + _dashboardFileName;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45305");
            }
        }

        private void fileOpenButtonItem_ItemClick(object sender, DevExpress.XtraBars.Ribbon.BackstageViewItemEventArgs e)
        {
            try
            {
                openFile();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45306");
            }
        }


        private void fileOpenBarButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                openFile();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45307");
            }
        } 

        #endregion

        #region Helper Methods

        /// <summary>
        /// Displays OpenFileDialog to open file with ESDX extension by default or XML or all
        /// </summary>
        private void openFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ESDX|*.esdx|XML|*.xml|All|*.*";
            openFileDialog.DefaultExt = "esdx";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _dashboardFileName = openFileDialog.FileName;
                dashboardDesigner1.LoadDashboard(_dashboardFileName);
            }
        } 

        #endregion

    }
}
