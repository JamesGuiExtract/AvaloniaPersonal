// AFEngineTester.h : Declaration of the CAFEngineTester

#pragma once

#include "resource.h"       // main symbols

#include <string>
#include <map>

/////////////////////////////////////////////////////////////////////////////
// CAFEngineTester
class ATL_NO_VTABLE CAFEngineTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAFEngineTester, &CLSID_AFEngineTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CAFEngineTester();
	
DECLARE_REGISTRY_RESOURCEID(IDR_AFENGINETESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAFEngineTester)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY2(IDispatch, ITestableComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAFEngineTester
public:
// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// enums for different types of tests to execute on the engine
	enum ETestType 
	{
		kAutoFile,
		kImageFile,
		kTextFile,
		kUSSFile,
		kText
	};

	/////////////
	// Variables
	/////////////

	IAttributeFinderEnginePtr m_ipAttrFinderEngine;

	ITestResultLoggerPtr m_ipResultLogger;

	IAFUtilityPtr m_ipAFUtility;

	////////////
	// Methods
	////////////
	
	//---------------------------------------------------------------------------------------------	
	// PROMISE:	To run a subcase for all the attributes for the method specified by eTestType 
	//			and additional subcases for each attribute in the ipExpectedAttr.  For the the image
	//			test additional cases will be tested for each page.
	// PURPOSE: Test each of the methods of the AttributeFinderEngine
	// REQUIRE:	The RSD file must have 3 attributes named page1, page2, page3 and they must only be 
	//			found on the page indicated by there name
	// ARGS:	eTestType			specifies the method to test
	//			strRSDFile			is the name of RSD test file
	//			strInputFileName	is the name of the input file name
	//			strCaseNo			is the case number of the test
	//			ipExpectedAttr		is a vector with the expected attributes
	//			bValidateOutput		if false runs the test without validation, if true will use the 
	//								RemoveInvalidEntries output handler
	//			nSubCaseNumber		is the number of the subcase
	//			ipAttributeNames	contains the attributes to find, if NULL finds all attributes
	//			bRecurse			if true will run subcases if false only runs a single test case
	//			nPagesToRecognize	only used for eTestType = kImageFile to specify the number of 
	//								pages to recognize default is -1 to recognize all pages
	void runTest(ETestType eTestType, const std::string& strRSDFile, 
		const std::string& strInputFileName, const std::string& strCaseNo,
		IIUnknownVectorPtr ipExpectedAttr, const bool bValidateOutput = false,
		int nSubCaseNumber = 1, IVariantVectorPtr ipAttributeNames = __nullptr,
		bool bRecurse = true, int nPagesToRecognize = -1);

	//---------------------------------------------------------------------------------------------
	// PROMISE:	To throw an exception if this object is not licensed to run
	void validateLicense();

	//---------------------------------------------------------------------------------------------
	// PROMISE: To return the full path to the test file strFileName.
	//			The returned full path will be constructed differently in 
	//			debug and release modes with knowledge of the appropriate
	//			test files folder.
	// ARGS:	strFileName is the name of a test file without any path (such
	//			as "test1.tif"
	const std::string getTestFileFullPath(const std::string& strFileName, IVariantVectorPtr ipParams, const std::string &strTCLFile) const;
	//---------------------------------------------------------------------------------------------
	// PROMISE: To start a test case using the test result logger using the information
	//			provided in the arguments
	void recordStartOfTestCase(const std::string& strCaseDescriptionBase,
		const std::string& strCaseNo, const int nSubCaseNumber, 
		const bool bValidateOutput);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To add a test case memo using the test result logger with the given
	//			title and a stringized version of the attributes in ipAttributes
	void addTestCaseMemoWithAttr(const std::string& strTitle, 
		const IIUnknownVectorPtr& ipAttributes);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To add a test case memo with the expected attributes, and to also add a
	//			test case memo with the found attributes if any attributes were found.
	//			If no attributes are found, then an exception will be thrown.  If attributes are
	//			found, and they match the expected attributes rbSuccess will be set to true.
	//			Otherwise, rbSuccess will be set to false.
	void processResults(IIUnknownVectorPtr ipExpectedAttr, 
		IIUnknownVectorPtr ipFoundAttr, bool& rbSuccess);
	//---------------------------------------------------------------------------------------------
};
