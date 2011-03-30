#include "stdafx.h"
#include "IFCore.h"
#include "OCRFilterMgr.h"

#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFilterMgr::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IOCRFilterMgr,
		&IID_IOCRFilter,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IOCRFilterMgr
//-------------------------------------------------------------------------------------------------
COCRFilterMgr::COCRFilterMgr()
: m_apFilterSchemeDlg(__nullptr)
{
}
//-------------------------------------------------------------------------------------------------
COCRFilterMgr::~COCRFilterMgr()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16463");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFilterMgr::GetCurrentScheme(BSTR *pstrSchemeName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pstrSchemeName = get_bstr_t( getFilterSchemeDlg()->getCurrentScheme() ).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03457");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFilterMgr::SetCurrentScheme(BSTR strSchemeName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		getFilterSchemeDlg()->setCurrentScheme( asString( strSchemeName ) );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03458");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFilterMgr::ShowFilterSchemesDlg()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		static bool bCreateModeless = false;
		if (!bCreateModeless)
		{
			getFilterSchemeDlg()->createModeless();
			bCreateModeless = true;
		}

		getFilterSchemeDlg()->ShowWindow(SW_SHOW);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03459");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFilterMgr::ShowFilterSettingsDlg()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		getFilterSchemeDlg()->showFilterSettingsDlg();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03460");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFilterMgr::GetValidChars(BSTR strInputType, BSTR *pstrValidChars)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string stdstrInputType = asString( strInputType );

		string strValidChars(getFilterSchemeDlg()->getValidChars(stdstrInputType));
		*pstrValidChars = get_bstr_t( strValidChars ).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03456");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFilterMgr::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		validateLicense();
		// if validateLicense doesn't throw any exception, then pbValue is true
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
OCRFilterSchemesDlg* COCRFilterMgr::getFilterSchemeDlg()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	if (m_apFilterSchemeDlg.get() == NULL)
	{
		m_apFilterSchemeDlg = unique_ptr<OCRFilterSchemesDlg>(new OCRFilterSchemesDlg);
	}

	return m_apFilterSchemeDlg.get();
}
//-------------------------------------------------------------------------------------------------
void COCRFilterMgr::validateLicense()
{
	static const unsigned long OCRFILTERMGR_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( OCRFILTERMGR_COMPONENT_ID, "ELI03872", "OCRFilterMgr" );
}
//-------------------------------------------------------------------------------------------------
