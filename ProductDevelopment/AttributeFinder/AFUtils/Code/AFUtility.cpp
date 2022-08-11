
// AFUtility.cpp : Implementation of CAFUtility

#include "stdafx.h"
#include "AFUtils.h"
#include "AFUtility.h"
#include "SpecialStringDefinitions.h"

#include <Common.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <RegistryPersistenceMgr.h>
#include <EncryptedFileManager.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <Misc.h>

/////////////
// Key Names
/////////////
static const string DOCTYPE_PREFIX = "Prefix";
static const string DOCTYPE_LOADFILE_PERSESSION = "LoadFilePerSession";

static const string DEFAULT_DOCTYPE_PREFIX = "";
static const string DEFAULT_DOCTYPE_LOADFILE_PERSESSION = "1";

/////////////
// Tag Names
/////////////
const string strRSD_FILE_DIR_TAG = "<RSDFileDir>";
const string strDOC_TYPE_TAG = "<" + DOC_TYPE + ">";
const string strCOMPONENT_DATA_DIR_TAG = "<ComponentDataDir>";
const string strSOURCE_DOC_NAME_TAG = "<SourceDocName>";
const string strSOURCE_DOC_EXT_TAG = "<SourceDocName.Extension>";
const string strSOURCE_DOC_FILENAME_TAG = "<SourceDocName.FileName>";
const string strSOURCE_DOC_PATH_TAG = "<SourceDocName.Path>";
const string strCOMMON_COMPONENTS_DIR_TAG = "<CommonComponentsDir>";

// globals and statics
map<string, string> CAFUtility::ms_mapCustomFileTagNameToValue;
CCriticalSection CAFUtility::ms_criticalSection;
map<long, CRuleSetProfiler> CAFUtility::ms_mapProfilers;
volatile long CAFUtility::ms_nNextProfilerHandle = 0;

//-------------------------------------------------------------------------------------------------
// CAFUtility
//-------------------------------------------------------------------------------------------------
CAFUtility::CAFUtility()
: ma_pUserCfgMgr(make_unique<RegistryPersistenceMgr>(HKEY_CURRENT_USER, gstrAF_REG_ROOT_FOLDER_PATH))
, ma_pMachineCfgMgr(make_unique<RegistryPersistenceMgr>(HKEY_LOCAL_MACHINE, gstrAF_REG_ROOT_FOLDER_PATH))
, m_ipMiscUtils(CLSID_MiscUtils)
, m_ipEngine(CLSID_AttributeFinderEngine)
, m_ipParser(__nullptr)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI19181", ma_pUserCfgMgr.get() != __nullptr );
		ASSERT_RESOURCE_ALLOCATION("ELI49951", ma_pMachineCfgMgr.get() != __nullptr );
		ASSERT_RESOURCE_ALLOCATION("ELI07623", m_ipMiscUtils != __nullptr);
		ASSERT_RESOURCE_ALLOCATION("ELI32488", m_ipEngine != __nullptr);
		m_strINIFileName = getModuleDirectory(_Module.m_hInst);
		m_strINIFileName += "\\UCLIDAFCore.INI";
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07338")
}
//-------------------------------------------------------------------------------------------------
CAFUtility::~CAFUtility()
{
	try
	{
		m_ipMiscUtils = __nullptr;
		m_ipEngine = __nullptr;
		m_ipParser = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20389")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAFUtility,
		&IID_ILicensedComponent,
		&IID_ITagUtility
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IAFUtility
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetNameToAttributesMap(IIUnknownVector *pVecAttributes, 
												IStrToObjectMap **ppMapNameToAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		IIUnknownVectorPtr ipVecAttributes(pVecAttributes);
		ASSERT_ARGUMENT("ELI06973", ipVecAttributes != __nullptr);
		ASSERT_ARGUMENT("ELI26567", ppMapNameToAttributes != __nullptr);

		// create the map for returning
		IStrToObjectMapPtr ipNameToAttributes(CLSID_StrToObjectMap);
		ASSERT_RESOURCE_ALLOCATION("ELI06974", ipNameToAttributes != __nullptr);

		// go through the attributes
		long nSize = ipVecAttributes->Size();
		for (long n=0; n<nSize; n++)
		{
			// get each attribute from ipVecAttributes
			IAttributePtr ipAttr = ipVecAttributes->At(n);
			ASSERT_RESOURCE_ALLOCATION("ELI06975", ipAttr != __nullptr);

			// Get the attribute name
			_bstr_t bstrAttrName = ipAttr->Name;

			// If the attribute name already exists in the map
			IIUnknownVectorPtr ipAttributesWithNameName =
				ipNameToAttributes->TryGetValue(bstrAttrName);
			if (ipAttributesWithNameName != __nullptr)
			{
				// Add this attribute to the vector
				ipAttributesWithNameName->PushBack(ipAttr);
			}
			else
			{
				// If the name entry doesn't exist yet, create an entry
				IIUnknownVectorPtr ipAttributesWithSameName(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI06976", ipAttributesWithSameName != __nullptr);
				ipAttributesWithSameName->PushBack(ipAttr);

				// Add to the map
				ipNameToAttributes->Set(bstrAttrName, ipAttributesWithSameName);
			}
		}

		*ppMapNameToAttributes = ipNameToAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06977");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetComponentDataFolder(IAFDocument *pAFDoc, BSTR *pstrComponentDataFolder)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI45666", ipAFDoc != __nullptr);

		// get the component data folder and return it
		string strFolder;
		getComponentDataFolder(ipAFDoc, strFolder);

		*pstrComponentDataFolder = _bstr_t(strFolder.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07101");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetComponentDataFolder2(BSTR bstrFKBVersion,
												 BSTR bstrAlternateComponentDataRoot,
												 BSTR *pstrComponentDataFolder)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// get the component data folder and return it
		string strFolder;
		strFolder = m_ipEngine->GetComponentDataFolder2(bstrFKBVersion, bstrAlternateComponentDataRoot);

		*pstrComponentDataFolder = _bstr_t(strFolder.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41609");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetPrefixedFileName(BSTR strNonPrefixFullPath, BSTR* pstrFileToRead)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Get local copy of string
		string	strNonPrefixFile = asString(strNonPrefixFullPath);

		// Check for RulesFilePrefix in registry
		string	strPrefix = getRulesFilePrefix();
		if (strPrefix.length() > 0)
		{
			// Get the file name and folder
			string strFileName = ::getFileNameFromFullPath(strNonPrefixFile);
			string strFolder = ::getDirectoryFromFullPath(strNonPrefixFile) + "\\";

			// Prepend the prefix to the filename
			string	strPrefixFile = strFolder + strPrefix + strFileName;

			// perform auto-encrypt actions on the file
			m_ipMiscUtils->AutoEncryptFile(_bstr_t(strPrefixFile.c_str()),
				_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()));

			// Check for file existence
			if (isFileOrFolderValid(strPrefixFile))
			{
				// Use the prefix file
				*pstrFileToRead = _bstr_t(strPrefixFile.c_str()).Detach();

				return S_OK;
			}
		}

		// perform auto-encrypt actions on the file
		m_ipMiscUtils->AutoEncryptFile(_bstr_t(strNonPrefixFile.c_str()),
			_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()));

		// Confirm that default file exists
		validateFileOrFolderExistence(strNonPrefixFile);

		// Just use the non-prefix file
		*pstrFileToRead = _bstr_t(strNonPrefixFile.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07332");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetLoadFilePerSession(VARIANT_BOOL *pbSetting)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license state
		validateLicense();

		// Check for key existence
		if (!ma_pUserCfgMgr->keyExists( gstrAF_REG_SETTINGS_FOLDER, DOCTYPE_LOADFILE_PERSESSION ))
		{
			// Default setting is true
			ma_pUserCfgMgr->createKey( gstrAF_REG_SETTINGS_FOLDER, DOCTYPE_LOADFILE_PERSESSION, 
				DEFAULT_DOCTYPE_LOADFILE_PERSESSION );

			*pbSetting = asVariantBool(asCppBool(DEFAULT_DOCTYPE_LOADFILE_PERSESSION));
		}

		// Get the existing setting
		bool bValue = ma_pUserCfgMgr->getKeyValue( 
			gstrAF_REG_SETTINGS_FOLDER, DOCTYPE_LOADFILE_PERSESSION,
			DEFAULT_DOCTYPE_LOADFILE_PERSESSION ) == "1";

		// Return setting to caller
		*pbSetting = asVariantBool(bValue);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07343");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::SetAutoEncrypt(VARIANT_BOOL bAutoEncryptOn)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Store the setting
		ma_pUserCfgMgr->setKeyValue( gstrAF_REG_SETTINGS_FOLDER, 
			gstrAF_AUTO_ENCRYPT_KEY, asCppBool(bAutoEncryptOn) ? "1" : "0" );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07348");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GenerateAttributesFromEAVFile(BSTR strEAVFileName,
													   IIUnknownVector **ppAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI26568", ppAttributes != __nullptr);

		IIUnknownVectorPtr ipRetAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI07362", ipRetAttributes != __nullptr);

		// Fill the vector with the attributes loaded from the file
		generateAttributesFromEAVFile(asString(strEAVFileName), ipRetAttributes);

		// Return the vector
		*ppAttributes = ipRetAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07359");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetAttributesFromDelimitedString(BSTR bstrAttributes, BSTR bstrDelimiter,
														  IIUnknownVector **ppAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI26569", ppAttributes != __nullptr);

		IIUnknownVectorPtr ipRetAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI19182", ipRetAttributes != __nullptr);

		string strDelimiter = asString(bstrDelimiter);
		string strAttributes = asString(bstrAttributes);
		vector<string> vecLines;

		StringTokenizer::sGetTokens(strAttributes, strDelimiter, vecLines);

		unsigned int uiCurrLine = 0;
		loadAttributesFromEavFile(ipRetAttributes, 0, uiCurrLine, vecLines);

		*ppAttributes = ipRetAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12306");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetAttributesAsString(IIUnknownVector *pAttributes, BSTR *pAttributesInString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI26570", pAttributesInString != __nullptr);

		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI07361", ipAttributes != __nullptr);

		// Get the count of attributes
		long nSize = ipAttributes->Size();

		// Default the string to empty
		string strAttributeString("");
		
		// If there is at least 1 attribute, add the first one to the string
		if (nSize > 0)
		{
			IAttributePtr ipAttribute = ipAttributes->At(0);
			ASSERT_RESOURCE_ALLOCATION("ELI26573", ipAttribute != __nullptr);

			strAttributeString = buildAttributeString(ipAttribute);
		}

		// For all other attributes, append a new line and then the attribute
		for (long n=1; n<nSize; n++)
		{
			strAttributeString += "\r\n";
			
			// Retrieve this Attribute
			IAttributePtr ipAttribute = ipAttributes->At(n);
			ASSERT_RESOURCE_ALLOCATION("ELI15579", ipAttribute != __nullptr);
			
			strAttributeString += buildAttributeString(ipAttribute);
		}

		*pAttributesInString = _bstr_t(strAttributeString.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07360");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::raw_ExpandTags(BSTR strInput, BSTR bstrSourceDocName, IUnknown *pData, VARIANT_BOOL vbStopEarly,
	BSTR *pstrOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// get the document as a smart pointer
		IAFDocumentPtr ipDoc(pData);
		ASSERT_RESOURCE_ALLOCATION("ELI35164", ipDoc != __nullptr);

		// Get the string from the input
		string stdstrInput = asString(strInput);

		// Expand the tags
		expandTags(stdstrInput, ipDoc, asCppBool(vbStopEarly));

		// return the string with the replacements made
		*pstrOutput = _bstr_t(stdstrInput.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35165");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::raw_ExpandFunction(BSTR bstrFunctionName, IVariantVector *pArgs,
	BSTR bstrSourceDocName, IUnknown *pData, BSTR *pbstrOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI43488", pbstrOutput != __nullptr);

		*pbstrOutput = _bstr_t().Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43489");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::ExpandTags(BSTR strInput, IAFDocument *pDoc, BSTR *pstrOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// get the document as a smart pointer
		IAFDocumentPtr ipDoc(pDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI07464", ipDoc != __nullptr);

		// Get the string from the input
		string stdstrInput = asString(strInput);

		// Expand the tags
		expandTags(stdstrInput, ipDoc, false);

		// return the string with the replacements made
		*pstrOutput = _bstr_t(stdstrInput.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07460");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::StringContainsTags(BSTR strInput, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26574", pbValue != __nullptr);

		validateLicense();

		// get the input as a STL string
		string stdstrInput = asString(strInput);
		
		// NOTE: getTagNames() is not supposed to be called for a string
		// that contains invalid tag specifications.  The getTagNames() 
		// method may throw exceptions if there is inappropriate usage of
		// tag indicating characters '<' and '>' (such as non-matching
		// pairs of these indicators, etc).  If such an exception is thrown,
		// it's OK for that to reach the outer scope.

		// get the tags in the string
		vector<string> vecTagNames = getTagNames(stdstrInput);

		// return true as long as there's at least one tag
		*pbValue = vecTagNames.empty() ? VARIANT_FALSE : VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07468");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::StringContainsInvalidTags(BSTR strInput, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// get the input as a STL string
		string stdstrInput = asString(strInput);
		
		// get the tags in the string
		vector<string> vecTagNames = getTagNames(stdstrInput);

		// if we reached here, that means that all tags defined were valid.
		// so return true.
		*pbValue = VARIANT_FALSE;
		return S_OK;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI07475");

	// if we reached here, its because there were incomplete tags or something
	// else went wrong, which indicates that there were some problems with
	// tag specifications.
	*pbValue = VARIANT_TRUE;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetAttributesFromFile(BSTR strFileName, IIUnknownVector **ppAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26575", ppAttributes != __nullptr);

		validateLicense();

		IIUnknownVectorPtr ipRetAttributes( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION( "ELI07854", ipRetAttributes != __nullptr );

		// Get file type
		string strFile = asString( strFileName );
		string strExt = getExtensionFromFullPath( strFile, true );
		if (strExt == ".eav")
		{
			// Read the attributes from the EAV file
			generateAttributesFromEAVFile(strFile, ipRetAttributes);
		}
		else if (strExt == ".voa" || strExt == ".evoa")
		{
			// Load the file into the IUnknownVector object directly
			ipRetAttributes->LoadFrom(strFileName, VARIANT_FALSE);

			// Confirm that each item in vector implements IAttribute
			long lCount = ipRetAttributes->Size();
			for (long i = 0; i < lCount; i++)
			{
				// Check for IAttribute support
				IAttributePtr ipAttr = ipRetAttributes->At( i );
				if (ipAttr == __nullptr)
				{
					// Throw exception
					UCLIDException ue( "ELI07871", 
						"Object loaded from file is not an attribute." );
					ue.addDebugInfo( "File Name", strFile );
					ue.addDebugInfo( "Item Number", i );
					throw ue;
				}
			}
		}
		else
		{
			// Throw exception
			UCLIDException ue( "ELI07859", "Cannot get attributes from unsupported file type." );
			ue.addDebugInfo( "File Type", strExt );
			throw ue;
		}

		// Return results
		*ppAttributes = ipRetAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07860");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::ApplyAttributeModifier(IIUnknownVector *pvecAttributes, IAFDocument *pDoc, 
												IAttributeModifyingRule *pAM, VARIANT_BOOL bRecursive)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Check arguments
		IIUnknownVectorPtr ipAttributes(pvecAttributes);
		ASSERT_ARGUMENT("ELI08688", ipAttributes != __nullptr);
		IAttributeModifyingRulePtr ipModifier(pAM);
		ASSERT_ARGUMENT("ELI08691", ipModifier != __nullptr);

		// Wrap document in smart pointer
		IAFDocumentPtr ipDoc(pDoc);

		// Apply the modifier
		applyAttributeModifier(ipAttributes, ipDoc, ipModifier, asCppBool(bRecursive));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08687");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::ExpandFormatString(IAttribute *pAttribute, BSTR bstrFormat,
											long nEndScopeChar, long *pnEndScopePos,
											ISpatialString** pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// use a smart pointer for the attribute
		IAttributePtr ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI09669", ipAttribute != __nullptr);

		// create a copy of the attribute's original value
		ICopyableObjectPtr ipCopy(ipAttribute->Value);
		ASSERT_RESOURCE_ALLOCATION("ELI17021", ipCopy != __nullptr);
		ISpatialStringPtr ipOutputSS = ipCopy->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI17019", ipOutputSS != __nullptr);

		// get the expanded string
		string strOriginalFormat = asString(bstrFormat);
		string strFormat = strOriginalFormat;
		ISpatialStringPtr ipExpandedSS =
			getReformattedName(strFormat, ipAttribute, false, (char)nEndScopeChar);

		if (ipExpandedSS == __nullptr)
		{
			// The format string could not be expanded due to a missing variable. Return an empty
			// SpatialString.
			ipOutputSS->Clear();
		}
		// replace the original string's value with the expanded string's value
		else if (ipExpandedSS->HasSpatialInfo() == VARIANT_TRUE)
		{
			// The expanded string has spatial info. Use this info to replace the original string.
			// [FlexIDSCore #3089]
			ipOutputSS = ipExpandedSS;
		}
		else if (ipOutputSS->IsEmpty() == VARIANT_TRUE)
		{
			ipOutputSS->ReplaceAndDowngradeToNonSpatial(ipExpandedSS->String);
		}
		else
		{
			// The expanded string is non spatial. Replace the original string to preserve its 
			// spatial info.
			ipOutputSS->Replace(ipOutputSS->String, ipExpandedSS->String, VARIANT_TRUE, 
				0, NULL);
		}

		// If within a scope delimited by custom chars, indicate the position at which the scope
		// ended in the originally supplied bstrFormat.
		if (pnEndScopePos != __nullptr)
		{
			if (strFormat.empty())
			{
				*pnEndScopePos = -1;
			}
			else
			{
				*pnEndScopePos = strOriginalFormat.length() - strFormat.length();
			}
		}

		// return the expanded spatial string
		*pRetVal = ipOutputSS.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19183");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::SortAttributesSpatially(IIUnknownVector* pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		validateLicense();
		
		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI11293", ipAttributes != __nullptr);

		ISortComparePtr ipCompare(CLSID_SpatiallyCompareAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI11292", ipCompare != __nullptr);
		
		ipAttributes->Sort(ipCompare);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11291");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::raw_GetBuiltInTags(IVariantVector** ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26578", ppTags != __nullptr);

		validateLicense();

		IVariantVectorPtr ipVec = getBuiltInTags();
		ASSERT_RESOURCE_ALLOCATION("ELI11773", ipVec != __nullptr);

		*ppTags = ipVec.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11770");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::raw_GetCustomFileTags(IVariantVector** ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		IVariantVectorPtr ipVec = loadCustomFileTagsFromINI();
		ASSERT_RESOURCE_ALLOCATION("ELI11774", ipVec != __nullptr);

		*ppTags = ipVec.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11771");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::raw_EditCustomTags(long hParentWindow)
{
	try
	{
		// Check license
		validateLicense();

		// No editing UI is provided; tags to be edited via INI.

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38062");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::raw_AddTag(BSTR bstrTagName, BSTR bstrTagValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI38086", bstrTagName != __nullptr);

		validateLicense();

		string strTag = asString(bstrTagName);
		if (strTag.substr(0, 1) != "<")
		{
			strTag = "<" + strTag;
		}
		if (strTag.substr(strTag.length() - 1, 1) != ">")
		{
			strTag += ">";
		}

		CSingleLock lock(&m_criticalSectionAddedTags, TRUE);
		m_mapAddedTags[strTag] = asString(bstrTagValue);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38087");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::raw_GetAddedTags(IIUnknownVector **ppStringPairTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI43278", ppStringPairTags != __nullptr);

		IIUnknownVectorPtr ipStringPairTags(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI43280", ipStringPairTags != __nullptr);

		CSingleLock lock(&m_criticalSectionAddedTags, TRUE);
		
		for (auto iter = m_mapAddedTags.begin(); iter != m_mapAddedTags.end(); iter++)
		{
			IStringPairPtr ipStringPair(CLSID_StringPair);
			ipStringPair->StringKey = iter->first.c_str();
			ipStringPair->StringValue = iter->second.c_str();
			ipStringPairTags->PushBack(ipStringPair);
		}
		*ppStringPairTags = ipStringPairTags.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43279");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::raw_GetAllTags(IVariantVector** ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		ASSERT_ARGUMENT("ELI26585", ppTags != __nullptr);

		validateLicense();

		// Get the built in tags
		IVariantVectorPtr ipVec1 = getBuiltInTags();
		ASSERT_RESOURCE_ALLOCATION("ELI26583", ipVec1 != __nullptr);

		// Get the custom tags
		IVariantVectorPtr ipVec2 = loadCustomFileTagsFromINI();
		ASSERT_RESOURCE_ALLOCATION("ELI26584", ipVec2 != __nullptr);

		// Append the custom tags to the built in tags
		ipVec1->Append(ipVec2);

		// Return the collection
		*ppTags = ipVec1.Detach();
	}	
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11772");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::get_ShouldCacheRSD(VARIANT_BOOL *pvbCacheRSD)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		// Check if the registry setting exists
		if (ma_pMachineCfgMgr->keyExists(gstrAF_REG_SETTINGS_FOLDER, gstrAF_CACHE_RSD_KEY))
		{
			// Check the registry setting
			string strValue = ma_pMachineCfgMgr->getKeyValue(gstrAF_REG_SETTINGS_FOLDER,
				gstrAF_CACHE_RSD_KEY, gstrAF_DEFAULT_CACHE_RSD);
			*pvbCacheRSD = asVariantBool(strValue == "1");
		}
		else
		{
			// Default to enabling RSD caching (https://extract.atlassian.net/browse/ISSUE-17019)
			ma_pMachineCfgMgr->createKey(gstrAF_REG_SETTINGS_FOLDER, gstrAF_CACHE_RSD_KEY,
				gstrAF_DEFAULT_CACHE_RSD);
			*pvbCacheRSD = asVariantBool(asCppBool(gstrAF_DEFAULT_CACHE_RSD));
		}

		// Per ISSUE-17019, The old key should be deleted to help avoid confusion as to which setting is controlling behavior
		ma_pUserCfgMgr->deleteKey(gstrAF_REG_SETTINGS_FOLDER, gstrAF_CACHE_RSD_KEY);

		return S_OK;
	}	
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24008");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::raw_ExpandTagsAndFunctions(BSTR bstrInput, BSTR bstrSourceDocName,
	IUnknown *pData, BSTR *pbstrOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI35168", pbstrOutput != __nullptr);

		validateLicense();

		UCLID_COMUTILSLib::ITagUtilityPtr ipThis(this);
		ASSERT_RESOURCE_ALLOCATION("ELI35169", ipThis != __nullptr);

		_bstr_t bstrOutput = m_ipMiscUtils->ExpandTagsAndFunctions(
			bstrInput, ipThis, bstrSourceDocName, pData);

		*pbstrOutput = bstrOutput.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35170");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::ExpandTagsAndFunctions(BSTR bstrInput, IAFDocument *pDoc,
												BSTR *pbstrOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI35171", pbstrOutput != __nullptr);

		validateLicense();

		UCLID_COMUTILSLib::ITagUtilityPtr ipThis(this);
		ASSERT_RESOURCE_ALLOCATION("ELI35172", ipThis != __nullptr);

		_bstr_t bstrOutput = m_ipMiscUtils->ExpandTagsAndFunctions(
			bstrInput, ipThis, "", pDoc);

		*pbstrOutput = bstrOutput.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35173");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::raw_GetFunctionNames(IVariantVector** ppFunctionNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI35174", ppFunctionNames != __nullptr);

		validateLicense();

		ITagUtilityPtr ipTagUtility(m_ipMiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI35175", ipTagUtility != __nullptr);

		IVariantVectorPtr ipFunctions = ipTagUtility->GetFunctionNames();
		ASSERT_RESOURCE_ALLOCATION("ELI35176", ipFunctions != __nullptr);

		*ppFunctionNames = ipFunctions.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35177");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::raw_GetFormattedFunctionNames(IVariantVector** ppFunctionNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI35178", ppFunctionNames != __nullptr);

		validateLicense();

		ITagUtilityPtr ipTagUtility(m_ipMiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI35179", ipTagUtility != __nullptr);

		IVariantVectorPtr ipFunctions = ipTagUtility->GetFormattedFunctionNames();
		ASSERT_RESOURCE_ALLOCATION("ELI35180", ipFunctions != __nullptr);

		*ppFunctionNames = ipFunctions.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35181");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::StartProfilingRule(BSTR bstrName, BSTR bstrType,
	IIdentifiableObject *pRuleObject, long nSubID, long* pnHandle)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI33749", pRuleObject != __nullptr);
		ASSERT_ARGUMENT("ELI33750", pnHandle != __nullptr);

		*pnHandle = ::InterlockedIncrement(&ms_nNextProfilerHandle);
		string strName = asString(bstrName);
		string strType = asString(bstrType);

		// Critical Section while accessing static collection
		CSingleLock lg(&ms_criticalSection, TRUE);

		ms_mapProfilers.insert(pair<long, CRuleSetProfiler>(
			*pnHandle, CRuleSetProfiler(strName, strType, pRuleObject, nSubID)));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33748");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::StopProfilingRule(long nHandle)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Critical Section while accessing static collection
		CSingleLock lg(&ms_criticalSection, TRUE);

		ms_mapProfilers.erase(nHandle);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33751");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::ValidateAsExplicitPath(BSTR bstrEliCode, BSTR bstrFilename)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		string strEliCode = asString(bstrEliCode);
		string strFilename = asString(bstrFilename);

		// If the filename contains tags, consider it valid.
		vector<string> vecTagNames = getTagNames(strFilename);
		if (vecTagNames.empty())
		{
			// If it doesn't contain tags, confirm this is a valid absolute path.
			// If the name isn't at least 2 characters, it can't be a valid absolute path.
			if (strFilename.length() <= 2)
			{
				UCLIDException ue(strEliCode, "Please specify a valid file name!");
				ue.addDebugInfo("File", strFilename);
				ue.addWin32ErrorInfo();
				throw ue;
			}

			string strRoot = strFilename.substr(0, 2);

			// An absolute path must begin with either a drive letter or double-backslash.
			if (strRoot != "\\\\" && (!isalpha(strRoot[0]) || strRoot[1] != ':'))
			{
				UCLIDException ue(strEliCode, "Explicit path required. Use a path tag or absolute path.");
				ue.addDebugInfo("File", strFilename);
				ue.addWin32ErrorInfo();
				throw ue;
			}
			// Ensure that the file exists
			else if (!isValidFile(strFilename))
			{
				UCLIDException ue(strEliCode, "Specified file does not exist!");
				ue.addDebugInfo("File", strFilename);
				ue.addWin32ErrorInfo();
				throw ue;
			}
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33845");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetNewRegExpParser(IAFDocument *pDoc, IRegularExprParser **ppRegExParser)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		
		ASSERT_ARGUMENT("ELI36218", ppRegExParser != __nullptr);
		ASSERT_ARGUMENT("ELI36223", pDoc != __nullptr);

		IRegularExprParserPtr ipRegExParser = m_ipMiscUtils->GetNewRegExpParserInstance("");
		ASSERT_RESOURCE_ALLOCATION("ELI36219", ipRegExParser != __nullptr);

		UCLID_AFUTILSLib::IAFExpressionFormatterPtr ipAFFormatter(CLSID_AFExpressionFormatter);
		ASSERT_RESOURCE_ALLOCATION("ELI36220", ipAFFormatter != __nullptr);

		ipAFFormatter->AFDocument = pDoc;

		IExpressionFormatterPtr ipFormatter(ipAFFormatter);
		ASSERT_RESOURCE_ALLOCATION("ELI36221", ipFormatter != __nullptr);

		ipRegExParser->ExpressionFormatter = ipFormatter;

		*ppRegExParser = ipRegExParser.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36222");
}
//-------------------------------------------------------------------------------------------------

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26586", pbValue != __nullptr);

		try
		{
			// check the license
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26587");
}

//-------------------------------------------------------------------------------------------------
// private / helper methods
//-------------------------------------------------------------------------------------------------
void CAFUtility::expandRSDFileDirTag(string& rstrInput,
									 IAFDocumentPtr& ripDoc)
{
	// if the <RSDFileDir> tag exists in the input string
	// make the corresponding substitution
	if (rstrInput.find(strRSD_FILE_DIR_TAG) != string::npos)
	{
		// Try to get the currently executing rule file's directory from the AFDocument
		string strDir = asString(ripDoc->GetCurrentRSDFileDir());

		// Else get from REE
		if (strDir.empty())
		{
			// Get the rule execution environment
			IRuleExecutionEnvPtr ipREE(CLSID_RuleExecutionEnv);
			ASSERT_RESOURCE_ALLOCATION("ELI07461", ipREE != __nullptr);

			// get the currently executing rule file's directory
			strDir = asString(ipREE->GetCurrentRSDFileDir());
		}

		// if there is no current RSD file, this tag cannot
		// be expanded.
		if (strDir.empty())
		{
			UCLIDException ue("ELI07500", "There is no current RSD file to expand the <RSDFileDir> tag.");
			ue.addDebugInfo("strInput", rstrInput);
			throw ue;
		}

		// replace instances of the tag with the value
		replaceVariable(rstrInput, strRSD_FILE_DIR_TAG, strDir);
	}
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::expandRuleExecIDTag(string& rstrInput,
									 IAFDocumentPtr& ripDoc)
{
	// if the <RuleExecutionID> tag exists in the input string
	// make the corresponding substitution
	if (rstrInput.find(gstrRULE_EXEC_ID_TAG) != string::npos)
	{
		// Retrieve existing String Tags from AFDocument
		IStrToStrMapPtr	ipStringTags = ripDoc->StringTags;
		ASSERT_RESOURCE_ALLOCATION("ELI09834", ipStringTags != __nullptr);

		// ensure that the rule execution ID tag exists
		if (ipStringTags->Contains(gstrRULE_EXEC_ID_TAG_NAME.c_str()) == VARIANT_FALSE)
		{
			string strMsg = "The ";
			strMsg += gstrRULE_EXEC_ID_TAG;
			strMsg += " tag cannot be expanded because the rule execution ID is not available.";
			throw UCLIDException("ELI09835", strMsg);
		}

		// get the rule execution ID
		string strRuleExecID = ipStringTags->GetValue(gstrRULE_EXEC_ID_TAG_NAME.c_str());

		// ensure that the rule execution ID string is not empty
		if (strRuleExecID.empty())
		{
			// this should never happen!
			THROW_LOGIC_ERROR_EXCEPTION("ELI09836")
		}

		// replace instances of the tag with the value
		replaceVariable(rstrInput, gstrRULE_EXEC_ID_TAG, strRuleExecID);
	}
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::expandSourceDocNameTag(string& rstrInput,
										IAFDocumentPtr& ripDoc)
{
	bool bSourceDocNameTagFound = rstrInput.find(strSOURCE_DOC_NAME_TAG) != string::npos;
	bool bSourceDocExtTagFound = rstrInput.find(strSOURCE_DOC_EXT_TAG) != string::npos;
	bool bSourceDocFileNameTagFound = rstrInput.find(strSOURCE_DOC_FILENAME_TAG) != string::npos;
	bool bSourceDocPathTagFound = rstrInput.find(strSOURCE_DOC_PATH_TAG) != string::npos;

	// if there are no source-document name related expansions to do, just
	// return.
	if (!bSourceDocNameTagFound && !bSourceDocExtTagFound &&
		!bSourceDocFileNameTagFound && !bSourceDocPathTagFound)
	{
		// no source-document related expansions to do.  Just return
		return;
	}

	// get the spatial string from the document
	ISpatialStringPtr ipDocText = ripDoc->Text;
	ASSERT_RESOURCE_ALLOCATION("ELI09070", ipDocText != __nullptr);

	// get the source document name from the spatial string
	string strSourceDocName = asString(ipDocText->SourceDocName);

	// if there is no current RSD file, this tag cannot
	// be expanded.
	if (strSourceDocName == "")
	{
		string strMsg = "There is no source document available to expand the ";
		strMsg += strSOURCE_DOC_NAME_TAG;
		strMsg += " tag.";
		UCLIDException ue("ELI09069", strMsg);
		ue.addDebugInfo("strInput", rstrInput);
		throw ue;
	}

	// expand the strSOURCE_DOC_NAME_TAG tag with the appropriate value
	if (bSourceDocNameTagFound)
	{
		replaceVariable(rstrInput, strSOURCE_DOC_NAME_TAG, strSourceDocName);
	}

	// expand the extension tag
	if (bSourceDocExtTagFound)
	{
		// get the extension and ensure it's not empty
		string strExt = getExtensionFromFullPath(strSourceDocName);
		if (strExt.length() <= 1)
		{
			string strMsg = "There is no extension available to expand the ";
			strMsg += strSOURCE_DOC_EXT_TAG;
			strMsg += " tag.";
			UCLIDException ue("ELI09819", strMsg);
			ue.addDebugInfo("strSourceDocName", strSourceDocName);
			throw ue;
		}

		// remove the first character, which is the dot in the extension
		strExt.erase(0, 1);

		// perform the expansion
		replaceVariable(rstrInput, strSOURCE_DOC_EXT_TAG, strExt);
	}

	// expand the filename tag
	if (bSourceDocFileNameTagFound)
	{
		// get the filename and ensure it's not empty
		string strFileName = getFileNameWithoutExtension(strSourceDocName);
		if (strFileName.empty())
		{
			string strMsg = "There is no filename available to expand the ";
			strMsg += strSOURCE_DOC_FILENAME_TAG;
			strMsg += " tag.";
			UCLIDException ue("ELI09820", strMsg);
			ue.addDebugInfo("strSourceDocName", strSourceDocName);
			throw ue;
		}

		// perform the expansion
		replaceVariable(rstrInput, strSOURCE_DOC_FILENAME_TAG, strFileName);
	}

	// expand the path tag
	if (bSourceDocPathTagFound)
	{
		// get the filename and ensure it's not empty
		string strPath = getDirectoryFromFullPath(strSourceDocName);
		if (strPath.empty())
		{
			string strMsg = "There is no path available to expand the ";
			strMsg += strSOURCE_DOC_PATH_TAG;
			strMsg += " tag.";
			UCLIDException ue("ELI09821", strMsg);
			ue.addDebugInfo("strSourceDocName", strSourceDocName);
			throw ue;
		}

		// perform the expansion
		replaceVariable(rstrInput, strSOURCE_DOC_PATH_TAG, strPath);
	}
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::expandDocTypeTag(string& rstrInput,
								  IAFDocumentPtr& ripDoc)
{
	// if the <DOCTYPE> tag exists in the input string
	// make the corresponding substitution
	if (rstrInput.find(strDOC_TYPE_TAG) != string::npos)
	{
		// get the object tags associated with the document
		IStrToObjectMapPtr ipObjTags(ripDoc->ObjectTags);
		ASSERT_RESOURCE_ALLOCATION("ELI07465", ipObjTags != __nullptr);

		// get the vector of document type names
		IVariantVectorPtr ipVecDocTypes = ipObjTags->TryGetValue(DOC_TYPE.c_str());

		// check to see if a string tag for the document type
		// exists.  If not, throw an exception
		if (ipVecDocTypes == __nullptr)
		{
			throw UCLIDException("ELI07466", "Document was not successfully classified.");
		}

		long nSize = ipVecDocTypes->Size;

		// If there's no doc type found or there are more than one 
		// type found, throw an exception
		if (nSize == 0)
		{
			throw UCLIDException("ELI07471", "No document type available.");
		}
		else if (nSize > 1)
		{
			throw UCLIDException("ELI07472", "No unique document type available.");
		}

		// get the string value for the document type tag
		string strDocType = asString(_bstr_t(ipVecDocTypes->GetItem(0)));
		
		// replace instances of the tag with the value
		replaceVariable(rstrInput, strDOC_TYPE_TAG, strDocType);
	}
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::expandComponentDataDirTag(string& rstrInput,
										   IAFDocumentPtr& ripDoc)
{
	// if the <ComponentDataDir> tag exists in the input string
	// make the corresponding substitution
	if (rstrInput.find(strCOMPONENT_DATA_DIR_TAG) != string::npos)
	{
		string strFolder;
		getComponentDataFolder(ripDoc, strFolder);

		// replace instances of the tag with the value
		replaceVariable(rstrInput, strCOMPONENT_DATA_DIR_TAG , strFolder);
	}
}
//-------------------------------------------------------------------------------------------------
bool CAFUtility::expandAFDocTags(string& rstrInput, IAFDocumentPtr& ripDoc, bool stopEarly)
{
	bool changed = false;

	IStrToStrMapPtr ipMap = ripDoc->GetStringTags();
	long nNumTags = ipMap->GetSize();
	for (long i = 0; i < nNumTags; i++)
	{
		CComBSTR bstrKey;
		CComBSTR bstrValue;
		ipMap->GetKeyValue(i, &bstrKey, &bstrValue);

		string strKey = asString(bstrKey);
		string strValue = asString(bstrValue);

		string strTag = "<" + strKey + ">";

		while (rstrInput.find(strTag) != string::npos)
		{
			// replace instances of the tag with the value
			changed = replaceVariable(rstrInput, strTag, strValue) || changed;
			if (changed && stopEarly)
			{
				break;
			}
		}
	}

	return changed;
}
//-------------------------------------------------------------------------------------------------
bool CAFUtility::getCustomTagValue(const string& strTagName, string& rstrTagValue)
{
	// remove the '<' and '>' from strTagName, and ensure
	long nLen = strTagName.size();
	if (nLen <= 2 || strTagName[0] != '<' ||
		strTagName[nLen - 1] != '>')
	{
		UCLIDException ue("ELI09817", "Invalid tag name.");
		ue.addDebugInfo(strTagName, strTagName);
		throw ue;
	}

	// Get the tag name without the < > symbols
	string strTag = strTagName.substr(1, nLen-2);

	// Scope for the critical section
	{
		// Critical Section while accessing static collection
		CSingleLock lg(&ms_criticalSection, TRUE);

		// check if we have already cached the value of the tag
		map<string, string>::iterator iter;
		iter = ms_mapCustomFileTagNameToValue.find(strTag);
		if (iter != ms_mapCustomFileTagNameToValue.end())
		{
			rstrTagValue = iter->second;
			return true;
		}

		// get the value of the tag from the INI file
		// NOTE: This code below is not very robust because it cannot
		// distinguish between a non-existent key in the INI file and
		// an existing key in the INI file with an empty value
		const char *pszSectionName = "ExpandableTags";
		char pszResult[1024];
		if (GetPrivateProfileString(pszSectionName, strTag.c_str(), "", 
			pszResult, sizeof(pszResult), m_strINIFileName.c_str()) <= 0)
		{
			return false;
		}

		// if the result is not-empty, cache it
		rstrTagValue = pszResult;
		if (!rstrTagValue.empty())
		{
			ms_mapCustomFileTagNameToValue[strTagName] = rstrTagValue;
		}
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CAFUtility::expandCustomFileTags(string& rstrInput, IAFDocumentPtr& ripDoc, bool stopEarly)
{
	bool tagWasReplaced = false;

	// Expand any tags that have been defined at the INI file level.
	long nStartSearchPos = 0;
	while (true)
	{
		// find the beginning and ending of a tag
		long nTagStartPos = rstrInput.find_first_of('<', nStartSearchPos);
		long nTagEndPos = rstrInput.find_first_of('>', nStartSearchPos);
		
		// if there are no more tags, just return
		if (nTagStartPos == string::npos && nTagEndPos == string::npos)
		{
			break;
		}

		// if there is not a matching pair of tags, that's an error condition
		if (nTagStartPos == string::npos || nTagEndPos == string::npos || 
			nTagEndPos < nTagStartPos)
		{
			UCLIDException ue("ELI19374", "Matching pairs of '<' and '>' not found.");
			ue.addDebugInfo("nTagStartPos", nTagStartPos);
			ue.addDebugInfo("nTagEndPos", nTagEndPos);
			ue.addDebugInfo("rstrInput", rstrInput);
			throw ue;
		}

		// a matching pair of tags have been found.  Get the tag name and
		// ensure that it is not empty
		string strTagName = rstrInput.substr(nTagStartPos,
			nTagEndPos - nTagStartPos + 1);
		if (strTagName.empty())
		{
			throw UCLIDException("ELI19375", "An empty tag name cannot be expanded.");
		}
	
		
		// retrieve the value of the tag from the INI file
		string strTagValue;
		// if the tag cannot be expanded from the INI FILE
		// we will continue
		if (!getCustomTagValue(strTagName, /* ref */ strTagValue))
		{
			// continue searching
			nStartSearchPos = nTagEndPos + 1;
			continue;
		}

		if (strTagValue.empty())
		{
			UCLIDException ue("ELI09815", "Cannot expand tag to an empty string.");
			ue.addDebugInfo("strTagName", strTagName);
			throw ue;
		}

		// replace the tag value
		rstrInput.erase(nTagStartPos, nTagEndPos - nTagStartPos + 1);
		rstrInput.insert(nTagStartPos, strTagValue);

		tagWasReplaced = true;

		if (stopEarly)
		{
			break;
		}

		// continue searching after the tag value
		nStartSearchPos = nTagStartPos + strTagValue.length() + 1;
	}

	return tagWasReplaced;
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::expandCommonComponentsDir(string& rstrInput)
{
	bool bCommonComponentsDirFound = rstrInput.find(strCOMMON_COMPONENTS_DIR_TAG) != string::npos;

	if (bCommonComponentsDirFound)
	{
		const string strCommonComponentsDir = getModuleDirectory("BaseUtils.dll");

		// Replace the common components dir tag
		replaceVariable(rstrInput, strCOMMON_COMPONENTS_DIR_TAG, strCommonComponentsDir);
	}
}
//-------------------------------------------------------------------------------------------------
vector<string> CAFUtility::getTagNames(const string& strInput) const
{
	// Create the result vector
	vector<string> vecTagNames;
	
	// get the tag names
	long nSearchStartPos = 0;
	while (true)
	{
		// find the beginning and ending of a tag
		long nTagStartPos = strInput.find_first_of('<', nSearchStartPos);
		long nTagEndPos = strInput.find_first_of('>', nSearchStartPos);
		
		// if there are no more tags, just return
		if (nTagStartPos == string::npos && nTagEndPos == string::npos)
		{
			break;
		}

		// if there is not a matching pair of tags, that's an error condition
		if (nTagStartPos == string::npos || nTagEndPos == string::npos || 
			nTagEndPos < nTagStartPos)
		{
			UCLIDException ue("ELI19184", "Matching pairs of '<' and '>' not found.");
			ue.addDebugInfo("nTagStartPos", nTagStartPos);
			ue.addDebugInfo("nTagEndPos", nTagEndPos);
			throw ue;
		}

		// a matching pair of tags have been found.  Get the tag name and
		// ensure that it is not empty
		string strTagName = strInput.substr(nTagStartPos,
			nTagEndPos - nTagStartPos + 1);
		if (strTagName.empty())
		{
			throw UCLIDException("ELI19185", "An empty tag name cannot be expanded.");
		}

		// continue searching at the next position
		vecTagNames.push_back(strTagName);
		nSearchStartPos = nTagEndPos + 1;
	}

	return vecTagNames;
}
//-------------------------------------------------------------------------------------------------
string CAFUtility::getRulesFilePrefix()
{
	// Check for key existence
	if (!ma_pUserCfgMgr->keyExists( gstrAF_REG_SETTINGS_FOLDER, DOCTYPE_PREFIX ))
	{
		string strPrefix(DEFAULT_DOCTYPE_PREFIX);
		ma_pUserCfgMgr->createKey( gstrAF_REG_SETTINGS_FOLDER, DOCTYPE_PREFIX,
			DEFAULT_DOCTYPE_PREFIX );

		return strPrefix;
	}

	return ma_pUserCfgMgr->getKeyValue(gstrAF_REG_SETTINGS_FOLDER, DOCTYPE_PREFIX,
		DEFAULT_DOCTYPE_PREFIX);
}
//-------------------------------------------------------------------------------------------------
bool CAFUtility::isAutoEncryptOn()
{
	// Check for key existence
	if (!ma_pUserCfgMgr->keyExists( gstrAF_REG_SETTINGS_FOLDER, gstrAF_AUTO_ENCRYPT_KEY ))
	{
		// Default setting is OFF
		string strAutoEncrypt( gstrAF_DEFAULT_AUTO_ENCRYPT );
		ma_pUserCfgMgr->createKey( gstrAF_REG_SETTINGS_FOLDER, gstrAF_AUTO_ENCRYPT_KEY, 
			gstrAF_DEFAULT_AUTO_ENCRYPT );

		return false;
	}

	return ma_pUserCfgMgr->getKeyValue(gstrAF_REG_SETTINGS_FOLDER, 
		gstrAF_AUTO_ENCRYPT_KEY, gstrAF_DEFAULT_AUTO_ENCRYPT) == "1";
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI06978", "AttributeFinder Utility");
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::getComponentDataFolder(IAFDocumentPtr ipAFDoc, string& rFolder)
{
	// Get the Component Data Folder from the AFDoc if possible
	// Changed this to not check the rsd stack so that I could use an AFDoc to pass along an FKB
	// without using a dummy RSD file
	// https://extract.atlassian.net/browse/ISSUE-15466
	_bstr_t bstrFKB = ipAFDoc->FKBVersion;
	_bstr_t bstrAltCCDir = ipAFDoc->AlternateComponentDataDir;
	if (SysStringLen(bstrFKB) != 0 || SysStringLen(bstrAltCCDir) != 0)
	{
		rFolder = m_ipEngine->
			GetComponentDataFolder2(bstrFKB, bstrAltCCDir);
	}
	else
	{
		// Else use the Rule Execution Env
		getComponentDataFolderFromRuleExecutionEnv(rFolder);
	}
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::getComponentDataFolderFromRuleExecutionEnv(string& rFolder)
{
	rFolder = m_ipEngine->GetComponentDataFolder();
}
//-------------------------------------------------------------------------------------------------
unsigned int CAFUtility::getAttributeLevel(const string& strName)
{
	return strName.find_first_not_of(".");
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::removeDots(string& rstrName)
{
	// Find the first non '.' character
	size_t pos = rstrName.find_first_not_of(".");

	if (pos != 0)
	{
		rstrName.erase(0, pos);
	}
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::loadAttributesFromEavFile(const IIUnknownVectorPtr& ipAttributes, 
										   unsigned long ulCurrLevel, 
										   unsigned int& uiCurrLine, vector<string> vecLines)
{
	IAttributePtr ipNewAttr = __nullptr;
	while (uiCurrLine < vecLines.size())
	{
		string strLine = vecLines[uiCurrLine];
		if (strLine.empty())
		{
			uiCurrLine++;
			continue;
		}

		unsigned int uiLevel = getAttributeLevel(strLine);
		if (uiLevel == ulCurrLevel)
		{
			ipNewAttr.CreateInstance(CLSID_Attribute);
			ASSERT_RESOURCE_ALLOCATION("ELI09538", ipNewAttr != __nullptr);
			removeDots(strLine);
			vector<string> vecTokens;
			StringTokenizer::sGetTokens(strLine, "|", vecTokens);
			if (vecTokens.size() < 2)
			{
				UCLIDException ue("ELI09539", "Invalid Attribute Specification.");
				ue.addDebugInfo("Line", strLine);
				throw ue;
			}
			ipNewAttr->Name = _bstr_t(vecTokens[0].c_str());
			string strValue = vecTokens[1];
			::convertNormalStringToCppString(strValue);
			ISpatialStringPtr ipValue = ipNewAttr->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI25947", ipValue != __nullptr);
			ipValue->ReplaceAndDowngradeToNonSpatial(strValue.c_str());

			if (vecTokens.size() > 2)
			{
				ipNewAttr->Type = _bstr_t(vecTokens[2].c_str());
			}	

			ipAttributes->PushBack(ipNewAttr);
			uiCurrLine++;
		}
		else if (uiLevel == ulCurrLevel + 1)
		{
			loadAttributesFromEavFile( ipNewAttr->SubAttributes, ulCurrLevel + 1, 
				uiCurrLine, vecLines );
		}
		else if (uiLevel < ulCurrLevel)
		{
			return;
		}
		else
		{
			UCLIDException ue("ELI19186", "Missing Parent Attribute Specification.");
			ue.addDebugInfo("Line", strLine);
			throw ue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CAFUtility::getReformattedName(string& strFormat,
												 const IAttributePtr& ipAttribute,
												 bool bScopeCloseExpected/* = false*/,
												 char cEndScopeChar/* = '\0'*/)
{
	string strOriginalFormat = strFormat;

	try
	{
		try
		{
			ISpatialStringPtr ipNewName(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI19187", ipNewName != __nullptr);

			// If cEndScopeChar was specified, a scope close is expected whether bScopeCloseExpected was
			// set or not.
			bScopeCloseExpected |= (cEndScopeChar != '\0');
			unsigned int ui;
			unsigned int uiLength = strFormat.length();
			bool bEscapeNextChar = false;
			bool bIgnoreResult = false;
			bool bFoundScopeClose = false;

			// Since calls to SpatialString::AppendString are expensive, rather than add all
			// literal chars to ipNewName one-by-one as they are encountered, buffer them until the
			// next nested scope, variable, or the end of the format string is encountered.
			string strPendingChars;

			for (ui = 0; ui < uiLength; ui++)
			{
				char c = strFormat.at(ui);

				// A new scope has been opened
				if (!bEscapeNextChar && c == '<')
				{
					string strScopeFormat = strFormat.substr(ui + 1);
					ISpatialStringPtr ipScopeStr = getReformattedName(strScopeFormat, ipAttribute, true);
					if (ipScopeStr != __nullptr)
					{
						if (!strPendingChars.empty())
						{
							ipNewName->AppendString(strPendingChars.c_str());
							strPendingChars = "";
						}

						ipNewName->Append(ipScopeStr);
					}

					// Pick up processing for the format string where the explicit scope ended.
					strFormat = strFormat.substr(0, ui) + strScopeFormat;
					uiLength = strFormat.length();
					// Reprocess at the position where the format string scope had been.
					ui--;
				}
				// The existing scope has been closed.
				else if (!bEscapeNextChar && (c == '>' || c == cEndScopeChar))
				{
					if (!bScopeCloseExpected)
					{
						UCLIDException ue("ELI36224",
							"An unexpected character was encountered in a format string.");
						ue.addDebugInfo("Character", c);
						throw ue;
					}

					bFoundScopeClose = true;
					// Remove the scope close char from the input format string before returning.
					strFormat = strFormat.substr(ui + 1);
					break;
				}
				// A variable
				else if (!bEscapeNextChar && c == '%')
				{
					string strVariableFormat = strFormat.substr(ui + 1);
					ISpatialStringPtr ipValue = parseVariableValue(strVariableFormat, ipAttribute);

					// Pick up processing for the format string where the variable definition ended.
					strFormat = strFormat.substr(0, ui) + strVariableFormat;
					uiLength = strFormat.length();
					// Reprocess at the position where the variable had been.
					ui--;

					if (ipValue == __nullptr)
					{
						// If the returned value is null, nothing should be returned for the current scope.
						bIgnoreResult = true;
					}
					else
					{
						if (!strPendingChars.empty())
						{
							ipNewName->AppendString(strPendingChars.c_str());
							strPendingChars = "";
						}

						ipNewName->Append(ipValue);
					}
				}
				else
				{
					if (!bEscapeNextChar && c == '\\')
					{
						// The character following an escape char should be treated as the literal char.
						bEscapeNextChar = true;
					}
					else
					{
						// Any other chars should just be appended to the result
						strPendingChars += c;
						bEscapeNextChar = false;
					}
				}
			}

			if (bScopeCloseExpected && !bFoundScopeClose)
			{
				UCLIDException ue("ELI37168", "A format string scope was not properly closed.");
				throw ue;
			}

			if (bIgnoreResult)
			{
				return __nullptr;
			}
			else
			{
				if (!strPendingChars.empty())
				{
					ipNewName->AppendString(strPendingChars.c_str());
				}

				return ipNewName;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36244");
	}
	catch (UCLIDException &ue)
	{
		UCLIDException ueOuter("ELI36245", "Unable to parse format string.", ue);
		ueOuter.addDebugInfo("Format string", strOriginalFormat, true);
		throw ueOuter;
	}
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CAFUtility::parseVariableValue(string& strVariable, const IAttributePtr& ipAttribute)
{
	string strOriginalVariable = strVariable;

	try
	{
		try
		{
			unsigned int ui;
			unsigned int uiLength = strVariable.length();
			bool bEscapeNextChar = false;
			bool bFoundClosingChar = false;
			string strQuery;
			string strSelection;
			string strDelim;
			bool bBuildingQuery = true;
			bool bBuildingSelector = false;
			bool bBuildingDelimiter = false;
			int nMaxValueLen = -1;

			// The expanded result.
			ISpatialStringPtr ipText(__nullptr);
			// A vector to hold the results of attributes expanded via a nested format string.
			IIUnknownVectorPtr ipNestedValues(__nullptr);

			for (ui = 0; ui < uiLength; ui++)
			{
				char c = strVariable.at(ui);

				// The variable scope has ended.
				if (!bEscapeNextChar && c == '%')
				{
					bFoundClosingChar = true;
					break;
				}
				// The query term of the variable has ended.
				else if (!bEscapeNextChar && bBuildingQuery && c == '<')
				{
					if (ui == uiLength - 1)
					{
						UCLIDException ue("ELI36225", "Unexpected end of format string variable.");
						throw ue;
					}

					// Check for the case that the nested format string is not specified.
					if (strVariable[ui + 1] == '>')
					{
						// Resume processing after the closing bracket.
						strVariable = strVariable.substr(0, ui) + strVariable.substr(ui);
						uiLength = strVariable.length();
						ui++;
					}
					// A nested format string to expand the reference attribute(s) follows.
					else
					{
						// Create a vector to hold the results of attributes expanded via a nested
						// format string.
						ipNestedValues.CreateInstance(CLSID_IUnknownVector);
						ASSERT_RESOURCE_ALLOCATION("ELI36226", ipNestedValues != __nullptr);

						string strNestedFormat = strVariable.substr(ui + 1);
						string strTempNestedFormat = strNestedFormat;

						// See if the expanded value is to be truncated after a specified number of chars.
						nMaxValueLen = getMaxValueLen(strQuery);

						// Get the attributes to be expanded.
						IIUnknownVectorPtr ipSubAttributes = (ipAttribute == __nullptr)
							? IIUnknownVectorPtr(CLSID_IUnknownVector)
							: getCandidateAttributesEnhanced(ipAttribute, strQuery);
				
						// If there are not attributes to expand, getReformattedName still needs to
						// be called in order to trim the term from strTempNestedFormat.
						size_t nCount = ipSubAttributes->Size();
						if (nCount == 0)
						{
							getReformattedName(strTempNestedFormat, __nullptr, true);
						}
						// Iterate all attributes to be expanded and expand them into ipNestedValues.
						else
						{
							for (size_t nIndex = 0; nIndex < nCount; nIndex++)
							{
								IAttributePtr ipSubAttribute = ipSubAttributes->At(nIndex);
								ASSERT_RESOURCE_ALLOCATION("ELI36227", ipSubAttribute != __nullptr);

								strTempNestedFormat = strNestedFormat;
								ISpatialStringPtr ipValue =
									getReformattedName(strTempNestedFormat, ipSubAttribute, true);

								if (ipValue != __nullptr)
								{
									ipNestedValues->PushBack(ipValue);
								}
							}
						}

						// Resume processing after the nested format string.
						strVariable = strVariable.substr(0, ui) + strTempNestedFormat;
						uiLength = strVariable.length();
						// Reprocess at the position where the nested format string had been.
						ui--;
					}

					bBuildingQuery = false;
					bBuildingSelector = true;
				}
				// The selection term of the variable has ended.
				else if (!bEscapeNextChar && bBuildingSelector && c == ':')
				{
					bBuildingSelector = false;
					bBuildingDelimiter = true;
				}
				// Any other chars should be appended to either the query, selector or the delimiter
				// term .(whichever is currently being built)
				else
				{
					if (!bEscapeNextChar && c == '\\')
					{
						bEscapeNextChar = true;
					}
					else
					{
						if (bBuildingQuery)
						{
							strQuery += c;
						}
						else if (bBuildingSelector)
						{
							strSelection += c;
						}
						else if (bBuildingDelimiter)
						{
							strDelim += c;
						}
						else
						{
							THROW_LOGIC_ERROR_EXCEPTION("ELI36235");
						}

						bEscapeNextChar = false;
					}
				}
			}

			if (!bFoundClosingChar || strQuery.empty())
			{
				UCLIDException ue("ELI36228", "Invalid variable in Format String.");
				ue.addDebugInfo("Variable", strVariable);
				throw ue;
			}

			if (strSelection.empty() && !strDelim.empty())
			{
				UCLIDException ue("ELI36252",
					"Unexpected delimiter without selection in Format String");
				ue.addDebugInfo("Selector", strSelection);
				ue.addDebugInfo("Delimiter", strDelim);
				throw ue;
			}

			// If variables haven't been expanded with a nested format string, get the variable value.
			if (ipNestedValues == __nullptr)
			{
				// See if the expanded value is to be truncated after a specified number of chars.
				nMaxValueLen = getMaxValueLen(strQuery);
				ipText = getVariableValue(strQuery, ipAttribute, strSelection, strDelim);
			}
			// If variables have been expanded with a nested format string, get the variable value.
			else
			{
				IIUnknownVectorPtr ipSelectedValues = getSelectedItems(ipNestedValues, strSelection);
				size_t nCount = ipSelectedValues->Size();

				bool bUniqueOnly = _strcmpi(strSelection.c_str(), "uniq") == 0 ||
								   _strcmpi(strSelection.c_str(), "unique") == 0;
				set<string> setUsedValues;

				IIUnknownVectorPtr ipValues(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI36248", ipValues != __nullptr);

				// Iterate through all selected attributes in order to collect values to concatenate.
				for (size_t ui = 0; ui < nCount; ui++)
				{
					ISpatialStringPtr ipValue = ipSelectedValues->At(ui);
					ASSERT_RESOURCE_ALLOCATION("ELI36229", ipValue != __nullptr);

					if (bUniqueOnly)
					{
						string strValue = asString(ipValue->String);
						if (setUsedValues.find(strValue) != setUsedValues.end())
						{
							continue;
						}

						setUsedValues.insert(strValue);
					}
		
					ICopyableObjectPtr ipSource = (ICopyableObjectPtr)ipValue;
					ASSERT_RESOURCE_ALLOCATION("ELI36230", ipSource != __nullptr);

					ISpatialStringPtr ipValueCopy = (ISpatialStringPtr)ipSource->Clone();
					ipValues->PushBack(ipValueCopy);
				}

				// Iterate through all selected values, and concatenate (delimiting as specified).
				nCount = ipValues->Size();
				for (size_t ui = 0; ui < nCount; ui++)
				{
					ISpatialStringPtr ipValue = ipValues->At(ui);
					ASSERT_RESOURCE_ALLOCATION("ELI36247", ipValue != __nullptr);

					if (ipText == nullptr)
					{
						ipText = ipValue;
					}
					else
					{
						ipText->Append(ipValue);
					}

					// Add the delimiter if not the last value
					if (ui < nCount - 1 && !strDelim.empty())
					{
						ipText->AppendString(strDelim.c_str());
					}
				}
			}

			// Truncate the result to the specified number of chars if necessary.
			if (ipText != __nullptr)
			{
				if (nMaxValueLen > 0 && ipText->Size > nMaxValueLen)
				{
					ipText = ipText->GetSubString(0, nMaxValueLen - 1);
				}
				else if (nMaxValueLen == 0)
				{
					ipText.CreateInstance(CLSID_SpatialString);
					ASSERT_RESOURCE_ALLOCATION("ELI36232", ipText != __nullptr);
				}
			}

			// Trim the return value so that it no longer contains the processed variable.
			strVariable = strVariable.substr(ui + 1);
			return ipText;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36242");
	}
	catch (UCLIDException &ue)
	{
		UCLIDException ueOuter("ELI36243", "Unable to parse format string variable.", ue);
		ueOuter.addDebugInfo("Variable", strOriginalVariable, true);
		throw ueOuter;
	}
}
//-------------------------------------------------------------------------------------------------
long CAFUtility::getMaxValueLen(string& strQuery)
{
	string strNum;

	// All leading digits should be interpreted as the maximum expanded length (if the expanded
	// value is longer, it shall be truncated).
	while (!strQuery.empty())
	{
		char c = strQuery[0];
		if (isdigit(c))
		{
			strNum += c;
			strQuery = strQuery.substr(1);
		}
		else
		{
			break;
		}
	}

	if (strNum.empty())
	{
		return -1;
	}
	else
	{
		return asLong(strNum);
	}
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CAFUtility::getSelectedItems(const IIUnknownVectorPtr& ipItems,
												string strSelection)
{
	try
	{
		try
		{
			size_t nCount = ipItems->Size();

			IIUnknownVectorPtr ipSelectedItems(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI36236", ipSelectedItems != __nullptr);

			if(nCount > 0)
			{
				// Handle special selection keywords
				if (strSelection.empty() || _strcmpi(strSelection.c_str(), "first") == 0)
				{
					strSelection = "1";
				}
				else if (_strcmpi(strSelection.c_str(), "last") == 0)
				{
					strSelection = "-1";
				}
				else if (_strcmpi(strSelection.c_str(), "all") == 0 ||
						 _strcmpi(strSelection.c_str(), "uniq") == 0 ||
						 _strcmpi(strSelection.c_str(), "unique") == 0)
				{
					strSelection = "1-";
				}
				else
				{
					// If the selection term is not a keyword, ensure it is a numerical list or range.
					for (size_t i = 0; i < strSelection.length(); i++)
					{
						char c = strSelection[i];
						if (!isDigitChar(c) && !isWhitespaceChar(c) && c != '-' && c != ',')
						{
							UCLIDException ue("ELI36251", "Invalid format string selection term.");
							ue.addDebugInfo("Selection term", strSelection, true);
							throw ue;
						}
					}
				}

				// getPageNumbers was written with 1-based sequences in mind, not 0-based sequences.
				// It can be used, but nCount should be considered the "last page" and the returned
				// indices must then be decremented before use.
				vector<int> vecIndices = getPageNumbers(nCount, strSelection);
				nCount = vecIndices.size();

				for (size_t ui = 0; ui < nCount; ui++)
				{
					// Decrement each index to account for getPageNumbers being written for 1-based
					// sequences.
					long nIndex = vecIndices[ui] - 1;

					ipSelectedItems->PushBack(ipItems->At(nIndex));
				}
			}

			return ipSelectedItems;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36240");
	}
	catch (UCLIDException &ue)
	{
		UCLIDException ueOuter("ELI36241", "Unable to parse format string selection term.", ue);
		ueOuter.addDebugInfo("Selection term", strSelection, true);
		throw ueOuter;
	}
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CAFUtility::getVariableValue(const string& strQuery,
											   const IAttributePtr& ipAttribute,
											   const string& strSelection, const string& strDelim)
{
	// TODO: check the cache
	// bool bFound = false;

	vector<string> vecTokens;
	StringTokenizer::sGetTokens(strQuery, ".", vecTokens);
	// There should only be a max of 2 tokens 
	// Token 1 should be an xpath query
	// Token 2 should be "Value", "Type" or "Name"

	// eFieldType determines whether to return Name, Type or Value
	enum EVariableType { kGetType, kGetName, kGetValue };
	EVariableType eFieldType = kGetValue;

	if(vecTokens.size() > 2)
	{
		UCLIDException ue("ELI09677", "Invalid Variable Query.");
		ue.addDebugInfo("Invalid Variable", strQuery);
		throw ue;
	}

	// If the specified attribute is null, return null regardless of what the variable query is.
	if (ipAttribute == __nullptr)
	{
		return __nullptr;
	}

	string strQueryPart1 = vecTokens[0];
	
	// Set use current node flag. Will set to false if sole token is not "Type" or "Value"
	bool bUseCurrentNode = vecTokens.size() == 1 || strQueryPart1 == "";

	if (vecTokens.size() == 2)
	{
		if(vecTokens[1] == "Type")
		{
			eFieldType = kGetType;
		}
		else if(vecTokens[1] == "Name")
		{
			eFieldType = kGetName;
		}
		else if(vecTokens[1] == "Value")
		{
		}
		else
		{
			UCLIDException ue("ELI09704", "Invalid Attribute Field in Variable.");
			ue.addDebugInfo("Invalid Field", vecTokens[1]);
			throw ue;
		}
	}
	else
	{
		if(vecTokens[0] == "Type")
		{
			eFieldType = kGetType;
		}
		else if(vecTokens[0] == "Value")
		{
		}
		else
		{
			bUseCurrentNode = false;
		}
	}

	ISpatialStringPtr ipNewString = __nullptr;
	if(bUseCurrentNode && eFieldType == kGetType)
	{
		// This means that the variable is %Type% or %.Type% which is just the Type of the
		// Selected Attribute

		ipNewString.CreateInstance(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI09707", ipNewString != __nullptr);
		string str = asString(ipAttribute->Type);
		if(str == "")
		{
			return __nullptr;
		}
		ipNewString->CreateNonSpatialString(str.c_str(), "");
	}
	else if(bUseCurrentNode && eFieldType == kGetName)
	{
		// This means that the variable is %Name% or %.Name% which is just the Name of the
		// Selected Attribute

		ipNewString.CreateInstance(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI37771", ipNewString != __nullptr);
		string str = asString(ipAttribute->Name);
		if(str == "")
		{
			return __nullptr;
		}
		ipNewString->CreateNonSpatialString(str.c_str(), "");
	}
	else if(bUseCurrentNode && eFieldType == kGetValue)
	{
		ipNewString = ipAttribute->Value;
	}
	else
	{
		IIUnknownVectorPtr ipSubAttributes =
			getCandidateAttributesEnhanced(ipAttribute, strQueryPart1);
		ASSERT_RESOURCE_ALLOCATION("ELI25948", ipSubAttributes != __nullptr);

		IIUnknownVectorPtr ipSelectedAttributes = getSelectedItems(ipSubAttributes, strSelection);
		size_t nCount = ipSelectedAttributes->Size();

		// If there are no attributes matching the query, return null so that the current scope
		// evaluates to nothing.
		if(nCount == 0)
		{
			return __nullptr;
		}

		bool bUniqueOnly = _strcmpi(strSelection.c_str(), "uniq") == 0 ||
						   _strcmpi(strSelection.c_str(), "unique") == 0;
		set<string> setUsedValues;

		IIUnknownVectorPtr ipValues(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI36246", ipValues != __nullptr);

		// Iterate through all selected attributes in order to collect values to concatenate.
		for (size_t ui = 0; ui < nCount; ui++)
		{
			IAttributePtr ipFoundAttr = ipSelectedAttributes->At(ui);
			ASSERT_RESOURCE_ALLOCATION("ELI09706", ipFoundAttr != __nullptr);
		
			if(eFieldType == kGetType)
			{
				if (bUniqueOnly)
				{
					string strType = asString(ipFoundAttr->Type);
					if (setUsedValues.find(strType) != setUsedValues.end())
					{
						continue;
					}

					setUsedValues.insert(strType);
				}

				ISpatialStringPtr ipValue(CLSID_SpatialString);
				ASSERT_RESOURCE_ALLOCATION("ELI19189", ipValue != __nullptr);
				ipValue->CreateNonSpatialString(ipFoundAttr->Type, "");
				ipValues->PushBack(ipValue);
			}
			else if(eFieldType == kGetName)
			{
				if (bUniqueOnly)
				{
					string strName = asString(ipFoundAttr->Name);
					if (setUsedValues.find(strName) != setUsedValues.end())
					{
						continue;
					}

					setUsedValues.insert(strName);
				}

				ISpatialStringPtr ipValue(CLSID_SpatialString);
				ASSERT_RESOURCE_ALLOCATION("ELI37770", ipValue != __nullptr);
				ipValue->CreateNonSpatialString(ipFoundAttr->Name, "");
				ipValues->PushBack(ipValue);
			}
			else
			{
				ISpatialStringPtr ipValue = ipFoundAttr->Value;
				ASSERT_RESOURCE_ALLOCATION("ELI36237", ipValue != __nullptr);

				if (bUniqueOnly)
				{
					string strValue = asString(ipValue->String);
					if (setUsedValues.find(strValue) != setUsedValues.end())
					{
						continue;
					}

					setUsedValues.insert(strValue);
				}

				ICopyableObjectPtr ipSource = (ICopyableObjectPtr)ipValue;
				ASSERT_RESOURCE_ALLOCATION("ELI36238", ipSource != __nullptr);

				ISpatialStringPtr ipValueCopy = (ISpatialStringPtr)ipSource->Clone();
				ASSERT_RESOURCE_ALLOCATION("ELI36239", ipValueCopy != __nullptr);
				ipValues->PushBack(ipValueCopy);
			}
		}

		// Iterate through all selected values, and concatenate (delimiting as specified).
		nCount = ipValues->Size();
		for (size_t ui = 0; ui < nCount; ui++)
		{
			ISpatialStringPtr ipValue = ipValues->At(ui);
			ASSERT_RESOURCE_ALLOCATION("ELI37167", ipValue != __nullptr);

			if (ipNewString == nullptr)
			{
				ipNewString = ipValue;
			}
			else
			{
				ipNewString->Append(ipValue);
			}

			// Add the delimiter if not the last value
			if (ui < nCount - 1 && !strDelim.empty())
			{
				ipNewString->AppendString(strDelim.c_str());
			}
		}
	}

	return ipNewString;
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::expandTags(string& rstrInput, IAFDocumentPtr ipDoc, bool stopEarly)
{
	try
	{
		ASSERT_ARGUMENT("ELI26167", ipDoc != __nullptr);

		// Expand the custom tags first, because the custom tag may use one or more of the other tags
		// Return if something was replaced and stopEarly is true
		if (expandCustomFileTags(rstrInput, ipDoc, stopEarly) && stopEarly)
		{
			return;
		}

		// expand the various other tags
		expandRSDFileDirTag(rstrInput, ipDoc);
		expandRuleExecIDTag(rstrInput, ipDoc);
		expandSourceDocNameTag(rstrInput, ipDoc);
		expandDocTypeTag(rstrInput, ipDoc);
		expandComponentDataDirTag(rstrInput, ipDoc);

		// Return if something was replaced and stopEarly is true
		if (expandAFDocTags(rstrInput, ipDoc, stopEarly) && stopEarly)
		{
			return;
		}

		expandCommonComponentsDir(rstrInput);

		// Expand any programmatically added tags.
		CSingleLock lock(&m_criticalSectionAddedTags, TRUE);
		for (map<string, string>::iterator iter = m_mapAddedTags.begin();
				iter != m_mapAddedTags.end();
				iter++)
		{
			string strTag = iter->first;
			string strValue = iter->second;

			// Return if something was replaced and stopEarly is true
			if (replaceVariable(rstrInput, strTag, strValue) && stopEarly)
			{
				return;
			}
		}

		// at this time, ensure that there are no more tags left
		vector<string> vecTagNames = getTagNames(rstrInput);
		if (!vecTagNames.empty())
		{
			UCLIDException ue("ELI09818", "One or more tag names could not be expanded.");
			ue.addDebugInfo("rstrInput", rstrInput);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26168");
}
//-------------------------------------------------------------------------------------------------
string CAFUtility::buildAttributeString(const IAttributePtr& ipAttribute)
{
	try
	{
		ASSERT_ARGUMENT("ELI26571", ipAttribute != __nullptr);

		// Append the Name
		string strAttribute = asString(ipAttribute->Name);
		strAttribute += "|";

		// Retrieve the Value
		ISpatialStringPtr ipValue = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI15580", ipValue != __nullptr);
		string strValue = asString(ipValue->String);

		// convert any cpp string (ex. \r, \n, etc. )to normal string
		// (ex. \\r, \\n, etc.) for display purpose
		::convertCppStringToNormalString(strValue);

		// Append the value to the string
		strAttribute += strValue;

		// add type if only it's not empty
		string strType = asString(ipAttribute->Type);
		if (!strType.empty())
		{
			strAttribute += "|";
			strAttribute += strType;
		}

		// Return the string
		return strAttribute;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26572");
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::generateAttributesFromEAVFile(const string& strFileName,
											   const IIUnknownVectorPtr& ipVector)
{
	try
	{
		ifstream ifs(strFileName.c_str());

		// confirm file is open
		if(!ifs.fail())
		{
			CommentedTextFileReader fileReader(ifs, "//", true);
			string strLine("");
			vector<string> vecLines;

			while (!ifs.eof())
			{
				strLine = fileReader.getLineText();
				vecLines.push_back(strLine);
			}

			unsigned int uiCurrLine = 0;

			loadAttributesFromEavFile(ipVector, 0, uiCurrLine, vecLines);
		}
		else
		{
			UCLIDException ue("ELI13035", "Unable to open file.");
			ue.addDebugInfo("Filename", strFileName);
			throw ue;
		}

		ifs.close();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26576");
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::applyAttributeModifier(const IIUnknownVectorPtr &ipAttributes,
										const IAFDocumentPtr &ipAFDoc,
										const IAttributeModifyingRulePtr &ipModifier,
										bool bRecursive)
{
	try
	{
		// Check arguments
		// Apply Attribute Modifier to each IAttribute
		long lCount = ipAttributes->Size();
		for (long i = 0; i < lCount; i++)
		{
			// Retrieve this Attribute
			IAttributePtr ipAttribute = ipAttributes->At( i );
			ASSERT_RESOURCE_ALLOCATION( "ELI08689", ipAttribute != __nullptr );

			// "Modify" the Attribute
			ipModifier->ModifyValue( ipAttribute, ipAFDoc, NULL );

			// If recursing then operate on the SubAttributes
			if (bRecursive)
			{
				// Get the sub attributes
				IIUnknownVectorPtr ipSubAttributes = ipAttribute->SubAttributes;
				if (ipSubAttributes != __nullptr)
				{
					applyAttributeModifier(ipSubAttributes, ipAFDoc, ipModifier, true);
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26577");
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr CAFUtility::getBuiltInTags()
{
	try
	{
		// Get the built in tags as a variant vector
		IVariantVectorPtr ipVec(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI26582", ipVec != __nullptr);

		// Add the tags to the vector
		ipVec->PushBack(get_bstr_t(strRSD_FILE_DIR_TAG));
		ipVec->PushBack(get_bstr_t(strDOC_TYPE_TAG));
		ipVec->PushBack(get_bstr_t(strCOMPONENT_DATA_DIR_TAG));
		ipVec->PushBack(get_bstr_t(strSOURCE_DOC_NAME_TAG));
		ipVec->PushBack(get_bstr_t(gstrRULE_EXEC_ID_TAG));
		ipVec->PushBack(get_bstr_t(strCOMMON_COMPONENTS_DIR_TAG));

		// Report any programmatically added tags.
		CSingleLock lock(&m_criticalSectionAddedTags, TRUE);
		for (map<string, string>::iterator iter = m_mapAddedTags.begin();
			 iter != m_mapAddedTags.end();
			 iter++)
		{
			ipVec->PushBack(get_bstr_t(iter->first));
		}

		// Return the vector
		return ipVec;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26579");
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr CAFUtility::loadCustomFileTagsFromINI()
{
	try
	{
		// Create a new Variant vector
		IVariantVectorPtr ipVec(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI26581", ipVec != __nullptr);

		// get the value of the tag from the INI file
		// NOTE: This code below is not very robust because it cannot
		// distinguish between a non-existent key in the INI file and
		// an existing key in the INI file with an empty value
		const char *pszSectionName = "ExpandableTags";
		char pszResult[2048];

		long nRet = GetPrivateProfileSection(pszSectionName, pszResult, sizeof(pszResult),
			m_strINIFileName.c_str());

		// if the section has tags
		if (nRet > 0)
		{
			long nCurrPos = 0;
			while(1)
			{
				string str = &pszResult[nCurrPos];
				nCurrPos += (str.length() + 1);

				long nIndex = str.find('=', 0);
				str = str.substr(0, nIndex);
				str = "<" + str + ">";
				ipVec->PushBack(get_bstr_t(str));

				if (pszResult[nCurrPos] == '\0')
				{
					break;
				}
			}
		}

		return ipVec;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26580");
}
//-------------------------------------------------------------------------------------------------
