#include "StdAfx.h"
#include "FileTagConditionDlg.h"

#include <UCLIDException.h>
#include <ComUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const int giANY_TAG = 0;
static const int giALL_TAG = 1;
static const int giNONE_TAG = 2;

//-------------------------------------------------------------------------------------------------
// FileTagConditionDlg dialog
//-------------------------------------------------------------------------------------------------
FileTagConditionDlg::FileTagConditionDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB)
: CDialog(FileTagConditionDlg::IDD),
m_ipFAMDB(ipFAMDB)
{
}
//-------------------------------------------------------------------------------------------------
FileTagConditionDlg::FileTagConditionDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
										 const FileTagCondition& settings)
: CDialog(FileTagConditionDlg::IDD),
m_ipFAMDB(ipFAMDB),
m_settings(settings)
{
}
//-------------------------------------------------------------------------------------------------
FileTagConditionDlg::~FileTagConditionDlg()
{
	try
	{
		m_ipFAMDB = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33814");
}
//-------------------------------------------------------------------------------------------------
void FileTagConditionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(ActionStatusConditionDlg)
	// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
	DDX_Control(pDX, IDC_CMB_ANY_ALL_TAGS, m_comboTagsAnyAll);
	DDX_Control(pDX, IDC_SELECT_LIST_TAGS, m_listTags);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileTagConditionDlg, CDialog)
	//{{AFX_MSG_MAP(FilePriorityConditionDlg)
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_SELECT_BTN_OK, &FileTagConditionDlg::OnClickedOK)
	ON_BN_CLICKED(IDC_SELECT_BTN_CANCEL, &FileTagConditionDlg::OnClickedCancel)
	END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// FileTagConditionDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL FileTagConditionDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();
		// Display a wait-cursor because we are getting information from the DB, 
		// which may take a few seconds
		CWaitCursor wait;

		// Configure the tag list and populate it with the current tags
		configureAndPopulateTagList();

		// Add the any and all values to the combo box
		m_comboTagsAnyAll.InsertString(giANY_TAG, "Any");
		m_comboTagsAnyAll.InsertString(giALL_TAG, "All");
		m_comboTagsAnyAll.InsertString(giNONE_TAG, "None");
		m_comboTagsAnyAll.SetCurSel(giANY_TAG);

		// Read the settings object and set the dialog based on the settings
		setControlsFromSettings();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33815")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void FileTagConditionDlg::OnOK()
{
	// Allow the user to use the enter key to close the dialog
	OnClickedOK();
}
//-------------------------------------------------------------------------------------------------
void FileTagConditionDlg::OnClose()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	try
	{
		EndDialog(IDCLOSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33816");
}
//-------------------------------------------------------------------------------------------------
void FileTagConditionDlg::OnClickedCancel()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Call cancel
		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33817");
}
//-------------------------------------------------------------------------------------------------
void FileTagConditionDlg::OnClickedOK()
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33818");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool FileTagConditionDlg::saveSettings()
{
	try
	{
		// Get each selected tag from the list
		vector<string> vecTags;
		int nCount = m_listTags.GetItemCount();
		for (int i=0; i < nCount; i++)
		{
			if (m_listTags.GetCheck(i) == TRUE)
			{
				// Get the text for the item
				CString zTagName = m_listTags.GetItemText(i, 0);
				vecTags.push_back((LPCTSTR) zTagName);
			}
		}

		// Check for at least 1 tag selected
		if (vecTags.size() == 0)
		{
			// Prompt the user
			MessageBox("You must select at least 1 tag!", "No Tag Selected",
				MB_OK | MB_ICONERROR);

			// Set focus to the list control
			m_listTags.SetFocus();

			// Return false
			return false;
		}

		m_settings.setTags(vecTags);
		m_settings.setTagType((TagMatchType) m_comboTagsAnyAll.GetCurSel());

		return true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33819")
}
//-------------------------------------------------------------------------------------------------
void FileTagConditionDlg::setControlsFromSettings()
{
	try
	{
		// Set the any/all value
		m_comboTagsAnyAll.SetCurSel((int) m_settings.getTagType());

		// Now attempt to select the appropriate tag names
		vector<string> vecTags = m_settings.getTags();
		vector<string> vecTagsNotFound;
		LVFINDINFO info;
		info.flags = LVFI_STRING;
		for (vector<string>::iterator it = vecTags.begin(); it != vecTags.end(); it++)
		{
			// Find each value in the list
			info.psz = it->c_str();
			int iIndex = m_listTags.FindItem(&info);
			if (iIndex == -1)
			{
				// Tag was not found, add to the list of not found
				vecTagsNotFound.push_back(*it);
			}
			else
			{
				// Set this item as checked
				m_listTags.SetCheck(iIndex, TRUE);
			}
		}

		if (vecTagsNotFound.size() > 0)
		{
			// Prompt the user that there were tags that no longer exist
			string strMessage = "The following tag(s) no longer exist in the database:\n";
			for (vector<string>::iterator it = vecTagsNotFound.begin();
				it != vecTagsNotFound.end(); it++)
			{
				strMessage += (*it) + "\n";
			}

			MessageBox(strMessage.c_str(), "Tags Not Found", MB_OK | MB_ICONINFORMATION);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33820");
}
//-------------------------------------------------------------------------------------------------
void FileTagConditionDlg::configureAndPopulateTagList()
{
	try
	{
		// Enable full row selection plus grid lines and checkboxes
		// for the tags list control
		m_listTags.SetExtendedStyle(LVS_EX_GRIDLINES | 
			LVS_EX_FULLROWSELECT | LVS_EX_CHECKBOXES);

		// Build column information struct
		LVCOLUMN lvColumn;
		lvColumn.mask = LVCF_FMT | LVCF_TEXT | LVCF_WIDTH;
		lvColumn.fmt = LVCFMT_LEFT;

		CRect recList;
		m_listTags.GetWindowRect(&recList);

		// Set information for tag name column
		lvColumn.pszText = "Tag name";
		lvColumn.cx = recList.Width() - 4; // Remove 4 pixels for the column divider

		// Get the list of tag names
		IVariantVectorPtr ipVecTagNames = m_ipFAMDB->GetTagNames();
		ASSERT_RESOURCE_ALLOCATION("ELI33821", ipVecTagNames != __nullptr);

		// Get the count of items
		long nSize = ipVecTagNames->Size;

		// Check if need to add space for scroll bar
		if (nSize > m_listTags.GetCountPerPage())
		{
			// Get the scroll bar width, if 0 and error occurred, just log an
			// application trace and set the width to 17
			int nVScrollWidth = GetSystemMetrics(SM_CXVSCROLL);
			if (nVScrollWidth == 0)
			{
				UCLIDException ue("ELI33822", "Application Trace: Unable to determine scroll bar width.");
				ue.log();

				nVScrollWidth = 17;
			}

			// Deduct space for the scroll bar from the width
			lvColumn.cx -= nVScrollWidth;
		}

		// Add the tag name column
		m_listTags.InsertColumn(0, &lvColumn);

		// Now add each tag name to the control
		for (long i=0; i < nSize; i++)
		{
			_bstr_t bstrTag(ipVecTagNames->Item[i]);

			m_listTags.InsertItem(i, (const char*)bstrTag);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33823");
}