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
// CFileProcessingManagerProcess
//--------------------------------------------------------------------------------------------------
CFileProcessingManagerProcess::CFileProcessingManagerProcess() : m_pUnkMarshaler(NULL)
{
	try
	{
		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28441");
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
STDMETHODIMP CFileProcessingManagerProcess::Start(BSTR bstrFPSFile)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		validateLicense();

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
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28446");
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
STDMETHODIMP CFileProcessingManagerProcess::GetCounts(LONG *plNumFilesProcessed,
													  LONG *plNumProcessingErrors,
													  LONG *plNumFilesSupplied,
													  LONG *plNumSupplyingErrors)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		validateLicense();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28449");
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
