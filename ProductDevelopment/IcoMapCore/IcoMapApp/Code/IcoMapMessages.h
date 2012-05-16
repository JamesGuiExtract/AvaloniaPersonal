
#pragma once

//-------------------------------------------------------------------------------------------------
// PURPOSE: To retrieve the handle of the Window IR Manager
// WPARAM:	Ignored.
// LPARAM:	Ignored.
// RETURNS: Returns the window handle of the Window IR Manager associated with
//			the singleton instance of the InputManager that IcoMap is using
const int WM_GET_WINDOW_IR_MANAGER = WM_USER + 1;
//-------------------------------------------------------------------------------------------------
// PURPOSE: To tell IcoMap to execute a shortcut command
// WPARAM:	Message ID, corresponding to the EShortcutType enumeration.
// LPARAM:	Ignored.
// RETURNS: 0 if successful, -1 otherwise
const int WM_EXECUTE_COMMAND = WM_USER + 2;
//-------------------------------------------------------------------------------------------------
