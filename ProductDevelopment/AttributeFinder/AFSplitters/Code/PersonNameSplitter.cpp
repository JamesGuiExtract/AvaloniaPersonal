// PersonNameSplitter.cpp : Implementation of CPersonNameSplitter
#include "stdafx.h"
#include "AFSplitters.h"
#include "PersonNameSplitter.h"

#include <cpputil.h>
#include <comutils.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CPersonNameSplitter
//-------------------------------------------------------------------------------------------------
CPersonNameSplitter::CPersonNameSplitter()
: m_bDirty(false)
{
	try
	{
		// Instantiate the Entity Keywords object
		m_ipKeys.CreateInstance( CLSID_EntityKeywords );
		ASSERT_RESOURCE_ALLOCATION( "ELI06473", m_ipKeys != NULL );
		
		m_ipMisc.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI22557", m_ipMisc != NULL);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI06474")
}
//-------------------------------------------------------------------------------------------------
CPersonNameSplitter::~CPersonNameSplitter()
{
	try
	{
		m_ipKeys = NULL;
		m_ipMisc = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29465");
}

//-------------------------------------------------------------------------------------------------
// IInterfaceSupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPersonNameSplitter::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeSplitter,
		&IID_IPersistStream,
		&IID_ICopyableObject,
		&IID_ICategorizedComponent,
		&IID_ILicensedComponent,
		&IID_IPersonNameSplitter
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IAttributeSplitter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPersonNameSplitter::raw_SplitAttribute(IAttribute *pAttribute, IAFDocument *pAFDoc,
													 IProgressStatus *pProgressStatus)
{
	try
	{
		// Create local copies of specified Attribute
		// and collection of associated SubAttributes
		IAttributePtr ipMainAttribute( pAttribute );
		IIUnknownVectorPtr ipMainAttrSub = ipMainAttribute->SubAttributes;

		// Retrieve Attribute Value text
		ISpatialStringPtr	ipEntity = pAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION( "ELI06756", ipEntity != NULL );

		// Trim a leading "8"
		// This was probably an OCR error and should have been an ampersand
		ipEntity->Trim( _bstr_t( "8" ), _bstr_t( "" ) );

		// Get a regex parser
		IRegularExprParserPtr ipParser = getParser();

		// Set to ignore case
		ipParser->IgnoreCase = VARIANT_TRUE;

		// Create SpatialString object for pattern searches
		ISpatialStringPtr	ipWord( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION( "ELI06475", ipWord != NULL );

		// Create local string to facilitate later searches
		string	strEntity = ipEntity->String;

		long	lStartPos = -1;
		long	lEndPos = -1;
		bool	bLastNamePrefix = false;
		bool	bLastNameFirst = false;
		CComBSTR	bstrValue;
		ITokenPtr ipToken;

		// "Tokenize" the string, i.e. find all "whole words"	
		// Each word is composed of alpha-numeric characters, dash(-), underscore(_), 
		// period(.), forward slash(/), backward slash(\) and apostrophe(')
		ipParser->Pattern = "[A-Za-z0-9\\-_\\./\\\\']+";
		IIUnknownVectorPtr ipMatches = ipParser->Find( 
			ipEntity->String, VARIANT_FALSE, VARIANT_FALSE );

		// Check if the first word is a title ("Mr.", "Ms.", etc.)
		if (ipMatches->Size() >= 2) 
		{
			// Retrieve first "word"
			ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(0))->Object1);
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

			// Search the word for a Person Title
			ipWord = ipEntity->GetSubString( lStartPos, lEndPos );

			if (asCppBool(ipWord->ContainsStringInVector(
				m_ipKeys->PersonTitles, VARIANT_FALSE, VARIANT_TRUE,  ipParser)))
			{
				// Just remove the first word from the collection of words
				ipMatches->Remove( 0 );

				// Create and include a Title sub-attribute
				IAttributePtr ipAttr( CLSID_Attribute );
				ASSERT_RESOURCE_ALLOCATION( "ELI10338", ipAttr != NULL );
				ipAttr->Name = "Title";
				ipAttr->Value = ipWord;
				ipMainAttrSub->PushBack( ipAttr );
			}
		}
		
		// Check if the last word is a suffix ("Jr.", "Sr.", etc.)
		if (ipMatches->Size() >= 2) 
		{
			// Retrieve last "word"
			IObjectPairPtr ipLastPair = ipMatches->At(ipMatches->Size() - 1);
			ASSERT_RESOURCE_ALLOCATION( "ELI15561", ipLastPair != NULL );

			ipToken = ITokenPtr(ipLastPair->Object1);
			ASSERT_RESOURCE_ALLOCATION( "ELI15562", ipToken != NULL );
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

			// Search the word for a Person Suffix
			ipWord = ipEntity->GetSubString( lStartPos, lEndPos );

			if (asCppBool(ipWord->ContainsStringInVector(
				m_ipKeys->PersonSuffixes, VARIANT_FALSE, VARIANT_TRUE, ipParser)))
			{
				// Just remove the last word from the collection of words
				ipMatches->Remove( ipMatches->Size() - 1 );

				// Create and include a Suffix sub-attribute
				IAttributePtr ipAttr( CLSID_Attribute );
				ASSERT_RESOURCE_ALLOCATION( "ELI10339", ipAttr != NULL );
				ipAttr->Name = "Suffix";
				ipAttr->Value = ipWord;
				ipMainAttrSub->PushBack( ipAttr );			
			}
		}

		// Check for compound last name, (i.e. "Van Buren" or "Mc Donald")
		// Also check for "last name, first name" format
		_bstr_t bstrLastNamePrefix = "";
		if (ipMatches->Size() >= 2) 
		{
			/////////////////////////////////
			// Retrieve second-to-last "word"
			/////////////////////////////////
			IObjectPairPtr ipSecondLastPair = ipMatches->At(ipMatches->Size() - 2);
			ASSERT_RESOURCE_ALLOCATION( "ELI15563", ipSecondLastPair != NULL );

			ipToken = ITokenPtr(ipSecondLastPair->Object1);
			ASSERT_RESOURCE_ALLOCATION( "ELI15564", ipToken != NULL );
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

			ipWord = ipEntity->GetSubString( lStartPos, lEndPos );

			// Check against Last Name Prefixes Keyword
			_bstr_t bstrPrefixes = m_ipKeys->GetKeywordPattern( _bstr_t( "LastNamePrefixes" ) );
			ipParser->Pattern = bstrPrefixes;

			if (ipParser->StringMatchesPattern( ipWord->String ))
			{
				// Set last name prefix flag
				bLastNamePrefix = true;
			}

			////////////////////////
			// Retrieve first "word"
			////////////////////////
			ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(0))->Object1);
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrValue );

			// Check next character
			if (strEntity[lEndPos + 1] == ',')
			{
				// Set "last, first" flag
				bLastNameFirst = true;
			}

			// Check against Last Name Prefixes Keyword
			if (ipParser->StringMatchesPattern(_bstr_t(bstrValue)))
			{
				// Also must check second word for trailing comma
				// implying last name, first format
				ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(1))->Object1);
				ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

				// Check next character
				if (((long)(strEntity.length()) > lEndPos + 1) && 
					(strEntity[lEndPos + 1] == ','))
				{
					// Set last name prefix flag and "last, first" flag
					bLastNamePrefix = true;
					bLastNameFirst = true;
				}
			}
		}

		// Split the name into "First", "Middle", and "Last" - if available
		switch (ipMatches->Size()) 
		{
		// No words left, nothing more to split
		case 0:
			break;
		// One word - it must be a first name ("John" in "John and Mary Smith")
		case 1:
		{
			// Retrieve the first "word"
			ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(0))->Object1);
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

			// Trim any leading or trailing commas
			ipWord = ipEntity->GetSubString( lStartPos, lEndPos );
			ipWord->Trim( _bstr_t( "," ), _bstr_t( "," ) );

			// Create and include a First sub-attribute
			IAttributePtr ipAttr( CLSID_Attribute );
			ASSERT_RESOURCE_ALLOCATION( "ELI10340", ipAttr != NULL );
			ipAttr->Name = "First";
			ipAttr->Value = ipWord;
			ipMainAttrSub->PushBack( ipAttr );	
		}
		break;
		// Two words - First and Last OR First and Middle
		//  possible - Prefix and Last but this will be treated as First and Last
		case 2:
		{
			// Retrieve the first "word"
			ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(0))->Object1);
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

			// Trim any leading or trailing commas
			ipWord = ipEntity->GetSubString( lStartPos, lEndPos );
			long lWordLength = ipWord->Size;
			ipWord->Trim( _bstr_t( "," ), _bstr_t( "," ) );

			// Create and include a First sub-attribute
			// or a Last sub-attribute if appropriate
			IAttributePtr ipAttr1( CLSID_Attribute );
			ASSERT_RESOURCE_ALLOCATION( "ELI10341", ipAttr1 != NULL );
			if (bLastNameFirst)
			{
				ipAttr1->Name = "Last";
			}
			else
			{
				ipAttr1->Name = "First";
			}
			ipAttr1->Value = ipWord;
			ipMainAttrSub->PushBack( ipAttr1 );	

			// Retrieve the second "word"
			ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(1))->Object1);
			bstrValue.Empty();
			ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, &bstrValue );

			// Trim any leading or trailing commas
			ipWord = ipEntity->GetSubString( lStartPos, lEndPos );
			ipWord->Trim( _bstr_t( "," ), _bstr_t( "," ) );

			// Check for Middle Initial
			bool	bMiddle = false;
			if (lEndPos - lStartPos < 2)
			{
				// Check for two chars
				if (lEndPos - lStartPos == 1)
				{
					// Check for period
					string strWord = asString( bstrValue );
					if (strWord[1] == '.')
					{
						// Space for Initial and period
						bMiddle = true;
					}
				}
				else
				{
					// Only one char, it is an initial
					bMiddle = true;
				}
			}

			// Handle second word as Middle or Last
			// or First if Last was already found
			IAttributePtr ipAttr2( CLSID_Attribute );
			ASSERT_RESOURCE_ALLOCATION( "ELI10342", ipAttr2 != NULL );
			if (bLastNameFirst)
			{
				ipAttr2->Name = "First";
			}
			else if (bMiddle)
			{
				ipAttr2->Name = "Middle";
			}
			else
			{
				ipAttr2->Name = "Last";
			}
			ipAttr2->Value = ipWord;
			ipMainAttrSub->PushBack( ipAttr2 );
		}
		break;
		// More than two words:
		//    First word  ===> First name
		//    Last  word  ===> Last name
		//    Other words ===> Middle name
		// OR
		//    First  word  ===> Last name followed by comma
		//    Second word  ===> First name
		//    Other  words ===> Middle name
		default:
			{
				long lTempStartPos = -1;
				long lTempEndPos = -1;

				//////////////////////////
				// Retrieve the first name
				//////////////////////////
				if (bLastNameFirst)
				{
					if (bLastNamePrefix)
					{
						// First name follows the two-word last name
						ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(2))->Object1);
					}
					else
					{
						// First name follows the one-word last name
						ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(1))->Object1);
					}
				}
				else
				{
					// First name is the first word
					ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(0))->Object1);
				}
				
				ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );

				// Trim any leading or trailing commas
				ipWord = ipEntity->GetSubString( lStartPos, lEndPos );
				ipWord->Trim( _bstr_t( "," ), _bstr_t( "," ) );

				// Create and include a First sub-attribute
				IAttributePtr ipAttr1( CLSID_Attribute );
				ASSERT_RESOURCE_ALLOCATION( "ELI10343", ipAttr1 != NULL );
				ipAttr1->Name = "First";
				ipAttr1->Value = ipWord;
				ipMainAttrSub->PushBack( ipAttr1 );	

				/////////////////////////
				// Retrieve the Last name
				/////////////////////////
				if (bLastNamePrefix)
				{
					// Retrieve first "word" in Last name
					if (bLastNameFirst)
					{
						ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(0))->Object1);
					}
					else
					{
						ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(
							ipMatches->Size() - 2))->Object1);
					}
					ipToken->GetTokenInfo( &lStartPos, &lTempEndPos, NULL, NULL );

					// Retrieve last "word" in Last name
					if (bLastNameFirst)
					{
						ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(1))->Object1);
					}
					else
					{
						ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(
							ipMatches->Size() - 1))->Object1);
					}
					ipToken->GetTokenInfo( &lTempStartPos, &lEndPos, NULL, NULL );
				}
				else
				{
					// Retrieve only "word" in Last name
					if (bLastNameFirst)
					{
						ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(0))->Object1);
					}
					else
					{
						ipToken = ITokenPtr(IObjectPairPtr(ipMatches->At(
							ipMatches->Size() - 1))->Object1);
					}
					ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );
				}

				// Trim any leading or trailing commas
				ipWord = ipEntity->GetSubString( lStartPos, lEndPos );
				ipWord->Trim( _bstr_t( "," ), _bstr_t( "," ) );

				// Create and include a Last sub-attribute
				IAttributePtr ipAttr2( CLSID_Attribute );
				ASSERT_RESOURCE_ALLOCATION( "ELI10344", ipAttr2 != NULL );
				ipAttr2->Name = "Last";
				ipAttr2->Value = ipWord;
				ipMainAttrSub->PushBack( ipAttr2 );

				//////////////////////////////////////////////////
				// Retrieve the remaining "words" as a Middle name
				//////////////////////////////////////////////////

				// By default, first word was first name
				//    last word was last name
				//    remaining words, if any, are the middle name
				long lStartOfMiddle = 1;
				long lEndOfMiddle = ipMatches->Size() - 2;

				// Consider Prefix and "Last,First" cases
				if (bLastNamePrefix)
				{
					if (bLastNameFirst)
					{
						// First two words were Last name
						// Next word was first name
						lStartOfMiddle = 3;
						lEndOfMiddle++;
					}
					else
					{
						// Last two words were Last name
						lEndOfMiddle--;
					}
				}
				else if (bLastNameFirst)
				{
					// First word was last name
					// Second word was first name
					lStartOfMiddle = 2;
					lEndOfMiddle = ipMatches->Size() - 1;
				}

				// Single "word" middle name
				if (lStartOfMiddle == lEndOfMiddle)
				{
					// Retrieve only "word" in Middle name
					IObjectPairPtr ipOnlyPair = ipMatches->At( lStartOfMiddle );
					ASSERT_RESOURCE_ALLOCATION( "ELI15565", ipOnlyPair != NULL );

					ipToken = ITokenPtr(ipOnlyPair->Object1);
					ASSERT_RESOURCE_ALLOCATION( "ELI15566", ipToken != NULL );
					ipToken->GetTokenInfo( &lStartPos, &lEndPos, NULL, NULL );
				}
				// Multiple "word" middle name
				else if (lStartOfMiddle < lEndOfMiddle)
				{
					// Retrieve first "word" in Middle name
					IObjectPairPtr ipFirstPair = ipMatches->At( lStartOfMiddle );
					ASSERT_RESOURCE_ALLOCATION( "ELI15567", ipFirstPair != NULL );

					ipToken = ITokenPtr(ipFirstPair->Object1);
					ASSERT_RESOURCE_ALLOCATION( "ELI15568", ipToken != NULL );
					ipToken->GetTokenInfo( &lStartPos, &lTempEndPos, NULL, NULL );

					// Retrieve last "word" in Middle name
					IObjectPairPtr ipLastPair = ipMatches->At( lEndOfMiddle );
					ASSERT_RESOURCE_ALLOCATION( "ELI15569", ipLastPair != NULL );

					ipToken = ITokenPtr(ipLastPair->Object1);
					ASSERT_RESOURCE_ALLOCATION( "ELI15570", ipToken != NULL );
					ipToken->GetTokenInfo( &lTempStartPos, &lEndPos, NULL, NULL );
				}
				// No middle name
				else
				{
					break;
				}

				// Get whole string and trim any leading or trailing commas
				ipWord = ipEntity->GetSubString( lStartPos, lEndPos );
				ipWord->Trim( _bstr_t( "," ), _bstr_t( "," ) );

				// Create and include a Middle sub-attribute
				IAttributePtr ipAttr3( CLSID_Attribute );
				ASSERT_RESOURCE_ALLOCATION( "ELI10345", ipAttr3 != NULL );
				ipAttr3->Name = "Middle";
				ipAttr3->Value = ipWord;
				ipMainAttrSub->PushBack( ipAttr3 );
			}		// end more than two words left after removing title and suffix
		}			// end switch ipMatches->Size()
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05360")
		
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPersonNameSplitter::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19565", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Split the name of a person").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05352")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPersonNameSplitter::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPersonNameSplitter::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_PersonNameSplitter);
		ASSERT_RESOURCE_ALLOCATION("ELI05354", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05353");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPersonNameSplitter::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_PersonNameSplitter;
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPersonNameSplitter::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPersonNameSplitter::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// TODO: Reset all the member variables

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
			UCLIDException ue( "ELI07635", "Unable to load newer PersonNameSplitter." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05355");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPersonNameSplitter::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05356");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPersonNameSplitter::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPersonNameSplitter::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IPersonNameSplitter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPersonNameSplitter::BuildAttribute(BSTR strParentName, BSTR strTitle, BSTR strFirst, 
												 BSTR strMiddle, BSTR strLast, BSTR strSuffix, 
												 VARIANT_BOOL bAutoBuildParent, IAttribute* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Create Parent Attribute
		IAttributePtr	ipParent( CLSID_Attribute );
		ASSERT_RESOURCE_ALLOCATION( "ELI08519", ipParent != NULL );

		// Retrieve collection of sub-attributes
		IIUnknownVectorPtr	ipSubs = ipParent->GetSubAttributes();
		ASSERT_RESOURCE_ALLOCATION( "ELI08520", ipSubs != NULL );

		// Create string for Parent Attribute
		// to be populated depending on bAutoBuildParent
		string	strParent;

		///////////////
		// Handle Title
		///////////////
		_bstr_t	strText( strTitle );
		if (strText.length() > 0)
		{
			// Create Attribute
			IAttributePtr	ipTitleAttribute( CLSID_Attribute );
			ASSERT_RESOURCE_ALLOCATION( "ELI08521", ipTitleAttribute != NULL );

			// Apply Name
			ipTitleAttribute->PutName( _bstr_t( "Title" ) );

			// Create and populate Spatial String object for Value
			ISpatialStringPtr	ipValue( CLSID_SpatialString );
			ASSERT_RESOURCE_ALLOCATION( "ELI08522", ipValue != NULL );
			ipValue->CreateNonSpatialString(strTitle, "");

			// Apply Value
			ipTitleAttribute->PutValue( ipValue );

			// Add to collected sub-attributes
			ipSubs->PushBack( ipTitleAttribute );

			// Update Parent string
			if (bAutoBuildParent)
			{
				strParent += asString( strTitle );
			}
		}

		////////////////////
		// Handle First name
		////////////////////
		strText = _bstr_t( strFirst );
		if (strText.length() > 0)
		{
			// Create Attribute
			IAttributePtr	ipFirstAttribute( CLSID_Attribute );
			ASSERT_RESOURCE_ALLOCATION( "ELI08524", ipFirstAttribute != NULL );

			// Apply Name
			ipFirstAttribute->PutName( _bstr_t( "First" ) );

			// Create and populate Spatial String object for Value
			ISpatialStringPtr	ipValue( CLSID_SpatialString );
			ASSERT_RESOURCE_ALLOCATION( "ELI08525", ipValue != NULL );
			ipValue->CreateNonSpatialString(strFirst, "");

			// Apply Value
			ipFirstAttribute->PutValue( ipValue );

			// Add to collected sub-attributes
			ipSubs->PushBack( ipFirstAttribute );

			// Update Parent string
			if (bAutoBuildParent)
			{
				// Add space before first name
				if (strParent.length() > 0)
				{
					strParent += " ";
				}

				strParent += asString( strFirst );
			}
		}

		/////////////////////
		// Handle Middle name
		/////////////////////
		strText = _bstr_t( strMiddle );
		if (strText.length() > 0)
		{
			// Create Attribute
			IAttributePtr	ipMiddleAttribute( CLSID_Attribute );
			ASSERT_RESOURCE_ALLOCATION( "ELI08526", ipMiddleAttribute != NULL );

			// Apply Name
			ipMiddleAttribute->PutName( _bstr_t( "Middle" ) );

			// Create and populate Spatial String object for Value
			ISpatialStringPtr	ipValue( CLSID_SpatialString );
			ASSERT_RESOURCE_ALLOCATION( "ELI08527", ipValue != NULL );
			ipValue->CreateNonSpatialString(strMiddle, "");

			// Apply Value
			ipMiddleAttribute->PutValue( ipValue );

			// Add to collected sub-attributes
			ipSubs->PushBack( ipMiddleAttribute );

			// Update Parent string
			if (bAutoBuildParent)
			{
				// Add space before middle name
				if (strParent.length() > 0)
				{
					strParent += " ";
				}

				strParent += asString( strMiddle );
			}
		}

		///////////////////
		// Handle Last name
		///////////////////
		strText = _bstr_t( strLast );
		if (strText.length() > 0)
		{
			// Create Attribute
			IAttributePtr	ipLastAttribute( CLSID_Attribute );
			ASSERT_RESOURCE_ALLOCATION( "ELI08528", ipLastAttribute != NULL );

			// Apply Name
			ipLastAttribute->PutName( _bstr_t( "Last" ) );

			// Create and populate Spatial String object for Value
			ISpatialStringPtr	ipValue( CLSID_SpatialString );
			ASSERT_RESOURCE_ALLOCATION( "ELI08529", ipValue != NULL );
			ipValue->CreateNonSpatialString(strLast, "");

			// Apply Value
			ipLastAttribute->PutValue( ipValue );

			// Add to collected sub-attributes
			ipSubs->PushBack( ipLastAttribute );

			// Update Parent string
			if (bAutoBuildParent)
			{
				// Add space before last name
				if (strParent.length() > 0)
				{
					strParent += " ";
				}

				strParent += asString( strLast );
			}
		}

		////////////////
		// Handle Suffix
		////////////////
		strText = _bstr_t( strSuffix );
		if (strText.length() > 0)
		{
			// Create Attribute
			IAttributePtr	ipSuffixAttribute( CLSID_Attribute );
			ASSERT_RESOURCE_ALLOCATION( "ELI08530", ipSuffixAttribute != NULL );

			// Apply Name
			ipSuffixAttribute->PutName( _bstr_t( "Suffix" ) );

			// Create and populate Spatial String object for Value
			ISpatialStringPtr	ipValue( CLSID_SpatialString );
			ASSERT_RESOURCE_ALLOCATION( "ELI08531", ipValue != NULL );
			ipValue->CreateNonSpatialString(strSuffix, "");

			// Apply Value
			ipSuffixAttribute->PutValue( ipValue );

			// Add to collected sub-attributes
			ipSubs->PushBack( ipSuffixAttribute );

			// Update Parent string
			if (bAutoBuildParent)
			{
				// Add space before suffix
				if (strParent.length() > 0)
				{
					strParent += " ";
				}

				strParent += asString( strSuffix );
			}
		}

		// Add combined name as Parent Value
		if (bAutoBuildParent)
		{
			// Create and populate Spatial String object for Value
			ISpatialStringPtr	ipValue( CLSID_SpatialString );
			ASSERT_RESOURCE_ALLOCATION( "ELI08532", ipValue != NULL );
			ipValue->CreateNonSpatialString(strParent.c_str(), "");

			// Apply Value
			ipParent->PutValue( ipValue );
		}

		// Provide completed Attribute to caller
		*pVal = ipParent.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08518");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// private / helper methods
//-------------------------------------------------------------------------------------------------
IRegularExprParserPtr CPersonNameSplitter::getParser()
{
	try
	{
		IRegularExprParserPtr ipParser = m_ipMisc->GetNewRegExpParserInstance("PersonNameSplitter");
		ASSERT_RESOURCE_ALLOCATION("ELI29467", ipParser != NULL);
		
		return ipParser;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29466");
}
//-------------------------------------------------------------------------------------------------
void CPersonNameSplitter::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI05570", "Person Name Splitter" );
}
//-------------------------------------------------------------------------------------------------
