
#pragma once

#include "BaseUtils.h"
#include "Singleton.h"

#include <vector>

//--------------------------------------------------------------------------------------------------
EXPORT_BaseUtils LRESULT CALLBACK BaseUtilsDLL_CallWndProc(int nCode, WPARAM wParam, 
													   LPARAM lParam);
//--------------------------------------------------------------------------------------------------
#define INITHOOK_WH_CALLWNDPROC() \
	{ \
		if (SystemHookMsgManager::sGetInstance().hWH_CALLWNDPROC == NULL) \
		{ \
			SystemHookMsgManager::sGetInstance().hWH_CALLWNDPROC = \
				SetWindowsHookEx(WH_CALLWNDPROC, BaseUtilsDLL_CallWndProc, \
				GetModuleHandle("BaseUtils.Dll"), NULL); \
			if (SystemHookMsgManager::sGetInstance().hWH_CALLWNDPROC == NULL) \
			{ \
				throw UCLIDException("ELI04037", "Unable to setup hook!"); \
			} \
		} \
	}
//--------------------------------------------------------------------------------------------------
#define DELETEHOOK_WH_CALLWNDPROC() \
	{ \
		if (SystemHookMsgManager::sGetInstance().hWH_CALLWNDPROC != __nullptr) \
		{ \
			UnhookWindowsHookEx(SystemHookMsgManager::sGetInstance().hWH_CALLWNDPROC); \
			SystemHookMsgManager::sGetInstance().hWH_CALLWNDPROC = NULL; \
		} \
	}
//--------------------------------------------------------------------------------------------------
class EXPORT_BaseUtils ISystemHookMsgHandler
{
public:
	virtual void onMsg(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam) = 0;
	virtual const std::vector<UINT>& getMessagesToHandle() const = 0;
	virtual const std::vector<HWND>& getWindowHandles() = 0;
	virtual ~ISystemHookMsgHandler() {}
};
//--------------------------------------------------------------------------------------------------
class EXPORT_BaseUtils SystemHookMsgManager : public Singleton<SystemHookMsgManager>
{
public:
	HHOOK hWH_CALLWNDPROC;
	void addMsgHandler(ISystemHookMsgHandler *pMsgHandler);
	void removeMsgHandler(ISystemHookMsgHandler *pMsgHandler);
	
	// re-inquire from each ISystemHookMsgHandler, the list of window handles 
	// & window messages to listen
	void refresh();

protected:
	// protected ctor to prevent construction other than as singleton
	SystemHookMsgManager();
	SystemHookMsgManager(const SystemHookMsgManager& toCopy);
	SystemHookMsgManager& operator = (const SystemHookMsgManager& toAssign);

	ALLOW_SINGLETON_ACCESS(SystemHookMsgManager);

private:
	std::vector<ISystemHookMsgHandler *> m_vecISystemHookMsgHandlers;
};
//--------------------------------------------------------------------------------------------------
