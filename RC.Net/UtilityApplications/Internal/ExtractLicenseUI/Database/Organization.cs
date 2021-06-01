using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using FirstFloor.ModernUI.Presentation;

namespace ExtractLicenseUI.Database
{
    public class Organization : NotifyPropertyChanged, IDataErrorInfo
    {

        private string _CustomerName = string.Empty;
        private string _Reseller = string.Empty;
        private string _SalesforceHyperlink = string.Empty;
        private string _State = string.Empty;
        private string _SalesForceAccountID = string.Empty;
        private Collection<ExtractLicense> _Licenses = new Collection<ExtractLicense>();
        private ObservableCollection<Contact> _Contacts = new ObservableCollection<Contact>();

        public Organization()
        {
            this.Contacts.CollectionChanged += delegate
            {
                OnPropertyChanged(nameof(Contacts));
            };
        }

        /// <summary>
        /// A unique identifier for the organization.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The state the customer is in.
        /// </summary>
        public string State
        {
            get { return this._State; }
            set
            {
                if (this._State != value)
                {
                    this._State = value;
                    OnPropertyChanged(nameof(_State));
                }
            }
        }

        /// <summary>
        /// Gets or sets the sales force account ID.
        /// </summary>
        public string SalesForceAccountID
        {
            get { return this._SalesForceAccountID; }
            set
            {
                if (this._SalesForceAccountID != value)
                {
                    this._SalesForceAccountID = value;
                    OnPropertyChanged(nameof(_SalesForceAccountID));
                }
            }
        }

        /// <summary>
        /// The name of the customer.
        /// </summary>
        public string CustomerName { 
            get { return this._CustomerName; } 
            set { 
                if(this._CustomerName != value)
                {
                    this._CustomerName = value;
                    OnPropertyChanged(nameof(CustomerName));
                }
            } 
        }

        /// <summary>
        /// The name of the reseller.
        /// </summary>
        public string Reseller {
            get { return this._Reseller; }
            set {
                if(this._Reseller != value)
                {
                    this._Reseller = value;
                    OnPropertyChanged(nameof(Reseller));
                }
            } 
        }

        /// <summary>
        /// The salesforce hyperlink.
        /// </summary>
        public string SalesforceHyperlink { 
            get { return this._SalesforceHyperlink; } 
            set { 
                if (!String.IsNullOrEmpty(value)) 
                { 
                    this._SalesforceHyperlink = value;
                } 
                else
                {
                    this._SalesforceHyperlink = @"https://www.salesforce.com/";
                }
                OnPropertyChanged(nameof(SalesforceHyperlink));
            } 
        }


        /// <summary>
        /// All of the licenses associated with a particular customer.
        /// </summary>
        public Collection<ExtractLicense> Licenses {
            get 
            {
                return new Collection<ExtractLicense>(this._Licenses.OrderByDescending(x => x.IssuedOn).ToList());
            }
            set
            {
                this._Licenses = value;
                OnPropertyChanged(nameof(Licenses));
            }
        }

        /// <summary>
        /// All of the contacts associated with an organization.
        /// </summary>
        public ObservableCollection<Contact> Contacts
        {
            get { return this._Contacts; }
            set
            {
                this._Contacts = value;
                OnPropertyChanged(nameof(Contacts));
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Organization organization &&
                   Guid.Equals(organization.Guid);
        }

        public override int GetHashCode()
        {
            return this.Guid.GetHashCode();
        }

        /// <summary>
        /// Used for the IDataError interface.
        /// </summary>
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
                    case "CustomerName":
                        return string.IsNullOrEmpty(this.CustomerName) ? "CustomerName is required!" : null;
                    case "SalesForceAccountID":
                        return string.IsNullOrEmpty(this.SalesForceAccountID) ? "SalesForce Account ID is required!" : null;
                }

                return null;
            }
        }
    }
}
