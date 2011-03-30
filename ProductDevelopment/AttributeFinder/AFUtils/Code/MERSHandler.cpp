// MERSHandler.cpp : Implementation of CMERSHandler
#include "stdafx.h"
#include "AFUtils.h"
#include "MERSHandler.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>
#include <ComUtils.h>

#include <ctype.h>

#define CRTDBG_MAP_ALLOC
#include <stdlib.h>
#include <crtdbg.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

// predefined expressions
static const string NON_TP = " \n\r\taAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ,.&'-";
static const string BORROWER_IS = ") \" borrower \" is|\" borrower \" is|borrower \" is|\" borrower is|) \" borrower|borrower \' is|) borrower .";
static const string LENDER_IS = ") \" lender \" is|\" lender \" is|lender \" is|\" lender is|) \" lender|lender \' is|) lender .";

//-------------------------------------------------------------------------------------------------
// CMERSHandler
//-------------------------------------------------------------------------------------------------
CMERSHandler::CMERSHandler()
: m_ipEntityFinder(NULL),
  m_ipSPM(NULL),
  m_ipExprDefined(NULL),
  m_bDirty(false)
{
	try
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI29454", m_ipMiscUtils != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29455");
}
//-------------------------------------------------------------------------------------------------
CMERSHandler::~CMERSHandler()
{
	try
	{
		// Release COM pointers
		m_ipMiscUtils = __nullptr;
		m_ipEntityFinder = __nullptr;
		m_ipSPM = __nullptr;
		m_ipExprDefined = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16333");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSHandler::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IMERSHandler,
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
// IMERSHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSHandler::ModifyEntities(IIUnknownVector *pVecAttributes, 
										  IAFDocument *pOriginalDoc, 
										  VARIANT_BOOL *pbFound)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		validateLicense();

		// ensure pre-requisites
		ASSERT_ARGUMENT("ELI06012", pVecAttributes != __nullptr)
		ASSERT_ARGUMENT("ELI06013", pOriginalDoc != __nullptr)
		IAFDocumentPtr ipOriginalDoc(pOriginalDoc);

		*pbFound = VARIANT_FALSE;

		IIUnknownVectorPtr ipFoundAttributes(pVecAttributes);
		long nSize = ipFoundAttributes->Size();
		for (long n = 0; n < nSize; n++)
		{
			IAttributePtr ipAttribute = ipFoundAttributes->At(n);
			ASSERT_RESOURCE_ALLOCATION("ELI06014", ipAttribute != __nullptr);
			
			ISpatialStringPtr ipAttrValue = ipAttribute->Value;
			// first to check if the value contains MERS keyword
			string strValue = ipAttrValue->String;
			int nStartPos = 0, nEndPos = 0;
			if (findMERSKeyword(strValue, nStartPos, nEndPos))
			{
				// if MERS keywords is found in the attribute value, 
				// then get this value
				ISpatialStringPtr ipMERSKeyword = ipAttrValue->GetSubString(nStartPos, nEndPos);

				// for each attribute value, call ModifyValue
				UCLID_AFCORELib::IAttributeModifyingRulePtr ipAttributeModifying = getThisAsCOMPtr();
				ASSERT_RESOURCE_ALLOCATION("ELI16880", ipAttributeModifying != __nullptr);
				ipAttributeModifying->ModifyValue(ipAttribute, ipOriginalDoc, NULL);
				
				// if the value contains no more MERS keyword
				strValue = ipAttrValue->String;
				if (!findMERSKeyword(strValue, nStartPos, nEndPos))
				{
					// create an sub attribute called Nominee 
					// with the value as MERS for this attribute
					IAttributePtr ipSubAttr(CLSID_Attribute);
					ASSERT_RESOURCE_ALLOCATION("ELI06397", ipSubAttr != __nullptr);
					ipSubAttr->Name = "Nominee";
					ipSubAttr->Value = ipMERSKeyword;

					// Retrieve collected sub-attributes
					IIUnknownVectorPtr ipSubs = ipAttribute->SubAttributes;
					ASSERT_RESOURCE_ALLOCATION("ELI15584", ipSubs != __nullptr);

					// add the sub attribute to the vector of sub attributes
					ipSubs->PushBack(ipSubAttr);

					// indicate that MERS is found and the 
					// attribute has been modified
					*pbFound = VARIANT_TRUE;
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05919")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSHandler::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CMERSHandler::raw_ModifyValue(IAttribute * pAttribute, IAFDocument* pOriginInput,
										   IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		IAFDocumentPtr ipOriginalDoc(pOriginInput);
		ASSERT_RESOURCE_ALLOCATION("ELI07136", ipOriginalDoc != __nullptr);
		ISpatialStringPtr ipOriginalText = ipOriginalDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI25951", ipOriginalText != __nullptr);

		IAttributePtr	ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION( "ELI09299", ipAttribute != __nullptr );

		// take the text to be modified
		ISpatialStringPtr ipTextToBeModified = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI07137", ipTextToBeModified != __nullptr);

		// get the string value of the text
		string strValue = asString(ipTextToBeModified->String);

		// If any "MERS" keywords is appearing in the text
		int nStartPos = 0, nEndPos = 0;
		if (findMERSKeyword(strValue, nStartPos, nEndPos))
		{
			// search pattern 3 first
			ISpatialStringPtr ipFoundValue = patternSearch3(ipOriginalText);
			if (ipFoundValue == __nullptr)
			{
				// now search pattern 1
				ipFoundValue = patternSearch1(ipOriginalText);
				if (ipFoundValue == __nullptr)
				{
					// no entity found in the first pattern,  
					// then search pattern 2
					ipFoundValue = patternSearch2(ipOriginalText);
				}
			}
			
			// Update the text to be modified
			ICopyableObjectPtr ipTextCopier = ipTextToBeModified;
			ASSERT_RESOURCE_ALLOCATION("ELI25952", ipTextCopier != __nullptr);
			if (ipFoundValue)
			{
				// if find any, update the attribute value
				ipTextCopier->CopyFrom(ipFoundValue);
			}
			else
			{
				// if MERS keywords is found in the text, 
				// then modify this value to have "MORTGAGE ELECTRONIC 
				// REGISTRATION SYSTEMS" only
				ISpatialStringPtr ipMERSKeyword = ipTextToBeModified->GetSubString(nStartPos, nEndPos);
				ASSERT_RESOURCE_ALLOCATION("ELI25953", ipMERSKeyword != __nullptr);
			
				ipTextCopier->CopyFrom(ipMERSKeyword);
			}

		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07131");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSHandler::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19570", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("MERS modifier").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07132");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSHandler::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSHandler::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Create a new MERSHandler object
		ICopyableObjectPtr ipObjCopy(CLSID_MERSHandler);
		ASSERT_RESOURCE_ALLOCATION("ELI08342", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07133");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSHandler::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_MERSHandler;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSHandler::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSHandler::Load(IStream *pStream)
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
			UCLIDException ue( "ELI07643", "Unable to load newer MERSHandler." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07134");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSHandler::Save(IStream *pStream, BOOL fClearDirty)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07135");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSHandler::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// private / helper methods
//-------------------------------------------------------------------------------------------------
UCLID_AFUTILSLib::IMERSHandlerPtr CMERSHandler::getThisAsCOMPtr()
{
	UCLID_AFUTILSLib::IMERSHandlerPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI16967", ipThis != __nullptr);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CMERSHandler::extractEntity(ISpatialStringPtr ipInputText)
{
	if (m_ipEntityFinder == __nullptr)
	{
		m_ipEntityFinder.CreateInstance(CLSID_EntityFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI05993", m_ipEntityFinder != __nullptr);
	}

	// make a copy of the input string
	ICopyableObjectPtr ipCopyable(ipInputText);
	ASSERT_RESOURCE_ALLOCATION("ELI06016", ipCopyable != __nullptr);

	ISpatialStringPtr ipRet(ipCopyable->Clone());
	ASSERT_RESOURCE_ALLOCATION("ELI06015", ipRet != __nullptr);

	m_ipEntityFinder->FindEntities(ipRet);

	// make sure the entity is found
	if (ipRet->String.length() == 0)
	{
		return NULL;
	}

	return ipRet;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CMERSHandler::findMatchEntity(ISpatialString* pInputText, 
												const string& strPattern,
												bool bCaseSensitive,
												bool bExtractEntity,
												bool bGreedy)
{
	// store input text in the smart pointer
	ISpatialStringPtr ipInputText(pInputText);
	ISpatialStringPtr ipResult(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI06017", ipResult != __nullptr);

	// get actual text value out from the input text
	if (ipInputText->String.length() == 0)
	{
		return NULL;
	}

	// find match using string pattern matcher
	IStrToObjectMapPtr ipFoundMatches = match(ipInputText, strPattern, 
		bCaseSensitive, bGreedy);
	ASSERT_RESOURCE_ALLOCATION("ELI06018", ipFoundMatches != __nullptr);

	// Note: in this perticular case, we always expect one and only one
	// value to be found in the input text
	if (ipFoundMatches->Size == 0)
	{
		return NULL;
	}
	else if (ipFoundMatches->Size > 1)
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI06284");
	}

	// get this one and only one found token value
	CComBSTR bstrVariableName;
	IUnknownPtr ipUnkVariableValue;
	ipFoundMatches->GetKeyValue(0, &bstrVariableName, &ipUnkVariableValue);

	ITokenPtr ipToken = ipUnkVariableValue;
	ASSERT_RESOURCE_ALLOCATION("ELI06019", ipToken != __nullptr);
	
	// get start and end position of the found value
	long nStartPos = ipToken->StartPosition;
	long nEndPos = ipToken->EndPosition;
	// make sure the value is not empty
	if (nStartPos >= nEndPos)
	{
		return NULL;
	}

	// Get the substring of the spatial string based on the start and end position
	ipResult = ipInputText->GetSubString(nStartPos, nEndPos);

	// extract the entity out from the value if required
	if (bExtractEntity)
	{
		ipResult = extractEntity(ipResult);
	}

	return ipResult;
}
//-------------------------------------------------------------------------------------------------
bool CMERSHandler::findMERSKeyword(const string& strInput, int& nStartPos, int& nEndPos)
{
	bool bFoundMatch = false;

	// Get the parser and search for matches
	IRegularExprParserPtr ipParser = getParser();
	IIUnknownVectorPtr ipFoundMatch = ipParser->Find(_bstr_t(strInput.c_str()), VARIANT_TRUE,
		VARIANT_FALSE);

	if (ipFoundMatch->Size() > 0)
	{
		bFoundMatch = true;

		// set start and end position
		IObjectPairPtr ipObjPair = ipFoundMatch->At(0);
		ASSERT_RESOURCE_ALLOCATION("ELI06371", ipObjPair != __nullptr);

		ITokenPtr ipMatch = ipObjPair->Object1;
		ASSERT_RESOURCE_ALLOCATION("ELI06372", ipMatch != __nullptr);

		nStartPos = ipMatch->StartPosition;
		nEndPos = ipMatch->EndPosition;
	}
	
	return bFoundMatch;
}
//-------------------------------------------------------------------------------------------------
bool CMERSHandler::isFirstWordStartsUpperCase(const string& strInput)
{
	char cLetter = 0;
	for (unsigned int ui = 0; ui < strInput.size(); ui++)
	{
		cLetter = strInput[ui];
		// find first alpha char
		if (::isalpha((unsigned char) cLetter))
		{
			// found one, break out of the loop
			break;
		}
	}

	// if first alpha char is upper case
	if (cLetter != 0 && ::isupper((unsigned char) cLetter))
	{
		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
IStrToObjectMapPtr CMERSHandler::match(ISpatialStringPtr& ripInput, 
									   const string& strPattern,
									   bool bCaseSensitive,
									   bool bGreedy)
{
	if (m_ipSPM == __nullptr)
	{
		m_ipSPM.CreateInstance(CLSID_StringPatternMatcher);
		ASSERT_RESOURCE_ALLOCATION("ELI05990", m_ipSPM != __nullptr);
		// by default, case insensitive
		m_ipSPM->CaseSensitive = VARIANT_FALSE;
		// by default, treat multiple white space a one
		m_ipSPM->TreatMultipleWSAsOne = VARIANT_TRUE;
	}

	setPredefinedExpressions();

	// TODO : use case sensitivity
	// vector of IToken
	return m_ipSPM->Match1(ripInput->String, _bstr_t(strPattern.c_str()), 
		m_ipExprDefined, bGreedy ? VARIANT_TRUE : VARIANT_FALSE);
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CMERSHandler::patternSearch1(ISpatialString* pOriginalText)
{
	ISpatialStringPtr ipOriginText(pOriginalText);
	// get the actual input text
	string strInput = asString(ipOriginText->String);
	// pattern
	string strPattern("nominee for Lender^?LenderName");

	// find the match
	ISpatialStringPtr ipFoundString = findMatchEntity(ipOriginText, 
		strPattern, false, false, true);
	if (ipFoundString == __nullptr)
	{
		// if this pattern can't be found in the input string, 
		// no more further search is required.
		return NULL;
	}

	// if the value is found in the original input text
	// Do further searching based on the found string (ipFoundString)...

	ISpatialStringPtr ipEntity(NULL);

	///***************
	// 1) 
	strPattern = "( 888 ) 679 - MERS|- MERS^?LenderName^200~organized and existing";
	// try to extract entity out from the value
	ipEntity = findMatchEntity(ipFoundString, strPattern, true, true, false);
	if (ipEntity)
	{
		// only return the entity if it's not null
		return ipEntity;
	}

	///***************
	// 2) check to see if the first word in the found string
	// is starting with an upper case alphabetic character
	string strFoundValue = ipFoundString->String;
	if (isFirstWordStartsUpperCase(strFoundValue))
	{	
		// if is, look for the following pattern
		strPattern = "?LenderName^@-NonTP";
		// otherwise, try to extract entity out from the value
		ipEntity = findMatchEntity(ipFoundString, strPattern, true, true, true);
		
		if (ipEntity)
		{
			// only return the entity if it's not null
			return ipEntity;
		}
		// if nothing is found, go on to the next rule
	}
	
	//******************
	// 3) if '"Lender"^is^?^TP' is found...
	strPattern = "\" Lender \"^20~[is]^?LenderName^@-NonTP";
	// This time, pass in the found string, not the original string
	// Also, when calling findMatchEntity(), do not ask for extracting entity
	// at this time, we'll do it if...
	ipEntity = findMatchEntity(ipFoundString, strPattern, false, false, true);
	if (ipEntity)
	{
		// make sure the starting alpha character is an upper case
		strFoundValue = ipEntity->String;
		if (isFirstWordStartsUpperCase(strFoundValue))
		{
			// now extract the entity
			ipEntity = extractEntity(ipEntity);
			if (ipEntity)
			{
				return ipEntity;
			}
		}

		// if not, go onto next rule
	}
	// if nothing is found, go to next rule

	//******************
	// 4) if 'TP^?^"Lender"' is found...
	strPattern = "@-NonTP^?LenderName^(\"Lender|\"Lender\"";
	// This time, pass in the found string, not the original string
	ipEntity = findMatchEntity(ipFoundString, strPattern, false, false, false);
	if (ipEntity)
	{
		// sometimes, the found value contains a leftover MERS string in
		// the beginning.  Search for the word MERS in the found value...and
		// if found, chop off anything to the left of it (including itself).
		strFoundValue = asString(ipEntity->String);
		size_t pos = strFoundValue.find("MERS");
		if (pos != string::npos)
		{
			ipEntity = ipEntity->GetSubString(pos+4, -1);
			ASSERT_RESOURCE_ALLOCATION("ELI25954", ipEntity != __nullptr);

			// now extract a better entity
			ipEntity = extractEntity(ipEntity);
		}

		// return the found (and possibly corrected) entity
		return ipEntity;
	}
	// if nothing is found, go to next rule

	//******************
	// 5) TODO: Spatially search for the entity text above or below 'name of Lender'

	return NULL;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CMERSHandler::patternSearch2(ISpatialString* pOriginalText)
{
	ISpatialStringPtr ipOriginText(pOriginalText);
	// get the actual input text
	string strInput = asString(ipOriginText->String);
	// pattern
	string strPattern("as a nominee|nominee for^?LenderName^400~assignee|successor|address|owner and holder");

	// find the match and extract the entity
	return findMatchEntity(ipOriginText, strPattern, false, true, true);
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CMERSHandler::patternSearch3(ISpatialString* pOriginalText)
{
	// In this pattern set, we need to first make sure
	// the words "nominee for.." exists
	ISpatialStringPtr ipOriginText(pOriginalText);
	// get the actual input text
	string strInput = asString(ipOriginText->String);
	// pattern
	string strPattern("as a nominee|nominee for^?Dummy^@-NonTP");

	// find the match
	ISpatialStringPtr ipFoundString = findMatchEntity(ipOriginText, 
		strPattern, false, false, true);
	if (ipFoundString == __nullptr)
	{
		// if this pattern can't be found in the input string, 
		// no more further search shall be proceeded
		return NULL;
	}

	setPredefinedExpressions();

	// Search for "Borrower" is...borrower is..."Lender" is...lender is
	strPattern = "@LenderIs^?Lender^400~lender is";
	ipFoundString = findMatchEntity(ipOriginText, strPattern, false, true, false);

	return ipFoundString;
}
//-------------------------------------------------------------------------------------------------
void CMERSHandler::setPredefinedExpressions()
{
	if (m_ipExprDefined == __nullptr)
	{
		m_ipExprDefined.CreateInstance(CLSID_StrToStrMap);
		ASSERT_RESOURCE_ALLOCATION("ELI05991", m_ipExprDefined != __nullptr);
		m_ipExprDefined->Set(_bstr_t("NonTP"), _bstr_t(NON_TP.c_str()));
		m_ipExprDefined->Set(_bstr_t("BorrowerIs"), _bstr_t(BORROWER_IS.c_str()));
		m_ipExprDefined->Set(_bstr_t("LenderIs"), _bstr_t(LENDER_IS.c_str()));
	}
}
//-------------------------------------------------------------------------------------------------
IRegularExprParserPtr CMERSHandler::getParser()
{
	try
	{
		IRegularExprParserPtr ipParser = m_ipMiscUtils->GetNewRegExpParserInstance("MERSHandler");
		ASSERT_RESOURCE_ALLOCATION("ELI09488", ipParser != __nullptr);

		ipParser->Pattern = "(Mortgage\\s+Electronic\\s+Registration\\s+System(s)?"
			"|\\bMERS\\b|Mortgage[\\s\\S]+?Registration\\s+System(s)?)([\\s\\S]{1,3}Inc(\\s*\\.)?)?";

		return ipParser;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29456");
}
//-------------------------------------------------------------------------------------------------
void CMERSHandler::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI05918", "MERS Handler");
}
//-------------------------------------------------------------------------------------------------
