#pragma once
#include "stdafx.h"
#include "uclidfileprocessing.h"

#include <COMUtils.h>
#include <StopWatch.h>
#include <UCLIDException.h>

#include <string>

using namespace std;


// FPWorkItem class - This is patterened after FileProcessingRecord class
class FPWorkItem
{
public:

	FPWorkItem(const UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr& ipWorkItem);

	FPWorkItem();

	// Copy Constructor and assignment operator
	FPWorkItem(const FPWorkItem& workItem);
	FPWorkItem& operator=(const FPWorkItem& workItem);

	~FPWorkItem(void);

	// PURPOSE: To reset all member variables
	void reset();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To make this object a copy of the object 'workItem'
	void copyFrom(const FPWorkItem& workItem);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To change status to current, start the stop watch, and perform other 
	//			initialization.
	// NOTES:	This method will initialize the top-level progress status object if and only if
	//			displaying of progress status information is enabled at the time of this call.
	void markAsStarted();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To change status to complete, and compute workItem duration.
	void markAsCompleted();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To change status to failed, compute workItem duration,
	// and to record the specified exception
	void markAsFailed(const string& strException);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return the number of seconds elapsed during the execution
	// of this task (regardless of whether the task completed successfully or not)
	double getWorkItemDuration();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the starting time of the stopwatch
	SYSTEMTIME getStartTime() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the filename from the IWorkItemRecord member variable
	//			if the m_ipWorkItemRecord is NULL an exception will be thrown
	string getFileName() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the FileID from the IWorkItemRecord member variable
	//			if the m_ipWorkItemRecord is NULL an exception will be thrown
	long getFileID() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the WorkItemID for the record from the IWorkItemRecrord member variable
	//			if the m_ipWorkItemRecord is NULL an exception will be thrown
	long getWorkItemID() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the WorkItemGroupID from the IWorkItemRecord member variable
	//			if the m_ipWorkItemRecord is NULL an exception will be thrown
	long getWorkItemGroupID() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the WorkItemRecord
	UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr getWorkItemRecord();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the priority for the work item
	UCLID_FILEPROCESSINGLib::EFilePriority getPriority();

private:
	// the UI class must be able to access all these member variables
	friend class FileProcessingDlg;
	friend class FPRecordManager;
	friend class CFileProcessingMgmtRole;
	friend class WorkItemsPage;

	// The work item record associated with this structure
	UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr m_ipWorkItemRecord;
	
	// Used to keep track of processing time
	StopWatch m_stopWatch;
	
	// Progress object used by the UI to display progress
	IProgressStatusPtr m_ipProgressStatus;

	// Stringized exception if the status is failed
	string m_strException;

	// Current processing status of the work item
	EWorkItemStatus m_status;
};

