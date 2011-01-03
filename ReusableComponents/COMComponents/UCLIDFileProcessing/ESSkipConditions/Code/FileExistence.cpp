// FileExistence.cpp : Implementation of CFileExistence

#include "stdafx.h"
#include "FileExistence.h"
#include "ESSkipConditions.h"
#include "SkipConditionUtils.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <TextFunctionExpander.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// current version
const unsigned long gnCurrentVersion = 1;

//--------------------------------------------------------------------------------------------------
// CFileExistence
//--------------------------------------------------------------------------------------------------
CFileExistence::CFileExistence()
:m_bDirty(false),
m_bFileDoesExist(true)
{
}
//--------------------------------------------------------------------------------------------------
CFileExistence::~CFileExistence()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16560");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_IFileExistenceFAMCondition,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IMustBeConfiguredObject,
		&IID_IFAMCondition,
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
// IFileExistenceFAMCondition
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::get_FileExists(VARIANT_BOOL* pRetVal)
{
	try
	{
		// Check license
		validateLicense();

		*pRetVal = asVariantBool(m_bFileDoesExist);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13545");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::put_FileExists(VARIANT_BOOL newVal)
{
	try
	{
		// Check license
		validateLicense();

		m_bFileDoesExist = (newVal==VARIANT_TRUE);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13547");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::get_FileString(BSTR *strFileString)
{
	try
	{
		// Check license
		validateLicense();

		*strFileString = _bstr_t(m_strFileName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13549");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::put_FileString(BSTR strFileString)
{
	try
	{
		// Check license
		validateLicense();

		string strFile = asString(strFileString);

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI14419", ipFAMTagManager != NULL);

		// make sure the file exists
		// or that it contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strFile.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI14420", "The FAM condition filename contains invalid tags!");
			ue.addDebugInfo("File", strFile);
			throw ue;
		}

		m_strFileName = strFile;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13554");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::raw_IsLicensed(VARIANT_BOOL * pbValue)
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

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19636", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Match based upon file existence").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13518")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		EXTRACT_FAMCONDITIONSLib::IFileExistenceFAMConditionPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI13564", ipCopyThis != NULL);
		
		m_bFileDoesExist = ((ipCopyThis->FileExists)==VARIANT_TRUE);
		m_strFileName = asString(ipCopyThis->FileString);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13519");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_FileExistence);
		ASSERT_RESOURCE_ALLOCATION("ELI13566", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13520");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		bool bConfigured = true;
		if (m_strFileName.empty())
		{
			bConfigured = false;
		}

		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13521");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_FileExistence;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// reset member variables
		m_bFileDoesExist = true;
		m_strFileName = "";

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
			UCLIDException ue("ELI13570", "Unable to load newer file existence FAM condition component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		dataReader >> m_bFileDoesExist;
		dataReader >> m_strFileName;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13523");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_bFileDoesExist;
		dataWriter << m_strFileName;

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
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13522");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IFAMCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::raw_FileMatchesFAMCondition(BSTR bstrFile, IFileProcessingDB* pFPDB, 
	long lFileID, long lActionID, IFAMTagManager* pFAMTM, VARIANT_BOOL* pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		std::string strSourceFileName = asString(bstrFile);

		// Call ExpandTagsAndTFE() to expand tags and utility functions
		string strFAMConditionFile = CFAMConditionUtils::ExpandTagsAndTFE(pFAMTM, m_strFileName, strSourceFileName);

		// Is the file really exist?
		bool bIsFileValid = isFileOrFolderValid(strFAMConditionFile);

		*pRetVal = asVariantBool(bIsFileValid == m_bFileDoesExist);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13669");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileExistence::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31205", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31206");
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CFileExistence::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI13516", "File Existence FAM Condition");
}
//-------------------------------------------------------------------------------------------------
