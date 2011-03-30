// ExtractLinePP.cpp : Implementation of CExtractLinePP
#include "stdafx.h"
#include "AFValueFinders.h"
#include "ExtractLinePP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CExtractLinePP
//-------------------------------------------------------------------------------------------------
CExtractLinePP::CExtractLinePP() 
{
	try
	{
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13041", ipMiscUtils != __nullptr );

		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEExtractLinePP;
		m_dwHelpFileID = IDS_HELPFILEExtractLinePP;
		m_dwDocStringID = IDS_DOCSTRINGExtractLinePP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI05435")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLinePP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CExtractLinePP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEFINDERSLib::IExtractLinePtr ipExtractLine = m_ppUnk[i];
						
			bool bExtractLineWithLineBreaks = IsDlgButtonChecked(IDC_RADIO_EXTRACT_LINES)==TRUE;
			bool bExtractLineWithoutLineBreaks = IsDlgButtonChecked(IDC_RADIO_EXTRACT_LINES2)==TRUE;
			ipExtractLine->EachLineAsUniqueValue 
				= (!bExtractLineWithLineBreaks && !bExtractLineWithoutLineBreaks) ?
					VARIANT_TRUE : VARIANT_FALSE;

			if (bExtractLineWithLineBreaks)
			{
				if (!storeLineNumbers1(ipExtractLine))
				{
					return S_FALSE;
				}
			}
			else if (bExtractLineWithoutLineBreaks)
			{
				if (!storeLineNumbers2(ipExtractLine))
				{
					return S_FALSE;
				}
			}
		}
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04561")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLinePP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CExtractLinePP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFVALUEFINDERSLib::IExtractLinePtr ipExtractLine = m_ppUnk[0];
		if (ipExtractLine)
		{
			// init picture
			m_picLineInfo = GetDlgItem(IDC_LINE_INFO);
			
			// create tooltip object
			m_infoTip.Create(CWnd::FromHandle(m_hWnd));
			// set no delay.
			m_infoTip.SetShowDelay(0);
			
			m_editLineNumbers1 = GetDlgItem(IDC_EDIT_LINE_NUMBER);
			m_editLineNumbers2 = GetDlgItem(IDC_EDIT_LINE_NUMBER2);
			// whether extract each line as unique value or not
			bool bEachLineAsUnique = ipExtractLine->EachLineAsUniqueValue == VARIANT_TRUE;
			
			CheckDlgButton(IDC_RADIO_UNIQUE, bEachLineAsUnique?BST_CHECKED:BST_UNCHECKED);
			if (bEachLineAsUnique)
			{
				// check boxes
				CheckDlgButton(IDC_RADIO_EXTRACT_LINES, BST_UNCHECKED);
				CheckDlgButton(IDC_RADIO_EXTRACT_LINES2, BST_UNCHECKED);
				// disable edit boxes for entering line numbers
				m_editLineNumbers1.EnableWindow(FALSE);
				m_editLineNumbers2.EnableWindow(FALSE);
			}
			else
			{
				// which extract line shall be checked
				bool bIncludeLineBreaks = ipExtractLine->IncludeLineBreak==VARIANT_TRUE;
				CheckDlgButton(IDC_RADIO_EXTRACT_LINES, bIncludeLineBreaks?BST_CHECKED:BST_UNCHECKED);
				CheckDlgButton(IDC_RADIO_EXTRACT_LINES2, bIncludeLineBreaks?BST_UNCHECKED:BST_CHECKED);
				m_editLineNumbers1.EnableWindow(bIncludeLineBreaks?TRUE:FALSE);
				m_editLineNumbers2.EnableWindow(bIncludeLineBreaks?FALSE:TRUE);

				// get the line numbers
				string strLineNumbers = ipExtractLine->LineNumbers;
				UINT nID = bIncludeLineBreaks ? IDC_EDIT_LINE_NUMBER : IDC_EDIT_LINE_NUMBER2;
				SetDlgItemText(nID, strLineNumbers.c_str());
			}
		}
		
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04559");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CExtractLinePP::OnClickedRadioExtractLineWithBreaks(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// enable/disable edit boxes
		m_editLineNumbers1.EnableWindow(TRUE);
		m_editLineNumbers1.SetFocus();
		m_editLineNumbers2.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05432");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CExtractLinePP::OnClickedRadioExtractLineWithoutBreaks(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// enable/disable edit boxes
		m_editLineNumbers1.EnableWindow(FALSE);
		m_editLineNumbers2.EnableWindow(TRUE);
		m_editLineNumbers2.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05433");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CExtractLinePP::OnClickedRadioUnique(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// enable/disable edit boxes
		m_editLineNumbers1.EnableWindow(FALSE);
		m_editLineNumbers2.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05434");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CExtractLinePP::OnClickedLineInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("- For option 1 and 2, specify one or more line numbers\n"
					  "  separated by comma (,).\n\n"
					  "- A range of lines can be specified by using a hyphen (-).\n"
					  "  For example, \"2,4,6,7-9\" means line 2, 4, 6 and line 7\n"
					  "  through line 9 need to be extracted. \"3-1\" will \n"
					  "  end up with line 3 first, followed by line 2, then line 1.\n\n"
					  "- Example :\n"
					  "  Input text :\n"
					  "  AAA\n"
					  "  BBB\n"
					  "  CCC\n"
					  "  With option 1 defined as \"3-1\", result will be :\n"
					  "  CCC\n"
					  "  BBB\n"
					  "  AAA\n"
					  "  With option 2 defined as \"3-1\", result will be :\n"
					  "  CCCBBBAAA\n\n"
					  "- Line breaks include new line characters (\\n) and carriage\n"
					  "  returns (\\r).");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05538");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
bool CExtractLinePP::storeLineNumbers1(UCLID_AFVALUEFINDERSLib::IExtractLinePtr ipExtractLine)
{
	try
	{
		CComBSTR bstrLineNumbers;
		// if the edit box has text
		GetDlgItemText(IDC_EDIT_LINE_NUMBER, bstrLineNumbers.m_str);
		ipExtractLine->LineNumbers = _bstr_t(bstrLineNumbers);
		ipExtractLine->IncludeLineBreak = VARIANT_TRUE;
		
		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05832");
	
	m_editLineNumbers1.SetSel(0, -1);
	m_editLineNumbers1.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CExtractLinePP::storeLineNumbers2(UCLID_AFVALUEFINDERSLib::IExtractLinePtr ipExtractLine)
{
	try
	{
		CComBSTR bstrLineNumbers;
		// if the edit box has text
		GetDlgItemText(IDC_EDIT_LINE_NUMBER2, bstrLineNumbers.m_str);
		ipExtractLine->LineNumbers = _bstr_t(bstrLineNumbers);
		ipExtractLine->IncludeLineBreak = VARIANT_FALSE;
		
		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05835");
	
	m_editLineNumbers2.SetSel(0, -1);
	m_editLineNumbers2.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
void CExtractLinePP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07680", 
		"ExtractLine Finder PP" );
}
//-------------------------------------------------------------------------------------------------
