// SelectPageRegionPP.cpp : Implementation of CSelectPageRegionPP
#include "stdafx.h"
#include "AFPreProcessors.h"
#include "SelectPageRegionPP.h"

#include <UCLIDException.h>
#include <comutils.h>

//-------------------------------------------------------------------------------------------------
// CSelectPageRegionPP
//-------------------------------------------------------------------------------------------------
CSelectPageRegionPP::CSelectPageRegionPP()
{
	m_dwTitleID = IDS_TITLESelectPageRegionPP;
	m_dwHelpFileID = IDS_HELPFILESelectPageRegionPP;
	m_dwDocStringID = IDS_DOCSTRINGSelectPageRegionPP;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegionPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CSelectPageRegionPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFPREPROCESSORSLib::ISelectPageRegionPtr ipSelectPageRegion(m_ppUnk[i]);
			if (ipSelectPageRegion)
			{
				// save include/exclude
				int nCurrentIndex = m_cmbIncludeExclude.GetCurSel();
				ipSelectPageRegion->IncludeRegionDefined = nCurrentIndex==0 ? VARIANT_TRUE : VARIANT_FALSE;

				// save page selections
				if (!savePageSelections(ipSelectPageRegion))
				{
					return S_FALSE;
				}

				// save restrictions
				if (!saveRestrictions(ipSelectPageRegion))
				{
					return S_FALSE;
				}

				// save OCR items
				if (!saveOCRItems( ipSelectPageRegion ))
				{
					return S_FALSE;
				}
			}
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07996")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CSelectPageRegionPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFPREPROCESSORSLib::ISelectPageRegionPtr ipSelectPageRegion = m_ppUnk[0];
		if (ipSelectPageRegion)
		{
			// create tooltip object
			m_infoTip.Create(CWnd::FromHandle(m_hWnd));
			// set no delay.
			m_infoTip.SetShowDelay(0);

			m_cmbIncludeExclude = GetDlgItem(IDC_CMB_INCLUDE_EXCLUDE);
			m_cmbIncludeExclude.AddString("Include");
			m_cmbIncludeExclude.AddString("Exclude");
			bool bExclude = ipSelectPageRegion->IncludeRegionDefined == VARIANT_FALSE;
			m_cmbIncludeExclude.SetCurSel(bExclude);

			// initialize the controls
			m_radioAllPages = GetDlgItem(IDC_RADIO_ALL_PAGES);

			m_radioSpecificPages = GetDlgItem(IDC_RADIO_SPECIFIC_PAGE);
			m_editSpecificPages = GetDlgItem(IDC_EDIT_SPECIFIC_PAGE);

			m_radioRegExpPages = GetDlgItem(IDC_RADIO_REGEXP_PAGE);
			m_cmbRegExpPages = GetDlgItem(IDC_CMB_REGEXP_PAGES);
			m_cmbRegExpPages.AddString("All");
			m_cmbRegExpPages.AddString("Leading");
			m_cmbRegExpPages.AddString("Trailing");
			m_cmbRegExpPages.SetCurSel(0);

			m_chkRegExp = GetDlgItem(IDC_CHECK_REGEXP);
			m_chkCaseSensitive = GetDlgItem(IDC_CHECK_CASE_SENSITIVE);
			m_editRegExp = GetDlgItem(IDC_EDIT_REGEXP_PAGE);

			m_cmbRegExpPages.EnableWindow(FALSE);
			m_chkRegExp.EnableWindow(FALSE);
			m_chkCaseSensitive.EnableWindow(FALSE);
			m_editRegExp.EnableWindow(FALSE);

			UCLID_AFPREPROCESSORSLib::EPageSelectionType ePageSelectionType = ipSelectPageRegion->GetPageSelectionType();
			if (ePageSelectionType == UCLID_AFPREPROCESSORSLib::kSelectAll)
			{
				m_radioAllPages.SetCheck(1);
				int nTmp;
				OnClickedRadioAllPages(0, 0, 0, nTmp);
			}
			else if (ePageSelectionType == UCLID_AFPREPROCESSORSLib::kSelectSpecified)
			{
				CComBSTR bstrSpecificPages;
				VARIANT_BOOL bTmp;
				ipSelectPageRegion->GetPageSelections(&bTmp, &bstrSpecificPages);
				string strSpecificPages = asString(bstrSpecificPages);
				m_radioSpecificPages.SetCheck(1);
				m_editSpecificPages.SetWindowText(strSpecificPages.c_str());

				int nTmp;
				OnClickedRadioSpecificPages(0, 0, 0, nTmp);
			}
			else if (ePageSelectionType == UCLID_AFPREPROCESSORSLib::kSelectWithRegExp)
			{
				m_radioRegExpPages.SetCheck(1);
				string strPattern = ipSelectPageRegion->Pattern;
				m_editRegExp.SetWindowText(strPattern.c_str());
				VARIANT_BOOL bIsRegExp = ipSelectPageRegion->GetIsRegExp();
				m_chkRegExp.SetCheck(bIsRegExp == VARIANT_TRUE ? 1 : 0);
				VARIANT_BOOL bIsCaseSensitive = ipSelectPageRegion->GetIsCaseSensitive();
				m_chkCaseSensitive.SetCheck(bIsCaseSensitive == VARIANT_TRUE ? 1 : 0);
				ERegExpPageSelectionType eType = (ERegExpPageSelectionType)ipSelectPageRegion->GetRegExpPageSelectionType();
				if (eType == kSelectAllPagesWithRegExp)
				{
					m_cmbRegExpPages.SetCurSel(0);
				}
				else if (eType == kSelectLeadingPagesWithRegExp)
				{
					m_cmbRegExpPages.SetCurSel(1);
				}
				else if (eType == kSelectTrailingPagesWithRegExp)
				{
					m_cmbRegExpPages.SetCurSel(2);
				}
				int nTmp;
				OnClickedRadioRegExpPages(0, 0, 0, nTmp);
			}

			m_chkHorizontalRestriction = GetDlgItem(IDC_CHECK_RESTRICT_HORIZON);
			m_editHorizontalStart = GetDlgItem(IDC_EDIT_START_HORIZON);
			m_editHorizontalEnd = GetDlgItem(IDC_EDIT_END_HORIZON);
			long nStartPercent = -1, nEndPercent = -1;
			ipSelectPageRegion->GetHorizontalRestriction(&nStartPercent, &nEndPercent);
			if (nStartPercent >= 0 && nEndPercent > 0)
			{
				m_chkHorizontalRestriction.SetCheck(1);
				SetDlgItemInt(IDC_EDIT_START_HORIZON, nStartPercent, FALSE);
				SetDlgItemInt(IDC_EDIT_END_HORIZON, nEndPercent, FALSE);
				m_editHorizontalStart.EnableWindow(TRUE);
				m_editHorizontalEnd.EnableWindow(TRUE);
			}
			else
			{
				m_chkHorizontalRestriction.SetCheck(0);
				m_editHorizontalStart.EnableWindow(FALSE);
				m_editHorizontalEnd.EnableWindow(FALSE);
			}

			m_chkVerticalRestriction = GetDlgItem(IDC_CHECK_RESTRICT_VERTICAL);
			m_editVerticalStart = GetDlgItem(IDC_EDIT_START_VERTICAL);
			m_editVerticalEnd = GetDlgItem(IDC_EDIT_END_VERTICAL);
			ipSelectPageRegion->GetVerticalRestriction(&nStartPercent, &nEndPercent);
			if (nStartPercent >= 0 && nEndPercent > 0)
			{
				m_chkVerticalRestriction.SetCheck(1);
				SetDlgItemInt(IDC_EDIT_START_VERTICAL, nStartPercent, FALSE);
				SetDlgItemInt(IDC_EDIT_END_VERTICAL, nEndPercent, FALSE);
				m_editVerticalStart.EnableWindow(TRUE);
				m_editVerticalEnd.EnableWindow(TRUE);
			}
			else
			{
				m_chkVerticalRestriction.SetCheck(0);
				m_editVerticalStart.EnableWindow(FALSE);
				m_editVerticalEnd.EnableWindow(FALSE);
			}

			// Define OCR items
			m_chkOCRRegion = GetDlgItem( IDC_CHECK_OCR );
			m_editRegionRotation = GetDlgItem( IDC_EDIT_ROTATION );

			// Initialize settings for OCR items
			bool bDoOCR = (ipSelectPageRegion->GetOCRSelectedRegion() == VARIANT_TRUE) ? 
				true : false;
			m_chkOCRRegion.SetCheck( (bDoOCR == true) ? 1 : 0 );
			if (bDoOCR)
			{
				// Enable edit box
				m_editRegionRotation.EnableWindow( TRUE );

				// Set value
				long nRotation = ipSelectPageRegion->GetSelectedRegionRotation();
				SetDlgItemInt( IDC_EDIT_ROTATION, nRotation, FALSE );
			}
			else
			{
				// Disable edit box
				m_editRegionRotation.EnableWindow( FALSE );
			}
		}
		
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08028");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSelectPageRegionPP::OnClickedRadioAllPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_editSpecificPages.EnableWindow(FALSE);

		m_cmbRegExpPages.EnableWindow(FALSE);
		m_chkRegExp.EnableWindow(FALSE);
		m_chkCaseSensitive.EnableWindow(FALSE);
		m_editRegExp.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08035");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSelectPageRegionPP::OnClickedRadioSpecificPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_editSpecificPages.EnableWindow(TRUE);
		m_editSpecificPages.SetFocus();
		m_editSpecificPages.SetSel(0, -1);
		
		m_cmbRegExpPages.EnableWindow(FALSE);
		m_chkRegExp.EnableWindow(FALSE);
		m_chkCaseSensitive.EnableWindow(FALSE);
		m_editRegExp.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08036");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSelectPageRegionPP::OnClickedChkRestrictHorizon(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		BOOL bEnable = m_chkHorizontalRestriction.GetCheck() == 1;
		m_editHorizontalStart.EnableWindow(bEnable);
		m_editHorizontalEnd.EnableWindow(bEnable);

		if (bEnable)
		{
			m_editHorizontalStart.SetFocus();
			m_editHorizontalStart.SetSel(0 , -1);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08037");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSelectPageRegionPP::OnClickedChkRestrictVertical(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		BOOL bEnable = m_chkVerticalRestriction.GetCheck() == 1;
		m_editVerticalStart.EnableWindow(bEnable);
		m_editVerticalEnd.EnableWindow(bEnable);

		if (bEnable)
		{
			m_editVerticalStart.SetFocus();
			m_editVerticalEnd.SetSel(0 , -1);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08038");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSelectPageRegionPP::OnClickedSpecificPageInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("Specify one or more pages. Page number must be greater than\n"
					  "or equal to 1. You can specify individual page number, a collection\n"
					  "of individual page numbers, a range of page numbers, or a mixture\n"
					  "of individual page numbers and range(s) of page numbers.\n"
					  "Use an integer followed by a hyphen (eg. \"4-\") to specify a range of\n"
					  "pages that the starting page is the integer, and the ending page is the\n"
					  "last page of the image.\n"
					  "Use a hyphen followed by a positive integer (eg. \"-3\") to specify last X\n"
					  "number of pages.\n"
					  "Any duplicate entries will be only counted once. All page numbers will\n"
					  "be sorted in an ascending fashion.\n\n"
					  "For instance, \"3\", \"1,4,6\", \"2-3\", \"2, 4-7, 9\", \"3-5, 6-8\", \"1,3,5-\", \"-2\"\n"
					  "are valid page numbers; \"6-2\", \"0, 2\", \"0-1\" are invalid.\n"
					  "\"1-6,2-4\" will be counted as page 1,2,3,4,5,6. \"-2\" will be last 2 pages of\n"
					  "original image. \"5,3,2\" will be same as \"2,3,5\"");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08041");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSelectPageRegionPP::OnClickedChkRegExp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		BOOL bEnable = m_chkRegExp.GetCheck() == 1;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09358");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSelectPageRegionPP::OnClickedChkCaseSensitive(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		BOOL bEnable = m_chkCaseSensitive.GetCheck() == 1;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09359");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSelectPageRegionPP::OnClickedRadioRegExpPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		BOOL bEnable = m_radioRegExpPages.GetCheck() == 1;

		m_editSpecificPages.EnableWindow(FALSE);

		m_cmbRegExpPages.EnableWindow(TRUE);
		m_cmbRegExpPages.SetFocus();
		m_chkRegExp.EnableWindow(TRUE);
		m_chkCaseSensitive.EnableWindow(TRUE);
		m_editRegExp.EnableWindow(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19153");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSelectPageRegionPP::OnClickedChkOCRRegion(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		bool bEnable = (m_chkOCRRegion.GetCheck() == 1);
		if (bEnable)
		{
			// Enable edit box
			m_editRegionRotation.EnableWindow( TRUE );
		}
		else
		{
			// Disable edit box
			m_editRegionRotation.EnableWindow( FALSE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12631");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool CSelectPageRegionPP::savePageSelections(UCLID_AFPREPROCESSORSLib::ISelectPageRegionPtr ipSelectPageRegion)
{
	bool bSpecificPages = m_radioSpecificPages.GetCheck() == 1;
	bool bAllPages = m_radioAllPages.GetCheck() == 1;
	bool bRegExpPages = m_radioRegExpPages.GetCheck() == 1;
	try
	{
		CComBSTR bstrSpecificPages;
		if (bSpecificPages)
		{
			m_editSpecificPages.GetWindowText(&bstrSpecificPages);
			ipSelectPageRegion->SelectPages(VARIANT_TRUE, _bstr_t(bstrSpecificPages));
			ipSelectPageRegion->PutPageSelectionType(UCLID_AFPREPROCESSORSLib::kSelectSpecified);
		}
		else if (bAllPages)
		{
			ipSelectPageRegion->PutPageSelectionType(UCLID_AFPREPROCESSORSLib::kSelectAll);
		}
		else if (bRegExpPages)
		{
			ipSelectPageRegion->PutPageSelectionType(UCLID_AFPREPROCESSORSLib::kSelectWithRegExp);
			ipSelectPageRegion->PutIsRegExp(m_chkRegExp.GetCheck() == 1 ? VARIANT_TRUE : VARIANT_FALSE);
			ipSelectPageRegion->PutIsCaseSensitive(m_chkCaseSensitive.GetCheck() == 1 ? VARIANT_TRUE : VARIANT_FALSE);
			CComBSTR bstrPattern;
			m_editRegExp.GetWindowText(&bstrPattern);
			ipSelectPageRegion->Pattern = _bstr_t(bstrPattern);
			if (m_cmbRegExpPages.GetCurSel() == 0)
			{
				ipSelectPageRegion->PutRegExpPageSelectionType(UCLID_AFPREPROCESSORSLib::kSelectAllPagesWithRegExp);
			}
			if (m_cmbRegExpPages.GetCurSel() == 1)
			{
				ipSelectPageRegion->PutRegExpPageSelectionType(UCLID_AFPREPROCESSORSLib::kSelectLeadingPagesWithRegExp);
			}
			if (m_cmbRegExpPages.GetCurSel() == 2)
			{
				ipSelectPageRegion->PutRegExpPageSelectionType(UCLID_AFPREPROCESSORSLib::kSelectTrailingPagesWithRegExp);
			}
		}
		
		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08039");
	
	if (bSpecificPages)
	{
		m_editSpecificPages.SetSel(0, -1);
		m_editSpecificPages.SetFocus();
	}
	if (bRegExpPages)
	{
		m_cmbRegExpPages.SetFocus();
	}
	
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CSelectPageRegionPP::saveRestrictions(UCLID_AFPREPROCESSORSLib::ISelectPageRegionPtr ipSelectPageRegion)
{
	bool bRet = true;
	bool bSetFocusHorizon = true;
	try
	{
		int nStartPercent, nEndPercent;
		try
		{
			if (m_chkHorizontalRestriction.GetCheck() == 1)
			{
				nStartPercent = GetDlgItemInt(IDC_EDIT_START_HORIZON, NULL, FALSE);
				nEndPercent = GetDlgItemInt(IDC_EDIT_END_HORIZON, NULL, FALSE);
				ipSelectPageRegion->SetHorizontalRestriction(nStartPercent, nEndPercent);
			}
			else
			{
				ipSelectPageRegion->SetHorizontalRestriction(-1, -1);
			}
		}
		catch (...)
		{
			bRet = false;
			throw;
		}
		
		try
		{
			if (m_chkVerticalRestriction.GetCheck() == 1)
			{
				nStartPercent = GetDlgItemInt(IDC_EDIT_START_VERTICAL, NULL, FALSE);
				nEndPercent = GetDlgItemInt(IDC_EDIT_END_VERTICAL, NULL, FALSE);
				ipSelectPageRegion->SetVerticalRestriction(nStartPercent, nEndPercent);
			}
			else
			{
				ipSelectPageRegion->SetVerticalRestriction(-1, -1);
			}
		}
		catch (...)
		{
			bRet = false;
			bSetFocusHorizon = false;
			throw;
		}
		
		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08040");
	
	if (bSetFocusHorizon)
	{
		m_editHorizontalStart.SetSel(0, -1);
		m_editHorizontalStart.SetFocus();
	}
	else
	{
		m_editVerticalStart.SetSel(0, -1);
		m_editVerticalStart.SetFocus();
	}
	
	return bRet;
}
//-------------------------------------------------------------------------------------------------
bool CSelectPageRegionPP::saveOCRItems(UCLID_AFPREPROCESSORSLib::ISelectPageRegionPtr ipSelectPageRegion)
{
	bool bRet = true;
	try
	{
		try
		{
			if (m_chkOCRRegion.GetCheck() == 1)
			{
				// Set flag
				ipSelectPageRegion->PutOCRSelectedRegion( VARIANT_TRUE );

				// Set rotation
				long nRotation = GetDlgItemInt( IDC_EDIT_ROTATION, NULL, FALSE );
				ipSelectPageRegion->PutSelectedRegionRotation( nRotation );
			}
			else
			{
				// Clear flag
				ipSelectPageRegion->PutOCRSelectedRegion( VARIANT_FALSE );

				// Clear rotation
				ipSelectPageRegion->PutSelectedRegionRotation( 0 );
			}
		}
		catch (...)
		{
			bRet = false;
			throw;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12632");
	
	return bRet;
}
//-------------------------------------------------------------------------------------------------
