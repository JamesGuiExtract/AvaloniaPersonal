// DoubleInputValidatorPP.cpp : Implementation of CDoubleInputValidatorPP
#include "stdafx.h"
#include "GeneralIV.h"
#include "DoubleInputValidatorPP.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CDoubleInputValidatorPP
//-------------------------------------------------------------------------------------------------
CDoubleInputValidatorPP::CDoubleInputValidatorPP() 
: m_bHasMinimum(false),
  m_bHasMaximum(false),
  m_bIncludeMinimum(false),
  m_bIncludeMaximum(false),
  m_bIncludeZero(false),
  m_bIncludeNegative(false),
  m_dMinimum(0.0),
  m_dMaximum(0.0)
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEDoubleInputValidatorPP;
		m_dwHelpFileID = IDS_HELPFILEDoubleInputValidatorPP;
		m_dwDocStringID = IDS_DOCSTRINGDoubleInputValidatorPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07705")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidatorPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CDoubleInputValidatorPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Retrieve this object pointer
			UCLID_GENERALIVLib::IDoubleInputValidatorPtr ipDoubleIV( m_ppUnk[i] );
			if (ipDoubleIV)
			{
				///////////////////
				// Set Minimum info
				///////////////////
				// Set flag
				ipDoubleIV->HasMin = m_bHasMinimum ? VARIANT_TRUE : VARIANT_FALSE;
				if (m_bHasMinimum)
				{
					// Retrieve and validate Minimum
					if (isMinimumValid())
					{
						// Set value
						ipDoubleIV->Min = m_dMinimum;
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
				ipDoubleIV->IncludeMinInRange = m_bIncludeMinimum ? VARIANT_TRUE 
					: VARIANT_FALSE;

				///////////////////
				// Set Maximum info
				///////////////////
				// Set flag
				ipDoubleIV->HasMax = m_bHasMaximum ? VARIANT_TRUE : VARIANT_FALSE;
				if (m_bHasMaximum)
				{
					// Retrieve and validate Maximum
					if (isMaximumValid())
					{
						// Set value
						ipDoubleIV->Max = m_dMaximum;
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
				ipDoubleIV->IncludeMaxInRange = m_bIncludeMaximum ? VARIANT_TRUE 
					: VARIANT_FALSE;

				///////////////////////////////////////
				// Maximum must be greater than minimum
				///////////////////////////////////////
				if (m_bHasMinimum && m_bHasMaximum && (m_dMinimum > m_dMaximum))
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
				ipDoubleIV->ZeroAllowed = m_bIncludeZero ? VARIANT_TRUE 
					: VARIANT_FALSE;

				// Test for inconsistent state where limit is < 0 and 
				// negatives are not to be included
				if ((m_dMinimum < 0) && !m_bIncludeNegative)
				{
					// Display error message
					AfxMessageBox( "Specified minimum is invalid when negative numbers are not allowed.");
					ATLControls::CEdit editMin( GetDlgItem( IDC_EDIT_MINIMUM ) );

					// Select entire text
					editMin.SetSel( 0, -1 );
					editMin.SetFocus();

					return S_FALSE;
				}

				if ((m_dMaximum < 0) && !m_bIncludeNegative)
				{
					// Display error message
					AfxMessageBox( "Specified maximum is invalid when negative numbers are not allowed.");
					ATLControls::CEdit editMax( GetDlgItem( IDC_EDIT_MAXIMUM ) );

					// Select entire text
					editMax.SetSel( 0, -1 );
					editMax.SetFocus();

					return S_FALSE;
				}

				ipDoubleIV->NegativeAllowed = m_bIncludeNegative ? VARIANT_TRUE 
					: VARIANT_FALSE;
			}
		}

		// Clear the Dirty flag
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04850");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidatorPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CDoubleInputValidatorPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve object pointer
		UCLID_GENERALIVLib::IDoubleInputValidatorPtr ipDoubleIV( m_ppUnk[0] );
		if (ipDoubleIV)
		{
			///////////////////
			// Set Minimum info
			///////////////////
			// Check for defined minimum
			m_bHasMinimum = ipDoubleIV->GetHasMin() == VARIANT_TRUE;
			ATLControls::CButton checkMin( GetDlgItem( IDC_CHECK_HAS_MIN ) );
			checkMin.SetCheck( m_bHasMinimum ? 1 : 0 );

			// Check value
			if (m_bHasMinimum)
			{
				m_dMinimum = ipDoubleIV->GetMin();
				CString	zTemp;
				zTemp.Format( "%G", m_dMinimum );
				ATLControls::CEdit editMin( GetDlgItem( IDC_EDIT_MINIMUM ) );

				// Set value
				editMin.SetWindowText( zTemp.operator LPCTSTR() );
			}

			// Check for Include minimum
			m_bIncludeMinimum = (ipDoubleIV->GetIncludeMinInRange() == 
				VARIANT_TRUE);
			checkMin = GetDlgItem( IDC_CHECK_INCLUDE_MIN );
			checkMin.SetCheck( m_bIncludeMinimum ? 1 : 0 );

			///////////////////
			// Set Maximum info
			///////////////////
			// Check for defined maximum
			m_bHasMaximum = ipDoubleIV->GetHasMax() == VARIANT_TRUE;
			ATLControls::CButton checkMax( GetDlgItem( IDC_CHECK_HAS_MAX ) );
			checkMax.SetCheck( m_bHasMaximum ? 1 : 0 );

			// Check value
			if (m_bHasMaximum)
			{
				m_dMaximum = ipDoubleIV->GetMax();
				CString	zTemp;
				zTemp.Format( "%G", m_dMaximum );
				ATLControls::CEdit editMax( GetDlgItem( IDC_EDIT_MAXIMUM ) );

				// Set value
				editMax.SetWindowText( zTemp.operator LPCTSTR() );
			}

			// Check for Include maximum
			m_bIncludeMaximum = (ipDoubleIV->GetIncludeMaxInRange() == 
				VARIANT_TRUE);
			checkMax = GetDlgItem( IDC_CHECK_INCLUDE_MAX );
			checkMax.SetCheck( m_bIncludeMaximum ? 1 : 0 );

			/////////////////
			// Set Other info
			/////////////////
			m_bIncludeZero = (ipDoubleIV->GetZeroAllowed() == VARIANT_TRUE);
			ATLControls::CButton checkOther( GetDlgItem( IDC_CHECK_INCLUDE_ZERO ) );
			checkOther.SetCheck( m_bIncludeZero ? 1 : 0 );

			m_bIncludeNegative = (ipDoubleIV->GetNegativeAllowed() == VARIANT_TRUE);
			checkOther = GetDlgItem( IDC_CHECK_INCLUDE_NEGATIVE );
			checkOther.SetCheck( m_bIncludeNegative ? 1 : 0 );

			// Enable/Disable appropriate controls
			setControlStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04851");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDoubleInputValidatorPP::OnClickedCheckHasMinimum(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve updated setting
		ATLControls::CButton checkMin( GetDlgItem( IDC_CHECK_HAS_MIN ) );
		m_bHasMinimum = (checkMin.GetCheck() == 1);

		// Set the Dirty flag
		SetDirty( TRUE );

		// Enable/Disable appropriate controls
		setControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04852");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDoubleInputValidatorPP::OnClickedCheckHasMaximum(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve updated setting
		ATLControls::CButton checkMax( GetDlgItem( IDC_CHECK_HAS_MAX ) );
		m_bHasMaximum = (checkMax.GetCheck() == 1);

		// Set the Dirty flag
		SetDirty( TRUE );

		// Enable/Disable appropriate controls
		setControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04853");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDoubleInputValidatorPP::OnClickedCheckIncludeMinimum(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve updated setting
		ATLControls::CButton checkInclude( GetDlgItem( IDC_CHECK_INCLUDE_MIN ) );
		m_bIncludeMinimum = (checkInclude.GetCheck() == 1);

		// Set the Dirty flag
		SetDirty( TRUE );

		// Enable/Disable appropriate controls
		setControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04854");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDoubleInputValidatorPP::OnClickedCheckIncludeMaximum(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve updated setting
		ATLControls::CButton checkInclude( GetDlgItem( IDC_CHECK_INCLUDE_MAX ) );
		m_bIncludeMaximum = (checkInclude.GetCheck() == 1);

		// Set the Dirty flag
		SetDirty( TRUE );

		// Enable/Disable appropriate controls
		setControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04855");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDoubleInputValidatorPP::OnClickedCheckIncludeZero(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve updated setting
		ATLControls::CButton checkInclude( GetDlgItem( IDC_CHECK_INCLUDE_ZERO ) );
		m_bIncludeZero = (checkInclude.GetCheck() == 1);

		// Set the Dirty flag
		SetDirty( TRUE );

		// Enable/Disable appropriate controls
		setControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04856");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDoubleInputValidatorPP::OnClickedCheckIncludeNegative(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve updated setting
		ATLControls::CButton checkInclude( GetDlgItem( IDC_CHECK_INCLUDE_NEGATIVE ) );
		m_bIncludeNegative = (checkInclude.GetCheck() == 1);

		// Set the Dirty flag
		SetDirty( TRUE );

		// Enable/Disable appropriate controls
		setControlStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04857");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool CDoubleInputValidatorPP::isMaximumValid()
{
	bool	bValid = false;

	// Retrieve text
	double	dValue = 0.0;
	ATLControls::CEdit editMax( GetDlgItem( IDC_EDIT_MAXIMUM ) );
	CComBSTR bstrTemp;
	editMax.GetWindowText( bstrTemp.m_str );
	_bstr_t	strTemp( bstrTemp );

	// Convert to double
	try
	{
		// Empty string is considered invalid
		if (strTemp.length() > 0)
		{
			dValue = asDouble( strTemp.operator const char *() );

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
		m_dMaximum = dValue;
	}

	return bValid;
}	
//-------------------------------------------------------------------------------------------------
bool CDoubleInputValidatorPP::isMinimumValid()
{
	bool	bValid = false;

	// Retrieve text
	double	dValue = 0.0;
	ATLControls::CEdit editMin( GetDlgItem( IDC_EDIT_MINIMUM ) );
	CComBSTR	bstrTemp;
	editMin.GetWindowText( bstrTemp.m_str );
	_bstr_t	strTemp( bstrTemp );

	// Convert to double
	try
	{
		// Empty string is considered invalid
		if (strTemp.length() > 0)
		{
			dValue = asDouble( strTemp.operator const char *() );

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
		m_dMinimum = dValue;
	}

	return bValid;
}	
//-------------------------------------------------------------------------------------------------
void CDoubleInputValidatorPP::setControlStates()
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
void CDoubleInputValidatorPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07703", 
		"Double Input Validator PP" );
}
//-------------------------------------------------------------------------------------------------
