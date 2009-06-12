// CreateValue.h : Declaration of the CCreateValue

#ifndef __CREATEVALUE_H_
#define __CREATEVALUE_H_

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
#include <map>

/////////////////////////////////////////////////////////////////////////////
// CCreateValue
class ATL_NO_VTABLE CCreateValue : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCreateValue, &CLSID_CreateValue>,
	public IPersistStream,
	public ISupportErrorInfo,
	public ISpecifyPropertyPagesImpl<CCreateValue>,
	public IDispatchImpl<ICreateValue, &IID_ICreateValue, &LIBID_UCLID_AFVALUEFINDERSLib>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CCreateValue();
	~CCreateValue();

DECLARE_REGISTRY_RESOURCEID(IDR_CREATEVALUE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CCreateValue)
	COM_INTERFACE_ENTRY(ICreateValue)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, ICreateValue)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CCreateValue)
	PROP_PAGE(CLSID_CreateValuePP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CCreateValue++)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICreateValue
	STDMETHOD(get_ValueString)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_ValueString)(/*[in]*/ BSTR newVal);

	STDMETHOD(get_TypeString)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_TypeString)(/*[in]*/ BSTR newVal);

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
	std::string m_strValue;
	std::string m_strType;

	IAFUtilityPtr m_ipAFUtility;

	// flag to keep track of whether this object has been modified
	// since the last save-to-stream operation
	bool m_bDirty;

	// throws an exception if this component is not licensed
	void validateLicense();
};

#endif //__CREATEVALUE_H_
