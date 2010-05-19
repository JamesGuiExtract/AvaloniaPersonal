// LegalDescSplitter.cpp : Implementation of CLegalDescSplitter
#include "stdafx.h"
#include "AFSplitters.h"
#include "LegalDescSplitter.h"

#include <Common.h>
// the file contains specific tag names for AFDocument
#include <SpecialStringDefinitions.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <CommentedTextFileReader.h>
#include <EncryptedFileManager.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <misc.h>
#include <ComponentLicenseIDs.h>

#include <map>

using namespace std;

#define RANGE_SEPARATORS	"through|-|thru|\\bto\\b"
#define RANGE_PATTERN		"\\d+\\s*?(through|-|thru|\\bto\\b)\\s*?\\d+"

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CLegalDescSplitter
//-------------------------------------------------------------------------------------------------
CLegalDescSplitter::CLegalDescSplitter()
: m_bDirty(false),
m_ipAFUtility(NULL),
m_ipMiscUtils(NULL),
m_ipRegExParser(NULL),
m_ipLegalTypeToRuleSetMap(NULL)

{
}
//-------------------------------------------------------------------------------------------------
CLegalDescSplitter::~CLegalDescSplitter()
{
	try
	{
		m_ipMiscUtils = NULL;
		m_ipRegExParser = NULL;
		m_ipAFUtility = NULL;
		m_ipLegalTypeToRuleSetMap = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29462");
}

//-------------------------------------------------------------------------------------------------
// IInterfaceSupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescSplitter::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeSplitter,
		&IID_IPersistStream,
		&IID_ICopyableObject,
		&IID_ICategorizedComponent,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescSplitter::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// NOTES about versions:
// Version 1:
//   * Saves the version only.
STDMETHODIMP CLegalDescSplitter::Load(IStream * pStream)
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
			UCLIDException ue( "ELI07927", "Unable to load newer LegalDescSplitter." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07926");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescSplitter::Save(IStream * pStream, BOOL fClearDirty)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07924");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescSplitter::GetSizeMax(ULARGE_INTEGER * pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescSplitter::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_LegalDescSplitter;

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeSplitter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescSplitter::raw_SplitAttribute(IAttribute *pAttribute, IAFDocument *pAFDoc,
													IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		try
		{
			validateLicense();

			//Create local copies of  Attribute and subattribues
			IAttributePtr ipMainAttribute( pAttribute );
			ASSERT_RESOURCE_ALLOCATION("ELI08404", ipMainAttribute != NULL );
			IIUnknownVectorPtr ipMainAttrSub = ipMainAttribute->SubAttributes;
			ASSERT_RESOURCE_ALLOCATION("ELI08407", ipMainAttrSub != NULL );

			ISpatialStringPtr ipValue = ipMainAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI10427", ipValue != NULL );

			// Get a new regular expression parser
			m_ipRegExParser = getParser();

			// Fix up OCR errors for the keywords
			ipValue->Replace("(\\b.ot(s)?\\b|\\bL.t(s)?\\b|\\bLo.(s)\\b)(?=.{1,30}\\d)", 
				"LOT", VARIANT_FALSE, 0, m_ipRegExParser);
			ipValue->Replace("(\\b.nit|\\bU.it|\\bUn.t|\\bUni.(?=s?\\b))(?=.{1,30}\\d)", 
				"UNIT", VARIANT_FALSE, 0, m_ipRegExParser);
			ipValue->Replace("(\\b.uilding|\\bB.ilding|\\bBu.lding|\\bBui.ding|\\bBuil.ing|\\bBuild.ng|\\bBuildi.g|\\bBuildin.)(?=.{1,30}\\d)",
				"BUILDING", VARIANT_FALSE, 0, m_ipRegExParser);
			ipValue->Replace("(\\b.utlot|\\bO.tlot|\\bOu[\\s\\S]{0,4}?ot|\\bOutl.t|\\bOutlo.\\b)(?=.{1,30}\\d)", 
				"OUTLOT", VARIANT_FALSE, 0, m_ipRegExParser);
			ipValue->Replace("(\\b.lock\\b|\\bB.ock\\b|\\bBl.ck\\b|\\bBlo.k\\b|\\bBloc.\\b)(?=.{1,30}\\d)", 
				"BLOCK", VARIANT_FALSE, 0, m_ipRegExParser);
			ipValue->Replace("\\b1N\\b", "IN", VARIANT_FALSE, 0, m_ipRegExParser);

			// Remove Colons and Semicolons
			ipValue->Replace("[;:]", "", VARIANT_FALSE, 0, m_ipRegExParser);

			processLegal ("any", ipMainAttribute->Value, ipMainAttrSub );

			// Reset the regular expression parser
			m_ipRegExParser = NULL;
		}
		catch(...)
		{
			try
			{
				// Ensure the regular expression parser is reset
				m_ipRegExParser = NULL;
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29463");

			throw;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07962");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescSplitter::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19564", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Split a legal description").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07920")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescSplitter::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescSplitter::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_LegalDescSplitter );
		ASSERT_RESOURCE_ALLOCATION("ELI08399", ipObjCopy != NULL );

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07922");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescSplitter::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Helper Functions
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI08056", "Legal Description Splitter" );
}
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::processAFDcoument(IAFDocumentPtr& ipAFDoc)
{
	ASSERT_ARGUMENT("ELI08409", ipAFDoc != NULL );

	// first check to see if the document has already been processed
	IStrToStrMapPtr ipStringTags(ipAFDoc->StringTags);
	// if string tags doesn't contain "DocProbability", 
	// then the AFDoc needs to be processed
	if (ipStringTags->Size == 0 
		||ipStringTags->Contains(_bstr_t(DOC_PROBABILITY.c_str())) == VARIANT_FALSE)
	{
		// use county document classifier to distinguish the document type
		if (m_ipDocPreprocessor == NULL)
		{
			m_ipDocPreprocessor.CreateInstance(CLSID_DocumentClassifier);
			ASSERT_RESOURCE_ALLOCATION("ELI08049", m_ipDocPreprocessor != NULL);
			// set industry category name
			IDocumentClassifierPtr ipDocClassifier(m_ipDocPreprocessor);
			ASSERT_RESOURCE_ALLOCATION("ELI08408", ipDocClassifier != NULL );
			ipDocClassifier->IndustryCategoryName = "Legal Descriptions";
		}
		m_ipDocPreprocessor->Process(ipAFDoc, NULL);
	}
}
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::processLegal( string strLegalType, ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipSubAttributes)
{
	ASSERT_ARGUMENT("ELI07973", ipInputText != NULL );
	ASSERT_ARGUMENT("ELI07974", ipSubAttributes != NULL );
	
	// get the input string from the spatial string
	_bstr_t _bstrText(ipInputText->String);

	// Load Regular expressions for finding valid location strings
	string strLocationPattern = getRegExpForType( "COMMON", "Location.dat.etf" );
	string strExBetweenPattern = getRegExpForType( "COMMON", "BetweenLocations.dat.etf" );
	string strLocationInclude = getRegExpForType( "COMMON", "LocationInclude.dat.etf" );
	string strIncBetweenPattern = getRegExpForType ("COMMON", "IncBetweenLocations.dat.etf");
	
	// Waukesha doesn't appear to need this 
	string strAlwaysKeepRegExp = getRegExpForType ("COMMON", "AlwaysKeepLocation.dat.etf");
	string strExcludeRegionExp = getRegExpForType ("COMMON", "ExcludeDetailed.dat.etf");
	
	IIUnknownVectorPtr ipLocationStrings = getFoundStrings( ipInputText, strLocationPattern, strExBetweenPattern, 
															strIncBetweenPattern, strLocationInclude, strAlwaysKeepRegExp, strExcludeRegionExp, true );

	IIUnknownVectorPtr ipMuniFound (CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION( "ELI10213", ipMuniFound != NULL );
	// Find Muni's
	// TODO: if multiple found they should be put with the correct location
	processMuni(ipInputText, ipMuniFound );
	
	// Add each found location to the subAttribute list 
 	long nNumLocations = ipLocationStrings->Size();
	if ( nNumLocations == 0 )
	{
		strLocationPattern = getRegExpForType ( "COMMON", "Location2.dat.etf");
		ipLocationStrings = getFoundStrings( ipInputText, strLocationPattern, strExBetweenPattern, 
											strIncBetweenPattern, strLocationInclude, "", strExcludeRegionExp  );
		nNumLocations = ipLocationStrings->Size();
	}
	if ( nNumLocations > 0 )
	{
		for ( int i = 0; i < nNumLocations; i++ )
		{
			ISpatialStringPtr ipLocation = ipLocationStrings->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI08410", ipLocation != NULL );
			if (ipLocation )
			{	
				//Create a AFDocument for processing the Attribute
				IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
				ASSERT_RESOURCE_ALLOCATION("ELI07953", ipAFDoc != NULL );
				ipAFDoc->Text = ipLocation;

				// first process the AFDocument to get proper tags for the document
				processAFDcoument(ipAFDoc);

				// given that the document has been processed, by this time,
				// the string tags collection MUST ABSOLUTELY contain 
				// the DOC PROBABILITY tag, otherwise somthing's wrong with
				// our logic.
				IStrToStrMapPtr ipStringTags(ipAFDoc->StringTags);
				ASSERT_RESOURCE_ALLOCATION("ELI07954", ipStringTags != NULL);
				if (ipStringTags->Contains(DOC_PROBABILITY.c_str()) == VARIANT_FALSE)
				{
					// something wrong in our program logic
					THROW_LOGIC_ERROR_EXCEPTION("ELI07955");
				}

				// get the object tags associated with the document
				IStrToObjectMapPtr ipObjTags(ipAFDoc->ObjectTags);
				ASSERT_RESOURCE_ALLOCATION("ELI07956", ipObjTags != NULL);

				// check to see if a string tag for the document type
				// exists.  If not, do not split
				if (ipObjTags->Contains(DOC_TYPE.c_str()) == VARIANT_FALSE)
				{
					// TODO: should set type to UNKNOWN and return
					//return; //S_OK;
					continue;
				}

				// get the vector of document type names
				IVariantVectorPtr ipVecDocTypes = ipObjTags->GetValue(_bstr_t(DOC_TYPE.c_str()));
				ASSERT_RESOURCE_ALLOCATION("ELI07958", ipVecDocTypes != NULL);
			
				// Process each document type found if none the Legal doesn't get processed
				long nNumDocTypes = ipVecDocTypes->Size;
				for ( int i = 0; i < nNumDocTypes; i++ )
				{
					// get the string value for the document type tag
					string strDocType = asString(_bstr_t(ipVecDocTypes->GetItem(i)));
					if ( strDocType == "SUB-BLO" || strDocType == "CONDO-U" 
						|| strDocType == "PLS-L" || strDocType == "CSM" )
					{
				
						// create an attribute to store the value
						IAttributePtr ipAttribute(CLSID_Attribute);
						ASSERT_RESOURCE_ALLOCATION("ELI07978", ipAttribute != NULL);

						// set the match as the value of the attribute
						ipAttribute->Value = ipLocation;
						ipAttribute->Name = "Location";

						//Add type sub attribute under Location
						// Create local copy of Location subattributes
						IIUnknownVectorPtr ipLocationSubAttr(ipAttribute->SubAttributes);
						ASSERT_RESOURCE_ALLOCATION("ELI07980", ipLocationSubAttr != NULL );
				
						//Create type sub attribute
						//IAttributePtr ipLegalDescType (CLSID_Attribute);
						//ASSERT_RESOURCE_ALLOCATION("ELI07960", ipLegalDescType != NULL );
				
						//Set Types Values and name
						//ipLegalDescType->Name = "Type";
						//ISpatialStringPtr ipTypeStr(CLSID_SpatialString);
						//ASSERT_RESOURCE_ALLOCATION("ELI07961", ipTypeStr != NULL );
						//ipTypeStr->String = strDocType.c_str();
						//ipLegalDescType->Value = ipTypeStr;
				
						//Add to the Sub Attribute List
						//ipLocationSubAttr->PushBack( ipLegalDescType );
				
						if ( strDocType == "SUB-BLO" )
						{
							// Process the parts of a location of type SUB-BLO
							processSubBLOLocation( ipAttribute->Value, ipLocationSubAttr );
							ipAttribute->Type = "SUB_BLO";
						}
						else if (strDocType == "CONDO-U")
						{
							processCondoULocation ( ipAttribute->Value, ipLocationSubAttr );
							ipAttribute->Type = "CONDO_U";
						}
						else if ( strDocType == "PLS-L")
						{
							processPLSLocation ( ipAttribute->Value, ipLocationSubAttr );
							ipAttribute->Type = "PLS_L";
						}
						else if ( strDocType == "CSM")
						{
							processCSMLocation( ipAttribute->Value, ipLocationSubAttr );
							ipAttribute->Type = "CSM_L";
						}
						// Assume one Muni found and put it on all found locations
						// TODO: Put Muni found with correct Location found
						int nNumMuni = ipMuniFound->Size();

						if ( nNumMuni > 0 )
						{
							// set to the first one that has the most sub attributes
							IAttributePtr ipSelectedMuni;
							int nMax = 0;

							for ( int k = 0; k < nNumMuni; k++ )
							{
								// Retrieve this Attribute
								IAttributePtr ipCurrMuni = ipMuniFound->At(k);
								ASSERT_RESOURCE_ALLOCATION("ELI15556", ipCurrMuni != NULL );

								// Retrieve collected sub-attributes
								IIUnknownVectorPtr ipSub = ipCurrMuni->SubAttributes;
								ASSERT_RESOURCE_ALLOCATION("ELI15557", ipSub != NULL);

								// Check sub-attribute count
								int nNumSubAttrs = ipSub->Size();
								if ( nNumSubAttrs > nMax )
								{
									// Make copy of this Attribute
									ICopyableObjectPtr ipObj = ipCurrMuni;
									ASSERT_RESOURCE_ALLOCATION("ELI15558", ipObj != NULL);
									ipSelectedMuni = ipObj->Clone();
									ASSERT_RESOURCE_ALLOCATION("ELI15559", ipSelectedMuni != NULL);

									// Update maximum count
									nMax = nNumSubAttrs;
								}
							}

							if ( nMax == 0 )
							{
								ICopyableObjectPtr ipObj = ipMuniFound->At(0);
								ipSelectedMuni = ipObj->Clone();
							}

							ipLocationSubAttr->PushBack(ipSelectedMuni);
						}
						// Add the attribute to the result vector
						//ipSubAttributes->PushBack(ipAttribute);
						reTypeAttribute( ipAttribute, ipSubAttributes);
					}
					else
					{
						// Nothing implemented
						THROW_LOGIC_ERROR_EXCEPTION("ELI08177");
					}
				}
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::processSubBLOLocation( ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipSubAttributes)
{

	// get the input string from the spatial string
	_bstr_t _bstrText(ipInputText->String);

	// Setup subdivision string for building by removing found lot and block parts
	ISpatialStringPtr ipSubdivision(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI08059", ipSubdivision != NULL );
	ICopyableObjectPtr ipObjectTo = ipSubdivision;
	ASSERT_RESOURCE_ALLOCATION("ELI08411", ipObjectTo != NULL );
	ipObjectTo->CopyFrom ( ipInputText );

	// Load Block regular expression strings
	string strBlockPart = getRegExpForType ("SUB-BLO", "BlockPart.dat.etf");
	string strBetweenBlockExp = getRegExpForType("SUB-BLO", "BetweenBlockParts.dat.etf");
	string strNumberLetterExp = getRegExpForType( "COMMON", "NumberLetter.dat.etf");

	// Find valid block segments
	IIUnknownVectorPtr ipBlockParts = getFoundStrings (ipInputText, strBlockPart, strBetweenBlockExp );

	// Get BeforeBlock string
	string strBeforeBlock = getRegExpForType("SUB-BLO", "BeforeBlock.dat.etf" );

	IIUnknownVectorPtr ipResultBlocks = 
			processValueParts( ipSubdivision, ipBlockParts, strNumberLetterExp, strBeforeBlock );

	// Add Block values as separate attributes
	addAsAttributes( "Block", ipResultBlocks, ipSubAttributes, true );

	//Get LotPart Reg Exp form file
	string strLotNumberLetterExp = strNumberLetterExp + "|\\bfeet\\b|\\bpart\\b|\\ball\\b|except[\\s\\S]+?(feet|thereof)";
	string strLotPart = getRegExpForType("SUB-BLO", "LotPart.dat.etf");
	string strBetweenLotExp = getRegExpForType("SUB-BLO", "BetweenLotParts.dat.etf");
	string strIncBetweenLotParts = getRegExpForType("SUB-BLO", "IncBetweenLotParts.dat.etf");
	//Get the lot parts from the value
	IIUnknownVectorPtr ipLotParts = getFoundStrings(ipInputText, strLotPart, strBetweenLotExp, strIncBetweenLotParts);
	IIUnknownVectorPtr ipResultLots =
			processValueParts( ipSubdivision, ipLotParts, strLotNumberLetterExp, strBeforeBlock );

	// Create vector for Full and Partial values
	IIUnknownVectorPtr ipFullValues (CLSID_IUnknownVector );
	IIUnknownVectorPtr ipPartValues (CLSID_IUnknownVector );
	// Divide the results into Full and Partial Values
	separateFullAndPartial( ipResultLots, ipFullValues, ipPartValues );
	// Consolidate the ranges and remove duplicates for the Full Values
	IIUnknownVectorPtr ipConsolidatedLots = consolidateValues ( ipFullValues );
	// Add Values as Lot with type FULL
	addAsAttributes ( "Lot", ipConsolidatedLots, ipSubAttributes, false, "", false, "FULL" );
	// Consolidate the ranges adn remove duplicates for the Partial Values
	ipConsolidatedLots = consolidateValues ( ipPartValues );
	// Add Values as Lot with Type Part
	addAsAttributes ( "Lot", ipConsolidatedLots, ipSubAttributes, false, "", false, "PART" );

	//Get OutLotPart Reg Exp form file
	string strOutlotPart = getRegExpForType("SUB-BLO", "OutlotPart.dat.etf");
	string strBetweenOutlotExp = getRegExpForType("SUB-BLO", "BetweenOutlotParts.dat.etf");
	//Get the lot parts from the value
	IIUnknownVectorPtr ipOutlotParts = getFoundStrings(ipInputText, strOutlotPart, strBetweenOutlotExp);
	IIUnknownVectorPtr ipResultOutlots =
			processValueParts ( ipSubdivision, ipOutlotParts, strLotNumberLetterExp, strBeforeBlock );

	// Create vector for Full and Partial values
	ipFullValues.CreateInstance (CLSID_IUnknownVector );
	ipPartValues.CreateInstance (CLSID_IUnknownVector );
	// Divide the results into Full and Partial Values
	separateFullAndPartial( ipResultOutlots, ipFullValues, ipPartValues );
	// Consolidate the ranges and remove duplicates for the Full Values
	ipConsolidatedLots = consolidateValues ( ipFullValues );
	// Add Values as Outlot with type FULL
	addAsAttributes ( "Outlot", ipConsolidatedLots, ipSubAttributes, false, "", false, "FULL" );
	// Consolidate the ranges adn remove duplicates for the Partial Values
	ipConsolidatedLots = consolidateValues ( ipPartValues );
	// Add Values as Outlot with Type Part
	addAsAttributes ( "Outlot", ipConsolidatedLots, ipSubAttributes, false, "", false, "PART" );

	// Add Subdivision sub attribute if it is not zero
	if ( ipSubdivision->IsEmpty() == VARIANT_FALSE )
	{
		// create an attribute to store the value
		IAttributePtr ipAttribute(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI19161", ipAttribute != NULL);

		// set the match as the value of the attribute, and add
		// the attribute to the result vector
		ipAttribute->Value = applyModifiers( "SUB-BLO", "Subdivision", ipSubdivision);
		ipAttribute->Name = "Subdivision";
		ipSubAttributes->PushBack(ipAttribute);
	}

}
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::processCondoULocation( ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipSubAttributes)
{
	ASSERT_ARGUMENT("ELI08412", ipInputText != NULL );
	ASSERT_ARGUMENT("ELI08413", ipSubAttributes != NULL );
	
	// get the input string from the spatial string
	_bstr_t _bstrText(ipInputText->String);

	// Setup Condominium string for building by removing found Unit and Building parts
	ISpatialStringPtr ipCondominium(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI19121", ipCondominium != NULL );
	ICopyableObjectPtr ipObjectTo = ipCondominium;
	ASSERT_RESOURCE_ALLOCATION("ELI08414", ipObjectTo != NULL );
	ipObjectTo->CopyFrom ( ipInputText );

	// Load Building regular expression strings
	string strBuildingPart = getRegExpForType ("CONDO-U", "BuildingPart.dat.etf");
	string strBetweenBuildingExp = getRegExpForType("CONDO-U", "BetweenBuildingParts.dat.etf");
	string strNumberLetterExp = getRegExpForType( "COMMON", "NumberLetter.dat.etf");

	// Find valid Building segments
	IIUnknownVectorPtr ipBuildingParts = getFoundStrings (ipInputText, strBuildingPart, strBetweenBuildingExp );

	// Get BeforeBuilding string
	string strBeforeBuilding = getRegExpForType("CONDO-U", "BeforeBuilding.dat.etf" );

	IIUnknownVectorPtr ipResultBuildings = 
			processValueParts( ipCondominium, ipBuildingParts, strNumberLetterExp, strBeforeBuilding );

	// Add Building values as separate attributes
	addAsAttributes( "Building", ipResultBuildings, ipSubAttributes, true );

		//Get UnitPart Reg Exp form file
	string strUnitPart = getRegExpForType("CONDO-U", "UnitPart.dat.etf");
	string strBetweenUnitExp = getRegExpForType("CONDO-U", "BetweenUnitParts.dat.etf");
	string strIncBetweenUnitParts = getRegExpForType("CONDO-U", "IncBetweenUnitParts.dat.etf");
	//Get the Unit parts from the value
	IIUnknownVectorPtr ipUnitParts = getFoundStrings(ipInputText, strUnitPart, strBetweenUnitExp, strIncBetweenUnitParts);
	IIUnknownVectorPtr ipResultUnits =
			processValueParts( ipCondominium, ipUnitParts, strNumberLetterExp, "" );

	//Add all Unit strings as one value separated by ,
	//TODO: Remove Duplicates and order them numerically
	addAsAttributes ( "Unit", ipResultUnits, ipSubAttributes, false);

	string strCarPortPart = getRegExpForType("CONDO-U", "CarPortPart.dat.etf");
	string strCarPortNumber = getRegExpForType("CONDO-U", "CarPortLetterNumber.dat.etf");
	//Get the Unit parts from the value
	IIUnknownVectorPtr ipCarPortParts = getFoundStrings(ipInputText, strCarPortPart );
	IIUnknownVectorPtr ipResultCarPorts =
			processValueParts( ipCondominium, ipCarPortParts, strCarPortNumber, "" );

	addAsAttributes ( "CarPort", ipResultCarPorts, ipSubAttributes, true);


	// Add Condominium sub attribute if it is not zero
	if ( ipCondominium->IsEmpty() == VARIANT_FALSE )
	{
		// create an attribute to store the value
		IAttributePtr ipAttribute(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI19451", ipAttribute != NULL);

		// set the match as the value of the attribute, and add
		// the attribute to the result vector
		ipAttribute->Value = applyModifiers("CONDO-U", "Condominium", ipCondominium );
		ipAttribute->Name = "Condominium";
		ipSubAttributes->PushBack(ipAttribute);
	}

}
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::processPLSLocation( ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipSubAttributes)
{
	ASSERT_ARGUMENT("ELI08478", ipInputText != NULL );
	ASSERT_ARGUMENT("ELI08479", ipSubAttributes != NULL );

	// get the input string from the spatial string
	_bstr_t _bstrText(ipInputText->String);

	// Setup subdivision string for building by removing found lot and block parts
	ISpatialStringPtr ipRemaining(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI08504", ipRemaining != NULL );
	ICopyableObjectPtr ipObjectTo = ipRemaining;
	ASSERT_RESOURCE_ALLOCATION("ELI08505", ipObjectTo != NULL );
	ipObjectTo->CopyFrom ( ipInputText );


	// Load Block regular expression strings
	string strSectionPartExp = getRegExpForType ("PLS-L", "SectionPart.dat.etf");
	string strTownPartExp = getRegExpForType("PLS-L", "TownPart.dat.etf");
	string strRangePartExp = getRegExpForType("PLS-L", "RangePart.dat.etf");
	string strValueDirectionExp = getRegExpForType( "PLS-L", "ValueDirection.dat.etf");
	string strNumberLetterExp = getRegExpForType ("COMMON", "NumberLetter.dat.etf");
	string strQuarterPartExp = getRegExpForType( "PLS-L", "QuarterPart.dat.etf");
	string strQuartersExp = getRegExpForType( "PLS-L", "Quarters.dat.etf");

	//Get LotPart Reg Exp form file
	string strLotNumberLetterExp = strNumberLetterExp + "|\\bfeet\\b|\\bpart\\b|\\ball\\b";
	string strLotPart = getRegExpForType("SUB-BLO", "LotPart.dat.etf");
	string strBetweenLotExp = getRegExpForType("SUB-BLO", "BetweenLotParts.dat.etf");
	string strIncBetweenLotParts = getRegExpForType("SUB-BLO", "IncBetweenLotParts.dat.etf");
	//Get the lot parts from the value
	IIUnknownVectorPtr ipLotParts = getFoundStrings(ipInputText, strLotPart, strBetweenLotExp, strIncBetweenLotParts);
	IIUnknownVectorPtr ipResultLots =
			processValueParts( ipRemaining, ipLotParts, strLotNumberLetterExp, "" );

	// Create vector for Full and Partial values
	IIUnknownVectorPtr ipFullValues (CLSID_IUnknownVector );
	IIUnknownVectorPtr ipPartValues (CLSID_IUnknownVector );
	// Divide the results into Full and Partial Values
	separateFullAndPartial( ipResultLots, ipFullValues, ipPartValues );
	// Consolidate the ranges and remove duplicates for the Full Values
	IIUnknownVectorPtr ipConsolidatedLots = consolidateValues ( ipFullValues );
	// Add Values as Lot with type FULL
	addAsAttributes ( "Lot", ipConsolidatedLots, ipSubAttributes, false, "", false, "FULL" );
	// Consolidate the ranges adn remove duplicates for the Partial Values
	ipConsolidatedLots = consolidateValues ( ipPartValues );
	// Add Values as Lot with Type Part
	addAsAttributes ( "Lot", ipConsolidatedLots, ipSubAttributes, false, "", false, "PART" );

	// Find valid block segments
	IIUnknownVectorPtr ipQuarters = getFoundStrings (ipRemaining, strQuarterPartExp);
	IIUnknownVectorPtr ipResultQuarters = 
			processValueParts( NULL , ipQuarters, strQuartersExp, "" );
	// Add Section values as separate attributes
	addAsAttributes( "Quarters", ipResultQuarters, ipSubAttributes, false, "PLS-L", true );


	// Find valid block segments
	IIUnknownVectorPtr ipSectionParts = getFoundStrings (ipRemaining, strSectionPartExp );
	IIUnknownVectorPtr ipResultSections = 
			processValueParts( NULL , ipSectionParts, strNumberLetterExp, "" );
	// Add Section values as separate attributes
	addAsAttributes( "Section", ipResultSections, ipSubAttributes, true, "PLS-L", true );

	// Find valid block segments
	IIUnknownVectorPtr ipTownParts = getFoundStrings (ipRemaining, strTownPartExp );
	IIUnknownVectorPtr ipResultTowns = 
			processValueParts( NULL , ipTownParts, strValueDirectionExp, "" );

	// Add Section values as separate attributes
	splitAndAddTown( ipResultTowns, ipSubAttributes );
	//addAsAttributes( "Town", ipResultTowns, ipSubAttributes, true );

	// Find valid block segments
	IIUnknownVectorPtr ipRangeParts = getFoundStrings (ipRemaining, strRangePartExp );
	IIUnknownVectorPtr ipResultRanges = 
			processValueParts( NULL , ipRangeParts, strValueDirectionExp, "" );
	// Add Section values as separate attributes
	splitAndAddRange( ipResultRanges, ipSubAttributes );
	//addAsAttributes( "Range", ipResultRanges, ipSubAttributes, true );

}
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::processCSMLocation( ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipSubAttributes)
{

	// get the input string from the spatial string
	_bstr_t _bstrText(ipInputText->String);

	// Load Block regular expression strings
	string strBlockPart = getRegExpForType ("CSM", "BlockPart.dat.etf");
	string strNumberLetterExp = getRegExpForType( "COMMON", "NumberLetter.dat.etf");
	

	// Find valid block segments
	IIUnknownVectorPtr ipBlockParts = getFoundStrings (ipInputText, strBlockPart, "" );
	IIUnknownVectorPtr ipResultBlocks = 
			processValueParts( NULL, ipBlockParts, strNumberLetterExp, "" );

	// Add Block values as separate attributes
	addAsAttributes( "Block", ipResultBlocks, ipSubAttributes, true );

	//Get LotPart Reg Exp form file
	string strLotPart = getRegExpForType("CSM", "LotPart.dat.etf");
	string strBetweenLotExp = getRegExpForType("CSM", "BetweenLotParts.dat.etf");
	string strIncBetweenLotParts = getRegExpForType("CSM", "IncBetweenLotParts.dat.etf");

	//Get the lot parts from the value
	string strLotNumberLetterExp = strNumberLetterExp + "|\\bfeet\\b|\\bpart\\b|\\ball\\b";
	IIUnknownVectorPtr ipLotParts = getFoundStrings(ipInputText, strLotPart, strBetweenLotExp, strIncBetweenLotParts);
	IIUnknownVectorPtr ipResultLots =
			processValueParts( NULL, ipLotParts, strLotNumberLetterExp, "" );

	// Create vector for Full and Partial values
	IIUnknownVectorPtr ipFullValues (CLSID_IUnknownVector );
	IIUnknownVectorPtr ipPartValues (CLSID_IUnknownVector );
	// Divide the results into Full and Partial Values
	separateFullAndPartial( ipResultLots, ipFullValues, ipPartValues );
	// Consolidate the ranges and remove duplicates for the Full Values
	IIUnknownVectorPtr ipConsolidatedLots = consolidateValues ( ipFullValues );
	// Add Values as Lot with type FULL
	addAsAttributes ( "Lot", ipConsolidatedLots, ipSubAttributes, false, "", false, "FULL" );
	// Consolidate the ranges adn remove duplicates for the Partial Values
	ipConsolidatedLots = consolidateValues ( ipPartValues );
	// Add Values as Lot with Type Part
	addAsAttributes ( "Lot", ipConsolidatedLots, ipSubAttributes, false, "", false, "PART" );

	//Get OutLotPart Reg Exp form file
	string strOutlotPart = getRegExpForType("CSM", "OutlotPart.dat.etf");
	string strBetweenOutlotExp = getRegExpForType("CSM", "BetweenOutlotParts.dat.etf");
	//Get the lot parts from the value
	IIUnknownVectorPtr ipOutlotParts = getFoundStrings(ipInputText, strOutlotPart, strBetweenOutlotExp);
	IIUnknownVectorPtr ipResultOutlots =
			processValueParts ( NULL, ipOutlotParts, strLotNumberLetterExp, "" );

	// Create vector for Full and Partial values
	ipFullValues.CreateInstance (CLSID_IUnknownVector );
	ipPartValues.CreateInstance (CLSID_IUnknownVector );
	// Divide the results into Full and Partial Values
	separateFullAndPartial( ipResultOutlots, ipFullValues, ipPartValues );
	// Consolidate the ranges and remove duplicates for the Full Values
	ipConsolidatedLots = consolidateValues ( ipFullValues );
	// Add Values as Lot with type FULL
	addAsAttributes ( "Outlot", ipConsolidatedLots, ipSubAttributes, false, "", false, "FULL" );
	// Consolidate the ranges adn remove duplicates for the Partial Values
	ipConsolidatedLots = consolidateValues ( ipPartValues );
	// Add Values as Lot with Type Part
	addAsAttributes ( "Outlot", ipConsolidatedLots, ipSubAttributes, false, "", false, "PART" );


	//Get LotPart Reg Exp form file
	string strCSMPart = getRegExpForType("CSM", "CSMPart.dat.etf");

	//Get the lot parts from the value
	IIUnknownVectorPtr ipCSMParts = getFoundStrings(ipInputText, strCSMPart, "", "");
	IIUnknownVectorPtr ipResultCSM =
			processValueParts( NULL, ipCSMParts, strNumberLetterExp, "" );

	//Add all lot strings as one value separated by ,
	//TODO: Remove Duplicates and order them numerically
	addAsAttributes ( "CSM_Number", ipResultCSM, ipSubAttributes, false);

}
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::processMuni( ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipSubAttributes)
{
	ASSERT_ARGUMENT("ELI08073", ipInputText != NULL );
	ASSERT_ARGUMENT("ELI08074", ipSubAttributes != NULL );

	// Setup the ruleset for finding the Municipality string and subattributes
	setupMuniRuleSet();

	// Setup working String so preprocessors have no effect on string
	ISpatialStringPtr ipWorkValue(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI08402", ipWorkValue != NULL );
	ICopyableObjectPtr ipObjectTo = ipWorkValue;
	ASSERT_RESOURCE_ALLOCATION("ELI08415", ipObjectTo != NULL );
	ipObjectTo->CopyFrom ( ipInputText );

	// Create AF Document for splitting
	IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
	ASSERT_RESOURCE_ALLOCATION("ELI08295", ipAFDoc != NULL );
	ipAFDoc->Text = ipWorkValue;

	IVariantVectorPtr ipvecAttributeNames(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI08296", ipvecAttributeNames != NULL );

	// Add the Attribute name to find
	ipvecAttributeNames->PushBack("Municipality");
	IIUnknownVectorPtr ipAttributes = m_ipMuniRuleSet->ExecuteRulesOnText( ipAFDoc, ipvecAttributeNames, NULL );
	
	//Determine if more than one type and rename Village, City, or Town Subattributes	
	// Holds the shared attributes
	IIUnknownVectorPtr ipSharedAttrs ( CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION("ELI08305", ipSharedAttrs != NULL );
	
	// Holds the each of the Attributes with the name of Village, City, or Town
	IIUnknownVectorPtr ipNameTypes ( CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION("ELI08307", ipNameTypes != NULL );

	// Number of Municipality attributes found
	long nNumAttr = ipAttributes->Size();
	for ( int i = 0; i < nNumAttr; i++ )
	{
		// Clear Prev iterations vectors
		ipSharedAttrs->Clear();
		ipNameTypes->Clear();

		// Current found attribute to work with
		IAttributePtr ipCurrAttr = ipAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI08416", ipCurrAttr != NULL );

		// Separate the Town, Village, and city attributes from the others
		IIUnknownVectorPtr ipCurrSubAttrs ( ipCurrAttr->SubAttributes );
		long nNumSubAttrs = ipCurrSubAttrs->Size();
		int c;
		for ( c = 0; c < nNumSubAttrs; c++ )
		{
			IAttributePtr ipCurrSubAttr = ipCurrSubAttrs->At(c);
			ASSERT_RESOURCE_ALLOCATION("ELI08417", ipCurrSubAttr != NULL );
			string strName = ipCurrSubAttr->Name;
			// If the attribute name is VillageOf, TownOf or CityOf these will be used for the type
			// of municipality and not be output as sub attributes.
			// NOTE: Thes are the attribute names specified in the MuniSplit.rsd file
			// PVCS: FlexIDSCore #4097
			if ((strName == "VillageOf" ) || (strName == "TownOf") || (strName == "CityOf"))
			{
				ipNameTypes->PushBack(ipCurrSubAttr);
			}
			else
			{
				ipSharedAttrs->PushBack(ipCurrSubAttr);
			}
		}

		// for each of the Town, Village, and cities found add separate municipality Attribute
		// to vector of split attributes
		long nNumNameTypes = ipNameTypes->Size();
		for ( c = 0; c < nNumNameTypes; c++ )
		{

			// Build the type Attribute
			IAttributePtr ipNameAttr = ipNameTypes->At(c);
			ASSERT_RESOURCE_ALLOCATION("ELI08418", ipNameAttr != NULL );
			// Create new Municipality object
			IAttributePtr ipCurrMuni = createAttribute( string(ipCurrAttr->Name), ipNameAttr->Value );
			ipCurrMuni->Type = ipNameAttr->Name;
			ipCurrSubAttrs = ipCurrMuni->SubAttributes;

			//IAttributePtr ipTypeAttr = createAttribute( "Type", string(ipNameAttr->Name) );

			// Rename the Name attribute found
			//ipNameAttr->Name = "Name";
			
			// Add to the Sub Attributes vector
			//ipCurrSubAttrs->PushBack( ipTypeAttr );
			//ipCurrSubAttrs->PushBack( ipNameAttr );
			ipCurrSubAttrs->Append( ipSharedAttrs );
			
			// Add to the split Subattributes vector
			ipSubAttributes->PushBack ( ipCurrMuni );
		}
	}		
}
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::addAsAttributes( string strAttributeName, 
										 IIUnknownVectorPtr ipAttributeStrings, 
										 IIUnknownVectorPtr ipAttributeList, 
										 bool bAsMultiple, 
										 string strLegalType,
										 bool bApplyModifiers,
										 string strAttrType)
{
	ASSERT_ARGUMENT ( "ELI08050", ipAttributeStrings != NULL );
	ASSERT_ARGUMENT ( "ELI08051", ipAttributeList != NULL );
	
	long nSize = ipAttributeStrings->Size();
	if ( nSize == 0 )
	{
		// nothing to do
		return;
	}
	if ( bAsMultiple )
	{
		// Add attribute to list for each of the strings in ipAttributeStrings with the name strAttributeName
		for ( int i = 0; i < nSize; i++ )
		{
			// create an attribute to store the value
			IAttributePtr ipAttribute(CLSID_Attribute);
			ASSERT_RESOURCE_ALLOCATION("ELI08052", ipAttribute != NULL);

			// set the match as the value of the attribute, and add
			// the attribute to the result vector
			ISpatialStringPtr ipValue = ipAttributeStrings->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI08460", ipValue != NULL );
			// Trim leading and trailing " marks
			ipValue->Trim( "\"", "\"" );	
			if ( bApplyModifiers )
			{
				ipAttribute->Value = applyModifiers( strLegalType , strAttributeName,  ipValue );
			}
			else
			{
				ipAttribute->Value = ipValue;
			}
			if ( strAttrType != "" )
			{
				ipAttribute->Type = _bstr_t( strAttrType.c_str() );
			}
			ipAttribute->Name = _bstr_t(strAttributeName.c_str());
			ipAttributeList->PushBack(ipAttribute);
			//string strValue = ipValue->String;
			//MessageBox(NULL, strValue.c_str(), "The Value", MB_OK );
		}
	}
	else
	{
		// Build string to add single attribute to ipAttributeList
		ISpatialStringPtr ipSingleStr(CLSID_SpatialString);
		for ( int i = 0; i < nSize; i++ )
		{
			// Get current string
			ISpatialStringPtr ipCurrentStr = ipAttributeStrings->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI08419", ipCurrentStr != NULL );
			// Trim leading and trailing " marks
			ipCurrentStr->Trim( "\"", "\"" );
			if ( ipCurrentStr )
			{
				if ( i != 0 )
				{
					// if it is not the first add the separator before the string
					ipSingleStr->AppendString(_bstr_t(","));
				}
				if ( bApplyModifiers )
				{
					ipSingleStr->Append( applyModifiers( strLegalType, strAttributeName, ipCurrentStr ));
				}
				else
				{
					ipSingleStr->Append( ipCurrentStr );
				}
			}
		}
		// create an attribute to store the value
		IAttributePtr ipAttribute(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI08053", ipAttribute != NULL);

		// set the match as the value of the attribute, and add
		// the attribute to the result vector
		ipAttribute->Value = ipSingleStr;
		ipAttribute->Name = _bstr_t(strAttributeName.c_str());
		if ( strAttrType != "" )
		{
			ipAttribute->Type = _bstr_t( strAttrType.c_str() );
		}
		
		ipAttributeList->PushBack(ipAttribute);
	}
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CLegalDescSplitter::getFoundStrings( ISpatialStringPtr ipInputText, 
													   string strFindRegExp, 
													   string strExBetweenRegExp,
													   string strIncBetweenRegExp,
													   string strIncludeRegExp,
													   string strAlwaysKeepRegExp,
													   string strExcludeRegionExp,
													   bool bExcludeAfter1stInitially  )
{
	ASSERT_ARGUMENT("ELI08420", ipInputText != NULL );
	// get the input string from the spatial string
	_bstr_t _bstrText(ipInputText->String);

	// use the regular expression engine to parse the text and find attribute values
	// matching the specified regular expression
	m_ipRegExParser->Pattern = _bstr_t(strFindRegExp.c_str());
	m_ipRegExParser->IgnoreCase = VARIANT_TRUE;
	IIUnknownVectorPtr ipMatches = m_ipRegExParser->Find(_bstrText, VARIANT_FALSE, VARIANT_FALSE);
	IIUnknownVectorPtr ipReturnStrings(CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI08057", ipReturnStrings != NULL );

	m_ipRegExParser->Pattern = _bstr_t(strExcludeRegionExp.c_str());
	IIUnknownVectorPtr ipExcludeRegions;
	if ( strExcludeRegionExp == "" ) 
	{
		ipExcludeRegions = NULL;
	}
	else
	{
		ipExcludeRegions = m_ipRegExParser->Find( _bstrText, VARIANT_FALSE, VARIANT_FALSE);
	}

	long nNumMatches = ipMatches->Size();
	if (nNumMatches > 0)
	{
		// Flag to indicate if previous string was excluded
		bool bPrevExclude = bExcludeAfter1stInitially;
		bool bFoundFirst = false;
		long nPrevEnd = 0;
		// iterate through the matches and populate the return vector
		for (int i = 0; i < nNumMatches; i++)
		{
			// each item in the ipMatches is of type IObjectPair
			IObjectPairPtr ipObjPair = ipMatches->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI08421", ipObjPair != NULL );
			// Token is the first object in the object pair
			ITokenPtr ipToken = ipObjPair->Object1;
			if (ipToken)
			{	
				long nStart, nEnd;
				ipToken->GetTokenInfo(&nStart, &nEnd, NULL, NULL);
				
				// check if value is within exclude region
				if ( valueWithinRegions( nStart, nEnd, ipExcludeRegions ))
				{
					nPrevEnd = nEnd;
					bPrevExclude = true;
					// process next match
					continue;
				}

				// create a spatial string representing the match
				ISpatialStringPtr ipMatch = ipInputText->GetSubString(nStart, nEnd);
				ASSERT_RESOURCE_ALLOCATION("ELI08058", ipMatch != NULL);

				//string strDisplay = ipMatch->String;
				//MessageBox(NULL, strDisplay.c_str(), "found String", MB_OK );

				// Trim Leading and trailing Spaces
				ipMatch->Trim(" "," " );
				string strMatchStr = ipMatch->String;
				bool bAlwaysKeep = false;
				if ( strAlwaysKeepRegExp != "" )
				{
					bAlwaysKeep = isRegExpInText( strMatchStr, strAlwaysKeepRegExp);
				}
				if ( !bFoundFirst || (strExBetweenRegExp.size() == 0) || bAlwaysKeep)
				{
					nPrevEnd = nEnd;
					// if strIncludeRegExp is empty or found in the string put in result vector
					if ( bAlwaysKeep || strIncludeRegExp == "" || isRegExpInText(strMatchStr, strIncludeRegExp))
					{
						bFoundFirst = true;
						ipReturnStrings->PushBack( ipMatch);
					}
				}
				else
				{
					//Process the between string
					string strInput = _bstrText;
					string strBetween = strInput.substr( nPrevEnd, nStart - nPrevEnd );
					// stop looking if strExBetweenRegExp is found in the string between values
					if ( bPrevExclude || isRegExpInText ( strBetween, strExBetweenRegExp) )
					{
						if ( strIncBetweenRegExp != "" )
						{
							if ( isRegExpInText ( strBetween, strIncBetweenRegExp ))
							{
								bPrevExclude = false;
							}
							else
							{
								bPrevExclude = true;
							}
						}
						else
						{
							bPrevExclude = true;
							// no need to process any more
							break;
						}
					}
					if ( !bPrevExclude ) 
					{
						// check for include string
						string strSearch = ipMatch->String;
						// if strIncludeRegExp is empty or found in the string put in result vector
						if ( strIncludeRegExp == "" || isRegExpInText ( strSearch, strIncludeRegExp ) )
						{
							ipReturnStrings->PushBack( ipMatch);
						}
					}
					nPrevEnd = nEnd;
				}

			}
		}

	}
	return ipReturnStrings;
}

//-------------------------------------------------------------------------------------------------
bool CLegalDescSplitter::isRegExpInText ( string strSearchText, string strRegExp )
{
	//if the strRegExp or StrSearchText are empty then return false other wise check for the RegExp in SearchText
	if ( strRegExp != "" && strSearchText != "" )
	{
		m_ipRegExParser->Pattern = _bstr_t(strRegExp.c_str());
		m_ipRegExParser->IgnoreCase = VARIANT_TRUE;
		IIUnknownVectorPtr ipFoundMatches = m_ipRegExParser->Find(_bstr_t(strSearchText.c_str()), 
			VARIANT_TRUE, VARIANT_FALSE );
		if ( ipFoundMatches->Size() > 0 )
		{
			return true;
		}
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
string CLegalDescSplitter::getRegExpForType( string strDocType, string strFileNameWOPath )
{
	string strFileName;
	string strComponentDataDir = getAFUtility()->GetComponentDataFolder();
	strFileName =  strComponentDataDir + "\\LegalDescSplitter\\" + strDocType + "\\" + strFileNameWOPath;

	// [FlexIDSCore:3643] Load the regular expression from disk if necessary.
	// Check for the regular expression being loaded previously
	map<string, CachedObjectFromFile<string, RegExLoader> >::iterator iter =
		m_mapFileNameToCachedRegExLoader.find(strFileName);
	if (iter == m_mapFileNameToCachedRegExLoader.end())
	{
		m_mapFileNameToCachedRegExLoader[strFileName] =
			CachedObjectFromFile<string, RegExLoader>(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str());
	}

	m_mapFileNameToCachedRegExLoader[strFileName].loadObjectFromFile(strFileName);

	// Retrieve the pattern
	string strRegExp = (string)m_mapFileNameToCachedRegExLoader[strFileName].m_obj;

	return strRegExp;
}
//-------------------------------------------------------------------------------------------------
IAFUtilityPtr CLegalDescSplitter::getAFUtility()
{
	if (m_ipAFUtility == NULL)
	{
		m_ipAFUtility.CreateInstance( CLSID_AFUtility );
		ASSERT_RESOURCE_ALLOCATION( "ELI07976", m_ipAFUtility != NULL );
	}
	
	return m_ipAFUtility;
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CLegalDescSplitter::getMiscUtils()
{
	if (m_ipMiscUtils == NULL)
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI07977", m_ipMiscUtils != NULL );
	}
	
	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CLegalDescSplitter::createAttribute( string strAttrName, ISpatialStringPtr ipAttrValue )
{
	IAttributePtr ipAttribute(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI08178", ipAttribute != NULL);

	// set the match as the value of the attribute, and add
	// the attribute to the result vector
	ipAttribute->Value = ipAttrValue;
	ipAttribute->Name = strAttrName.c_str();
	return ipAttribute;
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CLegalDescSplitter::createAttribute( string strAttrName, string strAttrValue )
{
	IAttributePtr ipAttribute(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI08180", ipAttribute != NULL);
	ISpatialStringPtr ipAttrValue(CLSID_SpatialString );
	ASSERT_RESOURCE_ALLOCATION("ELI08183", ipAttrValue != NULL );
	ipAttrValue->CreateNonSpatialString(strAttrValue.c_str(), "");

	// set the match as the value of the attribute, and add
	// the attribute to the result vector
	ipAttribute->Value = ipAttrValue;
	ipAttribute->Name = strAttrName.c_str();
	return ipAttribute;
}
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::setupMuniRuleSet()
{

	// if m_ipFindInfo has been setup reload if LoadFilePerSession is not set
	if (m_ipMuniRuleSet != NULL ) 
	{
		if ( getAFUtility()->GetLoadFilePerSession() == VARIANT_TRUE )
		{
			// already setup and LoadFilePerSession is set
			return;
		}
	}
	
	// compute the name of the file to be imported and perform
	// any appropriate auto-encrypt actions
	string strComponentDir = getAFUtility()->GetComponentDataFolder();
	string strRSDFile = strComponentDir + "\\LegalDescSplitter\\MuniFind.rsd.etf";
	getMiscUtils()->AutoEncryptFile ( _bstr_t(strRSDFile.c_str()),
		_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()));

	m_ipMuniRuleSet.CreateInstance(CLSID_RuleSet);
	ASSERT_RESOURCE_ALLOCATION("ELI08373", m_ipMuniRuleSet != NULL );

	// Load Ruleset
	m_ipMuniRuleSet->LoadFrom(_bstr_t(strRSDFile.c_str()), VARIANT_FALSE);

}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CLegalDescSplitter::processValueParts( ISpatialStringPtr ipMainValue, 
														 IIUnknownVectorPtr ipPartStrings, 
														 string strExtractRegExp,
														 string strBeforeRegExp )
{
	ASSERT_ARGUMENT("ELI08480", ipPartStrings != NULL );

	IIUnknownVectorPtr ipResults(CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION("ELI08062", ipResults != NULL );
	// Setup ipRemoveStrings to hold strings to remove from ipMainValue
	IVariantVectorPtr ipRemoveStrings ( CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI08054", ipRemoveStrings != NULL);
	// Extract the values from the Parts and remove from ipMainValue
	long nNumValues = ipPartStrings->Size();
	for ( int i = 0; i < nNumValues; i++ )
	{
		// Get Part to process
		ISpatialStringPtr ipPart = ipPartStrings->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI08422", ipPart != NULL );

		//Extract the value from the part
		IIUnknownVectorPtr ipFoundStrings = getFoundStrings( ipPart, strExtractRegExp );
		string strPart = ipPart->String;

		if ( isRegExpInText ( strPart, RANGE_PATTERN ))
		{
			//IIUnknownVectorPtr ipExpandedResults = expandNumericRanges( ipFoundStrings );
			//makeRangeSeparatorDash( ipFoundStrings );
			ipResults->Append( ipFoundStrings );
		}
		else
		{
			// Add to Result so all can be added at once
			ipResults->Append( ipFoundStrings );
		}
	
		if ( ipMainValue != NULL )
		{
			// Add to Remove Strings to find and remove from MainValue string
			ipRemoveStrings->PushBack(_bstr_t ( strPart.c_str() ));
			//Allocate for Finding position of string
			long lStart= 0;
			long lEnd = 0;
			// Find location of Part string in MainValue string
			ipMainValue->FindFirstItemInVector( ipRemoveStrings, 
				VARIANT_FALSE, VARIANT_FALSE, 0, &lStart, &lEnd );
			if (lStart != -1 && lEnd != -1)
			{
				string strMainValue = ipMainValue->String;
				if ( strBeforeRegExp != "" )
				{
					string strSearch = strMainValue.substr(0, lStart);
					if ( isRegExpInText( strSearch, strBeforeRegExp ) )
					{
						// remove part from the end of ipMainValue 
						ipMainValue->Remove( lStart, -1 );
					}
					else if ( lEnd + 5 > (long)strMainValue.size() )
					{
						// Remove from ipMainValue 
						ipMainValue->Remove ( lStart, lEnd );
					}
					else
					{
						// Remove part from the beginning of ipMainValue
						ipMainValue->Remove(0, lEnd );
					}
				}
//				else if ( lStart > (strMainValue.size() / 2))
//				{
//					// Remove from ipMainValue 
//					ipMainValue->Remove ( lStart, lEnd );
//				}
				else
				{
					// Remove part from the beginning of ipMainValue
					ipMainValue->Remove(0, lEnd );
				}
			}
			ipRemoveStrings->Clear();
		}
	}
	return ipResults;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CLegalDescSplitter::expandNumericRanges ( IIUnknownVectorPtr ipRangedValues )
{
	ASSERT_ARGUMENT("ELI08423", ipRangedValues != NULL );
	IIUnknownVectorPtr ipResults(CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION("ELI08400", ipResults != NULL );

	// check each found string for range
	long nNumValues = ipRangedValues->Size();
	for ( int i = 0; i < nNumValues; i++ )
	{
		ISpatialStringPtr ipRangeValue = ipRangedValues->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI08424", ipRangeValue != NULL );
		string strRangeValue = ipRangeValue->String;
		if ( isRegExpInText ( strRangeValue, RANGE_PATTERN ) )
		{
			// found string is a range and should have a beginning and ending value
			IIUnknownVectorPtr ipRangeLimits = getFoundStrings (ipRangeValue, "\\d+" );
			long nNumLimits = ipRangeLimits->Size();
			if ( nNumLimits != 2 )
			{
				// something wrong in our program logic
				THROW_LOGIC_ERROR_EXCEPTION("ELI08087");
			}
			ISpatialStringPtr ipFirst = ipRangeLimits->At(0);
			ASSERT_RESOURCE_ALLOCATION("ELI08425", ipFirst != NULL );
			ISpatialStringPtr ipLast = ipRangeLimits->At(1);
			ASSERT_RESOURCE_ALLOCATION("ELI08426", ipLast != NULL );
			string strFirst = asString(ipFirst->String);
			string strLast = asString(ipLast->String);
			long lFirst = asLong(strFirst);
			long lLast = asLong(strLast);
			// put on the first value
			ipResults->PushBack ( ipFirst );
			for ( long i = lFirst + 1; i < lLast ; i++ )
			{
				ISpatialStringPtr ipValue (CLSID_SpatialString );
				ASSERT_RESOURCE_ALLOCATION("ELI08079", ipValue != NULL );
				ipValue->CreateNonSpatialString(asString(i).c_str(), "");
					
				ipResults->PushBack ( ipValue );
			}

			// add the last
			ipResults->PushBack( ipLast );
		}
		else 
		{
			ipResults->PushBack( ipRangeValue );
		}
	}
	return ipResults;

}
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::makeRangeSeparatorDash( IIUnknownVectorPtr ipRangedValues )
{
	ASSERT_ARGUMENT("ELI10353", ipRangedValues != NULL );

	// check each found string for range
	long nNumValues = ipRangedValues->Size();
	for ( int i = 0; i < nNumValues; i++ )
	{
		ISpatialStringPtr ipRangeValue = ipRangedValues->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI10354", ipRangeValue != NULL );
		string strRangeValue = ipRangeValue->String;
		if ( isRegExpInText ( strRangeValue, RANGE_PATTERN ) )
		{
			// found string is a range and should have a beginning and ending value
			IIUnknownVectorPtr ipRangeLimits = getFoundStrings (ipRangeValue, "\\d+" );
			long nNumLimits = ipRangeLimits->Size();
			if ( nNumLimits != 2 )
			{
				// something wrong in our program logic
				THROW_LOGIC_ERROR_EXCEPTION("ELI10355");
			}
			ISpatialStringPtr ipFirst = ipRangeLimits->At(0);
			ASSERT_RESOURCE_ALLOCATION("ELI10356", ipFirst != NULL );
			ISpatialStringPtr ipLast = ipRangeLimits->At(1);
			ASSERT_RESOURCE_ALLOCATION("ELI10357", ipLast != NULL );

			// Test of having a range that always has  - separating it
			ipRangeValue->Clear();
			ipRangeValue->Append( ipFirst);
			ipRangeValue->AppendString ( "-");
			ipRangeValue->Append ( ipLast );
		}
	}
	return;

}
//-------------------------------------------------------------------------------------------------
IRuleSetPtr CLegalDescSplitter::getModifierRuleSet( string strLegalType )
{
	if ( strLegalType == "" )
	{
		throw UCLIDException("ELI08465", "Legal Description Type is not valid.");
	}

	// If m_ipLegalTypeToRuleSetMap has not yet been used create it
	if ( m_ipLegalTypeToRuleSetMap == NULL )
	{
		m_ipLegalTypeToRuleSetMap.CreateInstance(CLSID_StrToObjectMap);
		ASSERT_RESOURCE_ALLOCATION("ELI08464", m_ipLegalTypeToRuleSetMap != NULL );
	}

	VARIANT_BOOL bLoadFilePerSession = getAFUtility()->GetLoadFilePerSession() ;
	if ( bLoadFilePerSession == VARIANT_TRUE )
	{
		if ( m_ipLegalTypeToRuleSetMap->Contains( strLegalType.c_str() ) )
		{
			IRuleSetPtr ipModifierRuleSet = m_ipLegalTypeToRuleSetMap->GetValue ( strLegalType.c_str());
			ASSERT_RESOURCE_ALLOCATION("ELI08466", ipModifierRuleSet != NULL );
			return ipModifierRuleSet;
		}
	}

	// compute the name of the file to be imported and perform
	// any appropriate auto-encrypt actions
	string strComponentDir = getAFUtility()->GetComponentDataFolder();
	string strRSDFile = strComponentDir + "\\LegalDescSplitter\\" + strLegalType + "\\ModifierRulesSet.rsd.etf";
	getMiscUtils()->AutoEncryptFile ( _bstr_t(strRSDFile.c_str()),
		_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()));

	IRuleSetPtr ipModifierRuleSet(CLSID_RuleSet);
	ASSERT_RESOURCE_ALLOCATION("ELI08467", ipModifierRuleSet != NULL );

	// Load Ruleset
	ipModifierRuleSet->LoadFrom(_bstr_t(strRSDFile.c_str()), VARIANT_FALSE);

	if ( bLoadFilePerSession == VARIANT_TRUE )
	{
		m_ipLegalTypeToRuleSetMap->Set( strLegalType.c_str(), ipModifierRuleSet );
	}
	return ipModifierRuleSet;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CLegalDescSplitter::applyModifiers( string strLegalType, string strAttrName, ISpatialStringPtr ipValue )
{
	ASSERT_ARGUMENT("ELI08470", ipValue != NULL );

	// Create AF Document for splitting
	IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
	ASSERT_RESOURCE_ALLOCATION("ELI08468", ipAFDoc != NULL );
	ipAFDoc->Text = ipValue;

	IVariantVectorPtr ipvecAttributeNames(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI08469", ipvecAttributeNames != NULL );

	// Add the Attribute name to find
	ipvecAttributeNames->PushBack( strAttrName.c_str());

	IRuleSetPtr ipModifierRuleSet = getModifierRuleSet( strLegalType );
	ASSERT_RESOURCE_ALLOCATION("ELI08471", ipModifierRuleSet != NULL );

	IIUnknownVectorPtr ipAttributes = ipModifierRuleSet->ExecuteRulesOnText( ipAFDoc, ipvecAttributeNames, NULL );

	// Return size is always expected to be <= 1 if not return the input string
	long nNumAttr = ipAttributes->Size();
	if ( nNumAttr == 1 )
	{
		IAttributePtr ipAttr = ipAttributes->At(0);
		ASSERT_RESOURCE_ALLOCATION("ELI08473", ipAttr != NULL );
		return ipAttr->Value;
	}
	// if 0 or more than one value return the original string
	return ipValue;
}
//-------------------------------------------------------------------------------------------------
bool CLegalDescSplitter::valueWithinRegions(long nStart, long nEnd, IIUnknownVectorPtr &ipRegions)
{
	if ( ipRegions == NULL  )
	{
		return false;
	}
	long nNumRegions = ipRegions->Size();
	for ( int i = 0; i < nNumRegions; i++ )
	{
		// each item in the ipMatches is of type IObjectPair
		IObjectPairPtr ipObjPair = ipRegions->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI08793", ipObjPair != NULL );
		// Token is the first object in the object pair
		ITokenPtr ipToken = ipObjPair->Object1;
		if (ipToken)
		{	
			long nStartRegion, nEndRegion;
			ipToken->GetTokenInfo(&nStartRegion, &nEndRegion, NULL, NULL);
			if ( nStart >= nStartRegion && nStart < nEndRegion )
			{
				return true;
			}
			if ( nEnd <= nEndRegion && nEnd > nStartRegion )
			{
				return true;
			}
		}
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::splitAndAddTown( IIUnknownVectorPtr ipTownValues, IIUnknownVectorPtr ipSubAttrs )
{
	ASSERT_ARGUMENT ( "ELI10214", ipSubAttrs != NULL )
	if ( ipTownValues == NULL )
	{
		return;
	}

	string strNumbers = "\\d+";
	IIUnknownVectorPtr ipResultTownNums= 
			processValueParts( NULL , ipTownValues, strNumbers, "" );
	
	// Add Town Nums
	addAsAttributes( "TownNum", ipResultTownNums, ipSubAttrs, true, "PLS-L", true );

	string strDir = "(N|S|NORTH|SOUTH)(?=\\r*$)";
	IIUnknownVectorPtr ipResultTownDirs = 
			processValueParts( NULL , ipTownValues, strDir, "" );

	// Add Town Dirs
	addAsAttributes ("TownDir", ipResultTownDirs, ipSubAttrs, true, "PLS-L", true );
		
}
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::splitAndAddRange( IIUnknownVectorPtr ipRangeValues, IIUnknownVectorPtr ipSubAttrs )
{
	ASSERT_ARGUMENT ( "ELI10215", ipSubAttrs != NULL )
	if ( ipRangeValues == NULL )
	{
		return;
	}

	string strNumbers = "\\d+";
	IIUnknownVectorPtr ipResultRangeNums= 
			processValueParts( NULL , ipRangeValues, strNumbers, "" );
	
	// Add Range Nums
	addAsAttributes( "RangeNum", ipResultRangeNums, ipSubAttrs, true, "PLS-L", true );

	string strDir = "(E|W|EAST|WEST)(?=\\r*$)";
	IIUnknownVectorPtr ipResultRangeDirs = 
			processValueParts( NULL , ipRangeValues, strDir, "" );

	// Add Range Dirs
	addAsAttributes ("RangeDir", ipResultRangeDirs, ipSubAttrs, true, "PLS-L", true );
		
}
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::reTypeAttribute ( IAttributePtr ipLocationAttr, IIUnknownVectorPtr ipParentSubAttrs )
{
	if ( ipLocationAttr == NULL )
	{
		return;
	}
	ASSERT_ARGUMENT( "ELI10216", ipParentSubAttrs != NULL );
	string strAttrType = ipLocationAttr->Type;
	if ( strAttrType == "SUB_BLO")
	{
		bool bBlockFound = false;
		bool bLotFound = false;
		bool bOutlotFound = false;
		vector<long> vecLotIndexes;
		vector<long> vecOutlotIndexes;
		IIUnknownVectorPtr ipLocationSubAttr = ipLocationAttr->SubAttributes;
		long nNumSubAttrs = ipLocationSubAttr->Size();
		long i;
		for ( i = 0; i < nNumSubAttrs; i++ )
		{
			IAttributePtr ipCurrAttr = ipLocationSubAttr->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI10217", ipCurrAttr != NULL );

			string strName = ipCurrAttr->Name;
			if ( strName == "Block" )
			{
				bBlockFound = true;
			}
			else if ( strName == "Lot")
			{
				bLotFound = true;
				vecLotIndexes.push_back(i);
			}
			else if ( strName == "Outlot" ) 
			{
				bOutlotFound = true;
				vecOutlotIndexes.push_back(i);
			}
		}
		if ( bOutlotFound && bLotFound )
		{
			ICopyableObjectPtr ipClone = ipLocationAttr;
			ASSERT_RESOURCE_ALLOCATION("ELI10218", ipClone != NULL );

			IAttributePtr ipOutlotAttr = ipClone->Clone();

			// Get sub-attributes
			IIUnknownVectorPtr ipSub = ipOutlotAttr->SubAttributes;
			ASSERT_RESOURCE_ALLOCATION("ELI15560", ipSub != NULL);

			long nNumLots = vecLotIndexes.size();
			for ( i = 0; i < nNumLots; i++ )
			{
				ipSub->Remove( vecLotIndexes[i]);
			}
			ipOutlotAttr->Type = "SUB_O";
			long nNumOutlots = vecOutlotIndexes.size();
			for ( i = 0; i < nNumOutlots; i++ )
			{
				ipLocationSubAttr->Remove( vecOutlotIndexes[i] );
			}
			ipLocationAttr->Type = "SUB_L";
			ipParentSubAttrs->PushBack( ipOutlotAttr );
		}
		else if ( bOutlotFound )
		{
			ipLocationAttr->Type = "SUB_O";
		}
		else if ( bLotFound )
		{
			ipLocationAttr->Type = "SUB_L";
		}
		else if ( bBlockFound )
		{
			ipLocationAttr->Type = "SUB_B";
		}
		else
		{
			ipLocationAttr->Type = "SUB";
		}
	}
	ipParentSubAttrs->PushBack( ipLocationAttr );
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr  CLegalDescSplitter::consolidateValues ( IIUnknownVectorPtr ipValueStrings, bool expandRanges )
{
	if (ipValueStrings == NULL )
	{
		return NULL;
	}

	// expand all ranges so i can work with the individual values
	IIUnknownVectorPtr ipExpandedValues = expandNumericRanges( ipValueStrings );

	// Allocate the return vector
	IIUnknownVectorPtr ipReturnValues (CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI10360", ipReturnValues != NULL );

	map< long, ISpatialStringPtr> mapValuesToLong;
	mapValuesToLong.clear();
	long nNumValues = ipExpandedValues->Size();
	for ( long i = 0; i < nNumValues; i++ )
	{
		ISpatialStringPtr ipCurrValue = ipExpandedValues->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI10361", ipCurrValue != NULL );

		string strValue = ipCurrValue->String;
		long nValue = atol(strValue.c_str());
		if ( nValue == 0 || isRegExpInText(strValue, "[^\\d]")) 
		{
			ipReturnValues->PushBack( ipCurrValue );
		}
		else if ( mapValuesToLong.find ( nValue ) == mapValuesToLong.end())
		{
			mapValuesToLong[nValue] = ipCurrValue;
		}
	}


	map< long, ISpatialStringPtr>::iterator iter = mapValuesToLong.begin();
	long nLastKey;
	ISpatialStringPtr ipRangeValue, ipLastValue;
	bool bIsRange = false;
	while ( iter != mapValuesToLong.end() )
	{
		if ( expandRanges )
		{
			ipReturnValues->PushBack( iter->second );
		}
		else if ( iter == mapValuesToLong.begin() )
		{
			nLastKey = iter->first;
			ipRangeValue = iter->second;
			ipLastValue = ipRangeValue;
		}
		else if ( iter->first == nLastKey + 1 )
		{
			// value is within a range
			if ( !bIsRange  )
			{
				bIsRange = true;
				ipRangeValue->AppendString( _bstr_t("-") );
			}
			nLastKey = iter->first;
			ipLastValue = iter->second;
		}
		else if ( bIsRange )
		{
			ipRangeValue->Append( ipLastValue );
			ipReturnValues->PushBack( ipRangeValue );
			nLastKey = iter->first;
			ipRangeValue = iter->second;
			ipLastValue = ipRangeValue;
			bIsRange = false;
		}
		else
		{
			ipReturnValues->PushBack( ipLastValue );
			nLastKey = iter->first;
			ipRangeValue = iter->second;
			ipLastValue = ipRangeValue;
		}
		iter++;
	}
	if ( bIsRange )
	{
		ipRangeValue->Append( ipLastValue );
		ipReturnValues->PushBack( ipRangeValue );
	}
	else if ( ipLastValue != NULL )
	{
		ipReturnValues->PushBack( ipLastValue );
	}
	return ipReturnValues;
}
//-------------------------------------------------------------------------------------------------
void CLegalDescSplitter::separateFullAndPartial ( IIUnknownVectorPtr ipValueStrings, IIUnknownVectorPtr &ipFullValues, IIUnknownVectorPtr &ipPartValues )
{
	ASSERT_ARGUMENT("ELI10364", ipValueStrings != NULL );
	ASSERT_ARGUMENT("ELI10365", ipValueStrings != NULL );
	ASSERT_ARGUMENT("ELI10366", ipValueStrings != NULL );

	long nNumValues = ipValueStrings->Size();
	bool isPartial = false;
	bool isExceptClause = false;
	for ( long i = 0; i < nNumValues; i++ )
	{
		if ( isExceptClause )
		{
			// if this flag is set the string will be an except clause and should be ignored
			isExceptClause = false;
			continue;
		}
		ISpatialStringPtr ipCurrValue = ipValueStrings->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI10367", ipCurrValue != NULL );
		if ( i + 1 < nNumValues )
		{
			ISpatialStringPtr ipNextValue = ipValueStrings->At(i+1);
			ASSERT_RESOURCE_ALLOCATION("ELI10445", ipNextValue != NULL );

			string strNextValue = ipNextValue->String;
			if ( isRegExpInText ( strNextValue, "except[\\s\\S]+?(feet|thereof)" ) )
			{
				isExceptClause = true;
			}
		}

		string strValue = ipCurrValue->String;
		transform( strValue.begin(), strValue.end(), strValue.begin(), toupper);
		if ( strValue == "FEET" || strValue == "PART" )
		{
			isPartial = true;
		}
		else if ( strValue == "ALL" )
		{
			isPartial = false;
		}
		else if (isPartial || isExceptClause )
		{
			ipPartValues->PushBack( ipCurrValue );
		}
		else
		{
			ipFullValues->PushBack( ipCurrValue );
		}
	}
}
//-------------------------------------------------------------------------------------------------
IRegularExprParserPtr CLegalDescSplitter::getParser()
{
	try
	{
		IRegularExprParserPtr ipParser =
			getMiscUtils()->GetNewRegExpParserInstance("LegalDescSplitter");
		ASSERT_RESOURCE_ALLOCATION("ELI07965", ipParser != NULL);

		return ipParser;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29464");
}
//-------------------------------------------------------------------------------------------------
