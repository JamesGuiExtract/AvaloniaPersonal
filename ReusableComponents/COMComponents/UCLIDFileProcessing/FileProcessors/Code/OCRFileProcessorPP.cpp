// OCRFileProcessorPP.cpp : Implementation of COCRFileProcessorPP
#include "stdafx.h"
#include "FileProcessors.h"
#include "OCRFileProcessorPP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// COCRFileProcessorPP
//-------------------------------------------------------------------------------------------------
COCRFileProcessorPP::COCRFileProcessorPP()
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEOCRFileProcessorPP;
		m_dwHelpFileID = IDS_HELPFILEOCRFileProcessorPP;
		m_dwDocStringID = IDS_DOCSTRINGOCRFileProcessorPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI11533")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessorPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP COCRFileProcessorPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("COCRFileProcessorPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_FILEPROCESSORSLib::IOCRFileProcessorPtr ipOCR(m_ppUnk[i]);
			if (ipOCR)
			{
				// save the check box state
				ipOCR->UseCleanedImage = 
					asVariantBool((m_checkUseCleanImage.GetCheck() == BST_CHECKED));

				// save page selections
				if (!savePageSelections(ipOCR))
				{
					return S_FALSE;
				}
			}
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11045")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT COCRFileProcessorPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_FILEPROCESSORSLib::IOCRFileProcessorPtr ipOCR = m_ppUnk[0];
		if (ipOCR)
		{
			// get the dialog items
			m_radioAllPages = GetDlgItem(IDC_RADIO_OCR_ALL);
			m_radioSpecificPages = GetDlgItem(IDC_RADIO_OCR_SPECIFIED);
			m_editSpecificPages = GetDlgItem(IDC_EDIT_PAGE_NUMBERS);
			m_checkUseCleanImage = GetDlgItem(IDC_CHECK_OCR_USE_CLEAN);

			// set the status of the check box
			m_checkUseCleanImage.SetCheck(
				(ipOCR->UseCleanedImage == VARIANT_TRUE) ? BST_CHECKED : BST_UNCHECKED);

			UCLID_FILEPROCESSORSLib::EOCRFPPageRangeType ePageRangeType = ipOCR->OCRPageRangeType;
			switch (ePageRangeType)
			{
			case UCLID_FILEPROCESSORSLib::kOCRAll:
				{
					m_radioAllPages.SetCheck(1);
					int nTmp;
					OnClickedRadioAllPages(0, 0, 0, nTmp);
				}
				break;
			case UCLID_FILEPROCESSORSLib::kOCRSpecifiedPages:
				{
					_bstr_t _bstrSpecificPages = ipOCR->SpecificPages;
					m_radioSpecificPages.SetCheck(1);
					string strSpecificPages = _bstrSpecificPages;
					m_editSpecificPages.SetWindowText(strSpecificPages.c_str());
					
					int nTmp;
					OnClickedRadioSpecificPages(0, 0, 0, nTmp);
				}
				break;
			default:
				break;
			}
		}
		
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10279");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT COCRFileProcessorPP::OnClickedRadioAllPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_editSpecificPages.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10280");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT COCRFileProcessorPP::OnClickedRadioSpecificPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_editSpecificPages.EnableWindow(TRUE);
		m_editSpecificPages.SetFocus();
		m_editSpecificPages.SetSel(0, -1);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10281");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool COCRFileProcessorPP::savePageSelections(UCLID_FILEPROCESSORSLib::IOCRFileProcessorPtr ipOCR)
{
	bool bAllPages = m_radioAllPages.GetCheck() == 1;
	bool bSpecificPages = m_radioSpecificPages.GetCheck() == 1;
	try
	{
		if (bAllPages)
		{
			ipOCR->OCRPageRangeType = (UCLID_FILEPROCESSORSLib::EOCRFPPageRangeType)UCLID_FILEPROCESSORSLib::kOCRAll;
		}
		else if (bSpecificPages)
		{
			CComBSTR bstrSpecificPages;
			m_editSpecificPages.GetWindowText(&bstrSpecificPages);
			ipOCR->OCRPageRangeType = (UCLID_FILEPROCESSORSLib::EOCRFPPageRangeType)UCLID_FILEPROCESSORSLib::kOCRSpecifiedPages;
			ipOCR->SpecificPages = _bstr_t(bstrSpecificPages);
		}
		
		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10282");
	
	if (bSpecificPages)
	{
		m_editSpecificPages.SetSel(0, -1);
		m_editSpecificPages.SetFocus();
	}
	
	return false;
}
//-------------------------------------------------------------------------------------------------
void COCRFileProcessorPP::validateLicense()
{
	static const unsigned long OCRFP_PP_COMPONENT_ID = gnOCR_ON_SERVER_FEATURE;

	VALIDATE_LICENSE( OCRFP_PP_COMPONENT_ID, "ELI11532", "OCR File Processor PP" );
}
//-------------------------------------------------------------------------------------------------
