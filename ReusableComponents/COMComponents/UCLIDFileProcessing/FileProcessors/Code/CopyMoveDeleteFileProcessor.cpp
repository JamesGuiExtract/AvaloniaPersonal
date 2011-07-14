// CopyMoveDeleteFileProcessor.cpp : Implementation of CCopyMoveDeleteFileProcessor
#include "stdafx.h"
#include "FileProcessors.h"
#include "CopyMoveDeleteFileProcessor.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <TemporaryFileName.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <io.h>
#include <TemporaryResourceOverride.h>
#include <misc.h>
#include <ComUtils.h>
#include <TextFunctionExpander.h>
#include <ComponentLicenseIDs.h>

#include <math.h>
#include <strstream>
#include <fstream>
#include <string>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// global variables / static variables / externs
extern CComModule _Module;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// current version
const unsigned long gnCurrentVersion = 5;
// Version 3:
//		Added m_bAllowReadonly to allow move and delete of readonly files
// Version 4:
//		Added m_bModifySourceDocName to allow modification of the SourceDocName in the database
// Version 5:
//		Added m_bSecureDelete and m_bThrowIfUnableToDeleteSecurely

//--------------------------------------------------------------------------------------------------
// CCopyMoveDeleteFileProcessor
//--------------------------------------------------------------------------------------------------
CCopyMoveDeleteFileProcessor::CCopyMoveDeleteFileProcessor()
: m_bDirty(false),
  m_eOperation(kCMDOperationCopyFile),
  m_strSrc(""),
  m_strDst(""),
  m_bCreateDirectory(true),
  m_bAllowReadonly(false),
  m_bModifySourceDocName(false),
  m_bSecureDelete(false),
  m_bThrowIfUnableToDeleteSecurely(false),
  m_eSrcMissingType(kCMDSourceMissingError),
  m_eDestPresentType(kCMDDestinationPresentError)
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI12154")
}
//--------------------------------------------------------------------------------------------------
CCopyMoveDeleteFileProcessor::~CCopyMoveDeleteFileProcessor()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12155")
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_IFileProcessingTask,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IMustBeConfiguredObject,
		&IID_ICopyMoveDeleteFileProcessor,
		&IID_IAccessRequired
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
		return E_POINTER;

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

//--------------------------------------------------------------------------------------------------
// IFileProcessingTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing to do
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17781")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::raw_ProcessFile(IFileRecord* pFileRecord,
    long nActionID, IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();
		
		ASSERT_ARGUMENT("ELI17904", pTagManager != __nullptr);
		ASSERT_ARGUMENT("ELI17903", pResult != __nullptr);
		IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_ARGUMENT("ELI31341", ipFileRecord != __nullptr);

		// Default to successful completion
		*pResult = kProcessingSuccessful;

		std::string strSourceDocName = asString(ipFileRecord->Name);
		ASSERT_ARGUMENT("ELI17915", strSourceDocName.empty() == false);

		// Call ExpandTagsAndTFE() to expand tags and functions
		std::string strExSrc = CFileProcessorsUtils::ExpandTagsAndTFE(pTagManager, m_strSrc, strSourceDocName);

		switch (m_eOperation)
		{
		case kCMDOperationMoveFile:
			{
				// Call ExpandTagsAndTFE() to expand tags and functions
				std::string strExDst = CFileProcessorsUtils::ExpandTagsAndTFE(pTagManager, m_strDst, strSourceDocName);

				simplifyPathName(strExDst);

				// Check destination folder
				handleDirectory( strExDst );

				// Check destination file
				if (!checkDestinationFile( strExDst ))
				{
					break;
				}

				// Check source file
				if (isFileOrFolderValid( strExSrc ))
				{
					// Move File - overwrite if present
					if (m_bSecureDelete)
					{
						// If m_bSecureDelete is set, delete securely.
						moveFile( strExSrc, strExDst, true, true );
					}
					else
					{
						// If m_bSecureDelete is not set, allow the SecureDeleteAllSensitiveFiles
						// registry entry to dictate whether the file is deleted securely.
						moveFile( strExSrc, strExDst, true );
					}

					if (m_bModifySourceDocName)
					{
						try
						{
							try
							{
								pDB->RenameFile(ipFileRecord, strExDst.c_str());
							}
							CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31468");
						}
						catch (UCLIDException &ue)
						{
							UCLIDException uex("ELI31469", "File was moved successfully, "
								"but unable to change name in FAM database.", ue);
							uex.addDebugInfo("Name in DB", strSourceDocName);
							uex.addDebugInfo("Name to change to", strExDst);
							throw uex;
						}
					}

				}
				// Source file not found, check for error condition
				else if (m_eSrcMissingType == kCMDSourceMissingError)
				{
					// Create and throw exception
					UCLIDException ue("ELI13181", "Cannot Move missing file!");
					ue.addDebugInfo("FileToMove", strExSrc);
					ue.addDebugInfo("Target", strExDst);
					throw ue;
				}
				// Source file not found, just skip this file
				else if (m_eSrcMissingType == kCMDSourceMissingSkip)
				{
					// Do not Move this file
					break;
				}
				else
				{
					THROW_LOGIC_ERROR_EXCEPTION("ELI13187");
				}
			}
			break;
		case kCMDOperationCopyFile:
			{
				// Call ExpandTagsAndTFE() to expand tags and functions
				std::string strExDst = CFileProcessorsUtils::ExpandTagsAndTFE(pTagManager, m_strDst, strSourceDocName);  

				simplifyPathName(strExDst);

				// Check destination folder
				handleDirectory( strExDst );

				// Check destination file
				if (!checkDestinationFile( strExDst ))
				{
					break;
				}

				// Check source file
				if (isFileOrFolderValid( strExSrc ))
				{
					// Copy File
					copyFile(strExSrc, strExDst);

					if (m_bModifySourceDocName)
					{
						try
						{
							try
							{
								pDB->RenameFile(ipFileRecord, strExDst.c_str());
							}
							CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31504");
						}
						catch (UCLIDException &ue)
						{
							UCLIDException uex("ELI31470", "File was copied successfully, "
								"but unable to change name in FAM database.", ue);
							uex.addDebugInfo("Name in DB", strSourceDocName);
							uex.addDebugInfo("Name to change to", strExDst);
							throw uex;
						}
					}

				}
				// Source file not found, check for error condition
				else if (m_eSrcMissingType == kCMDSourceMissingError)
				{
					// Create and throw exception
					UCLIDException ue("ELI13182", "Cannot copy missing file!");
					ue.addDebugInfo("FileToCopy", strExSrc);
					ue.addDebugInfo("Target", strExDst);
					throw ue;
				}
				// Source file not found, just skip this file
				else if (m_eSrcMissingType == kCMDSourceMissingSkip)
				{
					// Do not Copy this file
					break;
				}
				else
				{
					THROW_LOGIC_ERROR_EXCEPTION("ELI13188");
				}
			}
			break;
		case kCMDOperationDeleteFile:
			{
				// Check source file
				if (isFileOrFolderValid( strExSrc ))
				{
					if (m_bSecureDelete)
					{
						// If m_bSecureDelete is set, delete securely.
						deleteFile(strExSrc, m_bAllowReadonly, true,
							m_bThrowIfUnableToDeleteSecurely);
					}
					else
					{
						// If m_bSecureDelete is not set, allow the SecureDeleteAllSensitiveFiles
						// registry entry to dictate whether the file is deleted securely.
						deleteFile(strExSrc, m_bAllowReadonly);
					}
				}
				// Source file not found, check for error condition
				else if (m_eSrcMissingType == kCMDSourceMissingError)
				{
					// Create and throw exception
					UCLIDException ue("ELI13183", "Cannot Delete missing file!");
					ue.addDebugInfo("FileToDelete", strExSrc);
					throw ue;
				}
				// Source file not found, just skip this file
				else if (m_eSrcMissingType == kCMDSourceMissingSkip)
				{
					// Do not Delete this file
					break;
				}
				else
				{
					THROW_LOGIC_ERROR_EXCEPTION("ELI13189");
				}
			}
			break;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12156")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// nothing to do
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12159")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing to do
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17780")
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31183", pbResult != __nullptr);

		*pbResult = m_bModifySourceDocName ? VARIANT_TRUE : VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31184");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19612", pstrComponentDescription != __nullptr);

		*pstrComponentDescription = _bstr_t("Core: Copy, move or delete file").Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12160")
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_FILEPROCESSORSLib::ICopyMoveDeleteFileProcessorPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI12161", ipCopyThis != __nullptr);
		
		m_eOperation = (ECopyMoveDeleteOperationType)ipCopyThis->Operation;
		m_strSrc = ipCopyThis->SourceFileName;
		m_strDst = ipCopyThis->DestinationFileName;
		m_bCreateDirectory = asCppBool(ipCopyThis->CreateFolder);
		m_eSrcMissingType = (ECMDSourceMissingType)ipCopyThis->GetSourceMissingType();
		m_eDestPresentType = (ECMDDestinationPresentType)ipCopyThis->GetDestinationPresentType();
		m_bAllowReadonly = asCppBool(ipCopyThis->AllowReadonly);
		m_bModifySourceDocName = asCppBool(ipCopyThis->ModifySourceDocName);
		m_bSecureDelete = asCppBool(ipCopyThis->SecureDelete);
		m_bThrowIfUnableToDeleteSecurely = asCppBool(ipCopyThis->ThrowIfUnableToDeleteSecurely);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12812");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_CopyMoveDeleteFileProcessor);
		ASSERT_RESOURCE_ALLOCATION("ELI12162", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12163");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_CopyMoveDeleteFileProcessor;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// reset member variables
		m_eOperation = kCMDOperationCopyFile;
		m_strSrc = "";
		m_strDst = "";
		m_bCreateDirectory = true;
		m_eSrcMissingType = kCMDSourceMissingError;
		m_eDestPresentType = kCMDDestinationPresentError;
		m_bAllowReadonly = false;
		m_bModifySourceDocName = false;
		m_bSecureDelete = false;
		m_bThrowIfUnableToDeleteSecurely = false;

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
			UCLIDException ue("ELI12164", "Unable to load newer CopyMoveDeleteFileProcessor component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Version 1 object behavior is FolderCreation = false
		if (nDataVersion == 1)
		{
			m_bCreateDirectory = false;
		}

		long nTmp;
		dataReader >> nTmp;
		m_eOperation = (ECopyMoveDeleteOperationType)nTmp;

		dataReader >> m_strSrc;
		dataReader >> m_strDst;

		// Get items for version 2
		if (nDataVersion >= 2)
		{
			dataReader >> m_bCreateDirectory;

			dataReader >> nTmp;
			m_eSrcMissingType = (ECMDSourceMissingType)nTmp;

			dataReader >> nTmp;
			m_eDestPresentType = (ECMDDestinationPresentType)nTmp;
		}

		// Get items for version 3
		if (nDataVersion >= 3)
		{
			dataReader >> m_bAllowReadonly;
		}

		// Get Items for version 4
		if (nDataVersion >= 4)
		{
			dataReader >> m_bModifySourceDocName;
		}

		if (nDataVersion >= 5)
		{
			dataReader >> m_bSecureDelete;
			dataReader >> m_bThrowIfUnableToDeleteSecurely;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12165");	
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << gnCurrentVersion;

		dataWriter << (long)m_eOperation;
		dataWriter << m_strSrc;
		dataWriter << m_strDst;

		dataWriter << m_bCreateDirectory;
		dataWriter << (long)m_eSrcMissingType;
		dataWriter << (long)m_eDestPresentType;

		dataWriter << m_bAllowReadonly;

		dataWriter << m_bModifySourceDocName;

		dataWriter << m_bSecureDelete;
		dataWriter << m_bThrowIfUnableToDeleteSecurely;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12166");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		bool bConfigured = true;

		switch(m_eOperation)
		{
		case kCMDOperationMoveFile:
		case kCMDOperationCopyFile:
			if (m_strSrc.size() == 0 || m_strDst.size() == 0)
			{
				bConfigured = false;
			}
			break;
		case kCMDOperationDeleteFile:
			if (m_strSrc.size() == 0)
			{
				bConfigured = false;
			}
			break;
		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI12130");
		}

		*pbValue = bConfigured ? VARIANT_TRUE : VARIANT_FALSE;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19406");
}

//-------------------------------------------------------------------------------------------------
// ICopyMoveDeleteFileProcessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::SetMoveFiles(BSTR bstrSrcDoc, BSTR bstrDstDoc)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		std::string strSrc = asString(bstrSrcDoc);
		if (strSrc.size() == 0)
		{
			UCLIDException ue("ELI12168", "Source string cannot be empty.");
			throw ue;
		}
		std::string strDst = asString(bstrDstDoc);
		if (strDst.size() == 0)
		{
			UCLIDException ue("ELI12169", "Destination string cannot be empty.");
			throw ue;
		}

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI14467", ipFAMTagManager != __nullptr);

		// make sure the file contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strSrc.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI14468", "The source string contains invalid tags!");
			ue.addDebugInfo("File", strSrc);
			throw ue;
		}
		if (ipFAMTagManager->StringContainsInvalidTags(strDst.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI14469", "The destination string contains invalid tags!");
			ue.addDebugInfo("File", strDst);
			throw ue;
		}

		// Store settings
		m_eOperation = kCMDOperationMoveFile;
		m_strSrc = strSrc;
		m_strDst = strDst;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12170");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::SetCopyFiles(BSTR bstrSrcDoc, BSTR bstrDstDoc)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		std::string strSrc = asString(bstrSrcDoc);
		if (strSrc.size() == 0)
		{
			UCLIDException ue("ELI12171", "Source document name cannot be empty.");
			throw ue;
		}
		std::string strDst = asString(bstrDstDoc);
		if (strDst.size() == 0)
		{
			UCLIDException ue("ELI12172", "Destination document name cannot be empty.");
			throw ue;
		}

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI14470", ipFAMTagManager != __nullptr);

		// make sure the file contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strSrc.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI14471", "The source document name contains invalid tags!");
			ue.addDebugInfo("File", strSrc);
			throw ue;
		}
		if (ipFAMTagManager->StringContainsInvalidTags(strDst.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI14472", "The destination document name contains invalid tags!");
			ue.addDebugInfo("File", strDst);
			throw ue;
		}

		// Store settings
		m_eOperation = kCMDOperationCopyFile;
		m_strSrc = strSrc;
		m_strDst = strDst;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12173");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::SetDeleteFiles(BSTR bstrSrcDoc)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		std::string strSrc = asString(bstrSrcDoc);
		if (strSrc.size() == 0)
		{
			UCLIDException ue("ELI12174", "Source document name cannot be empty.");
			throw ue;
		}

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI14473", ipFAMTagManager != __nullptr);

		// make sure the file contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strSrc.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI14474", "The source document name contains invalid tags!");
			ue.addDebugInfo("File", strSrc);
			throw ue;
		}

		// Store settings
		m_eOperation = kCMDOperationDeleteFile;
		m_strSrc = strSrc;
		m_strDst = "";
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12175");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::get_Operation(ECopyMoveDeleteOperationType *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pRetVal = m_eOperation;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12176");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::get_SourceFileName(BSTR *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pRetVal = _bstr_t(m_strSrc.c_str()).copy();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12177");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::get_DestinationFileName(BSTR *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pRetVal = _bstr_t(m_strDst.c_str()).copy();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12178");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::get_CreateFolder(VARIANT_BOOL *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pRetVal = m_bCreateDirectory ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13163");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::put_CreateFolder(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bCreateDirectory = (newVal == VARIANT_TRUE);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13164");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::get_SourceMissingType(ECMDSourceMissingType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pVal = m_eSrcMissingType;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13165");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::put_SourceMissingType(ECMDSourceMissingType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_eSrcMissingType = newVal;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13166");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::get_DestinationPresentType(ECMDDestinationPresentType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pVal = m_eDestPresentType;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13167");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::put_DestinationPresentType(ECMDDestinationPresentType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_eDestPresentType = newVal;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13168");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::get_AllowReadonly(VARIANT_BOOL *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI23605", pRetVal != __nullptr);

		// Check license
		validateLicense();

		*pRetVal = asVariantBool(m_bAllowReadonly);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI23606");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::put_AllowReadonly(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bAllowReadonly = asCppBool(newVal);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI23607");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::get_ModifySourceDocName(VARIANT_BOOL *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI31276", pRetVal != __nullptr);

		// Check license
		validateLicense();

		*pRetVal = asVariantBool(m_bModifySourceDocName);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31277");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::put_ModifySourceDocName(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bModifySourceDocName = asCppBool(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31278");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::get_SecureDelete(VARIANT_BOOL *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI32856", pRetVal != __nullptr);

		// Check license
		validateLicense();

		*pRetVal = asVariantBool(m_bSecureDelete);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32857");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::put_SecureDelete(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bSecureDelete = asCppBool(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32858");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::get_ThrowIfUnableToDeleteSecurely(VARIANT_BOOL *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI32859", pRetVal != __nullptr);

		// Check license
		validateLicense();

		*pRetVal = asVariantBool(m_bThrowIfUnableToDeleteSecurely);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32860");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCopyMoveDeleteFileProcessor::put_ThrowIfUnableToDeleteSecurely(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bThrowIfUnableToDeleteSecurely = asCppBool(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32861");
}
//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CCopyMoveDeleteFileProcessor::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFILE_ACTION_MANAGER_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI12179", "CopyMoveDelete File Processor");
}
//-------------------------------------------------------------------------------------------------
bool CCopyMoveDeleteFileProcessor::checkDestinationFile(const std::string& strDestinationFile)
{
	// Check previous existence of destination file
	if (isFileOrFolderValid( strDestinationFile ))
	{
		if (m_eDestPresentType == kCMDDestinationPresentError)
		{
			// Create and throw exception
			UCLIDException ue("ELI13185", "Destination file already exists.");
			ue.addDebugInfo("Destination", strDestinationFile);
			throw ue;
		}
		else if (m_eDestPresentType == kCMDDestinationPresentOverwrite)
		{
			// Continue with processing
			return true;
		}
		else if (m_eDestPresentType == kCMDDestinationPresentSkip)
		{
			// Do not continue with processing
			return false;
		}
		else
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI13186");
		}
	}

	// Destination file not found, continue with operation
	return true;
}
//-------------------------------------------------------------------------------------------------
void CCopyMoveDeleteFileProcessor::handleDirectory(const std::string& strDestinationFile)
{
	// Get destination folder
	string strFolder = getDirectoryFromFullPath( strDestinationFile );
	if (!isFileOrFolderValid( strFolder ))
	{
		// Destination folder does not exist
		if (m_bCreateDirectory)
		{
			// Create the directory first
			createDirectory( strFolder );
		}
		else
		{
			// Create and throw exception
			UCLIDException ue("ELI13192", "Destination folder does not exist.");
			ue.addDebugInfo("Target Folder", strFolder);
			ue.addDebugInfo("Target File", strDestinationFile);
			throw ue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
