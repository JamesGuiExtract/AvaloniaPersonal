using Extract.Database;
using Newtonsoft.Json;
using System.Data.Common;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    public class SerializeLabDEEncounter : ISerialize
    {
        public void SerializeTable(DbConnection dbConnection, StreamWriter writer)
        {
            new ExportHelper().WriteTableInBatches(GetBatchSQL(), writer, dbConnection, "SELECT COUNT(*) AS COUNT FROM [dbo].[LabDEEncounter]");
        }

        private static string GetBatchSQL()
        {
            return $@"
                    SELECT
	                    [CSN]
                        , [PatientMRN]
                        , [EncounterDateTime]
                        , [Department]
                        , [EncounterType]
                        , [EncounterProvider]
                        , [DischargeDate]
                        , [AdmissionDate]
                        , [ADTMessage]
                    FROM 
	                    [Extract_ANONOMYZE].[dbo].[LabDEEncounter]
                    ORDER BY
	                    [CSN]
                        , [PatientMRN]
                        , [EncounterDateTime]
                        , [Department]
                        , [EncounterType]
                        , [EncounterProvider]
                        , [DischargeDate]
                        , [AdmissionDate]";
        }
    }
}
