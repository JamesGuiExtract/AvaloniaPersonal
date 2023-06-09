﻿using Extract.Licensing.Internal;
using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ExtractLicenseUI.Database
{
    public class ExtractLicense : NotifyPropertyChanged, IDataErrorInfo
    {
        private Guid _Guid;
        private string _RequestKey;
        private string _IssuedBy;
        private DateTime _IssuedOn;
        private DateTime? _ExpiresOn = DateTime.Now.AddDays(30);
        private bool _IsActive;
        private ExtractLicense _TransferLicense;
        private ExtractLicense _UpgradedLicense;
        private string _MachineName;
        private string _Comments;
        private bool _Isproduction;
        private bool _RestrictByDiskSerialNumber = true;
        private bool _PayRoyalties;
        private string _LicenseKey;
        private bool _SignedLicenseTransferForm;
        private bool _IsPermanent;
        private string _SDKPassword = "Not Implemented";
        private string _LicenseName;
        private ExtractVersion _ExtractVersion = new ExtractVersion();
        private LicenseInfo LicenseInfo = new LicenseInfo();

        public ExtractLicense()
        {
            this.Guid = Guid.NewGuid();
            this.IssuedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            this.IssuedOn = DateTime.Now;
            this.LicenseKey = "Generated On Save";
            this.IsActive = true;
        }

        /// <summary>
        /// True or false depending on if we need to pay royalties for using a license.
        /// </summary>
        public bool PayRoyalties
        {
            get { return this._PayRoyalties; }
            set
            {
                if (this._PayRoyalties != value)
                {
                    this._PayRoyalties = value;
                    OnPropertyChanged(nameof(PayRoyalties));
                }
            }
        }

        /// <summary>
        /// The unique identifier for the license.
        /// </summary>
        public Guid Guid
        {
            get { return this._Guid; }
            set
            {
                if (this._Guid != value)
                {
                    this._Guid = value;
                    OnPropertyChanged(nameof(Guid));
                }
            }
        }

        /// <summary>
        /// Indicates if the license is permanent
        /// </summary>
        public bool IsPermanent
        {
            get { return this._IsPermanent; }
            set
            {
                if (this._IsPermanent != value)
                {
                    this._IsPermanent = value;
                    OnPropertyChanged(nameof(IsPermanent));
                    OnPropertyChanged(nameof(ExpiresOn));
                }
            }
        }

        /// <summary>
        /// Indicates if a license should be restricted by the disk serial number
        /// </summary>
        public bool RestrictByDiskSerialNumber
        {
            get { return this._RestrictByDiskSerialNumber; }
            set
            {
                if (this._RestrictByDiskSerialNumber != value)
                {
                    this._RestrictByDiskSerialNumber = value;
                    OnPropertyChanged(nameof(RestrictByDiskSerialNumber));
                }
            }
        }

        /// <summary>
        /// Also called user license string. This is what is received in emails.
        /// </summary>
        public string RequestKey
        {
            get { return this._RequestKey; }
            set
            {
                if(value != null)
                {
                    //remove all of the white space because its common for it to be copied when copying from an email.
                    this._RequestKey = value.Replace(@"\s+", string.Empty);
                }
                else
                {
                    this._RequestKey = value;
                }

                // We can ignore this exception because its possible for the request key to be null.
                try
                {
                    this.LicenseInfo.UserString = this._RequestKey;
                    this.MachineName = this.LicenseInfo.UserComputerName;
                }
                catch (Exception) {
                    this.MachineName = string.Empty;
                }

                OnPropertyChanged(nameof(RequestKey));
            }
        }

        /// <summary>
        /// Indicates the person who issues the license.
        /// </summary>
        public string IssuedBy { get { return this._IssuedBy; }
            set {
                if(this._IssuedBy != value)
                {
                    this._IssuedBy = value;
                    OnPropertyChanged(nameof(IssuedBy));
                }
            } 
        }

        /// <summary>
        /// Indicates the date/time a license was issued
        /// </summary>
        public DateTime IssuedOn
        {
            get { return this._IssuedOn; }
            set
            {
                if (this._IssuedOn != value)
                {
                    this._IssuedOn = value;
                    OnPropertyChanged(nameof(IssuedOn));
                }
            }
        }

        /// <summary>
        /// Indicates the license expiration date. If this is null it is assumed it does not expire
        /// </summary>
        public DateTime? ExpiresOn
        {
            get { return this._ExpiresOn; }
            set
            {
                if (this._ExpiresOn != value)
                {
                    this._ExpiresOn = value;
                    OnPropertyChanged(nameof(ExpiresOn));
                }
            }
        }

        /// <summary>
        /// Used to determine if the license is currently active.
        /// </summary>
        public bool IsActive
        {
            get { return this._IsActive; }
            set
            {
                if (this._IsActive != value)
                {
                    this._IsActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        /// <summary>
        /// If this license was transfered to another one, link it here.
        /// </summary>
        public ExtractLicense TransferLicense
        {
            get { return this._TransferLicense; }
            set
            {
                if (this._TransferLicense != value)
                {
                    this._TransferLicense = value;
                    OnPropertyChanged(nameof(TransferLicense));
                }
            }
        }

        /// <summary>
        /// If this license was upgraded link it here.
        /// </summary>
        public ExtractLicense UpgradedLicense
        {
            get { return this._UpgradedLicense; }
            set
            {
                if (this._UpgradedLicense != value)
                {
                    this._UpgradedLicense = value;
                    OnPropertyChanged(nameof(UpgradedLicense));
                }
            }
        }

        /// <summary>
        /// The machine name the license is issued to.
        /// </summary>
        public string MachineName
        {
            get { return this._MachineName; }
            set
            {
                if (this._MachineName != value)
                {
                    this._MachineName = value;
                    OnPropertyChanged(nameof(MachineName));
                }
            }
        }

        /// <summary>
        /// Any comments about the issued license.
        /// </summary>
        public string Comments
        {
            get { return this._Comments; }
            set
            {
                if (this._Comments != value)
                {
                    this._Comments = value;
                    OnPropertyChanged(nameof(Comments));
                }
            }
        }

        /// <summary>
        /// Indicates if this license is being used in production
        /// </summary>
        public bool IsProduction
        {
            get { return this._Isproduction; }
            set
            {
                if (this._Isproduction != value)
                {
                    this._Isproduction = value;
                    OnPropertyChanged(nameof(IsProduction));
                }
            }
        }

        /// <summary>
        /// The key generated by our license code.
        /// </summary>
        public string LicenseKey
        {
            get { return this._LicenseKey; }
            set
            {
                if (this._LicenseKey != value)
                {
                    this._LicenseKey = value;
                    this.LicenseInfo = new LicenseInfo(this._LicenseKey);
                    OnPropertyChanged(nameof(LicenseKey));
                }
            } 
        }

        /// <summary>
        /// Indicates if we have a signed transfer form somewhere.
        /// </summary>
        public bool SignedTransferForm
        {
            get { return this._SignedLicenseTransferForm; }
            set
            {
                if (this._SignedLicenseTransferForm != value)
                {
                    this._SignedLicenseTransferForm = value;
                    OnPropertyChanged("SignedLicenseTransferForm");
                }
            }
        }

        /// <summary>
        /// Sets the password for the SDK.
        /// </summary>
        public string SDKPassword
        {
            get { return this._SDKPassword; }
            set
            {
                if (this._SDKPassword != value)
                {
                    this._SDKPassword = value;
                    OnPropertyChanged(nameof(SDKPassword));
                }
            }
        }

        /// <summary>
        /// Indicates the version this license is associated with.
        /// </summary>
        public ExtractVersion ExtractVersion {
            get { return this._ExtractVersion; }
            set
            {
                this._ExtractVersion = value;
                OnPropertyChanged(nameof(ExtractVersion));
            }
        }

        /// <summary>
        /// An arbitrary name that you can set when making a license.
        /// </summary>
        public string LicenseName
        {
            get { return this._LicenseName; }
            set
            {
                if (this._LicenseName != value)
                {
                    this._LicenseName = value;
                    OnPropertyChanged(nameof(LicenseName));
                }
            }
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
                    case "LicenseName":
                        return string.IsNullOrEmpty(this.LicenseName) ? "Required Value" : null;
                    case "ExtractVersion":
                        return string.IsNullOrEmpty(ExtractVersion.Version) ? "Select a Version" : null;
                    case "ExpiresOn":
                        if (this.ExpiresOn != null && this.ExpiresOn < DateTime.Now)
                        {
                            return "Select a later Date";
                        }
                        else if (!this.IsPermanent && this.ExpiresOn == null)
                        {
                            return "Select a date";
                        }
                        break;
                    case "RequestKey":
                        if (this.RestrictByDiskSerialNumber)
                        {
                            return string.IsNullOrEmpty(this.RequestKey) ? "Required Value" : null;
                        }
                        break;
                    case "Comments":
                        if(this.Comments?.Length >= 4000)
                        {
                            return "Your comment is too long. The database is limited to 4000 characters.";
                        }
                        break;
                }

                return null;
            }
        }

        /// <summary>
        /// Saves the currently generated license to a file.
        /// </summary>
        /// <param name="fullPath">The full path (including filename) to save the license to.</param>
        public void GenerateLicenseFile(string fullPath)
        {
            this.LicenseInfo.SaveToFile(fullPath);
        }

        /// <summary>
        /// Generates a new license key.
        /// </summary>
        /// <param name="organization">The organization to generate the license for</param>
        /// <param name="packages">A list of packages to generate the license for.</param>
        public void GenerateNewLicenseKey(Organization organization, ExtractLicense license, Collection<Package> packages)
        {
            if(organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }
            if(packages == null)
            {
                throw new ArgumentNullException(nameof(packages));
            }
            if(license == null)
            {
                throw new ArgumentNullException(nameof(license));
            }

            using (var databaseReader = new DatabaseReader())
            {
                foreach (var package in packages)
                {
                    package.Components = databaseReader.ReadComponents(package);
                }
            }

            var uniqueComponentIDs = GetUniquePackgeCompontentIDs(packages);

            this.PrepareLicenseInfo(organization);

            foreach (var id in uniqueComponentIDs)
            {
                // A single day was added because there were reports that licenses were expring early.
                this.LicenseInfo.ComponentIDToInfo[(uint) id] = license.ExpiresOn != null 
                    ? new ComponentInfo(((DateTime)license.ExpiresOn).AddDays(1)) 
                    : new ComponentInfo();
            }

            this.LicenseKey = this.LicenseInfo.CreateCode();
        }

        public void GenerateUnlockCode(Organization organization, string folderPath)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }
            this.PrepareLicenseInfo(organization);
            this.LicenseInfo.GenerateUnlockCodeFile(folderPath, DateTime.Now.AddDays(30));
        }

        private void PrepareLicenseInfo(Organization organization)
        {
            if (!String.IsNullOrEmpty(this.RequestKey))
            {
                this.LicenseInfo.UserString = this._RequestKey;
            }

            this.LicenseInfo.OrganizationName = organization.CustomerName;
            this.LicenseInfo.LicenseeName = organization.Reseller;
            this.LicenseInfo.IssuerName = this._IssuedBy;
            this.LicenseInfo.UseSerialNumber = this.RestrictByDiskSerialNumber;
            this.LicenseInfo.UseComputerName = false;
            this.LicenseInfo.UseMACAddress = false;
        }

        /// <summary>
        /// Gets all of the unique package component ID's.
        /// </summary>
        /// <param name="packages">A collection of packages</param>
        /// <returns>A unique set of packgeID's</returns>
        private static HashSet<int> GetUniquePackgeCompontentIDs(Collection<Package> packages)
        {
            HashSet<int> uniqueIDs = new HashSet<int>();
            foreach(var package in packages)
            {
                foreach(var componenent in package.Components)
                {
                    uniqueIDs.Add(componenent.ComponentID);
                }
            }
            
            return uniqueIDs;
        }
    }
}
