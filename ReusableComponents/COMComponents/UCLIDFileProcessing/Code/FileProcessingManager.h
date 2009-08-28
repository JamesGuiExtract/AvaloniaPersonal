// FileProcessingManager.h : Declaration of the CFileProcessingManager

#pragma once

#include "resource.h"       // main symbols
#include "FileProcessingDlg.h"
#include "FPRecordManager.h"
#include "FileSupplyingMgmtRole.h"
#include "FileProcessingMgmtRole.h"

#include <memory>
#include <Win32Event.h>
#include <FolderEventsListener.h>
#include <MTSafeQueue.h>
#include <Win32CriticalSection.h>

#include <string>
#include <vector>
#include <map>
#include "UCLIDFileProcessing.h"

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
	STDMETHOD(SaveTo)(/*[in]*/ BSTR strFullFileName, VARIANT_BOOL bClearDirty);
	STDMETHOD(LoadFrom)(/*[in]*/ BSTR strFullFileName, VARIANT_BOOL bSetDirtyFlagToTrue);
	STDMETHOD(get_FPSFileName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_FPSFileName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_MaxStoredRecords)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_MaxStoredRecords)(/*[in]*/ long newVal);
	STDMETHOD(get_RestrictNumStoredRecords)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_RestrictNumStoredRecords)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(PauseProcessing)();
	STDMETHOD(get_ProcessingStarted)(/*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(get_ProcessingPaused)(/*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(LoadFilesFromFile)(BSTR bstrFileName);
	STDMETHOD(get_ActionName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_ActionName)(/*[in]*/ BSTR newVal);
//	STDMETHOD(get_ActionID)(/*[out, retval]*/ long *pVal);
//	STDMETHOD(put_ActionID)(/*[in]*/ long newVal);
	STDMETHOD(get_DisplayOfStatisticsEnabled)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_DisplayOfStatisticsEnabled)(/*[in]*/  VARIANT_BOOL newVal);
	STDMETHOD(Clear)();
	STDMETHOD(ValidateStatus)(void);
	STDMETHOD(get_FileSupplyingMgmtRole)(/*[out, retval]*/ IFileSupplyingMgmtRole **pVal);
	STDMETHOD(get_FileProcessingMgmtRole)(/*[out, retval]*/ IFileProcessingMgmtRole **pVal);
	STDMETHOD(GetActionIDFromName)(/*[in]*/ BSTR bstrActionName, /*[out, retval]*/ long *pVal);
	STDMETHOD(get_DatabaseServer)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_DatabaseServer)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_DatabaseName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_DatabaseName)(/*[in]*/ BSTR newVal);

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

	std::auto_ptr<FileProcessingDlg> m_apDlg;

	// The filename that this manager was most recently
	// loaded from or saved to
	std::string m_strFPSFileName;

	// This flag will be set to true
	bool bRunOnInit;

	// The Database to work with
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFPMDB;

	UCLID_FILEPROCESSINGLib::IFileSupplyingMgmtRolePtr m_ipFSMgmtRole;

	UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr m_ipFPMgmtRole;

	// FAM TagManager pointer
	UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr m_ipFAMTagManager;

	// Action Name being processed
	std::string m_strAction;

	// flag of the statistics checkbox in action tab
	bool m_bDisplayOfStatisticsEnabled;

	// Is database connection ready
	bool m_isDBConnectionReady;

	// previous DB server and previous DB name 
	// added as per [p13 #4581 & #4580] so that
	// the dirty flag can be properly set
	std::string m_strPreviousDBServer;
	std::string m_strPreviousDBName;

	CMutex m_mutexLockFilter;

	// this mutex is acquired while processing is taking place
	// to check if processing is taking place, one just needs to
	// see if this mutex is acquired.  To wait until processing
	// is complete, one just needs to try to acquire this mutex;
	CMutex m_threadLock;

	FPRecordManager m_recordMgr;

	// Used by IPersistStream Implementation
	bool m_bDirty;

	// Status to describe whether FAM is beginning to process,
	// beginning to stop processing or actually stops processing.
	enum EStartStopStatus
	{
		kStart,
		kBeginStop,
		kEndStop
	};

	// Counter used to increment the unique process ID with each processing run
	int m_iUPICounter;

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

	// Returns the value of m_ipFPMDB. If it is NULL a new instance will be created if
	// unable to create and instance an exception will be thrown
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr getFPMDB();

	// get the file processor data vector for brief use
	IIUnknownVectorPtr getFileProcessorsData();

	// get the file supplier data vector for brief use
	IIUnknownVectorPtr getFileSuppliersData();

	// Log the start and stop processing information
	void logStatusInfo(EStartStopStatus eStatus);

	// Gets the this pointer as smart com pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr getThisAsCOMPtr();

	void validateLicense();
};
