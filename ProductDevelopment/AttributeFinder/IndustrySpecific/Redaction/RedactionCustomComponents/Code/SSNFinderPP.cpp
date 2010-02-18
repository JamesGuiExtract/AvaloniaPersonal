// SSNFinderPP.cpp : Implementation of the CSSNFinderPP property page class.

#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "SSNFinderPP.h"

#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>

//-------------------------------------------------------------------------------------------------
// CSSNFinderPP
//-------------------------------------------------------------------------------------------------
CSSNFinderPP::CSSNFinderPP() 
{
	try
	{
		// check licensing
		validateLicense();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI17259");
}
//-------------------------------------------------------------------------------------------------
CSSNFinderPP::~CSSNFinderPP() 
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17260");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinderPP::Apply()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
		
	try
	{
		// check licensing
		validateLicense();

		// get the subattribute name entered by the user
		_bstr_t bstrSubattributeName;
		m_editSubattributeName.GetWindowText(bstrSubattributeName.GetAddress());

		// ensure that one was entered
		if(bstrSubattributeName.length() == 0)
		{
			MessageBox("Subattribute name must contain a value", "Error", MB_ICONEXCLAMATION);
			m_editSubattributeName.SetFocus();
			return S_FALSE;
		}
		try
		{
			validateIdentifier(asString(bstrSubattributeName));
		}
		catch(UCLIDException& uex)
		{
			uex.display();
			m_editSubattributeName.SetFocus();
			return S_FALSE;
		}
		
		// get the status of the checkboxes
		bool bSpatialSubattribute(m_chkHybridSubattribute.GetCheck() == BST_UNCHECKED);
		bool bClearIfNoneFound(m_chkClearIfNoneFound.GetCheck() == BST_CHECKED);

		// set the options of the associated objects accordingly
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_REDACTIONCUSTOMCOMPONENTSLib::ISSNFinderPtr ipSSNFinder(m_ppUnk[i]);
			ASSERT_RESOURCE_ALLOCATION("ELI18168", ipSSNFinder != NULL);

			ipSSNFinder->SetOptions(bstrSubattributeName, asVariantBool(bSpatialSubattribute),
				asVariantBool(bClearIfNoneFound));
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17261");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinderPP::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check parameter
		ASSERT_ARGUMENT("ELI18258", pbValue != NULL);

		try
		{
			// check license
			validateLicense();

			// if no exception was thrown, then the license is valid
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18308");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Message Handlers
//-------------------------------------------------------------------------------------------------
LRESULT CSSNFinderPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// get the SSNFinder associated with this property page
		// NOTE: this assumes only one coclass is associated with this property page
		UCLID_REDACTIONCUSTOMCOMPONENTSLib::ISSNFinderPtr ipSSNFinder(m_ppUnk[0]);
		ASSERT_RESOURCE_ALLOCATION("ELI18169", ipSSNFinder != NULL);

		// get the dialog items
		m_editSubattributeName = GetDlgItem(IDC_SSNFINDER_EDIT_SUBATTRIBUTE_NAME);
		m_chkHybridSubattribute = GetDlgItem(IDC_SSNFINDER_CHECK_SUBATTRIBUTE_SPATIAL);
		m_chkClearIfNoneFound = GetDlgItem(IDC_SSNFINDER_CHECK_CLEAR_IF_NONE_FOUND);

		// get the SSN Finder's options
		_bstr_t bstrSubattributeName;
		VARIANT_BOOL vbSpatialSubattribute, vbClearIfNoneFound;
		ipSSNFinder->GetOptions(bstrSubattributeName.GetAddress(), &vbSpatialSubattribute, 
			&vbClearIfNoneFound);

		// Validate the sub attribute name and prompt the user if it is invalid
		try
		{
			string strSub = asString(bstrSubattributeName);
			if (!strSub.empty())
			{
				validateIdentifier(strSub);
			}
		}
		catch(...)
		{
			MessageBox("Sub attribute name is invalid.", "Invalid Name", MB_OK | MB_ICONWARNING);
		}

		// set the dialog items' values to SSNFinder's options
		m_editSubattributeName.SetWindowText(bstrSubattributeName);
		m_chkHybridSubattribute.SetCheck(vbSpatialSubattribute == VARIANT_TRUE ? BST_UNCHECKED : BST_CHECKED);
		m_chkClearIfNoneFound.SetCheck(vbClearIfNoneFound == VARIANT_TRUE ? BST_CHECKED : BST_UNCHECKED);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17262");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CSSNFinderPP::validateLicense()
{
	VALIDATE_LICENSE(gnRULESET_EDITOR_UI_OBJECT, "ELI18299", "SSNFinder Property Page");
}
//-------------------------------------------------------------------------------------------------
