
#include "stdafx.h"
#include "IFCore.h"
#include "InputManagerSingleton.h"

#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// static/global variables
//CComPtr<IInputManager> CInputManagerSingleton::m_ipInputManager;

//-------------------------------------------------------------------------------------------------
// CInputManagerSingleton
//-------------------------------------------------------------------------------------------------
CInputManagerSingleton::CInputManagerSingleton()
{
}
//--------------------------------------------------------------------------------------------------
CInputManagerSingleton::~CInputManagerSingleton()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16462");}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManagerSingleton::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInputManagerSingleton
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IInputManagerSingleton
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManagerSingleton::GetInstance(IInputManager **pInputManager)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// create the singleton instance if it has not yet been created
		if (m_ipInputManager == __nullptr)
		{
			m_ipInputManager.CreateInstance(CLSID_InputManager);
			ASSERT_RESOURCE_ALLOCATION("ELI03684", m_ipInputManager != __nullptr);

			// by default, for the singleton input manager, make the main application
			// window as the parent window
			long parentWndHandle = (long) AfxGetApp()->m_pMainWnd;
			if (parentWndHandle != 0)
			{
				m_ipInputManager->ParentWndHandle = parentWndHandle;
			}
		}

		// copy m_ipInputManager.  
		UCLID_INPUTFUNNELLib::IInputManagerPtr ipShallowCopy = m_ipInputManager;
		*pInputManager = (IInputManager*) ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03685")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManagerSingleton::DeleteInstance()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// delete instance if the instance exists
		if (m_ipInputManager != __nullptr)
		{
			m_ipInputManager = __nullptr;
		}	
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04358")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
