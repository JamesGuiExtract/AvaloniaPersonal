// FileProcessingAddActionDlg.cpp : implementation file
//

#include "stdafx.h"
#include "FileProcessingAddActionDlg.h"
#include <ComUtils.h>
#include <cpputil.h>
#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// FileProcessingAddActionDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(FileProcessingAddActionDlg, CDialog)
//-------------------------------------------------------------------------------------------------
FileProcessingAddActionDlg::FileProcessingAddActionDlg(
						UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFPMDB,
						CWnd* pParent /*=NULL*/)
: CDialog(FileProcessingAddActionDlg::IDD, pParent),
m_iDefaultStatus(0),
m_ipFPMDB(ipFPMDB)
{

}
//-------------------------------------------------------------------------------------------------
FileProcessingAddActionDlg::~FileProcessingAddActionDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16552");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingAddActionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_RDSETTO, m_btnSetToStatus);
	DDX_Control(pDX, IDC_RDCOPYFROM, m_btnCopyFromStatus);
	DDX_Control(pDX, IDC_EDT_ACTION_NAME, m_edActionName);
	DDX_Control(pDX, IDC_CMB_STATUS, m_cmbStatus);
	DDX_Control(pDX, IDC_CMB_COPY_FROM, m_cmbCopyStatus);
	DDX_Text(pDX, IDC_EDT_ACTION_NAME, m_zActionName);
	//{{AFX_DATA_MAP(FileProcessingAddActionDlg)
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileProcessingAddActionDlg, CDialog)
	//{{AFX_MSG_MAP(FileProcessingAddActionDlg)
	ON_BN_CLICKED(IDOK, &FileProcessingAddActionDlg::OnBnClickOK)
	ON_BN_CLICKED(IDC_RDSETTO, &FileProcessingAddActionDlg::OnBnClickedRdsetto)
	ON_BN_CLICKED(IDC_RDCOPYFROM, &FileProcessingAddActionDlg::OnBnClickedRdcopyfrom)
	ON_CBN_SELCHANGE(IDC_CMB_STATUS, &FileProcessingAddActionDlg::OnCbnSelchangeCmbStatus)
	ON_CBN_SELCHANGE(IDC_CMB_COPY_FROM, &FileProcessingAddActionDlg::OnCbnSelchangeCmbCopyFrom)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// FileProcessingAddActionDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingAddActionDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();

		// Select the text in the action name text box
		m_edActionName.SetSel(0, -1);

		// Check the set to status ComboBox
		m_btnSetToStatus.SetCheck(BST_CHECKED);

		// display a wait-cursor because we are getting information from the DB, which may take a few seconds
		CWaitCursor wait;

		// Insert the action status to the ComboBox
		// The items are inserted the same order as the EActionStatus
		m_cmbStatus.InsertString(0, "Unattempted");
		m_cmbStatus.InsertString(1, "Pending");
		m_cmbStatus.InsertString(2, "Processing");
		m_cmbStatus.InsertString(3, "Completed");
		m_cmbStatus.InsertString(4, "Failed");
		m_cmbStatus.InsertString(5, "Skipped");

		// Set the default item in the status ComboBox to "Unattempted"
		m_cmbStatus.SetCurSel(0);

		// Disable the Copy From ComboBox
		m_cmbCopyStatus.EnableWindow(FALSE);

		// Set the default status to "Unattempted"
		m_iDefaultStatus = 0;

		// Read all actions from the DB
		IStrToStrMapPtr pMapActions = m_ipFPMDB->GetActions();

		// Insert actions into ComboBox
		for (int i = 0; i < pMapActions->GetSize(); i++)
		{
			// Get one actions' name and ID inside the database
			CComBSTR bstrKey, bstrValue;
			pMapActions->GetKeyValue(i, &bstrKey, &bstrValue);
			string strAction = asString(bstrKey);
			DWORD nID = asUnsignedLong(asString(bstrValue));

			// Insert the action name into ComboBox
			int nIndex = m_cmbCopyStatus.InsertString(-1, strAction.c_str());
			// Set the index of the item inside the Combobox same as the ID of the action
			m_cmbCopyStatus.SetItemData(nIndex, nID);
		}
		
		// Set the default item in the Copy From ComboBox to the first action
		m_cmbCopyStatus.SetCurSel(0);
		// Set the focus to the edit box
		m_edActionName.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14089")

	return FALSE;
}

//-------------------------------------------------------------------------------------------------
//Public Methods
//-------------------------------------------------------------------------------------------------
CString FileProcessingAddActionDlg::GetActionName()
{
	// Return the newly added action name
	return m_zActionName;
}
//-------------------------------------------------------------------------------------------------
int FileProcessingAddActionDlg::GetDefaultStatus()
{
	// Return the default status for the new action
	return m_iDefaultStatus;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingAddActionDlg::OnBnClickOK()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Check if the action edit box is empty
		if (m_edActionName.GetWindowTextLength() == 0)
		{
			// Remind the user to enter the name of the new action
			MessageBox("Please specify a name for the new action.", "Add Action", MB_ICONINFORMATION);

			// Set the focus to the edit box
			m_edActionName.SetFocus();
			return;
		}

		// Validate the new action name
		CString zText;
		m_edActionName.GetWindowTextA( zText );
		string strNewName = (LPCTSTR)zText;
		if (!isValidIdentifier( strNewName ))
		{
			// Invalid identifier, throw exception
			UCLIDException ue( "ELI14770", "Invalid name for new action!" );
			ue.addDebugInfo( "Invalid Name", strNewName );
			ue.addDebugInfo( "Valid Pattern", "[_a-zA-Z]+[_a-zA-Z0-9]*" );
			throw ue;
		}
		
		// Confirm if we really want to add this action to the database
		string strPrompt = "Do you really want to add '";
		strPrompt += strNewName.c_str();
		strPrompt += "' to the database?";
		int iResult = MessageBox( strPrompt.c_str(), "Confirmation", MB_YESNOCANCEL );
		if (iResult == IDCANCEL || iResult == IDNO)
		{
			// Return to the dialog if we don't want to add this action.
			return;
		}

		// New action is valid, add it to the database
		OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14082");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingAddActionDlg::OnBnClickedRdsetto()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Enable the Status ComboBox if it is not enabled
		if (!m_cmbStatus.IsWindowEnabled())
		{
			m_cmbStatus.EnableWindow(TRUE);
		}

		// Get the Current selected default status
		m_iDefaultStatus = m_cmbStatus.GetCurSel();
		// Disable Copy Status From ComboBox
		m_cmbCopyStatus.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14090")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingAddActionDlg::OnBnClickedRdcopyfrom()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// If the call to OnBnClickedRdcopyfrom() is caused by SetCheck() method
		// Simply return if it is set to unchecked (see the comment 12 lines below)
		if (m_btnCopyFromStatus.GetCheck() == BST_UNCHECKED)
		{
			return;
		}

		// Check if there is an existing action that can copy status from
		if (m_cmbCopyStatus.GetCount() == 0)
		{
			// Prompt to the user that there is no action in the database to copy 
			// the status from
			MessageBox(
				"There must be at least one action in the database from which to copy status!", 
				"Error", MB_ICONINFORMATION);

			// Uncheck the CopyFromStatus radio button
			// which will cause another call to OnBnClickedRdcopyfrom() and it will be ignored.
			m_btnCopyFromStatus.SetCheck(BST_UNCHECKED);

			// Set back to the select status radio button
			m_btnSetToStatus.SetCheck(BST_CHECKED);
			return;
		}

		// Enable the Copy Status From ComboBox
		m_cmbCopyStatus.EnableWindow(TRUE);

		// Set the default status to -1, so that the FileProcessingDlg will 
		// notice and handle this situation
		m_iDefaultStatus = -1;
		// Disable the Status ComboBox
		m_cmbStatus.EnableWindow(FALSE);

		int iIndex = m_cmbCopyStatus.GetCurSel();
		m_iActionID = m_cmbCopyStatus.GetItemData(iIndex);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14091")
}
//-------------------------------------------------------------------------------------------------
DWORD FileProcessingAddActionDlg::GetCopyStatusActionID()
{
	return m_iActionID;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingAddActionDlg::OnCbnSelchangeCmbStatus()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Get the default action status 
		m_iDefaultStatus = m_cmbStatus.GetCurSel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14093")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingAddActionDlg::OnCbnSelchangeCmbCopyFrom()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Get the action ID from which we will copy the status
		int iIndex = m_cmbCopyStatus.GetCurSel();
		m_iActionID = m_cmbCopyStatus.GetItemData(iIndex);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14095")
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
