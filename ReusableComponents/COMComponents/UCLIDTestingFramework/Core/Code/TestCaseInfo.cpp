
#include "stdafx.h"
#include "TestCaseInfo.h"

#include <UCLIDException.h>
#include <StringTokenizer.h>

using namespace std;

TestCaseInfo::TestCaseInfo(const string& strTestCaseID, const string& strTestCaseDescription, 
						   const string& strTestCaseHtmlDoc)
:m_strTestCaseID(strTestCaseID), m_strTestCaseDescription(strTestCaseDescription),
 m_strTestCaseHtmlDoc(strTestCaseHtmlDoc)
{
}

TestCaseInfo::TestCaseInfo(const TestCaseInfo& objToCopy)
{
	m_strTestCaseID = objToCopy.m_strTestCaseID ;
	m_strTestCaseDescription = objToCopy.m_strTestCaseDescription;
	m_strTestCaseHtmlDoc = objToCopy.m_strTestCaseHtmlDoc;
}

TestCaseInfo& TestCaseInfo::operator = (const TestCaseInfo& objToAssign)
{
	m_strTestCaseID = objToAssign.m_strTestCaseID ;
	m_strTestCaseDescription = objToAssign.m_strTestCaseDescription;
	m_strTestCaseHtmlDoc = objToAssign.m_strTestCaseHtmlDoc;

	return *this;
}

bool operator == (const TestCaseInfo& t1, const TestCaseInfo& t2)
{
	// each test case is expected to have a unique test case Id
	return t1.m_strTestCaseID == t2.m_strTestCaseID;
}

bool operator < (const TestCaseInfo& t1, const TestCaseInfo& t2)
{
	// each test case is expected to have a unique test case Id
	return t1.m_strTestCaseID < t2.m_strTestCaseID;
}

ostream& operator << (ostream& os, const TestCaseInfo& testCaseInfo)
{
	os << testCaseInfo.m_strTestCaseID << ",";
	os << testCaseInfo.m_strTestCaseDescription << ",";
	os << testCaseInfo.m_strTestCaseHtmlDoc << endl;

	return os;
}

istream& operator >> (istream& is, TestCaseInfo& testCaseInfo)
{
	// the same TestCaseInfo object may be used multiple times...so reset its members before
	// reading the data
	testCaseInfo.m_strTestCaseID = "";
	testCaseInfo.m_strTestCaseDescription = "";
	testCaseInfo.m_strTestCaseHtmlDoc = "";

	// get a line from the input stream
	string strTemp;
	getline(is, strTemp);

	if (is)
	{
		if (strTemp != "")
		{
			// ensure that the file contains exactly three fields separated by commas
			vector<string> vecTokens;
			StringTokenizer st;
			st.parse(strTemp, vecTokens);
			if (vecTokens.size() == 3)
			{
				// the file format is ok...
				testCaseInfo.m_strTestCaseID = vecTokens[0];
				testCaseInfo.m_strTestCaseDescription = vecTokens[1];
				testCaseInfo.m_strTestCaseHtmlDoc = vecTokens[2];
			}
			else
			{
				UCLIDException ue("ELI02239", "ITC file not in expected format!");
				ue.addDebugInfo("Line text", strTemp);
				ue.addDebugInfo("Actual # tokens", vecTokens.size());
				ue.addDebugInfo("Expected # tokens", 3);
				throw ue;
			}
		}
	}

	return is;
}
