// TestHarness.h : Declaration of the CTestHarness

#pragma once

#include "stdafx.h"
#include "resource.h"       // main symbols

#include <vector>
#include <string>
#include <list>
#include <VariableRegistry.h>

/////////////////////////////////////////////////////////////////////////////
// CTestHarness
class ATL_NO_VTABLE CTestHarness : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTestHarness, &CLSID_TestHarness>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestHarness, &IID_ITestHarness, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CTestHarness();
	~CTestHarness();

DECLARE_REGISTRY_RESOURCEID(IDR_TESTHARNESS)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CTestHarness)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ITestHarness)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ITestHarness
	STDMETHOD(raw_RunAutomatedTests)();
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_RunAllTests)();
	STDMETHOD(raw_SetTCLFile)(BSTR strTCLFile);
	STDMETHOD(raw_ExecuteITCFile)(BSTR strITCFile);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:

	////////////
	// Variables
	////////////
	struct TestComponent
	{
		TestComponent()	: m_ipParams(CLSID_VariantVector) {}
		// each testable component's prog id
		std::string m_strProgID;
		// any additional parameters for that component
		IVariantVectorPtr m_ipParams;
	};

	ITestResultLoggerPtr m_ipTestResultLogger;
	IInteractiveTestExecuterPtr m_ipInteractiveTestExecuter;

	std::string m_strTCLFile;
	std::string m_strITCFile;
	std::string m_strDefaultOutputFolder;
	std::string m_strTestStartTime;

	std::vector<TestComponent> m_vecTestComponents;

	// used to store ($...) style variables for use in TCL files
	VariableRegistry m_Variables;

	//////////
	// Methods
	//////////

	// this method verifies that a progid is a valid progid
	void verifyValidProgID(const std::string& strProgID);

	// the following methods start/end the harness & component tests by
	// send appropriate messages to the test result logger
	void startTestHarness();
	void endTestHarness();
	void startComponentTest(const std::string& strComponentDescription,
							const std::string& strOutputFileName);
	void endComponentTest();

	// Check license state
	void validateLicense();

	// member function that does the thread work
	void runAutoTests();

	// member function that does the thread work
	void runInteractiveTests();

	// member function that does the thread work
	void runAllTests();

	// make an output file name out from the TestComponent
	std::string getOutputFileName(const TestComponent& component);
};
	
