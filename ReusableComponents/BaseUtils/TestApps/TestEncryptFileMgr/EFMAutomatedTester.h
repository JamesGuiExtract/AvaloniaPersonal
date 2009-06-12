// EFMAutomatedTester.h : Declaration of the CEFMAutomatedTester

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CEFMAutomatedTester
class ATL_NO_VTABLE CEFMAutomatedTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEFMAutomatedTester, &CLSID_EFMAutomatedTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<IEFMAutomatedTester, &IID_IEFMAutomatedTester, &LIBID_TESTENCRYPTFILEMGRLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CEFMAutomatedTester();

DECLARE_REGISTRY_RESOURCEID(IDR_EFMAUTOMATEDTESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CEFMAutomatedTester)
	COM_INTERFACE_ENTRY(IEFMAutomatedTester)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IEFMAutomatedTester)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:
	ITestResultLoggerPtr		m_ipResultLogger;
};
