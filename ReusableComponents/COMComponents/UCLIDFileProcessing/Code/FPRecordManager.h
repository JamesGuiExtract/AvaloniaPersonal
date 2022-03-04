#pragma once 
#include "resource.h"
#include "FileProcessingRecord.h"
#include "FPWorkItem.h"

#include <afxmt.h>
#include <list>
#include <vector>
#include <map>
#include <set>
#include <Win32Event.h>
#include <Win32Semaphore.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// FPRecordManager
//-------------------------------------------------------------------------------------------------
class FPRecordManager
{
public:
	//---------------------------------------------------------------------------------------------
	// PROMISE: Will create an empty queue with no window to receive update messages
	FPRecordManager();
	//---------------------------------------------------------------------------------------------
	// PROMISE: Will set a dlg to receive update messages
	//			hDlg specifies the FileProcessingDlg that will receive update messages as the 
	//			queue is updated
	void setDlg(HWND hDlg);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Will empty all of the tasks currently in the queue, and open the processing queue
	//			If bClearUI is true clear messages will be sent to 
	//			the FileProcessingDlg specified by m_hDlg
	//			Subsequent calls to processingQueueIsOpen() will return true.
	//			Subsequent calls to processingQueueIsDiscarded() will return false.
	void clear(bool bClearUI);
	//---------------------------------------------------------------------------------------------
	// Resets the Win32Events that are indicating the queue is discarded and closed
	// 
	void clearEvents();
	//---------------------------------------------------------------------------------------------
	// REQUIRE: Nothing
	// PROMISE: If processingQueueIsOpen() == true, then the specified task will be added to the 
	//			back of the queue and the appropriate messages will be sent to the dialog, and true
	//			is returned.
	//			If processingQueueIsOpen() == false, no actions are taken, and false is returned.
	bool push(FileProcessingRecord& task);
	//---------------------------------------------------------------------------------------------
	// PROMISE: If a FileProcessingRecord exists with the same id as "task" is will be updated to have
	//			the properties of task
	void updateTask(FileProcessingRecord& task);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Will remove any tasks in the queue with strFileName as their file (there should
	//			be only one).  Appropriate messages will be sent
	void remove(const string& strFileName);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Will remove the specified file ID from the queue. Appropriate messages will be sent.
	bool remove(const long nFileId);
	//---------------------------------------------------------------------------------------------	
	// PROMISE: Returns the ID of the next file queued for processing. If nAfterFileId is not -1,
	//			it will return the ID of any file after nAfterFileId that is queued for processing.
	//			If nAfterFileId is not -1 but the specified file is not processing, this call will
	//			behave as if nAfterFileId is -1.
	//			If no qualifying file is queued for processing, -1 is returned.
	long peekNext(long nAfterFileId = -1);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Puts any task that was delayed back on the front of the queue (to be processed next).
	int requeueDelayedTasks();
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To return true if the processing queue is open and to return false otherwise.
	bool processingQueueIsOpen();
	//---------------------------------------------------------------------------------------------
	// REQUIRE: processingQueueIsOpen() == true
	// PROMISE:	To close the processing queue. Subsequent calls to processingQueueIsOpen() will
	//			return false.
	void closeProcessingQueue();
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To return true if the processing queue is discarded and to return false otherwise.
	bool processingQueueIsDiscarded();
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To discard (i.e. not process) any files in the processing queue.
	//			Subsequent calls to processingQueueIsDiscarded() will return true.
	void discardProcessingQueue();
	//---------------------------------------------------------------------------------------------
	// PROMISE: If processingQueueIsDiscarded() == true, this method will return false 
	//			immediately and the task method parameter is untouched.
	//			If processingQueueIsDiscarded() == false and the queue contains at least one
	//			file to be processed, then the first file in the queue will be popped and 
	//			assigned to task, and true will be returned.
	//			If processingQueueIsDiscarded() == false and the queue contains no files to be
	//			processed, and the processing queue is closed, and the database returns no
	//			records to be processed then false is returned and the
	//			task method parameter is untouched.
	//			If processingQueueIsDiscarded() == false and the queue contains no files to be
	//			processed, and the processing queue is open and the database returns no records to
	//			process, then this method will wait until the processing queue is closed or
	//			a file is added to the queue for processing if bWait is true. Otherwise, false is
	//			returned and the task method parameter is untouched.
	//			In all cases, if pbProcessingActive is not null, when returned the value will
	//			indicate whether the processing queue is still active.
	//			processSemaphore will always be acquired if this returns true and not acquired if
	//			this returns false
	bool pop(FileProcessingRecord& task, bool bWait, Win32Semaphore &processSemaphore, bool* pbProcessingActive = __nullptr);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Requests that the specified file be locked for processing and added to the queue.
	//			If rnFileId == -1 it indicates the next file in the queue should be grabbed and will
	//			updated with the ID of the file that was checked out.
	//			peaPreviousStatus indicates the status of the file before being set to processing.
	// RETURNS: true if the file was able to be checked out or it was already checked out, false
	//			if another process already had the file checked out.
	bool checkoutForProcessing(long &rnFileId, bool bAllowQueuedStatusOverride, EActionStatus *peaPreviousStatus);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Moves the specified file to the front of the queue to ensure it is the next file
	//			that starts processing.
	// RETURNS: true if the file was currently checked out for processing and was able to be moved
	//			to the front. false if the file is either already processing or otherwise not in
	//			the queue.
	bool moveToFrontOfQueue(long nFileId);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Temporarily cancels processing of the specified task. The next file in the queue
	//			(if any) will jump ahead of it. If not released, the delayed file will then be
	//			processed again after the intervening file.
	void delay(FileProcessingRecord& task);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Will return FileProcessingRecord Associated with nTaskId
	FileProcessingRecord getTask(long nTaskID);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Will try to find the nTaskID in the m_mapTasks map if nTaskID is found the task
	//			record will be returned in task argument and will return true. If nTaskID is not 
	//			found method will return false.
	bool getTask(long nTaskID, FileProcessingRecord& task);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Will return an IProgressStatus indicating the progress of the task with the
	// specified id.
	IProgressStatusPtr getProgressStatus(long nTaskID);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Returns the number of files on the pending queue
	long getNumberPending();
	//---------------------------------------------------------------------------------------------
	// PROMISE: Sets the maximum number of records that will be stored in memory
	void setMaxStoredRecords(long nMaxStoredRecords);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Returns the maximum number of records that will be stored in memory
	long getMaxStoredRecords();
	//---------------------------------------------------------------------------------------------
	// PROMISE: Restricts the number of records stored in memory to the number specified by 
	//			setMaxStoredRecords()
	void setRestrictNumStoredRecords(bool bRestrict);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Returns true id the maximum number of records stored in memory is restricted
	bool getRestrictNumStoredRecords();
	//---------------------------------------------------------------------------------------------
	void setFPMDB(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFPMDB);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To set the Action ID for the action to process
	//			this method will get the Action name from the DB
	void setActionID(long nActionID );
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return the ID of the action being processed.
	long getActionID();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To set the KeepProcessingAsAdded flag so when the Record Manager 
	//			will keep trying to get more files from the database indefinitely
	//			if it is set to false when no file obtained from the DB pop
	//			will return false
	void setKeepProcessingAsAdded(bool bVal);
	//---------------------------------------------------------------------------------------------
	// PROMISE: TO return the status as a string
	static string statusAsString(const ERecordStatus& eStatus);

	// Sets the number of files to process and resets the number processed.
	// if nNumberOfFiles == 0 then there is not a restriction on the number
	// of files processed. if > 0 then only nNumberOfFiles will be returned from the
	// pop method
	void setNumberOfFilesToProcess(long nNumberOfFiles);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To set processing to process files in skipped status if bSkippedFiles == true
	void setProcessSkippedFiles(bool bSkippedFiles = true);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To set skipped file processing to only process files skipped by the
	//			current user if bSkippedForCurrentUser == true, and process files skipped
	//			by any user if bSkippedForCurrentUser == false
	void setSkippedForCurrentUser(bool bSkippedForCurrentUser = true);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return the number of files that have been processed successfully (this includes
	// the number of skipped files).
	long getNumberOfFilesProcessedSuccessfully();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return the number of files that have failed processing
	long getNumberOfFilesFailed();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To signal the stop processing event.
	void stopProcessingQueue();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return true if stop processing has been signaled and false otherwise.
	bool processingIsStopped();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To set the max number of files to get from the database at once
	inline void setMaxNumberOfFilesFromDB(long nMaxNumberOfFiles)
		{ m_nMaxFilesFromDB = nMaxNumberOfFiles; }
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return a work item to process if one is available. returns false if stopWorkItemThreadEvent
	//			is signaled and no more work items are available.
	bool getWorkItemToProcess(FPWorkItem& workItem, Win32Event &stopWorkItemThreadEvent);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return a copy of the work item with workitem ID that is in m_mapWorkItems
	FPWorkItem getWorkItem(long workItemID);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To update the work item record in m_mapWorkItems
	void updateWorkItem(FPWorkItem &workItem);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Will return an IProgressStatus indicating the progress of the WorkItem with the
	// specified id.
	IProgressStatusPtr getWorkItemProgressStatus(long nWorkItemID);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Sets the flag to indicate that work units will processed
	void enableParallelizable(bool bEnable);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Set the flag to only retrieve work items for files being processed in the current
	// instance
	void setRestrictToFAMSessionID(bool bRestrictToFAMSessionID);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Indicates whether any files are either processing or are queued to be processed.
	bool areAnyFilesActive();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To set whether to use random queue order
	inline void setUseRandomIDForQueueOrder(bool bUseRandomIDForQueueOrder)
		{ m_bUseRandomIDForQueueOrder = bUseRandomIDForQueueOrder; }
	//---------------------------------------------------------------------------------------------
	// PROMISE: To set whether to process only files for the current user's specific queue
	inline void setLimitToUserQueue(bool bLimitToUserQueue)
		{ m_bLimitToUserQueue = bLimitToUserQueue; }

private:

	////////////////
	// Variables
	////////////////

	// This event controls whether files can be added to the queue for processing.
	// If the queue is closed, then files cannot be added to the queue (they can still be 
	// popped off the queue using the appropriate method calls).  This is like a bank closing
	// their main doors so that they cannot be opened from outside.  Customers who are in the
	// queue inside the bank can still be served.
	Win32Event m_queueClosedEvent;

	// This event controls whether the queue is in a discarded state.  If the queue is in
	// a discarded state, that means we will not process any files pending in the queue.
	// This is like a bank employee telling customers in the queue inside the bank that
	// their computers are down and so no customers can be served for now.
	Win32Event m_queueDiscardedEvent;

	// This event controls whether more files should be added to the queue.  If the
	// queue is in the stop state, that means that we will not let any more files be added
	// to the queue nor will we let any more files be removed from the queue (threads in the
	// pop method will return without getting a file).
	Win32Event m_queueStopEvent;

	// A handle to the File Processing dialog that will be sent update messages as 
	// the queues status changes
	HWND m_hDlg;

	// the id of the last task to be added to the queue
	long m_ulLastAddedID;

	typedef list<long> TaskIdList;
	TaskIdList m_queTaskIds;
	TaskIdList m_queFinishedTasks;
	TaskIdList m_queDelayedTasks;
	set<long> m_setRemovedFiles;

	typedef map<long, FileProcessingRecord> TaskMap;
	TaskMap m_mapTasks;

	long m_nMaxStoredRecords;
	bool m_bRestrictNumStoredRecords;
	
	// the number of bytes in the list
	LONGLONG m_nNumBytes;

	// CriticalSection to lock for updating the m_queTaskIds, m_queDelayedTasks, m_queFinishedTask
	CCriticalSection m_objLock;

	// CriticalSection to lock when reading or updating the m_mapTasks object
	CCriticalSection m_readTaskMapMutex;
	
	// CriticalSection used to block the check to load the queue from the database
	CCriticalSection m_LoadDBLock;

	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFPMDB;

	// The name of the action being processed
	string m_strAction;

	// The id of the action being processed
	long m_nActionID;

	// The max number of files to get from the database each time the queue is empty
	long m_nMaxFilesFromDB;

	vector<unsigned long> m_vecSleepTimes;
	vector<unsigned long>::iterator m_currentSleepTime; // Iterator into the sleep time vector
	vector<unsigned long>::iterator m_currentWorkItemTime; // Iterator into the sleep time vector for workItem time
	vector<unsigned long>::iterator m_maxSleepTime; // Iterator that points to the max sleep time
	bool m_bSleepTimeCalculated;

	// This flag indicates that processing should continue waiting for files to be added to the DB
	bool m_bKeepProcessingAsAdded;

	// Contains the number of files to process for each run in this session. 
	// if == 0 there is no restriction on the number of files to run.
	long m_nNumberOfFilesToProcess;

	// Value contains the number of files processed in the current run if m_nNumberOfFilesToProcess
	// is > 0 otherwise this value is not updated.
	long m_nNumberOfFilesProcessed;

	// Value contains the number of files that processed successfully in the current run
	long m_nNumberOfFilesProcessedSuccessfully;

	// Value contains the number of files that failed to process in the current run
	long m_nNumberOfFilesFailed;

	// Values for determining if processing pending files or skipped files and if processing
	// skipped files whether to process those skipped by all users or just current user
	bool m_bProcessSkippedFiles;
	bool m_bSkippedFilesForCurrentUser;

	// Define the workItemMap type - currently only used once by make have same as other tasks
	typedef map<long, FPWorkItem> workItemMap;

	// Map of work items being processed by this FAM
	workItemMap m_mapWorkItems;

	typedef list<long> WorkItemIdList;

	// List of work items available for processing - the work item records are in the m_mapWorkItems
	WorkItemIdList m_queWorkItemIds;

	// CriticalSections to use when reading or updating the m_mapWorkItems map
	CCriticalSection m_queWorkItemsMutex; // protects m_queWorkItemIds
	CCriticalSection m_mapReadWorkItems; // protects m_mapWorkItems

	// This should be set to the priority of the last file that was marked as current or if there
	// was no file to mark as current this should be set to kPriorityDefault.  This value is used to 
	// establish the minimum priority of the work items to be repopulate the work item queue
	EFilePriority m_eLastFilePriority;
	
	// Flag to indicate that parallelization is enabled
	bool m_bParallelizableEnabled;

	// Flag to indicate that only workitems for files being processed in the current instance 
	// should be retrieved from the database
	bool m_bRestrictToCurrentFAMSessionID;

	// Flag to indicate that restartable processing is enabled. This is only used 
	// if m_bParallelizableEnabled is true
	bool m_bAllowRestartableProcessing;

	// Flag to indicate that the AllowRestartableProcessing has been retrieved from the database
	bool m_bAllowRestartableFlagRetrievedFromDB;

	// Whether to use random queue order
	bool m_bUseRandomIDForQueueOrder;

	// Whether to process only files for the current user's specific queue
	bool m_bLimitToUserQueue;

	////////////////
	// Methods
	////////////////

	//---------------------------------------------------------------------------------------------
	void changeState(const FPWorkItem& workItem);
	//---------------------------------------------------------------------------------------------
	void changeState(const FileProcessingRecord& task);
	//---------------------------------------------------------------------------------------------
	void SendStatusMessage(HWND hWnd, const FileProcessingRecord *pTask, ERecordStatus eOldStatus);
	//---------------------------------------------------------------------------------------------
	void SendStatusMessage(HWND hWnd, const FPWorkItem* pWorkItem, EWorkItemStatus eOldStatus);
	//---------------------------------------------------------------------------------------------
	// Loads max of nNumToLoad records in the processing queue from the database
	// returns the number of records loaded
	long loadTasksFromDB(long nNumToLoad);
	//---------------------------------------------------------------------------------------------
	// Loads specified nFileId from the database or the next file in the queue if rnFileId == -1.
	// rnFileId will be set to the ID of the loaded file if -1 was passed in.
	// Returns true if the file was loaded.
	bool loadTaskFromDB(long &rnFileId, bool bAllowQueuedStatusOverride, EActionStatus *peaPreviousStatus);
	//---------------------------------------------------------------------------------------------
	// Loads max of nNumToLoad workitems from the database to the workItem processing queue
	// returns the number of records loaded
	long loadWorkItemsFromDB(long nNumToLoad, UCLID_FILEPROCESSINGLib::EFilePriority eMinPriority);

	//---------------------------------------------------------------------------------------------
	// PROMISE: Will try to find the nWorkItemID in the m_mapWorkItems map if nWorkItemID is found
	//			the work item record will be returned in workItem argument and will return true. 
	//			If nWorkItemID is not found method will return false. 
	bool getWorkItem(long nWorkItemID, FPWorkItem& workItem);

	// PROMISE: To remove the task from the m_mapTasks if the task is not current or in either the
	//			pending or finished lists.
	//			returns true if task was not current or in either list and was removed from the
	//			queue.
	//			returns false if task is current or in either list.
	//			if task is not pending or current and not in the task map an exception will be
	//			logged and false will be returned.
	bool removeTaskIfNotCurrentOrInLists(long nTaskID);

	//---------------------------------------------------------------------------------------------
	// PROMISE: To get the default sleep time and max sleep time from the DB config and compute
	//			the different sleep time intervals based on the range
	void computeSleepIntervals();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To reset the sleep interval values so that they will be recalculated the next
	//			time they are needed.
	void resetSleepIntervals();

	// if restartable processing is enabled all work items that have been previously retrieved 
	// from the database and are not processing will be "returned" to the database (set to pending in the database)
	// otherwise keep the work items for the files that are being processed by the current instance.
	void discardWorkItems();

	// returns true if there are workItems to process and false if there are not work items to process
	bool workItemsToProcess();

	// Retrieves the AllowRestartable setting from the database
	void loadAllowRestartableFromDB();

	// Checks the database for at least one file that is either Pending or Processing for the current action
	bool areFilesProcessingWithParallelize();
};