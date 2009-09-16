
#pragma once

#include <UCLIDException.h>

#include <string>

using namespace std;

//---------------------------------------------------------------------------------------------
// EFileSupplyingRecordType
//---------------------------------------------------------------------------------------------
// Enumerated events to be logged on the Queue Log page
enum EFileSupplyingRecordType
{
	kNoAction = 0,
	kFileAdded = 1,
	kFileRemoved = 2,
	kFileRenamed = 3,
	kFileModified = 4,
	kFolderRemoved = 5,
	kFolderRenamed = 6
};

//---------------------------------------------------------------------------------------------
// EFileSupplyingRecordType
//---------------------------------------------------------------------------------------------
// This enum has enumerations to represent which list a record appears in the queue log tab
enum EQueueEventStatus
{
	kQueueEventReceived, 
	kQueueEventHandled,
	kQueueEventFailed
};

//---------------------------------------------------------------------------------------------
// FileSupplyingRecord
//---------------------------------------------------------------------------------------------
// Information associated with one or more Queue Log events
struct FileSupplyingRecord
{
	// ctor
	FileSupplyingRecord();

	// Unique identifier for this file in the database
	unsigned long m_ulFileID;

	// Event that affects the database queue
	EFileSupplyingRecordType m_eFSRecordType;

	// Original name of file or folder for Rename event
	string m_strOriginalFileName;

	// Name of file affected by the queue event
	// Also holds new name of file or folder for Rename event
	string m_strNewFileName;

	// Number of pages in image file OR 
	// 0 if m_strNewFileName is not an image file
	unsigned long m_ulNumPages;

	// Description string associated with the File Supplier
	string m_strFSDescription;

	// True if result from database operation indicates that 
	// m_strNewFileName already exists in the database queue,
	// False otherwise
	bool m_bAlreadyExisted;

	// Status of m_strNewFileName in the database queue
	// as { "U", "P", "R", "C", "F" }
	UCLID_FILEPROCESSINGLib::EActionStatus m_ePreviousActionStatus;

	// what is the status of this queue event
	EQueueEventStatus m_eQueueEventStatus;

	// execption, if any, associated with handling this event
	UCLIDException m_ueException;

	// Priority of the file being queued
	string m_strPriority;
};
//---------------------------------------------------------------------------------------------
