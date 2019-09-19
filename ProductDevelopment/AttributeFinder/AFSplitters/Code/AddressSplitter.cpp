// AddressSplitter.cpp : Implementation of CAddressSplitter
#include "stdafx.h"
#include "AFSplitters.h"
#include "AddressSplitter.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <comutils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Version 1:
//   * Saves the version only.
// Version 2:
//   * Saves the Combined Name Address setting
// Version 3: Added CIdentifiableObject
const unsigned long gnCurrentVersion = 3;

//-------------------------------------------------------------------------------------------------
// CAddressSplitter
//-------------------------------------------------------------------------------------------------
CAddressSplitter::CAddressSplitter()
: m_bDirty(false),
  m_bFoundCity(false),
  m_bFoundState(false),
  m_bFoundZip(false),
  m_lNumRecipientLines(0),
  m_lNumAddressLines(0),
  m_ipParser(NULL),
  m_bCombinedNameAddress(false)
{
	try
	{
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13024", ipMiscUtils != __nullptr );

		// Instantiate the Entity Keywords object
		m_ipKeys.CreateInstance( CLSID_EntityKeywords );
		ASSERT_RESOURCE_ALLOCATION( "ELI07210", m_ipKeys != __nullptr );
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07167")
}

//-------------------------------------------------------------------------------------------------
// IInterfaceSupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitter::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeSplitter,
		&IID_IAddressSplitter,
		&IID_IPersistStream,
		&IID_ICopyableObject,
		&IID_ICategorizedComponent,
		&IID_ILicensedComponent,
		&IID_IIdentifiableObject
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
// IAttributeSplitter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitter::raw_SplitAttribute(IAttribute * pAttribute, IAFDocument *pAFDoc, 
												  IProgressStatus *pProgressStatus)
{
	try
	{
		// This splitter is obsolete so throw exception if this method is called
		UCLIDException ue("ELI28704", "Address splitter is obsolete.");
		throw ue;

		validateLicense();

		// Reset flags, collection and counters
		m_bFoundCity = false;
		m_bFoundState = false;
		m_bFoundZip = false;
		m_lNumRecipientLines = 0;
		m_lNumAddressLines = 0;

		if (m_ipTrailingLines != __nullptr)
		{
			m_ipTrailingLines->Clear();
		}

		// Initialize the keywords object (loads lists from component data)
		m_ipKeys->Init(pAFDoc);

		// Create local copies of specified Attribute
		// and collection of associated SubAttributes
		IAttributePtr ipMainAttribute( pAttribute );
		ASSERT_RESOURCE_ALLOCATION("ELI24892", ipMainAttribute != __nullptr);
		IIUnknownVectorPtr ipMainAttrSub = ipMainAttribute->SubAttributes;
		ASSERT_RESOURCE_ALLOCATION("ELI24893", ipMainAttrSub != __nullptr);

		// Retrieve Attribute Value text
		ISpatialStringPtr	ipAddress = pAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION( "ELI07174", ipAddress != __nullptr );

		// Find city, state and zip code sub-attributes from end of text
		long lCityStart = doCityStateZip( ipAddress, ipMainAttrSub );

		// Retrieve unparsed first part of original address
		if (lCityStart > 0)
		{
			ISpatialStringPtr	ipFirstPart = ipAddress->GetSubString( 0, lCityStart - 1 );
			ASSERT_RESOURCE_ALLOCATION( "ELI07185", ipFirstPart != __nullptr );

			// Find name and address sub-attributes from beginning of text
			doNameAddress( ipFirstPart, ipMainAttrSub );
		}

		// Process any trailing lines
		handleUnprocessedLines( ipMainAttrSub );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07168")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitter::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitter::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19562", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Z_Legacy Split an address").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07169")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitter::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		UCLID_AFSPLITTERSLib::IAddressSplitterPtr ipSource( pObject );
		ASSERT_RESOURCE_ALLOCATION("ELI08500", ipSource !=NULL);

		// Retrieve Combined Name Address setting
		m_bCombinedNameAddress = (ipSource->GetCombinedNameAddress() == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08499");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitter::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_AddressSplitter );
		ASSERT_RESOURCE_ALLOCATION( "ELI19156", ipObjCopy != __nullptr );

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07170");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitter::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_AddressSplitter;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitter::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitter::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset the member variables
		m_bCombinedNameAddress = false;

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
			UCLIDException ue( "ELI19342", "Unable to load newer AddressSplitter." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Read Combined Name Address setting
		if (nDataVersion >= 2)
		{
			dataReader >> m_bCombinedNameAddress;
		}

		if (nDataVersion >= 3)
		{
			// Load the GUID for the IIdentifiableObject interface.
			loadGUID(pStream);
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07171");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitter::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// This splitter is obsolete so throw exception if this method is called
		UCLIDException ue("ELI34191", "Address splitter is obsolete and cannot be saved.");
		throw ue;

		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;
		dataWriter << m_bCombinedNameAddress;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Save the GUID for the IIdentifiableObject interface.
		saveGUID(pStream);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07172");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitter::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IAddressSplitter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitter::get_CombinedNameAddress(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		*pVal = m_bCombinedNameAddress ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08493")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitter::put_CombinedNameAddress(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		m_bCombinedNameAddress = (newVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08494")
		
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitter::get_InstanceGUID(GUID *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = getGUID();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33555")
}

//-------------------------------------------------------------------------------------------------
// Helper methods
//-------------------------------------------------------------------------------------------------
void CAddressSplitter::addSubAttribute(ISpatialStringPtr ipText, std::string strName, 
									   long lPosition, IIUnknownVectorPtr ipSubAttr)
{
	// Create the Attribute
	IAttributePtr ipNewAttribute( CLSID_Attribute );
	ASSERT_RESOURCE_ALLOCATION( "ELI07176", ipNewAttribute != __nullptr );

	// Trim leading spaces and carriage returns and trailing
	// spaces, commas, and carriage returns from Value text
	ipText->Trim( _bstr_t( " \r\n" ), _bstr_t( "\r\n, " ) );

	// Set Name and Value
	ipNewAttribute->Name = strName.c_str();
	ipNewAttribute->Value = ipText;

	// Add to collection of sub-attributes
	if ((lPosition == -1) || (lPosition > ipSubAttr->Size()))
	{
		ipSubAttr->Insert( ipSubAttr->Size(), ipNewAttribute );
	}
	else
	{
		ipSubAttr->Insert( lPosition, ipNewAttribute );
	}
}
//-------------------------------------------------------------------------------------------------
void CAddressSplitter::divideRecipientAddress(ISpatialStringPtr ipText, 
											  IIUnknownVectorPtr ipSubAttr)
{
	long		lAddressStart = -1;
	long		lAddressEnd = -1;
	string		strWord;
	ITokenPtr	ipToken;

	// "Tokenize" the string into whole words
	m_ipParser->Pattern = "\\S+";
	IIUnknownVectorPtr ipMatches = m_ipParser->Find( ipText->String, VARIANT_FALSE, VARIANT_FALSE, VARIANT_FALSE );

	// Check word count
	long lCount = ipMatches->Size();
	if (lCount < 3)
	{
		// One or two words ===> entire string is Address
		addSubAttribute( ipText, "Address1", 0, ipSubAttr );
	}
	// Three or more words
	else
	{
		// Get last word for initial Address position
		ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( lCount - 1 ) )->Object1 );
		long lStartPos = ipToken->StartPosition;
		long lEndPos = ipToken->EndPosition;

		lAddressStart = lStartPos;
		lAddressEnd = lEndPos;

		// Loop through words from end to beginning and look 
		// for a word with digits to be treated as first Address word
		// NOTE: The last word is assumed to already be part of the Address
		bool bFinished = false;
		for (int i = lCount - 2; i >= 0; i--)
		{
			// Extract this word
			ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( i ) )->Object1 );
			lStartPos = ipToken->StartPosition;
			strWord = ipToken->Value;

			// Check if word contains digits and is not an ordinal number
			if (strWord.find_first_of( "0123456789" ) != string::npos &&
				!isOrdinalNumber(strWord))
			{
				// This is first word of Address, store new start point
				lAddressStart = lStartPos;

				// Finished processing the line
				bFinished = true;
			}
			else if (bFinished)
			{
				break;
			}
		}		// end for each word

		if (bFinished)
		{
			// Get an ISpatialString object for the Address line
			ISpatialStringPtr ipAddress = ipText->GetSubString(lAddressStart, lAddressEnd);

			// Check for Combined Names & Addresses
			bool bSecondLine = lAddressStart > 1;
			if (m_bCombinedNameAddress && bSecondLine)
			{
				// Add the SubAttribute before all other sub-attributes
				// NOTE: The following Recipient line will be labelled Address1
				addSubAttribute(ipAddress, "Address2", 0, ipSubAttr);
			}
			else
			{
				// Add the SubAttribute before all other sub-attributes
				addSubAttribute(ipAddress, "Address1", 0, ipSubAttr);
			}

			if (bSecondLine)
			{
				// Get an ISpatialString object for the Recipient line
				ISpatialStringPtr ipRecipient = ipText->GetSubString(0, lAddressStart - 1);

				// Check for Combined Names & Addresses
				if (m_bCombinedNameAddress)
				{
					// Add the SubAttribute before all other sub-attributes
					addSubAttribute(ipRecipient, "Address1", 0, ipSubAttr);
				}
				else
				{
					// Add the SubAttribute before all other sub-attributes
					addSubAttribute(ipRecipient, "Recipient1", 0, ipSubAttr);
				}
			}
		}
		else
		{
			// No digits words to indicate beginning of Address
			// Therefore entire text must be the Address
			addSubAttribute(ipText, "Address1", 0, ipSubAttr);

			// Increment counter
			m_lNumAddressLines++;

		}		// end if NOT finished processing the line
	}			// end else three or more words
}
//-------------------------------------------------------------------------------------------------
long CAddressSplitter::doCityStateZip(ISpatialStringPtr ipText, IIUnknownVectorPtr ipSubAttr)
{
	long		lLineStart = -1;
	long		lLineEnd = -1;
	long		lStartPos = -1;
	long		lEndPos = -1;
	CComBSTR	bstrValue;
	string		strTest;
	ITokenPtr	ipToken;

	// "Tokenize" the string, i.e. find all "whole lines"	
	m_ipParser->Pattern = "[^\\r\\n]+";
	IIUnknownVectorPtr ipMatches = m_ipParser->Find( ipText->String, VARIANT_FALSE, VARIANT_FALSE, VARIANT_FALSE );
	ASSERT_RESOURCE_ALLOCATION("ELI24894", ipMatches != __nullptr);

	long lLastLineFullyProcessed = ipMatches->Size();
	if (lLastLineFullyProcessed > 0)
	{
		////////////////
		// Find Zip Code
		////////////////
		while (!m_bFoundZip && lLastLineFullyProcessed > 0)
		{
			// Retrieve last line not fully processed
			ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( 
				lLastLineFullyProcessed - 1 ) )->Object1 );
			ASSERT_RESOURCE_ALLOCATION("ELI24895", ipToken != __nullptr);
			bstrValue.Empty();
			ipToken->GetTokenInfo( &lLineStart, &lLineEnd, NULL, &bstrValue );

			// Create test string and remove trailing whitespace
			strTest = asString( bstrValue );
			strTest = trim( strTest, "", " \r\n" );

			// Evaluate line for zip code
			m_bFoundZip = evaluateStringForZipCode( strTest, 
				&lStartPos, &lEndPos );
			if (m_bFoundZip)
			{
				// Check if this is at the beginning of a line and we are on the first
				// line of the address.  If this is the case then this is most likely
				// not a zip code but rather a 5 digit house number, set the last line
				// counter back to the last line of the address (ipMatches->Size()) and
				// move to the state finding - do not add a zip code sub-attribute
				// Get zip code as Spatial String
				if (lStartPos == 0 && lLastLineFullyProcessed == 1)
				{
					// Reset lines to search back to beginning
					lLastLineFullyProcessed = ipMatches->Size();

					// Set up the test string to search for state
					// Retrieve last line not fully processed
					ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( 
						lLastLineFullyProcessed - 1 ) )->Object1 );
					ASSERT_RESOURCE_ALLOCATION("ELI24906", ipToken != __nullptr);
					bstrValue.Empty();
					ipToken->GetTokenInfo( &lLineStart, &lLineEnd, NULL, &bstrValue );

					// Update test string to be ready for State
					strTest = asString( bstrValue );

					// Remove trailing whitespace
					strTest = trim( strTest.c_str(), "", " \r\n" );
				}
				else
				{
					ISpatialStringPtr	ipZip = ipText->GetSubString( 
						lLineStart + lStartPos, lLineStart + lEndPos );
					ASSERT_RESOURCE_ALLOCATION("ELI24896", ipZip != __nullptr);

					// Add the SubAttribute
					addSubAttribute( ipZip, "ZipCode", -1, ipSubAttr );

					// Check to see if state might also be on this line
					if (lStartPos == 0)
					{
						// Finished with this line, adjust counter
						lLastLineFullyProcessed--;

						// Retrieve last line not fully processed
						ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( 
							lLastLineFullyProcessed - 1 ) )->Object1 );
						ASSERT_RESOURCE_ALLOCATION("ELI24897", ipToken != __nullptr);
						bstrValue.Empty();
						ipToken->GetTokenInfo( &lLineStart, &lLineEnd, NULL, &bstrValue );

						// Update test string to be ready for State
						strTest = asString( bstrValue );

						// Remove trailing whitespace
						strTest = trim( strTest.c_str(), "", " \r\n" );

					}	// end if zip code finishes this line
					else
					{
						// Trim the zip code from the test string
						strTest = strTest.substr( 0, lStartPos );

						// Remove trailing whitespace
						strTest = trim( strTest.c_str(), "", " \r\n" );
					}
				}
			}
			else
			{
				// Adjust counter so as to process previous line
				lLastLineFullyProcessed--;

				if (lLastLineFullyProcessed > 0)
				{
					// Store the unprocessed line
					ISpatialStringPtr	ipExtra = ipText->GetSubString( lLineStart, lLineEnd );
					ASSERT_RESOURCE_ALLOCATION("ELI24898", ipExtra != __nullptr);
					storeUnprocessedLine( ipExtra );

					// Retrieve last line not fully processed
					ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( 
						lLastLineFullyProcessed - 1 ) )->Object1 );
					ASSERT_RESOURCE_ALLOCATION("ELI24899", ipToken != __nullptr);
					bstrValue.Empty();
					ipToken->GetTokenInfo( &lLineStart, &lLineEnd, NULL, &bstrValue );

					// Update test string to be ready for State
					strTest = asString( bstrValue );

					// Remove trailing whitespace
					strTest = trim( strTest.c_str(), "", " \r\n" );
				}	// end if this wasn't the last line
				else
				{
					// Revert to previous line to locate City and State
					lLastLineFullyProcessed++;

					// Set Zip Code flag to exit this loop
					m_bFoundZip = true;

				}	// end else last line and no zip code found
			}		// end else Zip Code not found on this line
		}			// end while Zip Code not found

		/////////////
		// Find State
		/////////////

		// Test string has been prepared for this search
		while (!m_bFoundState && lLastLineFullyProcessed > 0)
		{
			// Evaluate test string for state
			m_bFoundState = evaluateStringForState( strTest, 
				&lStartPos, &lEndPos );
			if (m_bFoundState)
			{
				// Get state as Spatial String
				ISpatialStringPtr	ipState = ipText->GetSubString( 
					lLineStart + lStartPos, lLineStart + lEndPos );
				ASSERT_RESOURCE_ALLOCATION("ELI24900", ipState != __nullptr);

				// Make state code upper case
				if (lEndPos == lStartPos + 1)
				{
					// This is a state code, convert to upper case
					ipState->ToUpperCase();
				}

				// Add the SubAttribute
				addSubAttribute( ipState, "State", 0, ipSubAttr );

				// Check to see if city might also be on this line
				if (lStartPos == 0)
				{
					// Finished with this line, adjust counter
					lLastLineFullyProcessed--;

					// Retrieve last line not fully processed
					if (lLastLineFullyProcessed > 0)
					{
						ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( 
							lLastLineFullyProcessed - 1 ) )->Object1 );
						ASSERT_RESOURCE_ALLOCATION("ELI24901", ipToken != __nullptr);
						bstrValue.Empty();
						ipToken->GetTokenInfo( &lLineStart, &lLineEnd, NULL, &bstrValue );

						// Update test string to be ready for City
						strTest = asString( bstrValue );

						// Remove trailing whitespace
						strTest = trim( strTest.c_str(), "", " \r\n" );
					}

				}	// end if city finishes this line
				else
				{
					// Trim the city from the test string
					strTest = strTest.substr( 0, lStartPos );

					// Remove trailing whitespace
					strTest = trim( strTest.c_str(), "", " \r\n" );
				}
			}
			else
			{
				// Adjust counter so as to process previous line
				lLastLineFullyProcessed--;

				// Retrieve last line not fully processed
				if (lLastLineFullyProcessed > 0)
				{
					ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( 
						lLastLineFullyProcessed - 1 ) )->Object1 );
					ASSERT_RESOURCE_ALLOCATION("ELI24902", ipToken != __nullptr);
					bstrValue.Empty();
					ipToken->GetTokenInfo( &lLineStart, &lLineEnd, NULL, &bstrValue );

					// Update test string to be ready for State
					strTest = asString( bstrValue );

					// Remove trailing whitespace
					strTest = trim( strTest.c_str(), "", " \r\n" );
				}	// end if this wasn't the last line
				else
				{
					// Revert to previous line to locate City
					lLastLineFullyProcessed++;

					// Set State flag to exit this loop
					m_bFoundState = true;

				}	// end else last line and no State found
			}		// end else State not found in this test string
		}			// end while State not found

		////////////
		// Find City
		////////////

		// Test string has been prepared for this search
		while (!m_bFoundCity && lLastLineFullyProcessed > 0)
		{
			// Evaluate test string for city
			m_bFoundCity = evaluateStringForCity( strTest, 
				&lStartPos, &lEndPos );
			if (m_bFoundCity)
			{
				// Get city as Spatial String
				ISpatialStringPtr	ipCity = ipText->GetSubString( 
					lLineStart + lStartPos, lLineStart + lEndPos );
				ASSERT_RESOURCE_ALLOCATION("ELI24903", ipCity != __nullptr);

				// Remove any trailing punctuation
				ipCity->Trim( _bstr_t(""), _bstr_t( ",." ) );

				// Add the SubAttribute
				addSubAttribute( ipCity, "City", 0, ipSubAttr );

				// Return the position of earliest processed character
				return (lLineStart + lStartPos);
			}
			else
			{
				// Adjust counter so as to process previous line
				lLastLineFullyProcessed--;

				// Retrieve last line not fully processed
				if (lLastLineFullyProcessed > 0)
				{
					ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( 
						lLastLineFullyProcessed - 1 ) )->Object1 );
					ASSERT_RESOURCE_ALLOCATION("ELI24904", ipToken != __nullptr);
					bstrValue.Empty();
					ipToken->GetTokenInfo( &lLineStart, &lLineEnd, NULL, &bstrValue );

					// Update test string
					strTest = asString( bstrValue );

					// Remove trailing whitespace
					strTest = trim( strTest.c_str(), "", " \r\n" );
				}
			}		// end else City not found in this test string
		}			// end while City not found
	}				// end if line count > 0

	// No lines found
	return -1;
}
//-------------------------------------------------------------------------------------------------
void CAddressSplitter::doNameAddress(ISpatialStringPtr ipText, IIUnknownVectorPtr ipSubAttr)
{
	// Create SpatialString object for pattern searches
	ISpatialStringPtr	ipLine( CLSID_SpatialString );
	ASSERT_RESOURCE_ALLOCATION( "ELI07175", ipLine != __nullptr );

	long		lLineStart = -1;
	long		lLineEnd = -1;
	long		lStartPos1 = -1;
	long		lEndPos1 = -1;
	long		lStartPos2 = -1;
	long		lEndPos2 = -1;
	CComBSTR	bstrValue;
	string		strTest;
	ITokenPtr	ipToken;

	// Initialize counters
	long	lLastLineFullyProcessed = -1;
	bool	bPreviousRecipient = true;

	// "Tokenize" the string, i.e. find all "whole lines"	
	m_ipParser->Pattern = "[^\\r\\n]+";
	IIUnknownVectorPtr ipMatches = m_ipParser->Find( ipText->String, VARIANT_FALSE, VARIANT_FALSE, VARIANT_FALSE );

	if (ipMatches->Size() == 1)
	{
		/////////////////////
		// Divide single line into Recipient and Address portions
		/////////////////////
		divideRecipientAddress( ipText, ipSubAttr );
	}
	else if (ipMatches->Size() > 1)
	{
		////////////////////
		// Process each line
		////////////////////
		while (lLastLineFullyProcessed < ipMatches->Size() - 1)
		{
			// Retrieve first line not fully processed
			ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( 
				lLastLineFullyProcessed + 1 ) )->Object1 );
			bstrValue.Empty();
			ipToken->GetTokenInfo( &lLineStart, &lLineEnd, NULL, &bstrValue );

			// Create test string and remove trailing whitespace
			strTest = asString( bstrValue );
			strTest = trim( strTest.c_str(), "", " \r\n" );

			//////////////////////////////
			// Evaluate line for recipient
			//////////////////////////////
			long lRecipientMeasure = evaluateStringForRecipient( strTest, 
				&lStartPos1, &lEndPos1 );

			///////////////////////////////////
			// Evaluate line for street address
			///////////////////////////////////
			long lAddressMeasure = evaluateStringForAddress( strTest, 
				&lStartPos2, &lEndPos2 );

			// Handle recipient case
			if (lRecipientMeasure > lAddressMeasure)
			{
				// Increment line counter
				m_lNumRecipientLines++;

				// Get this line as Spatial String
				ipLine = ipText->GetSubString( 
					lLineStart + lStartPos1, lLineStart + lEndPos1 );

				// Remove any trailing punctuation
				ipLine->Trim( _bstr_t(""), _bstr_t( "," ) );

				// Check if the SubAttribute should be labelled as an Address line
				CString	zLabel;
				if (m_bCombinedNameAddress)
				{
					// Create Name string for Attribute
					zLabel.Format( "Address%d", m_lNumRecipientLines + m_lNumAddressLines );

					// Add the SubAttribute
					// after all recipient lines and after any previous address lines
					addSubAttribute( ipLine, zLabel.operator LPCTSTR(), 
						m_lNumRecipientLines + m_lNumAddressLines - 1, ipSubAttr );
				}
				// No, the SubAttribute should be labelled as a Recipient line
				else
				{
					// Create Name string for Attribute
					zLabel.Format( "Recipient%d", m_lNumRecipientLines );

					// Add the SubAttribute
					// before all address lines and after any previous recipient lines
					addSubAttribute( ipLine, zLabel.operator LPCTSTR(), 
						m_lNumRecipientLines - 1, ipSubAttr );
				}

				// Update counter and flag
				lLastLineFullyProcessed++;
				bPreviousRecipient = true;
			}
			// Handle address case and default a non-zero tie to an address line
			else if ((lAddressMeasure >= lRecipientMeasure) && (lAddressMeasure != 0))
			{
				// Increment line counter
				m_lNumAddressLines++;
			
				// Get this line as Spatial String
				ipLine = ipText->GetSubString( 
					lLineStart + lStartPos2, lLineStart + lEndPos2 );

				// Remove any trailing punctuation
				ipLine->Trim( _bstr_t(""), _bstr_t( "," ) );

				// Check for Combined Names & Addresses
				CString	zLabel;
				if (m_bCombinedNameAddress)
				{
					zLabel.Format( "Address%d", m_lNumAddressLines + m_lNumRecipientLines );
				}
				else
				{
					zLabel.Format( "Address%d", m_lNumAddressLines );
				}

				// Add SubAttribute
				// after all recipient lines and after any previous address lines
				addSubAttribute( ipLine, zLabel.operator LPCTSTR(), 
					m_lNumRecipientLines + m_lNumAddressLines - 1, ipSubAttr );

				// Update counter and flag
				lLastLineFullyProcessed++;
				bPreviousRecipient = false;
			}
			/////////////////////////
			// Handle uncertain cases
			/////////////////////////
			else
			{
				/////////////////////////////////////////////
				// Case: Trim garbage lines with length <= 2
				//       AND No name info AND No address info
				/////////////////////////////////////////////
				if ((strTest.length() < 3) && 
					(lRecipientMeasure == 0) && 
					(lAddressMeasure == 0))
				{
					// Update counter
					lLastLineFullyProcessed++;

				}		// end if garbage line
				/////////////////////////////////////////////
				// Case: Last line to be processed and 
				//       no recipient AND / OR no address yet
				/////////////////////////////////////////////
				else if ((lLastLineFullyProcessed + 1 == ipMatches->Size() - 1) &&
					((m_lNumRecipientLines == 0) || (m_lNumAddressLines == 0)))
				{
					// No recipient lines
					if ((m_lNumRecipientLines == 0) && (m_lNumAddressLines > 0))
					{
						// Increment counter
						m_lNumRecipientLines++;

						// Get this line as Spatial String
						ipLine = ipText->GetSubString( 
							lLineStart + lStartPos1, lLineStart + lEndPos1 );

						// Remove any trailing punctuation
						ipLine->Trim( _bstr_t(""), _bstr_t( "," ) );

						// Check if the SubAttribute should be labelled as an Address line
						CString	zLabel;
						if (m_bCombinedNameAddress)
						{
							// Handle this line as another address line
							zLabel.Format( "Address%d", m_lNumRecipientLines + m_lNumAddressLines );

							// Add SubAttribute
							// after all recipient lines and after any previous address lines
							addSubAttribute( ipLine, zLabel.operator LPCTSTR(), 
								m_lNumRecipientLines + m_lNumAddressLines - 1, ipSubAttr );
						}
						else
						{
							// Handle this line as a recipient line
							zLabel.Format( "Recipient1" );

							// Add SubAttribute
							// before all address lines and after any previous recipient lines
							addSubAttribute( ipLine, zLabel.operator LPCTSTR(), 
								0, ipSubAttr );
						}

						// Update flag
						bPreviousRecipient = true;
					}
					// No address lines
					else if ((m_lNumRecipientLines > 0) && (m_lNumAddressLines == 0))
					{
						// Increment counter
						m_lNumAddressLines++;

						// Get this line as Spatial String
						ipLine = ipText->GetSubString( 
							lLineStart + lStartPos2, lLineStart + lEndPos2 );

						// Remove any trailing punctuation
						ipLine->Trim( _bstr_t(""), _bstr_t( "," ) );

						// Check for Combined Names & Addresses
						CString	zLabel;
						if (m_bCombinedNameAddress)
						{
							zLabel.Format( "Address%d", m_lNumAddressLines + m_lNumRecipientLines );
						}
						else
						{
							zLabel.Format( "Address1" );
						}

						// Add SubAttribute
						// after all recipient lines and after any previous address lines
						addSubAttribute( ipLine, zLabel.operator LPCTSTR(), 
							m_lNumRecipientLines + m_lNumAddressLines - 1, ipSubAttr );

						// Update flag
						bPreviousRecipient = false;
					}
					// No address lines AND no recipient lines
					else if ((m_lNumRecipientLines == 0) && (m_lNumAddressLines == 0))
					{
						// Increment counter
						m_lNumAddressLines++;

						// Get this line as Spatial String
						ipLine = ipText->GetSubString( 
							lLineStart + lStartPos2, lLineStart + lEndPos2 );

						// Remove any trailing punctuation
						ipLine->Trim( _bstr_t(""), _bstr_t( "," ) );

						// Check for Combined Names & Addresses
						CString	zLabel;
						if (m_bCombinedNameAddress)
						{
							zLabel.Format( "Address%d", m_lNumAddressLines + m_lNumRecipientLines );
						}
						else
						{
							zLabel.Format( "Address1" );
						}

						// Add SubAttribute
						// after all recipient lines and after any previous address lines
						addSubAttribute( ipLine, zLabel.operator LPCTSTR(), 
							m_lNumRecipientLines + m_lNumAddressLines - 1, ipSubAttr );

						// Update flag
						bPreviousRecipient = false;
					}

					// Update counter
					lLastLineFullyProcessed++;

				}		// end else processing last line
				///////////////////////////////////
				// Case: First line to be processed
				///////////////////////////////////
				else if (lLastLineFullyProcessed == -1)
				{
					// Increment counter
					m_lNumRecipientLines++;

					// Get this line as Spatial String
					ipLine = ipText->GetSubString( 
						lLineStart + lStartPos1, lLineStart + lEndPos1 );

					// Remove any trailing punctuation
					ipLine->Trim( _bstr_t(""), _bstr_t( "," ) );

					// Check if the SubAttribute should be labelled as an Address line
					CString	zLabel;
					if (m_bCombinedNameAddress)
					{
						// Handle this line as another address line
						zLabel.Format( "Address%d", m_lNumRecipientLines + m_lNumAddressLines );

						// Add SubAttribute
						// after all recipient lines and after any previous address lines
						addSubAttribute( ipLine, zLabel.operator LPCTSTR(), 
							m_lNumRecipientLines + m_lNumAddressLines - 1, ipSubAttr );
					}
					else
					{
						// Handle this line as a recipient line
						zLabel.Format( "Recipient%d", m_lNumRecipientLines );

						// Add SubAttribute
						// before all address lines and after any previous recipient lines
						addSubAttribute( ipLine, zLabel.operator LPCTSTR(), 
							m_lNumRecipientLines - 1, ipSubAttr );
					}

					// Update counter and flag
					lLastLineFullyProcessed++;
					bPreviousRecipient = true;
				}
				////////////////////////////////////
				// Case: No Name AND No Address info
				//       but not the first line
				////////////////////////////////////
				else if ((lRecipientMeasure == 0) && (lAddressMeasure == 0))
				{
					// Was previous line a Recipient?
					if (bPreviousRecipient)
					{
						// Increment counter
						m_lNumRecipientLines++;

						// Get this line as Spatial String
						ipLine = ipText->GetSubString( 
							lLineStart + lStartPos1, lLineStart + lEndPos1 );

						// Remove any trailing punctuation
						ipLine->Trim( _bstr_t(""), _bstr_t( "," ) );

						// Check if the SubAttribute should be labelled as an Address line
						CString	zLabel;
						if (m_bCombinedNameAddress)
						{
							// Handle this line as another address line
							zLabel.Format( "Address%d", m_lNumRecipientLines + m_lNumAddressLines );

							// Add SubAttribute
							// after all recipient lines and after any previous address lines
							addSubAttribute( ipLine, zLabel.operator LPCTSTR(), 
								m_lNumRecipientLines + m_lNumAddressLines - 1, ipSubAttr );
						}
						else
						{
							// Handle this line as a recipient line
							CString	zLabel;
							zLabel.Format( "Recipient%d", m_lNumRecipientLines );

							// Add SubAttribute
							// before all address lines and after any previous recipient lines
							addSubAttribute( ipLine, zLabel.operator LPCTSTR(), 
								m_lNumRecipientLines - 1, ipSubAttr );
						}
					}
					// Else previous line was an Address
					else
					{
						// Increment counter
						m_lNumAddressLines++;

						// Get this line as Spatial String
						ipLine = ipText->GetSubString( 
							lLineStart + lStartPos2, lLineStart + lEndPos2 );

						// Remove any trailing punctuation
						ipLine->Trim( _bstr_t(""), _bstr_t( "," ) );

						// Check for Combined Names & Addresses
						CString	zLabel;
						if (m_bCombinedNameAddress)
						{
							zLabel.Format( "Address%d", m_lNumAddressLines + m_lNumRecipientLines );
						}
						else
						{
							zLabel.Format( "Address%d", m_lNumAddressLines );
						}

						// Add SubAttribute
						// after all recipient lines and after any previous address lines
						addSubAttribute( ipLine, zLabel.operator LPCTSTR(), 
							m_lNumRecipientLines + m_lNumAddressLines - 1, ipSubAttr );
					}

					// Update counter
					lLastLineFullyProcessed++;

				}		// end else both scores = 0 but not first line
				else
				{
					// Update counter
					lLastLineFullyProcessed++;

				}		// end else not last line, but not already processed
			}			// end else uncertain if recipient or address line
		}				// end while still processing lines
	}					// end if at least one line to be processed
}
//-------------------------------------------------------------------------------------------------
long CAddressSplitter::evaluateStringForAddress(std::string strTest, long *plStartPos, 
												long *plEndPos )
{
	// Initialize counter
	long		lCount = 0;

	long		lStartPos = -1;
	long		lEndPos = -1;
	CComBSTR	bstrValue;
	string		strWord;
	ITokenPtr	ipToken;

	// If string is empty, just return [FlexIDSCore #3576]
	if (strTest.empty())
	{
		return lCount;
	}

	// Create ISpatialString object for searches
	ISpatialStringPtr	ipLine( CLSID_SpatialString );
	ASSERT_RESOURCE_ALLOCATION( "ELI07209", ipLine != __nullptr );
	ipLine->CreateNonSpatialString(strTest.c_str(), "");

	///////////////////////////////////////////
	// Check for digits contained in first word
	///////////////////////////////////////////

	// "Tokenize" the string, i.e. find the first "word"	
	m_ipParser->Pattern = "\\S+";
	IIUnknownVectorPtr ipMatches = m_ipParser->Find( ipLine->String, VARIANT_TRUE, VARIANT_FALSE, VARIANT_FALSE );

	// If no match found just return [FlexIDSCore #3576]
	if (ipMatches->Size() == 0)
	{
		return lCount;
	}

	// Extract the token as a string
	ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( 0 ) )->Object1 );
	ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrValue );
	strWord = asString( bstrValue );

	// Check word for any digits
	if (strWord.find_first_of( "0123456789" ) != string::npos)
	{
		// Increment counter
		lCount++;
	}

	///////////////////////////////////////////
	// Check for Post Office Box and variations
	///////////////////////////////////////////

	// Define pattern for PO Box
	// Note that "Box" is optional and not included in the pattern
	m_ipParser->Pattern = "\\bP(\\s*(\\.\\s*)?)?O\\b(\\s*(\\.\\s*)?)?";
	ipMatches = m_ipParser->Find( ipLine->String, VARIANT_TRUE, VARIANT_FALSE, VARIANT_FALSE );

	// Check count of matches
	if (ipMatches->Size() > 0)
	{
		// Increment counter
		lCount++;
	}

	//////////////////////////////////////
	// Check for street type names (i.e. Avenue, Court, Road, Street, etc.)
	// Check for street type abbreviations (i.e. Ave, Ct, Rd, St, etc.)
	//////////////////////////////////////
	bool	bFound = false;

	// Search the string for a Street Name
	ipLine->FindFirstItemInRegExpVector(m_ipKeys->StreetNames, VARIANT_FALSE, VARIANT_FALSE,
		0, m_ipParser, &lStartPos, &lEndPos);
	if (lStartPos != -1 && lEndPos != -1)
	{
		// Set the flag
		bFound = true;
	}
	else
	{
		// Search the string for a Street Abbreviation
		ipLine->FindFirstItemInRegExpVector( m_ipKeys->StreetAbbreviations, 
							VARIANT_FALSE, VARIANT_FALSE, 0, m_ipParser, &lStartPos, &lEndPos );
		if (lStartPos != -1 && lEndPos != -1)
		{
			// Set the flag
			bFound = true;
		}
	}

	if (bFound)
	{
		// Increment counter
		lCount++;

		// Decrement the counter if Street information is at or near the beginning
		if (lStartPos < 2)
		{
			lCount--;
		}
	}

	///////////////////////////////////////////////
	// Check for building subdivision names (i.e. Apartment, Suite, etc.)
	// Check for building subdivision abbreviations (i.e. Apt, Ste, etc.)
	///////////////////////////////////////////////
	bFound = false;

	// Check if string contains Building Subdivision Name
	if ( asCppBool(ipLine->ContainsStringInVector(
		m_ipKeys->BuildingNames, VARIANT_FALSE, VARIANT_TRUE, m_ipParser)) )
	{
		// Set the flag
		bFound = true;
	}
	else
	{
		// Check if the string contains a Building Subdivision Abbreviation
		if (asCppBool( ipLine->ContainsStringInVector(
			m_ipKeys->BuildingAbbreviations, VARIANT_FALSE, VARIANT_TRUE, m_ipParser)) )
		{
			// Set the flag
			bFound = true;
		}
	}

	if (bFound)
	{
		// Increment counter
		lCount++;
	}

	// Store position information
	*plStartPos = 0;
	*plEndPos = strTest.length() - 1;

	// Return result
	return lCount;
}
//-------------------------------------------------------------------------------------------------
bool CAddressSplitter::evaluateStringForCity(std::string strText, long *plStartPos, long *plEndPos)
{
	// Default return values
	bool bFoundCity = false;
	*plStartPos = -1;
	*plEndPos = -1;

	// Trim any trailing commas or semicolon delimiters
	string strLocal = trim( strText.c_str(), "", ",;" );

	// Make sure string has a reasonable length
	long lLength = strLocal.length();
	if (lLength < 2)
	{
		return bFoundCity;
	}

	// Find a comma or semicolon as delimiter
	long lDelimiterPos = strLocal.find_last_of( ",;" );

	// Check for no delimiter  ===>  need to evaluate individual words
	if (lDelimiterPos == string::npos)
	{
		long		lStartPos = -1;
		long		lEndPos = -1;
		long		lPreviousStart = -1;
		long		lPreviousEnd = -1;
		CComBSTR	bstrValue;
		string		strWord;
		string		strPreviousWord;
		ITokenPtr	ipToken;

		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13025", ipMiscUtils != __nullptr );

		// Create a local parser object
		IRegularExprParserPtr ipLocalParser = ipMiscUtils->GetNewRegExpParserInstance("AddressSplitter");
		ASSERT_RESOURCE_ALLOCATION( "ELI07349", ipLocalParser != __nullptr );

		// Parse the string into words
		ipLocalParser->Pattern = "\\S+";
		IIUnknownVectorPtr ipMatches = ipLocalParser->Find( 
			_bstr_t( strLocal.c_str() ), VARIANT_FALSE, VARIANT_FALSE, VARIANT_FALSE );

		// Check word count
		long lCount = ipMatches->Size();
		if (lCount < 3)
		{
			// One or two words ===> entire string is city
			*plStartPos = 0;
			*plEndPos = lLength - 1;

			// Set success flag
			bFoundCity = true;
		}
		// Three or more words
		else
		{
			// Get last word for initial position values
			ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( lCount - 1 ) )->Object1 );
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

			*plStartPos = lStartPos;
			*plEndPos = lEndPos;

			// Loop through words from end to beginning and look 
			// for an Address indicator that prefaces the City string
			// NOTE: Last word is assumed to be part of City

			// Extract this word
			ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( lCount - 2 ) )->Object1 );
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrValue );
			strWord = asString( bstrValue );

			for (int i = lCount - 2; i > 0; i--)
			{
				// Extract the previous word
				ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( i - 1 ) )->Object1 );
				bstrValue.Empty();
				ipToken->GetTokenInfo( &lPreviousStart, &lPreviousEnd, NULL, &bstrValue );
				strPreviousWord = asString( bstrValue );

				// Check if "this" word is an Address indicator
				if (isWordAddressIndicator( strWord, strPreviousWord ))
				{
					// Subsequent word is first word of city name
					// and previous saved position values apply.
					// Just set success flag
					bFoundCity = true;
					break;
				}
				else
				{
					// Assume that "this" word is part of the City name
					// so reset saved Start Position
					*plStartPos = lStartPos;

					// Update "this" word text and positions from 
					// "previous" values
					strWord = strPreviousWord;
					lStartPos = lPreviousStart;
					lEndPos = lPreviousEnd;
				}		// end else "this" word not an Address indicator
			}			// end for each word

			/////////////////////////////////////////////////////
			// Consider first word and still no address indicator
			/////////////////////////////////////////////////////
			if (!bFoundCity)
			{
				// Extract the first word
				ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( 0 ) )->Object1 );
				ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

				// Store start position of first word as 
				// start position of City
				*plStartPos = lStartPos;
				bFoundCity = true;

			}			// end if first word and still no City found
		}				// end else three or more words
	}					// end if no comma/semicolon delimiter
	else
	{
		// Need last substring, but without any whitespace
		// Find first subsequent non-whitespace character 
		long lNextChar = strLocal.find_first_not_of( " \\t", lDelimiterPos + 1 );
		if (lNextChar != string::npos)
		{
			// Store position information
			*plStartPos = lNextChar;
			*plEndPos = lLength - 1;

			// Set success flag
			bFoundCity = true;
		}
	}


	// Return result
	return bFoundCity;
}
//-------------------------------------------------------------------------------------------------
long CAddressSplitter::evaluateStringForRecipient(std::string strTest, long *plStartPos, 
												  long *plEndPos )
{
	// Initialize counter
	long lCount = 0;

	long		lStartPos = -1;
	long		lEndPos = -1;
	string		strLine;
	ITokenPtr	ipToken;

	// Create ISpatialString object for searches
	ISpatialStringPtr	ipLine( CLSID_SpatialString );
	ASSERT_RESOURCE_ALLOCATION( "ELI07211", ipLine != __nullptr );
	ipLine->CreateNonSpatialString(strTest.c_str(), "");

	////////////////////////////
	// Check for person titles
	// Check for person suffixes
	////////////////////////////
	bool	bFound = false;

	// Check if the text contains a Person Title
	if (asCppBool(ipLine->ContainsStringInVector( 
		m_ipKeys->PersonTitles, VARIANT_FALSE, VARIANT_TRUE, m_ipParser)))
	{
		// Set the flag
		bFound = true;
	}
	else
	{
		// Check if the string contains a Person Suffix
		if (asCppBool(ipLine->ContainsStringInVector(
			m_ipKeys->PersonSuffixes, VARIANT_FALSE, VARIANT_TRUE, m_ipParser) ))
		{
			// Set the flag
			bFound = true;
		}
	}

	if (bFound)
	{
		// Increment counter
		lCount++;
	}

	////////////////////////////////
	// Check for company suffixes
	// Check for company designators
	////////////////////////////////
	bFound = false;

	// Check if the string contains a Company Suffix
	if (asCppBool(ipLine->ContainsStringInVector(
		m_ipKeys->CompanySuffixes, VARIANT_FALSE, VARIANT_TRUE,  m_ipParser)))
	{
		// Set the flag
		bFound = true;
	}
	else
	{
		// Check if the string contains a Company Designator
		if (asCppBool(ipLine->ContainsStringInVector(
			m_ipKeys->CompanyDesignators, VARIANT_FALSE, VARIANT_TRUE,  m_ipParser)))
		{
			// Consider special case of address line being tagged as a recipient
			// due to containing a CompanyDesignator
			// Treat this as a Recipient if either counter = 0
			// If both counters > 0, give no special treatment
			if ((m_lNumRecipientLines == 0) ||
				(m_lNumAddressLines == 0))
			{
				// Set the flag 
				bFound = true;
			}
		}
	}

	if (bFound)
	{
		// Increment counter
		lCount++;

		// Give additional boost if Company info is at or near the end of the string
	}

	/////////////////////////////////
	// Check for direction indicators (i.e. Attention, Attn, Mail To, Return To, etc.)
	/////////////////////////////////
	bFound = false;

	// Check if text contains a Direction Indicator
	if (asCppBool(ipLine->ContainsStringInVector(
		m_ipKeys->DirectionIndicators, VARIANT_FALSE, VARIANT_TRUE,  m_ipParser)))
	{
		// Set the flag
		bFound = true;
	}

	if (bFound)
	{
		// Increment counter twice - this is a strong indicator!
		lCount += 2;
	}

	// Store position information
	*plStartPos = 0;
	*plEndPos = strTest.length() - 1;

	// Return result
	return lCount;
}
//-------------------------------------------------------------------------------------------------
bool CAddressSplitter::evaluateStringForState(std::string strText, long *plStartPos, long *plEndPos)
{
	// Default return values
	bool bFoundState = false;
	*plStartPos = -1;
	*plEndPos = -1;

	ITokenPtr	ipToken;
	long		lNameStart = -1;
	long		lNameEnd = -1;
	long		lCodeStart = -1;
	long		lCodeEnd = -1;

	// Check string length
	long lLength = strText.length();

	// Use regular expression to find State name
	m_ipParser->Pattern = _bstr_t( "\\b(Alabama|Alaska|Arizona|Arkansas|California|Colorado|Connecticut|Delaware|District\\s*of\\s*Columbia|(North|South)\\s*(Carolina|Dakota)|Florida|Georgia|Hawaii|Idaho|Illinois|Indiana|Iowa|Kansas|Kentucky|Louisiana|Maine|Maryland|Massachusetts|Michigan|Minnesota|Mississippi|Missouri|Montana|Nebraska|New\\s*Hampshire|New\\s*Jersey|New\\s*Mexico|New\\s*York|Nevada|Ohio|Oklahoma|Oregon|Pennsylvania|Rhode\\s+Island|Tennessee|Texas|Utah|Vermont|Washington|Wisconsin|Wyoming|(West\\s*)?Virginia|Guam|Puerto\\s*Rico|Virgin\\s*Islands)\\b" );
	_bstr_t	bstrLocal( strText.c_str() );
	IIUnknownVectorPtr ipNameMatches = m_ipParser->Find( bstrLocal, VARIANT_FALSE, VARIANT_FALSE, VARIANT_FALSE );
	if (ipNameMatches->Size() > 0)
	{
		// Retrieve last token
		ipToken = ITokenPtr( IObjectPairPtr( ipNameMatches->At( ipNameMatches->Size() - 1 ) )->Object1 );
		ipToken->GetTokenInfo( &lNameStart, &lNameEnd, NULL, NULL );

		*plStartPos = lNameStart;
		*plEndPos = lNameEnd;

		// Set preliminary success flag
		bFoundState = true;
	}

	// Check to see if state name is NOT at the end of the string
	if (lNameEnd != lLength - 1)
	{
		// State name may be part of a city name ===> look for a state code

		// Use regular expression to find State code
		m_ipParser->Pattern = _bstr_t( "\\b(AL|AK|AZ|AR|CA|CO|CT|DE|DC|NC|ND|SC|SD|FL|GA|HI|ID|IL|IN|IA|KS|KY|LA|ME|MD|MA|MI|MN|MS|MO|MT|NE|NH|NJ|NM|NY|NV|OH|OK|OR|PA|RI|TN|TX|UT|VT|WA|WI|WY|VA|WV|GU|PR|VI)\\b" );

		IIUnknownVectorPtr ipCodeMatches = m_ipParser->Find( bstrLocal, VARIANT_FALSE, VARIANT_FALSE, VARIANT_FALSE );
		if (ipCodeMatches->Size() > 0)
		{
			// Retrieve last token
			ipToken = ITokenPtr( IObjectPairPtr( ipCodeMatches->At( ipCodeMatches->Size() - 1 ) )->Object1 );
			ipToken->GetTokenInfo( &lCodeStart, &lCodeEnd, NULL, NULL );

			// Set success flag
			bFoundState = true;

			// Retain state code positions
			*plStartPos = lCodeStart;
			*plEndPos = lCodeEnd;
		}
	}
	else
	{
		// State name is at end of string, retain those positions
		*plStartPos = lNameStart;
		*plEndPos = lNameEnd;
	}

	// Return result
	return bFoundState;
}
//-------------------------------------------------------------------------------------------------
bool CAddressSplitter::evaluateStringForZipCode(std::string strText, long *plStartPos, long *plEndPos)
{
	// Default return values
	bool bFoundZip = false;
	*plStartPos = -1;
	*plEndPos = -1;

	// Use regular expression to find zip code
	m_ipParser->Pattern = _bstr_t( "\\d{5}(\\s*(-)?\\s*\\d{4})?" );
	_bstr_t	bstrLocal( strText.c_str() );
	IIUnknownVectorPtr ipMatches = m_ipParser->Find( bstrLocal, VARIANT_FALSE, VARIANT_FALSE, VARIANT_FALSE );
	if (ipMatches->Size() > 0)
	{
		// Retrieve last token
		ITokenPtr	ipToken;

		ipToken = ITokenPtr( IObjectPairPtr( ipMatches->At( ipMatches->Size() - 1 ) )->Object1 );
		ipToken->GetTokenInfo( plStartPos, plEndPos, NULL, NULL );

		// Set success flag
		bFoundZip = true;
	}

	// Return result
	return bFoundZip;
}
//-------------------------------------------------------------------------------------------------
void CAddressSplitter::handleUnprocessedLines(IIUnknownVectorPtr ipSubAttr)
{
	// Any unprocessed lines?
	if (m_ipTrailingLines == __nullptr)
	{
		return;
	}

	// Check number of unprocessed lines
	long lCount = m_ipTrailingLines->Size();
	for (int i = 0; i < lCount; i++)
	{
		// Retrieve this Spatial String
		ISpatialStringPtr	ipLine = m_ipTrailingLines->At( i );

		// Ignore short lines
		if (ipLine->GetSize() > 2)
		{
			//////////////////////////////////
			// Handle this line as a Recipient
			//////////////////////////////////

			// Increment counter
			m_lNumRecipientLines++;

			// Check for Combined Names & Addresses
			CString	zLabel;
			if (m_bCombinedNameAddress)
			{
				zLabel.Format( "Address%d", m_lNumAddressLines + m_lNumRecipientLines );

				// Add the SubAttribute
				// after all previous address lines
				addSubAttribute( ipLine, zLabel.operator LPCTSTR(), 
					m_lNumAddressLines + m_lNumRecipientLines - 1, ipSubAttr );
			}
			else
			{
				zLabel.Format( "Recipient%d", m_lNumRecipientLines );

				// Add the SubAttribute
				// before all address lines and after any previous recipient lines
				addSubAttribute( ipLine, zLabel.operator LPCTSTR(), 
					m_lNumRecipientLines - 1, ipSubAttr );
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool CAddressSplitter::isOrdinalNumber(const string& strWord)
{
	// Ordinal numbers have at least three characters
	if (strWord.length() < 3)
	{
		return false;
	}

	// Ordinal numbers have at least one digit character and end with two non-digit characters
	size_t nFirstLetter = strWord.find_first_not_of("0123456789");
	if (nFirstLetter < 1 || strWord.length() - nFirstLetter != 2)
	{
		return false;
	}

	// Get the ordinal indicator (-st, -nd, -rd, or -th)
	string strIndicator = strWord.substr(nFirstLetter);
	makeLowerCase(strIndicator);
	return strIndicator == "st" || strIndicator == "nd" || 
		strIndicator == "rd" || strIndicator == "th";
}
//-------------------------------------------------------------------------------------------------
bool CAddressSplitter::isWordAddressIndicator(std::string strWord, std::string strPreviousWord)
{
	///////////////////////
	// Check for any digits in the word
	///////////////////////
	if (strWord.find_first_of( "0123456789" ) != string::npos)
	{
		// Digits found, therefore this word is NOT 
		// part of a city name, just return
		return true;
	}

	//////////////////////////
	// Check for a Street Name 
	//////////////////////////
	long	lTempStart = -1;
	long	lTempEnd = -1;

	// Get Spatial String test object
	ISpatialStringPtr	ipWord( CLSID_SpatialString );
	ipWord->CreateNonSpatialString(strWord.c_str(), "");

	// Check if word contains a Street Name
	if (asCppBool(ipWord->ContainsStringInVector(
		m_ipKeys->StreetNames, VARIANT_FALSE, VARIANT_TRUE,  m_ipParser)))
	{
		// Street Name found, therefore this word is NOT 
		// part of a city name, just return
		return true;
	}

	//////////////////////////////////
	// Check for a Street Abbreviation
	//////////////////////////////////
	lTempStart = -1;
	lTempEnd = -1;

	// Check if word contains a Street Abbreviation
	if (asCppBool(ipWord->ContainsStringInVector(
		m_ipKeys->StreetAbbreviations, VARIANT_FALSE, VARIANT_TRUE,  m_ipParser)))
	{
		// Street Abbreviation found, therefore this word is 
		// probably NOT part of the city name.
		// Check for "St" that might be Street or Saint
		if (strWord.find_first_of( "St" ) != string::npos)
		{
			// "St" found, therefore need to check previous word
			if (strPreviousWord.length() > 0)
			{
				// No preceding previous word is available
				if (isWordAddressIndicator( strPreviousWord, "" ))
				{
					// Preceding word IS an address indicator, 
					// therefore this word is NOT and is part of 
					// a city name
					return false;
				}
				else
				{
					// Preceding word is NOT an address indicator, 
					// therefore this word IS an address indicator
					// just return
					return true;
				}	// end else preceding word not an address indicator
			}		// end if previous word not empty
			// No previous word, assume this is a Street Abbreviation
			else
			{
				return true;
			}		// else no previous word specified
		}			// end if "St" is the abbreviation found
		else
		{
			// "St" not found, therefore this word is 
			// a street abbreviation and NOT part of a 
			// city name, just return
			return true;
		}			// end else "St" is not the abbreviation found
	}				// end if Street Abbreviation found

	// Return failure to find an Address indicator
	return false;
}
//-------------------------------------------------------------------------------------------------
void CAddressSplitter::storeUnprocessedLine(ISpatialStringPtr ipExtra)
{
	// Create collection
	if (m_ipTrailingLines == __nullptr)
	{
		m_ipTrailingLines.CreateInstance( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION( "ELI07373", m_ipTrailingLines != __nullptr );
	}

	// Push this Spatial String into collection
	m_ipTrailingLines->PushBack( ipExtra );
}
//-------------------------------------------------------------------------------------------------
void CAddressSplitter::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI07173", "Address Splitter" );
}
//-------------------------------------------------------------------------------------------------
