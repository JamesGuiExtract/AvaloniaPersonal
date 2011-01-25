// FileProcessingTaskExecutor.h : Declaration of the CFileProcessingTaskExecutor

#pragma once
#include "resource.h"       // main symbols
#include "UCLIDFileProcessing.h"
#include "Win32Event.h"

#include <UCLIDException.h>

#include <string>
#include <vector>
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
		IFAMTagManager *pFAMTagManager);
	STDMETHOD(ProcessFile)(IFileRecord* pFileRecord, long nActionID,
		IProgressStatus *pProgressStatus, VARIANT_BOOL vbCancelRequested,
		EFileProcessingResult* pResult);
	STDMETHOD(InitProcessClose)(IFileRecord* pFileRecord, IIUnknownVector *pFileProcessingTasks, 
		long nActionID, IFileProcessingDB *pDB, IFAMTagManager *pFAMTagManager,
		IProgressStatus *pProgressStatus, VARIANT_BOOL bCancelRequested,
		EFileProcessingResult* pResult);
	STDMETHOD(Cancel)();
	STDMETHOD(Close)();
	STDMETHOD(GetCurrentTask)(IFileProcessingTask ** ppCurrentTask);
	STDMETHOD(get_IsInitialized)(VARIANT_BOOL *pVal);
	
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	///////////
	// Helper class
	///////////
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

	///////////
	//Variables
	//////////
	
	Win32Event m_eventCancelRequested;
	CMutex m_mutex;
	CMutex m_mutexCurrentTask;

	// List of tasks that will be used to process a file
	vector<ProcessingTask> m_vecProcessingTasks;

	// The task that is currently executing.  NULL if no task is executing.  NULL does not nessasarily mean all tasks
	// are done processing, it just means that ProcessFile is not currently executing on any of the tasks
	UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr m_ipCurrentTask;

	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipDB;
	UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr m_ipFAMTagManager;
	
	// Text that should prepend any status message to be posted to a ProgressStatus object
	string m_strStatusMessagePrefix;
	
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
		const UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr& ipFAMTagManager);

	// Internal GetCurrentTask method
	UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr getCurrentTask();

	// Internal Close method
	void close();

	// Returns the count of enabled file processing tasks
	long countEnabledTasks();
};
