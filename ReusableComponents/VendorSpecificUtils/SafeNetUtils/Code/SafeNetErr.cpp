
#include "stdafx.h"
#include "SafeNetErr.h"

#include <map>
#include <string>
using namespace std;

#include <UCLIDException.h>

// the following macro is used in loadScansoftRecErrInfo() to add the scansoft 
// error codes to a map so that a stringized version of the error codes can be displayed 
// to the user as part of the debug information.
#define ADD_TO_MAP(mapName, code) mapName[##code] = #code

//--------------------------------------------------------------------------------------------------
void loadSafeNetErrInfo(UCLIDException& ue, SP_STATUS snStatus)
{
	bool sbMapsInitialized = false;
	static map<int, string> mapSafeNetErrToDescription;
	static map<int, string> mapSafeNetErrToSafeNetErString;

	if (!sbMapsInitialized)
	{
		// initialize the map from RecErr codes to descriptions
		mapSafeNetErrToDescription[SP_ERR_SUCCESS] = "The function completed successfully.";
		mapSafeNetErrToDescription[SP_ERR_INVALID_FUNCTION_CODE] = "Invalid function code.";
		mapSafeNetErrToDescription[SP_ERR_INVALID_PACKET] = "A checksum error was detected in the command packet.";
		mapSafeNetErrToDescription[SP_ERR_UNIT_NOT_FOUND] = "Unable to find the desired hardware key.";
		mapSafeNetErrToDescription[SP_ERR_ACCESS_DENIED] = "Attempted to perform an illegal action on a cell.";
		mapSafeNetErrToDescription[SP_ERR_INVALID_MEMORY_ADDRESS] = "Invalid memory address.";
		mapSafeNetErrToDescription[SP_ERR_INVALID_ACCESS_CODE] = "Invalid access code.";
		mapSafeNetErrToDescription[SP_ERR_PORT_IS_BUSY] = "Port is busy.";
		mapSafeNetErrToDescription[SP_ERR_WRITE_NOT_READY] = "The write or decrement operation could not be performed due to lack of sufficient power.";
		mapSafeNetErrToDescription[SP_ERR_NO_PORT_FOUND] = "No parallel ports could be found, or there was a problem with the protocol being used on the network.";
		mapSafeNetErrToDescription[SP_ERR_ALREADY_ZERO] = "Tried to decrement a counter that contains the value zero.";
		mapSafeNetErrToDescription[SP_ERR_DRIVER_NOT_INSTALLED] = "The Sentinel Driver was not installed or detected. Communication to the hardware key was not possible. Verify the device driver is correctly installed.";
		mapSafeNetErrToDescription[SP_ERR_IO_COMMUNICATIONS_ERROR] = "The system device driver is having problems communicating. Verify the device driver is correctly installed.";
		mapSafeNetErrToDescription[SP_ERR_PACKET_TOO_SMALL] = "The memory allocated for the API packet is less than the required size.";
		mapSafeNetErrToDescription[SP_ERR_INVALID_PARAMETER] = "Arguments and values passed to the API function are invalid.";
		mapSafeNetErrToDescription[SP_ERR_VERSION_NOT_SUPPORTED] = "The current system device driver is outdated. Update the driver.";
		mapSafeNetErrToDescription[SP_ERR_OS_NOT_SUPPORTED] = "The operating system or environment is not supported by the client library.";
		mapSafeNetErrToDescription[SP_ERR_QUERY_TOO_LONG] = "Query string longer than 56 characters. Send a shorter string.";
		mapSafeNetErrToDescription[SP_ERR_INVALID_COMMAND] = "An invalid command was specified in the API call.";
		mapSafeNetErrToDescription[SP_ERR_DRIVER_IS_BUSY] = "The Sentinel Driver is busy.";
		mapSafeNetErrToDescription[SP_ERR_PORT_ALLOCATION_FAILURE] = "Failure to allocate a parallel port through the operating systems parallel port contention handler.";
		mapSafeNetErrToDescription[SP_ERR_PORT_RELEASE_FAILURE] = "Failure to release a previously allocated parallel port through the operating systems parallel port contention handler.";
		mapSafeNetErrToDescription[SP_ERR_ACQUIRE_PORT_TIMEOUT] = "Failure to access the parallel port within the defined time.";
		mapSafeNetErrToDescription[SP_ERR_SIGNAL_NOT_SUPPORTED] = "The particular system does not support a signal line.";
		mapSafeNetErrToDescription[SP_ERR_INIT_NOT_CALLED] = "The key is not initialized.";
		mapSafeNetErrToDescription[SP_ERR_DRVR_TYPE_NOT_SUPPORTED] = "The type of driver access, either direct I/O or system driver, is not supported for the defined operating system and client library.";
		mapSafeNetErrToDescription[SP_ERR_FAIL_ON_DRIVER_COMM] = "The client library failed to communicate with the Sentinel Driver.";
		mapSafeNetErrToDescription[SP_ERR_SERVER_PROBABLY_NOT_UP] = "The protection server is not responding or the client has timed-out.";
		mapSafeNetErrToDescription[SP_ERR_UNKNOWN_HOST] = "The protection server host is unknown or not on the network, or an invalid host name was specified.";
		mapSafeNetErrToDescription[SP_ERR_SENDTO_FAILED] = "The client was unable to send a message to the protection server.";
		mapSafeNetErrToDescription[SP_ERR_SOCKET_CREATION_FAILED] = "Client was unable to open a network socket.";
		mapSafeNetErrToDescription[SP_ERR_NORESOURCES] = "Could not locate enough licensing requirements. Insufficient resources (such as memory) are available to complete the request.";
		mapSafeNetErrToDescription[SP_ERR_BROADCAST_NOT_SUPPORTED] = "Broadcast is not supported by the network interface on the system.";
		mapSafeNetErrToDescription[SP_ERR_BAD_SERVER_MESSAGE] = "Could not understand the message received from the Sentinel Protection Server.";
		mapSafeNetErrToDescription[SP_ERR_NO_SERVER_RUNNING] = "Cannot communicate to the Sentinel Protection Server.";
		mapSafeNetErrToDescription[SP_ERR_NO_NETWORK] = "Unable to talk to the specified host.";
		mapSafeNetErrToDescription[SP_ERR_NO_SERVER_RESPONSE] = "There is no Sentinel Protection Server running in the subnet, or the desired key is not available.";
		mapSafeNetErrToDescription[SP_ERR_NO_LICENSE_AVAILABLE] = "All licenses are currently in use. The key has no more licenses to issue.";
		mapSafeNetErrToDescription[SP_ERR_INVALID_LICENSE] = "License is no longer valid. It probably expired due to time-out.";
		mapSafeNetErrToDescription[SP_ERR_INVALID_OPERATION] = "Specified operation cannot be performed.";
		mapSafeNetErrToDescription[SP_ERR_BUFFER_TOO_SMALL] = "The size of the buffer is not sufficient to hold the expected data.";
		mapSafeNetErrToDescription[SP_ERR_INTERNAL_ERROR] = "An internal error, such as failure to encrypt or decrypt a message being sent or received, has occurred.";
		mapSafeNetErrToDescription[SP_ERR_PACKET_ALREADY_INITIALIZED] = "The API packet was already initialized.";
		mapSafeNetErrToDescription[SP_ERR_PROTOCOL_NOT_INSTALLED] = "The specified protocol is not installed.";
		mapSafeNetErrToDescription[SP_ERR_NO_LEASE_FEATURE] = "The element does not contain any lease period. Probably, this is not a Full License element.";
		mapSafeNetErrToDescription[SP_ERR_LEASE_EXPIRED] = "The date up to which the trial version of the application was valid (expiration date) has reached before the application was first run.";
		mapSafeNetErrToDescription[SP_ERR_COUNTER_LIMIT_REACHED] = "The value requested for the decrement operation exceeds the current counter value.";
		mapSafeNetErrToDescription[SP_ERR_NO_DIGITAL_SIGNATURE] = "The Sentinel Driver binary is not signed by a valid authority.";
		mapSafeNetErrToDescription[SP_ERR_SYS_FILE_CORRUPTED] = "The digital certificate of the Sentinel Driver is not valid.";
		mapSafeNetErrToDescription[SP_ERR_STRING_BUFFER_TOO_LONG] = "The length of the string must not exceed 50 characters.";
		
		
		// initialize the map from recerr codes to stringized versions of recerr codes
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_SUCCESS);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_INVALID_FUNCTION_CODE);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_INVALID_PACKET);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_UNIT_NOT_FOUND);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_ACCESS_DENIED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_INVALID_MEMORY_ADDRESS);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_INVALID_ACCESS_CODE);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_PORT_IS_BUSY);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_WRITE_NOT_READY);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_NO_PORT_FOUND);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_ALREADY_ZERO);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_DRIVER_NOT_INSTALLED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_IO_COMMUNICATIONS_ERROR);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_PACKET_TOO_SMALL);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_INVALID_PARAMETER);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_VERSION_NOT_SUPPORTED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_OS_NOT_SUPPORTED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_QUERY_TOO_LONG);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_INVALID_COMMAND);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_DRIVER_IS_BUSY);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_PORT_ALLOCATION_FAILURE);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_PORT_RELEASE_FAILURE);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_ACQUIRE_PORT_TIMEOUT);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_SIGNAL_NOT_SUPPORTED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_INIT_NOT_CALLED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_DRVR_TYPE_NOT_SUPPORTED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_FAIL_ON_DRIVER_COMM);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_SERVER_PROBABLY_NOT_UP);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_UNKNOWN_HOST);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_SENDTO_FAILED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_SOCKET_CREATION_FAILED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_NORESOURCES);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_BROADCAST_NOT_SUPPORTED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_BAD_SERVER_MESSAGE);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_NO_SERVER_RUNNING);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_NO_NETWORK);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_NO_SERVER_RESPONSE);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_NO_LICENSE_AVAILABLE);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_INVALID_LICENSE);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_INVALID_OPERATION);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_BUFFER_TOO_SMALL);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_INTERNAL_ERROR);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_PACKET_ALREADY_INITIALIZED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_PROTOCOL_NOT_INSTALLED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_NO_LEASE_FEATURE);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_LEASE_EXPIRED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_COUNTER_LIMIT_REACHED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_NO_DIGITAL_SIGNATURE);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_SYS_FILE_CORRUPTED);
		ADD_TO_MAP(mapSafeNetErrToSafeNetErString, SP_ERR_STRING_BUFFER_TOO_LONG);

		// record that the maps have been initialized
		sbMapsInitialized = true;
	}

	// add the stringized recerr code to the debug info
	map<int, string>::const_iterator iter1 = mapSafeNetErrToSafeNetErString.find(snStatus);
	if (iter1 != mapSafeNetErrToSafeNetErString.end())
		ue.addDebugInfo("SafeNetErr", iter1->second);
	else
		ue.addDebugInfo("SafeNetErr", "Unknown SafeNet Error code.");

	// add the recerr description to the debug info
	map<int, string>::const_iterator iter2 = mapSafeNetErrToDescription.find(snStatus);
	if (iter2 != mapSafeNetErrToDescription.end())
		ue.addDebugInfo("SafeNetErr Desc.", iter2->second);
	else
		ue.addDebugInfo("SafeNetErr Desc.", "Unknown SafeNet Error code.");

	// add the integer recerr code to the debug info
	ue.addDebugInfo("(int) SafeNetErr", snStatus);
}
//--------------------------------------------------------------------------------------------------
