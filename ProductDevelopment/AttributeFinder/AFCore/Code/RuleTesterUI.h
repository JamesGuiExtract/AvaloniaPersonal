// RuleTesterUI.h : Declaration of the CRuleTesterUI

#pragma once

#include "resource.h"       // main symbols
#include "RuleTesterDlg.h"

#include <vector>
#include <string>
#include <memory>

/////////////////////////////////////////////////////////////////////////////
// CRuleTesterUI
class ATL_NO_VTABLE CRuleTesterUI : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRuleTesterUI, &CLSID_RuleTesterUI>,
	public ISupportErrorInfo,
	public IDispatchImpl<IRuleTesterUI, &IID_IRuleTesterUI, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CRuleTesterUI();
	~CRuleTesterUI();
DECLARE_REGISTRY_RESOURCEID(IDR_RULETESTERUI)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRuleTesterUI)
	COM_INTERFACE_ENTRY(IRuleTesterUI)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRuleTesterUI
	STDMETHOD(ShowUI)(BSTR strFileName);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);
public:

private:
	std::unique_ptr<RuleTesterDlg> m_apDlg;

	void validateLicense();

};
