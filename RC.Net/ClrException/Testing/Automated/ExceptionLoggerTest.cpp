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
using namespace Extract::ErrorHandling::Test;
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

void Extract::Test::ExceptionLoggerTest::LoadExtractExceptionInUclidException()
{
	auto ee = gcnew Extract::ErrorHandling::ExtractException("ELITest", "Test Message");
	ee->AddDebugData("TestStringData", "TestDataString", true);
	ee->AddDebugData("TestInt", 111, true);
	string stringized = marshal_as<string>( ee->AsStringizedByteStream());
	UCLIDException ue;
	try
	{
		ue.createFromString("ELITest2", stringized, false, false);
	}
	catch (...)
	{
		// should not have thrown
		Assert::Fail("Unable to load from ExtractException's stringized exception");
	}
	auto vecDebug = ue.getDebugVector();
	Assert::AreEqual(2, vecDebug.size());
	auto pair = vecDebug[0];
	Assert::AreEqual(marshal_as<String^>("TestStringData"), marshal_as<String^>(pair.GetName()));
	Assert::AreEqual(marshal_as<String^>("TestDataString"), marshal_as<String^>(UCLIDException::sGetDataValue(pair.GetPair().getStringValue())));

	pair = vecDebug[1];
	string look = UCLIDException::sGetDataValue(pair.GetPair().getStringValue());
	Assert::AreEqual(marshal_as<String^>("TestInt"), marshal_as<String^>(pair.GetName()));
	Assert::AreEqual(marshal_as<String^>("111"), marshal_as<String^>(UCLIDException::sGetDataValue(pair.GetPair().getStringValue())));
}

void Extract::Test::ExceptionLoggerTest::LoadUclidExecptionInExtractException()
{
	UCLIDException ue("ELITest", "Test Message");
	ue.addDebugInfo("TestStringData", "TestDataString", true);
	ue.addDebugInfo("TestInt", 111, true);

	Extract::ErrorHandling::ExtractException^ ee;
	auto stringized = marshal_as<String^>(ue.asStringizedByteStream());
	try
	{
		ee = Extract::ErrorHandling::ExtractException::LoadFromByteStream(stringized);
	}
	catch (...)
	{
		// should not have thrown
		Assert::Fail("Unable to load from UclidException's stringized exception");
	}
	Assert::AreEqual(2, ee->Data->Count);
	Assert::IsTrue(ee->Data->Contains("TestStringData"));
	auto decrypted = DebugDataHelper::GetValueAsType<String^>(ee->Data["TestStringData"]);
	Assert::AreEqual(marshal_as<String^>("TestDataString"), decrypted);

	Assert::IsTrue(ee->Data->Contains("TestInt"));
	decrypted = DebugDataHelper::GetValueAsType<String^>(ee->Data["TestInt"]);
	Assert::AreEqual(marshal_as<String^>("111"), decrypted);
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
