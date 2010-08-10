// FileProcessingDlgTaskPage.cpp : implementation file
// Note:
// There are 2 similar functions: 
//		OnBtnAdd() and 
//		addFileProcessor(IObjectWithDescriptionPtr ipObj);
// OnBtnAdd works when the user clicks on the add button and inserts
// a file processor into the list. addFileProcessor is called when 
// the File Processing Manager is opening a file and it populates the
// list control in a slightly different way. 

#include "stdafx.h"
#include "resource.h"
#include "FileProcessingDlgTaskPage.h"
#include "FPCategories.h"
#include "FileProcessingUtils.h"
#include "FileProcessingDlg.h"
#include "CommonConstants.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Width of Run column
const long	glENABLED_WIDTH = 50;

// Index of "Anyone" from the skipped file scope combo box
const int giCOMBO_INDEX_ANYONE = 1;

// The upper bound for the threads dialog box
const int giTHREADS_UPPER_RANGE = 100;

// ProgID for the set schedule COM object
const string gstrSET_SCHEDULE_PROG_ID = "Extract.FileActionManager.Forms.SetProcessingSchedule";

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgTaskPage property page
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(FileProcessingDlgTaskPage, CPropertyPage)
//-------------------------------------------------------------------------------------------------
FileProcessingDlgTaskPage::FileProcessingDlgTaskPage() 
: CPropertyPage(FileProcessingDlgTaskPage::IDD), 
  m_ipClipboardMgr(NULL),
  m_ipMiscUtils(NULL),
  m_ipSchedule(NULL),
  m_dwSel(0),
  m_bEnabled(true),
  m_bInitialized(false),
  m_bLogErrorDetails(FALSE),
  m_bExecuteErrorTask(FALSE),
  m_pFPM(NULL),
  m_bLimitProcessingTimes(FALSE)
{
}
//-------------------------------------------------------------------------------------------------
FileProcessingDlgTaskPage::~FileProcessingDlgTaskPage()
{
	try
	{
		m_ipClipboardMgr = NULL;
		m_ipMiscUtils = NULL;
		m_ipSchedule = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16529");
}
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlgTaskPage::PreTranslateMessage(MSG* pMsg) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15272")

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_BTN_ADD, m_btnAdd);
	DDX_Control(pDX, IDC_BTN_MODIFY, m_btnModify);
	DDX_Control(pDX, IDC_BTN_REMOVE, m_btnRemove);
	DDX_Control(pDX, IDC_BTN_UP, m_btnUp);
	DDX_Control(pDX, IDC_BTN_DOWN, m_btnDown);
	DDX_Control(pDX, IDC_RADIO_MAX_THREADS, m_btnMaxThreads);
	DDX_Control(pDX, IDC_RADIO_THREADS, m_btnNumThreads);
	DDX_Control(pDX, IDC_EDIT_THREADS, m_editThreads);
	DDX_Control(pDX, IDC_SPIN_THREADS, m_SpinThreads);
	DDX_Control(pDX, IDC_RADIO_KEEP_PROCESSING_FILES, m_btnKeepProcessingWithEmptyQueue);
	DDX_Control(pDX, IDC_RADIO_STOP_PROCESSING_FILES, m_btnStopProcessingWithEmptyQueue);
	DDX_Control(pDX, IDC_LIST_FP, m_fileProcessorList);
	DDX_Control(pDX, IDC_CHECK_LOG_ERROR_DETAILS, m_btnLogErrorDetails);
	DDX_Check(pDX, IDC_CHECK_LOG_ERROR_DETAILS, m_bLogErrorDetails);
	DDX_Control(pDX, IDC_EDIT_ERROR_LOG, m_editErrorLog);
	DDX_Text(pDX, IDC_EDIT_ERROR_LOG, m_zErrorLog);
	DDX_Control(pDX, IDC_BTN_SELECT_DOC_TAG, m_btnErrorSelectTag);
	DDX_Control(pDX, IDC_BTN_BROWSE_LOG, m_btnBrowseErrorLog);
	DDX_Control(pDX, IDC_CHECK_EXECUTE_TASK, m_btnExecuteErrorTask);
	DDX_Check(pDX, IDC_CHECK_EXECUTE_TASK, m_bExecuteErrorTask);
	DDX_Text(pDX, IDC_EDIT_EXECUTE_TASK, m_zErrorTaskDescription);
	DDX_Control(pDX, IDC_BTN_SELECT_ERROR_TASK, m_btnSelectErrorTask);
	DDX_Control(pDX, IDC_RADIO_PROCESS_ALL_FILES_PRIORITY, m_radioProcessAll);
	DDX_Control(pDX, IDC_RADIO_PROCESS_SKIPPED_FILES, m_radioProcessSkipped);
	DDX_Control(pDX, IDC_COMBO_SKIPPED_SCOPE, m_comboSkipped);
	DDX_Control(pDX, IDC_STATIC_SKIPPED, m_staticSkipped);
	DDX_Control(pDX, IDC_STATIC_PROCESSING_SCHEDULE, m_groupProcessingSchedule);
	DDX_Check(pDX, IDC_CHECK_LIMIT_PROCESSING, m_bLimitProcessingTimes);
	DDX_Control(pDX, IDC_BUTTON_SET_SCHEDULE, m_btnSetSchedule);
	DDX_Control(pDX, IDC_CHECK_LIMIT_PROCESSING, m_checkLimitProcessing);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileProcessingDlgTaskPage, CPropertyPage)
	ON_BN_CLICKED(IDC_BTN_ADD, OnBtnAdd)
	ON_BN_CLICKED(IDC_BTN_REMOVE, OnBtnRemove)
	ON_BN_CLICKED(IDC_BTN_MODIFY, OnBtnModify)
	ON_BN_CLICKED(IDC_BTN_DOWN, OnBtnDown)
	ON_BN_CLICKED(IDC_BTN_UP, OnBtnUp)
	ON_BN_CLICKED(IDC_RADIO_MAX_THREADS, OnBtnMaxThread)
	ON_BN_CLICKED(IDC_RADIO_THREADS, OnBtnNumThread)
	ON_BN_CLICKED(IDC_RADIO_KEEP_PROCESSING_FILES, OnBtnKeepProcessingWithEmptyQueue)
	ON_BN_CLICKED(IDC_RADIO_STOP_PROCESSING_FILES, OnBtnStopProcessingWithEmptyQueue)
	ON_LBN_SELCHANGE(IDC_LIST_FP, OnSelchangeListFileProcessors)
	ON_LBN_DBLCLK(IDC_LIST_FP, OnDblclkListFileProcessors)
	ON_WM_SIZE()
	ON_BN_CLICKED(IDC_CHECK_LOG_ERROR_DETAILS, OnCheckLogErrorDetails)
	ON_EN_CHANGE(IDC_EDIT_ERROR_LOG, &FileProcessingDlgTaskPage::OnEnChangeEditErrorLog)
	ON_BN_CLICKED(IDC_BTN_SELECT_DOC_TAG, OnBtnErrorSelectDocTag)
	ON_BN_CLICKED(IDC_BTN_BROWSE_LOG, OnBtnBrowseErrorLog)
	ON_BN_CLICKED(IDC_CHECK_EXECUTE_TASK, OnCheckExecuteErrorTask)
	ON_BN_CLICKED(IDC_BTN_SELECT_ERROR_TASK, OnBtnAddErrorTask)
	ON_NOTIFY(NM_DBLCLK, IDC_LIST_FP, &FileProcessingDlgTaskPage::OnNMDblclkListFp)
	ON_NOTIFY(NM_RCLICK, IDC_LIST_FP, &FileProcessingDlgTaskPage::OnNMRclickListFp)
	ON_COMMAND(ID_CONTEXT_CUT, &FileProcessingDlgTaskPage::OnContextCut)
	ON_COMMAND(ID_CONTEXT_COPY, &FileProcessingDlgTaskPage::OnContextCopy)
	ON_COMMAND(ID_CONTEXT_PASTE, &FileProcessingDlgTaskPage::OnContextPaste)
	ON_COMMAND(ID_CONTEXT_DELETE, &FileProcessingDlgTaskPage::OnContextDelete)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_FP, &FileProcessingDlgTaskPage::OnLvnItemchangedListFp)
	ON_EN_CHANGE(IDC_EDIT_THREADS, &FileProcessingDlgTaskPage::OnEnChangeEditThreads)
	ON_BN_CLICKED(IDC_RADIO_PROCESS_ALL_FILES_PRIORITY, &FileProcessingDlgTaskPage::OnBtnProcessAllOrSkipped)
	ON_BN_CLICKED(IDC_RADIO_PROCESS_SKIPPED_FILES, &FileProcessingDlgTaskPage::OnBtnProcessAllOrSkipped)
	ON_CBN_SELCHANGE(IDC_COMBO_SKIPPED_SCOPE, &FileProcessingDlgTaskPage::OnComboSkippedChange)
	ON_BN_CLICKED(IDC_CHECK_LIMIT_PROCESSING, &FileProcessingDlgTaskPage::OnBtnClickedCheckLimitProcessing)
	ON_BN_CLICKED(IDC_BUTTON_SET_SCHEDULE, &FileProcessingDlgTaskPage::OnBnClickedButtonSetSchedule)
	ON_WM_LBUTTONDBLCLK()
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgTaskPage message handlers
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlgTaskPage::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CPropertyPage::OnInitDialog();
		
		//create instances of necessary variables
		m_ipClipboardMgr.CreateInstance(CLSID_ClipboardObjectManager);
		ASSERT_RESOURCE_ALLOCATION("ELI18616", m_ipClipboardMgr != NULL);

		// load icons for up and down buttons
		m_btnUp.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_UP)));
		m_btnDown.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_DOWN)));

		//Set the Edit Threads box to be controlled by the spin control.
		CEdit *pEdit;
		pEdit = (CEdit *)GetDlgItem( IDC_EDIT_THREADS );
		m_SpinThreads.SetBuddy(pEdit);
		m_SpinThreads.SetRange32(0, giTHREADS_UPPER_RANGE);

		// Enable full row selection plus grid lines and checkboxes
		m_fileProcessorList.SetExtendedStyle( LVS_EX_GRIDLINES | 
			LVS_EX_FULLROWSELECT | LVS_EX_CHECKBOXES );

		//////////////////
		// Prepare headers
		//////////////////
		// Get dimensions of control
		CRect	rect;
		m_fileProcessorList.GetClientRect( &rect );

		// Compute width for Description column
		long	lDWidth = rect.Width() - glENABLED_WIDTH;

		// Add 2 column headings to list
		m_fileProcessorList.InsertColumn( 0, "Run", LVCFMT_LEFT, 
			glENABLED_WIDTH, 0 );
		m_fileProcessorList.InsertColumn( 1, "Task", LVCFMT_LEFT, 
			lDWidth, 1 );

		// Load icon for Doc Tag selection
		m_btnErrorSelectTag.SetIcon( ::LoadIcon( _Module.m_hInstResource, 
			MAKEINTRESOURCE( IDI_ICON_SELECT_DOC_TAG ) ) );

		// Set the item list for the skipped combo box
		m_comboSkipped.AddString("I");
		m_comboSkipped.AddString("Anyone");
		m_comboSkipped.SetCurSel(0);

		// Set radio buttons and update button states
		refresh();
		setButtonStates();

		// Set m_bInitialized to true so that 
		// next call to OnSize() will not be skipped
		m_bInitialized = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13504")
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnAdd()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// create a new ObjectWithDescription
		IObjectWithDescriptionPtr ipObject(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI16051", ipObject != NULL);

		// allow the user to select and configure
		VARIANT_BOOL vbDirty = getMiscUtils()->AllowUserToSelectAndConfigureObject(ipObject, 
			"Task",	get_bstr_t(FP_FILE_PROC_CATEGORYNAME), VARIANT_FALSE, 0, NULL);

		// check OK was selected
		if (vbDirty)
		{
			// Get index of previously selected File Processor
			int iIndex = -1;
			POSITION pos = m_fileProcessorList.GetFirstSelectedItemPosition();
			if (pos != NULL)
			{
				// Get index of first selection
				iIndex = m_fileProcessorList.GetNextSelectedItem( pos );

				// Insert position is after the first selected item (P13 #4732)
				iIndex++;
			}

			// If no current selection, insert item at end of list
			if (iIndex == -1)
			{
				iIndex = m_fileProcessorList.GetItemCount();
			}

			// Insert the object-with-description into the vector and refresh the list
			getFileProcessorsData()->Insert( iIndex, ipObject );
			refresh();

			// clear the previous selection if any
			clearListSelection();

			// Retain selection and focus
			m_fileProcessorList.SetItemState( iIndex, LVIS_SELECTED, LVIS_SELECTED );
			m_fileProcessorList.SetFocus();

			// Update the display
			UpdateData( FALSE );

			// Update button states
			setButtonStates();

			// Update UI, menu and toolbar items
			updateUI();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13481")
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnRemove() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Check for current selection
		int iIndex = -1;
		POSITION pos = m_fileProcessorList.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			// Get index of first selection
			iIndex = m_fileProcessorList.GetNextSelectedItem( pos );
		}

		// Check for multiple selection
		int iIndex2 = m_fileProcessorList.GetNextSelectedItem( pos );

		// Handle single-selection case
		int		iResult;
		CString	zPrompt;
		if (iIndex2 == -1)
		{
			// Retrieve current file processor description
			CString	zDescription = m_fileProcessorList.GetItemText( iIndex, 1 );

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
		iResult = MessageBox( zPrompt.operator LPCTSTR(), "Confirm Delete", 
			MB_YESNO | MB_ICONQUESTION );

		// Act on response
		if (iResult == IDYES)
		{
			// Mark selected items for deletion
			markSelectedTasks();

			// Delete the marked file processors
			deleteMarkedTasks();

			// Select the next (or the last) file processor
			int iCount = m_fileProcessorList.GetItemCount();
			if (iCount <= iIndex)
			{
				iIndex = iCount - 1;
			}
		}

		// Retain selection and focus
		m_fileProcessorList.SetItemState( iIndex, LVIS_SELECTED, LVIS_SELECTED );
		m_fileProcessorList.SetFocus();

		// Refresh the display
		UpdateData( TRUE );

		// Update button states
		setButtonStates();

		// Update UI, menu and toolbar items
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13505")
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnModify() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Check for current file processor selection
		int iIndex = -1;
		POSITION pos = m_fileProcessorList.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			// Get index of first selection
			iIndex = m_fileProcessorList.GetNextSelectedItem( pos );
		}

		if (iIndex > -1)
		{
			// get the current file processor
			IObjectWithDescriptionPtr	ipFP = getFileProcessorsData()->At( iIndex );
			ASSERT_RESOURCE_ALLOCATION("ELI15896", ipFP != NULL);
			
			// get the position and dimensions of the command button
			RECT rectCommandButton;
			getDlgItemWindowRect(IDC_BTN_MODIFY, rectCommandButton);

			// allow the user to modify the file processor
			VARIANT_BOOL vbDirty = getMiscUtils()->HandlePlugInObjectCommandButtonClick(ipFP, 
				"Task",	get_bstr_t(FP_FILE_PROC_CATEGORYNAME), VARIANT_FALSE, 0, NULL, 
				rectCommandButton.right, rectCommandButton.top);

			// Check result
			if (vbDirty == VARIANT_TRUE)
			{
				// update the file processor list box
				replaceFileProcessorAt(iIndex, ipFP);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13486")
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnDown()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Check for current file processor selection
		int iIndex = -1;
		POSITION pos = m_fileProcessorList.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			// Get index of first selection
			iIndex = m_fileProcessorList.GetNextSelectedItem( pos );
		}

		// Selection cannot be at bottom of list
		int iCount = m_fileProcessorList.GetItemCount();
		if (iIndex < iCount - 1)
		{
			// Update the file processors vector and refresh the list
			getFileProcessorsData()->Swap( iIndex, iIndex + 1 );
			refresh();

			// Retain selection and focus
			m_fileProcessorList.SetItemState( iIndex + 1, LVIS_SELECTED, LVIS_SELECTED );
			m_fileProcessorList.SetFocus();

			// Update button states
			setButtonStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13506")
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnUp() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Check for current file processor selection
		int iIndex = -1;
		POSITION pos = m_fileProcessorList.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			// Get index of first selection
			iIndex = m_fileProcessorList.GetNextSelectedItem( pos );
		}

		// Selection cannot be at top of list
		if (iIndex > 0)
		{
			// Update the file processors vector and refresh the list
			getFileProcessorsData()->Swap( iIndex, iIndex - 1 );
			refresh();

			// Retain selection and focus
			m_fileProcessorList.SetItemState( iIndex - 1, LVIS_SELECTED, LVIS_SELECTED );
			m_fileProcessorList.SetFocus();

			// Update button states
			setButtonStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13507")
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnSelchangeListFileProcessors() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Update button states
		setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13508")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnDblclkListFileProcessors() 
{
	// call modify for the selected item
	OnBtnModify();
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnCheckLogErrorDetails()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		UpdateData(TRUE);

		// Provide new setting to File Processing Mgmt Role object
		getFPMgmtRole()->LogErrorDetails = asVariantBool( m_bLogErrorDetails );

		// Enable / disable controls as appropriate
		setButtonStates();
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16043")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnEnChangeEditErrorLog()
{
	try
	{
		UpdateData(TRUE);

		// Update the error log
		getFPMgmtRole()->ErrorLogName = get_bstr_t( m_zErrorLog );

		// Enable / disable controls as appropriate
		setButtonStates();
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16089");
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnErrorSelectDocTag()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Retrieve position of doc tag button
		RECT rect;
		m_btnErrorSelectTag.GetWindowRect(&rect);

		// Save the location of the current edit selection
		m_dwSel = m_editErrorLog.GetSel();

		// Display menu and make selection
		std::string strChoice = CFileProcessingUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);

		if (strChoice != "")
		{
			// Retrieve current text in error log edit box
			CString zWindowText;
			m_editErrorLog.GetWindowText( zWindowText );

			// Remove any selected text and replace with selected doc tag string
			zWindowText.Delete(LOWORD(m_dwSel), HIWORD(m_dwSel) - LOWORD(m_dwSel) );
			zWindowText.Insert(LOWORD(m_dwSel), strChoice.c_str());
			m_editErrorLog.SetWindowText(zWindowText);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16079")
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnBrowseErrorLog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Set up supported file types for log file
//		CString zFileType("UEX files (*.uex)|*.uex|Text Files (*.txt)|*.txt||");
		CString zFileType("UEX files (*.uex)|*.uex||");

		// Display the browse dialog and handle the result
		CFileDialog dlg(TRUE, ".uex", NULL, OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			zFileType, this);
		if (dlg.DoModal() == IDOK)
		{
			// Retrieve file name
			m_zErrorLog = dlg.GetPathName();
			UpdateData(FALSE);

			getFPMgmtRole()->ErrorLogName = get_bstr_t(m_zErrorLog);

			// Update menu items
			updateUI();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16031")
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnCheckExecuteErrorTask()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		UpdateData(TRUE);

		// Provide new setting to File Processing Mgmt Role object
		getFPMgmtRole()->ExecuteErrorTask = asVariantBool( m_bExecuteErrorTask );

		// Enable / disable controls as appropriate
		setButtonStates();
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16045")
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnAddErrorTask()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// retrieve the error task
		IObjectWithDescriptionPtr ipOWD = getFPMgmtRole()->ErrorTask;
		ASSERT_RESOURCE_ALLOCATION("ELI16111", ipOWD != NULL);

		// get position and dimensions of error task command button
		RECT rectCommandButton;
		getDlgItemWindowRect(IDC_BTN_SELECT_ERROR_TASK, rectCommandButton);

		// allow user to select or configure the error task
		VARIANT_BOOL vbDirty = getMiscUtils()->HandlePlugInObjectCommandButtonClick(
			getFPMgmtRole()->ErrorTask, "Task", get_bstr_t(FP_FILE_PROC_CATEGORYNAME), 
			VARIANT_TRUE, 0, NULL, rectCommandButton.right, rectCommandButton.top);

		// Check result
		if (vbDirty == VARIANT_TRUE)
		{
			// Update the description
			m_zErrorTaskDescription = ipOWD->Description.operator const char *();
			
			if (ipOWD->Object == NULL)
			{
				// If the user selected "<NONE>", disable the error task
				m_bExecuteErrorTask = false;
				getFPMgmtRole()->ExecuteErrorTask = asVariantBool(m_bExecuteErrorTask);
			}

			// Temporary solution to set the MgmtRole's dirty flag. (This call is not otherwise necessary)
			// The long run solution should be that all ICopyableObjects set their dirty flag in CopyFrom.
			// See P13:4627
			getFPMgmtRole()->ErrorTask = ipOWD;

			UpdateData( FALSE );
		}

		// Enable / disable controls as appropriate
		setButtonStates();
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16048")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnSize(UINT nType, int cx, int cy) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CPropertyPage::OnSize(nType, cx, cy);

		// first call to this function shall be ignored
		if (!m_bInitialized) 
		{
			return;
		}

		// Declare height values and rectangles
		static bool bInit = false;
		static int nLen1, nLen2, nLen3, nLen4, nLen5, nAddButtonWidth, nUpWidth;
		static int nKeepProcessingAsAddedHeight = 0;
		CRect rectDlg, rectDownButton, rectList;
		CRect rectAddButton, rectUpButton, rectDeleteButton, rectModifyButton;
		CRect rectErrorGrp, rectCheckLogError, rectEditErrorLog, rectCheckExecuteTask, rectEditExecuteTask, 
			rectDocTag, rectBrowse, rectSelect;
		CRect rectProcessScopeGrp, rectProcessAll, rectProcessSkipped, rectComboSkippedScope, rectSkippedText;
		CRect rectContinuousGrp, rectKeepProc, rectStopProc;
		CRect rectThreadGrp, rectMaxThreadOption, rectThreadOption, rectEditThreads, rectThreadTxt, rectSpinBtn;
		CRect rectScheduleGrp, rectLimitCheck, rectScheduleBtn;

		// Get positions of list and buttons
		getDlgItemWindowRect(IDC_LIST_FP, rectList);
		ScreenToClient(&rectList);
		getDlgItemWindowRect(IDC_BTN_ADD, rectAddButton);
		ScreenToClient(&rectAddButton);
		getDlgItemWindowRect(IDC_BTN_REMOVE, rectDeleteButton);
		ScreenToClient(&rectDeleteButton);
		getDlgItemWindowRect(IDC_BTN_MODIFY, rectModifyButton);
		ScreenToClient(&rectModifyButton);			
		getDlgItemWindowRect(IDC_BTN_UP, rectUpButton);
		ScreenToClient(&rectUpButton);
		getDlgItemWindowRect(IDC_BTN_DOWN, rectDownButton);
		ScreenToClient(&rectDownButton);

		// Get positions of Error group
		getDlgItemWindowRect(IDC_STATIC_ERROR_GROUP, rectErrorGrp);
		ScreenToClient(&rectErrorGrp);
		getDlgItemWindowRect(IDC_CHECK_LOG_ERROR_DETAILS, rectCheckLogError);
		ScreenToClient(&rectCheckLogError);
		getDlgItemWindowRect(IDC_EDIT_ERROR_LOG, rectEditErrorLog);
		ScreenToClient(&rectEditErrorLog);
		getDlgItemWindowRect(IDC_BTN_SELECT_DOC_TAG, rectDocTag);
		ScreenToClient(&rectDocTag);
		getDlgItemWindowRect(IDC_BTN_BROWSE_LOG, rectBrowse);
		ScreenToClient(&rectBrowse);
		getDlgItemWindowRect(IDC_CHECK_EXECUTE_TASK, rectCheckExecuteTask);
		ScreenToClient(&rectCheckExecuteTask);
		getDlgItemWindowRect(IDC_EDIT_EXECUTE_TASK, rectEditExecuteTask);
		ScreenToClient(&rectEditExecuteTask);
		getDlgItemWindowRect(IDC_BTN_SELECT_ERROR_TASK, rectSelect);
		ScreenToClient(&rectSelect);

		// Get positions of the Process Scope group items
		getDlgItemWindowRect(IDC_STATIC_FPSCOPE_GROUP, rectProcessScopeGrp);
		ScreenToClient(&rectProcessScopeGrp);
		getDlgItemWindowRect(IDC_RADIO_PROCESS_ALL_FILES_PRIORITY, rectProcessAll);
		ScreenToClient(&rectProcessAll);
		getDlgItemWindowRect(IDC_RADIO_PROCESS_SKIPPED_FILES, rectProcessSkipped);
		ScreenToClient(&rectProcessSkipped);
		getDlgItemWindowRect(IDC_COMBO_SKIPPED_SCOPE, rectComboSkippedScope);
		ScreenToClient(&rectComboSkippedScope);
		getDlgItemWindowRect(IDC_STATIC_SKIPPED, rectSkippedText);
		ScreenToClient(&rectSkippedText);

		// Get positions of the Processing schedule group
		getDlgItemWindowRect(IDC_STATIC_PROCESSING_SCHEDULE, rectScheduleGrp);
		ScreenToClient(&rectScheduleGrp);
		getDlgItemWindowRect(IDC_CHECK_LIMIT_PROCESSING, rectLimitCheck);
		ScreenToClient(&rectLimitCheck);
		getDlgItemWindowRect(IDC_BUTTON_SET_SCHEDULE, rectScheduleBtn);
		ScreenToClient(&rectScheduleBtn);

		// Get positions of Continuous Processing group items
		getDlgItemWindowRect(IDC_STATIC_CONTINUOUS_GROUP, rectContinuousGrp);
		ScreenToClient(&rectContinuousGrp);
		getDlgItemWindowRect(IDC_RADIO_KEEP_PROCESSING_FILES, rectKeepProc);
		ScreenToClient(&rectKeepProc);
		getDlgItemWindowRect(IDC_RADIO_STOP_PROCESSING_FILES, rectStopProc);
		ScreenToClient(&rectStopProc);

		// Get positions of Threads group items
		getDlgItemWindowRect(IDC_STATIC_THREAD_GROUP, rectThreadGrp);
		ScreenToClient(&rectThreadGrp);
		getDlgItemWindowRect(IDC_RADIO_MAX_THREADS, rectMaxThreadOption);
		ScreenToClient(&rectMaxThreadOption);
		getDlgItemWindowRect(IDC_RADIO_THREADS, rectThreadOption);
		ScreenToClient(&rectThreadOption);
		getDlgItemWindowRect(IDC_EDIT_THREADS, rectEditThreads);
		ScreenToClient(&rectEditThreads);
		getDlgItemWindowRect(IDC_SPIN_THREADS, rectSpinBtn);
		ScreenToClient(&rectSpinBtn);
		getDlgItemWindowRect(IDC_STATIC_THREADS, rectThreadTxt);
		ScreenToClient(&rectThreadTxt);

		// Set values for static items
		if (!bInit)
		{
			GetClientRect(&rectDlg);

			// distance from right of the Add button to the right of the dialog
			// should be the same as the distance from left of List to left of Dialog
			nLen1 = rectList.left - rectDlg.left;

			// distance in between Up and Down button
			nLen2 = rectDownButton.left - rectUpButton.right;

			// distance in between Add button and the list box
			nLen3 = rectAddButton.left - rectList.right;

			// distance between group boxes
			nLen4 = rectErrorGrp.top - rectList.bottom;

			// distance between group box and first contained control
			nLen5 = rectProcessAll.top - rectProcessScopeGrp.top;

			nAddButtonWidth = rectAddButton.Width();
			nUpWidth = rectUpButton.Width();
			bInit = true;
		}
		
		// get dialog rect
		GetClientRect(&rectDlg);

		// resize list box width
		rectList.right = rectDlg.right - nLen1 - nAddButtonWidth - nLen3;

		// set the width of the second column to reflect the width of the list control
		// We are subtracting 4 pixels here for the left border line, right border line, and
		// the two column dividing lines.
		m_fileProcessorList.SetColumnWidth(1, rectList.Width() - glENABLED_WIDTH - 4 );

		// Resize buttons
		rectAddButton.right = rectDlg.right - nLen1;
		rectAddButton.left = rectAddButton.right - nAddButtonWidth;
		rectDeleteButton.right = rectDlg.right - nLen1;
		rectDeleteButton.left = rectDeleteButton.right - nAddButtonWidth;
		rectModifyButton.right = rectDlg.right - nLen1;
		rectModifyButton.left = rectModifyButton.right - nAddButtonWidth;
		rectDownButton.right = rectDlg.right - nLen1;
		rectDownButton.left = rectDownButton.right - nUpWidth;
		rectUpButton.right = rectDownButton.left - nLen2;
		rectUpButton.left = rectUpButton.right - nUpWidth;

		// Resize Continuous Processing group items
		int nWidth = rectThreadGrp.Width();
		int nGroupHeight = rectContinuousGrp.Height();
		rectContinuousGrp.bottom = rectDlg.bottom - nLen4;
		rectContinuousGrp.top = rectContinuousGrp.bottom - nGroupHeight;
		rectContinuousGrp.right = rectList.right - (nWidth + nLen4);

		int nHeight = rectKeepProc.Height();
		int nSpace = rectStopProc.top - rectKeepProc.bottom;
		rectKeepProc.top = rectContinuousGrp.top + nLen5;
		rectKeepProc.bottom = rectKeepProc.top + nHeight;
		rectStopProc.top = rectKeepProc.bottom + nSpace;
		rectStopProc.bottom = rectStopProc.top + nHeight;

		// Resize Threads group items
		int nShiftRight = rectList.right - rectThreadGrp.right;
		nGroupHeight = rectThreadGrp.Height();
		rectThreadGrp.bottom = rectContinuousGrp.bottom;
		rectThreadGrp.top = rectContinuousGrp.top;
		rectThreadGrp.right = rectList.right;
		rectThreadGrp.left = rectThreadGrp.right - nWidth;

		nHeight = rectMaxThreadOption.Height();
		nSpace = rectThreadOption.top - rectMaxThreadOption.bottom;
		rectMaxThreadOption.OffsetRect(nShiftRight, 0);
		rectMaxThreadOption.top = rectThreadGrp.top + nLen5;
		rectMaxThreadOption.bottom = rectMaxThreadOption.top + nHeight;

		nHeight = rectThreadOption.Height();
		rectThreadOption.top = rectMaxThreadOption.bottom + nSpace;
		rectThreadOption.bottom = rectThreadOption.top + nHeight;
		rectThreadOption.OffsetRect(nShiftRight, 0);
		nHeight = rectThreadTxt.Height();
		rectThreadTxt.top = rectThreadOption.top;
		rectThreadTxt.bottom = rectThreadOption.top + nHeight;
		rectThreadTxt.OffsetRect(nShiftRight, 0);

		nHeight = rectEditThreads.Height();
		rectEditThreads.top = rectThreadTxt.top + rectThreadTxt.Height()/2 - nHeight/2;
		rectEditThreads.bottom = rectEditThreads.top + nHeight;
		rectEditThreads.OffsetRect(nShiftRight, 0);
		nSpace = rectSpinBtn.Width();
		rectSpinBtn.left = rectEditThreads.right;
		rectSpinBtn.right = rectSpinBtn.left + nSpace;
		rectSpinBtn.top = rectEditThreads.top;
		rectSpinBtn.bottom = rectEditThreads.bottom;

		// Resize processing scope group items
		nGroupHeight = rectProcessScopeGrp.Height();
		rectProcessScopeGrp.bottom = rectThreadGrp.top - nLen4;
		rectProcessScopeGrp.top = rectProcessScopeGrp.bottom - nGroupHeight;
		rectProcessScopeGrp.right = rectList.right - (rectThreadGrp.Width() + nLen4);

		nHeight = rectProcessAll.Height();
		nSpace = rectProcessSkipped.top - rectProcessAll.bottom;
		rectProcessAll.top = rectProcessScopeGrp.top + nLen5;
		rectProcessAll.bottom = rectProcessAll.top + nHeight;
		rectProcessSkipped.top = rectProcessAll.bottom + nSpace;
		rectProcessSkipped.bottom = rectProcessSkipped.top + nHeight;
		rectSkippedText.top = rectProcessSkipped.top;
		rectSkippedText.bottom = rectSkippedText.top + nHeight;
		
		// Resize the processing schedule controls
		rectScheduleGrp.top = rectProcessScopeGrp.top;
		rectScheduleGrp.bottom = rectProcessScopeGrp.bottom;
		rectScheduleGrp.right = rectList.right;
		rectScheduleGrp.left = rectThreadGrp.left;

		nWidth = rectLimitCheck.Width();
		nHeight = rectLimitCheck.Height();
		rectLimitCheck.left = rectScheduleGrp.left + nLen5;
		rectLimitCheck.right = rectLimitCheck.left + nWidth;
		rectLimitCheck.top = rectScheduleGrp.top + nLen5;
		rectLimitCheck.bottom = rectLimitCheck.top + nHeight;
		
		// Width of the line used for the group box
		static const int nGROUPBOX_LINE_WIDTH = 2;

		nWidth = rectScheduleBtn.Width();
		nHeight = rectScheduleBtn.Height();
		rectScheduleBtn.top = rectLimitCheck.bottom + nSpace;
		rectScheduleBtn.bottom = rectScheduleBtn.top + nHeight;
		rectScheduleBtn.right = rectScheduleGrp.right - nLen2 - nGROUPBOX_LINE_WIDTH;
		rectScheduleBtn.left = rectScheduleBtn.right - nWidth;

		// Move the combo box
		nHeight = rectComboSkippedScope.Height();
		nWidth = rectComboSkippedScope.Width();
		rectComboSkippedScope.top = rectProcessSkipped.top + rectProcessSkipped.Height()/2 - nHeight/2;
		rectComboSkippedScope.bottom = rectComboSkippedScope.top + nHeight;
		rectComboSkippedScope.left = rectProcessSkipped.right + nSpace;
		rectComboSkippedScope.right = rectComboSkippedScope.left + nWidth;

		// Move the skipped text now that the combo box has been moved
		nWidth = rectSkippedText.Width();
		rectSkippedText.left = rectComboSkippedScope.right + nSpace;
		rectSkippedText.right = rectSkippedText.left + nWidth;
 
		// Resize Error group items
		int nButtonToEditVerticalAdjustment = rectEditErrorLog.top - rectBrowse.top;
		nGroupHeight = rectErrorGrp.Height();
		nSpace = rectEditErrorLog.top - rectErrorGrp.top;
		int nSpace2 = rectCheckExecuteTask.top - rectErrorGrp.top;
		int nSpace3 = rectSelect.top - rectErrorGrp.top;
		rectErrorGrp.bottom = rectProcessScopeGrp.top - nLen4;
		rectErrorGrp.top = rectErrorGrp.bottom - nGroupHeight;
		rectErrorGrp.right =  rectList.right;

		nHeight = rectCheckLogError.Height();
		rectCheckLogError.top = rectErrorGrp.top + nLen5;
		rectCheckLogError.bottom = rectCheckLogError.top + nHeight;

		nHeight = rectEditErrorLog.Height();
		rectEditErrorLog.top = rectErrorGrp.top + nSpace;
		rectEditErrorLog.bottom = rectEditErrorLog.top + nHeight;

		nWidth = rectBrowse.Width();
		nHeight = rectBrowse.Height();
		rectBrowse.right = rectErrorGrp.right - nLen2 - nGROUPBOX_LINE_WIDTH;
		rectBrowse.left = rectBrowse.right - nWidth;
		rectBrowse.top = rectEditErrorLog.top - nButtonToEditVerticalAdjustment;
		rectBrowse.bottom = rectBrowse.top + nHeight;

		nWidth = rectDocTag.Width();
		rectDocTag.right = rectBrowse.left - nLen2;
		rectDocTag.left = rectDocTag.right - nWidth;
		rectDocTag.top = rectBrowse.top;
		rectDocTag.bottom = rectBrowse.bottom;
		rectEditErrorLog.right = rectDocTag.left - nLen2;

		nHeight = rectCheckExecuteTask.Height();
		rectCheckExecuteTask.top = rectErrorGrp.top + nSpace2;
		rectCheckExecuteTask.bottom = rectCheckExecuteTask.top + nHeight;

		nHeight = rectSelect.Height();
		nWidth = rectSelect.Width();
		rectSelect.right = rectErrorGrp.right - nLen2 - nGROUPBOX_LINE_WIDTH;
		rectSelect.left = rectSelect.right - nWidth;
		rectSelect.top = rectErrorGrp.top + nSpace3;
		rectSelect.bottom = rectSelect.top + nHeight;

		nHeight = rectEditExecuteTask.Height();
		rectEditExecuteTask.right = rectSelect.left - nLen2;
		rectEditExecuteTask.top = rectSelect.top + nButtonToEditVerticalAdjustment;
		rectEditExecuteTask.bottom = rectEditExecuteTask.top + nHeight;

		// Resize list box height
		rectList.bottom = rectErrorGrp.top - nLen4;

		// Move list and buttons
		m_fileProcessorList.MoveWindow(&rectList);
		m_btnAdd.MoveWindow(&rectAddButton);
		m_btnRemove.MoveWindow(&rectDeleteButton);
		m_btnModify.MoveWindow(&rectModifyButton);
		m_btnDown.MoveWindow(&rectDownButton);
		m_btnUp.MoveWindow(&rectUpButton);

		// Move Error group items
		GetDlgItem(IDC_STATIC_ERROR_GROUP)->MoveWindow(&rectErrorGrp);
		m_btnLogErrorDetails.MoveWindow(&rectCheckLogError);
		m_editErrorLog.MoveWindow(&rectEditErrorLog);
		m_btnErrorSelectTag.MoveWindow(&rectDocTag);
		m_btnBrowseErrorLog.MoveWindow(&rectBrowse);
		m_btnExecuteErrorTask.MoveWindow(&rectCheckExecuteTask);
		m_btnSelectErrorTask.MoveWindow(&rectSelect);
		GetDlgItem(IDC_EDIT_EXECUTE_TASK)->MoveWindow(&rectEditExecuteTask);

		// Move processing scope group items
		GetDlgItem(IDC_STATIC_FPSCOPE_GROUP)->MoveWindow(&rectProcessScopeGrp);
		m_radioProcessAll.MoveWindow(&rectProcessAll);
		m_radioProcessSkipped.MoveWindow(&rectProcessSkipped);
		m_comboSkipped.MoveWindow(&rectComboSkippedScope);
		GetDlgItem(IDC_STATIC_SKIPPED)->MoveWindow(&rectSkippedText);

		// Move Continuous Processing group items
		GetDlgItem(IDC_STATIC_CONTINUOUS_GROUP)->MoveWindow(&rectContinuousGrp);
		m_btnKeepProcessingWithEmptyQueue.MoveWindow(&rectKeepProc);
		m_btnStopProcessingWithEmptyQueue.MoveWindow(&rectStopProc);

		// Move Thread group items
		GetDlgItem(IDC_STATIC_THREAD_GROUP)->MoveWindow(&rectThreadGrp);
		m_btnMaxThreads.MoveWindow(&rectMaxThreadOption);
		m_btnNumThreads.MoveWindow(&rectThreadOption);
		m_editThreads.MoveWindow(&rectEditThreads);
		GetDlgItem(IDC_STATIC_THREADS)->MoveWindow(&rectThreadTxt);
		m_SpinThreads.MoveWindow(&rectSpinBtn);

		// Move process schedule items
		m_groupProcessingSchedule.MoveWindow(&rectScheduleGrp);
		m_checkLimitProcessing.MoveWindow(&rectLimitCheck);
		m_btnSetSchedule.MoveWindow(&rectScheduleBtn);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13509")
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnMaxThread() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		getFPMgmtRole()->NumThreads = 0;
		setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13401");
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnNumThread()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// if the editbox is empty, default its value to 1
		CString zText;
		m_editThreads.GetWindowTextA(zText);
		if (zText.IsEmpty())
		{
			m_editThreads.SetWindowText("1");
		}

		// Provide number of threads to the MgmtRole object (P13 #4312)
		int nNumThreads = getNumThreads();
		getFPMgmtRole()->NumThreads = nNumThreads;

		// Enable and disable controls as appropriate
		setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13402");
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnKeepProcessingWithEmptyQueue()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Update the File Proceesing Management Role setting
		getFPMgmtRole()->KeepProcessingAsAdded = VARIANT_TRUE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15952");
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnStopProcessingWithEmptyQueue()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Update the File Proceesing Management Role setting
		getFPMgmtRole()->KeepProcessingAsAdded = VARIANT_FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15953");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnNMDblclkListFp(NMHDR *pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// get the current file processor selection
		int iIndex = -1;
		POSITION pos = m_fileProcessorList.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			// Get index of first selection
			iIndex = m_fileProcessorList.GetNextSelectedItem(pos);
		}

		if (iIndex > -1)
		{
			// retrieve the selected file processor
			IObjectWithDescriptionPtr ipObject = getFileProcessorsData()->At(iIndex);
			ASSERT_RESOURCE_ALLOCATION("ELI16094", ipObject != NULL);

			// allow the user to modify ipObject
			VARIANT_BOOL vbDirty = getMiscUtils()->HandlePlugInObjectDoubleClick(ipObject,
				"Task", get_bstr_t(FP_FILE_PROC_CATEGORYNAME), VARIANT_FALSE, 0, NULL);

			// check if ipObject has been changed
			if(vbDirty == VARIANT_TRUE)
			{
				// insert the modified file processor into the list box
				replaceFileProcessorAt(iIndex, ipObject);
			}
		}

		*pResult = 0;

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13490")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnNMRclickListFp(NMHDR *pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	// Check for current selection
		if (pNMHDR)
		{
			int iIndex = -1;
			POSITION pos = m_fileProcessorList.GetFirstSelectedItemPosition();
			if (pos != NULL)
			{
				// Get index of first selection
				iIndex = m_fileProcessorList.GetNextSelectedItem( pos );
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
			
			// enable/disable context menu items properly
			pContextMenu->EnableMenuItem(ID_CONTEXT_CUT, iIndex == -1 ? nDisable : nEnable);
			pContextMenu->EnableMenuItem(ID_CONTEXT_COPY, iIndex == -1 ? nDisable : nEnable );
			pContextMenu->EnableMenuItem(ID_CONTEXT_DELETE, iIndex == -1 ? nDisable : nEnable );
			
			// Check Clipboard object type
			if(m_ipClipboardMgr != NULL &&
				(asCppBool(m_ipClipboardMgr->IUnknownVectorIsOWDOfType(IID_IFileProcessingTask)) ||
				asCppBool(m_ipClipboardMgr->ObjectIsTypeWithDescription(IID_IFileProcessingTask))))
			{
				// Object is a vector of IFileProcessingTasks OR Object is a
				// single IFileProcessingTask
				pContextMenu->EnableMenuItem( ID_CONTEXT_PASTE, nEnable);
			}
			else
			{
				// The clipboard manager is either NULL OR Object is
				// neither a vector of IFileProcessingTask items
				// nor a single IFileProcessingTask item
				pContextMenu->EnableMenuItem( ID_CONTEXT_PASTE, nDisable );
			}
			
			// Map the point to the correct position
			CPoint	point;
			GetCursorPos( &point );
			
			// Display and manage the context menu
			pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
				point.x, point.y, this );
		}
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13492")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnContextCut()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		if (m_ipClipboardMgr)
		{
			//put the info on the clipboard
			OnContextCopy();

			//remove the info from the list
			OnBtnRemove();

			//Update the button states
			setButtonStates();
		}
		else
		{
			throw UCLIDException("ELI18624", "There is no clipboard manager allocated!");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15353");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnContextCopy()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		if (m_ipClipboardMgr)
		{
			// Check for current selection
			int iIndex = -1;
			POSITION pos = m_fileProcessorList.GetFirstSelectedItemPosition();
			if (pos != NULL)
			{
				// Get index of first selection
				iIndex = m_fileProcessorList.GetNextSelectedItem( pos );
			}

			if (iIndex == -1)
			{
				// Throw exception
				throw UCLIDException( "ELI13500", 
					"Unable to determine selected File Processor!" );
			}

			// Create a vector for selected File Processors
			IIUnknownVectorPtr	ipCopiedFPs( CLSID_IUnknownVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI13501", ipCopiedFPs != NULL );

			// Add each selected to vector
			while (iIndex != -1)
			{
				// Retrieve the selected
				IUnknownPtr	ipObject = getFileProcessorsData()->At( iIndex );
				ASSERT_RESOURCE_ALLOCATION( "ELI13502", ipObject != NULL );

				// Add the File Processor to the vector
				ipCopiedFPs->PushBack( ipObject );

				// Get the next selection
				iIndex = m_fileProcessorList.GetNextSelectedItem( pos );
			}

			// ClipboardManager will handle the Copy
			m_ipClipboardMgr->CopyObjectToClipboard( ipCopiedFPs );
		}
		else
		{
			throw UCLIDException("ELI18625", "There is no clipboard manager allocated!");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15351");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnContextPaste()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Test ClipboardManager object
		IUnknownPtr	ipObject( NULL );
		bool	bSingleTask = false;
		if (m_ipClipboardMgr->ObjectIsIUnknownVectorOfType(
			IID_IObjectWithDescription ))
		{
			// Object is a vector of ObjectWithDescription items
			// We expect each embedded object to be a File Processor
			ipObject = m_ipClipboardMgr->GetObjectInClipboard();
			ASSERT_RESOURCE_ALLOCATION( "ELI13494", ipObject != NULL );
		}
		else if (m_ipClipboardMgr->ObjectIsTypeWithDescription( 
			IID_IObjectWithDescription  ))
		{
			// Object is a single ObjectWithDescription item
			// We expect the embedded object to be a File Processor
			ipObject = m_ipClipboardMgr->GetObjectInClipboard();
			ASSERT_RESOURCE_ALLOCATION( "ELI13495", ipObject != NULL );
			bSingleTask = true;
		}
		else
		{
			// Throw exception, object is not a vector of ObjectWithDescription items
			throw UCLIDException( "ELI13496", 
				"Clipboard object is not a File Processor." );
		}

		// Check for current File Processor selection
		int iIndex = -1;
		POSITION pos = m_fileProcessorList.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			// Get index of first selection
			iIndex = m_fileProcessorList.GetNextSelectedItem( pos );
		}

		// Check for item count if no selection
		if (iIndex == -1)
		{
			iIndex = m_fileProcessorList.GetItemCount();
		}

		clearListSelection();

		// Handle single-task case
		if (bSingleTask)
		{
			// Retrieve File Processor and description
			IObjectWithDescriptionPtr	ipNewFP = ipObject;
			ASSERT_RESOURCE_ALLOCATION( "ELI13497", ipNewFP != NULL );
			string	strDescription( ipNewFP->GetDescription() );

			// Insert the new FP object-with-description into the vector
			// This MUST be done before setting the checked state in the list
			// [LRCAU #5603]
			getFileProcessorsData()->Insert( iIndex, ipNewFP );

			// Insert the item into the list
			m_fileProcessorList.InsertItem( iIndex, "" );

			// Add the description and update the checkbox setting
			m_fileProcessorList.SetItemText( iIndex, 1, strDescription.c_str() );
			m_fileProcessorList.SetCheck( iIndex, asMFCBool( ipNewFP->Enabled ) );

			// Select the new item
			m_fileProcessorList.SetItemState( iIndex, LVIS_SELECTED, LVIS_SELECTED );
		}
		// Handle vector of one-or-more File Processors case
		else
		{
			// Get count of File Processors in Clipboard vector
			IIUnknownVectorPtr	ipPastedFPs = ipObject;
			ASSERT_RESOURCE_ALLOCATION( "ELI13498", ipPastedFPs != NULL );
			int iCount = ipPastedFPs->Size();

			// Add each File Processor to the list and the vector
			for (int i = 0; i < iCount; i++)
			{
				// Retrieve File Processor and description
				IObjectWithDescriptionPtr	ipNewFP = ipPastedFPs->At( i );
				ASSERT_RESOURCE_ALLOCATION( "ELI13499", ipNewFP != NULL );
				string	strDescription( ipNewFP->GetDescription() );

				// Insert the new File Processor object-with-description into the vector
				// This MUST be done before setting the checked state in the list
				// [LRCAU #5603]
				getFileProcessorsData()->Insert( iIndex + i, ipNewFP );

				// Insert the item into the list
				m_fileProcessorList.InsertItem( iIndex + i, "" );

				// Add the description and update the checkbox setting
				m_fileProcessorList.SetItemText( iIndex + i, 1, strDescription.c_str() );
				m_fileProcessorList.SetCheck( iIndex + i, asMFCBool( ipNewFP->Enabled ) );

				// select the new item
				m_fileProcessorList.SetItemState( iIndex+i, LVIS_SELECTED, LVIS_SELECTED );
			}
		}

		// Update the button states
		setButtonStates();

		// // Update UI, menu and toolbar items
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15352");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnContextDelete()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// delete selected items via context menu
		OnBtnRemove();	
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15354");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnLvnItemchangedListFp(NMHDR *pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

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

		// If this file processor is already present
		IIUnknownVectorPtr ipCollection = getFileProcessorsData();
		ASSERT_RESOURCE_ALLOCATION("ELI15995", ipCollection != NULL);
		if (ipCollection->Size() > pNMLV->iItem)
		{
			// Retrieve affected file processor
			IObjectWithDescriptionPtr	ipFP = getFileProcessorsData()->At( pNMLV->iItem );
			ASSERT_RESOURCE_ALLOCATION("ELI15983", ipFP != NULL);

			// Retrieve existing state
			VARIANT_BOOL vbExisting = ipFP->Enabled;

			// Provide new checkbox state to file processor object
			if (!isEqual(vbExisting, bChecked))
			{
				ipFP->Enabled = asVariantBool(bChecked);
			}
			// else Enabled flag already matches the checkbox
		}

		// Update menu items as needed
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13503");

	*pResult = 0;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnLButtonDblClk(UINT nFlags, CPoint point)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Handling the Left button dbl click on the property page was implemented instead
		// of having the different methods for the controls, to fix the issue with double click 
		// copying the label contents to the clipboard FlexIDSCore #4227

		// [LegacyRCAndUtils:5879]
		if (!m_bEnabled)
		{
			return;
		}

		// Get the child window the mouse was double clicked in
		CWnd *tmpChild = ChildWindowFromPoint(point, CWP_SKIPTRANSPARENT);

		// if the child was returned check if it is the execute task control
		if (tmpChild != NULL && tmpChild->GetDlgCtrlID() == IDC_EDIT_EXECUTE_TASK)
		{
			// get the error task
			IObjectWithDescriptionPtr ipErrorTask = getFPMgmtRole()->ErrorTask;
			ASSERT_RESOURCE_ALLOCATION("ELI16114", ipErrorTask != NULL);

			// select a file processor
			VARIANT_BOOL vbDirty = getMiscUtils()->HandlePlugInObjectDoubleClick(
				getFPMgmtRole()->ErrorTask, "Task", get_bstr_t(FP_FILE_PROC_CATEGORYNAME), 
				VARIANT_TRUE, 0, NULL);

			// check result
			if (vbDirty == VARIANT_TRUE)
			{
				// Update the description
				m_zErrorTaskDescription = ipErrorTask->Description.operator const char *();

				if (ipErrorTask->Object == NULL)
				{
					// If the user selected "<NONE>", disable the error task
					m_bExecuteErrorTask = false;
					getFPMgmtRole()->ExecuteErrorTask = asVariantBool(m_bExecuteErrorTask);
				}

				// Temporary solution to set the MgmtRole's dirty flag. (This call is not otherwise necessary)
				// The long run solution should be that all ICopyableObjects set their dirty flag in CopyFrom.
				// See P13:4627
				getFPMgmtRole()->ErrorTask = ipErrorTask;

				UpdateData( FALSE );
			}

			// Enable / disable controls as appropriate
			setButtonStates();
			updateUI();
		}
		else
		{
			// Not in a control that needs special handling so call the base class method
			CPropertyPage::OnLButtonDblClk(nFlags, point);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16113");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnEnChangeEditThreads()
{
	try
	{
		// In order to prevent unnecessary "dirty" state in the FPM, set the # of threads
		// only if it is different from what the current value is.
		long nCurrNumThreads = getFPMgmtRole()->NumThreads;
		long nNewNumThreads = getNumThreads();
		if (nCurrNumThreads != nNewNumThreads)
		{
			getFPMgmtRole()->NumThreads = nNewNumThreads;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15356");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnProcessAllOrSkipped()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		if (m_radioProcessAll.GetCheck() == BST_CHECKED)
		{
			getFPMgmtRole()->ProcessSkippedFiles = VARIANT_FALSE;
		}
		else
		{
			// Set the mgmt role
			UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr ipMgmtRole = getFPMgmtRole();
			ipMgmtRole->ProcessSkippedFiles = VARIANT_TRUE;
			ipMgmtRole->SkippedForAnyUser =
				asVariantBool(m_comboSkipped.GetCurSel() == giCOMBO_INDEX_ANYONE);
		}

		setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26922");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnComboSkippedChange()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		getFPMgmtRole()->SkippedForAnyUser =
			asVariantBool(m_comboSkipped.GetCurSel() == giCOMBO_INDEX_ANYONE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26923");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnClickedCheckLimitProcessing()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		UpdateData(TRUE);

		// Set the Limit processing to scheduled flag
		getFPMgmtRole()->LimitProcessingToSchedule = asVariantBool(m_bLimitProcessingTimes);

		// Enable / disable controls as appropriate
		setButtonStates();
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28053");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBnClickedButtonSetSchedule()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		if (m_ipSchedule == NULL)
		{
			m_ipSchedule = getFPMgmtRole()->ProcessingSchedule;
		}

		// Get the set schedule COM object
		UCLID_FILEPROCESSINGLib::ISetProcessingSchedulePtr ipSet(gstrSET_SCHEDULE_PROG_ID.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI30412", ipSet != NULL);

		// Prompt the user to set the new schedule
		IVariantVectorPtr ipSchedule = ipSet->PromptForSchedule(m_ipSchedule);
		if (ipSchedule != NULL)
		{
			// Update the FPMgmtRole schedule
			getFPMgmtRole()->ProcessingSchedule = ipSchedule;
			m_ipSchedule = ipSchedule;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28063");
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::refresh()
{
	// Remove any listed File Processors
	m_fileProcessorList.DeleteAllItems();

	// Add each File Processor to the list
	IIUnknownVectorPtr ipFPData = getFileProcessorsData();
	long nSize = ipFPData->Size();
	int i;
	for (i = 0; i < nSize; i++)
	{
		// Add each File Processor to the list
		addFileProcessor( ipFPData->At(i) );
	}

	UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr ipMgmtRole = getFPMgmtRole();

	// Retrieve number of desired threads
	long lCount = ipMgmtRole->NumThreads;

	// Handle max count case
	if (lCount == 0)
	{
		// Select the Max Count button
		m_btnNumThreads.SetCheck( 0 );
		m_btnMaxThreads.SetCheck( 1 );

		// Clear and disable the edit box
		m_editThreads.SetWindowTextA( "" );
		m_editThreads.EnableWindow( FALSE );
		m_SpinThreads.EnableWindow( FALSE );
	}
	else
	{
		// Select the Num Threads button
		m_btnMaxThreads.SetCheck( 0 );
		m_btnNumThreads.SetCheck( 1 );

		// Update and enable the edit box
		m_editThreads.EnableWindow( TRUE );
		m_editThreads.SetWindowTextA( asString( lCount ).c_str() );
		m_SpinThreads.EnableWindow( TRUE );
	}

	// Get the CompleteOnNoRcdFromDB flag
	bool bCompleteOnNoRcdFromDB = ipMgmtRole->KeepProcessingAsAdded == VARIANT_FALSE;
	if (bCompleteOnNoRcdFromDB)
	{
		// Select the Stop Processing button
		m_btnKeepProcessingWithEmptyQueue.SetCheck( 0 );
		m_btnStopProcessingWithEmptyQueue.SetCheck( 1 );
	}
	else
	{
		// Select the Keep Processing button
		m_btnStopProcessingWithEmptyQueue.SetCheck( 0 );
		m_btnKeepProcessingWithEmptyQueue.SetCheck( 1 );
	}

	// Update error log items
	m_bLogErrorDetails = asMFCBool( ipMgmtRole->LogErrorDetails );
	m_zErrorLog = asString( ipMgmtRole->ErrorLogName ).c_str();

	// Update error task items
	m_bExecuteErrorTask = asMFCBool( ipMgmtRole->ExecuteErrorTask );

	IObjectWithDescriptionPtr ipTask = ipMgmtRole->ErrorTask;
	ASSERT_RESOURCE_ALLOCATION("ELI16093", ipTask != NULL);
	m_zErrorTaskDescription = asString( ipTask->Description ).c_str();

	// Get the processing scope and update radio buttons
	bool bProcessSkipped = asCppBool(ipMgmtRole->ProcessSkippedFiles);
	m_radioProcessAll.SetCheck(asBSTChecked(!bProcessSkipped));
	m_radioProcessSkipped.SetCheck(asBSTChecked(bProcessSkipped));
	m_comboSkipped.SetCurSel(ipMgmtRole->SkippedForAnyUser == VARIANT_FALSE ? 0 : 1);

	// Get the limit processing times flag
	m_bLimitProcessingTimes = asMFCBool(ipMgmtRole->LimitProcessingToSchedule);

	m_ipSchedule = NULL;

	// If limiting the processing get the schedule
	if (asCppBool(m_bLimitProcessingTimes))
	{
		m_ipSchedule = ipMgmtRole->ProcessingSchedule;
		ASSERT_RESOURCE_ALLOCATION("ELI28164", m_ipSchedule != NULL);
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::setFPMgr(UCLID_FILEPROCESSINGLib::IFileProcessingManager* pFPMgr)
{
	m_pFPM = pFPMgr;
	ASSERT_RESOURCE_ALLOCATION("ELI14083", m_pFPM != NULL);
}

//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::setEnabled(bool bEnabled)
{
	m_bEnabled = bEnabled;

	if (!m_bEnabled)
	{
		m_btnAdd.EnableWindow(FALSE);
		m_btnModify.EnableWindow(FALSE);
		m_btnRemove.EnableWindow(FALSE);
		m_fileProcessorList.SetSelectionMark(-1);
		m_fileProcessorList.EnableWindow(FALSE);
		m_btnUp.EnableWindow(FALSE);
		m_btnDown.EnableWindow(FALSE);
		m_btnMaxThreads.EnableWindow(FALSE);
		m_btnNumThreads.EnableWindow(FALSE);
		m_editThreads.EnableWindow(FALSE);
		m_SpinThreads.EnableWindow(FALSE);
		GetDlgItem(IDC_STATIC_THREADS)->EnableWindow( FALSE );
		m_btnKeepProcessingWithEmptyQueue.EnableWindow(FALSE);
		m_btnStopProcessingWithEmptyQueue.EnableWindow(FALSE);

		// Error log and task controls
		m_btnLogErrorDetails.EnableWindow(FALSE);
		m_editErrorLog.EnableWindow(FALSE);
		m_btnErrorSelectTag.EnableWindow(FALSE);
		m_btnBrowseErrorLog.EnableWindow(FALSE);

		// Scope controls
		m_radioProcessAll.EnableWindow(FALSE);
		m_radioProcessSkipped.EnableWindow(FALSE);
		m_comboSkipped.EnableWindow(FALSE);
		m_staticSkipped.EnableWindow(FALSE);

		// Error task controls
		m_btnExecuteErrorTask.EnableWindow(FALSE);
		m_btnSelectErrorTask.EnableWindow(FALSE);
		GetDlgItem(IDC_EDIT_EXECUTE_TASK)->EnableWindow(FALSE);

		// Processing schedule controls
		m_checkLimitProcessing.EnableWindow(FALSE);
		m_btnSetSchedule.EnableWindow(FALSE);
	}
	else
	{
		m_btnMaxThreads.EnableWindow(TRUE);
		m_btnNumThreads.EnableWindow(TRUE);
		m_btnKeepProcessingWithEmptyQueue.EnableWindow(TRUE);
		m_btnStopProcessingWithEmptyQueue.EnableWindow(TRUE);

		// Error log and task controls
		m_btnLogErrorDetails.EnableWindow(TRUE);
		if (m_bLogErrorDetails)
		{
			m_editErrorLog.EnableWindow(TRUE);
			m_btnErrorSelectTag.EnableWindow(TRUE);
			m_btnBrowseErrorLog.EnableWindow(TRUE);
		}

		// Scope controls
		m_radioProcessAll.EnableWindow(TRUE);
		m_radioProcessSkipped.EnableWindow(TRUE);
		m_staticSkipped.EnableWindow(TRUE);

		m_checkLimitProcessing.EnableWindow(TRUE);

		// Set the button states that depend on settings
		setButtonStates();
	}
}
//-------------------------------------------------------------------------------------------------
int FileProcessingDlgTaskPage::getNumThreads()
{
	// Default to Max
	int nNewValue = 0;
	if ( m_hWnd != NULL ) 
	{
		if ( m_btnNumThreads.GetCheck() == BST_CHECKED )
		{
			CString zNumThreads;
			m_editThreads.GetWindowTextA( zNumThreads);
			string strNumThreads = zNumThreads;
			if ( strNumThreads != "" )
			{
				bool bUpdateText = false;
				try
				{
					// Get the value
					nNewValue = asLong( strNumThreads );

					// Check that it is in the acceptable range
					if (nNewValue > giTHREADS_UPPER_RANGE)
					{
						bUpdateText = true;
						nNewValue = giTHREADS_UPPER_RANGE;
					}
					else if (nNewValue < 0)
					{
						bUpdateText = true;
						nNewValue = 0;
					}

				}
				catch(...)
				{
					// Set the value to 1
					nNewValue = 1;
					bUpdateText = true;
				}

				if (bUpdateText)
				{
					m_editThreads.SetWindowText(asString(nNewValue).c_str());
				}
			}
			else
			{
				nNewValue = 0;
			}
		}
	}

	return nNewValue;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::ResetInitialized()
{
	m_bInitialized = false;
}

//-------------------------------------------------------------------------------------------------
// Private / helper methods
//-------------------------------------------------------------------------------------------------
bool FileProcessingDlgTaskPage::selectFileProcessor(IObjectWithDescriptionPtr ipObjWithDesc)
{
	// create the object selector UI object and license it
	IObjectSelectorUIPtr ipObjSelect(CLSID_ObjectSelectorUI);
	ASSERT_RESOURCE_ALLOCATION("ELI13511", ipObjSelect != NULL);

	// initialize private license for the object
	IPrivateLicensedComponentPtr ipPLComponent = ipObjSelect;
	ASSERT_RESOURCE_ALLOCATION("ELI10301", ipPLComponent != NULL);
	_bstr_t _bstrKey = LICENSE_MGMT_PASSWORD.c_str();
	ipPLComponent->InitPrivateLicense(_bstrKey);
	
	// first make a copy of the ObjectWithDescption in case 
	// the user cancels out the dialog
	IObjectWithDescriptionPtr ipObjectWithDescription(ipObjWithDesc->Clone());
	ASSERT_RESOURCE_ALLOCATION("ELI13512", ipObjectWithDescription != NULL);

	// Prepare the title and prompts
	_bstr_t	bstrTitle("File Processor");
	_bstr_t	bstrDesc("File Processor Description");
	_bstr_t	bstrSelect("Select File Processor");
	_bstr_t	bstrCategory(FP_FILE_PROC_CATEGORYNAME.c_str());
	
	// show object selection dlg
	VARIANT_BOOL vbResult = ipObjSelect->ShowUI1(bstrTitle, bstrDesc, 
		bstrSelect, bstrCategory, ipObjectWithDescription, VARIANT_FALSE);
	
	// if the user clicked Cancel
	if (vbResult == VARIANT_FALSE)
	{
		return false;
	}
	
	// if the object needs a private license
	IPrivateLicensedComponentPtr ipPrivateLicObject(ipObjectWithDescription->Object);
	if (ipPrivateLicObject)
	{
		_bstr_t _bstrPrivateLicenseCode(::LICENSE_MGMT_PASSWORD.c_str());
		ipPrivateLicObject->InitPrivateLicense(_bstrPrivateLicenseCode);
	}

	// copy the content to the pass-in object
	ipObjWithDesc->CopyFrom(ipObjectWithDescription);

	return true;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::addFileProcessor(IObjectWithDescriptionPtr ipObject)
{
	ASSERT_RESOURCE_ALLOCATION("ELI10973", ipObject != NULL);
	CString zDescription = (char*)ipObject->Description;

	// Add item to end of list, set the text, set the checkbox
	int iIndex = m_fileProcessorList.GetItemCount();
	int iNewIndex = m_fileProcessorList.InsertItem( iIndex, "" );
	m_fileProcessorList.SetItemText( iNewIndex, 1, zDescription.operator LPCTSTR() );
	m_fileProcessorList.SetCheck( iNewIndex, asMFCBool( ipObject->Enabled ) );

	//Update the button states
	setButtonStates();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::clearListSelection()
{
	POSITION pos = m_fileProcessorList.GetFirstSelectedItemPosition();
	if (pos == NULL)
	{
		// no item selected, return
		return;
	}
	
	while (pos)
	{
		int nItemSelected = m_fileProcessorList.GetNextSelectedItem(pos);
		m_fileProcessorList.SetItemState(nItemSelected, 0, LVIS_SELECTED);
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::markSelectedTasks() 
{
	POSITION pos = m_fileProcessorList.GetFirstSelectedItemPosition();
	if (pos != NULL)
	{
		// Get index of first selection
		int iIndex = m_fileProcessorList.GetNextSelectedItem( pos );

		// Loop through selected items
		while (iIndex != -1)
		{
			// Set ItemData = 1 as a "mark"
			m_fileProcessorList.SetItemData( iIndex, (DWORD) 1 );

			// Get index of next selected item
			iIndex = m_fileProcessorList.GetNextSelectedItem( pos );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::deleteMarkedTasks() 
{
	// Get list count
	int iCount = m_fileProcessorList.GetItemCount();
	if (iCount == 0)
	{
		return;
	}

	// Check vector of File Processor
	if (getFileProcessorsData()->Front() == NULL)
	{
		// Throw exception, File Processor not defined
		throw UCLIDException( "ELI13493", "File Processors are not defined." );
	}

	// Step backwards through list
	for (int i = iCount - 1; i >= 0; i--)
	{
		// Retrieve ItemData and look for "mark"
		DWORD	dwData = m_fileProcessorList.GetItemData( i );
		if (dwData == 1)
		{
			// Remove this item from list
			m_fileProcessorList.DeleteItem( i );

			// Remove this item from the vector of file processors
			getFileProcessorsData()->Remove( i );
		}
	}

	// Update the button states
	setButtonStates();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::setButtonStates()
{
	// Retrieve current settings
	UpdateData(FALSE);

	// Enable the Add button and the list
	m_btnAdd.EnableWindow( TRUE );
	m_fileProcessorList.EnableWindow( TRUE );

	// Check count of file processors
	int	iCount = m_fileProcessorList.GetItemCount();
	if (iCount == 0)
	{
		// Disable other buttons
		m_btnRemove.EnableWindow( FALSE );
		m_btnModify.EnableWindow( FALSE );
		m_btnUp.EnableWindow( FALSE );
		m_btnDown.EnableWindow( FALSE );
	}
	else
	{	
		// Next have to see if an item is selected
		int iIndex = -1;
		POSITION pos = m_fileProcessorList.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			// Get index of first selection
			iIndex = m_fileProcessorList.GetNextSelectedItem( pos );
		}
		if (iIndex > -1)
		{
			// Enable the Delete button
			m_btnRemove.EnableWindow( TRUE );

			// Check for multiple selection
			int iIndex2 = m_fileProcessorList.GetNextSelectedItem( pos );
			if (iIndex2 == -1)
			{
				// Only one selected, enable Configure
				m_btnModify.EnableWindow( TRUE );

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
				m_btnModify.EnableWindow( FALSE );
				m_btnUp.EnableWindow( FALSE );
				m_btnDown.EnableWindow( FALSE );
			}
		}
		else
		{
			// No selection --> disable most buttons
			m_btnRemove.EnableWindow( FALSE );
			m_btnModify.EnableWindow( FALSE );
			m_btnUp.EnableWindow( FALSE );
			m_btnDown.EnableWindow( FALSE );
		}
	}

	// Enable / disable the edit box and spin control for number of threads
	BOOL bNumThreadsChecked = asMFCBool(m_btnNumThreads.GetCheck() == BST_CHECKED);
	m_editThreads.EnableWindow( bNumThreadsChecked );
	m_SpinThreads.EnableWindow( bNumThreadsChecked );
	GetDlgItem(IDC_STATIC_THREADS)->EnableWindow( TRUE );

	// Can always check/uncheck log error details
	m_btnLogErrorDetails.EnableWindow(TRUE); 

	// Enable / disable controls for logging error details
	m_editErrorLog.EnableWindow(m_bLogErrorDetails);
	m_btnErrorSelectTag.EnableWindow(m_bLogErrorDetails);
	m_btnBrowseErrorLog.EnableWindow(m_bLogErrorDetails);

	// Can always check/uncheck error task
	m_btnExecuteErrorTask.EnableWindow(TRUE);

	// Enable the combo box for skipped files if process skipped files is checked
	m_comboSkipped.EnableWindow(asMFCBool(m_radioProcessSkipped.GetCheck() == BST_CHECKED));

	// Enable / disable error task controls
	GetDlgItem(IDC_EDIT_EXECUTE_TASK)->EnableWindow(m_bExecuteErrorTask);
	m_btnSelectErrorTask.EnableWindow(m_bExecuteErrorTask);

	m_btnSetSchedule.EnableWindow(m_bLimitProcessingTimes);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::updateUI()
{
	// Get the pointer to the Property sheet object
	ResizablePropertySheet* pFPDPropSheet = (ResizablePropertySheet*)GetParent();
	// Get the pointer to the current FileProcessingDlg object
	FileProcessingDlg* pFPDlg = (FileProcessingDlg*)pFPDPropSheet->GetParent();

	pFPDlg->updateUI();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::replaceFileProcessorAt(int iIndex, IObjectWithDescriptionPtr ipNewFP)
{
	// Update the file processors data
	getFileProcessorsData()->Remove( iIndex );
	getFileProcessorsData()->Insert(iIndex, ipNewFP );

	// Update the file processor listbox
	m_fileProcessorList.DeleteItem( iIndex );
	m_fileProcessorList.InsertItem( iIndex, "" );
	m_fileProcessorList.SetItemText( iIndex, 1, ipNewFP->Description );
	m_fileProcessorList.SetCheck( iIndex, ipNewFP->Enabled );

	// Retain selection and focus
	m_fileProcessorList.SetItemState( iIndex, LVIS_SELECTED, LVIS_SELECTED );
	m_fileProcessorList.SetFocus();
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr FileProcessingDlgTaskPage::getFPMgmtRole()
{
	UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr ipFPM( m_pFPM );
	ASSERT_RESOURCE_ALLOCATION("ELI14294", ipFPM != NULL);

	// get the file processing mgmt role
	UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr ipFPMgmtRole = ipFPM->FileProcessingMgmtRole;
	ASSERT_RESOURCE_ALLOCATION("ELI14295", ipFPMgmtRole != NULL);

	return ipFPMgmtRole;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr FileProcessingDlgTaskPage::getFileProcessorsData()
{
	// get the file supplying mgmt role
	IIUnknownVectorPtr ipFileProcessorsData = getFPMgmtRole()->FileProcessors;
	ASSERT_RESOURCE_ALLOCATION("ELI14310", ipFileProcessorsData != NULL);

	return ipFileProcessorsData;
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr FileProcessingDlgTaskPage::getMiscUtils()
{
	// check if a MiscUtils object has all ready been created
	if (!m_ipMiscUtils)
	{
		// create MiscUtils object
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI16130", m_ipMiscUtils != NULL);
	}

	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::getDlgItemWindowRect(UINT uiDlgItemResourceID, RECT &rectWindow)
{
	// retrieve the dialog item using its resource ID
	CWnd* cwndDlgItem = GetDlgItem(uiDlgItemResourceID);
	ASSERT_RESOURCE_ALLOCATION("ELI16132", cwndDlgItem != NULL);

	// set the window rect to the appropriate position and dimensions
	cwndDlgItem->GetWindowRect(&rectWindow);
}
//-------------------------------------------------------------------------------------------------
