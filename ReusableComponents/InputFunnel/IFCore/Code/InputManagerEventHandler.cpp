
#include "stdafx.h"
#include "InputManagerEventHandler.h"
#include <UCLIDException.h>

_ATL_FUNC_INFO NotifyInputReceivedInfo =
{
	CC_STDCALL, //calling conv...
	VT_EMPTY, //return value...
	1 , //number of arguments...
	{VT_UNKNOWN} //argumnent types...
};

//--------------------------------------------------------------------------------------------------
InputManagerEventHandler::InputManagerEventHandler()
{
	m_bUseSingleton = false;
	m_ipInputManager = NULL;
}
//--------------------------------------------------------------------------------------------------
InputManagerEventHandler::~InputManagerEventHandler()
{
	try
	{
//			AfxMessageBox("Destructing an InputManagerEventHandler");
//			SetInputManager(NULL);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16572");
}
//--------------------------------------------------------------------------------------------------
void InputManagerEventHandler::UseSingletonInputManager()
{
	if(m_bUseSingleton)
	{
		return;
	}

	
	IInputManagerSingletonPtr ipSingleton(CLSID_InputManagerSingleton);
	ASSERT_RESOURCE_ALLOCATION("ELI10371", ipSingleton != NULL);	
	// Note: This call will set m_bUseSingleton to false so we must
	// set it to true afterward
	// It will also store the Singleton input manager in m_ipInputManager
	// which is badhis is bad
	// We make this call to avoid duplicating the DispEvent code
	SetInputManager(ipSingleton->GetInstance());
	// we DO NOT want to store a pointer to the singleton input manager
	m_ipInputManager = NULL;
	// Use the singleton from now on
	m_bUseSingleton = true;
}
//--------------------------------------------------------------------------------------------------
void InputManagerEventHandler::SetInputManager(IInputManager *pInputManager)
{
	// Get the current input manager
	m_ipInputManager = getInputManager();

	// Use the stored manager from now on
	m_bUseSingleton = false;

	if (m_ipInputManager != NULL)
	{
		DispEventUnadvise(m_ipInputManager, &DIID__IInputManagerEvents);
	}

	if (pInputManager != NULL)
	{
		DispEventAdvise(pInputManager, &DIID__IInputManagerEvents);
	}

	m_ipInputManager = pInputManager;
}
//--------------------------------------------------------------------------------------------------
IInputManagerPtr InputManagerEventHandler::getInputManager()
{
	// return the singleton input manager if appropriate
	if(m_bUseSingleton)
	{
		IInputManagerSingletonPtr ipSingleton(CLSID_InputManagerSingleton);
		ASSERT_RESOURCE_ALLOCATION("ELI10372", ipSingleton != NULL);	
		return ipSingleton->GetInstance();
	}
	else //return the stroed inputmanager
	{
		if(m_ipInputManager == NULL)
		{
			IInputManagerPtr ipTmp = NULL;
			return NULL;
		}
		return m_ipInputManager;
	}
}