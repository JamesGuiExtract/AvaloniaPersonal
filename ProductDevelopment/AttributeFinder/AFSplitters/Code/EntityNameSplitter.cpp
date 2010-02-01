// EntityNameSplitter.cpp : Implementation of CEntityNameSplitter
#include "stdafx.h"
#include "AFSplitters.h"
#include "EntityNameSplitter.h"
#include "PersonNameSplitter.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <RegistryPersistenceMgr.h>
#include <common.h>
#include <ByteStreamManipulator.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 5;
const string gstrAF_SPLITTERS_PATH = gstrAF_REG_ROOT_FOLDER_PATH + string("\\AFSplitters");

//-------------------------------------------------------------------------------------------------
// CEntityNameSplitter
//-------------------------------------------------------------------------------------------------
CEntityNameSplitter::CEntityNameSplitter()
: m_bDirty(false),
  m_eAliasChoice(kIgnoreLaterEntities),
  m_bMoveTrustName(false),
  m_ipRegExprParser(NULL)
{
	try
	{
		// Instantiate the Entity Keywords object
		m_ipKeys.CreateInstance( CLSID_EntityKeywords );
		ASSERT_RESOURCE_ALLOCATION( "ELI06023", m_ipKeys != NULL );

		// Instantiate the Entity Finder object
		m_ipFinder.CreateInstance( CLSID_EntityFinder );
		ASSERT_RESOURCE_ALLOCATION( "ELI06024", m_ipFinder != NULL );

		// Create pointer to Registry Persistence Manager
		ma_pUserCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr( HKEY_CURRENT_USER, gstrAF_SPLITTERS_PATH) );
		ASSERT_RESOURCE_ALLOCATION( "ELI10459", ma_pUserCfgMgr.get() != NULL );

		// Create pointer to Entity Name Splitter settings
		ma_pENSConfigMgr = auto_ptr<ENSConfigMgr>(
			new ENSConfigMgr( ma_pUserCfgMgr.get(), "\\EntityNameSplitter" ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI10460", ma_pENSConfigMgr.get() != NULL );

		// Check flag for moving Trust names
		m_bMoveTrustName = (ma_pENSConfigMgr->getMoveNames() > 0) ? true : false;

		// Get Misc Utils pointer to get an instance of the regular expression parser
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI22438", m_ipMiscUtils != NULL );
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI06028")
}
//-------------------------------------------------------------------------------------------------
CEntityNameSplitter::~CEntityNameSplitter()
{
	try
	{
		m_ipFinder = NULL;
		m_ipKeys = NULL;
		m_ipMiscUtils = NULL;
		m_ipRegExprParser = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28855");
}

//-------------------------------------------------------------------------------------------------
// IInterfaceSupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitter::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeSplitter,
		&IID_IEntityNameSplitter,
		&IID_IPersistStream,
		&IID_ICopyableObject,
		&IID_ICategorizedComponent,
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
// IAttributeSplitter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitter::raw_SplitAttribute(IAttribute *pAttribute, IAFDocument *pAFDoc, 
													 IProgressStatus *pProgressStatus)
{
	try
	{
		try
		{
			//////////////////////////////////////////////
			// Step 1: Setup and retrieve text to be split
			//////////////////////////////////////////////

			bool	bIsCompany = false;
			bool	bIsTrust = false;
			bool	bIsMunicipality = false;
			bool	bSuccess = false;

			IAttributePtr ipMainAttribute( pAttribute );
			IIUnknownVectorPtr ipMainAttrSub = ipMainAttribute->SubAttributes;

			// Just return if SubAttributes not empty
			// i.e. do not split text again (P16 #1210)
			if (ipMainAttrSub->Size() > 0)
			{
				return S_OK;
			}

			// Wrap the AFDocument in smart pointer
			// This does not need to be non-NULL since the Entity Name Splitter does 
			// not use the AFDocument object
			IAFDocumentPtr ipAFDoc(pAFDoc);

			// Get a new parser (we will release this parser when the method exits)
			m_ipRegExprParser = getParser();

			// Set search to ignore case
			m_ipRegExprParser->IgnoreCase = VARIANT_TRUE;

			ISpatialStringPtr	ipOriginal = pAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION( "ELI06000", ipOriginal != NULL );

			// Make copy of text for later use
			ICopyableObjectPtr	ipCopy = ipOriginal;
			ASSERT_RESOURCE_ALLOCATION( "ELI06744", ipCopy != NULL );
			ISpatialStringPtr	ipEntity = ipCopy->Clone();

			// Define pointer to parent Attribute - used for relationships with Alias subattributes
			IAttributePtr ipParent( NULL );

			// Determine current string length
			int iLength = ipEntity->GetSize();

			// Check for zero and replace with O
			// Check for degree symbol and replace with O [FlexIDSCore #2964]
			ipEntity->Replace(" 0 ", " O ", VARIANT_FALSE, 0, m_ipRegExprParser);
			ipEntity->Replace(" 0. "," O. ", VARIANT_FALSE, 0, m_ipRegExprParser);
			ipEntity->Replace("°", "O", VARIANT_FALSE, 0, m_ipRegExprParser);

			//////////////////////////////////////////////////
			// Step 1B: Trim unwanted text from the string
			//          Using EntityTrimAnyPhrases expressions
			//////////////////////////////////////////////////
			// Retrieve list of EntityTrimAnyPhrases expressions
			IVariantVectorPtr ipTrimAny = m_ipKeys->GetKeywordCollection( 
				_bstr_t( "EntityTrimAnyPhrases" ) );
			ASSERT_RESOURCE_ALLOCATION( "ELI26466", ipTrimAny != NULL );

			// Check string for Phrases
			long lTrimStart = -1;
			long lTrimEnd = -1;
			bool bFoundPhrase = true;
			while (bFoundPhrase)
			{
				// Clear the flag so that search is only done once unless 
				// trimming is done
				bFoundPhrase = false;

				// Check string for first Phrase, if any
				ipEntity->FindFirstItemInRegExpVector( ipTrimAny, VARIANT_FALSE, VARIANT_FALSE, 
					0, m_ipRegExprParser, &lTrimStart, &lTrimEnd );
				if (lTrimStart != -1 && lTrimEnd != -1)
				{
					// A phrase for trimming was found, trim it
					ipEntity->Remove(lTrimStart, lTrimEnd);

					// Set flag to keep searching
					bFoundPhrase = true;
				}
			}

			// Consolidate whitespace
			ipEntity->ConsolidateChars( _bstr_t( " " ), VARIANT_FALSE );

			///////////////////////////////////////////
			// Step 2A: Check text for Trust Indicators
			///////////////////////////////////////////

			// Retrieve list of Trust Indicator expressions
			IVariantVectorPtr ipTrustInd = m_ipKeys->GetKeywordCollection( _bstr_t( "TrustIndicators" ) );
			ASSERT_RESOURCE_ALLOCATION( "ELI10320", ipTrustInd != NULL );

			// Check if string contains a Trust Indicator
			if (asCppBool(ipEntity->ContainsStringInVector(
				ipTrustInd, VARIANT_FALSE, VARIANT_TRUE,  m_ipRegExprParser)))
			{
				// Just set the flag here
				bIsTrust = true;
			}

			//////////////////////////////////////////////////
			// Step 2B: Check text for Municipality Indicators
			//////////////////////////////////////////////////

			// Retrieve list of Municipality Indicator expressions
			IVariantVectorPtr ipMuniInd = m_ipKeys->GetKeywordCollection( 
				_bstr_t( "MunicipalityIndicators" ) );
			ASSERT_RESOURCE_ALLOCATION( "ELI10526", ipMuniInd != NULL );

			// Check if string contains a Muni Indicator
			if (asCppBool(ipEntity->ContainsStringInVector(
				ipMuniInd, VARIANT_FALSE, VARIANT_TRUE,  m_ipRegExprParser)))
			{
				// Just set the flag here
				bIsMunicipality = true;
			}

			/////////////////////////////////////////
			// Step 3A: Check text for Person Suffix
			/////////////////////////////////////////

			// Check string for item
			long	lStartPos = -1;
			long	lEndPos = -1;
			bool	bSuffixFound = false;
			ipEntity->FindFirstItemInRegExpVector( m_ipKeys->PersonSuffixes, 
				VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lStartPos, &lEndPos );
			if (lStartPos != -1 && lEndPos != -1)
			{
				// Set flag
				bSuffixFound = true;

				// Check for preceding period
				if (lStartPos > 1)
				{
					// Retrieve character two back from Start
					char cChar = (char)ipEntity->GetChar( lStartPos - 2 );

					// Check for period
					if (cChar == '.')
					{
						// Change the period to a comma
						ipEntity->SetChar(lStartPos - 2, ',' );
					}
				}
			}

			//////////////////////////////////////
			// Step 3B: Check text for digit words
			//          that indicate Company
			//////////////////////////////////////
			if (!bIsTrust && !bIsMunicipality &&!bSuffixFound)
			{
				string strTest = ipEntity->String;
				long lDigitPos = strTest.find_first_of( "0123456789", 0 );
				if (lDigitPos != string::npos)
				{
					// This is a company, set flags
					bIsCompany = true;
					bSuccess = true;
				}
			}

			///////////////////////////////////////////////
			// Step 3C: Check text for Person Designator(s)
			///////////////////////////////////////////////

			// Check string for Person Designators
			lStartPos = -1;
			lEndPos = -1;
			bool	bDone = false;
			while (!bDone && !bSuccess)
			{
				// Search the substring for another Person Designator
				ipEntity->FindFirstItemInRegExpVector( m_ipKeys->PersonDesignators, 
					VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lStartPos, &lEndPos );
				if (lStartPos != -1 && lEndPos != -1)
				{
					// Remove the Person Designator less one character
					ipEntity->Remove( lStartPos, lEndPos - 1 );

					// Replace retained character with semicolon to facilitate later splitting
					ipEntity->SetChar( lStartPos, ';' );
				}
				else
				{
					// No (more) Person Designators, stop searching
					bDone = true;
				}
			}

			// Check length of updated string
			if (ipEntity->GetSize() < iLength)
			{
				// Person Designator(s) were found and removed
				bSuccess = true;
			}

			/////////////////////////////////////////
			// Step 4: Handle Trust and Trustee items
			/////////////////////////////////////////
			if (bIsTrust)
			{
				// Return from here if Trust items were found
				bIsTrust = doTrustSplitting( ipEntity, ipAFDoc, ipMainAttrSub );
				if (bIsTrust)
				{
					return S_OK;
				}
			}

			////////////////////////////////////////
			// Step 5: Check text for Company Suffix
			////////////////////////////////////////

			if (!bSuccess && !bIsMunicipality)
			{
				// Check string for item
				long	lStartPos = -1;
				long	lEndPos = -1;
				ipEntity->FindFirstItemInRegExpVector( m_ipKeys->CompanySuffixes, 
					VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lStartPos, &lEndPos );
				if (lStartPos != -1 && lEndPos != -1)
				{
					// Company suffix found, set flags
					bIsCompany = true;
					bSuccess = true;
					bSuffixFound = true;

					// Check for preceding period
					if (lStartPos > 1)
					{
						// Retrieve character two back from Start
						char cChar = (char)ipEntity->GetChar(lStartPos - 2);

						// Check for period
						if (cChar == '.')
						{
							// Change the period to a comma
							ipEntity->SetChar(lStartPos - 2, ',' );
						}
					}
				}
			}

			//////////////////////////////////////////////////
			// Step 6: Check text for other Company Designator
			//////////////////////////////////////////////////

			if (!bSuccess && !bIsMunicipality)
			{
				// Check if string contains a Company Designator
				if (asCppBool(ipEntity->ContainsStringInVector(
					m_ipKeys->CompanyDesignators, VARIANT_FALSE, VARIANT_TRUE,  m_ipRegExprParser)))
				{
					// Company designator found, set flags
					bIsCompany = true;
					bSuccess = true;
				}
			}

			//////////////////////////////////////////////////////////////
			// Step 7: Assume that text is collection of 1 or more Persons
			//////////////////////////////////////////////////////////////

			// Create Person Splitter object
			IAttributeSplitterPtr ipPersonSplitter( CLSID_PersonNameSplitter );
			ASSERT_RESOURCE_ALLOCATION( "ELI09377", ipPersonSplitter != NULL );

			if (!bIsCompany && !bIsMunicipality)
			{
				////////////////////////////
				// if "A and B" (two or more humans) - split into multiple sub-attributes
				////////////////////////////

				// Find and remove some PersonTrimIdentifiers
				// before trimming and parsing
				bool bDonePTI = false;
				while (!bDonePTI)
				{
					long	lStartPos = -1;
					long	lEndPos = -1;
					ipEntity->FindFirstItemInRegExpVector( m_ipKeys->PersonTrimIdentifiers, 
						VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lStartPos, &lEndPos );
					if (lStartPos != -1 && lEndPos != -1)
					{
						// Remove the text less one character
						ipEntity->Remove( lStartPos, lEndPos - 1 );

						// Replace retained character with semicolon to facilitate later division
						ipEntity->SetChar( lStartPos, ';' );
					}
					else
					{
						bDonePTI = true;
					}
				}

				// Trim any leading or trailing spaces or periods or colons
				string	strFinal;
				if (!bSuffixFound)
				{
					ipEntity->Trim( _bstr_t( " .:;" ), _bstr_t( " .:;" ) );
				}
				else
				{
					// Do NOT trim trailing period if suffix found
					ipEntity->Trim( _bstr_t( " .:;" ), _bstr_t( " :;" ) );
				}
				_bstr_t bstrLocal = ipEntity->GetString();

				// Divide text into multiple entities
				IIUnknownVectorPtr ipMatches( CLSID_IUnknownVector );
				ASSERT_RESOURCE_ALLOCATION( "ELI09376", ipMatches != NULL );
				findNameDelimiters( ipEntity, bSuffixFound, ipMatches );

				// If the first person does not have a last name, we assume it is that
				// of the second person.  The boolean variable is true until we find
				// a last name
				bool bNoLastName = true;

				// Reset pointer to parent Attribute
				ipParent = NULL;

				// Process Person names
				long	lSize = ipMatches->Size();
				long	lEndOfPreviousAnd = -1;
				long	lPersonCount = 0;
				bool	bProcessed = false;
				for (int i = 0; i <= lSize; i++)
				{
					ISpatialStringPtr	ipPerson( CLSID_SpatialString );
					ASSERT_RESOURCE_ALLOCATION( "ELI06680", ipPerson != NULL );

					// Get the name as a string
					if (lSize > 0)
					{
						// Retrieve this potential Entity
						ipPerson = getEntityFromDelimiters( i, ipEntity, ipMatches );
					}

					// Continue only with valid Entities
					if (isValidEntity( ipPerson, true ))
					{
						// Always send ipPerson through handleAlias() at least once
						UCLID_AFUTILSLib::EAliasType	ePreviousType = (UCLID_AFUTILSLib::EAliasType)0;
						UCLID_AFUTILSLib::EAliasType	eNextType = (UCLID_AFUTILSLib::EAliasType)0;
						long		lPreviousAliasItem = -1;
						long		lNextAliasItem = -1;
						while (true)
						{
							// Create Spatial String for Alias information
							// to be processed later
							ISpatialStringPtr	ipExtra( CLSID_SpatialString );
							ASSERT_RESOURCE_ALLOCATION("ELI08711", ipExtra != NULL);

							// Check Name for Person Alias
							bool bContinue = handleAlias( ipPerson, ipExtra, eNextType, 
								lNextAliasItem );

							// Check for Person Trim Identifiers
							removePersonTrimIdentifiers( ipPerson, true );

							// I just like re-using the ipAttr name, so - unnamed blocks
							if (isValidEntity( ipPerson, true ))
							{
								// Set Attribute Value field
								IAttributePtr ipAttr( CLSID_Attribute );
								ASSERT_RESOURCE_ALLOCATION("ELI09615", ipAttr != NULL);
								ipAttr->Value = ipPerson;

								// Provide debug output
								string strResult = asString( ipPerson->String );
								::convertCppStringToNormalString( strResult );
								TRACE( "Person = \"%s\"\r\n", strResult.c_str() );

								// Set Type field
								setTypeFromAlias( ipAttr, ePreviousType, lPreviousAliasItem );

								// Set Attribute Name field
								bool bSplitPerson = true;
								if (ePreviousType == kAliasNone)
								{
									// This Attribute is not an Alias
									// Judge Person vs. Company
									if (entityIsCompany( ipPerson ))
									{
										ipAttr->Name = "Company";
										bSplitPerson = false;
									}
									else
									{
										ipAttr->Name = "Person";
									}

									// Set pointer to Parent for possible Alias items
									ipParent = ipAttr;
								}
								else if (ePreviousType == kAliasCompany)
								{
									// This Attribute is an Alias
									ipAttr->Name = "CompanyAlias";
									bSplitPerson = false;
								}
								else
								{
									// This Attribute is an Alias
									ipAttr->Name = "PersonAlias";
								}

								// Pass to PersonNameSplitter for First, Last, etc.
								if (bSplitPerson)
								{
									ipPersonSplitter->SplitAttribute( ipAttr, ipAFDoc, NULL );
								}
								lPersonCount++;

								// Check for "Last" sub-attribute
								// Only for first Attribute
								if ((lPersonCount == 1) && bSplitPerson)
								{
									// Retrieve collected sub-attributes
									IIUnknownVectorPtr ipSub = ipAttr->SubAttributes;
									ASSERT_RESOURCE_ALLOCATION("ELI15539", ipSub != NULL);
									for (int j = 0; j < ipSub->Size(); j++)
									{
										// Get the sub-attribute
										IAttributePtr ipThis = ipSub->At( j );
										ASSERT_RESOURCE_ALLOCATION("ELI15540", ipThis != NULL);

										// Compare the Names
										if (_bstr_t(ipThis->Name) == _bstr_t("Last"))
										{
											bNoLastName = false;
											break;
										}
									}
								}

								// Add the sub-attribute to the vector
								if (ipParent == ipAttr)
								{
									ipMainAttrSub->PushBack( ipAttr );	
								}
								// Add the Alias item to the Parent
								else
								{
									if (ipParent)
									{
										// Get the sub-attributes
										IIUnknownVectorPtr ipSub = ipParent->GetSubAttributes();
										ASSERT_RESOURCE_ALLOCATION("ELI15541", ipSub != NULL);
										ipSub->PushBack( ipAttr );
									}
									else
									{
										ipMainAttrSub->PushBack( ipAttr );	
									}
								}

								bProcessed = true;
							}

							// Check for more Alias information
							if (bContinue)
							{
								// Copy leftover Entity information into ipPerson object
								ipPerson = ipExtra->GetSubString( 0, -1 );

								// Store Alias details
								ePreviousType = eNextType;
								lPreviousAliasItem = lNextAliasItem;
							}
							else
							{
								break;
							}
						}		// end while Aliases to be handled
					}			// end if ipPerson not empty
				}				// end for each Person name

				// If the first person has no last name, copy one from the second
				if (bProcessed && bNoLastName && ipMainAttrSub->Size() > 1)
				{
					// Create new Attribute for the Last name
					IAttributePtr ipAttr = NULL;

					// Retrieve first person and second person
					IAttributePtr ipAttr0 = ipMainAttrSub->At(0);
					IAttributePtr ipAttr1 = ipMainAttrSub->At(1);

					// Step through the sub-attributes of the 2nd person
					IIUnknownVectorPtr ipSub = ipAttr1->SubAttributes;
					ASSERT_RESOURCE_ALLOCATION("ELI15543", ipSub != NULL);
					for (int i = 0; i < ipSub->Size(); i++)
					{
						// Retrieve this sub-attribute, looking for "Last"
						IAttributePtr ipAttrSub = ipSub->At(i);
						ASSERT_RESOURCE_ALLOCATION("ELI15676", ipAttrSub != NULL);

						if (ipAttrSub->Name == _bstr_t("Last"))
						{
							// Provide clone of this Attribute to first person
							ICopyableObjectPtr ipCopy = ipAttrSub;
							ASSERT_RESOURCE_ALLOCATION("ELI15674", ipCopy != NULL);
							ipAttr = ipCopy->Clone();
							ASSERT_RESOURCE_ALLOCATION("ELI15675", ipAttr != NULL);

							// Get the first Person string
							ISpatialStringPtr ipFirstPerson = ipAttr0->Value;
							ASSERT_RESOURCE_ALLOCATION("ELI15545", ipFirstPerson != NULL);

							// Append a space to the first Person string
							string strAppend = " ";
							ipFirstPerson->AppendString( strAppend.c_str() );

							// Append the Last name to the first Person string
							ipFirstPerson->Append( ipAttrSub->Value );
							break;
						}
					}

					// Add new Lastname Attribute to collected subattributes
					if (ipAttr)
					{
						// Retrieve sub-attributes for first Person
						IIUnknownVectorPtr ipFirstSub = ipAttr0->SubAttributes;
						ASSERT_RESOURCE_ALLOCATION("ELI15546", ipFirstSub != NULL);

						// Add the new "Last" attribute
						ipFirstSub->PushBack( ipAttr );	
					}
				}
				// only one person or persons without separators
				else if (!bProcessed)
				{
					// Replace carriage returns with spaces 
					ipEntity->Replace("\r", " ", VARIANT_FALSE, 0, m_ipRegExprParser);
					ipEntity->Replace("\n", " ", VARIANT_FALSE, 0, m_ipRegExprParser);

					// Get vector of names from found words
					IIUnknownVectorPtr	ipNames = getNamesFromWords( ipEntity );
					int iNameCount = ipNames->Size();

					// Reset pointer to parent Attribute
					ipParent = NULL;

					// Create and split Attribute for each name
					for (int j = 0; j < iNameCount; j++)
					{
						// Retrieve this name
						ISpatialStringPtr	ipName = ipNames->At( j );

						// Always send ipName through handleAlias() at least once
						UCLID_AFUTILSLib::EAliasType	ePreviousType = (UCLID_AFUTILSLib::EAliasType)0;
						UCLID_AFUTILSLib::EAliasType	eNextType = (UCLID_AFUTILSLib::EAliasType)0;
						long		lPreviousAliasItem = -1;
						long		lNextAliasItem = -1;
						while (true)
						{
							// Create Spatial String for Alias information
							// to be processed later
							ISpatialStringPtr	ipExtra( CLSID_SpatialString );
							ASSERT_RESOURCE_ALLOCATION("ELI08714", ipExtra != NULL);

							// Check Name for Person Alias
							bool bContinue = handleAlias( ipName, ipExtra, eNextType, lNextAliasItem );

							// Check for Person Trim Identifiers
							removePersonTrimIdentifiers( ipName, true );

							// Create and populate the attribute
							if (isValidEntity( ipName, true ))
							{
								IAttributePtr ipAttr( CLSID_Attribute );
								ASSERT_RESOURCE_ALLOCATION("ELI09617", ipAttr != NULL);
								ipAttr->Value = ipName;

								// Provide debug output
								string strResult = asString( ipName->String );
								::convertCppStringToNormalString( strResult );
								TRACE( "Person = \"%s\"\r\n", strResult.c_str() );

								// Set Type field
								setTypeFromAlias( ipAttr, ePreviousType, lPreviousAliasItem );

								// Set Attribute Name field
								bool bSplitPerson = true;
								if (ePreviousType == kAliasNone)
								{
									// This Attribute is not an Alias
									// Judge Person vs. Company
									if (entityIsCompany( ipName ))
									{
										ipAttr->Name = "Company";
										bSplitPerson = false;
									}
									else
									{
										ipAttr->Name = "Person";
									}

									// Set pointer to Parent for possible Alias items
									ipParent = ipAttr;
								}
								else if (ePreviousType == kAliasCompany)
								{
									// This Attribute is an Alias
									ipAttr->Name = "CompanyAlias";
									bSplitPerson = false;
								}
								else
								{
									// This Attribute is an Alias
									ipAttr->Name = "PersonAlias";
								}

								// Pass to PersonNameSplitter for First, Last, etc.
								if (bSplitPerson)
								{
									ipPersonSplitter->SplitAttribute( ipAttr, ipAFDoc, NULL );
								}

								// Check for "Last" sub-attribute in Attribute #1
								// If it is missing, it must be added later
								if ((j == 0) && bSplitPerson)
								{
									// Retrieve sub-attributes
									IIUnknownVectorPtr ipSub = ipAttr->SubAttributes;
									ASSERT_RESOURCE_ALLOCATION("ELI15547", ipSub != NULL);

									for (int k = 0; k < ipSub->Size(); k++)
									{
										// Retrieve this sub-attribute
										IAttributePtr ipSubAttr = ipSub->At( k );
										ASSERT_RESOURCE_ALLOCATION("ELI15548", ipSubAttr != NULL);

										// Check for "Last"
										if (_bstr_t(ipSubAttr->Name) == _bstr_t("Last"))
										{
											bNoLastName = false;
											break;
										}
									}
								}

								// Add the sub-attribute to the vector
								if (ipParent == ipAttr)
								{
									ipMainAttrSub->PushBack( ipAttr );	
								}
								// Add the Alias item to the Parent
								else
								{
									if (ipParent)
									{
										IIUnknownVectorPtr ipSub = ipParent->SubAttributes;
										ASSERT_RESOURCE_ALLOCATION("ELI15549", ipSub != NULL);
										ipSub->PushBack( ipAttr );
									}
									else
									{
										ipMainAttrSub->PushBack( ipAttr );	
									}
								}

								bProcessed = true;
							}

							// Check for more Alias information
							if (bContinue)
							{
								// Copy leftover Entity information into ipName object
								ipName = ipExtra->GetSubString( 0, -1 );

								// Store Alias details
								ePreviousType = eNextType;
								lPreviousAliasItem = lNextAliasItem;
							}
							else
							{
								break;
							}
						}		// end while Aliases to be handled
					}			// end for each Name

					// Check for missing last name
					if (bProcessed && bNoLastName && ipMainAttrSub->Size() > 1)
					{
						// Create new Lastname Attribute with empty string Value
						IAttributePtr ipAttr( CLSID_Attribute );
						ASSERT_RESOURCE_ALLOCATION("ELI09618", ipAttr != NULL);
						ipAttr->Name = "Last";

						ISpatialStringPtr ipValue = ipAttr->Value;
						ASSERT_RESOURCE_ALLOCATION("ELI15550", ipValue != NULL);

						// Retrieve first person
						IAttributePtr ipAttr0 = ipMainAttrSub->At(0);
						// Retrieve second person
						IAttributePtr ipAttr1 = ipMainAttrSub->At(1);

						// Retrieve sub-attributes from second person
						IIUnknownVectorPtr ipSub1 = ipAttr1->SubAttributes;
						ASSERT_RESOURCE_ALLOCATION("ELI15551", ipSub1 != NULL);

						// Examine each sub-attribute
						long lSubSize = ipSub1->Size();
						for (long i = 0; i < lSubSize; i++)
						{
							// Retrieve this sub-attribute
							IAttributePtr ipAttrSub = ipSub1->At(i);
							ASSERT_RESOURCE_ALLOCATION("ELI15552", ipAttrSub != NULL);

							// Check if this is a lastname sub-attribute
							if (ipAttrSub->Name == _bstr_t("Last"))
							{
								// Save the actual last name
								ipValue = ipAttrSub->Value;
								ASSERT_RESOURCE_ALLOCATION("ELI25941", ipValue != NULL);

								// Get the first Person string
								ISpatialStringPtr ipFirst = ipAttr0->Value;
								ASSERT_RESOURCE_ALLOCATION("ELI15553", ipFirst != NULL);

								// Append a space to the first Person string
								ipFirst->AppendString( _bstr_t( " " ) );

								// Append the Last name to the first Person string
								ipFirst->Append( ipValue );
								break;
							}
						}

						// Add new Lastname Attribute to collected subattributes
						if (ipValue->IsEmpty() == VARIANT_FALSE)
						{
							// Retrieve sub-attributes from first person
							IIUnknownVectorPtr ipSub0 = ipAttr0->SubAttributes;
							ASSERT_RESOURCE_ALLOCATION("ELI15554", ipSub0 != NULL);

							ipSub0->PushBack( ipAttr );	
						}
					}
				}
			}
			// Company or Municipality information was found - treat the text as 
			// multiple companies delimited by semicolons
			else
			{
				// Check string for Company Assignor
				long	lStartPos = -1;
				long	lEndPos = -1;
				ipEntity->FindFirstItemInRegExpVector( m_ipKeys->CompanyAssignors, 
					VARIANT_FALSE, VARIANT_FALSE, 0, m_ipRegExprParser, &lStartPos, &lEndPos );
				if (lStartPos != -1 && lEndPos != -1)
				{
					// Company assignor found, remove this portion of string
					ipEntity->Remove( lStartPos, -1 );
				}

				// Check string for forward slash - modify this to AKA
				// unless part of digits
				//			handleCompanySlash( ipEntity );

				// Treat semicolon as delimiter between Companies
				IIUnknownVectorPtr ipMatches( CLSID_IUnknownVector );
				ASSERT_RESOURCE_ALLOCATION( "ELI10539", ipMatches != NULL );
				processDelimiter( ipEntity, ";", ipMatches );

				// Reset pointer to parent Attribute
				ipParent = NULL;

				// Process Company names
				long	lSize = ipMatches->Size();
				for (int iIndex = 0; iIndex <= lSize; iIndex++)
				{
					ISpatialStringPtr	ipCompany( CLSID_SpatialString );
					ASSERT_RESOURCE_ALLOCATION( "ELI10540", ipCompany != NULL );

					// Retrieve this potential Entity
					ipCompany = getEntityFromDelimiters( iIndex, ipEntity, ipMatches );

					// Always send ipCompany through handleAlias() at least once
					EAliasType	ePreviousType = (UCLID_AFUTILSLib::EAliasType)0;
					EAliasType	eNextType = (UCLID_AFUTILSLib::EAliasType)0;
					long		lPreviousAliasItem = -1;
					long		lNextAliasItem = -1;
					while (true)
					{
						// Create Spatial String for Alias information
						// to be processed later
						ISpatialStringPtr	ipExtra( CLSID_SpatialString );
						ASSERT_RESOURCE_ALLOCATION("ELI08715", ipExtra != NULL);

						// Check Entity for Company Alias
						bool bContinue = handleAlias( ipCompany, ipExtra, eNextType, lNextAliasItem );

						// Check for Person Trim Identifiers
						removePersonTrimIdentifiers( ipCompany, false );

						if (isValidEntity( ipCompany, false ))
						{
							IAttributePtr ipAttr( CLSID_Attribute );
							ASSERT_RESOURCE_ALLOCATION("ELI09619", ipAttr != NULL);
							ipAttr->Value = ipCompany;

							// Prepare debug output
							string strResult = asString( ipCompany->String );
							::convertCppStringToNormalString( strResult );

							// Confirm that this entity looks like a Company
							if (entityIsCompany( ipCompany ))
							{
								// Provide debug output
								TRACE( "Company = \"%s\"\r\n", strResult.c_str() );

								// Set Attribute Name field
								if (ePreviousType == kAliasNone)
								{
									// This Attribute is not an Alias
									ipAttr->Name = "Company";

									// Set pointer to Parent for possible Alias items
									ipParent = ipAttr;
								}
								else if ((ePreviousType == kAliasCompany) || (ePreviousType == kAliasPerson))
								{
									// This Attribute is an Alias
									ipAttr->Name = "CompanyAlias";
								}
								else
								{
									// This Attribute is a Related Company
									ipAttr->Name = "RelatedCompany";
								}
							}
							else
							{
								// Provide debug output
								TRACE( "Person = \"%s\"\r\n", strResult.c_str() );

								// Set Attribute Name field
								if (ePreviousType == kAliasNone)
								{
									// This Attribute is not an Alias
									ipAttr->Name = "Person";

									// Set pointer to Parent for possible Alias items
									ipParent = ipAttr;
								}
								else if (ePreviousType == kAliasCompany)
								{
									// This Attribute is an Alias
									ipAttr->Name = "CompanyAlias";
								}
								else
								{
									// This Attribute is an Alias
									ipAttr->Name = "PersonAlias";
								}

								// Split into First, Middle, Last
								ipPersonSplitter->SplitAttribute( ipAttr, ipAFDoc, NULL );
							}

							// Set Type field
							setTypeFromAlias( ipAttr, ePreviousType, lPreviousAliasItem );

							// Add the sub-attribute to the vector
							if (ipParent == ipAttr)
							{
								ipMainAttrSub->PushBack( ipAttr );	
							}
							// Add the Alias item to the Parent
							else
							{
								if (ipParent)
								{
									// Retrieve the sub-attributes
									IIUnknownVectorPtr ipSub = ipParent->SubAttributes;
									ASSERT_RESOURCE_ALLOCATION("ELI15555", ipSub != NULL);

									ipSub->PushBack( ipAttr );
								}
								else
								{
									ipMainAttrSub->PushBack( ipAttr );	
								}
							}
						}

						// Check for more Alias information
						if (bContinue)
						{
							// Copy leftover Entity information into ipCompany object
							ipCompany = ipExtra->GetSubString( 0, -1 );

							// Store Alias details
							ePreviousType = eNextType;
							lPreviousAliasItem = lNextAliasItem;
						}
						else
						{
							break;
						}
					}	// end while Aliases to be handled
				}		// end for each Company in collection
			}			// end else Company found

			// Reset the regular expression parser
			m_ipRegExprParser = NULL;
		}
		catch(...)
		{
			try
			{
				// Ensure the regular expression parser gets reset
				m_ipRegExprParser = NULL;
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29461");

			throw;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06643")
}

//-------------------------------------------------------------------------------------------------
// IEntityNameSplitter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitter::get_EntityAliasChoice(EEntityAliasChoice *pChoice)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve the setting
		*pChoice = m_eAliasChoice;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08662")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitter::put_EntityAliasChoice(EEntityAliasChoice newChoice)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Store the setting
		m_eAliasChoice = newChoice;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08663")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitter::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19563", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Split the name of a company or a person").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05347")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitter::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		UCLID_AFSPLITTERSLib::IEntityNameSplitterPtr ipSource( pObject );
		ASSERT_RESOURCE_ALLOCATION("ELI08682", ipSource != NULL);

		// Retrieve Alias Choice setting
		m_eAliasChoice = (EEntityAliasChoice) ipSource->GetEntityAliasChoice();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08683");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitter::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_EntityNameSplitter );
		ASSERT_RESOURCE_ALLOCATION( "ELI05348", ipObjCopy != NULL );

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05349");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitter::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_EntityNameSplitter;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitter::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// NOTES about versions:
// Version 1:
//   * Saved the version only.
// Version 2:
//   * Saves the VariantVector of CompanyClueType's
// Version 3:
//   * Saves the version only
// Version 4:
//   * Also saves the Alias choice
// Version 5:
//   * Also saves the Trust Name Exchange
STDMETHODIMP CEntityNameSplitter::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset data elements
		m_eAliasChoice = kIgnoreLaterEntities;
		m_bMoveTrustName = false;

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
			UCLIDException ue( "ELI07634", "Unable to load newer EntityNameSplitter." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Read an outdated VariantVector of clues
		if (nDataVersion == 2)
		{
			IPersistStreamPtr ipObj;
			readObjectFromStream(ipObj, pStream, "ELI09985");
			if (ipObj == NULL)
			{
				throw UCLIDException( "ELI05468", 
					"Company Clues vector could not be read from stream!" );
			}
//			m_ipCompanyClues = ipObj;
		}

		// Read alias choice
		if (nDataVersion > 3)
		{
			unsigned long ulTemp;
			dataReader >> ulTemp;
			m_eAliasChoice = (EEntityAliasChoice) ulTemp;
		}

		// Read Trust Name Exchange
		if (nDataVersion > 4)
		{
			dataReader >> m_bMoveTrustName;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05471");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitter::Save(IStream *pStream, BOOL fClearDirty)
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

		// Alias choice
		dataWriter << (unsigned long) m_eAliasChoice;

		// Trust Name Exchange
		dataWriter << m_bMoveTrustName;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05470");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitter::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitter::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
