// LaunchAppFileProcessor.cpp : Implementation of CLaunchAppFileProcessor
#include "stdafx.h"
#include "FileProcessors.h"
#include "LaunchAppFileProcessor.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <TemporaryFileName.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <io.h>
#include <TemporaryResourceOverride.h>
#include <misc.h>
#include <ComUtils.h>
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
const unsigned long gnCurrentVersion = 3;

//--------------------------------------------------------------------------------------------------
// CLaunchAppFileProcessor
//--------------------------------------------------------------------------------------------------
CLaunchAppFileProcessor::CLaunchAppFileProcessor()
: m_bDirty(false),
  m_bBlocking(true),
  m_strCmdLine(""),
  m_bPropagateErrors(false),
  m_strWorkingDir("")
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI12195")
}
//--------------------------------------------------------------------------------------------------
CLaunchAppFileProcessor::~CLaunchAppFileProcessor()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12196")
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_IFileProcessingTask,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IMustBeConfiguredObject,
		&IID_ILaunchAppFileProcessor,
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
STDMETHODIMP CLaunchAppFileProcessor::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CLaunchAppFileProcessor::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17782");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::raw_ProcessFile(IFileRecord* pFileRecord, long nActionID,
	IFAMTagManager *pFAMTM, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		ASSERT_ARGUMENT("ELI17918", pFAMTM != __nullptr);
		ASSERT_ARGUMENT("ELI17919", pResult != __nullptr);
		IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_ARGUMENT("ELI31340", ipFileRecord != __nullptr);

		// Default to successful completion
		*pResult = kProcessingSuccessful;

		string strSourceDocName = asString(ipFileRecord->Name);
		ASSERT_ARGUMENT("ELI17917", strSourceDocName.empty() == false);

		// Call ExpandTagsAndTFE() to expand tags and functions
		string strCmdLineExp =
			CFileProcessorsUtils::ExpandTagsAndTFE(pFAMTM, m_strCmdLine, strSourceDocName);
		string strWorkingDirExp =
			CFileProcessorsUtils::ExpandTagsAndTFE(pFAMTM, m_strWorkingDir, strSourceDocName);
		string strParameters =
			CFileProcessorsUtils::ExpandTagsAndTFE(pFAMTM, m_strParameters, strSourceDocName);

		// Ensure the command line is quoted
		if (strCmdLineExp[0] != '"' && strCmdLineExp[strCmdLineExp.length() - 1] != '"')
		{
			strCmdLineExp = "\"" + strCmdLineExp + "\"";
		}

		// Ensure the working directory is not quoted
		strWorkingDirExp = trim(strWorkingDirExp, "\"", "\"");

		// If propogating errors back to the FAM then runExtractEXE rather than runEXE
		// [LRCAU #5536]
		if (m_bPropagateErrors)
		{
			runExtractEXE(strCmdLineExp, strParameters, (m_bBlocking ? INFINITE : 0), NULL,
				strWorkingDirExp, 0);
		}
		else
		{
			runEXE(strCmdLineExp, strParameters,
				(m_bBlocking ? INFINITE : 0), NULL, strWorkingDirExp, 0);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12199")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12201");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing todo
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17783");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31185", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31186");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19613", pstrComponentDescription != __nullptr);

		*pstrComponentDescription = _bstr_t("Core: Launch application").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12202")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_FILEPROCESSORSLib::ILaunchAppFileProcessorPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI12205", ipCopyThis != __nullptr);
		
		m_strCmdLine = asString(ipCopyThis->CommandLine);
		m_strWorkingDir = asString(ipCopyThis->WorkingDirectory);
		m_strParameters = asString(ipCopyThis->Parameters);

		m_bBlocking = asCppBool(ipCopyThis->IsBlocking);

		m_bPropagateErrors = asCppBool(ipCopyThis->PropagateErrors);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12811");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ASSERT_ARGUMENT("ELI25463", pObject != __nullptr);

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_LaunchAppFileProcessor);
		ASSERT_RESOURCE_ALLOCATION("ELI12203", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12204");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ASSERT_ARGUMENT("ELI25462", pClassID != __nullptr);

		*pClassID = CLSID_LaunchAppFileProcessor;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25461");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Version 2 - Added parameters value so that command line and parameters are separated
STDMETHODIMP CLaunchAppFileProcessor::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI25460", pStream != __nullptr);

		// reset member variables
		m_bBlocking = true;
		m_strCmdLine = "";
		m_strWorkingDir = "";
		m_strParameters = "";
		m_bPropagateErrors = false;

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
			UCLIDException ue("ELI12214", "Unable to load newer LaunchAppFileProcessor component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		dataReader >> m_strCmdLine;
		dataReader >> m_strWorkingDir;

		// For version 1 FPS, try to extract the parameters from the command line
		if (nDataVersion == 1)
		{
			size_t nSpace = m_strCmdLine.find_first_of(" ");
			size_t nQuote = m_strCmdLine.find_first_of("\"");

			// If the first space is after the first quote, look for the first space
			// after the last quote
			if (nSpace != string::npos && nQuote != string::npos && nSpace > nQuote)
			{
				nQuote = m_strCmdLine.find_first_of("\"", nQuote+1);
				if (nQuote != string::npos)
				{
					nSpace = m_strCmdLine.find_first_of(" ", nQuote);
				}
			}

			if (nSpace != string::npos)
			{
				m_strParameters = m_strCmdLine.substr(nSpace+1);
				m_strCmdLine = m_strCmdLine.substr(0, nSpace);
			}
		}
		// For all other versions, read the parameters from the stream
		else
		{
			dataReader >> m_strParameters;
		}

		dataReader >> m_bBlocking;

		if (nDataVersion >= 3)
		{
			// Read the propagate errors value
			dataReader >> m_bPropagateErrors;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12215");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI25459", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << gnCurrentVersion;
		dataWriter << m_strCmdLine;
		dataWriter << m_strWorkingDir;
		dataWriter << m_strParameters;
		dataWriter << m_bBlocking;
		dataWriter << m_bPropagateErrors;

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12216");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI25458", pbValue != __nullptr);

		// Both command-line and working directory must be defined
		bool bConfigured = !(m_strCmdLine.empty() || m_strWorkingDir.empty());

		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12217");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILaunchAppFileProcessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::get_CommandLine(BSTR *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI25464", pRetVal != __nullptr);

		*pRetVal = _bstr_t(m_strCmdLine.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12218");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::put_CommandLine(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		string strCmdLine = asString(newVal);
		if (strCmdLine.size() == 0)
		{
			UCLIDException ue("ELI12219", "Command Line cannot be empty.");
			throw ue;
		}

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI14475", ipFAMTagManager != __nullptr);

		// make sure the file contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strCmdLine.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI14476", "Command Line contains invalid tags!");
			ue.addDebugInfo("File", strCmdLine);
			throw ue;
		}

		m_strCmdLine = strCmdLine;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12220");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::get_WorkingDirectory(BSTR *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI25465", pRetVal != __nullptr);

		*pRetVal = _bstr_t(m_strWorkingDir.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12221");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::put_WorkingDirectory(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		string strWorkingDir = asString(newVal);
		if (strWorkingDir.size() == 0)
		{
			UCLIDException ue("ELI12222", "Working Directory name cannot be empty.");
			throw ue;
		}
		
		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI14477", ipFAMTagManager != __nullptr);

		// make sure the file contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strWorkingDir.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI14478", "Working Directory name contains invalid tags!");
			ue.addDebugInfo("File", strWorkingDir);
			throw ue;
		}

		m_strWorkingDir = strWorkingDir;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12223");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::get_Parameters(BSTR* pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI25474", pRetVal != __nullptr);

		*pRetVal = _bstr_t(m_strParameters.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25472");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::put_Parameters(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_strParameters = asString(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25473");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::get_IsBlocking(VARIANT_BOOL *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI25466", pRetVal != __nullptr);

		*pRetVal = asVariantBool(m_bBlocking);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12226");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::put_IsBlocking(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bBlocking = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12227");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::get_PropagateErrors(VARIANT_BOOL* pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI28973", pbVal != __nullptr);

		*pbVal = asVariantBool(m_bPropagateErrors);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28796");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLaunchAppFileProcessor::put_PropagateErrors(VARIANT_BOOL bVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_bPropagateErrors = asCppBool(bVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28797");
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CLaunchAppFileProcessor::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFILE_ACTION_MANAGER_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI12224", "LaunchApplication File Processor");
}
//-------------------------------------------------------------------------------------------------
