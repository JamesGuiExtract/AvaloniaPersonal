// LabDE_DB_SQL.h - Constants for DB SQL queries that are LabDE Specific

#pragma once

#include <string>

using namespace std;

//--------------------------------------------------------------------------------------------------
// LabDEPatient Table
//--------------------------------------------------------------------------------------------------
static const string gstrLABDE_PATIENT_TABLE = "LabDEPatient";

static const string gstrCREATE_PATIENT_TABLE = 
	"CREATE TABLE [dbo].[LabDEPatient]( "
	" [MRN] NVARCHAR(20) NOT NULL CONSTRAINT [PK_Patient] PRIMARY KEY CLUSTERED, "
	" [FirstName] NVARCHAR(50) NOT NULL, "
	" [MiddleName] NVARCHAR(50) NULL, "
	" [LastName] NVARCHAR(50) NOT NULL, "
	" [Suffix] NVARCHAR(50) NULL, "
	" [DOB] DATETIME NULL,"
	" [Gender] NCHAR(1) NULL)";

static const string gstrCREATE_PATIENT_FIRSTNAME_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_Patient_FirstName] "
	"	ON [LabDEPatient]([FirstName], [LastName], [DOB])";

static const string gstrCREATE_PATIENT_LASTNAME_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_Patient_LastName] "
	"	ON [LabDEPatient]([LastName], [FirstName], [DOB])";

static const string gstrCREATE_PATIENT_DOB_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_Patient_DOB] "
	"	ON [LabDEPatient]([DOB], [LastName], [FirstName])";

//--------------------------------------------------------------------------------------------------
// LabDEOrderStatus Table
//--------------------------------------------------------------------------------------------------
static const string gstrLABDE_ORDER_STATUS_TABLE = "LabDEOrderStatus";

static const string gstrCREATE_ORDER_STATUS_TABLE = 
	"CREATE TABLE [dbo].[LabDEOrderStatus]( "
	" [Code] NCHAR(1) NOT NULL CONSTRAINT [PK_OrderStatus] PRIMARY KEY CLUSTERED, "
	" [Meaning] NVARCHAR(255) NOT NULL)";

// Populate the LabDEOrderStatus table.
static const string gstrPOPULATE_ORDER_STATUSES = 
	"INSERT INTO [dbo].[LabDEOrderStatus] VALUES ('A', "
		"'Available: Has not been cancelled; may or may not have been fulfilled'); "
	"INSERT INTO [dbo].[LabDEOrderStatus] VALUES ('C', 'Canceled');";

//--------------------------------------------------------------------------------------------------
// Order Table
//--------------------------------------------------------------------------------------------------
static const string gstrLABDE_ORDER_TABLE = "LabDEOrder";

static const string gstrCREATE_ORDER_TABLE = 
	"CREATE TABLE [dbo].[LabDEOrder]( "
	" [OrderNumber] NVARCHAR(20) NOT NULL CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED, "
	" [OrderCode] NVARCHAR(30) NOT NULL, "
	" [PatientMRN] NVARCHAR(20) NULL, "
	" [ReceivedDateTime] DATETIME NOT NULL DEFAULT GETDATE(), "
	" [OrderStatus] NCHAR(1) NOT NULL DEFAULT 'A', "
	" [ReferenceDateTime] DATETIME, "
	" [ORMMessage] XML)";

static const string gstrADD_FK_ORDER_PATIENT_MRN = 
	"ALTER TABLE [dbo].[LabDEOrder]  "
	" WITH CHECK ADD CONSTRAINT [FK_Order_Patient] FOREIGN KEY ([PatientMRN]) "
	" REFERENCES [dbo].[LabDEPatient] ([MRN])"
	" ON UPDATE CASCADE "
	" ON DELETE SET NULL";

static const string gstrADD_FK_ORDER_ORDERSTATUS = 
	"ALTER TABLE [dbo].[LabDEOrder]  "
	" WITH CHECK ADD CONSTRAINT [FK_Order_OrderStatus] FOREIGN KEY ([OrderStatus]) "
	" REFERENCES [dbo].[LabDEOrderStatus] ([Code])"
	" ON UPDATE CASCADE "
	" ON DELETE NO ACTION";

static const string gstrCREATE_ORDER_MRN_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_Order_PatientMRN] ON [LabDEOrder]([PatientMRN])";

static const string gstrCREATE_ORDER_ORDERCODERECEIVEDDATETIME_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_Order_OrderCodeReceivedDateTime] "
	"	ON [LabDEOrder]([OrderCode], [ReceivedDateTime])";

//--------------------------------------------------------------------------------------------------
// LabDEOrderFile Table
//--------------------------------------------------------------------------------------------------
static const string gstrLABDE_ORDER_FILE_TABLE = "LabDEOrderFile";

static const string gstrCREATE_ORDER_FILE_TABLE = 
	"CREATE TABLE [dbo].[LabDEOrderFile]( "
	" [OrderNumber] NVARCHAR(20) NOT NULL, "
	" [FileID] INT NOT NULL, "
	" [CollectionDate] DATETIME, "
	" CONSTRAINT [PK_OrderFile] PRIMARY KEY CLUSTERED "
	" ( "
	"	[OrderNumber], "
	"	[FileID]"
	" ))";

static const string gstrADD_FK_ORDERFILE_ORDER = 
	"ALTER TABLE [dbo].[LabDEOrderFile]  "
	" WITH CHECK ADD CONSTRAINT [FK_OrderFile_Order] FOREIGN KEY ([OrderNumber]) "
	" REFERENCES [dbo].[LabDEOrder] ([OrderNumber])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

static const string gstrADD_FK_ORDERFILE_FAMFILE = 
	"ALTER TABLE [dbo].[LabDEOrderFile]  "
	" WITH CHECK ADD CONSTRAINT [FK_OrderFile_FAMFile] FOREIGN KEY ([FileID]) "
	" REFERENCES [dbo].[FAMFile] ([ID])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

static const string gstrCREATE_ORDERFILE_ORDER_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_OrderFile_Order] ON [LabDEOrderFile]([OrderNumber])";

static const string gstrCREATE_ORDERFILE_FAMFILE_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_OrderFile_FAMFile] ON [LabDEOrderFile]([FileID])";

//--------------------------------------------------------------------------------------------------
// Stored procedures
//--------------------------------------------------------------------------------------------------
static const string gstrCREATE_PROCEDURE_ADD_OR_UPDATE_ORDER =
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
	"	SELECT @orderExists = COUNT([OrderNumber]) FROM [LabDEOrder] "
	"		WHERE [OrderNumber] = @OrderNumber "

	"	IF @orderExists = 1 "
	"		BEGIN "
	"			UPDATE [LabDEOrder] SET "
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
	"			INSERT INTO [LabDEOrder] "
	"				([OrderNumber], [OrderCode], [PatientMRN], [ReceivedDateTime], "
	"					[ReferenceDateTime], [OrderStatus], [ORMMessage]) "
	"			VALUES (@OrderNumber, @OrderCode, @PatientMRN, GETDATE(), "
	"				@ReferenceDateTime, @OrderStatus, @ORMMessage) "
	"		END "
	"END";