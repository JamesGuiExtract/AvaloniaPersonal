// EntityFinder_Internal.cpp : Implementation of CEntityFinder private methods
#include "stdafx.h"
#include "AFUtils.h"
#include "EntityFinder.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <comutils.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <Misc.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long	glDesignatorLimit = 150;
const string gstrDigits = "0123456789";
const string gstrUpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
const string gstrLowerCase = "abcdefghijklmnopqrstuvwxyz";
const string gstrLetters = gstrUpperCase + gstrLowerCase;
const string gstrUpperWithDigits = gstrUpperCase + gstrDigits;
const string gstrLowerWithDigits = gstrLowerCase + gstrDigits;
const string gstrWhiteSpace = " \r\n\t";

//-------------------------------------------------------------------------------------------------
// private / helper methods
//-------------------------------------------------------------------------------------------------
void CEntityFinder::findEntities(const ISpatialStringPtr& ipText)
{
	try
	{
		////////
		// Setup
		////////
		bool	bPersonSuccess = false;
		bool	bFoundCompany = false;
		long	lSuffixStart = -1;
		long	lSuffixStop = -1;
		bool	bCompanySuffix = false;
		bool	bPersonSuffix = false;
		long	lPTIStop = -1;
		long	lKeywordStop = -1;
		bool	bFoundAlias = false;
		long	lDesignatorLimit = 150;

		// Get text as local string
		string	strLocal = asString(ipText->String);

		// Check original length
		int iLength = strLocal.length();
		if (iLength == 0)
		{
			return;
		}

		// Make copy of original string for LOG
		string	strOriginal = strLocal;

		// Instantiate the Regular Expression parser
		IRegularExprParserPtr ipParser = m_ipMiscUtils->GetNewRegExpParserInstance("EntityFinder");
		ASSERT_RESOURCE_ALLOCATION( "ELI06026", ipParser != NULL );

		///////////////////////////////////////////////////
		// Step 0A: Remove any embedded Address information
		///////////////////////////////////////////////////
		bool bFoundAddress = removeAddressText( ipText, ipParser );

		////////////////////////////////////////////////////
		// Step 0B: Look for XXand or andXX and insert space
		////////////////////////////////////////////////////
		long lSpacePos = makeSpaceForAnd( ipText, ipParser );
		while (lSpacePos != -1)
		{
			// Insert the space
			ipText->InsertString(lSpacePos, " ");

			// Keep checking
			lSpacePos = makeSpaceForAnd( ipText, ipParser );
		}

		// Make copy of current string for later substring extraction
		string	strStart = asString( ipText->String );

		// Update local string
		strLocal = strStart;

		//////////////////////////////////////////////////////
		// Step 1: Trim unwanted text from beginning of string
		//////////////////////////////////////////////////////
		strLocal = trimLeadingNonsense( strLocal, ipParser );

		// Create local ISpatialString
		ISpatialStringPtr	ipSpatial( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION( "ELI06159", ipSpatial != NULL );

		// Final string as collection of portions
		string strFinal;

		// Substring to be checked for designator
		string strRight = strLocal;
		ipSpatial->CreateNonSpatialString(strRight.c_str(), "");

		///////////////////////////////////////////////////////////////////
		// Step 1B: More trimming of unwanted text from beginning of string
		//          Using EntityTrimLeadingPhrases expressions
		///////////////////////////////////////////////////////////////////
		// Retrieve list of EntityTrimLeadingPhrases expressions
		IVariantVectorPtr ipTrimLeading = m_ipKeys->GetKeywordCollection( 
			_bstr_t( "EntityTrimLeadingPhrases" ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI26463", ipTrimLeading != NULL );

		// Check string for Phrases
		long	lTrimStart = -1;
		long	lTrimEnd = -1;
		bool	bFoundPhrase = true;
		while (bFoundPhrase)
		{
			// Clear the flag so that search is only done once unless 
			// trimming is done
			bFoundPhrase = false;

			// Check string for first Phrase, if any
			ipSpatial->FindFirstItemInRegExpVector( ipTrimLeading, VARIANT_FALSE, VARIANT_FALSE, 
				0, ipParser, &lTrimStart, &lTrimEnd );
			if (lTrimStart != -1 && lTrimEnd != -1)
			{
				// A phrase for trimming was found, trim it if at or near the beginning of the text
				if (lTrimStart < 4)
				{
					ipSpatial->Remove(lTrimStart, lTrimEnd);

					// Set flag to keep searching
					bFoundPhrase = true;
				}
			}
		}

		////////////////////////////////////
		// Step 2A: Look for Trust Indicator
		//          and trim as appropriate
		////////////////////////////////////
		bool bFoundTrust = doTrustTrimming( ipSpatial, ipParser );

		//////////////////////////////////////////////////
		// Step 2B: Check text for Municipality Indicators
		//////////////////////////////////////////////////
		bool bIsMunicipality = false;

		// Retrieve list of Municipality Indicator expressions
		IVariantVectorPtr ipMuniInd = m_ipKeys->GetKeywordCollection( 
			_bstr_t( "MunicipalityIndicators" ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI10527", ipMuniInd != NULL );

		// Check string for Muni Indicators
		long	lMuniStart = -1;
		long	lMuniEnd = -1;
		ipSpatial->FindFirstItemInRegExpVector( ipMuniInd, VARIANT_FALSE, VARIANT_FALSE, 
			0, ipParser, &lMuniStart, &lMuniEnd );
		if (lMuniStart != -1 && lMuniEnd != -1 && lMuniStart < 10)
		{
			// Just set the flag here
			bIsMunicipality = true;

			// Save the endpoint
			lKeywordStop = lMuniEnd;
		}

		// Update local strings
		strLocal = asString(ipSpatial->String);
		strRight = strLocal;

		/////////////////////////////////////////
		// Step 3A: Locate last person designator
		/////////////////////////////////////////

		// Check string for Person Designators
		long	lStartPos = -1;
		long	lEndPos = -1;
		bool	bDone = false;
		bool	bFindPerson = true;
		while (!bDone && !bFoundTrust)
		{
			// Search the substring for another Person Designator
			ipSpatial->FindFirstItemInRegExpVector( m_ipKeys->PersonDesignators, 
							VARIANT_FALSE, VARIANT_FALSE, 0, ipParser, &lStartPos, &lEndPos );
			if (lStartPos != -1 && lEndPos != -1 && lStartPos < lDesignatorLimit)
			{
				// Check for Person Designator with preceding dash or succeeding dash
				//   Generally indicates "-Single" or "Single-" and is not a valid designator
				if (((lStartPos > 0) && (strLocal[lStartPos-1] == '-')) || 
					((lEndPos < (long)(strLocal.length()) - 1) && (strLocal[lStartPos+1] == '-')))
				{
					// This is not a valid Designator, just move to Step 2B
					bFindPerson = false;
					bDone = true;
				}

				if (bFindPerson)
				{
					// Trim after the Person Designator and append to existing string
					strFinal = strFinal + strRight.substr( 0, lEndPos + 1 );

					// Update the string to be searched
					strRight = strRight.substr( lEndPos + 1, strRight.length() - lEndPos - 1 );
					ipSpatial->ReplaceAndDowngradeToNonSpatial(strRight.c_str());

					// Save the latest endpoint
					lKeywordStop = strFinal.length() - 1;
				}
				else
				{
					bDone = true;
				}
			}
			else
			{
				bDone = true;
			}
		}

		if (strFinal.length() > 0)
		{
			// Set flag
			bPersonSuccess = true;
		}

		////////////////////////////////////////////////
		// Step 3B: Look for last Person Trim Identifier
		//          after last Designator found
		////////////////////////////////////////////////

		// Reset flags
		lStartPos = -1;
		lEndPos = -1;
		bDone = false;

		// Update string to be searched if a Designator was found
		if (bPersonSuccess)
		{
			// Preset the PTI Stop position for later trimming
			lPTIStop = strFinal.length();

			// Found a designator already, define the substring AFTER the designator
			strRight = strLocal.substr( lPTIStop, strLocal.length() - lPTIStop );

			// Use the substring
			ipSpatial->ReplaceAndDowngradeToNonSpatial(strRight.c_str());
		}

		while (!bDone && !bFoundTrust)
		{
			// Search the substring for another Person Trim Identifier
			ipSpatial->FindFirstItemInRegExpVector( m_ipKeys->PersonTrimIdentifiers, 
							VARIANT_FALSE, VARIANT_FALSE, 0, ipParser, &lStartPos, &lEndPos );
			if (lStartPos != -1 && lEndPos != -1)
			{
				// Update the string to be searched
				strRight = strRight.substr( lEndPos + 1, strRight.length() - lEndPos - 1 );
				ipSpatial->ReplaceAndDowngradeToNonSpatial(strRight.c_str());

				// Update the end position
				lPTIStop += (lEndPos + 1);
				if (lPTIStop >= (long)(strLocal.length()))
				{
					lPTIStop = strLocal.length() - 1;
				}
			}
			else
			{
				bDone = true;
			}
		}

		///////////////
		// Check result
		///////////////

		// Check for PTI w/o designator
		if ((lPTIStop > -1) && !bPersonSuccess)
		{
			// Just set the flag, trimming will occur later
			bPersonSuccess = true;
		}
		// Check for no PTI after Designator
		else if (bPersonSuccess && (lPTIStop == strFinal.length()))
		{
			////////////////////////////////////////////////////
			// Look for non-separator lower-case word to include 
			// Persons after Designators and after PTI items
			////////////////////////////////////////////////////

			// Find first lower-case word
			long lLCTrimPos = findFirstLowerCaseWordToTrim( strLocal, ipParser, lPTIStop, false );

			// Remove appropriate text
			if (lLCTrimPos > -1)
			{
				strLocal = strLocal.substr( 0, lLCTrimPos );
			}

			// Reset Trim Indicator position because further trimming is not needed
			lPTIStop = -1;
		}

		//////////////////////////////////
		// Step 3C: Look for person suffix
		//////////////////////////////////

		// Update ISpatialString
		if (!bFoundTrust && !bIsMunicipality)
		{
			ipSpatial->ReplaceAndDowngradeToNonSpatial(strLocal.c_str());

			// Search the substring for a Person Suffix.
			lStartPos = -1;
			lEndPos = -1;

			if (asCppBool(ipSpatial->ContainsStringInVector(
				m_ipKeys->PersonSuffixes, VARIANT_FALSE, VARIANT_TRUE, ipParser)))
			{
				// Just set the flag
				bPersonSuffix = true;
			}
		}

		///////////////////////////////////////////////
		// Step 4: Locate first Company Suffix or 
		//         last Suffix if they immediately
		//         succeed each other.
		//         Suffix will be ignored if before
		//         a previously found Person Designator
		///////////////////////////////////////////////
		long	lCompanyEnd = -1;
		if (!bFoundTrust)
		{
			bFoundCompany = findCompanyEnd( ipSpatial, ipParser, 0, &lSuffixStart, &lSuffixStop, 
				&lCompanyEnd, &bCompanySuffix, &bFoundAlias );

			// Just trim after the Company endpoint and update SpatialString
			if ((bFoundCompany && !bIsMunicipality) || 
				(bFoundCompany && bIsMunicipality && (lCompanyEnd < lMuniEnd)))
			{
				strLocal = strLocal.substr( 0, lCompanyEnd + 1 );
				
				// Trim leading and trailing whitespace
				strLocal = trim( strLocal, " \r\n", " \r\n" );

				// Update endpoint of keyword to end of last suffix
				if (bCompanySuffix)
				{
					lKeywordStop = lSuffixStop;
				}
				// Check for blank line near end of text and trim here
				else
				{
					long lPos = strLocal.find_last_of( "\r\n\r\n" );
					if ((lPos != string::npos) && (strLocal.length() - lPos < 4))
					{
						strLocal = strLocal.substr( 0, lPos );
						strLocal = trim( strLocal, "", " \r\n" );
					}
				}

				ipSpatial->ReplaceAndDowngradeToNonSpatial(strLocal.c_str());
			}
			else
			{
				// Reset flag
				bFoundCompany = false;
			}
		}
		else if (bFoundTrust)
		{
			// Trust Indicator found, update local string
			strLocal = asString(ipSpatial->String);

			// Update keyword position to prevent unwanted trimming
			lKeywordStop = strLocal.length() - 1;
		}

		////////////////////////////////////////////
		// Step 5: Intelligently trim at blank lines
		////////////////////////////////////////////
		if ((lKeywordStop == -1) || (lPTIStop == -1))
		{
			strLocal = doBlankLineTrimming( strLocal, ipParser, lKeywordStop, bFoundTrust );
		}

		//////////////////////////////////////////////////////
		// Step 6: Locate first lower-case word or punctuation
		//         Exceptions: "and", "&"
		//////////////////////////////////////////////////////

		// Enter here only if neither person designator nor company item found
		// Also enter here if just a Person Trim Identifier was found
		if ((!bPersonSuccess && !bFoundCompany && !bFoundTrust) || 
			(bPersonSuccess && lPTIStop > -1))
		{
			// Determine position from which to start searching for LC or punctuation
			int iStart = 0;
			if (bIsMunicipality && (lKeywordStop > -1) && 
				((long)(strLocal.length()) > lKeywordStop))
			{
				// Start after Municipality Indicator
				iStart = lKeywordStop + 1;
			}
			else if ((lPTIStop > -1) && 
				((long)(strLocal.length()) > lPTIStop))
			{
				iStart = lPTIStop + 1;
			}

			// Find first lower-case word
			int iTrimPos = findFirstLowerCaseWordToTrim( strLocal, ipParser, iStart, bIsMunicipality );

			// Find first punctuation character
			string	strPunct( ":^\"" );
			int iFirstPunct = strLocal.find_first_of( strPunct, iStart );
			if ((iFirstPunct > -1) && (iFirstPunct < iTrimPos))
			{
				// Punctuation comes first, trim at that point
				iTrimPos = iFirstPunct;
			}

			// Remove appropriate text
			if (iTrimPos > -1)
			{
				strLocal = strLocal.substr( 0, iTrimPos );
			}
		}

		//////////////////////////////////////////
		// Step 7A: Trim leading and trailing junk
		//////////////////////////////////////////

		// Do not trim parentheses as they will be handled later
		strLocal = trim( strLocal, " :,.\"", " :,[]\"" );

		/////////////////////////////////////////////
		// Step 7B: Remove Entity trim phrases
		// NOTE: Entity trimming not done for persons
		/////////////////////////////////////////////

		strLocal = doGeneralTrimming( strLocal, bPersonSuccess, ipParser );

		/////////////////////////////////////
		// Step 7C: Company-specific trimming
		/////////////////////////////////////

		if (bFoundCompany && !bPersonSuccess)
		{
			strLocal = doCompanyPostProcessing( strLocal, lSuffixStart, 
				lSuffixStop, bFoundAlias );

			// Check for comma in string
			if (!bCompanySuffix && !bFoundAlias && !bIsMunicipality)
			{
				int iCommaPos = strLocal.find( ',', 0 );
				if (iCommaPos != string::npos)
				{
					// Trim the comma and succeeding text
					strLocal = strLocal.substr( 0, iCommaPos );
				}
			}
		}

		/////////////////////////////////////////////////
		// Step 8: Find pending result string in original
		//         string and extract the appropriate
		//         ISpatialString
		/////////////////////////////////////////////////

		ISpatialStringPtr	ipResult( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION( "ELI06673", ipResult != NULL );

		iLength = strLocal.length();
		if (iLength > 0)
		{
			long lTrimPos = strStart.find( strLocal, 0 );
			if (lTrimPos != string::npos)
			{
				// Trim the carriage returns and succeeding text
				ipResult = ipText->GetSubString( lTrimPos, lTrimPos + iLength - 1 );
				ASSERT_RESOURCE_ALLOCATION("ELI25949", ipResult != NULL);
			}
			else
			{
				// Throw exception
			}
		}

		//////////////////////////////////////////////////////////////
		// Step 9A: Remove words surrounded by parentheses or brackets
		//          except if words contain Alias or Person Designator
		//          Remove any leftover parentheses and brackets
		//////////////////////////////////////////////////////////////
		handleParentheses( ipResult, ipParser );

		///////////////////////////////////////////
		// Step 9B: Trim leading and trailing stuff
		///////////////////////////////////////////

		// Replace explicit goofy characters with spaces
		ipResult->Replace("ý", " ", VARIANT_FALSE, 0, ipParser);

		// Leading and trailing spaces
		ipResult->Trim( _bstr_t( " " ), _bstr_t( " " ) );

		// Consolidate whitespace, periods, commas
		ipResult->ConsolidateChars( _bstr_t( " " ), VARIANT_FALSE );
		ipResult->ConsolidateChars( _bstr_t( "." ), VARIANT_FALSE );
		ipResult->ConsolidateChars( _bstr_t( "," ), VARIANT_FALSE );

		// Leading and trailing punctuation and whitespace
		if ((bFoundCompany && bCompanySuffix) || bPersonSuffix)
		{
			ipResult->Trim( _bstr_t( " ,.()\"-_;" ), _bstr_t( " ,()\"-_;" ) );
		}
		// Trim trailing periods except if suffix is found
		else
		{
			ipResult->Trim( _bstr_t( " ,.()\"-_;" ), _bstr_t( " ,.()\"-_;" ) );
		}

		/////////////////////////////////////////////////////
		// Step 9C: Update ISpatialString with trimmed string
		/////////////////////////////////////////////////////

		// Store the trimmed string
		ICopyableObjectPtr ipInput = ipText;
		ASSERT_RESOURCE_ALLOCATION("ELI25950", ipInput != NULL);
		ipInput->CopyFrom(ipResult);

#ifdef _DEBUG
		// Provide debug output
		string strResult = asString( ipResult->String );
		::convertCppStringToNormalString( strResult );
		TRACE( "EFA Output = \"%s\"\r\n", strResult.c_str() );
#endif

		/////////////////////////////////////
		// Step Last: Log results, if desired
		/////////////////////////////////////

		if (m_bLoggingEnabled)
		{
			strLocal = asString(ipText->String);
			logResults( strOriginal, strLocal );
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26595");
}
//-------------------------------------------------------------------------------------------------
string CEntityFinder::consolidateMultipleCharsIntoOne(const string& strInput, 
													  const string& strChars)
{
	string strRet(strInput);

	// Current starting position
	int nCurrentPos = 0;
	
	// Previously found position
	int nPrevFound = -1;
	int nCharsSize = strChars.size();
	string strToFind(strChars);
	string strTemp(strRet);

	// Make all upper case
	makeUpperCase( strToFind );
	makeUpperCase( strTemp );

	while (true)
	{
		int nFoundPos = strTemp.find( strToFind, nCurrentPos );
		if (nFoundPos == string::npos) 
		{
			break;
		}

		// if found pos is next to previous found pos, 
		// then this is a duplicate, eliminate it
		if (nPrevFound >=0 && nPrevFound == nFoundPos-nCharsSize)
		{
			strRet = strRet.erase(nFoundPos, nCharsSize);
			// refresh the temp string
			strTemp = strRet;
			
			makeUpperCase(strTemp);

			nCurrentPos = nFoundPos;
		}
		else
		{
			// no repeating duplicates, then go on to the next position
			nPrevFound = nFoundPos;
			nCurrentPos = nFoundPos+nCharsSize;
		}
	};

	return strRet;
}
//-------------------------------------------------------------------------------------------------
string CEntityFinder::doBlankLineTrimming(string strInput, IRegularExprParserPtr ipParser,
										  long lKeywordEndPos, bool bFoundTrust)
{
	int		iBlankLineCounter = 0;
	string	strLocal = strInput;

	// Do special word comparisons if no keyword is available
	if (lKeywordEndPos == -1)
	{
		// Trim leading and trailing whitespace and carriage returns
		strLocal = trim( strInput, "\r\n ", "\r\n " );
		unsigned long ulStartLength = strLocal.length();

		int iCRCRPos = strLocal.find( "\r\n\r\n", 0 );
		if (iCRCRPos != string::npos)
		{
			// Parse input text into lines
			StringTokenizer	st( '\n' );
			vector<string>	vecTextLines;
			st.parse( strLocal.c_str(), vecTextLines );

			// Do word comparisons between text on lines before and after blank lines
			bool bCanTrim = true;
			for (unsigned int ui = 1; ui < vecTextLines.size(); ui++)
			{
				// Retrieve this line and reset flag
				string strThis = vecTextLines.at( ui );
				bCanTrim = true;

				// Check for a short line with weird characters 
				// and just remove it from the collection
				long lTestLength = strThis.length();
				if ((lTestLength > 0) && (lTestLength < 5) && 
					!containsAlphaNumericChar( strThis ) && 
					containsNonWhitespaceChars( strThis ))
				{
					vecTextLines.erase( vecTextLines.begin() + ui );
					ui--;
					continue;
				}

				// Check for "empty" line before last line
				if (!containsNonWhitespaceChars(strThis) && (ui < vecTextLines.size() - 1))
				{
					// Increment counter
					iBlankLineCounter++;

					// Get previous and subsequent lines
					string	strFirst = vecTextLines.at( ui - 1 );
					string	strSecond = vecTextLines.at( ui + 1 );
					if (!containsNonWhitespaceChars(strFirst) || 
						!containsNonWhitespaceChars(strSecond))
					{
						// Don't bother testing if one of the lines is "empty"
						break;
					}

					// Find the position of the most recent blank line
					int iTrimPos = -1;
					for (int m = 0; m < iBlankLineCounter; m++)
					{
						iTrimPos = strLocal.find( "\r\n\r\n", max( iTrimPos + 1, lKeywordEndPos ) );
					}

					///////////////////////////////////////////////////////////
					// Check for one line containing no alphanumeric characters
					// This is sufficient information for trimming
					///////////////////////////////////////////////////////////

					// Check for strFirst containing no alphanumeric chars
					if (!containsAlphaNumericChar( strFirst ))
					{
						// Remove the first line and continue 
						// checking for blank lines to trim
						strLocal = strLocal.substr( iTrimPos + 4 );

						// Return result of a recursive test
						return doBlankLineTrimming( strLocal, ipParser, -1, bFoundTrust );
					}
					// Check for strSecond containing no alphanumeric chars
					else if (!containsAlphaNumericChar( strSecond ))
					{
						// Retain text before the blank line
						strLocal = strLocal.substr( 0, iTrimPos );
						return strLocal;
					}

					///////////////////////////////////////////////////////////////////
					// Check for one line containing only numeric and punctuation chars
					// This is sufficient information for trimming
					///////////////////////////////////////////////////////////////////

					// Check for strFirst containing no alphabetic chars
					if (!containsAlphaChar( strFirst ))
					{
						// Remove the first line and continue 
						// checking for blank lines to trim
						strLocal = strLocal.substr( iTrimPos + 4 );

						// Return result of a recursive test
						return doBlankLineTrimming( strLocal, ipParser, -1, bFoundTrust );
					}
					// Check for strSecond containing no alphabetic chars
					else if (!containsAlphaChar( strSecond ))
					{
						// Retain text before the blank line
						strLocal = strLocal.substr( 0, iTrimPos );
						return strLocal;
					}

					///////////////////////////////////////////////////
					// Check for one very short line
					// This is also sufficient information for trimming
					///////////////////////////////////////////////////
					if ((strFirst.length() < 5) && (strSecond.length() >= 5))
					{
						// Remove the first line and continue 
						// checking for blank lines to trim
						strLocal = strLocal.substr( iTrimPos + 4 );

						// Return result of a recursive test
						return doBlankLineTrimming( strLocal, ipParser, -1, bFoundTrust );
					}
					// Check for strSecond too short
					else if ((strFirst.length() >= 5) && (strSecond.length() < 5))
					{
						// Retain text before the blank line
						strLocal = strLocal.substr( 0, iTrimPos );
						return strLocal;
					}

					///////////////////////////////////////////////////
					// Check for one line containing a date string
					// This is also sufficient information for trimming
					///////////////////////////////////////////////////
					if (hasDateText( strFirst, ipParser ))
					{
						// Remove the first line and continue 
						// checking for blank lines to trim
						strLocal = strLocal.substr( iTrimPos + 4 );

						// Return result of a recursive test
						return doBlankLineTrimming( strLocal, ipParser, -1, bFoundTrust );
					}
					// Check for strSecond containing a date string
					else if (hasDateText( strSecond, ipParser ))
					{
						// Retain text before the blank line
						strLocal = strLocal.substr( 0, iTrimPos );
						return strLocal;
					}

					///////////////////////////////////////////////////////
					// Check for one line containing an exact keyword match
					// This is also sufficient information for trimming
					///////////////////////////////////////////////////////
					if (isKeywordPhrase( strFirst, ipParser ))
					{
						// Remove the first line and continue 
						// checking for blank lines to trim
						strLocal = strLocal.substr( iTrimPos + 4 );

						// Return result of a recursive test
						return doBlankLineTrimming( strLocal, ipParser, -1, bFoundTrust );
					}
					// Check for strSecond matching a keyword
					else if (isKeywordPhrase( strSecond, ipParser ))
					{
						// Retain text before the blank line
						strLocal = strLocal.substr( 0, iTrimPos );
						return strLocal;
					}

					///////////////////////////////////////////////////
					// Check for first line ending in a semicolon with 
					//   no semicolon in the second line
					// This is also sufficient information for trimming
					///////////////////////////////////////////////////
					strFirst = trim( strFirst, "", " \r\n" );
					strSecond = trim( strSecond, "", " \r\n" );
					if ((strFirst[strFirst.length()-1] == ';') && 
						(strSecond.find( ';', 0 ) == string::npos))
					{
						// Retain text before the blank line
						strLocal = strLocal.substr( 0, iTrimPos );
						return strLocal;
					}

					//////////////////////////////////////////////////////
					// Check for keyword phrase that crosses the two lines
					// If found, do NOT trim here
					//////////////////////////////////////////////////////
					if (foundKeywordPhraseOverlap( strFirst, strSecond, ipParser ))
					{
						bCanTrim = false;
					}

					/////////////////////////////////////////////////////
					// Check for "and" ending the first line or beginning
					// the second line.  If found, remove these words 
					// before testing case of lines below
					/////////////////////////////////////////////////////
					// Test second line
					bool bFound = trimLeadingWord( strSecond, "and" );

					// Reverse first line, test and reverse the result
					reverseString( strFirst );
					bFound |= trimLeadingWord( strFirst, "dna" );
					reverseString( strFirst );

					/////////////////////////////////////////////////////////
					// Check for one line upper case and the other title case
					// This is also sufficient information for trimming
					/////////////////////////////////////////////////////////
					bool bFirstUpper = false;
					string strUpper( strFirst );
					makeUpperCase( strUpper );
					if (strFirst == strUpper)
					{
						bFirstUpper = true;
					}

					bool bSecondUpper = false;
					strUpper = strSecond;
					makeUpperCase( strUpper );
					if (strSecond == strUpper)
					{
						bSecondUpper = true;
					}

					// Forbid trimming here if an "and" was trimmed and the result 
					// has both lines completely upper case [FlexIDSCore #2959]
					if (bFound && bFirstUpper && bSecondUpper)
					{
						bCanTrim = false;
					}

					// Check for first line lower, second line upper OR
					// first line upper, second line lower
					// IF AND ONLY IF this is not a Trust Entity
					if (bCanTrim && !bFoundTrust && 
						((!bFirstUpper && bSecondUpper) || (bFirstUpper && !bSecondUpper)))
					{
						// Remove the first line and continue 
						// checking for blank lines to trim
						if (!bFirstUpper)
						{
							strLocal = strLocal.substr( iTrimPos + 4 );

							// Return result of a recursive test
							return doBlankLineTrimming( strLocal, ipParser, -1, bFoundTrust );
						}
						// Retain text before the blank line if upper case
						else
						{
							strLocal = strLocal.substr( 0, iTrimPos );
							return strLocal;
						}
					}

					// Check each line for a semicolon (indicating an address removal)
					bool	bTwoSemicolons = false;
					long	lTemp1 = strFirst.find( ';', 0 );
					long	lTemp2 = strSecond.find( ';', 0 );
					if ((lTemp1 != string::npos) && (lTemp2 != string::npos))
					{
						bTwoSemicolons = true;
					}

					// Compare words in first string with words in second string
					if (!bTwoSemicolons && bCanTrim)
					{
						// Divide first line into words
						StringTokenizer	stLine( ' ' );
						vector<string>	vecFirstWords;
						stLine.parse( strFirst.c_str(), vecFirstWords );

						// Divide second line into words
						vector<string>	vecSecondWords;
						stLine.parse( strSecond.c_str(), vecSecondWords );

						for (unsigned int uj = 0; uj < vecFirstWords.size(); uj++)
						{
							// Retrieve this word from first line
							string strFirstWord = vecFirstWords.at( uj );
							strFirstWord = trim( strFirstWord, ",\r\n ", ",\r\n " );

							// Check this word against second line words if not an initial
							if ((strFirstWord.size() > 2) || 
								(strFirstWord.size() == 2 && strFirstWord.at( 1 ) != '.'))
							{
								for (unsigned int uk = 0; uk < vecSecondWords.size(); uk++)
								{
									// Retrieve this word from second line
									string strSecondWord = vecSecondWords.at( uk );
									strSecondWord = trim( strSecondWord, ",\r\n ", ",\r\n " );

									// Check this word against current first line word if not an initial
									if ((strSecondWord.size() > 2) || 
										(strSecondWord.size() == 2 && strSecondWord.at( 1 ) != '.'))
									{
										if (strFirstWord == strSecondWord)
										{
											bCanTrim = false;
											break;
										}
									}
								}
							}

							if (!bCanTrim)
							{
								break;
							}
						}

						// Trim at the most recent blank line, if appropriate
						if (bCanTrim)
						{
							// Find character position of most recent blank line
							int iTrimPos = -1;
							for (int m = 0; m < iBlankLineCounter; m++)
							{
								iTrimPos = strLocal.find( "\r\n\r\n", max( iTrimPos + 1, lKeywordEndPos ) );
							}

							// Trim here
							if (iBlankLineCounter > 0)
							{
								strLocal = strLocal.substr( 0, iTrimPos );
							}
						}
					}			// end if semicolon on each line

					// Stop checking lines if already trimmed
					if (bCanTrim && strLocal.length() < ulStartLength)
					{
						break;
					}
				}		// end if blank line found
			}			// end for each line of text
		}
	}
	else
	{
		/////////////////////////////////////////////////////
		// Check for first double carriage return that occurs 
		// after the keyword and trim it if found
		/////////////////////////////////////////////////////

		// First test
		bool	bTrimmed = false;
		int iCRCRPos = strLocal.find( "\n\n", lKeywordEndPos );
		if (iCRCRPos != string::npos)
		{
			// Trim the carriage returns and succeeding text
			strLocal = strLocal.substr( 0, iCRCRPos );
			bTrimmed = true;
		}

		// Second test
		iCRCRPos = strLocal.find( "\r\n\r\n", lKeywordEndPos );
		if (iCRCRPos != string::npos)
		{
			// Trim the carriage returns and succeeding text
			strLocal = strLocal.substr( 0, iCRCRPos );
			bTrimmed = true;
		}

		// Last chance if no trimming yet
		if (!bTrimmed)
		{
			// Try trimming while ignoring the keyword information
			string strLastChance = doBlankLineTrimming( strLocal, ipParser, -1, bFoundTrust );

			// Locate new string within original string
			long lNewPos = strLocal.find( strLastChance );

			// Last chance trim is accepted if
			//   New String > 3 characters AND
			//   New String < original string AND
			//   New String is not at the begnning of the original string
			unsigned long ulLength = strLastChance.length();
			if ((ulLength > 3) && (ulLength < strLocal.length()) && (lNewPos > 2))
			{
				// Retain the trimmed string
				strLocal = strLastChance;
			}
		}
	}

	return strLocal;
}
//-------------------------------------------------------------------------------------------------
string CEntityFinder::doCompanyPostProcessing(const string& strInput, long lSuffixStart, 
											  long lSuffixStop, bool bAliasFound)
{
	bool	bColonDone = false;
	bool	bPeriodDone = false;
	string	strPunctuation( ",.\"" );

	// Trim leading and trailing stuff
	string	strFinal = trim( strInput, " ,:.\"", "\r\n ,:\"" );

	////////////////////////////////////////////////////
	// Look for trailing space, char, period and trim it
	// only if this is outside the located suffix
	////////////////////////////////////////////////////
	unsigned int	uiStart = strFinal.length();
	unsigned int	uiStop = 0;
	if ((uiStart > 3) && (uiStart > (unsigned long)(lSuffixStop + 1)))
	{
		if ((strFinal[uiStart - 1] == '.') && (strFinal[uiStart - 3] == ' '))
		{
			// Found the period and the space, trim the last three
			strFinal = strFinal.substr( 0, uiStart - 3 );
		}
	}

	// Define start point for R-to-L search
	if (lSuffixStart != -1)
	{
		// Do not look for periods inside suffix
		uiStop = (unsigned long)lSuffixStart;
	}

	//////////////////////////////
	// Entire string is upper case
	//////////////////////////////
	// Just check for and handle embedded colons
	string	strUpper( strFinal );
	makeUpperCase( strUpper );
	if (strFinal == strUpper)
	{
		// Look for Colons inside the text
		unsigned int uiColonPos = strFinal.find_last_of( ':', uiStart );
		if (uiColonPos != string::npos)
		{
			// Colon is okay at end
			if (strFinal.length() > uiColonPos + 1)
			{
				// Look for the start of the next word
				unsigned int uiWordPos = strFinal.find_first_not_of( 
					strPunctuation, uiColonPos + 1 );
				if (uiWordPos != string::npos)
				{
					// Found a word after the colon, evaluate the substrings
					string	strFirst = strFinal.substr( 0, uiWordPos );
					string	strSecond = strFinal.substr( uiWordPos, strFinal.length() - uiWordPos );

					// Only continue if neither substring looks like an abbreviation
					if (!isAbbreviation( strFirst ) && !isAbbreviation( strSecond ))
					{
						// Default to second string
						strFinal = strSecond;
					}
					else
					{
						// Reset starting point
						uiStart = uiColonPos - 1;
					}
				}
				else
				{
					// No subsequent word found, continue searching for periods
					uiStart = uiColonPos - 1;
				}
			}
			else
			{
				// Reset starting point
				uiStart = uiColonPos - 1;
			}
		}

		// Set flags, no further checking is needed
		bColonDone = true;
		bPeriodDone = true;
	}

	/////////////////////////////
	// Search for embedded colons
	/////////////////////////////
	while (!bColonDone)
	{
		// Look for Colons inside the text
		unsigned int uiColonPos = strFinal.find_last_of( ':', uiStart );
		if (uiColonPos != string::npos)
		{
			// Colon is okay at end
			if (strFinal.length() > uiColonPos + 1)
			{
				// Look for the start of the next word
				unsigned int uiWordPos = strFinal.find_first_not_of( 
					strPunctuation, uiColonPos + 1 );
				if (uiWordPos != string::npos)
				{
					// Found a word after the colon, evaluate the substrings
					string	strFirst = strFinal.substr( 0, uiWordPos );
					string	strSecond = strFinal.substr( uiWordPos, strFinal.length() - uiWordPos );

					// Only continue if neither substring looks like an abbreviation
					bool	bFirstUpper = false;
					bool	bSecondUpper = false;
					if (!isAbbreviation( strFirst ) && !isAbbreviation( strSecond ))
					{
						// Check case of first string
						string	strUpper( strFirst );
						makeUpperCase( strUpper );
						if (strFirst == strUpper)
						{
							bFirstUpper = true;
						}

						// Check case of second string
						strUpper = strSecond;
						makeUpperCase( strUpper );
						if (strSecond == strUpper)
						{
							bSecondUpper = true;
						}
					}
					else
					{
						// Reset starting point
						uiStart = uiColonPos - 1;
					}

					// Keep first string if it is UC and second is not
					if (bFirstUpper && !bSecondUpper)
					{
						strFinal = strFirst;
						bColonDone = true;
					}
					// Keep second string if it is UC and first is not
					else if (bSecondUpper && !bFirstUpper)
					{
						strFinal = strSecond;
						bColonDone = true;
					}
					else
					{
						// Default to second string
						strFinal = strSecond;
						bColonDone = true;
					}
				}
				else
				{
					// No subsequent word found, continue searching for periods
					uiStart = uiColonPos - 1;
				}
			}
			else
			{
				// Reset starting point
				uiStart = uiColonPos - 1;
			}
		}
		else
		{
			// No colon to find
			bColonDone = true;
		}
	}

	//////////////////////////////
	// If no colons found, then 
	// search for embedded periods
	// if no Alias is present
	//////////////////////////////
	if (lSuffixStart > -1)
	{
		uiStart = lSuffixStart;
	}

	while (!bPeriodDone && !bAliasFound)
	{
		// Look for periods inside the text
		string strPeriod = ".";
		unsigned int uiPeriodPos = strFinal.find_last_of( strPeriod.c_str(), uiStart );

		if (uiPeriodPos != string::npos)
		{
			// Period is okay at end
			if (strFinal.length() > uiPeriodPos + 1)
			{
				// Look for the start of the next word
				unsigned int uiWordPos = strFinal.find_first_not_of( 
					strPunctuation, uiPeriodPos + 1 );
				if (uiWordPos != string::npos)
				{
					// Found a word after the period, evaluate the substrings
					string	strFirst = strFinal.substr( 0, uiWordPos );
					string	strSecond = strFinal.substr( uiWordPos, strFinal.length() - uiWordPos );

					// Only continue if neither substring looks like an abbreviation
					bool	bFirstUpper = false;
					bool	bSecondUpper = false;
					if (!isAbbreviation( strFirst ) && !isAbbreviation( strSecond ))
					{
						// Check case of first string
						string	strUpper( strFirst );
						makeUpperCase( strUpper );
						if (strFirst == strUpper)
						{
							bFirstUpper = true;
						}

						// Check case of second string
						strUpper = strSecond;
						makeUpperCase( strUpper );
						if (strSecond == strUpper)
						{
							bSecondUpper = true;
						}
					}
					else
					{
						// Reset starting point
						uiStart = uiPeriodPos - 1;
					}

					// Keep first string if it is UC and second is not
					if (bFirstUpper && !bSecondUpper)
					{
						strFinal = strFirst;
						bPeriodDone = true;
					}
					// Keep second string if it is UC and first is not
					else if (bSecondUpper && !bFirstUpper)
					{
						strFinal = strSecond;
						bPeriodDone = true;
					}
					else
					{
						// Reset starting point
						uiStart = uiPeriodPos - 1;
					}
				}
				else
				{
					// No subsequent word found, continue searching for periods
					uiStart = uiPeriodPos - 1;
				}
			}
			else
			{
				// Reset starting point
				uiStart = uiPeriodPos - 1;
			}
		}
		else
		{
			// No period to find
			bPeriodDone = true;
		}
	}

	// Provide updated string to caller
	return strFinal;
}
//-------------------------------------------------------------------------------------------------
string CEntityFinder::doGeneralTrimming(string strInput, bool bPersonFound,
										IRegularExprParserPtr ipParser)
{
	string	strBreakChars( "()[]" );
	long lLength = strInput.length();

	///////////////////////
	// Remove trailing " ."
	///////////////////////
	if ((lLength > 2) && (strInput[lLength-1] == '.') && (strInput[lLength-2] == ' '))
	{
		strInput = strInput.substr( 0, lLength - 2 );
	}

	//////////////////////////////////
	// Check EntityTrimTrailingPhrases
	// unless Person found
	//////////////////////////////////

	if (!bPersonFound)
	{
		// Create local ISpatialString
		ISpatialStringPtr	ipSpatial( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION( "ELI06379", ipSpatial != NULL );
		ipSpatial->CreateNonSpatialString(strInput.c_str(), "");

		long	lStartPos = -1;
		long	lEndPos = -1;

		// Search the string for an Entity Trim Phrase
		ipSpatial->FindFirstItemInRegExpVector( m_ipKeys->EntityTrimTrailingPhrases, 
						VARIANT_FALSE, VARIANT_FALSE, 0, ipParser, &lStartPos, &lEndPos );
		if (lStartPos != -1 && lEndPos != -1)
		{
			// Create a composite collection of Alias items
			IShallowCopyablePtr ipCopier = m_ipKeys->GetCompanyAlias( 
				(UCLID_AFUTILSLib::ECompanyAliasType)kCompanyAliasAll );
			ASSERT_RESOURCE_ALLOCATION("ELI26072", ipCopier != NULL);
			IVariantVectorPtr	ipAliases = ipCopier->ShallowCopy();
			ASSERT_RESOURCE_ALLOCATION( "ELI10561", ipAliases != NULL );
			ipAliases->Append( m_ipKeys->GetRelatedCompany( 
				(UCLID_AFUTILSLib::ERelatedCompanyType)kRelatedCompanyAll ) );

			// Check for subsequent Alias
			long	lAliasStartPos = -1;
			long	lAliasEndPos = -1;

			// Search the string for an Entity Trim Phrase
			if (!asCppBool(ipSpatial->ContainsStringInVector(
				ipAliases, VARIANT_FALSE, VARIANT_TRUE, ipParser)))
			{
				// Trim at the Phrase if not at or near beginning
				if (lStartPos > 8)
				{
					strInput = strInput.substr( 0, lStartPos );
				}
				// Return empty string if Trim Phrase is entire text
				else if ((lStartPos == 0) && (lEndPos == strInput.length() - 1))
				{
					strInput = "";
				}
			}
			// Alias found, assume that Company has already been trimmed
		}
	}

	// Provide updated string to caller
	return strInput;
}
//-------------------------------------------------------------------------------------------------
bool CEntityFinder::findCompanyEnd(ISpatialStringPtr ipText, IRegularExprParserPtr ipParser,
								   long lStartPos, long *plSuffixStart, long *plSuffixEnd,
								   long *plEndPos, bool *pbFoundSuffix, bool *pbFoundAlias)
{
	bool	bSuffix = false;
	bool	bDesignator = false;
	long	lAliasStart = -1;
	long	lTrimPos = -1;
	bool	bInputAlias = *pbFoundAlias;
	bool	bThisAlias = false;
	bool	bInputSuffix = *pbFoundSuffix;
	bool	bThisSuffix = false;

	// Default return information
	*plSuffixStart = -1;
	*plSuffixEnd = -1;
	*plEndPos = -1;

	// Create a composite collection of Company Alias items
	IShallowCopyablePtr ipCopier = m_ipKeys->GetCompanyAlias( 
		(UCLID_AFUTILSLib::ECompanyAliasType)kCompanyAliasAll );
	ASSERT_RESOURCE_ALLOCATION("ELI26073", ipCopier != NULL);
	IVariantVectorPtr	ipAliases = ipCopier->ShallowCopy();
	ASSERT_RESOURCE_ALLOCATION( "ELI10562", ipAliases != NULL );
	ipAliases->Append( m_ipKeys->GetRelatedCompany( 
		(UCLID_AFUTILSLib::ERelatedCompanyType)kRelatedCompanyAll ) );

	ASSERT_ARGUMENT( "ELI08868", ipText != NULL );

	string	strLocal = asString(ipText->String);
	long	lLength = strLocal.length();

	// First look for a Company Suffix
	long	lSuffixStart = -1;
	long	lSuffixStop = -1;
	long	lLocalStart = lStartPos;
	long	lEndPreviousSuffix = -1;
	bool	bDone = false;
	while (!bDone)
	{
		ipText->FindFirstItemInRegExpVector( m_ipKeys->CompanySuffixes, 
			VARIANT_FALSE, VARIANT_FALSE, lLocalStart, ipParser, &lSuffixStart, &lSuffixStop );
		if ((lSuffixStart != -1) && lSuffixStop != -1 &&
			(lSuffixStart - lLocalStart < glDesignatorLimit) && 
			((lEndPreviousSuffix == -1) || (lSuffixStart - lEndPreviousSuffix < 4)))
		{
			// Check for Company Suffix with preceding dash or ending dash
			//   Generally indicates "-CO" or "Co-" and is not a valid suffix
			if (((lSuffixStart > 0) && (strLocal[lSuffixStart-1] == '-')) || 
				(strLocal[lSuffixStop] == '-'))
			{
				// This is not a valid Suffix, just move on
				if (lLength > lSuffixStop + 1)
				{
					lLocalStart = lSuffixStop + 1;
				}
				else
				{
					bDone = true;
				}
			}

			// Check for Company Suffix too early in text
			//   with enough subsequent text
			else if ((lSuffixStart < 4) && (lSuffixStop < lLength - 5))
			{
				// Retain only the text AFTER the Company Suffix
				strLocal = strLocal.substr( lSuffixStop + 1, lLength - lSuffixStop - 1 );

				// If the string has spatial info then replace and make it hybrid
				if (ipText->HasSpatialInfo() == VARIANT_TRUE)
				{
					ipText->ReplaceAndDowngradeToHybrid(strLocal.c_str());
				}
				else
				{
					ipText->ReplaceAndDowngradeToNonSpatial(strLocal.c_str());
				}

				long lSuffStart = -1;
				long lSuffEnd = -1;
				long lFinish = -1;
				bool bFound = findCompanyEnd( ipText, ipParser, lStartPos, &lSuffStart, &lSuffEnd, 
					&lFinish, &bThisSuffix, &bThisAlias );
				if (bFound)
				{
					// Offset positions by size of leading Suffix
					*plSuffixStart = lSuffStart + lSuffixStop + 1;
					*plSuffixEnd = lSuffEnd + lSuffixStop + 1;
					*plEndPos = lFinish + lSuffixStop + 1;
					*pbFoundSuffix = (bInputSuffix || bThisSuffix);
					*pbFoundAlias = (bInputAlias || bThisAlias);
					return true;
				}
				else
				{
					*plEndPos = -1;
					*pbFoundSuffix = bInputSuffix;
					*pbFoundAlias = bInputAlias;
					return false;
				}
			}

			// Otherwise this is a good suffix
			else
			{
				// Save endpoint of THIS Suffix
				lEndPreviousSuffix = lSuffixStop;
				*plSuffixEnd = lSuffixStop;

				// Suffix start point is the beginning of the first Suffix
				if (*plSuffixStart > -1)
				{
					*plSuffixStart = min( lSuffixStart, *plSuffixStart);
				}
				else
				{
					*plSuffixStart = lSuffixStart;
				}

				// Update the new start point
				if (lLength > lEndPreviousSuffix + 1)
				{
					lLocalStart = lEndPreviousSuffix + 1;
				}
				// This suffix finishes the string
				else
				{
					// Set flags
					bSuffix = true;
					bThisSuffix = true;
					bDone = true;
				}
			}
		}
		else if (lEndPreviousSuffix > -1)
		{
			// Set flag and start point for Alias search
			bSuffix = true;
			bThisSuffix = true;
			bDone = true;
			if (lLength > lEndPreviousSuffix + 1)
			{
				lAliasStart = lEndPreviousSuffix + 1;
			}
		}
		else
		{
			bDone = true;
		}
	}

	// May need to look for a Company Designator
	if (!bSuffix)
	{
		// Check string for first item
		long	lDesignatorStart = -1;
		long	lDesignatorStop = -1;
		ipText->FindFirstItemInRegExpVector( m_ipKeys->CompanyDesignators, 
			VARIANT_FALSE, VARIANT_FALSE, lStartPos, ipParser, &lDesignatorStart, &lDesignatorStop );
		if (lDesignatorStart != -1 && lDesignatorStop != -1)
		{
			// Set flag and default endpoint
			bDesignator = true;
			lTrimPos = lDesignatorStop;

			// Check for Company Designator with preceding dash or succeeding dash
			//   Generally indicates "-Fannie Mae" and is not a valid designator
			if (((lDesignatorStart > 0) && (strLocal[lDesignatorStart-1] == '-')) || 
				((lDesignatorStop < lLength - 1) && (strLocal[lDesignatorStart+1] == '-')))
			{
				// This is not a valid Designator, just move on
				bDesignator = false;
			}

			// Do not automatically trim after the first word
			else if (lDesignatorStart == 0)
			{
				// Reset positions to skip the first word and check again
				long lNewStart = lDesignatorStop + 1;
				lDesignatorStart = -1;
				lDesignatorStop = -1;
				ipText->FindFirstItemInRegExpVector(m_ipKeys->CompanyDesignators, 
					VARIANT_FALSE, VARIANT_FALSE, lNewStart, ipParser, &lDesignatorStart, &lDesignatorStop);
				if (lDesignatorStart != -1 && lDesignatorStop != -1)
				{
					// Another Designator found!
					// Find next lower-case word in original
					// Digits count as lower-case
					int iFirstLower = findFirstCaseWord( strLocal, 
						lDesignatorStop + 1, false, true, true );
					if ((iFirstLower > 0) && (lLength > iFirstLower + 1))
					{
						// Set start point for Alias search
						lAliasStart = iFirstLower - 1;
					}
					// else all remaining words are upper case and will be retained
					else
					{
						lTrimPos = lLength - 1;
					}

					// Check for Alias in between Designators
					long	lAliasStartPos = -1;
					long	lAliasEndPos = -1;
					ipText->FindFirstItemInRegExpVector( ipAliases, VARIANT_FALSE, VARIANT_FALSE,
						lNewStart, ipParser, &lAliasStartPos, &lAliasEndPos );
					if (lAliasStartPos != -1 && lAliasEndPos != -1)
					{
						// Check for more text following the Alias
						if (lLength - lAliasEndPos > 3)
						{
							// Make sure that Alias flag is set
							bThisAlias = true;
						}
					}
				}
				else
				{
					// No second Designator found
					// Find next lower-case word in original
					// Digits count as lower-case
					int iFirstLower = findFirstCaseWord( strLocal, lNewStart, false, 
						true, true );
					if ((iFirstLower > 0) && (lLength > iFirstLower + 1))
					{
						// Set start point for Alias search
						lAliasStart = iFirstLower - 1;
					}
					// else all remaining words are upper case and will be retained
					else
					{
						lTrimPos = lLength - 1;
					}
				}
			}		// end else if Designator at beginning of string

			// Designator found but is not the first word
			// Find trim position as starting point for Alias search
			else if (bDesignator)
			{
				// Set start point for Alias search
				lAliasStart = lDesignatorStop + 1;
			}
		}		// end if Designator found
	}			// end if no Suffix was found

	// Do not allow infinite loop of looking for Alias
	bool	bOnceOnly = false;

AliasStart:
	// May need to look for Alias keywords
	if (bSuffix || bDesignator)
	{
		if (lAliasStart == -1)
		{
			// Set end point as end of Suffix or Designator
			*plEndPos = bSuffix ? lSuffixStop : lTrimPos;
			*pbFoundSuffix = (bInputSuffix || bThisSuffix);
			*pbFoundAlias = (bInputAlias || bThisAlias);
		}
		else
		{
			// Search the substring for an Alias following 
			// the current search position
			long	lAliasStartPos = -1;
			long	lAliasEndPos = -1;
			ipText->FindFirstItemInRegExpVector( ipAliases, VARIANT_FALSE, VARIANT_FALSE,
				lAliasStart, ipParser, &lAliasStartPos, &lAliasEndPos );
			if (lAliasStartPos != -1 && lAliasEndPos != -1 &&
				(lAliasStartPos - lAliasStart < 50))
			{
				// Check for more text following the Alias
				if (lLength - lAliasEndPos > 3)
				{
					// Update the start position and find another Company
					long lSuffStart = -1;
					long lSuffEnd = -1;
					long lFinish = -1;
					bThisAlias = true;
					bool bFound = findCompanyEnd( ipText, ipParser, lAliasEndPos + 1, &lSuffStart, 
						&lSuffEnd, &lFinish, &bThisSuffix, &bThisAlias );
					if (bFound)
					{
						// End point is end of next Company
						*plEndPos = lFinish;
						*pbFoundSuffix = (bInputSuffix || bThisSuffix);
						*pbFoundAlias = (bInputAlias || bThisAlias);
					}
					else
					{
						// No subsequent Company found
						// End point is end of Suffix or Designator OR
						// immediately before the lower-case word trim position
						if (lLength <= lAliasEndPos + 3)
						{
							// Set end point as space before Alias
							*plEndPos = lAliasStart - 1;
							*pbFoundSuffix = (bInputSuffix || bThisSuffix);
							*pbFoundAlias = bInputAlias;
						}
						else
						{
							long lNewStartPos = -1;
							int iTrimPos = findFirstLowerCaseWordToTrim( strLocal, ipParser,
								lAliasEndPos + 1, true );
							if ((iTrimPos > 0) && (lLength > iTrimPos + 1))
							{
								// Set end point as space before LC word
								*plEndPos = iTrimPos - 1;
								*pbFoundSuffix = (bInputSuffix || bThisSuffix);
								*pbFoundAlias = (bInputAlias || bThisAlias);
							}
							// else all remaining words are upper case and will be retained
							else
							{
								// Set end point as end of string
								*plEndPos = lLength - 1;
								*pbFoundSuffix = (bInputSuffix || bThisSuffix);
								*pbFoundAlias = (bInputAlias || bThisAlias);
							}
						}
					}
				}
				else
				{
					// No text found after the Alias so exclude it
					*plEndPos = lAliasStart - 1;
					*pbFoundSuffix = (bInputSuffix || bThisSuffix);
					*pbFoundAlias = bInputAlias;
				}
			}

			// Check for Alias found too far away from Suffix or Designator endpoint
			else if ((lAliasStartPos != -1) && (lAliasEndPos != -1) && !bOnceOnly)
			{
				// Find lower-case word following the Designator
				long lNewStartPos = -1;
				int iTrimPos = findFirstLowerCaseWordToTrim( strLocal, ipParser,
					lAliasStart, true );
				if ((iTrimPos > 0) && (lLength > iTrimPos + 1))
				{
					// Set new start point for Alias search
					lAliasStart = iTrimPos - 1;

					// Set once-only flag
					bOnceOnly = true;

					// Go back and look for Alias again
					goto AliasStart;
				}
				// else all remaining words are upper case and will be retained
				else
				{
					// Set end point as end of string
					*plEndPos = lLength - 1;
					*pbFoundSuffix = (bInputSuffix || bThisSuffix);
					*pbFoundAlias = true;
				}
			}

			// No Alias was found
			else
			{
				// No Alias found, end point is end of Suffix
				if (bSuffix)
				{
					*plEndPos = lAliasStart - 1;
					*pbFoundSuffix = (bInputSuffix || bThisSuffix);
					*pbFoundAlias = (bInputAlias || bThisAlias);
				}
				// No Alias found, end point is at first LC word after Designator
				else
				{
					long lNewStartPos = -1;
					int iTrimPos = findFirstLowerCaseWordToTrim( strLocal, ipParser,
						lAliasStart, true );
					if ((iTrimPos > 0) && (lLength >= iTrimPos + 1))
					{
						*plEndPos = iTrimPos - 1;
						*pbFoundSuffix = (bInputSuffix || bThisSuffix);
						*pbFoundAlias = (bInputAlias || bThisAlias);
					}
					// else all remaining words are upper case and will be retained
					else
					{
						// Set end point as end of string
						*plEndPos = lLength - 1;
						*pbFoundSuffix = (bInputSuffix || bThisSuffix);
						*pbFoundAlias = (bInputAlias || bThisAlias);
					}
				}
			}
		}
	}

	// Company found if either flag is true
	return (bSuffix || bDesignator);
}
//-------------------------------------------------------------------------------------------------
long CEntityFinder::findFirstCaseWord(const string& strText, int iStartPos, bool bUpperCase, 
									  bool bAcceptDigit, bool bIsCompany)
{
	long	lPos = -1;

	// Separate string into individual words
	UCLID_COMUTILSLib::IRegularExprParserPtr ipParser = 
		m_ipMiscUtils->GetNewRegExpParserInstance("EntityFinder");
	ASSERT_RESOURCE_ALLOCATION("ELI13038", ipParser != NULL);

	if (ipParser != NULL)
	{
		ipParser->IgnoreCase = VARIANT_TRUE;

		// Get the desired substring
		string strSub = strText.substr( iStartPos, strText.length() - iStartPos );

		// Convert text to BSTR
		_bstr_t bstrText( strSub.c_str() );

		// Tokenize the string, i.e. find all "whole words"	
		ipParser->Pattern = "\\S+";
		IIUnknownVectorPtr ipMatches = ipParser->Find( bstrText, VARIANT_FALSE, VARIANT_FALSE );

		// Check each word
		int iCount = ipMatches->Size();
		ITokenPtr ipToken;
		long lStartPos, lEndPos;
		string strWord;
		bool	bLastWordLC = false;
		long	lLastWordStartPos = -1;
		for (int i = 0; i < iCount; i++)
		{
			CComBSTR bstrValue;
			// Retrieve this token
			ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At(i) )->Object1 );
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrValue );

			// Get the word as a string
			strWord = asString( bstrValue );

			// Stop here if "Described" is found
			string strUpper = strWord;
			makeUpperCase( strUpper );
			if (strUpper == "DESCRIBED")
			{
				return iStartPos + lStartPos;
			}

			// Ignore this word if it contains only punctuation characters
			if (hasOnlyPunctuation( strWord ))
			{
				continue;
			}

			// Ignore the word "of" if this is a Company and not the last word
			if (bIsCompany && (i < iCount - 1) && 
				((strWord[0] == 'o' || strWord[0] == 'O') && 
				(strWord[1] == 'f' || strWord[1] == 'F')))
			{
				// "of" will not be ignored if next word is also lower case
				if (strWord[0] == 'o')
				{
					bLastWordLC = true;
					lLastWordStartPos = lStartPos;
				}

				continue;
			}

			// Check for any upper case letter
			if (bUpperCase)
			{
				// Also accept this word if it contains ".com" (P16 #2020)
				if (hasUpperCaseLetter( strWord, bAcceptDigit ) || 
					(strWord.find( ".com" ) != string::npos))
				{
					// Reset LastWord items
					bLastWordLC = false;
					lLastWordStartPos = -1;

					// Retain the start position of this word and stop searching
					lPos = lStartPos + iStartPos;
					break;
				}		// end if hasUpperCaseLetter()
			}			// end if bUpper
			// Check for no upper case letters
			else
			{
				// Invert the bAcceptDigit flag since we want NO upper-case letters
				if (!hasUpperCaseLetter( strWord, !bAcceptDigit ))
				{
					// Check for preceding "of"
					if (bLastWordLC)
					{
						// Retain the start position of the preceding "of" and stop searching
						lPos = lLastWordStartPos + iStartPos;
					}
					else
					{
						// Retain the start position of this word and stop searching
						lPos = lStartPos + iStartPos;
					}
					break;
				}		// end if !hasUpperCaseLetter()
				else
				{
					// Reset LastWord items
					bLastWordLC = false;
					lLastWordStartPos = -1;
				}
			}			// end else !bUpper
		}				// end for each token/word
	}

	return lPos;
}
//-------------------------------------------------------------------------------------------------
long CEntityFinder::findFirstLowerCaseWordToTrim(const string& strText,
												 IRegularExprParserPtr ipParser,
												 int iStartPos, bool bIsCompany)
{
	long	lTrimPos = -1;
	long	lLength = strText.length();
	long	lCurrentStartPos = iStartPos;
	bool	bSeparatorFound = false;
	long	lSeparatorStartPos = -1;
	long	lSeparatorEndPos = -1;
	bool	bDone = false;

	while (!bDone)
	{
		// Find next lower-case word
		// Digits count as lower-case
		long lFirstLower = findFirstCaseWord( strText, lCurrentStartPos,
			false, true, bIsCompany );
		if (lFirstLower > -1)
		{
			// Check for this lower-case word immediately following a separator
			if (bSeparatorFound && (lFirstLower - lSeparatorEndPos < 4))
			{
				// Yes, next LC after Separator so trim the Separator
				lTrimPos = lSeparatorStartPos;
				bDone = true;
			}
			else
			{
				////////////////////////////
				// Check for valid separator
				////////////////////////////
				lSeparatorEndPos = findSeparatorWordEnd( strText, ipParser, lFirstLower, bIsCompany );
				if (lSeparatorEndPos != -1)
				{
					lSeparatorStartPos = lFirstLower;
					bSeparatorFound = true;
					lCurrentStartPos = lSeparatorEndPos + 1;
					continue;
				}
				else
				{
					// Just accept the result and trim here
					lTrimPos = lFirstLower;
					bDone = true;
				}			// end if LC word != separator
			}				// end else LC word NOT immediately after Separator
		}					// end if LC word found
		else
		{
			// No lower-case words to find, stop looking
			bDone = true;
		}
	}						// end while NOT done looking

	// Return position to be trimmed
	return lTrimPos;
}
//-------------------------------------------------------------------------------------------------
long CEntityFinder::findSeparatorWordEnd(const string& strText, IRegularExprParserPtr ipParser,
										 long lStart, bool bIsCompany)
{
	long lSeparatorEnd = -1;
	long lLength = strText.length();

	////////////////////////////////////////
	// Create collection of valid separators
	////////////////////////////////////////
	IShallowCopyablePtr ipCopier = m_ipKeys->GetPersonAlias( 
		(UCLID_AFUTILSLib::EPersonAliasType)kPersonAliasAll );
	ASSERT_RESOURCE_ALLOCATION("ELI26074", ipCopier != NULL);
	IVariantVectorPtr	ipSeparators = ipCopier->ShallowCopy();
	ASSERT_RESOURCE_ALLOCATION( "ELI09394", ipSeparators != NULL );
	long lSize = ipSeparators->Size;

	// Add "&", "and", "etux"
	ipSeparators->Insert( lSize++, _bstr_t( "&" ) );
	ipSeparators->Insert( lSize++, _bstr_t( "\\band\\b" ) );
	ipSeparators->Insert( lSize++, _bstr_t( "\\bet\\s*ux\\b" ) );

	// Add "0", "0."
	ipSeparators->Insert( lSize++, _bstr_t( "\\b0\\b(\\.)?" ) );

	// Add "of" and collected Company Aliases
	if (bIsCompany)
	{
		ipSeparators->Insert( lSize++, _bstr_t( "\\bof\\b" ) );
		ipSeparators->Append( m_ipKeys->GetCompanyAlias( 
			(UCLID_AFUTILSLib::ECompanyAliasType)kCompanyAliasAll ) );
	}

	//////////////////////
	// Test each separator
	//////////////////////

	// Create local SpatialString for testing
	ISpatialStringPtr	ipLocal( CLSID_SpatialString );
	ASSERT_RESOURCE_ALLOCATION( "ELI09395", ipLocal != NULL );
	ipLocal->CreateNonSpatialString(strText.c_str(), "");

	// Find earliest separator
	long lStartPos = -1;
	long lEndPos = -1;
	ipLocal->FindFirstItemInRegExpVector( ipSeparators, VARIANT_FALSE, VARIANT_FALSE, 
		lStart, ipParser, &lStartPos, &lEndPos );
	if (lStartPos == lStart)
	{
		// Check for non-whitespace character following end of separator
		if (lEndPos < lLength - 1)
		{
			string strRemainder = strText.substr( lEndPos + 1, lLength - lEndPos - 1 );
			if (containsNonWhitespaceChars( strRemainder ))
			{
				// Store the end position of the separator
				lSeparatorEnd = lEndPos;
			}
			// else Remainder string contains only whitespace AND
			// separator is not valid
		}
		// else separator ends strText AND 
		// separator is not valid
	}

	return lSeparatorEnd;
}
//-------------------------------------------------------------------------------------------------
bool CEntityFinder::foundKeywordPhraseOverlap(const string& strText1, const string& strText2,
											  IRegularExprParserPtr ipParser)
{
	// Construct test string as str1 + \n + str2
	string strTest = strText1;
	strTest += '\n';
	strTest += strText2;

	// Compute length of first string to determine if phrase overlaps
	long lSize1 = strText1.length();

	// Create long list of keyword phrases to include: 
	//   - PersonDesignators
	//   - PersonTrimIdentifiers
	IShallowCopyablePtr	ipPD = m_ipKeys->PersonDesignators;
	ASSERT_RESOURCE_ALLOCATION( "ELI15581", ipPD != NULL );
	IVariantVectorPtr	ipPhrases = ipPD->ShallowCopy();
	ASSERT_RESOURCE_ALLOCATION( "ELI10256", ipPhrases != NULL );
	ipPhrases->Append( m_ipKeys->PersonTrimIdentifiers );

	// Create SpatialString for testing
	ISpatialStringPtr	ipSpatial( CLSID_SpatialString );
	ASSERT_RESOURCE_ALLOCATION( "ELI10257", ipSpatial != NULL );
	ipSpatial->CreateNonSpatialString(strTest.c_str(), "");

	// Check test string for any overlapping phrase
	bool bDone = false;
	long lSearchPos = 0;
	while (!bDone)
	{
		long lStartPos = -1;
		long lEndPos = -1;
		ipSpatial->FindFirstItemInRegExpVector( ipPhrases, VARIANT_FALSE, VARIANT_FALSE,
			lSearchPos, ipParser, &lStartPos, &lEndPos );
		if (lStartPos != -1 && lEndPos != -1)
		{
			// A keyword phrase was found, check for overlap
			if ((lStartPos < lSize1) && (lEndPos < lSize1))
			{
				// Phrase is entirely on the first line, 
				// advance the search position and check again
				lSearchPos = lEndPos + 1;
			}
			else if ((lStartPos < lSize1) && (lEndPos > lSize1))
			{
				return true;
			}
			// else lStartPos >= lSize1 and no overlap was found
			else
			{
				bDone = true;
			}
		}
		else
		{
			// No phrase found, stop searching
			bDone = true;
		}
	}

	// No phrase was found or the found phrase did not overlap
	return false;
}
//-------------------------------------------------------------------------------------------------
string CEntityFinder::getAddressSuffixPattern()
{
	string strPattern;

	// Get path to file
	UCLID_AFUTILSLib::IAFUtilityPtr ipAFUtils( CLSID_AFUtility );
	ASSERT_RESOURCE_ALLOCATION( "ELI09474", ipAFUtils != NULL );

	_bstr_t bstrFolder = ipAFUtils->GetComponentDataFolder();
	string strPatternFile = bstrFolder.operator const char *();
	strPatternFile += "\\ReturnAddrFinder\\ReturnAddrSuffix.dat.etf";

	// Copy pattern from ReturnAddrSuffix.dat
	autoEncryptFile( strPatternFile, gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str() );
	if (isFileOrFolderValid( strPatternFile ))
	{
		strPattern = getRegExpFromFile( strPatternFile );
	}

	// Provide pattern to caller
	return strPattern;
}
//-------------------------------------------------------------------------------------------------
long CEntityFinder::getWordCount(const string& strText)
{
	long	lCount = -1;

	vector<string> vecTokens;
	StringTokenizer::sGetTokens(strText, gstrWhiteSpace, vecTokens, true);
	lCount = vecTokens.size();

	return lCount;
}
//-------------------------------------------------------------------------------------------------
void CEntityFinder::handleParentheses(ISpatialStringPtr &ripText, IRegularExprParserPtr ipParser)
{
	// Define search strings once
	static string strParenthesesPattern = "\\([^\\(\\)]*\\)";
	static string strBracketsPattern = "\\[[^\\(\\)]*\\]";

	// Get the collection of regular expressions for the existence of states [P16 2051]
	IShallowCopyablePtr ipStateList = m_ipKeys->GetKeywordCollection( "StateList" );
	ASSERT_RESOURCE_ALLOCATION("ELI15730", ipStateList != NULL);
	IVariantVectorPtr	ipSearchStates = ipStateList->ShallowCopy();

	// Create IMiscUtil object to process results
	IMiscUtilsPtr ipMiscUtils( CLSID_MiscUtils );
	ASSERT_RESOURCE_ALLOCATION( "ELI09387", ipMiscUtils != NULL );

	// Create object to hold found patterns
	IIUnknownVectorPtr ipFound( CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI09388", ipFound != NULL );

	// Define collection of regular expressions for Person Alias, Person Designator, and states
	IShallowCopyablePtr ipCopier = m_ipKeys->GetPersonAlias( 
		(UCLID_AFUTILSLib::EPersonAliasType)kPersonAliasAll );
	ASSERT_RESOURCE_ALLOCATION("ELI26082", ipCopier != NULL);
	IVariantVectorPtr	ipSearchRE = ipCopier->ShallowCopy();
	ASSERT_RESOURCE_ALLOCATION( "ELI09389", ipSearchRE != NULL );
	ipSearchRE->Append( m_ipKeys->PersonDesignators );
	ipSearchRE->Append( ipSearchStates );

	////////////////////////////
	// Handle paired parentheses
	////////////////////////////

	// Set pattern
	ipParser->Pattern = _bstr_t( strParenthesesPattern.c_str() );

	while (true)
	{
		// Find an instance of the search pattern
		ipFound = ipParser->Find( ripText->String, VARIANT_TRUE, VARIANT_FALSE );

		// Retrieve text from vector of results
		if (ipFound->Size() > 0)
		{
			// Get first (and only) result
			long nStart, nEnd;
			ipMiscUtils->GetRegExpData( ipFound, 0, -1, &nStart, &nEnd );

			ISpatialStringPtr	ipResult = ripText->GetSubString( nStart, nEnd );
			ASSERT_RESOURCE_ALLOCATION( "ELI09390", ipResult != NULL );

			// Check result for desired pattern (Alias or Designator)
			if (!asCppBool(ipResult->ContainsStringInVector(
				ipSearchRE, VARIANT_FALSE, VARIANT_TRUE, ipParser)))
			{
				// Not found, just remove the text and parentheses from the text
				ripText->Remove( nStart, nEnd );
			}
			else
			{
				// Desired pattern was found, just replace the parentheses with spaces
				ripText->SetChar( nStart, ' ' );
				ripText->SetChar( nEnd, ' ' );
			}
		}
		else
		{
			// Search pattern was not found
			break;
		}
	}

	////////////////////////////////
	// Handle paired square brackets
	////////////////////////////////

	// Set pattern
	ipParser->Pattern = _bstr_t( strBracketsPattern.c_str() );

	while (true)
	{
		// Find an instance of the search pattern
		ipFound = ipParser->Find( ripText->String, VARIANT_TRUE, VARIANT_FALSE );

		// Retrieve text from vector of results
		if (ipFound->Size() > 0)
		{
			// Get first (and only) result
			long nStart, nEnd;
			ipMiscUtils->GetRegExpData( ipFound, 0, -1, &nStart, &nEnd );

			ISpatialStringPtr	ipResult = ripText->GetSubString( nStart, nEnd );
			ASSERT_RESOURCE_ALLOCATION( "ELI09391", ipResult != NULL );

			// Check result for desired pattern (Alias or Designator)
			if (!asCppBool(ipResult->ContainsStringInVector(
				ipSearchRE, VARIANT_FALSE, VARIANT_TRUE, ipParser)))
			{
				// Not found, just remove the text and brackets from the text
				ripText->Remove( nStart, nEnd );
			}
			else
			{
				// Desired pattern was found, just replace the brackets with spaces
				ripText->SetChar( nStart, ' ' );
				ripText->SetChar( nEnd, ' ' );
			}
		}
		else
		{
			// Search pattern was not found
			break;
		}
	}

	// Replace any leftover single parentheses and brackets with spaces
	ripText->Replace("(", " ", VARIANT_FALSE, 0, NULL);
	ripText->Replace(")", " ", VARIANT_FALSE, 0, NULL);
	ripText->Replace("[", " ", VARIANT_FALSE, 0, NULL);
	ripText->Replace("]", " ", VARIANT_FALSE, 0, NULL);
}
//-------------------------------------------------------------------------------------------------
bool CEntityFinder::doTrustTrimming(ISpatialStringPtr &ripSpatial, IRegularExprParserPtr ipParser)
{
	bool bFound = false;
	long lSize = ripSpatial->Size;

	// Retrieve list of Trust Indicator expressions
	IVariantVectorPtr ipTrustInd = m_ipKeys->GetKeywordCollection( _bstr_t( "TrustIndicators" ) );
	ASSERT_RESOURCE_ALLOCATION( "ELI10347", ipTrustInd != NULL );

	// Local string for searches and trimming
	string strTrust = asString(ripSpatial->String);

	// Search the string for a Trust Indicator
	long lTrustStartPos = -1;
	long lTrustEndPos = -1;
	long lStart = 0;
	long lLastTrust = 0;
	bool bDone = false;
	while (!bDone)
	{
		ripSpatial->FindFirstItemInRegExpVector( ipTrustInd, VARIANT_FALSE, VARIANT_FALSE,
			lStart, ipParser, &lTrustStartPos, &lTrustEndPos );

		// Continue only if a Trust Indicator was found
		if (lTrustStartPos > -1 && (lTrustEndPos > -1) && (lTrustStartPos - lStart < 150))
		{
			// Basic validation of second and later TRUST items
			if (lLastTrust > 0)
			{
				string strInBetween = strTrust.substr( lLastTrust + 1, 
					lTrustStartPos - lLastTrust - 1 );

				// Ignore this TRUST item if
				//   -- substring contains no upper-case letters OR
				//   -- substring contains "(" but not ")"
				if ((strInBetween.find_first_of( "ABCDEFGHIJKLMNOPQRSTUVWXYZ" ) == string::npos) || 
					((strInBetween.find( '(' ) != string::npos) && (strInBetween.find( ')' ) == string::npos)))
				{
					// Ignore this item with respect to upcoming trimming

					// Update the starting position for the next search
					lStart = lTrustEndPos;
				}
				else
				{
					// Valid item, update the starting position for the next search
					lStart = lTrustEndPos;

					// Save last end point
					lLastTrust = lTrustEndPos;
				}
			}
			// Assume that first TRUST is okay
			else
			{
				// Update the starting position for the next search
				lStart = lTrustEndPos;

				// Save last end point
				lLastTrust = lTrustEndPos;
			}
		}
		else
		{
			bDone = true;
		}
	}

	// Continue only if a Trust Indicator was found
	if (lStart > 0)
	{
		// Retrieve list of Non Trust Indicator expressions
		IVariantVectorPtr ipNonTrust = m_ipKeys->GetKeywordCollection( _bstr_t( "NonTrust" ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI10485", ipNonTrust != NULL );

		// Continue only if a Non-Trust Indicator was not found
		if (!asCppBool(ripSpatial->ContainsStringInVector(
			ipNonTrust, VARIANT_FALSE, VARIANT_TRUE, ipParser)))
		{
			bFound = true;

			// Just trim the string at the Trust endpoint (if needed)
			// Check after first Trust indicator
			if (lSize > lStart + 1)
			{
				long lTrimPos = trimAfterTrust( strTrust, lLastTrust + 1 );
				if (lTrimPos > 0)
				{
					ripSpatial = ripSpatial->GetSubString( 0, lTrimPos );
				}
			}

			// Provide debug output
			string strResult = asString( ripSpatial->String );
			::convertCppStringToNormalString( strResult );
			TRACE( "Trust Output = \"%s\"\r\n", strResult.c_str() );
		}
	}

	return bFound;
}
//-------------------------------------------------------------------------------------------------
long CEntityFinder::trimAfterTrust(const string& strInput, long lSearchStart)
{
	long	lTrimPos = 0;
	long	lLength = strInput.length();

	// Only search if start position is within the string
	if ((lSearchStart > 0) && (lSearchStart < lLength - 1))
	{
		///////////////////////////////////////
		// Step 1: Search for a lower-case word
		///////////////////////////////////////
		long lFirstLower = findFirstCaseWord( strInput, lSearchStart, 
			false, true, false );
		if (lFirstLower > -1)
		{
			lTrimPos = lFirstLower - 1;
		}

		////////////////////////////
		// Step 2A: Search for DATED
		////////////////////////////
		long lPos = strInput.find( "DATED", lSearchStart );
		if ((lPos != string::npos) && 
			(((lTrimPos > 0) && (lPos < lTrimPos)) | (lTrimPos == 0)))
		{
			// Retain this trim position only if earlier than previously found
			lTrimPos = lPos - 1;
		}

		lPos = strInput.find( "Dated", lSearchStart );
		if ((lPos != string::npos) && 
			(((lTrimPos > 0) && (lPos < lTrimPos)) | (lTrimPos == 0)))
		{
			// Retain this trim position only if earlier than previously found
			lTrimPos = lPos - 1;
		}

		//////////////////////////
		// Step 2B: Search for DTD
		//          and variations
		//////////////////////////
		lPos = strInput.find( "DTD", lSearchStart );
		if ((lPos != string::npos) && 
			(((lTrimPos > 0) && (lPos < lTrimPos)) | (lTrimPos == 0)))
		{
			// Retain this trim position only if earlier than previously found
			lTrimPos = lPos - 1;
		}

		lPos = strInput.find( "Dtd", lSearchStart );
		if ((lPos != string::npos) && 
			(((lTrimPos > 0) && (lPos < lTrimPos)) | (lTrimPos == 0)))
		{
			// Retain this trim position only if earlier than previously found
			lTrimPos = lPos - 1;
		}

		lPos = strInput.find( "UTD", lSearchStart );
		if ((lPos != string::npos) && 
			(((lTrimPos > 0) && (lPos < lTrimPos)) | (lTrimPos == 0)))
		{
			// Retain this trim position only if earlier than previously found
			lTrimPos = lPos - 1;
		}

		////////////////////////////////
		// Step 2C: Search for AGREEMENT
		////////////////////////////////
		lPos = strInput.find( "AGREEMENT", lSearchStart );
		if ((lPos != string::npos) && 
			(((lTrimPos > 0) && (lPos < lTrimPos)) | (lTrimPos == 0)))
		{
			// Retain this trim position only if earlier than previously found
			lTrimPos = lPos - 1;
		}

		lPos = strInput.find( "Agreement", lSearchStart );
		if ((lPos != string::npos) && 
			(((lTrimPos > 0) && (lPos < lTrimPos)) | (lTrimPos == 0)))
		{
			// Retain this trim position only if earlier than previously found
			lTrimPos = lPos - 1;
		}

		/////////////////////////////////
		// Step 3: Search for punctuation
		/////////////////////////////////
		lPos = strInput.find_first_of( ".,;:", lSearchStart );
		if ((lPos != string::npos) && 
			(((lTrimPos > 0) && (lPos < lTrimPos)) | (lTrimPos == 0)))
		{
			// Retain this trim position only if earlier than previously found
			lTrimPos = lPos - 1;
		}
	}

	return lTrimPos;
}
//-------------------------------------------------------------------------------------------------
string CEntityFinder::handleTrustDated(string strInput)
{
	////////////////////////////////////
	// Check first words for only digits
	// Remove first words if only digits
	////////////////////////////////////
	long lTrustPos = strInput.find("TRUST", 0 );
	if (lTrustPos != string::npos)
	{
		bool	bDatedFound = false;

		// Check for immediately following DATED
		long lDatedPos = strInput.find("DATED", lTrustPos );
		if (lDatedPos == lTrustPos + 6)
		{
			// Set flag
			bDatedFound = true;
		}

		// Check for immediately following DTD
		if (!bDatedFound)
		{
			lDatedPos = strInput.find("DTD", lTrustPos );
			if (lDatedPos == lTrustPos + 6)
			{
				// Set flag
				bDatedFound = true;
			}
		}

		// Trim the string if appropriate
		if (bDatedFound)
		{
			strInput = strInput.substr( 0, lTrustPos + 5 );
		}
	}

	// Provide updated string to caller
	return strInput;
}
//-------------------------------------------------------------------------------------------------
bool CEntityFinder::hasDateText(const string& strText, IRegularExprParserPtr ipParser)
{
	bool	bFoundDate = false;

	// Trim leading whitespace
	string strLocal = trim( strText, " \r\n", "" );

	// Create SpatialString for testing
	ISpatialStringPtr	ipSpatial( CLSID_SpatialString );
	ASSERT_RESOURCE_ALLOCATION( "ELI08751", ipSpatial != NULL );
	ipSpatial->CreateNonSpatialString(strLocal.c_str(), "");

	// Check string for Month Word
	long	lStartPos = -1;
	long	lEndPos = -1;
	ipSpatial->FindFirstItemInRegExpVector( m_ipKeys->MonthWords, 
					VARIANT_FALSE, VARIANT_FALSE, 0, ipParser, &lStartPos, &lEndPos );
	if (lStartPos != -1 && lEndPos != -1)
	{
		// Check for digits within the next 5 characters
		if ((long)strLocal.length() > lEndPos + 2)
		{
			long lDigitPos = strLocal.find_first_of( "0123456789", lEndPos + 1 );
			if ((lDigitPos != string::npos) && (lDigitPos < lEndPos + 6))
			{
				bFoundDate = true;
			}
			// else digits are too far away to be considered part of a date
		}
		// else string ends with a Month Word
		// This is suspicious but not grounds for trimming
	}

	return bFoundDate;
}
//-------------------------------------------------------------------------------------------------
bool CEntityFinder::hasOnlyPunctuation(const string& strWord)
{
	// Define simple punctuation string
	// Do NOT treat "&" as a punctuation character
	static string strPunct( ".,;:'!/)" );

	return (strWord.find_first_not_of(strPunct) == string::npos);
}
//-------------------------------------------------------------------------------------------------
bool CEntityFinder::hasUpperCaseLetter(const string& strWord, bool bAcceptDigit)
{
	string strSearch = bAcceptDigit ? gstrUpperWithDigits : gstrUpperCase;

	return (strWord.find_first_of(strSearch) != string::npos);
}
//-------------------------------------------------------------------------------------------------
bool CEntityFinder::isAbbreviation(const string& strWord)
{
	// Check for lower-case character or digit
	long lPos = strWord.find_first_of(gstrLowerWithDigits);

	// Definitely NOT an abbreviation if LC character or digit found
	// also, only a short string is likely to be an abbreviation so
	// check word count and string length (not an abbreviation if length > 8 and
	// word count > 1)
	return !(lPos != string::npos || (strWord.length() > 8 && getWordCount(strWord) > 1));
}
//-------------------------------------------------------------------------------------------------
bool CEntityFinder::isKeywordPhrase(const string& strText, IRegularExprParserPtr ipParser)
{
	// Trim whitespace and punctuation from input string
	string strTest = trim( strText, " \r\n.,;", " \r\n.,;" );

	// Get length of string to determine if phrase exactly matches
	long lSize = strTest.length();

	// Create long list of keyword phrases to include:
	//   - PersonDesignators
	//   - PersonTrimIdentifiers
	IShallowCopyablePtr	ipPD = m_ipKeys->PersonDesignators;
	ASSERT_RESOURCE_ALLOCATION( "ELI15582", ipPD != NULL );
	IVariantVectorPtr	ipPhrases = ipPD->ShallowCopy();
	ASSERT_RESOURCE_ALLOCATION( "ELI10258", ipPhrases != NULL );
	ipPhrases->Append( m_ipKeys->PersonTrimIdentifiers );

	// Create SpatialString for testing
	ISpatialStringPtr	ipSpatial( CLSID_SpatialString );
	ASSERT_RESOURCE_ALLOCATION( "ELI10259", ipSpatial != NULL );
	ipSpatial->CreateNonSpatialString(strTest.c_str(), "");

	// Check test string for an exact match
	long lStartPos = -1;
	long lEndPos = -1;
	ipSpatial->FindFirstItemInRegExpVector( ipPhrases, VARIANT_FALSE, VARIANT_FALSE, 
		0, ipParser, &lStartPos, &lEndPos );
	if ((lStartPos == 0) && (lEndPos == lSize - 1))
	{
		// A keyword phrase exactly matched
		return true;
	}

	// No exact match was found
	return false;
}
//-------------------------------------------------------------------------------------------------
void CEntityFinder::logResults(string strInitial, string strFinal)
{
	// Convert each string to single-line string
	::convertCppStringToNormalString( strInitial );
	::convertCppStringToNormalString( strFinal );

	// Get path to log file
	string strLogFile = ::getModuleDirectory(_Module.m_hInst) + "\\" + string( "EFALog.dat" );

	// Open the log file
	ofstream ofsLogFile( strLogFile.c_str(), (ios::out | ios::app) );

	// Get the output string
	string	strPipe( "|" );
	string	strOut = strInitial + strPipe + strFinal;

	// Write string to log file
	ofsLogFile << strOut << endl << endl;

	// Close the log file
	ofsLogFile.close();
	waitForFileToBeReadable(strLogFile);
}
//-------------------------------------------------------------------------------------------------
long CEntityFinder::makeSpaceForAnd(ISpatialStringPtr ipText, IRegularExprParserPtr ipParser)
{
	// Retrieve existing case-sensitivity setting
	VARIANT_BOOL vbCase = ipParser->GetIgnoreCase();

	////////////////////////////////////////////////
	// Define pattern for leading upper-case letters
	////////////////////////////////////////////////
	ipParser->PutPattern( _bstr_t( "[A-Z]{2,}and\\s" ) );
	ipParser->PutIgnoreCase( VARIANT_FALSE );

	// Locate match
	IIUnknownVectorPtr	ipMatch1 = ipParser->Find( ipText->String, VARIANT_TRUE, VARIANT_FALSE );
	ASSERT_RESOURCE_ALLOCATION( "ELI10532", ipMatch1 != NULL );

	long		lStartPos, lEndPos;
	ITokenPtr	ipToken;

	// Retrieve this token
	if (ipMatch1->Size() > 0)
	{
		ipToken = ITokenPtr( IObjectPairPtr( ipMatch1->At( 0 ) )->Object1 );
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

		// Replace case-sensitivity setting
		ipParser->PutIgnoreCase( vbCase );

		return lEndPos - 3;
	}

	/////////////////////////////////////////////////
	// Define pattern for trailing upper-case letters
	/////////////////////////////////////////////////
	ipParser->PutPattern( _bstr_t( "\\sand[A-Z]{2,}" ) );

	// Locate match
	IIUnknownVectorPtr	ipMatch2 = ipParser->Find( ipText->String, VARIANT_TRUE, VARIANT_FALSE );
	ASSERT_RESOURCE_ALLOCATION( "ELI10533", ipMatch2 != NULL );

	// Retrieve this token
	if (ipMatch2->Size() > 0)
	{
		ipToken = ITokenPtr( IObjectPairPtr( ipMatch2->At( 0 ) )->Object1 );
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

		// Replace case-sensitivity setting
		ipParser->PutIgnoreCase( vbCase );

		return lStartPos + 4;
	}

	// Replace case-sensitivity setting
	ipParser->PutIgnoreCase( vbCase );

	// No match found
	return -1;
}
//-------------------------------------------------------------------------------------------------
long CEntityFinder::removeFirstDigitsWords(const string& strInput, IRegularExprParserPtr ipParser)
{
	static string strBreakChars( "()[]" );

	///////////////////////////////////////////////////////////
	// Check first words for digits and letters
	// Acceptable: 
	//   Letters
	//   Digits + Letters
	// Not Acceptable: 
	//   Digits
	//   Letters + Digits
	//   Digits + Letters + Digits
	///////////////////////////////////////////////////////////
	ipParser->Pattern = "\\S+";
	IIUnknownVectorPtr ipMatches = ipParser->Find(strInput.c_str(), VARIANT_FALSE, 
		VARIANT_FALSE );
	long lCount = ipMatches->Size();
	long lTrimPos = 0;
	if (lCount > 0)
	{
		// Retrieve first token
		long		lStartPos, lEndPos;
		ITokenPtr	ipToken;

		// Review each token until a non-digit word is found
		for (long lItem = 0; lItem < lCount; lItem++)
		{
			// Retrieve this token
			ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( lItem ) )->Object1 );
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

			// Get the word
			string strWord = strInput.substr( lStartPos, lEndPos - lStartPos + 1 );

			// Check for digits
			long lDigitPos = strWord.find_first_of(gstrDigits, 0 );

			// Check for subsequent letters
			bool bNotAcceptable = false;
			if (lDigitPos != string::npos)
			{
				// Letters + Digits
				if (lDigitPos > 0)
				{
					bNotAcceptable = true;
				}
				else
				{
					// Check for letters after digits
					long lLetterPos = strWord.find_first_of(gstrLetters, lDigitPos); 
					if (lLetterPos == string::npos)
					{
						// Only digits - except for four-digit year
						if ((strWord.length() != 4) || (strWord[0] == '0'))
						{
							bNotAcceptable = true;
						}
					}
					// Check for more digits
					if (lLetterPos != string::npos)
					{
						// Digits + Letters + Digits
						if (strWord.find_first_of(gstrDigits, lLetterPos ) != string::npos)
						{
							bNotAcceptable = true;
						}
					}
				}
			}

			if (bNotAcceptable)
			{
				// Update trim position
				lTrimPos = lEndPos + 1;
			}
			else
			{
				// Word contains alphabetic characters, retain this word
				break;
			}
		}
	}

	// Provide updated string to caller
	return lTrimPos;
}
//-------------------------------------------------------------------------------------------------
string	CEntityFinder::trimLeadingNonsense(string strInput, IRegularExprParserPtr ipParser)
{
	long	lLength = strInput.length();

	// Continue trimming sequence until no further trims are needed
	bool	bStillTrimming = true;
	while (bStillTrimming)
	{
		// Clear flag for this iteration
		bStillTrimming = false;

		///////////////////////////////////////////////
		// Step 1A: Trim leading non-alphanumeric chars
		//          Left parentheses are also okay
		///////////////////////////////////////////////
		long lStartPos = strInput.find_first_of( 
			"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789(" );
		if (lStartPos > 0)
		{
			// Trim before the first alpha char
			strInput = strInput.substr( lStartPos, lLength - lStartPos );
			lLength = strInput.length();

			// Set flag
			bStillTrimming = true;
		}
		else if (lStartPos == string::npos)
		{
			// Only non-alphanumeric characters, erase the string
			strInput = "";
			break;
		}

		/////////////////////////////////////////
		// Step 1B: Trim leading lower-case words
		/////////////////////////////////////////

		// Find first word that contains an upper-case letter
		// Digits do count as upper-case
		// This is NOT a Company yet
		int iFirstUpper = findFirstCaseWord( strInput, 0, true, true, false );
		if (iFirstUpper > 0)
		{
			// Remove leading lower-case text
			strInput = strInput.substr( iFirstUpper, lLength - iFirstUpper );

			// Trim this word if starts with a lower-case character
			// Unless it contains ".com" (P16 #2020)
			if ((strInput[0] >= 'a') && (strInput[0] <= 'z') && 
				(strInput.find( ".com" ) == string::npos))
			{
				long lWhitePos = strInput.find_first_of( " \r\n" );
				if (lWhitePos != string::npos)
				{
					strInput = strInput.substr( lWhitePos, lLength - lWhitePos );
				}
			}

			lLength = strInput.length();

			// Set flag
			bStillTrimming = true;
		}
		else if (iFirstUpper == string::npos)
		{
			// Only lower-case characters, erase the string
			strInput = "";
			break;
		}

		//////////////////////////////////////////
		// Step 1C: Trim leading digits-only words
		//////////////////////////////////////////
		long lTrimPos = removeFirstDigitsWords( strInput, ipParser );
		if (lTrimPos > 0)
		{
			// Remove leading digits-only words
			strInput = strInput.substr( lTrimPos, lLength - lTrimPos );
			lLength = strInput.length();

			// Set flag
			bStillTrimming = true;
		}

		// Trim this leading word before Step 1D
		bStillTrimming |= trimLeadingWord( strInput, "A N D" );

		///////////////////////////////////////////////
		// Step 1D: Trim lines that are 3 chars or less
		//          followed by a blank line
		///////////////////////////////////////////////
		long lLinePos = strInput.find( "\r\n\r\n" );
		if ((lLinePos != string::npos) && (lLinePos < 4))
		{
			// remove the characters before the \r\n and
			// remove the \r\n as well
			strInput = strInput.substr( lLinePos + 1, string::npos);
			lLength = strInput.length();

			// Set flag
			bStillTrimming = true;
		}
	}					// end while continuing through the trimming sequence

	return strInput;
}
//-------------------------------------------------------------------------------------------------
bool CEntityFinder::removeAddressText(const ISpatialStringPtr& ripText,
									  IRegularExprParserPtr ipParser)
{
	bool bFound = false;

	// Trim leading spaces and new lines
	ripText->Trim( _bstr_t( " \r\n" ), _bstr_t( "" ) );

	///////////////////////////////////////////////////////
	// Search for Addresses with leading Address Indicators
	///////////////////////////////////////////////////////
	string	strNow = asString( ripText->String );

	// Retrieve collection of Address Indicator items
	IShallowCopyablePtr	ipAI = m_ipKeys->AddressIndicators;
	ASSERT_RESOURCE_ALLOCATION( "ELI15583", ipAI != NULL );
	IVariantVectorPtr	ipAddress = ipAI->ShallowCopy();
	ASSERT_RESOURCE_ALLOCATION( "ELI09464", ipAddress != NULL );

	// Get Address Suffix pattern
	string strAddressSuffix = getAddressSuffixPattern();

	// Append Address Suffix pattern to each Indicators item
	long lSize = ipAddress->Size;
	for (long i = 0; i < lSize; i++)
	{
		// Retrieve this Indicator phrase
		_bstr_t bstrPattern = ipAddress->GetItem( i );

		// Append 100-character allowance for Address text
		bstrPattern += "[\\s\\S]{0,100}?";

		// Append Address Suffix pattern
		bstrPattern += strAddressSuffix.c_str();

		// Replace enhanced pattern in the collection
		ipAddress->Set(i, _variant_t(bstrPattern));
	}

	// Search for each instance of Address text
	while (true)
	{
		long lStartPos = -1;
		long lEndPos = -1;
		ripText->FindFirstItemInRegExpVector( ipAddress, VARIANT_FALSE, VARIANT_FALSE,
			0, ipParser, &lStartPos, &lEndPos );
		if ((lStartPos != -1) && (lEndPos != -1))
		{
			// Remove the Address text less one character
			ripText->Remove( lStartPos, lEndPos - 1 );

			// Replace retained character with semicolon to facilitate later splitting
			ripText->SetChar( lStartPos, ';' );

			// Change any carriage return chars to spaces
			// to avoid later blank-line trimming issue
			lStartPos--;
			for (; lStartPos > 0; lStartPos--)
			{
				char c = (char) ripText->GetChar(lStartPos);
				if (c == '\n' || c == '\r')
				{
					ripText->SetChar(lStartPos, ' ');
				}
			}

			// Set flag
			bFound = true;
		}
		else
		{
			break;
		}
	}
	strNow = asString( ripText->String );

	//////////////////////////////////////////////////////////
	// Search for Addresses without leading Address Indicators
	//////////////////////////////////////////////////////////
	if (!bFound)
	{
		// Clear the collection of regular expressions
		ipAddress->Clear();

		// Add pattern for Address without special leading phrase
		//     one or more digits followed by 
		//     up to 100 characters followed by 
		//     Address Suffix
		_bstr_t bstrPattern = "[\\s\\S]{0,100}?";
		bstrPattern += strAddressSuffix.c_str();
		_bstr_t bstr1 = "\\b\\d+" + bstrPattern;
		ipAddress->PushBack( bstr1 );

		// Add pattern for Address with "OF" followed by 
		//     one or more digits followed by 
		//     up to 100 characters followed by 
		//     Address Suffix
		_bstr_t bstr2 = "\\bOf\\s+\\d+" + bstrPattern;
		ipAddress->PushBack( bstr2 );
	}

	// Search for each instance of Address text
	while (true)
	{
		long lStartPos = -1;
		long lEndPos = -1;
		ripText->FindFirstItemInRegExpVector( ipAddress, VARIANT_FALSE, VARIANT_FALSE,
			0, ipParser, &lStartPos, &lEndPos );

		// Address must be more than 10 characters long
		if ((lStartPos != -1) && lEndPos != -1 && (lEndPos - lStartPos > 9))
		{
			// Remove the Address text less one character
			ripText->Remove( lStartPos, lEndPos - 1 );

			// Replace retained character with semicolon to facilitate later splitting
			ripText->SetChar( lStartPos, ';' );

			// Change any carriage return chars to spaces
			// to avoid later blank-line trimming issue
			lStartPos--;
			for (; lStartPos > 0; lStartPos--)
			{
				char c = (char) ripText->GetChar(lStartPos);
				if (c == '\n' || c == '\r')
				{
					ripText->SetChar(lStartPos, ' ');
				}
			}

			// Set flag
			bFound = true;
		}
		else
		{
			break;
		}
	}
	strNow = asString( ripText->String );

	////////////////////////////////
	// Search for trailing "Address"
	////////////////////////////////
	// Clear the collection of regular expressions
	ipAddress->Clear();

	// Add pattern for Address
	_bstr_t bstrText = "\\bAddr.{0,2}s.{0,3}\\b";
	ipAddress->PushBack( bstrText );

	while (true)
	{
		long lStartPos = -1;
		long lEndPos = -1;
		ripText->FindFirstItemInRegExpVector( ipAddress, VARIANT_FALSE, VARIANT_FALSE,
			0, ipParser, &lStartPos, &lEndPos );

		// Text must be near the end of the string
		if (lEndPos > -1 && ripText->Size - lEndPos < 10)
		{
			// Remove the Address text
			ripText->Remove( lStartPos, lEndPos );

			// Set flag
			bFound = true;
		}
		else
		{
			break;
		}
	}

	return bFound;
}
//-------------------------------------------------------------------------------------------------
void CEntityFinder::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI05880", "Find Company or Person(s)");
}
//-------------------------------------------------------------------------------------------------
