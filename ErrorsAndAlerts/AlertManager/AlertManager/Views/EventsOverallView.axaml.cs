using Avalonia.Controls;

namespace AlertManager.Views
{
    public partial class EventsOverallView : Window
    {
        public EventsOverallView()
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
