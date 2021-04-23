using Extract.FileConverter.Converters.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Extract.FileConverter.Pages
{
    /// <summary>
    /// Interaction logic for ImageFormatConverter.xaml
    /// </summary>
    sealed public partial class LeadtoolsConverter : UserControl
    {        
        public LeadtoolsConverter(Converters.LeadtoolsConverter leadtoolsConverterWindow)
        {
            InitializeComponent();
            this.SettingsPannel.DataContext = leadtoolsConverterWindow;
        }
    }
}
