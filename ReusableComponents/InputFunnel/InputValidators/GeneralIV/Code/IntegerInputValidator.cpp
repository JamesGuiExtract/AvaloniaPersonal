//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IntegerInputValidator.cpp
//
// PURPOSE:	Implementation of CIntegerInputValidator class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//-------------------------------------------------------------------------------------------------

#include "stdafx.h"
#include "GeneralIV.h"
#include "IntegerInputValidator.h"
#include "GeneralInputTypes.h"

#include <LicenseMgmt.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Constants for positive and negative infinity
const long	glPosInfinity = INT_MAX;		// glPosInfinity = 2147483647
const long	glNegInfinity = INT_MIN;		// glNegInfinity = -2147483648 yields warning C4146
const unsigned long	gulPosInfinity = ULONG_MAX;		// gulPosInfinity = 4294967295;

//-------------------------------------------------------------------------------------------------
// CIntegerInputValidator Methods
//-------------------------------------------------------------------------------------------------
CIntegerInputValidator::CIntegerInputValidator()
: m_bDirty(false)
{
	// Set defaults
	setDefaults();
}
//-------------------------------------------------------------------------------------------------
CIntegerInputValidator::~CIntegerInputValidator()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Force destruction of member objects within this scope so that
		// all destruction happens within the scope of the correct AFX state
		m_ipLogger = __nullptr;
		m_ipExecuter = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16444");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IIntegerInputValidator,
		&IID_ICopyableObject,
		&IID_IInputValidator,
		&IID_IPersistStream,
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
// ICopyableObject
//-------------------------------------------------------------------------------------------------

STDMETHODIMP CIntegerInputValidator::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_GENERALIVLib::IIntegerInputValidatorPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08300", ipSource != __nullptr);

		m_bIncludeMaximum = (ipSource->GetIncludeMaxInRange()==VARIANT_TRUE) ? true : false;
		m_bIncludeMinimum = (ipSource->GetIncludeMinInRange()==VARIANT_TRUE) ? true : false;
		m_bMaxDefined = (ipSource->GetHasMax()==VARIANT_TRUE) ? true : false;
		m_bMinDefined = (ipSource->GetHasMin()==VARIANT_TRUE) ? true : false;

		m_lMaximum = ipSource->GetMax();
		m_lMinimum = ipSource->GetMin();

		m_bNegativeAllowed = (ipSource->GetNegativeAllowed()==VARIANT_TRUE) ? true : false;
		m_bZeroAllowed = (ipSource->GetZeroAllowed()==VARIANT_TRUE) ? true : false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08301");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_IntegerInputValidator);
		ASSERT_RESOURCE_ALLOCATION("ELI08368", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04877");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_IntegerInputValidator;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::Load(IStream *pStream)
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
		m_lMinimum = 0;
		m_lMaximum = 0;
		m_ulMaximum = 0;

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
			dataReader >> m_lMinimum;
			dataReader >> m_lMaximum;
			dataReader >> m_ulMaximum;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04689");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_lMinimum;
		dataWriter << m_lMaximum;
		dataWriter << m_ulMaximum;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04688");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19622", pbstrComponentDescription != __nullptr)

		// Retrieve definition
		*pbstrComponentDescription = _bstr_t( 
			gstrINTEGER_INPUT_TYPE.c_str() ).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07304")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::raw_ValidateInput(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful)
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
		ASSERT_ARGUMENT("ELI10161", ipTextInput != __nullptr);

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

			// Check signed/unsigned flag
			if (m_bNegativeAllowed)
			{
				// Convert string to signed integer
				m_lValue = asLong( strInput );

				// Check signed value against limits
				bOkay = checkLimits();
			}
			else
			{
				// Convert string to unsigned integer
				m_ulValue = asUnsignedLong( strInput );

				// Check unsigned value against limits
				bOkay = checkLimits();
			}

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
			// Failed to convert entire string to integer
			// therefore ITextInput object is not valid
			*pbSuccessful = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03716")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::raw_GetInputType(BSTR * pstrInputType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	HRESULT hr = S_OK;

	try
	{
		ASSERT_ARGUMENT("ELI19623", pstrInputType != __nullptr)

		// Check license
		validateLicense();

		// Just provide the component description
		hr = raw_GetComponentDescription( pstrInputType );

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03708")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
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

		// Run test of string formats
		doTest7();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03714")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::raw_RunInteractiveTests()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Path will be either Debug or Release directory
		string strDir = getModuleDirectory(_Module.m_hInst) + "\\";

		// Bring up the exe 
		string strEXEFile = strDir + string( "..\\VBTest\\TestGeneralIV.exe" );
		runEXE( strEXEFile );

		string strITCFile = strDir + string("..\\Test Files\\InteractiveTest\\TestIntegerIV.itc");

		_bstr_t bstrITCFile( strITCFile.c_str() );
		m_ipExecuter->ExecuteITCFile( bstrITCFile );

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03695")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store pointer to the logger
		m_ipLogger = pLogger;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03706")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store pointer
		m_ipExecuter = pInteractiveTestExecuter;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19279")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IIntegerInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::get_Min(long *pVal)
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
		*pVal = m_lMinimum;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03717")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::put_Min(long newVal)
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
			UCLIDException uclidException("ELI03733", 
				"Invalid integer minimum value, negative numbers are not allowed.");
			uclidException.addDebugInfo("Requested minimum", newVal);
			throw uclidException;
		}
		else
		{
			// Store the new limit
			m_lMinimum = newVal;
		}
	
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03718")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::get_Max(long *pVal)
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

		// Check signed/unsigned state
		if (m_bNegativeAllowed)
		{
			// Use signed maximum
			*pVal = m_lMaximum;
		}
		else
		{
//			// Use unsigned maximum
//			*pVal = m_ulMaximum;
			// Use signed maximum (P13 #4605)
			*pVal = m_lMaximum;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03719")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::put_Max(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check signed/unsigned state
		if (m_bNegativeAllowed)
		{
			// Use signed maximum
			m_lMaximum = newVal;
		}
		else
		{
			if (newVal < 0)
			{
				// Throw exception - Invalid maximum
				UCLIDException uclidException("ELI03734", 
					"Invalid integer maximum value, negative numbers are not allowed.");
				uclidException.addDebugInfo("Requested maximum", newVal);
				throw uclidException;
			}
			else
			{
				// Use unsigned maximum
				m_ulMaximum = newVal;
			}
		}
	
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03720")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::get_HasMin(VARIANT_BOOL *pVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03721")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::put_HasMin(VARIANT_BOOL newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03722")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::get_HasMax(VARIANT_BOOL *pVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03723")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::put_HasMax(VARIANT_BOOL newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03724")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::get_ZeroAllowed(VARIANT_BOOL *pVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03725")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::put_ZeroAllowed(VARIANT_BOOL newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03726")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::get_NegativeAllowed(VARIANT_BOOL *pVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03727")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::put_NegativeAllowed(VARIANT_BOOL newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03728")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::get_IncludeMinInRange(VARIANT_BOOL *pVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03729")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::put_IncludeMinInRange(VARIANT_BOOL newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03730")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::get_IncludeMaxInRange(VARIANT_BOOL *pVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03731")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIntegerInputValidator::put_IncludeMaxInRange(VARIANT_BOOL newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03732")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
bool CIntegerInputValidator::checkLimits()
{
	bool bReturn = true;

	///////////////////////////////
	// Check the signed data member
	///////////////////////////////
	if (m_bNegativeAllowed)
	{
		// Check for positive/negative infinity
		if ((m_lValue == glPosInfinity) || (m_lValue == glNegInfinity))
		{
			bReturn = false;
		}

		// Check against minimum limit
		if (m_bMinDefined)
		{
			// Check boundary condition
			if (m_bIncludeMinimum)
			{
				// NOTE: m_lValue == m_lMinimum is valid
				if (m_lValue < m_lMinimum)
				{
					bReturn = false;
				}
			}
			else
			{
				// NOTE: m_lValue == m_lMinimum is invalid
				if (m_lValue <= m_lMinimum)
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
				// NOTE: m_lValue == m_lMaximum is valid
				if (m_lValue > m_lMaximum)
				{
					bReturn = false;
				}
			}
			else
			{
				// NOTE: m_lValue == m_lMaximum is invalid
				if (m_lValue >= m_lMaximum)
				{
					bReturn = false;
				}
			}
		}

		// Check zero
		if (!m_bZeroAllowed && m_lValue == 0)
		{
			bReturn = false;
		}
	}
	/////////////////////////////////
	// Check the unsigned data member
	/////////////////////////////////
	else
	{
		// Check for positive infinity
		if (m_ulValue == gulPosInfinity)
		{
			bReturn = false;
		}

		// Check against minimum limit
		if (m_bMinDefined)
		{
			// Check boundary condition
			if (m_bIncludeMinimum)
			{
				// NOTE: m_ulValue == m_lMinimum is valid
				if (m_ulValue < (unsigned long)m_lMinimum)
				{
					bReturn = false;
				}
			}
			else
			{
				// NOTE: m_ulValue == m_lMinimum is invalid
				if (m_ulValue <= (unsigned long)m_lMinimum)
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
				// NOTE: m_ulValue == m_ulMaximum is valid
				if (m_ulValue > m_ulMaximum)
				{
					bReturn = false;
				}
			}
			else
			{
				// NOTE: m_ulValue == m_ulMaximum is invalid
				if (m_ulValue >= m_ulMaximum)
				{
					bReturn = false;
				}
			}
		}

		// Check zero
		if (!m_bZeroAllowed && m_ulValue == 0)
		{
			bReturn = false;
		}
	}

	// Return result
	return bReturn;
}
//-------------------------------------------------------------------------------------------------
void CIntegerInputValidator::doTest1()
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
	put_Min( 10 );

	// Prepare ITextInput object with test string
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10162", ipTextInput != __nullptr);

	ipTextInput->InitTextInput( NULL, _bstr_t("5") );

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
void CIntegerInputValidator::doTest2()
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
	put_Max( 10 );

	// Prepare ITextInput object with test string
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10163", ipTextInput != __nullptr);

	ipTextInput->InitTextInput( NULL, _bstr_t("15") );

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
void CIntegerInputValidator::doTest3()
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
	ASSERT_RESOURCE_ALLOCATION("ELI10164", ipTextInput != __nullptr);

	ipTextInput->InitTextInput( NULL, _bstr_t("0") );

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
void CIntegerInputValidator::doTest4()
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
	ASSERT_RESOURCE_ALLOCATION("ELI10165", ipTextInput != __nullptr);

	ipTextInput->InitTextInput( NULL, _bstr_t("-50") );

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
		// If invalid, leave a note
		string	strNote = string( "String below zero passed without NegativeAllowed." );
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
void CIntegerInputValidator::doTest5()
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
	put_Min( 10 );

	// Set HasMin property
	put_HasMin( VARIANT_TRUE );

	// Prepare ITextInput object with test string
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10166", ipTextInput != __nullptr);
	
	ipTextInput->InitTextInput( NULL, _bstr_t("10") );

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
void CIntegerInputValidator::doTest6()
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
	put_Max( 10 );

	// Set HasMax property
	put_HasMax( VARIANT_TRUE );

	// Prepare ITextInput object with test string
	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10167", ipTextInput != __nullptr);
	
	ipTextInput->InitTextInput( NULL, _bstr_t("10") );

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
void CIntegerInputValidator::doTest7()
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
	_bstr_t	bstrTest( "-102" );

	ITextInputPtr ipTextInput(CLSID_TextInput);
	ASSERT_RESOURCE_ALLOCATION("ELI10168", ipTextInput != __nullptr);
	
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
		string	strNote = string( "Simple negative number format succeeded: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );
	}

	// Next string
	bstrTest = "-104.05";
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
		string	strNote = string( "Simple negative floating-point number format failed: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );
	}

	// Next string
	bstrTest = "1001234567890123456789012345";
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
		string	strNote = string( "Number too large failed: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );
	}

	// Next string
	bstrTest = "2147483647";
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
		string	strNote = string( "Positive infinity failed: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );
	}

	// Next string
	bstrTest = "-2147483648";
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
		string	strNote = string( "Negative infinity failed: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );
	}

	// Next string
	bstrTest = "-2147483647";
	ipTextInput->SetText( bstrTest );

	// Retest validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_FALSE)
	{
		// If valid, leave a note
		string	strNote = string( "String format failed: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );

		// Set flag
		bTestSuccess = false;
	}
	else
	{
		string	strNote = string( "Largest valid negative number succeeded: " ) + string( bstrTest );
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
		string	strNote = string( "Simple positive number succeeded: " ) + string( bstrTest );
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
		string	strNote = string( "Scientific notation number failed: " ) + string( bstrTest );
		_bstr_t	bstrNote = strNote.c_str();
		m_ipLogger->AddTestCaseNote( bstrNote );
	}

	// Next string
	bstrTest = "0x00CD";
	ipTextInput->SetText( bstrTest );

	// Retest validity - expect success
	vbResult = getThisAsInputValidatorPtr()->ValidateInput( ipTextInput );
	if (vbResult == VARIANT_TRUE)
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
		string	strNote = string( "Simple hexadecimal number succeeded: " ) + string( bstrTest );
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
void CIntegerInputValidator::setDefaults()
{
	// Set defaults
	// -Infinity (-2147483648) is considered to be an error
	m_lMinimum = -2147483647;		
	// +Infinity (2147483647) is considered to be an error
	m_lMaximum = 2147483646;
	// +Infinity (4294967295) is considered to be an error
	m_ulMaximum = 4294967294;

	m_bMinDefined = false;
	m_bMaxDefined = false;
	m_bZeroAllowed = true;
	m_bNegativeAllowed = true;
	m_bIncludeMinimum = true;
	m_bIncludeMaximum = true;
}
//-------------------------------------------------------------------------------------------------
IInputValidatorPtr CIntegerInputValidator::getThisAsInputValidatorPtr()
{
	IInputValidatorPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI22011", ipThis != __nullptr);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
void CIntegerInputValidator::validateLicense()
{
	static const unsigned long INTEGERIV_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( INTEGERIV_COMPONENT_ID, "ELI03693",
		"Integer Input Validator" );
}
//-------------------------------------------------------------------------------------------------
