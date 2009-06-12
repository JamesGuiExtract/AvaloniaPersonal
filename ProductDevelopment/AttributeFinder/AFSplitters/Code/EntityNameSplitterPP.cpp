// EntityNameSplitterPP.cpp : Implementation of CEntityNameSplitterPP
#include "stdafx.h"
#include "AFSplitters.h"
#include "EntityNameSplitterPP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CEntityNameSplitterPP
//-------------------------------------------------------------------------------------------------
CEntityNameSplitterPP::CEntityNameSplitterPP() : 
  m_eAliasChoice(kIgnoreLaterEntities)
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEEntityNameSplitterPP;
		m_dwHelpFileID = IDS_HELPFILEEntityNameSplitterPP;
		m_dwDocStringID = IDS_DOCSTRINGEntityNameSplitterPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI08676")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitterPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitterPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CEntityNameSplitterPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFSPLITTERSLib::IEntityNameSplitterPtr ipSplitter(m_ppUnk[i]);
			if (ipSplitter)
			{
				// Set the alias choice
				ipSplitter->EntityAliasChoice = 
					(UCLID_AFSPLITTERSLib::EEntityAliasChoice) m_eAliasChoice;
			}
		}

		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08677");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CEntityNameSplitterPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFSPLITTERSLib::IEntityNameSplitterPtr ipSplitter(m_ppUnk[0]);
		if (ipSplitter)
		{
			// Retrieve and apply Alias Choice setting
			EEntityAliasChoice	eChoice = (EEntityAliasChoice) ipSplitter->EntityAliasChoice;

			// Select radio button
			int nID = 0;
			switch (eChoice)
			{
			case kIgnoreLaterEntities:
				nID = IDC_RADIO_IGNORE;
				break;

			case kLaterEntitiesAsAttributes:
				nID = IDC_RADIO_ATTRIBUTES;
				break;

			case kLaterEntitiesAsSubattributes:
				nID = IDC_RADIO_SUBATTRIBUTES;
				break;
			}

			if (nID != 0)
			{
				ATLControls::CButton radioChoice( GetDlgItem( nID ) );
				radioChoice.SetCheck( 1 );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08678");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CEntityNameSplitterPP::OnClickedRadioIgnoreAlias(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_eAliasChoice = (EEntityAliasChoice) UCLID_AFSPLITTERSLib::kIgnoreLaterEntities;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08679");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CEntityNameSplitterPP::OnClickedRadioAliasToAttribute(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_eAliasChoice = (EEntityAliasChoice) UCLID_AFSPLITTERSLib::kLaterEntitiesAsAttributes;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08680");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CEntityNameSplitterPP::OnClickedRadioAliasToSubAttribute(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_eAliasChoice = (EEntityAliasChoice) UCLID_AFSPLITTERSLib::kLaterEntitiesAsSubattributes;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08681");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitterPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI19159", "EntityNameSplitter PP" );
}
//-------------------------------------------------------------------------------------------------
