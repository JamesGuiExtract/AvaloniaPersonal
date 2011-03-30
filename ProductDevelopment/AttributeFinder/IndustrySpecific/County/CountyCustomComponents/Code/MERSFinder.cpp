// MERSFinder.cpp: implementation of the MERSFinder class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "MERSFinder.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>

#include <ctype.h>


#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

const long MINSCORE = 6;

// predefined expressions
static const string NON_TP = " \n\r\taAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ,.&'-";
static const string BORROWER_IS = ") \" borrower \" is|\" borrower \" is|borrower \" is|\" borrower is|) \" borrower|borrower \' is|) borrower .";
static const string LENDER_IS = ") \" lender \" is|\" lender \" is|lender \" is|\" lender is|) \" lender|lender \' is|) lender .";
static const string MORGAGEE = "Mortgagee :|Lender :|Beneficiary :";

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

MERSFinder::MERSFinder()
: m_ipEntityFinder(NULL),
  m_ipSPM(NULL),
  m_ipExprDefined(NULL),
  m_ipDataScorer(NULL)
{
	try
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI29478", m_ipMiscUtils);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29479");
}
//-------------------------------------------------------------------------------------------------
MERSFinder::~MERSFinder()
{
	try
	{
		m_ipEntityFinder = __nullptr;
		m_ipSPM = __nullptr;
		m_ipExprDefined = __nullptr;
		m_ipDataScorer = __nullptr;
		m_ipMiscUtils = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16434");
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
void MERSFinder::findMERS( IIUnknownVectorPtr ipVecEntities, IAFDocumentPtr ipAFDoc )
{
	// ensure pre-requisites
	ASSERT_ARGUMENT("ELI09475", ipVecEntities != __nullptr)
	ASSERT_ARGUMENT("ELI09476", ipAFDoc != __nullptr)
	
	IAFDocumentPtr ipOriginalDoc(ipAFDoc);
	
	// Get the regex parser
	IRegularExprParserPtr ipParser = getMERSParser();

	IIUnknownVectorPtr ipFoundAttributes(ipVecEntities);
	long nSize = ipFoundAttributes->Size();
	for (long n = 0; n < nSize; n++)
	{
		IAttributePtr ipAttribute = ipFoundAttributes->At(n);
		ASSERT_RESOURCE_ALLOCATION("ELI09477", ipAttribute != __nullptr);
		
		ISpatialStringPtr ipAttrValue = ipAttribute->Value;
		// first to check if the value contains MERS keyword
		string strValue = ipAttrValue->String;
		int nStartPos = 0, nEndPos = 0;
		if (findMERSKeyword(strValue, nStartPos, nEndPos, ipParser))
		{
			// if MERS keywords is found in the attribute value, 
			// then get this value
			ISpatialStringPtr ipMERSKeyword = ipAttrValue->GetSubString(nStartPos, nEndPos);
			
			// for each attribute value, call ModifyValue
			findNomineeFor(ipAttribute, ipOriginalDoc, ipParser);
			
			// if the value contains no more MERS keyword
			strValue = ipAttrValue->String;
			if (!findMERSKeyword(strValue, nStartPos, nEndPos, ipParser))
			{
				// if MERS is found the subattributes from previous split are invalid so remove them
				IIUnknownVectorPtr ipSubs = ipAttribute->SubAttributes;
				ASSERT_RESOURCE_ALLOCATION("ELI15603", ipSubs != __nullptr);
				ipSubs->Clear();
				
				// Create Splitter Object
				IAttributeSplitterPtr ipEntitySplitter(CLSID_EntityNameSplitter);
				ASSERT_RESOURCE_ALLOCATION("ELI09809", ipEntitySplitter != __nullptr );

				// Assign mers keyword to the attribute value
				ipAttribute->Value = ipMERSKeyword;

				// Split the new value
				ipEntitySplitter->SplitAttribute( ipAttribute, ipAFDoc, NULL );

				// create an sub attribute called NomineeFor 
				// with the found value as for this attribute
				IAttributePtr ipSubAttr(CLSID_Attribute);
				ASSERT_RESOURCE_ALLOCATION("ELI09478", ipSubAttr != __nullptr);
				ipSubAttr->Name = "NomineeFor";
				ipSubAttr->Value = ipAttrValue;

				// Split the company in the NomineeFor SubAttribute
				ipEntitySplitter->SplitAttribute( ipSubAttr, ipAFDoc, NULL );

				// Place the NomineeFor subattribute under the First split subattribute
				int nNumSubAttr = ipSubs->Size();
				if ( nNumSubAttr > 0 ) 
				{
					// Retrieve the first sub-attribute
					IAttributePtr ipFirstSubAttr = ipSubs->At(0);
					ASSERT_RESOURCE_ALLOCATION( "ELI10187", ipFirstSubAttr != __nullptr );

					// Retrieve the sub-attributes
					IIUnknownVectorPtr ipFirstSubs = ipFirstSubAttr->SubAttributes;
					ASSERT_RESOURCE_ALLOCATION("ELI15604", ipFirstSubs != __nullptr);

					// Add NomineeFor under the first split sub attribute
					ipFirstSubs->PushBack( ipSubAttr );
				}
			}
		}
	}

}

//-------------------------------------------------------------------------------------------------
// private / helper methods
//-------------------------------------------------------------------------------------------------
void MERSFinder::findNomineeFor( IAttributePtr ipAttr, IAFDocumentPtr ipAFDoc,
								IRegularExprParserPtr ipParser )
{
	IAFDocumentPtr ipOriginalDoc(ipAFDoc);
	ASSERT_RESOURCE_ALLOCATION("ELI09479", ipOriginalDoc != __nullptr);
	ISpatialStringPtr ipOriginalText = ipOriginalDoc->Text;
	
	IAttributePtr	ipAttribute(ipAttr);
	ASSERT_RESOURCE_ALLOCATION( "ELI19456", ipAttribute != __nullptr );
	
	// take the text to be modified
	ISpatialStringPtr ipTextToBeModified = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI19360", ipTextToBeModified != __nullptr);
	
	// Allocate data scorer for scoring results
	if ( m_ipDataScorer == __nullptr )
	{
		m_ipDataScorer.CreateInstance(CLSID_EntityNameDataScorer);
		ASSERT_RESOURCE_ALLOCATION("ELI09498", m_ipDataScorer != __nullptr );
	}
	// get the string value of the text
	string strValue = asString(ipTextToBeModified->String);
	
	// If any "MERS" keywords is appearing in the text
	int nStartPos = 0, nEndPos = 0;
	if (findMERSKeyword(strValue, nStartPos, nEndPos, ipParser))
	{
		IAttributePtr ipTempAttr(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI09503", ipTempAttr != __nullptr );
		// search pattern 3 first
		ISpatialStringPtr ipFoundValue = patternSearch3(ipOriginalText);
		ipTempAttr->Value = ipFoundValue;

		string strValueFound = "";
		if ( ipFoundValue != __nullptr )
		{
			strValueFound = asString(ipFoundValue->String);
		}
		long nDataScore = m_ipDataScorer->GetDataScore1( ipTempAttr );
		if (ipFoundValue == __nullptr || findMERSKeyword ( strValueFound, nStartPos, nEndPos, ipParser ) 
				|| nDataScore < MINSCORE )
		{
			// now search pattern 1
			ipFoundValue = patternSearch1(ipOriginalText);
			ipTempAttr->Value = ipFoundValue;
			if ( ipFoundValue != __nullptr )
			{
				strValueFound = ipFoundValue->String;
			}
			nDataScore = m_ipDataScorer->GetDataScore1( ipTempAttr );
			if (ipFoundValue == __nullptr || findMERSKeyword ( strValueFound, nStartPos, nEndPos, ipParser )
				|| nDataScore < MINSCORE)
			{
				// no entity found in the first pattern,  
				// then search pattern 2
				ipFoundValue = patternSearch2(ipOriginalText);
				ipTempAttr->Value = ipFoundValue;
				nDataScore = m_ipDataScorer->GetDataScore1( ipTempAttr );
				if ( nDataScore < MINSCORE )
				{
					ipFoundValue = __nullptr;
				}
			}
		}
		
		ICopyableObjectPtr ipCopier = ipTextToBeModified;
		ASSERT_RESOURCE_ALLOCATION("ELI25966", ipCopier != __nullptr);

		if (ipFoundValue)
		{
			// if find any, update the attribute value
			ipCopier->CopyFrom(ipFoundValue);
		}
		else
		{
			// if MERS keywords is found in the text, 
			// then modify this value to have "MORTGAGE ELECTRONIC 
			// REGISTRATION SYSTEMS" only
			ISpatialStringPtr ipMERSKeyword = ipTextToBeModified->GetSubString(nStartPos, nEndPos);
			ASSERT_RESOURCE_ALLOCATION("ELI25967", ipMERSKeyword != __nullptr);
			ipCopier->CopyFrom(ipMERSKeyword);
		}
	}

}

ISpatialStringPtr MERSFinder::extractEntity(ISpatialStringPtr ipInputText)
{
	if (m_ipEntityFinder == __nullptr)
	{
		m_ipEntityFinder.CreateInstance(CLSID_EntityFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI09481", m_ipEntityFinder != __nullptr);
	}

	// make a copy of the input string
	ICopyableObjectPtr ipCopyable(ipInputText);
	ASSERT_RESOURCE_ALLOCATION("ELI09482", ipCopyable != __nullptr);

	ISpatialStringPtr ipRet(ipCopyable->Clone());
	ASSERT_RESOURCE_ALLOCATION("ELI09483", ipRet != __nullptr);

	m_ipEntityFinder->FindEntities(ipRet);

	// make sure the entity is found
	if (ipRet->String.length() == 0)
	{
		return NULL;
	}

	return ipRet;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr MERSFinder::findMatchEntity(ISpatialString* pInputText, 
												const string& strPattern,
												bool bCaseSensitive,
												bool bExtractEntity,
												bool bGreedy)
{
	// store input text in the smart pointer
	ISpatialStringPtr ipInputText(pInputText);
	ISpatialStringPtr ipResult(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI09484", ipResult != __nullptr);

	// get actual text value out from the input text
	if (ipInputText->String.length() == 0)
	{
		return NULL;
	}

	// find match using string pattern matcher
	IStrToObjectMapPtr ipFoundMatches = match(ipInputText, strPattern, 
		bCaseSensitive, bGreedy);
	ASSERT_RESOURCE_ALLOCATION("ELI09485", ipFoundMatches != __nullptr);

	// Note: in this perticular case, we always expect one and only one
	// value to be found in the input text
	if (ipFoundMatches->Size == 0)
	{
		return NULL;
	}
	else if (ipFoundMatches->Size > 1)
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI09486");
	}

	// get this one and only one found token value
	CComBSTR bstrVariableName;
	IUnknownPtr ipUnkVariableValue;
	ipFoundMatches->GetKeyValue(0, &bstrVariableName, &ipUnkVariableValue);

	ITokenPtr ipToken = ipUnkVariableValue;
	ASSERT_RESOURCE_ALLOCATION("ELI09487", ipToken != __nullptr);
	
	// get start and end position of the found value
	long nStartPos = ipToken->StartPosition;
	long nEndPos = ipToken->EndPosition;
	// make sure the value is not empty
	if (nStartPos >= nEndPos)
	{
		return NULL;
	}

	// Get the substring of the spatial string based on the start and end position
	ipResult = ipInputText->GetSubString(nStartPos, nEndPos);

	// extract the entity out from the value if required
	if (bExtractEntity)
	{
		ipResult = extractEntity(ipResult);
	}

	return ipResult;
}
//-------------------------------------------------------------------------------------------------
bool MERSFinder::findMERSKeyword(const string& strInput, int& nStartPos, int& nEndPos,
								 IRegularExprParserPtr ipParser)
{
	bool bFoundMatch = false;

	// if "MORTGAGE ELECTRONIC REGISTRATION SYSTEMS" is found..
	string strPattern;
	// if the pattern is found, that means the MERS is in the strInput
	IIUnknownVectorPtr ipFoundMatch = ipParser->Find(_bstr_t(strInput.c_str()), VARIANT_TRUE,
		VARIANT_FALSE);

	if (ipFoundMatch->Size() > 0)
	{
		bFoundMatch = true;

		// set start and end position
		IObjectPairPtr ipObjPair = ipFoundMatch->At(0);
		ASSERT_RESOURCE_ALLOCATION("ELI09489", ipObjPair != __nullptr);

		ITokenPtr ipMatch = ipObjPair->Object1;
		ASSERT_RESOURCE_ALLOCATION("ELI09491", ipMatch != __nullptr);

		nStartPos = ipMatch->StartPosition;
		nEndPos = ipMatch->EndPosition;
	}
	
	return bFoundMatch;
}
//-------------------------------------------------------------------------------------------------
bool MERSFinder::isFirstWordStartsUpperCase(const string& strInput)
{
	char cLetter = 0;
	for (unsigned int ui = 0; ui < strInput.size(); ui++)
	{
		cLetter = strInput[ui]; 
		// find first alpha char
		if (::isalpha((unsigned char) cLetter))
		{
			// found one, break out of the loop
			break;
		}
	}

	// if first alpha char is upper case
	if (cLetter != 0 && ::isupper((unsigned char) cLetter))
	{
		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
IStrToObjectMapPtr MERSFinder::match(ISpatialStringPtr& ripInput, 
									   const string& strPattern,
									   bool bCaseSensitive,
									   bool bGreedy)
{
	if (m_ipSPM == __nullptr)
	{
		m_ipSPM.CreateInstance(CLSID_StringPatternMatcher);
		ASSERT_RESOURCE_ALLOCATION("ELI09492", m_ipSPM != __nullptr);
		// by default, case insensitive
		m_ipSPM->CaseSensitive = VARIANT_FALSE;
		// by default, treat multiple white space a one
		m_ipSPM->TreatMultipleWSAsOne = VARIANT_TRUE;
	}

	setPredefinedExpressions();

	// TODO : use case sensitivity
	// vector of IToken
	return m_ipSPM->Match1(ripInput->String, _bstr_t(strPattern.c_str()), 
		m_ipExprDefined, bGreedy ? VARIANT_TRUE : VARIANT_FALSE);
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr MERSFinder::patternSearch1(ISpatialString* pOriginalText)
{
	ISpatialStringPtr ipOriginText(pOriginalText);
	// get the actual input text
	string strInput = ipOriginText->String;
	// pattern
	string strPattern("nominee for Lender^?LenderName");

	// find the match
	ISpatialStringPtr ipFoundString = findMatchEntity(ipOriginText, 
		strPattern, false, false, true);
	if (ipFoundString == __nullptr)
	{
		// if this pattern can't be found in the input string, 
		// no more further search is required.
		return NULL;
	}

	// if the value is found in the original input text
	// Do further searching based on the found string (ipFoundString)...

	ISpatialStringPtr ipEntity(NULL);

	///***************
	// 1) 
	strPattern = "( 888 ) 679 - MERS . The Lender is|( 888 ) 679 - MERS|- MERS^?LenderName^300~organized and existing";
	// try to extract entity out from the original input text
	ipEntity = findMatchEntity(ipOriginText, strPattern, true, true, false);
	if (ipEntity)
	{
		// only return the entity if it's not null
		return ipEntity;
	}

	///***************
	// 2) check to see if the first word in the found string
	// is starting with an upper case alphabetic character
	string strFoundValue = ipFoundString->String;
	if (isFirstWordStartsUpperCase(strFoundValue))
	{	
		// if is, look for the following pattern
		strPattern = "?LenderName^@-NonTP";
		// otherwise, try to extract entity out from the value
		ipEntity = findMatchEntity(ipFoundString, strPattern, true, true, false);
		
		if (ipEntity)
		{
			// only return the entity if it's not null
			return ipEntity;
		}
		// if nothing is found, go on to the next rule
	}
	
	//******************
	// 3) if '"Lender"^is^?^TP' is found...
	strPattern = "\" Lender \"^20~[is]^?LenderName^@-NonTP";
	// This time, pass in the found string, not the original string
	// Also, when calling findMatchEntity(), do not ask for extracting entity
	// at this time, we'll do it if...
	ipEntity = findMatchEntity(ipFoundString, strPattern, false, false, false);
	if (ipEntity)
	{
		// make sure the starting alpha character is an upper case
		strFoundValue = ipEntity->String;
		if (isFirstWordStartsUpperCase(strFoundValue))
		{
			// now extract the entity
			ipEntity = extractEntity(ipEntity);
			if (ipEntity)
			{
				return ipEntity;
			}
		}

		// if not, go onto next rule
	}
	// if nothing is found, go to next rule

	//******************
	// 4) if 'TP^?^"Lender"' is found...
	strPattern = "@-NonTP^?LenderName^(\"Lender|\"Lender\"";
	// This time, pass in the found string, not the original string
	ipEntity = findMatchEntity(ipFoundString, strPattern, false, false, false);
	if (ipEntity)
	{
		// sometimes, the found value contains a leftover MERS string in
		// the beginning.  Search for the word MERS in the found value...and
		// if found, chop off anything to the left of it (including itself).
		strFoundValue = asString(ipEntity->String);
		size_t pos = strFoundValue.find("MERS");
		if (pos != string::npos)
		{
			ipEntity = ipEntity->GetSubString(pos+4, -1);

			// now extract a better entity
			ipEntity = extractEntity(ipEntity);
		}

		// return the found (and possibly corrected) entity
		return ipEntity;
	}
	// if nothing is found, go to next rule

	//******************
	// 5) TODO: Spatially search for the entity text above or below 'name of Lender'

	return NULL;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr MERSFinder::patternSearch2(ISpatialString* pOriginalText)
{
	ISpatialStringPtr ipOriginText(pOriginalText);
	// get the actual input text
	string strInput = ipOriginText->String;
	// pattern
	string strPattern("as a nominee|nominee for|nominee of^?LenderName^400~assignee|successor|address|owner and holder|organized and existing|as lender and beneficiary|ammends and");

	// find the match and extract the entity
	return findMatchEntity(ipOriginText, strPattern, false, true, true);
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr MERSFinder::patternSearch3(ISpatialString* pOriginalText)
{
	// In this pattern set, we need to first make sure
	// the words "nominee for.." exists
	ISpatialStringPtr ipOriginText(pOriginalText);
	// get the actual input text
	string strInput = ipOriginText->String;
	// pattern
	string strPattern("as a nominee|nominee for^?Dummy^@-NonTP");

	// find the match
	ISpatialStringPtr ipFoundString = findMatchEntity(ipOriginText, 
		strPattern, false, false, true);
	if (ipFoundString == __nullptr)
	{
		// if this pattern can't be found in the input string, 
		// no more further search shall be proceeded
		return NULL;
	}

	setPredefinedExpressions();

	// Search for "Borrower" is...borrower is..."Lender" is...lender is
	strPattern = "@LenderIs^?Lender^400~lender is|lender ' s address";
	ipFoundString = findMatchEntity(ipOriginText, strPattern, false, true, false);

	if (ipFoundString)
	{
		return ipFoundString;
	}

	strPattern = "@MortgageeKeyword^?Lender^600~\" MERS|mortgage amount|conveyance|address :|Background";
	ipFoundString = findMatchEntity(ipOriginText, strPattern, false, true, false);

	return ipFoundString;
}
//-------------------------------------------------------------------------------------------------
void MERSFinder::setPredefinedExpressions()
{
	if (m_ipExprDefined == __nullptr)
	{
		m_ipExprDefined.CreateInstance(CLSID_StrToStrMap);
		ASSERT_RESOURCE_ALLOCATION("ELI09493", m_ipExprDefined != __nullptr);
		m_ipExprDefined->Set(_bstr_t("NonTP"), _bstr_t(NON_TP.c_str()));
		m_ipExprDefined->Set(_bstr_t("BorrowerIs"), _bstr_t(BORROWER_IS.c_str()));
		m_ipExprDefined->Set(_bstr_t("LenderIs"), _bstr_t(LENDER_IS.c_str()));
		m_ipExprDefined->Set(_bstr_t("MortgageeKeyword"), _bstr_t(MORGAGEE.c_str()));
	}
}
//-------------------------------------------------------------------------------------------------
IRegularExprParserPtr MERSFinder::getMERSParser()
{
	try
	{
		IRegularExprParserPtr ipMERSParser = m_ipMiscUtils->GetNewRegExpParserInstance("MERSFinder");
		ASSERT_RESOURCE_ALLOCATION("ELI19370", ipMERSParser != __nullptr);

		ipMERSParser->Pattern = "(Mortgage\\s+Electronic\\s+Registration\\s+System(s)?"
			"|\\bMERS\\b|Mortgage[\\s\\S]+?Registration\\s+System(s)?)([\\s\\S]{1,3}Inc(\\s*\\.)?)?";

		return ipMERSParser;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29480");
}
//-------------------------------------------------------------------------------------------------
