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
CSpatiallyCompareAttributes::CSpatiallyCompareAttributes() : m_ipSortCompare(NULL)
{
	try
	{
		// Initialize the sort compare pointer
		m_ipSortCompare.CreateInstance(CLSID_SpatiallyCompareStrings);
		ASSERT_RESOURCE_ALLOCATION("ELI26621", m_ipSortCompare != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26622");
}
//-------------------------------------------------------------------------------------------------
HRESULT CSpatiallyCompareAttributes::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CSpatiallyCompareAttributes::FinalRelease()
{
	try
	{
		// Release COM objects before the object is destroyed
		m_ipSortCompare = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI26623");
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
STDMETHODIMP CSpatiallyCompareAttributes::raw_LessThan(IUnknown * pObj1, IUnknown * pObj2,
													   VARIANT_BOOL * pbRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check the arguments
		ASSERT_ARGUMENT("ELI26624", pbRetVal != __nullptr);
		UCLID_AFCORELib::IAttributePtr ipA1(pObj1);
		ASSERT_RESOURCE_ALLOCATION("ELI11277", ipA1 != __nullptr);
		UCLID_AFCORELib::IAttributePtr ipA2(pObj2);
		ASSERT_RESOURCE_ALLOCATION("ELI11278", ipA2 != __nullptr);

		// Get the spatial strings for the attributes
		ISpatialStringPtr ipValue1 = ipA1->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI26625", ipValue1 != __nullptr);
		ISpatialStringPtr ipValue2 = ipA2->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI26626", ipValue2 != __nullptr);

		// Check the spatialness of the strings
		bool bV1Spatial = asCppBool(ipValue1->HasSpatialInfo());
		bool bV2Spatial = asCppBool(ipValue2->HasSpatialInfo());

		if (bV1Spatial && bV2Spatial)
		{
			*pbRetVal = m_ipSortCompare->LessThan(ipValue1, ipValue2);
		}
		else
		{
			*pbRetVal = asVariantBool(bV1Spatial);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11279")
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
