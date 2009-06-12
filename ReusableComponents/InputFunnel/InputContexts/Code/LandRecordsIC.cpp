// LandRecordsIC.cpp : Implementation of CLandRecordsIC
#include "stdafx.h"
#include "InputContexts.h"
#include "LandRecordsIC.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CLandRecordsIC
//-------------------------------------------------------------------------------------------------
CLandRecordsIC::CLandRecordsIC()
:m_ipParagraphTextCorrector(NULL),
 m_ipParagraphTextHandlers(NULL),
 m_ipSubImageHandler(NULL),
 m_ipMCRLineTextCorrector(NULL),
 m_ipLineTextEvaluator(NULL)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		init();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI04116")
}
//-------------------------------------------------------------------------------------------------
CLandRecordsIC::~CLandRecordsIC()
{
	try
	{
		m_ipParagraphTextCorrector = NULL;
		m_ipParagraphTextHandlers = NULL;
		m_ipSubImageHandler = NULL;
		m_ipMCRLineTextCorrector = NULL;
		m_ipLineTextEvaluator = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI04123")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLandRecordsIC::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInputContext,
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
// IInputContext
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLandRecordsIC::raw_Activate(IInputManager* pInputManager)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		IInputManagerPtr ipInputMgr(pInputManager);
		if (ipInputMgr)
		{
			// Set the input manager for the highlighted text window PTH, which we know 
			// is the first PTH in the vector of PTHs
			IHighlightedTextWindowPTHPtr ipHighlightedTextWindowPTH = 
				m_ipParagraphTextHandlers->At(0);
			ipHighlightedTextWindowPTH->SetInputManager(ipInputMgr);
			
			// Set input manager for SRWSubImageHandler
			ISRWSubImageHandlerPtr ipSRWSubImageHandler(m_ipSubImageHandler);
			ipSRWSubImageHandler->SetInputManager(ipInputMgr);
		
			// iterate through all input receivers and update the 
			// handlers for spot recognition
			IIUnknownVectorPtr ipInputReceivers(ipInputMgr->GetInputReceivers());
			if (ipInputReceivers != NULL && ipInputReceivers->Size() > 0)
			{
				long nSize = ipInputReceivers->Size();
				for (long n=0; n<nSize; n++)
				{
					IInputReceiverPtr ipInputReceiver(ipInputReceivers->At(n));
					connectSRIR(ipInputReceiver);
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04110")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLandRecordsIC::raw_NotifyNewIRConnected(IInputReceiver *pNewInputReceiver)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// check what kind of input receiver it is
		connectSRIR(pNewInputReceiver);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04111")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLandRecordsIC::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		// If validateLicense doesn't throw any exception, pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
bool CLandRecordsIC::connectSRIR(IInputReceiver *pNewInputReceiver)
{
	bool bSuccess = false;
	ISpotRecognitionWindowPtr ipSpotRecIR(pNewInputReceiver);
	if (ipSpotRecIR)
	{
		// Set the line text evaluator, line text corrector, paragraph text 
		// handler, and paragraph text corrector properties
		ipSpotRecIR->SetLineTextCorrector(m_ipMCRLineTextCorrector);
		ipSpotRecIR->SetLineTextEvaluator(m_ipLineTextEvaluator);
		ipSpotRecIR->SetParagraphTextCorrector(m_ipParagraphTextCorrector);
		ipSpotRecIR->SetParagraphTextHandlers(m_ipParagraphTextHandlers);
		ipSpotRecIR->SetSubImageHandler(m_ipSubImageHandler, 
			_bstr_t("Crop and display a portion of the image in a new SpotRecognitionWindow"),	_bstr_t(""));
		
		bSuccess = true;
	}

	return bSuccess;
}
//-------------------------------------------------------------------------------------------------
void CLandRecordsIC::init()
{
	//*********************************
	// create variables for spot rec
	if (FAILED(m_ipParagraphTextCorrector.CreateInstance(CLSID_LegalDescriptionTextCorrector)))
	{
		throw UCLIDException("ELI04117","Unable to create LegalDescriptionTextCorrector object!");
	}
	else if (FAILED(m_ipParagraphTextHandlers.CreateInstance(CLSID_IUnknownVector)))
	{
		throw UCLIDException("ELI04807", "Unable to create vector object!");
	}
	else if (FAILED(m_ipMCRLineTextCorrector.CreateInstance(CLSID_MCRTextCorrector)))
	{
		throw UCLIDException("ELI04119", "Unable to create MCRTextCorrector object!");
	}
	else if (FAILED(m_ipLineTextEvaluator.CreateInstance(CLSID_MCRLineTextEvaluator)))
	{
		throw UCLIDException("ELI04120", "Unable to create MCRLineTextEvaluator object!");
	}
	else if (FAILED(m_ipSubImageHandler.CreateInstance(CLSID_SRWSubImageHandler)))
	{
		throw UCLIDException("ELI04121", "Unable to create SRWSubImageHandler object!");
	}
	
	// create and add PTHs to the vector of PTHs
	IParagraphTextHandlerPtr ipPTH(CLSID_HighlightedTextWindowPTH);
	if (ipPTH == NULL)
	{
		throw UCLIDException("ELI04118", "Unable to create HighlightedTextWindowPTH object!");
	}
	else
	{
		m_ipParagraphTextHandlers->PushBack(ipPTH);
	}
}
//-------------------------------------------------------------------------------------------------
void CLandRecordsIC::validateLicense()
{
	static const unsigned long LANDRECORDS_IC_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( LANDRECORDS_IC_COMPONENT_ID, 
		"ELI04112", "LandRecords Input Context" );
}
//-------------------------------------------------------------------------------------------------
