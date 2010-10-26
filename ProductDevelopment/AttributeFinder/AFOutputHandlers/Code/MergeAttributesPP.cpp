// MergeAttributesPP.cpp : Implementation of CMergeAttributesPP

#include "stdafx.h"
#include "MergeAttributesPP.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// CMergeAttributesPP
//--------------------------------------------------------------------------------------------------
CMergeAttributesPP::CMergeAttributesPP()
{
}
//--------------------------------------------------------------------------------------------------
CMergeAttributesPP::~CMergeAttributesPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI22782");
}
//--------------------------------------------------------------------------------------------------
HRESULT CMergeAttributesPP::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributesPP::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// Windows message handlers
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFOUTPUTHANDLERSLib::IMergeAttributesPtr ipRule = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI22783", ipRule);

		// Map controls to member variables
		m_editAttributeQuery		= GetDlgItem(IDC_EDIT_QUERY);
		m_editOverlapPercent		= GetDlgItem(IDC_EDIT_OVERLAP_PERCENT);
		m_btnSpecifyName			= GetDlgItem(IDC_RADIO_SPECIFY_NAME);
		m_btnPreserveName			= GetDlgItem(IDC_RADIO_PRESERVE_NAME);
		m_btnSpecifyType			= GetDlgItem(IDC_RADIO_SPECIFY_TYPE);
		m_btnCombineType			= GetDlgItem(IDC_RADIO_COMBINE_TYPES);
		m_editSpecifiedName			= GetDlgItem(IDC_EDIT_NAME);
		m_editSpecifiedType			= GetDlgItem(IDC_EDIT_TYPE);
		m_editSpecifiedValue		= GetDlgItem(IDC_EDIT_VALUE);
		m_listNameMergePriority		= GetDlgItem(IDC_LIST_NAMES);
		m_btnAdd					= GetDlgItem(IDC_BTN_ADD_NAME);
		m_btnModify					= GetDlgItem(IDC_BTN_MODIFY_NAME);
		m_btnRemove					= GetDlgItem(IDC_BTN_REMOVE_NAME);
		m_btnPreserveAsSubAttributes = GetDlgItem(IDC_CHECK_SUBATTRIBUTES);
		m_btnCreateMergedRegion		= GetDlgItem(IDC_RADIO_CREATE_MERGED_REGION);
		m_btnMergeIndividualZones	= GetDlgItem(IDC_RADIO_MERGE_INDIVIDUAL_ZONES);
		m_btnUp.SubclassDlgItem(IDC_BTN_NAME_UP, CWnd::FromHandle(m_hWnd));
		m_btnDown.SubclassDlgItem(IDC_BTN_NAME_DOWN, CWnd::FromHandle(m_hWnd));
		
		// Assign icons to the up and down buttons
		m_btnUp.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_UP)));
		m_btnDown.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_DOWN)));

		// Load the rule values into the property page.
		m_editAttributeQuery.SetWindowText(asString(ipRule->AttributeQuery).c_str());
		m_editOverlapPercent.SetWindowText(asString(ipRule->OverlapPercent, 0, 5).c_str());
		m_btnSpecifyName.SetCheck(asBSTChecked(ipRule->NameMergeMode == kSpecifyField));
		m_btnPreserveName.SetCheck(asBSTChecked(ipRule->NameMergeMode == kPreserveField));
		m_btnSpecifyType.SetCheck(asBSTChecked(ipRule->TypeMergeMode == kSpecifyField));
		m_btnCombineType.SetCheck(asBSTChecked(ipRule->TypeMergeMode == kCombineField));
		m_editSpecifiedName.SetWindowText(asString(ipRule->SpecifiedName).c_str());
		m_editSpecifiedType.SetWindowText(asString(ipRule->SpecifiedType).c_str());
		m_editSpecifiedValue.SetWindowText(asString(ipRule->SpecifiedValue).c_str());
		m_btnPreserveAsSubAttributes.SetCheck(asBSTChecked(ipRule->PreserveAsSubAttributes));
		m_btnCreateMergedRegion.SetCheck(asBSTChecked(ipRule->CreateMergedRegion));
		m_btnMergeIndividualZones.SetCheck(asBSTChecked(ipRule->CreateMergedRegion == VARIANT_FALSE));

		initializeList(m_listNameMergePriority, ipRule->NameMergePriority);

		updateControls();

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22784");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPP::OnBnClickedRadioBtn(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
												BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// A radio button option has been changed.  Enable/disable controls to reflect the selected
		// option.
		updateControls();

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22866");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPP::OnBnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
											  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Prompt user to enter a new value
		CString zValue;
		if (promptForValue(zValue, m_listNameMergePriority, "", -1, true))
		{
			int nTotal = m_listNameMergePriority.GetItemCount();
				
			// new item index
			int nIndex = m_listNameMergePriority.InsertItem(nTotal, zValue);
			for (int i = 0; i <= nTotal; i++)
			{
				// deselect any other items
				int nState = (i == nIndex) ? LVIS_SELECTED : 0;

				// only select current newly added item
				m_listNameMergePriority.SetItemState(i, nState, LVIS_SELECTED);
			}

			// adjust the column width in case there is a vertical scrollbar
			CRect rect;
			m_listNameMergePriority.GetClientRect(&rect);			
			m_listNameMergePriority.SetColumnWidth(0, rect.Width());

			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22851");

	// Call updateButtons to update the related buttons
	updateButtons();

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPP::OnBnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
												 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get currently selected item
		int nSelectedItemIndex = m_listNameMergePriority.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex==LB_ERR)
		{
			return 0;
		}

		// Get selected text
		CComBSTR bstrValue;
		m_listNameMergePriority.GetItemText(nSelectedItemIndex, 0, bstrValue.m_str);

		// Prompt user to modify the current value
		CString zValue(asString(bstrValue.m_str).c_str());
		if (promptForValue(zValue, m_listNameMergePriority, "", nSelectedItemIndex))
		{
			// If the user OK'd the box, save the new value
			m_listNameMergePriority.DeleteItem(nSelectedItemIndex);
			
			int nIndex = m_listNameMergePriority.InsertItem(nSelectedItemIndex, zValue);

			m_listNameMergePriority.SetItemState(nIndex, LVIS_SELECTED, LVIS_SELECTED);

			// Call updateButtons to update the related buttons
			updateButtons();

			// Set Dirty flag
			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22852");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPP::OnBnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
												 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get first selected item
		int nItem = m_listNameMergePriority.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		
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
					m_listNameMergePriority.DeleteItem(nItem);
					// Get next item selected item
					nItem = m_listNameMergePriority.GetNextItem(nItem - 1, 
						((nItem == 0) ? LVNI_ALL : LVNI_BELOW) | LVNI_SELECTED);
				}
				
				// If there's more item(s) below last deleted item, then set 
				// Selection on the next item
				int nTotalNumOfItems = m_listNameMergePriority.GetItemCount();
				if (nFirstItem < nTotalNumOfItems)
				{
					m_listNameMergePriority.SetItemState(nFirstItem, LVIS_SELECTED, LVIS_SELECTED);
				}
				else if (nTotalNumOfItems > 0)
				{
					// Select the last item
					m_listNameMergePriority.SetItemState(nTotalNumOfItems - 1, LVIS_SELECTED, 
						LVIS_SELECTED);
				}
			}

			// Adjust the column width in case there is a vertical scrollbar now
			CRect rect;
			m_listNameMergePriority.GetClientRect(&rect);
			m_listNameMergePriority.SetColumnWidth(0, rect.Width());

			// Call updateButtons to update the related buttons
			updateButtons();

			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22853");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPP::OnBnClickedBtnUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get current selected item index
		int nSelectedItemIndex = m_listNameMergePriority.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
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
			m_listNameMergePriority.GetItemText(nSelectedItemIndex, 0, bstrValue.m_str);

			// Then remove the selected item
			m_listNameMergePriority.DeleteItem(nSelectedItemIndex);

			// Now insert the item right before the item that was above
			int nActualIndex = m_listNameMergePriority.InsertItem(nAboveIndex, 
				asString(bstrValue.m_str).c_str());
			
			// Keep this item selected
			m_listNameMergePriority.SetItemState(nActualIndex, LVIS_SELECTED, LVIS_SELECTED);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22854");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPP::OnBnClickedBtnDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get current selected item index
		int nSelectedItemIndex = m_listNameMergePriority.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{
			// Get the index of the item right below currently selected item
			int nBelowIndex = nSelectedItemIndex + 1;
			if (nBelowIndex == m_listNameMergePriority.GetItemCount())
			{
				return 0;
			}

			// Get selected item text from list
			CComBSTR bstrValue;
			m_listNameMergePriority.GetItemText(nSelectedItemIndex, 0, bstrValue.m_str);

			// Then remove the selected item
			m_listNameMergePriority.DeleteItem(nSelectedItemIndex);

			// Now insert the item right before the item that was above
			int nActualIndex = m_listNameMergePriority.InsertItem(nBelowIndex, 
				asString(bstrValue.m_str).c_str());

			// Keep this item selected
			m_listNameMergePriority.SetItemState(nActualIndex, LVIS_SELECTED, LVIS_SELECTED);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22855");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPP::OnItemChangedList(int idCtrl, LPNMHDR pNMHDR, BOOL& bHandle)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// A new name has been selected in the preservation priority list box.  Update the buttons
		// as appropriate for the new selection.
		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22856");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPP::OnDblclkList(int idCtrl, LPNMHDR pNMHDR, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_listNameMergePriority.GetSelectedCount() == 1)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22857");

	return 0;
}

//--------------------------------------------------------------------------------------------------
// IPropertyPage
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributesPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ATLTRACE(_T("CMergeAttributesPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Obtain interface pointer to the IMergeAttributes class
			UCLID_AFOUTPUTHANDLERSLib::IMergeAttributesPtr ipRule = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI22785", ipRule != NULL);

			// Apply settings to rule
			ipRule->AttributeQuery = verifyControlValueAsBSTR(m_editAttributeQuery,
				"Specify a query to select attributes that may be merged!");

			ipRule->OverlapPercent = verifyControlValueAsDouble(m_editOverlapPercent, 0, 100,
				"The minimum mutual overlap percentage must be in the range 0 - 100", 
				75,	"Specify a minimum mutual overlap percentage for merging");

			IVariantVectorPtr ipNamePrioirtyList = retrieveListValues(m_listNameMergePriority);
			
			if ((m_btnSpecifyName.GetCheck() == BST_CHECKED))
			{
				// Get the specified name and validate it
				_bstr_t bstrName = verifyControlValueAsBSTR(m_editSpecifiedName,
					"Specify a name for the merged attribute!");
				try
				{
					validateIdentifier(asString(bstrName));
				}
				catch(...)
				{
					m_editSpecifiedName.SetFocus();
					throw;
				}

				ipRule->NameMergeMode = (UCLID_AFOUTPUTHANDLERSLib::EFieldMergeMode) kSpecifyField;
				ipRule->SpecifiedName = bstrName;
			}
			else
			{
				ipRule->NameMergeMode = (UCLID_AFOUTPUTHANDLERSLib::EFieldMergeMode) kPreserveField;
				ipRule->SpecifiedName = verifyControlValueAsBSTR(m_editSpecifiedName);

				if (ipNamePrioirtyList->Size == 0)
				{
					m_btnAdd.SetFocus();
					throw UCLIDException("ELI22942", "Specify at least one name to preserve!");
				}
			}

			ipRule->NameMergePriority = ipNamePrioirtyList;

			if (m_btnSpecifyType.GetCheck() == BST_CHECKED)
			{
				_bstr_t bstrType = verifyControlValueAsBSTR(m_editSpecifiedType);

				if (bstrType.length() > 0)
				{
					try
					{
						validateIdentifier(asString(bstrType));
					}
					catch(...)
					{
						m_editSpecifiedType.SetFocus();
						throw;
					}
				}

				ipRule->TypeMergeMode = (UCLID_AFOUTPUTHANDLERSLib::EFieldMergeMode) kSpecifyField;
				ipRule->SpecifiedType = bstrType;
			}
			else
			{
				ipRule->TypeMergeMode = (UCLID_AFOUTPUTHANDLERSLib::EFieldMergeMode) kCombineField;
				ipRule->SpecifiedType = verifyControlValueAsBSTR(m_editSpecifiedType);
			}

			ipRule->SpecifiedValue = verifyControlValueAsBSTR(m_editSpecifiedValue,
				"Specify a value for the merged attribute!");
				
			ipRule->PreserveAsSubAttributes =
				asVariantBool(m_btnPreserveAsSubAttributes.GetCheck() == BST_CHECKED);

			ipRule->CreateMergedRegion =
				asVariantBool(m_btnCreateMergedRegion.GetCheck() == BST_CHECKED);
		}
		
		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22786");

	// If we reached here, it's because of an exception
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributesPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22787", pbValue != NULL);

		try
		{
			// Check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22788");
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CMergeAttributesPP::initializeList(ATLControls::CListViewCtrl &listControl,
										IVariantVectorPtr ipEntries)
{
	ASSERT_RESOURCE_ALLOCATION("ELI22850", ipEntries != NULL);

	listControl.SetExtendedListViewStyle(LVS_EX_GRIDLINES|LVS_EX_FULLROWSELECT);
	CRect rect;			
	listControl.GetClientRect(&rect);
	
	// Make the first column as wide as the whole list box
	listControl.InsertColumn(0, "", LVCFMT_LEFT, rect.Width(), 0);

	// Add every value in ipEntries to the list.
	long nSize = ipEntries->Size;
	for (int i = 0; i < nSize; i++)
	{
		listControl.InsertItem(i, asString(ipEntries->GetItem(i).bstrVal).c_str());
	}

	listControl.GetClientRect(&rect);
	// Adjust the column width in case there is a vertical scrollbar now
	listControl.SetColumnWidth(0, rect.Width());
}
//--------------------------------------------------------------------------------------------------
IVariantVectorPtr CMergeAttributesPP::retrieveListValues(ATLControls::CListViewCtrl &listControl)
{
	IVariantVectorPtr ipEntries(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI22874", ipEntries != NULL);

	// Retrieve the entries from listControl (validate each entry)
	for (long i = 0; i < listControl.GetItemCount(); i++)
	{
		CComBSTR bstrValue;
		listControl.GetItemText(i, 0, bstrValue.m_str);
		validateIdentifier(asString(bstrValue));
		ipEntries->PushBack(bstrValue.m_str);
	}
	
	return ipEntries;
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributesPP::updateControls()
{
	if (m_btnSpecifyName.GetCheck() == BST_CHECKED)
	{
		// Enable the name specification edit box.
		m_editSpecifiedName.EnableWindow(TRUE);

		// Disable all controls dealing with name preservation.
		// Deselect items in list and disable it before disabling the buttons so that events fired
		// while de-activating list don't cause buttons to become re-enabled.
		ATLControls::CListViewCtrl listNames = GetDlgItem(IDC_LIST_NAMES);
		for (int i = 0; i < listNames.GetItemCount(); i++)
		{
			listNames.SetItemState(i, 0, LVIS_SELECTED);
		}
		listNames.EnableWindow(FALSE);

		// Disable the buttons associated with the name priority list control.
		GetDlgItem(IDC_BTN_ADD_NAME).EnableWindow(FALSE);
		GetDlgItem(IDC_BTN_MODIFY_NAME).EnableWindow(FALSE);
		GetDlgItem(IDC_BTN_REMOVE_NAME).EnableWindow(FALSE);
		GetDlgItem(IDC_BTN_NAME_UP).EnableWindow(FALSE);
		GetDlgItem(IDC_BTN_NAME_DOWN).EnableWindow(FALSE);
	}
	else
	{
		// Disable the name specification edit box.
		m_editSpecifiedName.EnableWindow(FALSE);

		// Enable the name priority list control and its buttons as appropriate.
		GetDlgItem(IDC_LIST_NAMES).EnableWindow(TRUE);
		updateButtons();
	}

	// Enable/disable the type specification edit box as appropriate.
	m_editSpecifiedType.EnableWindow(asMFCBool(m_btnSpecifyType.GetCheck() == BST_CHECKED));
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributesPP::updateButtons()
{
	// Enable/disable up and down arrow key buttons appropriately
	m_btnUp.EnableWindow(FALSE);
	m_btnDown.EnableWindow(FALSE);
	m_btnAdd.EnableWindow(TRUE);

	// Get current selected item index
	int nSelectedItemIndex = m_listNameMergePriority.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
	int nSelCount = m_listNameMergePriority.GetSelectedCount();
	int nCount = m_listNameMergePriority.GetItemCount();
	
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
//--------------------------------------------------------------------------------------------------
void CMergeAttributesPP::validateLicense()
{
	VALIDATE_LICENSE(gnRULESET_EDITOR_UI_OBJECT, "ELI22789", 
		"Merge attributes output handler PP");
}
//--------------------------------------------------------------------------------------------------
