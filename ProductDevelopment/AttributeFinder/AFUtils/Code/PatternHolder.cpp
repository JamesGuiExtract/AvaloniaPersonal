#include "stdafx.h"
#include "PatternHolder.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// PatternHolder
//-------------------------------------------------------------------------------------------------
PatternHolder::PatternHolder()
: m_eConfidenceLevel(kZero),
  m_bIsAndRelationship(false),
  m_dStartingRange(0.0),
  m_dEndingRange(1.0),
  m_bCaseSensitive(false),
  m_nStartPage(0),
  m_nEndPage(0),
  m_ipMisc(NULL)
{
	try
	{
		m_vecPatterns.clear();

		// Create the misc utils pointer
		m_ipMisc.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI29444", m_ipMisc != NULL);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29445");
}
//-------------------------------------------------------------------------------------------------
PatternHolder::PatternHolder(const PatternHolder& objToCopy)
: m_ipMisc(NULL)
{
	try
	{
		m_eConfidenceLevel = objToCopy.m_eConfidenceLevel;
		m_bIsAndRelationship = objToCopy.m_bIsAndRelationship;
		m_vecPatterns = objToCopy.m_vecPatterns;
		m_dStartingRange = objToCopy.m_dStartingRange;
		m_dEndingRange = objToCopy.m_dEndingRange;
		m_bCaseSensitive = objToCopy.m_bCaseSensitive;
		m_nStartPage = objToCopy.m_nStartPage;
		m_nEndPage = objToCopy.m_nEndPage;

		// NEW FORMAT items
		m_strBlockID = objToCopy.m_strBlockID;
		m_strSubType = objToCopy.m_strSubType;

		// Create the misc utils pointer
		m_ipMisc.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI29446", m_ipMisc != NULL);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29447");
}
//-------------------------------------------------------------------------------------------------
PatternHolder& PatternHolder::operator=(const PatternHolder& objToAssign)
{
	try
	{
		m_eConfidenceLevel = objToAssign.m_eConfidenceLevel;
		m_bIsAndRelationship = objToAssign.m_bIsAndRelationship;
		m_vecPatterns = objToAssign.m_vecPatterns;
		m_dStartingRange = objToAssign.m_dStartingRange;
		m_dEndingRange = objToAssign.m_dEndingRange;
		m_bCaseSensitive = objToAssign.m_bCaseSensitive;
		m_nStartPage = objToAssign.m_nStartPage;
		m_nEndPage = objToAssign.m_nEndPage;

		// NEW FORMAT items
		m_strBlockID = objToAssign.m_strBlockID;
		m_strSubType = objToAssign.m_strSubType;

		// Clear the misc utils pointer and create a new one
		m_ipMisc = NULL;
		m_ipMisc.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI29448", m_ipMisc != NULL);

		return *this;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29449");
}
//-------------------------------------------------------------------------------------------------
PatternHolder::~PatternHolder()
{
	try
	{
		// Release the COM pointers
		m_ipMisc = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28218");
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
bool PatternHolder::foundPatternsInText(const ISpatialStringPtr& ipInputText, DocPageCache& cache)
{
	try
	{
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

		// Get the parser
		IRegularExprParserPtr ipParser = getParser();

		// truncate the input string according to the range
		string strInputWithinRange = strInputText.substr(nStartPos, nEndPos-nStartPos + 1);
		for (unsigned int ui = 0; ui < m_vecPatterns.size(); ui++)
		{
			// Retrieve pattern plus optional Rule ID
			string& strIDPlusPattern = m_vecPatterns[ui];

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

			ipParser->Pattern = strPattern.c_str();

			// whether or not this pattern is found in the input text
			bool bFound = asCppBool(ipParser->StringContainsPattern(strInputWithinRange.c_str()));
			if (bFound != m_bIsAndRelationship)
			{
				// If found and OR relationship ||
				// !found and AND relationship
				// save the rule ID and return bFound immediately
				m_strRuleID = strRuleID;
				return bFound;
			}
		}

		// once this point is reached, 
		// if m_bIsAndRelationship == true, return true
		// if m_bIsAndRelationship == false, return false
		return m_bIsAndRelationship;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28698");
}
//-------------------------------------------------------------------------------------------------
bool PatternHolder::isUniqueRuleID(const string& strIDPlusPattern)
{
	// Extract Rule ID from input text
	long lLength = strIDPlusPattern.length();
	long lPos = strIDPlusPattern.find( '=', 0 );
	if ((lPos == string::npos) || (lPos == lLength - 1))
	{
		// No defined Rule ID, return true
		return true;
	}
	else
	{
		// Rule ID is before EQUAL sign
		string strRuleID = strIDPlusPattern.substr( 0, lPos );

		if (m_setRuleIDs.find(strRuleID) != m_setRuleIDs.end())
		{
				// Match found, return false
				return false;
		}

		// No match found, return true
		m_setRuleIDs.insert(strRuleID);
		return true;
	}
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
std::string PatternHolder::getInputText(const ISpatialStringPtr& ipInputText, DocPageCache& cache)
{
	// If the range is the full document or the string is non-spatial, just return the
	// entire document text [FlexIDSCore #3758]
	if ((m_nStartPage == 1 && m_nEndPage == -1) || ipInputText->GetMode() == kNonSpatialMode)
	{
		return asString(ipInputText->String);
	}

	// Check if the page range is cached
	string strInputText;
	if (cache.find(m_nStartPage, m_nEndPage, strInputText))
	{
		return strInputText;
	}
	
	// The page range is not cached
	// get input text from the specified range of pages
	ISpatialStringPtr ipInputOnPages = ipInputText->GetRelativePages(m_nStartPage, m_nEndPage);
	ASSERT_RESOURCE_ALLOCATION("ELI07424", ipInputOnPages != NULL);
	strInputText = asString(ipInputOnPages->String);

	// add the page range to the cache
	cache.add(m_nStartPage, m_nEndPage, strInputText);
	return strInputText;
}
//-------------------------------------------------------------------------------------------------
IRegularExprParserPtr PatternHolder::getParser()
{
	try
	{
		// Create a new parser (this object is held inside the document classifier so get
		// the parser for the classifier)
		IRegularExprParserPtr ipParser = m_ipMisc->GetNewRegExpParserInstance("DocumentClassifier");
		ASSERT_RESOURCE_ALLOCATION("ELI29452", ipParser != NULL);

		// Set the case sensitivity flag
		ipParser->IgnoreCase = asVariantBool(!m_bCaseSensitive);

		// Return the new parser
		return ipParser;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29450");
}
//-------------------------------------------------------------------------------------------------
