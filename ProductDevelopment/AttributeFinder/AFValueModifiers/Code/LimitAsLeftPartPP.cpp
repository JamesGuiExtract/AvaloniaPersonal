// LimitAsLeftPartPP.cpp : Implementation of CLimitAsLeftPartPP
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "LimitAsLeftPartPP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CLimitAsLeftPartPP
//-------------------------------------------------------------------------------------------------
CLimitAsLeftPartPP::CLimitAsLeftPartPP() 
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLELimitAsLeftPartPP;
		m_dwHelpFileID = IDS_HELPFILELimitAsLeftPartPP;
		m_dwDocStringID = IDS_DOCSTRINGLimitAsLeftPartPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07715")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPartPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CLimitAsLeftPartPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEMODIFIERSLib::ILimitAsLeftPartPtr ipLimitLeft = m_ppUnk[i];
			if (ipLimitLeft)
			{
				try
				{
					// num of characters
					UINT nNumOfChars = GetDlgItemInt(IDC_EDIT_NUM_OF_CHARS_LEFT, NULL, FALSE);
					try
					{
						ipLimitLeft->NumberOfCharacters = (long)nNumOfChars;
					}
					CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI05821");
				}
				catch (...)
				{
					ATLControls::CEdit editBox(GetDlgItem(IDC_EDIT_NUM_OF_CHARS_LEFT));
					editBox.SetSel(0, -1);
					editBox.SetFocus();
					return S_FALSE;
				}
				
				// set boolean
				ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_ACCEPT_LEFT));
				int nChecked = checkBox.GetCheck();
				VARIANT_BOOL bAcceptSmallerLength = nChecked==1?VARIANT_TRUE:VARIANT_FALSE;
				
				ipLimitLeft->AcceptSmallerLength = bAcceptSmallerLength;
				
				// Store Extract vs. Remove choice
				VARIANT_BOOL vbExtract = VARIANT_TRUE;
				if (IsDlgButtonChecked( IDC_RADIO_EXTRACT ) == BST_UNCHECKED)
				{
					// Remove characters
					vbExtract = VARIANT_FALSE;
				}
				ipLimitLeft->Extract = vbExtract;
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04245");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPartPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Windows Message Handlers
//-------------------------------------------------------------------------------------------------
LRESULT CLimitAsLeftPartPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFVALUEMODIFIERSLib::ILimitAsLeftPartPtr ipLimitLeft(m_ppUnk[0]);
		if (ipLimitLeft)
		{
			// copy all properies from that translate value to internal translate value object.
			long nNumOfChars = ipLimitLeft->NumberOfCharacters;
			if (nNumOfChars < 0) nNumOfChars = 0;
			SetDlgItemInt(IDC_EDIT_NUM_OF_CHARS_LEFT, nNumOfChars, FALSE);

			// set check
			VARIANT_BOOL bAcceptSmallerLength = ipLimitLeft->AcceptSmallerLength;
			ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_ACCEPT_LEFT));
			checkBox.SetCheck(bAcceptSmallerLength==VARIANT_TRUE?1:0);

			// Set appropriate radio button
			bool bExtract = (ipLimitLeft->Extract == VARIANT_TRUE);
			if (bExtract)
			{
				CheckDlgButton( IDC_RADIO_EXTRACT, BST_CHECKED );
				CheckDlgButton( IDC_RADIO_REMOVE, BST_UNCHECKED );
			}
			else
			{
				CheckDlgButton( IDC_RADIO_EXTRACT, BST_UNCHECKED );
				CheckDlgButton( IDC_RADIO_REMOVE, BST_CHECKED );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04244");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CLimitAsLeftPartPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07689", 
		"LimitAsLeftPart Modifier PP" );
}
//-------------------------------------------------------------------------------------------------
