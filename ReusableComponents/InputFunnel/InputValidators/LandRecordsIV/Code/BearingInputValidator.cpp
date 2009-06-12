
#include "stdafx.h"
#include "LandRecordsIV.h"
#include "BearingInputValidator.h"
#include "InputTypes.h"

#include <Bearing.hpp>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CBearingInputValidator
//-------------------------------------------------------------------------------------------------
CBearingInputValidator::CBearingInputValidator()
: m_ipExecuter(NULL)
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearingInputValidator::InterfaceSupportsErrorInfo(REFIID riid)
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
STDMETHODIMP CBearingInputValidator::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pbstrComponentDescription = _bstr_t(gstrBEARING_INPUT_TYPE.c_str()).copy();

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearingInputValidator::raw_ValidateInput(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		*pbSuccessful = VARIANT_FALSE;
		
		// get the string out from pTextInput
		_bstr_t bstrInput = pTextInput->GetText();
		
		// validate the bearing input
		static Bearing bearing;
		bearing.resetVariables();
		bearing.evaluate(bstrInput);
		if (bearing.isValid())
		{
			*pbSuccessful = VARIANT_TRUE;
			
			CComQIPtr<IBearing> ipBearing;
			ipBearing.CoCreateInstance(__uuidof(Bearing));
			
			ipBearing->InitBearing(CComBSTR(bearing.asString().c_str()));
			
			// set validated input object
			pTextInput->SetValidatedInput(ipBearing);	
		}
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02560")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearingInputValidator::raw_GetInputType(BSTR * pstrInputType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	HRESULT ret = S_OK;
	try
	{
		validateLicense();

		ret = raw_GetComponentDescription(pstrInputType);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02563")
	
	return ret;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearingInputValidator::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CBearingInputValidator::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearingInputValidator::raw_RunInteractiveTests()
{
	try
	{
		// Path will be either Debug or Release directory
		string strDir = getModuleDirectory(_Module.m_hInst) + "\\";

		// bring up the exe 
		string strEXEFile = strDir + string("..\\VBTest\\TestLandRecordsIV.exe");
		runEXE(strEXEFile);

		string strITCFile = strDir + string("..\\Test\\TestBearingIV.itc");

		_bstr_t bstrITCFile(strITCFile.c_str());
		m_ipExecuter->ExecuteITCFile(bstrITCFile);
	}
	CATCH_UCLID_EXCEPTION("ELI02407")
	CATCH_COM_EXCEPTION("ELI02408")
	CATCH_UNEXPECTED_EXCEPTION("ELI02409")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearingInputValidator::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	m_ipExecuter->SetResultLogger(pLogger);
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBearingInputValidator::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	m_ipExecuter = pInteractiveTestExecuter;
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CBearingInputValidator::validateLicense()
{
	static const unsigned long BEARINGIV_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( BEARINGIV_COMPONENT_ID, "ELI02621", "Bearing Input Validator" );
}
//-------------------------------------------------------------------------------------------------
