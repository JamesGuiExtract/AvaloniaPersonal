using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using reflection")]
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
            [Guid] uniqueidentifier NOT NULL,
            )";

        private readonly string insertSQL = @"
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
                , CSN = UpdatingLabDEEncounter.CSN
            FROM
                ##LabDEEncounter AS UpdatingLabDEEncounter
            WHERE
                LabDEEncounter.Guid = UpdatingLabDEEncounter.Guid
            ;
            INSERT INTO dbo.LabDEEncounter(CSN, PatientMRN, EncounterDateTime, Department, EncounterType, EncounterProvider, DischargeDate, AdmissionDate, ADTMessage, Guid)

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
                , Guid
            FROM 
                ##LabDEEncounter
            WHERE
                Guid NOT IN (SELECT Guid FROM dbo.LabDEEncounter)";

        private readonly string insertTempTableSQL = @"
            INSERT INTO ##LabDEEncounter(CSN, PatientMRN, EncounterDateTime, Department, EncounterType, EncounterProvider, DischargeDate, AdmissionDate, ADTMessage, Guid)
            VALUES
            ";

		private readonly string InsertReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Info'
                , 'LabDEEncounter'
                , CONCAT('The LabDEEncounter table will have ', COUNT(*), ' rows added to the database')
            FROM
                ##LabDEEncounter
                    LEFT OUTER JOIN dbo.LabDEEncounter
                        ON dbo.LabDEEncounter.Guid = ##LabDEEncounter.Guid
            WHERE
                dbo.LabDEEncounter.Guid IS NULL";

		private readonly string UpdateReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Update'
                , 'Info'
                , 'LabDEEncounter'
                , CONCAT('The LabDEEncounter table will have ', COUNT(*) ,' rows updated.')

            FROM
                ##LabDEEncounter AS UpdatingLabDEEncounter
                    
                    INNER JOIN dbo.LabDEEncounter
                        ON dbo.LabDEEncounter.Guid = UpdatingLabDEEncounter.Guid

            WHERE
                ISNULL(UpdatingLabDEEncounter.PatientMRN, '') <> ISNULL(dbo.LabDEEncounter.PatientMRN, '')
                OR
                ISNULL(UpdatingLabDEEncounter.EncounterDateTime, '') <> ISNULL(dbo.LabDEEncounter.EncounterDateTime, '')
                OR
                ISNULL(UpdatingLabDEEncounter.Department, '') <> ISNULL(dbo.LabDEEncounter.Department, '')
                OR
                ISNULL(UpdatingLabDEEncounter.EncounterType, '') <> ISNULL(dbo.LabDEEncounter.EncounterType, '')
                OR
                ISNULL(UpdatingLabDEEncounter.EncounterProvider, '') <> ISNULL(dbo.LabDEEncounter.EncounterProvider, '')
                OR
                ISNULL(UpdatingLabDEEncounter.DischargeDate, '') <> ISNULL(dbo.LabDEEncounter.DischargeDate, '')
                OR
                ISNULL(UpdatingLabDEEncounter.AdmissionDate, '') <> ISNULL(dbo.LabDEEncounter.AdmissionDate, '')
                OR
                ISNULL(UpdatingLabDEEncounter.CSN, '') <> ISNULL(dbo.LabDEEncounter.CSN, '')";

		public Priorities Priority => Priorities.MediumLow;

		public string TableName => "LabDEEncounter";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

			ImportHelper.PopulateTemporaryTable<LabDEEncounter>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.InsertReportingSQL);
			importOptions.ExecuteCommand(this.UpdateReportingSQL);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
