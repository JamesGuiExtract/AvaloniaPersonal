
#include "stdafx.h"
#include "ParagraphTextHandlers.h"
#include "HighlightedTextWindowPTH.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CHighlightedTextWindowPTH
//-------------------------------------------------------------------------------------------------
CHighlightedTextWindowPTH::CHighlightedTextWindowPTH()
: m_ipInputManager(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CHighlightedTextWindowPTH::~CHighlightedTextWindowPTH()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20407");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindowPTH::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IParagraphTextHandler,
		&IID_ILicensedComponent,
		&IID_IHighlightedTextWindowPTH
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IHighlightedTextWindowPTH
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindowPTH::SetInputManager(IInputManager *pInputManager)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState())

		// ensure that this component is licensed.
		validateLicense();

		// store the reference to the input manager 
		m_ipInputManager = pInputManager;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02797")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindowPTH::raw_NotifyParagraphTextRecognized(ISpotRecognitionWindow *pSourceSRWindow, 
																		  ISpatialString *pText)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState())

		// ensure that this component is licensed.
		validateLicense();

		// ensure proper spatial text passed in
		ISpatialStringPtr ipText(pText);
		ASSERT_RESOURCE_ALLOCATION("ELI06544", ipText != NULL);

		// if the input manager reference has not been specified, then
		// throw an exception
		if (m_ipInputManager == NULL)
		{
			throw UCLIDException("ELI02800", "Input Manager reference has not been specified!");
		}

		// create an instance of the highlighted text window

		UCLID_HIGHLIGHTEDTEXTIRLib::IHighlightedTextWindowPtr ipHTWindow(CLSID_HighlightedTextWindow);
		ASSERT_RESOURCE_ALLOCATION("ELI02801", ipHTWindow != NULL);

		// do a QI for the InputReceiver interface
		UCLID_INPUTFUNNELLib::IInputReceiverPtr ipIR = ipHTWindow;
		if (ipIR == NULL)
		{
			throw UCLIDException("ELI02802", "The Highlighted Text Window does not support IInputReceiver interface!");
		}

		// connect the IR to the input manager
		long lHandle = m_ipInputManager->ConnectInputReceiver(ipIR);

		// initialize the window with the specific text
		ipHTWindow->SetText(ipText->String);

		// set current indirect source name
		ipHTWindow->SetIndirectSource(pSourceSRWindow->GetImageFileName());

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02798")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindowPTH::raw_GetPTHDescription(BSTR *pstrDescription)
{
	*pstrDescription = _bstr_t("Send text to Highlighted Text Window").copy();
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindowPTH::raw_IsPTHEnabled(VARIANT_BOOL *pbEnabled)
{
	// this PTH is always enabled
	*pbEnabled = VARIANT_TRUE;
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindowPTH::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Helper function
//-------------------------------------------------------------------------------------------------
void CHighlightedTextWindowPTH::validateLicense()
{
	static const unsigned long ulTHIS_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( ulTHIS_COMPONENT_ID, "ELI02799",
		"Highlighted Text Window PTH" );
}
//-------------------------------------------------------------------------------------------------
