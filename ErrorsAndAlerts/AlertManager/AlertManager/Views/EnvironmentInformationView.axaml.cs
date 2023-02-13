using Avalonia.Controls;

namespace AlertManager.Views
{
    public partial class EnvironmentInformationView : Window
    {
        public EnvironmentInformationView()
        {
            InitializeComponent();

            closeWindow.Click += delegate
            {
                this.Close("Close");
            };
        }
    }
}
