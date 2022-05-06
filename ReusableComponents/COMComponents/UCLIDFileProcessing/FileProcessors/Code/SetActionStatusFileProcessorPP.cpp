
#include "stdafx.h"
#include "SetActionStatusFileProcessorPP.h"
#include "FileProcessorsUtils.h"
#include "LoadFileDlgThread.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <DocTagUtils.h>
#include <FAMUtilsConstants.h>

// other constants
const int giNUM_VALID_STATUSES = 5;
const EActionStatus gVALID_ACTION_STATUSES[giNUM_VALID_STATUSES] = {kActionUnattempted,
        kActionPending, kActionCompleted, kActionFailed, kActionSkipped};
const string gstrSTATUS_STRINGS[giNUM_VALID_STATUSES] = { "Unattempted",
        "Pending", "Completed", "Failed", "Skipped" };
const string gstrCURRENTLY_ASSIGNED_USER = "<currently assigned user>";

extern const char* gpszPDF_FILE_EXTS;

//-------------------------------------------------------------------------------------------------
// CSetActionStatusFileProcessorPP
//-------------------------------------------------------------------------------------------------
CSetActionStatusFileProcessorPP::CSetActionStatusFileProcessorPP(): 
m_dwActionSel(0),
m_dwWorkflowSel(0)
{
    try
    {
        m_dwTitleID = IDS_TITLESETACTIONSTATUSFILEPROCESSORPP;
        m_dwHelpFileID = IDS_HELPFILESETACTIONSTATUSFILEPROCESSORPP;
        m_dwDocStringID = IDS_DOCSTRINGSETACTIONSTATUSFILEPROCESSORPP;
    }
    CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI15128")
}
//-------------------------------------------------------------------------------------------------
CSetActionStatusFileProcessorPP::~CSetActionStatusFileProcessorPP()
{
    try
    {
    }
    CATCH_AND_LOG_ALL_EXCEPTIONS("ELI15151")
}
//-------------------------------------------------------------------------------------------------
HRESULT CSetActionStatusFileProcessorPP::FinalConstruct()
{
    return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CSetActionStatusFileProcessorPP::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessorPP::Apply()
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ATLTRACE(_T("CSetActionStatusFileProcessorPP::Apply\n"));

        // update the settings in each of the objects associated with this UI
        for (UINT i = 0; i < m_nObjects; i++)
        {
            UCLID_FILEPROCESSORSLib::ISetActionStatusFileProcessorPtr ipFP(m_ppUnk[i]);
            ASSERT_RESOURCE_ALLOCATION("ELI15141", ipFP != __nullptr);

            string strActionName = getActionName();

            // Check if the action name does not contain tags and is not from the list
            if (strActionName.find('$') == string::npos)
            {
                int iIndex = m_cmbActionName.FindStringExact(-1, strActionName.c_str());
                if(iIndex == CB_ERR)
                {
                    MessageBox(
                        ("Action not found: " + strActionName + ". Please specify a new action.").c_str(), 
                        "Error", MB_OK | MB_ICONEXCLAMATION);
                    return S_FALSE;
                }
            }

            // update the action name in the underlying object
            ipFP->ActionName = strActionName.c_str();

            string documentName = GetDocumentName();
            if ( documentName.empty() )
            {
                MessageBox( "Empty document file name - please specify a file name.", 
                            "Error", 
                            MB_OK | MB_ICONEXCLAMATION);
                return S_FALSE;
            }

            ipFP->DocumentName = documentName.c_str();

            // retrieve the action status and update it in the underlying object
            int iActionStatusIndex = m_cmbActionStatus.GetCurSel();
            if (iActionStatusIndex == -1 || iActionStatusIndex >= giNUM_VALID_STATUSES)
            {
                UCLIDException ue("ELI15143", "Internal logic error!");
                ue.addDebugInfo("iActionStatusIndex", iActionStatusIndex);
                throw ue;
            }

            ipFP->ActionStatus = gVALID_ACTION_STATUSES[iActionStatusIndex];

            // apply radio buttons setting
            bool reportError = BST_CHECKED == m_radioBtnReportError.GetCheck();
            ipFP->ReportErrorWhenFileNotQueued = asVariantBool(reportError);

			// Set the workflow in the database
			int index = m_cmbWorkflow.GetCurSel();
			if (index > 0)
			{
				CString zValue;
				m_cmbWorkflow.GetWindowText(zValue);
				ipFP->Workflow = (LPCSTR)zValue;
				if (MessageBox("Workflow is set to something other than <Current workflow>. Are you sure?",
					"Workflow configuration", MB_YESNO | MB_ICONQUESTION) == IDNO)
				{
					return S_FALSE;
				}
			}
			else
			{
				ipFP->Workflow = "";
			}
            
            if (isValidTargetUser())
            {
                string target = getTargetUserName();
                if (target == gstrCURRENTLY_ASSIGNED_USER)
                {
                    target = "";
                }
                ipFP->TargetUser = target.c_str();
            }
            else
            {
                m_cmbTargetUser.SetFocus();
                UCLIDException ue("ELI53302", "TargetUser must be selected from the combo box or use Tags.");
                ue.addDebugInfo("TargetUser", getTargetUserName().c_str());
                throw ue;
            }
        }

        SetDirty(FALSE);
        return S_OK;
    }
    CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15142")

    // an exception was caught
    return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CSetActionStatusFileProcessorPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
                                                      BOOL& bHandled)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // get the underlying object
        UCLID_FILEPROCESSORSLib::ISetActionStatusFileProcessorPtr ipSetActionStatusFP = m_ppUnk[0];

        if (ipSetActionStatusFP != __nullptr)
        {
            // bind the combo box controls to the UI controls
            m_cmbActionName = GetDlgItem(IDC_COMBO_ACTION);
            m_cmbActionStatus = GetDlgItem(IDC_COMBO_STATUS);

            // Limit the amount of text to the maximum size of an action name
            m_cmbActionName.LimitText(50);

            // Bind the action tag button
            m_btnActionTag.SubclassDlgItem(IDC_BTN_ACTION_TAG, CWnd::FromHandle(m_hWnd));
            m_btnActionTag.SetIcon(
                ::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

            m_btnDocumentTag.SubclassDlgItem(IDC_BTN_DOCUMENT_TAG, CWnd::FromHandle(m_hWnd));
            m_btnDocumentTag.SetIcon( ::LoadIcon(_Module.m_hInstResource, 
                                                 MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

            m_btnTargetUserTag.SubclassDlgItem(IDC_BTN_TARGET_USER_TAG, CWnd::FromHandle(m_hWnd));
            m_btnTargetUserTag.SetIcon(::LoadIcon(_Module.m_hInstResource,
                MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

            m_editDocumentName = GetDlgItem(IDC_EDIT_DOCUMENT_NAME);
            string documentName = asString(ipSetActionStatusFP->DocumentName);
			if (documentName.empty())
				documentName = "<SourceDocName>";
            SetDlgItemText(IDC_EDIT_DOCUMENT_NAME, _T(documentName.c_str()));
            
            // get the action name and action status from the file processor
            string strActionName = asString(ipSetActionStatusFP->ActionName);
            EActionStatus eActionStatus = (EActionStatus) ipSetActionStatusFP->ActionStatus;

            // add entries the combo box for status
            int iDefaultActionStatusIndex = 0;
            for (long i = 0; i < giNUM_VALID_STATUSES; i++)
            {
                EActionStatus eTempActionStatus = gVALID_ACTION_STATUSES[i];
                m_cmbActionStatus.AddString(gstrSTATUS_STRINGS[i].c_str());
                if (eTempActionStatus == eActionStatus)
                {
                    iDefaultActionStatusIndex = i;
                }
            }

            // create a database manager object so that we can retrieve
            // the actions stored in the database
            IFAMDBUtilsPtr ipFAMDBUtils(CLSID_FAMDBUtils);
            ASSERT_RESOURCE_ALLOCATION("ELI34523", ipFAMDBUtils != __nullptr);
    
			m_ipFAMDB.CreateInstance(CLSID_FileProcessingDB);
            ASSERT_RESOURCE_ALLOCATION("ELI15131", m_ipFAMDB != __nullptr);

            // Connect database using last used settings in this instance
			m_ipFAMDB->ConnectLastUsedDBThisProcess();

			// Set the workflow on this database - this will cause the actions to be retrieved for the
			// the current workflow
			string strWorkflow = asString(ipSetActionStatusFP->Workflow);
			if (strWorkflow == gstrCURRENT_WORKFLOW)
			{
				m_ipFAMDB->ActiveWorkflow = "";
			}
			else
			{
				m_ipFAMDB->ActiveWorkflow = ipSetActionStatusFP->Workflow;
			}

            // select the action status associated with this object, or select "Pending"
            // as the default
            if (m_cmbActionStatus.GetCount() > 0)
            {
                m_cmbActionStatus.SetCurSel(iDefaultActionStatusIndex);
            }

			// Set up workflow
			m_cmbWorkflow = GetDlgItem(IDC_COMBO_WORKFLOW);

			// add entries to the combo box for workflow
			IStrToStrMapPtr ipWorkflowIDToNameMap = m_ipFAMDB->GetWorkflows();
			ASSERT_RESOURCE_ALLOCATION("ELI15132", ipWorkflowIDToNameMap != __nullptr);

			long lSize = ipWorkflowIDToNameMap->Size;
			for (long i = 0; i < lSize; i++)
			{
				CComBSTR bstrKey, bstrValue;
				ipWorkflowIDToNameMap->GetKeyValue(i, &bstrKey, &bstrValue);
				int index = m_cmbWorkflow.AddString(asString(bstrKey).c_str());
				m_cmbWorkflow.SetDlgItemInt(index, asLong(asString(bstrValue)));
			}

			if (lSize > 0)
			{
				m_cmbWorkflow.InsertString(0, gstrCURRENT_WORKFLOW.c_str());
				if (strWorkflow.empty())
				{
					m_cmbWorkflow.SetCurSel(0);
				}
				else
				{
					int iIndex = m_cmbWorkflow.FindStringExact(-1, strWorkflow.c_str());
					m_cmbWorkflow.SetCurSel(iIndex);
				}
			}
			else
			{
				// Disable the workflow items since they are not defined in the database
				m_cmbWorkflow.EnableWindow(FALSE);
				ATLControls::CStatic workflowLabel = GetDlgItem(IDC_STATIC_WORKFLOW);
				workflowLabel.EnableWindow(FALSE);
			}
			loadActionCombo(strActionName);

            // Setup radio buttons - "If the file does not exist in the database:"
            // * Report an error - default
            // o Queue the file
            m_radioBtnReportError = GetDlgItem(IDC_RADIO_REPORT_ERROR);
            m_radioBtnQueueFiles = GetDlgItem(IDC_RADIO_QUEUE_FILE);

            bool reportError = asCppBool(ipSetActionStatusFP->ReportErrorWhenFileNotQueued);
            if ( reportError )
            {
                m_radioBtnReportError.SetCheck(BST_CHECKED);
                m_radioBtnQueueFiles.SetCheck(BST_UNCHECKED);
            }
            else
            {
                m_radioBtnReportError.SetCheck(BST_UNCHECKED);
                m_radioBtnQueueFiles.SetCheck(BST_CHECKED);
            }

            string targetUser = asString(ipSetActionStatusFP->TargetUser);
            m_cmbTargetUser = GetDlgItem(IDC_COMBO_SELECT_USER);
            loadTargetUserCombo(targetUser);
           
        }
    }
    CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15129")

    return TRUE;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSetActionStatusFileProcessorPP::OnCbnSelEndCancelCmbActionName(WORD wNotifyCode, WORD wID, 
    HWND hWndCtl, BOOL& bHandled)
{
    AFX_MANAGE_STATE(AfxGetModuleState());

    try
    {
        // Save the location of the current edit selection
        // It includes the starting and end position of the selection
        m_dwActionSel = m_cmbActionName.GetEditSel();
    }
    CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29125")

    return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSetActionStatusFileProcessorPP::OnCbnSelEndCancelCmbTargetUser(WORD wNotifyCode, WORD wID,
    HWND hWndCtl, BOOL& bHandled)
{
    AFX_MANAGE_STATE(AfxGetModuleState());

    try
    {
        // Save the location of the current edit selection
        // It includes the starting and end position of the selection
        m_dwTargetUserSel = m_cmbTargetUser.GetEditSel();
    }
    CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29125")

        return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSetActionStatusFileProcessorPP::OnClickedBtnTargetUserTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ChooseDocTagForComboBox(ITagUtilityPtr(CLSID_FAMTagManager), m_btnTargetUserTag,
            m_cmbTargetUser, m_dwTargetUserSel);
    }
    CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI53300") 

    return TRUE;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSetActionStatusFileProcessorPP::OnClickedBtnActionTag(WORD wNotifyCode, WORD wID, 
                                                               HWND hWndCtl, BOOL& bHandled)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ChooseDocTagForComboBox(ITagUtilityPtr(CLSID_FAMTagManager), m_btnActionTag,
            m_cmbActionName, m_dwActionSel);
    }
    CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29124")

    return TRUE;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
string CSetActionStatusFileProcessorPP::getActionName()
{
    CString zText;
    m_cmbActionName.GetWindowText(zText);
    return (LPCTSTR)zText;
}
//-------------------------------------------------------------------------------------------------
string CSetActionStatusFileProcessorPP::GetDocumentName()
{
    CString zText;
    m_editDocumentName.GetWindowText(zText);
    return (LPCTSTR)zText;
}
//-------------------------------------------------------------------------------------------------
string CSetActionStatusFileProcessorPP::getTargetUserName()
{
    CString zText;
    m_cmbTargetUser.GetWindowText(zText);

    return (LPCTSTR)zText;
}
//-------------------------------------------------------------------------------------------------

LRESULT CSetActionStatusFileProcessorPP::OnBnClickedBtnDocumentTag(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ChooseDocTagForEditBox( ITagUtilityPtr(CLSID_FAMTagManager), 
                                m_btnDocumentTag,
                                m_editDocumentName );
    }
    CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38447") 

    return TRUE;
}


LRESULT CSetActionStatusFileProcessorPP::OnBnClickedBtnFileSelector(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // create the input image file dialog
        CFileDialog fileDlg( TRUE, 
                             NULL,            // The default file name extension. If this parameter is NULL, no extension is appended.
                             NULL,            // The initial file name that appears in the Filename box. If NULL, no initial file name appears.
                             OFN_HIDEREADONLY | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
                             gpszPDF_FILE_EXTS, 
                             CWnd::FromHandle(m_hWnd));
    
        // prompt the user to select an input image file
        ThreadFileDlg tfd(&fileDlg);
        if (tfd.doModal() == IDOK)
        {
            // set the input image filename edit control to the user-selected file
            m_editDocumentName.SetWindowText( fileDlg.GetPathName() );
        }
    }
    CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI38448");

    return 0;
}


LRESULT CSetActionStatusFileProcessorPP::OnCbnSelendokComboWorkflow(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	try
	{
		// Set the workflow in the database
		int index = m_cmbWorkflow.GetCurSel();
		if (index > 0)
		{
			CString zValue;
			m_cmbWorkflow.GetWindowText(zValue);
			m_ipFAMDB->ActiveWorkflow = (LPCSTR)zValue;
		}
		else
		{
			m_ipFAMDB->ActiveWorkflow = "";
		}

		// Get the current selection 
		loadActionCombo(getActionName());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI42128");

	return 0;
}

void CSetActionStatusFileProcessorPP::loadActionCombo(string strActionName)
{
	// add entries to the combo box for actions
	IStrToStrMapPtr ipActionIDToNameMap = m_ipFAMDB->GetActions();
    
    loadCombo(m_cmbActionName, ipActionIDToNameMap);

	// Ensure at least one action exists
	if (m_cmbActionName.GetCount() > 0)
	{
		// Check if an action has been specified.
		if (strActionName.empty())
		{
			// No action was specified. Select the first action by default.
			m_cmbActionName.SetCurSel(0);
		}
		else
		{
			// An action was specified. Find it by name from the list. [P13 #4967]
			int iIndex = m_cmbActionName.FindStringExact(-1, strActionName.c_str());
			if (iIndex == CB_ERR)
			{
				m_cmbActionName.SetWindowText(strActionName.c_str());

				if (strActionName.find('$') == string::npos)
				{
					// If the action is not found, display an error message. [P13 #4966] 
					MessageBox(
						("Action not found: " + strActionName + ". Please specify a new action.").c_str(),
						"Error", MB_OK | MB_ICONEXCLAMATION);
				}
			}
			else
			{
				// Select the current action.
				m_cmbActionName.SetCurSel(iIndex);
			}
		}
	}
}

void CSetActionStatusFileProcessorPP::loadTargetUserCombo(string strTargetUser)
{
    // Get the users from the database
    auto ipUsers = m_ipFAMDB->GetFamUsers();

    loadCombo(m_cmbTargetUser, ipUsers);

    // Insert the <any user> at the top
    m_cmbTargetUser.InsertString(0, gstrCURRENTLY_ASSIGNED_USER.c_str());

    int index = m_cmbTargetUser.FindStringExact(-1, strTargetUser.c_str());
    if (index != CB_ERR)
    {
        m_cmbTargetUser.SetCurSel(index);
    }
    else if (strTargetUser.empty())
    {
        m_cmbTargetUser.SetCurSel(0);
    }
    else
    {
        m_cmbTargetUser.SetWindowText(strTargetUser.c_str());
    }
}

void CSetActionStatusFileProcessorPP::loadCombo(ATLControls::CComboBox combo, IStrToStrMapPtr ipDataMap)
{
    ASSERT_ARGUMENT("ELI53284", ipDataMap != __nullptr);

    combo.ResetContent();

    long lSize = ipDataMap->Size;
    for (long i = 0; i < lSize; i++)
    {
        CComBSTR bstrKey, bstrValue;
        ipDataMap->GetKeyValue(i, &bstrKey, &bstrValue);
        combo.AddString(asString(bstrKey).c_str());
    }

}

bool CSetActionStatusFileProcessorPP::isValidTargetUser()
{
    string targetUser = getTargetUserName();
    if (m_cmbTargetUser.FindStringExact(0,targetUser.c_str()) != CB_ERR)
    {
        return true;
    }

    if (targetUser.empty())
    {
        return false;
    }

    // must not contain <any user>
    if (targetUser.find(gstrCURRENTLY_ASSIGNED_USER) != string::npos)
    {
        return false;
    }
    
    // Tags are not validated but there will be consistency checks
    // Count <, >, (, ), $  - these are all parts of tags
    int countLeftAngle = 0;
    int countRightAngle = 0;
    int countLeftParen = 0;
    int countRightParen = 0;
    int countDollerSign = 0;

    for each (auto c in targetUser)
    {
        if (c == '<') countLeftAngle++;
        if (c == '>') countRightAngle++;
        if (c == '(') countLeftParen++;
        if (c == ')') countRightParen++;
        if (c == '$') countDollerSign++;
    }

    if (countLeftAngle == 0 && countDollerSign == 0)
    {
        return false;
    }

    return countLeftAngle == countRightAngle ||
        countDollerSign == countLeftParen == countRightParen;

}
