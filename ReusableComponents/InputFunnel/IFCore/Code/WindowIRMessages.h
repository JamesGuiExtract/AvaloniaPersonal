
#pragma once


//--------------------------------------------------------------------------------------------------
// Messages sent to the InputManager
//--------------------------------------------------------------------------------------------------
// PURPOSE: To connect a new Window-InputReceiver to the InputManager
// WPARAM:	Handle of InputReceiver window
// LPARAM:	Not used.
// RETURNS:	TRUE if call was successful, FALSE otherwise.
// PROMISE: If Input is current enabled in the InputFunnel, WM_NOTIFY_INPUT_ENABLED
//			will be posted to the IR window represented by wParam.  If input is
//			currently disabled, WM_NOTIFY_INPUT_DISABLED will be sent to the IR
//			window represened by wParam.
const int WM_CONNECT_WINDOW_IR = WM_USER + 1;
//--------------------------------------------------------------------------------------------------
// PURPOSE: To disconnect a connected Window-InputReceiver from the InputManager
// WPARAM:	Handle of InputReceiver window
// LPARAM:	Not used.
// RETURNS:	TRUE if call was successful, FALSE otherwise.
const int WM_DISCONNECT_WINDOW_IR = WM_USER + 2;
//--------------------------------------------------------------------------------------------------
// PURPOSE: To ask the input manager to process input
// WPARAM:	Atom representing the input that must be processed
// LPARAM:	Not used.
// RETURNS:	TRUE if call was successful, FALSE otherwise.
const int WM_PROCESS_INPUT = WM_USER + 3;
//--------------------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------------------
// Messages sent from the InputManager to the Window-InputReceiver
//--------------------------------------------------------------------------------------------------
// PURPOSE: To notify the Window-InputReceiver that input has been enabled
// WPARAM:	A global atom associated with the InputType string
// LPARAM:	A global atom associated with the Prompt string
// REQUIRE: The window being notified is responsible to delete the Global atoms
//			represented by wParam and lParam
const int WM_NOTIFY_INPUT_ENABLED = WM_USER + 3000;
//--------------------------------------------------------------------------------------------------
// PURPOSE: To notify the Window-InputReceiver that input has been disabled
// WPARAM:	Not used.
// LPARAM:	Not used.
// REQUIRE: The window being notified is responsible to delete the Global atoms
//			represented by wParam and lParam
const int WM_NOTIFY_INPUT_DISABLED = WM_USER + 3001;
//--------------------------------------------------------------------------------------------------
