// FileProcessingManager.h : Declaration of the CFileProcessingManager

#pragma once

#include "resource.h"       // main symbols
#include "FileProcessingDlg.h"
#include "FPRecordManager.h"
#include "FileSupplyingMgmtRole.h"
#include "FileProcessingMgmtRole.h"
#include "UCLIDFileProcessing.h"

#include <Win32Event.h>
#include <FolderEventsListener.h>
#include <MTSafeQueue.h>
#include <Win32CriticalSection.h>

#include <memory>
#include <string>
#include <vector>
#include <map>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CFileProcessingManager
//-------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CFileProcessingManager : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFileProcessingManager, &CLSID_FileProcessingManager>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IFileProcessingManager, &IID_IFileProcessingManager, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<IRoleNotifyFAM, &__uuidof(IRoleNotifyFAM), &LIBID_UCLID_FILEPROCESSINGLib, /* wMajor = */ 1, /* wMinor = */ 0>
{
public:
	CFileProcessingManager();
	~CFileProcessingManager();

	DECLARE_REGISTRY_RESOURCEID(IDR_FILEPROCESSINGMANAGER)

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	BEGIN_COM_MAP(CFileProcessingManager)
		COM_INTERFACE_ENTRY(IFileProcessingManager)
		COM_INTERFACE_ENTRY2(IDispatch, IFileProcessingManager)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IRoleNotifyFAM)
	END_COM_MAP()

public:
	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// IFileProcessingManager
	STDMETHOD(StopProcessing)();
	STDMETHOD(StartProcessing)();
	STDMETHOD(ShowUI)(VARIANT_BOOL bRunOnInit, VARIANT_BOOL bCloseOnComplete, VARIANT_BOOL bForceClose, 
		int iNumDocsToExecute, void * pFRM);
	STDMETHOD(SaveTo)(BSTR strFullFileName, VARIANT_BOOL bClearDirty);
	STDMETHOD(LoadFrom)(BSTR strFullFileName, VARIANT_BOOL bSetDirtyFlagToTrue);
	STDMETHOD(get_FPSFileName)(BSTR *pVal);
	STDMETHOD(put_FPSFileName)(BSTR newVal);
	STDMETHOD(get_MaxStoredRecords)(long *pVal);
	STDMETHOD(put_MaxStoredRecords)(long newVal);
	STDMETHOD(get_RestrictNumStoredRecords)(VARIANT_BOOL *pVal);
	STDMETHOD(put_RestrictNumStoredRecords)(VARIANT_BOOL newVal);
	STDMETHOD(PauseProcessing)();
	STDMETHOD(get_ProcessingStarted)(VARIANT_BOOL *pbValue);
	STDMETHOD(get_ProcessingPaused)(VARIANT_BOOL *pbValue);
	STDMETHOD(LoadFilesFromFile)(BSTR bstrFileName);
	STDMETHOD(get_ActionName)(BSTR *pVal);
	STDMETHOD(put_ActionName)(BSTR newVal);
	STDMETHOD(get_DisplayOfStatisticsEnabled)(VARIANT_BOOL *pVal);
	STDMETHOD(put_DisplayOfStatisticsEnabled)(VARIANT_BOOL newVal);
	STDMETHOD(Clear)();
	STDMETHOD(ValidateStatus)(void);
	STDMETHOD(get_FileSupplyingMgmtRole)(IFileSupplyingMgmtRole **pVal);
	STDMETHOD(get_FileProcessingMgmtRole)(IFileProcessingMgmtRole **pVal);
	STDMETHOD(GetActionIDFromName)(BSTR bstrActionName, long *pVal);
	STDMETHOD(get_DatabaseServer)(BSTR *pVal);
	STDMETHOD(put_DatabaseServer)(BSTR newVal);
	STDMETHOD(get_DatabaseName)(BSTR *pVal);
	STDMETHOD(put_DatabaseName)(BSTR newVal);
	STDMETHOD(GetCounts)(long* plNumFilesProcessedSuccessfully, long* plNumProcessingErrors,
		long* plNumFilesSupplied, long* plNumSupplyingErrors);
	STDMETHOD(get_IsDBPasswordRequired)(VARIANT_BOOL* pvbIsDBPasswordRequired);
	STDMETHOD(GetExpandedActionName)(BSTR *pbstrAction);
	STDMETHOD(put_NumberOfDocsToProcess)(long lNumberOfDocsToProcess);
	STDMETHOD(get_IsUserAuthenticationRequired)(VARIANT_BOOL* pvbIsAuthenticationRequired);
	STDMETHOD(ProcessSingleFile)(BSTR bstrSourceDocName,VARIANT_BOOL vbQueue,
		VARIANT_BOOL vbProcess, VARIANT_BOOL vbForceProcessing, EFilePriority eFilePriority);
	STDMETHOD(AuthenticateForProcessing)(VARIANT_BOOL* pvbAuthenticated);
	STDMETHOD(get_MaxFilesFromDB)(long* pVal);
	STDMETHOD(put_MaxFilesFromDB)(long newVal);

	// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// IRoleNotifyFAM Methods
	STDMETHOD(NotifyProcessingCompleted)();
	STDMETHOD(NotifySupplyingCompleted)();
	STDMETHOD(NotifyProcessingCancelling)();

private:
	// Thread procedure which initiates the stopping of processing asynchronously
	static UINT handleStopRequestAsynchronously(void *pData);

	/////////////
	// Variables
	/////////////

	// a flag to indicate the process in on going
	volatile bool m_bProcessing;

	// A flag to indicate the process is cancelling
	volatile bool m_bCancelling;
	
	// a flag to indicate that supplying is going on
	volatile bool m_bSupplying;

	// a flag to indicate if the processing has currently been paused
	volatile bool m_bPaused;

	// If this is 0 processing will be normal, if > 0 then that number of files
	// will be processed.
	long m_nNumberOfFilesToExecute;

	// vector of thread data objects containing data for each of the processing threads
	vector<ProcessingThreadData *> m_vecProcessingThreadData;

	unique_ptr<FileProcessingDlg> m_apDlg;

	// The filename that this manager was most recently
	// loaded from or saved to
	string m_strFPSFileName;

	// This flag will be set to true
	bool bRunOnInit;

	// The Database to work with
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFPMDB;

	UCLID_FILEPROCESSINGLib::IFileSupplyingMgmtRolePtr m_ipFSMgmtRole;

	UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr m_ipFPMgmtRole;

	// FAM TagManager pointer
	UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr m_ipFAMTagManager;

	// Action Name being processed
	string m_strAction;

	// flag of the statistics checkbox in action tab
	bool m_bDisplayOfStatisticsEnabled;

	// Is database connection ready
	bool m_isDBConnectionReady;

	// previous DB server and previous DB name 
	// added as per [p13 #4581 & #4580] so that
	// the dirty flag can be properly set
	string m_strPreviousDBServer;
	string m_strPreviousDBName;

	CMutex m_mutexLockFilter;

	// this mutex is acquired while processing is taking place
	// to check if processing is taking place, one just needs to
	// see if this mutex is acquired.  To wait until processing
	// is complete, one just needs to try to acquire this mutex;
	CMutex m_threadLock;

	FPRecordManager m_recordMgr;

	// Used by IPersistStream Implementation
	bool m_bDirty;

	// Whether or not to record FAM session information
	bool m_bRecordFAMSessions;

	// Status to describe whether FAM is beginning to process,
	// beginning to stop processing or actually stops processing.
	enum EStartStopStatus
	{
		kStart,
		kBeginStop,
		kEndStop
	};

	// The max number of files to grab from the DB when processing
	long m_nMaxFilesFromDB;

	///////////
	// Methods
	///////////
	//------------------------------------------------------------------------------------------
	// PURPOSE: Internal method to clear all member variables and release any allocated memory
	void clear();
	//------------------------------------------------------------------------------------------
	// PURPOSE: To return the IFileActionMgmtRole interface pointer on the given object
	// REQUIRE: ipUnknown must implement the IFileActionMgmtRole interface
	UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr getActionMgmtRole(IUnknownPtr ipUnknown);

	// Returns the action name with the tags expanded
	// NOTE: This method also sets the FPSFileDir value for the tag manager
	string getExpandedActionName();

	// Returns the value of m_ipFPMDB. If it is NULL a new instance will be created if
	// unable to create and instance an exception will be thrown
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr getFPMDB();

	// get the file processor data vector for brief use
	IIUnknownVectorPtr getFileProcessorsData();

	// get the file supplier data vector for brief use
	IIUnknownVectorPtr getFileSuppliersData();

	// Determines if the specified action is in the database
	bool isActionNameInDatabase(const string& strAction);

	// Log the start and stop processing information
	void logStatusInfo(EStartStopStatus eStatus);

	// Indicates whether user authentication is required to run.
	bool isUserAuthenticationRequired();

	// Indicates whether a DB admin password is required to run.
	bool isDBPasswordRequired();

	// Prompts for user and DB admin passwords as appropriate to run. Returns true if processing is
	// allowed to run, false if the user was prompted for a password they did not correctly enter.
	bool authenticateForProcessing();

	// Gets the this pointer as smart com pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr getThisAsCOMPtr();

	void validateLicense();
};
