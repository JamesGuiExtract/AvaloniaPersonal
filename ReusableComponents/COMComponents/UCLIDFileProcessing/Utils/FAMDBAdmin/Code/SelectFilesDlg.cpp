// SelectFilesDlg.cpp : implementation file
//

#include "stdafx.h"
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
// CSelectFilesDlg dialog
//-------------------------------------------------------------------------------------------------
CSelectFilesDlg::CSelectFilesDlg(const IFileProcessingDBPtr& ipFAMDB,
								 const string& strSectionHeader, const string& strQueryHeader,
								 const SelectFileSettings& settings)
: CDialog(CSelectFilesDlg::IDD),
m_ipFAMDB(ipFAMDB),
m_strSectionHeader(strSectionHeader),
m_strQueryHeader(strQueryHeader),
m_settings(settings)
{
}
//-------------------------------------------------------------------------------------------------
CSelectFilesDlg::~CSelectFilesDlg()
{
	try
	{
		m_ipFAMDB = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27325");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CSelectFilesDlg)
	// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
	DDX_Control(pDX, IDC_GROUP_SELECT, m_grpSelectFor);
	DDX_Control(pDX, IDC_SLCT_FILE_QUERY_LABEL, m_lblQuery);
	DDX_Control(pDX, IDC_RADIO_ALL_FILES, m_radioAllFiles);
	DDX_Control(pDX, IDC_RADIO_FILES_UNDER_STATUS, m_radioFilesForWhich);
	DDX_Control(pDX, IDC_CMB_FILE_ACTION, m_comboFilesUnderAction);
	DDX_Control(pDX, IDC_CMB_FILE_STATUS, m_comboFilesUnderStatus);
	DDX_Control(pDX, IDC_CMB_FILE_SKIPPED_USER, m_comboSkippedUser);
	DDX_Control(pDX, IDC_RADIO_SQL_QUERY, m_radioFilesFromQuery);
	DDX_Control(pDX, IDC_EDIT_SQL_QUERY, m_editSelectQuery);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSelectFilesDlg, CDialog)
	//{{AFX_MSG_MAP(CSelectFilesDlg)
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_RADIO_ALL_FILES, &CSelectFilesDlg::OnClickedRadioAllFiles)
	ON_BN_CLICKED(IDC_RADIO_FILES_UNDER_STATUS, &CSelectFilesDlg::OnClickedRadioFilesStatus)
	ON_BN_CLICKED(IDC_RADIO_SQL_QUERY, &CSelectFilesDlg::OnClickedRadioFilesFromQuery)
	ON_BN_CLICKED(IDC_SELECT_BTN_OK, &CSelectFilesDlg::OnClickedOK)
	ON_BN_CLICKED(IDC_SELECT_BTN_CANCEL, &CSelectFilesDlg::OnClickedCancel)
	ON_CBN_SELCHANGE(IDC_CMB_FILE_ACTION, &CSelectFilesDlg::OnFilesUnderActionChange)
	ON_CBN_SELCHANGE(IDC_CMB_FILE_STATUS, &CSelectFilesDlg::OnFilesUnderStatusChange)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CSelectFilesDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CSelectFilesDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		// Set the group box caption
		m_grpSelectFor.SetWindowText(m_strSectionHeader.c_str());

		// Set the query box header
		m_lblQuery.SetWindowText(m_strQueryHeader.c_str());

		// Read all actions from the DB
		IStrToStrMapPtr ipMapActions = m_ipFAMDB->GetActions();
		ASSERT_RESOURCE_ALLOCATION("ELI26985", ipMapActions != NULL);

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

		// Select the all file reference radio button and new action status
		// radio button as default setting
		m_radioAllFiles.SetCheck(BST_CHECKED);

		// Update the controls
		updateControls();

		// Read the settings object and set the dialog based on the settings
		setControlsFromSettings();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26986")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnClickedRadioAllFiles()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update the controls
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26987")
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnClickedRadioFilesStatus()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update the controls
		updateControls();

		// Set focus to the action combo box under condition
		m_comboFilesUnderAction.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26988")
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnClickedRadioFilesFromQuery()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update the controls
		updateControls();

		// Set focus to the sql edit box
		m_editSelectQuery.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26989")
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnOK()
{
	// Stubbed in to prevent dialog closing when enter is pressed
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnCancel()
{
	// Stubbed in to prevent dialog closing when escape is pressed
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnClose()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	try
	{
		EndDialog(IDCLOSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26990");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnClickedCancel()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Call cancel
		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26991");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnClickedOK()
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26992");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnFilesUnderActionChange()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update the controls (this will also refill the skipped user combo box)
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26993");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::OnFilesUnderStatusChange()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update the controls
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26994");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool CSelectFilesDlg::saveSettings()
{
	try
	{
		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		// If choose to change that status for all the files
		if (m_radioAllFiles.GetCheck() == BST_CHECKED)
		{
			// Set the scope to all files
			m_settings.setScope(eAllFiles);
		}
		else if (m_radioFilesForWhich.GetCheck() == BST_CHECKED)
		{
			// Set the scope to all files which
			m_settings.setScope(eAllFilesForWhich);

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
		}
		else if (m_radioFilesFromQuery.GetCheck() == BST_CHECKED)
		{
			// Set the scope to all files from query
			m_settings.setScope(eAllFilesQuery);

			// Get the query from the edit box
			CString zTemp;
			m_editSelectQuery.GetWindowText(zTemp);
			if (zTemp.IsEmpty())
			{
				// Show error message to user
				MessageBox("Query may not be blank!", "Configuration Error",
					MB_OK | MB_ICONERROR);

				// Set focus to query
				m_editSelectQuery.SetFocus();

				// Return false
				return false;
			}
			m_settings.setSQLString((LPCTSTR) zTemp);
		}
		else
		{
			// We will never reach here
			THROW_LOGIC_ERROR_EXCEPTION("ELI26995");
		}

		return true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26996")
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::updateControls() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		BOOL bFilesForWhich = asMFCBool(m_radioFilesForWhich.GetCheck() == BST_CHECKED);
		BOOL bFilesFromSQL = asMFCBool(m_radioFilesFromQuery.GetCheck() == BST_CHECKED);

		// Enable the query edit box based on radio selection
		m_editSelectQuery.EnableWindow(bFilesFromSQL);

		// Enable the files under combo boxes based on radio selection
		// (set skipped user to disabled and only enable if files under status
		//	is enabled and is set to skipped - handled in If block)
		m_comboFilesUnderAction.EnableWindow(bFilesForWhich);
		m_comboFilesUnderStatus.EnableWindow(bFilesForWhich);
		m_comboSkippedUser.EnableWindow(FALSE);

		// Check if files for which is checked 
		if (bFilesForWhich == TRUE)
		{
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
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26997");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::setControlsFromSettings()
{
	try
	{
		int nSetAllFiles = BST_UNCHECKED;
		int nSetAllFileForWhich = BST_UNCHECKED;
		int nSetAllFilesQuery = BST_UNCHECKED;

		// Check which scope is selected and set the appropriate controls
		switch(m_settings.getScope())
		{
		case eAllFiles:
			nSetAllFiles = BST_CHECKED;
			break;

		case eAllFilesForWhich:
			{
				nSetAllFileForWhich = BST_CHECKED;

				// Search for the action name from the settings
				int nSelection =
					m_comboFilesUnderAction.FindString(-1, m_settings.getAction().c_str());
				// Ensure the action name was found
				if (nSelection == CB_ERR)
				{
					UCLIDException ue("ELI26998", "Action no longer exists!");
					ue.addDebugInfo("Action Name", m_settings.getAction());
					throw ue;
				}
				// Select the specified action
				m_comboFilesUnderAction.SetCurSel(nSelection);

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

					// Search for the specified user
					nSelection =
						m_comboSkippedUser.FindString(-1, m_settings.getUser().c_str());
					if (nSelection != CB_ERR)
					{
						m_comboSkippedUser.SetCurSel(nSelection);
					}
					else
					{
						m_comboSkippedUser.SetCurSel(0);
					}
				}
			}
			break;

		case eAllFilesQuery:
			{
				nSetAllFilesQuery = BST_CHECKED;
				m_editSelectQuery.SetWindowText(m_settings.getSQLString().c_str());
			}
			break;

		case eAllFilesTag:
			// NOT IMPLEMENTED CURRENTLY
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI26999");
		}

		// Set the radio buttons
		m_radioAllFiles.SetCheck(nSetAllFiles);
		m_radioFilesForWhich.SetCheck(nSetAllFileForWhich);
		m_radioFilesFromQuery.SetCheck(nSetAllFilesQuery);

		// Since changes have been made, re-update the controls
		updateControls();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27000");
}
//-------------------------------------------------------------------------------------------------
void CSelectFilesDlg::fillSkippedUsers()
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
		ASSERT_RESOURCE_ALLOCATION("ELI27001", ipRecords != NULL);

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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27002");
}
//-------------------------------------------------------------------------------------------------
