// LabDEProductDBMgr.cpp : Implementation of CLabDEProductDBMgr

#include "stdafx.h"
#include "LabDEProductDBMgr.h"
#include "LabDE_DB_SQL.h"
#include "LabDE_DB_SQL_Old_versions.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <COMUtils.h>
#include <LockGuard.h>
#include <FAMUtilsConstants.h>
#include <TransactionGuard.h>
#include <ADOUtils.h>
#include <FAMDBHelperFunctions.h>
#include <cpputil.h>

using namespace ADODB;
using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------

// This must be updated when the DB schema changes
// !!!ATTENTION!!!
// An UpdateToSchemaVersion method must be added when checking in a new schema version.
// Version 2: https://extract.atlassian.net/browse/ISSUE-12801
// Version 3: https://extract.atlassian.net/browse/ISSUE-12805 
//			  (Order.ReferenceDateTime and ORMMessage columns)
// Version 4: https://extract.atlassian.net/browse/ISSUE-12902
//			  Prefixed table names with "LabDE", added CollectionDate column to LabDEOrderFile
// Version 5: Added DOB index on LabDEPatient table.
// Version 6: Changed AddOrUpdateLabDEOrder stored procedure to be created as dbo.
// Version 7: https://extract.atlassian.net/browse/ISSUE-13579
//			  Added MergedInto and CurrentMRN column to LabDEPatient table.
// Version 8: https://extract.atlassian.net/browse/ISSUE-14152
//			  Create FAM DB schema to create encounters table
// Version 9: https://extract.atlassian.net/browse/ISSUE-14153
//            Create LabDEAddOrUpdateOrderWithEncounter stored procedure
// Version 10: https://extract.atlassian.net/browse/ISSUE-14164
//			  EncounterFile and PatientFile needed to track submissions against encounter and patient
// Version 11: https://extract.atlassian.net/browse/ISSUE-14199
//			  Added index for LabDEEncounter.PatientMRN
// Version 12:https://extract.atlassian.net/browse/ISSUE-14988
//			  Increased size of OrderNumber Field in LabDEOrder and LabDEOrderFile
// Version 13:https://extract.atlassian.net/browse/ISSUE-15000
//			  Added DischargeDate and AdmissionDate to LabDEEncounter table
// Version 14:https://extract.atlassian.net/browse/ISSUE-15106
//			  Added AccessionNunmber
// Version 15:https://extract.atlassian.net/browse/ISSUE-15225
//			  This just drops and recreates the LabDEAddOrUpdateEncounterAndIPDates stored procedure
// Version 16:https://extract.atlassian.net/browse/ISSUE-15225
//			  This just drops and recreates the LabDEAddOrUpdateOrderWithEncounterAndIPDates stored procedure
// WARNING -- When the version is changed, the corresponding switch handler needs to be updated, see WARNING!!!
static const long glLABDE_DB_SCHEMA_VERSION = 17;
static const string gstrLABDE_SCHEMA_VERSION_NAME = "LabDESchemaVersion";
static const string gstrDESCRIPTION = "LabDE database manager";

//------------------------------------------------------------------------------------------------- 
// Schema update functions
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion1(_ConnectionPtr ipConnection, long* pnNumSteps, 
    IProgressStatusPtr ipProgressStatus)
{
    try
    {
        int nNewSchemaVersion = 1;

        if (pnNumSteps != __nullptr)
        {
            *pnNumSteps += 3;
            return nNewSchemaVersion;
        }

        vector<string> vecQueries;
        vecQueries.push_back(gstrCREATE_PATIENT_TABLE_V1);
        vecQueries.push_back(gstrCREATE_PATIENT_FIRSTNAME_INDEX_V1);
        vecQueries.push_back(gstrCREATE_PATIENT_LASTNAME_INDEX_V1);
        vecQueries.push_back(gstrCREATE_ORDER_STATUS_TABLE_V1);
        vecQueries.push_back(gstrPOPULATE_ORDER_STATUSES_V2);
        vecQueries.push_back(gstrCREATE_ORDER_TABLE_V1);
        vecQueries.push_back(gstrADD_FK_ORDER_PATIENT_MRN_V1);
        vecQueries.push_back(gstrADD_FK_ORDER_ORDERSTATUS_V1);
        vecQueries.push_back(gstrCREATE_ORDER_MRN_INDEX_V1);
        vecQueries.push_back(gstrCREATE_ORDER_ORDERCODE_INDEX_V1);
        vecQueries.push_back(gstrCREATE_ORDER_FILE_TABLE_V1);
        vecQueries.push_back(gstrADD_FK_ORDERFILE_ORDER_V1);
        vecQueries.push_back(gstrADD_FK_ORDERFILE_FAMFILE_V1);
        vecQueries.push_back(gstrCREATE_ORDERFILE_ORDER_INDEX_V1);
        vecQueries.push_back(gstrCREATE_ORDERFILE_FAMFILE_INDEX_V1);

        vecQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES ('" + 
            gstrLABDE_SCHEMA_VERSION_NAME + "', '" + asString(nNewSchemaVersion) + "' )");

        executeVectorOfSQL(ipConnection, vecQueries);

        return nNewSchemaVersion;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37840");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion2(_ConnectionPtr ipConnection, long* pnNumSteps, 
    IProgressStatusPtr ipProgressStatus)
{
    try
    {
        int nNewSchemaVersion = 2;

        if (pnNumSteps != __nullptr)
        {
            *pnNumSteps += 3;
            return nNewSchemaVersion;
        }

        vector<string> vecQueries;
        
        // Add gender column to patient table.
        vecQueries.push_back("ALTER TABLE [dbo].[Patient] ADD [Gender] NCHAR(1) NULL");

        // Rename RequestDateTime column to ReceivedDateTime and drop CollectionDateTime
        vecQueries.push_back("EXEC sp_rename 'dbo.Order.RequestDateTime', 'ReceivedDateTime'");
        vecQueries.push_back("ALTER TABLE [dbo].[Order] ADD DEFAULT GETDATE() FOR [ReceivedDateTime]");
        vecQueries.push_back("ALTER TABLE [dbo].[Order] DROP COLUMN [CollectionDateTime]");
        
        // These indices all inappropriately had a unique constraint. Drop and re-add the indices to
        // fix.
        vecQueries.push_back("DROP INDEX [IX_Patient_FirstName] ON [dbo].[Patient]");
        vecQueries.push_back("DROP INDEX [IX_Patient_LastName] ON [dbo].[Patient]");
        vecQueries.push_back("DROP INDEX [IX_Order_OrderCode] ON [dbo].[Order]");
        vecQueries.push_back("DROP INDEX [IX_Order_PatientMRN] ON [dbo].[Order]");
        vecQueries.push_back("DROP INDEX [IX_OrderFile_Order] ON [dbo].[OrderFile]");
        vecQueries.push_back("DROP INDEX [IX_OrderFile_FAMFile] ON [dbo].[OrderFile]");
        vecQueries.push_back(gstrCREATE_PATIENT_FIRSTNAME_INDEX_V1);
        vecQueries.push_back(gstrCREATE_PATIENT_LASTNAME_INDEX_V1);
        vecQueries.push_back(gstrCREATE_ORDER_MRN_INDEX_V1);
        vecQueries.push_back(gstrCREATE_ORDER_ORDERCODERECEIVEDDATETIME_INDEX_V2);
        vecQueries.push_back(gstrCREATE_ORDERFILE_ORDER_INDEX_V1);
        vecQueries.push_back(gstrCREATE_ORDERFILE_FAMFILE_INDEX_V1);

        // We have changed the code "O" for "Open" to "A" for "Available"
        // This re-population would cause problems on databases that had existing orders, but at
        // this point no such databases exist, so...
        vecQueries.push_back("DELETE FROM [dbo].[OrderStatus]");
        vecQueries.push_back(gstrPOPULATE_ORDER_STATUSES_V2);

        vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
            "' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

        executeVectorOfSQL(ipConnection, vecQueries);

        return nNewSchemaVersion;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37890");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion3(_ConnectionPtr ipConnection, long* pnNumSteps, 
    IProgressStatusPtr ipProgressStatus)
{
    try
    {
        int nNewSchemaVersion = 3;

        if (pnNumSteps != __nullptr)
        {
            *pnNumSteps += 3;
            return nNewSchemaVersion;
        }

        vector<string> vecQueries;

        vecQueries.push_back("ALTER TABLE [Order] ADD [ReferenceDateTime] DATETIME");
        vecQueries.push_back("ALTER TABLE [Order] ADD [ORMMessage] XML");
        vecQueries.push_back(gstrCREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_V3);
        
        // The default was added to the Order table that was created with version 3
        vecQueries.push_back("ALTER TABLE [dbo].[Order] ADD DEFAULT 'A' FOR OrderStatus");

        vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
            "' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

        executeVectorOfSQL(ipConnection, vecQueries);

        return nNewSchemaVersion;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37920");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion4(_ConnectionPtr ipConnection, long* pnNumSteps, 
    IProgressStatusPtr ipProgressStatus)
{
    try
    {
        int nNewSchemaVersion = 4;

        if (pnNumSteps != __nullptr)
        {
            *pnNumSteps += 3;
            return nNewSchemaVersion;
        }

        vector<string> vecQueries;
        
        vecQueries.push_back("exec sp_rename 'Order', 'LabDEOrder'");
        vecQueries.push_back("exec sp_rename 'OrderFile', 'LabDEOrderFile'");
        vecQueries.push_back("exec sp_rename 'OrderStatus', 'LabDEOrderStatus'");
        vecQueries.push_back("exec sp_rename 'Patient', 'LabDEPatient'");
        vecQueries.push_back("ALTER TABLE LabDEOrderFile ADD [CollectionDate] DATETIME");
        vecQueries.push_back(gstrDROP_PROCEDURE_ADD_OR_UPDATE_ORDER);
        vecQueries.push_back(gstrCREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_V4);

        // Change Default value for ReceivedDateTime field on LabDEOrder
        vecQueries.push_back("UPDATE LabDEOrder SET ReceivedDateTime = GETDATE() WHERE (ReceivedDateTime IS NULL)");
        vecQueries.push_back("DROP INDEX IX_Order_OrderCodeReceivedDateTime ON LabDEOrder ");
        vecQueries.push_back("ALTER TABLE LabDEOrder ALTER COLUMN [ReceivedDateTime] DATETIME NOT NULL ");
        vecQueries.push_back(gstrCREATE_ORDER_ORDERCODERECEIVEDDATETIME_INDEX);


        vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
            "' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

        executeVectorOfSQL(ipConnection, vecQueries);

        return nNewSchemaVersion;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38120");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion5(_ConnectionPtr ipConnection, long* pnNumSteps, 
    IProgressStatusPtr ipProgressStatus)
{
    try
    {
        int nNewSchemaVersion = 5;

        if (pnNumSteps != __nullptr)
        {
            *pnNumSteps += 3;
            return nNewSchemaVersion;
        }

        vector<string> vecQueries;
        
        // Update existing patient table indices
        vecQueries.push_back("DROP INDEX [IX_Patient_FirstName] ON [dbo].[LabDEPatient]");
        vecQueries.push_back("DROP INDEX [IX_Patient_LastName] ON [dbo].[LabDEPatient]");
        vecQueries.push_back(gstrCREATE_PATIENT_FIRSTNAME_INDEX);
        vecQueries.push_back(gstrCREATE_PATIENT_LASTNAME_INDEX);

        // Add new DOB index.
        vecQueries.push_back(gstrCREATE_PATIENT_DOB_INDEX);

        vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
            "' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

        executeVectorOfSQL(ipConnection, vecQueries);

        return nNewSchemaVersion;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38279");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion6(_ConnectionPtr ipConnection, long* pnNumSteps, 
    IProgressStatusPtr ipProgressStatus)
{
    try
    {
        int nNewSchemaVersion = 6;

        if (pnNumSteps != __nullptr)
        {
            // This is such a small tweak-- use a single step as opposed to the usual 3.
            *pnNumSteps += 1;
            return nNewSchemaVersion;
        }

        vector<string> vecQueries;
        
        // Update AddOrUpdateLabDEOrder stored procedure to make it dbo
        vecQueries.push_back(gstrDROP_PROCEDURE_ADD_OR_UPDATE_ORDER);
        vecQueries.push_back(gstrCREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_V4);

        vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
            "' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

        executeVectorOfSQL(ipConnection, vecQueries);

        return nNewSchemaVersion;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38405");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion7(_ConnectionPtr ipConnection, long* pnNumSteps, 
    IProgressStatusPtr ipProgressStatus)
{
    try
    {
        int nNewSchemaVersion = 7;

        if (pnNumSteps != __nullptr)
        {
            *pnNumSteps += 10;
            return nNewSchemaVersion;
        }

        vector<string> vecQueries;
        
        vecQueries.push_back("ALTER TABLE [LabDEPatient] ADD [MergedInto] NVARCHAR(20) NULL");
        vecQueries.push_back("ALTER TABLE [LabDEPatient] ADD [CurrentMRN] NVARCHAR(20) NULL ");
        vecQueries.push_back("UPDATE [LabDEPatient] SET [CurrentMRN] = [MRN]");
        vecQueries.push_back("ALTER TABLE [LabDEPatient] ALTER COLUMN [CurrentMRN] NVARCHAR(20) NOT NULL ");
        vecQueries.push_back(gstrADD_FK_PATIENT_MERGEDINTO);
        vecQueries.push_back(gstrADD_FK_PATIENT_CURRENTMRN);
        vecQueries.push_back(gstrCREATE_PATIENT_CURRENT_MRN_INDEX);
        vecQueries.push_back(gstrCREATE_PROCEDURE_MERGE_PATIENTS);
        // Rename AddOrUpdateLabDEOrder to LabDEAddOrUpdateOrder
        vecQueries.push_back(gstrDROP_PROCEDURE_ADD_OR_UPDATE_ORDER);
        vecQueries.push_back(gstrCREATE_PROCEDURE_ADD_OR_UPDATE_ORDER);

        vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
            "' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

        executeVectorOfSQL(ipConnection, vecQueries);

        return nNewSchemaVersion;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39371");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion8(_ConnectionPtr ipConnection, 
                           long* pnNumSteps,
                           IProgressStatusPtr ipProgressStatus)
{
    try
    {
        int nNewSchemaVersion = 8;

        if (pnNumSteps != __nullptr)
        {
            *pnNumSteps += 10;
            return nNewSchemaVersion;
        }

        vector<string> vecQueries;

        // Now create the Encounter table
        vecQueries.push_back(gstrCREATE_ENCOUNTER_TABLE_V8);
        vecQueries.push_back(gstrADD_FK_ENCOUNTER_PATIENT);

        // Add EncounterID column to LabDEOrder table.
        vecQueries.push_back(gstrCREATE_ENCOUNTERID_COLUMN);

        // Add FK to LabDEOrder that references the new Encounter table.
        vecQueries.push_back(gstrADD_FK_ORDER_TO_ENCOUNTER);

        vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
            "' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

        executeVectorOfSQL(ipConnection, vecQueries);

        return nNewSchemaVersion;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41411");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion9(_ConnectionPtr ipConnection, 
                           long* pnNumSteps,
                           IProgressStatusPtr ipProgressStatus)
{
    try
    {
        int nNewSchemaVersion = 9;

        if (pnNumSteps != __nullptr)
        {
            *pnNumSteps += 10;
            return nNewSchemaVersion;
        }

        vector<string> vecQueries;

        vecQueries.push_back(gstr_CREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_WITH_ENCOUNTER_V9);
        vecQueries.push_back(gstrCREATE_PROCEDURE_ADD_OR_MODIFY_ENCOUNTER_V9);

        vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
            "' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

        executeVectorOfSQL(ipConnection, vecQueries);

        return nNewSchemaVersion;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41430");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion10(_ConnectionPtr ipConnection,
							long* pnNumSteps,
							IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 10;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 10;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// LabDEPatientFile table
		vecQueries.push_back(gstrCREATE_PATIENT_FILE_TABLE);
		vecQueries.push_back(gstrADD_FK_PATIENTFILE_PATIENT);
		vecQueries.push_back(gstrADD_FK_PATIENTFILE_FAMFILE);
		vecQueries.push_back(gstrCREATE_PATIENTFILE_PATIENT_INDEX);
		vecQueries.push_back(gstrCREATE_PATIENTFILE_FAMFILE_INDEX);

		// LabDEEncounterFile table
		vecQueries.push_back(gstrCREATE_ENCOUNTER_FILE_TABLE);
		vecQueries.push_back(gstrADD_FK_ENCOUNTERFILE_ENCOUNTER);
		vecQueries.push_back(gstrADD_FK_ENCOUNTERFILE_FAMFILE);
		vecQueries.push_back(gstrCREATE_ENCOUNTERFILE_ENCOUNTER_INDEX);
		vecQueries.push_back(gstrCREATE_ENCOUNTERFILE_FAMFILE_INDEX);

		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
			"' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41505");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion11(_ConnectionPtr ipConnection,
							long* pnNumSteps,
							IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 11;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 10;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// LabDEEncounter table changes
		vecQueries.push_back(gstrCREATE_ENCOUNTER_PATIENT_MRN_INDEX);

		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
			"' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41641");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion12(_ConnectionPtr ipConnection,
							long* pnNumSteps,
							IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 12;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 10;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		// Drop OrderNumber related constraints on LabDEOrderFile table
		vecQueries.push_back("ALTER TABLE [dbo].[LabDEOrderFile] DROP CONSTRAINT [FK_OrderFile_Order]");
		vecQueries.push_back("ALTER TABLE [dbo].[LabDEOrderFile] DROP CONSTRAINT [PK_OrderFile]");
		vecQueries.push_back("DROP INDEX [IX_OrderFile_Order] ON [dbo].[LabDEOrderFile]");

		// Drop Primary key on LabDEOrder table
		vecQueries.push_back("ALTER TABLE [dbo].[LabDEOrder] DROP CONSTRAINT PK_Order");

		// Resize OrderNumber columns
		vecQueries.push_back("ALTER TABLE [dbo].[LabDEOrderFile] ALTER COLUMN [OrderNumber] nvarchar(50) not null");
		vecQueries.push_back("ALTER TABLE [dbo].[LabDEOrder] ALTER COLUMN [OrderNumber] nvarchar(50) not null");

		// Add back Primary keys on LabDEOrder and LabDEOrderFile tables
		vecQueries.push_back("ALTER TABLE [dbo].[LabDEOrder] ADD CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED ([OrderNumber])");
		vecQueries.push_back("ALTER TABLE [dbo].[LabDEOrderFile] ADD CONSTRAINT [PK_OrderFile] PRIMARY KEY CLUSTERED ([OrderNumber], [FileID]) ");

		// Add back LabDEOrder and LabDEOrderFile foreign key
		vecQueries.push_back(gstrADD_FK_ORDERFILE_ORDER);

		// Add back the LabDEOrderFile index on OrderNumber
		vecQueries.push_back("CREATE NONCLUSTERED INDEX [IX_OrderFile_Order] ON [LabDEOrderFile]([OrderNumber])");
		

		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
			"' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45026");
}

//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion13(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 13;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 10;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("ALTER TABLE [dbo].[LabDEEncounter] ADD [DischargeDate] DATETIME NULL");
		vecQueries.push_back("ALTER TABLE [dbo].[LabDEEncounter] ADD [AdmissionDate] DATETIME NULL");
		// redefining these stored procedures to use the new ones
		vecQueries.push_back("DROP PROCEDURE [dbo].[LabDEAddOrUpdateEncounter]");
		vecQueries.push_back("DROP PROCEDURE [dbo].[LabDEAddOrUpdateOrderWithEncounter]");
		// The new stored procedures need to be created before the older names
		vecQueries.push_back(gstrCREATE_PROCEDURE_ADD_OR_MODIFY_ENCOUNTER_AND_IP_DATES_V13);
		vecQueries.push_back(gstr_CREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_WITH_ENCOUNTER_AND_IP_DATES_V13);

		vecQueries.push_back(gstrCREATE_PROCEDURE_ADD_OR_MODIFY_ENCOUNTER_V13);
		vecQueries.push_back(gstr_CREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_WITH_ENCOUNTER_V13);

		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
			"' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45095");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion14(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 14;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 10;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("ALTER TABLE [dbo].[LabDEOrder] ADD [AccessionNumber] NVARCHAR(50) NULL");
		// redefining these stored procedures to use the new ones
		vecQueries.push_back("DROP PROCEDURE [dbo].[LabDEAddOrUpdateOrderWithEncounter]");
		vecQueries.push_back("DROP PROCEDURE [dbo].[LabDEAddOrUpdateOrderWithEncounterAndIPDates]");
		vecQueries.push_back("DROP PROCEDURE [dbo].[LabDEAddOrUpdateOrder]");
		// The new stored procedures need to be created before the older names
		vecQueries.push_back(gstr_CREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_WITH_ENCOUNTER_AND_IP_DATES);

		vecQueries.push_back(gstrCREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_WITH_ACCESSION);

		vecQueries.push_back(gstr_CREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_WITH_ENCOUNTER);
		vecQueries.push_back(gstrCREATE_PROCEDURE_ADD_OR_UPDATE_ORDER);

		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
			"' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45286");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion15(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 15;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("DROP PROCEDURE [dbo].[LabDEAddOrUpdateEncounterAndIPDates]");

		vecQueries.push_back(gstrCREATE_PROCEDURE_ADD_OR_MODIFY_ENCOUNTER_AND_IP_DATES);

		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
			"' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45493");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion16(_ConnectionPtr ipConnection,
	long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 16;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("DROP PROCEDURE [dbo].[LabDEAddOrUpdateOrderWithEncounterAndIPDates]");

		vecQueries.push_back(gstr_CREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_WITH_ENCOUNTER_AND_IP_DATES);

		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
			"' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45538");
}
//-------------------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion17(_ConnectionPtr ipConnection, long* pnNumSteps,
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		const int nNewSchemaVersion = 17;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;
		vecQueries.push_back(
		"DECLARE @sql NVARCHAR(MAX) = N''; "
		"	SELECT @sql += N'DROP INDEX ' "
		"	+ QUOTENAME(SCHEMA_NAME(sys.tables.[schema_id])) "
		"	+ '.' + QUOTENAME(sys.tables.[name]) "
		"	+ '.' + QUOTENAME(sys.indexes.[name]) + ';' "
		"	FROM "
		"		sys.indexes "
		"			INNER JOIN sys.tables "
		"				ON sys.indexes.[object_id] = sys.tables.[object_id] "
		"	WHERE "
		"		sys.indexes.is_primary_key = 0 "
		"		AND "
		"		sys.indexes.index_id < > 0 "
		"		AND "
		"		sys.tables.is_ms_shipped = 0 "
		"		AND "
		"		sys.tables.[name] = 'LabDEProvider'; "

		" EXEC sp_executesql @sql; " );
		vecQueries.push_back(gstrCREATE_LABDE_PROVIDER_TABLE);
		vecQueries.push_back(gstrCREATE_LABDE_PROVIDER_INDEX_FIRST);
		vecQueries.push_back(gstrCREATE_LABDE_PROVIDER_INDEX_LAST);
		vecQueries.push_back(gstrCREATE_LABDE_PROVIDER_INDEX_FIRSTLAST);
		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '" + asString(nNewSchemaVersion) +
			"' WHERE [Name] = '" + gstrLABDE_SCHEMA_VERSION_NAME + "'");

		// Execute the queries
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46997");
}

//-------------------------------------------------------------------------------------------------
// CLabDEProductDBMgr
//-------------------------------------------------------------------------------------------------
CLabDEProductDBMgr::CLabDEProductDBMgr()
: m_ipFAMDB(__nullptr)
, m_nNumberOfRetries(0)
, m_dRetryTimeout(0.0)
, m_currentRole(CppBaseApplicationRoleConnection::kExtractRole)
{
}
//-------------------------------------------------------------------------------------------------
CLabDEProductDBMgr::~CLabDEProductDBMgr()
{
    try
    {
        m_ipFAMDB = __nullptr;
        m_ipDBConnection = __nullptr;
    }
    CATCH_AND_LOG_ALL_EXCEPTIONS("ELI37841");
}
//-------------------------------------------------------------------------------------------------
HRESULT CLabDEProductDBMgr::FinalConstruct()
{
    return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CLabDEProductDBMgr::FinalRelease()
{
    try
    {
        // Release COM objects before the object is destructed
        m_ipFAMDB = __nullptr;
        m_ipDBConnection = __nullptr;
    }
    CATCH_AND_LOG_ALL_EXCEPTIONS("ELI37842");
}
//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLabDEProductDBMgr::InterfaceSupportsErrorInfo(REFIID riid)
{
    static const IID* arr[] = 
    {
        &IID_ILabDEProductDBMgr,
        &__uuidof(IProductSpecificDBMgr),
        &IID_ICategorizedComponent,
        &IID_ILicensedComponent
    };

    for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
    {
        if (InlineIsEqualGUID(*arr[i],riid))
            return S_OK;
    }
    return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLabDEProductDBMgr::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
    try
    {
        AFX_MANAGE_STATE(AfxGetStaticModuleState());

        ASSERT_ARGUMENT("ELI37843", pstrComponentDescription != __nullptr);

        *pstrComponentDescription = _bstr_t(gstrDESCRIPTION.c_str()).Detach();
    
        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37844");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLabDEProductDBMgr::raw_IsLicensed(VARIANT_BOOL  * pbValue)
{
    try
    {
        AFX_MANAGE_STATE(AfxGetStaticModuleState());

        ASSERT_ARGUMENT("ELI37845", pbValue != __nullptr);

        try
        {
            // check the license
            validateLicense();

            // If no exception, then pbValue is true
            *pbValue = VARIANT_TRUE;
        }
        catch(...)
        {
            *pbValue = VARIANT_FALSE;
        }

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37846");
}

//-------------------------------------------------------------------------------------------------
// IProductSpecificDBMgr Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLabDEProductDBMgr::raw_AddProductSpecificSchema(_Connection* pConnection, 
                                                              IFileProcessingDB *pDB,
                                                              VARIANT_BOOL bOnlyTables,
                                                              VARIANT_BOOL bAddUserTables)
{
    try
    {
        AFX_MANAGE_STATE(AfxGetStaticModuleState());

        // Make DB a smart pointer
        IFileProcessingDBPtr ipDB(pDB);
        ASSERT_RESOURCE_ALLOCATION("ELI37847", ipDB != __nullptr);

        // Create the connection object
        _ConnectionPtr ipDBConnection(pConnection);
        ASSERT_RESOURCE_ALLOCATION("ELI37848", ipDBConnection != __nullptr);

        // Retrieve the queries for creating LabDE DB table(s).
        const vector<string> vecTableCreationQueries = getTableCreationQueries(asCppBool(bAddUserTables));
        vector<string> vecCreateQueries(vecTableCreationQueries.begin(), vecTableCreationQueries.end());

        // Add the queries to create keys/constraints.
        vecCreateQueries.push_back(gstrCREATE_PATIENT_FIRSTNAME_INDEX);
        vecCreateQueries.push_back(gstrCREATE_PATIENT_LASTNAME_INDEX);
        vecCreateQueries.push_back(gstrCREATE_PATIENT_DOB_INDEX);
        vecCreateQueries.push_back(gstrCREATE_PATIENT_CURRENT_MRN_INDEX);
        vecCreateQueries.push_back(gstrPOPULATE_ORDER_STATUSES);
        vecCreateQueries.push_back(gstrADD_FK_PATIENT_MERGEDINTO);
        vecCreateQueries.push_back(gstrADD_FK_PATIENT_CURRENTMRN);
        vecCreateQueries.push_back(gstrADD_FK_ORDER_PATIENT_MRN);
        vecCreateQueries.push_back(gstrADD_FK_ORDER_ORDERSTATUS);
        vecCreateQueries.push_back(gstrCREATE_ORDER_MRN_INDEX);
        vecCreateQueries.push_back(gstrCREATE_ORDER_ORDERCODERECEIVEDDATETIME_INDEX);
        vecCreateQueries.push_back(gstrADD_FK_ORDERFILE_ORDER);
        vecCreateQueries.push_back(gstrADD_FK_ORDERFILE_FAMFILE);
        vecCreateQueries.push_back(gstrCREATE_ORDERFILE_ORDER_INDEX);
        vecCreateQueries.push_back(gstrCREATE_ORDERFILE_FAMFILE_INDEX);
        vecCreateQueries.push_back(gstrADD_FK_ENCOUNTER_PATIENT);
        vecCreateQueries.push_back(gstrADD_FK_ORDER_TO_ENCOUNTER);
		vecCreateQueries.push_back(gstrADD_FK_ENCOUNTERFILE_ENCOUNTER);
		vecCreateQueries.push_back(gstrADD_FK_ENCOUNTERFILE_FAMFILE);
		vecCreateQueries.push_back(gstrCREATE_ENCOUNTERFILE_ENCOUNTER_INDEX);
		vecCreateQueries.push_back(gstrCREATE_ENCOUNTERFILE_FAMFILE_INDEX);
		vecCreateQueries.push_back(gstrADD_FK_PATIENTFILE_PATIENT);
		vecCreateQueries.push_back(gstrADD_FK_PATIENTFILE_FAMFILE);
		vecCreateQueries.push_back(gstrCREATE_PATIENTFILE_PATIENT_INDEX);
		vecCreateQueries.push_back(gstrCREATE_PATIENTFILE_FAMFILE_INDEX);
		vecCreateQueries.push_back(gstrCREATE_ENCOUNTER_PATIENT_MRN_INDEX);
		vecCreateQueries.push_back(gstrCREATE_LABDE_PROVIDER_INDEX_FIRST);
		vecCreateQueries.push_back(gstrCREATE_LABDE_PROVIDER_INDEX_LAST);
		vecCreateQueries.push_back(gstrCREATE_LABDE_PROVIDER_INDEX_FIRSTLAST);

        if (!asCppBool(bOnlyTables))
        {
            // Add stored procedures
            vecCreateQueries.push_back(gstrCREATE_PROCEDURE_MERGE_PATIENTS);
			// The new stored procedures need to be created before the older names
			vecCreateQueries.push_back(gstrCREATE_PROCEDURE_ADD_OR_MODIFY_ENCOUNTER_AND_IP_DATES);
			vecCreateQueries.push_back(gstr_CREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_WITH_ENCOUNTER_AND_IP_DATES);
			vecCreateQueries.push_back(gstrCREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_WITH_ACCESSION);

            vecCreateQueries.push_back(gstrCREATE_PROCEDURE_ADD_OR_UPDATE_ORDER);
            vecCreateQueries.push_back(gstr_CREATE_PROCEDURE_ADD_OR_UPDATE_ORDER_WITH_ENCOUNTER);
            vecCreateQueries.push_back(gstrCREATE_PROCEDURE_ADD_OR_MODIFY_ENCOUNTER);
        }

        // Execute the queries to create the LabDE tables
        executeVectorOfSQL(ipDBConnection, vecCreateQueries);

        // Set the default values for the DBInfo settings.
        map<string, string> mapDBInfoDefaultValues = getDBInfoDefaultValues();
        for (map<string, string>::iterator iterDBInfoValues = mapDBInfoDefaultValues.begin();
            iterDBInfoValues != mapDBInfoDefaultValues.end();
            iterDBInfoValues++)
        {
            VARIANT_BOOL vbSetIfExists =
                asVariantBool(iterDBInfoValues->first == gstrLABDE_SCHEMA_VERSION_NAME);

            ipDB->SetDBInfoSetting(iterDBInfoValues->first.c_str(),
                iterDBInfoValues->second.c_str(), vbSetIfExists, VARIANT_FALSE);
        }

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37849");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLabDEProductDBMgr::raw_AddProductSpecificSchema80(IFileProcessingDB *pDB)
{
    // LabDEProductDBMgr did not exist in 8.0.
    return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLabDEProductDBMgr::raw_RemoveProductSpecificSchema(_Connection* pConnection,
                                                                 IFileProcessingDB *pDB,
                                                                 VARIANT_BOOL bOnlyTables,
                                                                 VARIANT_BOOL bRetainUserTables,
                                                                 VARIANT_BOOL *pbSchemaExists)
{
    try
    {
        AFX_MANAGE_STATE(AfxGetStaticModuleState());

        ASSERT_ARGUMENT("ELI38282", pbSchemaExists != __nullptr);

        _ConnectionPtr ipConnection(pConnection);
        ASSERT_RESOURCE_ALLOCATION("ELI53066", ipConnection != __nullptr);

        // Make DB a smart pointer
        IFileProcessingDBPtr ipDB(pDB);
        ASSERT_RESOURCE_ALLOCATION("ELI37850", ipDB != __nullptr);

        m_ipFAMDB = ipDB;

        string strValue = asString(ipDB->GetDBInfoSetting(
            gstrLABDE_SCHEMA_VERSION_NAME.c_str(), VARIANT_FALSE));

        if (strValue.empty())
        {
            *pbSchemaExists = VARIANT_FALSE;
            return S_OK;
        }
        else
        {
            *pbSchemaExists = VARIANT_TRUE;
        }

        vector<string> vecTables;
        getLabDETables(vecTables);

        dropTablesInVector(ipConnection, vecTables);

        if (!asCppBool(bOnlyTables))
        {
            executeCmdQuery(ipConnection, "DROP PROCEDURE [CullEmptyORMMessageNodes]", false);
        }

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37852");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLabDEProductDBMgr::raw_ValidateSchema(IFileProcessingDB* pDB)
{
    try
    {
        AFX_MANAGE_STATE(AfxGetStaticModuleState());

        // Update the FAMDB pointer if it is new.
        if (m_ipFAMDB != pDB)
        {
            m_ipFAMDB = pDB;
            m_ipFAMDB->GetConnectionRetrySettings(&m_nNumberOfRetries, &m_dRetryTimeout);
        
            // Reset the database connection
            m_ipDBConnection = __nullptr;
        }

        validateLabDESchemaVersion(true);

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37853");
}
//-------------------------------------------------------------------------------------------------
// WARNING: If any DBInfo row is removed, this code needs to be modified so that it does not treat
// the removed element(s) on and old schema versions as unrecognized.
STDMETHODIMP CLabDEProductDBMgr::raw_GetDBInfoRows(IVariantVector** ppDBInfoRows)
{
    try
    {
        AFX_MANAGE_STATE(AfxGetStaticModuleState());

        IVariantVectorPtr ipDBInfoRows(CLSID_VariantVector);
        ASSERT_RESOURCE_ALLOCATION("ELI37854", ipDBInfoRows != __nullptr);

        map<string, string> mapDBInfoValues = getDBInfoDefaultValues();
        for (map<string, string>::iterator iterDBInfoValues = mapDBInfoValues.begin();
             iterDBInfoValues != mapDBInfoValues.end();
             iterDBInfoValues++)
        {
            ipDBInfoRows->PushBack(iterDBInfoValues->first.c_str());
        }

        *ppDBInfoRows = ipDBInfoRows.Detach();

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37855");
}
//-------------------------------------------------------------------------------------------------
// WARNING: If any table is removed, this code needs to be modified so that it does not treat the
// removed element(s) on and old schema versions as unrecognized.
STDMETHODIMP CLabDEProductDBMgr::raw_GetTables(IVariantVector** ppTables)
{
    try
    {
        AFX_MANAGE_STATE(AfxGetStaticModuleState());

        IVariantVectorPtr ipTables(CLSID_VariantVector);
        ASSERT_RESOURCE_ALLOCATION("ELI37856", ipTables != __nullptr);

        const vector<string> vecTableCreationQueries = getTableCreationQueries(true);
        vector<string> vecTablesNames = getTableNamesFromCreationQueries(vecTableCreationQueries);

        // Add tables that have been deleted
        addOldTables(vecTablesNames);

        for (vector<string>::iterator iter = vecTablesNames.begin();
             iter != vecTablesNames.end();
             iter++)
        {
            ipTables->PushBack(iter->c_str());
        }

        *ppTables = ipTables.Detach();

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37857");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLabDEProductDBMgr::raw_UpdateSchemaForFAMDBVersion(IFileProcessingDB* pDB,
    _Connection* pConnection, long nFAMDBSchemaVersion, long* pnProdSchemaVersion, long* pnNumSteps,
    IProgressStatus* pProgressStatus)
{
    try
    {
        AFX_MANAGE_STATE(AfxGetStaticModuleState());

        IFileProcessingDBPtr ipDB(pDB);
        ASSERT_ARGUMENT("ELI37858", ipDB != __nullptr);

        _ConnectionPtr ipConnection(pConnection);
        ASSERT_ARGUMENT("ELI37859", ipConnection != __nullptr);

        ASSERT_ARGUMENT("ELI37860", pnProdSchemaVersion != __nullptr);

        if (*pnProdSchemaVersion == 0)
        {
            string strVersion = asString(
                ipDB->GetDBInfoSetting(gstrLABDE_SCHEMA_VERSION_NAME.c_str(), VARIANT_FALSE));
            
            // If upgrading past the FAMDBSchemaVersion where the LabDE specific components first
            // became available, offer the user the choice whether to add them. If the user chooses
            // not to add the LabDE specific components, they will not be prompted again once the
            // schema is upgraded beyond FAMDBSchemaVersion 123.
            if (nFAMDBSchemaVersion == 123 && strVersion.empty())
            {
                // If adding the LabDE schema elements, treat this as schema version "0" rather
                // than non-existent so that if falls into the update code.
                strVersion = "0";
            }
            else if (strVersion.empty())
            {
                // if FAMDBSchemaVersion is 184 all product specific schemas should exist so add the product schema
                if (nFAMDBSchemaVersion == 184)
                {
                    if (pnNumSteps != __nullptr)
                    {
                        *pnNumSteps = 8;
                        *pnProdSchemaVersion = 0;
                    }
                    else
                    {
                        IProductSpecificDBMgrPtr ipThis(this);
                        ipThis->AddProductSpecificSchema(ipConnection, ipDB, VARIANT_FALSE, VARIANT_TRUE);
                        *pnProdSchemaVersion = glLABDE_DB_SCHEMA_VERSION;
                    }
                }
                return S_OK;
            }

            *pnProdSchemaVersion = asLong(strVersion);
        }

        // WARNING!!! - Fix up this switch when the version has been changed.
        switch (*pnProdSchemaVersion)
        {
            case 0:	// The initial schema should be added against FAM DB schema version 123
                    if (nFAMDBSchemaVersion == 123)
                    {
                        *pnProdSchemaVersion = UpdateToSchemaVersion1(ipConnection, pnNumSteps, NULL);
                    }
                    break;

            case 1: // The schema update from 1 to 2 needs to take place against FAM DB schema version 125
                    if (nFAMDBSchemaVersion == 125)
                    {
                        *pnProdSchemaVersion = UpdateToSchemaVersion2(ipConnection, pnNumSteps, NULL);
                    }
                    // Intentionally leaving out break since both updates 2 and 3 take place within
                    // FAM schema 125.

            case 2: // The schema update from 2 to 3 needs to take place against FAM DB schema version 125
                    if (nFAMDBSchemaVersion == 125)
                    {
                        *pnProdSchemaVersion = UpdateToSchemaVersion3(ipConnection, pnNumSteps, NULL);
                    }
                    break;

            case 3: // The schema update from 3 to 4 needs to take place against FAM DB schema version 127
                    if (nFAMDBSchemaVersion == 127)
                    {
                        *pnProdSchemaVersion = UpdateToSchemaVersion4(ipConnection, pnNumSteps, NULL);
                    }
                    // Intentionally leaving out break since both updates 4 and 5 take place within
                    // FAM schema 127.

            case 4: // The schema update from 4 to 5 needs to take place against FAM DB schema version 127
                    if (nFAMDBSchemaVersion == 127)
                    {
                        *pnProdSchemaVersion = UpdateToSchemaVersion5(ipConnection, pnNumSteps, NULL);
                    }
                    // Intentionally leaving out break since both updates 4 and 5 take place within
                    // FAM schema 127.

            case 5: // The schema update from 5 to 6 needs to take place against FAM DB schema version 127
                    if (nFAMDBSchemaVersion == 127)
                    {
                        *pnProdSchemaVersion = UpdateToSchemaVersion6(ipConnection, pnNumSteps, NULL);
                    }
                    break;

            case 6: // The schema update from 6 to 7 needs to take place against FAM DB schema version 136
                    if (nFAMDBSchemaVersion == 136)
                    {
                        *pnProdSchemaVersion = UpdateToSchemaVersion7(ipConnection, pnNumSteps, NULL);
                    }
                    break;

            case 7: // The schema update from 7 to 8 needs to take place using FAM DB schema version 141
                    if (nFAMDBSchemaVersion == 141)
                    {
                        *pnProdSchemaVersion = UpdateToSchemaVersion8(ipConnection, pnNumSteps, NULL);
                    }
                    // Intentionally leaving out break since both updates 7 and 8 take place within
                    // FAM schema 141.                   
            case 8:	// The schema update from 8 to 9 needs to take place using FAM DB schema version 141
                    if (nFAMDBSchemaVersion == 141)
                    {
                        *pnProdSchemaVersion = UpdateToSchemaVersion9(ipConnection, pnNumSteps, NULL);
                    }
					// Intentionally leaving out break since both updates 8 and 9 take place within
					// FAM schema 141.                   
			case 9:	// The schema update from 9 to 10 needs to take place using FAM DB schema version 141
					if (nFAMDBSchemaVersion == 141)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion10(ipConnection, pnNumSteps, NULL);
					}
					// Intentionally leaving out break since both updates 9 and 10 take place within
					// FAM schema 141.                   
			case 10:// The schema update from 10 to 11 needs to take place using FAM DB schema version 141
					if (nFAMDBSchemaVersion == 141)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion11(ipConnection, pnNumSteps, NULL);
					}
					break;
			case 11:// The schema update from 11 to 12 needs to take place using FAM DB schema version 156	
					if (nFAMDBSchemaVersion == 156)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion12(ipConnection, pnNumSteps, NULL);
					}
					break;
			case 12:// The schema update from 12 to 13 needs to take place using FAM DB schema version 157
					if (nFAMDBSchemaVersion == 157)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion13(ipConnection, pnNumSteps, NULL);
					}
					// no break since 12 to 13 are within version 157
			case 13:// The schema update from 13 to 14 needs to take place using FAM DB schema version 157
					if (nFAMDBSchemaVersion == 157)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion14(ipConnection, pnNumSteps, NULL);
					}
					break;
			case 14: // The schema update from 14 to 15 needs to take place using FAM DB schema version 160
					if (nFAMDBSchemaVersion == 160)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion15(ipConnection, pnNumSteps, NULL);
					}
					break;
			case 15: // The schema update from 15 to 16 needs to take place using FAM DB schema version 162
					if (nFAMDBSchemaVersion == 162)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion16(ipConnection, pnNumSteps, NULL);
					}
					break;
			case 16: 
					// The schema update from 15 to 16 needs to take place using FAM DB schema version 172
					if (nFAMDBSchemaVersion == 172)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion17(ipConnection, pnNumSteps, NULL);
					}
					break;
			case 17:
					break;

            default:
            {
                UCLIDException ue("ELI37861",
                    "Automatic updates are not supported for the current schema.");
                ue.addDebugInfo("FAM Schema Version", nFAMDBSchemaVersion, false);
                ue.addDebugInfo("LabDE Schema Version", *pnProdSchemaVersion, false);
                throw ue;
            }
        }

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37862");
}

//-------------------------------------------------------------------------------------------------
// ILabDEProductDBMgr Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLabDEProductDBMgr::put_FAMDB(IFileProcessingDB* newVal)
{
    try
    {
        AFX_MANAGE_STATE(AfxGetStaticModuleState());

        ASSERT_ARGUMENT("ELI37863", newVal != __nullptr);

        // Only update if it is a new value
        if (m_ipFAMDB != newVal)
        {
            m_ipFAMDB = newVal;
            m_ipFAMDB->GetConnectionRetrySettings(&m_nNumberOfRetries, &m_dRetryTimeout);
        
            // Reset the database connection
            m_ipDBConnection = __nullptr;
        }

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37864");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
shared_ptr<CppBaseApplicationRoleConnection> CLabDEProductDBMgr::getAppRoleConnection()
{
    // If the FAMDB is not set throw an exception
    if (m_ipFAMDB == __nullptr)
    {
        UCLIDException ue("ELI37865",
            "FAMDB pointer has not been initialized! Unable to open connection.");
        throw ue;
    }

	bool connectionExists = m_ipDBConnection != __nullptr && m_ipDBConnection->ADOConnection()->State != adStateClosed;

	if (connectionExists)
	{
		return m_ipDBConnection;
	}

	_ConnectionPtr adoConnection(_uuidof(Connection));
    ASSERT_RESOURCE_ALLOCATION("ELI51856", adoConnection != __nullptr);

	// Get database server from FAMDB
	string strDatabaseServer = asString(m_ipFAMDB->DatabaseServer);

	// Get DatabaseName from FAMDB
	string strDatabaseName = asString(m_ipFAMDB->DatabaseName);

	// create the connection string
	string strConnectionString = createConnectionString(strDatabaseServer, strDatabaseName);
    if (!strDatabaseServer.empty() && !strDatabaseName.empty())
    {
		adoConnection->Open(strConnectionString.c_str(), "", "", adConnectUnspecified);

		// Get the command timeout from the FAMDB DBInfo table
		adoConnection->CommandTimeout =
			asLong(m_ipFAMDB->GetDBInfoSetting(gstrCOMMAND_TIMEOUT.c_str(), VARIANT_TRUE));
    }

    long nDBHash = asLong(m_ipFAMDB->GetDBInfoSetting("DatabaseHash", VARIANT_FALSE));
    m_ipDBConnection = m_roleUtility.CreateAppRole(adoConnection, m_currentRole, nDBHash);

	return m_ipDBConnection;
}
//-------------------------------------------------------------------------------------------------
void CLabDEProductDBMgr::validateLicense()
{
    // May eventually want to create & use a gnLABDE_CORE_OBJECTS license ID.
    VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI37867", gstrDESCRIPTION);
}
//-------------------------------------------------------------------------------------------------
void CLabDEProductDBMgr::getLabDETables(vector<string>& rvecTables)
{
    rvecTables.clear();
    rvecTables.push_back(gstrLABDE_PATIENT_TABLE);
    rvecTables.push_back(gstrLABDE_ORDER_STATUS_TABLE);
    rvecTables.push_back(gstrLABDE_ORDER_TABLE);
    rvecTables.push_back(gstrLABDE_ORDER_FILE_TABLE);
	rvecTables.push_back(gstrLABDE_ENCOUNTER_TABLE);
	rvecTables.push_back(gstrLABDE_ENCOUNTER_FILE_TABLE);
	rvecTables.push_back(gstrLABDE_PATIENT_FILE_TABLE);
	rvecTables.push_back(gstrCREATE_LABDE_PROVIDER_TABLE);
}
//-------------------------------------------------------------------------------------------------
void CLabDEProductDBMgr::validateLabDESchemaVersion(bool bThrowIfMissing)
{
    ASSERT_RESOURCE_ALLOCATION("ELI37868", m_ipFAMDB != __nullptr);

    // Get the Version from the FAMDB DBInfo table
    string strValue = asString(m_ipFAMDB->GetDBInfoSetting(
        gstrLABDE_SCHEMA_VERSION_NAME.c_str(), asVariantBool(bThrowIfMissing)));

    if (bThrowIfMissing || !strValue.empty())
    {
        // Check against expected version
        if (asLong(strValue) != glLABDE_DB_SCHEMA_VERSION)
        {
            UCLIDException ue("ELI37869", "LabDE database schema is not current version!");
            ue.addDebugInfo("Expected", glLABDE_DB_SCHEMA_VERSION);
            ue.addDebugInfo("Database Version", strValue);
            throw ue;
        }
    }
}
//-------------------------------------------------------------------------------------------------
const vector<string> CLabDEProductDBMgr::getTableCreationQueries(bool bAddUserTables)
{
    vector<string> vecQueries;

    // WARNING: If any table is removed, code needs to be modified so that
    // findUnrecognizedSchemaElements does not treat the element on old schema versions as
    // unrecognized.
    vecQueries.push_back(gstrCREATE_PATIENT_TABLE);
    vecQueries.push_back(gstrCREATE_ORDER_STATUS_TABLE);
    vecQueries.push_back(gstrCREATE_ORDER_TABLE);
    vecQueries.push_back(gstrCREATE_ORDER_FILE_TABLE);
	vecQueries.push_back(gstrCREATE_ENCOUNTER_TABLE);
	vecQueries.push_back(gstrCREATE_ENCOUNTER_FILE_TABLE);
	vecQueries.push_back(gstrCREATE_PATIENT_FILE_TABLE);
	vecQueries.push_back(gstrCREATE_LABDE_PROVIDER_TABLE);

    return vecQueries;
}
//-------------------------------------------------------------------------------------------------
map<string, string> CLabDEProductDBMgr::getDBInfoDefaultValues()
{
    map<string, string> mapDefaultValues;

    // WARNING: If any DBInfo row is removed, code needs to be modified so that
    // findUnrecognizedSchemaElements does not treat the element on old schema versions as
    // unrecognized.
    mapDefaultValues[gstrLABDE_SCHEMA_VERSION_NAME] = asString(glLABDE_DB_SCHEMA_VERSION);

    return mapDefaultValues;
}
//-------------------------------------------------------------------------------------------------
void CLabDEProductDBMgr::addOldTables(vector<string>& vecTables)
{
    // Version 4 renamed the tables
    vecTables.push_back(gstrORDER_FILE_TABLE);
    vecTables.push_back(gstrPATIENT_TABLE);
    vecTables.push_back(gstrORDER_TABLE);
    vecTables.push_back(gstrORDER_STATUS_TABLE);
}
//-------------------------------------------------------------------------------------------------
