// LabDE_DB_SQL_V3.h - Constants for DB SQL queries that are LabDE Specific
// needed to update from version prior to 4

#pragma once

#include <string>

using namespace std;

//--------------------------------------------------------------------------------------------------
// Patient Table
//--------------------------------------------------------------------------------------------------
static const string gstrPATIENT_TABLE = "Patient";

// The original schema of the patient table; only used as part of a schema update.
static const string gstrCREATE_PATIENT_TABLE_V1 = 
	"CREATE TABLE [dbo].[Patient]( "
	" [MRN] NVARCHAR(20) NOT NULL CONSTRAINT [PK_Patient] PRIMARY KEY CLUSTERED, "
	" [FirstName] NVARCHAR(50) NOT NULL, "
	" [MiddleName] NVARCHAR(50) NULL, "
	" [LastName] NVARCHAR(50) NOT NULL, "
	" [Suffix] NVARCHAR(50) NULL, "
	" [DOB] DATETIME NULL)";

static const string gstrCREATE_PATIENT_TABLE_V2 = 
	"CREATE TABLE [dbo].[Patient]( "
	" [MRN] NVARCHAR(20) NOT NULL CONSTRAINT [PK_Patient] PRIMARY KEY CLUSTERED, "
	" [FirstName] NVARCHAR(50) NOT NULL, "
	" [MiddleName] NVARCHAR(50) NULL, "
	" [LastName] NVARCHAR(50) NOT NULL, "
	" [Suffix] NVARCHAR(50) NULL, "
	" [DOB] DATETIME NULL,"
	" [Gender] NCHAR(1) NULL)";

static const string gstrCREATE_PATIENT_FIRSTNAME_INDEX_V1 =
	"CREATE NONCLUSTERED INDEX [IX_Patient_FirstName] "
	"	ON [Patient]([FirstName], [LastName])";

static const string gstrCREATE_PATIENT_LASTNAME_INDEX_V1 =
	"CREATE NONCLUSTERED INDEX [IX_Patient_LastName] "
	"	ON [Patient]([LastName], [FirstName])";

//--------------------------------------------------------------------------------------------------
// OrderStatus Table
//--------------------------------------------------------------------------------------------------
static const string gstrORDER_STATUS_TABLE = "OrderStatus";

static const string gstrCREATE_ORDER_STATUS_TABLE_V1 = 
	"CREATE TABLE [dbo].[OrderStatus]( "
	" [Code] NCHAR(1) NOT NULL CONSTRAINT [PK_OrderStatus] PRIMARY KEY CLUSTERED, "
	" [Meaning] NVARCHAR(255) NOT NULL)";

// Populate the OrderStatus table.
static const string gstrPOPULATE_ORDER_STATUSES_V2 = 
	"INSERT INTO [dbo].[OrderStatus] VALUES ('A', "
		"'Available: Has not been cancelled; may or may not have been fulfilled'); "
	"INSERT INTO [dbo].[OrderStatus] VALUES ('C', 'Canceled');";

//--------------------------------------------------------------------------------------------------
// Order Table
//--------------------------------------------------------------------------------------------------
static const string gstrORDER_TABLE = "Order";

// The original schema of the order table; only used as part of a schema update.
// The RequestDateTime field was modified to be NOT NULL since this was all added new for 

static const string gstrCREATE_ORDER_TABLE_V1 = 
	"CREATE TABLE [dbo].[Order]( "
	" [OrderNumber] NVARCHAR(20) NOT NULL CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED, "
	" [OrderCode] NVARCHAR(30) NOT NULL, "
	" [PatientMRN] NVARCHAR(20) NULL, "
	" [RequestDateTime] DATETIME NOT NULL, " 
	" [CollectionDateTime] DATETIME NULL, "
	" [OrderStatus] NCHAR(1) NOT NULL)";

static const string gstrCREATE_ORDER_TABLE_V3 = 
	"CREATE TABLE [dbo].[Order]( "
	" [OrderNumber] NVARCHAR(20) NOT NULL CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED, "
	" [OrderCode] NVARCHAR(30) NOT NULL, "
	" [PatientMRN] NVARCHAR(20) NULL, "
	" [ReceivedDateTime] DATETIME NOT NULL DEFAULT GETDATE(), "
	" [OrderStatus] NCHAR(1) NOT NULL DEFAULT 'A', "
	" [ReferenceDateTime] DATETIME, "
	" [ORMMessage] XML)";

static const string gstrADD_FK_ORDER_PATIENT_MRN_V1 = 
	"ALTER TABLE [dbo].[Order]  "
	" WITH CHECK ADD CONSTRAINT [FK_Order_Patient] FOREIGN KEY ([PatientMRN]) "
	" REFERENCES [dbo].[Patient] ([MRN])"
	" ON UPDATE CASCADE "
	" ON DELETE SET NULL";

static const string gstrADD_FK_ORDER_ORDERSTATUS_V1 = 
	"ALTER TABLE [dbo].[Order]  "
	" WITH CHECK ADD CONSTRAINT [FK_Order_OrderStatus] FOREIGN KEY ([OrderStatus]) "
	" REFERENCES [dbo].[OrderStatus] ([Code])"
	" ON UPDATE CASCADE "
	" ON DELETE NO ACTION";

static const string gstrCREATE_ORDER_MRN_INDEX_V1 =
	"CREATE NONCLUSTERED INDEX [IX_Order_PatientMRN] ON [Order]([PatientMRN])";

// This index is no longer used since RequestDateTime was renamed to ReceivedDateTime 
static const string gstrCREATE_ORDER_ORDERCODE_INDEX_V1 =
	"CREATE NONCLUSTERED INDEX [IX_Order_OrderCode] "
	"	ON [Order]([OrderCode], [RequestDateTime])";

static const string gstrCREATE_ORDER_ORDERCODERECEIVEDDATETIME_INDEX_V2 =
	"CREATE NONCLUSTERED INDEX [IX_Order_OrderCodeReceivedDateTime] "
	"	ON [Order]([OrderCode], [ReceivedDateTime])";

//--------------------------------------------------------------------------------------------------
// OrderFile Table
//--------------------------------------------------------------------------------------------------
static const string gstrORDER_FILE_TABLE = "OrderFile";

static const string gstrCREATE_ORDER_FILE_TABLE_V1 = 
	"CREATE TABLE [dbo].[OrderFile]( "
	" [OrderNumber] NVARCHAR(20) NOT NULL, "
	" [FileID] INT NOT NULL, "
	" CONSTRAINT [PK_OrderFile] PRIMARY KEY CLUSTERED "
	" ( "
	"	[OrderNumber], "
	"	[FileID]"
	" ))";

static const string gstrADD_FK_ORDERFILE_ORDER_V1 = 
	"ALTER TABLE [dbo].[OrderFile]  "
	" WITH CHECK ADD CONSTRAINT [FK_OrderFile_Order] FOREIGN KEY ([OrderNumber]) "
	" REFERENCES [dbo].[Order] ([OrderNumber])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

static const string gstrADD_FK_ORDERFILE_FAMFILE_V1 = 
	"ALTER TABLE [dbo].[OrderFile]  "
	" WITH CHECK ADD CONSTRAINT [FK_OrderFile_FAMFile] FOREIGN KEY ([FileID]) "
	" REFERENCES [dbo].[FAMFile] ([ID])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

static const string gstrCREATE_ORDERFILE_ORDER_INDEX_V1 =
	"CREATE NONCLUSTERED INDEX [IX_OrderFile_Order] ON [OrderFile]([OrderNumber])";

static const string gstrCREATE_ORDERFILE_FAMFILE_INDEX_V1 =
	"CREATE NONCLUSTERED INDEX [IX_OrderFile_FAMFile] ON [OrderFile]([FileID])";

//--------------------------------------------------------------------------------------------------
// Stored procedures
//--------------------------------------------------------------------------------------------------
static const string gstrCREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_V3 =
	"CREATE PROCEDURE [AddOrUpdateLabDEOrder] "
	"	@OrderNumber NVARCHAR(20), "
	"	@OrderCode NVARCHAR(30), "
	"	@PatientMRN NVARCHAR(20), "
	"	@ReferenceDateTime DATETIME, "
	"	@OrderStatus NVARCHAR(1), "
	"	@ORMMessage XML "
	"AS "
	"BEGIN "
	// SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	"	SET NOCOUNT ON; "
	// Cull empty XML nodes from the ORM XML.
	"	SET @ORMMessage.modify('delete (//*[not(text())][not(*//text())])') "

	"	DECLARE @orderExists INT "
	"	SELECT @orderExists = COUNT([OrderNumber]) FROM [Order] "
	"		WHERE [OrderNumber] = @OrderNumber "

	"	IF @orderExists = 1 "
	"		BEGIN "
	"			UPDATE [Order] SET "
	"					[OrderCode] = @OrderCode, "
	"					[PatientMRN] = @PatientMRN, "
	"					[ReceivedDateTime] = GETDATE(), "
	"					[ReferenceDateTime] = @ReferenceDateTime, "
	"					[OrderStatus] = @OrderStatus, "
	"					[ORMMessage] = @ORMMessage "
	"				WHERE [OrderNumber] = @OrderNumber "
	"		END "
	"	ELSE  "
	"		BEGIN "
	"			INSERT INTO [Order] "
	"				([OrderNumber], [OrderCode], [PatientMRN], [ReceivedDateTime], "
	"					[ReferenceDateTime], [OrderStatus], [ORMMessage]) "
	"			VALUES (@OrderNumber, @OrderCode, @PatientMRN, GETDATE(), "
	"				@ReferenceDateTime, @OrderStatus, @ORMMessage) "
	"		END "
	"END";