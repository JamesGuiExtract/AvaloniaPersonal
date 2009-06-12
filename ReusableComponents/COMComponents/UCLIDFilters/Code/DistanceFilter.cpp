// DistanceFilter.cpp : Implementation of CDistanceFilter
#include "stdafx.h"
#include "UCLIDFilters.h"
#include "DistanceFilter.h"

#include <UCLIDException.hpp>
#include <LicenseMgmt.h>
#include <cpputil.hpp>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CDistanceFilter
//-------------------------------------------------------------------------------------------------
CDistanceFilter::CDistanceFilter()
{
}
//-------------------------------------------------------------------------------------------------
CDistanceFilter::~CDistanceFilter()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceFilter::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDistanceFilter
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IDistanceFilter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceFilter::AsStringInUnit(BSTR *pbstrOut, EDistanceUnitType eUnitType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// get the value in double first
		string strDistance = m_DistanceCore.asStringInUnit(eUnitType);

		*pbstrOut = _bstr_t(strDistance.c_str()).copy();

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02941");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceFilter::Evaluate(BSTR bstrInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string str((CString)bstrInput);
		m_DistanceCore.evaluate(str);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02942");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceFilter::GetDistanceInUnit(double *pdOutValue, EDistanceUnitType eOutUnit)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pdOutValue = m_DistanceCore.getDistanceInUnit(eOutUnit);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02945");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceFilter::GetOriginalInputString(BSTR *pbstrOrignInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pbstrOrignInput = _bstr_t(m_DistanceCore.getOriginalInputString().c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02943");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceFilter::IsValid(VARIANT_BOOL *pbValid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pbValid = m_DistanceCore.isValid() ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02944");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceFilter::Reset()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_DistanceCore.reset();

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceFilter::SetDefaultUnitType(EDistanceUnitType eDefaultUnit)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_DistanceCore.setDefaultDistanceUnit(eDefaultUnit);

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceFilter::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CDistanceFilter::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		testDistanceFilter();
	}
	CATCH_UCLID_EXCEPTION("ELI02948")
	CATCH_COM_EXCEPTION("ELI02949")
	CATCH_UNEXPECTED_EXCEPTION("ELI02950")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceFilter::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceFilter::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceFilter::raw_SetResultLogger(ITestResultLogger * pLogger)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	m_ipResultLogger = pLogger;

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CDistanceFilter::testDistanceFilter()
{
	// test 1
	m_ipResultLogger->StartTestCase(_bstr_t("1"), _bstr_t("Test Distance Filter"), kAutomatedTestCase); 
	Evaluate(CComBSTR("1234 chains 23 link"));
	VARIANT_BOOL bValid = VARIANT_FALSE;
	IsValid(&bValid);
	m_ipResultLogger->EndTestCase(bValid);

	// test 2
	m_ipResultLogger->StartTestCase(_bstr_t("2"), _bstr_t("Test Distance Filter"), kAutomatedTestCase); 
	Evaluate(CComBSTR(" 124	m."));
	IsValid(&bValid);
	m_ipResultLogger->EndTestCase(bValid);

	// test 3
	m_ipResultLogger->StartTestCase(_bstr_t("3"), _bstr_t("Test Distance Filter"), kAutomatedTestCase); 
	Evaluate(CComBSTR("1234 inches 234 feet"));
	IsValid(&bValid);
	m_ipResultLogger->EndTestCase(bValid ? VARIANT_FALSE : VARIANT_TRUE);

	// test 4
	m_ipResultLogger->StartTestCase(_bstr_t("4"), _bstr_t("Test Distance Filter"), kAutomatedTestCase); 
	Evaluate(CComBSTR("12 km.	"));
	IsValid(&bValid);
	double dDistance = 0.0;
	GetDistanceInUnit(&dDistance, kFeet);
	if ( (dDistance - 39370.07874) > 1e-5 )
	{
		bValid = VARIANT_FALSE;
	}
	m_ipResultLogger->EndTestCase(bValid);

	// test 5
	m_ipResultLogger->StartTestCase(_bstr_t("5"), _bstr_t("Test Distance Filter"), kAutomatedTestCase); 
	Evaluate(CComBSTR("12 km.	"));
	IsValid(&bValid);
	BSTR bstrDistance = ::SysAllocString(L"");
	AsStringInUnit(&bstrDistance, kFeet);
	CString cstrTemp(bstrDistance);
	::SysFreeString(bstrDistance);
	if ( strcmpi(cstrTemp, "39370.078740157478000 Feet") != 0 )
	{
		bValid = VARIANT_FALSE;
	}
	m_ipResultLogger->EndTestCase(bValid);

	// test 6
	m_ipResultLogger->StartTestCase(_bstr_t("6"), _bstr_t("Test Distance Filter"), kAutomatedTestCase); 
	try
	{
		SetDefaultUnitType(kFeet);
		Evaluate(CComBSTR("12345"));
		IsValid(&bValid);
		double dDistance = 0.0;
		GetDistanceInUnit(&dDistance, kFeet);
		if ( (dDistance - 12345.0) > 1e-5 )
		{
			bValid = VARIANT_FALSE;
		}
	}
	catch(UCLIDException& ue)
	{
		string strError(ue.asStringizedByteStream());
		m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()));
		bValid = VARIANT_FALSE;
	}
	catch (_com_error& e) 
	{ 
		UCLIDException ue; 
		_bstr_t _bstrDescription = e.Description(); 
		char *pszDescription = _bstrDescription; 
		if (pszDescription) 
		{
			ue.createFromString("ELI02956", pszDescription); 
		}
		else 
		{
			ue.createFromString("ELI19493", "COM exception caught!"); 
		}
		ue.addDebugInfo("err.Error", e.Error()); 
		ue.addDebugInfo("err.WCode", e.WCode()); 
		string strError(ue.asStringizedByteStream());
		m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()));
		bValid = VARIANT_FALSE;

	}
	catch (...)
	{
		bValid = VARIANT_FALSE;
	}
	m_ipResultLogger->EndTestCase(bValid);
}
//-------------------------------------------------------------------------------------------------
void CDistanceFilter::validateLicense()
{
	static const unsigned long FILTERS_DISTANCE_COMPONENT_ID = 34;
	
	VALIDATE_LICENSE( FILTERS_DISTANCE_COMPONENT_ID, "ELI02940",
		"Distance Filter" );
}
//-------------------------------------------------------------------------------------------------
