
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

//--------------------------------------------------------------------------------------------------
// Static members
//--------------------------------------------------------------------------------------------------
bool FileProcessingRecord::m_sbDisableProgressStatusDisplay = false;

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
	m_dTaskDuration = 0.0;
	m_ipProgressStatus = __nullptr;
	m_ipSubProgressStatus = __nullptr;
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
	m_ipSubProgressStatus = task.m_ipSubProgressStatus;
	m_dTaskDuration = task.m_dTaskDuration;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingRecord::markAsStarted()
{
	// Mark the record as current
	m_eStatus = kRecordCurrent;

	// Determine display of progress status
	static bool sbRegistryRead = false;
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
			gstrFPRECORD_FOLDER, gstrDISPLAY_PROGRESS_STATUS_KEY, gstrDEFAULT_PROGRESS_STATUS ) );

		// Set flags
		m_sbDisableProgressStatusDisplay = (nDisplayProgressStatus == 0);
		sbRegistryRead = true;
	}

	// Create and initialize the progress status object if appropriate
	if (!m_sbDisableProgressStatusDisplay)
	{
		// TODO: if an exception is thrown from this location, the FAM does not behave
		// correctly.  The FAM stops processing, but the file still stays in the "currently being
		// processed" grid.  Need to find out why and fix the problem.

		// Create the progress status object
		m_ipProgressStatus.CreateInstance(CLSID_ProgressStatus);
		ASSERT_RESOURCE_ALLOCATION("ELI16075", m_ipProgressStatus != __nullptr);
		
		// Initialize the progress status object
		m_ipProgressStatus->InitProgressStatus("Initializing processing...", 0, 1, VARIANT_TRUE);

		// Create the sub progress status object
		m_ipSubProgressStatus.CreateInstance(CLSID_ProgressStatus);
		ASSERT_RESOURCE_ALLOCATION("ELI37635", m_ipSubProgressStatus != __nullptr);
		m_ipProgressStatus->SubProgressStatus = m_ipSubProgressStatus;
	}

	// Start m_stopWatch and progress timer, if it exists
	startTaskTimer();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingRecord::markAsCompleted()
{
	stopTaskTimer();
	m_eStatus = kRecordComplete;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingRecord::markAsPending()
{
	stopTaskTimer();
	m_eStatus = kRecordPending;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingRecord::markAsFailed(const string& strException)
{
	stopTaskTimer();
	m_strException = strException;
	m_eStatus = kRecordFailed;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingRecord::markAsNone()
{
	stopTaskTimer();
	m_eStatus = kRecordNone;
}
//---------------------------------------------------------------------------------------------
void FileProcessingRecord::markAsProcessingError(const string& strException)
{
	stopTaskTimer();
	m_strException = strException;
	m_eStatus = kRecordProcessingError;
}
//---------------------------------------------------------------------------------------------
void FileProcessingRecord::markAsSkipped()
{
	stopTaskTimer();
	m_eStatus = kRecordSkipped;
}
//---------------------------------------------------------------------------------------------
void FileProcessingRecord::notifyRunningErrorTask()
{
	// m_stopWatch will include the time elapsed while running m_stopWatchErrorTask 
	startTaskTimer();
	m_stopWatchErrorTask.start();
}
//---------------------------------------------------------------------------------------------
void FileProcessingRecord::notifyErrorTaskCompleted()
{
	// m_stopWatch will include the time elapsed while running m_stopWatchErrorTask
	m_stopWatchErrorTask.stop();
	stopTaskTimer();
}
//---------------------------------------------------------------------------------------------
void FileProcessingRecord::notifyErrorTaskFailed(const string& strException)
{
	// Task timer will include the time elapsed while running m_stopWatchErrorTask
	m_stopWatchErrorTask.stop();
	stopTaskTimer();

	m_strErrorTaskException = strException;
}
//---------------------------------------------------------------------------------------------
bool FileProcessingRecord::isActive()
{
	return m_stopWatch.isRunning();
}
//---------------------------------------------------------------------------------------------
void FileProcessingRecord::startTaskTimer()
{
	m_stopWatch.start();
	if (m_ipSubProgressStatus != __nullptr)
	{
		m_ipSubProgressStatus->StartProgressTimer();
	}
}
//---------------------------------------------------------------------------------------------
void FileProcessingRecord::stopTaskTimer()
{
	m_stopWatch.stop();
	if (m_ipSubProgressStatus != __nullptr)
	{
		m_ipSubProgressStatus->StopProgressTimer();
		
		// Partial fix to P16#2457 (see notes of that record)
		// There may be thousands of records shown in the completed and failed-files lists,
		// depending upon the setting configured by the user in the options dialog.
		// We don't want a progress status object to be stored for each of those records as it just
		// takes up more memory and OS handles (each COM object consumes one OS handle).
	
		// Since this record has completed or failed processing or been set back to pending,
		// the progress status objects are no longer useful.
		
		// Save the sub-progress status time since that reflects actual task time
		// (Top level is just "Executing task #..." and might also describe time spent waiting
		// for processor time)
		m_dTaskDuration = m_ipSubProgressStatus->GetProgressDuration();
		
		m_ipProgressStatus = __nullptr;
		m_ipSubProgressStatus = __nullptr;
	}
}
//---------------------------------------------------------------------------------------------
double FileProcessingRecord::getTaskDuration()
{
	// If no progress status object was used then return the stopwatch time
	if (m_sbDisableProgressStatusDisplay)
	{
		return m_stopWatch.getElapsedTime();
	}
	// Otherwise use time from the progress status object if is still exists
	else if (m_ipSubProgressStatus != __nullptr)
	{
		return m_ipSubProgressStatus->GetProgressDuration();
	}
	// If the progress status object was discarded then this record is no longer active and its
	// duration was recorded in m_dTaskDuration. Since error tasks are run after the progress
	// status object is gone, the error task time needs to be added to the main task time.
	else
	{
		return m_dTaskDuration + getErrorTaskDuration();
	}
}
//-------------------------------------------------------------------------------------------------
CTime FileProcessingRecord::getStartTime() const
{
	return m_stopWatch.getBeginTime();
}
//-------------------------------------------------------------------------------------------------
double FileProcessingRecord::getErrorTaskDuration()
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
UCLID_FILEPROCESSINGLib::EActionStatus FileProcessingRecord::getFallbackStatus() const
{
	if (m_lfrFileRcd.FileRecord == __nullptr)
	{
		UCLIDException ue("ELI37482", "File record is not set.");
		throw ue;
	}
	return m_lfrFileRcd.FileRecord->FallbackStatus;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileRecordPtr FileProcessingRecord::getFileRecord() 
{
	return m_lfrFileRcd.getFileRecord();
}