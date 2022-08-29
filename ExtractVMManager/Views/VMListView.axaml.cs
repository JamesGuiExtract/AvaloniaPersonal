using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ExtractVMManager.Views
{
    public partial class VMListView : UserControl
    {
        public VMListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
