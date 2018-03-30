using System;
using System.Windows.Forms;

namespace Extract.Dashboard.Forms
{
    public partial class DashboardFileDetailConfigurationForm : Form
    {
        public string RowQuery
        {
            get
            {
                return _rowQueryTextBox.Text;
            }

            set
            {
                if (_rowQueryTextBox.Text != value)
                {
                    _rowQueryTextBox.Text = value;
                }
            }
        }

        public DashboardFileDetailConfigurationForm(string title, string rowQuery)
        {
            InitializeComponent();
            Text = title;
            RowQuery = rowQuery;
        }

        void HandleOKButtonClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
