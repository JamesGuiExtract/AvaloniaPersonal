// MathematicalConditionPP.cpp : Implementation of CMathematicalConditionPP
#include "stdafx.h"
#include "ESSkipConditions.h"
#include "MathematicalConditionPP.h"

#include <ComponentLicenseIDs.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const int gnCONSIDER_MET = 0;
static const int gnCONSIDER_NOT_MET = 1;

//-------------------------------------------------------------------------------------------------
// CMathematicalConditionPP
//-------------------------------------------------------------------------------------------------
CMathematicalConditionPP::CMathematicalConditionPP()
{
	try
	{
		// Check licensing
		validateLicense();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI27168")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalConditionPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI27169", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27170");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalConditionPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CMathematicalConditionPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			EXTRACT_FAMCONDITIONSLib::IMathematicalFAMConditionPtr ipMathematicalCondition(m_ppUnk[i]);
			if (ipMathematicalCondition)
			{
				IMathConditionCheckerPtr ipChecker = __nullptr;

				// Get the data (validating as we go)
				if (m_radioRandom.GetCheck() == BST_CHECKED)
				{
					CString zTemp;
					m_editRandomPercent.GetWindowText(zTemp);

					// Ensure a percent is specified
					if (zTemp.IsEmpty())
					{
						MessageBox("Must specify a percentage!",
							"Blank Percentage", MB_OK | MB_ICONERROR);
						m_editRandomPercent.SetFocus();
						return S_FALSE;
					}

					// No need to handle exception on asLong since the edit box is restricted
					// to numbers (and 2 characters at max)
					long nVal = asLong((LPCTSTR)zTemp);

					if (nVal < 1 || nVal > 99)
					{
						MessageBox("Invalid percentage: Must be between 1 and 99 (inclusive)!",
							"Invalid Percentage", MB_OK | MB_ICONERROR);
						m_editRandomPercent.SetSel(0, -1, TRUE);
						m_editRandomPercent.SetFocus();
						return S_FALSE;
					}

					// Create a new condition and set the percentage
					IRandomMathConditionPtr ipRandom(CLSID_RandomMathCondition);
					ASSERT_RESOURCE_ALLOCATION("ELI27171", ipRandom != __nullptr);
					ipRandom->Percent = nVal;

					// Set the checker condition object
					ipChecker = ipRandom;
				}
				else if(m_radioOnceEvery.GetCheck() == BST_CHECKED)
				{
					CString zTemp;
					m_editOnceEvery.GetWindowText(zTemp);

					// Ensure a count is specified
					if (zTemp.IsEmpty())
					{
						MessageBox("Must specify a count!",
							"Blank Count", MB_OK | MB_ICONERROR);
						m_editOnceEvery.SetFocus();
						return S_FALSE;
					}

					// No need to handle exception on asLong since the edit box is
					// restricted to numbers
					long nVal = asLong((LPCTSTR)zTemp);

					if (nVal < 2)
					{
						MessageBox("Invalid count: Must be greater than 1!",
							"Invalid Count", MB_OK | MB_ICONERROR);
						m_editOnceEvery.SetSel(0, -1, TRUE);
						m_editOnceEvery.SetFocus();
						return S_FALSE;
					}

					// Need to generate a GUID for this object
					GUID gGuid;
					HRESULT hr = CoCreateGuid(&gGuid);
					if (hr != S_OK)
					{
						UCLIDException ue("ELI27172", "Failed to generate unique identifier!");
						ue.addHresult(hr);
						throw ue;
					}

					// Get the GUID as a string
					string strGuid = asString(gGuid);

					// Create a new condition and set the count and unique ID
					IOnceEveryMathConditionPtr ipOnce(CLSID_OnceEveryMathCondition);
					ASSERT_RESOURCE_ALLOCATION("ELI27173", ipOnce != __nullptr);
					ipOnce->NumberOfTimes = nVal;
					ipOnce->UsageID = strGuid.c_str();

					// Set the checker condition object
					ipChecker = ipOnce;
				}
				else if (m_radioModulus.GetCheck() == BST_CHECKED)
				{
					// Get the modulus
					CString zTemp;
					m_editModulus.GetWindowText(zTemp);

					// Ensure a modulus is specified
					if (zTemp.IsEmpty())
					{
						MessageBox("Must specify a modulus!",
							"Blank Modulus", MB_OK | MB_ICONERROR);
						m_editModulus.SetFocus();
						return S_FALSE;
					}

					// No need to handle the exception on asLong since the cell is
					// restricted to numbers
					long nMod = asLong((LPCTSTR) zTemp);
					if (nMod < 2)
					{
						MessageBox("Invalid modulus: Must be greater than 1!",
							"Invalid Modulus", MB_OK | MB_ICONERROR);
						m_editModulus.SetSel(0, -1, TRUE);
						m_editModulus.SetFocus();
						return S_FALSE;
					}

					// Clear the temp string
					zTemp.Empty();

					// Get the mod equals value
					m_editModEquals.GetWindowText(zTemp);

					// Ensure a mod equals is specified
					if (zTemp.IsEmpty())
					{
						MessageBox("Must specify a value for modulus equals!",
							"Blank Modulus Equals", MB_OK | MB_ICONERROR);
						m_editModEquals.SetFocus();
						return S_FALSE;
					}

					// No need to handle the exception on asLong since the cell is
					// restricted to numbers
					long nModEquals = asLong((LPCTSTR) zTemp);

					if (nModEquals >= nMod || nModEquals < 0)
					{
						MessageBox("Invalid equals value: Must be greater than 0 and less than Modulus",
							"Invalid Equals", MB_OK | MB_ICONERROR);
						m_editModEquals.SetSel(0, -1, TRUE);
						m_editModEquals.SetFocus();
						return S_FALSE;
					}

					// Create a new condition and set the modulus and equals values
					IModulusEqualsMathConditionPtr ipMod(CLSID_ModulusEqualsMathCondition);
					ASSERT_RESOURCE_ALLOCATION("ELI27174", ipMod != __nullptr);
					ipMod->Modulus = nMod;
					ipMod->ModEquals = nModEquals;

					// Set the checker condition object
					ipChecker = ipMod;
				}
				else
				{
					// Should never get here
					THROW_LOGIC_ERROR_EXCEPTION("ELI27175");
				}

				// Ensure the checker object has been set
				ASSERT_RESOURCE_ALLOCATION("ELI27176", ipChecker != __nullptr);

				// Set the math condition object
				ipMathematicalCondition->MathematicalCondition = ipChecker;
				ipMathematicalCondition->ConsiderMet =
					asVariantBool(m_cmbConsiderCondition.GetCurSel() == gnCONSIDER_MET);
			}
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27177")

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CMathematicalConditionPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		EXTRACT_FAMCONDITIONSLib::IMathematicalFAMConditionPtr ipCondition = m_ppUnk[0];
		if (ipCondition)
		{
			// Get the controls from dialog
			m_cmbConsiderCondition = GetDlgItem(IDC_MATH_COMBO_MET);
			m_radioRandom = GetDlgItem(IDC_MATH_RADIO_RANDOM);
			m_radioOnceEvery = GetDlgItem(IDC_MATH_RADIO_ONCE_EVERY);
			m_radioModulus = GetDlgItem(IDC_MATH_RADIO_MODULUS);
			m_editRandomPercent = GetDlgItem(IDC_MATH_EDIT_RANDOM_PERCENT);
			m_editOnceEvery = GetDlgItem(IDC_MATH_EDIT_EVERY);
			m_editModulus = GetDlgItem(IDC_MATH_EDIT_MODULUS);
			m_editModEquals = GetDlgItem(IDC_MATH_EDIT_MOD_EQUALS);

			// Insert "met" and "not met" into combobox
			m_cmbConsiderCondition.InsertString(gnCONSIDER_MET, _bstr_t("met"));
			m_cmbConsiderCondition.InsertString(gnCONSIDER_NOT_MET, _bstr_t("not met"));

			m_cmbConsiderCondition.SetCurSel(ipCondition->ConsiderMet == VARIANT_TRUE ?
				gnCONSIDER_MET : gnCONSIDER_NOT_MET);

			// Default the radio buttons to Randomly
			bool bRandom = true;
			bool bEvery = false;
			bool bModulus = false;

			// Get the mathematical condition object
			IUnknownPtr ipChecker = ipCondition->MathematicalCondition;

			// If it is not NULL, then check for which condition object it is
			// and update the UI accordingly
			if (ipChecker != __nullptr)
			{
				// Attempt to get the checker as each type of checker object
				IRandomMathConditionPtr ipRandom = ipChecker;
				IOnceEveryMathConditionPtr ipOnce = ipChecker;
				IModulusEqualsMathConditionPtr ipMod = ipChecker;
				if (ipRandom != __nullptr)
				{
					// Bools are already defaulted to this case, no need to change them

					// Set the edit box from the object
					m_editRandomPercent.SetWindowText(asString(ipRandom->Percent).c_str());
				}
				else if (ipOnce != __nullptr)
				{
					// Update bools
					bRandom = false;
					bEvery = true;

					// Set the edit box from the object
					m_editOnceEvery.SetWindowText(asString(ipOnce->NumberOfTimes).c_str());
				}
				else if (ipMod != __nullptr)
				{
					// Update bools
					bRandom = false;
					bModulus = true;

					// Set the edit boxes from this object
					m_editModulus.SetWindowText(asString(ipMod->Modulus).c_str());
					m_editModEquals.SetWindowText(asString(ipMod->ModEquals).c_str());
				}
				else
				{
					// Should never get here
					THROW_LOGIC_ERROR_EXCEPTION("ELI27251");
				}
			}

			// Set the initial checked state of the radio buttons
			m_radioRandom.SetCheck(asBSTChecked(bRandom));
			m_radioOnceEvery.SetCheck(asBSTChecked(bEvery));
			m_radioModulus.SetCheck(asBSTChecked(bModulus));
				
			// Limit the amount of text in the percentage edit box to 2 characters
			m_editRandomPercent.SetLimitText(2);

			updateControls();
		}
		
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27178");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMathematicalConditionPP::OnBnClickedRadio(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get which control was checked
		bool bRandom = hWndCtl == m_radioRandom.m_hWnd;
		bool bOnce = !bRandom && hWndCtl == m_radioOnceEvery.m_hWnd;
		bool bMod = !bRandom && !bOnce;

		// Update the control status based on which item was checked
		m_radioRandom.SetCheck(asBSTChecked(bRandom));
		m_radioOnceEvery.SetCheck(asBSTChecked(bOnce));
		m_radioModulus.SetCheck(asBSTChecked(bMod));

		// Update the other controls
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27179");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CMathematicalConditionPP::updateControls()
{
	// Set BOOLs based on radio button status
	BOOL bRandom = asMFCBool(m_radioRandom.GetCheck() == BST_CHECKED);
	BOOL bOnceEvery = asMFCBool(m_radioOnceEvery.GetCheck() == BST_CHECKED);
	BOOL bModulus = asMFCBool(m_radioModulus.GetCheck() == BST_CHECKED);

	// Enable/disable controls based on radio buttons
	m_editRandomPercent.EnableWindow(bRandom);
	m_editOnceEvery.EnableWindow(bOnceEvery);
	m_editModulus.EnableWindow(bModulus);
	m_editModEquals.EnableWindow(bModulus);
}
//-------------------------------------------------------------------------------------------------
void CMathematicalConditionPP::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI27180", "Mathematical FAM Condition PP");
}
//-------------------------------------------------------------------------------------------------
