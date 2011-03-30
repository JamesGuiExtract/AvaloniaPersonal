// ReformatPersonNamesPP.cpp : Implementation of CReformatPersonNamesPP
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "ReformatPersonNamesPP.h"
#include "..\..\AFCore\Code\EditorLicenseID.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CReformatPersonNamesPP
//-------------------------------------------------------------------------------------------------
CReformatPersonNamesPP::CReformatPersonNamesPP()
{
		m_dwTitleID = IDS_TITLEReformatPersonNamesPP;
		m_dwHelpFileID = IDS_HELPFILEReformatPersonNamesPP;
		m_dwDocStringID = IDS_DOCSTRINGReformatPersonNamesPP;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNamesPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CReformatPersonNamesPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFOUTPUTHANDLERSLib::IReformatPersonNamesPtr ipRPN = m_ppUnk[i];
			if(ipRPN == __nullptr)
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI09588");
			}
			
			// Check Query edit box for text
			// Then save the query information
			CComBSTR bstrAttributeQuery;
			m_editAttributeQuery.GetWindowText(&bstrAttributeQuery);
			_bstr_t _bstrQuery( bstrAttributeQuery );
			if (_bstrQuery.length() == 0)
			{
				m_editAttributeQuery.SetFocus();
				throw UCLIDException( "ELI09589", "Please specify a Query!" );
			}
			ipRPN->PutPersonAttributeQuery(_bstrQuery);

			ipRPN->ReformatPersonSubAttributes = m_chkReformatPersonSubAttributes.GetCheck() == 1 ? VARIANT_TRUE : VARIANT_FALSE;

			CComBSTR bstrFormat;
			m_editFormatString.GetWindowText(&bstrFormat);
			_bstr_t _bstrFormat( bstrFormat );
			if (_bstrFormat.length() == 0)
			{
				m_editFormatString.SetFocus();
				throw UCLIDException( "ELI09601", "Please specify a new format for the names!" );
			}
			ipRPN->FormatString = _bstrFormat;

		}
		m_bDirty = FALSE;
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09590")
	// if we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNamesPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CReformatPersonNamesPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
											  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFOUTPUTHANDLERSLib::IReformatPersonNamesPtr ipRPN = m_ppUnk[0];
		if (ipRPN != __nullptr)
		{

			// create tooltip object
			m_infoTip.Create(CWnd::FromHandle(m_hWnd));
			// set no delay.
			m_infoTip.SetShowDelay(0);

			// "Create" all the controls
			m_editAttributeQuery = GetDlgItem( IDC_EDIT_QUERY_PERSON_ATTRIBUTES );
			m_chkReformatPersonSubAttributes = GetDlgItem( IDC_CHK_REFORMAT_SUB_NAMES );
			m_editFormatString = GetDlgItem( IDC_EDIT_NAME_FORMAT );;
			
			// Initialize the UI to the state of the current IReformatPersonNamesPtr
			// Set up the Query Box
			string strQuery = ipRPN->GetPersonAttributeQuery();
			m_editAttributeQuery.SetWindowText( strQuery.c_str() );
			m_editAttributeQuery.SetFocus();

			m_chkReformatPersonSubAttributes.SetCheck((ipRPN->ReformatPersonSubAttributes == VARIANT_TRUE) ? 1 : 0);

			string strFormat = ipRPN->FormatString;
			m_editFormatString.SetWindowText( strFormat.c_str() );
		}
	
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09591");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CReformatPersonNamesPP::OnClickedNameFormatInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("A valid format string consists of a sequence of Text and Variables.\n"
			"- A Variable is defined by %VARNAME where VARNAME refers to a sub attribute of \n"
			"a person name.  Valid VARNAMEs for a person are %First %Last %Middle %Title and\n" 
			"%Suffix.\n"
			"- In addition a number can be optionally specified between % and VARNAME (i.e. \n"
			"%2VARNAME).  The number indicates how many characters of the variable to use.\n"
			"For instance %1Middle returns the first letter of the middle name(the middle initial).\n"
			"Blocks of Text and Variables may be optionally enclosed in a Scope.\n"
			"- A Scope is defined by a scope opening operator \'<\' and a closing operator \'>\'.\n"
			"The contents of a scope will only be used if ALL variables in the scope have a value\n"
			"for a particular person.\n"
			"Example: Format String \"%Last, %First< %Middle.>\"\n"
			"- The name \"Steve J Perry\" would be reformatted to \"Perry, Steve J.\"\n"
			"- The name \"Steve Perry\" would be reformatted to \"Perry, Steve\"\n"
			"- The importance of the scope operator can be seen in this example because\n"
			"it allows a name with no middle initial to ignore the \" \" and the \".\" around\n"
			"the middle initial if one does not exist as in the second example case.");   
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09628");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CReformatPersonNamesPP::validateLicense()
{
	// Validation requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI09592", 
		"Reformat Person Nmaes PP" );
}
//-------------------------------------------------------------------------------------------------
