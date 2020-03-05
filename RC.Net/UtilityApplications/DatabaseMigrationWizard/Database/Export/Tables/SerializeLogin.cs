using Extract.Database;
using Newtonsoft.Json;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", Justification = "Naming violations are a result of acronyms in the database.")]
    public class SerializeLogin : ISerialize
    {
        private readonly string sql =
                            @"
                            SELECT  
	                            *
                            FROM 
	                            [dbo].[Login]";

        public void SerializeTable(DbConnection dbConnection, StreamWriter writer)
        {
            ExportHelper.WriteTableInBulk(this.sql, writer, dbConnection);
        }
    }
}
