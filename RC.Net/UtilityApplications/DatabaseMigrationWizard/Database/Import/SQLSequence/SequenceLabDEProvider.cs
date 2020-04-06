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
	                                    [MFNMessage] [xml] NULL,
                                        [Guid] uniqueidentifier NOT NULL,
                                        )";

        private readonly string insertSQL = @"
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
										, ID = UpdatingLabDEProvider.ID
									FROM
										##LabDEProvider AS UpdatingLabDEProvider
									WHERE
										LabDEProvider.Guid = UpdatingLabDEProvider.Guid
									;
									INSERT INTO dbo.LabDEProvider(ID, FirstName, MiddleName, LastName, ProviderType, Title, Degree, Departments, Specialties, Phone, Fax, Address, OtherProviderID, Inactive, MFNMessage, Guid)

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
										, Guid
									FROM 
										##LabDEProvider
									WHERE
										Guid NOT IN (SELECT Guid FROM dbo.LabDEProvider)
									";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##LabDEProvider (ID, FirstName, MiddleName, LastName, ProviderType, Title, Degree, Departments, Specialties, Phone, Fax, Address, OtherProviderID, Inactive, MFNMessage, Guid)
                                            VALUES
                                            ";

		private readonly string ReportingSQL = @"
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
									SELECT
										'Info'
										, 'LabDEProvider'
										, CONCAT('The LabDEProvider table will have ', COUNT(*), ' rows added to the database')
									FROM
										##LabDEProvider
											LEFT OUTER JOIN dbo.LabDEProvider
												ON dbo.LabDEProvider.Guid = ##LabDEProvider.Guid
									WHERE
										dbo.LabDEProvider.Guid IS NULL";

		public Priorities Priority => Priorities.Medium;

		public string TableName => "LabDEProvider";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

			ImportHelper.PopulateTemporaryTable<LabDEProvider>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.ReportingSQL);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
