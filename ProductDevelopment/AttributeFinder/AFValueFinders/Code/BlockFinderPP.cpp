// BlockFinderPP.cpp : Implementation of CBlockFinderPP
#include "stdafx.h"
#include "AFValueFinders.h"
#include "BlockFinderPP.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <comutils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

const int NUM_OF_CHARS = 4096;

//-------------------------------------------------------------------------------------------------
// CBlockFinderPP
//-------------------------------------------------------------------------------------------------
CBlockFinderPP::CBlockFinderPP()
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEBlockFinderPP;
		m_dwHelpFileID = IDS_HELPFILEBlockFinderPP;
		m_dwDocStringID = IDS_DOCSTRINGBlockFinderPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI05701")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinderPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CBlockFinderPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEFINDERSLib::IBlockFinderPtr ipBlockFinder = m_ppUnk[i];

			if(m_radioDefineSeparator.GetCheck() == BST_CHECKED)
			{
				ipBlockFinder->DefineBlocksType = UCLID_AFVALUEFINDERSLib::kSeparatorString;
				ipBlockFinder->InputAsOneBlock = m_chkAllIfNoSeparator.GetCheck() == BST_CHECKED ? VARIANT_TRUE : VARIANT_FALSE;
				// get block separator
				if (!storeSeparator(ipBlockFinder))
				{
					return S_FALSE;
				}
			}
			else if(m_radioDefineBeginEnd.GetCheck() == BST_CHECKED)
			{
				ipBlockFinder->DefineBlocksType = UCLID_AFVALUEFINDERSLib::kBeginAndEndString;
				ipBlockFinder->PairBeginAndEnd = m_chkPairBeginEnd.GetCheck() == BST_CHECKED ? VARIANT_TRUE : VARIANT_FALSE;
				// get block separator
				if (!storeBlockBegin(ipBlockFinder))
				{
					return S_FALSE;
				}
				// get block separator
				if (!storeBlockEnd(ipBlockFinder))
				{
					return S_FALSE;
				}
			}
			else 
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI10070");
			}
			
			// find what?
			bool bFindAll = IsDlgButtonChecked(IDC_RADIO_FIND_ALL)==BST_CHECKED;			
			ipBlockFinder->FindAllBlocks = bFindAll?VARIANT_TRUE:VARIANT_FALSE;
			if (!bFindAll)
			{	
				if (!storeMinNumber(ipBlockFinder))
				{
					return S_FALSE;
				}
				
				// clues can't be empty
				int nTotalNumOfClues = m_listClues.GetItemCount();
				if (nTotalNumOfClues==0)
				{
					MessageBox("Please specify one or more clues.", "Configuration");
					return S_FALSE;
				}

				// get all list items
				IVariantVectorPtr ipClues(CLSID_VariantVector);
				ASSERT_RESOURCE_ALLOCATION("ELI05731", ipClues!=NULL);
				char pszValue[NUM_OF_CHARS];
				for (long n=0; n<nTotalNumOfClues; n++)
				{
					// always get the first column item
					m_listClues.GetItemText(n, 0, pszValue, NUM_OF_CHARS);
					ipClues->PushBack(_bstr_t(pszValue));
				}
				ipBlockFinder->Clues = ipClues;
				
				bool bChecked = IsDlgButtonChecked(IDC_CHK_AS_REG_EXP)==BST_CHECKED;
				ipBlockFinder->IsClueRegularExpression = bChecked?VARIANT_TRUE:VARIANT_FALSE;
				bChecked = IsDlgButtonChecked(IDC_CHK_GET_MAX)==BST_CHECKED;
				ipBlockFinder->GetMaxOnly = bChecked?VARIANT_TRUE:VARIANT_FALSE;
				bChecked = IsDlgButtonChecked(IDC_CHK_PART_OF_WORD)==BST_CHECKED;
				ipBlockFinder->IsCluePartOfAWord = bChecked?VARIANT_TRUE:VARIANT_FALSE;
			}
		}
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05702")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinderPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CBlockFinderPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFVALUEFINDERSLib::IBlockFinderPtr ipBlockFinder = m_ppUnk[0];
		if (ipBlockFinder)
		{
			// create tooltip object
			m_infoTip.Create(CWnd::FromHandle(m_hWnd));
			// set no delay.
			m_infoTip.SetShowDelay(0);

			// init controls
			m_editMinNumOfClues = GetDlgItem(IDC_EDIT_MIN_NUMBER);
			m_btnAdd = GetDlgItem(IDC_BTN_ADD_BF);
			m_btnRemove = GetDlgItem(IDC_BTN_REMOVE_BF);
			m_btnModify = GetDlgItem(IDC_BTN_MODIFY_BF);
			m_chkAsRegExpr = GetDlgItem(IDC_CHK_AS_REG_EXP);
			m_chkFindMax = GetDlgItem(IDC_CHK_GET_MAX);
			m_chkPartOfWord = GetDlgItem(IDC_CHK_PART_OF_WORD);
			m_listClues = GetDlgItem(IDC_LIST_CLUES);
			m_listClues.SetExtendedListViewStyle(LVS_EX_GRIDLINES|LVS_EX_FULLROWSELECT);
			CRect rect;			
			m_listClues.GetClientRect(&rect);
			// make the first column as wide as the whole list box
			m_listClues.InsertColumn(0, "Dummy Header", LVCFMT_LEFT, rect.Width(), 0);

			m_chkAllIfNoSeparator = GetDlgItem(IDC_CHK_INPUT_AS_BLOCK);
			m_editSeparator = GetDlgItem(IDC_EDIT_SEPARATOR);

			m_radioDefineSeparator = GetDlgItem(IDC_RADIO_DEFINE_BLOCKS_SEPARATOR);
			m_radioDefineBeginEnd = GetDlgItem(IDC_RADIO_DEFINE_BLOCKS_BEGINEND);
			m_editBlockBegin = GetDlgItem(IDC_EDIT_BLOCK_BEGIN);
			m_editBlockEnd = GetDlgItem(IDC_EDIT_BLOCK_END);
			m_chkPairBeginEnd = GetDlgItem(IDC_CHK_PAIRBEGINEND);

			// get the separator
			string strSeparator = asString(ipBlockFinder->BlockSeparator);
			// convert the separator to normal readable format for display purpose
			::convertCppStringToNormalString(strSeparator);
			m_editSeparator.SetWindowText(strSeparator.c_str());
	
			// if no block separator is found, what to do?
			m_chkAllIfNoSeparator.SetCheck(ipBlockFinder->InputAsOneBlock==VARIANT_TRUE ? BST_CHECKED : BST_UNCHECKED);

			string strBlockBegin = asString(ipBlockFinder->BlockBegin);
			// convert the separator to normal readable format for display purpose
			::convertCppStringToNormalString(strBlockBegin);
			m_editBlockBegin.SetWindowText(strBlockBegin.c_str());

			string strBlockEnd = asString(ipBlockFinder->BlockEnd);
			// convert the separator to normal readable format for display purpose
			::convertCppStringToNormalString(strBlockEnd);
			m_editBlockEnd.SetWindowText(strBlockEnd.c_str());

			// should pairing be done??
			m_chkPairBeginEnd.SetCheck(ipBlockFinder->PairBeginAndEnd == VARIANT_TRUE ? 1 : 0);

			if (ipBlockFinder->DefineBlocksType == UCLID_AFVALUEFINDERSLib::kSeparatorString)
			{
				BOOL bTmp;
				OnClickedRadioDefineSeparator(0, 0, 0, bTmp);
				m_radioDefineSeparator.SetCheck(1);
			}
			else if (ipBlockFinder->DefineBlocksType == UCLID_AFVALUEFINDERSLib::kBeginAndEndString)
			{
				BOOL bTmp;
				OnClickedRadioDefineBeginEnd(0, 0, 0, bTmp);
				m_radioDefineBeginEnd.SetCheck(1);
			}

			// find what?
			bool bFindAllBlocks = ipBlockFinder->FindAllBlocks==VARIANT_TRUE;
			int nCheckBoxID = bFindAllBlocks ? IDC_RADIO_FIND_ALL : IDC_RADIO_BLOCK_WITH_CLUE;
			CheckDlgButton(nCheckBoxID, BST_CHECKED);

			// if 'Find blocks containing...' is selected, update its associated controls
			if (!bFindAllBlocks)
			{
				// minimum number of clues required
				long nMinNumOfClues = ipBlockFinder->MinNumberOfClues;
				SetDlgItemInt(IDC_EDIT_MIN_NUMBER, nMinNumOfClues, FALSE);
				// fill up the list box
				IVariantVectorPtr ipClues = ipBlockFinder->Clues;
				if (ipClues)
				{
					long nSize = ipClues->Size;
					if (nSize > 0)
					{
						int nIndexToInsert = 0;
						for (int n=0; n<nSize; n++)
						{
							string strClue = asString(_bstr_t(ipClues->GetItem(n)));
							m_listClues.InsertItem(nIndexToInsert, strClue.c_str());
							nIndexToInsert++;
						}
						// select the first item in the list
						m_listClues.SetItemState(0, LVIS_SELECTED, LVIS_SELECTED);
					}
				}

				m_listClues.GetClientRect(&rect);
				// adjust the column width in case there is a vertical scrollbar now
				m_listClues.SetColumnWidth(0, rect.Width());

				// regular expression?
				int nCheck = ipBlockFinder->IsClueRegularExpression==VARIANT_TRUE?1:0;
				m_chkAsRegExpr.SetCheck(nCheck);
				// find block containing max number of clues only?
				nCheck = ipBlockFinder->GetMaxOnly==VARIANT_TRUE?1:0;
				m_chkFindMax.SetCheck(nCheck);
				// each clue text is part of a word?
				nCheck = ipBlockFinder->IsCluePartOfAWord==VARIANT_TRUE?1:0;
				m_chkPartOfWord.SetCheck(nCheck);


			
			}

			// update certain controls' states
			updateControlStates();
		}
		
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05703");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBlockFinderPP::OnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CString zEnt;
		bool bSuccess = promptForValue(zEnt, m_listClues);

		if (bSuccess)
		{
			int nTotal = m_listClues.GetItemCount();
			// new item index
			int nIndex = m_listClues.InsertItem(nTotal, zEnt);
			for (int n = 0; n <= nTotal; n++)
			{
				// deselect any other items
				int nState = (n == nIndex) ? LVIS_SELECTED : 0;
				// only select current newly added item
				m_listClues.SetItemState(n, nState, LVIS_SELECTED);
			}

			// adjust the column width in case there is a vertical scrollbar
			CRect rect;
			m_listClues.GetClientRect(&rect);			
			m_listClues.SetColumnWidth(0, rect.Width());

			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05719");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBlockFinderPP::OnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get currently selected item
		int nSelectedItemIndex = m_listClues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex==LB_ERR)
		{
			return 0;
		}

		char pszValue[NUM_OF_CHARS];
		// get selected text
		m_listClues.GetItemText(nSelectedItemIndex, 0, pszValue, NUM_OF_CHARS);
		CString zEnt(pszValue);
		bool bSuccess = promptForValue(zEnt, m_listClues);
		if (bSuccess)
		{
			m_listClues.DeleteItem(nSelectedItemIndex);

			int nTotal = m_listClues.GetItemCount();
			
			int nIndex = m_listClues.InsertItem(nTotal, zEnt);
			
			for (int i = 0; i <= nTotal; i++)
			{
				int nState = (i == nIndex) ? LVIS_SELECTED : 0;
				
				m_listClues.SetItemState(i, nState, LVIS_SELECTED);
			}

			// adjust the column width in case there is a vertical scrollbar now
			CRect rect;
			m_listClues.GetClientRect(&rect);
			m_listClues.SetColumnWidth(0, rect.Width());

			// Set Dirty flag
			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05720");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBlockFinderPP::OnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get first selected item
		int nItem = m_listClues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		
		if (nItem != -1)
		{
			int nRes = MessageBox("Delete selected item?", "Confirm", MB_YESNO);
			if (nRes == IDYES)
			{
				// remove selected items
				int nFirstItem = nItem;
				
				// delete any selected item since this list ctrl allows multiple selection
				while(nItem != -1)
				{
					// remove from the UI listbox
					m_listClues.DeleteItem(nItem);
					// get next item selected item
					nItem = m_listClues.GetNextItem(nItem - 1, ((nItem == 0) ? LVNI_ALL : LVNI_BELOW) | LVNI_SELECTED);
				}
				
				// if there's more item(s) below last deleted item, then set 
				// selection on the next item
				int nTotalNumOfItems = m_listClues.GetItemCount();
				if (nFirstItem < nTotalNumOfItems)
				{
					m_listClues.SetItemState(nFirstItem, LVIS_SELECTED, LVIS_SELECTED);
				}
				else if (nTotalNumOfItems > 0)
				{
					// select the last item
					m_listClues.SetItemState(nTotalNumOfItems - 1, LVIS_SELECTED, LVIS_SELECTED);
				}
				
			}
		}
				
		// adjust the column width in case there is a vertical scrollbar now
		CRect rect;
		m_listClues.GetClientRect(&rect);
		m_listClues.SetColumnWidth(0, rect.Width());

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05721");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBlockFinderPP::OnClickedRadioBlockWithClue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05722");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBlockFinderPP::OnClickedRadioFindAll(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05723");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBlockFinderPP::OnItemchangedListClues(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05724");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBlockFinderPP::OnDblclkListClues(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	return OnClickedBtnModify(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
}
//-------------------------------------------------------------------------------------------------
LRESULT CBlockFinderPP::OnClickedSeparatorInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("- Interpretation : C++ string.\n"
					  "  For example, \"\\t\" will be interpreted as the Tab\n"
					  "  character, \"\\r\\n\" will be considered as one Carriage\n" 
					  "  Return character and one New Line character.\n\n"
					  "- Case sensitive : Yes");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06848");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBlockFinderPP::OnClickedCluesInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("- Interpretation : String literal or regular expression,\n"
					  "  depends upon the state of regular expression checkbox.\n\n"
					  "- Case sensitive : No\n\n"
					  "- Dynamically loading a string list from a file is supported.\n"
					  "- To specify a dynamic file, an entry must begin with \"file://\".\n"
					  "- A file may be specified in combination with static entries or\n"
					  "  additional dynamic lists.\n"
					  "- Path tags such as <RSDFileDir> and <ComponentDataDir> may be used.\n"
					  "- For example, if an entry in the list is file://<RSDFileDir>\\list.txt,\n"
					  "  the entry will be replaced dynamically at runtime with the contents\n"
					  "  of the file.\n");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06849");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBlockFinderPP::OnClickedWordPartInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("If this box is checked, any word from the input text\n"
					  "containing one of the clues will be considered as a hit.\n"
					  "For instance, if a clue is defined as \"one\", and the\n"
					  "box is checked, \"one\", \"bone\", \"tone\", \"honest\"\n"
					  "will all be counted. If the box is unchecked, only the\n"
					  "word \"one\" is counted");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06851");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBlockFinderPP::OnClickedMaxCluesInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("Example:\n"
					  "Block #1 has total of three of the clues defined,\n"
					  "Block #2 has two of them, Block #3 has one and\n"
					  "Block #4 has three. As a result, Block #1 and #4\n"
					  "are considered as the blocks containing maximum\n"
					  "number of clues.");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06850");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBlockFinderPP::OnClickedRadioDefineSeparator(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_chkAllIfNoSeparator.EnableWindow(TRUE);
		m_editSeparator.EnableWindow(TRUE);

		m_editBlockBegin.EnableWindow(FALSE);
		m_editBlockEnd.EnableWindow(FALSE);
		m_chkPairBeginEnd.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10068");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBlockFinderPP::OnClickedRadioDefineBeginEnd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_chkAllIfNoSeparator.EnableWindow(FALSE);
		m_editSeparator.EnableWindow(FALSE);

		m_editBlockBegin.EnableWindow(TRUE);
		m_editBlockEnd.EnableWindow(TRUE);
		m_chkPairBeginEnd.EnableWindow(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10069");

	return 0;
}
//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool CBlockFinderPP::storeSeparator(UCLID_AFVALUEFINDERSLib::IBlockFinderPtr ipBlockFinder)
{
	try
	{
		// get block separator
		CComBSTR bstrSeparators;
		m_editSeparator.GetWindowText(&bstrSeparators);
		string strSeparator = asString(bstrSeparators);
		// if user input some escape squence, convert them into cpp recognizable string
		// ex. \\n -> \n, \\ -> \, etc.
		::convertNormalStringToCppString(strSeparator);
		ipBlockFinder->BlockSeparator = _bstr_t(strSeparator.c_str());

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05828");
	m_editSeparator.SetSel(0, -1);
	m_editSeparator.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CBlockFinderPP::storeBlockBegin(UCLID_AFVALUEFINDERSLib::IBlockFinderPtr ipBlockFinder)
{
	try
	{
		// get block separator
		CComBSTR bstrBlockBegin;
		m_editBlockBegin.GetWindowText(&bstrBlockBegin);
		string strBlockBegin = asString(bstrBlockBegin);
		// if user input some escape squence, convert them into cpp recognizable string
		// ex. \\n -> \n, \\ -> \, etc.
		::convertNormalStringToCppString(strBlockBegin);
		ipBlockFinder->BlockBegin = _bstr_t(strBlockBegin.c_str());

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10071");
	
	m_editBlockBegin.SetSel(0, -1);
	m_editBlockBegin.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CBlockFinderPP::storeBlockEnd(UCLID_AFVALUEFINDERSLib::IBlockFinderPtr ipBlockFinder)
{
	try
	{
		// get block separator
		CComBSTR bstrBlockEnd;
		m_editBlockEnd.GetWindowText(&bstrBlockEnd);
		string strBlockEnd = asString(bstrBlockEnd);
		// if user input some escape squence, convert them into cpp recognizable string
		// ex. \\n -> \n, \\ -> \, etc.
		::convertNormalStringToCppString(strBlockEnd);
		ipBlockFinder->BlockEnd = _bstr_t(strBlockEnd.c_str());

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19381");
	
	m_editBlockEnd.SetSel(0, -1);
	m_editBlockEnd.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CBlockFinderPP::storeMinNumber(UCLID_AFVALUEFINDERSLib::IBlockFinderPtr ipBlockFinder)
{
	try
	{
		// if 'Find blocks containing...' is selected
		// minimun number of clues must be defined and greater than 0
		int nMinNumber = GetDlgItemInt(IDC_EDIT_MIN_NUMBER, NULL, FALSE);
		ipBlockFinder->MinNumberOfClues = nMinNumber;
		
		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05829");
	
	ATLControls::CEdit editMin(GetDlgItem(IDC_EDIT_MIN_NUMBER));
	editMin.SetSel(0, -1);
	editMin.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
void CBlockFinderPP::updateButtonStates()
{
	// enable/disable buttons according the number of
	// items selected in the list box
	int nSelCount = m_listClues.GetSelectedCount();
	int nCount = m_listClues.GetItemCount();
	
	if (nCount == 0)
	{
		m_btnModify.EnableWindow(FALSE);
		m_btnRemove.EnableWindow(FALSE);
	}
	else
	{
		m_btnModify.EnableWindow(nSelCount == 1 ? TRUE : FALSE);
		m_btnRemove.EnableWindow(nSelCount >= 1 ? TRUE : FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
void CBlockFinderPP::updateControlStates()
{
	BOOL bEnable = FALSE;
	if (IsDlgButtonChecked(IDC_RADIO_BLOCK_WITH_CLUE))
	{
		bEnable = TRUE;
	}

	// check state of the radio buttons
	m_editMinNumOfClues.EnableWindow(bEnable);
	if (bEnable)
	{
		int nMinNumber = GetDlgItemInt(IDC_EDIT_MIN_NUMBER, NULL, FALSE);
		if (nMinNumber <= 0)
		{
			SetDlgItemInt(IDC_EDIT_MIN_NUMBER, 1, FALSE);
		}
	}

	m_listClues.EnableWindow(bEnable);
	m_btnAdd.EnableWindow(bEnable);
	m_btnRemove.EnableWindow(bEnable);
	m_btnModify.EnableWindow(bEnable);
	m_chkAsRegExpr.EnableWindow(bEnable);
	m_chkFindMax.EnableWindow(bEnable);
	m_chkPartOfWord.EnableWindow(bEnable);

	// update Remove/Modify buttons' state
	if (bEnable)
	{
		updateButtonStates();
	}
}
//-------------------------------------------------------------------------------------------------
void CBlockFinderPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07679", "BlockFinder PP" );
}
//-------------------------------------------------------------------------------------------------
