// TestSpatialString.h : Declaration of the CTestSpatialString

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CTestSpatialString
class ATL_NO_VTABLE CTestSpatialString : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTestSpatialString, &CLSID_TestSpatialString>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CTestSpatialString();
	~CTestSpatialString();

DECLARE_REGISTRY_RESOURCEID(IDR_TESTSPATIALSTRING)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CTestSpatialString)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY2(IDispatch, ITestableComponent)
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
	std::string m_strTestFilesFolder;

	ITestResultLoggerPtr m_ipResultLogger;

	ISpatialStringPtr m_ipSpatialString;

	IRegularExprParserPtr m_ipRegExParser;

	//////////
	// Methods
	//////////
	// Almost each of the methods in ISpatialString 
	// will have a test case associated with it
	void runTestCase2();
	void runTestCase4();
	void runTestCase5();
	void runTestCase6();
	void runTestCase7();
	void runTestCase8();
	void runTestCase9();
	void runTestCase10();
	void runTestCase11();
	void runTestCase12();
	void runTestCase13();
	void runTestCase14();
	void runTestCase15();
	void runTestCase16();
	void runTestCase17();
	void runTestCase18();
	void runTestCase19();
	void runTestCase20();
	void runTestCase21();
	void runTestCase22();
	void runTestCase23(); // added as per [p13 #4942] - 03/28/2008 - JDS
	void runTestCase24(); // added as per [LegacyRCAndUtils #4976] - 05/14/2008 - JDS

	// Sets the test file folder
	void setTestFileFolder(IVariantVectorPtr ipParams, const std::string &strTCLFile);
};