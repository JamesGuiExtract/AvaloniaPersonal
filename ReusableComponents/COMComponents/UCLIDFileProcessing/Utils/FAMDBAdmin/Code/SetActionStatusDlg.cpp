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
CSetActionStatusDlg::CSetActionStatusDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB,
										 CFAMDBAdminDlg* pFAMDBAdmin)
: CDialog(CSetActionStatusDlg::IDD),
m_ipFAMDB(ipFAMDB),
m_pFAMDBAdmin(pFAMDBAdmin)
{
	ASSERT_ARGUMENT("ELI31255", ipFAMDB != __nullptr);
	ASSERT_ARGUMENT("ELI27697", pFAMDBAdmin != __nullptr);
}
//-------------------------------------------------------------------------------------------------
CSetActionStatusDlg::CSetActionStatusDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB,
	CFAMDBAdminDlg* pFAMDBAdmin, const SelectFileSettings &selectSettings)
: CDialog(CSetActionStatusDlg::IDD)
, m_ipFAMDB(ipFAMDB)
, m_pFAMDBAdmin(pFAMDBAdmin)
, m_settings(selectSettings)
{
	ASSERT_ARGUMENT("ELI31256", ipFAMDB != __nullptr);
	ASSERT_ARGUMENT("ELI31257", pFAMDBAdmin != __nullptr);
}
//-------------------------------------------------------------------------------------------------
CSetActionStatusDlg::~CSetActionStatusDlg()
{
	try
	{
		m_ipFAMDB = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27695");
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
		ASSERT_RESOURCE_ALLOCATION("ELI26879", ipMapActions != __nullptr);

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
		
		// Per discussion with Arvind:
		// Don't initialize the target action or status. If a user is forced to consciously make a
		// choice, it is less likely they will accidentally make a mistake that will be hard to
		// correct.
		// m_comboActions.SetCurSel(0);
		// m_comboStatusFromAction.SetCurSel(0);
		// m_comboNewStatus.SetCurSel(1);

		// Set the status items into combo boxes
		CFAMDBAdminUtils::addStatusInComboBox(m_comboNewStatus);

		// Select the all file reference radio button and new action status
		// radio button as default setting
		m_radioNewStatus.SetCheck(BST_CHECKED);

		// Update the controls
		updateControls();

		// Update the summary edit box with the settings
		m_editSummary.SetWindowText(m_settings.getSummaryString().c_str());

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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27422");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CSetActionStatusDlg::applyActionStatusChanges(bool bCloseDialog)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Validate action selection
		long lIndex = m_comboActions.GetCurSel();
		if (lIndex == CB_ERR)
		{
			MessageBox("You must select and action to set.", "No Action", MB_OK | MB_ICONERROR);
			m_comboActions.SetFocus();
			return;
		}

		// Validate target status selection
		bool bNewStatus = m_radioNewStatus.GetCheck() == BST_CHECKED;
		int nNewStatusIndex = bNewStatus ?
			m_comboNewStatus.GetCurSel() : m_comboStatusFromAction.GetCurSel();
		if (nNewStatusIndex == CB_ERR)
		{
			string strMessage;
			string strCaption;
			CComboBox* pCombo = __nullptr;
			if (bNewStatus)
			{
				strMessage = "You must select a new status.";
				strCaption = "No Status Selected";
				pCombo = &m_comboNewStatus;
			}
			else
			{
				strMessage = "You must select a source action.";
				strCaption = "No Source Action";
				pCombo = &m_comboStatusFromAction;
			}
			MessageBox(strMessage.c_str(), strCaption.c_str(), MB_OK | MB_ICONERROR);
			pCombo->SetFocus();
			return;
		}

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
		long lToActionID = m_comboActions.GetItemData(lIndex);
		uex.addDebugInfo("Action To Change", (LPCTSTR)zToActionName);
		uex.addDebugInfo("Action ID To Change", lToActionID);

		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		// Check whether setting a new status or copying from existing action status
		long lFromActionID = -1;
		CString zFromAction = "";
		EActionStatus eNewStatus = kActionUnattempted;
		if (bNewStatus)
		{
			// Get the new status ID and cast to EActionStatus
			eNewStatus = (EActionStatus)(nNewStatusIndex);
			uex.addDebugInfo("New Status", asString(m_ipFAMDB->AsStatusString(eNewStatus)));
		}
		else
		{
			lFromActionID = m_comboStatusFromAction.GetItemData(nNewStatusIndex);
			m_comboStatusFromAction.GetWindowText(zFromAction);
			uex.addDebugInfo("Copy From Action", lFromActionID); 
		}

		// Check the scope for logging application trace
		switch(m_settings.getScope())
		{
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

			// If going from the skipped status check user name list
			string strUser = "";
			if (eWhereStatus == kActionSkipped)
			{
				// Get the user name from the settings
				strUser = m_settings.getUser();
				uex.addDebugInfo("Skipped By User", strUser);
			}
		}
		break;

		case eAllFilesPriority:
			{
				uex.addDebugInfo("Priority String", m_settings.getPriorityString());
			}
			break;
		}

		// If not processing all files or limiting the scope by a random subset
		// then the operation must be performed a file at a time, otherwise
		// just set the status for all files
		if (m_settings.getScope() != eAllFiles || m_settings.getLimitByRandomCondition())
		{
			// Get the query for updating files
			string strSelect = "FAMFile.ID";
			string strQuery = m_settings.buildQuery(m_ipFAMDB, strSelect, "");
			uex.addDebugInfo("Query", strQuery);

			// Modify the file status
			m_ipFAMDB->ModifyActionStatusForQuery(strQuery.c_str(), (LPCTSTR)zToActionName,
				eNewStatus, (LPCTSTR)zFromAction, m_settings.getRandomCondition());
		}
		else
		{
			if (lFromActionID == -1)
			{
				m_ipFAMDB->SetStatusForAllFiles((LPCTSTR)zToActionName, eNewStatus);
			}
			else
			{
				m_ipFAMDB->CopyActionStatusFromAction(lFromActionID, lToActionID);
			}
		}


		// Log application trace [LRCAU #5052 - JDS - 12/18/2008]
		uex.log();

		// Alert the FAMDBAdmin to update the status tab
		m_pFAMDBAdmin->UpdateSummaryTab(lToActionID);

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
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14904");
}
//-------------------------------------------------------------------------------------------------
