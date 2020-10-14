using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ExtractLicenseUI.Database
{
    /// <summary>
    /// Represents a collection of packages.
    /// </summary>
    public class PackageHeader : INotifyPropertyChanged
    {
        /// <summary>
        /// The name of the package header.
        /// </summary>
        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private PropertyChangedEventHandler _propertyChangedEventHandler;

        /// <summary>
        /// The packages associated with a particular header.
        /// </summary>
        public ObservableCollection<Package> Packages { get; } = new ObservableCollection<Package>();

        /// <summary>
        /// Checks to see if any of the child packages are selected/checked.
        /// </summary>
        public bool PackagesChecked
        {
            get
            {
                return Packages.Where(m => m.IsChecked).Any();
            }
        }

        public PackageHeader()
        {
            // The code in this constructor is to detect when a child package changes
            // If it changes, it fires off the Child_PropertyChanged method.
            _propertyChangedEventHandler = Child_PropertyChanged;

            Packages.CollectionChanged += delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                // Subscribe event
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        // Subscribe
                        foreach (INotifyPropertyChanged propertyChanged in e.NewItems)
                        {
                            propertyChanged.PropertyChanged += _propertyChangedEventHandler;
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        // Unsubscribe
                        foreach (INotifyPropertyChanged propertyChanged in e.OldItems)
                        {
                            propertyChanged.PropertyChanged -= _propertyChangedEventHandler;
                        }
                        break;
                }
            };
        }

        /// <summary>
        /// Handles the PropertyChanged event of the Child control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                OnPropertyChanged(nameof(PackagesChecked));
            }
        }

        /// <summary>
        /// Called when a property has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
