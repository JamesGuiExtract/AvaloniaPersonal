#include "stdafx.h"
#include "AttributeTester.h"

#include <COMUtils.h>
#include <SpecialStringDefinitions.h>
#include <UCLIDException.h>

#include <vector>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------

const string gstrATTRIBUTE_DOCTYPE	= "DocumentType";

//-------------------------------------------------------------------------------------------------
// Attribute Tester interface
//-------------------------------------------------------------------------------------------------
IAttributeTester::~IAttributeTester()
{
	try
	{

	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25140")
}
//-------------------------------------------------------------------------------------------------
bool IAttributeTester::isMetadataName(const string& strName)
{
	if (strName.length() > 0)
	{
		if (strName[0] == '_')
		{
			// This is metadata if it starts with an underscore
			return true;
		}
		else if (strName == gstrATTRIBUTE_DOCTYPE)
		{
			// The document type attribute is metadata
			return true;
		}
	}

	return false;
}

//-------------------------------------------------------------------------------------------------
// Attribute Tester
//-------------------------------------------------------------------------------------------------
AttributeTester::AttributeTester()
{
}
//-------------------------------------------------------------------------------------------------
AttributeTester::~AttributeTester()
{
	try
	{
		// Delete the testers
		vector<IAttributeTester*>::const_iterator iter = m_vecTesters.begin();
		while (iter != m_vecTesters.end())
		{
			delete (*iter);
			iter++;
		}

		// Delete short circuited testers
		iter = m_vecShortcircuitedTesters.begin();
		while (iter != m_vecShortcircuitedTesters.end())
		{
			delete (*iter);
			iter++;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25141")

}
//-------------------------------------------------------------------------------------------------
void AttributeTester::addTester(IAttributeTester* pTester)
{
	ASSERT_ARGUMENT("ELI25146", pTester != __nullptr);

	m_vecTesters.push_back(pTester);
}
//-------------------------------------------------------------------------------------------------
bool AttributeTester::getResult() const
{
	vector<IAttributeTester*>::const_iterator iter = m_vecTesters.begin();
	while (iter != m_vecTesters.end())
	{
		if ((*iter)->getResult())
		{
			return true;
		}
		
		iter++;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void AttributeTester::reset()
{
	// Move the contents of the short circuited list into the testers list
	m_vecTesters.insert(m_vecTesters.end(), 
		m_vecShortcircuitedTesters.begin(), m_vecShortcircuitedTesters.end());
	m_vecShortcircuitedTesters.clear();

	vector<IAttributeTester*>::iterator iter = m_vecTesters.begin();
	while (iter != m_vecTesters.end())
	{
		(*iter)->reset();

		iter++;
	}
}
//-------------------------------------------------------------------------------------------------
bool AttributeTester::test(const string& strName, const string& strValue)
{
	// Iterate through the testers
	vector<IAttributeTester*>::iterator iter = m_vecTesters.begin();
	while (iter != m_vecTesters.end())
	{
		// Check this tester
		IAttributeTester* pTester = *iter;
		if (pTester->test(strName, strValue))
		{
			// This tester does not need to look at any more attributes. If this is the only 
			// tester, we are done. If this tester's result is true, the result of ORing the 
			// remaining testers will also be true, so we are done.
			if (m_vecTesters.size() == 1 || pTester->getResult())
			{
				return true;
			}
			else
			{	
				// The short circuited tester's result is false. Move this tester into the 
				// vector of short circuited testers and continue to check the other testers.
				m_vecShortcircuitedTesters.push_back(pTester);
				iter = m_vecTesters.erase(iter);
				continue;
			}
		}

		iter++;
	}

	// Keep looking, don't short circuit.
	return false;
}

//-------------------------------------------------------------------------------------------------
// Data Type Attribute Tester
//-------------------------------------------------------------------------------------------------
DataTypeAttributeTester::DataTypeAttributeTester(const set<string>& setDataTypes,
												 bool bInitialResult, bool bCaseSensitive)
	: m_bResult(bInitialResult),
	  m_bCaseSensitive(bCaseSensitive)
{
	if (bCaseSensitive)
	{
		m_setDataTypes.insert(setDataTypes.begin(), setDataTypes.end());
	}
	else
	{
		for (set<string>::const_iterator it = setDataTypes.begin(); it != setDataTypes.end(); it++)
		{
			string strTemp = *it;
			makeLowerCase(strTemp);
			m_setDataTypes.insert(strTemp);
		}
	}
}
//-------------------------------------------------------------------------------------------------
DataTypeAttributeTester::~DataTypeAttributeTester()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25142")
}
//-------------------------------------------------------------------------------------------------
bool DataTypeAttributeTester::getResult() const
{
	return m_bResult;
}
//-------------------------------------------------------------------------------------------------
void DataTypeAttributeTester::reset()
{
	m_bResult = false;
}

//-------------------------------------------------------------------------------------------------
// None Data Type Attribute Tester
//-------------------------------------------------------------------------------------------------
NoneDataTypeAttributeTester::NoneDataTypeAttributeTester(const set<string>& setDataTypes)
	: DataTypeAttributeTester(setDataTypes, true)
{

}
//-------------------------------------------------------------------------------------------------
void NoneDataTypeAttributeTester::reset()
{
	// Default to a successful match
	m_bResult = true;
}
//-------------------------------------------------------------------------------------------------
bool NoneDataTypeAttributeTester::test(const string& strName, const string& strValue)
{
	// Ignore metadata attributes [FIDSC #4233]
	if (!isMetadataName(strName))
	{
		// Copy the name and if not case sensitive make the string lowercase
		string strTest = strName;
		if (!m_bCaseSensitive)
		{
			makeLowerCase(strTest);
		}

		// Check if this is an expected type
		if (m_setDataTypes.find(strTest) != m_setDataTypes.end())
		{
			// We have found a counter example
			m_bResult = false;

			// Short circuit evaluation
			return true;
		}
	}

	// Keep looking
	return false;
}

//-------------------------------------------------------------------------------------------------
// Any Data Type Attribute Tester
//-------------------------------------------------------------------------------------------------
AnyDataTypeAttributeTester::AnyDataTypeAttributeTester(const set<string>& setDataTypes)
	: DataTypeAttributeTester(setDataTypes)
{

}
//-------------------------------------------------------------------------------------------------
bool AnyDataTypeAttributeTester::test(const string& strName, const string& strValue)
{
	// Ignore metadata attributes [FIDSC #4233]
	if (!isMetadataName(strName))
	{
		// Copy the name and if not case sensitive make the string lowercase
		string strTest = strName;
		if (!m_bCaseSensitive)
		{
			makeLowerCase(strTest);
		}

		// Check if this is an expected type (ignore doctype attributes)
		if (m_setDataTypes.find(strTest) != m_setDataTypes.end())
		{
			// We have found a match
			m_bResult = true;

			// Short circuit evaluation
			return true;
		}
	}

	// Keep looking
	return false;
}

//-------------------------------------------------------------------------------------------------
// One of Each Data Type Attribute Tester
//-------------------------------------------------------------------------------------------------
OneOfEachDataTypeAttributeTester::OneOfEachDataTypeAttributeTester(const set<string>& setDataTypes)
	: DataTypeAttributeTester(setDataTypes)
{

}
//-------------------------------------------------------------------------------------------------
void OneOfEachDataTypeAttributeTester::reset()
{
	// Not a match until at least one found
	m_bResult = false;

	// Move the contents of the matched set into the data types set
	set<string>::const_iterator iter = m_setMatchedDataTypes.begin();
	while (iter != m_setMatchedDataTypes.end())
	{
		m_setDataTypes.insert(*iter);

		iter++;
	}
	m_setMatchedDataTypes.clear();
}
//-------------------------------------------------------------------------------------------------
bool OneOfEachDataTypeAttributeTester::test(const string& strName, const string& strValue)
{
	// Ignore metadata attributes [FIDSC #4233]
	if (!isMetadataName(strName))
	{
		// Copy the name and if not case sensitive make the string lowercase
		string strTest = strName;
		if (!m_bCaseSensitive)
		{
			makeLowerCase(strTest);
		}

		// Check if this is an expected type
		set<string>::iterator iter = m_setDataTypes.find(strTest);
		if (iter != m_setDataTypes.end())
		{
			// We have found a match, move this data type into the matched set
			m_setMatchedDataTypes.insert(strTest);
			m_setDataTypes.erase(iter);

			// Check if this was the last match
			if (m_setDataTypes.empty())
			{
				// We have found at least one of each
				m_bResult = true;

				// Short circuit evaluation
				return true;
			}
		}
	}

	// Keep looking
	return false;
}

//-------------------------------------------------------------------------------------------------
// Only Any Data Type Attribute Tester
//-------------------------------------------------------------------------------------------------
OnlyAnyDataTypeAttributeTester::OnlyAnyDataTypeAttributeTester(const set<string>& setDataTypes)
	: DataTypeAttributeTester(setDataTypes)
{

}
//-------------------------------------------------------------------------------------------------
bool OnlyAnyDataTypeAttributeTester::test(const string& strName, const string& strValue)
{
	// Ignore metadata attributes [FIDSC #4233]
	if (!isMetadataName(strName))
	{
		// Copy the name and if not case sensitive make the string lowercase
		string strTest = strName;
		if (!m_bCaseSensitive)
		{
			makeLowerCase(strTest);
		}

		// Check if this is an expected type
		if (m_setDataTypes.find(strTest) == m_setDataTypes.end())
		{
			// We have found a counter example
			m_bResult = false;

			// Short circuit evaluation
			return true;
		}
		else
		{
			// We have found at least one match
			m_bResult = true;
		}
	}

	// Keep looking
	return false;
}

//-------------------------------------------------------------------------------------------------
// Document Type Attribute Tester
//-------------------------------------------------------------------------------------------------
DocTypeAttributeTester::DocTypeAttributeTester(const set<string>& setDocTypes)
: m_bResult(false),
  m_iDocTypeCount(0),
  m_setDocTypes(setDocTypes)
{
}
//-------------------------------------------------------------------------------------------------
DocTypeAttributeTester::~DocTypeAttributeTester()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25143")
}
//-------------------------------------------------------------------------------------------------
bool DocTypeAttributeTester::getResult() const
{
	// If the specified document tag was found, we are done
	if (m_bResult)
	{
		return true;
	}

	// Check if we have fulfilled one of the special document tag types
	if (m_iDocTypeCount > 1)
	{
		// Check for multiple classified document condition
		return m_setDocTypes.find(gstrSPECIAL_MULTIPLE_CLASS) != m_setDocTypes.end();
	}
	else if (m_iDocTypeCount == 1)
	{
		// Check for uniquely classified document condition
		return m_setDocTypes.find(gstrSPECIAL_ANY_UNIQUE) != m_setDocTypes.end();
	}

	// The document count is zero.
	// Check for unknown document type condition
	return m_setDocTypes.find(gstrSPECIAL_UNKNOWN) != m_setDocTypes.end();
}
//-------------------------------------------------------------------------------------------------
void DocTypeAttributeTester::reset()
{
	m_bResult = false;
	m_iDocTypeCount = 0;
}
//-------------------------------------------------------------------------------------------------
bool DocTypeAttributeTester::test(const string& strName, const string& strValue)
{
	// Check if this is a document type attribute
	if (strName == gstrATTRIBUTE_DOCTYPE)
	{
		// Increment the document type count
		m_iDocTypeCount++;

		// Check if this is an expected document type
		if (m_setDocTypes.find(strValue) != m_setDocTypes.end())
		{
			// Set the result to true
			m_bResult = true;

			// Short-circuit evaluation
			return true;
		}
	}

	// Continue searching
	return false;
}
//-------------------------------------------------------------------------------------------------
