// ExportFileListDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ExportFileListDlg.h"
#include "FAMDBAdminUtils.h"
#include "SelectFilesDlg.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <LoadFileDlgThread.h>
#include <FAMUtilsConstants.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constant
const string gstrSQL_SELECT_VALUES = "FAMFile.FileName, FAMFile.ID";

//-------------------------------------------------------------------------------------------------
// CExportFileListDlg dialog
//-------------------------------------------------------------------------------------------------
CExportFileListDlg::CExportFileListDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB)
: CDialog(CExportFileListDlg::IDD),
m_ipFAMDB(ipFAMDB)
{
	//{{AFX_DATA_INIT(CExportFileListDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
CExportFileListDlg::~CExportFileListDlg()
{
	try
	{
		m_ipFAMDB = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27682");
}
//-------------------------------------------------------------------------------------------------
void CExportFileListDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CExportFileListDlg)
	// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
	DDX_Control(pDX, IDC_EDIT_FL_SLCT_SMRY_EXPORT, m_editSummary);
	DDX_Control(pDX, IDC_EDT_FILE_NAME, m_editFileName);
	DDX_Control(pDX, IDC_BTN_BROWSE_FILE, m_btnBrowse);
	DDX_Control(pDX, IDOK, m_btnOk);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CExportFileListDlg, CDialog)
	//{{AFX_MSG_MAP(CExportFileListDlg)
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_BTN_SLCT_FLS_EXPORT, &CExportFileListDlg::OnClickedSelectFiles)
	ON_BN_CLICKED(IDC_BTN_BROWSE_FILE, &CExportFileListDlg::OnClickedBrowseFile)
	ON_BN_CLICKED(IDOK, &CExportFileListDlg::OnClickedOK)
	ON_EN_CHANGE(IDC_EDT_FILE_NAME, &CExportFileListDlg::OnChangedEditFileName)
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

		updateControls();

		// Update the summary with the settings string
		m_editSummary.SetWindowText(m_settings.getSummaryString().c_str());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14730")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CExportFileListDlg::OnClickedBrowseFile()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		const static string s_strAllFiles = "Text files (*.txt)|*.txt|All files (*.*)|*.*||";

		// Bring open file dialog
		CFileDialog fileDlg(FALSE, ".txt", NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
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

		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14731");
}
//-------------------------------------------------------------------------------------------------
void CExportFileListDlg::OnChangedEditFileName()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update the controls (enables/disables the ok button appropriately)
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27066");
}
//-------------------------------------------------------------------------------------------------
void CExportFileListDlg::OnClickedSelectFiles()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CSelectFilesDlg dlg(m_ipFAMDB, "Select filenames to export", 
			"SELECT " + gstrSQL_SELECT_VALUES + " FROM ", m_settings);

		// Display the dialog and save changes if user clicked OK
		if (dlg.DoModal() == IDOK)
		{
			// Get the settings from the dialog
			m_settings = dlg.getSettings();

			// Update the summary description
			m_editSummary.SetWindowText(m_settings.getSummaryString().c_str());
		}

		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27063");
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

		// Build the query from the settings
		string strQuery = m_settings.buildQuery(m_ipFAMDB, gstrSQL_SELECT_VALUES, 
			" ORDER BY [FAMFile].[ID]");

		// Call ExportFileList() to export the file list and get a count of exported files
		long lNumFilesExported = m_ipFAMDB->ExportFileList(strQuery.c_str(), _bstrFileName,
			m_settings.getRandomCondition());

		// Prompt the users that exporting files is finished and
		// if they want to open the file contains the file list
		CString zPrompt;
		zPrompt.Format("A list of %ld files were\n\rexported to the selected output file.",
			lNumFilesExported);
		MessageBox(zPrompt, "Success", MB_ICONINFORMATION);

		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14886");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CExportFileListDlg::updateControls() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Enable/disable the okay button based on whether the settings are initialized
		// Get the text from the edit box
		CString zText;
		m_editFileName.GetWindowText(zText);

		// Enable the OK button if the edit box contains at least 4 characters
		// and the file name is not a relative path
		bool bEnable = zText.GetLength() > 4 && isAbsolutePath((LPCTSTR)zText);
		m_btnOk.EnableWindow(asMFCBool(bEnable));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14732");
}
//-------------------------------------------------------------------------------------------------