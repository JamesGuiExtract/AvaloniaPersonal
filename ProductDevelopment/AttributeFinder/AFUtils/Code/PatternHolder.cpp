#include "stdafx.h"
#include "PatternHolder.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// PatternHolder
//-------------------------------------------------------------------------------------------------
PatternHolder::PatternHolder(IRegularExprParserPtr ipRegExpr)
: m_eConfidenceLevel(kZero),
  m_bIsAndRelationship(false),
  m_dStartingRange(0.0),
  m_dEndingRange(1.0),
  m_bCaseSensitive(false),
  m_nStartPage(0),
  m_nEndPage(0),
  m_ipRegExpr(ipRegExpr)
{
	m_vecPatterns.clear();
}
//-------------------------------------------------------------------------------------------------
PatternHolder::PatternHolder(const PatternHolder& objToCopy)
{
	m_eConfidenceLevel = objToCopy.m_eConfidenceLevel;
	m_bIsAndRelationship = objToCopy.m_bIsAndRelationship;
	m_vecPatterns = objToCopy.m_vecPatterns;
	m_dStartingRange = objToCopy.m_dStartingRange;
	m_dEndingRange = objToCopy.m_dEndingRange;
	m_bCaseSensitive = objToCopy.m_bCaseSensitive;
	m_nStartPage = objToCopy.m_nStartPage;
	m_nEndPage = objToCopy.m_nEndPage;
	m_ipRegExpr = objToCopy.m_ipRegExpr;

	// NEW FORMAT items
	m_strBlockID = objToCopy.m_strBlockID;
	m_strSubType = objToCopy.m_strSubType;
}
//-------------------------------------------------------------------------------------------------
PatternHolder& PatternHolder::operator=(const PatternHolder& objToAssign)
{
	m_eConfidenceLevel = objToAssign.m_eConfidenceLevel;
	m_bIsAndRelationship = objToAssign.m_bIsAndRelationship;
	m_vecPatterns = objToAssign.m_vecPatterns;
	m_dStartingRange = objToAssign.m_dStartingRange;
	m_dEndingRange = objToAssign.m_dEndingRange;
	m_bCaseSensitive = objToAssign.m_bCaseSensitive;
	m_nStartPage = objToAssign.m_nStartPage;
	m_nEndPage = objToAssign.m_nEndPage;
	m_ipRegExpr = objToAssign.m_ipRegExpr;

	// NEW FORMAT items
	m_strBlockID = objToAssign.m_strBlockID;
	m_strSubType = objToAssign.m_strSubType;

	return *this;
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
bool PatternHolder::foundPatternsInText(ISpatialStringPtr ipInputText, DocPageCache& cache)
{
	ASSERT_RESOURCE_ALLOCATION("ELI07425", m_ipRegExpr != NULL);
	ASSERT_ARGUMENT("ELI07423", ipInputText != NULL);

	// if there's no pattern defined, return false
	if (m_vecPatterns.empty())
	{
		m_strRuleID = "";
		return false;
	}

	// get input text
	string strInputText = getInputText(ipInputText, cache);

	if (strInputText.empty())
	{
		m_strRuleID = "";
		return false;
	}

	m_ipRegExpr->IgnoreCase = m_bCaseSensitive ? VARIANT_FALSE : VARIANT_TRUE;
	int nInputSize = strInputText.size();
	// start from where in the input text
	int nStartPos = (int)(m_dStartingRange * nInputSize);
	// end at where
	int nEndPos = (int)(m_dEndingRange * nInputSize);
	// make sure end pos greater than start pos, and all within range
	if (nStartPos > nEndPos || nStartPos > nInputSize || nEndPos > nInputSize)
	{
		UCLIDException ue("ELI07083", "Invalid starting/ending range defined in the file.");
		ue.addDebugInfo("Starting Range", m_dStartingRange);
		ue.addDebugInfo("Ending Range", m_dEndingRange);
		throw ue;
	}

	// truncate the input string according to the range
	string strInputWithinRange = strInputText.substr(nStartPos, nEndPos-nStartPos + 1);
	for (unsigned int ui = 0; ui < m_vecPatterns.size(); ui++)
	{
		// Retrieve pattern plus optional Rule ID
		string strIDPlusPattern = m_vecPatterns[ui];

		// Separate pattern and rule ID
		string strPattern;
		string strRuleID;
		unsigned long ulLength = strIDPlusPattern.length();
		unsigned long ulPos = strIDPlusPattern.find( '=', 0 );
		if ((ulPos == string::npos) || (ulPos == ulLength - 1))
		{
			// No defined Rule ID
			strPattern = strIDPlusPattern;
		}
		else
		{
			// Rule ID is before EQUAL sign
			strRuleID = strIDPlusPattern.substr( 0, ulPos );

			// Pattern starts after EQUAL sign
			strPattern = strIDPlusPattern.substr( ulPos + 1, ulLength - ulPos - 1 );
		}

		// Provide pattern to parser
		m_ipRegExpr->Pattern = _bstr_t(strPattern.c_str());
		
		// whether or not this pattern is found in the input text
		//bool bFound = false;
		bool bFound = m_ipRegExpr->StringContainsPattern( 
			_bstr_t(strInputWithinRange.c_str())) == VARIANT_TRUE;
		
		if (bFound && !m_bIsAndRelationship)
		{
			// if it's OR relationship, once a pattern is found
			// save rule ID and return true immediately
			m_strRuleID = strRuleID;
			return true;
		}
		else if (!bFound && m_bIsAndRelationship)
		{
			// if it's AND relationship, once a pattern can't be found
			// save rule ID and return false immediately
			m_strRuleID = strRuleID;
			return false;
		}
	}

	// once this point is reached, 
	// if m_bIsAndRelationship == true, return true
	// if m_bIsAndRelationship == false, return false
	return m_bIsAndRelationship;
}
//-------------------------------------------------------------------------------------------------
bool PatternHolder::isUniqueRuleID(std::string strIDPlusPattern)
{
	// Extract Rule ID from input text
	string strRuleID;
	long lLength = strIDPlusPattern.length();
	long lPos = strIDPlusPattern.find( '=', 0 );
	if ((lPos == string::npos) || (lPos == lLength - 1))
	{
		// No defined Rule ID, return false
		return true;
	}
	else
	{
		// Rule ID is before EQUAL sign
		strRuleID = strIDPlusPattern.substr( 0, lPos );

		// Examine previous Rule ID strings
		long lCount = m_vecRuleIDs.size();
		for (int i = 0; i < lCount; i++)
		{
			// Retrieve this ID
			string strID = m_vecRuleIDs[i];

			// Compare
			if (strID.compare( strRuleID ) == 0)
			{
				// Match found, return false
				return false;
			}
		}
	}

	// No match found, return true
	m_vecRuleIDs.push_back( strRuleID );
	return true;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
std::string PatternHolder::getInputText(ISpatialStringPtr ipInputText, DocPageCache& cache)
{
	// if the range is thge full document
	string strInputText;
	if (m_nStartPage == 1 && m_nEndPage == -1)
	{
		strInputText = asString(ipInputText->String);
		return strInputText;
	}

	// if the page range is cached
	ISpatialStringPtr ipInputOnPages = cache.find(m_nStartPage, m_nEndPage);
	if(ipInputOnPages != NULL)
	{
		strInputText = asString(ipInputOnPages->String);
		return strInputText;
	}
	
	// The page range is not cached
	// get input text from the specified range of pages
	ipInputOnPages = ipInputText->GetRelativePages(m_nStartPage, m_nEndPage);
	ASSERT_RESOURCE_ALLOCATION("ELI07424", ipInputOnPages != NULL);
	strInputText = asString(ipInputOnPages->String);

	// add the page range to the cache
	cache.add(m_nStartPage, m_nEndPage, ipInputOnPages);
	return strInputText;
}
//-------------------------------------------------------------------------------------------------
