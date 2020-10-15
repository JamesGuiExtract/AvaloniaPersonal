﻿using ExtractLicenseUI.Utility;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace ExtractLicenseUI.Database
{
    public class DatabaseReader : IDisposable
    {
        // To detect redundant calls
        private bool _disposed = false;

        private SqlConnection SqlConnection;

        public DatabaseReader()
        {
            this.SqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            SqlConnection.Open();
        }

        /// <summary>
        /// Reads the organizations from the database.
        /// </summary>
        /// <returns>A collection of the organizations.</returns>
        public Collection<Organization> ReadOrganizations()
        {
            Collection<Organization> organizations = new Collection<Organization>();
            using(SqlCommand command = new SqlCommand("Select GUID, SalesForce_Hyperlink, Customer_Name, Reseller, State FROM Organization", this.SqlConnection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            organizations.Add(new Organization()
                            {
                                CustomerName = reader["Customer_Name"].ToString(),
                                Reseller = reader["Reseller"].ToString(),
                                Guid = Guid.Parse(reader["Guid"].ToString()),
                                SalesforceHyperlink = reader["Salesforce_Hyperlink"]?.ToString(),
                                State = reader["State"].ToString(),
                            });
                        }
                    }
                }
            }

            return organizations;
        }

        /// <summary>
        /// Reads all of the valid versions from the database.
        /// </summary>
        /// <returns>A collection of valid versions.</returns>
        public ObservableCollection<ExtractVersion> ReadVersions()
        {
            ObservableCollection<ExtractVersion> extractVersions = new ObservableCollection<ExtractVersion>();
            using (SqlCommand command = new SqlCommand("Select GUID, Version FROM [ExtractVersion]", this.SqlConnection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            extractVersions.Add(new ExtractVersion()
                            {
                                Guid = Guid.Parse(reader["GUID"].ToString()),
                                Version = reader["Version"].ToString()
                            });
                        }
                    }
                }
            }

            return extractVersions;
        }

        /// <summary>
        /// Reads components from the database.
        /// </summary>
        /// <param name="package">The package to get the components for.</param>
        /// <returns>Returns a collection of components for a package.</returns>
        public Collection<Component> ReadComponents(Package package)
        {
            if(package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }
            Collection<Component> components = new Collection<Component>();
            string sql = @"
                SELECT 
	                Component.ID
	                , Component.Name
                FROM 
	                [dbo].PackageComponentMapping
		                INNER JOIN dbo.Component
			                ON PackageComponentMapping.Component_ID = dbo.Component.ID
			                AND dbo.PackageComponentMapping.Package_Guid = @PackageGuid";

            using (SqlCommand command = new SqlCommand(sql, this.SqlConnection))
            {
                command.Parameters.AddWithValue("@PackageGuid", package.Guid);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            components.Add(new Component() {
                                ComponentID = int.Parse(reader["ID"].ToString(), CultureInfo.InvariantCulture),
                                ComponentName = reader["Name"].ToString()
                            });
                        }
                    }
                }
            }

            return components;
        }

        /// <summary>
        /// Reads all of the packages for a given version
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public ObservableCollection<PackageHeader> ReadPackages(ExtractVersion version)
        {
            if(version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }
            using (SqlCommand command = new SqlCommand("Select GUID, Name, PackageHeader FROM [Package] WHERE Version_GUID = @Version ORDER BY PackageHeader", this.SqlConnection))
            {
                command.Parameters.AddWithValue("@Version", version.Guid);
                return ReadPackagesFromDB(command, version, true);
            }
        }

        /// <summary>
        /// Reads all of the packages for a given license
        /// </summary>
        /// <param name="license"></param>
        /// <returns></returns>
        public ObservableCollection<PackageHeader> ReadPackages(ExtractLicense license)
        {
            if(license == null)
            {
                throw new ArgumentNullException(nameof(license));
            }
            string sql = @"
                Select 
	                GUID
	                , Name
	                , PackageHeader 
                FROM 
	                [Package] 
		                INNER JOIN SelectedPackagesInLicense
			                ON SelectedPackagesInLicense.Package_Guid = [Package].[GUID]
			                AND SelectedPackagesInLicense.License_Guid = @LicenseGuid
                ORDER BY
                    PackageHeader";
            using (SqlCommand command = new SqlCommand(sql, this.SqlConnection))
            {
                command.Parameters.AddWithValue("@LicenseGuid", license.Guid);
                return ReadPackagesFromDB(command, license.ExtractVersion, false);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                this.SqlConnection?.Close();
                this.SqlConnection?.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Reads the packages from the db, and sorts them into headers.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="version">The version to read</param>
        /// <param name="allowPackageModification">If viewing a license false, if a new license true.</param>
        /// <returns></returns>
        private static ObservableCollection<PackageHeader> ReadPackagesFromDB(SqlCommand command, ExtractVersion version, bool allowPackageModification)
        {
            ObservableCollection<PackageHeader> packageHeaders = new ObservableCollection<PackageHeader>();
            
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    PackageHeader header = null;
                    while (reader.Read())
                    {
                        string packageHeader = reader["PackageHeader"].ToString();
                        if (header == null || !packageHeader.Equals(header.Name, StringComparison.CurrentCultureIgnoreCase))
                        {
                            header = new PackageHeader() { Name = packageHeader };
                            packageHeaders.Add(header);
                        }

                        header.Packages.Add(new Package()
                        {
                            Guid = Guid.Parse(reader["GUID"].ToString()),
                            Version = version,
                            Name = reader["Name"].ToString(),
                            AllowPackageModification = allowPackageModification,
                            IsChecked = !allowPackageModification,
                        });
                    }
                }
            }

            return packageHeaders;
        }


        /// <summary>
        /// Get all licenses associated with an organization.
        /// </summary>
        /// <param name="OrganizationGuid">The organization guid to read from.</param>
        /// <returns>A collection of licenses.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Its safe.")]
        public Collection<ExtractLicense> GetExtractLicenses(Guid OrganizationGuid)
        {
            Collection<ExtractLicense> extractLicenses = new Collection<ExtractLicense>();
            using (SqlCommand command = new SqlCommand(
$@"SELECT 
	License.[GUID]
    , [Request_Key]
    , [Issued_By]
    , [Issued_On]
    , [Expires_On]
    , [Active]
    , [Transfer_License]
    , [Machine_Name]
    , [Comments]
    , [Production]
    , [License_Key]
    , [Signed_Transfer_Form]
    , [SDK_Password]
    , [Extract_Version_GUID]
    , dbo.ExtractVersion.Version
    , [License_Name]
    , [Restrict_By_Disk_Serial_Number]
FROM 
	[dbo].[License]
        LEFT OUTER JOIN dbo.ExtractVersion
            ON dbo.License.Extract_Version_GUID = dbo.ExtractVersion.GUID
WHERE
    [Organization_GUID] = '{OrganizationGuid}'", this.SqlConnection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var licenseToAdd = new ExtractLicense()
                            {
                                Guid = Guid.Parse(reader["Guid"].ToString()),
                                RequestKey = reader["Request_Key"]?.ToString(),
                                IssuedBy = reader["Issued_By"].ToString(),
                                IssuedOn = DateTime.Parse(reader["Issued_On"].ToString(), CultureInfo.InvariantCulture),
                                ExpiresOn = string.IsNullOrEmpty(reader["Expires_On"]?.ToString()) ? (DateTime?)null : DateTime.Parse(reader["Expires_On"].ToString(), CultureInfo.InvariantCulture),
                                IsActive = bool.Parse(reader["Active"].ToString()),
                                TransferLicense = string.IsNullOrEmpty(reader["Transfer_License"].ToString()) ? (Guid?)null : Guid.Parse(reader["Transfer_License"].ToString()),
                                ExtractVersion = new ExtractVersion()
                                {
                                    Guid = Guid.Parse(reader["Extract_Version_GUID"].ToString()),
                                    Version = reader["Version"].ToString()
                                },
                                MachineName = reader["Machine_Name"].ToString(),
                                Comments = reader["Comments"].ToString(),
                                IsProduction = bool.Parse(reader["Production"].ToString()),
                                LicenseKey = reader["License_Key"].ToString(),
                                SignedTransferForm = bool.Parse(reader["Signed_Transfer_Form"].ToString()),
                                SDKPassword = reader["SDK_Password"].ToString(),
                                LicenseName = reader["License_Name"].ToString(),
                                RestrictByDiskSerialNumber = bool.Parse(reader["Restrict_By_Disk_Serial_Number"].ToString()),
                                IsPermanent = string.IsNullOrEmpty(reader["Expires_On"]?.ToString()) ? true : false,
                            };

                            extractLicenses.Add(licenseToAdd);
                        }
                    }
                }
            }

            return extractLicenses;
        }

        /// <summary>
        /// Gets all of the contacts for a given organization.
        /// </summary>
        /// <param name="OrganizationGuid">The GUID of the organization</param>
        /// <returns>Returns an observable collection of the organization contacts</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Its secure.")]
        public ObservableCollection<Contact> GetOrganizationContacts(Guid OrganizationGuid)
        {
            ObservableCollection<Contact> Contacts = new ObservableCollection<Contact>();
            using (SqlCommand command = new SqlCommand(
$@"SELECT 
	[GUID]
    , [First_Name]
    , [Last_Name]
    , [Email_Address]
    , [Phone_Number]
    , [Organization_GUID]
    , [Title]
FROM 
	[dbo].[Contact]
WHERE
    [Organization_GUID] = '{OrganizationGuid}'", this.SqlConnection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var contactToAdd = new Contact()
                            {
                                Guid = Guid.Parse(reader["Guid"].ToString()),
                                FirstName = reader["First_Name"].ToString(),
                                LastName = reader["Last_Name"].ToString(),
                                EmailAddress = reader["Email_Address"].ToString(),
                                PhoneNumber = reader["Phone_Number"].ToString(),
                                Title = reader["Title"].ToString(),
                            };

                            Contacts.Add(contactToAdd);
                        }
                    }
                }
            }

            return Contacts;
        }
    }
}
