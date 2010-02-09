// RemoveEntriesFromListPP.cpp : Implementation of CRemoveEntriesFromListPP
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "RemoveEntriesFromListPP.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <FileDialogEx.h>
#include <LicenseMgmt.h>
#include <EditorLicenseID.h>
#include <ComponentLicenseIDs.h>

using namespace std;

const int NUM_OF_CHARS = 4096;

//-------------------------------------------------------------------------------------------------
// CRemoveEntriesFromListPP
//-------------------------------------------------------------------------------------------------
CRemoveEntriesFromListPP::CRemoveEntriesFromListPP()
: m_ipInternalObject(CLSID_RemoveEntriesFromList)
{
	try
	{
		// Check licensing
		validateLicense();

		ASSERT_RESOURCE_ALLOCATION("ELI07308", m_ipInternalObject != NULL);
		
		m_dwTitleID = IDS_TITLERemoveEntriesFromListPP;
		m_dwHelpFileID = IDS_HELPFILERemoveEntriesFromListPP;
		m_dwDocStringID = IDS_DOCSTRINGRemoveEntriesFromListPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI06757")
}
//-------------------------------------------------------------------------------------------------
CRemoveEntriesFromListPP::~CRemoveEntriesFromListPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16316");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromListPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CRemoveEntriesFromListPP::Apply\n"));
		
		// get all values from the list box
		if (!saveListValues())
		{
			return S_FALSE;
		}
		
		// set case sensitivity
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_CASE_SENSITIVE));
		// check whether the check box is checked
		int nChecked = checkBox.GetCheck();
		m_ipInternalObject->IsCaseSensitive = nChecked==1?VARIANT_TRUE:VARIANT_FALSE;

		for (UINT i = 0; i < m_nObjects; i++)
		{	
			// assign the value lst to the object
			ICopyableObjectPtr ipValues = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI08378", ipValues != NULL);
			ICopyableObjectPtr ipCopyableObj(m_ipInternalObject);
			if (ipCopyableObj)
			{
				ipValues->CopyFrom(ipCopyableObj);
			}
		}

		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06758");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromListPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
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

//-------------------------------------------------------------------------------------------------
// Windows Message Handlers
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveEntriesFromListPP::OnInitDialog(UINT uMsg, WPARAM wParam, 
											   LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_listEntries = GetDlgItem(IDC_LIST_ENTRIES);
		m_listEntries.SetExtendedListViewStyle(LVS_EX_GRIDLINES|LVS_EX_FULLROWSELECT);
		
		CRect rect;
		m_listEntries.GetClientRect(&rect);
		m_listEntries.InsertColumn(0, "Value", LVCFMT_LEFT, rect.Width(), 0);
		
		ICopyableObjectPtr ipObject(m_ppUnk[0]);
		if (ipObject)
		{	
			ICopyableObjectPtr ipCopy = m_ipInternalObject;
			ASSERT_RESOURCE_ALLOCATION("ELI08379", ipCopy != NULL);
			ipCopy->CopyFrom(ipObject);
			
			// populate the dialog
			loadListValues();
		}
		
		m_listEntries.GetClientRect(&rect);
		
		// adjust the column width in case there is a vertical scrollbar now
		m_listEntries.SetColumnWidth(0, rect.Width());

		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06759");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveEntriesFromListPP::OnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CString zEnt = "";
		int nExistingItemIndex = LB_ERR;
		
		bool bSuccess = promptForValue(zEnt, m_listEntries);
		
		if (bSuccess)
		{
			int nTotal = m_listEntries.GetItemCount();
			
			int nIndex = m_listEntries.InsertItem(nTotal, zEnt);
			for (int i = 0; i <= nTotal; i++)
			{
				int nState = (i == nIndex) ? LVIS_SELECTED : 0;
				
				m_listEntries.SetItemState(i, nState, LVIS_SELECTED);
			}
			
			CRect rect;
			m_listEntries.GetClientRect(&rect);
			// adjust the column width in case there is a vertical scrollbar now
			m_listEntries.SetColumnWidth(0, rect.Width());
			
			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06762");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveEntriesFromListPP::OnClickedBtnLoad(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_listEntries.GetItemCount() > 0)
		{
			// prompt for overwrite
			int nRes = MessageBox("The existing entries will be overwritten. Do you wish to continue?", "Confirm", MB_YESNO);
			if (nRes == IDNO)
			{
				return 0;
			}
		}
		
		// show pick file dialog, do not show delimiter related windows
		CFileDialogEx openDialog( TRUE, ".txt", NULL, 
			OFN_HIDEREADONLY | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			"Text files (*.txt)|*.txt|All files (*.*)|*.*||", NULL);
		
		if (openDialog.DoModal() == IDOK)
		{
			// get file name
			CString zFileName = openDialog.GetPathName();
			
			// load values
			if (m_ipInternalObject)
			{
				// pass the file name to translate value to get translations
				m_ipInternalObject->LoadEntriesFromFile(_bstr_t(zFileName));
				
				// get map of translations
				loadListValues();
			}
		}
		
		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06763");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveEntriesFromListPP::OnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get currently selected item (assuming there's only one)		
		int nSelectedItemIndex = m_listEntries.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{			
			char pszValue[NUM_OF_CHARS];
			// get selected text
			m_listEntries.GetItemText(nSelectedItemIndex, 0, pszValue, NUM_OF_CHARS);
			CString zOld(pszValue);
			CString zEnt(pszValue);
			int nExistingItemIndex = LB_ERR;
			
			bool bSuccess = promptForValue(zEnt, m_listEntries);
			
			if (bSuccess)
			{
				m_listEntries.DeleteItem(nSelectedItemIndex);

				int nTotal = m_listEntries.GetItemCount();
				
				int nIndex = m_listEntries.InsertItem(nTotal, zEnt);

				for (int i = 0; i <= nTotal; i++)
				{
					int nState = (i == nIndex) ? LVIS_SELECTED : 0;
					
					m_listEntries.SetItemState(i, nState, LVIS_SELECTED);

				}
				
				SetDirty(TRUE);
				
				CRect rect;
				m_listEntries.GetClientRect(&rect);
				// adjust the column width in case there is a vertical scrollbar now
				m_listEntries.SetColumnWidth(0, rect.Width());
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06764");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveEntriesFromListPP::OnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		int nItem = m_listEntries.GetNextItem(-1, LVNI_ALL|LVNI_SELECTED);
		if (nItem != -1)
		{
			int nRes = MessageBox("Delete selected item(s)?", "Confirm Delete", MB_YESNO);
			if (nRes == IDYES)
			{
				// remove selected items
				int nFirstItem = nItem;
				while(nItem != -1)
				{
					// remove from the UI listbox
					m_listEntries.DeleteItem(nItem);
					nItem = m_listEntries.GetNextItem(nItem - 1, ((nItem == 0) ? LVNI_ALL : LVNI_BELOW) | LVNI_SELECTED);
				}
				
				// if there's more item(s) below last deleted item, then set 
				// selection on the next item
				int nTotalNumOfItems = m_listEntries.GetItemCount();
				if (nFirstItem < nTotalNumOfItems)
				{
					m_listEntries.SetItemState(nFirstItem, LVIS_SELECTED, LVIS_SELECTED);
				}
				else if (nTotalNumOfItems > 0)
				{
					// select the last item
					m_listEntries.SetItemState(nTotalNumOfItems - 1, LVIS_SELECTED, LVIS_SELECTED);
				}
			}
		}
		
		CRect rect;
		m_listEntries.GetClientRect(&rect);		
		// adjust the column width in case there is a vertical scrollbar now
		m_listEntries.SetColumnWidth(0, rect.Width());

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06765");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveEntriesFromListPP::OnClickedBtnSave(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (!saveListValues())
		{
			return 0;
		}
		
		// bring the pick file dialog and use it as a save dialog
		static CString zFileName("");
		// show pick file dialog
		CFileDialogEx openDialog(FALSE, NULL, NULL, OFN_HIDEREADONLY | OFN_OVERWRITEPROMPT | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			"All files (*.*)|*.*||", NULL);
		
		if (openDialog.DoModal() == IDOK)
		{
			// get file name
			CString zFileName = openDialog.GetPathName();
			
			// save the file
			if (m_ipInternalObject)
			{
				m_ipInternalObject->SaveEntriesToFile(_bstr_t(zFileName));
			}
		}	
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06766");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveEntriesFromListPP::OnItemListChanged(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06767");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveEntriesFromListPP::OnKeydownList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// only delete or del key can delete the selected item
		int nDelete = ::GetKeyState(VK_DELETE);
		if (nDelete < 0)
		{
			return OnClickedBtnRemove(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06768");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveEntriesFromListPP::OnDblclkList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return OnClickedBtnModify(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CRemoveEntriesFromListPP::loadListValues()
{
	m_listEntries.DeleteAllItems();
	
	if (m_ipInternalObject)
	{
		// m_listEntries of values
		IVariantVectorPtr ipVecValues(m_ipInternalObject->EntryList);
		if (ipVecValues)
		{
			CString zValue("");
			long nSize = ipVecValues->Size;
			for (long n=0; n<nSize; n++)
			{
				zValue = (char*)_bstr_t(ipVecValues->GetItem(n));
				m_listEntries.InsertItem(n, zValue);
			}
			// set selection to the first item
			if (nSize > 0)
			{
				m_listEntries.SetItemState(0, 1, LVNI_SELECTED);
			}
		}
		
		// case-sensitivity
		VARIANT_BOOL bCaseSensitive = m_ipInternalObject->IsCaseSensitive;
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_CASE_SENSITIVE));
		checkBox.SetCheck(bCaseSensitive==VARIANT_TRUE?1:0);
	}
}
//-------------------------------------------------------------------------------------------------
void CRemoveEntriesFromListPP::updateButtons()
{
	int nSelCount = m_listEntries.GetSelectedCount();
	int nCount = m_listEntries.GetItemCount();
	
	if (nCount == 0)
	{
		ATLControls::CButton(GetDlgItem(IDC_BTN_MODIFY)).EnableWindow(FALSE);
		ATLControls::CButton(GetDlgItem(IDC_BTN_REMOVE)).EnableWindow(FALSE);
	}
	else
	{
		ATLControls::CButton(GetDlgItem(IDC_BTN_MODIFY)).EnableWindow(nSelCount == 1 ? true : false);
		ATLControls::CButton(GetDlgItem(IDC_BTN_REMOVE)).EnableWindow(nSelCount >= 1 ? true : false);
	}
}
//-------------------------------------------------------------------------------------------------
bool CRemoveEntriesFromListPP::saveListValues()
{
	// get all values from the list box
	char pszValue[NUM_OF_CHARS];
	IVariantVectorPtr ipEntryList(CLSID_VariantVector);
	if (ipEntryList)
	{
		ipEntryList->Clear();
		// populate the list of values in m_ipInternalObject
		int nSize = m_listEntries.GetItemCount();
		if (nSize==0)
		{
			MessageBox("Please provide one or more string to search for.", "Configuration");
			return false;
		}
		
		for (int n=0; n< nSize; n++)
		{
			m_listEntries.GetItemText(n, 0, pszValue, NUM_OF_CHARS);
			ipEntryList->PushBack(_bstr_t(pszValue));
		}
		
		m_ipInternalObject->EntryList = ipEntryList;
		
		return true;
	}
	
	return false;
}
//-------------------------------------------------------------------------------------------------
void CRemoveEntriesFromListPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07702", 
		"RemoveEntriesFromList Output Handler PP" );
}
//-------------------------------------------------------------------------------------------------
