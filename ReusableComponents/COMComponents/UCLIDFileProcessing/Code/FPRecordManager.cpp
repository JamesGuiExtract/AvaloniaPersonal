#include "stdafx.h"
#include "FPRecordManager.h"
#include "FP_UI_Notifications.h"
#include "CommonConstants.h"

#include <FileProcessingConfigMgr.h>
#include <cpputil.h>
#include <LMData.h>
#include <UCLIDException.h>
#include <COMUtils.h>
#include <RegistryPersistenceMgr.h>
#include <RegConstants.h>

//-------------------------------------------------------------------------------------------------
// FPRecordManager
//-------------------------------------------------------------------------------------------------
FPRecordManager::FPRecordManager()
: m_hDlg(0), 
  m_nMaxStoredRecords(5),
  m_bRestrictNumStoredRecords(false),
  m_strAction(""),
  m_nActionID(0),
  m_ipFPMDB(NULL),
  m_bKeepProcessingAsAdded(true),
  m_bProcessSkippedFiles(false),
  m_bSkippedFilesForCurrentUser(true),
  m_nNumberOfFilesProcessed(0),
  m_nNumberOfFilesProcessedSuccessfully(0),
  m_nNumberOfFilesFailed(0),
  m_nMaxFilesFromDB(gnMAX_NUMBER_OF_FILES_FROM_DB)
{
	try
	{
		// Get the max number of files to load in the record manager from the database
		FileProcessingConfigMgr fpCfgMgr;
		m_nMillisecondsBetweenDBCheck = fpCfgMgr.getMillisecondsBetweenDBCheck();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13993");
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::setDlg(HWND hDlg)
{
	CSingleLock lockGuard(&m_objLock, TRUE);

	m_hDlg = hDlg;
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::push(FileProcessingRecord& task)
{
	// Don't put files on queue if the queue is discarded
	if (processingQueueIsDiscarded())
	{
		return false;
	}

	// Set initial stat of the task to pending
	task.m_eStatus = kRecordPending;
	CSingleLock lockGuard(&m_objLock, TRUE);
	changeState(task, lockGuard);
	return true;
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::processingQueueIsOpen()
{
	// check the status of the queue closed event and return boolean value
	return !m_queueClosedEvent.isSignaled();
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::closeProcessingQueue()
{
	m_queueClosedEvent.signal();
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::stopProcessingQueue()
{
	m_queueStopEvent.signal();
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::processingIsStopped()
{
	// Check the status of the processing is stopped event
	return m_queueStopEvent.isSignaled();
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::processingQueueIsDiscarded()
{
	// check the status of the queue discarded event and return boolean value
	return m_queueDiscardedEvent.isSignaled();
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::discardProcessingQueue()
{
	ASSERT_ARGUMENT("ELI14002", m_ipFPMDB != __nullptr);

	// If the queue has already been discarded just return [FlexIDSCore #3738]
	if (processingQueueIsDiscarded())
	{
		return;
	}

	// lock the DBLoad so that the queue will not be added to while setting to discard and cleaning up
	CSingleLock lockDBLoad(&m_LoadDBLock, TRUE );

	// If the queue has already been discarded just return [FlexIDSCore #3738]
	if (processingQueueIsDiscarded())
	{
		return;
	}

	// set event to indicate the queue has been discared
	m_queueDiscardedEvent.signal();

	// Close the queue
	closeProcessingQueue();

	// For all of the remaining files re-mark them as pending in the database
	CSingleLock lockGuard(&m_objLock, TRUE);

	_bstr_t bstrAction = m_strAction.c_str();

	// For all of the remaining files remark them as pending in the database
	for each ( long l in m_queTaskIds)
	{
		if (m_bProcessSkippedFiles)
		{
			// Return the state of the file to skipped in the database
			m_ipFPMDB->SetFileStatusToSkipped( l, bstrAction, VARIANT_FALSE);
		}
		else
		{
			// Return the state of the file to pending in the database
			m_ipFPMDB->SetFileStatusToPending( l, bstrAction);
		}
	}

	// Ensure the queue is cleared since we have now reverted the state of all files
	// [FlexIDSCore #3738]
	m_queTaskIds.clear();
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::updateTask(FileProcessingRecord& task)
{
	CSingleLock lockGuard(&m_objLock, TRUE);
	changeState(task, lockGuard);
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::remove(const std::string& strFileName)
{
	CSingleLock lockGuard(&m_objLock, TRUE);

	TaskIdList::iterator it;
	for(it = m_queTaskIds.begin(); it !=  m_queTaskIds.end(); it++)
	{
		FileProcessingRecord task = getTask(*it);
		if(task.getFileName() == strFileName)
		{
			task.m_eStatus = kRecordNone;
			changeState(task, lockGuard);
			// There should only be one instance of any filename in the queue
			// if that is not the case we will still only delete one
			break;
		}	
	}
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::pop(FileProcessingRecord& task)
{
	// keep waiting until either the queue is closed, or a file is available in the
	// queue for processing
	while (!processingIsStopped())
	{
		// if the queue is discarded, then return false immediately
		if (processingQueueIsDiscarded())
		{
			return false;
		}

		// this is only used when deciding if the processing is done
		long nNumFromDB = 0;
		
		// Need to make sure that the the queue gets loaded from the db if it is empty but only from one thread at a time
		CSingleLock lockDBLoad(&m_LoadDBLock, TRUE );

		// if the queue is discarded, then return false immediately
		if (processingQueueIsDiscarded())
		{
			return false;
		}

		// Determine the number of files to get from the database
		long nNumberToLoad = m_nMaxFilesFromDB;

		// If only processing a set number of files
		if 	(m_nNumberOfFilesToProcess > 0 )
		{
			// Have we processed the required number of files
			if ( m_nNumberOfFilesProcessed >= m_nNumberOfFilesToProcess)
			{
				// processed the fixed number of files so discard the Processing queue which
				// will stop any further processing
				discardProcessingQueue();

				// Return false to indicate no more files available to process
				return false;
			}

			// The number of files to get from the database will be the lesser of the 
			// normal number of files to get from the database and the total number of
			// files left to process in this run.
			nNumberToLoad = min(m_nMaxFilesFromDB, 
				m_nNumberOfFilesToProcess - m_nNumberOfFilesProcessed);
		}

		// Check if processing has stopped before loading new tasks or grabbing a new file
		// from the queue. [LRCAU #5610]
		if (processingIsStopped())
		{
			return false;
		}

		if (m_queTaskIds.size() <= 0 && !processingQueueIsDiscarded())
		{
			// load from Database;
			nNumFromDB = loadTasksFromDB(nNumberToLoad);
		}
		
		// if a file is available in the queue for processing, then update the task variable with
		// its information and return true
		CSingleLock lockGuard(&m_objLock, TRUE);
		if (m_queTaskIds.size() <= 0)
		{
			// there are no files in the queue for processing.  If the queue has
			// been closed, then just return false, as there is nothing more
			// to pop, if the queue is open and m_pKeepProcessingAsAdded is
			// is false exit and return false
			if (nNumFromDB == 0 && (!processingQueueIsOpen() || !m_bKeepProcessingAsAdded))
			{
				// Discard processing queue so that once one thread exits because no files to process
				// all threads will exit even if more files get supplied.
				// FlexIDSCore #3717
				discardProcessingQueue();
				return false;
			}
		}
		else
		{
			// Only increment the number of files processed if restricting the number
			// of files to process and a FileProcessingRecord is being returned
			// [LRCAU #5573]
			if (m_nNumberOfFilesToProcess > 0)
			{
				m_nNumberOfFilesProcessed++;
			}

			long nTaskID = *(m_queTaskIds.begin());

			task = getTask(nTaskID);
			task.m_eStatus = kRecordCurrent;
			changeState(task, lockGuard);
			return true;
		}

		// Unlock the lock guards before sleeping
		lockGuard.Unlock();
		lockDBLoad.Unlock();

		// if a file is not available in the queue for processing,
		// sleep for the specified amount of time.
		Sleep(m_nMillisecondsBetweenDBCheck);
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::clear(bool bClearUI)
{
	if(m_hDlg != __nullptr && bClearUI)
	{
		::SendMessage(m_hDlg, FP_CLEAR_UI, 0, 0);
	}

	CSingleLock lockGuard(&m_objLock, TRUE);

	// Lock Mutex for the m_mapTasks
	{
		CSingleLock lockGuard(&m_readTaskMapMutex, TRUE);
		m_mapTasks.clear();
	}

	m_queTaskIds.clear();
	m_queFinishedTasks.clear();
	
	clearEvents();
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::clearEvents()
{
	CSingleLock lockGuard(&m_objLock, TRUE);

	m_queueClosedEvent.reset();
	m_queueDiscardedEvent.reset();
	m_queueStopEvent.reset();
}
//-------------------------------------------------------------------------------------------------
const FileProcessingRecord& FPRecordManager::getTask(long nTaskID)
{
	// Lock the mutex for the Task map while reading so it will not be 
	// updated
	CSingleLock lockGuard(&m_readTaskMapMutex, TRUE);

	TaskMap::iterator it = m_mapTasks.find(nTaskID);
	if(it != m_mapTasks.end())
	{
		return m_mapTasks[nTaskID];
	}
	else
	{
		UCLIDException ue("ELI10147", "Invalid Task Id.");
		ue.addDebugInfo("Id", nTaskID);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
long FPRecordManager::getNumberPending()
{
	CSingleLock lockGuard(&m_objLock, TRUE);
	// TODO: Need to return the number of file from the database also
	return m_queTaskIds.size();
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::setMaxStoredRecords(long nMaxStoredRecords)
{
	CSingleLock lockGuard(&m_objLock, TRUE);
	m_nMaxStoredRecords = nMaxStoredRecords;
}
//-------------------------------------------------------------------------------------------------
long FPRecordManager::getMaxStoredRecords()
{
	CSingleLock lockGuard(&m_objLock, TRUE);
	return m_nMaxStoredRecords;
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::setRestrictNumStoredRecords(bool bRestrict)
{
	CSingleLock lockGuard(&m_objLock, TRUE);
	m_bRestrictNumStoredRecords = bRestrict;
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::getRestrictNumStoredRecords()
{
	CSingleLock lockGuard(&m_objLock, TRUE);
	return m_bRestrictNumStoredRecords;
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::setKeepProcessingAsAdded(bool bVal)
{
	m_bKeepProcessingAsAdded = bVal;
}
//-------------------------------------------------------------------------------------------------
std::string FPRecordManager::statusAsString(const ERecordStatus& eStatus)
{
	// Convert status to the appropriate string
	switch (eStatus)
	{
	case	kRecordNone:		return "None";
	case	kRecordPending:		return "Pending";
	case	kRecordCurrent:		return "Current";
	case	kRecordComplete:	return "Complete";
	case	kRecordFailed:		return "Failed";
	case	kRecordSkipped:		return "Skipped";
	}

	// Return empty string if not any of the above
	return "Unknown";
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::setProcessSkippedFiles(bool bSkippedFiles)
{
	m_bProcessSkippedFiles = bSkippedFiles;
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::setSkippedForCurrentUser(bool bSkippedForCurrentUser)
{
	m_bSkippedFilesForCurrentUser = bSkippedForCurrentUser;
}
//-------------------------------------------------------------------------------------------------
long FPRecordManager::getNumberOfFilesProcessedSuccessfully()
{
	return m_nNumberOfFilesProcessedSuccessfully;
}
//-------------------------------------------------------------------------------------------------
long FPRecordManager::getNumberOfFilesFailed()
{
	return m_nNumberOfFilesFailed;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void FPRecordManager::SendStatusMessage(HWND hWnd, long m_nTaskId, ERecordStatus eOldStatus, ERecordStatus eNewStatus)
{
	LPARAM lParam = MAKELPARAM((WORD)eOldStatus, (WORD)eNewStatus);
	::SendMessage( hWnd, FP_STATUS_CHANGE, m_nTaskId, lParam );
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::changeState(FileProcessingRecord& task, CSingleLock& rLockGuard)
{
	INIT_EXCEPTION_AND_TRACING("MLI02389");

	try
	{
		ERecordStatus eNewStatus = task.m_eStatus;
		ERecordStatus eOldStatus = kRecordNone;

		_lastCodePos = "10";
		bool bRemoveOldTask = false;
		long nRemoveTaskId = -1;
		FileProcessingRecord oldTask;
		long nTaskID = task.getFileID();
		_lastCodePos = "20";

		// Try to get the existing task
		if(!getTask(nTaskID, oldTask))
		{
			_lastCodePos = "30";
			// The record did not previously exist so its old status was none
			eOldStatus = kRecordNone;

			// Files may only be added to the pending queue
			if(eNewStatus != kRecordPending)
			{
				_lastCodePos = "40";
				UCLIDException ue("ELI10180", "Invalid Record State Transition!");
				ue.addDebugInfo("Old Status", statusAsString(eOldStatus));
				ue.addDebugInfo("New Status", statusAsString(eNewStatus));
				ue.addDebugInfo("Action Name", m_strAction);
				ue.addDebugInfo("File Name", task.getFileName());
				ue.addDebugInfo("File ID", nTaskID);
				throw ue;
			}
		}
		else
		{
			eOldStatus = oldTask.m_eStatus;
		}

		_lastCodePos = "50";
		// Add the record to the pending queue if appropriate
		// Note that this is done before any notification message is sent
		if (eNewStatus == kRecordPending)
		{
			_lastCodePos = "60";
			// if the oldStatus is current we do not want to do anything
			if (eOldStatus == kRecordCurrent)
			{
				// Returning here does not change the tasks state from current 
				// so that when the thread that is processing the files will have
				// the state changed properly to complete or failed
				return;
			}
			_lastCodePos = "70";
			// Status change from any to Pending should always be allowed

			// Add the task to the pending queue if old status was not pending
			if (eOldStatus != kRecordPending)
			{
				_lastCodePos = "80";
				m_queTaskIds.push_back(nTaskID);
				_lastCodePos = "90";
			}
		}
		_lastCodePos = "100";
		// update the record's status to currently processing
		if (eNewStatus == kRecordCurrent)
		{
			_lastCodePos = "110";
			// Transition from Pending or Current  is allowed
			if (eOldStatus != kRecordPending &&
				eOldStatus != kRecordCurrent)
			{
				_lastCodePos = "120";
				UCLIDException ue("ELI10184", "Invalid Record State Transition!");
				ue.addDebugInfo("Old Status", statusAsString(eOldStatus));
				ue.addDebugInfo("New Status", statusAsString(eNewStatus));
				ue.addDebugInfo("Action Name", m_strAction);
				ue.addDebugInfo("File Name", task.getFileName());
				ue.addDebugInfo("File ID", nTaskID);
				throw ue;
			}
			_lastCodePos = "130";
			// Remove from pending queue if old status was pending
			if (eOldStatus == kRecordPending)
			{
				_lastCodePos = "140";
				TaskIdList::iterator queIt;
				for(queIt = m_queTaskIds.begin(); queIt != m_queTaskIds.end(); queIt++)
				{
					_lastCodePos = "150";
					if(*queIt == nTaskID)
					{	
						_lastCodePos = "160";
						m_queTaskIds.erase(queIt);
						break;
					}
				}
			}
		}
		_lastCodePos = "170";
		// this comes after the update because we may be removing the entry we updated e.g. when 0 is 
		// maxNumStoresRecords
		if (eOldStatus == kRecordCurrent)
		{
			_lastCodePos = "180";
			if (eNewStatus == kRecordComplete ||
				eNewStatus == kRecordFailed ||
				eNewStatus == kRecordSkipped || 
				eNewStatus == kRecordProcessingError)
			{
				_lastCodePos = "190";
				m_queFinishedTasks.push_back(nTaskID);
				_lastCodePos = "200";

				ASSERT_ARGUMENT("ELI14003", m_ipFPMDB != __nullptr);
				// Notify the database of the processed state
				if ( eNewStatus == kRecordComplete )
				{
					_lastCodePos = "210";
					// Notify the DB that the file was processed
					m_ipFPMDB->NotifyFileProcessed(nTaskID, m_strAction.c_str());
					m_nNumberOfFilesProcessedSuccessfully++;
				}
				else if ( eNewStatus == kRecordFailed )
				{
					_lastCodePos = "220";
					// Notify the DB that the file failed to process
					m_ipFPMDB->NotifyFileFailed(nTaskID, m_strAction.c_str(), task.m_strException.c_str());

					m_nNumberOfFilesFailed++;
				}
				else if ( eNewStatus == kRecordSkipped )
				{
					_lastCodePos = "225";
					m_ipFPMDB->NotifyFileSkipped(nTaskID, m_nActionID);
					m_nNumberOfFilesProcessedSuccessfully++;
				}

				_lastCodePos = "230";
				if (m_bRestrictNumStoredRecords && m_queFinishedTasks.size() > (unsigned long) m_nMaxStoredRecords)
				{
					_lastCodePos = "240";
					bRemoveOldTask = true;
					nRemoveTaskId = *m_queFinishedTasks.begin();
					_lastCodePos = "250";
					m_queFinishedTasks.pop_front();
					_lastCodePos = "260";
				}
				_lastCodePos = "270";
			}
			else if (eNewStatus == kRecordNone)
			{
				_lastCodePos = "280";
				// If the new status is none and the old status is current, 
				// Reset the file status to skipped or pending depending
				// on whether skipped files are being processed or pending files
				// [LRCAU #5396 - 08/11/2009 - JDS]
				if (m_bProcessSkippedFiles)
				{
					// Set the file back to skipped (do not update the skipped table)
					m_ipFPMDB->SetFileStatusToSkipped(nTaskID, m_strAction.c_str(), VARIANT_FALSE);
					_lastCodePos = "290_A";
				}
				else
				{
					m_ipFPMDB->SetFileStatusToPending(nTaskID, m_strAction.c_str());
					_lastCodePos = "290_B";
				}
			}
		}
		
		_lastCodePos = "300";
		// Send a notification message
		// Notice that this method is not protected by the lockMap semaphore
		// This is because whatever method handles this method may need to call the 
		// get task method which requires that semaphore.  Note also that whatever code handles 
		// this message may not call any methods such as push or pop on the queue
		rLockGuard.Unlock();

		_lastCodePos = "310";
		// Lock the task map mutex while updating task map
		// Updating the task map needs to be done after DB and finished queue have successfully
		// been updated for but before the UI is updated
		{
			CSingleLock lockGuard(&m_readTaskMapMutex, TRUE);
			_lastCodePos = "320";
			// update task Map
			m_mapTasks[nTaskID] = task;
			_lastCodePos = "330";
		}

		_lastCodePos = "340";
		if(m_hDlg != __nullptr)
		{
			_lastCodePos = "350";
			SendStatusMessage(m_hDlg, nTaskID, eOldStatus, eNewStatus);
			_lastCodePos = "360";
		}

		// Again We must protect the queue from simultaneous access by the getTask() method
		// from another thread
		// This code removes a record from the map if necessary.
		// Note that it is called after the message is processed
		{
			rLockGuard.Lock();
			_lastCodePos = "370";

			// remove the file if that is the appropriate transition
			if(eNewStatus == kRecordNone)
			{
				// remove the task from the pending queue first
				TaskIdList::iterator queIt;
				for(queIt = m_queTaskIds.begin(); queIt != m_queTaskIds.end(); queIt++)
				{
					_lastCodePos = "420";
					if(*queIt == nTaskID)
					{	
						_lastCodePos = "490";
						m_queTaskIds.erase(queIt);
					}
				}					
				
				// Lock the task map mutex while updating task map
				{
					_lastCodePos = "380";
					CSingleLock lockGuard(&m_readTaskMapMutex, TRUE);

					_lastCodePos = "390";

					// Remove the task from map
					if (!removeTaskIfNotPendingOrCurrent(nTaskID))
					{
						UCLIDException ue("ELI25346", "Application trace: Unable to remove TaskID from map.");
						ue.addDebugInfo("Old Status", statusAsString(eOldStatus));
						ue.addDebugInfo("New Status", statusAsString(eNewStatus));
						ue.addDebugInfo("Action Name", m_strAction);
						ue.addDebugInfo("File Name", task.getFileName());
						ue.addDebugInfo("File ID", nTaskID);
						ue.log();
					}
					_lastCodePos = "410";
				}
			}

			_lastCodePos = "500";
			// If an old task has to be removed
			if (bRemoveOldTask)
			{
				// Lock the task map mutex while updating task map
				{
					CSingleLock lockGuard(&m_readTaskMapMutex, TRUE);
					_lastCodePos = "510";

					// Remove task from map
					if (!removeTaskIfNotPendingOrCurrent(nRemoveTaskId))
					{
						UCLIDException ue("ELI25345", "Application trace: Unable to remove TaskID from map.");
						ue.addDebugInfo("Task to be removed", nRemoveTaskId);
						ue.addDebugInfo("Old Status", statusAsString(eOldStatus));
						ue.addDebugInfo("New Status", statusAsString(eNewStatus));
						ue.addDebugInfo("Action Name", m_strAction);
						ue.addDebugInfo("File Name", task.getFileName());
						ue.addDebugInfo("File ID", nTaskID);
						ue.log();
					}
					_lastCodePos = "520";
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24633");
}
//-------------------------------------------------------------------------------------------------
long FPRecordManager::loadTasksFromDB(long nNumToLoad)
{
	ASSERT_ARGUMENT("ELI14004", m_ipFPMDB != __nullptr);

	// Get the list of file records
	string strSkippedUser = 
		m_bSkippedFilesForCurrentUser && m_bProcessSkippedFiles ? getCurrentUserName() : "";
	IIUnknownVectorPtr ipFileList = m_ipFPMDB->GetFilesToProcess(m_strAction.c_str(), nNumToLoad,
		asVariantBool(m_bProcessSkippedFiles), strSkippedUser.c_str());

	// Attempt to create a task for each file record and add it to the queue
	long nNumFiles = ipFileList->Size();
	long nNumFilesAddedToQ = 0;
	for ( long n = 0; n < nNumFiles; n++ )
	{
		UCLID_FILEPROCESSINGLib::IFileRecordPtr ipRecord = ipFileList->At(n);
		ASSERT_RESOURCE_ALLOCATION("ELI14205", ipRecord != __nullptr );

		// Create task with File ID and name
		FileProcessingRecord fpTask( ipRecord );
		
		// put the new task in the queue
		// if the queue has been discarded, all remaining files in the list need to have their
		// status reset in the database
		if (push(fpTask) == false)
		{
			m_ipFPMDB->SetFileStatusToPending(fpTask.getFileID(), m_strAction.c_str());
		}
		else
		{
			nNumFilesAddedToQ++;
		}
	}

	return nNumFilesAddedToQ;
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::setFPMDB(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFPMDB)
{
	ASSERT_ARGUMENT("ELI14005", ipFPMDB != __nullptr);
	// Set the database object
	m_ipFPMDB = ipFPMDB;
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::setActionID(long nActionID)
{
	try
	{
		// set the actionID member and just return if 0
		m_nActionID = nActionID;
		if (m_nActionID == 0)
		{
			return;
		}

		// Check for no defined database
		if ( m_ipFPMDB == __nullptr )
		{
			UCLIDException ue("ELI14110", "The database has not been set.");
			throw ue;
		}

		// Get the action name from the database (will throw exception
		// if the action ID is not found in the databse)
		m_strAction = asString(m_ipFPMDB->GetActionName(m_nActionID));
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26723");
}
//-------------------------------------------------------------------------------------------------
long FPRecordManager::getActionID()
{
	// Return the current action ID
	return m_nActionID;
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::getTask(long nTaskID, FileProcessingRecord& task)
{
	// Lock the mutex for the Task map while reading so it will not be 
	// updated
	CSingleLock lockGuard(&m_readTaskMapMutex, TRUE);

	TaskMap::iterator it = m_mapTasks.find(nTaskID);
	if(it != m_mapTasks.end())
	{
		task = it->second;
		return true;
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::setNumberOfFilesToProcess(long nNumberOfFiles)
{
	m_nNumberOfFilesToProcess = nNumberOfFiles;
	m_nNumberOfFilesProcessed = 0;
	m_nNumberOfFilesProcessedSuccessfully = 0;
	m_nNumberOfFilesFailed = 0;
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::removeTaskIfNotPendingOrCurrent(long nTaskID)
{
	// Before removing task make sure it is not in the pending task list
	TaskIdList::iterator itPending;
	for (itPending = m_queTaskIds.begin(); itPending != m_queTaskIds.end(); itPending++)
	{
		// If the taskID to be removed is in the pending list don't remove it
		if ( *itPending == nTaskID )
		{
			return false;
		}
	}

 	// The task is not in the pending queue so it can be removed from the tasks map
	TaskMap::iterator it = m_mapTasks.find(nTaskID);
	if (it != m_mapTasks.end())
	{
		// Only erase if its status is not pending or current 
		ERecordStatus eStatus = it->second.m_eStatus;
		if (eStatus != kRecordCurrent && eStatus != kRecordPending )
		{
			// remove the task from the map
			m_mapTasks.erase(it);
			return true;
		}
		else
		{
			UCLIDException ue("ELI25355", "Application trace: Task cannot be removed.");
			ue.addDebugInfo("Record Status", statusAsString(eStatus));
			ue.addDebugInfo("Task to be removed", nTaskID);
			ue.log();
		}
	}
	else
	{
		UCLIDException ue("ELI25353", "Application trace: TaskID not found in map.");
		ue.addDebugInfo("Task to be removed", nTaskID);
		ue.log();
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
