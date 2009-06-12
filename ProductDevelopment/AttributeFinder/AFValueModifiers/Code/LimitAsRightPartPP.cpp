// LimitAsRightPartPP.cpp : Implementation of CLimitAsRightPartPP
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "LimitAsRightPartPP.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CLimitAsRightPartPP
//-------------------------------------------------------------------------------------------------
CLimitAsRightPartPP::CLimitAsRightPartPP() 
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLELimitAsRightPartPP;
		m_dwHelpFileID = IDS_HELPFILELimitAsRightPartPP;
		m_dwDocStringID = IDS_DOCSTRINGLimitAsRightPartPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07717")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsRightPartPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{		
		ATLTRACE(_T("CLimitAsRightPartPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEMODIFIERSLib::ILimitAsRightPartPtr ipLimitRight = m_ppUnk[i];
			if (ipLimitRight)
			{
				try
				{
					// num of characters
					UINT nNumOfChars = GetDlgItemInt(IDC_EDIT_NUM_OF_CHARS_RIGHT, NULL, FALSE);
					try
					{
						ipLimitRight->NumberOfCharacters = (long)nNumOfChars;
					}
					CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI05827");
				}
				catch (...)
				{
					ATLControls::CEdit editBox(GetDlgItem(IDC_EDIT_NUM_OF_CHARS_RIGHT));
					editBox.SetSel(0, -1);
					editBox.SetFocus();
					return S_FALSE;
				}
				
				// set boolean
				ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_ACCEPT_RIGHT));
				int nChecked = checkBox.GetCheck();
				VARIANT_BOOL bAcceptSmallerLength = nChecked==1?VARIANT_TRUE:VARIANT_FALSE;
				ipLimitRight->AcceptSmallerLength = bAcceptSmallerLength;
				
				// Store Extract vs. Remove choice
				VARIANT_BOOL vbExtract = VARIANT_TRUE;
				if (IsDlgButtonChecked( IDC_RADIO_EXTRACT ) == BST_UNCHECKED)
				{
					// Remove characters
					vbExtract = VARIANT_FALSE;
				}
				ipLimitRight->Extract = vbExtract;
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04298");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsRightPartPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CLimitAsRightPartPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFVALUEMODIFIERSLib::ILimitAsRightPartPtr ipLimitRight(m_ppUnk[0]);
		if (ipLimitRight)
		{
			// copy all properies from that translate value to internal translate value object.
			long nNumOfChars = ipLimitRight->NumberOfCharacters;
			if (nNumOfChars < 0) nNumOfChars = 0;
			SetDlgItemInt(IDC_EDIT_NUM_OF_CHARS_RIGHT, nNumOfChars, FALSE);

			VARIANT_BOOL bAcceptSmallerLength = ipLimitRight->AcceptSmallerLength;
			ATLControls::CButton checkBox(GetDlgItem(IDC_CHK_ACCEPT_RIGHT));
			checkBox.SetCheck(bAcceptSmallerLength==VARIANT_TRUE?1:0);

			// Set appropriate radio button
			bool bExtract = (ipLimitRight->Extract == VARIANT_TRUE);
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04297");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CLimitAsRightPartPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07691", 
		"LimitAsRightPart Modifier PP" );
}
//-------------------------------------------------------------------------------------------------
