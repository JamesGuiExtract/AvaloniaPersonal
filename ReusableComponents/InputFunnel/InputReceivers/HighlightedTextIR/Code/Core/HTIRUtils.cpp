// HTIRUtils.cpp : Implementation of CHTIRUtils
#include "stdafx.h"
#include "HighlightedTextIR.h"
#include "HTIRUtils.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CHTIRUtils
//-------------------------------------------------------------------------------------------------
CHTIRUtils::CHTIRUtils()
{
}
//-------------------------------------------------------------------------------------------------
CHTIRUtils::~CHTIRUtils()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20397");
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRUtils::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IHTIRUtils,
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
// IHTIRUtils
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRUtils::IsExactlyOneTextFileOpen(IInputManager *pInputMgr, 
												  VARIANT_BOOL* pbExactOneFileOpen,
												  BSTR *pstrCurrentOpenFileName,
												  IHighlightedTextWindow **pHTIR)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IInputManagerPtr ipInputManager(pInputMgr);
		ASSERT_RESOURCE_ALLOCATION("ELI07141", ipInputManager != NULL);

		*pbExactOneFileOpen = VARIANT_FALSE;
		*pstrCurrentOpenFileName = _bstr_t("");
		*pHTIR = NULL;

		string strTextFileName("");

		// iterate through all the input receivers, and check to see if there 
		// is only one text window open
		IIUnknownVectorPtr ipIRs = ipInputManager->GetInputReceivers();
		ASSERT_RESOURCE_ALLOCATION("ELI07142", ipIRs != NULL);

		// try to find the text window
		long nNumIRs = ipIRs->Size();
		bool bExactlyOneFileWindow = false;
		UCLID_HIGHLIGHTEDTEXTIRLib::IHighlightedTextWindowPtr ipHTIR(NULL);
		for (int i = 0; i < nNumIRs; i++)
		{
			// check to see if the IR is a text window
			UCLID_HIGHLIGHTEDTEXTIRLib::IHighlightedTextWindowPtr ipTempHTIR = ipIRs->At(i);
			
			// if the IR is a spot recognition window
			if (ipTempHTIR)
			{
				// get the name of the text file opened in the HTIR window
				// and see if it matches to the name we're looking for
				string strTempFileName = ipTempHTIR->GetFileName();

				// if there is a window without any file open
				if (strTempFileName.empty())
				{
					// go to the next window
					continue;
				}

				// If there are more than one text window and
				// all have files open
				if (bExactlyOneFileWindow)
				{
					return S_OK;
				}

				strTextFileName = strTempFileName;
				bExactlyOneFileWindow = true;
				ipHTIR = ipTempHTIR;
			}
		}

		if (bExactlyOneFileWindow && ipHTIR != NULL && !strTextFileName.empty())
		{
			*pbExactOneFileOpen = VARIANT_TRUE;
			*pstrCurrentOpenFileName = _bstr_t(strTextFileName.c_str()).copy();
			*pHTIR = (IHighlightedTextWindow*)ipHTIR.Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07140")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRUtils::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		if (pbValue == NULL)
			return E_POINTER;

		// try to ensure that this component is licensed.
		validateLicense();

		// if the above method call does not throw an exception, then this
		// component is licensed.
		*pbValue = VARIANT_TRUE;
	}
	catch (...)
	{
		// if we caught some exception, then this component is not licensed.
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private method
//-------------------------------------------------------------------------------------------------
void CHTIRUtils::validateLicense()
{
	static const unsigned long ulTHIS_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE(ulTHIS_COMPONENT_ID, "ELI07139", "HTIR Utils");
}
//-------------------------------------------------------------------------------------------------

