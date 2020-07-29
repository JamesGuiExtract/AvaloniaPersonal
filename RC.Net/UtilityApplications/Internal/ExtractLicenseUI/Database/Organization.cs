using System;
using System.Collections.ObjectModel;
using FirstFloor.ModernUI.Presentation;

namespace ExtractLicenseUI.Database
{
    public class Organization : NotifyPropertyChanged
    {

        private string _CustomerName;
        private string _Reseller;
        private string _SalesforceHyperlink = string.Empty;
        private ExtractLicense _SelectedLicense = new ExtractLicense();
        private Collection<ExtractLicense> _Licenses = new Collection<ExtractLicense>();

        /// <summary>
        /// A unique identifier for the organization.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The name of the customer.
        /// </summary>
        public string CustomerName { 
            get { return this._CustomerName; } 
            set { 
                if(this._CustomerName != value)
                {
                    this._CustomerName = value;
                    OnPropertyChanged("CustomerName");
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
                    OnPropertyChanged("Reseller");
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
                OnPropertyChanged("SalesforceHyperlink");
            } 
        }

        /// <summary>
        /// All of the licenses associated with a particular customer.
        /// </summary>
        public Collection<ExtractLicense> Licenses {
            get { return this._Licenses; }
            set
            {
                this._Licenses = value;
                OnPropertyChanged("Licenses");
            }
        }

        /// <summary>
        /// The active selected license to be modified.
        /// </summary>
        public ExtractLicense SelectedLicense {
            get { return this._SelectedLicense; }
            set
            {
                this._SelectedLicense = value;
                OnPropertyChanged("SelectedLicense");
            }
        }
    }
}
