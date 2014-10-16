#include "stdafx.h"
#include "WindowPersistenceMgr.h"

#include "UCLIDException.h"

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string WINDOW_POS_X = "WindowPositionX";
const string WINDOW_POS_Y = "WindowPositionY";
const string WINDOW_SIZE_X = "WindowSizeX";
const string WINDOW_SIZE_Y = "WindowSizeY";
const string WINDOW_MAXIMIZED = "WindowMaximized";

//-------------------------------------------------------------------------------------------------
// WindowPersistenceMgr
//-------------------------------------------------------------------------------------------------
WindowPersistenceMgr::WindowPersistenceMgr(CWnd *pWnd, string strRegistryKey)
: m_wnd(*pWnd)
, m_registryManager(HKEY_CURRENT_USER, strRegistryKey)
{
	try
	{
		ASSERT_ARGUMENT("ELI31579", pWnd != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31580");
}
//-------------------------------------------------------------------------------------------------
void WindowPersistenceMgr::SaveWindowPosition()
{
	try
	{
		// Ensure the registry values exist.
		createRegistryValues();

		WINDOWPLACEMENT windowPlacement;
		windowPlacement.length = sizeof( WINDOWPLACEMENT );
		if (m_wnd.GetWindowPlacement(&windowPlacement) != 0)
		{
			CRect rect(windowPlacement.rcNormalPosition);

			// Store whether the window is maximized
			if (windowPlacement.showCmd == SW_SHOWMAXIMIZED)
			{
				m_registryManager.setKeyValue("", WINDOW_MAXIMIZED, "1");
			}
			else
			{
				m_registryManager.setKeyValue("", WINDOW_MAXIMIZED, "0");
			}
			
			// If the window is not maximized or minimized, store the actual position on the screen
			// (Preserves aero-snapped position)
			if (windowPlacement.showCmd != SW_SHOWMAXIMIZED
				&& windowPlacement.showCmd != SW_SHOWMINIMIZED)
			{
				m_wnd.GetWindowRect(rect);
			}

			// Format strings to use as registry values
			CString	zX, zY, zWidth, zHeight;
			zX.Format("%ld", rect.left);
			zY.Format("%ld", rect.top);
			zWidth.Format("%ld", rect.Width());
			zHeight.Format("%ld", rect.Height());

			// Store the position and size info.
			m_registryManager.setKeyValue("", WINDOW_POS_X, (LPCTSTR)zX);
			m_registryManager.setKeyValue("", WINDOW_POS_Y, (LPCTSTR)zY);
			m_registryManager.setKeyValue("", WINDOW_SIZE_X, (LPCTSTR)zWidth);
			m_registryManager.setKeyValue("", WINDOW_SIZE_Y, (LPCTSTR)zHeight);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31581");
}
//-------------------------------------------------------------------------------------------------
void WindowPersistenceMgr::RestoreWindowPosition()
{
	try
	{
		if (createRegistryValues())
		{
			// If the registry values hadn't yet existed, initialize them with the current window
			// position.
			SaveWindowPosition();
		}
		else
		{
			// Retrieve the stored window position
			bool bMaximized = (m_registryManager.getKeyValue("", WINDOW_MAXIMIZED, "") == "1");
			long nLeft = asLong(m_registryManager.getKeyValue("", WINDOW_POS_X, ""));
			long nTop = asLong(m_registryManager.getKeyValue("", WINDOW_POS_Y, ""));
			long nWidth = asLong(m_registryManager.getKeyValue("", WINDOW_SIZE_X, ""));
			long nHeight = asLong(m_registryManager.getKeyValue("", WINDOW_SIZE_Y, ""));

			// Initialize a WINDOWPLACEMENT based on the values stored in the registry. 
			WINDOWPLACEMENT windowPlacement;
			windowPlacement.length = sizeof(WINDOWPLACEMENT);
			windowPlacement.showCmd = bMaximized ? SW_SHOWMAXIMIZED : SW_SHOWNORMAL;
			CRect rect(nLeft, nTop, nLeft + nWidth, nTop + nHeight);
			
			// Adjust window position based on retrieved settings
			windowPlacement.rcNormalPosition = rect;
			m_wnd.SetWindowPlacement(&windowPlacement);

			// If not maximized, set actual last, non-minimized position (not normal position)
			if (!bMaximized)
			{
				m_wnd.MoveWindow(rect, TRUE);
				m_wnd.GetWindowPlacement(&windowPlacement);

				// Put top-left corner on screen if it less than zero
				if (windowPlacement.rcNormalPosition.top < 0)
				{
					rect = windowPlacement.rcNormalPosition;
					rect.MoveToY(0);
					windowPlacement.rcNormalPosition = rect;
					m_wnd.SetWindowPlacement(&windowPlacement);
				}
				if (windowPlacement.rcNormalPosition.left < 0)
				{
					rect = windowPlacement.rcNormalPosition;
					rect.MoveToX(0);
					windowPlacement.rcNormalPosition = rect;
					m_wnd.SetWindowPlacement(&windowPlacement);
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31582");
}
//-------------------------------------------------------------------------------------------------
bool WindowPersistenceMgr::createRegistryValues()
{
	bool bCreatedValues = false;

	if (!m_registryManager.keyExists("", WINDOW_POS_X))
	{
		bCreatedValues = true;
		m_registryManager.createKey("", WINDOW_POS_X, "");
	}

	if (!m_registryManager.keyExists("", WINDOW_POS_Y))
	{
		bCreatedValues = true;
		m_registryManager.createKey("", WINDOW_POS_Y, "");
	}

	if (!m_registryManager.keyExists("", WINDOW_SIZE_X))
	{
		bCreatedValues = true;
		m_registryManager.createKey("", WINDOW_SIZE_X, "");
	}

	if (!m_registryManager.keyExists("", WINDOW_SIZE_Y))
	{
		bCreatedValues = true;
		m_registryManager.createKey("", WINDOW_SIZE_Y, "");
	}

	if (!m_registryManager.keyExists("", WINDOW_MAXIMIZED))
	{
		bCreatedValues = true;
		m_registryManager.createKey("", WINDOW_MAXIMIZED, "");
	}
	return bCreatedValues;
}
//-------------------------------------------------------------------------------------------------
void WindowPersistenceMgr::moveAnchoredAll(CWnd &pCtr, int nOldWidth, int nOldHeight, BOOL bRepaint)
{
	CRect rectWnd, rectControl;
	m_wnd.GetClientRect(rectWnd);
	pCtr.GetWindowRect(rectControl);
	m_wnd.ScreenToClient(rectControl);
	rectControl.right = rectControl.right + rectWnd.Width() - nOldWidth;
	rectControl.bottom = rectControl.bottom + rectWnd.Height() - nOldHeight;
	pCtr.MoveWindow(rectControl, bRepaint);
}
//-------------------------------------------------------------------------------------------------
void WindowPersistenceMgr::moveAnchoredTopRight(CWnd &pCtr, int nOldWidth, int nOldHeight, BOOL bRepaint)
{
	CRect rectWnd, rectControl;
	m_wnd.GetClientRect(rectWnd);
	pCtr.GetWindowRect(rectControl);
	m_wnd.ScreenToClient(rectControl);
	rectControl.MoveToX(rectControl.left + rectWnd.Width() - nOldWidth);
	pCtr.MoveWindow(rectControl, bRepaint);
}
//-------------------------------------------------------------------------------------------------
void WindowPersistenceMgr::moveAnchoredBottomLeft(CWnd &pCtr, int nOldWidth, int nOldHeight, BOOL bRepaint)
{
	CRect rectWnd, rectControl;
	m_wnd.GetClientRect(rectWnd);
	pCtr.GetWindowRect(rectControl);
	m_wnd.ScreenToClient(rectControl);
	rectControl.MoveToY(rectControl.top + rectWnd.Height() - nOldHeight);
	pCtr.MoveWindow(rectControl, bRepaint);
}
//-------------------------------------------------------------------------------------------------
void WindowPersistenceMgr::moveAnchoredBottomLeftRight(CWnd &pCtr, int nOldWidth, int nOldHeight, BOOL bRepaint)
{
	CRect rectWnd, rectControl;
	m_wnd.GetClientRect(rectWnd);
	pCtr.GetWindowRect(rectControl);
	m_wnd.ScreenToClient(rectControl);
	rectControl.right = rectControl.right + rectWnd.Width() - nOldWidth;
	rectControl.MoveToY(rectControl.top + rectWnd.Height() - nOldHeight);
	pCtr.MoveWindow(rectControl, bRepaint);
}
//-------------------------------------------------------------------------------------------------
