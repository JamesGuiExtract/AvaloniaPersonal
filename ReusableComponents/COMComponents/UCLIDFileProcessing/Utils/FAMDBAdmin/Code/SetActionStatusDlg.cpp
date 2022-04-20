// SetActionStatusDlg.cpp : implementation file
//

#include "stdafx.h"
#include "SetActionStatusDlg.h"
#include "FAMDBAdminUtils.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <ADOUtils.h>
#include <FAMHelperFunctions.h>

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
m_pFAMDBAdmin(pFAMDBAdmin),
m_ipFileSelector(CLSID_FAMFileSelector)
{
	ASSERT_ARGUMENT("ELI31255", ipFAMDB != __nullptr);
	ASSERT_ARGUMENT("ELI27697", pFAMDBAdmin != __nullptr);
	ASSERT_ARGUMENT("ELI35681", m_ipFileSelector != __nullptr);
}
//-------------------------------------------------------------------------------------------------
CSetActionStatusDlg::CSetActionStatusDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB,
	CFAMDBAdminDlg* pFAMDBAdmin, UCLID_FILEPROCESSINGLib::IFAMFileSelectorPtr ipFileSelector)
: CDialog(CSetActionStatusDlg::IDD)
, m_ipFAMDB(ipFAMDB)
, m_pFAMDBAdmin(pFAMDBAdmin)
, m_ipFileSelector(ipFileSelector)
{
	ASSERT_ARGUMENT("ELI31256", ipFAMDB != __nullptr);
	ASSERT_ARGUMENT("ELI31257", pFAMDBAdmin != __nullptr);
	ASSERT_ARGUMENT("ELI35682", m_ipFileSelector != __nullptr);
}
//-------------------------------------------------------------------------------------------------
CSetActionStatusDlg::~CSetActionStatusDlg()
{
	try
	{
		m_ipFAMDB = __nullptr;
		m_ipFileSelector = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27695");
}
//-------------------------------------------------------------------------------------------------
void CSetActionStatusDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_CMB_ACTION_SET, m_comboActions);
	DDX_Control(pDX, IDC_CMB_NEW_STATUS, m_comboNewStatus);
	DDX_Control(pDX, IDC_CMB_USER, m_comboUser);
	DDX_Control(pDX, IDC_EDIT_FL_SLCT_SMRY_STATUS, m_editSummary);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSetActionStatusDlg, CDialog)
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

		if (asCppBool(m_ipFAMDB->GetUsingWorkflows()) && m_ipFAMDB->ActiveWorkflow.length() == 0)
		{
			if (IDYES != MessageBox(
				"Setting action status under <All workflows> may cause \r\n"
				"unexpected results. This will result in the same operation \r\n"
				"being executed separately for each workflow in the database.\r\n\r\n"
				"Be sure you are clear on the effect this will have on each \r\n"
				"workflow before proceeding.\r\n\r\n"
				"Proceed?", "Warning", MB_YESNO | MB_ICONWARNING))
			{
				EndDialog(0);
				return FALSE;
			}
		}

		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		// Read all actions from the DB
		IStrToStrMapPtr ipMapActions = m_ipFAMDB->GetActions();
		ASSERT_RESOURCE_ALLOCATION("ELI26879", ipMapActions != __nullptr);

		// Fill the Actions combo boxes
		fillComboBoxFromMap(m_comboActions, ipMapActions);

		IStrToStrMapPtr ipUsers = m_ipFAMDB->GetFamUsers();
		ASSERT_RESOURCE_ALLOCATION("ELI53357", ipUsers != __nullptr);
		fillComboBoxFromMap(m_comboUser, ipUsers);
		m_comboUser.InsertString(0, "<no user>");
		m_comboUser.InsertString(0, "<any user>");
		m_comboUser.SetItemData(0, -1);
		m_comboUser.SetCurSel(0);

		// Per discussion with Arvind:
		// Don't initialize the target action or status. If a user is forced to consciously make a
		// choice, it is less likely they will accidentally make a mistake that will be hard to
		// correct.
		// m_comboActions.SetCurSel(0);
		// m_comboStatusFromAction.SetCurSel(0);
		// m_comboNewStatus.SetCurSel(1);

		// Set the status items into combo boxes
		CFAMDBAdminUtils::addStatusInComboBox(m_comboNewStatus);

		// Update the summary edit box with the settings
		m_editSummary.SetWindowText(asString(
			m_ipFileSelector->GetSummaryString(m_ipFAMDB, false)).c_str());

		// Set the focus to the select files button
		GetDlgItem(IDC_BTN_SLCT_FLS_STATUS)->SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14898")

	return FALSE;
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
		// Display the select files configuration dialog
		bool bAppliedSettings = asCppBool(m_ipFileSelector->Configure(m_ipFAMDB,
			"Select files to change action status for", "SELECT FAMFile.ID FROM FAMFile", false));

		// Update the summary text if new settings were applied.
		if (bAppliedSettings)
		{
			string strSummaryString = asString(
				m_ipFileSelector->GetSummaryString(m_ipFAMDB, false));
			m_editSummary.SetWindowText(strSummaryString.c_str());
		}
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
			MessageBox("You must select an action to set.", "No Action", MB_OK | MB_ICONERROR);
			m_comboActions.SetFocus();
			return;
		}

		// Validate target status selection
		int nNewStatusIndex = m_comboNewStatus.GetCurSel();
		if (nNewStatusIndex == CB_ERR)
		{
			string strMessage;
			string strCaption;
			CComboBox* pCombo = __nullptr;
			strMessage = "You must select a new status.";
			strCaption = "No Status Selected";
			pCombo = &m_comboNewStatus;
			MessageBox(strMessage.c_str(), strCaption.c_str(), MB_OK | MB_ICONERROR);
			pCombo->SetFocus();
			return;
		}

		if (asCppBool(m_ipFAMDB->IsAnyFAMActive()))
		{
			int nContinue = MessageBox(
				"It is recommended that processing be stopped before modifying file "
				"action statuses.\r\n\r\n"
				"During this operation the database will be locked. It is possible this will "
				"lead to processing/queuing errors if this operation takes a long time.\r\n\r\n"
				"Proceed with the operation?",
				"Processing is active", MB_ICONWARNING | MB_YESNO);

			if (nContinue == IDNO)
			{
				return;
			}
		}

		// Get the selected user
		long nUserIndex = m_comboUser.GetCurSel();
		long nUserID = m_comboUser.GetItemData(nUserIndex);
		CString zSelectedUser;
		m_comboUser.GetWindowText(zSelectedUser);

		// Add application trace whenever a database modification is made
		// [LRCAU #5052 - JDS - 12/18/2008]
		UCLIDException uex("ELI23597", "Application trace: Database change");
		uex.addDebugInfo("Change", "Set action status");
		uex.addDebugInfo("User Name", getCurrentUserName());
		uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
		uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));
		uex.addDebugInfo("TargetUser", (LPCSTR)zSelectedUser);

		// Get the selected action name and ID that will change the status
		CString zToActionName;
		m_comboActions.GetWindowText(zToActionName);
		long lToActionID = m_comboActions.GetItemData(lIndex);
		uex.addDebugInfo("Action To Change", (LPCTSTR)zToActionName);
		uex.addDebugInfo("Action ID To Change", lToActionID);

		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		// Get the new status ID and cast to EActionStatus
		EActionStatus eNewStatus = (EActionStatus)(nNewStatusIndex);
		uex.addDebugInfo("New Status", asString(m_ipFAMDB->AsStatusString(eNewStatus)));

		string strSummaryString = asString(
			m_ipFileSelector->GetSummaryString(m_ipFAMDB, false));
		uex.addDebugInfo("Files Selected", strSummaryString);

		if (!asCppBool(m_ipFileSelector->SelectingAllFiles))
		{
			// Get the query for updating files
			string strSelect = "FAMFile.ID";
			string strQuery =
				asString(m_ipFileSelector->BuildQuery(m_ipFAMDB, get_bstr_t(strSelect), "", VARIANT_FALSE));
			uex.addDebugInfo("Query", strQuery);

			// Modify the file status
			try
			{
				try
				{
					m_ipFAMDB->ModifyActionStatusForSelection(m_ipFileSelector, (LPCTSTR)zToActionName,
						eNewStatus, /*vbModifyWhenTargetActionMissingForSomeFiles*/ VARIANT_FALSE, nUserID);
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51517");
			}
			catch (UCLIDException &ueModifyError)
			{
				if (ueModifyError.getTopELI() == "ELI51515")
				{
					if (!handleCantSetActionStatusForAllWorkflows(ueModifyError, zToActionName, eNewStatus, nUserID))
					{
						if (bCloseDialog)
						{
							OnCancel();
						}
						return;
					}
				}
				else
				{
					throw ueModifyError;
				}
			}
		}
		else
		{
			m_ipFAMDB->SetStatusForAllFiles((LPCTSTR)zToActionName, eNewStatus, nUserID);
		}

		// Log application trace [LRCAU #5052 - JDS - 12/18/2008]
		uex.log();

		// Alert the FAMDBAdmin to update the status tab
		m_pFAMDBAdmin->UpdateSummaryTab();

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
bool CSetActionStatusDlg::handleCantSetActionStatusForAllWorkflows(UCLIDException& ueModifyError,
	CString& zToActionName, EActionStatus eNewStatus, long nUserIdToSet)
{
	long nCount = 0;
	for each (auto debugData in ueModifyError.getDebugVector())
	{
		if (debugData.GetName() == "Number able to set")
		{
			nCount = debugData.GetPair().getLongValue();
			break;
		}
	}

	if (nCount > 0)
	{
		CString zPrompt = "The target action of '" + zToActionName +
			"' does not exist in the workflow(s) of all specified files.\r\n\r\n";

		for (auto uexInner = ueModifyError.getInnerException();
			uexInner != __nullptr;
			uexInner = uexInner->getInnerException())
		{
			zPrompt += CString(uexInner->getTopText().c_str()) + "\r\n";
		}

		zPrompt += Util::Format("\r\nDo you want to set the status for the %d "
			"files from workflows in which the action '%s' exists?",
			nCount, (LPCTSTR)zToActionName).c_str();

		if (IDYES == MessageBox(zPrompt, "Target action missing", MB_YESNO))
		{
			m_ipFAMDB->ModifyActionStatusForSelection(m_ipFileSelector, (LPCTSTR)zToActionName,
				eNewStatus, /*vbModifyWhenTargetActionMissingForSomeFiles*/ VARIANT_TRUE, nUserIdToSet);

			return true;
		}
		else
		{
			ueModifyError.log();

			return false;
		}
	}
	else
	{
		throw ueModifyError;
	}
}

//-------------------------------------------------------------------------------------------------
