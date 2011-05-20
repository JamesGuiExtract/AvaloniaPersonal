//==================================================================================================
//
// COPYRIGHT (c) 2000 - 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	UCLIDException.h
//
// PURPOSE:	Definition of the UCLIDException class
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (August 2000 - present)
//
//==================================================================================================

#pragma once

//==================================================================================================
//== I N C L U D E S ===============================================================================
//==================================================================================================
#include <comdef.h>

#include "BaseUtils.h"
#include "cpputil.h"
#include "ValueTypePair.h"
#include "NamedValueTypePair.h"
#include "Win32CriticalSection.h"
#include "ByteStream.h"

#include <string>
#include <vector>
#include <map>

using namespace std;

#pragma warning(push)
#pragma warning(disable: 4251) // Eliminate warning arising from <vector>
//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// String constant for default INI file
const string	gstrLICENSE_CORRUPTION_ELI = "ELI15373";

// Displayed to user if this debug information is sensitive
const string gstrENCRYPTED_INDICATOR = "<Encrypted>";

// Mutex name string to be used when accessing the log file
const string gstrLOG_FILE_MUTEX = "Global\\0A7EF4EA-E618-4A07-9D77-7F4E48D6B224";

// PURPOSE: To provide an external function that can be called from C# via P/Invoke
//			that will take an array of bytes and a pointer that will point to an 
//			array of encrypted of bytes that will be encrypted using the internal
//			Systems passwords. The memory allocated for the encrypted bytes must be
//			released by the caller (unless there was an exception thrown in which case
//			the method itself will have cleaned it up).  The caller must release the memory
//			with a call to CoTaskMemFree(pBuffer) in C++ or Marshal.FreeCoTaskMem(buffer) in C#.
//			Added as per [LegacyRCAndUtils #4974] - JDS - 05/08/2008.
//			After the call has returned, pulLength will contain the length of the newly
//			allocated buffer that has been returned.
//
// ARGS:	pszInput - A pointer to an array of bytes to be encrypted
//			pulLength - A pointer to an unsigned long that will contain the length
//				of the returned buffer containing the encrypted bytes.
extern EXPORT_BaseUtils unsigned char* externManipulator(const char* pszInput,
	unsigned long* pulLength);

// PURPOSE: To provide a way to log an exception from C# if the COMUCLIDException object fails
//			in some way.
// ARGS:	pszELICode - EliCode for the exception.
//			pszMessage - Message for the exception.
extern EXPORT_BaseUtils void externLogException(char *pszELICode, char *pszMessage);

// forward reference this class so that pointers to it can be used in method arguments and return
// values
class UCLIDExceptionHandler;

//==================================================================================================
// PURPOSE: This class is used to keep track of the last code position reached within a
//			particular method, before an exception was thrown.  This class is to be used along
//			with the INIT_EXCEPTION_AND_TRACING macro.  See that macro's documentation for 
//			further information on how this class should be used.
class EXPORT_BaseUtils LastCodePosition
{
public:
	// Constructor, that requires initialization of the method location identifier.
	LastCodePosition(const string& strMethodLocationIdentifier);

	// Overload the string assignment operator so that the m_strLastCodePos
	// member can easily be updated by setting this object equal to a string
	// value representing the last code position.
	void operator=(const string& strLastCodePos);

	// Return the fully qualified last code position, which is a string obtained by
	// combining the two member variables of this object (m_strMethodLocationIdentifier
	// and m_strLastCodePos) with a "." in between.
	string get() const;

	// This method will return true only if the last call to the assignment operator
	// was made with a non-empty value for strLastCodePos.  In other words, this
	// method will return true only if m_strLastCodePos is not empty.
	bool isDefined() const;

	// Overload the string operator so that a string object can easily be set to this object.
	// This method returns the fully qualified last code position returned by the get()
	// method.
	operator const string();

private:
	// The method location identifier (e.g. "MLI01234")
	string m_strMethodLocationIdentifier;

	// The last code position within the method, represented by any string chosen by the
	// developer of that method, such as "1", "3.1", "6.41c", etc.
	string m_strLastCodePos;
};

//==================================================================================================
//
// CLASS:	UCLIDException
//
// PURPOSE:	This class is to be used as the generic means of throwing exceptions in C++ code.
//			Developers are encouraged to throw application specific exceptions where appropriate and
//			throw this generic C++ exception object everywhere else.
//
// REQUIRE:	1) In general, whenever this exception is caught, it has to be displayed to the user via an
//			appropriate user interface.  In some very rare situations, throwing of this exception
//			may be handled by the program, and therefore the information associated with this
//			exception may not need to be displayed to the user.
//			2) Whenever this exception is caught, if the component catching this exception does not 
//			know how to handle the exception, it must rethrow this exception to its caller (i.e the
//			next higher scope). If any debug info or a new ELI code needs to be associated with
//			the exception, a new UCLIDException should be created that has the caught exception
//			as the inner exception. Any new debug info should be added to the new exception
//			and then the new exception should be thrown.
//			3) Do not catch this exception object if you don't have any specific exception handling
//			requirements associated with catching this exception object.  In other words, if you 
//			don't have anything specific to do after catching this exception, don't catch this 
//			exception - just let it be propagated to the higher scope.
// 
// EXTENSIONS:
//
// NOTES:	
//
class EXPORT_BaseUtils UCLIDException
{
public:
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To construct a new UCLIDException object.
	// REQUIRE: Nothing.
	// PROMISE: To initialize an empty exception object.
	// ARGS:	none
	UCLIDException(void);
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To construct a new UCLIDException object.
	// REQUIRE: strELI must be a unique string among all ELI's of exceptions that are
	//			thrown by software components developed at Extract Systems.
	//			strELI != ""
	//			strText != ""
	// PROMISE: To initialize the constructed object with an initial strELI and strText respectively.
	// ARGS:	strELI: must be a unique string among all ELI's.
	//			strText: Description of the the exception being thrown.
	UCLIDException(const string& strELI, const string& strText);
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To allow copy construction of the UCLIDException object.
	// REQUIRE: Nothing.
	// PROMISE: To construct a new UCLIDException object initialized to the same state as the
	//			UCLIDException object being copied.
	// ARGS:	uclidException: the UCLIDException object, that is to be used to initialize
	//			the newly constructed UCLIDException object.
	UCLIDException(const UCLIDException& uclidException);
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To construct a new exception object that has an inner exception
	// REQUIRE: Nothing.
	// PROMISE: To construct a new UCLIDException object initialized with the strELI and strText for
	//			for the description. 
	// ARGS:	strELI contains the ELI Code.
	//			strText contains the descrption.
	//			ueInnerException contains the inner exception
	UCLIDException(const string& strELI, const string& strText, const UCLIDException& ueInnerException);
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: Destructor to clean up members.
	~UCLIDException();
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To allow assignment of one UCLIDException object to another.
	// REQUIRE: Nothing.
	// PROMISE: To ensure that the data associated with this object is the same as the data
	//			of the assigned object.
	// ARGS:	uclidException: The UCLIDException to be copied.
	UCLIDException& operator=(const UCLIDException& uclidException);
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To initialize this UCLIDException object from a string returned by a call to
	//			asStringizedByteStream.
	// REQUIRE: Nothing.
	// PROMISE: If strData is a string returned from a call to the asStringizedByteStream() method
	//			of another UCLIDException object, then this object is contructed with all the
	//			data associated with the other object.  Otherwise, this exception object is created
	//			using the ELI code given by strELI, and who's text is equal to
	//			strData.
	// ARGS:	strData - exception data in a stringized form.  strData could be a simple error
	//			message or a stringized bytestream representing a complex UCLIDException object.
	//			bLogExceptions - if true then any exceptions thrown while creating the exception
	//				from string will be logged to the exception file.  If false then any
	//				exceptions thrown while creating the exception will be eaten.
	void createFromString(const string& strELI, const string& strData, bool bLogExceptions = true);
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To return a stringized bytestream representing this object.
	// REQUIRE: Nothing.
	// PROMISE: To return a stringized bytestream representing this object.  An exact copy of this
	//			object can be made if the return string is passed to back to the constructor that
	//			takes a string argument.
	// ARGS:	None.
	string asStringizedByteStream() const;
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To return a bytestream representing this object.
	// REQUIRE: Nothing.
	// PROMISE:	To return a bytestream representing this object. An exact copy of this
	//			object can be made if the return stream is passed to the loadFromStream method.
	// ARGS:	None.
	ByteStream asByteStream() const;
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To associate additional debug information to this exception.
	// REQUIRE: strKeyName != ""
	// PROMISE: To return the provided debug information (key/value pair) in all future calls to
	//			getDebugInfo().
	// ARGS:	strKeyName: a unique name (within all debug information keys associated with this 
	//				exception object).
	//			keyValue: a value/type pair containing the debug information to be associated with
	//				the provided keyname.
	//			bValueIsEncrypted: true if value information should be prefaced by 
	//				"Extract_Encrypted: ".  The UCLIDExceptionDetailsDlg will display this value 
	//				as "<Encrypted>" unless Extract System Internal Tools is licensed.
	void addDebugInfo(const string& strKeyName, const ValueTypePair& keyValue, 
		const bool bEncryptValue = false);
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To add debug information containing the last code position.
	// REQUIRE: Nothing.
	// PROMISE: If lastCodePos.isDefined(), then the fully qualified last code position
	//			will be added as debug information to this exception, with the "LastCodePos"
	//			keyname.
	void addDebugInfo(const LastCodePosition& lastCodePos);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To associate an additional exception as debug information.
	// PARAMS:  strKeyName: A unique name (within all debug keys associated with this exception object)
	//          ue: The UCLIDException to add.
	void addDebugInfo(const string& strKeyName, const UCLIDException& ue);
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To associate an additional resolution to help fix the problem that caused this 
	//			exception to be raised.
	// REQUIRE: strResolution != ""
	// PROMISE: To return the provided resolution in all future calls to getPossibleResolutions().
	// ARGS:	strResolution: the developer-provided resolution string to be added to this
	//			exception's list of possible resolutions.
	void addPossibleResolution(const string& strResolution);
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To associate additional debug information to this exception.  Provides a human-
	//			readable error string associated with a non-zero return from GetLastError().
	// REQUIRE: None
	// PROMISE: To return the provided debug information (key/value pair) in all future calls to
	//			getDebugInfo().
	// ARGS:	None
	void addWin32ErrorInfo();
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To associate additional debug information to this exception.  Provides a human-
	//			readable error string associated with a non-zero return from GetLastError().
	// REQUIRE: None
	// PROMISE: To return the provided debug information (key/value pair) in all future calls to
	//			getDebugInfo().
	// ARGS:	dwErrorCode - The Win32 error code (typically retrieved through a call to
	//			GetLastError())
	void addWin32ErrorInfo(DWORD dwErrorCode);
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To associate additional debug information to this exception.  
	//			- HRESULT formatted as a Hex string
	//			- Error message associated with the HRESULT
	//			- Label associated with the HRESULT (e.g. if hr=0x80004004 Label = E_ABORT)
	// REQUIRE: None
	// PROMISE: To return the provided debug information (key/value pair) in all future calls to
	//			getDebugInfo().
	// ARGS:	hr: the value of the HRESULT we want information for
	void addHresult(HRESULT hr);
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To format a string suitable for output by log() or saveTo()
	// REQUIRE: Nothing.
	// PROMISE: Nothing
	// ARGS:	pszMachineName: The name of the machine to place in the log string (if the
	//				machine name is __nullptr then the current machine name will be used)
	//			pszUserName: The name of the user to place in the log string (if the user name
	//				is __nullptr then the current user name will be used)
	//			nDateTime: The number of seconds since 01/01/1970 00:00:00 UTC to place in the
	//				log string (if -1, then	the current time will be used)
	//			nPid: The process id to place in the log string (if -1 the current pid will be used)
	//			pszProductVersion: The product version to place in the log string (if __nullptr
	//				the current product version will be used.
	string createLogString(const char* pszMachineName = __nullptr,
		const char* pszUserName = __nullptr, long nDateTime = -1, int nPid = -1,
		const char* pszProductVersion = __nullptr) const;
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To retrieve text for the exception.
	// PROMISE: To return the exception text associated with this exception.
	// ARGS:	None.
	const string& getTopText() const;
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To retrieve the ELI code for the exception.
	// PROMISE: To return the ELI code associated with the exception.
	// ARGS:	None.
	const string& getTopELI() const;
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To retrieve all the ELI codes.
	// PROMISE: To return all the ELI codes of the current instance and for each inner exception.
	// ARGS:	None.
	string getAllELIs() const;
	//----------------------------------------------------------------------------------------------	
	// PURPOSE: To retrieve the exception problem resolution strings.
	// REQUIRE: Nothing.
	// PROMISE: To return a reference to the developer-provided resolutions associated with this
	//			exception being raised.
	// ARGS:	None.
	const vector<string>& getPossibleResolutions() const;
	//----------------------------------------------------------------------------------------------		
	// PURPOSE: To retrieve the exception debug information.
	// REQUIRE: Nothing.
	// PROMISE: To return a reference to the developer-provided debug-information associated with
	//			this thrown exception object.
	// ARGS:	None.
	const vector<NamedValueTypePair>& getDebugVector() const;
	//----------------------------------------------------------------------------------------------	
	// PURPOSE: To rename the current log file to a timestamped version and logs an exception
	//			in the current log file with the specified message and the name of the renamed
	//			log file in the debug data.
	// ARGS:	strFileName - The exception file to rename.
	//			bUserRenamed - Whether the user initiated the log file rename or it was
	//				auto-renamed.
	//			strComment - The comment that will be added as debug data in the
	//				rename exception trace.
	//			bThrowExceptionOnFailure - Whether an exception should be thrown if the file
	//				cannot be renamed.
	static void renameLogFile(const string& strFileName, bool bUserRenamed = false,
		const string& strComment = "", bool bThrowExceptionOnFailure = false);
	//----------------------------------------------------------------------------------------------	
	// PURPOSE: Writes the exception information to UCLIDException.log file
	// REQUIRE: Nothing
	// PROMISE: The exception will be logged and the FailureDetectionAndReporting System (FDRS)
	//			if available, will be notified of this logged exception event if 
	//			bNotifyFDRS == true
	// ARGS:	strFile: path to file where the exception will be logged.  If blank, the 
	//				exception will be logged to the default ExtractException.uex location.
	//			bNotifyFDRS: a boolean flag indicating whether the FailureDetectionAndReporting 
	//				system should be notified of this exception being logged.  In general, 
	//				the FailureDetectionAndReporting system should be notified of all logged 
	//				exceptions that are not displayed to the user.
	//			bAddDisplayedTag: Prefix the description with "Displayed: " to indicated the
	//				exception was originally displayed to the user.
	//			pszMachineName: The name of the machine to place in the log string (if the
	//				machine name is __nullptr then the current machine name will be used)
	//			pszUserName: The name of the user to place in the log string (if the user name
	//				is __nullptr then the current user name will be used)
	//			nDateTime: The number of seconds since 01/01/1970 00:00:00 UTC to place in the
	//				log string (if -1, then	the current time will be used)
	//			nPid: The process id to place in the log string (if -1 the current pid will be used)
	//			pszProductVersion: The product version to place in the log string (if __nullptr
	//				the current product version will be used.
	void log(const string& strFile = "", bool bNotifyFDRS = true, bool bAddDisplayedTag = false,
		const char* pszMachineName = __nullptr, const char* pszUserName = __nullptr,
		long nDateTime = -1, int nPid = -1, const char* pszProductVersion = __nullptr) const;
	//----------------------------------------------------------------------------------------------	
	// PURPOSE: To save the contents of vectors into file strFile
	// REQUIRE: all parameters should contain valid information.
	// PROMISE: saves the contents into the specified file
	// ARGS:	strFile: path of the file to save with
	//				bAppend: append exception information instead of overwriting the file
	void saveTo(const string& strFile, bool bAppend=false) const;
	//----------------------------------------------------------------------------------------------	
	// PURPOSE: To throw this exception as a STL string object
	// REQUIRE: Nothing.
	// PROMISE: To throw this exception as a STL string object.  The returned object will
	//			be contain the data of this object in a stringized form, as obtained from a 
	//			call to asString()
	// ARGS:	None
	void throwAsSTLString(void) const;
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return the information associated with this object as a string for simple
	//			display purposes.
	// REQUIRE: Nothing.
	// PROMISE: To return all the information associated with this object as a multi-line string
	//			containing the history information, resolutions, and debug information.
	// ARGS:	rResult : the string object that is to contain the information of this object in
	//			string form.
	void asString(string& rResult) const;
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return a default exception handler object.
	// REQUIRE: The caller shall not delete the returned object.
	// PROMISE: To return a default exception handler object which can be used on Win32 platforms
	//			to display information about a caught exception.
	// ARGS:	None.
	static UCLIDExceptionHandler* getDefaultHandler();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Registers a UCLIDExceptionHandler to handle the display of UCLIDExceptions.
	// REQUIRE: This can only be called once.  
	//			The caller is responsible for freeing the handler when done with it.
	// PROMISE: To return the current exception handler, if any.  If there is no current
	//			exception handler, NULL will be returned.
	// ARGS:	UCLIDExceptionHandler* pHandler - the UCLIDExceptionHandler to handle all exceptions	
	static UCLIDExceptionHandler* UCLIDException::setExceptionHandler(UCLIDExceptionHandler* pHandler);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: The UCLIDException will display itself using the default UCLIDExceptionHandler.
	// REQUIRE: 
	// PROMISE: 
	// ARGS:	bLogException - also log this exception
	//			bForceDisplay - display this exception even if it is a subsequent example of 
	//				an error that has been blocked.  One example of a blocked error is 
	//				ELI15373 for License Corruption.
	void display(bool bLogException = true, bool bForceDisplay = false) const;
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return a pointer to the inner exception
	// PROMISE: To return a const pointer to the inner exception of this exception if it is not
	//			NULL otherwise NULL will be returned.
	// ARGS:	
	const UCLIDException *getInnerException() const;
	//----------------------------------------------------------------------------------------------
	// PROMISE: To add a stack trace entry to the vector of Stack Trace entries.
	// ARGS:	strStackTraceEntry is a string that represents a stack trace entry from .NET.
	void addStackTraceEntry(const string& strStackTraceEntry);
	//----------------------------------------------------------------------------------------------
	// PROMISE: To return a reference to the stack trace vector of this exception object.
	const vector<string>& getStackTrace() const;
	//----------------------------------------------------------------------------------------------
	static void setApplication(string strName);
	//----------------------------------------------------------------------------------------------
	static void setSerialNumber(string strSerial);
	//----------------------------------------------------------------------------------------------
	static string getApplication(void);
	//----------------------------------------------------------------------------------------------
	static const string& getSerialNumber(void);
	//----------------------------------------------------------------------------------------------
	void throwAsCOMError();
	//----------------------------------------------------------------------------------------------
	static const string& getDefaultLogFileFullPath();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: for common error codes, set rstrErrorLabel to be the error label as listed in
	//			winerror.h (e.g. if hr == E_ACCESSDENIED rstrErrorLabel = "E_ACCESSDENIED")
	void setErrorLabel(HRESULT hr, string& rstrErrorLabel);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To decrypt the encrypted data value if decryption is allowed, if
	//			decryption is not allowed returns gstrENCRYPTED_INDICATOR.
	//			If the value is not encrypted returns the value.
	static string sGetDataValue(const string& strEncryptedValue);
	//----------------------------------------------------------------------------------------------
	// This class initializes the static critical section, and any other static initialization code
	// in a thread safe manner
	friend class UCLIDExceptionInitializer;
	//----------------------------------------------------------------------------------------------

private:
	static const string ms_strByteStreamSignature;

	// the one and only exception handler
	static UCLIDExceptionHandler* ms_pCurrentExceptionHandler;

	// Provides application name and version
	static string ms_strApplication;

	// Provides hardware lock serial number
	static string ms_strSerial;

	// ELI code associated with this exception.
	string m_strELI;

	// The description string associated with this exception.
	string m_strDescription;

	// Pointer to the inner exception if this is NULL there is no inner exception.
	unique_ptr<UCLIDException> m_apueInnerException;

	// Vector of stack trace items
	vector<string> m_vecStackTrace;

	// vecResolution stores the developer-provided resolutions associated with the problem that
	// caused this exception to be raised.
	vector<string> m_vecResolution;

	vector<NamedValueTypePair> m_vecDebugInfo; //developer-provided debug information associated with this exception

	// Modifies the VTP with "Extract_Encrypted: " + 0x0x0x0x0x0x0x0x
	// where 0x0x... is a stringized encrypted ByteStream of the actual VTP
	// NOTES: (P13 #4642)
	// - The UCLIDExceptionDetailsDlg will display "<Encrypted>" in the 
	//		Value column if Internal_Tools IS NOT licensed
	// - The UCLIDExceptionDetailsDlg will display the actual Value in the 
	//		Value column if Internal_Tools IS licensed
	void getEncryptedValueTypePair(ValueTypePair& keyValue);

	// Method gets the values from the strExceptionAsString for the exception.
	// strExceptionAsString must contain a string that was created
	// with any version of UCLIDException with a call to asStringizedByteStream
	void loadFromString(const string& strExceptionAsString);

	// Method gets the values from the ByteStream version of an exception.
	void loadFromStream(ByteStream& rByteStream);

	// Clears the internal data of the exception object. 
	// This should be called before an exception is loaded from a bytestream or file.
	void clear();

	// Returns the exception as a string. 
	// if bRecursiveCall is false, the string passed will be cleared and the Application version 
	//		will be added as the last line
	// if bRecursiveCall is true the exception information will be appended to the string.
	void asString(string& rResult, bool bRecursiveCall) const;
};

//==================================================================================================
//
// CLASS:	UCLIDExceptionHandler
//
// PURPOSE:	This pure abstract class defines the interface for any class that can handle
//			a caught UCLIDException.  In general, this interface is designed for use with
//			platform specific GUI's that can display the UCLIDException details.
//
// REQUIRE:	Nothing.
// 
// INVARIANTS:
//
// EXTENSIONS:
//
// NOTES:	
//
class EXPORT_BaseUtils UCLIDExceptionHandler
{
public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To handle the caught exception.
	// REQUIRE: Nothing.
	// PROMISE: Nothing.
	// ARGS:	uclidException: the UCLIDException object that was thrown, and caught, and now
	//			needs to be handled.
	virtual void handleException(const UCLIDException& uclidException) = 0;
	//----------------------------------------------------------------------------------------------
};

//==================================================================================================
// PURPOSE: To modify the current exception handler within a given scope
// PROMISE: To restore the previous exception handler when this object
//			goes out of scope.
class EXPORT_BaseUtils GuardedExceptionHandler
{
public:
	// ARGS: pNewHandler: the new exception handler to be set for the scope of
	//		of this object.  If the default exception handler is to be used for
	//		the scope of this object, pass in NULL for this argument.
	GuardedExceptionHandler(UCLIDExceptionHandler* pNewHandler);
	~GuardedExceptionHandler();

private:
	UCLIDExceptionHandler* m_pOldHandler;
};

//==================================================================================================
//== M A C R O S ===================================================================================
//==================================================================================================
// PURPOSE: The purpose of this macro is to define an instance of the LastCodePosition object,
//			initializing it with a Method-Location-Identifier (MLI) code, and giving the variable
//			a pre-defined, hard-coded name of _lastCodePos.  All the various CATCH_.... macros
//			defined below will automatically add _lastCodePos to the debug information of the
//			thrown, displayed, or logged exception if _lastCodePos has been set to a non-empty
//			string in code.
//
// REQUIRE:	READ CAREFULLY - THIS IS IMPORTANT!!!
//			1. Once an MLI code has been used in code, it must never be reused anywhere else in code.
//			2. The last code position strings must be unique within the scope of a method.
//			
// NOTES:	Below is an example of a method where this macro is used.  Within the method, this
//			code goes to the extreme extent of marking the current position after every method
//			call.  Of course, the last code position does not need to be updated that frequently.
//			Each developer should decide how frequently, and where in code it would be most
//			appropriate to update the last code position.
//
//			void MyObject::method1(int iArg1, int iArg2)
//			{
//				INIT_EXCEPTION_AND_TRACING("MLI00000");
//
//				try
//				{
//					doSomething1();	// may throw exception
//					_lastCodePos = "1";
//					doSomething2(); // may throw exception
//					_lastCodePos = "2";
//					if (iArg1 < 0)
//					{
//						_lastCodePos = "3a.1";
//						doSomething3(); // may throw exception
//						_lastCodePos = "3a.2";
//						doSomething4(); // may throw exception
//					}
//					else
//					{
//						_lastCodePos = "3b.1";
//						doSomething5();
//					}
//				}
//				CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI99999")
//				// Any of the other CATCH_... macros can be used above
//				// and they will automatically add the last code position as debug
//				// information, if a last code position has been defined.
//			}
//
#define INIT_EXCEPTION_AND_TRACING(x) \
	 LastCodePosition _lastCodePos(x); \
	_lastCodePos = "";

//--------------------------------------------------------------------------------------------------
// PURPOSE: To provide an easy mechanism to validate input arguments and throw an UCLIDException
//			when the input requirements are not met.
// REQUIRE: strELI must be a unique string among all ELI's of exceptions that are thrown by software
//			components developed at UCLID or for UCLID.
//			strELI != ""
//			bCondition must be evaluate to a boolean value.
// PROMISE: if bCondition evaluates to false, a UCLIDException will be thrown with the specified
//			ELI, and with the stringized condition as the associated debug information.
#define ASSERT_ARGUMENT(strELI, bCondition) \
	if (!(bCondition)) \
	{ \
		UCLIDException ue(strELI, "Internal error: Function/Method requirements not met."); \
		ue.addDebugInfo("Failed condition", #bCondition); \
		throw ue; \
	}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To provide an easy mechanism to validate resource allocation and throw an UCLIDException
//			when the resource allocation requirements are not met.
// REQUIRE: strELI must be a unique string among all ELI's of exceptions that are thrown by software
//			components developed at UCLID or for UCLID.
//			strELI != ""
//			bCondition must be evaluate to a boolean value.
// PROMISE: if bCondition evaluates to false, a UCLIDException will be thrown with the specified
//			ELI, and with the stringized condition as the associated debug information.
#define ASSERT_RESOURCE_ALLOCATION(strELI, bCondition) \
	if (!(bCondition)) \
	{ \
		UCLIDException ue(strELI, "System error: Unable to allocate necessary resources."); \
		ue.addDebugInfo("Failed condition", #bCondition); \
		throw ue; \
	}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To provide an easy mechanism to throw a UCLIDException when a logic error is encountered.
// REQUIRE: strELI must be a unique string among all ELI's of exceptions that are thrown by software
//			components developed at UCLID or for UCLID.
//			strELI != ""
// PROMISE: A UCLIDException will be thrown with the specified ELI, and "Internal logic error." as 
//			the exception text.
#define THROW_LOGIC_ERROR_EXCEPTION(strELI) \
	{ \
		UCLIDException ue(strELI, "Internal logic error."); \
		throw ue; \
	}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To provide an easy mechanism to handle UCLID exceptions by default.
// REQUIRE: Nothing.
// PROMISE: This macro will expand into a catch(UCLIDException& ue), displaying the caught
//			UCLIDException using the default exception handler.
#define CATCH_UCLID_EXCEPTION(strELI) \
	catch (UCLIDException& ue) \
	{ \
		ue.addDebugInfo("CatchID", strELI); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
	}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To provide an easy mechanism to handle COM exceptions by default.
// REQUIRE: Nothing.
// PROMISE: This macro will expand into a catch(_com_error& e), converting the caught
//			COM exception into an UCLIDException, and displaying the UCLIDException using the 
//			default exception handler.
#define CATCH_COM_EXCEPTION(strELI) \
	catch (_com_error& e) \
	{ \
		UCLIDException ue; \
		_bstr_t _bstrDescription = e.Description(); \
		char *pszDescription = _bstrDescription; \
		if (pszDescription) \
			ue.createFromString(strELI, pszDescription); \
		else \
			ue.createFromString(strELI, "COM exception caught!"); \
		ue.addHresult(e.Error()); \
		ue.addDebugInfo("err.WCode", e.WCode()); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
	}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To provide an easy mechanism to handle OLE exceptions by default.
// REQUIRE: Nothing.
// PROMISE: This macro will expand into a catch(COleException& e), converting the caught
//			OLE exception into an UCLIDException, adding available information as debug info, and 
//			displaying the UCLIDException using the default exception handler.
#define CATCH_OLE_EXCEPTION(strELI) \
	catch (COleDispatchException *pEx) \
	{ \
		string strDesc = (LPCTSTR) pEx->m_strDescription; \
		UCLIDException ue; \
		ue.createFromString(strELI, strDesc.empty() ? "OLE dispatch exception caught." : strDesc); \
		ue.addDebugInfo("Error Code", pEx->m_wCode); \
		pEx->Delete(); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
	} \
	catch (COleDispatchException& ex) \
	{ \
		string strDesc = (LPCTSTR) ex.m_strDescription; \
		UCLIDException ue; \
		ue.createFromString(strELI, strDesc.empty() ? "OLE dispatch exception caught." : strDesc); \
		ue.addDebugInfo("Error Code", ex.m_wCode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
	} \
	catch (COleException& ex) \
	{ \
		char pszCause[256] = {0}; \
		ex.GetErrorMessage(pszCause, 255); \
		UCLIDException ue; \
		ue.createFromString(strELI, pszCause == "" ? "OLE exception caught." : pszCause); \
		ue.addDebugInfo("Status Code", ex.m_sc); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
	}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To provide an easy mechanism to handle unexpected exceptions by default.
// REQUIRE: strELI must be a unique string among all ELI's of exceptions that are thrown by software
//			components developed at UCLID or for UCLID.
//			strELI != ""
// PROMISE: This macro will expand into a catch(...), displaying a UCLIDException object
//			with the ELI set to strELI and the message set to "Unexpected exception caught".
#define CATCH_UNEXPECTED_EXCEPTION(strELI) \
	catch (...) \
	{ \
		UCLIDException ue(strELI, "Unexpected exception caught."); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
	}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To provide an easy mechanism to return a COM error from caught exception objects
// REQUIRE: strELI must be a unique string among all ELI's of exceptions that are thrown by software
//			components developed at UCLID or for UCLID.
//			strELI != ""
// PROMISE: This macro will expand to catch certain exceptions and return as COM Error codes to the 
//			caller.  If UCLIDException objects are caught, then the stringized UCLIDException object
//			is returned as the COM error string.  If a _com_error object is caught, it's various 
//			important data members are stringized and returned as a COM error string.  If any other
//			exception is caught, then a new UCLIDException object is created with the specified ELI
//			code (strELICode), and a stringized version of this UCLIDException object is returned as
//			a COM error.
#define CATCH_ALL_AND_RETURN_AS_COM_ERROR(strELICode) \
	catch (_com_error& err) \
	{ \
		UCLIDException ue; \
		_bstr_t _bstrDescription = err.Description(); \
		char *pszDescription = _bstrDescription; \
		if (pszDescription) \
			ue.createFromString(strELICode, pszDescription); \
		else \
			ue.createFromString(strELICode, "COM exception caught!"); \
		ue.addHresult(err.Error()); \
		ue.addDebugInfo("err.WCode", err.WCode()); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		return Error(ue.asStringizedByteStream().c_str()); \
	} \
	catch (COleDispatchException *pEx) \
	{ \
		string strDesc = (LPCTSTR) pEx->m_strDescription; \
		UCLIDException ue; \
		ue.createFromString(strELICode, strDesc.empty() ? "OLE dispatch exception caught." : strDesc); \
		ue.addDebugInfo("Error Code", pEx->m_wCode); \
		pEx->Delete(); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		return Error(ue.asStringizedByteStream().c_str()); \
	} \
	catch (COleDispatchException& ex) \
	{ \
		string strDesc = (LPCTSTR) ex.m_strDescription; \
		UCLIDException ue; \
		ue.createFromString(strELICode, strDesc.empty() ? "OLE dispatch exception caught." : strDesc); \
		ue.addDebugInfo("Error Code", ex.m_wCode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		return Error(ue.asStringizedByteStream().c_str()); \
	} \
	catch (COleException& ex) \
	{ \
		char pszCause[256] = {0}; \
		ex.GetErrorMessage(pszCause, 255); \
		UCLIDException ue; \
		ue.createFromString(strELICode, pszCause == "" ? "OLE exception caught." : pszCause); \
		ue.addDebugInfo("Status Code", ex.m_sc); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		return Error(ue.asStringizedByteStream().c_str()); \
	} \
	catch (CException* pEx) \
	{ \
		char pszCause[256] = {0}; \
		pEx->GetErrorMessage(pszCause, 255); \
		pEx->Delete(); \
		UCLIDException ue; \
		ue.createFromString(strELICode, pszCause == "" ? "C Exception caught." : pszCause); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		return Error(ue.asStringizedByteStream().c_str()); \
	} \
	catch (UCLIDException& ue) \
	{ \
		ue.addDebugInfo("CatchID", strELICode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		return Error(ue.asStringizedByteStream().c_str()); \
	} \
	catch (...) \
	{ \
		UCLIDException ue(strELICode, "Unexpected exception caught!"); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		return Error(ue.asStringizedByteStream().c_str()); \
	} \
	return S_FALSE; /* dummy */

//--------------------------------------------------------------------------------------------------
// PURPOSE:	This macro has the same use as the above macro, except that it is intended for
//			use in projects where there is no MFC support (e.g. services, or ATL COM EXE projects)
#define CATCH_ALL_AND_RETURN_AS_COM_ERROR_NO_MFC(strELICode) \
	catch (_com_error& err) \
	{ \
		UCLIDException ue; \
		_bstr_t _bstrDescription = err.Description(); \
		char *pszDescription = _bstrDescription; \
		if (pszDescription) \
			ue.createFromString(strELICode, pszDescription); \
		else \
			ue.createFromString(strELICode, "COM exception caught!"); \
		ue.addHresult(err.Error()); \
		ue.addDebugInfo("err.WCode", err.WCode()); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		return Error(ue.asStringizedByteStream().c_str()); \
	} \
	catch (UCLIDException& ue) \
	{ \
		ue.addDebugInfo("CatchID", strELICode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		return Error(ue.asStringizedByteStream().c_str()); \
	} \
	catch (...) \
	{ \
		UCLIDException ue(strELICode, "Unexpected exception caught!"); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		return Error(ue.asStringizedByteStream().c_str()); \
	} \
	return S_FALSE; /* dummy */

//--------------------------------------------------------------------------------------------------
// PURPOSE: To provide an easy mechanism to catch an exception and log it.
// REQUIRE: strELI must be a unique string among all ELI's of exceptions that are thrown by software
//			components developed at UCLID or for UCLID.
//			strELI != ""
// PROMISE: This macro will expand to catch certain exceptions and log them.
//			If UCLIDException objects are caught, they are just logged.  If a _com_error object 
//			is caught, it's various important data members are stringized and logged.  If any other
//			exception is caught, then a new UCLIDException object is created with the specified ELI
//			code (strELICode) and logged.
#define CATCH_AND_LOG_ALL_EXCEPTIONS(strELICode) \
	catch (_com_error& err) \
	{ \
		UCLIDException ue; \
		_bstr_t _bstrDescription = err.Description(); \
		char *pszDescription = _bstrDescription; \
		if (pszDescription) \
			ue.createFromString(strELICode, pszDescription); \
		else \
			ue.createFromString(strELICode, "COM exception caught!"); \
		ue.addHresult(err.Error()); \
		ue.addDebugInfo("err.WCode", err.WCode()); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.log(); \
	} \
	catch (UCLIDException& ue) \
	{ \
		ue.addDebugInfo("CatchID", strELICode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.log(); \
	} \
	catch (COleDispatchException *pEx) \
	{ \
		string strDesc = (LPCTSTR) pEx->m_strDescription; \
		UCLIDException ue; \
		ue.createFromString(strELICode, strDesc.empty() ? "OLE dispatch exception caught." : strDesc); \
		ue.addDebugInfo("Error Code", pEx->m_wCode); \
		pEx->Delete(); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.log(); \
	} \
	catch (COleDispatchException& ex) \
	{ \
		string strDesc = (LPCTSTR) ex.m_strDescription; \
		UCLIDException ue; \
		ue.createFromString(strELICode, strDesc.empty() ? "OLE dispatch exception caught." : strDesc); \
		ue.addDebugInfo("Error Code", ex.m_wCode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.log(); \
	} \
	catch (COleException& ex) \
	{ \
		char pszCause[256] = {0}; \
		ex.GetErrorMessage(pszCause, 255); \
		UCLIDException ue; \
		ue.createFromString(strELICode, pszCause == "" ? "OLE exception caught." : pszCause); \
		ue.addDebugInfo("Status Code", ex.m_sc); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.log(); \
	} \
	catch (CException* pEx) \
	{ \
		char pszCause[256] = {0}; \
		pEx->GetErrorMessage(pszCause, 255); \
		pEx->Delete(); \
		UCLIDException ue; \
		ue.createFromString(strELICode, pszCause == "" ? "C Exception caught." : pszCause); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.log(); \
	} \
	catch (...) \
	{ \
		UCLIDException ue(strELICode, "Unexpected exception caught!"); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.log(); \
	}

//--------------------------------------------------------------------------------------------------
// PURPOSE: Same as the CATCH_AND_LOG_ALL_EXCEPTIONS macro, except no MFC support
#define CATCH_AND_LOG_ALL_EXCEPTIONS_NO_MFC(strELICode) \
	catch (_com_error& err) \
	{ \
		UCLIDException ue; \
		_bstr_t _bstrDescription = err.Description(); \
		char *pszDescription = _bstrDescription; \
		if (pszDescription) \
			ue.createFromString(strELICode, pszDescription); \
		else \
			ue.createFromString(strELICode, "COM exception caught!"); \
		ue.addHresult(err.Error()); \
		ue.addDebugInfo("err.WCode", err.WCode()); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.log(); \
	} \
	catch (UCLIDException& ue) \
	{ \
		ue.addDebugInfo("CatchID", strELICode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.log(); \
	} \
	catch (...) \
	{ \
		UCLIDException ue(strELICode, "Unexpected exception caught!"); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.log(); \
	}

//--------------------------------------------------------------------------------------------------
// PURPOSE: To provide an basic mechanism to handle all exceptions by default.
// REQUIRE: Nothing.
// PROMISE: This macro will expand into a sequence of catch blocks, converting the caught
//			exception into an UCLIDException, adding available information as debug info, and 
//			displaying the UCLIDException using the default exception handler.  If bRethrow
//			is true, then the displayed exception is rethrown.
#define CATCH_AND_DISPLAY_ALL_EXCEPTIONS_ROOT(strELI, bRethrow) \
	catch (UCLIDException& ue) \
	{ \
		ue.addDebugInfo("CatchID", strELI); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
		if (bRethrow) throw; \
	} \
	catch (_com_error& e) \
	{ \
		UCLIDException ue; \
		_bstr_t _bstrDescription = e.Description(); \
		char *pszDescription = _bstrDescription; \
		if (pszDescription) \
			ue.createFromString(strELI, pszDescription); \
		else \
			ue.createFromString(strELI, "COM exception caught!"); \
		ue.addHresult(e.Error()); \
		ue.addDebugInfo("err.WCode", e.WCode()); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
		if (bRethrow) throw; \
	} \
	catch (COleDispatchException *pEx) \
	{ \
		string strDesc = (LPCTSTR) pEx->m_strDescription; \
		UCLIDException ue; \
		ue.createFromString(strELI, strDesc.empty() ? "OLE dispatch exception caught." : strDesc); \
		ue.addDebugInfo("Error Code", pEx->m_wCode); \
		pEx->Delete(); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
		if (bRethrow) throw; \
	} \
	catch (COleDispatchException& ex) \
	{ \
		string strDesc = (LPCTSTR) ex.m_strDescription; \
		UCLIDException ue; \
		ue.createFromString(strELI, strDesc.empty() ? "OLE dispatch exception caught." : strDesc); \
		ue.addDebugInfo("Error Code", ex.m_wCode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
		if (bRethrow) throw; \
	} \
	catch (COleException& ex) \
	{ \
		char pszCause[256] = {0}; \
		ex.GetErrorMessage(pszCause, 255); \
		UCLIDException ue; \
		ue.createFromString(strELI, pszCause == "" ? "OLE exception caught." : pszCause); \
		ue.addDebugInfo("Status Code", ex.m_sc); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
		if (bRethrow) throw; \
	} \
	catch (CException* pEx) \
	{ \
		char pszCause[256] = {0}; \
		pEx->GetErrorMessage(pszCause, 255); \
		pEx->Delete(); \
		UCLIDException ue; \
		ue.createFromString(strELI, pszCause == "" ? "C Exception caught." : pszCause); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
		if (bRethrow) throw; \
	} \
	catch (...) \
	{ \
		UCLIDException ue(strELI, "Unexpected exception caught."); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
		if (bRethrow) throw; \
	}
//--------------------------------------------------------------------------------------------------
// PURPOSE:	This macro has the same use as the above macro, except that it is intended for
//			use in projects where there is no MFC support (e.g. services, or ATL COM EXE projects)
#define CATCH_AND_DISPLAY_ALL_EXCEPTIONS_ROOT_NO_MFC(strELI, bRethrow) \
	catch (UCLIDException& ue) \
	{ \
		ue.addDebugInfo("CatchID", strELI); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
		if (bRethrow) throw; \
	} \
	catch (_com_error& e) \
	{ \
		UCLIDException ue; \
		_bstr_t _bstrDescription = e.Description(); \
		char *pszDescription = _bstrDescription; \
		if (pszDescription) \
			ue.createFromString(strELI, pszDescription); \
		else \
			ue.createFromString(strELI, "COM exception caught!"); \
		ue.addHresult(e.Error()); \
		ue.addDebugInfo("err.WCode", e.WCode()); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
		if (bRethrow) throw; \
	} \
	catch (...) \
	{ \
		UCLIDException ue(strELI, "Unexpected exception caught."); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
		if (bRethrow) throw; \
	}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To provide an basic mechanism to handle all exceptions by default.
// REQUIRE: Nothing.
// PROMISE: This macro will expand into a sequence of catch blocks, converting the caught
//			exception into an UCLIDException, adding available information as debug info, and 
//			then the created UCLIDException is rethrown.
#define CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION(strELI) \
	catch (UCLIDException& ue) \
	{ \
		ue.addDebugInfo("CatchID", strELI); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		throw ue; \
	} \
	catch (_com_error& e) \
	{ \
		UCLIDException ue; \
		_bstr_t _bstrDescription = e.Description(); \
		char *pszDescription = _bstrDescription; \
		if (pszDescription) \
			ue.createFromString(strELI, pszDescription); \
		else \
			ue.createFromString(strELI, "COM exception caught!"); \
		ue.addHresult(e.Error()); \
		ue.addDebugInfo("err.WCode", e.WCode()); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		throw ue; \
	} \
	catch (COleDispatchException *pEx) \
	{ \
		string strDesc = (LPCTSTR) pEx->m_strDescription; \
		UCLIDException ue; \
		ue.createFromString(strELI, strDesc.empty() ? "OLE dispatch exception caught." : strDesc); \
		ue.addDebugInfo("Error Code", pEx->m_wCode); \
		pEx->Delete(); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		throw ue; \
	} \
	catch (COleDispatchException& ex) \
	{ \
		string strDesc = (LPCTSTR) ex.m_strDescription; \
		UCLIDException ue; \
		ue.createFromString(strELI, strDesc.empty() ? "OLE dispatch exception caught." : strDesc); \
		ue.addDebugInfo("Error Code", ex.m_wCode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		throw ue; \
	} \
	catch (COleException& ex) \
	{ \
		char pszCause[256] = {0}; \
		ex.GetErrorMessage(pszCause, 255); \
		UCLIDException ue; \
		ue.createFromString(strELI, pszCause == "" ? "OLE exception caught." : pszCause); \
		ue.addDebugInfo("Status Code", ex.m_sc); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		throw ue; \
	} \
	catch (CException* pEx) \
	{ \
		char pszCause[256] = {0}; \
		pEx->GetErrorMessage(pszCause, 255); \
		pEx->Delete(); \
		UCLIDException ue; \
		ue.createFromString(strELI, pszCause == "" ? "C Exception caught." : pszCause); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		throw ue; \
	} \
	catch (...) \
	{ \
		UCLIDException ue(strELI, "Unexpected exception caught."); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		throw ue; \
	}
//--------------------------------------------------------------------------------------------------
// PURPOSE: This macro has the same use as the above macro, except that it is intended for
//			use in projects where there is no MFC support (e.g. services, or ATL COM EXE projects)
#define CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION_NO_MFC(strELI) \
	catch (UCLIDException& ue) \
	{ \
		ue.addDebugInfo("CatchID", strELI); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		throw ue; \
	} \
	catch (_com_error& e) \
	{ \
		UCLIDException ue; \
		_bstr_t _bstrDescription = e.Description(); \
		char *pszDescription = _bstrDescription; \
		if (pszDescription) \
			ue.createFromString(strELI, pszDescription); \
		else \
			ue.createFromString(strELI, "COM exception caught!"); \
		ue.addHresult(e.Error()); \
		ue.addDebugInfo("err.WCode", e.WCode()); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		throw ue; \
	} \
	catch (...) \
	{ \
		UCLIDException ue(strELI, "Unexpected exception caught."); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		throw ue; \
	}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To provide an easy way to catch any exception and send to the test result logger
//			that is part of the UCLID Core Testing Framework.
// REQUIRE: rbExceptionCaught is of bool type, and has been initialized to false.
//			bFailTest is of VARIANT_BOOL type
// PROMISE: If an exception is caught, this macro will catch different types of exceptions, 
//			convert them into UCLIDExceptions, add the test case exception to the test result 
//			logger, and set the rbExceptionCaught boolean variable to true.
#define CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION(strELICode, pTestResultLogger, rbExceptionCaught, vbFailTest) \
	catch (_com_error& err) \
	{ \
		UCLIDException ue; \
		_bstr_t _bstrDescription = err.Description(); \
		char *pszDescription = _bstrDescription; \
		if (pszDescription) \
			ue.createFromString(strELICode, pszDescription); \
		else \
			ue.createFromString(strELICode, "COM exception caught!"); \
		ue.addHresult(err.Error()); \
		ue.addDebugInfo("err.WCode", err.WCode()); \
		ue.addDebugInfo("CatchID", strELICode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		rbExceptionCaught = true; \
		pTestResultLogger->AddTestCaseException(_bstr_t(ue.asStringizedByteStream().c_str()), vbFailTest); \
	} \
	catch (COleDispatchException *pEx) \
	{ \
		string strDesc = (LPCTSTR) pEx->m_strDescription; \
		UCLIDException ue; \
		ue.createFromString(strELICode, strDesc.empty() ? "OLE dispatch exception caught." : strDesc); \
		ue.addDebugInfo("Error Code", pEx->m_wCode); \
		pEx->Delete(); \
		ue.addDebugInfo("CatchID", strELICode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		rbExceptionCaught = true; \
		pTestResultLogger->AddTestCaseException(_bstr_t(ue.asStringizedByteStream().c_str()), vbFailTest); \
	} \
	catch (COleDispatchException& ex) \
	{ \
		string strDesc = (LPCTSTR) ex.m_strDescription; \
		UCLIDException ue; \
		ue.createFromString(strELICode, strDesc.empty() ? "OLE dispatch exception caught." : strDesc); \
		ue.addDebugInfo("Error Code", ex.m_wCode); \
		ue.addDebugInfo("CatchID", strELICode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		rbExceptionCaught = true; \
		pTestResultLogger->AddTestCaseException(_bstr_t(ue.asStringizedByteStream().c_str()), vbFailTest); \
	} \
	catch (COleException& ex) \
	{ \
		char pszCause[256] = {0}; \
		ex.GetErrorMessage(pszCause, 255); \
		UCLIDException ue; \
		ue.createFromString(strELICode, pszCause == "" ? "OLE exception caught." : pszCause); \
		ue.addDebugInfo("Status Code", ex.m_sc); \
		ue.addDebugInfo("CatchID", strELICode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		rbExceptionCaught = true; \
		pTestResultLogger->AddTestCaseException(_bstr_t(ue.asStringizedByteStream().c_str()), vbFailTest); \
	} \
	catch (CException* pEx) \
	{ \
		char pszCause[256] = {0}; \
		pEx->GetErrorMessage(pszCause, 255); \
		pEx->Delete(); \
		UCLIDException ue; \
		ue.createFromString(strELICode, pszCause == "" ? "C Exception caught." : pszCause); \
		ue.addDebugInfo("CatchID", strELICode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		rbExceptionCaught = true; \
		pTestResultLogger->AddTestCaseException(_bstr_t(ue.asStringizedByteStream().c_str()), vbFailTest); \
	} \
	catch (UCLIDException& ue) \
	{ \
		ue.addDebugInfo("CatchID", strELICode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		rbExceptionCaught = true; \
		pTestResultLogger->AddTestCaseException(_bstr_t(ue.asStringizedByteStream().c_str()), vbFailTest); \
	} \
	catch (...) \
	{ \
		UCLIDException ue(strELICode, "Unexpected exception caught!"); \
		ue.addDebugInfo("CatchID", strELICode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		rbExceptionCaught = true; \
		pTestResultLogger->AddTestCaseException(_bstr_t(ue.asStringizedByteStream().c_str()), vbFailTest); \
	}

//--------------------------------------------------------------------------------------------------
// PURPOSE: To provide an easy mechanism to handle all exceptions by default.
// REQUIRE: Nothing.
// PROMISE: This macro will expand into a sequence of catch blocks, converting the caught
//			exception into an UCLIDException, adding available information as debug info, and 
//			displaying the UCLIDException using the default exception handler.
#define CATCH_AND_DISPLAY_ALL_EXCEPTIONS(strELI) \
	catch (UCLIDException& ue) \
	{ \
		ue.addDebugInfo("CatchID", strELI); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
	} \
	catch (_com_error& e) \
	{ \
		UCLIDException ue; \
		_bstr_t _bstrDescription = e.Description(); \
		char *pszDescription = _bstrDescription; \
		if (pszDescription) \
			ue.createFromString(strELI, pszDescription); \
		else \
			ue.createFromString(strELI, "COM exception caught!"); \
		ue.addHresult(e.Error()); \
		ue.addDebugInfo("err.WCode", e.WCode()); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
	} \
	catch (COleDispatchException *pEx) \
	{ \
		string strDesc = (LPCTSTR) pEx->m_strDescription; \
		UCLIDException ue; \
		ue.createFromString(strELI, strDesc.empty() ? "OLE dispatch exception caught." : strDesc); \
		ue.addDebugInfo("Error Code", pEx->m_wCode); \
		pEx->Delete(); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
	} \
	catch (COleDispatchException& ex) \
	{ \
		string strDesc = (LPCTSTR) ex.m_strDescription; \
		UCLIDException ue; \
		ue.createFromString(strELI, strDesc.empty() ? "OLE dispatch exception caught." : strDesc); \
		ue.addDebugInfo("Error Code", ex.m_wCode); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
	} \
	catch (COleException& ex) \
	{ \
		char pszCause[256] = {0}; \
		ex.GetErrorMessage(pszCause, 255); \
		UCLIDException ue; \
		ue.createFromString(strELI, pszCause == "" ? "OLE exception caught." : pszCause); \
		ue.addDebugInfo("Status Code", ex.m_sc); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
	} \
	catch (CException* pEx) \
	{ \
		char pszCause[256] = {0}; \
		pEx->GetErrorMessage(pszCause, 255); \
		pEx->Delete(); \
		UCLIDException ue; \
		ue.createFromString(strELI, pszCause == "" ? "C Exception caught." : pszCause); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
	} \
	catch (...) \
	{ \
		UCLIDException ue(strELI, "Unexpected exception caught."); \
		__if_exists(_lastCodePos) \
		{ \
			ue.addDebugInfo(_lastCodePos); \
		} \
		ue.display(); \
	}
//--------------------------------------------------------------------------------------------------
// PURPOSE:	This macro has the same use as the above macro, except that it is intended for
//			use in projects where there is no MFC support (e.g. services, or ATL COM EXE projects)
#define CATCH_AND_DISPLAY_ALL_EXCEPTIONS_NO_MFC(strELI) \
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS_ROOT_NO_MFC(strELI, false)
//--------------------------------------------------------------------------------------------------
// PURPOSE: To provide an easy mechanism to handle all exceptions by default.
// REQUIRE: Nothing.
// PROMISE: This macro will expand into a sequence of catch blocks, converting the caught
//			exception into an UCLIDException, adding available information as debug info, and 
//			displaying the UCLIDException using the default exception handler.  Further, after
//			the exception is displayed, it will be rethrown to the outer scope.
#define CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS(strELI) \
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS_ROOT(strELI, true)
//--------------------------------------------------------------------------------------------------
// PURPOSE:	This macro has the same use as the above macro, except that it is intended for
//			use in projects where there is no MFC support (e.g. services, or ATL COM EXE projects)
#define CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS_NO_MFC(strELI) \
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS_ROOT_NO_MFC(strELI, true)
//--------------------------------------------------------------------------------------------------
// PURPOSE: To deal with failed HRESULTS in the context of exception handling in COM objects
// REQUIRE: hr = an HRESULT (or expression which evaluates to an HRESULT)
//			strELICode = a unique ELI code for this HANDLE_HRESULT
//			strErrorDescription = a description of this error
//			interface_ptr = a pointer to the interface being accessed at the time of failure
//			iid_interface_name = the IID of the interface being accesses at the time of failure
// PROMISE: This macro check for a failed HRESULT and in the case of failed HRESULT will build
//			a new UCLIDException and throw it.  This macro will attempt to gather as much error
//			information as possible regarding the failed HRESULT and add that as debug info to
//			the new exception
#define HANDLE_HRESULT(hr, strELICode, strErrorDescription, interface_ptr, iid_interface_name) \
{ \
	if (FAILED(hr)) \
	{ \
		CComQIPtr<ISupportErrorInfo> ipSupportErrorInfoPtr = interface_ptr; \
		if (ipSupportErrorInfoPtr != __nullptr) \
		{ \
			if (FAILED(ipSupportErrorInfoPtr->InterfaceSupportsErrorInfo(iid_interface_name))) \
			{ \
				UCLIDException ue("ELI16874", "COM Exception caught!", UCLIDException(strELICode, strErrorDescription)); \
				ue.addHresult(hr); \
				ue.addDebugInfo("Reason", "COM object does not support rich error information for the given interface!"); \
				ue.addDebugInfo("Interface", #iid_interface_name); \
				throw ue; \
			} \
			IErrorInfoPtr ipErrorInfo; \
			if (GetErrorInfo(NULL, &ipErrorInfo) == S_OK) \
			{ \
				_bstr_t _bzDesc = ""; \
				ipErrorInfo->GetDescription(_bzDesc.GetAddress()); \
				if (!_bzDesc) \
				{ \
					_bzDesc = ""; \
				} \
				_bstr_t _bzSource = ""; \
				ipErrorInfo->GetSource(_bzSource.GetAddress()); \
				if (!_bzSource) \
				{ \
					_bzSource = ""; \
				} \
				_bstr_t _bzHelpFile = ""; \
				ipErrorInfo->GetHelpFile(_bzHelpFile.GetAddress()); \
				if (!_bzHelpFile) \
				{ \
					_bzHelpFile = ""; \
				} \
				DWORD dHelpContext; \
				ipErrorInfo->GetHelpContext(&dHelpContext); \
				string strDescription(_bzDesc); \
				string strSource(_bzSource); \
				string strHelpFile(_bzHelpFile); \
				string strHelpContext = asString(dHelpContext); \
				UCLIDException ueInner; \
				ueInner.createFromString("ELI16875", strDescription); \
				UCLIDException ue(strELICode, strErrorDescription, ueInner); \
				ue.addDebugInfo("Source", strSource); \
				ue.addDebugInfo("Help File", strHelpFile); \
				ue.addDebugInfo("Help Context", strHelpContext); \
				ue.addHresult(hr); \
				ue.addDebugInfo("Interface", #iid_interface_name); \
				throw ue; \
			} \
			else \
			{ \
				UCLIDException ue("ELI16876", "COM Exception caught!", UCLIDException(strELICode, strErrorDescription)); \
				ue.addHresult(hr); \
				ue.addDebugInfo("Reason", "Unable to retrieve COM error information!"); \
				ue.addDebugInfo("Interface", #iid_interface_name); \
				throw ue; \
			} \
		} \
		else \
		{ \
			UCLIDException ue("ELI16877", "COM Exception caught!", UCLIDException(strELICode, strErrorDescription)); \
			ue.addHresult(hr); \
			ue.addDebugInfo("Reason", "COM object does not implement ISupportErrorInfo!"); \
			ue.addDebugInfo("Interface", #iid_interface_name); \
			throw ue; \
		} \
	} \
}
//--------------------------------------------------------------------------------------------------
#pragma warning(pop)