// ChangeCasePP.cpp : Implementation of CChangeCasePP
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "ChangeCasePP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CChangeCasePP
//-------------------------------------------------------------------------------------------------
CChangeCasePP::CChangeCasePP() 
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEChangeCasePP;
		m_dwHelpFileID = IDS_HELPFILEChangeCasePP;
		m_dwDocStringID = IDS_DOCSTRINGChangeCasePP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07713")
}
//-------------------------------------------------------------------------------------------------
CChangeCasePP::~CChangeCasePP() 
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16355");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CChangeCasePP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CChangeCasePP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEMODIFIERSLib::IChangeCasePtr ipChangeCase = m_ppUnk[i];

			// Get case type
			EChangeCaseType eCaseType = kNoChangeCase;
			if (IsDlgButtonChecked(IDC_RADIO_UPPERCASE))
			{
				eCaseType = kMakeUpperCase;
			}
			else if (IsDlgButtonChecked(IDC_RADIO_LOWERCASE))
			{
				eCaseType = kMakeLowerCase;
			}
			else if (IsDlgButtonChecked(IDC_RADIO_TITLECASE))
			{
				eCaseType = kMakeTitleCase;
			}

			// Save the setting
			ipChangeCase->CaseType = (UCLID_AFVALUEMODIFIERSLib::EChangeCaseType)eCaseType;
		}
		
		// Clear flag
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06391");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CChangeCasePP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CChangeCasePP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFVALUEMODIFIERSLib::IChangeCasePtr ipChangeCase(m_ppUnk[0]);
		if (ipChangeCase)
		{
			// Retrieve Case setting
			EChangeCaseType eCaseType = (EChangeCaseType)ipChangeCase->CaseType;

			// Set appropriate radio button
			switch (eCaseType)
			{
			case kNoChangeCase:
				CheckDlgButton(IDC_RADIO_NOCHANGE, BST_CHECKED);
				break;
			case kMakeUpperCase:
				CheckDlgButton(IDC_RADIO_UPPERCASE, BST_CHECKED);
				break;
			case kMakeLowerCase:
				CheckDlgButton(IDC_RADIO_LOWERCASE, BST_CHECKED);
				break;
			case kMakeTitleCase:
				CheckDlgButton(IDC_RADIO_TITLECASE, BST_CHECKED);
				break;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06392");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CChangeCasePP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07687", 
		"ChangeCase Modifier PP" );
}
//-------------------------------------------------------------------------------------------------
