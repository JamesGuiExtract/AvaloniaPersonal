using System.Windows.Controls;

namespace Extract.FileConverter.Pages
{
    /// <summary>
    /// Interaction logic for ImageFormatConverter.xaml
    /// </summary>
    sealed public partial class OfficeConverter : UserControl
    {
        public OfficeConverter()
        {
            this.DataContext = this;
            InitializeComponent();
        }
    }
}
