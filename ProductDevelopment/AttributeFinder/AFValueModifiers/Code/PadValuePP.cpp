// PadValuePP.cpp : Implementation of CPadValuePP
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "PadValuePP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <comutils.h>

//-------------------------------------------------------------------------------------------------
// CPadValuePP
//-------------------------------------------------------------------------------------------------
CPadValuePP::CPadValuePP() 
{
	m_dwTitleID = IDS_TITLEPadValuePP;
	m_dwHelpFileID = IDS_HELPFILEPadValuePP;
	m_dwDocStringID = IDS_DOCSTRINGPadValuePP;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValuePP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CPadValuePP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEMODIFIERSLib::IPadValuePtr ipPadValue = m_ppUnk[i];
			if (ipPadValue)
			{
				UINT nID = IDC_EDIT_REQUIRED_LENGTH;
				try
				{
					// Validate the format of required size
					BOOL bGetIntSucc = FALSE; 
					UINT nRequiredSize = GetDlgItemInt(IDC_EDIT_REQUIRED_LENGTH, &bGetIntSucc, FALSE);
					if (!bGetIntSucc)
					{
						AfxMessageBox("The Required edit box can not accept non-digit characters.");
						return S_FALSE;
					}

					CComBSTR bstrPaddingChar;
					GetDlgItemText(IDC_EDIT_PADDING_CHARACTER, bstrPaddingChar.m_str);
					try
					{
						// Store Required Size
						ipPadValue->RequiredSize = (long)nRequiredSize;
						nID = IDC_EDIT_PADDING_CHARACTER;
						// Store the Padding Charater
						string strPadChar = asString(bstrPaddingChar);
						if ( strPadChar.size() == 1)
						{
							long nPaddingCharacter = (long) strPadChar[0];
							ipPadValue->PaddingCharacter = (long)nPaddingCharacter;
						}
					}
					CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI09700");
				}
				catch (...)
				{
					ATLControls::CEdit editBox(GetDlgItem(nID));					
					editBox.SetSel(0, -1);
					editBox.SetFocus();
					return S_FALSE;
				}
				// Store radio button choice
				VARIANT_BOOL bChecked = VARIANT_FALSE;
				if (IsDlgButtonChecked(IDC_RADIO_PADLEFT) == BST_CHECKED)
				{
					// Do NOT accept smaller length
					bChecked = VARIANT_TRUE;
				}
				ipPadValue->PadLeft = bChecked;
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09698");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValuePP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CPadValuePP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFVALUEMODIFIERSLib::IPadValuePtr ipPadValue(m_ppUnk[0]);
		if (ipPadValue)
		{
			// copy all properies from that translate value to internal translate value object.

			// copy Required Size and Padding Character
			long nRequiredSize = ipPadValue->RequiredSize;
			long nPaddingCharacter = ipPadValue->PaddingCharacter;
			SetDlgItemInt(IDC_EDIT_REQUIRED_LENGTH, nRequiredSize, FALSE);
			// Set up the value for the padding character
			string strPaddingChar =  " ";
			strPaddingChar[0] = (char)nPaddingCharacter;
			SetDlgItemText(IDC_EDIT_PADDING_CHARACTER, strPaddingChar.c_str());

			//Set the max length of the padding edit text to 1
			ATLControls::CEdit editBox(GetDlgItem(IDC_EDIT_PADDING_CHARACTER));					
			editBox.SetLimitText(1);

			// Set appropriate radio button
			bool bPadLeft = (ipPadValue->PadLeft == VARIANT_TRUE);
			if (bPadLeft)
			{
				CheckDlgButton( IDC_RADIO_PADLEFT, BST_CHECKED );
				CheckDlgButton( IDC_RADIO_PADRIGHT, BST_UNCHECKED );
			}
			else
			{
				CheckDlgButton( IDC_RADIO_PADLEFT, BST_UNCHECKED );
				CheckDlgButton( IDC_RADIO_PADRIGHT, BST_CHECKED );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09699");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CPadValuePP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI09703", "Pad Value Modifier PP" );
}
//-------------------------------------------------------------------------------------------------
