// SpatialStringStringOperations.cpp : Implementation of ISpatialString string operations
#include "stdafx.h"
#include "SpatialString.h"
#include "UCLIDRasterAndOCRMgmt.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>

#include <set>

//-------------------------------------------------------------------------------------------------
// ISpatialString - String operations
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::Insert(long nPos, ISpatialString *pString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipString(pString);
		ASSERT_ARGUMENT("ELI25883", ipString != __nullptr);

		insert(nPos, ipString);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06432")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::Append(ISpatialString *pString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();
		
		// verify valid argument sent in
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipStringToAppend(pString);
		ASSERT_RESOURCE_ALLOCATION("ELI06456", ipStringToAppend != __nullptr);

		// Append is an insert at the end of the string.
		append(ipStringToAppend);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06431")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetSubString(long nStart, long nEnd, 
										  ISpatialString **pSubString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI25884", pSubString != __nullptr);

		// Check license
		validateLicense();

		// Get the sub-string
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipString = getSubString(nStart, nEnd);
		ASSERT_RESOURCE_ALLOCATION("ELI25885", ipString != __nullptr);

		// Set the return value
		*pSubString = (ISpatialString*) ipString.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06430")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::Remove(long nStart, long nEnd)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Call the internal remove method
		remove(nStart, nEnd);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06429")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::Replace(BSTR strToFind, BSTR strReplacement, 
	VARIANT_BOOL vbCaseSensitive, long lOccurrence, IRegularExprParser *pRegExpr)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// make sure the strToFind is not the same as strReplacement
		string stdstrToFind = asString(strToFind);
		ASSERT_ARGUMENT("ELI06869", !stdstrToFind.empty());
		
		string stdstrReplacement = asString(strReplacement);
		if (stdstrToFind == stdstrReplacement)
		{
			// do no replacement
			return S_OK;
		}

		// Perform the replacement
		performReplace(stdstrToFind, stdstrReplacement, vbCaseSensitive, lOccurrence, pRegExpr);

		// ensure that this object is spatial
		// perform consistency check to ensure that the size of the
		// string and letters vector are equal (if the letters vector exists)
		performConsistencyCheck();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06517")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::ConsolidateChars(BSTR strChars, VARIANT_BOOL bCaseSensitive)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// the current starting position
		size_t nCurrentPos = 0;
		string stdstrChars = asString(strChars);
		if (bCaseSensitive == VARIANT_FALSE)
		{
			::makeUpperCase(stdstrChars);
		}

		while (true)
		{
			// make a copy of this string for operation purpose
			string strTempCurrentString(m_strString);
			if (bCaseSensitive == VARIANT_FALSE)
			{
				::makeUpperCase(strTempCurrentString);
			}

			// search for the first (or next) instance of any of the chars in strChars
			size_t nFoundPos = strTempCurrentString.find_first_of(stdstrChars, nCurrentPos);
			if (nFoundPos == string::npos)
			{
				break;
			}

			// search for consecutive characters following the above found
			// character
			size_t nStartDeletePos = nFoundPos + 1;
			size_t nEndDeletePos = nFoundPos + 1;
			bool bDelete = false;
			while (nEndDeletePos < strTempCurrentString.length() &&
				   stdstrChars.find(strTempCurrentString[nEndDeletePos]) != string::npos)
			{
				nEndDeletePos++;
				bDelete = true;
			}

			// if consecutive characters are found, then perform the
			// appropriate delete operation
			if (bDelete)
			{
				nEndDeletePos--;
				remove(nStartDeletePos, nEndDeletePos);
			}

			// by default, continue searching after the above found character
			nCurrentPos = nFoundPos + 1;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06519")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::ConsolidateString(BSTR strConsolidateString,
											   VARIANT_BOOL bCaseSensitive)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// the current starting position
		int nCurrentPos = 0;
		string stdstrConsolidateString = asString(strConsolidateString);
		// get the length of the string to be consolidated
		long nLen = stdstrConsolidateString.size();
		if (bCaseSensitive == VARIANT_FALSE)
		{
			::makeUpperCase(stdstrConsolidateString);
		}

		while (true)
		{
			// make a copy of this string for operation purpose
			string strTempCurrentString(m_strString);
			if (bCaseSensitive == VARIANT_FALSE)
			{
				::makeUpperCase(strTempCurrentString);
			}

			// search for the first (or next) instance of any of the chars in strChars
			int nFoundPos = strTempCurrentString.find(stdstrConsolidateString, nCurrentPos);
			if (nFoundPos == string::npos)
			{
				break;
			}

			// search for consecutive characters following the above found
			// character
			long nStartDeletePos = nFoundPos + nLen;
			long nEndDeletePos = nFoundPos + nLen;
			
			// if found the characters at nEndDeletePos, it means
			// there's repeating pattern of the characters 
			// right after the first occurrence
			while (strTempCurrentString.find(stdstrConsolidateString, nEndDeletePos) == nEndDeletePos)
			{
				nEndDeletePos = nEndDeletePos + nLen;				
			}

			if (nEndDeletePos == nStartDeletePos)
			{
				// if no repeating pattern found right after the first occurrence,
				// then go on to the next position
				nCurrentPos = nFoundPos + nLen;
				continue;
			}

			// if consecutive characters are found, then perform the
			// appropriate delete operation
			nEndDeletePos--;
			remove(nStartDeletePos, nEndDeletePos);

			// by default, continue searching after the above found character
			nCurrentPos = nFoundPos + nLen;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06906")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::Trim(BSTR strTrimLeadingChars, BSTR strTrimTrailingChars)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();
		string stdstrLeadingChars = asString(strTrimLeadingChars);
		string stdstrTrailingChars = asString(strTrimTrailingChars);

		// the remainder of this code has been copied and adopted from 
		// trim() in .../RC/BaseUtils/Code/CppUtil.cpp
		
		// if this string is empty, then just return
		if (m_strString.length() == 0)
		{
			return S_OK;
		}

		// calculate the start and end positions of post-trim string
		int b = stdstrLeadingChars.empty() ? 0 : 
				m_strString.find_first_not_of(stdstrLeadingChars);
		int e = stdstrTrailingChars.empty() ? m_strString.length() - 1: 
				m_strString.find_last_not_of(stdstrTrailingChars);

		if (b == string::npos || e == string::npos)
		{
			// Reset everything but the source doc name
			reset(false, true);
			m_strString = "";
		}
		else
		{
			// do trimming of trailing chars if appropriate
			if (e != m_strString.length() -1)
			{
				remove(e+1, -1);
			}

			// do the trimming of the leading chars if appropriate
			if (b != 0)
			{
				remove(0, b-1);
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06520")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::FindFirstInstanceOfChar(long nChar, long nStartPos, 
													 long *pMatchPos)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// ensure valid arguments
		ASSERT_ARGUMENT("ELI06522", pMatchPos != __nullptr);
		verifyValidIndex(nStartPos);

		// return position
		unsigned char nCharToFind = (unsigned char) nChar;
		*pMatchPos = m_strString.find_first_of(nCharToFind, nStartPos);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06521")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::Tokenize(BSTR strDelimiter, IIUnknownVector **pvecItems)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Create vector for resulting ISpatialStrings
		IIUnknownVectorPtr ipTokens(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI06597", ipTokens != __nullptr);

		// find delimiter
		string stdstrDelimiter = asString(strDelimiter);
		int nDelimLen = stdstrDelimiter.length();
		int nDelimPos = m_strString.find(stdstrDelimiter);
		
		// the start position for each token
		long nStartPos = 0;
		while (nDelimPos != string::npos)
		{
			// Get the token before the delimiter
			// In the case where there are 2 delimiters in a row, do not make an item
			// just skip past the second delimiter and continue PVCS P13:4222
			if( nDelimPos -1 >= nStartPos )
			{
				UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipItem =
					getSubString(nStartPos, nDelimPos - 1);
				ASSERT_RESOURCE_ALLOCATION("ELI15419", ipItem != __nullptr);

				// Push the sub string from between the two delimeters onto the list of tokens
				ipTokens->PushBack(ipItem);
			}
		
			// move the start position to the position 
			// right after the current found delimiter
			nStartPos = nDelimPos + nDelimLen;
			// find next delimiter
			nDelimPos = m_strString.find(stdstrDelimiter, nStartPos);
			
			// if no more delimiter is found and there's still 
			// some more text after the last found delimiter, 
			// add the string to the vector
			if (nDelimPos == string::npos && (unsigned long) nStartPos <= m_strString.length() - 1)
			{
				// get the rest of the text after last found delimiter
				UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLastItem = 
					getSubString(nStartPos, m_strString.size() - 1);
				ASSERT_RESOURCE_ALLOCATION("ELI16918", ipLastItem != __nullptr);
				ipTokens->PushBack(ipLastItem);
				break;
			}
			else if (nDelimPos == string::npos)
			{
				// create an empty spatial string
				UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLastItem(CLSID_SpatialString);
				ASSERT_RESOURCE_ALLOCATION("ELI06647", ipLastItem != __nullptr);
				ipTokens->PushBack(ipLastItem);
				break;
			}
		}

		// Return the vector of tokenized strings
		*pvecItems = ipTokens.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06596")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::ToLowerCase()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// perform consistency check to ensure that the size of the
		// string and letters vector are equal (if the letters vector exists)
		performConsistencyCheck();

		// operate on the string member
		makeLowerCase(m_strString);

		// if this is a spatial string, then operate on the letters too
		if (m_eMode == kSpatialMode)
		{
			long nSize = m_vecLetters.size();
			for (int n = 0; n < nSize; n++)
			{
				m_vecLetters[n].m_usGuess1 = getLowerCaseChar((char) m_vecLetters[n].m_usGuess1);
			}
		}

		// set the dirty flag to true since a modification was made
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06612");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::ToUpperCase()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// perform consistency check to ensure that the size of the
		// string and letters vector are equal (if the letters vector exists)
		performConsistencyCheck();

		// operate on the string member
		makeUpperCase(m_strString);

		// if this is a spatial string, then operate on the letters too
		if (m_eMode == kSpatialMode)
		{
			long nSize = m_vecLetters.size();
			for (int n = 0; n < nSize; n++)
			{
				m_vecLetters[n].m_usGuess1 = getUpperCaseChar((char) m_vecLetters[n].m_usGuess1);
			}
		}

		// set the dirty flag to true since a modification was made
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06613");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::ToTitleCase()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// perform consistency check to ensure that the size of the
		// string and letters vector are equal (if the letters vector exists)
		performConsistencyCheck();

		// make first character of each word into upper case
		// walk through each character and check if there is 
		// no character or a non-alphanumeric character to its left
		for (unsigned int n = 0; n < m_strString.size(); n++)
		{
			// if current character is a alpha character
			if (::isalpha((unsigned char) m_strString[n]))
			{
				// always make the first character (must be an alpha char)
				// of each word into upper case
				if (n == 0 || !::isalnum((unsigned char) m_strString[n - 1]))
				{
					// convert char in string to uppercase
					m_strString[n] = getUpperCaseChar(m_strString[n]);
					
					// convert letter to uppercase if appropriate
					if (m_eMode == kSpatialMode)
					{
						m_vecLetters[n].m_usGuess1 = getUpperCaseChar((char) m_vecLetters[n].m_usGuess1);
					}
				}
				else
				{
					// convert char in string to lowercase
					m_strString[n] = getLowerCaseChar(m_strString[n]);

					// convert letter to lowercase if appropriate
					if (m_eMode == kSpatialMode)
					{
						m_vecLetters[n].m_usGuess1 = getLowerCaseChar((char) m_vecLetters[n].m_usGuess1);
					}
				}
			}
		}

		// set the dirty flag to true since a modification was made
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06614");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::AppendString(BSTR strTextToAppend)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		appendString(asString(strTextToAppend));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06738")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::InsertString(long nPos, BSTR bstrText)
{
	try
	{
		validateLicense();

		insertString(nPos, asString(bstrText));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26594");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::FindFirstInstanceOfString(BSTR strSearchString, long nStartPos, 
													   long *pMatchPos)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Ensure valid arguments
		ASSERT_ARGUMENT( "ELI10451", pMatchPos != __nullptr );
		verifyValidIndex( nStartPos );

		// Return position from case-sensitive search
		*pMatchPos = findFirstInstanceOfStringCS( asString( strSearchString ).c_str(), 
			nStartPos, true );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10450")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::FindFirstInstanceOfStringCIS(BSTR strSearchString, long nStartPos, 
														  long *pMatchPos)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Ensure valid arguments
		ASSERT_ARGUMENT( "ELI19099", pMatchPos != __nullptr );
		verifyValidIndex( nStartPos );

		// Return position from case-insensitive search
		*pMatchPos = findFirstInstanceOfStringCS( asString( strSearchString ).c_str(), 
			nStartPos, false );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19106")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::ContainsStringInVector(IVariantVector* pVecBSTRs, 
		VARIANT_BOOL vbCaseSensitive, VARIANT_BOOL vbAreRegExps, IRegularExprParser *pRegExprParser, 
		VARIANT_BOOL *pvbContainsString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Make sure the parser passed is not NULL.
		IRegularExprParserPtr ipRegExprParser(pRegExprParser);
		ASSERT_ARGUMENT("ELI22548", ipRegExprParser != __nullptr );

		// for storing endpoints of found string
		long lStart, lEnd;

		// Retrieve incoming pattern
		string strIncomingPattern = asString(ipRegExprParser->Pattern);

		// Put the searching code in a try/catch block in order to restore the 
		// pattern even if a problem occurs
		try
		{
			// check if the vector of BSTRs should be treated as regular expressions
			if(vbAreRegExps == VARIANT_TRUE)
			{
				// NOTE: It is more efficient to set the vbPrioritizedVector flag in this case,
				// since we do not need to find the earliest occurring regular expression. 
				// findFirstItemInRegExpVector will return immediately after the first found string.
				findFirstItemInRegExpVector(pVecBSTRs, asCppBool(vbCaseSensitive), true, 
					0, ipRegExprParser, lStart, lEnd);
			}
			else
			{
				// NOTE: It is more efficient to set the vbPrioritizedVector flag in this case,
				// since we do not need to find the earliest occurring regular expression.
				// findFirstItemInVector will return immediately after the first found string.
				findFirstItemInVector(pVecBSTRs, asCppBool(vbCaseSensitive), true, 
					0, lStart, lEnd);
			}
		}
		catch (...)
		{
			// Restore the pattern [FlexIDSCore #3104]
			ipRegExprParser->Pattern = _bstr_t( strIncomingPattern.c_str() );

			// Rethrow the exception
			throw;
		}

		// Restore the pattern [FlexIDSCore #3104]
		ipRegExprParser->Pattern = _bstr_t( strIncomingPattern.c_str() );

		// the vector contained the item if a valid start point and end point was found
		*pvbContainsString = asVariantBool(lStart >= 0 && lEnd >= 0);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16811");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::FindFirstItemInVector(IVariantVector* pList,
												   VARIANT_BOOL bCaseSensitive,
												   VARIANT_BOOL bPrioritizedVector,
												   long lStartSearchPos,
												   long* plStart, 
												   long* plEnd)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI25886", plStart != __nullptr);
		ASSERT_ARGUMENT("ELI25887", plEnd != __nullptr);

		// Check licensing
		validateLicense();

		long lStart(-1), lEnd(-1);
		findFirstItemInVector(pList, asCppBool(bCaseSensitive), asCppBool(bPrioritizedVector),
			lStartSearchPos, lStart, lEnd);

		*plStart = lStart;
		*plEnd = lEnd;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05998")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::FindFirstItemInRegExpVector(IVariantVector* pList,
														 VARIANT_BOOL bCaseSensitive,
														 VARIANT_BOOL bPrioritizedVector,
														 long lStartSearchPos,
														 IRegularExprParser *pRegExprParser,
														 long* plStart, 
														 long* plEnd)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI25888", plStart != __nullptr);
		ASSERT_ARGUMENT("ELI25889", plEnd != __nullptr);

		// Check licensing
		validateLicense();

		long lStart(-1), lEnd(-1);
		findFirstItemInRegExpVector(pList, asCppBool(bCaseSensitive), asCppBool(bPrioritizedVector),
			lStartSearchPos, pRegExprParser, lStart, lEnd);

		*plStart = lStart;
		*plEnd = lEnd;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06358")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::RemoveText(ISpatialString* pTextToRemove, long* pnPos)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipTextToRemove = pTextToRemove;
		ASSERT_ARGUMENT("ELI36412", ipTextToRemove != __nullptr);
		ASSERT_ARGUMENT("ELI36413", pnPos != __nullptr);

		// Check license
		validateLicense();

		// Make sure this string is spatial
		verifySpatialness();

		if (ipTextToRemove->GetMode() != kSpatialMode)
		{
			UCLIDException ue("ELI36414", "RemoveText requires a spatial string.");
			throw ue;
		}

		long nNumLettersToRemove = 0;
		CPPLetter *pLettersToRemove = __nullptr;
		ipTextToRemove->GetOCRImageLetterArray(&nNumLettersToRemove, (void **)&pLettersToRemove);

		long nLetterCount = m_vecLetters.size();
		long nPosition = 0;
		long nFirstPosition = -1;
		long nLastPosition = nLetterCount;
		set<long> setLettersToRemove;
		bool bLastLetterRemoved = false;

		// Loop through each letter to remove
		for (long i = 0; i < nNumLettersToRemove; i++)
		{
			// May need to restart loop of existing chars if the last letter to remove could not be
			// found at a position after the last character was found.
			if (nPosition == nLetterCount)
			{
				nPosition = 0;
			}

			CPPLetter &letterToRemove = pLettersToRemove[i];

			// If the next letter to remove is non-spatial, it should be removed only if the
			// preceding spatial character was removed. (otherwise there is no good way of telling
			// one non-spatial character from another).
			if (!letterToRemove.m_bIsSpatial)
			{
				if (bLastLetterRemoved && letterToRemove == m_vecLetters[nPosition])
				{
					setLettersToRemove.insert(nPosition);
					nLastPosition = nPosition;

					if (nLastPosition == nLetterCount)
					{
						bLastLetterRemoved = false;
					}
				}
				else
				{
					bLastLetterRemoved = false;
				}

				nPosition++;
				continue;
			}

			bLastLetterRemoved = false;
			
			// Continuing from the last point in the string where a character has been identified
			// for removal, continue searching for a character that matches the current
			// letterToRemove (in most cases letters to remove will be found sequentially).
			while (nPosition != nLastPosition)
			{
				// If we've searched to the end of the string, loop back to the start until reaching
				// the last index from which a character was identified for removal.
				if (nPosition == nLetterCount)
				{
					nPosition = 0;
					bLastLetterRemoved = false;
				}

				CPPLetter &currentLetter = m_vecLetters[nPosition];

				// If this character is equivalent to the letter to remove, add it to
				// setLettersToRemove then start looking for the next letter to remove
				if (letterToRemove == currentLetter)
				{
					setLettersToRemove.insert(nPosition);
					
					// Keep track of the index at which the first char to remove was found (the
					// return value).
					if (nFirstPosition == -1)
					{
						nFirstPosition = nPosition;
					}

					nLastPosition = nPosition;
					bLastLetterRemoved = true;
					nPosition++;
					break;
				}
				
				nPosition++;
			}
		}

		// If at least one character was found to remove, remove the characters in
		// setLettersToRemove.
		if (nFirstPosition != -1)
		{
			long nNewLetterCount = nLetterCount - setLettersToRemove.size();
			vector<CPPLetter> vecNewLetters;
			vecNewLetters.reserve(nNewLetterCount);
			for (long k = 0; k < nLetterCount; k++)
			{
				if (setLettersToRemove.find(k) == setLettersToRemove.end())
				{
					vecNewLetters.push_back(m_vecLetters[k]);
				}
			}

			if (nNewLetterCount == 0)
			{
				reset(false, false);
			}
			else
			{
				CPPLetter *pNewLetters = &vecNewLetters[0];
				updateLetters(pNewLetters, nNewLetterCount);
			}

			m_bDirty = true;
		}

		*pnPos = nFirstPosition;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36415")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::InsertBySpatialPosition(ISpatialString *pString,
													 VARIANT_BOOL vbAllowOverlappingInsertion,
													 VARIANT_BOOL *pbStringWasInserted)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipString(pString);
		ASSERT_ARGUMENT("ELI36416", ipString != __nullptr);
		ASSERT_ARGUMENT("ELI36430", pbStringWasInserted != __nullptr);
		*pbStringWasInserted = VARIANT_FALSE;

		if (ipString->GetMode() != kSpatialMode)
		{
			UCLIDException ue("ELI36417", "Cannot insert non-spatial string by spatial position");
			throw ue;
		}

		long nPage = ipString->GetFirstPageNumber();
		if (nPage != ipString->GetLastPageNumber())
		{
			UCLIDException ue("ELI36418", "Cannot insert multi-page string by spatial position");
			throw ue;
		}

		ILongRectanglePtr ipBounds = ipString->GetOCRImageBounds();
		long nLeft;
		long nTop;
		long nRight;
		long nBottom;
		ipBounds->GetBounds(&nLeft, &nTop, &nRight, &nBottom);

		bool bStartOfNewWord = true;
		long nPos;
		long nContingencyPos = -1;
		bool bNewLineContingency = false;
		bool bContingencyInMiddleOfWord = false;
		long nCount = m_vecLetters.size();

		// Loop through each positon in the string starting on the target page.
		for (nPos = getFirstCharPositionOfPage(nPage); nPos < nCount; nPos++)
		{
			CPPLetter& letter = m_vecLetters[nPos];
			if (!letter.m_bIsSpatial)
			{
				continue;
			}

			if ((long)letter.m_usPageNumber == nPage)
			{
				// Compare the middle of the current letter to the area of the string to be added.
				CPoint ptMiddleOfLetter(
					(letter.m_ulLeft + letter.m_ulRight) / 2,
					(letter.m_ulTop + letter.m_ulBottom) / 2);

				// If this letter is in-line or below the string to be added and to the right of the
				// new string, this is the spot the new string should be added.
				if (ptMiddleOfLetter.y > nTop && ptMiddleOfLetter.x >= nRight)
				{
					break;
				}
				// If this letter is a normal height char that is completely below the string to be
				// added, this is the positon to add it.
				else if ((long)letter.m_ulTop > nBottom && letter.m_usGuess1 != '.' &&
					letter.m_usGuess1 != ',' && letter.m_usGuess1 != '_')
				{
					break;
				}
				// If the letter appears that it may be in line at or below the string to add, set
				// up a contingency index which will be used as the insertion point if a more
				// definitive position is not subsequently found.
				else if (ptMiddleOfLetter.y > nTop && !bNewLineContingency)
				{
					if (bStartOfNewWord && ptMiddleOfLetter.y > nBottom)
					{
						// If we've gotten to a position where it appears this char may be the
						// start of a new word on a line below the new string, keep track of this
						// position as the position to insert if a more definitive location is not
						// found.
						nContingencyPos = nPos;
						// This contingency location appears to be on a new line, it won't be
						// updated from this point; either it will or won't be the insertion
						// position.
						bNewLineContingency = true;
					}
					else if (ptMiddleOfLetter.x >= nLeft)
					{
						// The current char appears likely to be in-line, but to the left of the
						// string to be added. The new string should be inserted after this
						// one unless another such char is found further to the right.
						nContingencyPos = nPos + 1;
						// If this letter isn't at the end of a word, this contingency is
						// suggesting insertion into the middle of a word.
						bContingencyInMiddleOfWord = !getIsEndOfWord(nPos);
					}
				}

				bStartOfNewWord = getIsEndOfWord(nPos);
			}
			// We're past the page on which the string is to be inserted. The string should be
			// inserted here unless a contingency position has been found.
			else
			{
				if (nContingencyPos != -1)
				{
					nPos = nContingencyPos;
					nContingencyPos = -1;
				}

				break;
			}
		}

		// If the insertion position is in the middle of an existing word, the inserted string
		// appears to overlap with existing text. Do not insert unless explicitly indicated.
		if (!bStartOfNewWord && bContingencyInMiddleOfWord &&
			!asCppBool(vbAllowOverlappingInsertion))
		{
			return S_OK;
		}

		// Ensure surrounding whitespace allows for a natural final result.
		setSurroundingWhitespace(ipString, nPos);

		// Insert the string at the calculated position.
		if (nPos == nCount)
		{
			getThisAsCOMPtr()->Append(ipString);
		}
		else
		{
			getThisAsCOMPtr()->Insert(nPos, ipString);
		}

		*pbStringWasInserted = VARIANT_TRUE;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36419")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::SetSurroundingWhitespace(ISpatialString *pString, long nPos, 
													  long *pnNewPos)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipString(pString);
		ASSERT_ARGUMENT("ELI36420", ipString != __nullptr);
		ASSERT_ARGUMENT("ELI36421", pnNewPos != __nullptr);

		*pnNewPos = nPos;
		setSurroundingWhitespace(ipString, *pnNewPos);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36422")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::TranslateToNewPageInfo(ILongToObjectMap* pPageInfoMap)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ILongToObjectMapPtr ipPageInfoMap = pPageInfoMap;
		ASSERT_ARGUMENT("ELI36423", ipPageInfoMap != __nullptr);

		// Make sure this string is spatial
		if( m_eMode == kNonSpatialMode )
		{
			UCLIDException ue("ELI36424", 
				"TranslateToNewPageInfo() requires a spatial string with spatial information!");
			throw ue;
		}

		// m_ipPageInfoMap will be read-only. Need to create a new copy to make it writable.
		// Shallow copy because the PageInfo instances themselves are immutable and don't need
		// to be cloned.
		IShallowCopyablePtr ipSourcePageInfoMap(getPageInfoMap());
		ASSERT_RESOURCE_ALLOCATION("ELI36425", ipSourcePageInfoMap != __nullptr);
		
		m_ipPageInfoMap = ipSourcePageInfoMap->ShallowCopy();
		ASSERT_RESOURCE_ALLOCATION("ELI36426", m_ipPageInfoMap != __nullptr);

		if (m_eMode == kHybridMode)
		{
			for each (UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipRasterZone in m_vecRasterZones)
			{
				translateToNewPageInfo(ipRasterZone, ipPageInfoMap);
			}
		}
		else
		{
			translateToNewPageInfo(&m_vecLetters[0], m_vecLetters.size(), ipPageInfoMap);
		}

		m_ipPageInfoMap = ipPageInfoMap;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36427")
}
//-------------------------------------------------------------------------------------------------
