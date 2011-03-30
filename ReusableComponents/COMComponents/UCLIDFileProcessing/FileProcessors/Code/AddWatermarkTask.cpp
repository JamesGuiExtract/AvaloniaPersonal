//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	AddWatermarkTask.cpp
//
// PURPOSE:	A file processing task that will apply an image as a stamp on another image
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#include "stdafx.h"
#include "FileProcessors.h"
#include "AddWatermarkTask.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <ByteStream.h>
#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>
#include <Misc.h>
#include <PasteImage.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// Version 2:
//	Changed the page to stamp from a single integer to a string specifying a range of pages
const unsigned long gnCurrentVersion = 2;

// component description
const string gstrADD_WATERMARK_COMPONENT_DESCRIPTION = "Core: Add watermark";

const string gstrDEFAULT_INPUT_IMAGE_FILENAME = "";

//--------------------------------------------------------------------------------------------------
// CAddWatermarkTask
//--------------------------------------------------------------------------------------------------
CAddWatermarkTask::CAddWatermarkTask() :
m_strInputImage(gstrDEFAULT_INPUT_IMAGE_FILENAME),
m_strStampImage(""),
m_dHorizontalPercentage(-1.0),
m_dVerticalPercentage(-1.0),
m_strPagesToStamp("1")
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI19912");
}
//--------------------------------------------------------------------------------------------------
CAddWatermarkTask::~CAddWatermarkTask()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI19913");
}

//--------------------------------------------------------------------------------------------------
// IAddWatermarkTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::get_InputImageFile(BSTR *pbstrInputImageFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19914", pbstrInputImageFile != __nullptr);

		validateLicense();

		*pbstrInputImageFile = _bstr_t(m_strInputImage.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19915");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::put_InputImageFile(BSTR bstrInputImageFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strInputImage = asString(bstrInputImageFile);

		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19916");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::get_StampImageFile(BSTR *pbstrStampImageFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19917", pbstrStampImageFile != __nullptr);

		validateLicense();

		*pbstrStampImageFile = _bstr_t(m_strStampImage.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19918");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::put_StampImageFile(BSTR bstrStampImageFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strStampImage = asString(bstrStampImageFile);

		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19919");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::get_HorizontalPercentage(double *pdHorizPercentage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19920", pdHorizPercentage != __nullptr);

		validateLicense();

		*pdHorizPercentage = m_dHorizontalPercentage;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19921");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::put_HorizontalPercentage(double dHorizPercentage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_dHorizontalPercentage = dHorizPercentage;

		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19922");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::get_VerticalPercentage(double *pdVertPercentage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19923", pdVertPercentage != __nullptr);

		validateLicense();

		*pdVertPercentage = m_dVerticalPercentage;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19924");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::put_VerticalPercentage(double dVertPercentage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_dVerticalPercentage = dVertPercentage;

		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19925");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::get_PagesToStamp(BSTR *pbstrPagesToStamp)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19926", pbstrPagesToStamp != __nullptr);

		validateLicense();

		*pbstrPagesToStamp = _bstr_t(m_strPagesToStamp.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19927");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::put_PagesToStamp(BSTR bstrPagesToStamp)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Get the page number string and validate it
		string strPagesToStamp = asString(bstrPagesToStamp);
		validatePageNumbers(strPagesToStamp);

		// Store the valid string
		m_strPagesToStamp = strPagesToStamp;

		// set the dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19928");
}

//--------------------------------------------------------------------------------------------------
// ICategorizedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::raw_GetComponentDescription(BSTR* pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19929", pstrComponentDescription);
		
		*pstrComponentDescription = 
			_bstr_t(gstrADD_WATERMARK_COMPONENT_DESCRIPTION.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19930");
	
	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ICopyableObject
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::raw_CopyFrom(IUnknown* pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license first
		validateLicense();

		// get the AddWatermarkTask object
		UCLID_FILEPROCESSORSLib::IAddWatermarkTaskPtr ipAddWatermarkTask(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI19931", ipAddWatermarkTask != __nullptr);

		// copy the data from the WatermarkTask object
		m_strInputImage = asString(ipAddWatermarkTask->InputImageFile);
		m_strStampImage = asString(ipAddWatermarkTask->StampImageFile);
		m_dHorizontalPercentage = ipAddWatermarkTask->HorizontalPercentage;
		m_dVerticalPercentage = ipAddWatermarkTask->VerticalPercentage;
		m_strPagesToStamp = asString(ipAddWatermarkTask->PagesToStamp);

		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19932");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::raw_Clone(IUnknown** ppObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license first
		validateLicense();

		// ensure that the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI19933", ppObject != __nullptr);

		// get the copyable object interface
		ICopyableObjectPtr ipObjCopy(CLSID_AddWatermarkTask);
		ASSERT_RESOURCE_ALLOCATION("ELI19934", ipObjCopy != __nullptr);

		// create a shallow copy
		IUnknownPtr ipUnknown(this);
		ASSERT_RESOURCE_ALLOCATION("ELI19935", ipUnknown != __nullptr);
		ipObjCopy->CopyFrom(ipUnknown);

		// return the new AddWatermarkTask to the caller
		*ppObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19936");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IFileProcessingTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19937");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::raw_ProcessFile(IFileRecord* pFileRecord, long nActionID,
	IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// check for NULL parameters
		ASSERT_ARGUMENT("ELI19939", pTagManager != __nullptr);
		ASSERT_ARGUMENT("ELI19940", pResult != __nullptr);
		IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_ARGUMENT("ELI31333", ipFileRecord != __nullptr);

		// get the source doc name
		string strSourceDoc = asString(ipFileRecord->Name);
		validateFileOrFolderExistence(strSourceDoc);

		// construct the full path to the input and output images
		string strInputImage = 
			CFileProcessorsUtils::ExpandTagsAndTFE(pTagManager, m_strInputImage, strSourceDoc);
		string strOutputImage = strInputImage;

		// construct the full path to the stamp image
		string strStampImage = CFileProcessorsUtils::ExpandTagsAndTFE(pTagManager, 
			m_strStampImage, strSourceDoc);

		// Apply the watermark to the image on the specified pages
		pasteImageAtLocation(strInputImage, strOutputImage, strStampImage,
			m_dHorizontalPercentage, m_dVerticalPercentage, m_strPagesToStamp);

		// completed successfully
		*pResult = kProcessingSuccessful;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19941")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19942");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19943");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31173", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31174");
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI19944", pbValue != __nullptr);

		try
		{
			// check license
			validateLicense();

			// if no exception was thrown, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19945");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::raw_IsConfigured(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI19946", pbValue != __nullptr);

		// AddWatermark is configured if there is an input image, a stamp image
		// and the horizontal and vertical percentages have been set
		bool bIsConfigured = (!m_strInputImage.empty()
								&& !m_strStampImage.empty()
								&& m_dHorizontalPercentage >= 0.0
								&& m_dVerticalPercentage >= 0.0);

		*pbValue = asVariantBool(bIsConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19947");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IPersistStream
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::GetClassID(CLSID* pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19948", pClassID != __nullptr);

		*pClassID = CLSID_AddWatermarkTask;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19949");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::Load(IStream* pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check license state
		validateLicense();

		// clear the options
		m_strInputImage = gstrDEFAULT_INPUT_IMAGE_FILENAME;
		m_strStampImage = "";
		m_dHorizontalPercentage = -1.0;
		m_dVerticalPercentage = -1.0;
		m_strPagesToStamp = "1";
		
		// use a smart pointer for the IStream interface
		IStreamPtr ipStream(pStream);
		ASSERT_RESOURCE_ALLOCATION("ELI19950", ipStream != __nullptr);

		// read the bytestream data from the IStream object
		long nDataLength = 0;
		HANDLE_HRESULT(ipStream->Read(&nDataLength, sizeof(nDataLength), NULL), "ELI19951", 
			"Unable to read object size from stream.", ipStream, IID_IStream);
		ByteStream data(nDataLength);
		HANDLE_HRESULT(ipStream->Read(data.getData(), nDataLength, NULL), "ELI19952", 
			"Unable to read object from stream.", ipStream, IID_IStream);

		// read the data version
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// check if file is a newer version than this object can use
		if (nDataVersion > gnCurrentVersion)
		{
			// throw exception
			UCLIDException ue("ELI19953", "Unable to load newer AddWatermarkTask.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// read the settings from the stream
		dataReader >> m_strInputImage;
		dataReader >> m_strStampImage;
		dataReader >> m_dHorizontalPercentage;
		dataReader >> m_dVerticalPercentage;

		// If the object is a version 1 object, read the specific page to stamp
		if (nDataVersion == 1)
		{
			long lTemp;
			dataReader >> lTemp;
			m_strPagesToStamp = asString(lTemp);
		}
		else
		{
			dataReader >> m_strPagesToStamp;
		}

		// clear the dirty flag since a new object was loaded
		m_bDirty = false;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19954");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::Save(IStream* pStream, BOOL fClearDirty)
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
		dataWriter << m_strInputImage;
		dataWriter << m_strStampImage;
		dataWriter << m_dHorizontalPercentage;
		dataWriter << m_dVerticalPercentage;
		dataWriter << m_strPagesToStamp;

		// flush the data to the stream
		dataWriter.flushToByteStream();

		// use a smart pointer for IStream interface
		IStreamPtr ipStream(pStream);
		ASSERT_RESOURCE_ALLOCATION("ELI19955", ipStream != __nullptr);

		// write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		HANDLE_HRESULT(ipStream->Write(&nDataLength, sizeof(nDataLength), NULL), "ELI19956", 
			"Unable to write object size to stream.", ipStream, IID_IStream);
		HANDLE_HRESULT(ipStream->Write(data.getData(), nDataLength, NULL), "ELI19957", 
			"Unable to write object to stream.", ipStream, IID_IStream);

		// clear the flag as specified
		if(fClearDirty)
		{
			m_bDirty = false;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19958");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::GetSizeMax(ULARGE_INTEGER* pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return E_NOTIMPL;
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTask::InterfaceSupportsErrorInfo(REFIID riid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		static const IID* arr[] = 
		{
			&IID_IAddWatermarkTask,
			&IID_ICategorizedComponent,
			&IID_ICopyableObject,
			&IID_IFileProcessingTask,
			&IID_ILicensedComponent,
			&IID_IMustBeConfiguredObject,
			&IID_IAccessRequired
		};

		for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
			{
				return S_OK;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19959");

	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// Private functions
//--------------------------------------------------------------------------------------------------
void CAddWatermarkTask::validateLicense()
{
	// ensure that add watermark is licensed
	VALIDATE_LICENSE(gnFILE_ACTION_MANAGER_OBJECTS, "ELI19960", "AddWatermarkTask");
}
//--------------------------------------------------------------------------------------------------
