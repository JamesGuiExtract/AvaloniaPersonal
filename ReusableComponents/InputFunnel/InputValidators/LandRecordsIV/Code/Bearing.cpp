// Bearing.cpp : Implementation of CBearing
#include "stdafx.h"
#include "LandRecordsIV.h"
#include "Bearing.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <mathUtil.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CBearing
//-------------------------------------------------------------------------------------------------
CBearing::CBearing()
: m_ipResultLogger(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CBearing::~CBearing()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearing::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IBearing,
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
// IBearing
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearing::GetBearingInRadians(double *dValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_Bearing.isValid())
		{
			*dValue = m_Bearing.getRadians();
		}
		else
		{
			UCLIDException uclidException("ELI02364", "Invalid bearing input");
			uclidException.addDebugInfo("strInput", m_Bearing.asString());
			throw uclidException;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02365")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearing::GetBearingInDegrees(double *dValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		if (m_Bearing.isValid())
		{
			*dValue = m_Bearing.getDegrees();
		}
		else
		{
			UCLIDException uclidException("ELI02362", "Invalid bearing input");
			uclidException.addDebugInfo("strInput", m_Bearing.asString());
			throw uclidException;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02363")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearing::InitBearing(BSTR strInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// init bearing
		m_Bearing.evaluate(_bstr_t(strInput));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02554")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearing::IsValid(VARIANT_BOOL *bValid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*bValid = m_Bearing.isValid() ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02557")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearing::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CBearing::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	try
	{
		executeTest1();
		executeTest2();
		executeTest3();
	}
	CATCH_UCLID_EXCEPTION("ELI02404")
	CATCH_COM_EXCEPTION("ELI02405")
	CATCH_UNEXPECTED_EXCEPTION("ELI02406")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearing::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearing::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearing::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CBearing::executeTest1()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestBearing1"), _bstr_t("Validate Bearing Input"), kAutomatedTestCase); 

	// init bearing
	HRESULT hr = InitBearing(CComBSTR("N45°56'12\"E"));
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02400", "Failed to initialize bearing object");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// validate
	VARIANT_BOOL bValid = VARIANT_FALSE;
	hr = IsValid(&bValid);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02401", "Failed to validate bearing input");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	m_ipResultLogger->EndTestCase(bValid);
}
//-------------------------------------------------------------------------------------------------
void CBearing::executeTest2()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestBearing2"), _bstr_t("Get Bearing in Radians"), kAutomatedTestCase); 

	// init bearing
	InitBearing(CComBSTR("N45°56'12\"E"));

	// get bearing in radians
	double dRadians = 0.0;
	HRESULT hr = GetBearingInRadians(&dRadians);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02402", "Failed to get bearing in radians");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// check the value in radians
	double dCorrectRadians = (90.0 - (45.0 + 56.0/60.0 + 12.0/3600.0)) * MathVars::PI / 180.0;

	VARIANT_BOOL bValid = VARIANT_FALSE;
	if (MathVars::isEqual(dRadians, dCorrectRadians))
	{
		bValid = VARIANT_TRUE;
	}

	m_ipResultLogger->EndTestCase(bValid);
}
//-------------------------------------------------------------------------------------------------
void CBearing::executeTest3()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestBearing3"), _bstr_t("Get Bearing in Degrees"), kAutomatedTestCase); 

	// init bearing
	InitBearing(CComBSTR("N45°56'12\"E"));

	// get bearing in degrees
	double dDegrees = 0.0;
	HRESULT hr = GetBearingInDegrees(&dDegrees);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02403", "Failed to get bearing in radians");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// check the value in degrees
	double dCorrectDegrees = 90.0 - (45.0 + 56.0/60.0 + 12.0/3600.0);

	VARIANT_BOOL bValid = VARIANT_FALSE;
	if (MathVars::isEqual(dDegrees, dCorrectDegrees))
	{
		bValid = VARIANT_TRUE;
	}

	m_ipResultLogger->EndTestCase(bValid);
}
//-------------------------------------------------------------------------------------------------
void CBearing::validateLicense()
{
	static const unsigned long BEARING_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( BEARING_COMPONENT_ID, "ELI02620", "Bearing" );
}
//-------------------------------------------------------------------------------------------------
