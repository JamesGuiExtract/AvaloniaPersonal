// SafeNetLicenseMgr.cpp: implementation of the SafeNetLicenseMgr class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"

#include "SafeNetLicenseMgr.h"
#include "SafeNetErr.h"
#include "SafeNetLicenseCfg.h"

#include <EncryptionEngine.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <ProgressDlgTaskRunner.h>
#include <cpputil.h>
#include <StringTokenizer.h>
#include <UCLIDException.h>
#include <StopWatch.h>
#include <RegConstants.h>
#include <TimedRetryDlg.h>

#include <fstream>
#include <cmath>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const DWORD gdwHEART_BEAT = 300; // seconds ( 5 min )
const DWORD gdwHEART_BEAT_REFRESH_TIME = gdwHEART_BEAT / 2 * 1000; // Milliseconds
const long gnSLEEP_TIME_BETWEEN_RETRIES = 500;

// Add item to exception log at this counter decrement frequency
const long gnLOG_FREQUENCY = 1000;

// these constants should be not be included in the code
//const SP_WORD  gwDEVELOPER_ID = 0x218E;      /* Developer ID of UltraPro key */
//const SP_WORD  gwWRITEPASSWORD = 0xbdfa;    /* Write Password of UltraPro key */
//const SP_WORD  gwOWPASSWORD1 = 0xdf5f;      /* Overwrite Password 1 of UltraPro key */
//const SP_WORD  gwOWPASSWORD2 = 0x112e;      /* Overwrite Password 2 of UltraPro key */

// Define four UCLID passwords used for encrypted ETF files
// NOTE: These passwords were not exposed at the header file level because
//		 no user of this class needs to know that these passwords exist
const unsigned long	gulUCLIDKey09 = 0x35026831;
const unsigned long	gulUCLIDKey10 = 0x57D74D90;
const unsigned long	gulUCLIDKey11 = 0x57250A5E;
const unsigned long	gulUCLIDKey12 = 0x0FCD7A67;

// Encrypted values for DeveloperID, Write Password, OverWritePassword 1 and 2
const unsigned long gulENCRYPTEDPasswords1 = 0xE9428F39;
const unsigned long gulENCRYPTEDPasswords2 = 0x88526517;

//-------------------------------------------------------------------------------------------------
// Staic Members
//-------------------------------------------------------------------------------------------------
CMutex SafeNetLicenseMgr::ms_mutex;

//-------------------------------------------------------------------------------------------------
// Globals
//-------------------------------------------------------------------------------------------------
DataCell gdcellFlexIndexingCounter(k32BitDataCell, SP_FLEXINDEXINGCOUNTER_INTEGER, SP_FLEXCOUNTERLOCK_INTEGER, "Indexing");
DataCell gdcellFlexPaginationCounter(k32BitDataCell, SP_PAGINATIONCOUNTER_INTEGER, SP_FLEXCOUNTERLOCK_INTEGER, "Pagination");
DataCell gdcellIDShieldRedactionCounter(k32BitDataCell, SP_REDACTIONCOUNTER_INTEGER, SP_FLEXCOUNTERLOCK_INTEGER, "Redaction");
DataCell gdcellCounterIncrementAmount(k32BitDataCell, SP_COUNTERINCAMOUNT_INTEGER, SP_FLEXCOUNTERLOCK_INTEGER, "Counter");
DataCell gdcellCounterToIncrement(k16BitDataCell, SP_COUNTERTOINCREMENT_INTEGER, SP_FLEXCOUNTERLOCK_INTEGER, "Increment");
DataCell gdcellIcoMapUserLimit(k16BitDataCell, SP_ICOMAP_FULLLICENSE_USER_LIMIT, 0, "IcoMap User Limit");
DataCell gdcellFlexIndexUserLimit(k16BitDataCell, SP_FLEXINDEX_FULLLICENSE_USER_LIMIT, 0, "IcoMap User Limit");


USBLicense gusblFlexIndex(SP_FLEXINDEX_FULLLICENSE, SP_FLEXINDEX_FULLLICENSE_USER_LIMIT, 
							(unsigned char *)QUERY_SP_FLEXINDEX_FULLLICENSE, 
							(unsigned char *)RESPONSE_SP_FLEXINDEX_FULLLICENSE, SP_NUM_OF_QR);
USBLicense gusblIcoMap(SP_ICOMAP_FULLLICENSE, SP_ICOMAP_FULLLICENSE_USER_LIMIT, 
							(unsigned char *)QUERY_SP_ICOMAP_FULLLICENSE, 
							(unsigned char *)RESPONSE_SP_ICOMAP_FULLLICENSE, SP_NUM_OF_QR);


// Only used internally
DataCell gdcellSerialNumber(k32BitDataCell, SP_SERIAL_NUMBER_INTEGER, SP_FLEXCOUNTERLOCK_INTEGER, "SerialNumber");

//-------------------------------------------------------------------------------------------------
// SafeNetPasswords
//-------------------------------------------------------------------------------------------------
class SafeNetPasswords
{
public:
	SafeNetPasswords();

	SP_WORD wDeveloperID;
	SP_WORD wWritePassword;
	SP_WORD wOWPassword1;
	SP_WORD wOWPassword2;
};

// Defined instance of SafeNetPasswords
SafeNetPasswords gsnPwds;

//-------------------------------------------------------------------------------------------------
SafeNetPasswords::SafeNetPasswords()
{

	// Setup password byte stream for encryption
	ByteStream passwordBytes;
	ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, passwordBytes);
	
	bsm << gulUCLIDKey09;
	bsm << gulUCLIDKey10;
	bsm << gulUCLIDKey11;
	bsm << gulUCLIDKey12;
	bsm.flushToByteStream( 8 );

	// Setup up byte stream of encrypted safenet passwords
	ByteStream encryptedPasswords;
	ByteStreamManipulator bsmEncrypted(ByteStreamManipulator::kWrite, encryptedPasswords);

	bsmEncrypted << gulENCRYPTEDPasswords1;
	bsmEncrypted << gulENCRYPTEDPasswords2;
	bsmEncrypted.flushToByteStream( 8 );
	
	// Decrypt the safenet passwords
	ByteStream decryptedPasswords;
	EncryptionEngine ee;
	ee.decrypt(decryptedPasswords, encryptedPasswords, passwordBytes );

	ByteStreamManipulator bsmDecrypted(ByteStreamManipulator::kRead, decryptedPasswords);
	bsmDecrypted >> wDeveloperID;
	bsmDecrypted >> wWritePassword;
	bsmDecrypted >> wOWPassword1;
	bsmDecrypted >> wOWPassword2;
}

//-------------------------------------------------------------------------------------------------
// DataCell Class
//-------------------------------------------------------------------------------------------------
DataCell::DataCell( ECellType eCellType, SP_DWORD dwCellAddr, SP_DWORD dwLockCellAddr, string strCellName )
: m_dwCellAddr ( dwCellAddr ),
	m_dwLockCellAddr( dwLockCellAddr ),
	m_eCellType ( eCellType ),
	m_strCellName(strCellName)
{
	
}
//-------------------------------------------------------------------------------------------------
string DataCell::getCellName()
{
	return m_strCellName;
}

//-------------------------------------------------------------------------------------------------
// QueryResponsePair Class
//-------------------------------------------------------------------------------------------------
QueryResponsePair::QueryResponsePair( unsigned char *pczQuery, unsigned char *pczResponse)
{
	ASSERT_ARGUMENT("ELI12136", pczQuery != __nullptr );
	ASSERT_ARGUMENT("ELI12137", pczResponse != __nullptr );
	// Set Query data
	memset(czQuery, 0, SP_LEN_OF_QR + 1);
	memmove(czQuery, pczQuery, SP_LEN_OF_QR );

	// Set Response data
	memset(czResponse, 0, SP_LEN_OF_QR + 1);
	memmove(czResponse, pczResponse, SP_LEN_OF_QR );
}
//-------------------------------------------------------------------------------------------------
bool QueryResponsePair::isValidResponse( unsigned char *pczResponse )
{
	return memcmp( czResponse, pczResponse, SP_LEN_OF_QR) == 0;
}

//-------------------------------------------------------------------------------------------------
// USBLicense Class
//-------------------------------------------------------------------------------------------------
USBLicense::USBLicense( SP_DWORD dwLicenseAddr,  SP_DWORD dwUserLimitAddr, 
			unsigned char *pszQueryArray, unsigned char *pszResponseArray, int iNumQrys )
:	m_dwLicenseAddr ( dwLicenseAddr ),
	m_dwUserLimitAddr (dwUserLimitAddr),
	m_pszQueryArray(pszQueryArray),
	m_pszResponseArray(pszResponseArray ),
	m_iNumQrys(iNumQrys)
{
	ASSERT_ARGUMENT("ELI12138", pszQueryArray != __nullptr );
	ASSERT_ARGUMENT("ELI12139", pszResponseArray != __nullptr );
}
//-------------------------------------------------------------------------------------------------
QueryResponsePair USBLicense::getQRPair( int iQryNumber )
{
	try
	{
		int iQryPos = iQryNumber * SP_LEN_OF_QR;
		QueryResponsePair qrpQR( &m_pszQueryArray[iQryPos], &m_pszResponseArray[iQryPos]);

		return qrpQR;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24876");
}

//-------------------------------------------------------------------------------------------------
// SafeNetLicenseMgr::CellLock
//-------------------------------------------------------------------------------------------------
SafeNetLicenseMgr::CellLock::CellLock(SafeNetLicenseMgr& rsnManager, DataCell & rCell)
:	m_rsnManager ( rsnManager ), 
	m_rCell ( rCell )
{
	if ( rCell.m_dwLockCellAddr == 0 )
	{
		UCLIDException ue("ELI12596", "Cell is not a valid cell for locking." );
		ue.addDebugInfo("Cell #", rCell.m_dwLockCellAddr );
		throw ue;
	}
	lock();
}
//-------------------------------------------------------------------------------------------------
SafeNetLicenseMgr::CellLock::~CellLock()
{
	try
	{
		// Unlock the counter
		unlock();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16587");
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseMgr::CellLock::lock()
{
	try
	{
		StopWatch swTimeOut;
		SP_STATUS spsStatus;

		// Get retry count and wait timeout from the registry
		long nNumberOfRetries = m_rsnManager.m_snlcConfig.getNumberRetries();
		double dRetryWaitTimeout = m_rsnManager.m_snlcConfig.getRetryWaitTime();

		// If unable to lock the key the first pass the license will be released and reaquired
		// to attempt to lock the key again
		for ( long i = 0; i < nNumberOfRetries; i++ )
		{
			swTimeOut.reset();

			// Start a timer
			swTimeOut.start();

			// Initialize elapsed time
			double dElapsedTime = 0.0;

			// Try to lock the cell - each pass through the loop will have released and got another license
			// if the failure was not SP_ERR_ACCESS_DENIED (was locked)
			do
			{
				// Lock the mutex
				CSingleLock slg(&ms_mutex, TRUE );

				// TODO: A Query Response check should be done here to verify that the key is still available
				// Lock the cell
				spsStatus = SFNTsntlLockData( &m_rsnManager.m_Packet, m_rCell.m_dwLockCellAddr , gsnPwds.wWritePassword );

				// If lock was success or not Access Denied keep retring
				if (spsStatus == SP_ERR_SUCCESS)
				{
					break;
				}
				else if  (spsStatus != SP_ERR_ACCESS_DENIED) 
				{
					// Log exception to indicate lock failed
					UCLIDException ue("ELI18226", "Unable to lock counter.");
					loadSafeNetErrInfo(ue, spsStatus);
					ue.log();
					
					// Unlock the mutex before calling releaseLicense and getLicense
					slg.Unlock();

					// Release the current license
					m_rsnManager.releaseLicense();

					// Call to get license to reconnect if the connection to the key has been lost
					m_rsnManager.getLicense();
				}
				dElapsedTime = swTimeOut.getElapsedTime();
			}
			while (dElapsedTime > 0.0 && dElapsedTime < dRetryWaitTimeout);

			// stop the timer
			swTimeOut.stop();

			// if lock call was successful exit loop
			if (spsStatus == SP_ERR_SUCCESS)
			{
				break;
			}

			// if the failure was ACCESS_DENIED the counter was locked so reset it before trying again
			if (spsStatus == SP_ERR_ACCESS_DENIED)
			{
				unlock();
				UCLIDException ue("ELI18223", "Lock operation timed out! Lock has been reset.");
				ue.addDebugInfo("Retry #", i);
				ue.log();
			}
		}

		// if error throw an exception
		if ( spsStatus != SP_ERR_SUCCESS )
		{
			UCLIDException ue("ELI11308", "Unable to lock counter for update.");
			loadSafeNetErrInfo(ue, spsStatus);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24877");
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseMgr::CellLock::unlock()
{
	try
	{
		// Validate heartbeat. This will also attempt to get a new license if license is invalid
		m_rsnManager.validateHeartbeatActive();

		// return status for the call to unlock the cell
		SP_STATUS spsStatus;

		// Lock mutex
		CSingleLock slg(&ms_mutex, TRUE );

		spsStatus = SFNTsntlUnlockData( &m_rsnManager.m_Packet, m_rCell.m_dwLockCellAddr, 
			gsnPwds.wWritePassword, gsnPwds.wOWPassword1, gsnPwds.wOWPassword2 );

		// if an error throw an exceptions
		if ( spsStatus != SP_ERR_SUCCESS )
		{
			UCLIDException ue("ELI11309", "Unable to unlock counter after update.");
			loadSafeNetErrInfo(ue, spsStatus);
			throw ue;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18189");
}

//-------------------------------------------------------------------------------------------------
// SafeNetLicenseMgr::ResetLockTask
//-------------------------------------------------------------------------------------------------
SafeNetLicenseMgr::ResetLockTask::ResetLockTask(SP_UPRO_APIPACKET &rPacket, DataCell & rCell, double dNumSecToWait)
:	m_rPacket ( rPacket ),
	m_rCell ( rCell ),
	m_dNumSecToWait( dNumSecToWait )
{
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseMgr::ResetLockTask::runTask(IProgress *pProgress, int nTaskID  )
{
	try
	{
		StopWatch swTimeOut;
		SP_STATUS spsStatus;
		pProgress->setText("Remaining Time" );
		pProgress->setTitle("Waiting for lock");
		swTimeOut.start();
		double dElapsedTime;
		do
		{
			CSingleLock slg(&ms_mutex, TRUE );

			// TODO: A Query Response check should be done here to verify that the key is still available
			spsStatus = SFNTsntlLockData( &m_rPacket, m_rCell.m_dwLockCellAddr , gsnPwds.wWritePassword );

			// If lock was success or not Access Denied keep retring
			if ( spsStatus == SP_ERR_SUCCESS || spsStatus != SP_ERR_ACCESS_DENIED )
			{
				break;
			}
			dElapsedTime = swTimeOut.getElapsedTime();
			if ( m_dNumSecToWait > 0 )
			{
				double fPercent = dElapsedTime / m_dNumSecToWait * (double)100;
				pProgress->setPercentComplete( fPercent );
			}
		}
		while ( spsStatus != SP_ERR_SUCCESS && dElapsedTime > 0.0 && dElapsedTime < m_dNumSecToWait );
		swTimeOut.stop();

		// Unlock the data cell
		CSingleLock slg(&ms_mutex, TRUE );

		spsStatus = SFNTsntlUnlockData( &m_rPacket, m_rCell.m_dwLockCellAddr, gsnPwds.wWritePassword, gsnPwds.wOWPassword1,gsnPwds.wOWPassword2 );

		if ( spsStatus != SP_ERR_SUCCESS )
		{
			UCLIDException ue("ELI19503", "Unable to reset counter lock.");
			loadSafeNetErrInfo(ue, spsStatus);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24878");
}

//-------------------------------------------------------------------------------------------------
// HeartbeatThreadData class
//-------------------------------------------------------------------------------------------------
SafeNetLicenseMgr::HeartbeatThreadData::HeartbeatThreadData	(SP_UPRO_APIPACKET &rPacket)
: m_bException(false), m_rPacket(rPacket), m_pThread(NULL), m_psnlmMgr(NULL), m_hThreadHandle(NULL)
{
}
//-------------------------------------------------------------------------------------------------
SafeNetLicenseMgr::HeartbeatThreadData::~HeartbeatThreadData()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16588");
}

//-------------------------------------------------------------------------------------------------
// SafeNetLicenseMgr Class
//-------------------------------------------------------------------------------------------------
SafeNetLicenseMgr::SafeNetLicenseMgr( USBLicense & rusblLicense, bool bObtainLicense, bool bNoLicenseRetry)
: 	m_bHasLicense(false),
	m_htdData(m_Packet),
	m_dwSN(0),
	m_rusblLicense(rusblLicense),
	m_ipEmailSettings(__nullptr),
	m_ipMessage(__nullptr),
	m_bLicenseRetry(bNoLicenseRetry),
	m_bLicenseHasBeenObtained(false)
{
	if ( bObtainLicense )
	{
		getLicense();
	}
}
//-------------------------------------------------------------------------------------------------
UINT heartbeatThreadProc(void *pData)
{
	SafeNetLicenseMgr::HeartbeatThreadData *htd = (SafeNetLicenseMgr::HeartbeatThreadData *)pData;

	// Verify the thread data was passed.
	if (htd == __nullptr)
	{
		// Log an exception to indicate that thread data was null.
		UCLIDException ue("ELI24879", "Heartbeat thread data was NULL.");
		ue.log();

		// Return non zero value to indicate an error.
		return 1;
	}
	
	try
	{
		try
		{
			// Signal that the thread was started.
			htd->m_threadStartedEvent.signal();
			htd->m_bException = false;

			while ( htd->m_threadStopRequest.wait(gdwHEART_BEAT_REFRESH_TIME) == WAIT_TIMEOUT )
			{
				SafeNetLicenseMgr* pLicenseMgr = htd->m_psnlmMgr;
				if (pLicenseMgr == __nullptr)
				{
					UCLIDException ue("ELI11557", "No USB Key License Manager.");
					throw ue;
				}
				else
				{
					// Validate the license -- this makes a call to the key to keep it from timing out
					pLicenseMgr->validateUSBLicense();
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI12085");
	}
	catch (UCLIDException ue )
	{
		ue.log();
		htd->m_ue = ue;
		htd->m_bException = true;
	}

	htd->m_threadEndedEvent.signal();
	return 0;
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseMgr::getLicense()
{
	try
	{
		CSingleLock slg(&ms_mutex, TRUE );

		// If a license has already been obtained once flag this as a reconnect attempt
		bool bIsReconnectAttempt = m_bHasLicense || m_bLicenseHasBeenObtained;

		// Check if already have a license or that the thread started event has been signaled
		if (m_bHasLicense)
		{
			// If the heartbeatthread is still running return
			if (isHeartBeatThreadRunning())
			{
				return;
			}

			// Release the bad license and clean up stuff
			releaseLicense("ELI29816");
		}

		// If this is a retry attempt log an exception
		if ( bIsReconnectAttempt )
		{
			UCLIDException ue("ELI25006", "Application trace: Attempting to reconnect to USB Key.");
			ue.log();
		}

		// This will be set to the last exception thrown within the timed loop below and will be
		// thrown later if the loop times out
		UCLIDException ueSave;
		bool bException = false;

		// Get retry count and wait timeout from the registry
		long nNumberOfRetries = m_snlcConfig.getNumberRetries();
		double dRetryWaitTimeout = m_snlcConfig.getRetryWaitTime();

		StopWatch sw;

		for (long i=0; i < nNumberOfRetries; i++)
		{
			sw.reset();

			// start the timer
			sw.start();

			// Initialize elapsed time
			double dElapsedTime = 0.0;

			do
			{
				try
				{
					try
					{
						// Reset the exception flag
						bException = false;

						SP_STATUS spsStatus;

						spsStatus=SFNTsntlInitialize(&m_Packet);

						if (spsStatus != SP_ERR_SUCCESS)
						{
							UCLIDException ue("ELI11298", "Failure to initialize USB Key license.");
							loadSafeNetErrInfo(ue, spsStatus);
							throw ue;
						}

						// If IcoMap license check for terminal services and if Flex turn it off
						if ( gusblIcoMap.m_dwLicenseAddr == m_rusblLicense.m_dwLicenseAddr)
						{
							// Turn on the check for terminal services
							spsStatus = SFNTsntlCheckTerminalservice(&m_Packet, SP_TERM_SERV_CHECK_ON);
						}
						else
						{
							// Turn off the check for terminal services
							spsStatus = SFNTsntlCheckTerminalservice(&m_Packet, SP_TERM_SERV_CHECK_OFF);
						}

						// 		#define SP_STANDALONE_MODE     "SP_STANDALONE_MODE"
						//		#define SP_DRIVER_MODE         "SP_DRIVER_MODE"
						//		#define SP_LOCAL_MODE          "SP_LOCAL_MODE"
						//		#define SP_BROADCAST_MODE      "SP_BROADCAST_MODE"
						//		#define SP_ALL_MODE            "SP_ALL_MODE"
						//		#define SP_SERVER_MODE         "SP_SERVER_MODE" 

						string	strServerName = m_snlcConfig.getContactServerName();

						spsStatus = SFNTsntlSetContactServer(&m_Packet, (char * )strServerName.c_str());

						if ( spsStatus != SP_ERR_SUCCESS )
						{
							UCLIDException ue("ELI11299", "Failure to set USB Key license server.");
							loadSafeNetErrInfo(ue, spsStatus);
							ue.addDebugInfo ( "ServerName", strServerName );
							throw ue;
						}
						spsStatus = SFNTsntlSetSharedLicense( &m_Packet, SP_SHARE_USERNAME, NULL );
						if ( spsStatus != SP_ERR_SUCCESS )
						{
							UCLIDException ue("ELI12621", "Unable to set license share mode.");
							loadSafeNetErrInfo(ue, spsStatus);
							throw ue;
						}
						// Since we are assuming the license is only obtained once when the application starts
						// we don't want to do retries on the get license
						spsStatus = SFNTsntlGetLicense(&m_Packet, gsnPwds.wDeveloperID, DESIGNID, m_rusblLicense.m_dwUserLimitAddr);

						if ( spsStatus != SP_ERR_SUCCESS )
						{
							UCLIDException ue("ELI11300", "Unable to obtain USB Key license.");
							loadSafeNetErrInfo(ue, spsStatus);
							ue.addDebugInfo("Server name or mode", strServerName);
							throw ue;
						}
						// TODO: Need to query the obtained license to make sure it is valid
						//			Queries should also be placed in multiple places

						// Set Heartbeat if applicable
						spsStatus = SFNTsntlSetHeartBeat(&m_Packet, gdwHEART_BEAT);

						if ( spsStatus != SP_ERR_SUCCESS && spsStatus != SP_ERR_INVALID_OPERATION)
						{
							UCLIDException ue("ELI11301", "Unable to set USB Key Heart beat.");
							loadSafeNetErrInfo(ue, spsStatus);
							ue.addDebugInfo("Server name or mode", strServerName);
							throw ue;
						}
						m_bHasLicense = true;
						if ( spsStatus == SP_ERR_SUCCESS )
						{
							m_htdData.m_psnlmMgr = this;

							// only need to do this if setting the key heartbeat is a valid operation
							m_htdData.m_pThread = AfxBeginThread(heartbeatThreadProc, &m_htdData,
								THREAD_PRIORITY_NORMAL, 0, CREATE_SUSPENDED);
							ASSERT_RESOURCE_ALLOCATION("ELI11556", m_htdData.m_pThread != __nullptr );

							// Save the thread's handle
							m_htdData.m_hThreadHandle = m_htdData.m_pThread->m_hThread;

							// Resume the thread
							m_htdData.m_pThread->ResumeThread();

							// Wait for thread to begin
							m_htdData.m_threadStartedEvent.wait();
						}

						// We have obtained a license so if this was a reconnect, log exception
						// to indicate it has been reconnected
						if (bIsReconnectAttempt)
						{
							UCLIDException ue("ELI24844",
								"Application trace: USB License reconnection successful.");
							ue.log();
						}

						m_bLicenseHasBeenObtained = true;

						// initialize the serial number
						m_dwSN = getKeySN();
					} 
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI18241");
				}
				catch ( UCLIDException &ue )
				{
					// Call releaseLicense to reset all values to defaults
					releaseLicense("ELI29817");

					// Set the saved exception everytime through the loop
					ueSave = UCLIDException ("ELI18243", "Get USB Key License timed out.", ue);

					// Flag that an exception has been thrown
					bException = true;
				}
				dElapsedTime = sw.getElapsedTime();

				// Only sleep if we don't have a license and the timeout has not been met
				if (!m_bLicenseRetry || !m_bHasLicense && dElapsedTime < dRetryWaitTimeout)
				{
					// Sleep for a while so this loop doesn't use lots of CPU time.
					Sleep(gnSLEEP_TIME_BETWEEN_RETRIES);
				}
			}
			while (m_bLicenseRetry && dElapsedTime > 0.0 && dElapsedTime < dRetryWaitTimeout  
				&& !isHeartBeatThreadRunning());

			// stop the timer
			sw.stop();

			// Exit loop if there should not be a retry or the retry has been successful
			if (!m_bLicenseRetry || m_bHasLicense && isHeartBeatThreadRunning())
			{
				break;
			}
			if (!isHeartBeatThreadRunning() && bException && i < nNumberOfRetries - 1 )
			{
				ueSave.addDebugInfo("Retries", i+1);
				ueSave.log();
			}
			else if (bException)
			{
				ueSave.addDebugInfo("Retries", i+1);
				throw ueSave;
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24880");
}
//-------------------------------------------------------------------------------------------------
SafeNetLicenseMgr::~SafeNetLicenseMgr()
{
	try
	{
		// Release the license (do not log exceptions if the release failed)
		releaseLicense("ELI29818", false);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16584");
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseMgr::releaseLicense(const string& strELICode, bool bLogReleaseException)
{
	try
	{
		resetHeartBeatThread();

		CSingleLock slg(&ms_mutex, TRUE );

		SP_STATUS spsStatus; 
		SP_WORD nLicenses = 1;
		if ( m_bHasLicense )
		{
			m_bHasLicense = false;

			spsStatus = SFNTsntlReleaseLicense(&m_Packet, m_rusblLicense.m_dwUserLimitAddr, &nLicenses);

			if ( spsStatus != SP_ERR_SUCCESS && bLogReleaseException )
			{
				UCLIDException ue("ELI11302", "Application trace: Unable to release USB Key License.");
				loadSafeNetErrInfo(ue, spsStatus);

				string	strServerName = m_snlcConfig.getContactServerName();
				ue.addDebugInfo("Server", strServerName );
				throw ue;
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS(strELICode);

	// Always call cleanup function
	try
	{
		SFNTsntlCleanup();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18573");
}
//-------------------------------------------------------------------------------------------------
SP_DWORD SafeNetLicenseMgr::getCellValue ( DataCell &rCell )
{
	try
	{
		// Validate heartbeat. This will also attempt to get a new license if license is invalid
		validateHeartbeatActive();

		SP_DWORD dwRtnValue;
		SP_STATUS spsStatus;

		CSingleLock slg(&ms_mutex, TRUE );

		spsStatus  = SFNTsntlReadValue ( &m_Packet, rCell.m_dwCellAddr, &dwRtnValue);

		if ( spsStatus != SP_ERR_SUCCESS )
		{
			UCLIDException ue("ELI11305", "Unable to Read Value.");
			loadSafeNetErrInfo(ue, spsStatus);
			throw ue;
		}
		return dwRtnValue;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24875");
}
//-------------------------------------------------------------------------------------------------
SP_DWORD SafeNetLicenseMgr::increaseCellValue( DataCell &rCell, SP_DWORD dwAmount )
{
	try
	{
		// Validate heartbeat. This will also attempt to get a new license if license is invalid
		validateHeartbeatActive();

		CSingleLock slg(&ms_mutex, TRUE );

		// if lock is not obtained an exception will be thrown
		CellLock clLock( *this, rCell );
		SP_DWORD dwCurrValue = getCellValue ( rCell );

		// Make sure the amount the counter is being increased will leave a valid value
		SP_DWORD dwMaxIncAmt;
		switch (rCell.m_eCellType )
		{
		case k32BitDataCell:
			dwMaxIncAmt = ULONG_MAX - dwCurrValue;
			break;
		case k16BitDataCell:
			dwMaxIncAmt = USHRT_MAX - (SP_WORD) dwCurrValue;
			break;
		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI11319");
		}
		if ( dwMaxIncAmt < dwAmount )
		{
			UCLIDException ue("ELI11316", "Counter value cannot be increased by amount." );
			ue.addDebugInfo ( "CounterValue", dwCurrValue );
			ue.addDebugInfo ( "AmountToDecrement", dwAmount );
			throw ue;
		}
		dwCurrValue += dwAmount;

		SP_STATUS spsStatus;
		spsStatus = SFNTsntlWriteValue( &m_Packet, rCell.m_dwCellAddr , dwCurrValue, 0 , gsnPwds.wWritePassword);
		if ( spsStatus != SP_ERR_SUCCESS )
		{
			UCLIDException ue("ELI11306", "Unable to update USB license count.");
			loadSafeNetErrInfo(ue, spsStatus);
			throw ue;
		}
		return dwCurrValue;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24881");
}
//-------------------------------------------------------------------------------------------------
SP_DWORD SafeNetLicenseMgr::decreaseCellValue( DataCell &rCell, SP_DWORD dwAmount )
{
	SP_DWORD dwCurrValue;
	try
	{
		{
			// Validate heartbeat. This will also attempt to get a new license if license is invalid
			validateHeartbeatActive();

			CSingleLock slg(&ms_mutex, TRUE );

			// if lock is not obtained an exception will be thrown
			CellLock clLock( *this, rCell );
			dwCurrValue = getCellValue ( rCell );

			// Make sure amount decrementing is available
			if ( (dwCurrValue < dwAmount)  )
			{
				bool bDone = false;
				string strMsg = "The " + rCell.getCellName() + " counter has insufficent counts to continue.\r\nPlease update the counter and try again.";
				string strCaption = rCell.getCellName() + " counter";
				int iWaitTimeout = m_snlcConfig.getWaitTimeoutForCounterOut();
				if ( iWaitTimeout != -1 )
				{
					CTimedRetryDlg timedRetry(strCaption, strMsg, iWaitTimeout);

					while (!bDone)
					{
						// Unlock the counter so that it will be possible to add counts
						// to the counter while the Retry dialog is displayed
						clLock.unlock();
						if ( timedRetry.DoModal() == IDCANCEL )
						{
							UCLIDException ue("ELI19502", "Not enough counts available. Need to add more counts." );
							ue.addDebugInfo ( "CounterValue", dwCurrValue );
							ue.addDebugInfo ( "AmountToDecrement", dwAmount );
							throw ue;
						}
						// lock the counter and read the value if still no counts continue loop
						clLock.lock();
						dwCurrValue = getCellValue( rCell );
						if ( dwCurrValue >= dwAmount )
						{
							// the count is great enough so continue
							break;
						}
					}
				}
				else
				{
					UCLIDException ue("ELI14493", "Not enough counts available. Need to add more counts." );
					ue.addDebugInfo ( "CounterValue", dwCurrValue );
					ue.addDebugInfo ( "AmountToDecrement", dwAmount );
					throw ue;
				}
			}
			// decrement the counter value
			dwCurrValue -= dwAmount;

			SP_STATUS spsStatus;

			spsStatus = SFNTsntlWriteValue( &m_Packet, rCell.m_dwCellAddr , dwCurrValue, 0 , gsnPwds.wWritePassword);

			if ( spsStatus != SP_ERR_SUCCESS )
			{
				UCLIDException ue("ELI11307", "Unable to update USB license count.");
				loadSafeNetErrInfo(ue, spsStatus);
				throw ue;
			}

			// Check new counter value and possibly add item to exception log
			if (dwCurrValue % gnLOG_FREQUENCY == 0)
			{
				UCLIDException ue("ELI15935", "Application trace: debug information");
				ue.addDebugInfo( "Item 1", getKeySN(), true );
				ue.addDebugInfo( "Item 2", dwCurrValue, true );
				ue.log();
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24882");

	try 
	{
		if ( m_snlcConfig.getSendAlert() )
		{
			checkAlert(rCell.getCellName(), dwCurrValue + dwAmount, dwCurrValue );
		}
	}
	// Need to log exceptions ( or display ) so an error in the alert checking will not
	// cause results to not be processed but the counter is already incremented
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI11869");

	return dwCurrValue;
}
//-------------------------------------------------------------------------------------------------
SP_DWORD SafeNetLicenseMgr::setCellValue(DataCell &rCell, SP_DWORD dwValue)
{
	try
	{
		// Validate heartbeat. This will also attempt to get a new license if license is invalid
		validateHeartbeatActive();

		CSingleLock slg(&ms_mutex, TRUE );

		// make sure the value is with in the range of the data value
		SP_DWORD dwMax;
		switch (rCell.m_eCellType )
		{
		case k32BitDataCell:
			dwMax = ULONG_MAX;
			break;
		case k16BitDataCell:
			dwMax = USHRT_MAX;
			break;
		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI11322");
		}
		if ( dwMax < dwValue )
		{
			UCLIDException ue("ELI11323", "Value is too large for data cell size." );
			ue.addDebugInfo( "NewValue", dwValue );
			ue.addDebugInfo( "CellMax", dwMax );
			throw ue;
		}

		CellLock clLock( *this, rCell );
		SP_DWORD dwOldValue = getCellValue ( rCell );

		SP_STATUS spsStatus;

		spsStatus = SFNTsntlWriteValue( &m_Packet, rCell.m_dwCellAddr , dwValue, 0 , gsnPwds.wWritePassword );

		if ( spsStatus != SP_ERR_SUCCESS )
		{
			UCLIDException ue("ELI11324", "Unable to update USP license count.");
			loadSafeNetErrInfo(ue, spsStatus);
			throw ue;
		}
		return dwOldValue;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24883");
}
//-------------------------------------------------------------------------------------------------
SP_DWORD SafeNetLicenseMgr::getKeySN()
{
	return getCellValue(gdcellSerialNumber);
}
//-------------------------------------------------------------------------------------------------
long SafeNetLicenseMgr::getHardLimit()
{
	try
	{
		// Get the license
		getLicense();

		SP_WORD wHardLimit;
		SFNTsntlGetHardLimit(&m_Packet, &wHardLimit);

		return (long) wHardLimit;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24884");
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseMgr::resetLock(DataCell &rCell, double fNumSecToWait)
{
	try
	{
		// Validate heartbeat. This will also attempt to get a new license if license is invalid
		validateHeartbeatActive();

		ResetLockTask resetTask(m_Packet, rCell, fNumSecToWait);
		ProgressDlgTaskRunner taskRunner(&resetTask);
		taskRunner.run();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11406");
}
//-------------------------------------------------------------------------------------------------
bool  SafeNetLicenseMgr::queryLicense( QueryResponsePair qrpQR )
{
	try
	{
		CSingleLock slg(&ms_mutex, TRUE );

		unsigned char czResponse[SP_LEN_OF_QR + 1];
		SP_DWORD dwResponse32 = 0;
		memset ( czResponse, 0, SP_LEN_OF_QR + 1 );

		SP_STATUS spsStatus;
		spsStatus = SFNTsntlQueryLicenseSimple (&m_Packet, m_rusblLicense.m_dwLicenseAddr, qrpQR.czQuery, czResponse, &dwResponse32, SP_LEN_OF_QR );
		if ( spsStatus != SP_ERR_SUCCESS )
		{
			UCLIDException ue("ELI12078", "Unable to Query license. USB License Invalid.");
			loadSafeNetErrInfo(ue, spsStatus);
			throw ue;
		}
		return qrpQR.isValidResponse( czResponse );
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24885");
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseMgr::validateUSBLicense()
{
	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		int iQryNumber = rand() % m_rusblLicense.m_iNumQrys;
		QueryResponsePair qrpQR = m_rusblLicense.getQRPair(iQryNumber);
		if ( !queryLicense( qrpQR ) )
		{
			// The license is not good so release it - this will stop heartbeat thread and reset every thing.
			releaseLicense("ELI29819");
			UCLIDException ue("ELI12112", "USB License not valid." );
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24886");
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseMgr::validateHeartbeatActive()
{
	try
	{
		// Make sure the license has been obtained
		getLicense();

		CSingleLock slg(&ms_mutex, TRUE );

		// Check to see if the thread has ended
		if (!isHeartBeatThreadRunning())	
		{
			// The license is no longer good so releaseLicense 
			releaseLicense("ELI29820");

			if ( m_htdData.m_bException )
			{
				// reset the flag for the exception so that the exception is not thrown again
				m_htdData.m_bException = false;
				UCLIDException uexOuter("ELI12133", "USB License heartbeat thread has exited.", m_htdData.m_ue);
				throw uexOuter;
			}
			else
			{
				UCLIDException ue("ELI12134", "USB License heartbeat thread has exited.");
				throw ue;
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24887");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseMgr::sendAlert(const string& strAlert)
{
	try
	{
		// if the email settings has not been setup then the email settings and Message need to 
		// be initialized
		if ( m_ipEmailSettings == __nullptr )
		{
			m_ipEmailSettings.CreateInstance(CLSID_SmtpEmailSettings);
			ASSERT_RESOURCE_ALLOCATION("ELI12307", m_ipEmailSettings != __nullptr );
		
			m_ipEmailSettings->LoadSettings(VARIANT_FALSE);
			
			m_ipMessage.CreateInstance(CLSID_ExtractEmailMessage);
			ASSERT_RESOURCE_ALLOCATION("ELI12401", m_ipMessage!= __nullptr );
		
			m_ipMessage->EmailSettings = m_ipEmailSettings;
		}
		addRecipients( m_ipMessage, m_snlcConfig.getAlertToList());

		m_ipMessage->Subject = "Extract Systems LM USB Key Counter value is low";
		m_ipMessage->Body = strAlert.c_str();
		m_ipMessage->Send();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI11873");
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseMgr::checkAlert(const string& strCounterName, SP_DWORD dwCounterValue, SP_DWORD dwNewValue )
{
	try
	{
		bool bSendAlert = false;
		{
			CSingleLock slg(&ms_mutex, TRUE );

			// if the counter value is zero assume an alert has already been sent and return
			// this is so several email messages will not be sent if a large batch is running
			if ( dwCounterValue == 0 ) 
			{
				return;
			}

			//SafeNetLicenseCfg snlcConfig;
			DWORD dwAlertValue = m_snlcConfig.getCounterAlertLevel( strCounterName );
			DWORD dwAlertMultiple = m_snlcConfig.getCounterAlertMultiple( strCounterName );

			if ( dwNewValue <= dwAlertValue || dwNewValue == 0 )
			{
				// make sure alert multiple is greater than 0
				if ( dwAlertMultiple > 0 )
				{
					// Alert may be required
					DWORD dwAlertRemainder = dwAlertValue % dwAlertMultiple;
					DWORD dwNewRemainder = dwNewValue % dwAlertMultiple;

					// Check if the new value is changing from above to below the level and alert is required
					if ( dwNewRemainder == dwAlertRemainder || dwNewValue == 0)
					{
						bSendAlert = true;
					}
				}
				else if (dwNewValue == dwAlertValue )
				{
					// send alert once when the new value equals the alert value
					bSendAlert = true;
				}
			}
		}
		if ( bSendAlert )
		{
			// Build the message to be sent
			string strNewValue =  asString(dwNewValue);
			string strAlertString;
			strAlertString = "The " + strCounterName + " Counter for USB Key SN "
				+ asString(m_dwSN) + " has " + strNewValue + " counts remaining.\r\n";

			// Add the Name, organiztion and phone number if given
			sendAlert(strAlertString);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24888");
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseMgr::addRecipients( IExtractEmailMessagePtr ipMessage,
	const string &strRecipients )
{
	try
	{
		ASSERT_ARGUMENT("ELI24890", ipMessage != __nullptr);

		CSingleLock slg(&ms_mutex, TRUE );

		vector<string> vecTokens;

		// Separate recipients
		StringTokenizer::sGetTokens(strRecipients, ';', vecTokens);
		int nNumRecipients =  vecTokens.size();
		IVariantVectorPtr ipRecipients(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI12402", ipRecipients != __nullptr );
		for ( int i = 0; i < nNumRecipients; i++ )
		{
			// Trim spaces
			string strRecipient = trim(vecTokens[i], " ", " " );
			// Add recipient to list
			ipRecipients->PushBack( strRecipient.c_str());

		}
		// Add Extract Systems as recipient if requested
		if ( m_snlcConfig.getSendToExtract() )
		{
			ipRecipients->PushBack( "support@extractsystems.com" );
		}

		ipMessage->Recipients = ipRecipients;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24889");
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseMgr::resetHeartBeatThread()
{
	// need to stop the heartbeat thread
	// Don't need to protect this with the ms_mutex 
	if ( m_htdData.m_pThread != __nullptr )
	{
		// Make sure the thread is still running
		if (isHeartBeatThreadRunning())
		{
			m_htdData.m_threadStopRequest.signal();

			m_htdData.m_threadEndedEvent.wait();
		}
	}

	// Lock mutex so that if getlicense is called on another thread this doesn't get called
	CSingleLock slg(&ms_mutex, TRUE );
	
	// The thread should not be running at this point so reset the thread pointer
	m_htdData.m_pThread = NULL;

	// Reset the thread signals since the heartbeat thread is not running at this point
	m_htdData.m_threadStopRequest.reset();
	m_htdData.m_threadEndedEvent.reset();
	m_htdData.m_threadStartedEvent.reset();

	// Handle any exception from the thread
	if ( m_htdData.m_bException )
	{
		// Encapsulate the thread exception and log
		UCLIDException ue("ELI24874", "Heartbeat thread exited with exception.", m_htdData.m_ue);
		ue.log();
		
		// Flag exception so that it will not be logged again.
		m_htdData.m_bException = false;
	}
}
//-------------------------------------------------------------------------------------------------
bool SafeNetLicenseMgr::isHeartBeatThreadRunning()
{
	// Get the events status
	bool bIsRunning = m_htdData.m_threadStartedEvent.isSignaled() &&
		!m_htdData.m_threadEndedEvent.isSignaled();	

	// If the Events indicate the thread is running return the status of the thread.
	 return bIsRunning && WaitForSingleObject(m_htdData.m_hThreadHandle, 0) == WAIT_TIMEOUT;
}
//-------------------------------------------------------------------------------------------------
