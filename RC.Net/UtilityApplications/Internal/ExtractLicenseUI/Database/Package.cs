using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractLicenseUI.Database
{
    public class Package : INotifyPropertyChanged
    {
        private Guid _guid;
        private string _name;
        private ExtractVersion _extractVersion = new ExtractVersion();
        private bool _isChecked;
        private bool _allowPackageModification;
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// All of the components associated with a package.
        /// </summary>
        public Collection<Component> Components { get; set; }


        /// <summary>
        /// A unique identifier for a package.
        /// </summary>
        public Guid Guid { 
            get {
                return this._guid;
            } set {
                this._guid = value;
                this.OnPropertyChanged(nameof(Guid));
            } 
        }

        /// <summary>
        /// The name of the package.
        /// </summary>
        public string Name {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
                this.OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// The version of a package.
        /// </summary>
        public ExtractVersion Version {
            get
            {
                return this._extractVersion;
            }
            set
            {
                this._extractVersion = value;
                this.OnPropertyChanged(nameof(Version));
            }
        }

        /// <summary>
        /// Used in the UI to determine if this package was selected or not.
        /// </summary>
        public bool IsChecked {
            get
            {
                return this._isChecked;
            }
            set
            {
                this._isChecked = value;
                this.OnPropertyChanged(nameof(IsChecked));
            }
        }

        /// <summary>
        /// This is for wpf control. It enables/disables the checkbox for selecting packages.
        /// This is disabled when viewing a license (you cant modify an existing license),
        /// But this is enabled when creating new ones.
        /// </summary>
        public bool AllowPackageModification
        {
            get
            {
                return this._allowPackageModification;
            }
            set
            {
                this._allowPackageModification = value;
                this.OnPropertyChanged(nameof(AllowPackageModification));
            }
        }

        /// <summary>
        /// Called when a property has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
