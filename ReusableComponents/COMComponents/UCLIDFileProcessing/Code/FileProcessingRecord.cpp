
#pragma once

#include "stdafx.h"
#include "FileProcessingRecord.h"

#include <cpputil.h>
#include <ComUtils.h>
#include <UCLIDException.h>
#include <RegistryPersistenceMgr.h>
#include <RegConstants.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const string gstrFILE_PROCESSING_REG_PATH		= gstrCOM_COMPONENTS_REG_PATH + "\\UCLIDFileProcessing";

const string gstrFPRECORD_FOLDER				= "\\FileProcessingRecord";
const string gstrDISPLAY_PROGRESS_STATUS_KEY	= "DisplayProgressStatus";
const string gstrDEFAULT_PROGRESS_STATUS		= "1";

const long FileProcessingRecord::NO_ID = 0; 

//-------------------------------------------------------------------------------------------------
FileProcessingRecord::FileProcessingRecord()
{
	reset();
}
//-------------------------------------------------------------------------------------------------
FileProcessingRecord::FileProcessingRecord(const UCLID_FILEPROCESSINGLib::IFileRecordPtr& ipFileRcd,
									   const string& strMachine)
{
	reset();
	m_strMachine = strMachine.empty() ? getComputerName() : strMachine;
	m_eStatus = kRecordNone;
	
	m_stopWatch.reset();
	m_stopWatchErrorTask.reset();

	// Set the File Record
	m_lfrFileRcd.setRecord(ipFileRcd);
}
//-------------------------------------------------------------------------------------------------
FileProcessingRecord::FileProcessingRecord(const FileProcessingRecord& task)
{
	copyFrom(task);
}
//-------------------------------------------------------------------------------------------------
FileProcessingRecord& FileProcessingRecord::operator=(const FileProcessingRecord& task)
{
	copyFrom(task);
	return *this;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingRecord::reset()
{
	// reset all member variables
	m_eStatus = kRecordPending;
	m_strMachine = "";
	m_strException = "";
	m_strErrorTaskException = "";
	m_stopWatch.reset();
	m_stopWatchErrorTask.reset();
	m_lfrFileRcd.reset();
	m_ipProgressStatus = NULL;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingRecord::copyFrom(const FileProcessingRecord& task)
{
	m_eStatus = task.m_eStatus;
	m_strMachine = task.m_strMachine;
	m_stopWatch = task.m_stopWatch;
	m_stopWatchErrorTask = task.m_stopWatchErrorTask;
	m_strException = task.m_strException;
	m_strErrorTaskException = task.m_strErrorTaskException;
	m_lfrFileRcd = task.m_lfrFileRcd;
	m_ipProgressStatus = task.m_ipProgressStatus;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingRecord::markAsStarted()
{
	// Start the stop watch and mark the record as current
	m_stopWatch.start();
	m_eStatus = kRecordCurrent;

	// Determine display of progress status
	static bool sbRegistryRead = false;
	static bool sbDisableProgressStatusDisplay = false;
	if (!sbRegistryRead)
	{
		// Create RPM to retrieve setting from registry
		RegistryPersistenceMgr rpm( HKEY_LOCAL_MACHINE, gstrFILE_PROCESSING_REG_PATH );

		// Check for key existence
		if (!rpm.keyExists( gstrFPRECORD_FOLDER, gstrDISPLAY_PROGRESS_STATUS_KEY ))
		{
			// Create key with default value
			rpm.createKey( gstrFPRECORD_FOLDER, gstrDISPLAY_PROGRESS_STATUS_KEY, gstrDEFAULT_PROGRESS_STATUS );
		};

		// Retrieve the value
		int nDisplayProgressStatus = asLong( rpm.getKeyValue( 
			gstrFPRECORD_FOLDER, gstrDISPLAY_PROGRESS_STATUS_KEY ) );

		// Set flags
		sbDisableProgressStatusDisplay = (nDisplayProgressStatus == 0);
		sbRegistryRead = true;
	}

	// Create and initialize the progress status object if appropriate
	if (!sbDisableProgressStatusDisplay)
	{
		// TODO: if an exception is thrown from this location, the FAM does not behave
		// correctly.  The FAM stops processing, but the file still stays in the "currently being
		// processed" grid.  Need to find out why and fix the problem.

		// Create the progress status object
		m_ipProgressStatus.CreateInstance(CLSID_ProgressStatus);
		ASSERT_RESOURCE_ALLOCATION("ELI16075", m_ipProgressStatus != NULL);

		// Initialize the progress status object
		m_ipProgressStatus->InitProgressStatus("Initializing processing...", 0, 1, VARIANT_TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingRecord::markAsCompleted()
{
	m_stopWatch.stop();
	m_eStatus = kRecordComplete;
	
	// Partial fix to P16#2457 (see notes of that record)
	// There may be thousands of records shown in the completed-files-list, depending upon the 
	// setting configured by the user in the options dialog.
	// We don't want a progress status object to be stored for each of those records as it just
	// takes up more memory and OS handles (each COM object consumes one OS handle).
	
	// Since this record has completed processing, the progress status object is not useful
	// anyway.  Just release the associated memory.
	m_ipProgressStatus = NULL;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingRecord::markAsPending()
{
	m_stopWatch.stop();
	m_eStatus = kRecordPending;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingRecord::markAsFailed(const string& strException)
{
	m_stopWatch.stop();
	m_strException = strException;
	m_eStatus = kRecordFailed;

	// Partial fix to P16#2457 (see notes of that record)
	// There may be thousands of records shown in the failed-files-list, depending upon the 
	// setting configured by the user in the options dialog.
	// We don't want a progress status object to be stored for each of those records as it just
	// takes up more memory and OS handles (each COM object consumes one OS handle).
	
	// Since this record has failed processing, the progress status object is not useful
	// anyway.  Just release the associated memory.
	m_ipProgressStatus = NULL;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingRecord::markAsNone()
{
	m_stopWatch.stop();
	m_eStatus = kRecordNone;
}
//---------------------------------------------------------------------------------------------
void FileProcessingRecord::markAsProcessingError(const string& strException)
{
	m_stopWatch.stop();
	m_strException = strException;
	m_eStatus = kRecordProcessingError;
}
//---------------------------------------------------------------------------------------------
void FileProcessingRecord::markAsSkipped()
{
	m_stopWatch.stop();
	m_eStatus = kRecordSkipped;

	// Partial fix to P16#2457 (see notes of that record)
	// There may be thousands of records shown in the completed-files-list, depending upon the 
	// setting configured by the user in the options dialog.
	// We don't want a progress status object to be stored for each of those records as it just
	// takes up more memory and OS handles (each COM object consumes one OS handle).
	
	// Since this record has completed processing, the progress status object is not useful
	// anyway.  Just release the associated memory.
	m_ipProgressStatus = NULL;
}
//---------------------------------------------------------------------------------------------
void FileProcessingRecord::notifyRunningErrorTask()
{
	// m_stopWatch will include the time elapsed while running m_stopWatchErrorTask 
	m_stopWatch.start();
	m_stopWatchErrorTask.start();
}
//---------------------------------------------------------------------------------------------
void FileProcessingRecord::notifyErrorTaskCompleted()
{
	// m_stopWatch will include the time elapsed while running m_stopWatchErrorTask
	m_stopWatchErrorTask.stop();
	m_stopWatch.stop();
}
//---------------------------------------------------------------------------------------------
void FileProcessingRecord::notifyErrorTaskFailed(const string& strException)
{
	// m_stopWatch will include the time elapsed while running m_stopWatchErrorTask
	m_stopWatchErrorTask.stop();
	m_stopWatch.stop();

	m_strErrorTaskException = strException;
}
//---------------------------------------------------------------------------------------------
double FileProcessingRecord::getTaskDuration() const
{
	return m_stopWatch.getElapsedTime();
}
//-------------------------------------------------------------------------------------------------
CTime FileProcessingRecord::getStartTime() const
{
	return m_stopWatch.getBeginTime();
}
//-------------------------------------------------------------------------------------------------
double FileProcessingRecord::getErrorTaskDuration() const
{
	return m_stopWatchErrorTask.getElapsedTime();
}
//-------------------------------------------------------------------------------------------------
CTime FileProcessingRecord::getErrorTaskStartTime() const
{
	return m_stopWatchErrorTask.getBeginTime();
}
//-------------------------------------------------------------------------------------------------
string FileProcessingRecord::getFileName() const
{
	if (m_lfrFileRcd.FileRecord == NULL)
	{
		UCLIDException ue("ELI14211", "File record is not set.");
		throw ue;
	}
	// since the file name can be changed inside the file record this now needs to return the 
	// file name in the stored FileRecord object
	return asString(m_lfrFileRcd.FileRecord->Name);
}
//-------------------------------------------------------------------------------------------------
long FileProcessingRecord::getFileID() const
{
	if (m_lfrFileRcd.FileRecord == NULL)
	{
		UCLIDException ue("ELI14212", "File record is not set.");
		throw ue;
	}
	return m_lfrFileRcd.FileID;
}
//-------------------------------------------------------------------------------------------------
long FileProcessingRecord::getActionID() const
{
	if (m_lfrFileRcd.FileRecord == NULL)
	{
		UCLIDException ue("ELI26738", "File record is not set.");
		throw ue;
	}

	return m_lfrFileRcd.ActionID;
}
//-------------------------------------------------------------------------------------------------
long long FileProcessingRecord::getFileSize() const
{
	if (m_lfrFileRcd.FileRecord == NULL)
	{
		UCLIDException ue("ELI14252", "File record is not set.");
		throw ue;
	}
	return m_lfrFileRcd.FileSize;
}
//-------------------------------------------------------------------------------------------------
long FileProcessingRecord::getNumberOfPages() const
{
	if (m_lfrFileRcd.FileRecord == NULL)
	{
		UCLIDException ue("ELI14253", "File record is not set.");
		throw ue;
	}
	return m_lfrFileRcd.NumberOfPages;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::EFilePriority FileProcessingRecord::getPriority() const
{
	if (m_lfrFileRcd.FileRecord == NULL)
	{
		UCLIDException ue("ELI27648", "File record is not set.");
		throw ue;
	}
	return m_lfrFileRcd.Priority;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileRecordPtr FileProcessingRecord::getFileRecord() 
{
	return m_lfrFileRcd.getFileRecord();
}