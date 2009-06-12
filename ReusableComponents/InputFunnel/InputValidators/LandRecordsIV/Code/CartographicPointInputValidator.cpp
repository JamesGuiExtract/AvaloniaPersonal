// CartographicPointInputValidator.cpp : Implementation of CCartographicPointInputValidator
#include "stdafx.h"
#include "LandRecordsIV.h"
#include "CartographicPointInputValidator.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CCartographicPointInputValidator
//-------------------------------------------------------------------------------------------------
CCartographicPointInputValidator::CCartographicPointInputValidator()
{
}
//-------------------------------------------------------------------------------------------------
CCartographicPointInputValidator::~CCartographicPointInputValidator()
{
}

//-------------------------------------------------------------------------------------------------
//ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPointInputValidator::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInputValidator,
		&IID_ICategorizedComponent,
		&IID_ILicensedComponent,
		&IID_IInputValidator,
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
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPointInputValidator::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pbstrComponentDescription = _bstr_t("Cartographic Point").copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02633")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPointInputValidator::raw_ValidateInput(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pbSuccessful = VARIANT_FALSE;

		_bstr_t bstrInput = pTextInput->GetText();

		CComQIPtr<ICartographicPoint> ipCartographicPoint;
		HRESULT hr = ipCartographicPoint.CoCreateInstance(__uuidof(CartographicPoint));

		if (SUCCEEDED(hr))
		{
			// init input
			ipCartographicPoint->InitPointInString(bstrInput);
			
			VARIANT_BOOL bValid = ipCartographicPoint->IsValid();
			
			if (bValid)
			{
				// set input text
				pTextInput->SetText(bstrInput);
				
				// set validated input object		
				pTextInput->SetValidatedInput(ipCartographicPoint);	
				
				*pbSuccessful = VARIANT_TRUE;
			}
		}
		else
		{
			UCLIDException uclidException("ELI02642", "Failed to create CartographicPoint object");
			uclidException.addDebugInfo("HRESULT", (int)hr);
			throw uclidException;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02634")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPointInputValidator::raw_GetInputType(BSTR * pstrInputType)
{		
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	HRESULT hr = S_OK;
	try
	{
		validateLicense();

		hr = raw_GetComponentDescription(pstrInputType);

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02635")

	return hr;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPointInputValidator::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
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
STDMETHODIMP CCartographicPointInputValidator::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPointInputValidator::raw_RunInteractiveTests()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		// Path will be either Debug or Release directory
		string strDir = getModuleDirectory(_Module.m_hInst) + "\\";

		// bring up the exe 
		string strEXEFile = strDir + string("..\\VBTest\\TestCPIV.exe");
		runEXE(strEXEFile);

		string strITCFile = strDir + string("..\\Test\\TestCPIV.itc");

		_bstr_t bstrITCFile(strITCFile.c_str());
		m_ipExecuter->ExecuteITCFile(bstrITCFile);

	}
	CATCH_UCLID_EXCEPTION("ELI02643")
	CATCH_COM_EXCEPTION("ELI02644")
	CATCH_UNEXPECTED_EXCEPTION("ELI02645")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPointInputValidator::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	m_ipExecuter->SetResultLogger(pLogger);

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCartographicPointInputValidator::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	m_ipExecuter = pInteractiveTestExecuter;

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CCartographicPointInputValidator::validateLicense()
{
	static const unsigned long CARTOGRAPHIC_POINTIV_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( CARTOGRAPHIC_POINTIV_COMPONENT_ID, "ELI02632",
		"Cartographic Point Input Validator" );
}
//-------------------------------------------------------------------------------------------------
