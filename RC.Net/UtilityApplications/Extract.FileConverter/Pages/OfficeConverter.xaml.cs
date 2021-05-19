using System.Windows.Controls;

namespace Extract.FileConverter
{
    /// <summary>
    /// Interaction logic for ImageFormatConverter.xaml
    /// </summary>
    public sealed partial class OfficeConverterUserControl : UserControl
    {
        public OfficeConverterUserControl()
        {
            DataContext = this;
            InitializeComponent();
        }
    }
}
