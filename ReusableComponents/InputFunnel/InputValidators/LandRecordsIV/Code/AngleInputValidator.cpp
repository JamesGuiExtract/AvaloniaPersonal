
#include "stdafx.h"
#include "LandRecordsIV.h"
#include "AngleInputValidator.h"
#include "InputTypes.h"

#include <Angle.hpp>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CAngleInputValidator
//-------------------------------------------------------------------------------------------------
CAngleInputValidator::CAngleInputValidator()
: m_ipExecuter(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CAngleInputValidator::~CAngleInputValidator()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngleInputValidator::InterfaceSupportsErrorInfo(REFIID riid)
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
STDMETHODIMP CAngleInputValidator::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pbstrComponentDescription = _bstr_t(gstrANGLE_INPUT_TYPE.c_str()).copy();

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngleInputValidator::raw_ValidateInput(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		*pbSuccessful = VARIANT_FALSE;
		
		// get the string out from pTextInput
		_bstr_t bstrInput = pTextInput->GetText();
		
		// validate the angle input
		static Angle angle;
		angle.resetVariables();
		angle.evaluate(bstrInput);
				
		if (angle.isValid())
		{
			*pbSuccessful = VARIANT_TRUE;
			
			CComPtr<IAngle> ipAngle;
			ipAngle.CoCreateInstance(__uuidof(Angle));
			
			ipAngle->InitAngle(CComBSTR(angle.asString().c_str()));
			
			// set validated input object
			pTextInput->SetValidatedInput(ipAngle);	
		}
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02544")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngleInputValidator::raw_GetInputType(BSTR * pstrInputType)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	HRESULT ret = S_OK;

	try
	{
		validateLicense();

		ret = raw_GetComponentDescription(pstrInputType);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02547")

	return ret;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngleInputValidator::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CAngleInputValidator::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngleInputValidator::raw_RunInteractiveTests()
{
	try
	{
		// Path will be either Debug or Release directory
		string strDir = getModuleDirectory(_Module.m_hInst) + "\\";

		// bring up the exe 
		string strEXEFile = strDir + string("..\\VBTest\\TestLandRecordsIV.exe");
		runEXE(strEXEFile);

		string strITCFile = strDir + string("..\\Test\\TestAngleIV.itc");

		_bstr_t bstrITCFile(strITCFile.c_str());
		m_ipExecuter->ExecuteITCFile(bstrITCFile);

	}
	CATCH_UCLID_EXCEPTION("ELI02397")
	CATCH_COM_EXCEPTION("ELI02398")
	CATCH_UNEXPECTED_EXCEPTION("ELI02399")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngleInputValidator::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	m_ipExecuter->SetResultLogger(pLogger);

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAngleInputValidator::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	m_ipExecuter = pInteractiveTestExecuter;

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CAngleInputValidator::validateLicense()
{
	static const unsigned long ANGLEIV_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( ANGLEIV_COMPONENT_ID, "ELI02619", "Angle Input Validator" );
}
//-------------------------------------------------------------------------------------------------
