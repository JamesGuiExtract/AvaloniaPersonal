using System.Windows.Controls;

namespace Extract.FileConverter
{
    /// <summary>
    /// Interaction logic for ImageFormatConverter.xaml
    /// </summary>
    public sealed partial class LeadtoolsConverterUserControl : UserControl
    {
        public LeadtoolsConverterUserControl(LeadtoolsConverter leadtoolsConverterWindow)
        {
            InitializeComponent();
            SettingsPannel.DataContext = leadtoolsConverterWindow;
        }
    }
}
