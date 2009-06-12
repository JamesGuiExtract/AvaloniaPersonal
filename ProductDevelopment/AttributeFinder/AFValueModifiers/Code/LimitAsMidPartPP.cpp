// LimitAsMidPartPP.cpp : Implementation of CLimitAsMidPartPP
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "LimitAsMidPartPP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CLimitAsMidPartPP
//-------------------------------------------------------------------------------------------------
CLimitAsMidPartPP::CLimitAsMidPartPP() 
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLELimitAsMidPartPP;
		m_dwHelpFileID = IDS_HELPFILELimitAsMidPartPP;
		m_dwDocStringID = IDS_DOCSTRINGLimitAsMidPartPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07716")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPartPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CLimitAsMidPartPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEMODIFIERSLib::ILimitAsMidPartPtr ipLimitMid = m_ppUnk[i];
			if (ipLimitMid)
			{
				UINT nID = IDC_EDIT_START_POS;
				try
				{
					// Validate start and end positions
					UINT nStartPos = GetDlgItemInt(IDC_EDIT_START_POS, NULL, FALSE);
					UINT nEndPos = GetDlgItemInt(IDC_EDIT_END_POS, NULL, FALSE);
					try
					{
						// Store start and end position
						ipLimitMid->StartPosition = (long)nStartPos;
						nID = IDC_EDIT_END_POS;
						ipLimitMid->EndPosition = (long)nEndPos;
					}
					CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI05822");
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
				if (IsDlgButtonChecked(IDC_CHK_ACCEPT_SHORTER_MID) == BST_CHECKED)
				{
					// Do NOT accept smaller length
					bChecked = VARIANT_TRUE;
				}
				ipLimitMid->AcceptSmallerLength = bChecked;

				// Store Extract vs. Remove choice
				VARIANT_BOOL vbExtract = VARIANT_TRUE;
				if (IsDlgButtonChecked( IDC_RADIO_EXTRACT ) == BST_UNCHECKED)
				{
					// Remove characters
					vbExtract = VARIANT_FALSE;
				}
				ipLimitMid->Extract = vbExtract;
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19288");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPartPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CLimitAsMidPartPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFVALUEMODIFIERSLib::ILimitAsMidPartPtr ipLimitMid(m_ppUnk[0]);
		if (ipLimitMid)
		{
			// copy all properies from that translate value to internal translate value object.

			// copy start and end position
			long nStartPos = ipLimitMid->StartPosition;
			long nEndPos = ipLimitMid->EndPosition;
			SetDlgItemInt(IDC_EDIT_START_POS, nStartPos, FALSE);
			SetDlgItemInt(IDC_EDIT_END_POS, nEndPos, FALSE);

			// check appropriate radio button
			CheckDlgButton(IDC_CHK_ACCEPT_SHORTER_MID, 
				ipLimitMid->AcceptSmallerLength == VARIANT_TRUE ? BST_CHECKED : BST_UNCHECKED);

			// Set appropriate radio button
			bool bExtract = (ipLimitMid->Extract == VARIANT_TRUE);
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19287");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CLimitAsMidPartPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07690", 
		"LimitAsMidPart Modifier PP" );
}
//-------------------------------------------------------------------------------------------------
