using DatabaseMigrationWizard.Pages.Utility;
using Extract;
using Extract.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DatabaseMigrationWizard.Database.Output
{
    public class ExportHelper
    {
        /// <summary>
        /// The number of records to buffer before writing. The smaller this number, the slower the speed of
        /// WriteTablesInBatches, but the less likely you are to get memory issues. So this number should be as big as you can get it
        /// Without getting a memory issue.
        /// </summary>
        private readonly int batchSize = 50000;

        /// <summary>
        /// Writes a table in one go (loads entire table in memory then writes it).
        /// This is the fastest method, but if your table is too big, this will throw out of memory exceptions!
        /// </summary>
        /// <param name="sql">The sql to get the data from the table</param>
        /// <param name="writer">The streamwriter to write the file to</param>
        /// <param name="dbConnection">The database to obtain the data from</param>
        public static void WriteTableInBulk(string sql, TextWriter writer, DbConnection dbConnection)
        {
            try
            {
                var dataTable = DBMethods.ExecuteDBQuery(dbConnection, sql);
                try
                {
                    writer.WriteLine(JsonConvert.SerializeObject(dataTable, Formatting.Indented));
                }
                catch(Exception e)
                {
                    ExtractException extractException = e.AsExtract("ELI49704");
                    throw extractException;
                }
            }
            catch(Exception e)
            {
                ExtractException extractException = e.AsExtract("ELI49703");
                extractException.AddDebugData("SQL", sql);
                extractException.AddDebugData("ConnectionString", dbConnection.ConnectionString);
                throw extractException;
            }
        }

        /// <summary>
        /// This will write out a table in large batches. Provided that the batch size above
        /// is small enough, you will not run into any memory errors.
        /// Note: This takes advantage of optimistic order for tables. If you are adding data to tables as this
        /// is running, it is possible you will be missing data after this finishes.
        /// </summary>
        /// <param name="orderBySql">A sql statement with an order by</param>
        /// <param name="writer">The streamwriter to write the file to</param>
        /// <param name="dbConnection">The database to obtain the data from</param>
        /// <param name="rowCountSql">Select the number of rows in the table, must label the count as "COUNT". See a calling example.</param>
        public void WriteTableInBatches(string orderBySql, TextWriter writer, DbConnection dbConnection, string rowCountSql )
        {
            try
            {
                int tableRowCount = int.Parse(DBMethods.ExecuteDBQuery(dbConnection, rowCountSql).Rows[0]["COUNT"].ToString(), CultureInfo.InvariantCulture);

                for (int i = 0; i < tableRowCount; i += batchSize)
                {
                    string sql = orderBySql + $@" OFFSET {i.ToString(CultureInfo.InvariantCulture)} ROWS FETCH NEXT {batchSize.ToString(CultureInfo.InvariantCulture)} ROWS ONLY";
                    try
                    {
                        var dataTable = DBMethods.ExecuteDBQuery(dbConnection, sql);

                        writer.WriteLine(JsonConvert.SerializeObject(dataTable, Formatting.Indented));
                    }
                    catch(Exception e)
                    {
                        ExtractException extractException = e.AsExtract("ELI49706");
                        extractException.AddDebugData("SQL", sql);
                        extractException.AddDebugData("ConnectionString", dbConnection.ConnectionString);
                        throw extractException;
                    }
                }
            }
            catch(Exception e)
            {
                throw e.AsExtract("ELI49705");
            }
        }

        public static void Export(ExportOptions exportOptions, IProgress<string> progress)
        {
            try
            {
                IEnumerable<ISerialize> instances = Universal.GetClassesThatImplementInterface<ISerialize>();

                if (!exportOptions.IncludeLabDETables)
                {
                    instances = instances.Where(m => !m.ToString().ToUpper(CultureInfo.InvariantCulture).Contains("LABDE"));
                }

                foreach (ISerialize instance in instances)
                {
                    SerializeTableAndWriteToFile(instance, exportOptions, progress);
                }
            }
            catch(Exception e)
            {
                ExtractException.Display("ELI49698", e);
            }
        }

        /// <summary>
        /// Calls serialize table on the SerializeInterface it was passed.
        /// </summary>
        /// <param name="instance"></param>
        private static void SerializeTableAndWriteToFile(ISerialize instance, ExportOptions exportOptions, IProgress<string> progress)
        {
            var TableName = instance.ToString().Substring(instance.ToString().LastIndexOf("Serialize", StringComparison.OrdinalIgnoreCase)).Replace("Serialize", string.Empty);
            progress.Report(TableName);

            using (var sqlConnection = new SqlConnection($@"Server={exportOptions.ConnectionInformation.DatabaseServer};Database={exportOptions.ConnectionInformation.DatabaseName};Integrated Security=SSPI"))
            using (StreamWriter writer = File.CreateText($@"{exportOptions.ExportPath}\{TableName}.json"))
            {
                sqlConnection.Open();
                instance.SerializeTable(sqlConnection, writer);
            }

            progress.Report(TableName);
        }
    }
}
