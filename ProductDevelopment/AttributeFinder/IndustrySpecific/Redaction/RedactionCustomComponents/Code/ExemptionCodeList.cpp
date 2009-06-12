#include "stdafx.h"
#include "ExemptionCodeList.h"

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// ExemptionCodeList
//-------------------------------------------------------------------------------------------------
ExemptionCodeList::ExemptionCodeList()
 : m_strCategory(""),
   m_strText("")
{
}
//-------------------------------------------------------------------------------------------------
string ExemptionCodeList::getCategory() const
{
	return m_strCategory;
}
//-------------------------------------------------------------------------------------------------
void ExemptionCodeList::setCategory(const string& strCategory)
{
	m_strCategory = strCategory;
}
//-------------------------------------------------------------------------------------------------
void ExemptionCodeList::addCode(const string& strCode)
{
	m_setCodes.insert(strCode);
}
//-------------------------------------------------------------------------------------------------
string ExemptionCodeList::getOtherText() const
{
	return m_strText;
}
//-------------------------------------------------------------------------------------------------
void ExemptionCodeList::setOtherText(const string& strText)
{
	m_strText = strText;
}
//-------------------------------------------------------------------------------------------------
string ExemptionCodeList::getAsString() const
{
	// Append the exemption codes together
	string strResult = "";
	set<string>::const_iterator iter = m_setCodes.begin();
	if (iter != m_setCodes.end())
	{
		// Append the first exemption codes
		strResult = *iter;
		iter++;

		// Add the other codes separated by commas
		while (iter != m_setCodes.end())
		{
			strResult += ", " + *iter;

			iter++;
		}
	}

	// Add the additional text
	if (!m_strText.empty())
	{
		if (strResult.empty())
		{
			strResult = m_strText;
		}
		else
		{
			strResult += ", " + m_strText;
		}
	}

	// Return the result
	return strResult;
}
//-------------------------------------------------------------------------------------------------
bool ExemptionCodeList::isEmpty() const
{
	return m_setCodes.empty() && m_strText.empty();
}
//-------------------------------------------------------------------------------------------------
bool ExemptionCodeList::hasCategory() const
{
	return !m_strCategory.empty();
}
//-------------------------------------------------------------------------------------------------
bool ExemptionCodeList::hasCode(const string& strCode) const
{
	return m_setCodes.find(strCode) != m_setCodes.end();
}
//-------------------------------------------------------------------------------------------------
bool operator==(const ExemptionCodeList &left, const ExemptionCodeList &right)
{
	// Empty exemption lists are considered equal if only their categories differ.
	if (left.m_setCodes.empty() && right.m_setCodes.empty())
	{
		return left.m_strText == right.m_strText;
	}

	// These are equal if and only if:
	// 1) The categories are the same
	// 2) The exemption codes are the same
	// 3) The other text is the same
	return left.m_strCategory == right.m_strCategory && left.m_setCodes == right.m_setCodes && 
		left.m_strText == right.m_strText;
}
//-------------------------------------------------------------------------------------------------
bool operator!=(const ExemptionCodeList &left, const ExemptionCodeList &right)
{
	return !(left == right);
}
//-------------------------------------------------------------------------------------------------
