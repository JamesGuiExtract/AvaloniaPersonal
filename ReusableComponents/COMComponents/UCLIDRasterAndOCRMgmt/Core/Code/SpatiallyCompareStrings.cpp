// SpatiallyCompareStrings.cpp : Implementation of CSpatiallyCompareStrings
#include "stdafx.h"
#include "UCLIDRasterAndOCRMgmt.h"
#include "SpatiallyCompareStrings.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CSpatiallyCompareStrings
//-------------------------------------------------------------------------------------------------
CSpatiallyCompareStrings::CSpatiallyCompareStrings()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallyCompareStrings::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISpatiallyCompareStrings,
		&IID_ISortCompare,
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
// ISortCompare
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallyCompareStrings::raw_LessThan(IUnknown * pObj1, IUnknown * pObj2, VARIANT_BOOL * pbRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipS1(pObj1);
		ASSERT_RESOURCE_ALLOCATION("ELI11271", ipS1 != __nullptr);
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipS2(pObj2);
		ASSERT_RESOURCE_ALLOCATION("ELI11272", ipS2 != __nullptr);

		*pbRetVal = ipS1->IsSpatiallyLessThan(ipS2);
		return S_OK;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11270")


	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallyCompareStrings::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Private
//-------------------------------------------------------------------------------------------------
void CSpatiallyCompareStrings::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI11315", "SpatiallyCompareStrings" );
}
//-------------------------------------------------------------------------------------------------
