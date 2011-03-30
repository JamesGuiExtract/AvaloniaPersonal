// ManageUsersDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ManageUsersDlg.h"

#include <SuspendWindowUpdates.h>
#include <UCLIDException.h>
#include <COMUtils.h>
#include <PromptDlg.h>
#include <StringCSIS.h>

#include <vector>
#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const int giUSER_COLUMN = 0;
static const int giPASSWORD_SET_COLUMN = 1;

static const int giUSER_COLUMN_WIDTH = 200;

//-------------------------------------------------------------------------------------------------
// CManageUsersDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CManageUsersDlg, CDialog)
//-------------------------------------------------------------------------------------------------
CManageUsersDlg::CManageUsersDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr &ipFAMDB,
							   CWnd* pParent) :
CDialog(CManageUsersDlg::IDD, pParent),
m_ipFAMDB(ipFAMDB)
{
	ASSERT_ARGUMENT("ELI29015", ipFAMDB != __nullptr);
}
//-------------------------------------------------------------------------------------------------
CManageUsersDlg::~CManageUsersDlg()
{
	try
	{
		// Ensure FamDB pointer is released
		m_ipFAMDB = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29016");
}
//-------------------------------------------------------------------------------------------------
void CManageUsersDlg::DoDataExchange(CDataExchange *pDX)
{
	try
	{
		CDialog::DoDataExchange(pDX);
		DDX_Control(pDX, IDC_BTN_ADD_USER, m_btnAdd);
		DDX_Control(pDX, IDC_BTN_DELETE_USER, m_btnRemove);
		DDX_Control(pDX, IDC_BTN_RENAME_USER, m_btnRename);
		DDX_Control(pDX, IDC_BTN_REFRESH_USERS, m_btnRefresh);
		DDX_Control(pDX, IDC_BTN_CLEAR_USER_PASSWORD, m_btnClearPassword);
		DDX_Control(pDX, IDC_LIST_USERS, m_listUsers);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29062");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CManageUsersDlg, CDialog)
	ON_BN_CLICKED(IDC_BTN_ADD_USER, &CManageUsersDlg::OnBtnAdd)
	ON_BN_CLICKED(IDC_BTN_DELETE_USER, &CManageUsersDlg::OnBtnRemove)
	ON_BN_CLICKED(IDC_BTN_RENAME_USER, &CManageUsersDlg::OnBtnRename)
	ON_BN_CLICKED(IDC_BTN_REFRESH_USERS, &CManageUsersDlg::OnBtnRefresh)
	ON_BN_CLICKED(IDC_BTN_CLEAR_USER_PASSWORD, &CManageUsersDlg::OnBtnClearPassword)
	ON_BN_CLICKED(IDC_BTN_USER_CLOSE, &CManageUsersDlg::OnBtnClose)
	ON_NOTIFY(NM_DBLCLK, IDC_LIST_USERS, &CManageUsersDlg::OnNMDblclkList)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_USERS, &CManageUsersDlg::OnLvnItemchangedList)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
BOOL CManageUsersDlg::PreTranslateMessage(MSG *pMsg)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (pMsg->message == WM_KEYDOWN)
		{
			// translate accelerators
			static HACCEL hAccel = LoadAccelerators(AfxGetApp()->m_hInstance, 
				MAKEINTRESOURCE(IDR_MANAGE_LOGIN_USERS_ACCELERATORS));
			if (TranslateAccelerator(m_hWnd, hAccel, pMsg))
			{
				// since the message has been handled, no further dispatch is needed
				return TRUE;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29017")

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
BOOL CManageUsersDlg::OnInitDialog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Configure the user column
		configureUserList();

		// Populate the list
		refreshUserList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29018");

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CManageUsersDlg::OnBtnAdd()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Get the current user name
		CString zUserName = getCurrentUserName().c_str();

		// Prompt for new user
		PromptDlg dlgAddUser("Add User", "Username", zUserName);
		if (dlgAddUser.DoModal() == IDOK)
		{
			string strUserToAdd = (LPCSTR)dlgAddUser.m_zInput;

			// Check to make user is not admin or administrator
			// Need to make sure the new name is not admin or administrator
			if (stringCSIS::sEqual(strUserToAdd, "admin") || 
				stringCSIS::sEqual(strUserToAdd, "administrator"))
			{
				UCLIDException ue("ELI29115", "Cannot add user admin or administrator.");
				ue.addDebugInfo("NewUserName", strUserToAdd);
				throw ue;
			}

			// Add the user to the database
			m_ipFAMDB->AddLoginUser(strUserToAdd.c_str());
			refreshUserList();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29021");
}
//-------------------------------------------------------------------------------------------------
void CManageUsersDlg::OnBtnRename()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// If there are no selected items or the user said no to delete then just return
		if (m_listUsers.GetSelectedCount() != 1)
		{
			// Nothing to do
			return;
		}

		POSITION pos = m_listUsers.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			int iIndex = m_listUsers.GetNextSelectedItem( pos );

			// Get hte user to rename
			string strUserToRename = m_listUsers.GetItemText(iIndex, giUSER_COLUMN);

			// Set up the caption for the prompt dialog using the user
			string strCaption = "Rename " + strUserToRename + " to";
			PromptDlg dlgRename("Rename login user", strCaption.c_str(), strUserToRename.c_str());

			// Display dialog and if OK was clicked process rename
			if (dlgRename.DoModal() == IDOK)
			{
				// Get the name to rename to
				string strNewUserName = (LPCSTR)dlgRename.m_zInput;

				// Need to make sure the new name is not admin or administrator
				if (stringCSIS::sEqual(strNewUserName, "admin") || 
					stringCSIS::sEqual(strNewUserName, "administrator"))
				{
					UCLIDException ue("ELI29071", "Cannot rename user to admin or administrator.");
					ue.addDebugInfo("NewUserName", strNewUserName);
					throw ue;
				}

				// Rename the user
				m_ipFAMDB->RenameLoginUser(strUserToRename.c_str(), strNewUserName.c_str());

				// Update the user list
				refreshUserList();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29024");
}
//-------------------------------------------------------------------------------------------------
void CManageUsersDlg::OnBtnRemove()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// If there are no selected items or the user said no to delete then just return
		if (m_listUsers.GetSelectedCount() == 0
			|| MessageBox("Delete the selected User(s)?", "Delete User?",
			MB_YESNO | MB_ICONQUESTION) == IDNO)
		{
			// Nothing to do
			return;
		}

		// Build the list of tags to delete
		vector<string> vecUsersToDelete;
		POSITION pos = m_listUsers.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			int iIndex = m_listUsers.GetNextSelectedItem( pos );

			// Loop through selected items and add each item to the vector
			while (iIndex != -1)
			{
				vecUsersToDelete.push_back((LPCTSTR) m_listUsers.GetItemText(iIndex, giUSER_COLUMN));

				iIndex = m_listUsers.GetNextSelectedItem(pos);
			}
		}

		// Log an application trace about the database change
		UCLIDException uex("ELI29068", "Application trace: Database change");
		uex.addDebugInfo("Change", "Login user(s) deleted");
		uex.addDebugInfo("User Name", getCurrentUserName());
		uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
		uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));
		string strUsers = "";
		for (vector<string>::iterator it = vecUsersToDelete.begin();
			it != vecUsersToDelete.end(); it++)
		{
			m_ipFAMDB->RemoveLoginUser(it->c_str());

			if (!strUsers.empty())
			{
				strUsers += ", ";
			}
			strUsers += (*it);
		}
		uex.addDebugInfo("Login user(s)", strUsers);
		uex.log();

		refreshUserList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29026");
}
//-------------------------------------------------------------------------------------------------
void CManageUsersDlg::OnBtnRefresh()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		refreshUserList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29027");
}
//-------------------------------------------------------------------------------------------------
void CManageUsersDlg::OnBtnClearPassword()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// If there are no selected items or the user said no to delete then just return
		if (m_listUsers.GetSelectedCount() != 1)
		{
			// Nothing to do
			return;
		}
		
		// refresh the user list so that the setting in the password list is up to date
		refreshUserList();
		
		POSITION pos = m_listUsers.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			int iIndex = m_listUsers.GetNextSelectedItem( pos );

			string strUser = m_listUsers.GetItemText(iIndex, giUSER_COLUMN);
			string strPasswordSet = m_listUsers.GetItemText(iIndex, giPASSWORD_SET_COLUMN);
			string strMessage = "Are you sure you want to clear password for " + strUser + "?";
			if (strPasswordSet != "No" && MessageBox(strMessage.c_str(), "Clear user password?", MB_YESNO | MB_ICONQUESTION) 
				== IDYES)
			{
				m_ipFAMDB->ClearLoginUserPassword(strUser.c_str());
				refreshUserList();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29037");
}
//-------------------------------------------------------------------------------------------------
void CManageUsersDlg::OnBtnClose()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Call dialog OnOK
		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29028");
}
//-------------------------------------------------------------------------------------------------
void CManageUsersDlg::OnNMDblclkList(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Same as clicking modify
		OnBtnRename();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29029");
}
//-------------------------------------------------------------------------------------------------
void CManageUsersDlg::OnLvnItemchangedList(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update controls when selection changes
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29030");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CManageUsersDlg::updateControls()
{
	try
	{
		// Get the selection count
		unsigned int iCount = m_listUsers.GetSelectedCount();

		// Enable delete if at least 1 item selected
		// Enable modify iff 1 item selected
		BOOL bEnableRemove = asMFCBool(iCount > 0);
		BOOL bEnableModify = asMFCBool(iCount == 1);

		m_btnRemove.EnableWindow(bEnableRemove);
		m_btnRename.EnableWindow(bEnableModify);
		m_btnClearPassword.EnableWindow(bEnableModify);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29031");
}
//-------------------------------------------------------------------------------------------------
void CManageUsersDlg::configureUserList()
{
	try
	{
		// Set list style
		m_listUsers.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);

		// Build column information struct
		LVCOLUMN lvColumn;
		lvColumn.mask = LVCF_FMT | LVCF_TEXT | LVCF_WIDTH;
		lvColumn.fmt = LVCFMT_LEFT;

		// Set information for User name column
		lvColumn.pszText = "Username";
		lvColumn.cx = giUSER_COLUMN_WIDTH;

		// Add the username column
		m_listUsers.InsertColumn(giUSER_COLUMN, &lvColumn);

		// Get dimensions of list control
		CRect recList;
		m_listUsers.GetClientRect(&recList);

		// Set heading and compute width for password set column
		lvColumn.pszText = "Password Set?";
		lvColumn.cx = recList.Width() - giUSER_COLUMN_WIDTH;

		// Add the description column
		m_listUsers.InsertColumn(giPASSWORD_SET_COLUMN, &lvColumn);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29032");
}
//-------------------------------------------------------------------------------------------------
void CManageUsersDlg::refreshUserList()
{
	try
	{
		SuspendWindowUpdates updater(*this);

		// Store the position of the first selected item
		int nSelectedItem = 0;
		POSITION pos = m_listUsers.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			nSelectedItem = m_listUsers.GetNextSelectedItem(pos);
		}
		
		clearListSelection();

		// Clear the current User list
		m_listUsers.DeleteAllItems();

		IStrToStrMapPtr ipUsers = m_ipFAMDB->GetLoginUsers();
		ASSERT_RESOURCE_ALLOCATION("ELI29033", ipUsers != __nullptr);

		// Get the number of Users
		long lSize = ipUsers->Size;

		// Need to ensure the password set column is the correct width
		CRect recList;
		m_listUsers.GetClientRect(&recList);
		int nPasswordSetWidth = recList.Width() - giUSER_COLUMN_WIDTH;

		// Check if need to add space for scroll bar
		if (lSize > m_listUsers.GetCountPerPage())
		{
			// Get the scroll bar width, if 0 and error occurred, just log an
			// application trace and set the width to 17
			int nVScrollWidth = GetSystemMetrics(SM_CXVSCROLL);
			if (nVScrollWidth == 0)
			{
				UCLIDException ue("ELI29034", "Application Trace: Unable to determine scroll bar width.");
				ue.log();

				nVScrollWidth = 17;
			}

			// Deduct space for the scroll bar from the width
			nPasswordSetWidth -= nVScrollWidth;
		}

		// Set the width of the description column
		m_listUsers.SetColumnWidth(giPASSWORD_SET_COLUMN, nPasswordSetWidth);

		// Populate the list
		for (long i=0; i < lSize; i++)
		{
			// Get the tag name and description
			_bstr_t bstrUserName;
			_bstr_t bstrPasswordSet;
			ipUsers->GetKeyValue(i, bstrUserName.GetAddress(), bstrPasswordSet.GetAddress());

			// Add the info into the list
			m_listUsers.InsertItem(i, (const char*)bstrUserName);
			m_listUsers.SetItemText(i, giPASSWORD_SET_COLUMN, (const char*)bstrPasswordSet);
		}

		// Select either the last selected item position or select the first item
		m_listUsers.SetItemState(nSelectedItem < lSize ? nSelectedItem : 0,
			LVIS_SELECTED, LVIS_SELECTED);

		updateControls();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29035");
}
//-------------------------------------------------------------------------------------------------
void CManageUsersDlg::clearListSelection()
{
	try
	{
		POSITION pos = m_listUsers.GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			// no item selected, return
			return;
		}

		// iterate through all selections and set them as unselected.  pos will be
		// set to NULL by GetNextSelectedItem if there are no more selected items
		while (pos)
		{
			int nItemSelected = m_listUsers.GetNextSelectedItem(pos);
			m_listUsers.SetItemState(nItemSelected, 0, LVIS_SELECTED);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29036");
}
//-------------------------------------------------------------------------------------------------

