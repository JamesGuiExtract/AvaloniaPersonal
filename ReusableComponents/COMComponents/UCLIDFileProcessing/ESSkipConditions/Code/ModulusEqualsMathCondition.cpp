// ModulusEqualsMathCondition.cpp : Implementation of CModulusEqualsMathCondition

#include "stdafx.h"
#include "ModulusEqualsMathCondition.h"
#include "ESSkipConditions.h"

#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// current version
const unsigned long gnCurrentVersion = 1;

//--------------------------------------------------------------------------------------------------
// CModulusEqualsMathCondition
//--------------------------------------------------------------------------------------------------
CModulusEqualsMathCondition::CModulusEqualsMathCondition()
:m_bDirty(false),
m_nModulus(-1),
m_nModEquals(-1)
{
}
//--------------------------------------------------------------------------------------------------
CModulusEqualsMathCondition::~CModulusEqualsMathCondition()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27181");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_IModulusEqualsMathCondition,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IMustBeConfiguredObject,
		&IID_IMathConditionChecker
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// IModulusEqualsMathConditionFAMCondition
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::get_Modulus(long* pnVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27182", pnVal != NULL);

		*pnVal = m_nModulus;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27183");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::put_Modulus(long nVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27184", nVal > 1);

		m_nModulus = nVal;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27185");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::get_ModEquals(long* pnVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27186", pnVal != NULL);

		*pnVal = m_nModEquals;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27187");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::put_ModEquals(long nVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27188", nVal >= 0 && (m_nModulus == -1 || nVal < m_nModulus));

		m_nModEquals = nVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27189");
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI27190", pbValue != NULL);

		try
		{
			// validate license
			validateLicense();
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27191");
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IModulusEqualsMathConditionPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI27192", ipCopyThis != NULL);
		
		// Copy the values from another object
		m_nModulus = ipCopyThis->Modulus;
		m_nModEquals = ipCopyThis->ModEquals;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27193");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ASSERT_ARGUMENT("ELI27194", pObject != NULL);

		ICopyableObjectPtr ipCopy(CLSID_ModulusEqualsMathCondition);
		ASSERT_RESOURCE_ALLOCATION("ELI27195", ipCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27196");
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27197", pbValue != NULL);

		// Configured if:
		// 1. Modulus > 1
		// 2. Value >= 0
		// 3. Value < Modulus
		*pbValue = m_nModulus > 1 && m_nModEquals >= 0 && m_nModEquals < m_nModulus;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27198");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	*pClassID = CLSID_ModulusEqualsMathCondition;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// reset member variables
		m_nModulus = -1;
		m_nModEquals = -1;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI27199", "Unable to load newer Modulus equals math condition component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Load the variables 
		dataReader >> m_nModulus;
		dataReader >> m_nModEquals;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27200");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// save the variables
		dataWriter << gnCurrentVersion;
		dataWriter << m_nModulus;
		dataWriter << m_nModEquals;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Clear the flag as specified
		if (fClearDirty == TRUE)
		{
			m_bDirty = false;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27201");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IMathConditionChecker
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModulusEqualsMathCondition::raw_CheckCondition(BSTR bstrFileName, long lFileID, 
	long lActionID, VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Ensure the file ID is valid
		if (lFileID <= 0)
		{
			UCLIDException ue("ELI29796", "Invalid file ID.");
			ue.addDebugInfo("File ID", lFileID);
			throw ue;
		}

		// Check the modulus result comparison and return as VARIANT_BOOL
		*pbResult = asVariantBool(lFileID % m_nModulus == m_nModEquals);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27204");
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CModulusEqualsMathCondition::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI27205", "Modulus Equals FAM Condition");
}
//-------------------------------------------------------------------------------------------------