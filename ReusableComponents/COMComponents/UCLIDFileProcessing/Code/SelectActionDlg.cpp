// SelectActionDlg.cpp : implementation file
//

#include "stdafx.h"
#include "SelectActionDlg.h"
#include "FileProcessingUtils.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <ComUtils.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string strSELECT_ACTION = "Select Action";
const string strSELECT_ACTION_LABEL = "Select an action to be used for processing";
const string strREMOVE_ACTION = "Remove Action";
const string strREMOVE_ACTION_LABEL = "Select action to remove from database";

//-------------------------------------------------------------------------------------------------
// SelectActionDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SelectActionDlg, CDialog)
//-------------------------------------------------------------------------------------------------
SelectActionDlg::SelectActionDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFPMDB,
	string strCaption, string strAction, bool bAllowTags, CWnd* pParent)
: CDialog(SelectActionDlg::IDD, pParent),
m_strCaption(strCaption),
m_strPrevAction(strAction),
m_bAllowTags(bAllowTags),
m_strSelectedActionName(""),
m_dwSelectedActionID(0),
m_dwActionSel(0),
m_ipFPMDB(ipFPMDB)
{
	ASSERT_RESOURCE_ALLOCATION("ELI29107", m_ipFPMDB != NULL);
}
//-------------------------------------------------------------------------------------------------
SelectActionDlg::~SelectActionDlg()
{
	try
	{
		m_ipFPMDB = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16536");
}
//-------------------------------------------------------------------------------------------------
void SelectActionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_CMB_ACTION, m_cmbAction);
	DDX_Control(pDX, IDC_STATICINFO, m_staticLabel);
	DDX_Control(pDX, IDC_BTN_ACTION_TAG, m_btnActionTag);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(SelectActionDlg, CDialog)
	ON_CBN_SELCHANGE(IDC_CMB_ACTION, &SelectActionDlg::OnCbnSelchangeCmbAction)
	ON_BN_CLICKED(IDOK, &SelectActionDlg::OnBnClickOK)
	ON_BN_CLICKED(IDC_BTN_ACTION_TAG, &SelectActionDlg::OnBnClickActionTag)
	ON_CBN_SELENDCANCEL(IDC_CMB_ACTION, &SelectActionDlg::OnCbnSelEndCancel)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// SelectActionDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL SelectActionDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CDialog::OnInitDialog();

		// Set the caption of the dialog
		SetWindowText(m_strCaption.c_str());

		// Set the Label text according to different caption
		if (m_strCaption == strSELECT_ACTION)
		{
			m_staticLabel.SetWindowText(strSELECT_ACTION_LABEL.c_str());
		}
		else if (m_strCaption == strREMOVE_ACTION)
		{
			m_staticLabel.SetWindowText(strREMOVE_ACTION_LABEL.c_str());
		}

		// Check if tags are allowed
		if (m_bAllowTags)
		{
			// Prepare the action tag button
			m_btnActionTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

			// Make the combo box editable
			makeDropListEditable(m_cmbAction);
		}
		else
		{
			// Hide the action tag button
			m_btnActionTag.ShowWindow(SW_HIDE);

			// Expand the combo box into the empty space
			CRect rect;
			m_cmbAction.GetWindowRect(&rect);
			ScreenToClient(rect);
			rect.right += 18;
			m_cmbAction.MoveWindow(&rect);
		}

		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		// Read all actions from the DB
		IStrToStrMapPtr ipMapActions = m_ipFPMDB->GetActions();
		ASSERT_RESOURCE_ALLOCATION("ELI29106", ipMapActions != NULL);

		int iSize = ipMapActions->Size;
		if (iSize > 0)
		{
			// An integer used to remember which item is the current action
			int iCurActionIndex = 0;

			// Insert actions into ComboBox
			for (int i = 0; i < iSize; i++)
			{
				// Bstr string to hold the name and ID of one action
				CComBSTR bstrKey, bstrValue;

				// Get one actions' name and ID inside the database
				ipMapActions->GetKeyValue(i, &bstrKey, &bstrValue);
				string strAction = asString(bstrKey);
				DWORD dwID = asUnsignedLong(asString(bstrValue));

				// Insert the action name into ComboBox
				int nIndex = m_cmbAction.InsertString(-1, strAction.c_str());

				// Set the index of the item inside the Combobox same as the ID of the action
				m_cmbAction.SetItemData(nIndex, dwID);

				// If the action is the current action used for FPM
				if (strAction == m_strPrevAction)
				{
					// Set dwCurrentAction
					iCurActionIndex = nIndex;
				}
			}
			
			// Set the default item in the Copy From ComboBox to the 
			// action now used in FPM, if there is no, set to the firt action
			m_cmbAction.SetCurSel(iCurActionIndex);

			// Set the current action ID to be the same as the current action
			// used in FPM, if there is no action in FPM, set it to the first action's ID
			m_dwSelectedActionID = m_cmbAction.GetItemData(iCurActionIndex);

			// Set the current action name to be the same as the current action
			// used in FPM, if there is no action in FPM, set it to the first action
			CString	zCurrentAction;
			m_cmbAction.GetLBText(iCurActionIndex, zCurrentAction);
			m_strSelectedActionName = string((LPCTSTR)zCurrentAction);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14032")

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void SelectActionDlg::OnCbnSelchangeCmbAction()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Update the Selected action ID when select another action
		int iCurSel = m_cmbAction.GetCurSel();
		m_dwSelectedActionID = iCurSel == CB_ERR ? -1 : m_cmbAction.GetItemData(iCurSel);

		// Update the Selected action name when select another action
		m_strSelectedActionName = getActionName();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14106")
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
void SelectActionDlg::GetSelectedAction(string& strAction, DWORD& iID)
{
	// Set the current action that has been selected
	 strAction = m_strSelectedActionName;
	 iID = m_dwSelectedActionID;
}
//-------------------------------------------------------------------------------------------------
void SelectActionDlg::OnBnClickOK()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Update the selected action name
		OnCbnSelchangeCmbAction();

		// If no action has been selected
		if (m_strSelectedActionName == "")
		{
			OnOK();
		}

		if (m_strCaption == strSELECT_ACTION && m_strSelectedActionName != m_strPrevAction 
			&& m_strPrevAction != "")
		{
			// Create the prompt for the message box
			string strPrompt = "Replace action '" + m_strPrevAction + "' with '" + m_strSelectedActionName +"'?";

			int iResult = MessageBox(strPrompt.c_str(), 
				"Confirmation", MB_YESNOCANCEL);
			// If Cancel is clicked, go back to SelectActionDlg
			if (iResult == IDCANCEL)
			{
				return;
			}
			// If No is clicked, reset the current action to the one used in FPM
			else if (iResult == IDNO)
			{
				m_strSelectedActionName = m_strPrevAction;
			}
		}

		// The dialog is used as Remove action
		if (m_strCaption == strREMOVE_ACTION)
		{
			// Create the prompt for the message box
			string strPrompt;
			if (m_strSelectedActionName == m_strPrevAction)
			{
				strPrompt += "Action to be removed is the current action. Removing it will cause the\n" 
					"File Action Manager to reset the Action tab and hide all the other tabs. \n\n";
			}
			strPrompt += "Remove the action '" + m_strSelectedActionName +
				"' from the database?";

			int iResult = MessageBox(strPrompt.c_str(), 
				"Confirmation", MB_YESNOCANCEL);
			// If Cancel is clicked, go back to SelectActionDlg
			if (iResult == IDCANCEL)
			{
				return;
			}
			// If No is clicked, do nothing and exit the dialog
			else if (iResult == IDNO)
			{
				m_strSelectedActionName = "";
			}
			else
			{
				// Create the prompt for the message box
				string strPrompt = "Do you really want to remove the action '" + m_strSelectedActionName +"' from the database? \n";

				int iResult = MessageBox(strPrompt.c_str(), 
					"Final Confirmation", MB_YESNO);
				if (iResult == IDNO)
				{
					m_strSelectedActionName = "";
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14094")

	OnOK();
}
//-------------------------------------------------------------------------------------------------
void SelectActionDlg::OnBnClickActionTag()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Retrieve position of doc tag button
		CRect rect;
		m_btnActionTag.GetWindowRect(&rect);
		
		// Display menu and make selection
		string strChoice = CFileProcessingUtils::ChooseDocTag(m_hWnd, rect.right, rect.top, false);

		// Replace text if selection was made
		if (strChoice != "")
		{
			// Replace the previously selected combobox text with the selected tag
			int iStart = LOWORD(m_dwActionSel);
			int iEnd = HIWORD(m_dwActionSel);

			string strText = getActionName();

			string strResult = strText.substr(0, iStart) + strChoice + strText.substr(iEnd);
			m_cmbAction.SetWindowText(strResult.c_str());

			// Reset the selection
			m_dwActionSel = MAKELONG(strResult.length(), strResult.length());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29111")
}
//-------------------------------------------------------------------------------------------------
void SelectActionDlg::OnCbnSelEndCancel()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		m_dwActionSel = m_cmbAction.GetEditSel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29113")
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
string SelectActionDlg::getActionName()
{
	CString zText;
	m_cmbAction.GetWindowText(zText);
	return (LPCTSTR)zText;
}
//-------------------------------------------------------------------------------------------------
void SelectActionDlg::makeDropListEditable(CComboBox& cmbDropList)
{
	// Get the previous settings
	DWORD dwStyle = cmbDropList.GetStyle();
	DWORD dwExStyle = cmbDropList.GetExStyle();
	int nID = cmbDropList.GetDlgCtrlID();
	CWnd *pPrevWindow = cmbDropList.GetNextWindow(GW_HWNDPREV);
	CWnd *pParentWnd = cmbDropList.GetParent();
	CFont *pFont = cmbDropList.GetFont();
	CRect rect;
	cmbDropList.GetWindowRect(rect);
	pParentWnd->ScreenToClient(rect);

	// Change the style from dropdown list to dropdown
	dwStyle &= ~CBS_DROPDOWNLIST;
	dwStyle |= CBS_DROPDOWN;

	// Destroy the previous combobox
	cmbDropList.DestroyWindow();

	// Create a new one in its place
	cmbDropList.CreateEx(dwExStyle, "COMBOBOX", "", dwStyle, rect, pParentWnd, nID);
	cmbDropList.SetFont(pFont, FALSE);
	cmbDropList.SetWindowPos(pPrevWindow, 0, 0, 0, 0, SWP_NOMOVE|SWP_NOSIZE);
}
//-------------------------------------------------------------------------------------------------