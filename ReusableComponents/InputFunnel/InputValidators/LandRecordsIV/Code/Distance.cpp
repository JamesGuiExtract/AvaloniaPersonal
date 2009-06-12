// Distance.cpp : Implementation of CDistance
#include "stdafx.h"
#include "LandRecordsIV.h"
#include "Distance.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <mathUtil.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CDistance
//-------------------------------------------------------------------------------------------------
CDistance::CDistance()
: m_ipDistanceConverter(__uuidof(DistanceConverter)),
  m_ipResultLogger(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CDistance::~CDistance()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistance::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDistance,
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
// IDistance
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistance::get_GlobalDefaultDistanceUnit(EDistanceUnitType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pVal = m_Distance.getDefaultDistanceUnit();

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistance::GetDistanceInUnit(EDistanceUnitType eOutUnit, double *dValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		if (!m_Distance.isValid())
		{
			UCLIDException uclidException("ELI02370", "Invalid distance input");
			uclidException.addDebugInfo("strInput", m_Distance.getOriginalInputString());
			throw uclidException;
		}
		
		// eOutUnit must be specified
		if (eOutUnit == kUnknownUnit)
		{
			UCLIDException uclidExcpetion("ELI02382", "Please specify the unit for the output distance!");
			uclidExcpetion.addDebugInfo("eOutUnit", eOutUnit);
			throw uclidExcpetion;
		}
						
		// get the distance value in specified unit
		*dValue = m_Distance.getDistanceInUnit(eOutUnit);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02371")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistance::InitDistance(BSTR strInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// init distance
		m_Distance.evaluate(string(CString(strInput)));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02568")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistance::IsValid(VARIANT_BOOL *bValid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		// check if the distance is valid or not
		*bValid = m_Distance.isValid() ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02571")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistance::put_GlobalDefaultDistanceUnit(EDistanceUnitType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_Distance.setDefaultDistanceUnit(newVal);

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistance::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CDistance::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	try
	{
		executeTest1();
		executeTest2();
		executeTest3();
	}
	CATCH_UCLID_EXCEPTION("ELI02413")
	CATCH_COM_EXCEPTION("ELI02414")
	CATCH_UNEXPECTED_EXCEPTION("ELI02415")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistance::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistance::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistance::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
//Helper functions
//-------------------------------------------------------------------------------------------------
void CDistance::executeTest1()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestDistance1"), _bstr_t("Validate Distance Input"), kAutomatedTestCase); 

	// init distance
	HRESULT hr = InitDistance(CComBSTR("12 chains 12 rods 25 links"));
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02416", "Failed to initialize distance object");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// validate
	VARIANT_BOOL bValid = VARIANT_FALSE;
	hr = IsValid(&bValid);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02417", "Failed to validate distance input");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	m_ipResultLogger->EndTestCase(bValid);
}
//-------------------------------------------------------------------------------------------------
void CDistance::executeTest2()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestDistance2"), _bstr_t("Get Distance Value in Meters"), kAutomatedTestCase); 

	// init distance with in-unit set to chains 
	InitDistance(CComBSTR("1234 meters"));

	// get distance in meters
	double dMeters = 0.0;
	HRESULT hr = GetDistanceInUnit(kMeters, &dMeters);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02418", "Failed to get distance in meters");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// check the value in meters
	double dCorrectMeters = 1234.0;

	VARIANT_BOOL bValid = VARIANT_FALSE;
	if (MathVars::isEqual(dMeters, dCorrectMeters))
	{
		bValid = VARIANT_TRUE;
	}

	m_ipResultLogger->EndTestCase(bValid);
}
//-------------------------------------------------------------------------------------------------
void CDistance::executeTest3()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestDistance3"), _bstr_t("Get Distance in Yards"), kAutomatedTestCase); 

	// init distance with in-unit set to meters
	InitDistance(CComBSTR("12 chains 12 rods 25 links"));

	// get distance in yards
	double dYards = 0.0;
	HRESULT hr = GetDistanceInUnit(kYards, &dYards);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02419", "Failed to get distance in yards");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// check the value in yards
	double dCorrectYards = ( 12 * 66 + 12 * 16.5 + 25 / 25 * 16.5 ) / 3.0;

	VARIANT_BOOL bValid = VARIANT_FALSE;
	if (MathVars::isEqual(dYards, dCorrectYards))
	{
		bValid = VARIANT_TRUE;
	}

	m_ipResultLogger->EndTestCase(bValid);
}
//-------------------------------------------------------------------------------------------------
void CDistance::validateLicense()
{
	static const unsigned long DISTANCE_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;
	
	VALIDATE_LICENSE( DISTANCE_COMPONENT_ID, "ELI02622", "Distance" );
}
//-------------------------------------------------------------------------------------------------
