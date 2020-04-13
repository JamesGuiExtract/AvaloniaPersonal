using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
	class SequenceLabDEEncounter : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##LabDEEncounter](
										[CSN] [nvarchar](20) NOT NULL,
										[PatientMRN] [nvarchar](20) NULL,
										[EncounterDateTime] [datetime] NOT NULL,
										[Department] [nvarchar](256) NOT NULL,
										[EncounterType] [nvarchar](256) NOT NULL,
										[EncounterProvider] [nvarchar](256) NOT NULL,
										[DischargeDate] [datetime] NULL,
										[AdmissionDate] [datetime] NULL,
										[ADTMessage] [xml] NULL,
										)";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.LabDEEncounter(CSN, PatientMRN, EncounterDateTime, Department, EncounterType, EncounterProvider, DischargeDate, AdmissionDate, ADTMessage)

									SELECT
										CSN
										, PatientMRN
										, EncounterDateTime
										, Department
										, EncounterType
										, EncounterProvider
										, DischargeDate
										, AdmissionDate
										, ADTMessage
									FROM 
										##LabDEEncounter
									WHERE
										CSN NOT IN (SELECT CSN FROM dbo.LabDEEncounter)
									;
									UPDATE
										dbo.LabDEEncounter
									SET
										PatientMRN = UpdatingLabDEEncounter.PatientMRN
										, EncounterDateTime = UpdatingLabDEEncounter.EncounterDateTime
										, Department = UpdatingLabDEEncounter.Department
										, EncounterType = UpdatingLabDEEncounter.EncounterType
										, EncounterProvider = UpdatingLabDEEncounter.EncounterProvider
										, DischargeDate = UpdatingLabDEEncounter.DischargeDate
										, AdmissionDate = UpdatingLabDEEncounter.AdmissionDate
										, ADTMessage = UpdatingLabDEEncounter.ADTMessage

									FROM
										##LabDEEncounter AS UpdatingLabDEEncounter
									WHERE
										LabDEEncounter.CSN = UpdatingLabDEEncounter.CSN
									;";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##LabDEEncounter(CSN, PatientMRN, EncounterDateTime, Department, EncounterType, EncounterProvider, DischargeDate, AdmissionDate, ADTMessage)
                                            VALUES
                                            ";

		public Priorities Priority => Priorities.MediumLow;

		public string TableName => "LabDEEncounter";

		public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);
			
            ImportHelper.PopulateTemporaryTable<LabDEEncounter>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);
			
			DbCommand dbCommand = dbConnection.CreateCommand();
			dbCommand.CommandTimeout = 0;
			dbCommand.CommandText = this.insertSQL;
            dbCommand.ExecuteNonQuery();
        }
    }
}
