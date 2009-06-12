// ParameterTypeValuePair.cpp : Implementation of CParameterTypeValuePair
#include "stdafx.h"
#include "UCLIDFeatureMgmt.h"
#include "ParameterTypeValuePair.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CParameterTypeValuePair
//-------------------------------------------------------------------------------------------------
CParameterTypeValuePair::CParameterTypeValuePair()
: m_eParamType(kInvalidParameterType),
  m_strValue("")
{
}
//-------------------------------------------------------------------------------------------------
CParameterTypeValuePair::~CParameterTypeValuePair()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CParameterTypeValuePair::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IParameterTypeValuePair,
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
// IParameterTypeValuePair
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CParameterTypeValuePair::get_eParamType(ECurveParameterType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// return the parameter type
		*pVal = m_eParamType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01639");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CParameterTypeValuePair::put_eParamType(ECurveParameterType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// store the specified parameter type
		m_eParamType = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01640");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CParameterTypeValuePair::get_strValue(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// return the current value
		*pVal = _bstr_t(m_strValue.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01641");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CParameterTypeValuePair::put_strValue(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// store the specified value
		m_strValue = _bstr_t(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01642");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CParameterTypeValuePair::valueIsEqualTo(IParameterTypeValuePair *pParamValueTypePair, 
													 VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		// get the param type and value
		UCLID_FEATUREMGMTLib::IParameterTypeValuePairPtr ptrParamValueTypePair(pParamValueTypePair);
		ECurveParameterType eCurveParameterType = ptrParamValueTypePair->eParamType;
		
		string strValue = _bstr_t(ptrParamValueTypePair->strValue);

		// return true or false depending upon whether the values are equal
		bool bEqual = (eCurveParameterType == m_eParamType && strValue == m_strValue);
		*pbValue = bEqual ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01643");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CParameterTypeValuePair::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CParameterTypeValuePair::validateLicense()
{
	static const unsigned long PARA_TYP_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( PARA_TYP_COMPONENT_ID, "ELI02615",
		"Parameter Type-Value Pair" );
}
//-------------------------------------------------------------------------------------------------
