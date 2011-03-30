//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DoubleInputValidator.cpp
//
// PURPOSE:	Implementation of CDoubleInputValidator class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//-------------------------------------------------------------------------------------------------

#include "stdafx.h"
#include "GeneralIV.h"
#include "DoubleInputValidator.h"
#include "GeneralInputTypes.h"

#include <LicenseMgmt.h>
#include <cpputil.h>
#include <mathUtil.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

#include <float.h>

//-------------------------------------------------------------------------------------------------
// CDoubleInputValidator
//-------------------------------------------------------------------------------------------------
CDoubleInputValidator::CDoubleInputValidator()
:m_bDirty(false)
{
	// Set defaults
	setDefaults();
}
//-------------------------------------------------------------------------------------------------
CDoubleInputValidator::~CDoubleInputValidator()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// Force destruction of member objects within this scope so that
		// all destruction happens within the scope of the correct AFX state
		m_ipLogger = __nullptr;
		m_ipExecuter = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16442");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDoubleInputValidator,
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
STDMETHODIMP CDoubleInputValidator::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CDoubleInputValidator::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19618", pbstrComponentDescription != __nullptr)

		// Retrieve definition
		*pbstrComponentDescription = _bstr_t(gstrDOUBLE_INPUT_TYPE.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03735")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_GENERALIVLib::IDoubleInputValidatorPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08298", ipSource != __nullptr);
		
		m_bIncludeMaximum = (ipSource->GetIncludeMaxInRange()==VARIANT_TRUE) ? true : false;
		m_bIncludeMinimum = (ipSource->GetIncludeMinInRange()==VARIANT_TRUE) ? true : false;
		m_bMaxDefined = (ipSource->GetHasMax()==VARIANT_TRUE) ? true : false;
		m_bMinDefined = (ipSource->GetHasMin()==VARIANT_TRUE) ? true : false;

		m_dMaximum = ipSource->GetMax();
		m_dMinimum = ipSource->GetMin();

		m_bNegativeAllowed = (ipSource->GetNegativeAllowed()==VARIANT_TRUE) ? true : false;
		m_bZeroAllowed = (ipSource->GetZeroAllowed()==VARIANT_TRUE) ? true : false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08299");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_DoubleInputValidator);
		ASSERT_RESOURCE_ALLOCATION("ELI08366", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04873");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::raw_ValidateInput(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful)
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
		ASSERT_ARGUMENT("ELI10153", ipTextInput != __nullptr);

		// Retrieve string
		string	strInput = ipTextInput->GetText();
		if (strInput.empty())
		{
			*pbSuccessful = VARIANT_FALSE;
			return S_OK;
		}

		try
		{
			bool	bOkay = true;

			// Convert string to double
			m_dValue = asDouble( strInput );

			// Check value against limits
			bOkay = checkLimits();

			// Provide result to caller
			if (bOkay)
			{
				// String parsed successfully and did not fail limits
				*pbSuccessful = VARIANT_TRUE;

				// NOTE: ITextInput::SetValidatedInput() will not be called
			}
			else
			{
				// String parsed successfully but failed limits
				*pbSuccessful = VARIANT_FALSE;
			}
		}
		catch (...)
		{
			// Failed to convert entire string to double
			// therefore ITextInput object is not valid
			*pbSuccessful = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03759")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::raw_GetInputType(BSTR * pstrInputType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	HRESULT hr = S_OK;

	try
	{
		ASSERT_ARGUMENT("ELI19619", pstrInputType != __nullptr)

		// Check license
		validateLicense();

		// Just provide the component description
		hr = raw_GetComponentDescription( pstrInputType );

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03736")
}

//-------------------------------------------------------------------------------------------------
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Run test of minimum
		doTest1();

		// Run test of maximum
		doTest2();

		// Run test of zero
		doTest3();

		// Run test of negative
		doTest4();

		// Run test of including minimums
		doTest5();

		// Run test of including maximums
		doTest6();

		// Run format tests
		doTest7();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03738")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::raw_RunInteractiveTests()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Path will be either Debug or Release directory
		string strDir = getModuleDirectory(_Module.m_hInst) + "\\";

		// Bring up the exe 
		string strEXEFile = strDir + string( "..\\VBTest\\TestGeneralIV.exe" );
		runEXE( strEXEFile );

		string strITCFile = strDir + string("..\\Test Files\\InteractiveTest\\TestDoubleIV.itc");

		_bstr_t bstrITCFile( strITCFile.c_str() );
		m_ipExecuter->ExecuteITCFile( bstrITCFile );

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03739")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store pointer to the logger
		m_ipLogger = pLogger;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03740")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store pointer
		m_ipExecuter = pInteractiveTestExecuter;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03707")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IDoubleInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::get_Min(double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pVal == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// Provide current minimum
		*pVal = m_dMinimum;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03741")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::put_Min(double newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check signed/unsigned state and new value
		if (!m_bNegativeAllowed && newVal < 0)
		{
			// Throw exception - Invalid minimum
			UCLIDException uclidException("ELI03757", 
				"Invalid double minimum value, negative numbers are not allowed.");
			uclidException.addDebugInfo("Requested minimum", newVal);
			throw uclidException;
		}
		else
		{
			// Store the new limit
			m_dMinimum = newVal;
		}

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03742")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::get_Max(double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pVal == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// Provide current maximum
		*pVal = m_dMaximum;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03743")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::put_Max(double newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check signed/unsigned state
		if (m_bNegativeAllowed)
		{
			// Store the new limit
			m_dMaximum = newVal;
		}
		else
		{
			if (newVal < 0)
			{
				// Throw exception - Invalid maximum
				UCLIDException uclidException("ELI03758", 
					"Invalid double maximum value, negative numbers are not allowed.");
				uclidException.addDebugInfo("Requested maximum", newVal);
				throw uclidException;
			}
			else
			{
				// Store the new limit
				m_dMaximum = newVal;
			}
		}

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03744")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::get_HasMin(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pVal == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// Provide flag
		*pVal = m_bMinDefined ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03745")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::put_HasMin(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store flag
		if (newVal == VARIANT_TRUE)
		{
			m_bMinDefined = true;
		}
		else
		{
			m_bMinDefined = false;
		}

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03746")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::get_HasMax(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pVal == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// Provide flag
		*pVal = m_bMaxDefined ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03747")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::put_HasMax(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store flag
		if (newVal == VARIANT_TRUE)
		{
			m_bMaxDefined = true;
		}
		else
		{
			m_bMaxDefined = false;
		}

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03748")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::get_ZeroAllowed(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pVal == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// Provide flag
		*pVal = m_bZeroAllowed ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03749")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::put_ZeroAllowed(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store flag
		if (newVal == VARIANT_TRUE)
		{
			m_bZeroAllowed = true;
		}
		else
		{
			m_bZeroAllowed = false;
		}

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03750")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::get_NegativeAllowed(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pVal == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// Provide flag
		*pVal = m_bNegativeAllowed ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03751")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::put_NegativeAllowed(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store flag
		if (newVal == VARIANT_TRUE)
		{
			m_bNegativeAllowed = true;
		}
		else
		{
			m_bNegativeAllowed = false;
		}

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03752")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::get_IncludeMinInRange(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pVal == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// Provide flag
		*pVal = m_bIncludeMinimum ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03753")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::put_IncludeMinInRange(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store flag
		if (newVal == VARIANT_TRUE)
		{
			m_bIncludeMinimum = true;
		}
		else
		{
			m_bIncludeMinimum = false;
		}

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03754")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::get_IncludeMaxInRange(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pVal == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// Provide flag
		*pVal = m_bIncludeMaximum ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03755")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::put_IncludeMaxInRange(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store flag
		if (newVal == VARIANT_TRUE)
		{
			m_bIncludeMaximum = true;
		}
		else
		{
			m_bIncludeMaximum = false;
		}

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03756")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_DoubleInputValidator;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the variables first
		m_bMinDefined = false;
		m_bMaxDefined = false;
		m_bZeroAllowed = false;
		m_bNegativeAllowed = false;
		m_bIncludeMinimum = false;
		m_bIncludeMaximum = false;
		m_dMinimum = 0.0;
		m_dMaximum = 0.0;

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
			dataReader >> m_bMinDefined;
			dataReader >> m_bMaxDefined;
			dataReader >> m_bZeroAllowed;
			dataReader >> m_bNegativeAllowed;
			dataReader >> m_bIncludeMinimum;
			dataReader >> m_bIncludeMaximum;
			dataReader >> m_dMinimum;
			dataReader >> m_dMaximum;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04672");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_bMinDefined;
		dataWriter << m_bMaxDefined;
		dataWriter << m_bZeroAllowed;
		dataWriter << m_bNegativeAllowed;
		dataWriter << m_bIncludeMinimum;
		dataWriter << m_bIncludeMaximum;
		dataWriter << m_dMinimum;
		dataWriter << m_dMaximum;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04673");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDoubleInputValidator::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
bool CDoubleInputValidator::checkLimits()
{
	bool bReturn = true;

	// First test for infinity
	if (!_finite( m_dValue ))
	{
		bReturn = false;
	}

	// Next test for Not A Number
	if (_isnan( m_dValue ))
	{
		bReturn = false;
	}

	// Check against minimum limit
	if (m_bMinDefined)
	{
		// Check boundary condition
		if (m_bIncludeMinimum)
		{
			// NOTE: m_dValue == m_dMinimum is valid
			if (m_dValue < m_dMinimum)
			{
				bReturn = false;
			}
		}
		else
		{
			// NOTE: m_dValue == m_dMinimum is invalid
			if (m_dValue <= m_dMinimum)
			{
				bReturn = false;
			}
		}
	}

	// Check against maximum limit
	if (m_bMaxDefined)
	{
		// Check boundary condition
		if (m_bIncludeMaximum)
		{
			// NOTE: m_dValue == m_dMaximum is valid
			if (m_dValue > m_dMaximum)
			{
				bReturn = false;
			}
		}
		else
		{
			// NOTE: m_dValue == m_dMaximum is invalid
			if (m_dValue >= m_dMaximum)
			{
				bReturn = false;
			}
		}
	}

	// Check negative limit
	if (!m_bNegativeAllowed && m_dValue < 0.0)
	{
		bReturn = false;
	}

	// Check zero
	if (!m_bZeroAllowed && MathVars::isZero( m_dValue ))
	{
		bReturn = false;
	}

	// Return result
	return bReturn;
}
//-------------------------------------------------------------------------------------------------
void CDoubleInputValidator::setDefaults()
{
	// Set defaults
	m_dMinimum = -1.7976931348623158e+308;
	m_dMaximum = 1.7976931348623158e+308;

	m_bMinDefined = false;
	m_bMaxDefined = false;
	m_bZeroAllowed = true;
	m_bNegativeAllowed = true;
	m_bIncludeMinimum = true;
	m_bIncludeMaximum = true;
}
//-------------------------------------------------------------------------------------------------
void CDoubleInputValidator::doTest1()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "1";
	_bstr_t	bstrTestCaseDescription = "Test Minimum";

	//////////////////////
	// Start the test case
	//////////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	bool			bTestSuccess = true;
	VARIANT_BOOL	vbResult;

	//////////////////////
	// Do the Minimum test
	//////////////////////
	setDefaults();

	// Check for a minimum - expect FALSE
	get_HasMin( &vbResult );
	if (vbResult == VARIANT_TRUE)
	{
		// If True, leave a note
		string	strNote = string( "Validator reports HasMin = True." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Set a minimum value
	put_Min( 10.0 );

	// Prepare ITextInput object with test string
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10154", ipTextInput != __nullptr);
	ipTextInput->InitTextInput( NULL, _bstr_t("5.0") );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "String below minimum failed without HasMin." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;	
	}

	// Set HasMin property
	put_HasMin( VARIANT_TRUE );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "String below minimum succeeded with HasMin." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	/////////////////////
	// End this test case
	/////////////////////
	ipTextInput = __nullptr;
	if (bTestSuccess)
	{
		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void CDoubleInputValidator::doTest2()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "2";
	_bstr_t	bstrTestCaseDescription = "Test Maximum";

	//////////////////////
	// Start the test case
	//////////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	bool			bTestSuccess = true;
	VARIANT_BOOL	vbResult;

	//////////////////////
	// Do the Maximum test
	//////////////////////
	setDefaults();

	// Check for a maximum - expect FALSE
	get_HasMax( &vbResult );
	if (vbResult == VARIANT_TRUE)
	{
		// If True, leave a note
		string	strNote = string( "Validator reports HasMax = True." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Set a maximum value
	put_Max( 10.0 );

	// Prepare ITextInput object with test string
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10156", ipTextInput != __nullptr);
	ipTextInput->InitTextInput( NULL, _bstr_t("15.0") );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "String above maximum failed without HasMax." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Set HasMax property
	put_HasMax( VARIANT_TRUE );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "String above maximum succeeded with HasMin." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	/////////////////////
	// End this test case
	/////////////////////
	ipTextInput = __nullptr;
	if (bTestSuccess)
	{
		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void CDoubleInputValidator::doTest3()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "3";
	_bstr_t	bstrTestCaseDescription = "Test Zero Included";

	//////////////////////
	// Start the test case
	//////////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	bool			bTestSuccess = true;
	VARIANT_BOOL	vbResult;

	///////////////////
	// Do the Zero test
	///////////////////
	setDefaults();

	// Check ZeroAllowed - expect TRUE
	get_ZeroAllowed( &vbResult );
	if (vbResult == VARIANT_FALSE)
	{
		// If False, leave a note
		string	strNote = string( "Validator reports ZeroAllowed = False." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Prepare ITextInput object with test string
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10155", ipTextInput != __nullptr);
	ipTextInput->InitTextInput( NULL, _bstr_t("0.0") );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "String at zero failed with ZeroAllowed." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Clear ZeroAllowed property
	put_ZeroAllowed( VARIANT_FALSE );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "String at zero succeeded without ZeroAllowed." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	/////////////////////
	// End this test case
	/////////////////////
	ipTextInput = __nullptr;
	if (bTestSuccess)
	{
		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void CDoubleInputValidator::doTest4()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "4";
	_bstr_t	bstrTestCaseDescription = "Test Negatives";

	//////////////////////
	// Start the test case
	//////////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	bool			bTestSuccess = true;
	VARIANT_BOOL	vbResult;

	///////////////////////
	// Do the Negative test
	///////////////////////
	setDefaults();

	// Check NegativeAllowed - expect TRUE
	get_NegativeAllowed( &vbResult );
	if (vbResult == VARIANT_FALSE)
	{
		// If False, leave a note
		string	strNote = string( "Validator reports NegativeAllowed = False." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Prepare ITextInput object with test string
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10157", ipTextInput != __nullptr);
	ipTextInput->InitTextInput( NULL, _bstr_t("-50.0") );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "String below zero failed with NegativeAllowed." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Clear NegativeAllowed property
	put_NegativeAllowed( VARIANT_FALSE );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "String below zero succeeded without NegativeAllowed." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	/////////////////////
	// End this test case
	/////////////////////
	ipTextInput = __nullptr;
	if (bTestSuccess)
	{
		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void CDoubleInputValidator::doTest5()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "5";
	_bstr_t	bstrTestCaseDescription = "Test Including Minimum";

	//////////////////////
	// Start the test case
	//////////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	bool			bTestSuccess = true;
	VARIANT_BOOL	vbResult;

	//////////////////////////////
	// Do the Include Minimum test
	//////////////////////////////
	setDefaults();

	// Check for including a minimum - expect TRUE
	get_IncludeMinInRange( &vbResult );
	if (vbResult == VARIANT_FALSE)
	{
		// If False, leave a note
		string	strNote = string( "Validator reports IncludeMinInRange = False." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Set a minimum value
	put_Min( 10.0 );

	// Set HasMin property
	put_HasMin( VARIANT_TRUE );

	// Prepare ITextInput object with test string
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10159", ipTextInput != __nullptr);

	ipTextInput->InitTextInput( NULL, _bstr_t("10.0") );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "String at minimum failed with IncludeMinInRange." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Clear IncludeMinInRange property
	put_IncludeMinInRange( VARIANT_FALSE );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "String at minimum succeeded without IncludeMinInRange." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	/////////////////////
	// End this test case
	/////////////////////
	ipTextInput = __nullptr;
	if (bTestSuccess)
	{
		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void CDoubleInputValidator::doTest6()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "6";
	_bstr_t	bstrTestCaseDescription = "Test Including Maximum";

	//////////////////////
	// Start the test case
	//////////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	bool			bTestSuccess = true;
	VARIANT_BOOL	vbResult;

	//////////////////////////////
	// Do the Include Maximum test
	//////////////////////////////
	setDefaults();

	// Check for including a maximum - expect TRUE
	get_IncludeMaxInRange( &vbResult );
	if (vbResult == VARIANT_FALSE)
	{
		// If False, leave a note
		string	strNote = string( "Validator reports IncludeMaxInRange = False." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Set a maximum value
	put_Max( 10.0 );

	// Set HasMax property
	put_HasMax( VARIANT_TRUE );

	// Prepare ITextInput object with test string
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10160", ipTextInput != __nullptr);
	
	ipTextInput->InitTextInput( NULL, _bstr_t("10.0") );

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "String at maximum failed with IncludeMaxInRange." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	// Clear IncludeMaxInRange property
	put_IncludeMaxInRange( VARIANT_FALSE );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "String at maximum succeeded without IncludeMaxInRange." );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}

	/////////////////////
	// End this test case
	/////////////////////
	ipTextInput = __nullptr;
	if (bTestSuccess)
	{
		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void CDoubleInputValidator::doTest7()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "7";
	_bstr_t	bstrTestCaseDescription = "Test String Formats";

	//////////////////////
	// Start the test case
	//////////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	bool			bTestSuccess = true;
	VARIANT_BOOL	vbResult;

	//////////////////////
	// Do the Format tests
	//////////////////////
	setDefaults();

	// Prepare ITextInput object with test string
	_bstr_t	bstrTest( "-1.02E+04" );

	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10158", ipTextInput != __nullptr);
	
	ipTextInput->InitTextInput(NULL, bstrTest);

	// Check validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "String format failed: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}
	else
	{
		string	strNote = string( "Simple scientific notation number succeeded: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );
	}

	// Next string
	bstrTest = "-1.04.05";
	ipTextInput->SetText( bstrTest );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "String format succeeded: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}
	else
	{
		string	strNote = string( "Number with two decimal points failed: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );
	}

	// Next string
	bstrTest = "100.1234567890123456789012345";
	ipTextInput->SetText( bstrTest );

	// Retest validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "String format failed: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}
	else
	{
		string	strNote = string( "Number with too many decimal digits succeeded: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );
	}

	// Next string
	bstrTest = "100";
	ipTextInput->SetText( bstrTest );

	// Retest validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If invalid, leave a note
		string	strNote = string( "String format failed: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}
	else
	{
		string	strNote = string( "Simple integer succeeded: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );
	}

	// Next string
	bstrTest = "-1.04G+04";
	ipTextInput->SetText( bstrTest );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "String format succeeded: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}
	else
	{
		string	strNote = string( "Number with invalid exponent indicator failed: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );
	}

	// Next string
	bstrTest = "-1.04E+444";
	ipTextInput->SetText( bstrTest );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "String format succeeded: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}
	else
	{
		string	strNote = string( "Number with exponent too large failed: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );
	}

	// Next string
	bstrTest = "0x00CD";
	ipTextInput->SetText( bstrTest );

	// Retest validity - expect failure
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
	{
		// If valid, leave a note
		string	strNote = string( "String format succeeded: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}
	else
	{
		string	strNote = string( "Simple hexadecimal number failed: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );
	}

	/////////////////////
	// End this test case
	/////////////////////
	ipTextInput = __nullptr;
	if (bTestSuccess)
	{
		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
IInputValidatorPtr CDoubleInputValidator::getThisAsInputValidatorPtr()
{
	IInputValidatorPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI22010", ipThis != __nullptr);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
void CDoubleInputValidator::validateLicense()
{
	static const unsigned long DOUBLEIV_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( DOUBLEIV_COMPONENT_ID, "ELI03737",
		"Double Input Validator" );
}
//-------------------------------------------------------------------------------------------------
