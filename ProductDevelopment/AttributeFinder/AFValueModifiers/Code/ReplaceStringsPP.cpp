// ReplaceStringsPP.cpp : Implementation of CReplaceStringsPP
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "ReplaceStringsPP.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <PickFileAndDelimiterDlg.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <Prompt2Dlg.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#include <map>

using namespace std;

#define	TO_BE_REPLACED_COLUMN	0
#define	REPLACEMENT_COLUMN		1

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int NUM_OF_CHARS = 4096;

//-------------------------------------------------------------------------------------------------
// CReplaceStringsPP
//-------------------------------------------------------------------------------------------------
CReplaceStringsPP::CReplaceStringsPP()
: m_ipInternalReplacements(CLSID_ReplaceStrings)
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEReplaceStringsPP;
		m_dwHelpFileID = IDS_HELPFILEReplaceStringsPP;
		m_dwDocStringID = IDS_DOCSTRINGReplaceStringsPP;

		// Create an IMiscUtilsPtr object
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI14563", ipMiscUtils != NULL );

		// Get the file header string and its length from IMiscUtilsPtr object
		m_strFileHeader = ipMiscUtils->GetFileHeader();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07719")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStringsPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CReplaceStringsPP::Apply\n"));

		if (!saveReplacements())
		{
			return S_FALSE;
		}

		bool bCaseSensitive  = IsDlgButtonChecked(IDC_CHK_CASE2)==BST_CHECKED;
		m_ipInternalReplacements->IsCaseSensitive = bCaseSensitive?VARIANT_TRUE:VARIANT_FALSE;
		
		bool bAsRegExpr = IsDlgButtonChecked(IDC_CHK_AS_REGEXP)==BST_CHECKED;
		m_ipInternalReplacements->AsRegularExpr = bAsRegExpr?VARIANT_TRUE:VARIANT_FALSE;;
		
		for (UINT i = 0; i < m_nObjects; i++)
		{
			ICopyableObjectPtr ipReplacementPairs = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI08382", ipReplacementPairs != NULL);
			// copy all contents from internal object to the 
			// translate value object that associated with this property page
			ICopyableObjectPtr ipCopyableObj(m_ipInternalReplacements);
			if (ipCopyableObj)
			{
				ipReplacementPairs->CopyFrom(ipCopyableObj);
			}
		}

		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04316");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStringsPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CReplaceStringsPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// create tooltip object
		m_infoTip.Create(CWnd::FromHandle(m_hWnd));
		// set no delay.
		m_infoTip.SetShowDelay(0);

		m_listReplacement = GetDlgItem(IDC_LIST_REPLACE_STRING);
		m_btnUp = GetDlgItem(IDC_BTN_UP);
		m_btnDown = GetDlgItem(IDC_BTN_DOWN);
		m_btnRemove = GetDlgItem(IDC_BTN_REMOVE2);
		m_btnModify = GetDlgItem(IDC_BTN_MODIFY2);
		m_btnAdd = GetDlgItem(IDC_BTN_ADD2);
		m_btnSaveFile = GetDlgItem(IDC_BTN_SAVEFILE2);

		// disable Remove and Modify buttons
		m_btnRemove.EnableWindow(FALSE);
		m_btnModify.EnableWindow(FALSE);

		// load icons for up and down buttons
		m_btnUp.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_UP)));
		m_btnDown.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_DOWN)));

		// update both buttons
		updateUpAndDownButtons();

		// Determine column widths for list
		CRect rectList;
		m_listReplacement.GetClientRect(rectList);
		long nWidth1 = rectList.Width() / 2;
		long nWidth2 = rectList.Width() - nWidth1;
		// Prepare list
		m_listReplacement.SetExtendedListViewStyle(LVS_EX_GRIDLINES|LVS_EX_FULLROWSELECT);
		m_listReplacement.InsertColumn(TO_BE_REPLACED_COLUMN, "To Be Replaced",
									LVCFMT_LEFT, nWidth1, TO_BE_REPLACED_COLUMN);
		m_listReplacement.InsertColumn(REPLACEMENT_COLUMN, "Replacement",
									LVCFMT_LEFT, nWidth2, REPLACEMENT_COLUMN);
		
		// get the replacements associated with this property page.
		// assume that this property page has one and only one coclass associated with it
		UCLID_AFVALUEMODIFIERSLib::IReplaceStringsPtr ipReplacements(m_ppUnk[0]);
		if (ipReplacements)
		{
			ICopyableObjectPtr ipCopyableObj(ipReplacements);
			if (ipCopyableObj)
			{
				// copy all properies into internal replacement object.
				ICopyableObjectPtr ipCopy = m_ipInternalReplacements;
				ASSERT_RESOURCE_ALLOCATION("ELI08383", ipCopy != NULL);
				ipCopy->CopyFrom(ipCopyableObj);
				loadReplacements();

				// update the buttons
				updateButtons();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04317");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CReplaceStringsPP::OnClickedBtnAddReplacement(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show prompt dialog for entering translation infos
		CString zEnt1, zEnt2;
		int nExistingItemIndex = -1;
		bool bSuccess = promptForReplacements(zEnt1, zEnt2, nExistingItemIndex);

		if (bSuccess)
		{
			// add entry to the list view
			int nTotalItems = m_listReplacement.GetItemCount();
			int nNewIndex = 0;
			if (nTotalItems>0)
			{
				nNewIndex = nTotalItems;
			}

			if (nExistingItemIndex>=0)
			{
				m_listReplacement.SetItemText(nExistingItemIndex, TO_BE_REPLACED_COLUMN, zEnt1);
				m_listReplacement.SetItemText(nExistingItemIndex, REPLACEMENT_COLUMN, zEnt2);

				for (int i = 0; i <= m_listReplacement.GetItemCount(); i++)
				{
					int nState = (i == nExistingItemIndex) ? LVIS_SELECTED : 0;
					
					m_listReplacement.SetItemState(i, nState, LVIS_SELECTED);
				}
			}
			else
			{
				int nActualIndex = m_listReplacement.InsertItem(nNewIndex, zEnt1);
				m_listReplacement.SetItemText(nActualIndex, REPLACEMENT_COLUMN, zEnt2);
				
				for (int i = 0; i <= m_listReplacement.GetItemCount(); i++)
				{
					int nState = (i == nNewIndex) ? LVIS_SELECTED : 0;
					
					m_listReplacement.SetItemState(i, nState, LVIS_SELECTED);
				}
			}

			updateUpAndDownButtons();

			CRect rect;
			
			m_listReplacement.GetClientRect(&rect);
			
			// adjust the column width in case there is a vertical scrollbar now
			m_listReplacement.SetColumnWidth(0, rect.Width() / 2);
			m_listReplacement.SetColumnWidth(1, rect.Width() / 2);
			
			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04318");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CReplaceStringsPP::OnClickedBtnLoadReplacementFromFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_listReplacement.GetItemCount() > 0)
		{
			// prompt for overwrite
			int nRes = MessageBox("The existing entries will be overwritten. Do you wish to continue?", "Confirm", MB_YESNO);
			if (nRes == IDNO)
			{
				return 0;
			}
		}

		static CString zFileName(""), zDelimiter("");
		// show pick file dialog
		PickFileAndDelimiterDlg fileDlg(zFileName, zDelimiter);
		if (fileDlg.DoModal() == IDOK)
		{
			// get the file name as well as the delimiter
			zFileName = fileDlg.m_zFileName;
			zDelimiter = fileDlg.m_zDelimiter;

			// load translations
			if (m_ipInternalReplacements)
			{
				// load from file
				m_ipInternalReplacements->LoadReplaceInfoFromFile(_bstr_t(zFileName), _bstr_t(zDelimiter));

				// get map of replacements
				loadReplacements();
			}

			CRect rect;
			
			m_listReplacement.GetClientRect(&rect);
			
			// adjust the column width in case there is a vertical scrollbar now
			m_listReplacement.SetColumnWidth(0, rect.Width() / 2);
			m_listReplacement.SetColumnWidth(1, rect.Width() / 2);

			// update the buttons
			updateButtons();

			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04319");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CReplaceStringsPP::OnClickedBtnModifyReplacement(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get current selected item index
		int nSelectedItemIndex = m_listReplacement.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);

		if (nSelectedItemIndex >= 0)
		{
			// get the selected item text to pass into the prompt dialog
			CString zEnt1 = getItemText(nSelectedItemIndex, TO_BE_REPLACED_COLUMN).c_str();
			CString zEnt2 = getItemText(nSelectedItemIndex, REPLACEMENT_COLUMN).c_str();

			int nExistingItemIndex = nSelectedItemIndex;
			bool bSuccess = promptForReplacements(zEnt1, zEnt2, nExistingItemIndex);
			// show prompt dialog for entering translation infos
			if (bSuccess)
			{
				// Get the cout of items inside the list box
				int iCount = m_listReplacement.GetItemCount();
				// If there is no itmes inside list box, insert the item as the first row
				if (iCount == 0)
				{
					m_listReplacement.InsertItem(0, zEnt1);
					m_listReplacement.SetItemText(0, REPLACEMENT_COLUMN, zEnt2);
				}
				else
				{
					// if the entry already exists, then replace the old one with the new one
					if (nExistingItemIndex >= 0 && nExistingItemIndex != nSelectedItemIndex)
					{
						nSelectedItemIndex = nExistingItemIndex;

					}
					// Set the text in the proper row
					m_listReplacement.SetItemText(nSelectedItemIndex, TO_BE_REPLACED_COLUMN, zEnt1);
					m_listReplacement.SetItemText(nSelectedItemIndex, REPLACEMENT_COLUMN, zEnt2);
				}

				// Update the buttons
				updateButtons();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04320");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CReplaceStringsPP::OnClickedBtnRemoveReplacement(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get current selected item index
		int nItem = m_listReplacement.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		
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
					m_listReplacement.DeleteItem(nItem);
					
					nItem = m_listReplacement.GetNextItem(nItem - 1, ((nItem == 0) ? LVNI_ALL : LVNI_BELOW) | LVNI_SELECTED);
				}
				
				// if there's more item(s) below last deleted item, then set 
				// selection on the next item
				int nTotalNumOfItems = m_listReplacement.GetItemCount();
				if (nFirstItem < nTotalNumOfItems)
				{
					m_listReplacement.SetItemState(nFirstItem, LVIS_SELECTED, LVIS_SELECTED);
				}
				else if (nTotalNumOfItems > 0)
				{
					// select the last item
					m_listReplacement.SetItemState(nTotalNumOfItems - 1, LVIS_SELECTED, LVIS_SELECTED);
				}
				
				CRect rect;
				
				m_listReplacement.GetClientRect(&rect);
				
				// adjust the column width in case there is a vertical scrollbar now
				m_listReplacement.SetColumnWidth(0, rect.Width() / 2);
				m_listReplacement.SetColumnWidth(1, rect.Width() / 2);

				// Update the buttons
				updateButtons();
				
				SetDirty(TRUE);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04321");

	return 0;
}
//-------------------------------------------------------------------------------------------------
/*
LRESULT CReplaceStringsPP::OnClickedChkReplacementCaseSensitive(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_CASE2));
		// check whether the check box is checked
		int nChecked = checkBox.GetCheck();
		// update translate value object
		if (m_ipInternalReplacements)
		{
			m_ipInternalReplacements->IsCaseSensitive = nChecked==1?VARIANT_TRUE:VARIANT_FALSE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04322");

	return 0;
}*/
//-------------------------------------------------------------------------------------------------
LRESULT CReplaceStringsPP::OnKeydownListReplacement(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// only delete or del key can delete the selected item
		int nDelete = ::GetKeyState(VK_DELETE);
		if (nDelete < 0)
		{
			return OnClickedBtnRemoveReplacement(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04323");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CReplaceStringsPP::OnDblclkListReplacement(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return OnClickedBtnModifyReplacement(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
}
//-------------------------------------------------------------------------------------------------
LRESULT CReplaceStringsPP::OnClickedBtnDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get current selected item index
		int nSelectedItemIndex = m_listReplacement.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{
			// get the index of the item right below currently selected item
			int nBelowIndex = nSelectedItemIndex+1;

			// get selected item text from list
			string strEnt1 = getItemText(nSelectedItemIndex, TO_BE_REPLACED_COLUMN);
			string strEnt2 = getItemText(nSelectedItemIndex, REPLACEMENT_COLUMN);
			// then remove the selected item
			m_listReplacement.DeleteItem(nSelectedItemIndex);

			// now insert the item right before the item that was above
			int nActualIndex = m_listReplacement.InsertItem(nBelowIndex, strEnt1.c_str());
			m_listReplacement.SetItemText(nActualIndex, REPLACEMENT_COLUMN, strEnt2.c_str());

			// keep this item selected
			m_listReplacement.SetItemState(nActualIndex, LVIS_SELECTED, LVIS_SELECTED);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04946");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CReplaceStringsPP::OnClickedBtnUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get current selected item index
		int nSelectedItemIndex = m_listReplacement.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{
			// get the index of the item right above currently selected item
			int nAboveIndex = nSelectedItemIndex-1;
			if (nAboveIndex < 0)
			{
				return 0;
			}

			// get selected item text from list
			string strEnt1 = getItemText(nSelectedItemIndex, TO_BE_REPLACED_COLUMN);
			string strEnt2 = getItemText(nSelectedItemIndex, REPLACEMENT_COLUMN);
			// then remove the selected item
			m_listReplacement.DeleteItem(nSelectedItemIndex);

			// now insert the item right before the item that was above
			int nActualIndex = m_listReplacement.InsertItem(nAboveIndex, strEnt1.c_str());
			m_listReplacement.SetItemText(nActualIndex, REPLACEMENT_COLUMN, strEnt2.c_str());
			
			// keep this item selected
			m_listReplacement.SetItemState(nActualIndex, LVIS_SELECTED, LVIS_SELECTED);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04947");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CReplaceStringsPP::OnItemchangedListReplacement(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateButtons();
		
		// update up and down buttons
		updateUpAndDownButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04948");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CReplaceStringsPP::OnClickedBtnSaveFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// save replacement to m_ipInternalReplacements first
		if (!saveReplacements())
		{
			return 0;
		}

		// bring the pick file dialog and use it as a save dialog
		static CString zFileName(""), zDelimiter("");
		// show pick file dialog
		PickFileAndDelimiterDlg fileDlg(zFileName, zDelimiter, true, false);
		if (fileDlg.DoModal() == IDOK)
		{
			// get the file name as well as the delimiter
			zFileName = fileDlg.m_zFileName;
			zDelimiter = fileDlg.m_zDelimiter;

			// save the file
			if (m_ipInternalReplacements)
			{
				m_ipInternalReplacements->SaveReplaceInfoToFile(_bstr_t(zFileName), _bstr_t(zDelimiter));
			}
		}	
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05578");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CReplaceStringsPP::OnClickedClueDynamicListInfo(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14604");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
int CReplaceStringsPP::existsStringToBeReplaced(const std::string& strToBeReplaced)
{
	int nExistsIndex = -1;

	// get total items count
	int nTotalNum = m_listReplacement.GetItemCount();
	map<string, int> mapStringPairs;
	for (int n=0; n<nTotalNum; n++)
	{
		string strToBeReplaced(getItemText(n, TO_BE_REPLACED_COLUMN));
		mapStringPairs[strToBeReplaced] = n;
	}

	map<string, int>::iterator it = mapStringPairs.find(strToBeReplaced);
	if (it != mapStringPairs.end())
	{
		nExistsIndex = it->second;
	}

	return nExistsIndex;
}
//-------------------------------------------------------------------------------------------------
string CReplaceStringsPP::getItemText(int nIndex, int nColumnNum)
{
	string strItemText("");
	
	// get current selected item texts
	if (nIndex >= 0)
	{
		char pszValue[NUM_OF_CHARS];
		m_listReplacement.GetItemText(nIndex, nColumnNum, pszValue, NUM_OF_CHARS);

		strItemText = pszValue;
	}

	return strItemText;
}
//-------------------------------------------------------------------------------------------------
void CReplaceStringsPP::loadReplacements()
{
	m_listReplacement.DeleteAllItems();
	
	if (m_ipInternalReplacements)
	{
		IIUnknownVectorPtr ipReplacementStringPairs = m_ipInternalReplacements->Replacements;
		if (ipReplacementStringPairs)
		{
			long nSize = ipReplacementStringPairs->Size();
			int nActualIndex = 0;
			CString zEnt1, zEnt2;
			for (long nIndex = 0; nIndex < nSize; nIndex++)
			{
				IStringPairPtr ipStringPair(ipReplacementStringPairs->At(nIndex));
				if (ipStringPair)
				{
					zEnt1 = (char*)ipStringPair->StringKey;
					zEnt2 = (char*)ipStringPair->StringValue;
					nActualIndex = m_listReplacement.InsertItem(nIndex, zEnt1);
					m_listReplacement.SetItemText(nActualIndex, REPLACEMENT_COLUMN, zEnt2);
				}
			}
		}

		// whether or not to treat the replacement info strings as regular expression
		bool bAsRegExpr = m_ipInternalReplacements->AsRegularExpr==VARIANT_TRUE;
		CheckDlgButton(IDC_CHK_AS_REGEXP, bAsRegExpr?BST_CHECKED:BST_UNCHECKED); 

		// case-sensitivity
		bool bCaseSensitive = m_ipInternalReplacements->IsCaseSensitive==VARIANT_TRUE;
		CheckDlgButton(IDC_CHK_CASE2, bCaseSensitive?BST_CHECKED:BST_UNCHECKED); 

		
		CRect rect;
		
		m_listReplacement.GetClientRect(&rect);
		
		// adjust the column width in case there is a vertical scrollbar now
		m_listReplacement.SetColumnWidth(0, rect.Width() / 2);
		m_listReplacement.SetColumnWidth(1, rect.Width() / 2);
		
		SetDirty(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
bool CReplaceStringsPP::promptForReplacements(CString& zEnt1, CString& zEnt2, int& nItemIndex)
{
	CString zHeader(m_strFileHeader.c_str());
	Prompt2Dlg promptDlg("Enter Replacement String Pair",
				"Specify string to be replaced : ", zEnt1,
				"Specify string to replace : ", zEnt2, true, zHeader);

	while (true)
	{
		int nRes = promptDlg.DoModal();
		if (nRes == IDOK)
		{
			zEnt1 = promptDlg.m_zInput1;
			zEnt2 = promptDlg.m_zInput2;
			if (zEnt1.IsEmpty())
			{
				AfxMessageBox("Please provide non-empty string to be replaced.");
				continue;
			}

			// Get the first length(m_strFileHeader) characters of the value
			string strHeader((LPCTSTR)zEnt1.Left(m_strFileHeader.length()));
			makeLowerCase(strHeader);

			// If the string in the first edit box is a valid file name (has a valid header), and
			// this string doesn't contain only the header, it will not be treated as a file name
			if (strHeader == m_strFileHeader && zEnt1.CompareNoCase(m_strFileHeader.c_str()) != 0)
			{
				// If there is a string specified in the second edit box (string to replace)
				if (!zEnt2.IsEmpty())
				{
					// Create prompt string
					string strPrompt = "Since you have specified a dynamic file reference, the replacement string will be ignored.";
					MessageBox(strPrompt.c_str(), "Confirm file selection", MB_OK|MB_ICONINFORMATION);
					// Set the string to replace to empty string
					zEnt2 = "";
				}
			}

			// check whether or not the entered string already exists in the list
			int nExistingItemIndex = existsStringToBeReplaced((LPCTSTR)zEnt1);
			if (nExistingItemIndex>=0 && nExistingItemIndex != nItemIndex) // entry already exists in the list
			{
				CString zMsg("");
				zMsg.Format("<%s> already exists in the list. Do you wish to overwrite the existing entry?", zEnt1);
				nRes = MessageBox(zMsg, "Overwrite Existing?", MB_YESNO);
				if (nRes == IDYES)
				{
					nItemIndex = nExistingItemIndex;
					return true;
				}
				
				// user clicked NO, let's keep the prompt dlg
				continue;
			}
			
			// no duplicates found
			return true;
		}

		// user clicked Cancel
		break;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
bool CReplaceStringsPP::saveReplacements()
{
	IIUnknownVectorPtr ipStrPairs(CLSID_IUnknownVector);
	if (ipStrPairs)
	{
		// get total items count
		int nTotalNum = m_listReplacement.GetItemCount();
		if (nTotalNum==0)
		{
			MessageBox("Please specify one or more string replacement information.", "Configuration");
			return false;
		}
		
		for (int n=0; n<nTotalNum; n++)
		{
			// get item texts from list
			string strEnt1 = getItemText(n, TO_BE_REPLACED_COLUMN);
			string strEnt2 = getItemText(n, REPLACEMENT_COLUMN);
			// store them in the map
			IStringPairPtr ipStringPair(CLSID_StringPair);
			ipStringPair->StringKey = _bstr_t(strEnt1.c_str());
			ipStringPair->StringValue = _bstr_t(strEnt2.c_str());
			ipStrPairs->PushBack(ipStringPair);
		}

		m_ipInternalReplacements->Replacements = ipStrPairs;

		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void CReplaceStringsPP::updateUpAndDownButtons()
{
	// enable/disable up and down arrow key buttons appropriately
	// disable both
	m_btnUp.EnableWindow(FALSE);
	m_btnDown.EnableWindow(FALSE);
	
	// get current selected item index
	int nSelectedItemIndex = m_listReplacement.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
	int nTotalNum = m_listReplacement.GetItemCount();
	int nSelCount = m_listReplacement.GetSelectedCount();
	
	if ((nTotalNum > 1) && (nSelCount == 1))
	{
		if (nSelectedItemIndex == 0)
		{
			// First item selected
			// enable down button only
			m_btnUp.EnableWindow(FALSE);
			m_btnDown.EnableWindow(TRUE);
		}
		else if (nSelectedItemIndex > 0 && nSelectedItemIndex < nTotalNum-1)
		{
			// Some item other that first and last item selected
			// enable both buttons
			m_btnUp.EnableWindow(TRUE);
			m_btnDown.EnableWindow(TRUE);
		}
		else if (nSelectedItemIndex == nTotalNum-1)
		{
			// Last item selected
			// enable up button only
			m_btnUp.EnableWindow(TRUE);
			m_btnDown.EnableWindow(FALSE);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CReplaceStringsPP::updateButtons()
{
	int nSelCount = m_listReplacement.GetSelectedCount();
	int nCount = m_listReplacement.GetItemCount();
	
	if (nCount == 0)
	{
		m_btnModify.EnableWindow(FALSE);
		m_btnRemove.EnableWindow(FALSE);
	}
	else
	{
		m_btnModify.EnableWindow(nSelCount == 1 ? true : false);
		m_btnRemove.EnableWindow(nSelCount >= 1 ? true : false);
	}
}
//-------------------------------------------------------------------------------------------------
void CReplaceStringsPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07693", 
		"ReplaceStrings Modifier PP" );
}
//-------------------------------------------------------------------------------------------------
