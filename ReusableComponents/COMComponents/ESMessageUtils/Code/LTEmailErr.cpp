
#include "stdafx.h"
#include "LTEmailErr.h"

#include <map>
#include <string>
using namespace std;

#include <UCLIDException.h>

// the following macro is used in loadScansoftRecErrInfo() to add the scansoft 
// error codes to a map so that a stringized version of the error codes can be displayed 
// to the user as part of the debug information.
#define ADD_TO_MAP(mapName, code) mapName[##code] = #code

//--------------------------------------------------------------------------------------------------
void loadLTEmailErrInfo(UCLIDException& ue, short nStatus)
{
	bool sbMapsInitialized = false;
	static map<int, string> mapLTEmailErrToDescription;
	static map<int, string> mapLTEmailErrToLTEmailErrString;

	if (!sbMapsInitialized)
	{
		
		mapLTEmailErrToDescription[EML_VIEWDLG_CLOSED] = "Refer to ShowViewDialog method (ILEADMessage Interface) or ShowViewDialog method (ILMimeEntity Interface).";
		mapLTEmailErrToDescription[EML_VIEWDLG_FORWARD] = "Refer to ShowViewDialog method (ILEADMessage Interface) or ShowViewDialog method (ILMimeEntity Interface).";
		mapLTEmailErrToDescription[EML_VIEWDLG_REPLYALL] = "Refer to ShowViewDialog method (ILEADMessage Interface) or ShowViewDialog method (ILMimeEntity Interface).";
		mapLTEmailErrToDescription[EML_VIEWDLG_REPLY] = "Refer to ShowViewDialog method (ILEADMessage Interface) or ShowViewDialog method (ILMimeEntity Interface).";
		mapLTEmailErrToDescription[EML_COMPDLG_CANCELED] = "Refer to ShowSendDialog method.";
		mapLTEmailErrToDescription[EML_COMPDLG_MESSAGE_UPDATED] = "Refer to ShowSendDialog method.";
		mapLTEmailErrToDescription[EML_COMPDLG_MESSAGE_SENT] = "Refer to ShowSendDialog method.";
		mapLTEmailErrToDescription[EML_SUCCESS] = "The operation completed successfully";
		mapLTEmailErrToDescription[EML_ERROR_ALLOC_MEMORY_FAILED] = "Couldn’t allocate memory.";
		mapLTEmailErrToDescription[EML_ERROR_SOCKET_ERROR] = "A socket error has occurred.";
		mapLTEmailErrToDescription[EML_ERROR_RESOLVE_HOST_FAILED] = "Unable to resolve host.";
		mapLTEmailErrToDescription[EML_ERROR_TIMEOUT] = "The timeout period expired.";
		mapLTEmailErrToDescription[EML_ERROR_FAILED] = "The function failed to continue.";
		mapLTEmailErrToDescription[EML_ERROR_CONNECTION_CLOSED] = "The server closed the connection.";
		mapLTEmailErrToDescription[EML_ERROR_UNEXPECTED_SERVER_RESPONSE] = "Unexpected server response.";
		mapLTEmailErrToDescription[EML_ERROR_AUTHENTICATION_FAILED] = "The server refused the authentication information.";
		mapLTEmailErrToDescription[EML_ERROR_NO_AUTHORS_SPECIFIED] = "No authors were specified for the composed message.";
		mapLTEmailErrToDescription[EML_ERROR_MUST_SPECIFY_SENDER] = "A sender must be specified since the composed message has more than one author.";
		mapLTEmailErrToDescription[EML_ERROR_NO_RECIPIENTS_SPECIFIED] = "No recipients were specified for the composed message.";
		mapLTEmailErrToDescription[EML_ERROR_INVALID_DISPLAY_NAME] = "Invalid display name.";
		mapLTEmailErrToDescription[EML_ERROR_INVALID_EMAIL_ADDRESS] = "Invalid email address.";
		mapLTEmailErrToDescription[EML_ERROR_INVALID_PARAMETER] = "An invalid parameter was passed to the method.";
		mapLTEmailErrToDescription[EML_ERROR_INVALID_HEADER_FIELD_BODY] = "Invalid header field body.";
		mapLTEmailErrToDescription[EML_ERROR_INVALID_HEADER_FIELD_NAME] = "Invalid header field name.";
		mapLTEmailErrToDescription[EML_ERROR_MUST_ENCODE_BODY] = "The textual body of the composed message or the body of the MIME entity needs to be encoded.";
		mapLTEmailErrToDescription[EML_ERROR_INVALID_FORMAT] = "Invalid format.";
		mapLTEmailErrToDescription[EML_ERROR_INVALID_CHARSET] = "Invalid character set.";
		mapLTEmailErrToDescription[EML_ERROR_ADDING_ATTACHMENT_FAILED] = "Failed to add the attachment to the composed message.";
		mapLTEmailErrToDescription[EML_ERROR_OPERATION_CANCELLED] = "The operation was canceled by the user.";
		mapLTEmailErrToDescription[EML_ERROR_SOCK_INIT_FAILED] = "Failed to initialize the use of the Windows Sockets library (Ws2_32.dll).";
		mapLTEmailErrToDescription[EML_ERROR_INVALID_WND_HANDLE] = "The window handle passed is invalid.";
		mapLTEmailErrToDescription[EML_ERROR_CREATE_DLG_FAILED] = "Failed to create the dialog box.";
		
		// initialize the map from recerr codes to stringized versions of error codes
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_VIEWDLG_CLOSED);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_VIEWDLG_FORWARD);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_VIEWDLG_REPLYALL);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_VIEWDLG_REPLY);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_COMPDLG_CANCELED);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_COMPDLG_MESSAGE_UPDATED);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_COMPDLG_MESSAGE_SENT);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_SUCCESS);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_ALLOC_MEMORY_FAILED);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_SOCKET_ERROR);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_RESOLVE_HOST_FAILED);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_TIMEOUT);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_FAILED);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_CONNECTION_CLOSED);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_UNEXPECTED_SERVER_RESPONSE);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_AUTHENTICATION_FAILED);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_NO_AUTHORS_SPECIFIED);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_MUST_SPECIFY_SENDER);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_NO_RECIPIENTS_SPECIFIED);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_INVALID_DISPLAY_NAME);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_INVALID_EMAIL_ADDRESS);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_INVALID_PARAMETER);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_INVALID_HEADER_FIELD_BODY);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_INVALID_HEADER_FIELD_NAME);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_MUST_ENCODE_BODY);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_INVALID_FORMAT);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_INVALID_CHARSET);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_ADDING_ATTACHMENT_FAILED);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_OPERATION_CANCELLED);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_SOCK_INIT_FAILED);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_INVALID_WND_HANDLE);
		ADD_TO_MAP(mapLTEmailErrToLTEmailErrString, EML_ERROR_CREATE_DLG_FAILED);

		
		// record that the maps have been initialized
		sbMapsInitialized = true;
	}

	// add the stringized recerr code to the debug info
	map<int, string>::const_iterator iter1 = mapLTEmailErrToLTEmailErrString.find(nStatus);
	if (iter1 != mapLTEmailErrToLTEmailErrString.end())
		ue.addDebugInfo("LTEmailErr", iter1->second);
	else
		ue.addDebugInfo("LTEmailErr", "Unknown LTEmailErr Error code.");

	// add the recerr description to the debug info
	map<int, string>::const_iterator iter2 = mapLTEmailErrToDescription.find(nStatus);
	if (iter2 != mapLTEmailErrToDescription.end())
		ue.addDebugInfo("LTEmailErr Desc.", iter2->second);
	else
		ue.addDebugInfo("LTEmailErr Desc.", "Unknown LTEmailErr Error code.");

	// add the integer recerr code to the debug info
	ue.addDebugInfo("(int) LTEmailErr", nStatus);
}
//--------------------------------------------------------------------------------------------------
