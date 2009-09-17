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
const string gstrPRIORITY_PLACEHOLDER = "<PriorityValue>";
const string gstrSQL_QUERY_STRING = "UPDATE FAMFile SET FAMFile.Priority = "
 + gstrPRIORITY_PLACEHOLDER + " FROM ";

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
			gstrSQL_QUERY_STRING, m_settings);

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

		string strQuery = gstrSQL_QUERY_STRING;

		switch(m_settings.getScope())
		{
			// Query based on the action status
		case eAllFilesForWhich:
			{
				strQuery += "FAMFile ";

				// Check if comparing skipped status
				if (m_settings.getStatus() == kActionSkipped)
				{
					strQuery += "INNER JOIN SkippedFile ON FAMFile.ID = SkippedFile.FileID WHERE "
						"(SkippedFile.ActionID = " + m_settings.getActionID();
					string strUser = m_settings.getUser();
					if (strUser != gstrANY_USER)
					{
						strQuery += " AND SkippedFile.UserName = '" + strUser + "'";
					}
					strQuery += ")";
				}
				else
				{
					// Get the status as a string
					string strStatus = m_ipFAMDB->AsStatusString(
						(UCLID_FILEPROCESSINGLib::EActionStatus)m_settings.getStatus());
				
					strQuery += "WHERE (ASC_" + m_settings.getAction() + " = '"
						+ strStatus + "')";
				}
			}
			break;

			// Query to export all the files
		case eAllFiles:
			{
				strQuery += "FAMFile";
			}
			break;

			// Export based on customer query
		case eAllFilesQuery:
			{
				// Get the query input by the user
				strQuery += m_settings.getSQLString();
			}
			break;

		case eAllFilesTag:
			{
				// Get the vector of tags
				vector<string> vecTags = m_settings.getTags();

				// Get the size and ensure there is at least 1 tag
				size_t nSize = vecTags.size();
				if (nSize == 0)
				{
					MessageBox("No tags selected!", "No Tags", MB_OK | MB_ICONERROR);
					return;
				}

				string strMainQueryTemp = gstrQUERY_FILES_WITH_TAGS;
				replaceVariable(strMainQueryTemp, gstrTAG_QUERY_SELECT, "FAMFile.FileName");

				// Get the conjunction for the where clause
				string strConjunction = m_settings.getAnyTags() ? "\nUNION\n" : "\nINTERSECT\n";

				strQuery += "(" + strMainQueryTemp;
				replaceVariable(strQuery, gstrTAG_NAME_VALUE, vecTags[0]);

				// Build the rest of the query
				for (size_t i=1; i < nSize; i++)
				{
					string strTemp = strMainQueryTemp;
					replaceVariable(strTemp, gstrTAG_NAME_VALUE, vecTags[i]);
					strQuery += strConjunction + strTemp;
				}

				strQuery += ") AS TempPriorityUpdater";
			}
			break;

		case eAllFilesPriority:
			{
				strQuery += "FAMFile WHERE FAMFile.Priority = "
					+ asString((long)m_settings.getPriority());
			}
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI27690");
		}

		// Set the priority value in the query
		replaceVariable(strQuery, gstrPRIORITY_PLACEHOLDER,
			asString(m_comboPriority.GetCurSel() + 1)); 

		// Execute the update query
		long nNumRecords = m_ipFAMDB->ExecuteCommandQuery(strQuery.c_str());

		// Prompt the users that the priority has been changed.
		CString zPrompt, zPriority;
		m_comboPriority.GetWindowText(zPriority);
		zPrompt.Format("A total of %ld files have had their priority set to '", nNumRecords);
		zPrompt.Append(zPriority); 
		zPrompt.Append("'.");
		MessageBox(zPrompt, "Success", MB_ICONINFORMATION);

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