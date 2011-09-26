#include "StdAfx.h"
#include "ActionStatusConditionDlg.h"

#include "FAMDBAdminUtils.h"

#include <UCLIDException.h>
#include <ComUtils.h>
#include <ADOUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// ActionStatusConditionDlg dialog
//-------------------------------------------------------------------------------------------------
ActionStatusConditionDlg::ActionStatusConditionDlg(const IFileProcessingDBPtr& ipFAMDB)
: CDialog(ActionStatusConditionDlg::IDD),
m_ipFAMDB(ipFAMDB)
{
}
//-------------------------------------------------------------------------------------------------
ActionStatusConditionDlg::ActionStatusConditionDlg(const IFileProcessingDBPtr& ipFAMDB,
												   const ActionStatusCondition& settings)
: CDialog(ActionStatusConditionDlg::IDD),
m_ipFAMDB(ipFAMDB),
m_settings(settings)
{
}
//-------------------------------------------------------------------------------------------------
ActionStatusConditionDlg::~ActionStatusConditionDlg()
{
	try
	{
		m_ipFAMDB = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33771");
}
//-------------------------------------------------------------------------------------------------
void ActionStatusConditionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(ActionStatusConditionDlg)
	// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
	DDX_Control(pDX, IDC_CMB_FILE_ACTION, m_comboFilesUnderAction);
	DDX_Control(pDX, IDC_CMB_FILE_STATUS, m_comboFilesUnderStatus);
	DDX_Control(pDX, IDC_CMB_FILE_SKIPPED_USER, m_comboSkippedUser);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(ActionStatusConditionDlg, CDialog)
	//{{AFX_MSG_MAP(ActionStatusConditionDlg)
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_SELECT_BTN_OK, &ActionStatusConditionDlg::OnClickedOK)
	ON_BN_CLICKED(IDC_SELECT_BTN_CANCEL, &ActionStatusConditionDlg::OnClickedCancel)
	ON_CBN_SELCHANGE(IDC_CMB_FILE_ACTION, &ActionStatusConditionDlg::OnFilesUnderActionChange)
	ON_CBN_SELCHANGE(IDC_CMB_FILE_STATUS, &ActionStatusConditionDlg::OnFilesUnderStatusChange)
	END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// ActionStatusConditionDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL ActionStatusConditionDlg::OnInitDialog() 
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
		ASSERT_RESOURCE_ALLOCATION("ELI33772", ipMapActions != __nullptr);

		// Insert actions into combo box
		long lSize = ipMapActions->Size;
		for (long i = 0; i < lSize; i++)
		{
			// Get the name and ID of the action
			_bstr_t bstrKey, bstrValue;
			ipMapActions->GetKeyValue(i, bstrKey.GetAddress(), bstrValue.GetAddress());
			string strAction = asString(bstrKey);
			DWORD nID = asUnsignedLong(asString(bstrValue));

			// Insert this action name into the combo box
			int iIndexActionUnderCondition = m_comboFilesUnderAction.InsertString(-1, strAction.c_str());

			// Set the index of the item inside the combo box same as the ID of the action
			m_comboFilesUnderAction.SetItemData(iIndexActionUnderCondition, nID);
		}
		
		// Set the current action to the first action in the combo box
		m_comboFilesUnderAction.SetCurSel(0);

		// Set the status items into combo box
		CFAMDBAdminUtils::addStatusInComboBox(m_comboFilesUnderStatus);

		// Set the initial status to Pending
		m_comboFilesUnderStatus.SetCurSel(1);

		// Update the controls
		updateControls();

		// Read the settings object and set the dialog based on the settings
		setControlsFromSettings();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33773")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void ActionStatusConditionDlg::OnOK()
{
	// Allow the user to use the enter key to close the dialog
	OnClickedOK();
}
//-------------------------------------------------------------------------------------------------
void ActionStatusConditionDlg::OnClose()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	try
	{
		EndDialog(IDCLOSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33774");
}
//-------------------------------------------------------------------------------------------------
void ActionStatusConditionDlg::OnClickedCancel()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Call cancel
		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33775");
}
//-------------------------------------------------------------------------------------------------
void ActionStatusConditionDlg::OnClickedOK()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Save the settings
		if (saveSettings())
		{
			// If settings saved successfully, close the dialog
			CDialog::OnOK();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33776");
}
//-------------------------------------------------------------------------------------------------
void ActionStatusConditionDlg::OnFilesUnderActionChange()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update the controls (this will also refill the skipped user combo box)
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33777");
}
//-------------------------------------------------------------------------------------------------
void ActionStatusConditionDlg::OnFilesUnderStatusChange()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update the controls
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33778");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool ActionStatusConditionDlg::saveSettings()
{
	try
	{
		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		// Get the From action ID and name
		long lIndex = m_comboFilesUnderAction.GetCurSel();
		long lFromActionID = m_comboFilesUnderAction.GetItemData(lIndex);
		CString zTemp;
		m_comboFilesUnderAction.GetWindowText(zTemp);

		// Set the action from name and ID
		m_settings.setAction((LPCSTR) zTemp);
		m_settings.setActionID(lFromActionID);

		// Get the status ID for the action from which we will copy the status to the selected action
		int iFromStatusID = m_comboFilesUnderStatus.GetCurSel();
		m_comboFilesUnderStatus.GetWindowText(zTemp);
		m_settings.setStatus(iFromStatusID);
		m_settings.setStatusString((LPCTSTR) zTemp);

		// If going from the skipped status check user name list
		if (iFromStatusID == kActionSkipped)
		{
			// Get the user name from the combo box
			m_comboSkippedUser.GetWindowText(zTemp);
			m_settings.setUser((LPCTSTR) zTemp);
		}

		return true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33779")
}
//-------------------------------------------------------------------------------------------------
void ActionStatusConditionDlg::updateControls() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		m_comboSkippedUser.EnableWindow(FALSE);

		// Enable the user combo box if the files under status is "Skipped"
		if (m_comboFilesUnderStatus.GetCurSel() == kActionSkipped)
		{
			// Get the current text from the combo box
			CString zText;
			m_comboSkippedUser.GetWindowText(zText);

			m_comboSkippedUser.EnableWindow(TRUE);

			// Update the skipped user list
			fillSkippedUsers();

			// Attempt to reselect the last selection (otherwise choose first item)
			int nSelection = m_comboSkippedUser.FindStringExact(-1, zText);
			m_comboSkippedUser.SetCurSel(nSelection != CB_ERR ? nSelection : 0);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33780");
}
//-------------------------------------------------------------------------------------------------
void ActionStatusConditionDlg::setControlsFromSettings()
{
	try
	{
		string strActionName = m_settings.getAction();
		
		if (!strActionName.empty())
		{
			// Search for the action name from the settings
			int nSelection =
				m_comboFilesUnderAction.FindString(-1, strActionName.c_str());
			// Ensure the action name was found
			if (nSelection == CB_ERR)
			{
				UCLIDException ue("ELI33781", "Action no longer exists!");
				ue.addDebugInfo("Action Name", strActionName);
				throw ue;
			}
			// Select the specified action
			m_comboFilesUnderAction.SetCurSel(nSelection);
		}

		// Now set the status
		long nStatus = m_settings.getStatus();
		m_comboFilesUnderStatus.SetCurSel(nStatus);

		// If the status is skipped, select the appropriate user
		if (nStatus == kActionSkipped)
		{
			// Update the skipped users combo box
			fillSkippedUsers();

			// Enable the combo box
			m_comboSkippedUser.EnableWindow(TRUE);

			string strUser = m_settings.getUser();

			// Search for the specified user
			int nSelection = m_comboSkippedUser.FindString(-1, strUser.c_str());
			if (nSelection != CB_ERR)
			{
				m_comboSkippedUser.SetCurSel(nSelection);
			}
			else
			{
				m_comboSkippedUser.SetCurSel(0);
			}
		}

		// Since changes have been made, re-update the controls
		updateControls();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33782");
}
//-------------------------------------------------------------------------------------------------
void ActionStatusConditionDlg::fillSkippedUsers()
{
	try
	{
		// Clear current entries from the Combo box
		m_comboSkippedUser.ResetContent();

		// Add the any user string to the combo box
		m_comboSkippedUser.AddString(gstrANY_USER.c_str());

		CString zActionName;
		m_comboFilesUnderAction.GetWindowText(zActionName);

		// Query to get the users from the DB
		string strSQL = "SELECT DISTINCT [SkippedFile].[UserName] FROM [SkippedFile] INNER JOIN "
			"[Action] ON [SkippedFile].[ActionID] = [Action].[ID] WHERE [Action].[ASCName] = '";
		strSQL += (LPCTSTR) zActionName;
		strSQL += "' ORDER BY [SkippedFile].[UserName]";

		// Get the user list from the database
		ADODB::_RecordsetPtr ipRecords = m_ipFAMDB->GetResultsForQuery(strSQL.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI33783", ipRecords != __nullptr);

		// Loop through each result and add the user names to the vector
		while (ipRecords->adoEOF == VARIANT_FALSE)
		{
			// Get the user name and add it to the combo box
			string strName = getStringField(ipRecords->Fields, "UserName");
			m_comboSkippedUser.AddString(strName.c_str());

			// Increment counter and move to next record
			ipRecords->MoveNext();
		}

		// Set first item as current
		m_comboSkippedUser.SetCurSel(0);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33784");
}
//-------------------------------------------------------------------------------------------------