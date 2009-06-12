// RSDSplitter.h : Declaration of the CRSDSplitter

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <CachedObjectFromFile.h>
#include "..\..\AFCore\Code\RuleSetLoader.h"

#include <string>
/////////////////////////////////////////////////////////////////////////////
// CRSDSplitter
class ATL_NO_VTABLE CRSDSplitter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRSDSplitter, &CLSID_RSDSplitter>,
	public ISupportErrorInfo,
	public IDispatchImpl<IRSDSplitter, &IID_IRSDSplitter, &LIBID_UCLID_AFSPLITTERSLib>,
	public IPersistStream,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CRSDSplitter>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IAttributeSplitter, &IID_IAttributeSplitter, &LIBID_UCLID_AFSPLITTERSLib>
{
public:
	CRSDSplitter();
	~CRSDSplitter();

DECLARE_REGISTRY_RESOURCEID(IDR_RSDSPLITTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRSDSplitter)
	COM_INTERFACE_ENTRY(IRSDSplitter)
	COM_INTERFACE_ENTRY2(IDispatch, IRSDSplitter)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	COM_INTERFACE_ENTRY(IAttributeSplitter)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_PROP_MAP(CRSDSplitter)
	PROP_PAGE(CLSID_RSDSplitterPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CRSDSplitter)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_ATTRIBUTE_SPLITTERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRSDSplitter
	STDMETHOD(get_RSDFileName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_RSDFileName)(/*[in]*/ BSTR newVal);

// IAttributeSplitter
	STDMETHOD(raw_SplitAttribute)(IAttribute *pAttribute, IAFDocument *pAFDoc,
		IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

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
	////////////
	// Variables
	////////////
	// flag to keep track of whether object is dirty
	bool m_bDirty;

	std::string m_strRSDFileName;

	// RuleSet object created from RSD file to further parse the attribute
	CachedObjectFromFile<IRuleSetPtr, RuleSetLoader> m_ipRuleSet;

	IRuleExecutionEnvPtr m_ipRuleExecutionEnv;
	
	IAFUtilityPtr m_ipAFUtility;

	IMiscUtilsPtr m_ipMiscUtils;

	// True if RSD files should be cached; false if they shouldn't be cached.
	bool m_bCacheRSD;

	//////////
	// Methods
	//////////

	IRuleSetPtr getRuleSet(string strRSDFile);

	// Returns full RSD file name
	std::string getAbsoluteRSDFile(const std::string& strRSDFileName);
	
	// ensure that this component is licensed
	void validateLicense();
};

