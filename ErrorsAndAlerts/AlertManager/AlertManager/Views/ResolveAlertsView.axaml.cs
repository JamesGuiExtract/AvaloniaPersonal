using Avalonia.Controls;

namespace AlertManager.Views
{
    public partial class ResolveAlertsView : Window
    {
        public ResolveAlertsView()
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
