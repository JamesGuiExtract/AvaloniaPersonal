// RegExprFinder.h : Declaration of the CRegExprFinder

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CRegExprFinder
class ATL_NO_VTABLE CRegExprFinder : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRegExprFinder, &CLSID_RegExprFinder>,
	public ISupportErrorInfo,
	public IDispatchImpl<IRegExprFinder, &IID_IRegExprFinder, &LIBID_UCLID_INPUTFINDERSLib>,
	public IDispatchImpl<IInputFinder, &IID_IInputFinder, &LIBID_UCLID_INPUTFINDERSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CRegExprFinder();

DECLARE_REGISTRY_RESOURCEID(IDR_REGEXPRFINDER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRegExprFinder)
	COM_INTERFACE_ENTRY(IRegExprFinder)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IRegExprFinder)
	COM_INTERFACE_ENTRY(IInputFinder)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IInputFinder
	STDMETHOD(ParseString)(BSTR strInput, IIUnknownVector * * ippTokenPositions);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IRegExprFinder
public:
	STDMETHOD(get_IgnoreCase)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IgnoreCase)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_Pattern)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Pattern)(/*[in]*/ BSTR newVal);

private:
	// VBScript Regular Expression data member 
	IRegularExprParserPtr	m_ipRegExpParser;

	// Checks state of license
	void validateLicense();
};
