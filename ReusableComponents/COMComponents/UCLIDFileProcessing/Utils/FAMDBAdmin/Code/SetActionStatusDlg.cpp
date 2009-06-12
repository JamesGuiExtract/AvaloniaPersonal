// SetActionStatusDlg.cpp : implementation file
//

#include "stdafx.h"
#include "SetActionStatusDlg.h"
#include "FAMDBAdminUtils.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>

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
	//{{AFX_DATA_INIT(CSetActionStatusDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void CSetActionStatusDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CSetActionStatusDlg)
	// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
	DDX_Control(pDX, IDC_CMB_ACTION_SET, m_comboActions);
	DDX_Control(pDX, IDC_RADIO_ALL_FILES, m_radioAllFiles);
	DDX_Control(pDX, IDC_RADIO_FILES_UNDER_STATUS, m_radioFilesForWhich);
	DDX_Control(pDX, IDC_CMB_FILE_ACTION, m_comboFilesUnderAction);
	DDX_Control(pDX, IDC_CMB_FILE_STATUS, m_comboFilesUnderStatus);
	DDX_Control(pDX, IDC_RADIO_NEW_STATUS, m_radioNewStatus);
	DDX_Control(pDX, IDC_RADIO_STATUS_OF_ACTION, m_radioStatusFromAction);
	DDX_Control(pDX, IDC_CMB_NEW_STATUS, m_comboNewStatus);
	DDX_Control(pDX, IDC_CMB_STATUS_OF_ACTION, m_comboStatusFromAction);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSetActionStatusDlg, CDialog)
	//{{AFX_MSG_MAP(CSetActionStatusDlg)
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_RADIO_ALL_FILES, &CSetActionStatusDlg::OnClickedRadioAllFiles)
	ON_BN_CLICKED(IDC_RADIO_FILES_UNDER_STATUS, &CSetActionStatusDlg::OnClickedRadioFilesStatus)
	ON_BN_CLICKED(IDC_RADIO_NEW_STATUS, &CSetActionStatusDlg::OnClickedRadioNewStatus)
	ON_BN_CLICKED(IDC_RADIO_STATUS_OF_ACTION, &CSetActionStatusDlg::OnClickedRadioStatusOfAction)
	ON_BN_CLICKED(IDOK, &CSetActionStatusDlg::OnClickedOK)
	ON_BN_CLICKED(IDC_BTN_APPLY_ACTION_STATUS, &CSetActionStatusDlg::OnClickedApply)
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
		IStrToStrMapPtr pMapActions = m_ipFAMDB->GetActions();

		// Insert actions into combo boxes
		for (int i = 0; i < pMapActions->Size; i++)
		{
			// Get one action's name and ID inside the database
			_bstr_t bstrKey, bstrValue;
			pMapActions->GetKeyValue(i, &bstrKey.GetBSTR(), &bstrValue.GetBSTR());
			string strAction = asString(bstrKey);
			DWORD nID = asUnsignedLong(asString(bstrValue));

			// Insert this action name into three combo boxes
			int iIndexActionTo = m_comboActions.InsertString(-1, strAction.c_str());
			int iIndexActionUnderCondition = m_comboFilesUnderAction.InsertString(-1, strAction.c_str());
			int iIndexActionFrom = m_comboStatusFromAction.InsertString(-1, strAction.c_str());

			// Set the index of the item inside the combo boxes same as the ID of the action
			m_comboActions.SetItemData(iIndexActionTo, nID);
			m_comboFilesUnderAction.SetItemData(iIndexActionUnderCondition, nID);
			m_comboStatusFromAction.SetItemData(iIndexActionFrom, nID);
		}
		
		// Set the current action to the first action in three combo boxes
		m_comboActions.SetCurSel(0);
		m_comboFilesUnderAction.SetCurSel(0);
		m_comboStatusFromAction.SetCurSel(0);

		// Set the status items into combo boxes
		CFAMDBAdminUtils::addStatusInComboBox(m_comboFilesUnderStatus);
		CFAMDBAdminUtils::addStatusInComboBox(m_comboNewStatus);

		// Set the initial status to Pending
		m_comboFilesUnderStatus.SetCurSel(1);
		m_comboNewStatus.SetCurSel(1);

		// Select the all file reference radio button and new action status
		// radio button as default setting
		m_radioAllFiles.SetCheck(BST_CHECKED);
		m_radioNewStatus.SetCheck(BST_CHECKED);

		// Update the controls
		updateControls();

		// Set the focus to the status combo box
		m_comboActions.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14898")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CSetActionStatusDlg::OnClickedRadioAllFiles()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update the controls
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14899")
}
//-------------------------------------------------------------------------------------------------
void CSetActionStatusDlg::OnClickedRadioFilesStatus()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update the controls
		updateControls();

		// Set focus to the action combo box under condition
		m_comboFilesUnderAction.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14900")
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

		// If choose to change that status for all the files
		if (m_radioAllFiles.GetCheck() == BST_CHECKED)
		{
			if (m_radioNewStatus.GetCheck() == BST_CHECKED)
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
		// If choose to change the status for the files according another action's status 
		else
		{
			// Get the From action ID
			long lIndex = m_comboFilesUnderAction.GetCurSel();
			long lFromActionID = m_comboFilesUnderAction.GetItemData(lIndex);

			// Get the status ID for the action from which we will copy the status to the selected action
			int iFromStatusID = m_comboFilesUnderStatus.GetCurSel();
			UCLID_FILEPROCESSINGLib::EActionStatus eFromStatus = 
				(UCLID_FILEPROCESSINGLib::EActionStatus)(iFromStatusID);
			uex.addDebugInfo("Action From", lFromActionID);
			uex.addDebugInfo("Action From Status", asString(m_ipFAMDB->AsStatusString(eFromStatus)));

			if (m_radioNewStatus.GetCheck() == BST_CHECKED)
			{
				// Get the new status ID and cast to EActionStatus
				int iStatusID = m_comboNewStatus.GetCurSel();
				UCLID_FILEPROCESSINGLib::EActionStatus eNewStatus = 
					(UCLID_FILEPROCESSINGLib::EActionStatus)(iStatusID);

				// Call SearchAndModifyFileStatus() to set the new status for the selected action
				m_ipFAMDB->SearchAndModifyFileStatus(lFromActionID, eFromStatus, lToActionID, eNewStatus);
				uex.addDebugInfo("Action To Status", asString(m_ipFAMDB->AsStatusString(eNewStatus)));
			}
			else
			{
				// We will never reach here
				THROW_LOGIC_ERROR_EXCEPTION("ELI14905");
			}
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
		// If the all files radio button is checked
		if (m_radioAllFiles.GetCheck() == BST_CHECKED)
		{
			// Disable the action and status combo boxes under 
			// "All files for which" radio button
			m_comboFilesUnderAction.EnableWindow(FALSE);
			m_comboFilesUnderStatus.EnableWindow(FALSE);

			// Enable the status of action radio button
			m_radioStatusFromAction.EnableWindow(TRUE);
		}
		else
		{
			// Enable the action and status combo boxes from which to select files
			m_comboFilesUnderAction.EnableWindow(TRUE);
			m_comboFilesUnderStatus.EnableWindow(TRUE);

			// Disable the status of action radio button and combo box
			m_radioStatusFromAction.EnableWindow(FALSE);
			m_comboStatusFromAction.EnableWindow(FALSE);
		}

		// If the new status radio button is checked
		if (m_radioNewStatus.GetCheck() == BST_CHECKED)
		{
			// Enable the new status combo box and disable 
			// the combo box from which to copy status
			m_comboNewStatus.EnableWindow(TRUE);
			m_comboStatusFromAction.EnableWindow(FALSE);

			// Enable the files under condition radio button and check boxes
			m_radioFilesForWhich.EnableWindow(TRUE);
		}
		else
		{
			// Disable the new status combo box and Enable the combo box
			// from which to copy status
			m_comboNewStatus.EnableWindow(FALSE);
			m_comboStatusFromAction.EnableWindow(TRUE);

			// Disable the files under condition radio button and check boxes
			m_radioFilesForWhich.EnableWindow(FALSE);
			m_comboFilesUnderAction.EnableWindow(FALSE);
			m_comboFilesUnderStatus.EnableWindow(FALSE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14904");
}
//-------------------------------------------------------------------------------------------------
