// SPMAutomatedTester.h : Declaration of the CSPMAutomatedTester

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CSPMAutomatedTester
class ATL_NO_VTABLE CSPMAutomatedTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSPMAutomatedTester, &CLSID_SPMAutomatedTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<ISPMAutomatedTester, &IID_ISPMAutomatedTester, &LIBID_STRINGPATTERNMATCHERAUTOTESTLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CSPMAutomatedTester();

DECLARE_REGISTRY_RESOURCEID(IDR_SPMAUTOMATEDTESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSPMAutomatedTester)
	COM_INTERFACE_ENTRY(ISPMAutomatedTester)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ISPMAutomatedTester

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:
	/////////////
	// Methods //
	/////////////
	void setTestFileFolder(IVariantVectorPtr ipParams, const std::string &strTCLFile);

	///////////////
	// Variables //
	///////////////
	ITestResultLoggerPtr		m_ipResultLogger;
	std::string					m_strTestFilesFolder;
};
