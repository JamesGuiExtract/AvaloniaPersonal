// PDFInputOutputManager.cpp : Implementation of CPDFInputOutputManager
#include "stdafx.h"
#include "UCLIDImageUtils.h"
#include "PDFInputOutputManager.h"

#include <ComponentLicenseIDs.h>
#include <ComUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <MiscLeadUtils.h>
#include <PDFInputOutputMgr.h>
#include <UCLIDException.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CPDFInputOutputManager
//-------------------------------------------------------------------------------------------------
CPDFInputOutputManager::CPDFInputOutputManager()
{
	try
	{
		// Ensure the PDF manager pointer is NULL
		m_apPDFManager.reset();

		// If PDF support is licensed initialize support
		// NOTE: no exception is thrown or logged if PDF support is not licensed.
		initPDFSupport();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI27818");
}
//-------------------------------------------------------------------------------------------------
CPDFInputOutputManager::~CPDFInputOutputManager()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27819");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPDFInputOutputManager::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IPDFInputOutputManager,
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
// IPDFInputOutputManager
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPDFInputOutputManager::SetFileData(BSTR bstrOriginalFileName,
												 VARIANT_BOOL bFileUsedAsInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Ensure the file name is not empty string
		string strOriginalFileName = asString(bstrOriginalFileName);
		ASSERT_ARGUMENT("ELI27831", !strOriginalFileName.empty());

		// Set the new PDF manager for the specified file
		m_apPDFManager.reset(new PDFInputOutputMgr(strOriginalFileName,
			asCppBool(bFileUsedAsInput)));
		ASSERT_RESOURCE_ALLOCATION("ELI27830", m_apPDFManager.get() != NULL);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27820")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPDFInputOutputManager::IsInputFile(VARIANT_BOOL *pbIsInputFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27821", pbIsInputFile != NULL);

		if (m_apPDFManager.get() == NULL)
		{
			UCLIDException ue("ELI27832", "PDF Input/Output manager has not been configured yet.");
			throw ue;
		}

		// Get the return value
		*pbIsInputFile = asVariantBool(m_apPDFManager->isInputFile());

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27822")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPDFInputOutputManager::get_FileName(BSTR* pbstrFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27823", pbstrFileName != NULL);

		if (m_apPDFManager.get() == NULL)
		{
			UCLIDException ue("ELI27833", "PDF Input/Output manager has not been configured yet.");
			throw ue;
		}

		// Get the file name
		*pbstrFileName = _bstr_t(m_apPDFManager->getFileName().c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27824")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPDFInputOutputManager::get_FileNameInformationString(BSTR* pbstrFileNameInformationString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27825", pbstrFileNameInformationString != NULL);

		if (m_apPDFManager.get() == NULL)
		{
			UCLIDException ue("ELI27834", "PDF Input/Output manager has not been configured yet.");
			throw ue;
		}

		// Get the file name information string
		*pbstrFileNameInformationString = _bstr_t(
			m_apPDFManager->getFileNameInformationString().c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27826");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPDFInputOutputManager::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI27827", pbValue != NULL);

		try
		{
			validateLicense();
			// if validateLicense doesn't throw any exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27828");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CPDFInputOutputManager::validateLicense()
{
	VALIDATE_LICENSE(gnEXTRACT_CORE_OBJECTS, "ELI27829", "PDF Input Output Manager" );
}
//-------------------------------------------------------------------------------------------------