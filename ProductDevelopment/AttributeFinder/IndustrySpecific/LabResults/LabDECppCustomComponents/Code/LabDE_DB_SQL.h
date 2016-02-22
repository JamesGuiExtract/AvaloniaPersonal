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
	" [DOB] DATETIME NULL, "
	" [Gender] NCHAR(1) NULL, "
	" [MergedInto] NVARCHAR(20) NULL, "
	" [CurrentMRN] NVARCHAR(20) NOT NULL)";

static const string gstrADD_FK_PATIENT_MERGEDINTO= 
	"ALTER TABLE [dbo].[LabDEPatient]  "
	" WITH CHECK ADD CONSTRAINT [FK_PatientMergedInto] FOREIGN KEY ([MergedInto]) "
	" REFERENCES [dbo].[LabDEPatient] ([MRN])"
	" ON UPDATE NO ACTION "
	" ON DELETE NO ACTION";

static const string gstrADD_FK_PATIENT_CURRENTMRN= 
	"ALTER TABLE [dbo].[LabDEPatient]  "
	" WITH CHECK ADD CONSTRAINT [FK_PatientCurrentMRN] FOREIGN KEY ([CurrentMRN]) "
	" REFERENCES [dbo].[LabDEPatient] ([MRN])"
	" ON UPDATE NO ACTION "
	" ON DELETE NO ACTION";

static const string gstrCREATE_PATIENT_FIRSTNAME_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_Patient_FirstName] "
	"	ON [LabDEPatient]([FirstName], [LastName], [DOB])";

static const string gstrCREATE_PATIENT_LASTNAME_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_Patient_LastName] "
	"	ON [LabDEPatient]([LastName], [FirstName], [DOB])";

static const string gstrCREATE_PATIENT_DOB_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_Patient_DOB] "
	"	ON [LabDEPatient]([DOB], [LastName], [FirstName])";

static const string gstrCREATE_PATIENT_CURRENT_MRN_INDEX =
	"CREATE NONCLUSTERED INDEX [IX_Patient_CurrentMRN] "
	"	ON [LabDEPatient]([CurrentMRN])";

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
	"CREATE PROCEDURE [dbo].[LabDEAddOrUpdateOrder] "
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

// Will merge or un-merge records in the LabDEPatient table.
// The result of the procedure will be NULL for unqualified success or a string warning in the case
// that the operation succeeded under unexpected circumstances.
// To un-merge, specify @TargetMRN as NULL.
// If @TargetMRN is non-NULL and does not exist, it will be created using the data passed in, though
// a warning will be returned.
// If @UpdateInfo is 1, the data passed in will be used to update the record referenced by
// @TargetMRN (or @SourceMRN if @TargetMRN is NULL)
static const string gstrCREATE_PROCEDURE_MERGE_PATIENTS =
	"CREATE PROCEDURE [dbo].[LabDEMergePatients] "
	"	@SourceMRN NVARCHAR(20), "
	"	@TargetMRN NVARCHAR(20), "
	"	@UpdateInfo BIT, "
	"	@FirstName NVARCHAR(50), "
	"	@MiddleName NVARCHAR(50), "
	"	@LastName NVARCHAR(50), "
	"	@Suffix NVARCHAR(50), "
	"	@DOB DATETIME, "
	"	@Gender NCHAR(1) "
	"AS "
	"BEGIN "

	// SET NOCOUNT ON added to improve performance and prevent extra result sets from interfering
	// with SELECT statements.
	"	SET NOCOUNT ON; "
	"	BEGIN TRAN Tran1; "

	"	DECLARE @warning NVARCHAR(1000) "
	"	DECLARE @currentMRN NVARCHAR(20) "
	"	DECLARE @originalValues TABLE (MergedInto NVARCHAR(20), CurrentMRN NVARCHAR(20)) "
	"	DECLARE @orignalMergedInto NVARCHAR(20) "
	"	DECLARE @orignalCurrentMRN NVARCHAR(20) "
	"	DECLARE @targetExists INT "

	"	IF (SELECT COUNT([MRN]) FROM [LabDEPatient] WHERE [MRN] = @SourceMRN) = 0 "
	"	BEGIN "
	"		SET @warning = 'Patient does not exist: ' + @SourceMRN "
	"		RAISERROR (@warning, 18, 1) "
	"	END "

	// Target record will be @SourceMRN if un-merging buy setting @TargetMRN as NULL
	"	SELECT @targetExists = COUNT([MRN]) FROM [LabDEPatient] "
	"		WHERE [MRN] = COALESCE(@TargetMRN, @SourceMRN) "
	
	// If the merge target does not exist
	"	IF @targetExists = 0  "
	"	BEGIN "
	"		SET @warning = 'Warning: Patient merge target ' + @TargetMRN + "
	"			' did not exist; created record.' "
	"		INSERT INTO [LabDEPatient]  "
	"			([MRN], [FirstName], [MiddleName], [LastName], [Suffix], [DOB], [Gender], [CurrentMRN]) "
	"			VALUES (@TargetMRN, @FirstName, @MiddleName, @LastName, @Suffix, @DOB, @Gender, @TargetMRN) "
	"	END "
	// else if the target record's info should be updated.
	"	ELSE IF @UpdateInfo = 1 "
	"	BEGIN "
	"		UPDATE [LabDEPatient] SET "
	"				[FirstName] = @FirstName, "
	"				[MiddleName] = @MiddleName, "
	"				[LastName] = @LastName, "
	"				[Suffix] = @Suffix, "
	"				[DOB] = @DOB, "
	"				[Gender] = @Gender  "
	// Target record will be @SourceMRN if un-merging by setting @TargetMRN as NULL
	"		WHERE [MRN] = COALESCE(@TargetMRN, @SourceMRN) "
	"	END "

	"	UPDATE [LabDEPatient] SET "
	"			[MergedInto] = @TargetMRN "
	"		OUTPUT DELETED.[MergedInto], DELETED.[CurrentMRN] INTO @originalValues "
	"		WHERE [MRN] = @SourceMRN "

	// Determine the new CurrentMRN that should be used for the source record and all other records
	// that have been merged into it.
	"	SELECT @currentMRN = COALESCE(MAX([CurrentMRN]), @SourceMRN) "
	"		FROM [LabDEPatient]  "
	"		WHERE [MRN] = @TargetMRN "

	// Warn if merging a record that was already merged.
	"	SELECT @orignalMergedInto = [MergedInto], @orignalCurrentMRN = [CurrentMRN] "
	"		FROM @originalValues "
	"	IF (@orignalMergedInto IS NOT NULL) AND (@TargetMRN IS NOT NULL) "
	"	BEGIN "
	"		SET @warning = 'Warning: Patient ' + @SourceMRN + "
	"			' was already merged into ' + @orignalCurrentMRN + '.' "
	"		IF (@orignalCurrentMRN <> @currentMRN) "
	"		BEGIN "
	"			SET @warning = @warning + ' Now merged to ' + @currentMRN + ' instead.'; "
	"		END "
	"	END "
	// Warn if un-merging a record that wasn't merged.
	"	ELSE IF (@orignalMergedInto IS NULL) AND (@TargetMRN IS NULL) "
	"	BEGIN "
	"		SET @warning = 'Warning: Cannot un-merge; patient ' + @SourceMRN + "
	"			' was not merged.' "
	"	END "
	// Warn if merging into a record that is itself already merged.
	"	ELSE IF (@TargetMRN IS NOT NULL) AND @currentMRN <> @TargetMRN "
	"	BEGIN "
	"		SET @warning = 'Warning: Merged patient ' + @SourceMRN + "
	"			' into ' + @TargetMRN + ' which has already been merged to ' "
	"			+ @currentMRN; "
	"	END "

	// Identify all patient records merged into this record by recursively tracing MergedInto. Do
	// not shortcut by using CurrentMRN to look up the records to support the case that the source
	// or target records have already been merged into other records. In these cases CurrentMRN
	// cannot be used to lookup all records that need to be updated. These are unexpected
	// scenarios, but it is better to be able to handle them (albeit, with a warning).
	"	;WITH [SourcePatients] AS "
	"	( "
	"		SELECT [MRN] "
	"		FROM	[LabDEPatient] "
	"		WHERE	[MRN] = @SourceMRN "
	"		UNION ALL "
	"		SELECT	recurs.[MRN] "
	"		FROM	[LabDEPatient] recurs INNER JOIN "
	"			[SourcePatients] ON recurs.[MergedInto] = [SourcePatients].[MRN] "
	"	) "
	// Update CurrentMRN for all SourcePatients records
	"	UPDATE [LabDEPatient] "
	"		SET [CurrentMRN] = @currentMRN "
	"	WHERE [MRN] IN "
	"	( "
	"		SELECT [MRN] FROM [SourcePatients] "
	"	) "

	"	COMMIT TRAN Tran1; "

	"	SELECT @warning "
	"	RETURN "

	"END";