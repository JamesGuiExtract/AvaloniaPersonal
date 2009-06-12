// DocumentClassifierTester.h : Declaration of the CDocumentClassifierTester

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CDocumentClassifierTester
class ATL_NO_VTABLE CDocumentClassifierTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDocumentClassifierTester, &CLSID_DocumentClassifierTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CDocumentClassifierTester();
	~CDocumentClassifierTester();

DECLARE_REGISTRY_RESOURCEID(IDR_DOCUMENTCLASSIFIERTESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDocumentClassifierTester)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY2(IDispatch, ITestableComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
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
	ITestResultLoggerPtr	m_ipResultLogger;
	IDocumentClassifierPtr	m_ipDocClassifier;

	bool		m_bAllPages;
	std::string	m_strPagesToProcess;

	// Counters for overall number of test cases and successes
	long		m_lTestCaseCounter;
	long		m_lSuccessfulTestCounter;

	////////////
	// Methods
	////////////
	bool compareTags(const std::string& strDocName,
		IAFDocumentPtr ipAFDoc, 
		const std::string& strExpectedDocTypes);

	// interprets 0-3 to Zero-Sure level
	std::string getDocumentProbabilityString(const std::string& strProbability);

	//---------------------------------------------------------------------------------------------
	// process the line that has another .dat file that requires further parsing
	void processDatFile(const std::string& strDatFileName);

	// process each test case
	// Return true if the test case succeeded, false otherwise
	bool processTestCase(const std::string& strInputFileName, 
						 const std::string& strIndustryCategoryName,
						 const std::string& strExpectedDocType);

	//---------------------------------------------------------------------------------------------
	// Get the rule or block id for the ones that actually extracts the attributes
	std::string getObjectID(IAFDocumentPtr ipAFDoc, const std::string& strTagName);

	//---------------------------------------------------------------------------------------------
	// PROMISE:	To throw an exception if this object is not licensed to run
	void validateLicense();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return the full path to test file.
	//			The returned value is different in debug and release modes.
	const std::string getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const;
	//---------------------------------------------------------------------------------------------
};

