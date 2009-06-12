// HTIRUtils.h : Declaration of the CHTIRUtils

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CHTIRUtils
class ATL_NO_VTABLE CHTIRUtils : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CHTIRUtils, &CLSID_HTIRUtils>,
	public ISupportErrorInfo,
	public IDispatchImpl<IHTIRUtils, &IID_IHTIRUtils, &LIBID_UCLID_HIGHLIGHTEDTEXTIRLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CHTIRUtils();
	~CHTIRUtils();

DECLARE_REGISTRY_RESOURCEID(IDR_HTIRUTILS)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CHTIRUtils)
	COM_INTERFACE_ENTRY(IHTIRUtils)
	COM_INTERFACE_ENTRY2(IDispatch,IHTIRUtils)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IHTIRUtils
	STDMETHOD(IsExactlyOneTextFileOpen)(
					/*[in]*/ IInputManager* pInputMgr, 
					/*[in, out]*/ VARIANT_BOOL* pbExactOneFileOpen,
					/*[in, out]*/ BSTR* pstrCurrentOpenFileName,
					/*[in, out]*/ IHighlightedTextWindow **pHTIR);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	///////////
	// Methods
	///////////
	void validateLicense();
};
