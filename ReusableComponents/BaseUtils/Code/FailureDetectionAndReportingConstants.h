
#pragma once

// Title of the window that receives the Failure Detection and Reporting 
// System (FDRS) exception logging related messages
const char *gpszFDRS_WINDOW_TITLE = "Extract Systems Failure Detection And Reporting";

// NOTE:	All these notification messages take a Unique Process Identifier (UPI)
//			the WPARAM.
//			The BaseUtils.dll/CppUtils.*/getUPI() method is used to get the UPI
//			associated with the current process.

// the base message ID for the various of messages related to the failure
// detection and reporting system
const UINT gBASE_MSG_ID = WM_USER + 5673;

//-----------------------------------------------------------------------------
// MESSAGE: gNOTIFY_EXCEPTION_LOGGED_MSG
// WPARAM:  A global atom containing the Unique Process Identifier (UPI).
// LPARAM:	A global atom containing the name of a file which contains one line
//			with the stringized exception.  The receiver of this message will
//			delete the file after it has processed this message.
// PURPOSE: To notify that an application logged an exception.
// PROMISE:	The atom represented by LPARAM will be released by the application
//			receiving this message.
// REQUIRE: The atom represented by WPARAM is not released by the application
//			receiving the message.  For efficiency reasons, it is the 
//			responsibility of the client application to release the atom
//			associated with WPARAM.
const UINT gNOTIFY_EXCEPTION_LOGGED_MSG = gBASE_MSG_ID + 1;
//-----------------------------------------------------------------------------
// MESSAGE: gNOTIFY_EXCEPTION_DISPLAYED_MSG
// WPARAM:  A global atom containing the Unique Process Identifier (UPI).
// LPARAM:	A global atom containing the name of a file which contains one line
//			with the stringized exception.  The receiver of this message will
//			delete the file after it has processed this message.
// PURPOSE: To notify that an application displayed an exception to the user
// PROMISE:	The atom represented by LPARAM will be released by the application
//			receiving this message.
// REQUIRE: The atom represented by WPARAM is not released by the application
//			receiving the message.  For efficiency reasons, it is the 
//			responsibility of the client application to release the atom
//			associated with WPARAM.
const UINT gNOTIFY_EXCEPTION_DISPLAYED_MSG = gBASE_MSG_ID + 2;
//-----------------------------------------------------------------------------
// MESSAGE: gNOTIFY_APPLICATION_RUNNING
// WPARAM:  A global atom containing the Unique Process Identifier (UPI).
// LPARAM:  Ignored.
// PURPOSE: To notify that an application is running (or still running)
// REQUIRE: The atom represented by WPARAM is not released by the application
//			receiving the message.  For efficiency reasons, it is the 
//			responsibility of the client application to release the atom
//			associated with WPARAM.
const UINT gNOTIFY_APPLICATION_RUNNING_MSG = gBASE_MSG_ID + 3;
//-----------------------------------------------------------------------------
// MESSAGE: gNOTIFY_APPLICATION_NORMAL_EXIT
// WPARAM:  A global atom containing the Unique Process Identifier (UPI).
// LPARAM:  Ignored.
// PURPOSE: To notify that an application has exited normally.
// REQUIRE: The atom represented by WPARAM is not released by the application
//			receiving the message.  For efficiency reasons, it is the 
//			responsibility of the client application to release the atom
//			associated with WPARAM.
const UINT gNOTIFY_APPLICATION_NORMAL_EXIT_MSG = gBASE_MSG_ID + 4;
//-----------------------------------------------------------------------------
// MESSAGE: gNOTIFY_APPLICATION_ABNORMAL_EXIT
// WPARAM:  A global atom containing the Unique Process Identifier (UPI).
// LPARAM:  Ignored.
// PURPOSE: To notify that an application has exited abnormally.
// REQUIRE: The atom represented by WPARAM is not released by the application
//			receiving the message.  For efficiency reasons, it is the 
//			responsibility of the client application to release the atom
//			associated with WPARAM.
const UINT gNOTIFY_APPLICATION_ABNORMAL_EXIT_MSG = gBASE_MSG_ID + 5;
//-----------------------------------------------------------------------------
// PURPOSE:	This constant represents the frequency at which FDRS-aware 
//			applications ping the FDRS to let it know that the application is
//			still running.
const UINT uiPING_FREQUENCY_IN_SECONDS = 30; // seconds
//-----------------------------------------------------------------------------
