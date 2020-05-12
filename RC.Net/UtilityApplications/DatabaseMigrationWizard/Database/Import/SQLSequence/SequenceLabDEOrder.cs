using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
	class SequenceLabDEOrder : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##LabDEOrder](
										[OrderNumber] [nvarchar](50) NOT NULL,
										[OrderCode] [nvarchar](30) NOT NULL,
										[PatientMRN] [nvarchar](20) NULL,
										[ReceivedDateTime] [datetime] NOT NULL,
										[OrderStatus] [nchar](1) NOT NULL,
										[ReferenceDateTime] [datetime] NULL,
										[ORMMessage] [xml] NULL,
										[EncounterID] [nvarchar](20) NULL,
										[AccessionNumber] [nvarchar](50) NULL,
                                        [Guid] uniqueidentifier NOT NULL,
										)";

        private readonly string insertSQL = @"
                                    UPDATE
										dbo.LabDEOrder
									SET
										OrderCode = UpdatingLabDEOrder.OrderCode
										, PatientMRN = UpdatingLabDEOrder.PatientMRN
										, ReceivedDateTime = UpdatingLabDEOrder.ReceivedDateTime
										, OrderStatus = UpdatingLabDEOrder.OrderStatus
										, ReferenceDateTime = UpdatingLabDEOrder.ReferenceDateTime
										, ORMMessage = UpdatingLabDEOrder.ORMMessage
										, EncounterID = UpdatingLabDEOrder.EncounterID
										, AccessionNumber = UpdatingLabDEOrder.AccessionNumber
										, OrderNumber = UpdatingLabDEOrder.OrderNumber

									FROM
										##LabDEOrder AS UpdatingLabDEOrder
									WHERE
										LabDEOrder.Guid = UpdatingLabDEOrder.Guid
									;
									INSERT INTO dbo.LabDEOrder(OrderNumber, OrderCode, PatientMRN, ReceivedDateTime, OrderStatus, ReferenceDateTime, ORMMessage, EncounterID, AccessionNumber, Guid)

									SELECT
										OrderNumber
										, OrderCode
										, PatientMRN
										, ReceivedDateTime
										, OrderStatus
										, ReferenceDateTime
										, ORMMessage
										, EncounterID
										, AccessionNumber
										, Guid
									FROM 
										##LabDEOrder
									WHERE
										Guid NOT IN (SELECT Guid FROM dbo.LabDEOrder)
									;";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##LabDEOrder(OrderNumber, OrderCode, PatientMRN, ReceivedDateTime, OrderStatus, ReferenceDateTime, ORMMessage, EncounterID, AccessionNumber, Guid)
                                            VALUES
                                            ";

		private readonly string InsertReportingSQL = @"
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
									SELECT
										'Insert'
	                                    , 'Info'
										, 'LabDEOrder'
										, CONCAT('The LabDEOrder table will have ', COUNT(*), ' rows added to the database')
									FROM
										##LabDEOrder
											LEFT OUTER JOIN dbo.LabDEOrder
												ON dbo.LabDEOrder.Guid = ##LabDEOrder.Guid
									WHERE
										dbo.LabDEOrder.Guid IS NULL";

		private readonly string UpdateReportingSQL = @"
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
									SELECT
										'Update'
										, 'Info'
										, 'LabDEOrder'
										, CONCAT('The LabDEOrder table will have ', COUNT(*) ,' rows updated.')

									FROM
										##LabDEOrder AS UpdatingLabDEOrder
		
											INNER JOIN dbo.LabDEOrder
												ON dbo.LabDEOrder.Guid = UpdatingLabDEOrder.Guid

									WHERE
										ISNULL(UpdatingLabDEOrder.OrderCode, '') <> ISNULL(dbo.LabDEOrder.OrderCode, '')
										OR
										ISNULL(UpdatingLabDEOrder.PatientMRN, '') <> ISNULL(dbo.LabDEOrder.PatientMRN, '')
										OR
										ISNULL(UpdatingLabDEOrder.ReceivedDateTime, '') <> ISNULL(dbo.LabDEOrder.ReceivedDateTime, '')
										OR
										ISNULL(UpdatingLabDEOrder.OrderStatus, '') <> ISNULL(dbo.LabDEOrder.OrderStatus, '')
										OR
										ISNULL(UpdatingLabDEOrder.ReferenceDateTime, '') <> ISNULL(dbo.LabDEOrder.ReferenceDateTime, '')
										OR
										ISNULL(UpdatingLabDEOrder.EncounterID, '') <> ISNULL(dbo.LabDEOrder.EncounterID, '')
										OR
										ISNULL(UpdatingLabDEOrder.AccessionNumber, '') <> ISNULL(dbo.LabDEOrder.AccessionNumber, '')
										OR
										ISNULL(UpdatingLabDEOrder.OrderNumber, '') <> ISNULL(dbo.LabDEOrder.OrderNumber, '')";

		public Priorities Priority => Priorities.Low;

		public string TableName => "LabDEOrder";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

			ImportHelper.PopulateTemporaryTable<LabDEOrder>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.InsertReportingSQL);
			importOptions.ExecuteCommand(this.UpdateReportingSQL);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
