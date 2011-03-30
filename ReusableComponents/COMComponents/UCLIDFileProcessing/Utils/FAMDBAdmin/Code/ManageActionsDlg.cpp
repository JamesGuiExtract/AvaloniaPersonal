// ManageActionsDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ManageActionsDlg.h"
#include "FileProcessingAddActionDlg.h"
#include "RenameActionDlg.h"

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
static const int giACTION_COLUMN = 0;

//-------------------------------------------------------------------------------------------------
// CManageActionsDlgdialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CManageActionsDlg, CDialog)
//-------------------------------------------------------------------------------------------------
CManageActionsDlg::CManageActionsDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr &ipFAMDB,
							   CWnd* pParent) :
CDialog(CManageActionsDlg::IDD, pParent),
m_ipFAMDB(ipFAMDB)
{
	ASSERT_ARGUMENT("ELI29084", ipFAMDB != __nullptr);
}
//-------------------------------------------------------------------------------------------------
CManageActionsDlg::~CManageActionsDlg()
{
	try
	{
		// Ensure FamDB pointer is released
		m_ipFAMDB = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29085");
}
//-------------------------------------------------------------------------------------------------
void CManageActionsDlg::DoDataExchange(CDataExchange *pDX)
{
	try
	{
		CDialog::DoDataExchange(pDX);
		DDX_Control(pDX, IDC_BTN_ADD_ACTION, m_btnAdd);
		DDX_Control(pDX, IDC_BTN_REMOVE_ACTION, m_btnRemove);
		DDX_Control(pDX, IDC_BTN_RENAME_ACTION, m_btnRename);
		DDX_Control(pDX, IDC_BTN_REFRESH_ACTIONS, m_btnRefresh);
		DDX_Control(pDX, IDC_LIST_ACTIONS_TO_MANAGE, m_listActions);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29086");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CManageActionsDlg, CDialog)
	ON_BN_CLICKED(IDC_BTN_ADD_ACTION, &CManageActionsDlg::OnBtnAdd)
	ON_BN_CLICKED(IDC_BTN_REMOVE_ACTION, &CManageActionsDlg::OnBtnRemove)
	ON_BN_CLICKED(IDC_BTN_RENAME_ACTION, &CManageActionsDlg::OnBtnRename)
	ON_BN_CLICKED(IDC_BTN_REFRESH_ACTIONS, &CManageActionsDlg::OnBtnRefresh)
	ON_BN_CLICKED(IDC_BTN_ACTION_CLOSE, &CManageActionsDlg::OnBtnClose)
	ON_NOTIFY(NM_DBLCLK, IDC_LIST_ACTIONS_TO_MANAGE, &CManageActionsDlg::OnNMDblclkList)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_ACTIONS_TO_MANAGE, &CManageActionsDlg::OnLvnItemchangedList)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
BOOL CManageActionsDlg::PreTranslateMessage(MSG *pMsg)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (pMsg->message == WM_KEYDOWN)
		{
			// translate accelerators
			static HACCEL hAccel = LoadAccelerators(AfxGetApp()->m_hInstance, 
				MAKEINTRESOURCE(IDR_MANAGE_ACTIONS_ACCELERATORS));
			if (TranslateAccelerator(m_hWnd, hAccel, pMsg))
			{
				// since the message has been handled, no further dispatch is needed
				return TRUE;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29087")

	return CDialog::PreTranslateMessage(pMsg); 
}
//-------------------------------------------------------------------------------------------------
BOOL CManageActionsDlg::OnInitDialog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Configure the user column
		configureActionList();

		// Populate the list
		refreshActionList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29088");

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CManageActionsDlg::OnBtnAdd()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Create an add action dialog
		FileProcessingAddActionDlg dlgAddAction(m_ipFAMDB);

		if (dlgAddAction.DoModal() == IDOK)
		{
			// Add application trace whenever a database modification is made
			// [LRCAU #5052 - JDS - 12/18/2008]
			UCLIDException uex("ELI23600", "Application trace: Database change");
			uex.addDebugInfo("Change", "Add new action");
			uex.addDebugInfo("User Name", getCurrentUserName());
			uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
			uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));

			// Get the new action name
			std::string strNewAction = std::string ((LPCTSTR) dlgAddAction.GetActionName());
			if (strNewAction != "")
			{
				// Get the default status for the new action
				int iStatus = dlgAddAction.GetDefaultStatus();

				// Add the new action to the database
				DWORD dwNewActionID;

				// Display a wait-cursor because we are adding an action to DB
				CWaitCursor wait;
				
				// Create the new action
				dwNewActionID = m_ipFAMDB->DefineNewAction(_bstr_t(strNewAction.c_str()));
				uex.addDebugInfo("New Action Name", strNewAction);
				uex.addDebugInfo("New Action ID", dwNewActionID);

				// If the default status equal to -1,
				// We should copy the status from one action to another
				if (iStatus == -1)
				{
					// Get the action ID that the status of which will be copied from
					DWORD dwCopyActionID = dlgAddAction.GetCopyStatusActionID();

					// Copy action status from another action to the new action
					m_ipFAMDB->CopyActionStatusFromAction(dwCopyActionID, dwNewActionID);
					uex.addDebugInfo("Action ID Copied From", dwCopyActionID);
				}
				// If the default status is not -1
				else
				{
					// Cast the default status to an EActionStatus type
					UCLID_FILEPROCESSINGLib::EActionStatus eStatus;
					eStatus = (UCLID_FILEPROCESSINGLib::EActionStatus)(iStatus);

					// Call SetStatusForAllFiles() to set the default status for
					// newly added action
					m_ipFAMDB->SetStatusForAllFiles(_bstr_t(strNewAction.c_str()), eStatus);
					uex.addDebugInfo("Default Status", asString(m_ipFAMDB->AsStatusString(eStatus)));
				} // Inner else block

				// Restore the wait cursor because we have finished adding an action to DB
				wait.Restore();

				// Log application trace [LRCAU #5052 - JDS - 12/18/2008]
				uex.log();
			} // Out if block

			// Prompt to the user that the action has been added
			string strPrompt = "'" + strNewAction + "' has been added to the database.";
			MessageBox(strPrompt.c_str(), "Add Action", MB_ICONINFORMATION);
		} // Dialog block
		refreshActionList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14865");
}
//-------------------------------------------------------------------------------------------------
void CManageActionsDlg::OnBtnRename()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );  

	try
	{
		// Check if there is no action action selected
		if ( m_listActions.GetSelectedCount() != 1 )
		{
			return;
		}

		// Get the selected action to rename
		POSITION pos = m_listActions.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Display the wait cursor wheil action is deleted
			CWaitCursor cursor;

			// Get index of first selection
			int iIndex = m_listActions.GetNextSelectedItem( pos );

			string strOldName = m_listActions.GetItemText(iIndex, giACTION_COLUMN);
			DWORD dwID = m_listActions.GetItemData(iIndex);

			// Create an add action dialog
			RenameActionDlg dlgRenameAction(m_ipFAMDB);

			// Set the old action name in the rename dialog
			dlgRenameAction.SetOldActionName(strOldName);

			if (dlgRenameAction.DoModal() == IDOK)
			{
				// Add application trace whenever a database modification is made
				// [LRCAU #5052 - JDS - 12/18/2008]
				UCLIDException uex("ELI23602", "Application trace: Database change");
				uex.addDebugInfo("Change", "Rename action");
				uex.addDebugInfo("User Name", getCurrentUserName());
				uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
				uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));

				// Display wait cursor
				CWaitCursor wait;

				// Init the action ID, old name and new name
				string strNewName = "";

				// Get the new name
				dlgRenameAction.GetNewActionName(strNewName);

				if (strOldName != strNewName)
				{
					// Call RenameAction to rename the action
					m_ipFAMDB->RenameAction(dwID, _bstr_t(strNewName.c_str()));
					uex.addDebugInfo("Old Action Name", strOldName);
					uex.addDebugInfo("New Action Name", strNewName);

					// Log application trace [LRCAU #5052 - JDS - 12/18/2008]
					uex.log();

					// Display the message that the action is renamed
					string strPrompt = "The action '" + strOldName + "' has been renamed to '"
						+ strNewName + "'.";
					MessageBox( strPrompt.c_str(), "Success", MB_OK|MB_ICONINFORMATION );
				}
			}
		}

		// Update the list
		refreshActionList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14867");
}
//-------------------------------------------------------------------------------------------------
void CManageActionsDlg::OnBtnRemove()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Check if there is no action action selected
		if ( m_listActions.GetSelectedCount() <= 0 )
		{
			return;
		}

		// Create IFAMDBUtilsPtr used to bring up select action dialog
		UCLID_FILEPROCESSINGLib::IFAMDBUtilsPtr ipFAMDBUtils(CLSID_FAMDBUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI14914", ipFAMDBUtils != __nullptr );

		// Get the selected action to remove
		POSITION pos = m_listActions.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Display the wait cursor wheil action is deleted
			CWaitCursor cursor;

			// Get index of first selection
			int iIndex = m_listActions.GetNextSelectedItem( pos );


			// Prompt to verify
			if (MessageBox("Remove the selected action(s)?", "Confirmation", MB_YESNOCANCEL) != IDYES)
			{
				return;
			}

			// Second confirmation
			string strPrompt = "Do you really want to remove the selected actions(s)";

			if (MessageBox(strPrompt.c_str(), "Final Confirmation", MB_YESNO) != IDYES)
			{
				return;
			}
			
			string strActionsDeleted = "";

			// Remove the actions
			while (iIndex != -1)
			{
				// Get the action name to remove
				string strActionName = m_listActions.GetItemText(iIndex, giACTION_COLUMN);

				// Catch any exceptions for each action being deleted.
				try
				{
					// Delete the action
					m_ipFAMDB->DeleteAction(strActionName.c_str());

					// Add the action to string for debug
					if (!strActionsDeleted.empty())
					{
						strActionsDeleted += ", ";
					}
					strActionsDeleted += strActionName;
				}
				CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29114");

				iIndex = m_listActions.GetNextSelectedItem(pos);
			}

			// Add application trace whenever a database modification is made
			// [LRCAU #5052 - JDS - 12/18/2008]
			UCLIDException uex("ELI23601", "Application trace: Database change");
			uex.addDebugInfo("Change", "Remove action(s)");
			uex.addDebugInfo("User Name", getCurrentUserName());
			uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
			uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));
			uex.addDebugInfo("Action(s) removed", strActionsDeleted);
			uex.log();
		}
		refreshActionList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14866");
}
//-------------------------------------------------------------------------------------------------
void CManageActionsDlg::OnBtnRefresh()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		refreshActionList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29094");
}
//-------------------------------------------------------------------------------------------------
void CManageActionsDlg::OnBtnClose()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Call dialog OnOK
		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29095");
}
//-------------------------------------------------------------------------------------------------
void CManageActionsDlg::OnNMDblclkList(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Same as clicking modify
		OnBtnRename();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29096");
}
//-------------------------------------------------------------------------------------------------
void CManageActionsDlg::OnLvnItemchangedList(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update controls when selection changes
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29097");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CManageActionsDlg::updateControls()
{
	try
	{
		// Get the selection count
		unsigned int iCount = m_listActions.GetSelectedCount();

		// Enable delete if at least 1 item selected
		// Enable modify iff 1 item selected
		BOOL bEnableRemove = asMFCBool(iCount > 0);
		BOOL bEnableModify = asMFCBool(iCount == 1);

		m_btnRemove.EnableWindow(bEnableRemove);
		m_btnRename.EnableWindow(bEnableModify);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29098");
}
//-------------------------------------------------------------------------------------------------
void CManageActionsDlg::configureActionList()
{
	try
	{
		// Set list style
		m_listActions.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);

		// Build column information struct
		LVCOLUMN lvColumn;
		lvColumn.mask = LVCF_FMT | LVCF_TEXT | LVCF_WIDTH;
		lvColumn.fmt = LVCFMT_LEFT;

		// Get dimensions of list control
		CRect recList;
		m_listActions.GetClientRect(&recList);

		// Set information for actions column
		lvColumn.pszText = "Actions";
		lvColumn.cx = recList.Width() ;

		// Add the actions column
		m_listActions.InsertColumn(giACTION_COLUMN, &lvColumn);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29099");
}
//-------------------------------------------------------------------------------------------------
void CManageActionsDlg::refreshActionList()
{
	try
	{
		SuspendWindowUpdates updater(*this);

		// Store the position of the first selected item
		int nSelectedItem = 0;
		POSITION pos = m_listActions.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			nSelectedItem = m_listActions.GetNextSelectedItem(pos);
		}
		
		clearListSelection();

		// Clear the current Actions list
		m_listActions.DeleteAllItems();

		IStrToStrMapPtr ipActions = m_ipFAMDB->GetActions();
		ASSERT_RESOURCE_ALLOCATION("ELI29100", ipActions != __nullptr);

		// Get the number of Actions
		long lSize = ipActions->Size;

		// Populate the list
		for (long i=0; i < lSize; i++)
		{
			// Get the Action name and ID
			_bstr_t bstrActionName;
			_bstr_t bstrActionID;
			ipActions->GetKeyValue(i, bstrActionName.GetAddress(), bstrActionID.GetAddress());

			// Add the info into the list
			m_listActions.InsertItem(i, (const char*)bstrActionName);
			DWORD dwID = asLong((const char*)bstrActionID);
			m_listActions.SetItemData(i, dwID);
		}

		// Select either the last selected item position or select the first item
		m_listActions.SetItemState(nSelectedItem < lSize ? nSelectedItem : 0,
			LVIS_SELECTED, LVIS_SELECTED);

		updateControls();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29102");
}
//-------------------------------------------------------------------------------------------------
void CManageActionsDlg::clearListSelection()
{
	try
	{
		POSITION pos = m_listActions.GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			// no item selected, return
			return;
		}

		// iterate through all selections and set them as unselected.  pos will be
		// set to NULL by GetNextSelectedItem if there are no more selected items
		while (pos)
		{
			int nItemSelected = m_listActions.GetNextSelectedItem(pos);
			m_listActions.SetItemState(nItemSelected, 0, LVIS_SELECTED);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29103");
}
//-------------------------------------------------------------------------------------------------

