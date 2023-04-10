// OCRFileProcessorPP.cpp : Implementation of COCRFileProcessorPP
#include "stdafx.h"
#include "FileProcessors.h"
#include "OCRFileProcessorPP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#include "XBrowseForFolder.h"
#include "FileProcessorsUtils.h"
#include <LoadFileDlgThread.h>
#include <DocTagUtils.h>
#include <LicenseUtils.h>

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
			
				// Update the Parallelize property
				IParallelizableTaskPtr ipParallelTask(ipOCR);
				ASSERT_RESOURCE_ALLOCATION("ELI37140", ipParallelTask != __nullptr);

				ipParallelTask->Parallelize = asVariantBool(m_checkParallelize.GetCheck() == BST_CHECKED);

				// save page selections
				if (!savePageSelections(ipOCR))
				{
					return S_FALSE;
				}

				// Save the OCR engine type
				ipOCR->OCREngineType = (UCLID_FILEPROCESSORSLib::EOCREngineType)
					m_comboOCREngine.GetItemData(m_comboOCREngine.GetCurSel());

				if (m_radioOCRParametersFromRuleset.GetCheck() == BST_CHECKED)
				{
					ipOCR->LoadOCRParametersFromRuleset = VARIANT_TRUE;
					_bstr_t bstrRulesetName;
					m_editOCRParametersRuleset.GetWindowText(&(bstrRulesetName.GetBSTR()));

					if (SysStringLen(bstrRulesetName) == 0)
					{
						MessageBox("RuleSet path for OCR parameters is missing.", "No RSD path set",
							MB_OK | MB_ICONERROR);
						m_editOCRParametersRuleset.SetFocus();

						return S_FALSE;
					}

					ipOCR->OCRParametersRulesetName = bstrRulesetName;

				}
				else
				{
					ipOCR->LoadOCRParametersFromRuleset = VARIANT_FALSE;
					if (m_radioNoOCRParameters.GetCheck() == BST_CHECKED)
					{
						ipOCR->LoadOCRParametersFromRuleset = VARIANT_FALSE;
						IHasOCRParametersPtr ipHasParams(ipOCR);
						ipHasParams->OCRParameters = __nullptr;
					}
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
			m_checkParallelize = GetDlgItem(IDC_CHECK_PARALLEL);
			m_radioNoOCRParameters = GetDlgItem(IDC_RADIO_NO_OCR_PARAMS);
			m_radioSpecifiedOCRParameters = GetDlgItem(IDC_RADIO_OCR_PARAMS_HERE);
			m_radioOCRParametersFromRuleset = GetDlgItem(IDC_RADIO_OCR_PARAMS_FROM_RULESET);
			m_editOCRParametersRuleset = GetDlgItem(IDC_OCR_PARAMETERS_RULESET);
			m_btnOCRParametersRulesetSelectTag.SubclassDlgItem(IDC_BTN_OCR_PARAMETERS_RULESET_DOC_TAG,
				CWnd::FromHandle(m_hWnd));
			m_btnOCRParametersRulesetSelectTag.SetIcon(::LoadIcon(_Module.m_hInstResource,
				MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
			m_btnOCRParametersRulesetBrowse = GetDlgItem(IDC_BTN_BROWSE_OCR_PARAMETERS_RULESET);
			m_btnEditOCRParameters = GetDlgItem(IDC_BTN_OCRPARAMETERS);

			// set the status of the check box
			m_checkUseCleanImage.SetCheck(
				(ipOCR->UseCleanedImage == VARIANT_TRUE) ? BST_CHECKED : BST_UNCHECKED);

			// Setup the Parallelize field
			IParallelizableTaskPtr ipParallelTask(ipOCR);
			ASSERT_RESOURCE_ALLOCATION("ELI37139", ipParallelTask != __nullptr);

			m_checkParallelize.EnableWindow(TRUE);
			m_checkParallelize.SetCheck(asBSTChecked(ipParallelTask->Parallelize));

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

		initializeOCREngineCombo(ipOCR->OCREngineType);

		if (ipOCR->LoadOCRParametersFromRuleset)
		{
			m_radioOCRParametersFromRuleset.SetCheck(BST_CHECKED);
			string strRuleset = ipOCR->OCRParametersRulesetName;
			if (!strRuleset.empty())
			{
				m_editOCRParametersRuleset.SetWindowText(strRuleset.c_str());
			}
			m_editOCRParametersRuleset.EnableWindow(TRUE);
			m_btnOCRParametersRulesetBrowse.EnableWindow(TRUE);
			m_btnOCRParametersRulesetSelectTag.EnableWindow(TRUE);
			m_btnEditOCRParameters.EnableWindow(FALSE);
		}
		else
		{
			IHasOCRParametersPtr ipHasParams(ipOCR);
			if (ipHasParams->OCRParameters->Size == 0)
			{
				m_radioNoOCRParameters.SetCheck(BST_CHECKED);
				m_btnEditOCRParameters.EnableWindow(FALSE);
			}
			else
			{
				m_radioSpecifiedOCRParameters.SetCheck(BST_CHECKED);
				m_btnEditOCRParameters.EnableWindow(TRUE);
			}
			m_editOCRParametersRuleset.EnableWindow(FALSE);
			m_btnOCRParametersRulesetBrowse.EnableWindow(FALSE);
			m_btnOCRParametersRulesetSelectTag.EnableWindow(FALSE);
		}

		// OCR Parameters are not supported by the GdPicture OCR engine, e.g.
		enableOcrParameterControls(ipOCR->OCREngineType == kKofaxOcrEngine);
		
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
LRESULT COCRFileProcessorPP::OnBnClickedBtnOcrparameters(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	try
	{
		// Create instance of the configure form using the Prog ID to avoid circular dependency
		IOCRParametersConfigurePtr ipConfigure;
		ipConfigure.CreateInstance("Extract.FileActionManager.Forms.OCRParametersConfigure");
		ASSERT_RESOURCE_ALLOCATION("ELI45863", ipConfigure != __nullptr);

		UCLID_FILEPROCESSORSLib::IOCRFileProcessorPtr ipOCR = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI45864", ipOCR != __nullptr);

		// Get the IHasOCRParameters interface pointer of the current instance
		IHasOCRParametersPtr ipOCRParameters(ipOCR);

		// Configure the parameters
		ipConfigure->ConfigureOCRParameters(ipOCRParameters, VARIANT_FALSE, (long)this->m_hWnd);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45865");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
LRESULT COCRFileProcessorPP::OnBnClickedBtnOcrParametersRulesetDocTag(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ChooseDocTagForEditBox(ITagUtilityPtr(CLSID_FAMTagManager), m_btnOCRParametersRulesetSelectTag,
			m_editOCRParametersRuleset);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45937");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
LRESULT COCRFileProcessorPP::OnBnClickedBtnBrowseOcrParametersRuleset(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		string strFile = chooseFile();	
		if (!strFile.empty())
		{
			m_editOCRParametersRuleset.SetWindowText(strFile.c_str());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45938");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
LRESULT COCRFileProcessorPP::OnBnClickedRadioNoOcrParams(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_editOCRParametersRuleset.EnableWindow(FALSE);
		m_btnOCRParametersRulesetBrowse.EnableWindow(FALSE);
		m_btnOCRParametersRulesetSelectTag.EnableWindow(FALSE);
		m_btnEditOCRParameters.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45939");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
LRESULT COCRFileProcessorPP::OnBnClickedRadioOcrParamsHere(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_editOCRParametersRuleset.EnableWindow(FALSE);
		m_btnOCRParametersRulesetBrowse.EnableWindow(FALSE);
		m_btnOCRParametersRulesetSelectTag.EnableWindow(FALSE);
		m_btnEditOCRParameters.EnableWindow(TRUE);

		// If settings are empty, open edit dialog so that they get set to the defaults
		IHasOCRParametersPtr ipOCRParameters(m_ppUnk[0]);
		if (ipOCRParameters->OCRParameters->Size == 0)
		{
			return OnBnClickedBtnOcrparameters(0, 0, 0, bHandled);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45940");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
LRESULT COCRFileProcessorPP::OnBnClickedRadioOcrParamsFromRuleset(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	try
	{
		m_editOCRParametersRuleset.EnableWindow(TRUE);
		m_btnOCRParametersRulesetBrowse.EnableWindow(TRUE);
		m_btnOCRParametersRulesetSelectTag.EnableWindow(TRUE);
		m_btnEditOCRParameters.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45941");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
LRESULT COCRFileProcessorPP::OnCbnSelchangeOcrEngineCombo(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		EOCREngineType ocrEngine = (EOCREngineType)m_comboOCREngine.GetItemData(m_comboOCREngine.GetCurSel());

		enableOcrParameterControls(ocrEngine == kKofaxOcrEngine);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI54224");

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
const std::string COCRFileProcessorPP::chooseFile()
{
	const static string s_strFiles = "Ruleset definition files (*.etf)|*.etf|All Files (*.*)|*.*||";

	// bring open file dialog
	CFileDialog fileDlg(TRUE, NULL, "", 
		OFN_HIDEREADONLY | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
		s_strFiles.c_str(), CWnd::FromHandle(m_hWnd));
	
	// Pass the pointer of dialog to create ThreadFileDlg object
	ThreadFileDlg tfd(&fileDlg);

	// If cancel button is clicked
	if (tfd.doModal() != IDOK)
	{
		return "";
	}
	
	string strFile = (LPCTSTR)fileDlg.GetPathName();

	return strFile;
}
//-------------------------------------------------------------------------------------------------
void COCRFileProcessorPP::initializeOCREngineCombo(UCLID_FILEPROCESSORSLib::EOCREngineType engineType)
{
	m_comboOCREngine = GetDlgItem(IDC_OCR_ENGINE_COMBO);

	// Setup the choices
	long idx = m_comboOCREngine.AddString("Kofax (Nuance)");
	m_comboOCREngine.SetItemData(idx, kKofaxOcrEngine);

	// Temporarily block use of GdPicture for OCR unless running internally at Extract
	// https://extract.atlassian.net/browse/ISSUE-19121
	if (isInternalToolsLicensed())
	{
		idx = m_comboOCREngine.AddString("GdPicture");
		m_comboOCREngine.SetItemData(idx, kGdPictureOcrEngine);
	}

	// Select the current choice
	for (idx = 0; idx < m_comboOCREngine.GetCount(); idx++)
	{
		if (m_comboOCREngine.GetItemData(idx) == engineType)
		{
			m_comboOCREngine.SetCurSel(idx);
			break;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void COCRFileProcessorPP::enableOcrParameterControls(bool enable)
{
	m_radioNoOCRParameters.EnableWindow(enable);
	m_radioSpecifiedOCRParameters.EnableWindow(enable);
	m_radioOCRParametersFromRuleset.EnableWindow(enable);
	m_btnEditOCRParameters.EnableWindow(enable);
	m_editOCRParametersRuleset.EnableWindow(enable);
	m_btnOCRParametersRulesetBrowse.EnableWindow(enable);
	m_btnOCRParametersRulesetSelectTag.EnableWindow(enable);
}
//-------------------------------------------------------------------------------------------------
