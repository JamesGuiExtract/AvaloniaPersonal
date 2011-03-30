// OnceEveryMathCondition.cpp : Implementation of COnceEveryMathCondition

#include "stdafx.h"
#include "OnceEveryMathCondition.h"
#include "ESSkipConditions.h"

#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// Statics
//--------------------------------------------------------------------------------------------------
CMutex COnceEveryMathCondition::ms_Mutex;
map<string, long> COnceEveryMathCondition::ms_mapIDToCount;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// current version
const unsigned long gnCurrentVersion = 1;

//--------------------------------------------------------------------------------------------------
// COnceEveryMathCondition
//--------------------------------------------------------------------------------------------------
COnceEveryMathCondition::COnceEveryMathCondition()
:m_bDirty(false),
m_nNumberOfTimes(-1),
m_strUsageID("")
{
}
//--------------------------------------------------------------------------------------------------
COnceEveryMathCondition::~COnceEveryMathCondition()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27206");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_IOnceEveryMathCondition,
		&IID_ICopyableObject,
		&IID_IClipboardCopyable,
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
// IOnceEveryMathConditionFAMCondition
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::get_NumberOfTimes(long* pnVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27207", pnVal != __nullptr);

		*pnVal = m_nNumberOfTimes;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27208");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::put_NumberOfTimes(long nVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27209", nVal > 1);

		m_nNumberOfTimes = nVal;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27210");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::get_UsageID(BSTR* pbstrUsageID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27211", pbstrUsageID != __nullptr);

		*pbstrUsageID = _bstr_t(m_strUsageID.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27212");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::put_UsageID(BSTR bstrUsageID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		string strUsageID = asString(bstrUsageID);
		ASSERT_ARGUMENT("ELI27213", !strUsageID.empty());

		m_strUsageID = strUsageID;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27214");
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI27215", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27216");
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IOnceEveryMathConditionPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI27217", ipCopyThis != __nullptr);
		
		// Copy the values from another object
		m_nNumberOfTimes = ipCopyThis->NumberOfTimes;
		m_strUsageID = asString(ipCopyThis->UsageID);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27218");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ASSERT_ARGUMENT("ELI27219", pObject != __nullptr);

		ICopyableObjectPtr ipCopy(CLSID_OnceEveryMathCondition);
		ASSERT_RESOURCE_ALLOCATION("ELI27220", ipCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27221");
}

//-------------------------------------------------------------------------------------------------
// IClipboardCopyable
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::raw_NotifyCopiedFromClipboard()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		// Need to generate a new GUID
		GUID gGuid;
		HRESULT hr = CoCreateGuid(&gGuid);
		if (hr != S_OK)
		{
			UCLIDException ue("ELI27257", "Failed to generate unique identifier!");
			ue.addHresult(hr);
			throw ue;
		}

		// Get the GUID as a string
		m_strUsageID = asString(gGuid);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27256");
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27222", pbValue != __nullptr);

		// Configured if:
		// 1. NumberOfTimes > 1
		// 2. UsageID is not empty
		*pbValue = m_nNumberOfTimes > 1 && !m_strUsageID.empty();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27223");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	*pClassID = CLSID_OnceEveryMathCondition;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// reset member variables
		m_nNumberOfTimes = -1;
		m_strUsageID = "";

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
			UCLIDException ue("ELI27224", "Unable to load newer Once for every number of times math condition component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Load the variables 
		dataReader >> m_nNumberOfTimes;
		dataReader >> m_strUsageID;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27225");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_nNumberOfTimes;
		dataWriter << m_strUsageID;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27226");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IMathConditionChecker
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COnceEveryMathCondition::raw_CheckCondition(IFileRecord* pFileRecord, 
	long lActionID, VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pbResult = asVariantBool(checkCount());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27227");
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void COnceEveryMathCondition::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI27228",
		"Once Every X Documents FAM Condition");
}
//-------------------------------------------------------------------------------------------------
bool COnceEveryMathCondition::checkCount()
{
	bool bResult = false;
	long nCount = 0;

	// Scope for the mutex
	{
		CSingleLock lg(&ms_Mutex, TRUE);

		// Search the map for the usage ID
		map<string, long>::iterator it = ms_mapIDToCount.find(m_strUsageID);
		if (it != ms_mapIDToCount.end())
		{
			nCount = it->second;
		}

		// Increment the count
		nCount++;

		// If the count matches the number of times
		// set the result to true and reset the count
		if (nCount == m_nNumberOfTimes)
		{
			bResult = true;
			nCount = 0;
		}

		// Store the updated count
		ms_mapIDToCount[m_strUsageID] = nCount;
	}

	// Return the result
	return bResult;
}
//-------------------------------------------------------------------------------------------------