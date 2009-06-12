// StringTokenizerModifierPP.cpp : Implementation of CStringTokenizerModifierPP
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "StringTokenizerModifierPP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CStringTokenizerModifierPP
//-------------------------------------------------------------------------------------------------
CStringTokenizerModifierPP::CStringTokenizerModifierPP()
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEStringTokenizerModifierPP;
		m_dwHelpFileID = IDS_HELPFILEStringTokenizerModifierPP;
		m_dwDocStringID = IDS_DOCSTRINGStringTokenizerModifierPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07720")
}
//-------------------------------------------------------------------------------------------------
CStringTokenizerModifierPP::~CStringTokenizerModifierPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16366");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifierPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CStringTokenizerModifierPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEMODIFIERSLib::IStringTokenizerModifierPtr ipStirngTokenizer = m_ppUnk[i];
			if (ipStirngTokenizer)
			{
				// get delimiter 
				if (!storeDelimiter(ipStirngTokenizer))
				{
					return S_FALSE;
				}
				
				// Get result expression
				if (!storeResultExpr(ipStirngTokenizer))
				{
					return S_FALSE;
				}
				
				// Get text in between
				CComBSTR bstrTextInBetween;
				GetDlgItemText(IDC_EDIT_TEXT_IN_BETWEEN, bstrTextInBetween.m_str);
				ipStirngTokenizer->TextInBetween = _bstr_t(bstrTextInBetween);
				
				// if current type is "Any"
				if (!storeNumOfTokens(ipStirngTokenizer))
				{
					return S_FALSE;
				}

			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05334");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifierPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CStringTokenizerModifierPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Define data members
		m_cmbNumOfTokensType = GetDlgItem(IDC_CMB_NUM_OF_TOKENS_TYPE);
		m_editNumOfTokens = GetDlgItem(IDC_EDIT_NUM_OF_TOKENS);
		m_editDelimiter = GetDlgItem( IDC_EDIT_DELIMITER );

		// populate the contents for the combo box
		m_cmbNumOfTokensType.AddString("Any");
		m_cmbNumOfTokensType.AddString("=");
		m_cmbNumOfTokensType.AddString(">");
		m_cmbNumOfTokensType.AddString(">=");

		// Limit the number of allowed delimiter characters (P16 #2191)
		m_editDelimiter.SetLimitText( 1 );

		// init picture
		m_picExprInfo = GetDlgItem(IDC_EXPR_INFO);

		// create tooltip object
		m_infoTip.Create(CWnd::FromHandle(m_hWnd));
		// set no delay.
		m_infoTip.SetShowDelay(0);

		UCLID_AFVALUEMODIFIERSLib::IStringTokenizerModifierPtr ipStringTokenizerModifier(m_ppUnk[0]);
		if (ipStringTokenizerModifier)
		{
			// add proper text to the edit boxes
			string strDelimiter = ipStringTokenizerModifier->Delimiter;
			SetDlgItemText(IDC_EDIT_DELIMITER, strDelimiter.c_str());
			string strResultExpr = ipStringTokenizerModifier->ResultExpression;
			SetDlgItemText(IDC_EDIT_RESULT, strResultExpr.c_str());
			string strTextInBetween = ipStringTokenizerModifier->TextInBetween;
			SetDlgItemText(IDC_EDIT_TEXT_IN_BETWEEN, strTextInBetween.c_str());

			long nNumOfTokenType = (long)ipStringTokenizerModifier->NumberOfTokensType;
			m_cmbNumOfTokensType.SetCurSel(nNumOfTokenType);
			if (nNumOfTokenType == 0)
			{
				// if length type is "Any" number of characters
				// Disable the edit box for entering number of token
				m_editNumOfTokens.EnableWindow(FALSE);
			}
			else
			{
				long nNumOfToken = ipStringTokenizerModifier->NumberOfTokensRequired;
				SetDlgItemInt(IDC_EDIT_NUM_OF_TOKENS, nNumOfToken, FALSE);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05335");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerModifierPP::OnSelchangeComboNumOfTokensType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// if current type is "Any"
		int nNumOfTokenType = m_cmbNumOfTokensType.GetCurSel();
		if (nNumOfTokenType == 0)
		{
			// if length type is "Any" number of characters
			// Disable the edit box for entering number of token
			SetDlgItemText(IDC_EDIT_NUM_OF_TOKENS, "");
			m_editNumOfTokens.EnableWindow(FALSE);
		}
		else
		{
			m_editNumOfTokens.EnableWindow(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05336");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerModifierPP::OnClickedExprInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("%Number shall be used to hold place for a specific token.\n"
					  "Use %1 to hold place for first token and %2 for second, etc..\n"
					  "A range of tokens can be represented by placing a hyphen (-)\n"
					  "sign in between two token place holders.\n"
					  "A slash plus a hyphen (\\-) shall be used to represent the\n"
					  "actual hyphen (-) character. A slash plus a percentage sign (\\%)\n"
					  "will represent the actual percentage sign (%). And \"\\\\\" will be\n"
					  "interpreted as \"\\\".\n\n"
					  "For example, if \"12,34,56,78\" is the input string, comma(,)\n"
					  "as the delimiter, \"%1\" will be \"12\", \"%3\" will be \"56\",\n"
					  "\"%2-%4\" will be \"345678\", \"%4\\-ABC\\%4\" will be \"78-ABC%4\".");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05542");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
bool CStringTokenizerModifierPP::storeDelimiter(
		UCLID_AFVALUEMODIFIERSLib::IStringTokenizerModifierPtr ipST)
{
	try
	{
		CComBSTR bstrDelimiter;
		GetDlgItemText(IDC_EDIT_DELIMITER, bstrDelimiter.m_str);

		ipST->Delimiter = _bstr_t(bstrDelimiter);

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05811");

	ATLControls::CEdit editDelimiter(GetDlgItem(IDC_EDIT_DELIMITER));
	editDelimiter.SetSel(0, -1);
	editDelimiter.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CStringTokenizerModifierPP::storeResultExpr(
		UCLID_AFVALUEMODIFIERSLib::IStringTokenizerModifierPtr ipST)
{
	try
	{
		CComBSTR bstrResultExpr;
		GetDlgItemText(IDC_EDIT_RESULT, bstrResultExpr.m_str);
		ipST->ResultExpression = _bstr_t(bstrResultExpr);
		
		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19315");
	
	
	ATLControls::CEdit editResult(GetDlgItem(IDC_EDIT_RESULT));
	editResult.SetSel(0, -1);
	editResult.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CStringTokenizerModifierPP::storeNumOfTokens(
		UCLID_AFVALUEMODIFIERSLib::IStringTokenizerModifierPtr ipST)
{
	try
	{
		int nNumOfTokenType = m_cmbNumOfTokensType.GetCurSel();
		ipST->NumberOfTokensType = (UCLID_AFVALUEMODIFIERSLib::ENumOfTokensType)nNumOfTokenType;
		long nNumOfToken = -1;
		if (nNumOfTokenType > 0)
		{
			// if length type is not "Any" number of token,
			// number of token must be specified
			// position must be specified
			nNumOfToken = GetDlgItemInt(IDC_EDIT_NUM_OF_TOKENS, NULL, FALSE);

			ipST->NumberOfTokensRequired = nNumOfToken;
		}

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05812");

	m_editNumOfTokens.SetSel(0, -1);
	m_editNumOfTokens.SetFocus();

	return false;
}
//-------------------------------------------------------------------------------------------------
void CStringTokenizerModifierPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07694", 
		"StringTokenizer Modifier PP" );
}
//-------------------------------------------------------------------------------------------------
