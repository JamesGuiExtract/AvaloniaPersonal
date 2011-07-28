// MergeAttributesPreferenceListDlg.cpp : Implementation of CMergeAttributesPreferenceListDlg

#include "stdafx.h"
#include "MergeAttributesPreferenceListDlg.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <ComUtils.h>

//--------------------------------------------------------------------------------------------------
// CMergeAttributesPreferenceListDlg
//--------------------------------------------------------------------------------------------------
CMergeAttributesPreferenceListDlg::CMergeAttributesPreferenceListDlg(const string& strDialogTitle,
	vector<string> &vecListValues, bool bValidateAsIdentifier, bool &rbTreatAsRegex)
: m_strDialogTitle(strDialogTitle)
, m_vecListValues(vecListValues)
, m_bValidateAsIdentifier(bValidateAsIdentifier)
, m_bTreatAsRegEx(rbTreatAsRegex)
{
}
//--------------------------------------------------------------------------------------------------
CMergeAttributesPreferenceListDlg::~CMergeAttributesPreferenceListDlg()
{
}
//--------------------------------------------------------------------------------------------------
bool CMergeAttributesPreferenceListDlg::EditList(const string& strDialogTitle,
				vector<string> &rvecListValues, bool bValidateAsIdentifier, bool &rbTreatAsRegex)
{
	vector<string> vecTempListValues = rvecListValues;
	bool bTempTreatAsRegex = rbTreatAsRegex;
	CMergeAttributesPreferenceListDlg dlg(strDialogTitle, vecTempListValues, bValidateAsIdentifier,
		bTempTreatAsRegex);

	if (dlg.DoModal() == IDOK)
	{
		rvecListValues = vecTempListValues;
		rbTreatAsRegex = bTempTreatAsRegex;

		return true;
	}

	return false;
}

//--------------------------------------------------------------------------------------------------
// Windows message handlers
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPreferenceListDlg::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam,
														BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CAxDialogImpl<CMergeAttributesPreferenceListDlg>::OnInitDialog(uMsg, wParam, lParam, bHandled);
		bHandled = TRUE;

		SetWindowText(m_strDialogTitle.c_str());

		// Map controls to member variables
		m_listMergePriority			= GetDlgItem(IDC_LIST_NAMES);
		m_btnAdd					= GetDlgItem(IDC_BTN_ADD_NAME);
		m_btnModify					= GetDlgItem(IDC_BTN_MODIFY_NAME);
		m_btnRemove					= GetDlgItem(IDC_BTN_REMOVE_NAME);
		m_btnUp.SubclassDlgItem(IDC_BTN_NAME_UP, CWnd::FromHandle(m_hWnd));
		m_btnDown.SubclassDlgItem(IDC_BTN_NAME_DOWN, CWnd::FromHandle(m_hWnd));
		m_btnTreatAsRegEx			= GetDlgItem(IDC_CHECK_REGEX);
		
		// Assign icons to the up and down buttons
		m_btnUp.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_UP)));
		m_btnDown.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_DOWN)));

		// Load the provided data.
		initializeList(m_vecListValues);
		m_btnTreatAsRegEx.SetCheck(asBSTChecked(m_bTreatAsRegEx));

		updateButtons();

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33062");

	return 1;  // Let the system set the focus
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPreferenceListDlg::OnClickedOK(WORD wNotifyCode, WORD wID, HWND hWndCtl,
													   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Update the values and close the dialog.
		m_vecListValues = retrieveListValues();
		m_bTreatAsRegEx = (m_btnTreatAsRegEx.GetCheck() == BST_CHECKED);

		EndDialog(wID);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33063");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPreferenceListDlg::OnClickedCancel(WORD wNotifyCode, WORD wID, HWND hWndCtl,
														   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// If cancelling, don't update values before ending the dialog.
		EndDialog(wID);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33064");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPreferenceListDlg::OnBnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
											  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Prompt user to enter a new value
		CString zValue;
		if (promptForValue(zValue, m_listMergePriority, "", -1))
		{
			int nTotal = m_listMergePriority.GetItemCount();
				
			// new item index
			int nIndex = m_listMergePriority.InsertItem(nTotal, zValue);
			for (int i = 0; i <= nTotal; i++)
			{
				// deselect any other items
				int nState = (i == nIndex) ? LVIS_SELECTED : 0;

				// only select current newly added item
				m_listMergePriority.SetItemState(i, nState, LVIS_SELECTED);
			}

			// adjust the column width in case there is a vertical scrollbar
			CRect rect;
			m_listMergePriority.GetClientRect(&rect);			
			m_listMergePriority.SetColumnWidth(0, rect.Width());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33065");

	// Call updateButtons to update the related buttons
	updateButtons();

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPreferenceListDlg::OnBnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
												 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get currently selected item
		int nSelectedItemIndex = m_listMergePriority.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex==LB_ERR)
		{
			return 0;
		}

		// Get selected text
		CComBSTR bstrValue;
		m_listMergePriority.GetItemText(nSelectedItemIndex, 0, bstrValue.m_str);

		// Prompt user to modify the current value
		CString zValue(asString(bstrValue.m_str).c_str());
		if (promptForValue(zValue, m_listMergePriority, "", nSelectedItemIndex))
		{
			// If the user OK'd the box, save the new value
			m_listMergePriority.DeleteItem(nSelectedItemIndex);
			
			int nIndex = m_listMergePriority.InsertItem(nSelectedItemIndex, zValue);

			m_listMergePriority.SetItemState(nIndex, LVIS_SELECTED, LVIS_SELECTED);

			// Call updateButtons to update the related buttons
			updateButtons();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33066");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPreferenceListDlg::OnBnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
												 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get first selected item
		int nItem = m_listMergePriority.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		
		if (nItem != -1)
		{
			int nRes = MessageBox("Delete selected item(s)?", "Confirm", MB_YESNO);
			if (nRes == IDYES)
			{
				// Remove selected items
				int nFirstItem = nItem;
				
				// Delete any selected item since this list ctrl allows multiple selection
				while(nItem != -1)
				{
					// Remove from the UI listbox
					m_listMergePriority.DeleteItem(nItem);
					// Get next item selected item
					nItem = m_listMergePriority.GetNextItem(nItem - 1, 
						((nItem == 0) ? LVNI_ALL : LVNI_BELOW) | LVNI_SELECTED);
				}
				
				// If there's more item(s) below last deleted item, then set 
				// Selection on the next item
				int nTotalNumOfItems = m_listMergePriority.GetItemCount();
				if (nFirstItem < nTotalNumOfItems)
				{
					m_listMergePriority.SetItemState(nFirstItem, LVIS_SELECTED, LVIS_SELECTED);
				}
				else if (nTotalNumOfItems > 0)
				{
					// Select the last item
					m_listMergePriority.SetItemState(nTotalNumOfItems - 1, LVIS_SELECTED, 
						LVIS_SELECTED);
				}
			}

			// Adjust the column width in case there is a vertical scrollbar now
			CRect rect;
			m_listMergePriority.GetClientRect(&rect);
			m_listMergePriority.SetColumnWidth(0, rect.Width());

			// Call updateButtons to update the related buttons
			updateButtons();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33067");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPreferenceListDlg::OnBnClickedBtnUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get current selected item index
		int nSelectedItemIndex = m_listMergePriority.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{
			// Get the index of the item right above currently selected item
			int nAboveIndex = nSelectedItemIndex - 1;
			if (nAboveIndex < 0)
			{
				return 0;
			}

			// Get selected item text from list
			CComBSTR bstrValue;
			m_listMergePriority.GetItemText(nSelectedItemIndex, 0, bstrValue.m_str);

			// Then remove the selected item
			m_listMergePriority.DeleteItem(nSelectedItemIndex);

			// Now insert the item right before the item that was above
			int nActualIndex = m_listMergePriority.InsertItem(nAboveIndex, 
				asString(bstrValue.m_str).c_str());
			
			// Keep this item selected
			m_listMergePriority.SetItemState(nActualIndex, LVIS_SELECTED, LVIS_SELECTED);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33068");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPreferenceListDlg::OnBnClickedBtnDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get current selected item index
		int nSelectedItemIndex = m_listMergePriority.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{
			// Get the index of the item right below currently selected item
			int nBelowIndex = nSelectedItemIndex + 1;
			if (nBelowIndex == m_listMergePriority.GetItemCount())
			{
				return 0;
			}

			// Get selected item text from list
			CComBSTR bstrValue;
			m_listMergePriority.GetItemText(nSelectedItemIndex, 0, bstrValue.m_str);

			// Then remove the selected item
			m_listMergePriority.DeleteItem(nSelectedItemIndex);

			// Now insert the item right before the item that was above
			int nActualIndex = m_listMergePriority.InsertItem(nBelowIndex, 
				asString(bstrValue.m_str).c_str());

			// Keep this item selected
			m_listMergePriority.SetItemState(nActualIndex, LVIS_SELECTED, LVIS_SELECTED);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33069");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPreferenceListDlg::OnItemChangedList(int idCtrl, LPNMHDR pNMHDR, BOOL& bHandle)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// A new name has been selected in the preservation priority list box.  Update the buttons
		// as appropriate for the new selection.
		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33070");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPreferenceListDlg::OnDblclkList(int idCtrl, LPNMHDR pNMHDR, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_listMergePriority.GetSelectedCount() == 1)
		{
			// If a single entry was double-clicked, open a dialog to allow editing of that entry.
			OnBnClickedBtnModify(0, 0, 0, bHandled);
		}
		else
		{
			// Open a dialog to allow a new value to be added to the list.
			OnBnClickedBtnAdd(0, 0, 0, bHandled);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33071");

	return 0;
}

//--------------------------------------------------------------------------------------------------
// Private members
//--------------------------------------------------------------------------------------------------
void CMergeAttributesPreferenceListDlg::initializeList(const vector<string> &vecEntries)
{
	m_listMergePriority.SetExtendedListViewStyle(LVS_EX_GRIDLINES|LVS_EX_FULLROWSELECT);
	CRect rect;			
	m_listMergePriority.GetClientRect(&rect);
	
	// Make the first column as wide as the whole list box
	m_listMergePriority.InsertColumn(0, "", LVCFMT_LEFT, rect.Width(), 0);

	// Add every value in ipEntries to the list.
	long nSize = vecEntries.size();
	for (int i = 0; i < nSize; i++)
	{
		m_listMergePriority.InsertItem(i, vecEntries[i].c_str());
	}

	m_listMergePriority.GetClientRect(&rect);
	// Adjust the column width in case there is a vertical scrollbar now
	m_listMergePriority.SetColumnWidth(0, rect.Width());
}
//--------------------------------------------------------------------------------------------------
vector<string> CMergeAttributesPreferenceListDlg::retrieveListValues()
{
	IVariantVectorPtr ipEntries(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI33072", ipEntries != __nullptr);
	
	vector<string> vecEntries;

	// Retrieve the entries from listControl (validate each entry)
	for (long i = 0; i < m_listMergePriority.GetItemCount(); i++)
	{
		CComBSTR bstrValue;
		m_listMergePriority.GetItemText(i, 0, bstrValue.m_str);
		string strValue = asString(bstrValue);
		
		// Ensure the value specified is a valid identifier if not treating the value as a regex.
		if (m_bValidateAsIdentifier && m_btnTreatAsRegEx.GetCheck() == BST_UNCHECKED)
		{
			validateIdentifier(strValue);
		}

		vecEntries.push_back(strValue);
	}
	
	return vecEntries;
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributesPreferenceListDlg::updateButtons()
{
	// Enable/disable up and down arrow key buttons appropriately
	m_btnUp.EnableWindow(FALSE);
	m_btnDown.EnableWindow(FALSE);
	m_btnAdd.EnableWindow(TRUE);

	// Get current selected item index
	int nSelectedItemIndex = m_listMergePriority.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
	int nSelCount = m_listMergePriority.GetSelectedCount();
	int nCount = m_listMergePriority.GetItemCount();
	
	if (nSelCount == 0)
	{
		// Modify and remove should be disabled if there are no entries selected
		m_btnModify.EnableWindow(FALSE);
		m_btnRemove.EnableWindow(FALSE);
	}
	else
	{
		// Modify should be enabled if there is exactly one entry selected
		m_btnModify.EnableWindow(asMFCBool(nSelCount == 1));
		// Remove should be enabled if there is at least one entry selected
		m_btnRemove.EnableWindow(asMFCBool(nSelCount >= 1));

		if ((nCount > 1) && (nSelCount == 1))
		{
			if (nSelectedItemIndex == 0)
			{
				// First item selected; enable down button only
				m_btnUp.EnableWindow(FALSE);
				m_btnDown.EnableWindow(TRUE);
			}
			else if (nSelectedItemIndex > 0 && nSelectedItemIndex < (nCount - 1))
			{
				// Some item other that first and last item selected; enable both buttons
				m_btnUp.EnableWindow(TRUE);
				m_btnDown.EnableWindow(TRUE);
			}
			else if (nSelectedItemIndex == (nCount - 1))
			{
				// Last item selected; enable up button only
				m_btnUp.EnableWindow(TRUE);
				m_btnDown.EnableWindow(FALSE);
			}
		}
	}
}