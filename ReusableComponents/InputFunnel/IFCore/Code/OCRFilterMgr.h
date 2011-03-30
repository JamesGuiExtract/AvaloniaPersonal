
#pragma once

#include "resource.h"       // main symbols

#include <OCRFilterSchemesDlg.h>

#include <map>
#include <string>
#include <memory>

/////////////////////////////////////////////////////////////////////////////
// COCRFilterMgr
class ATL_NO_VTABLE COCRFilterMgr : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COCRFilterMgr, &CLSID_OCRFilterMgr>,
	public ISupportErrorInfo,
	public IDispatchImpl<IOCRFilterMgr, &IID_IOCRFilterMgr, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<IOCRFilter, &IID_IOCRFilter, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	COCRFilterMgr();
	~COCRFilterMgr();

DECLARE_REGISTRY_RESOURCEID(IDR_OCRFILTERMGR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(COCRFilterMgr)
	COM_INTERFACE_ENTRY(IOCRFilterMgr)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IOCRFilterMgr)
	COM_INTERFACE_ENTRY(IOCRFilter)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IOCRFilterMgr
	STDMETHOD(ShowFilterSettingsDlg)();
	STDMETHOD(ShowFilterSchemesDlg)();
	STDMETHOD(SetCurrentScheme)(/*[in]*/ BSTR strSchemeName);
	STDMETHOD(GetCurrentScheme)(/*[out, retval]*/ BSTR *pstrSchemeName);
// IOCRFilter
	STDMETHOD(GetValidChars)(BSTR strInputType, BSTR * pstrValidChars);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	std::map<std::string, std::string> m_mapInputTypeToValidChars;

	std::unique_ptr<OCRFilterSchemesDlg> m_apFilterSchemeDlg;

	OCRFilterSchemesDlg* getFilterSchemeDlg();

	void validateLicense();
};