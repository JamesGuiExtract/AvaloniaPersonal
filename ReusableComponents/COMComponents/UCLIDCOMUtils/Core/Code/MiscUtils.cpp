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
#include <Misc.h>
#include <ExtractFileLock.h>
#include <Range.h>
#include <iostream>
#include <sstream>

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// const string for file name header
const string gstrFILE_NAME_HEADER = "file://";
const string gstrDEFAULT_PARSER_PROG_ID = "ESRegExParser.DotNetRegExParser.1";

// Tag names
const std::string strSOURCE_DOC_NAME_TAG = "<SourceDocName>";
const std::string strCOMMON_COMPONENTS_DIR_TAG = "<CommonComponentsDir>";

//-------------------------------------------------------------------------------------------------
// CMiscUtils
//-------------------------------------------------------------------------------------------------
CMiscUtils::CMiscUtils()
{
	try
	{
		// create instance of the registry persistence mgr
		m_apSettings.reset(new RegistryPersistenceMgr(HKEY_CURRENT_USER, ""));
		ASSERT_RESOURCE_ALLOCATION("ELI07621", m_apSettings.get() != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI07620");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IMiscUtils,
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
// IMiscUtils
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::AutoEncryptFile(BSTR strFile, BSTR strRegistryKey)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

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

		autoEncryptFile(asString(strFile), asString(strRegistryKey));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07619")
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
		ASSERT_RESOURCE_ALLOCATION("ELI08917", ipFoundData != __nullptr);

		UCLID_COMUTILSLib::IObjectPairPtr ipObjPair = ipFoundData->At(nIndex);
		ASSERT_RESOURCE_ALLOCATION("ELI08923", ipObjPair != __nullptr);

		if(nSubIndex < 0)
		{
			UCLID_COMUTILSLib::ITokenPtr ipTok = ipObjPair->GetObject1();
			ASSERT_RESOURCE_ALLOCATION("ELI08922", ipTok != __nullptr);

			*pnStartPos = ipTok->GetStartPosition();
			*pnEndPos = ipTok->GetEndPosition();
		}
		else
		{
			UCLID_COMUTILSLib::IIUnknownVectorPtr ipTmpVec = ipObjPair->GetObject2();
			ASSERT_RESOURCE_ALLOCATION("ELI08920", ipTmpVec != __nullptr);

			UCLID_COMUTILSLib::ITokenPtr ipTok = ipTmpVec->At(nSubIndex);
			ASSERT_RESOURCE_ALLOCATION("ELI08921", ipTok != __nullptr);

			*pnStartPos = ipTok->GetStartPosition();
			*pnEndPos = ipTok->GetEndPosition();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08919")
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
		ASSERT_RESOURCE_ALLOCATION("ELI16010", ipObjectWithDescription != __nullptr);

		// access the CopyableObject interface
		UCLID_COMUTILSLib::ICopyableObjectPtr ipCopyableObj = ipObjectWithDescription;
		ASSERT_RESOURCE_ALLOCATION("ELI16011", ipCopyableObj != __nullptr);
		
		// copy the ObjectWithDescription to prevent corruption of original
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObjectWithDescriptionCopy = ipCopyableObj->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI16012", ipObjectWithDescriptionCopy != __nullptr);

		// create the IObjectSelectorUI object
		UCLID_COMUTILSLib::IObjectSelectorUIPtr ipObjectSelectorUI(CLSID_ObjectSelectorUI);
		ASSERT_RESOURCE_ALLOCATION("ELI16013", ipObjectSelectorUI != __nullptr);

		// access licensing interface of ObjectSelectorUI
		UCLID_COMLMLib::IPrivateLicensedComponentPtr ipPLComponent = ipObjectSelectorUI;
		ASSERT_RESOURCE_ALLOCATION("ELI16014", ipPLComponent != __nullptr);

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

		// success
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16015")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::AllowUserToSelectAndConfigureObject2(BSTR bstrTitleAfterSelect, BSTR bstrCategory, BSTR bstrAFAPICategory, 
							LONG lNumRequiredIIDs, GUID *pRequiredIIDs, IUnknown **ppObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI18092", ppObject != __nullptr);

		// Initialize return value to NULL
		*ppObject = NULL;

		// Create an ObjectWithDescription object to use to collect the selected object from IObjectSelectorUI
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObjectWithDescription(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI18091", ipObjectWithDescription != __nullptr);

		// create the IObjectSelectorUI object
		UCLID_COMUTILSLib::IObjectSelectorUIPtr ipObjectSelectorUI(CLSID_ObjectSelectorUI);
		ASSERT_RESOURCE_ALLOCATION("ELI18094", ipObjectSelectorUI != __nullptr);

		// access licensing interface of ObjectSelectorUI
		UCLID_COMLMLib::IPrivateLicensedComponentPtr ipPLComponent = ipObjectSelectorUI;
		ASSERT_RESOURCE_ALLOCATION("ELI18095", ipPLComponent != __nullptr);

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

		// success
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18096")
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
		ASSERT_RESOURCE_ALLOCATION("ELI15977", ipObjectWithDescription != __nullptr);

		// get CategorizedComponent from ObjectWithDescription
		UCLID_COMUTILSLib::ICategorizedComponentPtr	
			ipCategorizedComponent = ipObjectWithDescription->Object;
		ASSERT_RESOURCE_ALLOCATION("ELI16024", ipCategorizedComponent != __nullptr);

		// Ensure the object is configurable
		if (!SupportsConfiguration(ipCategorizedComponent))
		{
			// show error message
			MessageBox(NULL,"This object has no properties to configure.","Error", MB_ICONEXCLAMATION);
			return S_FALSE;
		}

		// access CopyableObject interface
		UCLID_COMUTILSLib::ICopyableObjectPtr ipCopyableObject = ipCategorizedComponent;
		ASSERT_RESOURCE_ALLOCATION("ELI15978", ipCopyableObject != __nullptr);

		// make a copy for the user to modify without corrupting the original
		UCLID_COMUTILSLib::ICategorizedComponentPtr ipCopy = ipCopyableObject->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI15979", ipCopy != __nullptr);

		// initialize title string
		string strComponentDesc = ipCategorizedComponent->GetComponentDescription();
		string strTitle = strComponentDesc + " settings";

		// create IObjectPropertiesUI object
		UCLID_COMUTILSLib::IObjectPropertiesUIPtr ipPropertiesUI(CLSID_ObjectPropertiesUI);
		ASSERT_RESOURCE_ALLOCATION("ELI15980", ipPropertiesUI != __nullptr);

		// Display UI using the copy of the categorized component and retain changes if OK was
		// selected.
		if(asCppBool(ipPropertiesUI->DisplayProperties1(ipCopy, strTitle.c_str())))
		{
			// retain changes
			ipCopyableObject->CopyFrom(ipCopy);

			// mark object as modified
			*pvbDirty = VARIANT_TRUE;
		}

		// success
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16034")
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
		ASSERT_RESOURCE_ALLOCATION("ELI15994", ipObjectWithDescription != __nullptr);

		// get object that ObjectWithDescription contains (must implement CategorizedComponent)
		UCLID_COMUTILSLib::ICategorizedComponentPtr 
			ipComponent = ipObjectWithDescription->Object;
		ASSERT_RESOURCE_ALLOCATION("ELI16025", ipComponent != __nullptr);

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
	
		// finished
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15993")
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
		ASSERT_RESOURCE_ALLOCATION("ELI16036", ipObject != __nullptr);

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

		// success
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16035")
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
		ASSERT_RESOURCE_ALLOCATION("ELI16059", ipObject != __nullptr);

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

		// add a separator
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

		// done
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16053");
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
		ASSERT_RESOURCE_ALLOCATION("ELI12966", ipTempRegExpParser != __nullptr);

		*pRegExpParser = (IRegularExprParser *)ipTempRegExpParser.Detach();

		return S_OK; 
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12967")
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
	
		// If the file is an encrypted file
		if ( strExt == ".etf" )
		{
			// Define a encrypted file manager object to read encrypted file
			MapLabelManager encryptedFileManager;

			// Return the vector of unencrypted strings from the file
			vector<string> vecStrFromFile = encryptedFileManager.getMapLabel( strFileName );

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
				if (!ifs)
				{
					UCLIDException ue("ELI50347", "Failed to read file");
					ue.addDebugInfo("Filename", strFileName);
					throw ue;
				}
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

		return S_OK; 
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14530")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetBase64StringFromFile(BSTR bstrFileName, BSTR *pbstrFromFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strFileName = asString(bstrFileName);

		// Get the extension of the file
		string strExt = ::getExtensionFromFullPath( strFileName, true );
		
		// A string to hold text read from the file
		string strFromFile;
	
		// If the file is an encrypted file
		if ( strExt == ".etf" )
		{
			// Define a encrypted file manager object to read encrypted file
			MapLabelManager encryptedFileManager;

			// Retrieve binary data from the text file
			unsigned long ulLength = 0;
			unsigned char* pszData = encryptedFileManager.getMapLabel(strFileName, &ulLength );
			std::vector<uchar> data(pszData, pszData + ulLength);
			delete [] pszData;
			strFromFile = Util::base64Encode(data);
		}
		else
		{
			CString zFileName(strFileName.c_str());
			ifstream ifs(zFileName, ifstream::binary);
			stringstream data;
			data << ifs.rdbuf();
			strFromFile = Util::base64Encode(data);
		}

		*pbstrFromFile = get_bstr_t(strFromFile).Detach();

		return S_OK; 
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI44902")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetFileHeader(BSTR *pbstrFileHeader)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pbstrFileHeader = get_bstr_t(gstrFILE_NAME_HEADER).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14543")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetColumnStringsOptionallyFromFile(BSTR bstrFileName, IVariantVector* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		UCLID_COMUTILSLib::IVariantVectorPtr ipVector(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI14591", ipVector != __nullptr);

		string strFileName = asString(bstrFileName);

		// Get the extension of the file
		string strExt = ::getExtensionFromFullPath( strFileName, true );
		
		// A string to hold one line of uncommented text read from the file
		string strFromFile = "";

		// If the file is an encrypted file
		if ( strExt == ".etf" )
		{
			// Define a encrypted file manager object to read encrypted file
			MapLabelManager encryptedFileManager;

			// Return the vector of unencrypted strings from the file
			vector<string> vecStrFromFile = encryptedFileManager.getMapLabel( strFileName );

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14592")
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14617")
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
		ASSERT_RESOURCE_ALLOCATION("ELI16264", ipVector != __nullptr);

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

		return S_OK;
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
		ASSERT_RESOURCE_ALLOCATION("ELI16614", ipVector != __nullptr);

		// Validate index
		long lCount = ipVector->Size();
		ASSERT_ARGUMENT("ELI16615", (nItemIndex >= 0 && nItemIndex < lCount));

		// Retrieve specified item
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipOWD = ipVector->At( nItemIndex );
		ASSERT_RESOURCE_ALLOCATION("ELI16616", ipOWD != __nullptr);

		// Provide Enabled setting
		*pvbEnabled = ipOWD->Enabled;

		return S_OK;
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
		ASSERT_RESOURCE_ALLOCATION("ELI16617", ipVector != __nullptr);

		// Validate index
		long lCount = ipVector->Size();
		ASSERT_ARGUMENT("ELI16618", (nItemIndex >= 0 && nItemIndex < lCount));

		// Retrieve specified item
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipOWD = ipVector->At( nItemIndex );
		ASSERT_RESOURCE_ALLOCATION("ELI16619", ipOWD != __nullptr);

		// Apply the Enabled setting
		ipOWD->Enabled = bEnabled;

		return S_OK;
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
		if (ipPersistStream == __nullptr)
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

		return S_OK;
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

		return S_OK;
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

		return S_OK;
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18163");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetObjectAsStringizedByteStream(IUnknown *pObject, BSTR *pbstrByteStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22016", pObject != __nullptr);
		ASSERT_ARGUMENT("ELI22022", pbstrByteStream != __nullptr);

		// ensure first that the object supports persistence
		IPersistStreamPtr ipPersistObj = pObject;
		if (ipPersistObj == __nullptr)
		{
			throw UCLIDException("ELI22017", "Object cannot be copied to byte stream "
				"because it does not support persistence!");
		}

		// create a temporary IStream object
		IStreamPtr ipStream;
		ipStream.Attach(SHCreateMemStream(__nullptr, 0));
		if (ipStream == __nullptr)
		{
			throw UCLIDException("ELI22018", "Unable to create stream object!");
		}

		// stream the object into the IStream
		writeObjectToStream(ipPersistObj, ipStream, "ELI22019", FALSE);

		// find the size of the stream
		LARGE_INTEGER zeroOffset;
		zeroOffset.QuadPart = 0;
		ULARGE_INTEGER length;
		ipStream->Seek(zeroOffset, STREAM_SEEK_END, &length);

		// Allocate memory to read stream
		unique_ptr<unsigned char[]> pszBuffer(new unsigned char[length.LowPart]);
		ASSERT_RESOURCE_ALLOCATION("ELI22020", pszBuffer.get() != __nullptr);
		memset(pszBuffer.get(), 0, length.LowPart);

		// copy the data in the stream to the buffer
		ipStream->Seek(zeroOffset, STREAM_SEEK_SET, NULL);
		ipStream->Read(pszBuffer.get(), length.LowPart, NULL);

		ByteStream byteStream(pszBuffer.get(), length.LowPart);
		string strTemp = byteStream.asString();

		// return the byte stream
		*pbstrByteStream = _bstr_t(strTemp.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22015");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetObjectFromStringizedByteStream(BSTR bstrByteStream, IUnknown **ppObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22024", ppObject != __nullptr);

		// create a byte stream from the BSTR
		ByteStream byteStream(asString(bstrByteStream));

		// create a temporary IStream object
		IStreamPtr ipStream;
		ipStream.Attach(SHCreateMemStream(__nullptr, 0));
		if (ipStream == __nullptr)
		{
			throw UCLIDException("ELI22025", "Unable to create stream object!");
		}

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

		return S_OK;
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
		ASSERT_ARGUMENT("ELI22713", bstrString != __nullptr);
		ASSERT_ARGUMENT("ELI22714", bstrSourceDocName != __nullptr);

		string strString = asString(bstrString);

		// Expand all functions and tags. This expands tags that are outside of any function's parameters as well
		string strExpanded = _textFunctionExpander.expandFunctions(
			strString, getThisAsCOMPtr(), bstrSourceDocName, __nullptr, 0);

		// Return the result
		*pbstrExpanded = _bstr_t(strExpanded.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22712");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetFunctionNames(IVariantVector** ppFunctionNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Get the available expansion functions
		vector<string> vecFunctions = _textFunctionExpander.getAvailableFunctions();
		
		// Populate an IVariantVector of BSTRs
		UCLID_COMUTILSLib::IVariantVectorPtr ipFunctions(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI35194", ipFunctions != __nullptr);
		for each (string function in vecFunctions)
		{
			ipFunctions->PushBack(function.c_str());
		}

		// Return the result
		*ppFunctionNames = (IVariantVector*) ipFunctions.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35195");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetFormattedFunctionNames(IVariantVector** ppFunctionNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Get the available expansion functions
		vector<string> vecFunctions = _textFunctionExpander.getAvailableFunctions();
		_textFunctionExpander.formatFunctions(vecFunctions);
		
		// Populate an IVariantVector of BSTRs
		UCLID_COMUTILSLib::IVariantVectorPtr ipFunctions(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI22717", ipFunctions != __nullptr);
		for each (string function in vecFunctions)
		{
			ipFunctions->PushBack(function.c_str());
		}

		// Return the result
		*ppFunctionNames = (IVariantVector*) ipFunctions.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22716");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetBuiltInTags(IVariantVector** ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI35196", ppTags != __nullptr);

		validateLicense();

		UCLID_COMUTILSLib::IVariantVectorPtr ipTags(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI35197", ipTags != __nullptr);

		ipTags->PushBack(get_bstr_t(strSOURCE_DOC_NAME_TAG));
		ipTags->PushBack(get_bstr_t(strCOMMON_COMPONENTS_DIR_TAG));

		// Report any programmatically added tags.
		CSingleLock lock(&m_criticalSectionAddedTags, TRUE);
		for (map<string, string>::iterator iter = m_mapAddedTags.begin();
			 iter != m_mapAddedTags.end();
			 iter++)
		{
			ipTags->PushBack(get_bstr_t(iter->first));
		}

		*ppTags = (IVariantVector *)ipTags.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35198");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetCustomFileTags(IVariantVector** ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI35199", ppTags != __nullptr);

		validateLicense();

		UCLID_COMUTILSLib::IVariantVectorPtr ipTags(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI35200", ipTags != __nullptr);

		*ppTags = (IVariantVector *)ipTags.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35201");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::EditCustomTags(long hParentWindow)
{
	try
	{
		// Check license
		validateLicense();

		// MiscUtils does not support custom tags; there is nothing to do.

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38061");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetAllTags(IVariantVector** ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI35202", ppTags != __nullptr);

		validateLicense();

		UCLID_COMUTILSLib::IVariantVectorPtr ipTags(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI35203", ipTags != __nullptr);

		*ppTags = (IVariantVector *)ipTags.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35204");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::ExpandTags(BSTR bstrInput, BSTR bstrSourceDocName, IUnknown *pData, VARIANT_BOOL vbStopEarly,
	BSTR* pbstrOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI35213", bstrInput != __nullptr);
		ASSERT_ARGUMENT("ELI35215", pbstrOutput != __nullptr);

		validateLicense();

		string strInput = asString(bstrInput);
		string strSourceDocName = asString(bstrSourceDocName);
		bool stopEarly = asCppBool(vbStopEarly);

		// Check if the source document name tag was found
		bool bSourceDocNameTagFound = strInput.find(strSOURCE_DOC_NAME_TAG) != string::npos;
		if (bSourceDocNameTagFound)
		{
			// Ensure a source document was specified
			if (strSourceDocName == "")
			{
				throw UCLIDException("ELI35205", "Cannot expand tag without source document.");
			}

			// Replace the source document tag
			replaceVariable(strInput, strSOURCE_DOC_NAME_TAG, strSourceDocName);
		}

		// Check if the common components directory tag was found
		bool bCommonComponentsDirFound = strInput.find(strCOMMON_COMPONENTS_DIR_TAG) != string::npos;
		if (bCommonComponentsDirFound)
		{
			const string strCommonComponentsDir = getModuleDirectory("BaseUtils.dll");

			// Replace the common components dir tag
			replaceVariable(strInput, strCOMMON_COMPONENTS_DIR_TAG, strCommonComponentsDir);
		}

		// Expand any programmatically added tags.
		CSingleLock lock(&m_criticalSectionAddedTags, TRUE);
		for (map<string, string>::iterator iter = m_mapAddedTags.begin();
			 iter != m_mapAddedTags.end();
			 iter++)
		{
			string strTag = iter->first;
			string strValue = iter->second;

			// Stop if the tag was replaced and stopEarly = true
			if (replaceVariable(strInput, strTag, strValue) && stopEarly)
			{
				break;
			}
		}

		*pbstrOutput = _bstr_t(strInput.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35206");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::ExpandTagsAndFunctions(BSTR bstrInput, BSTR bstrSourceDocName,
	IUnknown *pData, BSTR* pbstrOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI35216", bstrInput != __nullptr);
		ASSERT_ARGUMENT("ELI35218", pbstrOutput != __nullptr);

		validateLicense();

		UCLID_COMUTILSLib::ITagUtilityPtr ipTagUtility(this);
		ASSERT_RESOURCE_ALLOCATION("ELI35208", ipTagUtility != __nullptr);

		string strInput = asString(bstrInput);

		// Expand all functions and tags. This expands tags that are outside of any function's parameters as well
		string strOutput =
			_textFunctionExpander.expandFunctions(strInput, ipTagUtility, bstrSourceDocName, pData, 0);

		// Return the result
		*pbstrOutput = _bstr_t(strOutput.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35209");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::ExpandFunction(BSTR bstrFunctionName, IVariantVector *pArgs,
	BSTR bstrSourceDocName, IUnknown *pData, BSTR *pbstrOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI43490", pbstrOutput != __nullptr);

		*pbstrOutput = _bstr_t().Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43491");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::ExpandTagsAndFunctions(BSTR bstrInput,
	ITagUtility *pTagUtility, BSTR bstrSourceDocName, IUnknown *pData, BSTR* pbstrOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI35219", bstrInput != __nullptr);
		ASSERT_ARGUMENT("ELI35220", pTagUtility != __nullptr);
		ASSERT_ARGUMENT("ELI35222", pbstrOutput != __nullptr);

		validateLicense();

		UCLID_COMUTILSLib::ITagUtilityPtr ipTagUtility(pTagUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI35211", ipTagUtility != __nullptr);

		string strInput = asString(bstrInput);

		// Expand all functions and tags. This expands tags that are outside of any function's parameters as well
		string strOutput = _textFunctionExpander.expandFunctions(strInput, ipTagUtility, bstrSourceDocName, pData, 0);

		// Return the result
		*pbstrOutput = _bstr_t(strOutput.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35212");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::AddTag(BSTR bstrTagName, BSTR bstrTagValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36101", bstrTagName != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36103");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetAddedTags(IIUnknownVector **ppStringPairTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI43281", ppStringPairTags != __nullptr);

		UCLID_COMUTILSLib::IIUnknownVectorPtr ipStringPairTags(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI43282", ipStringPairTags != __nullptr);

		CSingleLock lock(&m_criticalSectionAddedTags, TRUE);

		for (auto iter = m_mapAddedTags.begin(); iter != m_mapAddedTags.end(); iter++)
		{
			UCLID_COMUTILSLib::IStringPairPtr ipStringPair(CLSID_StringPair);
			ipStringPair->StringKey = iter->first.c_str();
			ipStringPair->StringValue = iter->second.c_str();
			ipStringPairTags->PushBack(ipStringPair);
		}
		*ppStringPairTags = (IIUnknownVector*) ipStringPairTags.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43283");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::HasImageFileExtension(BSTR bstrFileName, VARIANT_BOOL* pvbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36708", pvbValue != __nullptr);

		validateLicense();

		string strFileName = asString(bstrFileName);
		string strExtension = getExtensionFromFullPath(strFileName);
		bool bIsImageExt = isImageFileExtension(strExtension);
		*pvbValue = asVariantBool(bIsImageExt);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36709");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::HasNumericFileExtension(BSTR bstrFileName, VARIANT_BOOL* pvbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36710", pvbValue != __nullptr);

		validateLicense();

		string strFileName = asString(bstrFileName);
		string strExtension = getExtensionFromFullPath(strFileName);
		bool bIsNumericExt = isNumericExtension(strExtension);
		*pvbValue = asVariantBool(bIsNumericExt);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36711");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::CreateExtractFileLock(BSTR bstrFileName, BSTR bstrContext, void **pLock)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI39257", pLock != nullptr);

		validateLicense();

		*pLock = new ExtractFileLock(asString(bstrFileName), true, asString(bstrContext));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39258");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::DeleteExtractFileLock(void *pLock)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI39259", pLock != nullptr);

		validateLicense();

		ExtractFileLock *pExtractFileLock = static_cast<ExtractFileLock*>(pLock);
		ASSERT_RUNTIME_CONDITION("ELI39260", pExtractFileLock->IsValid(),
			"Cannot release invalid file lock.");

		delete pExtractFileLock;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39261");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::IsExtractFileLockForFile(void *pLock, BSTR bstrFileName, 
												  VARIANT_BOOL *pbIsForFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI39262", pLock != nullptr);
		ASSERT_ARGUMENT("ELI39263", pbIsForFile != nullptr);

		validateLicense();

		ExtractFileLock *pExtractFileLock = static_cast<ExtractFileLock*>(pLock);
		ASSERT_RUNTIME_CONDITION("ELI39264", pExtractFileLock->IsValid(), "File lock is not valid.");

		*pbIsForFile = asVariantBool(pExtractFileLock->IsForFile(asString(bstrFileName)));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39265");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMiscUtils::GetNumbersFromRange(UINT uiRangeMax, BSTR bstrRangeSpec, /*[out, retval]*/ IVariantVector** ppNumbers)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI53945", ppNumbers != __nullptr);

		validateLicense();

		vector<size_t> vecNumbers = Range::getNumbers(uiRangeMax, asString(bstrRangeSpec));

		UCLID_COMUTILSLib::IVariantVectorPtr ipNumbers(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI53944", ipNumbers != __nullptr);

		for each (size_t ui in vecNumbers)
		{
			ipNumbers->PushBack(ui);
		}

		*ppNumbers = (IVariantVector *)ipNumbers.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI53943")
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
		if (pObject != __nullptr)
		{
			// Check to see if pObject is configurable via ISpecifyPropertyPages
			ISpecifyPropertyPagesPtr ipSpecifyPropertyPages(pObject);
			if (ipSpecifyPropertyPages != __nullptr)
			{
				bConfigurable = true;
			}
			// Check to see if pObject is configurable via IConfigurableObject
			else
			{
				UCLID_COMUTILSLib::IConfigurableObjectPtr ipConfigurableObject(pObject);
				if (ipConfigurableObject != __nullptr) 
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
	ASSERT_RESOURCE_ALLOCATION("ELI16973", ipThis != __nullptr);

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