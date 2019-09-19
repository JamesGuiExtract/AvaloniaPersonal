// RemoveCharactersPP.cpp : Implementation of CRemoveCharactersPP
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "RemoveCharactersPP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CRemoveCharactersPP
//-------------------------------------------------------------------------------------------------
CRemoveCharactersPP::CRemoveCharactersPP() 
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLERemoveCharactersPP;
		m_dwHelpFileID = IDS_HELPFILERemoveCharactersPP;
		m_dwDocStringID = IDS_DOCSTRINGRemoveCharactersPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07718")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharactersPP::Apply()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		_bstr_t _bstrChars("");
		ATLControls::CButton checkBoxSpace(GetDlgItem(IDC_CHK_SPACE));
		if (checkBoxSpace.GetCheck() == 1)
		{
			_bstrChars = " ";
		}
		ATLControls::CButton checkBoxTab(GetDlgItem(IDC_CHK_TAB));
		if (checkBoxTab.GetCheck() == 1)
		{
			_bstrChars += "\t";
		}
		ATLControls::CButton checkBoxNewLine(GetDlgItem(IDC_CHK_RETURN));
		if (checkBoxNewLine.GetCheck() == 1)
		{
			_bstrChars += "\r\n";
		}
		ATLControls::CButton checkBoxOtherChars(GetDlgItem(IDC_CHK_OTHER_CHARS));
		if (checkBoxOtherChars.GetCheck() == 1)
		{
			CComBSTR bstr;
			GetDlgItemText(IDC_EDIT_OTHER_CHARS, bstr.m_str);
			_bstrChars += _bstr_t(bstr);
		}

		// _bstrChars must not be empty
		if (_bstrChars.length()==0)
		{
			MessageBox("Please specify one or more characters to be removed.", "Configuration");
			return S_FALSE;
		}

		bool bRemoveAll = IsDlgButtonChecked(IDC_RADIO_REMOVE_ALL)==BST_CHECKED;
		bool bConsolidate = (!bRemoveAll) && (IsDlgButtonChecked(IDC_CHK_CONSOLIDATE)==BST_CHECKED);
		bool bTrimLeading = (!bRemoveAll) && (IsDlgButtonChecked(IDC_CHK_TRIM_LEADING)==BST_CHECKED);
		bool bTrimTrailing = (!bRemoveAll) && (IsDlgButtonChecked(IDC_CHK_TRIM_TRAILING)==BST_CHECKED);

		if (!bRemoveAll && !bConsolidate && !bTrimLeading && !bTrimTrailing)
		{
			MessageBox("Please select one or more ways of removing characters.", "Configuration");
			return S_FALSE;
		}

		// update the associated object
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEMODIFIERSLib::IRemoveCharactersPtr ipRemoveChars(m_ppUnk[i]);
			ipRemoveChars->Characters = _bstrChars;
			ipRemoveChars->IsCaseSensitive = m_bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE;
			
			ipRemoveChars->RemoveAll = bRemoveAll ? VARIANT_TRUE : VARIANT_FALSE;
			ipRemoveChars->Consolidate = bConsolidate ? VARIANT_TRUE : VARIANT_FALSE;
			ipRemoveChars->TrimLeading = bTrimLeading ? VARIANT_TRUE : VARIANT_FALSE;
			ipRemoveChars->TrimTrailing = bTrimTrailing ? VARIANT_TRUE : VARIANT_FALSE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04347");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharactersPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CRemoveCharactersPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// create tooltip object
		m_infoTip.Create(CWnd::FromHandle(m_hWnd));
		// set no delay.
		m_infoTip.SetShowDelay(0);

		UCLID_AFVALUEMODIFIERSLib::IRemoveCharactersPtr ipRemoveChars(m_ppUnk[0]);
		if (ipRemoveChars)
		{
			// set case sensitivity
			m_bCaseSensitive = ipRemoveChars->IsCaseSensitive==VARIANT_TRUE;
			ATLControls::CButton chkCaseSensitive(GetDlgItem(IDC_CHK_CASE4));
			chkCaseSensitive.SetCheck(m_bCaseSensitive?1:0);
			
			ATLControls::CEdit editOtherChars(GetDlgItem(IDC_EDIT_OTHER_CHARS));
			editOtherChars.EnableWindow(FALSE);
			// find out what are the characters in the string
			_bstr_t _bstrCharactersDefined(ipRemoveChars->Characters);
			if (_bstrCharactersDefined.length()>0)
			{
				string strChars(_bstrCharactersDefined);
				// if there's any space char
				if (strChars.find(" ") != string::npos)
				{
					ATLControls::CButton chkSpace(GetDlgItem(IDC_CHK_SPACE));
					chkSpace.SetCheck(1);
				}

				// if there's any tab char
				if (strChars.find("\t") != string::npos)
				{
					ATLControls::CButton chkTab(GetDlgItem(IDC_CHK_TAB));
					chkTab.SetCheck(1);
				}

				// if there's any new line chars
				if (strChars.find("\n") != string::npos 
					|| (strChars.find("\r") != string::npos))
				{
					ATLControls::CButton chkNewLine(GetDlgItem(IDC_CHK_RETURN));
					chkNewLine.SetCheck(1);
				}

				IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
				ASSERT_RESOURCE_ALLOCATION("ELI13057", ipMiscUtils != __nullptr );

				// look for non-space characters
				IRegularExprParserPtr ipRegExpr = ipMiscUtils->GetNewRegExpParserInstance("RemoveCharacters");
				ASSERT_RESOURCE_ALLOCATION("ELI04359", ipRegExpr != __nullptr);

				ipRegExpr->Pattern = "[\\S]+";
				IIUnknownVectorPtr ipFoundStringInfos(ipRegExpr->Find(_bstrCharactersDefined, 
					VARIANT_FALSE, VARIANT_FALSE, VARIANT_FALSE));
				long nSize = ipFoundStringInfos->Size();
				BOOL bCheckOthers = nSize!=0;
				ATLControls::CButton chkOtherChars(GetDlgItem(IDC_CHK_OTHER_CHARS));
				chkOtherChars.SetCheck(bCheckOthers);
				ATLControls::CEdit editOtherChars(GetDlgItem(IDC_EDIT_OTHER_CHARS));
				editOtherChars.EnableWindow(bCheckOthers);
				if (bCheckOthers)
				{
					// set chars in the edit box
					string strChars("");
					for (long n=0; n<nSize; n++)
					{
						IObjectPairPtr ipObjPair = ipFoundStringInfos->At(n);
						ITokenPtr ipToken = ipObjPair->Object1;
						if (ipToken)
						{
							_bstr_t _bstrValue = ipToken->Value;
							strChars += string(_bstrValue);
						}
					}

					SetDlgItemText(IDC_EDIT_OTHER_CHARS, strChars.c_str());
				}
			}		

			bool bRemoveAll = ipRemoveChars->RemoveAll==VARIANT_TRUE;
			if (bRemoveAll)
			{
				CheckDlgButton(IDC_RADIO_REMOVE_ALL, BST_CHECKED);
			}
			else
			{
				CheckDlgButton(IDC_RADIO_SELECT_FOLLOW, BST_CHECKED);
				bool bConsolidate = ipRemoveChars->Consolidate==VARIANT_TRUE;
				bool bTrimLeading = ipRemoveChars->TrimLeading==VARIANT_TRUE;
				bool bTrimTrailing = ipRemoveChars->TrimTrailing==VARIANT_TRUE;
				if (bConsolidate)
				{
					CheckDlgButton(IDC_CHK_CONSOLIDATE, BST_CHECKED);
				}
				if (bTrimLeading)
				{
					CheckDlgButton(IDC_CHK_TRIM_LEADING, BST_CHECKED);
				}
				if (bTrimTrailing)
				{
					CheckDlgButton(IDC_CHK_TRIM_TRAILING, BST_CHECKED);
				}
				
			}
			// enable/disable check boxes properly
			SetButtonStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04348");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveCharactersPP::OnClickedChkCaseRemoveChar(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_CASE4));
		// check whether the check box is checked
		int nChecked = checkBox.GetCheck();
		m_bCaseSensitive = nChecked==1;

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04349");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveCharactersPP::OnClickedChkOtherChars(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_OTHER_CHARS));
		// check whether the check box is checked
		int nChecked = checkBox.GetCheck();
		BOOL bEnableEdit = nChecked==1;
		// enable/disable edit box associated with the check box
		ATLControls::CEdit editOtherChars(GetDlgItem(IDC_EDIT_OTHER_CHARS));
		editOtherChars.EnableWindow(bEnableEdit);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04350");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveCharactersPP::OnClickedRadioRemoveAll(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		SetButtonStates();

		// clear all the checks
		CheckDlgButton(IDC_CHK_CONSOLIDATE, BST_UNCHECKED);
		CheckDlgButton(IDC_CHK_TRIM_LEADING, BST_UNCHECKED);
		CheckDlgButton(IDC_CHK_TRIM_TRAILING, BST_UNCHECKED);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04355");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveCharactersPP::OnClickedRadioSelectFollowing(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		SetButtonStates();

		// check every all check boxes
		CheckDlgButton(IDC_CHK_CONSOLIDATE, BST_CHECKED);
		CheckDlgButton(IDC_CHK_TRIM_LEADING, BST_CHECKED);
		CheckDlgButton(IDC_CHK_TRIM_TRAILING, BST_CHECKED);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04354");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveCharactersPP::OnClickedConsolidateInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("Replaces consecutive appearances of any character defined\n"
					  "above with the first instance of such appearance in the\n"
					  "sequence.\n"
					  "For example, if Space is selected, and comma (,) is defined\n"
					  "as the other character. If input text is \"Jan 12, 2004\", the\n"
					  "output will be \"Jan 12,2004\".\n"
					  "If you wish to replace consecutive appearances of same\n"
					  "character with such character (for instance, modify\n"
					  " \"Jan 12,   2004\" as \"Jan 12, 2004\"), you need to define\n"
					  "one character per RemoveCharacters value modifier.");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06909");

	return 0;
}

//--------------------------------------------------------------------------------------------------
// Helper functions
//--------------------------------------------------------------------------------------------------
void CRemoveCharactersPP::SetButtonStates()
{
	bool bRemoveAll = IsDlgButtonChecked(IDC_RADIO_REMOVE_ALL)==BST_CHECKED;
	
	// enable/disable check boxes properly
	ATLControls::CButton chkConsolidate(GetDlgItem(IDC_CHK_CONSOLIDATE));
	chkConsolidate.EnableWindow(bRemoveAll?FALSE:TRUE);
	ATLControls::CButton chkTrimLeading(GetDlgItem(IDC_CHK_TRIM_LEADING));
	chkTrimLeading.EnableWindow(bRemoveAll?FALSE:TRUE);
	ATLControls::CButton chkTrimTrailing(GetDlgItem(IDC_CHK_TRIM_TRAILING));
	chkTrimTrailing.EnableWindow(bRemoveAll?FALSE:TRUE);
}
//--------------------------------------------------------------------------------------------------
void CRemoveCharactersPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07692", 
		"RemoveCharacters Modifier PP" );
}
//-------------------------------------------------------------------------------------------------
