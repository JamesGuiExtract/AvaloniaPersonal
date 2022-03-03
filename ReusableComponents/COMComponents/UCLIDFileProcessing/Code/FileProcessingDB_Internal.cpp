// FileProcessingDB_Internal.cpp : Implementation of CFileProcessingDB private methods

#include "stdafx.h"
#include "FileProcessingDB.h"
#include "FAMDB_SQL.h"
#include "FAMDB_SQL_80.h"
#include "FAMDB_SQL_Legacy.h"
#include "FPCategories.h"
#include "FAMDBHelperFunctions.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <ExtractMFCUtils.h>
#include <LicenseMgmt.h>
#include <EncryptionEngine.h>
#include <ComponentLicenseIDs.h>
#include <FAMUtilsConstants.h>
#include <ADOUtils.h>
#include <StopWatch.h>
#include <StringTokenizer.h>
#include <stringCSIS.h>
#include <UPI.h>
#include <ValueRestorer.h>
#include <VectorOperations.h>
#include <FAMDBSemaphore.h>
#include <CppApplicationRoleConnection.h>
#include <LicenseUtils.h>

#include <string>
#include <memory>
#include <map>

using namespace ADODB;
using namespace FAMUtils;
using namespace std;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// Define four UCLID passwords used for encrypting the password
// NOTE: These passwords were not exposed at the header file level because
//		 no user of this class needs to know that these passwords exist
// These passwords are also uses in the FileProcessingDB.cpp
const unsigned long	gulFAMKey1 = 0x78932517;
const unsigned long	gulFAMKey2 = 0x193E2224;
const unsigned long	gulFAMKey3 = 0x20134253;
const unsigned long	gulFAMKey4 = 0x15990323;

static const string gstrTAG_REGULAR_EXPRESSION = "^[a-zA-Z0-9_][a-zA-Z0-9\\s_]*$";

//--------------------------------------------------------------------------------------------------
// FILE-SCOPE FUNCTIONS
//--------------------------------------------------------------------------------------------------
// NOTE: This function is purposely not exposed at header file level as a class
//		 method, as no user of this class needs to know that such a function
//		 exists.
void getFAMPassword(ByteStream& rPasswordBytes)
{
	ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, rPasswordBytes);
	
	bsm << gulFAMKey1;
	bsm << gulFAMKey2;
	bsm << gulFAMKey3;
	bsm << gulFAMKey4;
	bsm.flushToByteStream(8);
}
//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::closeDBConnection()
{
	// Lock mutex to keep other instances from running code that may cause the
	// connection to be reset
	CSingleLock lg(&m_criticalSection, TRUE);

	// Get the current thread ID
	DWORD dwThreadID = GetCurrentThreadId();

	auto it = m_mapThreadIDtoDBConnections.find(dwThreadID);

	if (it != m_mapThreadIDtoDBConnections.end())
	{
		// close the connection if it is open
		_ConnectionPtr ipConnection = it->second->ADOConnection();
		if (ipConnection != __nullptr && ipConnection->State != adStateClosed)
	{
			// close the database connection
			ipConnection->Close();
		}
		// Post message indicating that the database's connection is no longer established
		postStatusUpdateNotification(kConnectionNotEstablished);
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::postStatusUpdateNotification(EDatabaseWrapperObjectStatus eStatus)
{
	if (m_hUIWindow)
	{
		::PostMessage(m_hUIWindow, FP_DB_STATUS_UPDATE, (WPARAM) eStatus, NULL);
	}
}
//--------------------------------------------------------------------------------------------------
set<long> CFileProcessingDB::getSkippedFilesForAction(const _ConnectionPtr& ipConnection,
													  long nActionId)
{
	try
	{
		string strQuery = "SELECT FileID FROM SkippedFile WHERE ActionID = @ActionID";
		auto cmd = buildCmd(ipConnection, strQuery, { {"@ActionID", nActionId} });

		_RecordsetPtr ipFileSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI30293", ipFileSet != __nullptr);

		// Open the file set
		ipFileSet->Open((IDispatch*)cmd, vtMissing, adOpenForwardOnly, adLockReadOnly, adCmdText);

		set<long> setFileIds;
		while (ipFileSet->adoEOF == VARIANT_FALSE)
		{
			setFileIds.insert(getLongField(ipFileSet->Fields, "FileID"));
			ipFileSet->MoveNext();
		}

		return setFileIds;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30294");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::setFileActionState(_ConnectionPtr ipConnection,
										   const vector<SetFileActionData>& vecSetData,
										   string strAction, const string& strState)
{
	try
	{
		// Get the ActionID
		long nActionID = getActionID(ipConnection, strAction);
		string strActionID = asString(nActionID);
		long nFAMUser = getFAMUserID(ipConnection);
		long nMachine = getMachineID(ipConnection);
		EActionStatus eaTo = asEActionStatus(strState);

		string strActionIDs = getActionIDsForActiveWorkflow(ipConnection, strAction);

		// Execute the queries in groups of 1000 File IDs
		size_t i = 0;
		size_t count = vecSetData.size();
		while (i < count)
		{
			string strFileIdList;
			for (int j = 0; i < count && j < 1000; j++)
			{
				const SetFileActionData& data = vecSetData[i++];
				if (!strFileIdList.empty())
				{
					strFileIdList += ", ";
				}
				strFileIdList += asString(data.FileID);

				// Update the stats				
				updateStats(ipConnection, nActionID, data.FromStatus, eaTo, data.FileRecord,
					data.FileRecord, data.IsFileDeleted);
			}

			// Build main queries
			_CommandPtr cmdUpdateFAS;
			_CommandPtr cmdDeleteFromFAS;
			_CommandPtr cmdInsertIntoFAS;
			map<string, _variant_t> params =
			{
				{"@ActionID", nActionID},
				{"@State", strState.c_str()},
				{"@FAMUserID", getFAMUserID(ipConnection)},
				{"@UserName", ((m_strFAMUserName.empty()) ? getCurrentUserName() : m_strFAMUserName).c_str()},
				{"@MachineID", getMachineID(ipConnection)},
				{"@|<VT_INT>ActionIDs|", strActionIDs.c_str()},
				{"@|<VT_INT>FileIDs|", strFileIdList.c_str()}
			};

			if  (eaTo == kActionUnattempted)
			{
				cmdDeleteFromFAS = buildCmd(ipConnection,
					"DELETE FROM FileActionStatus WHERE ActionID = @ActionID"
					" AND FileID IN (@|<VT_INT>FileIDs|)", params);
			}
			else
			{
				cmdUpdateFAS = buildCmd(ipConnection,
					"UPDATE FileActionStatus SET ActionStatus = @State"
					" WHERE ActionID = @ActionID AND FileID IN (@|<VT_INT>FileIDs|)", params);
				cmdInsertIntoFAS = buildCmd(ipConnection,
					"INSERT INTO FileActionStatus (FileID, ActionID, ActionStatus, Priority) "
					"SELECT FAMFile.ID, @ActionID AS ActionID, @State AS ActionStatus, "
					"COALESCE(FileActionStatus.Priority, FAMFile.Priority) AS Priority FROM FAMFile WITH (NOLOCK) "
					"LEFT JOIN FileActionStatus ON FAMFile.ID = FileActionStatus.FileID "
					"AND ActionID = @ActionID WHERE ActionID IS NULL AND FAMFile.ID IN (@|<VT_INT>FileIDs|)", params);
			}
			_CommandPtr cmdDeleteLockedFile = buildCmd(ipConnection, 
				"DELETE FROM LockedFile WHERE ActionID = @ActionID "
				" AND FileID IN (@|<VT_INT>FileIDs|)", params);
			_CommandPtr cmdRemoveSkippedFile = buildCmd(ipConnection, 
				"DELETE FROM SkippedFile WHERE ActionID = @ActionID"
				" AND FileID IN (@|<VT_INT>FileIDs|)", params);
			// There are no cases where this method should not just ignore all pending entries in
			// [QueuedActionStatusChange] for the selected files.
			_CommandPtr cmdUpdateQueuedActionStatusChange = buildCmd(ipConnection,
				"UPDATE [QueuedActionStatusChange] SET [ChangeStatus] = 'I'"
				"WHERE [ChangeStatus] = 'P' AND [ActionID] = @ActionID AND FileID IN (@|<VT_INT>FileIDs|)", params);
			_CommandPtr cmdFastQuery = buildCmd(ipConnection,
				"INSERT INTO " + gstrFILE_ACTION_STATE_TRANSITION
				+ " (FileID, ActionID, ASC_From, ASC_To, DateTimeStamp, FAMUserID, MachineID "
				+ ") SELECT FAMFile.ID, @ActionID AS ActionID, "
				+ "COALESCE(ActionStatus, 'U') AS ASC_From, @State AS ASC_To, "
				+ "GETDATE() AS DateTimeStamp, @FAMUserID AS FAMUserID, @MachineID AS MachineID "
				+ "FROM FAMFile WITH (NOLOCK) "
				+ "LEFT JOIN FileActionStatus ON FAMFile.ID = FileActionStatus.FileID  AND "
				+ "FileActionStatus.ActionID = @ActionID "
				+ "WHERE FAMFile.ID IN (@|<VT_INT>FileIDs|)", params);
			_CommandPtr cmdClearComments = m_bAutoDeleteFileActionComment && strState == "C"
				? buildCmd(ipConnection,
					"DELETE FROM FileActionComment WHERE ActionID = @ActionID AND FileID IN(@|<VT_INT>FileIDs|)", params)
				: __nullptr;
			_CommandPtr cmdAddSkipRecord = strState == "S"
				? buildCmd(ipConnection,
					"INSERT INTO SkippedFile (UserName, FileID, ActionID) SELECT @UserName AS UserName, FAMFile.ID, "
					"@ActionID AS ActionID FROM FAMFile WITH (NOLOCK) WHERE FAMFile.ID IN (@|<VT_INT>FileIDs|)", params)
				: __nullptr;

			// This is used when processing state changes to "U", "C", "F" and if restartable processing
			// is turned off "P"
			_CommandPtr cmdDeleteWorkItemGroup = buildCmd(ipConnection, 
				"DELETE FROM WorkItemGroup WHERE ActionID IN (@|<VT_INT>ActionIDs|)"
				" AND FileID IN (@|<VT_INT>FileIDs|)", params);

			// Execute the queries (execute the FAMFile update last)
			if (m_bUpdateFASTTable)
			{
				executeCmd(cmdFastQuery);
			}
			executeCmd(cmdDeleteLockedFile);
			executeCmd(cmdRemoveSkippedFile);
			executeCmd(cmdUpdateQueuedActionStatusChange);
			if ((!m_bAllowRestartableProcessing && strState == "P") || strState == "U" 
                || strState == "C" || strState == "F")
			{
				executeCmd(cmdDeleteWorkItemGroup);
			}

			if (cmdClearComments != __nullptr)
			{
				executeCmd(cmdClearComments);
			}
			if (cmdAddSkipRecord != __nullptr)
			{
				executeCmd(cmdAddSkipRecord);
			}
			if (eaTo == kActionUnattempted)
			{
				executeCmd(cmdDeleteFromFAS);
			}
			else
			{
				executeCmd(cmdUpdateFAS);
				executeCmd(cmdInsertIntoFAS);
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30295");
}
//--------------------------------------------------------------------------------------------------
EActionStatus CFileProcessingDB::setFileActionState(_ConnectionPtr ipConnection, long nFileID, 
													string strAction, long nWorkflowID,
													const string& strState,
													const string& strException,
													bool bQueueChangeIfProcessing,
													bool bAllowQueuedStatusOverride,
													long nForUserID, long nActionID,
													bool bRemovePreviousSkipped,
													const string& strFASTComment,
													bool bThisIsRevertingStuckFile)
{
	INIT_EXCEPTION_AND_TRACING("MLI03279");

	try
	{
		ASSERT_ARGUMENT("ELI30390", ipConnection != __nullptr);
		ASSERT_ARGUMENT("ELI30391", !strAction.empty() || nActionID != -1);

		// Per [LegacyRCAndUtils:6350], ensure the isolation level here is high enough so that
		// other threads/processes don't modify records related to the file action state in the
		// midst of this call.
		if (ipConnection->IsolationLevel < adXactRepeatableRead)
		{
			UCLIDException ue("ELI35103",
				"Database connection does not have sufficient isolation level.");
			ue.addDebugInfo("IsolationLevel", ipConnection->IsolationLevel);
			throw ue;
		}

		_lastCodePos = "10";
		EActionStatus easRtn = kActionUnattempted;

		// If the Action ID was specified but the Workflow ID wasn't then get the WorkflowID from the Action ID
		if (nWorkflowID <= 0 && nActionID > 0)
		{
			nWorkflowID = getWorkflowID(ipConnection, nActionID);
		}
		// Else get the active Workflow ID if there is one so that the join to the workflow file table will work correctly
		else if (nWorkflowID <= 0)
		{
			nWorkflowID = getActiveWorkflowID(ipConnection);
		}

		// Update action ID/Action name
		if (!strAction.empty() && nActionID == -1)
		{	
			_lastCodePos = "30";
			nActionID = getActionID(ipConnection, strAction, nWorkflowID);
		}
		else if (strAction.empty() && nActionID != -1)
		{
			_lastCodePos = "40";
			strAction = getActionName(ipConnection, nActionID);
		}
		_lastCodePos = "50";

		// Set up the select query to select the file to change and include and skipped file data
		// If there is no skipped file record the SkippedActionID will be -1
		_CommandPtr cmdGetFileState = 
			buildCmd(ipConnection,
				"SELECT FAMFile.ID as ID, FileName, FileSize, Pages, [FAMFile].Priority, "
				"COALESCE(ActionStatus, 'U') AS ActionStatus, "
				"COALESCE(SkippedFile.ActionID, -1) AS SkippedActionID, "
				"COALESCE(QueuedActionStatusChange.ID, -1) AS QueuedStatusChangeID, "
				"COALESCE(~WorkflowFile.Invisible, -1) AS IsFileInWorkflow "
			"FROM FAMFile  "
				"LEFT OUTER JOIN SkippedFile ON SkippedFile.FileID = FAMFile.ID " 
				"	AND SkippedFile.ActionID = @ActionID "
				"LEFT OUTER JOIN FileActionStatus WITH (ROWLOCK, UPDLOCK) ON FileActionStatus.FileID = FAMFile.ID "
				"	AND FileActionStatus.ActionID = @ActionID "
				" LEFT OUTER JOIN QueuedActionStatusChange WITH (ROWLOCK, UPDLOCK) ON QueuedActionStatusChange.ChangeStatus = @ChangeStatus "
				"	AND QueuedActionStatusChange.FileID = FAMFile.ID "
				"	AND QueuedActionStatusChange.ActionID = @ActionID "
				" LEFT OUTER JOIN WorkflowFile ON WorkflowFile.FileID = FAMFile.ID AND WorkflowFile.WorkflowID = @WorkflowID "
				" WHERE FAMFile.ID = @FileID",
			{
				{"@ChangeStatus", "P"},
				{"@ActionID", nActionID},
				{"@WorkflowID", nWorkflowID},
				{"@FileID", nFileID}
			});
		
		_lastCodePos = "60";

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();
		_lastCodePos = "70";

		_RecordsetPtr ipFileSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI30392", ipFileSet != __nullptr);
		_lastCodePos = "80";

		ipFileSet->Open((IDispatch *)cmdGetFileState, vtMissing,
			adOpenStatic, adLockReadOnly, adCmdText);

		_lastCodePos = "90";

		// Find the file if it exists
		if (ipFileSet->adoEOF == VARIANT_FALSE)
		{
			_lastCodePos = "120";
			FieldsPtr ipFileSetFields = ipFileSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI30393", ipFileSetFields != __nullptr);

			_lastCodePos = "130";
			// Get the previous state
			string strPrevStatus = getStringField(ipFileSetFields, "ActionStatus"); 
			_lastCodePos = "140";

			easRtn = asEActionStatus (strPrevStatus);
			_lastCodePos = "150";

			// Account for any entries in QueuedActionStatusChange for this file (whether that means
			// applying the queued change or ignoring it).
			string strNewState = strState;
			string strNewFASTComment = strFASTComment;

			long nQueuedStatusChangeID = -1;

			// If the status change is to be queued when the file is currently processing and the
			// file is currently processing, update the QueuedActionStatusChange table and return.
			if (bQueueChangeIfProcessing && strPrevStatus == "R" && strNewState != "R")
			{
				// Any previous pending entry in the QueuedActionStatusChange table for
				// this file and status should be set to "O" to indicate the previously
				// queued change has been overridden by this one.
				executeCmd(buildCmd(ipConnection,
						"UPDATE [QueuedActionStatusChange] "
						"SET [ChangeStatus] = @NewStatus, [TargetUserID] = @ForUserID "
						" WHERE [ChangeStatus] = @OldStatus AND [ActionID] = @ActionID AND [FileID] = @FileID",
					{
							{"@NewStatus", "O" },
							{"@ForUserID", (nForUserID < 0) ? vtMissing : nForUserID },
							{"@OldStatus", "P" },
							{"@ActionID", nActionID},
							{"@FileID", nFileID}
					}));

				// Add a new QueuedActionStatusChange entry to queue this change.
				executeCmd(buildCmd(ipConnection,
					"INSERT INTO [QueuedActionStatusChange]"
					" (FileID, ActionID, ASC_To, TargetUserID, DateTimeStamp, MachineID, FAMUserID, FAMSessionID, ChangeStatus)"
					" VALUES (@FileID, @ActionID, @State, @TargetUserID, GETDATE(), @MachineID, @UserID, @FAMSessionID, @ChangeStatus)",
					{
						{ "@FileID", nFileID },
						{ "@ActionID", nActionID},
						{ "@State", strNewState.c_str() },
						{ "@TargetUserID", (nForUserID < 0) ? vtMissing : nForUserID },
						{ "@MachineID", getMachineID(ipConnection) },
						{ "@UserID", getFAMUserID(ipConnection) },
						{ "@FAMSessionID", (m_nFAMSessionID == 0) ? vtMissing : m_nFAMSessionID },
						{ "@ChangeStatus", "P" }
					}));

				return easRtn;
			}
			else
			{
				nQueuedStatusChangeID = getLongField(ipFileSetFields, "QueuedStatusChangeID");

				_lastCodePos = "151";

				if (nQueuedStatusChangeID >= 0)
				{
					// Open the relevant record in QueuedActionStatusChange.
					_lastCodePos = "152";
					_RecordsetPtr ipQueuedChangeSet(__uuidof(Recordset));
					ASSERT_RESOURCE_ALLOCATION("ELI34184", ipQueuedChangeSet != __nullptr);

					_CommandPtr cmdQueuedActionStatusChange = buildCmd(ipConnection,
						"SELECT [ASC_To], [ChangeStatus], [TargetUserID] FROM [QueuedActionStatusChange] WHERE [ID] = @QueuedStatusChangeID",
						{ { "@QueuedStatusChangeID", nQueuedStatusChangeID} });

					ipQueuedChangeSet->Open((IDispatch *)cmdQueuedActionStatusChange, vtMissing,
						adOpenDynamic, adLockOptimistic, adCmdText);

					_lastCodePos = "153";

					if (bAllowQueuedStatusOverride)
					{
						_lastCodePos = "154";
						string strOverrideState = getStringField(ipQueuedChangeSet->Fields, "ASC_To");

						// If overrides from the QueuedChange table are allowed, update the target state
						// and apply a comment that notes the override.
						if (strOverrideState != strState)
						{
							strNewState = strOverrideState;
							strNewFASTComment = "Transition to " + strState + " overridden";
							nForUserID = getLongField(ipQueuedChangeSet->Fields, "TargetUserID", -1);
						}

						_lastCodePos = "155";
						setStringField(ipQueuedChangeSet->Fields, "ChangeStatus", "C");
						ipQueuedChangeSet->Update();
					}
					else
					{
						// If overrides from the QueuedChange table are not allowed, mark the
						// QueuedChange record as ignored.
						nQueuedStatusChangeID = -1;
					
						_lastCodePos = "156";
					}

					// While there should only ever be one pending row in QueuedActionStatusChange for
					// each FileID/ActionID pair, this call ensures sure that all pending changes for
					// this file are reset here (whether or not the change was applied.
					executeCmd(buildCmd(ipConnection,
						"UPDATE [QueuedActionStatusChange] SET [ChangeStatus] = @NewStatus "
						" WHERE [ChangeStatus] = @OldStatus AND [ActionID] = @ActionID AND [FileID] = @FileID",
						{
							{"@NewStatus", "I" },
							{"@OldStatus", "P" },
							{"@ActionID", nActionID},
							{"@FileID", nFileID}
						}));

					_lastCodePos = "157";
				}
			}

			// Get the current record
			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipCurrRecord;
			ipCurrRecord = getFileRecordFromFields(ipFileSetFields);
			_lastCodePos = "160";

			// Get the skipped ActionID
			long nSkippedActionID = getLongField(ipFileSetFields, "SkippedActionID");
			_lastCodePos = "170";

			// Get whether the file is in the workflow and if it is invisible (deleted via the Document Web API)
			long nIsFileInWorkflow = getLongField(ipFileSetFields, "IsFileInWorkflow");

			// Update the FileActionStatus table appropriately
			if (easRtn != kActionUnattempted && strNewState != "U")
			{
				// update an existing record
				executeCmd(buildCmd(ipConnection, "UPDATE [FileActionStatus] "
					"SET [ActionStatus] = @State, [UserID] = @UserID "
					"WHERE [FileID] = @FileID AND [ActionID] = @ActionID",
					{
						{ "@State", strNewState.c_str() },
						{ "@UserID", (nForUserID < 0) ? vtMissing : nForUserID },
						{ "@ActionID", nActionID },
						{ "@FileID", nFileID }
					}));
			}

			// if the new state is unattempted there should be no record in the FileActionStatus table
			// for the file id and action id
			if (strNewState == "U")
			{
				// delete any record for file id and action id in the FileActionStatus table
				executeCmd(buildCmd(ipConnection,
					"DELETE FROM FileActionStatus WHERE FileID = @FileID AND ActionID = @ActionID",
					{
						{ "@ActionID", nActionID },
						{ "@FileID", nFileID }
					}));
			}
			
			// if the old state is unattempted and the new state is not need to add record to FileActionStatus table
			if (easRtn == kActionUnattempted && strNewState != "U")
			{
				executeCmd(buildCmd(ipConnection,
					"INSERT INTO FileActionStatus (FileID, ActionID, ActionStatus, Priority, UserID) "
					"VALUES(@FileID, @ActionID, @State, @Priority, @UserID)",
					{
						{ "@ActionID", nActionID },
						{ "@FileID", nFileID },
						{ "@State", strNewState.c_str() },
						{ "@Priority", ipCurrRecord->Priority },
						{ "@UserID", (nForUserID < 0) ? vtMissing : nForUserID }
					}));

				// If the workflow is known but the file is not registered in the workflow table
				// then record the file as having been active in this workflow.
				if (nWorkflowID > 0 && nIsFileInWorkflow == -1)
				{
					executeCmd(buildCmd(ipConnection,
						"IF NOT EXISTS ( \r\n"
						"	SELECT * FROM [WorkflowFile] WHERE [WorkflowID] = @WorkflowID AND [FileID] = @FileID) \r\n"
						"BEGIN \r\n"
						"	INSERT INTO [WorkflowFile] ([WorkflowID], [FileID]) VALUES (@WorkflowID, @FileID) \r\n"
						"END",
						{
							{ "@WorkflowID", nWorkflowID },
							{ "@FileID", nFileID },
						}));
				}
			}

			_lastCodePos = "180";

			// If transition to complete and AutoDeleteFileActionComment == true
			// then clear the file action comment for this file
			if (strNewState == "C" && m_bAutoDeleteFileActionComment)
			{
				_lastCodePos = "190";
				clearFileActionComment(ipConnection, nFileID, nActionID);
			}
			_lastCodePos = "200";

			// if the old status does not equal the new status add transition records
			if (strPrevStatus != strNewState || bRemovePreviousSkipped)
			{
				_lastCodePos = "210";
				// update the statistics
				EActionStatus easStatsFrom = easRtn;
				if (easRtn == kActionProcessing)
				{
					_lastCodePos = "220";

					// Remove record from the LockedFileTable
					executeCmd(buildCmd(ipConnection,
							"DELETE FROM [LockedFile] "
							"	WHERE [FileID] = @FileID "
							"	AND [ActionID] = @ActionID "
							"	AND [ActiveFAMID] = @ActiveFAMID",
						{
							{ "@ActionID", nActionID },
							{ "@FileID", nFileID },
							{ "@ActiveFAMID", m_nActiveFAMID}
						}));
				}
				_lastCodePos = "250";

				updateStats(ipConnection, nActionID, easStatsFrom, asEActionStatus(strNewState),
					ipCurrRecord, ipCurrRecord, nIsFileInWorkflow == 0); // 0 = Deleted/Invisible

				_lastCodePos = "260";
				// Only update FileActionStateTransition table if required
				if (m_bUpdateFASTTable)
				{
					_lastCodePos = "270";
					addFileActionStateTransition(ipConnection, nFileID, nActionID, strPrevStatus, 
						strNewState, strException, strNewFASTComment, nQueuedStatusChangeID);
				}
				_lastCodePos = "280";

				// Determine if existing skipped record should be removed
				bool bSkippedRemoved = nSkippedActionID != -1 && (bRemovePreviousSkipped || strNewState != "S");

				// These calls are order dependent.
				// Remove the skipped record (if any) and add a new
				// skipped file record if the new state is skipped
				if (bSkippedRemoved)
				{
					_lastCodePos = "290";
					removeSkipFileRecord(ipConnection, nFileID, nActionID);
				}
				_lastCodePos = "300";

				if (strNewState == "S")
				{
					// If this is a stuck-in-processing file then don't take over the session (nor throw an exception if there is no current session)
					if (bThisIsRevertingStuckFile)
					{
					}
					else if (nSkippedActionID == -1 || bSkippedRemoved)
					{
						_lastCodePos = "310";

						// Add a record to the skipped table
						addSkipFileRecord(ipConnection, nFileID, nActionID);
					}
					else 
					{
						_lastCodePos = "320";

						if (m_nFAMSessionID == 0)
						{
							throw UCLIDException("ELI38468",
								"Cannot skip a file outside of a FAM session.");
						}

						// Update the FAMSessionID to current process so it will be not be selected
						// again as a skipped file for the current process
						// Also update the time stamp and the UserName (since the user
						// could be processing all files skipped by any user)
						// [LRCAU #5853]
						executeCmd(buildCmd(ipConnection,
							"UPDATE SkippedFile "
							"	SET FAMSessionID = @FAMSessionID, DateTimeStamp = GETDATE(), UserName = @UserName "
							"	WHERE FileID = @FileID",
							{
								{ "@FAMSessionID", m_nFAMSessionID },
								{ "@FileID", nFileID },
								{ "@UserName", ((m_strFAMUserName.empty()) ? getCurrentUserName() : m_strFAMUserName).c_str() }
							}));
					}
				}
			}
			_lastCodePos = "330";


			// Remove WorkItemGroups when file status changes -- if processing will be allowed to
			// stop before all work items complete then this will need to be changed to 
			// only delete if new status is 'U', 'C' or 'F'
			if ((!m_bAllowRestartableProcessing && strNewState == "P") || strNewState == "U" || strNewState == "C" || strNewState == "F")
			{
				long nActionID = getActionIDNoThrow(ipConnection, strAction, nWorkflowID);
				// ActionID may be -1 if setting an action that exists in nWorkflowID but not this workflow.
				if (nActionID > 0)
				{
					executeCmd(buildCmd(ipConnection,
						"DELETE FROM WorkItemGroup WHERE FileID = @FileID AND ActionID = @ActionID",
						{
							{ "@FileID", nFileID },
							{ "@ActionID", nActionID }
						}));
				}
			}

			_lastCodePos = "340";

			// If Restartable processing is enabled and the new state will be pending update the 
			// WorkItemGroup record (if there is one) to not have a FAMSessionID to indicate that
			// the file has been split into work items but the file is not currently processing in
			// any FAMs
			if (m_bAllowRestartableProcessing && strNewState == "P")
			{
				executeCmd(buildCmd(ipConnection,
					"UPDATE WorkItemGroup SET FAMSessionID = NULL WHERE FileID = @FileID",
					{ { "@FileID", nFileID } }));
			}

			_lastCodePos = "345";
		}
		else
		{
			_lastCodePos = "360";

			// No file with the given id
			UCLIDException ue("ELI30394", "File ID was not found.");
			ue.addDebugInfo ("File ID", nFileID);
			throw ue;
		}
		_lastCodePos = "370";

		return easRtn;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30395");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::setStatusForFile(_ConnectionPtr ipConnection, long nFileID, string strAction,
	long nWorkflowID, long nForUserID, EActionStatus eStatus, bool bQueueChangeIfProcessing,
	bool bAllowQueuedStatusOverride, EActionStatus *poldStatus)
{
	try
	{
		// Change the status for the given file and return the previous state
		EActionStatus oldStatus = setFileActionState(ipConnection, nFileID, strAction, nWorkflowID,
			asStatusString(eStatus), "", bQueueChangeIfProcessing, bAllowQueuedStatusOverride, nForUserID);

		if (poldStatus != __nullptr)
		{
			*poldStatus = oldStatus;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI35102");
}
//--------------------------------------------------------------------------------------------------
EActionStatus CFileProcessingDB::asEActionStatus  (const string& strStatus)
{
	EActionStatus easRtn;

	if (strStatus == "P")
	{
		easRtn = kActionPending;
	}
	else if (strStatus == "R")
	{
		easRtn = kActionProcessing;
	}
	else if (strStatus == "F")
	{
		easRtn = kActionFailed;
	}
	else if (strStatus == "C")
	{
		easRtn = kActionCompleted;
	}
	else if (strStatus == "U")
	{
		easRtn = kActionUnattempted;
	}
	else if (strStatus == "S")
	{
		easRtn = kActionSkipped;
	}
	else
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI13552");
	}
	return easRtn;
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::asStatusString(EActionStatus eStatus)
{
	switch (eStatus)
	{
	case kActionUnattempted:
		return "U";
	case kActionPending:
		return "P";
	case kActionProcessing:
		return "R";
	case kActionCompleted:
		return "C";
	case kActionFailed:
		return "F";
	case kActionSkipped:
		return "S";
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI13562");
	}
	return "U";
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::asStatusName(const string& strStatus)
{
	if (strStatus.length() != 1)
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI29623");
	}

	switch (strStatus[0])
	{
		case 'U':	return "Unattempted";
		case 'P':	return "Pending";
		case 'R':	return "Processing";
		case 'C':	return "Completed";
		case 'F':	return "Failed";
		case 'S':	return "Skipped";

		default: THROW_LOGIC_ERROR_EXCEPTION("ELI29625");
	}
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::asStatusName(EActionStatus eStatus)
{
	switch (eStatus)
	{
		case kActionUnattempted:	return "Unattempted";
		case kActionPending:		return "Pending";
		case kActionProcessing:		return "Processing";
		case kActionCompleted:		return "Completed";
		case kActionFailed:			return "Failed";
		case kActionSkipped:		return "Skipped";

		default: THROW_LOGIC_ERROR_EXCEPTION("ELI35690");
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addQueueEventRecord(_ConnectionPtr ipConnection, long nFileID, 
											long nActionID, string strFileName, 
											string strQueueEventCode, long long llFileSize/* =-1*/)
{
	try
	{
		INIT_EXCEPTION_AND_TRACING("MLI02855");
		try
		{
			// Check if QueueEvent Table should be updated
			if (!m_bUpdateQueueEventTable)
			{
				return;
			}
			_lastCodePos = "10";

			variant_t vtFileModifyTime;
			vtFileModifyTime.vt = VT_NULL;
			long long llFileSize = 0;
			// File should exist for these options
			if (strQueueEventCode == "A" || strQueueEventCode == "M")
			{
				_lastCodePos = "80_10";

				// if adding or modifying the file add the file modified and file size fields
				CTime fileTime;
				fileTime = getFileModificationTimeStamp(strFileName);
				_lastCodePos = "80_20";

				vtFileModifyTime = fileTime.Format("%m/%d/%y %I:%M:%S %p");

				// Get the file size
				llFileSize = getSizeOfFile(strFileName);
				_lastCodePos = "80_50";
			}
			variant_t vtAction;
			if (nActionID < 0)
				vtAction.vt = VT_NULL;
			else
				vtAction = nActionID;

			executeCmd(buildCmd(ipConnection,
				"INSERT INTO [QueueEvent] (FileID, ActionID, DateTimeStamp, QueueEventCode, FAMUserID, MachineID, FileModifyTime, FileSizeInBytes) "
				" VALUES (@FileID, @ActionID, GETDATE(), @QueueEventCode, @FAMUserID, @MachineID, @FileModifyTime, @FileSizeInBytes)",
				{
					{ "@FileID", nFileID },
					{ "@ActionID", vtAction },
					{ "@QueueEventCode", strQueueEventCode.c_str() },
					{ "@FAMUserID", getFAMUserID(ipConnection) },
					{ "@MachineID", getMachineID(ipConnection) },
					{ "@FileModifyTime", vtFileModifyTime },
					{ "@FileSizeInBytes", llFileSize }
				}));
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25376");
	}
	catch(UCLIDException& uex)
	{
		uex.addDebugInfo("File ID", nFileID);
		uex.addDebugInfo("File To Add", strFileName);
		uex.addDebugInfo("Queue Event Code", strQueueEventCode);
		if (ipConnection == __nullptr)
		{
			uex.addDebugInfo("ConnectionValue", "NULL");
		}

		throw uex;
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addFileActionStateTransition(_ConnectionPtr ipConnection,
													  long nFileID, long nActionID, 
													  const string &strFromState, 
													  const string &strToState, 
													  const string &strException, 
													  const string &strComment,
													  long nQueuedActionStatusChangeID/* = -1*/)
{
	string strInsertQuery = "";
	try
	{
		try
		{
			// check if updates to FileActionStateTransition table are required
			if (!m_bUpdateFASTTable || strToState == strFromState)
			{
				// nothing to do
				return;
			}

			executeCmd(buildCmd(ipConnection,
				"INSERT INTO [FileActionStateTransition]"
				" (FileID, ActionID, ASC_From, ASC_To, DateTimeStamp, FAMUserID, MachineID, Exception, Comment, QueueID)"
				" VALUES"
				" (@FileID, @ActionID, @From, @To, GETDATE(), @FAMUserID, @MachineID, @Exception, @Comment, @QueueID)",
				{
					{ "@FileID", nFileID },
					{ "@ActionID", nActionID },
					{ "@From", strFromState.c_str() },
					{ "@To", strToState.c_str() },
					{ "@FAMUserID", getFAMUserID(ipConnection) },
					{ "@MachineID", getMachineID(ipConnection) },
					{ "@Exception", (strException.empty() ? vtMissing : strException.c_str()) },
					{ "@Comment", (strComment.empty() ? vtMissing : strComment.c_str()) },
					{ "@QueueID", ((nQueuedActionStatusChangeID < 0) ? vtMissing : nQueuedActionStatusChangeID) }
				}));
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28236");
	}
	catch(UCLIDException& ue)
	{
		ue.addDebugInfo("SQL Query", strInsertQuery);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getFileID(_ConnectionPtr ipConnection, string& rstrFileName)
{
	try
	{
		return getKeyID(ipConnection, gstrFAM_FILE, "FileName", rstrFileName, false);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26720");
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getActiveWorkflowID(_ConnectionPtr ipConnection)
{
	if (m_nActiveWorkflowID != 0)
	{
		return m_nActiveWorkflowID;
	}
	else
	{
		string strWorkflow = getActiveWorkflow();

		if (strWorkflow.empty())
		{
			// Using <All workflows>
			m_nActiveWorkflowID = -1;
		}
		else
		{
			// CAUTION: Call getWorkflowID() only when strWorkflow is not empty to avoid circular reference.
			m_nActiveWorkflowID = getWorkflowID(ipConnection, strWorkflow);
		}

		return m_nActiveWorkflowID;
	}
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getWorkflowID(_ConnectionPtr ipConnection, string strWorkflowName)
{
	try
	{
		if (strWorkflowName.empty())
		{
			// CAUTION: Call getWorkflowID() only when strWorkflowName is empty to avoid circular reference.
			return getActiveWorkflowID(ipConnection);
		}

			long nWorkflowID = 0;

		getCmdId(buildCmd(ipConnection,
			"SELECT [ID] FROM [Workflow] WHERE [Workflow].[Name] = @WorkflowName",
			{ { "@WorkflowName", strWorkflowName.c_str() } })
			, &nWorkflowID);

		return nWorkflowID;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26721");
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getWorkflowID(_ConnectionPtr ipConnection, long nActionID)
{
	try
	{
		string strQuery = "SELECT [WorkflowID] FROM [Action] WHERE [ID] = @ActionID";
		auto cmd = buildCmd(ipConnection, strQuery, { {"@ActionID", nActionID} });

		_RecordsetPtr ipActionSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI42144", ipActionSet != __nullptr);
		
		ipActionSet->Open((IDispatch*) cmd, vtMissing,
			adOpenStatic, adLockOptimistic, adCmdText);

		if (ipActionSet->adoEOF == VARIANT_FALSE)
		{
			FieldsPtr ipFields = ipActionSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI42145", ipFields != __nullptr);

			if (!isNULL(ipFields, "WorkflowID"))
			{
				return getLongField(ipFields, "WorkflowID");
			}
		}

		return -1;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42103");
}
//--------------------------------------------------------------------------------------------------
int CFileProcessingDB::isFileInWorkflow(_ConnectionPtr ipConnection, long nFileID, long nWorkflowID)
{
	try
	{
		if (nWorkflowID <= 0)
		{
			nWorkflowID = getWorkflowID(ipConnection, getActiveWorkflow());
		}

		long nResult = -1;
		getCmdId(buildCmd(ipConnection, (nWorkflowID > 0)
			// Use ~ operator to invert invisible flag as we want 1 to indicate a file in the workflow
			// and 0 for a file that has been marked deleted (invisible).
			// Add + 0 to convert to int since bit fields are not allowed in aggregate functions
			? "SELECT COALESCE(MAX(~[Invisible]+0), -1) AS [ID] FROM [WorkflowFile] "
				"WHERE [FileID] = @FileID AND [WorkflowID] = @WorkflowID"
			: "SELECT COALESCE(MAX(1), -1) AS [ID] FROM [FAMFile] WHERE [ID] = @FileID",
			{
				{"@FileID", nFileID},
				{"@WorkflowID", nWorkflowID}
			})
			, &nResult);

		return nResult;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43218");
}
//--------------------------------------------------------------------------------------------------
int CFileProcessingDB::isFileInWorkflow(_ConnectionPtr ipConnection, string strFileName,
										 long nWorkflowID)
{
	try
	{
		if (nWorkflowID <= 0)
		{
			nWorkflowID = getWorkflowID(ipConnection, getActiveWorkflow());
		}


		string strQuery;
		map<string, variant_t> vecParams;
		vecParams["@FileName"] = strFileName.c_str();
		if (nWorkflowID > 0)
		{
			vecParams["@WorkflowID"] = nWorkflowID;
			// Use ~ operator to invert deleted flag as we want 1 to indicate a file in the workflow
			// and 0 for a file that has been marked deleted (invisible).
			// Add + 0 to convert to int since bit fields are not allowed in aggregate functions
			strQuery = "SELECT COALESCE(MAX(~[Invisible]+0), -1) AS [ID] FROM [WorkflowFile] "
				"INNER JOIN [FAMFile] ON [FileID] = [FAMFile].[ID] "
				"WHERE [FileName] = @FileName AND [WorkflowID] = @WorkflowID";
		}
		else
		{
			strQuery = "SELECT COALESCE(MAX(1), -1) AS [ID] FROM [FAMFile] WHERE [FileName] = @FileName";
		}


		long nResult = -1;
		getCmdId(buildCmd(ipConnection, strQuery, vecParams), &nResult);

		return nResult;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI44849");
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::getActiveWorkflow()
{
	CSingleLock lock(&m_criticalSection, TRUE);

	return m_strActiveWorkflow;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::setActiveWorkflow(string strWorkflowName)
{
	CSingleLock lock(&m_criticalSection, TRUE);

	if (m_strActiveWorkflow == strWorkflowName)
	{
		// https://extract.atlassian.net/browse/ISSUE-14766
		// If the specified workflow is the same as the current workflow, do nothing.
		// This should prevent any possible code paths that do a "refresh" of current DB settings
		// from triggering assertion ELI42030.
		return;
	}

	if (m_nFAMSessionID != 0)
	{
		throw UCLIDException("ELI42030", "Cannot set workflow while a session is open.");
	}

	m_strActiveWorkflow = strWorkflowName;
	ms_strLastWorkflow = strWorkflowName;

	// Clear cached action IDs
	m_mapActionIdsForActiveWorkflow.clear();
	// Zero indicates the ID needs to be looked up next time the ID is requested.
	// -1 indicates there is no active workflow.
	m_nActiveWorkflowID = 0;
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getActionID(_ConnectionPtr ipConnection, const string& strActionName)
{
	try
	{
		return getActionID(ipConnection, strActionName, getActiveWorkflow());
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42035");
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getActionID(_ConnectionPtr ipConnection, const string& strActionName,
									long nWorkflowID)
{
	try
	{
		if (nWorkflowID <= 0)
		{
			return getActionID(ipConnection, strActionName, getActiveWorkflow());
		}
		else
		{
			long nActionID = -1;
			getCmdId(buildCmd(ipConnection,
				"SELECT COALESCE(MAX([ID]), -1) AS[ID] FROM[Action] WHERE[ASCName] = @ActionName AND[WorkflowID] = @WorkflowID",
				{
					{ "@ActionName", strActionName.c_str() },
					{ "@WorkflowID", nWorkflowID }
				})
				, &nActionID);

			if (nActionID < 0)
			{
				UCLIDException ue("ELI43444", Util::Format(
					"Action \"%s\" does not exist in workflow.",
					strActionName.c_str()));
				ue.addDebugInfo("WorkflowID", nWorkflowID, false);
				throw ue;
			}

			return nActionID;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42146");
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getActionID(_ConnectionPtr ipConnection, const string& strActionName, const string& strWorkflow)
{
	try
	{
		long nActionID = -1;
		getCmdId(buildCmd(ipConnection, string(
			"SELECT COALESCE(MAX([Action].[ID]), -1) AS [ID] FROM [Action] "
			"	LEFT JOIN [WorkFlow] ON [WorkflowID] = [Workflow].[ID]"
			"	WHERE [ASCName] = @ActionName AND [Workflow].[Name] ") +
			+ (strWorkflow.empty() ? "IS NULL" : "= @WorkflowName")
			,{
				{ "@ActionName", strActionName.c_str() },
				{ "@WorkflowName", strWorkflow.c_str() }
			})
			, &nActionID);

		if (nActionID < 0)
		{
			if (strWorkflow.empty())
			{
				throw UCLIDException("ELI43442", Util::Format("Action \"%s\" does not exist.",
					strActionName.c_str()));
			}
			else
			{
				UCLIDException ue("ELI43443", Util::Format(
					"Action \"%s\" does not exist in workflow.",
					strActionName.c_str()));
				// Add workflow as debug for consistency with errors emanating from getActionID with
				// workflow ID specified.
				ue.addDebugInfo("Workflow", strWorkflow, false);
				throw ue;
			}
		}

		return nActionID;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42036");
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getActionIDNoThrow(_ConnectionPtr ipConnection, const string& strActionName,
										   const string& strWorkflow)
{
	try
	{
		// This query will always return a row-- it will return -1 when no matching action is present.
		string strQuery =
			"SELECT COALESCE(MAX([Action].[ID]), -1) AS [ID] FROM [Action] "
			"	LEFT JOIN [WorkFlow] ON [WorkflowID] = [Workflow].[ID]"
			"	WHERE [ASCName] = @ActionName AND ISNULL([Workflow].[Name],'') = ISNULL(@WorkflowName,'')";

		variant_t vtWorkflow;
			
		if (!strWorkflow.empty())
			vtWorkflow = strWorkflow.c_str();
		else
			vtWorkflow.vt = VT_NULL;

		long nActionID = -1;

		getCmdId(buildCmd(ipConnection, strQuery,
			{
				{"@ActionName", strActionName.c_str()},
				{"@WorkflowName", vtWorkflow}
			}), &nActionID);

		return nActionID;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42067");
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getActionIDNoThrow(_ConnectionPtr ipConnection, const string& strActionName,
										   long nWorkflowID)
{
	try
	{
		if (nWorkflowID <= 0)
		{
			return getActionIDNoThrow(ipConnection, strActionName, "");
		}
		else
		{
			string strQuery =
				"SELECT COALESCE(MAX([ID]), -1) AS [ID] FROM [Action] WHERE [ASCName] = @ActionName AND [WorkflowID] = @WorkflowID";

			long nActionID = -1;
			getCmdId(buildCmd(ipConnection, strQuery,
				{
					{"@ActionName", strActionName.c_str()},
					{"@WorkflowID", nWorkflowID}
				}), &nActionID);

			return nActionID;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43483");
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::getActionIDsForActiveWorkflow(_ConnectionPtr ipConnection, const string& strActionName)
{
	try
	{
		// This method is used frequently in processing. For efficiency, retrieve the currently workflow's
		// action ID(s) from cache.
		{
			CSingleLock lock(&m_criticalSection, TRUE);
			auto actionIDsIter = m_mapActionIdsForActiveWorkflow.find(strActionName);
			if (actionIDsIter != m_mapActionIdsForActiveWorkflow.end())
			{
				return actionIDsIter->second;
			}
		}

		string strActionIDs;

		string strActiveWorkflow = getActiveWorkflow();

		// If processing on all workflows for this action
		if (strActiveWorkflow.empty())
		{
			// Create a pointer to a recordset
			_RecordsetPtr ipActionSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI42080", ipActionSet != __nullptr);
			
			_CommandPtr cmdActions  = buildCmd(ipConnection,
				"SELECT [Action].[ID] AS [ID] FROM [Action]"
				"	WHERE [ASCName] = @ActionName",
				{ {"@ActionName", strActionName.c_str()} });

			// Open the Action table in the database
			ipActionSet->Open((IDispatch*)cmdActions, vtMissing, adOpenDynamic, adLockOptimistic, adCmdText);

			while (ipActionSet->adoEOF == VARIANT_FALSE)
			{
				if (!strActionIDs.empty())
				{
					strActionIDs += ",";
				}

				FieldsPtr ipFields = ipActionSet->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI42081", ipFields != __nullptr);

				long nActionID = getLongField(ipFields, "ID");

				strActionIDs += asString(nActionID);

				ipActionSet->MoveNext();
			}
		}
		// Running a specific workflow
		else
		{
			strActionIDs = asString(getActionID(ipConnection, strActionName, strActiveWorkflow));
		}

		// Cache strActionIDs for subsequent calls.
		{
			CSingleLock lock(&m_criticalSection, TRUE);
			m_mapActionIdsForActiveWorkflow[strActionName] = strActionIDs;
		}

		return strActionIDs;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42082");
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::getActionName(_ConnectionPtr ipConnection, long nActionID)
{
	try
	{
		_RecordsetPtr ipAction(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI14046", ipAction != __nullptr);

		_variant_t vtActionName;
		if (!executeCmd(buildCmd(ipConnection,
			"SELECT [ASCName] FROM [Action] WHERE [ID] = @ActionID ",
				{ { "@ActionID", nActionID } })
			, false, true, "ASCName",&vtActionName))
		{
			UCLIDException ue("ELI14047", "Action ID was not found.");
			ue.addDebugInfo("Action ID", nActionID);
			throw ue;
		}
		else if (vtActionName.vt != VT_BSTR)
		{
			UCLIDException ue("ELI51599", "Value is not a string type");
			ue.addDebugInfo("Type", vtActionName.vt);
			throw ue;
		}

		return asString(vtActionName.bstrVal);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26722");
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::addAction(_ConnectionPtr ipConnection, const string &strAction,
								  const string &strWorkflow)
{
	try
	{	
		long lActionId = 0;

		// If no workflow, add a workflow independent action
		if (strWorkflow.empty())
		{
			getCmdId( buildCmd(ipConnection, "INSERT INTO [Action] ([ASCName]) "
				"OUTPUT INSERTED.ID "
				"VALUES (@ActionName)",
				{ {"@ActionName", strAction.c_str()} }),
				&lActionId);
			//executeCmdQuery(ipConnection, strQuery, false, &lActionId);
		}
		// If a workflow is specified, add a workflow specific action. A separate call with
		// strWorkflow == "" may be needed to create the base workflow-independent action.
		else
		{
			long nWorkflowID = getWorkflowID(ipConnection, strWorkflow);
			getCmdId(buildCmd(ipConnection, "INSERT INTO [Action] ([ASCName], [WorkflowID]) "
				"OUTPUT INSERTED.ID "
				"VALUES (@ActionName, @WorkflowId)",
				{
					{"@ActionName", strAction.c_str()},
					{"@WorkflowId", nWorkflowID}
				}), &lActionId);
		}

		return lActionId;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30528")
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addASTransFromSelect(_ConnectionPtr ipConnection,
											  map<string,variant_t> &params,
											  const string &strAction, long nActionID,
											  const string &strToState, const string &strException,
											  const string &strComment, const string &strWhereClause, 
											  const string &strTopClause)
{
	try
	{
		if (!m_bUpdateFASTTable)
		{
			return;
		}
		if (params.find("@ActionID") == params.end())
		{
			params["@ActionID"] = nActionID;
		}

		// Create the from string
		string strFrom = " FROM FAMFile WITH (NOLOCK) LEFT JOIN FileActionStatus WITH (NOLOCK) "
			" ON FAMFile.ID = FileActionStatus.FileID AND FileActionStatus.ActionID = @ActionID " + strWhereClause;
		
		// if the strException string is empty NULL should be added to the db
		variant_t vtNullValue;
		vtNullValue.vt = VT_NULL;
		
		params["@Exception"] = (strException.empty()) ? vtNullValue : strException.c_str();

		// if the strComment is empty the NULL should be added to the database
		params["@Comment"] = (strComment.empty()) ? vtNullValue : strComment.c_str();

		params["@FAMUserID"] = getFAMUserID(ipConnection);
		params["@MachineID"] = getMachineID(ipConnection);
		params["@ToState"] = strToState.c_str();

		// create the insert string
		string strInsertTrans = "INSERT INTO FileActionStateTransition (FileID, ActionID, ASC_From, "
			"ASC_To, DateTimeStamp, Exception, Comment, FAMUserID, MachineID) ";
		strInsertTrans += "SELECT " + strTopClause + " FAMFile.ID, @ActionID " + 
			" as ActionID, COALESCE(FileActionStatus.ActionStatus, 'U') as ActionStatus, @ToState" + 
			" as ASC_To, GetDate() as DateTimeStamp, @Exception, @Comment, @FAMUserID, @MachineID " + 
			strFrom;

		// Insert the records
		executeCmd(buildCmd(ipConnection, strInsertTrans, params));
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26937");
}
//--------------------------------------------------------------------------------------------------
shared_ptr<CppBaseApplicationRoleConnection> CFileProcessingDB::getAppRoleConnection()
{
	INIT_EXCEPTION_AND_TRACING("MLI00018");
	try
	{
		validateServerAndDatabase();

		// Get the current threads ID
		DWORD dwThreadID = GetCurrentThreadId();

		// Lock mutex to keep other instances from running code that may cause the
		// connection to be reset
		CSingleLock lg(&m_criticalSection, TRUE);

		auto it = m_mapThreadIDtoDBConnections.find(dwThreadID);
		_lastCodePos = "5";

		if (it != m_mapThreadIDtoDBConnections.end())
		{
			shared_ptr<CppBaseApplicationRoleConnection> appRole = it->second;
			if (appRole && appRole->ADOConnection()->State != ADODB::adStateClosed)
			{
				if (!m_roleUtility.UseApplicationRoles)
				{
					return appRole;
				}

				if (appRole->ActiveRole() == m_currentRole)
				{
					// The cached role will work for this request
					return appRole;
				}

				// Leave the open connection in the cache, it will be replaced below
			}
			else
			{
				// The connection is closed so remove it from the cache.
				// This might trigger the bFirstConnection logic below
				m_mapThreadIDtoDBConnections.erase(dwThreadID);
			}
		}

		bool bFirstConnection = m_mapThreadIDtoDBConnections.size() == 0;
		if (bFirstConnection)
		{
			resetOpenConnectionData();
		}

		_ConnectionPtr ipConnection = getDBConnectionWithoutAppRole();

		// Get database ID instead of using checkDatabaseIDValid because we want to be able to get the
		// password even in the database ID is corrupted.
		auto getDatabaseID = [&]() -> long { return asLong(getDBInfoSetting(ipConnection, "DatabaseHash", false)); };
		auto appRoleConnection = m_roleUtility.CreateAppRole(ipConnection, m_currentRole, getDatabaseID);

		m_mapThreadIDtoDBConnections[dwThreadID] = appRoleConnection;

		// return the open connection
		return appRoleConnection;
	}
	catch (...)
	{
		UCLIDException ue = uex::fromCurrent("ELI18320", &_lastCodePos);

		// if we catch any exception, that means that we could not
		// establish a connection successfully
		// Post message indicating that the database's connection is no longer established
		postStatusUpdateNotification(kConnectionNotEstablished);

		// Update the connection Status string 
		// TODO:  may want to get more detail as to what is the problem
		m_strCurrentConnectionStatus = gstrUNABLE_TO_CONNECT_TO_SERVER;

		// throw the exception to the outer scope
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
_ConnectionPtr CFileProcessingDB::getDBConnectionRegardlessOfRole()
{
	try
	{
		try
		{
			validateServerAndDatabase();

			// Get the current threads ID
			DWORD dwThreadID = GetCurrentThreadId();

			// Lock mutex to keep other instances from running code that may cause the
			// connection to be reset
			CSingleLock lg(&m_criticalSection, TRUE);

			auto it = m_mapThreadIDtoDBConnections.find(dwThreadID);

			if (it != m_mapThreadIDtoDBConnections.end())
			{
				if (it->second != __nullptr
					&& it->second->ADOConnection()->State != ADODB::adStateClosed)
				{
					return it->second->ADOConnection();
				}

				m_mapThreadIDtoDBConnections.erase(dwThreadID);
			}

			bool bFirstConnection = m_mapThreadIDtoDBConnections.size() == 0;
			if (bFirstConnection)
			{
				resetOpenConnectionData();
			}

			_ConnectionPtr ipConnection = getDBConnectionWithoutAppRole();

			// While an app role connection wrapper is not needed to return the connection, create one
			// so this connection can be cached for subsequent calls.
			auto appRoleConnection = m_roleUtility.CreateAppRole(
				ipConnection, AppRole::kNoRole, __nullptr);

			m_mapThreadIDtoDBConnections[dwThreadID] = appRoleConnection;

			return appRoleConnection->ADOConnection();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI53098");
	}
	catch (UCLIDException ue)
	{
		// if we catch any exception, that means that we could not
		// establish a connection successfully
		// Post message indicating that the database's connection is no longer established
		postStatusUpdateNotification(kConnectionNotEstablished);

		// Update the connection Status string 
		// TODO:  may want to get more detail as to what is the problem
		m_strCurrentConnectionStatus = gstrUNABLE_TO_CONNECT_TO_SERVER;

		// throw the exception to the outer scope
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::resetOpenConnectionData()
{
	// Since the database is being opened reset the m_lFAMUserID and m_lMachineID
	m_lFAMUserID = 0;
	m_lMachineID = 0;
	m_bDeniedFastCountPermission = false;
	m_mapWorkflowDefinitions.clear();
	m_mapActionIdsForActiveWorkflow.clear();

	// Zero indicates the ID needs to be looked up next time the ID is requested.
	// -1 indicates there is no active workflow.
	m_nActiveWorkflowID = 0;

	// Set the status of the connection to not connected
	m_strCurrentConnectionStatus = gstrNOT_CONNECTED;
	m_ipDBInfoSettings = __nullptr;

	// Reset the schema version to indicate that it needs to be read from DB
	m_iDBSchemaVersion = 0;

	// After every successful connection, re-check the enabled features from the DB.
	m_bCheckedFeatures = false;
}
//--------------------------------------------------------------------------------------------------
_ConnectionPtr CFileProcessingDB::getDBConnectionWithoutAppRole()
{
	string strConnectionString;

	try
	{
		_ConnectionPtr ipConnection(__uuidof(Connection));
		ASSERT_RESOURCE_ALLOCATION("ELI51778", ipConnection != __nullptr);

		validateServerAndDatabase();

		// Create the connection string with the current server and database
		strConnectionString = createConnectionString(m_strDatabaseServer,
			m_strDatabaseName);

		// If any advanced connection string properties are specified, update/override the
		// default connection string.
		if (!m_strAdvConnStrProperties.empty())
		{
			updateConnectionStringProperties(strConnectionString, m_strAdvConnStrProperties);

			// Log an application trace to indicate this process in connecting with advanced
			// connection string properties. Only do this once per process unless the
			// connection string changes.
			CSingleLock lock(&ms_mutexSpecialLoggingLock, TRUE);
			if (ms_strLastUsedAdvConnStr != strConnectionString)
			{
				UCLIDException ue("ELI35133", "Application trace: Attempting connection with "
					"advanced connection string attributes.");
				ue.addDebugInfo("Connection string", strConnectionString);
				ue.log();
				ms_strLastUsedAdvConnStr = strConnectionString;
			}
		}

		// Open the database
		ipConnection->Open(strConnectionString.c_str(), "", "", adConnectUnspecified);
		if ((ipConnection->State & ADODB::adStateOpen) == 0)
		{
			UCLIDException ue("ELI51852", "Connection was not opened");
			throw ue;
		}

		// Connection has been established 
		m_strCurrentConnectionStatus = gstrCONNECTION_ESTABLISHED;

		// Ensure that if we have connected to a different DB that we last connected to,
		// user will need to re-authenticate.
		if (strConnectionString != m_strLastConnectionString)
		{
			m_bLoggedInAsAdmin = false;
		}

		m_strLastConnectionString = strConnectionString;

		// Post message indicating that the database's connection is now established
		postStatusUpdateNotification(kConnectionEstablished);

		ipConnection->CommandTimeout = m_iCommandTimeout;

		return ipConnection;
	}
	catch (UCLIDException ue)
	{
		// if we catch any exception, that means that we could not
		// establish a connection successfully
		// Post message indicating that the database's connection is no longer established
		postStatusUpdateNotification(kConnectionNotEstablished);

		// Update the connection Status string 
		// TODO:  may want to get more detail as to what is the problem
		m_strCurrentConnectionStatus = gstrUNABLE_TO_CONNECT_TO_SERVER;

		ue.addDebugInfo("Connection string", strConnectionString);

		// throw the exception to the outer scope
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
shared_ptr<NoRoleConnection> CFileProcessingDB::confirmNoRoleConnection(const string& eliCode,
	const shared_ptr<CppBaseApplicationRoleConnection>& appRoleConnection)
{
	auto noRoleConnection = dynamic_pointer_cast<NoRoleConnection>(appRoleConnection);
	ASSERT_RUNTIME_CONDITION(eliCode, noRoleConnection != __nullptr, "Could not cast appRoleConnection");

	return noRoleConnection;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::validateServerAndDatabase()
{
	if (m_strDatabaseName.empty() || m_strDatabaseServer.empty())
	{
		UCLIDException ue("ELI51779", "No Database or Server set.");
		ue.addDebugInfo("Server", m_strDatabaseServer);
		ue.addDebugInfo("Database", m_strDatabaseName);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI13668", "File Processing DB");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::reCalculateStats(_ConnectionPtr ipConnection, long nActionID)
{
	// Get the name of the action for the ID
	string strWhere = "WHERE ActionID = @ActionID";
	map<string, variant_t> mapParams;
	mapParams["@ActionID"] = nActionID;

	// Ensure no other stats or action status change until recalculation is complete.
	lockDBTableForTransaction(ipConnection, "FileActionStatus");
	lockDBTableForTransaction(ipConnection, "SkippedFile");
	lockDBTableForTransaction(ipConnection, "ActionStatisticsDelta");
	lockDBTableForTransaction(ipConnection, "ActionStatistics");

	// Delete existing stats for action
	string strDeleteExistingStatsSQL = "DELETE FROM ActionStatistics " + strWhere;
	executeCmd(buildCmd(ipConnection, strDeleteExistingStatsSQL, mapParams));

	// Set up the query to recreate the statistics
	string strCreateActionStatsSQL = gstrRECREATE_ACTION_STATISTICS_FOR_ACTION;
	replaceVariable(strCreateActionStatsSQL, "<ActionIDWhereClause>", strWhere);

	// Recreate the statistics
	executeCmd(buildCmd(ipConnection, strCreateActionStatsSQL, mapParams));

	// need to delete the records in the delta since they have been included in the total
	executeCmd(buildCmd(ipConnection, "DELETE FROM ActionStatisticsDelta " + strWhere, mapParams));
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::dropTables(bool bRetainUserTables)
{
	try
	{
		// Get the list of tables
		vector<string> vecTables; 
		getExpectedTables(vecTables);

		// Retain the user tables if necessary
		if (bRetainUserTables)
		{
			eraseFromVector(vecTables, gstrACTION);
			eraseFromVector(vecTables, gstrDB_INFO);
			eraseFromVector(vecTables, gstrFAM_TAG);
			eraseFromVector(vecTables, gstrUSER_CREATED_COUNTER);
			eraseFromVector(vecTables, gstrLOGIN);
			eraseFromVector(vecTables, gstrDB_FIELD_SEARCH);
			eraseFromVector(vecTables, gstrDB_FILE_HANDLER);
			eraseFromVector(vecTables, gstrDB_FEATURE);
			eraseFromVector(vecTables, gstrMETADATA_FIELD);
			eraseFromVector(vecTables, gstrWORKFLOW_TYPE);
			eraseFromVector(vecTables, gstrWORKFLOW);
			eraseFromVector(vecTables, gstrWEB_APP_CONFIG);
			eraseFromVector(vecTables, gstrDATABASE_SERVICE);
			eraseFromVector(vecTables, gstrMLMODEL);
			eraseFromVector(vecTables, gstrDASHBOARD);
			eraseFromVector(vecTables, gstrREPORTING_DATABASE_MIGRATION_WIZARD);
			eraseFromVector(vecTables, gstrROLE);
			eraseFromVector(vecTables, gstrSECURITY_GROUP);
			eraseFromVector(vecTables, gstrLOGIN_GROUP_MEMBERSHIP);
			eraseFromVector(vecTables, gstrSECURITY_GROUP_ACTION);
			eraseFromVector(vecTables, gstrSECURITY_GROUP_DASHBOARD);
			eraseFromVector(vecTables, gstrSECURITY_GROUP_REPORT);
			eraseFromVector(vecTables, gstrSECURITY_GROUP_WORKFLOW);
			eraseFromVector(vecTables, gstrSECURITY_GROUP_Role);
		}

		// Never drop these tables
		eraseFromVector(vecTables, gstrSECURE_COUNTER);
		eraseFromVector(vecTables, gstrSECURE_COUNTER_VALUE_CHANGE);

		// Drop the tables in the vector
		auto role = getAppRoleConnection();
		dropTablesInVector(role->ADOConnection(), vecTables);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27605")
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addTables(bool bAddUserTables)
{
	try
	{
		// Get a vector of SQL queries that will create the database tables
		vector<string> vecQueries = getTableCreationQueries(bAddUserTables);

		auto role = getAppRoleConnection();

		// Only create the login table if it does not already exist
		if (doesTableExist(role->ADOConnection(), "Login"))
		{
			eraseFromVector(vecQueries, gstrCREATE_LOGIN_TABLE);
		}

		// if both the Secure counter tables exist the FK between them will exist 
		// and so we don't want to create it again
		bool bAddSecureCounterTablesFK = false;
		// Only create the SecureCounter table if it does not already exist
		if (doesTableExist(role->ADOConnection(), gstrSECURE_COUNTER))
		{
			eraseFromVector(vecQueries, gstrCREATE_SECURE_COUNTER);
		}
		else
		{
			bAddSecureCounterTablesFK = true;
		}
		// Only create the SecureCounterValueChange table if it does not already exist
		if (doesTableExist(role->ADOConnection(), gstrSECURE_COUNTER_VALUE_CHANGE))
		{
			eraseFromVector(vecQueries, gstrCREATE_SECURE_COUNTER_VALUE_CHANGE);
		}
		else
		{
			bAddSecureCounterTablesFK = true;
		}

		// Add indexes
		vecQueries.push_back(gstrCREATE_FAM_FILE_INDEX);
		vecQueries.push_back(gstrCREATE_QUEUE_EVENT_INDEX);
		vecQueries.push_back(gstrCREATE_FILE_ACTION_COMMENT_INDEX);
		vecQueries.push_back(gstrCREATE_SKIPPED_FILE_INDEX);
		vecQueries.push_back(gstrCREATE_ACTIONSTATUS_PRIORITY_FILE_ACTIONID_USERID_INDEX);
		vecQueries.push_back(gstrCREATE_FILE_TAG_INDEX);
		vecQueries.push_back(gstrCREATE_ACTIVE_FAM_SESSION_INDEX);
		vecQueries.push_back(gstrCREATE_FPS_FILE_NAME_INDEX);
		vecQueries.push_back(gstrCREATE_INPUT_EVENT_INDEX);
		vecQueries.push_back(gstrCREATE_INPUT_EVENT_FAMUSER_WITH_TIMESTAMP_INDEX);
		vecQueries.push_back(gstrCREATE_FILE_ACTION_STATUS_ALL_INDEX);
		vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_DELTA_ACTIONID_INDEX);
		vecQueries.push_back(gstrCREATE_QUEUED_ACTION_STATUS_CHANGE_INDEX);
		vecQueries.push_back(gstrCREATE_WORK_ITEM_GROUP_FAM_SESSION_INDEX);
		vecQueries.push_back(gstrCREATE_WORK_ITEM_STATUS_INDEX);
		vecQueries.push_back(gstrCREATE_WORK_ITEM_ID_STATUS_INDEX);
		vecQueries.push_back(gstrCREATE_WORK_ITEM_FAM_SESSION_INDEX);
		vecQueries.push_back(gstrMETADATA_FIELD_VALUE_INDEX);
		vecQueries.push_back(gstrMETADATA_FIELD_VALUE_VALUE_INDEX);
		vecQueries.push_back(gstrCREATE_FAST_ACTIONID_INDEX);
		vecQueries.push_back(gstrCREATE_FAST_FILEID_ACTIONID_INDEX);
		vecQueries.push_back(gstrCREATE_FILE_TASK_SESSION_DATETIMESTAMP_INDEX);
		vecQueries.push_back(gstrCREATE_FILE_TASK_SESSION_FAMSESSION_INDEX);
		vecQueries.push_back(gstrCREATE_FILE_TASK_SESSION_TASKCLASSID_WITH_ID_SESSIONID_DATE);
		vecQueries.push_back(gstrCREATE_PAGINATION_ORIGINALFILE_INDEX);
		vecQueries.push_back(gstrCREATE_PAGINATION_FILETASKSESSION_INDEX);
		vecQueries.push_back(gstrCREATE_FILE_TASK_SESSION_ACTION_INDEX);
		vecQueries.push_back(gstrCREATE_PAGINATION_DESTFILE_INDEX);
		vecQueries.push_back(gstrCREATE_PAGINATION_SOURCEFILE_INDEX);
		vecQueries.push_back(gstrCREATE_FAMSESSION_ID_FAMUSERID_INDEX);
		vecQueries.push_back(gstrCREATE_FILETASKSESSION_DATETIMESTAMP_WITH_INCLUDES_INDEX);
		vecQueries.push_back(gstrCREATE_WORKFLOWFILE_FILEID_WORKFLOWID_INVISIBLE_INDEX);

		// Add user-table specific indices if necessary.
		if (bAddUserTables)
		{
			vecQueries.push_back(gstrGRANT_DBINFO_SELECT_TO_PUBLIC);

			vecQueries.push_back(gstrCREATE_USER_CREATED_COUNTER_VALUE_INDEX);
			vecQueries.push_back(gstrCREATE_DB_INFO_ID_INDEX);
			vecQueries.push_back(gstrCREATE_DATABASE_SERVICE_DESCRIPTION_INDEX);
		}

		// Add Foreign keys 
		vecQueries.push_back(gstrADD_STATISTICS_ACTION_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_FAM_FILE_FK);
		vecQueries.push_back(gstrADD_QUEUE_EVENT_FAM_FILE_FK);
		vecQueries.push_back(gstrADD_QUEUE_EVENT_QUEUE_EVENT_CODE_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_MACHINE_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_FAM_USER_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_STATE_TO_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_STATE_FROM_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_QUEUE_FK);
		vecQueries.push_back(gstrADD_QUEUE_EVENT_MACHINE_FK);
		vecQueries.push_back(gstrADD_QUEUE_EVENT_FAM_USER_FK);
		vecQueries.push_back(gstrADD_QUEUE_EVENT_ACTION_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_COMMENT_ACTION_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_COMMENT_FAM_FILE_FK);
		vecQueries.push_back(gstrADD_SKIPPED_FILE_FAM_FILE_FK);
		vecQueries.push_back(gstrADD_SKIPPED_FILE_ACTION_FK);
		vecQueries.push_back(gstrADD_SKIPPED_FILE_FAM_SESSION_FK);
		vecQueries.push_back(gstrADD_FILE_TAG_FAM_FILE_FK);
		vecQueries.push_back(gstrADD_FILE_TAG_TAG_ID_FK);
		vecQueries.push_back(gstrADD_LOCKED_FILE_ACTION_FK);
		vecQueries.push_back(gstrADD_LOCKED_FILE_ACTION_STATE_FK);
		vecQueries.push_back(gstrADD_LOCKED_FILE_FAMFILE_FK);
		vecQueries.push_back(gstrADD_LOCKED_FILE_ACTIVEFAM_FK);
		vecQueries.push_back(gstrADD_ACTIVEFAM_FAM_SESSION_FK);
		vecQueries.push_back(gstrADD_FAM_SESSION_ACTION_FK);
		vecQueries.push_back(gstrADD_FAM_SESSION_MACHINE_FK);
		vecQueries.push_back(gstrADD_FAM_SESSION_FAMUSER_FK);
		vecQueries.push_back(gstrADD_FAM_SESSION_FPSFILE_FK);
		vecQueries.push_back(gstrADD_INPUT_EVENT_ACTION_FK);
		vecQueries.push_back(gstrADD_INPUT_EVENT_MACHINE_FK);
		vecQueries.push_back(gstrADD_INPUT_EVENT_FAMUSER_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATUS_ACTION_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATUS_FAMFILE_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATUS_ACTION_STATUS_FK);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATUS_FAMUSER_FK);
		vecQueries.push_back(gstrADD_ACTION_STATISTICS_DELTA_ACTION_FK);
		vecQueries.push_back(gstrADD_SOURCE_DOC_CHANGE_HISTORY_FAMFILE_FK);
		vecQueries.push_back(gstrADD_SOURCE_DOC_CHANGE_HISTORY_FAMUSER_FK);
		vecQueries.push_back(gstrADD_SOURCE_DOC_CHANGE_HISTORY_MACHINE_FK);
		vecQueries.push_back(gstrADD_DOC_TAG_HISTORY_FAMFILE_FK);
		vecQueries.push_back(gstrADD_DOC_TAG_HISTORY_TAG_FK);
		vecQueries.push_back(gstrADD_DOC_TAG_HISTORY_FAMUSER_FK);
		vecQueries.push_back(gstrADD_DOC_TAG_HISTORY_MACHINE_FK);
		vecQueries.push_back(gstrADD_DB_INFO_HISTORY_FAMUSER_FK);
		vecQueries.push_back(gstrADD_DB_INFO_HISTORY_MACHINE_FK);
		vecQueries.push_back(gstrADD_DB_INFO_HISTORY_DB_INFO_FK);
		vecQueries.push_back(gstrADD_FTP_EVENT_HISTORY_FTP_ACCOUNT_FK);
		vecQueries.push_back(gstrADD_FTP_EVENT_HISTORY_FAM_FILE_FK);
		vecQueries.push_back(gstrADD_FTP_EVENT_HISTORY_ACTION_FK);
		vecQueries.push_back(gstrADD_FTP_EVENT_HISTORY_MACHINE_FK);
		vecQueries.push_back(gstrADD_FTP_EVENT_HISTORY_FAM_USER_FK);
		vecQueries.push_back(gstrADD_QUEUED_ACTION_STATUS_CHANGE_FAMFILE_FK);
		vecQueries.push_back(gstrADD_QUEUED_ACTION_STATUS_CHANGE_ACTION_FK);
		vecQueries.push_back(gstrADD_QUEUED_ACTION_STATUS_CHANGE_MACHINE_FK);
		vecQueries.push_back(gstrADD_QUEUED_ACTION_STATUS_CHANGE_USER_FK);
		vecQueries.push_back(gstrADD_QUEUED_ACTION_STATUS_CHANGE_FAM_SESSION_FK);
		vecQueries.push_back(gstrADD_QUEUED_ACTION_STATUS_CHANGE_TARGETUSER_FK);
		vecQueries.push_back(gstrADD_WORK_ITEM_GROUP_ACTION_FK);
		vecQueries.push_back(gstrADD_WORK_ITEM_GROUP_FAMFILE_FK);
		vecQueries.push_back(gstrADD_WORK_ITEM_GROUP_FAM_SESSION_FK);
		vecQueries.push_back(gstrADD_WORK_ITEM__WORK_ITEM_GROUP_FK);
		vecQueries.push_back(gstrADD_WORK_ITEM_FAM_SESSION_FK);
		vecQueries.push_back(gstrADD_METADATA_FIELD_VALUE_FAMFILE_FK);
		vecQueries.push_back(gstrADD_METADATA_FIELD_VALUE_METADATA_FIELD_FK);
		vecQueries.push_back(gstrADD_FILE_TASK_SESSION_FAM_SESSION_FK);
		vecQueries.push_back(gstrADD_FILE_TASK_SESSION_TASK_CLASS_FK);
		vecQueries.push_back(gstrADD_FILE_TASK_SESSION_FAMFILE_FK);
		vecQueries.push_back(gstrADD_FILE_TASK_SESSION_CACHE_ACTIVEFAM_FK);
		vecQueries.push_back(gstrADD_SECURE_COUNTER_VALUE_CHANGE_FAM_SESSION_FK);
		vecQueries.push_back(gstrADD_PAGINATION_SOURCEFILE_FAMFILE_FK);
		vecQueries.push_back(gstrADD_PAGINATION_DESTFILE_FAMFILE_FK);
		vecQueries.push_back(gstrADD_PAGINATION_ORIGINALFILE_FAMFILE_FK);
		vecQueries.push_back(gstrADD_PAGINATION_FILETASKSESSION_FK);
		vecQueries.push_back(gstrADD_WORKFLOWFILE_FAMFILE_FK);
		vecQueries.push_back(gstrADD_WORKFLOWFILE_WORKFLOW_FK);
		vecQueries.push_back(gstrADD_WORKFLOWCHANGE_WORKFLOW_FK);
		vecQueries.push_back(gstrADD_WORKFLOWCHANGEFILE_FAMFILE_FK);
		vecQueries.push_back(gstrADD_WORKFLOWCHANGEFILE_WORKFLOWCHANGE_FK);
		vecQueries.push_back(gstrCREATE_WORKFLOWCHANGEFILE_INDEX);
		vecQueries.push_back(gstrADD_MLDATA_FAMFILE_FK);
		vecQueries.push_back(gstrADD_REPORTING_VERIFICATION_RATES_FAMFILE_FK);
		vecQueries.push_back(gstrADD_REPORTING_VERIFICATION_RATES_DATABASE_SERVICE_FK);
		vecQueries.push_back(gstrADD_REPORTING_VERIFICATION_RATES_ACTION_FK);
		vecQueries.push_back(gstrADD_REPORTING_VERIFICATION_RATES_TASK_CLASS_FK);
		vecQueries.push_back(gstrADD_REPORTING_VERIFICATION_RATES_FILE_TASK_SESSION_FK);
		vecQueries.push_back(gstrADD_DATABASESERVICE_MACHINE_FK);
		vecQueries.push_back(gstrADD_DATABASESERVICE_ACTIVEFAM_FK);
		vecQueries.push_back(gstrADD_DATABASESERVICE_ACTIVE_MACHINE_FK);
		vecQueries.push_back(gstrADD_EMAILSOURCE_FAMSESSION_ID_FK);
		vecQueries.push_back(gstrADD_EMAILSOURCE_QUEUEEVENT_ID_FK);
		vecQueries.push_back(gstrADD_EMAILSOURCE_FAMFILE_ID_FK);

		if (bAddUserTables)
		{
			vecQueries.push_back(gstrADD_ACTION_WORKFLOW_FK);
			vecQueries.push_back(gstrADD_WORKFLOW_WORKFLOWTYPE_FK);
			vecQueries.push_back(gstrADD_WORKFLOW_STARTACTION_FK);
			vecQueries.push_back(gstrADD_WORKFLOW_ENDACTION_FK);
			vecQueries.push_back(gstrADD_WORKFLOW_POSTWORKFLOWACTION_FK);
			vecQueries.push_back(gstrADD_WORKFLOW_EDITACTION_FK);
			vecQueries.push_back(gstrADD_WORKFLOW_POSTEDITACTION_FK);
			// Foreign key for OutputAttributeSetID is added in AttributeDBMgr
			vecQueries.push_back(gstrADD_WORKFLOW_OUTPUTFILEMETADATAFIELD_FK);
			vecQueries.push_back(gstrADD_FILE_HANDLER_WORKFLOW_FK);
			vecQueries.push_back(gstrADD_WEB_APP_CONFIG_WORKFLOW_FK);
			vecQueries.push_back(gstrADD_MLDATA_MLMODEL_FK);
			vecQueries.push_back(gstrADD_DASHBOARD_FAMUSER_FK);

			vecQueries.push_back(gstrADD_LOGINGROUPMEMBERSHIP_GROUP_ID_FK);
			vecQueries.push_back(gstrADD_LOGINGROUPMEMBERSHIP_LOGIN_ID_FK);
			vecQueries.push_back(gstrADD_GROUPACTION_GROUP_ID_FK);
			vecQueries.push_back(gstrADD_GROUPACTION_Action_ID_FK);
			vecQueries.push_back(gstrADD_GROUPDASHBOARD_GROUP_ID_FK);
			vecQueries.push_back(gstrADD_GROUPDASHBOARD_DASHBOARD_GUID_FK);
			vecQueries.push_back(gstrADD_GROUPREPORT_GROUP_ID_FK);
			vecQueries.push_back(gstrADD_GROUPWORKFLOW_GROUP_ID_FK);
			vecQueries.push_back(gstrADD_GROUPWORKFLOW_WORKFLOW_ID_FK);
			vecQueries.push_back(gstrADD_GROUPROLE_ROLE_ID_FK);
			vecQueries.push_back(gstrADD_GROUPROLE_GROUP_ID_FK);
			vecQueries.push_back(gstrADD_FAMUSER_LOGIN_ID_FK);
			
			// Add triggers
			vecQueries.push_back(gstrCREATE_ACTION_ON_DELETE_TRIGGER);
			vecQueries.push_back(gstrCREATE_WORKFLOW_ON_DELETE_TRIGGER);
			vecQueries.push_back(gstrCREATE_DATABASE_SERVICE_UPDATE_TRIGGER);
		}

		// Don't create the FK between the Secure counter tables unless at least one
		// of the SercureCounter tables had to be created
		if (bAddSecureCounterTablesFK)
		{
			vecQueries.push_back(gstrADD_SECURE_COUNTER_VALUE_CHANGE_SECURE_COUNTER_FK);
		}

		vecQueries.push_back(gstrADD_DB_PROCEXECUTOR_ROLE);

		// Add Views
		vecQueries.push_back(gstrCREATE_PAGINATED_DEST_FILES_VIEW);
		vecQueries.push_back(gstrCREATE_USERS_WITH_ACTIVE_VIEW);
		vecQueries.push_back(gstrCREATE_FAMUSER_INPUT_EVENTS_TIME_VIEW);
		vecQueries.push_back(gstrCREATE_PAGINATION_DATA_WITH_RANK_VIEW);
		vecQueries.push_back(gstrCREATE_PROCESSING_DATA_VIEW);
		vecQueries.push_back(gstrCREATE_FAMUSER_INPUT_EVENTS_TIME_VIEW_LEGACY_166);
		vecQueries.push_back(gstrCREATE_FAMUSER_INPUT_EVENTS_TIME_WITH_FILEID_VIEW);
		vecQueries.push_back(gstrCREATE_USAGE_FOR_SPECIFIC_USER_SPECIFIC_DAY_PROCEDURE);
		vecQueries.push_back(gstrCREATE_TABLE_FROM_COMMA_SEPARATED_LIST_FUNCTION);
		vecQueries.push_back(gstrCREATE_USER_COUNTS_STORED_PROCEDURE);
		vecQueries.push_back(gstrCREATE_GET_FILES_TO_PROCESS_STORED_PROCEDURE);

		// Execute all of the queries
		executeVectorOfSQL(role->ADOConnection(), vecQueries);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI18011");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addTables80()
{
	try
	{
		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_DB_INFO_TABLE_80);
		vecQueries.push_back(gstrCREATE_FAM_TAG_TABLE_80);
		vecQueries.push_back(gstrCREATE_USER_CREATED_COUNTER_TABLE_80);
		vecQueries.push_back(gstrCREATE_USER_CREATED_COUNTER_VALUE_INDEX_80);
		vecQueries.push_back(gstrCREATE_ACTION_TABLE_80);
		vecQueries.push_back(gstrCREATE_LOCK_TABLE_80);
		vecQueries.push_back(gstrCREATE_ACTION_STATE_TABLE_80);
		vecQueries.push_back(gstrCREATE_FAM_FILE_TABLE_80);
		vecQueries.push_back(gstrCREATE_FAM_FILE_ID_PRIORITY_INDEX_80);
		vecQueries.push_back(gstrCREATE_FAM_FILE_INDEX_80);
		vecQueries.push_back(gstrCREATE_QUEUE_EVENT_CODE_TABLE_80);
		vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_TABLE_80);
		vecQueries.push_back(gstrCREATE_FILE_ACTION_STATE_TRANSITION_TABLE_80);
		vecQueries.push_back(gstrCREATE_QUEUE_EVENT_TABLE_80);
		vecQueries.push_back(gstrCREATE_QUEUE_EVENT_INDEX_80);
		vecQueries.push_back(gstrCREATE_MACHINE_TABLE_80);
		vecQueries.push_back(gstrCREATE_FAM_USER_TABLE_80);
		vecQueries.push_back(gstrCREATE_FAM_FILE_ACTION_COMMENT_TABLE_80);
		vecQueries.push_back(gstrCREATE_FILE_ACTION_COMMENT_INDEX_80);
		vecQueries.push_back(gstrCREATE_FAM_SKIPPED_FILE_TABLE_80);
		vecQueries.push_back(gstrCREATE_SKIPPED_FILE_INDEX_80);
		vecQueries.push_back(gstrCREATE_SKIPPED_FILE_UPI_INDEX_80);
		vecQueries.push_back(gstrCREATE_FAM_FILE_TAG_TABLE_80);
		vecQueries.push_back(gstrCREATE_FILE_TAG_INDEX_80);
		vecQueries.push_back(gstrCREATE_PROCESSING_FAM_TABLE_80);
		vecQueries.push_back(gstrCREATE_PROCESSING_FAM_UPI_INDEX_80);
		vecQueries.push_back(gstrCREATE_LOCKED_FILE_TABLE_80);
		vecQueries.push_back(gstrCREATE_FPS_FILE_TABLE_80);
		vecQueries.push_back(gstrCREATE_FPS_FILE_NAME_INDEX_80);
		vecQueries.push_back(gstrCREATE_FAM_SESSION_80);
		vecQueries.push_back(gstrCREATE_INPUT_EVENT_80);
		vecQueries.push_back(gstrCREATE_INPUT_EVENT_INDEX_80);
		vecQueries.push_back(gstrCREATE_LOGIN_TABLE_80);
		vecQueries.push_back(gstrADD_STATISTICS_ACTION_FK_80);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_FK_80);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_FAM_FILE_FK_80);
		vecQueries.push_back(gstrADD_QUEUE_EVENT_FAM_FILE_FK_80);
		vecQueries.push_back(gstrADD_QUEUE_EVENT_QUEUE_EVENT_CODE_FK_80);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_MACHINE_FK_80);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_FAM_USER_FK_80);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_STATE_TO_FK_80);
		vecQueries.push_back(gstrADD_FILE_ACTION_STATE_TRANSITION_ACTION_STATE_FROM_FK_80);
		vecQueries.push_back(gstrADD_QUEUE_EVENT_MACHINE_FK_80);
		vecQueries.push_back(gstrADD_QUEUE_EVENT_FAM_USER_FK_80);
		vecQueries.push_back(gstrADD_QUEUE_EVENT_ACTION_FK_80);
		vecQueries.push_back(gstrADD_FILE_ACTION_COMMENT_ACTION_FK_80);
		vecQueries.push_back(gstrADD_FILE_ACTION_COMMENT_FAM_FILE_FK_80);
		vecQueries.push_back(gstrADD_SKIPPED_FILE_FAM_FILE_FK_80);
		vecQueries.push_back(gstrADD_SKIPPED_FILE_ACTION_FK_80);
		vecQueries.push_back(gstrADD_FILE_TAG_FAM_FILE_FK_80);
		vecQueries.push_back(gstrADD_FILE_TAG_TAG_ID_FK_80);
		vecQueries.push_back(gstrADD_LOCKED_FILE_ACTION_FK_80);
		vecQueries.push_back(gstrADD_LOCKED_FILE_ACTION_STATE_FK_80);
		vecQueries.push_back(gstrADD_LOCKED_FILE_FAMFILE_FK_80);
		vecQueries.push_back(gstrADD_LOCKED_FILE_PROCESSINGFAM_FK_80);
		vecQueries.push_back(gstrADD_FAM_SESSION_MACHINE_FK_80);
		vecQueries.push_back(gstrADD_FAM_SESSION_FAMUSER_FK_80);
		vecQueries.push_back(gstrADD_FAM_SESSION_FPSFILE_FK_80);
		vecQueries.push_back(gstrADD_INPUT_EVENT_ACTION_FK_80);
		vecQueries.push_back(gstrADD_INPUT_EVENT_MACHINE_FK_80);
		vecQueries.push_back(gstrADD_INPUT_EVENT_FAMUSER_FK_80);

		// Execute all of the queries
		executeVectorOfSQL(getDBConnectionWithoutAppRole(), vecQueries);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34240");
}
//--------------------------------------------------------------------------------------------------
vector<string> CFileProcessingDB::getTableCreationQueries(bool bIncludeUserTables)
{
	vector<string> vecQueries;

	// WARNING: If any table is removed, code needs to be modified so that
	// findUnrecognizedSchemaElements does not treat the element on old schema versions as
	// unrecognized.

	// Add the user tables if necessary
	if (bIncludeUserTables)
	{
		vecQueries.push_back(gstrCREATE_ACTION_TABLE);
		vecQueries.push_back(gstrCREATE_DB_INFO_TABLE);
		vecQueries.push_back(gstrCREATE_FAM_TAG_TABLE);
		vecQueries.push_back(gstrCREATE_USER_CREATED_COUNTER_TABLE);
		vecQueries.push_back(gstrCREATE_FIELD_SEARCH_TABLE);
		vecQueries.push_back(gstrCREATE_FILE_HANDLER_TABLE);
		vecQueries.push_back(gstrCREATE_FEATURE_TABLE);
		vecQueries.push_back(gstrCREATE_METADATA_FIELD_TABLE);
		vecQueries.push_back(gstrCREATE_SECURE_COUNTER);
		vecQueries.push_back(gstrCREATE_SECURE_COUNTER_VALUE_CHANGE);
		vecQueries.push_back(gstrCREATE_WORKFLOW_TYPE);
		vecQueries.push_back(gstrCREATE_WORKFLOW);
		vecQueries.push_back(gstrCREATE_WEB_APP_CONFIG);
		vecQueries.push_back(gstrCREATE_DATABASE_SERVICE_TABLE);
		vecQueries.push_back(gstrCREATE_MLMODEL);
		vecQueries.push_back(gstrCREATE_DASHBOARD_TABLE);
		vecQueries.push_back(gstrCREATE_SECURITY_SCHEMA);
		vecQueries.push_back(gstrCREATE_ROLE_TABLE);
		vecQueries.push_back(gstrCREATE_GROUP_TABLE);
		vecQueries.push_back(gstrCREATE_LOGINGROUPMEMBERSHIP_TABLE);
		vecQueries.push_back(gstrCREATE_GROUPACTION_TABLE);
		vecQueries.push_back(gstrCREATE_GROUPDASHBOARD_TABLE);
		vecQueries.push_back(gstrCREATE_GROUPREPORT_TABLE);
		vecQueries.push_back(gstrCREATE_GROUPWORKFLOW_TABLE);
		vecQueries.push_back(gstrCREATE_GROUPROLE_TABLE);
	}

	// Add queries to create tables to the vector
	vecQueries.push_back(gstrCREATE_LOCK_TABLE);
	vecQueries.push_back(gstrCREATE_ACTION_STATE_TABLE);
	vecQueries.push_back(gstrCREATE_FAM_FILE_TABLE);
	vecQueries.push_back(gstrCREATE_QUEUE_EVENT_CODE_TABLE);
	vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_TABLE);
	vecQueries.push_back(gstrCREATE_FILE_ACTION_STATE_TRANSITION_TABLE);
	vecQueries.push_back(gstrCREATE_QUEUE_EVENT_TABLE);
	vecQueries.push_back(gstrCREATE_MACHINE_TABLE);
	vecQueries.push_back(gstrCREATE_FAM_USER_TABLE);
	vecQueries.push_back(gstrCREATE_FAM_FILE_ACTION_COMMENT_TABLE);
	vecQueries.push_back(gstrCREATE_FAM_SKIPPED_FILE_TABLE);
	vecQueries.push_back(gstrCREATE_FAM_FILE_TAG_TABLE);
	vecQueries.push_back(gstrCREATE_ACTIVE_FAM_TABLE);
	vecQueries.push_back(gstrCREATE_LOCKED_FILE_TABLE);
	vecQueries.push_back(gstrCREATE_FPS_FILE_TABLE);
	vecQueries.push_back(gstrCREATE_FAM_SESSION);
	vecQueries.push_back(gstrCREATE_INPUT_EVENT);
	vecQueries.push_back(gstrCREATE_FILE_ACTION_STATUS);
	vecQueries.push_back(gstrCREATE_ACTION_STATISTICS_DELTA_TABLE);
	vecQueries.push_back(gstrCREATE_LOGIN_TABLE);
	vecQueries.push_back(gstrCREATE_SOURCE_DOC_CHANGE_HISTORY);
	vecQueries.push_back(gstrCREATE_DOC_TAG_HISTORY_TABLE);
	vecQueries.push_back(gstrCREATE_DB_INFO_CHANGE_HISTORY_TABLE);
	vecQueries.push_back(gstrCREATE_FTP_ACCOUNT);
	vecQueries.push_back(gstrCREATE_FTP_EVENT_HISTORY_TABLE);
	vecQueries.push_back(gstrCREATE_QUEUED_ACTION_STATUS_CHANGE_TABLE);
	vecQueries.push_back(gstrCREATE_WORK_ITEM_GROUP_TABLE);
	vecQueries.push_back(gstrCREATE_WORK_ITEM_TABLE);
	vecQueries.push_back(gstrCREATE_FILE_METADATA_FIELD_VALUE_TABLE);
	vecQueries.push_back(gstrCREATE_TASK_CLASS);
	vecQueries.push_back(gstrCREATE_FILE_TASK_SESSION);
	vecQueries.push_back(gstrCREATE_FILE_TASK_SESSION_CACHE);
	vecQueries.push_back(gstrCREATE_PAGINATION);
	vecQueries.push_back(gstrCREATE_WORKFLOWFILE);
	vecQueries.push_back(gstrCREATE_WORKFLOWCHANGE);
	vecQueries.push_back(gstrCREATE_WORKFLOWCHANGEFILE);
	vecQueries.push_back(gstrCREATE_MLDATA);
	vecQueries.push_back(gstrCREATE_REPORTING_VERIFICATION_RATES);
	vecQueries.push_back(gstrCREATE_DATABASE_MIGRATION_WIZARD_REPORTING);
	vecQueries.push_back(gstrCREATE_EMAIL_SOURCE_TABLE);

	return vecQueries;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::initializeTableValues(bool bInitializeUserTables)
{
	try
	{
		vector<string> vecQueries;

		// Add valid action states to the Action State table
		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('C', 'Complete')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('F', 'Failed')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('P', 'Pending')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('R', 'Processing')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('U', 'Unattempted')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('S', 'Skipped')");

		// Add Valid Queue event codes the QueueEventCode table
		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('A', 'File added to queue')");
		
		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('D', 'File deleted from queue')");

		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('F', 'Folder was deleted')");

		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('M', 'File was modified')");

		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('R', 'File was renamed')");

		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('P', 'File was programmatically added without being queued')");

		vecQueries.push_back(gstrINSERT_TASKCLASS_STORE_RETRIEVE_ATTRIBUTES);
		vecQueries.push_back(gstrINSERT_PAGINATION_TASK_CLASS);
		vecQueries.push_back(gstrSPLIT_MULTI_PAGE_DOCUMENT_TASK_CLASS);
		vecQueries.push_back(gstrINSERT_TASKCLASS_DOCUMENT_API);
		vecQueries.push_back(gstrINSERT_TASKCLASS_WEB_VERIFICATION);
		vecQueries.push_back(gstrINSERT_AUTO_PAGINATE_TASK_CLASS);
		vecQueries.push_back(gstrINSERT_RTF_DIVIDE_BATCHES_TASK_CLASS);
		vecQueries.push_back(gstrINSERT_RTF_UPDATE_BATCHES_TASK_CLASS);
		vecQueries.push_back(gstrINSERT_SPLIT_MIME_FILE_TASK_CLASS);

		vecQueries.push_back(gstrINSERT_ROLE_DEFAULT_ROLES);
		vecQueries.push_back(gstrINSERT_SECURITYGROUP_DEFAULT_GROUPS);

		// Initialize the DB Info settings if necessary
		if (bInitializeUserTables)
		{
			// Retrieve the default DBInfo values
			map<string, string> mapDBInfoDefaultValues = getDBInfoDefaultValues();

			// For each DBInfo value, create a query to set the default value.
			for (map<string, string>::iterator iterDBInfoValues = mapDBInfoDefaultValues.begin();
				iterDBInfoValues != mapDBInfoDefaultValues.end();
				iterDBInfoValues++)
			{
				string strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" +
					iterDBInfoValues->first + "', '" + iterDBInfoValues->second + "')";
				vecQueries.push_back(strSQL);
			}

			// Add the queries to populate the features table.
			vector<string> vecFeatureDefinitionQueries = getFeatureDefinitionQueries();
			vecQueries.insert(vecQueries.end(),
				vecFeatureDefinitionQueries.begin(), vecFeatureDefinitionQueries.end());

			vecQueries.push_back("INSERT INTO [WorkflowType] ([Code], [Meaning]) "
				"VALUES('U', 'Undefined')");

			vecQueries.push_back("INSERT INTO [WorkflowType] ([Code], [Meaning]) "
				"VALUES('R', 'Redaction')");

			vecQueries.push_back("INSERT INTO [WorkflowType] ([Code], [Meaning]) "
				"VALUES('E', 'Extraction')");

			vecQueries.push_back("INSERT INTO [WorkflowType] ([Code], [Meaning]) "
				"VALUES('C', 'Classification')");
		}

		// Clear status in DatabaseService Table
		// https://extract.atlassian.net/browse/ISSUE-15465
		vecQueries.push_back("UPDATE DatabaseService SET Status = NULL");

		auto role = getAppRoleConnection();
		executeVectorOfSQL(role->ADOConnection(), vecQueries);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27606")
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::initializeTableValues80()
{
	try
	{
		vector<string> vecQueries;

		// Add valid action states to the Action State table
		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('C', 'Complete')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('F', 'Failed')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('P', 'Pending')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('R', 'Processing')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('U', 'Unattempted')");

		vecQueries.push_back("INSERT INTO [ActionState] ([Code], [Meaning]) "
			"VALUES('S', 'Skipped')");

		// Add Valid Queue event codes the QueueEventCode table
		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('A', 'File added to queue')");
		
		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('D', 'File deleted from queue')");

		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('F', 'Folder was deleted')");

		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('M', 'File was modified')");

		vecQueries.push_back("INSERT INTO [QueueEventCode] ([Code], [Description]) "
			"VALUES('R', 'File was renamed')");

		// Add the schema version to the DBInfo table
		string strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrFAMDB_SCHEMA_VERSION +
			"', '23')";
		vecQueries.push_back(strSQL);

		// Add Command Timeout setting
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrCOMMAND_TIMEOUT +
			"', '" + asString(glDEFAULT_COMMAND_TIMEOUT) + "')";
		vecQueries.push_back(strSQL);

		// Add Update Queue Event Table setting
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrUPDATE_QUEUE_EVENT_TABLE 
			+ "', '1')";
		vecQueries.push_back(strSQL);

		// Add Update Queue Event Table setting
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrUPDATE_FAST_TABLE + "', '1')";
		vecQueries.push_back(strSQL);

		// Add Auto Delete File Action Comment On Complete setting
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrAUTO_DELETE_FILE_ACTION_COMMENT
			+ "', '0')";
		vecQueries.push_back(strSQL);

		// Add Require Password To Process All Skipped Files setting (default to true)
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrREQUIRE_PASSWORD_TO_PROCESS_SKIPPED
			+ "', '1')";
		vecQueries.push_back(strSQL);

		// Add Allow Dynamic Tag Creation setting (default to false)
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrALLOW_DYNAMIC_TAG_CREATION
			+ "', '0')";
		vecQueries.push_back(strSQL);

		// (LEGACY) Add AutoRevertLockedFiles setting (default to true) 
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('AutoRevertLockedFiles', '1')";
		vecQueries.push_back(strSQL);

		// Add AutoRevertTimeOutInMinutes setting (default to 60 minutes)
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrAUTO_REVERT_TIME_OUT_IN_MINUTES
			+ "', '60')";
		vecQueries.push_back(strSQL);
			
		// Add AutoRevertNotifyEmailList setting (default to empty string)
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrAUTO_REVERT_NOTIFY_EMAIL_LIST
			+ "', '')";
		vecQueries.push_back(strSQL);

		// Add NumberOfConnectionRetries setting (default to empty string)
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrNUMBER_CONNECTION_RETRIES
			+ "', '10')";
		vecQueries.push_back(strSQL);
			
		// Add ConnectionRetryTimeout setting (default to empty string)
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrCONNECTION_RETRY_TIMEOUT
			+ "', '120')";
		vecQueries.push_back(strSQL);

		// (LEGACY) Add StoreFAMSessionHistory setting (default to true)
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrSTORE_FAM_SESSION_HISTORY
			+ "', '1')";
		vecQueries.push_back(strSQL);

		// Add the EnableInputEventTracking setting (default to false)
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrENABLE_INPUT_EVENT_TRACKING
			+ "', '0')";
		vecQueries.push_back(strSQL);

		// Add the InputEventHistorySize setting (default to 30 days)
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrINPUT_EVENT_HISTORY_SIZE
			+ "', '30')";
		vecQueries.push_back(strSQL);

		// Add RequireAuthenticationBeforeRun setting (default to false)
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrREQUIRE_AUTHENTICATION_BEFORE_RUN
			+ "', '0')";
		vecQueries.push_back(strSQL);

		// Add Auto Create Actions setting (default to false)
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" + gstrAUTO_CREATE_ACTIONS
			+ "', '0')";
		vecQueries.push_back(strSQL);

		// Add Skip Authentication On Machines
		strSQL = "INSERT INTO [DBInfo] ([Name], [Value]) VALUES('SkipAuthenticationOnMachines', '')";
		vecQueries.push_back(strSQL);

		// Execute all of the queries
		executeVectorOfSQL(getDBConnectionWithoutAppRole(), vecQueries);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34239")
}
//-------------------------------------------------------------------------------------------------
map<string, string> CFileProcessingDB::getDBInfoDefaultValues()
{
	map<string, string> mapDefaultValues;

	// WARNING: If any DBInfo row is removed, code needs to be modified so that
	// findUnrecognizedSchemaElements does not treat the element on old schema versions as
	// unrecognized.
	mapDefaultValues[gstrFAMDB_SCHEMA_VERSION] = asString(ms_lFAMDBSchemaVersion);
	mapDefaultValues[gstrCOMMAND_TIMEOUT] = asString(glDEFAULT_COMMAND_TIMEOUT);
	mapDefaultValues[gstrUPDATE_QUEUE_EVENT_TABLE] = "1";
	mapDefaultValues[gstrUPDATE_FAST_TABLE] = "1";
	mapDefaultValues[gstrAUTO_DELETE_FILE_ACTION_COMMENT] = "0";
	mapDefaultValues[gstrREQUIRE_PASSWORD_TO_PROCESS_SKIPPED] = "1";
	mapDefaultValues[gstrALLOW_DYNAMIC_TAG_CREATION] = "0";
	mapDefaultValues[gstrAUTO_REVERT_TIME_OUT_IN_MINUTES] = "5";
	mapDefaultValues[gstrAUTO_REVERT_NOTIFY_EMAIL_LIST] = "";
	mapDefaultValues[gstrNUMBER_CONNECTION_RETRIES] = "10";
	mapDefaultValues[gstrCONNECTION_RETRY_TIMEOUT] = "120";
	mapDefaultValues[gstrSTORE_FAM_SESSION_HISTORY] = "1";
	mapDefaultValues[gstrREQUIRE_AUTHENTICATION_BEFORE_RUN] = "0";
	mapDefaultValues[gstrAUTO_CREATE_ACTIONS] = "0";
	mapDefaultValues[gstrSKIP_AUTHENTICATION_ON_MACHINES] = "*";
	mapDefaultValues[gstrACTION_STATISTICS_UPDATE_FREQ_IN_SECONDS] = "300";
	mapDefaultValues[gstrGET_FILES_TO_PROCESS_TRANSACTION_TIMEOUT] =
		asString(gdMINIMUM_TRANSACTION_TIMEOUT, 0);
	mapDefaultValues[gstrSTORE_SOURCE_DOC_NAME_CHANGE_HISTORY] = "1";
	mapDefaultValues[gstrSTORE_DOC_TAG_HISTORY] = "1";
	mapDefaultValues[gstrSTORE_DB_INFO_HISTORY] = "1";
	mapDefaultValues[gstrMIN_SLEEP_BETWEEN_DB_CHECKS] = asString(gnDEFAULT_MIN_SLEEP_TIME_BETWEEN_DB_CHECK);
	mapDefaultValues[gstrMAX_SLEEP_BETWEEN_DB_CHECKS] = asString(gnDEFAULT_MAX_SLEEP_TIME_BETWEEN_DB_CHECK);
	mapDefaultValues[gstrSTORE_FTP_EVENT_HISTORY] = "1";
	mapDefaultValues[gstrALTERNATE_COMPONENT_DATA_DIR] = "";
	mapDefaultValues[gstrENABLE_LOAD_BALANCING] = "1";
	// Email setting defaults should be kept in sync with Extract.Utilities.Email.ExtractSmtp
	mapDefaultValues[gstrEMAIL_ENABLE_SETTINGS] = "0";
	mapDefaultValues[gstrEMAIL_SERVER] = "";
	mapDefaultValues[gstrEMAIL_PORT] = "25";
	mapDefaultValues[gstrEMAIL_SENDER_NAME] = "";
	mapDefaultValues[gstrEMAIL_SENDER_ADDRESS] = "";
	mapDefaultValues[gstrEMAIL_SIGNATURE] = "";
	mapDefaultValues[gstrEMAIL_USERNAME] = "";
	mapDefaultValues[gstrEMAIL_PASSWORD] = "";
	mapDefaultValues[gstrEMAIL_TIMEOUT] = "0";
	mapDefaultValues[gstrEMAIL_USE_SSL] = "0";
	mapDefaultValues[gstrEMAIL_POSSIBLE_INVALID_SERVER] = "0";
	mapDefaultValues[gstrEMAIL_POSSIBLE_INVALID_SENDER_ADDRESS] = "0";

	mapDefaultValues[gstrALLOW_RESTARTABLE_PROCESSING] = "0";
	mapDefaultValues[gstrSEND_ALERTS_TO_EXTRACT] = "0";
	mapDefaultValues[gstrSEND_ALERTS_TO_SPECIFIED] = "0";
	mapDefaultValues[gstrSPECIFIED_ALERT_RECIPIENTS] = "";
	mapDefaultValues[gstrLICENSE_CONTACT_ORGANIZATION] = "";
	mapDefaultValues[gstrLICENSE_CONTACT_EMAIL] = "";
	mapDefaultValues[gstrLICENSE_CONTACT_PHONE] = "";
	mapDefaultValues[gstrINPUT_ACTIVITY_TIMEOUT] = "30";
	mapDefaultValues[gstrVERIFICATION_SESSION_TIMEOUT] = "0";

	CTime ct = CTime::GetCurrentTime();
	mapDefaultValues[gstrETL_RESTART] = ct.Format(_T("%Y-%m-%dT%H:%M:%S")).operator LPCSTR();

	mapDefaultValues[gstrDASHBOARD_INCLUDE_FILTER] = "";
	mapDefaultValues[gstrDASHBOARD_EXCLUDE_FILTER] = "";

	mapDefaultValues[gstrAZURE_TENNANT] = "";
	mapDefaultValues[gstrAZURE_CLIENT_ID] = "";
	mapDefaultValues[gstrAZURE_INSTANCE] = "https://login.microsoftonline.com/";

	// Require 8 char password with upper, lower, and digits for new databases
	mapDefaultValues[gstrPASSWORD_COMPLEXITY_REQUIREMENTS] =
		isInternalToolsLicensed() ? "1" :"8ULD";

	// Create a new database ID  or use existing if it has been set
	ByteStream bsDatabaseID;
	auto role = getAppRoleConnection();
	if (!m_bDatabaseIDValuesValidated)
	{
		createDatabaseID(role->ADOConnection(), m_strDatabaseServer, bsDatabaseID);
	}
	else
	{
		// Puts the current DatabaseID into the bsDatabaseID bytestream
		ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, bsDatabaseID);
		bsm << m_DatabaseIDValues;
		bsm.flushToByteStream(8);
	}

	// Encrypt the DatabaseID
	// The actual value will change in the DBInfo table but it will be the same DatabaseID
	ByteStream bsPW;
	getFAMPassword(bsPW);
	mapDefaultValues[gstrDATABASEID] = MapLabel::setMapLabelWithS(bsDatabaseID,bsPW);
	
	try
	{
		mapDefaultValues[gstrLAST_DB_INFO_CHANGE] = getSQLServerDateTime(role->ADOConnection());
	}
	catch(...)
	{
		// Just eat an exception if the current time could not be retrieved from the DB
		mapDefaultValues[gstrLAST_DB_INFO_CHANGE] = "";
	}

	mapDefaultValues[gstrROOT_PATH_FOR_DASHBOARD_EXTRACTED_DATA] = "";

	return mapDefaultValues;
}
//-------------------------------------------------------------------------------------------------
vector<string> CFileProcessingDB::getFeatureNames()
{
	vector<string> vecFeatureNames;
	vecFeatureNames.push_back(gstrFEATURE_FILE_HANDLER_COPY_NAMES);
	vecFeatureNames.push_back(gstrFEATURE_FILE_HANDLER_COPY_FILES);
	vecFeatureNames.push_back(gstrFEATURE_FILE_HANDLER_COPY_FILES_AND_DATA);
	vecFeatureNames.push_back(gstrFEATURE_FILE_HANDLER_OPEN_FILE_LOCATION);
	vecFeatureNames.push_back(gstrFEATURE_FILE_RUN_DOCUMENT_SPECIFIC_REPORTS);

	return vecFeatureNames;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::copyActionStatus(const _ConnectionPtr& ipConnection, const string& strFrom, 
										 string strTo, bool bAddTransRecords, long nToActionID)
{
	try
	{
		// Temporary string for from action string since getActionID cannot use const string
		// TODO: This can be removed and the const string& strFrom changed to non const ref
		string strTmpFrom = strFrom;

		// Get from action ID
		long nFromActionID = getActionID(ipConnection, strTmpFrom);

		// Set string for the ToActionID
		nToActionID = nToActionID == -1 ? getActionID(ipConnection, strTo) : nToActionID;
		if (bAddTransRecords && m_bUpdateFASTTable)
		{

			string strTransition = "INSERT INTO FileActionStateTransition "
				"(FileID, ActionID, ASC_From, ASC_To, DateTimeStamp, Comment, FAMUserID, MachineID) "
				"SELECT ID, @ToActionID AS ActionID, "
				"COALESCE(fasFrom.ActionStatus, 'U') as ASC_From, "
				"COALESCE(fasTo.ActionStatus, 'U') as ASC_To, "
				"GETDATE() AS TS_Trans, 'Copy status from ' + @FromAction + '"
				" to ' + @ToAction + '''' AS Comment, @FAMUserID, @MachineID "
				" FROM FAMFile WITH (NOLOCK) "
				" LEFT JOIN FileActionStatus as fasFrom WITH (NOLOCK) ON FAMFile.ID = fasFrom.FileID AND fasFrom.ActionID = @FromActionID "
				" LEFT JOIN FileActionStatus as fasTo WITH (NOLOCK) ON FAMFile.ID = fasTo.FileID AND fasTo.ActionID = @ToActionID";

			executeCmd(buildCmd(ipConnection, strTransition,
				{
					{"@FAMUserID", getFAMUserID(ipConnection)},
					{"@MachineID", getMachineID(ipConnection)},
					{"@ToActionID", nToActionID},
					{"@FromAction", strFrom.c_str()},
					{"@ToAction", strTo.c_str()},
					{"@FromActionID", nFromActionID }
				}));
		}

		// Check if the skipped table needs to be updated
		if (nToActionID != -1)
		{
			// Delete any existing skipped records (files may be leaving skipped status)
			string strDeleteSkipped = "DELETE FROM SkippedFile WHERE ActionID = @ToActionID";

			// Need to add any new skipped records (files may be entering skipped status)
			string strAddSkipped = "INSERT INTO SkippedFile (FileID, ActionID, UserName, FAMSessionID) SELECT "
				" FAMFile.ID, @ToActionID AS NewActionID, @FAMUserName" 
				" AS NewUserName, @FAMSessionID AS FAMSessionID FROM FAMFile WITH (NOLOCK) "
				"INNER JOIN FileActionStatus WITH (NOLOCK) ON FAMFile.ID = FileActionStatus.FileID AND "
				"FileActionStatus.ActionID = @FromActionID WHERE ActionStatus = 'S'";

			// Delete the existing skipped records for this action and insert any new ones
			executeCmd(buildCmd(ipConnection, strDeleteSkipped, { {"@ToActionID", nToActionID } }));
			executeCmd(buildCmd(ipConnection, strAddSkipped,
				{
					{"@ToActionID", nToActionID},
					{"@FAMUserName", ((m_strFAMUserName.empty()) ? getCurrentUserName() : m_strFAMUserName).c_str()},
					{"@FAMSessionID", m_nFAMSessionID == 0 ? vtMissing : m_nFAMSessionID},
					{"@FromActionID", nFromActionID}
				}));
		}

		// Delete all of the previous status for the to action
		string strDeleteTo = "DELETE FROM FileActionStatus WHERE ActionID = @ToActionID";
		executeCmd(buildCmd(ipConnection, strDeleteTo, { {"@ToActionID", nToActionID} }));
		
		// There are no cases where this method should not just ignore all pending entries in
		// [QueuedActionStatusChange] for the selected files.
		string strUpdateQueuedActionStatusChange =
			"UPDATE [QueuedActionStatusChange] SET [ChangeStatus] = 'I'"
			"WHERE [ChangeStatus] = 'P' AND [ActionID] = @ToActionID";
		executeCmd(buildCmd(ipConnection, strUpdateQueuedActionStatusChange, 
			{ {"@ToActionID", nToActionID} }));

		// Create new FileActionStatus records based on the value of the from action ID
		string strCopy = "INSERT INTO FileActionStatus (FileID, ActionID, ActionStatus, Priority) "
			"SELECT FileID, @ToActionID as ActionID, ActionStatus, Priority "
			"FROM FileActionStatus WHERE ActionID = @FromActionID";
		executeCmd(buildCmd(ipConnection, strCopy,
			{
				{"@ToActionID", nToActionID},
				{"@FromActionID", nFromActionID}
			}));
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27054");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addActionColumn(const _ConnectionPtr& ipConnection, const string& strAction)
{
	UCLIDException ue("ELI30514", "Adding Action Column is obsolete.");
	ue.addDebugInfo("Action", strAction);
	throw ue;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::removeActionColumn(const _ConnectionPtr& ipConnection,
										   const string& strAction)
{
	UCLIDException ue("ELI30515", "Removing Action Column is obsolete.");
	ue.addDebugInfo("Action", strAction);
	throw ue;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::updateStats(_ConnectionPtr ipConnection, long nActionID, 
									EActionStatus eFromStatus, EActionStatus eToStatus, 
									UCLID_FILEPROCESSINGLib::IFileRecordPtr ipNewRecord, 
									UCLID_FILEPROCESSINGLib::IFileRecordPtr ipOldRecord,
									bool bIsInvisible)
{
	// Only time a ipOldRecord can be NULL is if the from status is kActionUnattempted
	if (eFromStatus != kActionUnattempted && ipOldRecord == __nullptr)
	{
		UCLIDException ue("ELI17029", "Must have an old record");
		ue.addDebugInfo("FromStatus", eFromStatus);
		ue.addDebugInfo("ToStatus", eToStatus);
		ue.addDebugInfo("ActionID", nActionID);
		throw ue;
	}

	// Only time a ipNewRecord can be NULL is if the to status is kActionUnattempted
	if (eToStatus != kActionUnattempted && ipNewRecord == __nullptr)
	{
		UCLIDException ue("ELI17030", "Must have a new record");
		ue.addDebugInfo("FromStatus", eFromStatus);
		ue.addDebugInfo("ToStatus", eToStatus);
		ue.addDebugInfo("ActionID", nActionID);
		throw ue;
	}

	// Attempt to get the file data
	LONGLONG llOldFileSize(-1), llNewFileSize(-1);
	long lOldPages(-1), lNewPages(-1), lTempFileID(-1), lTempActionID(-1), lTempWorkflowID(-1);
	UCLID_FILEPROCESSINGLib::EFilePriority ePriority(
		(UCLID_FILEPROCESSINGLib::EFilePriority)kPriorityDefault);
	_bstr_t bstrTemp;
	if (ipOldRecord != __nullptr)
	{
		ipOldRecord->GetFileData(&lTempFileID, &lTempActionID, bstrTemp.GetAddress(),
			&llOldFileSize, &lOldPages, &ePriority, &lTempWorkflowID);
	}
	if (ipNewRecord != __nullptr)
	{
		// If the records are the same, just copy the data that was already retrieved
		if (ipNewRecord == ipOldRecord)
		{
			llNewFileSize = llOldFileSize;
			lNewPages = lOldPages;
		}
		else
		{
			ipNewRecord->GetFileData(&lTempFileID, &lTempActionID, bstrTemp.GetAddress(),
				&llNewFileSize, &lNewPages, &ePriority, &lTempWorkflowID);
		}
	}

	// Nothing to do if the "from" status == the "to" status
	if (eFromStatus == eToStatus)
	{
		// If the to and from status is unattempted there is nothing to do
		// Otherwise if the FileSize and the number Pages are the same there is nothing to do
		if (eFromStatus == kActionUnattempted ||
			(ipNewRecord != __nullptr && ipOldRecord != __nullptr &&
			llNewFileSize == llOldFileSize && 
			lNewPages == lOldPages))
		{
			return;
		}
	}
	
	// Initialize the differences
	long lNumDocsTotal(0), lNumPagesTotal(0);
	LONGLONG llNumBytesTotal(0);
	long lNumDocsFailed(0), lNumPagesFailed(0);
	LONGLONG llNumBytesFailed(0);
	long lNumDocsComplete(0), lNumPagesComplete(0);
	LONGLONG llNumBytesComplete(0);
	long lNumDocsSkipped(0), lNumPagesSkipped(0);
	LONGLONG llNumBytesSkipped(0);
	long lNumDocsPending(0), lNumPagesPending(0);
	LONGLONG llNumBytesPending(0);

	// get the changes for the Delta record
	switch (eToStatus)
	{
	case kActionFailed:
		{
			lNumDocsFailed++;
			lNumPagesFailed += lNewPages;
			llNumBytesFailed += llNewFileSize;
			break;
		}

	case kActionCompleted:
		{
			lNumDocsComplete++;
			lNumPagesComplete += lNewPages;
			llNumBytesComplete += llNewFileSize;
			break;
		}

	case kActionSkipped:
		{
			lNumDocsSkipped++;
			lNumPagesSkipped += lNewPages;
			llNumBytesSkipped += llNewFileSize;
			break;
		}
	case kActionPending:
		{
			lNumDocsPending++;
			lNumPagesPending += lNewPages;
			llNumBytesPending += llNewFileSize;
			break;
		}			
	}
	// Add the new counts to the totals if the to status is not unattempted
	if (eToStatus != kActionUnattempted)
	{
		lNumDocsTotal++;
		lNumPagesTotal += lNewPages;
		llNumBytesTotal += llNewFileSize;
	}

	switch (eFromStatus)
	{
	case kActionFailed:
		{
			lNumDocsFailed--;
			lNumPagesFailed -= lOldPages;
			llNumBytesFailed -= llOldFileSize;
			break;
		}

	case kActionCompleted:
		{
			lNumDocsComplete--;
			lNumPagesComplete -= lOldPages;
			llNumBytesComplete -= llOldFileSize;
			break;
		}

	case kActionSkipped:
		{
			lNumDocsSkipped--;
			lNumPagesSkipped -= lOldPages;
			llNumBytesSkipped -= llOldFileSize;
			break;
		}
	case kActionPending:
		{
			lNumDocsPending--;
			lNumPagesPending -= lOldPages;
			llNumBytesPending -= llOldFileSize;
			break;
		}
	}

	// Remove the counts from the totals if the from status is not unattempted
	if (eFromStatus != kActionUnattempted)
	{
		lNumDocsTotal--;
		lNumPagesTotal -= lOldPages;
		llNumBytesTotal -= llOldFileSize;
	}

	string strAddDeltaSQL;
	executeCmd(buildCmd(ipConnection,
		"INSERT INTO ActionStatisticsDelta ("
		"ActionID, Invisible, "
		"NumDocuments, NumDocumentsPending, NumDocumentsComplete, NumDocumentsFailed, NumDocumentsSkipped, "
		"NumPages, NumPagesPending, NumPagesComplete, NumPagesFailed, NumPagesSkipped, "
		"NumBytes, NumBytesPending, NumBytesComplete, NumBytesFailed, NumBytesSkipped) "
		"VALUES ("
		"@ActionID, @Invisible,"
		"@NumDocuments, @NumDocumentsPending, @NumDocumentsComplete, @NumDocumentsFailed, @NumDocumentsSkipped, "
		"@NumPages, @NumPagesPending, @NumPagesComplete, @NumPagesFailed, @NumPagesSkipped, "
		"@NumBytes, @NumBytesPending, @NumBytesComplete, @NumBytesFailed, @NumBytesSkipped)",
		{
			{ "@ActionID", nActionID },
			{ "@Invisible", (bIsInvisible ? 1 : 0) },
			{ "@NumDocuments", lNumDocsTotal },
			{ "@NumDocumentsPending", lNumDocsPending },
			{ "@NumDocumentsComplete", lNumDocsComplete },
			{ "@NumDocumentsFailed", lNumDocsFailed },
			{ "@NumDocumentsSkipped", lNumDocsSkipped },
			{ "@NumPages", lNumPagesTotal },
			{ "@NumPagesPending", lNumPagesPending },
			{ "@NumPagesComplete", lNumPagesComplete },
			{ "@NumPagesFailed", lNumPagesFailed },
			{ "@NumPagesSkipped", lNumPagesSkipped },
			{ "@NumBytes", llNumBytesTotal },
			{ "@NumBytesPending", llNumBytesPending },
			{ "@NumBytesComplete", llNumBytesComplete },
			{ "@NumBytesFailed", llNumBytesFailed },
			{ "@NumBytesSkipped", llNumBytesSkipped }
		}));
}
//--------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IActionStatisticsPtr CFileProcessingDB::loadStats(_ConnectionPtr ipConnection, 
	long nActionID, EWorkflowVisibility eWorkflowVisibility, bool bForceUpdate, bool bDBLocked)
{
	// Create a pointer to a recordset
	_RecordsetPtr ipActionStatSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI14099", ipActionStatSet != __nullptr);

	// Select the existing Statistics records if they exist
	string strSelectStat = gstrGET_ACTION_STATISTICS_FOR_ACTION;
	replaceVariable(strSelectStat, "<ActionIDWhereClause>", asString(nActionID));
	if (eWorkflowVisibility == EWorkflowVisibility::Invisible)
	{
		replaceVariable(strSelectStat, "<VisibilityWhereClause>", "AND [ActionStatistics].[Invisible] = 1");
	}
	else if (eWorkflowVisibility == EWorkflowVisibility::Visible)
	{
		replaceVariable(strSelectStat, "<VisibilityWhereClause>", "AND [ActionStatistics].[Invisible] = 0");
	}
	else
	{
		replaceVariable(strSelectStat, "<VisibilityWhereClause>", "");
	}

	// Open the recordset for the statistics with the records for ActionID if they exists
	ipActionStatSet->Open(strSelectStat.c_str(),
		_variant_t((IDispatch *)ipConnection, true), adOpenStatic,
		adLockOptimistic, adCmdText);

	if (asCppBool(ipActionStatSet->adoEOF))
	{
		if (bDBLocked)
		{
			reCalculateStats(ipConnection, nActionID);
			ipActionStatSet->Requery(adOptionUnspecified);
		}
		else
		{
			UCLIDException ue("ELI30976", "DB needs to be locked to calculate stats.");
			ue.addDebugInfo("ActionID", nActionID);
			throw ue;
		}
	}
	if (ipActionStatSet->adoEOF == VARIANT_TRUE)
	{
		UCLIDException ue("ELI14100", "Unable to load statistics.");
		ue.addDebugInfo("ActionID", nActionID);
		throw ue;
	}

	// Get the fields from the action stat set
	FieldsPtr ipFields = ipActionStatSet->Fields;
	ASSERT_RESOURCE_ALLOCATION("ELI26863", ipFields != __nullptr);

	// Check the last updated time stamp 
	CTime timeCurrent = getSQLServerDateTimeAsSystemTime(ipConnection);
	CTime timeLastUpdated = getTimeDateField(ipFields, "LastUpdateTimeStamp");
	CTimeSpan ts = timeCurrent - timeLastUpdated;
	if (bForceUpdate || ts.GetTotalSeconds() > m_nActionStatisticsUpdateFreqInSeconds)
	{
		if (bDBLocked)
		{
			// Need to update the ActionStatistics from the Delta table
			updateActionStatisticsFromDelta(ipConnection, nActionID);

			ipActionStatSet->Requery(adOptionUnspecified);

			ipFields = ipActionStatSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI30751", ipFields != __nullptr);
		}
		else
		{
			UCLIDException  ue("ELI30977", "DB needs to be locked to update stats.");
			ue.addDebugInfo("ActionID", nActionID);
			throw ue;
		}
	}
	else
	{
		// [LegacyRCAndUtils:6233]
		// If m_nActionStatisticsUpdateFreqInSeconds has not expired since the last update,
		// calculate stats using a query that aggregates all the values in the ActionStatisicsDelta
		// table ActionStatistics so that m_nActionStatisticsUpdateFreqInSeconds can be a large
		// value. On a stressed DB, this has very minimal cost compared to performing the locking
		// that is necessary for updateActionStatisticsFromDelta, even when the delta table has
		// tens of thousands of records.
		ipActionStatSet->Close();

		string strCalcStat = gstrCALCULATE_ACTION_STATISTICS_FOR_ACTION;
		replaceVariable(strCalcStat, "<ActionIDWhereClause>", asString(nActionID));
		if (eWorkflowVisibility == EWorkflowVisibility::Invisible)
		{
			replaceVariable(strCalcStat, "<VisibilityWhereClause>", "AND [ActionStatistics].[Invisible] = 1");
		}
		else if (eWorkflowVisibility == EWorkflowVisibility::Visible)
		{
			replaceVariable(strCalcStat, "<VisibilityWhereClause>", "AND [ActionStatistics].[Invisible] = 0");
		}
		else
		{
			replaceVariable(strCalcStat, "<VisibilityWhereClause>", "");
		}

		ipActionStatSet->Open(strCalcStat.c_str(), 
			_variant_t((IDispatch *)ipConnection, true), adOpenStatic, adLockOptimistic, adCmdText);

		ipFields = ipActionStatSet->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI34152", ipFields != __nullptr);
	}

	// Get all the data from the recordset
	long lNumDocsFailed =  getLongField(ipFields, "NumDocumentsFailed");
	long lNumPagesFailed = getLongField(ipFields, "NumPagesFailed");
	LONGLONG llNumBytesFailed = getLongLongField(ipFields, "NumBytesFailed");
	long lNumDocsSkipped =  getLongField(ipFields, "NumDocumentsSkipped");
	long lNumPagesSkipped = getLongField(ipFields, "NumPagesSkipped");
	LONGLONG llNumBytesSkipped = getLongLongField(ipFields, "NumBytesSkipped");
	long lNumDocsComplete = getLongField(ipFields, "NumDocumentsComplete");
	long lNumPagesComplete = getLongField(ipFields, "NumPagesComplete");
	LONGLONG llNumBytesComplete = getLongLongField(ipFields, "NumBytesComplete");
	long lNumDocs = getLongField(ipFields, "NumDocuments");
	long lNumPages = getLongField(ipFields, "NumPages");
	LONGLONG llNumBytes = getLongLongField(ipFields, "NumBytes");
	long lNumDocsPending = getLongField(ipFields, "NumDocumentsPending");
	long lNumPagesPending = getLongField(ipFields, "NumPagesPending");
	LONGLONG llNumBytesPending = getLongLongField(ipFields, "NumBytesPending");

	// Create an ActionStatistics pointer to return the values
	UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats(CLSID_ActionStatistics);
	ASSERT_RESOURCE_ALLOCATION("ELI14101", ipActionStats != __nullptr);

	// Transfer the data from the recordset to the ActionStatisticsPtr
	ipActionStats->SetAllStatistics(lNumDocs, lNumDocsPending, lNumDocsComplete, lNumDocsFailed, 
		lNumDocsSkipped, lNumPages, lNumPagesPending, lNumPagesComplete, lNumPagesFailed, 
		lNumPagesSkipped, llNumBytes, llNumBytesPending, llNumBytesComplete, llNumBytesFailed, 
		llNumBytesSkipped);

	m_timeLastStatsCheck = CTime::GetCurrentTime();

	// Return the ActionStats pointer,
	return ipActionStats;
}
//--------------------------------------------------------------------------------------------------
int CFileProcessingDB::getDBSchemaVersion()
{
	// if the value of the SchemaVersion is not 0 then it has already been read from the db
	if (m_iDBSchemaVersion != 0)
	{
		return m_iDBSchemaVersion;
	}

	// Get all of the settings from the DBInfo
	loadDBInfoSettings();

	// if the Schema version is still 0 there is a problem
	if (m_iDBSchemaVersion == 0)
	{
		UCLIDException ue("ELI14775", "Unable to obtain DB Schema Version.");
		throw ue;
	}

	// return the Schema version
	return m_iDBSchemaVersion;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::validateDBSchemaVersion(bool bCheckForUnaffiliatedFiles/* = false */)
{
	// If in the process of checking the database schema or during an update operation, do not
	// validate the database schema; this would cause the schema update to fail or infinite recursion.
	if (!m_bValidatingOrUpdatingSchema)
	{
		// Prevent recursion when the product specific DBs check their version.
		ValueRestorer<volatile bool> restorer(m_bValidatingOrUpdatingSchema, false);
		m_bValidatingOrUpdatingSchema = true;

		// Get the Schema Version from the database
		int iDBSchemaVersion = getDBSchemaVersion();
		if (iDBSchemaVersion != ms_lFAMDBSchemaVersion)
		{
			// Update the current connection status string
			m_strCurrentConnectionStatus = gstrWRONG_SCHEMA;

			UCLIDException ue("ELI14380", "DB Schema version does not match.");
			ue.addDebugInfo("SchemaVer in Database", iDBSchemaVersion);
			ue.addDebugInfo("SchemaVer expected", ms_lFAMDBSchemaVersion);
            ue.addDebugInfo("Database server", m_strDatabaseServer);
            ue.addDebugInfo("Database Name", m_strDatabaseName);
			throw ue;
		}

		// If we have yet to flag all the product specific DBs as valid, attempt to validate each.
		if (!m_bProductSpecificDBSchemasAreValid)
		{
			// Get a list of all installed & licensed product-specific database managers.
			IIUnknownVectorPtr ipProdSpecificMgrs = getLicensedProductSpecificMgrs();
			ASSERT_RESOURCE_ALLOCATION("ELI31433", ipProdSpecificMgrs != __nullptr);

			// Loop through the managers and validate the schema of each.
			long nCountProdSpecMgrs = ipProdSpecificMgrs->Size();
			for (long i = 0; i < nCountProdSpecMgrs; i++)
			{
				UCLID_FILEPROCESSINGLib::IProductSpecificDBMgrPtr ipProdSpecificDBMgr =
					ipProdSpecificMgrs->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI31434", ipProdSpecificDBMgr != __nullptr);

				try
				{
					ipProdSpecificDBMgr->ValidateSchema(getThisAsCOMPtr());
				}
				catch (...)
				{
					// Update the current connection status string
					m_strCurrentConnectionStatus = gstrWRONG_SCHEMA;
					throw;
				}
			}

			// If we reached this point without and exception being thrown, all installed and
			// licensed product specific DB components have up-to-data schema versions.
			m_bProductSpecificDBSchemasAreValid = true;
		}

		if (bCheckForUnaffiliatedFiles && unaffiliatedWorkflowFilesExist())
		{
			m_strCurrentConnectionStatus = gstrUNAFFILIATED_FILES;
			throw UCLIDException("ELI43450", "Workflows exist, but there are unaffiliated files.");
		}
	}
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::unaffiliatedWorkflowFilesExist()
{
	auto role = getAppRoleConnection();
	_ConnectionPtr ipConnection = role->ADOConnection();
	ASSERT_RESOURCE_ALLOCATION("ELI43449", ipConnection != __nullptr);

	if (databaseUsingWorkflows(ipConnection))
	{
		string strQuery =
			"SELECT COALESCE(MAX([ID]), -1) AS [ID] FROM \r\n"
			"( \r\n"
			"	SELECT TOP 1 [ID] FROM [FAMFile] \r\n"
			"	LEFT JOIN [WorkflowFile] ON [FAMFile].[ID] = [FileID] \r\n"
			"	WHERE [WorkflowID] IS NULL \r\n"
			") T";
		long nUnaffiliatedFileId = -1;
		executeCmdQuery(ipConnection, strQuery, false, &nUnaffiliatedFileId);

		if (nUnaffiliatedFileId != -1)
		{
			// https://extract.atlassian.net/browse/ISSUE-15404
			// There seems to be some timing related issues with files getting assigned to workflows
			// I don't immediately understand. In addition to only executing this check from FAMDBAdmin,
			// upon finding an apparent unaffiliated file, we should re-check after a slight
			// pause to ensure the file detected as unaffiated still is.
			Sleep(500);

			strQuery =
				"	SELECT COALESCE(MAX([ID]), -1) AS [ID] FROM [FAMFile] \r\n"
				"	LEFT JOIN [WorkflowFile] ON [FAMFile].[ID] = [FileID] \r\n"
				"	WHERE [ID] = @UnaffiliatedFileID \r\n"
				"	AND [WorkflowID] IS NULL";

			long nDoubleCheckFileId = -1;
			getCmdId(buildCmd(ipConnection, strQuery,
				{ {"@UnaffiliatedFileID", nUnaffiliatedFileId} }), &nDoubleCheckFileId);

			return (nUnaffiliatedFileId == nDoubleCheckFileId);
		}
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::setActiveAction(_ConnectionPtr ipConnection, const string& strActionName)
{
	try
	{
		try
		{
			string strActiveWorkflow = getActiveWorkflow();
			int actionID = getActionID(ipConnection, strActionName, strActiveWorkflow);
			m_nActiveActionID = actionID;

			long nWorkflowActionCount = 0;
			string strWorkflowActionQuery =
				"SELECT COUNT(*) AS [ID] FROM [Action] WHERE [ASCName] = @ActionName AND [WorkflowID] IS NOT NULL";
			getCmdId(buildCmd(ipConnection, strWorkflowActionQuery, { {"@ActionName", strActionName.c_str()} }),
				&nWorkflowActionCount);
			
			m_bUsingWorkflowsForCurrentAction = (nWorkflowActionCount > 0);
			m_bRunningAllWorkflows = (m_bUsingWorkflowsForCurrentAction && strActiveWorkflow == "");
			m_nProcessStart = 0;
			m_vecActionsProcessOrder.clear();

			if (m_bRunningAllWorkflows)
			{
				loadActionsProcessOrder(ipConnection, strActionName);
			}
			else
			{
				m_vecActionsProcessOrder.push_back(actionID);
			}

			// is this really needed here? This should probably be some DB consistancy check done when first connecting to a database
			if (m_bUsingWorkflowsForCurrentAction)
			{
				long nExternalWorkflowFiles = 0;
				string strExternalFilesQuery =
					"SELECT COUNT(*) AS [ID] FROM [FileActionStatus] WHERE [ActionID] = @ActionID";
				getCmdId(buildCmd(ipConnection, strExternalFilesQuery, 
					{ {"@ActionID", getActionID(ipConnection, strActionName, "")} })
					, &nExternalWorkflowFiles);

				if (nExternalWorkflowFiles > 0)
				{
					UCLIDException ue("ELI42092", "Error initializing FAM session; "
						"database files exist external to defined workflows.");
					ue.addDebugInfo("Action", strActionName);
					throw ue;
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42093")
	}
	catch (UCLIDException& ue)
	{
		m_nActiveActionID = -1;

		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::loadActionsProcessOrder(_ConnectionPtr ipConnection, const string& strActionName)
{
	string strActionIDs = getActionIDsForActiveWorkflow(ipConnection, strActionName);

	// Tokenize by either comma, semicolon, or pipe
	vector<string> vecTokens;
	m_vecActionsProcessOrder.clear();
	StringTokenizer::sGetTokens(strActionIDs, ",;|", vecTokens, true);
	for (size_t i = 0; i < vecTokens.size(); i++)
	{
		long actionID = asLong(vecTokens[i]);
		long workflowID = getWorkflowID(ipConnection, actionID);
		UCLID_FILEPROCESSINGLib::IWorkflowDefinitionPtr workflowDef = __nullptr;
		if (workflowID > 0)
		{
			workflowDef = getWorkflowDefinition(ipConnection, workflowID);
			for (int n = 0; n < workflowDef->LoadBalanceWeight; n++)
			{
				m_vecActionsProcessOrder.push_back(actionID);
			}
		}

	}
	if (m_vecActionsProcessOrder.size() > 1)
		shuffleVector(m_vecActionsProcessOrder);
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::lockDB(_ConnectionPtr ipConnection, const string& strLockName)
{
	// https://extract.atlassian.net/browse/ISSUE-13910
	// Since the schema of the LockTable can and has changed over time, we cannot assume this
	// function will work on an older database. However, since our locking system only really matters
	// when 2 processes collide and since 2 process should not be allowed when updating the schema,
	// locking should be disabled during a schema update.
	if (m_bValidatingOrUpdatingSchema)
	{
		return;
	}

	// https://extract.atlassian.net/browse/ISSUE-12328
	// If we can see that this thread already has a lock on this thread, throw an exception rather
	// than allow a deadlock situation to occur.
	if (FAMDBSemaphore::ThisThreadHasLock(strLockName))
	{
		throw UCLIDException("ELI37214", "Re-entrant DB lock attempted.");
	}

	int nMaxWaitTime = -1;

	// Lock insertion string for this process
	_CommandPtr cmdAddLock = buildCmd(ipConnection,
		"INSERT INTO LockTable (LockName, UPI) VALUES (@LockName, @UPI)",
		{ 
			{ "@LockName", strLockName.c_str() },
			{ "@UPI", m_strUPI.c_str() } 
		});
	_CommandPtr cmdDeleteLock = buildCmd(ipConnection, gstrDELETE_DB_LOCK,
		{ { gstrDB_LOCK_NAME_VAL, strLockName.c_str() } });
	_CommandPtr cmdGetLock = buildCmd(ipConnection, gstrDB_LOCK_QUERY,
		{ { gstrDB_LOCK_NAME_VAL, strLockName.c_str() } });

	// Keep trying to lock the DB until it is locked by this thread
	while (true)
	{
		// Flag to indicate if the connection is in a good state
		// this will be determined if the TransactionGuard does not throw an exception
		// this needs to be initialized each time through the loop
		bool bConnectionGood = false;

		// put this in a try catch block to catch the possibility that another 
		// instance is trying to lock the DB at exactly the same time
		try
		{
			try
			{
				// [LegacyRCAndUtils:6120]
				// Multiple threads should not be able to share the same DB lock. If the database is
				// currently locked, wait until it is unlocked before attempting to lock it on this
				// thread.
				if (!FAMDBSemaphore::Lock(strLockName, m_lDBLockTimeout * 1000))
				{
					throw UCLIDException("ELI34039", "Timeout waiting for DB lock.");
				}

				// Lock while updating the lock table and m_bDBLocked variable
				CSingleLock lock2(&m_criticalSection, TRUE);

				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				// Create a pointer to a recordset
				_RecordsetPtr ipLockTable(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI14550", ipLockTable != __nullptr);

				try
				{
					// Open recordset with the locktime 
					ipLockTable->Open((IDispatch*)cmdGetLock, vtMissing, adOpenStatic,
						adLockReadOnly, adCmdText);
				}
				catch (const std::exception& e)
				{
					UCLIDException ue("ELI51737", "Exception getting lock");
					ue.addDebugInfo("ExceptionText", e.what());
					throw ue;
				}

				// If we get to here the db connection should be valid
				bConnectionGood = true;

				// If there is an existing record check to see if the time to see if the lock
				// has been on for more than the lock timeout
				if (ipLockTable->adoEOF == VARIANT_FALSE)
				{
					// We already have the lock; nothing to do.
					string strExistingLock = getStringField(ipLockTable->Fields, "UPI");
					if (m_strUPI == strExistingLock)
					{
						break;
					}

					// Get the time locked value from the record 
					long nSecondsLocked = getLongField(ipLockTable->Fields, "TimeLocked"); 
					if (nSecondsLocked > m_lDBLockTimeout)
					{
						// Delete the lock record since it has been in the db for
						// more than the lock period
						executeCmd(cmdDeleteLock);

						// commit the changes
						// this may throw an exception if another instance gets here
						// at the same time
						tg.CommitTrans();

						// log an exception that the lock has been reset
						UCLIDException ue("ELI15406",
							"Application trace: Lock timed out. Lock has been reset.");
						ue.addDebugInfo("Lock Name", strLockName);
						ue.addDebugInfo ("Lock Timeout", m_lDBLockTimeout);
						ue.addDebugInfo ("Actual Lock Time", asString(nSecondsLocked));
						ue.log();

						// [FlexIDSCore:4911]
						// Unlock the semaphore since we don't yet have the DB lock and are about to
						// restart this loop.
						// NOTE: Not sure if the loop does need to be restarted anymore, but since
						// I don't know the reason it was coded this way, I don't want to change it.
						FAMDBSemaphore::Unlock(strLockName);

						// Restart the loop since we don't want to assume this instance will 
						// get the lock
						continue;
					}

					throw UCLIDException("ELI34153", "Another process has the lock.");
				}

				try
				{
					// Add the lock
					executeCmd(cmdAddLock);

				}
				catch (const std::exception& e)
				{
					UCLIDException ue("ELI51738", "Exception adding lock.");
					ue.addDebugInfo("ExceptionText", e.what());
					throw ue;
				}
				try
				{
					// [LegacyRCAndUtils:6154]
					// If this thread has the lock, but collides with another thread which is not locked
					// over a database resource, the thread that is locked should win the deadlock
					// (cause the unlocked thread to be chosen as the deadlock victim).
					executeCmdQuery(ipConnection, "SET DEADLOCK_PRIORITY HIGH");

				}
				catch (const std::exception& e)
				{
					UCLIDException ue("ELI51739", "Exception Setting deadlock priority.");
					ue.addDebugInfo("ExceptionText", e.what());
					throw ue;
				}
				// Commit the changes
				// If a DB lock is in the table for another process this will throw an exception
				tg.CommitTrans();

				// Lock obtained, break from the loop
				break;
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI14973");
		}
		catch(UCLIDException &ue)
		{
			if (!FAMDBSemaphore::ThisThreadHasLock(strLockName))
			{
				// If we timed out getting the semaphore lock, throw the time out exception.
				throw;
			}

			// [LegacyRCAndUtils:5934]
			// To avoid an error that can be displayed if the FAM is stopped while attempting to
			// lock the DB, check if m_mapThreadIDtoDBConnections is empty which indicates
			// closeAllDBConnections has been called since this method was first called.
			bool bAllConnectionsClosed = false;
			if (!bConnectionGood)
			{
				CSingleLock lock(&m_criticalSection, TRUE);
				bAllConnectionsClosed = (m_mapThreadIDtoDBConnections.size() == 0);
			}

			// Ensure the semaphore lock is released if we had it.
			FAMDBSemaphore::Unlock(strLockName);

			// If all connections have been intentionally closed, return without error.
			if (bAllConnectionsClosed)
			{
				return;
			}

			// if the bConnectionGood flag is false the exception should be thrown
			if (!bConnectionGood) 
			{
				UCLIDException uexOuter("ELI15459", "Connection is no longer good", ue);
				postStatusUpdateNotification(kConnectionNotEstablished);
				throw uexOuter;
			}
		};

		// Determine the range of possible wait times for each attempt at getting the lock based
		// upon how many active FAMs there are. The more active FAMs, the greater the likelihood
		// that multiple processes are hitting the DB in this loop.
		if (nMaxWaitTime < 0)
		{
			try
			{
				_RecordsetPtr ipActiveFAMs(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI34154", ipActiveFAMs != __nullptr);

				// Retrieve the count of active FAMs
				ipActiveFAMs->Open("SELECT COUNT(*) AS [FAMCount] FROM [ActiveFAM]", 
					_variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
					adLockReadOnly, adCmdText);

				ipActiveFAMs->MoveFirst();

				nMaxWaitTime = 50 + (10 * getLongField(ipActiveFAMs->Fields, "FAMCount"));
			}
			catch (...)
			{
				// If the check for the number of processing FAMs fails for any reason, just
				// use 50 ms as the max.
				nMaxWaitTime = 50;
			}
		}
		
		// Wait a random time from 0 to nMaxWaitTime
		unsigned int iRandom;
		rand_s(&iRandom);
		Sleep(iRandom % nMaxWaitTime);
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::unlockDB(_ConnectionPtr ipConnection, const string& strLockName)
{
	try
	{
		try
		{
			// Restore normal deadlock priority.
			executeCmdQuery(ipConnection, "SET DEADLOCK_PRIORITY NORMAL");

			// Delete the Lock record
			string strDeleteSQL = gstrDELETE_DB_LOCK + " AND UPI = @UPI";
			executeCmd(
				buildCmd(ipConnection, strDeleteSQL,
					{
						{ gstrDB_LOCK_NAME_VAL, strLockName.c_str() } ,
						{ "@UPI", m_strUPI.c_str() } 
					}));
			
			// This releases the semaphore and allows the next thread in.
			FAMDBSemaphore::Unlock(strLockName);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34041");
	}
	catch (UCLIDException &ue)
	{
		// Even if we failed to remove the lock table entry, allow the next thread to proceed.
		FAMDBSemaphore::Unlock(strLockName);

		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::lockDBTableForTransaction(_ConnectionPtr ipConnection,
	const string& strTableName)
{
	// Using (TABLOCKX, XLOCK) prevents any access to the table from other sessions.
	executeCmdQuery(ipConnection, "SELECT TOP 1 * FROM " + strTableName + " WITH (TABLOCKX, XLOCK)");
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::getEncryptedPWFromDB(string &rstrEncryptedPW, bool bUseAdmin)
{
	try
	{
		// Open the Login Table
		// Lock the mutex for this instance
		CSingleLock lock(&m_criticalSection, TRUE);

		// Create a pointer to a recordset
		_RecordsetPtr ipLoginSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI15103", ipLoginSet != __nullptr);

		auto role = getAppRoleConnection();
		auto ipConnection = role->ADOConnection();

		// setup the SQL Query to get the encrypted combo for admin or user
		string strSQL = "SELECT * FROM LOGIN WHERE UserName = @UserName ";
		auto cmd = buildCmd(ipConnection, strSQL, { {"@UserName",
			((bUseAdmin) ? gstrADMIN_USER.c_str() : m_strFAMUserName.c_str())} });

		// Open the set for the user being logged in
		ipLoginSet->Open((IDispatch*)cmd, vtMissing, adOpenStatic, adLockReadOnly, adCmdText);

		// user was in the DB if not at the end of file
		if (ipLoginSet->adoEOF == VARIANT_FALSE)
		{
			// Return the encrypted password that is stored in the DB
			rstrEncryptedPW = getStringField(ipLoginSet->Fields, "Password");
			return true;
		}

		// record not found in DB for user or admin
		rstrEncryptedPW = "";
		return false;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29897");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::encryptAndStoreUserNamePassword(const string& strUser,
														const string& strPassword,
														bool bFailIfUserDoesNotExist)
{
	// Get the encrypted version of the combined string
	string strEncryptedCombined = getEncryptedString(1, strUser + strPassword);

	storeEncryptedPasswordAndUserName(strUser, strEncryptedCombined, bFailIfUserDoesNotExist);
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::storeEncryptedPasswordAndUserName(const string& strUser,
														  const string& strEncryptedPW,
														  bool bFailIfUserDoesNotExist,
														  bool bCreateTransactionGuard)
{
	// Lock the mutex for this instance
	CSingleLock lock(&m_criticalSection, TRUE);

	// Create a pointer to a recordset
	_RecordsetPtr ipLoginSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI15722", ipLoginSet != __nullptr);

	auto role = getAppRoleConnection();

	// Begin Transaction if needed
	unique_ptr<TransactionGuard> apTg;
	if (bCreateTransactionGuard)
	{
		apTg.reset(new TransactionGuard(role->ADOConnection(), adXactChaos, __nullptr));
		ASSERT_RESOURCE_ALLOCATION("ELI29896", apTg.get() != __nullptr);
	}

	// Retrieve records from Login table for the admin or current user
	string strSQL = "SELECT * FROM LOGIN WHERE UserName = @UserName";
	auto cmd = buildCmd(role->ADOConnection(), strSQL, { {"@UserName", strUser.c_str()} });
	ipLoginSet->Open((IDispatch*)cmd, vtMissing,
		adOpenDynamic, adLockPessimistic, adCmdText);

	// User not in DB if at the end of file
	if (ipLoginSet->adoEOF == VARIANT_TRUE)
	{
		if (bFailIfUserDoesNotExist)
		{
			UCLIDException ue("ELI30012", "The specified user was not found, cannot set password.");
			ue.addDebugInfo("User Name", strUser);
			throw ue;
		}

		// Insert a new record
		ipLoginSet->AddNew();

		// Set the UserName field
		setStringField(ipLoginSet->Fields, "UserName", strUser);
	}

	// Update the password field
	setStringField(ipLoginSet->Fields, "Password", strEncryptedPW);
	ipLoginSet->Update();

	// Commit the changes
	if (apTg.get() != __nullptr)
	{
		apTg->CommitTrans();
	}
}
//--------------------------------------------------------------------------------------------------
template <typename T>
string CFileProcessingDB::getEncryptedString(size_t nCount, const T input, ...)
{
	// Put the input string into the byte manipulator
	ByteStream bytes;
	ByteStreamManipulator bytesManipulator(ByteStreamManipulator::kWrite, bytes);
	
	va_list vaList;
	va_start(vaList, nCount);
	for (size_t i = 0; i < nCount; i++)
	{
		bytesManipulator << va_arg(vaList, T);
	}
	va_end(vaList);

	// Convert information to a stream of bytes
	// with length divisible by 8 (in variable called 'bytes')
	bytesManipulator.flushToByteStream(8);

	// Get the password 'key' based on the 4 hex global variables
	ByteStream pwBS;
	getFAMPassword(pwBS);

	// Do the encryption
	ByteStream encryptedBS;
	MapLabel encryptionEngine;
	encryptionEngine.setMapLabel(encryptedBS, bytes, pwBS);

	// Return the encrypted value
	return encryptedBS.asString();
}
//--------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr CFileProcessingDB::getThisAsCOMPtr()
{
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipThis;
	ipThis = this;
	ASSERT_RESOURCE_ALLOCATION("ELI17015", ipThis != __nullptr);
	return ipThis;
}
//--------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileRecordPtr CFileProcessingDB::getFileRecordFromFields(
	const FieldsPtr& ipFields, bool bGetPriority)
{
	// Make sure the ipFields argument is not NULL
	ASSERT_ARGUMENT("ELI17028", ipFields != __nullptr);

	UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(CLSID_FileRecord);
	ASSERT_RESOURCE_ALLOCATION("ELI17027", ipFileRecord != __nullptr);
	
	// Depending on context, the query may not have returned ActionID or WorkflowID;
	// default to 0 if missing.
	ipFileRecord->SetFileData(getLongField(ipFields, "ID"), getLongField(ipFields, "ActionID", 0),
		getStringField(ipFields, "FileName").c_str(), getLongLongField(ipFields, "FileSize"),
		getLongField(ipFields, "Pages"), (UCLID_FILEPROCESSINGLib::EFilePriority)
		(bGetPriority ? getLongField(ipFields, "Priority") : 0), getLongField(ipFields, "WorkflowID", 0));

	return ipFileRecord;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::setFieldsFromFileRecord(const FieldsPtr& ipFields, 
		const UCLID_FILEPROCESSINGLib::IFileRecordPtr& ipFileRecord, bool bSetPriority)
{
	// Make sure the ipFields argument is not NULL
	ASSERT_ARGUMENT("ELI17031", ipFields != __nullptr);

	// Make sure the ipFileRecord object is not NULL
	ASSERT_ARGUMENT("ELI17032", ipFileRecord != __nullptr);
	
	// Get the file data
	long lFileID(-1), lActionID(-1), lNumPages(-1), lWorkflowID(-1);
	LONGLONG llFileSize(-1);
	UCLID_FILEPROCESSINGLib::EFilePriority ePriority(
		(UCLID_FILEPROCESSINGLib::EFilePriority)kPriorityDefault);
	_bstr_t bstrFileName;
	ipFileRecord->GetFileData(&lFileID, &lActionID, bstrFileName.GetAddress(),
		&llFileSize, &lNumPages, &ePriority, &lWorkflowID);

	// set the file name field
	setStringField(ipFields, "FileName", asString(bstrFileName));

	// Set the file Size
	setLongLongField(ipFields, "FileSize", llFileSize);

	// Set the number of pages
	setLongField(ipFields, "Pages", lNumPages);

	if (bSetPriority)
	{
		// Set the priority
		setLongField(ipFields, "Priority",
			(ePriority == kPriorityDefault ? glDEFAULT_FILE_PRIORITY : (long) ePriority));
	}
}
//--------------------------------------------------------------------------------------------------
bool  CFileProcessingDB::isPasswordValid(const string& strPassword, bool bUseAdmin)
{
	// Set the user to validate
	string  strUser = bUseAdmin ? gstrADMIN_USER : m_strFAMUserName;
	
	// Make combined string for comparison
	string strCombined = strUser + strPassword;
	
	// Get the stored password (if it exists)
	string strStoredEncryptedCombined;
	if (!getEncryptedPWFromDB(strStoredEncryptedCombined, bUseAdmin))
	{
		UCLIDException uex("ELI30013",
			"The specified user was not found, cannot authenticate password.");
		uex.addDebugInfo("User Name", strUser);
		throw uex;
	}

	// Check for no password
	if (strStoredEncryptedCombined.empty())
	{
		// if there is no stored password then strPassword should be empty
		return strPassword.empty();
	}

	// Get the password 'key' based on the 4 hex global variables
	ByteStream pwBS;
	getFAMPassword(pwBS);

	// Stream to hold the decrypted PW
	ByteStream decryptedPW;

	// Decrypt the stored, encrypted PW
	MapLabel encryptionEngine;
	encryptionEngine.getMapLabel(decryptedPW, strStoredEncryptedCombined, pwBS);
	ByteStreamManipulator bsm(ByteStreamManipulator::kRead, decryptedPW);

	// Get the decrypted combined username and password from byte stream
	string strDecryptedCombined = "";
	bsm >> strDecryptedCombined;

	// Since the username is not case sensitive and the password is, will need to separate them

	// Extract the user from the decrypted username password combination using the expected
	// username length
	int iUserNameSize = strUser.length();

	// Successful login if decrypted user matches the expected and the decrypted password matches 
	// the expected password
	return (stringCSIS::sEqual(strUser, strDecryptedCombined.substr(0, iUserNameSize)) &&
		strDecryptedCombined.substr(iUserNameSize) == strCombined.substr(iUserNameSize));
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::authenticateOneTimePassword(const string& strPassword)
{
	// One-time passwords must be relative to an active FAMSession started for FPSFileName "<Admin>"
	// within the past minute for the active user/machine. One-time passwords are limited by the
	// current Windows machine/user rather than a user that may have been specified via LoginUser
	string strQueryForSession =
		"SELECT TOP 1 [FAMSession].[ID]"
		" FROM dbo.[FAMSession]"
		" JOIN dbo.[Machine] ON [MachineID] = [Machine].[ID]"
		" JOIN dbo.[FAMUser] ON [FAMUserID] = [FAMUser].[ID]"
		" JOIN dbo.[FPSFile] ON [FPSFileID] = [FPSFile].[ID]"
		" WHERE [StopTime] IS NULL"
		"  AND [FPSFileName] = @FPSFileName "
		"  AND [MachineName] = @MachineName "
		"  AND [UserName] = @UserName "
		"  AND [StartTime] > DATEADD(minute, -1, GETDATE()) "
		" ORDER BY [FAMSession].[ID] DESC";
	
	try
	{
		try
		{
			auto role = getAppRoleConnection();
			_ConnectionPtr ipConnection = role->ADOConnection();
			long nFAMSessionID = 0;

			auto cmd = buildCmd(ipConnection, strQueryForSession,
				{
					{"@FPSFileName", gstrONE_TIME_ADMIN_USER.c_str()},
					{"@MachineName", m_strMachineName.c_str()},
					{"@UserName", getCurrentUserName().c_str()}
				});
			getCmdId(cmd, &nFAMSessionID);

			// If an open session was found for a one-time password, initialize m_nFAMSessionID and
			// m_bLoggedInAsAdmin, then validate the password is what it should be for the session.
			m_nFAMSessionID = nFAMSessionID;
			m_bLoggedInAsAdmin = true;

			string strExpectedPassword = getOneTimePassword(ipConnection);

			ASSERT_RUNTIME_CONDITION("ELI49833", strPassword == strExpectedPassword, "Invalid password");

			// If the password was valid, close out m_nFAMSessionID. This will prevent the password from
			// being used again.
			getThisAsCOMPtr()->RecordFAMSessionStop();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49835")
	}
	catch (UCLIDException &ue)
	{
		// Authentication failed. Ensure m_nFAMSessionID and m_bLoggedInAsAdmin are reset.
		m_nFAMSessionID = 0;
		m_bLoggedInAsAdmin = false;

		UCLIDException ueOuter("ELI49839", "Authentication failed", ue);

		throw ueOuter;
	}
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::getOneTimePassword(_ConnectionPtr ipConnection)
{
	ASSERT_RUNTIME_CONDITION("ELI49834", m_bLoggedInAsAdmin && m_nFAMSessionID > 0,
		"Not authorized to generate password");

	checkDatabaseIDValid(ipConnection, false, false);
	
	// Generate an encrypted string specific to this database and m_nFAMSessionID.
	string strEncrypted = getEncryptedString(
		2, (long)m_nFAMSessionID, m_DatabaseIDValues.m_nHashValue);
	
	return strEncrypted;
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::isExistingDB()
{
	_ConnectionPtr dbConnection(__uuidof(Connection)); 

	try
	{
		try
		{
			dbConnection->Open(createConnectionString(m_strDatabaseServer, "master").c_str(), "", "", adConnectUnspecified);

			string dbExistsQuery = "IF DB_ID('" + m_strDatabaseName + "') IS NOT NULL SELECT 1";
			_RecordsetPtr results = dbConnection->Execute(dbExistsQuery.c_str(), NULL, adCmdText);

			// The query returns a closed recordset if the DB does not exist but check for not EOF just in case
			bool dbExists = ((results->State & adStateOpen) >  0) && !results->adoEOF;

			dbConnection->Close();

			return dbExists;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49935")
	}
	catch (UCLIDException &ex)
	{
		try
		{
			dbConnection->Close();
		}
		catch (...) { }

		throw ex;
	}
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::isBlankDB()
{
	try
	{
		// A blank DB will not be able to provide an app role connection. Use kNoRole for this check.
		ValueRestorer<AppRole> applicationRoleRestorer(m_currentRole);
		m_currentRole = AppRole::kNoRole;

		_ConnectionPtr ipConnection;
		auto role = getAppRoleConnection();
		try
		{
			ipConnection = role->ADOConnection();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49933");

		//Check if user has permission to view the database definition
		auto ipPermission = ipConnection->Execute(
			"SELECT entity_name, permission_name "
			"	FROM fn_my_permissions(NULL, 'Database') "
			"	WHERE permission_name = 'VIEW DEFINITION'", __nullptr, adCmdUnspecified);
		if (ipPermission->adoEOF)
		{
			UCLIDException ue("ELI53027", "User does not have sufficient permissions to view the definition.");
			throw ue;
		}
		ipPermission->Close();
		ipPermission = __nullptr;


		// Get the tables that exist in the database
		_RecordsetPtr ipTables = ipConnection->OpenSchema(adSchemaTables);

		// Set blank flag to true
		bool bBlank = true;

		// Go thru all of the tables
		while (!asCppBool(ipTables->adoEOF))
		{
			// Get the Table Type
			string strType = getStringField(ipTables->Fields, "TABLE_TYPE");

			// Only need to look at the tables (no system tables or views)
			if (strType == "TABLE")
			{
				// There is at least one non system table
				bBlank = false;
				break;
			}

			// Get next table
			ipTables->MoveNext();
		}

		return bBlank;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33400");
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::initializeDB(bool initWithoutPrompt, string strAdminPassword)
{
	try
	{
		if (initWithoutPrompt)
		{
			clear(false, true, false);

			encryptAndStoreUserNamePassword(gstrADMIN_USER, strAdminPassword, false);

			return true;
		}
		else
		{
			// Default to using the desktop as the parent for the messagebox below
			HWND hParent = getAppMainWndHandle();

			int iResult = ::MessageBox(hParent,
				"This database exists but has not been initialized for use.\r\n\r\n"
				"Do you wish to initialize it now?", "Initialize Database?", MB_YESNO);
			if (iResult == IDYES)
			{
				clear(false, true, false);

				return true;
			}

			return false;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33401");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::getExpectedTables(std::vector<string>& vecTables)
{
	// Add Tables
	vecTables.push_back(gstrACTION);
	vecTables.push_back(gstrACTION_STATE);
	vecTables.push_back(gstrACTION_STATISTICS);
	vecTables.push_back(gstrACTION_STATISTICS_DELTA);
	vecTables.push_back(gstrDB_INFO);
	vecTables.push_back(gstrFAM_FILE);
	vecTables.push_back(gstrFILE_ACTION_STATE_TRANSITION);
	vecTables.push_back(gstrLOCK_TABLE);
	vecTables.push_back(gstrLOGIN);
	vecTables.push_back(gstrQUEUE_EVENT);
	vecTables.push_back(gstrQUEUE_EVENT_CODE);
	vecTables.push_back(gstrMACHINE);
	vecTables.push_back(gstrFAM_USER);
	vecTables.push_back(gstrFAM_FILE_ACTION_COMMENT);
	vecTables.push_back(gstrFAM_SKIPPED_FILE);
	vecTables.push_back(gstrFAM_FILE_TAG);
	vecTables.push_back(gstrFAM_TAG);
	vecTables.push_back(gstrACTIVE_FAM);
	vecTables.push_back(gstrLOCKED_FILE);
	vecTables.push_back(gstrUSER_CREATED_COUNTER);
	vecTables.push_back(gstrFPS_FILE);
	vecTables.push_back(gstrFAM_SESSION);
	vecTables.push_back(gstrINPUT_EVENT);
	vecTables.push_back(gstrFILE_ACTION_STATUS);
	vecTables.push_back(gstrSOURCE_DOC_CHANGE_HISTORY);
	vecTables.push_back(gstrDOC_TAG_HISTORY);
	vecTables.push_back(gstrDB_INFO_HISTORY);
	vecTables.push_back(gstrDB_FTP_ACCOUNT);
	vecTables.push_back(gstrDB_FTP_EVENT_HISTORY);
	vecTables.push_back(gstrDB_QUEUED_ACTION_STATUS_CHANGE);
	vecTables.push_back(gstrDB_FIELD_SEARCH);
	vecTables.push_back(gstrDB_FILE_HANDLER);
	vecTables.push_back(gstrDB_FEATURE);
	vecTables.push_back(gstrWORK_ITEM);
	vecTables.push_back(gstrWORK_ITEM_GROUP);
	vecTables.push_back(gstrMETADATA_FIELD);
	vecTables.push_back(gstrFILE_METADATA_FIELD_VALUE);
	vecTables.push_back(gstrTASK_CLASS);
	vecTables.push_back(gstrFILE_TASK_SESSION);
	vecTables.push_back(gstrFILE_TASK_SESSION_CACHE);
	vecTables.push_back(gstrSECURE_COUNTER);
	vecTables.push_back(gstrSECURE_COUNTER_VALUE_CHANGE);
	vecTables.push_back(gstrPAGINATION);
	vecTables.push_back(gstrWORKFLOW_TYPE);
	vecTables.push_back(gstrWORKFLOW);
	vecTables.push_back(gstrWORKFLOW_FILE);
	vecTables.push_back(gstrWORKFLOWCHANGE);
	vecTables.push_back(gstrWORKFLOWCHANGE_FILE);
	vecTables.push_back(gstrMLMODEL);
	vecTables.push_back(gstrMLDATA);
	vecTables.push_back(gstrWEB_APP_CONFIG);
	vecTables.push_back(gstrDATABASE_SERVICE);
	vecTables.push_back(gstrREPORTING_VERIFICATION_RATES);
	vecTables.push_back(gstrDASHBOARD);
	vecTables.push_back(gstrREPORTING_DATABASE_MIGRATION_WIZARD);
	vecTables.push_back(gstrROLE);
	vecTables.push_back(gstrSECURITY_GROUP);
	vecTables.push_back(gstrLOGIN_GROUP_MEMBERSHIP);
	vecTables.push_back(gstrSECURITY_GROUP_ACTION);
	vecTables.push_back(gstrSECURITY_GROUP_DASHBOARD);
	vecTables.push_back(gstrSECURITY_GROUP_REPORT);
	vecTables.push_back(gstrSECURITY_GROUP_WORKFLOW);
	vecTables.push_back(gstrSECURITY_GROUP_Role);
	vecTables.push_back(gstrEMAIL_SOURCE);
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::isExtractTable(const string& strTable)
{
	// Get the list of tables
	vector<string> vecTables;
	getExpectedTables(vecTables);

	return vectorContainsElement(vecTables, strTable);
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::dropAllFKConstraints()
{
	// Get the tables for removing the foreign key constraints
	vector<string> vecTables;
	getExpectedTables(vecTables);

	// Drop the constraints
	auto role = getAppRoleConnection();
	dropFKContraintsOnTables(role->ADOConnection(), vecTables);
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getMachineID(_ConnectionPtr ipConnection)
{
	// if the m_lMachineID == 0, get the id from the database
	if (m_lMachineID == 0)
	{
		CSingleLock lg(&m_criticalSection, TRUE);
		m_lMachineID = getKeyID(ipConnection, gstrMACHINE, "MachineName", m_strMachineName);
	}
	return m_lMachineID;
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getFAMUserID(_ConnectionPtr ipConnection)
{
	// if the m_lFAMUserID == 0, get the id from the database
	if (m_lFAMUserID == 0)
	{
		CSingleLock lg(&m_criticalSection, TRUE);
		m_lFAMUserID = getKeyID(ipConnection, gstrFAM_USER, "UserName", m_strFAMUserName);
	}
	return m_lFAMUserID;
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::addOrUpdateFAMUser(_ConnectionPtr ipConnection)
{
	try
	{
		string strUserName = getCurrentUserName();
		string strFullUserName = getFullUserName();
		long lFAMUserID = getKeyID(ipConnection, "FAMUser", "UserName", strUserName);

		string strQuery =
			"UPDATE FAMUser SET FullUserName = @FullUserName "
			" WHERE ID = @FAMUserID "
			"		AND(FullUserName IS NULL OR FullUserName <> @FullUserName)";
		executeCmd(buildCmd(ipConnection, strQuery,
			{
				{"@FullUserName", strFullUserName.c_str()},
				{"@FAMUserID", lFAMUserID}
			}));

		return lFAMUserID;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45997");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::loadDBInfoSettings(_ConnectionPtr ipConnection/* = __nullptr*/)
{
	try
	{
		if (m_ipDBInfoSettings != __nullptr && m_iDBSchemaVersion != 0)
		{
			return;
		}

		if (ipConnection == __nullptr || ipConnection->State == adStateClosed)
		{
			ipConnection = getDBConnectionRegardlessOfRole();
		}

		if (ipConnection->State == adStateClosed)
		{
			throw UCLIDException("ELI51736", "Connection is Closed!");
		}

		// Initialize settings to default values
		m_ipDBInfoSettings = __nullptr;
		m_iDBSchemaVersion = 0;
		m_iCommandTimeout = glDEFAULT_COMMAND_TIMEOUT;
		m_bUpdateQueueEventTable = true;
		m_bUpdateFASTTable = true;
		m_iNumberOfRetries = m_bNumberOfRetriesOverridden ? m_iNumberOfRetries : giDEFAULT_RETRY_COUNT;
		m_dRetryTimeout = m_bRetryTimeoutOverridden ? m_dRetryTimeout : gdDEFAULT_RETRY_TIMEOUT;
		m_dGetFilesToProcessTransactionTimeout = gdMINIMUM_TRANSACTION_TIMEOUT;
		m_dVerificationSessionTimeout = 0;
		m_bAllowRestartableProcessing = false;

		// Only load the settings if the table exists
		if (doesTableExist(ipConnection, "DBInfo"))
		{
			// Create a pointer to a recordset
			_RecordsetPtr ipDBInfoSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI31897", ipDBInfoSet != __nullptr);

			// Open the record set using the Setting Query		
			ipDBInfoSet->Open(gstrDBINFO_GET_SETTINGS_QUERY.c_str(),
				_variant_t((IDispatch*)ipConnection, true), adOpenForwardOnly,
				adLockReadOnly, adCmdText);

			IStrToStrMapPtr ipDBInfoSettings(CLSID_StrToStrMap);
			ASSERT_RESOURCE_ALLOCATION("ELI31896", ipDBInfoSettings != __nullptr);

			ipDBInfoSettings->CaseSensitive = VARIANT_FALSE;


			while (ipDBInfoSet->adoEOF == VARIANT_FALSE)
			{
				FieldsPtr ipFields = ipDBInfoSet->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI31898", ipFields != __nullptr);

				string strKey = getStringField(ipFields, "Name");
				string strValue = getStringField(ipFields, "Value");
				ipDBInfoSettings->Set(strKey.c_str(), strValue.c_str());

				if (strKey == gstrFAMDB_SCHEMA_VERSION)
				{
					m_iDBSchemaVersion = asLong(strValue);
				}
				else if (strKey == "FPMDBSchemaVersion")
				{
					// This is for an even older schema version
					m_iDBSchemaVersion = asLong(strValue);
				}
				else if (strKey == gstrCOMMAND_TIMEOUT)
				{
					m_iCommandTimeout = asLong(strValue);
				}
				else if (strKey == gstrUPDATE_QUEUE_EVENT_TABLE)
				{
					m_bUpdateQueueEventTable = strValue == "1";
				}
				else if (strKey == gstrUPDATE_FAST_TABLE)
				{
					m_bUpdateFASTTable = strValue == "1";
				}
				else if (strKey == gstrNUMBER_CONNECTION_RETRIES)
				{
					if (!m_bNumberOfRetriesOverridden)
					{
						// Get the Connection retry count
						m_iNumberOfRetries = asLong(strValue);
					}
				}
				else if (strKey == gstrCONNECTION_RETRY_TIMEOUT)
				{
					if (!m_bRetryTimeoutOverridden)
					{
						// Get the connection retry timeout
						m_dRetryTimeout = asDouble(strValue);
					}
				}
				else if (strKey == gstrAUTO_DELETE_FILE_ACTION_COMMENT)
				{
					m_bAutoDeleteFileActionComment = strValue == "1";
				}
				else if (strKey == gstrAUTO_REVERT_TIME_OUT_IN_MINUTES)
				{
					m_nAutoRevertTimeOutInMinutes = asLong(strValue);

					// [LegacyRCAndUtils:6172]
					// Don't enforce gnMINIMUM_AUTO_REVERT_TIME_OUT_IN_MINUTES in debug mode; having a low value is useful in development.
#ifndef _DEBUG
					// if less that a minimum value this should be reset to the minimum value
					if (m_nAutoRevertTimeOutInMinutes < gnMINIMUM_AUTO_REVERT_TIME_OUT_IN_MINUTES)
					{
						try
						{
							string strNewValue = asString(gnMINIMUM_AUTO_REVERT_TIME_OUT_IN_MINUTES);
							// Log application trace exception 
							UCLIDException ue("ELI29826", "Application trace: AutoRevertTimeOutInMinutes changed to " +
								strNewValue + " minutes.");
							ue.addDebugInfo("Old value", m_nAutoRevertTimeOutInMinutes);
							ue.addDebugInfo("New value", gnMINIMUM_AUTO_REVERT_TIME_OUT_IN_MINUTES);
							ue.log();

							// Change the setting in the DBInfo table
							executeCmd(buildCmd(ipConnection,
								"UPDATE DBInfo SET Value = @NewValue WHERE DBInfo.Name = @KeyName ",
								{
									{"@NewValue", strNewValue.c_str()},
									{"@KeyName", strKey.c_str()}
								}));
						}
						CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29832");

						m_nAutoRevertTimeOutInMinutes = gnMINIMUM_AUTO_REVERT_TIME_OUT_IN_MINUTES;
					}
#endif
				}
				else if (strKey == gstrAUTO_REVERT_NOTIFY_EMAIL_LIST)
				{
					m_strAutoRevertNotifyEmailList = strValue;
				}
				else if (strKey == gstrACTION_STATISTICS_UPDATE_FREQ_IN_SECONDS)
				{
					m_nActionStatisticsUpdateFreqInSeconds = asLong(strValue);
				}
				else if (strKey == gstrGET_FILES_TO_PROCESS_TRANSACTION_TIMEOUT)
				{
					m_dGetFilesToProcessTransactionTimeout = asDouble(strValue);

					// Need to make sure the value is above the minimum
					if (m_dGetFilesToProcessTransactionTimeout < gdMINIMUM_TRANSACTION_TIMEOUT)
					{
						try
						{
							string strNewValue = asString(gdMINIMUM_TRANSACTION_TIMEOUT, 0);
							// Log application trace exception 
							UCLIDException ue("ELI31146", "Application trace: DBInfo setting changed.");
							ue.addDebugInfo("Setting", gstrGET_FILES_TO_PROCESS_TRANSACTION_TIMEOUT);
							ue.addDebugInfo("Old value", m_dGetFilesToProcessTransactionTimeout);
							ue.addDebugInfo("New value", gdMINIMUM_TRANSACTION_TIMEOUT);
							ue.log();
							
							// Change the setting in the DBInfo table
							executeCmd(buildCmd(ipConnection,
								"UPDATE DBInfo SET Value = @NewValue WHERE DBInfo.Name = @KeyName ",
								{
									{"@NewValue", strNewValue.c_str()},
									{"@KeyName", strKey.c_str()}
								}));
						}
						CATCH_AND_LOG_ALL_EXCEPTIONS("ELI31520");

						m_dGetFilesToProcessTransactionTimeout = gdMINIMUM_TRANSACTION_TIMEOUT;
					}
				}
				else if (strKey == gstrVERIFICATION_SESSION_TIMEOUT)
				{
					m_dVerificationSessionTimeout = asDouble(strValue);
				}
				else if (strKey == gstrSTORE_SOURCE_DOC_NAME_CHANGE_HISTORY)
				{
					m_bStoreSourceDocChangeHistory = strValue == "1";
				}
				else if (strKey == gstrALLOW_DYNAMIC_TAG_CREATION)
				{
					m_bAllowDynamicTagCreation = strValue == "1";
				}
				else if (strKey == gstrSTORE_DOC_TAG_HISTORY)
				{
					m_bStoreDocTagHistory = strValue == "1";
				}
				else if (strKey == gstrSTORE_FTP_EVENT_HISTORY)
				{
					m_bStoreFTPEventHistory = strValue == "1";
				}
				else if (strKey == gstrALLOW_RESTARTABLE_PROCESSING)
				{
					m_bAllowRestartableProcessing = strValue == "1";
				}
				else if (strKey == gstrDATABASEID)
				{
					m_strEncryptedDatabaseID = strValue;
					m_bDatabaseIDValuesValidated = false;
					m_bLoggedInvalidDatabaseID = false;
				}
				else if (strKey == gstrSTORE_DB_INFO_HISTORY)
				{
					m_bStoreDBInfoChangeHistory = strValue == "1";
				}
				else if (strKey == gstrENABLE_LOAD_BALANCING)
				{
					m_bLoadBalance = strValue == "1";
				}

				ipDBInfoSet->MoveNext();
			}

			m_ipDBInfoSettings = ipDBInfoSettings;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18146");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::checkFeatures(_ConnectionPtr ipConnection)
{
	try
	{
		// If the feature data has already been retrieved, no need to do it again.
		if (!m_bCheckedFeatures)
		{
			m_mapEnabledFeatures.clear();

			_RecordsetPtr ipResultSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI36078", ipResultSet != __nullptr);

			ipResultSet->Open(gstrGET_ENABLED_FEATURES_QUERY.c_str(),
				_variant_t((IDispatch *)ipConnection, true), adOpenStatic, adLockOptimistic, adCmdText);

			// Loop through each feature to collect the data for the feature.
			while (!asCppBool(ipResultSet->adoEOF))
			{
				FieldsPtr ipFields = ipResultSet->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI36079", ipFields != __nullptr);
			
				string strFeatureName = getStringField(ipFields, "FeatureName");			
				bool bAdminOnly = getBoolField(ipFields, "AdminOnly");

				m_mapEnabledFeatures[strFeatureName] = bAdminOnly;

				ipResultSet->MoveNext();
			}

			ipResultSet->Close();

			m_bCheckedFeatures = true;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI36080");
}
//--------------------------------------------------------------------------------------------------
HWND CFileProcessingDB::getAppMainWndHandle()
{
	// try to use this application's main window as the parent for the messagebox below
	// get the main window
	CWnd *pWnd = AfxGetMainWnd();//pApp->GetMainWnd();
	if (pWnd)
	{
		return pWnd->m_hWnd;
	}
	return NULL;
}
//--------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CFileProcessingDB::getLicensedProductSpecificMgrs()
{
	// Create the category manager instance
	ICategoryManagerPtr ipCategoryMgr(CLSID_CategoryManager);
	ASSERT_RESOURCE_ALLOCATION("ELI18948", ipCategoryMgr != __nullptr);

	// Get map of licensed prog ids that belong to the product specific db managers category
	IStrToStrMapPtr ipProductSpecMgrProgIDs = 
		ipCategoryMgr->GetDescriptionToProgIDMap1(FP_FAM_PRODUCT_SPECIFIC_DB_MGRS.c_str());
	ASSERT_RESOURCE_ALLOCATION("ELI18947", ipProductSpecMgrProgIDs != __nullptr);

	// Create a vector to contain instances of DB managers to return
	IIUnknownVectorPtr ipProdSpecMgrs(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI18949", ipProdSpecMgrs != __nullptr);

	// Get the number of licensed product specific db managers
	long nSize = ipProductSpecMgrProgIDs->Size;
	for (long n = 0; n < nSize; n++)
	{
		// get the prog id
		CComBSTR bstrKey, bstrValue;
		ipProductSpecMgrProgIDs->GetKeyValue(n, &bstrKey, &bstrValue);
		
		// Create the object
		ICategorizedComponentPtr ipComponent(asString(bstrValue).c_str());
		if (ipComponent == __nullptr)
		{
			UCLIDException ue("ELI18950", "Unable to create Product Specific DB Manager!");
			ue.addDebugInfo("ObjectName", asString(bstrValue));
			throw ue;
		}
		
		// Put instance on the return vector
		ipProdSpecMgrs->PushBack(ipComponent);
	}

	// Return the vector of product specific managers
	return ipProdSpecMgrs;
}
//--------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CFileProcessingDB::removeProductSpecificDB(_ConnectionPtr ipConnection, bool bOnlyTables, bool bRetainUserTables)
{
	try
	{	
		// Get vector of all license product specific managers
		IIUnknownVectorPtr ipProdSpecMgrs = getLicensedProductSpecificMgrs();
		ASSERT_RESOURCE_ALLOCATION("ELI18951", ipProdSpecMgrs != __nullptr);

		// Loop through all of the objects and call the RemoveProductSpecificSchema 
		long nSize = ipProdSpecMgrs->Size();
		for (long n = 0; n < nSize; n++)
		{
			UCLID_FILEPROCESSINGLib::IProductSpecificDBMgrPtr ipMgr = ipProdSpecMgrs->At(n);
			ASSERT_RESOURCE_ALLOCATION("ELI18952", ipMgr != __nullptr);

			// Remove the schema for the product specific manager
			bool bSchemaRemoved = asCppBool(ipMgr->RemoveProductSpecificSchema(ipConnection, getThisAsCOMPtr(),
				asVariantBool(bOnlyTables), asVariantBool(bRetainUserTables)));

			// If the schema had not been present in the database, remove it from the return value
			// which represents the schemas that were removed.
			if (!bSchemaRemoved)
			{
				ipProdSpecMgrs->Remove(n);
				n--;
				nSize--;
			}
		}

		return ipProdSpecMgrs;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27610")
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addProductSpecificDB(_ConnectionPtr ipConnection,
										     IIUnknownVectorPtr ipProdSpecMgrs,
											 bool bOnlyTables, bool bAddUserTables)
{
	try
	{
		// Loop through all of the objects and call the AddProductSpecificSchema
		long nSize = ipProdSpecMgrs->Size();
		for (long n = 0; n < nSize; n++)
		{
			UCLID_FILEPROCESSINGLib::IProductSpecificDBMgrPtr ipMgr = ipProdSpecMgrs->At(n);
			ASSERT_RESOURCE_ALLOCATION("ELI19791", ipMgr != __nullptr);

			// Add the schema from the product specific db manager
			ipMgr->AddProductSpecificSchema(ipConnection, getThisAsCOMPtr(),
				asVariantBool(bOnlyTables), asVariantBool(bAddUserTables));
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27608")
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addProductSpecificDB80()
{
	try
	{
		// Get vector of all license product specific managers
		IIUnknownVectorPtr ipProdSpecMgrs = getLicensedProductSpecificMgrs();
		ASSERT_RESOURCE_ALLOCATION("ELI34241", ipProdSpecMgrs != __nullptr);

		// Loop through all of the objects and call the AddProductSpecificSchema
		long nSize = ipProdSpecMgrs->Size();
		for (long n = 0; n < nSize; n++)
		{
			UCLID_FILEPROCESSINGLib::IProductSpecificDBMgrPtr ipMgr = ipProdSpecMgrs->At(n);
			ASSERT_RESOURCE_ALLOCATION("ELI34242", ipMgr != __nullptr);

			// Add the schema from the product specific db manager
			ipMgr->AddProductSpecificSchema80(getThisAsCOMPtr());
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34243")
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::isConnectionAlive(_ConnectionPtr ipConnection)
{
	try
	{
		if (ipConnection != __nullptr)
		{
			getSQLServerDateTime(ipConnection);
			return true;
		}
	}
	catch(...){};
	
	return false;
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::reConnectDatabase(string ELICodeOfCaller)
{
	StopWatch sw;
	sw.start();
	bool bNoMoreRetries = false;
	DWORD dwThreadID = GetCurrentThreadId();
	do
	{
		try
		{
			try
			{
				// Reset all database connections since if one is in a bad state all will be.				
				resetDBConnection();

				// Exception logged to indicate the retry was successful.
				UCLIDException ueConnected("ELI23614",
					"Application trace: Connection retry successful.");
				ueConnected.addDebugInfo("CallerELICode", ELICodeOfCaller);
				ueConnected.log();

				return true;
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23503");
		}
		catch(UCLIDException ue)
		{
			// Check to see if the timeout has been reached.
			if (sw.getElapsedTime() > m_dRetryTimeout)
			{
				// Set the flag for no more retries to exit the loop.
				bNoMoreRetries = true;

				// Create exception to indicate retry timed out
				UCLIDException uex("ELI23612", "Database connection retry timed out!", ue);
				uex.addDebugInfo("ELICodeOfCaller", ELICodeOfCaller);

				// Log the caught exception.
				uex.log();
			}
			else
			{
				// Sleep to reduce the number of retries/second
				Sleep(100);
			}
		}
	}
	while (!bNoMoreRetries);

	return false;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::addSkipFileRecord(const _ConnectionPtr &ipConnection,
										  long nFileID, long nActionID)
{
	try
	{
		if (m_nFAMSessionID == 0)
		{
			throw UCLIDException("ELI38469", "Cannot skip a file outside of a FAM session.");
		}

		string strSkippedSQL = "SELECT * FROM SkippedFile WHERE FileID = "
			+ asString(nFileID) + " AND ActionID = " + asString(nActionID);

		_RecordsetPtr ipSkippedSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI26884", ipSkippedSet != __nullptr);

		ipSkippedSet->Open(strSkippedSQL.c_str(), _variant_t((IDispatch*)ipConnection, true),
			adOpenDynamic, adLockOptimistic, adCmdText);

		// Ensure no records returned
		if (ipSkippedSet->adoEOF == VARIANT_FALSE)
		{
			UCLIDException uex("ELI26806", "File has already been skipped for this action!");
			uex.addDebugInfo("Action ID", nActionID);
			uex.addDebugInfo("File ID", nFileID);
			throw uex;
		}
		else
		{
			string strUserName = (m_strFAMUserName.empty()) ? getCurrentUserName() : m_strFAMUserName;

			// Add a new row
			ipSkippedSet->AddNew();

			// Get the fields pointer
			FieldsPtr ipFields = ipSkippedSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI26807", ipFields != __nullptr);

			// Set the fields from the provided data
			setStringField(ipFields, "UserName", strUserName);
			setLongField(ipFields, "FileID", nFileID);
			setLongField(ipFields, "ActionID", nActionID);
			setLongField(ipFields, "FAMSessionID", m_nFAMSessionID);

			// Update the row
			ipSkippedSet->Update();
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26804");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::removeSkipFileRecord(const _ConnectionPtr &ipConnection,
											 long nFileID, long nActionID)
{
	try
	{
		string strSkippedSQL = "SELECT * FROM SkippedFile WHERE FileID = "
			+ asString(nFileID) + " AND ActionID = " + asString(nActionID);

		auto cmd = buildCmd(ipConnection, strSkippedSQL,
			{ {"@FileID", nFileID}, {"@ActionID", nActionID} });

		_RecordsetPtr ipSkippedSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI26885", ipSkippedSet != __nullptr);

		ipSkippedSet->Open((IDispatch*)cmd, vtMissing,
			adOpenDynamic, adLockOptimistic, adCmdText);

		// Only delete the record if it is found
		if (ipSkippedSet->BOF == VARIANT_FALSE)
		{
			// Delete the row
			ipSkippedSet->Delete(adAffectCurrent);
			ipSkippedSet->Update();
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26805");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::resetDBConnection(bool bCheckForUnaffiliatedFiles/* = false */)
{
	INIT_EXCEPTION_AND_TRACING("MLI03268");
	try
	{
		_lastCodePos = "10";

		CSingleLock lock(&m_criticalSection, TRUE);

		bool bDBSpecified = (!m_strDatabaseServer.empty() && !m_strDatabaseName.empty());

		// Close all the DB connections and clear the map [LRCAU# 5659]
		// The close is only temporary if the db has been specified.
		closeAllDBConnections(bDBSpecified);

		_lastCodePos = "40";

		// If there is a non empty server and database name get a connection and validate
		if (bDBSpecified)
		{
			// Reset the validation flags so all versions are checked
			m_bProductSpecificDBSchemasAreValid = false;
			m_bValidatingOrUpdatingSchema = false;

			m_roleUtility.UseApplicationRoles = (getDBSchemaVersion() >= 205)
				? m_regFPCfgMgr.getUseApplicationRoles()
				: false;

			// This will create a new connection for this thread and initialize the schema
			auto role = getAppRoleConnection();

			_lastCodePos = "50";

			// Validate the schema
			validateDBSchemaVersion(bCheckForUnaffiliatedFiles);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26869");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::closeAllDBConnections(bool bTemporaryClose)
{
	INIT_EXCEPTION_AND_TRACING("MLI03275");
	try
	{
		CSingleLock lock(&m_criticalSection, TRUE);
		
		_lastCodePos = "20";

		// Clear all of the connections in all of the threads
		m_mapThreadIDtoDBConnections.clear();
		_lastCodePos = "35";

		// If the close is not temporary, don't carry any credentials over to the next connection.
		if (!bTemporaryClose)
		{
			m_bLoggedInAsAdmin = false;
		}

		// Zero indicates the ID needs to be looked up next time the ID is requested.
		// -1 indicates there is no active workflow.
		m_nActiveWorkflowID = -1;
		m_mapWorkflowDefinitions.clear();

		// Reset the Current connection status to not connected
		m_strCurrentConnectionStatus = gstrNOT_CONNECTED;
		m_ipDBInfoSettings = __nullptr;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29885");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::clear(bool bLocked, bool bInitializing, bool retainUserValues)
{
	try
	{
		try
		{
			{
				// Direct authentication of user is needed to clear the DB (as opposed to using app roles)
				ValueRestorer<AppRole> applicationRoleRestorer(m_currentRole);
				m_currentRole = AppRole::kNoRole;

				// Get the connection pointer
				auto role = getAppRoleConnection();
				_ConnectionPtr ipConnection = role->ADOConnection();

				// If the ActiveFAM table does exist will need check for active processing
				// since part of checking will be to revert timed out FAMS need to lock the database
				// LegacyRCAndUtils #5940
				if (doesTableExist(ipConnection, gstrACTIVE_FAM))
				{
					if (!bLocked)
					{
						// Make sure processing is not active
						// This check needs to be done with the database locked since it will attempt to
						// revert timed out FAM's as part of the check for active processing
						// Lock the database for this instance
						LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
							gstrMAIN_DB_LOCK);

						assertProcessingNotActiveForAnyAction(true);
					}
					else
					{
						assertProcessingNotActiveForAnyAction(true);
					}
				}

				// Need to make sure the databaseID is loaded from the DBInfo table if it is there
				if (doesTableExist(ipConnection, gstrDB_INFO))
				{
					checkDatabaseIDValid(ipConnection, true, false);
				}

				CSingleLock lock(&m_criticalSection, TRUE);

				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				string strAdminPW;

				// Only get the admin password if we are not retaining user values and the
				// Login table already exists [LRCAU #5780]
				if (!retainUserValues && doesTableExist(ipConnection, "Login"))
				{
					// Need to store the admin login and add it back after re-adding the table
					getEncryptedPWFromDB(strAdminPW, true);
				}

				// First remove all Product Specific stuff
				IIUnknownVectorPtr ipProdSpecMgrs = __nullptr;
				if (!bInitializing)
				{
					ipProdSpecMgrs = removeProductSpecificDB(ipConnection, true, retainUserValues);
					ASSERT_RESOURCE_ALLOCATION("ELI38283", ipProdSpecMgrs != __nullptr);

					// Clear Status info from DatabaseService table - this needs to be done before tables are dropped
					executeCmdQuery(ipConnection, gstr_CLEAR_DATABASE_SERVICE_STATUS_FIELDS);

					dropTables(retainUserValues);
				}

				// Add the tables back
				addTables(!retainUserValues);

				// Setup the tables that require initial values
				initializeTableValues(!retainUserValues);
			
				// Add the admin user back with admin PW
				if (!strAdminPW.empty())
				{
					storeEncryptedPasswordAndUserName(gstrADMIN_USER, strAdminPW, false, false);
				}

				if (bInitializing)
				{
					executeCmdQuery(ipConnection, gstrPUBLIC_VIEW_DEFINITION_QUERY);

					// https://extract.atlassian.net/browse/ISSUE-12686
					// When creating a new database to ensure we are adding all schema currently
					// installed and licensed, do a check for any schema manager whose components are
					// not yet registered.
					checkForNewDBManagers();

					ipProdSpecMgrs = getLicensedProductSpecificMgrs();
					ASSERT_RESOURCE_ALLOCATION("ELI38284", ipProdSpecMgrs != __nullptr);
				}

				// Add the Product specific db 
				addProductSpecificDB(ipConnection, ipProdSpecMgrs, !bInitializing, !retainUserValues);
				tg.CommitTrans();

				if (bInitializing)
				{
					// Need to ensure all tables before adding roles, some of which have table-specific restrictions.
					// Get database ID instead of using checkDatabaseIDValid because we want to be able to get the
					// password even in the database ID is corrupted.
					long nDBHash = asLong(getDBInfoSetting(ipConnection, "DatabaseHash", false));
					CppSqlApplicationRole::CreateAllRoles(ipConnection, nDBHash);
				}

				// Shrink the database
				executeCmdQuery(ipConnection, gstrSHRINK_DATABASE);
			}

			// Reset the database connection
			resetDBConnection();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26870");
	}
	catch (UCLIDException &ue)
	{
		if (ue.getTopText().find("permission") != string::npos)
		{
			throw UCLIDException("ELI34204",
				"You do not appear to have sufficient permissions to clear the database.",
				ue);
		}
		
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::init80DB()
{
	try
	{
		try
		{
			// Get the connection pointer
			_ConnectionPtr ipConnection = getDBConnectionWithoutAppRole();

			CSingleLock lock(&m_criticalSection, TRUE);

			// Begin a transaction
			TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

			// Add the tables back
			addTables80();

			// Setup the tables that require initial values
			initializeTableValues80();

			tg.CommitTrans();

			// Add the Product specific db after the base tables have been committed
			addProductSpecificDB80();

			// Shrink the database
			auto role = getAppRoleConnection();
			executeCmdQuery(role->ADOConnection(), gstrSHRINK_DATABASE);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34237");
	}
	catch (UCLIDException &ue)
	{
		if (ue.getTopText().find("permission") != string::npos)
		{
			throw UCLIDException("ELI34238",
				"You do not appear to have sufficient permissions to clear the database.",
				ue);
		}
		
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
IStrToStrMapPtr CFileProcessingDB::getActions(_ConnectionPtr ipConnection,
										      const string& strWorkflow/* =""*/)
{
	try
	{
		// Create a pointer to a recordset
		_RecordsetPtr ipActionSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI13530", ipActionSet != __nullptr);

		string strQuery = "SELECT * FROM [Action] WHERE [WorkFlowID] IS NULL";
		if (!strWorkflow.empty())
		{
			long nWorkflowID = getWorkflowID(ipConnection, strWorkflow);
			strQuery = "SELECT * FROM [Action] WHERE [WorkflowID] = " + 
				asString(nWorkflowID);
		}

		// Open the Action table
		ipActionSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic,
			adLockReadOnly, adCmdText);

		// Create StrToStrMap to return the list of actions
		IStrToStrMapPtr ipActions(CLSID_StrToStrMap);
		ASSERT_RESOURCE_ALLOCATION("ELI29687", ipActions != __nullptr);

		ipActions->CaseSensitive = VARIANT_FALSE;

		// Step through all records
		while (ipActionSet->adoEOF == VARIANT_FALSE)
		{
			// Get the fields from the action set
			FieldsPtr ipFields = ipActionSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI26871", ipFields != __nullptr);

			// get the action name
			string strActionName = getStringField(ipFields, "ASCName");

			// get the action ID
			long lID = getLongField(ipFields, "ID");
			string strID = asString(lID);

			// Put the values in the StrToStrMap
			ipActions->Set(strActionName.c_str(), strID.c_str());

			// Move to the next record in the table
			ipActionSet->MoveNext();
		}

		return ipActions;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29686");
}
//-------------------------------------------------------------------------------------------------
long CFileProcessingDB::defineNewAction(_ConnectionPtr ipConnection, const string& strActionName)
{
	try
	{
		long nActionID = getActionIDNoThrow(ipConnection, strActionName, "");

		// Check to see if action exists
		if (nActionID > 0)
		{
			// Build error string (P13 #3931)
			CString zText;
			zText.Format("The action '%s' already exists, and therefore cannot be added again.", 
				strActionName.c_str());
			UCLIDException ue("ELI13946", LPCTSTR(zText));
			throw ue;
		}

		// Create a new action and return its ID
		return addAction(ipConnection, strActionName, "");
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29681");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::getFilesSkippedByUser(vector<long>& rvecSkippedFileIDs, long nActionID,
													  string strUserName,
													  const _ConnectionPtr& ipConnection)
{
	try
	{
		// Clear the vector
		rvecSkippedFileIDs.clear();

		string strSQL = "SELECT [FileID] FROM [SkippedFile] WHERE [ActionID] = @ActionID ";
		map<string, variant_t> mapParam;
		mapParam["@ActionID"] = nActionID;
		if (!strUserName.empty())
		{
			strSQL += " AND [UserName] = @UserName";
			mapParam["@UserName"] = strUserName.c_str();
		}
		auto cmd = buildCmd(ipConnection, strSQL, mapParam);

		// Make sure the DB Schema is the expected version
		validateDBSchemaVersion();

		// Recordset to contain the files to process
		_RecordsetPtr ipFileIDSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI26909", ipFileIDSet != __nullptr);

		// get the recordset with skipped file ID's
		ipFileIDSet->Open((IDispatch*) cmd, vtMissing,
			adOpenForwardOnly, adLockReadOnly, adCmdText);

		// Loop through the result set adding the file ID's to the vector
		while (ipFileIDSet->adoEOF == VARIANT_FALSE)
		{
			// Get the file ID and add it to the vector
			long nFileID = getLongField(ipFileIDSet->Fields, "FileID");
			rvecSkippedFileIDs.push_back(nFileID);

			// Move to the next record
			ipFileIDSet->MoveNext();
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26907");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::clearFileActionComment(const _ConnectionPtr& ipConnection, long nFileID,
											   long nActionID)
{
	try
	{
		map<string, variant_t> mapParams;
		
		// Query for deleting the comment
		string strCommentSQL = "DELETE FROM FileActionComment ";
		string strWhere = "";
		if (nFileID != -1)
		{
			mapParams["@FileID"] = nFileID;
			strWhere += "FileID = @FileID";
		}

		// If nActionID == -1 then delete all comments for the FileID
		if (nActionID != -1)
		{
			if (nFileID != -1)
			{
				strWhere += " AND ";
			}
			strWhere += "ActionID = @ActionID";
			mapParams["@ActionID"] = nActionID;
		}

		if (!strWhere.empty())
		{
			strCommentSQL += "WHERE " + strWhere;
		}

		// Perform deletion
		executeCmd(buildCmd(ipConnection, strCommentSQL, mapParams));
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27109");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::validateTagName(const string& strTagName)
{
	try
	{
		// Tag name is invalid if either:
		// 1. Empty
		// 2. Longer than 100 characters
		// 3. Does not match the TAG_REGULAR_EXPRESSION
		if (strTagName.empty() || strTagName.length() > 100
			|| getParser()->StringMatchesPattern(strTagName.c_str()) == VARIANT_FALSE)
		{
			UCLIDException ue("ELI27383", "Invalid tag name!");
			ue.addDebugInfo("Tag", strTagName);
			ue.addDebugInfo("Tag Length", strTagName.length());
			ue.addDebugInfo("Valid Tag Name", gstrTAG_REGULAR_EXPRESSION);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27384");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::validateFileID(const _ConnectionPtr& ipConnection, long nFileID)
{
	try
	{
		string strQuery = "SELECT [FileName] FROM [" + gstrFAM_FILE + "] WITH (NOLOCK) WHERE [ID] = @FileID";
		auto cmd = buildCmd(ipConnection, strQuery, { {"@FileID", nFileID} });

		_RecordsetPtr ipRecord(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI27385", ipRecord != __nullptr);

		ipRecord->Open((IDispatch*) cmd, vtMissing, adOpenStatic,
			adLockOptimistic, adCmdText);

		if (ipRecord->adoEOF == VARIANT_TRUE)
		{
			UCLIDException ue("ELI27386", "Invalid File ID: File ID does not exist in database!");
			ue.addDebugInfo("File ID", nFileID);
			ue.addDebugInfo("Database Name", m_strDatabaseName);
			ue.addDebugInfo("Database Server", m_strDatabaseServer);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27387");
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::getDBInfoSetting(const _ConnectionPtr& ipConnection,
	const string& strSettingName,
	bool bThrowIfMissing)
{
	try
	{
		loadDBInfoSettings(ipConnection);

		// When initializing a database, the DBInfo table may not yet exist. As long as caller is not
		// expecting an error to be thrown, return empty string m_ipDBInfoSettings wasn't created. 
		if (m_ipDBInfoSettings == __nullptr && !bThrowIfMissing)
		{
			// Treat a corrupted ID as "0" so it can still be converted to a long and used
			// to generate a DB role pw.
			if (strSettingName == "DatabaseHash")
			{
				return "0";
			}
			else
			{
				return "";
			}
		}

		ASSERT_RUNTIME_CONDITION("ELI51657", m_ipDBInfoSettings != __nullptr, "Unable to load DBInfo");

		if (strSettingName == "DatabaseHash")
		{
			string strEncryptedDatabaseID = asString(m_ipDBInfoSettings->GetValue(gstrDATABASEID.c_str()));
			try
			{
				try
				{
					if (strEncryptedDatabaseID.empty())
					{
						throw UCLIDException("ELI53106", "DatabaseID missing");
					}

					// Consider DatabaseHash to be missing if the database ID is corrupted so as not
					// to throw an exception that would prevent a login.
					DatabaseIDValues databaseIDValues(strEncryptedDatabaseID);

					return asString(databaseIDValues.m_nHashValue);
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI53031");
			}
			catch (UCLIDException& ue)
			{
				if (bThrowIfMissing)
				{
					throw ue;
				}
				else
				{
					// Treat a missing/invalid ID as "0" so it can still be converted to a long and used
					// to generate a DB role pw.
					return "0";
				}
			}
		}
		else if (strSettingName == "ExpectedSchemaVersion")
		{
			// Undocumented "setting" added to allow TestFAMDBSchemaUpdates to know the expected
			// version to which databases should be updated.
			return asString(ms_lFAMDBSchemaVersion);
		}
		else if (asCppBool(m_ipDBInfoSettings->Contains(strSettingName.c_str())))
		{
			// Return the setting value
			return asString(m_ipDBInfoSettings->GetValue(strSettingName.c_str()));
		}
		else if (bThrowIfMissing)
		{
			UCLIDException ue("ELI18940", "DBInfo setting does not exist!");
			ue.addDebugInfo("Setting", strSettingName);
			throw  ue;
		}
		else
		{
			return "";
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27388");
}
//--------------------------------------------------------------------------------------------------
long CFileProcessingDB::getTagID(const _ConnectionPtr &ipConnection, string &rstrTagName)
{
	try
	{
		return getKeyID(ipConnection, gstrFAM_TAG, "TagName", rstrTagName, false);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27389");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::revertLockedFilesToPreviousState(const _ConnectionPtr& ipConnection, 
														 long nActiveFAMID,
														 const string& strFASTComment, 
														 UCLIDException *pUE)
{
	try
	{
		// Setup Setting Query
		string strSQL = "SELECT [LockedFile].[FileID], [LockedFile].[ActionID], StatusBeforeLock, [ActionName] "
			" FROM LockedFile "
			" INNER JOIN [FileActionStatus] ON [LockedFile].[ActionID] = [FileActionStatus].[ActionID]"
			"	AND [LockedFile].[FileID] = [FileActionStatus].[FileID]"
			" WHERE [LockedFile].[ActiveFAMID] = @ActionFAMID "
			"	AND [FileActionStatus].[ActionStatus] = 'R'";
		auto cmd = buildCmd(ipConnection, strSQL, { {"@ActionFAMID", nActiveFAMID} });
		// Open a recordset that has the action names that need to have files reset
		_RecordsetPtr ipFileSet(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI27737", ipFileSet != __nullptr);

		ipFileSet->Open((IDispatch*)cmd, vtMissing,
			adOpenForwardOnly, adLockReadOnly, adCmdText);

		// Map to track the number of files for each action that are being reset
		map<string, map<string, int>> map_StatusCounts;
		map_StatusCounts.clear();

		if (ipFileSet->adoEOF == VARIANT_FALSE)
		{
			TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

			// Step through all of the file records in the LockedFile table for the dead ActiveFAMID
			while (ipFileSet->adoEOF == VARIANT_FALSE)
			{
				FieldsPtr ipFields = ipFileSet->Fields;

				// Get the action name and previous status
				string strActionName = getStringField(ipFields, "ActionName");
				string strRevertToStatus = getStringField(ipFields, "StatusBeforeLock");

				// Add to the count
				map_StatusCounts[strActionName][strRevertToStatus] =
					map_StatusCounts[strActionName][strRevertToStatus] + 1;

				// Pass bAllowQueuedStatusOverride so that any queued changes for files that were
				// processing when the FAM crashed are applied now.
				setFileActionState(ipConnection, getLongField(ipFields, "FileID"),
					strActionName, -1, strRevertToStatus,
					"", false, true, -1, getLongField(ipFields, "ActionID"), false, strFASTComment, true);

				ipFileSet->MoveNext();
			}
		}

		// Delete the record from the ActiveFAM table
		// By virtue of an FK with cascading deletes, this ensures files associated with the session
		// can't be left behind in LockedFile or FileTaskSessionCache.
		string strQuery = "DELETE FROM [ActiveFAM] WHERE [ID] = @ActiveFAMID";
		executeCmd(buildCmd(ipConnection, strQuery, { {"@ActiveFAMID", nActiveFAMID} }));

		// Set up the logged exception if it is not null
		if (pUE != __nullptr)
		{
			bool bAtLeastOneReset = false;
			string strEmailMessage = "";

			map<string, map<string, int>>::iterator itMap = map_StatusCounts.begin();
			for (; itMap != map_StatusCounts.end(); itMap++)
			{
				map<string, int>::iterator itCounts = itMap->second.begin();
				for (; itCounts != itMap->second.end(); itCounts++)
				{
					string strAction = itMap->first;
					string strDebugInfo = "CountOf_" + strAction + "_RevertedTo_" + itCounts->first;
					if (!bAtLeastOneReset)
					{
						// If this is the first item added to the message, add the FAST comment
						strEmailMessage = strFASTComment;
					}
					pUE->addDebugInfo(strDebugInfo, itCounts->second);
					strEmailMessage += "\r\n    " + strDebugInfo + ": " + asString(itCounts->second);
					bAtLeastOneReset = true;
				}
			}

			// Only log the reset exception if one or more files were reset
			if (bAtLeastOneReset)
			{
				pUE->log();

				// Send the email message if list is setup and message to send
				if (!m_strAutoRevertNotifyEmailList.empty() && !strEmailMessage.empty())
				{
					emailMessage(strEmailMessage);
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27738");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::pingDB()
{
	// if m_bFAMRegistered is false there is nothing to do
	if ( !m_bFAMRegistered )
	{
		return;
	}

	// Use ms_mutexPingDBLock to ensure no other thread from this process is also trying to add or
	// update the same entry in ActiveFAM (any thread from this process would be using the same
	// ActiveFAMID). If another thread has the lock, there is no need to block; we can assume the other
	// thread will update the ActiveFAM table appropriately.
	CSingleLock lock(&ms_mutexPingDBLock);
	if (!asCppBool(lock.Lock(0)))
	{
		return;
	}

	// Always call the getKeyID to make sure the record wasn't removed by another instance because
	// this instance lost the DB for a while.
	auto role = getAppRoleConnection();
	try
	{
		try
		{
			// Will throw an exception if m_nActiveFAMID does not exist in the ActiveFAM table.
			long nFAMSessionID = 0;
			// Return FAMSessionID as ID so it will populate nFAMSessionID
			getCmdId(buildCmd(role->ADOConnection(),
				"SELECT [FAMSessionID] AS [ID] FROM [ActiveFAM] WITH (NOLOCK) WHERE [ID] = @ActiveFAMID ", { {"@ActiveFAMID", m_nActiveFAMID} }),
				&nFAMSessionID);
			if (nFAMSessionID != m_nFAMSessionID)
			{
				UCLIDException ue("ELI34118", "Unexpected FAMSessionID.");
				ue.addDebugInfo("Expected ID", m_nFAMSessionID);
				ue.addDebugInfo("New ID", nFAMSessionID);
				throw ue;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34114");
	}
	catch (UCLIDException &ue)
	{
		UCLIDException ueOuter("ELI34115", "ActiveFAM registration has been lost.", ue);
		ueOuter.addDebugInfo("ActiveFAM ID", m_nActiveFAMID);
		ueOuter.addDebugInfo("FAMSession ID", m_nFAMSessionID);
		throw ueOuter;
	}

	// Update the ping record. 
	executeCmd(buildCmd(role->ADOConnection(),
		"UPDATE [ActiveFAM] SET [LastPingTime]=GETUTCDATE() WHERE [ID] = @ActiveFAMID", { {"@ActiveFAMID", m_nActiveFAMID} }));

	m_dwLastPingTime = GetTickCount();
}
//--------------------------------------------------------------------------------------------------
UINT CFileProcessingDB::maintainLastPingTimeForRevert(void *pData)
{
	try
	{
		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		CFileProcessingDB *pDB = static_cast<CFileProcessingDB *>(pData);
		ASSERT_ARGUMENT("ELI27746", pDB != __nullptr);

		// Enclose so that the exited event can always be signaled
		try
		{
			try
			{
				while (pDB->m_eventStopMaintenanceThreads.wait(gnPING_TIMEOUT) == WAIT_TIMEOUT)
				{
					// Surround call to PingDB with code from the BEGIN_CONNECTION_RETRY macro to
					// ensure the ping thread has an opportunity to reconnect just as the processing
					// does. Unlike BEGIN_CONNECTION_RETRY, it will continue to try to re-establish
					// connection indefinitely. But if the PingDB call fails on a good connection
					// even after taking out a lock on the database, the active FAM's registration
					// will be invalidated preventing further processing.
					ADODB::_ConnectionPtr ipConnection = __nullptr;
					bool bSuccess = false;
					bool bLock = false;
					do
					{
						try
						{
							try
							{
								auto role = pDB->getAppRoleConnection();
								if (bLock)
								{
									// Lock the database if the last attempt with a valid connection
									// failed.
									LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(
										pDB, gstrMAIN_DB_LOCK);

									ipConnection = role->ADOConnection();

									pDB->pingDB();
								}
								else
								{
									ipConnection = role->ADOConnection();

									pDB->pingDB();
								}

								bSuccess = true;
							}
							CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34116")
						}
						catch (UCLIDException &ue)
						{
							if (pDB->m_eventStopMaintenanceThreads.wait(0) != WAIT_TIMEOUT)
							{
								break;
							}

							if (pDB->isConnectionAlive(ipConnection))
							{ 
								// If the update failed without a lock, try again with a lock.
								if (!bLock)
								{
									bLock = true;
									continue;
								}

								throw ue;
							}

							// If the connection is not valid, the next attempt with a valid
							// connection should be without a lock.
							bLock = false;

							try
							{
								// Unlike with END_CONNECTION_RETRY, failed reconnections here
								// shouldn't be fatal. Just keep trying until processing is
								// stopped or we finally get a connection.
								pDB->reConnectDatabase("ELI51745");
							}
							catch (...) {}
						}
					}
					while (!bSuccess);
				}
			}
			catch (...)
			{
				// If we cannot succeed in pinging the database, consider this instance to be
				// unregistered which will prevent any further processing.
				pDB->m_bFAMRegistered = false;
				pDB->m_nActiveActionID = -1;

				throw;
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27858");

		// Signal that the thread has exited
		pDB->m_eventPingThreadExited.signal();

		CoUninitialize();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27745");

	return 0;
}
//--------------------------------------------------------------------------------------------------
UINT CFileProcessingDB::maintainActionStatistics(void *pData)
{
	try
	{
		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		CFileProcessingDB *pDB = static_cast<CFileProcessingDB *>(pData);
		ASSERT_ARGUMENT("ELI35126", pDB != __nullptr);

		// Stagger the start of the maintenance threads so that, at least initially, the threads
		// aren't firing at the same time. Use a random time so that if a bunch of processes are
		// started simultaneously, they don't all hit the DB at the same time.
		unsigned int nTimeToSleep;
		rand_s(&nTimeToSleep);
		// Somewhere between 1/4 and 3/4 of gnSTATS_MAINT_TIMEOUT.
		nTimeToSleep = (nTimeToSleep % (gnSTATS_MAINT_TIMEOUT / 2)) + (gnSTATS_MAINT_TIMEOUT / 4);
		pDB->m_eventStopMaintenanceThreads.wait(nTimeToSleep);

		// Enclose so that the exited event can always be signaled if it can be.
		try
		{
			while (pDB->m_eventStopMaintenanceThreads.wait(gnSTATS_MAINT_TIMEOUT) == WAIT_TIMEOUT)
			{
				// Surround call to update stats with code from the BEGIN_CONNECTION_RETRY macro to
				// ensure this thread has an opportunity to reconnect just as the processing does.
				// Unlike BEGIN_CONNECTION_RETRY, it will continue to try to re-establish connection
				// indefinitely.
				ADODB::_ConnectionPtr ipConnection = __nullptr;
				bool bSuccess = false;
				bool bLocked = false;
				int nActionID = -1;

				do
				{
					try
					{
						try
						{
							nActionID = pDB->m_nActiveActionID;

							// If the action ID isn't valid there is nothing to do
							if (nActionID <= 0)
							{
								bSuccess = true;
								continue;
							}

							// If GetStats has been called recently by pDB (via the FAM, etc),
							// there is no need to update ActionStatistics on this background
							// thread.
							CTimeSpan timeSinceLastStatsCheck =
								CTime::GetCurrentTime() - pDB->m_timeLastStatsCheck;
							if (timeSinceLastStatsCheck.GetTotalSeconds() <
								pDB->m_nActionStatisticsUpdateFreqInSeconds)
							{
								bSuccess = true;
								continue;
							}

							auto role = pDB->getAppRoleConnection();
							ipConnection = role->ADOConnection();
							
							string strActionName = pDB->getActionName(ipConnection, nActionID);
							string strActionIDs = pDB->getActionIDsForActiveWorkflow(ipConnection, strActionName);

							// Tokenize by either comma, semicolon, or pipe
							vector<string> vecTokens;
							vector<int> vecActionIds;
							
							StringTokenizer::sGetTokens(strActionIDs, ",;|", vecTokens, true);
							for each (auto action in vecTokens)
								vecActionIds.push_back(asLong(action));

							for each (nActionID in vecActionIds)
							{
								if (pDB->isStatisticsUpdateFromDeltaNeeded(ipConnection, nActionID))
								{
									// A Lock is needed to update the ActionStatistics table.
									LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(
										pDB, gstrMAIN_DB_LOCK);

									bLocked = true;

									// Begin a transaction
									TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

									pDB->updateActionStatisticsFromDelta(ipConnection, nActionID);

									tg.CommitTrans();
								}
							}
							
							bSuccess = true;
						}
						CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI35127")
					}
					catch (UCLIDException &ue)
					{
						ue.addDebugInfo("ActionID", nActionID);

						if (pDB->m_eventStopMaintenanceThreads.wait(0) != WAIT_TIMEOUT)
						{
							break;
						}

						if (pDB->isConnectionAlive(ipConnection))
						{ 
							// If the statistics update failed despite having a lock and a good
							// connection, we're in a bad state.
							if (bLocked)
							{
								throw ue;
							}

							continue;
						}

						try
						{
							// Unlike with END_CONNECTION_RETRY, failed reconnections here
							// shouldn't be fatal. Just keep trying until processing is
							// stopped or we finally get a connection.
							pDB->reConnectDatabase("ELI51746");
						}
						catch (...) {}
					}
				}
				while (!bSuccess);
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI35128");

		pDB->m_eventStatsThreadExited.signal();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI35129");

	return 0;
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::revertTimedOutProcessingFAMs(bool bDBLocked, const _ConnectionPtr& ipConnection)
{
	// check to see if this already running in this process
	if (m_bRevertInProgress)
	{
		return;
	}

	if (ms_dwLastRevertTime > 0
		&& (GetTickCount() - ms_dwLastRevertTime) < gnPING_TIMEOUT)
	{ 
		return;
	}

	CSingleLock lock(&ms_mutexAutoRevertLock);
	if (!asCppBool(lock.Lock(0)))
	{
		return;
	}

	if (ms_dwLastRevertTime > 0
		&& (GetTickCount() - ms_dwLastRevertTime) < gnPING_TIMEOUT)
	{
		return;
	}

	try
	{
		// Set the revert in progress flag so only one thread executes this per process
		m_bRevertInProgress = true;

		TransactionGuard tgRevert(ipConnection, adXactRepeatableRead, &m_criticalSection);

		// Make sure the LastPingTime is up to date before reverting so that the
		// current session doesn't get auto reverted
		// pingDB can be expensive under heavy workloads as it is called by every get files to process
		// call and can cause SQL locks to build up on the ActiveFAM table. Call only the first time or
		// if the pingDB has not been called in longer than gnPING_TIMEOUT.
		if (m_dwLastPingTime == 0 ||
			(GetTickCount() - m_dwLastPingTime) > gnPING_TIMEOUT)
		{
			pingDB();
		}

		// Query to show the elapsed time since last ping for all ActiveFAM records
		string strElapsedSQL = "SELECT [ID], "
			"DATEDIFF(minute,[LastPingTime],GetUTCDate()) as Elapsed "
			"FROM [ActiveFAM] WITH (NOLOCK)";

		long nTimedOutActiveFAMID = 0;
		while (getCmdId(buildCmd(ipConnection,
			"SELECT [ID] FROM [ActiveFAM] "
			"WHERE DATEDIFF(minute,[LastPingTime],GetUTCDate()) > @AutoRevertTimeout",
			{ { "@AutoRevertTimeout", m_nAutoRevertTimeOutInMinutes} })
			, &nTimedOutActiveFAMID
			, false))
		{
			if (!bDBLocked)
			{
				UCLIDException ue("ELI31136", "Database must be locked to revert files.");
				throw  ue;
			}

			_variant_t elapsed;
			executeCmd(buildCmd(ipConnection,
				"SELECT DATEDIFF(minute,[LastPingTime],GetUTCDate()) as Elapsed "
				"FROM [ActiveFAM] "
				"WHERE [ID] = @ID",
				{ { "@ID", nTimedOutActiveFAMID } })
				, false
				, true
				, "Elapsed"
				, &elapsed);

			UCLIDException ue("ELI27814", "Application Trace: Files were reverted to original status.");
			ue.addDebugInfo("Minutes files locked", elapsed.lVal);

			// Build the comment for the FAST table
			string strRevertComment = "Auto reverted after " + asString(elapsed.lVal) + " minutes.";

			// Revert the files for this dead FAM to there previous status
			revertLockedFilesToPreviousState(ipConnection, nTimedOutActiveFAMID, strRevertComment, &ue);
		}

		tgRevert.CommitTrans();

		ms_dwLastRevertTime = GetTickCount();
		m_bRevertInProgress = false;
	}
	catch(...)
	{
		m_bRevertInProgress = false;
		throw;
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::ensureFAMRegistration()
{
	if (!m_bFAMRegistered)
	{
		// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
		ADODB::_ConnectionPtr ipConnection = __nullptr;

		BEGIN_CONNECTION_RETRY();

		auto role = getAppRoleConnection();
		ipConnection = role->ADOConnection();

		// Re-add a new ActiveFAM table entry. (The circumstances where this code will be used are
		// rare, and not worth finding a way to pass on whether queuing is active).
		getThisAsCOMPtr()->UnregisterActiveFAM();
		getThisAsCOMPtr()->RegisterActiveFAM();

		END_CONNECTION_RETRY(ipConnection, "ELI37456");

		UCLIDException ue("ELI37457",
			"Application trace: ActiveFAM registration has been restored.");
		ue.addDebugInfo("ActiveFAM ID", m_nActiveFAMID);
		ue.log();
	}
}
//--------------------------------------------------------------------------------------------------
bool CFileProcessingDB::isMachineInListOfMachinesToSkipUserAuthentication(
	const _ConnectionPtr& ipConnection)
{
	try
	{
		// Get the list of machine names
		string strMachines = getDBInfoSetting(ipConnection, gstrSKIP_AUTHENTICATION_ON_MACHINES, true);

		// Tokenize by either comma, semicolon, or pipe
		vector<string> vecTokens;
		StringTokenizer::sGetTokens(strMachines, ",;|", vecTokens, true);

		string strMachineName = m_strMachineName;
		makeLowerCase(strMachineName);

		// Look through the list of strings for the machine name
		for(vector<string>::iterator it = vecTokens.begin(); it != vecTokens.end(); it++)
		{
			// Trim whitespace and make the string lower case
			string strTemp = trim(*it, " \t", " \t");
			makeLowerCase(strTemp);

			// Check if it matches the machine name
			// or special, wildcard value, see https://extract.atlassian.net/browse/ISSUE-17137
			if (strTemp == strMachineName || strTemp == "*")
			{
				return true;
			}
		}

		// No match found, return false
		return false;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29188");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::validateNewActionName(const string& strActionName)
{
	// Check if the action name is valid
	if (strActionName.length() > 50 || !isValidIdentifier(strActionName))
	{
		// Throw an exception
		UCLIDException ue("ELI29706", "Specified action name is invalid.");
		ue.addDebugInfo("Action Name", strActionName);
		ue.addDebugInfo("Valid Pattern", "[_a-zA-Z][_a-zA-Z0-9]*" );
		ue.addDebugInfo("Action Name Length", strActionName.length());
		ue.addDebugInfo("Maximum Length", "50");
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::validateMetadataFieldName(const string& strMetadataFieldName)
{
	// Check if the metadata field name is valid
	if (strMetadataFieldName.length() > 50 || !isValidIdentifier(strMetadataFieldName))
	{
		// Throw an exception
		UCLIDException ue("ELI38261", "Specified metadata field name is invalid.");
		ue.addDebugInfo("Metadata Field Name", strMetadataFieldName);
		ue.addDebugInfo("Valid Pattern", "[_a-zA-Z][_a-zA-Z0-9]*" );
		ue.addDebugInfo("Metadata Field Name Length", strMetadataFieldName.length());
		ue.addDebugInfo("Maximum Length", "50");
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
IRegularExprParserPtr CFileProcessingDB::getParser()
{
	try
	{
		IRegularExprParserPtr ipParser = m_ipMiscUtils->GetNewRegExpParserInstance("");
		ASSERT_RESOURCE_ALLOCATION("ELI27382", ipParser != __nullptr);

		// Set the pattern
		ipParser->Pattern = gstrTAG_REGULAR_EXPRESSION.c_str();

		return ipParser;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29458");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::emailMessage(const string & strMessage)
{
	AfxBeginThread(emailMessageThread, 
		new EmailThreadData(m_strAutoRevertNotifyEmailList, strMessage)); 
}
//--------------------------------------------------------------------------------------------------
UINT CFileProcessingDB::emailMessageThread(void *pData)
{
	try
	{
		// Put the emailThreadData pointer passed in to an auto pointer.
		unique_ptr<EmailThreadData> apEmailThreadData(static_cast<EmailThreadData *>(pData));
		ASSERT_RESOURCE_ALLOCATION("ELI27999", apEmailThreadData.get() != __nullptr);

		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		try
		{
			try
			{
				// Email Settings
				ISmtpEmailSettingsPtr ipEmailSettings(CLSID_SmtpEmailSettings);
				ASSERT_RESOURCE_ALLOCATION("ELI27962", ipEmailSettings != __nullptr);
				ipEmailSettings->LoadSettings(VARIANT_FALSE);

				// If there is no SMTP server set log an exception and return
				string strServer = asString(ipEmailSettings->Server);
				if (strServer.empty())
				{
					UCLIDException ue("ELI27969", 
						"Email settings have not been specified. Unable to send auto-revert message.");
					ue.log();
					return 0;
				}

				// Email Message 
				IExtractEmailMessagePtr ipMessage(CLSID_ExtractEmailMessage);
				ASSERT_RESOURCE_ALLOCATION("ELI27964", ipMessage != __nullptr);

				ipMessage->EmailSettings = ipEmailSettings;

				vector<string> vecRecipients;
				StringTokenizer::sGetTokens(apEmailThreadData->m_strRecipients, ",;", vecRecipients, true);

				IVariantVectorPtr ipRecipients(CLSID_VariantVector);
				ASSERT_RESOURCE_ALLOCATION("ELI27966", ipRecipients != __nullptr);

				for (unsigned int i = 0; i < vecRecipients.size(); i++)
				{
					ipRecipients->PushBack(vecRecipients[i].c_str());
				}

				// Add Recipients list to email message
				ipMessage->Recipients = ipRecipients;

				ipMessage->Subject = "Files were reverted to previous status."; 

				ipMessage->Body = apEmailThreadData->m_strMessage.c_str();

				// Send  the message
				ipMessage->Send();
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27971")
		}
		catch(UCLIDException ue)
		{
			UCLIDException uex("ELI27970", "Unable to send email.", ue);
			uex.addDebugInfo("Recipients", apEmailThreadData->m_strRecipients);
			uex.addDebugInfo("Message", apEmailThreadData->m_strMessage);
			uex.log();
		}		

		CoUninitialize();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27998");

	return 0;
}
//--------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CFileProcessingDB::setFilesToProcessing(bool bDBLocked, const _ConnectionPtr& ipConnection,
	const FilesToProcessRequest& request)
{
	// Declare query string so that if there is an exception the query can be added to debug info
	string strQuery;
	try
	{
		try
		{
			// IUnknownVector to hold the FileRecords to return
			IIUnknownVectorPtr ipFiles(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI51462", ipFiles != __nullptr);

			// Revert files
			revertTimedOutProcessingFAMs(bDBLocked, ipConnection);

			bool bTransactionSuccessful = false;

			// Start the stopwatch to use to check for transaction timeout
			StopWatch swTransactionRetryTimeout;
			swTransactionRetryTimeout.start();
			long nTransactionRetryCount = 0;

			// Retry the transaction until successful
			while (!bTransactionSuccessful)
			{
				try
				{
					try
					{
						vector<UCLID_FILEPROCESSINGLib::IFileRecordPtr> tempFileVector;

						// Begin a transaction
						TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

						if (m_bRunningAllWorkflows && m_bLoadBalance)
						{
							// Make a copy of the request with maxFiles set to 1 for the loops
							FilesToProcessRequest singleFileRequest(request);
							singleFileRequest.maxFiles = 1;

							int current = m_nProcessStart;
							int filesObtainedPreviously;
							int currentNumberOfFilesObtained = 0;
							do
							{
								filesObtainedPreviously = currentNumberOfFilesObtained;
								do
								{
									int actionID = m_vecActionsProcessOrder[current];
									current = (current + 1) % m_vecActionsProcessOrder.size();

									_RecordsetPtr ipFileSet = spGetFilesToProcessForActionID(ipConnection, singleFileRequest, actionID);

									auto results = getFilesFromRecordset(ipFileSet);
									tempFileVector.insert(tempFileVector.end(), results.begin(), results.end());
									currentNumberOfFilesObtained = tempFileVector.size();
								} while (current != m_nProcessStart && currentNumberOfFilesObtained < request.maxFiles);
							} while (currentNumberOfFilesObtained < request.maxFiles
								&& currentNumberOfFilesObtained != filesObtainedPreviously);
							// Commit the transaction before transfering the data from the recordset
							tg.CommitTrans();
							m_nProcessStart = current;
						}
						else
						{
							_RecordsetPtr ipFileSet = spGetFilesToProcessForActionID(ipConnection, request, 0);

							tempFileVector = getFilesFromRecordset(ipFileSet);
							tg.CommitTrans();
						}
						// Now that the transaction has processed correctly, copy the file records
						// that have been set to processing over to ipFiles.
						for each (UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord in tempFileVector)
						{
							ipFiles->PushBack(ipFileRecord);
						}
						bTransactionSuccessful = true;
						return ipFiles;
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51464");
				}
				catch (UCLIDException& ue)
				{
					// Check if this is a bad connection
					if (!isConnectionAlive(ipConnection))
					{
						// if the connection is not alive just rethrow the exception 
						throw ue;
					}

					if (nTransactionRetryCount == 0)
					{
						UCLIDException uex("ELI51731", "Start retry for set files to processing transaction.", ue);
						uex.log();
					}
					// Check to see if the timeout value has been reached
					if (swTransactionRetryTimeout.getElapsedTime() > m_dGetFilesToProcessTransactionTimeout)
					{
						UCLIDException uex("ELI51465", "Application Trace: Transaction retry timed out.", ue);
						uex.addDebugInfo(gstrGET_FILES_TO_PROCESS_TRANSACTION_TIMEOUT,
							asString(m_dGetFilesToProcessTransactionTimeout));
						throw uex;
					}

					nTransactionRetryCount++;
					// In the case that the exception is because the database has gotten into an
					// inconsistent state (as with LegacyRCAndUtiles:6350), use a small sleep here to
					// prevent thousands (or millions) of successive failures which may bog down the
					// DB and burn through table IDs.
					Sleep(100);
				}
			} // end while (!bTransactionSuccessful)
			return ipFiles; // This is unreachable but the compiler can't figure that out
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51463");
	}
	catch (UCLIDException& ue)
	{
		ue.addDebugInfo("Record Query", strQuery, true);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
_RecordsetPtr CFileProcessingDB::spGetFilesToProcessForActionID(const _ConnectionPtr& ipConnection,
	const FilesToProcessRequest& request, const int actionID)
{
	_CommandPtr cmd;
	cmd.CreateInstance(__uuidof(Command));
	ASSERT_RESOURCE_ALLOCATION("ELI51466", cmd != __nullptr);

	cmd->ActiveConnection = ipConnection;
	cmd->CommandText = _bstr_t("dbo.GetFilesToProcessForAction");
	cmd->CommandType = adCmdStoredProc;
	cmd->Parameters->Refresh();
	cmd->Parameters->Item["@ActionID"]->Value = variant_t(actionID);
	int workflowID = getActiveWorkflowID(ipConnection);
	if (workflowID != -1)
		cmd->Parameters->Item["@WorkflowID"]->Value = variant_t(workflowID);
	cmd->Parameters->Item["@ActionName"]->Value = variant_t(request.actionName.c_str());
	cmd->Parameters->Item["@BatchSize"]->Value = variant_t(request.maxFiles);
	//TODO: make this so it can process Skipped for all or single user
	cmd->Parameters->Item["@StatusToQueue"]->Value = variant_t(request.statusToSelect().c_str());
	cmd->Parameters->Item["@MachineID"]->Value = variant_t(getMachineID(ipConnection));
	cmd->Parameters->Item["@UserID"]->Value = variant_t(getFAMUserID(ipConnection));
	cmd->Parameters->Item["@ActiveFAMID"]->Value = variant_t(m_nActiveFAMID);
	cmd->Parameters->Item["@FAMSessionID"]->Value = variant_t(m_nFAMSessionID);
	cmd->Parameters->Item["@RecordFASTEntry"]->Value = variant_t(m_bUpdateFASTTable);
	cmd->Parameters->Item["@SkippedForUser"]->Value = variant_t(request.skippedUser.c_str());
	cmd->Parameters->Item["@CheckDeleted"]->Value = variant_t(m_bCurrentSessionIsWebSession);
	cmd->Parameters->Item["@UseRandomIDForQueueOrder"]->Value = variant_t(request.useRandomIDForQueueOrder);
	cmd->Parameters->Item["@LimitToUserQueue"]->Value = variant_t(request.limitToUserQueue);
	variant_t vtEmpty;
	return cmd->Execute(&vtEmpty, &vtEmpty, adCmdStoredProc);
}
//--------------------------------------------------------------------------------------------------
vector<UCLID_FILEPROCESSINGLib::IFileRecordPtr> CFileProcessingDB::getFilesFromRecordset(_RecordsetPtr ipFileSet)
{
	// Vector to hold the files
	vector<UCLID_FILEPROCESSINGLib::IFileRecordPtr> tempFileVector;
	// Fill the ipFiles collection and update the stats
	while (ipFileSet->adoEOF == VARIANT_FALSE)
	{
		FieldsPtr ipFields = ipFileSet->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI30403", ipFields != __nullptr);

		// Get the file Record from the fields
		UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord =
			getFileRecordFromFields(ipFields);
		ASSERT_RESOURCE_ALLOCATION("ELI30404", ipFileRecord != __nullptr);

		// [LegacyRCAndUtils:6225]
		// Do not add the record to ipFiles until after we have successfully
		// committed the transaction. If another thread/process has tried to
		// grab the same file, an "Invalid File State Transition" exception
		// will be thrown and, therefore, this thread/process should not process
		// the file.
		tempFileVector.push_back(ipFileRecord);

		string strFileID = asString(ipFileRecord->FileID);

		// Get the previous state
		string strFileFromState = getStringField(ipFields, "ASC_From");

		ipFileRecord->FallbackStatus =
			(UCLID_FILEPROCESSINGLib::EActionStatus)asEActionStatus(strFileFromState);

		ipFileSet->MoveNext();
	}
	return tempFileVector;
}
//--------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CFileProcessingDB::setFilesToProcessing(bool bDBLocked, const _ConnectionPtr& ipConnection,
	const string& strSelectSQL,
	const string& strActionName,
	long nMaxFiles,
	const string& strAllowedCurrentStatus)
{
	// Declare query string so that if there is an exception the query can be added to debug info
	string strQuery;
	try
	{
		try
		{
			// IUnknownVector to hold the FileRecords to return
			IIUnknownVectorPtr ipFiles(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI30401", ipFiles != __nullptr);

			// Revert files
			revertTimedOutProcessingFAMs(bDBLocked, ipConnection);

			bool bTransactionSuccessful = false;

			// Start the stopwatch to use to check for transaction timeout
			StopWatch swTransactionRetryTimeout;
			swTransactionRetryTimeout.start();

			// Retry the transaction until successful
			while (!bTransactionSuccessful)
			{
				// Begin a transaction
				TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

				try
				{
					try
					{
						// Maintains a list of files getting set to processing until the transaction
						// has completed successfully.
						vector<UCLID_FILEPROCESSINGLib::IFileRecordPtr> tempFileVector;

						string strQuery = gstrGET_FILES_TO_PROCESS_QUERY;
						replaceVariable(strQuery, "<SelectFilesToProcessQuery>", strSelectSQL);

						// Setup query that will set the action status to processing and update the FAST and
						// LockedFile records
						_CommandPtr getFilesToProcessCmd = buildCmd(ipConnection, strQuery,
							{
								{ "@FAMSessionID", m_nFAMSessionID},
								{ "@ActiveFAMID", m_nActiveFAMID },
								{ "@ActionName", strActionName.c_str() },
								{ "@WorkflowID", getWorkflowID(ipConnection, getActiveWorkflow())},
								{ "@UserID", getFAMUserID(ipConnection) },
								{ "@MachineID", getMachineID(ipConnection) },
								{ "@RecordFastEntry", m_bUpdateFASTTable },
								{ "@MaxFiles", nMaxFiles },
								{ "@LoadBalance", m_bLoadBalance && m_bRunningAllWorkflows }
							});

						// Loop to retry getting files until there are either no records returned 
						// Get recordset of files to be set to processing.
						// NOTE: Using execute to return a recordset because opening the query in a recordset results
						// in odd behavior where 1 record would be returned but 6 records would be set to 
						// processing
						variant_t vtRecordsAffected = 0L;
						_RecordsetPtr ipFileSet = getFilesToProcessCmd->Execute(&vtRecordsAffected, nullptr, adOptionUnspecified);
						ASSERT_RESOURCE_ALLOCATION("ELI30402", ipFileSet != __nullptr);

						// Fill the ipFiles collection and update the stats
						while (ipFileSet->adoEOF == VARIANT_FALSE)
						{
							FieldsPtr ipFields = ipFileSet->Fields;
							ASSERT_RESOURCE_ALLOCATION("ELI30403", ipFields != __nullptr);

							// Get the file Record from the fields
							UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord =
								getFileRecordFromFields(ipFields);
							ASSERT_RESOURCE_ALLOCATION("ELI30404", ipFileRecord != __nullptr);

							// [LegacyRCAndUtils:6225]
							// Do not add the record to ipFiles until after we have successfully
							// committed the transaction. If another thread/process has tried to
							// grab the same file, an "Invalid File State Transition" exception
							// will be thrown and, therefore, this thread/process should not process
							// the file.
							tempFileVector.push_back(ipFileRecord);

							string strFileID = asString(ipFileRecord->FileID);

							// Get the previous state
							string strFileFromState = getStringField(ipFields, "ASC_From");

							ipFileRecord->FallbackStatus =
								(UCLID_FILEPROCESSINGLib::EActionStatus)asEActionStatus(strFileFromState);

							// Make sure the transition is valid
							// Setting to processing if already processing is invalid in all cases.
							if (strFileFromState == "R" ||
								(!strAllowedCurrentStatus.empty() &&
									strAllowedCurrentStatus.find(strFileFromState) == string::npos))
							{
								UCLIDException ue("ELI30405", "Invalid File State Transition!");
								ue.addDebugInfo("Old Status", asStatusName(strFileFromState));
								ue.addDebugInfo("New Status", "Processing");
								ue.addDebugInfo("Action Name", strActionName);
								ue.addDebugInfo("File ID", strFileID);
								throw ue;
							}

							// There are no cases where this method should not just ignore all
							// pending entries in [QueuedActionStatusChange] for the selected files.
							executeCmd(buildCmd(ipConnection,
								"UPDATE [QueuedActionStatusChange] SET [ChangeStatus] = @NewStatus "
								" WHERE [ChangeStatus] = @OldStatus AND [ActionID] = @ActionID "
								" AND [FileID] = @FileID",
								{
									{ "@NewStatus", "I" },
									{ "@OldStatus", "P" },
									{ "@ActionID", ipFileRecord->ActionID },
									{ "@FileID", ipFileRecord->FileID}
								}));

							bool bIsDeleted =
								ipFileRecord->WorkflowID <= 0
								? false
								: isFileInWorkflow(ipConnection, ipFileRecord->FileID, ipFileRecord->WorkflowID) == 0; // 0 = deleted

							// Update the Statistics
							updateStats(ipConnection, ipFileRecord->ActionID, asEActionStatus(strFileFromState),
								kActionProcessing, ipFileRecord, ipFileRecord, bIsDeleted);

							// move to the next record in the recordset
							ipFileSet->MoveNext();
						}

						// Commit the changes to the database
						tg.CommitTrans();

						// Now that the transaction has processed correctly, copy the file records
						// that have been set to processing over to ipFiles.
						for each (UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord in tempFileVector)
						{
							ipFiles->PushBack(ipFileRecord);
						}

						bTransactionSuccessful = true;
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31138");
				}
				catch (UCLIDException& ue)
				{
					// Check if this is a bad connection
					if (!isConnectionAlive(ipConnection))
					{
						// if the connection is not alive just rethrow the exception 
						throw ue;
					}

					// Check to see if the timeout value has been reached
					if (swTransactionRetryTimeout.getElapsedTime() > m_dGetFilesToProcessTransactionTimeout)
					{
						UCLIDException uex("ELI31587", "Application Trace: Transaction retry timed out.", ue);
						uex.addDebugInfo(gstrGET_FILES_TO_PROCESS_TRANSACTION_TIMEOUT,
							asString(m_dGetFilesToProcessTransactionTimeout));
						throw uex;
					}

					// In the case that the exception is because the database has gotten into an
					// inconsistent state (as with LegacyRCAndUtiles:6350), use a small sleep here to
					// prevent thousands (or millions) of successive failures which may bog down the
					// DB and burn through table IDs.
					Sleep(100);
				}
			}
			return ipFiles;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30407");
	}
	catch (UCLIDException& ue)
	{
		ue.addDebugInfo("Record Query", strQuery, true);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::doesLoginUserNameExist(const _ConnectionPtr& ipConnection, const string &strUserName)
{
	// Create a pointer to a recordset
	_RecordsetPtr ipLoginSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI29710", ipLoginSet != __nullptr);

	// Sql query that should either be empty if the passed in users is not in the table
	// or will return the record with the given username
	string strLoginSelect = "Select Username From Login Where UserName = @UserName";
	auto cmd = buildCmd(ipConnection, strLoginSelect, { {"@UserName", strUserName.c_str()} });

	// Open the sql query
	ipLoginSet->Open((IDispatch*) cmd, vtMissing, adOpenStatic,	adLockReadOnly, adCmdText);

	// if not at the end of file then there is a user by that name.
	if (!asCppBool(ipLoginSet->adoEOF))
	{
		return true;
	}

	// No user by that name was found
	return false;
}
//-------------------------------------------------------------------------------------------------
_RecordsetPtr CFileProcessingDB::getFileActionStatusSet(_ConnectionPtr& ipConnection, long nFileID, long nActionID)
{
	try
	{
		EActionStatus eCurrentStatus = kActionUnattempted;

		_CommandPtr cmdSelectActionStatus = buildCmd(ipConnection,
			"SELECT ActionStatus From FileActionStatus WITH (NOLOCK) WHERE ActionID = @ActionID AND FileID = @FileID",
			{
				{ "@ActionID", nActionID },
				{ "@FileID", nFileID }
			});

		_RecordsetPtr ipFileActionStatus(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI51610", ipFileActionStatus != __nullptr);

		ipFileActionStatus->Open((IDispatch *)cmdSelectActionStatus, vtMissing, adOpenStatic,
			adLockReadOnly, adCmdText);

		return ipFileActionStatus;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30536")
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::assertProcessingNotActiveForAction(bool bDBLocked, _ConnectionPtr ipConnection, 
	const string &strActionName)
{
	// If the ActiveFAM table does not exist nothing is processing so return
	if (!doesTableExist(ipConnection, gstrACTIVE_FAM))
	{
		return;
	}

	revertTimedOutProcessingFAMs(bDBLocked, ipConnection);

	// Check for active processing for the action
	_RecordsetPtr ipProcessingSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI31589", ipProcessingSet != __nullptr);

	// Open recordset with ActiveFAM records that show processing on the action
	string strSQL = "SELECT [UPI] FROM [FAMSession] WITH (NOLOCK) "
		"INNER JOIN [ActiveFAM] WITH (NOLOCK) ON [FAMSessionID] = [FAMSession].[ID] "
		"INNER JOIN [Action] ON [ActionID] = [Action].[ID] "
		"WHERE [ASCName] = @ActionName";
	auto cmd = buildCmd(ipConnection, strSQL, { {"@ActionName", strActionName.c_str()} });

	ipProcessingSet->Open((IDispatch*)cmd, vtMissing, adOpenStatic,
		adLockReadOnly, adCmdText);

	// if there are any records in ipProcessingSet there is active processing.
	if (!asCppBool(ipProcessingSet->adoEOF))
	{
		// Since processing is occurring need to throw an exception.
		UCLIDException ue("ELI30547", "Processing is active for this action.");
		ue.addDebugInfo("Action", strActionName);
		FieldsPtr ipFields = ipProcessingSet->Fields;
		if (ipFields != __nullptr)
		{
			string strUPI = getStringField(ipFields, "UPI");
			ue.addDebugInfo("First UPI", strUPI.c_str());
		}
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::isFAMActiveForAnyAction(bool bDBLocked)
{
	auto role = getAppRoleConnection();
	_ConnectionPtr ipConnection = role->ADOConnection();

	// If the ActiveFAM table does not exist nothing is processing so return
	if (!doesTableExist(ipConnection, gstrACTIVE_FAM))
	{
		return false;
	}

	revertTimedOutProcessingFAMs(bDBLocked, ipConnection);

	// Check for active processing 
	long nActiveFAMCount = 0;
	executeCmdQuery(ipConnection,
		"SELECT Count([ID]) AS [ID] FROM [ActiveFAM] WITH (NOLOCK)", false, &nActiveFAMCount);

	return (nActiveFAMCount > 0);
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::assertProcessingNotActiveForAnyAction(bool bDBLocked)
{
	auto role = getAppRoleConnection();
	_ConnectionPtr ipConnection = role->ADOConnection();

	// Check for active processing 
	if (isFAMActiveForAnyAction(bDBLocked))
	{
		// Since processing is occurring need to throw an exception.
		throw UCLIDException("ELI30608", "Database has active processing.");
	}
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::assertNotActiveBeforeSchemaUpdate()
{
	auto role = getAppRoleConnection();
	_ConnectionPtr ipConnection = role->ADOConnection();
	
	// In schema versions < 110, the ProcessingFAM table will contain the processing FAMs
	if (doesTableExist(ipConnection, gstrPROCESSING_FAM))
	{
		_RecordsetPtr ipProcessingFAMCount(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI33958", ipProcessingFAMCount != __nullptr);

		ipProcessingFAMCount->Open("SELECT COUNT(*) AS FAMCOUNT FROM [ProcessingFAM]",
			_variant_t((IDispatch *)ipConnection, true), adOpenDynamic, adLockOptimistic, adCmdText);

		ipProcessingFAMCount->MoveFirst();
		long nRowCount = getLongField(ipProcessingFAMCount->Fields, "FAMCOUNT");
		if (nRowCount > 0)
		{
			throw UCLIDException("ELI33959", "Unable to update database since at least one instance "
				"of File Action Manager is currently processing files in the database");
		}
	}
	// In schema versions >= 110, the ActiveFAM table will contain any active FAMs (processing,
	// queuing, or just displaying stats)
	else if (doesTableExist(ipConnection, gstrACTIVE_FAM))
	{
		_RecordsetPtr ipProcessingFAMCount(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI33960", ipProcessingFAMCount != __nullptr);

		ipProcessingFAMCount->Open("SELECT COUNT(*) AS FAMCOUNT FROM [dbo].[ActiveFAM] WITH (NOLOCK)",
			_variant_t((IDispatch *)ipConnection, true), adOpenDynamic, adLockOptimistic, adCmdText);

		ipProcessingFAMCount->MoveFirst();
		long nRowCount = getLongField(ipProcessingFAMCount->Fields, "FAMCOUNT");
		if (nRowCount > 0)
		{
			throw UCLIDException("ELI33961", "Unable to update database since at least one instance "
				"of File Action Manager is active in the database");
		}
	}

	// https://extract.atlassian.net/browse/ISSUE-13484
	// Do not allow schema updates if there is pending parallelized work items.
	if (doesTableExist(ipConnection, gstrWORK_ITEM))
	{
		_RecordsetPtr ipPendingWorkItems(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI39229", ipPendingWorkItems != __nullptr);

		ipPendingWorkItems->Open("SELECT * FROM [dbo].[WorkItem] WITH (NOLOCK) ",
			_variant_t((IDispatch *)ipConnection, true), adOpenDynamic, adLockOptimistic, adCmdText);

		if (!asCppBool(ipPendingWorkItems->adoEOF))
		{
			throw UCLIDException("ELI39230", "Unable to update database since at least there is "
				"pending work for a parallelized task that has not been completed.");
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::isStatisticsUpdateFromDeltaNeeded(const _ConnectionPtr& ipConnection, const long nActionID)
{
	// Create a pointer to a recordset
	_RecordsetPtr ipActionStatSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI35130", ipActionStatSet != __nullptr);

	// Select the existing Statistics record if it exists
	string strSelectStat = "SELECT * FROM ActionStatistics WHERE ActionID = @ActionID";
	auto cmd = buildCmd(ipConnection, strSelectStat, { {"@ActionID", nActionID} });

	// Open the recordset for the statistics with the record for ActionID if it exists
	ipActionStatSet->Open((IDispatch*)cmd, vtMissing, adOpenStatic,
		adLockOptimistic, adCmdText);

	if (!asCppBool(ipActionStatSet->adoEOF))
	{
		// Get the fields from the action stat set
		FieldsPtr ipFields = ipActionStatSet->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI35131", ipFields != __nullptr);

		// Check the last updated time stamp 
		CTime timeCurrent = getSQLServerDateTimeAsSystemTime(ipConnection);
		CTime timeLastUpdated = getTimeDateField(ipFields, "LastUpdateTimeStamp");
		CTimeSpan ts = timeCurrent - timeLastUpdated;
		if (ts.GetTotalSeconds() > m_nActionStatisticsUpdateFreqInSeconds)
		{
			return true;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::updateActionStatisticsFromDelta(const _ConnectionPtr& ipConnection, const long nActionID)
{
	// Get the current last record id from the ActionStatisticsDelta table for the ActionID
	string strActionStatisticsDeltaSQL =
		"SELECT COALESCE(MAX(ID),0) AS LastDeltaID FROM ActionStatisticsDelta where ActionID = @ActionID";
	auto cmd = buildCmd(ipConnection, strActionStatisticsDeltaSQL, { {"@ActionID", nActionID} });

	_RecordsetPtr ipActionStatisticsDeltaSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI30749", ipActionStatisticsDeltaSet != __nullptr);

	// Open the set that will give the last id in the delta table for the action
	ipActionStatisticsDeltaSet->Open((IDispatch*) cmd, vtMissing, adOpenStatic, adLockReadOnly, adCmdText);

	// Since the query for the set uses a Aggregate function (MAX) there should always
	// be at least one record if there is not a problem
	if (asCppBool(ipActionStatisticsDeltaSet->adoEOF))
	{
		UCLIDException ue("ELI30774", "No records found.");
		ue.addDebugInfo("ActionID", nActionID);
		throw ue;
	}

	// get the fields
	FieldsPtr ipFields = ipActionStatisticsDeltaSet->Fields;
	ASSERT_RESOURCE_ALLOCATION("ELI30750", ipFields != __nullptr);

	// Get the last delta id to update ( need this so we know what to remove for Delta table
	LONGLONG llLastDeltaID = getLongLongField(ipFields, "LastDeltaID");

	// IF the nLastDeltaID is 0 then there are not records in the delta table for the Action
	if (llLastDeltaID == 0)
	{
		// No Delta records so just update the time stamp
		string strUpdateTimeStamp = "UPDATE ActionStatistics SET [LastUpdateTimeStamp] = GetDate() "
			"WHERE ActionID = @ActionID";
		executeCmd(buildCmd(ipConnection, strUpdateTimeStamp, { {"@ActionID", nActionID} }));
		return;
	}


	// Build update query
	string strUpdateActionStatistics = gstrUPDATE_ACTION_STATISTICS_FOR_ACTION_FROM_DELTA;

	// Update the ActionStatistics table
	executeCmd(buildCmd(ipConnection, strUpdateActionStatistics, { {"@LastDeltaID", llLastDeltaID}, {"@ActionIDToUpdate", nActionID} }));
	
	// Delete the ActionStatisticsDelta records that have just been updated
	string strDeleteActionStatisticsDelta = "DELETE FROM ActionStatisticsDelta WHERE ActionID = @ActionID AND ID <= @LastDeltaID";
	executeCmd(buildCmd(ipConnection, strDeleteActionStatisticsDelta, { {"@ActionID", nActionID}, {"@LastDeltaID",llLastDeltaID} }));
}
//-------------------------------------------------------------------------------------------------
set<string> getDBTableNames(const _ConnectionPtr& ipConnection)
{
	set<string> setTableNames;

	// Retrieve the schema info for all tables in the database.
	_RecordsetPtr ipTables = ipConnection->OpenSchema(adSchemaTables);
	ASSERT_RESOURCE_ALLOCATION("ELI31391", ipTables != __nullptr);

	// Loop through all tables to compile a list of all table names (in uppercase)
	while (!asCppBool(ipTables->adoEOF))
	{
		string strType = getStringField(ipTables->Fields, "TABLE_TYPE");

		// Include only dbo tables in the list, not sys tables.
		// (Using a criteria on the OpenSchema call does not seem to work).
		if (_stricmp(strType.c_str(), "TABLE") == 0)
		{
			// Get the Name of the Foreign key table
			string strTableName = getStringField(ipTables->Fields, "TABLE_NAME");
			makeUpperCase(strTableName);

			setTableNames.insert(strTableName);
		}

		ipTables->MoveNext();
	}

	return setTableNames;
}
//-------------------------------------------------------------------------------------------------
set<string> getDBInfoRowNames(const _ConnectionPtr& ipConnection)
{
	set<string> setDBInfoRows;

	_RecordsetPtr ipResultSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI31392", ipResultSet != __nullptr);

	// Query for all rows in the DBInfo table
	string strDBInfoQuery = "SELECT [Name] FROM DBInfo";
	ipResultSet->Open(strDBInfoQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
		adOpenStatic, adLockReadOnly, adCmdText);

	// Loop through all DBInfo rows to compile a list of the names (in uppercase).
	while (!asCppBool(ipResultSet->adoEOF))
	{
		string strRowName = getStringField(ipResultSet->Fields, "Name");
		makeUpperCase(strRowName);
		setDBInfoRows.insert(strRowName);

		ipResultSet->MoveNext();
	}

	return setDBInfoRows;
}
//-------------------------------------------------------------------------------------------------
set<string> getDBFeatureNames(const _ConnectionPtr& ipConnection)
{
	set<string> setDBInfoRows;

	_RecordsetPtr ipResultSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI38262", ipResultSet != __nullptr);

	// Query for all rows in the Feature table
	string strFeaturesQuery = "SELECT [FeatureName] FROM [" + gstrDB_FEATURE + "]";
	ipResultSet->Open(strFeaturesQuery.c_str(), _variant_t((IDispatch *)ipConnection, true),
		adOpenStatic, adLockReadOnly, adCmdText);

	// Loop through all Feature table rows to compile a list of the names (in uppercase).
	while (!asCppBool(ipResultSet->adoEOF))
	{
		string strRowName = getStringField(ipResultSet->Fields, "FeatureName");
		makeUpperCase(strRowName);
		setDBInfoRows.insert(strRowName);

		ipResultSet->MoveNext();
	}

	return setDBInfoRows;
}
//-------------------------------------------------------------------------------------------------
// WARNING: If any DBInfo row or table is removed, this code needs to be modified so that it does
// not treat the removed element(s) on old schema versions as unrecognized.
vector<string> CFileProcessingDB::findUnrecognizedSchemaElements(const _ConnectionPtr& ipConnection)
{
	vector<string> vecUnrecognizedElements;

	// Get an uppercase list of the names of all tables currently in the database.
	set<string> setTableNames = getDBTableNames(ipConnection);

	// Get an uppercase list of the names of all DBInfo rows currently in the database.
	set<string> setDBInfoRows;
	if (setTableNames.find("DBINFO") != setTableNames.end())
	{
		setDBInfoRows = getDBInfoRowNames(ipConnection);
	}

	// Get an uppercase list of the names of all features currently in the database.
	set<string> setFeatureNames;
	if (setTableNames.find("FEATURE") != setTableNames.end())
	{
		setFeatureNames = getDBFeatureNames(ipConnection);
	}

	// Retrieve a list of all tables the FAM DB has managed since version 23
	vector<string> vecTableCreationQueries = getTableCreationQueries(true);
	vector<string> vecFAMDBTableNames = getTableNamesFromCreationQueries(vecTableCreationQueries);
	addOldTables(vecFAMDBTableNames);

	// Remove all tables known to the FAM DB from the names of tables found in the DB to leave a
	// list of tables unknown to the FAM DB.
	long nFAMTableCount = vecFAMDBTableNames.size();
	for (long i = 0; i < nFAMTableCount; i++)
	{
		string strTableName = vecFAMDBTableNames[i];
		makeUpperCase(strTableName); 
		setTableNames.erase(strTableName);
	}

	// Retrieve a list of all DBInfo rows the FAM DB has managed since version 23
	map<string, string> mapDBInfoValues = getDBInfoDefaultValues();
	addOldDBInfoValues(mapDBInfoValues);

	long nDBInfoValueCount = mapDBInfoValues.size();

	// Remove all rows known to the FAM DB from the names of DBINfo rows found in the DB to leave a
	// list of DBInfo rows unknown to the FAM DB.
	for (map<string, string>::iterator iterDBInfoValues = mapDBInfoValues.begin();
		 iterDBInfoValues != mapDBInfoValues.end();
		 iterDBInfoValues++)
	{
		string strDBInfoValueName = iterDBInfoValues->first;
		makeUpperCase(strDBInfoValueName);
		setDBInfoRows.erase(strDBInfoValueName);
	}

	// Retrieve a list of all features in the Feature table.
	vector<string> vecFeatureNames = getFeatureNames();
	size_t nFeatureCount = vecFeatureNames.size();

	// Remove all features known to the FAM DB from the names features found in the DB to leave a
	// list of features in the DB unknown to the FAM DB.
	for (size_t i = 0; i < nFeatureCount; i++)
	{
		string strDBFeatureName = vecFeatureNames[i];
		makeUpperCase(strDBFeatureName);
		setFeatureNames.erase(strDBFeatureName);
	}

	// If all lists are now empty, there is no need to check with product specific databases.
	if (setTableNames.size() == 0 && setDBInfoRows.size() == 0 && setFeatureNames.size() == 0)
	{
		return vecUnrecognizedElements;
	}

	// Get a list of all installed & licensed product-specific database managers.
	IIUnknownVectorPtr ipProdSpecificMgrs = getLicensedProductSpecificMgrs();
	ASSERT_RESOURCE_ALLOCATION("ELI31394", ipProdSpecificMgrs != __nullptr);

	// Loop through the managers asking them to remove from the list of existing tables and DBInfo
	// rows the elements they have managed since FAM DB schema version 23.
	long nCountProdSpecMgrs = ipProdSpecificMgrs->Size();
	for (long i = 0; i < nCountProdSpecMgrs; i++)
	{
		UCLID_FILEPROCESSINGLib::IProductSpecificDBMgrPtr ipProdSpecificDBMgr =
			ipProdSpecificMgrs->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI31395", ipProdSpecificDBMgr != __nullptr);

		IVariantVectorPtr ipVecProdSpecificDBInfoRows = ipProdSpecificDBMgr->GetDBInfoRows();
		ASSERT_RESOURCE_ALLOCATION("ELI31396", ipVecProdSpecificDBInfoRows != __nullptr);

		long nCountDBInfoRows = ipVecProdSpecificDBInfoRows->Size;
		for (long j = 0; j < nCountDBInfoRows; j++)
		{
			string strDBInfoRow = asString(ipVecProdSpecificDBInfoRows->GetItem(j).bstrVal);
			makeUpperCase(strDBInfoRow);

			setDBInfoRows.erase(strDBInfoRow);
		}

		IVariantVectorPtr ipVecProdSpecificTables = ipProdSpecificDBMgr->GetTables();
		ASSERT_RESOURCE_ALLOCATION("ELI31397", ipVecProdSpecificTables != __nullptr);

		long nCountTables = ipVecProdSpecificTables->Size;
		for (long j = 0; j < nCountTables; j++)
		{
			string strTableName = asString(ipVecProdSpecificTables->GetItem(j).bstrVal);
			makeUpperCase(strTableName);

			setTableNames.erase(strTableName);
		}
	}

	// If setDBInfoRows has any remaining values, add them to the list of unrecognized elements in
	// the DB.
	if (setDBInfoRows.size() > 0)
	{
		vecUnrecognizedElements.insert(vecUnrecognizedElements.end(),
			setDBInfoRows.begin(), setDBInfoRows.end());
	}

	// If setTableNames has any remaining values, add them to the list of unrecognized elements in
	// the DB.
	if (setTableNames.size() > 0)
	{
		vecUnrecognizedElements.insert(vecUnrecognizedElements.end(),
			setTableNames.begin(), setTableNames.end());
	}

	return vecUnrecognizedElements;
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::addOldDBInfoValues(map<string, string>& mapOldValues)
{
	// Version 107 - Changed the name of the SkipAuthenticationOnMachines to
	//		SkipAuthenticationForServiceOnMachines, need to add the old name
	//		to the map so that a database that still contains the old name
	//		is not treated as an unrecognized element
	mapOldValues["SkipAuthenticationOnMachines"] = "";
	// https://extract.atlassian.net/browse/ISSUE-12373
	// Version 122 - Removed ability to turn off auto-revert
	mapOldValues["AutoRevertLockedFiles"] = "";
	// https://extract.atlassian.net/browse/ISSUE-12789
	// Version 128 - Storing FAMSession data is now mandatory.
	mapOldValues["StoreFAMSessionHistory"] = "";
	// https://extract.atlassian.net/browse/ISSUE-15054
	// Version 161 - Input event tracking is always enabled
	mapOldValues["EnableInputEventTracking"] = "";
	mapOldValues["InputEventHistorySize"] = "";
	// Version 192 - UseGetFilesLegacy is no longer used
	mapOldValues["UseGetFilesLegacy"] = "";
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::addOldTables(vector<string>& vecTables)
{
	// Version 110 - ProcessingFAM has become ActiveFAM
	vecTables.push_back(gstrPROCESSING_FAM);
	// Version 116 - LaunchApp has become FileListHandlers
	vecTables.push_back(gstrDB_LAUNCH_APP);
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::executeProdSpecificSchemaUpdateFuncs(_ConnectionPtr ipConnection,
	IIUnknownVectorPtr ipProdSpecificMgrs, int nFAMSchemaVersion, long *pnStepCount,
	IProgressStatusPtr ipProgressStatus, map<string, long> &rmapProductSpecificVersions)
{
	// Loop through all installed & licensed product-specific DB managers and call
	// UpdateSchemaForFAMDBVersion for each.
	long nCountProdSpecMgrs = ipProdSpecificMgrs->Size();
	for (long i = 0; i < nCountProdSpecMgrs; i++)
	{
		UCLID_FILEPROCESSINGLib::IProductSpecificDBMgrPtr ipProdSpecificDBMgr =
			ipProdSpecificMgrs->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI31399", ipProdSpecificDBMgr != __nullptr);

		ICategorizedComponentPtr ipComponent(ipProdSpecificDBMgr);
		ASSERT_RESOURCE_ALLOCATION("ELI31400", ipComponent != __nullptr);

		string strId = asString(ipComponent->GetComponentDescription());
		
		ipProdSpecificDBMgr->UpdateSchemaForFAMDBVersion(getThisAsCOMPtr(), ipConnection,
			nFAMSchemaVersion, &(rmapProductSpecificVersions[strId]), pnStepCount, ipProgressStatus);
	}
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr CFileProcessingDB::getWorkItemFromFields(const FieldsPtr& ipFields)
{
	// Make sure the ipFields argument is not NULL
	ASSERT_ARGUMENT("ELI36878", ipFields != __nullptr);

	UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr ipWorkItemRecord(CLSID_WorkItemRecord);
	ASSERT_RESOURCE_ALLOCATION("ELI36879", ipWorkItemRecord != __nullptr);
	
	// Set the file data from the fields collection (set ActionID to 0)
	ipWorkItemRecord->WorkItemID = getLongField(ipFields, "ID");
	ipWorkItemRecord->WorkItemGroupID = getLongField(ipFields, "WorkItemGroupID");
	switch (getStringField(ipFields, "Status")[0])
	{
	case 'P': 
		ipWorkItemRecord->Status = (UCLID_FILEPROCESSINGLib::EWorkItemStatus)kWorkUnitPending;
		break;
	case 'R':
		ipWorkItemRecord->Status = (UCLID_FILEPROCESSINGLib::EWorkItemStatus)kWorkUnitProcessing;
		break;
	case 'C':
		ipWorkItemRecord->Status = (UCLID_FILEPROCESSINGLib::EWorkItemStatus)kWorkUnitComplete;
		break;
	case 'F':		
		ipWorkItemRecord->Status = (UCLID_FILEPROCESSINGLib::EWorkItemStatus)kWorkUnitFailed;
		break;
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI36877");
	};

	ipWorkItemRecord->Input = getStringField(ipFields, "Input").c_str();
	ipWorkItemRecord->Output = getStringField(ipFields, "Output").c_str();
	ipWorkItemRecord->FAMSessionID = getLongField(ipFields, "FAMSessionID");
	ipWorkItemRecord->StringizedException = getStringField(ipFields, "StringizedException").c_str();
	ipWorkItemRecord->FileName = getStringField(ipFields, "FileName").c_str();
	ipWorkItemRecord->BinaryOutput = getIPersistObjFromField(ipFields, "BinaryOutput");
	ipWorkItemRecord->BinaryInput = getIPersistObjFromField(ipFields, "BinaryInput");
	ipWorkItemRecord->FileID = getLongField(ipFields, "FileID");
	ipWorkItemRecord->WorkGroupFAMSessionID = getLongField(ipFields, "WorkGroupFAMSessionID");
	ipWorkItemRecord->Priority = (UCLID_FILEPROCESSINGLib::EFilePriority) getLongField(ipFields, "Priority");
	ipWorkItemRecord->RunningTaskDescription = getStringField(ipFields, "RunningTaskDescription").c_str();

	return ipWorkItemRecord;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr CFileProcessingDB::setWorkItemToProcessing(bool bDBLocked, 
	string strActionName, bool bRestrictToFAMSessionID, EFilePriority eMinPriority,
	const _ConnectionPtr &ipConnection)
{
	IIUnknownVectorPtr ipWorkItems = setWorkItemsToProcessing(bDBLocked, strActionName, 1,
		bRestrictToFAMSessionID, kPriorityDefault, ipConnection);
	ASSERT_RESOURCE_ALLOCATION("ELI37421", ipWorkItems != __nullptr);

	// if the size is not 1 then return a null pointer
	if (ipWorkItems->Size() != 1)
	{
		return __nullptr;
	}
	return ipWorkItems->At(0);
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CFileProcessingDB::setWorkItemsToProcessing(bool bDBLocked, string strActionName, 
	long nNumberToGet, bool bRestrictToFAMSessionID, EFilePriority eMinPriority,
	const _ConnectionPtr &ipConnection)
{
	// Declare query string so that if there is an exception the query can be added to debug info
	map<string, variant_t> params;
	try
	{
		try
		{
			// Revert any workitems associated with timed out FAMs
			revertTimedOutWorkItems(bDBLocked, ipConnection);

			bool bTransactionSuccessful = false;

			// Start the stopwatch to use to check for transaction timeout
			StopWatch swTransactionRetryTimeout;
			swTransactionRetryTimeout.start();

			// Set to the query to get workitems to process
			string strActionIDs = getActionIDsForActiveWorkflow(ipConnection, strActionName);
			params =
			{
				{"@|<VT_INT>ActionIDs", strActionIDs.c_str()},
				{"@FAMSessionID", m_nFAMSessionID},
				{"@GroupFAMSessionID", (bRestrictToFAMSessionID) ? m_nFAMSessionID : 0},
				{"@MaxWorkItems", nNumberToGet},
				{"@MinPriority", (int)eMinPriority}
			};
			auto cmd = buildCmd(ipConnection, gstrGET_WORK_ITEM_TO_PROCESS, params);

			UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr ipWorkItem = __nullptr;

			IIUnknownVectorPtr ipWorkItems(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI37422", ipWorkItems != __nullptr);

			long nTransactionRetryCount = 0;

			// Retry the transaction until successful
			while (!bTransactionSuccessful)
			{
				try
				{
					// Begin a transaction
					TransactionGuard tg(ipConnection, adXactRepeatableRead, &m_criticalSection);

					try
					{
                        variant_t vtRecordsAffected = 0L;
						_RecordsetPtr ipWorkItemSet = cmd->Execute(&vtRecordsAffected, nullptr, adCmdText);
						ASSERT_RESOURCE_ALLOCATION("ELI36887", ipWorkItemSet != __nullptr);

						while (!asCppBool(ipWorkItemSet->adoEOF))
						{
							// Get the fields from the file set
							FieldsPtr ipFields = ipWorkItemSet->Fields;
							ASSERT_RESOURCE_ALLOCATION("ELI36888", ipFields != __nullptr);

							ipWorkItem = getWorkItemFromFields(ipFields);

							ipWorkItems->PushBack(ipWorkItem);
							
							ipWorkItemSet->MoveNext();
						}
						// Commit the changes to the database
						tg.CommitTrans();
						bTransactionSuccessful = true;
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36889");
				}
				catch (UCLIDException &ue)
				{
					// Check if this is a bad connection
					if (!isConnectionAlive(ipConnection))
					{
						// if the connection is not alive just rethrow the exception 
						throw ue;
					}

					if (nTransactionRetryCount == 0)
					{
						UCLIDException uex("ELI51732", "Start transaction retry to set work items to processing", ue);
						uex.log();
					}

					// Check to see if the timeout value has been reached
					if (swTransactionRetryTimeout.getElapsedTime() > m_dGetFilesToProcessTransactionTimeout)
					{
						UCLIDException uex("ELI36890", "Application Trace: Transaction retry timed out.", ue);
						uex.addDebugInfo(gstrGET_FILES_TO_PROCESS_TRANSACTION_TIMEOUT, 
							asString(m_dGetFilesToProcessTransactionTimeout));
						throw uex;
					}

					nTransactionRetryCount++;
					// In the case that the exception is because the database has gotten into an
					// inconsistent state (as with LegacyRCAndUtiles:6350), use a small sleep here to
					// prevent thousands (or millions) of successive failures which may bog down the
					// DB and burn through table IDs.
					Sleep(100);
				}
			}
			return ipWorkItems;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36891");
	}
	catch (UCLIDException &ue)
	{
		ue.addDebugInfo("Record Query", gstrGET_WORK_ITEM_TO_PROCESS, true);
		for each (auto p in params)
		{
			ue.addDebugInfo(p.first, p.second);
		}
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::revertTimedOutWorkItems(bool bDBLocked, const _ConnectionPtr &ipConnection)
{
	if (m_bWorkItemRevertInProgress)
	{
		return;
	}

	m_bWorkItemRevertInProgress = true;

	try
	{
		try
		{
			// Begin a transaction
			TransactionGuard tgRevert(ipConnection, adXactRepeatableRead, &m_criticalSection);

			// Reset any work items that have a status of processing but the FAM is no longer active.
			long nNumberWorkItemsReset = executeCmd(buildCmd(ipConnection, gstrRESET_TIMEDOUT_WORK_ITEM_QUERY,
				{ {"@TimeOutInSeconds",m_nAutoRevertTimeOutInMinutes * 60} }));

			// Commit the reverted files
			tgRevert.CommitTrans();

			m_bWorkItemRevertInProgress = false;

			// Only log the reset exception if one or more WorkItems were reset
			if (nNumberWorkItemsReset > 0)
			{
				UCLIDException ue ("ELI37767", "Application trace: Work Items reverted");
				ue.addDebugInfo("NumberOfWorkItemsReverted", nNumberWorkItemsReset);
				ue.log();
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37768")
	}
	catch(UCLIDException &ue)
	{
		m_bWorkItemRevertInProgress = false;

		// if the DB is not locked rethrow exception - if locked just log exception
		if (!bDBLocked)
		{
			throw;
		}
		ue.log();
	}
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::checkForNewDBManagers()
{
	// create a vector of all categories we care about.
	IVariantVectorPtr ipCategoryNames(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI37870", ipCategoryNames != __nullptr);

	// Only want to get all Product Specific DB managers
	ipCategoryNames->PushBack(get_bstr_t(FP_FAM_PRODUCT_SPECIFIC_DB_MGRS.c_str()));
	
	ICategoryManagerPtr ipCategoryManager(CLSID_CategoryManager);
	ASSERT_RESOURCE_ALLOCATION("ELI37871", ipCategoryManager != __nullptr);

	ipCategoryManager->CheckForNewComponents(ipCategoryNames);
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::checkDatabaseIDValid(_ConnectionPtr ipConnection, bool bRefreshData, bool bThrowIfInvalid, bool bIsRetry /*= false*/)
{
	if (!bRefreshData
		&& !m_DatabaseIDValues.m_strName.empty()
		&& m_DatabaseIDValues.m_strName != m_strDatabaseName)
	{
		// If the database name has changed, then force a full refresh of counter data.
		bRefreshData = true;
	}

	if (bRefreshData)
	{
		m_strEncryptedDatabaseID = "";
		m_bDatabaseIDValuesValidated = false;
		m_bLoggedInvalidDatabaseID = false;
		m_ipDBInfoSettings = __nullptr;
		m_DatabaseIDValues = DatabaseIDValues();
	}

	if (m_bDatabaseIDValuesValidated)
	{
		return true;
	}

	// If the database id is empty load from DBInfo - and if still empty it is invalid
	if (m_strEncryptedDatabaseID.empty())
	{
		// Load from the database
		m_strEncryptedDatabaseID = getDBInfoSetting(ipConnection, gstrDATABASEID, bThrowIfInvalid);
		if (m_strEncryptedDatabaseID.empty())
		{
			UCLIDException ueEmpty("ELI38795", "DatabaseID missing");
			if (bThrowIfInvalid)
			{
				throw ueEmpty;
			}
			else if (!m_bLoggedInvalidDatabaseID)
			{
				ueEmpty.log();
				m_bLoggedInvalidDatabaseID = true;
			}
			return false;
		}
	};

	try
	{
		DatabaseIDValues storedDatabaseIDValues(m_strEncryptedDatabaseID);
		m_DatabaseIDValues = storedDatabaseIDValues;

		if (ipConnection == __nullptr)
		{
			ipConnection = getDBConnectionRegardlessOfRole();
		}

		// If the caller asked to refresh, there is no need to retry (behave as if we already are)
		bIsRetry |= bRefreshData;

		if (storedDatabaseIDValues.CheckIfValid(ipConnection, m_strDatabaseServer, bThrowIfInvalid && bIsRetry))
		{
			m_bDatabaseIDValuesValidated = true;
			m_bLoggedInvalidDatabaseID = false;
			return true;
		}
		else if (!bIsRetry)
		{
			// In case the DatabaseID has changed since DBInfo settings were last read,force
			// a reload dbInfo values and try again.
			return checkDatabaseIDValid(ipConnection, true, bThrowIfInvalid, true);
		}
	}
	catch(...)
	{
		UCLIDException uexInvalid("ELI53108", "Invalid DatabaseID", uex::fromCurrent("ELI53107"));

		// if the not valid set the saved encrypted database ID string to an empty string 
		m_strEncryptedDatabaseID = "";

		if (bThrowIfInvalid)
		{
			throw uexInvalid;
		}
		else if (!m_bLoggedInvalidDatabaseID)
		{
			uexInvalid.log();
			m_bLoggedInvalidDatabaseID = true;
		}
	}
	
	// if the not valid set the saved encrypted database ID string to an empty string
	m_strEncryptedDatabaseID = "";

	return false;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::checkCountersValid(_ConnectionPtr& ipConnection, vector<DBCounter>* pvecDBCounters /*= __nullptr*/)
{
	bool bValid = true;

	// Create a pointer to a recordset
	_RecordsetPtr ipResultSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI53037", ipResultSet != __nullptr);

	// Open the secure counter table
	ipResultSet->Open(gstrSELECT_SECURE_COUNTER_WITH_MAX_VALUE_CHANGE.c_str(),
		_variant_t((IDispatch*)ipConnection, true), adOpenStatic, adLockReadOnly,
		adCmdText);

	while (!asCppBool(ipResultSet->adoEOF))
	{
		DBCounter dbCounter;
		dbCounter.LoadFromFields(ipResultSet->Fields);

		bValid = bValid && dbCounter.isValid(
			m_DatabaseIDValues, ipResultSet->Fields);

		if (pvecDBCounters != __nullptr)
		{
			pvecDBCounters->push_back(dbCounter);
		}

		ipResultSet->MoveNext();
	}

	return bValid;
}
//-------------------------------------------------------------------------------------------------
string CFileProcessingDB::updateCounters(_ConnectionPtr ipConnection, DBCounterUpdate &counterUpdates,
	UCLIDException &ueLog)
{
	string strReturnValue = "";
	ueLog.addDebugInfo("CodeType", "Update");

	// get the update operations
	vector<CounterOperation> &vecOperations = counterUpdates.m_vecOperations;

	// Get the new DatabaseID - will be the same as old except for the LastUpdated
	DatabaseIDValues newDatabaseIDValues = m_DatabaseIDValues;
	auto role = confirmNoRoleConnection("ELI53086", getAppRoleConnection());
	newDatabaseIDValues.m_stLastUpdated = getSQLServerDateTimeAsSystemTime(role->ADOConnection());

	// Get the time since the request was generated
	CTimeSpan tsDiff = CTime(newDatabaseIDValues.m_stLastUpdated) - CTime(counterUpdates.m_stTimeCodeGenerated);
	if (tsDiff.GetDays() >= 4 || tsDiff.GetDays() < 0)
	{
		// Code has expired
		UCLIDException ue("ELI38996", "Counter update code has expired.");
		ue.addDebugInfo("UpdateCodeDate", (LPCSTR)CTime(counterUpdates.m_stTimeCodeGenerated).Format(gstrDATE_TIME_FORMAT.c_str()));
		throw ue;
	}

	newDatabaseIDValues.CalculateHashValue(newDatabaseIDValues.m_nHashValue);

	// get a map of the standard counter names
	map<long, string> &mapCounterNames = DBCounter::ms_mapOfStandardNames;

	// Get the last issued FAMFile id
	executeCmdQuery(ipConnection,"SELECT cast(IDENT_CURRENT('FAMFile') as int) AS ID",
		false, &m_nLastFAMFileID);

	// Get map with the key as CounterId and the counter as a CounterOperation with operation set to kNone
	map<long, CounterOperation> mapCounters;
	getCounterInfo(mapCounters);
	vector<DBCounterChangeValue> vecCounterChanges;

	// Go thru all of the operations that are requested
	for (size_t i=0; i < vecOperations.size(); i++)
	{
		CounterOperation &counterOp = vecOperations[i];
		
		// Find the counter in the map of existing counters
		auto counter = mapCounters.find(counterOp.m_nCounterID);

		// Set the name of the non custom counters
		if (counterOp.m_nCounterID < 100)
		{
			counterOp.m_strCounterName = mapCounterNames[counterOp.m_nCounterID];
		}

		// check if the counter exists
		if (counterOp.m_eOperation == kCreate && counter != mapCounters.end())
		{
			UCLIDException ueCreate("ELI38911", "Counter already exists.");
			ueCreate.addDebugInfo("CounterID", counterOp.m_nCounterID);
			ueCreate.addDebugInfo("CounterName", counterOp.m_strCounterName);
			throw ueCreate;
		}
		else if (counterOp.m_eOperation != kCreate && counter == mapCounters.end())
		{
			UCLIDException ueMissingCounter("ELI38912", "Counter doesn't exist.");
			ueMissingCounter.addDebugInfo("CounterID", counterOp.m_nCounterID);
			ueMissingCounter.addDebugInfo("CounterName", counterOp.m_strCounterName);
			throw ueMissingCounter;
		}

		// Create the counter change records
		DBCounterChangeValue counterChange(m_DatabaseIDValues);
		counterChange.m_nCounterID = counterOp.m_nCounterID;
		counterChange.m_stUpdatedTime = newDatabaseIDValues.m_stLastUpdated;
		counterChange.m_nLastUpdatedByFAMSessionID = m_nFAMSessionID;
		counterChange.m_llMinFAMFileCount = m_nLastFAMFileID;
		counterChange.m_strComment = "Update";

		string strOperationPerformed;

		// Set the counterChange data for the operation and convert the 
		// Increase and Decrease operations to Sets ( for generating the SQL statements to 
		// update the value
		switch (counterOp.m_eOperation)
		{
		case kCreate:
			counterChange.m_nToValue = counterOp.m_nValue;
			counterChange.m_nFromValue = 0;
			counterChange.CalculateHashValue(counterChange.m_llHashValue);
			vecCounterChanges.push_back(counterChange);

			// Add a create record to the map
			mapCounters[counterOp.m_nCounterID] = counterOp;
			strOperationPerformed = "Created counter " + counterOp.m_strCounterName + " with " 
				+ commaFormatNumber((LONGLONG)counterOp.m_nValue) + " counts.";
			ueLog.addDebugInfo("Create counter", counterChange.m_nCounterID);
			ueLog.addDebugInfo("CounterName", mapCounters[counterOp.m_nCounterID].m_strCounterName);
			ueLog.addDebugInfo("CounterValue", mapCounters[counterOp.m_nCounterID].m_nValue);
			break;
		case kSet:
			counterChange.m_nToValue = counterOp.m_nValue;
			counterChange.m_nFromValue = counter->second.m_nValue;
			counterChange.CalculateHashValue(counterChange.m_llHashValue);
			vecCounterChanges.push_back(counterChange);

			// Update the counterOp record in map to the counterOp being performed
			mapCounters[counterOp.m_nCounterID] = counterOp;
			strOperationPerformed = "Set counter " + counterOp.m_strCounterName + " to "
				+ commaFormatNumber((LONGLONG)counterOp.m_nValue) + " counts.";
			ueLog.addDebugInfo("Set counter", counterChange.m_nCounterID);
			ueLog.addDebugInfo("CounterName", mapCounters[counterOp.m_nCounterID].m_strCounterName);
			ueLog.addDebugInfo("CounterValue", mapCounters[counterOp.m_nCounterID].m_nValue);
			break;
		case kIncrement:
			counterChange.m_nToValue = counter->second.m_nValue + counterOp.m_nValue;
			counterChange.m_nFromValue = counter->second.m_nValue;
			counterChange.CalculateHashValue(counterChange.m_llHashValue);
			vecCounterChanges.push_back(counterChange);

			// Modify the existing counterOp record in map to change to a set with the new counter value
			mapCounters[counterOp.m_nCounterID].m_eOperation = kSet;
			mapCounters[counterOp.m_nCounterID].m_nValue = counterChange.m_nToValue;
			strOperationPerformed = "Incremented counter " + counterOp.m_strCounterName + " by "
				+ commaFormatNumber((LONGLONG)counterOp.m_nValue) + ".";
			ueLog.addDebugInfo("Increment counter", counterChange.m_nCounterID);
			ueLog.addDebugInfo("CounterName", mapCounters[counterOp.m_nCounterID].m_strCounterName);
			ueLog.addDebugInfo("CounterValue", mapCounters[counterOp.m_nCounterID].m_nValue);
			break;
		case kDecrement:
			// An operation to decrement should set counter value to 0 if the decrement value
			// is larger than the value of the counter
			counterChange.m_nToValue = max(0, counter->second.m_nValue - counterOp.m_nValue);
			counterChange.m_nFromValue = counter->second.m_nValue;
			counterChange.CalculateHashValue(counterChange.m_llHashValue);
			vecCounterChanges.push_back(counterChange);

			// Modify the existing counterOp record in map to change to a set with the new counter value
			mapCounters[counterOp.m_nCounterID].m_eOperation = kSet;
			mapCounters[counterOp.m_nCounterID].m_nValue = counterChange.m_nToValue;

			strOperationPerformed = "Decremented counter " + counterOp.m_strCounterName + " by "
				+ commaFormatNumber((LONGLONG)counterOp.m_nValue) + ".";
			ueLog.addDebugInfo("Decrement counter", counterChange.m_nCounterID);
			ueLog.addDebugInfo("CounterName", mapCounters[counterOp.m_nCounterID].m_strCounterName);
			ueLog.addDebugInfo("CounterValue", mapCounters[counterOp.m_nCounterID].m_nValue);
			break;
		case kDelete:
			ueLog.addDebugInfo("Delete counter", counterChange.m_nCounterID);
			ueLog.addDebugInfo("CounterName", mapCounters[counterOp.m_nCounterID].m_strCounterName);
			ueLog.addDebugInfo("CounterValue", mapCounters[counterOp.m_nCounterID].m_nValue);

			// Modify existing counterOp record to the delete op
			mapCounters[counterOp.m_nCounterID] = counterOp;

			strOperationPerformed = "Deleted counter " + counterOp.m_strCounterName;
			break;
		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI38913");
		}
		strReturnValue += strOperationPerformed + "\r\n";
	}

	// the mapCounters has the changes that need to be made to the SecureCounter table
	// and the vecCounterChanges has the changes that need to be added to the SecureCounterChange table
	storeNewDatabaseID(*role, newDatabaseIDValues);

	vector<string> vecUpdateQueries;

	// Add the queries to update the counter records
	createCounterUpdateQueries(newDatabaseIDValues, vecUpdateQueries, mapCounters);

	// add counter change value records
	for (auto scvc = vecCounterChanges.begin(); scvc != vecCounterChanges.end(); ++scvc)
	{
		vecUpdateQueries.push_back((*scvc).GetInsertQuery());
	}

	// Apply the changes with the new counter change records
	executeVectorOfSQL(ipConnection, vecUpdateQueries);
	
	return strReturnValue;
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::getCounterInfo(map<long, CounterOperation> &mapOfCounterOps, bool bCheckCounterHash)
{
	// Load counters from the database and check that they the SecureCounterValueChange table last
	// counter value matches the encrypted counter value.
	auto role = getAppRoleConnection();
	ADODB::_ConnectionPtr ipConnection = role->ADOConnection();
	
	// Create a pointer to a recordset
	_RecordsetPtr ipResultSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI38915", ipResultSet != __nullptr);
	
	ipResultSet->Open(gstrSELECT_SECURE_COUNTER_WITH_MAX_VALUE_CHANGE.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
		adLockReadOnly, adCmdText);
	while (!asCppBool(ipResultSet->adoEOF))
	{
		DBCounter dbCounter;
		DBCounterChangeValue counterChange(m_DatabaseIDValues);

		FieldsPtr fields = ipResultSet->Fields;
		dbCounter.LoadFromFields(ipResultSet->Fields);

		if (bCheckCounterHash)
		{
			// Check the counter hash and validate counter value against the last ChangeValue for the
			// counter
			dbCounter.validate(m_DatabaseIDValues);
		}

		CounterOperation counterOp(dbCounter);
	
		mapOfCounterOps[dbCounter.m_nID] = counterOp;

		ipResultSet->MoveNext();
	}
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::createCounterUpdateQueries(const DatabaseIDValues &databaseIDValues, 
	vector<string> &vecCounterUpdates, map<long, CounterOperation> &mapCounters)
{
	// Go thru all counterOp records in map and create the update queries
	// All existing counter records have to be updated to be encrypted with the new database id
	for (auto c = mapCounters.begin(); c != mapCounters.end(); ++c)
	{
		vecCounterUpdates.push_back(c->second.GetSQLQuery(databaseIDValues));
	}
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::unlockCounters(_ConnectionPtr ipConnection, DBCounterUpdate &counterUpdates,
	UCLIDException &ueLog)
{
	auto role = confirmNoRoleConnection("ELI53115", getAppRoleConnection());
	
	DatabaseIDValues& newDatabaseID = counterUpdates.m_DatabaseID;

	// Update the newDatabaseID DatabaseID to have a new last updated time before checking validity
	// as this time is part of the validity check.
	newDatabaseID.m_stLastUpdated = getSQLServerDateTimeAsSystemTime(role->ADOConnection());

	// Check the unlock code databaseID, it should be valid since the request code contained the 
	// fixed up version
	bool bValid = newDatabaseID.CheckIfValid(ipConnection, m_strDatabaseServer, false);
	if (!bValid)
	{
		// Unlock code is not valid
		UCLIDException ue("ELI38977", "Unlock code is invalid.");
		counterUpdates.m_DatabaseID.addAsDebugInfo(ue, "UnlockCode");
		throw ue;
	}
	ueLog.addDebugInfo("CodeType", "Unlock");

	// Get the time since the request was generated
	CTimeSpan tsDiff = CTime(newDatabaseID.m_stLastUpdated) - CTime(counterUpdates.m_stTimeCodeGenerated);
	if (tsDiff.GetDays() >= 4 || tsDiff.GetDays() < 0)
	{
		// Code has expired
		UCLIDException ue("ELI38988", "Unlock code has expired.");
		ue.addDebugInfo("UnlockCodeDate", (LPCSTR)CTime(counterUpdates.m_stTimeCodeGenerated).Format(gstrDATE_TIME_FORMAT.c_str()));
		throw ue;
	}

	// Update the hash
	newDatabaseID.CalculateHashValue(newDatabaseID.m_nHashValue);

	// Get the last issued FAMFile id
	executeCmdQuery(ipConnection,"SELECT cast(IDENT_CURRENT('FAMFile') as int) AS ID",
		false, &m_nLastFAMFileID);

	// Get map with the key as CounterId and the counter as a CounterOperation with operation set to kNone
	map<long, CounterOperation> mapCounters;
	// Get the counters from SecureCounter table but don't check the hash (the counterID portion of the hash
	// will still be checked
	getCounterInfo(mapCounters, false);
	vector<DBCounterChangeValue> vecCounterChanges;

	// The number of records in mapCounters should be the same as the number of operations in counterUpdates.m_vecOperations
	if (mapCounters.size() != counterUpdates.m_vecOperations.size())
	{
		UCLIDException ue("ELI38978", "Number of counters in unlock code does not match the number of counters.");
		ue.addDebugInfo("NumberInUnlockCode", counterUpdates.m_vecOperations.size());
		ue.addDebugInfo("NumberInDatabase", mapCounters.size());
		throw ue;
	}

	vector<string> vecUpdateQueries;

	// Put the operations from the unlock code in a map
	map<long, CounterOperation> mapUnlockCounters;
	for (size_t i = 0; i < counterUpdates.m_vecOperations.size(); i++)
	{
		CounterOperation &operation = counterUpdates.m_vecOperations[i];
		auto foundCounter = mapCounters.find(operation.m_nCounterID);
		if (foundCounter == mapCounters.end())
		{
			UCLIDException ue("ELI38980", "Counter in unlock code does not exist.");
			ue.addDebugInfo("CounterID", operation.m_nCounterID);
			throw ue;
		}
		CounterOperation &existingCounter = foundCounter->second;

		if (operation.m_eOperation != kNone && existingCounter.m_eOperation != kNone
			&& operation.m_nValue < existingCounter.m_nValue)
		{
			UCLIDException ue("ELI38979", "Unlock code counter information invalid.");
			throw ue;
		}
		vecUpdateQueries.push_back(existingCounter.GetSQLQuery(newDatabaseID));
		vecUpdateQueries.push_back(getQueryToResetCounterCorruption(existingCounter,
			newDatabaseID, ueLog));
	}
	
	storeNewDatabaseID(*role, newDatabaseID);

	// Only once the new database ID is in place, run the counter update queries.
	executeVectorOfSQL(ipConnection, vecUpdateQueries);
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::createAndStoreNewDatabaseID(const NoRoleConnection& noAppRoleConnection)
{
	_ConnectionPtr ipConnection = noAppRoleConnection.ADOConnection();

	TransactionGuard tg(ipConnection, adXactRepeatableRead, __nullptr);

	// Create a new DatabaseID and encrypt it
	ByteStream bsDatabaseID;
	createDatabaseID(ipConnection, m_strDatabaseServer, bsDatabaseID);

	ByteStream bsPW;
	getFAMPassword(bsPW);
	m_strEncryptedDatabaseID = MapLabel::setMapLabelWithS(bsDatabaseID, bsPW);

	executeCmd(buildCmd(ipConnection, gstADD_UPDATE_DBINFO_SETTING,
		{
			{gstrSETTING_NAME.c_str(), gstrDATABASEID.c_str()}
			,{gstrSETTING_VALUE.c_str(), m_strEncryptedDatabaseID.c_str()}
			,{"@UserID", getFAMUserID(ipConnection)}
			,{"@MachineID", getMachineID(ipConnection)}
			,{gstrSAVE_HISTORY.c_str(), (m_bStoreDBInfoChangeHistory) ? 1 : 0 }
		}));
					
	// The DatabaseID should be valid now so check it and throw exception if it isn't
	checkDatabaseIDValid(ipConnection, true, true);

	// Whenever the database ID changes, the passwords for application roles need to changed
	// to be based off the new m_nHashValue.
	CppSqlApplicationRole::UpdateAllExtractRoles(ipConnection, m_DatabaseIDValues.m_nHashValue);

	tg.CommitTrans();

	auto ue = UCLIDException("ELI53036", "Application Trace: New database ID created");
	ue.addDebugInfo("DatabaseID", asString(m_DatabaseIDValues.m_GUID));
	ue.log();
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::isFileInPagination(_ConnectionPtr ipConnection, long nFileID)
{
	bool bResult = false;
	
	long nId = 0;
	bResult = getCmdId(buildCmd(ipConnection, gstrACTIVE_PAGINATION_FILEID,
		{ { "@FileID", nFileID } })
		, &nId);

	return bResult;
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::updateDatabaseIDAndSecureCounterTablesSchema183(const NoRoleConnection& noAppRoleConnection)
{
	try
	{
		_ConnectionPtr ipConnection = noAppRoleConnection.ADOConnection();
		TransactionGuard tg(ipConnection, adXactChaos, __nullptr);
		createAndStoreNewDatabaseID(noAppRoleConnection);

		UCLIDException ueLog("ELI49974", "Application Trace: Database counters updated.");
		vector<string> vecQueries;

		// Get map with the key as CounterId and the counter as a CounterOperation with operation set to kNone
		map<long, CounterOperation> mapCounters;
		// Get the counters from SecureCounter table but don't check the hash (the counterID portion of the hash
		// will still be checked
		getCounterInfo(mapCounters, false);
		auto iter = mapCounters.begin();
		for (auto iter = mapCounters.begin(); iter != mapCounters.end(); iter++)
		{
			CounterOperation &existingCounter = iter->second;
			vecQueries.push_back(existingCounter.GetSQLQuery(m_DatabaseIDValues));
			vecQueries.push_back(getQueryToResetCounterCorruption(existingCounter,
				m_DatabaseIDValues, ueLog, "Schema Update"));
		}

		executeVectorOfSQL(ipConnection, vecQueries);
		m_ipDBInfoSettings = __nullptr;

		ueLog.log();

		tg.CommitTrans();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46486");
}

//-------------------------------------------------------------------------------------------------

//-------------------------------------------------------------------------------------------------
string CFileProcessingDB::getQueryToResetCounterCorruption(CounterOperation counter,
	DatabaseIDValues databaseID, UCLIDException &ueLog, string strComment)
{
	vector<string> resultQueries;
	
	// Create the counter change records
	DBCounterChangeValue counterChange(m_DatabaseIDValues);
	counterChange.m_nCounterID = counter.m_nCounterID;
	counterChange.m_stUpdatedTime = databaseID.m_stLastUpdated;
	counterChange.m_nLastUpdatedByFAMSessionID = m_nFAMSessionID;
	counterChange.m_llMinFAMFileCount = m_nLastFAMFileID;
	counterChange.m_strComment = strComment;
	counterChange.m_nFromValue = counter.m_nValue;
	counterChange.m_nToValue = counter.m_nValue;
	counterChange.CalculateHashValue(counterChange.m_llHashValue);

	ueLog.addDebugInfo("CounterID", counter.m_nCounterID);
	ueLog.addDebugInfo("CounterName", counter.m_strCounterName);
	ueLog.addDebugInfo("CounterValue", counter.m_nValue);
	
	return counterChange.GetInsertQuery();
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::storeNewDatabaseID(const NoRoleConnection& noAppRoleConnection
	, DatabaseIDValues databaseID)
{
	_ConnectionPtr ipConnection = noAppRoleConnection.ADOConnection();

	TransactionGuard tg(ipConnection, adXactRepeatableRead, __nullptr);

	// Need to updated the DatabaseID record
	ByteStream bsNewDBID;
	ByteStreamManipulator bsmNewDBID(ByteStreamManipulator::kWrite, bsNewDBID);
	bsmNewDBID << databaseID;
	bsmNewDBID.flushToByteStream(8);

	ByteStream bsFAMPassword;
	getFAMPassword(bsFAMPassword);

	string strNewEncryptedDBID = MapLabel::setMapLabelWithS(bsNewDBID, bsFAMPassword);

	executeCmd(buildCmd(ipConnection, gstADD_UPDATE_DBINFO_SETTING,
		{
			{gstrSETTING_NAME.c_str(), gstrDATABASEID.c_str()}
			,{gstrSETTING_VALUE.c_str(), strNewEncryptedDBID.c_str()}
			,{"@UserID", 0}
			,{"@MachineID",0}
			,{gstrSAVE_HISTORY.c_str(), 0 }
		}));

	checkDatabaseIDValid(ipConnection, true, true);

	// Whenever the database ID changes, the passwords for application roles need to changed
	// to be based off the new m_nHashValue.
	CppSqlApplicationRole::UpdateAllExtractRoles(ipConnection, m_DatabaseIDValues.m_nHashValue);

	tg.CommitTrans();
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IWorkflowDefinitionPtr CFileProcessingDB::getWorkflowDefinition(
	_ConnectionPtr ipConnection, long nID)
{
	_RecordsetPtr ipWorkflowSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI41893", ipWorkflowSet != __nullptr);

	string strQuery =
			"SELECT [ID] "
			", [Name] "
			", [WorkflowTypeCode] "
			", [Description] "
			", [StartActionID] "
			", [EditActionID] "
			", [PostEditActionID] "
			", [EndActionID] "
			", [PostWorkflowActionID] "
			", [DocumentFolder] "
			", [OutputAttributeSetID] "
			", [OutputFileMetadataFieldID] "
			", [OutputFilePathInitializationFunction] "
			", [LoadBalanceWeight] "
			"	FROM [Workflow]"
			"	WHERE [ID] = @WorkflowID";
	auto cmd = buildCmd(ipConnection, strQuery, { {"@WorkflowID", nID} });

	ipWorkflowSet->Open((IDispatch*)cmd, vtMissing, adOpenStatic, adLockReadOnly, adCmdText);

	if (asCppBool(ipWorkflowSet->adoEOF))
	{
		UCLIDException ue("ELI41894", "Failed to get workflow definition.");
		ue.addDebugInfo("ID", nID);
		throw ue;
	}

	FieldsPtr ipFields = ipWorkflowSet->Fields;
	ASSERT_RESOURCE_ALLOCATION("ELI41895", ipFields != __nullptr);

	UCLID_FILEPROCESSINGLib::IWorkflowDefinitionPtr ipWorkflowDefinition(CLSID_WorkflowDefinition);
	ASSERT_RESOURCE_ALLOCATION("ELI41896", ipWorkflowDefinition != __nullptr);

	ipWorkflowDefinition->ID = getLongField(ipFields, "ID");
	ipWorkflowDefinition->Name = getStringField(ipFields, "Name").c_str();
	char eWorkflowType = getStringField(ipFields, "WorkflowTypeCode").c_str()[0];
	switch (eWorkflowType)
	{
	case 'U': ipWorkflowDefinition->Type = UCLID_FILEPROCESSINGLib::kUndefined; break;
	case 'R': ipWorkflowDefinition->Type = UCLID_FILEPROCESSINGLib::kRedaction; break;
	case 'E': ipWorkflowDefinition->Type = UCLID_FILEPROCESSINGLib::kExtraction; break;
	case 'C': ipWorkflowDefinition->Type = UCLID_FILEPROCESSINGLib::kClassification; break;
	}
	ipWorkflowDefinition->Description = getStringField(ipFields, "Description").c_str();
	ipWorkflowDefinition->StartAction = isNULL(ipFields, "StartActionID")
		? ""
		: get_bstr_t(getActionName(ipConnection, getLongField(ipFields, "StartActionID")));
	ipWorkflowDefinition->EndAction = isNULL(ipFields, "EndActionID")
		? ""
		: get_bstr_t(getActionName(ipConnection, getLongField(ipFields, "EndActionID")));
	ipWorkflowDefinition->PostWorkflowAction = isNULL(ipFields, "PostWorkflowActionID")
		? ""
		: get_bstr_t(getActionName(ipConnection, getLongField(ipFields, "PostWorkflowActionID")));
	ipWorkflowDefinition->DocumentFolder = getStringField(ipFields, "DocumentFolder").c_str();
	
	ipWorkflowDefinition->EditAction = isNULL(ipFields, "EditActionID")
		? ""
		: get_bstr_t(getActionName(ipConnection, getLongField(ipFields, "EditActionID")));
	ipWorkflowDefinition->PostEditAction = isNULL(ipFields, "PostEditActionID")
		? ""
		: get_bstr_t(getActionName(ipConnection, getLongField(ipFields, "PostEditActionID")));

	if (isNULL(ipFields, "OutputAttributeSetID"))
	{
		ipWorkflowDefinition->OutputAttributeSet = "";
	}
	else
	{
		long long llAttributeSetID = getLongLongField(ipFields, "OutputAttributeSetID");

		string strAttributeSetQuery = "SELECT [Description] FROM [dbo].[AttributeSetName] WHERE [ID]= @AttributeSetID";

		auto cmd = buildCmd(ipConnection, strAttributeSetQuery, { {"@AttributeSetID", llAttributeSetID} });

		_RecordsetPtr ipAttributeSetResult(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI41919", ipAttributeSetResult != __nullptr);

		ipAttributeSetResult->Open((IDispatch*)cmd, vtMissing, adOpenStatic, adLockReadOnly, adCmdText);

		ASSERT_RUNTIME_CONDITION("ELI41920", !asCppBool(ipAttributeSetResult->adoEOF),
			"Unknown attribute set ID");

		FieldsPtr ipAttributeSetFields = ipAttributeSetResult->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI41921", ipAttributeSetFields != __nullptr);

		ipWorkflowDefinition->OutputAttributeSet =
			get_bstr_t(getStringField(ipAttributeSetFields, "Description"));
	}

	if (isNULL(ipFields, "OutputFileMetadataFieldID"))
	{
		ipWorkflowDefinition->OutputFileMetadataField = "";
	}
	else
	{
		long lMetadataFieldID = getLongField(ipFields, "OutputFileMetadataFieldID");

		string strMetadataFieldQuery = "SELECT [Name] FROM [dbo].[MetadataField] WHERE [ID] = @MetadataFieldID";
		auto cmd = buildCmd(ipConnection, strMetadataFieldQuery, { {"@MetadataFieldID", lMetadataFieldID} });
		_RecordsetPtr ipMetadataFieldResult(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI42049", ipMetadataFieldResult != __nullptr);

		ipMetadataFieldResult->Open((IDispatch*)cmd, vtMissing,
			adOpenStatic, adLockReadOnly, adCmdText);

		ASSERT_RUNTIME_CONDITION("ELI42050", !asCppBool(ipMetadataFieldResult->adoEOF),
			"Unknown metadata field ID");

		FieldsPtr ipMetadataFieldFields = ipMetadataFieldResult->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI42051", ipMetadataFieldFields != __nullptr);

		ipWorkflowDefinition->OutputFileMetadataField =
			get_bstr_t(getStringField(ipMetadataFieldFields, "Name"));
	}

	ipWorkflowDefinition->OutputFilePathInitializationFunction = isNULL(ipFields, "OutputFilePathInitializationFunction")
		? ""
		: (_bstr_t)ipFields->Item["OutputFilePathInitializationFunction"]->GetValue();

	ipWorkflowDefinition->LoadBalanceWeight = getLongField(ipFields, "LoadBalanceWeight");

	return ipWorkflowDefinition;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IWorkflowDefinitionPtr CFileProcessingDB::getCachedWorkflowDefinition(
	_ConnectionPtr ipConnection, long nWorkflowID /*= -1*/)
{
	CSingleLock lock(&m_criticalSection, TRUE);

	if (nWorkflowID < 1)
	{
		nWorkflowID = getActiveWorkflowID(ipConnection);
		ASSERT_RUNTIME_CONDITION("ELI49567", nWorkflowID > 0, "No active workflow set!");
	}

	auto iterWorkflow = m_mapWorkflowDefinitions.find(nWorkflowID);
	if (iterWorkflow != m_mapWorkflowDefinitions.end())
	{
		return iterWorkflow->second;
	}
	else
	{
		UCLID_FILEPROCESSINGLib::IWorkflowDefinitionPtr ipWorkflowDef =
			getWorkflowDefinition(ipConnection, nWorkflowID);
		m_mapWorkflowDefinitions[nWorkflowID] = ipWorkflowDef;
		return ipWorkflowDef;
	}
}
//-------------------------------------------------------------------------------------------------
vector<tuple<long, string, bool>> CFileProcessingDB::getWorkflowActions(_ConnectionPtr ipConnection, long nWorkflowID)
{
	vector<tuple<long, string, bool>> vecWorkflowActions;

	_RecordsetPtr ipActionSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI41992", ipActionSet != __nullptr);

	string strQuery = "SELECT [ID], [ASCName], [MainSequence] "
			"FROM dbo.[Action] "
			"WHERE [WorkflowID] = @WorkflowID";

	auto cmd = buildCmd(ipConnection, strQuery, { {"@WorkflowID", nWorkflowID} });

	ipActionSet->Open((IDispatch*) cmd, vtMissing, adOpenStatic, adLockReadOnly, adCmdText);

	while (!asCppBool(ipActionSet->adoEOF))
	{
		FieldsPtr ipFields = ipActionSet->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI41993", ipFields != __nullptr);

		long nId = getLongField(ipFields, "ID");
		string strName = getStringField(ipFields, "ASCName");
		bool bMainSequence = isNULL(ipFields, "MainSequence")
			? true
			: getBoolField(ipFields, "MainSequence");

		vecWorkflowActions.push_back(make_tuple(nId, strName, bMainSequence));

		ipActionSet->MoveNext();
	}

	return vecWorkflowActions;
}
//-------------------------------------------------------------------------------------------------
vector<pair<string, string>> CFileProcessingDB::getWorkflowNamesAndIDs(_ConnectionPtr ipConnection)
{
	vector<pair<string, string>> vecNamesAndIDs;

	// Create a pointer to a recordset
	_RecordsetPtr ipWorkflowSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI41936", ipWorkflowSet != __nullptr);

	// Query to get the workflow 
	string strQuery = "SELECT ID, Name FROM dbo.Workflow";
		
	ipWorkflowSet->Open(strQuery.c_str(), _variant_t((IDispatch *)ipConnection, true), adOpenStatic,
		adLockReadOnly, adCmdText);

	// Step through all records
	while (ipWorkflowSet->adoEOF == VARIANT_FALSE)
	{
		// Get the fields from the workflows set
		FieldsPtr ipFields = ipWorkflowSet->Fields;
		ASSERT_RESOURCE_ALLOCATION("ELI41938", ipFields != __nullptr);

		// get the workflow name
		string strWorkflowName = getStringField(ipFields, "Name");

		// get the workflow ID
		long lID = getLongField(ipFields, "ID");
		string strID = asString(lID);

		vecNamesAndIDs.push_back(pair<string, string>(strWorkflowName, strID));

		// Move to the next record in the table
		ipWorkflowSet->MoveNext();
	}

	return vecNamesAndIDs;
}
//-------------------------------------------------------------------------------------------------
vector<tuple<long, string>> CFileProcessingDB::getWorkflowStatus(long nFileID, 
																 bool bReturnFileStatuses/* = false*/)
{
	vector<tuple<long, string>> vecStatuses;

	// This needs to be allocated outside the BEGIN_CONNECTION_RETRY
	ADODB::_ConnectionPtr ipConnection = __nullptr;

	BEGIN_CONNECTION_RETRY();

		auto role = getAppRoleConnection();
		ipConnection = role->ADOConnection();
		validateDBSchemaVersion();

		long nWorkflowID = getActiveWorkflowID(ipConnection);

		UCLID_FILEPROCESSINGLib::IWorkflowDefinitionPtr ipWorkflowDefinition =
			getCachedWorkflowDefinition(ipConnection, nWorkflowID);
		ASSERT_RESOURCE_ALLOCATION("ELI42137", ipWorkflowDefinition != __nullptr);

		vector<string> vecIncludedActionIDs;
		vector<tuple<long, string, bool>> vecWorkflowActions = getWorkflowActions(ipConnection, nWorkflowID);
		for each (tuple<long, string, bool> item in vecWorkflowActions)
		{
			if (get<2>(item))
			{
				vecIncludedActionIDs.push_back(asString(get<0>(item)));
			}
		}

		string strActionIDs = asString(vecIncludedActionIDs, true, ",");
		string strEndAction = asString(ipWorkflowDefinition->EndAction);
		ASSERT_RUNTIME_CONDITION("ELI46432", !strEndAction.empty(),
			"Workflow has not been properly configured; EndAction not defined.");
		long nEndActionID = getActionID(ipConnection, strEndAction);

        auto cmd = buildCmd(ipConnection, gstrGET_WORKFLOW_STATUS,
        {
            {"@FileID", nFileID},
            {"@WorkflowID", nWorkflowID},
            {"@|<VT_INT>ActionIDs", strActionIDs.c_str()},
            {"@EndActionID", nEndActionID},
            {"@ReturnFileStatuses", bReturnFileStatuses ? 1: 0}
        });

		_RecordsetPtr ipWorkflowStatus(__uuidof(Recordset));
		ASSERT_RESOURCE_ALLOCATION("ELI42138", ipWorkflowStatus);

		ipWorkflowStatus->Open((IDispatch *)cmd, vtMissing,
			adOpenForwardOnly, adLockReadOnly, adCmdText);

		while (ipWorkflowStatus->adoEOF == VARIANT_FALSE)
		{
			FieldsPtr ipFields = ipWorkflowStatus->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI42139", ipFields != __nullptr);

			if (bReturnFileStatuses)
			{
				int nFileID = getLongField(ipFields, "FileID", 0);
				string strStatus = getStringField(ipFields, "Status");
				vecStatuses.push_back(make_tuple(nFileID, strStatus));
			}
			else
			{
				string strStatus = getStringField(ipFields, "Status");
				long nCount = getLongField(ipFields, "Count");
				vecStatuses.push_back(make_tuple(nCount, strStatus));
			}

			ipWorkflowStatus->MoveNext();
		}

	END_CONNECTION_RETRY(ipConnection, "ELI42142");

	return vecStatuses;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingDB::databaseUsingWorkflows(_ConnectionPtr ipConnection)
{
	string strQuery = "SELECT COUNT([ID]) AS [ID] FROM [Workflow]\r\n";

	long nWorkflowCount = -1;
	executeCmdQuery(ipConnection, strQuery, false, &nWorkflowCount);

	return nWorkflowCount > 0;
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::setStatusForAllFiles(_ConnectionPtr ipConnection, const string& strAction,
	EActionStatus eStatus)
{
	long nWorkflowId = -1;
	string strActiveWorkflow = getActiveWorkflow();
	if (!strActiveWorkflow.empty())
	{
		nWorkflowId = getWorkflowID(ipConnection, strActiveWorkflow);
	}

	// Get the action ID and update the strActionName to stored value
	long nActionID = getActionID(ipConnection, strAction);

	string strActionStatus = asStatusString(eStatus);

	map<string, variant_t> mapParams;
	mapParams["@ActionID"] = nActionID;

	// Get the action ID as a string
	string strActionID = asString(nActionID);

	// Remove any records from the skipped file table that where skipped for this action
	string strDeleteSkippedSQL = "DELETE FROM [SkippedFile] WHERE ActionID = @ActionID";
	executeCmd(buildCmd(ipConnection, strDeleteSkippedSQL, mapParams));

	// Remove any records in the LockedFile table for status changing from Processing to pending
	string strDeleteLockedFiles = "DELETE FROM [LockedFile] WHERE ActionID = @ActionID";
	executeCmd(buildCmd(ipConnection, strDeleteLockedFiles, mapParams));

	// There are no cases where this method should not just ignore all pending entries in
	// [QueuedActionStatusChange] for the selected files.
	string strUpdateQueuedActionStatusChange =
		"UPDATE [QueuedActionStatusChange] SET [ChangeStatus] = 'I'"
		"WHERE [ChangeStatus] = 'P' AND [ActionID] = @ActionID";
	executeCmd(buildCmd(ipConnection, strUpdateQueuedActionStatusChange, mapParams));

	// If setting files to skipped, need to add skipped record for each file
	if (eStatus == kActionSkipped)
	{
		// Get the current user name
		string strUserName = (m_strFAMUserName.empty()) ? getCurrentUserName() : m_strFAMUserName;
		mapParams["@UserName"] = strUserName.c_str();

		// Add all files to the skipped table for this action
		string strSQL = "INSERT INTO [SkippedFile] ([FileID], [ActionID], [UserName]) ";
		if (nWorkflowId > 0)
		{
			mapParams["@WorkflowID"] = nWorkflowId;
			strSQL +=
				"(SELECT [FileID] AS ID, @ActionID AS ActionID, @UserName AS UserName FROM [WorkflowFile] WITH (NOLOCK) WHERE [WorkflowID] = @WorkflowID)";
		}
		else
		{
			strSQL +=
				"(SELECT [ID], @ActionID AS ActionID, @UserName AS UserName FROM [FAMFile])";
		}
		executeCmd(buildCmd(ipConnection, strSQL, mapParams));
	}

	map<string, variant_t> params;

	params["@ActionID"] = nActionID;
	params["@ActionStatus"] = strActionStatus.c_str();

	// Only want to change the status that is different from status that is being changed to
	string strWhere = " WHERE ActionStatus  <> @ActionStatus ";

	// Add the transition records
	addASTransFromSelect(ipConnection, params, strAction, nActionID, strActionStatus,
		"", "", strWhere, "");

	// if the new status is Unattempted
	if (eStatus == kActionUnattempted)
	{
		string strDeleteStatus = "DELETE FROM FileActionStatus "
			" WHERE ActionID = @ActionID";
		executeCmd(buildCmd(ipConnection, strDeleteStatus, { {"@ActionID", nActionID } }));
	}
	else
	{
		// Update status of existing records
		string strUpdateStatus = "UPDATE FileActionStatus SET ActionStatus = @ActionStatus "
			"FROM FileActionStatus INNER JOIN FAMFile ON FileActionStatus.FileID = FAMFile.ID AND "
			"FileActionStatus.ActionID = @ActionID ";
		if (nWorkflowId > 0)
		{
			strUpdateStatus +=
				" INNER JOIN [WorkflowFile] WITH (NOLOCK) ON [FileActionStatus].[FileID] = [WorkflowFile].[FileID]" 
				" AND [WorkflowID] = @WorkflowID";
			params["@WorkflowID"] = nWorkflowId;
		}
		executeCmd(buildCmd(ipConnection, strUpdateStatus, params));

		// Insert new records where previous status was 'U'
		string strInsertStatus = "INSERT INTO FileActionStatus "
			"(FileID, ActionID, ActionStatus, Priority) "
			" SELECT FAMFile.ID, @ActionID as ActionID, @ActionStatus"
			" AS ActionStatus, "
			"COALESCE(FileActionStatus.Priority, FAMFile.Priority) AS Priority "
			"FROM FAMFile ";
		if (nWorkflowId > 0)
		{
			strInsertStatus +=
				"INNER JOIN [WorkflowFile] WITH (NOLOCK) ON [FAMFile].[ID] = [WorkflowFile].[FileID] AND [WorkflowID] = @WorkflowID ";
		}
		strInsertStatus +=
			"LEFT JOIN FileActionStatus ON [FAMFile].[ID] = FileActionStatus.FileID "
			"	AND FileActionStatus.ActionID = @ActionID "
			"WHERE FileActionStatus.ActionID IS NULL";
		executeCmd(buildCmd(ipConnection, strInsertStatus, params));
	}

	// If going to complete status and AutoDeleteFileActionComments == true then
	// clear the file action comments
	if (eStatus == kActionCompleted && m_bAutoDeleteFileActionComment)
	{
		clearFileActionComment(ipConnection, -1, nActionID);
	}

	// update the stats
	reCalculateStats(ipConnection, nActionID);
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::modifyActionStatusForSelection(
	UCLID_FILEPROCESSINGLib::IFAMFileSelectorPtr ipFileSelector,
	string strToAction, string strNewStatus, string strFromAction, long* pnNumRecordsModified)
{
	_RecordsetPtr ipFileSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI30382", ipFileSet != __nullptr);

	_bstr_t bstrQueryFrom = ipFileSelector->BuildQuery(
		getThisAsCOMPtr(), "[FAMFile].[ID]", "", VARIANT_FALSE);

	auto role = getAppRoleConnection();
	_ConnectionPtr ipConnection = role->ADOConnection();

	// Open the file set
	ipFileSet->Open(bstrQueryFrom, _variant_t(ipConnection, true),
		adOpenForwardOnly, adLockReadOnly, adCmdText);

	// Create an empty file record object for the random condition.
	UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(CLSID_FileRecord);
	ipFileRecord->Name = "";
	ipFileRecord->FileID = 0;

	long nWorkflowID = getActiveWorkflowID(ipConnection);
	bool bFromSpecified = !strFromAction.empty();
	long nToActionID = 
		nWorkflowID <= 0
		? getActionID(ipConnection, strToAction)
		: getActionIDNoThrow(ipConnection, strToAction, nWorkflowID);
	long nFromActionID = bFromSpecified
		? getActionID(ipConnection, strFromAction)
		: 0;

	// Get the list of file ID's to modify
	long &nNumRecordsModified = *pnNumRecordsModified;
	vector<long> vecFileIds;
	while (ipFileSet->adoEOF == VARIANT_FALSE)
	{
		vecFileIds.push_back(getLongField(ipFileSet->Fields, "ID"));

		ipFileSet->MoveNext();
	}
	ipFileSet->Close();

	// Action id to change
	string strToActionID = asString(nToActionID);

	string strSelectQuery = "SELECT FAMFile.ID, FileName, FileSize, Pages, \r\n";

	if (nWorkflowID > 0)
	{
		strSelectQuery += "COALESCE(~[WorkflowFile].[Invisible], -1) AS IsFileInWorkflow, \r\n";
	}

	// Loop through the file Ids to change in groups of 10000 populating the SetFileActionData
	size_t count = vecFileIds.size();
	size_t i = 0;
	if (!bFromSpecified)
	{
		strSelectQuery += "COALESCE (ToFAS.Priority, FAMFile.Priority) AS Priority, \r\n"
			"COALESCE (ToFAS.ActionStatus, 'U') AS ToActionStatus \r\n"
			"FROM FAMFile \r\n"
			"LEFT JOIN FileActionStatus as ToFAS ON FAMFile.ID = ToFAS.FileID \r\n"
			"	AND ToFAS.ActionID = " + strToActionID + "\r\n";
	}
	else
	{
		strSelectQuery += "COALESCE(ToFAS.Priority, FAMFile.Priority) AS Priority, \r\n"
			"COALESCE(ToFAS.ActionStatus, 'U') AS ToActionStatus, \r\n"
			"COALESCE(FromFAS.ActionStatus, 'U') AS FromActionStatus \r\n"
			"FROM FAMFile \r\n"
			"LEFT JOIN FileActionStatus as ToFAS ON FAMFile.ID = ToFAS.FileID \r\n"
			"	AND ToFAS.ActionID = " + strToActionID + "\r\n"
			"LEFT JOIN FileActionStatus as FromFAS WITH (NOLOCK) ON FAMFile.ID = FromFAS.FileID \r\n"
			"	AND FromFAS.ActionID = " + asString(nFromActionID) + "\r\n";
	}

	if (count > 0 && nToActionID == -1)
	{
		// WARNING: This ELI code is referenced by ModifyActionStatusForSelection_Internal. Do not change.
		UCLIDException ue("ELI51514", Util::Format(
			"Cannot set %d file(s) in workflow \"%s\"; action \"%s\" does not exist.",
			count, getActiveWorkflow().c_str(), strToAction.c_str()));
		throw ue;
	}

	if (nWorkflowID > 0)
	{
		strSelectQuery += "\r\nLEFT JOIN [WorkflowFile] ON [FAMFile].[ID] = [WorkflowFile].[FileID] "
			"AND [WorkflowID] = " + asString(nWorkflowID) + "\r\n";
	}

	while (i < count)
	{
		map<string, vector<SetFileActionData>> mapFromStatusToId;

		string strQuery = strSelectQuery + "WHERE FAMFile.ID IN (";
		string strFileIds = asString(vecFileIds[i++]);
		for (int j = 1; i < count && j < 10000; j++)
		{
			strFileIds += ", " + asString(vecFileIds[i++]);
		}
		strQuery += strFileIds;
		strQuery += ")";

		ipFileSet->Open(strQuery.c_str(), _variant_t((IDispatch*)ipConnection, true),
			adOpenForwardOnly, adLockReadOnly, adCmdText);

		// Loop through each record
		while (ipFileSet->adoEOF == VARIANT_FALSE)
		{
			FieldsPtr ipFields = ipFileSet->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI30383", ipFields != __nullptr);

			long nFileID = getLongField(ipFields, "ID");
			EActionStatus oldStatus = asEActionStatus(getStringField(ipFields, "ToActionStatus"));

			// If copying from an action, get the status for the action
			if (bFromSpecified)
			{
				strNewStatus = getStringField(ipFields, "FromActionStatus");
			}

			bool isFileDeleted = (nWorkflowID > 0)
				? (getLongField(ipFields, "IsFileInWorkflow") == 0) // 0 = deleted
				: false;

			mapFromStatusToId[strNewStatus].push_back(SetFileActionData(nFileID,
				getFileRecordFromFields(ipFields, false), oldStatus, isFileDeleted));

			ipFileSet->MoveNext();
		}
		ipFileSet->Close();

		// Set the file action state for each vector of file data
		for (map<string, vector<SetFileActionData>>::iterator it = mapFromStatusToId.begin();
			it != mapFromStatusToId.end(); it++)
		{
			setFileActionState(ipConnection, it->second, strToAction, it->first);
			nNumRecordsModified += it->second.size();
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::setMetadataFieldValue(_ConnectionPtr connection, long nFileID,
	string strMetadataFieldName, string strMetadataFieldValue)
{
	string strFileID = asString(nFileID);
	executeCmd(buildCmd(connection,
		"DECLARE @fieldID INT "
		"SELECT @fieldID = [ID] FROM [MetadataField] "
		"	WHERE [Name] = @MetadataFieldName "

		"IF EXISTS (SELECT * FROM [FileMetadataFieldValue] WHERE [FileID] = @FileID AND [MetadataFieldID] = @fieldID) "
		"	UPDATE [FileMetadataFieldValue] SET [Value] = @MetadataFieldValue "
		"		WHERE [FileID] = @FileID AND [MetadataFieldID] = @fieldID "
		"ELSE "
		"	INSERT INTO [FileMetadataFieldValue] ([FileID], [MetadataFieldID], [Value]) "
		"		VALUES (@FileID, @fieldID, @MetadataFieldValue)",
		{
			{ "@FileID", nFileID },
			{ "@MetadataFieldName", strMetadataFieldName.c_str() },
			{ "@MetadataFieldValue", strMetadataFieldValue.c_str() }
		}));
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::initOutputFileMetadataFieldValue(_ConnectionPtr ipConnection,
	long nFileID, string strFileName, long nWorkflowID)
{
	UCLID_FILEPROCESSINGLib::IWorkflowDefinitionPtr ipWorkflowDefinition =
		getCachedWorkflowDefinition(ipConnection, nWorkflowID);
	ASSERT_RESOURCE_ALLOCATION("ELI43187", ipWorkflowDefinition != __nullptr);

	string strOutputFileMetadataField = ipWorkflowDefinition->OutputFileMetadataField;
	string strPath = ipWorkflowDefinition->OutputFilePathInitializationFunction;
	if (!strOutputFileMetadataField.empty() && !strPath.empty())
	{
		string strExpandedPath =
			asString(m_ipFAMTagManager->ExpandTagsAndFunctions(strPath.c_str(), strFileName.c_str()));

		setMetadataFieldValue(ipConnection, nFileID, strOutputFileMetadataField, strExpandedPath);
	}
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::verifyDestinationActions(ADODB::_ConnectionPtr &ipConnection, std::string &strSelectionFrom)
{
	string strMissingActions = Util::Format(
		"SELECT DISTINCT SA.ASCName as MissingAction \r\n"
		"%s "
		" AND DA.ID IS NULL AND SA.ID IS NOT NULL",
		strSelectionFrom.c_str());

	// Check if there are Actions missing in the dest
	_RecordsetPtr ipMissingActions(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI43438", ipMissingActions != __nullptr);

	ipMissingActions->Open(strMissingActions.c_str(), _variant_t((IDispatch*)ipConnection, true), adOpenStatic,
		adLockReadOnly, adCmdText);

	if (!asCppBool(ipMissingActions->adoEOF))
	{
		UCLIDException ue("ELI43439", "Destination workflow is missing actions in the source workflow.");
		vector<string> vecMissingActions;
		while (!asCppBool(ipMissingActions->adoEOF))
		{
			vecMissingActions.push_back(getStringField(ipMissingActions->Fields, "MissingAction"));
			ipMissingActions->MoveNext();
		}
		ue.addDebugInfo("MissingActions", asString(vecMissingActions, true, ","));
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingDB::createTempTableOfSelectedFiles(ADODB::_ConnectionPtr &ipConnection, std::string &strQueryFrom)
{
	vector<string> vecCreateTemp;
	vecCreateTemp.push_back(gstrDROP_TEMP_FILESELECTION_PROC);

	string strCreateTempProc = gstrCREATE_TEMP_FILESELECTION_PROC;
	replaceVariable(strCreateTempProc, "<SelectionQuery>", strQueryFrom);

	vecCreateTemp.push_back(strCreateTempProc);

	vecCreateTemp.push_back(gstrCREATE_TEMP_SELECTEDFILESTOMOVE);

	executeVectorOfSQL(ipConnection, vecCreateTemp);
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::getWebAppSettings(_ConnectionPtr ipConnection, long nWorkflowId, string strType)
{
	string strQuery = "SELECT [Settings] "
		"	FROM dbo.[WebAppConfig] "
		"	WHERE[Type] =  @Type AND [WorkflowID] = @WorkflowID";

	auto cmd = buildCmd(ipConnection, strQuery, { {"@Type", strType.c_str()}, {"@WorkflowID", nWorkflowId} });

	// Create a pointer to a recordset
	_RecordsetPtr ipWebAppSettings(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI45071", ipWebAppSettings != __nullptr);

	ipWebAppSettings->Open((IDispatch*)cmd, vtMissing, adOpenStatic,
		adLockOptimistic, adCmdText);

	if (ipWebAppSettings->adoEOF == VARIANT_TRUE)
	{
		return "";
	}
	else
	{
		return getStringField(ipWebAppSettings->Fields, "Settings");
	}
}
//--------------------------------------------------------------------------------------------------
string CFileProcessingDB::getWebAppSetting(const string& strSettings, const string& strSettingName)
{
	string searchString = Util::Format("\"%s\":\"", strSettingName.c_str());
	size_t pos = strSettings.find(searchString);
	if (pos != string::npos)
	{
		pos += searchString.length();
		size_t endPos = strSettings.find("\"", pos);
		if (endPos == string::npos)
		{
			throw new UCLIDException("ELI45228", "Failed to parse web app settings.");
		}

		return strSettings.substr(pos, endPos - pos);
	}

	return "";
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingDB::setDefaultSessionMemberValues()
{
	m_nFAMSessionID = 0;
	m_bCurrentSessionIsWebSession = false;
	m_strUPI = UPI::getCurrentProcessUPI().getUPI();
	m_strMachineName = getComputerName();
	m_strFAMUserName = getCurrentUserName();
	m_strFPSFileName = "";
	m_bFAMRegistered = false;
	m_nActiveFAMID = 0;
	m_nActiveActionID = -1;
	m_dwLastPingTime = 0;
}