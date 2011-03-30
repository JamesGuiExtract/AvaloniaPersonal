// SRIRUtils.cpp : Implementation of CSRIRUtils
#include "stdafx.h"
#include "SpotRecognitionIR.h"
#include "SRIRUtils.h"
#include "GDDFileManager.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

#include <io.h>

//-------------------------------------------------------------------------------------------------
// CSRIRUtils
//-------------------------------------------------------------------------------------------------
CSRIRUtils::CSRIRUtils()
{
}
//-------------------------------------------------------------------------------------------------
CSRIRUtils::~CSRIRUtils()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16502");
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRUtils::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISRIRUtils,
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
// ISRIRUtils
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRUtils::GetSRIRWithImage(BSTR strImageFileName, 
										  IInputManager *pInputManager, 
										  VARIANT_BOOL bAutoCreate, 
										  ISpotRecognitionWindow **ppSRIR)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IInputManagerPtr ipInputManager(pInputManager);
		ASSERT_RESOURCE_ALLOCATION("ELI06994", ipInputManager != __nullptr);

		// if the file passed in is actually a GDD file, then get the
		// name of the image file referenced by the GDD file
		string strImage = asString( strImageFileName );
		if (GDDFileManager::sIsGDDFile(strImage))
		{
			strImage = GDDFileManager::sGetImageNameFromGDDFile(strImage);
		}

		// iterate through all the input receivers, and check to see if there 
		// is a spot recognition window already open with the specified image
		IIUnknownVectorPtr ipIRs = ipInputManager->GetInputReceivers();
		ASSERT_RESOURCE_ALLOCATION("ELI06992", ipIRs != __nullptr);

		// try to find the spot recognition window that may already have
		// the image open
		UCLID_SPOTRECOGNITIONIRLib::ISpotRecognitionWindowPtr ipSRIR;
		long nNumIRs = ipIRs->Size();
		for (int i = 0; i < nNumIRs; i++)
		{
			// check to see if the IR is a spot recognition window
			UCLID_SPOTRECOGNITIONIRLib::ISpotRecognitionWindowPtr ipTempSRIR = ipIRs->At(i);
			
			// if the IR is a spot recognition window, check to see if
			// it has the desired image open
			if (ipTempSRIR)
			{
				// regardless of whether the correct image is opened,
				// delete all temporary highlights in the spot recognition 
				// window
				ipTempSRIR->DeleteTemporaryHighlight();

				// get the name of the image opened in the SRIR window
				// and see if it matches to the name we're looking for
				string strTempImage = ipTempSRIR->GetImageFileName();
				if (_strcmpi(strTempImage.c_str(), strImage.c_str()) == 0)
				{
					// we have found the desired spot recognition window
					// break out of the loop
					ipSRIR = ipTempSRIR;
					break;
				}
			}
		}

		// if there is no spot recognition window with the desired image
		// open, then create a new  spot recognition window, and connect 
		// it to the input manager
		if (ipSRIR == __nullptr && bAutoCreate == VARIANT_TRUE)
		{
			// if the image is no longer available or readable, and it is
			// not the empty string then throw an exception
			if (strImage != "" && !fileExistsAndIsReadable(strImage))
			{
				UCLIDException ue("ELI06993", "Unable to open source document image!");
				ue.addDebugInfo("SourceDocName", strImage);
				ue.addWin32ErrorInfo();
				throw ue;
			}

			// create new spot recognition window
			long nID = ipInputManager->CreateNewInputReceiver("Image viewer");
			ipSRIR = ipInputManager->GetInputReceiver(nID);
			ASSERT_RESOURCE_ALLOCATION("ELI06995", ipSRIR != __nullptr);

			// check for empty string before attempting to open the file
			if (strImage != "")
			{
				// open the desired image in the newly created window
				ipSRIR->OpenImageFile(strImage.c_str());
			}
		}

		if (ipSRIR)
		{
			*ppSRIR = (ISpotRecognitionWindow*)ipSRIR.Detach();
		}
		else
		{
			*ppSRIR = NULL;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06991")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRUtils::IsExactlyOneImageOpen(IInputManager *pInputMgr,
											   VARIANT_BOOL* pbExactOneFileOpen,
											   BSTR *pstrCurrentOpenImageName,
											   ISpotRecognitionWindow **ppSRIR)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IInputManagerPtr ipInputManager(pInputMgr);
		ASSERT_RESOURCE_ALLOCATION("ELI07008", ipInputManager != __nullptr);

		*pbExactOneFileOpen = VARIANT_FALSE;
		*pstrCurrentOpenImageName = _bstr_t("").copy();
		*ppSRIR = NULL;

		string strImageFileName("");

		// iterate through all the input receivers, and check to see if there 
		// is only one spot recognition window open
		IIUnknownVectorPtr ipIRs = ipInputManager->GetInputReceivers();
		ASSERT_RESOURCE_ALLOCATION("ELI19118", ipIRs != __nullptr);

		// try to find the spot recognition window
		long nNumIRs = ipIRs->Size();
		bool bExactlyOneImageFile = false;
		UCLID_SPOTRECOGNITIONIRLib::ISpotRecognitionWindowPtr ipSRIR(NULL);
		for (int i = 0; i < nNumIRs; i++)
		{
			// check to see if the IR is a spot recognition window
			UCLID_SPOTRECOGNITIONIRLib::ISpotRecognitionWindowPtr ipTempSRIR = ipIRs->At(i);
			
			// if the IR is a spot recognition window
			if (ipTempSRIR)
			{
				// get the name of the image opened in the SRIR window
				// and see if it matches to the name we're looking for
				string strTempFileName = ipTempSRIR->GetImageFileName();

				if (strTempFileName.empty())
				{
					continue;
				}

				// If there are more than one image window open,
				// return empty image file name
				if (bExactlyOneImageFile)
				{
					return S_OK;
				}

				strImageFileName = strTempFileName;
				bExactlyOneImageFile = true;
				ipSRIR = ipTempSRIR;
			}
		}

		if (bExactlyOneImageFile && ipSRIR != __nullptr && !strImageFileName.empty())
		{
			*pbExactOneFileOpen = VARIANT_TRUE;
			*pstrCurrentOpenImageName = _bstr_t(strImageFileName.c_str()).copy();
			*ppSRIR = (ISpotRecognitionWindow*)ipSRIR.Detach();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07007")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRUtils::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CSRIRUtils::validateLicense()
{
	static const unsigned long ulTHIS_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE(ulTHIS_COMPONENT_ID, "ELI06990", "SRIR Utils");
}
//-------------------------------------------------------------------------------------------------