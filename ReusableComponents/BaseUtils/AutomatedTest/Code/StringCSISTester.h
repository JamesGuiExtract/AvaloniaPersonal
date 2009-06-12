// StringCSISTester.h : Declaration of the CStringCSISTester

#pragma once
#include "resource.h"       // main symbols
#include "BaseUtilsTest.h"

#include <StringCSIS.h>

#include <string>
#include <vector>
#include <set>

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CStringCSISTester
class ATL_NO_VTABLE CStringCSISTester :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CStringCSISTester, &CLSID_StringCSISTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>
{
public:
	CStringCSISTester();

	DECLARE_REGISTRY_RESOURCEID(IDR_STRINGCSISTESTER)

	BEGIN_COM_MAP(CStringCSISTester)
		COM_INTERFACE_ENTRY(ITestableComponent)
		COM_INTERFACE_ENTRY2(IDispatch, ITestableComponent)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

private:

	////////////
	// Variables
	////////////
	// result logger
	ITestResultLoggerPtr m_ipResultLogger;

	// Method returns the full file name of the Master dat file. The master file name is specified on the 
	// line in the TCL file that runs this test.
	const std::string getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const;

	// Reads and processes the Dat file specified with strFile
	void processFile(const std::string& strFile);

	// Processes a line from the dat file and interprets the <TESTCASE>, <FILE>
	void processLine(std::string strLine, const std::string& strFileWithLine, 
		std::ifstream &ifs);

	// Performs the test
	void doTest(const std::string& strOperation, const stringCSIS& str1, const stringCSIS& str2, 
		const std::string strFindType, const long lCount, const std::vector<std::string>& vecValues );

	// NOTE: for the method comments below, CPC = "char * with count"

	// finds all occurrences of str2 in str1 calling str1.find using the parameter type specified in strFindType
	// if strFindType is CPC, the lCount value is used for the count of chars to use from str2
	// Results are returned in setResults
	void getFindResults( const stringCSIS& str1, const stringCSIS& str2, const std::string strFindType, 
		const long lCount,	std::set<size_t>& setResults );

	// finds all characters not in str2 in str1 calling str1.find_first_not_of using the parameter type specified in strFindType
	// if strFindType is CPC the lCount value is used for the count of chars to use from str2
	// Results are returned in setResults
	void getFindFirstNotOfResults(const stringCSIS& str1, const stringCSIS& str2, const std::string strFindType, 
		const long lCount,	std::set<size_t>& setResults );

	// finds all characters in str2 in str1 calling str1.find_first_of using the parameter type specified in strFindType
	// if strFindType is CPC the lCount value is used for the count of chars to use from str2
	// Results are returned in setResults
	void getFindFirstOfResults(const stringCSIS& str1, const stringCSIS& str2, const std::string strFindType, 
		const long lCount,	std::set<size_t>& setResults );

	// finds all characters not in str2 in str1 calling str1.find_last_not_of using the parameter type specified in strFindType
	// if strFindType is CPC the lCount value is used for the count of chars to use from str2
	// Results are returned in setResults
	void getFindLastNotOfResults(const stringCSIS& str1, const stringCSIS& str2, const std::string strFindType, 
		const long lCount,	std::set<size_t>& setResults );

	// finds all characters in str2 in str1 calling str1.find_last_of using the parameter type specified in strFindType
	// if strFindType is CPC the lCount value is used for the count of chars to use from str2
	// Results are returned in setResults
	void getFindLastOfResults(const stringCSIS& str1, const stringCSIS& str2, const std::string strFindType, 
		const long lCount,	std::set<size_t>& setResults );

	// finds all occurrences of str2 in str1 calling str1.rfind using the parameter type specified in strFindType
	// if strFindType is CPC the lCount value is used for the count of chars to use from str2
	// Results are returned in setResults
	void getRfindResults(const stringCSIS& str1, const stringCSIS& str2, const std::string strFindType, 
		const long lCount,	std::set<size_t>& setResults );

	// Used to return the string represention of the given set
	std::string getSetValueString( const std::string strSetName, std::set<size_t> setValues );

	// process a line with the file keyword in the test input file
	void processFileKeyword(const string& strTCLFile, const string& strLine, vector<string>& rvecTokens);
	
	// process a line with the the testcase keyword in the test input file
	void processTestCase(const string& strLine, vector<string>& rvecTokens);

	// Performs operator test 
	//		if strOperation is ==,!=, <, <=, >, or >= will perform str1 <strOperation> str2 using the 
	//			and compares the result to strExpectedResult(converted to boolean)
	//			result and adds Detail note with the results to the test result logger
	//			return value will be true
	//			rbSuccess will be true if the found results == expected results
	//
	//		if strOperation is not an operator returns false
	//		
	bool performOperatorTest(const string& strOperation, 
							const stringCSIS& str1, const stringCSIS& str2,
							const string& strExpectedResult,
							bool& rbSuccess );

	// Performs the find operation test for operation in strOperation 
	//	`	if strOperaion is a find operation execute the operation, analyze the results and return true
	//			rbSuccess will be true if the found results == expected results false otherwise
	//
	//		if strOperation is not a find operation return false
	bool performFindOperation (const string& strOperation, const stringCSIS& str1, const stringCSIS& str2,
		const std::string strFindType, const long lCount, const std::vector<std::string>& vecValues,
		bool& bSuccess  );

	// Analyzes the results of the Find by comparing with the expected values in vecValues 
	// the results are added to the test case and true is returned if the expected matched the 
	// found otherwise false is returned
	bool analyzeFindResults( const set<size_t>& setResults, const vector<string>& vecValues );

	// Returns true if strOperation is "==", "!=", "<", "<=", ">=", or ">"
	bool isOperator( const std::string strOperation );

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(StringCSISTester), CStringCSISTester)
