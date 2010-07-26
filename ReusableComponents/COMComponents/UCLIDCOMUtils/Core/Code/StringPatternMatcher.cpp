// StringPatternMatcher.cpp : Implementation of CStringPatternMatcher

#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "StringPatternMatcher.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

#include <map>
#include <algorithm>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

// uncomment the following line to enable logging
// #define LOGGING_ENABLED
const char *gpszLOG_FILE = "c:\\spm.txt";

//-------------------------------------------------------------------------------------------------
// CStringPatternMatcher
//-------------------------------------------------------------------------------------------------
CStringPatternMatcher::CStringPatternMatcher()
:m_bCaseSensitive(stringCSIS::isCaseSensitiveByDefault()),
 m_TreatMultipleWSAsOne(true)
{
	// Set case sensitivity for the member strings
	m_strCurrentExprName.setCaseSensitive(m_bCaseSensitive);
	m_strCurrentToken.setCaseSensitive(m_bCaseSensitive);
	m_strLastInput.setCaseSensitive(m_bCaseSensitive);
	m_strText.setCaseSensitive(m_bCaseSensitive);
}
//-------------------------------------------------------------------------------------------------
CStringPatternMatcher::~CStringPatternMatcher()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16506");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPatternMatcher::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IStringPatternMatcher
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IStringPatternMatcher
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPatternMatcher::Match1(BSTR bstrText, BSTR bstrPattern, 
										  IStrToStrMap *pExprMap,
										  VARIANT_BOOL bGreedy,
										  IStrToObjectMap **pMatches)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// lock the object
		CSingleLock lock( &m_lock, TRUE );

		// validate license
		validateLicense();

		// call the match() method common to Match1() and Match2()
		long nPatternStartPos, nPatternEndPos;

		match(bstrText, bstrPattern, pExprMap, bGreedy, &nPatternStartPos, &nPatternEndPos, pMatches);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05875")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPatternMatcher::Match2(BSTR bstrText, BSTR bstrPattern, 
										  IStrToStrMap *pExprMap, 
										  VARIANT_BOOL bGreedy,
										  long *pnPatternStartPos, 
										  long *pnPatternEndPos,
										  IStrToObjectMap **pMatches)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// lock the object
		CSingleLock lock( &m_lock, TRUE );

		// validate license
		validateLicense();

		// call the match() method common to Match1() and Match2()
		match(bstrText, bstrPattern, pExprMap, bGreedy, pnPatternStartPos, pnPatternEndPos, pMatches);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06362")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPatternMatcher::get_CaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// lock the object
		CSingleLock lock( &m_lock, TRUE );

		// validate license
		validateLicense();

		*pVal = m_bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05996")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPatternMatcher::put_CaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// lock the object
		CSingleLock lock( &m_lock, TRUE );

		// validate license
		validateLicense();

		m_bCaseSensitive = (newVal == VARIANT_TRUE);
		
		// Set case sensitivity for the member strings
		m_strCurrentExprName.setCaseSensitive(m_bCaseSensitive);
		m_strCurrentToken.setCaseSensitive(m_bCaseSensitive);
		m_strText.setCaseSensitive(m_bCaseSensitive);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05995")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPatternMatcher::get_TreatMultipleWSAsOne(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// lock the object
		CSingleLock lock( &m_lock, TRUE );

		// validate license
		validateLicense();

		*pVal = m_TreatMultipleWSAsOne ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06258")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPatternMatcher::put_TreatMultipleWSAsOne(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// lock the object
		CSingleLock lock( &m_lock, TRUE );

		// validate license
		validateLicense();

		m_TreatMultipleWSAsOne = (newVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06259")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private / Helper functions
//-------------------------------------------------------------------------------------------------
void CStringPatternMatcher::performNonGreedyLeftTrim(long nToken)
{
	ASSERT_ARGUMENT("ELI09274", nToken > 0);

	// get the current token
	SPMTokenInfo& rTokenInfo = m_vecTokenInfo[nToken];

	// if this is token n where n > 1, then when trying to match token[n - 1],
	// we need to check for max-ignore-chars constraint since the end of
	// token [n - 2]
	// if this token n where n = 1, we don't need to worry about the max-ignore
	// chars constraint because if a match variable is used in the second token
	// the first token cannot use the max-ignore-chars constraint (as there is 
	// no token previous to it)
	long nPreviousTokenStartSearchPos = 0;

	if (nToken > 1)
	{
		const SPMTokenInfo& prevToPrevTokenInfo = m_vecTokenInfo[nToken - 2];
		nPreviousTokenStartSearchPos = prevToPrevTokenInfo.m_nMatchEndPos + 1;
	}

	// keep trimming as much as possible until we run past our current
	// match attribute's value
	bool bTrimSuccessful;
	do
	{
		bTrimSuccessful = false;

		// try to find the token
		long nTokenStartPos, nTokenEndPos;
		bool bConstraintMet = false;
		bool bTokenFound = findToken(nToken - 1, rTokenInfo.m_nMatchStartPos + 1, 
			nPreviousTokenStartSearchPos, nTokenStartPos, nTokenEndPos, 
			bConstraintMet);
		
		if (bTokenFound && bConstraintMet)
		{
			// a token was found.  If it was before the end of
			// the current match's value, then trim and try again
			if (nTokenEndPos < rTokenInfo.m_nMatchEndPos)
			{
				rTokenInfo.m_nMatchStartPos = nTokenEndPos + 1;
				bTrimSuccessful = true;

				// adjust the token match information for the previous token
				if (nToken > 0)
				{
					m_vecTokenInfo[nToken - 1].m_nMatchStartPos = nTokenStartPos;
					m_vecTokenInfo[nToken - 1].m_nMatchEndPos = nTokenEndPos;
				}
			}
		}
	} while (bTrimSuccessful);
}
//-------------------------------------------------------------------------------------------------
void CStringPatternMatcher::performGreedyRightExtend(long nToken)
{
	ASSERT_ARGUMENT("ELI09278", (unsigned long) nToken < m_vecTokenInfo.size() - 1);

	// get the current token
	SPMTokenInfo& rTokenInfo = m_vecTokenInfo[nToken];

	// NOTE: for trimming to the right of a match variable, we don't need
	// to worry about max-ignore-chars constraint because the token immediately
	// following a match variable token cannot contain max-ignore-chars constraint

	// keep trimming as much as possible until we run into the next-next-token's
	// value (if there is a next-next-token), or until the end of the input (if
	// there is only one token after the current token)
	long nMaximumNextTokenEndPos;
	if ((unsigned long) nToken < m_vecTokenInfo.size() - 2)
	{
		const SPMTokenInfo& nextNextTokenInfo = m_vecTokenInfo[nToken + 2];
		nMaximumNextTokenEndPos = nextNextTokenInfo.m_nMatchStartPos - 1;
	}
	else
	{
		nMaximumNextTokenEndPos = m_strText.length() - 1;
	}

	bool bTrimSuccessful;
	do
	{
		bTrimSuccessful = false;

		// try to find the token
		long nTokenStartPos, nTokenEndPos;
		bool bConstraintMet = false;
		bool bTokenFound = findToken(nToken + 1, rTokenInfo.m_nMatchEndPos + 2, 
			0, nTokenStartPos, nTokenEndPos, bConstraintMet);
		
		if (bTokenFound && bConstraintMet)
		{
			// a token was found.
			if (nTokenEndPos <= nMaximumNextTokenEndPos)
			{
				rTokenInfo.m_nMatchEndPos = nTokenStartPos - 1;
				bTrimSuccessful = true;
			}

			// adjust the token match information for the next token
			if ((unsigned long) nToken < m_ulNumTokens - 1)
			{
				m_vecTokenInfo[nToken + 1].m_nMatchStartPos = nTokenStartPos;
				m_vecTokenInfo[nToken + 1].m_nMatchEndPos = nTokenEndPos;
			}
		}
	} while (bTrimSuccessful);
}
//-------------------------------------------------------------------------------------------------
void CStringPatternMatcher::convertMatchesToGreedySpecification()
{
	// iterate through the tokens and find the match variables, and make
	// their values conform to the non-greedy concept
	long m_ulNumTokens = m_vecTokenInfo.size();
	for (int i = 0; i < m_ulNumTokens; i++)
	{
		// get the current token
		const SPMTokenInfo& tokenInfo = m_vecTokenInfo[i];

		// if the current token is a match variable, then try to narrow 
		// down the token value to be non-greedy
		if (tokenInfo.isMatchVariable())
		{
			// NOTE: the way this algorithm works, by default the left
			// side of the match variable has been captured in a greedy way,
			// and the right side of the match variable has been captured in
			// a non-greedy way.  So, depending upon the greedy specification,
			// we may have to trim on the left, or extend on the right

			// if this is not the first token, then 
			// there's an opportunity to do trimming on the left
			if (i > 0 && !tokenInfo.m_bMatchGreedyOnLeft)
			{
				performNonGreedyLeftTrim(i);
			}

			// if this is not the last token, then
			// there's some opportunity to do trimming on the right
			if (i < m_ulNumTokens - 1 && tokenInfo.m_bMatchGreedyOnRight)
			{
				performGreedyRightExtend(i);
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CStringPatternMatcher::match(BSTR bstrText, BSTR bstrPattern, 
								  IStrToStrMap *pExprMap, 
								  VARIANT_BOOL bGreedy,
								  long *pnPatternStartPos, 
								  long *pnPatternEndPos,
								  IStrToObjectMap **pMatches)
{
	// convert the incoming BSTR's arguments into string's
	// for easier processing
	m_strText = stringCSIS( asString( bstrText ), m_bCaseSensitive );

	string strPattern = asString( bstrPattern );

	// remember the default greedy flag
	m_bGreedyByDefault = (bGreedy == VARIANT_TRUE);

	// if the text is different than it was the last time, clear the map
	// of literal search strings
	if (m_strText != m_strLastInput || m_strText.isCaseSensitive() != m_strLastInput.isCaseSensitive())
	{
		m_mapLiteralToSearchData.clear();
	}

	// convert the incoming COM based string-to-string map into an STL 
	// map<string, string> for easier processing.
	m_mapExpressions.clear();
	UCLID_COMUTILSLib::IStrToStrMapPtr ipMap(pExprMap);

	for (int j = 0; j < ipMap->Size; j++)
	{
		CComBSTR key, value;
		ipMap->GetKeyValue(j, &key.m_str, &value.m_str);

		stringCSIS stdstrKey( asString(key), m_bCaseSensitive);
		stringCSIS stdstrValue ( asString(value), m_bCaseSensitive);

		m_mapExpressions[stdstrKey] = stdstrValue;
	}

#ifdef LOGGING_ENABLED
	{
		ofstream outfile(gpszLOG_FILE, ios::app);
		outfile << "Searching for pattern: ";
		outfile << strPattern << endl;

		// Close the file and wait for it to be readable
		outfile.close();
		waitForFileAccess(gpszLOG_FILE, giMODE_READ_ONLY);
	}
#endif

	// tokenize the input pattern by using ^
	vector<string> vecTokens;
	StringTokenizer st('^');
	st.parse(strPattern, vecTokens);

	// check syntax of the pattern string to ensure
	// that there are no syntax errors and populate the
	// m_vecTokenInfo vector
	validateTokens(vecTokens);

	// all the matches are stored in m_ipMatches, which is a 
	// IIUnknownVector of IToken objects.
	UCLID_COMUTILSLib::IStrToObjectMapPtr ipMatches;

	// process each of the tokens in the pattern string starting at token 0
	long nStartPos = 0, nEndPos = 0;
	bool bStopSearching = false;
	if (!m_strText.empty() && processTokens(0, 0, nStartPos, nEndPos, bStopSearching))
	{
		// convert matches to specification of the greedy attribute
		convertMatchesToGreedySpecification();

		// get the matches to return to the caller
		ipMatches = getMatches();
		ASSERT_RESOURCE_ALLOCATION("ELI09275", ipMatches != NULL);

		*pnPatternStartPos = nStartPos;
		*pnPatternEndPos = nEndPos;
	}
	else
	{
		// create an empty map to return to the caller
		ipMatches.CreateInstance(CLSID_StrToObjectMap);
		ASSERT_RESOURCE_ALLOCATION("ELI06268", ipMatches != NULL);

		ipMatches->CaseSensitive = m_bCaseSensitive;
	}

	// Update the last text searched variable so that subsequent
	// searches on the same text go faster
	m_strLastInput = m_strText;
	m_strLastInput.setCaseSensitive(m_strText.isCaseSensitive());

	// set the IIUnknownVector object to be returned to the caller
	CComQIPtr<IStrToObjectMap> ipTemp = ipMatches;
	*pMatches = ipTemp.Detach();
}
//-------------------------------------------------------------------------------------------------
UCLID_COMUTILSLib::IStrToObjectMapPtr CStringPatternMatcher::getMatches() const
{
	// create the object to be returned
	UCLID_COMUTILSLib::IStrToObjectMapPtr ipMatches(CLSID_StrToObjectMap);
	ASSERT_RESOURCE_ALLOCATION("ELI09276", ipMatches != NULL);

	ipMatches->CaseSensitive = m_bCaseSensitive;

	// iterate through the tokens and for each match variable 
	long m_ulNumTokens = m_vecTokenInfo.size();
	for (int i = 0; i < m_ulNumTokens; i++)
	{
		// get the current token info
		const SPMTokenInfo& tokenInfo = m_vecTokenInfo[i];

		if (tokenInfo.isMatchVariable())
		{
			// create a token object to represent the match
			UCLID_COMUTILSLib::ITokenPtr ipToken(CLSID_Token);
			ASSERT_RESOURCE_ALLOCATION("ELI05917", ipToken != NULL);

			// extract the match text and add to result vector
			string strMatch(static_cast<string>(m_strText), tokenInfo.m_nMatchStartPos, 
				tokenInfo.m_nMatchEndPos - tokenInfo.m_nMatchStartPos + 1);

			// initialize the token with the match position and value
			ipToken->InitToken(tokenInfo.m_nMatchStartPos, tokenInfo.m_nMatchEndPos, 
				_bstr_t(""), _bstr_t(strMatch.c_str()));

			// get the match variable name
			const stringCSIS& strVariableName = tokenInfo.m_strExprOrVariableName;

			// add the token to the vector of found matches
			ipMatches->Set(_bstr_t(strVariableName.c_str()), ipToken);
		}
	}

	return ipMatches;
}
//-------------------------------------------------------------------------------------------------
bool CStringPatternMatcher::findToken(unsigned long nStartToken, 
		long nSearchStartPos, long nCompareStartPos, long& rnThisTokenStartPos, 
		long& rnThisTokenEndPos, bool& rbConstraintMet)
{
	bool bTokenFound = false;

	// get access to the token info object
	const SPMTokenInfo& tokenInfo = m_vecTokenInfo[nStartToken];

	// try to find the token
	switch (tokenInfo.m_eTokenType)
	{
	case kDesiredMatch:
		// nothing to do
		break;

	case kLiteralOrList:
		bTokenFound = processLiteralOrList(tokenInfo.m_strToken, nSearchStartPos, 
			rnThisTokenStartPos, rnThisTokenEndPos);
		break;

	case kCharMustNotMatch:
		bTokenFound = processCharMustNotMatchToken(nStartToken, nSearchStartPos, 
			rnThisTokenStartPos, rnThisTokenEndPos);
		break;

	case kCharMustMatch:
		bTokenFound = processCharMustMatchToken(nStartToken, nSearchStartPos, 
			rnThisTokenStartPos, rnThisTokenEndPos);
		break;

	case kExpression:
		bTokenFound = processExpressionToken(nStartToken, nSearchStartPos, 
			rnThisTokenStartPos, rnThisTokenEndPos);
		break;

	};

	// if we found a match, check that it meets the constraints
	if (bTokenFound)
	{
		// calculate if the actual number of characters that were ignored
		// since the end of the last match
		unsigned long nActualIgnoreChars = (rnThisTokenStartPos - nCompareStartPos);

		// if the max-ignore-chars constraint was not met,
		// then we actually did not find the token
		// NOTE: The max-ignore chars constraint does not matter for the first
		// token
		rbConstraintMet = nStartToken == 0 ? true : 
			(nActualIgnoreChars <= tokenInfo.m_ulMaxIgnoreChars);
	}

	return bTokenFound;
}
//-------------------------------------------------------------------------------------------------
bool CStringPatternMatcher::processTokens(unsigned long nStartToken, 
		long nProcessingStartPos, long& rnPatternStartPos, long& rnPatternEndPos,
		bool& rbStopSearching)
{
	// get access to the token info object
	SPMTokenInfo& rTokenInfo = m_vecTokenInfo[nStartToken];

	// determine if the current token is a match variable
	bool bTokenIsMatchVariable = rTokenInfo.isMatchVariable();

	// determine if we are processing the first or last token
	bool bIsFirstToken = (nStartToken == 0);
	bool bIsLastToken = (nStartToken == m_ulNumTokens - 1);

	// determine if the previous token is a match variable
	bool bPreviousTokenIsMatchVariable = (nStartToken > 0 && 
		 m_vecTokenInfo[nStartToken - 1].isMatchVariable());

	// we want to capture a match if (we're processing the second token
	// and the first token is a match variable), or (if we are processing
	// the last token and the last token is a match variable)
	bool bCaptureMatch = (bPreviousTokenIsMatchVariable ||
		(bTokenIsMatchVariable && (bIsLastToken || bIsFirstToken)));

	// get the current token name
	const stringCSIS& strCurrentToken = rTokenInfo.m_strToken;

	long nCurPos = nProcessingStartPos;

	do
	{
#ifdef LOGGING_ENABLED
		{
			ofstream outfile(gpszLOG_FILE, ios::app);

			string strMsg = "Searching for token ";
			strMsg += asString(nStartToken);
			strMsg += " starting at ";
			strMsg += asString(nCurPos);

			outfile << strMsg << endl;
			outfile << "  Token is <" << rTokenInfo.m_strToken << ">" << endl;

			// Close the file and wait for it to be readable
			outfile.close();
			waitForFileAccess(gpszLOG_FILE, giMODE_READ_ONLY);
		}
#endif

		// by default, we want to keep searching at higher levels if finding
		// of this token fails
		rbStopSearching = false;

		// try to find the token
		long nThisTokenStartPos, nThisTokenEndPos;
		bool bConstraintMet = false;
		bool bTokenOriginallyFound = findToken(nStartToken, nCurPos, nCurPos, 
			nThisTokenStartPos, nThisTokenEndPos, bConstraintMet);

#ifdef LOGGING_ENABLED
		{
			ofstream outfile(gpszLOG_FILE, ios::app);
			
			if (bTokenOriginallyFound)
			{
				outfile << "  Token found at {";
				outfile << nThisTokenStartPos << "," << nThisTokenEndPos << "}" << endl;
				if (bConstraintMet)
				{
					outfile << "  Token constraint met" << endl;
				}
				else
				{
					outfile << "  Token constraint NOT met" << endl;
				}
			}
			else if (!bTokenIsMatchVariable)
			{
				outfile << "  Token not found!" << endl;
			}

			// Close the file and wait for it to be readable
			outfile.close();
			waitForFileAccess(gpszLOG_FILE, giMODE_READ_ONLY);
		}
#endif

		// if the token was found, but the constraint was not met,
		// then maybe there's a different instance of the 
		// previous token that could be found for which this token's
		// constraint would have been met.  But if we didn't find
		// the token at all, then there's no point in searching anymore.
		// We will never find a match for this pattern
		if (!bTokenOriginallyFound && !bTokenIsMatchVariable)
		{
			rbStopSearching = true;
			return false;
		}

		// at this point, we need to consider the token as found only
		// if the constraint was also met
		bool bTokenFound = bTokenOriginallyFound && bConstraintMet;

		// update the token position information if the token was found.
		if (bTokenFound)
		{
			rTokenInfo.m_nMatchStartPos = nThisTokenStartPos;
			rTokenInfo.m_nMatchEndPos = nThisTokenEndPos;
		}

		// if the token was not found, but we were supposed to find one,
		// then return false
		if (!bTokenFound && !bTokenIsMatchVariable)
		{
			return false;
		}

		// at this point in the code we are guaranteed that
		// (a) the token was found, or
		// (b) no token was searched for because we're 
		//	   processing the match variable token
		
		// if the current token is a match variable, then return
		// the state of matches to all subsequent tokens
		if (bTokenIsMatchVariable && !bIsLastToken)
		{
			return processTokens(nStartToken + 1, nProcessingStartPos, 
				rnPatternStartPos, rnPatternEndPos, rbStopSearching);
		}

		// if we just processed the first token and the first token
		// was not a variable, then mark the location of the
		// first token.  We only need to do this if we found a match
		// for our first token
		if (nStartToken == 0)
		{
			rnPatternStartPos = bTokenIsMatchVariable ? 
				nCurPos : nThisTokenStartPos;
		}

		// if we just processed the last token, then remember the end
		// of the last token
		if (bIsLastToken)
		{
			rnPatternEndPos  = bTokenIsMatchVariable ?
				m_strText.length() - 1 : nThisTokenEndPos;
		}

		// if we're supposed to be capturing a match, and we
		// have processed a token other than the "?" token, then
		// capture the match
		// NOTE: If the last token is a question mark, then we 
		// want to capture all the remaining text in the input string
		if (bCaptureMatch)
		{
			int iMatchStartPos = nProcessingStartPos;

			// calculate the ending position of the match
			// depending upon whether the current token is a variable
			int iMatchEndPos;
			if (bTokenFound)
			{
				iMatchEndPos = nThisTokenStartPos - 1;
			}
			else if (bTokenIsMatchVariable && bIsLastToken)
			{
				iMatchEndPos = m_strText.length() - 1;
			}
			else
			{
				// we should never reach here
				THROW_LOGIC_ERROR_EXCEPTION("ELI09257")
			}

			// if endpos < start pos for the match, that means that
			// we had a NULL match, which is not intended to be captured
			// A NULL match for the variable in this situation for example:
			// input: abc123
			// pattern: abc^?var^123
			if (iMatchEndPos >= iMatchStartPos)
			{
				// update the position attributes of the token
				if (bTokenIsMatchVariable)
				{
					rTokenInfo.m_nMatchStartPos = iMatchStartPos;
					rTokenInfo.m_nMatchEndPos = iMatchEndPos;
				}
				else
				{
					SPMTokenInfo& rPreviousToken = m_vecTokenInfo[nStartToken - 1];
					rPreviousToken.m_nMatchStartPos = iMatchStartPos;
					rPreviousToken.m_nMatchEndPos = iMatchEndPos;
				}
			}
			else
			{
				// a null match is not allowed
				return false;
			}
		}

		// if we found a token, start searching for the token again, or
		// end the recursion if that makes sense.
		if (bIsLastToken && (bTokenFound || bTokenIsMatchVariable))
		{
			// We found a token, and it was the last token to be found
			// we are done.  Or we processed the last token which was a
			// match variable - either way, return true and end the recursion
			return true;
		}
		else if (bTokenFound)
		{
			// we found the token, and it was not the last token, so there's
			// more to be found.  This "instance" of the recursion is successful
			// only if all subsequent recursions to find subsequent tokens
			// are successful.

			// try to find all subsequent tokens
			bool bResult = processTokens(nStartToken + 1, nThisTokenEndPos + 1, 
				rnPatternStartPos, rnPatternEndPos, rbStopSearching);

			// if we found all subsequent tokens, this instance of the recursion
			// was successful
			if (bResult)
			{
				return true;
			}
			else if (rbStopSearching)
			{
				return false;
			}
			else
			{
				// we did not find all subsequent tokens.  Try to see if there
				// is another instance of the current token and whether the 
				// subsequent tokens can be found after the next instance of
				// the current token

				// continue searching for the next instance of this token
				nCurPos = nThisTokenEndPos + 1;

				// the earliest match that we can find in the next iteration
				// of the loop is nCurPos.  If that is too far (with respect
				// to max-ignore-chars constraint), then no point in searching
				// - just return false
				// NOTE: The max-ignore-chars constraint does not matter for
				// the first token
				if (!bIsFirstToken && nCurPos - (unsigned long) nProcessingStartPos > 
					rTokenInfo.m_ulMaxIgnoreChars)
				{
					return false;
				}
			}
		}
	} while (true);

	// we should never reach here
	THROW_LOGIC_ERROR_EXCEPTION("ELI09270");
}
//-------------------------------------------------------------------------------------------------
bool CStringPatternMatcher::stringContainsCharacters(const stringCSIS& strInput, 
													 const stringCSIS& strCharsToCheck)
{
	return strInput.find_first_of(strCharsToCheck) != string::npos;
}
//-------------------------------------------------------------------------------------------------
void CStringPatternMatcher::validateCharList(const stringCSIS& strCharList,
											 const stringCSIS& strToken, 
											 const int iTokenNum)
{
	// the list of chars must not be empty
	if (strCharList.empty())
	{
		UCLIDException ue("ELI05942", "Invalid character list invoked in pattern!");
		ue.addDebugInfo("Token#", iTokenNum);
		ue.addDebugInfo("Token", static_cast<string>(strToken));
		ue.addDebugInfo("CharList", static_cast<string>(strCharList));
		throw ue;
	}

	// the char list may not contain the the special characters
	if (stringContainsCharacters(strCharList, "@?^|~"))
	{
		UCLIDException ue("ELI05958", "Invalid syntax in pattern - cannot use special characters in character list!");
		ue.addDebugInfo("Token#", iTokenNum);
		ue.addDebugInfo("Token", static_cast<string>(strToken));
		ue.addDebugInfo("CharList", static_cast<string>(strCharList));
		throw ue;
	}

	// TODO: do further checking when char list supports character ranges,
	// hex chars, escape sequence, etc.
}
//-------------------------------------------------------------------------------------------------
void CStringPatternMatcher::validateExpressionName(const stringCSIS& strExpressionName,
												   const stringCSIS& strToken, 
												   const int iTokenNum)
{
	// the name of the expression must not be empty, and it must be
	// found in our expression name-to-value map
	if (strExpressionName.empty() ||
		m_mapExpressions.find(strExpressionName) == m_mapExpressions.end())
	{
		UCLIDException ue("ELI05941", "Invalid named expression in pattern!");
		ue.addDebugInfo("Token#", iTokenNum);
		ue.addDebugInfo("Token", static_cast<string>(strToken));
		ue.addDebugInfo("ExpressionName", static_cast<string>(strExpressionName));
		throw ue;
	}

	// the expression name may not contain the the special characters
	if (stringContainsCharacters(strExpressionName, "@?^|~"))
	{
		UCLIDException ue("ELI05959", "Invalid syntax in pattern - cannot use special characters in character list!");
		ue.addDebugInfo("Token#", iTokenNum);
		ue.addDebugInfo("Token", static_cast<string>(strToken));
		ue.addDebugInfo("CharList", static_cast<string>(strExpressionName));
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CStringPatternMatcher::validateLiteralOrList(const stringCSIS& strLiteralOrList,
												  const stringCSIS& strToken, 
												  const int iTokenNum)
{
	// make sure the literal-or-list is not empty
	if (strLiteralOrList.empty())
	{
		UCLIDException ue("ELI05882", "Invalid token in pattern - token is empty!");
		ue.addDebugInfo("Token#", iTokenNum);
		ue.addDebugInfo("Token", static_cast<string>(strToken));
		throw ue;
	}

	// the literal "or-list" may not contain the the special characters other than |
	if (stringContainsCharacters(strLiteralOrList, "@?^~"))
	{
		UCLIDException ue("ELI05944", "Invalid syntax in pattern - cannot use special characters in literals!");
		ue.addDebugInfo("Token#", iTokenNum);
		ue.addDebugInfo("Token", static_cast<string>(strToken));
		ue.addDebugInfo("LiteralOrList", static_cast<string>(strLiteralOrList));
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CStringPatternMatcher::validateTokens(const vector<string>& vecTokens)
{
	// TODO: ensure that two match variables cannot appear in sequence

	// clear the token info vector
	m_vecTokenInfo.clear();

	vector<string> vecMatchVariableNames;

	// process each token and make sure there are no syntax problems
	long nNumTokens = vecTokens.size();
	for (int i = 0; i < nNumTokens; i++)
	{
		SPMTokenInfo tokenInfo;

		// get the token and analyze it
		tokenInfo.m_strToken = stringCSIS(vecTokens[i], m_bCaseSensitive);

		// Do the analysis that should be done before calling analyzeToken()
		// there cannot be more than 1 tilde in a token, and a tilde
		// cannot be used in a match-variable token
		size_t pos = tokenInfo.m_strToken.find('~');
		if (pos != string::npos)
		{
			if (tokenInfo.m_strToken.find('~', pos + 1) != string::npos)
			{
				UCLIDException ue("ELI06417", "There cannot be more than one tilde (~) in a token!");
				ue.addDebugInfo("Token#", i);
				ue.addDebugInfo("Token", static_cast<string>(tokenInfo.m_strToken));
				throw ue;
			}
			else if (tokenInfo.m_strToken.find('?') != string::npos)
			{
				UCLIDException ue("ELI06419", "Maximum number of ignore characters cannot be specified for a match-variable token!");
				ue.addDebugInfo("Token#", i);
				ue.addDebugInfo("Token", static_cast<string>(tokenInfo.m_strToken));
				throw ue;
			}
		}

		// analyze the token
		tokenInfo.m_eTokenType = analyzeToken(tokenInfo.m_strToken, 
			tokenInfo.m_strExprOrVariableName, tokenInfo.m_ulMaxIgnoreChars);
		string strTokenName = tokenInfo.m_strExprOrVariableName;

		// if the token contains the character '?', make sure there is nothing
		// preceeding the ?, and that there is at most one ? in the token
		if (tokenInfo.m_strToken.find('?') != string::npos)
		{
			if (tokenInfo.m_strToken[0] != '?' || 
				tokenInfo.m_strToken.find('?', 1) != string::npos)
			{
				UCLIDException ue("ELI06265", "Invalid usage of '?' in pattern!");
				ue.addDebugInfo("Token#", i);
				ue.addDebugInfo("Token", static_cast<string>(tokenInfo.m_strToken));
				throw ue;
			}
			else
			{
				// we have come across a match variable
				// set the defaults for the greedy/NonGreedy flags
				// depending upon the Greedy setting
				tokenInfo.m_bMatchGreedyOnLeft = m_bGreedyByDefault;
				tokenInfo.m_bMatchGreedyOnRight = m_bGreedyByDefault;

				// overwrite the default greedy setting with any
				// specification specific to this match variable
				// which will be specified by '<' or '>' characters
				// at the beginning or end of the expression name
				stringCSIS& rVarName = tokenInfo.m_strExprOrVariableName;
				char cVarNameFirstChar = rVarName[0];
				if (cVarNameFirstChar == '<')
				{
					tokenInfo.m_bMatchGreedyOnLeft = true;
					rVarName.erase(0, 1);
				}
				else if (cVarNameFirstChar == '>')
				{
					tokenInfo.m_bMatchGreedyOnLeft = false;
					rVarName.erase(0, 1);
				}

				// the variable name must be greater than 1 char
				// at this time
				unsigned long ulLength = rVarName.length();
				if (ulLength == 0)
				{
					throw UCLIDException("ELI09467", "Invalid match variable name!");
				}

				// overwrite the default greedy setting for the
				// right side of the variable
				unsigned long ulLastCharIndex = ulLength - 1;
				char cVarNameLastChar = rVarName[ulLastCharIndex];
				if (cVarNameLastChar == '<')
				{
					tokenInfo.m_bMatchGreedyOnRight = false;
					rVarName.erase(ulLastCharIndex, 1);
				}
				else if (cVarNameLastChar == '>')
				{
					tokenInfo.m_bMatchGreedyOnRight = true;
					rVarName.erase(ulLastCharIndex, 1);
				}

				// the variable name must again be greater than 1 char
				// at this time
				ulLength = rVarName.length();
				if (ulLength == 0)
				{
					throw UCLIDException("ELI09469", "Invalid match variable name!");
				}
			}
		}

		// if the token contains the character '@', make sure there is nothing
		// preceeding the @, and that there is at most one @ in the token
		if (tokenInfo.m_strToken.find('@') != string::npos &&
			(tokenInfo.m_strToken[0] != '@' || 
		    tokenInfo.m_strToken.find('@', 1) != string::npos))
		{
			UCLIDException ue("ELI05943", "Invalid usage of '@' in pattern!");
			ue.addDebugInfo("Token#", i);
			ue.addDebugInfo("Token", static_cast<string>(tokenInfo.m_strToken));
			throw ue;
		}

		// for each token type, do the appropriate validation
		switch (tokenInfo.m_eTokenType)
		{
		case kDesiredMatch:
			if (!containsNonWhitespaceChars(strTokenName))
			{
				// match variable must contain at least one non-whitespace char
				UCLIDException ue("ELI06267", "Invalid match variable name!");
				ue.addDebugInfo("Variable", strTokenName);
				throw ue;
			}
			else if (vectorContainsElement(vecMatchVariableNames, strTokenName))
			{
				// the match variable cannot be used twice in the same pattern
				UCLIDException ue("ELI06266", "Match variable already defined!");
				ue.addDebugInfo("Variable", strTokenName);
				throw ue;
			}
			else
			{
				// This is a newly used match variable name...remember it
				// to make sure it's not used again.
				vecMatchVariableNames.push_back(strTokenName);
			}

			break;

		case kLiteralOrList:
			validateLiteralOrList(tokenInfo.m_strToken, tokenInfo.m_strToken, i);
			break;

		case kExpression:
			validateExpressionName(tokenInfo.m_strExprOrVariableName, 
				tokenInfo.m_strToken, i);
			validateLiteralOrList(
				m_mapExpressions[tokenInfo.m_strExprOrVariableName], 
				tokenInfo.m_strToken, i);
			break;

		case kCharMustMatch:
			validateExpressionName(tokenInfo.m_strExprOrVariableName, 
				tokenInfo.m_strToken, i);
			validateCharList(
				m_mapExpressions[tokenInfo.m_strExprOrVariableName], 
				tokenInfo.m_strToken, i);
			break;

		case kCharMustNotMatch:
			validateExpressionName(tokenInfo.m_strExprOrVariableName, 
				tokenInfo.m_strToken, i);
			validateCharList(
				m_mapExpressions[tokenInfo.m_strExprOrVariableName], 
				tokenInfo.m_strToken, i);
			break;
		}

		// if we reached here, the token is fine...add it to the
		// vector of tokens
		m_vecTokenInfo.push_back(tokenInfo);
	}

	// update the number of tokens so that we don't have to
	// keep querying for it during processing
	m_ulNumTokens = m_vecTokenInfo.size();
}
//-------------------------------------------------------------------------------------------------
bool CStringPatternMatcher::isWordBoundaryConstraintChar(char cChar) const
{
	// return true if cChar is a character representing word boundary in
	// the pattern string
	return (cChar == ']' || cChar == '[');
}
//-------------------------------------------------------------------------------------------------
bool CStringPatternMatcher::isWordBoundaryChar(char cChar) const
{
	// return true if cChar is a character representing a word boundary 
	// i.e. it is a non alpha-numeric character
	// in the text where matches are being searched
	bool bIsWordBoundaryChar = !::isalnum((unsigned char) cChar);
	return bIsWordBoundaryChar;
}
//-------------------------------------------------------------------------------------------------
bool CStringPatternMatcher::findNextInstanceOfLiteralIgnoreMWS(
	const stringCSIS& strLiteral, long nStartSearchPos, long& rnTokenStartPos, 
	long& rnTokenEndPos)
{
	// find all spaces in the literal.  Each space in the literal represents
	// one or more consequtive spaces in the actual input string
	vector<string> vecTokens;
	StringTokenizer st(' ');
	string strForParse(strLiteral);
	// trim off leading and trailing white spaces
	strForParse = ::trim(strForParse, " \t\r\n", " \t\r\n");

	st.parse(strForParse, vecTokens);
	long nNumTokens = vecTokens.size();
	if (!m_TreatMultipleWSAsOne || nNumTokens < 2)
	{
		return findNextInstanceOfLiteral(strLiteral, nStartSearchPos, 
			rnTokenStartPos, rnTokenEndPos);
	}
	else
	{
		// ensure that there's at least two tokens
		ASSERT_ARGUMENT("ELI06261", nNumTokens >= 2);

		// get the first token
		string strFirstToken = vecTokens[0];

		// we need to search for multiple instances of the first
		// token until we find an instance of the first token that's
		// also followed by the other tokens
		// we need to do this to take care of situations where
		// we want to find word1 followed by word2 followed by word3, and
		// the input looks like this: "word1 word2 xxxx word1 word2 word3 yyyy"
		int iFirstTokenStartSearchPos = nStartSearchPos;
		do
		{
			long nFirstTokenStartPos, nFirstTokenEndPos;

			// find the next instance of the first token in the 
			// input string
			// if we don't find a match, then return false
			if (!findNextInstanceOfLiteral(stringCSIS(strFirstToken, m_bCaseSensitive), 
				iFirstTokenStartSearchPos, nFirstTokenStartPos, 
				nFirstTokenEndPos))
			{
				return false;
			}

			// there are multiple words separated by spaces.
			// find each word, and then ignore all following whitespace chars
			// and then search for the the next word and ensure that the next
			// word immediately follows the last whitespace char.
			
			int iNextTokenExpectedStartPos = nFirstTokenEndPos + 1;
			for (int i = 1; i < nNumTokens; i++)
			{
				// forward the position of the next expected token
				// by ignoring any whitespace since the end of the last
				// found token
				while (isWhitespaceChar(m_strText[iNextTokenExpectedStartPos]))
				{
					iNextTokenExpectedStartPos++;
				}

				// find the next instance of the current token in the 
				// input string.  if that fails, return false
				string strThisToken = vecTokens[i];
				long nThisTokenStartPos, nThisTokenEndPos;
				if (!findNextInstanceOfLiteral(stringCSIS(strThisToken, m_bCaseSensitive),
					iNextTokenExpectedStartPos, nThisTokenStartPos, nThisTokenEndPos))
				{
					return false;
				}
				
				// if we found a word but, not at the expected location, 
				// then search again for another instance of the first token
				if (nThisTokenStartPos != iNextTokenExpectedStartPos)
				{
					iFirstTokenStartSearchPos = nFirstTokenEndPos + 1;
					break;
				}
				else
				{
					// we found a word that we're looking for.  Next,
					// ignore all whitespace chars that follow the word
					// that we just found.  We need to do this only if
					// we're not processing the last token
					if (i == nNumTokens - 1)
					{
						// if we reached here, that means that all the words were found
						// in the original document and in between the words there existed
						// one or more whitespace chars.  The search was successful, so update
						// the caller's
						rnTokenStartPos = nFirstTokenStartPos;
						rnTokenEndPos = nThisTokenEndPos;
						return true;
					}

					// remember the location at which the next word must
					// be found for the search to not be stopped.
					iNextTokenExpectedStartPos = nThisTokenEndPos + 1;
				}
			}
		} while (true);
	}
}
//-------------------------------------------------------------------------------------------------
bool CStringPatternMatcher::findNextInstanceOfLiteral(stringCSIS strLiteral, 
	long nStartSearchPos, long& rnTokenStartPos, long& rnTokenEndPos)
{
	// confirm that the string is not empty
	if(!strLiteral.empty())
	{
		// determine if the literal must have word boundary at the start or end
		bool bMustStartAtWordBoundary = isWordBoundaryConstraintChar(strLiteral[0]);
		bool bMustEndAtWordBoundary = isWordBoundaryConstraintChar(strLiteral[strLiteral.length() - 1]);
		
		// remove the word boundary constraint characters, if any
		if (bMustStartAtWordBoundary)
		{
			strLiteral.erase(0, 1);
		}

		if (bMustEndAtWordBoundary)
		{
			strLiteral.erase(strLiteral.length() - 1, 1);
		}

		// find the first instance of the literal that we're looking for
		long iPos = string::npos;
		do
		{
			// get the literal search data associated with this literal
			SPMLiteralSearchData& rSD = m_mapLiteralToSearchData[strLiteral];

			// find the position of the literal in the text starting
			// at nStartSearchPos
			long iPos = rSD.find(m_strText, strLiteral, nStartSearchPos);

			// if we did not find a match, return false
			if (iPos == string::npos)
			{
				return false;
			}
			else
			{
				// the literal we are looking for has been found.  Update
				// the starting position for subsequent literal searches
				nStartSearchPos += strLiteral.length();
			}

			// determine if the word-boudary constraints (if any) have
			// been met for the literal string that we searched for
			// NOTE: the following two OR expressions count on the fact that
			//		 evaluation of the expression is left to right and that
			//		 the evaluation stops as soon as the expression is true.
			//		 (i.e. we are counting on the fact that if the "righter" part 
			//		 of the expression is being evaluated, the "lefter" parts
			//		 have evaluated to false.
			bool bWordStartConditionMet = !bMustStartAtWordBoundary || 
				iPos == 0 || isWordBoundaryChar(m_strText[iPos - 1]);

			bool bWordEndConditionMet = !bMustEndAtWordBoundary || 
				(iPos + strLiteral.length()) >= m_strText.length() || 
				isWordBoundaryChar(m_strText[iPos + strLiteral.length()]);

			// if we found the literal string we're looking for along and
			// all specified word bourndary constraints (if any) were met,
			// then return the results
			if (bWordStartConditionMet && bWordEndConditionMet)
			{
				// return the token position
				rnTokenStartPos = iPos;
				rnTokenEndPos = rnTokenStartPos + strLiteral.length() - 1;
				return true;
			}
		} while (true);
	}
	else
	{
		// the string was empty
		return false;
	}

	// NOTE: we should never reach here.
	THROW_LOGIC_ERROR_EXCEPTION("ELI09271");
}
//-------------------------------------------------------------------------------------------------
bool CStringPatternMatcher::findNextMatchForLiteralOrList(
	const stringCSIS& strLiteralOrList, long nStartSearchPos, long& rnTokenStartPos, 
	long& rnTokenEndPos)
{
	// tokenize the literal orlist into the individual literal tokens
	vector<string> vecTokens;
	StringTokenizer st('|');
	st.parse(strLiteralOrList, vecTokens);

	// if only one literal is in the literal-or-list, then just call
	// findNextMatchForLiteralOrList
	long nNumTokens = vecTokens.size();
	if (nNumTokens < 2)
	{
		return findNextInstanceOfLiteralIgnoreMWS(strLiteralOrList, 
			nStartSearchPos, rnTokenStartPos, rnTokenEndPos);
	}

	// At this point, we are guaranteed that there is more than one literal
	// in the literal or-list
	// find the earliest match in the input string starting at riCurPos that
	// matches one of the literals in the literal or-list
	long nBestMatchStartPos = -1, nBestMatchEndPos;
	for (int i = 0; i < nNumTokens; i++)
	{
		long nMatchStartPos, nMatchEndPos;

		// try to find a match for the current token
		if (findNextInstanceOfLiteralIgnoreMWS(
			stringCSIS(vecTokens[i], m_bCaseSensitive), nStartSearchPos, 
			nMatchStartPos, nMatchEndPos))
		{
			// if we found an earlier match or our first match - update
			// the best-match related variables
			if (nBestMatchStartPos == -1 || 
				nMatchStartPos < nBestMatchStartPos)
			{
				nBestMatchStartPos = nMatchStartPos;
				nBestMatchEndPos = nMatchEndPos;
			}
		}
	}

	// if we found a match, return the position of that match
	if (nBestMatchStartPos != -1)
	{
		rnTokenStartPos = nBestMatchStartPos;
		rnTokenEndPos = nBestMatchEndPos;
		return true;
	}

	// if we reached here, that's because no match was found
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CStringPatternMatcher::processExpressionToken(long nToken,
	long nStartSearchPos, long& rnTokenStartPos, long& rnTokenEndPos)
{
	// get the string associated with the expression name
	const stringCSIS& strExprName = m_vecTokenInfo[nToken].m_strExprOrVariableName;
	const stringCSIS& strExpressionValue = m_mapExpressions.find(strExprName)->second;

	// the expression value is expected to be treated the same as
	// a literal-or expression
	return findNextMatchForLiteralOrList(strExpressionValue, nStartSearchPos,
		rnTokenStartPos, rnTokenEndPos);
}
//-------------------------------------------------------------------------------------------------
bool CStringPatternMatcher::processLiteralOrList(const stringCSIS& strPattern,
	long nStartSearchPos, long& rnTokenStartPos, long& rnTokenEndPos)
{
	// just find the next match for the strPattern literal-or-list, and if 
	// the search is succesful, return the position of the found match, as well
	// as update m_nCurPos to be position at which subsequent searches should
	// commence
	return findNextMatchForLiteralOrList(strPattern, nStartSearchPos,
		rnTokenStartPos, rnTokenEndPos);
}
//-------------------------------------------------------------------------------------------------
bool CStringPatternMatcher::processCharMustNotMatchToken(long nToken,
	long nStartSearchPos, long& rnTokenStartPos, long& rnTokenEndPos) const
{
	// get the string associated with the expression name
	const stringCSIS& strExprName = m_vecTokenInfo[nToken].m_strExprOrVariableName;
	const stringCSIS& strCharList = m_mapExpressions.find(strExprName)->second;
	
	// find the position of the first character beginning at the current
	// position, that is not one of the characters in the token expression
	long iPos = m_strText.find_first_not_of(strCharList, nStartSearchPos);
	
	if (iPos != string::npos)
	{
		// update the reference variables
		rnTokenStartPos = iPos;
		rnTokenEndPos = iPos;

		// return the position where this token's representation in
		// the string started
		return true;
	}

	// we could not find a character in the string which did not match
	// chars in the token expression
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CStringPatternMatcher::processCharMustMatchToken(long nToken,
	long nStartSearchPos, long& rnTokenStartPos, long& rnTokenEndPos) const
{
	// get the string associated with the expression name
	const stringCSIS& strExprName = m_vecTokenInfo[nToken].m_strExprOrVariableName;
	const stringCSIS& strCharList = m_mapExpressions.find(strExprName)->second;
	
	// find the position of the first character beginning at the current
	// position, that is one of the characters in the token expression
	long iPos = m_strText.find_first_of(strCharList, nStartSearchPos);

	if (iPos != string::npos)
	{
		// update the reference variables
		rnTokenStartPos = iPos;
		rnTokenEndPos = iPos;

		// return the position where this token's representation in
		// the string started
		return true;
	}

	// we could not find a character in the string which matched
	// chars in the token expression
	return false;
}
//-------------------------------------------------------------------------------------------------
ETokenType CStringPatternMatcher::analyzeToken(
	stringCSIS& rstrToken, stringCSIS& rstrExprName, 
	unsigned long& rnMaxIgnoreChars)
{
	// by default, any number of characters can be ignored
	unsigned long nDefaultIgnoreChars = 2000;
	rnMaxIgnoreChars = nDefaultIgnoreChars;

	// if the token contains a ~ (tilde), then everything to the left
	// of the tilde is expected to be a number, indicating the maximum
	// number of characters ignored since the last matched token (if any)
	// make sure that there is only one tilde (at most) in the token,
	// and that the contents to the left of the tilde is an integer.
	size_t pos = rstrToken.find('~');
	if (pos != string::npos)
	{
		// get the string to the left of the tilde and ensure that it is
		// a valid unsigned integer
		string strNumber(rstrToken, 0, pos);
		
		try
		{
			// convert the string into a number
			rnMaxIgnoreChars = asUnsignedLong(strNumber);

			// given that the number is valid, remove the tilde and anything
			// to the left of it from the token for further processing
			rstrToken.erase(0, pos + 1);
		}
		catch (UCLIDException& ue)
		{
			UCLIDException uexOuter("ELI06418", "Invalid value specified for the maximum number of characters to be ignored!", ue);
			uexOuter.addDebugInfo("strNumber", strNumber);
			throw uexOuter;
		}
	}

	// Case sensitivity of the ExprName needs to be the same as the Token
	rstrExprName.setCaseSensitive( rstrToken.isCaseSensitive() );

	// a token is a special token if it is it begins with a ? or @
	// character.  Any token beginning with the @character is
	// also associated with an expression, in which case the expression name
	// is retrieved and stored in rstrExpressionName.  If the special token
	// starts with a ? character, then the name of the match variable is
	// returned 
	if (rstrToken.find("?") == 0)
	{
		rstrExprName.assign(rstrToken, 1, string::npos);
		return kDesiredMatch;
	}
	else if (rstrToken.find("@+") == 0)
	{
		rstrExprName.assign(rstrToken, 2, string::npos);
		return kCharMustMatch;
	}
	else if (rstrToken.find("@-") == 0)
	{
		rstrExprName.assign(rstrToken, 2, string::npos);
		return kCharMustNotMatch;
	}
	else if (rstrToken.find("@") == 0)
	{
		rstrExprName.assign(rstrToken, 1, string::npos);
		return kExpression;
	}

	// by default any token is assumed to be a string literal (or an or-list of
	// string literals)
	return kLiteralOrList;
}
//-------------------------------------------------------------------------------------------------
void CStringPatternMatcher::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI06648", "String Pattern Matcher" );
}
//-------------------------------------------------------------------------------------------------
