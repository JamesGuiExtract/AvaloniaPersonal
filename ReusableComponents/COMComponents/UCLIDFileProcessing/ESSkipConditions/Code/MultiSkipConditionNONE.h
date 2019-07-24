// MultiFAMConditionNONE.h : Declaration of the CMultiFAMConditionNONE

#pragma once

#include "resource.h"       // main symbols
#include "..\..\Code\FPCategories.h"
#include "ESSkipConditions.h"
#include "GenericMultiSkipCondition.h"

/////////////////////////////////////////////////////////////////////////////
// CMultiFAMConditionNONE
class ATL_NO_VTABLE CMultiFAMConditionNONE : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMultiFAMConditionNONE, &CLSID_MultiFAMConditionNONE>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IFAMCondition, &IID_IFAMCondition, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<IPaginationCondition, & IID_IPaginationCondition, & LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<IFAMCancelable, &IID_IFAMCancelable, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<IFAMProcessingResult, &IID_IFAMProcessingResult, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<IInitClose, &IID_IInitClose, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMultipleObjectHolder, &IID_IMultipleObjectHolder, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IParallelizableTask, &__uuidof(IParallelizableTask), &LIBID_UCLID_FILEPROCESSINGLib, /* wMajor = */ 1>,
	public ISpecifyPropertyPagesImpl<CMultiFAMConditionNONE>
{
public:
	CMultiFAMConditionNONE();
	~CMultiFAMConditionNONE();

DECLARE_REGISTRY_RESOURCEID(IDR_MULTIFAMCONDITIONNONE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CMultiFAMConditionNONE)
	COM_INTERFACE_ENTRY(IFAMCondition)
	COM_INTERFACE_ENTRY2(IDispatch, IFAMCondition)
	COM_INTERFACE_ENTRY(IPaginationCondition)
	COM_INTERFACE_ENTRY(IFAMCancelable)
	COM_INTERFACE_ENTRY(IFAMProcessingResult)
	COM_INTERFACE_ENTRY(IInitClose)
	COM_INTERFACE_ENTRY(IAccessRequired)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IMultipleObjectHolder)
	COM_INTERFACE_ENTRY(IParallelizableTask)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CMultiFAMConditionNONE)
	PROP_PAGE(CLSID_MultipleObjSelectorPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CMultiFAMConditionNONE)
	IMPLEMENTED_CATEGORY(CATID_FP_FAM_CONDITIONS)
	IMPLEMENTED_CATEGORY(CATID_FP_PAGINATION_CONDITIONS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IFAMCondition
	STDMETHOD(raw_FileMatchesFAMCondition)(IFileRecord* pFileRecord, IFileProcessingDB* pFPDB, 
		long lActionID, IFAMTagManager* pFAMTM, VARIANT_BOOL* pRetVal);

// IPaginationCondition
	STDMETHOD(get_IsPaginationCondition)(VARIANT_BOOL* pbIsPaginationCondition);
	STDMETHOD(put_IsPaginationCondition)(VARIANT_BOOL bIsPaginationCondition);

	STDMETHOD(raw_FileMatchesPaginationCondition)(IFileRecord* pSourceFileRecord,
		BSTR bstrProposedFileName, BSTR bstrDocumentStatus, BSTR bstrSerializedDocumentAttributes,
		IFileProcessingDB* pFPDB, long lActionID, IFAMTagManager* pFAMTagManager,
		VARIANT_BOOL* pRetVal);

// IAccessRequired
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR* pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_CopyFrom)(IUnknown* pObject);
	STDMETHOD(raw_Clone)(IUnknown** pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL* pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID* pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream* pStm);
	STDMETHOD(Save)(IStream* pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER* pcbSize);

// IMultipleObjectHolder
	STDMETHOD(raw_GetObjectCategoryName)(BSTR* pstrCategoryName);
	STDMETHOD(get_ObjectsVector)(IIUnknownVector** pVal);
	STDMETHOD(put_ObjectsVector)(IIUnknownVector *newVal);
	STDMETHOD(raw_GetObjectType)(BSTR* pstrObjectType);
	STDMETHOD(raw_GetRequiredIID)(IID *riid);
	
	// IFAMCancelable
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_IsCanceled)(VARIANT_BOOL *pvbCanceled);

	// IParallelizableTask Methods
	STDMETHOD(raw_ProcessWorkItem)(IWorkItemRecord *pWorkItem, long nActionID,
		IFAMTagManager* pFAMTM,  IFileProcessingDB* pDB, IProgressStatus *pProgressStatus);
	STDMETHOD(get_Parallelize)(VARIANT_BOOL *pVal);
	STDMETHOD(put_Parallelize)(VARIANT_BOOL newVal);

	// IInitClose
	STDMETHOD(raw_Init)(long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB* pDB,
			IFileRequestHandler* pFileRequestHandler);
	STDMETHOD(raw_Close)();

	// IFAMProcessingResult
	STDMETHOD(raw_GetResult)(EFileProcessingResult* pResult);

private:
	/////////////
	// Methods
	/////////////
	void validateLicense();
	
	// The task that should be run to evaluate whether this condition succeeds or fails.
	EXTRACT_FAMCONDITIONSLib::IGenericMultiFAMConditionPtr getGenericMultiFAMCondition();

	/////////////
	// Variables
	/////////////
	// A pointer to IGenericMultiFAMCondition object
	// the IGenericMultiFAMCondition will do the real work and 
	// it contains an IIUnknownVectorPtr contain the same FAM conditions.
	EXTRACT_FAMCONDITIONSLib::IGenericMultiFAMConditionPtr m_ipGenericMultiFAMCondition;

	// each entry in the vector below is expected to be of type 
	// IObjectWithDescription and the object contained therein is expected to 
	// be of type IFAMCondition
	IIUnknownVectorPtr m_ipMultiFAMConditions;

	// Used to allow PaginationTask to inform this condition when it is being used in the context
	// of the IPaginationCondition interface.
	bool m_bIsPaginationCondition;

	// flag to keep track of whether object is dirty
	bool m_bDirty;
};
