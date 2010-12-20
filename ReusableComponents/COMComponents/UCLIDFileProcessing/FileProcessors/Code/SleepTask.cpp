//==================================================================================================
//
// COPYRIGHT (c) 2009 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SleepTask.cpp
//
// PURPOSE:	A file processing task that will allow a processing thread to sleep.
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#include "stdafx.h"
#include "FileProcessors.h"
#include "SleepTask.h"
#include "SleepConstants.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <ByteStream.h>
#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

// component description
const string gstrSLEEP_COMPONENT_DESCRIPTION = "Core: Sleep";

//--------------------------------------------------------------------------------------------------
// CSleepTask
//--------------------------------------------------------------------------------------------------
CSleepTask::CSleepTask() :
m_TimeUnits(kSleepSeconds),
m_lSleepTime(0),
m_bRandom(false),
m_bCancel(false),
m_bDirty(false)
{
}

//--------------------------------------------------------------------------------------------------
// ISleepTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::put_SleepTime(long lSleepTime)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();
		
		// Ensure the sleep time is positive
		ASSERT_ARGUMENT("ELI29568", lSleepTime > 0);

		m_lSleepTime = lSleepTime;

		// Set the dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29569");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::get_SleepTime(long* plSleepTime)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		ASSERT_ARGUMENT("ELI29570", plSleepTime != NULL);

		*plSleepTime = m_lSleepTime;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29571");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::put_TimeUnits(ESleepTimeUnitType eTimeUnits)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		// Store the new operation type
		m_TimeUnits = eTimeUnits;

		// Set the dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29572");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::get_TimeUnits(ESleepTimeUnitType* peTimeUnits)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		ASSERT_ARGUMENT("ELI29573", peTimeUnits != NULL);

		// Get the operation type
		*peTimeUnits = m_TimeUnits;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29574");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::put_Random(VARIANT_BOOL bRandom)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		// Store the new operation type
		m_bRandom = asCppBool(bRandom);

		// Set the dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29575");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::get_Random(VARIANT_BOOL *pbRandom)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		ASSERT_ARGUMENT("ELI29576", pbRandom != NULL);

		// Get the operation type
		*pbRandom = asVariantBool(m_bRandom);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29577");
}

//--------------------------------------------------------------------------------------------------
// ICategorizedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::raw_GetComponentDescription(BSTR* pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI29578", pstrComponentDescription != NULL);
		
		*pstrComponentDescription = 
			_bstr_t(gstrSLEEP_COMPONENT_DESCRIPTION.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29579");
	
	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ICopyableObject
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::raw_CopyFrom(IUnknown* pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license first
		validateLicense();

		// get the SleepTask object
		UCLID_FILEPROCESSORSLib::ISleepTaskPtr ipSleepTask(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI29580", ipSleepTask != NULL);

		// Copy the object
		m_lSleepTime = ipSleepTask->SleepTime;
		m_TimeUnits = (ESleepTimeUnitType) ipSleepTask->TimeUnits;
		m_bRandom = asCppBool(ipSleepTask->Random);

		// set the dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29581");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::raw_Clone(IUnknown** ppObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate license first
		validateLicense();

		// Ensure that the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI29582", ppObject != NULL);

		// Get the copyable object interface
		ICopyableObjectPtr ipObjCopy(CLSID_SleepTask);
		ASSERT_RESOURCE_ALLOCATION("ELI29583", ipObjCopy != NULL);

		// Create a shallow copy
		IUnknownPtr ipUnknown(this);
		ASSERT_RESOURCE_ALLOCATION("ELI29584", ipUnknown != NULL);
		ipObjCopy->CopyFrom(ipUnknown);

		// Return the new SleepTask to the caller
		*ppObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29585");
}

//--------------------------------------------------------------------------------------------------
// IFileProcessingTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_bCancel = false;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29586");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::raw_ProcessFile(BSTR bstrFileFullName, long nFileID, long nActionID,
	IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI29587", pResult != NULL);

		// Compute the sleep time
		DWORD dwSleepTime = computeSleepTime();

		// Check for progress status
		IProgressStatusPtr ipProgress(pProgressStatus);
		if (ipProgress != NULL)
		{
			int iNumItems = (dwSleepTime / 1000) + 1;
			ipProgress->InitProgressStatus("Sleeping...", 0, iNumItems, VARIANT_TRUE);
		}

		while (dwSleepTime > 0)
		{
			// Check for processing cancelled
			if (m_bCancel)
			{
				// Indicate task cancelled and return immediately
				*pResult = kProcessingCancelled;
				return S_OK;
			}

			// Sleep for min of 1 second or remaining sleep time
			DWORD dwTemp = min(dwSleepTime, 1000);
			Sleep(dwTemp);
			dwSleepTime -= dwTemp;

			// Update progress status if necessary
			if (ipProgress != NULL)
			{
				ipProgress->CompleteProgressItems("Sleeping...", 1);
			}
		}

		// Ensure the last group gets completed
		if (ipProgress != NULL && ipProgress->NumItemsInCurrentGroup > 0)
		{
			ipProgress->CompleteCurrentItemGroup();
		}

		// Indicate successful completion
		*pResult = kProcessingSuccessful;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29588")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Set the cancel flag to indicate task should be cancelled
		m_bCancel = true;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29589");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29590");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31193", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31194");
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI29591", pbValue != NULL);

		try
		{
			// Check license
			validateLicense();

			// If no exception was thrown, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29592");
}

//--------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::raw_IsConfigured(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// Ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI29593", pbValue != NULL);

		// Configured if:
		// 1. Sleep time is > 0
		bool bIsConfigured = m_lSleepTime > 0;

		*pbValue = asVariantBool(bIsConfigured);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29594");
}

//--------------------------------------------------------------------------------------------------
// IPersistStream
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::GetClassID(CLSID* pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI29595", pClassID != NULL);

		*pClassID = CLSID_SleepTask;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29596");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::Load(IStream* pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check license state
		validateLicense();

		// clear the options
		m_lSleepTime = 0;
		m_TimeUnits = (ESleepTimeUnitType) kSleepSeconds;
		m_bRandom = false;
		
		// use a smart pointer for the IStream interface
		IStreamPtr ipStream(pStream);
		ASSERT_RESOURCE_ALLOCATION("ELI29597", ipStream != NULL);

		// read the bytestream data from the IStream object
		long nDataLength = 0;
		HANDLE_HRESULT(ipStream->Read(&nDataLength, sizeof(nDataLength), NULL), "ELI29598", 
			"Unable to read object size from stream.", ipStream, IID_IStream);
		ByteStream data(nDataLength);
		HANDLE_HRESULT(ipStream->Read(data.getData(), nDataLength, NULL), "ELI29599", 
			"Unable to read object from stream.", ipStream, IID_IStream);

		// read the data version
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// check if file is a newer version than this object can use
		if (nDataVersion > gnCurrentVersion)
		{
			// throw exception
			UCLIDException ue("ELI29600", "Unable to load newer Sleep task.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read the sleep time
		dataReader >> m_lSleepTime;

		// Read the sleep units
		long lTemp;
		dataReader >> lTemp;
		m_TimeUnits = (ESleepTimeUnitType) lTemp;

		// Read the random value
		dataReader >> m_bRandom;

		// clear the dirty flag since a new object was loaded
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29601");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::Save(IStream* pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check license state
		validateLicense();

		// create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		dataWriter << gnCurrentVersion;
		dataWriter << m_lSleepTime;
		dataWriter << (long) m_TimeUnits;
		dataWriter << m_bRandom;

		// flush the data to the stream
		dataWriter.flushToByteStream();

		// use a smart pointer for IStream interface
		IStreamPtr ipStream(pStream);
		ASSERT_RESOURCE_ALLOCATION("ELI29602", ipStream != NULL);

		// write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		HANDLE_HRESULT(ipStream->Write(&nDataLength, sizeof(nDataLength), NULL), "ELI29603", 
			"Unable to write object size to stream.", ipStream, IID_IStream);
		HANDLE_HRESULT(ipStream->Write(data.getData(), nDataLength, NULL), "ELI29604", 
			"Unable to write object to stream.", ipStream, IID_IStream);

		// clear the flag as specified
		if(fClearDirty)
		{
			m_bDirty = false;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29605");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::GetSizeMax(ULARGE_INTEGER* pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return E_NOTIMPL;
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSleepTask::InterfaceSupportsErrorInfo(REFIID riid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		static const IID* arr[] = 
		{
			&IID_ISleepTask,
			&IID_ICategorizedComponent,
			&IID_ICopyableObject,
			&IID_IFileProcessingTask,
			&IID_ILicensedComponent,
			&IID_IMustBeConfiguredObject
		};

		for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
			{
				return S_OK;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29606");

	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// Private functions
//--------------------------------------------------------------------------------------------------
void CSleepTask::validateLicense()
{
	// ensure that add watermark is licensed
	VALIDATE_LICENSE(gnFILE_ACTION_MANAGER_OBJECTS, "ELI29607", "Sleep Task");
}
//--------------------------------------------------------------------------------------------------
DWORD CSleepTask::computeSleepTime()
{
	unsigned long ulTimeMultiplier = 0;
	switch(m_TimeUnits)
	{
	case kSleepMilliseconds:
			ulTimeMultiplier = glMILLISECONDS_MULTIPLIER;
			break;

	case kSleepSeconds:
			ulTimeMultiplier = glSECONDS_MULTIPLIER;
			break;
			
	case kSleepMinutes:
			ulTimeMultiplier = glMINUTES_MULTIPLIER;
			break;

	case kSleepHours:
			ulTimeMultiplier = glHOURS_MULTIPLIER;
			break;

	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI29608");
	}

	DWORD dwSleepTime = m_lSleepTime * ulTimeMultiplier;

	// If sleeping a random amount of time, get a random value between 1 and dwSleepTime
	if (m_bRandom)
	{
		dwSleepTime = m_Random.uniform(1, dwSleepTime);
	}

	return dwSleepTime;
}
//--------------------------------------------------------------------------------------------------
