// CreateValuePP.cpp : Implementation of CCreateValuePP
#include "stdafx.h"
#include "AFValueFinders.h"
#include "CreateValuePP.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CCreateValuePP
//-------------------------------------------------------------------------------------------------
CCreateValuePP::CCreateValuePP() 
{
	m_dwTitleID = IDS_TITLECreateValuePP;
	m_dwHelpFileID = IDS_HELPFILECreateValuePP;
	m_dwDocStringID = IDS_DOCSTRINGCreateValuePP;
}
//-------------------------------------------------------------------------------------------------
CCreateValuePP::~CCreateValuePP() 
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16342");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValuePP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CCreateValuePP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEFINDERSLib::ICreateValuePtr ipCreateValue = m_ppUnk[i];

			CComBSTR bstrValue;
			GetDlgItemText(IDC_EDIT_VALUE, bstrValue.m_str);
			_bstr_t _bstrValue = bstrValue;
			if(_bstrValue.length() == 0)
			{
				throw UCLIDException("ELI09861", "Please Specify a value.");
			}
			ipCreateValue->ValueString = _bstrValue;

			CComBSTR bstrType;
			GetDlgItemText(IDC_EDIT_TYPE, bstrType.m_str);
			_bstr_t _bstrType = bstrType;
			ipCreateValue->TypeString = _bstrType;
		}
		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09860")

	// Getting here means an exception was thrown
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CCreateValuePP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{

		m_editValue = GetDlgItem(IDC_EDIT_VALUE);
		m_editType = GetDlgItem(IDC_EDIT_TYPE);

		if (m_nObjects > 0)
		{

			UCLID_AFVALUEFINDERSLib::ICreateValuePtr ipCreateValue = m_ppUnk[0];
			m_editValue.SetWindowText(ipCreateValue->ValueString);
			m_editType.SetWindowText(ipCreateValue->TypeString);
		}
		
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09859");

	return 1;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValuePP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CCreateValuePP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI09858", 
		"Create Value PP" );
}
//-------------------------------------------------------------------------------------------------
