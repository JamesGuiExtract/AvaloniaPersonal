#include "stdafx.h"
#include "IcoMapApp.h"
#include "IcoMapInputContext.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CIcoMapInputContext
//-------------------------------------------------------------------------------------------------
CIcoMapInputContext::CIcoMapInputContext()
: m_pIcoMapApplication(NULL)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// create the instance of the land-records input context so that
		// we can delegate incoming calls to it.
		if (FAILED(m_ipLandRecordsIC.CreateInstance(CLSID_LandRecordsIC)))
		{
			throw UCLIDException("ELI04129", "Unable to create instance of the Land Records Context!");
		}
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI04125")
}
//-------------------------------------------------------------------------------------------------
CIcoMapInputContext::~CIcoMapInputContext()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI04126")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapInputContext::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInputContext,
		&IID_IIcoMapInputContext,
		&IID_ISRWEventHandler,
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
// IIcoMapInputContext
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapInputContext::SetIcoMapApplication(IIcoMapApplication* pIcoMapApplication)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_pIcoMapApplication = pIcoMapApplication;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11865");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IInputContext
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapInputContext::raw_Activate(IInputManager* pInputManager)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// find all Spot-recognition IR's and set this object
		// as the event handler for spot-recognition-IR events
		IInputManagerPtr ipInputMgr(pInputManager);
		if (ipInputMgr)
		{	
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

		// delegate call to LandRecordsIC
		if (m_ipLandRecordsIC)
		{
			m_ipLandRecordsIC->Activate(pInputManager);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04127");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapInputContext::raw_NotifyNewIRConnected(IInputReceiver *pNewInputReceiver)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// if the input receiver is a SRIR, then set this object
		// as the event handler for spot-recognition-IR events
		connectSRIR(pNewInputReceiver);

		// delegate call to LandRecordsIC
		if (m_ipLandRecordsIC)
		{
			m_ipLandRecordsIC->NotifyNewIRConnected(pNewInputReceiver);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04128");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ISRWEventHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapInputContext::raw_AboutToRecognizeParagraphText()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

/*	try
	{
		validateLicense();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04131");*/

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapInputContext::raw_AboutToRecognizeLineText()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

/*	try
	{
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04132");*/

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapInputContext::raw_NotifyKeyPressed(long nKeyCode, short shiftState)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_pIcoMapApplication != NULL)
		{
			// if F12 key is pressed, call finish sketch
			ICOMAPAPPLib::IIcoMapApplicationPtr ipIcoMapApp(m_pIcoMapApplication);
			ipIcoMapApp->ProcessKeyDown(nKeyCode, shiftState);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11853");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapInputContext::raw_NotifyCurrentPageChanged()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapInputContext::raw_NotifyEntitySelected(long nZoneID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11382")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapInputContext::raw_NotifyZoneEntityCreated(long nZoneID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11383")

	return S_OK;
}//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapInputContext::raw_NotifyFileOpened(BSTR bstrFileFullPath)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13258")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapInputContext::raw_NotifyOpenToolbarButtonPressed(VARIANT_BOOL *pbContinueWithOpen)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pbContinueWithOpen = VARIANT_TRUE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19485")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapInputContext::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
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

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
bool CIcoMapInputContext::connectSRIR(IInputReceiver *pNewInputReceiver)
{
	bool bSuccess = false;
	ISpotRecognitionWindowPtr ipSpotRecIR(pNewInputReceiver);
	if (ipSpotRecIR)
	{
		// Set this object as the event handler for spot-recognition-IR events
		ipSpotRecIR->SetSRWEventHandler(this);
		
		bSuccess = true;
	}

	return bSuccess;
}
//-------------------------------------------------------------------------------------------------
void CIcoMapInputContext::validateLicense()
{
	static const unsigned long THIS_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_ID, "ELI07463", "IcoMap Input Context");
}
//-------------------------------------------------------------------------------------------------
