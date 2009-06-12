// IntegerInputValidatorPP.cpp : Implementation of CIntegerInputValidatorPP
#include "stdafx.h"
#include "GeneralIV.h"
#include "IntegerInputValidatorPP.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CIntegerInputValidatorPP
//-------------------------------------------------------------------------------------------------
CIntegerInputValidatorPP::CIntegerInputValidatorPP() 
: m_bHasMinimum(false),
  m_bHasMaximum(false),
  m_bIncludeMinimum(false),
  m_bIncludeMaximum(false),
  m_bIncludeZero(false),
  m_bIncludeNegative(false),
  m_lMinimum(0),
  m_lMaximum(0)
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEIntegerInputValidatorPP;
		m_dwHelpFileID = IDS_HELPFILEIntegerInputValidatorPP;
		m_dwDocStringID = IDS_DOCSTRINGIntegerInputValidatorPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07706")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidatorPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CIntegerInputValidatorPP::Apply\n"));

		// Check licensing
		validateLicense();

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Retrieve this object pointer
			UCLID_GENERALIVLib::IIntegerInputValidatorPtr ipIntegerIV( m_ppUnk[i] );
			if (ipIntegerIV)
			{
				///////////////////
				// Set Minimum info
				///////////////////
				// Set flag
				ipIntegerIV->HasMin = m_bHasMinimum ? VARIANT_TRUE : VARIANT_FALSE;
				if (m_bHasMinimum)
				{
					// Retrieve and validate Minimum
					if (isMinimumValid())
					{
						// Set value
						ipIntegerIV->Min = m_lMinimum;
					}
					else
					{
						// Display error message
						AfxMessageBox( "Specified minimum is invalid.");
						ATLControls::CEdit editMin( GetDlgItem( IDC_EDIT_MINIMUM ) );

						// Select entire text
						editMin.SetSel( 0, -1 );
						editMin.SetFocus();

						return S_FALSE;
					}
				}
				// Set Included flag
				ipIntegerIV->IncludeMinInRange = m_bIncludeMinimum ? VARIANT_TRUE 
					: VARIANT_FALSE;

				///////////////////
				// Set Maximum info
				///////////////////
				// Set flag
				ipIntegerIV->HasMax = m_bHasMaximum ? VARIANT_TRUE : VARIANT_FALSE;
				if (m_bHasMaximum)
				{
					// Retrieve and validate Maximum
					if (isMaximumValid())
					{
						// Set value
						ipIntegerIV->Max = m_lMaximum;
					}
					else
					{
						// Display error message
						AfxMessageBox( "Specified maximum is invalid.");
						ATLControls::CEdit editMax( GetDlgItem( IDC_EDIT_MAXIMUM ) );

						// Select entire text
						editMax.SetSel( 0, -1 );
						editMax.SetFocus();

						return S_FALSE;
					}
				}
				// Set Included flag
				ipIntegerIV->IncludeMaxInRange = m_bIncludeMaximum ? VARIANT_TRUE 
					: VARIANT_FALSE;

				///////////////////////////////////////
				// Maximum must be greater than minimum
				///////////////////////////////////////
				if (m_bHasMinimum && m_bHasMaximum && (m_lMinimum > m_lMaximum))
				{
					// Display error message
					AfxMessageBox( "Maximum must be greater than minimum.");
					ATLControls::CEdit editMax( GetDlgItem( IDC_EDIT_MAXIMUM ) );

					// Select entire text
					editMax.SetSel( 0, -1 );
					editMax.SetFocus();

					return S_FALSE;
				}

				//////////////////
				// Set Other flags
				//////////////////
				ipIntegerIV->ZeroAllowed = m_bIncludeZero ? VARIANT_TRUE 
					: VARIANT_FALSE;

				// Test for inconsistent state where limit is < 0 and 
				// negatives are not to be included
				if ((m_lMinimum < 0) && !m_bIncludeNegative)
				{
					// Display error message
					AfxMessageBox( "Specified minimum is invalid when negative numbers are not allowed.");
					ATLControls::CEdit editMin( GetDlgItem( IDC_EDIT_MINIMUM ) );

					// Select entire text
					editMin.SetSel( 0, -1 );
					editMin.SetFocus();

					return S_FALSE;
				}

				if ((m_lMaximum < 0) && !m_bIncludeNegative)
				{
					// Display error message
					AfxMessageBox( "Specified maximum is invalid when negative numbers are not allowed.");
					ATLControls::CEdit editMax( GetDlgItem( IDC_EDIT_MAXIMUM ) );

					// Select entire text
					editMax.SetSel( 0, -1 );
					editMax.SetFocus();

					return S_FALSE;
				}

				ipIntegerIV->NegativeAllowed = m_bIncludeNegative ? VARIANT_TRUE 
					: VARIANT_FALSE;
			}
		}

		// Clear the Dirty flag
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04831");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidatorPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CIntegerInputValidatorPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Retrieve object pointer
		UCLID_GENERALIVLib::IIntegerInputValidatorPtr ipIntegerIV( m_ppUnk[0] );
		if (ipIntegerIV)
		{
			///////////////////
			// Set Minimum info
			///////////////////
			// Check for defined minimum
			m_bHasMinimum = ipIntegerIV->GetHasMin() == VARIANT_TRUE;
			ATLControls::CButton checkMin( GetDlgItem( IDC_CHECK_HAS_MIN ) );
			checkMin.SetCheck( m_bHasMinimum ? 1 : 0 );

			// Check value
			if (m_bHasMinimum)
			{
				m_lMinimum = ipIntegerIV->GetMin();
				CString	zTemp;
				zTemp.Format( "%ld", m_lMinimum );
				ATLControls::CEdit editMin( GetDlgItem( IDC_EDIT_MINIMUM ) );

				// Set value
				editMin.SetWindowText( zTemp.operator LPCTSTR() );
			}

			// Check for Include minimum
			m_bIncludeMinimum = (ipIntegerIV->GetIncludeMinInRange() == 
				VARIANT_TRUE);
			checkMin = GetDlgItem( IDC_CHECK_INCLUDE_MIN );
			checkMin.SetCheck( m_bIncludeMinimum ? 1 : 0 );

			///////////////////
			// Set Maximum info
			///////////////////
			// Check for defined maximum
			m_bHasMaximum = ipIntegerIV->GetHasMax() == VARIANT_TRUE;
			ATLControls::CButton checkMax( GetDlgItem( IDC_CHECK_HAS_MAX ) );
			checkMax.SetCheck( m_bHasMaximum ? 1 : 0 );

			// Check value
			if (m_bHasMaximum)
			{
				m_lMaximum = ipIntegerIV->GetMax();
				CString	zTemp;
				zTemp.Format( "%ld", m_lMaximum );
				ATLControls::CEdit editMax( GetDlgItem( IDC_EDIT_MAXIMUM ) );

				// Set value
				editMax.SetWindowText( zTemp.operator LPCTSTR() );
			}

			// Check for Include maximum
			m_bIncludeMaximum = (ipIntegerIV->GetIncludeMaxInRange() == 
				VARIANT_TRUE);
			checkMax = GetDlgItem( IDC_CHECK_INCLUDE_MAX );
			checkMax.SetCheck( m_bIncludeMaximum ? 1 : 0 );

			/////////////////
			// Set Other info
			/////////////////
			m_bIncludeZero = (ipIntegerIV->GetZeroAllowed() == VARIANT_TRUE);
			ATLControls::CButton checkOther( GetDlgItem( IDC_CHECK_INCLUDE_ZERO ) );
			checkOther.SetCheck( m_bIncludeZero ? 1 : 0 );

			m_bIncludeNegative = (ipIntegerIV->GetNegativeAllowed() == VARIANT_TRUE);
			checkOther = GetDlgItem( IDC_CHECK_INCLUDE_NEGATIVE );
			checkOther.SetCheck( m_bIncludeNegative ? 1 : 0 );

			// Enable/Disable appropriate controls
			setControlStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04824");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CIntegerInputValidatorPP::OnClickedCheckHasMinimum(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Retrieve updated setting
		ATLControls::CButton checkMin( GetDlgItem( IDC_CHECK_HAS_MIN ) );
		m_bHasMinimum = (checkMin.GetCheck() == 1);

		// Set the Dirty flag
		SetDirty( TRUE );

		// Enable/Disable appropriate controls
		setControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04829");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CIntegerInputValidatorPP::OnClickedCheckHasMaximum(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Retrieve updated setting
		ATLControls::CButton checkMax( GetDlgItem( IDC_CHECK_HAS_MAX ) );
		m_bHasMaximum = (checkMax.GetCheck() == 1);

		// Set the Dirty flag
		SetDirty( TRUE );

		// Enable/Disable appropriate controls
		setControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04830");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CIntegerInputValidatorPP::OnClickedCheckIncludeMinimum(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Retrieve updated setting
		ATLControls::CButton checkInclude( GetDlgItem( IDC_CHECK_INCLUDE_MIN ) );
		m_bIncludeMinimum = (checkInclude.GetCheck() == 1);

		// Set the Dirty flag
		SetDirty( TRUE );

		// Enable/Disable appropriate controls
		setControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04837");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CIntegerInputValidatorPP::OnClickedCheckIncludeMaximum(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Retrieve updated setting
		ATLControls::CButton checkInclude( GetDlgItem( IDC_CHECK_INCLUDE_MAX ) );
		m_bIncludeMaximum = (checkInclude.GetCheck() == 1);

		// Set the Dirty flag
		SetDirty( TRUE );

		// Enable/Disable appropriate controls
		setControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04839");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CIntegerInputValidatorPP::OnClickedCheckIncludeZero(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Retrieve updated setting
		ATLControls::CButton checkInclude( GetDlgItem( IDC_CHECK_INCLUDE_ZERO ) );
		m_bIncludeZero = (checkInclude.GetCheck() == 1);

		// Set the Dirty flag
		SetDirty( TRUE );

		// Enable/Disable appropriate controls
		setControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04840");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CIntegerInputValidatorPP::OnClickedCheckIncludeNegative(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Retrieve updated setting
		ATLControls::CButton checkInclude( GetDlgItem( IDC_CHECK_INCLUDE_NEGATIVE ) );
		m_bIncludeNegative = (checkInclude.GetCheck() == 1);

		// Set the Dirty flag
		SetDirty( TRUE );

		// Enable/Disable appropriate controls
		setControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04841");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool CIntegerInputValidatorPP::isMaximumValid()
{
	bool	bValid = false;

	// Retrieve text
	long	lValue = 0;
	ATLControls::CEdit editMax( GetDlgItem( IDC_EDIT_MAXIMUM ) );
	CComBSTR bstrTemp;
	editMax.GetWindowText( bstrTemp.m_str );
	_bstr_t	strTemp( bstrTemp );

	// Convert to long
	try
	{
		// Empty string is considered invalid
		if (strTemp.length() > 0)
		{
			lValue = asLong( strTemp.operator const char *() );

			// Set flag
			bValid = true;
		}
	}
	catch (...)
	{
		// Trap and ignore exception
	}

	// If valid, store into data member
	if (bValid)
	{
		m_lMaximum = lValue;
	}

	return bValid;
}	
//-------------------------------------------------------------------------------------------------
bool CIntegerInputValidatorPP::isMinimumValid()
{
	bool	bValid = false;

	// Retrieve text
	long	lValue = 0;
	ATLControls::CEdit editMin( GetDlgItem( IDC_EDIT_MINIMUM ) );
	CComBSTR	bstrTemp;
	editMin.GetWindowText( bstrTemp.m_str );
	_bstr_t	strTemp( bstrTemp );

	// Convert to long
	try
	{
		// Empty string is considered invalid
		if (strTemp.length() > 0)
		{
			lValue = asLong( strTemp.operator const char *() );

			// Set flag
			bValid = true;
		}
	}
	catch (...)
	{
		// Trap and ignore exception
	}

	// If valid, store into data member
	if (bValid)
	{
		m_lMinimum = lValue;
	}

	return bValid;
}	
//-------------------------------------------------------------------------------------------------
void CIntegerInputValidatorPP::setControlStates()
{
	// Check Minimum flag
	ATLControls::CEdit editMin( GetDlgItem( IDC_EDIT_MINIMUM ) );
	ATLControls::CButton btnInclude( GetDlgItem( IDC_CHECK_INCLUDE_MIN ) );
	ATLControls::CButton btnMin( GetDlgItem( IDC_CHECK_HAS_MIN ) );

	if (btnMin.GetCheck() != 1)
	{
		// Disable the edit box
		editMin.EnableWindow( FALSE );

		// Disable the Include check box
		btnInclude.EnableWindow( FALSE );
	}
	else
	{
		// Enable the edit box
		editMin.EnableWindow( TRUE );

		// Enable the Include check box
		btnInclude.EnableWindow( TRUE );
	}

	// Check Maximum flag
	btnInclude = GetDlgItem( IDC_CHECK_INCLUDE_MAX );
	ATLControls::CEdit editMax( GetDlgItem( IDC_EDIT_MAXIMUM ) );
	ATLControls::CButton btnMax( GetDlgItem( IDC_CHECK_HAS_MAX ) );

	if (btnMax.GetCheck() != 1)
	{
		// Disable the edit box
		editMax.EnableWindow( FALSE );

		// Disable the Include check box
		btnInclude.EnableWindow( FALSE );
	}
	else
	{
		// Enable the edit box
		editMax.EnableWindow( TRUE );

		// Enable the Include check box
		btnInclude.EnableWindow( TRUE );
	}
}	
//-------------------------------------------------------------------------------------------------
void CIntegerInputValidatorPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07704", 
		"Integer Input Validator PP" );
}
//-------------------------------------------------------------------------------------------------
