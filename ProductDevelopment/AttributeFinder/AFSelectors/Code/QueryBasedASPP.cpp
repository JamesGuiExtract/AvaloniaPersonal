// QueryBasedASPP.cpp : Implementation of CQueryBasedASPP

#include "stdafx.h"
#include "QueryBasedASPP.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <Comutils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CQueryBasedASPP
//-------------------------------------------------------------------------------------------------
CQueryBasedASPP::CQueryBasedASPP()
{
	m_dwTitleID = IDS_TITLEQUERYBASEDASPP;
	m_dwHelpFileID = IDS_HELPFILEQUERYBASEDASPP;
	m_dwDocStringID = IDS_DOCSTRINGQUERYBASEDASPP;
}

//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedASPP::Apply()
{
	try
	{
		// Check license
		validateLicense();

		ATLTRACE(_T("CQueryBasedASPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFSELECTORSLib::IQueryBasedASPtr ipQBAS = m_ppUnk[i];
			if ( ipQBAS != __nullptr )
			{
				CString zQueryText;
				m_editQuery.GetWindowTextA(zQueryText);
				if ( zQueryText.IsEmpty() )
				{
					UCLIDException ue("ELI13351", "Query Text must not be empty.");
					throw ue;
				}
				ipQBAS->QueryText = zQueryText.operator LPCTSTR();
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13326")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CQueryBasedASPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
											  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		UCLID_AFSELECTORSLib::IQueryBasedASPtr ipQBAS = m_ppUnk[0];
		if (ipQBAS != __nullptr)
		{
			// "Create" all the controls
			m_editQuery.Attach( GetDlgItem( IDC_EDIT_QUERY ));
			string strQuery = asString(ipQBAS->QueryText);
			m_editQuery.SetWindowTextA(strQuery.c_str());
		}
	
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13329");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedASPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CQueryBasedASPP::validateLicense()
{
	static const unsigned long QUERY_BASED_ASPP_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( QUERY_BASED_ASPP_ID, "ELI13359", "Query Based Attribute Selector PP" );
}
//-------------------------------------------------------------------------------------------------
