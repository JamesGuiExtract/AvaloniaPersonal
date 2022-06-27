#include "StdAfx.h"
#include "ActionStatusConditionDlg.h"
#include "FileProcessingUtils.h"

#include <UCLIDException.h>
#include <ComUtils.h>
#include <ADOUtils.h>
#include <FAMHelperFunctions.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// ActionStatusConditionDlg dialog
//-------------------------------------------------------------------------------------------------
ActionStatusConditionDlg::ActionStatusConditionDlg(
										const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB)
: CDialog(ActionStatusConditionDlg::IDD),
m_ipFAMDB(ipFAMDB)
{
}
//-------------------------------------------------------------------------------------------------
ActionStatusConditionDlg::ActionStatusConditionDlg(
										const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
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
	DDX_Control(pDX, IDC_CMB_FILE_USER, m_comboUser);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(ActionStatusConditionDlg, CDialog)
	//{{AFX_MSG_MAP(ActionStatusConditionDlg)
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_SELECT_BTN_OK, &ActionStatusConditionDlg::OnClickedOK)
	ON_BN_CLICKED(IDC_SELECT_BTN_CANCEL, &ActionStatusConditionDlg::OnClickedCancel)
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
		fillComboBoxFromMap(m_comboFilesUnderAction, ipMapActions);
		
		// Set the status items into combo box
		CFileProcessingUtils::addStatusInComboBox(m_comboFilesUnderStatus);

		// Set the initial status to Pending
		m_comboFilesUnderStatus.SetCurSel(1);

		fillByUserWithFAMUsers();

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

		// Get the status ID for the action from which we will copy the status to the selected action
		int iFromStatusID = m_comboFilesUnderStatus.GetCurSel();
		m_comboFilesUnderStatus.GetWindowText(zTemp);
		m_settings.setStatus(iFromStatusID);
		m_settings.setStatusString((LPCTSTR) zTemp);

		// Get the user name from the combo box
		m_comboUser.GetWindowText(zTemp);
		m_settings.setUser((LPCTSTR) zTemp);

		return true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33779")
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

		string strUser = m_settings.getUser();

		// Search for the specified user
		int nSelection = m_comboUser.FindString(-1, strUser.c_str());
		if (nSelection != CB_ERR)
		{
			m_comboUser.SetCurSel(nSelection);
		}
		else
		{
			m_comboUser.SetCurSel(0);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33782");
}
//-------------------------------------------------------------------------------------------------
void ActionStatusConditionDlg::fillByUserWithFAMUsers()
{
	try
	{
		// Clear current entries from the Combo box
		m_comboUser.ResetContent();

		// Add the any user string to the combo box 
		m_comboUser.AddString(gstrANY_USER.c_str());
		m_comboUser.AddString(gstrNO_USER.c_str());

		CString zActionName;
		m_comboFilesUnderAction.GetWindowText(zActionName);

		// Query to get the users from the DB
		string strSQL = "SELECT [UserName] FROM [FAMUser]  "
			"ORDER BY [UserName]";

		fillComboBoxFromDB(m_comboUser, strSQL, "UserName");
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI53352");
}
//-------------------------------------------------------------------------------------------------
void ActionStatusConditionDlg::fillComboBoxFromDB(CComboBox &rCombo, string strQuery, string fieldName)
{
	try
	{
		// Get the list from the database
		ADODB::_RecordsetPtr ipRecords = m_ipFAMDB->GetResultsForQuery(strQuery.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI53353", ipRecords != __nullptr);

		// Loop through each result and add the field value to the combo
		while (ipRecords->adoEOF == VARIANT_FALSE)
		{
			string strName = getStringField(ipRecords->Fields, fieldName);
			rCombo.AddString(strName.c_str());

			// Increment counter and move to next record
			ipRecords->MoveNext();
		}

		// Set first item as current
		rCombo.SetCurSel(0);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI53354");
}
//-------------------------------------------------------------------------------------------------
