// ConditionalTask.h : Declaration of the CConditionalTask

#pragma once

#include "resource.h"       // main symbols
#include "FileProcessors.h"
#include "..\..\..\UCLIDFileProcessing\Code\FPCategories.h"

#include <map>

using namespace std;

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

//--------------------------------------------------------------------------------------------------
// CConditionalTask
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CConditionalTask :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CConditionalTask, &CLSID_ConditionalTask>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IConditionalTask, &IID_IConditionalTask, &LIBID_UCLID_FILEPROCESSORSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<IFileProcessingTask, &__uuidof(IFileProcessingTask), &LIBID_UCLID_FILEPROCESSINGLib, /* wMajor = */ 1>,
	public IDispatchImpl<ICategorizedComponent, &__uuidof(ICategorizedComponent), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ICopyableObject, &__uuidof(ICopyableObject), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<IClipboardCopyable, &__uuidof(IClipboardCopyable), &LIBID_UCLID_COMUTILSLib, 1>,
	public IDispatchImpl<IMustBeConfiguredObject, &__uuidof(IMustBeConfiguredObject), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>,
	public ISpecifyPropertyPagesImpl<CConditionalTask>
{
public:
	CConditionalTask();
	~CConditionalTask();

	DECLARE_REGISTRY_RESOURCEID(IDR_CONDITIONALTASK)

	BEGIN_COM_MAP(CConditionalTask)
		COM_INTERFACE_ENTRY(IConditionalTask)
		COM_INTERFACE_ENTRY2(IDispatch, IConditionalTask)
		COM_INTERFACE_ENTRY(IFileProcessingTask)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(IClipboardCopyable)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

	BEGIN_PROP_MAP(CConditionalTask)
		PROP_PAGE(CLSID_ConditionalTaskPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CConditionalTask)
		IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
	END_CATEGORY_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()
	HRESULT FinalConstruct();
	void FinalRelease();

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR *pstrComponentDescription);
	
// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IClipboardCopyable
	STDMETHOD(raw_NotifyCopiedFromClipboard)();

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IConditionalTask
	STDMETHOD(get_FAMCondition)(IObjectWithDescription ** pVal);
	STDMETHOD(put_FAMCondition)(IObjectWithDescription * newVal);
	STDMETHOD(get_TasksForConditionTrue)(IIUnknownVector ** pVal);
	STDMETHOD(put_TasksForConditionTrue)(IIUnknownVector * newVal);
	STDMETHOD(get_TasksForConditionFalse)(IIUnknownVector ** pVal);
	STDMETHOD(put_TasksForConditionFalse)(IIUnknownVector * newVal);

// IFileProcessingTask
	STDMETHOD(raw_Init)(long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB *pDB);
	STDMETHOD(raw_ProcessFile)(BSTR bstrFileFullName, long nFileID, long nActionID,
		IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
		VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult);
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_Close)();

private:

	//////////////
	// Data
	//////////////

	// FAM Condition
	IObjectWithDescriptionPtr m_ipFAMCondition;

	// Collected OWD tasks to be executed if condition is True
	IIUnknownVectorPtr m_ipTasksForTrue;

	// Collected OWD tasks to be executed if condition is False
	IIUnknownVectorPtr m_ipTasksForFalse;

	// Executor utility to process conditional tasks
	IFileProcessingTaskExecutorPtr m_ipFAMTaskExecutor;

	IMiscUtilsPtr m_ipMiscUtils;

	bool	m_bDirty;

	/////////////
	// Methods
	/////////////

	// Creates the MiscUtils object if necessary and returns it
	IMiscUtilsPtr getMiscUtils();

	// Iterates the list of tasks searching for IClipboardCopyable items and calls
	// NotifyCopiedFromClipboard on each one
	void notifyClipboardCopiedForTask(const IIUnknownVectorPtr& ipTasks);

	void	validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(ConditionalTask), CConditionalTask)
