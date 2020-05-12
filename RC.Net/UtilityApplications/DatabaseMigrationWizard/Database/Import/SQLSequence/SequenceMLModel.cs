using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceMLModel : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##MLModel](
	                                    [Name] [nvarchar](255) NOT NULL,
                                        [Guid] uniqueidentifier NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                    UPDATE
	                                    dbo.MLModel
                                    SET
	                                    Name = UpdatingMLModel.Name
                                    FROM
	                                    ##MLModel AS UpdatingMLModel
                                    WHERE
	                                    dbo.MLModel.Guid = UpdatingMLModel.Guid
                                    ;
                                    INSERT INTO dbo.MLModel(Name, Guid)

                                    SELECT
	                                    Name
	                                    , Guid
                                    FROM 
	                                    ##MLModel
                                    WHERE
	                                    Guid NOT IN (SELECT Guid FROM dbo.MLModel)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##MLModel (Name, Guid)
                                            VALUES
                                            ";

        private readonly string InsertReportingSQL = @"
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
                                    SELECT
	                                    'Insert'
	                                    , 'Warning'
	                                    , 'MLModel'
	                                    , CONCAT('The MLModel ', dbo.MLModel.Name, ' is present in the destination database, but NOT in the importing source.')
                                    FROM
	                                    dbo.MLModel
		                                    LEFT OUTER JOIN ##MLModel
			                                    ON dbo.MLModel.Guid = ##MLModel.GUID
                                    WHERE
	                                    ##MLModel.GUID IS NULL
                                    ;
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
                                    SELECT
	                                    'Insert'
	                                    , 'Info'
	                                    , 'MLModel'
	                                    , CONCAT('The MLModel ', ##MLModel.Name, ' will be added to the database')
                                    FROM
	                                    ##MLModel
		                                    LEFT OUTER JOIN dbo.MLModel
			                                    ON dbo.MLModel.Guid = ##MLModel.GUID
                                    WHERE
	                                    dbo.MLModel.Guid IS NULL";

        private readonly string UpdateReportingSQL = @"
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
                                    SELECT
	                                    'Update'
	                                    , 'Info'
	                                    , 'MLModel'
	                                    , 'The MLModel will be updated'
	                                    , dbo.MLModel.Name
	                                    , UpdatingMLModel.Name
                                    FROM
	                                    ##MLModel AS UpdatingMLModel

			                                    INNER JOIN dbo.MLModel
				                                    ON dbo.MLModel.Guid = UpdatingMLModel.Guid

                                    WHERE
	                                    ISNULL(UpdatingMLModel.Name, '') <> ISNULL(dbo.MLModel.Name, '')";

        public Priorities Priority => Priorities.Low;

        public string TableName => "MLModel";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<MLModel>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.InsertReportingSQL);
            importOptions.ExecuteCommand(this.UpdateReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
