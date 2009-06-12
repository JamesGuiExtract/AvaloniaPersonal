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
	DDX_Control(pDX, IDC_CMB_ACTION_NAMES, m_CMBAction);
	DDX_Control(pDX, IDC_EDT_NEW_ACTION, m_EditNewAction);
	DDX_Text(pDX, IDC_EDT_NEW_ACTION, m_zNewActionName);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(RenameActionDlg, CDialog)
	ON_CBN_SELCHANGE(IDC_CMB_ACTION_NAMES, &RenameActionDlg::OnCbnSelchangeCmbActionNames)
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
		m_EditNewAction.SetWindowText("");

		// Set focus to new name edit box
		m_EditNewAction.SetFocus();

		// Read all actions from the DB
		IStrToStrMapPtr pMapActions = m_ipFPMDB->GetActions();

		if (pMapActions->GetSize() > 0)
		{
			// Insert actions into ComboBox
			for (int i = 0; i < pMapActions->GetSize(); i++)
			{
				// Bstr string to hold the name and ID of one action
				CComBSTR bstrKey, bstrValue;

				// Get one actions' name and ID inside the database
				pMapActions->GetKeyValue(i, &bstrKey, &bstrValue);
				string strAction = asString(bstrKey);
				DWORD dwID = asUnsignedLong(asString(bstrValue));

				// Insert the action name into ComboBox
				int nIndex = m_CMBAction.InsertString(-1, strAction.c_str());
				// Set the item data of the new item to the ID of the action
				m_CMBAction.SetItemData(nIndex, dwID);
			}
			
			// Set the default item to the first action
			m_CMBAction.SetCurSel(0);

			// Select the first action
			m_dwSelectedActionID = m_CMBAction.GetItemData(0);
			m_CMBAction.GetLBText(0, m_zSelectedActionName);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14191")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void RenameActionDlg::OnCbnSelchangeCmbActionNames()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Update the Selected action ID when select another action
		m_dwSelectedActionID = m_CMBAction.GetItemData(m_CMBAction.GetCurSel());

		// Update the Selected action name when select another action
		m_CMBAction.GetLBText(m_CMBAction.GetCurSel(), m_zSelectedActionName);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14192")	
}
//-------------------------------------------------------------------------------------------------
void RenameActionDlg::OnBnClickedOK()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Retrieve latest text
		UpdateData( TRUE );

		// Confirm an action is specified for renaming
		if (m_zSelectedActionName.IsEmpty())
		{
			MessageBox("An action must be specified before it can be renamed.", "Error", 
				MB_OK|MB_ICONINFORMATION);

			// Set the focus to the action combo box
			m_CMBAction.SetFocus();
			return;
		}

		// Confirm non-empty name
		if (m_zNewActionName.IsEmpty())
		{
			MessageBox("The new name for the selected action is empty.", "Error", 
				MB_OK|MB_ICONINFORMATION);

			// Set the focus to the edit box
			m_EditNewAction.SetFocus();
			return;
		}

		// Confirm that the name actually changes (P13 #3926)
		if (m_zNewActionName == m_zSelectedActionName)
		{
			MessageBox("The new name for the selected action cannot match the old name.", 
				"Error", MB_OK|MB_ICONINFORMATION );

			// Set the focus to the edit box
			m_EditNewAction.SetFocus();
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
			m_EditNewAction.SetSel( 0, -1 );
			m_EditNewAction.SetFocus();
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
			m_EditNewAction.SetSel(0, -1);
			m_EditNewAction.SetFocus();
			throw ue;
		}

		// Confirm the rename
		if (m_zNewActionName != m_zSelectedActionName)
		{
			CString zText;
			zText.Format( "Do you really want to replace '%s' with '%s'?", 
				m_zSelectedActionName, m_zNewActionName );
			int nRes = MessageBox( zText, "Confirm Rename", MB_YESNOCANCEL);
			if (nRes == IDCANCEL)
			{
				return;
			}
			else if (nRes == IDNO)
			{
				// Reset the name back to the current name
				m_zNewActionName = m_zSelectedActionName;
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
DWORD RenameActionDlg::GetOldNameAndNewName(string& strOld, string& strNew)
{
	strOld = std::string((LPCTSTR) m_zSelectedActionName);
	strNew = std::string((LPCTSTR) m_zNewActionName);

	// Return the ID
	return m_dwSelectedActionID;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------