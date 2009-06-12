// EnumSegment.cpp : Implementation of CEnumSegment
#include "stdafx.h"
#include "UCLIDFeatureMgmt.h"
#include "EnumSegment.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CEnumSegment
//-------------------------------------------------------------------------------------------------
CEnumSegment::CEnumSegment()
{
	// set the iterator to NULL.
	m_iter = m_vecSegments.end();
	m_bResetOnNext = true;
}
//-------------------------------------------------------------------------------------------------
CEnumSegment::~CEnumSegment()
{
}

//-------------------------------------------------------------------------------------------------
// ISupprotsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnumSegment::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IEnumSegment,
		&IID_IEnumSegmentModifier,
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
// IEnumSegment
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnumSegment::reset()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// flag the object to reset the iterator when next() is called.
		m_bResetOnNext = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01650");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnumSegment::next(IESSegment **pSegment)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// reset the iterator if appropriate.
		if (m_bResetOnNext)
		{
			m_iter = m_vecSegments.begin();
			m_bResetOnNext = false;
		}

		// if the iterator is at the end, return NULL.
		if (m_iter == m_vecSegments.end())
			*pSegment = NULL;
		else
		{
			// iterator not at the end
			// retrieve the object and return a reference to it
			UCLID_FEATUREMGMTLib::IESSegmentPtr ptrSegment(*m_iter);
			*pSegment = (IESSegment*)ptrSegment.Detach();

			// step the iterator to the next object in the vector
			m_iter++;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01651");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnumSegment::addSegment(IESSegment * pSegment)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// add the segment to the vector and increment its reference
		m_vecSegments.push_back(pSegment);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01652");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnumSegment::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Helper function
//-------------------------------------------------------------------------------------------------
void CEnumSegment::validateLicense()
{
	static const unsigned long ENUM_SEGMENT_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( ENUM_SEGMENT_COMPONENT_ID, "ELI02612", "Enum Segment" );
}
//-------------------------------------------------------------------------------------------------
