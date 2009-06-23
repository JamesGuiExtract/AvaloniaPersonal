
// AFUtility.cpp : Implementation of CAFUtility

#include "stdafx.h"
#include "AFUtils.h"
#include "AFUtility.h"
#include "SpecialStringDefinitions.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <RegistryPersistenceMgr.h>
#include <EncryptedFileManager.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <TextFunctionExpander.h>

#include <afxmt.h>
#include <io.h>
#include <stdio.h>
#include <stdlib.h>

using namespace std;

/////////////
// Key Names
/////////////
static const string DOCTYPE_PREFIX = "Prefix";
static const string DOCTYPE_LOADFILE_PERSESSION = "LoadFilePerSession";

/////////////
// Tag Names
/////////////
const std::string strRSD_FILE_DIR_TAG = "<RSDFileDir>";
const std::string strDOC_TYPE_TAG = "<" + DOC_TYPE + ">";
const std::string strCOMPONENT_DATA_DIR_TAG = "<ComponentDataDir>";
const std::string strSOURCE_DOC_NAME_TAG = "<SourceDocName>";
const std::string strSOURCE_DOC_EXT_TAG = "<SourceDocName.Extension>";
const std::string strSOURCE_DOC_FILENAME_TAG = "<SourceDocName.FileName>";
const std::string strSOURCE_DOC_PATH_TAG = "<SourceDocName.Path>";

// globals and statics
std::map<std::string, std::string> CAFUtility::ms_mapINIFileTagNameToValue;
//std::vector<std::string> CAFUtility::ms_vecValidTags;
CMutex CAFUtility::ms_mutex;
 
//-------------------------------------------------------------------------------------------------
// CAFUtility
//-------------------------------------------------------------------------------------------------
CAFUtility::CAFUtility()
{
	try
	{
		// create instance of the persistence mgr
		ma_pUserCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr( HKEY_CURRENT_USER, gstrAF_REG_ROOT_FOLDER_PATH ));
		ASSERT_RESOURCE_ALLOCATION( "ELI19181", ma_pUserCfgMgr.get() != NULL );

		// create instance of the misc utils object
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI07623", m_ipMiscUtils != NULL);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07338")
}
//-------------------------------------------------------------------------------------------------
CAFUtility::~CAFUtility()
{
	try
	{
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
// IAFUtility
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetNameToAttributesMap(IIUnknownVector *pVecAttributes, 
												IStrToObjectMap **ppMapNameToAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();

		IIUnknownVectorPtr ipVecAttributes(pVecAttributes);
		ASSERT_ARGUMENT("ELI06973", ipVecAttributes != NULL);

		// create the map for returning
		IStrToObjectMapPtr ipNameToAttributes(CLSID_StrToObjectMap);
		ASSERT_RESOURCE_ALLOCATION("ELI06974", ipNameToAttributes != NULL);

		// go through the attributes
		long nSize = ipVecAttributes->Size();
		for (long n=0; n<nSize; n++)
		{
			// get each attribute from ipVecAttributes
			IAttributePtr ipAttr(ipVecAttributes->At(n));
			ASSERT_RESOURCE_ALLOCATION("ELI06975", ipAttr != NULL);

			_bstr_t _bstrAttrName = ipAttr->Name;
			// if the attribute name already exists in the map
			if (ipNameToAttributes->Contains(_bstrAttrName) == VARIANT_TRUE)
			{
				// get the vec containing attributes with the same name
				IIUnknownVectorPtr ipAttributesWithSameName 
					= ipNameToAttributes->GetValue(_bstrAttrName);
				// add this attribute to the vector
				ipAttributesWithSameName->PushBack(ipAttr);
			}
			else
			{
				// if the name entry doesn't exist yet, create an entry
				IIUnknownVectorPtr ipAttributesWithSameName(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI06976", ipAttributesWithSameName != NULL);
				ipAttributesWithSameName->PushBack(ipAttr);

				// add to the map
				ipNameToAttributes->Set(_bstrAttrName, ipAttributesWithSameName);
			}
		}

		*ppMapNameToAttributes = ipNameToAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06977");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetComponentDataFolder(BSTR *pstrComponentDataFolder)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();

		// get the component data folder and return it
		string strFolder;
		getComponentDataFolder(strFolder);

		*pstrComponentDataFolder = _bstr_t(strFolder.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07101");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetPrefixedFileName(BSTR strNonPrefixFullPath, BSTR* pstrFileToRead)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CSingleLock lg( &ms_mutex, TRUE );
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
			if (isFileOrFolderValid(strPrefixFile.c_str()))
			{
				// Use the prefix file
				*pstrFileToRead = _bstr_t(strPrefixFile.c_str()).copy();

				return S_OK;
			}
		}
		
		// perform auto-encrypt actions on the file
		m_ipMiscUtils->AutoEncryptFile(_bstr_t(strNonPrefixFile.c_str()),
			_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()));

		// Confirm that default file exists
		validateFileOrFolderExistence(strNonPrefixFile);

		// Just use the non-prefix file
		*pstrFileToRead = _bstr_t(strNonPrefixFile.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07332");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetLoadFilePerSession(VARIANT_BOOL *pbSetting)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CSingleLock lg( &ms_mutex, TRUE );
		// Check license state
		validateLicense();

		// Check for key existence
		if (!ma_pUserCfgMgr->keyExists( gstrAF_REG_SETTINGS_FOLDER, DOCTYPE_LOADFILE_PERSESSION ))
		{
			// Default setting is true
			string strPerSession( "1" );
			ma_pUserCfgMgr->createKey( gstrAF_REG_SETTINGS_FOLDER, DOCTYPE_LOADFILE_PERSESSION, 
				strPerSession );

			*pbSetting = VARIANT_TRUE;
		}

		// Get the existing setting
		bool bValue = ma_pUserCfgMgr->getKeyValue( 
			gstrAF_REG_SETTINGS_FOLDER, DOCTYPE_LOADFILE_PERSESSION ) == "1";

		// Return setting to caller
		*pbSetting = bValue ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07343");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::SetAutoEncrypt(VARIANT_BOOL bAutoEncryptOn)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();

		// Translate setting to string
		string	strSetting;
		if (bAutoEncryptOn == VARIANT_TRUE)
		{
			strSetting = "1";
		}
		else
		{
			strSetting = "0";
		}

		// Store the setting
		ma_pUserCfgMgr->setKeyValue( gstrAF_REG_SETTINGS_FOLDER, 
			gstrAF_AUTO_ENCRYPT_KEY, strSetting );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07348");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GenerateAttributesFromEAVFile(BSTR strEAVFileName, IIUnknownVector **ppAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();

		IIUnknownVectorPtr ipRetAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI07362", ipRetAttributes != NULL);

		string strEAVFile = asString(strEAVFileName);
		ifstream ifs(strEAVFile.c_str());

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

			loadAttributesFromEavFile(ipRetAttributes, 0, uiCurrLine, vecLines);
			*ppAttributes = ipRetAttributes.Detach();
		}
		else
		{
			UCLIDException ue("ELI13035", "Unable to open file!");
			ue.addDebugInfo("Filename", strEAVFile);
			throw ue;
		}

		ifs.close();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07359");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetAttributesFromDelimitedString(BSTR bstrAttributes, BSTR bstrDelimiter, IIUnknownVector **ppAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();

		IIUnknownVectorPtr ipRetAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI19182", ipRetAttributes != NULL);

		std::string strDelimiter = asString(bstrDelimiter);
		std::string strAttributes = asString(bstrAttributes);
		std::vector<std::string> vecLines;

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
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();

		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI07361", ipAttributes != NULL);

		string strAttributeString("");
		
		long nSize = ipAttributes->Size();
		for (long n=0; n<nSize; n++)
		{
			// add line break if this is not the first item
			if (n > 0)
			{
				strAttributeString += "\r\n";
			}
			
			// Retrieve this Attribute
			IAttributePtr ipAttribute = ipAttributes->At(n);
			ASSERT_RESOURCE_ALLOCATION("ELI15579", ipAttribute != NULL);
			
			// Append the Name
			string strAttribute = ipAttribute->Name;
			strAttribute += "|";
			
			// Retrieve and append the Value
			ISpatialStringPtr ipValue = ipAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI15580", ipValue != NULL);
			string strValue = ipValue->String;

			// convert any cpp string (ex. \r, \n, etc. )to normal string
			// (ex. \\r, \\n, etc.) for display purpose
			::convertCppStringToNormalString(strValue);
			
			strAttribute += strValue;
			// add type if only it's not empty
			string strType = ipAttribute->Type;
			if (!strType.empty())
			{
				strAttribute += "|";
				strAttribute += strType;
			}

			// Append this Attribute to string of collected Attributes
			strAttributeString += strAttribute;
		}

		*pAttributesInString = _bstr_t(strAttributeString.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07360");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::expandRSDFileDirTag(std::string& rstrInput,
									 IAFDocumentPtr& ripDoc)
{
	// if the <RSDFileDir> tag exists in the input string
	// make the corresponding substitution
	if (rstrInput.find(strRSD_FILE_DIR_TAG) != string::npos)
	{
		// if the Rule Execution Environment object has not
		// yet been created, create it
		if (m_ipREE == NULL)
		{
			m_ipREE.CreateInstance(CLSID_RuleExecutionEnv);
			ASSERT_RESOURCE_ALLOCATION("ELI07461", m_ipREE != NULL);
		}

		// get the currently executing rule file's directory
		string strDir = m_ipREE->GetCurrentRSDFileDir();

		// if there is no current RSD file, this tag cannot
		// be expanded.
		if (strDir == "")
		{
			UCLIDException ue("ELI07500", "There is no current RSD file to expand the <RSDFileDir> tag!");
			ue.addDebugInfo("strInput", rstrInput);
			throw ue;
		}

		// replace instances of the tag with the value
		replaceVariable(rstrInput, strRSD_FILE_DIR_TAG, strDir);
	}
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::expandRuleExecIDTag(std::string& rstrInput,
									 IAFDocumentPtr& ripDoc)
{
	// if the <RuleExecutionID> tag exists in the input string
	// make the corresponding substitution
	if (rstrInput.find(gstrRULE_EXEC_ID_TAG) != string::npos)
	{
		// Retrieve existing String Tags from AFDocument
		IStrToStrMapPtr	ipStringTags = ripDoc->StringTags;
		ASSERT_RESOURCE_ALLOCATION("ELI09834", ipStringTags != NULL);

		// ensure that the rule execution ID tag exists
		if (ipStringTags->Contains(gstrRULE_EXEC_ID_TAG_NAME.c_str()) == VARIANT_FALSE)
		{
			string strMsg = "The ";
			strMsg += gstrRULE_EXEC_ID_TAG;
			strMsg += " tag cannot be expanded because the rule execution ID is not available!";
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
void CAFUtility::expandSourceDocNameTag(std::string& rstrInput,
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
	ASSERT_RESOURCE_ALLOCATION("ELI09070", ipDocText != NULL);

	// get the source document name from the spatial string
	string strSourceDocName = asString(ipDocText->SourceDocName);

	// if there is no current RSD file, this tag cannot
	// be expanded.
	if (strSourceDocName == "")
	{
		string strMsg = "There is no source document available to expand the ";
		strMsg += strSOURCE_DOC_NAME_TAG;
		strMsg += " tag!";
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
			strMsg += " tag!";
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
		// get the filname and ensure it's not empty
		string strFileName = getFileNameWithoutExtension(strSourceDocName);
		if (strFileName.empty())
		{
			string strMsg = "There is no filename available to expand the ";
			strMsg += strSOURCE_DOC_FILENAME_TAG;
			strMsg += " tag!";
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
		// get the filname and ensure it's not empty
		string strPath = getDirectoryFromFullPath(strSourceDocName);
		if (strPath.empty())
		{
			string strMsg = "There is no path available to expand the ";
			strMsg += strSOURCE_DOC_PATH_TAG;
			strMsg += " tag!";
			UCLIDException ue("ELI09821", strMsg);
			ue.addDebugInfo("strSourceDocName", strSourceDocName);
			throw ue;
		}

		// perform the expansion
		replaceVariable(rstrInput, strSOURCE_DOC_PATH_TAG, strPath);
	}
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::expandDocTypeTag(std::string& rstrInput,
								  IAFDocumentPtr& ripDoc)
{
	// if the <DOCTYPE> tag exists in the input string
	// make the corresponding substitution
	if (rstrInput.find(strDOC_TYPE_TAG) != string::npos)
	{
		// get the object tags associated with the document
		IStrToObjectMapPtr ipObjTags(ripDoc->ObjectTags);
		ASSERT_RESOURCE_ALLOCATION("ELI07465", ipObjTags != NULL);

		// check to see if a string tag for the document type
		// exists.  If not, throw an exception
		if (ipObjTags->Contains(DOC_TYPE.c_str()) == VARIANT_FALSE)
		{
			throw UCLIDException("ELI07466", "Document was not successfully classified!");
		}

		// get the vector of document type names
		IVariantVectorPtr ipVecDocTypes = ipObjTags->GetValue(_bstr_t(DOC_TYPE.c_str()));
		ASSERT_RESOURCE_ALLOCATION("ELI07470", ipVecDocTypes != NULL);
		
		// If there's no doc type found or there are more than one 
		// type found, throw an exception
		if (ipVecDocTypes->Size == 0)
		{
			throw UCLIDException("ELI07471", "No document type available!");
		}
		else if (ipVecDocTypes->Size > 1)
		{
			throw UCLIDException("ELI07472", "No unique document type available!");
		}

		// get the string value for the document type tag
		string strDocType = asString(_bstr_t(ipVecDocTypes->GetItem(0)));
		
		// replace instances of the tag with the value
		replaceVariable(rstrInput, strDOC_TYPE_TAG, strDocType);
	}
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::expandComponentDataDirTag(std::string& rstrInput,
										   IAFDocumentPtr& ripDoc)
{
	// if the <ComponentDataDir> tag exists in the input string
	// make the corresponding substitution
	if (rstrInput.find(strCOMPONENT_DATA_DIR_TAG) != string::npos)
	{
		// get the Component Data Folder			
		string strFolder;
		getComponentDataFolder(strFolder);

		// replace instances of the tag with the value
		replaceVariable(rstrInput, strCOMPONENT_DATA_DIR_TAG , strFolder);
	}
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::expandAFDocTags(std::string& rstrInput, IAFDocumentPtr& ripDoc)
{
	IStrToStrMapPtr ipMap = ripDoc->GetStringTags();
	long nNumTags = ipMap->GetSize();
	int i;
	for(i = 0; i < nNumTags; i++)
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
			replaceVariable(rstrInput, strTag , strValue);
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool CAFUtility::getTagValueFromINIFile(string strTagName, std::string& rstrTagValue)
{
	// remove the '<' and '>' from strTagName, and ensure
	long nLen = strTagName.size();
	if (nLen <= 2 || strTagName[0] != '<' ||
		strTagName[nLen - 1] != '>')
	{
		UCLIDException ue("ELI09817", "Invalid tag name!");
		ue.addDebugInfo(strTagName, strTagName);
		throw ue;
	}
	else
	{
		strTagName.erase(0, 1);
		strTagName.erase(nLen - 2, 1);
	}

	// check if we have already cached the value of the tag
	std::map<std::string, std::string>::iterator iter;
	iter = ms_mapINIFileTagNameToValue.find(strTagName);
	if (iter != ms_mapINIFileTagNameToValue.end())
	{
		rstrTagValue = iter->second;
		return true;
	}

	// compute the INI file name if it hasn't been computed already
	static string ls_strINIFileName;
	if (ls_strINIFileName.empty())
	{
		ls_strINIFileName = getModuleDirectory(_Module.m_hInst);
		ls_strINIFileName += "\\UCLIDAFCore.INI";
	}

	// get the value of the tag from the INI file
	// NOTE: This code below is not very robust because it cannot
	// distinguish between a non-existant key in the INI file and
	// an existing key in the INI file with an empty value
	const char *pszSectionName = "ExpandableTags";
	char pszResult[1024];
	if (GetPrivateProfileString(pszSectionName, strTagName.c_str(), "", 
		pszResult, sizeof(pszResult), ls_strINIFileName.c_str()) <= 0)
	{
		return false;
	}
	
	// if the result is not-empty, cache it
	rstrTagValue = pszResult;
	if (!rstrTagValue.empty())
	{
		ms_mapINIFileTagNameToValue[strTagName] = rstrTagValue;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::expandINIFileTags(std::string& rstrInput,
								   IAFDocumentPtr& ripDoc)
{
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
			UCLIDException ue("ELI19374", "Matching pairs of '<' and '>' not found!");
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
			throw UCLIDException("ELI19375", "An empty tag name cannot be expanded!");
		}
	
		
		// retrieve the value of the tag from the INI file
		string strTagValue;
		// if the tag cannot be expanded from the INI FILE
		// we will continue
		if (!getTagValueFromINIFile(strTagName, /* ref */ strTagValue))
		{
			// continue searching
			nStartSearchPos = nTagEndPos + 1;
			continue;
		}

		if (strTagValue.empty())
		{
			UCLIDException ue("ELI09815", "Cannot expand tag to an empty string!");
			ue.addDebugInfo("strTagName", strTagName);
			throw ue;
		}

		// replace the tag value
		rstrInput.erase(nTagStartPos, nTagEndPos - nTagStartPos + 1);
		rstrInput.insert(nTagStartPos, strTagValue);

		// continue searching after the tag value
		nStartSearchPos = nTagStartPos + strTagValue.length() + 1;
		
	}
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::ExpandTags(BSTR strInput, IAFDocument *pDoc, BSTR *pstrOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();

		// get the document as a smart pointer
		IAFDocumentPtr ipDoc(pDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI07464", ipDoc != NULL);

		// Get the string from the input
		string stdstrInput = asString(strInput);

		// Expand the tags
		expandTags(stdstrInput, ipDoc);

		// return the string with the replacements made
		*pstrOutput = _bstr_t(stdstrInput.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07460");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::StringContainsTags(BSTR strInput, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CSingleLock lg( &ms_mutex, TRUE );
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
		vector<string> vecTagNames;
		getTagNames(stdstrInput, vecTagNames);

		// return true as long as there's at least one tag
		*pbValue = vecTagNames.empty() ? VARIANT_FALSE : VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07468");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::StringContainsInvalidTags(BSTR strInput, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();

		// get the input as a STL string
		string stdstrInput = asString(strInput);
		
		// get the tags in the string
		vector<string> vecTagNames;
		getTagNames(stdstrInput, vecTagNames);

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
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();

		IIUnknownVectorPtr ipRetAttributes( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION( "ELI07854", ipRetAttributes != NULL );

		// Get file type
		string strFile = asString( strFileName );
		string strExt = getExtensionFromFullPath( strFile, true );
		if (strExt == ".eav")
		{
			// Pass through to EAV reader
			ipRetAttributes = getThisAsCOMPtr()->GenerateAttributesFromEAVFile(strFileName);
		}
		else if (strExt == ".voa")
		{
			// Load the file into the IUnknownVector object directly
			ipRetAttributes->LoadFrom(strFileName, VARIANT_FALSE);

			// Confirm that each item in vector implements IAttribute
			long lCount = ipRetAttributes->Size();
			for (int i = 0; i < lCount; i++)
			{
				// Retrieve this item
				IUnknownPtr	ipUnknown = ipRetAttributes->At( i );

				// Check for IAttribute support
				IAttributePtr	ipAttr = ipUnknown;
				if (ipAttr == NULL)
				{
					// Throw exception
					UCLIDException ue( "ELI07871", 
						"Object loaded from file is not an attribute!" );
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
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();

		// Get Attribute collection as a smart pointer
		IIUnknownVectorPtr ipAttributes( pvecAttributes );
		ASSERT_RESOURCE_ALLOCATION( "ELI08688", ipAttributes != NULL );
		
		// Get attribute modifier as a smart pointer
		IAttributeModifyingRulePtr ipAM(pAM);
		ASSERT_RESOURCE_ALLOCATION( "ELI08691", ipAM != NULL );

		// Apply Attribute Modifier to each IAttribute
		long lCount = ipAttributes->Size();
		for (int i = 0; i < lCount; i++)
		{
			// Retrieve this Attribute
			IAttributePtr ipAttribute = ipAttributes->At( i );
			ASSERT_RESOURCE_ALLOCATION( "ELI08689", ipAttribute != NULL );

			// "Modify" the Attribute
			ipAM->ModifyValue( ipAttribute, pDoc, NULL );

			// Operate on the SubAttributes
			IIUnknownVectorPtr ipSubAttributes = ipAttribute->SubAttributes;
			if ((bRecursive == VARIANT_TRUE) && (ipSubAttributes != NULL))
			{
				getThisAsCOMPtr()->ApplyAttributeModifier(ipSubAttributes, pDoc, pAM, bRecursive);
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08687");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::ExpandFormatString(IAttribute *pAttribute, BSTR bstrFormat, ISpatialString** pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CSingleLock lg(&ms_mutex, TRUE);
		validateLicense();

		// use a smart pointer for the attribute
		IAttributePtr ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI09669", ipAttribute != NULL);

		// create a copy of the attribute's original value
		ICopyableObjectPtr ipCopy(ipAttribute->Value);
		ASSERT_RESOURCE_ALLOCATION("ELI17021", ipCopy != NULL);
		ISpatialStringPtr ipOutputSS = ipCopy->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI17019", ipOutputSS != NULL);

		// get the expanded string
		ISpatialStringPtr ipExpandedSS = getReformattedName(asString(bstrFormat), pAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI17020", ipExpandedSS != NULL);

		// replace the original string's value with the expanded string's value
		if (ipExpandedSS->HasSpatialInfo() == VARIANT_TRUE)
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

		// return the expanded spatial string
		*pRetVal = ipOutputSS.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19183");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::SortAttributesSpatially(IIUnknownVector* pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();
		
		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI11293", ipAttributes != NULL);

		ISortComparePtr ipCompare(CLSID_SpatiallyCompareAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI11292", ipCompare != NULL);
		
		ipAttributes->Sort(ipCompare);
	}	
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11291");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetBuiltInTags(IVariantVector** ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();

		IVariantVectorPtr ipVec(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI11773", ipVec != NULL);

		ipVec->PushBack(get_bstr_t(strRSD_FILE_DIR_TAG));
		ipVec->PushBack(get_bstr_t(strDOC_TYPE_TAG));
		ipVec->PushBack(get_bstr_t(strCOMPONENT_DATA_DIR_TAG));
		ipVec->PushBack(get_bstr_t(strSOURCE_DOC_NAME_TAG));
		ipVec->PushBack(get_bstr_t(gstrRULE_EXEC_ID_TAG));

		*ppTags = ipVec.Detach();
	}	
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11770");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetINIFileTags(IVariantVector** ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();

		IVariantVectorPtr ipVec(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI11774", ipVec != NULL);

		static string ls_strINIFileName;
		if (ls_strINIFileName.empty())
		{
			ls_strINIFileName = getModuleDirectory(_Module.m_hInst);
			ls_strINIFileName += "\\UCLIDAFCore.INI";
		}

		// get the value of the tag from the INI file
		// NOTE: This code below is not very robust because it cannot
		// distinguish between a non-existant key in the INI file and
		// an existing key in the INI file with an empty value
		const char *pszSectionName = "ExpandableTags";
		char pszResult[2048];

		long nRet = GetPrivateProfileSection(pszSectionName, pszResult, sizeof(pszResult), ls_strINIFileName.c_str());
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
		*ppTags = ipVec.Detach();
	}	
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11771");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetAllTags(IVariantVector** ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();

		UCLID_AFUTILSLib::IAFUtilityPtr ipThis(this);
		ASSERT_RESOURCE_ALLOCATION("ELI11781", ipThis != NULL);

		IVariantVectorPtr ipVec1 = ipThis->GetBuiltInTags();
		IVariantVectorPtr ipVec2 = ipThis->GetINIFileTags();
		ipVec1->Append(ipVec2);

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
		CSingleLock lg(&ms_mutex, TRUE);

		// Check if the registry setting exists
		if (ma_pUserCfgMgr->keyExists(gstrAF_REG_SETTINGS_FOLDER, gstrAF_CACHE_RSD_KEY))
		{
			// Check the registry setting
			string strValue = ma_pUserCfgMgr->getKeyValue(gstrAF_REG_SETTINGS_FOLDER, gstrAF_CACHE_RSD_KEY);
			*pvbCacheRSD = asVariantBool(strValue == "1");
		}
		else
		{
			// Default to enabling RSD caching
			ma_pUserCfgMgr->createKey(gstrAF_REG_SETTINGS_FOLDER, gstrAF_CACHE_RSD_KEY, "1");
			*pvbCacheRSD = VARIANT_TRUE;
		}

		return S_OK;
	}	
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24008");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::ExpandTagsAndFunctions(BSTR bstrInput, IAFDocument *pDoc,
												BSTR *pbstrOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock lg( &ms_mutex, TRUE );
		validateLicense();

		// get the document as a smart pointer
		IAFDocumentPtr ipDoc(pDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI26443", ipDoc != NULL);

		// Get the string from the input
		string strInput = asString(bstrInput);

		// Expand the tags
		expandTags(strInput, ipDoc);

		// Expand the text functions
		TextFunctionExpander tfe;
		strInput = tfe.expandFunctions(strInput);

		// return the string with the replacements made
		*pbstrOutput = _bstr_t(strInput.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26166");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// private / helper methods
//-------------------------------------------------------------------------------------------------
UCLID_AFUTILSLib::IAFUtilityPtr CAFUtility::getThisAsCOMPtr()
{
	UCLID_AFUTILSLib::IAFUtilityPtr ipThis = this;
	ASSERT_RESOURCE_ALLOCATION("ELI16965", ipThis != NULL);
	return ipThis;
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::getTagNames(const string& strInput, 
							 vector<string>& rvecTagNames) const
{
	// clear the result vector
	rvecTagNames.clear();
	
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
			UCLIDException ue("ELI19184", "Matching pairs of '<' and '>' not found!");
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
			throw UCLIDException("ELI19185", "An empty tag name cannot be expanded!");
		}

		// continue searching at the next position
		rvecTagNames.push_back(strTagName);
		nSearchStartPos = nTagEndPos + 1;
	}
}
//-------------------------------------------------------------------------------------------------
string CAFUtility::getRulesFilePrefix()
{
	// Check for key existence
	if (!ma_pUserCfgMgr->keyExists( gstrAF_REG_SETTINGS_FOLDER, DOCTYPE_PREFIX ))
	{
		string strPrefix("");
		ma_pUserCfgMgr->createKey( gstrAF_REG_SETTINGS_FOLDER, DOCTYPE_PREFIX, strPrefix );

		return strPrefix;
	}

	return ma_pUserCfgMgr->getKeyValue(gstrAF_REG_SETTINGS_FOLDER, DOCTYPE_PREFIX);
}
//-------------------------------------------------------------------------------------------------
bool CAFUtility::isAutoEncryptOn()
{
	// Check for key existence
	if (!ma_pUserCfgMgr->keyExists( gstrAF_REG_SETTINGS_FOLDER, gstrAF_AUTO_ENCRYPT_KEY ))
	{
		// Default setting is OFF
		string strAutoEncrypt( "0" );
		ma_pUserCfgMgr->createKey( gstrAF_REG_SETTINGS_FOLDER, gstrAF_AUTO_ENCRYPT_KEY, 
			strAutoEncrypt );

		return false;
	}

	return ma_pUserCfgMgr->getKeyValue(gstrAF_REG_SETTINGS_FOLDER, 
		gstrAF_AUTO_ENCRYPT_KEY) == "1";
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI06978", "AttributeFinder Utility");
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::getComponentDataFolder(string& rFolder)
{
	CSingleLock lg( &ms_mutex, TRUE );

	// this method gets called quite often - so cache the results
	// NOTE: This means that the component data folder cannot be effectively changed while
	// FlexIndex components are running
	static string ls_strComponentDataFolder;
	if (ls_strComponentDataFolder.empty())
	{
		// create an instance of the attribute finder engine
		IAttributeFinderEnginePtr ipEngine(CLSID_AttributeFinderEngine);
		ASSERT_RESOURCE_ALLOCATION("ELI13444", ipEngine != NULL);
	
		ls_strComponentDataFolder = ipEngine->GetComponentDataFolder();
	}

	rFolder = ls_strComponentDataFolder;
}
//-------------------------------------------------------------------------------------------------
unsigned int CAFUtility::getAttributeLevel(string strName)
{
	unsigned int ui;
	for (ui = 0; ui < strName.length(); ui++)
	{
		if (strName.at(ui) != '.')
		{
			break;
		}
	}
	return ui;
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::removeDots(string& strName)
{
	while(strName.length() > 0)
	{
		if (strName.at(0) == '.')
		{
			strName.erase(strName.begin());
		}
		else
		{
			break;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::loadAttributesFromEavFile(IIUnknownVectorPtr ipAttributes, 
										   unsigned long ulCurrLevel, 
										   unsigned int& uiCurrLine, vector<string> vecLines)
{
	IAttributePtr ipNewAttr = NULL;
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
			ASSERT_RESOURCE_ALLOCATION("ELI09538", ipNewAttr != NULL);
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
			ASSERT_RESOURCE_ALLOCATION("ELI25947", ipValue != NULL);
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
ISpatialStringPtr CAFUtility::getReformattedName(const std::string& strFormat, IAttributePtr ipAttribute)
{
	ISpatialStringPtr ipNewName(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI19187", ipNewName != NULL);

	unsigned int ui;
	for (ui = 0; ui < strFormat.length(); ui++)
	{
		char c = strFormat.at(ui);
		if (c == '<')
		{
			// a new scope is being opened
			// to process it we will get the entire scope as a substring 
			// and recurse adding the sub string wherever it is supposed to go
			unsigned long ulClosePos = getCloseScopePos(strFormat, ui, '<', '>');

			if (ulClosePos == string::npos)
			{
				UCLIDException ue("ELI19188", "Unmatched \'<\' in name formatting pattern.");
				throw ue;
			}

			// ulClosePos is the index that immediately follows the closing > of
			// the scope and we want the string between < and > exclusive
			unsigned long ulStart = ui+1;
			unsigned long ulLength = (ulClosePos - 1) - ulStart;
			string strNewFormat = strFormat.substr( ui+1, ulLength );
			ISpatialStringPtr ipScopeStr = getReformattedName(strNewFormat, ipAttribute);

			if (ipScopeStr != NULL)
			{
				ipNewName->Append(ipScopeStr);
			}
			// -1 because i will auto increment (for loop) 
			ui = ulClosePos - 1;
		}
		else if (c == '%')
		{
			// % must be followed by a valid identifier that is the name of an attribute
			// it must be either %First, %Last, %Middle... 
			// there can optionally be a number between % and the identifier which is the 
			// number of characters to use
			long nNumChars = -1;

			unsigned long ulIdentStartPos = ui + 1;
			unsigned long ulIdentEndPos = strFormat.find("%", ulIdentStartPos);
			unsigned long ulLength = ulIdentEndPos - ulIdentStartPos;

			// if no terminating "%" is found
			if ((ulIdentEndPos == string::npos) || (ulIdentStartPos == ulIdentEndPos))
			{
				UCLIDException ue("ELI09765", "Invalid Variable in Format String.");
				ue.addDebugInfo("Variable", strFormat.substr(ui, ulLength));
				throw ue;
			}

			// if character after starting "%" is digit extract the number from the identifier
			if (isDigitChar(strFormat[ulIdentStartPos]))
			{
				unsigned long ulNumberEndPos = strFormat.find_first_not_of("0123456789", ulIdentStartPos);

				// confirm that the string contains something other than digits as an identifier name
				if (ulNumberEndPos != ulIdentEndPos)
				{
					string strNum = strFormat.substr( ulIdentStartPos, ulNumberEndPos - ulIdentStartPos );
					nNumChars = asLong( strNum.c_str() );
					ulLength = ulLength - (ulNumberEndPos - ulIdentStartPos);
					ulIdentStartPos = ulNumberEndPos;
				}
				else
				{
					UCLIDException ue(UCLIDException("ELI12972", "Invalid Variable - Variable identifier cannot be only digits!"));
					ue.addDebugInfo("Variable", strFormat.substr(ui, ulLength));
					throw ue;
				}
			}
			
			string strIdent = strFormat.substr( ulIdentStartPos, ulLength );
			ISpatialStringPtr ipText = getVariableValue(strIdent, ipAttribute);

			if (ipText == NULL)
			{
				// If the %var has no value we can either ignore it and continue
				// or invalidate the entire scope and return an empty string
				ISpatialStringPtr ipEmpty( CLSID_SpatialString );
				ASSERT_RESOURCE_ALLOCATION("ELI17546", ipEmpty != NULL);
				return ipEmpty;
			}
			else
			{
				if (nNumChars > 0)
				{
					long nLength = ipText->Size;

					if (nNumChars < nLength)
					{
						ipText = ipText->GetSubString( 0, nNumChars - 1 );
					}
				}
				ipNewName->Append(ipText);
			}
			// don't forget to jump ahead in the string
			ui = ulIdentEndPos;
		}
		else
		{
			// Add this character to the string
			string str;
			str += c;
			ipNewName->AppendString(_bstr_t(str.c_str()));
		}
	}
	return ipNewName;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CAFUtility::getVariableValue(const std::string& strVariable, IAttributePtr ipAttribute)
{
	// TODO: check the cache
	bool bFound = false;
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(strVariable, ".", vecTokens);
	// There should only be a max of 2 tokens 
	// Token 1 should be an xpath query
	// Token 2 should be either "Value" or "Name"
	// If bGetType is true we will return the type of the attribute
	// otherwise we will return the value
	bool bGetType = false;
	if(vecTokens.size() > 2)
	{
		UCLIDException ue("ELI09677", "Invalid Variable Query.");
		ue.addDebugInfo("Invalid Variable", strVariable);
		throw ue;
	}
	string strQuery = vecTokens[0];
	if(vecTokens.size() == 2)
	{
		if(vecTokens[1] == "Type")
		{
			bGetType = true;
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
	ISpatialStringPtr ipNewString = NULL;
	if(strQuery == "Type")
	{
		// This means that the variable is %Type which is just the Type of the
		// Selected Attribute
		ipNewString.CreateInstance(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI09707", ipNewString != NULL);
		string str = asString(ipAttribute->Type);
		if(str == "")
		{
			return NULL;
		}
		ipNewString->CreateNonSpatialString(str.c_str(), "");
	}
	else if(strQuery == "Value")
	{
		// This means that the variable is %Value which is just the Value of the
		// Selected Attribute
		ipNewString = ipAttribute->Value;
	}
	else
	{
		UCLID_AFUTILSLib::IAFUtilityPtr ipThis(this);
		ASSERT_RESOURCE_ALLOCATION("ELI09705", ipThis != NULL);

		IIUnknownVectorPtr ipSubAttributes = ipThis->QueryAttributes(ipAttribute->SubAttributes, 
			_bstr_t(strQuery.c_str()), VARIANT_FALSE);
		ASSERT_RESOURCE_ALLOCATION("ELI25948", ipSubAttributes != NULL);

		// Now we will just arbitrarily choose the first match
		if(ipSubAttributes->Size() <= 0)
		{
			return NULL;
		}
		
		IAttributePtr ipFoundAttr = ipSubAttributes->At(0);
		ASSERT_RESOURCE_ALLOCATION("ELI09706", ipFoundAttr != NULL);
		
		if(bGetType)
		{
			ipNewString.CreateInstance(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI19189", ipNewString != NULL);
			ipNewString->CreateNonSpatialString(ipFoundAttr->Type, "");
		}
		else
		{
			ipNewString = ipFoundAttr->Value;
		}
	}
	return ipNewString;
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::expandTags(string& rstrInput, IAFDocumentPtr ipDoc)
{
	try
	{
		ASSERT_ARGUMENT("ELI26167", ipDoc != NULL);

		// expand the INI file tags first, because the INI file tag
		// may use one or more of the other tags
		expandINIFileTags(rstrInput, ipDoc);

		// expand the various other tags
		expandRSDFileDirTag(rstrInput, ipDoc);
		expandRuleExecIDTag(rstrInput, ipDoc);
		expandSourceDocNameTag(rstrInput, ipDoc);
		expandDocTypeTag(rstrInput, ipDoc);
		expandComponentDataDirTag(rstrInput, ipDoc);
		expandAFDocTags(rstrInput, ipDoc);

		// at this time, ensure that there are no more tags left
		vector<string> vecTagNames;
		getTagNames(rstrInput, vecTagNames);
		if (!vecTagNames.empty())
		{
			UCLIDException ue("ELI09818", "One or more tag names could not be expanded!");
			ue.addDebugInfo("rstrInput", rstrInput);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26168");
}
//-------------------------------------------------------------------------------------------------
