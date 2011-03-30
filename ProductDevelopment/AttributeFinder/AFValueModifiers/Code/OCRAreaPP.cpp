// OCRAreaPP.cpp : Implementation of the COCRAreaPP property page class.

#include "stdafx.h"
#include "AFValueModifiers.h"
#include "OCRAreaPP.h"

#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>
#include <OCRConstants.h>

//-------------------------------------------------------------------------------------------------
// COCRAreaPP
//-------------------------------------------------------------------------------------------------
COCRAreaPP::COCRAreaPP() 
{
	try
	{
		// check licensing
		validateLicense();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI18483");
}
//-------------------------------------------------------------------------------------------------
COCRAreaPP::~COCRAreaPP() 
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18484");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRAreaPP::Apply()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
		
	try
	{
		// check licensing
		validateLicense();

		// get the filter options from the checkboxes
		bool bAlphaFilter = (m_chkAlphaFilter.GetCheck() == BST_CHECKED);
		bool bNumeralFilter = (m_chkDigitsFilter.GetCheck() == BST_CHECKED);
		bool bPeriodFilter = (m_chkPeriodFilter.GetCheck() == BST_CHECKED);
		bool bHyphenFilter = (m_chkHyphenFilter.GetCheck() == BST_CHECKED);
		bool bUnderscoreFilter = (m_chkUnderscoreFilter.GetCheck() == BST_CHECKED);
		bool bCommaFilter = (m_chkCommaFilter.GetCheck() == BST_CHECKED);
		bool bForwardSlashFilter = (m_chkForwardSlashFilter.GetCheck() == BST_CHECKED);
		bool bReturnUnrecognized(m_chkUnrecognizedFilter.GetCheck() == BST_CHECKED);
		bool bCustomFilter = (m_chkCustomFilter.GetCheck() == BST_CHECKED);

		// get the custom filter character set from the edit box
		_bstr_t bstrCustomFilterCharacters;
		m_editCustomFilterCharacters.GetWindowText(bstrCustomFilterCharacters.GetAddress());

		// create the set of filter characters
		EFilterCharacters eFilter(kNoFilter);
		if(bAlphaFilter)
		{
			eFilter = EFilterCharacters(eFilter | kAlphaFilter);	
		}
		if(bNumeralFilter)
		{
			eFilter = EFilterCharacters(eFilter | kNumeralFilter);
		}
		if(bPeriodFilter)
		{
			eFilter = EFilterCharacters(eFilter | kPeriodFilter);
		}
		if(bHyphenFilter)
		{
			eFilter = EFilterCharacters(eFilter | kHyphenFilter);
		}
		if(bUnderscoreFilter)
		{
			eFilter = EFilterCharacters(eFilter | kUnderscoreFilter);
		}
		if(bCommaFilter)
		{
			eFilter = EFilterCharacters(eFilter | kCommaFilter);
		}
		if(bForwardSlashFilter)
		{
			eFilter = EFilterCharacters(eFilter | kForwardSlashFilter);
		}
		if(bCustomFilter)
		{
			// ensure that at least one custom filter character has been specified
			if(bstrCustomFilterCharacters.length() == 0)
			{
				MessageBox("Please specify other filter characters.", "Error", MB_ICONEXCLAMATION);
				return S_FALSE;
			}

			eFilter = EFilterCharacters(eFilter | kCustomFilter);
		}

		// ensure that at least one is filter option was checked
		if(eFilter == kNoFilter)
		{
			MessageBox("At least one filter option besides unrecognized characters must be selected.", 
				"Error", MB_ICONEXCLAMATION);
			return S_FALSE;
		}

		// get the radio button options
		bool bDetectHandwriting(m_radioHandwriting.GetCheck() == BST_CHECKED);
		bool bClearIfNoneFound(m_radioClearIfNoneFound.GetCheck() == BST_CHECKED);

		// set the options of the associated objects accordingly
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEMODIFIERSLib::IOCRAreaPtr ipOCRArea(m_ppUnk[i]);
			ASSERT_RESOURCE_ALLOCATION("ELI18485", ipOCRArea != __nullptr);

			ipOCRArea->SetOptions(eFilter, bstrCustomFilterCharacters, asVariantBool(bDetectHandwriting), 
				asVariantBool(bReturnUnrecognized), asVariantBool(bClearIfNoneFound));
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18486");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRAreaPP::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check parameter
		ASSERT_ARGUMENT("ELI18487", pbValue != __nullptr);

		try
		{
			// check license
			validateLicense();

			// if no exception was thrown, then the license is valid
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18488");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Message Handlers
//-------------------------------------------------------------------------------------------------
LRESULT COCRAreaPP::OnCheckCustomFilterClicked(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// enable the custom filter characters edit box iff the custom filter checkbox is checked
		m_editCustomFilterCharacters.EnableWindow( 
			asMFCBool(m_chkCustomFilter.GetCheck() == BST_CHECKED) );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18522");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT COCRAreaPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// get the OCRArea associated with this property page
		// NOTE: this assumes only one coclass is associated with this property page
		UCLID_AFVALUEMODIFIERSLib::IOCRAreaPtr ipOCRArea(m_ppUnk[0]);
		ASSERT_RESOURCE_ALLOCATION("ELI18489", ipOCRArea != __nullptr);

		// get the filter options dialog items
		m_chkAlphaFilter = GetDlgItem(IDC_OCRAREA_CHECK_ALPHA);
		m_chkDigitsFilter = GetDlgItem(IDC_OCRAREA_CHECK_DIGIT);
		m_chkPeriodFilter = GetDlgItem(IDC_OCRAREA_CHECK_PERIOD);
		m_chkHyphenFilter = GetDlgItem(IDC_OCRAREA_CHECK_HYPHEN);
		m_chkUnderscoreFilter = GetDlgItem(IDC_OCRAREA_CHECK_UNDERSCORE);
		m_chkCommaFilter = GetDlgItem(IDC_OCRAREA_CHECK_COMMA);
		m_chkForwardSlashFilter = GetDlgItem(IDC_OCRAREA_CHECK_FORWARD_SLASH);
		m_chkUnrecognizedFilter = GetDlgItem(IDC_OCRAREA_CHECK_UNRECOGNIZED);
		m_chkCustomFilter = GetDlgItem(IDC_OCRAREA_CHECK_CUSTOM);
		m_editCustomFilterCharacters = GetDlgItem(IDC_OCRAREA_EDIT_CUSTOM_CHARS);

		// get the detect handwriting radio buttons
		m_radioHandwriting = GetDlgItem(IDC_OCRAREA_RADIO_HANDWRITING);
		m_radioPrintedText = GetDlgItem(IDC_OCRAREA_RADIO_PRINTED);

		// get the clear if no text found radio buttons
		m_radioClearIfNoneFound = GetDlgItem(IDC_OCRAREA_RADIO_CLEAR_IF_NONE_FOUND);
		m_radioRetainIfNoneFound = GetDlgItem(IDC_OCRAREA_RADIO_RETAIN_IF_NONE_FOUND);

		// get the SSN Finder's options
		EFilterCharacters eFilter;
		_bstr_t bstrCustomFilterCharacters;
		VARIANT_BOOL vbDetectHandwriting, vbReturnUnrecognized, vbClearIfNoneFound;
		ipOCRArea->GetOptions(&eFilter, bstrCustomFilterCharacters.GetAddress(), 
			&vbDetectHandwriting, &vbReturnUnrecognized, &vbClearIfNoneFound);

		// determine whether the custom filter is enabled
		bool bCustomFilter = (eFilter & kCustomFilter) != 0;

		// set the filter option check boxes
		m_chkAlphaFilter.SetCheck((eFilter & kAlphaFilter) == 0 ? BST_UNCHECKED : BST_CHECKED);
		m_chkDigitsFilter.SetCheck((eFilter & kNumeralFilter) == 0 ? BST_UNCHECKED : BST_CHECKED);
		m_chkPeriodFilter.SetCheck((eFilter & kPeriodFilter) == 0 ? BST_UNCHECKED : BST_CHECKED);
		m_chkHyphenFilter.SetCheck((eFilter & kHyphenFilter) == 0 ? BST_UNCHECKED : BST_CHECKED);
		m_chkUnderscoreFilter.SetCheck((eFilter & kUnderscoreFilter) == 0 ? BST_UNCHECKED : BST_CHECKED);
		m_chkCommaFilter.SetCheck((eFilter & kCommaFilter) == 0 ? BST_UNCHECKED : BST_CHECKED);
		m_chkForwardSlashFilter.SetCheck((eFilter & kForwardSlashFilter) == 0 ? BST_UNCHECKED : BST_CHECKED);
		m_chkCustomFilter.SetCheck(bCustomFilter ? BST_CHECKED : BST_UNCHECKED);

		// set the unrecognized filter checkbox
		m_chkUnrecognizedFilter.SetCheck(vbReturnUnrecognized == VARIANT_TRUE ? BST_CHECKED : BST_UNCHECKED);

		// get the unrecognized filter checkbox caption
		_bstr_t bstrUnrecognizedFilterText;
		m_chkUnrecognizedFilter.GetWindowText(bstrUnrecognizedFilterText.GetAddress());
		string strUnrecognizedFilterText(bstrUnrecognizedFilterText);

		// replace the format field with the unrecognized character
		int iFormatFieldIndex(strUnrecognizedFilterText.find('%'));
		if(iFormatFieldIndex == string::npos)
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI18521");
		}
		strUnrecognizedFilterText[iFormatFieldIndex] = gcUNRECOGNIZED;

		// set the unrecognized filter checkbox caption
		m_chkUnrecognizedFilter.SetWindowText(strUnrecognizedFilterText.c_str());

		// check if the custom filter is enabled
		if(bCustomFilter)
		{
			// set and enable the custom characters edit box
			m_editCustomFilterCharacters.SetWindowText(bstrCustomFilterCharacters);
			m_editCustomFilterCharacters.EnableWindow();
		}
		else
		{
			// clear and disable the custom characters edit box
			m_editCustomFilterCharacters.SetWindowText("");
			m_editCustomFilterCharacters.EnableWindow(FALSE);
		}

		// set the detect handwriting radio buttons
		if(vbDetectHandwriting == VARIANT_TRUE)
		{
			m_radioHandwriting.SetCheck(BST_CHECKED);
		}
		else
		{
			m_radioPrintedText.SetCheck(BST_CHECKED);
		}

		// set the clear if no text found radio buttons
		if(vbClearIfNoneFound == VARIANT_TRUE)
		{
			m_radioClearIfNoneFound.SetCheck(BST_CHECKED);
		}
		else
		{
			m_radioRetainIfNoneFound.SetCheck(BST_CHECKED);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18490");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void COCRAreaPP::validateLicense()
{
	VALIDATE_LICENSE(gnRULESET_EDITOR_UI_OBJECT, "ELI18491", "OCRArea Property Page");
}
//-------------------------------------------------------------------------------------------------

