// EntityFinderTester.h : Declaration of the CEntityFinderTester

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CEntityFinderTester
class ATL_NO_VTABLE CEntityFinderTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEntityFinderTester, &CLSID_EntityFinderTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CEntityFinderTester();

DECLARE_REGISTRY_RESOURCEID(IDR_ENTITYFINDERTESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CEntityFinderTester)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY2(IDispatch, ITestableComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	////////////
	// Variables
	////////////
	// Object that does finding of Persons or Companies
	IEntityFinderPtr		m_ipEntityFinder;

	// For automated testing
	ITestResultLoggerPtr	m_ipResultLogger;
	ISpatialStringPtr		m_ipTestString;
	ISpatialStringPtr		m_ipExpectedString;

	//////////
	// Methods
	//////////

	// Compares TestString and ExpectedString and sets test result
	bool	checkTestResults();

	// Creates a test case with specified label and calls Entity Finder with m_ipTestString
	void	doTest(std::string strLabel);

	// Creates TestString and ExpectedString
	void	prepareTests();

	// Processes lines in specified file
	void	processFile(const std::string& strFile);

	// Parses specified line and performs test of Entity Finder or 
	// processes items in the provided file
	void	processLine(std::string strLine, 
		const std::string& strFileWithLine);

	//---------------------------------------------------------------------------------------------
	// PROMISE:	To throw an exception if this object is not licensed to run
	void	validateLicense();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return the full path to the master test file.  The path is 
	//			computed differently in debug and release builds.
	const std::string getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const;
	//---------------------------------------------------------------------------------------------
};
