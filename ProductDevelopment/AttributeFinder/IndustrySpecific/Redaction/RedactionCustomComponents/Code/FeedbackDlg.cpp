// FeedbackDlg.cpp : implementation file
//

#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "FeedbackDlg.h"
#include "RedactionCCUtils.h"

#include <COMUtils.h>
#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <XBrowseForFolder.h>

//-------------------------------------------------------------------------------------------------
// CFeedbackDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CFeedbackDlg, CDialog)
//-------------------------------------------------------------------------------------------------
CFeedbackDlg::CFeedbackDlg(const FeedbackOptions &options, CWnd* pParent /*=NULL*/)
	: CDialog(CFeedbackDlg::IDD, pParent),
	  m_options(options)
{

}
//-------------------------------------------------------------------------------------------------
CFeedbackDlg::~CFeedbackDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI24548")
}
//-------------------------------------------------------------------------------------------------
void CFeedbackDlg::getOptions(FeedbackOptions &rOptions)
{
	rOptions = m_options;
}
//-------------------------------------------------------------------------------------------------
void CFeedbackDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);

	DDX_Control(pDX, IDC_EDIT_FEEDBACK_FOLDER, m_editFeedbackFolder);
	DDX_Control(pDX, IDC_BUTTON_FEEDBACK_FOLDER_BROWSE, m_btnBrowseFeedbackFolder);
	DDX_Control(pDX, IDC_CHECK_INCLUDE_REDACTION_INFO, m_chkIncludeRedactInfo);
	DDX_Control(pDX, IDC_INCLUDE_FEEDBACK_IMAGE, m_chkCollectFeedbackImage);
	DDX_Control(pDX, IDC_RADIO_ORIGINAL_FEEDBACK_FILENAMES, m_optionOriginalFeedbackFilenames);
	DDX_Control(pDX, IDC_RADIO_GENERATE_FEEDBACK_FILENAMES, m_optionGenerateFeedbackFilenames);
	DDX_Control(pDX, IDC_CHECK_COLLECT_FEEDBACK_ALL, m_chkAllFeedback);
	DDX_Control(pDX, IDC_CHECK_COLLECT_FEEDBACK_REDACTIONS, m_chkRedactFeedback);
	DDX_Control(pDX, IDC_CHECK_COLLECT_FEEDBACK_CORRECTIONS, m_chkCorrectFeedback);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFeedbackDlg, CDialog)
	ON_BN_CLICKED(IDC_BUTTON_FEEDBACK_FOLDER_BROWSE, &CFeedbackDlg::OnBnClickedButtonFeedbackFolderBrowse)
	ON_BN_CLICKED(IDC_BUTTON_SELECT_FEEDBACK_FOLDER_TAG, &CFeedbackDlg::OnBnClickedButtonSelectFeedbackFolderTag)
	ON_BN_CLICKED(IDC_CHECK_COLLECT_FEEDBACK_ALL, &CFeedbackDlg::updateControls)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CFeedbackDlg message handlers
//-------------------------------------------------------------------------------------------------
bool CFeedbackDlg::isValidFeedbackDataFolder(const string& strFeedbackFolder)
{
	// Ensure the feedback folder is non-empty
	if(strFeedbackFolder.length() == 0)
	{
		MessageBox("Please specify a feedback data folder.", "Invalid options",
			MB_ICONEXCLAMATION);
		m_editFeedbackFolder.SetSel(0, -1);
		m_editFeedbackFolder.SetFocus();
		return false;
	}
	
	// Create a local IFAMTagManagerPtr object
	IFAMTagManagerPtr ipFAMTagManager(CLSID_FAMTagManager);
	ASSERT_RESOURCE_ALLOCATION("ELI15038", ipFAMTagManager != __nullptr);

	// Ensure sure the folder name contains valid string tags
	if(ipFAMTagManager->StringContainsInvalidTags(strFeedbackFolder.c_str()) == VARIANT_TRUE)
	{
		MessageBox("Feedback data folder contains invalid tags.", 
			"Invalid options", MB_ICONEXCLAMATION);
		m_editFeedbackFolder.SetSel(0, -1);
		m_editFeedbackFolder.SetFocus();
		return false;
	}

	// If we reached this point, the folder is valid
	return true;
}
//-------------------------------------------------------------------------------------------------
void CFeedbackDlg::updateControls()
{
	// Check if all verified documents checkbox is checked
	bool bAllFeedback = m_chkAllFeedback.GetCheck() == BST_CHECKED;
	if (bAllFeedback)
	{
		// Ensure the redaction and correction feedback options are also checked
		m_chkRedactFeedback.SetCheck(TRUE);
		m_chkCorrectFeedback.SetCheck(TRUE);
	}

	// Enable/disable the redaction and correction feedback options
	BOOL bEnable = asMFCBool(!bAllFeedback);
	m_chkRedactFeedback.EnableWindow(bEnable);
	m_chkCorrectFeedback.EnableWindow(bEnable);
}
//-------------------------------------------------------------------------------------------------
BOOL CFeedbackDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	CDialog::OnInitDialog();

	try
	{
		// Feedback data storage and options
		m_btnFeedbackFolderTag.SubclassDlgItem(IDC_BUTTON_SELECT_FEEDBACK_FOLDER_TAG, 
			CWnd::FromHandle(m_hWnd));
		m_btnFeedbackFolderTag.SetIcon(::LoadIcon(_Module.m_hInstResource, 
			MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

		// Set feedback data storage options
		m_editFeedbackFolder.SetWindowText(m_options.strDataFolder.c_str());
		m_chkIncludeRedactInfo.SetCheck(BST_CHECKED);
		m_chkCollectFeedbackImage.SetCheck(asBSTChecked(m_options.bCollectImage));

		// Set feedback filename radio buttons
		if (m_options.bOriginalFilenames)
		{
			m_optionOriginalFeedbackFilenames.SetCheck(BST_CHECKED);
			m_optionGenerateFeedbackFilenames.SetCheck(BST_UNCHECKED);
		}
		else
		{
			m_optionOriginalFeedbackFilenames.SetCheck(BST_UNCHECKED);
			m_optionGenerateFeedbackFilenames.SetCheck(BST_CHECKED);
		}

		// Set feedback collection checkbox options
		m_chkAllFeedback.SetCheck( asBSTChecked(
			m_options.eCollectionOptions == kFeedbackCollectAll) );
		m_chkRedactFeedback.SetCheck( asBSTChecked(
			(m_options.eCollectionOptions & kFeedbackCollectRedact) != 0) );
		m_chkCorrectFeedback.SetCheck( asBSTChecked( 
			(m_options.eCollectionOptions & kFeedbackCollectCorrect) != 0) );

		// Update the controls
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24542")

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void CFeedbackDlg::OnOK()
{
	try
	{
		// Get the feedback folder
		CString zFeedbackFolder;
		m_editFeedbackFolder.GetWindowText(zFeedbackFolder);
		string strFeedbackFolder = zFeedbackFolder;

		// Validate folder name if collect feedback option is enabled
		if ( !isValidFeedbackDataFolder(strFeedbackFolder) )
		{
			// NOTE: isValidFeedbackDataFolder has already displayed a message to the user
			return;
		}

		// Set the feedback folder
		m_options.strDataFolder = strFeedbackFolder;

		// Set whether to include the original document in feedback data
		m_options.bCollectImage = m_chkCollectFeedbackImage.GetCheck() == BST_CHECKED;

		// Set whether to use the original filenames or to generate unique filenames
		m_options.bOriginalFilenames = m_optionOriginalFeedbackFilenames.GetCheck() == BST_CHECKED;

		// Set the feedback collection object
		if (m_chkAllFeedback.GetCheck() == BST_CHECKED)
		{
			m_options.eCollectionOptions = 
				(UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption) kFeedbackCollectAll;
		}
		else
		{
			// Calculate the feedback collection option from the checkboxes
			UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption eFeedbackCollect =
				(UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption) kFeedbackCollectNone;
			if(m_chkRedactFeedback.GetCheck() == BST_CHECKED)
			{
				eFeedbackCollect = (UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption) 
					(eFeedbackCollect | kFeedbackCollectRedact);
			}
			if(m_chkCorrectFeedback.GetCheck() == BST_CHECKED)
			{
				eFeedbackCollect = (UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption) 
					(eFeedbackCollect | kFeedbackCollectCorrect);
			}

			// Ensure at least one feedback collection option was selected
			if (eFeedbackCollect == kFeedbackCollectNone)
			{
				MessageBox(
					"Please specify at least one type of document from which to collect feedback.", 
					"Invalid options", MB_ICONEXCLAMATION);
				m_chkAllFeedback.SetFocus();
				return;
			}

			// Store the result
			m_options.eCollectionOptions = eFeedbackCollect;
		}

		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24543")
}
//-------------------------------------------------------------------------------------------------
void CFeedbackDlg::OnBnClickedButtonFeedbackFolderBrowse()
{
	try
	{
		CString zOutputFile;
		m_editFeedbackFolder.GetWindowText(zOutputFile);
		
		// Separate folder and file portions of output file path
		long lTotal = zOutputFile.GetLength();
		string strFolder;
		string strFile;
		if (lTotal > 0)
		{
			strFolder = getDirectoryFromFullPath( LPCTSTR(zOutputFile) );
			long lLength = strFolder.length();

			// Provide trailing backslash to folder
			if (zOutputFile.GetAt( lLength ) == '\\')
			{
				lLength++;
			}
			strFile = LPCTSTR(zOutputFile.Right(lTotal - lLength));
		}

		// Display folder selection dialog
		char pszPath[MAX_PATH + 1];
		if (XBrowseForFolder( m_hWnd, strFolder.c_str(), pszPath, sizeof(pszPath) ))
		{
			// Refresh the display by appending the filename to the new folder
			zOutputFile = pszPath;
			zOutputFile += "\\";
			zOutputFile += strFile.c_str();

			m_editFeedbackFolder.SetWindowText(zOutputFile);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24544")
}
//-------------------------------------------------------------------------------------------------
void CFeedbackDlg::OnBnClickedButtonSelectFeedbackFolderTag()
{
	try
	{
		// Get the position and dimensions of the button
		RECT rect;
		m_btnFeedbackFolderTag.GetWindowRect(&rect);

		// Get the user selection
		string strChoice = CRedactionCustomComponentsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);

		// Set the corresponding edit box if the user selected a tag
		if(strChoice != "")
		{
			m_editFeedbackFolder.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24545")
}