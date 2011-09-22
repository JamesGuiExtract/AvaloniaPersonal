// AttributeFinderEngine.cpp : Implementation of CAttributeFinderEngine
#include "stdafx.h"
#include "AFCore.h"
#include "AttributeFinderEngine.h"
#include "Common.h"
#include "AFInternalUtils.h"
#include "AFAboutDlg.h"
#include "RuleSetProfiler.h"

#include <SpecialStringDefinitions.h>
#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <RegistryPersistenceMgr.h>
#include <ComUtils.h>
#include <Misc.h>
#include <ComponentLicenseIDs.h>
#include <FileIterator.h>
#include <StringTokenizer.h>

using namespace std;

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const string COMPONENT_DATA_FOLDER_KEY = "ComponentDataFolder";
static const char* LATEST_FKB_FLAG = "Latest";

//-------------------------------------------------------------------------------------------------
// Statics
//-------------------------------------------------------------------------------------------------
unique_ptr<IConfigurationSettingsPersistenceMgr> CAttributeFinderEngine::mu_spUserCfgMgr(
		new RegistryPersistenceMgr(HKEY_CURRENT_USER, gstrAF_REG_ROOT_FOLDER_PATH));
string CAttributeFinderEngine::ms_strLegacyFKBVersion = "_uninitialized_";
CMutex CAttributeFinderEngine::m_mutex;

//-------------------------------------------------------------------------------------------------
// Global function
//-------------------------------------------------------------------------------------------------
bool isOptionalProgressDlgLicensed()
{
	static const unsigned long OPTIONAL_PROGRESS_ID = 167;

	static CMutex sMutex;
	CSingleLock lg(&sMutex, TRUE);

	static bool bLicensed = false;

	if (bLicensed)
	{
		return true;
	}

	bLicensed = LicenseManagement::isLicensed(OPTIONAL_PROGRESS_ID);

	return bLicensed;
}

//-------------------------------------------------------------------------------------------------
// CAttributeFinderEngine
//-------------------------------------------------------------------------------------------------
CAttributeFinderEngine::CAttributeFinderEngine()
: m_ipOCREngine(__nullptr),
  m_ipOCRUtils(__nullptr)
{
	try
	{
		// create instance of the persistence mgr
		mu_pUserCfgMgr.reset(new RegistryPersistenceMgr(HKEY_CURRENT_USER,
			gstrAF_REG_ROOT_FOLDER_PATH ));
		ASSERT_RESOURCE_ALLOCATION("ELI07337", mu_pUserCfgMgr.get()!= __nullptr);

		// Check the profiling setting, and apply it to CRuleSetProfiler.
		CRuleSetProfiler::ms_bEnabled = isProfilingEnabled();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI13441")
}
//-------------------------------------------------------------------------------------------------
CAttributeFinderEngine::~CAttributeFinderEngine()
{
	try
	{
		// Release COM objects
		m_ipOCREngine = __nullptr;
		m_ipOCRUtils = __nullptr;
		mu_pUserCfgMgr.reset();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16300");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFinderEngine::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeFinderEngine,
		&IID_ILicensedComponent
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
// IAttributeFinderEngine
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFinderEngine::FindAttributes(IAFDocument *pDoc,
													BSTR strSrcDocFileName,
													long nNumOfPagesToRecognize,
													VARIANT varRuleSet,
													IVariantVector *pvecAttributeNames,
													VARIANT_BOOL vbUseAFDocText,
													IProgressStatus *pProgressStatus,
													IIUnknownVector** ppAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride rcOverride(_Module.m_hInstResource);

	INIT_EXCEPTION_AND_TRACING("MLI00023");

	try
	{
		// Check licensing
		validateLicense();
		_lastCodePos = "1";

		ASSERT_ARGUMENT("ELI28077", ppAttributes != __nullptr);

		// Convert the source document name from a BSTR to a STL string
		string strInputFile = asString(strSrcDocFileName);

		// get the document object
		UCLID_AFCORELib::IAFDocumentPtr ipAFDoc(pDoc);
		ASSERT_ARGUMENT("ELI09173", ipAFDoc != __nullptr);

		_lastCodePos = "2";

		// Determine file type
		EFileType eFileType = getFileType(strInputFile);

		// Check source document name (only if vbUseAFDocText == FALSE)
		bool bLoadFromFile = false;
		bool bNeedToOCR = false;
		if (vbUseAFDocText == VARIANT_FALSE && !strInputFile.empty())
		{
			// verify validity of input file
			validateFileOrFolderExistence(strInputFile);
			_lastCodePos = "3";	

			if (eFileType == kUSSFile)
			{
				// Load uss file.
				bLoadFromFile = true;
			}
			else
			{
				// If an image OCR... if text bNeedToOCR will cause the text to get loaded as
				// "indexed" text.
				bNeedToOCR = true;
			}

			_lastCodePos = "5";
		}

		// Wrap the progress status object in a smart pointer
		IProgressStatusPtr ipProgressStatus(pProgressStatus);
		_lastCodePos = "6";

		// Based upon whether OCR will need to be run, determine the total number
		// of items to use for progress status updates
		// NOTE: The constants below have been configured such that if a USS/TXT file needs to be loaded and then the
		// rules need to be executed, the loading represents 10% of the work and rule running represents 90% of the work.
		// If OCR needs to be done first, and then the rules need to be executed, the OCR and rule execution each 
		// represent 50% of the work.
		const long nNUM_PROGRESS_ITEMS_LOAD_USS_OR_TXT_FILE = 1;
		const long nNUM_PROGRESS_ITEMS_OCR = 9;
		const long nNUM_PROGRESS_ITEMS_EXECUTE_RULES = 9;
		long nTOTAL_PROGRESS_ITEMS = nNUM_PROGRESS_ITEMS_EXECUTE_RULES; // the rules always need to be executed
		nTOTAL_PROGRESS_ITEMS += bLoadFromFile ? nNUM_PROGRESS_ITEMS_LOAD_USS_OR_TXT_FILE : 0;
		nTOTAL_PROGRESS_ITEMS += bNeedToOCR ? nNUM_PROGRESS_ITEMS_OCR : 0;
		_lastCodePos = "7";

		// Initialize the progress status
		if (ipProgressStatus)
		{
			_lastCodePos = "8";
			ipProgressStatus->InitProgressStatus("Initializing rules execution...", 0, 
				nTOTAL_PROGRESS_ITEMS, VARIANT_TRUE);
		}
		_lastCodePos = "9";

		// Load the source document from the file if appropriate
		if (bLoadFromFile)
		{
			_lastCodePos = "10";

			// Update the progress status
			if (ipProgressStatus)
			{
				ipProgressStatus->StartNextItemGroup("Loading source document...", nNUM_PROGRESS_ITEMS_LOAD_USS_OR_TXT_FILE);
			}
			_lastCodePos = "11";
	
			// Get the spatial string from the document and load it from the file
			ISpatialStringPtr ipInputText = ipAFDoc->Text;
			ASSERT_RESOURCE_ALLOCATION("ELI07829", ipInputText != __nullptr);

			// Load the file
			ipInputText->LoadFrom(strSrcDocFileName, VARIANT_FALSE);
		}
		_lastCodePos = "12";

		// OCR the source document if appropriate
		if (bNeedToOCR)
		{
			_lastCodePos = "13";

			// Update the progress status
			if (ipProgressStatus)
			{
				ipProgressStatus->StartNextItemGroup("Performing OCR...", nNUM_PROGRESS_ITEMS_OCR);
			}
			_lastCodePos = "14";
			
			// If this is a text file, load the file as "indexed" text.
			if (eFileType == kTXTFile || eFileType == kXMLFile)
			{
				ISpatialStringPtr ipText = ipAFDoc->Text;
				ASSERT_RESOURCE_ALLOCATION("ELI31687", ipText != __nullptr);

				// Load the spatial string from the file
				ipText->LoadFrom(strInputFile.c_str(), VARIANT_FALSE);
			}
			// Assume this is an image file and attempt OCR [P16 #2813]
			else
			{
				// Retrieve text from all pages of image, retaining spatial information
				// pass in the SubProgressStatus, or __nullptr if ipProgressStatus is __nullptr
				ipAFDoc->Text = getOCRUtils()->RecognizeTextInImageFile(strSrcDocFileName,
					nNumOfPagesToRecognize, getOCREngine(),
					ipProgressStatus ? ipProgressStatus->SubProgressStatus : __nullptr);
			}
		}
		_lastCodePos = "15";

		// Create RuleSet object
		UCLID_AFCORELib::IRuleSetPtr ipRuleSet;
		_variant_t _varRuleSet(varRuleSet);
		_bstr_t _bstrRSDFileName = "";
		if (_varRuleSet.vt == VT_BSTR)
		{
			_lastCodePos = "16";

			// create new RuleSet object
			ipRuleSet.CreateInstance( CLSID_RuleSet );
			ASSERT_RESOURCE_ALLOCATION( "ELI07827", ipRuleSet != __nullptr );

			// Load Rule Set from specified file
			_bstrRSDFileName = _varRuleSet;
			ipRuleSet->LoadFrom(_bstrRSDFileName, VARIANT_FALSE);
		}
		else if (_varRuleSet.vt == VT_UNKNOWN || _varRuleSet.vt == VT_DISPATCH)
		{
			_lastCodePos = "17";

			// Use the RuleSet that was passed in
			IUnknownPtr ipUnknown = _varRuleSet;
			ASSERT_RESOURCE_ALLOCATION( "ELI07912", ipUnknown != __nullptr );
			
			ipRuleSet = ipUnknown;
			ASSERT_RESOURCE_ALLOCATION( "ELI07910", ipRuleSet != __nullptr );
		}
		_lastCodePos = "18";

		// Update progress status to indicate that we are next going to execute rules
		if (ipProgressStatus)
		{
			_lastCodePos = "25";
			ipProgressStatus->StartNextItemGroup("Executing rules...", nNUM_PROGRESS_ITEMS_EXECUTE_RULES);
		}
		_lastCodePos = "26";

		// Create a pointer to the Sub-ProgressStatus object, depending upon whether
		// the caller requested progress information
		IProgressStatusPtr ipSubProgressStatus = (ipProgressStatus == __nullptr) ? 
			__nullptr : ipProgressStatus->SubProgressStatus;
		_lastCodePos = "27";

		// Find the Attributes (wrap the attribute names vector in smart pointer)
		IVariantVectorPtr ipvecAttributeNames(pvecAttributeNames);
		IIUnknownVectorPtr ipAttributes =  findAttributesInText(ipAFDoc, ipRuleSet,
			ipvecAttributeNames, ipSubProgressStatus);
		ASSERT_RESOURCE_ALLOCATION("ELI28079", ipAttributes != __nullptr);

		// Set the return value for the attributes
		*ppAttributes = ipAttributes.Detach();
		_lastCodePos = "30";

		// Update progress status to indicate that we are done executing rules
		if (ipProgressStatus)
		{
			_lastCodePos = "31";
			ipProgressStatus->CompleteCurrentItemGroup();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07820");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFinderEngine::ShowHelpAboutBox(EHelpAboutType eType, BSTR strProductVersion)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		// Check licensing
		validateLicense();

		// Display the About box with version information
		std::string strProduct = asString( strProductVersion );
		CAFAboutDlg dlgAbout( eType, strProduct );
		dlgAbout.DoModal();

		return S_OK;
	}	
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11641");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFinderEngine::GetComponentDataFolder(BSTR *pstrComponentDataFolder)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// get the component data folder and return it
		string strFolder;
		getComponentDataFolder(strFolder);

		*pstrComponentDataFolder = _bstr_t(strFolder.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13440");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFinderEngine::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == __nullptr)
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
// Public functions
//-------------------------------------------------------------------------------------------------
string CAttributeFinderEngine::getLegacyFKBVersion()
{
	if (ms_strLegacyFKBVersion == "_uninitialized_")
	{
		// To avoid the need to recalculate the legacy version multiple times, this is a static
		// method that requires locking.
		CSingleLock lg(&m_mutex, TRUE);

		if (ms_strLegacyFKBVersion == "_uninitialized_")
		{
			// get the component data folder
			string strComponentDataFolder;
			bool bOverriden;
			getRootComponentDataFolder(strComponentDataFolder, bOverriden);
			string strFKBVersionFile = strComponentDataFolder + string("\\FKBVersion.txt");

			// open the FKB version file
			ifstream infile(strFKBVersionFile.c_str());
			if (infile.good())
			{
				// read the version line
				string strVersionLine;
				getline(infile, strVersionLine);

				// ensure that the version information is in the expected format
				if (strVersionLine.find("FKB Ver.") == 0)
				{
					ms_strLegacyFKBVersion = trim(strVersionLine.substr(9), " \t", " \t");
				}
				else
				{
					// version file was found, but content is not as excepted
					UCLIDException ue("ELI32472", "Unexpected FKB version information in FKB version file!");
					ue.addDebugInfo("VersionLine", strVersionLine);
					ue.log();

					ms_strLegacyFKBVersion = "[Unexpected FKB version]";
				}
			}
			else
			{
				ms_strLegacyFKBVersion = "";
			}
		}
	}

	return ms_strLegacyFKBVersion;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CAttributeFinderEngine::findAttributesInText(
	const UCLID_AFCORELib::IAFDocumentPtr& ipAFDoc, const UCLID_AFCORELib::IRuleSetPtr& ipRuleSet,
	const IVariantVectorPtr& ipvecAttributeNames, const IProgressStatusPtr& ipProgressStatus)
{
	try
	{
		IIUnknownVectorPtr ipAttributes = __nullptr;
		if (ipRuleSet != __nullptr)
		{		
			// find all attributes' values through current rule set
			ipAttributes = ipRuleSet->ExecuteRulesOnText(ipAFDoc, 
				ipvecAttributeNames, ipProgressStatus);
		}
		else
		{
			// Just create an empty vector
			ipAttributes.CreateInstance(CLSID_IUnknownVector);
		}
		ASSERT_RESOURCE_ALLOCATION("ELI28081", ipAttributes != __nullptr);

		return ipAttributes;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28082");
}
//-------------------------------------------------------------------------------------------------
IOCREnginePtr CAttributeFinderEngine::getOCREngine()
{
	if (m_ipOCREngine == __nullptr)
	{
		m_ipOCREngine.CreateInstance( CLSID_ScansoftOCR );
		ASSERT_RESOURCE_ALLOCATION( "ELI07830", m_ipOCREngine != __nullptr );
		
		_bstr_t _bstrPrivateLicenseCode = get_bstr_t(LICENSE_MGMT_PASSWORD);
		IPrivateLicensedComponentPtr ipScansoftEngine(m_ipOCREngine);
		ipScansoftEngine->InitPrivateLicense( _bstrPrivateLicenseCode );
	}

	return m_ipOCREngine;
}
//-------------------------------------------------------------------------------------------------
IOCRUtilsPtr CAttributeFinderEngine::getOCRUtils()
{
	if (m_ipOCRUtils == __nullptr)
	{
		m_ipOCRUtils.CreateInstance(CLSID_OCRUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI07831", m_ipOCRUtils != __nullptr);
	}

	return m_ipOCRUtils;
}
//-------------------------------------------------------------------------------------------------
void CAttributeFinderEngine::getRootComponentDataFolder(string& rstrFolder, bool& rbOverridden)
{
	// Lock to protect access to a static registry manager instance for this method.
	CSingleLock lg(&m_mutex, TRUE);

	// No matter whether we are in debug or release mode, if the component data folder has been
	// defined in the registry we should use that folder
	if (!mu_spUserCfgMgr->keyExists( gstrAF_REG_SETTINGS_FOLDER, COMPONENT_DATA_FOLDER_KEY ))
	{
		// Default value for this key is an empty string
		mu_spUserCfgMgr->createKey( gstrAF_REG_SETTINGS_FOLDER, COMPONENT_DATA_FOLDER_KEY, "");
	};

	// Get the key value
	rstrFolder = mu_spUserCfgMgr->getKeyValue(gstrAF_REG_SETTINGS_FOLDER, COMPONENT_DATA_FOLDER_KEY, "");
	
	// If the key value is not empty, then it shall be assumed it's the component data folder that
	// we should use.
	if (rstrFolder.empty())
	{
		rbOverridden = false;
	}
	else
	{
		// For consistency, ensure the path returned does not end with a slash.
		if (rstrFolder[rstrFolder.length() - 1] == '\\')
		{
			rstrFolder = rstrFolder.substr(0, rstrFolder.length() - 1);
		}

		rbOverridden = true;
		return;
	}

	// The registry key was not defined - so, use the default definition of the component data folder.
	const string COMPONENT_DATA = "ComponentData";

	string strThisModulePath = ::getModuleDirectory(_Module.m_hInst);

	// Go up one level 
	int nLastSlash = strThisModulePath.find_last_of("\\");
	rstrFolder = strThisModulePath.substr(0, nLastSlash+1);
	
	// Append the FlexIndexComponents and ComponentData folder names (FlexIDSCore #3198)
	rstrFolder += "FlexIndexComponents\\" + COMPONENT_DATA;

	if (!isValidFolder(rstrFolder))
	{
		UCLIDException ue("ELI32473", "ComponentData folder doesn't exist.");
		ue.addDebugInfo("ComponentData Folder", rstrFolder);
		ue.addWin32ErrorInfo();
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
// Converts a 4 part version string into a ULONGLONG. Helper function for getComponentDataFolder
ULONGLONG getVersionAsULONGLONG(string strVersion)
{
	ULONGLONG nVersion = 0;

	try
	{
		vector<string> vecVersionParts;
		StringTokenizer	st('.');
		st.parse(strVersion, vecVersionParts);
		for (int i = 0; i < 4; i++)
		{
			ULONGLONG nVersionPart = asLong(vecVersionParts[i]);
			nVersion  |= (nVersionPart << (48 - (i * 16)));
		}
	}
	catch (...)
	{
		// Ignore any errors parsing the version numbers and return 0 to indicate it was not parsed.
		return 0;
	}

	return nVersion;
}
//-------------------------------------------------------------------------------------------------
void CAttributeFinderEngine::getComponentDataFolder(string& rstrFolder)
{
	bool bOverriden = false;
	getRootComponentDataFolder(rstrFolder, bOverriden);

	// It the path has been overridden, don't take the FKB version into account; just return.
	if (bOverriden)
	{
		return;
	}

	// Use the RuleExecutionEnv to find any FKB version that should be used to resolve the path.
	if (m_ipRuleExecutionEnv == __nullptr)
	{
		m_ipRuleExecutionEnv.CreateInstance(CLSID_RuleExecutionEnv);
		ASSERT_RESOURCE_ALLOCATION("ELI32474", m_ipRuleExecutionEnv != __nullptr);
	}

	string strFKBVersion = asString(m_ipRuleExecutionEnv->FKBVersion);
	string strLegacyFKBVersion = getLegacyFKBVersion();

	// If no FKB version has been assigned for this thread or the "Latest" keyword is specified,
	// use highest installed version number.
	if (strFKBVersion.empty() || _stricmp(strFKBVersion.c_str(), LATEST_FKB_FLAG) == 0)
	{
		ULONGLONG nHighestVersion = 0;

		// Iterate through all component data sub-directories matching the version number pattern.
		vector<string> vecDirectories;
		string strRootFolder = rstrFolder + "\\";
		FileIterator iter(strRootFolder + "*");
		while (iter.moveNext())
		{
			// Only look at directories matching the version string pattern.
			if (!iter.isDirectory())
			{
				continue;
			}

			string strFolder = iter.getFileName();

			if (count(strFolder.begin(), strFolder.end(), '.') == 3)
			{
				ULONGLONG dwVersion = getVersionAsULONGLONG(iter.getFileName());

				if (dwVersion > nHighestVersion)
				{
					nHighestVersion = dwVersion;
					rstrFolder = strRootFolder + strFolder;
				}
			}
		}
		
		// If there were no FKB folders found, use the legacy version (if available).
		if (nHighestVersion == 0 && !strLegacyFKBVersion.empty())
		{
			rstrFolder = strRootFolder + strLegacyFKBVersion;
		}
	}
	// If the assigned FKB version matches an installed legacy FKB version installed to the root of
	// the component data folder, use the legacy FKB version for backward compatibility.
	else if (strFKBVersion == strLegacyFKBVersion)
	{
		m_ipRuleExecutionEnv->FKBVersion = strLegacyFKBVersion.c_str();
	}
	// Otherwise, use the version number to find the version specific component data path
	else
	{
		rstrFolder += "\\" + strFKBVersion;
	}
	
	if (!isValidFolder(rstrFolder))
	{
		UCLIDException ue("ELI32475", "Version specific ComponentData folder doesn't exist.");
		ue.addDebugInfo("ComponentData Folder", rstrFolder);
		ue.addDebugInfo("FKB Version", strFKBVersion);
		ue.addWin32ErrorInfo();
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
bool CAttributeFinderEngine::isProfilingEnabled()
{
	if (!LicenseManagement::isLicensed(gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS))
	{
		return false;
	}

	if (!mu_spUserCfgMgr->keyExists(gstrAF_REG_SETTINGS_FOLDER, gstrAF_PROFILE_RULES_KEY))
	{
		mu_spUserCfgMgr->createKey(gstrAF_REG_SETTINGS_FOLDER, gstrAF_PROFILE_RULES_KEY,
			gstrAF_DEFAULT_PROFILE_RULES);
		return (gstrAF_DEFAULT_PROFILE_RULES == "1");
	}

	string strValue = mu_spUserCfgMgr->getKeyValue(gstrAF_REG_SETTINGS_FOLDER, gstrAF_PROFILE_RULES_KEY,
		gstrAF_DEFAULT_PROFILE_RULES);

	return (strValue == "1");
}
//-------------------------------------------------------------------------------------------------
void CAttributeFinderEngine::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04904", "Attribute Finder Engine" );
}
//-------------------------------------------------------------------------------------------------
