
#pragma once

#include <vector>

//-------------------------------------------------------------------------------------------------
// struct holding all data associated with a single test case
struct TestCaseData
{
	// ctor
	TestCaseData();

	// copy ctor and assignment operator so that this object
	// can be used in vector
	TestCaseData(const TestCaseData& objToCopy);
	TestCaseData& operator=(const TestCaseData& objToAssign);

	// public member variables associated with each case
	// with these two ints:
	//   -1 means "not specified - so use default value"
	//    0 means "false"
	//    1 means "true"
	int m_iCaseSensitive;
	int m_iTreatMultipleWSAsOne;
	
	bool m_bGreedy;

	_bstr_t m_bstrXML;
	_bstr_t m_bstrInput;
	_bstr_t m_bstrPattern;
	IStrToStrMapPtr m_ipExprMap;
	IStrToStrMapPtr m_ipExpectedMatches;
};
//-------------------------------------------------------------------------------------------------
// a class that encapsulates all the test data (read from TestData.xml)
class TestData
{
public:
	TestData(const std::string &strTestFilesFolder);
	unsigned long getNumTestCases() const;
	TestCaseData& operator[](unsigned long iIndex);

private:
	void loadTestData();
	void loadTestCaseData(MSXML::IXMLDOMNodePtr ipTestCaseNode);
	void loadExpressionMap(MSXML::IXMLDOMNodePtr ipExpressionsNode, TestCaseData& rTC);
	void loadMatches(MSXML::IXMLDOMNodePtr ipMatchesNode, TestCaseData& rTC);

	std::vector<TestCaseData> m_vecTestCaseData;
	std::string m_strTestFilesFolder;
};
//-------------------------------------------------------------------------------------------------
