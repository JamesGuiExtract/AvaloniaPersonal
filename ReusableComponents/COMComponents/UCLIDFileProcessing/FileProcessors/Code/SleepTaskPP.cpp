// SleepTaskPP.cpp : Implementation of CSleepTaskPP

#include "stdafx.h"
#include "SleepTaskPP.h"

#include <FPCategories.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <COMUtils.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// CSleepTaskPP
//-------------------------------------------------------------------------------------------------
CSleepTaskPP::CSleepTaskPP()
{
	try
	{
		// Check licensing
		validateLicense();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI29609")
}
//--------------------------------------------------------------------------------------------------
CSleepTaskPP::~CSleepTaskPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29610")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTaskPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI29611", pbValue != NULL);

		try
		{
			// Check license
			validateLicense();

			// If no exception was thrown, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29612");
}

//--------------------------------------------------------------------------------------------------
// IPropertyPage
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTaskPP::Apply()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ATLTRACE(_T("CSleepTaskPP::Apply\n"));

		// Update the settings in each of the objects associated with this UI
		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Get the SleepTask associated with this property page
			// NOTE: this assumes only one coclass is associated with this property page
			UCLID_FILEPROCESSORSLib::ISleepTaskPtr ipSleep(m_ppUnk[0]);
			ASSERT_RESOURCE_ALLOCATION("ELI29613", ipSleep != NULL);

			CString zText;
			m_editSleepTime.GetWindowText(zText);
			unsigned long ulSleepTime = asUnsignedLong((LPCTSTR)zText);
			if (ulSleepTime <= 0 || ulSleepTime > LONG_MAX)
			{
				string strMessage = "Sleep time must be > 0 and <= "
					+ asString(LONG_MAX);
				MessageBox(strMessage.c_str(), "Invalid Time", MB_OK | MB_ICONERROR);

				m_editSleepTime.SetSel(0, -1);
				m_editSleepTime.SetFocus();
				return S_FALSE;
			}

			// Store the settings
			ipSleep->SleepTime = (long) ulSleepTime;
			ipSleep->TimeUnits =
				(UCLID_FILEPROCESSORSLib::ESleepTimeUnitType) (m_comboUnits.GetCurSel());
			ipSleep->Random = asVariantBool(m_checkRandom.GetCheck() == BST_CHECKED);
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29614")

	// An exception was caught
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// Message handlers
//--------------------------------------------------------------------------------------------------
LRESULT CSleepTaskPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Get the SleepTask associated with this property page
		// NOTE: this assumes only one coclass is associated with this property page
		UCLID_FILEPROCESSORSLib::ISleepTaskPtr ipSleep(m_ppUnk[0]);
		ASSERT_RESOURCE_ALLOCATION("ELI29615", ipSleep != NULL);

		// Prepare controls
		prepareControls();

		long lSleepTime = ipSleep->SleepTime;
		if (lSleepTime > 0)
		{
			m_editSleepTime.SetWindowText(asString(ipSleep->SleepTime).c_str());
		}

		int lTemp = (int) ipSleep->TimeUnits;
		m_comboUnits.SetCurSel(lTemp);

		m_checkRandom.SetCheck(asBSTChecked(ipSleep->Random));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29616")

	return TRUE;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CSleepTaskPP::prepareControls()
{
	// Get radio buttons
	m_editSleepTime = GetDlgItem(IDC_EDIT_SLEEP_TIME);
	m_comboUnits = GetDlgItem(IDC_COMBO_SLEEP_UNITS);
	m_checkRandom = GetDlgItem(IDC_CHECK_SLEEP_RANDOM);

	// Limit the number of characters in the edit control to 10
	m_editSleepTime.SetLimitText(10);

	// Populate the combo box with the units
	m_comboUnits.AddString("Millisecond(s)");
	m_comboUnits.AddString("Second(s)");
	m_comboUnits.AddString("Minute(s)");
	m_comboUnits.AddString("Hour(s)");
}
//-------------------------------------------------------------------------------------------------
void CSleepTaskPP::validateLicense()
{
	VALIDATE_LICENSE( gnFILE_ACTION_MANAGER_OBJECTS, "ELI29617", 
		"Sleep Task PP" );
}
//-------------------------------------------------------------------------------------------------