using System.Data.Common;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    public class SerializeDashboard : ISerialize
    {
        private readonly string sql =
                            @"
                            SELECT 
	                            Dashboard.DashboardName
	                            , Dashboard.Definition
	                            , Dashboard.FAMUserID
	                            , Dashboard.LastImportedDate
	                            , Dashboard.UseExtractedData
	                            , Dashboard.ExtractedDataDefinition
                                , Dashboard.GUID AS DashboardGuid
                                , FAMUser.FullUserName
	                            , FAMUser.UserName
                            FROM 
	                            [dbo].[Dashboard]
		                            LEFT OUTER JOIN dbo.FAMUser
			                            ON dbo.FAMUser.ID = dbo.Dashboard.FAMUserID";

        public void SerializeTable(DbConnection dbConnection, TextWriter writer)
        {
            ExportHelper.WriteTableInBulk(this.sql, writer, dbConnection);
        }
    }
}
