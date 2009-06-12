// InteractiveTestExecuter.cpp : Implementation of CInteractiveTestExecuter
#include "stdafx.h"
#include "UCLIDTestingFrameworkCore.h"
#include "InteractiveTestExecuter.h"
#include "InteractiveTestDlg.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CInteractiveTestExecuter
//-------------------------------------------------------------------------------------------------
CInteractiveTestExecuter::CInteractiveTestExecuter()
{
}
//-------------------------------------------------------------------------------------------------
CInteractiveTestExecuter::~CInteractiveTestExecuter()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16541");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInteractiveTestExecuter::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInteractiveTestExecuter,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IInteractiveTestExecuter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInteractiveTestExecuter::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	try
	{
		// Check licensing
		validateLicense();

		m_ipResultLogger = pLogger;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02294")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInteractiveTestExecuter::raw_ExecuteITCFile(BSTR strITCFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		// ensure that SetResultLogger() has been called
		if (!m_ipResultLogger.GetInterfacePtr())
		{
			UCLIDException ue("ELI02250", "Test Result Logger not initialized!");
			throw ue;
		}

		// bring up the dialog box to execute the test cases
		_bstr_t bstrITCFile(strITCFile);
		InteractiveTestDlg dlg(m_ipResultLogger, bstrITCFile);
		dlg.DoModal();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02247")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInteractiveTestExecuter::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Failure of validateLicense is indicated by an exception being thrown
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper methods
//-------------------------------------------------------------------------------------------------
void CInteractiveTestExecuter::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI07043", "Interactive Test Executor" );
}
//-------------------------------------------------------------------------------------------------
