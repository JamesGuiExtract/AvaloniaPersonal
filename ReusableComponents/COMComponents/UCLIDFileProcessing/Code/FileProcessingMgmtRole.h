// FileProcessingMgmtRole.h : Declaration of the CFileProcessingMgmtRole

#pragma once
#include "FPRecordManager.h"
#include "resource.h"

#include <Win32CriticalSection.h>
#include <Win32Event.h>
#include <UCLIDException.h>

#include <string>
#include <vector>

class CFileProcessingMgmtRole;

//-------------------------------------------------------------------------------------------------
// ProcessingThreadData class
//-------------------------------------------------------------------------------------------------
class ProcessingThreadData
{
public:
	ProcessingThreadData();	
	~ProcessingThreadData();

	Win32CriticalSection m_cs;

	CFileProcessingMgmtRole* m_pFPMgmtRole;
	Win32Event m_threadStartedEvent;
	Win32Event m_threadEndedEvent;
	CWinThread* m_pThread;
	UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr m_ipTaskExecutor;
	UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr m_ipErrorTaskExecutor;
};

//-------------------------------------------------------------------------------------------------
// CFileProcessingMgmtRole
//-------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CFileProcessingMgmtRole :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFileProcessingMgmtRole, &CLSID_FileProcessingMgmtRole>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IFileActionMgmtRole, &IID_IFileActionMgmtRole, &LIBID_UCLID_FILEPROCESSINGLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IFileProcessingMgmtRole, &IID_IFileProcessingMgmtRole, &LIBID_UCLID_FILEPROCESSINGLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CFileProcessingMgmtRole();
	~CFileProcessingMgmtRole();

DECLARE_REGISTRY_RESOURCEID(IDR_FILEPROCESSINGMGMTROLE)

BEGIN_COM_MAP(CFileProcessingMgmtRole)
	COM_INTERFACE_ENTRY(IFileProcessingMgmtRole)
	COM_INTERFACE_ENTRY(IFileActionMgmtRole)
	// TODO: why does the following line cause a compile error here, but similar code in other classes don't?
	//COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY2(IDispatch, IFileProcessingMgmtRole)
	COM_INTERFACE_ENTRY(IPersistStream)
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
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IFileActionMgmtRole
	STDMETHOD(Start)(IFileProcessingDB *pDB, BSTR bstrAction, long hWndOfUI, IFAMTagManager *pTagManager, 
		IRoleNotifyFAM *pRoleNotifyFAM);
	STDMETHOD(Stop)(void);
	STDMETHOD(Pause)(void);
	STDMETHOD(Resume)(void);
	STDMETHOD(get_Enabled)(VARIANT_BOOL* pVal);
	STDMETHOD(put_Enabled)(VARIANT_BOOL newVal);
	STDMETHOD(Clear)(void);
	STDMETHOD(ValidateStatus)(void);

// IFileProcessingMgmtRole
	STDMETHOD(get_FileProcessors)(IIUnknownVector ** pVal);
	STDMETHOD(put_FileProcessors)(IIUnknownVector * newVal);
	STDMETHOD(get_NumThreads)(long *pVal);
	STDMETHOD(put_NumThreads)(long newVal);
	STDMETHOD(SetDirty)(VARIANT_BOOL newVal);
	STDMETHOD(SetRecordMgr)(void *pRecordMgr);
	STDMETHOD(get_OkToStopWhenQueueIsEmpty)(VARIANT_BOOL *pVal);
	STDMETHOD(put_OkToStopWhenQueueIsEmpty)(VARIANT_BOOL newVal);
	STDMETHOD(get_KeepProcessingAsAdded)(VARIANT_BOOL* pVal);
	STDMETHOD(put_KeepProcessingAsAdded)(VARIANT_BOOL newVal);
	STDMETHOD(get_LogErrorDetails)(VARIANT_BOOL* pVal);
	STDMETHOD(put_LogErrorDetails)(VARIANT_BOOL newVal);
	STDMETHOD(get_ErrorLogName)(BSTR *pVal);
	STDMETHOD(put_ErrorLogName)(BSTR newVal);
	STDMETHOD(get_ExecuteErrorTask)(VARIANT_BOOL* pVal);
	STDMETHOD(put_ExecuteErrorTask)(VARIANT_BOOL newVal);
	STDMETHOD(get_ErrorTask)(IObjectWithDescription * *pVal);
	STDMETHOD(put_ErrorTask)(IObjectWithDescription * newVal);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	/////////////////
	// File Processors and associated settings
	/////////////////

	// Thread procedure which represents a single file processing thread
	static UINT CFileProcessingMgmtRole::fileProcessingThreadProc(void *pData);

	// Thread procedure which watches all the file processing threads
	// and waits for them to complete.  When they are done processing, then
	// some status updates are sent to the UI
	static UINT CFileProcessingMgmtRole::fileProcessingThreadsWatcherThread(void *pData);

	// Thread procedure which initiates the stopping of processing asynchronously
	static UINT handleStopRequestAsynchronously(void *pData);

	/////////////
	// Variables
	/////////////

	// a flag to indicate the process in on going
	volatile bool m_bProcessing;

	// processing should wait until this is signaled
	Win32Event m_eventResume;

	// list of file processors
	IIUnknownVectorPtr m_ipFileProcessingTasks;

	// The number of threads that will process files
	// simultaneously
	long m_nNumThreads;

	// vector of thread data objects containing data for each of the processing threads
	std::vector<ProcessingThreadData *> m_vecProcessingThreadData;

	// a flag to indicate whether the file Processing role is enabled or not
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

	// This mutex should be acquired before attempting to lock all
	// of the semaphore counts that are available for the m_threadDataSemaphore
	// to prevent a deadlock when 2 methods want all semaphore counts and only one 
	// is available.
	CMutex m_threadLock;

	// This semaphore will have 2 counts. Both counts need to be aquired
	// when thread data is being added to the m_vecProcessingThreadData
	// or when that data is being cleaned up.
	// One count should be aquired to use that thread data.  This
	// keeps data from being deleted while it is in use in 2 locations
	CSemaphore m_threadDataSemaphore;

	FPRecordManager* m_pRecordMgr;

	// This flag indicates that processing should continue until the stop
	// button is pressed
	bool m_bKeepProcessingAsAdded;

	// it is ok to stop if the queue is empty
	// this means no supplying is going on so the m_bKeepProcessingAsAdded determine
	// whether to keep processing
	bool m_bOkToStopWhenQueueIsEmpty;

	// Action Name being processed
	std::string m_strAction;

	// Log error details to this file
	bool m_bLogErrorDetails;
	std::string m_strErrorLogFile;

	// Execute this task if an error occurs ( depending on the built-in Enabled flag )
	IObjectWithDescriptionPtr m_ipErrorTask;

	///////////
	// Methods
	///////////

	//----------------------------------------------------------------------------------------------
	UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr getThisAsCOMPtr();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To start the file processing, launch X number of threads to
	//			process files in parallel, and wait for them to complete their work
	//			or for the user to cancel the operation.
	void processFiles();
	//---------------------------------------------------------------------------------------------
	// PURPOSE:	This method represents each thread that is running in parallel to 
	//			process files.
	//			Some of the file processors cannot be safely run in multiple threads
	//			simultaneously.  So, we sometimes pass a copy of certain file 
	//			processors to each thread.  ripFileProcessingTasks represent the 
	//			file processor objects to be used for the current thread.
	void processFiles2(ProcessingThreadData *pThreadData);

	void processTask(FileProcessingRecord& task, ProcessingThreadData* pThreadData);

	// This method will sequentially execute all enabled file processors beginning at the
	// index position nFileProcNum.  If any problems occur during the processing, an exception
	// will be thrown.  If no exception was thrown, the return value is to be interpreted as follows:
	//		1) A return value of true means that the file was successfully processed by all the 
	//		enabled file processors beginning at the index nFileProcNum.
	//		2) A return value of false means that the file was not processed because the user
	//		requested the processing to stop.
	EFileProcessingResult startFileProcessingChain(FileProcessingRecord& task,
		ProcessingThreadData* pThreadData);

	// Handles logging of error details if this option is enabled.
	// Handles execution of the error-handling task if this option is enabled.
	void handleProcessingError(FileProcessingRecord &task, 
							   const ProcessingThreadData *pThreadData,
							   const UCLIDException &rUE);

	// Writes specially formatted error information to the specified text file.
	void writeErrorDetailsText(const string& strLogFile, const string& strSourceDocument, 
		const UCLIDException &rUE);

	// internal method to clear data
	void clear();

	// Returns error-handling task, creating the object if needed
	IObjectWithDescriptionPtr getErrorHandlingTask();

	// Returns true if error-handling task is both defined and enabled, otherwise false
	bool isErrorHandlingTaskEnabled();

	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr getFPMDB();

	// Get FAM Tag Manager pointer for brief use
	UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr getFAMTagManager();

	void releaseProcessingThreadDataObjects();

	// this method will return a new IIUnknownVector of FileProcessingTasks with the same number
	// and type of processors as the input.  The FileProcessingTasks in the output will
	// be copies of their input counterpart
	IIUnknownVectorPtr copyFileProcessingTasks(IIUnknownVectorPtr ipFileProcessingTasks);

	// Return the action ID from name, return 0 if the name doesn't exist in DB
	DWORD getActionID(const std::string & strAct);
	
	// A method to return the count of enabled file processors in a IIUnknownVector of 
	// IObjectWithDescription of IFileProcessingTask objects
	long getEnabledFileProcessingTasksCount(IIUnknownVectorPtr ipFileProcessingTasks) const;

	// Calls NotifyStopRequested for all of the processors
	void notifyFileProcessingTasksOfStopRequest();

	void validateLicense();
};
//-------------------------------------------------------------------------------------------------

OBJECT_ENTRY_AUTO(__uuidof(FileProcessingMgmtRole), CFileProcessingMgmtRole)