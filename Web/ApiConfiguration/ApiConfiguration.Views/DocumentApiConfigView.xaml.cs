using System;

namespace Extract.Web.ApiConfiguration.Views
{
    public partial class DocumentApiConfigView
    {
        public DocumentApiConfigView()
        {
            InitializeComponent();

            // Set focus on the first field in this control if it is newly-created so that a user can start
            // editing with the keyboard immediately after adding a new config
            Dispatcher.BeginInvoke(() =>
            {
                if (configurationName.Text == "New Configuration")
                {
                    configurationName.SelectAll();
                    configurationName.Focus();
                }
            });
        }
    }
}
