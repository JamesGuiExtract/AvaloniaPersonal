// FileProcessingTaskExecutor.h : Declaration of the CFileProcessingTaskExecutor

#pragma once
#include "resource.h"       // main symbols
#include "UCLIDFileProcessing.h"
#include "Win32Event.h"

#include <UCLIDException.h>

#include <string>
#include <vector>
#include <map>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CFileProcessingTaskExecutor
/////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CFileProcessingTaskExecutor :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFileProcessingTaskExecutor, &CLSID_FileProcessingTaskExecutor>,
	public ISupportErrorInfo,
	public IDispatchImpl<IFileProcessingTaskExecutor, &IID_IFileProcessingTaskExecutor, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CFileProcessingTaskExecutor();
	~CFileProcessingTaskExecutor();

DECLARE_REGISTRY_RESOURCEID(IDR_FILEPROCESSINGTASKEXECUTOR)

BEGIN_COM_MAP(CFileProcessingTaskExecutor)
	COM_INTERFACE_ENTRY(IFileProcessingTaskExecutor)
	COM_INTERFACE_ENTRY2(IDispatch,IFileProcessingTaskExecutor)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

// IFileProcessingTaskExecutor
	STDMETHOD(Init)(IIUnknownVector *pFileProcessingTasks, long nActionID, IFileProcessingDB *pDB,
		IFAMTagManager *pFAMTagManager, IFileRequestHandler* pFileRequestHandler);
	STDMETHOD(ProcessFile)(IFileRecord* pFileRecord, long nActionID,
		IProgressStatus *pProgressStatus, VARIANT_BOOL vbCancelRequested,
		EFileProcessingResult* pResult);
	STDMETHOD(InitProcessClose)(IFileRecord* pFileRecord, IIUnknownVector *pFileProcessingTasks, 
		long nActionID, IFileProcessingDB *pDB, IFAMTagManager *pFAMTagManager,
		IFileRequestHandler* pFileRequestHandler, IProgressStatus *pProgressStatus,
		VARIANT_BOOL bCancelRequested, EFileProcessingResult* pResult);
	STDMETHOD(Cancel)();
	STDMETHOD(Close)();
	STDMETHOD(GetCurrentTask)(IFileProcessingTask ** ppCurrentTask);
	STDMETHOD(get_IsInitialized)(VARIANT_BOOL *pVal);
	STDMETHOD(Standby)(VARIANT_BOOL *pVal);
	STDMETHOD(EndStandby)();
	
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	//////////////////
	// Helper classes
	//////////////////

	// Represents a File processing task (decomposed from IObjectWithDescription)
	class ProcessingTask
	{
	public:
		// The task
		UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr Task;

		// The description
		string Description;

		// Whether the task is enabled or not
		bool Enabled;

		ProcessingTask(const UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr& ipTask,
			const string& strDescription, bool bEnabled) : Task(ipTask),
			Description(strDescription), Enabled(bEnabled)
		{
		}

		~ProcessingTask()
		{
			try
			{
				// Clear the task
				Task = NULL;
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI26768");
		}
	};

	// In the event that the pending queue is emptied but processing is configured to continue until
	// the next document is queued, this class manages separate threads used to notify the processing
	// threads to standby.
	class StandbyThread : public CWinThread
	{
	public:
		StandbyThread(Win32Event& eventCancelProcessing,
			UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipFileProcessingTask);
		~StandbyThread();

		// Initialized the instance
		BOOL InitInstance();

		// The main code for the standby thread.
		int Run();

		// This should be called to end standby if another file is supplied or if processing is
		// stopped. After this call, the thread will no longer be able to signal for processing to
		// stop.
		// The thread will be guaranteed to remain alive until endStandby is called, but may end and
		// self-delete at any time following this call.
		void endStandby();

		// The event that should be fired if one of the processing tasks requests for processing to
		// stop.
		Win32Event& m_eventCancelProcessing;

		// m_eventStandbyEnding is signaled when endStandby is called.
		Win32Event m_eventStandbyEnding;

		// m_eventStandbyEnded is signaled once the thread has ended and endStandby call is
		// complete. Note: the main thread will not go out of scope until m_eventEndStandbyEnded is
		// set.
		Win32Event m_eventStandbyEnded;

		// m_eventEndStandbyEnded is signaled once the endStandby call has ended and the main thread
		// is clear to go out of scope.
		Win32Event m_eventEndStandbyEnded;

		UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr m_ipFileProcessingTask;
	};

	///////////
	//Variables
	//////////
	
	Win32Event m_eventCancelRequested;
	CCriticalSection m_criticalSection;
	CCriticalSection m_criticalSectionCurrentTask;

	// Signaled if standby mode has ended and the executor should not longer wait on any standby
	// calls that are blocking.
	Win32Event m_eventEndStandby;

	// Signaled after the standby thread is initialized and is available to be ended.
	Win32Event m_eventStandbyRunning;

	// List of tasks that will be used to process a file
	vector< unique_ptr<ProcessingTask> > m_vecProcessingTasks;

	// The task that is currently executing.  NULL if no task is executing.  NULL does not nessasarily mean all tasks
	// are done processing, it just means that ProcessFile is not currently executing on any of the tasks
	UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr m_ipCurrentTask;

	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipDB;
	UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr m_ipFAMTagManager;
	UCLID_FILEPROCESSINGLib::IFileRequestHandlerPtr m_ipFileRequestHandler;
	
	// Text that should prepend any status message to be posted to a ProgressStatus object
	string m_strStatusMessagePrefix;

	// Maps workflow IDs to the corresponding workflow name.
	map<long, _bstr_t> m_mapWorkflowNames;
	
	// Indicates that a valid task list was provided and Init was called on every enabled task
	bool m_bInitialized;

	//////////
	//Methods
	/////////
	
	// Verify a valid processing task list exists and has been initialized
	void CFileProcessingTaskExecutor::verifyInitialization();
		
	void validateLicense();

	// Internal ProcessFile method
	EFileProcessingResult processFile(IFileRecord* pFileRecord,
		long nActionID, const IProgressStatusPtr& ipProgressStatus,
		bool bCancelRequested);

	// Internal Init method
	void init(const IIUnknownVectorPtr& ipFileProcessingTasks,
		long actionID,
		const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipDB,
		const UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr& ipFAMTagManager,
		const UCLID_FILEPROCESSINGLib::IFileRequestHandlerPtr& ipFileRequestHandler);

	// Internal GetCurrentTask method
	UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr getCurrentTask();

	// Internal Close method
	void close();

	// Returns the count of enabled file processing tasks
	long countEnabledTasks();

	// Gets the name of the workflow with the specified ID. The name will be cached and would not
	// reflect any changes to the workflow name in the midst of processing were they to occur.
	_bstr_t getWorkflowName(long nWorkflowID);
};
