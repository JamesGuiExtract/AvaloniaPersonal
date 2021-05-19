using System.Windows.Controls;
using System.Linq;

namespace Extract.FileConverter
{
    /// <summary>
    /// Interaction logic for ImageFormatConverter.xaml
    /// </summary>
    public sealed partial class KofaxConverterUserControl : UserControl
    {
        public KofaxConverterUserControl(ConverterSettingsWindow converterSettingsWindow)
        {
            InitializeComponent();
            SettingsPannel.DataContext = converterSettingsWindow.Converters.OfType<KofaxConverter>().First();
            KofaxFormat.ItemsSource = converterSettingsWindow.KofaxFileFormats;
        }
    }
}
