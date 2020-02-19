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
										)";

        private readonly string insertSQL = @"
                                    ALTER TABLE dbo.LabDEPatient NOCHECK CONSTRAINT ALL;
									INSERT INTO dbo.LabDEPatient(MRN, FirstName, MiddleName, LastName, Suffix, DOB, Gender, MergedInto, CurrentMRN)

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
									FROM 
										##LabDEPatient
									WHERE
										MRN NOT IN (SELECT MRN FROM dbo.LabDEPatient)
									;
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

									FROM
										##LabDEPatient AS UpdatingLabDEPatient
									WHERE
										LabDEPatient.MRN = UpdatingLabDEPatient.MRN
									;
									ALTER TABLE dbo.LabDEPatient WITH CHECK CHECK CONSTRAINT ALL;";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##LabDEPatient (MRN, FirstName, MiddleName, LastName, Suffix, DOB, Gender, MergedInto, CurrentMRN)
                                            VALUES
                                            ";

		public Priorities Priority => Priorities.Medium;

		public string TableName => "LabDEPatient";

		public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);
			
            ImportHelper.PopulateTemporaryTable<LabDEPatient>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);
			
            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
