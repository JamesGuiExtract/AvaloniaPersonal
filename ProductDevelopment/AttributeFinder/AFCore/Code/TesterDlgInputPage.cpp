// TesterDlgInputPage.cpp : implementation file
//

#include "stdafx.h"
#include "afcore.h"
#include "TesterDlgInputPage.h"
#include "TesterConfigMgr.h"
#include "msword9.h"
#include "AFInternalUtils.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <Win32Util.h>
#include <ComUtils.h>
#include <LicenseMgmt.h>

#include <io.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern const int giCONTROL_SPACING;

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// TesterDlgInputPage property page
//-------------------------------------------------------------------------------------------------
TesterDlgInputPage::TesterDlgInputPage()
: CPropertyPage(TesterDlgInputPage::IDD), 
  m_pTesterConfigMgr(NULL),
  m_ipOCREngine(NULL),
  m_strCurrentInputFile("")
{
	try
	{
		//{{AFX_DATA_INIT(TesterDlgInputPage)
			// NOTE: the ClassWizard will add member initialization here
		//}}AFX_DATA_INIT
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI06512")
}
//-------------------------------------------------------------------------------------------------
TesterDlgInputPage::~TesterDlgInputPage()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16304");
}
//-------------------------------------------------------------------------------------------------
void TesterDlgInputPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(TesterDlgInputPage)
	DDX_Control(pDX, IDC_COMBO_INPUT, m_comboInput);
	DDX_Control(pDX, IDC_EDIT_TESTINPUT, m_editInput);
	DDX_Control(pDX, IDC_EDIT_FILE, m_editFile);
	DDX_Control(pDX, IDC_CHECK_PERFORM_OCR, m_checkOCR);
	DDX_Control(pDX, ID_BROWSE, m_btnBrowse);
	DDX_Control(pDX, IDC_STATIC_INPUT, m_labelInput);
	DDX_Control(pDX, IDC_STATIC_FILENAME, m_labelFileName);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
void TesterDlgInputPage::setTesterConfigMgr(TesterConfigMgr *pTesterConfigMgr)
{
	m_pTesterConfigMgr = pTesterConfigMgr;
}
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TesterDlgInputPage, CPropertyPage)

BEGIN_MESSAGE_MAP(TesterDlgInputPage, CPropertyPage)
	//{{AFX_MSG_MAP(TesterDlgInputPage)
	ON_WM_SIZE()
	ON_BN_CLICKED(ID_BROWSE, OnBrowse)
	ON_CBN_SELCHANGE(IDC_COMBO_INPUT, OnSelchangeComboInput)
	ON_EN_KILLFOCUS(IDC_EDIT_FILE, OnKillFocusEditFile)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// TesterDlgInputPage message handlers
//-------------------------------------------------------------------------------------------------
void TesterDlgInputPage::resizeControls()
{
	// only do resizing if the controls have been initialized
	if (GetDlgItem(IDC_EDIT_TESTINPUT) != NULL)
	{
		// get the client coords of the dialog
		CRect rectDlg;
		GetClientRect(&rectDlg);
		
		// resize the input static label
		CRect rectInputLabel;
		m_labelInput.GetWindowRect(&rectInputLabel);
		ScreenToClient(&rectInputLabel);
		CRect rectCombo;
		m_comboInput.GetWindowRect(&rectCombo);
		ScreenToClient(&rectCombo);
		long nInputLabelWidth = rectInputLabel.Width();
		long nComboHeight = rectCombo.Height();
		rectInputLabel.left = giCONTROL_SPACING;
		rectInputLabel.top = giCONTROL_SPACING;
		rectInputLabel.right = rectInputLabel.left + nInputLabelWidth;
		rectInputLabel.bottom = rectInputLabel.top + nComboHeight;
		m_labelInput.MoveWindow(&rectInputLabel);

		// resize the combo box
		rectCombo.left = rectInputLabel.right + giCONTROL_SPACING;
		rectCombo.top = giCONTROL_SPACING;
		rectCombo.right = rectDlg.right - giCONTROL_SPACING;
		rectCombo.bottom = rectCombo.top + nComboHeight;
		m_comboInput.MoveWindow(&rectCombo);

		// the top of the edit control will be different
		// depending upon whether we are displaying the filename
		// related controls
		long nTopOfEditControl = 0;

		if (m_comboInput.GetCurSel() == m_comboInput.FindStringExact(-1,
			TesterConfigMgr::gstrTEXTFROMFILE.c_str()))
		{
			// resize the "select file (browse)" button
			CRect rectBrowseButton;
			m_btnBrowse.GetWindowRect(&rectBrowseButton);
			ScreenToClient(&rectBrowseButton);
			long nBrowseButtonWidth = rectBrowseButton.Width();
			long nBrowseButtonHeight = rectBrowseButton.Height();
			rectBrowseButton.right = rectDlg.right - giCONTROL_SPACING;
			rectBrowseButton.left = rectBrowseButton.right - nBrowseButtonWidth;
			rectBrowseButton.top = rectCombo.bottom + giCONTROL_SPACING;
			rectBrowseButton.bottom = rectBrowseButton.top + nBrowseButtonHeight;
			m_btnBrowse.MoveWindow(&rectBrowseButton);
			m_btnBrowse.ShowWindow(TRUE);

			// resize the filename static label
			CRect rectFileNameLabel;
			m_labelFileName.GetWindowRect(&rectFileNameLabel);
			ScreenToClient(&rectFileNameLabel);
			long nLabelHeight = rectFileNameLabel.Height();
			long nLabelWidth = rectFileNameLabel.Width();
			rectFileNameLabel.left = giCONTROL_SPACING;
			rectFileNameLabel.top = rectCombo.bottom + giCONTROL_SPACING;
			rectFileNameLabel.right = rectFileNameLabel.left + nLabelWidth;
			rectFileNameLabel.bottom = rectBrowseButton.bottom;
			m_labelFileName.MoveWindow(&rectFileNameLabel);
			m_labelFileName.ShowWindow(TRUE);

			// resize the filename editbox
			CRect rectFileName;
			m_editFile.GetWindowRect(&rectFileName);
			ScreenToClient(&rectFileName);
			long nFileNameHeight = rectFileName.Height();
			rectFileName.left = rectFileNameLabel.right + giCONTROL_SPACING;
			rectFileName.top = rectCombo.bottom + giCONTROL_SPACING;
			rectFileName.right = rectBrowseButton.left - giCONTROL_SPACING;
			rectFileName.bottom = rectFileName.top + nFileNameHeight;
			m_editFile.MoveWindow(&rectFileName);
			m_editFile.ShowWindow(TRUE);

			// resize the perform OCR checkbox
			CRect rectPerformOCR;
			m_checkOCR.GetWindowRect(&rectPerformOCR);
			ScreenToClient(&rectPerformOCR);
			long nPerformOCRHeight = rectPerformOCR.Height();
			long nPerformOCRWidth = rectPerformOCR.Width();
			rectPerformOCR.left = giCONTROL_SPACING;
			rectPerformOCR.top = rectFileName.bottom + giCONTROL_SPACING;
			rectPerformOCR.right = rectPerformOCR.left + nPerformOCRWidth;
			rectPerformOCR.bottom = rectPerformOCR.top + nPerformOCRHeight;
			m_checkOCR.MoveWindow(&rectPerformOCR);
			m_checkOCR.ShowWindow(TRUE);

			nTopOfEditControl = rectPerformOCR.bottom + giCONTROL_SPACING;
		}
		else
		{
			// there are no filename related controls.  The edit
			// box appears directly underneath the combo box
			m_checkOCR.ShowWindow(FALSE);
			m_labelFileName.ShowWindow(FALSE);
			m_editFile.ShowWindow(FALSE);
			m_btnBrowse.ShowWindow(FALSE);
			nTopOfEditControl = rectCombo.bottom + giCONTROL_SPACING;
		}

		// resize the input edit box 
		CRect rectEdit;
		m_editInput.GetWindowRect(&rectEdit);
		ScreenToClient(&rectEdit);
		rectEdit.left = giCONTROL_SPACING;
		rectEdit.top = nTopOfEditControl;
		rectEdit.right = rectDlg.right - giCONTROL_SPACING;
		rectEdit.bottom = rectDlg.bottom - giCONTROL_SPACING;
		m_editInput.MoveWindow(&rectEdit);
	}
}
//-------------------------------------------------------------------------------------------------
void TesterDlgInputPage::OnSize(UINT nType, int cx, int cy) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// call the base class method
	CPropertyPage::OnSize(nType, cx, cy);

	// do not continue if the configuration mgr has not been set
	if (!m_pTesterConfigMgr)
	{
		UCLIDException("ELI05223", "Internal coding error!").display();
		return;
	}

	// resize the controls
	resizeControls();
}
//-------------------------------------------------------------------------------------------------
void TesterDlgInputPage::OnBrowse() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		/////////////////////////////
		// Prepare a File Open dialog
		/////////////////////////////
		
		// Create the filters string with all the supported extensions
		char pszStrFilter[] = 
			"All Files (*.*)|*.*"
			"|UCLID Spatial String Files (*.uss)|*.uss"
			"|Text Files (*.txt)|*.txt"
			"|TIF Image Files (*.tif)|*.tif;*.tiff"
			"|Microsoft Word Documents (*.doc)|*.doc"
			"|Works 4.0 for Windows (*.wps)|*.wps"
			"|Word Perfect 5.x Documents (*.doc)|*.doc"
			"|Word Perfect 6.x Documents (*.wpd;*.doc)|*.wpd;*.doc"
			"|Rich Text Format (*.rtf)|*.rtf"
			"|Microsoft Word for Macintosh (*.mcw)|*.mcw||";
		
		// Create a buffer pszData to receive the key word of Word.Application from the registry.
		LONG lSize = 128;
		char pszData [128];
		
		// Query registry whether key word of Word Application exists or not
		LONG lRet = RegQueryValue( HKEY_CLASSES_ROOT, "Word.Application", pszData, 
			&lSize );
		
		if (lRet != ERROR_SUCCESS)
		{
			// Microsoft Word is not installed, change the filters to show in the FileDialog
			// to open only text, tif and uss files
			strcpy_s( (char*) pszStrFilter, sizeof(pszData), 
				"All Files (*.*)|*.*"
				"|UCLID Spatial String Files (*.uss)|*.uss"
				"|Text Files (*.txt)|*.txt"
				"|TIF Image Files (*.tif)|*.tif;*.tiff||" );
		}
	
		// Show the file dialog to select the file to be opened
		string strTemp = m_pTesterConfigMgr->getLastFileName();
		CFileDialog fileDlg( TRUE, NULL, strTemp.c_str(), OFN_READONLY | OFN_HIDEREADONLY, 
			pszStrFilter, this );
			
		// Modify the initial directory
		strTemp = m_pTesterConfigMgr->getLastFileOpenDirectory();
		fileDlg.m_ofn.lpstrInitialDir = strTemp.c_str();
		
		if (fileDlg.DoModal() == IDOK)
		{
			// what we want to do with the selected file is the same
			// as what we would want to do if that file was dragged-and-dropped
			// into this window - so just call that method to prevent
			// code duplication
			inputFileChanged((LPCTSTR) fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04890")
}
//-------------------------------------------------------------------------------------------------
BOOL TesterDlgInputPage::OnInitDialog() 
{
	try
	{
		CPropertyPage::OnInitDialog();
	
		// By default set the Perform OCR checkbox to checked
		m_checkOCR.SetCheck(BST_CHECKED);

		// Add items to combo box
		m_comboInput.AddString( TesterConfigMgr::gstrTEXTFROMIMAGEWINDOW.c_str() );
		m_comboInput.AddString( TesterConfigMgr::gstrTEXTFROMFILE.c_str() );
		m_comboInput.AddString( TesterConfigMgr::gstrMANUALTEXT.c_str() );
		
		///////////////////////////
		// Retrieve persistent Input type - for combo box
		///////////////////////////
		m_strCurrentInputType = m_pTesterConfigMgr->getLastInputType();
		m_comboInput.SelectString(-1, m_strCurrentInputType.c_str());
		OnSelchangeComboInput();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05248")

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void TesterDlgInputPage::OnSelchangeComboInput() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Retrieve input text
		CString	zInputText;
		m_editInput.GetWindowText(zInputText);

		// Retrieve the "new" input type
		CString zNewInputType;
		m_comboInput.GetLBText(m_comboInput.GetCurSel(), zNewInputType);
		string strNewInputType = (LPCTSTR) zNewInputType;

		// Update the cache of input data for previous input type
		// as long as the edit box is currently editable (in which case
		// the user may have changed the contents)
		if (!(m_editInput.GetStyle() & ES_READONLY) && 
			m_strCurrentInputType != strNewInputType)
		{
			ISpatialStringPtr ipText(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI06510", ipText != NULL);
			ipText->CreateNonSpatialString(get_bstr_t(zInputText), "");
			m_mapInputTypeToText[m_strCurrentInputType] = ipText;
		}

		// set the "new" input type as the current input type
		m_strCurrentInputType = strNewInputType;

		// Determine if text from file input is being used, but the specified file does not exist.
		bool bInvalidInputFileSpecified = (m_strCurrentInputFile.empty()  &&
			m_strCurrentInputType == TesterConfigMgr::gstrTEXTFROMFILE);

		// restore the last text stored for the new input type
		// if there is no entry in the map of input type to text for the
		// currently selected, input type, then associate an empty string
		// with the specified input type
		if (m_mapInputTypeToText.find(m_strCurrentInputType) == 
			m_mapInputTypeToText.end())
		{
			ISpatialStringPtr ipText(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI06799", ipText != NULL);
			m_mapInputTypeToText[m_strCurrentInputType] = ipText;
		}

		// get the spatial text associated with the current input type
		// if our logic is current, the pointer should never be NULL
		ISpatialStringPtr ipSpatialText = m_mapInputTypeToText[m_strCurrentInputType];
		if (ipSpatialText == NULL)
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI06800");
		}

		// set the text of the edit box to the appropriate content
		_bstr_t _bstrText = ipSpatialText->String;
		string strText = _bstrText;
		m_editInput.SetWindowText(strText.c_str());

		// The text input box should be read only if ipSpatialText has spatial info or if file input
		// is selected but the specified file does not exist.
		BOOL bMakeTextReadonly =
			asMFCBool(bInvalidInputFileSpecified || asCppBool(ipSpatialText->HasSpatialInfo()));

		if (bInvalidInputFileSpecified)
		{
			// Indicate that the specified file cannot be found
			m_editInput.SetWindowText("[File not found]");
		}

		// if the text associated with the current input type is spatial then
		// make the edit box into a readonly editbox
		m_editInput.SetReadOnly(bMakeTextReadonly);

		// remember the last used input type
		m_pTesterConfigMgr->setLastInputType(m_strCurrentInputType);
		
		// Move input controls
		resizeControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04889")
}
//--------------------------------------------------------------------------------------------------
void TesterDlgInputPage::OnKillFocusEditFile()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		inputFileChanged();		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29888")
}

//-------------------------------------------------------------------------------------------------
// Private / helper methods
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr TesterDlgInputPage::openFile(const string& strFileName) 
{
	// Get the length of the file path
	int iLength = strFileName.length();
	
	// If no file path, return
	if (iLength == 0)
	{
		// Create and throw exception
		UCLIDException ue( "ELI04861", "Could not open file, path is empty!" );
		ue.addDebugInfo( "Path", strFileName );
		throw ue;
	}

	// create the spatial string to be returned
	ISpatialStringPtr ipText(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI06801", ipText != NULL);

	// Get the extension of the file to be opened.
	string strExtension = getExtensionFromFullPath(strFileName);
	makeUpperCase(strExtension);
	
	// If the extension is txt get the file into a buffer and set the 
	// text into the edit control
	if (strExtension == ".TXT" || strExtension == ".USS")
	{
		ipText->LoadFrom(get_bstr_t(strFileName.c_str()), VARIANT_FALSE);
		return ipText;
	}

	// NOTE: if the extension is 3 digits, then assume it's an image
	// (Many customers store tif images with a 3 digit extension where
	// the three digits represent the number of pages in the image)
	if (isImageFileExtension(strExtension ) || isThreeDigitExtension(strExtension))
	{
		// If perform OCR is checked, then try to either load the USS file or perform
		// the OCR on the image
		if (m_checkOCR.GetCheck() == BST_CHECKED)
		{
			// if the tif file already has an associated uss file then we will use that 
			// otherwise we will re ocr
			string strUssFileName = strFileName + ".uss";
			if (isFileOrFolderValid(strUssFileName))
			{
				ipText->LoadFrom(get_bstr_t(strUssFileName), VARIANT_FALSE);
			}
			else
			{
				ipText = getOCREngine()->RecognizeTextInImage(
					strFileName.c_str(), 1, -1, UCLID_RASTERANDOCRMGMTLib::kNoFilter, "", 
					UCLID_RASTERANDOCRMGMTLib::kRegistry, VARIANT_TRUE, NULL); 
			}
		}
		else
		{
			// Just return an empty non-spatial string
			ipText->CreateNonSpatialString("", strFileName.c_str());
		}

		return ipText;
	}

	// if we reached here, it's because the file does not have one of the
	// extensions we recognize, and therefore we're going to attempt opening
	// the file using Microsoft Word (which may or may not be installed on 
	// this machine)

	// If the file is not a text file, copy the contents through 
	// MS Word to the Clipboard
	
	// Modify example from Q220911
	_Application	oWord;
	Documents		oDocs;
	_Document		oDoc;
	Range			oRange;
	COleVariant		vtOptional( (long)DISP_E_PARAMNOTFOUND, VT_ERROR );
	COleVariant		vtFalse( (short)FALSE );
	COleVariant		vtFile( strFileName.c_str(), VT_BSTR );
		
	// Create an instance of Word
	if (!oWord.CreateDispatch("Word.Application")) 
	{
		// Create and throw exception
		UCLIDException ue( "ELI04862", 
			"Microsoft Word failed to start or is not installed on this machine!" );
		throw ue;
	} 
	else 
	{
		// Set the visible property
		oWord.SetVisible( FALSE );
		
		// Add a new document
		oDocs = oWord.GetDocuments();
		
		// Open the file
		oDoc = oDocs.Open( vtFile, vtOptional, vtOptional, vtOptional, 
			vtOptional, vtOptional, vtOptional, vtOptional, vtOptional, 
			vtOptional, vtOptional, vtOptional );
		
		// Create a Range object encompassing the entire text area
		oRange = oDoc.Range( vtOptional, vtOptional );
		
		// Copy the text to the Clipboard
		oRange.Copy();
		
		// get the text from the Clipboard
		if (IsClipboardFormatAvailable(CF_TEXT))
		{
			// get the clipboard data
			ClipboardOpenerCloser cb(this);
			GlobalMemoryHandler hMemory = GetClipboardData(CF_TEXT);		
			char *pBuffer = (char *) hMemory.getData();
			DWORD dwBufferLength = GlobalSize(hMemory);
			
			string strText(pBuffer, dwBufferLength);
			ipText->CreateNonSpatialString(strText.c_str(), "");
		}
		else
		{
			throw UCLIDException("ELI06802", "Unable to retrieve text from document!");
		}

		// Close the document and application
		oDoc.Close( vtFalse, vtOptional, vtOptional );
		oWord.Quit( vtFalse, vtOptional, vtOptional );
	}

	return ipText;
}
//-------------------------------------------------------------------------------------------------
void TesterDlgInputPage::notifyImageWindowInputReceived(
	ISpatialStringPtr ipSpatialText)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// store the new text associated with the image window
	m_mapInputTypeToText[m_pTesterConfigMgr->gstrTEXTFROMIMAGEWINDOW] = ipSpatialText;
	
	// set the current selection in the combo box to 
	// OCR text from image
	m_comboInput.SetCurSel(m_comboInput.FindStringExact(-1, 
		TesterConfigMgr::gstrTEXTFROMIMAGEWINDOW.c_str()));

	// notify combo box selection change
	OnSelchangeComboInput();
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr TesterDlgInputPage::getText()
{
	// if the input page has not even been opened yet, then
	// there is not text available
	if (GetDlgItem(IDC_EDIT_TESTINPUT) == NULL)
	{
		return NULL;
	}

	// If the input is a file, call inputFileChanged in case the filename has changed since the file
	// was last loaded.
	if (m_strCurrentInputType == TesterConfigMgr::gstrTEXTFROMFILE)
	{
		inputFileChanged();
	}
	// if the edit box is not-readonly, then the user
	// could have made some changes to it
	else if (!(m_editInput.GetStyle() & ES_READONLY))
	{
		// get the current text in the editbox
		CString	zInputText;
		m_editInput.GetWindowText(zInputText);

		// associate the text with the current input type
		ISpatialStringPtr ipText = m_mapInputTypeToText[m_strCurrentInputType];
		if (ipText == NULL)
		{
			ipText.CreateInstance(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI06812", ipText != NULL);
			m_mapInputTypeToText[m_strCurrentInputType] = ipText;
		}

		// If the string has spatial info then replace the text and make it hybrid
		if (ipText->HasSpatialInfo())
		{
			ipText->ReplaceAndDowngradeToHybrid(get_bstr_t(zInputText));
		}
		else
		{
			// Replace the text in the spatial string (the string is already
			// non-spatial so no downgrade will occur)
			ipText->ReplaceAndDowngradeToNonSpatial(get_bstr_t(zInputText));
		}
	}

	// get the spatial string associated with the current input type
	// or return a dumb-non-spatial string
	if (m_mapInputTypeToText.find(m_strCurrentInputType)
		!= m_mapInputTypeToText.end())
	{
		ISpatialStringPtr ipString = m_mapInputTypeToText[m_strCurrentInputType];
		ASSERT_RESOURCE_ALLOCATION("ELI06810", ipString != NULL);
		return ipString;
	}
	else
	{
		// create an empty string and return
		ISpatialStringPtr ipText(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI06549", ipText != NULL);

		return ipText;
	}
}
//-------------------------------------------------------------------------------------------------
void TesterDlgInputPage::inputFileChanged(string strFileName/* = */)
{
	try
	{
		CWaitCursor wait;

		ISpatialStringPtr ipSpatialString(CLSID_SpatialString);

		// If no filename is specified, use the text in m_editFile.
		if (strFileName.empty())
		{
			CString	zInputFile;
			m_editFile.GetWindowText(zInputFile);
			strFileName = (LPCTSTR)zInputFile;
		}

		// Trim the file name [FlexIDSCore #4456]
		strFileName = trim(strFileName, " ", " ");

		// If the filename has not changed since the last call, just return and use the current
		// text.
		if (strFileName == m_strCurrentInputFile)
		{
			return;
		}

		if (isValidFile(strFileName))
		{
			// Open the specified file 
			ipSpatialString = openFile(strFileName);
			m_strCurrentInputFile = strFileName;
		}
		else
		{
			m_strCurrentInputFile = "";
		}

		// Update the internal map 
		m_mapInputTypeToText[TesterConfigMgr::gstrTEXTFROMFILE] = ipSpatialString;

		// set the current combo box selection to input-from-file
		m_comboInput.SetCurSel(m_comboInput.FindStringExact(-1, 
			TesterConfigMgr::gstrTEXTFROMFILE.c_str()));

		// notify combo box selection change so that the current text
		// in the editbox get saved as appropriate
		OnSelchangeComboInput();

		// resize/reshow the appropriate controls
		resizeControls();

		// Get and display the selected file complete path
		m_editFile.SetWindowText(strFileName.c_str());

		// Extract and store the directory
		string	strDir = getDirectoryFromFullPath(strFileName);
		m_pTesterConfigMgr->setLastFileOpenDirectory(strDir);

		// Get and store the actual file name
		string	strFile = getFileNameFromFullPath(strFileName);
		m_pTesterConfigMgr->setLastFileName(strFile);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05251")
}
//-------------------------------------------------------------------------------------------------
IOCREnginePtr TesterDlgInputPage::getOCREngine()
{
	if(!m_ipOCREngine)
	{
		// create an instance of the OCR engine
		m_ipOCREngine.CreateInstance(CLSID_ScansoftOCR);
		ASSERT_RESOURCE_ALLOCATION("ELI09543", m_ipOCREngine != NULL);

		// initialize the private license
		IPrivateLicensedComponentPtr ipScansoftEngine = m_ipOCREngine;
		ASSERT_RESOURCE_ALLOCATION("ELI10565", ipScansoftEngine != NULL);
		ipScansoftEngine->InitPrivateLicense(LICENSE_MGMT_PASSWORD.c_str());

		// set this instance of the OCR engine as the default
		// OCR engine for the input manager
		IInputManagerSingletonPtr ipInputMgrSingleton(CLSID_InputManagerSingleton);
		ASSERT_RESOURCE_ALLOCATION("ELI19131", ipInputMgrSingleton != NULL);
		IInputManagerPtr ipInputManager = ipInputMgrSingleton->GetInstance();
		ASSERT_RESOURCE_ALLOCATION("ELI10566", ipInputMgrSingleton != NULL);
		ipInputManager->SetOCREngine(m_ipOCREngine);
	}

	return m_ipOCREngine;
}
