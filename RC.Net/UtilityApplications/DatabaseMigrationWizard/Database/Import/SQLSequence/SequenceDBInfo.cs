using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceDBInfo : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##DBInfo](
	                                    [Name] [nvarchar](50) NOT NULL,
	                                    [Value] [nvarchar](max) NULL
                                        )";

        private readonly string insertSQL = @"
                                           UPDATE
			                                    dbo.DBInfo
		                                    SET
			                                    Value = UpdatingDBInfo.Value
		                                    FROM
			                                    ##DBInfo AS UpdatingDBInfo
		                                    WHERE
			                                    LOWER(dbo.DBInfo.Name) NOT LIKE '%version%'
			                                    AND
			                                    dbo.DBInfo.Name <> 'DatabaseID'
			                                    AND
			                                    dbo.DBInfo.Name = UpdatingDBInfo.Name
                                    ";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##DBInfo (Name, Value)
                                            VALUES
                                            ";

        private readonly string ReportingSQL = @"
										INSERT INTO
											dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
										SELECT
											'Warning'
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
											dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
										SELECT
											'Info'
											, 'DBInfo'
											, CONCAT('The DBInfo ', ##DBInfo.Name, ' will be added to the database')
										FROM
											##DBInfo
												LEFT OUTER JOIN dbo.DBInfo
													ON dbo.DBInfo.Name = ##DBInfo.Name
										WHERE
											dbo.DBInfo.Name IS NULL";

        public Priorities Priority => Priorities.Low;

        public string TableName => "DBInfo";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<DBInfo>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.ReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
