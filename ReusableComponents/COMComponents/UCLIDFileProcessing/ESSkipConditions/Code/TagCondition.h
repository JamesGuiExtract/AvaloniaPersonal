// TagCondition.h : Declaration of the CTagCondition

#pragma once
#include "resource.h"       // main symbols
#include "ESSkipConditions.h"
#include "..\..\Code\FPCategories.h"

////////////////////////////////////////////////////////////////////////////////////////////////////
// CTagCondition
////////////////////////////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CTagCondition :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTagCondition, &CLSID_TagCondition>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<ITagCondition, &IID_ITagCondition, &LIBID_EXTRACT_FAMCONDITIONSLib>,
	public IDispatchImpl<IFAMCondition, &IID_IFAMCondition, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public ISpecifyPropertyPagesImpl<CTagCondition>
{
public:
	CTagCondition();
	~CTagCondition();
	
	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_TAGCONDITION)

	BEGIN_COM_MAP(CTagCondition)
		COM_INTERFACE_ENTRY(ITagCondition)
		COM_INTERFACE_ENTRY2(IDispatch, ITagCondition)
		COM_INTERFACE_ENTRY(IFAMCondition)
		COM_INTERFACE_ENTRY(IAccessRequired)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

	BEGIN_PROP_MAP(CTagCondition)
		PROP_PAGE(CLSID_TagConditionPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CTagCondition)
		IMPLEMENTED_CATEGORY(CATID_FP_FAM_CONDITIONS)
	END_CATEGORY_MAP()

// ITagCondition
	STDMETHOD(get_ConsiderMet)(VARIANT_BOOL* pVal);
	STDMETHOD(put_ConsiderMet)(VARIANT_BOOL newVal);
	STDMETHOD(get_AnyTags)(VARIANT_BOOL* pVal);
	STDMETHOD(put_AnyTags)(VARIANT_BOOL newVal);
	STDMETHOD(get_Tags)(IVariantVector** ppVecTags);
	STDMETHOD(put_Tags)(IVariantVector* pVecTags);

// IFAMCondition
	STDMETHOD(raw_FileMatchesFAMCondition)(BSTR bstrFile, IFileProcessingDB* pFPDB, long lFileID, 
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

private:
	/////////////////
	// Variables
	/////////////////

	bool m_bDirty;

	// Whether to consider the condition met or unmet
	bool m_bConsiderMet;

	// If the file must contain all the tags or just one of the tags
	bool m_bAnyTags;

	// The vector of tags to compare
	IVariantVectorPtr m_ipVecTags;

	/////////////////
	// Methods
	/////////////////

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(TagCondition), CTagCondition)
