// LegalDescriptionFinder.cpp : Implementation of CLegalDescriptionFinder
#include "stdafx.h"
#include "AFValueFinders.h"
#include "LegalDescriptionFinder.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

using namespace std;

// minimum score for matching in the string
#define MINIMUM_SCORE	1

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CLegalDescriptionFinder
//-------------------------------------------------------------------------------------------------
CLegalDescriptionFinder::CLegalDescriptionFinder()
: m_ipBlockFinder(NULL)
{
	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05741")
}
//-------------------------------------------------------------------------------------------------
CLegalDescriptionFinder::~CLegalDescriptionFinder()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16345");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILegalDescriptionFinder,
		&IID_IAttributeFindingRule,
		&IID_IPersistStream,
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
// IAttributeFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionFinder::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
													IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// This finder is obsolete so throw exception if this method is called
		UCLIDException ue("ELI28779", "Legal Description finder is obsolete.");
		throw ue;

		// Check license
		validateLicense();

		// Check Block Finder
		if (m_ipBlockFinder == __nullptr)
		{
			m_ipBlockFinder.CreateInstance( CLSID_BlockFinder );
			ASSERT_RESOURCE_ALLOCATION("ELI12422", m_ipBlockFinder != __nullptr);

			// clues to search in input text
			IVariantVectorPtr ipClues(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI05743", ipClues!=NULL);
			ipClues->PushBack(_bstr_t("\\b(Out)?lot[\\s\\S]{0,30}\\d+"));
			ipClues->PushBack(_bstr_t("(lot[\\s\\S]*)?section[\\s\\S]{1,50}town(ship)?([\\s\\S]{1,50}range)?"));
			ipClues->PushBack(_bstr_t("town(ship)?[\\s\\S]{1,50}range"));
			ipClues->PushBack(_bstr_t("commenc(e|ing|ement)[\\s\\S]*thence"));
			ipClues->PushBack(_bstr_t("point\\s+of\\s+(beginning|reference)"));
			ipClues->PushBack(_bstr_t("sec(tion)?\\s*\\d+\\s*T\\s*\\d+\\s*(N|E|S|W)\\s*R\\s*\\d+\\s*(N|E|S|W)"));
			ipClues->PushBack(_bstr_t("reel[\\s\\S]{1,20}image"));
			ipClues->PushBack(_bstr_t("((South|North)(easter|wester|er)ly)|((easter|wester)ly)"));
			ipClues->PushBack(_bstr_t("corner[\\s\\S]{1,20}\\blot\\b"));
			ipClues->PushBack(_bstr_t("(volume|\\bliber\\b)[\\s\\S]{1,50}page"));
			ipClues->PushBack(_bstr_t("\\bunit\\b[\\s\\S]{1,30}(building)?[\\s\\S]{1,30}condominium"));
			ipClues->PushBack(_bstr_t("condominium\\s+commonly\\s+known\\s+as"));
			ipClues->PushBack(_bstr_t("Ptn[\\s\\S]{1,5}Blk[\\s\\S]{1,30}Add(ition|n)"));
			ipClues->PushBack(_bstr_t("(abutting|adjoining)[\\s\\S]{1,30}said\\s+(lot|premise)"));
			ipClues->PushBack(_bstr_t("portion\\s+of\\s+(lot|block)"));
			ipClues->PushBack(_bstr_t("Range\\s+\\d+\\s+(East|West)"));
			ipClues->PushBack(_bstr_t("((city|town(ship)?|village)\\s+of|(in[\\s\\S]{1,50}?township)|[\\s\\w]+?Town(ship)?(?!\\s+of|\\s+\\d))[\\s,]+[\\s\\S]{1,50}?County[\\s\\S]+?"));
			ipClues->PushBack(_bstr_t("certified\\s+survey\\s+map|\\bCSM\\b"));
			ipClues->PushBack(_bstr_t("(north|south)(east|west)"));
			ipClues->PushBack(_bstr_t("(\\b(Out)?lot\\b|block\\b)[\\s\\S]{1,100}\\d+[\\s\\S]+subdivision"));
			ipClues->PushBack(_bstr_t("centerline|right[\\s\\S]{1,3}of[\\s\\S]{1,3}way\\b"));

			m_ipBlockFinder->InputAsOneBlock = VARIANT_TRUE;
			m_ipBlockFinder->FindAllBlocks = VARIANT_FALSE;
			m_ipBlockFinder->MinNumberOfClues = MINIMUM_SCORE;
			m_ipBlockFinder->IsClueRegularExpression = VARIANT_TRUE;
			m_ipBlockFinder->GetMaxOnly = VARIANT_FALSE;
			m_ipBlockFinder->IsCluePartOfAWord = VARIANT_TRUE;
			m_ipBlockFinder->Clues = ipClues;
		}

		IAFDocumentPtr ipAFDoc(pAFDoc);
		// Get the text out from the spatial string
		ISpatialStringPtr ipInputText(ipAFDoc->Text);
		// get the input string from the spatial string
		string strInput = ipInputText->String;

		// get the line break string (ex. \r\n\r\n, or \n\n, etc.) from input
		string strSeparator = getLineBreakString(strInput);
		// assume that the paragraph separator has at least two line breaks
		strSeparator = strSeparator + strSeparator;
		// block separator will be dependent on the input text
		m_ipBlockFinder->BlockSeparator = _bstr_t(strSeparator.c_str());
		
		IIUnknownVectorPtr ipAttributes(NULL);

		IAttributeFindingRulePtr ipAttFinder(m_ipBlockFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI08064", ipAttFinder != __nullptr);
		
		// find the legal description blocks
		ipAttributes = ipAttFinder->ParseText(ipAFDoc, NULL);
		// go through each found value and combine as many as we can
		combineNearbyAttributes(ipInputText, ipAttributes);

		*pAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05681");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionFinder::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19580", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Legal description finder").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05682")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionFinder::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08249");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionFinder::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_LegalDescriptionFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI08345", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05685");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionFinder::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_LegalDescriptionFinder;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionFinder::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionFinder::Load(IStream *pStream)
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
			UCLIDException ue( "ELI07646", "Unable to load newer LegalDescription Finder." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05686");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionFinder::Save(IStream *pStream, BOOL fClearDirty)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05687");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionFinder::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CLegalDescriptionFinder::combineNearbyAttributes(ISpatialStringPtr ipOriginInput, 
													  IIUnknownVectorPtr ipAttributes)
{
	string strInputText = ipOriginInput->String;
	if (strInputText.empty())
	{
		return;
	}

	// obtain each found attribute value's position in the original input text
	vector<Position> vecPositions;
	unsigned long ulSize = (unsigned long)ipAttributes->Size();
	if (ulSize <= 1)
	{
		return;
	}

	unsigned long ul;
	for (ul = 0; ul < ulSize; ul++)
	{
		// Retrieve this Attribute
		IAttributePtr ipAttr = ipAttributes->At(ul);
		ASSERT_RESOURCE_ALLOCATION("ELI15598", ipAttr != __nullptr);

		// Retrieve the Value as a string
		ISpatialStringPtr ipValue = ipAttr->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI15599", ipValue != __nullptr);

		string strValue = ipValue->String;
		Position position;
		position.m_nStartPos = strInputText.find(strValue);
		if (position.m_nStartPos != string::npos)
		{
			position.m_nEndPos = position.m_nStartPos + strValue.size() - 1;
			vecPositions.push_back(position);
		}
	}

	// examine the text in between every two attributes
	ul = 0;
	while (ul + 1 < vecPositions.size())
	{
		Position pos1 = vecPositions[ul];
		Position pos2 = vecPositions[ul+1];
		// get the string in between
		string strInBetween = strInputText.substr(pos1.m_nEndPos+1, pos2.m_nStartPos-pos1.m_nEndPos-1);
		// if the string in between only contains the white spaces, then combine the two
		strInBetween = ::trim(strInBetween, " \t\r\n", " \t\r\n");
		if (strInBetween.empty())
		{
			vecPositions[ul+1].m_nStartPos = pos1.m_nStartPos;
			vecPositions.erase(vecPositions.begin() + ul);
			continue;
		}

		ul++;
	}

	// if the vector size is unchanged, no change shall be made to the vec of attributes
	if (vecPositions.size() == ulSize)
	{
		return;
	}

	for (ul = 0; ul < vecPositions.size(); ul++)
	{
		ISpatialStringPtr ipNewValue = ipOriginInput->GetSubString(
							vecPositions[ul].m_nStartPos, vecPositions[ul].m_nEndPos);
		IAttributePtr ipAttr = ipAttributes->At(ul);
		ipAttr->Value = ipNewValue;
	}

	// remove the rest of the attributes in the vec
	if (vecPositions.size() < ulSize)
	{
		ipAttributes->RemoveRange(vecPositions.size(), ulSize-1);
	}
}
//-------------------------------------------------------------------------------------------------
string CLegalDescriptionFinder::getLineBreakString(const string& strInput)
{
	// first define the line terminator
	string strLineBreak("");
	int nFound1 = strInput.find("\n");
	int nFound2 = strInput.find("\r");
	if (nFound1 != string::npos && nFound1+1 == nFound2)
	{
		strLineBreak = "\n\r";
	}
	else if (nFound2 != string::npos && nFound1 == nFound2+1)
	{
		strLineBreak = "\r\n";
	}
	else if (nFound1 == string::npos && nFound2 != string::npos)
	{
		strLineBreak = "\r";
	}
	else if (nFound1 != string::npos && nFound2 == string::npos)
	{
		strLineBreak = "\n";
	}
	else
	{
		// otherwise, assume line break is \n
		strLineBreak = "\n";
	}

	return strLineBreak;
}
//-------------------------------------------------------------------------------------------------
void CLegalDescriptionFinder::validateLicense()
{
	static const unsigned long LEGAL_DESC_FINDER_ID = gnFLEXINDEX_CORE_OBJECTS;

	VALIDATE_LICENSE( LEGAL_DESC_FINDER_ID, "ELI05688", "Legal Description Finder" );
}
//-------------------------------------------------------------------------------------------------
