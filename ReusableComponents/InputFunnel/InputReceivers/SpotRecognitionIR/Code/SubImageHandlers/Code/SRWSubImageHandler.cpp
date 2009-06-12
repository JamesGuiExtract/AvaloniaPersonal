// SRWSubImageHandler.cpp : Implementation of CSRWSubImageHandler
#include "stdafx.h"
#include "SubImageHandlers.h"
#include "SRWSubImageHandler.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CSRWSubImageHandler
//-------------------------------------------------------------------------------------------------
CSRWSubImageHandler::CSRWSubImageHandler()
{
}
//-------------------------------------------------------------------------------------------------
CSRWSubImageHandler::~CSRWSubImageHandler()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20408");
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRWSubImageHandler::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISRWSubImageHandler,
		&IID_ILicensedComponent,
		&IID_ISubImageHandler
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ISubImageHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRWSubImageHandler::raw_NotifySubImageCreated(ISpotRecognitionWindow *pSourceSRWindow, 
															IRasterZone *pSubImageZone, 
															double dRotationAngle)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// ensure that this component is licensed.
		validateLicense();

		if (m_ipInputManager == NULL)
		{
			throw UCLIDException("ELI03254", "InputManager reference has not been specified!");
		}

		// get the source SRW
		ISpotRecognitionWindowPtr ipSourceSRWindow(pSourceSRWindow);
		if (ipSourceSRWindow == NULL)
		{
			throw UCLIDException("ELI03279", "Parent spot recognition window is NULL.");
		}

		// create an instance of the Spot rec window
		ISpotRecognitionWindowPtr ipSRWindow(CLSID_SpotRecognitionWindow);
		if (ipSRWindow == NULL)
		{
			throw UCLIDException("ELI03255", "Unable to create an instance of the SpotRecognition Window!");
		}
		// do a QI for the InputReceiver interface
		IInputReceiverPtr ipIR(ipSRWindow);

		// connect the IR to the input manager
		long lHandle = m_ipInputManager->ConnectInputReceiver(ipIR);
		
		ILineTextEvaluatorPtr ipLineTextEvaluator(ipSourceSRWindow->GetLineTextEvaluator());
		if (ipLineTextEvaluator != NULL)
		{
			if (FAILED(ipSRWindow->SetLineTextEvaluator(ipLineTextEvaluator)))
			{
				throw UCLIDException("ELI03265", "Unable to associate a Line Text Evaluator with the Spot Recognition Window!");
			}
		}
		
		ILineTextCorrectorPtr ipLineTextCorrector(ipSourceSRWindow->GetLineTextCorrector());
		if (ipLineTextCorrector != NULL)
		{
			ipSRWindow->SetLineTextCorrector(ipLineTextCorrector);
		}
		
		IParagraphTextCorrectorPtr ipParagraphTextCorrector(ipSourceSRWindow->GetParagraphTextCorrector());
		if (ipParagraphTextCorrector != NULL)
		{
			ipSRWindow->SetParagraphTextCorrector(ipParagraphTextCorrector);
		}

		CComBSTR bstrTooltip, bstrTrainingFile;
		IIUnknownVectorPtr ipPTHs;
		ipPTHs = ipSourceSRWindow->GetParagraphTextHandlers();
		if (ipPTHs != NULL)
		{
			ipSRWindow->SetParagraphTextHandlers(ipPTHs);
		}

		ISRWEventHandlerPtr ipSRWEventHandler(ipSourceSRWindow->GetSRWEventHandler());
		if (ipSRWEventHandler != NULL)
		{
			ipSRWindow->SetSRWEventHandler(ipSRWEventHandler);
		}
		
		// pass this image handler to the new spot rec window
		CComQIPtr<ISubImageHandler> ipSubImageHandler;
		ipSourceSRWindow->GetSubImageHandler(&ipSubImageHandler, &bstrTooltip, &bstrTrainingFile);
		ipSRWindow->SetSubImageHandler(ipSubImageHandler, _bstr_t(bstrTooltip), _bstr_t(bstrTrainingFile));

		ipSRWindow->OpenImagePortion( ipSourceSRWindow->GetImageFileName(), pSubImageZone, dRotationAngle );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03252")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ISRWSubImageHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRWSubImageHandler::SetInputManager(IInputManager *pInputManager)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// ensure that this component is licensed.
		validateLicense();

		// store the reference to the input manager 
		m_ipInputManager = pInputManager;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03251")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRWSubImageHandler::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Helper functions
//-------------------------------------------------------------------------------------------------
void CSRWSubImageHandler::validateLicense()
{
	static const unsigned long ulTHIS_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( ulTHIS_COMPONENT_ID, "ELI03250",
		"SRW Sub-Image Handler" );
}
//-------------------------------------------------------------------------------------------------
