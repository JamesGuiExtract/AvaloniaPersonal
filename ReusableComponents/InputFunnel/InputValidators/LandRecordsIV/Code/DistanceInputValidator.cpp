
#include "stdafx.h"
#include "LandRecordsIV.h"
#include "DistanceInputValidator.h"
#include "InputTypes.h"

#include <LicenseMgmt.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CDistanceInputValidator
//-------------------------------------------------------------------------------------------------
CDistanceInputValidator::CDistanceInputValidator()
: m_ipExecuter(NULL)
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceInputValidator::InterfaceSupportsErrorInfo(REFIID riid)
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
STDMETHODIMP CDistanceInputValidator::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pbstrComponentDescription = _bstr_t(gstrDISTANCE_INPUT_TYPE.c_str()).copy();

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceInputValidator::raw_ValidateInput(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pbSuccessful = VARIANT_FALSE;
		
		// get the string out from pTextInput
		_bstr_t bstrInput = pTextInput->GetText();
		
		// validate the distance input
		m_distance.evaluate(string(bstrInput));
		if (m_distance.isValid())
		{
			// Also check for Distance > 0
			double dTest = m_distance.getDistanceInUnit( kFeet );
			if (dTest > 0.0)
			{
				*pbSuccessful = VARIANT_TRUE;
				
				CComPtr<IDistance> ipDistance;
				ipDistance.CoCreateInstance(__uuidof(Distance));
				// no matter what's in the string, always set it to kUnknownUnit. 
				// Such that it will be Distance (the COM Object) object's resposibility
				// to convert the distance value according to app specified output unit.
				ipDistance->InitDistance( CComBSTR( 
					m_distance.getOriginalInputString().c_str() ) );
				
				// set validated input object
				pTextInput->SetValidatedInput(ipDistance);	
			}
		}
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02575")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceInputValidator::raw_GetInputType(BSTR * pstrInputType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	HRESULT ret = S_OK;
	try
	{
		validateLicense();

		ret = raw_GetComponentDescription(pstrInputType);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02578")

	return ret;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceInputValidator::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CDistanceInputValidator::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceInputValidator::raw_RunInteractiveTests()
{
	try
	{
		// Path will be either Debug or Release directory
		string strDir = getModuleDirectory(_Module.m_hInst) + "\\";

		// bring up the exe 
		string strEXEFile = strDir + string("..\\VBTest\\TestLandRecordsIV.exe");
		runEXE(strEXEFile);

		string strITCFile = strDir + string("..\\Test\\TestDistanceIV.itc");

		_bstr_t bstrITCFile(strITCFile.c_str());
		m_ipExecuter->ExecuteITCFile(bstrITCFile);
	}
	CATCH_UCLID_EXCEPTION("ELI02410")
	CATCH_COM_EXCEPTION("ELI02411")
	CATCH_UNEXPECTED_EXCEPTION("ELI02412")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceInputValidator::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	m_ipExecuter->SetResultLogger(pLogger);

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceInputValidator::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	m_ipExecuter = pInteractiveTestExecuter;

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CDistanceInputValidator::validateLicense()
{
	static const unsigned long DISTANCEIV_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( DISTANCEIV_COMPONENT_ID, "ELI02623",
		"Distance Input Validator" );
}
//-------------------------------------------------------------------------------------------------
