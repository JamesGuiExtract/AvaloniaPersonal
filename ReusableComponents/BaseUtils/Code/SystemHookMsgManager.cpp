
#include "stdafx.h"
#include "SystemHookMsgManager.h"
#include "UCLIDException.h"

#include <vector>
#include <map>
#include <algorithm>

using namespace std;

map<HWND, vector<UINT> > g_mapWindowToMsgIds;
map<HWND, ISystemHookMsgHandler *> g_mapWindowToMsgHandler;

//--------------------------------------------------------------------------------------------------
LRESULT CALLBACK BaseUtilsDLL_CallWndProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	try
	{
		static map<HWND, vector<UINT> >::const_iterator iter;
		
		CWPSTRUCT *pData = (CWPSTRUCT *) lParam;
		ASSERT_RESOURCE_ALLOCATION("ELI25241", pData != __nullptr);
		
		// first check whether the message is for one of the windows we care about
		HWND& hwnd = pData->hwnd;
		iter = g_mapWindowToMsgIds.find(hwnd);
		if (iter != g_mapWindowToMsgIds.end())
		{
			// next check if the message is one of the messages we care about
			UINT& message = pData->message;
			const vector<UINT>& vecMsgIds = iter->second;
			if (find(vecMsgIds.begin(), vecMsgIds.end(), message) != vecMsgIds.end())
			{
				// we care about this message, send it to the appropriate msg handler
				map<HWND, ISystemHookMsgHandler*>::iterator iter = g_mapWindowToMsgHandler.find(hwnd);
				if (iter == g_mapWindowToMsgHandler.end())
				{
					THROW_LOGIC_ERROR_EXCEPTION("ELI25243");
				}
				ISystemHookMsgHandler* pHandler = iter->second;
				ASSERT_RESOURCE_ALLOCATION("ELI25244", pHandler != __nullptr);
				pHandler->onMsg(hwnd, message, pData->wParam, pData->lParam);
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI04038")

	return CallNextHookEx(SystemHookMsgManager::sGetInstance().hWH_CALLWNDPROC,
		nCode, wParam, lParam);
}
//--------------------------------------------------------------------------------------------------
SystemHookMsgManager::SystemHookMsgManager()
:hWH_CALLWNDPROC(NULL)
{
	m_vecISystemHookMsgHandlers.clear();
}
//--------------------------------------------------------------------------------------------------
SystemHookMsgManager::SystemHookMsgManager(const SystemHookMsgManager& toCopy)
{
	m_vecISystemHookMsgHandlers = toCopy.m_vecISystemHookMsgHandlers;
}
//--------------------------------------------------------------------------------------------------
SystemHookMsgManager& SystemHookMsgManager::operator = (const SystemHookMsgManager& toAssign)
{
	m_vecISystemHookMsgHandlers = toAssign.m_vecISystemHookMsgHandlers;
	return *this;
}
//--------------------------------------------------------------------------------------------------
void SystemHookMsgManager::addMsgHandler(ISystemHookMsgHandler *pMsgHandler)
{
	// add to current list of ISystemHookMsgHandlers
	std::vector<ISystemHookMsgHandler *>::iterator iter;
	iter = find(m_vecISystemHookMsgHandlers.begin(), 
		m_vecISystemHookMsgHandlers.end(), pMsgHandler);
	if (iter == m_vecISystemHookMsgHandlers.end())
	{
		m_vecISystemHookMsgHandlers.push_back(pMsgHandler);

		refresh();
	}
}
//--------------------------------------------------------------------------------------------------
void SystemHookMsgManager::removeMsgHandler(ISystemHookMsgHandler *pMsgHandler)
{
	// remove the message handler from the vector
	std::vector<ISystemHookMsgHandler *>::iterator iter;
	iter = find(m_vecISystemHookMsgHandlers.begin(), 
		m_vecISystemHookMsgHandlers.end(), pMsgHandler);
	if (iter != m_vecISystemHookMsgHandlers.end())
	{
		m_vecISystemHookMsgHandlers.erase(iter);
	}

	// refresh the windows & messages list
	refresh();
}
//--------------------------------------------------------------------------------------------------
void SystemHookMsgManager::refresh()
{
	// clear the maps
	g_mapWindowToMsgHandler.clear();
	g_mapWindowToMsgIds.clear();

	// update the two maps
	std::vector<ISystemHookMsgHandler *>::iterator iter;
	for (iter = m_vecISystemHookMsgHandlers.begin(); 
		 iter != m_vecISystemHookMsgHandlers.end(); iter++)
	{
		ISystemHookMsgHandler *pMsgHandler = *iter;
		const vector<HWND>& vecWindowHandles = pMsgHandler->getWindowHandles();
		vector<HWND>::const_iterator iter;
		for (iter = vecWindowHandles.begin(); iter != vecWindowHandles.end(); iter++)
		{
			HWND hWnd = *iter;
			g_mapWindowToMsgHandler[hWnd] = pMsgHandler;
			g_mapWindowToMsgIds[hWnd] = pMsgHandler->getMessagesToHandle();
		}
	}
}
//--------------------------------------------------------------------------------------------------
