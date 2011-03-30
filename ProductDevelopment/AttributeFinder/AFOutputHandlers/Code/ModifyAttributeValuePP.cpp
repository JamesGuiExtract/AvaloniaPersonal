// ModifyAttributeValuePP.cpp : Implementation of CModifyAttributeValuePP
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "ModifyAttributeValuePP.h"
#include "..\..\AFCore\Code\EditorLicenseID.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <StringTokenizer.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CModifyAttributeValuePP
//-------------------------------------------------------------------------------------------------
CModifyAttributeValuePP::CModifyAttributeValuePP()
{
	m_dwTitleID = IDS_TITLEModifyAttributeValuePP;
	m_dwHelpFileID = IDS_HELPFILEModifyAttributeValuePP;
	m_dwDocStringID = IDS_DOCSTRINGModifyAttributeValuePP;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValuePP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CModifyAttributeValuePP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Get the output handler object
			UCLID_AFOUTPUTHANDLERSLib::IModifyAttributeValueOHPtr ipModifyValue = m_ppUnk[i];

			// Check Name edit box for text
			CComBSTR bstrAttributeQuery;
			GetDlgItemText( IDC_EDIT_ATTRIBUTEQUERY, bstrAttributeQuery.m_str );
			_bstr_t _bstrQuery( bstrAttributeQuery );
			if (_bstrQuery.length() == 0)
			{
				throw UCLIDException( "ELI08559", "Please specify an Attribute Query!" );
			}

			// At least one checkbox must be set
			if (!m_bUpdateName && !m_bUpdateValue && !m_bUpdateType)
			{
				throw UCLIDException( "ELI08740", "At least one of Name, Value or Type must be set!" );
			}

			// Store the Query
			ipModifyValue->AttributeQuery = _bstrQuery;

			// Retrieve and store Name
			CComBSTR bstrAttributeName;
			GetDlgItemText( IDC_EDIT_ATTRIBUTENAMECHANGE, bstrAttributeName.m_str );
			_bstr_t _bstrName( bstrAttributeName );

			// Retrieve and store Value
			CComBSTR bstrAttributeValue;
			GetDlgItemText( IDC_EDIT_ATTRIBUTEVALUE, bstrAttributeValue.m_str );
			_bstr_t _bstrValue( bstrAttributeValue );
			if(m_bUpdateValue)
			{
				ipModifyValue->AttributeValue = _bstrValue;
			}
			// Retrieve Type
			CComBSTR bstrAttributeType;
			GetDlgItemText( IDC_EDIT_ATTRIBUTETYPE, bstrAttributeType.m_str );
			_bstr_t _bstrType( bstrAttributeType );

			// Dummy IAttribute object for Name and Type validation
			IAttributePtr	ipDummy( CLSID_Attribute );
			ASSERT_RESOURCE_ALLOCATION( "ELI09531", ipDummy != __nullptr );
			if(m_bUpdateName)
			{
				ipDummy->PutName( _bstrName );
				ipModifyValue->AttributeName = _bstrName;
			}
			if(m_bUpdateType)
			{
				ipDummy->PutType( _bstrType );
				ipModifyValue->AttributeType = _bstrType;
			}
			

			// Apply checkbox settings
			ipModifyValue->SetAttributeName = 
				(m_bUpdateName ? VARIANT_TRUE : VARIANT_FALSE );
			ipModifyValue->SetAttributeValue = 
				(m_bUpdateValue ? VARIANT_TRUE : VARIANT_FALSE );
			ipModifyValue->SetAttributeType = 
				(m_bUpdateType ? VARIANT_TRUE : VARIANT_FALSE );

			ipModifyValue->CreateSubAttribute = 
				m_radioCreateSubAttr.GetCheck() == 1 ? VARIANT_TRUE : VARIANT_FALSE;
		}

		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08560")

	// if we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValuePP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CModifyAttributeValuePP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
											  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFOUTPUTHANDLERSLib::IModifyAttributeValueOHPtr ipModifyValue = m_ppUnk[0];
		if (ipModifyValue)
		{
			m_editAttributeQuery = GetDlgItem( IDC_EDIT_ATTRIBUTEQUERY );
			m_editAttributeName = GetDlgItem( IDC_EDIT_ATTRIBUTENAMECHANGE );
			m_editAttributeValue = GetDlgItem( IDC_EDIT_ATTRIBUTEVALUE );
			m_editAttributeType = GetDlgItem( IDC_EDIT_ATTRIBUTETYPE );
			m_radioModifySelected = GetDlgItem( IDC_RADIO_MODIFY_SELECTED );
			m_radioCreateSubAttr = GetDlgItem( IDC_RADIO_CREATE_SUB_ATTRIBUTE );

			// Retrieve text settings
			string strQuery = ipModifyValue->AttributeQuery;
			string strName = ipModifyValue->AttributeName;
			string strValue = ipModifyValue->AttributeValue;
			string strType = ipModifyValue->AttributeType;

			// Initialize edit boxes
			m_editAttributeQuery.SetWindowText( strQuery.c_str() );
			m_editAttributeName.SetWindowText( strName.c_str() );
			m_editAttributeValue.SetWindowText( strValue.c_str() );
			m_editAttributeType.SetWindowText( strType.c_str() );

			// Retrieve and apply check box settings
			m_bUpdateName = (ipModifyValue->SetAttributeName == VARIANT_TRUE);
			CheckDlgButton( IDC_CHECK_UPDATENAME, m_bUpdateName ? BST_CHECKED : BST_UNCHECKED );
			m_bUpdateValue = (ipModifyValue->SetAttributeValue == VARIANT_TRUE);
			CheckDlgButton( IDC_CHECK_UPDATEVALUE, m_bUpdateValue ? BST_CHECKED : BST_UNCHECKED );
			m_bUpdateType = (ipModifyValue->SetAttributeType == VARIANT_TRUE);
			CheckDlgButton( IDC_CHECK_UPDATETYPE, m_bUpdateType ? BST_CHECKED : BST_UNCHECKED );

			// Enable / disable Value and Type edit boxes
			m_editAttributeName.EnableWindow( m_bUpdateName ? TRUE : FALSE );
			m_editAttributeValue.EnableWindow( m_bUpdateValue ? TRUE : FALSE );
			m_editAttributeType.EnableWindow( m_bUpdateType ? TRUE : FALSE );

			// Set focus to the Name editbox
			m_editAttributeQuery.SetSel( 0, -1 );
			m_editAttributeQuery.SetFocus();

			if(ipModifyValue->CreateSubAttribute == VARIANT_TRUE)
			{
				m_radioCreateSubAttr.SetCheck(1);
				BOOL bTmp;
				OnClickedRadioCreateSubAttribute(0, 0, 0, bTmp);
			}
			else
			{
				m_radioModifySelected.SetCheck(1);
				BOOL bTmp;
				OnClickedModifySelected(0, 0, 0, bTmp);
			}
		}
	
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08561");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CModifyAttributeValuePP::OnClickedCheckUpdateName(WORD wNotifyCode, WORD wID, 
														   HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve setting
		ATLControls::CButton checkBox( GetDlgItem( IDC_CHECK_UPDATENAME ) );
		m_bUpdateName = checkBox.GetCheck() == 1;

		// Enable/disable associated edit box
		m_editAttributeName.EnableWindow( m_bUpdateName ? TRUE : FALSE );

		// Set dirty flag
		SetDirty( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10008");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CModifyAttributeValuePP::OnClickedCheckUpdateValue(WORD wNotifyCode, WORD wID, 
														   HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve setting
		ATLControls::CButton checkBox( GetDlgItem( IDC_CHECK_UPDATEVALUE ) );
		m_bUpdateValue = checkBox.GetCheck() == 1;

		// Enable/disable associated edit box
		m_editAttributeValue.EnableWindow( m_bUpdateValue ? TRUE : FALSE );

		// Set dirty flag
		SetDirty( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08731");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CModifyAttributeValuePP::OnClickedCheckUpdateType(WORD wNotifyCode, WORD wID, 
														  HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve setting
		ATLControls::CButton checkBox( GetDlgItem( IDC_CHECK_UPDATETYPE ) );
		m_bUpdateType = checkBox.GetCheck() == 1;

		// Enable/disable associated edit box
		m_editAttributeType.EnableWindow( m_bUpdateType ? TRUE : FALSE );

		// Set dirty flag
		SetDirty( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08732");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CModifyAttributeValuePP::OnClickedRadioCreateSubAttribute(WORD wNotifyCode, WORD wID, 
														  HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{

		// Retrieve setting
		ATLControls::CButton checkBox( GetDlgItem( IDC_CHECK_UPDATENAME ) );
		checkBox.SetCheck(1);
		m_bUpdateName = checkBox.GetCheck() == 1;
		checkBox.EnableWindow(FALSE);


		// Enable/disable associated edit box
		m_editAttributeName.EnableWindow(TRUE);

		// Set dirty flag
		SetDirty( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19136");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CModifyAttributeValuePP::OnClickedModifySelected(WORD wNotifyCode, WORD wID, 
														  HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{

		// Retrieve setting
		ATLControls::CButton checkBox( GetDlgItem( IDC_CHECK_UPDATENAME ) );
		checkBox.EnableWindow(TRUE);
		m_bUpdateName = checkBox.GetCheck() == 1;


		if(checkBox.GetCheck() == 1)
		{
			m_editAttributeName.EnableWindow(TRUE);
		}
		else
		{
			m_editAttributeName.EnableWindow(FALSE);
		}
		// Set dirty flag
		SetDirty( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19137");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CModifyAttributeValuePP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI08558", 
		"ModifyAttributeValue PP" );
}
//-------------------------------------------------------------------------------------------------
