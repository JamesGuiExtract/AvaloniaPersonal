// CharacterConfidenceConditionPP.cpp : Implementation of CCharacterConfidenceConditionPP

#include "stdafx.h"
#include "CharacterConfidenceConditionPP.h"

#include <AFCategories.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CCharacterConfidenceConditionPP
//-------------------------------------------------------------------------------------------------
CCharacterConfidenceConditionPP::CCharacterConfidenceConditionPP()
{
	try
	{
		m_dwTitleID = IDS_TITLECHARACTERCONFIDENCECONDITIONPP;
		m_dwHelpFileID = IDS_HELPFILECHARACTERCONFIDENCECONDITIONPP;
		m_dwDocStringID = IDS_DOCSTRINGCHARACTERCONFIDENCECONDITIONPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI29441");
}
//-------------------------------------------------------------------------------------------------
HRESULT CCharacterConfidenceConditionPP::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CCharacterConfidenceConditionPP::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CCharacterConfidenceConditionPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Obtain interface pointer to the IFindingRuleCondition class
		UCLID_AFCONDITIONSLib::ICharacterConfidenceConditionPtr ipCharacterConfidencCondition = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI29376", ipCharacterConfidencCondition != __nullptr);

		// Attach controls
		m_comboAggregateFunction.Attach(GetDlgItem(IDC_COMBO_AGGREGATE_FUNCTION));
		m_comboFirstCondition.Attach(GetDlgItem(IDC_COMBO_FIRST_CONDITION));
		m_editFirstScore.Attach(GetDlgItem(IDC_EDIT_FIRST_SCORE));
		m_checkSecondCondition.Attach(GetDlgItem(IDC_CHECK_SECOND_CONDITION));
		m_comboAndOr.Attach(GetDlgItem(IDC_COMBO_AND_OR));
		m_comboSecondCondition.Attach(GetDlgItem(IDC_COMBO_SECOND_CONDITION));
		m_editSecondScore.Attach(GetDlgItem(IDC_EDIT_SECOND_SCORE));
		m_staticIs.Attach(GetDlgItem(IDC_STATIC_IS));
		m_comboMet.Attach(GetDlgItem(IDC_COMBO_MET));

		// Initialize met/not met combo 
		m_comboMet.InsertString(0, "met");
		m_comboMet.InsertString(1, "not met");

		// Setup combo box
		m_comboAggregateFunction.InsertString(kAverage, "average");
		m_comboAggregateFunction.InsertString(kMinimum, "minimum");
		m_comboAggregateFunction.InsertString(kMaximum, "maximum");

		// Initialize the And/or combo
		m_comboAndOr.AddString("and");
		m_comboAndOr.AddString("or");

		// Initialize first condition combo
		m_comboFirstCondition.InsertString(kEQ, "=");
		m_comboFirstCondition.InsertString(kNEQ, "!=");
		m_comboFirstCondition.InsertString(kLT, "<");
		m_comboFirstCondition.InsertString(kGT, ">");
		m_comboFirstCondition.InsertString(kLEQ, "<=");
		m_comboFirstCondition.InsertString(kGEQ, ">=");

		// Initialize second condition combo
		m_comboSecondCondition.InsertString(kEQ, "=");
		m_comboSecondCondition.InsertString(kNEQ, "!=");
		m_comboSecondCondition.InsertString(kLT, "<");
		m_comboSecondCondition.InsertString(kGT, ">");
		m_comboSecondCondition.InsertString(kLEQ, "<=");
		m_comboSecondCondition.InsertString(kGEQ, ">=");

		// Initialize the values
		m_comboAggregateFunction.SetCurSel((long) ipCharacterConfidencCondition->AggregateFunction);
		m_comboFirstCondition.SetCurSel((long)ipCharacterConfidencCondition->FirstScoreCondition);
		m_editFirstScore.SetWindowText(asString(ipCharacterConfidencCondition->FirstScoreToCompare).c_str());
		m_checkSecondCondition.SetCheck(asBSTChecked(ipCharacterConfidencCondition->IsSecondCondition));
		m_editSecondScore.SetWindowText(asString(ipCharacterConfidencCondition->SecondScoreToCompare).c_str());

		// Determine the selection for the comboAndOr control
		if (asCppBool(ipCharacterConfidencCondition->AndSecondCondition))
		{
			m_comboAndOr.SelectString(-1, "and");
		}
		else
		{
			m_comboAndOr.SelectString(-1, "or");
		}

		m_comboSecondCondition.SetCurSel((long)ipCharacterConfidencCondition->SecondScoreCondition);

		// Determine the selection for the met/not met combo
		if (asCppBool(ipCharacterConfidencCondition->IsMet))
		{
			m_comboMet.SelectString(-1, "met");
		}
		else
		{
			m_comboMet.SelectString(-1, "not met");
		}

		// Update controls based on the property data
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29377");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CCharacterConfidenceConditionPP::OnBnClickedCheckSecondCondition(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE( AfxGetModuleState());

	try
	{
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29435");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceConditionPP::Apply(void)
{
	try
	{
		ATLTRACE(_T("CCharacterConfidenceConditionPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFCONDITIONSLib::ICharacterConfidenceConditionPtr ipCCC = m_ppUnk[i];
			if (ipCCC != __nullptr)
			{
				// Save the first score condition
				ipCCC->FirstScoreCondition = (EConditionalOp) m_comboFirstCondition.GetCurSel();
				
				// Temp variable used for getting the scores
				CString zTemp;

				// Validate the first score to compare
				m_editFirstScore.GetWindowTextA( zTemp );
				long lValue;
				try
				{
					lValue = asLong((LPCSTR) zTemp);
					if ( lValue < 0 || lValue > 100)
					{
						UCLIDException ue("ELI29436", "First value to compare must be > 0 and < 100.");
						throw ue;
					}
					ipCCC->FirstScoreToCompare = lValue;
				}
				catch(...)
				{
					// Set focus to the edit box that is in a bad state
					m_editFirstScore.SetFocus();
					throw;
				}

				// Check if there is a second condition
				if (m_checkSecondCondition.GetCheck() == BST_CHECKED)
				{
					// Update the IsSecondCondition flag
					ipCCC->IsSecondCondition = VARIANT_TRUE;

					// Get the text at the current selection
					m_comboAndOr.GetLBText(m_comboAndOr.GetCurSel(), zTemp); 

					// If 'and' is selected set the flag
					ipCCC->AndSecondCondition = asVariantBool(zTemp == "and");

					// Save the second scores condition
					ipCCC->SecondScoreCondition = (EConditionalOp) m_comboSecondCondition.GetCurSel();

					// Validate the second score to compare
					m_editSecondScore.GetWindowTextA( zTemp );
					long lValue;
					try
					{
						lValue = asLong((LPCSTR) zTemp);
						if ( lValue < 0 || lValue > 100)
						{
							UCLIDException ue("ELI29437", "Second value to compare must be > 0 and < 100.");
							throw ue;
						}
						ipCCC->SecondScoreToCompare = lValue;
					}
					catch(...)
					{
						// Set focus to the edit box that is in a bad state
						m_editSecondScore.SetFocus();
						throw;
					}
				}
				else
				{
					// Set the second condtion to the default state
					ipCCC->IsSecondCondition = VARIANT_FALSE;
					ipCCC->SecondScoreToCompare = 0;
					ipCCC->AndSecondCondition = VARIANT_TRUE;
					ipCCC->SecondScoreCondition = kEQ;
				}
				
				// Save the new value for the aggregate function
				ipCCC->AggregateFunction = 
					(EAggregateFunctions) m_comboAggregateFunction.GetCurSel();

				// Get the text at the current selection
				m_comboMet.GetLBText(m_comboMet.GetCurSel(), zTemp); 

				// If 'and' is selected set the flag
				ipCCC->IsMet = asVariantBool(zTemp == "met");
			}
		}

		m_bDirty = FALSE;
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29381");

	// If we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceConditionPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI29378", pbValue != __nullptr);

		try
		{
			// check the license
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29379");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CCharacterConfidenceConditionPP::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI29380", "Character Confidence Condition PP");
}
//-------------------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------------------
void CCharacterConfidenceConditionPP::updateControls()
{
	// If there is no second condition then those controls need to be disabbled
	if (m_checkSecondCondition.GetCheck() == BST_CHECKED)
	{
		m_comboAndOr.EnableWindow(TRUE);
		m_comboSecondCondition.EnableWindow(TRUE);
		m_editSecondScore.EnableWindow(TRUE);
		m_staticIs.EnableWindow(TRUE);
	}
	else
	{
		m_comboAndOr.EnableWindow(FALSE);
		m_comboSecondCondition.EnableWindow(FALSE);
		m_editSecondScore.EnableWindow(FALSE);
		m_staticIs.EnableWindow(FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
