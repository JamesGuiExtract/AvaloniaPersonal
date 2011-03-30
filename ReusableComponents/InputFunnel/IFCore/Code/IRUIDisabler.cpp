// IRUIDisabler.cpp : Implementation of CIRUIDisabler
#include "stdafx.h"
#include "IFCore.h"
#include "IRUIDisabler.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CIRUIDisabler
//-------------------------------------------------------------------------------------------------
CIRUIDisabler::CIRUIDisabler()
{
}
//-------------------------------------------------------------------------------------------------
CIRUIDisabler::~CIRUIDisabler()
{
	try
	{
		// enable these windows
		int nSize = m_vecWndHandles.size();
		for (int n=0; n<nSize; n++)
		{
			CWnd *pIRWnd = CWnd::FromHandle(m_vecWndHandles[n]);
			pIRWnd->EnableWindow(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07674");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIRUIDisabler::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IIRUIDisabler,
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
// IIRUIDisabler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIRUIDisabler::SetInputManager(IInputManager *pInputManager)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_vecWndHandles.clear();

		UCLID_INPUTFUNNELLib::IInputManagerPtr ipInputManager(pInputManager);
		// if the input manager is null, then use the singleton input manager
		if (ipInputManager == __nullptr)
		{
			// get signleton input manager
			UCLID_INPUTFUNNELLib::IInputManagerSingletonPtr ipSingleton;
			ipSingleton.CreateInstance(CLSID_InputManagerSingleton);
			ipInputManager = ipSingleton->GetInstance();
		}

		// go through all input receivers
		IIUnknownVectorPtr ipIRs = ipInputManager->GetInputReceivers();
		if (ipIRs)
		{
			long nSize = ipIRs->Size();
			for (long n=0; n<nSize; n++)
			{
				// check if this IR has a window
				UCLID_INPUTFUNNELLib::IInputReceiverPtr ipIR = ipIRs->At(n);
				if (ipIR)
				{
					if (ipIR->HasWindow == VARIANT_TRUE)
					{
						// get the window handle
						HWND hwndHandle = (HWND)ipIR->WindowHandle;
						// Create a window object out from the handle
						CWnd *pIRWnd = CWnd::FromHandle(hwndHandle);

						// only disable those windows that are currently enabled
						if (pIRWnd->IsWindowEnabled())
						{
							pIRWnd->EnableWindow(FALSE);
							// store in the vector for destructor to use
							m_vecWndHandles.push_back(hwndHandle);
						}
					}
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07673");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIRUIDisabler::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Private methods
//-------------------------------------------------------------------------------------------------
void CIRUIDisabler::validateLicense()
{
	static const unsigned long THIS_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_ID, "ELI07675", "Input Receiver UI Disabler");
}
//-------------------------------------------------------------------------------------------------

