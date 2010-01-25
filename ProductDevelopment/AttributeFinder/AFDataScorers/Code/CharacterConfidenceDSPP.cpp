// CharacterConfindenceDSPP.cpp : Implementation of CCharacterConfindenceDSPP

#include "stdafx.h"
#include "CharacterConfidenceDSPP.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <Comutils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CCharacterConfindenceDSPP
//-------------------------------------------------------------------------------------------------
CCharacterConfidenceDSPP::CCharacterConfidenceDSPP()
{
	m_dwTitleID = IDS_TITLECHARACTERCONFIDENCEDSPP;
	m_dwHelpFileID = IDS_HELPFILECHARACTERCONFIDENCEDSPP;
	m_dwDocStringID = IDS_DOCSTRINGCHARACTERCONFIDENCEDSPP;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDSPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ATLTRACE(_T("CCharacterConfidenceDSPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFDATASCORERSLib::ICharacterConfidenceDSPtr ipCCDS = m_ppUnk[i];
			if (ipCCDS != NULL)
			{
				// Save the new value for the aggregate function
				ipCCDS->AggregateFunction = 
					(UCLID_AFDATASCORERSLib::EAggregateFunctions) m_comboAggregateFunction.GetCurSel();
			}
		}
		m_bDirty = FALSE;
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29328");
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CCharacterConfidenceDSPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
											  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		UCLID_AFDATASCORERSLib::ICharacterConfidenceDSPtr ipCCDS = m_ppUnk[0];
		if (ipCCDS != NULL)
		{
			// Attach controls
			m_comboAggregateFunction.Attach(GetDlgItem(IDC_COMBO_AGGREGATE_FUNCTION));

			// Setup combo box
			m_comboAggregateFunction.InsertString(kAverage, "average");
			m_comboAggregateFunction.InsertString(kMinimum, "minimum");
			m_comboAggregateFunction.InsertString(kMaximum, "maximum");

			// Initialize the combo box value
			m_comboAggregateFunction.SetCurSel((long) ipCCDS->AggregateFunction);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29329");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// ILicensed Component Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDSPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

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
void CCharacterConfidenceDSPP::validateLicense()
{
	static const unsigned long CHARACTER_CONFIDENCE_DATA_SCORER_PP_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( CHARACTER_CONFIDENCE_DATA_SCORER_PP_ID, "ELI29330", 
		"Character Confidence Data Scorer Property Page" );
}
//-------------------------------------------------------------------------------------------------
