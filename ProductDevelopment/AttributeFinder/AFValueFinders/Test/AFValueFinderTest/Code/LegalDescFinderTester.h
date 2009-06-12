// LegalDescFinderTester.h : Declaration of the CLegalDescFinderTester

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CLegalDescFinderTester
class ATL_NO_VTABLE CLegalDescFinderTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLegalDescFinderTester, &CLSID_LegalDescFinderTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CLegalDescFinderTester();
	~CLegalDescFinderTester();

DECLARE_REGISTRY_RESOURCEID(IDR_LEGALDESCFINDERTESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLegalDescFinderTester)
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
	// Methods
	////////////
	const std::string getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const;

	//------------------------------------------------------------------------------------------------
	// PURPOSE: returns a pointer to a valid SSOCR engine.
	// PROMISE: creates and licenses a new SSOCR engine if m_ipOCREngine is NULL.
	//          returns m_ipOCREngine.
	IOCREnginePtr getOCREngine();

	////////////
	// Variables
	////////////
	// result logger
	ITestResultLoggerPtr m_ipResultLogger;

	IAttributeFindingRulePtr m_ipLegalDescFinder;
};

