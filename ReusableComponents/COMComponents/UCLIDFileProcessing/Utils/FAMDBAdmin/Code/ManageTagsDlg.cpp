// ManageTagsDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ManageTagsDlg.h"

#include <SuspendWindowUpdates.h>
#include <UCLIDException.h>

#include <vector>
#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const int giTAG_COLUMN = 0;
static const int giDESCRIPTION_COLUMN = 1;

static const int giTAG_COLUMN_WIDTH = 150;

//-------------------------------------------------------------------------------------------------
// CAddModifyTagsDlg dialog
//-------------------------------------------------------------------------------------------------
CManageTagsDlg::CAddModifyTagsDlg::CAddModifyTagsDlg(const string &strTagToModify,
													 const string &strDescriptionToModify,
													 CWnd *pParent) :
CDialog(CManageTagsDlg::CAddModifyTagsDlg::IDD, pParent),
m_strTagName(strTagToModify),
m_strDescription(strDescriptionToModify)
{
}
//-------------------------------------------------------------------------------------------------
CManageTagsDlg::CAddModifyTagsDlg::~CAddModifyTagsDlg()
{
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::CAddModifyTagsDlg::DoDataExchange(CDataExchange *pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_EDIT_TAG_NAME, m_editTagName);
	DDX_Control(pDX, IDC_EDIT_TAG_DESCRIPTION, m_editDescription);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CManageTagsDlg::CAddModifyTagsDlg, CDialog)
	ON_BN_CLICKED(IDC_BTN_ADD_TAG_OK, CManageTagsDlg::CAddModifyTagsDlg::OnBtnOK)
	ON_BN_CLICKED(IDC_BTN_ADD_TAG_CANCEL, CManageTagsDlg::CAddModifyTagsDlg::OnBtnCancel)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::CAddModifyTagsDlg::OnOK()
{
	// Do nothing - stubbed in to prevent close on enter
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::CAddModifyTagsDlg::OnCancel()
{
	// Do nothing - stubbed in to prevent close on escape
}
//-------------------------------------------------------------------------------------------------
BOOL CManageTagsDlg::CAddModifyTagsDlg::OnInitDialog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Set the tag name if the string is non-empty
		if (!m_strTagName.empty())
		{
			m_editTagName.SetWindowText(m_strTagName.c_str());

			// If tag name was specified this is a modify operation, change
			// the caption to Modify tag
			this->SetWindowText("Modify tag");
		}

		// Set the description if the string is non-empty
		if (!m_strDescription.empty())
		{
			m_editDescription.SetWindowText(m_strDescription.c_str());
		}

		// Limit the text in the edit boxes
		m_editTagName.SetLimitText(100);
		m_editDescription.SetLimitText(255);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27413");

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::CAddModifyTagsDlg::OnBtnOK()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Ensure a tag name has been specified
		CString zTagName;
		m_editTagName.GetWindowText(zTagName);

		if (zTagName.IsEmpty())
		{
			MessageBox("A tag name must be specified!", "No Tag Name", MB_OK | MB_ICONERROR);
			m_editTagName.SetFocus();

			return;
		}

		// Get the description
		CString zDescription;
		m_editDescription.GetWindowText(zDescription);

		// Set the string values
		m_strTagName = (LPCTSTR) zTagName;
		m_strDescription = (LPCTSTR) zDescription;

		// Close the dialog and return OK
		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27414");
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::CAddModifyTagsDlg::OnBtnCancel()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Close the dialog and return cancel
		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27415");
}

//-------------------------------------------------------------------------------------------------
// CManageTagsDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CManageTagsDlg, CDialog)
//-------------------------------------------------------------------------------------------------
CManageTagsDlg::CManageTagsDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr &ipFAMDB,
							   CWnd* pParent) :
CDialog(CManageTagsDlg::IDD, pParent),
m_ipFAMDB(ipFAMDB)
{
	ASSERT_ARGUMENT("ELI27390", ipFAMDB != NULL);
}
//-------------------------------------------------------------------------------------------------
CManageTagsDlg::~CManageTagsDlg()
{
	try
	{
		// Ensure FamDB pointer is released
		m_ipFAMDB = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27391");
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::DoDataExchange(CDataExchange *pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_BTN_ADD_TAG, m_btnAdd);
	DDX_Control(pDX, IDC_BTN_MODIFY_TAG, m_btnModify);
	DDX_Control(pDX, IDC_BTN_DELETE_TAGS, m_btnDelete);
	DDX_Control(pDX, IDC_BTN_REFRESH_TAGS, m_btnRefresh);
	DDX_Control(pDX, IDC_LIST_TAGS, m_listTags);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CManageTagsDlg, CDialog)
	ON_BN_CLICKED(IDC_BTN_ADD_TAG, &CManageTagsDlg::OnBtnAdd)
	ON_BN_CLICKED(IDC_BTN_MODIFY_TAG, &CManageTagsDlg::OnBtnModify)
	ON_BN_CLICKED(IDC_BTN_DELETE_TAGS, &CManageTagsDlg::OnBtnDelete)
	ON_BN_CLICKED(IDC_BTN_REFRESH_TAGS, &CManageTagsDlg::OnBtnRefresh)
	ON_BN_CLICKED(IDC_BTN_TAGS_CLOSE, &CManageTagsDlg::OnBtnClose)
	ON_NOTIFY(NM_DBLCLK, IDC_LIST_TAGS, &CManageTagsDlg::OnNMDblclkList)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_TAGS, &CManageTagsDlg::OnLvnItemchangedList)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::OnOK()
{
	// Stubbed in to prevent close on enter
}
//-------------------------------------------------------------------------------------------------
BOOL CManageTagsDlg::PreTranslateMessage(MSG *pMsg)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (pMsg->message == WM_KEYDOWN)
		{
			// translate accelerators
			static HACCEL hAccel = LoadAccelerators(AfxGetApp()->m_hInstance, 
				MAKEINTRESOURCE(IDR_MANAGE_TAG_ACCELERATORS));
			if (TranslateAccelerator(m_hWnd, hAccel, pMsg))
			{
				// since the message has been handled, no further dispatch is needed
				return TRUE;
			}

			// Find and eat an Escape character
			if (pMsg->wParam == VK_ESCAPE)
			{
				return TRUE;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27392")

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
BOOL CManageTagsDlg::OnInitDialog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Configure the tag column
		configureTagList();

		// Populate the list
		refreshTagList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27393");

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::OnBtnAdd()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CAddModifyTagsDlg dlg;
		
		if (dlg.DoModal() == IDOK)
		{
			// Get the tag name and description from the dialog
			string strTagName = dlg.getTagName();
			string strDescription = dlg.getDescription();

			// Add the new tag
			m_ipFAMDB->AddTag(strTagName.c_str(), strDescription.c_str());

			// Refresh list for new tag
			refreshTagList();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27394");
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::OnBtnModify()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Get the currently selected item
		POSITION pos = m_listTags.GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			return;
		}

		int iIndex = m_listTags.GetNextSelectedItem(pos);
		if (iIndex != -1)
		{
			// Get the strings from the current selection
			CString zTagName = m_listTags.GetItemText(iIndex, giTAG_COLUMN);
			CString zDescription = m_listTags.GetItemText(iIndex, giDESCRIPTION_COLUMN);

			CAddModifyTagsDlg dlg((LPCTSTR)zTagName, (LPCTSTR) zDescription);

			if (dlg.DoModal() == IDOK)
			{
				// Modify the existing tag
				m_ipFAMDB->ModifyTag((LPCTSTR)zTagName, dlg.getTagName().c_str(),
					dlg.getDescription().c_str());

				// Refresh list for modified tag
				refreshTagList();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27395");
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::OnBtnDelete()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// If there are no selected items or the user said no to delete then just return
		if (m_listTags.GetSelectedCount() == 0
			|| MessageBox("Delete the selected tag(s)?", "Delete Tags?",
			MB_YESNO | MB_ICONQUESTION) == IDNO)
		{
			// Nothing to do
			return;
		}

		// Build the list of tags to delete
		vector<string> vecTagsToDelete;
		POSITION pos = m_listTags.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			// Get index of first selection
			int iIndex = m_listTags.GetNextSelectedItem( pos );

			// Loop through selected items and add each item to the vector
			while (iIndex != -1)
			{
				vecTagsToDelete.push_back((LPCTSTR) m_listTags.GetItemText(iIndex, giTAG_COLUMN));

				iIndex = m_listTags.GetNextSelectedItem(pos);
			}
		}

		for (vector<string>::iterator it = vecTagsToDelete.begin();
			it != vecTagsToDelete.end(); it++)
		{
			m_ipFAMDB->DeleteTag(it->c_str());
		}

		refreshTagList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27396");
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::OnBtnRefresh()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		refreshTagList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27397");
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::OnBtnClose()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Call dialog OnOK
		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27399");
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::OnNMDblclkList(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Same as clicking modify
		OnBtnModify();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27400");
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::OnLvnItemchangedList(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update controls when selection changes
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27401");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::updateControls()
{
	try
	{
		// Get the selection count
		unsigned int iCount = m_listTags.GetSelectedCount();

		// Enable delete if at least 1 item selected
		// Enable modify iff 1 item selected
		BOOL bEnableDelete = asMFCBool(iCount > 0);
		BOOL bEnableModify = asMFCBool(iCount == 1);

		m_btnDelete.EnableWindow(bEnableDelete);
		m_btnModify.EnableWindow(bEnableModify);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27402");
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::configureTagList()
{
	try
	{
		// Set list style
		m_listTags.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);

		// Build column information struct
		LVCOLUMN lvColumn;
		lvColumn.mask = LVCF_FMT | LVCF_TEXT | LVCF_WIDTH;
		lvColumn.fmt = LVCFMT_LEFT;

		// Set information for tag name column
		lvColumn.pszText = "Tag name";
		lvColumn.cx = giTAG_COLUMN_WIDTH;

		// Add the tag name column
		m_listTags.InsertColumn(giTAG_COLUMN, &lvColumn);

		// Get dimensions of list control
		CRect recList;
		m_listTags.GetClientRect(&recList);

		// Set heading and compute width for description column
		lvColumn.pszText = "Description";
		lvColumn.cx = recList.Width() - giTAG_COLUMN_WIDTH;

		// Add the description column
		m_listTags.InsertColumn(giDESCRIPTION_COLUMN, &lvColumn);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27403");
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::refreshTagList()
{
	try
	{
		SuspendWindowUpdates updater(*this);

		// Store the position of the first selected item
		int nSelectedItem = 0;
		POSITION pos = m_listTags.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			nSelectedItem = m_listTags.GetNextSelectedItem(pos);
		}
		
		clearListSelection();

		// Clear the current tag list
		m_listTags.DeleteAllItems();

		IStrToStrMapPtr ipTags = m_ipFAMDB->GetTags();
		ASSERT_RESOURCE_ALLOCATION("ELI27404", ipTags != NULL);

		// Get the number of tags
		long lSize = ipTags->Size;

		// Need to ensure the description column is the correct width
		CRect recList;
		m_listTags.GetClientRect(&recList);
		int nDescriptionWidth = recList.Width() - giTAG_COLUMN_WIDTH;

		// Check if need to add space for scroll bar
		if (lSize > m_listTags.GetCountPerPage())
		{
			// Get the scroll bar width, if 0 and error occurred, just log an
			// application trace and set the width to 17
			int nVScrollWidth = GetSystemMetrics(SM_CXVSCROLL);
			if (nVScrollWidth == 0)
			{
				UCLIDException ue("ELI27408", "Application Trace: Unable to determine scroll bar width.");
				ue.log();

				nVScrollWidth = 17;
			}

			// Deduct space for the scroll bar from the width
			nDescriptionWidth -= nVScrollWidth;
		}

		// Set the width of the description column
		m_listTags.SetColumnWidth(giDESCRIPTION_COLUMN, nDescriptionWidth);

		// Populate the list
		for (long i=0; i < lSize; i++)
		{
			// Get the tag name and description
			_bstr_t bstrTagName;
			_bstr_t bstrTagDescription;
			ipTags->GetKeyValue(i, bstrTagName.GetAddress(), bstrTagDescription.GetAddress());

			// Add the info into the list
			m_listTags.InsertItem(i, (const char*)bstrTagName);
			m_listTags.SetItemText(i, giDESCRIPTION_COLUMN, (const char*)bstrTagDescription);
		}

		// Select either the last selected item position or select the first item
		m_listTags.SetItemState(nSelectedItem < lSize ? nSelectedItem : 0,
			LVIS_SELECTED, LVIS_SELECTED);

		updateControls();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27405");
}
//-------------------------------------------------------------------------------------------------
void CManageTagsDlg::clearListSelection()
{
	try
	{
		POSITION pos = m_listTags.GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			// no item selected, return
			return;
		}

		// iterate through all selections and set them as unselected.  pos will be
		// set to NULL by GetNextSelectedItem if there are no more selected items
		while (pos)
		{
			int nItemSelected = m_listTags.GetNextSelectedItem(pos);
			m_listTags.SetItemState(nItemSelected, 0, LVIS_SELECTED);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27406");
}
//-------------------------------------------------------------------------------------------------

