// AdvancedReplaceString.cpp : Implementation of CAdvancedReplaceString
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "AdvancedReplaceString.h"
#include "Common.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <AFTagManager.h>
#include <EncryptedFileManager.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CAdvancedReplaceString
//-------------------------------------------------------------------------------------------------
CAdvancedReplaceString::CAdvancedReplaceString()
: m_strToBeReplaced(""),
  m_strReplacement(""),
  m_bCaseSensitive(false),
  m_bAsRegularExpression(false),
  m_eOccurrenceType(kAllOccurrences),
  m_nSpecifiedOccurrence(0),
  m_bDirty(false)
{
	try
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13055", m_ipMiscUtils != __nullptr );
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI06795")
}
//-------------------------------------------------------------------------------------------------
CAdvancedReplaceString::~CAdvancedReplaceString()
{
	try
	{
		m_ipMiscUtils = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16353");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAdvancedReplaceString,
		&IID_IAttributeModifyingRule,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent,
		&IID_IMustBeConfiguredObject,
		&IID_IDocumentPreprocessor
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
// IAdvancedReplaceString
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::get_StrToBeReplaced(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strToBeReplaced.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04991");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::put_StrToBeReplaced(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Check for empty string (P16 #1727)
		string strToBeReplaced = asString(newVal);
		if (strToBeReplaced.empty())
		{
			UCLIDException ue("ELI05819", "Please specify a non-empty string to be replaced.");
			ue.addDebugInfo("String to be replaced", strToBeReplaced);
			throw ue;
		}

		m_strToBeReplaced = strToBeReplaced;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04992");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::get_Replacement(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strReplacement.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04993");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::put_Replacement(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Check for empty string (P16 #1727)
		m_strReplacement = asString(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04994");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::get_IsCaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04995");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::put_IsCaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bCaseSensitive = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04996");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::get_AsRegularExpression(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bAsRegularExpression ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04997");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::put_AsRegularExpression(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bAsRegularExpression = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04998");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::get_ReplacementOccurrenceType(EReplacementOccurrenceType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_eOccurrenceType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04999");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::put_ReplacementOccurrenceType(EReplacementOccurrenceType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_eOccurrenceType = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05000");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::get_SpecifiedOccurrence(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_nSpecifiedOccurrence;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05001");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::put_SpecifiedOccurrence(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (newVal <= 0)
		{
			UCLIDException ue("ELI05818", "Please specify a meaningful occurrence number.");
			ue.addDebugInfo("Invalid occurrence number", newVal);
			ue.addPossibleResolution("For example, \"1\" means first occurrence, \"4\" means fourth occurrence, etc.");
			throw ue;
		}

		m_nSpecifiedOccurrence = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05002");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
// Given strInput and strReplacement, find out what is the actual
// replacement for defined pattern in pRegExpr
// Example: pattern = (the)\B, 
//			input = other
//			replacment = $1_abc
//			actual replacment = the_abc
//
string getActualReplacment(IRegularExprParser *pRegExpr,
						   const string& strInput,
						   const string& strReplacement)
{
	IRegularExprParserPtr ipRegExpr(pRegExpr);
	// find the pattern to be replaced
	IIUnknownVectorPtr ipMatches = ipRegExpr->Find(_bstr_t(strInput.c_str()), VARIANT_TRUE, 
		VARIANT_FALSE);
	if (ipMatches == __nullptr || ipMatches->Size() <= 0)
	{
		return "";
	}

	IObjectPairPtr ipMatchInfo = ipMatches->At(0);
	ASSERT_RESOURCE_ALLOCATION("ELI07313", ipMatchInfo != __nullptr);
	ITokenPtr ipMatch = ipMatchInfo->Object1;
	ASSERT_RESOURCE_ALLOCATION("ELI07314", ipMatch != __nullptr);
	// get the start and end positions for the match
	long nStartPos = ipMatch->StartPosition;
	long nEndPos = ipMatch->EndPosition;
	
	// get the input after replacement
	string strReplacedInput = ipRegExpr->ReplaceMatches(
		_bstr_t(strInput.c_str()), _bstr_t(strReplacement.c_str()), VARIANT_TRUE);
	
	// get the string after the match
	string strRemainingString = strInput.substr(nEndPos+1);
	string strActualReplacement("");
	if (!strRemainingString.empty())
	{
		// find the start position of remaining string in the replaced string
		int nFound = strReplacedInput.rfind(strRemainingString);
		if (nFound != string::npos)
		{
			strActualReplacement = strReplacedInput.substr(nStartPos, nFound-nStartPos);
		}
	}
	else
	{
		strActualReplacement = strReplacedInput.substr(nStartPos);
	}

	return strActualReplacement;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput, 
													 IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAttributePtr	ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION( "ELI09285", ipAttribute != __nullptr );

		ISpatialStringPtr ipInputText = ipAttribute->GetValue();
		ASSERT_RESOURCE_ALLOCATION( "ELI09282", ipInputText != __nullptr );

		// Call local method
		modifyValue( ipInputText, pOriginInput );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04953");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEMODIFIERSLib::IAdvancedReplaceStringPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08271", ipSource != __nullptr);
		m_strToBeReplaced = asString(ipSource->GetStrToBeReplaced());
		m_strReplacement = asString(ipSource->GetReplacement());
		m_bCaseSensitive = (ipSource->GetIsCaseSensitive()==VARIANT_TRUE) ? true : false;
		m_bAsRegularExpression = (ipSource->GetAsRegularExpression()==VARIANT_TRUE) ? true : false;
		m_eOccurrenceType = (EReplacementOccurrenceType)ipSource->GetReplacementOccurrenceType();
		m_nSpecifiedOccurrence = ipSource->GetSpecifiedOccurrence();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08272");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_AdvancedReplaceString);
		ASSERT_RESOURCE_ALLOCATION("ELI08358", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04955");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19596", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Advanced replace string").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04956");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		bool bConfigured = !m_strToBeReplaced.empty();
		if (m_eOccurrenceType == kSpecifiedOccurrence)
		{
			bConfigured = bConfigured && m_nSpecifiedOccurrence > 0;
		}

		*pbValue = bConfigured ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04957")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IDocumentPreprocessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::raw_Process(IAFDocument* pDocument, IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAFDocumentPtr ipInputDoc(pDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI07433", ipInputDoc != __nullptr);

		// Call local method
		modifyValue( ipInputDoc->Text, ipInputDoc );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07432");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_AdvancedReplaceString;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the variables first
		m_strToBeReplaced = "";
		m_strReplacement = "";
		m_bCaseSensitive = false;
		m_bAsRegularExpression = false;
		m_eOccurrenceType = kAllOccurrences;
		m_nSpecifiedOccurrence = 0;

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
			UCLIDException ue( "ELI07651", "Unable to load newer AdvancedReplaceString Modifier." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_strToBeReplaced;
			dataReader >> m_strReplacement;
			dataReader >> m_bCaseSensitive;
			dataReader >> m_bAsRegularExpression;
			long nOccurrenceType = 0;
			dataReader >> nOccurrenceType;
			m_eOccurrenceType = (EReplacementOccurrenceType)nOccurrenceType;
			dataReader >> m_nSpecifiedOccurrence;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04958");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_strToBeReplaced;
		dataWriter << m_strReplacement;
		dataWriter << m_bCaseSensitive;
		dataWriter << m_bAsRegularExpression;
		dataWriter << (long)m_eOccurrenceType;
		dataWriter << m_nSpecifiedOccurrence;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04959");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAdvancedReplaceString::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CAdvancedReplaceString::modifyValue(ISpatialString* pText, IAFDocument* pAFDoc)
{
	ISpatialStringPtr ipInputText(pText);
	ASSERT_RESOURCE_ALLOCATION( "ELI09304", ipInputText != __nullptr );

	// Get the actual string from file if m_strToBeReplaced and m_strReplacement are valid file names
	string strStringToBeReplaced = m_strToBeReplaced;
	string strStringReplacement = m_strReplacement;
	getStringsFromFiles(pAFDoc, strStringToBeReplaced, strStringReplacement);

	// Get the occurrence type
	long lOccurrence = 0;
	switch (m_eOccurrenceType)
	{
	case kAllOccurrences:
		break;

	case kFirstOccurrence:
		lOccurrence = 1;
		break;

	case kLastOccurrence:
		lOccurrence = -1;
		break;

	case kSpecifiedOccurrence:
		lOccurrence = m_nSpecifiedOccurrence;
		break;

	default:
		UCLIDException ue("ELI25273", "Unexpected occurrence type");
		ue.addDebugInfo("Occurrence type number", m_eOccurrenceType);
		throw ue;
	}

	IRegularExprParserPtr ipParser = __nullptr;
	if (m_bAsRegularExpression)
	{
		ipParser = m_ipMiscUtils->GetNewRegExpParserInstance("AdvancedReplaceString");
		ASSERT_RESOURCE_ALLOCATION("ELI06796", ipParser != __nullptr);
	}

	// Replace specified occurrence(s) of the pattern string
	ipInputText->Replace(strStringToBeReplaced.c_str(), strStringReplacement.c_str(), 
		asVariantBool(m_bCaseSensitive), lOccurrence, ipParser);
}
//-------------------------------------------------------------------------------------------------
void CAdvancedReplaceString::getStringsFromFiles(IAFDocument* pAFDoc, std::string& strFind, std::string& strReplaced)
{
	IAFDocumentPtr ipAFDoc(pAFDoc);
	ASSERT_RESOURCE_ALLOCATION("ELI14510", ipAFDoc != __nullptr);

	// Define a tag manager object to expand tags
	AFTagManager tagMgr;

	// Define an IMiscUtilsPtr object used to get string from file
	IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
	ASSERT_RESOURCE_ALLOCATION("ELI14529", ipMiscUtils != __nullptr );

	// Try to remove the header if the string is a file name, 
	// otherwise simply return the original string.
	string strAfterRemoveHeader = asString( ipMiscUtils->GetFileNameWithoutHeader(_bstr_t(strFind.c_str())) );

	// Expand tags only happens when the string is a file name
	// if it is not, all tags will be treated as common strings [P16: 2118]

	// Compare the new string with the original string
	if (strFind != strAfterRemoveHeader)
	{
		// Expand tags and functions in the file name
		strFind = tagMgr.expandTagsAndFunctions(strAfterRemoveHeader, ipAFDoc);

		// perform any appropriate auto-encrypt actions on the file that contains the string
		ipMiscUtils->AutoEncryptFile(get_bstr_t(strFind.c_str()),
			get_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()));

		// If the header has been removed, call getFindString() to load the strings inside the file
		strFind = getFindString(strFind);
	}

	// Try to remove the header if the string is a file name, 
	// otherwise simply return the original string.
	strAfterRemoveHeader = asString( ipMiscUtils->GetFileNameWithoutHeader(_bstr_t(strReplaced.c_str())) );
	// Compare the new string with the original string
	if (strReplaced != strAfterRemoveHeader)
	{
		// Expand tags and functions in the file name
		strReplaced = tagMgr.expandTagsAndFunctions(strAfterRemoveHeader, ipAFDoc);

		// perform any appropriate auto-encrypt actions on the file that contains the string
		ipMiscUtils->AutoEncryptFile(get_bstr_t(strReplaced.c_str()),
			get_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()));

		// If the header has been removed, call getReplaceString() to load the strings inside the file
		strReplaced = getReplaceString(strReplaced);
	}
}
//-------------------------------------------------------------------------------------------------
string CAdvancedReplaceString::getFindString(std::string strFile)
{
	// Call loadObjectFromFile() to load the string from the file
	m_cachedFindStringLoader.loadObjectFromFile(strFile);

	// Return the string got from file
	return m_cachedFindStringLoader.m_obj;
}
//-------------------------------------------------------------------------------------------------
string CAdvancedReplaceString::getReplaceString(std::string strFile)
{
	// Call loadObjectFromFile() to load the string from the file
	m_cachedReplaceStringLoader.loadObjectFromFile(strFile);

	// Return the string got from file
	return m_cachedReplaceStringLoader.m_obj;
}
//-------------------------------------------------------------------------------------------------
void CAdvancedReplaceString::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04952", "Advanced Replace String" );
}
//-------------------------------------------------------------------------------------------------
