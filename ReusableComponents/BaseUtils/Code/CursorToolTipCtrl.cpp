#include "stdafx.h"
#include "CursorToolTipCtrl.h"
#include "UCLIDException.h"

#include <string>
#include <vector>
#include <map>

using namespace std;

// static/global variables
CursorToolTipCtrl* CursorToolTipCtrl::ms_pActiveInstance = NULL;

//--------------------------------------------------------------------------------------------------
VOID CALLBACK TimerProc(
  HWND hwnd,     // handle of window for m_timer messages
  UINT uMsg,     // WM_m_tiMER message
  UINT idEvent,  // m_timer idenm_tifier
  DWORD dwm_time   // current system m_time
)
{
	// Unused parameters
	hwnd;
	uMsg;
	dwm_time;

	try
	{
		if (CursorToolTipCtrl::ms_pActiveInstance && 
			CursorToolTipCtrl::ms_pActiveInstance->m_uiTimerID == idEvent)
		{
			CursorToolTipCtrl::ms_pActiveInstance->hideToolTip();
		}
	}
	catch (...)
	{
	}
}
//--------------------------------------------------------------------------------------------------
CursorToolTipCtrl::CursorToolTipCtrl(CWnd *pParentWnd)
:m_pParentWnd(pParentWnd)
{
	// CREATE A tooltip WINDOW
	m_hwndTT = CreateWindowEx(WS_EX_TOPMOST,
							TOOLTIPS_CLASS,
							NULL,
							TTS_NOPREFIX | TTS_ALWAYSTIP,		
							CW_USEDEFAULT,
							CW_USEDEFAULT,
							CW_USEDEFAULT,
							CW_USEDEFAULT,
							NULL,
							NULL,
							NULL,
							NULL
						   );

	// INITIALIZE MEMBERS OF THE TOOLINFO STRUCTURE
	m_ti.cbSize = sizeof(TOOLINFO);
	m_ti.uFlags = TTF_ABSOLUTE | TTF_TRACK;
	m_ti.hwnd = NULL;
	m_ti.hinst = NULL;
	m_ti.uId = 0;
	m_ti.lpszText = (LPSTR)(LPCSTR) "";		
    
	// tooltip control will cover the whole window
	m_ti.rect.left = 0;
	m_ti.rect.top = 0;
	m_ti.rect.right = 0;
	m_ti.rect.bottom = 0;

	// SEND AN ADDTOOL MESSAGE TO THE tooltip CONTROL WINDOW
	::SendMessage(m_hwndTT, TTM_ADDTOOL, 0, (LPARAM) (LPTOOLINFO) &m_ti);

	m_bActive = false;
	m_uiTimerID = 0;
}
//--------------------------------------------------------------------------------------------------
CursorToolTipCtrl::~CursorToolTipCtrl()
{
	try
	{
		// hide the tooltip if it is shown
		hideToolTip();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16380");
}
//--------------------------------------------------------------------------------------------------
void CursorToolTipCtrl::hideToolTip()
{
	try
	{
		// if a tooltip is currently active and we are waiting on a timer event, kill the
		// timer event
		if (m_uiTimerID)
		{
			KillTimer(NULL, m_uiTimerID);
			ms_pActiveInstance = NULL;
			m_uiTimerID = 0;
		}

		// deactivate the tooltip
		m_bActive = false;
		::SendMessage(m_hwndTT, TTM_TRACKACTIVATE, false, (LPARAM)(LPTOOLINFO) &m_ti);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI03233")
}
//--------------------------------------------------------------------------------------------------
void CursorToolTipCtrl::updateTipText(const string& strText, long nClientX, 
									  long nClientY, unsigned long ulTimeDuration)
{
	try
	{
		// if any tooltip is currently shown, hide it.
		if (ms_pActiveInstance)
			ms_pActiveInstance->hideToolTip();

		if (ulTimeDuration != 0)
		{
			// activate the tooltip
			m_bActive = true;
			ms_pActiveInstance = this;
			
			// update the tooltip text to be the specified value
			m_ti.lpszText = (LPSTR)(LPCSTR) strText.c_str();		
			::SendMessage(m_hwndTT, TTM_UPDATETIPTEXT, 0, (LPARAM)(LPTOOLINFO) &m_ti);

			// if the time duration is not infinite, then set a m_timer so that
			// the tooltip can be deactivated after the specified time.
			if (ulTimeDuration != INFINITE)
			{
				m_uiTimerID = SetTimer(NULL, NULL, ulTimeDuration, TimerProc);
			}
			else
			{
				m_uiTimerID = 0;
			}

			// force an update of the tooltip at the current cursor position
			updateCursorPos(nClientX, nClientY);
		}
		else
		{
			m_bActive = false;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI03232")
}
//--------------------------------------------------------------------------------------------------
void CursorToolTipCtrl::updateCursorPos(long nClientX, long nClientY)
{
	if (!m_bActive)
		return;

	// get the screen coordinates of the parent windwo
	CRect rect;
	m_pParentWnd->GetWindowRect(&rect);
	
	// get the position at which the tooltip should be displayed
	POINT p;	
	if (nClientX == -1 || nClientY == -1)
	{
		// use the mouse cursor position
		GetCursorPos(&p);
	}
	else
	{
		// use the specified position - but the specified position needs
		// to be converted to screen coords.
		p.x = rect.left + nClientX;
		p.y = rect.top + nClientY;
	}

	// force an update of the tooltip at the current cursor position
	// if the current cursor posim_tion is inside the bounds of the window
	// where we want to show the tooltip
	if (rect.PtInRect(p))
	{
		// the tooltip is within the bounds of the window
		// activate and show the tooltip
		::SendMessage(m_hwndTT, TTM_TRACKPOSITION, 0, (LPARAM)(DWORD) MAKELONG(p.x, p.y));
		::SendMessage(m_hwndTT, TTM_TRACKACTIVATE, true, (LPARAM)(LPTOOLINFO) &m_ti);
	}
	else
	{
		// the tooltip is NOT within the bounds of the window
		// deactivate the tooltip
		::SendMessage(m_hwndTT, TTM_TRACKACTIVATE, false, (LPARAM)(LPTOOLINFO) &m_ti);
	}
}
//--------------------------------------------------------------------------------------------------

