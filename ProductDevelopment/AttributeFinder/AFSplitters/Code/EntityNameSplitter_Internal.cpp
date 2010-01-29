// EntityNameSplitter_Internal.cpp : Implementation of CEntityNameSplitter Private functions
#include "stdafx.h"
#include "AFSplitters.h"
#include "EntityNameSplitter.h"
#include "PersonNameSplitter.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// private / helper methods
//-------------------------------------------------------------------------------------------------
bool CEntityNameSplitter::doTrustSplitting(ISpatialStringPtr ipGroup, IAFDocumentPtr ipAFDoc, 
										   IIUnknownVectorPtr ipMainAttrSub)
{
	// Find delimiters between potential Entities
	IIUnknownVectorPtr ipDelimiters( CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI10325", ipDelimiters != NULL );
	findNameDelimiters( ipGroup, true, ipDelimiters );
	long	lSize = ipDelimiters->Size();

	// Local collection of Trust expressions
	IVariantVectorPtr ipTrustInd = m_ipKeys->GetKeywordCollection( _bstr_t( "TrustIndicators" ) );
	ASSERT_RESOURCE_ALLOCATION( "ELI10331", ipTrustInd != NULL );

	// Local collection of Non Trust expressions
	IVariantVectorPtr ipNonTrust = m_ipKeys->GetKeywordCollection( _bstr_t( "NonTrust" ) );
	ASSERT_RESOURCE_ALLOCATION( "ELI10486", ipNonTrust != NULL );

	// Create collection of valid Entities
	IIUnknownVectorPtr ipEntities( CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI10330", ipEntities != NULL );

	long	lEntityCount = 0;
	long	lLastTrustIndex = -1;
	int		i;
	for (i = 0; i <= lSize; i++)
	{
		// Retrieve this potential Entity
		ISpatialStringPtr	ipItem = getEntityFromDelimiters( i, ipGroup, ipDelimiters );

		// Add to collection if valid
		if (isValidEntity( ipItem, false ))
		{
			// Check if Entity contains a Trust Indicator
			if (asCppBool(ipItem->ContainsStringInVector(
				ipTrustInd, VARIANT_FALSE, VARIANT_TRUE,  m_ipRegExprParser)))
			{
				// Continue only if a Non-Trust Indicator was not found
				if (!asCppBool(ipItem->ContainsStringInVector(
					ipNonTrust, VARIANT_FALSE, VARIANT_TRUE,  m_ipRegExprParser)))
				{
					// Save this index
					lLastTrustIndex = lEntityCount;
				}
			}

			// Increment count
			lEntityCount++;

			// Add to collection
			ipEntities->PushBack( ipItem );
		}
	}

	// Create the Trust sub-attribute
	if (lLastTrustIndex != -1)
	{
		// Retrieve the Trust Entity
		ISpatialStringPtr ipTrust = ipEntities->At( lLastTrustIndex );
		ASSERT_RESOURCE_ALLOCATION( "ELI10332", ipTrust != NULL );

		// Trim leading THE from Trust text
		string strStart = ipTrust->String;
		string strTemp( strStart );
		if (trimLeadingWord( strTemp, "THE" ))
		{
			long lPos = strStart.find( strTemp );
			ipTrust = ipTrust->GetSubString( lPos, -1 );
		}

		// Create the Trust attribute
		IAttributePtr ipTrustAttr( CLSID_Attribute );
		ASSERT_RESOURCE_ALLOCATION( "ELI10333", ipTrustAttr != NULL );

		// Move name(s), if desired
		if (m_bMoveTrustName)
		{
			moveTrustNames( ipTrust );
		}

		ipTrustAttr->Value = ipTrust;
		ipTrustAttr->Name = "Trust";

		// Retrieve collection of Trustees
		IIUnknownVectorPtr ipTrustees = ipTrustAttr->SubAttributes;
		ASSERT_RESOURCE_ALLOCATION( "ELI10336", ipTrustees != NULL );

		// Create Person Splitter object
		IAttributeSplitterPtr ipPersonSplitter( CLSID_PersonNameSplitter );
		ASSERT_RESOURCE_ALLOCATION( "ELI10337", ipPersonSplitter != NULL );

		// Add each remaining Entity as a sub-attribute under the Trust
		for (i = 0; i < lEntityCount; i++)
		{
			// Skip the Trust item
			if (i == lLastTrustIndex)
			{
				continue;
			}

			// Retrieve this Trustee Entity
			ISpatialStringPtr ipTrustee = ipEntities->At( i );
			ASSERT_RESOURCE_ALLOCATION( "ELI10334", ipTrustee != NULL );

			// Create the Trustee attribute
			IAttributePtr ipTrusteeAttr( CLSID_Attribute );
			ASSERT_RESOURCE_ALLOCATION( "ELI10335", ipTrusteeAttr != NULL );

			// Judge Person vs. Company
			if (entityIsCompany( ipTrustee ))
			{
				ipTrusteeAttr->Name = "Company";
				ipTrusteeAttr->Value = ipTrustee;
				ipTrusteeAttr->Type = "Trustee";
			}
			else
			{
				ipTrusteeAttr->Name = "Person";
				ipTrusteeAttr->Value = ipTrustee;
				ipTrusteeAttr->Type = "Trustee";

				// Split the Person Trustee
				ipPersonSplitter->SplitAttribute( ipTrusteeAttr, ipAFDoc, NULL );
			}

			// Add Trustee to collection
			ipTrustees->PushBack( ipTrusteeAttr );
		}

		// Add completed Trust tree to collected sub-attributes
		ipMainAttrSub->PushBack( ipTrustAttr );
		return true;
	}
	else
	{
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
bool CEntityNameSplitter::doLeadingWordTrim(ISpatialStringPtr& ripEntity, string strTrim)
{
	// Initial test for empty string
	if (ripEntity->IsEmpty() == VARIANT_TRUE)
	{
		// Nothing to trim
		return false;
	}

	// Search for strTrim within local string
	bool bReturn = false;
	string strStart = ripEntity->String;
	string strTemp = strStart;
	if (trimLeadingWord( strTemp, strTrim.c_str() ))
	{
		if (strTemp.length() == 0)
		{
			// Entire text is trimmed
			ripEntity->Clear();
			return true;
		}
		else
		{
			// Avoid infinite loop if lPos == 0
			long lPos = strStart.find( strTemp );
			if (lPos == 0)
			{
				// Look again - starting with second character
				lPos = strStart.find( strTemp, 1 );
			}

			// Do the same trimming on the Spatial String 
			ripEntity = ripEntity->GetSubString( lPos, -1 );
			return true;
		}
	}

	// strTrim not found
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CEntityNameSplitter::entityIsCompany(ISpatialStringPtr ipEntity)
{
	bool bCompany = false;

	// Retrieve collection of Company Suffixes
	// Add collection of Company Designators
	IShallowCopyablePtr ipCopier = m_ipKeys->GetKeywordCollection("CompanySuffixes");
	ASSERT_RESOURCE_ALLOCATION("ELI26067", ipCopier != NULL);
	IVariantVectorPtr ipIndicators = ipCopier->ShallowCopy();
	ASSERT_RESOURCE_ALLOCATION("ELI10536", ipIndicators != NULL);
	ipIndicators->Append(m_ipKeys->GetKeywordCollection("CompanyDesignators"));

	// Check for Company indication
	if (asCppBool(ipEntity->ContainsStringInVector(
		ipIndicators, VARIANT_FALSE, VARIANT_TRUE,  m_ipRegExprParser)))
	{
		// The entity looks like a Company
		bCompany = true;
	}

	// Additional test for Municipality
	if (!bCompany)
	{
		// Retrieve Municiaplity patterns
		ipCopier = m_ipKeys->GetKeywordCollection("MunicipalityIndicators");
		ASSERT_RESOURCE_ALLOCATION("ELI26068", ipCopier != NULL);
		ipIndicators = ipCopier->ShallowCopy();

		// Test
		// Check if ipEntity contains any strings in ipIndicators
		if (asCppBool(ipEntity->ContainsStringInVector(
			ipIndicators, VARIANT_FALSE, VARIANT_TRUE,  m_ipRegExprParser)))
		{
			// The entity looks like a Company
			bCompany = true;
		}
	}

	// Additional test for digits
	if (!bCompany)
	{
		string strTest = asString(ipEntity->String);
		long lDigitPos = strTest.find_first_of( "0123456789", 0 );
		if (lDigitPos != string::npos)
		{
			// The entity looks like a Company
			bCompany = true;
		}
	}

	return bCompany;
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::moveTrustNames(ISpatialStringPtr &ripTrust)
{
	string strOriginal = asString(ripTrust->String);

	// Divide Trust text into words
	IIUnknownVectorPtr	ipWords( CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI10446", ipWords != NULL );
	long lCount = getWordsFromString( strOriginal, ipWords );

	// Continue processing only if at least three words
	if (lCount > 2)
	{
		// Retrieve collection of Trust flags
		// Add collection of Person Suffixes
		IShallowCopyablePtr ipCopier = m_ipKeys->GetKeywordCollection("TrustFlags");
		ASSERT_RESOURCE_ALLOCATION("ELI26069", ipCopier != NULL);
		IVariantVectorPtr ipFlags = ipCopier->ShallowCopy();
		ASSERT_RESOURCE_ALLOCATION( "ELI10447", ipFlags != NULL );
		ipFlags->Append(m_ipKeys->GetKeywordCollection("PersonSuffixes"));

		///////////////////////////////////////////
		// Examine the leading word for a date
		// if found, will move it after the name(s)
		///////////////////////////////////////////
		long		lStartPos = -1;
		long		lEndPos = -1;
		ITokenPtr	ipToken;
		CComBSTR	bstrValue;
		ipToken = ITokenPtr( IObjectPairPtr( ipWords->At(0) )->Object1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI10448", ipToken != NULL );
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrValue );

		ISpatialStringPtr ipDate = NULL;

		// Convert token to word and convert it to a number
		string strWord = asString( bstrValue );
		long lDate = -1;
		long lDateLength = -1;
		try
		{
			lDate = asLong(strWord);
			lDateLength = strWord.length();
			ipDate = ripTrust->GetSubString(lStartPos, lEndPos + 1);
			ASSERT_RESOURCE_ALLOCATION( "ELI10453", ipDate != NULL );
		}
		catch (...)
		{
		}

		////////////////////////////////////////////
		// Examine each word looking for a last name
		// Start from the end
		////////////////////////////////////////////
		bool bFoundFlag = false;
		for (int i = lCount - 1; i > 0; i--)
		{
			// Extract the word's position in the SpatialString
			long		lStartPos = -1;
			long		lEndPos = -1;
			ITokenPtr	ipToken;
			ipToken = ITokenPtr( IObjectPairPtr( ipWords->At(i) )->Object1 );
			ASSERT_RESOURCE_ALLOCATION( "ELI19157", ipToken != NULL );
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

			// Extract the word as a Spatial String from the original
			// Get the subsequent space if this is not the last word
			if (i != lCount - 1)
			{
				lEndPos++;
			}
			ISpatialStringPtr ipSpatial = ripTrust->GetSubString( lStartPos, lEndPos );
			ASSERT_RESOURCE_ALLOCATION( "ELI10449", ipSpatial != NULL );

			// Check if this word contains a Trust Flag
			if (asCppBool(ipSpatial->ContainsStringInVector(
				ipFlags, VARIANT_FALSE, VARIANT_TRUE,  m_ipRegExprParser)))
			{
				// This is a Trust Flag and is not a Last Name, so set flag
				bFoundFlag = true;
			}
			else if (bFoundFlag)
			{
				// Already found at least one Trust Flag so this is 
				// assumed to be a last name

				// Move Date word
				if ((lDate != -1) && (lDateLength > 0) && ipDate != NULL)
				{
					// Insert Date word
					ripTrust->Insert( lStartPos, ipDate );

					// Remove leading Date
					ripTrust = ripTrust->GetSubString( lDateLength, -1 );
					ASSERT_RESOURCE_ALLOCATION("ELI26070", ripTrust != NULL);
				}

				// Get length of word
				long lWordLength = ipSpatial->Size;

				// Prepend this Last Name (with its trailing space) 
				// to the beginning of the Trust text
				ripTrust->Insert( 0, ipSpatial );

				// Trim the trailing space from the last name
				ipSpatial->Trim( _bstr_t( "" ), _bstr_t( " \r\n" ) );
				_bstr_t bstrLastName = ipSpatial->String;

				// Remove each copy of the last name from the Trust text
				long lFindPos = 0;
				while (lFindPos != -1)
				{
					// Find the last name (but not at the beginning)
					lFindPos = ripTrust->FindFirstInstanceOfString( 
						bstrLastName, lWordLength );
					
					if (lFindPos != -1)
					{
						// Remove the last name and the preceding space
						// to allow any subsequent punctuation to be retained
						ripTrust->Remove( lFindPos - 1, lFindPos + lWordLength - 2 );
					}
				}

				// No need to check remaining words
				break;
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::handleCompanySlash(ISpatialStringPtr &ripCompany)
{
	long lFound = ripCompany->FindFirstInstanceOfChar( '/', 0 );
	if (lFound != -1)
	{
		// Local copy of SpatialString for testing
		ISpatialStringPtr	ipLocal = ripCompany;
		ASSERT_RESOURCE_ALLOCATION("ELI10496", ipLocal != NULL);

		bool bDone = false;
		long lFoundLocal = lFound;
		while (!bDone)
		{
			// Check for slash as part of an Alias
			UCLID_AFUTILSLib::EAliasType	eNextType = (UCLID_AFUTILSLib::EAliasType)0;
			long		lNextAliasItem = -1;

			// Check Name for Alias
			long	lStartPos = -1;
			long	lEndPos = -1;
			findAlias( ipLocal, &lStartPos, &lEndPos, eNextType, lNextAliasItem );
			string strX = ipLocal->String;
			if (lStartPos != -1)
			{
				// Alias found, check positions
				if ((lFoundLocal >= lStartPos) && (lFoundLocal <= lEndPos))
				{
					// Alias overlaps the slash, just return
					return;
				}
				// Check for Alias past slash
				else if (lFoundLocal < lStartPos)
				{
					bDone = true;
				}
				// Check for Alias before slash
				else if (lEndPos < lFoundLocal)
				{
					// Use post-Alias string for continued testing
					ipLocal = ipLocal->GetSubString( lEndPos + 2, -1 );

					// Adjust local lFound
					lFoundLocal -= (lEndPos + 2);
				}
			}
			else
			{
				bDone = true;
			}
		}

		// Check for surrounding digits
		bool bPrevious = false;
		bool bNext = false;

		// Check previous character or next previous if a space
		if (lFound > 0)
		{
			if (isDigitChar( (unsigned char)ripCompany->GetChar( lFound - 1 ) ))
			{
				bPrevious = true;
			}
			else if (isWhitespaceChar( (unsigned char)ripCompany->GetChar( lFound - 1 )))
			{
				if (lFound > 1)
				{
					if (isDigitChar( (unsigned char)ripCompany->GetChar( lFound - 2 ) ))
					{
						bPrevious = true;
					}
				}
			}
		}

		// Check next character or next next if a space
		if (lFound < ripCompany->Size - 1)
		{
			if (isDigitChar( (unsigned char)ripCompany->GetChar( lFound + 1 ) ))
			{
				bNext = true;
			}
			else if (isWhitespaceChar( (unsigned char)ripCompany->GetChar( lFound + 1 )))
			{
				if (lFound < ripCompany->Size - 2)
				{
					if (isDigitChar( (unsigned char)ripCompany->GetChar( lFound + 2 ) ))
					{
						bNext = true;
					}
				}
			}
		}

		// Remove the slash if not surrounding digits
		if (!bPrevious || !bNext)
		{
			ripCompany->Remove( lFound, lFound );

			// Insert " AKA " in place of the slash
			ripCompany->InsertString(lFound, " AKA ");
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::findNameDelimiters(ISpatialStringPtr ipEntity, bool bSuffixFound, 
											 IIUnknownVectorPtr &ripMatches)
{
	// Clear the result vector
	ripMatches->Clear();

	///////////////////////////////////
	// Consider heirarchy of delimiters
	// Step 1A: Semicolon
	///////////////////////////////////
	processDelimiter( ipEntity, ";", ripMatches );

	// Remove "invalid" comma tokens
	validateSemicolonTokens( ipEntity, ripMatches );

	/////////////////
	// Step 1B: Slash
	/////////////////
	processDelimiter( ipEntity, "/", ripMatches );

	// Remove "invalid" slash tokens
	validateSlashTokens( ipEntity, ripMatches );

	////////////////////
	// Step 2: Ampersand
	////////////////////
	processDelimiter( ipEntity, "&", ripMatches );

	// Remove "invalid" ampersand tokens
	validateAmpersandTokens( ipEntity, "&", ripMatches );

	///////////////
	// Step 3A: And
	///////////////
	processDelimiter( ipEntity, "\\band\\b", ripMatches );

	// Remove "invalid" and tokens
	validateAmpersandTokens( ipEntity, "and", ripMatches );

	/////////////////
	// Step 3B: Et Ux
	/////////////////
	processDelimiter( ipEntity, "\\bet\\s*ux\\b", ripMatches );

	// Remove "invalid" and tokens
	validateAmpersandTokens( ipEntity, "etux", ripMatches );
	validateAmpersandTokens( ipEntity, "et ux", ripMatches );

	////////////////
	// Step 4: Comma
	////////////////
	processDelimiter( ipEntity, ",", ripMatches );

	// Remove "invalid" comma tokens
	validateCommaTokens( ipEntity, ripMatches );

	////////////////////////////////
	// Step 5: Blank Line
	//   only if no other delimiters
	////////////////////////////////
	if (ripMatches->Size() == 0)
	{
		processDelimiter( ipEntity, "\\r\\n\\r\\n", ripMatches );

		// Remove "invalid" blank-line tokens
		validateBlankLineTokens( ipEntity, ripMatches );
	}
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CEntityNameSplitter::getEntityFromDelimiters(int iEntity, 
															   ISpatialStringPtr ipGroup, 
															   IIUnknownVectorPtr ipDelimiters)
{
	// Validate parameters
	int iSize = ipDelimiters->Size();
	if ((iEntity < 0) || (iEntity > iSize))
	{
		// Throw exception
		UCLIDException ue( "ELI10327", "Invalid Entity extraction." );
		ue.addDebugInfo( "Requested Entity Index", iEntity );
		ue.addDebugInfo( "Group Size", iSize );
		throw ue;
	}

	// Create the output SpatialString
	ISpatialStringPtr ipEntity( CLSID_SpatialString );
	ASSERT_RESOURCE_ALLOCATION( "ELI10326", ipEntity != NULL );

	// Check for single Entity case
	if (iSize == 0)
	{
		ipEntity = ipGroup;
	}
	// Multiple Entities are defined
	else
	{
		// Determine which token follows desired Entity
		int iDesiredToken = iEntity;
		if (iEntity == iSize)
		{
			// Last token precedes last Entity
			iDesiredToken--;
		}

		// Extract desired token and its position in the SpatialString
		long		lStartPos = -1;
		long		lEndPos = -1;
		ITokenPtr	ipToken;
		ipToken = ITokenPtr( IObjectPairPtr( ipDelimiters->At(iDesiredToken) )->Object1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI10328", ipToken != NULL );
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

		// Initial Entity desired?
		if (iEntity == 0)
		{
			// Retrieve first Entity
			if (lStartPos > -1)
			{
				// Entity is before the first token
				ipEntity = ipGroup->GetSubString( 0, lStartPos - 1 );
			}
		}
		// Last Entity desired?
		else if (iEntity == iSize)
		{
			// Retrieve last Entity
			if (lStartPos > -1)
			{
				// Entity is after the last token
				ipEntity = ipGroup->GetSubString( lEndPos + 1, -1 );
			}
		}
		// Intermediate Entity desired
		else
		{
			// Extract preceding token and its position in the SpatialString
			long		lPreStartPos = -1;
			long		lPreEndPos = -1;
			ITokenPtr	ipPreToken;
			ipPreToken = ITokenPtr( IObjectPairPtr( ipDelimiters->At(iDesiredToken-1) )->Object1 );
			ASSERT_RESOURCE_ALLOCATION( "ELI10329", ipPreToken != NULL );
			ipPreToken->GetTokenInfo( &lPreStartPos, &lPreEndPos, NULL, NULL );

			// Entity is after ipPreToken and before ipToken
			if ((lStartPos > -1) && (lPreStartPos > -1))
			{
				ipEntity = ipGroup->GetSubString( lPreEndPos + 1, lStartPos - 1 );
			}
		}
	}

	// Provide Entity to caller
	return ipEntity;
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::processDelimiter(ISpatialStringPtr ipEntity, string strDelimiter, 
										   IIUnknownVectorPtr &ripMatches)
{
	// Create local collection for interim results
	IIUnknownVectorPtr ipResults( CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI09308", ipResults != NULL );

	// Provide delimiter to parser
	m_ipRegExprParser->Pattern = _bstr_t( strDelimiter.c_str() );

	// Step through each (if any) substring and find specified delimiter(s)
	long	lCount = ripMatches->Size();
	long	lEndOfPreviousToken = -1;
	long	lAddedTokens = 0;
	int		i;
	for (i = 0; i <= lCount; i++)
	{
		long		lStartPos = -1;
		long		lEndPos = -1;
		ITokenPtr	ipToken;
		ISpatialStringPtr	ipSubstring( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION( "ELI09309", ipSubstring != NULL );

		////////////////////
		// Get the substring
		////////////////////

		// Substring is before this token
		if ((lCount > 0) && (i < lCount))
		{
			// Get token information
			ipToken = ITokenPtr( IObjectPairPtr( ripMatches->At(i+lAddedTokens) )->Object1 );
			ASSERT_RESOURCE_ALLOCATION( "ELI09334", ipToken != NULL );
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

			if (lStartPos > 0)
			{
				// Substring is between end of previous Token and beginning of this Token
				ipSubstring = ipEntity->GetSubString( lEndOfPreviousToken + 1, lStartPos - 1 );

				// Locate any delimiters in the substring
				ipResults = m_ipRegExprParser->Find( ipSubstring->String, VARIANT_FALSE, VARIANT_FALSE );
			}
			// Ignore empty substring before leading token
		}
		// i == lCount, substring is after the last token
		else if (lCount > 0)
		{
			// Check string length
			if (ipEntity->GetSize() > lEndOfPreviousToken + 1)
			{
				// Substring is after the last token
				ipSubstring = ipEntity->GetSubString( lEndOfPreviousToken + 1, -1 );

				// Locate any delimiters in the substring
				ipResults = m_ipRegExprParser->Find( ipSubstring->String, VARIANT_FALSE, VARIANT_FALSE );
				lEndPos = ipEntity->Size - 1;
			}
			// Ignore empty substring after trailing token
			else
			{
				// Clear any previous results
				ipResults->Clear();
			}
		}		// end else processing last substring
		// lCount == 0, substring is entire string
		else
		{
			// Locate any delimiters in the entire string
			ipResults = m_ipRegExprParser->Find( ipEntity->String, VARIANT_FALSE , VARIANT_FALSE);
			lEndPos = ipEntity->Size - 1;
		}

		/////////////////////////
		// Merge delimiter tokens into original collection
		// Update start and end positions to reflect location in ipEntity
		/////////////////////////

		if (ipResults->Size() > 0)
		{
			// Update start and end positions based on position of the substring
			if (i == 0)
			{
				// Substring is before first token
				// Start and End positions do not need to be changed
				// Insert new tokens at position 0
				ripMatches->InsertVector( 0, ipResults );

				// Update counter
				lAddedTokens = ipResults->Size();
			}
			else if (i < lCount)
			{
				// Substring is before Ith token
				// Start and End positions must be updated
				updateTokenPositions( ipResults, lEndOfPreviousToken + 1 );

				// Insert new tokens before Ith original token
				ripMatches->InsertVector( i + lAddedTokens, ipResults );

				// Update counter
				lAddedTokens += ipResults->Size();
			}
			else if (i == lCount)
			{
				// Substring is after last token
				// Start and End positions must be updated
				updateTokenPositions( ipResults, lEndOfPreviousToken + 1 );

				// Insert new tokens after last token
				ripMatches->Append( ipResults );
			}
		}

		// Update the End position
		lEndOfPreviousToken = lEndPos;
	}			// end for each substring

	//////////////////////////////////////////////////////
	// Final check, remove tokens that do not isolate text
	//////////////////////////////////////////////////////
	lCount = ripMatches->Size();
	if (lCount == 0)
	{
		return;
	}

	lEndOfPreviousToken = -1;
	for (i = 0; i <= lCount; i++)
	{
		long		lStartPos = -1;
		long		lEndPos = -1;
		bool		bRemoveToken = false;
		bool		bTrailingToken = false;
		ITokenPtr	ipToken;
		ISpatialStringPtr	ipSubstring( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION( "ELI09897", ipSubstring != NULL );

		////////////////////
		// Get the substring
		////////////////////

		// Substring is before this token
		if ((lCount > 0) && (i < lCount))
		{
			// Get this ObjectPair
			IObjectPairPtr	ipObjPair( ripMatches->At( i ) );
			ASSERT_RESOURCE_ALLOCATION( "ELI12908", ipObjPair != NULL );

			// Get token information
			ipToken = ITokenPtr( ipObjPair->Object1 );
			ASSERT_RESOURCE_ALLOCATION( "ELI09898", ipToken != NULL );
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

			if (lStartPos == 0)
			{
				// Empty substring before leading token
				bRemoveToken = true;
			}
			else if (lStartPos > 0)
			{
				// Check for back-to-back Tokens
				if (lEndOfPreviousToken + 1 > lStartPos - 1)
				{
					// Substring is empty
					bRemoveToken = true;
				}
				else
				{
					// Substring is between end of previous Token and beginning of this Token
					ipSubstring = ipEntity->GetSubString( lEndOfPreviousToken + 1, lStartPos - 1 );

					// Check substring contents
					if (!containsNonWhitespaceChars( _bstr_t( ipSubstring->String ).operator const char *() ))
					{
						// Substring contains only whitespace
						bRemoveToken = true;
					}
				}
			}
		}
		// i == lCount, substring is after the last token
		else if (lCount > 0)
		{
			// Check string length
			if (ipEntity->GetSize() > lEndOfPreviousToken + 1)
			{
				// Substring is after the last token
				ipSubstring = ipEntity->GetSubString( lEndOfPreviousToken + 1, -1 );

				// Check substring contents
				if (!containsNonWhitespaceChars( _bstr_t( ipSubstring->String ).operator const char *() ))
				{
					// Substring contains only whitespace
					bRemoveToken = true;
					bTrailingToken = true;
				}
			}
			// Empty substring after trailing token
			else
			{
				// Set flags
				bRemoveToken = true;
				bTrailingToken = true;
			}
		}		// end else processing last substring
		// lCount == 0, substring is entire string

		// Update the End position
		if (lEndPos > -1)
		{
			lEndOfPreviousToken = lEndPos;
		}

		if (bRemoveToken)
		{
			// Remove this token as invalid delimiter
			ripMatches->Remove( bTrailingToken ? i-1 : i );

			// Update loop index and count to avoid missing subsequent tokens
			if (!bTrailingToken)
			{
				i--;
				lCount--;
			}
		}
	}			// end for each delimiter
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::validateAmpersandTokens(ISpatialStringPtr ipEntity, 
												  string strDelimiter, 
												  IIUnknownVectorPtr &ripMatches)
{
	// Get local copy of entire string
	string	strEntity = ipEntity->String;

	// Step through each token and evaluate words on either side of an ampersand
	long	lEndOfPreviousToken = -1;
	long		lStartPos = -1;
	long		lEndPos = -1;
	long		lStartPos2 = -1;
	long		lEndPos2 = -1;
	string		strPreviousDelimiter;
	for (int i = 0; i < ripMatches->Size(); i++)
	{
		ITokenPtr	ipToken;
		ITokenPtr	ipToken2;
		IObjectPairPtr ipMatch;
		CComBSTR	bstrValue;

		// Get token information
		ipMatch = IObjectPairPtr( ripMatches->At(i) );
		ASSERT_RESOURCE_ALLOCATION( "ELI23647", ipMatch != NULL );
		ipToken = ITokenPtr( ipMatch->Object1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI09381", ipToken != NULL );
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrValue );

		// Convert token and delimiter to upper case
		string strValue = asString( bstrValue );
		makeUpperCase( strValue );
		makeUpperCase( strDelimiter );

		// Ignore any tokens that are not the specified delimiter
		if (strValue == strDelimiter)
		{
			string	strPrevious;
			string	strNext;

			/////////////////////////////
			// Get the previous substring
			/////////////////////////////
			if (i == 0)
			{
				// Preceding substring is beginning of string
				strPrevious = strEntity.substr( 0, lStartPos );
			}
			else
			{
				// Preceding substring is in between two tokens
				// so get information about preceding token
				ipMatch = IObjectPairPtr( ripMatches->At(i-1) );
				ASSERT_RESOURCE_ALLOCATION( "ELI23648", ipMatch != NULL );
				ipToken2 = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI09382", ipToken2 != NULL );
				ipToken2->GetTokenInfo( &lStartPos2, &lEndPos2, NULL, NULL );

				// Extract the substring
				strPrevious = strEntity.substr( lEndPos2 + 1, lStartPos - lEndPos2 - 1 );
			}

			/////////////////////////
			// Get the next substring
			/////////////////////////
			if (i == ripMatches->Size() - 1)
			{
				// Next substring is end of string
				strNext = strEntity.substr( lEndPos + 1, strEntity.length() - lEndPos - 1 );
			}
			else
			{
				// Next substring is in between two tokens
				// so get information about next token
				ipMatch = IObjectPairPtr( ripMatches->At(i+1) );
				ASSERT_RESOURCE_ALLOCATION( "ELI23649", ipMatch != NULL );
				ipToken2 = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI09383", ipToken2 != NULL );
				ipToken2->GetTokenInfo( &lStartPos2, &lEndPos2, NULL, NULL );

				// Extract the substring
				strNext = strEntity.substr( lEndPos + 1, lStartPos2 - lEndPos - 1 );
			}

			// Trim whitespace from substrings
			strPrevious = trim( strPrevious, " \r\n", " \r\n" );
			strNext = trim( strNext, " \r\n", " \r\n" );

			// Convert strings to upper case
			makeUpperCase( strPrevious );
			makeUpperCase( strNext );

			// Look for TRUST in subsequent string - except at beginning
			bool	bFoundTrust = false;
			long lPos = strNext.find( "TRUST" );
			if ((lPos != string::npos) && (lPos > 3))
			{
				bFoundTrust = true;
			}

			// Trim substrings to leave just previous and next words
			lPos = strPrevious.find_last_of( ' ' );
			bool bPreviousIsSingleWord = false;
			if (lPos != string::npos)
			{
				strPrevious = strPrevious.substr( lPos + 1, strPrevious.length() - lPos - 1 );
			}
			else
			{
				bPreviousIsSingleWord = true;
			}

			lPos = strNext.find_first_of( ' ' );
			if (lPos != string::npos)
			{
				strNext = strNext.substr( 0, lPos );
			}

			// Trim whitespace from words
			strPrevious = trim( strPrevious, " \r\n", ", \r\n" );
			strNext = trim( strNext, " \r\n", ", \r\n" );

			//////////////////////////////////////
			// Check substrings for paired matches
			//////////////////////////////////////
			bool	bFoundMatch = false;
			if (!bFoundMatch && 
				(((strPrevious == "HOUSING") && (strNext == "URBAN")) || 
				((strPrevious == "WILL") && (strNext == "TESTAMENT")) || 
				((strPrevious == "BANK") && (strNext == "TRUST")) || 
				((strPrevious == "ONE") && (strNext == "THE"))))
			{
				bFoundMatch = true;
			}

			/////////////////////////////////////////
			// If subsequent phrase includes "TRUST",
			// check previous word for initial or 
			// previous phrase for single word or 
			// previous delimiter as semicolon
			/////////////////////////////////////////
			long lPreviousLength = strPrevious.length();
			if (!bFoundMatch && bFoundTrust &&
				((bPreviousIsSingleWord) || 
				(strPreviousDelimiter == ";") || 
				(lPreviousLength == 1) || 
				((lPreviousLength == 2) && (strPrevious[1] == '.'))))
			{
				bFoundMatch = true;
			}

			//////////////////////////////////////
			// Remove delimiter if match was found
			//////////////////////////////////////
			if (bFoundMatch)
			{
				// Remove this token as invalid delimiter
				ripMatches->Remove( i );

				// Update loop index to avoid missing subsequent tokens
				i--;
			}

			// Save this delimiter as previous
			strPreviousDelimiter = strValue;
		}
		else
		{
			// Save this delimiter as previous
			strPreviousDelimiter = strValue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::validateBlankLineTokens(ISpatialStringPtr ipEntity,	
												  IIUnknownVectorPtr &ripMatches)
{
	// Get local copy of entire string
	string	strEntity = ipEntity->String;

	// Step through each token and evaluate any comma
	long	lEndOfPreviousToken = -1;
	for (int i = 0; i < ripMatches->Size(); i++)
	{
		long		lStartPos = -1;
		long		lEndPos = -1;
		long		lStartPos2 = -1;
		long		lEndPos2 = -1;
		CComBSTR	bstrValue;
		ITokenPtr	ipToken;
		IObjectPairPtr ipMatch;

		// Get token information
		ipMatch = IObjectPairPtr( ripMatches->At(i) );
		ASSERT_RESOURCE_ALLOCATION( "ELI23650", ipMatch != NULL );
		ipToken = ITokenPtr( ipMatch->Object1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI10541", ipToken != NULL );
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrValue );

		string stdstrValue = asString(bstrValue);

		// Ignore any tokens that are not blank lines
		if (!containsNonWhitespaceChars( stdstrValue ))
		{
			string	strSubstring;

			//////////////////////////////////////////////////
			// Check the preceding substring for single word
			// and assume that this by itself is not an entity
			//////////////////////////////////////////////////
			if (i == 0)
			{
				// Preceding substring is beginning of string
				strSubstring = strEntity.substr( 0, lStartPos );
			}
			else
			{
				// Preceding substring is in between two tokens
				// so get information about preceding token
				ipMatch = IObjectPairPtr( ripMatches->At(i-1) );
				ASSERT_RESOURCE_ALLOCATION( "ELI23651", ipMatch != NULL );
				ITokenPtr ipToken2 = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI10542", ipToken2 != NULL );
				ipToken2->GetTokenInfo( &lStartPos2, &lEndPos2, NULL, NULL );

				// Extract the substring
				strSubstring = strEntity.substr( lEndPos2 + 1, lStartPos - lEndPos2 - 1 );
			}

			// Get count of words within this substring
			IIUnknownVectorPtr	ipWords( CLSID_IUnknownVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09338", ipWords != NULL );
			long lWordCount = getWordsFromString( strSubstring, ipWords );

			if (lWordCount == 1)
			{
				// Substring has only one word.  Assume this is an
				// invalid entity and remove this token
				ripMatches->Remove( i );

				// Update loop index to avoid missing subsequent tokens
				i--;
				continue;
			}		// end if preceding substring has one word

			/////////////////////////////////////////////////
			// Check the subsequent substring for single word
			/////////////////////////////////////////////////
			if (i == ripMatches->Size() - 1)
			{
				// Subsequent substring is end of string
				strSubstring = strEntity.substr( lEndPos + 1, strEntity.length() - lEndPos - 1 );
			}
			else
			{
				// Subsequent substring is in between two tokens
				// so get information about subsequent token
				ipMatch = IObjectPairPtr( ripMatches->At(i+1) );
				ASSERT_RESOURCE_ALLOCATION( "ELI23652", ipMatch != NULL );
				ITokenPtr ipToken2 = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI10543", ipToken2 != NULL );
				ipToken2->GetTokenInfo( &lStartPos2, &lEndPos2, NULL, NULL );

				// Extract the substring
				strSubstring = strEntity.substr( lEndPos + 1, lStartPos2 - lEndPos - 1 );
			}

			// Get count of words within this substring after trimming whitespace
			strSubstring = trim( strSubstring, " ", " " );
			lWordCount = getWordsFromString( strSubstring, ipWords );

			if (lWordCount == 1)
			{
				// Substring has only one word.  Assume this is an
				// invalid entity and remove this token
				ripMatches->Remove( i );

				// Update loop index to avoid missing subsequent tokens
				i--;
				continue;
			}		// end if next substring has one word
		}			// end if token is a blank line
	}				// end for each token
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::validateCommaTokens(ISpatialStringPtr ipEntity,
											  IIUnknownVectorPtr &ripMatches)
{
	// Get local copy of entire string
	string	strEntity = ipEntity->String;

	// Step through each token and evaluate any comma
	long	lEndOfPreviousToken = -1;
	for (int i = 0; i < ripMatches->Size(); i++)
	{
		long		lStartPos = -1;
		long		lEndPos = -1;
		long		lStartPos2 = -1;
		long		lEndPos2 = -1;
		CComBSTR	bstrValue;
		ITokenPtr	ipToken;
		IObjectPairPtr ipMatch;

		// Get token information
		ipMatch = IObjectPairPtr( ripMatches->At(i) );
		ASSERT_RESOURCE_ALLOCATION( "ELI23653", ipMatch != NULL );
		ipToken = ITokenPtr( ipMatch->Object1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI09326", ipToken != NULL );
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrValue );

		string stdstrValue = asString(bstrValue);

		// Ignore any tokens that are not commas
		if (stdstrValue == ",")
		{
			string	strSubstring;

			////////////////////////////////////////////////
			// Check the preceding substring for single word
			// and assume that this represents a last name
			////////////////////////////////////////////////
			if (i == 0)
			{
				// Preceding substring is beginning of string
				strSubstring = strEntity.substr( 0, lStartPos );
			}
			else
			{
				// Preceding substring is in between two tokens
				// so get information about preceding token
				ipMatch = IObjectPairPtr( ripMatches->At(i-1) );
				ASSERT_RESOURCE_ALLOCATION( "ELI23654", ipMatch != NULL );
				ITokenPtr ipToken2 = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI09327", ipToken2 != NULL );
				ipToken2->GetTokenInfo( &lStartPos2, &lEndPos2, NULL, NULL );

				// Extract the substring
				strSubstring = strEntity.substr( lEndPos2 + 1, lStartPos - lEndPos2 - 1 );
			}

			// Get count of words within this substring
			IIUnknownVectorPtr	ipWords( CLSID_IUnknownVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI19158", ipWords != NULL );
			long lWordCount = getWordsFromString( strSubstring, ipWords );

			if (lWordCount == 1)
			{
				// Preceding substring has only one word.  Assume this is a 
				// last name, first name situation and remove this token
				ripMatches->Remove( i );

				// Update loop index to avoid missing subsequent tokens
				i--;
				continue;
			}		// end if preceding substring has one word
			else if (lWordCount == 2)
			{
				// Check first word for Last Name Prefix (Van Buren or Mc Donald)
				_bstr_t bstrPrefixes = m_ipKeys->GetKeywordPattern( _bstr_t( "LastNamePrefixes" ) );
				m_ipRegExprParser->Pattern = bstrPrefixes;

				// Retrieve token
				ipMatch = IObjectPairPtr( ipWords->At(0) );
				ASSERT_RESOURCE_ALLOCATION( "ELI23655", ipMatch != NULL );
				ITokenPtr ipToken1 = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI09340", ipToken1 != NULL );
				bstrValue.Empty();
				ipToken1->GetTokenInfo( &lStartPos2, &lEndPos2, NULL, &bstrValue );

				if (m_ipRegExprParser->StringMatchesPattern( _bstr_t(bstrValue) ))
				{
					// Preceding substring looks like a compound last name.  Assume 
					// this is a last name, first name situation and remove this token
					ripMatches->Remove( i );

					// Update loop index to avoid missing subsequent tokens
					i--;
					continue;
				}
			}		// end else preceding substring has two words

			////////////////////////////////
			// Check the substring for Alias
			////////////////////////////////
			bool	bFoundAlias = false;
			IVariantVectorPtr ipAliasRE = 
				m_ipKeys->GetKeywordCollection( _bstr_t( "PersonAlias" ) );
			ASSERT_RESOURCE_ALLOCATION( "ELI11399", ipAliasRE != NULL );

			// Create temporary Spatial String to facilitate the search (P16 #2049)
			ISpatialStringPtr ipTemp( CLSID_SpatialString );
			ASSERT_RESOURCE_ALLOCATION( "ELI15709", ipTemp != NULL );
			ipTemp->CreateNonSpatialString(strSubstring.c_str(), "");

			// Check for an Alias pattern
			long	lAliasStart = -1;
			long	lAliasStop = -1;
			ipTemp->FindFirstItemInRegExpVector( ipAliasRE, VARIANT_FALSE, VARIANT_FALSE,
				0, m_ipRegExprParser, &lAliasStart, &lAliasStop );
			if (lAliasStart != -1 && lAliasStop != -1)
			{
				// If pattern was found near the end of the substring, 
				// remove this comma delimiter
				bool bRemoveToken = false;
				long lSubstringLength = strSubstring.length();
				if (lSubstringLength - lAliasStop < 3)
				{
					bRemoveToken = true;
				}

				if (bRemoveToken)
				{
					// Substring looks like a suffix, remove this token
					ripMatches->Remove( i );

					// Update loop index to avoid missing subsequent tokens
					i--;
					continue;
				}
			}

			////////////////////////////////////////////
			// Check the subsequent substring for suffix or Alias
			////////////////////////////////////////////
			if (i == ripMatches->Size() - 1)
			{
				// Subsequent substring is end of string
				strSubstring = strEntity.substr( lEndPos + 1, strEntity.length() - lEndPos - 1 );
			}
			else
			{
				// Subsequent substring is in between two tokens
				// so get information about subsequent token
				ipMatch = IObjectPairPtr( ripMatches->At(i+1) );
				ASSERT_RESOURCE_ALLOCATION( "ELI23656", ipMatch != NULL );
				ITokenPtr ipToken2 = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI09362", ipToken2 != NULL );
				ipToken2->GetTokenInfo( &lStartPos2, &lEndPos2, NULL, NULL );

				// Extract the substring
				strSubstring = strEntity.substr( lEndPos + 1, lStartPos2 - lEndPos - 1 );
			}

			// Get count of words within this substring after trimming whitespace
			strSubstring = trim( strSubstring, " ", " " );
			lWordCount = getWordsFromString( strSubstring, ipWords );

			if (lWordCount > 0)
			{
				// Retrieve first word
				ipMatch = IObjectPairPtr( ipWords->At(0) );
				ASSERT_RESOURCE_ALLOCATION( "ELI24215", ipMatch != NULL );
				ITokenPtr ipToken2 = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI09373", ipToken2 != NULL );
				bstrValue.Empty();
				ipToken2->GetTokenInfo( &lStartPos2, &lEndPos2, NULL, &bstrValue );

				// Check word for Suffix
				bool	bFoundSuffix = false;
				IVariantVectorPtr ipSuffixRE = m_ipKeys->PersonSuffixes;
				ASSERT_RESOURCE_ALLOCATION( "ELI09363", ipSuffixRE != NULL );

				// Check each Suffix pattern
				int j;
				for (j = 0; j < ipSuffixRE->Size; j++)
				{
					// Get this pattern
					m_ipRegExprParser->Pattern = ipSuffixRE->GetItem( j ).bstrVal;

					// Test this pattern against the substring
					if (m_ipRegExprParser->StringMatchesPattern( _bstr_t( bstrValue ) ))
					{
						bFoundSuffix = true;
						break;
					}
				}

				// Check substring for Alias
				bFoundAlias = false;
				IVariantVectorPtr ipAliasRE = 
					m_ipKeys->GetKeywordCollection( _bstr_t( "PersonAlias" ) );
				ASSERT_RESOURCE_ALLOCATION( "ELI11400", ipAliasRE != NULL );

				// Check each Alias pattern
				for (j = 0; j < ipAliasRE->Size; j++)
				{
					// Get this pattern
					m_ipRegExprParser->Pattern = ipAliasRE->GetItem( j ).bstrVal;

					// Test this pattern against the substring
					if (m_ipRegExprParser->StringMatchesPattern( _bstr_t( strSubstring.c_str() ) ))
					{
						bFoundAlias = true;
						break;
					}
				}

				if (bFoundSuffix | bFoundAlias)
				{
					// Substring looks like a suffix, remove this token
					ripMatches->Remove( i );

					// Update loop index to avoid missing subsequent tokens
					i--;
				}
			}		// end if at least one word found after comma
		}			// end if token is a comma
	}				// end for each token
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::validateSemicolonTokens(ISpatialStringPtr ipEntity, 
												  IIUnknownVectorPtr &ripMatches)
{
	// Get local copy of entire string
	string	strEntity = ipEntity->String;

	// Step through each token and evaluate any semicolon
	long	lEndOfPreviousToken = -1;
	long	lLastRemoved = -1;
	int		i;
	for (i = 0; i < ripMatches->Size(); i++)
	{
		long		lStartPos = -1;
		long		lEndPos = -1;
		long		lStartPos2 = -1;
		long		lEndPos2 = -1;
		CComBSTR	bstrValue;
		ITokenPtr	ipToken;
		IObjectPairPtr ipMatch;

		// Get token information
		ipMatch = IObjectPairPtr( ripMatches->At(i) );
		ASSERT_RESOURCE_ALLOCATION( "ELI23645", ipMatch != NULL );
		ipToken = ITokenPtr( ipMatch->Object1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI23642", ipToken != NULL );
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrValue );

		string stdstrValue = asString(bstrValue);

		// Ignore any tokens that are not semicolons
		if (stdstrValue == ";")
		{
			string	strPrevious;

			////////////////////////////////////////////////
			// Check the preceding substring for alias
			////////////////////////////////////////////////
			if (i == 0)
			{
				// Preceding substring is beginning of string
				strPrevious = strEntity.substr( 0, lStartPos );
			}
			else
			{
				// Preceding substring is in between two tokens
				// so get information about preceding token
				ipMatch = IObjectPairPtr( ripMatches->At(i-1) );
				ASSERT_RESOURCE_ALLOCATION( "ELI23646", ipMatch != NULL );
				ITokenPtr ipToken2 = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI23643", ipToken2 != NULL );
				ipToken2->GetTokenInfo( &lStartPos2, &lEndPos2, NULL, NULL );

				// Extract the substring
				strPrevious = strEntity.substr( lEndPos2 + 1, lStartPos - lEndPos2 - 1 );
			}

			// Check substring for Alias
			bool bFoundAlias = false;
			IVariantVectorPtr ipAliasRE = 
				m_ipKeys->GetKeywordCollection( _bstr_t( "PersonAlias" ) );
			ASSERT_RESOURCE_ALLOCATION( "ELI23644", ipAliasRE != NULL );

			// Check each Alias pattern
			int nCollectionSize = ipAliasRE->Size;
			for (int j = 0; j < nCollectionSize; j++)
			{
				// Get this pattern
				m_ipRegExprParser->Pattern = ipAliasRE->GetItem( j ).bstrVal;

				// Test this pattern against the substring
				if (m_ipRegExprParser->StringContainsPattern( _bstr_t( strPrevious.c_str() ) ))
				{
					bFoundAlias = true;
					break;
				}
			}

			if (bFoundAlias)
			{
				// Substring looks like a alias, remove this token
				ripMatches->Remove( i );

				// Update loop index to avoid missing subsequent tokens
				i--;
			}
		}			// end if token is a semicolon
	}				// end for each token
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::validateSlashTokens(ISpatialStringPtr ipEntity,
											  IIUnknownVectorPtr &ripMatches)
{
	// Collection of token indices to be removed
	std::vector<int>	vecInvalidTokens;

	// Get local copy of entire string
	string	strEntity = ipEntity->String;

	// Step through each token and evaluate any slash
	long	lEndOfPreviousToken = -1;
	long	lLastRemoved = -1;
	int		i;
	for (i = 0; i < ripMatches->Size(); i++)
	{
		long		lStartPos = -1;
		long		lEndPos = -1;
		long		lStartPos2 = -1;
		long		lEndPos2 = -1;
		CComBSTR	bstrValue;
		ITokenPtr	ipToken;
		IObjectPairPtr ipMatch;

		// Get token information
		ipMatch = IObjectPairPtr( ripMatches->At(i) );
		ASSERT_RESOURCE_ALLOCATION( "ELI23657", ipMatch != NULL );
		ipToken = ITokenPtr( ipMatch->Object1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI09673", ipToken != NULL );
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrValue );

		string stdstrValue = asString(bstrValue);

		// Ignore any tokens that are not slashes
		if (stdstrValue == "/")
		{
			string	strPrevious;
			string	strNext;

			////////////////////////////////////////////////
			// Check the preceding substring for single word
			// and assume that this represents:
			//  - Part of an Alias (i.e. S/B/M)
			//  - Part of a Designator (i.e. H/W)
			////////////////////////////////////////////////
			if (i == 0)
			{
				// Preceding substring is beginning of string
				strPrevious = strEntity.substr( 0, lStartPos );
			}
			else
			{
				// Preceding substring is in between two tokens
				// so get information about preceding token
				ipMatch = IObjectPairPtr( ripMatches->At(i-1) );
				ASSERT_RESOURCE_ALLOCATION( "ELI23658", ipMatch != NULL );
				ITokenPtr ipToken2 = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI09674", ipToken2 != NULL );
				ipToken2->GetTokenInfo( &lStartPos2, &lEndPos2, NULL, NULL );

				// Extract the substring
				strPrevious = strEntity.substr( lEndPos2 + 1, lStartPos - lEndPos2 - 1 );
			}

			// Get count of words within this substring
			IIUnknownVectorPtr	ipWords( CLSID_IUnknownVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09675", ipWords != NULL );
			long lWordCount = getWordsFromString( strPrevious, ipWords );

			if (lWordCount <= 1)
			{
				// This token is invalid, add this index to collection
				vecInvalidTokens.push_back( i );
				lLastRemoved = i;
			}		// end if preceding substring has one word
			else
			{
				// Retrieve last word
				ipMatch = IObjectPairPtr( ipWords->At(lWordCount-1) );
				ASSERT_RESOURCE_ALLOCATION( "ELI23659", ipMatch != NULL );
				ITokenPtr ipToken2 = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI09679", ipToken2 != NULL );
				bstrValue.Empty();
				ipToken2->GetTokenInfo( &lStartPos2, &lEndPos2, NULL, &bstrValue );

				strPrevious = asString( bstrValue );
			}

			/////////////////////////////////////////////////
			// Check the subsequent substring for single word
			/////////////////////////////////////////////////
			if (i == ripMatches->Size() - 1)
			{
				// Subsequent substring is end of string
				strNext = strEntity.substr( lEndPos + 1, strEntity.length() - lEndPos - 1 );
			}
			else
			{
				// Subsequent substring is in between two tokens
				// so get information about subsequent token
				ipMatch = IObjectPairPtr( ripMatches->At(i+1) );
				ASSERT_RESOURCE_ALLOCATION( "ELI23660", ipMatch != NULL );
				ITokenPtr ipToken2 = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI09678", ipToken2 != NULL );
				ipToken2->GetTokenInfo( &lStartPos2, &lEndPos2, NULL, NULL );

				// Extract the substring
				strNext = strEntity.substr( lEndPos + 1, lStartPos2 - lEndPos - 1 );
			}

			// Get count of words within this substring after trimming whitespace
			strNext = trim( strNext, " ", " " );
			lWordCount = getWordsFromString( strNext, ipWords );

			if (lWordCount < 2)
			{
				// This token is invalid, add this index to collection
				if (lLastRemoved != i)
				{
					vecInvalidTokens.push_back( i );
					lLastRemoved = i;
				}
			}		// end if preceding substring has one word
			else
			{
				// Retrieve first word
				ipMatch = IObjectPairPtr( ipWords->At(0) );
				ASSERT_RESOURCE_ALLOCATION( "ELI23661", ipMatch != NULL );
				ITokenPtr ipToken2 = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI09680", ipToken2 != NULL );
				bstrValue.Empty();
				ipToken2->GetTokenInfo( &lStartPos2, &lEndPos2, NULL, &bstrValue );

				strNext = asString( bstrValue );
			}

			//////////////////////////////////////////////////////
			// Check the previous and next words for "HIS" & "HER"
			//////////////////////////////////////////////////////
			makeUpperCase( strPrevious );
			strPrevious = trim( strPrevious, " ", " " );
			makeUpperCase( strNext );
			strNext = trim( strNext, " ", " " );

			if ((strPrevious == "HIS") && (strNext == "HER"))
			{
				// This token is invalid, add this index to collection
				if (lLastRemoved != i)
				{
					vecInvalidTokens.push_back( i );
					lLastRemoved = i;
				}
			}
		}			// end if token is a slash
	}				// end for each token

	// Remove each invalid token
	int iOffset = 0;
	for (unsigned int ui = 0; ui < vecInvalidTokens.size(); ui++)
	{
		// Remove this token
		ripMatches->Remove( vecInvalidTokens[ui] - iOffset );

		iOffset++;
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::updateTokenPositions(IIUnknownVectorPtr &ripMatches, long lOffset)
{
	// Check for trivial update
	if (lOffset == 0)
	{
		return;
	}

	// Step through each item and update the Start and End positions
	long		lStartPos = -1;
	long		lEndPos = -1;
	long		lCount = ripMatches->Size();
	for (int i = 0; i < lCount; i++)
	{
		// Retrieve token
		IObjectPairPtr ipMatch = IObjectPairPtr( ripMatches->At(i) );
		ASSERT_RESOURCE_ALLOCATION( "ELI23662", ipMatch != NULL );
		ITokenPtr ipToken = ITokenPtr( ipMatch->Object1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI09315", ipToken != NULL );
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

		// Update positions
		ipToken->PutStartPosition( lStartPos + lOffset );
		ipToken->PutEndPosition( lEndPos + lOffset );
	}
}
//-------------------------------------------------------------------------------------------------
long CEntityNameSplitter::getWordsFromString(string strText, IIUnknownVectorPtr &ripMatches)
{
	// Each word is composed of alpha-numeric characters, dash(-), underscore(_), 
	// period(.), forward slash(/), backward slash(\) and apostrophe(')
	m_ipRegExprParser->Pattern = _bstr_t("[A-Za-z0-9\\-_\\./\\\\']+");
	ripMatches = m_ipRegExprParser->Find( _bstr_t( strText.c_str() ), VARIANT_FALSE, VARIANT_FALSE );

	// Return the word count
	return ripMatches->Size();
}
//-------------------------------------------------------------------------------------------------
int CEntityNameSplitter::getDuplicateWordFirstIndex(IIUnknownVectorPtr ipMatches, int iFirstIndex, 
													int iLastIndex)
{
	// Define variables
	long		lStartPos = -1;
	long		lEndPos = -1;
	ITokenPtr	ipToken;
	CComBSTR	bstrWord;
	string		strWord;
	int			iFirstDuplicate = -1;
	std::map<std::string, int>	mapWordToIndex;

	// Confirm that vector of words is non-empty
	long	lSize = ipMatches->Size();
	if (lSize == 0)
	{
		// Create and throw exception
		throw UCLIDException( "ELI06506", "Cannot get words from empty vector." );
	}

	// Validate parameters
	if ((iFirstIndex < 0) || (iLastIndex >= lSize))
	{
		// Create and throw exception
		UCLIDException ue( "ELI06507", "Invalid index." );
		ue.addDebugInfo( "First Index", iFirstIndex );
		ue.addDebugInfo( "Last Index", iLastIndex );
		ue.addDebugInfo( "Size", lSize );
		throw ue;
	}

	// Step through each desired item in the vector
	for (int i = iFirstIndex; i <= iLastIndex; i++)
	{
		// Retrieve this token
		ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At(i) )->Object1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI09328", ipToken != NULL );
		bstrWord.Empty();
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrWord );

		// Retrieve the word as a string
		strWord = asString( bstrWord );

		// Trim any leading and trailing commas
		strWord = trim( strWord, ",", "," );

		// Check the map for this word
		map<std::string, int>::iterator iterMap = mapWordToIndex.find( strWord );
		if (iterMap != mapWordToIndex.end())
		{
			// Word length must be considered because duplicate initials are okay
			long lLength = strWord.length();
			if (lLength == 2)
			{
				// If last character is NOT a period, then it is a name
				if (strWord[1] != '.')
				{
					// Retrieve the index of first word
					iFirstDuplicate = (*iterMap).second;
					break;
				}
				// else it is an initial and we don't need to add it to the map
			}
			else if (lLength > 2)
			{
				// Retrieve the index of first word
				iFirstDuplicate = (*iterMap).second;
				break;
			}
			// else Length == 1 and we don't need to add it to the map
		}
		else
		{
			// Further testing for hyphenated names
			map<std::string, int>::iterator iterMap2 = mapWordToIndex.begin();
			while (iterMap2 != mapWordToIndex.end())
			{
				// Retrieve this word
				string	strMapWord = (*iterMap2).first;

				// Check for hyphen in either word
				if ((strMapWord.find( '-', 0 ) != string::npos) ||
					(strWord.find( '-', 0 ) != string::npos))
				{
					// Check for one word as subset of the other
					if ((strMapWord.find( strWord, 0 ) != string::npos) ||
						(strWord.find( strMapWord, 0 ) != string::npos))
					{
						// Retrieve the index of first word
						iFirstDuplicate = (*iterMap2).second;
						break;
					}
				}

				iterMap2++;
			}

			// Add this string to the map
			mapWordToIndex[strWord] = i;
		}
	}

	// Return result
	return iFirstDuplicate;
}
//-------------------------------------------------------------------------------------------------
int CEntityNameSplitter::getWordBreakIndex(IIUnknownVectorPtr ipMatches)
{
	// Define variables
	long		lStartPos = -1;
	long		lEndPos = -1;
	ITokenPtr	ipToken;
	CComBSTR	bstrWord;
	string		strWord;

	// Confirm that vector of words is non-empty
	long	lSize = ipMatches->Size();
	if (lSize == 0)
	{
		// Create and throw exception
		throw UCLIDException( "ELI08750", "Cannot get words from empty vector." );
	}

	// Minimum vector size is 4
	if (lSize < 4)
	{
		return -1;
	}

	// Initial split is halfway
	int		iWordBreak = (int)(lSize / 2);
	bool	bDone = false;

	while (!bDone)
	{
		// Retrieve this token
		ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At(iWordBreak - 1) )->Object1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI09329", ipToken != NULL );
		bstrWord.Empty();
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrWord );

		// Retrieve the word as a string
		strWord = asString( bstrWord );

		// Trim any leading and trailing commas
		strWord = trim( strWord, ",", "," );

		// Check word length
		bool	bWordIsInitial = true;
		long lLength = strWord.length();
		if (lLength == 2)
		{
			// If last character is not a period, then it is not an initial
			if (strWord[1] != '.')
			{
				bWordIsInitial = false;
			}
		}
		else if (lLength > 2)
		{
			bWordIsInitial = false;
		}

		// Cannot end a name with an initial
		if (bWordIsInitial)
		{
			// Advance to the next word
			if (++iWordBreak == lSize - 1)
			{
				// Cannot create a single-word second name
				// Just return -1 to indicate no split is available
				return -1;
			}
		}
		else
		{
			// Set flag
			bDone = true;
		}
	}

	// Return result
	return iWordBreak - 1;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CEntityNameSplitter::getNamesFromWords(ISpatialStringPtr ipText)
{
	// Define variables
	int		iLastWordUsed = -1;
	int		iLastCharUsed = -1;
	int		iLastCharSoFar = -1;
	bool	bNamePending = false;
	bool	bTitlePending = false;
	bool	bTrailingComma = false;

	// Get the collection of words
	IIUnknownVectorPtr ipMatches( CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI09336", ipMatches != NULL );

	////////////////////////////////////////
	// Replace alias strings with label text
	////////////////////////////////////////
	// Search for AKA items
	long	lAliasStartPos = -1;
	long	lAliasEndPos = -1;
	long	lCurrentStart = 0;
	while (true)
	{
		// Do the search
		ipText->FindFirstItemInRegExpVector( m_ipKeys->GetPersonAlias( kPersonAliasAKA ), 
			VARIANT_FALSE, VARIANT_FALSE, lCurrentStart, m_ipRegExprParser, &lAliasStartPos, &lAliasEndPos );
		if (lAliasStartPos != -1 && lAliasEndPos)
		{
			// Remove the alias
			ipText->Remove( lAliasStartPos, lAliasEndPos );

			// Insert the single-word alias
			ipText->InsertString(lAliasStartPos, m_ipKeys->GetPersonAliasLabel(kPersonAliasAKA));

			// Update start position
			if (ipText->Size > lAliasStartPos + 3)
			{
				lCurrentStart = lAliasStartPos + 3;
			}
			else
			{
				// Done searching
				break;
			}
		}
		else
		{
			// Done searching
			break;
		}
	}

	// Search for FKA items
	lCurrentStart = 0;
	while (true)
	{
		// Do the search
		ipText->FindFirstItemInRegExpVector( m_ipKeys->GetPersonAlias( kPersonAliasFKA ), 
			VARIANT_FALSE, VARIANT_FALSE, lCurrentStart, m_ipRegExprParser, &lAliasStartPos, &lAliasEndPos );
		if (lAliasStartPos != -1 && lAliasEndPos != -1)
		{
			// Remove the alias
			ipText->Remove( lAliasStartPos, lAliasEndPos );

			// Insert the single-word alias
			ipText->InsertString(lAliasStartPos, m_ipKeys->GetPersonAliasLabel(kPersonAliasFKA));

			// Update start position
			if (ipText->Size > lAliasStartPos + 3)
			{
				lCurrentStart = lAliasStartPos + 3;
			}
			else
			{
				// Done searching
				break;
			}
		}
		else
		{
			// Done searching
			break;
		}
	}

	// Search for NKA items
	lCurrentStart = 0;
	while (true)
	{
		// Do the search
		ipText->FindFirstItemInRegExpVector( m_ipKeys->GetPersonAlias( kPersonAliasNKA ), 
			VARIANT_FALSE, VARIANT_FALSE, lCurrentStart, m_ipRegExprParser, &lAliasStartPos, 
			&lAliasEndPos );
		if (lAliasStartPos != -1 && lAliasEndPos != -1)
		{
			// Remove the alias
			ipText->Remove( lAliasStartPos, lAliasEndPos );

			// Insert the single-word alias
			ipText->InsertString(lAliasStartPos, m_ipKeys->GetPersonAliasLabel(kPersonAliasNKA));

			// Update start position
			if (ipText->Size > lAliasStartPos + 3)
			{
				lCurrentStart = lAliasStartPos + 3;
			}
			else
			{
				// Done searching
				break;
			}
		}
		else
		{
			// Done searching
			break;
		}
	}

	/////////////////////////////////////////////////
	// Get local string after Alias items replacement
	/////////////////////////////////////////////////
	string strText = asString(ipText->String);
	getWordsFromString(strText, ipMatches);

	// Create the vector to hold stringized names
	IIUnknownVectorPtr	ipNames( CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI06686", ipNames != NULL );

	// Confirm that vector of words is non-empty
	long	lSize = ipMatches->Size();
	if (lSize == 0)
	{
		// Just return an empty vector - an exception here results in 
		// inaccurate counts
		return ipNames;
	}

	// Create an object for internal Keyword searching
	ISpatialStringPtr	ipWord( CLSID_SpatialString );
	ASSERT_RESOURCE_ALLOCATION( "ELI06508", ipWord != NULL );

	////////////////////////////////////////////////////////////////////////
	// Step through collection of words checking for Titles, Suffixes, Other
	////////////////////////////////////////////////////////////////////////
	long		lStartPos = -1;
	long		lEndPos = -1;
	long		lSecondDuplicateWordIndex = -1;
	ITokenPtr	ipToken;
	IObjectPairPtr ipMatch;
	CComBSTR	bstrWord;
	for (int i = 0; i < lSize; i++)
	{
		// Retrieve this token
		ipMatch = IObjectPairPtr( ipMatches->At(i) );
		ASSERT_RESOURCE_ALLOCATION( "ELI23688", ipMatch != NULL );
		ipToken = ITokenPtr( ipMatch->Object1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI09330", ipToken != NULL );
		bstrWord.Empty();
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrWord );

		string	strWord = asString( bstrWord );

		// Is the word a Title 
		ipWord->ReplaceAndDowngradeToNonSpatial(strWord.c_str());

		// Check if word contains a Person Title
		if (asCppBool(ipWord->ContainsStringInVector(
			m_ipKeys->PersonTitles, VARIANT_FALSE, VARIANT_TRUE,  m_ipRegExprParser)))
		{
			// This will begin another name, so check for pending name
			if (bNamePending)
			{
				// Build the complete name string
				ISpatialStringPtr	ipName = ipText->GetSubString( 
					iLastCharUsed + 1, iLastCharSoFar );

				// Trim leading and trailing whitespace
				ipName->Trim( _bstr_t( " " ), _bstr_t( " " ) );

				// Add the name to the vector
				ipNames->PushBack( ipName );

				// Update the last used variables
				iLastWordUsed = i - 1;
				iLastCharUsed = iLastCharSoFar;
			}

			// Set flags and continue with the next word
			bNamePending = true;
			bTitlePending = true;
			bTrailingComma = false;
			continue;
		}

		// Is the word an Alias - then treat it just like a Title
		if (asCppBool(ipWord->ContainsStringInVector(
			m_ipKeys->GetCompanyAlias( kCompanyAliasAll ), VARIANT_FALSE, VARIANT_TRUE,  
			m_ipRegExprParser)))
		{
			// This will begin another name, so check for pending name
			if (bNamePending)
			{
				// Protect against inappropriate Start & End values (P16 #2927)
				if (iLastCharUsed + 1 <= iLastCharSoFar)
				{
					// Build the complete name string
					ISpatialStringPtr	ipName = ipText->GetSubString( 
						iLastCharUsed + 1, iLastCharSoFar );

					// Trim leading and trailing whitespace
					ipName->Trim( _bstr_t( " " ), _bstr_t( " " ) );

					// Add the name to the vector
					ipNames->PushBack( ipName );

					// Update the last used variables
					iLastWordUsed = i - 1;
					iLastCharUsed = iLastCharSoFar;
				}
			}

			// Set flags and continue with the next word
			bNamePending = true;
			bTitlePending = true;
			bTrailingComma = false;
			continue;
		}

		// Is the word a Suffix
		long	lSuffixStartPos = -1;
		long	lSuffixEndPos = -1;
		ipWord->FindFirstItemInRegExpVector( m_ipKeys->PersonSuffixes, 
					VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lSuffixStartPos, &lSuffixEndPos );
		if (lSuffixStartPos != -1 && lSuffixEndPos != -1)
		{
			// Build the complete name string
			// eg. John Smith, Jr. will be stored in the ipNames
			ISpatialStringPtr	ipName = ipText->GetSubString( 
				iLastCharUsed + 1, lStartPos + lSuffixEndPos );

			// Trim leading and trailing whitespace
			ipName->Trim( _bstr_t( " " ), _bstr_t( " " ) );

			// Add the name to the vector
			ipNames->PushBack( ipName );

			// Update the last used variables
			iLastWordUsed = i;
			iLastCharUsed = lStartPos + lSuffixEndPos;

			// Clear flags and continue with the next word
			bNamePending = false;
			bTrailingComma = false;
			bTitlePending = false;
			continue;
		}

		// Check for previous trailing comma
		if (bTrailingComma)
		{
			// Since a Suffix was not found, the comma can be treated as a separator
			// and this word will be the beginning of a new name

			// Build the complete name string
			ISpatialStringPtr	ipName = ipText->GetSubString( 
				iLastCharUsed + 1, iLastCharSoFar );

			// Trim leading whitespace and trailing whitespace and comma
			ipName->Trim( _bstr_t( " " ), _bstr_t( " ," ) );

			// Add the name to the vector
			ipNames->PushBack( ipName );

			// Update the last used variables
			iLastWordUsed = i - 1;
			iLastCharUsed = iLastCharSoFar;

			// Clear the trailing comma flag
			bTrailingComma = false;

			// Set pending flag and continue with the next word
			bNamePending = true;
			continue;
		}

		// Set the pending flag
		bNamePending = true;

		// Set the last character flag
		iLastCharSoFar = lEndPos;

		// Get the word and its length
		unsigned long ulWordLength = strWord.length();

		// confirm that the length is not 0
		if (ulWordLength != 0)
		{
			// Does this word have a trailing comma
			if (strWord[ulWordLength - 1] == ',')
			{
				// Set the comma flag
				bTrailingComma = true;
			}
		}

		// Check for an unusually long name like...
		//    Mr John Smith Mary Smith  ==>  iNumWords = 5
		// OR    John Smith Mary Smith  ==>  iNumWords = 4
		int iNumWords = i - iLastWordUsed;
		if ((bTitlePending && (iNumWords > 4)) || 
			(!bTitlePending && (iNumWords > 3)))
		{
			// Check for duplicate words within this long name
			int iDuplicateWordIndex = getDuplicateWordFirstIndex( 
				ipMatches, iLastWordUsed + 1, i );
			if (iDuplicateWordIndex > -1)
			{
				// Retrieve the first copy of the duplicate word
				ipMatch = IObjectPairPtr( ipMatches->At(iDuplicateWordIndex) );
				ASSERT_RESOURCE_ALLOCATION( "ELI23687", ipMatch != NULL );
				ipToken = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI09331", ipToken != NULL );
				ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

				// Matching words were found, treat the duplicates as last names and
				// divide the pending name into two parts
				ISpatialStringPtr	ipFirst = ipText->GetSubString( 
					iLastCharUsed + 1, lEndPos );
				ASSERT_RESOURCE_ALLOCATION( "ELI09332", ipFirst != NULL );

				// Store the index of the duplicate word
				lSecondDuplicateWordIndex = i;

				// Trim leading and trailing whitespace
				ipFirst->Trim( _bstr_t( " " ), _bstr_t( " " ) );

				// Add the first name to the vector,
				// the second will be added when it is more surely complete
				ipNames->PushBack( ipFirst );

				// Update the last used variables
				iLastWordUsed = iDuplicateWordIndex;
				iLastCharUsed = lEndPos;
			}
		}
	}

	// Finished with last word, add a pending name
	if (bNamePending)
	{
		// Special handling for 6 words in entire string without other delimiters
		// i.e. JOHN H. SMITH JANE M JONES
		if ((lSize == 6) && (iLastWordUsed == -1))
		{
			////////////////////
			// Check for initial in first three words
			////////////////////
			bool bFoundEarlyInitial = false;

			// Check second word
			ipMatch = IObjectPairPtr( ipMatches->At(1) );
			ASSERT_RESOURCE_ALLOCATION( "ELI23681", ipMatch != NULL );
			ipToken = ITokenPtr( ipMatch->Object1 );
			ASSERT_RESOURCE_ALLOCATION( "ELI23682", ipToken != NULL );
			bstrWord.Empty();
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrWord );
			string strWord = asString( bstrWord );
			int nLength = strWord.length();
			if ((nLength == 1) || (nLength == 2 && strWord[1] == '.'))
			{
				// Found JOHN H SMITH or JOHN H. SMITH
				bFoundEarlyInitial = true;
			}

			////////////////////
			// Check for initial in last three words
			////////////////////
			bool bFoundLateInitial = false;

			// Check fifth word
			ipMatch = IObjectPairPtr( ipMatches->At(4) );
			ASSERT_RESOURCE_ALLOCATION( "ELI23683", ipMatch != NULL );
			ipToken = ITokenPtr( ipMatch->Object1 );
			ASSERT_RESOURCE_ALLOCATION( "ELI23684", ipToken != NULL );
			bstrWord.Empty();
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrWord );
			strWord = asString( bstrWord );
			nLength = strWord.length();
			if ((nLength == 1) || (nLength == 2 && strWord[1] == '.'))
			{
				// Found JOHN H SMITH or JOHN H. SMITH
				bFoundLateInitial = true;
			}

			// Divide string into two names if both substrings had initials
			if (bFoundEarlyInitial && bFoundLateInitial)
			{
				// Find starting position of fourth word
				ipMatch = IObjectPairPtr( ipMatches->At(3) );
				ASSERT_RESOURCE_ALLOCATION( "ELI23685", ipMatch != NULL );
				ipToken = ITokenPtr( ipMatch->Object1 );
				ASSERT_RESOURCE_ALLOCATION( "ELI23686", ipToken != NULL );
				bstrWord.Empty();
				ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrWord );

				// First name is from beginning of string to character before fourth word
				// Trim leading and trailing whitespace
				// Add the name to the vector
				ISpatialStringPtr	ipName1 = ipText->GetSubString( 
					iLastCharUsed + 1, lStartPos - 1 );
				ipName1->Trim( _bstr_t( " " ), _bstr_t( " " ) );
				ipNames->PushBack( ipName1 );

				// Second name is from beginning of fourth word to end of string
				// Trim leading and trailing whitespace
				// Add the name to the vector
				ISpatialStringPtr	ipName2 = ipText->GetSubString( 
					lStartPos, ipText->GetSize() - 1 );
				ipName2->Trim( _bstr_t( " " ), _bstr_t( " " ) );
				ipNames->PushBack( ipName2 );
			}
		}
		// Else if a second last name was found that is not the last word, 
		// add the second whole name and add the remaining words as a third name.
		else if (lSecondDuplicateWordIndex > -1 && lSize > lSecondDuplicateWordIndex + 1)
		{
			// Retrieve the second copy of the duplicate word
			ipMatch = IObjectPairPtr( ipMatches->At(lSecondDuplicateWordIndex) );
			ASSERT_RESOURCE_ALLOCATION( "ELI24014", ipMatch != NULL );
			ipToken = ITokenPtr( ipMatch->Object1 );
			ASSERT_RESOURCE_ALLOCATION( "ELI24015", ipToken != NULL );
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

			// Matching words were found, treat the duplicates as last names and
			// divide the pending name into two parts
			ISpatialStringPtr	ipSecond = ipText->GetSubString( 
				iLastCharUsed + 1, lEndPos );
			ASSERT_RESOURCE_ALLOCATION( "ELI24016", ipSecond != NULL );

			// Trim leading and trailing whitespace
			ipSecond->Trim( _bstr_t( " " ), _bstr_t( " " ) );

			// Add the second name to the vector,
			ipNames->PushBack( ipSecond );

			// Update the last used variables
			iLastWordUsed = lSecondDuplicateWordIndex;
			iLastCharUsed = lEndPos;

			// Build the final name string
			ISpatialStringPtr	ipName = ipText->GetSubString( 
				iLastCharUsed + 1, ipText->GetSize() - 1 );
			ASSERT_RESOURCE_ALLOCATION( "ELI24018", ipName != NULL );

			// Trim leading and trailing whitespace
			ipName->Trim( _bstr_t( " " ), _bstr_t( " " ) );

			// Add the name to the vector
			ipNames->PushBack( ipName );
		}
		// Else just package up the remaining words into a single name
		else
		{
			// Build the complete name string
			ISpatialStringPtr	ipName = ipText->GetSubString( 
				iLastCharUsed + 1, ipText->GetSize() - 1 );
			ASSERT_RESOURCE_ALLOCATION( "ELI24019", ipName != NULL );

			// Trim leading and trailing whitespace
			ipName->Trim( _bstr_t( " " ), _bstr_t( " " ) );

			// Add the name to the vector
			ipNames->PushBack( ipName );
		}
	}

	// Return the collection of names
	return ipNames;
}
//-------------------------------------------------------------------------------------------------
bool CEntityNameSplitter::handleAlias(ISpatialStringPtr ipEntity, ISpatialStringPtr &ipExtra, 
									  UCLID_AFUTILSLib::EAliasType& reType, long& rlAliasItem)
{
	bool	bReturn = false;
	long	lStartPos = -1;
	long	lEndPos = -1;

	// Locate the first alias
	findAlias( ipEntity, &lStartPos, &lEndPos, reType, rlAliasItem );

	// Alias found, store post-Alias portion of string in ipExtra object
	if ((lStartPos != -1) && (lEndPos != -1))
	{
		// Check for trailing Alias
		if (lEndPos + 1 >= ipEntity->Size)
		{
			// Just remove the hanging alias
			ipEntity->Remove( lStartPos, -1 );
		}
		else
		{
			// Discard the Alias text
			ipExtra = ipEntity->GetSubString( lEndPos + 1, - 1 );

			// Remove alias from first Entity
			ipEntity->Remove( lStartPos, -1 );

			// Set flag for continuation
			bReturn = true;
		}
	}

	// Trim leading and trailing spaces, dashes, commas, underscores, colons, slashes
	ipEntity->Trim( _bstr_t( " -,_:/\r\n" ), _bstr_t( " -,_:/\r\n)" ) );

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::findAlias(ISpatialStringPtr ipEntity, long* plStartPos, long* plEndPos, 
									UCLID_AFUTILSLib::EAliasType& reType, long& rlAliasItem)
{
	// Default to Alias not found for positions
	*plStartPos = -1;
	*plEndPos = -1;
	bool	bAliasFound = false;

	/////////////////////////////////////
	// Check string for each Person Alias
	/////////////////////////////////////
	long lPersonStartPos = -1;
	long lPersonEndPos = -1;

	// Find AKA
	ipEntity->FindFirstItemInRegExpVector( m_ipKeys->GetPersonAlias( kPersonAliasAKA ), 
				VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lPersonStartPos, &lPersonEndPos );
	if (lPersonStartPos != -1 && lPersonEndPos != -1)
	{
		// Preliminary storage of positions
		*plStartPos = lPersonStartPos;
		*plEndPos = lPersonEndPos;

		// Set type information
		bAliasFound = true;
		reType = kAliasPerson;
		rlAliasItem = (long)kPersonAliasAKA;
	}

	// Find FKA
	ipEntity->FindFirstItemInRegExpVector( m_ipKeys->GetPersonAlias( kPersonAliasFKA ), 
				VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lPersonStartPos, &lPersonEndPos );
	if (lPersonStartPos != -1 && lPersonEndPos != -1)
	{
		// Check for earlier position OR same position with longer string
		if ((bAliasFound && 
			((lPersonStartPos < *plStartPos) || ((lPersonStartPos == *plStartPos) && (lPersonEndPos > *plEndPos)))) || 
			!bAliasFound)
		{
			// Use these positions
			*plStartPos = lPersonStartPos;
			*plEndPos = lPersonEndPos;

			// Set type information
			bAliasFound = true;
			reType = kAliasPerson;
			rlAliasItem = (long)kPersonAliasFKA;
		}
	}

	// Find NKA
	ipEntity->FindFirstItemInRegExpVector( m_ipKeys->GetPersonAlias( kPersonAliasNKA ), 
				VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lPersonStartPos, &lPersonEndPos );
	if (lPersonStartPos != -1 && lPersonEndPos != -1)
	{
		// Check for earlier position OR same position with longer string
		if ((bAliasFound && 
			((lPersonStartPos < *plStartPos) || ((lPersonStartPos == *plStartPos) && (lPersonEndPos > *plEndPos)))) || 
			!bAliasFound)
		{
			// Use these positions
			*plStartPos = lPersonStartPos;
			*plEndPos = lPersonEndPos;

			// Set type information
			bAliasFound = true;
			reType = kAliasPerson;
			rlAliasItem = (long)kPersonAliasNKA;
		}
	}

	//////////////////////////////////////////////
	// Check string for each Company Alias
	// NOTE: CompanyAKA & CompanyFKA  & CompanyNKA 
	//       would be found above
	//////////////////////////////////////////////
	long lCompanyStartPos = -1;
	long lCompanyEndPos = -1;

	// Find DBA
	ipEntity->FindFirstItemInRegExpVector( m_ipKeys->GetCompanyAlias( kCompanyAliasDBA ), 
				VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lCompanyStartPos, &lCompanyEndPos );
	if (lCompanyStartPos != -1 && lCompanyEndPos != -1)
	{
		// Check for earlier position OR same position with longer string
		if ((bAliasFound && 
			((lCompanyStartPos < *plStartPos) || ((lCompanyStartPos == *plStartPos) && (lCompanyEndPos > *plEndPos)))) || 
			!bAliasFound)
		{
			// Use these positions
			*plStartPos = lCompanyStartPos;
			*plEndPos = lCompanyEndPos;

			// Set type information
			bAliasFound = true;
			reType = kAliasCompany;
			rlAliasItem = (long)kCompanyAliasDBA;
		}
	}

	// Find SBM
	ipEntity->FindFirstItemInRegExpVector( m_ipKeys->GetCompanyAlias( kCompanyAliasSBM ), 
				VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lCompanyStartPos, &lCompanyEndPos );
	if (lCompanyStartPos != -1 && lCompanyEndPos != -1)
	{
		// Check for earlier position OR same position with longer string
		if ((bAliasFound && 
			((lCompanyStartPos < *plStartPos) || ((lCompanyStartPos == *plStartPos) && (lCompanyEndPos > *plEndPos)))) || 
			!bAliasFound)
		{
			// Use these positions
			*plStartPos = lCompanyStartPos;
			*plEndPos = lCompanyEndPos;

			// Set type information
			bAliasFound = true;
			reType = kAliasCompany;
			rlAliasItem = (long)kCompanyAliasSBM;
		}
	}

	// Find SII
	ipEntity->FindFirstItemInRegExpVector( m_ipKeys->GetCompanyAlias( kCompanyAliasSII ), 
				VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lCompanyStartPos, &lCompanyEndPos );
	if (lCompanyStartPos != -1 && lCompanyEndPos != -1)
	{
		// Check for earlier position OR same position with longer string
		if ((bAliasFound && 
			((lCompanyStartPos < *plStartPos) || ((lCompanyStartPos == *plStartPos) && (lCompanyEndPos > *plEndPos)))) || 
			!bAliasFound)
		{
			// Use these positions
			*plStartPos = lCompanyStartPos;
			*plEndPos = lCompanyEndPos;

			// Set type information
			bAliasFound = true;
			reType = kAliasCompany;
			rlAliasItem = (long)kCompanyAliasSII;
		}
	}

	// Find BMW
	ipEntity->FindFirstItemInRegExpVector( m_ipKeys->GetCompanyAlias( kCompanyAliasBMW ), 
				VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lCompanyStartPos, &lCompanyEndPos );
	if (lCompanyStartPos != -1 && lCompanyEndPos != -1)
	{
		// Check for earlier position OR same position with longer string
		if ((bAliasFound && 
			((lCompanyStartPos < *plStartPos) || ((lCompanyStartPos == *plStartPos) && (lCompanyEndPos > *plEndPos)))) || 
			!bAliasFound)
		{
			// Use these positions
			*plStartPos = lCompanyStartPos;
			*plEndPos = lCompanyEndPos;

			// Set type information
			bAliasFound = true;
			reType = kAliasCompany;
			rlAliasItem = (long)kCompanyAliasBMW;
		}
	}

	///////////////////////////////////
	// Check string for Related Company
	///////////////////////////////////
	lCompanyStartPos = -1;
	lCompanyEndPos = -1;

	// Find Division
	ipEntity->FindFirstItemInRegExpVector( m_ipKeys->GetRelatedCompany( kRelatedCompanyDivision ), 
				VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lCompanyStartPos, &lCompanyEndPos );
	if (lCompanyStartPos != -1 && lCompanyEndPos != -1)
	{
		// Check for earlier position OR same position with longer string
		if ((bAliasFound && 
			((lCompanyStartPos < *plStartPos) || ((lCompanyStartPos == *plStartPos) && (lCompanyEndPos > *plEndPos)))) || 
			!bAliasFound)
		{
			// Use these positions
			*plStartPos = lCompanyStartPos;
			*plEndPos = lCompanyEndPos;

			// Set type information
			bAliasFound = true;
			reType = kAliasRelated;
			rlAliasItem = (long)kRelatedCompanyDivision;
		}
	}

	// Find Subdivision
	ipEntity->FindFirstItemInRegExpVector( m_ipKeys->GetRelatedCompany( kRelatedCompanySubdivision ), 
				VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lCompanyStartPos, &lCompanyEndPos );
	if (lCompanyStartPos != -1 && lCompanyEndPos != -1)
	{
		// Check for earlier position OR same position with longer string
		if ((bAliasFound && 
			((lCompanyStartPos < *plStartPos) || ((lCompanyStartPos == *plStartPos) && (lCompanyEndPos > *plEndPos)))) || 
			!bAliasFound)
		{
			// Use these positions
			*plStartPos = lCompanyStartPos;
			*plEndPos = lCompanyEndPos;

			// Set type information
			bAliasFound = true;
			reType = kAliasRelated;
			rlAliasItem = (long)kRelatedCompanySubdivision;
		}
	}

	// Find Subsidiary
	ipEntity->FindFirstItemInRegExpVector( m_ipKeys->GetRelatedCompany( kRelatedCompanySubsidiary ), 
				VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lCompanyStartPos, &lCompanyEndPos );
	if (lCompanyStartPos != -1 && lCompanyEndPos != -1)
	{
		// Check for earlier position OR same position with longer string
		if ((bAliasFound && 
			((lCompanyStartPos < *plStartPos) || ((lCompanyStartPos == *plStartPos) && (lCompanyEndPos > *plEndPos)))) || 
			!bAliasFound)
		{
			// Use these positions
			*plStartPos = lCompanyStartPos;
			*plEndPos = lCompanyEndPos;

			// Set type information
			bAliasFound = true;
			reType = kAliasRelated;
			rlAliasItem = (long)kRelatedCompanySubsidiary;
		}
	}

	// Find Branch
	ipEntity->FindFirstItemInRegExpVector( m_ipKeys->GetRelatedCompany( kRelatedCompanyBranch ), 
				VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lCompanyStartPos, &lCompanyEndPos );
	if (lCompanyStartPos != -1 && lCompanyEndPos != -1)
	{
		// Check for earlier position OR same position with longer string
		if ((bAliasFound && 
			((lCompanyStartPos < *plStartPos) || ((lCompanyStartPos == *plStartPos) && (lCompanyEndPos > *plEndPos)))) || 
			!bAliasFound)
		{
			// Use these positions
			*plStartPos = lCompanyStartPos;
			*plEndPos = lCompanyEndPos;

			// Set type information
			bAliasFound = true;
			reType = kAliasRelated;
			rlAliasItem = (long)kRelatedCompanyBranch;
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool CEntityNameSplitter::isValidEntity(ISpatialStringPtr& ripEntity, bool bIsPerson)
{
	bool bValid = true;
	bool bTrimming = true;

	// Step through trimming sequence at least once
	while (bTrimming)
	{
		bTrimming = false;

		// Trim leading / trailing semicolons, whitespace, misc.
		ripEntity->Trim( _bstr_t( " \r\n;*," ), _bstr_t( " \r\n;*," ) );

		// Trim any trailing lower case words
		trimTrailingLowerCaseWords( ripEntity );

		// Trim trailing TRUSTEE and AGREEMENT
		trimTrailingWord( ripEntity, "TRUSTEE" );
		trimTrailingWord( ripEntity, "AGREEMENT" );

		// Trim each leading phrase
		bTrimming |= doLeadingWordTrim( ripEntity, "AND" );
		bTrimming |= doLeadingWordTrim( ripEntity, "PAID BY" );
		bTrimming |= doLeadingWordTrim( ripEntity, "CO-TRUSTEES" );
		if (bIsPerson)
		{
			// Trim leading TRUSTEE except for Companies
			bTrimming |= doLeadingWordTrim( ripEntity, "TRUSTEE" );
		}
		bTrimming |= doLeadingWordTrim( ripEntity, "TRUSTEES" );

		// Check for empty Spatial String
		if (ripEntity->IsEmpty() == VARIANT_TRUE)
		{
			return false;
		}
	}

	//////////////////////////////
	// Special testing for Persons
	//////////////////////////////
	if (bIsPerson)
	{
		// Search the string for any number word
		// or InvalidPerson items (of, to, the, etc. P16 #1310)

		// Retrieve collection of Number Words
		// Add collection of Invalid Persons
		IShallowCopyablePtr ipCopier = m_ipKeys->GetKeywordCollection("NumberWords");
		ASSERT_RESOURCE_ALLOCATION("ELI26071", ipCopier != NULL);
		IVariantVectorPtr ipUnwanted = ipCopier->ShallowCopy();
		ASSERT_RESOURCE_ALLOCATION( "ELI10497", ipUnwanted != NULL );
		ipUnwanted->Append(m_ipKeys->GetKeywordCollection("InvalidPersons"));

		if (bValid && asCppBool(ripEntity->ContainsStringInVector(
			ipUnwanted, VARIANT_FALSE, VARIANT_TRUE,  m_ipRegExprParser)))
		{
			bValid = false;
		}
	}

	// Check for string too short
	if (ripEntity->Size < 3)
	{
		bValid = false;
	}

	// Check for string without upper-case chars
	// Accept a word that contains ".com" (P16 #2020)
	string strTest = asString( ripEntity->GetString() );
	long lPos = strTest.find_first_of( "ABCDEFGHIJKLMNOPQRSTUVWXYZ" );
	if (bValid && lPos == string::npos && (strTest.find( ".com" ) == string::npos))
	{
		bValid = false;
	}

	// Disallow single-word entities containing digits
	long lSpace = strTest.find( ' ' );
	long lDigit = strTest.find_first_of( "0123456789" );
	if (bValid && (lSpace == string::npos) && (lDigit != string::npos))
	{
		bValid = false;
	}

	/////////////////////////////////
	// Test invalid words and phrases
	/////////////////////////////////
	if (bValid)
	{
		// Create SpatialString for testing
		ISpatialStringPtr	ipTest( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION( "ELI10101", ipTest != NULL );

		// Temporary replacement of carriage-return characters with spaces 
		// to avoid discarding an otherwise valid entity (P16 #2048)
		string strTemp = strTest;
		replaceVariable( strTemp, "\r\n", " " );
		ipTest->CreateNonSpatialString(strTemp.c_str(), "");

		// Retrieve list of invalid entity expressions
		IVariantVectorPtr ipInvalid = m_ipKeys->GetKeywordCollection( _bstr_t( "InvalidEntities" ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI10102", ipInvalid != NULL );

		// Check for invalid pattern
		if (asCppBool(ipTest->ContainsStringInVector(
			ipInvalid, VARIANT_FALSE, VARIANT_TRUE, m_ipRegExprParser)))
		{
			bValid = false;
		}
	}

	return bValid;
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::removePersonTrimIdentifiers(ISpatialStringPtr& ripEntity, bool bIsPerson)
{
	/////////////////////////////////////////
	// Check string for an Entity Trim Phrase
	/////////////////////////////////////////
	long	lStartPos = -1;
	long	lEndPos = -1;
	ripEntity->FindFirstItemInRegExpVector( m_ipKeys->EntityTrimTrailingPhrases, 
							VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lStartPos, &lEndPos );
	if (lStartPos != -1 && lEndPos != -1)
	{
		// Trim the Phrase if not at or near beginning
		if (lStartPos > 8)
		{
			ripEntity->Remove( lStartPos, lEndPos );
		}
	}

	// Reset variables 
	lStartPos = -1;
	lEndPos = -1;

	///////////////////////////////////////////////////
	// Check string for leading Person Trim Identifiers
	///////////////////////////////////////////////////
	ripEntity->FindFirstItemInRegExpVector( m_ipKeys->PersonTrimIdentifiers, 
					VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lStartPos, &lEndPos );
	if (lStartPos != -1 && lEndPos != -1)
	{
		// Trim the Identifier if at or near beginning of string
		if (lStartPos < 3)
		{
			if (lEndPos + 1 == ripEntity->GetSize())
			{
				// Remove Identifier at end of string
				ripEntity->Remove( lStartPos, lEndPos );
			}
			else
			{
				// Remove Identifier and following whitespace
				ripEntity->Remove( lStartPos, lEndPos + 1 );
			}
		}
	}

	// Reset variables 
	lStartPos = -1;
	lEndPos = -1;

	//////////////////////////////////////////////////////////
	// Check string again for trailing Person Trim Identifiers
	//////////////////////////////////////////////////////////
	ripEntity->FindFirstItemInRegExpVector( m_ipKeys->PersonTrimIdentifiers, 
						VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lStartPos, &lEndPos );
	if (lStartPos != -1 && lEndPos != -1)
	{
		// Trim the Identifier if at or near end of string
		if (lEndPos + 3 >= ripEntity->GetSize())
		{
			ripEntity->Remove( lStartPos, -1 );
		}
		// Just remove the Identifier if it is not near the beginning of the string
		else if (lStartPos > 5)
		{
			// Also remove the subsequent space character
			ripEntity->Remove( lStartPos, lEndPos + 1 );
		}
	}

	/////////////////////////////////////
	// Trim leading and trailing stuff
	//    { -,} for persons & companies
	//    and periods for persons unless 
	//    suffix found
	/////////////////////////////////////
	
	// Reset variables 
	lStartPos = -1;
	lEndPos = -1;

	if (bIsPerson)
	{
		// Check for a suffix here
		if (asCppBool(ripEntity->ContainsStringInVector(
			m_ipKeys->PersonSuffixes, VARIANT_FALSE, VARIANT_TRUE,  m_ipRegExprParser)))
		{
			// Trim space, dash, comma
			ripEntity->Trim( _bstr_t( " -,." ), _bstr_t( " -," ) );
		}
		else
		{
			// Check for last "word" being an initial
			bool	bLastIsInitial = false;
			long lSize = ripEntity->GetSize();
			if (lSize > 2)
			{
				// Retrieve characters of interest
				long lLastChar = ripEntity->GetChar( lSize - 1 );
				long lSecondLastChar = ripEntity->GetChar( lSize - 2 );
				long lThirdLastChar = ripEntity->GetChar( lSize - 3 );

				// Check for space + non-space + trailing period
				if ((lThirdLastChar == ' ') && 
					(lSecondLastChar != ' ') && 
					(lLastChar == '.'))
				{
					// Set flag
					bLastIsInitial = true;
				}
			}

			if (bLastIsInitial)
			{
				// Trim space, dash, comma
				ripEntity->Trim( _bstr_t( " -,." ), _bstr_t( " -," ) );
			}
			else
			{
				// Trim space, dash, comma and period
				ripEntity->Trim( _bstr_t( " -,." ), _bstr_t( " -,." ) );
			}
		}
	}
	else
	{
		// Trim space, dash, comma
		ripEntity->Trim( _bstr_t( " -,." ), _bstr_t( " -," ) );
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::setTypeFromAlias(IAttributePtr ipAttr, UCLID_AFUTILSLib::EAliasType eType, 
										   long lAliasItem)
{
	// Check for meaningful Alias item
	if (lAliasItem == -1)
	{
		return;
	}

	// Get label for Type field
	switch (eType)
	{
	case (UCLID_AFUTILSLib::EAliasType)0: //eAliasTypeNone:
		// Do not modify Type field, this is not an Alias
		break;

	case (UCLID_AFUTILSLib::EAliasType)1: //eAliasTypePerson:
		{
			// Get specific type of Person Alias
			EPersonAliasType ePType = (EPersonAliasType)lAliasItem;

			// Get the appropriate label
			_bstr_t	strType = m_ipKeys->GetPersonAliasLabel( ePType );

			// Add the label to the Type field
			ipAttr->AddType( strType );
		}
		break;

	case (UCLID_AFUTILSLib::EAliasType)2: //eAliasTypeCompany:
		{
			// Get specific type of Company Alias
			ECompanyAliasType eCType = (ECompanyAliasType)lAliasItem;

			// Get the appropriate label
			_bstr_t	strType = m_ipKeys->GetCompanyAliasLabel( eCType );

			// Add the label to the Type field
			ipAttr->AddType( strType );
		}
		break;

	case (UCLID_AFUTILSLib::EAliasType)3: //eAliasTypeRelated:
		{
			// Get specific type of Related Company
			ERelatedCompanyType eRType = (ERelatedCompanyType)lAliasItem;

			// Get the appropriate label
			_bstr_t	strType = m_ipKeys->GetRelatedCompanyLabel( eRType );

			// Add the label to the Type field
			ipAttr->AddType( strType );
		}
		break;

	default:
		// Unknown Alias Type
		break;
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::trimTrailingLowerCaseWords(ISpatialStringPtr& ripEntity)
{
	// Trim leading and trailing space, dash, comma
	ripEntity->Trim( _bstr_t( " -,." ), _bstr_t( " -," ) );

	// Get local string for parsing
	string strTest = asString( ripEntity->String );
	long lLength = strTest.length();
	if (lLength == 0)
	{
		return;
	}

	IIUnknownVectorPtr ipWords( CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI09804", ipWords != NULL );

	// Get the collection of words
	long lCount = getWordsFromString( strTest, ipWords );

	// Check each word for all lower case characters
	long		lStartPos = -1;
	long		lEndPos = -1;
	ITokenPtr	ipToken;
	CComBSTR	bstrWord;
	long		lLowerCaseStart = lLength;
	for (int i = lCount - 1; i >= 0; i--)
	{
		// Retrieve this token
		ipToken = ITokenPtr( IObjectPairPtr( ipWords->At(i) )->Object1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI09805", ipToken != NULL );
		bstrWord.Empty();
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrWord );

		// Check for all lower-case characters
		string	strWord = asString( bstrWord );

		string strLower = strWord;
		makeLowerCase( strLower );

		// A word that is all digits is NOT counted as lower case
		// Accept a word that contains ".com" (P16 #2020)
		if ((strWord == strLower) && containsAlphaChar( strWord ) && 
			(strWord.find( ".com" ) == string::npos))
		{
			// Is lower case
			lLowerCaseStart = lStartPos;
		}
		else
		{
			// Not lower case, retain remaining words
			break;
		}
	}

	// Some lower case words have been trimmed
	if (lLowerCaseStart < lLength)
	{
		if (lLowerCaseStart > 0)
		{
			ripEntity = ripEntity->GetSubString( 0, lLowerCaseStart - 1 );
		}
		else
		{
			// Just retrieve an empty string
			ripEntity = ripEntity->GetSubString( 0, 0 );
		}
	}

	// Trim leading and trailing space, dash, comma
	ripEntity->Trim( _bstr_t( " -,." ), _bstr_t( " -," ) );
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::trimTrailingWord(ISpatialStringPtr& ripEntity, string strTrim)
{
	// Trim leading and trailing space, dash, comma
	ripEntity->Trim( _bstr_t( " -,." ), _bstr_t( " -," ) );

	// Get local string for parsing
	string strTest = asString( ripEntity->String );
	long lLength = strTest.length();
	if (lLength == 0)
	{
		return;
	}

	IIUnknownVectorPtr ipWords( CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI10538", ipWords != NULL );

	// Get the collection of words
	long lCount = getWordsFromString( strTest, ipWords );

	makeUpperCase( strTrim );

	// Check each word for against parameter
	long		lStartPos = -1;
	long		lEndPos = -1;
	ITokenPtr	ipToken;
	CComBSTR	bstrWord;
	long		lTrimPos = lLength;
	for (int i = lCount - 1; i >= 0; i--)
	{
		// Retrieve this token
		ipToken = ITokenPtr( IObjectPairPtr( ipWords->At(i) )->Object1 );
		ASSERT_RESOURCE_ALLOCATION( "ELI19130", ipToken != NULL );
		bstrWord.Empty();
		ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrWord );

		// Do case-insensitive word comparison
		string	strWord = asString( bstrWord );
		makeUpperCase( strWord );
		if (strWord == strTrim)
		{
			// Words match
			lTrimPos = lStartPos;
		}
		else
		{
			// No match, retain remaining words
			break;
		}
	}

	// Some words have been trimmed
	if (lTrimPos < lLength)
	{
		if (lTrimPos > 0)
		{
			ripEntity = ripEntity->GetSubString( 0, lTrimPos - 1 );
		}
		else
		{
			// Just retrieve an empty string
			ripEntity = ripEntity->GetSubString( 0, 0 );
		}
	}

	// Trim leading and trailing space, dash, comma
	ripEntity->Trim( _bstr_t( " -,." ), _bstr_t( " -," ) );
}
//-------------------------------------------------------------------------------------------------
IRegularExprParserPtr CEntityNameSplitter::getParser()
{
	try
	{
		// Get a regular expression parser
		IRegularExprParserPtr ipParser =
			m_ipMiscUtils->GetNewRegExpParserInstance("EntityNameSplitter");
		ASSERT_RESOURCE_ALLOCATION( "ELI22439", ipParser != NULL );

		return ipParser;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29459");
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitter::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI05569", "Entity Name Splitter" );
}
//-------------------------------------------------------------------------------------------------
