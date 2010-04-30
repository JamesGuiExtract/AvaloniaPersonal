// ValueBeforeCluePP.cpp : Implementation of CValueBeforeCluePP
#include "stdafx.h"
#include "AFValueFinders.h"
#include "ValueBeforeCluePP.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <comutils.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int NUM_OF_CHARS = 4096;

//-------------------------------------------------------------------------------------------------
// CValueBeforeCluePP
//-------------------------------------------------------------------------------------------------
CValueBeforeCluePP::CValueBeforeCluePP() 
: m_eRefiningType(UCLID_AFVALUEFINDERSLib::kNoRefiningType),
  m_bCaseSensitive(false),
  m_strOtherPunctuations(""),
  m_strOtherStops(""),
  m_nNumOfWords(0),
  m_nNumOfLines(0),
  m_strSpecifiedString(""),
  m_bStopAtNewLine(false),
  m_bStopForOther(false),
  m_bSpacesAsPunctuations(false),
  m_bSpecifyOtherPunctuations(false),
  m_bIncludeClueLine(false)
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEValueBeforeCluePP;
		m_dwHelpFileID = IDS_HELPFILEValueBeforeCluePP;
		m_dwDocStringID = IDS_DOCSTRINGValueBeforeCluePP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI04634")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeCluePP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CValueBeforeCluePP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEFINDERSLib::IValueBeforeCluePtr ipValueBeforeClue(m_ppUnk[i]);
			if (ipValueBeforeClue)
			{
				// get lst of clues from the lst box
				IVariantVectorPtr ipClues = ipValueBeforeClue->Clues;
				if (ipClues)
				{
					ipClues->Clear();
					ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_BEFORE_CLUE));
					char pszValue[NUM_OF_CHARS];
					int nSize = lst.GetItemCount();
					if (nSize<=0)
					{
						MessageBox("Please provide one or more clue text.",
							"Invalid Configuration", MB_OK | MB_ICONERROR);
						lst.SetFocus();
						return S_FALSE;
					}

					for (int n=0; n< nSize; n++)
					{
						lst.GetItemText(n, 0, pszValue, NUM_OF_CHARS);
						ipClues->PushBack(_bstr_t(pszValue));
					}			
				}
				
				// get ClueAsRegExpr
				bool bClueAsRegExpr = IsDlgButtonChecked(IDC_CHK_AS_REGEXPR_BC)==BST_CHECKED;
				ipValueBeforeClue->ClueAsRegExpr = bClueAsRegExpr?VARIANT_TRUE:VARIANT_FALSE;

				// get case sensitivity
				ipValueBeforeClue->IsCaseSensitive = m_bCaseSensitive?VARIANT_TRUE:VARIANT_FALSE;
				
				// get current refining type
				switch (m_eRefiningType)
				{
				case kNoRefiningType:
					{
						ipValueBeforeClue->SetNoRefiningType();
					}
					break;
				case kClueLine:
					{
						ipValueBeforeClue->SetClueLineType();
					}
					break;
				case kUptoXWords:
					{
						// must be a positive number
						if (m_nNumOfWords<=0)
						{
							MessageBox("Please specify a positive number of words.",
								"Invalid Configuration", MB_OK | MB_ICONERROR);
							ATLControls::CEdit editNumOfWords(GetDlgItem(IDC_EDIT_NUM_OF_WORDS_BC));
							editNumOfWords.SetSel(0, -1);
							editNumOfWords.SetFocus();

							return S_FALSE;
						}

						if (!m_bSpacesAsPunctuations && !m_bSpecifyOtherPunctuations)
						{
							MessageBox("No punctuation characters were specified for seperating "
								"words.\r\nSpace, tab and new line characters will be used by default.",
								"Default Setting", MB_OK | MB_ICONWARNING);
							m_bSpacesAsPunctuations = true;
						}

						string strPuncs("");
						if (m_bSpacesAsPunctuations)
						{
							strPuncs += " \t\n\r";
						}
						if (m_bSpecifyOtherPunctuations)
						{
							if (m_strOtherPunctuations.empty())
							{
								MessageBox("Please specify character(s) as word separator(s).",
									"Invalid Configuration", MB_OK | MB_ICONERROR);
								ATLControls::CEdit editOthers(GetDlgItem(IDC_EDIT_OTHER_PUNC_BC));
								editOthers.SetSel(0, -1);
								editOthers.SetFocus();
								return S_FALSE;
							}

							strPuncs += m_strOtherPunctuations;
						}

						if (m_bStopForOther)
						{
							if (m_strOtherStops.empty())
							{
								MessageBox("Please specify character(s) after which to stop searching.",
									"Invalid Configuration", MB_OK | MB_ICONERROR);
								ATLControls::CEdit editStops( GetDlgItem(IDC_EDIT_OTHER_STOP_BC) );
								editStops.SetSel(0, -1);
								editStops.SetFocus();
								return S_FALSE;
							}
						}
						else
						{
							// empty the contents of the m_strOtherStops
							m_strOtherStops = "";
						}

						ipValueBeforeClue->SetUptoXWords(m_nNumOfWords, _bstr_t(strPuncs.c_str()), 
							m_bStopAtNewLine ? VARIANT_TRUE : VARIANT_FALSE, 
							_bstr_t(m_strOtherStops.c_str()));
					}
					break;
				case kUptoXLines:
					{
						// must be a positive number
						if (m_nNumOfLines<=0)
						{
							MessageBox("Please specify a positive number of lines.",
								"Invalid Configuration", MB_OK | MB_ICONERROR);
							ATLControls::CEdit editNumOfLines(GetDlgItem(IDC_EDIT_NUM_OF_LINES_BC));
							editNumOfLines.SetSel(0, -1);
							editNumOfLines.SetFocus();
							return S_FALSE;
						}

						ipValueBeforeClue->SetUptoXLines(m_nNumOfLines, 
									m_bIncludeClueLine?VARIANT_TRUE:VARIANT_FALSE);
					}
					break;
				case kClueToString:
					{
						if (m_strSpecifiedString.empty())
						{
							MessageBox("Please provide non-empty string.",
								"Invalid Configuration", MB_OK | MB_ICONERROR);
							ATLControls::CEdit editString(GetDlgItem(IDC_EDIT_STRING_SPEC_BC));
							editString.SetSel(0, -1);
							editString.SetFocus();
							return S_FALSE;
						}

						ipValueBeforeClue->SetClueToString(_bstr_t(m_strSpecifiedString.c_str()));
				
						// get ClueToStringAsRegExpr
						bool bAsRegExpr = IsDlgButtonChecked( IDC_CHK_STRING_AS_REGEXPR_BC ) == BST_CHECKED;
						ipValueBeforeClue->ClueToStringAsRegExpr = 
							(bAsRegExpr ? VARIANT_TRUE : VARIANT_FALSE);
					}
					break;
				}
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04635");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeCluePP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CValueBeforeCluePP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// which refining type is selected
		UCLID_AFVALUEFINDERSLib::IValueBeforeCluePtr ipValueBeforeClue(m_ppUnk[0]);
		if (ipValueBeforeClue)
		{	
			// create tooltip object
			m_infoTip.Create(CWnd::FromHandle(m_hWnd));
			// set no delay.
			m_infoTip.SetShowDelay(0);

			// populate the clue text lst
			ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_BEFORE_CLUE));
			lst.SetExtendedListViewStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
			
			CRect rect;
			
			lst.GetClientRect(&rect);
			
			lst.InsertColumn( 0, "Value", LVCFMT_LEFT, 
				rect.Width(), 0 );
			
			lst.DeleteAllItems();
			
			// lst of values
			IVariantVectorPtr ipVecClues(ipValueBeforeClue->Clues);
			if (ipVecClues)
			{
				CString zValue("");
				long nSize = ipVecClues->Size;
				for (long n=0; n<nSize; n++)
				{
					zValue = (char*)_bstr_t(ipVecClues->GetItem(n));
					lst.InsertItem(n, zValue);
				}
				// set selection to the first item
				if (nSize > 0)
				{
					lst.SetItemState(0, LVIS_SELECTED, LVIS_SELECTED);
				}
			}

			
			lst.GetClientRect(&rect);
			
			// adjust the column width in case there is a vertical scrollbar now
			lst.SetColumnWidth(0, rect.Width());
			
			// Whether or not defined clue texts will be treated 
			// as regular expressions
			bool bClueAsRegExpr = ipValueBeforeClue->ClueAsRegExpr==VARIANT_TRUE;
			CheckDlgButton(IDC_CHK_AS_REGEXPR_BC, 
							bClueAsRegExpr?BST_CHECKED:BST_UNCHECKED);

			// case-sensitivity
			m_bCaseSensitive = ipValueBeforeClue->IsCaseSensitive==VARIANT_TRUE;
			ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_CASE_BC));
			checkBox.SetCheck(m_bCaseSensitive?1:0);

			// get currently selected refining type
			m_eRefiningType = ipValueBeforeClue->RefiningType;
			// set radio check and its associated check boxes and edit boxes
			selectRefiningType();
			int nID = 0;
			switch (m_eRefiningType)
			{
			case kNoRefiningType:
				{
					nID = IDC_RADIO_NO_TYPE_BC;
				}
				break;
			case kClueLine:
				{
					nID = IDC_RADIO_CLUE_LINE_BC;
				}
				break;
			case kUptoXWords:
				{
					nID = IDC_RADIO_UPTO_XWORDS_BC;
					CComBSTR bstrPunctuations, bstrStops;
					VARIANT_BOOL bStopAtNewLine(VARIANT_TRUE);
					ipValueBeforeClue->GetUptoXWords( &m_nNumOfWords, &bstrPunctuations, 
						&bStopAtNewLine, &bstrStops );

					// set num of words
					SetDlgItemInt(IDC_EDIT_NUM_OF_WORDS_BC, m_nNumOfWords, FALSE);

					// stop at new line?
					m_bStopAtNewLine = bStopAtNewLine==VARIANT_TRUE;
					ATLControls::CButton checkStopAtNewLine(GetDlgItem(IDC_CHK_STOP_AT_NEWLINE_BC));
					checkStopAtNewLine.SetCheck(m_bStopAtNewLine?1:0);
					
					// Stop for other?
					m_strOtherStops = asString(bstrStops);
					m_bStopForOther = (m_strOtherStops.length() > 0) ? true : false;
					ATLControls::CButton checkStopForOther( GetDlgItem(IDC_CHK_OTHER_STOP_BC) );
					checkStopForOther.SetCheck( m_bStopForOther ? 1 : 0 );

					// Set the other Stop characters
					ATLControls::CEdit editStops( GetDlgItem(IDC_EDIT_OTHER_STOP_BC) );
					editStops.EnableWindow( m_bStopForOther ? TRUE : FALSE );
					if (m_bStopForOther)
					{
						SetDlgItemText( IDC_EDIT_OTHER_STOP_BC, m_strOtherStops.c_str() );
					}

					// Get a regex parser
					IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
					ASSERT_RESOURCE_ALLOCATION("ELI13054", ipMiscUtils != NULL );

					IRegularExprParserPtr ipRegExpParser =
						ipMiscUtils->GetNewRegExpParserInstance("ValueBeforeClue");
					ASSERT_RESOURCE_ALLOCATION("ELI04633", ipRegExpParser != NULL);

					// parse the available punctuations, into spaces and non-spaces chars
					string strPunct = asString(bstrPunctuations);
					m_bSpacesAsPunctuations = strPunct.find(" ") != string::npos;
					ATLControls::CButton checkSpaces(GetDlgItem(IDC_CHK_SPACES_BC));
					checkSpaces.SetCheck(m_bSpacesAsPunctuations ? 1 : 0);

					ipRegExpParser->Pattern = "[\\S]+";
					ipRegExpParser->IgnoreCase = m_bCaseSensitive?VARIANT_FALSE:VARIANT_TRUE;
					IIUnknownVectorPtr ipVecObjPairs = 
						ipRegExpParser->Find(_bstr_t(bstrPunctuations), VARIANT_FALSE, 
						VARIANT_FALSE);

					ATLControls::CButton checkOther(GetDlgItem(IDC_CHK_OTHER_PUNC_BC));
					long nSize = ipVecObjPairs->Size();
					m_bSpecifyOtherPunctuations = nSize > 0 ;
					checkOther.SetCheck(m_bSpecifyOtherPunctuations?1:0);
					ATLControls::CEdit editOther(GetDlgItem(IDC_EDIT_OTHER_PUNC_BC));
					editOther.EnableWindow(m_bSpecifyOtherPunctuations?TRUE:FALSE);
					if (m_bSpecifyOtherPunctuations)
					{
						// set chars in the edit box
						for (long n=0; n<nSize; n++)
						{
							IObjectPairPtr ipObjPair = ipVecObjPairs->At(n);
							ITokenPtr ipToken = ipObjPair->Object1;
							if (ipToken)
							{
								_bstr_t _bstrValue = ipToken->Value;
								m_strOtherPunctuations += string(_bstrValue);
							}
						}
						
						SetDlgItemText(IDC_EDIT_OTHER_PUNC_BC, m_strOtherPunctuations.c_str());
					}
				}
				break;
			case kUptoXLines:
				{
					nID = IDC_RADIO_UPTO_XLINES_BC;

					VARIANT_BOOL bIncludeClueLine(VARIANT_TRUE);
					ipValueBeforeClue->GetUptoXLines(&m_nNumOfLines, &bIncludeClueLine);
					SetDlgItemInt(IDC_EDIT_NUM_OF_LINES_BC, m_nNumOfLines);
					m_bIncludeClueLine = bIncludeClueLine==VARIANT_TRUE;
					ATLControls::CButton checkClueLine(GetDlgItem(IDC_CHK_INCLUDE_CLUE_LINE_BC));
					checkClueLine.SetCheck(m_bIncludeClueLine?1:0);
				}
				break;
			case kClueToString:
				{
					nID = IDC_RADIO_CLUE_TO_STRING_BC;
					_bstr_t _bstrSpecifiedString(ipValueBeforeClue->GetClueToString());
					m_strSpecifiedString = string(_bstrSpecifiedString);
					SetDlgItemText(IDC_EDIT_STRING_SPEC_BC, (char*)_bstrSpecifiedString);
					
					// Set associated Regular Expression check box
					bool bIsRegExpr = ipValueBeforeClue->ClueToStringAsRegExpr == VARIANT_TRUE;
					CheckDlgButton( IDC_CHK_STRING_AS_REGEXPR_BC, 
						bIsRegExpr ? BST_CHECKED : BST_UNCHECKED );
				}
				break;
			}

			// which one needs to be selected
			ATLControls::CButton radioRefiningType(GetDlgItem(nID));
			radioRefiningType.SetCheck(1);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04636");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedBtnAddBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_BEFORE_CLUE));

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
		}
		
		CRect rect;
		
		lst.GetClientRect(&rect);
		
		// adjust the column width in case there is a vertical scrollbar now
		lst.SetColumnWidth(0, rect.Width());

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04637");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnListItemChanged(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	updateButtons();
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedBtnModifyBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_BEFORE_CLUE));

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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04638");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedBtnRemoveBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_BEFORE_CLUE));

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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04639");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedChkCaseBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_CASE_BC));
		m_bCaseSensitive = checkBox.GetCheck()==1;

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04640");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedChkClueLineBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_INCLUDE_CLUE_LINE_BC));
		m_bIncludeClueLine = checkBox.GetCheck()==1;

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04641");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedChkOtherPuncBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_OTHER_PUNC_BC));
		m_bSpecifyOtherPunctuations = checkBox.GetCheck()==1;
		ATLControls::CEdit editOther(GetDlgItem(IDC_EDIT_OTHER_PUNC_BC));
		editOther.EnableWindow(m_bSpecifyOtherPunctuations?TRUE:FALSE);

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04642");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedChkSpacesBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_SPACES_BC));
		m_bSpacesAsPunctuations = checkBox.GetCheck()==1;

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04643");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedChkStopAtNewLineBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_STOP_AT_NEWLINE_BC));
		m_bStopAtNewLine = checkBox.GetCheck() == 1;

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04644");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedChkStopForOtherBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve and store setting
		ATLControls::CButton checkBox( GetDlgItem(IDC_CHK_OTHER_STOP_BC) );
		m_bStopForOther = (checkBox.GetCheck() == 1);

		// Enable or disable the edit box
		ATLControls::CEdit editStop( GetDlgItem(IDC_EDIT_OTHER_STOP_BC) );
		editStop.EnableWindow( m_bStopForOther ? TRUE : FALSE );

		// Set Dirty flag
		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05174");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedRadioClueLineBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kClueLine;

		disableCheckAndEditBoxes();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04645");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedRadioClueToStringBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kClueToString;

		selectRefiningType();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04647");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedRadioNoTypeBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kNoRefiningType;
		
		disableCheckAndEditBoxes();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04648");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedRadioUptoXLinesBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kUptoXLines;

		selectRefiningType();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04649");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedRadioUptoXWordsBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kUptoXWords;

		if (!m_bSpacesAsPunctuations && !m_bSpecifyOtherPunctuations)
		{
			ATLControls::CButton checkSpaces(GetDlgItem(IDC_CHK_SPACES_BC));
			checkSpaces.SetCheck(1);
			m_bSpacesAsPunctuations = true;
		}

		selectRefiningType();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04650");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnChangeEditNumOfLinesBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UINT nNumOfLines = GetDlgItemInt(IDC_EDIT_NUM_OF_LINES_BC, NULL, FALSE);
		m_nNumOfLines = nNumOfLines;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04651");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnChangeEditNumOfWordsBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UINT nNumOfWords = GetDlgItemInt(IDC_EDIT_NUM_OF_WORDS_BC, NULL, FALSE);
		m_nNumOfWords = nNumOfWords;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04652");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnChangeEditOtherPuncsBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CComBSTR bstrPuncs;
		GetDlgItemText(IDC_EDIT_OTHER_PUNC_BC, bstrPuncs.m_str);
		
		m_strOtherPunctuations = asString(bstrPuncs);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04653");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnChangeEditOtherStopsBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve string
		CComBSTR bstrStops;
		GetDlgItemText( IDC_EDIT_OTHER_STOP_BC, bstrStops.m_str );
		
		// Store string
		m_strOtherStops = asString(bstrStops);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05175");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnChangeEditSpecStringBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CComBSTR bstrString;
		GetDlgItemText(IDC_EDIT_STRING_SPEC_BC, bstrString.m_str);
		
		m_strSpecifiedString = asString(bstrString);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04655");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnDblclkListValue(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return OnClickedBtnModifyBC(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnKeyDownList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// only delete or del key can delete the selected item
		int nDelete = ::GetKeyState(VK_DELETE);
		if (nDelete < 0)
		{
			return OnClickedBtnRemoveBC(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07310");
	
	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedStopCharInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("- Interpretation : C++ string.\n"
					  "  For example, \"\\t\" will be interpreted as the Tab\n"
					  "  character, \"\\r\\n\" will be considered as one Carriage\n" 
					  "  Return character and one New Line character.");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06854");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CValueBeforeCluePP::OnClickedSeparateCharInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("- Interpretation : C++ string.\n"
					  "  For example, \"\\t\" will be interpreted as the Tab\n"
					  "  character, \"\\r\\n\" will be considered as one Carriage\n" 
					  "  Return character and one New Line character.");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06855");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CValueBeforeCluePP::disableCheckAndEditBoxes()
{
	// associated with up to x words
	ATLControls::CButton checkStopAtNewLine(GetDlgItem(IDC_CHK_STOP_AT_NEWLINE_BC));
	checkStopAtNewLine.EnableWindow(FALSE);
	ATLControls::CButton checkStopForOther( GetDlgItem(IDC_CHK_OTHER_STOP_BC) );
	checkStopForOther.EnableWindow( FALSE );
	ATLControls::CButton checkSpaces(GetDlgItem(IDC_CHK_SPACES_BC));
	checkSpaces.EnableWindow(FALSE);
	ATLControls::CButton checkOther(GetDlgItem(IDC_CHK_OTHER_PUNC_BC));
	checkOther.EnableWindow(FALSE);
	ATLControls::CEdit editNumOfWords(GetDlgItem(IDC_EDIT_NUM_OF_WORDS_BC));
	editNumOfWords.EnableWindow(FALSE);
	ATLControls::CEdit editOther(GetDlgItem(IDC_EDIT_OTHER_PUNC_BC));
	editOther.EnableWindow(FALSE);
	ATLControls::CEdit editStop( GetDlgItem(IDC_EDIT_OTHER_STOP_BC) );
	editStop.EnableWindow( FALSE );

	// associated with up to x lines
	ATLControls::CButton checkClueLine(GetDlgItem(IDC_CHK_INCLUDE_CLUE_LINE_BC));
	checkClueLine.EnableWindow(FALSE);
	ATLControls::CEdit editNumOfLines(GetDlgItem(IDC_EDIT_NUM_OF_LINES_BC));
	editNumOfLines.EnableWindow(FALSE);

	// associated with clue to string
	ATLControls::CEdit editSpecString(GetDlgItem(IDC_EDIT_STRING_SPEC_BC));
	editSpecString.EnableWindow(FALSE);
	ATLControls::CButton checkAsRegExpr(GetDlgItem(IDC_CHK_STRING_AS_REGEXPR_BC));
	checkAsRegExpr.EnableWindow(FALSE);
}
//-------------------------------------------------------------------------------------------------
void CValueBeforeCluePP::selectRefiningType()
{
	// disable all associated check boxes and edit boxes first
	disableCheckAndEditBoxes();

	switch (m_eRefiningType)
	{
	case kUptoXWords:
		{
			// enable its associated check boxes and edit boxes
			ATLControls::CEdit editNumOfWords(GetDlgItem(IDC_EDIT_NUM_OF_WORDS_BC));
			editNumOfWords.EnableWindow(TRUE);
			editNumOfWords.SetSel(0, -1);
			editNumOfWords.SetFocus();
			ATLControls::CButton checkStopAtNewLine(GetDlgItem(IDC_CHK_STOP_AT_NEWLINE_BC));
			checkStopAtNewLine.EnableWindow(TRUE);
			ATLControls::CButton checkStopOther( GetDlgItem(IDC_CHK_OTHER_STOP_BC) );
			checkStopOther.EnableWindow( TRUE );

			ATLControls::CButton checkSpaces(GetDlgItem(IDC_CHK_SPACES_BC));
			checkSpaces.EnableWindow(TRUE);
			ATLControls::CButton checkOther(GetDlgItem(IDC_CHK_OTHER_PUNC_BC));
			checkOther.EnableWindow(TRUE);
			int nCheck = checkOther.GetCheck();
			ATLControls::CEdit editOther(GetDlgItem(IDC_EDIT_OTHER_PUNC_BC));
			editOther.EnableWindow(nCheck==1?TRUE:FALSE);

			nCheck = checkStopOther.GetCheck();
			ATLControls::CEdit editStop( GetDlgItem(IDC_EDIT_OTHER_STOP_BC) );
			editStop.EnableWindow( (nCheck == 1) ? TRUE : FALSE );
		}
		break;
	case kUptoXLines:
		{
			ATLControls::CEdit editNumOfLines(GetDlgItem(IDC_EDIT_NUM_OF_LINES_BC));
			editNumOfLines.EnableWindow(TRUE);
			editNumOfLines.SetSel(0, -1);
			editNumOfLines.SetFocus();
			ATLControls::CButton checkClueLine(GetDlgItem(IDC_CHK_INCLUDE_CLUE_LINE_BC));
			checkClueLine.EnableWindow(TRUE);
		}
		break;
	case kClueToString:
		{
			ATLControls::CEdit editSpecString(GetDlgItem(IDC_EDIT_STRING_SPEC_BC));
			editSpecString.EnableWindow(TRUE);
			editSpecString.SetSel(0, -1);
			editSpecString.SetFocus();
			ATLControls::CButton checkAsRegExpr(GetDlgItem(IDC_CHK_STRING_AS_REGEXPR_BC));
			checkAsRegExpr.EnableWindow(TRUE);
		}
		break;
	}
}
//-------------------------------------------------------------------------------------------------
void CValueBeforeCluePP::updateButtons()
{
	ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_BEFORE_CLUE));
	int nSelCount = lst.GetSelectedCount();
	int nCount = lst.GetItemCount();
	
	if (nCount == 0)
	{
		ATLControls::CButton(GetDlgItem(IDC_BTN_MODIFY_BEFORE_CLUE)).EnableWindow(FALSE);
		ATLControls::CButton(GetDlgItem(IDC_BTN_REMOVE_BEFORE_CLUE)).EnableWindow(FALSE);
	}
	else
	{
		ATLControls::CButton(GetDlgItem(IDC_BTN_MODIFY_BEFORE_CLUE)).EnableWindow(nSelCount == 1 ? true : false);
		ATLControls::CButton(GetDlgItem(IDC_BTN_REMOVE_BEFORE_CLUE)).EnableWindow(nSelCount >= 1 ? true : false);
	}
}
//-------------------------------------------------------------------------------------------------
void CValueBeforeCluePP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07684", "Value Before Clue PP" );
}
//-------------------------------------------------------------------------------------------------
