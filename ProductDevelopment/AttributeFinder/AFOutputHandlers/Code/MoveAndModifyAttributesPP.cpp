// MoveAndModifyAttributesPP.cpp : Implementation of CMoveAndModifyAttributesPP
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "MoveAndModifyAttributesPP.h"
#include "..\..\AFCore\Code\EditorLicenseID.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CMoveAndModifyAttributesPP
//-------------------------------------------------------------------------------------------------
CMoveAndModifyAttributesPP::CMoveAndModifyAttributesPP()
{
	m_dwTitleID = IDS_TITLEMoveAndModifyAttributesPP;
	m_dwHelpFileID = IDS_HELPFILEMoveAndModifyAttributesPP;
	m_dwDocStringID = IDS_DOCSTRINGMoveAndModifyAttributesPP;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributesPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CMoveAndModifyAttributesPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFOUTPUTHANDLERSLib::IMoveAndModifyAttributesPtr ipMMA = m_ppUnk[i];
			if (ipMMA == __nullptr)
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI09444");
			}
			
			// Check Query edit box for text
			// Then save the query information
			CComBSTR bstrAttributeQuery;
			m_editAttributeQuery.GetWindowText(&bstrAttributeQuery);
			_bstr_t _bstrQuery( bstrAttributeQuery );
			if (_bstrQuery.length() == 0)
			{
				m_editAttributeQuery.SetFocus();
				throw UCLIDException( "ELI09445", "Please specify a Query!" );
			}
			ipMMA->PutAttributeQuery(_bstrQuery);

			// move attribute to
			EMoveAttributeLevel eMoveAttributeLevel 
					= m_radioMoveToRoot.GetCheck() == 1 ? kMoveToRoot : kMoveToParent;
			ipMMA->MoveAttributeLevel = (UCLID_AFOUTPUTHANDLERSLib::EMoveAttributeLevel)eMoveAttributeLevel;

			// Save the attribute name change information
			if (m_radioDoNotChangeName.GetCheck() == 1)
			{
				ipMMA->PutOverwriteAttributeName(UCLID_AFOUTPUTHANDLERSLib::kDoNotOverwrite);
			}
			else if (m_radioUseRootOrParentName.GetCheck() == 1)
			{
				ipMMA->PutOverwriteAttributeName(UCLID_AFOUTPUTHANDLERSLib::kOverwriteWithRootOrParentName);
			}
			else if (m_radioUseSpecifiedName.GetCheck() == 1)
			{
				// Check name edit box for text
				CComBSTR bstrAttributeName;
				m_editSpecifiedName.GetWindowText(&bstrAttributeName);
				_bstr_t _bstrName( bstrAttributeName );
				if (_bstrName.length() == 0)
				{
					m_editSpecifiedName.SetFocus();
					throw UCLIDException( "ELI19143", "Please specify an Attribute Name!" );
				}
				ipMMA->PutOverwriteAttributeName(UCLID_AFOUTPUTHANDLERSLib::kOverwriteWithSpecifiedName);
				ipMMA->PutSpecifiedAttributeName(_bstrName);
			}
			else
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI09446");
			}

			// Save the attribute type change information
			if (m_radioRetainType.GetCheck() == 1)
			{
				ipMMA->PutRetainAttributeType(VARIANT_TRUE);
			}
			else if (m_radioDoNotRetainType.GetCheck() == 1)
			{
				ipMMA->PutRetainAttributeType(VARIANT_FALSE);
			}
			else
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI09447");
			}

			if (m_chkAddRootOrParentType.GetCheck() == 1)
			{
				ipMMA->PutAddRootOrParentAttributeType(VARIANT_TRUE);
			}
			else
			{
				ipMMA->PutAddRootOrParentAttributeType(VARIANT_FALSE);
			}

			if (m_chkAddNameToType.GetCheck() == 1)
			{
				ipMMA->PutAddAttributeNameToType(VARIANT_TRUE);
			}
			else
			{
				ipMMA->PutAddAttributeNameToType(VARIANT_FALSE);
			}

			if (m_chkAddSpecifiedType.GetCheck() == 1)
			{
				// Check name type box for text
				CComBSTR bstrAttributeType;
				m_editSpecifiedType.GetWindowText(&bstrAttributeType);
				_bstr_t _bstrType( bstrAttributeType );
				if (_bstrType.length() == 0)
				{
					m_editSpecifiedType.SetFocus();
					throw UCLIDException( "ELI19144", "Please specify an Attribute Type!" );
				}
				ipMMA->PutSpecifiedAttributeType(_bstrType);
				ipMMA->PutAddSpecifiedAttributeType(VARIANT_TRUE);
			}
			else
			{
				ipMMA->PutAddSpecifiedAttributeType(VARIANT_FALSE);
			}

			if (m_chkDeleteRootOrParentIfAllChildrenMoved.GetCheck() == 1)
			{
				ipMMA->PutDeleteRootOrParentIfAllChildrenMoved(VARIANT_TRUE);
			}
			else
			{
				ipMMA->PutDeleteRootOrParentIfAllChildrenMoved(VARIANT_FALSE);
			}
		}
		m_bDirty = FALSE;
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09436")

	// if we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributesPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CMoveAndModifyAttributesPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
											  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFOUTPUTHANDLERSLib::IMoveAndModifyAttributesPtr ipMMA = m_ppUnk[0];
		if (ipMMA != __nullptr)
		{

			// "Create" all the controls
			m_editAttributeQuery = GetDlgItem( IDC_EDIT_QUERY );
			m_radioMoveToRoot = GetDlgItem(IDC_RADIO_MOVE_TO_ROOT);
			m_radioMoveToParent = GetDlgItem(IDC_RADIO_MOVE_TO_PARENT);
			m_radioDoNotChangeName = GetDlgItem( IDC_RADIO_DO_NOT_CHANGE_NAME);
			m_radioUseRootOrParentName = GetDlgItem( IDC_RADIO_ROOT_NAME );
			m_radioUseSpecifiedName = GetDlgItem( IDC_RADIO_SPECIFY_NAME );
			m_editSpecifiedName = GetDlgItem( IDC_EDIT_SPECIFY_NAME );
			m_radioRetainType = GetDlgItem( IDC_RADIO_RETAIN_TYPE );
			m_radioDoNotRetainType = GetDlgItem( IDC_RADIO_EMPTY_TYPE );
			m_chkAddRootOrParentType = GetDlgItem( IDC_CHECK_ADD_ROOT_TYPE );
			m_chkAddNameToType = GetDlgItem( IDC_CHECK_ADD_NAME_TO_TYPE );
			m_chkAddSpecifiedType = GetDlgItem( IDC_CHECK_ADD_SPECIFIED_TYPE );
			m_editSpecifiedType = GetDlgItem( IDC_EDIT_SPECIFY_TYPE );
			m_chkDeleteRootOrParentIfAllChildrenMoved = GetDlgItem( IDC_CHECK_DELETE_ROOT );

			// Initialize the UI to the state of the current IMoveAndModifyAttributesPtr
			// Set up the Query Box
			string strQuery = ipMMA->GetAttributeQuery();
			m_editAttributeQuery.SetWindowText( strQuery.c_str() );
			m_editAttributeQuery.SetFocus();

			// move to where ?
			EMoveAttributeLevel eMoveAttributeLevel = (EMoveAttributeLevel)ipMMA->MoveAttributeLevel;
			switch (eMoveAttributeLevel)
			{
			case kMoveToRoot:
				m_radioMoveToRoot.SetCheck(1);
				break;
			case kMoveToParent:
				m_radioMoveToParent.SetCheck(1);
				break;
			default:
				throw UCLIDException("ELI10204", "MoveAttributeLevel is not set properly.");
				break;
			}
			updateDisplayText();

			// Set up the Attribute Name group
			EOverwriteAttributeName eOverwriteName = (EOverwriteAttributeName)ipMMA->GetOverwriteAttributeName();
			if (eOverwriteName == kDoNotOverwrite)
			{
				m_radioDoNotChangeName.SetCheck(1);
				m_editSpecifiedName.EnableWindow(FALSE);
			}
			else if (eOverwriteName == kOverwriteWithRootOrParentName)
			{
				m_radioUseRootOrParentName.SetCheck(1);
				m_editSpecifiedName.EnableWindow(FALSE);
			}
			else if (eOverwriteName == kOverwriteWithSpecifiedName)
			{
				m_radioUseSpecifiedName.SetCheck(1);
				m_editSpecifiedName.EnableWindow(TRUE);
			}
			else
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI09438");
			}
			string strSpecName = ipMMA->GetSpecifiedAttributeName();
			m_editSpecifiedName.SetWindowText( strSpecName.c_str() );

			// Set up the Attribute Type group
			if (ipMMA->RetainAttributeType == VARIANT_TRUE)
			{
				m_radioRetainType.SetCheck(1);
				m_radioDoNotRetainType.SetCheck(0);
			}
			else
			{
				m_radioDoNotRetainType.SetCheck(1);
				m_radioRetainType.SetCheck(0);
			}
			m_chkAddRootOrParentType.SetCheck(ipMMA->AddRootOrParentAttributeType == VARIANT_TRUE ? 1 : 0);
			m_chkAddNameToType.SetCheck(ipMMA->AddAttributeNameToType == VARIANT_TRUE ? 1 : 0);
			m_chkAddSpecifiedType.SetCheck(ipMMA->AddSpecifiedAttributeType == VARIANT_TRUE ? 1 : 0);
			if (m_chkAddSpecifiedType.GetCheck() == 1)
			{
				m_editSpecifiedType.EnableWindow(TRUE);
			}
			else
			{
				m_editSpecifiedType.EnableWindow(FALSE);
			}
			
			string strSpecType = ipMMA->GetSpecifiedAttributeType();
			m_editSpecifiedType.SetWindowText( strSpecType.c_str() );

			m_chkDeleteRootOrParentIfAllChildrenMoved.SetCheck(ipMMA->DeleteRootOrParentIfAllChildrenMoved == VARIANT_TRUE ? 1 : 0);

			m_editAttributeQuery.SetFocus();
		}
	
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09440");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMoveAndModifyAttributesPP::OnClickedRadioMoveToRoot(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateDisplayText();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10198");
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMoveAndModifyAttributesPP::OnClickedRadioMoveToParent(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateDisplayText();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10199");
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMoveAndModifyAttributesPP::OnClickedRadioSpecifyName(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_editSpecifiedName.EnableWindow(TRUE);
		m_editSpecifiedName.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09439");
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMoveAndModifyAttributesPP::OnClickedRadioDoNotChangeName(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_editSpecifiedName.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09441");
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMoveAndModifyAttributesPP::OnClickedRadioRootName(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_editSpecifiedName.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09442");
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMoveAndModifyAttributesPP::OnClickedCheckAddSpecifiedType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		BOOL bEnable = m_chkAddSpecifiedType.GetCheck() == 1 ? TRUE : FALSE;
		m_editSpecifiedType.EnableWindow(bEnable);
		m_editSpecifiedType.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09443");
	return 0;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CMoveAndModifyAttributesPP::updateDisplayText()
{
	static const string ROOT_NAME = "Use the root ancestor's name";
	static const string PARENT_NAME = "Use the parent's name";
	static const string ROOT_TYPE = "Add the root ancestor's type";
	static const string PARENT_TYPE = "Add the parent's type";
	static const string DELETE_ROOT = "If, after the above operations, the root ancestor has no children, delete it";
	static const string DELETE_PARENT = "If, after the above operations, the parent of queried attribute has no children, delete it";

	bool bUseRoot = m_radioMoveToRoot.GetCheck() == 1;

	string strName = bUseRoot ? ROOT_NAME : PARENT_NAME;
	string strType = bUseRoot ? ROOT_TYPE : PARENT_TYPE;
	string strDelete = bUseRoot ? DELETE_ROOT : DELETE_PARENT;

	m_radioUseRootOrParentName.SetWindowText(strName.c_str());
	m_chkAddRootOrParentType.SetWindowText(strType.c_str());
	m_chkDeleteRootOrParentIfAllChildrenMoved.SetWindowText(strDelete.c_str());
}
//-------------------------------------------------------------------------------------------------
void CMoveAndModifyAttributesPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI09470", 
		"MoveAndModifyAttributes PP" );
}
//-------------------------------------------------------------------------------------------------
