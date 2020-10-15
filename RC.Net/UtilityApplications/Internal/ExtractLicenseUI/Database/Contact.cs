using FirstFloor.ModernUI.Presentation;
using System;
using System.ComponentModel;

namespace ExtractLicenseUI.Database
{
    public class Contact : NotifyPropertyChanged, IDataErrorInfo
    {
        public Guid Guid { get; set; } = Guid.NewGuid();

        private string _FirstName { get; set; }

        private string _LastName { get; set; }

        private string _EmailAddress { get; set; }

        private string _PhoneNumber { get; set; }

        private string _Title { get; set; }

        public string FirstName
        {
            get { return this._FirstName; }
            set
            {
                if (this._FirstName != value)
                {
                    this._FirstName = value;
                    OnPropertyChanged(nameof(FirstName));
                }
            }
        }

        public string LastName
        {
            get { return this._LastName; }
            set
            {
                if (this._LastName != value)
                {
                    this._LastName = value;
                    OnPropertyChanged(nameof(LastName));
                }
            }
        }

        public string EmailAddress
        {
            get { return this._EmailAddress; }
            set
            {
                if (this._EmailAddress != value)
                {
                    this._EmailAddress = value;
                    OnPropertyChanged(nameof(EmailAddress));
                }
            }
        }

        public string PhoneNumber
        {
            get { return this._PhoneNumber; }
            set
            {
                if (this._PhoneNumber != value)
                {
                    this._PhoneNumber = value;
                    OnPropertyChanged(nameof(PhoneNumber));
                }
            }
        }

        public string Title
        {
            get { return this._Title; }
            set
            {
                if (this._Title != value)
                {
                    this._Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string Error
        {
            get { return null; }
        }

        /// <summary>
        /// Used for WPF validation. Validates each property.
        /// </summary>
        /// <param name="columnName">The property to validate.</param>
        /// <returns></returns>
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "FirstName":
                        return string.IsNullOrEmpty(this.FirstName) ? "Required!" : null;
                    case "EmailAddress":
                        return string.IsNullOrEmpty(this.EmailAddress) ? "Required!" : null;
                }
                return null;
            }
        }
    }
}
