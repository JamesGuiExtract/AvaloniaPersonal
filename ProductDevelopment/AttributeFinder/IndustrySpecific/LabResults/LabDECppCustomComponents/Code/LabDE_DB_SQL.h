// LabDE_DB_SQL.h - Constants for DB SQL queries that are LabDE Specific

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

static const string gstrCREATE_PATIENT_TABLE = 
	"CREATE TABLE [dbo].[Patient]( "
	" [MRN] NVARCHAR(20) NOT NULL CONSTRAINT [PK_Patient] PRIMARY KEY CLUSTERED, "
	" [FirstName] NVARCHAR(50) NOT NULL, "
	" [MiddleName] NVARCHAR(50) NULL, "
	" [LastName] NVARCHAR(50) NOT NULL, "
	" [Suffix] NVARCHAR(50) NULL, "
	" [DOB] DATETIME NULL,"
	" [Gender] NCHAR(1) NULL)";

static const string gstrCREATE_PATIENT_FIRSTNAME_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_Patient_FirstName] "
	"	ON [Patient]([FirstName], [LastName])";

static const string gstrCREATE_PATIENT_LASTNAME_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_Patient_LastName] "
	"	ON [Patient]([LastName], [FirstName])";

//--------------------------------------------------------------------------------------------------
// OrderStatus Table
//--------------------------------------------------------------------------------------------------
static const string gstrORDER_STATUS_TABLE = "OrderStatus";

static const string gstrCREATE_ORDER_STATUS_TABLE = 
	"CREATE TABLE [dbo].[OrderStatus]( "
	" [Code] NCHAR(1) NOT NULL CONSTRAINT [PK_OrderStatus] PRIMARY KEY CLUSTERED, "
	" [Meaning] NVARCHAR(255) NOT NULL)";

// Populate the OrderStatus table.
static const string gstrPOPULATE_ORDER_STATUSES = 
	"INSERT INTO [dbo].[OrderStatus] VALUES ('A', "
		"'Available: Has not been cancelled; may or may not have been fulfilled'); "
	"INSERT INTO [dbo].[OrderStatus] VALUES ('C', 'Canceled');";

//--------------------------------------------------------------------------------------------------
// Order Table
//--------------------------------------------------------------------------------------------------
static const string gstrORDER_TABLE = "Order";

// The original schema of the order table; only used as part of a schema update.
static const string gstrCREATE_ORDER_TABLE_V1 = 
	"CREATE TABLE [dbo].[Order]( "
	" [OrderNumber] NVARCHAR(20) NOT NULL CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED, "
	" [OrderCode] NVARCHAR(30) NOT NULL, "
	" [PatientMRN] NVARCHAR(20) NULL, "
	" [RequestDateTime] DATETIME NULL, "
	" [CollectionDateTime] DATETIME NULL, "
	" [OrderStatus] NCHAR(1) NOT NULL)";

static const string gstrCREATE_ORDER_TABLE = 
	"CREATE TABLE [dbo].[Order]( "
	" [OrderNumber] NVARCHAR(20) NOT NULL CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED, "
	" [OrderCode] NVARCHAR(30) NOT NULL, "
	" [PatientMRN] NVARCHAR(20) NULL, "
	" [ReceivedDateTime] DATETIME NOT NULL DEFAULT GETDATE(), "
	" [OrderStatus] NCHAR(1) NOT NULL DEFAULT 'A')";

static const string gstrADD_FK_ORDER_PATIENT_MRN = 
	"ALTER TABLE [dbo].[Order]  "
	" WITH CHECK ADD CONSTRAINT [FK_Order_Patient] FOREIGN KEY ([PatientMRN]) "
	" REFERENCES [dbo].[Patient] ([MRN])"
	" ON UPDATE CASCADE "
	" ON DELETE SET NULL";

static const string gstrADD_FK_ORDER_ORDERSTATUS = 
	"ALTER TABLE [dbo].[Order]  "
	" WITH CHECK ADD CONSTRAINT [FK_Order_OrderStatus] FOREIGN KEY ([OrderStatus]) "
	" REFERENCES [dbo].[OrderStatus] ([Code])"
	" ON UPDATE CASCADE "
	" ON DELETE NO ACTION";

static const string gstrCREATE_ORDER_MRN_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_Order_PatientMRN] ON [Order]([PatientMRN])";

// This index is no longer used since RequestDateTime was renamed to ReceivedDateTime 
static const string gstrCREATE_ORDER_ORDERCODE_INDEX_V1 =
	"CREATE NONCLUSTERED INDEX [IX_Order_OrderCode] "
	"	ON [Order]([OrderCode], [RequestDateTime])";

static const string gstrCREATE_ORDER_ORDERCODERECEIVEDDATETIME_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_Order_OrderCodeReceivedDateTime] "
	"	ON [Order]([OrderCode], [ReceivedDateTime])";

//--------------------------------------------------------------------------------------------------
// OrderFile Table
//--------------------------------------------------------------------------------------------------
static const string gstrORDER_FILE_TABLE = "OrderFile";

static const string gstrCREATE_ORDER_FILE_TABLE = 
	"CREATE TABLE [dbo].[OrderFile]( "
	" [OrderNumber] NVARCHAR(20) NOT NULL, "
	" [FileID] INT NOT NULL, "
	" CONSTRAINT [PK_OrderFile] PRIMARY KEY CLUSTERED "
	" ( "
	"	[OrderNumber], "
	"	[FileID]"
	" ))";

static const string gstrADD_FK_ORDERFILE_ORDER = 
	"ALTER TABLE [dbo].[OrderFile]  "
	" WITH CHECK ADD CONSTRAINT [FK_OrderFile_Order] FOREIGN KEY ([OrderNumber]) "
	" REFERENCES [dbo].[Order] ([OrderNumber])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

static const string gstrADD_FK_ORDERFILE_FAMFILE = 
	"ALTER TABLE [dbo].[OrderFile]  "
	" WITH CHECK ADD CONSTRAINT [FK_OrderFile_FAMFile] FOREIGN KEY ([FileID]) "
	" REFERENCES [dbo].[FAMFile] ([ID])"
	" ON UPDATE CASCADE "
	" ON DELETE CASCADE";

static const string gstrCREATE_ORDERFILE_ORDER_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_OrderFile_Order] ON [OrderFile]([OrderNumber])";

static const string gstrCREATE_ORDERFILE_FAMFILE_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_OrderFile_FAMFile] ON [OrderFile]([FileID])";