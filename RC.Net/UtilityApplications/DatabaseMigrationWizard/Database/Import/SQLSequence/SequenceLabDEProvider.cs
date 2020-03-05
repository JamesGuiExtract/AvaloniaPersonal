using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
	class SequenceLabDEProvider : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##LabDEProvider](
	                                    [ID] [nvarchar](64) NOT NULL,
	                                    [FirstName] [nvarchar](64) NOT NULL,
	                                    [MiddleName] [nvarchar](64) NULL,
	                                    [LastName] [nvarchar](64) NOT NULL,
	                                    [ProviderType] [nvarchar](32) NULL,
	                                    [Title] [nvarchar](12) NULL,
	                                    [Degree] [nvarchar](12) NULL,
	                                    [Departments] [nvarchar](64) NOT NULL,
	                                    [Specialties] [nvarchar](200) NULL,
	                                    [Phone] [nvarchar](32) NULL,
	                                    [Fax] [nvarchar](32) NULL,
	                                    [Address] [nvarchar](1000) NULL,
	                                    [OtherProviderID] [nvarchar](64) NULL,
	                                    [Inactive] [bit] NULL,
	                                    [MFNMessage] [xml] NULL
                                        )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.LabDEProvider(ID, FirstName, MiddleName, LastName, ProviderType, Title, Degree, Departments, Specialties, Phone, Fax, Address, OtherProviderID, Inactive, MFNMessage)

									SELECT
										ID
										, FirstName
										, MiddleName
										, LastName
										, ProviderType
										, Title
										, Degree
										, Departments
										, Specialties
										, Phone
										, Fax
										, Address
										, OtherProviderID
										, Inactive
										, MFNMessage
									FROM 
										##LabDEProvider
									WHERE
										ID NOT IN (SELECT ID FROM dbo.LabDEProvider)
									;
									UPDATE
										dbo.LabDEProvider
									SET
										FirstName = UpdatingLabDEProvider.FirstName
										, MiddleName = UpdatingLabDEProvider.MiddleName
										, LastName = UpdatingLabDEProvider.LastName
										, ProviderType = UpdatingLabDEProvider.ProviderType
										, Title = UpdatingLabDEProvider.Title
										, Degree = UpdatingLabDEProvider.Degree
										, Departments = UpdatingLabDEProvider.Departments
										, Specialties = UpdatingLabDEProvider.Specialties
										, Phone = UpdatingLabDEProvider.Phone
										, Fax = UpdatingLabDEProvider.Fax
										, Address = UpdatingLabDEProvider.Address
										, OtherProviderID = UpdatingLabDEProvider.OtherProviderID
										, Inactive = UpdatingLabDEProvider.Inactive
										, MFNMessage = UpdatingLabDEProvider.MFNMessage

									FROM
										##LabDEProvider AS UpdatingLabDEProvider
									WHERE
										LabDEProvider.ID = UpdatingLabDEProvider.ID";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##LabDEProvider (ID, FirstName, MiddleName, LastName, ProviderType, Title, Degree, Departments, Specialties, Phone, Fax, Address, OtherProviderID, Inactive, MFNMessage)
                                            VALUES
                                            ";

		public Priorities Priority => Priorities.Medium;

		public string TableName => "LabDEProvider";

		public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);
			
            ImportHelper.PopulateTemporaryTable<LabDEProvider>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);
			
            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
