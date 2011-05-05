//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2004 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	TimeRollbackPreventer.cpp
//
// PURPOSE:	Implementation of the TimeRollbackPreventer class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//-------------------------------------------------------------------------------------------------

#include "stdafx.h"
#include "TimeRollbackPreventer.h"

#include <RegistryPersistenceMgr.h>
#include <EncryptionEngine.h>
#include <FileDateTimeRestorer.h>
#include <UCLIDException.h>
#include <ExtractMFCUtils.h>
#include <cpputil.h>
#include <RegConstants.h>
#include <MutexUtils.h>

#include <shlobj.h>
#include <iostream>

using namespace std;

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Registry keys for Date-Time items
//   NOTE: Each key is under HKEY_CURRENT_USER
//const string TimeRollbackPreventer::ITEM_SECTION_NAME = "\\Windows\\System32";	// Old location
const string ITEM_SECTION_NAME1 = "Identities\\{7FEF3749-A8CC-4CD0-9CEB-E6D267FA524E}";
const string ITEM_SECTION_NAME2 = "Identities\\{526988F0-27BE-4451-B741-D8614827B838}";

const string COUNT_SECTION_NAME = "Software\\Windows";
const string UNLOCK_SECTION_NAME = "Software\\Classes\\Code";

// Registry keys
const string LAST_TIME_USED = "LTUSWU";
const string COUNT = "Count";

// Define the user-specific subfolder and filename for Date-Time encryption
//   i.e. full path will be $(Documents and Settings\Username\Application Data) + gstrDateTimeSubfolderFile
const string gstrDateTimeSubfolderFile = "\\Windows\\{EFF9AEFC-3046-48BC-84D1-E9862F9D1E22}\\estrpmfc.dll";

// Min and max for the polling interval range, polling will occur at a random number
// of seconds between 30 and 300
const unsigned long gulTRP_TIMER_MIN = 30;
const unsigned long gulTRP_TIMER_MAX = 300;

// Modulo constant for random additions to DT strings
const unsigned long gulMODULO_CONSTANT = 53;

// Standard ELI error message indicating license corruption
const char gpszLicenseIsCorrupt[] = 
	"Extract Systems license state has been corrupted!\r\n\r\nPlease contact Extract Systems technical support to restore your system to a valid licensed state.";

// Registry Constants for Settings
const string strCOMLM_CORE_SETTINGS = gstrBASEUTILS_REG_PATH;
const string strTRP_RW_TIMEOUT_KEY = "TRP_ReadWriteTimeOut";
const unsigned long ulDEFAULT_RW_TIME0UT = 60000; // 1 minute

// Define twelve UCLID passwords used for Date-Time encryption
// 1 - 4 : For Date-Time File
// 5 - 8 : For Date-Time Registry Key
// 9 - C : For Unlock File and Registry Key(s)
const unsigned long	gulUCLIDDateTimeKey1 = 0x4FAE190C;
const unsigned long	gulUCLIDDateTimeKey2 = 0x459665E7;
const unsigned long	gulUCLIDDateTimeKey3 = 0x57860D67;
const unsigned long	gulUCLIDDateTimeKey4 = 0x603851E7;
const unsigned long	gulUCLIDDateTimeKey5 = 0x58951DAE;
const unsigned long	gulUCLIDDateTimeKey6 = 0x0E9D58BD;
const unsigned long	gulUCLIDDateTimeKey7 = 0x17812F25;
const unsigned long	gulUCLIDDateTimeKey8 = 0x296F0F43;
const unsigned long	gulUCLIDDateTimeKey9 = 0x6A642D5C;
const unsigned long	gulUCLIDDateTimeKeyA = 0x1BB30F42;
const unsigned long	gulUCLIDDateTimeKeyB = 0x42791871;
const unsigned long	gulUCLIDDateTimeKeyC = 0x7D1C1047;

//-------------------------------------------------------------------------------------------------
// Thread Procedure
//-------------------------------------------------------------------------------------------------
// This requires the TimeRollbackPreventer "this" pointer to be passed as pParam
UINT TRPThreadProc(LPVOID pParam)
{
	try
	{
		// make sure the pParam is not null
		ASSERT_ARGUMENT( "ELI12995", pParam != __nullptr );
		TimeRollbackPreventer *trpInstance = (TimeRollbackPreventer *) pParam;

		unique_ptr<CMutex> pValidState(getGlobalNamedMutex(gpszGoodStateMutex));
		ASSERT_RESOURCE_ALLOCATION("ELI32540", pValidState != __nullptr);

		try
		{
			Random random;

			// Set the state to valid
			CSingleLock lValid(pValidState.get(), FALSE);
			while (!trpInstance->m_eventKillThread.isSignaled()
				&& lValid.Lock(1000) == FALSE);

			// Check if end thread is signaled
			if (trpInstance->m_eventKillThread.isSignaled())
			{
				return 0;
			}

			// Continuous updates
			do
			{
				// Update the Date-Time items
				trpInstance->updateDateTimeItems();
			}
			while (trpInstance->m_eventKillThread.wait(
				random.uniform(gulTRP_TIMER_MIN, gulTRP_TIMER_MAX+1) * 1000 ) == WAIT_TIMEOUT);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10692");
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI07616", "Error in maintaining licensed state!", ue);
		
		// Force display (and logging) of exception
		uexOuter.display();
	}
	return 0;
}

//-------------------------------------------------------------------------------------------------
// TimeRollbackPreventer
//-------------------------------------------------------------------------------------------------
TimeRollbackPreventer::TimeRollbackPreventer(bool bLaunchThread)
:	m_tmLastUpdate(-1),
	m_ulRWTimeout(ulDEFAULT_RW_TIME0UT),
	m_apThread(__nullptr)
{
	try
	{
		try
		{
			// Setup Registry items
			ma_pRollbackCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
				new RegistryPersistenceMgr( HKEY_CURRENT_USER, "" ));

			try
			{
				// Attempt to get the TRP RW value from the registry, if the value is not
				// there or invalid, just use the default value
				string strTimeOut = ma_pRollbackCfgMgr->getKeyValue(
					strCOMLM_CORE_SETTINGS, strTRP_RW_TIMEOUT_KEY);
				if (!strTimeOut.empty())
				{
					m_ulRWTimeout = asUnsignedLong(strTimeOut);
				}
			}
			catch(...)
			{
				// Just eat the exception, and use default RW timeout value
			}

			// This try catch is just to give more trace information
			try
			{
				// Check license items
				checkDateTimeItems();
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10709");

			if (bLaunchThread)
			{
				startTrpThread();
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10693");
	}
	catch(UCLIDException& ue)
	{
#ifdef _DEBUG
		ue.log();
#endif
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
TimeRollbackPreventer::~TimeRollbackPreventer()
{
	try
	{
		endTrpThread();

		// Reset the auto pointers
		m_upmutexReadWrite.reset();
		m_upmutexUnlock.reset();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16412");
}
//-------------------------------------------------------------------------------------------------
void TimeRollbackPreventer::checkDateTimeItems()
{
	try
	{
		////////////////////////////////////////////
		// Check for system time already rolled back
		////////////////////////////////////////////
		string strDLL = getModuleDirectory("BaseUtils.dll");
		strDLL += "\\";
		strDLL += "COMLMCore.dll";
		CTime tmDLL = getFileModificationTimeStamp( strDLL );
		CTime tmNow = CTime::GetCurrentTime();
		if (tmNow < tmDLL)
		{
			// Signal BAD STATE event and throw exception
			// This Exception and ELI code will be seen in UEX log file
			UCLIDException ue( "ELI11549", gpszLicenseIsCorrupt );
			ue.log();
			throw ue;
		}

		// Check for unlock code found
		// This try catch is just to give more trace information
		try
		{
			handleUnlockCode();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10705");

		// Get the encrypted data from (local) DT file and registry
		string	strLocalData1;
		string	strRemoteData1;
		string	strLocalData2;
		string	strRemoteData2;
		{
			// Protect the read accesses
			CSingleLock lg( getReadWriteMutex() );
			if ( lg.Lock(m_ulRWTimeout) == 0 )
			{
				// unable to lock the mutex signal bad state and throw exception
				UCLIDException ue( "ELI12996", gpszLicenseIsCorrupt );
				ue.log();
				throw ue;
			}

			string strDateTimeFilePath = getDateTimeFilePath();
			strLocalData1 = getLocalDateTimeString(strDateTimeFilePath);
			strRemoteData1 = getRemoteDateTimeString(ITEM_SECTION_NAME1, LAST_TIME_USED);

			strLocalData2 = getLocalDateTimeString(strDateTimeFilePath + ".old");
			strRemoteData2 = getRemoteDateTimeString(ITEM_SECTION_NAME2, LAST_TIME_USED);
		}

		// Convert the encrypted Date-Time strings into CTime objects
		CTime	tmLocal;
		CTime	tmRemote;
		bool	bLocal = decryptDateTimeString( strLocalData1, getPassword1(), &tmLocal, "ELI32494" );
		bool	bRemote = decryptDateTimeString( strRemoteData1, getPassword2(), &tmRemote, "ELI32495" );

		////////////////
		// Check results
		////////////////

		bool bFail = false;
		bool bTryBackup = false;
		string strELI = "ELI00000";

		// Try to compare the date/time from the primary locations

		// Both encrypted strings were found AND
		// both Date-Time values were decrypted
		if (bLocal && bRemote)
		{
			// Compare decrypted times
			if (tmLocal == tmRemote)
			{
				// Current time must be greater than saved times
				CTime currentTime = CTime::GetCurrentTime();
				if (currentTime < tmLocal)
				{
					strELI = "ELI07444";
					bFail = true;
					bTryBackup = false;
				}
				else
				{
					bFail = false;
					bTryBackup = false;
				}
			}
			// Primary: File time is before Registry time ---> ERROR
			else if (tmLocal < tmRemote)
			{
				strELI = "ELI12748";
				bFail = true;
				bTryBackup = true;
			}
			// Stored times do not match ---> ERROR
			// Primary: Registry time is before File time ---> ERROR
			else
			{
				strELI = "ELI07443";
				bFail = true;
				bTryBackup = true;
			}
		}
		// Both encrypted strings were missing
		else if (strLocalData1.empty() && strRemoteData1.empty())
		{
			// This try catch is just to give more trace information
			try
			{
				// Force creation of the local and remote items
				updateDateTimeItems( true );
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10704");
		}
		// Remote item is missing or corrupt ---> ERROR
		else if (bLocal && !bRemote)
		{
			// Throw exception
			strELI = "ELI07408";
			bFail = true;
			bTryBackup = true;
		}
		// Local item is missing or string is corrupt ---> ERROR
		else if (!bLocal && bRemote)
		{
			strELI = "ELI07441";
			bFail = true;
			bTryBackup = true;
		}
		// Both items found and both failed decryption ---> ERROR
		else
		{
			strELI = "ELI08393";
			bFail = true;
			bTryBackup = true;
		}

		// This exception will only be thrown if the backup location check fails
		// It is created here to preserve the ELI code from the failure of the 
		// first exception
		UCLIDException ue(strELI, gpszLicenseIsCorrupt);

		// If the data from the primary location is missing or corrupt it 
		// could be the result of an unexpected program failure last time 
		// the TRLP was run.  In that case we will try to comapre data from the backup
		// location
		if (bTryBackup)
		{
			// Convert the encrypted Date-Time strings into CTime objects
			bLocal = decryptDateTimeString( strLocalData2, getPassword1(), &tmLocal, "ELI32496" );
			bRemote = decryptDateTimeString( strRemoteData2, getPassword2(), &tmRemote, "ELI32497" );

			if (bLocal && bRemote)
			{
				// Compare decrypted times
				if (tmLocal == tmRemote)
				{
					// Current time must be greater than saved times
					CTime currentTime = CTime::GetCurrentTime();
					if (currentTime < tmLocal)
					{
						ue.addDebugInfo("Trace", "ELI10985");
						bFail = true;
					}
					else
					{
						bFail = false;
					}
				}
				// Stored times do not match ---> ERROR
				else if (tmLocal < tmRemote)
				{
					ue.addDebugInfo("Trace", "ELI12749");
					bFail = true;
				}
				// Stored times do not match ---> ERROR
				else
				{
					ue.addDebugInfo("Trace", "ELI10983");
					bFail = true;
				}
			}
			else if (!bLocal && !bRemote)
			{
				// Both decryptions failed
				ue.addDebugInfo("Trace", "ELI12750");
				bFail = true;
			}
			else if (!bLocal)
			{
				// Local decryption failed
				ue.addDebugInfo("Trace", "ELI12751");
				bFail = true;
			}
			else
			{
				// Remote decryption failed
				ue.addDebugInfo("Trace", "ELI10984");
				bFail = true;
			}
		}

		// If both the primary and backup comparisons fail
		// the we will invalidate the license state.
		if (bFail)
		{
			// Make sure that the exception gets logged
			ue.log();

			// Signal a badstate
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32493");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool TimeRollbackPreventer::decryptDateTimeString(const string& strEncryptedDT, 
												  const ByteStream& bsPassword, 
												  CTime* ptmResult,
												  const string& strELITraceCode)
{
	try
	{
		try
		{
			// Decrypt the bytes
			ByteStream bsEncrypted( strEncryptedDT );
			ByteStream bsUnencrypted;
			EncryptionEngine ee;
			ee.decrypt( bsUnencrypted, bsEncrypted, bsPassword );

			// Extract CTime data from the bytes
			ByteStreamManipulator bsm( ByteStreamManipulator::kRead, bsUnencrypted );

			// Get first random unsigned short and modulo and confirm divisibility by modulo constant
			unsigned long val(0), mod(0);
			bsm >> val;
			bsm >> mod;
			if (val % gulMODULO_CONSTANT != mod)
			{
				return false;
			}

			// Retrieve CTime data
			CTime	tmTemp;
			bsm >> tmTemp;

			// Get second random unsigned short and confirm divisibility by modulo constant
			bsm >> val;
			bsm >> mod;
			if (val % gulMODULO_CONSTANT != mod)
			{
				return false;
			}

			// Provide CTime to caller
			*ptmResult = tmTemp;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10700")
	}
	catch(UCLIDException& ue)
	{
		UCLIDException uex(strELITraceCode, "Unable to read data.", ue);
		uex.addDebugInfo("Initial String", strEncryptedDT, true);
		uex.log();

		return false;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool TimeRollbackPreventer::encryptDateTime(CTime tmTime, const ByteStream& bsPassword, 
											string &strEncryptedDT, const string& strELITraceCode)
{
	try
	{
		try
		{
			// Write the CTime to a byte stream
			ByteStream unencryptedByteStream;
			ByteStreamManipulator bsm( ByteStreamManipulator::kWrite, unencryptedByteStream );

			// Add first random unsigned short with specific modulus
			unsigned long val = m_random.uniform(gulMODULO_CONSTANT + 1, ULONG_MAX);
			unsigned long mod = val % gulMODULO_CONSTANT;
			bsm << val;
			bsm << mod;

			// Add the time
			bsm << tmTime;

			// Add second random unsigned short with specific modulus
			val = m_random.uniform(gulMODULO_CONSTANT + 1, ULONG_MAX);
			mod = val % gulMODULO_CONSTANT;
			bsm << val;
			bsm << mod;

			// Ensure 8-byte boundary for encryption
			bsm.flushToByteStream( 8 );

			// Encrypt the byte stream
			ByteStream encryptedByteStream;
			EncryptionEngine ee;
			ee.encrypt( encryptedByteStream, unencryptedByteStream, bsPassword );

			// Convert the encrypted stream of bytes to an upper-case string
			strEncryptedDT = encryptedByteStream.asString();
			makeUpperCase( strEncryptedDT );
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10696")
	}
	catch(UCLIDException& ue)
	{
		UCLIDException uex(strELITraceCode, "Unable to write data.", ue);
		uex.log();

		return false;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool TimeRollbackPreventer::evaluateUnlockCode(string strCode)
{
	bool bReturn = true;

	try
	{
		try
		{
			string			strUserComputerName;
			unsigned long	ulUserSerialNumber;
			string			strUserMACAddress;
			CTime			tmExpires;
			ByteStream		decryptedBS;

			// Remove any whitespace from the unlock code (P13 #4390)
			replaceVariable( strCode, " ", "" );
			replaceVariable( strCode, "\t", "" );
			replaceVariable( strCode, "\r", "" );
			replaceVariable( strCode, "\n", "" );

			// Create bytestream from the trimmed unlock code
			ByteStream		bytes( strCode );

			//////////////////////
			// Decrypt unlock code
			//////////////////////
			//LMData	Data;
			EncryptionEngine ee;
			ee.decrypt( decryptedBS, bytes, getUnlockPassword());

			ByteStreamManipulator bsm( ByteStreamManipulator::kRead, decryptedBS );

			// Extract User computer name
			bsm >> strUserComputerName;

			// Extract User disk serial number
			bsm >> ulUserSerialNumber;

			// Extract User MAC address
			bsm >> strUserMACAddress;

			// Extract Expiration Date
			bsm >> tmExpires;

			/////////////////////
			// Compare user items
			/////////////////////
			bool	bComparisonOK = true;
			if (strUserComputerName != getComputerName())
			{
				// Create and log UCLID Exception
				UCLIDException ex("ELI18120", "Computer name does not match for Unlock file!");
				ex.addDebugInfo( "Actual Item", getComputerName(), true );
				ex.addDebugInfo( "Unlock Item", strUserComputerName, true );
				ex.log();

				bComparisonOK = false;
			}

			if (ulUserSerialNumber != getDiskSerialNumber())
			{
				// Create and log UCLID Exception
				UCLIDException ex("ELI18121", "Serial number does not match for Unlock file!");
				ex.addDebugInfo( "Actual Item", getDiskSerialNumber(), true );
				ex.addDebugInfo( "Unlock Item", ulUserSerialNumber, true );
				ex.log();

				bComparisonOK = false;
			}

			if (strUserMACAddress != getMACAddress())
			{
				// Create and log UCLID Exception
				UCLIDException ex("ELI18122", "MAC address does not match for Unlock file!");
				ex.addDebugInfo( "Actual Item", getMACAddress(), true );
				ex.addDebugInfo( "Unlock Item", strUserMACAddress, true );
				ex.log();

				bComparisonOK = false;
			}

			////////////////////////
			// Check expiration date
			////////////////////////

			// Get current date
			CTime	tmNow = CTime::GetCurrentTime();
			if (tmExpires < tmNow)
			{
				// Format the time values
				CString zNow = tmNow.Format( "%m/%d/%Y %H:%M:%S" );
				CString zExp = tmExpires.Format( "%m/%d/%Y %H:%M:%S" );

				// Create and log UCLID Exception
				UCLIDException ex("ELI18123", "Unlock file has expired!");
				ex.addDebugInfo( "Actual Item", (LPCTSTR) zNow, true );
				ex.addDebugInfo( "Unlock Item", (LPCTSTR) zExp, true );
				ex.log();

				bComparisonOK = false;
			}

			// Final results
			bReturn = bComparisonOK;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10697")
	}
	catch(UCLIDException ue)
	{
		// Make sure that the exception is logged
		ue.log();
		bReturn = false;
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
string TimeRollbackPreventer::getDateTimeFilePath() const
{

	// Need a try catch so that a bad state can be signaled if an exception is thrown
	try
	{
		try
		{
			string strDTFile;

			// Get path to special user-specific folder
			getSpecialFolderPath(CSIDL_APPDATA, strDTFile);

			// Add the extra path
			strDTFile += gstrDateTimeSubfolderFile;

			return strDTFile;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10708")
	}
	catch(UCLIDException& uex)
	{
		// Ensure the exception is logged and signal bad state
		uex.log();

		throw uex;
	}
}
//-------------------------------------------------------------------------------------------------
string TimeRollbackPreventer::getLocalDateTimeString(const string& strDTFile) const
{
	// This try catch is just to give more trace information
	try
	{
		string strData("");

		if (isFileOrFolderValid( strDTFile ))
		{
			// Read the string
			strData = getTextFileContentsAsString( strDTFile );
		}

		return strData;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10702")
}
//-------------------------------------------------------------------------------------------------
ByteStream TimeRollbackPreventer::getPassword1() const
{

	// This try catch is just to give more trace information
	try
	{
		ByteStream passwordBytes1;

		// Create a 16 byte password from LMData constants
		passwordBytes1.setSize( 16 );
		unsigned char* pData = passwordBytes1.getData();
		pData[0]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey1)));
		pData[1]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey1)));
		pData[2]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey1)));
		pData[3]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey1)));

		pData[4]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey2)));
		pData[5]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey2)));
		pData[6]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey2)));
		pData[7]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey2)));

		pData[8]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey3)));
		pData[9]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey3)));
		pData[10]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey3)));
		pData[11]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey3)));

		pData[12]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey4)));
		pData[13]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey4)));
		pData[14]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey4)));
		pData[15]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey4)));

		return passwordBytes1;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10706");
}
//--------------------------------------------------------------------------------------------------
ByteStream TimeRollbackPreventer::getPassword2() const
{
	// This try catch is just to give more trace information
	try
	{
		ByteStream passwordBytes2;

		// Create a 16 byte password from LMData constants
		passwordBytes2.setSize( 16 );
		unsigned char* pData = passwordBytes2.getData();
		pData[0]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey5)));
		pData[1]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey5)));
		pData[2]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey5)));
		pData[3]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey5)));

		pData[4]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey6)));
		pData[5]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey6)));
		pData[6]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey6)));
		pData[7]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey6)));

		pData[8]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey7)));
		pData[9]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey7)));
		pData[10]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey7)));
		pData[11]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey7)));

		pData[12]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey8)));
		pData[13]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey8)));
		pData[14]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey8)));
		pData[15]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey8)));

		return passwordBytes2;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10707")
}
//-------------------------------------------------------------------------------------------------
string TimeRollbackPreventer::getRemoteDateTimeString(const string& strPath, const string& strKey) const
{
	// This try catch is just to give more trace information
	try
	{
		if (!ma_pRollbackCfgMgr->keyExists( strPath, strKey ))
		{
			ma_pRollbackCfgMgr->createKey( strPath, strKey, "" );
			return "";
		}

		return ma_pRollbackCfgMgr->getKeyValue( strPath, strKey );
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10703")
}
//-------------------------------------------------------------------------------------------------
void TimeRollbackPreventer::handleUnlockCode()
{
	// Get fully qualified path to unlock file
	string strUnlockFile = getExtractLicenseFilesPath() + gpszDateTimeUnlockFile;

	// Find the file
	if (isFileOrFolderValid( strUnlockFile ))
	{
		// Mutex around unlock file access
		CSingleLock lg(getUnlockFileMutex(), TRUE);

		// Ensure the unlock file is still there
		if (!isFileOrFolderValid(strUnlockFile))
		{
			return;
		}

		// Read the string
		string strUnlockCode = getTextFileContentsAsString( strUnlockFile );

		// Continue only if a code was read
		if (!strUnlockCode.empty())
		{
			// Unroll the character swapping in the unlock code (the swapping takes place
			// in the encryptUnlockStream)
			swapUnlockCodeChars(strUnlockCode);

			// Get count of previous unlock codes from registry
			long lUpdateCount = 0;
			if (ma_pRollbackCfgMgr->keyExists( COUNT_SECTION_NAME, COUNT ))
			{
				// Retrieve string
				string	strCount = ma_pRollbackCfgMgr->getKeyValue( 
					COUNT_SECTION_NAME, COUNT );

				// Convert to value
				if (strCount.length() > 0)
				{
					lUpdateCount = asLong( strCount );
				}
			}

			// Examine each previously used unlock code
			bool bFoundPreviouslyUsedCode = false;
			for (int i = 0; i < lUpdateCount; i++)
			{
				// Retrieve this previous unlock code
				string	strPreviousCode;
				if (ma_pRollbackCfgMgr->keyExists( UNLOCK_SECTION_NAME, 
					asString( i + 1 ) ))
				{
					strPreviousCode = ma_pRollbackCfgMgr->getKeyValue( 
						UNLOCK_SECTION_NAME, asString( i + 1 ) );
				}

				// Compare codes
				if (strPreviousCode == strUnlockCode)
				{
					// Set flag
					bFoundPreviouslyUsedCode = true;
					break;
				}
			}	// end for each previous unlock code

			// Continue if this is a new unlock code
			if (!bFoundPreviouslyUsedCode)
			{
				// Test Unlock code
				if (evaluateUnlockCode( strUnlockCode ))
				{
					// Update collection of Unlock codes in registry
					ma_pRollbackCfgMgr->createKey( UNLOCK_SECTION_NAME, 
						asString( lUpdateCount + 1 ), strUnlockCode );

					// Update Unlock code Count item in registry
					ma_pRollbackCfgMgr->setKeyValue( COUNT_SECTION_NAME, COUNT, 
						asString( lUpdateCount + 1 ) );

					// Get current time
					CTime	tmNow = CTime::GetCurrentTime();

					// Create encrypted Date-Time strings for file and registry
					string	strDTLocal;
					string	strDTRemote;
					bool	bLocal = encryptDateTime( tmNow, getPassword1(), strDTLocal, "ELI32504" );
					bool	bRemote = encryptDateTime( tmNow, getPassword2(), strDTRemote, "ELI32505" );

					// Store encrypted strings
					if (bLocal && bRemote)
					{
						// Protect the write accesses
						CSingleLock lg( getReadWriteMutex() );
						if ( lg.Lock(m_ulRWTimeout) == 0 )
						{
							// unable to lock the mutex:
							// log exception, signal bad state and throw exception
							UCLIDException ue( "ELI12997", gpszLicenseIsCorrupt );
							ue.log();
							throw ue;
						}
						writeDateTime(strDTLocal, strDTRemote, true);

						// Since the unlock codes where valid and the time has been reset,
						// if the trp thread is running, reset it
						if (m_apThread.get() != __nullptr)
						{
							// Just log any exceptions that occur attempting to restart
							// the thread, the user can close the app and reopen to run
							// again
							try
							{
								endTrpThread();
								startTrpThread();
							}
							CATCH_AND_LOG_ALL_EXCEPTIONS("ELI32532");
						}

					}	// end if successful Date-Time strings creation
					else if (!bLocal && !bRemote)
					{
						UCLIDException ue( "ELI12741", "Both local and remote failure." );
						throw ue;
					}	// end else both encryptions failed
					else if (!bLocal)
					{
						UCLIDException ue( "ELI12742", "Local failure." );
						throw ue;
					}	// end else local encryption failed
					else
					{
						UCLIDException ue( "ELI12743", "Remote failure." );
						throw ue;
					}	// end else remote encryption failed
				}		// end if evaluateUnlockCode()
				else
				{
					// Unlock code not valid
					UCLIDException ue( "ELI12747", "Unlock code is not valid." );
					throw ue;
				}
			}			// end if new Unlock code
			else
			{
				// Unlock code previously used
				UCLIDException ue( "ELI12746", "Unlock file has been used previously." );
				throw ue;
			}

			// Delete the Unlock file
			try
			{
				deleteFile(strUnlockFile, true);
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25127");
		}				// end if strUnlockCode is not empty
		else
		{
			// Unlock file not found
			UCLIDException ue( "ELI12745", "Empty string from unlock file." );
			ue.addDebugInfo( "Path", strUnlockFile );
			throw ue;
		}
	}					// end if strUnlockFile is valid
}
//--------------------------------------------------------------------------------------------------
void TimeRollbackPreventer::swapUnlockCodeChars(string& rstrCode)
{
	size_t length = rstrCode.length() - 1;
	for (size_t i=0; i < length; i += 2)
	{
		char temp = rstrCode[i];
		rstrCode[i] = rstrCode[i+1];
		rstrCode[i+1] = temp;
	}
}
//--------------------------------------------------------------------------------------------------
bool TimeRollbackPreventer::putLocalDateTimeString(const string& strDTFile,
	const string& strEncrypted, bool bForceCreation)
{
	bool	bResult = true;

	try
	{
		try
		{
			CStdioFile fileDT2;
			CFileException	e;

			// Set file flags to be either
			//   just write access OR
			//   create + write access
			UINT uiFlags = CFile::modeWrite;
			if (bForceCreation)
			{
				// Check for file existence
				if (isFileOrFolderValid( strDTFile ))
				{
					// Remove Hidden attribute
					if (!SetFileAttributes( strDTFile.c_str(), FILE_ATTRIBUTE_NORMAL ))
					{
						// Removing Hidden attribute failed
						bResult = false;
					}
				}

				// Create the file if not already present
				uiFlags |= CFile::modeCreate;

				// Make sure that folder exists
				createDirectory( getDirectoryFromFullPath( strDTFile ) );
			}

			// Create the File Date Time Restorer object
			FileDateTimeRestorer fdtr( strDTFile.c_str() );

			// Safely open the file
			if (!fileDT2.Open( strDTFile.c_str(), uiFlags, &e ))
			{
				// Open and/or creation failed
				bResult = false;
			}
			else
			{
				// Write the string to the file
				fileDT2.WriteString( strEncrypted.c_str() );
				fileDT2.Close();
				waitForFileToBeReadable(strDTFile);

				// Set Hidden attribute
				if (!SetFileAttributes( strDTFile.c_str(), FILE_ATTRIBUTE_HIDDEN ))
				{
					// Setting Hidden attribute failed
					bResult = false;
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10698")
	}
	catch(UCLIDException ue)
	{
		// Make sure that the exception gets logged
		ue.log();
		bResult = false;
	}

	return bResult;
}
//--------------------------------------------------------------------------------------------------
bool TimeRollbackPreventer::putRemoteDateTimeString(const string& strPath, const string& strKey,
	const string& strEncrypted, bool bForceCreation)
{
	bool	bResult = true;

	try
	{
		try
		{
			// Check for key existence
			if (!ma_pRollbackCfgMgr->keyExists( strPath, strKey ))
			{
				// Key does not exist, shall the key be created
				if (bForceCreation)
				{
					// Create the key
					ma_pRollbackCfgMgr->createKey( strPath, strKey, strEncrypted );
				}
				else
				{
					// Do not create the key, just return false
					bResult = false;
				}
			}
			else
			{
				// Store the setting
				ma_pRollbackCfgMgr->setKeyValue( strPath, strKey, strEncrypted );
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10699")
	}
	catch(UCLIDException ue)
	{
		// Make sure that the exception gets logged
		ue.log();
		bResult = false;
	}

	return bResult;
}
//--------------------------------------------------------------------------------------------------
void TimeRollbackPreventer::deleteLocalDateTimeString(const string& strFileName) const
{
	try
	{
		CFile::Remove(strFileName.c_str());
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32455");
}
//--------------------------------------------------------------------------------------------------
void TimeRollbackPreventer::deleteRemoteDateTimeString(const string& strPath) const
{
	try
	{
		ma_pRollbackCfgMgr->deleteFolder(strPath);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32454");
}
//--------------------------------------------------------------------------------------------------
void TimeRollbackPreventer::updateDateTimeItems(bool bForceCreation)
{
	// Get current time
	CTime currentTime = CTime::GetCurrentTime();
	if (!bForceCreation && m_tmLastUpdate > -1)
	{
		// Compute timespan
		CTimeSpan spanTime = currentTime - m_tmLastUpdate;
		CTimeSpan spanMinimum( 0, 0, 0, 0 );

		// Make sure that time has not rolled back or stopped between updates
		if (spanTime < spanMinimum)
		{
			// Create and log exception
			UCLIDException ue( "ELI07537", gpszLicenseIsCorrupt );
			ue.addDebugInfo( "Span", (long)spanTime.GetTotalSeconds() );
			ue.log();

			throw ue;
		}
	}
	
	// Encrypt current time for local registry item
	string	strDTLocalEncrypted;
	bool bLocal = encryptDateTime( currentTime, getPassword1(), strDTLocalEncrypted, "ELI32506" );

	// Encrypt current time for remote registry item
	string	strDTRemoteEncrypted;
	bool bRemote = encryptDateTime( currentTime, getPassword2(), strDTRemoteEncrypted, "ELI32507" );

	if (bLocal && bRemote)
	{
		// Protect the write accesses
		CSingleLock lg( getReadWriteMutex() );
		if ( lg.Lock(m_ulRWTimeout) == 0)
		{
			// Unable to lock the mutex, signal bad state and throw exception
			UCLIDException ue( "ELI12998", gpszLicenseIsCorrupt );
			ue.log();
			throw ue;
		}
		// Write local and remote Date-Time strings
		writeDateTime(strDTLocalEncrypted, strDTRemoteEncrypted, bForceCreation);
	}

	// Check results
	if (!bLocal)
	{
		// Create and log exception
		UCLIDException ue( "ELI07445", gpszLicenseIsCorrupt );
		ue.log();

		// Signal Bad state and throw the exception
		throw ue;
	}
	else if (!bRemote)
	{
		// Create and log exception
		UCLIDException ue( "ELI07446", gpszLicenseIsCorrupt );
		ue.log();

		// Signal Bad state and throw the exception
		throw ue;
	}

	// Just use current time
	m_tmLastUpdate = currentTime;
}
//-------------------------------------------------------------------------------------------------
void TimeRollbackPreventer::writeDateTime(const string& strLocal, const string& strRemote, bool bForceCreation)
{
	// Get the paths to the two files
	string strOutFile1 = getDateTimeFilePath();
	string strOutFile2 = strOutFile1 + ".old";
	
	// Write to the backup locations
	putLocalDateTimeString(strOutFile2, strLocal, true);
	putRemoteDateTimeString(ITEM_SECTION_NAME2, LAST_TIME_USED,	strRemote, true);

	// Write to the new locations
	putLocalDateTimeString(strOutFile1, strLocal, bForceCreation);
	putRemoteDateTimeString(ITEM_SECTION_NAME1, LAST_TIME_USED,	strRemote, bForceCreation);

	// delete the backup locations
	try
	{
		deleteLocalDateTimeString(strOutFile2);
		deleteRemoteDateTimeString(ITEM_SECTION_NAME2);
	}
	catch(...)
	{
	}
}
//-------------------------------------------------------------------------------------------------
string TimeRollbackPreventer::encryptUnlockStream(const ByteStream& bytes)
{
	try
	{
		ByteStream			encryptedByteStream;
		EncryptionEngine	ee;
		ee.encrypt( encryptedByteStream, bytes, getUnlockPassword() );

		// Convert the encrypted stream of bytes to a string
		string strResult = encryptedByteStream.asString();
		makeUpperCase( strResult );

		// Manipulate the letters in the string
		swapUnlockCodeChars(strResult);

		return strResult;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32446");
}
//-------------------------------------------------------------------------------------------------
bool TimeRollbackPreventer::getIdentityDataFromUnlockStream(string strCode,
	string& rstrUserComputerName, unsigned long& rulUserSerialNumber,
	string& rstrUserMACAddress, CTime& rtmExpires)
{
	try
	{
		swapUnlockCodeChars(strCode);
		ByteStream bytes(strCode);

		EncryptionEngine ee;
		ByteStream decryptedBS;
		ee.decrypt( decryptedBS, bytes, TimeRollbackPreventer::getUnlockPassword() );

		ByteStreamManipulator bsm( ByteStreamManipulator::kRead, decryptedBS );

		// Extract User computer name
		string strUserComputerName;
		bsm >> strUserComputerName;

		// Extract User disk serial number
		unsigned long ulUserSerialNumber;
		bsm >> ulUserSerialNumber;

		// Extract User MAC address
		string strUserMACAddress;
		bsm >> strUserMACAddress;

		// Extract Expiration Date
		CTime tmExpires;
		bsm >> tmExpires;

		// Only copy back the data if all was successful
		rstrUserComputerName = strUserComputerName;
		rulUserSerialNumber = ulUserSerialNumber;
		rstrUserMACAddress = strUserMACAddress;
		rtmExpires = tmExpires;

		return true;
	}
	catch(...)
	{
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
ByteStream TimeRollbackPreventer::getUnlockPassword()
{
	try
	{
		ByteStream passwordBytes3;

		// Create a 16 byte password from LMData constants
		passwordBytes3.setSize( 16 );
		unsigned char* pData = passwordBytes3.getData();
		pData[0]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKey9)));
		pData[1]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKey9)));
		pData[2]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKey9)));
		pData[3]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKey9)));

		pData[4]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKeyA)));
		pData[5]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKeyA)));
		pData[6]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKeyA)));
		pData[7]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKeyA)));

		pData[8]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKeyB)));
		pData[9]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKeyB)));
		pData[10]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKeyB)));
		pData[11]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKeyB)));

		pData[12]  = (unsigned char)(LOBYTE(LOWORD(gulUCLIDDateTimeKeyC)));
		pData[13]  = (unsigned char)(HIBYTE(LOWORD(gulUCLIDDateTimeKeyC)));
		pData[14]  = (unsigned char)(LOBYTE(HIWORD(gulUCLIDDateTimeKeyC)));
		pData[15]  = (unsigned char)(HIBYTE(HIWORD(gulUCLIDDateTimeKeyC)));

		return passwordBytes3;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32447");
}
//-------------------------------------------------------------------------------------------------
CMutex* TimeRollbackPreventer::getReadWriteMutex()
{
	if (m_upmutexReadWrite.get() == __nullptr)
	{
		m_upmutexReadWrite.reset(getGlobalNamedMutex(
			"Global\\UCLID_Licensing_F01E2C82-5091-4C36-905D-0C219C89CA47"));
		ASSERT_RESOURCE_ALLOCATION("ELI29993", m_upmutexReadWrite.get() != __nullptr);
	}

	return m_upmutexReadWrite.get();
}
//-------------------------------------------------------------------------------------------------
CMutex* TimeRollbackPreventer::getUnlockFileMutex()
{
	if (m_upmutexUnlock.get() == __nullptr)
	{
		m_upmutexUnlock.reset(getGlobalNamedMutex(
			"Global\\UCLID_Unlock_BDDD1127-65B9-450C-BE70-21D10D4C7B1E"));
		ASSERT_RESOURCE_ALLOCATION("ELI32444", m_upmutexUnlock.get() != __nullptr);
	}

	return m_upmutexUnlock.get();
}
//-------------------------------------------------------------------------------------------------
void TimeRollbackPreventer::startTrpThread()
{
	// This try catch is just to give more trace information
	try
	{
		// Ensure the thread is stopped first
		endTrpThread();

		m_eventKillThread.reset();

		// Start the thread that handles updates
		m_apThread.reset(AfxBeginThread(TRPThreadProc, this, THREAD_PRIORITY_NORMAL, 0, CREATE_SUSPENDED));
		m_apThread->m_bAutoDelete = FALSE;
		m_apThread->ResumeThread();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10711");
}
//-------------------------------------------------------------------------------------------------
void TimeRollbackPreventer::endTrpThread()
{
	try
	{
		// Kill the thread if it exists
		if (m_apThread.get() != __nullptr)
		{
			m_eventKillThread.signal();

			DWORD dwRet = WaitForSingleObject(m_apThread->m_hThread, 2000);
			if(dwRet == WAIT_TIMEOUT)
			{
				UCLIDException ue("ELI10940", "Licensing system unable to shutdown correctly.");
				throw ue;
			}

			// delete the thread
			m_apThread.reset();
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32531");
}
//-------------------------------------------------------------------------------------------------
