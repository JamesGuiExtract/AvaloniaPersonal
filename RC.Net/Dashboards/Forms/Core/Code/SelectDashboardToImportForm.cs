using System;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Dashboard.Forms
{
    public partial class SelectDashboardToImportForm : Form
    {
        #region Constructors

        public SelectDashboardToImportForm()
        {
            InitializeComponent();

        }

        public SelectDashboardToImportForm(bool dashboardNameReadOnly, string dashboardName = "")
        {
            InitializeComponent();
            DashboardName = dashboardName;
            _dashboardNameTextBox.ReadOnly = dashboardNameReadOnly;
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

        void HandleOKButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_dashboardFileTextBox.Text))
                {
                    ExtractException exFile = new ExtractException("ELI46456", "Dashboard file must be specified.");
                    _dashboardFileTextBox.Focus();
                    DialogResult = DialogResult.None;
                    throw exFile;
                }
                else if (string.IsNullOrEmpty(_dashboardNameTextBox.Text))
                {
                    ExtractException exName = new ExtractException("ELI46457", "Dashboard name must be specified.");
                    _dashboardNameTextBox.Focus();
                    DialogResult = DialogResult.None;
                    throw exName;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46454");
            }

        }

        void HandleBrowseButtonClick(object sender, EventArgs e)
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
