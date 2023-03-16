//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	UCLIDException.cpp
//
// PURPOSE:	Implementation of the UCLIDException class
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (August 2000 - present)
//
//==================================================================================================
#include "stdafx.h"
#include "UCLIDException.h"
#include "cpputil.h"
#include "ByteStream.h"
#include "ByteStreamManipulator.h"
#include "ErrorInfo.h"
#include "Win32GlobalAtom.h"
#include "FailureDetectionAndReportingMgr.h"
#include "TemporaryFileName.h"
#include "EncryptionEngine.h"
#include "LicenseUtils.h"
#include "MutexUtils.h"
#include "WindowsProcessData.h"

#include <ExceptionLogger.h>

#include <io.h>
#include <algorithm>
#include <comdef.h>
#include <afxmt.h>
#include <string>

extern AFX_EXTENSION_MODULE BaseUtilsDLL;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// A stream with this signature string contains a version value to indicate the
// actual version so this string should not need to be changed for different versions
// this will result in a stringized exception to begin with the string 1F000000.
const string UCLIDException::ms_strByteStreamSignature = "UCLIDException Object Version 2";

// This is the string value that will be at the start of a stringized exception.
const string gstrSTRINGIZED_BYTE_STREAM_OF_SIGNATURE = "1f000000";

// This is the signature string for the original version of the UCLIDException object
const string gstrBYTE_STREAM_SIGNATURE_VERSION1 = "UCLIDException Object";

// This is the signature of a UCLIDException for original version.
const string gstrSTRINGIZED_BYTE_STREAM_OF_SIGNATURE_OLD = "15000000";

const string gstrEMPTY_STRING = "";

// A stringized byte stream of the exception used as the return string from
// asStringizedByteStream if an exception occurred while converting the exception
// into a stringized byte stream.
// ELI29917: Exception caught in UCLIDException::asStringizedByteStream.
const string gstrAS_STRINGIZED_BYTESTREAM_EXCEPTION_STRING = "1f00000055434c4944457863657074696f"
	"6e204f626a6563742056657273696f6e20320300000008000000454c4932393931373b000000457863657074696"
	"f6e2063617567687420696e2055434c4944457863657074696f6e3a3a6173537472696e67697a65644279746553"
	"747265616d2e00000000000000000000000000";

// Size of the signature at the beginning of a stringized exception.
const unsigned long gnSIGNATURE_SIZE = 8;

// Current version number of the UCLIDException class
const unsigned long gnCURRENT_VERSION = 3;

// The name of the exception helper application
const string gstrEXCEPTION_HELPER_EXE = "ExceptionHelper.exe";

// Registry key that contains the remote exception service address
const string gstrREMOTE_EXCEPTION_LOGGER_KEY = "RemoteExceptionServiceAddress";

UCLIDExceptionHandler* UCLIDException::ms_pCurrentExceptionHandler = __nullptr;	

// Stores the name and version of the application throwing the exception
string UCLIDException::ms_strApplication = "";

// Stores the hardware lock serial number
string UCLIDException::ms_strSerial = "";

bool UCLIDException::ms_bRemoteLoggerRead = false;
string UCLIDException::ms_strRemoteExceptionLoggerAddress = "";

// Path in the 'all user\application data' folder to the exception log
// [LRCAU #5028 - 11/18/2008 JDS]
// [LRCAU #6115] - 05/23/2011 JDS] remove the \misc folder
const string gstrDEFAULT_EXCEPTION_LOG_PATH = "\\LogFiles\\ExtractException.Uex";

// Mutex for protecting access to the log file
static unique_ptr<CMutex> upmutexLogFile;
static CMutex smutexCreate;

//-------------------------------------------------------------------------------------------------
// Local helper methods
//-------------------------------------------------------------------------------------------------
CMutex* getLogFileMutex()
{
	// Check if the log file mutex has been created yet
	if (upmutexLogFile.get() == __nullptr)
	{
		// Lock around creating the log file mutex
		CSingleLock lgTemp(&smutexCreate, TRUE);

		// Check again if it has been created
		if (upmutexLogFile.get() == __nullptr)
		{
			// Create the log file mutex
			upmutexLogFile.reset(getGlobalNamedMutex(gstrLOG_FILE_MUTEX));
			ASSERT_RESOURCE_ALLOCATION("ELI29994", upmutexLogFile.get() != __nullptr);
		}
	}

	return upmutexLogFile.get();
}

//-------------------------------------------------------------------------------------------------
// Exported encrypt method for P/Invoke calls from C#
//-------------------------------------------------------------------------------------------------
unsigned char* externManipulator(const char* pszInput, unsigned long* pulLength)
{
	unsigned char* pszOutput = __nullptr;

	INIT_EXCEPTION_AND_TRACING("MLI00417");

	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI21087", pszInput != __nullptr);
			ASSERT_ARGUMENT("ELI21089", pulLength != __nullptr);

			string strInput(pszInput);

			// Load the byte stream from the input string
			ByteStream bsInput(strInput);
			_lastCodePos = "20";

			// Create the 8-byte padded input
			ByteStream bsPaddedInput;
			ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, bsPaddedInput);
			bsm.write(bsInput);
			bsm.flushToByteStream(8);
			_lastCodePos = "30";

			// Create a byte stream to represent the encryption passwords
			ByteStream bsPassword;
			getUEPassword(bsPassword);
			_lastCodePos = "40";

			// Create an encryption engine and use it to encrypt the input data
			MapLabel encryptionEngine;
			ByteStream bsEncryptedBytes;
			encryptionEngine.setMapLabel(bsEncryptedBytes, bsPaddedInput, bsPassword);
			_lastCodePos = "50";

			*pulLength = bsEncryptedBytes.getLength();

			// Allocate a buffer to hold the encrypted data.
			// NOTE: Need to use CoTaskMemAlloc to allocate the memory so that it can be
			// released on the C# side, CANNOT USE NEW
			pszOutput = (unsigned char*) CoTaskMemAlloc(sizeof(char) * *pulLength);
			ASSERT_RESOURCE_ALLOCATION("ELI21614", pszOutput != __nullptr);
			
			// Copy the encrypted bytes into the newly allocated buffer
			memcpy(pszOutput, bsEncryptedBytes.getData(), *pulLength);
			_lastCodePos = "80";

			return pszOutput;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21090");
	}
	catch(UCLIDException& ue)
	{
		if (pulLength != __nullptr)
		{
			*pulLength = 0;
		}

		// Ensure all allocated memory is cleaned up
		if (pszOutput != __nullptr)
		{
			CoTaskMemFree(pszOutput);	
		}

		// Log this exception to the standard exception log
		ue.log("", true, false, true);

		// Now throw the exception
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
// Exported externLogException
//-------------------------------------------------------------------------------------------------
void externLogException(char *pszELICode, char *pszMessage)
{
	UCLIDException ue( pszELICode, pszMessage);
	ue.log("", true, false, true);
}

//-------------------------------------------------------------------------------------------------
// DefaultUCLIDExceptionHandler class
//-------------------------------------------------------------------------------------------------
// define the default UCLID exception handler class, which can be used to display
// the data associated with the UCLIDException object in a simple messagebox, when no
// other sophisticated display mechanisms are available.
class DefaultUCLIDExceptionHandler : public UCLIDExceptionHandler
{
public:
	void handleException(const UCLIDException& uclidException)
	{
		string strMsg;
		uclidException.asString(strMsg);
		AfxMessageBox(strMsg.c_str());
	}
};

//-------------------------------------------------------------------------------------------------
// GuardedExceptionHandler class
//-------------------------------------------------------------------------------------------------
GuardedExceptionHandler::GuardedExceptionHandler(UCLIDExceptionHandler* pNewHandler)
{
	m_pOldHandler = UCLIDException::setExceptionHandler(pNewHandler);
}
//-------------------------------------------------------------------------------------------------
GuardedExceptionHandler::~GuardedExceptionHandler()
{
	try
	{
		UCLIDException::setExceptionHandler(m_pOldHandler);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16573");
}

//-------------------------------------------------------------------------------------------------
// LastCodePosition class
//-------------------------------------------------------------------------------------------------
LastCodePosition::LastCodePosition(const string& strMethodLocationIdentifier)
:m_strMethodLocationIdentifier(strMethodLocationIdentifier)
{
}
//-------------------------------------------------------------------------------------------------
void LastCodePosition::operator=(const string& strLastCodePos)
{
	m_strLastCodePos = strLastCodePos;
}
//-------------------------------------------------------------------------------------------------
string LastCodePosition::get() const
{
	string strCodePosString = m_strMethodLocationIdentifier + string(".") + m_strLastCodePos;
	return strCodePosString;
}
//-------------------------------------------------------------------------------------------------
bool LastCodePosition::isDefined() const
{
	return !m_strLastCodePos.empty();
}
//-------------------------------------------------------------------------------------------------
LastCodePosition::operator const string()
{
	return get();
}

//-------------------------------------------------------------------------------------------------
// UCLIDException class
//-------------------------------------------------------------------------------------------------
UCLIDException::UCLIDException(void)
:	m_strELI(""),
	m_strDescription(""),
	m_apueInnerException(__nullptr),
	m_ProcessData(),
	m_strDatabaseName(""),
	m_strDatabaseServer(""),
	m_lActionID(0),
	m_lFileID(0)
{
	CoCreateGuid(&m_guidExceptionIdentifier);
	m_unixExceptionTime = time(NULL);
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::createFromString(const string& strELI, const string& strData,
									  bool bLogExceptions, bool bAddELICodeToDebugData)
{
	try
	{
		INIT_EXCEPTION_AND_TRACING("MLI00035");
		try
		{
			// clear internal data
			clear();

			// see if the leading characters of the string match the expected
			// signature
			string::size_type nOldSignaturePos = strData.find(gstrSTRINGIZED_BYTE_STREAM_OF_SIGNATURE_OLD);
			string::size_type nSignaturePos = strData.find(gstrSTRINGIZED_BYTE_STREAM_OF_SIGNATURE);
			_lastCodePos = "40";

			// If a valid signature is not found just set the ELI code and the description.
			if (nSignaturePos != 0 && nOldSignaturePos != 0)
			{
				// strdata does not represent stringized bytestream data of a UCLIDexception object
				m_strELI = strELI;
				m_strDescription = strData;
				CoCreateGuid(&m_guidExceptionIdentifier);
				m_unixExceptionTime = time(NULL);
				return;
			}
			_lastCodePos = "50";

			// Load the exception from the string
			loadFromString(strData);

			// add the CatchID debug info
			if (bAddELICodeToDebugData)
			{
				addDebugInfo("CatchID", strELI);
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20286");
	}
	catch(UCLIDException& uex)
	{
		// Only log exceptions if required
		if (bLogExceptions)
		{
			uex.log();
		}
	}
}
//-------------------------------------------------------------------------------------------------
UCLIDException::UCLIDException(const string& strELI, const string& strText)
	: m_strELI(strELI),
	m_strDescription(strText),
	m_apueInnerException(__nullptr),
	m_unixExceptionTime(0),
	m_ProcessData(),
	m_strDatabaseName(""),
	m_strDatabaseServer(""),
	m_lActionID(0),
	m_lFileID(0)
{
	try
	{
		CoCreateGuid(&m_guidExceptionIdentifier);
		m_unixExceptionTime = time(NULL);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20289");
}
//-------------------------------------------------------------------------------------------------
UCLIDException::UCLIDException(const UCLIDException& uclidException)
:	m_strELI(uclidException.m_strELI),
	m_strDescription(uclidException.m_strDescription),
	m_ProcessData(uclidException.m_ProcessData),
	m_strDatabaseName(""),
	m_strDatabaseServer(""),
	m_lActionID(0),
	m_lFileID(0)
{
	try
	{
		// copy all data from the passed in object to this object.
		m_vecResolution = uclidException.m_vecResolution;
		m_vecDebugInfo = uclidException.m_vecDebugInfo;
		m_vecStackTrace = uclidException.m_vecStackTrace;
		m_guidExceptionIdentifier = uclidException.m_guidExceptionIdentifier;
		m_unixExceptionTime = uclidException.m_unixExceptionTime;
		m_lFileID = uclidException.m_lFileID;
		m_lActionID = uclidException.m_lActionID;
		m_strDatabaseServer = uclidException.m_strDatabaseServer;
		m_strDatabaseName = uclidException.m_strDatabaseName;

		// Create the copy of the Inner Exception.
		if (uclidException.m_apueInnerException.get() != __nullptr)
		{
			// Set the inner exception.
			m_apueInnerException.reset(new UCLIDException(*uclidException.getInnerException()));
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20290");
}
//-------------------------------------------------------------------------------------------------
UCLIDException::UCLIDException(const string& strELI, const string& strText, 
							   const UCLIDException& ueInnerException)
:	m_strELI(strELI),
	m_strDescription(strText),
	m_ProcessData(),
	m_strDatabaseName(""),
	m_strDatabaseServer(""),
	m_lActionID(0),
	m_lFileID(0)
{
	try
	{
		CoCreateGuid(&m_guidExceptionIdentifier);
		m_unixExceptionTime = time(NULL);

		// Create a new copy of the inner exception.
		m_apueInnerException.reset(new UCLIDException(ueInnerException));
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21242");
}
//-------------------------------------------------------------------------------------------------
UCLIDException::~UCLIDException()
{
	try
	{
		// Clear all internal data.
		clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21243");
}
//-------------------------------------------------------------------------------------------------
UCLIDException& UCLIDException::operator=(const UCLIDException& uclidException)
{
	try
	{
		// copy all data from the passed in object to this object.
		m_vecResolution = uclidException.m_vecResolution;
		m_vecDebugInfo = uclidException.m_vecDebugInfo;
		m_vecStackTrace = uclidException.m_vecStackTrace;
		m_strELI = uclidException.m_strELI;
		m_strDescription = uclidException.m_strDescription;
		m_ProcessData = uclidException.m_ProcessData;
		m_guidExceptionIdentifier = uclidException.m_guidExceptionIdentifier;
		m_unixExceptionTime = uclidException.m_unixExceptionTime;
		m_lFileID = uclidException.m_lFileID;
		m_lActionID = uclidException.m_lActionID;
		m_strDatabaseServer = uclidException.m_strDatabaseServer;
		m_strDatabaseName = uclidException.m_strDatabaseName;

		// Create the copy of the Inner Exception.
		if (uclidException.m_apueInnerException.get() != __nullptr)
		{
			// Set the inner exception.
			m_apueInnerException.reset(new UCLIDException(*uclidException.getInnerException()));
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20291");

	return *this;
}
//-------------------------------------------------------------------------------------------------
ByteStream UCLIDException::asByteStream() const
{
	INIT_EXCEPTION_AND_TRACING("MLI00036");
	try
	{
		// Set up the bytestream for writing.
		ByteStream byteStream;
		ByteStreamManipulator streamManipulator(ByteStreamManipulator::kWrite, byteStream);
		_lastCodePos = "10";

		// write the signature
		streamManipulator << ms_strByteStreamSignature;
		_lastCodePos = "20";

		// Save the version in the stream
		streamManipulator << gnCURRENT_VERSION;

		// Write the ELI code and description to the stream
		streamManipulator << m_strELI;
		streamManipulator << m_strDescription;
		_lastCodePos = "30";

		// Output boolean value to indicate if there is an inner exception 
		bool bIsInnerException = m_apueInnerException.get() != __nullptr;
		streamManipulator << bIsInnerException;

		// Write the inner exception to the string
		if (bIsInnerException)
		{
			streamManipulator.write(m_apueInnerException->asByteStream());
		}

		_lastCodePos = "40";

		// write the number of resolution strings, followed by each of the resolution
		// strings
		const vector<string>& vecResolutions = getPossibleResolutions();
		
		AddVectorToStream(streamManipulator, vecResolutions);
		_lastCodePos = "50";

		// To allow the extra data that has been added in the UCLIDException and the Extract.ErrorHandling.ExtractException
		// to be transferred when Extract.ExtractException gets involved add the new fields as debug data
		// UCLIDException and Extract.ErrorHandling.ExtractException will remove them when converting the string back to an
		// Exception object

		const vector<NamedValueTypePair>& rootValues = GetVectorOfRootValues();
		_lastCodePos = "60";

		// Get the debugVector
		const vector<NamedValueTypePair>& vecDebugInfo = getDebugVector();
		_lastCodePos = "70";
		
		vector<NamedValueTypePair> vecCombined;
		vecCombined.insert(vecCombined.end(), vecDebugInfo.begin(), vecDebugInfo.end());
		vecCombined.insert(vecCombined.end(), rootValues.begin(), rootValues.end());
		_lastCodePos = "80";

		AddVectorToStream(streamManipulator, vecCombined);

		// Write the number of stack trace records.
		const vector<string>& vecStackTrace = getStackTrace();

		AddVectorToStream(streamManipulator, vecStackTrace);
		_lastCodePos = "90";
		
		// Add process data to the data
		streamManipulator << m_ProcessData.m_PID;
		streamManipulator << m_ProcessData.m_strComputerName;
		streamManipulator << m_ProcessData.m_strProcessName;
		streamManipulator << m_ProcessData.m_strUserName;
		streamManipulator << m_ProcessData.m_strVersion;
		streamManipulator << m_guidExceptionIdentifier;
		streamManipulator << m_unixExceptionTime;

		streamManipulator << m_lFileID;
		streamManipulator << m_lActionID;
		streamManipulator << m_strDatabaseServer;
		streamManipulator << m_strDatabaseName;

		// flush the data to the bytestream
		streamManipulator.flushToByteStream();
		return byteStream;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20292");

	ByteStream bsTemp;
	return bsTemp;
}
//-------------------------------------------------------------------------------------------------
string UCLIDException::asStringizedByteStream() const
{
	try
	{
		ByteStream byteStream = asByteStream();

		// If the returned byte stream had a length of 0, this indicates
		// that an exception occurred in the asByteStream call. Return
		// a static exception string indicating an exception occurred
		// in the asStringizedByteStream call. [LRCAU #5792]
		if (byteStream.getLength() == 0)
		{
			return gstrAS_STRINGIZED_BYTESTREAM_EXCEPTION_STRING;
		}

		return byteStream.asString();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25378");

	// If an exception occurred, return an exception indicating an error
	// occurred in the asStringizedByteStream method. [LRCAU #5792]
	return gstrAS_STRINGIZED_BYTESTREAM_EXCEPTION_STRING;
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::addDebugInfo(const string& strKeyName, const ValueTypePair& keyValue, 
								  const bool bEncryptValue)
{
	try
	{
		// ASSERT arguments
		//	ASSERT_ARGUMENT("ELI00004", strKeyName != "");
		string l_strKeyName = strKeyName.empty() ? "NoKeyName" : strKeyName;

		// Build data item for this debug information
		NamedValueTypePair info;
		info.SetName(l_strKeyName);

		// Special handling of ValueTypePair if value is to be encrypted
		if (bEncryptValue)
		{
			ValueTypePair vtpEncrypted = keyValue;
			getEncryptedValueTypePair( vtpEncrypted );
			info.SetPair( vtpEncrypted );
		}
		else
		{
			// Do not encrypt, just provide the item directly
			info.SetPair(keyValue);
		}

		// Update the collection of Debug info
		m_vecDebugInfo.push_back(info);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20293");
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::addDebugInfo(const LastCodePosition& lastCodePos)
{
	try
	{
		// If a last code position has been defined, then add it to the debug information
		// associated with this exception.
		if (lastCodePos.isDefined())
		{
			// The last code position is not encrypted
			addDebugInfo("LastCodePos", lastCodePos.get(), false);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20294");
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::addDebugInfo(const string& strKeyName, const UCLIDException& ue)
{
	try
	{
		try
		{
			// Add as debug info all history entries from ue
			for (const UCLIDException *pException = &ue; pException != __nullptr; 
				pException = pException->getInnerException())
			{
				string strException = pException->getTopELI() + " " + pException->getTopText();
				addDebugInfo(strKeyName, strException);

				// Copy all the debug info from pException into this exception
				vector<NamedValueTypePair> vecDebugInfo = pException->getDebugVector();
				for each (NamedValueTypePair debugEntry in vecDebugInfo)
				{
					addDebugInfo(debugEntry.GetName(), debugEntry.GetPair());
				}
			}
		}
		catch (...)
		{
			// Log the exception that was added
			UCLIDException ex("ELI26944", "Unable to add exception as debug info.", ue);
			ex.log();

			throw;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI26942");
}
//-------------------------------------------------------------------------------------------------
const vector<NamedValueTypePair>& UCLIDException::getDebugVector() const
{
	// Return a reference to the Debug info vector
	return m_vecDebugInfo;
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::addPossibleResolution(const string& strResolution)
{
	try
	{
		// ASSERT arguments
		ASSERT_ARGUMENT("ELI00003", strResolution != "");

		// add the provided resolution to the list of resolutions already associated 
		// with this exception.
		m_vecResolution.push_back(strResolution);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20297");
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::addWin32ErrorInfo()
{
	try
	{
		addWin32ErrorInfo(GetLastError());
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20298");
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::addWin32ErrorInfo(DWORD dwErrorCode)
{
	try
	{
		// Retrieve error information if the error code is not success
		if (dwErrorCode != ERROR_SUCCESS)
		{
			// Create the human-readable string
			string strError = getWindowsErrorString( dwErrorCode );

			// Add this error string to the debug information
			addDebugInfo( "Win32 Error", strError );
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI24963");
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::addHresult(HRESULT hr)
{
	try
	{
		// format the HRESULT to a hex string
		CString zHresultCode;
		zHresultCode.Format("0x%08x", hr);

		// add the HRESULT hex string and the error message to the debug info
		addDebugInfo("HRESULT", string(zHresultCode));

		// get the associated error message from the _com_error class
		// if no associated message is found then will return a generic message
		// "Unknown error #<hresult>" 
		CString zErrorString = _com_error(hr).ErrorMessage();

		// add the formatted error string to the debug info
		addDebugInfo("Hresult Error String" , string(zErrorString));

		// for common known error strings, get the error label as a string (e.g. E_ACCESSDENIED)
		string strErrorLabel(""); 
		setErrorLabel(hr, strErrorLabel);

		// add the short error string to the debug info
		addDebugInfo("Short Error String", strErrorLabel);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20299");
}
//-------------------------------------------------------------------------------------------------
string UCLIDException::createLogString(const char* pszMachineName, const char* pszUserName,
	long nDateTime, int nPid, const char* pszProductVersion) const
{
	try
	{
		// Retrieve information for output
		string strSerial = getSerialNumber();
		string strApp = pszProductVersion != __nullptr ? pszProductVersion : getApplication();
		string strComputer = pszMachineName != __nullptr ? pszMachineName : getComputerName();
		string strUser = pszUserName != __nullptr ? pszUserName : getCurrentUserName();
		string strPID = nPid > 0 ? ::asString(nPid) : getCurrentProcessID();

		string strTime;
		if (nDateTime > 0)
		{
			strTime = ::asString(nDateTime);
		}
		else
		{
			// Get current time as string
			char pszTime[20] = {0};
			sprintf_s(pszTime, sizeof(pszTime), "%lld", static_cast<long long>(time(NULL)));
			strTime = pszTime;
		}

		/////////////////////////////////////////////
		// Comma-separated single-line output format:
		/////////////////////////////////////////////
		// Serial number of hardware lock
		// Application name
		// Computer name
		// User name
		// Process ID
		// Current time
		// Exception string
		string strResult = strSerial + "," + strApp + "," + strComputer + "," + strUser + 
			"," + strPID + "," + strTime + "," + asStringizedByteStream();

		return strResult;
	}
	catch(...)
	{
		// Do not want to log an exception when creating an exception to log
	}
	
	return gstrEMPTY_STRING;
}
//-------------------------------------------------------------------------------------------------
const string& UCLIDException::getTopText() const
{
	try
	{
		return m_strDescription;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20301");

	return gstrEMPTY_STRING;
}
//-------------------------------------------------------------------------------------------------
const string& UCLIDException::getTopELI() const
{
	try
	{
		return m_strELI;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20302");

	return gstrEMPTY_STRING;
}
//-------------------------------------------------------------------------------------------------
string UCLIDException::getAllELIs() const
{
	INIT_EXCEPTION_AND_TRACING("MLI00037");
	try
	{
		// Add the current exceptions ELI code
		string strResult = m_strELI;

		// sometimes, ELI's are provided as debug information.  Walk through all the
		// debug information records, and search for any ELI codes in the value fields
		vector<NamedValueTypePair>::const_iterator iter2;
		_lastCodePos = "25";
		for (iter2 = m_vecDebugInfo.begin(); iter2 != m_vecDebugInfo.end(); iter2++)
		{
			_lastCodePos = "30";
			const NamedValueTypePair& debugRecord = *iter2;
			const ValueTypePair& debugValue = debugRecord.GetPair();
			_lastCodePos = "40";
			if (debugValue.getType() == ValueTypePair::kString)
			{
				_lastCodePos = "50";
				string strValue = debugValue.getStringValue();
				_lastCodePos = "60";

				// if the string is longer than an ELI code,
				// then let's check if the string is starting with an 
				// ELI code
				if (strValue.length() > 8)
				{
					strValue = strValue.substr(0, 8);
				}
				_lastCodePos = "70";

				// ELI codes must be of the format ELIddddd where is a digit
				if (strValue.length() == 8 && strValue.find("ELI") == 0)
				{
					_lastCodePos = "80";
					int iNumDigitsFound = 0;
					for (int i = 3; i < 8; i++)
					{
						if (isdigit((unsigned char) strValue[i]))
						{
							iNumDigitsFound++;
						}
					}
					_lastCodePos = "90";

					// we have found an ELI code if we found exactly five digits
					// after the string "ELI"
					if (iNumDigitsFound == 5)
					{
						if (strResult != "")
						{
							strResult += ",";
						}

						strResult += strValue;
					}
				}
			}
		}
		_lastCodePos = "100";

		// If there is an inner exception add the ELI Codes for it.
		if (m_apueInnerException.get() != __nullptr)
		{
			_lastCodePos = "10";
			strResult += "," + m_apueInnerException->getAllELIs();
		}
		_lastCodePos = "20";

		return strResult;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20303");

	return gstrEMPTY_STRING;
}
//-------------------------------------------------------------------------------------------------
const vector<string>& UCLIDException::getPossibleResolutions() const
{
	// return a const reference to the developer-added resolutions associated with this
	// exception object.
	return m_vecResolution;
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::renameLogFile(const string& strFileName, bool bUserRenamed,
								   const string& strComment, bool bThrowExceptionOnFailure)
{
	try
	{
		validateFileOrFolderExistence(strFileName, "ELI29950");

		// Compute the date/time prefix string
		SYSTEMTIME	st;
		GetLocalTime( &st );
		CString zDateTimePrefix;
		zDateTimePrefix.Format( "%4d-%02d-%02d %02dh%02dm%02d.%03ds ",
			st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds );

		// compute the full path to the file to which this file needs to be renamed
		string strRenameFileTo = getDirectoryFromFullPath(strFileName);
		strRenameFileTo += "\\";
		strRenameFileTo += (LPCTSTR) zDateTimePrefix;
		strRenameFileTo += getFileNameFromFullPath(strFileName);

		// Mutex around log file access
		CSingleLock lg(getLogFileMutex(), TRUE);

		// perform the rename.  If the rename fails, just ignore it (we don't want to cause more 
		// errors in the logging process)
		try
		{
			try
			{
				moveFile(strFileName.c_str(), strRenameFileTo.c_str(), false, false);

				string strELICode = "ELI14818";
				string strMessage =
					"Application trace: Current log file was time stamped and renamed.";
				if (bUserRenamed)
				{
					strELICode = "ELI29952";
					strMessage = "User renamed log file.";
				}

				// log an entry in the new log file indicating the file has been renamed.
				UCLIDException ue(strELICode, strMessage);
				ue.addDebugInfo("RenamedLogFile", strRenameFileTo);
				if (!strComment.empty())
				{
					ue.addDebugInfo("User Comment", strComment);
				}
				ue.log(strFileName, false);
			}
			// Only need to build an exception and throw it if bThrowExceptionOnFailure == true
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32916")
		}
		catch (UCLIDException &ue)
		{
			if (bThrowExceptionOnFailure)
			{
				UCLIDException uex("ELI29951", "Unable to rename log file.", ue);
				uex.addWin32ErrorInfo();
				uex.addDebugInfo("Log File Name", strFileName);
				uex.addDebugInfo("New Log File Name", strRenameFileTo);
				throw uex;
			}
		}
	}
	catch(...)
	{
		// Check if the exception should be thrown or just eaten
		if (bThrowExceptionOnFailure)
		{
			throw;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::log(const string& strFile, bool bNotifyExceptionEvent, bool bAddDisplayedTag,
	bool bForceLocal, const char* pszMachineName, const char* pszUserName, long nDateTime, int nPid,
	const char* pszProductVersion) const
{
	// Use try/catch block to trap any exception
	try
	{
		try
		{
			// Add the "Displayed" prefix if necessary
			unique_ptr<UCLIDException> upUclid(__nullptr);
			const UCLIDException* pEx = this;
			if (bAddDisplayedTag)
			{
				upUclid.reset(new UCLIDException(*this));
				upUclid->m_strDescription = "Displayed: " + upUclid->m_strDescription;
				pEx = upUclid.get();
			}

			// Only log remotely if no remote is false and a file has not been specified
			if (!bForceLocal && strFile.empty())
			{
				string strRemoteAddress = getRemoteLoggingAddress();
				if (!strRemoteAddress.empty())
				{
					pEx->logExceptionRemotely(strRemoteAddress);
					return;
				}
			}

			// This is the name of the file we are actually going to log this exception to
			string strOutputLogFile = (strFile == "") ? getDefaultLogFileFullPath() : strFile;

			// Retrieve folder
			string strFolder = getDirectoryFromFullPath( strOutputLogFile );

			// Make sure that the directory exists
			createDirectory(strFolder);
			
			pEx->saveTo(strOutputLogFile, true, pszMachineName, pszUserName, nDateTime,
					nPid, pszProductVersion);

			// notify the Failure Detection & Reporting system that
			// an exception was logged
			if (bNotifyExceptionEvent)
			{
				FailureDetectionAndReportingMgr::notifyExceptionLogged(this);
			}
		}
		catch (UCLIDException& ue)
		{
			// If there are any problems logging exceptions, and the caller attempted to log
			// the exception to a non-default location, then try to log the exception to
			// the default location
			if (!strFile.empty())
			{
				// Create an outer exception.
				UCLIDException ueOuter("ELI16096", "Failed to log exception to non-default location!", ue);

				// Add debug info
				ueOuter.addDebugInfo( "Log File", strFile );
				ueOuter.addDebugInfo( "TopELI", getTopELI());
				ueOuter.addDebugInfo( "TopText", getTopText());

				// Attempt logging to the default location, the exception that couldn't be logged 
				// to the custom location
				ueOuter.log("", bNotifyExceptionEvent, false, true);
			}

			// If there was a failure to log to the default location, do nothing.
		}
	}
	catch (...)
	{
		// We need to guarantee that this method does not throw exceptions
	}
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::saveTo(const string& strFile, bool bAppend, const char* pszMachineName,
	const char* pszUserName, long nDateTime, int nPid, const char* pszProductVersion) const
{
	try
	{
		try
		{
			ExceptionLogger elogger(strFile);
			if (ExceptionLogger::UseNetLogging)
			{
				elogger.Log(asStringizedByteStream());
				return;
			}
		}
		catch (...)
		{

		}

		// Get the output string
		// NOTE: we are doing this before we do any file I/O because we want the file I/O
		// to take as little time as possible.  So, any computations that need to be performed
		// should be done before the file I/O is started
         		string strOut = createLogString(pszMachineName, pszUserName, nDateTime, nPid,
			pszProductVersion);

		// Mutex around log file access
		CSingleLock lg(getLogFileMutex(), TRUE);

		// get the size of the current log file
		ULONGLONG ullCurrentFileSize = 0;
		try
		{
			if (isValidFile(strFile))
			{
				// the getSizeOfFile method may throw exceptions (for example if filename is not valid)
				ullCurrentFileSize = getSizeOfFile(strFile);
			}
		}
		catch (...)
		{
			// if there was any problem in retrieving the file size, just ignore it
			// and we'll continue as if the file size was zero
		}

		// if the UEX file is larger than a particular threshold size, then rename the UEX file with
		// a timestamp prefix to keep the log files of managable size
		const unsigned long ulUEX_FILE_SIZE_THRESHOLD = 2000000;
		if (ullCurrentFileSize >= ulUEX_FILE_SIZE_THRESHOLD)
		{
			// Rename the log file
			renameLogFile(strFile);
		}

		// Set the flags for the file open
		UINT uiFlags = CFile::modeWrite | CFile::modeCreate | CFile::osWriteThrough | CFile::shareExclusive;
		if (bAppend)
		{
			uiFlags |= CFile::modeNoTruncate;
		}

		// Open the file and seek to the end
		CStdioFile cOutFile(strFile.c_str(), uiFlags);
		cOutFile.SeekToEnd();

		// Write the line to the file and close the file
		cOutFile.WriteString(strOut.c_str());
		cOutFile.WriteString("\n");
		cOutFile.Close();
	}
	catch(...)
	{
		// Eat exceptions since logging could cause a recursive call. 
	}
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::throwAsSTLString() const
{
	string strTemp;
	asString(strTemp);
	throw strTemp;
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::asString(string& rResult) const
{
	try
	{
		asString(rResult, false);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20305");
}
//-------------------------------------------------------------------------------------------------
UCLIDExceptionHandler* UCLIDException::getDefaultHandler()
{
	try
	{
		static DefaultUCLIDExceptionHandler handler;
		return &handler;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20306");
	
	return __nullptr;
}
//-------------------------------------------------------------------------------------------------
UCLIDExceptionHandler* UCLIDException::setExceptionHandler(UCLIDExceptionHandler* pHandler)
{
	try
	{
		UCLIDExceptionHandler *pOldHandler = ms_pCurrentExceptionHandler;

		if (pHandler != __nullptr)
		{
			ms_pCurrentExceptionHandler = pHandler;
		}
		else
		{
			ms_pCurrentExceptionHandler = getDefaultHandler();
		}

		return pOldHandler;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20307");

	return __nullptr;
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::display(bool bLogException, bool bForceDisplay) const
{
	try
	{
		if (ms_pCurrentExceptionHandler == __nullptr)
		{
			ms_pCurrentExceptionHandler = getDefaultHandler();
		}

		// log the exception
		if (bLogException)
		{
			// Do not send NotifyExceptionEvent
			log("", false, true);
		}

		// notify the Failure Detection And Reporting system
		// that this exception was displayed
		FailureDetectionAndReportingMgr::notifyExceptionDisplayed(this);

		// Check to see if this Exception is from license corruption
		string strAllELICodes = getAllELIs();

		// Search for ELI15373 from VALIDATE_LICENSE within LicenseMgmt.h
		if (strAllELICodes.find( gstrLICENSE_CORRUPTION_ELI.c_str() ) != string::npos)
		{
			static bool ls_bDisplayLicenseCorruptionUE = false;

			// Do not display this License Corrupted exception if we have 
			// already seen one unless specifically requested to do so
			// (P13 #4216)
			if (ls_bDisplayLicenseCorruptionUE && !bForceDisplay)
			{
				return;
			}
			else
			{
				ls_bDisplayLicenseCorruptionUE = true;
			}
		}

		// display the exception using the current handler
		ms_pCurrentExceptionHandler->handleException(*this);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20308");
}
//-------------------------------------------------------------------------------------------------
const UCLIDException *UCLIDException::getInnerException() const
{
	return m_apueInnerException.get();
}
//----------------------------------------------------------------------------------------------
void UCLIDException::addStackTraceEntry(const string& strStackTraceEntry)
{
	// Add the stack trace entry at the end of the stack trace
	m_vecStackTrace.push_back(strStackTraceEntry);
}
//----------------------------------------------------------------------------------------------
const vector<string>& UCLIDException::getStackTrace() const
{
	return m_vecStackTrace;
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::setApplication(string strName)
{
	try
	{
		// Remove any commas and pipes from Name string because they are reserved
		// chars used for delimiting
		replaceVariable( strName, ",", "" );
		replaceVariable( strName, "|", "" );

		// append the delimiter if this is not the first application
		if (!ms_strApplication.empty())
		{
			ms_strApplication += "|";
		}

		// append the application name to the internal string
		// NOTE: we're appending here instead of replacing because there can be
		// multiple products running at the same time within the same process
		// (such as IcoMap and SwipeIt inside ArcMap.exe)
		ms_strApplication += strName;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20309");
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::setSerialNumber(string strSerial)
{
	try
	{
		// Remove any commas and pipes from Serial# string because they are reserved
		// chars used for delimiting
		replaceVariable( strSerial, ",", "" );
		replaceVariable( strSerial, "|", "" );

		// append the delimiter if this is not the first application
		if (!ms_strSerial.empty())
		{
			ms_strSerial += "|";
		}

		// append the serial# to the internal string
		// NOTE: we're appending here instead of replacing because there can be
		// multiple products running at the same time within the same process
		// (such as IcoMap and SwipeIt inside ArcMap.exe)
		ms_strSerial += strSerial;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20310");
}
//-------------------------------------------------------------------------------------------------
string UCLIDException::getApplication(void)
{
	return ms_strApplication;
}
//-------------------------------------------------------------------------------------------------
const string& UCLIDException::getSerialNumber(void)
{
	return ms_strSerial;
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::getEncryptedValueTypePair(ValueTypePair& vtpNew)
{
	try
	{
		// Get unencrypted string for Value
		string strTemp = vtpNew.getValueAsString();

		// Prepare the ByteStreamManipulator for the input
		ByteStream bsInput;
		ByteStreamManipulator bsmInput(ByteStreamManipulator::kWrite, bsInput);
		bsmInput << strTemp;
		bsmInput.flushToByteStream( 8 );

		// Encrypt the ByteStream
		ByteStream encryptedBytes;
		MapLabel encryptionEngine;
		ByteStream passwordBytes;
		getUEPassword( passwordBytes );
		encryptionEngine.setMapLabel( encryptedBytes, bsInput, passwordBytes );

		// Retrieve encrypted value as Hex string
		string strEncryptedHex = encryptedBytes.asString();

		// Prepare final value
		string strFinal = gstrENCRYPTED_PREFIX;
		strFinal += strEncryptedHex.c_str();

		// Store the new value
		vtpNew.setValue( strFinal );
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20311");
}
//-------------------------------------------------------------------------------------------------
int Error(const char *pszInput)
{
	// this method only exists so that the CATCH_ALL_AND_RETURN_AS_COM_ERROR macro
	// can be tested in the function below.

	// The CATCH_ALL_AND_RETURN_AS_COM_ERROR would normally be used in ATL components
	// which derive from a base class that has a method called Error.  Since we are
	// testing the macro from a normal c++ project file, this Error() function has been
	// defined so that the macro expansion can be tested for errors from this file.

	pszInput;	// unused parameter
	return 0;
}
//-------------------------------------------------------------------------------------------------
int macroTest()
{
	INIT_EXCEPTION_AND_TRACING("MLI-----");

	// this method tests to ensure that the macro expansions are valid
	string strTemp = "";
	ASSERT_ARGUMENT("ELI-----", strTemp != "");
	ASSERT_RESOURCE_ALLOCATION("ELI-----", strTemp != "");
	THROW_LOGIC_ERROR_EXCEPTION("ELI-----");
	
	// test the catch & display macros
	try
	{
	}
	CATCH_UCLID_EXCEPTION("ELI-----")
	CATCH_COM_EXCEPTION("ELI-----")
	CATCH_OLE_EXCEPTION("ELI-----")
	CATCH_UNEXPECTED_EXCEPTION("ELI-----")

	// test the "catch & return as COM error" macros
	try
	{
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI-----")
	
	// test the "catch & log exception" macros
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI-----")
	
	// test the exception display macros
	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI-----")

	// test the rethrow exception macros
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI-----")
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::throwAsCOMError()
{
	try
	{
		ErrorInfo *pErrorInfoObj = new ErrorInfo(asStringizedByteStream());
		IErrorInfo *pErrorInfo;
		pErrorInfoObj->QueryInterface(IID_IErrorInfo, (void **) &pErrorInfo);
		_com_raise_error(S_FALSE, pErrorInfo);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20312");
}
//-------------------------------------------------------------------------------------------------
const string& UCLIDException::getDefaultLogFileFullPath()
{
	static string ls_strLogFile = "";

	try
	{
		// initialize the log file name if it has not yet been initialized
		if (ls_strLogFile == "")
		{
			ls_strLogFile = getExtractApplicationDataPath() + gstrDEFAULT_EXCEPTION_LOG_PATH;
		}

		return ls_strLogFile;
	}
	catch (...)
	{
		return gstrEMPTY_STRING;
	}
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::setErrorLabel(HRESULT hr, string& rstrErrorLabel)
{
	try
	{
		// set the ErrorLabel string for common errors otherwise set to "UNKNOWN ERROR CODE"
		switch (hr)
		{
		case E_ABORT:
			rstrErrorLabel = "E_ABORT";
			break;
		case E_ACCESSDENIED:
			rstrErrorLabel = "E_ACCESSDENIED";
			break;
		case E_FAIL:
			rstrErrorLabel = "E_FAIL";
			break;
		case E_HANDLE:
			rstrErrorLabel = "E_HANDLE";
			break;
		case E_INVALIDARG:
			rstrErrorLabel = "E_INVALIDARG";
			break;
		case E_NOINTERFACE:
			rstrErrorLabel = "E_NOINTERFACE";
			break;
		case E_NOTIMPL:
			rstrErrorLabel = "E_NOTIMPL";
			break;
		case E_OUTOFMEMORY:
			rstrErrorLabel = "E_OUTOFMEMORY";
			break;
		case E_POINTER:
			rstrErrorLabel = "E_POINTER";
			break;
		case E_UNEXPECTED:
			rstrErrorLabel = "E_UNEXPECTED";
			break;
		case STG_E_INVALIDFUNCTION:
			rstrErrorLabel = "STG_E_INVALIDFUNCTION";
			break;
		case STG_E_FILENOTFOUND:
			rstrErrorLabel = "STG_E_FILENOTFOUND";
			break;
		case STG_E_PATHNOTFOUND:
			rstrErrorLabel = "STG_E_PATHNOTFOUND";
			break;
		case STG_E_TOOMANYOPENFILES:
			rstrErrorLabel = "STG_E_TOOMANYOPENFILES";
			break;
		case STG_E_ACCESSDENIED:
			rstrErrorLabel = "STG_E_ACCESSDENIED";
			break;
		case STG_E_INVALIDHANDLE:
			rstrErrorLabel = "STG_E_INVALIDHANDLE";
			break;
		case STG_E_INSUFFICIENTMEMORY:
			rstrErrorLabel = "STG_E_INSUFFICIENTMEMORY";
			break;
		case STG_E_INVALIDPOINTER:
			rstrErrorLabel = "STG_E_INVALIDPOINTER";
			break;
		case STG_E_NOMOREFILES:
			rstrErrorLabel = "STG_E_NOMOREFILES";
			break;
		case STG_E_DISKISWRITEPROTECTED:
			rstrErrorLabel = "STG_E_DISKISWRITEPROTECTED";
			break;
		case STG_E_SEEKERROR:
			rstrErrorLabel = "STG_E_SEEKERROR";
			break;
		case STG_E_WRITEFAULT:
			rstrErrorLabel = "STG_E_WRITEFAULT";
			break;
		case STG_E_READFAULT:
			rstrErrorLabel = "STG_E_READFAULT";
			break;
		case STG_E_SHAREVIOLATION:
			rstrErrorLabel = "STG_E_SHAREVIOLATION";
			break;
		case STG_E_LOCKVIOLATION:
			rstrErrorLabel = "STG_E_LOCKVIOLATION";
			break;
		case STG_E_FILEALREADYEXISTS:
			rstrErrorLabel = "STG_E_FILEALREADYEXISTS";
			break;
		case STG_E_INVALIDPARAMETER:
			rstrErrorLabel = "STG_E_INVALIDPARAMETER";
			break;
		case STG_E_MEDIUMFULL:
			rstrErrorLabel = "STG_E_MEDIUMFULL";
			break;
		case STG_E_PROPSETMISMATCHED:
			rstrErrorLabel = "STG_E_PROPSETMISMATCHED";
			break;
		case STG_E_ABNORMALAPIEXIT:
			rstrErrorLabel = "STG_E_ABNORMALAPIEXIT";
			break;
		case STG_E_INVALIDHEADER:
			rstrErrorLabel = "STG_E_INVALIDHEADER";
			break;
		case STG_E_INVALIDNAME:
			rstrErrorLabel = "STG_E_INVALIDNAME";
			break;
		case STG_E_UNKNOWN:
			rstrErrorLabel = "STG_E_UNKNOWN";
			break;
		case STG_E_UNIMPLEMENTEDFUNCTION:
			rstrErrorLabel = "STG_E_UNIMPLEMENTEDFUNCTION";
			break;
		case STG_E_INVALIDFLAG:
			rstrErrorLabel = "STG_E_INVALIDFLAG";
			break;
		case STG_E_INUSE:
			rstrErrorLabel = "STG_E_INUSE";
			break;
		case STG_E_NOTCURRENT:
			rstrErrorLabel = "STG_E_NOTCURRENT";
			break;
		case STG_E_REVERTED:
			rstrErrorLabel = "STG_E_REVERTED";
			break;
		case STG_E_CANTSAVE:
			rstrErrorLabel = "STG_E_CANTSAVE";
			break;
		case STG_E_OLDFORMAT:
			rstrErrorLabel = "STG_E_OLDFORMAT";
			break;
		case STG_E_OLDDLL:
			rstrErrorLabel = "STG_E_OLDDLL";
			break;
		case STG_E_SHAREREQUIRED:
			rstrErrorLabel = "STG_E_SHAREREQUIRED";
			break;
		case STG_E_NOTFILEBASEDSTORAGE:
			rstrErrorLabel = "STG_E_NOTFILEBASEDSTORAGE";
			break;
		case STG_E_EXTANTMARSHALLINGS:
			rstrErrorLabel = "STG_E_EXTANTMARSHALLINGS";
			break;
		case STG_E_DOCFILECORRUPT:
			rstrErrorLabel = "STG_E_DOCFILECORRUPT";
			break;
		case STG_E_BADBASEADDRESS:
			rstrErrorLabel = "STG_E_BADBASEADDRESS";
			break;
		case STG_E_DOCFILETOOLARGE:
			rstrErrorLabel = "STG_E_DOCFILETOOLARGE";
			break;
		case STG_E_NOTSIMPLEFORMAT:
			rstrErrorLabel = "STG_E_NOTSIMPLEFORMAT";
			break;
		case STG_E_INCOMPLETE:
			rstrErrorLabel = "STG_E_INCOMPLETE";
			break;
		case STG_E_TERMINATED:
			rstrErrorLabel = "STG_E_TERMINATED";
			break;
		case STG_S_CONVERTED:
			rstrErrorLabel = "STG_S_CONVERTED";
			break;
		case STG_S_BLOCK:
			rstrErrorLabel = "STG_S_BLOCK";
			break;
		case STG_S_RETRYNOW:
			rstrErrorLabel = "STG_S_RETRYNOW";
			break;
		case STG_S_MONITORING:
			rstrErrorLabel = "STG_S_MONITORING";
			break;
		case STG_S_MULTIPLEOPENS:
			rstrErrorLabel = "STG_S_MULTIPLEOPENS";
			break;
		case STG_S_CONSOLIDATIONFAILED:
			rstrErrorLabel = "STG_S_CONSOLIDATIONFAILED";
			break;
		case STG_S_CANNOTCONSOLIDATE:
			rstrErrorLabel = "STG_S_CANNOTCONSOLIDATE";
			break;
		case STG_E_STATUS_COPY_PROTECTION_FAILURE:
			rstrErrorLabel = "STG_E_STATUS_COPY_PROTECTION_FAILURE";
			break;
		case STG_E_CSS_AUTHENTICATION_FAILURE:
			rstrErrorLabel = "STG_E_CSS_AUTHENTICATION_FAILURE";
			break;
		case STG_E_CSS_KEY_NOT_PRESENT:
			rstrErrorLabel = "STG_E_CSS_KEY_NOT_PRESENT";
			break;
		case STG_E_CSS_KEY_NOT_ESTABLISHED:
			rstrErrorLabel = "STG_E_CSS_KEY_NOT_ESTABLISHED";
			break;
		case STG_E_CSS_SCRAMBLED_SECTOR:
			rstrErrorLabel = "STG_E_CSS_SCRAMBLED_SECTOR";
			break;
		case STG_E_CSS_REGION_MISMATCH:
			rstrErrorLabel = "STG_E_CSS_REGION_MISMATCH";
			break;
		case STG_E_RESETS_EXHAUSTED:
			rstrErrorLabel = "STG_E_RESETS_EXHAUSTED";
			break;
		case CO_E_FAILEDTOIMPERSONATE:
			rstrErrorLabel = "CO_E_FAILEDTOIMPERSONATE";
			break;
		case CO_E_FAILEDTOGETSECCTX:
			rstrErrorLabel = "CO_E_FAILEDTOGETSECCTX";
			break;
		case CO_E_FAILEDTOOPENTHREADTOKEN:
			rstrErrorLabel = "CO_E_FAILEDTOOPENTHREADTOKEN";
			break;
		case CO_E_FAILEDTOGETTOKENINFO:
			rstrErrorLabel = "CO_E_FAILEDTOGETTOKENINFO";
			break;
		case CO_E_TRUSTEEDOESNTMATCHCLIENT:
			rstrErrorLabel = "CO_E_TRUSTEEDOESNTMATCHCLIENT";
			break;
		case CO_E_FAILEDTOQUERYCLIENTBLANKET:
			rstrErrorLabel = "CO_E_FAILEDTOQUERYCLIENTBLANKET";
			break;
		case CO_E_FAILEDTOSETDACL:
			rstrErrorLabel = "CO_E_FAILEDTOSETDACL";
			break;
		case CO_E_ACCESSCHECKFAILED:
			rstrErrorLabel = "CO_E_ACCESSCHECKFAILED";
			break;
		case CO_E_NETACCESSAPIFAILED:
			rstrErrorLabel = "CO_E_NETACCESSAPIFAILED";
			break;
		case CO_E_WRONGTRUSTEENAMESYNTAX:
			rstrErrorLabel = "CO_E_WRONGTRUSTEENAMESYNTAX";
			break;
		case CO_E_INVALIDSID:
			rstrErrorLabel = "CO_E_INVALIDSID";
			break;
		case CO_E_CONVERSIONFAILED:
			rstrErrorLabel = "CO_E_CONVERSIONFAILED";
			break;
		case CO_E_NOMATCHINGSIDFOUND:
			rstrErrorLabel = "CO_E_NOMATCHINGSIDFOUND";
			break;
		case CO_E_LOOKUPACCSIDFAILED:
			rstrErrorLabel = "CO_E_LOOKUPACCSIDFAILED";
			break;
		case CO_E_NOMATCHINGNAMEFOUND:
			rstrErrorLabel = "CO_E_NOMATCHINGNAMEFOUND";
			break;
		case CO_E_LOOKUPACCNAMEFAILED:
			rstrErrorLabel = "CO_E_LOOKUPACCNAMEFAILED";
			break;
		case CO_E_SETSERLHNDLFAILED:
			rstrErrorLabel = "CO_E_SETSERLHNDLFAILED";
			break;
		case CO_E_FAILEDTOGETWINDIR:
			rstrErrorLabel = "CO_E_FAILEDTOGETWINDIR";
			break;
		case CO_E_PATHTOOLONG:
			rstrErrorLabel = "CO_E_PATHTOOLONG";
			break;
		case CO_E_FAILEDTOGENUUID:
			rstrErrorLabel = "CO_E_FAILEDTOGENUUID";
			break;
		case CO_E_FAILEDTOCREATEFILE:
			rstrErrorLabel = "CO_E_FAILEDTOCREATEFILE";
			break;
		case CO_E_FAILEDTOCLOSEHANDLE:
			rstrErrorLabel = "CO_E_FAILEDTOCLOSEHANDLE";
			break;
		case CO_E_EXCEEDSYSACLLIMIT:
			rstrErrorLabel = "CO_E_EXCEEDSYSACLLIMIT";
			break;
		case CO_E_ACESINWRONGORDER:
			rstrErrorLabel = "CO_E_ACESINWRONGORDER";
			break;
		case CO_E_INCOMPATIBLESTREAMVERSION:
			rstrErrorLabel = "CO_E_INCOMPATIBLESTREAMVERSION";
			break;
		case CO_E_FAILEDTOOPENPROCESSTOKEN:
			rstrErrorLabel = "CO_E_FAILEDTOOPENPROCESSTOKEN";
			break;
		case CO_E_DECODEFAILED:
			rstrErrorLabel = "CO_E_DECODEFAILED";
			break;
		case CO_E_ACNOTINITIALIZED:
			rstrErrorLabel = "CO_E_ACNOTINITIALIZED";
			break;
		case CO_E_CANCEL_DISABLED:
			rstrErrorLabel = "CO_E_CANCEL_DISABLED";
			break;	

			// unrecognized error code
		default:
			rstrErrorLabel = "UNRECOGNIZED ERROR CODE";
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20288");
}
//-------------------------------------------------------------------------------------------------
string UCLIDException::sGetDataValue(const string& strEncryptedValue)
{
	// Make a working copy so if it fails we can return the original
	string strValue = strEncryptedValue;

	// Check Value for Encryption
	if (strValue.find( gstrENCRYPTED_PREFIX.c_str(), 0 ) == 0)
	{
		// Check Internal Tools license
		if (isInternalToolsLicensed())
		{
			// Decrypt the value
			try
			{
				try
				{
					///////////////////////////////////
					// Licensed, provide decrypted text
					///////////////////////////////////
					// Remove encryption indicator prefix
					strValue.erase( 0, gstrENCRYPTED_PREFIX.length() );

					// Create encrypted ByteStream from the hex string
					ByteStream bsInput(strValue);

					// Decrypt the ByteStream
					ByteStream decryptedBytes;
					MapLabel encryptionEngine;
					ByteStream passwordBytes;

					getUEPassword( passwordBytes );

					encryptionEngine.getMapLabel( decryptedBytes, bsInput, passwordBytes );

					// Retrieve the final string
					ByteStreamManipulator bsmFinal(ByteStreamManipulator::kRead, decryptedBytes);
					bsmFinal >> strValue;
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21609")
			}
			catch(UCLIDException ue)
			{
				UCLIDException uex("ELI21613", "Unable to decrypt value", ue);
				uex.log();
				strValue = strEncryptedValue;
			}
		}
		else
		{
			// Not licensed, replace text with Encrypted indicator
			strValue = gstrENCRYPTED_INDICATOR;
		}
	}
	return strValue;
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::loadFromStream(ByteStream& rByteStream)
{
	INIT_EXCEPTION_AND_TRACING("MLI00516");
	try
	{
		// Clear the internal data.
		clear();

		ByteStreamManipulator streamManipulator(ByteStreamManipulator::kRead, rByteStream);
		_lastCodePos = "60";
		
		// read the bytestream signature and verify its correctness
		string strTemp;
		streamManipulator >> strTemp;
		_lastCodePos = "70";

		// Make default version number 1
		unsigned long nVersionNumber = 1;

		// If the current signature
		if (strTemp == ms_strByteStreamSignature)
		{
			// Read the version from the stream
			streamManipulator >> nVersionNumber;
			if (nVersionNumber > gnCURRENT_VERSION)
			{
				UCLIDException ue("ELI25481", "Cannot load newer version of exception!");
				ue.addDebugInfo("Current Version", gnCURRENT_VERSION);
				ue.addDebugInfo("Version", nVersionNumber);
				throw ue;
			}

			// Read the ELI code and description from the stream
			streamManipulator >> m_strELI;
			streamManipulator >> m_strDescription;
			_lastCodePos = "30";

			// Read boolean value to indicate if there is an inner exception from the stream
			bool bIsInnerException;
			streamManipulator >> bIsInnerException;

			// There is an inner exception so load it.
			if (bIsInnerException)
			{
				m_apueInnerException.reset(new UCLIDException());
				ASSERT_RESOURCE_ALLOCATION("ELI21244", m_apueInnerException.get() != __nullptr);

				// Version 2 inner exceptions where streamed in stringized byte form
				if (nVersionNumber == 2)
				{
					streamManipulator >> strTemp;

					// Load the inner exception from the string
					m_apueInnerException->loadFromString(strTemp);
				}
				else
				{
					// Read the bytestream version of the inner exception
					ByteStream bsTemp;
					streamManipulator.read(bsTemp);

					// Load the inner exception from the stream
					m_apueInnerException->loadFromStream(bsTemp);
				}
			}
		}
		// Older version had HistoryRecords that were just ELI code and Description
		else if (strTemp == gstrBYTE_STREAM_SIGNATURE_VERSION1)
		{
			// read the number of history records, followed by each of the history records
			unsigned long ulTemp;
			streamManipulator >> ulTemp;

			_lastCodePos = "90";

			// Set the InnerMost pointer to this
			UCLIDException *ueInnerMost = this;
			for (unsigned long n = 0; n < ulTemp; n++)
			{
				_lastCodePos = "100-" + ::asString(n);
				string strELI, strText;
				streamManipulator >> strELI;
				streamManipulator >> strText;
				
				// If n==0 the ELI code and descrption should be 
				// associated with the current exception.
				if (n == 0)
				{
					m_strELI = strELI;
					m_strDescription = strText;
				}
				else
				{
					// Add a new inner exception making the old inner the inner exception
					// of the new inner exception
					ueInnerMost->m_apueInnerException.reset(new UCLIDException(strELI, strText));
					ueInnerMost = ueInnerMost->m_apueInnerException.get();
					ASSERT_RESOURCE_ALLOCATION("ELI21245", ueInnerMost != __nullptr);
					_lastCodePos = "110";
				}
			}
		}
		else
		{
			throw UCLIDException("ELI01687", "Invalid bytestream signature!");
		}
		_lastCodePos = "120";

		// read the number of resolution strings, followed by each of the resolution
		// strings
		GetVectorFromStream<string>(streamManipulator, m_vecResolution);

		// read the number of debug information records followed by each of the debug information
		// records.
		GetVectorFromStream<NamedValueTypePair>(streamManipulator, m_vecDebugInfo,
			[&](NamedValueTypePair namedPair) -> bool
			{
				return SetRootValues(namedPair);
			});

		if (nVersionNumber >= 2)
		{
			// Get the stack trace information
			GetVectorFromStream(streamManipulator, m_vecStackTrace);

			if (!streamManipulator.IsEndOfStream())
			{
				streamManipulator >> m_ProcessData.m_PID;
				streamManipulator >> m_ProcessData.m_strComputerName;
				streamManipulator >> m_ProcessData.m_strProcessName;
				streamManipulator >> m_ProcessData.m_strUserName;
				streamManipulator >> m_ProcessData.m_strVersion;
				streamManipulator >> m_guidExceptionIdentifier;
				streamManipulator >> m_unixExceptionTime;
			}
			if (!streamManipulator.IsEndOfStream())
			{
				streamManipulator >> m_lFileID;
				streamManipulator >> m_lActionID;
				streamManipulator >> m_strDatabaseServer;
				streamManipulator >> m_strDatabaseName;
			}
		}
		
		_lastCodePos = "280";
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21246");
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::loadFromString(const string& strExceptionAsString)
{
	try
	{
		// Create the byte stream
		ByteStream byteStream(strExceptionAsString);

		// Load the exception from the bytestream
		loadFromStream(byteStream);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25440");
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::clear()
{
	// Clear the resolution vector
	m_vecResolution.clear();

	// Clear the DebugInfo vector
	m_vecDebugInfo.clear();

	// Clear the StackTrace vector
	m_vecStackTrace.clear();

	// Clear the ELI code and description
	m_strELI = "";
	m_strDescription = "";

	// Clear the Inner Exceptions.
	m_apueInnerException.reset();
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::asString(string& rResult, bool bRecursiveCall) const
{
	if (!bRecursiveCall)
	{
		// Clear the result
		rResult = "";
	}

	// Add ELI Code and Description
	rResult += m_strELI + string(": ");
	rResult += m_strDescription + string("\n");

	// populate the resolutions
	if (!m_vecResolution.empty())
	{
		rResult += "\nResolutions:\n";
		vector<string>::const_iterator iter2;
		for (iter2 = m_vecResolution.begin(); iter2 != m_vecResolution.end(); iter2++)
		{
			rResult +=	iter2->c_str() + string("\n");
		}
	}

	// TODO: populate the debug information
	if (!m_vecDebugInfo.empty())
	{
		rResult += "\nDebug information:\n";
		vector<NamedValueTypePair>::const_iterator iter2;
		for (iter2 = m_vecDebugInfo.begin(); iter2 != m_vecDebugInfo.end(); iter2++)
		{
			rResult +=	iter2->GetName() + string(" = ");
			string strValue = "";
			string strType = "";
			switch (iter2->GetPair().getType())
			{
			case ValueTypePair::kString:
				strValue = iter2->GetPair().getStringValue();
				strType = " (string)";
				break;
			case ValueTypePair::kOctets:
				// TODO: get bytes as a string
				strValue = "...";
				strType = " (octets)";
				break;
			case ValueTypePair::kInt:
				strValue = ::asString(iter2->GetPair().getIntValue());
				strType = " (int)";
				break;
			case ValueTypePair::kInt64:
				strValue = ::asString(iter2->GetPair().getInt64Value());
				strType = " (int64)";
			case ValueTypePair::kLong:
				strValue = ::asString(iter2->GetPair().getLongValue());
				strType = " (long)";
				break;
			case ValueTypePair::kUnsignedLong:
				strValue = ::asString(iter2->GetPair().getUnsignedLongValue());
				strType = " (unsigned long)";
				break;
			case ValueTypePair::kDouble:
				strValue = ::asString(iter2->GetPair().getDoubleValue());
				strType = " (double)";
				break;
			case ValueTypePair::kBoolean:
				strValue = iter2->GetPair().getBooleanValue() ? "True" : "False";
				strType = " (bool)";
				break;
			case ValueTypePair::kNone:
				strValue = "Unknown value (Unknown type)";
				break;
			case ValueTypePair::kGuid:
				strValue = ::asString(iter2->GetPair().getGuidValue());
				strType = "(GUID)";
				break;
			default:
				strValue = "Unknown value (Unknown type)";
			}

			// Check Value for Encryption
			rResult += UCLIDException::sGetDataValue(strValue);
			rResult += strType;

			rResult += string("\n");
		}
	}

	// Add the Stack Trace info
	vector<string>::const_iterator iterStackTrace;
	for (iterStackTrace = m_vecStackTrace.begin(); iterStackTrace != m_vecStackTrace.end();
		iterStackTrace++)
	{
		rResult +=	iterStackTrace->c_str() + string("\n");
	}

	// Call recursively if there is an inner exception
	const UCLIDException *pueInner = getInnerException();
	if (pueInner != __nullptr)
	{
		pueInner->asString(rResult, true);
	}

	// if application/version # info is available write that out as well
	if (!bRecursiveCall && !ms_strApplication.empty())
	{
		rResult += "\n\n[";
		rResult += ms_strApplication;
		rResult += "]";
	}
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::logExceptionRemotely(const string& strRemoteAddress) const
{
	try
	{
		try
		{
			string strHelperApp = getModuleDirectory("baseutils.dll") + "\\" + gstrEXCEPTION_HELPER_EXE;

			// Get a temporary file and write the exception data to it
			TemporaryFileName tempFile(false, "", "", false);
			ofstream fOut(tempFile.getName().c_str());
			if (!fOut.is_open())
			{
				UCLIDException ue("ELI34215", "Output file could not be opened.");
				ue.addDebugInfo("Filename", tempFile.getName());
				ue.addWin32ErrorInfo();
				throw ue;
			}

			fOut << asStringizedByteStream();
			fOut.close();

			string strParams = "\"" + tempFile.getName() + "\" /remote \"" + strRemoteAddress
				+ "\" /pid " + getCurrentProcessID() + " /delete";

			string strApp = getApplication();
			if (!strApp.empty())
			{
				strParams += " /product \"" + strApp + "\"";
			}

			// Run the exception helper app
			runEXE(strHelperApp, strParams);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32599");
	}
	catch(UCLIDException& uex)
	{
		uex.log("", true, false, true);
	}
}
//-------------------------------------------------------------------------------------------------
string UCLIDException::getRemoteLoggingAddress()
{
	// Check if the remote logger data has been read yet
	if (!ms_bRemoteLoggerRead)
	{
		CSingleLock lg(getLogFileMutex(), TRUE);

		// Ensure the data has not been read yet
		if (!ms_bRemoteLoggerRead)
		{
			// Check registry for file access timeout
			RegistryPersistenceMgr machineCfgMgr = RegistryPersistenceMgr( HKEY_LOCAL_MACHINE, "" );

			// Check for existence of file access timeout
			if (machineCfgMgr.keyExists( gstrBASEUTILS_REG_PATH, gstrREMOTE_EXCEPTION_LOGGER_KEY ))
			{
				ms_strRemoteExceptionLoggerAddress = machineCfgMgr.getKeyValue(gstrBASEUTILS_REG_PATH,
					gstrREMOTE_EXCEPTION_LOGGER_KEY, "");
			}

			ms_bRemoteLoggerRead = true;
		}
	}

	return ms_strRemoteExceptionLoggerAddress;
}
//-------------------------------------------------------------------------------------------------
void UCLIDException::addDatabaseRelatedInfo(long fileID, long actionID, string databaseServer, string databaseName)
{
	m_lFileID = fileID;
	m_lActionID = actionID;
	m_strDatabaseServer = databaseServer;
	m_strDatabaseName = databaseName;
}
//-------------------------------------------------------------------------------------------------
bool UCLIDException::SetRootValues(const NamedValueTypePair& namedPair)
{
	string name = namedPair.GetName();
	if (name == "ExceptionData_m_ProcessData.m_PID")
	{
		m_ProcessData.m_PID = namedPair.GetPair().getUnsignedLongValue();
		return true;
	}
	if (name == "ExceptionData_m_ProcessData.m_strComputerName")
	{
		m_ProcessData.m_strComputerName = namedPair.GetPair().getStringValue();
		return true;
	}
	if (name == "ExceptionData_m_ProcessData.m_strProcessName")
	{
		m_ProcessData.m_strProcessName = namedPair.GetPair().getStringValue();
		return true;
	}
	if (name == "ExceptionData_m_ProcessData.m_strUserName")
	{
		m_ProcessData.m_strUserName = namedPair.GetPair().getStringValue();
		return true;
	}
	if (name == "ExceptionData_m_ProcessData.m_strVersion")
	{
		m_ProcessData.m_strVersion = namedPair.GetPair().getStringValue();
		return true;
	}
	if (name == "ExceptionData_m_guidExceptionIdentifier")
	{
		m_guidExceptionIdentifier = namedPair.GetPair().getGuidValue();
		return true;
	}
	if (name == "ExceptionData_m_unixExceptionTime")
	{
		m_unixExceptionTime = namedPair.GetPair().getInt64Value();
		return true;
	}
	if (name == "ExceptionData_m_lActionID")
	{
		m_lActionID = namedPair.GetPair().getLongValue();
		return true;
	}
	if (name == "ExceptionData_m_lFileID")
	{
		m_lFileID = namedPair.GetPair().getLongValue();
		return true;
	}
	if (name == "ExceptionData_m_strDatabaseName")
	{
		m_strDatabaseName = namedPair.GetPair().getStringValue();
		return true;
	}
	if (name == "ExceptionData_m_strDatabaseServer")
	{
		m_strDatabaseServer = namedPair.GetPair().getStringValue();
		return true;
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
vector<NamedValueTypePair> UCLIDException::GetVectorOfRootValues() const
{
	vector<NamedValueTypePair> returnVector;
	NamedValueTypePair namedPair;
	ValueTypePair valuePair;

	returnVector.push_back(
		NamedValueTypePair("ExceptionData_m_ProcessData.m_PID", 
			ValueTypePair(m_ProcessData.m_PID)));

	returnVector.push_back(
		NamedValueTypePair("ExceptionData_m_ProcessData.m_strComputerName",
			ValueTypePair(m_ProcessData.m_strComputerName)));
	
	returnVector.push_back(
		NamedValueTypePair("ExceptionData_m_ProcessData.m_strProcessName",
			ValueTypePair(m_ProcessData.m_strProcessName)));

	returnVector.push_back(
		NamedValueTypePair("ExceptionData_m_ProcessData.m_strUserName",
			ValueTypePair(m_ProcessData.m_strUserName)));

	returnVector.push_back(
		NamedValueTypePair("ExceptionData_m_ProcessData.m_strVersion",
			ValueTypePair(m_ProcessData.m_strVersion)));

	returnVector.push_back(
		NamedValueTypePair("ExceptionData_m_guidExceptionIdentifier",
			ValueTypePair(m_guidExceptionIdentifier)));

	returnVector.push_back(
		NamedValueTypePair("ExceptionData_m_unixExceptionTime",
			ValueTypePair(m_unixExceptionTime)));

	returnVector.push_back(
		NamedValueTypePair("ExceptionData_m_lActionID",
			ValueTypePair(m_lActionID)));

	returnVector.push_back(
		NamedValueTypePair("ExceptionData_m_lFileID",
			ValueTypePair(m_lFileID)));

	returnVector.push_back(
		NamedValueTypePair("ExceptionData_m_strDatabaseName",
			ValueTypePair(m_strDatabaseName)));

	returnVector.push_back(
		NamedValueTypePair("ExceptionData_m_strDatabaseServer",
			ValueTypePair(m_strDatabaseServer)));

	return returnVector;
}
 
