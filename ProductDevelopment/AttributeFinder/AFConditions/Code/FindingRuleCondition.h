// FindingRuleCondition.h : Declaration of the CFindingRuleCondition

#pragma once
#include "resource.h"       // main symbols
#include "AFConditions.h"

#include <AFCategories.h>

/////////////////////////////////////////////////////////////////////////////
// CFindingRuleCondition
/////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CFindingRuleCondition :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFindingRuleCondition, &CLSID_FindingRuleCondition>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IFindingRuleCondition, &IID_IFindingRuleCondition, &LIBID_UCLID_AFCONDITIONSLib>,
	public IDispatchImpl<IAFCondition, &IID_IAFCondition, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CFindingRuleCondition>
{
public:
	CFindingRuleCondition();
	~CFindingRuleCondition();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

DECLARE_REGISTRY_RESOURCEID(IDR_FINDINGRULECONDITION)

BEGIN_COM_MAP(CFindingRuleCondition)
	COM_INTERFACE_ENTRY(IFindingRuleCondition)
	COM_INTERFACE_ENTRY2(IDispatch, IFindingRuleCondition)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IAFCondition)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CFindingRuleCondition)
	PROP_PAGE(CLSID_FindingRuleConditionPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CFindingRuleCondition)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_CONDITIONS)
END_CATEGORY_MAP()

// IFindingRuleCondition
	STDMETHOD(get_AFRule)(IAttributeFindingRule **ppRetVal);
	STDMETHOD(put_AFRule)(IAttributeFindingRule *pNewVal);

// IAFCondition
	STDMETHOD(raw_ProcessCondition)(IAFDocument *pAFDoc, VARIANT_BOOL *pbRetVal);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR *pbstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown **pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL *pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStream);
	STDMETHOD(Save)(IStream *pStream, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

private:
	///////////////
	// Variables
	///////////////

	bool m_bDirty;

	// The currently configured attribute finding rule to use in evaluating the condition
	IAttributeFindingRulePtr m_ipAFRule;

	///////////////
	// Methods
	///////////////

	// Check licensing
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(FindingRuleCondition), CFindingRuleCondition)
