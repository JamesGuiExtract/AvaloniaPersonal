// TaskCondition.h : Declaration of the CTaskCondition

#pragma once
#include "resource.h"       // main symbols
#include "ESSkipConditions.h"
#include "IdentifiableObject.h"
#include "..\..\Code\FPCategories.h"

#include <string>
using namespace std;

////////////////////////////////////////////////////////////////////////////////////////////////////
// CTaskCondition
////////////////////////////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CTaskCondition :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTaskCondition, &CLSID_TaskCondition>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<ITaskCondition, &IID_ITaskCondition, &LIBID_EXTRACT_FAMCONDITIONSLib>,
	public IDispatchImpl<IFAMCondition, &IID_IFAMCondition, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<IFAMCancelable, &IID_IFAMCancelable, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<IFAMProcessingResult, &IID_IFAMProcessingResult, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<IInitClose, &IID_IInitClose, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public ISpecifyPropertyPagesImpl<CTaskCondition>,
	public IDispatchImpl<IParallelizableTask, &__uuidof(IParallelizableTask), &LIBID_UCLID_FILEPROCESSINGLib, /* wMajor = */ 1>,
	public IDispatchImpl<IIdentifiableObject, &IID_IIdentifiableObject, &LIBID_UCLID_COMUTILSLib>,
	public CIdentifiableObject
{
public:
	CTaskCondition();
	~CTaskCondition();
	
	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_TASKCONDITION)

	BEGIN_COM_MAP(CTaskCondition)
		COM_INTERFACE_ENTRY(ITaskCondition)
		COM_INTERFACE_ENTRY2(IDispatch, ITaskCondition)
		COM_INTERFACE_ENTRY(IFAMCondition)
		COM_INTERFACE_ENTRY(IFAMCancelable)
		COM_INTERFACE_ENTRY(IFAMProcessingResult)
		COM_INTERFACE_ENTRY(IInitClose)
		COM_INTERFACE_ENTRY(IAccessRequired)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
		COM_INTERFACE_ENTRY(IParallelizableTask)
		COM_INTERFACE_ENTRY(IIdentifiableObject)
	END_COM_MAP()

	BEGIN_PROP_MAP(CTaskCondition)
		PROP_PAGE(CLSID_TaskConditionPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CTaskCondition)
		IMPLEMENTED_CATEGORY(CATID_FP_FAM_CONDITIONS)
	END_CATEGORY_MAP()

// ITaskCondition
	STDMETHOD(get_Task)(IFileProcessingTask** ppVal);
	STDMETHOD(put_Task)(IFileProcessingTask* pNewVal);
	STDMETHOD(get_LogExceptions)(VARIANT_BOOL* pVal);
	STDMETHOD(put_LogExceptions)(VARIANT_BOOL newVal);

// IFAMCondition
	STDMETHOD(raw_FileMatchesFAMCondition)(IFileRecord* pFileRecord, IFileProcessingDB* pFPDB, 
		long lActionID, IFAMTagManager* pFAMTM, VARIANT_BOOL* pRetVal);

// IAccessRequired
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID* pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream* pStm);
	STDMETHOD(Save)(IStream* pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER* pcbSize);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown** pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown* pObject);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR* pstrComponentDescription);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL* pbValue);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

	// IParallelizableTask Methods
	STDMETHOD(raw_ProcessWorkItem)(IWorkItemRecord *pWorkItem, long nActionID,
		IFAMTagManager* pFAMTM,  IFileProcessingDB* pDB, IProgressStatus *pProgressStatus);
	STDMETHOD(get_Parallelize)(VARIANT_BOOL *pVal);
	STDMETHOD(put_Parallelize)(VARIANT_BOOL newVal);
	
	// IIdentifiableObject
	STDMETHOD(get_InstanceGUID)(GUID *pVal);

	// IFAMCancelable
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_IsCanceled)(VARIANT_BOOL *pvbCanceled);

	// IInitClose
	STDMETHOD(raw_Init)(long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB* pDB,
			IFileRequestHandler* pFileRequestHandler);
	STDMETHOD(raw_Close)();

	// IFAMProcessingResult
	STDMETHOD(raw_GetResult)(EFileProcessingResult* pResult);

private:
	/////////////////
	// Variables
	/////////////////

	bool m_bDirty;

	// Should we log exceptions thrown by the configured task?
	bool m_bLogExceptions;

	// The task that should be run to evaluate whether this condition succeeds or fails.
	IFileProcessingTaskPtr m_ipTask;

	// Executor to run the configured task
	IFileProcessingTaskExecutorPtr m_ipFAMTaskExecutor;

	// Used to indicate that there was a cancel this should be set in the FileMatchesFAMCondition method
	bool m_bCanceled;

	// Flag to indicate that cancel has been requested
	bool m_bCancelRequested;

	// Result of the processFile call for the on the task
	EFileProcessingResult m_eTaskResult;

	/////////////////
	// Methods
	/////////////////

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(TaskCondition), CTaskCondition)
