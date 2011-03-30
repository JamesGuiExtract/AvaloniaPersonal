//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2007 - 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ImageCleanupSettingsEditorDlg.cpp
//
// PURPOSE:	Implementation of the CImageCleanupSettingsEditorDlg class	
//
// NOTES:	
//
// AUTHORS:	Jeff Shergalis
//
//-------------------------------------------------------------------------------------------------

#include "stdafx.h"
#include "ImageCleanupSettingsEditor.h"
#include "ImageCleanupSettingsEditorDlg.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <RegistryPersistenceMgr.h>
#include <RegConstants.h>
#include <ICCategories.h>
#include <Win32Util.h>
#include <Misc.h>

#include <string>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const int giRUN_LIST_COLUMN = 0;
const int giCLEAN_OPERATIONS_LIST_COLUMN = 1;
const int giRUN_WIDTH = 50;
const int giAUTO_SAVE_TIMER_ID = 1042;
const int giAUTO_SAVE_FREQUENCY = 60 * 1000;

const std::string gstrICS_FILE_OPEN_TYPES = ".ics;.etf";
const std::string gstrICS_FILE_OPEN_FILTER =
	"Image cleanup settings files (*.ics;*.ics.etf)|*.ics;*.ics.etf|"
	"Image cleanup settings files (*.ics)|*.ics|"
	"Encrypted Image cleanup settings files (*.ics.etf)|*.ics.etf|"
	"All Files (*.*)|*.*||";

const std::string gstrICS_FILE_SAVE_TYPES = ".ics";
const std::string gstrICS_FILE_SAVE_FILTER =
	"Image Cleanup Settings files (*.ics)|*.ics|All Files (*.*)|*.*||";

const std::string gstrICS_FILE_TEST_FILTER =
	"All image files|*.bmp*;*.rle*;*.dib*;*.rst*;*.gp4*;*.mil*;*.cal*;*.cg4*;*.flc*;*.fli*;*.gif*;*.jpg*;*.pcx*;*.pct*;*.png*;*.tga*;*.tif*;*.pdf*|"
	"GDD files|*.gdd|"
	"BMP files (*.bmp;*.rle;*.dib)|*.bmp*;*.rle*;*.dib*|"
	"CALS1 files (*.rst;*.gp4;*.mil;*.cal;*.cg4)|*.rst*;*.gp4*;*.mil*;*.cal*;*.cg4*|"
	"FLIC files (*.flc;*.fli)|*.flc*;*.fli*|"
	"GIF files (*.gif)|*.gif*|"
	"JFIF files (*.jpg)|*.jpg*|"
	"PCX files (*.pcx)|*.pcx*|"
	"PICT files (*.pct)|*.pct*|"
	"PNG files (*.png)|*.png*|"
	"TGA files (*.tga)|*.tga*|"
	"TIFF files (*.tif)|*.tif*|"
	"PDF files (*.pdf)|*.pdf*|"
	"All files (*.*)|*.*||";

const std::string gstrIC_REG_ROOT_FOLDER_PATH = gstrREG_ROOT_KEY + std::string("\\ImageCleanup");
const std::string gstrIC_ICSETTINGS_KEY = "ICSettings";
const std::string gstrIC_ICSETTINGS_KEY_PATH = gstrIC_REG_ROOT_FOLDER_PATH + std::string("\\") +
		gstrIC_ICSETTINGS_KEY;

const std::string gstrRECOVERY_PROMPT =
	"It appears that you were unable to save your work from \n"
	"the previous session because of an application crash\n"
	"or other catastrophic failure.  Would you like to attempt\n"
	"recovering the Image Cleanup Settings from your previous session?\n";

const std::string gstrTESTING_DONE = "Image cleaning is now complete.";

const int gnRUN_SPOT_RECOGNITION = 0;
const int gnRUN_REGISTERED_PROGRAM = 1;

//--------------------------------------------------------------------------------------------------
// CAboutDlg dialog used for App About
//--------------------------------------------------------------------------------------------------
class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

// Implementation
protected:
	DECLARE_MESSAGE_MAP()
};

//--------------------------------------------------------------------------------------------------
// CAboutDlg
//--------------------------------------------------------------------------------------------------
CAboutDlg::CAboutDlg() : 
CDialog(CAboutDlg::IDD)
{
}
//--------------------------------------------------------------------------------------------------
void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
BOOL CAboutDlg::OnInitDialog()
{
	try
	{
		// Get module path and filename
		char zFileName[MAX_PATH];
		int ret = ::GetModuleFileName(NULL, zFileName, MAX_PATH);
		if (ret == 0)
		{
			throw UCLIDException("ELI17563", "Unable to retrieve module file name!");
		}

		// Retrieve the Version string
		string strVersion = "Image Cleanup Settings Editor Version ";
		strVersion += getFileVersion(string(zFileName));

		// set the version information
		SetDlgItemText(IDC_EDIT_VERSION, strVersion.c_str());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17240");

	return TRUE;
}

//--------------------------------------------------------------------------------------------------
// CImageCleanupSettingsEditorDlg dialog
//--------------------------------------------------------------------------------------------------
CImageCleanupSettingsEditorDlg::CImageCleanupSettingsEditorDlg(CWnd* pParent /*=NULL*/,
															   const string& rstrFileToOpen)
	: 
	CDialog(CImageCleanupSettingsEditorDlg::IDD, pParent),
	m_ipMiscUtils(NULL),
	m_ipSettings(NULL),
	m_strCurrentFileName(rstrFileToOpen),
	m_strBinFolder(""),
	m_strLastFileOpened(""),
	m_FRM(".tmp"),
	m_strImageViewerExePath(""),
	m_bDirty(false),
	m_pMRUFilesMenu(NULL)
{
	try
	{
		// load the icon
		m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

		// create the Configuration Manager
		ma_pUserCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_CURRENT_USER, gstrIC_ICSETTINGS_KEY_PATH));
		
		// create the MRU List
		ma_pMRUList = unique_ptr<MRUList>(new MRUList(ma_pUserCfgMgr.get(), 
			"\\ImageCleanupSettings\\MRUList", "File_%d", 5));

		// create an instance of the image cleanup settings object
		m_ipSettings.CreateInstance(CLSID_ImageCleanupSettings);
		ASSERT_RESOURCE_ALLOCATION("ELI17180", m_ipSettings != __nullptr);

		// create an instance of the clipboard object manager
		m_ipClipboardMgr.CreateInstance(CLSID_ClipboardObjectManager);
		ASSERT_RESOURCE_ALLOCATION("ELI17242", m_ipClipboardMgr != __nullptr);

		// check for recovery file
		string strRecoveryFileName;
		if (m_FRM.recoveryFileExists(strRecoveryFileName))
		{
			int iResult = MessageBox(gstrRECOVERY_PROMPT.c_str(), "Recovery", 
				MB_YESNO + MB_ICONQUESTION);

			if (iResult == IDYES)
			{
				// load the clean settings from the clean settings recovery file
				// NOTE: We are setting the bSetDirtyFlagToTrue flag to VARIANT_TRUE
				// so that the user is prompted for saving when they try
				// to close the Image Cleaning Settings Editor window
				m_ipSettings->LoadFrom(get_bstr_t(strRecoveryFileName), VARIANT_TRUE);
			}

			// at this point, regardless of whether the user decided to
			// recover the file or not, the recovery file should be deleted.
			// the recovery file can be deleted
			m_FRM.deleteRecoveryFile(strRecoveryFileName);
		}

		// get the current Bin folder
		m_strBinFolder = getCurrentProcessEXEDirectory();

		// trim trailing slash
		m_strBinFolder = trim(m_strBinFolder, "", "\\");

	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI17181");
}
//--------------------------------------------------------------------------------------------------
CImageCleanupSettingsEditorDlg::~CImageCleanupSettingsEditorDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17182");
}

//--------------------------------------------------------------------------------------------------
//  CImageCleanupSettingsEditorDlg message handlers
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_LIST_TASKS, m_lstOperationList);
	DDX_Control(pDX, IDC_BTN_ADD, m_btnAdd);
	DDX_Control(pDX, IDC_BTN_REMOVE, m_btnRemove);
	DDX_Control(pDX, IDC_BTN_CONFIG, m_btnConfigure);
	DDX_Control(pDX, IDC_BTN_UP, m_btnUp);
	DDX_Control(pDX, IDC_BTN_DN, m_btnDown);
	DDX_Control(pDX, IDC_RADIO_ALLPAGES, m_radioAllPages);
	DDX_Control(pDX, IDC_EDIT_FIRSTPAGES, m_editFirstPages);
	DDX_Control(pDX, IDC_EDIT_LASTPAGES, m_editLastPages);
	DDX_Control(pDX, IDC_EDIT_SPECIFIEDPAGES, m_editSpecifiedPages);
	DDX_Control(pDX, IDC_EDIT_TEST_FILE_NAME, m_editInFile);
	DDX_Control(pDX, IDC_EDIT_TEST_OUT_FILE, m_editOutFile);
	DDX_Control(pDX, IDC_CHECK_OUT_FILE, m_chkOutFile);
	DDX_Control(pDX, IDC_BTN_OPEN_IN_IMAGE, m_btnOpenInFile);
	DDX_Control(pDX, IDC_BTN_OPEN_OUT_IMAGE, m_btnOpenOutFile);
	DDX_Control(pDX, IDC_BTN_TEST, m_btnTest);
	DDX_Control(pDX, IDC_RADIO_EXTRACT, m_radioExtract);
	DDX_Control(pDX, IDC_BTN_BROWSE_TEST_OUT_FILE, m_btnBrowseOutFile);
	DDX_Control(pDX, IDC_CHECK_OVERWRITE, m_chkOverwrite);
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CImageCleanupSettingsEditorDlg, CDialog)
	ON_COMMAND(ID_FILE_EXIT, &CImageCleanupSettingsEditorDlg::OnFileExit)
	ON_COMMAND(ID_FILE_NEW_SETTINGS, &CImageCleanupSettingsEditorDlg::OnFileNew)
	ON_COMMAND(ID_FILE_OPEN_SETTINGS, &CImageCleanupSettingsEditorDlg::OnFileOpen)
	ON_COMMAND(ID_FILE_SAVE_SETTINGS, &CImageCleanupSettingsEditorDlg::OnFileSave)
	ON_COMMAND(ID_FILE_SAVEAS, &CImageCleanupSettingsEditorDlg::OnFileSaveas)
	ON_COMMAND(ID_HELP_ABOUT, &CImageCleanupSettingsEditorDlg::OnHelpAbout)
	ON_COMMAND(ID_TOOLS_CHECKFORNEWCOMPONENTS, &CImageCleanupSettingsEditorDlg::OnToolsCheckForNewComponents)
	ON_COMMAND_RANGE(ID_MRU_FILE1, ID_MRU_FILE5, &CImageCleanupSettingsEditorDlg::OnSelectMRUMenu)
	ON_COMMAND(ID_EDIT_CUT, &CImageCleanupSettingsEditorDlg::OnEditCut)
	ON_COMMAND(ID_EDIT_COPY, &CImageCleanupSettingsEditorDlg::OnEditCopy)
	ON_COMMAND(ID_EDIT_PASTE, &CImageCleanupSettingsEditorDlg::OnEditPaste)
	ON_COMMAND(ID_EDIT_DELETE, &CImageCleanupSettingsEditorDlg::OnEditDelete)
	ON_COMMAND(IDC_RADIO_ALLPAGES, &CImageCleanupSettingsEditorDlg::OnRadioPagesClicked)
	ON_COMMAND(IDC_RADIO_FIRSTPAGES, &CImageCleanupSettingsEditorDlg::OnRadioPagesClicked)
	ON_COMMAND(IDC_RADIO_LASTPAGES, &CImageCleanupSettingsEditorDlg::OnRadioPagesClicked)
	ON_COMMAND(IDC_RADIO_SPECIFIEDPAGES, &CImageCleanupSettingsEditorDlg::OnRadioPagesClicked)
	ON_BN_CLICKED(IDC_BTN_BROWSE_TEST_FILE_NAME, &CImageCleanupSettingsEditorDlg::OnBtnBrowseInFile)
	ON_BN_CLICKED(IDC_BTN_BROWSE_TEST_OUT_FILE, &CImageCleanupSettingsEditorDlg::OnBtnBrowseOutFile)
	ON_BN_CLICKED(IDC_BTN_TEST, &CImageCleanupSettingsEditorDlg::OnBtnTest)
	ON_BN_CLICKED(IDC_BTN_OPEN_IN_IMAGE, &CImageCleanupSettingsEditorDlg::OnBtnOpenInImage)
	ON_BN_CLICKED(IDC_BTN_OPEN_OUT_IMAGE, &CImageCleanupSettingsEditorDlg::OnBtnOpenOutImage)
	ON_BN_CLICKED(IDC_BTN_ADD, OnBtnAdd)
	ON_BN_CLICKED(IDC_BTN_REMOVE, &CImageCleanupSettingsEditorDlg::OnBtnRemove)
	ON_BN_CLICKED(IDC_BTN_CONFIG, &CImageCleanupSettingsEditorDlg::OnBtnConfig)
	ON_BN_CLICKED(IDC_BTN_DN, &CImageCleanupSettingsEditorDlg::OnBtnDown)
	ON_BN_CLICKED(IDC_BTN_UP, &CImageCleanupSettingsEditorDlg::OnBtnUp)
	ON_BN_CLICKED(IDC_CHECK_OUT_FILE, &CImageCleanupSettingsEditorDlg::OnClickOverideCheck)
	ON_EN_CHANGE(IDC_EDIT_TEST_FILE_NAME, &CImageCleanupSettingsEditorDlg::OnChangeTestInFileName)
	ON_EN_CHANGE(IDC_EDIT_TEST_OUT_FILE, &CImageCleanupSettingsEditorDlg::OnChangeTestOutFileName)
	ON_EN_CHANGE(IDC_EDIT_FIRSTPAGES, &CImageCleanupSettingsEditorDlg::OnChangeRangeText)
	ON_EN_CHANGE(IDC_EDIT_LASTPAGES, &CImageCleanupSettingsEditorDlg::OnChangeRangeText)
	ON_EN_CHANGE(IDC_EDIT_SPECIFIEDPAGES, &CImageCleanupSettingsEditorDlg::OnChangeRangeText)
	ON_WM_CLOSE()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_TIMER()
	ON_WM_DROPFILES()
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_TASKS, &CImageCleanupSettingsEditorDlg::OnLvnItemchangedListTsk)
	ON_NOTIFY(NM_DBLCLK, IDC_LIST_TASKS, &CImageCleanupSettingsEditorDlg::OnDblclkListTasks)
	//}}AFX_MSG_MAP
	ON_NOTIFY(NM_RCLICK, IDC_LIST_TASKS, &CImageCleanupSettingsEditorDlg::OnRightClickTasks)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
BOOL CImageCleanupSettingsEditorDlg::PreTranslateMessage(MSG *pMsg)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		if (pMsg->message == WM_KEYDOWN)
		{
			// translate accelerators
			static HACCEL hAccel = LoadAccelerators(AfxGetApp()->m_hInstance, 
				MAKEINTRESOURCE(IDR_ACCELERATORS));
			if (TranslateAccelerator(m_hWnd, hAccel, pMsg))
			{
				// since the message has been handled, no further dispatch is needed
				return TRUE;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19904");

	return CDialog::PreTranslateMessage(pMsg);
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnCancel()
{
	// purpose of having this function here is to prevent the user from closing the
	// dialog by pressing the Escape key
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnOK()
{
	// purpose of having this function here is to prevent the user from closing the
	// dialog by pressing the Enter key
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnBtnAdd()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// create a new ObjectWithDescription
		IObjectWithDescriptionPtr ipObject(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI17183", ipObject != __nullptr);

		// allow the user to select and configure
		VARIANT_BOOL vbDirty = getMiscUtils()->AllowUserToSelectAndConfigureObject(ipObject, 
			"Task",	get_bstr_t(ICO_CLEANING_OPERATIONS_CATEGORYNAME), VARIANT_FALSE, 0, NULL);

		// check OK was selected
		if (vbDirty)
		{
			// set the dirty flag for the menu update
			m_bDirty = true;

			// Get index of previously selected Image Cleanup Operation
			int iIndex = -1;
			POSITION pos = m_lstOperationList.GetFirstSelectedItemPosition();
			if (pos != __nullptr)
			{
				// Get index of first selection
				iIndex = m_lstOperationList.GetNextSelectedItem( pos );
			}

			// If no current selection, insert item at end of list
			if (iIndex == -1)
			{
				iIndex = m_lstOperationList.GetItemCount();
			}

			// Insert the object-with-description into the vector and refresh the list
			getImageCleanupOperations()->Insert( iIndex, ipObject );
			refresh();

			// clear the previous selection if any
			clearListSelection();

			// Retain selection and focus
			m_lstOperationList.SetItemState( iIndex, LVIS_SELECTED, LVIS_SELECTED );
			m_lstOperationList.SetFocus();

			// Update the display
			UpdateData( FALSE );

			// Update button and menu states
			setButtonStates();
			setMenuStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17184")
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnBtnRemove() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Check for current selection
		int iIndex = -1;
		POSITION pos = m_lstOperationList.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			iIndex = m_lstOperationList.GetNextSelectedItem( pos );
		}
		else
		{
			// if pos is NULL, nothing is selected, no work to do
			return;
		}

		// Check for multiple selection
		int iIndex2 = m_lstOperationList.GetNextSelectedItem( pos );

		// Handle single-selection case
		int		iResult;
		CString	zPrompt;
		if (iIndex2 == -1)
		{
			// Retrieve current cleanup operation description
			CString	zDescription = m_lstOperationList.GetItemText( iIndex, 1 );

			// Create prompt for confirmation
			zPrompt.Format( "Are you sure that task '%s' should be deleted?", 
				zDescription );
		}
		// Handle multiple-selection case
		else
		{
			// Create prompt for confirmation
			zPrompt.Format( "Are you sure that the selected tasks should be deleted?" );
		}

		// Present MessageBox
		iResult = MessageBox( LPCTSTR(zPrompt), "Confirm Delete", 
			MB_YESNO | MB_ICONQUESTION );

		// Act on response
		if (iResult == IDYES)
		{
			// Delete the marked cleanup operations
			deleteSelectedTasks();

			// Select the next (or the last) cleanup operation
			int iCount = m_lstOperationList.GetItemCount();
			if (iCount <= iIndex)
			{
				iIndex = iCount - 1;
			}
		}

		// made a change, set dirty flag
		m_bDirty = true;

		// Retain selection and focus
		m_lstOperationList.SetItemState( iIndex, LVIS_SELECTED, LVIS_SELECTED );
		m_lstOperationList.SetFocus();

		// Refresh the display
		UpdateData( TRUE );

		// Update button and menu states
		setButtonStates();
		setMenuStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17185")
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnBtnConfig() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Check for current cleanup operation selection
		int iIndex = -1;
		POSITION pos = m_lstOperationList.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			iIndex = m_lstOperationList.GetNextSelectedItem( pos );
		}
		else
		{
			// if pos is NULL then nothing is selected thus nothing to do
			return;
		}

		// get the current cleanup operation
		IObjectWithDescriptionPtr	ipCO = getImageCleanupOperations()->At( iIndex );
		ASSERT_RESOURCE_ALLOCATION("ELI17186", ipCO != __nullptr);
		
		// get the position and dimensions of the command button
		RECT rectCommandButton;
		getDlgItemWindowRect(IDC_BTN_CONFIG, rectCommandButton);

		// allow the user to modify the cleanup operation
		VARIANT_BOOL vbDirty = getMiscUtils()->HandlePlugInObjectCommandButtonClick(ipCO, 
			"Task",	get_bstr_t(ICO_CLEANING_OPERATIONS_CATEGORYNAME), VARIANT_FALSE, 0, NULL, 
			rectCommandButton.right, rectCommandButton.top);

		// Check result
		if (vbDirty == VARIANT_TRUE)
		{
			// update the cleanup operation list box
			replaceCleanupOperationAt(iIndex, ipCO);

			// made a change, set dirty flag
			m_bDirty = true;

			// update menus
			setMenuStates();
		}

		// Retain selection and focus
		m_lstOperationList.SetItemState( iIndex, LVIS_SELECTED, LVIS_SELECTED );
		m_lstOperationList.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17187")
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnBtnDown()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{	
		// Check for current cleanup operation selection
		int iIndex = -1;
		POSITION pos = m_lstOperationList.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			iIndex = m_lstOperationList.GetNextSelectedItem( pos );
		}
		else
		{
			// if pos is NULL then nothing selected, nothing to do
			return;
		}

		// Selection cannot be at bottom of list
		int iCount = m_lstOperationList.GetItemCount();
		if (iIndex < iCount - 1)
		{
			// Update the cleanup operation vector and refresh the list
			getImageCleanupOperations()->Swap( iIndex, iIndex + 1 );
			refresh();

			// Retain selection and focus
			m_lstOperationList.SetItemState( iIndex + 1, LVIS_SELECTED, LVIS_SELECTED );
			m_lstOperationList.SetFocus();

			// we made a change so set the dirty flag
			m_bDirty = true;

			// Update button and menu states
			setButtonStates();
			setMenuStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17188")
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnBtnUp() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{	
		// Check for current cleanup operation selection
		int iIndex = -1;
		POSITION pos = m_lstOperationList.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			iIndex = m_lstOperationList.GetNextSelectedItem( pos );
		}
		else
		{
			// nothing selected so nothing to do
			return;
		}

		// Selection cannot be at top of list
		if (iIndex > 0)
		{
			// Update the cleanup operation vector and refresh the list
			getImageCleanupOperations()->Swap( iIndex, iIndex - 1 );
			refresh();

			// Retain selection and focus
			m_lstOperationList.SetItemState( iIndex - 1, LVIS_SELECTED, LVIS_SELECTED );
			m_lstOperationList.SetFocus();

			// set the dirty flag
			m_bDirty = true;

			// Update button and menu states
			setButtonStates();
			setMenuStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17189")
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnClose()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	OnFileExit();
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnFileExit()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// if checkModification() returns false, it means the user
		// want to cancel current task, i.e. creating a new file
		if (!checkModification())
		{
			return;
		}

		// clear the clipboard manager
		m_ipClipboardMgr->Clear();

		// if we are exiting with the users request then delete the recovery file
		m_FRM.deleteRecoveryFile();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17190");
	
	CDialog::OnClose();

	CDialog::OnCancel();
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnFileNew() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// if checkModification() returns false, it means the user
		// want to cancel current task, i.e. creating a new file
		if (!checkModification())
		{
			return;
		}

		// set the dirty flag
		m_bDirty = false;

		// reset the Image Cleanup Settings
		m_ipSettings->Clear();

		// refresh the UI
		refreshUIFromImageCleanupSettings();

		// set scope of cleanup operations
		setScopeOfCleanupOperations();

		// A new file doesn't have a name yet
		m_strCurrentFileName = "";
		
		// update window caption
		updateWindowCaption();

		// Delete recovery file - user has started a new file
		m_FRM.deleteRecoveryFile();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17192")
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnFileOpen() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// call openFile with "" argument to indicate that we want the file-open
		// dialog box to be displayed.
		openFile("");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17193")
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnFileSave()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{	
		if (m_strCurrentFileName.empty())
		{
			// if current image cleanup settings haven't been saved yet.
			OnFileSaveas();
		}
		else 
		{
			bool bFileReadOnly = isFileReadOnly( m_strCurrentFileName );
			try
			{
				if (bFileReadOnly)
				{
					UCLIDException ue("ELI17194", "File is read only!");
					ue.addDebugInfo("FileName", m_strCurrentFileName);
					throw ue;
				}
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17195");
			if (bFileReadOnly)
			{
				OnFileSaveas();
			}
			else
			{
				// store the appropriate page range settings to the ImageCleanupSettings object
				if (!storePageRangeSettings())
				{
					// storing settings failed, return without saving
					return;
				}

				// save the settings to the specified file
				m_ipSettings->SaveTo(m_strCurrentFileName.c_str(), VARIANT_TRUE);

				m_bDirty = false;
				setMenuStates();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17196")
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnFileSaveas() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{	
		// store the page range settings to the ImageCleanupSettings Object
		if (!storePageRangeSettings())
		{
			// failed storing page range settings return without saving file
			return;
		}
		
		// ask user to select file to save to
		CFileDialog fileDlg(FALSE, gstrICS_FILE_SAVE_TYPES.c_str(), NULL, 
			OFN_ENABLESIZING | OFN_EXPLORER | OFN_HIDEREADONLY | OFN_PATHMUSTEXIST 
			| OFN_OVERWRITEPROMPT, gstrICS_FILE_SAVE_FILTER.c_str(), this);

		string strDir = getDirectoryFromFullPath(m_strLastFileOpened) + "\\";
		fileDlg.m_ofn.lpstrInitialDir = strDir.c_str();
			
		if (fileDlg.DoModal() != IDOK)
		{
			return;
		}

		// Save the image cleanup settings object to the specified file
		_bstr_t bstrFileName = fileDlg.GetPathName();
		m_ipSettings->SaveTo(bstrFileName, VARIANT_TRUE);

		// update the caption of the window to contain the filename
		m_strLastFileOpened = asString(bstrFileName);
		m_strCurrentFileName = m_strLastFileOpened;
		updateWindowCaption();

		// save to MRU list
		addFileToMRUList(m_strLastFileOpened);

		m_bDirty = false;

		setMenuStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17197")
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnHelpAbout() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// create dialog box
		CAboutDlg dlgAbout;
		
		// display dialog box
		dlgAbout.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17198");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnEditCut() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// First copy the item to the Clipboard
		OnEditCopy();

		// Delete the item
		deleteSelectedTasks();

		// Update button and menu states
		setButtonStates();
		setMenuStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17243")
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnEditCopy() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Check for current operation selection
		int iIndex = -1;
		POSITION pos = m_lstOperationList.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			iIndex = m_lstOperationList.GetNextSelectedItem( pos );
		}

		if (iIndex == -1)
		{
			// Throw exception
			throw UCLIDException( "ELI17246", 
				"Unable to determine selected Cleanup operation!" );
		}

		// Retrieve vector of existing operations
		IIUnknownVectorPtr	ipOperations = getImageCleanupOperations();
		ASSERT_RESOURCE_ALLOCATION("ELI17247", ipOperations != __nullptr);

		// Create a vector for selected operations
		IIUnknownVectorPtr	ipCopiedOperations( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION( "ELI17248", ipCopiedOperations != __nullptr );

		// Add each selected operation to vector
		while (iIndex != -1)
		{
			// Retrieve the selected image cleanup operation
			IUnknownPtr	ipObject = ipOperations->At(iIndex);
			ASSERT_RESOURCE_ALLOCATION( "ELI17249", ipObject != __nullptr );

			// Add the operation to the vector
			ipCopiedOperations->PushBack( ipObject );

			// Get the next selection
			iIndex = m_lstOperationList.GetNextSelectedItem( pos );
		}

		// ClipboardManager will handle the Copy
		m_ipClipboardMgr->CopyObjectToClipboard( ipCopiedOperations );
  	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17244")
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnEditPaste() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Test ClipboardManager object to see if it is a vector of Objects with description
		IUnknownPtr	ipObject( NULL );
		if (m_ipClipboardMgr->ObjectIsIUnknownVectorOfType(IID_IObjectWithDescription))
		{
			// Retrieve object from ClipboardManager
			ipObject = m_ipClipboardMgr->GetObjectInClipboard();
			ASSERT_RESOURCE_ALLOCATION("ELI17250", ipObject != __nullptr);
		}
		else
		{
			// Throw exception, object is not a vector of ObjectsWithDescription
			throw UCLIDException("ELI17251", 
				"Clipboard object is not a vector of Cleanup Operations!" );
		}

		// Check for current operation selection
		int iIndex = -1;
		POSITION pos = m_lstOperationList.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			iIndex = m_lstOperationList.GetNextSelectedItem( pos );
		}

		// Check for item count if no selection
		if (iIndex == -1)
		{
			iIndex = m_lstOperationList.GetItemCount();
		}

		// Retrieve vector of existing operations
		IIUnknownVectorPtr	ipOperations = getImageCleanupOperations(); 
		ASSERT_RESOURCE_ALLOCATION("ELI17252", ipOperations != __nullptr);

		// Get count of Operations in Clipboard vector
		IIUnknownVectorPtr	ipPastedOperations = ipObject;
		ASSERT_RESOURCE_ALLOCATION( "ELI17253", ipPastedOperations != __nullptr );
		int iCount = ipPastedOperations->Size();

		// clear selections
		clearListSelection();

		// Add each Operation to the the vector in reverse order
		for (int i = iCount-1; i >= 0; i--)
		{
			// Retrieve the operation
			IObjectWithDescriptionPtr ipNewOperation = ipPastedOperations->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI17254", ipNewOperation != __nullptr);

			// insert the operation into the vector
			getImageCleanupOperations()->Insert(iIndex, ipNewOperation);
		}

		// refresh the list
		refresh();

		// select the new items
		for (int i = 0; i < iCount; i++)
		{
			m_lstOperationList.SetItemState(iIndex+i, LVIS_SELECTED, LVIS_SELECTED);
		}

		// set focus on the list
		m_lstOperationList.SetFocus();

		// Update button and menu states
		setButtonStates();
		setMenuStates();

		// Refresh the display
		UpdateData( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17245")
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnEditDelete() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// do the same thing as the remove button
		OnBtnRemove();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17255")
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnToolsCheckForNewComponents()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// check for new components
		updateComponentCacheFile();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17975");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnBtnBrowseInFile()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// create a dialog box that will allow the user to browse for an image file
		CFileDialog dlg(TRUE, NULL, NULL, 
			OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			gstrICS_FILE_TEST_FILTER.c_str(), NULL);

		// display the file dialog
		if (dlg.DoModal() == IDOK)
		{
			CString zInImageFile = dlg.GetPathName();
			m_editInFile.SetWindowText(zInImageFile);

			// set the new input file name
			m_strInImageFile = zInImageFile;

			// check if user is overriding the output directory
			if (m_chkOutFile.GetCheck() == BST_UNCHECKED)
			{
				// user is using default output file name so set the output file name
				// with the new output file
				m_strOutImageFile = getCleanImageName(m_strInImageFile);
				m_editOutFile.SetWindowText(m_strOutImageFile.c_str());
			}

			// need to update the button states
			setButtonStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17319");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnBtnBrowseOutFile()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CFileDialog dlg(TRUE, NULL, NULL, OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			gstrICS_FILE_TEST_FILTER.c_str(), NULL);

		// display the file dialog
		if (dlg.DoModal() == IDOK)
		{
			CString zOutImageFile = dlg.GetPathName();
			m_editOutFile.SetWindowText(zOutImageFile);

			// we changed the output file 
			m_strOutImageFile = zOutImageFile;

			// need to update the button states
			setButtonStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17320");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnBtnTest()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (getImageCleanupOperations()->Size() == 0)
		{
			AfxMessageBox("There are no operations to perform!", MB_OK | MB_ICONEXCLAMATION);
			return;
		}

		if (m_bDirty)
		{
			if(!storePageRangeSettings())
			{
				return;
			}
		}

		// need to validate the input image file
		if (!isFileOrFolderValid(m_strInImageFile))
		{
			UCLIDException ue("ELI17339", "Cannot open input image file!");
			ue.addDebugInfo("File Name", m_strInImageFile);
			throw ue;
		}

		// check if the overwrite checkbox is unchecked.  if unchecked then
		// check if the output file already exists, if it does warn user and exit function 
		if ((m_chkOverwrite.GetCheck() == BST_UNCHECKED) && isFileOrFolderValid(m_strOutImageFile))
		{
			string strErrorPrompt = "File: " + m_strOutImageFile + "\nalready exists!";
			AfxMessageBox(strErrorPrompt.c_str(), MB_OK | MB_ICONWARNING);

			return;
		}

		// enable the Wait Cursor as we clean the image
		CWaitCursor waitCursor;

		IImageCleanupEnginePtr ipCleanEngine(CLSID_ImageCleanupEngine);
		ASSERT_RESOURCE_ALLOCATION("ELI17322", ipCleanEngine != __nullptr);

		// InternalUseOnly is not thread safe, but we only have one thread so its okay
		ipCleanEngine->CleanupImageInternalUseOnly(m_strInImageFile.c_str(), m_strOutImageFile.c_str(), 
			m_ipSettings);

		// update the button states
		setButtonStates();

		// restore normal cursor
		waitCursor.Restore();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17321");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnBtnOpenInImage()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (m_strInImageFile != "")
		{
			openImageFile(m_strInImageFile);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17325");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnBtnOpenOutImage()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (m_strOutImageFile != "")
		{
			openImageFile(m_strOutImageFile);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17327");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnChangeTestInFileName()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// get the current text in the edit box
		CString zFileName;
		m_editInFile.GetWindowText(zFileName);

		string strImageFileName(zFileName);

		// user has changed the input file name, set the new in file 
		m_strInImageFile = strImageFileName;

		// check if the user is not overriding the output folder
		if (m_chkOutFile.GetCheck() == BST_UNCHECKED)
		{
			string strExt = getExtensionFromFullPath(m_strInImageFile, true);
			if (isImageFileExtension(strExt) || isNumericExtension(strExt))
			{
				m_strOutImageFile = getCleanImageName(m_strInImageFile);
				m_editOutFile.SetWindowText(m_strOutImageFile.c_str());
			}
		}

		// need to update the button states
		setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17334");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnChangeTestOutFileName()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// get the current text in the edit box
		CString zFileName;
		m_editOutFile.GetWindowText(zFileName);

		m_strOutImageFile = zFileName;

		// need to update the button states
		setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17532");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnClickOverideCheck()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// if the user has unchecked the override box then reset the out file
		if (m_chkOutFile.GetCheck() == BST_UNCHECKED)
		{
			// get the in file name
			CString zInFile;
			m_editInFile.GetWindowText(zInFile);

			// get in file as std::string
			string strInFile(zInFile);

			// set the edit box for the ouput file to read only
			m_editOutFile.SetReadOnly(TRUE);

			// check for empty string
			if (strInFile != "")
			{
				// not empty, get output file from input file name
				m_strOutImageFile = getCleanImageName(strInFile);
				m_editOutFile.SetWindowText(m_strOutImageFile.c_str());
			}
			else
			{
				// empty string, set folder to empty string
				m_editOutFile.SetWindowText("");
			}
		}
		else
		{
			// clear the data in the out file edit box
			m_editOutFile.SetWindowText("");

			// set the edit box for the output file to read/write
			m_editOutFile.SetReadOnly(FALSE);
		}

		// update the button states
		setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17335");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnRadioPagesClicked()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// default all page range edit boxes to disabled
		BOOL bEnableFirstPages(FALSE), bEnableLastPages(FALSE), bEnableSpecifiedPages(FALSE);

		// check which button has been checked. store the changed settings in our settings
		// object. enable the edit box (if any) associated with the selected radio button
		switch(GetCheckedRadioButton(IDC_RADIO_ALLPAGES, IDC_RADIO_SPECIFIEDPAGES))
		{
		case IDC_RADIO_ALLPAGES:
			m_ipSettings->ICPageRangeType = ESImageCleanupLib::kCleanAll;
			m_ipSettings->SpecifiedPages = "";
			break;

		case IDC_RADIO_FIRSTPAGES:
			bEnableFirstPages = TRUE;
			{
				// get the specified pages from the edit box
				CString zFirstPage;
				m_editFirstPages.GetWindowText(zFirstPage);
				
				// store the settings
				m_ipSettings->SpecifiedPages = get_bstr_t(zFirstPage);
				m_ipSettings->ICPageRangeType = ESImageCleanupLib::kCleanFirst;
			}
			break;

		case IDC_RADIO_LASTPAGES:
			bEnableLastPages = TRUE;
			{
				// get the specified pages from the edit box
				CString zLastPage;
				m_editFirstPages.GetWindowText(zLastPage);
				
				// store the settings
				m_ipSettings->SpecifiedPages = get_bstr_t(zLastPage);
				m_ipSettings->ICPageRangeType = ESImageCleanupLib::kCleanLast;
			}
			break;

		case IDC_RADIO_SPECIFIEDPAGES:
			bEnableSpecifiedPages = TRUE;
			{
				// get the specified pages from the edit box
				CString zSpecifiedPages;
				m_editFirstPages.GetWindowText(zSpecifiedPages);
				
				// store the settings
				m_ipSettings->SpecifiedPages = get_bstr_t(zSpecifiedPages);
				m_ipSettings->ICPageRangeType = ESImageCleanupLib::kCleanSpecified;
			}
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI17530");
		}

		// made a change, set dirty flag
		m_bDirty = true;

		// set the enabled/disabled state of the edit boxes
		// NOTE:	must do this before calling setButtonStates
		//			or the test button may be enabled/disabled
		//			incorrectly as it checks the enabled state
		//			of the edit boxes
		m_editFirstPages.EnableWindow(bEnableFirstPages);
		m_editLastPages.EnableWindow(bEnableLastPages);
		m_editSpecifiedPages.EnableWindow(bEnableSpecifiedPages);

		// update the button and menu states
		setButtonStates();
		setMenuStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17527");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnChangeRangeText()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// update the button and menu states
		setButtonStates();
		setMenuStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17556");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnDropFiles(HDROP hDropInfo)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// setup hDropInfo to automatically be released when we go out of scope
		DragDropFinisher finisher(hDropInfo);

		unsigned int iNumFiles = DragQueryFile(hDropInfo, 0xFFFFFFFF, NULL, NULL);
		for (unsigned int ui = 0; ui < iNumFiles; ui++)
		{
			// get the full path to the dragged filename
			char pszFile[MAX_PATH + 1];
			DragQueryFile(hDropInfo, ui, pszFile, MAX_PATH);

			// process dropped file
			processDroppedFile(pszFile);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17228");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnSelectMRUMenu(UINT nID)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{	
		if (nID >= ID_MRU_FILE1 && nID <= ID_MRU_FILE5)
		{
			// Get the current selected file index of MRU list
			int nCurrentSelectedFileIndex = nID - ID_MRU_FILE1;

			// Get file name string
			string strFileToOpen(ma_pMRUList->at(nCurrentSelectedFileIndex));

			openFile(strFileToOpen);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17234");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnTimer(UINT nIDEvent)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// if received auto-save event, do the auto-save as long as the settings are not
		// encrypted
		if (nIDEvent == giAUTO_SAVE_TIMER_ID &&
			m_ipSettings->IsEncrypted == VARIANT_FALSE)
		{
			m_ipSettings->SaveTo(get_bstr_t(m_FRM.getRecoveryFileName()), VARIANT_FALSE);
		}

		CDialog::OnTimer(nIDEvent);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17201");
}
//--------------------------------------------------------------------------------------------------
BOOL CImageCleanupSettingsEditorDlg::OnInitDialog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	try
	{
		CDialog::OnInitDialog();

		// set the auto-save timer
		SetTimer(giAUTO_SAVE_TIMER_ID, giAUTO_SAVE_FREQUENCY, NULL);

		SetIcon(m_hIcon, TRUE);			// Set big icon
		SetIcon(m_hIcon, FALSE);		// Set small icon

		// load the icons for the up and down buttons
		m_btnUp.SetIcon(AfxGetApp()->LoadIcon(ICI_ICON_UP));
		m_btnDown.SetIcon(AfxGetApp()->LoadIcon(ICI_ICON_DOWN));

		// center the static prompt which shows the "The Image Cleanup Settings are encrypted" message
		CWnd *pWnd = GetDlgItem( IDC_STATIC_PROMPT );
		if (pWnd != __nullptr)
		{
			pWnd->CenterWindow();
			pWnd->ShowWindow(SW_HIDE);
		}

		// set the output directory override to unchecked
		m_chkOutFile.SetCheck(BST_UNCHECKED);

		// set the use extract image viewer radio button to checked
		m_radioExtract.SetCheck(BST_CHECKED);

		// set the all pages radio button
		m_radioAllPages.SetCheck(BST_CHECKED);

		// set up list control
		prepareList();

		// refresh the dialog
		refresh();

		// set the scope of cleanup operations
		setScopeOfCleanupOperations();

		// set the MRU file list
		refreshFileMRU();

		// set the button and menu states
		setButtonStates();
		setMenuStates();

		if (m_strCurrentFileName != "")
		{
			openFile(m_strCurrentFileName);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17202");

	return TRUE;  // return TRUE unless you set the focus to a control
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnPaint()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (IsIconic())
		{
			CPaintDC dc(this); // device context for painting

			SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

			// Center icon in client rectangle
			int cxIcon = GetSystemMetrics(SM_CXICON);
			int cyIcon = GetSystemMetrics(SM_CYICON);
			CRect rect;
			GetClientRect(&rect);
			int x = (rect.Width() - cxIcon + 1) / 2;
			int y = (rect.Height() - cyIcon + 1) / 2;

			// Draw the icon
			dc.DrawIcon(x, y, m_hIcon);
		}
		else
		{
			CDialog::OnPaint();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17258");
}
//--------------------------------------------------------------------------------------------------
HCURSOR CImageCleanupSettingsEditorDlg::OnQueryDragIcon()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	return static_cast<HCURSOR>(m_hIcon);
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnDblclkListTasks(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);

	try
	{
		// Check for current cleanup operation selection
		int iIndex = pNMLV->iItem;

		if (iIndex > -1)
		{
			// get the current cleanup operation
			IObjectWithDescriptionPtr	ipCO = getImageCleanupOperations()->At( iIndex );
			ASSERT_RESOURCE_ALLOCATION("ELI17239", ipCO != __nullptr);
			
			// allow the user to modify the cleanup operation
			VARIANT_BOOL vbDirty = getMiscUtils()->HandlePlugInObjectDoubleClick(ipCO, 
				"Task", get_bstr_t(ICO_CLEANING_OPERATIONS_CATEGORYNAME), VARIANT_FALSE,
				0, NULL);

			// Check result
			if (vbDirty == VARIANT_TRUE)
			{
				// update the cleanup operation list box
				replaceCleanupOperationAt(iIndex, ipCO);

				// made a change, set dirty flag
				m_bDirty = true;

				// update menus
				setMenuStates();
			}

			// Retain selection and focus
			m_lstOperationList.SetItemState( iIndex, LVIS_SELECTED, LVIS_SELECTED );
			m_lstOperationList.SetFocus();
		}
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17233");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnLvnItemchangedListTsk(NMHDR *pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);

	try
	{
		// Update button states first
		setButtonStates();


		// Notification code derived from 
		// http://www.codeguru.com/cpp/controls/listview/checkboxes/article.php/c917/

		// Check for no changes
		if ((pNMLV->uOldState == 0) && (pNMLV->uNewState == 0))
		{
			return;
		}

		// Retrieve old check box state
		BOOL bPrevState = (BOOL)(((pNMLV->uOldState & LVIS_STATEIMAGEMASK)>>12)-1);
		if (bPrevState < 0)
		{
			// On startup there's no previous state so assign as false (unchecked)
			bPrevState = 0;
		}

		// New check box state
		BOOL bChecked = (BOOL)(((pNMLV->uNewState & LVIS_STATEIMAGEMASK)>>12)-1);
		if (bChecked < 0)
		{
			// On non-checkbox notifications assume false
			bChecked = 0;
		}

		// Just return if no change in check box
		if (bPrevState == bChecked)
		{
			return;
		}

		// If this cleanup operation is already present
		IIUnknownVectorPtr ipCollection = getImageCleanupOperations();
		ASSERT_RESOURCE_ALLOCATION("ELI17203", ipCollection != __nullptr);
		if (ipCollection->Size() > pNMLV->iItem)
		{
			// Retrieve affected cleanup operation
			IObjectWithDescriptionPtr	ipCO = ipCollection->At( pNMLV->iItem );
			ASSERT_RESOURCE_ALLOCATION("ELI17204", ipCO != __nullptr);

			// Retrieve existing state
			VARIANT_BOOL vbExisting = ipCO->Enabled;

			// Provide new checkbox state to cleanup operation object
			if (!isEqual(vbExisting, bChecked))
			{
				ipCO->Enabled = asVariantBool(bChecked);
				m_bDirty = true;
			}
		}

		// set the menu states
		setMenuStates();
		
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17205");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::OnRightClickTasks(NMHDR *pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// always wait till the item(s) r-clicked is(are) selected
		// then bring up the context menu
		if (pNMHDR)
		{
			int iIndex = -1;
			POSITION pos = m_lstOperationList.GetFirstSelectedItemPosition();
			if (pos != __nullptr)
			{
				// Get index of first selection
				iIndex = m_lstOperationList.GetNextSelectedItem( pos );
			}

			// Load the context menu
			CMenu menu;
			menu.LoadMenu( IDR_MNU_CONTEXT );
			CMenu *pContextMenu = menu.GetSubMenu( 0 );
			
			//////////////////////////
			// Enable or disable items
			//////////////////////////
			UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
			UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
			bool bEnable = iIndex > -1;
			
			// disable cut, copy and delete if no settings item selected
			pContextMenu->EnableMenuItem(ID_EDIT_CUT, bEnable ? nEnable : nDisable);
			pContextMenu->EnableMenuItem(ID_EDIT_COPY, bEnable ? nEnable : nDisable);
			pContextMenu->EnableMenuItem(ID_EDIT_DELETE, bEnable ? nEnable : nDisable);
			
			bEnable = 
				m_ipClipboardMgr->IUnknownVectorIsOWDOfType(IID_IImageCleanupOperation) == VARIANT_TRUE;
			
			// enable paste if there is a vector of ObjectsWithDescription in the clipboard
			pContextMenu->EnableMenuItem(ID_EDIT_PASTE, bEnable ? nEnable : nDisable);
			
			// Map the point to the correct position
			CPoint	point;
			GetCursorPos(&point);
			
			// Display and manage the context menu
			pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
				point.x, point.y, this );
		}

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17241")
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::prepareList()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	// set list style
	m_lstOperationList.SetExtendedStyle(
		LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT | LVS_EX_CHECKBOXES);

	// build column information struct
	LVCOLUMN lvColumn;
	lvColumn.mask = LVCF_FMT | LVCF_TEXT | LVCF_WIDTH;
	lvColumn.fmt = LVCFMT_LEFT;

	// set information for run column (run column has fixed width)
	lvColumn.pszText = "Run";
	lvColumn.cx = giRUN_WIDTH;

	// add the run column
	m_lstOperationList.InsertColumn(giRUN_LIST_COLUMN, &lvColumn);
	
	// get dimensions of list control
	CRect recList;
	m_lstOperationList.GetClientRect(&recList);

	// set heading and compute width for operations column
	lvColumn.pszText = "Cleanup Operation";
	lvColumn.cx = recList.Width() - giRUN_WIDTH;

	// add the operations column
	m_lstOperationList.InsertColumn(giCLEAN_OPERATIONS_LIST_COLUMN, &lvColumn);
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::setButtonStates()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Retrieve current settings
		UpdateData(FALSE);

		// Enable the Add button and the list
		m_btnAdd.EnableWindow( TRUE );
		m_lstOperationList.EnableWindow( TRUE );

		// Check count of cleanup operation
		int	iCount = m_lstOperationList.GetItemCount();
		if (iCount == 0)
		{
			// Disable other buttons
			m_btnRemove.EnableWindow( FALSE );
			m_btnConfigure.EnableWindow( FALSE );
			m_btnUp.EnableWindow( FALSE );
			m_btnDown.EnableWindow( FALSE );
		}
		else
		{	
			// Next have to see if an item is selected
			int iIndex = -1;
			POSITION pos = m_lstOperationList.GetFirstSelectedItemPosition();
			if (pos != __nullptr)
			{
				// Get index of first selection
				iIndex = m_lstOperationList.GetNextSelectedItem( pos );
			}
			if (iIndex > -1)
			{
				// Enable the Delete button
				m_btnRemove.EnableWindow( TRUE );

				// Check for multiple selection
				int iIndex2 = m_lstOperationList.GetNextSelectedItem( pos );
				if (iIndex2 == -1)
				{
					// Only one selected, enable Configure
					m_btnConfigure.EnableWindow( TRUE );

					// Must be more than one for these buttons
					if (iCount > 1)
					{
						// Check boundary conditions
						if (iIndex == iCount - 1)
						{
							// Cannot move last item down
							m_btnDown.EnableWindow( FALSE );
						}
						else
						{
							m_btnDown.EnableWindow( TRUE );
						}

						if (iIndex == 0)
						{
							// Cannot move first item up
							m_btnUp.EnableWindow( FALSE );
						}
						else
						{
							m_btnUp.EnableWindow( TRUE );
						}
					}
					else
					{
						// Cannot change order if only one 
						m_btnUp.EnableWindow( FALSE );
						m_btnDown.EnableWindow( FALSE );
					}
				}
				else
				{
					// More than one is selected, disable other buttons
					m_btnConfigure.EnableWindow( FALSE );
					m_btnUp.EnableWindow( FALSE );
					m_btnDown.EnableWindow( FALSE );
				}
			}
			else
			{
				// No selection --> disable most buttons
				m_btnRemove.EnableWindow( FALSE );
				m_btnConfigure.EnableWindow( FALSE );
				m_btnUp.EnableWindow( FALSE );
				m_btnDown.EnableWindow( FALSE );
			}
		}

		// set the test area button states
		bool bEnableIn(false), bEnableOut(false);

		// check if it is image extension or numeric extension (many customers store tif
		// images with a numeric extension where the digits represent the number of
		// pages in the image
		string strExt = getExtensionFromFullPath(m_strInImageFile, true);
		if (isImageFileExtension(strExt) || isNumericExtension(strExt))
		{
			bEnableIn = isFileOrFolderValid(m_strInImageFile);
		}

		// if the input file name is an image file type and the file exists
		m_btnOpenInFile.EnableWindow(asMFCBool(bEnableIn));

		// check if the output image file has an image extension
		strExt = getExtensionFromFullPath(m_strOutImageFile, true);
		bEnableOut = (isImageFileExtension(strExt) || isNumericExtension(strExt));

		m_btnOpenOutFile.EnableWindow(asMFCBool(bEnableOut));

		// if we have an input image that exists, an output image that is an image file, data
		// in the operations list, and an appropriate cleaning scope - enable the test button
		bool bEnableTest = (bEnableIn && bEnableOut && 
			(getImageCleanupOperations()->Size() > 0) && !isPageRangeEmpty());
		m_btnTest.EnableWindow(asMFCBool(bEnableTest));

		// enable the output file browse button if the override box is checked
		m_btnBrowseOutFile.EnableWindow(asMFCBool(m_chkOutFile.GetCheck() == BST_CHECKED));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17338");
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::setMenuStates()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// get the main menu
		CMenu* pMainMenu = GetMenu();

		// enable Save if the dirty flag is set, there are items in the settings list,
		// and a page range has been set
		bool bEnable = (m_bDirty && getImageCleanupOperations()->Size() > 0 && !isPageRangeEmpty());
		pMainMenu->EnableMenuItem( ID_FILE_SAVE_SETTINGS, 
			bEnable ? MF_BYCOMMAND | MF_ENABLED : MF_BYCOMMAND | MF_GRAYED );

		// enable SaveAs if there are items in the list and a page range has been set
		bEnable = (getImageCleanupOperations()->Size() > 0 && !isPageRangeEmpty());
		pMainMenu->EnableMenuItem( ID_FILE_SAVEAS, 
			bEnable ? MF_BYCOMMAND | MF_ENABLED : MF_BYCOMMAND | MF_GRAYED );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17533");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::setScopeOfCleanupOperations()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// clear and disable the scope edit boxes
		m_editFirstPages.SetWindowText("");
		m_editFirstPages.EnableWindow(FALSE);
		m_editLastPages.SetWindowText("");
		m_editLastPages.EnableWindow(FALSE);
		m_editSpecifiedPages.SetWindowText("");
		m_editSpecifiedPages.EnableWindow(FALSE);

		// variable to store the appropriate radio button value
		int nCheckedButton = 0;

		// check the stored PageRangeType
		switch(m_ipSettings->ICPageRangeType)
		{
		case ESImageCleanupLib::kCleanAll:
			// set the all pages radio button, no edit box to set
			nCheckedButton = IDC_RADIO_ALLPAGES;
			break;

		case ESImageCleanupLib::kCleanFirst:
			// set the first radio button
			nCheckedButton = IDC_RADIO_FIRSTPAGES;

			// get the specified page value from the settings pointer
			m_editFirstPages.SetWindowText(m_ipSettings->SpecifiedPages);

			// enable the first pages edit box
			m_editFirstPages.EnableWindow(TRUE);
			break;

		case ESImageCleanupLib::kCleanLast:
			// set the last radio button
			nCheckedButton = IDC_RADIO_LASTPAGES;

			// get the specified page value from the settings pointer
			m_editLastPages.SetWindowText(m_ipSettings->SpecifiedPages);

			// enable the last pages edit box
			m_editLastPages.EnableWindow(TRUE);
			break;

		case ESImageCleanupLib::kCleanSpecified:
			// set the specified radio button
			nCheckedButton = IDC_RADIO_SPECIFIEDPAGES;

			// get the specified page value from the settings pointer
			m_editSpecifiedPages.SetWindowText(m_ipSettings->SpecifiedPages);

			// enable the specified pages edit box
			m_editSpecifiedPages.EnableWindow(TRUE);
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI17536");
		}

		// check the appropriate radio button
		CheckRadioButton(IDC_RADIO_ALLPAGES, IDC_RADIO_SPECIFIEDPAGES, nCheckedButton);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17534");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::refresh()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// clear the list
		m_lstOperationList.DeleteAllItems();

		// get the cleanup operations vector
		IIUnknownVectorPtr ipVecImageCleanupOperations = getImageCleanupOperations();

		// get the size of the vector
		long lSize = ipVecImageCleanupOperations->Size();

		// loop through the vector adding the cleanup operations to the list
		for (long i = 0; i < lSize; i++)
		{
			addImageCleanupOperation(ipVecImageCleanupOperations->At(i));
		}

		//Update the button and menu states
		setButtonStates();
		setMenuStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17567");
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::deleteSelectedTasks()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	int iCount = m_lstOperationList.GetItemCount();

	// step through the list backwards
	for (int i = iCount-1; i >= 0; i--)
	{
		// check if the item is selected
		if (m_lstOperationList.GetItemState(i, LVIS_SELECTED) == LVIS_SELECTED)
		{
			// delete selected item
			m_lstOperationList.DeleteItem(i);

			// remove the item from the vector of cleanup operation
			getImageCleanupOperations()->Remove(i);
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::clearListSelection()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	POSITION pos = m_lstOperationList.GetFirstSelectedItemPosition();
	if (pos == NULL)
	{
		// no item selected, return
		return;
	}
	
	// iterate through all selections and set them as unselected.  pos will be
	// set to NULL by GetNextSelectedItem if there are no more selected items
	while (pos)
	{
		int nItemSelected = m_lstOperationList.GetNextSelectedItem(pos);
		m_lstOperationList.SetItemState(nItemSelected, 0, LVIS_SELECTED);
	}
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::replaceCleanupOperationAt(int iIndex, IObjectWithDescriptionPtr ipNewCO)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update the cleanup operation vector
		getImageCleanupOperations()->Set(iIndex, ipNewCO );

		// Update the cleanup operation listbox
		m_lstOperationList.DeleteItem( iIndex );
		m_lstOperationList.InsertItem( iIndex, "" );
		m_lstOperationList.SetItemText( iIndex, 1, ipNewCO->Description );
		m_lstOperationList.SetCheck( iIndex, ipNewCO->Enabled );

		// Retain selection and focus
		m_lstOperationList.SetItemState( iIndex, LVIS_SELECTED, LVIS_SELECTED );
		m_lstOperationList.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17568");
}
//--------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CImageCleanupSettingsEditorDlg::getImageCleanupOperations()
{
	// get the image cleanup operations vector
	IIUnknownVectorPtr ipImageCleanupOperations = m_ipSettings->ImageCleanupOperations;
	ASSERT_RESOURCE_ALLOCATION("ELI17206", ipImageCleanupOperations != __nullptr);

	return ipImageCleanupOperations;
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CImageCleanupSettingsEditorDlg::getMiscUtils()
{
	// check if a MiscUtils object has already been created
	if (!m_ipMiscUtils)
	{
		// create MiscUtils object
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI17207", m_ipMiscUtils != __nullptr);
	}

	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
// this method is only called by the refresh method which calls setButtonStates and setMenuStates
// so there is no need to do that in here as well.
void CImageCleanupSettingsEditorDlg::addImageCleanupOperation(IObjectWithDescriptionPtr ipObject)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		ASSERT_ARGUMENT("ELI17208", ipObject != __nullptr);

		CString zDescription = (char*)ipObject->Description;

		// Add item to end of list, set the text, set the checkbox
		int iIndex = m_lstOperationList.GetItemCount();
		int iNewIndex = m_lstOperationList.InsertItem( iIndex, "" );
		m_lstOperationList.SetItemText( iNewIndex, 1, zDescription );
		m_lstOperationList.SetCheck( iNewIndex, asMFCBool( ipObject->Enabled ) );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17569");
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::getDlgItemWindowRect(UINT uiDlgItemResourceID, RECT &rectWindow)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// retrieve the dialog item using its resource ID
		CWnd* cwndDlgItem = GetDlgItem(uiDlgItemResourceID);
		ASSERT_RESOURCE_ALLOCATION("ELI17215", cwndDlgItem != __nullptr);

		// set the window rect to the appropriate position and dimensions
		cwndDlgItem->GetWindowRect(&rectWindow);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17570");
}
//-------------------------------------------------------------------------------------------------
bool CImageCleanupSettingsEditorDlg::checkModification()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Check modified state, prompt the user to save changes, etc.
		if (m_ipSettings)
		{
			IPersistStreamPtr ipPersistStream(m_ipSettings);
			ASSERT_RESOURCE_ALLOCATION("ELI17209", ipPersistStream != __nullptr);
			if (ipPersistStream)
			{
				// if the image cleanup settings are modified, prompt for saving
				if (m_bDirty || ipPersistStream->IsDirty() == S_OK)
				{
					int nRes = 
						MessageBox("Current Image Cleanup Settings have been modified. "
						"Do you wish to save the changes?", 
						"Save Changes?", MB_YESNOCANCEL);
					if (nRes == IDCANCEL)
					{
						// user wants to cancel the action, 
						// do not continue with any further action
						return false;
					}
					
					if (nRes == IDYES)
					{
						// save the changes
						OnFileSave();

						// Check for Cancel from Save dialog
						if (ipPersistStream->IsDirty() == S_OK)
						{
							// do not continue with any further action
							return false;
						}
					}
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17571");

	return true;
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::updateWindowCaption()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		const string strWINDOW_TITLE = "Image Cleanup Settings Editor";
		
		// compute the window caption depending upon the name of the
		// currently loaded file, if any
		string strResult;
		if (!m_strCurrentFileName.empty())
		{
			// if a file is currently loaded, then only display the filename and
			// not the full path.
			strResult = getFileNameFromFullPath(m_strCurrentFileName);
			strResult += " - ";
			strResult += strWINDOW_TITLE;
		}
		else
		{
			strResult = strWINDOW_TITLE;
		}

		// update the window caption
		SetWindowText(strResult.c_str());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17112");
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::openFile(string strFileName) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// if checkModification() returns false, it means the user
		// want to cancel current task, i.e. opening another file
		if (!checkModification()) return;
		
		// set the dirty flag for the menu update
		m_bDirty = false;

		if (strFileName == "")
		{
			// ask user to select file to load
			CFileDialog fileDlg(TRUE, gstrICS_FILE_OPEN_TYPES.c_str(), m_strLastFileOpened.c_str(), 
				OFN_ENABLESIZING | OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST, 
				gstrICS_FILE_OPEN_FILTER.c_str(), this);
			
			if (fileDlg.DoModal() != IDOK)
			{
				return;
			}
			
			strFileName = (LPCTSTR) fileDlg.GetPathName();
		}

		CWaitCursor wait;
		
		// make sure the file exists
		validateFileOrFolderExistence(strFileName);

		// verify extension is ICS File
		string strExt = getExtensionFromFullPath( strFileName, true );
		if (( strExt != ".ics" ) && ( strExt != ".etf" ))
		{
			UCLIDException ue("ELI17210", "File is not an ICS file.");
			ue.addDebugInfo("File Name", strFileName);
			throw ue;
		}
		
		// load the clean settings object from the specified file
		m_ipSettings->LoadFrom(get_bstr_t(strFileName), VARIANT_FALSE);

		if (!isVectorOfOWDOfCleanupOperations(getImageCleanupOperations()))
		{
			UCLIDException ue("ELI17229", "Failed opening file, file is corrupt!");
			ue.addDebugInfo("File Name", strFileName);
			getImageCleanupOperations()->Clear();
			throw ue;
		}
		
		// add the file to MRU list
		addFileToMRUList(strFileName);
		
		// set the scope of cleanup operations
		setScopeOfCleanupOperations();

		refreshUIFromImageCleanupSettings();
		
		// update the caption of the window to contain the filename
		m_strLastFileOpened = strFileName;
		m_strCurrentFileName = m_strLastFileOpened;
		updateWindowCaption();
	}
	catch (...)
	{
		removeFileFromMRUList(strFileName);
		throw;
	}
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::refreshFileMRU()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// if this is a initialization call, i.e. the dialog is
		// just created, then we need to read MRU from Registry
		ma_pMRUList->readFromPersistentStore();
		int nListSize = ma_pMRUList->getCurrentListSize();
		if (nListSize == 0)
		{
			// if the m_pMRUFilesMenu has not been set yet, then return
			if (m_pMRUFilesMenu == NULL)
			{
				return;
			}
		}

		if (m_pMRUFilesMenu == NULL)
		{
			CMenu* pMainMenu = GetMenu();
			CMenu* pFileMenu = pMainMenu->GetSubMenu(0);
			m_pMRUFilesMenu = pFileMenu->GetSubMenu(6);
			if (m_pMRUFilesMenu == NULL)
			{
				throw UCLIDException("ELI17211", "Unable to get MRU File menu.");
			}
			// remove the "No File" item from the menu
			m_pMRUFilesMenu->RemoveMenu(ID_FILE_MRU, MF_BYCOMMAND);
		}

		// get total number of items currently on the menu
		int nTotalItems = m_pMRUFilesMenu->GetMenuItemCount();
		int n = 0;
		for (n = 0; n < nListSize; n++)
		{
			// if the file item already exists on the menu, just modify the file name
			if (nTotalItems > 0 && n < nTotalItems)
			{
				m_pMRUFilesMenu->ModifyMenu(n, MF_BYPOSITION, ID_MRU_FILE1 + n, ma_pMRUList->at(n).c_str());
				continue;
			}

			m_pMRUFilesMenu->InsertMenu(-1, MF_BYCOMMAND, ID_MRU_FILE1 + n, ma_pMRUList->at(n).c_str());
		}

		// if total number of items on the menu exceeds the 
		// number of entries from the Registry, remove the unnecessary one(s)
		if (nTotalItems > nListSize)
		{
			for (n = nListSize; n < nTotalItems; n++)
			{
				m_pMRUFilesMenu->RemoveMenu(ID_MRU_FILE1+n, MF_BYCOMMAND);
			}

			if (m_pMRUFilesMenu->GetMenuItemCount() == 0)
			{
				m_pMRUFilesMenu->InsertMenu(-1, MF_BYCOMMAND, ID_MRU_FILE1, "No File");
				m_pMRUFilesMenu->EnableMenuItem(ID_MRU_FILE1, MF_BYCOMMAND | MF_GRAYED);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17113");
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::addFileToMRUList(const string& strFileToBeAdded)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// we wish to have updated items all the time
		ma_pMRUList->readFromPersistentStore();
		ma_pMRUList->addItem(strFileToBeAdded);
		ma_pMRUList->writeToPersistentStore();

		refreshFileMRU();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17114");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::removeFileFromMRUList(const string& strFileToBeRemoved)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// remove the bad file from MRU List
		ma_pMRUList->readFromPersistentStore();
		ma_pMRUList->removeItem(strFileToBeRemoved);
		ma_pMRUList->writeToPersistentStore();

		refreshFileMRU();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17115");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::refreshUIFromImageCleanupSettings()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (m_ipSettings->GetIsEncrypted() == VARIANT_TRUE)
		{
			enableEditFeatures(false);
			return;
		}

		enableEditFeatures(true);

		refresh();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17256");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::enableEditFeatures(bool bEnable) 
{
	static vector<long> vecICSControlIDs;
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// populate IDs of UI controls that must only be enabled in edit mode
		if (vecICSControlIDs.empty())
		{
			vecICSControlIDs.push_back(IDC_STATIC);
			vecICSControlIDs.push_back(IDC_LIST_TASKS);
			vecICSControlIDs.push_back(IDC_BTN_ADD);
			vecICSControlIDs.push_back(IDC_BTN_REMOVE);
			vecICSControlIDs.push_back(IDC_BTN_CONFIG);
			vecICSControlIDs.push_back(IDC_BTN_UP);
			vecICSControlIDs.push_back(IDC_BTN_DN);
			vecICSControlIDs.push_back(IDC_RADIO_ALLPAGES);
			vecICSControlIDs.push_back(IDC_RADIO_FIRSTPAGES);
			vecICSControlIDs.push_back(IDC_RADIO_LASTPAGES);
			vecICSControlIDs.push_back(IDC_RADIO_SPECIFIEDPAGES);
			vecICSControlIDs.push_back(IDC_EDIT_FIRSTPAGES);
			vecICSControlIDs.push_back(IDC_EDIT_LASTPAGES);
			vecICSControlIDs.push_back(IDC_EDIT_SPECIFIEDPAGES);
			vecICSControlIDs.push_back(IDC_GROUP_SCOPE);
			vecICSControlIDs.push_back(IDC_STATIC_FIRSTPAGES);
			vecICSControlIDs.push_back(IDC_STATIC_LASTPAGES);
		}

		// Show/Hide the controls that should only be shown in edit-mode
		vector<long>::const_iterator iter;
		for (iter = vecICSControlIDs.begin(); iter != vecICSControlIDs.end(); iter++)
		{
			long nID = *iter;
			CWnd*	pWnd = GetDlgItem( nID );
			if (pWnd != __nullptr)
			{
				pWnd->ShowWindow( bEnable ? SW_SHOW : SW_HIDE );
			}
		}

		/////////////////////////////////
		// Hide/show the encrypted prompt
		/////////////////////////////////
		CWnd *pWnd = GetDlgItem( IDC_STATIC_PROMPT );
		if (pWnd != __nullptr)
		{
			pWnd->ShowWindow( bEnable ? SW_HIDE : SW_SHOW );
		}

		// populate IDs of menu items that must only be enabled in edit mode
		static vector<long> vecICSMenuIDs;
		if (vecICSMenuIDs.empty())
		{
			vecICSMenuIDs.push_back(ID_FILE_SAVE_SETTINGS);
			vecICSMenuIDs.push_back(ID_FILE_SAVEAS);
		}

		CMenu* pMainMenu = GetMenu();
		// Enable/disable the menu items that should only be enabled in edit-mode
		for (iter = vecICSMenuIDs.begin(); iter != vecICSMenuIDs.end(); iter++)
		{
			long nID = *iter;

			pMainMenu->EnableMenuItem( nID, 
				bEnable ? MF_BYCOMMAND | MF_ENABLED : MF_BYCOMMAND | MF_GRAYED );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17257");
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::updateComponentCacheFile()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{	
		// create instance of the category manager
		ICategoryManagerPtr ipCatMgr(CLSID_CategoryManager);
		if (ipCatMgr == __nullptr)
		{
			throw UCLIDException("ELI17212", "Unable to create instance of CategoryManager!");
		}

		// create a vector of all categories we care about.
		vector<string> vecCategories;
		vecCategories.push_back(ICO_CLEANING_OPERATIONS_CATEGORYNAME);

		// Create a vector for component counts
		vector<int> vecCounts;

		// for each category we care about, find new components that may
		// be registered in that category
		vector<string>::const_iterator iter;
		for (iter = vecCategories.begin(); iter != vecCategories.end(); iter++)
		{
			// delete the cache file for the current category
			_bstr_t _bstrCategory = get_bstr_t(iter->c_str());
			ipCatMgr->DeleteCache(_bstrCategory);

			// recreate the cache file for the category
			IStrToStrMapPtr ipMap = ipCatMgr->GetDescriptionToProgIDMap1(_bstrCategory);
			ASSERT_RESOURCE_ALLOCATION("ELI17213", ipMap != __nullptr);

			// Store count of components found
			vecCounts.push_back( ipMap->Size );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17214")
}
//-------------------------------------------------------------------------------------------------
bool CImageCleanupSettingsEditorDlg::isVectorOfOWDOfCleanupOperations(IIUnknownVectorPtr ipVector)
{
	bool bResult = false;

	try
	{
		ASSERT_ARGUMENT("ELI17227", ipVector != __nullptr);

		bool bVectorIsOK = true;
		// get vector size
		long lSize = ipVector->Size();

		// check each item in vector
		for (long i = 0; i < lSize; i++)
		{
			IObjectWithDescriptionPtr ipOWD = ipVector->At(i);
			if (ipOWD == __nullptr)
			{
				// object is not an ObjectWithDescription. set result to false and exit loop.
				bVectorIsOK = false;
				break;
			}

			// get the object from the ObjectWithDescription
			IUnknownPtr ipObject = ipOWD->Object;
			ASSERT_RESOURCE_ALLOCATION("ELI17574", ipObject != __nullptr);

			// check if the object is an ImageCleanupOperation
			IImageCleanupOperationPtr ipICO = ipObject;
			if (ipICO == __nullptr)
			{
				// object is not an ImageCleanupOperation. set result to false and exit loop.
				bVectorIsOK = false;
				break;
			}
		}

		bResult = bVectorIsOK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17572");

	return bResult;
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::processDroppedFile(char* pszFile)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	// act as if the user opened the file from the File-Open dialog box
	openFile(pszFile);
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettingsEditorDlg::openImageFile(const string& strImageFileName)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// check to be sure image file exists
		validateFileOrFolderExistence(strImageFileName);

		// options for opening the file with the SRW
		string strOpenOptions = "\"" + strImageFileName + "\"";
		switch(GetCheckedRadioButton(IDC_RADIO_EXTRACT, IDC_RADIO_REGISTERED))
		{
		case IDC_RADIO_EXTRACT:
			runEXE(getPathToSpotRecognitionWindowExe(), strOpenOptions);
			break;

		case IDC_RADIO_REGISTERED:
			ShellExecute(this->m_hWnd, "open", strImageFileName.c_str(), NULL, NULL, SW_SHOW);
			break;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17326");
}
//-------------------------------------------------------------------------------------------------
std::string CImageCleanupSettingsEditorDlg::getPathToSpotRecognitionWindowExe()
{
	if (m_strImageViewerExePath == "")
	{
		string strPath = "";

		// check debug path first
		strPath = getCurrentProcessEXEDirectory();
		strPath += "\\.\\ImageViewer.exe";
		simplifyPathName(strPath);

		// check for Image Viewer
		if (isFileOrFolderValid(strPath))
		{
			m_strImageViewerExePath = strPath;
		}
		else
		{
			UCLIDException ue("ELI17356", "Cannot find ImageViewer.exe.");
			ue.addDebugInfo("Path", strPath);
		}
	}

	return m_strImageViewerExePath;
}
//-------------------------------------------------------------------------------------------------
bool CImageCleanupSettingsEditorDlg::storePageRangeSettings()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	// flag for return
	bool bRangeOK = false;

	try
	{
		switch(GetCheckedRadioButton(IDC_RADIO_ALLPAGES, IDC_RADIO_SPECIFIEDPAGES))
		{
		case IDC_RADIO_ALLPAGES:
			{
				// store the settings
				m_ipSettings->SpecifiedPages = "";
				m_ipSettings->ICPageRangeType = ESImageCleanupLib::kCleanAll;

				bRangeOK = true;
			}
			break;

		case IDC_RADIO_FIRSTPAGES:
			{
				// get the specified pages from the edit box
				CString zFirstPage;
				m_editFirstPages.GetWindowText(zFirstPage);

				// check for empty range
				if (zFirstPage == "")
				{
					AfxMessageBox("Cannot leave the pages field blank!", MB_OK | MB_ICONSTOP);
					m_editFirstPages.SetFocus();
				}
				else
				{
					// store the settings
					m_ipSettings->SpecifiedPages = get_bstr_t(zFirstPage);
					m_ipSettings->ICPageRangeType = ESImageCleanupLib::kCleanFirst;

					bRangeOK = true;
				}
			}
			break;

		case IDC_RADIO_LASTPAGES:
			{
				// get the specified pages from the edit box
				CString zLastPage;
				m_editLastPages.GetWindowText(zLastPage);

				// check for empty range
				if (zLastPage == "")
				{
					AfxMessageBox("Cannot leave the pages field blank!", MB_OK | MB_ICONSTOP);
					m_editLastPages.SetFocus();
				}
				else
				{
					// store the settings
					m_ipSettings->SpecifiedPages = get_bstr_t(zLastPage);
					m_ipSettings->ICPageRangeType = ESImageCleanupLib::kCleanLast;

					bRangeOK = true;
				}
			}
			break;

		case IDC_RADIO_SPECIFIEDPAGES:
			{
				// get the specified pages from the edit box
				CString zSpecifiedPages;
				m_editSpecifiedPages.GetWindowText(zSpecifiedPages);

				string strSpecifiedPages(zSpecifiedPages);

				// check for empty range
				if (strSpecifiedPages == "")
				{
					AfxMessageBox("Cannot leave the pages field blank!", MB_OK | MB_ICONSTOP);
					m_editSpecifiedPages.SetFocus();
				}
				else
				{
					// validate the page string
					try
					{
						// will throw an exception if the string is invalid
						validatePageNumbers(strSpecifiedPages);

						// store the settings
						m_ipSettings->SpecifiedPages = strSpecifiedPages.c_str();
						m_ipSettings->ICPageRangeType = ESImageCleanupLib::kCleanSpecified;

						bRangeOK = true;
					}
					catch(...)
					{
						// prompt the user with the incorrect error string
						string strErrorMessage = "\"" + strSpecifiedPages + "\" is an ";
						strErrorMessage += "invalid page specification!";
						AfxMessageBox(strErrorMessage.c_str(), MB_OK | MB_ICONSTOP);

						// select the invalid page range
						m_editSpecifiedPages.SetSel(0, m_editSpecifiedPages.GetWindowTextLength());

						// set the focus to the invalid page range
						m_editSpecifiedPages.SetFocus();
					}
				}
			}
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI17541");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17540");

	return bRangeOK;
}
//-------------------------------------------------------------------------------------------------
bool CImageCleanupSettingsEditorDlg::isPageRangeEmpty()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	// boolean for return value - default to page range not empty 
	bool bRangeEmpty = false;

	try
	{
		// check each possible edit box to see if it is enabled, if it is
		// check if the text length is 0
		if (asCppBool(m_editFirstPages.IsWindowEnabled()))
		{
			bRangeEmpty = (m_editFirstPages.GetWindowTextLength() == 0);
		}
		else if (asCppBool(m_editLastPages.IsWindowEnabled()))
		{
			bRangeEmpty = (m_editLastPages.GetWindowTextLength() == 0);
		}
		else if (asCppBool(m_editSpecifiedPages.IsWindowEnabled()))
		{
			bRangeEmpty = (m_editSpecifiedPages.GetWindowTextLength() == 0);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17555");

	return bRangeEmpty;
}
//-------------------------------------------------------------------------------------------------