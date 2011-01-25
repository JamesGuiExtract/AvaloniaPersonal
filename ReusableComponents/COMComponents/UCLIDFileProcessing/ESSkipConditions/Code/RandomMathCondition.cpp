// RandomMathCondition.cpp : Implementation of CRandomMathCondition

#include "stdafx.h"
#include "RandomMathCondition.h"
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
// CRandomMathCondition
//--------------------------------------------------------------------------------------------------
CRandomMathCondition::CRandomMathCondition()
:m_bDirty(false),
m_nPercent(-1),
m_bSeeded(false)
{
}
//--------------------------------------------------------------------------------------------------
CRandomMathCondition::~CRandomMathCondition()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27230");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CRandomMathCondition::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_IRandomMathCondition,
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
// IRandomMathConditionFAMCondition
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CRandomMathCondition::get_Percent(long* pnVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27231", pnVal != NULL);

		*pnVal = m_nPercent;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27232");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CRandomMathCondition::put_Percent(long nVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27233", nVal > 0 && nVal < 100);

		m_nPercent = nVal;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27234");
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CRandomMathCondition::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI27235", pbValue != NULL);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27236");
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRandomMathCondition::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IRandomMathConditionPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI27237", ipCopyThis != NULL);
		
		// Copy the values from another object
		m_nPercent = ipCopyThis->Percent;

		// Reset the seeded value
		m_bSeeded = false;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27238");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRandomMathCondition::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ASSERT_ARGUMENT("ELI27239", pObject != NULL);

		ICopyableObjectPtr ipCopy(CLSID_RandomMathCondition);
		ASSERT_RESOURCE_ALLOCATION("ELI27240", ipCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27241");
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRandomMathCondition::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27242", pbValue != NULL);

		// Configured if:
		// 1. Percent > 0 && Percent < 100
		*pbValue = m_nPercent > 0 && m_nPercent < 100;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27243");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRandomMathCondition::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	*pClassID = CLSID_RandomMathCondition;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRandomMathCondition::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRandomMathCondition::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// reset member variables
		m_nPercent = -1;
		m_bSeeded = false;

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
			UCLIDException ue("ELI27244", "Unable to load newer Random math condition component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Load the variables 
		dataReader >> m_nPercent;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27245");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRandomMathCondition::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_nPercent;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27246");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRandomMathCondition::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IMathConditionChecker
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRandomMathCondition::raw_CheckCondition(IFileRecord* pFileRecord, 
	long lActionID, VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		if (!m_bSeeded)
		{
			// Seed the random number generator
			srand((unsigned int) time(NULL));
			m_bSeeded = true;
		}
			
		// Get a random number between 1 and 99 (inclusive)
		int nNum = rand() % 99 + 1;

		// Return true if the nNum <= m_nPercent
		*pbResult = asVariantBool(nNum <= m_nPercent);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27247");
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CRandomMathCondition::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI27248", "Random FAM condition");
}
//-------------------------------------------------------------------------------------------------