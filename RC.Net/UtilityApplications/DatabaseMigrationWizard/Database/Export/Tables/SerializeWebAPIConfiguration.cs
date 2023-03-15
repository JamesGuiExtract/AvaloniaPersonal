using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "API")]
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
