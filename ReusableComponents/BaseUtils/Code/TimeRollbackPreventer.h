//============================================================================
//
// COPYRIGHT (c) 2004 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	TimeRollbackPreventer.h
//
// PURPOSE:	Definition of the TimeRollbackPreventer class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#pragma once

#include "BaseUtils.h"
#include "IConfigurationSettingsPersistenceMgr.h"
#include "Win32Event.h"
#include "ByteStream.h"
#include "ByteStreamManipulator.h"

#include <afxmt.h>
#include <memory>
#include <string>

using namespace std;

// Define the filename for Date-Time encryption unlock file
// read from the CommonComponents folder
const char gpszDateTimeUnlockFile[] = "Extract_UnlockLicense.txt";

class EXPORT_BaseUtils TimeRollbackPreventer
{
public:
	//=======================================================================
	// PURPOSE: Constructs a TimeRollbackPreventer object.
	// REQUIRE: Reference to manually reset event to signal a bad state
	// PROMISE: If a bad license state is detect the rEventBadState event will be signaled
	// ARGS:	None
	TimeRollbackPreventer(Win32Event &rEventBadState );
	//=======================================================================
	// PURPOSE: Destructor for the TimeRollbackPreventer object.
	// REQUIRE: Nothing
	// PROMISE: Kills the timer.
	// ARGS:	None
	~TimeRollbackPreventer();
	//=======================================================================
	// PURPOSE: Creates a unique password to be used in encryption and 
	//				decryption of Unlock License data.
	// REQUIRE: Nothing
	// PROMISE: Provides a 16-byte password.
	// ARGS:	None
	static const ByteStream& getUnlockPassword();
	//=======================================================================
	// PURPOSE: Examines file and registry entries and compares decrypted 
	//          time against system time.  Check is successful if 
	//          1. Both times match and before current system time OR
	//          2. Both items are missing and creation of each works
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	// This is public because it is used in UserLicense Utility
	void	checkDateTimeItems();
	//=======================================================================

private:
	//////////
	// Methods
	//////////

	// Decrypts specified Date-Time string using specified byte stream password
	// Results are provided in specified CTime object
	// Returns true if decryption was sucessful, false otherwise
	bool decryptDateTimeString(std::string strEncryptedDT, const ByteStream& bsPassword, 
		CTime* ptmResult);

	// Encrypts specified date and time using specified byte stream password
	// Results are provided in specified string
	// Returns true if encryption was sucessful, false otherwise
	bool encryptDateTime(CTime tmTime, const ByteStream& bsPassword, 
		std::string &strEncryptedDT);

	// Decrypts specified Unlock string, checks user computer name, disk serial
	// number, and MAC address. Also compares expiration time against system
	// time.
	// Returns true if checks matched and system time < expiration time, 
	// false otherwise
	bool evaluateUnlockCode(std::string strCode);

	// Retrieve fully qualified path to Date-Time file.
	// Presence or absence of file is not tested
	std::string getDateTimeFilePath() const;

	// Retrieve encrypted string from Date-Time file and registry items.
	// Returns empty string if Date-Time item is not found.
	std::string getLocalDateTimeString(std::string strFileName) const;
	std::string getRemoteDateTimeString(std::string strPath, std::string strKey) const;
	void deleteLocalDateTimeString(std::string strFileName) const;
	void deleteRemoteDateTimeString(std::string strPath, std::string strKey) const;

	// Retrieves encryption/decryption passwords
	// 1: for Date-Time file
	// 2: for Date-Time registry key
	const ByteStream& getPassword1() const;
	const ByteStream& getPassword2() const;

	// Checks for UCLID_UnlockLicense.txt file in CommonComponents folder.
	// If file is found, decrypts text to retrieve:
	// 1. Disk serial number
	// 2. Computer name
	// 3. MAC address
	// 4. Unlock expiration date
	// If first three items match and current system date is before 
	// the expiration date, the Date-Time file and registry items 
	// are reset.  The unlock string is also stored in the registry
	// to prevent the unlock code from being used more than once.
	// The unlock file is deleted after the check is completed.
	// If the current unlock string is found in the registry, the 
	// file is deleted and the Date-Time items are unchanged.
	void handleUnlockCode();

	// Friend function definition
	// The TRPThreadProc requires the "this" pointer of the TimeRollBackPreventer instance passed as pParm
	friend UINT TRPThreadProc(LPVOID pParam);

	void writeDateTime(std::string strLocal, std::string strRemote, bool bForceCreation);

	// Stores encrypted string to Date-Time location
	// Returns false if item is not found unless bForceCreation = true
	bool putLocalDateTimeString(std::string strFileName, std::string strEncrypted, bool bForceCreation = false);
	bool putRemoteDateTimeString(std::string strPath, std::string strKey, std::string strEncrypted, bool bForceCreation = false);

	// Updates file and registry entries with encrypted current system time
	// Update is successful if 
	// 1. Both items already exist and new strings were stored
	// 2. bForceCreation = true and new strings were stored
	void updateDateTimeItems(bool bForceCreation = false);

	///////
	// Data
	///////

	// Handles Date-Time Registry items
	std::auto_ptr<IConfigurationSettingsPersistenceMgr> ma_pRollbackCfgMgr;

	// System time last used to update File and Registry items
	CTime	m_tmLastUpdate;

	// Mutex to protect reading and writing of licensing items
	static CMutex ms_mutexReadWrite;

	// Time to wait for mutex to read or write the file
	unsigned long m_ulRWTimeout;

	Win32Event m_eventKillThread;

	// Pointer to a event indicating a bad state
	Win32Event &m_rEventBadState;

	// Pointer to thread that manages periodic updates
	std::auto_ptr<CWinThread>	m_apThread;

private:
	// Registry keys for information persistence
	static const std::string ITEM_SECTION_NAME1;
	static const std::string ITEM_SECTION_NAME2;
	static const std::string COUNT_SECTION_NAME;
	static const std::string UNLOCK_SECTION_NAME;
	static const std::string LAST_TIME_USED;
	static const std::string COUNT;
};
