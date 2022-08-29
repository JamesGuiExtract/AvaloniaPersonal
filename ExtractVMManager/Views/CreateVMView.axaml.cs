using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ExtractVMManager.Views
{
    public partial class CreateVMView : UserControl
    {
        public CreateVMView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
