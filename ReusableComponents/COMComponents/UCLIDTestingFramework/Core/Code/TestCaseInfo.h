
#pragma once

#include <string>
#include <iostream>

class TestCaseInfo
{
public:
	// constructors & assignment operators
	TestCaseInfo(const std::string& strTestCaseID = "", 
		const std::string& strTestCaseDescription = "", const std::string& strTestCaseHtmlDoc = "");
	TestCaseInfo(const TestCaseInfo& objToCopy);
	TestCaseInfo& operator = (const TestCaseInfo& objToAssign);

	// operators to check for equality, and for sorting purposes
	friend bool operator == (const TestCaseInfo& t1, const TestCaseInfo& t2);
	friend bool operator < (const TestCaseInfo& t1, const TestCaseInfo& t2);

	// stream insertion/extration operators
	friend std::ostream& operator << (std::ostream& os, const TestCaseInfo& testCaseInfo);
	friend std::istream& operator >> (std::istream& is, TestCaseInfo& testCaseInfo);

	std::string m_strTestCaseID;
	std::string m_strTestCaseDescription;
	std::string m_strTestCaseHtmlDoc;
};