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

		private readonly string ReportingSQL = @"
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
									SELECT
										'Info'
										, 'LabDEOrder'
										, CONCAT('The LabDEOrder table will have ', COUNT(*), ' rows added to the database')
									FROM
										##LabDEOrder
											LEFT OUTER JOIN dbo.LabDEOrder
												ON dbo.LabDEOrder.Guid = ##LabDEOrder.Guid
									WHERE
										dbo.LabDEOrder.Guid IS NULL";

		public Priorities Priority => Priorities.Low;

		public string TableName => "LabDEOrder";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

			ImportHelper.PopulateTemporaryTable<LabDEOrder>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.ReportingSQL);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
