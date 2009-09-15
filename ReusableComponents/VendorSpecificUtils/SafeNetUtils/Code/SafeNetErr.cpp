#include "stdafx.h"
#include "SafeNetErr.h"

#include <UCLIDException.h>

#include <map>
#include <string>

using namespace std;

//--------------------------------------------------------------------------------------------------
void loadSafeNetErrInfo(UCLIDException& ue, SP_STATUS snStatus)
{
	// Add the SafeNet description to the debug info
	ue.addDebugInfo("SafeNetErr Desc.", getSafeNetErrorDescription(snStatus));

	// Add the stringized SafeNet code to the debug info
	ue.addDebugInfo("SafeNetErr", getSafeNetErrorAsString(snStatus));

	// Add the integer SafeNet code to the debug info
	ue.addDebugInfo("(int) SafeNetErr", snStatus);
}
//-------------------------------------------------------------------------------------------------
string getSafeNetErrorDescription(SP_STATUS snStatus)
{
	switch (snStatus)
	{
	case SP_ERR_SUCCESS:
		return "The function completed successfully.";
	case SP_ERR_INVALID_FUNCTION_CODE:
		return "Invalid function code.";
	case SP_ERR_INVALID_PACKET:
		return "A checksum error was detected in the command packet.";
	case SP_ERR_UNIT_NOT_FOUND:
		return "Unable to find the desired hardware key.";
	case SP_ERR_ACCESS_DENIED:
		return "Attempted to perform an illegal action on a cell.";
	case SP_ERR_INVALID_MEMORY_ADDRESS:
		return "Invalid memory address.";
	case SP_ERR_INVALID_ACCESS_CODE:
		return "Invalid access code.";
	case SP_ERR_PORT_IS_BUSY:
		return "Port is busy.";
	case SP_ERR_WRITE_NOT_READY:
		return "The write or decrement operation could not be performed due to lack of sufficient power.";
	case SP_ERR_NO_PORT_FOUND:
		return "No parallel ports could be found, or there was a problem with the protocol being used on the network.";
	case SP_ERR_ALREADY_ZERO:
		return "Tried to decrement a counter that contains the value zero.";
	case SP_ERR_DRIVER_NOT_INSTALLED:
		return "The Sentinel Driver was not installed or detected. Communication to the hardware key was not possible. Verify the device driver is correctly installed.";
	case SP_ERR_IO_COMMUNICATIONS_ERROR:
		return "The system device driver is having problems communicating. Verify the device driver is correctly installed.";
	case SP_ERR_PACKET_TOO_SMALL:
		return "The memory allocated for the API packet is less than the required size.";
	case SP_ERR_INVALID_PARAMETER:
		return "Arguments and values passed to the API function are invalid.";
	case SP_ERR_VERSION_NOT_SUPPORTED:
		return "The current system device driver is outdated. Update the driver.";
	case SP_ERR_OS_NOT_SUPPORTED:
		return "The operating system or environment is not supported by the client library.";
	case SP_ERR_QUERY_TOO_LONG:
		return "Query string longer than 56 characters. Send a shorter string.";
	case SP_ERR_INVALID_COMMAND:
		return "An invalid command was specified in the API call.";
	case SP_ERR_DRIVER_IS_BUSY:
		return "The Sentinel Driver is busy.";
	case SP_ERR_PORT_ALLOCATION_FAILURE:
		return "Failure to allocate a parallel port through the operating system's parallel port contention handler.";
	case SP_ERR_PORT_RELEASE_FAILURE:
		return "Failure to releace a previously allocated parallel port through the operating system's parallel port contention handler.";
	case SP_ERR_ACQUIRE_PORT_TIMEOUT:
		return "Failure to access the parallel port within the defined time.";
	case SP_ERR_SIGNAL_NOT_SUPPORTED:
		return "The particular system does not support a signal line.";
	case SP_ERR_INIT_NOT_CALLED:
		return "The key is not initialized.";
	case SP_ERR_DRVR_TYPE_NOT_SUPPORTED:
		return "The type of driver access, either direct I/O or system driver, is not supported for the defined operating system and client library.";
	case SP_ERR_FAIL_ON_DRIVER_COMM:
		return "The client library failed to communicate with the Sentinel Driver.";
	case SP_ERR_SERVER_PROBABLY_NOT_UP:
		return "The protection server is not responding or the client has timed-out.";
	case SP_ERR_UNKNOWN_HOST:
		return "The protection server host is unknown or not on the network, or an invalid host name was specified.";
	case SP_ERR_SENDTO_FAILED:
		return "The client was unable to send a message to the protection server.";
	case SP_ERR_SOCKET_CREATION_FAILED:
		return "Client was unable to open a network socket.";
	case SP_ERR_NORESOURCES:
		return "Could not locate enough licensing requirements. Insufficient resources (such as memory) are available to complete the request.";
	case SP_ERR_BROADCAST_NOT_SUPPORTED:
		return "Broadcast is not supported by the network interface on the system.";
	case SP_ERR_BAD_SERVER_MESSAGE:
		return "Could not understand the message received from the Sentinel Protection Server.";
	case SP_ERR_NO_SERVER_RUNNING:
		return "Cannot communicate to the Sentinel Protection Server.";
	case SP_ERR_NO_NETWORK:
		return "Unable to talk to the specified host.";
	case SP_ERR_NO_SERVER_RESPONSE:
		return "There is no Sentinel Protection Server running in the subnet, or the desired key is not available.";
	case SP_ERR_NO_LICENSE_AVAILABLE:
		return "All licenses are currently in use. The key has no more licenses to issue.";
	case SP_ERR_INVALID_LICENSE:
		return "License is no longer valid. It probably expired due to time-out.";
	case SP_ERR_INVALID_OPERATION:
		return "Specified operation cannot be performed.";
	case SP_ERR_BUFFER_TOO_SMALL:
		return "The size of the buffer is not sufficient to hold the expected data.";
	case SP_ERR_INTERNAL_ERROR:
		return "An internal error, such as failure to encrypt or decrypt a message being sent or received, has occurred.";
	case SP_ERR_PACKET_ALREADY_INITIALIZED:
		return "The API packet was already initialized.";
	case SP_ERR_PROTOCOL_NOT_INSTALLED:
		return "The specified protocol is not installed.";
	case SP_ERR_NO_LEASE_FEATURE:
		return "The element does not contain any lease period. Probably, this is not a Full License element.";
	case SP_ERR_LEASE_EXPIRED:
		return "The date up to which the trial version of the application was valid (expiration date) has reached before the application was first run.";
	case SP_ERR_COUNTER_LIMIT_REACHED:
		return "The value requested for the decrement operation exceeds the current counter value.";
	case SP_ERR_NO_DIGITAL_SIGNATURE:
		return "The Sentinel Driver binary is not signed by a valid authority.";
	case SP_ERR_SYS_FILE_CORRUPTED:
		return "The digital certificate of the Sentinel Driver is not valid.";
	case SP_ERR_STRING_BUFFER_TOO_LONG:
		return "The length of the string must not exceed 50 characters.";
	default:
		return "Unknown SafeNet Error code.";
	}
}
//-------------------------------------------------------------------------------------------------
string getSafeNetErrorAsString(SP_STATUS snStatus)
{
	switch (snStatus)
	{
	case SP_ERR_SUCCESS:
		return "SP_ERR_SUCCESS";
	case SP_ERR_INVALID_FUNCTION_CODE:
		return "SP_ERR_INVALID_FUNCTION_CODE";
	case SP_ERR_INVALID_PACKET:
		return "SP_ERR_INVALID_PACKET";
	case SP_ERR_UNIT_NOT_FOUND:
		return "SP_ERR_UNIT_NOT_FOUND";
	case SP_ERR_ACCESS_DENIED:
		return "SP_ERR_ACCESS_DENIED";
	case SP_ERR_INVALID_MEMORY_ADDRESS:
		return "SP_ERR_INVALID_MEMORY_ADDRESS";
	case SP_ERR_INVALID_ACCESS_CODE:
		return "SP_ERR_INVALID_ACCESS_CODE";
	case SP_ERR_PORT_IS_BUSY:
		return "SP_ERR_PORT_IS_BUSY";
	case SP_ERR_WRITE_NOT_READY:
		return "SP_ERR_WRITE_NOT_READY";
	case SP_ERR_NO_PORT_FOUND:
		return "SP_ERR_NO_PORT_FOUND";
	case SP_ERR_ALREADY_ZERO:
		return "SP_ERR_ALREADY_ZERO";
	case SP_ERR_DRIVER_NOT_INSTALLED:
		return "SP_ERR_DRIVER_NOT_INSTALLED";
	case SP_ERR_IO_COMMUNICATIONS_ERROR:
		return "SP_ERR_IO_COMMUNICATIONS_ERROR";
	case SP_ERR_PACKET_TOO_SMALL:
		return "SP_ERR_PACKET_TOO_SMALL";
	case SP_ERR_INVALID_PARAMETER:
		return "SP_ERR_INVALID_PARAMETER";
	case SP_ERR_VERSION_NOT_SUPPORTED:
		return "SP_ERR_VERSION_NOT_SUPPORTED";
	case SP_ERR_OS_NOT_SUPPORTED:
		return "SP_ERR_OS_NOT_SUPPORTED";
	case SP_ERR_QUERY_TOO_LONG:
		return "SP_ERR_QUERY_TOO_LONG";
	case SP_ERR_INVALID_COMMAND:
		return "SP_ERR_INVALID_COMMAND";
	case SP_ERR_DRIVER_IS_BUSY:
		return "SP_ERR_DRIVER_IS_BUSY";
	case SP_ERR_PORT_ALLOCATION_FAILURE:
		return "SP_ERR_PORT_ALLOCATION_FAILURE";
	case SP_ERR_PORT_RELEASE_FAILURE:
		return "SP_ERR_PORT_RELEASE_FAILURE";
	case SP_ERR_ACQUIRE_PORT_TIMEOUT:
		return "SP_ERR_ACQUIRE_PORT_TIMEOUT";
	case SP_ERR_SIGNAL_NOT_SUPPORTED:
		return "SP_ERR_SIGNAL_NOT_SUPPORTED";
	case SP_ERR_INIT_NOT_CALLED:
		return "SP_ERR_INIT_NOT_CALLED";
	case SP_ERR_DRVR_TYPE_NOT_SUPPORTED:
		return "SP_ERR_DRVR_TYPE_NOT_SUPPORTED";
	case SP_ERR_FAIL_ON_DRIVER_COMM:
		return "SP_ERR_FAIL_ON_DRIVER_COMM";
	case SP_ERR_SERVER_PROBABLY_NOT_UP:
		return "SP_ERR_SERVER_PROBABLY_NOT_UP";
	case SP_ERR_UNKNOWN_HOST:
		return "SP_ERR_UNKNOWN_HOST";
	case SP_ERR_SENDTO_FAILED:
		return "SP_ERR_SENDTO_FAILED";
	case SP_ERR_SOCKET_CREATION_FAILED:
		return "SP_ERR_SOCKET_CREATION_FAILED";
	case SP_ERR_NORESOURCES:
		return "SP_ERR_NORESOURCES";
	case SP_ERR_BROADCAST_NOT_SUPPORTED:
		return "SP_ERR_BROADCAST_NOT_SUPPORTED";
	case SP_ERR_BAD_SERVER_MESSAGE:
		return "SP_ERR_BAD_SERVER_MESSAGE";
	case SP_ERR_NO_SERVER_RUNNING:
		return "SP_ERR_NO_SERVER_RUNNING";
	case SP_ERR_NO_NETWORK:
		return "SP_ERR_NO_NETWORK";
	case SP_ERR_NO_SERVER_RESPONSE:
		return "SP_ERR_NO_SERVER_RESPONSE";
	case SP_ERR_NO_LICENSE_AVAILABLE:
		return "SP_ERR_NO_LICENSE_AVAILABLE";
	case SP_ERR_INVALID_LICENSE:
		return "SP_ERR_INVALID_LICENSE";
	case SP_ERR_INVALID_OPERATION:
		return "SP_ERR_INVALID_OPERATION";
	case SP_ERR_BUFFER_TOO_SMALL:
		return "SP_ERR_BUFFER_TOO_SMALL";
	case SP_ERR_INTERNAL_ERROR:
		return "SP_ERR_INTERNAL_ERROR";
	case SP_ERR_PACKET_ALREADY_INITIALIZED:
		return "SP_ERR_PACKET_ALREADY_INITIALIZED";
	case SP_ERR_PROTOCOL_NOT_INSTALLED:
		return "SP_ERR_PROTOCOL_NOT_INSTALLED";
	case SP_ERR_NO_LEASE_FEATURE:
		return "SP_ERR_NO_LEASE_FEATURE";
	case SP_ERR_LEASE_EXPIRED:
		return "SP_ERR_LEASE_EXPIRED";
	case SP_ERR_COUNTER_LIMIT_REACHED:
		return "SP_ERR_COUNTER_LIMIT_REACHED";
	case SP_ERR_NO_DIGITAL_SIGNATURE:
		return "SP_ERR_NO_DIGITAL_SIGNATURE";
	case SP_ERR_SYS_FILE_CORRUPTED:
		return "SP_ERR_SYS_FILE_CORRUPTED";
	case SP_ERR_STRING_BUFFER_TOO_LONG:
		return "SP_ERR_STRING_BUFFER_TOO_LONG";
	default:
		return "UNKNOWN";
	}
}
//-------------------------------------------------------------------------------------------------