// EnumPart.cpp : Implementation of CEnumPart
#include "stdafx.h"
#include "UCLIDFeatureMgmt.h"
#include "EnumPart.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CEnumPart
//-------------------------------------------------------------------------------------------------
CEnumPart::CEnumPart()
{
	// set the iterator to NULL.
	m_iter = m_vecParts.end();
	m_bResetOnNext = true;
}
//-------------------------------------------------------------------------------------------------
CEnumPart::~CEnumPart()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnumPart::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IEnumPart,
		&IID_IEnumPartModifier,
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
// IEnumPart
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnumPart::reset()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// flag the object to reset the iterator when next() is called.
		m_bResetOnNext = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01666");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnumPart::next(IPart **pPart)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// reset the iterator if appropriate.
		if (m_bResetOnNext)
		{
			m_iter = m_vecParts.begin();
			m_bResetOnNext = false;
		}

		// if the iterator is at the end, return NULL.
		if (m_iter == m_vecParts.end())
			*pPart = NULL;
		else
		{
			// iterator not at the end
			// retrieve the object and increment its reference
			UCLID_FEATUREMGMTLib::IPartPtr ptrPart = *m_iter;
			*pPart = (IPart *)ptrPart.Detach();

			// step the iterator to the next object in the vector
			m_iter++;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01665");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnumPart::addPart(IPart * pPart)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// add the segment to the vector and increment its reference
		m_vecParts.push_back(pPart);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01664");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnumPart::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CEnumPart::validateLicense()
{
	static const unsigned long ENUM_PART_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( ENUM_PART_COMPONENT_ID, "ELI02611", "Enum Part" );
}
//-------------------------------------------------------------------------------------------------
