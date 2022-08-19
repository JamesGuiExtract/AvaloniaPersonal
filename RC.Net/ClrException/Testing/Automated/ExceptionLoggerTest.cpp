#include "stdafx.h"
#include <vcclr.h>
#include "ExceptionLoggerTest.h"
#include <ExceptionLogger.h>
#include <msclr\marshal_cppstd.h>

using namespace System;
using namespace System::Diagnostics;
using namespace System::IO;
using namespace System::Security::Principal;
using namespace Extract::ErrorHandling;
using namespace Extract::Utilities;
using namespace msclr::interop;

void Extract::Test::ExceptionLoggerTest::LogTest()
{
	UCLIDException ue("ELITest", "TestException");

	ExceptionLogger logger;
	bool throwsException = false;
	auto unixStartTime = ExtractConvertExtensions::ToUnixTime(DateTime::Now);
	try
	{
		logger.Log(ue.asStringizedByteStream());
	}
	catch (...)
	{
		throwsException = true;
	}
	Assert::IsFalse(throwsException);

	TestSavedLine(GetDefaultFileExceptionFullPath(), unixStartTime);
}

void Extract::Test::ExceptionLoggerTest::LogWithFileName ()
{
	UCLIDException ue("ELITest", "TestException");
	TemporaryFile^ tempUEX = gcnew TemporaryFile(".uex", false);
	ExceptionLogger logger(marshal_as<string>(tempUEX->FileName));
	bool throwsException = false;
	auto unixStartTime = ExtractConvertExtensions::ToUnixTime(DateTime::Now);
	try
	{
		logger.Log(ue.asStringizedByteStream());
	}
	catch (...)
	{
		throwsException = true;
	}
	Assert::IsFalse(throwsException);

	TestSavedLine(tempUEX->FileName, unixStartTime);
}

void Extract::Test::ExceptionLoggerTest::LogWithFileNameEmpty()
{
	UCLIDException ue("ELITest", "TestException");
	TemporaryFile^ tempUEX = gcnew TemporaryFile(".uex", false);
	ExceptionLogger logger(string(""));
	bool throwsException = false;
	auto unixStartTime = ExtractConvertExtensions::ToUnixTime(DateTime::Now);
	try
	{
		logger.Log(ue.asStringizedByteStream());
	}
	catch (...)
	{
		throwsException = true;
	}
	Assert::IsFalse(throwsException);

	TestSavedLine(GetDefaultFileExceptionFullPath(), unixStartTime);
}

void Extract::Test::ExceptionLoggerTest::LogWithFileNameCharPtr()
{
	UCLIDException ue("ELITest", "TestException");
	TemporaryFile^ tempUEX = gcnew TemporaryFile(".uex", false);
	ExceptionLogger logger(marshal_as<string>(tempUEX->FileName).c_str());
	bool throwsException = false;
	auto unixStartTime = ExtractConvertExtensions::ToUnixTime(DateTime::Now);
	try
	{
		logger.Log(ue.asStringizedByteStream());
	}
	catch (...)
	{
		throwsException = true;
	}
	Assert::IsFalse(throwsException);
	
	TestSavedLine(tempUEX->FileName, unixStartTime);
}

void Extract::Test::ExceptionLoggerTest::LogWithFileNameCharPtrNull()
{
	UCLIDException ue("ELITest", "TestException");
	char* empty = nullptr;
	ExceptionLogger logger(empty);
	bool throwsException = false;
	auto unixStartTime = ExtractConvertExtensions::ToUnixTime(DateTime::Now);
	auto PID = Process::GetCurrentProcess()->Id;
	try
	{
		logger.Log(ue.asStringizedByteStream());
	}
	catch (...)
	{
		throwsException = true;
	}
	Assert::IsFalse(throwsException);

	// With null value the exception should be logged in the default location
	TestSavedLine(GetDefaultFileExceptionFullPath(), unixStartTime);
}

void Extract::Test::ExceptionLoggerTest::TestSavedLine(String^ fileName, long long unixStartTime)
{
	auto strSerial = String::Empty; // No longer used
	auto appName = AppDomain::CurrentDomain->FriendlyName;
	auto appVersion = Process::GetCurrentProcess()->MainModule->FileVersionInfo->ProductVersion;
	auto strApp = appName + " - " + appVersion;
	strApp = strApp->Replace(" ,", ",");
	strApp = strApp->Replace(',', '.');
	auto strComputer = Environment::MachineName;
	auto userParts = WindowsIdentity::GetCurrent()->Name->Split('\\');
	auto strUser = userParts[userParts->Length - 1];
	auto PID = Process::GetCurrentProcess()->Id;
	auto unixEndTime = ExtractConvertExtensions::ToUnixTime(DateTime::Now);

	// Load the exception from the log file
	auto lines = File::ReadAllLines(fileName);
	// Test last line
	auto tokens = lines[lines->Length - 1]->Split(',');
	Assert::AreEqual(7, tokens->Length);
	auto e = String::Empty;
	Assert::AreEqual(String::Empty, tokens[0]);
	Assert::AreEqual(strComputer, tokens[2]);
	Assert::AreEqual(strUser, tokens[3]);
	Int64 loggedTime = Int64::Parse(tokens[5]);
	Assert::IsTrue(loggedTime <= unixEndTime&& loggedTime >= unixStartTime, "Logged time should be within range");
	Assert::AreEqual(PID, Int32::Parse(tokens[4]));
	Assert::AreEqual(strApp, tokens[1]);

	UCLIDException newUE;
	auto stringized = tokens[6];
	newUE.createFromString("newELI", marshal_as<string>(stringized), false, false);
	Assert::AreEqual("ELITest", marshal_as<String^>(newUE.getTopELI()));
	Assert::AreEqual("TestException", marshal_as<String^>(newUE.getTopText()));
}

String^ Extract::Test::ExceptionLoggerTest::GetDefaultFileExceptionFullPath()
{
	return Path::Combine(Environment::GetFolderPath(Environment::SpecialFolder::CommonApplicationData)
		, R"(Extract Systems\LogFiles)"
		, "ExtractException.uex");
}
