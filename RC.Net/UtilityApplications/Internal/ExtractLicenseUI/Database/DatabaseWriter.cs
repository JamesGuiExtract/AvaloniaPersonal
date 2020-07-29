using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractLicenseUI.Database
{
    public class DatabaseWriter : IDisposable
    {
        // To detect redundant calls
        private bool _disposed = false;

        private SqlConnection SqlConnection;

        private readonly string InsertSelectedPackages = @"
INSERT INTO dbo.SelectedPackagesInLicense
(
	[Package_Guid]
    , [License_Guid]
)
VALUES
(
    @PackageGuid
    , @LicenseGuid
)";

        private readonly string InsertLicenseQuery = @"
INSERT INTO dbo.License
(
    [Guid]
	, [Organization_GUID]
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
	, [License_Name]
    , [Restrict_By_Disk_Serial_Number]
)
VALUES
(
    @Guid
    , @Organization_GUID
	, @Request_Key
	, @Issued_By
	, @Issued_On
	, @Expires_On
	, @Active
	, @Transfer_License
	, @Machine_Name
	, @Comments
	, @Production
	, @License_Key
	, @Signed_Transfer_Form
	, @SDK_Password
	, @Extract_Version_GUID
	, @License_Name
    , @Restrict_By_Disk_Serial_Number
)";

        public DatabaseWriter()
        {
            this.SqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            SqlConnection.Open();
        }

        /// <summary>
        /// Writes the selected license to the database.
        /// </summary>
        /// <param name="organization">The organizaion with the selected license</param>
        /// <returns>Returns true if successful</returns>
        public bool WriteLicense(Organization organization)
        {
            
            using (SqlCommand command = new SqlCommand(this.InsertLicenseQuery, this.SqlConnection))
            {
                command.Parameters.AddWithValue("@Guid", organization.SelectedLicense.Guid);
                command.Parameters.AddWithValue("@Organization_GUID", organization.Guid);
                if (organization.SelectedLicense.RequestKey == null)
                {
                    command.Parameters.AddWithValue("@Request_Key", DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@Request_Key", organization.SelectedLicense.RequestKey);
                }

                command.Parameters.AddWithValue("@Issued_By", organization.SelectedLicense.IssuedBy);
                command.Parameters.AddWithValue("@Issued_On", organization.SelectedLicense.IssuedOn);

                if (organization.SelectedLicense.ExpiresOn == null)
                {
                    command.Parameters.AddWithValue("@Expires_On", DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@Expires_On", organization.SelectedLicense.ExpiresOn);
                }
                command.Parameters.AddWithValue("@Active", organization.SelectedLicense.IsActive);
                if(organization.SelectedLicense.TransferLicense == null)
                {
                    command.Parameters.AddWithValue("@Transfer_License", DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@Transfer_License", organization.SelectedLicense.TransferLicense);
                }
                
                command.Parameters.AddWithValue("@Machine_Name", organization.SelectedLicense.MachineName);
                if (organization.SelectedLicense.Comments == null)
                {
                    command.Parameters.AddWithValue("@Comments", DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@Comments", organization.SelectedLicense.Comments);
                }
                command.Parameters.AddWithValue("@Production", organization.SelectedLicense.IsProduction);
                command.Parameters.AddWithValue("@License_Key", organization.SelectedLicense.LicenseKey);
                command.Parameters.AddWithValue("@Signed_Transfer_Form", organization.SelectedLicense.SignedTransferForm);

                if (organization.SelectedLicense.SDKPassword == null)
                {
                    command.Parameters.AddWithValue("@SDK_Password", DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@SDK_Password", organization.SelectedLicense.SDKPassword);
                }
                command.Parameters.AddWithValue("@Extract_Version_GUID", organization.SelectedLicense.ExtractVersion.Guid);
                command.Parameters.AddWithValue("@License_Name", organization.SelectedLicense.LicenseName);
                command.Parameters.AddWithValue("@Restrict_By_Disk_Serial_Number", organization.SelectedLicense.RestrictByDiskSerialNumber);
                command.ExecuteNonQuery();
            }

            return true;
        }

        /// <summary>
        /// Writes packages associated with a particular license to the database.
        /// </summary>
        /// <param name="selectedLicense">The license the packages were selected for</param>
        /// <param name="selectedPackages">The selected licenses.</param>
        internal void WritePackages(ExtractLicense selectedLicense, IEnumerable<Package> selectedPackages)
        {
            foreach (var package in selectedPackages)
            {
                using (SqlCommand command = new SqlCommand(this.InsertSelectedPackages, this.SqlConnection))
                {
                    command.Parameters.AddWithValue("@PackageGuid", package.Guid);
                    command.Parameters.AddWithValue("@LicenseGuid", selectedLicense.Guid);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Dispose()
        {
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
    }
}
