// TranslateValuePP.cpp : Implementation of CTranslateValuePP
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "TranslateValuePP.h"

#include <PickFileAndDelimiterDlg.h>

#include <UCLIDException.h>
#include <Prompt2Dlg.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <comutils.h>

#include <map>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int	giTRANS_FROM_COLUMN			= 0;
const int	giTRANS_TO_COLUMN			= 1;

const int NUM_OF_CHARS = 4096;

//-------------------------------------------------------------------------------------------------
// CTranslateValuePP
//-------------------------------------------------------------------------------------------------
CTranslateValuePP::CTranslateValuePP()
: m_ipInternalTransValue(CLSID_TranslateValue)
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLETranslateValuePP;
		m_dwHelpFileID = IDS_HELPFILETranslateValuePP;
		m_dwDocStringID = IDS_DOCSTRINGTranslateValuePP;

		// Create an IMiscUtilsPtr object
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI30074", ipMiscUtils != __nullptr );

		// Get the file header string and its length from IMiscUtilsPtr object
		m_strFileHeader = ipMiscUtils->GetFileHeader();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07722")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValuePP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CTranslateValuePP::Apply\n"));

		if (!saveAllTranslationPairsFromList())
		{
			return S_FALSE;
		}

		for (UINT i = 0; i < m_nObjects; i++)
		{
			ICopyableObjectPtr ipTranslateValue = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI08387", ipTranslateValue != __nullptr);

			if(m_ipInternalTransValue->TranslateFieldType == UCLID_AFVALUEMODIFIERSLib::kTranslateType)
			{
				// Test to ensure all the strings are valid types
				IIUnknownVectorPtr ipTransPairs = m_ipInternalTransValue->TranslationStringPairs;
				ASSERT_RESOURCE_ALLOCATION("ELI09667", ipTransPairs != __nullptr);
			
				// A temporary attribute for type validation
				IAttributePtr ipAttr(CLSID_Attribute);
				ASSERT_RESOURCE_ALLOCATION("ELI09666", ipAttr != __nullptr);
				int j;
				for(j = 0; j < ipTransPairs->Size(); j++)
				{
					IStringPairPtr ipPair = ipTransPairs->At(j);
					ASSERT_RESOURCE_ALLOCATION("ELI09668", ipPair != __nullptr);
					_bstr_t bstrKey(ipPair->StringKey);
					_bstr_t bstrValue(ipPair->StringValue);
					string strKey = asString(bstrKey);
					string strValue = asString(bstrValue);

					// If a dynamic list has not been supplied, check to see that the replacement
					// value is permitted.
					if (_stricmp(strKey.substr(0, m_strFileHeader.length()).c_str(), 
								 m_strFileHeader.c_str()) != 0)
					{
						ipAttr->Type = bstrKey;
						ipAttr->Type = bstrValue;
					}
				}
			}
			// copy all contents from internal object to the 
			// translate value object that associated with this property page
			ICopyableObjectPtr ipCopyableObj(m_ipInternalTransValue);
			if (ipCopyableObj)
			{
				ipTranslateValue->CopyFrom(ipCopyableObj);
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04243");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValuePP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CTranslateValuePP::OnInitDialog(UINT uMsg, WPARAM wParam, 
										LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_radioTranslateValue = GetDlgItem(IDC_RADIO_TRANSLATE_VALUE);
		m_radioTranslateType = GetDlgItem(IDC_RADIO_TRANSLATE_TYPE);

		// Determine column widths for list
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_TRANS_VALUE));
		CRect rectList;
		lst.GetClientRect(rectList);
		long nWidth1 = rectList.Width() / 2;
		long nWidth2 = rectList.Width() - nWidth1;

		// Prepare lst
		lst.SetExtendedListViewStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
		lst.InsertColumn(giTRANS_FROM_COLUMN, "Translate From", LVCFMT_LEFT,
							nWidth1, giTRANS_FROM_COLUMN);
		lst.InsertColumn(giTRANS_TO_COLUMN, "Translate To", LVCFMT_LEFT,
							nWidth2, giTRANS_TO_COLUMN);

		
		// get the translate value associated with this property page.
		// assume that this property page has one and only one coclass associated with it
		UCLID_AFVALUEMODIFIERSLib::ITranslateValuePtr ipTransValue(m_ppUnk[0]);
		if (ipTransValue)
		{
			ICopyableObjectPtr ipCopyableObj(ipTransValue);
			if (ipCopyableObj)
			{
				// copy all properies from that translate value to internal translate value object.
				ICopyableObjectPtr ipCopy = m_ipInternalTransValue;
				ASSERT_RESOURCE_ALLOCATION("ELI08386", ipCopy != __nullptr);
				ipCopy->CopyFrom(ipCopyableObj);
				loadTranslations();
			}
		}

		// Create and initialize the info tip control
		m_infoTip.Create(CWnd::FromHandle(m_hWnd));
		m_infoTip.SetShowDelay(0);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19286");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateValuePP::OnClickedBtnAdd(WORD wNotifyCode, WORD wID, 
										   HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// add entry to the list view
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_TRANS_VALUE));
		int nTotalItems = lst.GetItemCount();
		int nNewIndex = 0;
		if (nTotalItems>0)
		{
			nNewIndex = nTotalItems;
		}

		// show prompt dialog for entering translation infos
		CString zEnt1, zEnt2;
		int nExistingItemIndex = -1;
		bool bSuccess = promptForTranslations(zEnt1, zEnt2, nExistingItemIndex);
		if (bSuccess)
		{
			if (nExistingItemIndex>=0)
			{
				lst.SetItemText(nExistingItemIndex, giTRANS_FROM_COLUMN, 
					zEnt1);
				lst.SetItemText(nExistingItemIndex, giTRANS_TO_COLUMN, 
					zEnt2);

				for (int i = 0; i <= lst.GetItemCount(); i++)
				{
					int nState = (i == nExistingItemIndex) ? LVIS_SELECTED : 0;
					
					lst.SetItemState(i, nState, LVIS_SELECTED);
				}
			}
			else
			{
				int nActualIndex = lst.InsertItem(nNewIndex, zEnt1);
				lst.SetItemText(nActualIndex, giTRANS_TO_COLUMN, zEnt2);

				for (int i = 0; i <= lst.GetItemCount(); i++)
				{
					int nState = (i == nActualIndex) ? LVIS_SELECTED : 0;
					
					lst.SetItemState(i, nState, LVIS_SELECTED);
				}
			}
				
			CRect rect;
			
			lst.GetClientRect(&rect);
			
			// adjust the column width in case there is a vertical scrollbar now
			lst.SetColumnWidth(0, rect.Width() / 2);
			lst.SetColumnWidth(1, rect.Width() / 2);
			
			SetDirty(TRUE);

		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04246");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateValuePP::OnClickedBtnLoadFromFile(WORD wNotifyCode, WORD wID, 
													HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_TRANS_VALUE));
		if (lst.GetItemCount() > 0)
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
			if (m_ipInternalTransValue)
			{
				// pass the file name to translate value to get translations
				m_ipInternalTransValue->LoadTranslationsFromFile(_bstr_t(zFileName), _bstr_t(zDelimiter));

				// get map of translations
				loadTranslations();
			}

			
			CRect rect;
			
			lst.GetClientRect(&rect);
			
			// adjust the column width in case there is a vertical scrollbar now
			lst.SetColumnWidth(0, rect.Width() / 2);
			lst.SetColumnWidth(1, rect.Width() / 2);
			
			SetDirty(TRUE);		
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04247");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateValuePP::OnClickedBtnModify(WORD wNotifyCode, WORD wID, 
											  HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_TRANS_VALUE));
		
		// get current selected item index
		int nSelectedItemIndex = lst.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);

		if (nSelectedItemIndex >= 0)
		{
			CString zEnt1 = getItemText(nSelectedItemIndex, giTRANS_FROM_COLUMN).c_str();
			CString zEnt2 = getItemText(nSelectedItemIndex, giTRANS_TO_COLUMN).c_str();

			int nExistingItemIndex = nSelectedItemIndex;
			bool bSuccess = promptForTranslations(zEnt1, zEnt2, nExistingItemIndex);
			// show prompt dialog for entering translation infos
			if (bSuccess)
			{
				// if the entry already exists, then replace the old one with the new one
				if (nExistingItemIndex>=0 && nExistingItemIndex != nSelectedItemIndex)
				{
					nSelectedItemIndex = nExistingItemIndex;
				}

				lst.SetItemText(nSelectedItemIndex, giTRANS_FROM_COLUMN, zEnt1);
				lst.SetItemText(nSelectedItemIndex, giTRANS_TO_COLUMN, zEnt2);				
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04248");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateValuePP::OnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get current selected item index
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_TRANS_VALUE));
	
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
				
				
				CRect rect;
				
				lst.GetClientRect(&rect);
				
				// adjust the column width in case there is a vertical scrollbar now
				lst.SetColumnWidth(0, rect.Width() / 2);
				lst.SetColumnWidth(1, rect.Width() / 2);
				
				SetDirty(TRUE);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04249");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateValuePP::OnClickedRadioTranslateValue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_ipInternalTransValue->TranslateFieldType = UCLID_AFVALUEMODIFIERSLib::kTranslateValue;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09658");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateValuePP::OnClickedRadioTranslateType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_ipInternalTransValue->TranslateFieldType = UCLID_AFVALUEMODIFIERSLib::kTranslateType;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09659");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateValuePP::OnClickedChkCaseSensitive(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_CASE1));
		// check whether the check box is checked
		int nChecked = checkBox.GetCheck();
		// update translate value object
		if (m_ipInternalTransValue)
		{
			m_ipInternalTransValue->IsCaseSensitive = nChecked==1?VARIANT_TRUE:VARIANT_FALSE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04269");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateValuePP::OnDblclkListTransValue(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return OnClickedBtnModify(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateValuePP::OnKeydownListTransValue(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	// only delete or del key can delete the selected item
	int nRes = ::GetKeyState(VK_DELETE);
	if (nRes < 0)
	{
		return OnClickedBtnRemove(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
	}

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateValuePP::OnClickedBtnSaveFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (!saveAllTranslationPairsFromList())
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
			if (m_ipInternalTransValue)
			{
				m_ipInternalTransValue->SaveTranslationsToFile(_bstr_t(zFileName), _bstr_t(zDelimiter));
			}
		}	
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05580");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateValuePP::OnListItemChanged(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	updateButtons();

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTranslateValuePP::OnClickedClueDynamicListInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl,
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30075");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
int CTranslateValuePP::existsTranslateFromString(const std::string& strTranslateFrom)
{
	int nExistsIndex = -1;

	ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_TRANS_VALUE));
	// get total items count
	int nTotalNum = lst.GetItemCount();
	map<string, int> mapTransToIndex;
	for (int n=0; n<nTotalNum; n++)
	{
		string strTrans(getItemText(n, giTRANS_FROM_COLUMN));
		mapTransToIndex[strTrans] = n;
	}

	map<string, int>::iterator it = mapTransToIndex.find(strTranslateFrom);
	if (it != mapTransToIndex.end())
	{
		nExistsIndex = it->second;
	}

	return nExistsIndex;
}
//-------------------------------------------------------------------------------------------------
bool CTranslateValuePP::saveAllTranslationPairsFromList()
{
	IIUnknownVectorPtr ipTranslationPairs(CLSID_IUnknownVector);
	if (ipTranslationPairs)
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_TRANS_VALUE));
		// get total items count
		int nTotalNum = lst.GetItemCount();
		if (nTotalNum==0)
		{
			MessageBox("Please specify one or more translation information.", "Configuration");
			return false;
		}

		for (int n=0; n<nTotalNum; n++)
		{
			string strEnt1 = getItemText(n, giTRANS_FROM_COLUMN);
			string strEnt2 = getItemText(n, giTRANS_TO_COLUMN);
			IStringPairPtr ipStringPair(CLSID_StringPair);
			ipStringPair->StringKey = _bstr_t(strEnt1.c_str());
			ipStringPair->StringValue = _bstr_t(strEnt2.c_str());
			ipTranslationPairs->PushBack(ipStringPair);
		}

		m_ipInternalTransValue->TranslationStringPairs = ipTranslationPairs;

		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
string CTranslateValuePP::getItemText(int nIndex, int nColumnNum)
{
	string strItemText("");
	
	ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_TRANS_VALUE));
	// get current selected item texts
	if (nIndex >= 0)
	{
		char pszValue[NUM_OF_CHARS];
		lst.GetItemText(nIndex, nColumnNum, pszValue, NUM_OF_CHARS);

		strItemText = pszValue;
	}

	return strItemText;
}
//-------------------------------------------------------------------------------------------------
void CTranslateValuePP::loadTranslations()
{
	ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_TRANS_VALUE));
	lst.DeleteAllItems();
	
	if (m_ipInternalTransValue)
	{
		IIUnknownVectorPtr ipTranslations = m_ipInternalTransValue->TranslationStringPairs;
		if (ipTranslations) 
		{
			long nSize = ipTranslations->Size();
			int nActualIndex = 0;
			CString zEnt1, zEnt2;
			for (long nIndex = 0; nIndex < nSize; nIndex++)
			{
				IStringPairPtr ipStringPair(ipTranslations->At(nIndex));
				if (ipStringPair)
				{
					zEnt1 = (char*)ipStringPair->StringKey;
					zEnt2 = (char*)ipStringPair->StringValue;
					nActualIndex = lst.InsertItem(nIndex, zEnt1);
					lst.SetItemText(nActualIndex, giTRANS_TO_COLUMN, zEnt2);
				}
			}
			
			CRect rect;
			
			lst.GetClientRect(&rect);
			
			// adjust the column width in case there is a vertical scrollbar now
			lst.SetColumnWidth(0, rect.Width() / 2);
			lst.SetColumnWidth(1, rect.Width() / 2);

			if(m_ipInternalTransValue->TranslateFieldType == kTranslateValue)
			{
				m_radioTranslateValue.SetCheck(1);
			}
			else if(m_ipInternalTransValue->TranslateFieldType == kTranslateType)
			{
				m_radioTranslateType.SetCheck(1);
			}
			else
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI09657");
			}
		}
		// case-sensitivity
		VARIANT_BOOL bCaseSensitive = m_ipInternalTransValue->IsCaseSensitive;
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_CASE1));
		checkBox.SetCheck(bCaseSensitive==VARIANT_TRUE?1:0);
	}
}
//-------------------------------------------------------------------------------------------------
bool CTranslateValuePP::promptForTranslations(CString& zEnt1, CString& zEnt2, int& nItemIndex)
{
	Prompt2Dlg promptDlg("Enter Translation String Pair",
				"Specify string to be translated from : ", zEnt1,
				"Specify string to be translated to : ", zEnt2, true, m_strFileHeader.c_str());

	while (true)
	{
		int nRes = promptDlg.DoModal();
		if (nRes == IDOK)
		{
			// Retrieve the two strings
			zEnt1 = promptDlg.m_zInput1;
			zEnt2 = promptDlg.m_zInput2;

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

			// If translating type validate the type name
			if (m_radioTranslateType.GetCheck() == BST_CHECKED)
			{
				if (!zEnt2.IsEmpty() && !isValidIdentifier((LPCTSTR)zEnt2))
				{
					MessageBox("The value to translate to is an invalid type identifier.",
						"Invalid Identifier", MB_OK | MB_ICONERROR);

					continue;
				}
			}

			// check whether or not the entered the zEnt1 already exists in the lst
			int nExistingItemIndex = existsTranslateFromString((LPCTSTR)zEnt1);
			if (nExistingItemIndex>=0 && nExistingItemIndex != nItemIndex) // entry already exists in the lst
			{
				CString zMsg("");
				zMsg.Format("<%s> already exists in the lst. Do you wish to overwrite the existing entry?", zEnt1);
				nRes = MessageBox(zMsg, "Overwrite Existing?", MB_YESNO);
				if (nRes == IDYES)
				{
					nItemIndex = nExistingItemIndex;
					return true;
				}

				continue;
			}

			// can't find any duplicates, simply return
			return true;
		}

		// user clicked cancel
		break;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void CTranslateValuePP::updateButtons()
{
	ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_TRANS_VALUE));
	int nSelCount = lst.GetSelectedCount();
	int nCount = lst.GetItemCount();
	
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
void CTranslateValuePP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07700", 
		"TranslateValue Modifier PP" );
}
//-------------------------------------------------------------------------------------------------
