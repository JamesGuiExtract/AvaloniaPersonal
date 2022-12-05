using Avalonia.Controls;

namespace AlertManager.Views
{
    public partial class MakeAlertView : Window
    {
        public MakeAlertView()
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
