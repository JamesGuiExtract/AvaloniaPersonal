using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_FILEPROCESSINGLib;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    class SequenceDBInfo : ISequence
    {
        internal static IReadOnlyCollection<string> ExcludedSettings => Array.AsReadOnly(excludeSettings);

		static readonly string[] excludeSettings = new []
		{
            "AlternateComponentDataDir",
            "AttributeCollectionSchemaVersion",
            "AutoRevertNotifyEmailList",
            "AzureClientId",
            "AzureInstance",
            "AzureTenant",
            "DatabaseID",
            "DataEntrySchemaVersion",
            "EmailPossibleInvalidSenderAddress",
            "EmailPossibleInvalidServer",
            "EmailSenderAddress",
            "EmailSenderName",
            "ETLRestart",
            "FAMDBSchemaVersion",
            "IDShieldSchemaVersion",
            "LabDESchemaVersion",
            "LastDBInfoChange",
            "RootPathForDashboardExtractedData",
            "SendAlertsToExtract",
            "SendAlertsToSpecified",
            "SpecifiedAlertRecipients",
		};

        static readonly string excludeSettingsSqlList = MakeSqlList(excludeSettings);

        static readonly string warnIfDestinationIsEmptySettingsSqlList =
            MakeSqlList(excludeSettings.Where(setting => setting != "ETLRestart"));


        private readonly string createTempTableSQL = @"
            CREATE TABLE [dbo].[##DBInfo](
            [Name] [nvarchar](50) NOT NULL,
            [Value] [nvarchar](max) NULL
            )";

        private readonly string insertSQL = $@"
            UPDATE
                dbo.DBInfo
            SET
                Value = UpdatingDBInfo.Value
            FROM
                ##DBInfo AS UpdatingDBInfo
            WHERE
                dbo.DBInfo.Name = UpdatingDBInfo.Name
                AND UpdatingDBInfo.Name NOT IN {excludeSettingsSqlList}";

        private readonly string insertTempTableSQL = @"
            INSERT INTO ##DBInfo (Name, Value)
            VALUES
            ";

        private readonly string reportingSQL = $@"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Warning'
                , 'DBInfo'
                , CONCAT('The DBInfo ', dbo.DBInfo.Name, ' is present in the destination database, but NOT in the importing source.')
            FROM
                dbo.DBInfo
                    LEFT OUTER JOIN ##DBInfo
                        ON dbo.DBInfo.Name = ##DBInfo.Name
            WHERE
                ##DBInfo.Name IS NULL
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Info'
                , 'DBInfo'
                , CONCAT('The DBInfo ', ##DBInfo.Name, ' will be added to the database')
            FROM
                ##DBInfo
                    LEFT OUTER JOIN dbo.DBInfo
                        ON dbo.DBInfo.Name = ##DBInfo.Name
            WHERE
                dbo.DBInfo.Name IS NULL
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Warning'
                , 'DBInfo'
                , 'The ' + dbo.DBInfo.Name + ' will have its value updated'
                , dbo.DBInfo.Value
                , ##DBInfo.Value
            FROM
                dbo.DBInfo
                    JOIN ##DBInfo
                        ON dbo.DBInfo.Name = ##DBInfo.Name
            WHERE
                dbo.DBInfo.Name NOT IN {excludeSettingsSqlList}
                AND
                dbo.DBInfo.Value != ##DBInfo.Value
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'N/A'
                , 'Warning'
                , 'DBInfo'
                , CONCAT('Uninitialized setting! ', dbo.DBInfo.Name, ' will not be set in the destination')
            FROM
                ##DBInfo
                    LEFT OUTER JOIN dbo.DBInfo
                        ON dbo.DBInfo.Name = ##DBInfo.Name
            WHERE
                ##DBInfo.Name IN {warnIfDestinationIsEmptySettingsSqlList}
                AND
                NULLIF(##DBInfo.Value, '') IS NOT NULL
                AND
                NULLIF(dbo.DBInfo.Value, '') IS NULL
            ";

        public Priorities Priority => Priorities.Low;

        public string TableName => "DBInfo";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.createTempTableSQL);

            ImportHelper.PopulateTemporaryTable<DBInfo>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.reportingSQL);
			importOptions.ExecuteCommand(GetSchemaCheckQuery(importOptions));

            importOptions.ExecuteCommand(this.insertSQL);
        }

		private static int GetDBSchemaVersion(ImportOptions importOptions)
		{
			var fileProcessingDb = new FileProcessingDB()
			{
				DatabaseServer = importOptions.ConnectionInformation.DatabaseServer,
				DatabaseName = importOptions.ConnectionInformation.DatabaseName
			};

			return fileProcessingDb.CurrentDBSchemaVersion;
		}

		private static string GetSchemaCheckQuery(ImportOptions importOptions)
		{
			return FormattableString.Invariant($@"
                INSERT INTO
                    dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
                SELECT
                    'N/A'
                    , 'Warning'
                    , 'DBInfo'
                    , 'Version mismatch! ApplicationVersion: {GetDBSchemaVersion(importOptions)} Database Version: ' + dbo.DBInfo.Value

                FROM
                    dbo.DBInfo
                WHERE
                    dbo.DBInfo.Name = 'FAMDBSchemaVersion'
                    AND
                    dbo.DBInfo.Value != { GetDBSchemaVersion(importOptions)}
                ");
		}

        private static string MakeSqlList(IEnumerable<string> values)
        {
            return "('" + string.Join("','", values) + "')";
        }
    }
}
