// FileSupplyingMgmtRole.h : Declaration of the CFileSupplyingMgmtRole

#pragma once

#include "resource.h"       // main symbols
#include "FileSupplyingRecord.h"

#include <Win32Event.h>
#include <string>
#include <vector>

using namespace std;

//-------------------------------------------------------------------------------------------------
// SupplierThreadData class
//-------------------------------------------------------------------------------------------------
class SupplierThreadData
{
public:
	// ctor
	SupplierThreadData(UCLID_FILEPROCESSINGLib::IFileSupplier* pFS,
		UCLID_FILEPROCESSINGLib::IFileSupplierTarget* pFST,
		UCLID_FILEPROCESSINGLib::IFAMTagManager* pFAMTM,
		IFileProcessingDB* pDB, long nActionID, bool displayExceptions);

	Win32Event m_threadStartedEvent;
	Win32Event m_threadEndedEvent;

private:
	// friends
	friend class CFileSupplyingMgmtRole;

	// members are private so that initializing in the ctor is forced
	UCLID_FILEPROCESSINGLib::IFileSupplierPtr m_ipFileSupplier;
	UCLID_FILEPROCESSINGLib::IFileSupplierTargetPtr m_ipFileSupplierTarget;
	UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr m_ipFAMTagManager;
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipDB;
	long m_nActionID;
	bool m_bDisplayExceptions;
};

//-------------------------------------------------------------------------------------------------
// CFileSupplyingMgmtRole
//-------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CFileSupplyingMgmtRole :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFileSupplyingMgmtRole, &CLSID_FileSupplyingMgmtRole>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IFileActionMgmtRole, &IID_IFileActionMgmtRole, &LIBID_UCLID_FILEPROCESSINGLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<IFileSupplyingMgmtRole, &IID_IFileSupplyingMgmtRole, &LIBID_UCLID_FILEPROCESSINGLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IFileSupplierTarget, &__uuidof(IFileSupplierTarget), &LIBID_UCLID_FILEPROCESSINGLib, /* wMajor = */ 1, /* wMinor = */ 0>
{
public:
	CFileSupplyingMgmtRole();
	~CFileSupplyingMgmtRole();

DECLARE_REGISTRY_RESOURCEID(IDR_FILESUPPLYINGMGMTROLE)

BEGIN_COM_MAP(CFileSupplyingMgmtRole)
	COM_INTERFACE_ENTRY(IFileSupplyingMgmtRole)
	COM_INTERFACE_ENTRY(IFileActionMgmtRole)
	// TODO: why does the following line cause a compile error here, but similar code in other classes don't?
	//COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY2(IDispatch, IFileSupplyingMgmtRole)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IFileSupplierTarget)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()
	HRESULT FinalConstruct();
	void FinalRelease();

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

// IFileActionMgmtRole
	STDMETHOD(Start)(IFileProcessingDB* pDB, long lActionId, BSTR bstrAction, long hWndOfUI, 
		IFAMTagManager* pTagManager, IRoleNotifyFAM* pRoleNotifyFAM, BSTR bstrFpsFileName);
	STDMETHOD(Stop)(void);
	STDMETHOD(Pause)(void);
	STDMETHOD(Resume)(void);
	STDMETHOD(get_Enabled)(VARIANT_BOOL* pVal);
	STDMETHOD(put_Enabled)(VARIANT_BOOL newVal);
	STDMETHOD(Clear)(void);
	STDMETHOD(ValidateStatus)(void);

// IFileSupplyingMgmtRole
	STDMETHOD(get_FileSuppliers)(IIUnknownVector** pVal);
	STDMETHOD(put_FileSuppliers)(IIUnknownVector* newVal);
	STDMETHOD(get_FAMCondition)(IObjectWithDescription** pVal);
	STDMETHOD(put_FAMCondition)(IObjectWithDescription* newVal);
	STDMETHOD(SetDirty)(VARIANT_BOOL newVal);
	STDMETHOD(GetSupplyingCounts)(long* plNumSupplied, long* plNumSupplyingErrors);
	STDMETHOD(get_SkipPageCount)(VARIANT_BOOL *pVal);
	STDMETHOD(put_SkipPageCount)(VARIANT_BOOL newVal);

// IFileSupplierTarget Methods
	STDMETHOD(NotifyFileAdded)(BSTR bstrFile,  IFileSupplier* pSupplier,
		IFileRecord** ppFileRecord);
	STDMETHOD(NotifyFileRemoved)(BSTR bstrFile,  IFileSupplier* pSupplier);
	STDMETHOD(NotifyFileRenamed)(BSTR bstrOldFile,  BSTR bstrNewFile,  IFileSupplier* pSupplier);
	STDMETHOD(NotifyFolderDeleted)(BSTR bstrFolder,  IFileSupplier* pSupplier);
	STDMETHOD(NotifyFolderRenamed)(BSTR bstrOldFolder,  BSTR bstrNewFolder,  IFileSupplier* pSupplier);
	STDMETHOD(NotifyFileModified)(BSTR bstrFile,  IFileSupplier* pSupplier);
	STDMETHOD(NotifyFileSupplyingDone)(IFileSupplier* pSupplier);
	STDMETHOD(NotifyFileSupplyingFailed)(IFileSupplier* pSupplier, BSTR strError );

// IPersistStream
	STDMETHOD(GetClassID)(CLSID* pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream* pStm);
	STDMETHOD(Save)(IStream* pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER* pcbSize);

private:
	/////////////////
	// File Suppliers and associated settings
	/////////////////
	// Contains IFileSupplierData items
	// where each item iincludes
	// - IObjectWithDescription (containing IFileSupplier, Description, Enabled)
	// - vbForceProcessing
	// - EFileSupplierStatus
	IIUnknownVectorPtr m_ipFileSuppliers;

	// Skip Condition
	IObjectWithDescriptionPtr m_ipFAMCondition;

	// Indicates whether the page count check should be skipped when queuing files.
	bool m_bSkipPageCount;

	// Action Name being supplied
	string m_strAction;

	// Action ID being supplied
	long m_lActionId;

	// vector of thread data objects containing data for each of the processing threads, and a method
	// to release the memory allocated to the objects referenced by pointers in the vector
	vector<SupplierThreadData *> m_vecSupplyingThreadData;

	// a flag to indicate whether the file supplying role is enabled or not
	bool m_bEnabled;

	// dirty flag to indicate whether this object has been modified
	bool m_bDirty;

	IFileProcessingDB *m_pDB;

	// FAM TagManager pointer
	IFAMTagManager *m_pFAMTagManager;

	// File ActionManager pointer
	UCLID_FILEPROCESSINGLib::IRoleNotifyFAMPtr m_ipRoleNotifyFAM;
 
	// handle of UI window to which status update messages are sent
	HWND m_hWndOfUI;

	// Keep track of the number of Suppliers that have finished
	long m_nFinishedSupplierCount;

	// Keep track of the number of suppliers that have been enabled
	long m_nEnabledSupplierCount;

	// Stores the count of files that have been supplied and that have failed supplying
	volatile long m_nFilesSupplied;
	volatile long m_nSupplyingErrors;

	CMutex m_mutex;

	UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr getFileSupplierData(UCLID_FILEPROCESSINGLib::IFileSupplierPtr ipFS);

	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr getFPMDB();

	// Get FAM Tag Manager pointer for brief use
	UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr getFAMTagManager();

	// Return the number of enabled suppliers
	long getEnabledSupplierCount();
	
	void validateLicense();

	// this method returns true if the file matches the FAM condition
	bool fileMatchesFAMCondition(const string& strFile);

	// internal method to clear data
	void clear();

	void releaseSupplyingThreadDataObjects();
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To post the queue-event-received notification to the UI with the appropriate
	//			data
	void postQueueEventReceivedNotification(BSTR bstrFile, const string& strFSDescription,
		const string& strPriority, EFileSupplyingRecordType eFSRecordType);
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To post the queue-event-failed notification to the UI with the appropriate
	//			data
	void postQueueEventFailedNotification(BSTR bstrFile, const string& strFSDescription,
		const string& strPriority, EFileSupplyingRecordType eFSRecordType, const UCLIDException& ue);
	//---------------------------------------------------------------------------------------------

	// Gets the this pointer as a IFileActionMgmtRole Pointer
	UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr getThisAsFileActionMgmtRole(); 

	// Returns true if the database is in a bad state and call the stop method on the 
	// IFileActionMgmtRole interface
	// NOTE: This function does not throw exceptions. It will log any exceptions and return true;
	bool stopSupplingIfDBNotConnected();

	// thread procedure that executes each file supplier in a separate thread
	static UINT CFileSupplyingMgmtRole::fileSupplyingThreadProc(void* pData);

	// Function for logging queue event exceptions in the UI and in the file table
	void CFileSupplyingMgmtRole::handleFileSupplyingException(
		const string& eliCode,
		EFileSupplyingRecordType eFSRecordType,
		BSTR bstrFile,
		const string& strFSDescription,
		const string& strPriority);
};
//-------------------------------------------------------------------------------------------------

OBJECT_ENTRY_AUTO(__uuidof(FileSupplyingMgmtRole), CFileSupplyingMgmtRole)
