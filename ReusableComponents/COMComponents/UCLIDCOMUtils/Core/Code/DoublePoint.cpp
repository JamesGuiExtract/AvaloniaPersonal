// DoublePoint.cpp : Implementation of CDoublePoint
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "DoublePoint.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CDoublePoint
//-------------------------------------------------------------------------------------------------
CDoublePoint::CDoublePoint()
:m_dX(0), m_dY(0)
{
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoublePoint::CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Ensure that the object is a Vector
		UCLID_COMUTILSLib::IDoublePointPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI15238", ipSource != __nullptr);

		m_dX = ipSource->X;
		m_dY = ipSource->Y;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15239");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoublePoint::Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// create a new variant vector
		UCLID_COMUTILSLib::ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_DoublePoint);
		ASSERT_RESOURCE_ALLOCATION("ELI15240", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15241");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo interface
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoublePoint::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDoublePoint,
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
STDMETHODIMP CDoublePoint::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IDoublePoint interface
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoublePoint::get_X(double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_dX;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15242");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoublePoint::put_X(double newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_dX = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15243");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoublePoint::get_Y(double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_dY;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15244");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoublePoint::put_Y(double newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_dY = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15245");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CDoublePoint::validateLicense()
{
	static const unsigned long DOUBLEPOINT_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE( DOUBLEPOINT_COMPONENT_ID, "ELI15247", "DoublePoint" );
}
//-------------------------------------------------------------------------------------------------