// FileProcessingManagerProcess.cpp : Implementation of CFileProcessingManagerProcess
#include "stdafx.h"
#include "FileProcessingManagerProcess.h"

#include <ComponentLicenseIDs.h>
#include <ComUtils.h>
#include <cppUtil.h>
#include <LicenseMgmt.h>
#include <UCLIDException.h>
 
// Add license management functions
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
static const long gnSTOP_PROCESSING_TIMEOUT = 60000;

//--------------------------------------------------------------------------------------------------
// CFileProcessingManagerProcess
//--------------------------------------------------------------------------------------------------
CFileProcessingManagerProcess::CFileProcessingManagerProcess() :
m_pUnkMarshaler(NULL)
{
	try
	{
		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28441");
}
//--------------------------------------------------------------------------------------------------
HRESULT CFileProcessingManagerProcess::FinalConstruct()
{
	try
	{
		m_ipFPM.CreateInstance(CLSID_FileProcessingManager);
		ASSERT_RESOURCE_ALLOCATION("ELI28463", m_ipFPM != NULL);

		return CoCreateFreeThreadedMarshaler(
			GetControllingUnknown(), &m_pUnkMarshaler.p);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28464");
}
//--------------------------------------------------------------------------------------------------
void CFileProcessingManagerProcess::FinalRelease()
{
	try
	{
		if (m_ipFPM != NULL && m_ipFPM->ProcessingStarted == VARIANT_TRUE)
		{
			m_ipFPM->StopProcessing();
			long nCount = 0;
			while (nCount < gnSTOP_PROCESSING_TIMEOUT && m_ipFPM->ProcessingStarted == VARIANT_TRUE)
			{
				Sleep(100);
				nCount += 100;
			}
		}
			
		m_ipFPM = NULL;
		m_pUnkMarshaler.Release();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28465");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManagerProcess::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFileProcessingManagerProcess,
		&IID_ILicensedComponent
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
STDMETHODIMP CFileProcessingManagerProcess::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI28442", pbValue != NULL);

		try
		{
			// Check license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch (...)
		{
			*pbValue = VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28443");
}

//--------------------------------------------------------------------------------------------------
// IFileProcessingManagerProcess
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManagerProcess::Ping()
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28444");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManagerProcess::Start(LONG lNumberOfFilesToProcess)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		validateLicense();

		// Check if already running
		if (m_ipFPM->ProcessingStarted == VARIANT_TRUE)
		{
			UCLIDException uex("ELI28469", "This object is already running.");
			throw uex;
		}

		// Ensure the FPS file is valid
		if (!isValidFile(m_strFPSFile))
		{
			UCLIDException uex("ELI28470", "Cannot process, the specified FPS file cannot be found.");
			uex.addDebugInfo("FPS File", m_strFPSFile);
			throw uex;
		}

		// Load the FPS file into the File processing manager
		m_ipFPM->LoadFrom(m_strFPSFile.c_str(), VARIANT_FALSE);

		if (m_ipFPM->IsDBPasswordRequired == VARIANT_TRUE)
		{
			UCLIDException uex("ELI28475",
				"DB Admin password required for current configuration, cannot process as a service.");
			throw uex;
		}

		// Set the number of files to process
		m_ipFPM->NumberOfDocsToProcess = lNumberOfFilesToProcess;

		// Start the processing
		m_ipFPM->StartProcessing(VARIANT_TRUE);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28445");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManagerProcess::Stop()
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		// Only call stop processing if we have started
		if (m_ipFPM->ProcessingStarted == VARIANT_TRUE)
		{
			m_ipFPM->StopProcessing();

			// Wait for processing to complete
			// Note: This may take a long time if processing a large document
			while (m_ipFPM->ProcessingStarted == VARIANT_TRUE)
			{
				Sleep(100);
			}
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28446");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManagerProcess::GetCounts(LONG *plNumFilesProcessed,
													  LONG *plNumProcessingErrors,
													  LONG *plNumFilesSupplied,
													  LONG *plNumSupplyingErrors)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		validateLicense();

		// Get the counts from the File processing manager
		m_ipFPM->GetCounts(plNumFilesProcessed, plNumProcessingErrors, plNumFilesSupplied,
			plNumSupplyingErrors);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28449");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManagerProcess::get_ProcessID(LONG* plPID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28447", plPID != NULL);

		*plPID = (LONG) GetCurrentProcessId();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28448");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManagerProcess::get_IsRunning(VARIANT_BOOL* pvbRunning)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28458", pvbRunning != NULL);

		*pvbRunning = m_ipFPM->ProcessingStarted;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28459");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManagerProcess::get_FPSFile(BSTR* pbstrFPSFile)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28460", pbstrFPSFile != NULL);

		*pbstrFPSFile = _bstr_t(m_strFPSFile.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28461");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManagerProcess::put_FPSFile(BSTR bstrFPSFile)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		validateLicense();

		string strFPSFile = asString(bstrFPSFile);
		if (m_ipFPM->ProcessingStarted == VARIANT_TRUE)
		{
			UCLIDException uex("ELI28471", "Cannot change FPS file while actively running.");
			uex.addDebugInfo("Current FPS File", m_strFPSFile);
			uex.addDebugInfo("New FPS File", strFPSFile);
			throw uex;
		}

		m_strFPSFile = strFPSFile;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28462");
}

//--------------------------------------------------------------------------------------------------
// Private helper methods
//--------------------------------------------------------------------------------------------------
void CFileProcessingManagerProcess::validateLicense()
{
	// Using same license ID as ProcessFiles.exe
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI28450", "File Processing Manager Process");
}
//--------------------------------------------------------------------------------------------------
