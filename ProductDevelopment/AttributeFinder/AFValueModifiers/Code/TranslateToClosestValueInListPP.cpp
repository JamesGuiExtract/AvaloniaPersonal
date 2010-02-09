// TranslateToClosestValueInListPP.cpp : Implementation of CTranslateToClosestValueInListPP
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "TranslateToClosestValueInListPP.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <FileDialogEx.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int NUM_OF_CHARS = 4096;

//-------------------------------------------------------------------------------------------------
// CTranslateToClosestValueInListPP
//-------------------------------------------------------------------------------------------------
CTranslateToClosestValueInListPP::CTranslateToClosestValueInListPP() 
: m_ipInternalValueList(CLSID_TranslateToClosestValueInList)
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLETranslateToClosestValueInListPP;
		m_dwHelpFileID = IDS_HELPFILETranslateToClosestValueInListPP;
		m_dwDocStringID = IDS_DOCSTRINGTranslateToClosestValueInListPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07721")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInListPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CTranslateToClosestValueInListPP::Apply\n"));

		if (!saveListValues())
		{
			return S_FALSE;
		}

		for (UINT i = 0; i < m_nObjects; i++)
		{	
			// assign the value lst to the object
			ICopyableObjectPtr ipValues = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI08384", ipValues != NULL);
			ICopyableObjectPtr ipCopyableObj(m_ipInternalValueList);
			if (ipCopyableObj)
			{
				ipValues->CopyFrom(ipCopyableObj);
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04337");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInListPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CTranslateToClosestValueInListPP::OnInitDialog(UINT uMsg, WPARAM wParam, 
													   LPARAM lParam, BOOL& bHandled)
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

		UCLID_AFVALUEMODIFIERSLib::ITranslateToClosestValueInListPtr ipValues(m_ppUnk[0]);
		if (ipValues)
		{
			ICopyableObjectPtr ipCopyableObj(ipValues);
			if (ipCopyableObj)
			{
				// copy all properies from that value lst to internal value lst object.
				ICopyableObjectPtr ipCopy = m_ipInternalValueList;
				ASSERT_RESOURCE_ALLOCATION("ELI08385", ipCopy != NULL);
				ipCopy->CopyFrom(ipCopyableObj);
				loadListValues();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04242");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateToClosestValueInListPP::OnClickedBtnAddValue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_VALUE_LIST));

		CString zEnt;
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04332");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateToClosestValueInListPP::OnClickedBtnLoadFileValues(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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

		CString zFileName("");
		// show file dialog
		CFileDialogEx fileDlg( TRUE, ".txt", zFileName, 
			OFN_HIDEREADONLY | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			"Text files (*.txt)|*.txt|All files (*.*)|*.*||", NULL );
		
		if (fileDlg.DoModal() == IDOK)
		{
			// get the file name
			zFileName = fileDlg.GetPathName();

			// load translations
			if (m_ipInternalValueList)
			{
				// pass the file name to translate value to get translations
				m_ipInternalValueList->LoadValuesFromFile(_bstr_t(zFileName));

				// get map of translations
				loadListValues();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04333");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateToClosestValueInListPP::OnClickedBtnModifyValue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_VALUE_LIST));

		// get currently selected item
		int nSelectedItemIndex = lst.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex==LB_ERR)
		{
			return 0;
		}

		char pszValue[NUM_OF_CHARS];
		// get selected text
		lst.GetItemText(nSelectedItemIndex, 0, pszValue, NUM_OF_CHARS);
		CString zEnt(pszValue);
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
			
			// Set Dirty flag
			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04334");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateToClosestValueInListPP::OnClickedBtnRemoveValue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_VALUE_LIST));

		// get first selected item
		int nItem = lst.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		
		if (nItem != -1)
		{
			int nRes = MessageBox("Delete selected item?", "Confirm Delete", MB_YESNO);
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04335");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateToClosestValueInListPP::OnClickedChkValueCaseSensitive(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_CASE3));
		// check whether the check box is checked
		int nChecked = checkBox.GetCheck();
		// update translate value object
		if (m_ipInternalValueList)
		{
			m_ipInternalValueList->IsCaseSensitive = nChecked==1?VARIANT_TRUE:VARIANT_FALSE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04336");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateToClosestValueInListPP::OnClickedChkForceMatch(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox( GetDlgItem( IDC_CHK_FORCE ) );

		// Check whether the check box is checked
		int nChecked = checkBox.GetCheck();

		// Update translate value object
		if (m_ipInternalValueList)
		{
			m_ipInternalValueList->IsForcedMatch = (nChecked == 1) 
				? VARIANT_TRUE : VARIANT_FALSE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05014");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateToClosestValueInListPP::OnClickedBtnSaveFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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
		// show file dialog
		CFileDialogEx fileDlg(FALSE, NULL, zFileName, 
			OFN_HIDEREADONLY|OFN_OVERWRITEPROMPT|OFN_PATHMUSTEXIST|OFN_NOCHANGEDIR,
			"All files (*.*)|*.*||", NULL);
		if (fileDlg.DoModal() == IDOK)
		{
			// get the file name as well as the delimiter
			zFileName = fileDlg.GetPathName();

			// save the file
			if (m_ipInternalValueList)
			{
				m_ipInternalValueList->SaveValuesToFile(_bstr_t(zFileName));
			}
		}	
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05579");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateToClosestValueInListPP::OnDblClkList(int idCtrl, 
									   LPNMHDR pnmh, 
									   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	return OnClickedBtnModifyValue(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateToClosestValueInListPP::OnListItemChanged(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	updateButtons();
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateToClosestValueInListPP::OnKeyDownList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// only delete or del key can delete the selected item
		int nDelete = ::GetKeyState(VK_DELETE);
		if (nDelete < 0)
		{
			return OnClickedBtnRemoveValue(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07312");
	
	return 0;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CTranslateToClosestValueInListPP::loadListValues()
{
	ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_VALUE_LIST));
	lst.DeleteAllItems();
	
	if (m_ipInternalValueList)
	{
		// lst of values
		IVariantVectorPtr ipVecValues(m_ipInternalValueList->ClosestValueList);
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
				lst.SetItemState(0, LVIS_SELECTED, LVIS_SELECTED);
			}
		}

		// case-sensitivity
		VARIANT_BOOL bCaseSensitive = m_ipInternalValueList->IsCaseSensitive;
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_CASE3));
		checkBox.SetCheck(bCaseSensitive==VARIANT_TRUE?1:0);

		// Force Match
		VARIANT_BOOL vbForceMatch = m_ipInternalValueList->IsForcedMatch;
		checkBox = GetDlgItem( IDC_CHK_FORCE );
		checkBox.SetCheck( (vbForceMatch == VARIANT_TRUE) ? 1 : 0 );
	}
}
//-------------------------------------------------------------------------------------------------
bool CTranslateToClosestValueInListPP::saveListValues()
{
	// get all values from the lst box
	ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_VALUE_LIST));
	char pszValue[NUM_OF_CHARS];
	IVariantVectorPtr ipValueList(CLSID_VariantVector);
	if (ipValueList)
	{
		ipValueList->Clear();
		// populate the lst of values in m_ipInternalValueList
		int nSize = lst.GetItemCount();
		if (nSize==0)
		{
			MessageBox("Please specify one or more translation strings.", "Configuration");
			return false;
		}
		
		for (int n=0; n< nSize; n++)
		{
			lst.GetItemText(n, 0, pszValue, NUM_OF_CHARS);
			ipValueList->PushBack(_bstr_t(pszValue));
		}

		m_ipInternalValueList->ClosestValueList = ipValueList;

		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void CTranslateToClosestValueInListPP::updateButtons()
{
	ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_VALUE_LIST));
	int nSelCount = lst.GetSelectedCount();
	int nCount = lst.GetItemCount();
	
	if (nCount == 0)
	{
		ATLControls::CButton(GetDlgItem(IDC_BTN_MODIFY_VALUE)).EnableWindow(FALSE);
		ATLControls::CButton(GetDlgItem(IDC_BTN_REMOVE_VALUE)).EnableWindow(FALSE);
	}
	else
	{
		ATLControls::CButton(GetDlgItem(IDC_BTN_MODIFY_VALUE)).EnableWindow(nSelCount == 1 ? true : false);
		ATLControls::CButton(GetDlgItem(IDC_BTN_REMOVE_VALUE)).EnableWindow(nSelCount >= 1 ? true : false);
	}
}
//-------------------------------------------------------------------------------------------------
void CTranslateToClosestValueInListPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07699", 
		"TranslateToClosestValueInList Modifier PP" );
}
//-------------------------------------------------------------------------------------------------
