// AddressSplitterPP.cpp : Implementation of CAddressSplitterPP

#include "stdafx.h"
#include "AFSplitters.h"
#include "AddressSplitterPP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CAddressSplitterPP
//-------------------------------------------------------------------------------------------------
CAddressSplitterPP::CAddressSplitterPP()
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEAddressSplitterPP;
		m_dwHelpFileID = IDS_HELPFILEAddressSplitterPP;
		m_dwDocStringID = IDS_DOCSTRINGAddressSplitterPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI08495")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitterPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CAddressSplitterPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CAddressSplitterPP::Apply\n"));

		// Check box setting
		ATLControls::CButton checkBox( GetDlgItem( IDC_CHECK_COMBINE_NAME_ADDRESS ) );
		int nChecked = checkBox.GetCheck();

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFSPLITTERSLib::IAddressSplitterPtr ipAddressSplitter = m_ppUnk[i];

			if (ipAddressSplitter)
			{
				// Set Combine Name Address setting
				ipAddressSplitter->CombinedNameAddress = 
					(nChecked == BST_CHECKED) ? VARIANT_TRUE : VARIANT_FALSE;
			}
		}

		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08497");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CAddressSplitterPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFSPLITTERSLib::IAddressSplitterPtr ipAddressSplitter(m_ppUnk[0]);
		if (ipAddressSplitter)
		{
			// Retrieve and apply Combine Name Address setting
			bool	bCombine = (ipAddressSplitter->CombinedNameAddress == VARIANT_TRUE);

			CheckDlgButton( IDC_CHECK_COMBINE_NAME_ADDRESS, 
				bCombine ? BST_CHECKED : BST_UNCHECKED );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08498");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CAddressSplitterPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI08496", "AddressSplitter PP" );
}
//-------------------------------------------------------------------------------------------------
