using Extract.FileConverter.Converters;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Extract.FileConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    sealed public partial class ConverterSettingsWindow : MetroWindow
    {
        /// <summary>
        /// A collection of converters supported by this task.
        /// </summary>
        private readonly IList<IConverter> _Converters = new ObservableCollection<IConverter>();

        /// <summary>
        /// Used to indicate to the calling object if it should save the settings.
        /// </summary>
        private bool _SaveSettings = false;

        /// <summary>
        /// Publicly accessible property for _Converters
        /// </summary>
        public IList<IConverter> Converters { get { return _Converters; } }

        /// <summary>
        /// Gets or sets the destination file format.
        /// </summary>
        public FileFormat DestinationFileFormat { get; set; } = FileFormat.Tiff;

        /// <summary>
        /// Publicly accessible property for _SaveSettings
        /// </summary>
        public bool SaveSettings { get { return _SaveSettings; } }

        /// <summary>
        /// Gets the supported destination file formats.
        /// </summary>
        public Collection<FileFormat> SupportedDestinationFormats { get; } = new Collection<FileFormat> { FileFormat.Tiff, FileFormat.Pdf };

        /// <summary>
        /// Default constructor that adds all of the supported converters.
        /// </summary>
        public ConverterSettingsWindow()
        {
            Setup();
            _Converters.Add(new OfficeConverter());
        }

        /// <summary>
        /// Initializes the ConverterSettingsWindow with its respective settings.
        /// </summary>
        /// <param name="converters">A collection of pre-configured converters</param>
        /// <param name="destinationFileFormat">The destination file format to set</param>
        public ConverterSettingsWindow(IList<IConverter> converters, FileFormat destinationFileFormat)
        {
            Setup();
            try
            {
                foreach (var converter in converters)
                {
                    this._Converters.Add(converter);
                }
                this.DestinationFileFormat = destinationFileFormat;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51672");
            }
        }

        /// <summary>
        /// Initializes the class, and assigns data contexts to items found in the XAML.
        /// </summary>
        private void Setup()
        {
            InitializeComponent();
            ConverterListBox.DataContext = this._Converters;
            DestinationFormat.DataContext = this;
        }

        /// <summary>
        /// Selects a converter when you click on them in the left pane.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Any routed arguments</param>
        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var converter = (IConverter)((StackPanel)sender).Tag;
            HideConverters();

            if (converter.ConverterName == new OfficeConverter().ConverterName)
            {
                ShowExtractOfficeConverter.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// This will collapse all converter windows (on the right side of the screen).
        /// </summary>
        private void HideConverters()
        {
            ShowExtractOfficeConverter.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Called when the user hits the save button. Sets the save settings to true so the calling program can access the property.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Any routed arguments</param>
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            this._SaveSettings = true;
            this.Close();
        }

        /// <summary>
        /// Called when the user hits the cancel button. Closes the active window.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Any routed arguments</param>
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
