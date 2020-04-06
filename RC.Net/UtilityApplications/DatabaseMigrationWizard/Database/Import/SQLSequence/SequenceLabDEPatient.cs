using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
	class SequenceLabDEPatient : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##LabDEPatient](
										[MRN] [nvarchar](20) NOT NULL,
										[FirstName] [nvarchar](50) NOT NULL,
										[MiddleName] [nvarchar](50) NULL,
										[LastName] [nvarchar](50) NOT NULL,
										[Suffix] [nvarchar](50) NULL,
										[DOB] [datetime] NULL,
										[Gender] [nchar](1) NULL,
										[MergedInto] [nvarchar](20) NULL,
										[CurrentMRN] [nvarchar](20) NOT NULL,
                                        [Guid] uniqueidentifier NOT NULL,
										)";

        private readonly string insertSQL = @"
                                    ALTER TABLE dbo.LabDEPatient NOCHECK CONSTRAINT ALL;
									UPDATE
										dbo.LabDEPatient
									SET
										FirstName = UpdatingLabDEPatient.FirstName
										, MiddleName = UpdatingLabDEPatient.MiddleName
										, LastName = UpdatingLabDEPatient.LastName
										, Suffix = UpdatingLabDEPatient.Suffix
										, DOB = UpdatingLabDEPatient.DOB
										, Gender = UpdatingLabDEPatient.Gender
										, MergedInto = UpdatingLabDEPatient.MergedInto
										, CurrentMRN = UpdatingLabDEPatient.CurrentMRN
										, MRN = UpdatingLabDEPatient.MRN
									FROM
										##LabDEPatient AS UpdatingLabDEPatient
									WHERE
										LabDEPatient.Guid = UpdatingLabDEPatient.Guid
									;
									INSERT INTO dbo.LabDEPatient(MRN, FirstName, MiddleName, LastName, Suffix, DOB, Gender, MergedInto, CurrentMRN, Guid)

									SELECT
										MRN
										, FirstName
										, MiddleName
										, LastName
										, Suffix
										, DOB
										, Gender
										, MergedInto
										, CurrentMRN
										, Guid
									FROM 
										##LabDEPatient
									WHERE
										Guid NOT IN (SELECT Guid FROM dbo.LabDEPatient)
									;

									ALTER TABLE dbo.LabDEPatient WITH CHECK CHECK CONSTRAINT ALL;";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##LabDEPatient (MRN, FirstName, MiddleName, LastName, Suffix, DOB, Gender, MergedInto, CurrentMRN, Guid)
                                            VALUES
                                            ";

		private readonly string ReportingSQL = @"
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
									SELECT
										'Info'
										, 'LabDEPatient'
										, CONCAT('The LabDEPatient table will have ', COUNT(*), ' rows added to the database')
									FROM
										##LabDEPatient
											LEFT OUTER JOIN dbo.LabDEPatient
												ON dbo.LabDEPatient.Guid = ##LabDEPatient.Guid
									WHERE
										dbo.LabDEPatient.Guid IS NULL";

		public Priorities Priority => Priorities.Medium;

		public string TableName => "LabDEPatient";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

			ImportHelper.PopulateTemporaryTable<LabDEPatient>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.ReportingSQL);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
