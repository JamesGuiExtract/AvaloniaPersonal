// MiscUtils.cpp : Implementation of CMiscUtils

#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "MiscUtils.h"

#include <ExtractMFCUtils.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <RegistryPersistenceMgr.h>
#include <UCLIDException.h>
#include <EncryptedFileManager.h>
#include <LicenseMgmt.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <ComponentLicenseIDs.h>
#include <PromptDlg.h>
#include <TextFunctionExpander.h>

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// const string for file name header
const string gstrFILE_NAME_HEADER = "file://";
const string gstrMISC_UTILS_REG_PATH = 
	"Software\\Extract Systems\\ReusableComponents\\UCLIDCOMUtils\\MiscUtils";
const string gstrDEFAULT_PARSER_KEY_NAME = "Default";
const string gstrDEFAULT_PARSER_PROG_ID = "ESRegExParser.DotNetRegExParser.1";

// Source document name tag
const std::string strSOURCE_DOC_NAME_TAG = "<SourceDocName>";

//-------------------------------------------------------------------------------------------------
// CMiscUtils
//-------------------------------------------------------------------------------------------------
CMiscUtils::CMiscUtils()
{
	try
	{
		// create instance of the registry persistence mgr
		m_apSettings = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_CURRENT_USER, ""));
		ASSERT_RESOURCE_ALLOCATION("ELI07621", m_apSettings.get() != NULL);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07620")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IMiscUtils,
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
// IMiscUtils
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::AutoEncryptFile(BSTR strFile, BSTR strRegistryKey)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// convert the arguments into STL strings
		string stdstrFile = asString( strFile );
		string stdstrRegFullKey = asString( strRegistryKey );

		// compute the folder and keyname from the registry key
		// for use with the RegistryPersistenceMgr class
		long nLastPos = stdstrRegFullKey.find_last_of('\\');
		if (nLastPos == string::npos)
		{
			UCLIDException ue("ELI07701", "Invalid registry key!");
			ue.addDebugInfo("RegKey", stdstrRegFullKey);
			throw ue;
		}
		string strRegFolder(stdstrRegFullKey, 0, nLastPos);
		string strRegKey(stdstrRegFullKey, nLastPos + 1, 
			stdstrRegFullKey.length() - nLastPos - 1);

		// if the extension is not .etf, then just return
		string strExt = getExtensionFromFullPath(stdstrFile, true);
		if (strExt != ".etf")
		{
			return S_OK;
		}

		// Get name for the base file,
		// for instance, if stdstrFile = "XYZ.dcc.etf" 
		// then the base file will be "XYZ.dcc"
		string strBaseFile = getPathAndFileNameWithoutExtension(stdstrFile);

		// File must exist to continue
		if (!isFileOrFolderValid(strBaseFile.c_str()))
		{
			return S_OK;
		}

		bool bAutoEncryptOn = false;
		
		// Protect the m_apSettings data member
		{
			CSingleLock lg( &m_mutex, TRUE );
			// check if the registry key for auto-encrypt exists.
			// if it does not, create the key with a default value of "0"
			if (!m_apSettings->keyExists(strRegFolder, strRegKey))
			{
				m_apSettings->createKey(strRegFolder, strRegKey, "0");
			}
			else
			{
				// get the key value. If it is "1", then auto-encrypt 
				// setting is on
				if (m_apSettings->getKeyValue(strRegFolder, strRegKey) == "1")
				{
					bAutoEncryptOn = true;
				}
			}
		}
		// AutoEncrypt must be ON in registry to continue
		if (!bAutoEncryptOn)
		{
			return S_OK;
		}

		// If ETF already exists, compare the last modification
		// on both the base file and the etf file
		if (::isFileOrFolderValid(stdstrFile.c_str()))
		{
			// Compare timestamps
			CTime tmBaseFile(getFileModificationTimeStamp(strBaseFile));
			CTime tmETFFile(getFileModificationTimeStamp(stdstrFile));
			if (tmBaseFile <= tmETFFile)
			{
				// no need to encrypt the file again
				return S_OK;
			}
		}
				
		// the auto-encrypt can only be done if
		// that functionality is licensed
		try
		{
			// Check licensing for Auto Encrypt utility
			validateETFEngineLicense();
		}
		catch (...)
		{
			return S_OK;
		}

		// Encrypt the base file
		static EncryptedFileManager efm;
		efm.encrypt(strBaseFile, stdstrFile);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07619")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetRegExpData(IIUnknownVector* pFoundData, 
						   long nIndex, long nSubIndex, 
						   long* pnStartPos, long* pnEndPos)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		UCLID_COMUTILSLib::IIUnknownVectorPtr ipFoundData(pFoundData);
		ASSERT_RESOURCE_ALLOCATION("ELI08917", ipFoundData != NULL);

		UCLID_COMUTILSLib::IObjectPairPtr ipObjPair = ipFoundData->At(nIndex);
		ASSERT_RESOURCE_ALLOCATION("ELI08923", ipObjPair != NULL);

		if(nSubIndex < 0)
		{
			UCLID_COMUTILSLib::ITokenPtr ipTok = ipObjPair->GetObject1();
			ASSERT_RESOURCE_ALLOCATION("ELI08922", ipTok != NULL);

			*pnStartPos = ipTok->GetStartPosition();
			*pnEndPos = ipTok->GetEndPosition();
		}
		else
		{
			UCLID_COMUTILSLib::IIUnknownVectorPtr ipTmpVec = ipObjPair->GetObject2();
			ASSERT_RESOURCE_ALLOCATION("ELI08920", ipTmpVec != NULL);

			UCLID_COMUTILSLib::ITokenPtr ipTok = ipTmpVec->At(nSubIndex);
			ASSERT_RESOURCE_ALLOCATION("ELI08921", ipTok != NULL);

			*pnStartPos = ipTok->GetStartPosition();
			*pnEndPos = ipTok->GetEndPosition();
		}

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08919")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::AllowUserToSelectAndConfigureObject(IObjectWithDescription* pObject, 
							BSTR bstrCategory, BSTR bstrAFAPICategory, VARIANT_BOOL bAllowNone, 
							LONG lNumRequiredIIDs, GUID* pRequiredIIDs, VARIANT_BOOL* pvbDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// mark pObject as unmodified
		*pvbDirty = VARIANT_FALSE;

		// use smart pointer
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObjectWithDescription = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI16010", ipObjectWithDescription != NULL);

		// access the CopyableObject interface
		UCLID_COMUTILSLib::ICopyableObjectPtr ipCopyableObj = ipObjectWithDescription;
		ASSERT_RESOURCE_ALLOCATION("ELI16011", ipCopyableObj != NULL);
		
		// copy the ObjectWithDescription to prevent corruption of original
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObjectWithDescriptionCopy = ipCopyableObj->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI16012", ipObjectWithDescriptionCopy != NULL);

		// create the IObjectSelectorUI object
		UCLID_COMUTILSLib::IObjectSelectorUIPtr ipObjectSelectorUI(CLSID_ObjectSelectorUI);
		ASSERT_RESOURCE_ALLOCATION("ELI16013", ipObjectSelectorUI != NULL);

		// access licensing interface of ObjectSelectorUI
		UCLID_COMLMLib::IPrivateLicensedComponentPtr ipPLComponent = ipObjectSelectorUI;
		ASSERT_RESOURCE_ALLOCATION("ELI16014", ipPLComponent != NULL);

		// initialize private license for the ObjectSelectorUI
		_bstr_t _bstrKey = LICENSE_MGMT_PASSWORD.c_str();
		ipPLComponent->InitPrivateLicense(_bstrKey);

		// prepare the prompts for object selector
		string strCategory = asString(bstrCategory);
		_bstr_t	bstrDesc = get_bstr_t(strCategory + " description");
		_bstr_t	bstrSelect = get_bstr_t("Select " + strCategory);

		// show the UI
		VARIANT_BOOL vbResult = ipObjectSelectorUI->ShowUI2(bstrCategory, bstrDesc, 
			bstrSelect, bstrAFAPICategory, ipObjectWithDescriptionCopy, bAllowNone,
			lNumRequiredIIDs, pRequiredIIDs);

		// if user selected OK
		if (vbResult == VARIANT_TRUE)
		{
			// retain new object or retain object changes
			ipCopyableObj->CopyFrom(ipObjectWithDescriptionCopy);

			// set dirty flag
			*pvbDirty = VARIANT_TRUE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16015")

	// success
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::AllowUserToSelectAndConfigureObject2(BSTR bstrTitleAfterSelect, BSTR bstrCategory, BSTR bstrAFAPICategory, 
							LONG lNumRequiredIIDs, GUID *pRequiredIIDs, IUnknown **ppObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI18092", ppObject != NULL);

		// Initialize return value to NULL
		*ppObject = NULL;

		// Create an ObjectWithDescription object to use to collect the selected object from IObjectSelectorUI
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObjectWithDescription(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI18091", ipObjectWithDescription != NULL);

		// create the IObjectSelectorUI object
		UCLID_COMUTILSLib::IObjectSelectorUIPtr ipObjectSelectorUI(CLSID_ObjectSelectorUI);
		ASSERT_RESOURCE_ALLOCATION("ELI18094", ipObjectSelectorUI != NULL);

		// access licensing interface of ObjectSelectorUI
		UCLID_COMLMLib::IPrivateLicensedComponentPtr ipPLComponent = ipObjectSelectorUI;
		ASSERT_RESOURCE_ALLOCATION("ELI18095", ipPLComponent != NULL);

		// initialize private license for the ObjectSelectorUI
		_bstr_t _bstrKey = LICENSE_MGMT_PASSWORD.c_str();
		ipPLComponent->InitPrivateLicense(_bstrKey);

		// prepare the prompt for object selector
		_bstr_t	bstrSelect = get_bstr_t("Select " + asString(bstrCategory));

		// show the UI
		VARIANT_BOOL vbResult = ipObjectSelectorUI->ShowUINoDescription(bstrTitleAfterSelect,
			bstrSelect, bstrAFAPICategory, ipObjectWithDescription,
			lNumRequiredIIDs, pRequiredIIDs);

		// if user selected OK
		if (vbResult == VARIANT_TRUE)
		{
			if (ipObjectWithDescription->Object == NULL)
			{
				UCLIDException ue("ELI18098","Internal error: Failure retrieving object selection!");
				ue.addDebugInfo("Category", asString(bstrAFAPICategory));
			}

			*ppObject = (IUnknown *)ipObjectWithDescription->Object.Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18096")

	// success
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::AllowUserToConfigureObjectProperties(IObjectWithDescription* pObject, 
															  VARIANT_BOOL* pvbDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// mark pObject as unmodified
		*pvbDirty = VARIANT_FALSE;

		// convert argument to a smart pointer
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObjectWithDescription = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI15977", ipObjectWithDescription != NULL);

		// get CategorizedComponent from ObjectWithDescription
		UCLID_COMUTILSLib::ICategorizedComponentPtr	
			ipCategorizedComponent = ipObjectWithDescription->Object;
		ASSERT_RESOURCE_ALLOCATION("ELI16024", ipCategorizedComponent != NULL);

		// Ensure the object is configurable
		if (!SupportsConfiguration(ipCategorizedComponent))
		{
			// show error message
			MessageBox(NULL,"This object has no properties to configure.","Error", MB_ICONEXCLAMATION);
			return S_FALSE;
		}

		// access CopyableObject interface
		UCLID_COMUTILSLib::ICopyableObjectPtr ipCopyableObject = ipCategorizedComponent;
		ASSERT_RESOURCE_ALLOCATION("ELI15978", ipCopyableObject != NULL);

		// make a copy for the user to modify without corrupting the original
		UCLID_COMUTILSLib::ICategorizedComponentPtr ipCopy = ipCopyableObject->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI15979", ipCopy != NULL);

		// initialize title string
		string strComponentDesc = ipCategorizedComponent->GetComponentDescription();
		string strTitle = string("Configure ") + strComponentDesc;

		// create IObjectPropertiesUI object
		UCLID_COMUTILSLib::IObjectPropertiesUIPtr ipPropertiesUI(CLSID_ObjectPropertiesUI);
		ASSERT_RESOURCE_ALLOCATION("ELI15980", ipPropertiesUI != NULL);

		// display UI using the copy of the categorized component
		HRESULT hr = ipPropertiesUI->DisplayProperties1(ipCopy, strTitle.c_str());

		// retain changes if OK was selected
		if (hr == S_OK)
		{
			// retain changes
			ipCopyableObject->CopyFrom(ipCopy);

			// mark object as modified
			*pvbDirty = VARIANT_TRUE;
		}
		else if (hr != S_FALSE) // if something other than 'Cancel' or 'OK' was selected
		{
			throw UCLIDException("ELI16044", "Unexpected exit condition from object properties dialog.");
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16034")

	// success
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::AllowUserToConfigureObjectDescription(IObjectWithDescription* pObject, 
															   VARIANT_BOOL* pvbDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// mark pObject as unmodified
		*pvbDirty = VARIANT_FALSE;

		// use smart pointer
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObjectWithDescription = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI15994", ipObjectWithDescription != NULL);

		// get object that ObjectWithDescription contains (must implement CategorizedComponent)
		UCLID_COMUTILSLib::ICategorizedComponentPtr 
			ipComponent = ipObjectWithDescription->Object;
		ASSERT_RESOURCE_ALLOCATION("ELI16025", ipComponent != NULL);

		// get ObjectWithDescription's description
		CString zDescription = static_cast<const char*> (ipObjectWithDescription->Description);

		// find starting angle bracket of component description
		int nComponentDescriptionStartIndex = zDescription.ReverseFind('<');

		// check if the angle bracket was found
		if (nComponentDescriptionStartIndex != -1)
		{
			// remove the component description
			zDescription = zDescription.Left(nComponentDescriptionStartIndex);
		}
		
		// create dialog prompt
		PromptDlg dlg("Configure description", "Description", zDescription);

		// display prompt and check if OK button was pressed
		if (dlg.DoModal() == IDOK)
		{		
			// get new user-inputted object description
			string strDescription = dlg.m_zInput;

			// append the component description
			strDescription += "<" + asString(ipComponent->GetComponentDescription()) + ">";

			// give object user-inputted description
			ipObjectWithDescription->Description = get_bstr_t(strDescription);

			// mark object has modified
			*pvbDirty = VARIANT_TRUE;
		}
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15993")

	// finished
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::HandlePlugInObjectDoubleClick(IObjectWithDescription* pObject, 
							BSTR bstrCategory, BSTR bstrAFAPICategory, VARIANT_BOOL bAllowNone,
							LONG lNumRequiredIIDs, GUID* pRequiredIIDs, VARIANT_BOOL* pvbDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// immediately obtain state of ALT key and CTRL key
		bool bALTKeyIsPressed = (GetKeyState(VK_MENU) < 0);
		bool bCTRLKeyIsPressed = (GetKeyState(VK_CONTROL) < 0);

		// mark pObject as unmodified
		*pvbDirty = VARIANT_FALSE;

		// use smart pointer
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObject = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI16036", ipObject != NULL);

		// get pObject's description and convert to a string
		string strDescription = asString(ipObject->Description);

		// ensure that the object is not <None> OR 
		// the CTRL key is pressed and an object is selected
		if( strDescription.empty() || bCTRLKeyIsPressed )
		{
			// allow the user to select an object
			*pvbDirty = getThisAsCOMPtr()->AllowUserToSelectAndConfigureObject( ipObject, 
				get_bstr_t(bstrCategory), get_bstr_t(bstrAFAPICategory), bAllowNone, 
				lNumRequiredIIDs, pRequiredIIDs);
		}
		else if (bALTKeyIsPressed) // the ALT key is pressed and an object is selected
		{
			// prompt and allow the user to modify ipCategorizedComponent's description
			*pvbDirty = getThisAsCOMPtr()->AllowUserToConfigureObjectDescription(ipObject);
		}
		else // regular double-click and an object is selected
		{
			// prompt and allow the user to modify ipCategorizedComponent's properties
			*pvbDirty = getThisAsCOMPtr()->AllowUserToConfigureObjectProperties(ipObject);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16035")

	// success
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::HandlePlugInObjectCommandButtonClick(IObjectWithDescription* pObject, 
							BSTR bstrCategory, BSTR bstrAFAPICategory, VARIANT_BOOL bAllowNone,
							LONG lNumRequiredIIDs, GUID* pRequiredIIDs,
							int iLeft, int iTop, VARIANT_BOOL* pvbDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// mark pObject as unmodified
		*pvbDirty = VARIANT_FALSE;

		// create pop up menu
		CMenu menu;
		menu.CreatePopupMenu();

		// convert category to a string
		string strCategory = asString(bstrCategory);

		// set the menu options
		string strSelectObject = "Select " + strCategory + "...";
		string strConfigureObjectProperties = "Configure " + strCategory + "...";

		// use a smart pointer
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObject = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI16059", ipObject != NULL);

		// get the object's description string
		string strDescription = asString(ipObject->Description);

		// set the flags for the configure object menu items
		UINT uiConfigureObjectFlags = MF_BYPOSITION | MF_STRING;
		if( strDescription.empty() )
		{
			uiConfigureObjectFlags |= MF_DISABLED | MF_GRAYED;
		}

		// insert menu options
		menu.InsertMenu(-1, MF_BYPOSITION | MF_STRING, 
			ID_MENU_SELECT_OBJECT, strSelectObject.c_str());
		menu.InsertMenu(-1, uiConfigureObjectFlags, 
			ID_MENU_CONFIGURE_OBJECT_PROPERTIES, strConfigureObjectProperties.c_str());
		menu.InsertMenu(-1, uiConfigureObjectFlags, 
			ID_MENU_CONFIGURE_OBJECT_DESCRIPTION, "Configure description...");

		// add a seperator
		menu.InsertMenu(-1, MF_BYPOSITION | MF_SEPARATOR, 0, "");

		// add cancel option
		menu.InsertMenu(-1, MF_BYPOSITION | MF_STRING, 0, "Cancel");

		// create a hidden window to be the menu's parent, or throw exception
		CWnd parent;
		if (!parent.CreateEx(NULL, AfxRegisterWndClass(NULL), "", NULL, 0, 0, 0, 0, NULL, NULL) )
		{
			throw UCLIDException("ELI16058", "Unable to create parent window for context menu.");
		}

		// show menu and get result of user selection
		int iSelected = menu.TrackPopupMenu(TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_NONOTIFY | TPM_RETURNCMD,
			iLeft, iTop, &parent);

		// check which menu option the user selected and handle accordingly
		if (iSelected == ID_MENU_SELECT_OBJECT)
		{
			*pvbDirty = getThisAsCOMPtr()->AllowUserToSelectAndConfigureObject(ipObject, 
				get_bstr_t(bstrCategory), get_bstr_t(bstrAFAPICategory), bAllowNone, lNumRequiredIIDs,
				pRequiredIIDs);
		}
		else if (iSelected == ID_MENU_CONFIGURE_OBJECT_PROPERTIES)
		{
			*pvbDirty = getThisAsCOMPtr()->AllowUserToConfigureObjectProperties(ipObject);
		}
		else if (iSelected == ID_MENU_CONFIGURE_OBJECT_DESCRIPTION)
		{
			*pvbDirty = getThisAsCOMPtr()->AllowUserToConfigureObjectDescription(ipObject);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16053");

	// done
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetNewRegExpParserInstance(BSTR strComponentName, 
													IRegularExprParser **pRegExpParser)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Create the Regular expression parser
		UCLID_COMUTILSLib::IRegularExprParserPtr ipTempRegExpParser(gstrDEFAULT_PARSER_PROG_ID.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI12966", ipTempRegExpParser != NULL);

		*pRegExpParser = (IRegularExprParser *)ipTempRegExpParser.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12967")

	return S_OK; 
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetStringOptionallyFromFile(BSTR bstrFileName, BSTR *pbstrFromFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strFileName = asString(bstrFileName);

		// Get the extension of the file
		string strExt = ::getExtensionFromFullPath( strFileName, true );
		
		// A string to hold all uncommented text read from the file
		string strFromFile = "";
	
		// If the file is an encrpted file
		if ( strExt == ".etf" )
		{
			// Define a encrypted file manager object to read encrypted file
			EncryptedFileManager efm;

			// Return the vector of unencrypted strings from the file
			vector<string> vecStrFromFile = efm.decryptTextFile( strFileName );

			// Create CommentedTextFileReader object to read the vector of string
			CommentedTextFileReader fileReader(vecStrFromFile, "//", false);

			while (!fileReader.reachedEndOfStream())
			{
				// Put each uncommented line into the string, add "\r\n" between each line
				string strTemp = fileReader.getLineText();
				strFromFile += strTemp + "\r\n";
			};
		}
		else
		{
			// Create CommentedTextFileReader object to read the file
			CString zFileName(strFileName.c_str());
			ifstream ifs(zFileName);
			CommentedTextFileReader fileReader(ifs, "//", false);

			while (!ifs.eof())
			{
				// Put each uncommented line into the string, add "\r\n" between each line
				string strTemp = fileReader.getLineText();
				strFromFile += strTemp + "\r\n";
			};
		}

		// Remove the last \r\n that has been added
		if (strFromFile.substr((strFromFile.length() - 2), strFromFile.length()) == "\r\n")
		{
			strFromFile.erase(strFromFile.length() - 2, strFromFile.length());
		}

		*pbstrFromFile = get_bstr_t(strFromFile).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14530")

	return S_OK; 
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetFileHeader(BSTR *pbstrFileHeader)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pbstrFileHeader = get_bstr_t(gstrFILE_NAME_HEADER).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14543")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetColumnStringsOptionallyFromFile(BSTR bstrFileName, IVariantVector* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		UCLID_COMUTILSLib::IVariantVectorPtr ipVector(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI14591", ipVector != NULL);

		string strFileName = asString(bstrFileName);

		// Get the extension of the file
		string strExt = ::getExtensionFromFullPath( strFileName, true );
		
		// A string to hold one line of uncommented text read from the file
		string strFromFile = "";

		// If the file is an encrypted file
		if ( strExt == ".etf" )
		{
			// Define a encrypted file manager object to read encrypted file
			EncryptedFileManager efm;

			// Return the vector of unencrypted strings from the file
			vector<string> vecStrFromFile = efm.decryptTextFile( strFileName );

			// Create CommentedTextFileReader object to read the vector of string
			CommentedTextFileReader fileReader(vecStrFromFile, "//", false);

			while (!fileReader.reachedEndOfStream())
			{
				// Get one line of text
				string strLine("");
				strLine = fileReader.getLineText();

				if (!strLine.empty())
				{
					ipVector->PushBack(get_bstr_t(strLine));
				}
			};
		}
		else
		{
			// Create CommentedTextFileReader object to read the file
			ifstream ifs(strFileName.c_str());
			CommentedTextFileReader fileReader(ifs, "//", false);

			while (!fileReader.reachedEndOfStream())
			{
				// Get one line of text
				string strLine("");
				strLine = fileReader.getLineText();
				if (!strLine.empty())
				{				
					ipVector->PushBack(get_bstr_t(strLine));
				}
			};
		}

		*pVal = (IVariantVector*)ipVector.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14592")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetFileNameWithoutHeader(BSTR bstrText, BSTR *pbstrFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strFileNameWithHeader = asString(bstrText);
		// Create a temp string that contain lower case of strFileNameWithHeader
		string strTempLowerCase = strFileNameWithHeader;
		makeLowerCase(strTempLowerCase);

		// If the string doesn't begin with gstrFILE_NAME_HEADER, or only contains gstrFILE_NAME_HEADER
		// it will be treated as a string instead of a file name
		int iLengthOfHeader = gstrFILE_NAME_HEADER.length();
		if (strTempLowerCase.substr(0, iLengthOfHeader) != gstrFILE_NAME_HEADER 
			|| strTempLowerCase == gstrFILE_NAME_HEADER)
		{
			*pbstrFileName = get_bstr_t(bstrText).Detach();
			return S_OK;
		}

		// Remove the header of the file name
		string strFileName = strFileNameWithHeader.substr(iLengthOfHeader, string::npos);

		// Trim the spaces on both sides
		strFileName = trim(strFileName, " ", " ");

		*pbstrFileName = get_bstr_t(strFileName).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14617")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::CountEnabledObjectsIn(IIUnknownVector* pVector, long* lNumEnabledObjects)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// use a smart pointer for the IUnknownVector
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipVector = pVector;
		ASSERT_RESOURCE_ALLOCATION("ELI16264", ipVector != NULL);

		// initialize the count of enabled objects
		long lEnabledObjectsCount = 0;
		
		// iterate through each object in the IUnknownVector
		long lNumTotalObjects = ipVector->Size();
		for(long i=0; i < lNumTotalObjects; i++)
		{
			// get the ObjectWithDescription interface of the current object
			UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObject = ipVector->At(i);

			// count this object if it is enabled
			if ( ipObject && asCppBool(ipObject->Enabled) )
			{
				lEnabledObjectsCount++;
			}
		}

		// set the return value
		*lNumEnabledObjects = lEnabledObjectsCount;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16260");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetEnabledState(IIUnknownVector* pVector, LONG nItemIndex, 
										 VARIANT_BOOL* pvbEnabled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Convert IUnknownVector to a smart pointer
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipVector = pVector;
		ASSERT_RESOURCE_ALLOCATION("ELI16614", ipVector != NULL);

		// Validate index
		long lCount = ipVector->Size();
		ASSERT_ARGUMENT("ELI16615", (nItemIndex >= 0 && nItemIndex < lCount));

		// Retrieve specified item
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipOWD = ipVector->At( nItemIndex );
		ASSERT_RESOURCE_ALLOCATION("ELI16616", ipOWD != NULL);

		// Provide Enabled setting
		*pvbEnabled = ipOWD->Enabled;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16612");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::SetEnabledState(IIUnknownVector* pVector, LONG nItemIndex, 
										 VARIANT_BOOL bEnabled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Convert IUnknownVector to a smart pointer
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipVector = pVector;
		ASSERT_RESOURCE_ALLOCATION("ELI16617", ipVector != NULL);

		// Validate index
		long lCount = ipVector->Size();
		ASSERT_ARGUMENT("ELI16618", (nItemIndex >= 0 && nItemIndex < lCount));

		// Retrieve specified item
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipOWD = ipVector->At( nItemIndex );
		ASSERT_RESOURCE_ALLOCATION("ELI16619", ipOWD != NULL);

		// Apply the Enabled setting
		ipOWD->Enabled = bEnabled;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16613");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::IsAnyObjectDirty1(IUnknown* pObject, VARIANT_BOOL* pvbDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Get persistence object
		IPersistStreamPtr ipPersistStream = pObject;
		if (ipPersistStream == NULL)
		{
			throw UCLIDException("ELI16632", "Object does not support persistence!");
		}

		// Check the dirty flag
		HRESULT hr = ipPersistStream->IsDirty();
		if (hr == S_OK)
		{
			// Object is dirty
			*pvbDirty = VARIANT_TRUE;
		}
		else
		{
			// Object is not dirty
			*pvbDirty = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16294");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::IsAnyObjectDirty2(IUnknown* pObject1, IUnknown* pObject2, 
										   VARIANT_BOOL* pvbDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// License validation is handled inside IsAnyObjectDirty1()

		UCLID_COMUTILSLib::IMiscUtilsPtr ipThis = getThisAsCOMPtr();

		// Test each object and check results
		if ((ipThis->IsAnyObjectDirty1(pObject1) == VARIANT_TRUE) || 
			(ipThis->IsAnyObjectDirty1(pObject2) == VARIANT_TRUE))
		{
			// An object is dirty
			*pvbDirty = VARIANT_TRUE;
		}
		else
		{
			// Neither object is dirty
			*pvbDirty = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16295");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::IsAnyObjectDirty3(IUnknown* pObject1, IUnknown* pObject2, 
										   IUnknown* pObject3, VARIANT_BOOL* pvbDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// License validation is handled inside IsAnyObjectDirty1()

		UCLID_COMUTILSLib::IMiscUtilsPtr ipThis = getThisAsCOMPtr();

		// Test each object and check results
		if ((ipThis->IsAnyObjectDirty1(pObject1) == VARIANT_TRUE) || 
			(ipThis->IsAnyObjectDirty1(pObject2) == VARIANT_TRUE) || 
			(ipThis->IsAnyObjectDirty1(pObject3) == VARIANT_TRUE))
		{
			// An object is dirty
			*pvbDirty = VARIANT_TRUE;
		}
		else
		{
			// None of the objects are dirty
			*pvbDirty = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16630");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::ShellOpenDocument(BSTR bstrFilename)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		shellOpenDocument(asString(bstrFilename));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18163");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetObjectAsStringizedByteStream(IUnknown *pObject, BSTR *pbstrByteStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22016", pObject != NULL);
		ASSERT_ARGUMENT("ELI22022", pbstrByteStream != NULL);

		// ensure first that the object supports persistence
		IPersistStreamPtr ipPersistObj = pObject;
		if (ipPersistObj == NULL)
		{
			throw UCLIDException("ELI22017", "Object cannot be copied to byte stream "
				"because it does not support persistence!");
		}

		// create a temporary IStream object
		IStreamPtr ipStream;
		HANDLE_HRESULT(CreateStreamOnHGlobal(NULL, TRUE, &ipStream),
			"ELI22018", "Unable to create stream object!", ipStream, IID_IStream);

		// stream the object into the IStream
		writeObjectToStream(ipPersistObj, ipStream, "ELI22019", FALSE);

		// find the size of the stream
		LARGE_INTEGER zeroOffset;
		zeroOffset.QuadPart = 0;
		ULARGE_INTEGER length;
		ipStream->Seek(zeroOffset, STREAM_SEEK_END, &length);

		// Allocate memory to read stream
		unsigned char* pszBuffer = new unsigned char[length.LowPart];
		ASSERT_RESOURCE_ALLOCATION("ELI22020", pszBuffer != NULL);
		memset(pszBuffer, 0, length.LowPart);

		try
		{
			try
			{
				// copy the data in the stream to the buffer
				ipStream->Seek(zeroOffset, STREAM_SEEK_SET, NULL);
				ipStream->Read(pszBuffer, length.LowPart, NULL);

				ByteStream byteStream(pszBuffer, length.LowPart);
				string strTemp = byteStream.asString();

				// Delete the memory allocated for the buffer
				delete [] pszBuffer;

				// return the byte stream
				*pbstrByteStream = get_bstr_t(strTemp).Detach();
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI22021");
		}
		catch(UCLIDException& uex)
		{
			// Need to clean up allocated memory
			if (pszBuffer != NULL)
			{
				delete [] pszBuffer;
			}

			throw uex;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22015");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetObjectFromStringizedByteStream(BSTR bstrByteStream, IUnknown **ppObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22024", ppObject != NULL);

		// create a byte stream from the BSTR
		ByteStream byteStream(asString(bstrByteStream));

		// create a temporary IStream object
		IStreamPtr ipStream;
		HANDLE_HRESULT(CreateStreamOnHGlobal(NULL, TRUE, &ipStream),
			"ELI22025", "Unable to create stream object!", ipStream, IID_IStream);

		// Write the buffer to the stream
		ipStream->Write(byteStream.getData(), byteStream.getLength(), NULL);

		// Reset the stream current position to the beginning of the stream
		LARGE_INTEGER zeroOffset;
		zeroOffset.QuadPart = 0;
		ipStream->Seek(zeroOffset, STREAM_SEEK_SET, NULL);

		// Stream the object out of the IStream
		IPersistStreamPtr ipPersistObj;
		readObjectFromStream(ipPersistObj, ipStream, "ELI22026");

		// Return the object
		*ppObject = ipPersistObj.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22023");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetExpandedTags(BSTR bstrString, BSTR bstrSourceDocName, 
										 BSTR* pbstrExpanded)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameters
		ASSERT_ARGUMENT("ELI22713", bstrString != NULL);
		ASSERT_ARGUMENT("ELI22714", bstrSourceDocName != NULL);

		// Convert to std strings
		string strString = asString(bstrString);
		string strSourceDocName = asString(bstrSourceDocName);

		// Check if the source document name tag was found
		bool bSourceDocNameTagFound = strString.find(strSOURCE_DOC_NAME_TAG) != string::npos;
		if (bSourceDocNameTagFound)
		{
			// Ensure a source document was specified
			if (strSourceDocName == "")
			{
				throw UCLIDException("ELI22715", "Cannot expand tag without source document.");
			}

			// Replace the source document tag
			replaceVariable(strString, strSOURCE_DOC_NAME_TAG, strSourceDocName);
		}

		// Expand the tag functions
		TextFunctionExpander tfe;
		string strExpanded = tfe.expandFunctions(strString); 

		// Return the result
		*pbstrExpanded = _bstr_t(strExpanded.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22712");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetExpansionFunctionNames(IVariantVector** ppFunctionNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Get the available expansion functions
		TextFunctionExpander tfe;
		vector<string> vecFunctions = tfe.getAvailableFunctions();
		tfe.formatFunctions(vecFunctions);
		
		// Populate an IVariantVector of BSTRs
		UCLID_COMUTILSLib::IVariantVectorPtr ipFunctions(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI22717", ipFunctions != NULL);
		for each (string function in vecFunctions)
		{
			ipFunctions->PushBack(function.c_str());
		}

		// Return the result
		*ppFunctionNames = (IVariantVector*) ipFunctions.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22716");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		validateLicense();

		// if validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Static public methods
//-------------------------------------------------------------------------------------------------
bool CMiscUtils::SupportsConfiguration(IUnknown *pObject)
{
	try
	{
		// Default to false (object does not support configuration)
		bool bConfigurable = false;

		// Do not ASSERT if pObject is NULL [FlexIDSCore #3554]
		if (pObject != NULL)
		{
			// Check to see if pObject is configurable via ISpecifyPropertyPages
			ISpecifyPropertyPagesPtr ipSpecifyPropertyPages(pObject);
			if (ipSpecifyPropertyPages != NULL)
			{
				bConfigurable = true;
			}
			// Check to see if pObject is configurable via IConfigurableObject
			else
			{
				UCLID_COMUTILSLib::IConfigurableObjectPtr ipConfigurableObject(pObject);
				if (ipConfigurableObject != NULL) 
				{
					bConfigurable = true;
				}
			}
		}

		return bConfigurable;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25497");
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
UCLID_COMUTILSLib::IMiscUtilsPtr CMiscUtils::getThisAsCOMPtr()
{
	UCLID_COMUTILSLib::IMiscUtilsPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI16973", ipThis != NULL);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
void CMiscUtils::validateLicense()
{
	VALIDATE_LICENSE( gnEXTRACT_CORE_OBJECTS, "ELI07622", "UCLID_COMUTILSLib.MiscUtils" );
}
//-------------------------------------------------------------------------------------------------
void CMiscUtils::validateETFEngineLicense()
{
	// RDT license is required for auto-encryption to encrypt
	VALIDATE_LICENSE( gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS, "ELI07105", "Encrypt File" );
}
//-------------------------------------------------------------------------------------------------
