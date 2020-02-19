using Extract.Database;
using Newtonsoft.Json;
using System.Data.Common;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    public class SerializeLabDEOrder : ISerialize
    {
        public void SerializeTable(DbConnection dbConnection, StreamWriter writer)
        {
            new ExportHelper().WriteTableInBatches(GetBatchSQL(), writer, dbConnection, "SELECT COUNT(*) AS COUNT FROM dbo.LabDEOrder");
        }

        private static string GetBatchSQL()
        {
            return
                @"
                SELECT
	                [OrderNumber]
                    , [OrderCode]
                    , [PatientMRN]
                    , [ReceivedDateTime]
                    , [OrderStatus]
                    , [ReferenceDateTime]
                    , [ORMMessage]
                    , [EncounterID]
                    , [AccessionNumber]
                FROM 
	                [dbo].[LabDEOrder]
                ORDER BY
	                [OrderNumber]
                     , [OrderCode]
                     , [PatientMRN]
                     , [ReceivedDateTime]
                     , [OrderStatus]
                     , [ReferenceDateTime]
                     , [EncounterID]
                     , [AccessionNumber]";
        }
    }
}
