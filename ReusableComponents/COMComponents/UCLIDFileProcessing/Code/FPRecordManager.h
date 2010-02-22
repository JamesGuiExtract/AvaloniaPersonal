#pragma once 
#include "resource.h"
#include "FileProcessingRecord.h"

#include <afxmt.h>
#include <list>
#include <vector>
#include <map>
#include <Win32Event.h>

//-------------------------------------------------------------------------------------------------
// FPRecordManager
//-------------------------------------------------------------------------------------------------
class FPRecordManager
{
public:
	//---------------------------------------------------------------------------------------------
	// PROMISE: Will create an empty queue with no window to recieve update messages
	FPRecordManager();
	//---------------------------------------------------------------------------------------------
	// PROMISE: Will set a dlg to recieve update messages
	//			hDlg specifies the FileProcessingDlg that will recieve update messages as the 
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
	void remove(const std::string& strFileName);
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
	//			process, then this method will wait until 
	//			the processing queue is closed and no files are returned from the database
	//			(in which case false is returned and the task 
	//			method parameter is untouched) or a file is added to the queue for processing
	//			(in which case that file is assigned to task and true is 
	//			returned) - whichever happens first.
	bool pop(FileProcessingRecord& task);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Will return FileProcessingRecord Associated with nTaskId
	const FileProcessingRecord& getTask(long nTaskID);
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
	static std::string statusAsString(const ERecordStatus& eStatus);

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
	long getNumberOfFilesProcessed();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return the number of files that have failed processing
	long getNumberOfFilesFailed();
	//---------------------------------------------------------------------------------------------

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

	// A handle to the File Processing dialog that will be sent update messages as 
	// the queues status changes
	HWND m_hDlg;

	// the id of the last task to be added to the queue
	long m_ulLastAddedID;

	typedef std::list<long> TaskIdList;
	TaskIdList m_queTaskIds;

	typedef std::map<long, FileProcessingRecord> TaskMap;
	TaskMap m_mapTasks;

	long m_nMaxStoredRecords;
	bool m_bRestrictNumStoredRecords;
	
	TaskIdList m_queFinishedTasks;

	// the number of bytes in the list
	LONGLONG m_nNumBytes;

	// Mutex to lock for updating the queTaskIds, m_queFinishedTask
	CMutex m_objLock;

	// Mutex to lock when reading or updating the m_mapTasks object
	CMutex m_readTaskMapMutex;
	
	// Mutex used to block the check to load the queue from the database
	CMutex m_LoadDBLock;

	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFPMDB;

	// The name of the action being processed
	std::string m_strAction;

	// The id of the action being processed
	long m_nActionID;

	// The max number of files to get from the database each time the queue is empty
	long m_nMaxFilesFromDB;

	// The number of milliseconds to sleep between checking database for new files
	long m_nMillisecondsBetweenDBCheck;

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

	////////////////
	// Methods
	////////////////

	//---------------------------------------------------------------------------------------------
	// REQUIRE: rLockGuard must have already obtained the lock to m_objLock.
	void changeState(FileProcessingRecord& task, CSingleLock& rLockGuard);
	//---------------------------------------------------------------------------------------------
	void SendStatusMessage(HWND hWnd, long m_nTaskId, ERecordStatus eOldStatus, ERecordStatus eNewStatus);
	//---------------------------------------------------------------------------------------------
	// Loads max of nNumToLoad records in the processing queue from the database
	// returns the number of records loaded
	long loadTasksFromDB(long nNumToLoad);

	//---------------------------------------------------------------------------------------------
	// PROMISE: Will try to find the nTaskID in the m_mapTasks map if nTaskID is found the task
	//			record will be returned in task argument and will return true. If nTaskID is not 
	//			found method will return false.
	bool getTask(long nTaskID, FileProcessingRecord& task);

	// PROMISE: To remove the task from the m_mapTasks if the task is not current or pending.
	//			returns true if task was not pending or current and was removed from the queue.
	//			returns false if task is pending or current
	//			if task is not pending or current and not in the task map an exception will be logged and
	//			false will be returned.
	bool removeTaskIfNotPendingOrCurrent(long nTaskID);
};