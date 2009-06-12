
#pragma once

#include <string>

// Title of the window that receives the time rollback prevention & licensing related messages
const std::string gstrTRP_WINDOW_TITLE = "Extract Systems TRP Window";

// The following is the name of the EXE which is run by the LicenseMgmt code
// in order to instantiate the TRP and establish a connection.
const std::string gstrTRP_EXE_NAME = "ExtractTRP2.exe";

// NOTE:	All these notification messages take a Unique Process Identifier (UPI)
//			the WPARAM.
//			The BaseUtils.dll/CppUtils.*/getUPI() method is used to get the UPI
//			associated with the current process.

// the base message ID for the various of messages related to the failure
// detection and reporting system
const UINT gBASE_MSG_ID = WM_USER + 4271;

//-----------------------------------------------------------------------------
// MESSAGE: gSTATE_IS_VALID_MSG
// WPARAM:  Not used
// LPARAM:	Not used
// PURPOSE: To allow the license corruption state to be checked via message.
// PROMISE:	Returns guiVALID_STATE_CODE if state is valid, 0 otherwise.
// REQUIRE: 
// NOTES:	If the returned value from this message is gVALID_STATE_CODE, 
//			then it should be assumed that the state is valid.
const UINT gSTATE_IS_VALID_MSG = gBASE_MSG_ID + 1;
const UINT guiVALID_STATE_CODE = 73763;
//-----------------------------------------------------------------------------
// MESSAGE: gGET_AUTHENTICATION_CODE_MSG
// WPARAM:  Not used
// LPARAM:	Not used
// PURPOSE: To allow the calling method to authenticate the ExtractTRP 
//			application by requesting an authentication code.
// PROMISE:	Returns an authentication code
// REQUIRE: 
const UINT gGET_AUTHENTICATION_CODE_MSG = gBASE_MSG_ID + 2;
//-----------------------------------------------------------------------------
