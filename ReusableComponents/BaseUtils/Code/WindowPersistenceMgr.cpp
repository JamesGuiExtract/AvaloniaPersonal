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
		ASSERT_ARGUMENT("ELI31579", pWnd != NULL);
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
			// Store whether the window is maximized
			if (windowPlacement.showCmd == SW_SHOWMAXIMIZED)
			{
				m_registryManager.setKeyValue("", WINDOW_MAXIMIZED, "1");
			}
			else
			{
				m_registryManager.setKeyValue("", WINDOW_MAXIMIZED, "0");
			}

			CRect rect(windowPlacement.rcNormalPosition);

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
			// If the regsitry values hadn't yet existed, initialize them with the current window
			// position.
			SaveWindowPosition();
		}
		else
		{
			// Retrieve the stored window position
			bool bMaximized = (m_registryManager.getKeyValue("", WINDOW_MAXIMIZED) == "1");
			long nLeft = asLong(m_registryManager.getKeyValue("", WINDOW_POS_X));
			long nTop = asLong(m_registryManager.getKeyValue("", WINDOW_POS_Y));
			long nWidth = asLong(m_registryManager.getKeyValue("", WINDOW_SIZE_X));
			long nHeight = asLong(m_registryManager.getKeyValue("", WINDOW_SIZE_Y));

			// Initialize a WINDOWPLACEMENT based on the values stored in the registry. 
			WINDOWPLACEMENT windowPlacement;
			windowPlacement.length = sizeof(WINDOWPLACEMENT);
			windowPlacement.showCmd = bMaximized ? SW_SHOWMAXIMIZED : SW_SHOWNORMAL;
			windowPlacement.rcNormalPosition = CRect(nLeft, nTop, nLeft + nWidth, nTop + nHeight);

			// Adjust window position based on retrieved settings
			m_wnd.SetWindowPlacement(&windowPlacement);
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

	return bCreatedValues;
}
//-------------------------------------------------------------------------------------------------
