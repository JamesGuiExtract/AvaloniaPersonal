// IRUIDisabler.h : Declaration of the CIRUIDisabler

#pragma once

#include "resource.h"       // main symbols

#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CIRUIDisabler
class ATL_NO_VTABLE CIRUIDisabler : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CIRUIDisabler, &CLSID_IRUIDisabler>,
	public ISupportErrorInfo,
	public IDispatchImpl<IIRUIDisabler, &IID_IIRUIDisabler, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CIRUIDisabler();
	~CIRUIDisabler();

DECLARE_REGISTRY_RESOURCEID(IDR_IRUIDISABLER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CIRUIDisabler)
	COM_INTERFACE_ENTRY(IIRUIDisabler)
	COM_INTERFACE_ENTRY2(IDispatch, IIRUIDisabler)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IIRUIDisabler
	STDMETHOD(SetInputManager)(IInputManager* pInputManager);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	//////////
	// Methods
	//////////
	void validateLicense();

	////////////
	// Variables
	////////////
	std::vector<HWND> m_vecWndHandles;
};
