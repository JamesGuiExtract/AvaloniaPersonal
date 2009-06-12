
#pragma once

#include <string>
#include <StopWatch.h>
//-------------------------------------------------------------------------------------------------
enum ERecordStatus
{
	kRecordNone,
	kRecordPending,
	kRecordCurrent,
	kRecordComplete,
	kRecordFailed,
	kRecordSkipped,
	kRecordProcessingError
};

//-------------------------------------------------------------------------------------------------
class FileProcessingRecord
{
public:
	//---------------------------------------------------------------------------------------------
	// default ctor
	FileProcessingRecord();
	//---------------------------------------------------------------------------------------------
	// ctor to initialize new task with the status of eTaskInProgress
	// This ctor will automatically populate m_strMachineName, m_eStatus
	// and m_strStartTime
	FileProcessingRecord(UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRcd,
		const std::string& strMachine = "");
	//---------------------------------------------------------------------------------------------
	// copy ctor and assignment operator
	FileProcessingRecord(const FileProcessingRecord& task);
	FileProcessingRecord& operator=(const FileProcessingRecord& task);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To reset all member variables
	void reset();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To make this object a copy of the object 'task'
	void copyFrom(const FileProcessingRecord& task);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To change status to current, start the stop watch, and perform other 
	//			initialization.
	// NOTES:	This method will initialize the top-level progress status object if and only if
	//			displaying of progress status information is enabled at the time of this call.
	void markAsStarted();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To change status to complete, and compute task duration.
	void markAsCompleted();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To change status to pending.
	void markAsPending();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To change status to failed, compute task duration,
	// and to record the specified exception
	void markAsFailed(const std::string& strException);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To change status to none.
	void markAsNone();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To change status to processing error.
	void markAsProcessingError(const std::string& strException);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To start stopwatches while error task executes (does not change file status)
	void notifyRunningErrorTask();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To stop stopwatches (does not change file status)
	void notifyErrorTaskCompleted();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To stop stopwatches and add a new exception entry concerning the failure
	// executing the error task. (does not change file status)
	void notifyErrorTaskFailed(const std::string& strException);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return the number of seconds elapsed during the execution
	// of this task (regardless of whether the task completed successfully or not)
	double getTaskDuration() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the starting time of the stopwatch
	CTime getStartTime() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return the number of seconds elapsed during the execution
	// of the error task (regardless of whether the error task completed successfully or not)
	double getErrorTaskDuration() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the starting time of the error task stopwatch
	CTime getErrorTaskStartTime() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the filename from the IFileRecord member variable
	//			if the m_ipFileRcd is NULL and exception will be thrown
	std::string getFileName() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the FileID for the record from the IFileRecord member variable
	//			if the m_ipFileRcd is NULL and exception will be thrown
	unsigned long getFileID() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the File size of the file 
	//			if the m_ipFileRcd is NULL and exception will be thrown
	long long getFileSize() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the # of pages for the file
	//			if the m_ipFileRcd is NULL and exception will be thrown
	long getNumberOfPages() const;
	//---------------------------------------------------------------------------------------------

	static const long NO_ID;

private:
	// the UI class must be able to access all these member variables
	friend class FileProcessingDlg;
	friend class FileProcessingDlgStatusPage;
	friend class FileProcessingDlgReportPage;
	friend class TaskInfoDlg;
	friend class CFileProcessingManager;
	friend class FileProcessingQueue;
	friend class FPRecordManager;
	friend class CFileProcessingMgmtRole;

	// TODO: Remove this member when all uses of are replaced with calls to getFileName()
	std::string m_strFile;
	
	ERecordStatus m_eStatus;
	std::string m_strMachine;
	std::string m_strException;

	// A separate exception for the case that we failed while executing an error task
	std::string m_strErrorTaskException;

	StopWatch m_stopWatch;

	// A separate stop watch to time the execution of a failure task.  The duration of m_stopWatchErrorTask
	// will be a subset of m_stopWatch's duration
	StopWatch m_stopWatchErrorTask;

	// Pointer to the File record that contains the ID, FileName, Pages and FileSize of this file
	UCLID_FILEPROCESSINGLib::IFileRecordPtr m_ipFileRcd;

	// Pointer to the ProgressStatus associated with the file record.
	// Note that this ProgressStatus is at the level of "Currently running task #x of y".
	// The progress status of the current task at any given time can be determined from
	// the SubProgressStatus member of this ProgressStatus object.
	IProgressStatusPtr m_ipProgressStatus;
};

//-------------------------------------------------------------------------------------------------
