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

static const string gstrCREATE_LABDE_PROVIDER_TABLE =
"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'LabDEProvider') "
"BEGIN "
"CREATE TABLE [dbo].[LabDEProvider]( "
"[ID][nvarchar](64) NOT NULL, "
"[FirstName][nvarchar](64) NOT NULL, "
"[MiddleName][nvarchar](64) NULL, "
"[LastName][nvarchar](64) NOT NULL, "
"[ProviderType][nvarchar](32) NULL, "
"[Title][nvarchar](12) NULL, "
"[Degree][nvarchar](12) NULL, "
"[Departments][nvarchar](64) NOT NULL, "
"[Specialties][nvarchar](200) NULL, "
"[Phone][nvarchar](32) NULL, "
"[Fax][nvarchar](32) NULL, "
"[Address][nvarchar](1000) NULL, "
"[OtherProviderID][nvarchar](64) NULL, "
"[Inactive][bit] NULL, "
"[MFNMessage][xml] NULL) "
"END ";

static const string gstrCREATE_LABDE_PROVIDER_INDEX_FIRSTLAST =
"IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_FirstLast' AND object_id = OBJECT_ID('dbo.LabDEProvider')) "
"BEGIN "
"CREATE NONCLUSTERED INDEX [IX_FirstLast] ON [dbo].[LabDEProvider] "
"([LastName] ASC, "
" [FirstName] ASC "
")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON[PRIMARY] "
"END ";

static const string gstrCREATE_LABDE_PROVIDER_INDEX_FIRST =
"IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_FirstName' AND object_id = OBJECT_ID('dbo.LabDEProvider')) "
"BEGIN "
"CREATE NONCLUSTERED INDEX [IX_FirstName] ON [dbo].[LabDEProvider]( "
"[FirstName] ASC "
")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON[PRIMARY] "
"END ";

static const string gstrCREATE_LABDE_PROVIDER_INDEX_LAST =
"IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_LastName' AND object_id = OBJECT_ID('dbo.LabDEProvider')) "
"BEGIN "
"CREATE NONCLUSTERED INDEX [IX_LastName] ON [dbo].[LabDEProvider]( "
"[LastName] ASC "
")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON[PRIMARY] "
"END ";

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
// LabDEEncounter Table
//--------------------------------------------------------------------------------------------------
static const string gstrLABDE_ENCOUNTER_TABLE = "LabDEEncounter";

static const string gstrCREATE_ENCOUNTER_TABLE =
    "CREATE TABLE [dbo].[LabDEEncounter] ("
    " [CSN] NVARCHAR(20) NOT NULL CONSTRAINT[PK_Encounter] PRIMARY KEY CLUSTERED, "
    " [PatientMRN] NVARCHAR(20) NULL, "
    " [EncounterDateTime] DATETIME NOT NULL DEFAULT GETDATE(), "
    " [Department] NVARCHAR(256) NOT NULL, "
    " [EncounterType] NVARCHAR(256) NOT NULL, "
    " [EncounterProvider] NVARCHAR(256) NOT NULL, "
	" [DischargeDate] DATETIME NULL, "
	" [AdmissionDate] DATETIME NULL, "
    " ADTMessage XML)";

static const string gstrADD_FK_ENCOUNTER_PATIENT =
"ALTER TABLE [dbo].[LabDEEncounter] "
" WITH CHECK ADD CONSTRAINT[FK_Encounter_Patient] FOREIGN KEY([PatientMRN]) "
" REFERENCES [dbo].[LabDEPatient]([MRN]) "
" ON UPDATE CASCADE "
" ON DELETE CASCADE";

static const string gstrCREATE_ENCOUNTER_PATIENT_MRN_INDEX =
"CREATE NONCLUSTERED INDEX [IX_LabDEEncounterPatientMRN] "
"ON [dbo].[LabDEEncounter]([PatientMRN]) "
"ON[PRIMARY] ";

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
    " [OrderNumber] NVARCHAR(50) NOT NULL CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED, "
    " [OrderCode] NVARCHAR(30) NOT NULL, "
    " [PatientMRN] NVARCHAR(20) NULL, "
    " [ReceivedDateTime] DATETIME NOT NULL DEFAULT GETDATE(), "
    " [OrderStatus] NCHAR(1) NOT NULL DEFAULT 'A', "
    " [ReferenceDateTime] DATETIME, "
    " [ORMMessage] XML, "
    " [EncounterID] NVARCHAR(20) NULL, "
	" [AccessionNumber] NVARCHAR(50) NULL )";

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

static const string gstrCREATE_ENCOUNTERID_COLUMN =
"ALTER TABLE [dbo].[LabDEOrder] ADD [EncounterID] NVARCHAR(20) NULL";

// Add a FK to the Encounter table.
static const string gstrADD_FK_ORDER_TO_ENCOUNTER =
"ALTER TABLE [dbo].[LabDEOrder] "
" WITH CHECK ADD CONSTRAINT[FK_Order_Encounter] FOREIGN KEY([EncounterID]) "
" REFERENCES [dbo].[LabDEEncounter] ([CSN]) "
" ON UPDATE NO ACTION "
" ON DELETE NO ACTION";

//--------------------------------------------------------------------------------------------------
// LabDEOrderFile Table
//--------------------------------------------------------------------------------------------------
static const string gstrLABDE_ORDER_FILE_TABLE = "LabDEOrderFile";

static const string gstrCREATE_ORDER_FILE_TABLE = 
    "CREATE TABLE [dbo].[LabDEOrderFile]( "
    " [OrderNumber] NVARCHAR(50) NOT NULL, "
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
// LabDEEncounterFile Table
//--------------------------------------------------------------------------------------------------
static const string gstrLABDE_ENCOUNTER_FILE_TABLE = "LabDEEncounterFile";

static const string gstrCREATE_ENCOUNTER_FILE_TABLE =
"CREATE TABLE [dbo].[LabDEEncounterFile]( "
" [EncounterID] NVARCHAR(20) NOT NULL, "
" [FileID] INT NOT NULL, "
" [DateTime] DATETIME, "
" CONSTRAINT [PK_EncounterFile] PRIMARY KEY CLUSTERED "
" ( "
"	[EncounterID], "
"	[FileID]"
" ))";

static const string gstrADD_FK_ENCOUNTERFILE_ENCOUNTER =
"ALTER TABLE [dbo].[LabDEEncounterFile]  "
" WITH CHECK ADD CONSTRAINT [FK_EncounterFile_EncounterID] FOREIGN KEY ([EncounterID]) "
" REFERENCES [dbo].[LabDEEncounter] ([CSN])"
" ON UPDATE CASCADE "
" ON DELETE CASCADE";

static const string gstrADD_FK_ENCOUNTERFILE_FAMFILE =
"ALTER TABLE [dbo].[LabDEEncounterFile]  "
" WITH CHECK ADD CONSTRAINT [FK_EncounterFile_FAMFile] FOREIGN KEY ([FileID]) "
" REFERENCES [dbo].[FAMFile] ([ID])"
" ON UPDATE CASCADE "
" ON DELETE CASCADE";

static const string gstrCREATE_ENCOUNTERFILE_ENCOUNTER_INDEX =
"CREATE NONCLUSTERED INDEX [IX_EncounterFile_Encounter] ON [LabDEEncounterFile]([EncounterID])";

static const string gstrCREATE_ENCOUNTERFILE_FAMFILE_INDEX =
"CREATE NONCLUSTERED INDEX [IX_EncounterFile_FAMFile] ON [LabDEEncounterFile]([FileID])";


//--------------------------------------------------------------------------------------------------
// LabDEPatientFile Table
//--------------------------------------------------------------------------------------------------
static const string gstrLABDE_PATIENT_FILE_TABLE = "LabDEPatientFile";

static const string gstrCREATE_PATIENT_FILE_TABLE =
"CREATE TABLE [dbo].[LabDEPatientFile]( "
" [PatientMRN] NVARCHAR(20) NOT NULL, "
" [FileID] INT NOT NULL, "
" [DateTime] DATETIME, "
" CONSTRAINT [PK_PatientFile] PRIMARY KEY CLUSTERED "
" ( "
"	[PatientMRN], "
"	[FileID]"
" ))";

static const string gstrADD_FK_PATIENTFILE_PATIENT =
"ALTER TABLE [dbo].[LabDEPatientFile] "
" WITH CHECK ADD CONSTRAINT [FK_PatientFile_PatientMRN] FOREIGN KEY ([PatientMRN]) "
" REFERENCES [dbo].[LabDEPatient]([MRN]) "
" ON UPDATE CASCADE "
" ON DELETE CASCADE";

static const string gstrADD_FK_PATIENTFILE_FAMFILE =
"ALTER TABLE [dbo].[LabDEPatientFile]  "
" WITH CHECK ADD CONSTRAINT [FK_PatientFile_FAMFile] FOREIGN KEY ([FileID]) "
" REFERENCES [dbo].[FAMFile] ([ID])"
" ON UPDATE CASCADE "
" ON DELETE CASCADE";

static const string gstrCREATE_PATIENTFILE_PATIENT_INDEX =
"CREATE NONCLUSTERED INDEX [IX_PatientFile_Encounter] ON [LabDEPatientFile]([PatientMRN])";

static const string gstrCREATE_PATIENTFILE_FAMFILE_INDEX =
"CREATE NONCLUSTERED INDEX [IX_EncounterFile_FAMFile] ON [LabDEPatientFile]([FileID])";

//--------------------------------------------------------------------------------------------------
// Stored procedures
//--------------------------------------------------------------------------------------------------
static const string gstrCREATE_PROCEDURE_ADD_OR_UPDATE_ORDER =
    "CREATE PROCEDURE [dbo].[LabDEAddOrUpdateOrder] "
    "	@OrderNumber NVARCHAR(50), "
    "	@OrderCode NVARCHAR(30), "
    "	@PatientMRN NVARCHAR(20), "
    "	@ReferenceDateTime DATETIME, "
    "	@OrderStatus NVARCHAR(1), "
    "	@ORMMessage XML "
    "AS "
    "BEGIN "
    // SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
    "	SET NOCOUNT ON; "
    "	EXEC [dbo].LabDEAddOrUpdateOrderWithAccession "
	"						@OrderNumber = @OrderNumber, "
	"						@OrderCode = @OrderCode, "
	"						@PatientMRN = @PatientMRN, "
	"						@ReferenceDateTime = @ReferenceDateTime, "
	"						@OrderStatus = @OrderStatus, "
	"						@ORMMessage = @ORMMessage, "
	"						@AccessionNumber = NULL "
    "END";

static const string gstrCREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_WITH_ACCESSION =
"CREATE PROCEDURE [dbo].[LabDEAddOrUpdateOrderWithAccession] "
"	@OrderNumber NVARCHAR(50), "
"	@OrderCode NVARCHAR(30), "
"	@PatientMRN NVARCHAR(20), "
"	@ReferenceDateTime DATETIME, "
"	@OrderStatus NVARCHAR(1), "
"	@ORMMessage XML, "
"   @AccessionNumber NVARCHAR(50) = NULL "
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
"					[ORMMessage] = @ORMMessage, "
"					[AccessionNumber] = @AccessionNumber "
"				WHERE [OrderNumber] = @OrderNumber "
"		END "
"	ELSE  "
"		BEGIN "
"			INSERT INTO [LabDEOrder] "
"				([OrderNumber], [OrderCode], [PatientMRN], [ReceivedDateTime], "
"					[ReferenceDateTime], [OrderStatus], [ORMMessage], [AccessionNumber]) "
"			VALUES (@OrderNumber, @OrderCode, @PatientMRN, GETDATE(), "
"				@ReferenceDateTime, @OrderStatus, @ORMMessage, @AccessionNumber) "
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
// Warnings returned:
// - Warning: Patient [MRN], subject of merge, does not exist.
// - Warning: Patient [MRN], target of un-merge, did not exist; created record.
// - Warning: Patient merge target [MRN] did not exist; created record.
// - Warning: Patient [MRN] was already merged into [MRN].
// - Warning: Patient [MRN] was already merged into [MRN]. Now merged to [MRN] instead.
// - Warning: Merged patient [MRN] into [MRN] which was already merged into [MRN]
// - Warning: Cannot un-merge; patient [MRN] was not merged.
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
    "	@Gender NCHAR(1), "
    "	@Warning NVARCHAR(1000) = NULL OUTPUT "
    "AS "
    "BEGIN "

    // SET NOCOUNT ON added to improve performance and prevent extra result sets from interfering
    // with SELECT statements.
    "	SET NOCOUNT ON; "
    "	BEGIN TRAN Tran1; "

    "	DECLARE @currentMRN NVARCHAR(20) "
    "	DECLARE @originalValues TABLE (MergedInto NVARCHAR(20), CurrentMRN NVARCHAR(20)) "
    "	DECLARE @originalMergedInto NVARCHAR(20) "
    "	DECLARE @originalCurrentMRN NVARCHAR(20) "
    "	DECLARE @targetExists INT "

    "	IF (SELECT COUNT([MRN]) FROM [LabDEPatient] WHERE [MRN] = @SourceMRN) = 0 "
    "	BEGIN "
    "		IF (@TargetMRN IS NULL) "
    "		BEGIN "
    "			SET @warning = 'Warning: Patient ' + @SourceMRN + "
    "				', target of un-merge, did not exist; created record.' "
    "		END "
    "		ELSE "
    "		BEGIN "
    "			SET @warning = 'Warning: Patient ' + @SourceMRN + "
    "				', subject of merge, does not exist.' "
    "		END "
    "	END "

    // Target record will be @SourceMRN if un-merging buy setting @TargetMRN as NULL
    "	SELECT @targetExists = COUNT([MRN]) FROM [LabDEPatient] "
    "		WHERE [MRN] = COALESCE(@TargetMRN, @SourceMRN) "
    
    // If the merge target does not exist
    "	IF @targetExists = 0  "
    "	BEGIN "
    "		DECLARE @newMRN NVARCHAR(20) "
    "		SET @newMRN = COALESCE(@TargetMRN, @SourceMRN) "

    "		IF (@TargetMRN IS NOT NULL) " // @Warning will be assigned above for un-merge
    "		BEGIN "
    "			SET @warning = 'Warning: Patient merge target ' + @newMRN + "
    "				' did not exist; created record.' "
    "		END "
    "		INSERT INTO [LabDEPatient]  "
    "			([MRN], [FirstName], [MiddleName], [LastName], [Suffix], [DOB], [Gender], [CurrentMRN]) "
    "			VALUES (@newMRN, @FirstName, @MiddleName, @LastName, @Suffix, @DOB, @Gender, @newMRN) "
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
    "	SELECT @originalMergedInto = [MergedInto], @originalCurrentMRN = [CurrentMRN] "
    "		FROM @originalValues "
    "	IF (@originalMergedInto IS NOT NULL) AND (@TargetMRN IS NOT NULL) "
    "	BEGIN "
    "		SET @Warning = 'Warning: Patient ' + @SourceMRN + "
    "			' was already merged into ' + @originalCurrentMRN + '.' "
    "		IF (@originalCurrentMRN <> @currentMRN) "
    "		BEGIN "
    "			SET @Warning = @Warning + ' Now merged to ' + @currentMRN + ' instead.'; "
    "		END "
    "	END "
    // Warn if un-merging a record that wasn't merged.
    "	ELSE IF (@originalMergedInto IS NULL) AND (@TargetMRN IS NULL) AND @warning IS NULL"
    "	BEGIN "
    "		SET @Warning = 'Warning: Cannot un-merge; patient ' + @SourceMRN + "
    "			' was not merged.' "
    "	END "
    // Warn if merging into a record that is itself already merged.
    "	ELSE IF (@TargetMRN IS NOT NULL) AND @currentMRN <> @TargetMRN "
    "	BEGIN "
    "		SET @Warning = 'Warning: Merged patient ' + @SourceMRN + "
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
    "	RETURN "

    "END";


    static const string gstr_CREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_WITH_ENCOUNTER_AND_IP_DATES =
    "CREATE PROCEDURE [dbo].[LabDEAddOrUpdateOrderWithEncounterAndIPDates] @OrderNumber   NVARCHAR(50),         \r\n"
    "                                                        @OrderCode         NVARCHAR(30),                   \r\n"
    "                                                        @PatientMRN        NVARCHAR(20),                   \r\n"
    "                                                        @ReferenceDateTime DATETIME,                       \r\n"
    "                                                        @OrderStatus       NVARCHAR(1),                    \r\n"
    "                                                        @EncounterID		NVARCHAR(20),                   \r\n"
    "                                                        @EncounterDateTime DATETIME,                       \r\n"
    "                                                        @Department        NVARCHAR(256),                  \r\n"
    "                                                        @EncounterType     NVARCHAR(256),                  \r\n"
    "                                                        @EncounterProvider NVARCHAR(256),                  \r\n"
    "                                                        @ADTMessage        XML,                            \r\n"
	"                                                        @AdmissionDate     DATETIME = NULL,                \r\n"
	"                                                        @DischargeDate     DATETIME = NULL,		        \r\n"
	"														 @AccessionNumber NVARCHAR(50) = NULL				\r\n"
	"	  AS                                                                                                     \r\n"
    "     BEGIN                                                                                                 \r\n"
    "         -- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.     \r\n"
    "         SET NOCOUNT ON;                                                                                   \r\n"
    "         -- Cull empty XML nodes from the ADT XML.                                                         \r\n"
    "         SET @ADTMessage.modify('delete (//*[not(text())][not(*//text())])');                              \r\n"
    "         DECLARE @encounterExists INT;                                                                     \r\n"
    "         SELECT @encounterExists = COUNT(CSN)                                                              \r\n"
    "         FROM LabDEEncounter                                                                               \r\n"
    "         WHERE CSN = @EncounterID;                                                                         \r\n"
    "         IF @encounterExists = 1                                                                           \r\n"
    "             BEGIN                                                                                         \r\n"
    "                 UPDATE dbo.LabDEEncounter                                                                 \r\n"
    "                   SET                                                                                     \r\n"
    "                       CSN = @EncounterID,                                                                 \r\n"
    "                       PatientMRN = @PatientMRN,                                                           \r\n"
    "                       EncounterDateTime = @EncounterDateTime,                                             \r\n"
    "                       Department = @Department,                                                           \r\n"
    "                       EncounterType = @EncounterType,                                                     \r\n"
    "                       EncounterProvider = @EncounterProvider,                                             \r\n"
    "                       ADTMessage = @ADTMessage,                                                           \r\n"
	"                       AdmissionDate = COALESCE(@AdmissionDate, AdmissionDate),                            \r\n"
	"                       DischargeDate = COALESCE(@DischargeDate, DischargeDate)								\r\n"
	"                 WHERE CSN = @EncounterID;                                                                 \r\n"
    "             END;                                                                                          \r\n"
    "         ELSE                                                                                              \r\n"
    "             BEGIN                                                                                         \r\n"
    "                 IF @EncounterID IS NOT NULL                                                               \r\n"
    "                     BEGIN                                                                                 \r\n"
    "                         INSERT INTO dbo.LabDEEncounter                                                    \r\n"
    "                         (CSN,                                                                             \r\n"
    "                          PatientMRN,                                                                      \r\n"
    "                          EncounterDateTime,                                                               \r\n"
    "                          Department,                                                                      \r\n"
    "                          EncounterType,                                                                   \r\n"
    "                          EncounterProvider,                                                               \r\n"
    "                          ADTMessage,                                                                      \r\n"
	"                          AdmissionDate,                                                                   \r\n"
	"                          DischargeDate                                                                    \r\n"
    "                         )                                                                                 \r\n"
    "                         VALUES                                                                            \r\n"
    "                         (@EncounterID,                                                                    \r\n"
    "                          @PatientMRN,                                                                     \r\n"
    "                          @EncounterDateTime,                                                              \r\n"
    "                          @Department,                                                                     \r\n"
    "                          @EncounterType,                                                                  \r\n"
    "                          @EncounterProvider,                                                              \r\n"
    "                          @ADTMessage,                                                                     \r\n"
	"                          @AdmissionDate,                                                                  \r\n"
	"                          @DischargeDate                                                                   \r\n"
	"                         );                                                                                \r\n"
    "                     END;                                                                                  \r\n"
    "             END;                                                                                          \r\n"
    "                                                                                                           \r\n"
    "         DECLARE @orderExists INT;                                                                         \r\n"
    "         SELECT @orderExists = COUNT(OrderNumber)                                                          \r\n"
    "         FROM LabDEOrder                                                                                   \r\n"
    "         WHERE OrderNumber = @OrderNumber                                                                  \r\n"
    "         IF @orderExists = 1                                                                               \r\n"
    "             BEGIN                                                                                         \r\n"
    "                 UPDATE LabDEOrder                                                                         \r\n"
    "                   SET                                                                                     \r\n"
    "                       OrderCode = @OrderCode,                                                             \r\n"
    "                       PatientMRN = @PatientMRN,                                                           \r\n"
    "                       ReceivedDateTime = GETDATE(),                                                       \r\n"
    "                       ReferenceDateTime = @ReferenceDateTime,                                             \r\n"
    "                       OrderStatus = @OrderStatus,                                                         \r\n"
    "                       EncounterID = @EncounterID,                                                         \r\n"
	"						AccessionNumber = @AccessionNumber,													\r\n"
	"                       ORMMessage = @ADTMessage                                                            \r\n"
    "                 WHERE OrderNumber = @OrderNumber;                                                         \r\n"
    "             END;                                                                                          \r\n"
    "         ELSE                                                                                              \r\n"
    "             BEGIN                                                                                         \r\n"
    "                 INSERT INTO LabDEOrder                                                                    \r\n"
    "                 (OrderNumber,                                                                             \r\n"
    "                  OrderCode,                                                                               \r\n"
    "                  PatientMRN,                                                                              \r\n"
    "                  ReceivedDateTime,                                                                        \r\n"
    "                  ReferenceDateTime,                                                                       \r\n"
    "                  OrderStatus,                                                                             \r\n"
    "                  EncounterID,                                                                             \r\n"
	"				   AccessionNumber,																			\r\n"	
    "                  ORMMessage                                                                               \r\n"
    "                 )                                                                                         \r\n"
    "                 VALUES                                                                                    \r\n"
    "                 (@OrderNumber,                                                                            \r\n"
    "                  @OrderCode,                                                                              \r\n"
    "                  @PatientMRN,                                                                             \r\n"
    "                  GETDATE(),                                                                               \r\n"
    "                  @ReferenceDateTime,                                                                      \r\n"
    "                  @OrderStatus,                                                                            \r\n"
    "                  @EncounterID,                                                                            \r\n"
	"				   @AccessionNumber,																		\r\n"
    "                  @ADTMessage                                                                              \r\n"
    "                 );                                                                                        \r\n"
    "             END;                                                                                          \r\n"
    "     END;                                                                                                  \r\n";


	static const string gstr_CREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_WITH_ENCOUNTER =
		"CREATE PROCEDURE [dbo].[LabDEAddOrUpdateOrderWithEncounter] @OrderNumber   NVARCHAR(50),                   \r\n"
		"                                                        @OrderCode         NVARCHAR(30),                   \r\n"
		"                                                        @PatientMRN        NVARCHAR(20),                   \r\n"
		"                                                        @ReferenceDateTime DATETIME,                       \r\n"
		"                                                        @OrderStatus       NVARCHAR(1),                    \r\n"
		"                                                        @EncounterID		NVARCHAR(20),                   \r\n"
		"                                                        @EncounterDateTime DATETIME,                       \r\n"
		"                                                        @Department        NVARCHAR(256),                  \r\n"
		"                                                        @EncounterType     NVARCHAR(256),                  \r\n"
		"                                                        @EncounterProvider NVARCHAR(256),                  \r\n"
		"                                                        @ADTMessage        XML                             \r\n"
		"AS                                                                                                         \r\n"
		"     BEGIN                                                                                                 \r\n"
		"         -- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.     \r\n"
		"         SET NOCOUNT ON;                                                                                   \r\n"
		"         EXEC  [dbo].[LabDEAddOrUpdateOrderWithEncounterAndIPDates]										\r\n"
		"			@OrderNumber = @OrderNumber ,																	\r\n"
		"			@OrderCode = @OrderCode,																		\r\n"
		"			@PatientMRN = @PatientMRN,																		\r\n"
		"			@ReferenceDateTime = @ReferenceDateTime,														\r\n"
		"			@OrderStatus = @OrderStatus,																	\r\n"
		"			@EncounterID = @EncounterID,																	\r\n"
		"			@EncounterDateTime = @EncounterDateTime,														\r\n"
		"			@Department =  @Department,																		\r\n"
		"			@EncounterType = @EncounterType,																\r\n"
		"			@EncounterProvider = @EncounterProvider,														\r\n"
		"			@ADTMessage = @ADTMessage,																		\r\n"
		"			@AdmissionDate = NULL,																			\r\n"
		"			@DischargeDate = NULL,																			\r\n"
		"			@AccessionNumber = NULL																			\r\n"
		"	 END																									\r\n";


    static const string gstrCREATE_PROCEDURE_ADD_OR_MODIFY_ENCOUNTER_AND_IP_DATES =
        "CREATE PROCEDURE [dbo].[LabDEAddOrUpdateEncounterAndIPDates] @PatientMRN        NVARCHAR(20),             \r\n"
        "                                                        @EncounterID		NVARCHAR(20),                   \r\n"
        "                                                        @EncounterDateTime DATETIME,                       \r\n"
        "                                                        @Department        NVARCHAR(256),                  \r\n"
        "                                                        @EncounterType     NVARCHAR(256),                  \r\n"
        "                                                        @EncounterProvider NVARCHAR(256),                  \r\n"
        "                                                        @ADTMessage        XML,                            \r\n"
		"                                                        @AdmissionDate     DATETIME = NULL,                \r\n"
		"                                                        @DischargeDate     DATETIME = NULL                 \r\n"
		"AS                                                                                                         \r\n"
        "     BEGIN                                                                                                 \r\n"
        "         -- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.     \r\n"
        "         SET NOCOUNT ON;                                                                                   \r\n"
        "         -- Cull empty XML nodes from the ADT XML.                                                         \r\n"
        "         SET @ADTMessage.modify('delete (//*[not(text())][not(*//text())])');                              \r\n"
        "         DECLARE @encounterExists INT;                                                                     \r\n"
        "         SELECT @encounterExists = COUNT(CSN)                                                              \r\n"
        "         FROM LabDEEncounter                                                                               \r\n"
        "         WHERE CSN = @EncounterID;                                                                         \r\n"
        "         IF @encounterExists = 1                                                                           \r\n"
        "             BEGIN                                                                                         \r\n"
        "                 UPDATE dbo.LabDEEncounter                                                                 \r\n"
        "                   SET                                                                                     \r\n"
        "                       CSN = @EncounterID,                                                                 \r\n"
        "                       PatientMRN = @PatientMRN,                                                           \r\n"
        "                       EncounterDateTime = @EncounterDateTime,                                             \r\n"
        "                       Department = @Department,                                                           \r\n"
        "                       EncounterType = @EncounterType,                                                     \r\n"
        "                       EncounterProvider = @EncounterProvider,                                             \r\n"
		"                       ADTMessage = @ADTMessage,                                                           \r\n"
		"                       AdmissionDate = COALESCE(@AdmissionDate, AdmissionDate),                            \r\n"
		"                       DischargeDate = COALESCE(@DischargeDate, DischargeDate)                             \r\n"
		"                 WHERE CSN = @EncounterID;                                                                 \r\n"
        "             END;                                                                                          \r\n"
        "         ELSE                                                                                              \r\n"
        "             BEGIN                                                                                         \r\n"
        "                 IF @EncounterID IS NOT NULL                                                               \r\n"
        "                     BEGIN                                                                                 \r\n"
        "                         INSERT INTO dbo.LabDEEncounter                                                    \r\n"
        "                         (CSN,                                                                             \r\n"
        "                          PatientMRN,                                                                      \r\n"
        "                          EncounterDateTime,                                                               \r\n"
        "                          Department,                                                                      \r\n"
        "                          EncounterType,                                                                   \r\n"
        "                          EncounterProvider,                                                               \r\n"
		"                          ADTMessage,                                                                      \r\n"
		"                          AdmissionDate,                                                                   \r\n"
		"                          DischargeDate                                                                    \r\n"
		"                         )                                                                                 \r\n"
        "                         VALUES                                                                            \r\n"
        "                         (@EncounterID,                                                                    \r\n"
        "                          @PatientMRN,                                                                     \r\n"
        "                          @EncounterDateTime,                                                              \r\n"
        "                          @Department,                                                                     \r\n"
        "                          @EncounterType,                                                                  \r\n"
        "                          @EncounterProvider,                                                              \r\n"
		"                          @ADTMessage,                                                                     \r\n"
		"                          @AdmissionDate,                                                                   \r\n"
		"                          @DischargeDate                                                                  \r\n"
		"                         );                                                                                \r\n"
        "                     END;                                                                                  \r\n"
        "             END;                                                                                          \r\n"
        "     END;                                                                                                  \r\n";

	static const string gstrCREATE_PROCEDURE_ADD_OR_MODIFY_ENCOUNTER =
		"CREATE PROCEDURE [dbo].[LabDEAddOrUpdateEncounter]		 @PatientMRN        NVARCHAR(20),					 \r\n"
		"                                                        @EncounterID		NVARCHAR(20),                   \r\n"
		"                                                        @EncounterDateTime DATETIME,                       \r\n"
		"                                                        @Department        NVARCHAR(256),                  \r\n"
		"                                                        @EncounterType     NVARCHAR(256),                  \r\n"
		"                                                        @EncounterProvider NVARCHAR(256),                  \r\n"
		"                                                        @ADTMessage        XML                             \r\n"
		"AS                                                                                                         \r\n"
		"     BEGIN                                                                                                 \r\n"
		"         -- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.     \r\n"
		"         SET NOCOUNT ON;                                                                                   \r\n"
		"         EXEC [dbo].[LabDEAddOrUpdateEncounterAndIPDates]				                                    \r\n"
		"			@PatientMRN = @PatientMRN,                                                                      \r\n"
		"			@EncounterID = @EncounterID,		                                                            \r\n"
		"			@EncounterDateTime = @EncounterDateTime,                                                        \r\n"
		"			@Department = @Department,                                                                      \r\n"
		"			@EncounterType = @EncounterType,                                                                \r\n"
		"			@EncounterProvider = @EncounterProvider,                                                        \r\n"
		"			@ADTMessage	= @ADTMessage,																		\r\n"
		"			@AdmissionDate= NULL,																			\r\n"
		"			@DischargeDate = NULL																			\r\n"
		"	 END																									\r\n";