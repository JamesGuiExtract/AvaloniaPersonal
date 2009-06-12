// RSDFileCondition.h : Declaration of the CRSDFileCondition

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"
#include <CachedObjectFromFile.h>
#include "..\..\AFCore\Code\RuleSetLoader.h"
/////////////////////////////////////////////////////////////////////////////
// RSDFileCondition
class ATL_NO_VTABLE CRSDFileCondition : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRSDFileCondition, &CLSID_RSDFileCondition>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IRSDFileCondition, &IID_IRSDFileCondition, &LIBID_UCLID_AFCONDITIONSLib>,
	public IDispatchImpl<IAFCondition, &IID_IAFCondition, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CRSDFileCondition>
{
public:
	CRSDFileCondition();
	~CRSDFileCondition();

DECLARE_REGISTRY_RESOURCEID(IDR_RSDFILECONDITION)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRSDFileCondition)
	COM_INTERFACE_ENTRY(IRSDFileCondition)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IRSDFileCondition)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IAFCondition)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CRSDFileCondition)
	PROP_PAGE(CLSID_RSDFileConditionPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CRSDFileCondition)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_CONDITIONS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRSDFileCondition
	STDMETHOD(get_RSDFileName)(BSTR *pRetVal);
	STDMETHOD(put_RSDFileName)(BSTR pNewVal);

// IAFCondition
	STDMETHOD(raw_ProcessCondition)(IAFDocument *pAFDoc, VARIANT_BOOL* pbRetVal);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	///////////////
	// Variables
	///////////////
	// flag to keep track of whether object is dirty
	bool m_bDirty;

	// This flag is set to indicate that a new .rsd File has been
	// specified and thus a new RuleSetNeeds to be loaded
	bool m_bNewFileName;

	// This contains the list of DocType strings that this 
	// condition will allow (or not allow)
	std::string m_strRSDFileName;

	// This will hold the rule set defined in strRSDFileName
	CachedObjectFromFile<IRuleSetPtr, RuleSetLoader> m_cachedRuleSet;

	IRuleExecutionEnvPtr m_ipRuleExecutionEnv;

	IAFUtilityPtr m_ipAFUtility;

	// True if RSD files should be cached; false if they shouldn't be cached.
	bool m_bCacheRSD;

	///////////////
	// Methods
	///////////////

	// Check licensing
	void validateLicense();
};