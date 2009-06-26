// EntityFinder.cpp : Implementation of CEntityFinder
#include "stdafx.h"
#include "AFUtils.h"
#include "EntityFinder.h"

#include <UCLIDException.h>
#include <RegistryPersistenceMgr.h>
#include <ByteStreamManipulator.h>
#include <cpputil.h>
#include <comutils.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

// constants moved to the header file so that they would be accessible to both
// EntityFinder and EntityFinder_Internal
//const string gstrAFUTILS_KEY_PATH = gstrAF_REG_ROOT_FOLDER_PATH + string("\\AFUtils");

//-------------------------------------------------------------------------------------------------
// CEntityFinder
//-------------------------------------------------------------------------------------------------
CEntityFinder::CEntityFinder()
: m_bLoggingEnabled(false)
{
	try
	{
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13036", ipMiscUtils != NULL );

		// Instantiate the Regular Expression parser
		m_ipParser = ipMiscUtils->GetNewRegExpParserInstance("EntityFinder");
		ASSERT_RESOURCE_ALLOCATION( "ELI06026", m_ipParser != NULL );

		// Instantiate the Entity Keywords object
		m_ipKeys.CreateInstance( CLSID_EntityKeywords );
		ASSERT_RESOURCE_ALLOCATION( "ELI06027", m_ipKeys != NULL );

		// Create pointer to Registry Persistence Manager
		ma_pUserCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr( HKEY_CURRENT_USER, gstrAFUTILS_KEY_PATH ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI06162", ma_pUserCfgMgr.get() != NULL );

		// Create pointer to Entity Finder settings
		ma_pEFConfigMgr = auto_ptr<EntityFinderConfigMgr>(
			new EntityFinderConfigMgr( ma_pUserCfgMgr.get(), "\\EntityFinder" ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI06163", ma_pEFConfigMgr.get() != NULL );

		// Check logging flag
		m_bLoggingEnabled = (ma_pEFConfigMgr->getLoggingEnabled() > 0) ? true : false;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI05953")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IEntityFinder,
		&IID_IAttributeModifyingRule,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IEntityFinder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::FindEntities(ISpatialString* pText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check licensing
		validateLicense();

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

		// Copy text to local SpatialString and string
		ISpatialStringPtr ipInputText( pText );
		ASSERT_RESOURCE_ALLOCATION( "ELI09399", ipInputText != NULL );
		string	strLocal = asString(ipInputText->String);

		// Check original length
		int iLength = strLocal.length();
		if (iLength == 0)
		{
			return S_OK;
		}

		// Make copy of original string for LOG
		// and for later substring extraction
		string	strOriginal( strLocal );

		///////////////////////////////////////////////////
		// Step 0A: Remove any embedded Address information
		///////////////////////////////////////////////////
		bool bFoundAddress = removeAddressText( ipInputText );

		////////////////////////////////////////////////////
		// Step 0B: Look for XXand or andXX and insert space
		////////////////////////////////////////////////////
		long lSpacePos = makeSpaceForAnd( ipInputText );
		while (lSpacePos != -1)
		{
			// Create the Spatial String
			ISpatialStringPtr	ipSpace( CLSID_SpatialString );
			ASSERT_RESOURCE_ALLOCATION( "ELI10535", ipSpace != NULL );
			ipSpace->CreateNonSpatialString(" ", "");

			// Insert the space
			ipInputText->Insert( lSpacePos, ipSpace );

			// Keep checking
			lSpacePos = makeSpaceForAnd( ipInputText );
		}

		// Make copy of current string for later substring extraction
		string	strStart = asString( ipInputText->String );

		// Update local string
		strLocal = strStart;

		//////////////////////////////////////////////////////
		// Step 1: Trim unwanted text from beginning of string
		//////////////////////////////////////////////////////
		strLocal = trimLeadingNonsense( strLocal );

		// Create local ISpatialString
		ISpatialStringPtr	ipSpatial( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION( "ELI06159", ipSpatial != NULL );

		// Final string as collection of portions
		string strFinal;

		// Substring to be checked for designator
		string strRight( strLocal.c_str() );
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
				0, m_ipParser, &lTrimStart, &lTrimEnd );
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
		bool bFoundTrust = doTrustTrimming( ipSpatial );

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
			0, m_ipParser, &lMuniStart, &lMuniEnd );
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
							VARIANT_FALSE, VARIANT_FALSE, 0, m_ipParser, &lStartPos, &lEndPos );
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
//			lPTIStop = lKeywordStop;

			// Found a designator already, define the substring AFTER the designator
			strRight = strLocal.substr( lPTIStop, strLocal.length() - lPTIStop );

			// Use the substring
			ipSpatial->ReplaceAndDowngradeToNonSpatial(strRight.c_str());
		}

		while (!bDone && !bFoundTrust)
		{
			// Search the substring for another Person Trim Identifier
			ipSpatial->FindFirstItemInRegExpVector( m_ipKeys->PersonTrimIdentifiers, 
							VARIANT_FALSE, VARIANT_FALSE, 0, m_ipParser, &lStartPos, &lEndPos );
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
			long lLCTrimPos = findFirstLowerCaseWordToTrim( strLocal, lPTIStop, false );

			// Remove appropriate text
			if (lLCTrimPos > -1)
			{
				strLocal = strLocal.substr( 0, lLCTrimPos );
			}

			// Retain previously trimmed string
//			strLocal = strFinal;

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
				m_ipKeys->PersonSuffixes, VARIANT_FALSE, VARIANT_TRUE, m_ipParser)))
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
			bFoundCompany = findCompanyEnd( ipSpatial, 0, &lSuffixStart, &lSuffixStop, 
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

		///////////////////////////////////////////
		// Step 4C: Special handling if TRUST found
		///////////////////////////////////////////
//		if (bFoundCompany)
//		{
//			strLocal = handleTrustDated( strLocal );
//		}

		////////////////////////////////////////////
		// Step 5: Intelligently trim at blank lines
		////////////////////////////////////////////
		if ((lKeywordStop == -1) || (lPTIStop == -1))
		{
			strLocal = doBlankLineTrimming( strLocal, lKeywordStop, bFoundTrust );
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
			int iTrimPos = findFirstLowerCaseWordToTrim( strLocal, iStart, bIsMunicipality );

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

		strLocal = doGeneralTrimming( strLocal, bPersonSuccess );

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
				ipResult = ipInputText->GetSubString( lTrimPos, lTrimPos + iLength - 1 );
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
		handleParentheses( ipResult );

		///////////////////////////////////////////
		// Step 9B: Trim leading and trailing stuff
		///////////////////////////////////////////

		// Replace explicit goofy characters with spaces
		ipResult->Replace("ý", " ", VARIANT_FALSE, 0, m_ipParser);

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
		ICopyableObjectPtr ipInput(ipInputText);
		ASSERT_RESOURCE_ALLOCATION("ELI25950", ipInput != NULL);
		ipInput->CopyFrom(ipResult);

		// Provide debug output
		string strResult = asString( ipResult->String );
		::convertCppStringToNormalString( strResult );
		TRACE( "EFA Output = \"%s\"\r\n", strResult.c_str() );

		/////////////////////////////////////
		// Step Last: Log results, if desired
		/////////////////////////////////////

		if (m_bLoggingEnabled)
		{
			strLocal = asString(ipInputText->String);
			logResults( strOriginal, strLocal );
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05879")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::FindEntitiesInAttributes(IIUnknownVector *pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// make sure the vec of attributes are not null
		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_ARGUMENT("ELI07081", ipAttributes != NULL);

		// go through all attributes in the vec
		long nSize = ipAttributes->Size();
		for (long n=0; n<nSize; n++)
		{
			IAttributePtr ipAttr = ipAttributes->At(n);
			// make sure the vector contains IAttribute
			if (ipAttr == NULL)
			{
				throw UCLIDException("ELI07082", "The IUnknownVector shall contain objects of IAttribute.");
			}

			// get spatial string from each attribute
			ISpatialStringPtr ipSpatialString = ipAttr->Value;
			// find proper entity from the string
			getThisAsCOMPtr()->FindEntities(ipSpatialString);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07080")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::raw_ModifyValue(IAttribute * pAttribute, IAFDocument* pOriginInput,
											IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		IAttributePtr	ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION( "ELI09297", ipAttribute != NULL );

		ISpatialStringPtr ipInputText = ipAttribute->GetValue();
		ASSERT_RESOURCE_ALLOCATION( "ELI09298", ipInputText != NULL);

		getThisAsCOMPtr()->FindEntities(ipInputText);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07064");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19569", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Find company or person(s)").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07065");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08242");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Create a new EntityFinder object
		ICopyableObjectPtr ipObjCopy(CLSID_EntityFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI08341", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07068");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_EntityFinder;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();
		
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI07642", "Unable to load newer EntityFinder." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07069");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << gnCurrentVersion;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07070");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
