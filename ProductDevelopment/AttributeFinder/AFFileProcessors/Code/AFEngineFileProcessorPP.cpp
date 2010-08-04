// AFEngineFileProcessorPP.cpp : Implementation of CAFEngineFileProcessorPP
#include "stdafx.h"
#include "AFFileProcessors.h"
#include "AFFileProcessorsUtils.h"
#include "AFEngineFileProcessorPP.h"
#include "Common.h"

#include <ComUtils.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <Misc.h>
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

			_bstr_t bstrRulesFileName;
			m_editRuleFileName.GetWindowText(bstrRulesFileName.GetAddress());

			// Ensure the rule file name is long enough (at least 8 characters - c:\a.rsd)
			if (bstrRulesFileName.length() < 8)
			{
				MessageBox("Rules file name must be at least 8 characters.", "Invalid Rules File",
					MB_OK | MB_ICONERROR);
				m_editRuleFileName.SetSel(0, -1);
				m_editRuleFileName.SetFocus();
				return S_FALSE;
			}

			// Set the default OCR to all pages
			EOCRPagesType ocrType = kOCRAllPages;

			// Check if OCRing specified pages, and if so ensure that the pages string is not empty
			if (m_radioSpecificPages.GetCheck() == BST_CHECKED)
			{
				_bstr_t bstrSpecifiedPages;
				m_editSpecificPages.GetWindowText(bstrSpecifiedPages.GetAddress());

				if (bstrSpecifiedPages.length() == 0)
				{
					MessageBox("Cannot leave specified pages blank.", "Blank Page Range",
						MB_OK | MB_ICONERROR);
					m_editSpecificPages.SetSel(0, -1);
					m_editSpecificPages.SetFocus();
					return S_FALSE;
				}
				else
				{
					// Validate the page numbers
					try
					{
						validatePageNumbers(asString(bstrSpecifiedPages));
					}
					catch(UCLIDException& uex)
					{
						uex.display();
						m_editSpecificPages.SetSel(0, -1);
						m_editSpecificPages.SetFocus();
						return S_FALSE;
					}
				}

				// Store the specific pages
				ipAFEFileProc->OCRCertainPages = bstrSpecifiedPages;

				// Set the ocr type to certain pages
				ocrType = kOCRCertainPages;
			}
			else if (m_radioOcrNone.GetCheck() == BST_CHECKED)
			{
				// Set the ocr type to none
				ocrType = kNoOCR;
			}

			// Store the rules file name
			ipAFEFileProc->RuleSetFileName = bstrRulesFileName;

			// Store the OCR type
			ipAFEFileProc->OCRPagesType = (UCLID_AFFILEPROCESSORSLib::EOCRPagesType) ocrType;

			// Store the check box settings
			ipAFEFileProc->ReadUSSFile = asVariantBool(m_chkReadUSS.GetCheck() == BST_CHECKED);
			ipAFEFileProc->CreateUSSFile =
				asVariantBool(m_chkSaveOcrResults.GetCheck() == BST_CHECKED);
			ipAFEFileProc->UseCleanedImage =
				asVariantBool(m_chkUseCleanedImage.GetCheck() == BST_CHECKED);
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
		m_chkSaveOcrResults = GetDlgItem(IDC_CHK_SAVE_RESULTS);
		m_chkUseCleanedImage = GetDlgItem(IDC_CHK_USE_CLEAN_IMAGE);
		m_radioAllPages = GetDlgItem(IDC_RADIO_OCR_ALL);
		m_radioSpecificPages = GetDlgItem(IDC_RADIO_OCR_SPECIFIED);
		m_radioOcrNone = GetDlgItem(IDC_RADIO_OCR_NONE);
		m_editSpecificPages = GetDlgItem(IDC_EDIT_PAGES);
		m_btnRuleFileSelectTag.SubclassDlgItem(IDC_BTN_DOCTAGS_AFE, CWnd::FromHandle(m_hWnd));
		m_btnRuleFileSelectTag.SetIcon(::LoadIcon(_Module.m_hInstResource,
			MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

		UCLID_AFFILEPROCESSORSLib::IAFEngineFileProcessorPtr ipAFEFileProc = m_ppUnk[0];
		if (ipAFEFileProc)
		{
			// Get the rules file name and set the edit box
			string strRuleFileName = asString(ipAFEFileProc->RuleSetFileName);
			m_editRuleFileName.SetWindowText(strRuleFileName.c_str());

			// Set the check box states
			m_chkReadUSS.SetCheck(asBSTChecked(ipAFEFileProc->ReadUSSFile));
			m_chkSaveOcrResults.SetCheck(asBSTChecked(ipAFEFileProc->CreateUSSFile));
			m_chkUseCleanedImage.SetCheck(asBSTChecked(ipAFEFileProc->UseCleanedImage));

			// Get the ocr type, set the radio buttons states and edit controls
			// based on the ocr type setting
			EOCRPagesType eType = (EOCRPagesType)ipAFEFileProc->OCRPagesType;
			int nCheckAll = BST_UNCHECKED;
			int nCheckSpecific = BST_UNCHECKED;
			int nCheckNoOcr = BST_UNCHECKED;
			BOOL bEnableSpecificPages = FALSE;
			switch(eType)
			{
			case kOCRAllPages:
				nCheckAll = BST_CHECKED;
				break;

			case kOCRSpecifiedPages:
				{
					nCheckSpecific = BST_CHECKED;
					bEnableSpecificPages = TRUE;
					string strPages = asString(ipAFEFileProc->OCRCertainPages);
					m_editSpecificPages.SetWindowText(strPages.c_str());
				}
				break;

			case kNoOCR:
				{
					// Clear and disable save OCR results if no OCR [FlexIDSCore #3715]
					m_chkSaveOcrResults.SetCheck(BST_UNCHECKED);
					m_chkSaveOcrResults.EnableWindow(FALSE);
					nCheckNoOcr = BST_CHECKED;
				}
				break;

			default:
				THROW_LOGIC_ERROR_EXCEPTION("ELI28089");
			}

			m_editSpecificPages.EnableWindow(bEnableSpecificPages);
			m_radioAllPages.SetCheck(nCheckAll);
			m_radioSpecificPages.SetCheck(nCheckSpecific);
			m_radioOcrNone.SetCheck(nCheckNoOcr);
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
		CFileDialog fileDlg(TRUE, ".rsd", NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
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
LRESULT CAFEngineFileProcessorPP::OnClickedRadioAllPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_editSpecificPages.EnableWindow(FALSE);

		// Ensure the save OCR results and Use cleaned image check boxes are enabled
		m_chkSaveOcrResults.EnableWindow(TRUE);
		m_chkUseCleanedImage.EnableWindow(TRUE);
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

		// Ensure the save OCR results and Use cleaned image check boxes are enabled
		m_chkSaveOcrResults.EnableWindow(TRUE);
		m_chkUseCleanedImage.EnableWindow(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10290");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAFEngineFileProcessorPP::OnClickedRadioOcrNone(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_editSpecificPages.EnableWindow(FALSE);

		// Clear and disable the Save OCR results checkbox [FlexIDSCore #3715]
		m_chkSaveOcrResults.SetCheck(BST_UNCHECKED);
		m_chkSaveOcrResults.EnableWindow(FALSE);

		// Clear and disable the Use cleaned image checkbox [FlexIDSCore #3745]
		m_chkUseCleanedImage.SetCheck(BST_UNCHECKED);
		m_chkUseCleanedImage.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28090");

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
void CAFEngineFileProcessorPP::validateLicense()
{
	static const unsigned long ENGINEFP_PP_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_SERVER_CORE;

	VALIDATE_LICENSE( ENGINEFP_PP_COMPONENT_ID, "ELI11526", "Attribute Finder Engine FP PP" );
}
//-------------------------------------------------------------------------------------------------
