// RuleTesterUI.cpp : Implementation of CRuleTesterUI
#include "stdafx.h"
#include "AFCore.h"
#include "RuleTesterUI.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CRuleTesterUI
//-------------------------------------------------------------------------------------------------
CRuleTesterUI::CRuleTesterUI()
{
}
//-------------------------------------------------------------------------------------------------
CRuleTesterUI::~CRuleTesterUI()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// clean up the dialog resource in this scope so that the
		// code executes in the correct AFX state
		m_apDlg.reset(__nullptr);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI08816")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleTesterUI::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRuleTesterUI,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IRuleTesterUI
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleTesterUI::ShowUI(BSTR strFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate License
		validateLicense();

		// Allocate Ruleset object
		UCLID_AFCORELib::IRuleSetPtr ipRuleSet(CLSID_RuleSet);
		ASSERT_RESOURCE_ALLOCATION("ELI08804", ipRuleSet != __nullptr );

		// if a file name is given load it into the ruleset
		string strRSDFile = bstr_t(strFileName);
		if ( strRSDFile != "" )
		{
			ipRuleSet->LoadFrom( strFileName, VARIANT_FALSE);
		}

		m_apDlg = unique_ptr<RuleTesterDlg>(new RuleTesterDlg(NULL, ipRuleSet));

		m_apDlg->DoModal();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08803");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleTesterUI::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Private functions
//-------------------------------------------------------------------------------------------------
void CRuleTesterUI::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI08829", "Rule Tester UI" );
}
//-------------------------------------------------------------------------------------------------
