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
#include "RegistryPersistenceMgr.h"
#include "EncryptionEngine.h"
#include "FileDateTimeRestorer.h"
#include "UCLIDException.h"
#include "ExtractMFCUtils.h"
#include "cpputil.h"
#include "RegConstants.h"
#include "MutexUtils.h"

#include <shlobj.h>
#include <iostream>

extern AFX_EXTENSION_MODULE BaseUtilsDLL;

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
const string TimeRollbackPreventer::ITEM_SECTION_NAME1 = "Identities\\{7FEF3749-A8CC-4CD0-9CEB-E6D267FA524E}";
const string TimeRollbackPreventer::ITEM_SECTION_NAME2 = "Identities\\{526988F0-27BE-4451-B741-D8614827B838}";

const string TimeRollbackPreventer::COUNT_SECTION_NAME = "Software\\Windows";
const string TimeRollbackPreventer::UNLOCK_SECTION_NAME = "Software\\Classes\\Code";

// Registry keys
const string TimeRollbackPreventer::LAST_TIME_USED = "LTUSWU";
const string TimeRollbackPreventer::COUNT = "Count";

// Define the Windows subfolder and filename for Date-Time encryption
//   i.e. full path will be $(System) + gpszDateTimeSubfolderFile
//   NOTE: This file is out-of-date as of 07/20/2004 P13 #3000
//const char gpszDateTimeSubfolderFile[] = 
//	"\\spool\\prtprocs\\w32x86\\tlsuuw.dll";

// Define the user-specific subfolder and filename for Date-Time encryption
//   i.e. full path will be $(Documents and Settings\Username\Application Data) + gpszDateTimeSubfolderFile
const char gpszDateTimeSubfolderFile[] = "\\Windows\\tlsuuw_DO_NOT_DELETE.dll";

// Polling interval for Date-Time updates
const unsigned long gulTRP_TIMER_MS = 30000;

// Modulo constant for random additions to DT strings
const unsigned short gusMODULO_CONSTANT = 17;

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
	bool bExceptionCaught = false;
	try
	{
		// make sure the pParam is not null
		ASSERT_ARGUMENT( "ELI12995", pParam != NULL );
		TimeRollbackPreventer *trpInstance = (TimeRollbackPreventer *) pParam;

		try
		{
			try
			{
				// Continuous updates
				while (trpInstance->m_eventKillThread.wait( gulTRP_TIMER_MS ) == WAIT_TIMEOUT)
				{
					// Update the Date-Time items
					trpInstance->updateDateTimeItems();
				}
			}
			catch (...)
			{
				// If any exceptions put licensing in a bad state
				trpInstance->m_rEventBadState.signal();
				throw;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10692");
	}
	catch (UCLIDException& ue)
	{
		bExceptionCaught = true;
		UCLIDException uexOuter("ELI07616", "Error in maintaining licensed state!", ue);
		
		// Force display (and logging) of exception
		uexOuter.display();
	}
	return 0;
}

//-------------------------------------------------------------------------------------------------
// TimeRollbackPreventer
//-------------------------------------------------------------------------------------------------
TimeRollbackPreventer::TimeRollbackPreventer(Win32Event &rEventBadState)
:	m_tmLastUpdate(-1),
	m_rEventBadState(rEventBadState),
	m_apThread(NULL)
{
	try
	{
		try
		{
			// Get Read Write mutex timeout value from the registry
			RegistryPersistenceMgr rpmLocalMachine ( HKEY_LOCAL_MACHINE, "" );
			if ( rpmLocalMachine.keyExists( strCOMLM_CORE_SETTINGS, strTRP_RW_TIMEOUT_KEY ) )
			{
				m_ulRWTimeout = asUnsignedLong( rpmLocalMachine.getKeyValue( strCOMLM_CORE_SETTINGS, strTRP_RW_TIMEOUT_KEY ));
			}
			else
			{
				// Create the key with the default value
				string strDefaultValue = asString ( ulDEFAULT_RW_TIME0UT );
				rpmLocalMachine.createKey( strCOMLM_CORE_SETTINGS, strTRP_RW_TIMEOUT_KEY, strDefaultValue );
				m_ulRWTimeout = ulDEFAULT_RW_TIME0UT;
			}

			// Setup Registry items
			ma_pRollbackCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
				new RegistryPersistenceMgr( HKEY_CURRENT_USER, "" ));

			// This try catch is just to give more trace information
			try
			{
				// Check license items
				checkDateTimeItems();
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10709")

			// This try catch is just to give more trace information
			try
			{
				m_eventKillThread.reset();
				// Start the thread that handles updates
				m_apThread = auto_ptr<CWinThread>(AfxBeginThread(TRPThreadProc, this, THREAD_PRIORITY_NORMAL, 0, CREATE_SUSPENDED));
				m_apThread.get()->m_bAutoDelete = FALSE;
				m_apThread.get()->ResumeThread();
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10711")
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10693");
	}
	catch(UCLIDException ue)
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
		// Kill the thread if it exists
		if (m_apThread.get())
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

		// Reset the read/write mutex auto pointer
		m_apmutexReadWrite.reset();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16412");
}
//-------------------------------------------------------------------------------------------------
void TimeRollbackPreventer::checkDateTimeItems()
{
	////////////////////////////////////////////
	// Check for system time already rolled back
	////////////////////////////////////////////
	string strDLL = getModuleDirectory( BaseUtilsDLL.hModule );
	strDLL += "\\";
	strDLL += "BaseUtils.dll";
	CTime tmDLL = getFileModificationTimeStamp( strDLL );
	CTime tmNow = CTime::GetCurrentTime();
	if (tmNow < tmDLL)
	{
		// Signal BAD STATE event and throw exception
		// This Exception and ELI code will be seen in UEX log file
		UCLIDException ue( "ELI11549", gpszLicenseIsCorrupt );
		ue.log();
		m_rEventBadState.signal();
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
			m_rEventBadState.signal();
			throw ue;
		}
		strLocalData1 = getLocalDateTimeString(getDateTimeFilePath());
		strRemoteData1 = getRemoteDateTimeString(ITEM_SECTION_NAME1, LAST_TIME_USED);

		strLocalData2 = getLocalDateTimeString(getDateTimeFilePath() + ".old");
		strRemoteData2 = getRemoteDateTimeString(ITEM_SECTION_NAME2, LAST_TIME_USED);
	}

	// Convert the encrypted Date-Time strings into CTime objects
	CTime	tmLocal;
	CTime	tmRemote;
	bool	bLocal = decryptDateTimeString( strLocalData1, getPassword1(), &tmLocal );
	bool	bRemote = decryptDateTimeString( strRemoteData1, getPassword2(), &tmRemote );

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
		bLocal = decryptDateTimeString( strLocalData2, getPassword1(), &tmLocal );
		bRemote = decryptDateTimeString( strRemoteData2, getPassword2(), &tmRemote );

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
		m_rEventBadState.signal();
		throw ue;
	}
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool TimeRollbackPreventer::decryptDateTimeString(std::string strEncryptedDT, 
												  const ByteStream& bsPassword, 
												  CTime* ptmResult)
{
	bool bReturn = true;

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

			// Get first random unsigned short
			unsigned short usTemp1;
			bsm >> usTemp1;

			// Confirm divisibility by modulo constant
			unsigned short usExtra = usTemp1 % gusMODULO_CONSTANT;
			if (usExtra != 0)
			{
				bReturn = false;
			}

			// Retrieve CTime data
			CTime	tmTemp;
			bsm >> tmTemp;

			// Get second random unsigned short
			unsigned short usTemp2;
			bsm >> usTemp2;

			// Confirm divisibility by modulo constant
			usExtra = usTemp2 % gusMODULO_CONSTANT;
			if (usExtra != 0)
			{
				bReturn = false;
			}

			// Provide CTime to caller
			*ptmResult = tmTemp;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10700")
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
bool TimeRollbackPreventer::encryptDateTime(CTime tmTime, const ByteStream& bsPassword, 
											std::string &strEncryptedDT)
{
	bool	bResult = true;

	try
	{
		try
		{
			// Write the CTime to a byte stream
			ByteStream unencryptedByteStream;
			ByteStreamManipulator bsm( ByteStreamManipulator::kWrite, unencryptedByteStream );

			// Add first random unsigned short with specific modulus
			srand( (unsigned)time( NULL ) );
			unsigned short usTemp1 = (unsigned short)rand();
			unsigned short usExtra = usTemp1 % gusMODULO_CONSTANT;
			usTemp1 -= usExtra;
			bsm << usTemp1;

			// Add the time
			bsm << tmTime;

			// Add second random unsigned short with specific modulus
			unsigned short usTemp2 = (unsigned short)rand();
			usExtra = usTemp2 % gusMODULO_CONSTANT;
			usTemp2 -= usExtra;
			bsm << usTemp2;

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
	catch(UCLIDException ue)
	{
		// Make sure that the exception is logged
		ue.log();
		bResult = false;
	}

	return bResult;
}
//-------------------------------------------------------------------------------------------------
bool TimeRollbackPreventer::evaluateUnlockCode(std::string strCode)
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
				ex.addDebugInfo( "Actual Item", zNow.operator LPCTSTR(), true );
				ex.addDebugInfo( "Unlock Item", zExp.operator LPCTSTR(), true );
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
std::string TimeRollbackPreventer::getDateTimeFilePath() const
{
	string	strDTFile;

	// This try catch is just to give more trace information
	try
	{
		// Get path to special user-specific folder
		char pszDir[MAX_PATH];
		if (SUCCEEDED (SHGetSpecialFolderPath( NULL, pszDir, CSIDL_APPDATA, 0 ))) 
		{
			// Add path and filename for DT file
			strDTFile = string( pszDir ) + string( gpszDateTimeSubfolderFile );
		}
		else
		{
			// Build and log the exception before signalling a bad state
			UCLIDException ue( "ELI07448", "Unable to get path to Special folder." );
			ue.addDebugInfo( "Last Error", GetLastError() );
			ue.log();

			// Signal a bad state
			m_rEventBadState.signal();
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10708")

	// Return results
	return strDTFile;
}
//-------------------------------------------------------------------------------------------------
std::string TimeRollbackPreventer::getLocalDateTimeString(std::string strFileName) const
{
	string	strData;

	// This try catch is just to give more trace information
	try
	{
		// Get path and filename for DT file
		string	strDTFile = strFileName;
		if (isFileOrFolderValid( strDTFile ))
		{
			// Read the string
			strData = getTextFileContentsAsString( strDTFile );
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10702")

	// Return results
	return strData;
}
//-------------------------------------------------------------------------------------------------
const ByteStream& TimeRollbackPreventer::getPassword1() const
{
	static ByteStream passwordBytes1;
	static bool bAlreadyInitialized1 = false;
	// This try catch is just to give more trace information
	try
	{
		if (!bAlreadyInitialized1)
		{
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

			// Set flag
			bAlreadyInitialized1 = true;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10706")

	return passwordBytes1;
}
//--------------------------------------------------------------------------------------------------
const ByteStream& TimeRollbackPreventer::getPassword2() const
{
	static ByteStream passwordBytes2;
	static bool bAlreadyInitialized2 = false;
	// This try catch is just to give more trace information
	try
	{
		if (!bAlreadyInitialized2)
		{
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

			// Set flag
			bAlreadyInitialized2 = true;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10707")

	return passwordBytes2;
}
//-------------------------------------------------------------------------------------------------
std::string TimeRollbackPreventer::getRemoteDateTimeString(std::string strPath, std::string strKey) const
{
	string	strData;

	// This try catch is just to give more trace information
	try
	{
		if (!ma_pRollbackCfgMgr->keyExists( strPath, strKey ))
		{
			ma_pRollbackCfgMgr->createKey( strPath, strKey, "" );
			return "";
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10703")

	return ma_pRollbackCfgMgr->getKeyValue( strPath, strKey );
}
//-------------------------------------------------------------------------------------------------
void TimeRollbackPreventer::handleUnlockCode()
{
	// Get fully qualified path to unlock file
	string strUnlockFile = ::getModuleDirectory("BaseUtils.dll") + "\\" + gpszDateTimeUnlockFile;

	// Find the file
	if (isFileOrFolderValid( strUnlockFile ))
	{
		// Read the string
		string strUnlockCode = getTextFileContentsAsString( strUnlockFile );

		// Continue only if a code was read
		if (!strUnlockCode.empty())
		{
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
					bool	bLocal = encryptDateTime( tmNow, getPassword1(), strDTLocal );
					bool	bRemote = encryptDateTime( tmNow, getPassword2(), strDTRemote );

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
							m_rEventBadState.signal();
							throw ue;
						}
						writeDateTime(strDTLocal, strDTRemote, true);

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
bool TimeRollbackPreventer::putLocalDateTimeString(std::string strFileName, std::string strEncrypted, bool bForceCreation)
{
	bool	bResult = true;

	try
	{
		try
		{
			// Get path and filename for DT file
			string	strDTFile = strFileName;

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
bool TimeRollbackPreventer::putRemoteDateTimeString(std::string strPath, std::string strKey, std::string strEncrypted, bool bForceCreation)
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
void TimeRollbackPreventer::deleteLocalDateTimeString(std::string strFileName) const
{
	CFile::Remove(strFileName.c_str());
}
//--------------------------------------------------------------------------------------------------
void TimeRollbackPreventer::deleteRemoteDateTimeString(std::string strPath, std::string strKey) const
{
	ma_pRollbackCfgMgr->deleteFolder(strPath);
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

			// Signal a bad state and throw the exception
			m_rEventBadState.signal();
			throw ue;
		}
	}
	
	// Encrypt current time for local registry item
	string	strDTLocalEncrypted;
	bool bLocal = encryptDateTime( currentTime, getPassword1(), strDTLocalEncrypted );

	// Encrypt current time for remote registry item
	string	strDTRemoteEncrypted;
	bool bRemote = encryptDateTime( currentTime, getPassword2(), strDTRemoteEncrypted );

	if (bLocal && bRemote)
	{
		// Protect the write accesses
		CSingleLock lg( getReadWriteMutex() );
		if ( lg.Lock(m_ulRWTimeout) == 0)
		{
			// Unable to lock the mutex, signal bad state and throw exception
			UCLIDException ue( "ELI12998", gpszLicenseIsCorrupt );
			ue.log();
			m_rEventBadState.signal();
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
		m_rEventBadState.signal();
		throw ue;
	}
	else if (!bRemote)
	{
		// Create and log exception
		UCLIDException ue( "ELI07446", gpszLicenseIsCorrupt );
		ue.log();

		// Signal Bad state and throw the exception
		m_rEventBadState.signal();
		throw ue;
	}

	// Just use current time
	m_tmLastUpdate = currentTime;
}
//-------------------------------------------------------------------------------------------------
void TimeRollbackPreventer::writeDateTime(std::string strLocal, std::string strRemote, bool bForceCreation)
{
	// Get the paths to the two files
	string strOutFile1 = getDateTimeFilePath();
	string strOutFile2 = getDateTimeFilePath() + ".old";
	
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
		deleteRemoteDateTimeString(ITEM_SECTION_NAME2, LAST_TIME_USED);
	}
	catch(...)
	{
	}
}
//-------------------------------------------------------------------------------------------------
const ByteStream& TimeRollbackPreventer::getUnlockPassword()
{
	static ByteStream passwordBytes3;
	static bool bAlreadyInitialized3 = false;
	if (!bAlreadyInitialized3)
	{
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

		// Set flag
		bAlreadyInitialized3 = true;
	}

	return passwordBytes3;
}
//-------------------------------------------------------------------------------------------------
CMutex* TimeRollbackPreventer::getReadWriteMutex()
{
	if (m_apmutexReadWrite.get() == NULL)
	{
		m_apmutexReadWrite.reset(getGlobalNamedMutex("Global\\UCLID_Licensing"));
		ASSERT_RESOURCE_ALLOCATION("ELI29993", m_apmutexReadWrite.get() != NULL);
	}

	return m_apmutexReadWrite.get();
}
//-------------------------------------------------------------------------------------------------
