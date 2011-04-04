// SafeNetLicenseMgr.h: interface for the SafeNetLicenseMgr class.
//
//////////////////////////////////////////////////////////////////////
#pragma once

#include "UpromepsDesign.h"
#include "SafeNetUtils.h"
#include "IProgressTask.h"
#include <UCLIDException.h>
#include <Win32Mutex.h>
#include <Win32Event.h>
#include "SafeNetLicenseCfg.h"

#include <string>
#include <memory>

//#import "..\..\..\APIs\LeadTools_13\bin\LTCML13N.DLL" no_namespace

using namespace std;

class SAFENETUTILS_API DataCell;
class SAFENETUTILS_API QueryResponsePair;
class SAFENETUTILS_API USBLicense;

class SAFENETUTILS_API SafeNetLicenseMgr  
{
public:
	// if bObtainLicense is true the constructor will call getLicense() function
	// if bLicenseRetry is true the manager will keep trying to get a license until a set timeout ( 2 min)
	SafeNetLicenseMgr( USBLicense &rusblLicense, bool bObtainLicense = false, bool bLicenseRetry = true );
	virtual ~SafeNetLicenseMgr();

	// returns the value of the cell at the given address
	// if error will throw UCLIDException 
	SP_DWORD getCellValue ( DataCell &rCell );

	// Returns the hard limit of the key
	long getHardLimit();

	/////////////////////////////////////////////////////////////////////////////////////
	// PURPOSE:		To increase the datacell value by the amount nAmount
	// RETURNS:		Returns the new value of the data cell
	// PROMISE:		To throw an exception if unable to update the datacell or
	//				if the new value would be greater than the maximum value the 
	//				data cell can contain
	// TESTTHIS made Amount SP_DWORD instead of long for increaseCellValue
	SP_DWORD increaseCellValue( DataCell &rCell, SP_DWORD dwAmount );
	
	/////////////////////////////////////////////////////////////////////////////////////
	// PURPOSE:		To decrease the datacell value by the amount nAmount
	// RETURNS:		Returns the new value of the data cell
	// PROMISE:		To throw an exception if unable to update the datacell or
	//				if the value of the data cell is less than the value being decremented
	//				if an exception is thrown the value is not updated
	// TESTTHIS made Amount SP_DWORD instead of long for decreaseCellValue
	SP_DWORD decreaseCellValue( DataCell &rCell, SP_DWORD dwAmount );
	/////////////////////////////////////////////////////////////////////////////////////
	// PURPOSE:		To set the value of the datacell to the given value 
	// RETURNS:		Returns the old value of the cell
	// PROMISE:		To throw an exception if unable to update the datacell
	//				if an exception is thrown the value is not updated
	SP_DWORD setCellValue(DataCell &rCell, SP_DWORD dwValue);

	/////////////////////////////////////////////////////////////////////////////////////
	// PURPOSE:		To reset the lock for the given cell after trying to obtain the lock
	//				for the given number of seconds
	void resetLock(DataCell &rCell, double fNumSecToWait);

	/////////////////////////////////////////////////////////////////////////////////////
	// PURPOSE:		To read the serial number from the key that the license was obtained from
	SP_DWORD getKeySN();

	/////////////////////////////////////////////////////////////////////////////////////
	// PURPOSE:		To query the license key with the Query value in qrpQR and if the 
	//				response is the same as the response in qrpQR then return true otherwise
	//				return false
	bool queryLicense( QueryResponsePair qrpQR );

	/////////////////////////////////////////////////////////////////////////////////////
	// PURPOSE:		Checks to make sure the heartbeat thread is still running and calls
	//				queryLicense with random QueryResponsePair if thread is not running 
	//				or queryLicense fails this function will throw and exception
	void validateUSBLicense();

	/////////////////////////////////////////////////////////////////////////////////////
	// PURPOSE:		If the heartbeat thread is no longer running this function will throw
	//				an exception, if the heartbeat thread ended because of an exception
	//				this function will rethrow that exception.
	void validateHeartbeatActive();

	/////////////////////////////////////////////////////////////////////////////////////
	// PURPOSE:		To check current status of the license and obtain a new one if there
	//				is not a valid license.
	//				There will only be one attempt to get the license if the m_bLicenseRetry
	//				is false.
	void getLicense();

	/////////////////////////////////////////////////////////////////////////////////////
	// PURPOSE:		To stop the heartbeat thread if it is running and release any license
	//				that has been obtained. Any exceptions are logged.
	// NOTE:		This calls SFNTsntlCleanup().
	void releaseLicense(const string& strELICode = "ELI18190", bool bLogReleaseException = true);
	bool hasLicense();

	/////////////////////////////////////////////////////////////////////////////////////
	// Public variables;
	/////////////////////////////////////////////////////////////////////////////////////
	
	// Used to keep multiple threads from updating while in the process of updating
	static CMutex ms_mutex;

	/////////////////////////////////////////////////////////////////////////////////////
	// Public Classes;
	/////////////////////////////////////////////////////////////////////////////////////
	
	class SAFENETUTILS_API CellLock
	{
	public:
		SAFENETUTILS_API friend class SafeNetLicenseMgr;
		CellLock(SafeNetLicenseMgr& rsnManager, DataCell & rCell);
		virtual ~CellLock();

		// method Locks the cell
		void lock();
		// Method unlocks the cell
		void unlock();
	private:
		SafeNetLicenseMgr& m_rsnManager;
		DataCell &m_rCell;
	};
	class ResetLockTask : IProgressTask
	{
	public:
		SAFENETUTILS_API friend class SafeNetLicenseMgr;
		ResetLockTask(SP_UPRO_APIPACKET &rPacket, DataCell & rCell, double dNumSecToWait);
		void runTask(IProgress *pProgress, int nTaskID = 0 );
	private:
		SP_UPRO_APIPACKET &m_rPacket;
		DataCell &m_rCell;
		double m_dNumSecToWait;
		bool m_bShowDialog;
	};

private:
	SAFENETUTILS_API friend class SafeNetLicenseMgr::CellLock;
	SAFENETUTILS_API friend class SafeNetLicenseMgr::ResetLockTask;

	friend UINT heartbeatThreadProc(void *pData);
	class HeartbeatThreadData
	{
	public:
		HeartbeatThreadData	(SP_UPRO_APIPACKET &rPacket);
		~HeartbeatThreadData();
		Win32Event m_threadStartedEvent;
		Win32Event m_threadEndedEvent;
		Win32Event m_threadStopRequest;
		CWinThread* m_pThread;
		HANDLE m_hThreadHandle;
		SafeNetLicenseMgr * m_psnlmMgr;
		UCLIDException m_ue;
		bool m_bException;
		// This will be used to call to get the serial number as a way to keep the key active
		SP_UPRO_APIPACKET &m_rPacket;
	};

	HeartbeatThreadData m_htdData;
	
	// Contains the intialized packet for the license obtained in the constructor
	// used to access the key 
	SP_UPRO_APIPACKET m_Packet;

	// Contains the address of the license on the key
	//SP_DWORD m_dwLicenseAddr;

	// Used to indicate the license has been obtained
	bool m_bHasLicense;

	SafeNetLicenseCfg m_snlcConfig;

	// holds the serial number
	SP_DWORD m_dwSN;

	// Holds the current license id being used ( IcoMap or FlexIndex )
	USBLicense& m_rusblLicense;

	// Email Settings
	ISmtpEmailSettingsPtr m_ipEmailSettings;
	
	// Email Message 
	IExtractEmailMessagePtr m_ipMessage;

	// Flag for license retry, if this is true an attempt to get a license will
	// keep retrying until the timeout otherwise there will be no retry
	bool m_bLicenseRetry;

	// Flag to indicate that the license has been obtained at least once for the
	// current SafeNetLicenseMgr object.
	bool m_bLicenseHasBeenObtained;

	//Methods

	// Looks up the alert value for the given data cell and compare against the given value
	// if == alert value alert will be sent
	void checkAlert(const string& strCounterName, SP_DWORD dwCounterValue, SP_DWORD dwNewValue );
	void sendAlert(const string& strAlert);

	void addRecipients( IExtractEmailMessagePtr ipMessage, const string &strRecipients );

	// This method will check if the heartbeat thread is running and stop it if it is.
	// After this method is called all of the heartbeat thread events will be in non signaled state.
	void resetHeartBeatThread();

	// Returns true if the heartbeat thread is still running
	bool isHeartBeatThreadRunning();
};

enum SAFENETUTILS_API ECellType
{
	kInvalidCelltype,
	k32BitDataCell,
	k16BitDataCell
};

class SAFENETUTILS_API DataCell
{
public:
	DataCell( ECellType eCellType, SP_DWORD dwCellAddr, SP_DWORD dwLockCellAddr, string strCellName );
	
	SAFENETUTILS_API friend class SafeNetLicenseMgr;
	SAFENETUTILS_API friend class SafeNetLicenseMgr::CellLock;
	SAFENETUTILS_API friend class SafeNetLicenseMgr::ResetLockTask;
	friend UINT heartbeatThreadProc(void *pData);
	
	string getCellName();
private:
	// Contains the type of the Cell
	ECellType m_eCellType;
	// Contains the Cell address
	SP_DWORD m_dwCellAddr;
	// Contains the Lock Cell address
	SP_DWORD m_dwLockCellAddr;

	string m_strCellName;
};

class SAFENETUTILS_API QueryResponsePair
{
public:
	SAFENETUTILS_API friend class SafeNetLicenseMgr;
	QueryResponsePair( unsigned char *pczQuery, unsigned char *pczResponse);
	
	// Returns true if the argument pczResponse matches the member var czResponse
	bool isValidResponse( unsigned char *pczResponse );
	
private:
	unsigned char czQuery[SP_LEN_OF_QR + 1];
	unsigned char czResponse[SP_LEN_OF_QR + 1];
};

class SAFENETUTILS_API USBLicense
{
public:
	SAFENETUTILS_API friend class SafeNetLicenseMgr;
	USBLicense( SP_DWORD dwLicenseAddr,  SP_DWORD dwUserLimitAddr, 
			unsigned char *pszQueryArray, unsigned char *pszResponseArray, int iNumQrys );
	QueryResponsePair getQRPair( int iQryNumber );

	int m_iNumQrys;
private:
	SP_DWORD m_dwLicenseAddr;
	SP_DWORD m_dwUserLimitAddr;
	unsigned char *m_pszQueryArray;
	unsigned char *m_pszResponseArray;
};

// External DataCell instances
extern SAFENETUTILS_API DataCell gdcellFlexIndexingCounter;
extern SAFENETUTILS_API DataCell gdcellFlexPaginationCounter;
extern SAFENETUTILS_API DataCell gdcellIDShieldRedactionCounter;
extern SAFENETUTILS_API DataCell gdcellCounterIncrementAmount;
extern SAFENETUTILS_API DataCell gdcellCounterToIncrement;

// User Limits for license
extern SAFENETUTILS_API DataCell gdcellIcoMapUserLimit;
extern SAFENETUTILS_API DataCell gdcellFlexIndexUserLimit;

// Available Licenses to use
extern SAFENETUTILS_API USBLicense gusblFlexIndex;
extern SAFENETUTILS_API USBLicense gusblIcoMap;


