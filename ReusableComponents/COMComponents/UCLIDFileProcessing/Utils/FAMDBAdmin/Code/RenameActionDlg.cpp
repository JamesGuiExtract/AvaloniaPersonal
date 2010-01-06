// RenameActionDlg.cpp : implementation file
//

#include "stdafx.h"
#include "RenameActionDlg.h"
#include <UCLIDException.h>
#include <ComUtils.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// RenameActionDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(RenameActionDlg, CDialog)
//-------------------------------------------------------------------------------------------------
RenameActionDlg::RenameActionDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFPMDB,
								 CWnd* pParent /*=NULL*/)
: CDialog(RenameActionDlg::IDD, pParent),
m_ipFPMDB(ipFPMDB)
{

}
//-------------------------------------------------------------------------------------------------
RenameActionDlg::~RenameActionDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16554");
}
//-------------------------------------------------------------------------------------------------
void RenameActionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_EDIT_OLD_ACTION_NAME, m_editOldAction);
	DDX_Control(pDX, IDC_EDT_NEW_ACTION, m_editNewAction);
	DDX_Text(pDX, IDC_EDT_NEW_ACTION, m_zNewActionName);
	DDX_Text(pDX, IDC_EDIT_OLD_ACTION_NAME, m_zOldActionName);
}

//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(RenameActionDlg, CDialog)
	ON_BN_CLICKED(IDOK, &RenameActionDlg::OnBnClickedOK)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// RenameActionDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL RenameActionDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();

		// Clear the new name edit box
		m_editNewAction.SetWindowText("");

		// Set focus to new name edit box
		m_editNewAction.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14191")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void RenameActionDlg::OnBnClickedOK()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Retrieve latest text
		UpdateData( TRUE );

		// Confirm non-empty name
		if (m_zNewActionName.IsEmpty())
		{
			MessageBox("The new name for the selected action is empty.", "Error", 
				MB_OK|MB_ICONINFORMATION);

			// Set the focus to the edit box
			m_editNewAction.SetFocus();
			return;
		}

		// Confirm that the name actually changes (P13 #3926)
		if (m_zNewActionName == m_zOldActionName)
		{
			MessageBox("The new name for the selected action cannot match the old name.", 
				"Error", MB_OK|MB_ICONINFORMATION );

			// Set the focus to the edit box
			m_editNewAction.SetFocus();
			return;
		}

		// Confirm that the new name is not already present in the DB (P13 #3925)
		bool bFoundNewName = false;
		try
		{
			long lExistingID = m_ipFPMDB->GetActionID( (LPCTSTR) m_zNewActionName );
			bFoundNewName = true;
		}
		catch (...)
		{
			// Catch and ignore the exception thrown because the new name 
			// does not have an Action ID in the database
		}

		if (bFoundNewName)
		{
			// Invalid action name, throw exception
			UCLIDException ue( "ELI15130", "The new action name is already present in the database." );
			ue.addDebugInfo( "New Name", (LPCTSTR) m_zNewActionName );

			// Set the focus to the edit box
			m_editNewAction.SetSel( 0, -1 );
			m_editNewAction.SetFocus();
			throw ue;
		}

		// Validate the new name of the action (P13 #4066)
		string strNewName = m_zNewActionName.operator LPCTSTR();
		if (!isValidIdentifier( strNewName ))
		{
			// Invalid identifier, throw exception
			UCLIDException ue( "ELI15040", "The new name for the selected action is invalid." );
			ue.addDebugInfo( "Invalid Name", strNewName );
			ue.addDebugInfo( "Valid Pattern", "_*[a-zA-Z][a-zA-Z0-9]*" );

			// Set the focus to the edit box
			m_editNewAction.SetSel(0, -1);
			m_editNewAction.SetFocus();
			throw ue;
		}

		// Confirm the rename
		if (m_zNewActionName != m_zOldActionName)
		{
			CString zText;
			zText.Format( "Do you really want to replace '%s' with '%s'?", 
				m_zOldActionName, m_zNewActionName );
			int nRes = MessageBox( zText, "Confirm Rename", MB_YESNOCANCEL);
			if (nRes == IDCANCEL)
			{
				return;
			}
			else if (nRes == IDNO)
			{
				// Reset the name back to the current name
				m_zNewActionName = m_zOldActionName;
				UpdateData(FALSE);
			}
		}
		
		OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14193");	
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
void RenameActionDlg::GetNewActionName(string& strNew)
{
	strNew = std::string((LPCTSTR) m_zNewActionName);
}
//-------------------------------------------------------------------------------------------------
void RenameActionDlg::SetOldActionName(const string& strOld)
{
	m_zOldActionName = strOld.c_str();
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
