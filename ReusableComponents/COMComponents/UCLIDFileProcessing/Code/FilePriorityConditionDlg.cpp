#include "StdAfx.h"
#include "FilePriorityConditionDlg.h"

#include <UCLIDException.h>
#include <ComUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// FilePriorityConditionDlg dialog
//-------------------------------------------------------------------------------------------------
FilePriorityConditionDlg::FilePriorityConditionDlg(
									const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB)
: CDialog(FilePriorityConditionDlg::IDD),
m_ipFAMDB(ipFAMDB)
{
}
//-------------------------------------------------------------------------------------------------
FilePriorityConditionDlg::FilePriorityConditionDlg(
									const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
									const FilePriorityCondition& settings)
: CDialog(FilePriorityConditionDlg::IDD),
m_ipFAMDB(ipFAMDB),
m_settings(settings)
{
}
//-------------------------------------------------------------------------------------------------
FilePriorityConditionDlg::~FilePriorityConditionDlg()
{
	try
	{
		m_ipFAMDB = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33796");
}
//-------------------------------------------------------------------------------------------------
void FilePriorityConditionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(ActionStatusConditionDlg)
	// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
	DDX_Control(pDX, IDC_CMB_FILE_PRIORITY, m_comboPriority);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FilePriorityConditionDlg, CDialog)
	//{{AFX_MSG_MAP(FilePriorityConditionDlg)
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_SELECT_BTN_OK, &FilePriorityConditionDlg::OnClickedOK)
	ON_BN_CLICKED(IDC_SELECT_BTN_CANCEL, &FilePriorityConditionDlg::OnClickedCancel)
	END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// FilePriorityConditionDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL FilePriorityConditionDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		IVariantVectorPtr ipVecPriority = m_ipFAMDB->GetPriorities();
		ASSERT_RESOURCE_ALLOCATION("ELI33797", ipVecPriority != __nullptr);

		// Add each priority to the combo box
		long lSize = ipVecPriority->Size;
		for (long i=0; i < lSize; i++)
		{
			m_comboPriority.AddString(asString(ipVecPriority->Item[i].bstrVal).c_str());
		}

		// Select the first item in the priority combo
		m_comboPriority.SetCurSel(0);

		// Read the settings object and set the dialog based on the settings
		setControlsFromSettings();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33798")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void FilePriorityConditionDlg::OnOK()
{
	// Allow the user to use the enter key to close the dialog
	OnClickedOK();
}
//-------------------------------------------------------------------------------------------------
void FilePriorityConditionDlg::OnClose()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	try
	{
		EndDialog(IDCLOSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33799");
}
//-------------------------------------------------------------------------------------------------
void FilePriorityConditionDlg::OnClickedCancel()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Call cancel
		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33800");
}
//-------------------------------------------------------------------------------------------------
void FilePriorityConditionDlg::OnClickedOK()
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33801");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool FilePriorityConditionDlg::saveSettings()
{
	try
	{
		// Set the priority (priority is current selected index + 1)
		m_settings.setPriority(
			(UCLID_FILEPROCESSINGLib::EFilePriority)(m_comboPriority.GetCurSel()+1));

		return true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33802")
}
//-------------------------------------------------------------------------------------------------
void FilePriorityConditionDlg::setControlsFromSettings()
{
	try
	{
		m_comboPriority.SetCurSel(((int)(m_settings.getPriority()))-1);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33803");
}