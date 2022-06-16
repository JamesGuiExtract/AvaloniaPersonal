// FileProcessingMgmtRole.h : Declaration of the CFileProcessingMgmtRole

#pragma once
#include "stdafx.h"
#include "FPRecordManager.h"
#include "resource.h"

#include <Win32CriticalSection.h>
#include <Win32Semaphore.h>
#include <Win32Event.h>
#include <UCLIDException.h>

#include <string>
#include <vector>

using namespace std;

class CFileProcessingMgmtRole;

//-------------------------------------------------------------------------------------------------
// StandbyThread class
//
// In the event that the pending queue is emptied but processing is configured to continue until
// the next document is queued, this class manages separate threads used to notify the processing
// threads to standby.
//-------------------------------------------------------------------------------------------------
class StandbyThread : public CWinThread
{
public:
	StandbyThread(Win32Event& eventCancelProcessing,
		const UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr& ipTaskExecutor);
	~StandbyThread();

	// Initialized the instance
	BOOL InitInstance();

	// The main code for the standby thread.
	int Run();

	// This should be called to end standby if another file is supplied or if processing is stopped.
	// After this call, the thread will no longer be able to signal for processing to stop.
	// The thread will be guaranteed to remain alive until endStandby is called, but may end and
	// self-delete at any time following this call.
	void endStandby();

	// The event that should be fired if one of the processing tasks requests for processing to stop.
	Win32Event& m_eventCancelProcessing;

	// m_eventStandbyEnded is signaled once the endStandby call is complete.
	Win32Event m_eventStandbyEnded;

	// The IFileProcessingTaskExecutor managing the processing tasks.
	UCLID_FILEPROCESSINGLib::IFileProcessingTaskExecutorPtr m_ipTaskExecutor;
};

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
// WorkItemThreadData class
//-------------------------------------------------------------------------------------------------
class WorkItemThreadData
{
public:
	WorkItemThreadData(CFileProcessingMgmtRole* pFPMgmtRole, long nActionID, Win32Semaphore &rSemaphore, 
		IFileProcessingDB *pDB);
	~WorkItemThreadData();

	CFileProcessingMgmtRole* m_pFPMgmtRole;
	long m_nActionID;
	IFileProcessingDB *m_pDB;
	Win32Semaphore &m_rSemaphore;

	Win32Event m_threadStartedEvent;
	Win32Event m_threadEndedEvent;
	static Win32Event ms_threadStopProcessing;
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
	public IDispatchImpl<IFileProcessingMgmtRole, &IID_IFileProcessingMgmtRole, &LIBID_UCLID_FILEPROCESSINGLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<IFileRequestHandler, &IID_IFileRequestHandler, &LIBID_UCLID_FILEPROCESSINGLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CFileProcessingMgmtRole();
	~CFileProcessingMgmtRole();

DECLARE_REGISTRY_RESOURCEID(IDR_FILEPROCESSINGMGMTROLE)

BEGIN_COM_MAP(CFileProcessingMgmtRole)
	COM_INTERFACE_ENTRY(IFileProcessingMgmtRole)
	COM_INTERFACE_ENTRY(IFileActionMgmtRole)
	COM_INTERFACE_ENTRY2(IDispatch, IFileProcessingMgmtRole)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAccessRequired)
	COM_INTERFACE_ENTRY(IFileRequestHandler)
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
	STDMETHOD(Start)(IFileProcessingDB* pDB, long lActionId, BSTR bstrAction, long hWndOfUI, 
		IFAMTagManager* pTagManager, IRoleNotifyFAM* pRoleNotifyFAM, BSTR bstrFpsFileName);
	STDMETHOD(Stop)(void);
	STDMETHOD(Pause)(void);
	STDMETHOD(Resume)(void);
	STDMETHOD(get_Enabled)(VARIANT_BOOL* pVal);
	STDMETHOD(put_Enabled)(VARIANT_BOOL newVal);
	STDMETHOD(Clear)(void);
	STDMETHOD(ValidateStatus)(void);

// IAccessRequired
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

// IFileRequestHandler
	STDMETHOD(CheckoutForProcessing)(long nFileID, VARIANT_BOOL vbAllowQueuedStatusOverride,
		EActionStatus* pPrevStatus, VARIANT_BOOL* pSucceeded);
	STDMETHOD(CheckoutNextFile)(VARIANT_BOOL vbAllowQueuedStatusOverride, long* pnFileID);
	STDMETHOD(GetNextCheckedOutFile)(long nAfterFileID, long* pnFileID);
	STDMETHOD(MoveToFrontOfProcessingQueue)(long nFileID, VARIANT_BOOL* pSucceeded);
	STDMETHOD(ReleaseFile)(long nFileID, VARIANT_BOOL* pSucceeded);
	STDMETHOD(SetFallbackStatus)(long nFileID, EActionStatus esFallbackStatus,
		VARIANT_BOOL* pSucceeded);
	STDMETHOD(PauseProcessingQueue)();
	STDMETHOD(ResumeProcessingQueue)();

// IFileProcessingMgmtRole
	STDMETHOD(get_FileProcessors)(IIUnknownVector** pVal);
	STDMETHOD(put_FileProcessors)(IIUnknownVector* newVal);
	STDMETHOD(get_NumThreads)(long* pVal);
	STDMETHOD(put_NumThreads)(long newVal);
	STDMETHOD(SetDirty)(VARIANT_BOOL newVal);
	STDMETHOD(SetRecordMgr)(void* pRecordMgr);
	STDMETHOD(get_OkToStopWhenQueueIsEmpty)(VARIANT_BOOL* pVal);
	STDMETHOD(put_OkToStopWhenQueueIsEmpty)(VARIANT_BOOL newVal);
	STDMETHOD(get_KeepProcessingAsAdded)(VARIANT_BOOL* pVal);
	STDMETHOD(put_KeepProcessingAsAdded)(VARIANT_BOOL newVal);
	STDMETHOD(get_LogErrorDetails)(VARIANT_BOOL* pVal);
	STDMETHOD(put_LogErrorDetails)(VARIANT_BOOL newVal);
	STDMETHOD(get_ErrorLogName)(BSTR* pVal);
	STDMETHOD(put_ErrorLogName)(BSTR newVal);
	STDMETHOD(get_ExecuteErrorTask)(VARIANT_BOOL* pVal);
	STDMETHOD(put_ExecuteErrorTask)(VARIANT_BOOL newVal);
	STDMETHOD(get_ErrorTask)(IObjectWithDescription** pVal);
	STDMETHOD(put_ErrorTask)(IObjectWithDescription* newVal);
	STDMETHOD(get_ProcessingSchedule)(IVariantVector** ppHoursSchedule);
	STDMETHOD(put_ProcessingSchedule)(IVariantVector* pHoursSchedule);
	STDMETHOD(get_LimitProcessingToSchedule)(VARIANT_BOOL* pbVal);
	STDMETHOD(put_LimitProcessingToSchedule)(VARIANT_BOOL bVal);
	STDMETHOD(ProcessSingleFile)(IFileRecord* pFileRecord, IFileProcessingDB* pFPDB,
		IFAMTagManager* pFAMTagManager);
	STDMETHOD(get_FPDB)(IFileProcessingDB** ppFPDB);
	STDMETHOD(put_FPDB)(IFileProcessingDB* pFPDB);
	STDMETHOD(get_SendErrorEmail)(VARIANT_BOOL* pVal);
	STDMETHOD(put_SendErrorEmail)(VARIANT_BOOL newVal);
	STDMETHOD(get_ErrorEmailTask)(IErrorEmailTask** pVal);
	STDMETHOD(put_ErrorEmailTask)(IErrorEmailTask* newVal);
	STDMETHOD(get_HasProcessingCompleted)(VARIANT_BOOL* pVal);
	STDMETHOD(get_ProcessingDisplaysUI)(VARIANT_BOOL* pProcessingDisplaysUI);
	STDMETHOD(get_QueueMode)(EQueueType* pVal);
	STDMETHOD(put_QueueMode)(EQueueType newVal);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID* pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream* pStm);
	STDMETHOD(Save)(IStream* pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER* pcbSize);

private:
	/////////////////
	// File Processors and associated settings
	/////////////////

	// Thread procedure which represents a single file processing thread
	static UINT CFileProcessingMgmtRole::fileProcessingThreadProc(void* pData);

	// Thread procedure which watches all the file processing threads
	// and waits for them to complete.  When they are done processing, then
	// some status updates are sent to the UI
	static UINT CFileProcessingMgmtRole::fileProcessingThreadsWatcherThread(void* pData);

	// Thread procedure which processes single work items
	static UINT CFileProcessingMgmtRole::workItemProcessingThreadProc(void *pData);

	// Thread procedure which initiates the stopping of processing asynchronously
	static UINT handleStopRequestAsynchronously(void* pData);

	// Thread procedure to handle starting and stopping of the processing. 
	// If scheduling is on, it will start and stop processing based on the schedule
	// otherwise it will start processing and wait for the event to stop processing.  
	// It also stops the processing on pause and restarts on resume.
	// If processing is ending because there are no more files to process or a manual stop
	// has been initiated the UI will be notified that processing is complete or stopped and then
	// this function will return.
	static UINT processManager(void* pData);

	// Thread procedure which handles the Init, ProcessFile and Close of a single file.
	static UINT processSingleFileThread(void *pData);

	/////////////
	// Variables
	/////////////

	// A flag to indicate the process in on going
	volatile bool m_bProcessing;

	// Indicates whether processing has completed.
	// NOTE: This method flag is set to false as processing is started and only indicates true
	// after at least one file has processed. If the process is configured to keep processing as
	// files are queued, but no files ever are, this flag will remain false.
	volatile bool m_bHasProcessingCompleted;

	// A flag to indicate a single file is currently being processed via ProcessSingleFile
	volatile bool m_bProcessingSingleFile;

	// The IFileRecord for the currently processing file via ProcessSingleFile (if any).
	UCLID_FILEPROCESSINGLib::IFileRecordPtr m_ipProcessingSingleFileRecord;

	// The FileProcessingRecord for the currently processing file via ProcessSingleFile (if any).
	unique_ptr<FileProcessingRecord> m_upProcessingSingleFileTask;

	// Event used to signal that processing should be resumed after a pause
	Win32Event m_eventResume;

	// Event used to signal that processing should be paused
	Win32Event m_eventPause;

	// list of file processors
	IIUnknownVectorPtr m_ipFileProcessingTasks;

	// The number of threads that will process files
	// simultaneously
	long m_nNumThreads;

	// vector of thread data objects containing data for each of the processing threads
	vector<ProcessingThreadData *> m_vecProcessingThreadData;

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
	string m_strAction;

	// Log error details to this file
	bool m_bLogErrorDetails;
	string m_strErrorLogFile;

	// Specifies email to be sent upon a task failure.
	bool m_bSendErrorEmail;
	IErrorEmailTaskPtr m_ipErrorEmailTask;

	// Execute this task if an error occurs ( depending on the built-in Enabled flag )
	IObjectWithDescriptionPtr m_ipErrorTask;

	EQueueType m_eQueueMode;

	// Enum used to indicate the current state of processing
	enum ERunningState{
		kNormalRun,
		kScheduleRun,
		kScheduleStop,
		kNormalStop,
		kPaused
	};
	
	// Flag to indicate that the processing should be limited by the scheduled hours in the
	// m_vecScheduledHours vector;
	bool m_bLimitProcessingToSchedule;

	// Vector that contains the schedule
	vector<bool> m_vecScheduledHours;

	// Variable to indicate the current running state of processing
	volatile ERunningState m_eCurrentRunningState;

	// Events used to indicate that the process manager thread has been started or it has exited
	Win32Event m_eventProcessManagerStarted;
	Win32Event m_eventProcessManagerExited;
	// Indicates whether the process manager thread is actively distributing files. Dictated by
	// PauseProcessingQueue/ResumeProcessingQueue.
	Win32Event m_eventProcessManagerActive;

	// Event to indicate that the processing is being stopped manually
	Win32Event m_eventManualStopProcessing;

	// Event to indicate that the thread watcher thread has exited.
	Win32Event m_eventWatcherThreadExited;

	// The name of the FPS file that is running (this value is set in the Start method)
	string m_strFpsFile;

	// contains the Semaphore used for parallelized task processing if it is created
	unique_ptr<Win32Semaphore> m_upParallelSemaphore;

	// vector to hold the data for the work item threads;
	vector<WorkItemThreadData *> m_vecWorkItemThreads;

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
	void processFiles2(ProcessingThreadData* pThreadData);

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
							   const ProcessingThreadData* pThreadData,
							   const UCLIDException &rUE);

	// Executes an IFileProcessingTask wrapped in an IObjectWithDescription as part of handling an
	// error in the specified task.
	void executeErrorTask(FileProcessingRecord &task,
		const ProcessingThreadData* pThreadData,
		IObjectWithDescriptionPtr fileProcessingTask);

	// Writes specially formatted error information to the specified text file.
	void writeErrorDetailsText(const string& strLogFile, const string& strSourceDocument, 
		const UCLIDException &rUE);

	// internal method to clear data
	void clear();

	// Returns the task to send an email as an error handler creating the object if needed.
	IErrorEmailTaskPtr getErrorEmailTask();

	// Returns error-handling task, creating the object if needed
	IObjectWithDescriptionPtr getErrorHandlingTask();

	// Returns true if error-handling task is both defined and enabled, otherwise false
	bool isErrorHandlingTaskEnabled();

	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr getFPMDB();

	// Get FAM Tag Manager pointer for brief use
	UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr getFAMTagManager();

	UCLID_FILEPROCESSINGLib::IFileRequestHandlerPtr getFileRequestHandler();

	void releaseProcessingThreadDataObjects();

	void releaseWorkItemThreadDataObjects();

	// this method will return a new IIUnknownVector of FileProcessingTasks with the same number
	// and type of processors as the input.  The FileProcessingTasks in the output will
	// be copies of their input counterpart
	IIUnknownVectorPtr copyFileProcessingTasks(IIUnknownVectorPtr ipFileProcessingTasks);

	// Return the action ID from name, return 0 if the name doesn't exist in DB
	DWORD getActionID(const string & strAct);
	
	// A method to return the count of enabled file processors in a IIUnknownVector of 
	// IObjectWithDescription of IFileProcessingTask objects
	long getEnabledFileProcessingTasksCount(IIUnknownVectorPtr ipFileProcessingTasks) const;

	// Calls NotifyStopRequested for all of the processors
	void notifyFileProcessingTasksOfStopRequest();

	// Method used to start processing, used by processManager 
	// if bDontStartThreads is false the processing threads are started
	// if it is true everything is setup as if it was running but the threads are not started
	void startProcessing(bool bDontStartThreads = false);

	// Method stops the processing threads but does not send any notification to the UI that 
	// processing is complete or stopped (that is handled in processManager method)
	void stopProcessing();

	// Returns the number of milliseconds that should pass before the next change from 
	// running to stopped or stopped to running.  
	// The argument eNextRunningState will be:
	//		kNormalRun - if not limiting to schedule or all scheduled times are running, 
	//			returns INFINITE
	//		kNormalStop - if all scheduled times are stopped, returns 0
	//		kScheduleRun - if next schedule change is to running, returns number of milliseconds
	//			to wait for that change
	//		kScheduleStop - if next schedule change is to stopped, returns number of milliseconds
	//			to wait for that change
	unsigned long timeTillNextProcessingChange(ERunningState &eNextRunningState);

	// Gets the stack size for any processing thread based on the MinStackSize value for all
	// FileProcessingTasks to be run.
	unsigned long getProcessingThreadStackSize();

	// Creates the sempaphores used for processing, before processing the workItem thread or 
	// file processing thread needs to get this semaphore. The nNumberOfCounts will be the number
	// threads (workItem or fileProcessing) that can process at the same time
	void createProcessingSemaphore(long nNumberOfCounts);

	// Checks all task for a parallelizable task, if one is found, sets up the m_vecWorkItemThreadData
	// for initializing the threads and returns true, if no tasks are parallelizable returns false
	bool setupWorkItemThreadData(long nNumberOfThreads, long lActionID);

	// Starts workItemThreads using the contents of the m_vecWorkItemThreadData. If this vector is 
	// empty no threads will be created
	void startWorkItemThreads(unsigned long ulStackSize);

	void signalWorkItemThreadsToStopAndWait();
	
	void validateLicense();
};
//-------------------------------------------------------------------------------------------------

OBJECT_ENTRY_AUTO(__uuidof(FileProcessingMgmtRole), CFileProcessingMgmtRole)