using Extract.Database;
using Newtonsoft.Json;
using System.Data.Common;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    public class SerializeLabDEProvider : ISerialize
    {
        public void SerializeTable(DbConnection dbConnection, TextWriter writer)
        {
            new ExportHelper().WriteTableInBatches(GetBatchSQL(), writer, dbConnection, "SELECT COUNT(*) AS COUNT FROM [dbo].[LabDEProvider]");
        }

        private static string GetBatchSQL()
        {
            return $@"
                    SELECT 
	                    [ID]
                        , [FirstName]
                        , [MiddleName]
                        , [LastName]
                        , [ProviderType]
                        , [Title]
                        , [Degree]
                        , [Departments]
                        , [Specialties]
                        , [Phone]
                        , [Fax]
                        , [Address]
                        , [OtherProviderID]
                        , [Inactive]
                        , [MFNMessage]
                        , [Guid]
                    FROM 
	                    [dbo].[LabDEProvider]
                    ORDER BY
	                    [ID]
                        , [FirstName]
                        , [MiddleName]
                        , [LastName]
                        , [ProviderType]
                        , [Title]
                        , [Degree]
                        , [Departments]
                        , [Specialties]
                        , [Phone]
                        , [Fax]
                        , [Address]
                        , [OtherProviderID]
                        , [Inactive]
                        , [Guid]";
        }
    }
}
