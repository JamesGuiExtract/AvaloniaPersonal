// SpatiallyCompareAttributes.cpp : Implementation of CSpatiallyCompareAttributes
#include "stdafx.h"
#include "AFCore.h"
#include "SpatiallyCompareAttributes.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CSpatiallyCompareAttributes
//-------------------------------------------------------------------------------------------------
CSpatiallyCompareAttributes::CSpatiallyCompareAttributes()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallyCompareAttributes::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISpatiallyCompareAttributes,
		&IID_ISortCompare
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ISortCompare
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallyCompareAttributes::raw_LessThan(IUnknown * pObj1, IUnknown * pObj2, VARIANT_BOOL * pbRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();
		UCLID_AFCORELib::IAttributePtr ipA1(pObj1);
		ASSERT_RESOURCE_ALLOCATION("ELI11277", ipA1 != NULL);
		UCLID_AFCORELib::IAttributePtr ipA2(pObj2);
		ASSERT_RESOURCE_ALLOCATION("ELI11278", ipA2 != NULL);

		ISortComparePtr ipCompare(CLSID_SpatiallyCompareStrings);
		ASSERT_RESOURCE_ALLOCATION("ELI11276", ipCompare != NULL);

		*pbRetVal = ipCompare->LessThan(ipA1->Value, ipA2->Value);
		return S_OK;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11279")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallyCompareAttributes::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		validateLicense();

		// If validateLicense doesn't throw any exception, then pbValue is true
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
void CSpatiallyCompareAttributes::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI11320", "SpatiallyCompareAttributes" );
}
//-------------------------------------------------------------------------------------------------
