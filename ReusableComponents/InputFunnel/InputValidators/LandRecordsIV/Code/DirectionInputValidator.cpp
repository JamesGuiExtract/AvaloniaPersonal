// DirectionInputValidator.cpp : Implementation of CDirectionInputValidator
#include "stdafx.h"
#include "LandRecordsIV.h"
#include "DirectionInputValidator.h"
#include "InputTypes.h"

#include <DirectionHelper.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CDirectionInputValidator
//-------------------------------------------------------------------------------------------------
CDirectionInputValidator::CDirectionInputValidator()
: m_ipExecuter(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CDirectionInputValidator::~CDirectionInputValidator()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirectionInputValidator::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInputValidator,
		&IID_ICategorizedComponent,
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
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirectionInputValidator::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pbstrComponentDescription = _bstr_t(gstrDirection_INPUT_TYPE.c_str()).copy();

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirectionInputValidator::raw_ValidateInput(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		*pbSuccessful = VARIANT_FALSE;
		
		// get the string out from pTextInput
		_bstr_t bstrInput = pTextInput->GetText();
		
		// validate the bearing input
		DirectionHelper direction;
		direction.evaluateDirection(string(bstrInput));
		if (direction.isDirectionValid())
		{
			*pbSuccessful = VARIANT_TRUE;
			
			CComQIPtr<IDirection> ipDirection;
			ipDirection.CoCreateInstance(__uuidof(Direction));
			
			ipDirection->InitDirection(CComBSTR(direction.directionAsString().c_str()));
			
			// set validated input object
			pTextInput->SetValidatedInput(ipDirection);	
		}	
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02877")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirectionInputValidator::raw_GetInputType(BSTR * pstrInputType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	HRESULT ret = S_OK;
	try
	{
		validateLicense();

		ret = raw_GetComponentDescription(pstrInputType);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02878")
	
	return ret;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirectionInputValidator::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CDirectionInputValidator::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirectionInputValidator::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirectionInputValidator::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	m_ipExecuter->SetResultLogger(pLogger);

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDirectionInputValidator::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	m_ipExecuter = pInteractiveTestExecuter;

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CDirectionInputValidator::validateLicense()
{
	static const unsigned long DIRECTIONIV_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( DIRECTIONIV_COMPONENT_ID, "ELI02879",
		"Direction Input Validator" );
}
//-------------------------------------------------------------------------------------------------
