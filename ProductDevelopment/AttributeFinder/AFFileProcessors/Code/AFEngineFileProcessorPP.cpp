// AFEngineFileProcessorPP.cpp : Implementation of CAFEngineFileProcessorPP
#include "stdafx.h"
#include "AFFileProcessors.h"
#include "AFFileProcessorsUtils.h"
#include "AFEngineFileProcessorPP.h"
#include "Common.h"

#include <FileDialogEx.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <LoadFileDlgThread.h>

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CAFEngineFileProcessorPP
//-------------------------------------------------------------------------------------------------
CAFEngineFileProcessorPP::CAFEngineFileProcessorPP()
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEAFEngineFileProcessorPP;
		m_dwHelpFileID = IDS_HELPFILEAFEngineFileProcessorPP;
		m_dwDocStringID = IDS_DOCSTRINGAFEngineFileProcessorPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI11527")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessorPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineFileProcessorPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CAFEngineFileProcessorPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// get the output handler object
			UCLID_AFFILEPROCESSORSLib::IAFEngineFileProcessorPtr ipAFEFileProc = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI09018", ipAFEFileProc != NULL);

			if (!storeRuleFileName(ipAFEFileProc))
			{
				m_editRuleFileName.SetSel(0, -1);
				m_editRuleFileName.SetFocus();
				return S_FALSE;
			}

			ipAFEFileProc->ReadUSSFile = m_chkReadUSS.GetCheck()==1 ? VARIANT_TRUE : VARIANT_FALSE;

			if (!storeOCRPages(ipAFEFileProc))
			{
				m_editSpecificPages.SetSel(0, -1);
				m_editSpecificPages.SetFocus();
				return S_FALSE;
			}
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09014")

	// if we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CAFEngineFileProcessorPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_editRuleFileName = GetDlgItem(IDC_EDIT_RULE_FILE);
		m_chkReadUSS = GetDlgItem(IDC_CHK_FROM_USS);
		m_chkCreateUSS = GetDlgItem(IDC_CHK_CREATE_USS);
		m_radioAllPages = GetDlgItem(IDC_RADIO_OCR_ALL);
		m_radioSpecificPages = GetDlgItem(IDC_RADIO_OCR_SPECIFIED);
		m_editSpecificPages = GetDlgItem(IDC_EDIT_PAGES);
		m_btnRuleFileSelectTag.SubclassDlgItem(IDC_BTN_DOCTAGS_AFE, CWnd::FromHandle(m_hWnd));
		m_btnRuleFileSelectTag.SetIcon(::LoadIcon(_Module.m_hInstResource,
			MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

		UCLID_AFFILEPROCESSORSLib::IAFEngineFileProcessorPtr ipAFEFileProc = m_ppUnk[0];
		if (ipAFEFileProc)
		{
			string strRuleFileName = ipAFEFileProc->RuleSetFileName;
			m_editRuleFileName.SetWindowText(strRuleFileName.c_str());

			bool bCheck = ipAFEFileProc->ReadUSSFile == VARIANT_TRUE;
			m_chkReadUSS.SetCheck(bCheck);

			bCheck = ipAFEFileProc->CreateUSSFile == VARIANT_TRUE;
			m_chkCreateUSS.SetCheck(bCheck);

			// if create uss file is checked, update the radio buttons
			m_radioAllPages.SetCheck(1);
			m_radioAllPages.EnableWindow(bCheck);
			m_radioSpecificPages.EnableWindow(bCheck);
			m_editSpecificPages.EnableWindow(FALSE);
			if (bCheck)
			{
				EOCRPagesType eType = (EOCRPagesType)ipAFEFileProc->OCRPagesType;
				if (eType == kOCRCertainPages)
				{
					m_radioAllPages.SetCheck(0);
					m_radioSpecificPages.SetCheck(1);
					m_editSpecificPages.EnableWindow(TRUE);
					string strSpecificPages = ipAFEFileProc->OCRCertainPages;
					m_editSpecificPages.SetWindowText(strSpecificPages.c_str());
				}
			}
		}
	
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09015");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAFEngineFileProcessorPP::OnClickedBtnBrowse(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		// bring open file dialog
		CFileDialogEx fileDlg(TRUE, ".rsd", NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
			 OFN_PATHMUSTEXIST, gstrRSD_FILE_OPEN_FILTER.c_str(), CWnd::FromHandle(m_hWnd));
		
		// Pass the pointer of dialog to create ThreadFileDlg object
		ThreadFileDlg tfd(&fileDlg);

		// If the user clicked on OK, then update the filename in the editbox
		if (tfd.doModal() == IDOK)
		{
			// get the file name
			m_editRuleFileName.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09016");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAFEngineFileProcessorPP::OnClickedCheckCreateUSS(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		BOOL bEnable = m_chkCreateUSS.GetCheck() == 1;
		m_radioAllPages.EnableWindow(bEnable);
		m_radioSpecificPages.EnableWindow(bEnable);
		m_editSpecificPages.EnableWindow(bEnable && m_radioSpecificPages.GetCheck() == 1);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10291");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAFEngineFileProcessorPP::OnClickedRadioAllPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_editSpecificPages.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10289");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAFEngineFileProcessorPP::OnClickedRadioSpecificPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_editSpecificPages.EnableWindow(TRUE);
		m_editSpecificPages.SetFocus();
		m_editSpecificPages.SetSel(0, -1);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10290");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAFEngineFileProcessorPP::OnClickedBtnRulesFileDocTags(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Get the rectangle for the doc tag button
		RECT rect;
		m_btnRuleFileSelectTag.GetWindowRect(&rect);

		// Get the doc tag choice
		string strChoice = CAFFileProcessorsUtils::ChooseDocTag(hWndCtl, rect.right, rect.top);
		if (strChoice != "")
		{
			// Replace the selection
			m_editRuleFileName.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26657");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
bool CAFEngineFileProcessorPP::storeRuleFileName(UCLID_AFFILEPROCESSORSLib::IAFEngineFileProcessorPtr ipAFEFileProc)
{
	try
	{
		CComBSTR bstrFileName;
		GetDlgItemText(IDC_EDIT_RULE_FILE, bstrFileName.m_str);

		ipAFEFileProc->RuleSetFileName = _bstr_t(bstrFileName);

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09017");

	return false;
}
//-------------------------------------------------------------------------------------------------
bool CAFEngineFileProcessorPP::storeOCRPages(UCLID_AFFILEPROCESSORSLib::IAFEngineFileProcessorPtr ipAFEFileProc)
{
	try
	{
		bool bCreateUSS = m_chkCreateUSS.GetCheck()==1;
		ipAFEFileProc->CreateUSSFile = bCreateUSS ? VARIANT_TRUE : VARIANT_FALSE;

		if (bCreateUSS)
		{
			bool bOCRAll = m_radioAllPages.GetCheck() == 1;
			EOCRPagesType eType = bOCRAll ? kOCRAllPages : kOCRCertainPages;

			ipAFEFileProc->OCRPagesType = (UCLID_AFFILEPROCESSORSLib::EOCRPagesType)eType;
			if (eType == kOCRCertainPages)
			{
				CComBSTR bstrPages;
				GetDlgItemText(IDC_EDIT_PAGES, bstrPages.m_str);
				ipAFEFileProc->OCRCertainPages = _bstr_t(bstrPages);
			}
		}

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10296");

	return false;
}
//-------------------------------------------------------------------------------------------------
void CAFEngineFileProcessorPP::validateLicense()
{
	static const unsigned long ENGINEFP_PP_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_SERVER_CORE;

	VALIDATE_LICENSE( ENGINEFP_PP_COMPONENT_ID, "ELI11526", "Attribute Finder Engine FP PP" );
}
//-------------------------------------------------------------------------------------------------
