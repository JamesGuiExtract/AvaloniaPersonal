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
#include <FAMUtilsConstants.h>
#include <UPI.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const size_t gnNUMBER_OF_SLEEP_INTERVALS = 10;

//-------------------------------------------------------------------------------------------------
// FPRecordManager
//-------------------------------------------------------------------------------------------------
FPRecordManager::FPRecordManager()
	: m_hDlg(0)
	, m_nMaxStoredRecords(5)
	, m_bRestrictNumStoredRecords(false)
	, m_strAction("")
	, m_nActionID(0)
	, m_ipFPMDB(__nullptr)
	, m_bKeepProcessingAsAdded(true)
	, m_bProcessSkippedFiles(false)
	, m_bSkippedFilesForCurrentUser(true)
	, m_nNumberOfFilesProcessed(0)
	, m_nNumberOfFilesProcessedSuccessfully(0)
	, m_nNumberOfFilesFailed(0)
	, m_vecSleepTimes(gnNUMBER_OF_SLEEP_INTERVALS)
	, m_bSleepTimeCalculated(false)
	, m_nMaxFilesFromDB(gnMAX_NUMBER_OF_FILES_FROM_DB)
	, m_bRestrictToCurrentFAMSessionID(false)
	, m_eLastFilePriority(kPriorityDefault)
	, m_bAllowRestartableProcessing(false)
	, m_bAllowRestartableFlagRetrievedFromDB(false)
	, m_bUseRandomIDForQueueOrder(false)
	, m_bLimitToUserQueue(false)
	, m_bIncludeFilesQueuedForOthers(true)
{
	try
	{
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

	// Set initial state of the task to pending
	task.m_eStatus = kRecordPending;
	CSingleLock lockGuard(&m_objLock, TRUE);
	changeState(task);
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

	// Set the Work Item processing to the current UPI
	m_bRestrictToCurrentFAMSessionID = true;

	discardWorkItems();

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

	// set event to indicate the queue has been discarded
	m_queueDiscardedEvent.signal();

	// Close the queue
	closeProcessingQueue();

	// To simplify handling delayed files, add them back to the main queue first so we can iterate
	// one queue to reset all queued files.
	requeueDelayedTasks();

	// For all of the remaining files re-mark them as pending in the database
	CSingleLock lockGuard(&m_objLock, TRUE);

	_bstr_t bstrAction = m_strAction.c_str();

	// For all of the remaining files set them back to their fallback status (usually their status
	// before lock) in the database.
	for(TaskIdList::iterator it = m_queTaskIds.begin(); it !=  m_queTaskIds.end(); it++)
	{
		FileProcessingRecord task = getTask(*it);

		// https://extract.atlassian.net/browse/ISSUE-12449
		// Now setting the action status back to the fallback status rather than pending or skipped
		// based upon m_bProcessSkippedFiles. (This may be something other than pending or skipped
		// in the case IFileRequestManager::CheckoutForProcessing was used).
		
		// Set vbAllowQueuedStatusOverride to true.
		UCLID_FILEPROCESSINGLib::EActionStatus eaOldStatus =
			(UCLID_FILEPROCESSINGLib::EActionStatus)kActionUnattempted;
		m_ipFPMDB->SetStatusForFile(task.getFileID(), m_strAction.c_str(), task.getWorkflowID(),
			task.getFallbackStatus(), VARIANT_FALSE, VARIANT_TRUE, &eaOldStatus);
	}

	// Ensure the queue is cleared since we have now reverted the state of all files
	// [FlexIDSCore #3738]
	m_queTaskIds.clear();
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::updateTask(FileProcessingRecord& task)
{
	CSingleLock lockGuard(&m_objLock, TRUE);

	// If the file was specified to be removed while actively processing, remove this file from the
	// queue as it comes out of processing. (If file is active, it is going into, not coming out of
	// the processing state).
	if (!task.isActive() && m_setRemovedFiles.find(task.getFileID()) != m_setRemovedFiles.end())
	{
		task.m_eStatus = kRecordNone;
		m_setRemovedFiles.erase(task.getFileID());
	}

	changeState(task);
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
			changeState(task);
			// There should only be one instance of any filename in the queue
			// if that is not the case we will still only delete one
			return;
		}	
	}

	// If a file was delayed, it may still be awaiting processing, but not yet be back in
	// m_queTaskIds. Check m_queDelayedTasks as well.
	for(it = m_queDelayedTasks.begin(); it !=  m_queDelayedTasks.end(); it++)
	{
		FileProcessingRecord task = getTask(*it);
		if(task.getFileName() == strFileName)
		{
			task.m_eStatus = kRecordNone;
			changeState(task);

			// There should only be one instance of any filename in the queue
			// if that is not the case we will still only delete one
			return;
		}	
	}
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::remove(const long nFileId)
{
	CSingleLock lockGuard(&m_objLock, TRUE);

	TaskIdList::iterator it;
	for(it = m_queTaskIds.begin(); it !=  m_queTaskIds.end(); it++)
	{
		FileProcessingRecord task = getTask(*it);
		if(task.getFileID() == nFileId)
		{
			task.m_eStatus = kRecordNone;
			changeState(task);

			// There should only be one instance of any filename in the queue
			// if that is not the case we will still only delete one
			return true;
		}	
	}

	// If a file was delayed, it may still be awaiting processing, but not yet be back in
	// m_queTaskIds. Check m_queDelayedTasks as well.
	for(it = m_queDelayedTasks.begin(); it !=  m_queDelayedTasks.end(); it++)
	{
		FileProcessingRecord task = getTask(*it);
		if(task.getFileID() == nFileId)
		{
			task.m_eStatus = kRecordNone;
			changeState(task);

			// There should only be one instance of any filename in the queue
			// if that is not the case we will still only delete one
			return true;
		}	
	}

	// If the file to remove is currently processing, flag it to prevent it from going back into
	// the queue via a delay call.
	FileProcessingRecord task;
	if(getTask(nFileId, task) && task.m_eStatus == kRecordCurrent)
	{
		m_setRemovedFiles.insert(nFileId);
		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
long FPRecordManager::peekNext(long nAfterFileId/* = -1*/)
{
	CSingleLock lockGuard(&m_objLock, TRUE);

	bool bSpecifiedFileIsInProcessing = false;
	bool bReturnNext = (nAfterFileId == -1);

	if (!bReturnNext)
	{
		// If nAfterFileId is not currently processing, grab the next file as if nAfterFileId was -1.
		TaskMap::iterator it = m_mapTasks.find(nAfterFileId);
		bSpecifiedFileIsInProcessing = (it != m_mapTasks.end());
		bReturnNext = !bSpecifiedFileIsInProcessing;
	}

	// First search delayed files in reverse as they will fall ahead of in order of logical queue.
	TaskIdList::reverse_iterator rit;
	for (rit = m_queDelayedTasks.rbegin(); rit != m_queDelayedTasks.rend(); rit++)
	{
		if (bReturnNext)
		{
			return *rit;
		}
		else if (*rit == nAfterFileId)
		{
			bReturnNext = true;
		}
	}

	// Search the primary queue of file IDs that have been grabbed for processing.
	TaskIdList::iterator it;
	for (it = m_queTaskIds.begin(); it != m_queTaskIds.end(); it++)
	{
		if (bReturnNext)
		{
			return *it;
		}
		else if (*it == nAfterFileId)
		{
			bReturnNext = true;
		}
	}
	
	// If nAfterFileId is actively processing (was not in either queue), then the next file should
	// simply be the next file in the queue.
	if (bSpecifiedFileIsInProcessing && !bReturnNext)
	{
		return peekNext(-1);
	}

	// No qualifying queued file was found.
	return -1;
}
//-------------------------------------------------------------------------------------------------
int FPRecordManager::requeueDelayedTasks()
{
	int count = 0;
	CSingleLock lockGuard(&m_objLock, TRUE);

	// If file 1 is delayed, then 2 is delayed, 1 should come back up for processing before 2...
	// since we are pushing onto the front of m_queTaskIds rather than the back, m_queDelayedTasks
	// needs to be iterated in reverse order so that they end up in m_queTaskIds in the correct
	// order.
	for(TaskIdList::reverse_iterator it = m_queDelayedTasks.rbegin();
		it !=  m_queDelayedTasks.rend(); it++)
	{
		FileProcessingRecord task = getTask(*it);
		if (task.m_eStatus != kRecordPending)
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI37488");
		}

		m_queTaskIds.push_front(*it);
		count++;
	}

	m_queDelayedTasks.clear();
	return count;
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::pop(FileProcessingRecord& task, bool bWait,
						  Win32Semaphore &processSemaphore,
						  bool* pbProcessingActive/*= __nullptr*/)
{
	if (pbProcessingActive != __nullptr)
	{
		*pbProcessingActive = false;
	}

	unsigned long nSleepTime = 0;

	do
	{
		// if the queue is discarded, then return false immediately
		if (processingQueueIsDiscarded())
		{
			return false;
		}

		// this is only used when deciding if the processing is done
		long nNumTasksLoaded = 0;
		
		// Need to make sure that the  queue gets loaded from the db if it is empty but only from
		// one thread at a time
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

		if (pbProcessingActive != __nullptr)
		{
			*pbProcessingActive = true;
		}

		if (m_queTaskIds.size() <= 0 && !processingQueueIsDiscarded())
		{
			// nNumberToLoad from the database should be decremented by the number of available
			// delayed tasks except if there is only one delayed task. In that case, we need to be
			// sure at least one file is grabbed from the database (if available), so that another
			// file is processed before the delayed file is back up for processing.
			nNumberToLoad = (m_queDelayedTasks.size() == 1)
				? max(nNumberToLoad - m_queDelayedTasks.size(), 1)
				: nNumberToLoad - m_queDelayedTasks.size();
			
			// load from Database;
			nNumTasksLoaded += loadTasksFromDB(nNumberToLoad);

			// If at least 1 file was loaded, reset the sleep time iterator to min sleep time
			if (nNumTasksLoaded > 0)
			{
				m_currentSleepTime = m_vecSleepTimes.begin();
			}
			else
			{
				// No files were loaded from the database. If there are any available delayed files,
				// put them back into the queue for processing.
				nNumTasksLoaded += requeueDelayedTasks();
				m_eLastFilePriority = kPriorityDefault;
			}
		}
		
		// if a file is available in the queue for processing, then update the task variable with
		// its information and return true
		CSingleLock lockGuard(&m_objLock, TRUE);
		if (m_queTaskIds.size() <= 0)
		{
			m_eLastFilePriority = kPriorityDefault;

			// there are no files in the queue for processing.  If the queue has
			// been closed, then just return false, as there is nothing more
			// to pop, if the queue is open and m_pKeepProcessingAsAdded is
			// is false and no files are processing for parallel processing exit and return false
			if (nNumTasksLoaded == 0 && (!processingQueueIsOpen() || !m_bKeepProcessingAsAdded &&
				!areFilesProcessingWithParallelize()))
			{
				// Discard processing queue so that once one thread exits because no files to process
				// all threads will exit even if more files get supplied.
				// FlexIDSCore #3717
				discardProcessingQueue();
				return false;
			}

			// No files are immediately available. If the caller does not want to wait for the next
			// file to be queued, return now.
			if (!bWait)
			{
				return false;
			}
		}
		else
		{
			long nTaskID = *(m_queTaskIds.begin());

			task = getTask(nTaskID);
			task.m_eStatus = kRecordCurrent;
			m_eLastFilePriority = (EFilePriority) task.getPriority();

			// Check for work items to process with higher priority
			if (workItemsToProcess())
			{
				// the work item priority is higher
				continue;
			}

			// Only increment the number of files processed if restricting the number
			// of files to process and a FileProcessingRecord is being returned
			// [LRCAU #5573]
			if (m_nNumberOfFilesToProcess > 0)
			{
				m_nNumberOfFilesProcessed++;
			}

			// Unlock before waiting so that other threads can update
			lockGuard.Unlock();

			// Acquire the semaphore before calling
			Win32SemaphoreLockGuard lg(processSemaphore, true);

			// Relock
			lockGuard.Lock();

			// Quit if the FAM has been stopped while waiting on the semaphore
			if (processingQueueIsDiscarded())
			{
				continue;
			}

			changeState(task);

			// After grabbing a file, put any delayed files back onto the front of the queue so
			// that the effect is that a single file was allowed to skip ahead of the delayed file.
			requeueDelayedTasks();

			// return with the semaphore still acquired
			lg.NoRelease();
			return true;
		}

		// Get the current sleep time (and increment the iterator if necessary)
		nSleepTime = *m_currentSleepTime;
		if (m_currentSleepTime != m_maxSleepTime)
		{
			m_currentSleepTime++;
		}

		// Unlock the lock guards before sleeping
		lockGuard.Unlock();
		lockDBLoad.Unlock();
	}
	while (m_queueStopEvent.wait(nSleepTime) == WAIT_TIMEOUT);
	// keep waiting until either the queue is closed, or a file is available in the
	// queue for processing

	return false;
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::checkoutForProcessing(long &rnFileId, bool bAllowQueuedStatusOverride,
											EActionStatus *peaPreviousStatus)
{
	ASSERT_ARGUMENT("ELI37489", peaPreviousStatus != __nullptr);

	return loadTaskFromDB(rnFileId, bAllowQueuedStatusOverride, peaPreviousStatus);
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::moveToFrontOfQueue(long nFileId)
{
	CSingleLock lockGuard(&m_objLock, TRUE);

	for (TaskIdList::iterator it = m_queTaskIds.begin(); it != m_queTaskIds.end(); it++)
	{
		if(*it == nFileId)
		{
			m_queTaskIds.remove(nFileId);
			m_queTaskIds.push_front(nFileId);
			return true;
		}
	}

	// If a file was delayed, it may still be awaiting processing, but not yet be back in
	// m_queTaskIds. Check m_queDelayedTasks as well.
	for (TaskIdList::iterator it = m_queDelayedTasks.begin(); it != m_queDelayedTasks.end(); it++)
	{
		if(*it == nFileId)
		{
			m_queDelayedTasks.remove(nFileId);
			m_queTaskIds.push_front(nFileId);
			return true;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::delay(FileProcessingRecord& task)
{
	// SendStatusMessage below needs to happen outside of m_objLock and m_readTaskMapMutex,
	// otherwise it can deadlock in situations that the UI thread also happens to be waiting on one
	// of the locks. Therefore, create a separate scope for the locks.
	{
		CSingleLock lockGuard(&m_objLock, TRUE);
		CSingleLock lockGuard2(&m_readTaskMapMutex, TRUE);

		long nFileId = task.getFileID();

		// Only a currently processing task can be delayed.
		FileProcessingRecord oldTask;
		if(!getTask(nFileId, oldTask) ||
			oldTask.m_eStatus != kRecordCurrent)
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI37490");
		}

		task.m_eStatus = kRecordPending;
		m_mapTasks[nFileId] = task;

		m_queDelayedTasks.push_back(nFileId);
	}
	
	// Normally SendStatusMessage is called by changeState. This is a special transition that I
	// don't what to have to further complicated changeState to handle. The only thing that needs
	// to be done outside of updating m_mapTasks is to notify the processing log tab of the
	// transition.
	SendStatusMessage(m_hDlg, &task, kRecordCurrent);
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
	m_queDelayedTasks.clear();
	m_queFinishedTasks.clear();
	m_setRemovedFiles.clear();
	
	clearEvents();
	resetSleepIntervals();

	m_bParallelizableEnabled = false;
	m_bRestrictToCurrentFAMSessionID = false;
	m_bAllowRestartableFlagRetrievedFromDB = false;
	m_bAllowRestartableProcessing = false;

	CSingleLock lg(&m_queWorkItemsMutex, TRUE);
	m_queWorkItemIds.clear();
	
	CSingleLock lgReadWorkItems(&m_mapReadWorkItems, TRUE);
	m_mapWorkItems.clear();
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
FileProcessingRecord FPRecordManager::getTask(long nTaskID)
{
	// Lock the mutex for the Task map while reading so it will not be 
	// updated
	CSingleLock lockGuard(&m_readTaskMapMutex, TRUE);

	TaskMap::iterator it = m_mapTasks.find(nTaskID);
	if(it != m_mapTasks.end())
	{
		return it->second;
	}
	else
	{
		UCLIDException ue("ELI10147", "Invalid Task Id.");
		ue.addDebugInfo("Id", nTaskID);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
IProgressStatusPtr FPRecordManager::getProgressStatus(long nTaskID)
{
	// Lock the mutex for the Task map while reading so it will not be 
	// updated
	CSingleLock lockGuard(&m_readTaskMapMutex, TRUE);

	TaskMap::iterator it = m_mapTasks.find(nTaskID);
	if(it != m_mapTasks.end())
	{
		if (it->second.m_ipProgressStatus == __nullptr)
		{
			return __nullptr;
		}
		else
		{
			return IProgressStatusPtr(it->second.m_ipProgressStatus);
		}
	}
	else
	{
		UCLIDException ue("ELI34360", "Invalid Task Id.");
		ue.addDebugInfo("Id", nTaskID);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
long FPRecordManager::getNumberPending()
{
	CSingleLock lockGuard(&m_objLock, TRUE);
	// TODO: Need to return the number of file from the database also
	return m_queTaskIds.size() + m_queDelayedTasks.size();
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
bool FPRecordManager::getWorkItemToProcess(FPWorkItem& workItem, Win32Event &stopWorkItemThreadEvent)
{
	unsigned long nSleepTime = gnDEFAULT_MIN_SLEEP_TIME_BETWEEN_DB_CHECK;

	// Using min sleep time until the sleep times have been calculated
	if (m_bSleepTimeCalculated)
	{
		nSleepTime = *m_currentWorkItemTime;
	}
	
	// Initialize the Allow restartable flag
	loadAllowRestartableFromDB();
	
	long nWorkItemsLoaded = 0;
	do
	{
		CSingleLock lg(&m_queWorkItemsMutex, TRUE);

		// check for available items in the queue
		if (!(m_bAllowRestartableProcessing && processingIsStopped()) && m_queWorkItemIds.size() <= 0)
		{
			// get more items
			nWorkItemsLoaded = loadWorkItemsFromDB(m_nMaxFilesFromDB, 
				(UCLID_FILEPROCESSINGLib::EFilePriority) m_eLastFilePriority);
		}

		// if no work items in the queue calculate the sleep time and continue
		if (m_queWorkItemIds.size() <= 0)
		{
			if (m_bSleepTimeCalculated)
			{
				nSleepTime = *m_currentWorkItemTime;
				if (m_currentWorkItemTime != m_maxSleepTime)
				{
					m_currentWorkItemTime++;
				}
			}
			continue;
		}
	
		// Reset the sleep time
		if (m_bSleepTimeCalculated)
		{
			m_currentWorkItemTime = m_vecSleepTimes.begin();
		}

		// get the next work item on the queue	
		long workItemID = m_queWorkItemIds.front();
		// Only lock the mutex for a short time to get the work item record
		{
			CSingleLock lockMap(&m_mapReadWorkItems, TRUE);
			workItem = m_mapWorkItems[workItemID];
		}

		// Remove from the work item queue because it will be processed
		m_queWorkItemIds.pop_front();

		// Update the status of the workitem
		updateWorkItem(workItem);
		return true;
	}
	while (stopWorkItemThreadEvent.wait(nSleepTime) == WAIT_TIMEOUT);
	
	return false;	
}
//-------------------------------------------------------------------------------------------------
FPWorkItem FPRecordManager::getWorkItem(long workItemID)
{
	CSingleLock lockGuard(&m_mapReadWorkItems, TRUE);

	workItemMap::iterator it = m_mapWorkItems.find(workItemID);
	if(it != m_mapWorkItems.end())
	{
		return it->second;
	}
	else
	{
		UCLIDException ue("ELI37262", "Work item ID not available.");
		ue.addDebugInfo("Id", workItemID);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
IProgressStatusPtr FPRecordManager::getWorkItemProgressStatus(long nWorkItemID)
{
	CSingleLock lg(&m_mapReadWorkItems, TRUE);
	return m_mapWorkItems[nWorkItemID].m_ipProgressStatus;
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::enableParallelizable(bool bEnable)
{
	m_bParallelizableEnabled = bEnable;
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::setRestrictToFAMSessionID(bool bRestrictToFAMSessionID)
{
	m_bRestrictToCurrentFAMSessionID = bRestrictToFAMSessionID;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void FPRecordManager::SendStatusMessage(HWND hWnd, const FileProcessingRecord *pTask,
	ERecordStatus eOldStatus)
{
	::SendMessage(hWnd, FP_STATUS_CHANGE, eOldStatus, (LPARAM)pTask);
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::changeState(const FileProcessingRecord& task)
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
		long nWorkflowID = task.getWorkflowID();
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
		// https://extract.atlassian.net/browse/ISSUE-12449
		// Status change may be kRecordPending -> kRecordNone in the case that remove(id) was called
		// by CFileProcessingMgmtRole::RemoveFile
		if (eOldStatus == kRecordCurrent ||
			(eOldStatus == kRecordPending && eNewStatus == kRecordNone))
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
					m_ipFPMDB->NotifyFileProcessed(nTaskID, m_strAction.c_str(), nWorkflowID,
						asVariantBool(task.getAllowedQueuedStatusOverride()));
					m_nNumberOfFilesProcessedSuccessfully++;
				}
				else if ( eNewStatus == kRecordFailed )
				{
					_lastCodePos = "220";
					// Notify the DB that the file failed to process
					m_ipFPMDB->NotifyFileFailed(nTaskID, m_strAction.c_str(), nWorkflowID, 
						task.m_strException.c_str(), asVariantBool(task.getAllowedQueuedStatusOverride()));

					m_nNumberOfFilesFailed++;
				}
				else if ( eNewStatus == kRecordSkipped )
				{
					_lastCodePos = "225";
					m_ipFPMDB->NotifyFileSkipped(nTaskID, m_strAction.c_str(), nWorkflowID,
						asVariantBool(task.getAllowedQueuedStatusOverride()));
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
				// https://extract.atlassian.net/browse/ISSUE-12449
				// Now setting the action status back to whatever it had been (may be other than
				// pending or skipped in the case of IFileRequestManager::CheckoutForProcessing).
				
				// Set vbAllowQueuedStatusOverride to true.
				UCLID_FILEPROCESSINGLib::EActionStatus eaOldStatus =
					(UCLID_FILEPROCESSINGLib::EActionStatus)kActionUnattempted;
				m_ipFPMDB->SetStatusForFile(nTaskID, m_strAction.c_str(), nWorkflowID, task.getFallbackStatus(),
					VARIANT_FALSE, VARIANT_TRUE, &eaOldStatus);
				_lastCodePos = "290";
			}
		}

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
			SendStatusMessage(m_hDlg, &task, eOldStatus);
			_lastCodePos = "360";
		}

		_lastCodePos = "370";

		// remove the file if that is the appropriate transition
		if(eNewStatus == kRecordNone)
		{
			// remove the task from the pending queue first
			TaskIdList::iterator queIt;
			for(queIt = m_queTaskIds.begin(); queIt != m_queTaskIds.end(); queIt++)
			{
				_lastCodePos = "380";
				if(*queIt == nTaskID)
				{	
					_lastCodePos = "390";
					m_queTaskIds.erase(queIt);
					break;
				}
			}

			// If a file was delayed, it may still be awaiting processing, but not yet be back in
			// m_queTaskIds. Check m_queDelayedTasks as well.
			for(queIt = m_queDelayedTasks.begin(); queIt != m_queDelayedTasks.end(); queIt++)
			{
				_lastCodePos = "392";
				if(*queIt == nTaskID)
				{	
					_lastCodePos = "394";
					m_queDelayedTasks.erase(queIt);
					break;
				}
			}	

			_lastCodePos = "400";

			// [FlexIDSCore:5186]
			// It is not an error if nTaskID is not removed from m_mapTasks as a result of this
			// call. There may still be additional instances of this task in the m_queTaskIds or
			// m_queFinishedTasks list for which the map entry is still needed.
			removeTaskIfNotCurrentOrInLists(nTaskID);

			_lastCodePos = "410";
		}

		_lastCodePos = "500";
		// If an old task has to be removed
		if (bRemoveOldTask)
		{
			_lastCodePos = "510";

			// [FlexIDSCore:5186]
			// It is not an error if nTaskID is not removed from m_mapTasks as a result of this
			// call. There may still be additional instances of this task in the m_queTaskIds or
			// m_queFinishedTasks list for which the map entry is still needed.
			removeTaskIfNotCurrentOrInLists(nRemoveTaskId);

			_lastCodePos = "520";
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24633");
}
//-------------------------------------------------------------------------------------------------
long FPRecordManager::loadTasksFromDB(long nNumToLoad)
{
	ASSERT_ARGUMENT("ELI14004", m_ipFPMDB != __nullptr);

	if (!m_bSleepTimeCalculated)
	{
		computeSleepIntervals();
	}

	// Get the list of file records
	string strSkippedUser = 
		m_bSkippedFilesForCurrentUser && m_bProcessSkippedFiles ? getCurrentUserName() : "";

	IIUnknownVectorPtr ipFileList = m_ipFPMDB->GetFilesToProcessAdvanced(
		m_strAction.c_str(), nNumToLoad, asVariantBool(m_bProcessSkippedFiles), strSkippedUser.c_str(),
		asVariantBool(m_bUseRandomIDForQueueOrder), asVariantBool(m_bLimitToUserQueue),
		asVariantBool(m_bIncludeFilesQueuedForOthers));

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
			// There should not be any pending records in QueuedActionStatusChange for this file
			// at this point but in case there are, use vbAllowQueuedStatusOverride as VARIANT_FALSE
			// to ignore them.
			m_ipFPMDB->SetFileStatusToPending(fpTask.getFileID(), m_strAction.c_str(), VARIANT_FALSE);
		}
		else
		{
			nNumFilesAddedToQ++;
		}
	}

	return nNumFilesAddedToQ;
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::loadTaskFromDB(long &rnFileId, bool bAllowQueuedStatusOverride,
									 EActionStatus *peaPreviousStatus)
{
	UCLID_FILEPROCESSINGLib::IFileRecordPtr ipRecord(nullptr);

	if (rnFileId == gnGET_NEXT_QUEUED_FILE)
	{
		// Get the list of file records
		string strSkippedUser = 
			m_bSkippedFilesForCurrentUser && m_bProcessSkippedFiles ? getCurrentUserName() : "";
		IIUnknownVectorPtr ipFileList = m_ipFPMDB->GetFilesToProcess(m_strAction.c_str(), 1,
			asVariantBool(m_bProcessSkippedFiles), strSkippedUser.c_str());

		if (ipFileList->Size() == 1)
		{
			ipRecord = ipFileList->At(0);
			ASSERT_RESOURCE_ALLOCATION("ELI40154", ipRecord != __nullptr );

			rnFileId = ipRecord->FileID;
		}
	}
	else
	{
		ipRecord = m_ipFPMDB->GetFileToProcess(rnFileId, m_strAction.c_str(), "");
		ASSERT_RESOURCE_ALLOCATION("ELI40155", ipRecord != __nullptr );
	}

	if (ipRecord != __nullptr)
	{
		// Create task with File ID and name
		FileProcessingRecord fpTask(ipRecord, "", bAllowQueuedStatusOverride);

		// Initially, FallbackStatus will be the same as the status before lock.
		*peaPreviousStatus = (EActionStatus)ipRecord->FallbackStatus;

		return push(fpTask);
	}
	else
	{
		*peaPreviousStatus = kActionUnattempted;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::setFPMDB(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFPMDB)
{
	ASSERT_ARGUMENT("ELI14005", ipFPMDB != __nullptr);

	// Lock the db load mutex since we are modifying the DB pointer
	CSingleLock lg(&m_LoadDBLock, TRUE);

	// Set the database object
	m_ipFPMDB = ipFPMDB;

	// New DB, reset the sleep interval settings
	resetSleepIntervals();
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
		// if the action ID is not found in the database)
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
bool FPRecordManager::removeTaskIfNotCurrentOrInLists(long nTaskID)
{
	// [FlexIDSCore:5186]
	// Before removing task make sure it is not in the pending, delayed or finished task list.
	// Even after removing one instance, these lists can conceivably have more than 
	if (find(m_queTaskIds.begin(), m_queTaskIds.end(), nTaskID) != m_queTaskIds.end() ||
		find(m_queDelayedTasks.begin(), m_queDelayedTasks.end(), nTaskID) != m_queDelayedTasks.end() ||
		find(m_queFinishedTasks.begin(), m_queFinishedTasks.end(), nTaskID) != m_queFinishedTasks.end())
	{
		// If the taskID to be removed is in the pending list or finished list; don't remove it.
		return false;
	}

	// Lock the task map mutex while updating task map
	CSingleLock lockGuard(&m_readTaskMapMutex, TRUE);

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
		// No need for an app trace here; it is perfectly acceptable for a file to have been
		// returned to the pending or processing state.
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
void FPRecordManager::computeSleepIntervals()
{
	try
	{
		auto nMinSleepTime = asUnsignedLong(asString(m_ipFPMDB->GetDBInfoSetting(
			gstrMIN_SLEEP_BETWEEN_DB_CHECKS.c_str(), VARIANT_TRUE)));
		auto nMaxSleepTime = asUnsignedLong(asString(m_ipFPMDB->GetDBInfoSetting(
			gstrMAX_SLEEP_BETWEEN_DB_CHECKS.c_str(), VARIANT_TRUE)));

		if (nMinSleepTime < gnMIN_ALLOWED_SLEEP_TIME_BETWEEN_DB_CHECK
			|| nMinSleepTime > gnMAX_ALLOWED_SLEEP_TIME_BETWEEN_DB_CHECK - 100)
		{
			auto nClosest = nMinSleepTime < gnMIN_ALLOWED_SLEEP_TIME_BETWEEN_DB_CHECK
				? gnMIN_ALLOWED_SLEEP_TIME_BETWEEN_DB_CHECK : gnMAX_ALLOWED_SLEEP_TIME_BETWEEN_DB_CHECK - 100;

			UCLIDException ue("ELI32602",
				"Invalid minimum sleep time specified in database. Using closest allowed minimum.");
			ue.addDebugInfo("Min Time From DB", nMinSleepTime);
			ue.addDebugInfo("Closest Min Allowed", nClosest);
			ue.log();
			nMinSleepTime = nClosest;
		}

		if (nMaxSleepTime > gnMAX_ALLOWED_SLEEP_TIME_BETWEEN_DB_CHECK
			|| nMaxSleepTime < gnMIN_ALLOWED_SLEEP_TIME_BETWEEN_DB_CHECK + 100)
		{
			auto nClosest = nMaxSleepTime > gnMAX_ALLOWED_SLEEP_TIME_BETWEEN_DB_CHECK
				? gnMAX_ALLOWED_SLEEP_TIME_BETWEEN_DB_CHECK : gnMIN_ALLOWED_SLEEP_TIME_BETWEEN_DB_CHECK + 100;

			UCLIDException ue("ELI32603",
				"Invalid maximum sleep time specified in database. Using closest allowed.");
			ue.addDebugInfo("Max Time From DB", nMaxSleepTime);
			ue.addDebugInfo("Closest Max Allowed", nClosest);
			ue.log();
			nMaxSleepTime = nClosest;
		}

		if (nMinSleepTime > nMaxSleepTime || nMinSleepTime < 100)
		{
			UCLIDException ue("ELI32570",
				"Invalid min and max sleep times specified in database. Using default values.");
			ue.addDebugInfo("Min Time", nMinSleepTime);
			ue.addDebugInfo("Max Time", nMaxSleepTime);
			ue.log();

			// Set the min and max to the defaults
			nMinSleepTime = (unsigned long) gnDEFAULT_MIN_SLEEP_TIME_BETWEEN_DB_CHECK;
			nMaxSleepTime = (unsigned long) gnDEFAULT_MAX_SLEEP_TIME_BETWEEN_DB_CHECK;
		}

		// Compute the amount of sleep time to increase in each interval
		size_t nNumIntervals = m_vecSleepTimes.size()-1;
		size_t nIntervalLength = (size_t)(
			(((nMaxSleepTime - nMinSleepTime)*1.0) / (nNumIntervals*1.0)) + 0.5);
		size_t nLastEntry = nNumIntervals;

		// Set the last sleep time to the max sleep time
		m_vecSleepTimes[nLastEntry] = nMaxSleepTime;

		// Set the other sleep time values
		size_t nTempTime = nMinSleepTime;
		for (size_t i = 0; i < nLastEntry; i++)
		{
			m_vecSleepTimes[i] = nTempTime;
			nTempTime += nIntervalLength;
		}

		// Set the current iterator to the min sleep time and the max iterator to the last entry
		m_currentSleepTime = m_vecSleepTimes.begin();
		m_currentWorkItemTime = m_currentSleepTime;
		m_maxSleepTime = m_vecSleepTimes.end() - 1;
		m_bSleepTimeCalculated = true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32571");
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::resetSleepIntervals()
{
	m_currentSleepTime = m_vecSleepTimes.end();
	m_maxSleepTime = m_currentSleepTime;
	m_bSleepTimeCalculated = false;
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::getWorkItem(long nWorkItemID, FPWorkItem& workItem)
{
	// Lock the mutex for the Task map while reading so it will not be 
	// updated
	CSingleLock lg(&m_mapReadWorkItems, TRUE);

	workItemMap::iterator it = m_mapWorkItems.find(nWorkItemID);
	if(it != m_mapWorkItems.end())
	{
		workItem = it->second;
		return true;
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::changeState(const FPWorkItem& workItem)
{
	// there are only two state changes I am wanting
	// kWorkUnitPending to kworkUnitProcessing
	// KWorkUnitProcessing to either kWorkUnitCompleted or kWorkUnitFailed
	FPWorkItem oldWorkItem;
	EWorkItemStatus eOldStatus = kWorkUnitPending;
	EWorkItemStatus newStatus = workItem.m_status;
	long nWorkItemID = workItem.getWorkItemID();

	// need to move the notify calls here also
	if (getWorkItem(nWorkItemID, oldWorkItem))
	{
		eOldStatus = oldWorkItem.m_status;
	}
	else
	{
		CSingleLock lg(&m_mapReadWorkItems, TRUE);
		m_mapWorkItems[nWorkItemID] = workItem;
	}
	SendStatusMessage(m_hDlg, &workItem, eOldStatus);

	CSingleLock lg(&m_mapReadWorkItems, TRUE);
	if (newStatus == kWorkUnitComplete || newStatus == kWorkUnitFailed )
	{
		workItemMap::iterator it = m_mapWorkItems.find(workItem.getWorkItemID());
		if(it != m_mapWorkItems.end())
		{
			m_mapWorkItems.erase(it);
		}
	}
	else
	{
		m_mapWorkItems[nWorkItemID] = workItem;
	}
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::SendStatusMessage(HWND hWnd, const FPWorkItem *pWorkItem,
	EWorkItemStatus eOldStatus)
{
	::SendMessage(hWnd, FP_WORK_ITEM_STATUS_CHANGE, eOldStatus, (LPARAM)pWorkItem);
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::updateWorkItem(FPWorkItem &workItem)
{
	CSingleLock lockMap(&m_queWorkItemsMutex, TRUE);
	changeState(workItem);
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::discardWorkItems()
{
	CSingleLock lg(&m_queWorkItemsMutex, TRUE);

	loadAllowRestartableFromDB();

	// If Restartable processing is not enabled then work items in the queue for file that are 
	// processing in the current FAM session should be kept in the queue to be processed. 
	// If Restartable processing is enabled all of the work items in the queue waiting to be processed
	// will be cleared if processing is stopped (discardWorkItems will be called if m_bKeepProcessingAsAdded is false
	// and the last file has been queued but processingIsStopped() will be false in this case 
	// no new files will be added but the work items for the currently processing files in this 
	// instance need to be processed)
	bool bKeepCurrentFAMSession = !m_bAllowRestartableProcessing || !processingIsStopped();

	// go through the list of waiting items
	long nNumberInQueue = m_queWorkItemIds.size();
	
	for (int i = 0; i < nNumberInQueue; i ++)
	{
		// Get the first Work item on the queue
		long workItemID = m_queWorkItemIds.front();

		// Take the first work item off the queue
		m_queWorkItemIds.pop_front();

		CSingleLock lgMap(&m_mapReadWorkItems, TRUE);
		FPWorkItem fpRecord = m_mapWorkItems[workItemID];
		
		UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr ipWorkItemRecord = fpRecord.getWorkItemRecord();
		if (bKeepCurrentFAMSession && 
			ipWorkItemRecord->WorkGroupFAMSessionID == m_ipFPMDB->FAMSessionID)
		{
			// push the item on the back of the list because it should be processed
			m_queWorkItemIds.push_back(workItemID);
		}
		else
		{
			try
			{
				// Set the work item back to pending
				m_ipFPMDB->SetWorkItemToPending(workItemID);
			
				// Find the work item in the work item map
				workItemMap::iterator it = m_mapWorkItems.find(workItemID);
				if(it != m_mapWorkItems.end())
				{
					// Remove the work item from the map
					m_mapWorkItems.erase(it);
				}
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS ("ELI37614");
		}
	}
}
//-------------------------------------------------------------------------------------------------
long FPRecordManager::loadWorkItemsFromDB(long nNumToLoad, UCLID_FILEPROCESSINGLib::EFilePriority eMinPriority)
{
	long numberWorkItemsLoaded;

	CSingleLock lg(&m_queWorkItemsMutex, TRUE);

	IIUnknownVectorPtr ipWorkItems = 
		m_ipFPMDB->GetWorkItemsToProcess(get_bstr_t(m_strAction), asVariantBool(m_bRestrictToCurrentFAMSessionID), 
			nNumToLoad, eMinPriority);

	numberWorkItemsLoaded = ipWorkItems->Size();

	// Move the work items to the map and the queue
	for (long i = 0; i < numberWorkItemsLoaded; i++)
	{
		UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr ipWorkItem = ipWorkItems->At(i);

		FPWorkItem newWorkItem(ipWorkItem);
	
		m_queWorkItemIds.push_back(newWorkItem.getWorkItemID());
		changeState(newWorkItem);
	}
	return numberWorkItemsLoaded;
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::workItemsToProcess()
{
	// if Parallelization is enabled check for work items to process with higher priority
	if (m_bParallelizableEnabled)
	{
		// Check the priority of the task that was just retrieved to the priority of the 
		// next work item priority
		CSingleLock lg(&m_queWorkItemsMutex, TRUE);
		long nWorkItemsAvailable = m_queWorkItemIds.size();

        if (!(m_bAllowRestartableProcessing && processingIsStopped()) && nWorkItemsAvailable <= 0)
		{
			nWorkItemsAvailable = loadWorkItemsFromDB(m_nMaxFilesFromDB, 
				(UCLID_FILEPROCESSINGLib::EFilePriority) m_eLastFilePriority);
		}
		if (nWorkItemsAvailable > 0)
		{
			return true;
		}
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
void FPRecordManager::loadAllowRestartableFromDB()
{
	// Only want to retrieve this the first time this is called
	if (!m_bAllowRestartableFlagRetrievedFromDB)
	{
		_bstr_t bstrAllowRestartable = m_ipFPMDB->GetDBInfoSetting("AllowRestartableProcessing", VARIANT_FALSE);
		m_bAllowRestartableProcessing = asString(bstrAllowRestartable) == "1";
		m_bAllowRestartableFlagRetrievedFromDB = true;
	}
}
//-------------------------------------------------------------------------------------------------
bool  FPRecordManager::areFilesProcessingWithParallelize()
{
	// Only do the check if Parallelization is enabled
	if (m_bParallelizableEnabled)
	{
		// Check database to see if any files are in the pending or processing state (another process is processing files)
		string strQuery = "SELECT TOP 1 FAMFile.ID FROM dbo.FileActionStatus WITH (NOLOCK) "
							"INNER JOIN dbo.FAMFile WITH (NOLOCK) "
							"  ON FileActionStatus.FileID = FAMFile.ID "
							"WHERE FileActionStatus.ActionStatus = 'R' "
							"OR FileActionStatus.ActionStatus = 'P' "
							"AND FileActionStatus.ActionID = " + asString(m_nActionID);
		
		_RecordsetPtr ipRecordSet = m_ipFPMDB->GetResultsForQuery(strQuery.c_str());

		// If there is at least one record return true
		return ipRecordSet->RecordCount > 0;
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
bool FPRecordManager::areAnyFilesActive()
{
	CSingleLock lockGuard(&m_readTaskMapMutex, TRUE);

	if (!m_queDelayedTasks.empty())
	{
		return true;
	}

	for (TaskMap::iterator it = m_mapTasks.begin(); it != m_mapTasks.end(); it++)
	{
		// Only erase if its status is not pending or current 
		ERecordStatus eStatus = it->second.m_eStatus;
		if (eStatus == kRecordCurrent || eStatus == kRecordPending)
		{
			return true;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------