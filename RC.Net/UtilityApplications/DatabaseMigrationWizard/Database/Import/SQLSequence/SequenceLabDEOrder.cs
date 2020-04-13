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
										)";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.LabDEOrder(OrderNumber, OrderCode, PatientMRN, ReceivedDateTime, OrderStatus, ReferenceDateTime, ORMMessage, EncounterID, AccessionNumber)

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
									FROM 
										##LabDEOrder
									WHERE
										OrderNumber NOT IN (SELECT OrderNumber FROM dbo.LabDEOrder)
									;
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

									FROM
										##LabDEOrder AS UpdatingLabDEOrder
									WHERE
										LabDEOrder.OrderNumber = UpdatingLabDEOrder.OrderNumber
									;";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##LabDEOrder(OrderNumber, OrderCode, PatientMRN, ReceivedDateTime, OrderStatus, ReferenceDateTime, ORMMessage, EncounterID, AccessionNumber)
                                            VALUES
                                            ";

		public Priorities Priority => Priorities.Low;

		public string TableName => "LabDEOrder";

		public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);
			
            ImportHelper.PopulateTemporaryTable<LabDEOrder>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);
			
			DbCommand dbCommand = dbConnection.CreateCommand();
			dbCommand.CommandTimeout = 0;
			dbCommand.CommandText = this.insertSQL;
            dbCommand.ExecuteNonQuery();
        }
    }
}
