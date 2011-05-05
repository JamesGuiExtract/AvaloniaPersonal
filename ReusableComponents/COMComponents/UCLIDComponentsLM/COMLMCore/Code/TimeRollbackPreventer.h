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

#include <IConfigurationSettingsPersistenceMgr.h>
#include <Win32Event.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <Random.h>

#include <afxmt.h>
#include <memory>
#include <string>

using namespace std;

// Define the filename for Date-Time encryption unlock file
// read from the CommonComponents folder
const char gpszDateTimeUnlockFile[] = "\\Extract_UnlockLicense.txt";

// Mutex names for TRP checks
const char gpszTrpRunning[] = "Global\\35B5705B-5454-4480-A047-1758143D6737";
const char gpszGoodStateMutex[] = "Global\\61593BF3-DDAC-4688-8252-AFFA0F5F5661";

class TimeRollbackPreventer
{
public:
	//=======================================================================
	// PURPOSE: Constructs a TimeRollbackPreventer object.
	// ARGS:	bLaunchThread - if true then the TRP thread will be created
	TimeRollbackPreventer(bool bLaunchThread = true);
	//=======================================================================
	// PURPOSE: Destructor for the TimeRollbackPreventer object.
	// REQUIRE: Nothing
	// PROMISE: Kills the timer.
	// ARGS:	None
	~TimeRollbackPreventer();
	//=======================================================================
	// PURPOSE: To encrypt the provided bytes.
	// REQUIRE: Nothing
	// PROMISE: To return an encrypted stringized version of the supplied bytes, this string
	//			will have each pair of characters swapped. To get the correct
	//			string back, you need to swap string[i] and string[i+1]
	// ARGS:	bytes - the bytes to be encrypted
	static string encryptUnlockStream(const ByteStream& bytes);
	//=======================================================================
	// PURPOSE: To get the identification data from the the unlock stream. Returns
	//			true if successfully decoded and false otherwise
	static bool getIdentityDataFromUnlockStream(string strCode,
		string& rstrUserComputerName, unsigned long& rulUserSerialNumber,
		string& rstrUserMACAddress, CTime& rtmExpires);
	//=======================================================================
	// PURPOSE: Examines file and registry entries and compares decrypted 
	//          time against system time.  Check is successful if 
	//          1. Both times match and before current system time OR
	//          2. Both items are missing and creation of each works
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	// This is public because it is used in UserLicense Utility
	void checkDateTimeItems();
	//=======================================================================

private:
	//////////
	// Methods
	//////////

	// Decrypts specified Date-Time string using specified byte stream password
	// Results are provided in specified CTime object
	// Returns true if decryption was sucessful, false otherwise
	bool decryptDateTimeString(const string& strEncryptedDT, const ByteStream& bsPassword, 
		CTime* ptmResult, const string& strELITraceCode);

	// Encrypts specified date and time using specified byte stream password
	// Results are provided in specified string
	// Returns true if encryption was sucessful, false otherwise
	bool encryptDateTime(CTime tmTime, const ByteStream& bsPassword, 
		string &strEncryptedDT, const string& strELITraceCode);

	// Decrypts specified Unlock string, checks user computer name, disk serial
	// number, and MAC address. Also compares expiration time against system
	// time.
	// Returns true if checks matched and system time < expiration time, 
	// false otherwise
	bool evaluateUnlockCode(string strCode);

	// Retrieve fully qualified path to Date-Time file.
	// Presence or absence of file is not tested
	string getDateTimeFilePath() const;

	// Retrieve encrypted string from Date-Time file and registry items.
	// Returns empty string if Date-Time item is not found.
	string getLocalDateTimeString(const string& strFileName) const;
	string getRemoteDateTimeString(const string& strPath, const string& strKey) const;
	void deleteLocalDateTimeString(const string& strFileName) const;
	void deleteRemoteDateTimeString(const string& strPath) const;

	// Retrieves encryption/decryption passwords
	// 1: for Date-Time file
	// 2: for Date-Time registry key
	ByteStream getPassword1() const;
	ByteStream getPassword2() const;

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

	// Swaps each pair of characters in the unlock code
	static void swapUnlockCodeChars(string& rstrCode);

	// Friend function definition
	// The TRPThreadProc requires the "this" pointer of the TimeRollBackPreventer instance passed as pParm
	friend UINT TRPThreadProc(LPVOID pParam);

	void writeDateTime(const string& strLocal, const string& strRemote, bool bForceCreation);

	// Stores encrypted string to Date-Time location
	// Returns false if item is not found unless bForceCreation = true
	bool putLocalDateTimeString(const string& strDTFile, const string& strEncrypted, bool bForceCreation = false);
	bool putRemoteDateTimeString(const string& strPath, const string& strKey, const string& strEncrypted, bool bForceCreation = false);

	// Updates file and registry entries with encrypted current system time
	// Update is successful if 
	// 1. Both items already exist and new strings were stored
	// 2. bForceCreation = true and new strings were stored
	void updateDateTimeItems(bool bForceCreation = false);

	// PURPOSE: Creates a unique password to be used in encryption and 
	//				decryption of Unlock License data.
	// REQUIRE: Nothing
	// PROMISE: Provides a 16-byte password.
	// ARGS:	None
	static ByteStream getUnlockPassword();

	void startTrpThread();
	void endTrpThread();

	///////
	// Data
	///////

	// Handles Date-Time Registry items
	unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pRollbackCfgMgr;

	// System time last used to update File and Registry items
	CTime	m_tmLastUpdate;

	// Mutex to protect reading and writing of licensing items
	unique_ptr<CMutex> m_upmutexReadWrite;

	// Mutex to protect reading and processing unlock file
	unique_ptr<CMutex> m_upmutexUnlock;

	// Time to wait for mutex to read or write the file
	unsigned long m_ulRWTimeout;

	Win32Event m_eventKillThread;

	// Pointer to thread that manages periodic updates
	unique_ptr<CWinThread>	m_apThread;

	// Class used to generate random numbers
	Random m_random;

	CMutex* getReadWriteMutex();
	CMutex* getUnlockFileMutex();
};
