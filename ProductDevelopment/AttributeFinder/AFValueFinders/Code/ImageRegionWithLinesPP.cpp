// ImageRegionWithLinesPP.cpp : Implementation of CImageRegionWithLinesPP

#include "stdafx.h"
#include "ImageRegionWithLinesPP.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <Misc.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const int gnUNSPECIFIED = -1;
const string gstrSPECIFY_PAGES = "Please specify pages to include!";
const string gstrINVALID_MIN_DIMENSION = "Invalid value.\r\n\r\nMinimum region dimension values "
	"must be a percentage value in the range 0 - 99 or be left blank for unspecified.";
const string gstrINVALID_MAX_DIMENSION = "Invalid value.\r\n\r\nMaximum region dimension values "
	"must be a percentage value in the range 0 - 100 or be left blank for unspecified.";
const long gnMINIMUM_DIMENSION_LOWER_LIMIT = 0;
const long gnMINIMUM_DIMENSION_UPPER_LIMIT = 99;
const long gnMAXIMUM_DIMENSION_LOWER_LIMIT = 0;
const long gnMAXIMUM_DIMENSION_UPPER_LIMIT = 100;

//-------------------------------------------------------------------------------------------------
// CImageRegionWithLinesPP
//-------------------------------------------------------------------------------------------------
CImageRegionWithLinesPP::CImageRegionWithLinesPP()
{
	
}
//-------------------------------------------------------------------------------------------------
CImageRegionWithLinesPP::~CImageRegionWithLinesPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18729");
}
//-------------------------------------------------------------------------------------------------
HRESULT CImageRegionWithLinesPP::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CImageRegionWithLinesPP::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CImageRegionWithLinesPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFVALUEFINDERSLib::IImageRegionWithLinesPtr ipImageRegionWithLines = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI18843", ipImageRegionWithLines);

		IImageLineUtilityPtr ipLineUtil = ipImageRegionWithLines->LineUtil;
		ASSERT_RESOURCE_ALLOCATION("ELI18844", ipLineUtil);
		
		// Assign region controls
		m_cmbRowCountMin				= GetDlgItem(IDC_COMBO_LINES_MIN);
		m_cmbRowCountMax				= GetDlgItem(IDC_COMBO_LINES_MAX);
		m_cmbColumnCountMin				= GetDlgItem(IDC_COMBO_COLUMNS_MIN);
		m_cmbColumnCountMax				= GetDlgItem(IDC_COMBO_COLUMNS_MAX);
		m_editColumnWidthMin			= GetDlgItem(IDC_EDIT_WIDTH_MIN);
		m_editColumnWidthMax			= GetDlgItem(IDC_EDIT_WIDTH_MAX);
		m_editLineSpacingMin			= GetDlgItem(IDC_EDIT_LINE_SPACING_MIN);
		m_editLineSpacingMax			= GetDlgItem(IDC_EDIT_LINE_SPACING_MAX);
		m_editColumnSpacingMax			= GetDlgItem(IDC_EDIT_COLUMN_SPACING_MAX);

		// Assign page selection controls
		m_radioAllPages					= GetDlgItem(IDC_RADIO_ALL_PAGES);
		m_radioFirstPages				= GetDlgItem(IDC_RADIO_FIRST_PAGES);
		m_radioLastPages				= GetDlgItem(IDC_RADIO_LAST_PAGES);
		m_radioSpecifiedPages			= GetDlgItem(IDC_RADIO_SPECIFIED_PAGES);
		m_editFirstPageNums				= GetDlgItem(IDC_EDIT_FIRST_PAGE_NUMS);
		m_editLastPageNums				= GetDlgItem(IDC_EDIT_LAST_PAGE_NUMS);
		m_editSpecifiedPageNums			= GetDlgItem(IDC_EDIT_SPECIFIED_PAGE_NUMS);

		// Assign attribute text control
		m_editAttributeText				= GetDlgItem(IDC_EDIT_ATTRIBUTE_TEXT);
		
		// Assign include lines check box
		m_chkIncludeLines				= GetDlgItem(IDC_CHK_INCLUDE_LINES);

		// Populate combo box text
		CString csLabel;
		m_cmbRowCountMax.AddString("<NONE>");
		for (int i = 2 ; i < 10; i++)
		{
			csLabel.Format("%i rows", i);
			m_cmbRowCountMin.AddString(csLabel);
			m_cmbRowCountMax.AddString(csLabel);
		}

		m_cmbColumnCountMax.AddString("<NONE>");
		m_cmbColumnCountMin.AddString("1 column");
		m_cmbColumnCountMax.AddString("1 column");
		for (int i = 2 ; i < 10; i++)
		{
			csLabel.Format("%i columns", i);
			m_cmbColumnCountMin.AddString(csLabel);
			m_cmbColumnCountMax.AddString(csLabel);
		}

		// Initialize controls
		initializeControls();

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18845");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CImageRegionWithLinesPP::OnBnClickedSelectedPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
														  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// enable/disable/check/uncheck appropriate controls
		onSelectPages(wID);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18939");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLinesPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ATLTRACE(_T("CImageRegionWithLinesPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Obtain interface pointer to the IImageRegionWithLines class
			UCLID_AFVALUEFINDERSLib::IImageRegionWithLinesPtr ipImageRegionWithLines = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI18730", ipImageRegionWithLines != __nullptr);

			IImageLineUtilityPtr ipLineUtil = ipImageRegionWithLines->LineUtil;
			ASSERT_RESOURCE_ALLOCATION("ELI18943", ipLineUtil);

			// Derive row count min from current combo box selection
			ipLineUtil->RowCountMin = m_cmbRowCountMin.GetCurSel() + 2;

			// Derive row count max from current combo box selection
			int nRowCountMax = m_cmbRowCountMax.GetCurSel();
			if (nRowCountMax == 0)
			{
				ipLineUtil->RowCountMax = gnUNSPECIFIED;
			}
			else
			{
				ipLineUtil->RowCountMax = nRowCountMax + 1;
			}

			if (ipLineUtil->RowCountMax != gnUNSPECIFIED &&
				ipLineUtil->RowCountMin > ipLineUtil->RowCountMax)
			{
				m_cmbRowCountMax.SetFocus();

				UCLIDException ue("ELI19213", "Row count maximum value is less than the minimum value!");
				throw ue;
			}

			// Derive column count min from current combo box selection
			ipLineUtil->ColumnCountMin = m_cmbColumnCountMin.GetCurSel() + 1;
			
			// Derive column count max from current combo box selection
			int nColumnCountMax = m_cmbColumnCountMax.GetCurSel();
			if (nColumnCountMax == 0)
			{
				ipLineUtil->ColumnCountMax = gnUNSPECIFIED;
			}
			else
			{
				ipLineUtil->ColumnCountMax = nColumnCountMax;
			}

			if (ipLineUtil->ColumnCountMax != gnUNSPECIFIED &&
				ipLineUtil->ColumnCountMin > ipLineUtil->ColumnCountMax)
			{
				m_cmbColumnCountMax.SetFocus();

				UCLIDException ue("ELI19214", "Column count maximum value is less than the minimum value!");
				throw ue;
			}
		
			// Validate and store image region dimension specifications
			ipLineUtil->ColumnWidthMin = verifyControlValueAsLong(m_editColumnWidthMin,
				gnMINIMUM_DIMENSION_LOWER_LIMIT, gnMINIMUM_DIMENSION_UPPER_LIMIT, 
				gstrINVALID_MIN_DIMENSION, gnUNSPECIFIED);
			ipLineUtil->ColumnWidthMax = verifyControlValueAsLong(m_editColumnWidthMax, 
				gnMAXIMUM_DIMENSION_LOWER_LIMIT, gnMAXIMUM_DIMENSION_UPPER_LIMIT, 
				gstrINVALID_MAX_DIMENSION, gnUNSPECIFIED);

			if (ipLineUtil->ColumnWidthMax != gnUNSPECIFIED &&
				ipLineUtil->ColumnWidthMin > ipLineUtil->ColumnWidthMax)
			{
				m_editColumnWidthMax.SetFocus();

				UCLIDException ue("ELI19215", "Column width maximum value is less than the minimum value!");
				throw ue;
			}

			ipLineUtil->LineSpacingMin = verifyControlValueAsLong(m_editLineSpacingMin,
				gnMINIMUM_DIMENSION_LOWER_LIMIT, gnMINIMUM_DIMENSION_UPPER_LIMIT,
				gstrINVALID_MIN_DIMENSION, gnUNSPECIFIED);
			ipLineUtil->LineSpacingMax = verifyControlValueAsLong(m_editLineSpacingMax,
				gnMAXIMUM_DIMENSION_LOWER_LIMIT, gnMAXIMUM_DIMENSION_UPPER_LIMIT,
				gstrINVALID_MAX_DIMENSION, gnUNSPECIFIED);

			if (ipLineUtil->LineSpacingMax != gnUNSPECIFIED &&
				ipLineUtil->LineSpacingMin > ipLineUtil->LineSpacingMax)
			{
				m_editLineSpacingMax.SetFocus();

				UCLIDException ue("ELI19516", "Line spacing maximum value is less than the minimum value!");
				throw ue;
			}

			ipLineUtil->ColumnSpacingMax = verifyControlValueAsLong(
				m_editColumnSpacingMax, 0, 100, gstrINVALID_MAX_DIMENSION, gnUNSPECIFIED);

			// Store page selection settings
			if (m_radioAllPages.GetCheck() == BST_CHECKED)
			{
				ipImageRegionWithLines->PageSelectionMode = UCLID_AFVALUEFINDERSLib::kAllPages;
			}
			else if (m_radioFirstPages.GetCheck() == BST_CHECKED)
			{
				ipImageRegionWithLines->PageSelectionMode = UCLID_AFVALUEFINDERSLib::kFirstPages;
			}
			else if (m_radioLastPages.GetCheck() == BST_CHECKED)
			{
				ipImageRegionWithLines->PageSelectionMode = UCLID_AFVALUEFINDERSLib::kLastPages;
			}
			else if (m_radioSpecifiedPages.GetCheck() == BST_CHECKED)
			{
				ipImageRegionWithLines->PageSelectionMode = UCLID_AFVALUEFINDERSLib::kSpecifiedPages;
			}

			// If FirstPages or LastPages is selected, ensure a page value has been provided
			ipImageRegionWithLines->NumFirstPages = verifyControlValueAsLong(m_editFirstPageNums, 0,
				m_radioFirstPages.GetCheck() == BST_CHECKED ? gstrSPECIFY_PAGES : "");

			ipImageRegionWithLines->NumLastPages = verifyControlValueAsLong(m_editLastPageNums, 0,
				m_radioLastPages.GetCheck() == BST_CHECKED ? gstrSPECIFY_PAGES : "");

			// Validate the specified pages value
			CComBSTR bstrPages;
			m_editSpecifiedPageNums.GetWindowText(&bstrPages);
			try
			{
				validatePageNumbers(asString(bstrPages));
				ipImageRegionWithLines->SpecifiedPages = bstrPages.m_str;
			}
			catch (UCLIDException &ue)
			{
				// If kSpecifiedPages is being used and we've failed validation, throw an execption
				if (ipImageRegionWithLines->PageSelectionMode == UCLID_AFVALUEFINDERSLib::kSpecifiedPages)
				{
					m_editSpecifiedPageNums.SetFocus();
					throw ue;
				}
				
				// If kSpecifiedPages is not being used, don't worry about a bad validation in this case;
				// Just don't save the new value
			}

			// Validate attribute text has been provided
			CComBSTR bstrValue;
			m_editAttributeText.GetWindowText(&bstrValue);
			if (bstrValue.Length() == 0)
			{
				m_editAttributeText.SetFocus();

				UCLIDException ue("ELI18945", "No attribute text specified!");
				throw ue;
			}
			else
			{
				ipImageRegionWithLines->AttributeText = bstrValue.m_str;
			}

			ipImageRegionWithLines->IncludeLines = asVariantBool(m_chkIncludeLines.GetCheck() == BST_CHECKED);
		}

		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18731");

	// If we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLinesPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI18732", pbValue != __nullptr);

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
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18733");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CImageRegionWithLinesPP::initializeControls()
{
	UCLID_AFVALUEFINDERSLib::IImageRegionWithLinesPtr ipImageRegionWithLines = m_ppUnk[0];
	ASSERT_RESOURCE_ALLOCATION("ELI19033", ipImageRegionWithLines);

	IImageLineUtilityPtr ipLineUtil = ipImageRegionWithLines->LineUtil;
	ASSERT_RESOURCE_ALLOCATION("ELI19034", ipLineUtil);

	// Initialize row count min selection
	m_cmbRowCountMin.SetCurSel(ipLineUtil->RowCountMin - 2);

	// Initialize row count max selection
	if (ipLineUtil->RowCountMax == gnUNSPECIFIED)
	{
		m_cmbRowCountMax.SetCurSel(0);
	}
	else
	{
		m_cmbRowCountMax.SetCurSel(ipLineUtil->RowCountMax - 1);
	}

	// Initialize column count min selection
	m_cmbColumnCountMin.SetCurSel(ipLineUtil->ColumnCountMin - 1);

	// Initialize column count max selection
	if (ipLineUtil->ColumnCountMax == gnUNSPECIFIED)
	{
		m_cmbColumnCountMax.SetCurSel(0);
	}
	else
	{
		m_cmbColumnCountMax.SetCurSel(ipLineUtil->ColumnCountMax);
	}

	// Fill in region dimension settings as required

	if (ipLineUtil->ColumnWidthMin != gnUNSPECIFIED)
	{
		m_editColumnWidthMin.SetWindowText(asString(ipLineUtil->ColumnWidthMin).c_str());
	}
	else
	{
		m_editColumnWidthMin.Clear();
	}

	if (ipLineUtil->ColumnWidthMax != gnUNSPECIFIED)
	{
		m_editColumnWidthMax.SetWindowText(asString(ipLineUtil->ColumnWidthMax).c_str());
	}
	else
	{
		m_editColumnWidthMax.Clear();
	}

	if (ipLineUtil->LineSpacingMin != gnUNSPECIFIED)
	{
		m_editLineSpacingMin.SetWindowText(asString(ipLineUtil->LineSpacingMin).c_str());
	}
	else
	{
		m_editLineSpacingMin.Clear();
	}

	if (ipLineUtil->LineSpacingMax != gnUNSPECIFIED)
	{
		m_editLineSpacingMax.SetWindowText(asString(ipLineUtil->LineSpacingMax).c_str());
	}
	else
	{
		m_editLineSpacingMax.Clear();
	}

	if (ipLineUtil->ColumnSpacingMax != gnUNSPECIFIED)
	{
		m_editColumnSpacingMax.SetWindowText(asString(ipLineUtil->ColumnSpacingMax).c_str());
	}
	else
	{
		m_editColumnSpacingMax.Clear();
	}

	// Initialize the page specification edit boxes
	if (ipImageRegionWithLines->NumFirstPages != 0)
	{
		m_editFirstPageNums.SetWindowText(asString(ipImageRegionWithLines->NumFirstPages).c_str());
	}
	if (ipImageRegionWithLines->NumLastPages != 0)
	{
		m_editLastPageNums.SetWindowText(asString(ipImageRegionWithLines->NumLastPages).c_str());
	}
	m_editSpecifiedPageNums.SetWindowText(asString(ipImageRegionWithLines->SpecifiedPages).c_str());

	// check and enable controls as necessary according to the page selection mode
	switch (ipImageRegionWithLines->PageSelectionMode)
	{
		case kAllPages:			onSelectPages(IDC_RADIO_ALL_PAGES); break;
		case kFirstPages:		onSelectPages(IDC_RADIO_FIRST_PAGES); break;
		case kLastPages:		onSelectPages(IDC_RADIO_LAST_PAGES); break;
		case kSpecifiedPages:	onSelectPages(IDC_RADIO_SPECIFIED_PAGES); break;
		default:				onSelectPages(IDC_RADIO_ALL_PAGES); break;
	}

	// initialize attribute text control
	m_editAttributeText.SetWindowText(asString(ipImageRegionWithLines->AttributeText).c_str());

	m_chkIncludeLines.SetCheck(
		asCppBool(ipImageRegionWithLines->IncludeLines) ? BST_CHECKED : BST_UNCHECKED);
}
//-------------------------------------------------------------------------------------------------
void CImageRegionWithLinesPP::onSelectPages(WORD wRadioId)
{
	// Start by unchecking and disabling all page controls
	m_radioAllPages.SetCheck(BST_UNCHECKED);
	m_radioFirstPages.SetCheck(BST_UNCHECKED);
	m_editFirstPageNums.EnableWindow(FALSE);
	m_radioLastPages.SetCheck(BST_UNCHECKED);
	m_editLastPageNums.EnableWindow(FALSE);
	m_radioSpecifiedPages.SetCheck(BST_UNCHECKED);
	m_editSpecifiedPageNums.EnableWindow(FALSE);

	switch(wRadioId)
	{
		case IDC_RADIO_ALL_PAGES: 
		{
			// check all pages radio
			m_radioAllPages.SetCheck(BST_CHECKED);
			break;
		}

		case IDC_RADIO_FIRST_PAGES: 
		{
			// check first pages radio, and enable first pages edit box
			m_radioFirstPages.SetCheck(BST_CHECKED);
			m_editFirstPageNums.EnableWindow(TRUE);
			break;
		}

		case IDC_RADIO_LAST_PAGES: 
		{
			// check last pages radio, and enable last pages edit box
			m_radioLastPages.SetCheck(BST_CHECKED);
			m_editLastPageNums.EnableWindow(TRUE);
			break;
		}

		case IDC_RADIO_SPECIFIED_PAGES: 
		{
			// check specified pages radio, and enable specified pages edit box
			m_radioSpecifiedPages.SetCheck(BST_CHECKED);
			m_editSpecifiedPageNums.EnableWindow(TRUE);
			break;
		}

		default:
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI18941");
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CImageRegionWithLinesPP::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI18734", "Image Region With Lines PP");
}
//-------------------------------------------------------------------------------------------------