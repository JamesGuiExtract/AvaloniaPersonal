
#include "stdafx.h"
#include "WindowIRManager.h"
#include "WindowIRMessages.h"

#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>

#include <algorithm>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//--------------------------------------------------------------------------------------------------
WindowIRManager::WindowIRManager(IInputManager *pInputManager)
:m_pInputManager(pInputManager), m_bInputEnabled(false)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	if (!CreateEx(NULL, AfxRegisterWndClass(NULL), "", NULL, 0, 0, 0, 0, NULL, NULL))
	{
		throw UCLIDException("ELI04977", "Unable to create window!");
	}
};
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(WindowIRManager, CWnd)
	//{{AFX_MSG_MAP(WindowIRManager)
	//}}AFX_MSG_MAP
	ON_MESSAGE(WM_CONNECT_WINDOW_IR, OnConnectWindowIR)
	ON_MESSAGE(WM_DISCONNECT_WINDOW_IR, OnDisconnectWindowIR)
	ON_MESSAGE(WM_PROCESS_INPUT, OnProcessInput)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
LRESULT WindowIRManager::OnConnectWindowIR(WPARAM wParam, LPARAM lParam)
{
	try
	{
		// add the specified window into the vector
		// ensure that the window handle is not doubly-added to the vector
		HWND hWnd = (HWND) wParam;
		vector<HWND>::iterator iter = find(m_vecIRWndHandles.begin(), 
			m_vecIRWndHandles.end(), hWnd);

		// if the hWnd was not found, add it to our vector.
		if (iter == m_vecIRWndHandles.end())
		{
			m_vecIRWndHandles.push_back(hWnd);

			// if input is currently enabled in the InputFunnel, send appropriate
			// messages to the newly added external window IR
			if (m_bInputEnabled)
			{
				NotifyInputEnabled(hWnd);
			}
			else
			{
				NotifyInputDisabled(hWnd);
			}

			return TRUE;
		}
		else
		{
			// call was not successful because the hWnd was already in our vector
			return FALSE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04982")

	return FALSE;
}
//--------------------------------------------------------------------------------------------------
LRESULT WindowIRManager::OnDisconnectWindowIR(WPARAM wParam, LPARAM lParam)
{
	try
	{
		// get the hWnd and find the hWnd in the vector
		HWND hWnd = (HWND) wParam;
		vector<HWND>::iterator iter = find(m_vecIRWndHandles.begin(), 
			m_vecIRWndHandles.end(), hWnd);

		// if the hWnd was found, delete it from our vector.
		if (iter != m_vecIRWndHandles.end())
		{
			m_vecIRWndHandles.erase(iter);
			return TRUE;
		}
		else
		{
			// call was not successful because the hWnd was not in our vector
			return FALSE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04981")

	return FALSE;
}
//--------------------------------------------------------------------------------------------------
LRESULT WindowIRManager::OnProcessInput(WPARAM wParam, LPARAM lParam)
{
	try
	{
		char pszInput[256];
		GlobalGetAtomName((ATOM) wParam, pszInput, sizeof(pszInput));

		_bstr_t _bstrInput = pszInput;
		m_pInputManager->ProcessTextInput(_bstrInput);
		
		// if everything went OK, return TRUE
		return TRUE;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI04980")

	// if we reached here, that's because an exception was thrown.
	// So, return FALSE
	return FALSE;
}
//--------------------------------------------------------------------------------------------------
void WindowIRManager::NotifyInputEnabled(HWND hWnd)
{
	WPARAM wParam = GlobalAddAtom(m_strInputType.c_str());
	LPARAM lParam = GlobalAddAtom(m_strPrompt.c_str());
	::PostMessage(hWnd, WM_NOTIFY_INPUT_ENABLED, wParam, lParam);
}
//--------------------------------------------------------------------------------------------------
void WindowIRManager::NotifyInputEnabled(const string& strInputType, 
										 const string& strPrompt)
{
	try
	{
		// update variables
		m_bInputEnabled = true;
		m_strInputType = strInputType;
		m_strPrompt = strPrompt;

		// post a message to each of the window IRs notifying them that input has been
		// enabled
		vector<HWND>::const_iterator iter;
		for (iter = m_vecIRWndHandles.begin(); iter != m_vecIRWndHandles.end(); iter++)
		{
			NotifyInputEnabled(*iter);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04978")
}
//--------------------------------------------------------------------------------------------------
void WindowIRManager::NotifyInputDisabled(HWND hWnd)
{
	::PostMessage(hWnd, WM_NOTIFY_INPUT_DISABLED, 0, 0);
}
//--------------------------------------------------------------------------------------------------
void WindowIRManager::NotifyInputDisabled()
{
	try
	{
		m_bInputEnabled = false;

		// post a message to each of the window IRs notifying them that input has been
		// disabled
		vector<HWND>::const_iterator iter;
		for (iter = m_vecIRWndHandles.begin(); iter != m_vecIRWndHandles.end(); iter++)
		{
			NotifyInputDisabled(*iter);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04979")
}
//--------------------------------------------------------------------------------------------------
