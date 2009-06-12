// AFUtilityTester.h : Declaration of the CAFUtilityTester

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CAFUtilityTester
class ATL_NO_VTABLE CAFUtilityTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAFUtilityTester, &CLSID_AFUtilityTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CAFUtilityTester();
	~CAFUtilityTester();

DECLARE_REGISTRY_RESOURCEID(IDR_AFUTILITYTESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAFUtilityTester)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:
	////////////
	// Variables
	////////////
	// For automated testing
	ITestResultLoggerPtr m_ipResultLogger;

	IAFUtilityPtr m_ipAFUtility;

	//////////
	// Methods
	//////////
	void test1();
};
