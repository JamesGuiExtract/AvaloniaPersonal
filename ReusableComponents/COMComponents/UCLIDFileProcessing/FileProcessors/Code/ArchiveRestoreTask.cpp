//==================================================================================================
//
// COPYRIGHT (c) 2009 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ArchiveRestoreTask.cpp
//
// PURPOSE:	A file processing task that will allow archiving and restoring of specified files
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#include "stdafx.h"
#include "FileProcessors.h"
#include "ArchiveRestoreTask.h"
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
const string gstrARCHIVE_RESTORE_COMPONENT_DESCRIPTION = "Core: Archive or restore associated file";

// The number of digits in an archive folder name
const int giARCHIVE_FOLDER_DIGIT_COUNT = 8;

//--------------------------------------------------------------------------------------------------
// CArchiveRestoreTask
//--------------------------------------------------------------------------------------------------
CArchiveRestoreTask::CArchiveRestoreTask() :
m_operationType(kCMDOperationArchiveFile),
m_strArchiveFolder(""),
m_strFileTag(""),
m_bAllowOverwrite(false),
m_strFileToArchive(""),
m_bDeleteFileAfterArchiving(false),
m_bDirty(false)
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI24549");
}
//--------------------------------------------------------------------------------------------------
CArchiveRestoreTask::~CArchiveRestoreTask()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI24550");
}

//--------------------------------------------------------------------------------------------------
// IArchiveRestoreTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::put_Operation(EArchiveRestoreOperationType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		// Store the new operation type
		m_operationType = newVal;

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24551");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::get_Operation(EArchiveRestoreOperationType* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		ASSERT_ARGUMENT("ELI24552", pVal != __nullptr);

		// Get the operation type
		*pVal = m_operationType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24553");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::put_ArchiveFolder(BSTR bstrArchiveFolder)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();
		
		// Store the new archive folder
		m_strArchiveFolder = asString(bstrArchiveFolder);

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24554");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::get_ArchiveFolder(BSTR* pbstrArchiveFolder)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		ASSERT_ARGUMENT("ELI24555", pbstrArchiveFolder != __nullptr);

		// Get the archive folder
		*pbstrArchiveFolder = _bstr_t(m_strArchiveFolder.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24556");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::put_FileTag(BSTR bstrFileTag)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		// Store the new file tag
		m_strFileTag = asString(bstrFileTag);

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24557");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::get_FileTag(BSTR* pbstrFileTag)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		ASSERT_ARGUMENT("ELI24558", pbstrFileTag != __nullptr);

		// Get the file tag
		*pbstrFileTag = _bstr_t(m_strFileTag.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24559");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::put_AllowOverwrite(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		// Store the new overwrite value
		m_bAllowOverwrite = asCppBool(newVal);

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24560");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::get_AllowOverwrite(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		ASSERT_ARGUMENT("ELI24561", pVal != __nullptr);

		// Get the overwrite value
		*pVal = asVariantBool(m_bAllowOverwrite);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24562");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::put_FileToArchive(BSTR bstrFileToArchive)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		// Store the new file to archive
		m_strFileToArchive = asString(bstrFileToArchive);

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24563");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::get_FileToArchive(BSTR* pbstrFileToArchive)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		ASSERT_ARGUMENT("ELI24564", pbstrFileToArchive != __nullptr);

		// Get the file to archive
		*pbstrFileToArchive = _bstr_t(m_strFileToArchive.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24565");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::put_DeleteFileAfterArchive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		// Store the new delete file value
		m_bDeleteFileAfterArchiving = asCppBool(newVal);

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24566");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::get_DeleteFileAfterArchive(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		ASSERT_ARGUMENT("ELI24567", pVal != __nullptr);

		// Get the delete file value
		*pVal = asVariantBool(m_bDeleteFileAfterArchiving);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24568");
}

//--------------------------------------------------------------------------------------------------
// ICategorizedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::raw_GetComponentDescription(BSTR* pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI24569", pstrComponentDescription != __nullptr);
		
		*pstrComponentDescription = 
			_bstr_t(gstrARCHIVE_RESTORE_COMPONENT_DESCRIPTION.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24570");
	
	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ICopyableObject
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::raw_CopyFrom(IUnknown* pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license first
		validateLicense();

		// get the ArchiveRestoreTask object
		UCLID_FILEPROCESSORSLib::IArchiveRestoreTaskPtr ipArchiveRestoreTask(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI24571", ipArchiveRestoreTask != __nullptr);

		// Copy the values from the object
		m_operationType = (EArchiveRestoreOperationType) ipArchiveRestoreTask->Operation;
		m_strArchiveFolder = asString(ipArchiveRestoreTask->ArchiveFolder);
		m_strFileTag = asString(ipArchiveRestoreTask->FileTag);
		m_bAllowOverwrite = asCppBool(ipArchiveRestoreTask->AllowOverwrite);
		m_strFileToArchive = asString(ipArchiveRestoreTask->FileToArchive);
		m_bDeleteFileAfterArchiving = asCppBool(ipArchiveRestoreTask->DeleteFileAfterArchive);

		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24572");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::raw_Clone(IUnknown** ppObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license first
		validateLicense();

		// ensure that the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI24573", ppObject != __nullptr);

		// get the copyable object interface
		ICopyableObjectPtr ipObjCopy(CLSID_ArchiveRestoreTask);
		ASSERT_RESOURCE_ALLOCATION("ELI24574", ipObjCopy != __nullptr);

		// create a shallow copy
		IUnknownPtr ipUnknown(this);
		ASSERT_RESOURCE_ALLOCATION("ELI24575", ipUnknown != __nullptr);
		ipObjCopy->CopyFrom(ipUnknown);

		// return the new ArchiveRestoreTask to the caller
		*ppObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24576");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IFileProcessingTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24577");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::raw_ProcessFile(IFileRecord* pFileRecord, long nActionID,
	IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	INIT_EXCEPTION_AND_TRACING("MLI02466");
	try
	{
		// Check license
		validateLicense();

		// check for NULL parameters
		ASSERT_ARGUMENT("ELI24579", pTagManager != __nullptr);
		ASSERT_ARGUMENT("ELI24580", pResult != __nullptr);
		IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_ARGUMENT("ELI31335", ipFileRecord != __nullptr);

		long nFileID = ipFileRecord->FileID;

		// Default to successful completion
		*pResult = kProcessingSuccessful;
		_lastCodePos = "10";

		// Check whether this is an archive operation or a restore
		bool bArchive = m_operationType == kCMDOperationArchiveFile;
		_lastCodePos = "20";

		// get the source doc name
		string strSourceDoc = asString(ipFileRecord->Name);
		_lastCodePos = "30";

		// construct the full path to the file to archive/restore
		string strFileToArchiveRestore = 
			CFileProcessorsUtils::ExpandTagsAndTFE(pTagManager, m_strFileToArchive, strSourceDoc);
		_lastCodePos = "40";
		string strArchiveFolder = 
			CFileProcessorsUtils::ExpandTagsAndTFE(pTagManager, m_strArchiveFolder, "");
		_lastCodePos = "50";

		// Ensure there is a '\' on the archive folder
		if (strArchiveFolder[strArchiveFolder.length()-1] != '\\')
		{
			strArchiveFolder += "\\";
		}
		_lastCodePos = "60";

		// Build the path to where the file should be archived to/restored from
		// <ArchiveRoot>\((FileID/1,000,000) * 1,000,000)\((FileID/10,000) * 10,000)\
		// ((FileID/100) * 100)\FileID\<Tag>
		string strArchiveFileName = strArchiveFolder
			+ padCharacter(asString((nFileID/1000000) * 1000000), true, '0', giARCHIVE_FOLDER_DIGIT_COUNT) + "\\"
			+ padCharacter(asString((nFileID/10000) * 10000), true, '0', giARCHIVE_FOLDER_DIGIT_COUNT) + "\\"
			+ padCharacter(asString((nFileID/100) * 100), true, '0', giARCHIVE_FOLDER_DIGIT_COUNT) + "\\"
			+ padCharacter(asString(nFileID), true, '0', giARCHIVE_FOLDER_DIGIT_COUNT) + "\\"
			+ m_strFileTag;
		_lastCodePos = "70";

		// If overwrite is not allowed then check for existence of
		// 1. If archiving check for archive file
		// 2. If restoring check for file to archive/restore
		if (!m_bAllowOverwrite)
		{
			// Check if the file exists
			if (isValidFile(bArchive ? strArchiveFileName : strFileToArchiveRestore))
			{
				string strMessage = bArchive ? "Archive" : "Restore";
				strMessage += " file already exists!";
				UCLIDException uex("ELI24759", strMessage);
				uex.addDebugInfo("Archive File Name", strArchiveFileName);
				uex.addDebugInfo("File To Archive/Restore", strFileToArchiveRestore);
				uex.addDebugInfo("Source Document", strSourceDoc);
				throw uex;
			}
		}
		_lastCodePos = "80";

		// Check for archive operation
		if (bArchive)
		{
			// Ensure the directory exists
			createDirectory(getDirectoryFromFullPath(strArchiveFileName));

			// Check for deleting source file
			if (m_bDeleteFileAfterArchiving)
			{
				moveFile(strFileToArchiveRestore, strArchiveFileName, true);
			}
			else
			{
				copyFile(strFileToArchiveRestore, strArchiveFileName);
			}
			_lastCodePos = "80_A";
		}
		// Restore operation
		else
		{
			// Ensure the directory exists
			createDirectory(getDirectoryFromFullPath(strFileToArchiveRestore));

			copyFile(strArchiveFileName, strFileToArchiveRestore);
			_lastCodePos = "80_B";
		}
		_lastCodePos = "90";
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24581")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24582");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24583");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31175", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31176");
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI24584", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24585");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::raw_IsConfigured(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI24586", pbValue != __nullptr);

		// Configured if:
		// 1. There is an archive folder defined
		// 2. There is a file tag defined
		// 3. There is a file to archive/restore defined
		bool bIsConfigured = (m_strArchiveFolder != "")
			&& (m_strFileTag != "")
			&& (m_strFileToArchive != "");

		*pbValue = asVariantBool(bIsConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24587");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IPersistStream
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::GetClassID(CLSID* pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI24588", pClassID != __nullptr);

		*pClassID = CLSID_ArchiveRestoreTask;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24589");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::Load(IStream* pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check license state
		validateLicense();

		// clear the options
		m_operationType = kCMDOperationArchiveFile;
		m_strArchiveFolder = "";
		m_strFileTag = "";
		m_bAllowOverwrite = false;
		m_strFileToArchive = "";
		m_bDeleteFileAfterArchiving = false;
		
		// use a smart pointer for the IStream interface
		IStreamPtr ipStream(pStream);
		ASSERT_RESOURCE_ALLOCATION("ELI24590", ipStream != __nullptr);

		// read the bytestream data from the IStream object
		long nDataLength = 0;
		HANDLE_HRESULT(ipStream->Read(&nDataLength, sizeof(nDataLength), NULL), "ELI24591", 
			"Unable to read object size from stream.", ipStream, IID_IStream);
		ByteStream data(nDataLength);
		HANDLE_HRESULT(ipStream->Read(data.getData(), nDataLength, NULL), "ELI24592", 
			"Unable to read object from stream.", ipStream, IID_IStream);

		// read the data version
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// check if file is a newer version than this object can use
		if (nDataVersion > gnCurrentVersion)
		{
			// throw exception
			UCLIDException ue("ELI24593", "Unable to load newer ArchiveRestoreTask.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// read the settings from the stream
		long lTemp;
		dataReader >> lTemp;
		m_operationType = (EArchiveRestoreOperationType) lTemp;
		dataReader >> m_strArchiveFolder;
		dataReader >> m_strFileTag;
		dataReader >> m_bAllowOverwrite;
		dataReader >> m_strFileToArchive;
		dataReader >> m_bDeleteFileAfterArchiving;

		// clear the dirty flag since a new object was loaded
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24594");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::Save(IStream* pStream, BOOL fClearDirty)
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
		dataWriter << (long) m_operationType;
		dataWriter << m_strArchiveFolder;
		dataWriter << m_strFileTag;
		dataWriter << m_bAllowOverwrite;
		dataWriter << m_strFileToArchive;
		dataWriter << m_bDeleteFileAfterArchiving;

		// flush the data to the stream
		dataWriter.flushToByteStream();

		// use a smart pointer for IStream interface
		IStreamPtr ipStream(pStream);
		ASSERT_RESOURCE_ALLOCATION("ELI24595", ipStream != __nullptr);

		// write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		HANDLE_HRESULT(ipStream->Write(&nDataLength, sizeof(nDataLength), NULL), "ELI24596", 
			"Unable to write object size to stream.", ipStream, IID_IStream);
		HANDLE_HRESULT(ipStream->Write(data.getData(), nDataLength, NULL), "ELI24597", 
			"Unable to write object to stream.", ipStream, IID_IStream);

		// clear the flag as specified
		if(fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24598");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::GetSizeMax(ULARGE_INTEGER* pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return E_NOTIMPL;
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArchiveRestoreTask::InterfaceSupportsErrorInfo(REFIID riid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		static const IID* arr[] = 
		{
			&IID_IArchiveRestoreTask,
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24599");

	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// Private functions
//--------------------------------------------------------------------------------------------------
void CArchiveRestoreTask::validateLicense()
{
	// ensure that add watermark is licensed
	VALIDATE_LICENSE(gnFILE_ACTION_MANAGER_OBJECTS, "ELI24600", "ArchiveRestoreTask");
}
//--------------------------------------------------------------------------------------------------
