// SelectActionDlg.cpp : implementation file
//

#include "stdafx.h"
#include "SelectActionDlg.h"
#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <ComUtils.h>
#include <cpputil.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const std::string strSELECT_ACTION = "Select Action";
const std::string strSELECT_ACTION_LABEL = "Select an action to be used for processing";
const std::string strREMOVE_ACTION = "Remove Action";
const std::string strREMOVE_ACTION_LABEL = "Select action to remove from database";

//-------------------------------------------------------------------------------------------------
// SelectActionDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SelectActionDlg, CDialog)
//-------------------------------------------------------------------------------------------------
SelectActionDlg::SelectActionDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFPMDB,
									   string strCaption, string strAction, CWnd* pParent)
: CDialog(SelectActionDlg::IDD, pParent),
m_ipFPMDB(ipFPMDB),
m_strSelectedActionName(""),
m_dwSelectedActionID(0)
{
	// Get the caption and current action used in FPM
	m_strCaption = strCaption;
	m_strPrevAction = strAction;
}
//-------------------------------------------------------------------------------------------------
SelectActionDlg::~SelectActionDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16536");
}
//-------------------------------------------------------------------------------------------------
void SelectActionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_CMB_ACTION, m_CMBAction);
	DDX_Control(pDX, IDC_STATICINFO, m_StaticLabel);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(SelectActionDlg, CDialog)
	ON_CBN_SELCHANGE(IDC_CMB_ACTION, &SelectActionDlg::OnCbnSelchangeCmbAction)
	ON_BN_CLICKED(IDOK, &SelectActionDlg::OnBnClickOK)
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
			m_StaticLabel.SetWindowText(strSELECT_ACTION_LABEL.c_str());
		}
		else if (m_strCaption == strREMOVE_ACTION)
		{
			m_StaticLabel.SetWindowText(strREMOVE_ACTION_LABEL.c_str());
		}

		// display a wait-cursor because we are getting information from the DB, which may take a few seconds
		CWaitCursor wait;

		// Read all actions from the DB
		IStrToStrMapPtr pMapActions = m_ipFPMDB->GetActions();

		if (pMapActions->GetSize() > 0)
		{
			// A integer used to memories which item is the current action
			int iCurActionIndex = 0;

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
				// Set the index of the item inside the Combobox same as the ID of the action
				m_CMBAction.SetItemData(nIndex, dwID);

				// If the action is the current action used for FPM
				if (strAction == m_strPrevAction)
				{
					// Set dwCurrentAction
					iCurActionIndex = nIndex;
				}
			}
			
			// Set the default item in the Copy From ComboBox to the 
			// action now used in FPM, if there is no, set to the firt action
			m_CMBAction.SetCurSel(iCurActionIndex);


			// Set the current action ID to be the same as the current action
			// used in FPM, if there is no action in FPM, set it to the first action's ID
			m_dwSelectedActionID = m_CMBAction.GetItemData(iCurActionIndex);

			// Set the current action name to be the same as the current action
			// used in FPM, if there is no action in FPM, set it to the first action
			CString	zCurrentAction;
			m_CMBAction.GetLBText(iCurActionIndex, zCurrentAction);
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
		m_dwSelectedActionID = m_CMBAction.GetItemData(m_CMBAction.GetCurSel());

		// Update the Selected action name when select another action
		CString	zCurrentAction;
		m_CMBAction.GetLBText(m_CMBAction.GetCurSel(), zCurrentAction);
		m_strSelectedActionName = string((LPCTSTR)zCurrentAction);
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
// Private Methods
//-------------------------------------------------------------------------------------------------