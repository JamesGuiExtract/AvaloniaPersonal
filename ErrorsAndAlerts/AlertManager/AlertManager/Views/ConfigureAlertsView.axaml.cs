using Avalonia.Controls;

namespace AlertManager.Views
{
    public partial class ConfigureAlertsView : Window
    {
        public ConfigureAlertsView()
        {
            InitializeComponent();
            InitializeButton();
        }

        public void CloseWindowBehind()
        {
            this.Close("Return");
        }

        private void InitializeButton()
        {

            closeWindow.Click += delegate
            {
                CloseWindowBehind();
            };
            
        }
    }
}
