// EnhanceOCRTaskPP.cpp : Implementation of CEnhanceOCRTaskPP

#include "stdafx.h"
#include "AFFileProcessors.h"
#include "EnhanceOCRTaskPP.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <AFTagManager.h>
#include <DocTagUtils.h>

#include <string>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CEnhanceOCRTaskPP
//--------------------------------------------------------------------------------------------------
CEnhanceOCRTaskPP::CEnhanceOCRTaskPP()
{
	try
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI36629", m_ipMiscUtils != __nullptr );

		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI36630", m_ipAFUtility != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36631");
}
//-------------------------------------------------------------------------------------------------
CEnhanceOCRTaskPP::~CEnhanceOCRTaskPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI36632");
}
//-------------------------------------------------------------------------------------------------
HRESULT CEnhanceOCRTaskPP::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CEnhanceOCRTaskPP::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CEnhanceOCRTaskPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// create tooltip object
		m_infoTip.Create(CWnd::FromHandle(m_hWnd));
		m_infoTip.SetShowDelay(0);

		UCLID_AFFILEPROCESSORSLib::IEnhanceOCRTaskPtr ipRule = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI36633", ipRule);

		// Map controls to member variables
		m_editConfidenceCriteria			= GetDlgItem(IDC_EDIT_CONFIDENCE_CRITERIA);
		m_radioGeneralFilters				= GetDlgItem(IDC_RADIO_GENERAL_FILTERS);
		m_radioHalftoneSpeckledFilters		= GetDlgItem(IDC_RADIO_HALFTONE_FILTERS);
		m_radioAliasedDiffuseFilters		= GetDlgItem(IDC_RADIO_ALIASED_FILTERS);
		m_radioLinesSmudgedFilters			= GetDlgItem(IDC_RADIO_SMUDGED_FILTERS);
		m_radioCustomFilters				= GetDlgItem(IDC_RADIO_CUSTOM_FILTERS);
		m_sliderFilterLevel					= GetDlgItem(IDC_FILTER_LEVEL);
		m_labelFilterLevel1					= GetDlgItem(IDC_STATIC_FILTER_LABEL1);
		m_labelFilterLevel2					= GetDlgItem(IDC_STATIC_FILTER_LABEL2);
		m_labelFilterLevel3					= GetDlgItem(IDC_STATIC_FILTER_LABEL3);
		m_editCustomerFilters				= GetDlgItem(IDC_EDIT_CUSTOM_FILTERS);
		m_btnCustomFiltersDocTag.SubclassDlgItem(
			IDC_BTN_CUSTOM_FILTERS_DOC_TAG, CWnd::FromHandle(m_hWnd));
		m_btnCustomFiltersDocTag.SetIcon(
			::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		m_btnCustomFiltersBrowse			= GetDlgItem(IDC_BTN_CUSTOM_FILTERS_BROWSE);
		m_editPreferredFormatRegex			= GetDlgItem(IDC_EDIT_PREFERRED_FORMAT);
		m_btnPreferredFormatDocTag.SubclassDlgItem(
			IDC_BTN_PREFERRED_FORMAT_DOC_TAG, CWnd::FromHandle(m_hWnd));
		m_btnPreferredFormatDocTag.SetIcon(
			::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		m_btnPreferredFormatBrowse			= GetDlgItem(IDC_BTN_PREFERRED_FORMAT_BROWSE);
		m_editCharsToIgnore					= GetDlgItem(IDC_EDIT_CHARS_TO_IGNORE);
		m_chkOutputFilteredImages			= GetDlgItem(IDC_CHK_OUTPUT_FILTERED_IMAGES);

		if (LicenseManagement::isLicensed(gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS))
		{
			m_chkOutputFilteredImages.ShowWindow(SW_SHOW);
		}

		m_sliderFilterLevel.SetRange(0, 2, TRUE);

		// Load the rule values into the property page.
		m_editConfidenceCriteria.SetWindowText(asString(ipRule->ConfidenceCriteria).c_str());
		switch(ipRule->FilterPackage)
		{
			case kLow:
				m_radioGeneralFilters.SetCheck(BST_CHECKED);
				m_sliderFilterLevel.SetPos(0);
				break;

			case kMedium:
				m_radioGeneralFilters.SetCheck(BST_CHECKED);
				m_sliderFilterLevel.SetPos(1);
				break;

			case kHigh:
				m_radioGeneralFilters.SetCheck(BST_CHECKED);
				m_sliderFilterLevel.SetPos(2);
				break;

			case kHalftoneSpeckled:
				m_radioHalftoneSpeckledFilters.SetCheck(BST_CHECKED);
				break;

			case kAliasedDiffuse:
				m_radioAliasedDiffuseFilters.SetCheck(BST_CHECKED);
				break;

			case kLinesSmudged:
				m_radioLinesSmudgedFilters.SetCheck(BST_CHECKED);
				break;

			case kCustom:
				m_radioCustomFilters.SetCheck(BST_CHECKED);
				break;
		}
		m_editCustomerFilters.SetWindowText(asString(ipRule->CustomFilterPackage).c_str());
		m_editPreferredFormatRegex.SetWindowText(asString(ipRule->PreferredFormatRegex).c_str());
		m_editCharsToIgnore.SetWindowText(asString(ipRule->CharsToIgnore).c_str());
		m_chkOutputFilteredImages.SetCheck(asBSTChecked(ipRule->OutputFilteredImages));
		
		SetDirty(FALSE);

		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36634");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CEnhanceOCRTaskPP::OnFilterRadioButton(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36635");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CEnhanceOCRTaskPP::OnClickedCustomFiltersDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl,
													BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ChooseDocTagForEditBox(
			ITagUtilityPtr(CLSID_AFUtility), m_btnCustomFiltersDocTag, m_editCustomerFilters);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36636");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CEnhanceOCRTaskPP::OnClickedCustomFiltersBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl,
													BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strAllFiles = "Text Files (*.txt)|*.txt"
											"|DAT Files (*.dat)|*.dat"
											"|Encrypted Text Files (*.etf)|*.etf"
											"|All Files (*.*)|*.*||";

		string strFileExtension(s_strAllFiles);
		// bring open file dialog
		CFileDialog fileDlg(TRUE, NULL, "", 
			OFN_HIDEREADONLY | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			strFileExtension.c_str(), NULL);
		
		if (fileDlg.DoModal() == IDOK)
		{			
			m_editCustomerFilters.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36637");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CEnhanceOCRTaskPP::OnClickedPreferredRegexHelp(WORD wNotifyCode, WORD wID, HWND hWndCtl,
												   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("- Dynamically loading a string from a file is supported\n"
					  "- For example, if the String to find edit box contains\n"
					  "  file://D:\\find.dat, the contents of the file will be\n"
					  "  loaded dynamically at run time for finding.\n\n"
					  "- The string should begin with \"file://\" and users can\n"
					  "  browse or use tags to define the file name.\n");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36638");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CEnhanceOCRTaskPP::OnClickedPreferredFormatDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl,
													  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ChooseDocTagForEditBox(
			ITagUtilityPtr(CLSID_AFUtility), m_btnPreferredFormatDocTag, m_editPreferredFormatRegex);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36639");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CEnhanceOCRTaskPP::OnClickedPreferredFormatBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl,
													  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strAllFiles = "Text Files (*.txt)|*.txt"
											"|DAT Files (*.dat)|*.dat"
											"|Encrypted Text Files (*.etf)|*.etf"
											"|All Files (*.*)|*.*||";

		string strFileExtension(s_strAllFiles);
		// bring open file dialog
		CFileDialog fileDlg(TRUE, NULL, "", 
			OFN_HIDEREADONLY | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			strFileExtension.c_str(), NULL);
		
		if (fileDlg.DoModal() == IDOK)
		{
			// get the file name
			CString zFullFileName = CString(
				asString(m_ipMiscUtils->GetFileHeader()).c_str() + fileDlg.GetPathName());
			m_editPreferredFormatRegex.SetWindowText(zFullFileName);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36640");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTaskPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ATLTRACE(_T("CEnhanceOCRTaskPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Obtain interface pointer to the IImageRegionWithLines class
			UCLID_AFFILEPROCESSORSLib::IEnhanceOCRTaskPtr ipRule = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI36641", ipRule != __nullptr);

			ipRule->ConfidenceCriteria = verifyControlValueAsLong(m_editConfidenceCriteria, 1, 100,
				"Confidence criteria must be a value between 1 and 100", 0,
				"Confidence criteria must be a value between 1 and 100");

			ipRule->FilterPackage = (EFilterPackage)getSelectedFilterPackage();

			CComBSTR bstrCustomFilter;
			m_editCustomerFilters.GetWindowText(bstrCustomFilter.m_str);
			if (ipRule->FilterPackage == kCustom)
			{
				if (bstrCustomFilter.Length() == 0)
				{
					throw UCLIDException("ELI36642", 
						"The custom filter package to use has not been specified");
				}
				m_ipAFUtility->ValidateAsExplicitPath("ELI36643", _bstr_t(bstrCustomFilter));
			}
			ipRule->CustomFilterPackage = _bstr_t(bstrCustomFilter);

			CComBSTR bstrPreferredFormatRegex;
			m_editPreferredFormatRegex.GetWindowText(bstrPreferredFormatRegex.m_str);
			AFTagManager::validateDynamicFilePath("ELI36644", asString(bstrPreferredFormatRegex));
			ipRule->PreferredFormatRegex = _bstr_t(bstrPreferredFormatRegex);
			
			CComBSTR bstrCharsToIgnore;
			m_editCharsToIgnore.GetWindowText(bstrCharsToIgnore.m_str);
			ipRule->CharsToIgnore = _bstr_t(bstrCharsToIgnore);

			ipRule->OutputFilteredImages =
				asVariantBool(m_chkOutputFilteredImages.GetCheck() == BST_CHECKED);
		}
		
		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36645");

	// If we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTaskPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI36646", pbValue != __nullptr);

		try
		{
			// check the license
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36647");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
EFilterPackage CEnhanceOCRTaskPP::getSelectedFilterPackage()
{
	if (asCppBool(m_radioGeneralFilters.GetCheck() == BST_CHECKED))
	{
		switch (m_sliderFilterLevel.GetPos())
		{
			case 0:	return kLow;
			case 1:	return kMedium;
			case 2:	return kHigh;
		}
	}
	else if (asCppBool(m_radioHalftoneSpeckledFilters.GetCheck() == BST_CHECKED))
	{
		return kHalftoneSpeckled;
	}
	else if (asCppBool(m_radioAliasedDiffuseFilters.GetCheck() == BST_CHECKED))
	{
		return kAliasedDiffuse;
	}
	else if (asCppBool(m_radioLinesSmudgedFilters.GetCheck() == BST_CHECKED))
	{
		return kLinesSmudged;
	}
	else if (asCppBool(m_radioCustomFilters.GetCheck() == BST_CHECKED))
	{
		return kCustom;
	}
	
	THROW_LOGIC_ERROR_EXCEPTION("ELI36648");
}
//-------------------------------------------------------------------------------------------------
void CEnhanceOCRTaskPP::updateUI()
{
	bool bEnableFilterSlider = getSelectedFilterPackage() <= kHigh;
	m_sliderFilterLevel.EnableWindow(asMFCBool(bEnableFilterSlider));
	m_labelFilterLevel1.EnableWindow(asMFCBool(bEnableFilterSlider));
	m_labelFilterLevel2.EnableWindow(asMFCBool(bEnableFilterSlider));
	m_labelFilterLevel3.EnableWindow(asMFCBool(bEnableFilterSlider));

	bool bEnableCustomFields = (getSelectedFilterPackage() == kCustom);
	m_editCustomerFilters.EnableWindow(asMFCBool(bEnableCustomFields));
	m_btnCustomFiltersDocTag.EnableWindow(asMFCBool(bEnableCustomFields));
	m_btnCustomFiltersBrowse.EnableWindow(asMFCBool(bEnableCustomFields));
}
//-------------------------------------------------------------------------------------------------
void CEnhanceOCRTaskPP::validateLicense()
{
	VALIDATE_LICENSE(gnFILE_ACTION_MANAGER_OBJECTS, "ELI36649", "Enhance OCR Task PP");
}
//-------------------------------------------------------------------------------------------------
