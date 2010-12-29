//==================================================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	UEXViewerDlg.cpp
//
// PURPOSE:	Provide a UI for UCLID Exception files.
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//==================================================================================================

#include "stdafx.h"
#include "UEXViewer.h"
#include "UEXViewerDlg.h"
#include "UEXFindDlg.h"
#include "AboutDlg.h"
#include "ExceptionListControlHelper.h"
#include "ExportDebugDataDlg.h"
#include "MutexUtils.h"

#include <StringTokenizer.h>
#include <UCLIDException.h>
#include <RegistryPersistenceMgr.h>
#include <cpputil.h>
#include <RegConstants.h>
#include <ClipboardManager.h>
#include <SuspendWindowUpdates.h>
#include <TemporaryFileName.h>
#include <PromptDlg.h>
#include <StringCSIS.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

// Number of tokens in UEX file
#define	OLD_UEX_TOKEN_COUNT			7

//-------------------------------------------------------------------------------------------------
// Persistence Information
//-------------------------------------------------------------------------------------------------

// Persistence Folders
const string COLUMNS			= "\\Columns";
const string GENERAL			= "\\General";

// Persistence Keys
const string DIRECTORY			= "Directory";
const string WINDOW_POS_X		= "WindowPositionX";
const string WINDOW_POS_Y		= "WindowPositionY";
const string WINDOW_SIZE_X		= "WindowSizeX";
const string WINDOW_SIZE_Y		= "WindowSizeY";
const string SERIAL_WIDTH		= "SerialWidth";
const string APPLICATION_WIDTH	= "ApplicationWidth";
const string COMPUTER_WIDTH		= "ComputerWidth";
const string USER_WIDTH			= "UserWidth";
const string PID_WIDTH			= "PidWidth";
const string TIME_WIDTH			= "TimeWidth";
const string ELI_WIDTH			= "ELICodeWidth";
const string EXCEPTION_WIDTH	= "ExceptionWidth";

// Window Size Bounds
#define	MIN_WINDOW_X				731
#define	MIN_WINDOW_Y				200

// Column Width Bounds
#define	MIN_WIDTH					0
#define	MAX_WIDTH					400

//-------------------------------------------------------------------------------------------------
// CUEXViewerDlg dialog
//-------------------------------------------------------------------------------------------------
CUEXViewerDlg::CUEXViewerDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CUEXViewerDlg::IDD, pParent),
	m_iSerialColumnWidth(80),
	m_iELIColumnWidth(60),
	m_iExceptionColumnWidth(200),
	m_iApplicationColumnWidth(170),
	m_iComputerColumnWidth(80),
	m_iUserColumnWidth(100),
	m_iPidColumnWidth(80),
	m_iTimeColumnWidth(130),
	m_bInitialized(false)
{
	try
	{
		m_zDirectory = _T("");

		// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
		m_hIcon = AfxGetApp()->LoadIcon( IDR_UEXVIEW );

		// Use Registry for data persistence
		string strRootFolder = gstrREG_ROOT_KEY + "\\Utilities\\UEXViewer";
		m_pCfgMgr = new RegistryPersistenceMgr( HKEY_CURRENT_USER, strRootFolder );

		m_apLogFileMutex.reset(getGlobalNamedMutex(gstrLOG_FILE_MUTEX));
		ASSERT_RESOURCE_ALLOCATION("ELI29995", m_apLogFileMutex.get() != NULL); 
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29996");
}
//-------------------------------------------------------------------------------------------------
CUEXViewerDlg::~CUEXViewerDlg()
{
	try
	{
		// Close and clean-up Find dialog
		if (ma_pFindDlg.get())
		{
			// ensure the window has been created before trying to destroy it
			if (asCppBool(::IsWindow(ma_pFindDlg->m_hWnd)))
			{
				ma_pFindDlg->DestroyWindow();
			}
			ma_pFindDlg.reset(NULL);
		}
		
		// Close and clean-up ExportDebug dialog
		if (ma_pExportDebugDataDlg.get())
		{
			// ensure the window has been created before trying to destroy it
			if (asCppBool(::IsWindow(ma_pExportDebugDataDlg->m_hWnd)))
			{
				ma_pExportDebugDataDlg->DestroyWindow();
			}
			ma_pExportDebugDataDlg.reset(NULL);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16546");
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_LIST_UEX, m_listUEX);
	DDX_Control(pDX, IDC_COMBO_EXCEPTION_FILE_LIST, m_comboExceptionsList);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CUEXViewerDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_DROPFILES()
	ON_WM_QUERYDRAGICON()
	ON_NOTIFY(NM_DBLCLK, IDC_LIST_UEX, OnDblclkListUex)
	ON_COMMAND(ID_FILE_OPEN, OnFileOpen)
	ON_COMMAND(ID_EDIT_PASTE, OnEditPaste)
	ON_COMMAND(ID_EDIT_CLEAR, OnEditClear)
	ON_COMMAND(ID_EXCEPTION_VIEW_DETAILS, OnEditViewDetails)
	ON_NOTIFY(NM_CLICK, IDC_LIST_UEX, OnClickListUex)
	ON_NOTIFY(LVN_COLUMNCLICK, IDC_LIST_UEX, OnColumnclickListUex)
	ON_COMMAND(ID_EDIT_DELETE_SELECTION, OnEditDeleteSelection)
	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
	ON_COMMAND(ID_FILE_SAVE_AS, OnFileExport)
	ON_COMMAND(ID_FILE_EXIT, OnFileExit)
	ON_COMMAND(ID_FILE_OPEN_PREV_LOG_FILE, OnFileOpenPrevLogFile)
	ON_COMMAND(ID_FILE_OPEN_NEXT_LOG_FILE, OnFileOpenNextLogFile)
	ON_COMMAND(ID_EDIT_INVERT_SELECTION, OnEditInvertSelection)
	ON_COMMAND(ID_EDIT_FIND, OnEditFind)
	ON_COMMAND(ID_HELP_ABOUT, OnHelpAbout)
	ON_COMMAND(ID_ELILISTCONTEXT_COPYELICODE, OnCopyELICode)
	ON_WM_CLOSE()
	ON_BN_CLICKED(ID_BTN_PREV_LOG_FILE, &CUEXViewerDlg::OnBnClickedBtnPrevLogFile)
	ON_BN_CLICKED(ID_BTN_NEXT_LOG_FILE, &CUEXViewerDlg::OnBnClickedBtnNextLogFile)
	ON_CBN_SELCHANGE(IDC_COMBO_EXCEPTION_FILE_LIST, &CUEXViewerDlg::OnCbnSelchangeComboExceptionFileList)
	ON_COMMAND(ID_FILE_REFRESHCURRENTLOGFILE, &CUEXViewerDlg::OnFileRefreshCurrentLogfile)
	ON_COMMAND(ID_FILE_START_NEW_LOG_FILE, &CUEXViewerDlg::OnFileStartNewLogFile)
	ON_NOTIFY(NM_RCLICK, IDC_LIST_UEX, &CUEXViewerDlg::OnNMRclickListUex)
	ON_COMMAND(ID_TOOLS_EXPORTDEBUGDATA, &CUEXViewerDlg::OnToolsExportDebugData)
	ON_COMMAND(ID_ELILISTCONTEXT_MATCHING_TOPLEVEL, &CUEXViewerDlg::OnSelectMatchingTopLevelExceptions)
	ON_COMMAND(ID_ELILISTCONTEXT_MATCHING_HIERARCHIES, &CUEXViewerDlg::OnSelectMatchingExceptionHierarchies)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CUEXViewerDlg public methods
//-------------------------------------------------------------------------------------------------
int CUEXViewerDlg::GetExceptionCount()
{
	return m_listUEX.GetItemCount();
}
//-------------------------------------------------------------------------------------------------
int CUEXViewerDlg::GetFirstSelectionIndex()
{
	// Default to no item selected
	int iFirst = -1;

	// Find position of first selection
	POSITION pos = m_listUEX.GetFirstSelectedItemPosition();
	if (pos != NULL)
	{
		// Get index of selected item
		iFirst = m_listUEX.GetNextSelectedItem( pos );
	}

	return iFirst;
}
//-------------------------------------------------------------------------------------------------
string CUEXViewerDlg::GetWholeExceptionString(int nIndex)
{
	// Validate index
	int iCount = m_listUEX.GetItemCount();
	if (nIndex >= iCount)
	{
		UCLIDException ue("ELI13416", "Invalid exception index!");
		ue.addDebugInfo("nIndex", nIndex);
		ue.addDebugInfo("Count", iCount);
		throw ue;
	}

	// Retrieve this data structure
	ITEMINFO*	pData = (ITEMINFO *)m_listUEX.GetItemData( nIndex );
	ASSERT_RESOURCE_ALLOCATION("ELI28706", pData != NULL);

	// Create a UCLIDException object
	// using new ELI code for this application
	UCLIDException	ue;
	ue.createFromString("ELI13417", pData->strData);

	// Convert this item to a displayable string and return
	string strFullText;
	ue.asString( strFullText );
	return strFullText;
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::SelectExceptions(vector<int> &rvecExceptionIndices)
{
	// Clear selection
	int iCount = m_listUEX.GetItemCount();
	for (int i = 0; i < iCount; i++)
	{
		// Clear selection state
		m_listUEX.SetItemState( i, 0, LVIS_SELECTED );
	}

	// Select items from ordered collection
	unsigned int uiSelectCount = rvecExceptionIndices.size();
	for (unsigned int uj = 0; uj < uiSelectCount; uj++)
	{
		// Get and validate this index
		int iIndex = rvecExceptionIndices[uj];
		if (iIndex >= iCount)
		{
			break;
		}

		// Set selection state
		m_listUEX.SetItemState( iIndex, LVIS_SELECTED, LVIS_SELECTED );
	}

	// Scroll first item into view
	if (uiSelectCount > 0)
	{
		m_listUEX.EnsureVisible( rvecExceptionIndices[0], FALSE );
	}

	// Done setting selection, refresh the display
	updateEnabledStateForControls();
	m_listUEX.SetFocus();
}

//-------------------------------------------------------------------------------------------------
// CUEXViewerDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CUEXViewerDlg::OnInitDialog()
{
	try
	{
		CDialog::OnInitDialog();

		// Set the icon for this dialog.  The framework does this automatically
		//  when the application's main window is not a dialog
		SetIcon( m_hIcon, TRUE );		// Set big icon
		SetIcon( m_hIcon, FALSE );		// Set small icon

		// Construct and create a modeless Find dialog
		ma_pFindDlg = auto_ptr<CUEXFindDlg>(new CUEXFindDlg( m_pCfgMgr, this ));
		ma_pFindDlg->Create( CUEXFindDlg::IDD, NULL );

		// Contruct and create a modeless export data dialog
		ma_pExportDebugDataDlg = auto_ptr<CExportDebugDataDlg>(new CExportDebugDataDlg(this));
		ma_pExportDebugDataDlg->Create(CExportDebugDataDlg::IDD, NULL);

		// Set flag
		m_bInitialized = true;

		// Retrieve settings
		initPersistent();

		// Setup list for UEX files
		prepareList();

		// by default, open the window as if it was just cleared
		OnEditClear();

		// Check for command line entry
		CString	zCommand;
		zCommand = AfxGetApp()->m_lpCmdLine;
		if (zCommand.GetLength() > 0)
		{
			// Remove quotes from path
			zCommand.Replace( "\"", "" );

			// Only load exceptions if the file exists (do not throw an exception
			// if the file does not exist) [LRCAU #5530]
			if (isValidFile((LPCTSTR) zCommand))
			{
				// Load this file
				addExceptions( (LPCTSTR) zCommand, true );
			}
			else
			{
				string strMessage = "The specified exception file does not exist. It may have "
					"been moved, deleted, or not created yet.\nFile Name: ";
				strMessage += (LPCTSTR)zCommand;
				AfxMessageBox(strMessage.c_str(), MB_OK | MB_ICONINFORMATION);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14820")
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
BOOL CUEXViewerDlg::DestroyWindow() 
{
	try
	{
		// Release allocated ITEMINFO memory
		int iCount = m_listUEX.GetItemCount();
		for (int i = 0; i < iCount; i++)
		{
			// Retrieve this data structure
			ITEMINFO*	pData = (ITEMINFO *)m_listUEX.GetItemData( i );

			// Release the memory
			if (pData != NULL)
			{
				delete pData;
				pData = NULL;
			}
		}

		// Release persistence manager
		delete m_pCfgMgr;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14821")

	return CDialog::DestroyWindow();
}
//-------------------------------------------------------------------------------------------------
BOOL CUEXViewerDlg::PreTranslateMessage(MSG* pMsg) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (pMsg->message == WM_KEYDOWN)
		{
			// translate accelerators
			static HACCEL hAccel = LoadAccelerators(AfxGetApp()->m_hInstance, 
				MAKEINTRESOURCE(IDR_ACCELERATOR_UEXVIEWER_DLG));
			if (TranslateAccelerator(m_hWnd, hAccel, pMsg))
			{
				// since the message has been handled, no further dispatch is needed
				return TRUE;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14822")

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CUEXViewerDlg::OnPaint() 
{
	try
	{
		if (IsIconic())
		{
			CPaintDC dc(this); // device context for painting

			SendMessage(WM_ICONERASEBKGND, (WPARAM) dc.GetSafeHdc(), 0);

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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14823")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnOK()
{
	try
	{
		// do nothing with the ENTER key is pressed
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14824")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnCancel()
{
	try
	{
		// do nothing when the ESC key is pressed.
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14825")
}
//-------------------------------------------------------------------------------------------------
// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CUEXViewerDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	try
	{
		// Minimum width to allow display of buttons
		lpMMI->ptMinTrackSize.x = MIN_WINDOW_X;

		// Minimum height to display edit box, tree, and buttons
		lpMMI->ptMinTrackSize.y = MIN_WINDOW_Y;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14828")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnSize(UINT nType, int cx, int cy) 
{
	try
	{
		CDialog::OnSize(nType, cx, cy);
		
		if (m_bInitialized)
		{
			// get the dialog's client coords
			CRect dlgClientRect;
			GetClientRect(&dlgClientRect);

			// Find the top/left position of the list control
			// The spacing above the list and to the left of the list
			// will determine the horizontal and vertical spacing in the
			// rest of the window
			CRect rectList;
			m_listUEX.GetWindowRect(&rectList);
			ScreenToClient( rectList );
			long nSpacingX = rectList.left;
			long nSpacingY = rectList.top;

			// re-position the next-file button
			CWnd *pNextButton = GetDlgItem(ID_BTN_NEXT_LOG_FILE);
			ASSERT_RESOURCE_ALLOCATION("ELI14856", pNextButton != NULL);
			CRect rectNextButton;
			pNextButton->GetWindowRect(&rectNextButton);
			ScreenToClient(rectNextButton);
			rectNextButton.MoveToXY(dlgClientRect.Width() - nSpacingX - rectNextButton.Width(),
				dlgClientRect.Height() - nSpacingY - rectNextButton.Height());
			pNextButton->MoveWindow(&rectNextButton);

			// reposition the prev-file button
			CWnd *pPrevButton = GetDlgItem(ID_BTN_PREV_LOG_FILE);
			ASSERT_RESOURCE_ALLOCATION("ELI14857", pPrevButton != NULL);
			CRect rectPrevButton;
			pPrevButton->GetWindowRect(&rectPrevButton);
			ScreenToClient(rectPrevButton);
			rectPrevButton.MoveToXY(rectNextButton.left - nSpacingX - rectPrevButton.Width(),
				dlgClientRect.Height() - nSpacingY - rectPrevButton.Height());
			pPrevButton->MoveWindow(&rectPrevButton);

			// reposition the drop-down list box with the exception file list
			CWnd *pExList = GetDlgItem(IDC_COMBO_EXCEPTION_FILE_LIST);
			ASSERT_RESOURCE_ALLOCATION("ELI14858", pExList != NULL);
			CRect rectExList;
			pExList->GetWindowRect(&rectExList);
			ScreenToClient(rectExList);
			rectExList.MoveToXY(nSpacingX, dlgClientRect.Height() - nSpacingY - rectExList.Height());
			rectExList.right = rectPrevButton.left - nSpacingX;
			pExList->MoveWindow(&rectExList);

			// resposition the label above the exception file list dropdown
			CWnd *pExListLabel = GetDlgItem(IDC_STATIC_EXCEPTION_FILE_LIST);
			ASSERT_RESOURCE_ALLOCATION("ELI14859", pExListLabel != NULL);
			CRect rectExListLabel;
			pExListLabel->GetWindowRect(&rectExListLabel);
			ScreenToClient(rectExListLabel);
			rectExListLabel.MoveToXY(nSpacingX, rectExList.top - nSpacingY - rectExListLabel.Height());
			pExListLabel->MoveWindow(&rectExListLabel);

			// reposition the main list control
			rectList.right = dlgClientRect.Width() - nSpacingX;
			rectList.bottom = rectExListLabel.top - nSpacingY;
			m_listUEX.MoveWindow(&rectList);

			// redraw the window
			Invalidate();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14829")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnDblclkListUex(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		// View the selected exception
		OnEditViewDetails();
		
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14830")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnEditViewDetails() 
{
	try
	{
		// Determine which list item is selected
		POSITION pos = m_listUEX.GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			// Display error message
			MessageBox( "Unable to view item without selection", "Error", 
				MB_ICONEXCLAMATION | MB_OK );
		}
		else
		{
			// Get index of selected item
			int iItem = m_listUEX.GetNextSelectedItem( pos );

			// Retrieve this data structure
			ITEMINFO*	pData = (ITEMINFO *)m_listUEX.GetItemData( iItem );
			ASSERT_RESOURCE_ALLOCATION("ELI28707", pData != NULL);

			// Create a UCLIDException object
			// using new ELI code for this application
			UCLIDException	ue;
			ue.createFromString("ELI02154", pData->strData);

			// Do not add this exception to the log
			// Force display of this exception even if it is a License Corruption 
			// exception and we have seen one before (P13 #4216)
			ue.display( false, true );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14831");
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnFileOpen() 
{
	try
	{
		// Show File Open dialog
		CFileDialog openFileDlg( TRUE, NULL, NULL, 
			OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST, 
		"UEX files (*.uex)|*.uex"
		"||", NULL );

		// Modify the initial directory
		openFileDlg.m_ofn.lpstrInitialDir = (LPCTSTR) m_zDirectory;

		if (openFileDlg.DoModal() == IDOK)
		{
			// Store folder of selected file
			CString	zPath = openFileDlg.GetPathName();
			m_zDirectory = getDirectoryFromFullPath( (LPCTSTR) zPath ).c_str();

			// Add exceptions from selected file to UEX list
			addExceptions( (LPCTSTR) zPath, true );
		}

		// Manage button states
		updateEnabledStateForControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14832")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnFileExit() 
{
	try
	{
		// send message to close
		SendMessage(WM_CLOSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14833")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnHelpAbout() 
{
	try
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14834")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnFileOpenPrevLogFile() 
{
	try
	{
		// get the current selected index
		int iCurSel = m_comboExceptionsList.GetCurSel();

		// verify that a iCurSel is a valid value and that we can navigate backwards
		if (iCurSel <= 0 || iCurSel >= m_comboExceptionsList.GetCount())
		{
			// this should never happen with the way the UI is coded
			THROW_LOGIC_ERROR_EXCEPTION("ELI14851");
		}

		// get the string at the previous index
		CString zPrevFile;
		m_comboExceptionsList.GetLBText(iCurSel - 1, zPrevFile);

		// compute the full path to the previous file
		string strPrevFile = getDirectoryFromFullPath(m_strCurrentFile);
		strPrevFile += "\\";
		strPrevFile += (LPCTSTR) zPrevFile;

		// open the exceptions from the previous file
		addExceptions(strPrevFile, true);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14835")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnFileOpenNextLogFile() 
{
	try
	{
		// get the current selected index
		int iCurSel = m_comboExceptionsList.GetCurSel();

		// verify that a iCurSel is a valid value and that we can navigate forwards
		if (iCurSel < 0 || iCurSel >= m_comboExceptionsList.GetCount() - 1)
		{
			// this should never happen with the way the UI is coded
			THROW_LOGIC_ERROR_EXCEPTION("ELI14852");
		}

		// get the string at the next index
		CString zNextFile;
		m_comboExceptionsList.GetLBText(iCurSel + 1, zNextFile);

		// compute the full path to the next file
		string strNextFile = getDirectoryFromFullPath(m_strCurrentFile);
		strNextFile += "\\";
		strNextFile += (LPCTSTR) zNextFile;

		// open the exceptions from the next file
		addExceptions(strNextFile, true);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14836")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnFileRefreshCurrentLogfile()
{
	try
	{
		// check if the m_strCurrentFile is empty, if it is
		// then we cannot refresh
		if (m_strCurrentFile != "")
		{
			// call addExceptions with replace mode true
			addExceptions(m_strCurrentFile, true);
		}
		else
		{
			AfxMessageBox("No file open, cannot refresh!");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16835")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnFileStartNewLogFile()
{
	try
	{
		// Prompt the user for a comment
		PromptDlg dlg("Add Rename Comment", "Comment", "", true, true, true, this);
		if (dlg.DoModal() == IDOK)
		{
			// Rename the log file
			UCLIDException::renameLogFile(UCLIDException::getDefaultLogFileFullPath(),
				true, (LPCTSTR) dlg.m_zInput, true);

			// Open the new log file
			addExceptions(m_strCurrentFile, true);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29953");
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnEditClear() 
{
	try
	{
		// Show the wait cursor and suspend window updates
		CWaitCursor wait;
		SuspendWindowUpdates suspend(*this);

		// Release allocated ITEMINFO memory
		int iCount = m_listUEX.GetItemCount();
		for (int i = 0; i < iCount; i++)
		{
			// Retrieve this data structure
			ITEMINFO*	pData = (ITEMINFO *)m_listUEX.GetItemData( i );

			// Release the memory
			if (pData != NULL)
			{
				delete pData;
				pData = NULL;
			}
		}

		// Clear the list
		m_listUEX.DeleteAllItems();

		// set the new current file to be nothing
		setNewCurrentFile("");

		// Manage button states
		updateEnabledStateForControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14837")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnEditPaste() 
{
	try
	{
		// Retrieve text Clipboard contents
		string strText;
		ClipboardManager	mgr( this );
		mgr.readText( strText );

		if (strText.length() > 0)
		{
			// Remove any carriage-return characters
			int iPos = strText.find_first_of( "\r\n" );
			while (iPos != string::npos)
			{
				// Erase this character
				strText.erase( iPos, 1 );

				// Keep searching
				iPos = strText.find_first_of( "\r\n" );
			}

			// Prepend commas to text to represent a line of text from a UEX file
			strText = string( ",,,,,," ) + strText;

			// Parse the text and add record to list
			parseLine( strText );

			// Select the new UEX record
			m_listUEX.SetItemState( 0, LVIS_SELECTED, LVIS_SELECTED );

			// Manage button states
			updateEnabledStateForControls();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14838")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnEditDeleteSelection() 
{
	try
	{
		// Show the wait cursor and suspend window updates
		CWaitCursor wait;
		SuspendWindowUpdates suspend(*this);

		// Determine which list item is selected
		POSITION pos = m_listUEX.GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			// Display error message
			MessageBox( "Unable to delete item without selection", "Error", 
				MB_ICONEXCLAMATION | MB_OK );
		}
		else
		{
			// Display wait cursor in case this is a lengthy operation - P13 #3915
			CWaitCursor wait;
			int iItem = 0;
			while (pos)
			{
				// Get index of selected item
				iItem = m_listUEX.GetNextSelectedItem( pos );

				// Retrieve this data structure
				ITEMINFO*	pData = (ITEMINFO *)m_listUEX.GetItemData( iItem );

				// Release the memory
				if (pData != NULL)
				{
					delete pData;
					pData = NULL;
				}

				// Clear ItemData to prepare for deletion
				m_listUEX.SetItemData( iItem, 0 );
			}

			// Delete each list item without assocated ITEMDATA structure
			int iCount = m_listUEX.GetItemCount();
			int i;
			for (i = 0; i < iCount; i++)
			{
				// Retrieve this data structure
				ITEMINFO*	pData = (ITEMINFO *)m_listUEX.GetItemData( i );

				// Check for NULL
				if (pData == NULL)
				{
					// Delete the item
					m_listUEX.DeleteItem( i );
					// Adjust index and count
					i--;
					iCount--;
				}
			}

			// Update selection to item underneath last selected (deleted)
			int iSelected = iItem;

			// Make sure that too many items weren't deleted
			if (iSelected >= iCount)
			{
				// Just default to last item
				iSelected = iCount - 1;
			}

			// Set the selection
			m_listUEX.SetItemState( iSelected, LVIS_SELECTED, LVIS_SELECTED );

			// Manage button states
			updateEnabledStateForControls();

			// Refresh index items in ItemData structures
			refreshIndices();

			// Restore focus to list
			m_listUEX.SetFocus();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14839")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnFileExport() 
{
	try
	{
		// Open a dialog to select a different directory
		// bring up the open dialog box here....
		CFileDialog dlg( FALSE, "uex", "export.uex", 
			OFN_PATHMUSTEXIST | OFN_NOREADONLYRETURN | OFN_OVERWRITEPROMPT,
			"UEX files (*.uex)|*.uex"
			"||", NULL);

		// Modify the initial directory
		dlg.m_ofn.lpstrInitialDir = (LPCTSTR) m_zDirectory;

		if (dlg.DoModal() == IDOK)
		{
			// Retrieve full path of selected file
			CString	zPath = dlg.GetPathName();

			// Save list information to file
			CStdioFile	outFile;
			if (!outFile.Open((LPCTSTR) zPath, 
				CFile::modeCreate | CFile::modeReadWrite ))
			{
				MessageBox( "Failed to open output file.", "Error" );
			}
			else
			{
				// Loop through listed exceptions
				string		strSerial;
				string		strApp;
				string		strComputer;
				string		strUser;
				string		strPid;
				string		strTime;
				string		strData;
				string		strComma = ",";
				string		strCombined;
				ITEMINFO*	pData = NULL;
				CString		zTemp;

				// Save listed exceptions in reverse order so that newest 
				// exceptions remain at the top of the list
				// WEL 11/09/06 ( P13 #3902 )
				int iCount = m_listUEX.GetItemCount();
				for (int i = iCount-1; i >= 0; i--)
				{
					// Retrieve strings from list
					zTemp = m_listUEX.GetItemText( i, SERIAL_LIST_COLUMN );
					strSerial = (LPCTSTR) zTemp;

					zTemp = m_listUEX.GetItemText( i, APPLICATION_LIST_COLUMN );
					strApp = (LPCTSTR) zTemp;

					zTemp = m_listUEX.GetItemText( i, COMPUTER_LIST_COLUMN );
					strComputer = (LPCTSTR) zTemp;

					zTemp = m_listUEX.GetItemText( i, USER_LIST_COLUMN );
					strUser = (LPCTSTR) zTemp;

					zTemp = m_listUEX.GetItemText( i, PID_LIST_COLUMN );
					strPid = (LPCTSTR) zTemp;

					// Retrieve ItemData
					pData = (ITEMINFO *)m_listUEX.GetItemData( i );
					ASSERT_RESOURCE_ALLOCATION("ELI28708", pData != NULL);

					// Store time info as string
					strTime = asString( pData->ulTime );

					// Extract exception data to string
					strData = pData->strData;

					// Create string combined elements
					strCombined = 
						strSerial + strComma +
						strApp + strComma + 
						strComputer + strComma +
						strUser + strComma + 
						strPid + strComma + 
						strTime + strComma + 
						strData + "\n";

					// Write string of exception data to file
					outFile.WriteString( strCombined.c_str() );
				}

				// Close the file
				outFile.Close();
				waitForFileToBeReadable((LPCTSTR)zPath);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14840")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnEditInvertSelection() 
{
	try
	{
		// Loop through all listed exceptions
		int iCount = m_listUEX.GetItemCount();
		for (int i = 0; i < iCount; i++)
		{
			// Get selection state of this item
			UINT uiState = m_listUEX.GetItemState( i, LVIS_SELECTED );

			// Toggle selection state
			UINT uiNewState = (uiState == 0) ? LVIS_SELECTED : 0;
			m_listUEX.SetItemState( i, uiNewState, LVIS_SELECTED );
		}

		// Update button states
		updateEnabledStateForControls();

		// Set focus back to list to refresh painted selection state
		m_listUEX.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14841")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnEditFind() 
{
	try
	{
		if (ma_pFindDlg.get() && asCppBool(::IsWindow(ma_pFindDlg->m_hWnd)))
		{
			// Show the find dialog
			ma_pFindDlg->ShowWindow(SW_SHOW);
		}
		else
		{
			throw UCLIDException("ELI18618", "Find dialog has not been initialized!");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14842")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnClickListUex(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		// Manage button states
		updateEnabledStateForControls();
	
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14843")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnColumnclickListUex(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		NM_LISTVIEW* pNMListView = (NM_LISTVIEW*)pNMHDR;

		// Compare items based on which column was clicked
		switch( pNMListView->iSubItem )
		{
		case TOP_ELI_COLUMN:
			// Sort the items using appropriate callback procedure.
			m_listUEX.SortItems( ELICompareProc, (LPARAM)(&m_listUEX) );
			break;

		case TOP_EXCEPTION_COLUMN:
			// Sort the items using appropriate callback procedure.
			m_listUEX.SortItems( ExceptionCompareProc, (LPARAM)(&m_listUEX) );
			break;

		case SERIAL_LIST_COLUMN:
			// Sort the items using appropriate callback procedure.
			m_listUEX.SortItems( SerialCompareProc, (LPARAM)(&m_listUEX) );
			break;

		case APPLICATION_LIST_COLUMN:
			// Sort the items using appropriate callback procedure.
			m_listUEX.SortItems( ApplicationCompareProc, (LPARAM)(&m_listUEX) );
			break;

		case COMPUTER_LIST_COLUMN:
			// Sort the items using appropriate callback procedure.
			m_listUEX.SortItems( ComputerCompareProc, (LPARAM)(&m_listUEX) );
			break;

		case USER_LIST_COLUMN:
			// Sort the items using appropriate callback procedure.
			m_listUEX.SortItems( UserCompareProc, (LPARAM)(&m_listUEX) );
			break;

		case PID_LIST_COLUMN:
			// Sort the items using appropriate callback procedure.
			m_listUEX.SortItems( PidCompareProc, (LPARAM)(&m_listUEX) );
			break;

		case TIME_LIST_COLUMN:
			// Sort the items using appropriate callback procedure.
			m_listUEX.SortItems( TimeCompareProc, (LPARAM)(&m_listUEX) );
			break;

		default:
			// Ignore this notification
			break;
		}

		UpdateData( FALSE );

		// Refresh index items in ItemData structures
		refreshIndices();

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14844")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnDropFiles(HDROP hDropInfo)
{
	try
	{
		unsigned int uiNumFiles = DragQueryFile(hDropInfo, 0xFFFFFFFF, NULL, NULL);
		for (unsigned int ui = 0; ui < uiNumFiles; ui++)
		{
			char pszFile[MAX_PATH+1];
			DragQueryFile(hDropInfo, ui, pszFile, MAX_PATH);
			
			// when a file is dragged and dropped, assume that the file should be
			// appeneded to the existing contents if the CTRL key is currently pressed
			// and that if the CTRL key is not currently pressed, the file's contents should
			// replace the current contents
			addExceptions(pszFile, !isVirtKeyCurrentlyPressed(VK_CONTROL));
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14845")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnClose()
{
	try
	{
		UpdateData( FALSE );

		//////////////////////////////
		// Retrieve and store settings
		//////////////////////////////
		CRect	rect;
		char	pszKey[20];
		string	strKey;

		// Window position and size
		GetWindowRect( &rect );
		sprintf_s( pszKey, "%d", rect.left );
		m_pCfgMgr->setKeyValue( GENERAL, WINDOW_POS_X, pszKey );

		sprintf_s( pszKey, "%d", rect.top );
		m_pCfgMgr->setKeyValue( GENERAL, WINDOW_POS_Y, pszKey );

		sprintf_s( pszKey, "%d", rect.Width() );
		m_pCfgMgr->setKeyValue( GENERAL, WINDOW_SIZE_X, pszKey );

		sprintf_s( pszKey, "%d", rect.Height() );
		m_pCfgMgr->setKeyValue( GENERAL, WINDOW_SIZE_Y, pszKey );

		// Directory information
		strKey = (LPCTSTR) m_zDirectory;
		m_pCfgMgr->setKeyValue( GENERAL, DIRECTORY, strKey, true );

		// ELI Code column width
		(m_listUEX.GetHeaderCtrl())->GetItemRect( TOP_ELI_COLUMN, &rect );
		m_iELIColumnWidth = rect.Width();
		sprintf_s( pszKey, "%d", m_iELIColumnWidth );
		strKey = pszKey;
		m_pCfgMgr->setKeyValue( COLUMNS, ELI_WIDTH, strKey, true );

		// Exception column width
		(m_listUEX.GetHeaderCtrl())->GetItemRect( TOP_EXCEPTION_COLUMN, &rect );
		m_iExceptionColumnWidth = rect.Width();
		sprintf_s( pszKey, "%d", m_iExceptionColumnWidth );
		strKey = pszKey;
		m_pCfgMgr->setKeyValue( COLUMNS, EXCEPTION_WIDTH, strKey, true );

		// Serial number column width
		(m_listUEX.GetHeaderCtrl())->GetItemRect( SERIAL_LIST_COLUMN, &rect );
		m_iSerialColumnWidth = rect.Width();
		sprintf_s( pszKey, "%d", m_iSerialColumnWidth );
		strKey = pszKey;
		m_pCfgMgr->setKeyValue( COLUMNS, SERIAL_WIDTH, strKey, true );

		// Application name and version column width
		(m_listUEX.GetHeaderCtrl())->GetItemRect( APPLICATION_LIST_COLUMN, &rect );
		m_iApplicationColumnWidth = rect.Width();
		sprintf_s( pszKey, "%d", m_iApplicationColumnWidth );
		strKey = pszKey;
		m_pCfgMgr->setKeyValue( COLUMNS, APPLICATION_WIDTH, strKey, true );
		
		// Computer name column width
		(m_listUEX.GetHeaderCtrl())->GetItemRect( COMPUTER_LIST_COLUMN, &rect );
		m_iComputerColumnWidth = rect.Width();
		sprintf_s( pszKey, "%d", m_iComputerColumnWidth );
		strKey = pszKey;
		m_pCfgMgr->setKeyValue( COLUMNS, COMPUTER_WIDTH, strKey, true );
		
		// User name column width
		(m_listUEX.GetHeaderCtrl())->GetItemRect( USER_LIST_COLUMN, &rect );
		m_iUserColumnWidth = rect.Width();
		sprintf_s( pszKey, "%d", m_iUserColumnWidth );
		strKey = pszKey;
		m_pCfgMgr->setKeyValue( COLUMNS, USER_WIDTH, strKey, true );
		
		// Process ID column width
		(m_listUEX.GetHeaderCtrl())->GetItemRect( PID_LIST_COLUMN, &rect );
		m_iPidColumnWidth = rect.Width();
		sprintf_s( pszKey, "%d", m_iPidColumnWidth );
		strKey = pszKey;
		m_pCfgMgr->setKeyValue( COLUMNS, PID_WIDTH, strKey, true );
			
		// Time column width
		(m_listUEX.GetHeaderCtrl())->GetItemRect( TIME_LIST_COLUMN, &rect );
		m_iTimeColumnWidth = rect.Width();
		sprintf_s( pszKey, "%d", m_iTimeColumnWidth );
		strKey = pszKey;
		m_pCfgMgr->setKeyValue( COLUMNS, TIME_WIDTH, strKey, true );

		// close the window and exit
		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14847")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnBnClickedBtnPrevLogFile()
{
	try
	{
		OnFileOpenPrevLogFile();

		// Set the focus back to list box [LRCU #4108]
		m_listUEX.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14848")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnBnClickedBtnNextLogFile()
{
	try
	{
		OnFileOpenNextLogFile();

		// Set the focus back to list box [LRCU #4108]
		m_listUEX.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14849")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnCbnSelchangeComboExceptionFileList()
{
	try
	{
		// Show the wait cursor and suspend window updates
		CWaitCursor wait;
		SuspendWindowUpdates suspend(*this);

		// get the current selected index
		int iCurSel = m_comboExceptionsList.GetCurSel();

		// get the string at the current index
		CString zCurrFile;
		m_comboExceptionsList.GetLBText(iCurSel, zCurrFile);

		// compute the full path to the currently selected file
		string strCurrFile = getDirectoryFromFullPath(m_strCurrentFile);
		strCurrFile += "\\";
		strCurrFile += (LPCTSTR) zCurrFile;

		// open the exceptions from the currently selected file
		addExceptions(strCurrFile, true);

		// Set the focus back to list box [LRCU #4108]
		m_listUEX.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14850")
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnNMRclickListUex(NMHDR *pNMHDR, LRESULT *pResult)
{
	try
	{
		*pResult = 0;

		// Get the selected position in the list
		POSITION pos = m_listUEX.GetFirstSelectedItemPosition();

		// return if there is no selection or more than one item is selected
		if (pos == NULL || m_listUEX.GetSelectedCount() > 1)
		{
			return;
		}

		// Create the list context menu
		CMenu menu;
		menu.LoadMenu( IDR_ELI_LIST_CONTEXT_MENU );
		CMenu *pContextMenu = menu.GetSubMenu( 0 );

		// Get the cursor position
		CPoint	point;
		GetCursorPos( &point );

		// Display the context menu
		pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
			point.x, point.y, this );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28681");
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnCopyELICode()
{
	try
	{
		POSITION pos = m_listUEX.GetFirstSelectedItemPosition();

		// Don't do anything unless one and only one item is selected.
		if (pos == NULL || m_listUEX.GetSelectedCount() > 1)
		{
			return;
		}

		// Get index of selected item
		int iItem = m_listUEX.GetNextSelectedItem( pos );
			
		// Get the ELI code from this item.
		string strTopELI = getItemELICodes(iItem, false);

		// Put the ELI code on the clipboard
		ClipboardManager clipboardMgr( this );
		clipboardMgr.writeText(strTopELI);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28682");
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnToolsExportDebugData()
{
	try
	{
		if (ma_pExportDebugDataDlg.get() != NULL && asCppBool(::IsWindow(ma_pExportDebugDataDlg->m_hWnd)))
		{
			// Show the find dialog
			ma_pExportDebugDataDlg->ShowWindow(SW_SHOW);
		}
		else
		{
			throw UCLIDException("ELI28735", "Export debug data dialog has not been initialized!");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28732");
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnSelectMatchingTopLevelExceptions()
{
	try
	{
		selectMatchingExceptions(false);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI31249");
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::OnSelectMatchingExceptionHierarchies()
{
	try
	{
		selectMatchingExceptions(true);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI31272");
}


//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::initPersistent()
{
	///////////////////////////
	// Dialog size and position
	///////////////////////////
	string	strWidth;
	string	strDir;
	bool	bMove = true;

	// Retrieve persistent size and position of dialog
	long	lLeft = 0;
	long	lTop = 0;
	long	lWidth = 0;
	long	lHeight = 0;

	// If X position key doesn't exist, retain default from resource file
	if (m_pCfgMgr->keyExists( GENERAL, WINDOW_POS_X ))
	{
		// Retrieve value
		strWidth = m_pCfgMgr->getKeyValue( GENERAL, WINDOW_POS_X );
		
		// Convert to integer
		lLeft = atoi( strWidth.c_str() );
	}
	else
	{
		bMove = false;
	}

	// If Y position key doesn't exist, retain default from resource file
	if (m_pCfgMgr->keyExists( GENERAL, WINDOW_POS_Y ))
	{
		// Retrieve value
		strWidth = m_pCfgMgr->getKeyValue( GENERAL, WINDOW_POS_Y );
		
		// Convert to integer
		lTop = atoi( strWidth.c_str() );
	}
	else
	{
		bMove = false;
	}

	// If width key doesn't exist, retain default from resource file
	if (m_pCfgMgr->keyExists( GENERAL, WINDOW_SIZE_X ))
	{
		// Retrieve value
		strWidth = m_pCfgMgr->getKeyValue( GENERAL, WINDOW_SIZE_X );
		
		// Convert to integer
		lWidth = atoi( strWidth.c_str() );
	}
	else
	{
		bMove = false;
	}

	// If height key doesn't exist, retain default from resource file
	if (m_pCfgMgr->keyExists( GENERAL, WINDOW_SIZE_Y ))
	{
		// Retrieve value
		strWidth = m_pCfgMgr->getKeyValue( GENERAL, WINDOW_SIZE_Y );
		
		// Convert to integer
		lHeight = atoi( strWidth.c_str() );
	}
	else
	{
		bMove = false;
	}

	// Minimum width to allow display of buttons
	if (lWidth < MIN_WINDOW_X)
	{
		lWidth = MIN_WINDOW_X;
	}

	// Minimum height to allow display of tree, list, and buttons
	if (lHeight < MIN_WINDOW_Y)
	{
		lHeight = MIN_WINDOW_Y;
	}

	// Make sure that values were retrieved
	if (bMove)
	{
		// Adjust window position based on retrieved settings
		MoveWindow( lLeft, lTop, lWidth, lHeight, TRUE );
	}
	else
	{
		////////////////////////////////////
		// No settings, default to maximized
		////////////////////////////////////
		// Get screen resolution
		RECT rectScreen;
		SystemParametersInfo( SPI_GETWORKAREA, 0, &rectScreen, 0 );
		int iWidth = rectScreen.right - rectScreen.left + 1;
		int iHeight = rectScreen.bottom - rectScreen.top + 1;

		// Reposition dialog to left third of screen with minimum border
		::SetWindowPos( m_hWnd, NULL, 0, 1, iWidth, iHeight, SWP_NOZORDER );
	}

	/////////////////////////
	// Retrieve UEX directory
	/////////////////////////

	// If directory key doesn't exist, just use "C:\"
	if (m_pCfgMgr->keyExists( GENERAL, DIRECTORY ))
	{
		// Retrieve value
		strDir = m_pCfgMgr->getKeyValue( GENERAL, DIRECTORY );
		
		// Store in CString
		m_zDirectory = strDir.c_str();

		// Make sure that last element is backslash
		if (m_zDirectory.Right( 1 ) != "\\")
		{
			m_zDirectory += "\\";
		}
	}
	else
	{
		// Default to C drive
		m_zDirectory = "C:\\";
	}

	// Retrieve column widths
	m_iTimeColumnWidth = getColumnWidth( TIME_WIDTH );
	m_iELIColumnWidth = getColumnWidth( ELI_WIDTH );
	m_iExceptionColumnWidth = getColumnWidth( EXCEPTION_WIDTH );
	m_iSerialColumnWidth = getColumnWidth( SERIAL_WIDTH );
	m_iApplicationColumnWidth = getColumnWidth( APPLICATION_WIDTH );
	m_iComputerColumnWidth = getColumnWidth( COMPUTER_WIDTH );
	m_iUserColumnWidth = getColumnWidth( USER_WIDTH );
	m_iPidColumnWidth = getColumnWidth( PID_WIDTH );
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::prepareList()
{
	// Enable full row selection and grid lines for UEX list
	m_listUEX.SetExtendedStyle( LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT );

	//////////////////////////
	// Add columns to UEX list
	//////////////////////////
	// Empty column is first
	m_listUEX.InsertColumn( EMPTY_LIST_COLUMN, "Empty", 
		LVCFMT_LEFT, 0, EMPTY_LIST_COLUMN );

	m_listUEX.InsertColumn( TIME_LIST_COLUMN, "Time", 
		LVCFMT_LEFT, m_iTimeColumnWidth, TIME_LIST_COLUMN );

	m_listUEX.InsertColumn( TOP_ELI_COLUMN, "Top ELI Code", 
		LVCFMT_LEFT, m_iELIColumnWidth, TOP_ELI_COLUMN );

	m_listUEX.InsertColumn( TOP_EXCEPTION_COLUMN, "Top Exception", 
		LVCFMT_LEFT, m_iExceptionColumnWidth, TOP_EXCEPTION_COLUMN );

	m_listUEX.InsertColumn( SERIAL_LIST_COLUMN, "Serial Number", 
		LVCFMT_LEFT, m_iSerialColumnWidth, SERIAL_LIST_COLUMN );

	m_listUEX.InsertColumn( APPLICATION_LIST_COLUMN, "Application", 
		LVCFMT_LEFT, m_iApplicationColumnWidth, APPLICATION_LIST_COLUMN );

	m_listUEX.InsertColumn( COMPUTER_LIST_COLUMN, "Computer", 
		LVCFMT_LEFT, m_iComputerColumnWidth, COMPUTER_LIST_COLUMN );

	m_listUEX.InsertColumn( USER_LIST_COLUMN, "User", 
		LVCFMT_LEFT, m_iUserColumnWidth, USER_LIST_COLUMN );

	m_listUEX.InsertColumn( PID_LIST_COLUMN, "Process ID", 
		LVCFMT_LEFT, m_iPidColumnWidth, PID_LIST_COLUMN );
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::updateEnabledStateForControls()
{
	// get the current menu object
	CMenu* pMenu = GetMenu();
	ASSERT_RESOURCE_ALLOCATION("ELI14853", pMenu != NULL);

	// Enable/disable Clear List button
	// Enable/disable save as button
	// Enable/disable Invert Selection button
	// Enable/disable Find button
	bool bNonZeroCount = (m_listUEX.GetItemCount() != 0);
	pMenu->EnableMenuItem(ID_EDIT_CLEAR, MF_BYCOMMAND | (bNonZeroCount ? MF_ENABLED: MF_GRAYED) );
	pMenu->EnableMenuItem(ID_FILE_SAVE_AS, MF_BYCOMMAND | (bNonZeroCount ? MF_ENABLED: MF_GRAYED) );
	pMenu->EnableMenuItem(ID_EDIT_INVERT_SELECTION, MF_BYCOMMAND | (bNonZeroCount ? MF_ENABLED: MF_GRAYED) );
	
	// Enable/disable File refresh
	pMenu->EnableMenuItem(ID_FILE_REFRESHCURRENTLOGFILE, MF_BYCOMMAND | (
		(m_strCurrentFile != "") ? MF_ENABLED: MF_GRAYED) );

	// Enable/disable File start new log file depending on whether current file is default log file
	pMenu->EnableMenuItem(ID_FILE_START_NEW_LOG_FILE, MF_BYCOMMAND | (
		stringCSIS::sEqual(m_strCurrentFile,UCLIDException::getDefaultLogFileFullPath())
		&& bNonZeroCount ? MF_ENABLED : MF_GRAYED));

	// Enable/disable View button
	bool bExactlyOneRowSelected = (m_listUEX.GetSelectedCount() == 1);
	pMenu->EnableMenuItem(ID_EXCEPTION_VIEW_DETAILS, MF_BYCOMMAND | (bExactlyOneRowSelected ? MF_ENABLED: MF_GRAYED) );

	// Enable/disable Delete button
	bool bSelectionIsNonZero = (m_listUEX.GetSelectedCount() != 0);
	pMenu->EnableMenuItem(ID_EDIT_DELETE_SELECTION, MF_BYCOMMAND | (bSelectionIsNonZero ? MF_ENABLED: MF_GRAYED) );

	// determine what the enabled/disabled state of the previous/next
	// log file navagiation commands should be
	int iSelectedIndex = m_comboExceptionsList.GetCurSel();
	bool bEnablePrevLogFileNavigation = (iSelectedIndex != -1 && iSelectedIndex > 0);
	bool bEnableNextLogFileNavigation = (iSelectedIndex != -1 && iSelectedIndex < m_comboExceptionsList.GetCount() - 1);

	// enable/disable the combo box
	m_comboExceptionsList.EnableWindow(iSelectedIndex != -1 ? TRUE : FALSE);

	// enable/disable the navigation menu items
	pMenu->EnableMenuItem(ID_FILE_OPEN_PREV_LOG_FILE, MF_BYCOMMAND | (bEnablePrevLogFileNavigation ? MF_ENABLED: MF_GRAYED) );
	pMenu->EnableMenuItem(ID_FILE_OPEN_NEXT_LOG_FILE, MF_BYCOMMAND | (bEnableNextLogFileNavigation ? MF_ENABLED: MF_GRAYED) );
	
	// enable/disable the navigation buttons
	CWnd *pPrevButton = GetDlgItem(ID_BTN_PREV_LOG_FILE);
	ASSERT_RESOURCE_ALLOCATION("ELI14854", pPrevButton != NULL);
	pPrevButton->EnableWindow(bEnablePrevLogFileNavigation ? TRUE : FALSE);
	CWnd *pNextButton = GetDlgItem(ID_BTN_NEXT_LOG_FILE);
	ASSERT_RESOURCE_ALLOCATION("ELI14855", pNextButton != NULL);
	pNextButton->EnableWindow(bEnableNextLogFileNavigation ? TRUE : FALSE);
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::addExceptions(string strUEXFile, bool bReplaceMode)
{
	// adding exceptions are done in either append mode or replace mode
	// depending upon the value of bReplaceMode

	// if we are in replace mode, clear the listing first
	if (bReplaceMode)
	{
		OnEditClear();
	}

	// Suspend window updates
	SuspendWindowUpdates suspend(*this);

	CStdioFile	file;
	CString		zLine;
	string	strLine;
	unsigned long ulLineNum = 0;

	// Show the wait cursor
	CWaitCursor wait;

	// Create a temporary file and attempt to copy the uex file to it
	// By operating on the copy we should be able to read the file without
	// causing corruption to the underlying exception file if processing is
	// taking place. [LRCAU #3986]
	TemporaryFileName tempFile;
	string strFileToOpen = strUEXFile;
	try
	{
		// Mutex around log file access
		CSingleLock lg(m_apLogFileMutex.get(), TRUE);

		copyFile(strUEXFile, tempFile.getName());
		strFileToOpen = tempFile.getName();
	}
	catch(...)
	{
		// If copy was unsuccessful, just try parsing the actual file
	}

	// Open the file and read each line from it
	vector<string> vecLines;
	if (file.Open( strFileToOpen.c_str(), CFile::modeRead ))
	{
		// Read each line of text
		while (file.ReadString( zLine ) == TRUE)
		{
			vecLines.push_back((LPCTSTR)zLine);
		}

		file.Close();
	}

	// Parse each line from the UEX file
	for (size_t ulLineNum = 0; ulLineNum < vecLines.size(); ulLineNum++)
	{
		const string& strLine = vecLines[ulLineNum];

		if (!parseLine( strLine ))
		{
			string strMessage = "Unable to parse current line of the UEX file. "
				"Would you like to continue parsing the file?\n"
				"File name: " + strUEXFile + "\nLine Number: " + asString(ulLineNum+1)
				+ "\nLine Text: " + (strLine.empty() ? "<Blank Line>" : strLine);

			// Prompt the user about the unparseable line
			if (AfxMessageBox(strMessage.c_str(), MB_YESNO | MB_ICONWARNING) == IDNO)
			{
				// User does not want to continue, break from the loop
				break;
			}
		}
	}

	// if we are in replace mode, set the UEX file as the new current file
	if (bReplaceMode)
	{
		setNewCurrentFile(strUEXFile);
	}
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::setNewCurrentFile(string strNewCurrentFile)
{
	// update current file member variable
	m_strCurrentFile = strNewCurrentFile;

	// Update the window caption
	static const string strCAPTION = "UEX Viewer";
	CString zTitle;
	if (m_strCurrentFile.empty())
	{
		zTitle = strCAPTION.c_str();
	}
	else
	{
		zTitle.Format( "%s - %s", strCAPTION.c_str(), 
			getFileNameFromFullPath( m_strCurrentFile.c_str() ).c_str() );
	}
	SetWindowText((LPCTSTR) zTitle);

	// clear the list of exceptions in the current folder
	m_comboExceptionsList.ResetContent();

	// find the filename, extension and folder associated with the current file
	if (!m_strCurrentFile.empty())
	{
		string strCurrentFolder = getDirectoryFromFullPath(m_strCurrentFile);
		string strCurrentFile = getFileNameFromFullPath(m_strCurrentFile);
		string strExtension = getExtensionFromFullPath(m_strCurrentFile);

		// get the UEX files in that folder
		vector<string> vecFiles;
		getFilesInDir(vecFiles, strCurrentFolder, "*.uex");

		// iterate through the files and populate the combo box with
		// just the filenames (without the path)
		vector<string>::const_iterator iter;
		for (iter = vecFiles.begin(); iter != vecFiles.end(); iter++)
		{
			string strFile = getFileNameFromFullPath(*iter);
			m_comboExceptionsList.AddString(strFile.c_str());
		}

		// if the extension of the current file is not .uex, then we need
		// to manually add it to the combo box as the above file search
		// would not have found the current file
		makeLowerCase(strExtension);
		if (strExtension != ".uex")
		{
			m_comboExceptionsList.AddString(strCurrentFile.c_str());
		}

		// select the current file as the current selection in the combo box
		m_comboExceptionsList.SelectString(0, strCurrentFile.c_str());
	}

	// update the state of menu items and controls
	updateEnabledStateForControls();
}
//-------------------------------------------------------------------------------------------------
bool CUEXViewerDlg::parseLine(const string& strText)
{
	int iIndex = -1;
	try
	{
		// Parse the string
		vector<string> vecTokens;
		StringTokenizer	s;
		s.parse( strText, vecTokens );

		// Step through tokens and add item to list
		unsigned long ulCount = vecTokens.size();
		string	strToken;

		// Dummy text for 0-width column
		iIndex = m_listUEX.InsertItem( 0, "a" );
		long lTime = 0;

		// Only parse lines with expected number of tokens
		if (ulCount == OLD_UEX_TOKEN_COUNT)
		{
			// Set the serial column
			setItemText(iIndex, SERIAL_LIST_COLUMN, vecTokens[SERIAL_VALUE]);

			// Set the application list column
			setItemText(iIndex, APPLICATION_LIST_COLUMN, vecTokens[APPLICATION_VALUE]);

			// Set the computer column
			setItemText(iIndex, COMPUTER_LIST_COLUMN, vecTokens[COMPUTER_VALUE]);

			// Set the user column
			setItemText(iIndex, USER_LIST_COLUMN, vecTokens[USER_VALUE]);

			// Set the PID column
			setItemText(iIndex, PID_LIST_COLUMN, vecTokens[PID_VALUE]);

			// Get the time value and set the time column
			try
			{
				lTime = asLong(vecTokens[TIME_VALUE]);
			}
			catch(...)
			{
			}
			CTime	time( lTime );
			CString	zTime = lTime > 0 ? time.Format("%m/%d/%Y %H:%M:%S") : "N/A";
			setItemText( iIndex, TIME_LIST_COLUMN, (LPCTSTR) zTime );

			// Retrieve exception string for storage in ItemData
			strToken = vecTokens[EXCEPTION_VALUE];

			// Create local UCLIDException object
			UCLIDException ue;
			try
			{
				// make sure there is a valid string
				ue.createFromString( "ELI12736", strToken, false );

				// Check that the top text is not just the token passed in
				// if it is, then this line could not be parsed
				if (ue.getTopText() == strToken || ue.getTopELI().empty())
				{
					UCLIDException uex("ELI29918", "*Unparsable exception*");
					uex.addDebugInfo("Exception String",
						strToken.empty() ? "<Empty String>" : strToken);
					strToken = uex.asStringizedByteStream();
					ue = uex;
				}
			}
			catch (...)
			{
				UCLIDException uex("ELI29919", "*Unparsable exception*");
				uex.addDebugInfo("Exception String",
					strToken.empty() ? "<Empty String>" : strToken);
				strToken = uex.asStringizedByteStream();
				ue = uex;
			}

			// Display Top ELI code and Top Exception
			setItemText( iIndex, TOP_ELI_COLUMN, ue.getTopELI() );
			setItemText( iIndex, TOP_EXCEPTION_COLUMN, ue.getTopText() );
		}
		else
		{
			// Invalid number of tokens, set the exception text to invalid token count
			UCLIDException ue("ELI29920", "*Exception line had invalid number of tokens.*");
			ue.addDebugInfo("Exception Line", strText);
			strToken = ue.asStringizedByteStream();

			// Display Top ELI code and Top Exception
			setItemText( iIndex, TOP_ELI_COLUMN, ue.getTopELI() );
			setItemText( iIndex, TOP_EXCEPTION_COLUMN, ue.getTopText() );

		}

		// Set all text items, now set the item data
		ITEMINFO*	pData = new ITEMINFO;
		ASSERT_RESOURCE_ALLOCATION("ELI29921", pData != NULL);

		pData->iIndex = iIndex;
		pData->ulTime = lTime;
		pData->strData = strToken;
		m_listUEX.SetItemData( iIndex, (DWORD)pData );

		return true;
	}
	catch(...)
	{
		// Exception occurred, remove the bad item from the list and return false
		if (iIndex != -1)
		{
			m_listUEX.DeleteItem(iIndex);
		}
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::refreshIndices()
{
	ITEMINFO*	pData = NULL;
	int iCount = m_listUEX.GetItemCount();
	for (int i = 0; i < iCount; i++)
	{
		// Retrieve ItemData
		pData = (ITEMINFO *)m_listUEX.GetItemData( i );
		ASSERT_RESOURCE_ALLOCATION("ELI28711", pData != NULL);

		// Replace index item
		pData->iIndex = i;
	}
}
//-------------------------------------------------------------------------------------------------
long CUEXViewerDlg::getColumnWidth(string strColumn)
{
	long lWidth = 80;

	if (m_pCfgMgr->keyExists( COLUMNS, strColumn.c_str() ))
	{
		// Retrieve value
		string strWidth = m_pCfgMgr->getKeyValue( COLUMNS, strColumn.c_str() );
		
		// Convert to integer
		int iActual = asLong( strWidth );

		// Sanity check - just retain default if out of bounds
		if ((iActual >= MIN_WIDTH) && (iActual <= MAX_WIDTH))
		{
			lWidth = iActual;
		}
	}

	return lWidth;
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::setItemText(int iIndex, int iColumn, string strText)
{
	// Write the text to the specified row and column
	m_listUEX.SetItemText( iIndex, iColumn, strText.c_str() );
}
//-------------------------------------------------------------------------------------------------
string CUEXViewerDlg::getItemELICodes(int iIndex, bool bIncludeInnerELICodes)
{
	// Retrieve this data structure
	ITEMINFO* pData = (ITEMINFO *)m_listUEX.GetItemData(iIndex);
	ASSERT_RESOURCE_ALLOCATION("ELI28709", pData != NULL);

	// This ELI Code will only be used it the pData->strData value is not a stringized exception
	string strELICode = "ELI28695";

	// Create a UCLIDException object
	UCLIDException	ue;
	// Create the exception from the items data which should always be stringized exception
	// if it is the ELI code in strELICode will not be used if it is not a 
	// stringized exception the strELICode will be the top ELI code of the exception
	ue.createFromString(strELICode, pData->strData);

	// Check if there the top ELICode is the same as the one passed to the creatFromString
	// method which should never happen
	string strELICodes = ue.getTopELI();
	if (strELICodes == strELICode)
	{
		// The exception should always be valid so throw a logic exception if this happens
		THROW_LOGIC_ERROR_EXCEPTION("ELI28696");
	}

	if (bIncludeInnerELICodes)
	{
		for (UCLIDException *pueInner = (UCLIDException*)ue.getInnerException();
			 pueInner != NULL;
			 pueInner = (UCLIDException*)pueInner->getInnerException())
		{
			strELICodes += "," + pueInner->getTopELI();
		}
	}

	return strELICodes;
}
//-------------------------------------------------------------------------------------------------
void CUEXViewerDlg::selectMatchingExceptions(bool bMatchEntireHierarchy)
{
	POSITION pos = m_listUEX.GetFirstSelectedItemPosition();

	// Don't do anything unless one and only one item is selected.
	if (pos == NULL || m_listUEX.GetSelectedCount() > 1)
	{
		return;
	}

	CWaitCursor wait;

	// Get index of selected item
	int iItem = m_listUEX.GetNextSelectedItem(pos);

	// Get the selected ELI code(s)
	string strSelectedELI = getItemELICodes(iItem, bMatchEntireHierarchy);

	int nCount = m_listUEX.GetItemCount();
	for (int i = 0; i < nCount; i++)
	{
		// Get the ELI code(s) from this item.
		string strELI = getItemELICodes(i, bMatchEntireHierarchy);

		if (strELI == strSelectedELI)
		{
			// Select if the ELI codes match
			m_listUEX.SetItemState(i, LVIS_SELECTED, LVIS_SELECTED);
		}
		else
		{
			// Otherwise, ensure the item is not selected.
			m_listUEX.SetItemState(i, 0, LVIS_SELECTED);
		}
	}
}