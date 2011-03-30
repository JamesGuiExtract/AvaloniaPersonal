// LongPoint.cpp : Implementation of CLongPoint
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "LongPoint.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CLongPoint
//-------------------------------------------------------------------------------------------------
CLongPoint::CLongPoint()
:m_nX(0), m_nY(0)
{
}
//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongPoint::CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Ensure that the object is a Vector
		UCLID_COMUTILSLib::ILongPointPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI10019", ipSource != __nullptr);

		m_nX = ipSource->X;
		m_nY = ipSource->Y;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10020");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongPoint::Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// create a new variant vector
		UCLID_COMUTILSLib::ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_LongPoint);
		ASSERT_RESOURCE_ALLOCATION("ELI10022", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10021");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo interface
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongPoint::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILongPoint,
		&IID_ICopyableObject,
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
STDMETHODIMP CLongPoint::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
	{
		return E_POINTER;
	}

	try
	{
		// validate license
		validateLicense();

		// Set to VARIANT_TRUE
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILongPoint interface
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongPoint::get_X(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_nX;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10023");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongPoint::put_X(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_nX = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10026");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongPoint::get_Y(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_nY;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10024");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongPoint::put_Y(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_nY = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10025");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CLongPoint::validateLicense()
{
	static const unsigned long LONGPOINT_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE( LONGPOINT_COMPONENT_ID, "ELI15249", "LongPoint" );
}
//-------------------------------------------------------------------------------------------------