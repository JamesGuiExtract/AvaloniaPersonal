// SetActionStatusDlg.cpp : implementation file
//

#include "stdafx.h"
#include "SetActionStatusDlg.h"
#include "SelectFileSettings.h"
#include "SelectFilesDlg.h"
#include "FAMDBAdminUtils.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <ADOUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CSetActionStatusDlg dialog
//-------------------------------------------------------------------------------------------------
CSetActionStatusDlg::CSetActionStatusDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB)
: CDialog(CSetActionStatusDlg::IDD),
m_ipFAMDB(ipFAMDB)
{
}
//-------------------------------------------------------------------------------------------------
void CSetActionStatusDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_CMB_ACTION_SET, m_comboActions);
	DDX_Control(pDX, IDC_RADIO_NEW_STATUS, m_radioNewStatus);
	DDX_Control(pDX, IDC_RADIO_STATUS_OF_ACTION, m_radioStatusFromAction);
	DDX_Control(pDX, IDC_CMB_NEW_STATUS, m_comboNewStatus);
	DDX_Control(pDX, IDC_CMB_STATUS_OF_ACTION, m_comboStatusFromAction);
	DDX_Control(pDX, IDC_EDIT_FL_SLCT_SMRY_STATUS, m_editSummary);
	DDX_Control(pDX, IDOK, m_btnOK);
	DDX_Control(pDX, IDC_BTN_APPLY_ACTION_STATUS, m_btnApply);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSetActionStatusDlg, CDialog)
	ON_BN_CLICKED(IDC_RADIO_NEW_STATUS, &CSetActionStatusDlg::OnClickedRadioNewStatus)
	ON_BN_CLICKED(IDC_RADIO_STATUS_OF_ACTION, &CSetActionStatusDlg::OnClickedRadioStatusOfAction)
	ON_BN_CLICKED(IDOK, &CSetActionStatusDlg::OnClickedOK)
	ON_BN_CLICKED(IDC_BTN_APPLY_ACTION_STATUS, &CSetActionStatusDlg::OnClickedApply)
	ON_BN_CLICKED(IDC_BTN_SLCT_FLS_STATUS, &CSetActionStatusDlg::OnClickedSelectFiles)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CSetActionStatusDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CSetActionStatusDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		// Read all actions from the DB
		IStrToStrMapPtr ipMapActions = m_ipFAMDB->GetActions();
		ASSERT_RESOURCE_ALLOCATION("ELI26879", ipMapActions != NULL);

		// Insert actions into combo boxes
		long lSize = ipMapActions->Size;
		for (long i = 0; i < lSize; i++)
		{
			// Get one action's name and ID inside the database
			_bstr_t bstrKey, bstrValue;
			ipMapActions->GetKeyValue(i, bstrKey.GetAddress(), bstrValue.GetAddress());
			string strAction = asString(bstrKey);
			DWORD nID = asUnsignedLong(asString(bstrValue));

			// Insert this action name into three combo boxes
			int iIndexActionTo = m_comboActions.InsertString(-1, strAction.c_str());
			int iIndexActionFrom = m_comboStatusFromAction.InsertString(-1, strAction.c_str());

			// Set the index of the item inside the combo boxes same as the ID of the action
			m_comboActions.SetItemData(iIndexActionTo, nID);
			m_comboStatusFromAction.SetItemData(iIndexActionFrom, nID);
		}
		
		// Set the current action to the first action in three combo boxes
		m_comboActions.SetCurSel(0);
		m_comboStatusFromAction.SetCurSel(0);

		// Set the status items into combo boxes
		CFAMDBAdminUtils::addStatusInComboBox(m_comboNewStatus);

		// Set the initial status to Pending
		m_comboNewStatus.SetCurSel(1);

		// Select the all file reference radio button and new action status
		// radio button as default setting
		m_radioNewStatus.SetCheck(BST_CHECKED);

		// Update the controls
		updateControls();

		// Set the focus to the select files button
		GetDlgItem(IDC_BTN_SLCT_FLS_STATUS)->SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14898")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CSetActionStatusDlg::OnClickedRadioNewStatus()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update the controls
		updateControls();

		// Set focus to the new status combo box
		m_comboNewStatus.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14901")
}
//-------------------------------------------------------------------------------------------------
void CSetActionStatusDlg::OnClickedRadioStatusOfAction()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update the controls
		updateControls();

		// Set focus to the status from action combo box
		m_comboStatusFromAction.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14902")
}
//-------------------------------------------------------------------------------------------------
void CSetActionStatusDlg::OnClickedOK()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// apply the action status changes and close the dialog
		applyActionStatusChanges(true);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16738");
}
//-------------------------------------------------------------------------------------------------
void CSetActionStatusDlg::OnClickedApply()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// apply the action status changes and keep the dialog open
		applyActionStatusChanges(false);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17618");
}
//-------------------------------------------------------------------------------------------------
void CSetActionStatusDlg::OnClickedSelectFiles()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Create the file select dialog
		CSelectFilesDlg dlg(m_ipFAMDB, "Select files to change action status for",
			"SELECT FAMFile.ID FROM", m_settings);

		// Display the dialog and save changes if user clicked OK
		if (dlg.DoModal() == IDOK)
		{
			// Get the settings from the dialog
			m_settings = dlg.getSettings();

			// Update the summary description
			m_editSummary.SetWindowText(m_settings.getSummaryString().c_str());
		}

		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26981");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CSetActionStatusDlg::applyActionStatusChanges(bool bCloseDialog)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Add application trace whenever a database modification is made
		// [LRCAU #5052 - JDS - 12/18/2008]
		UCLIDException uex("ELI23597", "Application trace: Database change");
		uex.addDebugInfo("Change", "Set action status");
		uex.addDebugInfo("User Name", getCurrentUserName());
		uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
		uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));

		// Get the selected action name and ID that will change the status
		CString zToActionName;
		m_comboActions.GetWindowText(zToActionName);
		long lIndex = m_comboActions.GetCurSel();
		long lToActionID = m_comboActions.GetItemData(lIndex);
		uex.addDebugInfo("Action To Change", (LPCTSTR)zToActionName);
		uex.addDebugInfo("Action ID To Change", lToActionID);

		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		// Check whether setting a new status or copying from existing action status
		bool bNewStatus = m_radioNewStatus.GetCheck() == BST_CHECKED;
		switch(m_settings.getScope())
		{
		// If choose to change that status for all the files
		case eAllFiles:
			{
				if (bNewStatus)
				{
					// Get the new status ID and cast to EActionStatus
					int iStatusID = m_comboNewStatus.GetCurSel();
					UCLID_FILEPROCESSINGLib::EActionStatus eNewStatus = 
						(UCLID_FILEPROCESSINGLib::EActionStatus)(iStatusID);

					// Call SetStatusForAllFiles() to set the new status for the selected action
					m_ipFAMDB->SetStatusForAllFiles(_bstr_t(zToActionName), eNewStatus);
					uex.addDebugInfo("New Status", asString(m_ipFAMDB->AsStatusString(eNewStatus)));
				}
				else
				{
					// Get the action ID from which we will copy the status to the selected action
					long lIndex = m_comboStatusFromAction.GetCurSel();
					long lFromActionID = m_comboStatusFromAction.GetItemData(lIndex);

					// Call CopyActionStatusFromAction() to set the new status for the selected action
					m_ipFAMDB->CopyActionStatusFromAction(lFromActionID, lToActionID);
					uex.addDebugInfo("Copy From Action", lFromActionID);
				}
			}
			break;

		case eAllFilesForWhich:
		// If choose to change the status for the files according another action's status 
		{
			// Get the From action ID
			long lWhereActionID = m_settings.getActionID();

			// Get the status ID for the action from which we will copy the status to the selected action
			int iWhereStatusID = m_settings.getStatus();
			UCLID_FILEPROCESSINGLib::EActionStatus eWhereStatus = 
				(UCLID_FILEPROCESSINGLib::EActionStatus)(iWhereStatusID);
			uex.addDebugInfo("Action Where", lWhereActionID);
			uex.addDebugInfo("Action Where Status", asString(m_ipFAMDB->AsStatusString(eWhereStatus)));

			UCLID_FILEPROCESSINGLib::EActionStatus eNewStatus =
				(UCLID_FILEPROCESSINGLib::EActionStatus)(0);
			long lFromActionID = -1;
			if (bNewStatus)
			{
				// Get the new status ID and cast to EActionStatus
				int iStatusID = m_comboNewStatus.GetCurSel();
				eNewStatus = (UCLID_FILEPROCESSINGLib::EActionStatus)(iStatusID);
				uex.addDebugInfo("Action To Status",
					asString(m_ipFAMDB->AsStatusString(eNewStatus)));
			}
			else
			{
				long lIndex = m_comboStatusFromAction.GetCurSel();
				lFromActionID = m_comboStatusFromAction.GetItemData(lIndex);
				uex.addDebugInfo("Copy From Action", lFromActionID); 
			}

			// If going from the skipped status check user name list
			string strUser = "";
			if (eWhereStatus == kActionSkipped)
			{
				// Get the user name from the settings
				strUser = m_settings.getUser();
				uex.addDebugInfo("Skipped By User", strUser);

				// If user is any user set user string to ""
				if (strUser == gstrANY_USER)
				{
					// Set status for all skipped files
					strUser = "";
				}
			}

			// Call SearchAndModifyFileStatus() to set the new status for the selected action
			m_ipFAMDB->SearchAndModifyFileStatus(lWhereActionID, eWhereStatus,
				lToActionID, eNewStatus, strUser.c_str(), lFromActionID);
		}
		break;

		case eAllFilesTag:
			// THIS IS NOT CURRENTLY IMPLEMENTED
			break;

		case eAllFilesQuery:
			{
				string strSQL = m_settings.getSQLString();
				uex.addDebugInfo("SQL Query", strSQL);
					UCLID_FILEPROCESSINGLib::EActionStatus eNewStatus = 
						(UCLID_FILEPROCESSINGLib::EActionStatus)(0);
					CString zFromAction = "";
				if (bNewStatus)
				{
					// Get the new status ID and cast to EActionStatus
					int iStatusID = m_comboNewStatus.GetCurSel();
					eNewStatus = (UCLID_FILEPROCESSINGLib::EActionStatus)(iStatusID);
					uex.addDebugInfo("Action To Status",
						asString(m_ipFAMDB->AsStatusString(eNewStatus)));
				}
				else
				{
					// Get the action name from the combo box
					m_comboStatusFromAction.GetWindowText(zFromAction);

					// Add the action ID to the debug data
					long nFromActionID =
						m_comboStatusFromAction.GetItemData(m_comboStatusFromAction.GetCurSel());
					uex.addDebugInfo("Copy From Action", nFromActionID); 
				}

				// Modify the action status from the specified query
				m_ipFAMDB->ModifyActionStatusForQuery(strSQL.c_str(),
					(LPCTSTR)zToActionName, eNewStatus, (LPCTSTR)zFromAction);
			}
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI26911");
		}

		// Log application trace [LRCAU #5052 - JDS - 12/18/2008]
		uex.log();

		if(bCloseDialog)
		{
			OnOK();
		}

		// Prompt to remind user that reset action is finished
		CString zPrompt = "The action status of '" + zToActionName + "' has been reset.";
		MessageBox(zPrompt, "Success", MB_ICONINFORMATION);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14903")
}
//-------------------------------------------------------------------------------------------------
void CSetActionStatusDlg::updateControls() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// If the new status radio button is checked
		if (m_radioNewStatus.GetCheck() == BST_CHECKED)
		{
			// Enable the new status combo box and disable 
			// the combo box from which to copy status
			m_comboNewStatus.EnableWindow(TRUE);
			m_comboStatusFromAction.EnableWindow(FALSE);
		}
		else
		{
			// Disable the new status combo box and Enable the combo box
			// from which to copy status
			m_comboNewStatus.EnableWindow(FALSE);
			m_comboStatusFromAction.EnableWindow(TRUE);
		}

		// Enable/disable the apply and ok buttons based on settings
		BOOL bEnable = asMFCBool(m_settings.isInitialized());
		m_btnOK.EnableWindow(bEnable);
		m_btnApply.EnableWindow(bEnable);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14904");
}
//-------------------------------------------------------------------------------------------------
