
#pragma once

#include <string>
#include <memory>

#include "BaseUtils.h"

class EXPORT_BaseUtils CursorToolTipCtrl
{
public:
	//----------------------------------------------------------------------------------------------
	CursorToolTipCtrl(CWnd *pParentWnd);
	~CursorToolTipCtrl();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: update the text associated with the tooltip
	// REQUIRE: strText != __nullptr
	//			If specified, nClientX and nClientY are in client window positions, relative to
	//			the specified parent window.
	// PROMISE:	To display the specified tooltip text at the current cursor position for a duration
	//			of ulTimeDuration milli-seconds.  If ulTimeDuration ==  INFINITE, the text is shown
	//			indefinitely.  If ulTimeDuration == 0, no text is displayed as tooltip, and the
	//			strText argument is ignored.  If (nClientX = -1 or nClientY = -1), the tooltip
	//			is displayed at the current cursor location if the cursor is inside the bounds of
	//			of the specified parent window.
	void updateTipText(const std::string& strText, long nClientX = -1, 
		long nClientY = -1, unsigned long ulTimeDuration = 3000);
	//----------------------------------------------------------------------------------------------
	void hideToolTip();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To relay mousemove messages to the tooltip control
	// REQUIRE: Call this method to update the tooltip position when a mousemove event has
	//			occurred.
	// PROMISE: The tooltip position will be updated to current mouse cursor position.  The current
	//			mouse cursor position is automatically determined using the Win32 API, and 
	//			consequently is not passed as an argument to this function.
	//			If (nClientX = -1 or nClientY = -1), the tooltip is displayed at the current cursor
	//			location if the cursor is inside the bounds of of the specified parent window.
	void updateCursorPos(long nClientX, long nClientY);
	//----------------------------------------------------------------------------------------------

private:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Because this object uses timers and multiple instances of this object may exist,
	//			it is necessary to process each timer message in the context of the CursorTooltipCtrl
	//			associated with the timer.
	// REQUIRE:	Do not call this method - it is for internal use, but is declared public because it
	//			needs to be called from some timer procedures.
	void processTimerMessage();
	//----------------------------------------------------------------------------------------------

	// friend function definitions
	friend VOID CALLBACK TimerProc(HWND hwnd, UINT uMsg, UINT idEvent, DWORD dwm_time);

	// static variables
	static CursorToolTipCtrl *ms_pActiveInstance;

	bool m_bActive;
	unsigned int m_uiTimerID;
	CWnd *m_pParentWnd;
	HWND m_hwndTT; 
	TOOLINFO m_ti;
};
