// ManageUserCountersDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ManageUserCountersDlg.h"

#include <SuspendWindowUpdates.h>
#include <UCLIDException.h>
#include <COMUtils.h>

#include <vector>
#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const int giCOUNTER_COLUMN = 0;
static const int giVALUE_COLUMN = 1;

static const int giCOUNTER_COLUMN_WIDTH = 150;

//-------------------------------------------------------------------------------------------------
// CAddModifyUserCountersDlg dialog
//-------------------------------------------------------------------------------------------------
CManageUserCountersDlg::CAddModifyUserCountersDlg::CAddModifyUserCountersDlg(const string &strCounterName,
													 LONGLONG llCounterValue, bool bAllowModifyName,
													 bool bAllowModifyValue, CWnd *pParent) :
CDialog(CManageUserCountersDlg::CAddModifyUserCountersDlg::IDD, pParent),
m_strCounterName(strCounterName),
m_llValue(llCounterValue),
m_bEnableCounterName(bAllowModifyName),
m_bEnableCounterValue(bAllowModifyValue)
{
}
//-------------------------------------------------------------------------------------------------
CManageUserCountersDlg::CAddModifyUserCountersDlg::~CAddModifyUserCountersDlg()
{
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::CAddModifyUserCountersDlg::DoDataExchange(CDataExchange *pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_EDIT_COUNTER_NAME, m_editCounterName);
	DDX_Control(pDX, IDC_EDIT_COUNTER_VALUE, m_editCounterValue);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CManageUserCountersDlg::CAddModifyUserCountersDlg, CDialog)
	ON_BN_CLICKED(IDC_BTN_ADD_COUNTER_OK, CManageUserCountersDlg::CAddModifyUserCountersDlg::OnBtnOK)
	ON_BN_CLICKED(IDC_BTN_ADD_COUNTER_CANCEL, CManageUserCountersDlg::CAddModifyUserCountersDlg::OnBtnCancel)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
BOOL CManageUserCountersDlg::CAddModifyUserCountersDlg::OnInitDialog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Set the counter name and the value
		m_editCounterName.SetWindowText(m_strCounterName.c_str());
		m_editCounterValue.SetWindowText(asString(m_llValue).c_str());

		if (m_bEnableCounterName)
		{
			// If the counter field is enabled then the dialog is being used
			// to either add a counter or rename a counter, set the
			// title based on whether the counter value is modifyable
			this->SetWindowText(m_bEnableCounterValue ? "Add Counter" : "Rename Counter");
		}
		else
		{
			// The dialog is being used to set the counter value
			this->SetWindowText("Set Counter Value");
		}

		// Enable/disable the controls appropriately
		m_editCounterName.EnableWindow(asMFCBool(m_bEnableCounterName));
		m_editCounterValue.EnableWindow(asMFCBool(m_bEnableCounterValue));

		// Limit the text in the edit boxes
		m_editCounterName.SetLimitText(50);
		m_editCounterValue.SetLimitText(22);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27788");

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::CAddModifyUserCountersDlg::OnOK()
{
	// Allow the user to use the enter key to close the dialog
	OnBtnOK();
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::CAddModifyUserCountersDlg::OnBtnOK()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (m_bEnableCounterName)
		{
			// Ensure a counter name has been specified
			CString zCounterName;
			m_editCounterName.GetWindowText(zCounterName);

			if (zCounterName.IsEmpty())
			{
				MessageBox("A counter name must be specified!", "No Counter Name", MB_OK | MB_ICONERROR);
				m_editCounterName.SetFocus();

				return;
			}

			m_strCounterName = (LPCTSTR) zCounterName;
		}

		if (m_bEnableCounterValue)
		{
			// Get the value
			CString zValue;
			m_editCounterValue.GetWindowText(zValue);

			// Try to convert the value to a long
			try
			{
				m_llValue = asLongLong((LPCTSTR) zValue);

				// Ensure the value was not bigger or smaller than a longlong
				if ((m_llValue == _I64_MAX || m_llValue == _I64_MIN) && errno == ERANGE)
				{
					string strMessage = "The value you entered is too large, number must be between: "
						+ asString(_I64_MAX) + " and " + asString(_I64_MIN) + ".";
					MessageBox(strMessage.c_str(), "Number Out Of Range", MB_OK | MB_ICONWARNING);
					m_editCounterValue.SetSel(0, -1);
					m_editCounterValue.SetFocus();

					return;
				}
			}
			catch(...)
			{
				MessageBox("Invalid counter value: Must specify a number!",
					"Invalid Counter Value", MB_OK | MB_ICONERROR);
				m_editCounterValue.SetSel(0, -1);
				m_editCounterValue.SetFocus();

				return;
			}
		}

		// Close the dialog and return OK
		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27789");
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::CAddModifyUserCountersDlg::OnBtnCancel()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Close the dialog and return cancel
		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27790");
}

//-------------------------------------------------------------------------------------------------
// CManageUserCountersDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CManageUserCountersDlg, CDialog)
//-------------------------------------------------------------------------------------------------
CManageUserCountersDlg::CManageUserCountersDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr &ipFAMDB,
							   CWnd* pParent) :
CDialog(CManageUserCountersDlg::IDD, pParent),
m_ipFAMDB(ipFAMDB)
{
	ASSERT_ARGUMENT("ELI27791", ipFAMDB != __nullptr);
}
//-------------------------------------------------------------------------------------------------
CManageUserCountersDlg::~CManageUserCountersDlg()
{
	try
	{
		// Ensure FamDB pointer is released
		m_ipFAMDB = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27792");
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::DoDataExchange(CDataExchange *pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_BTN_ADD_COUNTER, m_btnAdd);
	DDX_Control(pDX, IDC_BTN_RENAME_COUNTER, m_btnRename);
	DDX_Control(pDX, IDC_BTN_DELETE_COUNTER, m_btnDelete);
	DDX_Control(pDX, IDC_BTN_SET_COUNTER_VALUE, m_btnSetValue);
	DDX_Control(pDX, IDC_BTN_REFRESH_COUNTERS, m_btnRefresh);
	DDX_Control(pDX, IDC_LIST_USER_COUNTERS, m_listCounters);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CManageUserCountersDlg, CDialog)
	ON_BN_CLICKED(IDC_BTN_ADD_COUNTER, &CManageUserCountersDlg::OnBtnAdd)
	ON_BN_CLICKED(IDC_BTN_RENAME_COUNTER, &CManageUserCountersDlg::OnBtnRename)
	ON_BN_CLICKED(IDC_BTN_SET_COUNTER_VALUE, &CManageUserCountersDlg::OnBtnSetValue)
	ON_BN_CLICKED(IDC_BTN_DELETE_COUNTER, &CManageUserCountersDlg::OnBtnDelete)
	ON_BN_CLICKED(IDC_BTN_REFRESH_COUNTERS, &CManageUserCountersDlg::OnBtnRefresh)
	ON_BN_CLICKED(IDC_BTN_COUNTERS_CLOSE, &CManageUserCountersDlg::OnBtnClose)
	ON_NOTIFY(NM_DBLCLK, IDC_LIST_USER_COUNTERS, &CManageUserCountersDlg::OnNMDblclkList)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_USER_COUNTERS, &CManageUserCountersDlg::OnLvnItemchangedList)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
BOOL CManageUserCountersDlg::PreTranslateMessage(MSG *pMsg)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (pMsg->message == WM_KEYDOWN)
		{
			// translate accelerators
			static HACCEL hAccel = LoadAccelerators(AfxGetApp()->m_hInstance, 
				MAKEINTRESOURCE(IDR_MANAGE_COUNTER_ACCELERATORS));
			if (TranslateAccelerator(m_hWnd, hAccel, pMsg))
			{
				// since the message has been handled, no further dispatch is needed
				return TRUE;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27793")

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
BOOL CManageUserCountersDlg::OnInitDialog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Configure the tag column
		configureCounterList();

		// Populate the list
		refreshCounterList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27794");

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::OnBtnAdd()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Create the add/modify dialog, set the initial value to 0 and enable all controls
		CAddModifyUserCountersDlg dlg("", 0, true, true);

		// Display the dialog
		if (dlg.DoModal() == IDOK)
		{
			// Show the wait cursor while updating the dialog
			CWaitCursor wait;

			// Get the counter name and value from the dialog
			// (Trim leading and trailing whitespace from the name [LRCAU #5516])
			string strCounterName = trim(dlg.getCounterName(), " \t", " \t");
			LONGLONG llValue = dlg.getValue();

			// Add the new counter
			m_ipFAMDB->AddUserCounter(strCounterName.c_str(), llValue);

			// Log an application trace about the database change
			UCLIDException uex("ELI27795", "Application trace: Database change");
			uex.addDebugInfo("Change", "Add user counter");
			uex.addDebugInfo("User Name", getCurrentUserName());
			uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
			uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));
			uex.addDebugInfo("Counter Name", strCounterName);
			uex.addDebugInfo("Counter Value", llValue);
			uex.log();

			// Refresh list for new tag
			refreshCounterList();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27796");
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::OnBtnRename()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Get the currently selected item
		POSITION pos = m_listCounters.GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			return;
		}

		int iIndex = m_listCounters.GetNextSelectedItem(pos);
		if (iIndex != -1)
		{
			// Get the strings from the current selection
			CString zCounterName = m_listCounters.GetItemText(iIndex, giCOUNTER_COLUMN);
			CString zValue = m_listCounters.GetItemText(iIndex, giVALUE_COLUMN);
			long nValue = asLong((LPCTSTR)zValue);

			// Create the dialog and enable the name control
			CAddModifyUserCountersDlg dlg((LPCTSTR)zCounterName, nValue, true, false);

			// Display the dialog
			if (dlg.DoModal() == IDOK)
			{
				// Show the wait cursor while updating the dialog
				CWaitCursor wait;

				string strNewCounterName = dlg.getCounterName();

				// Modify the existing tag
				m_ipFAMDB->RenameUserCounter((LPCTSTR)zCounterName, strNewCounterName.c_str());

				// Log an application trace about the database change
				UCLIDException uex("ELI27797", "Application trace: Database change");
				uex.addDebugInfo("Change", "Rename user counter");
				uex.addDebugInfo("User Name", getCurrentUserName());
				uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
				uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));
				uex.addDebugInfo("Original Counter Name", (LPCTSTR) zCounterName);
				uex.addDebugInfo("New Counter Name", strNewCounterName);
				uex.log();

				// Refresh list for modified tag
				refreshCounterList(strNewCounterName);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27798");
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::OnBtnSetValue()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Get the currently selected item
		POSITION pos = m_listCounters.GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			return;
		}

		int iIndex = m_listCounters.GetNextSelectedItem(pos);
		if (iIndex != -1)
		{
			// Get the strings from the current selection
			CString zCounterName = m_listCounters.GetItemText(iIndex, giCOUNTER_COLUMN);
			CString zValue = m_listCounters.GetItemText(iIndex, giVALUE_COLUMN);
			LONGLONG llValue = asLongLong((LPCTSTR)zValue);

			// Create the dialog and enable the value control
			CAddModifyUserCountersDlg dlg((LPCTSTR)zCounterName, llValue, false, true);

			// Display the dialog
			if (dlg.DoModal() == IDOK)
			{
				// Show the wait cursor while updating the dialog
				CWaitCursor wait;

				LONGLONG llNewValue = dlg.getValue();

				// Modify the existing tag
				m_ipFAMDB->SetUserCounterValue((LPCTSTR)zCounterName, llNewValue);

				// Log an application trace about the database change
				UCLIDException uex("ELI27799", "Application trace: Database change");
				uex.addDebugInfo("Change", "Set user counter value");
				uex.addDebugInfo("User Name", getCurrentUserName());
				uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
				uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));
				uex.addDebugInfo("Counter Name", (LPCTSTR) zCounterName);
				uex.addDebugInfo("Original Counter value", llValue);
				uex.addDebugInfo("New Counter value", llNewValue);
				uex.log();

				// Refresh list
				refreshCounterList((LPCTSTR)zCounterName);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27800");
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::OnBtnDelete()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// If there are no selected items or the user said no to delete then just return
		if (m_listCounters.GetSelectedCount() == 0
			|| MessageBox("Delete the selected counter(s)?", "Delete Counters?",
			MB_YESNO | MB_ICONQUESTION) == IDNO)
		{
			// Nothing to do
			return;
		}

		// Build the list of tags to delete
		vector<string> vecCountersToDelete;
		POSITION pos = m_listCounters.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			int iIndex = m_listCounters.GetNextSelectedItem( pos );

			// Loop through selected items and add each item to the vector
			while (iIndex != -1)
			{
				vecCountersToDelete.push_back((LPCTSTR) m_listCounters.GetItemText(iIndex, giCOUNTER_COLUMN));

				iIndex = m_listCounters.GetNextSelectedItem(pos);
			}
		}

		// Log an application trace about the database change
		UCLIDException uex("ELI27801", "Application trace: Database change");
		uex.addDebugInfo("Change", "Delete file counter(s)");
		uex.addDebugInfo("User Name", getCurrentUserName());
		uex.addDebugInfo("Server Name", asString(m_ipFAMDB->DatabaseServer));
		uex.addDebugInfo("Database", asString(m_ipFAMDB->DatabaseName));
		string strCounters = "";
		for (vector<string>::iterator it = vecCountersToDelete.begin();
			it != vecCountersToDelete.end(); it++)
		{
			m_ipFAMDB->RemoveUserCounter(it->c_str());

			if (!strCounters.empty())
			{
				strCounters += ", ";
			}
			strCounters += (*it);
		}
		uex.addDebugInfo("User Counter(s)", strCounters);
		uex.log();

		refreshCounterList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27802");
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::OnBtnRefresh()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		refreshCounterList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27803");
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::OnBtnClose()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Call dialog OnOK
		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27804");
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::OnNMDblclkList(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Same as clicking set value
		OnBtnSetValue();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27805");
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::OnLvnItemchangedList(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Update controls when selection changes
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27806");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::updateControls()
{
	try
	{
		// Get the selection count
		unsigned int iCount = m_listCounters.GetSelectedCount();

		// Enable delete if at least 1 item selected
		// Enable modify iff 1 item selected
		BOOL bEnableDelete = asMFCBool(iCount > 0);
		BOOL bEnableModify = asMFCBool(iCount == 1);

		m_btnDelete.EnableWindow(bEnableDelete);
		m_btnRename.EnableWindow(bEnableModify);
		m_btnSetValue.EnableWindow(bEnableModify);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27807");
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::configureCounterList()
{
	try
	{
		// Set list style
		m_listCounters.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);

		// Build column information struct
		LVCOLUMN lvColumn;
		lvColumn.mask = LVCF_FMT | LVCF_TEXT | LVCF_WIDTH;
		lvColumn.fmt = LVCFMT_LEFT;

		// Set information for tag name column
		lvColumn.pszText = "Counter name";
		lvColumn.cx = giCOUNTER_COLUMN_WIDTH;

		// Add the tag name column
		m_listCounters.InsertColumn(giCOUNTER_COLUMN, &lvColumn);

		// Get dimensions of list control
		CRect recList;
		m_listCounters.GetClientRect(&recList);

		// Set heading and compute width for description column
		lvColumn.pszText = "Value";
		lvColumn.cx = recList.Width() - giCOUNTER_COLUMN_WIDTH;

		// Add the description column
		m_listCounters.InsertColumn(giVALUE_COLUMN, &lvColumn);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27808");
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::refreshCounterList(const string& strNameToSelect)
{
	try
	{
		SuspendWindowUpdates updater(*this);

		// Store the position of the first selected item
		int nSelectedItem = 0;
		POSITION pos = m_listCounters.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			nSelectedItem = m_listCounters.GetNextSelectedItem(pos);
		}
		
		clearListSelection();

		// Clear the current counter list
		m_listCounters.DeleteAllItems();

		IStrToStrMapPtr ipCounters = m_ipFAMDB->GetUserCounterNamesAndValues();
		ASSERT_RESOURCE_ALLOCATION("ELI27809", ipCounters != __nullptr);

		// Get the number of counters
		long lSize = ipCounters->Size;

		// Need to ensure the value column is the correct width
		CRect recList;
		m_listCounters.GetClientRect(&recList);
		int nValueWidth = recList.Width() - giCOUNTER_COLUMN_WIDTH;

		// Check if need to add space for scroll bar
		if (lSize > m_listCounters.GetCountPerPage())
		{
			// Get the scroll bar width, if 0 and error occurred, just log an
			// application trace and set the width to 17
			int nVScrollWidth = GetSystemMetrics(SM_CXVSCROLL);
			if (nVScrollWidth == 0)
			{
				UCLIDException ue("ELI27810", "Application Trace: Unable to determine scroll bar width.");
				ue.log();

				nVScrollWidth = 17;
			}

			// Deduct space for the scroll bar from the width
			nValueWidth -= nVScrollWidth;
		}

		// Set the width of the value column
		m_listCounters.SetColumnWidth(giVALUE_COLUMN, nValueWidth);

		// Populate the list
		for (long i=0; i < lSize; i++)
		{
			// Get the counter name and value
			_bstr_t bstrCounterName;
			_bstr_t bstrCounterValue;
			ipCounters->GetKeyValue(i, bstrCounterName.GetAddress(), bstrCounterValue.GetAddress());

			// If selecting a particular entry, update the selected item value with
			// its index
			if (!strNameToSelect.empty() && strNameToSelect == asString(bstrCounterName))
			{
				nSelectedItem = i;
			}

			// Add the info into the list
			m_listCounters.InsertItem(i, (const char*)bstrCounterName);
			m_listCounters.SetItemText(i, giVALUE_COLUMN,
				commaFormatNumber(asLongLong(asString(bstrCounterValue))).c_str());
		}

		// Select either the last selected item position or select the first item
		m_listCounters.SetItemState(nSelectedItem < lSize ? nSelectedItem : 0,
			LVIS_SELECTED, LVIS_SELECTED);

		updateControls();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27811");
}
//-------------------------------------------------------------------------------------------------
void CManageUserCountersDlg::clearListSelection()
{
	try
	{
		POSITION pos = m_listCounters.GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			// no item selected, return
			return;
		}

		// iterate through all selections and set them as unselected.  pos will be
		// set to NULL by GetNextSelectedItem if there are no more selected items
		while (pos)
		{
			int nItemSelected = m_listCounters.GetNextSelectedItem(pos);
			m_listCounters.SetItemState(nItemSelected, 0, LVIS_SELECTED);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27812");
}
//-------------------------------------------------------------------------------------------------

