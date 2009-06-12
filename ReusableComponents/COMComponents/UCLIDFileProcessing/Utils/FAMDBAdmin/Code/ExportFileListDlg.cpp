// ExportFileListDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ExportFileListDlg.h"
#include "FAMDBAdminUtils.h"

#include <UCLIDException.h>
#include <FileDialogEx.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <LoadFileDlgThread.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constant
const CString gzSQL_QUERY_STRING = "SELECT FAMFile.FileName FROM ";
//-------------------------------------------------------------------------------------------------
// CExportFileListDlg dialog
//-------------------------------------------------------------------------------------------------
CExportFileListDlg::CExportFileListDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB)
: CDialog(CExportFileListDlg::IDD),
m_ipFAMDB(ipFAMDB),
m_zSqlQuery("")
{
	//{{AFX_DATA_INIT(CExportFileListDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void CExportFileListDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CExportFileListDlg)
	// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
	DDX_Control(pDX, IDC_RADIO_ALL_FILES, m_radioAllFiles);
	DDX_Control(pDX, IDC_RADIO_STATUS, m_radioStatus);
	DDX_Control(pDX, IDC_CMB_ACTION_STATUS, m_comboStatus);
	DDX_Control(pDX, IDC_CMB_ACTION_NAME, m_comboActions);
	DDX_Control(pDX, IDC_RADIO_SQL_QUERY, m_radioQuery);
	DDX_Control(pDX, IDC_EDIT_SQL_QUERY, m_editQuery);
	DDX_Control(pDX, IDC_EDT_FILE_NAME, m_editFileName);
	DDX_Control(pDX, IDC_BTN_BROWSE_FILE, m_btnBrowse);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CExportFileListDlg, CDialog)
	//{{AFX_MSG_MAP(CExportFileListDlg)
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_RADIO_ALL_FILES, &CExportFileListDlg::OnRadioAllFiles)
	ON_BN_CLICKED(IDC_RADIO_STATUS, &CExportFileListDlg::OnRadioStatus)
	ON_BN_CLICKED(IDC_RADIO_SQL_QUERY, &CExportFileListDlg::OnRadioSqlQuery)
	ON_BN_CLICKED(IDC_BTN_BROWSE_FILE, &CExportFileListDlg::OnClickedBrowseFile)
	ON_BN_CLICKED(IDOK, &CExportFileListDlg::OnClickedOK)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CExportFileListDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CExportFileListDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// display a wait-cursor because we are getting information from the DB, which may take a few seconds
		CWaitCursor wait;

		// Insert the action status to the ComboBox
		CFAMDBAdminUtils::addStatusInComboBox(m_comboStatus);

		// Set the default item in the status ComboBox to Pending
		m_comboStatus.SetCurSel(1);

		// Read all actions from the DB
		IStrToStrMapPtr pMapActions = m_ipFAMDB->GetActions();

		// Insert actions into ComboBox
		for (int i = 0; i < pMapActions->GetSize(); i++)
		{
			// Get one actions' name and ID inside the database
			CComBSTR bstrKey, bstrValue;
			pMapActions->GetKeyValue(i, &bstrKey, &bstrValue);
			string strAction = asString(bstrKey);
			DWORD nID = asUnsignedLong(asString(bstrValue));

			// Insert the action name into combo box
			int nIndex = m_comboActions.InsertString(-1, strAction.c_str());

			// Set the index of the item inside the Combobox same as the ID of the action
			m_comboActions.SetItemData(nIndex, nID);
		}
		
		// Set the current action to the first action
		m_comboActions.SetCurSel(0);

		// Select the status radio button as default setting
		m_radioStatus.SetCheck(BST_CHECKED);
		updateControls();

		// Set the focus to the status combo box
		m_comboStatus.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14730")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CExportFileListDlg::OnRadioAllFiles()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		updateControls();
		m_editFileName.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14881")
}
//-------------------------------------------------------------------------------------------------
void CExportFileListDlg::OnRadioStatus()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		updateControls();
		m_comboStatus.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14882")
}
//-------------------------------------------------------------------------------------------------
void CExportFileListDlg::OnRadioSqlQuery()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		updateControls();
		m_editQuery.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14883")
}
//-------------------------------------------------------------------------------------------------
void CExportFileListDlg::OnClickedBrowseFile()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		const static string s_strAllFiles = "Text files (*.txt)|*.txt|All files (*.*)|*.*||";

		// Bring open file dialog
		CFileDialogEx fileDlg(FALSE, ".txt", NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
			OFN_NOREADONLYRETURN | OFN_PATHMUSTEXIST | OFN_OVERWRITEPROMPT,
			s_strAllFiles.c_str(), this);

		// Pass the pointer of dialog to create ThreadFileDlg object
		ThreadFileDlg tfd(&fileDlg);

		// If OK is clicked
		if (tfd.doModal() == IDOK)
		{
			// Get the file name
			m_editFileName.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14731");
}
//-------------------------------------------------------------------------------------------------
void CExportFileListDlg::OnClickedOK()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Get the file name to save the exported file list
		CString zFileName;
		m_editFileName.GetWindowText(zFileName);

		// Check for empty file name
		if (zFileName.IsEmpty())
		{
			// Prompt to the user that a file has not been specified
			MessageBox("Please specify a file name to export the file list!", 
				"Empty File Name", MB_ICONINFORMATION);
			m_editFileName.SetFocus();
			return;
		}
		// Check for relative path [LRCAU #4900]
		else if(!isAbsolutePath((LPCTSTR)zFileName))
		{
			MessageBox("Please specify an absolute path for the export file list!",
				"Relative Path", MB_ICONINFORMATION);
			m_editFileName.SetFocus();
			return;
		}

		_bstr_t _bstrFileName(zFileName);

		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		// Query based on the action status
		if (m_radioStatus.GetCheck() == BST_CHECKED)
		{
			// Get the current action name
			CString zAction;
			m_comboActions.GetWindowText(zAction);

			if (zAction.IsEmpty())
			{
				// Prompt to the users that there should be an action specified 
				// if they choose to export file list according to file name and status
				MessageBox("An action must be specified to export the file list!", 
					"Error", MB_ICONINFORMATION);
				
				// Set the focus back to action combo box and return
				m_comboActions.SetFocus();
				return;
			}

			// Action status selected in the status combo box
			UCLID_FILEPROCESSINGLib::EActionStatus eStatus;

			// Cast the current selected status to an EActionStatus type
			int iStatusID = m_comboStatus.GetCurSel();
			eStatus = (UCLID_FILEPROCESSINGLib::EActionStatus)(iStatusID);

			// Get the action status as a string
			CString zActionStatus = asStatusString(eStatus);

			// Action Column to export
			CString zActionCol = "ASC_" + zAction;

			// Form the sql query string
			m_zSqlQuery = gzSQL_QUERY_STRING + "FAMFile WHERE (" + zActionCol + " = '" + zActionStatus + "')";
		}
		// Query to export all the files
		else if (m_radioAllFiles.GetCheck() == BST_CHECKED)
		{
			// Form the sql query string
			m_zSqlQuery = gzSQL_QUERY_STRING + "FAMFile";
		}
		// Export based on customer query
		else if (m_radioQuery.GetCheck() == BST_CHECKED)
		{
			// Get the query input by the user
			CString zCustomQuery;
			m_editQuery.GetWindowText(zCustomQuery);

			// Add the header and build a complete query
			m_zSqlQuery = gzSQL_QUERY_STRING + zCustomQuery;
		}
		else
		{
			// We should never reach here
			THROW_LOGIC_ERROR_EXCEPTION("ELI14879");
		}

		// Define how many file has been exported 
		long lNumFilesExported;

		// Call ExportFileList() to export the fil list
		lNumFilesExported = m_ipFAMDB->ExportFileList(m_zSqlQuery.operator LPCTSTR(), _bstrFileName);

		OnOK();

		// Prompt the users that epxorting files is finished and
		// if they want to open the file contains the file list
		CString zPrompt;
		zPrompt.Format("A list of %ld files were\n\rexported to the selected output file.", lNumFilesExported);
		MessageBox(zPrompt, "Success", MB_ICONINFORMATION);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14886");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CExportFileListDlg::updateControls() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (m_radioAllFiles.GetCheck() == BST_CHECKED)
		{
			// Disable two combo boxes and the query edit box
			m_comboStatus.EnableWindow(FALSE);
			m_comboActions.EnableWindow(FALSE);
			m_editQuery.EnableWindow(FALSE);
		}
		else
		{
			if (m_radioStatus.GetCheck() == BST_CHECKED)
			{
				// Enable combo boxes and disable the query edit box
				m_comboStatus.EnableWindow(TRUE);
				m_comboActions.EnableWindow(TRUE);
				m_editQuery.EnableWindow(FALSE);
			}
			else
			{
				// Disable combo boxes and enable the query edit box
				m_comboStatus.EnableWindow(FALSE);
				m_comboActions.EnableWindow(FALSE);
				m_editQuery.EnableWindow(TRUE);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14732");
}
//-------------------------------------------------------------------------------------------------
CString CExportFileListDlg::asStatusString(UCLID_FILEPROCESSINGLib::EActionStatus eStatusID)
{
	switch ( eStatusID )
	{
	case kActionUnattempted:
		return "U";
	case kActionPending:
		return "P";
	case kActionProcessing:
		return "R";
	case kActionCompleted:
		return "C";
	case kActionFailed:
		return "F";
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI14880");
	}
	return "U";
}
//-------------------------------------------------------------------------------------------------