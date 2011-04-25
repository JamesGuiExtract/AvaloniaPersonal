// InputValidator.cpp : Implementation of CInputValidator
#include "stdafx.h"
#include "IFCore.h"
#include "TextInputValidator.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInputValidator,
		&IID_ICategorizedComponent,
		&IID_IPersistStream,
		&IID_ILicensedComponent,
		&IID_ITestableComponent,
		&IID_ITextInputValidator
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// CTextInputValidator
//-------------------------------------------------------------------------------------------------
CTextInputValidator::CTextInputValidator()
: m_ipResultLogger(NULL), m_bEmptyInputOK(false), m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CTextInputValidator::~CTextInputValidator()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16467");
}

//-------------------------------------------------------------------------------------------------
// IInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::ValidateInput(ITextInput *pTextInput, VARIANT_BOOL *pbSuccessful)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pbSuccessful = VARIANT_TRUE;
		
		// if empty input is OK, then input is always valid as far as this
		// object is concerned
		if (m_bEmptyInputOK)
		{
			return S_OK;
		}

		// wrap pTextInput in a smart pointer
		UCLID_INPUTFUNNELLib::ITextInputPtr ipTextInput(pTextInput);
		ASSERT_RESOURCE_ALLOCATION("ELI16897", ipTextInput != __nullptr);

		// check if the input text is empty
		_bstr_t bstrText = ipTextInput->GetText();
		if (bstrText.length() == 0)
		{
			*pbSuccessful = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02525")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::GetInputType(BSTR *pstrInputType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	HRESULT ret = S_OK;
	try
	{
		ASSERT_ARGUMENT("ELI19627", pstrInputType != __nullptr)

		// Return this input validator description
		ret = raw_GetComponentDescription(pstrInputType);

		return ret;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02528")
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{	
	try
	{
		ASSERT_ARGUMENT("ELI19626", pbstrComponentDescription != __nullptr)

		*pbstrComponentDescription = _bstr_t("Text").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02531")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ITextInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::get_EmptyInputOK(VARIANT_BOOL *pbVal)
{
	try
	{
		validateLicense();
		
		*pbVal = m_bEmptyInputOK ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11351")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::put_EmptyInputOK(VARIANT_BOOL bVal)
{
	try
	{
		validateLicense();
		
		m_bEmptyInputOK = (bVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11352")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_TextInputValidator;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// TODO: When this object has member variables, code needs
		// to be added here to load those member variable values from a stream
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read( &nDataLength, sizeof(nDataLength), NULL );
		ByteStream data( nDataLength );
		pStream->Read( data.getData(), nDataLength, NULL );
		ByteStreamManipulator dataReader( ByteStreamManipulator::kRead, data );

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04696");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// TODO: When this object has member variables, code needs
		// to be added here to save those member variable values to a stream
		// Set current version number
		const unsigned long nCurrentVersion = 1;

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << nCurrentVersion;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04695");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CTextInputValidator::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	try
	{
		// first run the test with invalid license
	//	executeTest2();
		// then test with valid license
		executeTest1();

		executeTest3();
	}
	CATCH_UCLID_EXCEPTION("ELI02423")
	CATCH_COM_EXCEPTION("ELI02424")
	CATCH_UNEXPECTED_EXCEPTION("ELI02425")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTextInputValidator::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CTextInputValidator::executeTest1()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestTextInputIV01"), _bstr_t("Is Licensed (with a valid license file)"), kAutomatedTestCase); 

	// init license key
//	LicenseManagement::initializeLicense("090000004475616E5F57616E67040000007465737404000000746573746DB4A03C0E00000001000000016DB4A03C02000000016DB4A03C03000000016DB4A03C04000000016DB4A03C05000000016DB4A03C06000000016DB4A03C07000000016DB4A03C08000000016DB4A03C09000000016DB4A03C0A000000016DB4A03C0B000000016DB4A03C0C000000016DB4A03C16000000016DB4A03C17000000016DB4A03C");

	VARIANT_BOOL bIsLicensed;
	HRESULT hr = raw_IsLicensed(&bIsLicensed);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02426", "Failed to check whether the component is licensed.");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	VARIANT_BOOL bValid = bIsLicensed;

	m_ipResultLogger->EndTestCase(bValid);
}
//-------------------------------------------------------------------------------------------------
void CTextInputValidator::executeTest2()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestTextInputIV02"), _bstr_t("Is Licensed (with an expired key)"), kAutomatedTestCase); 

	// init with an invalid license key
//	LicenseManagement::initializeLicense("090000004475616E5F57616E670100000074010000007426BAA03C0E0000000100000000DF341F350200000000DF341F350300000000DF341F350400000000DF341F350500000000DF341F350600000000DF341F350700000000DF341F350800000000DF341F350900000000DF341F350A00000000DF341F350B00000000DF341F350C00000000DF341F351600000000DF341F351700000000DF341F35");

	VARIANT_BOOL bIsLicensed;
	HRESULT hr = raw_IsLicensed(&bIsLicensed);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02427", "Failed to check whether the component is licensed.");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	// if the license is valid, then this test fails
	VARIANT_BOOL bValid = !bIsLicensed ? VARIANT_TRUE : VARIANT_FALSE;

	m_ipResultLogger->EndTestCase(bValid);
}
//-------------------------------------------------------------------------------------------------
void CTextInputValidator::executeTest3()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestTextInputIV03"), _bstr_t("Get Input Type"), kAutomatedTestCase); 

	CComBSTR bstrInputType;
	HRESULT hr = GetInputType(&bstrInputType);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02428", "Failed to get input type.");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	VARIANT_BOOL bValid = VARIANT_FALSE;

	if (CComBSTR("Text") == bstrInputType)
	{
		bValid = VARIANT_TRUE;
	}

	m_ipResultLogger->EndTestCase(bValid);
}
//-------------------------------------------------------------------------------------------------
void CTextInputValidator::validateLicense()
{
	static const unsigned long TEXT_INPUT_IV_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( TEXT_INPUT_IV_COMPONENT_ID, "ELI02605", "Text Input Validator" );
}
//-------------------------------------------------------------------------------------------------
