// ObjectPair.cpp : Implementation of CObjectPair
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "ObjectPair.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CObjectPair
//-------------------------------------------------------------------------------------------------
CObjectPair::CObjectPair()
: m_ipObject1(NULL),
  m_ipObject2(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CObjectPair::~CObjectPair()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16512");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectPair::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IObjectPair,
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
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectPair::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IObjectPair
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectPair::get_Object1(IUnknown **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipObject1)
		{
			IUnknownPtr ipShallowCopy = m_ipObject1;
			*pVal = ipShallowCopy.Detach();
		}
		else
		{
			*pVal = NULL;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05302");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectPair::put_Object1(IUnknown *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipObject1 = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05303");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectPair::get_Object2(IUnknown **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipObject2)
		{
			IUnknownPtr ipShallowCopy = m_ipObject2;
			*pVal = ipShallowCopy.Detach();
		}
		else
		{
			*pVal = NULL;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05304");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectPair::put_Object2(IUnknown *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipObject2 = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05305");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper function
//-------------------------------------------------------------------------------------------------
void CObjectPair::validateLicense()
{
	static const unsigned long OBJECTPAIR_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE(OBJECTPAIR_COMPONENT_ID, "ELI05301", "ObjectPair");
}
//-------------------------------------------------------------------------------------------------
