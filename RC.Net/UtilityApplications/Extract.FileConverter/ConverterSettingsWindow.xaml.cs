using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Extract.FileConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class ConverterSettingsWindow : MetroWindow
    {
        private DestinationFileFormat _DestinationFileFormat = DestinationFileFormat.Tif;

        public ObservableCollection<KofaxFileFormat> KofaxFileFormats { get; } = new ObservableCollection<KofaxFileFormat>() { KofaxFileFormat.None };

        private bool isDirty;

        public IList<IConverter> Converters { get; } = new ObservableCollection<IConverter>();

        /// <summary>
        /// Gets or sets the destination file format.
        /// </summary>
        public DestinationFileFormat DestinationFileFormat
        {
            get => _DestinationFileFormat;
            set
            {
                _DestinationFileFormat = value;
                isDirty = true;
                ClearAndPopulateKofaxFileFormats();
                UpdateCompressionSilder();
            }
        }

        /// <summary>
        /// Publicly accessible property for _SaveSettings
        /// </summary>
        public bool SaveSettings { get; private set; } = false;

        /// <summary>
        /// Gets the supported destination file formats.
        /// </summary>
        public Collection<DestinationFileFormat> SupportedDestinationFormats { get; } = new Collection<DestinationFileFormat> { DestinationFileFormat.Tif, DestinationFileFormat.Pdf };

        private LeadtoolsConverterUserControl _leadtoolsConverter;
        private OfficeConverterUserControl _officeConverter;
        private KofaxConverterUserControl _kofaxConverter;

        /// <summary>
        /// Default constructor that adds all of the supported converters.
        /// </summary>
        public ConverterSettingsWindow()
        {
            Setup();
        }

        /// <summary>
        /// Initializes the ConverterSettingsWindow with its respective settings.
        /// </summary>
        /// <param name="converters">A collection of pre-configured converters</param>
        /// <param name="destinationFileFormat">The destination file format to set</param>
        public ConverterSettingsWindow(IList<IConverter> converters, DestinationFileFormat destinationFileFormat)
        {
            Converters.Clear();
            try
            {
                foreach (IConverter converter in converters)
                {
                    Converters.Add(converter);
                }
                DestinationFileFormat = destinationFileFormat;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51672");
            }
            Setup();
        }

        private void ConverterSettingsWindow_Closing(object sender, CancelEventArgs e)
        {
            if (isDirty)
            {
                string msg = "Would you like to save your changes?";
                MessageBoxResult result =
                  MessageBox.Show(
                    msg,
                    "File Conversion Utility",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    SaveSettings = true;
                }
            }
        }

        /// <summary>
        /// Initializes the class, and assigns data contexts to items found in the XAML.
        /// </summary>
        private void Setup()
        {
            InitializeComponent();
            ClearAndPopulateKofaxFileFormats();
            ConverterListBox.DataContext = Converters;
            DestinationFormat.DataContext = this;
            AddInMissingOrNewConverters();
            _officeConverter = new OfficeConverterUserControl();

            LeadtoolsConverter leadToolsConverter = Converters.OfType<LeadtoolsConverter>().First();
            _leadtoolsConverter = new LeadtoolsConverterUserControl(leadToolsConverter);
            leadToolsConverter.LeadtoolsModel.PropertyChanged += ModelPropertyChange;

            KofaxConverter kofaxConverter = Converters.OfType<KofaxConverter>().First();
            _kofaxConverter = new KofaxConverterUserControl(this);
            kofaxConverter.KofaxModel.PropertyChanged += ModelPropertyChange;

            isDirty = false;
            UpdateCompressionSilder();
        }

        private void ClearAndPopulateKofaxFileFormats()
        {
            if (_kofaxConverter != null)
            {
                _kofaxConverter.KofaxFormat.SelectedItem = KofaxFileFormat.None;
            }

            KofaxFileFormats.Where(m => !m.Equals(KofaxFileFormat.None)).ToList().ForEach(m => KofaxFileFormats.Remove(m));

            //foreach(KofaxFileFormat value in toremove)
            //{
            //    KofaxFileFormats.Remove(value);
            //}

            foreach (object fileformat in Enum.GetValues(typeof(KofaxFileFormat)))
            {
                KofaxFileFormat format = (KofaxFileFormat)fileformat;
                if (format.ToString().ToUpper(CultureInfo.InvariantCulture).Contains(DestinationFileFormat.ToString().ToUpper(CultureInfo.InvariantCulture)))
                {
                    KofaxFileFormats.Add(format);
                }
            }
        }

        private void AddInMissingOrNewConverters()
        {
            if (!Converters.OfType<LeadtoolsConverter>().Any())
            {
                Converters.Add(new LeadtoolsConverter());
            }
            if (!Converters.OfType<OfficeConverter>().Any())
            {
                Converters.Add(new OfficeConverter());
            }
            if (!Converters.OfType<KofaxConverter>().Any())
            {
                Converters.Add(new KofaxConverter());
            }
        }

        /// <summary>
        /// Called when the user hits the save button. Sets the save settings to true so the calling program can access the property.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Any routed arguments</param>
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<IConverter> convertersToValidate = Converters.Where(m => m.IsEnabled);
            bool allowSave = true;
            foreach (IConverter converter in convertersToValidate)
            {
                if (converter.HasDataError)
                {
                    allowSave = false;
                    break;
                }
            }
            if (allowSave)
            {
                isDirty = false;
                SaveSettings = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please correct all validation errors before saving!", "File Conversion Utility");
            }
        }

        /// <summary>
        /// Called when the user hits the cancel button. Closes the active window.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Any routed arguments</param>
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isDirty = false;
            Close();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void ConverterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDisplayedConverterSettings((IConverter)((ItemDragAndDropListBox)sender).SelectedValue);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            isDirty = true;
            ConverterListBox.SelectedValue = ((CheckBox)sender).DataContext;
            UpdateDisplayedConverterSettings((IConverter)ConverterListBox.SelectedValue);
        }

        private void UpdateDisplayedConverterSettings(IConverter converter)
        {
            if (converter != null)
            {
                ConverterSettings.Children.Clear();

                if (converter.GetType().Equals(typeof(OfficeConverter)))
                {
                    ConverterSettings.Children.Add(_officeConverter);
                }
                if (converter.GetType().Equals(typeof(LeadtoolsConverter)))
                {
                    ConverterSettings.Children.Add(_leadtoolsConverter);
                }
                if (converter.GetType().Equals(typeof(KofaxConverter)))
                {
                    ConverterSettings.Children.Add(_kofaxConverter);
                }
                UpdateCompressionSilder();
            }
        }

        private void ModelPropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            isDirty = true;
        }

        private void UpdateCompressionSilder()
        {
            if (Converters.Count > 0 && _kofaxConverter != null)
            {
                _kofaxConverter.CompressionSlider.IsEnabled = _DestinationFileFormat.Equals(DestinationFileFormat.Pdf) && Converters.OfType<KofaxConverter>().First().IsEnabled;
            }
        }
    }
}
