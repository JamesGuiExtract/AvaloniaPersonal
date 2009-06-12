// CartographicPoint.cpp : Implementation of CCartographicPoint
#include "stdafx.h"
#include "UCLIDMeasurements.h"
#include "CartographicPoint.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <cpputil.h>
#include <mathUtil.h>
#include <ComponentLicenseIDs.h>

#include <math.h>

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPoint::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ICartographicPoint,
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
// ICartographicPoint
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPoint::InitPointInString(BSTR strInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		parseInput(strInput);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03440")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPoint::InitPointInXY(double dX, double dY)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_dX = dX;
		m_dY = dY;

		m_bValid = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03441")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPoint::GetPointInXY(double *pdX, double *pdY)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (!m_bValid)
		{
			UCLIDException uclidException("ELI03442", "Input string for the point is invalid");
			throw uclidException;
		}
		
		*pdX = m_dX;
		*pdY = m_dY;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03444")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPoint::IsEqual(ICartographicPoint *pPointToCompare, VARIANT_BOOL *pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		double dX, dY;
		pPointToCompare->GetPointInXY(&dX, &dY);

		*pbVal = MathVars::isEqual(dX, m_dX) && MathVars::isEqual(dY, m_dY) 
				 ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03437")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPoint::IsValid(VARIANT_BOOL *pbValid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pbValid = m_bValid ? VARIANT_TRUE : VARIANT_FALSE;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03443")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPoint::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		executeTest1();
		executeTest2();
		executeTest3();
	}
	CATCH_UCLID_EXCEPTION("ELI02646")
	CATCH_COM_EXCEPTION("ELI02647")
	CATCH_UNEXPECTED_EXCEPTION("ELI02648")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPoint::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPoint::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPoint::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPoint::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Helper functions
//-------------------------------------------------------------------------------------------------
void CCartographicPoint::validateLicense()
{
	static const unsigned long CARTOGRAPHIC_POINT_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( CARTOGRAPHIC_POINT_COMPONENT_ID, "ELI02624",
		"Cartographic Point" );
}
//-------------------------------------------------------------------------------------------------
void CCartographicPoint::parseInput(const CString& cstrInput)
{
	try
	{
		// trim the string first
		CString cstrTemp(cstrInput);
		cstrTemp.TrimLeft("\t ");
		cstrTemp.TrimRight("\t ");
		
		// the string should be delimited by a comma, for example, "1234.09, 1564.21"
		vector<string> vecTokens;
		StringTokenizer tokenizer;
		tokenizer.parse(cstrTemp, vecTokens);
		if (vecTokens.size() != 2) 
		{
			m_bValid = false;	
			return;
		}
		
		// trim the token
		string strTemp(vecTokens[0]);
		cstrTemp = strTemp.c_str();
		cstrTemp.TrimLeft("\t ");
		cstrTemp.TrimRight("\t ");
		
		// Extract X
		m_dX = strTemp.empty() ? 0.0 : asDouble(strTemp);
		
		// trim the token
		cstrTemp = vecTokens[1].c_str();
		cstrTemp.TrimLeft("\t ");
		cstrTemp.TrimRight("\t ");
		
		// Extract Y
		m_dY = asDouble(string(cstrTemp));
		
		// set to be true
		m_bValid = true;	
	}
	catch (...)
	{
		m_bValid = false;
	}
}
//-------------------------------------------------------------------------------------------------
void CCartographicPoint::executeTest1()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestCP1"), _bstr_t("Validate Point Input with an invalid input"), kAutomatedTestCase); 

	// init angle
	HRESULT hr = InitPointInString(CComBSTR("A String"));
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02649", "Failed to initialize CartographicPoint object");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// validate
	VARIANT_BOOL bValid = VARIANT_FALSE;
	hr = IsValid(&bValid);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02650", "Failed to validate CartographicPoint input");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// if return value is invalid, then pass the test
	m_ipResultLogger->EndTestCase(bValid ? VARIANT_FALSE : VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CCartographicPoint::executeTest2()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestCP2"), _bstr_t("Validate Point Input with a valid input"), kAutomatedTestCase); 

	// init angle
	HRESULT hr = InitPointInString(CComBSTR("   123.021  ,  1222.3214   "));
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02651", "Failed to initialize CartographicPoint object");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// validate
	VARIANT_BOOL bValid = VARIANT_FALSE;
	hr = IsValid(&bValid);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02652", "Failed to validate CartographicPoint input");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// if return value is valid, then pass the test
	m_ipResultLogger->EndTestCase(bValid);
}
//-------------------------------------------------------------------------------------------------
void CCartographicPoint::executeTest3()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestCP3"), _bstr_t("Validate return valud"), kAutomatedTestCase); 

	// init angle
	HRESULT hr = InitPointInString(CComBSTR("   123.021  ,  1222.3214   "));
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02653", "Failed to initialize CartographicPoint object");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// validate
	VARIANT_BOOL bRet = VARIANT_FALSE;
	hr = IsValid(&bRet);
	if (FAILED(hr) || !bRet)
	{
		UCLIDException uclidException("ELI02654", "Invalid CartographicPoint input");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// get point value in doubles
	double dX, dY;
	hr = GetPointInXY(&dX, &dY);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02655", "Failed to get CartographicPoint value");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	bRet = VARIANT_FALSE;
	if (dX == 123.021 && dY == 1222.3214)
	{
		bRet = VARIANT_TRUE;
	}

	// if return value is valid, then pass the test
	m_ipResultLogger->EndTestCase(bRet);
}
//-------------------------------------------------------------------------------------------------
