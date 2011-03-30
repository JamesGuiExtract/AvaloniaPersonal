// DataScorerBasedASPP.cpp : Implementation of CDataScorerBasedASPP

#include "stdafx.h"
#include "DataScorerBasedASPP.h"


#include <AFCategories.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <Comutils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CDataScorerBasedASPP
//-------------------------------------------------------------------------------------------------
CDataScorerBasedASPP::CDataScorerBasedASPP()
{
	try
	{
		m_dwTitleID = IDS_TITLEDATASCORERBASEDASPP;
		m_dwHelpFileID = IDS_HELPFILEDATASCORERBASEDASPP;
		m_dwDocStringID = IDS_DOCSTRINGDATASCORERBASEDASPP;

		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI29301", m_ipMiscUtils != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI29302");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedASPP::Apply(void)
{
	try
	{
		ATLTRACE(_T("CDataScorerBasedASPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFSELECTORSLib::IDataScorerBasedASPtr ipDSAS = m_ppUnk[i];
			if ( ipDSAS != __nullptr )
			{
				// Save the data scorer
				ipDSAS->DataScorer = m_ipSelectedDataScorer;
				
				// Save the first score condition
				ipDSAS->FirstScoreCondition = (EConditionalOp) m_comboFirstCondition.GetCurSel();
				
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
						UCLIDException ue("ELI29298", "First score to compare must be > 0 and < 100.");
						throw ue;
					}
					ipDSAS->FirstScoreToCompare = lValue;
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
					ipDSAS->IsSecondCondition = VARIANT_TRUE;

					// Get the text at the current selection
					m_comboAndOr.GetLBText(m_comboAndOr.GetCurSel(), zTemp); 

					// If 'and' is selected set the flag
					ipDSAS->AndSecondCondition = asVariantBool(zTemp == "and");

					// Save the second scores condition
					ipDSAS->SecondScoreCondition = (EConditionalOp) m_comboSecondCondition.GetCurSel();

					// Validate the second score to compare
					m_editSecondScore.GetWindowTextA( zTemp );
					long lValue;
					try
					{
						lValue = asLong((LPCSTR) zTemp);
						if ( lValue < 0 || lValue > 100)
						{
							UCLIDException ue("ELI29299", "Second score to compare must be > 0 and < 100.");
							throw ue;
						}
						ipDSAS->SecondScoreToCompare = lValue;
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
					ipDSAS->IsSecondCondition = VARIANT_FALSE;
					ipDSAS->SecondScoreToCompare = 0;
					ipDSAS->AndSecondCondition = VARIANT_TRUE;
					ipDSAS->SecondScoreCondition = kEQ;
				}
			}
		}
		m_bDirty = FALSE;
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29264");
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CDataScorerBasedASPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
											  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		UCLID_AFSELECTORSLib::IDataScorerBasedASPtr ipDSAS = m_ppUnk[0];
		if (ipDSAS != __nullptr)
		{
			// Attach the controls to the member variables
			m_comboFirstCondition.Attach(GetDlgItem(IDC_COMBO_FIRST_CONDITION));
			m_editFirstScore.Attach(GetDlgItem(IDC_EDIT_FIRST_SCORE));
			m_checkSecondCondition.Attach(GetDlgItem(IDC_CHECK_SECOND_CONDITION));
			m_comboAndOr.Attach(GetDlgItem(IDC_COMBO_AND_OR));
			m_comboSecondCondition.Attach(GetDlgItem(IDC_COMBO_SECOND_CONDITION));
			m_editSecondScore.Attach(GetDlgItem(IDC_EDIT_SECOND_SCORE));
			m_staticDataScorer.Attach(GetDlgItem(IDC_STATIC_DATA_SCORER));
			m_staticIs.Attach(GetDlgItem(IDC_STATIC_IS));
			m_buttonCommand.Attach(GetDlgItem(IDC_BUTTON_COMMANDS_DATA_SCORER));

			// Initialize the And or combo
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

			// Set data from object
			m_ipSelectedDataScorer = ipDSAS->DataScorer;
			ASSERT_RESOURCE_ALLOCATION("ELI29297", m_ipSelectedDataScorer != __nullptr);

			// Update controls to contain properties data
			m_staticDataScorer.SetWindowText(m_ipSelectedDataScorer->Description);
			m_comboFirstCondition.SetCurSel((long)ipDSAS->FirstScoreCondition);
			m_editFirstScore.SetWindowText(asString(ipDSAS->FirstScoreToCompare).c_str());
			m_checkSecondCondition.SetCheck(asBSTChecked(ipDSAS->IsSecondCondition));
			m_editSecondScore.SetWindowText(asString(ipDSAS->SecondScoreToCompare).c_str());

			// Determine the selection for the comboAndOr control
			if (asCppBool(ipDSAS->AndSecondCondition))
			{
				m_comboAndOr.SelectString(-1, "and");
			}
			else
			{
				m_comboAndOr.SelectString(-1, "or");
			}

			m_comboSecondCondition.SetCurSel((long)ipDSAS->SecondScoreCondition);
			
			// Update controls based on the property data
			updateControls();
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29260");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDataScorerBasedASPP::OnBnClickedCheckSecondCondition(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE( AfxGetModuleState());

	try
	{
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29263");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDataScorerBasedASPP::OnBnClickedButtonCommandsDataScorer(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE( AfxGetModuleState());

	try
	{	
		// get the position of the handler button
		RECT rect;
		m_buttonCommand.GetWindowRect(&rect);

		IObjectWithDescriptionPtr ipTmp = m_ipSelectedDataScorer;
		// prompt user to select and configure 
		VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectCommandButtonClick(
			ipTmp, "Data Scorer" , 
			get_bstr_t(AFAPI_DATA_SCORERS_CATEGORYNAME), VARIANT_TRUE, 0, NULL, rect.right, rect.top);

		//// check if output handler has been modified
		if (vbDirty == VARIANT_TRUE)
		{
			m_ipSelectedDataScorer = ipTmp;
			m_staticDataScorer.SetWindowTextA(m_ipSelectedDataScorer->Description);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29331")
	return 0;
}

//-------------------------------------------------------------------------------------------------
// ILicensed Component Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedASPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CDataScorerBasedASPP::validateLicense()
{
	static const unsigned long DATA_SCORER_BASED_ASPP_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( DATA_SCORER_BASED_ASPP_ID, "ELI29261", 
		"Data Scorer Based Attribute Selector Property Page" );
}
//-------------------------------------------------------------------------------------------------
void CDataScorerBasedASPP::updateControls()
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
