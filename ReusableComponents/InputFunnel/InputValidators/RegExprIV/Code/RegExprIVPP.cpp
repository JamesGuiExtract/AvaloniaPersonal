// RegExprIVPP.cpp : Implementation of CRegExprIVPP
#include "stdafx.h"
#include "RegExprIV.h"
#include "RegExprIVPP.h"

#include <UCLIDException.h>
#include <comutils.h>

//--------------------------------------------------------------------------------------------------
// CRegExprIVPP
//--------------------------------------------------------------------------------------------------
CRegExprIVPP::CRegExprIVPP() 
{
	m_dwTitleID = IDS_TITLERegExprIVPP;
	m_dwHelpFileID = IDS_HELPFILERegExprIVPP;
	m_dwDocStringID = IDS_DOCSTRINGRegExprIVPP;
}

//--------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprIVPP::Apply()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CRegExprIVPP::Apply\n"));

		// get the pattern string, and verify it is not empty
		char pszPattern[4096] = {0};
		GetDlgItemText(IDC_EDIT_REGEXPR, pszPattern, sizeof(pszPattern));
		if (_strcmpi(pszPattern, "") == 0)
		{
			MessageBox("Please provide non-empty regular expression.", "Configuration");
			ATLControls::CEdit editBox(GetDlgItem(IDC_EDIT_REGEXPR));
			editBox.SetSel(0, -1);
			editBox.SetFocus();
			return S_FALSE;
		}

		// check whether the check box is checked
		ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_REG_EXP_CASE));
		int nChecked = checkBox.GetCheck();

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_REGEXPRIVLib::IRegExprInputValidatorPtr ipRegExprIV = m_ppUnk[i];
			
			if (ipRegExprIV)
			{
				ipRegExprIV->Pattern = get_bstr_t(pszPattern);
				ipRegExprIV->IgnoreCase = (nChecked == BST_CHECKED) ? VARIANT_FALSE : VARIANT_TRUE;
				ipRegExprIV->SetInputType("");
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04869");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
LRESULT CRegExprIVPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_REGEXPRIVLib::IRegExprInputValidatorPtr ipRegExprIV(m_ppUnk[0]);
		if (ipRegExprIV)
		{	
			IInputValidatorPtr ipInputValidator(ipRegExprIV);
			ASSERT_RESOURCE_ALLOCATION("ELI30077", ipInputValidator != __nullptr);

			// pattern string
			_bstr_t _bstrPattern(ipRegExprIV->Pattern);
			SetDlgItemText(IDC_EDIT_REGEXPR, (char*)_bstrPattern);

			// case sensitivity
			bool bCaseSensitive = ipRegExprIV->IgnoreCase==VARIANT_FALSE;
			CheckDlgButton(IDC_CHK_REG_EXP_CASE, asBSTChecked(bCaseSensitive));
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04882");

	return 0;
}
//--------------------------------------------------------------------------------------------------
