using Avalonia.Controls;

namespace AlertManager.Views
{
    public partial class EventListWindowView : Window
    {
        public EventListWindowView()
        {
            InitializeComponent();

            closeWindow.Click += delegate
            {
                this.Close("Close");
            };
        }
    }
}
