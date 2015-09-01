
#include "stdafx.h"
#include "SetActionStatusFileProcessorPP.h"
#include "FileProcessorsUtils.h"
#include "LoadFileDlgThread.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <DocTagUtils.h>

// other constants
const int giNUM_VALID_STATUSES = 5;
const EActionStatus gVALID_ACTION_STATUSES[giNUM_VALID_STATUSES] = {kActionUnattempted,
        kActionPending, kActionCompleted, kActionFailed, kActionSkipped};
const string gstrSTATUS_STRINGS[giNUM_VALID_STATUSES] = { "Unattempted",
        "Pending", "Completed", "Failed", "Skipped" };

extern const char* gpszPDF_FILE_EXTS;

//-------------------------------------------------------------------------------------------------
// CSetActionStatusFileProcessorPP
//-------------------------------------------------------------------------------------------------
CSetActionStatusFileProcessorPP::CSetActionStatusFileProcessorPP(): 
m_dwActionSel(0)
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

            // apply radiobuttons setting
            bool reportError = BST_CHECKED == m_radioBtnReportError.GetCheck();
            ipFP->ReportErrorWhenFileNotQueued = asVariantBool(reportError);
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
        // get the underlying objet
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
    
            IFileProcessingDBPtr ipDB(ipFAMDBUtils->GetFAMDBProgId().operator LPCSTR());
            ASSERT_RESOURCE_ALLOCATION("ELI15131", ipDB != __nullptr);

            // Connect database using last used settings in this instance
            ipDB->ConnectLastUsedDBThisProcess();

            // add entries to the combo box for actions
            IStrToStrMapPtr ipActionIDToNameMap = ipDB->GetActions();
            ASSERT_RESOURCE_ALLOCATION("ELI15132", ipActionIDToNameMap != __nullptr);

            long lSize = ipActionIDToNameMap->Size;
            for (long i = 0; i < lSize; i++)
            {
                CComBSTR bstrKey, bstrValue;
                ipActionIDToNameMap->GetKeyValue(i, &bstrKey, &bstrValue);
                m_cmbActionName.AddString(asString(bstrKey).c_str());
            }

            // Ensure at least one action exists
            if(m_cmbActionName.GetCount() > 0)
            {
                // Check if an action has been specified.
                if(strActionName.empty())
                {
                    // No action was specified. Select the first action by default.
                    m_cmbActionName.SetCurSel(0);
                }
                else
                {
                    // An action was specified. Find it by name from the list. [P13 #4967]
                    int iIndex = m_cmbActionName.FindStringExact(-1, strActionName.c_str());
                    if(iIndex == CB_ERR)
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

            // select the action status associated with this object, or select "Pending"
            // as the default
            if (m_cmbActionStatus.GetCount() > 0)
            {
                m_cmbActionStatus.SetCurSel(iDefaultActionStatusIndex);
            }

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
