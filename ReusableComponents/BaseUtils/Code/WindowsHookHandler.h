#ifndef WINDOWS_HOOK_HANDLER_HPP
#define WINDOWS_HOOK_HANDLER_HPP

#include "BaseUtils.h"

class WindowsHookHandler
{
public:
	virtual void onMessage(MSG *pMsg) = 0;
};

#endif //  WINDOWS_HOOK_HANDLER_HPP