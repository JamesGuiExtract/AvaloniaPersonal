using Avalonia.Controls;

namespace AlertManager.Views
{
    public partial class AlertDetailsView : Window
    {

        public AlertDetailsView()
        {
            InitializeComponent();

            
            InitializeCloseButton();
        }

        public void CloseWindowBehind()
        {
            this.Close("Return");
        }

        private void InitializeCloseButton()
        {

            closeWindow.Click += delegate
            {
                CloseWindowBehind();
            };

        }
    }
}
