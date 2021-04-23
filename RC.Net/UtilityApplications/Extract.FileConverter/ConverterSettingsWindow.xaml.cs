using Extract.FileConverter.Converters;
using Extract.FileConverter.Pages.Utility;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Extract.FileConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    sealed public partial class ConverterSettingsWindow : MetroWindow
    {
        /// <summary>
        /// Used to indicate to the calling object if it should save the settings.
        /// </summary>
        private bool _SaveSettings = false;
        private FileFormat _DestinationFileFormat = FileFormat.Tiff;

        private bool isDirty;

        public IList<IConverter> Converters { get; } = new ObservableCollection<IConverter>() { new OfficeConverter(), new LeadtoolsConverter() };

        /// <summary>
        /// Gets or sets the destination file format.
        /// </summary>
        public FileFormat DestinationFileFormat 
        {
            get 
            {
                return this._DestinationFileFormat;
            }
            set 
            {
                this._DestinationFileFormat = value;
                this.isDirty = true;
            }
        }

        /// <summary>
        /// Publicly accessible property for _SaveSettings
        /// </summary>
        public bool SaveSettings { get { return _SaveSettings; } }

        /// <summary>
        /// Gets the supported destination file formats.
        /// </summary>
        public Collection<FileFormat> SupportedDestinationFormats { get; } = new Collection<FileFormat> { FileFormat.Tiff, FileFormat.Pdf };

        private Pages.LeadtoolsConverter _leadtoolsConverter;
        private Pages.OfficeConverter _officeConverter;

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
        public ConverterSettingsWindow(IList<IConverter> converters, FileFormat destinationFileFormat)
        {
            this.Converters.Clear();
            try
            {
                foreach (var converter in converters)
                {
                    this.Converters.Add(converter);
                }
                this.DestinationFileFormat = destinationFileFormat;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51672");
            }
            Setup();
        }

        private void ConverterSettingsWindow_Closing(object sender, CancelEventArgs e)
        {
            if(this.isDirty)
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
                    this._SaveSettings = true;
                }
            }
        }

        /// <summary>
        /// Initializes the class, and assigns data contexts to items found in the XAML.
        /// </summary>
        private void Setup()
        {
            InitializeComponent();
            ConverterListBox.DataContext = this.Converters;
            DestinationFormat.DataContext = this;
            AddInMissingOrNewConverters();
            this._officeConverter = new Pages.OfficeConverter();
            var leadToolsConverter = ((LeadtoolsConverter)this.Converters.Where(m => m.ConverterName.Equals(new LeadtoolsConverter().ConverterName)).First());
            this._leadtoolsConverter = new Pages.LeadtoolsConverter(leadToolsConverter);
            leadToolsConverter.LeadtoolsModel.PropertyChanged += propertyChanged;
            this.isDirty = false;
        }

        private void AddInMissingOrNewConverters()
        {
            var leadTools = new LeadtoolsConverter();
            if (!this.Converters.Where(m => m.ConverterName.Equals(leadTools.ConverterName)).Any())
            {
                this.Converters.Add(leadTools);
            }
            var officeConverter = new OfficeConverter();
            if (!this.Converters.Where(m => m.ConverterName.Equals(officeConverter.ConverterName)).Any())
            {
                this.Converters.Add(officeConverter);
            }
        }

        /// <summary>
        /// Called when the user hits the save button. Sets the save settings to true so the calling program can access the property.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Any routed arguments</param>
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            isDirty = false;
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
            isDirty = false;
            this.Close();
        }

        private void ConverterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDisplayedConverterSettings(((IConverter)((ItemDragAndDropListBox)sender).SelectedValue));
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            this.isDirty = true;
            ConverterListBox.SelectedValue = ((CheckBox)sender).DataContext;
            UpdateDisplayedConverterSettings((IConverter)ConverterListBox.SelectedValue);
        }

        private void UpdateDisplayedConverterSettings(IConverter converter)
        {
            if (converter != null)
            {
                ConverterSettings.Children.Clear();

                if (converter.ConverterName == new OfficeConverter().ConverterName)
                {
                    ConverterSettings.Children.Add(this._officeConverter);
                }
                if (converter.ConverterName == new LeadtoolsConverter().ConverterName)
                {
                    ConverterSettings.Children.Add(this._leadtoolsConverter);
                }
            }
        }

        void propertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.isDirty = true;
        }
    }
}
