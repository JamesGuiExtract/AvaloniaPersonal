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
#include "AdvancedTaskSettingsDlg.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <DocTagUtils.h>

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

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgTaskPage property page
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(FileProcessingDlgTaskPage, CPropertyPage)
//-------------------------------------------------------------------------------------------------
FileProcessingDlgTaskPage::FileProcessingDlgTaskPage() 
: CPropertyPage(FileProcessingDlgTaskPage::IDD), 
  m_ipClipboardMgr(NULL),
  m_ipMiscUtils(NULL),
  m_dwSel(0),
  m_bEnabled(true),
  m_bInitialized(false),
  m_bLogErrorDetails(FALSE),
  m_bExecuteErrorTask(FALSE),
  m_pFPM(NULL)
{
}
//-------------------------------------------------------------------------------------------------
FileProcessingDlgTaskPage::~FileProcessingDlgTaskPage()
{
	try
	{
		m_ipClipboardMgr = __nullptr;
		m_ipMiscUtils = __nullptr;
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
	DDX_Control(pDX, IDC_CHECK_LOG_ERROR_DETAILS, m_btnLogErrorDetails);
	DDX_Check(pDX, IDC_CHECK_LOG_ERROR_DETAILS, m_bLogErrorDetails);
	DDX_Control(pDX, IDC_EDIT_ERROR_LOG, m_editErrorLog);
	DDX_Text(pDX, IDC_EDIT_ERROR_LOG, m_zErrorLog);
	DDX_Control(pDX, IDC_BTN_SELECT_DOC_TAG, m_btnErrorSelectTag);
	DDX_Control(pDX, IDC_BTN_BROWSE_LOG, m_btnBrowseErrorLog);
	DDX_Control(pDX, IDC_CHECK_EXECUTE_TASK, m_btnExecuteErrorTask);
	DDX_Check(pDX, IDC_CHECK_EXECUTE_TASK, m_bExecuteErrorTask);
	DDX_Control(pDX, IDC_EDIT_EXECUTE_TASK, m_editExecuteTask);
	DDX_Text(pDX, IDC_EDIT_EXECUTE_TASK, m_zErrorTaskDescription);
	DDX_Control(pDX, IDC_BTN_SELECT_ERROR_TASK, m_btnSelectErrorTask);
	DDX_Control(pDX, IDC_RADIO_PROCESS_ALL_FILES_PRIORITY, m_radioProcessAll);
	DDX_Control(pDX, IDC_RADIO_PROCESS_SKIPPED_FILES, m_radioProcessSkipped);
	DDX_Control(pDX, IDC_COMBO_SKIPPED_SCOPE, m_comboSkipped);
	DDX_Control(pDX, IDC_STATIC_SKIPPED, m_staticSkipped);
	DDX_Control(pDX, IDC_BUTTON_TASK_ADVANCED_SETTINGS, m_btnAdvancedSettings);
	DDX_Control(pDX, IDC_CHECK_SEND_ERROR_EMAIL, m_btnSendErrorEmail);
	DDX_Check(pDX, IDC_CHECK_SEND_ERROR_EMAIL, m_bSendErrorEmail);
	DDX_Control(pDX, IDC_EDIT_ERROR_EMAIL_RECIPIENTS, m_editErrorEmailRecipients);
	DDX_Text(pDX, IDC_EDIT_ERROR_EMAIL_RECIPIENTS, m_zErrorEmailRecipients);
	DDX_Control(pDX, IDC_BTN_CONFIGURE_ERROR_EMAIL, m_btnConfigureErrorEmail);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileProcessingDlgTaskPage, CPropertyPage)
	ON_BN_CLICKED(IDC_BTN_ADD, OnBtnAdd)
	ON_BN_CLICKED(IDC_BTN_REMOVE, OnBtnRemove)
	ON_BN_CLICKED(IDC_BTN_MODIFY, OnBtnModify)
	ON_BN_CLICKED(IDC_BTN_DOWN, OnBtnDown)
	ON_BN_CLICKED(IDC_BTN_UP, OnBtnUp)
	ON_MESSAGE(WM_TASK_GRID_SELCHANGE, OnGridSelChange)
	ON_WM_SIZE()
	ON_BN_CLICKED(IDC_CHECK_LOG_ERROR_DETAILS, OnCheckLogErrorDetails)
	ON_EN_CHANGE(IDC_EDIT_ERROR_LOG, &FileProcessingDlgTaskPage::OnEnChangeEditErrorLog)
	ON_BN_CLICKED(IDC_BTN_SELECT_DOC_TAG, OnBtnErrorSelectDocTag)
	ON_BN_CLICKED(IDC_BTN_BROWSE_LOG, OnBtnBrowseErrorLog)
	ON_BN_CLICKED(IDC_CHECK_SEND_ERROR_EMAIL, &FileProcessingDlgTaskPage::OnCheckSendErrorEmail)
	ON_EN_CHANGE(IDC_EDIT_ERROR_EMAIL_RECIPIENTS, &FileProcessingDlgTaskPage::OnEnChangeErrorEmailRecipients)
	ON_BN_CLICKED(IDC_BTN_CONFIGURE_ERROR_EMAIL, &FileProcessingDlgTaskPage::OnBtnConfigureErrorEmail)
	ON_BN_CLICKED(IDC_CHECK_EXECUTE_TASK, OnCheckExecuteErrorTask)
	ON_BN_CLICKED(IDC_BTN_SELECT_ERROR_TASK, OnBtnAddErrorTask)
	ON_MESSAGE(WM_TASK_GRID_DBLCLICK, OnGridDblClick)
	ON_MESSAGE(WM_TASK_GRID_RCLICK, OnGridRightClick)
	ON_COMMAND(ID_CONTEXT_CUT, &FileProcessingDlgTaskPage::OnContextCut)
	ON_COMMAND(ID_CONTEXT_COPY, &FileProcessingDlgTaskPage::OnContextCopy)
	ON_COMMAND(ID_CONTEXT_PASTE, &FileProcessingDlgTaskPage::OnContextPaste)
	ON_COMMAND(ID_CONTEXT_DELETE, &FileProcessingDlgTaskPage::OnContextDelete)
	ON_MESSAGE(WM_TASK_GRID_CELL_VALUE_CHANGE, OnCellValueChange)
	ON_BN_CLICKED(IDC_RADIO_PROCESS_ALL_FILES_PRIORITY, &FileProcessingDlgTaskPage::OnBtnProcessAllOrSkipped)
	ON_BN_CLICKED(IDC_RADIO_PROCESS_SKIPPED_FILES, &FileProcessingDlgTaskPage::OnBtnProcessAllOrSkipped)
	ON_CBN_SELCHANGE(IDC_COMBO_SKIPPED_SCOPE, &FileProcessingDlgTaskPage::OnComboSkippedChange)
	ON_WM_LBUTTONDBLCLK()
	ON_BN_CLICKED(IDC_BUTTON_TASK_ADVANCED_SETTINGS, &FileProcessingDlgTaskPage::OnBtnAdvancedSettings)
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

		m_fileProcessorList.AttachGrid(this, IDC_LIST_FP);
		
		//create instances of necessary variables
		m_ipClipboardMgr.CreateInstance(CLSID_ClipboardObjectManager);
		ASSERT_RESOURCE_ALLOCATION("ELI18616", m_ipClipboardMgr != __nullptr);

		// load icons for up and down buttons
		m_btnUp.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_UP)));
		m_btnDown.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_DOWN)));

		//////////////////
		// Prepare headers
		//////////////////
		// Get dimensions of control
		CRect	rect;
		m_fileProcessorList.GetClientRect( &rect );

		// Compute width for Description column
		long	lDWidth = rect.Width() - glENABLED_WIDTH;

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
		ASSERT_RESOURCE_ALLOCATION("ELI16051", ipObject != __nullptr);

		// allow the user to select and configure
		VARIANT_BOOL vbDirty = getMiscUtils()->AllowUserToSelectAndConfigureObject(ipObject, 
			"Task",	get_bstr_t(FP_FILE_PROC_CATEGORYNAME), VARIANT_FALSE, 0, NULL);

		// check OK was selected
		if (vbDirty)
		{
			// Get index of first selection.
			int iIndex = m_fileProcessorList.GetFirstSelectedRow();
			
			// If no current selection, insert item at end of list
			if (iIndex == -1)
			{
				iIndex = m_fileProcessorList.GetNumberRows();
			}
			else
			{
				// Insert position is after the first selected item (P13 #4732)
				iIndex++;
			}

			// Insert the object-with-description into the vector and refresh the list
			getFileProcessorsData()->Insert( iIndex, ipObject );
			refresh();

			// clear the previous selection if any
			m_fileProcessorList.ClearSelections();

			// Set selection and focus
			m_fileProcessorList.SelectRow(iIndex);
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
		// Get index of first selection.
		int iIndex = m_fileProcessorList.GetFirstSelectedRow();
		
		// Handle single-selection case
		int		iResult;
		CString	zPrompt;
		if (m_fileProcessorList.GetNextSelectedRow() == -1)
		{
			// Retrieve current file processor description
			CString	zDescription = m_fileProcessorList.GetText(iIndex).c_str();

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
			// Delete the marked file processors
			deleteSelectedTasks();

			// Select the next (or the last) file processor
			int iCount = m_fileProcessorList.GetNumberRows();
			if (iCount <= iIndex)
			{
				iIndex = iCount - 1;
			}
		}

		// Retain selection and focus
		m_fileProcessorList.SelectRow(iIndex);
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
		// Get index of first selection.
		int iIndex = m_fileProcessorList.GetFirstSelectedRow();

		if (iIndex > -1)
		{
			// get the current file processor
			IObjectWithDescriptionPtr	ipFP = getFileProcessorsData()->At( iIndex );
			ASSERT_RESOURCE_ALLOCATION("ELI15896", ipFP != __nullptr);
			
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
		// Get index of first selection
		int iIndex = m_fileProcessorList.GetFirstSelectedRow();
		
		// Selection cannot be at bottom of list
		int iCount = m_fileProcessorList.GetNumberRows();
		if (iIndex < iCount - 1)
		{
			// Update the file processors vector and refresh the list
			getFileProcessorsData()->Swap( iIndex, iIndex + 1 );
			refresh();

			// Set selection and focus
			m_fileProcessorList.ClearSelections();
			m_fileProcessorList.SelectRow(iIndex + 1);
			m_fileProcessorList.SetFocus();

			// Sometimes the Ultimate Grid doesn't draw all rows correctly if RedrawAll isn't called
			// here.
			m_fileProcessorList.RedrawAll();

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
		// Get index of first selection
		int iIndex = m_fileProcessorList.GetFirstSelectedRow();

		// Selection cannot be at top of list
		if (iIndex > 0)
		{
			// Update the file processors vector and refresh the list
			getFileProcessorsData()->Swap( iIndex, iIndex - 1 );
			refresh();

			// Set selection and focus
			m_fileProcessorList.ClearSelections();
			m_fileProcessorList.SelectRow(iIndex - 1);
			m_fileProcessorList.SetFocus();

			// Sometimes the Ultimate Grid doesn't draw all rows correctly if RedrawAll isn't called
			// here.
			m_fileProcessorList.RedrawAll();

			// Update button states
			setButtonStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13507")
}
//--------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlgTaskPage::OnGridSelChange(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Update button states
		setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13508")

	return 0;
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
		ChooseDocTagForEditBox(ITagUtilityPtr(CLSID_FAMTagManager),
			m_btnErrorSelectTag, m_editErrorLog);
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
void FileProcessingDlgTaskPage::OnCheckSendErrorEmail()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		UpdateData(TRUE);

		// Provide new setting to File Processing Mgmt Role object
		getFPMgmtRole()->SendErrorEmail = asVariantBool(m_bSendErrorEmail);

		if (m_bSendErrorEmail)
		{
			IErrorEmailTaskPtr ipErrorEmailTask = getFPMgmtRole()->ErrorEmailTask;
			ASSERT_RESOURCE_ALLOCATION("ELI36169", ipErrorEmailTask != __nullptr);

			if (!asCppBool(ipErrorEmailTask->IsEmailServerConfigured()))
			{
				m_bSendErrorEmail = false;
				getFPMgmtRole()->SendErrorEmail = VARIANT_FALSE;

				UpdateData(FALSE);

				MessageBox("Before configuring email notification of errors, outbound email server "
                    "settings need to be configured in the FAM database.\r\n\r\n"
                    "In the DB Administration utility, select the \"Database | Database "
					"options...\" menu option, then use the \"Email\" tab to configure the "
					"outbound email server.", "Outbound email server not configured",
					MB_ICONWARNING | MB_OK);
			}
		}

		// Enable / disable controls as appropriate
		setButtonStates();
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36128")
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnEnChangeErrorEmailRecipients()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );
	try
	{
		UpdateData(TRUE);

		IErrorEmailTaskPtr ipErrorEmailTask = getFPMgmtRole()->ErrorEmailTask;
		ASSERT_RESOURCE_ALLOCATION("ELI36129", ipErrorEmailTask != __nullptr);

		// Update the error email recipients
		ipErrorEmailTask->Recipient = get_bstr_t(m_zErrorEmailRecipients);
		
		// Error email may now be valid; enable run/save if appropriate.
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36130");
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnConfigureErrorEmail()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		IErrorEmailTaskPtr ipErrorEmailTask = getFPMgmtRole()->ErrorEmailTask;
		ASSERT_RESOURCE_ALLOCATION("ELI36131", ipErrorEmailTask != __nullptr);

		if (asCppBool(ipErrorEmailTask->ConfigureErrorEmail()))
		{
			// If the user okay'd the configuration, apply the current recipients from the email
			// task to the recipients edit box.
			m_zErrorEmailRecipients = asString(ipErrorEmailTask->Recipient).c_str();
			UpdateData(FALSE);

			// Error email may now be valid; enable run/save if appropriate.
			updateUI();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36132")
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
		ASSERT_RESOURCE_ALLOCATION("ELI16111", ipOWD != __nullptr);

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

		// Width of the line used for the group box
		static const int nGROUPBOX_LINE_WIDTH = 2;

		CRect rectDlg, rectDownButton, rectList;
		CRect rectAddButton, rectUpButton, rectDeleteButton, rectModifyButton;
		CRect rectErrorGrp, rectCheckLogError, rectEditErrorLog, rectCheckErrorEmail,
			rectEditErrorEmailRecipients, rectConfigureEmailButton, rectCheckExecuteTask,
			rectEditExecuteTask, rectDocTag, rectBrowse, rectSelect;
		CRect rectProcessScopeGrp, rectProcessAll, rectProcessSkipped, rectComboSkippedScope, rectSkippedText;
		CRect rectAdvancedButton;

		// Get positions of list and buttons
		m_fileProcessorList.GetWindowRect(&rectList);
		ScreenToClient(&rectList);
		m_btnAdd.GetWindowRect(&rectAddButton);
		ScreenToClient(&rectAddButton);
		m_btnRemove.GetWindowRect(&rectDeleteButton);
		ScreenToClient(&rectDeleteButton);
		m_btnModify.GetWindowRect(&rectModifyButton);
		ScreenToClient(&rectModifyButton);			
		m_btnUp.GetWindowRect(&rectUpButton);
		ScreenToClient(&rectUpButton);
		m_btnDown.GetWindowRect(&rectDownButton);
		ScreenToClient(&rectDownButton);

		// Get positions of Error group
		getDlgItemWindowRect(IDC_STATIC_ERROR_GROUP, rectErrorGrp);
		ScreenToClient(&rectErrorGrp);
		m_btnLogErrorDetails.GetWindowRect(&rectCheckLogError);
		ScreenToClient(&rectCheckLogError);
		m_editErrorLog.GetWindowRect(&rectEditErrorLog);
		ScreenToClient(&rectEditErrorLog);
		m_btnErrorSelectTag.GetWindowRect(&rectDocTag);
		ScreenToClient(&rectDocTag);
		m_btnBrowseErrorLog.GetWindowRect(&rectBrowse);
		ScreenToClient(&rectBrowse);
		m_btnSendErrorEmail.GetWindowRect(&rectCheckErrorEmail);
		ScreenToClient(&rectCheckErrorEmail);
		m_editErrorEmailRecipients.GetWindowRect(&rectEditErrorEmailRecipients);
		ScreenToClient(&rectEditErrorEmailRecipients);
		m_btnConfigureErrorEmail.GetWindowRect(&rectConfigureEmailButton);
		ScreenToClient(&rectConfigureEmailButton);
		m_btnExecuteErrorTask.GetWindowRect(&rectCheckExecuteTask);
		ScreenToClient(&rectCheckExecuteTask);
		m_editExecuteTask.GetWindowRect(&rectEditExecuteTask);
		ScreenToClient(&rectEditExecuteTask);
		m_btnSelectErrorTask.GetWindowRect(&rectSelect);
		ScreenToClient(&rectSelect);

		// Get positions of the Process Scope group items
		getDlgItemWindowRect(IDC_STATIC_FPSCOPE_GROUP, rectProcessScopeGrp);
		ScreenToClient(&rectProcessScopeGrp);
		m_radioProcessAll.GetWindowRect(&rectProcessAll);
		ScreenToClient(&rectProcessAll);
		m_radioProcessSkipped.GetWindowRect(&rectProcessSkipped);
		ScreenToClient(&rectProcessSkipped);
		m_comboSkipped.GetWindowRect(&rectComboSkippedScope);
		ScreenToClient(&rectComboSkippedScope);
		m_staticSkipped.GetWindowRect(&rectSkippedText);
		ScreenToClient(&rectSkippedText);
		m_btnAdvancedSettings.GetWindowRect(&rectAdvancedButton);
		ScreenToClient(&rectAdvancedButton);

		GetClientRect(&rectDlg);

		// Set values for static items
		if (!bInit)
		{
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

		// resize list box width
		rectList.right = rectDlg.right - nLen1 - nAddButtonWidth - nLen3;

		// Resize buttons
		int nButtonLeft = rectDlg.right - nLen1 - nAddButtonWidth;
		rectAddButton.MoveToX(nButtonLeft);
		rectDeleteButton.MoveToX(nButtonLeft);
		rectModifyButton.MoveToX(nButtonLeft);
		rectUpButton.MoveToX(nButtonLeft);
		rectDownButton.MoveToX(rectUpButton.right + nLen2);

		// Resize processing scope group items
		int nGroupHeight = rectProcessScopeGrp.Height();
		rectProcessScopeGrp.bottom = rectDlg.bottom - nLen4;
		rectProcessScopeGrp.top = rectProcessScopeGrp.bottom - nGroupHeight;
		rectProcessScopeGrp.left = rectList.left;
		rectProcessScopeGrp.right = rectList.right;

		int nHeight = rectProcessAll.Height();
		long nSpace = rectProcessSkipped.top - rectProcessAll.bottom;
		rectProcessAll.top = rectProcessScopeGrp.top + nLen5;
		rectProcessAll.bottom = rectProcessAll.top + nHeight;
		rectProcessSkipped.top = rectProcessAll.bottom + nSpace;
		rectProcessSkipped.bottom = rectProcessSkipped.top + nHeight;
		rectSkippedText.top = rectProcessSkipped.top;
		rectSkippedText.bottom = rectSkippedText.top + nHeight;

		// Move the combo box
		nHeight = rectComboSkippedScope.Height();
		int nWidth = rectComboSkippedScope.Width();
		rectComboSkippedScope.top = rectProcessSkipped.top + rectProcessSkipped.Height()/2 - nHeight/2;
		rectComboSkippedScope.bottom = rectComboSkippedScope.top + nHeight;
		rectComboSkippedScope.left = rectProcessSkipped.right + nSpace;
		rectComboSkippedScope.right = rectComboSkippedScope.left + nWidth;

		// Move the skipped text now that the combo box has been moved
		nWidth = rectSkippedText.Width();
		rectSkippedText.left = rectComboSkippedScope.right + nSpace;
		rectSkippedText.right = rectSkippedText.left + nWidth;
 
		// Move the advanced button
		rectAdvancedButton.MoveToXY(nButtonLeft, rectProcessScopeGrp.top + 5);
		
		// Resize Error group items
		int nButtonToEditVerticalAdjustment = rectEditErrorLog.top - rectBrowse.top;
		nGroupHeight = rectErrorGrp.Height();
		nSpace = rectEditErrorLog.top - rectErrorGrp.top;
		int nSpaceErrorEmailCheck = rectCheckErrorEmail.top - rectErrorGrp.top;
		int nSpaceErrorEmailEdit = rectEditErrorEmailRecipients.top - rectErrorGrp.top;
		int nSpaceExecuteTask = rectCheckExecuteTask.top - rectErrorGrp.top;
		int nSpaceErrorGroup = rectSelect.top - rectErrorGrp.top;
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
		
		nHeight = rectCheckErrorEmail.Height();
		rectCheckErrorEmail.top = rectErrorGrp.top + nSpaceErrorEmailCheck;
		rectCheckErrorEmail.bottom = rectCheckErrorEmail.top + nHeight;

		nHeight = rectEditErrorEmailRecipients.Height();
		rectEditErrorEmailRecipients.top = rectErrorGrp.top + nSpaceErrorEmailEdit;
		rectEditErrorEmailRecipients.bottom = rectEditErrorEmailRecipients.top + nHeight;

		nWidth = rectConfigureEmailButton.Width();
		nHeight = rectConfigureEmailButton.Height();
		rectConfigureEmailButton.right = rectErrorGrp.right - nLen2 - nGROUPBOX_LINE_WIDTH;
		rectConfigureEmailButton.left = rectConfigureEmailButton.right - nWidth;
		rectConfigureEmailButton.top = rectEditErrorEmailRecipients.top - nButtonToEditVerticalAdjustment;
		rectConfigureEmailButton.bottom = rectConfigureEmailButton.top + nHeight;
		rectEditErrorEmailRecipients.right = rectConfigureEmailButton.left - nLen2;

		nHeight = rectCheckExecuteTask.Height();
		rectCheckExecuteTask.top = rectErrorGrp.top + nSpaceExecuteTask;
		rectCheckExecuteTask.bottom = rectCheckExecuteTask.top + nHeight;

		nHeight = rectSelect.Height();
		nWidth = rectSelect.Width();
		rectSelect.right = rectErrorGrp.right - nLen2 - nGROUPBOX_LINE_WIDTH;
		rectSelect.left = rectSelect.right - nWidth;
		rectSelect.top = rectErrorGrp.top + nSpaceErrorGroup;
		rectSelect.bottom = rectSelect.top + nHeight;

		nHeight = rectEditExecuteTask.Height();
		rectEditExecuteTask.right = rectSelect.left - nLen2;
		rectEditExecuteTask.top = rectSelect.top + nButtonToEditVerticalAdjustment;
		rectEditExecuteTask.bottom = rectEditExecuteTask.top + nHeight;

		// Resize list box height
		rectList.bottom = rectErrorGrp.top - nLen4;

		// Move list and buttons
		m_fileProcessorList.MoveWindow(&rectList);

		// Resize the picture control around the processor list grid
		rectList.InflateRect(1, 1, 1, 1);
		GetDlgItem(IDC_PICTURE)->MoveWindow(&rectList);
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
		m_btnSendErrorEmail.MoveWindow(&rectCheckErrorEmail);
		m_editErrorEmailRecipients.MoveWindow(&rectEditErrorEmailRecipients);
		m_btnConfigureErrorEmail.MoveWindow(&rectConfigureEmailButton);
		m_btnExecuteErrorTask.MoveWindow(&rectCheckExecuteTask);
		m_btnSelectErrorTask.MoveWindow(&rectSelect);
		m_editExecuteTask.MoveWindow(&rectEditExecuteTask);

		// Move processing scope group items
		GetDlgItem(IDC_STATIC_FPSCOPE_GROUP)->MoveWindow(&rectProcessScopeGrp);
		m_radioProcessAll.MoveWindow(&rectProcessAll);
		m_radioProcessSkipped.MoveWindow(&rectProcessSkipped);
		m_comboSkipped.MoveWindow(&rectComboSkippedScope);
		m_staticSkipped.MoveWindow(&rectSkippedText);

		// Move the advanced button
		m_btnAdvancedSettings.MoveWindow(&rectAdvancedButton);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13509")
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::OnBtnAdvancedSettings()
{
	try
	{
		UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr ipFPMgr = getFPMgr();
		UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr ipFPM = getFPMgmtRole(ipFPMgr);
		IVariantVectorPtr ipSchedule = __nullptr;
		if (ipFPM->LimitProcessingToSchedule == VARIANT_TRUE)
		{
			ipSchedule = ipFPM->ProcessingSchedule;
		}
		AdvancedTaskSettingsDlg settings(ipFPM->NumThreads,
			asCppBool(ipFPM->KeepProcessingAsAdded), ipSchedule, ipFPMgr->MaxFilesFromDB,
			ipFPMgr->UseRandomIDForQueueOrder, this);
		if (settings.DoModal() == IDOK)
		{
			ipFPM->NumThreads = settings.getNumberOfThreads();
			ipFPM->KeepProcessingAsAdded = asVariantBool(settings.getKeepProcessing());
			ipSchedule = settings.getSchedule();
			if (ipSchedule != __nullptr)
			{
				ipFPM->LimitProcessingToSchedule = VARIANT_TRUE;
				ipFPM->ProcessingSchedule = ipSchedule;
			}
			else
			{
				ipFPM->LimitProcessingToSchedule = VARIANT_FALSE;
			}
			ipFPMgr->MaxFilesFromDB = settings.getNumberOfFilesFromDb();
			ipFPMgr->UseRandomIDForQueueOrder = settings.getUseRandomIDForQueueOrder();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32138");
}
//-------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlgTaskPage::OnGridDblClick(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Get index of first selection
		int iIndex = m_fileProcessorList.GetFirstSelectedRow();
		
		if (iIndex > -1)
		{
			// retrieve the selected file processor
			IObjectWithDescriptionPtr ipObject = getFileProcessorsData()->At(iIndex);
			ASSERT_RESOURCE_ALLOCATION("ELI16094", ipObject != __nullptr);

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
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13490")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT FileProcessingDlgTaskPage::OnGridRightClick(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Get index of first selection
		int iIndex = lParam;
			
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

		// Paste will be disabled if clipboard manager is null or the data on the clipboard is not 
		// readable - not a known object or compatible version
		UINT nPasteSetting = nDisable;

		// Check Clipboard object type
		if (m_ipClipboardMgr != __nullptr)
		{
			// Only interested in enabling or disabling the paste menu and don't want an exception displayed
			// so added the try catch block to remove the display of the exception when displaying the
			// context menu
			// https://extract.atlassian.net/browse/ISSUE-13155
			try 
			{
				if (asCppBool(m_ipClipboardMgr->IUnknownVectorIsOWDOfType(IID_IFileProcessingTask)) ||
					asCppBool(m_ipClipboardMgr->ObjectIsTypeWithDescription(IID_IFileProcessingTask)))
				{
					// Object is a vector of IFileProcessingTasks OR Object is a
					// single IFileProcessingTask
					nPasteSetting = nEnable;
				}
			}
			catch (...)
			{
				// Just eat the exception 
			}
		}
			
		pContextMenu->EnableMenuItem( ID_CONTEXT_PASTE, nPasteSetting );

		// Map the point to the correct position
		CPoint	point;
		GetCursorPos( &point );
			
		// Display and manage the context menu
		pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
			point.x, point.y, this );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13492")

	return 0;
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
			// Get index of first selection
			int iIndex = m_fileProcessorList.GetFirstSelectedRow();

			if (iIndex == -1)
			{
				// Throw exception
				throw UCLIDException( "ELI13500", 
					"Unable to determine selected File Processor!" );
			}

			// Create a vector for selected File Processors
			IIUnknownVectorPtr	ipCopiedFPs( CLSID_IUnknownVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI13501", ipCopiedFPs != __nullptr );

			// Add each selected to vector
			do
			{
				// Retrieve the selected
				IUnknownPtr	ipObject = getFileProcessorsData()->At( iIndex );
				ASSERT_RESOURCE_ALLOCATION( "ELI13502", ipObject != __nullptr );

				// Add the File Processor to the vector
				ipCopiedFPs->PushBack( ipObject );

				iIndex = m_fileProcessorList.GetNextSelectedRow();
			}
			while (iIndex != -1);

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
			ASSERT_RESOURCE_ALLOCATION( "ELI13494", ipObject != __nullptr );
		}
		else if (m_ipClipboardMgr->ObjectIsTypeWithDescription( 
			IID_IObjectWithDescription  ))
		{
			// Object is a single ObjectWithDescription item
			// We expect the embedded object to be a File Processor
			ipObject = m_ipClipboardMgr->GetObjectInClipboard();
			ASSERT_RESOURCE_ALLOCATION( "ELI13495", ipObject != __nullptr );
			bSingleTask = true;
		}
		else
		{
			// Throw exception, object is not a vector of ObjectWithDescription items
			throw UCLIDException( "ELI13496", 
				"Clipboard object is not a File Processor." );
		}

		// Get index of first selection
		int iIndex = m_fileProcessorList.GetFirstSelectedRow();
		
		// Check for item count if no selection
		if (iIndex == -1)
		{
			iIndex = m_fileProcessorList.GetNumberRows();
		}

		// The indexes of all rows added so that they can be selected once the operation is complete.
		set<int> setAddedRows;

		// Handle single-task case
		if (bSingleTask)
		{
			// Clone the object before adding
			ICopyableObjectPtr ipCopyObj = ipObject;
			ASSERT_RESOURCE_ALLOCATION("ELI37121", ipCopyObj != __nullptr);

			// Retrieve File Processor and description
			IObjectWithDescriptionPtr	ipNewFP = ipCopyObj->Clone();
			ASSERT_RESOURCE_ALLOCATION( "ELI13497", ipNewFP != __nullptr );
			string	strDescription( ipNewFP->GetDescription() );

			// Insert the new FP object-with-description into the vector
			// This MUST be done before setting the checked state in the list
			// [LRCAU #5603]
			getFileProcessorsData()->Insert( iIndex, ipNewFP );

			// Insert the item into the list
			m_fileProcessorList.InsertRow(iIndex);

			// Add the description and update the checkbox setting
			m_fileProcessorList.SetText(iIndex, strDescription.c_str());
			m_fileProcessorList.SetCheck(iIndex, asCppBool(ipNewFP->Enabled));

			setAddedRows.insert(iIndex);
		}
		// Handle vector of one-or-more File Processors case
		else
		{
			// Get count of File Processors in Clipboard vector
			IIUnknownVectorPtr	ipPastedFPs = ipObject;
			ASSERT_RESOURCE_ALLOCATION( "ELI13498", ipPastedFPs != __nullptr );
			int iCount = ipPastedFPs->Size();

			// Add each File Processor to the list and the vector
			for (int i = 0; i < iCount; i++, iIndex++)
			{
				// Clone the object before adding
				ICopyableObjectPtr ipCopyObj = ipPastedFPs->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI37120", ipCopyObj != __nullptr);

				// Retrieve File Processor and description
				IObjectWithDescriptionPtr	ipNewFP = ipCopyObj->Clone();
				ASSERT_RESOURCE_ALLOCATION( "ELI13499", ipNewFP != __nullptr );
				string	strDescription( ipNewFP->GetDescription() );

				// Insert the new File Processor object-with-description into the vector
				// This MUST be done before setting the checked state in the list
				// [LRCAU #5603]
				getFileProcessorsData()->Insert(iIndex, ipNewFP );

				// Insert the item into the list
				m_fileProcessorList.InsertRow(iIndex );

				// Add the description and update the checkbox setting
				m_fileProcessorList.SetText(iIndex, strDescription.c_str());
				m_fileProcessorList.SetCheck(iIndex, asCppBool(ipNewFP->Enabled));

				setAddedRows.insert(iIndex);
			}
		}

		// Select the new item(s)
		m_fileProcessorList.ClearSelections();
		for (set<int>::iterator iter = setAddedRows.begin(); iter != setAddedRows.end(); iter++)
		{
			m_fileProcessorList.SelectRow(*iter);
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
LRESULT FileProcessingDlgTaskPage::OnCellValueChange(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Update button states first
		setButtonStates();

		// Extract Row and Column
		short nRow = (short)LOWORD(lParam);
		short nCol = (short)HIWORD(lParam);

		// Only concerned here with whether the check box column of a valid row changed.
		if (nRow < 0 || nCol != 0)
		{
			return 0;
		}

		bool bChecked = m_fileProcessorList.GetCheck(nRow);

		// If this file processor is already present
		IIUnknownVectorPtr ipCollection = getFileProcessorsData();
		ASSERT_RESOURCE_ALLOCATION("ELI15995", ipCollection != __nullptr);
		if (ipCollection->Size() > nRow)
		{
			// Retrieve affected file processor
			IObjectWithDescriptionPtr	ipFP = getFileProcessorsData()->At(nRow);
			ASSERT_RESOURCE_ALLOCATION("ELI15983", ipFP != __nullptr);

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

	return 0;
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
		if (tmpChild != __nullptr && tmpChild->GetDlgCtrlID() == IDC_EDIT_EXECUTE_TASK)
		{
			// get the error task
			IObjectWithDescriptionPtr ipErrorTask = getFPMgmtRole()->ErrorTask;
			ASSERT_RESOURCE_ALLOCATION("ELI16114", ipErrorTask != __nullptr);

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
// Public methods
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::refresh()
{
	// Remove any listed File Processors
	while (m_fileProcessorList.GetNumberRows() > 0)
	{
		m_fileProcessorList.DeleteRow(0);
	}

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

	// Update error log items
	m_bLogErrorDetails = asMFCBool( ipMgmtRole->LogErrorDetails );
	m_zErrorLog = asString( ipMgmtRole->ErrorLogName ).c_str();

	// Update error email UI elements
	m_bSendErrorEmail = asMFCBool(getFPMgmtRole()->SendErrorEmail);

	IErrorEmailTaskPtr ipErrorEmailTask = getFPMgmtRole()->ErrorEmailTask;
	m_zErrorEmailRecipients = (ipErrorEmailTask == __nullptr)
		? ""
		: asString(ipErrorEmailTask->Recipient).c_str();

	// Update error task items
	m_bExecuteErrorTask = asMFCBool( ipMgmtRole->ExecuteErrorTask );

	IObjectWithDescriptionPtr ipTask = ipMgmtRole->ErrorTask;
	ASSERT_RESOURCE_ALLOCATION("ELI16093", ipTask != __nullptr);
	m_zErrorTaskDescription = asString( ipTask->Description ).c_str();

	// Get the processing scope and update radio buttons
	bool bProcessSkipped = asCppBool(ipMgmtRole->ProcessSkippedFiles);
	m_radioProcessAll.SetCheck(asBSTChecked(!bProcessSkipped));
	m_radioProcessSkipped.SetCheck(asBSTChecked(bProcessSkipped));
	m_comboSkipped.SetCurSel(ipMgmtRole->SkippedForAnyUser == VARIANT_FALSE ? 0 : 1);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::setFPMgr(UCLID_FILEPROCESSINGLib::IFileProcessingManager* pFPMgr)
{
	m_pFPM = pFPMgr;
	ASSERT_RESOURCE_ALLOCATION("ELI14083", m_pFPM != __nullptr);
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
		m_fileProcessorList.ClearSelections();
		m_fileProcessorList.EnableWindow(FALSE);
		m_btnUp.EnableWindow(FALSE);
		m_btnDown.EnableWindow(FALSE);

		// Error log and task controls
		m_btnLogErrorDetails.EnableWindow(FALSE);
		m_editErrorLog.EnableWindow(FALSE);
		m_btnErrorSelectTag.EnableWindow(FALSE);
		m_btnBrowseErrorLog.EnableWindow(FALSE);

		// Error email controls
		m_btnSendErrorEmail.EnableWindow(FALSE);
		m_editErrorEmailRecipients.EnableWindow(FALSE);
		m_btnConfigureErrorEmail.EnableWindow(FALSE);

		// Scope controls
		m_radioProcessAll.EnableWindow(FALSE);
		m_radioProcessSkipped.EnableWindow(FALSE);
		m_comboSkipped.EnableWindow(FALSE);
		m_staticSkipped.EnableWindow(FALSE);

		// Error task controls
		m_btnExecuteErrorTask.EnableWindow(FALSE);
		m_btnSelectErrorTask.EnableWindow(FALSE);
		GetDlgItem(IDC_EDIT_EXECUTE_TASK)->EnableWindow(FALSE);

		// Advanced setting controls
		m_btnAdvancedSettings.EnableWindow(FALSE);
	}
	else
	{
		// Error log and task controls
		m_btnLogErrorDetails.EnableWindow(TRUE);
		if (m_bLogErrorDetails)
		{
			m_editErrorLog.EnableWindow(TRUE);
			m_btnErrorSelectTag.EnableWindow(TRUE);
			m_btnBrowseErrorLog.EnableWindow(TRUE);
		}

		// Error email controls
		m_btnSendErrorEmail.EnableWindow(TRUE);
		if (m_bSendErrorEmail)
		{
			m_editErrorEmailRecipients.EnableWindow(TRUE);
			m_btnConfigureErrorEmail.EnableWindow(TRUE);
		}

		// Scope controls
		m_radioProcessAll.EnableWindow(TRUE);
		m_radioProcessSkipped.EnableWindow(TRUE);
		m_staticSkipped.EnableWindow(TRUE);

		// Set the button states that depend on settings
		setButtonStates();
	}
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
	ASSERT_RESOURCE_ALLOCATION("ELI13511", ipObjSelect != __nullptr);

	// initialize private license for the object
	IPrivateLicensedComponentPtr ipPLComponent = ipObjSelect;
	ASSERT_RESOURCE_ALLOCATION("ELI10301", ipPLComponent != __nullptr);
	_bstr_t _bstrKey = LICENSE_MGMT_PASSWORD.c_str();
	ipPLComponent->InitPrivateLicense(_bstrKey);
	
	// first make a copy of the ObjectWithDescption in case 
	// the user cancels out the dialog
	IObjectWithDescriptionPtr ipObjectWithDescription(ipObjWithDesc->Clone());
	ASSERT_RESOURCE_ALLOCATION("ELI13512", ipObjectWithDescription != __nullptr);

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
	ASSERT_RESOURCE_ALLOCATION("ELI10973", ipObject != __nullptr);
	CString zDescription = (char*)ipObject->Description;

	// Add item to end of list, set the text, set the checkbox
	int iIndex = m_fileProcessorList.GetNumberRows();
	m_fileProcessorList.InsertRow(iIndex);
	m_fileProcessorList.SetText(iIndex, (LPCTSTR)zDescription);
	m_fileProcessorList.SetCheck(iIndex, asCppBool(ipObject->Enabled));

	//Update the button states
	setButtonStates();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::deleteSelectedTasks() 
{
	// Get list count
	int iCount = m_fileProcessorList.GetNumberRows();
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

	// Pust the rows to delete to a stack so that they can be deleted in reverse order.
	stack<int> rowsToDelete;
	int iIndex = m_fileProcessorList.GetFirstSelectedRow();
	while (iIndex != -1)
	{
		rowsToDelete.push(iIndex);
		iIndex = m_fileProcessorList.GetNextSelectedRow();
	}

	// Delete the rows in reverse order so that the indexes remain valid.
	while (!rowsToDelete.empty())
	{
		int iIndex = rowsToDelete.top();

		// Remove this item from list
		m_fileProcessorList.DeleteRow(iIndex);

		// Remove this item from the vector of file processors
		getFileProcessorsData()->Remove(iIndex);

		rowsToDelete.pop();
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
	int	iCount = m_fileProcessorList.GetNumberRows();
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
		// Get index of first selection
		int iIndex = m_fileProcessorList.GetFirstSelectedRow();

		if (iIndex > -1)
		{
			// Enable the Delete button
			m_btnRemove.EnableWindow( TRUE );

			// Check for multiple selection
			if (m_fileProcessorList.GetNextSelectedRow() == -1)
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

	// Can always check/uncheck log error details
	m_btnLogErrorDetails.EnableWindow(TRUE); 

	// Enable / disable controls for logging error details
	m_editErrorLog.EnableWindow(m_bLogErrorDetails);
	m_btnErrorSelectTag.EnableWindow(m_bLogErrorDetails);
	m_btnBrowseErrorLog.EnableWindow(m_bLogErrorDetails);

	// Can always check/uncheck send error email
	m_btnSendErrorEmail.EnableWindow(TRUE);

	// Enable / disable controls for configuring error email
	m_editErrorEmailRecipients.EnableWindow(m_bSendErrorEmail);
	m_btnConfigureErrorEmail.EnableWindow(m_bSendErrorEmail);

	// Can always check/uncheck error task
	m_btnExecuteErrorTask.EnableWindow(TRUE);

	// Enable the combo box for skipped files if process skipped files is checked
	m_comboSkipped.EnableWindow(asMFCBool(m_radioProcessSkipped.GetCheck() == BST_CHECKED));

	// Enable / disable error task controls
	GetDlgItem(IDC_EDIT_EXECUTE_TASK)->EnableWindow(m_bExecuteErrorTask);
	m_btnSelectErrorTask.EnableWindow(m_bExecuteErrorTask);

	// Advanced settings
	m_btnAdvancedSettings.EnableWindow(TRUE);
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
	auto ipFPD = getFileProcessorsData();
	ipFPD->Remove( iIndex );
	ipFPD->Insert(iIndex, ipNewFP );

	// Update the file processor listbox
	m_fileProcessorList.DeleteRow(iIndex);
	m_fileProcessorList.InsertRow(iIndex);
	m_fileProcessorList.SetText(iIndex, asString(ipNewFP->Description));
	m_fileProcessorList.SetCheck(iIndex, asCppBool(ipNewFP->Enabled));

	// Retain selection and focus
	m_fileProcessorList.ClearSelections();
	m_fileProcessorList.SelectRow(iIndex);
	m_fileProcessorList.SetFocus();
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr FileProcessingDlgTaskPage::getFPMgr()
{
	UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr ipFPM( m_pFPM );
	ASSERT_RESOURCE_ALLOCATION("ELI14294", ipFPM != __nullptr);

	return ipFPM;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr FileProcessingDlgTaskPage::getFPMgmtRole(
	UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr ipFPM)
{
	// get the file processing mgmt role
	UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr ipFPMgmtRole =
		(ipFPM != __nullptr ? ipFPM : getFPMgr())->FileProcessingMgmtRole;
	ASSERT_RESOURCE_ALLOCATION("ELI14295", ipFPMgmtRole != __nullptr);

	return ipFPMgmtRole;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr FileProcessingDlgTaskPage::getFileProcessorsData()
{
	// get the file supplying mgmt role
	IIUnknownVectorPtr ipFileProcessorsData = getFPMgmtRole()->FileProcessors;
	ASSERT_RESOURCE_ALLOCATION("ELI14310", ipFileProcessorsData != __nullptr);

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
		ASSERT_RESOURCE_ALLOCATION("ELI16130", m_ipMiscUtils != __nullptr);
	}

	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgTaskPage::getDlgItemWindowRect(UINT uiDlgItemResourceID, RECT &rectWindow)
{
	// retrieve the dialog item using its resource ID
	CWnd* cwndDlgItem = GetDlgItem(uiDlgItemResourceID);
	ASSERT_RESOURCE_ALLOCATION("ELI16132", cwndDlgItem != __nullptr);

	// set the window rect to the appropriate position and dimensions
	cwndDlgItem->GetWindowRect(&rectWindow);
}
//-------------------------------------------------------------------------------------------------
