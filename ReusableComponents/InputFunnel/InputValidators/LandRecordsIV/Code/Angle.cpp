// Angle.cpp : Implementation of CAngle
#include "stdafx.h"
#include "LandRecordsIV.h"
#include "Angle.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <mathUtil.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CAngle
//-------------------------------------------------------------------------------------------------
CAngle::CAngle()
: m_ipResultLogger(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CAngle::~CAngle()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngle::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAngle,
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
// IAngle
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngle::GetAngleInRadians(double *dValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_Angle.isValid())
		{
			*dValue = m_Angle.getRadians();
		}
		else
		{
			UCLIDException uclidException("ELI02366", "Invalid angle input");
			uclidException.addDebugInfo("strInput", m_Angle.asString());
			throw uclidException;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02367")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngle::GetAngleInDegrees(double *dValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_Angle.isValid())
		{
			*dValue = m_Angle.getDegrees();
		}
		else
		{
			UCLIDException uclidException("ELI02368", "Invalid angle input");
			uclidException.addDebugInfo("strInput", m_Angle.asString());
			throw uclidException;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02369")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngle::InitAngle(BSTR strInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_Angle.evaluate(_bstr_t(strInput));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02538")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngle::IsValid(VARIANT_BOOL *bValid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*bValid = m_Angle.isValid() ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02541")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngle::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CAngle::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	try
	{
		executeTest1();
		executeTest2();
		executeTest3();
	}
	CATCH_UCLID_EXCEPTION("ELI02393")
	CATCH_COM_EXCEPTION("ELI02394")
	CATCH_UNEXPECTED_EXCEPTION("ELI02395")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngle::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngle::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngle::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CAngle::executeTest1()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestAngle1"), _bstr_t("Validate Angle Input"), kAutomatedTestCase); 

	// init angle
	HRESULT hr = InitAngle(CComBSTR("45°56'12\""));
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02389", "Failed to initialize angle object");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// validate
	VARIANT_BOOL bValid = VARIANT_FALSE;
	hr = IsValid(&bValid);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02391", "Failed to validate angle input");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	m_ipResultLogger->EndTestCase(bValid);
}
//-------------------------------------------------------------------------------------------------
void CAngle::executeTest2()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestAngle2"), _bstr_t("Get Angle in Radians"), kAutomatedTestCase); 

	// init angle
	InitAngle(CComBSTR("45°56'12\""));

	// get angle in radians
	double dRadians = 0.0;
	HRESULT hr = GetAngleInRadians(&dRadians);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02390", "Failed to get angle in radians");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// check the value in radians
	double dCorrectRadians = (45.0 + 56.0/60.0 + 12.0/3600.0) * MathVars::PI / 180.0;

	VARIANT_BOOL bValid = VARIANT_FALSE;
	if (MathVars::isEqual(dRadians, dCorrectRadians))
	{
		bValid = VARIANT_TRUE;
	}

	m_ipResultLogger->EndTestCase(bValid);
}
//-------------------------------------------------------------------------------------------------
void CAngle::executeTest3()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestAngle3"), _bstr_t("Get Angle in Degrees"), kAutomatedTestCase); 

	// init angle
	InitAngle(CComBSTR("45°56'12\""));

	// get angle in degrees
	double dDegrees = 0.0;
	HRESULT hr = GetAngleInDegrees(&dDegrees);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02392", "Failed to get angle in radians");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// check the value in degrees
	double dCorrectDegrees = 45.0 + 56.0/60.0 + 12.0/3600.0;

	VARIANT_BOOL bValid = VARIANT_FALSE;
	if (MathVars::isEqual(dDegrees, dCorrectDegrees))
	{
		bValid = VARIANT_TRUE;
	}

	m_ipResultLogger->EndTestCase(bValid);
}
//-------------------------------------------------------------------------------------------------
void CAngle::validateLicense()
{
	static const unsigned long ANGLE_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( ANGLE_COMPONENT_ID, "ELI02618", "Angle" );
}
//-------------------------------------------------------------------------------------------------
