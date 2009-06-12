// InteractiveTestExecuter.h : Declaration of the CInteractiveTestExecuter

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CInteractiveTestExecuter
class ATL_NO_VTABLE CInteractiveTestExecuter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CInteractiveTestExecuter, &CLSID_InteractiveTestExecuter>,
	public ISupportErrorInfo,
	public IDispatchImpl<IInteractiveTestExecuter, &IID_IInteractiveTestExecuter, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CInteractiveTestExecuter();
	~CInteractiveTestExecuter();

DECLARE_REGISTRY_RESOURCEID(IDR_INTERACTIVETESTEXECUTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CInteractiveTestExecuter)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IInteractiveTestExecuter)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

public:
// IInteractiveTestExecuter
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_ExecuteITCFile)(BSTR strITCFile);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	ITestResultLoggerPtr m_ipResultLogger;

	// Check license state
	void	validateLicense();
};

