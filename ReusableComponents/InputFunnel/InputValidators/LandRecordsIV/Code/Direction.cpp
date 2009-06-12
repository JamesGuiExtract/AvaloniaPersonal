// Direction.cpp : Implementation of CDirection
#include "stdafx.h"
#include "LandRecordsIV.h"
#include "Direction.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CDirection
//-------------------------------------------------------------------------------------------------
CDirection::CDirection()
{
}
//-------------------------------------------------------------------------------------------------
CDirection::~CDirection()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirection::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDirection,
		&IID_ILicensedComponent,
		&IID_ITestableComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirection::GetDirectionAsPolarAngleInDegrees(double *pdPolarAngleDegrees)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pdPolarAngleDegrees = m_DirectionHelper.getPolarAngleDegrees();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02881")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirection::get_GlobalDirectionType(ECartographicDirection *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// TODO: once we merge the direction type, no need for conversion
		*pVal = static_cast<ECartographicDirection>(m_DirectionHelper.sGetDirectionType());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03034")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirection::GetDirectionAsPolarAngleInRadians(double *pdPolarAngleRadians)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pdPolarAngleRadians = m_DirectionHelper.getPolarAngleRadians();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02882")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirection::InitDirection(BSTR bstrInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// init Direction
		m_DirectionHelper.evaluateDirection(string(_bstr_t(bstrInput)));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02883")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirection::IsValid(VARIANT_BOOL *pbValid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pbValid = m_DirectionHelper.isDirectionValid() ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02884")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirection::put_GlobalDirectionType(ECartographicDirection newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// TODO: once we merge the direction type, no need for conversion
		m_DirectionHelper.sSetDirectionType(static_cast<EDirection>(newVal));

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03036")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirection::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirection::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	try
	{
	}
	CATCH_UCLID_EXCEPTION("ELI02898")
	CATCH_COM_EXCEPTION("ELI02899")
	CATCH_UNEXPECTED_EXCEPTION("ELI02900")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirection::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirection::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirection::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CDirection::validateLicense()
{
	static const unsigned long DIRECTION_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( DIRECTION_COMPONENT_ID, "ELI02880",
		"Direction" );
}
//-------------------------------------------------------------------------------------------------
