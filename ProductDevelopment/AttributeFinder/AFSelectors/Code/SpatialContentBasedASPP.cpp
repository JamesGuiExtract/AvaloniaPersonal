// SpatialContentBasedASPP.cpp : Implementation of CSpatialContentBasedASPP

#include "stdafx.h"
#include "SpatialContentBasedASPP.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <Comutils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CSpatialContentBasedASPP
//-------------------------------------------------------------------------------------------------
CSpatialContentBasedASPP::CSpatialContentBasedASPP()
{
	m_dwTitleID = IDS_TITLESPATIALCONTENTBASEDASPP;
	m_dwHelpFileID = IDS_HELPFILESPATIALCONTENTBASEDASPP;
	m_dwDocStringID = IDS_DOCSTRINGSPATIALCONTENTBASEDASPP;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedASPP::Apply()
{
	try
	{
		validateLicense();

		ATLTRACE(_T("CSpatialContentBasedASPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFSELECTORSLib::ISpatialContentBasedASPtr ipSBAS = m_ppUnk[i];
			if ( ipSBAS != __nullptr )
			{
				if ( m_cmbContains.GetCurSel() == 0 )
				{
					ipSBAS->Contains = VARIANT_TRUE;
				}
				else
				{
					ipSBAS->Contains = VARIANT_FALSE;
				}
				CString zTemp;

				// Validate Number of Consecutive Rows > 0
				m_editConsecutiveRows.GetWindowTextA( zTemp );
				long lValue;
				try
				{
					lValue = asLong( zTemp.operator LPCSTR() );
					if ( lValue <= 0 )
					{
						UCLIDException ue("ELI13352", "Consecutive Rows must be > 0.");
						throw ue;
					}
					ipSBAS->ConsecutiveRows = lValue;
				}
				catch (...)
				{
					m_editConsecutiveRows.SetFocus();
					throw;
				}

				// Validate Maximum Percent < 100
				m_editMaxPercent.GetWindowTextA( zTemp );
				long lMax;
				try
				{
					lMax = asLong ( zTemp.operator LPCSTR() );
					if ( lMax > 100 )
					{
						m_editMaxPercent.SetFocus();
						UCLIDException ue("ELI13353", "Max percent must not be greater than 100.");
						throw ue;
					}
					ipSBAS->MaxPercent = lMax;
				}
				catch(...)
				{
					m_editMaxPercent.SetFocus();
					throw;
				}

				// Validate Minimum Percent < Maximum Percent
				m_editMinPercent.GetWindowTextA( zTemp );
				long lMin;
				try
				{
					lMin = asLong ( zTemp.operator LPCSTR() );
					if ( lMin > lMax )
					{
						UCLIDException ue("ELI13354", "Min Percent must be less than or equal to Max Percent.");
						throw ue;
					}
					ipSBAS->MinPercent = lMin;
				}
				catch(...)
				{
					m_editMinPercent.SetFocus();
					throw;
				}

				ipSBAS->IncludeNonSpatial = (m_checkIncludeNonSpatial.GetCheck() == BST_CHECKED ) ? VARIANT_TRUE : VARIANT_FALSE;
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13328")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CSpatialContentBasedASPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
											  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		UCLID_AFSELECTORSLib::ISpatialContentBasedASPtr ipSBAS = m_ppUnk[0];
		if (ipSBAS != __nullptr)
		{
			m_cmbContains.Attach( GetDlgItem( IDC_COMBO_CONTAINS ));
			m_editConsecutiveRows.Attach( GetDlgItem( IDC_EDIT_PIXEL_ROWS ));
			m_editMinPercent.Attach( GetDlgItem( IDC_EDIT_MIN_PERCENT ));
			m_editMaxPercent.Attach( GetDlgItem( IDC_EDIT_MAX_PERCENT ));
			m_checkIncludeNonSpatial.Attach( GetDlgItem( IDC_CHECK_INCLUDE_NONSPATIAL ));

			m_cmbContains.ResetContent();
			m_cmbContains.AddString( "contain");
			m_cmbContains.AddString( "do not contain");
			if ( ipSBAS->Contains == VARIANT_TRUE )
			{
				m_cmbContains.SetCurSel(0);
			}
			else
			{
				m_cmbContains.SetCurSel(1);
			}
			m_editConsecutiveRows.SetWindowTextA( asString( ipSBAS->ConsecutiveRows ).c_str());
			m_editMinPercent.SetWindowTextA( asString( ipSBAS->MinPercent ).c_str());
			m_editMaxPercent.SetWindowTextA( asString( ipSBAS->MaxPercent ).c_str());
			if ( ipSBAS->IncludeNonSpatial == VARIANT_TRUE )
			{
				m_checkIncludeNonSpatial.SetCheck ( BST_CHECKED );
			}
			else
			{
				m_checkIncludeNonSpatial.SetCheck ( BST_UNCHECKED );
			}
		}
	
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13327");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// ILicensed Component Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedASPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Private Methods
//-------------------------------------------------------------------------------------------------
void CSpatialContentBasedASPP::validateLicense()
{
	static const unsigned long SPATIAL_CONTENT_BASED_ASPP_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( SPATIAL_CONTENT_BASED_ASPP_ID, "ELI13358", 
		"Spatial Content Based Attribute Selector Property Page" );
}
//-------------------------------------------------------------------------------------------------
