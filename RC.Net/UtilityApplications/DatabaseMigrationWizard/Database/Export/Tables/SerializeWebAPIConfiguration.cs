using System.Data.Common;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    public class SerializeWebAPIConfiguration : ISerialize
    {
        private readonly string sql =
                            @"
                            SELECT  
                                [Guid]
                                , [Name]
                                , [Settings]
                            FROM 
                                [dbo].[WebAPIConfiguration]";

        public void SerializeTable(DbConnection dbConnection, TextWriter writer)
        {
            ExportHelper.WriteTableInBulk(this.sql, writer, dbConnection);
        }
    }
}
