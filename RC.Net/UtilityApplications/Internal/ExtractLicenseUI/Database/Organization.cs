using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using FirstFloor.ModernUI.Presentation;

namespace ExtractLicenseUI.Database
{
    public class Organization : NotifyPropertyChanged
    {

        private string _CustomerName;
        private string _Reseller;
        private string _SalesforceHyperlink = string.Empty;
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
        public string State { get; set; }

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
    }
}
