// ObjectSelectorUI.cpp : Implementation of CObjectSelectorUI
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "ObjectSelectorUI.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <comutils.h>
#include <ComponentLicenseIDs.h>

// add license management functions
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CObjectSelectorUI
//-------------------------------------------------------------------------------------------------
CObjectSelectorUI::CObjectSelectorUI()
: m_bPrivateLicenseInitialized(false)
{
}
//-------------------------------------------------------------------------------------------------
CObjectSelectorUI::~CObjectSelectorUI()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16513");
}

//-------------------------------------------------------------------------------------------------
// IObjectSelectorUI
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectSelectorUI::ShowUI1(BSTR strTitleAfterSelect, BSTR strPrompt1, 
									   BSTR strPrompt2, BSTR strCategory, 
									   IObjectWithDescription *pObj, 
									   VARIANT_BOOL vbAllowNone, VARIANT_BOOL *pbOK)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		showObjSelectDlg(asString(strTitleAfterSelect), asString(strPrompt1), asString(strPrompt2),
			asString(strCategory), pObj, asCppBool(vbAllowNone), 0, NULL, NULL, true, pbOK);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18107");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectSelectorUI::ShowUI2(BSTR strTitleAfterSelect, BSTR strPrompt1, 
									   BSTR strPrompt2, BSTR strCategory, 
									   IObjectWithDescription *pObj, 
									   VARIANT_BOOL vbAllowNone,
									   long nNumRequiredIIDs,
									   IID pRequiredIIDs[],
									   VARIANT_BOOL *pbOK)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		showObjSelectDlg(asString(strTitleAfterSelect), asString(strPrompt1), asString(strPrompt2),
			asString(strCategory), pObj, asCppBool(vbAllowNone), nNumRequiredIIDs, pRequiredIIDs, NULL, true, pbOK);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04215");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectSelectorUI::ShowUINoDescription(BSTR bstrTitleAfterSelect, BSTR bstrPrompt, 
													BSTR bstrCategory, 
													IObjectWithDescription *pObj, 
													long nNumRequiredIIDs,
													IID pRequiredIIDs[],
													VARIANT_BOOL *pbOK)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		showObjSelectDlg(asString(bstrTitleAfterSelect), "", asString(bstrPrompt),
			asString(bstrCategory), pObj, false, nNumRequiredIIDs, pRequiredIIDs, NULL, false, pbOK);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18103");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectSelectorUI::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IObjectSelectorUI,
		&IID_ILicensedComponent
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// IPrivateLicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectSelectorUI::raw_InitPrivateLicense(BSTR strPrivateLicenseKey)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// NOTE: this method shall not check regular license

		string stdstrPrivateLicenseKey = asString( strPrivateLicenseKey );

		// if the private license key is not valid, throw an exception
		if (!IS_VALID_PRIVATE_LICENSE(stdstrPrivateLicenseKey))
		{
			throw UCLIDException("ELI10299", "Invalid private license key!");
		}

		// the private license key is valid, set the bit.
		m_bPrivateLicenseInitialized = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10298")
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectSelectorUI::raw_IsPrivateLicensed(VARIANT_BOOL *pbIsLicensed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// NOTE: this method shall not check regular license

		*pbIsLicensed = m_bPrivateLicenseInitialized ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10300")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectSelectorUI::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}
			
		// Check license state
		validateLicense();

		// If validateLicense doesn't throw an exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CObjectSelectorUI::validateLicense()
{
	static const unsigned long OBJECT_SELECTOR_UI_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	// Check license state
	static bool	bLicensed1 = LicenseManagement::isLicensed( 
		OBJECT_SELECTOR_UI_COMPONENT_ID );

	// either one will do
	if (bLicensed1 || m_bPrivateLicenseInitialized)
		return;

	// Prepare and throw an exception if component is not licensed
	UCLIDException ue("ELI04914", "ObjectSelectorUI component is not licensed!");
	ue.addDebugInfo("Component Name", "ObjectSelectorUI");
	throw ue;
}
//--------------------------------------------------------------------------------------------------
void CObjectSelectorUI::showObjSelectDlg(string strTitleAfterSelect,
		string strPrompt1, string strPrompt2, string strCategory, 
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObj, bool bAllowNone, long nNumRequiredIIDs,
		IID pRequiredIIDs[], CWnd* pParent, bool bConfigureDescription, VARIANT_BOOL *pbOK)
{
	// Check license state
	validateLicense();

	ASSERT_ARGUMENT("ELI18148", ipObj != __nullptr);
	ASSERT_ARGUMENT("ELI18130", pbOK != __nullptr);

	// Since each call into here will need a CObjSelectDlg with unique parameters,
	// don't maintain the CObjSelectDlg after this call
	std::unique_ptr<CObjSelectDlg> apDlg( new CObjSelectDlg(strTitleAfterSelect, strPrompt1, strPrompt2,
		strCategory, ipObj, bAllowNone, nNumRequiredIIDs, pRequiredIIDs, pParent, bConfigureDescription));

	ASSERT_RESOURCE_ALLOCATION("ELI18129", apDlg.get() != __nullptr);

	// Show the dialog
	*pbOK = asVariantBool(apDlg->DoModal() == IDOK);
}
//--------------------------------------------------------------------------------------------------