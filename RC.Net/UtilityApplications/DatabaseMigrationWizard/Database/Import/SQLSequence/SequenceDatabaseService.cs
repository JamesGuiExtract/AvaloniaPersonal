using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceDatabaseService : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##DatabaseService](
										[Description] [nvarchar](256) NOT NULL,
										[Settings] [nvarchar](max) NOT NULL,
										[Enabled] [bit] NOT NULL,
                                        [Guid] uniqueidentifier NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                            UPDATE
	                                            dbo.DatabaseService
                                            SET
	                                            Description = UpdatingDatabaseService.Description
	                                            , Enabled = UpdatingDatabaseService.Enabled
	                                            , Settings = UpdatingDatabaseService.Settings
                                            FROM
	                                            ##DatabaseService AS UpdatingDatabaseService
                                            WHERE
	                                            UpdatingDatabaseService.Guid = dbo.DatabaseService.Guid
                                            ;
                                            INSERT INTO dbo.DatabaseService(Description, Settings, Enabled, Guid)

                                            SELECT
	                                            UpdatingDatabaseService.Description
	                                            , UpdatingDatabaseService.Settings
	                                            , UpdatingDatabaseService.Enabled
	                                            , UpdatingDatabaseService.Guid
                                            FROM 
	                                            ##DatabaseService AS UpdatingDatabaseService
                                            WHERE
	                                            UpdatingDatabaseService.Guid NOT IN (SELECT Guid FROM dbo.DatabaseService)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##DatabaseService (Description, Settings, Enabled, Guid)
                                            VALUES
                                            ";

        private readonly string ReportingSQL = @"
                                            INSERT INTO
	                                            dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                            SELECT
	                                            'Warning'
	                                            , 'DatabaseService'
	                                            , CONCAT('The DatabaseService ', dbo.DatabaseService.Description, ' is present in the destination database, but NOT in the importing source.')
                                            FROM
	                                            dbo.DatabaseService
		                                            LEFT OUTER JOIN ##DatabaseService
			                                            ON dbo.DatabaseService.Guid = ##DatabaseService.GUID
                                            WHERE
	                                            ##DatabaseService.GUID IS NULL
                                            ;
                                            INSERT INTO
	                                            dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                            SELECT
	                                            'Warning'
	                                            , 'DatabaseService'
	                                            , CONCAT('The service ', ##DatabaseService.Description, ' will be added to the database. Please be sure the configuration/schedule is appropriate for the new enviornment.')
                                            FROM
	                                            ##DatabaseService
		                                            LEFT OUTER JOIN dbo.DatabaseService
			                                            ON dbo.DatabaseService.Guid = ##DatabaseService.GUID
                                            WHERE
	                                            dbo.DatabaseService.Guid IS NULL";
        private readonly string CheckForJsonSQL = @"
                                            SELECT
	                                            ##DatabaseService.Description
	                                            , ##DatabaseService.Settings
                                            FROM
	                                            ##DatabaseService
		                                            LEFT OUTER JOIN dbo.DatabaseService
			                                            ON dbo.DatabaseService.Guid = ##DatabaseService.GUID
                                            WHERE
	                                            dbo.DatabaseService.Guid IS NULL";

        private static readonly Regex ABSOLUTE_PATH_REGEX = new Regex(@"^[a-zA-Z]:\\ | ^\\\\[-\w.\x20]+\\",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        public Priorities Priority => Priorities.Medium;

        public string TableName => "DatabaseService";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<DatabaseService>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.ReportingSQL);
            this.ReportFilePaths(importOptions);

            importOptions.ExecuteCommand(this.insertSQL);
        }

        private void ReportFilePaths(ImportOptions importOptions)
        {
            string sql = @"INSERT INTO dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message) ";
            var servicesWithFilePaths = new Collection<(string service, string path)>();
            using (DataTable dataTable = new DataTable())
            using (DbCommand dbCommand = importOptions.SqlConnection.CreateCommand())
            {
                dbCommand.Transaction = importOptions.Transaction;
                dbCommand.CommandText = CheckForJsonSQL;
                dataTable.Load(dbCommand.ExecuteReader());
                foreach (DataRow row in dataTable.Rows)
                {
                    string description = row["Description"].ToString();
                    dynamic settings = row["Settings"];
                    if (settings != null)
                    {
                        var json = JObject.Parse(settings);
                        foreach (var absolutePath in RecursiveJsonSearch(json, ABSOLUTE_PATH_REGEX))
                        {
                            servicesWithFilePaths.Add((description, absolutePath));
                        }
                    }
                }
            }

            foreach (var (service, path) in servicesWithFilePaths)
            {
                sql += $" SELECT 'Warning', 'DatabaseService', 'The {service} service uses the absolute path {path}, please double check it is applicable to this environment' UNION ALL";
            }
            if (servicesWithFilePaths.Count > 0)
            {
                importOptions.ExecuteCommand(sql.Remove(sql.LastIndexOf("UNION ALL")));
            }
        }

        private static IEnumerable<string> RecursiveJsonSearch(JToken token, Regex searchRegex)
        {
            foreach (var item in token.Children())
            {
                if (item.HasValues)
                {
                    foreach (var value in RecursiveJsonSearch(item, searchRegex))
                    {
                        yield return value;
                    }
                }
                else
                {
                    string JsonValueToCheck = (string)(item as JValue) ?? string.Empty;
                    if (searchRegex.Match(JsonValueToCheck).Success)
                    {
                        yield return JsonValueToCheck;
                    }
                }
            }
        }
    }
}
