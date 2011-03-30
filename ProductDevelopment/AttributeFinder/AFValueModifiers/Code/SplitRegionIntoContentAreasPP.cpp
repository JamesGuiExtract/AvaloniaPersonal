// SplitRegionIntoContentAreasPP.cpp : Implementation of CSplitRegionIntoContentAreasPP

#include "stdafx.h"
#include "SplitRegionIntoContentAreasPP.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

#include <string>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CSplitRegionIntoContentAreasPP
//--------------------------------------------------------------------------------------------------
CSplitRegionIntoContentAreasPP::CSplitRegionIntoContentAreasPP()
{
}
//-------------------------------------------------------------------------------------------------
CSplitRegionIntoContentAreasPP::~CSplitRegionIntoContentAreasPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI22088");
}
//-------------------------------------------------------------------------------------------------
HRESULT CSplitRegionIntoContentAreasPP::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreasPP::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CSplitRegionIntoContentAreasPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFVALUEMODIFIERSLib::ISplitRegionIntoContentAreasPtr ipRule = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI22089", ipRule);

		// Map controls to member variables
		m_editMinimumWidth					= GetDlgItem(IDC_EDIT_MINIMUM_WIDTH);
		m_editMinimumHeight					= GetDlgItem(IDC_EDIT_MINIMUM_HEIGHT);
		m_chkIncludeGoodOCR					= GetDlgItem(IDC_CHECK_INCLUDE_GOOD_OCR);
		m_chkIncludePoorOCR					= GetDlgItem(IDC_CHECK_INCLUDE_POOR_OCR);
		m_editGoodOCRType					= GetDlgItem(IDC_EDIT_GOOD_OCR_TYPE);
		m_editPoorOCRType					= GetDlgItem(IDC_EDIT_POOR_OCR_TYPE);
		m_editOCRThreshold					= GetDlgItem(IDC_EDIT_OCR_THRESHOLD);
		m_chkUseLines						= GetDlgItem(IDC_CHECK_USE_LINES);
		m_chkReOCRWithHandwriting			= GetDlgItem(IDC_CHECK_RE_OCR);
		m_chkIncludeOCRAsTrueSpatialString	= GetDlgItem(IDC_CHECK_OCR_AS_SUBATTRIBUTE);
		m_editAttributeName					= GetDlgItem(IDC_EDIT_SUBATTRIBUTE_NAME);
		m_editDefaultAttributeText			= GetDlgItem(IDC_EDIT_DEFAULT_TEXT);
		m_editRequiredHorizontalSeparation	= GetDlgItem(IDC_EDIT_REQD_HORZ_SEPARATION);

		// Load the rule values into the property page.
		m_editMinimumWidth.SetWindowText(asString(ipRule->MinimumWidth, 2).c_str());
		m_editMinimumHeight.SetWindowText(asString(ipRule->MinimumHeight, 2).c_str());
		m_chkIncludeGoodOCR.SetCheck(asBSTChecked(ipRule->IncludeGoodOCR));
		m_chkIncludePoorOCR.SetCheck(asBSTChecked(ipRule->IncludePoorOCR));
		m_editGoodOCRType.SetWindowText(asString(ipRule->GoodOCRType).c_str());
		m_editPoorOCRType.SetWindowText(asString(ipRule->PoorOCRType).c_str());
		m_editOCRThreshold.SetWindowText(asString(ipRule->OCRThreshold).c_str());
		m_chkUseLines.SetCheck(asBSTChecked(ipRule->UseLines));
		m_chkReOCRWithHandwriting.SetCheck(asBSTChecked(ipRule->ReOCRWithHandwriting));
		m_chkIncludeOCRAsTrueSpatialString.SetCheck(
			asBSTChecked(ipRule->IncludeOCRAsTrueSpatialString));
		m_editAttributeName.SetWindowText(asString(ipRule->AttributeName).c_str());
		m_editDefaultAttributeText.SetWindowText(asString(ipRule->DefaultAttributeText).c_str());
		m_editRequiredHorizontalSeparation.SetWindowText(
			asString(ipRule->RequiredHorizontalSeparation).c_str());
		
		SetDirty(FALSE);

		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22090");

	return 0;
}

//-------------------------------------------------------------------------------------------------
LRESULT CSplitRegionIntoContentAreasPP::OnBnClickedCheckIncludeGoodOcr(WORD /*wNotifyCode*/, 
	WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI25575")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CSplitRegionIntoContentAreasPP::OnBnClickedCheckIncludePoorOcr(WORD /*wNotifyCode*/, 
	WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI25576")

	return 0;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreasPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ATLTRACE(_T("CSplitRegionIntoContentAreasPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Obtain interface pointer to the IImageRegionWithLines class
			UCLID_AFVALUEMODIFIERSLib::ISplitRegionIntoContentAreasPtr ipRule = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI22092", ipRule != __nullptr);

			ipRule->MinimumWidth = verifyControlValueAsDouble(m_editMinimumWidth, 0, 99999,
				"The minimum width value must be a positive number.", 3.0);
			ipRule->MinimumHeight = verifyControlValueAsDouble(m_editMinimumHeight, 0, 99999,
				"The minimum height value must be a positive number.", 0.5);
			ipRule->IncludeGoodOCR = asVariantBool(m_chkIncludeGoodOCR.GetCheck() == BST_CHECKED);
			ipRule->IncludePoorOCR = asVariantBool(m_chkIncludePoorOCR.GetCheck() == BST_CHECKED);

			if (!asCppBool(ipRule->IncludeGoodOCR) && !asCppBool(ipRule->IncludePoorOCR))
			{
				m_chkIncludeGoodOCR.SetFocus();

				UCLIDException ue("ELI22519", "Results based on either well OCR'd or poorly OCR'd "
					"text must be included!");
				throw ue;
			}

			// Retrieve the type to apply to areas based on well OCR'd text
			CComBSTR bstrGoodOCRType;
			m_editGoodOCRType.GetWindowText(&bstrGoodOCRType);
			ipRule->GoodOCRType = bstrGoodOCRType.m_str;

			// Retrieve the type to apply to areas based on poorly OCR'd text
			CComBSTR bstrPoorOCRType;
			m_editPoorOCRType.GetWindowText(&bstrPoorOCRType);
			ipRule->PoorOCRType = bstrPoorOCRType.m_str;
			
			ipRule->OCRThreshold = verifyControlValueAsLong(m_editOCRThreshold, 0, 100,
				"The required OCR confidence value must be 0 - 100", 60,
				"Please specify the OCR Confidence requirement.");

			ipRule->UseLines = asVariantBool(m_chkUseLines.GetCheck() == BST_CHECKED);

			ipRule->ReOCRWithHandwriting = 
				asVariantBool(m_chkReOCRWithHandwriting.GetCheck() == BST_CHECKED);

			ipRule->IncludeOCRAsTrueSpatialString = 
				asVariantBool(m_chkIncludeOCRAsTrueSpatialString.GetCheck() == BST_CHECKED);

			ipRule->RequiredHorizontalSeparation = verifyControlValueAsLong(
				m_editRequiredHorizontalSeparation, 0, 9999999, 
				"The character width to determine whether content areas be merged must be >= 0", 4,
				"Please specify the character width to determine whether content areas be merged.");

			// Retrieve the specified attribute name
			CComBSTR bstrAttributeName;
			m_editAttributeName.GetWindowText(&bstrAttributeName);
			ipRule->AttributeName = bstrAttributeName.m_str;

			// Retrieve the specified default attribute value
			CComBSTR bstrDefaultAttributeText;
			m_editDefaultAttributeText.GetWindowText(&bstrDefaultAttributeText);

			if (bstrDefaultAttributeText.Length() == 0)
			{
				m_editDefaultAttributeText.SetFocus();

				UCLIDException ue("ELI22224", "No default text specified!");
				throw ue;
			}
			else
			{
				ipRule->DefaultAttributeText = bstrDefaultAttributeText.m_str;
			}
		}
		
		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22091");

	// If we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreasPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22093", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22094");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreasPP::updateUI()
{
	BOOL bIncludeGoodOcr = m_chkIncludeGoodOCR.GetCheck() == BST_CHECKED ? TRUE : FALSE;
	BOOL bIncludePoorOcr = m_chkIncludePoorOCR.GetCheck() == BST_CHECKED ? TRUE : FALSE;

	m_editGoodOCRType.EnableWindow(bIncludeGoodOcr);
	m_editPoorOCRType.EnableWindow(bIncludePoorOcr);
}
//-------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreasPP::validateLicense()
{
	VALIDATE_LICENSE(gnRULESET_EDITOR_UI_OBJECT, "ELI22095", "Split region into content areas PP");
}
//-------------------------------------------------------------------------------------------------