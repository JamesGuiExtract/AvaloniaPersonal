// DateTimeSplitterPP.cpp : Implementation of CDateTimeSplitterPP
#include "stdafx.h"
#include "AFSplitters.h"
#include "DateTimeSplitterPP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <comutils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CDateTimeSplitterPP
//-------------------------------------------------------------------------------------------------
CDateTimeSplitterPP::CDateTimeSplitterPP() 
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEDateTimeSplitterPP;
		m_dwHelpFileID = IDS_HELPFILEDateTimeSplitterPP;
		m_dwDocStringID = IDS_DOCSTRINGDateTimeSplitterPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI09743")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateTimeSplitterPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CDateTimeSplitterPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CDateTimeSplitterPP::Apply\n"));

		// First confirm that some sub-attributes will be created
		if ((m_btnSplitDefaults.GetCheck() == 0) && 
			(m_btnShowFormatted.GetCheck() == 0) && 
			(m_btnDayOfWeek.GetCheck() == 0))
		{
			MessageBox( "At least one checkbox must be checked.", "Error", MB_OK );

			return S_FALSE;
		}

		// Next validate the Format string, if necessary
		if (m_btnShowFormatted.GetCheck() == 1)
		{
			string strOutput = formatCurrentTime();

			// If result is empty, Format string is empty or not valid
			if (strOutput.length() == 0)
			{
				CComBSTR bstrFormat;
				m_editFormat.GetWindowText( bstrFormat.m_str );
				string strFormat = asString( bstrFormat.m_str );

				if (strFormat.length() == 0)
				{
					MessageBox( "Format text is empty.", "Error", MB_OK );
				}
				else
				{
					MessageBox( "Format text is not valid.", "Error", MB_OK );
				}

				return S_FALSE;
			}
		}

		// Validate two digit year if necessary
		if (m_btnTwoDigitYearSpecified.GetCheck() == BST_CHECKED)
		{
			long lMinYear = getMinimumTwoDigitYear();
			if (lMinYear < 1000 || lMinYear > 9999)
			{
				MessageBox("Please enter a four digit year.", "Error", MB_OK);
				m_editMinimumTwoDigitYear.SetFocus();
				return S_FALSE;
			}
		}

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Update the underlying objects
			UCLID_AFSPLITTERSLib::IDateTimeSplitterPtr ipDTSplitter = m_ppUnk[i];
			if (ipDTSplitter)
			{
				// Month setting
				if (m_btnMonthName.GetCheck() == 1)
				{
					ipDTSplitter->SplitMonthAsName = VARIANT_TRUE;
				}
				else
				{
					ipDTSplitter->SplitMonthAsName = VARIANT_FALSE;
				}

				// Year setting
				if (m_btnYearFour.GetCheck() == 1)
				{
					ipDTSplitter->SplitFourDigitYear = VARIANT_TRUE;
				}
				else
				{
					ipDTSplitter->SplitFourDigitYear = VARIANT_FALSE;
				}

				// Time setting
				if (m_btnTimeMilitary.GetCheck() == 1)
				{
					ipDTSplitter->SplitMilitaryTime = VARIANT_TRUE;
				}
				else
				{
					ipDTSplitter->SplitMilitaryTime = VARIANT_FALSE;
				}

				// Default
				if (m_btnSplitDefaults.GetCheck() == 1)
				{
					ipDTSplitter->SplitDefaults = VARIANT_TRUE;
				}
				else
				{
					ipDTSplitter->SplitDefaults = VARIANT_FALSE;
				}

				// Day Of Week
				if (m_btnDayOfWeek.GetCheck() == 1)
				{
					ipDTSplitter->SplitDayOfWeek = VARIANT_TRUE;
				}
				else
				{
					ipDTSplitter->SplitDayOfWeek = VARIANT_FALSE;
				}

				// Formatted output
				if (m_btnShowFormatted.GetCheck() == 1)
				{
					// Set flag
					ipDTSplitter->ShowFormattedOutput = VARIANT_TRUE;

					// Retrieve format
					_bstr_t bstrFormat;
					m_editFormat.GetWindowText( bstrFormat.GetAddress() );

					// Set format
					ipDTSplitter->OutputFormat = bstrFormat;
				}
				else
				{
					// Clear flag
					ipDTSplitter->ShowFormattedOutput = VARIANT_FALSE;

					// Clear format
					ipDTSplitter->OutputFormat = "";
				}

				// Store two digit year settings
				if (m_btnTwoDigitYearSpecified.GetCheck() == BST_CHECKED)
				{
					ipDTSplitter->TwoDigitYearBeforeCurrent = VARIANT_FALSE;
					ipDTSplitter->MinimumTwoDigitYear = getMinimumTwoDigitYear();
				}
				else
				{
					ipDTSplitter->TwoDigitYearBeforeCurrent = VARIANT_TRUE;
				}
			}
		}

		m_bDirty = FALSE;

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09742");

	// if we reached here, it's because something went wrong
	// (such as one of the put methods throwing an exception because the
	// put data was invalid)
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CDateTimeSplitterPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Create tooltip object and set no delay.
		m_infoTip.Create(CWnd::FromHandle(m_hWnd));
		m_infoTip.SetShowDelay(0);

		// "Connect" the control classes to the actual controls
		m_btnMonthNumber = GetDlgItem( IDC_RADIO_MONTHNUMBER );
		m_btnMonthName = GetDlgItem( IDC_RADIO_MONTHNAME );
		m_btnYearFour = GetDlgItem( IDC_RADIO_YEARFOUR );
		m_btnYearTwo = GetDlgItem( IDC_RADIO_YEARTWO );
		m_btnTimeNormal = GetDlgItem( IDC_RADIO_TIMENORMAL );
		m_btnTimeMilitary = GetDlgItem( IDC_RADIO_TIMEMILITARY );
		m_btnDayOfWeek = GetDlgItem( IDC_CHECK_DAYOFWEEK );
		m_btnShowFormatted = GetDlgItem( IDC_CHECK_SHOWFORMATTED );
		m_btnSplitDefaults = GetDlgItem( IDC_CHECK_DEFAULTS );
		m_editFormat = GetDlgItem( IDC_EDIT_FORMAT );
		m_editOutput = GetDlgItem( IDC_EDIT_TEST );
		m_btnTwoDigitYearSpecified = GetDlgItem(IDC_RADIO_TWO_DIGIT_YEAR_SPECIFIED);
		m_btnTwoDigitYearCurrent = GetDlgItem(IDC_RADIO_TWO_DIGIT_YEAR_CURRENT);
		m_editMinimumTwoDigitYear = GetDlgItem(IDC_EDIT_TWO_DIGIT_YEAR);

		// Access the underlying object
		UCLID_AFSPLITTERSLib::IDateTimeSplitterPtr ipObj = m_ppUnk[0];
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI09768", "No object associated with this property page!");
		}

		/////////////////////////////////
		// Get and apply initial settings
		/////////////////////////////////

		// Update Month format
		if (ipObj->GetSplitMonthAsName() == VARIANT_TRUE)
		{
			m_btnMonthName.SetCheck( TRUE );
		}
		else
		{
			m_btnMonthNumber.SetCheck( TRUE );
		}

		// Update Year format
		if (ipObj->GetSplitFourDigitYear() == VARIANT_TRUE)
		{
			m_btnYearFour.SetCheck( TRUE );
		}
		else
		{
			m_btnYearTwo.SetCheck( TRUE );
		}

		// Update Time format
		if (ipObj->GetSplitMilitaryTime() == VARIANT_TRUE)
		{
			m_btnTimeMilitary.SetCheck( TRUE );
		}
		else
		{
			m_btnTimeNormal.SetCheck( TRUE );
		}

		// Set Defaults
		if (ipObj->GetSplitDefaults() == VARIANT_TRUE)
		{
			m_btnSplitDefaults.SetCheck( TRUE );
		}
		else
		{
			m_btnSplitDefaults.SetCheck( FALSE );
		}

		// Set Day Of Week
		if (ipObj->GetSplitDayOfWeek() == VARIANT_TRUE)
		{
			m_btnDayOfWeek.SetCheck( TRUE );
		}
		else
		{
			m_btnDayOfWeek.SetCheck( FALSE );
		}

		// Set Show Formatted Output
		if (ipObj->GetShowFormattedOutput() == VARIANT_TRUE)
		{
			m_btnShowFormatted.SetCheck( TRUE );

			// Enable Format edit box
			m_editFormat.EnableWindow( TRUE );
		}
		else
		{
			m_btnShowFormatted.SetCheck( FALSE );

			// Disable Format edit box
			m_editFormat.EnableWindow( FALSE );
		}

		// Set Format
		if (ipObj->GetShowFormattedOutput() == VARIANT_TRUE)
		{
			// Retrieve and display the format
			_bstr_t	bstrFormat = ipObj->GetOutputFormat();
			m_editFormat.SetWindowText( bstrFormat.operator const char *() );
		}
		else
		{
			// Clear the format
			m_editFormat.SetWindowText( "" );
		}

		// Clear the output text
		m_editOutput.SetWindowText( "" );

		// Set two digit year radio buttons
		if (ipObj->TwoDigitYearBeforeCurrent == VARIANT_TRUE)
		{
			m_btnTwoDigitYearCurrent.SetCheck(BST_CHECKED);
		}
		else
		{
			m_btnTwoDigitYearSpecified.SetCheck(BST_CHECKED);
		}

		// Set two digit year edit box
		long lMinYear = ipObj->MinimumTwoDigitYear;
		m_editMinimumTwoDigitYear.SetWindowText(asString(lMinYear).c_str());
		updateTwoDigitYearEditBox();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09771");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDateTimeSplitterPP::OnClickedCheckShowFormatted(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Enable Format edit box
		if (m_btnShowFormatted.GetCheck() == 1)
		{
			m_editFormat.EnableWindow( TRUE );
		}
		// Disable Format edit box
		else
		{
			m_editFormat.EnableWindow( FALSE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09770")
		
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDateTimeSplitterPP::OnClickedButtonTest(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
												 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Clear the output text
		m_editOutput.SetWindowText( "" );

		// Exercise the format string
		string strOutput = formatCurrentTime();

		// Display results or error message
		if (strOutput.length() == 0)
		{
			CComBSTR bstrFormat;
			m_editFormat.GetWindowText( bstrFormat.m_str );
			string strFormat = asString( bstrFormat.m_str );

			if (strFormat.length() == 0)
			{
				m_editOutput.SetWindowText( "<Format text is empty>" );
			}
			else
			{
				m_editOutput.SetWindowText( "<Format test yielded empty string>" );
			}
		}
		else
		{
			m_editOutput.SetWindowText( strOutput.c_str() );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09772")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDateTimeSplitterPP::OnClickedFormatInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
												 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Show tooltip info
		CString zText("Provide a format string that can include case-sensitive strftime formatting codes.\n"
			"Common Examples:\n"
			"%B - Full month name\n"
			"%d - Day of month as decimal number\n"
			"%H - Hour in 24-hour format\n"
			"%I - Hour in 12-hour format\n"
			"%m - Month as decimal number\n"
			"%M - Minute as decimal number\n"
			"%p - Current locale's A.M. / P.M. indicator\n"
			"%S - Second as decimal number\n"
			"%y - Year without century, as decimal number\n"
			"%Y - Year with century, as decimal number\n\n"
			"\"%B %d, %Y at %I:%M\" yields \"February 15, 2007 at 2:34\"");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15696");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDateTimeSplitterPP::OnBnClickedRadioTwoDigitYearSpecified(WORD wNotifyCode, WORD wID, 
	HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateTwoDigitYearEditBox();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI25723");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDateTimeSplitterPP::OnBnClickedRadioTwoDigitYearCurrent(WORD wNotifyCode, WORD wID, 
	HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateTwoDigitYearEditBox();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI25724");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
string CDateTimeSplitterPP::formatCurrentTime()
{
	string strResult;

	// Retrieve Format string
	CComBSTR bstrFormat;
	m_editFormat.GetWindowText( bstrFormat.m_str );
	string strFormat = asString( bstrFormat.m_str );

	// Use system time to exercise the specified Format string
	if (strFormat.length() > 0)
	{
		// Get current system time
		SYSTEMTIME	now;
		GetSystemTime( &now );

		// Initialize Date-Time object with current time
		COleDateTime	dt( now );

		long lTemp = dt.GetHour();
		lTemp = dt.GetMinute();

		// Exercise the format string
		CString zFormat = dt.Format( strFormat.c_str() );
		strResult = LPCTSTR(zFormat);
	}

	return strResult;
}
//-------------------------------------------------------------------------------------------------
long CDateTimeSplitterPP::getMinimumTwoDigitYear()
{
	// Get the two digit year as a string
	_bstr_t bstrTwoDigitYear;
	m_editMinimumTwoDigitYear.GetWindowText(bstrTwoDigitYear.GetAddress());
	string strTwoDigitYear = asString(bstrTwoDigitYear);

	// Convert the string to a long, or else return -1
	long lResult = -1;
	if (!strTwoDigitYear.empty())
	{
		try
		{
			lResult = asLong(strTwoDigitYear);
		}
		catch (...)
		{
			// No need to log an exception, just return -1 to indicate failure.
		}
	}

	return lResult;	
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitterPP::updateTwoDigitYearEditBox()
{
	bool bEnabled = m_btnTwoDigitYearSpecified.GetCheck() == BST_CHECKED;
	m_editMinimumTwoDigitYear.EnableWindow( asMFCBool(bEnabled) );
}
//-------------------------------------------------------------------------------------------------
void CDateTimeSplitterPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI09744", 
		"Date-Time Splitter PP" );
}
//-------------------------------------------------------------------------------------------------
