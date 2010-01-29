// RegExprInputValidator.cpp : Implementation of CRegExprInputValidator
#include "stdafx.h"
#include "RegExprIV.h"
#include "RegExprInputValidator.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CRegExprInputValidator
//-------------------------------------------------------------------------------------------------
CRegExprInputValidator::CRegExprInputValidator()
: m_bDirty(false)
{
	try
	{
		// Get a regular expression parser.
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI22291", m_ipMiscUtils != NULL);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI22278");
}
//-------------------------------------------------------------------------------------------------
CRegExprInputValidator::~CRegExprInputValidator()
{
	try
	{
		m_ipMiscUtils = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29484");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRegExprInputValidator,
		&IID_IInputValidator,
		&IID_ICategorizedComponent,
		&IID_IPersistStream,
		&IID_IMustBeConfiguredObject,
		&IID_ICopyableObject,
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
// IInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::raw_ValidateInput(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate the input text
		ITextInputPtr ipTextInput(pTextInput);
		*pbSuccessful = getParser()->StringMatchesPattern(ipTextInput->GetText());

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03865")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::raw_GetInputType(BSTR * pstrInputType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pstrInputType = _bstr_t(m_strInputTypeName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03866")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19629", pbstrComponentDescription != NULL)

		// Retrieve definition
		*pbstrComponentDescription = _bstr_t("Regular expression").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03715")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IRegExprInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::get_Pattern(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI29481", pVal != NULL);

		*pVal = _bstr_t(m_strPattern.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03867")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::put_Pattern(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_strPattern = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03868")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::get_IgnoreCase(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI29482", pVal != NULL);

		*pVal = asVariantBool(m_bIgnoreCase);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03869")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::put_IgnoreCase(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bIgnoreCase = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03870")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::SetInputType(BSTR strInputTypeName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_strInputTypeName = asString( strInputTypeName );

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03871")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::GetInputType(BSTR* strInputTypeName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI29483", strInputTypeName != NULL);

		*strInputTypeName = _bstr_t(m_strInputTypeName.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08309")
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_REGEXPRIVLib::IRegExprInputValidatorPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08303", ipSource != NULL);

		m_bIgnoreCase = asCppBool(ipSource->IgnoreCase);
		m_strPattern = asString(ipSource->Pattern);
		m_strInputTypeName = asString(ipSource->GetInputType());

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08310");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_RegExprInputValidator);
		ASSERT_RESOURCE_ALLOCATION("ELI08370", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04866");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pbValue = asVariantBool(!m_strPattern.empty() && !m_strInputTypeName.empty()); 

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04868");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_RegExprInputValidator;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the variables first
		m_strInputTypeName = "";
		m_strPattern = "";
		m_bIgnoreCase = false;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read( &nDataLength, sizeof(nDataLength), NULL );
		ByteStream data( nDataLength );
		pStream->Read( data.getData(), nDataLength, NULL );
		ByteStreamManipulator dataReader( ByteStreamManipulator::kRead, data );

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;
		if (nDataVersion >= 1)
		{
			dataReader >> m_strInputTypeName;
			dataReader >> m_strPattern;
			dataReader >> m_bIgnoreCase;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04665");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Set current version number
		const unsigned long nCurrentVersion = 1;

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << nCurrentVersion;
		dataWriter << m_strInputTypeName;

		// Write the regex pattern to the bytestream
		dataWriter << m_strPattern;

		// Write the case-sensitive flag to the bytestream
		dataWriter << m_bIgnoreCase;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04667");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CRegExprInputValidator::raw_RunAutomatedTests(IVariantVector * pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Test pattern and case sensitivity
		doTest1();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10481")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store pointer to the logger
		m_ipLogger = pLogger;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10482")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprInputValidator::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
UCLID_REGEXPRIVLib::IRegExprInputValidatorPtr CRegExprInputValidator::getThisAsCOMPtr()
{
	UCLID_REGEXPRIVLib::IRegExprInputValidatorPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI16970", ipThis != NULL);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
void CRegExprInputValidator::doTest1()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "1";
	_bstr_t	bstrTestCaseDescription = "Test pattern and case-sensitivity";

	//////////////////////
	// Start the test case
	//////////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	bool			bTestSuccess = true;
	VARIANT_BOOL	vbResult = VARIANT_FALSE;

	/////////////////////////////////
	// Set regular expression pattern
	//   for 3 or more digits
	/////////////////////////////////
	m_strPattern = "\\d{3,}\\s*With";

	/////////////////////////////////////////////
	// Prepare ITextInput object with test string
	/////////////////////////////////////////////
	ITextInputPtr ipTextInput( CLSID_TextInput );
	ASSERT_RESOURCE_ALLOCATION( "ELI10483", ipTextInput != NULL );
	ipTextInput->InitTextInput( NULL, _bstr_t( "122569 with" ) );

	IInputValidatorPtr ipThis = getThisAsCOMPtr();
	ASSERT_RESOURCE_ALLOCATION("ELI22013", ipThis != NULL);

	// Check validity - expect success
	vbResult = ipThis->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"122569 with\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Set case sensitivity flag
	m_bIgnoreCase = false;

	// Retest validity - expect failure
	vbResult = ipThis->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"122569 with\" passed case-sensitive validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	/////////////////////
	// End this test case
	/////////////////////
	ipTextInput = NULL;
	m_ipLogger->EndTestCase( bTestSuccess ? VARIANT_TRUE : VARIANT_FALSE );
}
//-------------------------------------------------------------------------------------------------
IRegularExprParserPtr CRegExprInputValidator::getParser()
{
	try
	{
		IRegularExprParserPtr ipParser =
			m_ipMiscUtils->GetNewRegExpParserInstance("RegExprInputValidator");
		ASSERT_RESOURCE_ALLOCATION("ELI22277", ipParser != NULL);

		// Set the case sensitivity and pattern
		ipParser->IgnoreCase = asVariantBool(m_bIgnoreCase);
		ipParser->Pattern = m_strPattern.c_str();

		return ipParser;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29485");
}
//-------------------------------------------------------------------------------------------------
void CRegExprInputValidator::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI03864", 
		"Regular Expression Input Validator" );
}
//-------------------------------------------------------------------------------------------------
