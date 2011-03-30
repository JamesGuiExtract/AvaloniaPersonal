// DateInputValidator.cpp : Implementation of CDateInputValidator
#include "stdafx.h"
#include "GeneralIV.h"
#include "DateInputValidator.h"
#include "GeneralInputTypes.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CDateInputValidator
//-------------------------------------------------------------------------------------------------
CDateInputValidator::CDateInputValidator()
:m_bDirty(false)
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDateInputValidator,
		&IID_IPersistStream,
		&IID_ICopyableObject,
		&IID_IInputValidator,
		&IID_ITestableComponent
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
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19616", pstrComponentDescription != __nullptr)

		// Retrieve definition
		*pstrComponentDescription = _bstr_t( 
			gstrDATE_INPUT_TYPE.c_str() ).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03822")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_DateInputValidator);
		ASSERT_RESOURCE_ALLOCATION("ELI08365", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04870");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_DateInputValidator;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the variables first

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
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04670");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::Save(IStream *pStream, BOOL fClearDirty)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04671");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::raw_ValidateInput(ITextInput * pTextInput, 
													VARIANT_BOOL * pbSuccessful)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbSuccessful == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		ITextInputPtr ipTextInput(pTextInput);
		ASSERT_ARGUMENT("ELI10152", ipTextInput != __nullptr);

		// Retrieve string
		string	strInput = ipTextInput->GetText();;

		try
		{
			// put the string into a VARIANT
			_variant_t _vValue;
			_vValue.vt = VT_BSTR;
			_vValue.bstrVal = _bstr_t(strInput.c_str());
			// Get the date from the VARIANT
			COleDateTime dDate(_vValue);

			// String parsed successfully, now check validity
			if (dDate.GetStatus() == COleDateTime::valid)
			{
				*pbSuccessful = VARIANT_TRUE;

				// Modify the actual text string to specific format
				CString	zTemp = dDate.Format( "%m/%d/%Y" );
				_bstr_t	bstrNewText( zTemp.operator LPCTSTR() );
				ipTextInput->SetText( bstrNewText );
			}
			else
			{
				*pbSuccessful = VARIANT_FALSE;
			}

			// NOTE: ITextInput::SetValidatedInput() will not be called
		}
		catch (...)
		{
			// Failed to convert entire string to date
			// therefore ITextInput object is not valid
			*pbSuccessful = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03824")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::raw_GetInputType(BSTR * pstrInputType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	HRESULT hr = S_OK;

	try
	{
		ASSERT_ARGUMENT("ELI19617", pstrInputType != __nullptr)

		// Check license
		validateLicense();

		// Just provide the component description
		hr = raw_GetComponentDescription( pstrInputType );

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03823")
}

//-------------------------------------------------------------------------------------------------
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::raw_RunAutomatedTests(IVariantVector * pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Run test of MM/DD/YY and MM/DD/YYYY
		doTest1();

		// Run test of Month DD, YYYY
		doTest2();

		// Run test of DD Month YYYY
		doTest3();

		// Run test of leap day items
		doTest4();

		// Run test of MM DD YY with various delimiters
		doTest5();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10470")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store pointer to the logger
		m_ipLogger = pLogger;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10471")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDateInputValidator::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CDateInputValidator::doTest1()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "1";
	_bstr_t	bstrTestCaseDescription = "Test MM/DD/YY and MM/DD/YYYY formats";

	//////////////////////
	// Start the test case
	//////////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	bool			bTestSuccess = true;
	VARIANT_BOOL	vbResult = VARIANT_FALSE;

	/////////////////////////////////////////////
	// Prepare ITextInput object with test string
	/////////////////////////////////////////////
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10472", ipTextInput != __nullptr);
	ipTextInput->InitTextInput( NULL, _bstr_t( "12/25/69" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"12/25/69\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "12/35/69" ) );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"12/35/69\" passed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "12/1/1969" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"12/1/1969\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "1/01/2000" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"1/01/2000\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "13/13/69" ) );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"13/13/69\" passed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "12/25/19069" ) );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"12/25/19069\" passed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	/////////////////////
	// End this test case
	/////////////////////
	ipTextInput = __nullptr;
	m_ipLogger->EndTestCase( bTestSuccess ? VARIANT_TRUE : VARIANT_FALSE );
}
//-------------------------------------------------------------------------------------------------
void CDateInputValidator::doTest2()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "2";
	_bstr_t	bstrTestCaseDescription = "Test Month DD, YYYY format";

	//////////////////////
	// Start the test case
	//////////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	bool			bTestSuccess = true;
	VARIANT_BOOL	vbResult = VARIANT_FALSE;

	/////////////////////////////////////////////
	// Prepare ITextInput object with test string
	/////////////////////////////////////////////
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10473", ipTextInput != __nullptr);
	ipTextInput->InitTextInput( NULL, _bstr_t( "December 25, 1969" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"December 25, 1969\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "January 35, 1969" ) );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"January 35, 1969\" passed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "February 01, 1969" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"February 01, 1969\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "March 1, 2000" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"March 1, 2000\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "Marcher 1,2000" ) );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"Marcher 1,2000\" passed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "Actual 11, 2005" ) );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"Actual 11, 2005\" passed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	/////////////////////
	// End this test case
	/////////////////////
	ipTextInput = __nullptr;
	m_ipLogger->EndTestCase( bTestSuccess ? VARIANT_TRUE : VARIANT_FALSE );
}
//-------------------------------------------------------------------------------------------------
void CDateInputValidator::doTest3()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "3";
	_bstr_t	bstrTestCaseDescription = "Test DD Month YYYY format";

	//////////////////////
	// Start the test case
	//////////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	bool			bTestSuccess = true;
	VARIANT_BOOL	vbResult = VARIANT_FALSE;

	/////////////////////////////////////////////
	// Prepare ITextInput object with test string
	/////////////////////////////////////////////
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10474", ipTextInput != __nullptr);
	ipTextInput->InitTextInput( NULL, _bstr_t( "25 May 1969" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"25 May 1969\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "35 June 1969" ) );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"35 June 1969\" passed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "01 June 1969" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"01 June 1969\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "1 July 2000" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"1 July 2000\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "31 September 2000" ) );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"31 September 2000\" passed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "11 Augustus 2005" ) );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"11 Augustus 2005\" passed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	/////////////////////
	// End this test case
	/////////////////////
	ipTextInput = __nullptr;
	m_ipLogger->EndTestCase( bTestSuccess ? VARIANT_TRUE : VARIANT_FALSE );
}
//-------------------------------------------------------------------------------------------------
void CDateInputValidator::doTest4()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "4";
	_bstr_t	bstrTestCaseDescription = "Test Leap Day items";

	//////////////////////
	// Start the test case
	//////////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	bool			bTestSuccess = true;
	VARIANT_BOOL	vbResult = VARIANT_FALSE;

	/////////////////////////////////////////////
	// Prepare ITextInput object with test string
	/////////////////////////////////////////////
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10477", ipTextInput != __nullptr);
	ipTextInput->InitTextInput( NULL, _bstr_t( "29 February 2004" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"29 February 2004\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "29 February 2005" ) );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"29 February 2005\" passed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "February 29 2000" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"February 29 2000\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "February 29 1900" ) );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"February 29 1900\" passed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "F 29 2000" ) );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"F 29 2000\" passed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	/////////////////////
	// End this test case
	/////////////////////
	ipTextInput = __nullptr;
	m_ipLogger->EndTestCase( bTestSuccess ? VARIANT_TRUE : VARIANT_FALSE );
}
//-------------------------------------------------------------------------------------------------
void CDateInputValidator::doTest5()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "5";
	_bstr_t	bstrTestCaseDescription = "Test MM DD YY with various delimiters";

	//////////////////////
	// Start the test case
	//////////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	bool			bTestSuccess = true;
	VARIANT_BOOL	vbResult = VARIANT_FALSE;

	/////////////////////////////////////////////
	// Prepare ITextInput object with test string
	/////////////////////////////////////////////
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10478", ipTextInput != __nullptr);
	ipTextInput->InitTextInput( NULL, _bstr_t( "05-25-69" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"05-25-69\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "7?17%1904" ) );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"7?17%1904\" passed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "7 . 17 . 1904" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "\"7 . 17 . 1904\" passed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "7 17 1904" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"7 17 1904\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Another test string
	ipTextInput->InitTextInput( NULL, _bstr_t( "09 30 09" ) );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "\"09 30 09\" failed validation." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	/////////////////////
	// End this test case
	/////////////////////
	ipTextInput = __nullptr;
	m_ipLogger->EndTestCase( bTestSuccess ? VARIANT_TRUE : VARIANT_FALSE );
}
//-------------------------------------------------------------------------------------------------
IInputValidatorPtr CDateInputValidator::getThisAsInputValidatorPtr()
{
	IInputValidatorPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI22009", ipThis != __nullptr);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
void CDateInputValidator::validateLicense()
{
	static const unsigned long DATEIV_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( DATEIV_COMPONENT_ID, "ELI03821",
		"Date Input Validator" );
}
//-------------------------------------------------------------------------------------------------
