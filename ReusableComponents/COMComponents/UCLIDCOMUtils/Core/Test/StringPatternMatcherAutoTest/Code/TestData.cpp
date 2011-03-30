
#include "stdafx.h"
#include "TestData.h"


#include <UCLIDException.h>
#include <cpputil.h>

#include <io.h>
using namespace std;

//-------------------------------------------------------------------------------------------------
// TestCaseData
//-------------------------------------------------------------------------------------------------
TestCaseData::TestCaseData()
:m_ipExprMap(CLSID_StrToStrMap),
 m_ipExpectedMatches(CLSID_StrToStrMap),
 m_iCaseSensitive(-1), m_iTreatMultipleWSAsOne(-1), m_bGreedy(true)
{
	ASSERT_RESOURCE_ALLOCATION("ELI05969", m_ipExprMap != __nullptr);
	ASSERT_RESOURCE_ALLOCATION("ELI05970", m_ipExpectedMatches != __nullptr);
}
//-------------------------------------------------------------------------------------------------
TestCaseData::TestCaseData(const TestCaseData& objToCopy)
{
	m_ipExprMap.CreateInstance(CLSID_StrToStrMap);
	ASSERT_RESOURCE_ALLOCATION("ELI05987", m_ipExprMap != __nullptr);

	m_ipExpectedMatches.CreateInstance(CLSID_StrToStrMap);
	ASSERT_RESOURCE_ALLOCATION("ELI05988", m_ipExpectedMatches != __nullptr);

	*this = objToCopy;
}
//-------------------------------------------------------------------------------------------------
TestCaseData& TestCaseData::operator=(const TestCaseData& objToAssign)
{
	// copy the input and pattern strings
	m_bstrInput = objToAssign.m_bstrInput.copy();
	m_bstrPattern = objToAssign.m_bstrPattern.copy();
	m_bstrXML = objToAssign.m_bstrXML.copy();
	m_iCaseSensitive = objToAssign.m_iCaseSensitive;
	m_iTreatMultipleWSAsOne = objToAssign.m_iTreatMultipleWSAsOne;
	m_bGreedy = objToAssign.m_bGreedy;
	
	// copy the expression map
	ICopyableObjectPtr ipObj = objToAssign.m_ipExprMap;
	ASSERT_RESOURCE_ALLOCATION("ELI05982", ipObj !=NULL);
//	ipObj->CopyTo(m_ipExprMap);
	ICopyableObjectPtr ipCopy = m_ipExprMap;
	ASSERT_RESOURCE_ALLOCATION("ELI08388", ipCopy !=NULL);
	ipCopy->CopyFrom(ipObj);

	// copy the expected matches vector
	ipObj = objToAssign.m_ipExpectedMatches;
	ASSERT_RESOURCE_ALLOCATION("ELI05983", ipObj !=NULL);
//	ipObj->CopyTo(m_ipExpectedMatches);
	ipCopy = m_ipExpectedMatches;
	ASSERT_RESOURCE_ALLOCATION("ELI08389", ipCopy !=NULL);
	ipCopy->CopyFrom(ipObj);	
	return *this;
}
//-------------------------------------------------------------------------------------------------

//-------------------------------------------------------------------------------------------------
// TestData
//-------------------------------------------------------------------------------------------------
TestData::TestData(const std::string &strTestFilesFolder)
: m_strTestFilesFolder(strTestFilesFolder)
{
	// load test data
	loadTestData();
}
//-------------------------------------------------------------------------------------------------
unsigned long TestData::getNumTestCases() const
{
	return m_vecTestCaseData.size();
}
//-------------------------------------------------------------------------------------------------
TestCaseData& TestData::operator[](unsigned long iIndex)
{
	// if the array index is OK, return the corresponding test case
	if (iIndex < m_vecTestCaseData.size())
	{
		return m_vecTestCaseData[iIndex];
	}

	// if we reached here, it's because iIndex is invalid
	UCLIDException ue("ELI05984", "Invalid test case index!");
	ue.addDebugInfo("iIndex", iIndex);
	throw ue;
}
//-------------------------------------------------------------------------------------------------
void TestData::loadTestData()
{
	// get the test data xml file path
	string strTestDataFile = m_strTestFilesFolder + string("\\TestData.xml");

	if (!isValidFile(strTestDataFile))
	{
		// if the test file doesn't exist
		UCLIDException ue("ELI05972", "Test data file can't be found.");
		ue.addDebugInfo("Test data file", strTestDataFile);
		ue.addWin32ErrorInfo();
		throw ue;
	}

	// create an instance of the XML DOM parser
	MSXML::IXMLDOMDocumentPtr ipDOMDocument(MSXML::CLSID_DOMDocument);
	if (ipDOMDocument == __nullptr)
	{
		throw UCLIDException("ELI05973", "Unable to create instance of XML parser engine!.");
	}

	// load the document in the DOM parser
	variant_t vLoadResult = ipDOMDocument->load(strTestDataFile.c_str());
	if (((bool) vLoadResult) != TRUE) // success!
	{
		UCLIDException ue("ELI05974", "Unable to load test data file in XML parser engine!.");
		ue.addDebugInfo("Test data file", strTestDataFile);
		throw ue;
	}
	
	// get the root document element
	MSXML::IXMLDOMElementPtr ipDocRoot = ipDOMDocument->documentElement;

	// get each of the nodes for the test cases and process them
	MSXML::IXMLDOMNodeListPtr ipList = ipDOMDocument->selectNodes("/TestData/TestCase");
	for (int i = 0; i < ipList->length; i++)
	{
		// retrieve test case node
		MSXML::IXMLDOMNodePtr ipTestCaseNode;
		if (FAILED(ipList->get_item(i, &ipTestCaseNode)))
		{
			UCLIDException ue("ELI05975", "Unable to retrieve test case data!");
			ue.addDebugInfo("i", i);
			throw ue;
		}

		// load data from the test case node
		loadTestCaseData(ipTestCaseNode);
	}
}
//-------------------------------------------------------------------------------------------------
void TestData::loadTestCaseData(MSXML::IXMLDOMNodePtr ipTestCaseNode)
{
	// create instance of TestCaseData to later add to the internal vector
	// of TestCaseData structures
	TestCaseData tc;

	// retrieve & store the XML for this test case 
	tc.m_bstrXML = ipTestCaseNode->xml;

	// check if the case-sensitivity (CS) attribute has been specified for
	// the test case
	MSXML::IXMLDOMNodePtr ipCSNode = ipTestCaseNode->selectSingleNode("@CaseSensitive");
	if (ipCSNode != __nullptr)
	{
		string strCSValue = ipCSNode->text;
		if (strCSValue == "0")
		{
			tc.m_iCaseSensitive = false;
		}
		else if (strCSValue == "1")
		{
			tc.m_iCaseSensitive = true;
		}
		else
		{
			UCLIDException ue("ELI05994", "Invalid value for the CaseSensitive attribute!");
			string strXML = ipTestCaseNode->xml;
			ue.addDebugInfo("TestCase", strXML);
			ue.addDebugInfo("CS Attribute Value", strCSValue);
			throw ue;
		}
	}

	// check if the m_iTreatMultipleWSAsOne attribute has been specified for
	// the test case
	MSXML::IXMLDOMNodePtr ipWSNode = ipTestCaseNode->selectSingleNode("@TreatMultipleWSAsOne");

	if (ipWSNode != __nullptr)
	{
		string strValue = ipWSNode->text;
		if (strValue == "0")
		{
			tc.m_iTreatMultipleWSAsOne = false;
		}
		else if (strValue == "1")
		{
			tc.m_iTreatMultipleWSAsOne = true;
		}
		else
		{
			UCLIDException ue("ELI06260", "Invalid value for the TreatMultipleWSAsOne attribute!");
			string strXML = ipTestCaseNode->xml;
			ue.addDebugInfo("TestCase", strXML);
			ue.addDebugInfo("Attribute Value", strValue);
			throw ue;
		}
	}

	// check if the m_bGreedy attribute has been specified for
	// the test case
	MSXML::IXMLDOMNodePtr ipGreedyNode = ipTestCaseNode->selectSingleNode("@Greedy");
	if (ipGreedyNode != __nullptr)
	{
		string strValue = ipGreedyNode->text;
		if (strValue == "0")
		{
			tc.m_bGreedy = false;
		}
		else if (strValue == "1")
		{
			tc.m_bGreedy = true;
		}
		else
		{
			UCLIDException ue("ELI06363", "Invalid value for the Greedy attribute!");
			string strXML = ipTestCaseNode->xml;
			ue.addDebugInfo("TestCase", strXML);
			ue.addDebugInfo("Attribute Value", strValue);
			throw ue;
		}
	}
	else
	{
		// the Greedy attribute must be specified
		UCLIDException ue("ELI06364", "No value specified for the Greedy attribute!");
		string strXML = ipTestCaseNode->xml;
		ue.addDebugInfo("TestCase", strXML);
		throw ue;
	}

	// for this node, get the input
	MSXML::IXMLDOMNodePtr ipInputNode = ipTestCaseNode->selectSingleNode("Input");
	if (ipInputNode == __nullptr)
	{
		UCLIDException ue("ELI05976", "Unable to retrieve input information!");
		string strXML = ipTestCaseNode->xml;
		ue.addDebugInfo("TestCase", strXML);
		throw ue;
	}
	tc.m_bstrInput = ipInputNode->text;

	// for this node, get the pattern
	MSXML::IXMLDOMNodePtr ipPatternNode = ipTestCaseNode->selectSingleNode("Pattern");
	if (ipPatternNode == __nullptr)
	{
		UCLIDException ue("ELI05977", "Unable to retrieve input information!");
		string strXML = ipTestCaseNode->xml;
		ue.addDebugInfo("TestCase", strXML);
		throw ue;
	}
	tc.m_bstrPattern = ipPatternNode->text;

	// for this node, find the expressions encapsulating node
	// NOTE: expression definition is optional, and so, if we can't find
	// this node, it's OK
	MSXML::IXMLDOMNodePtr ipExpressionsNode = ipTestCaseNode->selectSingleNode("Expressions");
	if (ipExpressionsNode != __nullptr)
	{
		loadExpressionMap(ipExpressionsNode, tc);
	}

	// find the expected matches encapsulating node
	MSXML::IXMLDOMNodePtr ipMatchesNode = ipTestCaseNode->selectSingleNode("ExpectedMatches");
	if (ipMatchesNode == __nullptr)
	{
		UCLIDException ue("ELI05980", "Unable to retrieve expected-matches information!");
		string strXML = ipTestCaseNode->xml;
		ue.addDebugInfo("TestCase", strXML);
		throw ue;
	}
	
	// load the matches
	loadMatches(ipMatchesNode, tc);

	// now that all information about the test case has been loaded, 
	// add the test case to the vector of to-be-executed testcases
	m_vecTestCaseData.push_back(tc);
}
//-------------------------------------------------------------------------------------------------
void TestData::loadExpressionMap(MSXML::IXMLDOMNodePtr ipExpressionsNode, 
								 TestCaseData& rTC)
{
	// find all the expression sub nodes
	MSXML::IXMLDOMNodeListPtr ipExpressionList = ipExpressionsNode->selectNodes("Expression");
	if (ipExpressionList != __nullptr)
	{
		for (int j = 0; j < ipExpressionList->length; j++)
		{
			// get the expression node
			MSXML::IXMLDOMNodePtr ipExpressionNode;
			ipExpressionList->get_item(j, &ipExpressionNode);
			if (ipExpressionNode == __nullptr)
			{
				UCLIDException ue("ELI05978", "Unable to retrieve expression!");
				string strXML = ipExpressionsNode->xml;
				ue.addDebugInfo("j", j);
				ue.addDebugInfo("Expressions", strXML);
				throw ue;
			}

			// get the name and value
			MSXML::IXMLDOMNodePtr ipNameNode = ipExpressionNode->selectSingleNode("Name");
			MSXML::IXMLDOMNodePtr ipValueNode = ipExpressionNode->selectSingleNode("Value");
			if (ipNameNode == __nullptr || ipValueNode == __nullptr)
			{
				UCLIDException ue("ELI05979", "Unable to retrieve expression!");
				string strXML = ipExpressionsNode->xml;
				ue.addDebugInfo("j", j);
				ue.addDebugInfo("Expressions", strXML);
				throw ue;
			}

			// the value string may contain special chars (like \n, \t, etc)
			// replace them with the appropriate chars.
			string strValue = ipValueNode->text;
			convertNormalStringToCppString(strValue);

			// store the expression in the map
			rTC.m_ipExprMap->Set(ipNameNode->text, _bstr_t(strValue.c_str()));
		}
	}
}
//-------------------------------------------------------------------------------------------------
void TestData::loadMatches(MSXML::IXMLDOMNodePtr ipMatchesNode, TestCaseData& rTC)
{
	// find all the expected-matches nodes
	MSXML::IXMLDOMNodeListPtr ipMatchesList = ipMatchesNode->selectNodes("Match");
	for (int j = 0; j < ipMatchesList->length; j++)
	{
		// get the match node
		_bstr_t _bstrVariableName, _bstrExpectedValue;
		MSXML::IXMLDOMNodePtr ipMatchNode;
		ipMatchesList->get_item(j, &ipMatchNode);
		
		// get the expected value of the match variable
		if (ipMatchNode == __nullptr)
		{
			UCLIDException ue("ELI05981", "Unable to retrieve expected-matches information!");
			string strXML = ipMatchesNode->xml;
			ue.addDebugInfo("j", j);
			ue.addDebugInfo("Matches", strXML);
			throw ue;
		}
		else
		{
			_bstrExpectedValue = ipMatchNode->text;
		}

		// get the name of the match variable
		MSXML::IXMLDOMNodePtr ipNameNode = ipMatchNode->selectSingleNode("@Name");
		if (ipNameNode != __nullptr)
		{
			_bstrVariableName = ipNameNode->text;
		}
		else
		{
			UCLIDException ue("ELI06272", "Match name node not specified!");
			string strXML = ipMatchesNode->xml;
			ue.addDebugInfo("xml", strXML);
			throw ue;
		}

		// store the match in the expected-matches map
		rTC.m_ipExpectedMatches->Set(_bstrVariableName, _bstrExpectedValue);
	}
}
//-------------------------------------------------------------------------------------------------
