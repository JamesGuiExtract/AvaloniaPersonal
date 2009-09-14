// AttributeFinderEngine.cpp : Implementation of CAttributeFinderEngine
#include "stdafx.h"
#include "AFCore.h"
#include "AttributeFinderEngine.h"
#include "Common.h"
#include "AFInternalUtils.h"
#include "AFAboutDlg.h"

#include <SpecialStringDefinitions.h>
#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <RegistryPersistenceMgr.h>
#include <ComUtils.h>
#include <Misc.h>
#include <ComponentLicenseIDs.h>

using namespace std;

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const string COMPONENT_DATA_FOLDER_KEY = "ComponentDataFolder";

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

	bLicensed = LicenseManagement::sGetInstance().isLicensed(OPTIONAL_PROGRESS_ID);

	return bLicensed;
}

//-------------------------------------------------------------------------------------------------
// CAttributeFinderEngine
//-------------------------------------------------------------------------------------------------
CAttributeFinderEngine::CAttributeFinderEngine()
: m_ipOCREngine(NULL),
  m_ipOCRUtils(NULL)
{
	try
	{
		// create instance of the persistence mgr
		ma_pUserCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr( HKEY_CURRENT_USER, gstrAF_REG_ROOT_FOLDER_PATH ));
		ASSERT_RESOURCE_ALLOCATION("ELI07337", ma_pUserCfgMgr.get()!= NULL);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI13441")
}
//-------------------------------------------------------------------------------------------------
CAttributeFinderEngine::~CAttributeFinderEngine()
{
	try
	{
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
													IProgressStatus *pProgressStatus,
													IIUnknownVector* *pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride rcOverride(_Module.m_hInstResource);

	INIT_EXCEPTION_AND_TRACING("MLI00023");

	try
	{
		// Check licensing
		validateLicense();
		_lastCodePos = "1";

		// Convert the source document name from a BSTR to a STL string
		string strInputFile = asString(strSrcDocFileName);

		// get the document object
		UCLID_AFCORELib::IAFDocumentPtr ipAFDoc(pDoc);
		ASSERT_ARGUMENT("ELI09173", ipAFDoc != NULL);

		// get the text associated with the document
		ISpatialStringPtr ipInputText = ipAFDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI07829", ipInputText != NULL);
		_lastCodePos = "2";

		// Check source document name
		bool bLoadFromFile = false;
		bool bNeedToOCR = false;
		if (!strInputFile.empty())
		{
			// verify validity of input file
			validateFileOrFolderExistence(strInputFile);
			_lastCodePos = "3";
			
			// Determine file type
			EFileType eFileType = getFileType(strInputFile);
			_lastCodePos = "4";

			// Since the Source document is given
			switch (eFileType)
			{
			// Read text from file into Spatial String
			case kTXTFile:
			case kUSSFile:
				bLoadFromFile = true;
				break;

			// assume this is an image file and attempt OCR [P16 #2813]
			default:
				bNeedToOCR = true;
				break;
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

			// Retrieve text from all pages of image, retaining spatial information
			// pass in the SubProgressStatus, or NULL if ipProgressStatus is NULL
			ipAFDoc->Text = getOCRUtils()->RecognizeTextInImageFile(strSrcDocFileName, nNumOfPagesToRecognize, 
				getOCREngine(), ipProgressStatus ? ipProgressStatus->SubProgressStatus : NULL);
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
			ASSERT_RESOURCE_ALLOCATION( "ELI07827", ipRuleSet != NULL );

			// Load Rule Set from specified file
			_bstrRSDFileName = _varRuleSet;
			ipRuleSet->LoadFrom(_bstrRSDFileName, VARIANT_FALSE);
		}
		else if (_varRuleSet.vt == VT_UNKNOWN || _varRuleSet.vt == VT_DISPATCH)
		{
			_lastCodePos = "17";

			// Use the RuleSet that was passed in
			IUnknownPtr ipUnknown = _varRuleSet;
			ASSERT_RESOURCE_ALLOCATION( "ELI07912", ipUnknown != NULL );
			
			ipRuleSet = ipUnknown;
			ASSERT_RESOURCE_ALLOCATION( "ELI07910", ipRuleSet != NULL );
		}
		_lastCodePos = "18";

		/////////////////////////////////////
		// Get Rule Execution ID for Feedback
		/////////////////////////////////////

		if (m_ipInternals == NULL)
		{
			_lastCodePos = "19";
			// Get Feedback Manager interface
			UCLID_AFCORELib::IFeedbackMgrPtr ipManager = getThisAsCOMPtr()->FeedbackManager;

			// Get Internals interface
			m_ipInternals = ipManager;
			ASSERT_RESOURCE_ALLOCATION( "ELI09054", m_ipInternals != NULL );
		}
		_lastCodePos = "20";

		// Get RSD File from Rule Set
		_bstr_t _bstrRSD = ipRuleSet->FileName;
		_lastCodePos = "21";

		// Get next ID
		_bstr_t	bstrID = m_ipInternals->RecordRuleExecution(ipAFDoc, _bstrRSD);
		_lastCodePos = "22";

		// Add string tag if ID is defined
		if (bstrID.length() > 0)
		{
			_lastCodePos = "23";

			// Retrieve existing String Tags from AFDocument
			IStrToStrMapPtr	ipStringTags = ipAFDoc->StringTags;
			ASSERT_RESOURCE_ALLOCATION("ELI09062", ipStringTags != NULL);

			// Add Rule ID tag to AFDocument
			ipStringTags->Set(get_bstr_t(gstrRULE_EXEC_ID_TAG_NAME.c_str()), bstrID);
		}
		_lastCodePos = "24";

		// Update progress status to indicate that we are next going to execute rules
		if (ipProgressStatus)
		{
			_lastCodePos = "25";
			ipProgressStatus->StartNextItemGroup("Executing rules...", nNUM_PROGRESS_ITEMS_EXECUTE_RULES);
		}
		_lastCodePos = "26";

		// Create a pointer to the Sub-ProgressStatus object, depending upon whether
		// the caller requested progress information
		IProgressStatusPtr ipSubProgressStatus = (ipProgressStatus == NULL) ? 
			NULL : ipProgressStatus->SubProgressStatus;
		_lastCodePos = "27";

		// Find the Attributes
		findAttributesInText(pDoc, ipRuleSet, pvecAttributeNames, ipSubProgressStatus, pAttributes);
		_lastCodePos = "28";

		// Provide Found Data to Feedback
		if (m_ipInternals)
		{
			_lastCodePos = "29";
			m_ipInternals->RecordFoundData(bstrID, *pAttributes);
		}
		_lastCodePos = "30";

		// Update progress status to indicate that we are done executing rules
		if (ipProgressStatus)
		{
			_lastCodePos = "31";
			ipProgressStatus->CompleteCurrentItemGroup();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07820");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeFinderEngine::get_FeedbackManager(IFeedbackMgr **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Default to NULL return
		*pVal = NULL;

		// Create the object if it has not yet been created
		if (m_ipFeedbackMgr == NULL)
		{
			// Retrieve ProgID from Registry
			RegistryPersistenceMgr	rpm( HKEY_LOCAL_MACHINE, 
				gstrAF_REG_UTILS_FOLDER_PATH );

			string strProgID = rpm.getKeyValue( gstrAF_REG_FEEDBACK_FOLDER, 
				gstrAF_FEEDBACK_PROGID_KEY );

			if (strProgID.length() > 2)
			{
				m_ipFeedbackMgr.CreateInstance( strProgID.c_str() );
				if (m_ipFeedbackMgr == NULL)
				{
					UCLIDException ue( "ELI11111", "Failed to create Feedback Manager." );
					ue.addDebugInfo( "Prog ID", strProgID );
					throw ue;
				}
			}
			else
			{
				UCLIDException ue( "ELI11110", "ProgID for Feedback Manager not found." );
				throw ue;
			}
		}

		CComQIPtr<IFeedbackMgr> ipManager( m_ipFeedbackMgr );
		ipManager.CopyTo( pVal );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09052")

	return S_OK;
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
		UCLID_AFCORELib::IAttributeFinderEnginePtr ipEngine(this);
		CAFAboutDlg dlgAbout( eType, strProduct, ipEngine );
		dlgAbout.DoModal();
	}	
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11641");

	return S_OK;
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
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13440");

	return S_OK;
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
// Public functions
//-------------------------------------------------------------------------------------------------
void CAttributeFinderEngine::findAttributesInText(IAFDocument* pAFDoc,
												  UCLID_AFCORELib::IRuleSetPtr ipRuleSet, 
												  IVariantVector *pvecAttributeNames, 
												  IProgressStatus *pProgressStatus,
												  IIUnknownVector* *pAttributes)
{
	CWaitCursor waitCursor;
	
	if (ipRuleSet != NULL)
	{		
		// find all attributes' values through current rule set
		UCLID_AFCORELib::IAFDocumentPtr ipAFDoc(pAFDoc);
		IIUnknownVectorPtr ipAttributes = ipRuleSet->ExecuteRulesOnText(ipAFDoc, 
			pvecAttributeNames, pProgressStatus);

		// return the attributes to the caller
		*pAttributes = ipAttributes.Detach();
	}
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
UCLID_AFCORELib::IAttributeFinderEnginePtr CAttributeFinderEngine::getThisAsCOMPtr()
{
	UCLID_AFCORELib::IAttributeFinderEnginePtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI16962", ipThis != NULL);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
IOCREnginePtr CAttributeFinderEngine::getOCREngine()
{
	if (m_ipOCREngine == NULL)
	{
		m_ipOCREngine.CreateInstance( CLSID_ScansoftOCR );
		ASSERT_RESOURCE_ALLOCATION( "ELI07830", m_ipOCREngine != NULL );
		
		_bstr_t _bstrPrivateLicenseCode = get_bstr_t(LICENSE_MGMT_PASSWORD);
		IPrivateLicensedComponentPtr ipScansoftEngine(m_ipOCREngine);
		ipScansoftEngine->InitPrivateLicense( _bstrPrivateLicenseCode );
	}

	return m_ipOCREngine;
}
//-------------------------------------------------------------------------------------------------
IOCRUtilsPtr CAttributeFinderEngine::getOCRUtils()
{
	if (m_ipOCRUtils == NULL)
	{
		m_ipOCRUtils.CreateInstance(CLSID_OCRUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI07831", m_ipOCRUtils != NULL);
	}

	return m_ipOCRUtils;
}
//-------------------------------------------------------------------------------------------------
void CAttributeFinderEngine::getComponentDataFolder(string& rFolder)
{
	// no matter whether we are in debug or release mode, if 
	// the component data folder has been defined in the registry
	// we should use that folder
	// Check for key existence
	if (!ma_pUserCfgMgr->keyExists( gstrAF_REG_SETTINGS_FOLDER, COMPONENT_DATA_FOLDER_KEY ))
	{
		// Default value for this key is an empty string
		ma_pUserCfgMgr->createKey( gstrAF_REG_SETTINGS_FOLDER, COMPONENT_DATA_FOLDER_KEY, 
			"");
	};

	// get the key value
	rFolder = ma_pUserCfgMgr->getKeyValue( 
		gstrAF_REG_SETTINGS_FOLDER, COMPONENT_DATA_FOLDER_KEY );
	
	// if the kay value is not empty, then it shall be assumed
	// that that's the component data folder that we should use
	if (!rFolder.empty())
	{
		return;
	}

	// the registry key was not defined - so, use the default
	// definition of the component data folder depending upon
	// whether we are in debug mode or release mode
	const string COMPONENT_DATA = "ComponentData";

	// else look for this component's location on the machine
	// It is always under FlexIndexComponents\Bin
	string strThisModulePath = ::getModuleDirectory(_Module.m_hInst);
	// go up one level 
	int nLastSlash = strThisModulePath.find_last_of("\\");
	rFolder = strThisModulePath.substr(0, nLastSlash+1);
	
	// append the ComponentData folder name
	// This module changed location and is now in the CommonComponents folder per FlexIDSCore #3198
	// so the folder FlexIndexComponents needs to be added before the ComponentData
	rFolder += "FlexIndexComponents\\" + COMPONENT_DATA;

	if (!isValidFolder(rFolder))
	{
		UCLIDException ue("ELI07102", "ComponentData folder doesn't exist.");
		ue.addDebugInfo("ComponentData Folder", rFolder);
		ue.addWin32ErrorInfo();
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CAttributeFinderEngine::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04904", "Attribute Finder Engine" );
}
//-------------------------------------------------------------------------------------------------
