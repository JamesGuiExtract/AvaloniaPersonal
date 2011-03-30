// ValueFromListPP.cpp : Implementation of CValueFromListPP
#include "stdafx.h"
#include "AFValueFinders.h"
#include "ValueFromListPP.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#include <string>
using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int NUM_OF_CHARS = 4096;

//-------------------------------------------------------------------------------------------------
// CValueFromListPP
//-------------------------------------------------------------------------------------------------
CValueFromListPP::CValueFromListPP()
: m_ipInternalObject(CLSID_ValueFromList)
{
	try
	{
		// Check licensing
		validateLicense();

		ASSERT_RESOURCE_ALLOCATION("ELI04557", m_ipInternalObject != __nullptr);
		
		m_dwTitleID = IDS_TITLEValueFromListPP;
		m_dwHelpFileID = IDS_HELPFILEValueFromListPP;
		m_dwDocStringID = IDS_DOCSTRINGValueFromListPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI04558")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromListPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CValueFromListPP::Apply\n"));

		// get all values from the lst box
		if (!saveListValues())
		{
			return S_FALSE;
		}

			
		for (UINT i = 0; i < m_nObjects; i++)
		{	
			// assign the value lst to the object
			ICopyableObjectPtr ipValues = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI08380", ipValues != __nullptr);
			ICopyableObjectPtr ipCopyableObj(m_ipInternalObject);
			if (ipCopyableObj)
			{
				ipValues->CopyFrom(ipCopyableObj);
			}
		}

		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04550");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromListPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CValueFromListPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_VALUE_LIST));
		lst.SetExtendedListViewStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
		
		CRect rect;
		
		lst.GetClientRect(&rect);
		
		lst.InsertColumn( 0, "Value", LVCFMT_LEFT, 
			rect.Width(), 0 );
		
		
		ICopyableObjectPtr ipObject(m_ppUnk[0]);
		if (ipObject)
		{	
			ICopyableObjectPtr ipCopy = m_ipInternalObject;
			ASSERT_RESOURCE_ALLOCATION("ELI08381", ipCopy != __nullptr);
			ipCopy->CopyFrom(ipObject);
			// populate the dialog
			loadListValues();
		}
		
		lst.GetClientRect(&rect);
		
		// adjust the column width in case there is a vertical scrollbar now
		lst.SetColumnWidth(0, rect.Width());

		// Create and initialize the info tip control
		m_infoTip.Create(CWnd::FromHandle(m_hWnd));
		m_infoTip.SetShowDelay(0);

		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04551");
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueFromListPP::OnClickedBtnAddValueToList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_VALUE_LIST));
		
		CString zEnt = "";
		int nExistingItemIndex = LB_ERR;
		
		bool bSuccess = promptForValue(zEnt, lst);
		
		if (bSuccess)
		{
			int nTotal = lst.GetItemCount();
			
			int nIndex = lst.InsertItem(nTotal, zEnt);
			for (int i = 0; i <= nTotal; i++)
			{
				int nState = (i == nIndex) ? LVIS_SELECTED : 0;
				
				lst.SetItemState(i, nState, LVIS_SELECTED);
			}
			
			CRect rect;
			
			lst.GetClientRect(&rect);
			
			// adjust the column width in case there is a vertical scrollbar now
			lst.SetColumnWidth(0, rect.Width());
			
			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04552");
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueFromListPP::OnClickedBtnLoadFromFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_VALUE_LIST));
		if (lst.GetItemCount() > 0)
		{
			// prompt for overwrite
			int nRes = MessageBox("The existing entries will be overwritten. Do you wish to continue?", "Confirm", MB_YESNO);
			if (nRes == IDNO)
			{
				return 0;
			}
		}
		
		// show pick file dialog, do not show delimiter related windows
		CFileDialog openDialog( TRUE, ".txt", NULL, 
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
				m_ipInternalObject->LoadListFromFile(_bstr_t(zFileName));
				
				// get map of translations
				loadListValues();
			}
		}
		
		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04553");
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueFromListPP::OnClickedBtnSaveToFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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
		CFileDialog openDialog(FALSE, NULL, NULL, OFN_HIDEREADONLY | OFN_OVERWRITEPROMPT | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			"All files (*.*)|*.*||", NULL);
		
		if (openDialog.DoModal() == IDOK)
		{
			// get file name
			CString zFileName = openDialog.GetPathName();
			
			// save the file
			if (m_ipInternalObject)
			{
				m_ipInternalObject->SaveListToFile(_bstr_t(zFileName));
			}
		}	
	
		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19301");
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueFromListPP::OnClickedBtnModifyValueInList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_VALUE_LIST));
		
		// get currently selected item (assuming there's only one)
		
		int nSelectedItemIndex = lst.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{			
			char pszValue[NUM_OF_CHARS];
			// get selected text
			lst.GetItemText(nSelectedItemIndex, 0, pszValue, NUM_OF_CHARS);
			CString zOld(pszValue);
			CString zEnt(pszValue);
			int nExistingItemIndex = LB_ERR;
			

			bool bSuccess = promptForValue(zEnt, lst);
			
			if (bSuccess)
			{
				lst.DeleteItem(nSelectedItemIndex);

				int nTotal = lst.GetItemCount();
				
				int nIndex = lst.InsertItem(nTotal, zEnt);

				for (int i = 0; i <= nTotal; i++)
				{
					int nState = (i == nIndex) ? LVIS_SELECTED : 0;
					
					lst.SetItemState(i, nState, LVIS_SELECTED);

				}
				
				SetDirty(TRUE);

				
				CRect rect;
				
				lst.GetClientRect(&rect);
				
				// adjust the column width in case there is a vertical scrollbar now
				lst.SetColumnWidth(0, rect.Width());

			}
			
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04554");
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueFromListPP::OnClickedBtnRemoveValueFromList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_VALUE_LIST));
		
		int nItem = lst.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		
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
					lst.DeleteItem(nItem);
					
					nItem = lst.GetNextItem(nItem - 1, ((nItem == 0) ? LVNI_ALL : LVNI_BELOW) | LVNI_SELECTED);
				}
				
				// if there's more item(s) below last deleted item, then set 
				// selection on the next item
				int nTotalNumOfItems = lst.GetItemCount();
				if (nFirstItem < nTotalNumOfItems)
				{
					lst.SetItemState(nFirstItem, LVIS_SELECTED, LVIS_SELECTED);
				}
				else if (nTotalNumOfItems > 0)
				{
					// select the last item
					lst.SetItemState(nTotalNumOfItems - 1, LVIS_SELECTED, LVIS_SELECTED);
				}
				
			}
		}
		
		CRect rect;
		lst.GetClientRect(&rect);
		// adjust the column width in case there is a vertical scrollbar now
		lst.SetColumnWidth(0, rect.Width());
		
		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04555");
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueFromListPP::OnClickedChkCaseValueFromList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_CASE_FROM_LIST));
		// check whether the check box is checked
		int nChecked = checkBox.GetCheck();
		
		m_ipInternalObject->IsCaseSensitive = nChecked==1?VARIANT_TRUE:VARIANT_FALSE;
		
		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04556");
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueFromListPP::OnDblClkList(int idCtrl, 
									   LPNMHDR pnmh, 
									   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	return OnClickedBtnModifyValueInList(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueFromListPP::OnListItemChanged(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	updateButtons();
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueFromListPP::OnKeyDownList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// only delete or del key can delete the selected item
		int nDelete = ::GetKeyState(VK_DELETE);
		if (nDelete < 0)
		{
			return OnClickedBtnRemoveValueFromList(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07311");
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueFromListPP::OnClickedClueDynamicListInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl,
													   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("- Dynamically loading a string list from a file is supported.\n"
					  "- To specify a dynamic file, an entry must begin with \"file://\".\n"
					  "- A file may be specified in combination with static entries or\n"
					  "  additional dynamic lists.\n"
					  "- Path tags such as <RSDFileDir> and <ComponentDataDir> may be used.\n"
					  "- For example, if an entry in the list is file://<RSDFileDir>\\list.txt,\n"
					  "  the entry will be replaced dynamically at runtime with the contents\n"
					  "  of the file.\n");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30067");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CValueFromListPP::loadListValues()
{
	ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_VALUE_LIST));
	lst.DeleteAllItems();
	
	if (m_ipInternalObject)
	{
		// lst of values
		IVariantVectorPtr ipVecValues(m_ipInternalObject->ValueList);
		if (ipVecValues)
		{
			CString zValue("");
			long nSize = ipVecValues->Size;
			for (long n=0; n<nSize; n++)
			{
				zValue = (char*)_bstr_t(ipVecValues->GetItem(n));
				lst.InsertItem(n, zValue);
			}
			// set selection to the first item
			if (nSize > 0)
			{
				lst.SetItemState(0, 1, LVNI_SELECTED);
			}
		}
		
		// case-sensitivity
		VARIANT_BOOL bCaseSensitive = m_ipInternalObject->IsCaseSensitive;
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_CASE_FROM_LIST));
		checkBox.SetCheck(bCaseSensitive==VARIANT_TRUE?1:0);
	}
}
//-------------------------------------------------------------------------------------------------
void CValueFromListPP::updateButtons()
{
	ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_VALUE_LIST));
	int nSelCount = lst.GetSelectedCount();
	int nCount = lst.GetItemCount();
	
	if (nCount == 0)
	{
		ATLControls::CButton(GetDlgItem(IDC_BTN_MODIFY_VALUE_IN_LIST)).EnableWindow(FALSE);
		ATLControls::CButton(GetDlgItem(IDC_BTN_REMOVE_VALUE_FROM_LIST)).EnableWindow(FALSE);
	}
	else
	{
		ATLControls::CButton(GetDlgItem(IDC_BTN_MODIFY_VALUE_IN_LIST)).EnableWindow(nSelCount == 1 ? true : false);
		ATLControls::CButton(GetDlgItem(IDC_BTN_REMOVE_VALUE_FROM_LIST)).EnableWindow(nSelCount >= 1 ? true : false);
	}
}
//-------------------------------------------------------------------------------------------------
bool CValueFromListPP::saveListValues()
{
	// get all values from the list box
	ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_VALUE_LIST));
	char pszValue[NUM_OF_CHARS];
	IVariantVectorPtr ipValueList(CLSID_VariantVector);
	if (ipValueList)
	{
		ipValueList->Clear();
		// populate the list of values in m_ipInternalObject
		int nSize = lst.GetItemCount();
		if (nSize==0)
		{
			MessageBox("Please provide one or more string to search for.", "Configuration");
			return false;
		}
		
		for (int n=0; n< nSize; n++)
		{
			lst.GetItemText(n, 0, pszValue, NUM_OF_CHARS);
			ipValueList->PushBack(_bstr_t(pszValue));
		}
		
		m_ipInternalObject->ValueList = ipValueList;
		
		return true;
	}
	
	return false;
}
//-------------------------------------------------------------------------------------------------
void CValueFromListPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07685", "Value From List PP" );
}
//-------------------------------------------------------------------------------------------------
