// FileProcessingDlgActionPage.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "FileProcessingDlg.h"
#include "FileProcessingDlgActionPage.h"
#include "HelperFunctions.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <FileDialogEx.h>
#include <ComUtils.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgTaskPage property page
//-------------------------------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(FileProcessingDlgActionPage, CPropertyPage)

//-------------------------------------------------------------------------------------------------
FileProcessingDlgActionPage::FileProcessingDlgActionPage()
: CPropertyPage(FileProcessingDlgActionPage::IDD),
m_bInitialized(false)
{

}
//-------------------------------------------------------------------------------------------------
FileProcessingDlgActionPage::~FileProcessingDlgActionPage()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16524");
}
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlgActionPage::PreTranslateMessage(MSG* pMsg) 
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15267")

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgActionPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(FileProcessingDlgScopePage)
	DDX_Control(pDX, IDC_CHK_QUEUE, m_btnQueue);
	DDX_Control(pDX, IDC_CHK_PROC, m_btnProcess);
	DDX_Control(pDX, IDC_CHK_DISPLAY, m_btnDisplay);
	DDX_Text(pDX, IDC_ACTION, m_zActionName);
	DDX_Control(pDX, IDC_BTN_SEL_ACTION, m_btnSelectAction);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileProcessingDlgActionPage, CPropertyPage)
	ON_WM_SIZE()
	ON_BN_CLICKED(IDC_CHK_QUEUE, &FileProcessingDlgActionPage::OnBnClickedChkQueue)
	ON_BN_CLICKED(IDC_BTN_SEL_ACTION, &FileProcessingDlgActionPage::OnBnClickedBtnSelAction)
	ON_BN_CLICKED(IDC_CHK_PROC, &FileProcessingDlgActionPage::OnBnClickedChkProc)
	ON_BN_CLICKED(IDC_CHK_DISPLAY, &FileProcessingDlgActionPage::OnBnClickedChkDisplay)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgActionPage message handlers
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlgActionPage::OnInitDialog()
{
	CPropertyPage::OnInitDialog();
	
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		m_btnQueue.EnableWindow(FALSE);
		m_btnProcess.EnableWindow(FALSE);
		m_btnDisplay.EnableWindow(FALSE);

		// Set m_bInitialized to true so that 
		// next call to OnSize() will not be skipped
		m_bInitialized = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14041")
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgActionPage::OnSize(UINT nType, int cx, int cy)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CPropertyPage::OnSize(nType, cx, cy);

		// First call to this function shall be ignored
		if (!m_bInitialized) 
		{
			return;
		}

		// Declare CRect variables
		static bool bInit = false;
		static int nLen1, nLen2, nSelectButtonWidth;
		CRect rectDlg, rectSelButton, rectLabelAction;

		// Get original sizes and positions
		GetDlgItem(IDC_BTN_SEL_ACTION)->GetWindowRect(&rectSelButton);
		ScreenToClient(&rectSelButton);
		GetDlgItem(IDC_ACTION)->GetWindowRect(&rectLabelAction);
		ScreenToClient(&rectLabelAction);

		if (!bInit)
		{
			GetClientRect(&rectDlg);

			// Distance from right of the select button to the right of the dialog
			// should be the same as the distance from left of label to left of Dialog
			nLen1 = rectLabelAction.left - rectDlg.left;
			// Distance in between select button and the label box
			nLen2 = rectSelButton.left - rectLabelAction.right;
			// Width of the select button
			nSelectButtonWidth = rectSelButton.Width();

			bInit = true;
		}
		
		// Get dialog rect
		GetClientRect(&rectDlg);

		// Resize the label
		rectLabelAction.right = rectDlg.right - nLen1 - nSelectButtonWidth - nLen2;
		// Move buttons
		rectSelButton.right = rectDlg.right - nLen1;
		rectSelButton.left = rectSelButton.right - nSelectButtonWidth;
		// Move the label and the button to its right position
		GetDlgItem(IDC_ACTION)->MoveWindow(&rectLabelAction);
		GetDlgItem(IDC_BTN_SEL_ACTION)->MoveWindow(&rectSelButton);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13994")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgActionPage::OnBnClickedBtnSelAction()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// display a wait-cursor because we are getting information from the DB, which may take a few seconds
		CWaitCursor wait;

		// Reset the DB Connection
		getDBPointer()->ResetDBConnection();

		// Check if there is no action inside the datebase;
		IStrToStrMapPtr ipMapActions = getDBPointer()->GetActions();
		ASSERT_RESOURCE_ALLOCATION("ELI15021", ipMapActions != NULL );

		// If there is no action inside database Prompt user to define an action first
		// unless AutoCreateAction is TRUE [LRCAU #5650]
		if (ipMapActions->GetSize() == 0 && getDBPointer()->GetAutoCreateActions() == VARIANT_FALSE)
		{
			string strPrompt = "There are no actions inside the current database!\n"
				"Please use the DB Administration tool to add an action first.";
			MessageBox(strPrompt.c_str(), "No action", MB_ICONINFORMATION);
			return;
		}

		// Create IFAMDBUtilsPtr object to bring the Select Action dialog
		UCLID_FILEPROCESSINGLib::IFAMDBUtilsPtr ipFAMDBUtils(CLSID_FAMDBUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI14907", ipFAMDBUtils != NULL );

		// Get the action's name from the dialog
		string strActionName = ipFAMDBUtils->PromptForActionSelection( 
			m_pFPMDB, "Select Action", m_zActionName.GetBuffer(), VARIANT_TRUE);

		// If the selected action is not empty
		if (strActionName != "")
		{
			// Set the Action Name
			getFPM()->ActionName = strActionName.c_str();

			// Update the UI
			refresh();
			updateUI();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14033");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgActionPage::OnBnClickedChkQueue()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// If the Queue check box is checked
		if (m_btnQueue.GetCheck() == BST_CHECKED)
		{
			if (!m_btnDisplay.IsWindowEnabled())
			{
				// Enable Processing file check box
				m_btnDisplay.EnableWindow(TRUE);
			}
		}

		// Call this function to add or remove property pages
		addRemovePages();

		// Enable the file supplying management role if Queue button is checked
		getSupplyingActionMgmtRole()->Enabled = asVariantBool(m_btnQueue.GetCheck() == BST_CHECKED);

		// Update the UI, menu and toolbar items
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14034");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgActionPage::OnBnClickedChkProc()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// If the Process check box is checked
		if (m_btnProcess.GetCheck() == BST_CHECKED)
		{
			// Enable processing file check box if need
			if (!m_btnDisplay.IsWindowEnabled())
			{
				m_btnDisplay.EnableWindow(TRUE);
			}
		}

		// Call this function to add or remove property pages
		addRemovePages();

		// get the file processing mgmt role and enable it if the Process button is checked
		getProcessingActionMgmtRole()->Enabled = asVariantBool(m_btnProcess.GetCheck() == BST_CHECKED);

		// Update the UI, menu and toolbar items
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14035");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgActionPage::OnBnClickedChkDisplay()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Call this function to add or remove property pages
		addRemovePages();

		// Set the check box status to FileProcessingManager
		getFPM()->DisplayOfStatisticsEnabled = asVariantBool(m_btnDisplay.GetCheck() == BST_CHECKED);

		// Update the UI, menu and toolbar items
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14036");
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgActionPage::ResetInitialized()
{
	m_bInitialized = false;
}
//-------------------------------------------------------------------------------------------------
std::string FileProcessingDlgActionPage::GetCurrentActionName()
{
	std::string strAction((LPCTSTR)m_zActionName);
	return strAction;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgActionPage::refresh()
{
	try
	{
		// Get the action name from FileProcessingManager
		std::string strActionName = asString(getFPM()->GetActionName());

		// Check for recently converted FPS file
		bool bResetPage = false;
		if (strActionName == gstrCONVERTED_FPS_ACTION_NAME.c_str())
		{
			// If the action name is the special name indicating that the FPS file 
			// was recently converted, present a MessageBox to the user
			CString zText;
			zText.Format( "The FPS file has been recently converted from a previous version.  An Action must be selected." );
			MessageBox( zText, "Status", MB_OK | MB_ICONINFORMATION );

			// Reset page elements
			bResetPage = true;
		}
		// Update UI if the action name is not empty
		else if (strActionName != "")
		{
			// Check for this action in the DB if it doesn't contain a function tag
			if (strActionName.find('$') == string::npos)
			{
				try
				{
					// Use try ... catch block to convert exception to UCLID Exception
					try
					{
						// Get the actionID from the database
						long lActionID = getDBPointer()->GetActionID( strActionName.c_str() );
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI19997");
				}
				catch (UCLIDException& ue)
				{
					// Log the exception just in case it is not related to the action name 
					ue.log();

					// Notify the user that they must select another action
					std::string	strPrompt;
					strPrompt = "The action '" + strActionName + 
						"' does not exist in the database any more, \nplease select another action.";

					MessageBox(strPrompt.c_str(), "Action not found", MB_OK | MB_ICONINFORMATION);

					// Reset page elements
					bResetPage = true;
				}
			}
		}
		// Action name is empty, reset the page
		else
		{
			bResetPage = true;
		}

		// Reset or update the action name and associated edit box
		m_zActionName = bResetPage ? "" : strActionName.c_str();
		GetDlgItem(IDC_ACTION)->SetWindowText( m_zActionName.GetBuffer() );

		// Enable or disable three check boxes
		m_btnQueue.EnableWindow(asMFCBool(!bResetPage));
		m_btnProcess.EnableWindow(asMFCBool(!bResetPage));
		m_btnDisplay.EnableWindow(asMFCBool(!bResetPage ));

		// Update the check boxes, tabs and UI
		updateChecksAndTabs();
		updateUI();

		UpdateData( FALSE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14136");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgActionPage::setFPMgr(UCLID_FILEPROCESSINGLib::IFileProcessingManager* pFPMgr)
{
	m_pFPM = pFPMgr;
	ASSERT_RESOURCE_ALLOCATION("ELI14137", m_pFPM != NULL);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgActionPage::setFPMDB(UCLID_FILEPROCESSINGLib::IFileProcessingDB* pFPMDB)
{
	m_pFPMDB = pFPMDB;
	ASSERT_RESOURCE_ALLOCATION("ELI14138", m_pFPMDB != NULL);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgActionPage::setEnabled(bool bEnabled)
{
	if (!bEnabled)
	{
		// Disable controls on action tab
		m_btnSelectAction.EnableWindow(FALSE);
		m_btnQueue.EnableWindow(FALSE);
		m_btnProcess.EnableWindow(FALSE);

		if (m_btnQueue.GetCheck() == BST_CHECKED || m_btnProcess.GetCheck() == BST_CHECKED)
		{
			// Always enable the statistics check box if one of the other two 
			// tabs is visible
			m_btnDisplay.EnableWindow(TRUE);
		}
		else
		{
			// Disable the statistics check box if statistics tab is the only 
			// tab that is visible while running
			m_btnDisplay.EnableWindow(FALSE);
		}
	}
	else
	{
		// Enable controls on action tab
		m_btnSelectAction.EnableWindow(TRUE);
		m_btnQueue.EnableWindow(asMFCBool(!m_zActionName.IsEmpty()));
		m_btnProcess.EnableWindow(asMFCBool(!m_zActionName.IsEmpty()));
		m_btnDisplay.EnableWindow(asMFCBool(!m_zActionName.IsEmpty()));
	}
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr FileProcessingDlgActionPage::getFPM()
{
	return UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr( m_pFPM );
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgActionPage::addRemovePages()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Initialize set of Pages to display
		set<FileProcessingDlg::EDlgTabPage> setPages;

		// Always display the database Page
		setPages.insert(FileProcessingDlg::kDatabasePage);

		// Also since this is the action page it should be displayed
		setPages.insert(FileProcessingDlg::kActionPage);

		// Get the pointer to the Property sheet object
		ResizablePropertySheet* pFPDPropSheet = (ResizablePropertySheet*)GetParent();
		
		// Get the pointer to the current FileProcessingDlg object
		FileProcessingDlg* pFPDlg = (FileProcessingDlg*)pFPDPropSheet->GetParent();

		// If the display stats button is enabled the queue pages should be displayed if
		// the queue button is checked
		if (!m_zActionName.IsEmpty() && m_btnQueue.GetCheck() == BST_CHECKED)
		{
			// Insert Queue tabs
			setPages.insert(FileProcessingDlg::kQueueSetupPage);
			setPages.insert(FileProcessingDlg::kQueueLogPage);
		}

		// If the display stats button is enabled the Process pages should be displayed if
		// the process button is checked
		if (!m_zActionName.IsEmpty() && m_btnProcess.GetCheck() == BST_CHECKED)
		{
			// Insert Process Tabs
			setPages.insert(FileProcessingDlg::kProcessingSetupPage);
			setPages.insert(FileProcessingDlg::kProcessingLogPage);
		}

		// If the display stats button is enabled the statistics page should be displayed if
		// the display stats button is checked
		if (!m_zActionName.IsEmpty() && m_btnDisplay.GetCheck() == BST_CHECKED)
		{
			// Insert statistics page
			setPages.insert(FileProcessingDlg::kStatisticsPage);
		}
		
		// Display the tabs
		pFPDlg->updateTabs(setPages);
	}// End of try block
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14042");
}
//--------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr FileProcessingDlgActionPage::getDBPointer()
{
	return UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr( m_pFPMDB );
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgActionPage::updateChecksAndTabs()
{
	try
	{
		// Get the Queue check box status from FPM
		bool bStatus = asCppBool(getSupplyingActionMgmtRole()->Enabled);
		if (bStatus)
		{
			m_btnQueue.SetCheck(BST_CHECKED);
		}
		else
		{
			m_btnQueue.SetCheck(BST_UNCHECKED);
		}

		// Get the Processing check box status from FPM
		bStatus = asCppBool(getProcessingActionMgmtRole()->Enabled);
		if (bStatus)
		{
			m_btnProcess.SetCheck(BST_CHECKED);
		}
		else
		{
			m_btnProcess.SetCheck(BST_UNCHECKED);
		}

		// Get the Display check box status from FPM
		bStatus = asCppBool(getFPM()->GetDisplayOfStatisticsEnabled());
		if (bStatus)
		{
			m_btnDisplay.SetCheck(BST_CHECKED);
		}
		else
		{
			m_btnDisplay.SetCheck(BST_UNCHECKED);
		}

		// Call this function to add or remove property pages
		addRemovePages();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14139");
}
//--------------------------------------------------------------------------------------------------
void FileProcessingDlgActionPage::updateUI()
{
	// Get the pointer to the Property sheet object
	ResizablePropertySheet* pFPDPropSheet = (ResizablePropertySheet*)GetParent();
	// Get the pointer to the current FileProcessingDlg object
	FileProcessingDlg* pFPDlg = (FileProcessingDlg*)pFPDPropSheet->GetParent();

	pFPDlg->updateUI();
}
//--------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileSupplyingMgmtRolePtr FileProcessingDlgActionPage::getFSMgmtRole()
{
	// get the file supplying mgmt role
	UCLID_FILEPROCESSINGLib::IFileSupplyingMgmtRolePtr ipFSMgmtRole = getFPM()->FileSupplyingMgmtRole;
	ASSERT_RESOURCE_ALLOCATION("ELI14265", ipFSMgmtRole != NULL);

	return ipFSMgmtRole;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr FileProcessingDlgActionPage::getSupplyingActionMgmtRole()
{
	UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipMgmtRole = getFSMgmtRole();
	ASSERT_RESOURCE_ALLOCATION("ELI14276", ipMgmtRole != NULL);
	return ipMgmtRole;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr FileProcessingDlgActionPage::getFPMgmtRole()
{
	// get the file Processing mgmt role
	UCLID_FILEPROCESSINGLib::IFileProcessingMgmtRolePtr ipFPMgmtRole = getFPM()->FileProcessingMgmtRole;
	ASSERT_RESOURCE_ALLOCATION("ELI14292", ipFPMgmtRole != NULL);

	return ipFPMgmtRole;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr FileProcessingDlgActionPage::getProcessingActionMgmtRole()
{
	UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipMgmtRole = getFPMgmtRole();
	ASSERT_RESOURCE_ALLOCATION("ELI14293", ipMgmtRole != NULL);
	return ipMgmtRole;
}
//-------------------------------------------------------------------------------------------------
