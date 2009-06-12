// FindFromRSD.h : Declaration of the CFindFromRSD

#ifndef __FINDFROMRSD_H_
#define __FINDFROMRSD_H_

#include "resource.h"       // main symbols
#include <CachedObjectFromFile.h>
#include "..\..\AFCore\Code\RuleSetLoader.h"
#include "..\..\AFCore\Code\AFCategories.h"
#include <string>
/////////////////////////////////////////////////////////////////////////////
// CFindFromRSD
class ATL_NO_VTABLE CFindFromRSD : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFindFromRSD, &CLSID_FindFromRSD>,
	public IPersistStream,
	public ISupportErrorInfo,
	public ISpecifyPropertyPagesImpl<CFindFromRSD>,
	public IDispatchImpl<IFindFromRSD, &IID_IFindFromRSD, &LIBID_UCLID_AFVALUEFINDERSLib>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CFindFromRSD();
	~CFindFromRSD();

DECLARE_REGISTRY_RESOURCEID(IDR_FINDFROMRSD)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CFindFromRSD)
	COM_INTERFACE_ENTRY(IFindFromRSD)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IFindFromRSD)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CFindFromRSD)
	PROP_PAGE(CLSID_FindFromRSDPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CFindFromRSD)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IFindFromRSD
	STDMETHOD(get_AttributeName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_AttributeName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_RSDFileName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_RSDFileName)(/*[in]*/ BSTR newVal);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IAttributeFindingRule
	STDMETHOD(raw_ParseText)(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
		IIUnknownVector **pAttributes);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);
public:

	/////////////////
	// Variables
	/////////////////
	std::string m_strAttributeName;
	std::string m_strRSDFileName;

	// RuleSet object created from RSD file to further parse the attribute
	CachedObjectFromFile<IRuleSetPtr, RuleSetLoader> m_cachedRuleSet;
	IRuleExecutionEnvPtr m_ipRuleExecutionEnv;
	IMiscUtilsPtr m_ipMiscUtils;

	// True if RSD files should be cached; false if they shouldn't be cached.
	bool m_bCacheRSD;

	bool m_bDirty;

	/////////////////
	// Methods
	/////////////////

	void validateLicense();
};

#endif //__FINDFROMRSD_H_
