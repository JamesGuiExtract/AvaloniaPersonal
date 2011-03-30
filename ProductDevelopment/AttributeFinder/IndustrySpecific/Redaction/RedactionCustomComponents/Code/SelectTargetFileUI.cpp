// SelectTargetFileUI.cpp : Implementation of CSelectTargetFileUI

#include "stdafx.h"
#include "SelectTargetFileUI.h"
#include "SelectTargetFileUIDlg.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// current version
const unsigned long gnCurrentVersion = 1;

//--------------------------------------------------------------------------------------------------
// CIDShieldVOAFileContentsCondition
//--------------------------------------------------------------------------------------------------
CSelectTargetFileUI::CSelectTargetFileUI()
{
}
//--------------------------------------------------------------------------------------------------
CSelectTargetFileUI::~CSelectTargetFileUI()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17474");
}
//--------------------------------------------------------------------------------------------------
HRESULT CSelectTargetFileUI::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CSelectTargetFileUI::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// ISelectTargetFileUI
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::get_FileName(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI17584", pVal != __nullptr);

		*pVal = _bstr_t(m_dlg.m_strFileName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17585");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::put_FileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_dlg.m_strFileName = asString(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17586");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::get_FileTypes(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI17581", pVal != __nullptr);

		*pVal = _bstr_t(m_dlg.m_strFileTypes.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17580");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::put_FileTypes(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_dlg.m_strFileTypes = asString(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17503");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::get_DefaultExtension(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI17582", pVal != __nullptr);

		*pVal = _bstr_t(m_dlg.m_strDefaultExtension.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17583");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::put_DefaultExtension(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_dlg.m_strDefaultExtension = asString(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17516");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::get_DefaultFileName(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI17587", pVal != __nullptr);

		*pVal = _bstr_t(m_dlg.m_strDefaultFileName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17588");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::put_DefaultFileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_dlg.m_strDefaultFileName = asString(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17496");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::get_Title(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI17589", pVal != __nullptr);

		*pVal = _bstr_t(m_dlg.m_strTitle.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17590");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::put_Title(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_dlg.m_strTitle = asString(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17497");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::get_Instructions(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI17591", pVal != __nullptr);

		*pVal = _bstr_t(m_dlg.m_strInstructions.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17592");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::put_Instructions(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_dlg.m_strInstructions = asString(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17498");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::PromptForFile(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI17593", pVal != __nullptr);

		if (m_dlg.DoModal() == IDOK)
		{
			*pVal = VARIANT_TRUE;
		}
		else
		{
			*pVal = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17500");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_ISelectTargetFileUI,
			&IID_ILicensedComponent
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17502");

	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectTargetFileUI::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// validate license
		validateLicense();

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
void CSelectTargetFileUI::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI19444", "Select Target File UI");
}
//-------------------------------------------------------------------------------------------------
