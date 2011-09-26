#include "StdAfx.h"
#include "QueryConditionDlg.h"

#include <UCLIDException.h>
#include <ComUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// QueryConditionDlg dialog
//-------------------------------------------------------------------------------------------------

QueryConditionDlg::QueryConditionDlg(const IFileProcessingDBPtr& ipFAMDB)
: CDialog(QueryConditionDlg::IDD),
m_ipFAMDB(ipFAMDB)
{
}
//-------------------------------------------------------------------------------------------------
QueryConditionDlg::QueryConditionDlg(const IFileProcessingDBPtr& ipFAMDB,
									 const QueryCondition& settings, const string& strQueryHeader)
: CDialog(QueryConditionDlg::IDD),
m_ipFAMDB(ipFAMDB),
m_settings(settings),
m_strQueryHeader(strQueryHeader)
{
}
//-------------------------------------------------------------------------------------------------
QueryConditionDlg::~QueryConditionDlg()
{
	try
	{
		m_ipFAMDB = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33805");
}
//-------------------------------------------------------------------------------------------------
void QueryConditionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(ActionStatusConditionDlg)
	// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
	DDX_Control(pDX, IDC_SLCT_FILE_QUERY_LABEL, m_lblQuery);
	DDX_Control(pDX, IDC_EDIT_SQL_QUERY, m_editSelectQuery);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(QueryConditionDlg, CDialog)
	//{{AFX_MSG_MAP(PriorityConditionDlg)
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_SELECT_BTN_OK, &QueryConditionDlg::OnClickedOK)
	ON_BN_CLICKED(IDC_SELECT_BTN_CANCEL, &QueryConditionDlg::OnClickedCancel)
	END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// QueryConditionDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL QueryConditionDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		m_lblQuery.SetWindowText(m_strQueryHeader.c_str());

		// Read the settings object and set the dialog based on the settings
		setControlsFromSettings();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33806")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void QueryConditionDlg::OnOK()
{
	// Allow the user to use the enter key to close the dialog
	OnClickedOK();
}
//-------------------------------------------------------------------------------------------------
void QueryConditionDlg::OnClose()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	try
	{
		EndDialog(IDCLOSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33807");
}
//-------------------------------------------------------------------------------------------------
void QueryConditionDlg::OnClickedCancel()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Call cancel
		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33808");
}
//-------------------------------------------------------------------------------------------------
void QueryConditionDlg::OnClickedOK()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Save the settings
		if (saveSettings())
		{
			// If settings saved successfully, close the dialog
			CDialog::OnOK();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33809");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool QueryConditionDlg::saveSettings()
{
	try
	{
		// Get the query from the edit box
		CString zTemp;
		m_editSelectQuery.GetWindowText(zTemp);
		if (zTemp.IsEmpty())
		{
			// Show error message to user
			MessageBox("Query may not be blank!", "Configuration Error",
				MB_OK | MB_ICONERROR);

			// Set focus to query
			m_editSelectQuery.SetFocus();

			// Return false
			return false;
		}

		m_settings.setSQLString((LPCTSTR) zTemp);

		return true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33810")
}
//-------------------------------------------------------------------------------------------------
void QueryConditionDlg::setControlsFromSettings()
{
	try
	{
		m_editSelectQuery.SetWindowText(m_settings.getSQLString().c_str());
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33811");
}