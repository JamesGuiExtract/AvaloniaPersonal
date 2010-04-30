// ValueAfterCluePP.cpp : Implementation of CValueAfterCluePP
#include "stdafx.h"
#include "AFValueFinders.h"
#include "ValueAfterCluePP.h"
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
// CValueAfterCluePP
//-------------------------------------------------------------------------------------------------
CValueAfterCluePP::CValueAfterCluePP() 
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

		m_dwTitleID = IDS_TITLEValueAfterCluePP;
		m_dwHelpFileID = IDS_HELPFILEValueAfterCluePP;
		m_dwDocStringID = IDS_DOCSTRINGValueAfterCluePP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI04585")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterCluePP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CValueAfterCluePP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEFINDERSLib::IValueAfterCluePtr ipValueAfterClue(m_ppUnk[i]);
			if (ipValueAfterClue)
			{
				// get list of clues from the lst box
				IVariantVectorPtr ipClues = ipValueAfterClue->Clues;
				if (ipClues)
				{
					ipClues->Clear();
					ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_AFTER_CLUE));
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
				bool bClueAsRegExpr = IsDlgButtonChecked(IDC_CHK_AS_REGEXPR_AC)==BST_CHECKED;
				ipValueAfterClue->ClueAsRegExpr = bClueAsRegExpr?VARIANT_TRUE:VARIANT_FALSE;

				// get case sensitivity
				ipValueAfterClue->IsCaseSensitive = m_bCaseSensitive?VARIANT_TRUE:VARIANT_FALSE;
				
				// get current refining type
				switch (m_eRefiningType)
				{
				case kNoRefiningType:
					{
						ipValueAfterClue->SetNoRefiningType();
					}
					break;
				case kClueLine:
					{
						ipValueAfterClue->SetClueLineType();
					}
					break;
				case kUptoXWords:
					{
						// must be a positive number
						if (m_nNumOfWords<=0)
						{
							MessageBox("Please specify a positive number of words.",
								"Invalid Configuration", MB_OK | MB_ICONERROR);
							ATLControls::CEdit editNumOfWords(GetDlgItem(IDC_EDIT_NUM_OF_WORDS_AC));
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
								ATLControls::CEdit editOthers(GetDlgItem(IDC_EDIT_OTHER_PUNC_AC));
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
								ATLControls::CEdit editStops( GetDlgItem(IDC_EDIT_OTHER_STOP_AC) );
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

						// Store the settings
						ipValueAfterClue->SetUptoXWords( m_nNumOfWords, _bstr_t( strPuncs.c_str() ), 
							m_bStopAtNewLine ? VARIANT_TRUE : VARIANT_FALSE, 
							_bstr_t( m_strOtherStops.c_str() ) );
					}
					break;
				case kUptoXLines:
					{
						// must be a positive number
						if (m_nNumOfLines<=0)
						{
							MessageBox("Please specify a positive number of lines.",
								"Invalid Configuration", MB_OK | MB_ICONERROR);
							ATLControls::CEdit editNumOfLines(GetDlgItem(IDC_EDIT_NUM_OF_LINES_AC));
							editNumOfLines.SetSel(0, -1);
							editNumOfLines.SetFocus();
							return S_FALSE;
						}

						ipValueAfterClue->SetUptoXLines(m_nNumOfLines, 
									m_bIncludeClueLine?VARIANT_TRUE:VARIANT_FALSE);
					}
					break;
				case kClueToString:
					{
						if (m_strSpecifiedString.empty())
						{
							MessageBox("Please provide non-empty string.",
								"Invalid Configuration", MB_OK | MB_ICONERROR);
							ATLControls::CEdit editString(GetDlgItem(IDC_EDIT_STRING_SPEC_AC));
							editString.SetSel(0, -1);
							editString.SetFocus();
							return S_FALSE;
						}

						ipValueAfterClue->SetClueToString(_bstr_t(m_strSpecifiedString.c_str()));
				
						// get ClueToStringAsRegExpr
						bool bAsRegExpr = IsDlgButtonChecked( IDC_CHK_STRING_AS_REGEXPR_AC ) == BST_CHECKED;
						ipValueAfterClue->ClueToStringAsRegExpr = 
							(bAsRegExpr ? VARIANT_TRUE : VARIANT_FALSE);
					}
					break;
				}
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04563");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterCluePP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CValueAfterCluePP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// which refining type is selected
		UCLID_AFVALUEFINDERSLib::IValueAfterCluePtr ipValueAfterClue(m_ppUnk[0]);
		if (ipValueAfterClue)
		{	
			// create tooltip object
			m_infoTip.Create(CWnd::FromHandle(m_hWnd));
			// set no delay.
			m_infoTip.SetShowDelay(0);

			// populate the clue text lst
			ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_AFTER_CLUE));
			lst.SetExtendedListViewStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
			
			CRect rect;
			
			lst.GetClientRect(&rect);
			
			lst.InsertColumn( 0, "Value", LVCFMT_LEFT, 
			rect.Width(), 0 );
			
			lst.DeleteAllItems();
			
			// lst of values
			IVariantVectorPtr ipVecClues(ipValueAfterClue->Clues);
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
			bool bClueAsRegExpr = ipValueAfterClue->ClueAsRegExpr==VARIANT_TRUE;
			CheckDlgButton(IDC_CHK_AS_REGEXPR_AC, 
							bClueAsRegExpr?BST_CHECKED:BST_UNCHECKED);

			// case-sensitivity
			m_bCaseSensitive = ipValueAfterClue->IsCaseSensitive==VARIANT_TRUE;
			ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_CASE_AC));
			checkBox.SetCheck(m_bCaseSensitive?1:0);

			// get currently selected refining type
			m_eRefiningType = ipValueAfterClue->RefiningType;
			// set radio check and its associated check boxes and edit boxes
			selectRefiningType();
			int nID = 0;
			switch (m_eRefiningType)
			{
			case kNoRefiningType:
				{
					nID = IDC_RADIO_NO_TYPE_AC;
				}
				break;
			case kClueLine:
				{
					nID = IDC_RADIO_CLUE_LINE_AC;
				}
				break;
			case kUptoXWords:
				{
					nID = IDC_RADIO_UPTO_XWORDS_AC;
					CComBSTR bstrPunctuations, bstrStops;
					VARIANT_BOOL bStopAtNewLine(VARIANT_TRUE);
					ipValueAfterClue->GetUptoXWords( &m_nNumOfWords, &bstrPunctuations, 
						&bStopAtNewLine, &bstrStops );

					// set num of words
					SetDlgItemInt(IDC_EDIT_NUM_OF_WORDS_AC, m_nNumOfWords, FALSE);

					// stop at new line?
					m_bStopAtNewLine = bStopAtNewLine==VARIANT_TRUE;
					ATLControls::CButton checkStopAtNewLine(GetDlgItem(IDC_CHK_STOP_AT_NEWLINE_AC));
					checkStopAtNewLine.SetCheck(m_bStopAtNewLine?1:0);
					
					// Stop for other?
					m_strOtherStops = asString(bstrStops);
					m_bStopForOther = (m_strOtherStops.length() > 0) ? true : false;
					ATLControls::CButton checkStopForOther( GetDlgItem(IDC_CHK_OTHER_STOP_AC) );
					checkStopForOther.SetCheck( m_bStopForOther ? 1 : 0 );

					// Set the other Stop characters
					ATLControls::CEdit editStops( GetDlgItem(IDC_EDIT_OTHER_STOP_AC) );
					editStops.EnableWindow( m_bStopForOther ? TRUE : FALSE );
					if (m_bStopForOther)
					{
						SetDlgItemText( IDC_EDIT_OTHER_STOP_AC, m_strOtherStops.c_str() );
					}

					// Get a regex parser
					IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
					ASSERT_RESOURCE_ALLOCATION("ELI13050", ipMiscUtils != NULL );

					IRegularExprParserPtr ipRegExpParser =
						ipMiscUtils->GetNewRegExpParserInstance("ValueAfterClue");
					ASSERT_RESOURCE_ALLOCATION("ELI04586", ipRegExpParser != NULL);

					// parse the available punctuations, into spaces and non-spaces chars
					string strPunct = asString(bstrPunctuations);
					m_bSpacesAsPunctuations = strPunct.find(" ") != string::npos;
					ATLControls::CButton checkSpaces(GetDlgItem(IDC_CHK_SPACES_AC));
					checkSpaces.SetCheck(m_bSpacesAsPunctuations ? 1 : 0);

					ipRegExpParser->Pattern = "[\\S]+";
					ipRegExpParser->IgnoreCase = m_bCaseSensitive?VARIANT_FALSE:VARIANT_TRUE;
					IIUnknownVectorPtr ipVecObjPairs = 
						ipRegExpParser->Find(_bstr_t(bstrPunctuations), VARIANT_FALSE, 
						VARIANT_FALSE);

					ATLControls::CButton checkOther(GetDlgItem(IDC_CHK_OTHER_PUNC_AC));
					long nSize = ipVecObjPairs->Size();
					m_bSpecifyOtherPunctuations = nSize > 0 ;
					checkOther.SetCheck(m_bSpecifyOtherPunctuations?1:0);
					ATLControls::CEdit editOther(GetDlgItem(IDC_EDIT_OTHER_PUNC_AC));
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
						
						SetDlgItemText(IDC_EDIT_OTHER_PUNC_AC, m_strOtherPunctuations.c_str());
					}
				}
				break;
			case kUptoXLines:
				{
					nID = IDC_RADIO_UPTO_XLINES_AC;

					VARIANT_BOOL bIncludeClueLine(VARIANT_TRUE);
					ipValueAfterClue->GetUptoXLines(&m_nNumOfLines, &bIncludeClueLine);
					SetDlgItemInt(IDC_EDIT_NUM_OF_LINES_AC, m_nNumOfLines);
					m_bIncludeClueLine = bIncludeClueLine==VARIANT_TRUE;
					ATLControls::CButton checkClueLine(GetDlgItem(IDC_CHK_INCLUDE_CLUE_LINE_AC));
					checkClueLine.SetCheck(m_bIncludeClueLine?1:0);
				}
				break;
			case kClueToString:
				{
					nID = IDC_RADIO_CLUE_TO_STRING_AC;
					_bstr_t _bstrSpecifiedString(ipValueAfterClue->GetClueToString());
					m_strSpecifiedString = string(_bstrSpecifiedString);
					SetDlgItemText(IDC_EDIT_STRING_SPEC_AC, (char*)_bstrSpecifiedString);
					
					// Set associated Regular Expression check box
					bool bIsRegExpr = ipValueAfterClue->ClueToStringAsRegExpr == VARIANT_TRUE;
					CheckDlgButton( IDC_CHK_STRING_AS_REGEXPR_AC, 
						bIsRegExpr ? BST_CHECKED : BST_UNCHECKED );
				}
				break;
			}

			// which one needs to be selected
			ATLControls::CButton radioRefiningType(GetDlgItem(nID));
			radioRefiningType.SetCheck(1);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04564");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedBtnAddAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_AFTER_CLUE));

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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04597");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedBtnModifyAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_AFTER_CLUE));

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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04598");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedBtnRemoveAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_AFTER_CLUE));

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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04599");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedChkCaseAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_CASE_AC));
		m_bCaseSensitive = checkBox.GetCheck()==1;

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04600");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedChkClueLineAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_INCLUDE_CLUE_LINE_AC));
		m_bIncludeClueLine = checkBox.GetCheck()==1;

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04601");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnListItemChanged(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	updateButtons();
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedChkOtherPuncAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_OTHER_PUNC_AC));
		m_bSpecifyOtherPunctuations = checkBox.GetCheck()==1;
		ATLControls::CEdit editOther(GetDlgItem(IDC_EDIT_OTHER_PUNC_AC));
		editOther.EnableWindow(m_bSpecifyOtherPunctuations?TRUE:FALSE);

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04602");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedChkSpacesAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_SPACES_AC));
		m_bSpacesAsPunctuations = checkBox.GetCheck()==1;

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04603");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedChkStopAtNewLineAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_STOP_AT_NEWLINE_AC));
		m_bStopAtNewLine= checkBox.GetCheck()==1;

		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04604");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedChkStopForOtherAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve checkbox setting and set/reset flag
		ATLControls::CButton checkBox( GetDlgItem(IDC_CHK_OTHER_STOP_AC) );
		m_bStopForOther = (checkBox.GetCheck() == 1);

		// Enable or disable the edit box
		ATLControls::CEdit editStop( GetDlgItem(IDC_EDIT_OTHER_STOP_AC) );
		editStop.EnableWindow( m_bStopForOther ? TRUE : FALSE );

		// Set Dirty flag
		SetDirty( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05116");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedRadioClueLineAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kClueLine;

		disableCheckAndEditBoxes();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04605");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedRadioClueToStringAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kClueToString;

		selectRefiningType();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04607");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedRadioNoTypeAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kNoRefiningType;
		
		disableCheckAndEditBoxes();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04608");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedRadioUptoXLinesAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kUptoXLines;

		selectRefiningType();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04609");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedRadioUptoXWordsAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kUptoXWords;
		if (!m_bSpacesAsPunctuations && !m_bSpecifyOtherPunctuations)
		{
			ATLControls::CButton checkSpaces(GetDlgItem(IDC_CHK_SPACES_AC));
			checkSpaces.SetCheck(1);
			m_bSpacesAsPunctuations = true;
		}

		selectRefiningType();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04610");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnChangeEditNumOfLinesAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UINT nNumOfLines = GetDlgItemInt(IDC_EDIT_NUM_OF_LINES_AC, NULL, FALSE);
		m_nNumOfLines = nNumOfLines;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04611");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnChangeEditNumOfWordsAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UINT nNumOfWords = GetDlgItemInt(IDC_EDIT_NUM_OF_WORDS_AC, NULL, FALSE);
		m_nNumOfWords = nNumOfWords;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04612");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnChangeEditOtherPuncsAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CComBSTR bstrPuncs;
		GetDlgItemText(IDC_EDIT_OTHER_PUNC_AC, bstrPuncs.m_str);
		
		m_strOtherPunctuations = asString(bstrPuncs);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04613");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnChangeEditOtherStopsAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve string
		CComBSTR bstrStops;
		GetDlgItemText( IDC_EDIT_OTHER_STOP_AC, bstrStops.m_str );
		
		// Store string
		m_strOtherStops = asString(bstrStops);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05120");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnChangeEditSpecStringAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CComBSTR bstrString;
		GetDlgItemText(IDC_EDIT_STRING_SPEC_AC, bstrString.m_str);
		
		m_strSpecifiedString = asString(bstrString);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04615");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnDblclkListValue(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return OnClickedBtnModifyAC(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnKeyDownList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// only delete or del key can delete the selected item
		int nDelete = ::GetKeyState(VK_DELETE);
		if (nDelete < 0)
		{
			return OnClickedBtnRemoveAC(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07309");
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedStopCharInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06852");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CValueAfterCluePP::OnClickedSeparateCharInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06853");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CValueAfterCluePP::disableCheckAndEditBoxes()
{
	// associated with up to x words
	ATLControls::CButton checkStopAtNewLine(GetDlgItem(IDC_CHK_STOP_AT_NEWLINE_AC));
	checkStopAtNewLine.EnableWindow(FALSE);
	ATLControls::CButton checkStopForOther( GetDlgItem(IDC_CHK_OTHER_STOP_AC) );
	checkStopForOther.EnableWindow( FALSE );
	ATLControls::CButton checkSpaces(GetDlgItem(IDC_CHK_SPACES_AC));
	checkSpaces.EnableWindow(FALSE);
	ATLControls::CButton checkOther(GetDlgItem(IDC_CHK_OTHER_PUNC_AC));
	checkOther.EnableWindow(FALSE);
	ATLControls::CEdit editNumOfWords(GetDlgItem(IDC_EDIT_NUM_OF_WORDS_AC));
	editNumOfWords.EnableWindow(FALSE);
	ATLControls::CEdit editOther(GetDlgItem(IDC_EDIT_OTHER_PUNC_AC));
	editOther.EnableWindow(FALSE);
	ATLControls::CEdit editStops( GetDlgItem(IDC_EDIT_OTHER_STOP_AC) );
	editStops.EnableWindow( FALSE );

	// associated with up to x lines
	ATLControls::CButton checkClueLine(GetDlgItem(IDC_CHK_INCLUDE_CLUE_LINE_AC));
	checkClueLine.EnableWindow(FALSE);
	ATLControls::CEdit editNumOfLines(GetDlgItem(IDC_EDIT_NUM_OF_LINES_AC));
	editNumOfLines.EnableWindow(FALSE);

	// associated with clue to string
	ATLControls::CEdit editSpecString(GetDlgItem(IDC_EDIT_STRING_SPEC_AC));
	editSpecString.EnableWindow(FALSE);
	ATLControls::CButton checkAsRegExpr(GetDlgItem(IDC_CHK_STRING_AS_REGEXPR_AC));
	checkAsRegExpr.EnableWindow(FALSE);
}
//-------------------------------------------------------------------------------------------------
void CValueAfterCluePP::selectRefiningType()
{
	// disable all associated check boxes and edit boxes first
	disableCheckAndEditBoxes();

	switch (m_eRefiningType)
	{
	case kUptoXWords:
		{
			// enable its associated check boxes and edit boxes
			ATLControls::CEdit editNumOfWords(GetDlgItem(IDC_EDIT_NUM_OF_WORDS_AC));
			editNumOfWords.EnableWindow(TRUE);
			editNumOfWords.SetSel(0, -1);
			editNumOfWords.SetFocus();
			ATLControls::CButton checkStopAtNewLine(GetDlgItem(IDC_CHK_STOP_AT_NEWLINE_AC));
			checkStopAtNewLine.EnableWindow(TRUE);
			ATLControls::CButton checkStopOther( GetDlgItem(IDC_CHK_OTHER_STOP_AC) );
			checkStopOther.EnableWindow( TRUE );

			ATLControls::CButton checkSpaces(GetDlgItem(IDC_CHK_SPACES_AC));
			checkSpaces.EnableWindow(TRUE);
			ATLControls::CButton checkOther(GetDlgItem(IDC_CHK_OTHER_PUNC_AC));
			checkOther.EnableWindow(TRUE);
			int nCheck = checkOther.GetCheck();
			ATLControls::CEdit editOther(GetDlgItem(IDC_EDIT_OTHER_PUNC_AC));
			editOther.EnableWindow(nCheck==1?TRUE:FALSE);

			nCheck = checkStopOther.GetCheck();
			ATLControls::CEdit editStop( GetDlgItem(IDC_EDIT_OTHER_STOP_AC) );
			editStop.EnableWindow( (nCheck == 1) ? TRUE : FALSE );
		}
		break;
	case kUptoXLines:
		{
			ATLControls::CEdit editNumOfLines(GetDlgItem(IDC_EDIT_NUM_OF_LINES_AC));
			editNumOfLines.EnableWindow(TRUE);
			editNumOfLines.SetSel(0, -1);
			editNumOfLines.SetFocus();
			ATLControls::CButton checkClueLine(GetDlgItem(IDC_CHK_INCLUDE_CLUE_LINE_AC));
			checkClueLine.EnableWindow(TRUE);
		}
		break;
	case kClueToString:
		{
			ATLControls::CEdit editSpecString(GetDlgItem(IDC_EDIT_STRING_SPEC_AC));
			editSpecString.EnableWindow(TRUE);
			editSpecString.SetSel(0, -1);
			editSpecString.SetFocus();
			ATLControls::CButton checkAsRegExpr(GetDlgItem(IDC_CHK_STRING_AS_REGEXPR_AC));
			checkAsRegExpr.EnableWindow(TRUE);
		}
		break;
	}
}
//-------------------------------------------------------------------------------------------------
void CValueAfterCluePP::updateButtons()
{
	ATLControls::CListViewCtrl lst(GetDlgItem(IDC_LIST_AFTER_CLUE));
	int nSelCount = lst.GetSelectedCount();
	int nCount = lst.GetItemCount();
	
	if (nCount == 0)
	{
		ATLControls::CButton(GetDlgItem(IDC_BTN_MODIFY_AFTER_CLUE)).EnableWindow(FALSE);
		ATLControls::CButton(GetDlgItem(IDC_BTN_REMOVE_AFTER_CLUE)).EnableWindow(FALSE);
	}
	else
	{
		ATLControls::CButton(GetDlgItem(IDC_BTN_MODIFY_AFTER_CLUE)).EnableWindow(nSelCount == 1 ? true : false);
		ATLControls::CButton(GetDlgItem(IDC_BTN_REMOVE_AFTER_CLUE)).EnableWindow(nSelCount >= 1 ? true : false);
	}
}
//-------------------------------------------------------------------------------------------------
void CValueAfterCluePP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07683", "Value After Clue PP" );
}
//-------------------------------------------------------------------------------------------------
