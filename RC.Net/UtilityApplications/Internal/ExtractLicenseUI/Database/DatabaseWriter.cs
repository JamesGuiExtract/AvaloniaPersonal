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

        private readonly string InsertUpdateContactSQL = @"
IF NOT EXISTS(SELECT GUID FROM dbo.Contact WHERE GUID like @GUID)
BEGIN
INSERT INTO 
	dbo.Contact(GUID, First_Name, Last_Name, Email_Address, Phone_Number, Title, Organization_GUID) 
VALUES 
	(@GUID, @First_Name, @Last_Name, @Email_Address, @Phone_Number, @Title, @Organization_GUID)
END

UPDATE 
	dbo.Contact
SET
	First_Name = @First_Name
	, Last_Name = @Last_Name
	, Email_Address = @Email_Address
	, Phone_Number = @Phone_Number
	, Title = @Title
    , Organization_GUID = @Organization_GUID
WHERE
    dbo.Contact.GUID = @GUID
";

        private readonly string DeleteContactSQL = @"
DELETE FROM dbo.Contact WHERE Guid = @Guid";

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

        private readonly string InsertUpdateLicenseQuery = @"
IF NOT EXISTS(SELECT GUID FROM dbo.License WHERE GUID like @GUID)
BEGIN
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
	)
END

UPDATE
	dbo.License
SET
	Comments = @Comments
	, Signed_Transfer_Form = @Signed_Transfer_Form
	, Active = @Active
	, Production = @Production
	, License_Name = @License_Name
WHERE
	dbo.License.GUID = @GUID";

        public DatabaseWriter()
        {
            this.SqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            SqlConnection.Open();
        }


        /// <summary>
        /// Writes the selected license to the database.
        /// </summary>
        /// <param name="organization">The organization with the selected license</param>
        /// <returns>Returns true if successful</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "The query is using Microsoft parameters. Its safe.")]
        public bool WriteLicense(Organization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }
            using (SqlCommand command = new SqlCommand(InsertUpdateLicenseQuery, SqlConnection))
            {
                command.Parameters.AddWithValue("@Guid", organization.SelectedLicense.Guid);
                command.Parameters.AddWithValue("@Organization_GUID", organization.Guid);
                command.Parameters.AddWithValue("@Request_Key", HandleNullTypes(organization.SelectedLicense.RequestKey));
                command.Parameters.AddWithValue("@Issued_By", organization.SelectedLicense.IssuedBy);
                command.Parameters.AddWithValue("@Issued_On", organization.SelectedLicense.IssuedOn);
                command.Parameters.AddWithValue("@Expires_On", HandleNullTypes(organization.SelectedLicense.ExpiresOn));
                command.Parameters.AddWithValue("@Active", organization.SelectedLicense.IsActive);
                command.Parameters.AddWithValue("@Transfer_License", HandleNullTypes(organization.SelectedLicense.TransferLicense));
                command.Parameters.AddWithValue("@Machine_Name", organization.SelectedLicense.MachineName);
                command.Parameters.AddWithValue("@Comments", HandleNullTypes(organization.SelectedLicense.Comments));
                command.Parameters.AddWithValue("@Production", organization.SelectedLicense.IsProduction);
                command.Parameters.AddWithValue("@License_Key", organization.SelectedLicense.LicenseKey);
                command.Parameters.AddWithValue("@Signed_Transfer_Form", organization.SelectedLicense.SignedTransferForm);
                command.Parameters.AddWithValue("@SDK_Password", HandleNullTypes(organization.SelectedLicense.SDKPassword));
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Its safe.")]
        internal void WritePackages(ExtractLicense selectedLicense, IEnumerable<Package> selectedPackages)
        {
            foreach (var package in selectedPackages)
            {
                using (SqlCommand command = new SqlCommand(InsertSelectedPackages, SqlConnection))
                {
                    command.Parameters.AddWithValue("@PackageGuid", package.Guid);
                    command.Parameters.AddWithValue("@LicenseGuid", selectedLicense.Guid);
                    command.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// Deletes a contact from the database.
        /// </summary>
        /// <param name="contact">The contact to remove.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "I'm using Microsoft parameters.")]
        public void DeleteContact(Contact contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }
            using (SqlCommand command = new SqlCommand(DeleteContactSQL, SqlConnection))
            {
                command.Parameters.AddWithValue("@Guid", contact.Guid);
                command.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// Inserts new contacts, and updates existing ones.
        /// </summary>
        /// <param name="contact">The contact to insert/update</param>
        /// <param name="organization">The organization associated with the contact.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "I'm using Microsoft parameters")]
        public void InsertUpdateContact(Contact contact, Organization organization)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }
            using (SqlCommand command = new SqlCommand(InsertUpdateContactSQL, SqlConnection))
            {
                command.Parameters.AddWithValue("@GUID", contact.Guid);
                command.Parameters.AddWithValue("@First_Name", contact.FirstName);
                command.Parameters.AddWithValue("@Last_Name", HandleNullTypes(contact.LastName));
                command.Parameters.AddWithValue("@Email_Address", HandleNullTypes(contact.EmailAddress));
                command.Parameters.AddWithValue("@Phone_Number", HandleNullTypes(contact.PhoneNumber));
                command.Parameters.AddWithValue("@Title", HandleNullTypes(contact.Title));
                command.Parameters.AddWithValue("@Organization_GUID", organization.Guid);
                command.ExecuteNonQuery();
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

        private static object HandleNullTypes(object parameterToCheck)
        {
            if(parameterToCheck == null)
            {
                return DBNull.Value;
            }

            return parameterToCheck;
        }
    }
}
