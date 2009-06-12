// RegExprFinder.cpp : Implementation of CRegExprFinder
#include "stdafx.h"
#include "InputFinders.h"
#include "RegExprFinder.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRegExprFinder,
		&IID_IInputFinder,
		&IID_ILicensedComponent
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IInputFinder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprFinder::ParseString(BSTR strInput, IIUnknownVector * * ippTokenPositions)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Check parameter
		if (ippTokenPositions == NULL)
		{
			return E_POINTER;
		}

		// get the vector of object pairs by searching
		IIUnknownVectorPtr ipObjectPairs = m_ipRegExpParser->Find(strInput, VARIANT_FALSE, VARIANT_FALSE);
		long nObjPairsCount = ipObjectPairs->Size();
		// get tokens out to creat a vector of tokens
		IIUnknownVectorPtr ipTokens(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI05313", ipTokens != NULL);
		for (long n=0; n<nObjPairsCount; n++)
		{
			IObjectPairPtr ipObjPair = ipObjectPairs->At(n);
			ITokenPtr ipToken = ipObjPair->Object1;
			if (ipToken)
			{
				ipTokens->PushBack(ipToken);
			}
		}

		// Send the string to the data member
		*ippTokenPositions = ipTokens.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03859")
		
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Failure of validateLicense is indicated by an exception being 
		// thrown
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// CRegExprFinder
//-------------------------------------------------------------------------------------------------
CRegExprFinder::CRegExprFinder()
{
	try
	{
		// Get a regular expression parser.
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI22293", ipMiscUtils != NULL);
		m_ipRegExpParser = ipMiscUtils->GetNewRegExpParserInstance("RegExprFinder");
		ASSERT_RESOURCE_ALLOCATION("ELI22281", m_ipRegExpParser != NULL);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI22280");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprFinder::get_Pattern(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Check parameter
		if (pVal == NULL)
		{
			return E_POINTER;
		}
			
		// Get the pattern from the data member
		m_ipRegExpParser->get_Pattern( pVal );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03860")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprFinder::put_Pattern(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Provide the pattern to the data member
		m_ipRegExpParser->put_Pattern( newVal );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03861")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprFinder::get_IgnoreCase(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Check parameter
		if (pVal == NULL)
		{
			return E_POINTER;
		}
			
		// Get the setting from the VBScript object
		m_ipRegExpParser->get_IgnoreCase( pVal );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03862")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprFinder::put_IgnoreCase(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Provide the setting to the data member
		m_ipRegExpParser->put_IgnoreCase( newVal );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03863")
		
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper function
//-------------------------------------------------------------------------------------------------
void CRegExprFinder::validateLicense()
{
	static const unsigned long REGEXP_FINDER_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( REGEXP_FINDER_COMPONENT_ID, "ELI03858", 
		"VBScript Regular Expression Finder" );
}
//-------------------------------------------------------------------------------------------------
