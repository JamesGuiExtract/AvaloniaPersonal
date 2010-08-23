// SetFilePriorityDlg.cpp : implementation file
//

#include "stdafx.h"
#include "SetFilePriorityDlg.h"
#include "FAMDBAdminUtils.h"
#include "SelectFilesDlg.h"

#include <ADOUtils.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <FAMUtilsConstants.h>
#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrSQL_SELECT_VALUE = "FAMFile.ID, FAMFile.Priority";

//-------------------------------------------------------------------------------------------------
// CSetFilePriorityDlg dialog
//-------------------------------------------------------------------------------------------------
CSetFilePriorityDlg::CSetFilePriorityDlg(IFileProcessingDBPtr ipFAMDB)
: CDialog(CSetFilePriorityDlg::IDD),
m_ipFAMDB(ipFAMDB)
{
}
//-------------------------------------------------------------------------------------------------
CSetFilePriorityDlg::~CSetFilePriorityDlg()
{
	try
	{
		m_ipFAMDB = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27687");
}
//-------------------------------------------------------------------------------------------------
void CSetFilePriorityDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_EDIT_FL_SLCT_SMRY_PRIORITY, m_editSummary);
	DDX_Control(pDX, IDC_CMB_FL_SLCT_PRIORITY, m_comboPriority);
	DDX_Control(pDX, IDOK, m_btnOk);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSetFilePriorityDlg, CDialog)
	ON_BN_CLICKED(IDC_BTN_SLCT_FLS_PRIORITY, &CSetFilePriorityDlg::OnClickedSelectFiles)
	ON_BN_CLICKED(IDOK, &CSetFilePriorityDlg::OnClickedOK)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CSetFilePriorityDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CSetFilePriorityDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Fill the priority combo box
		fillPriorityCombo();

		// Get the default priority string
		_bstr_t bstrDefaultPriority = m_ipFAMDB->AsPriorityString(kPriorityDefault);

		// Find the index of the default priority
		int nIndex = m_comboPriority.FindString(-1, (const char*)bstrDefaultPriority);
		if (nIndex == CB_ERR)
		{
			UCLIDException ue("ELI27694", "Unable to find default priority value.");
			ue.addDebugInfo("Default Priority String", asString(bstrDefaultPriority));
			throw ue;
		}

		// Set the default priority value in the combo box
		m_comboPriority.SetCurSel(nIndex);

		// Update the summary with the settings string
		m_editSummary.SetWindowText(m_settings.getSummaryString().c_str());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27688");

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CSetFilePriorityDlg::OnClickedSelectFiles()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CSelectFilesDlg dlg(m_ipFAMDB, "Select files to modify priority", 
			"SELECT " + gstrSQL_SELECT_VALUE + " FROM ", m_settings);

		// Display the dialog and save changes if user clicked OK
		if (dlg.DoModal() == IDOK)
		{
			// Get the settings from the dialog
			m_settings = dlg.getSettings();

			// Update the summary description
			m_editSummary.SetWindowText(m_settings.getSummaryString().c_str());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27689");
}
//-------------------------------------------------------------------------------------------------
void CSetFilePriorityDlg::OnClickedOK()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		// Build the query for setting priority
		string strQuery = m_settings.buildQuery(m_ipFAMDB, gstrSQL_SELECT_VALUE, "");

		// Get the priority string
		CString zPriority;
		m_comboPriority.GetWindowText(zPriority);

		// Execute the update query
		long nNumRecords = m_ipFAMDB->SetPriorityForFiles(strQuery.c_str(),
			(EFilePriority)(m_comboPriority.GetCurSel()+1), m_settings.getRandomCondition());

		// Prompt the users that the priority has been changed.
		CString zPrompt;
		zPrompt.Format("A total of %ld files have had their priority set to '", nNumRecords);
		zPrompt.Append(zPriority); 
		zPrompt.Append("'.");
		MessageBox(zPrompt, "Success", MB_ICONINFORMATION);

		// Build an application trace for the database change
		UCLIDException uex("ELI27700", "Application trace: Database change");
		uex.addDebugInfo("Change", "Set file priority");
		uex.addDebugInfo("User Name", getCurrentUserName());
		uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
		uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));
		uex.addDebugInfo("New Priority", (LPCTSTR)zPriority);
		uex.addDebugInfo("Query", strQuery);
		uex.log();

		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27691");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CSetFilePriorityDlg::fillPriorityCombo()
{
	try
	{
		// Get the list of priorities
		IVariantVectorPtr ipVecPriority = m_ipFAMDB->GetPriorities();
		ASSERT_RESOURCE_ALLOCATION("ELI27692", ipVecPriority != NULL);

		// Add each priority to the combo box
		long lSize = ipVecPriority->Size;
		for (long i=0; i < lSize; i++)
		{
			m_comboPriority.AddString(asString(ipVecPriority->Item[i].bstrVal).c_str());
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27693");
}
//-------------------------------------------------------------------------------------------------